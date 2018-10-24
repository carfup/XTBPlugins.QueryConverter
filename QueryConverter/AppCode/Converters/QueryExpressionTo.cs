﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;

namespace Carfup.XTBPlugins.QueryConverter.AppCode.Converters
{
    public class QueryExpressionTo
    {
        public ConverterHelper converterHelper = null;
        private EntityMetadata entityMetadata = null;

        public QueryExpressionTo(ConverterHelper convertHelper)
        {
            this.converterHelper = convertHelper;
        }

        public string ProcessToFetchXml(string input)
        {
            var queryToTransform = this.FromStringToQueryExpression(input);
            var fetchXml = FromQueryExpressionToFetchXml(queryToTransform);
            XDocument doc = XDocument.Parse(fetchXml);

            return doc.ToString();
        }

        public string FromQueryExpressionToFetchXml(QueryExpression query)
        {
            // Convert the FetchXML into a query expression.
            var conversionRequest = new QueryExpressionToFetchXmlRequest
            {
                Query = query
            };

            var conversionResponse =
                (QueryExpressionToFetchXmlResponse)this.converterHelper.service.Execute(conversionRequest);

            return conversionResponse.FetchXml;
        }

        public QueryExpression FromStringToQueryExpression(string input)
        {
            List<object> convertVariable = new List<object>();

            //Making sure the String has a variable name to convert it into a QueryExpression
            if (input.ToLower().StartsWith("new queryexpression"))
            {
                input = $"var {this.converterHelper.queryVariableName} = {input}";
            }

            var result = Task.Run<object>(async () =>
            {
                // CSharpScript.RunAsync can also be generic with typed ReturnValue
                var s = await CSharpScript.RunAsync(@"using Microsoft.Xrm.Sdk.Query;", ScriptOptions.Default.WithReferences(typeof(Microsoft.Xrm.Sdk.Query.QueryExpression).Assembly));

                // continuing with previous evaluation state
                s = await s.ContinueWithAsync(input);

                // inspecting defined variables
                Console.WriteLine("inspecting defined variables:");
                foreach (var variable in s.Variables)
                {
                    convertVariable.Add(variable.Value);
                }
                return s;

            }).Result;

            QueryExpression queryToTransform = null;

            if (convertVariable.FirstOrDefault().GetType() == typeof(QueryExpression))
                queryToTransform = convertVariable.FirstOrDefault() as QueryExpression;

            return queryToTransform;
        }

        #region Linq
        public string ProcessToLinq(QueryExpression query)
        {
            entityMetadata = LoadEntityMetadata(query.EntityName);
            var entitySet = $"var {this.converterHelper.queryVariableName} = {this.converterHelper.serviceContextName}.{entityMetadata.SchemaName}Set";
            var conditions = ManageCriteriaLinq(query.Criteria);
            var columns = ManageColumsetToLinq(query.ColumnSet);
            var order = ManageOrdersToLinq(query.Orders);
            var topCount = ManageTopCountToLinq(query.TopCount);
            return entitySet + conditions + columns + order + topCount + ";";
        }

        private string ManageTopCountToLinq(int? topCount)
        {
            var result = "";
            if (topCount != null)
                result = $"{Environment.NewLine}.Take({topCount.Value})";

            return result;
        }

        public string ManageCriteriaLinq(FilterExpression criteria, bool linkEntity = false)
        {
            var conditions = "";

            if (criteria.Conditions.Count == 0 && criteria.Filters.Count == 0)
                return conditions;

            if (criteria.Conditions.Count > 0 || criteria.Filters.Count > 0)
            {
                // start criteria + conditions
                conditions += $"{Environment.NewLine}.Where(w => ";

                // Managing filterExpression
                if (!linkEntity && criteria.Filters.Count > 0)
                {
                    List<string> filterExpressions = new List<string>();
                    foreach (var filter in criteria.Filters)
                    {
                        var filterExpressionString = ManageConditionsToLinq(filter.Conditions, filter.FilterOperator);
                        filterExpressions.Add(filterExpressionString);
                    }

                    var logicalOperatorForLinq = criteria.FilterOperator.ToString().ToLower() == "and" ? "&&" : "||";

                    conditions += String.Join($" {logicalOperatorForLinq} ", filterExpressions);
                }
                else
                {
                    conditions += ManageConditionsToLinq(criteria.Conditions, criteria.FilterOperator);
                }

                // end criteria
                conditions += ")";
            }

            return conditions;
        }

        public string ManageConditionsToLinq(DataCollection<ConditionExpression> conditions, LogicalOperator logicalOperator)
        {
            var conditionsString = "(";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                var schemaAttributeName = entityMetadata.Attributes.Where(x => x.LogicalName == condition.AttributeName)
                    .Select(x => x.SchemaName).FirstOrDefault();

                var formatedCondition = this.converterHelper.ConditionHandling("queryexpression", "linq",
                    condition.Operator.ToString(), schemaAttributeName, condition.Values.ToList());

                if (formatedCondition == null)
                    continue;

                conditionExpressions.Add($"{formatedCondition}");
            }

            var logicalOperatorForLinq = logicalOperator.ToString().ToLower() == "and" ? "&&" : "||";

            conditionsString += String.Join($" {logicalOperatorForLinq} ", conditionExpressions);
            conditionsString += ")";

            return conditionsString;
        }

        private string ManageColumsetToLinq(ColumnSet columnSet)
        {
            var columns = "";

            if (columnSet.AllColumns || columnSet.Columns.Count == 0)
                return columns;

            if (columnSet.Columns.Count > 0)
            {
                var columnslist = columnSet.Columns;
                columns = String.Join(",", columnslist);
                columns = columns.Count() > 1 ? string.Join(", ", columns.Split(',').Select(x => string.Format("col.{0}", entityMetadata.Attributes.Where(xx => xx.LogicalName == x)
                    .Select(xx => xx.SchemaName).FirstOrDefault())).ToList()) : columns;
            }

            var stringq = $"{Environment.NewLine}.Select(col => new {{{columns}}})";

            return stringq;
        }

        private string ManageOrdersToLinq(DataCollection<OrderExpression> ordersList)
        {
            var orders = "";

            if (ordersList.Count == 0)
                return orders;

            orders += Environment.NewLine;

            for (int i = 0; i < ordersList.Count; i++)
            {
                var order = ordersList[i];

                var prefix = "OrderBy";
                if (i == 0 && order.OrderType == OrderType.Descending)
                    prefix = "OrderByDescending";
                else if (i > 0)
                {
                    if (order.OrderType == OrderType.Ascending)
                        prefix = "ThenBy";
                    else if (order.OrderType == OrderType.Descending)
                        prefix = "ThenByDescending";
                }
                orders += $".{prefix}(ord => ord.Attributes[\"{order.AttributeName}\"])";
            }

            return orders;
        }
        #endregion

        #region WebApi
        public string ProcessToWebApi(string input)
        {
           // QueryExpressionTo queryExpressionTo = new QueryExpressionTo(this.convertHelper);

            var queryExpression = this.FromStringToQueryExpression(input);

            var url = ((OrganizationServiceProxy)this.converterHelper.service).EndpointSwitch.PrimaryEndpoint.Host;
            var completeLink = $"https://{url}/api/data/v{this.converterHelper.GetCrmVersion()}/{this.converterHelper.GetEntityPlural(queryExpression.EntityName.ToLower())}?";

            // We go for the FetchXml WebApi
            if (queryExpression.LinkEntities.Count > 0)
            {
                var fetchXml = this.FromQueryExpressionToFetchXml(queryExpression);

                // working the url encoded here
                fetchXml = WebUtility.UrlEncode(fetchXml);

                completeLink += $"fetchXml={fetchXml}";
            }
            else // we might go for the url
            {
                var conditions = ManageConditionsToWebApi(queryExpression.Criteria, "");
                var columns = ManageColumsetToWebApi(queryExpression.ColumnSet);
                var order = ManageOrdersToWebApi(queryExpression.Orders.ToList());
                var pageInfo = queryExpression.PageInfo;
                var topCount = ManageTopCountToWebApi(queryExpression.TopCount);

                completeLink += $"{(columns != "" ? exclaOrIntePoint(completeLink) + columns : "")}";
                completeLink += $"{(conditions != "" ? exclaOrIntePoint(completeLink) + conditions : "")}";
                completeLink += $"{(order != "" ? exclaOrIntePoint(completeLink) + order : "")}";
                completeLink += $"{(topCount != "" ? exclaOrIntePoint(completeLink) + topCount : "")}";
            }

            return completeLink;
        }

        private string exclaOrIntePoint(string link)
        {
            if (link.EndsWith("?"))
                return "";
            else
                return "&";
        }

        public string ManageTopCountToWebApi(int? topCount)
        {
            return topCount == null ? "" : $"$top={topCount.Value}";
        }

        public string ManageOrdersToWebApi(List<OrderExpression> orderExpressions)
        {
            var ordersString = "";

            if (orderExpressions.Count == 0)
                return ordersString;

            ordersString = "$orderby=";

            List<string> orderExp = new List<string>();
            foreach (var order in orderExpressions)
            {
                orderExp.Add($"{order.AttributeName} {((order.OrderType.ToString().ToLower() == "ascending") ? "asc" : "desc")}");
            }

            ordersString += String.Join($",", orderExp);

            return ordersString;
        }

        public string ManageConditionsToWebApi(FilterExpression filterExpression, string conditionsString, int depth = 0, int maxDepth = 0)
        {
            
            var logicalOperator = filterExpression.FilterOperator;
            var conditions = filterExpression.Conditions;

            if (conditions.Count == 0 && filterExpression.Filters.Count > 0 && depth <= maxDepth)
            {
                // if maxdepth is already defined, we dont update it.
                maxDepth = maxDepth == 0 ? filterExpression.Filters.Count - 1 : maxDepth;
                conditions = filterExpression.Filters[0].Conditions;
            }
            else if (conditions.Count == 0)
                return conditionsString;

            if(depth == 0)
                conditionsString = "$filter=";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                //var operatorValue = ConstantHelper.operatorsMapping.FirstOrDefault(x => x.Key == condition.Operator.ToString()).Value;
                var formatedCondition = this.converterHelper.ConditionHandling("queryexpression", "webapi",
                    condition.Operator.ToString(), condition.AttributeName, condition.Values.ToList());

                if (formatedCondition == null)
                    continue;

                conditionExpressions.Add(formatedCondition);
            }

            conditionsString += "(" + String.Join($" {logicalOperator.ToString().ToLower()} ", conditionExpressions) + ")";

            if(depth < maxDepth)
            {
                depth++;
                conditionsString += " and ";
                conditionsString = ManageConditionsToWebApi(filterExpression.Filters[depth], conditionsString, depth, maxDepth);
            }

            return conditionsString;
        }

        public string ManageColumsetToWebApi(ColumnSet columnSet, bool linkEntity = false)
        {
            var columns = "";

            // If all column or not selected, no need to go further
            if (columnSet.AllColumns || columnSet.Columns.Count == 0)
                return columns;

            if (columnSet.Columns.Count > 0)
            {
                var columnslist = columnSet.Columns;
                columns = String.Join(",", columnslist);
                columns = columns.Count() > 1 ? string.Join(",", columns.Split(',').Select(x => string.Format("{0}", x)).ToList()) : columns;
            }

            var stringq = $"$select={columns}";

            return stringq;
        }
        #endregion

        private EntityMetadata LoadEntityMetadata(string entity)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entity
            };

            var attributesList = (RetrieveEntityResponse)this.converterHelper.service.Execute(request);
            if (attributesList != null)
                return attributesList.EntityMetadata;
            else return null;
        }
    }
}
