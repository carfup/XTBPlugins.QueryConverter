using System;
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

namespace Carfup.XTBPlugins.AppCode.Converters
{
    class QueryExpressionTo
    {
        public ConverterHelper converterHelper = null;

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
                input = "var query = " + input;
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
            var entitySet = query.EntityName + "Set";
            var conditions = ManageCriteriaLinq(query.Criteria);
            var columns = ManageColumsetToLinq(query.ColumnSet);
            var order = ManageOrdersToLinq(query.Orders);
            return entitySet + conditions + columns + order;
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
                    conditions += String.Join($" {criteria.FilterOperator.ToString().ToLower()} ", filterExpressions);
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
                var formatedCondition = this.converterHelper.ConditionHandling("queryexpression", "linq",
                    condition.Operator.ToString(), condition.AttributeName, condition.Values.ToList());

                if (formatedCondition == null)
                    continue;

                conditionExpressions.Add($"{formatedCondition}");
            }

            conditionsString += String.Join($" {logicalOperator.ToString().ToLower()} ", conditionExpressions);
            conditionsString += ")";

            return conditionsString;
        }

        public string ManageColumsetToLinq(ColumnSet columnSet)
        {
            var columns = "";

            if (columnSet.AllColumns || columnSet.Columns.Count == 0)
                return columns;

            if (columnSet.Columns.Count > 0)
            {
                var columnslist = columnSet.Columns;
                columns = String.Join(",", columnslist);
                columns = columns.Count() > 1 ? string.Join(",", columns.Split(',').Select(x => string.Format("col.Attributes[\"{0}\"]", x)).ToList()) : columns;
            }

            var stringq = $"{Environment.NewLine}.Select(col => {columns})";

            return stringq;
        }

        public string ManageOrdersToLinq(DataCollection<OrderExpression> ordersList)
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
                var order = ManageOrdersToWebApi(queryExpression.Orders);
                var pageInfo = queryExpression.PageInfo;

                completeLink += $"{(columns != "" ? columns : "")}";
                completeLink += $"{(conditions != "" ? "&" + conditions : "")}";
                completeLink += $"{(order != "" ? "&" + order : "")}";
            }

            return completeLink;
        }

        public string ManageOrdersToWebApi(DataCollection<OrderExpression> orderExpressions)
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

        public JObject FormatConditionForMapper(ConditionExpression condition)
        {
            dynamic operatorMapping = new JObject();
            
            var values = (condition.Values.Count == 0) ? null : String.Join(",", condition.Values);
            if (values == "") // handling empty values
                values = "''";
            else if (values != null)
                values = values.Count() > 1 ? string.Join(",", values.Split(',').Select(x => $"{x}").ToList()) : values;

            operatorMapping[condition.Operator.ToString()] = values;
            

            return operatorMapping;
        }
    }
}
