using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using RestSharp;

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
            QueryExpression queryToTransform = null;

            try
            {
                //Making sure sur QE is in "right" format : variable defined and ending with a ;
                //Making sure the String has a variable name to convert it into a QueryExpression
                if (input.ToLower().StartsWith("new queryexpression"))
                {
                    input = $"var {this.converterHelper.queryVariableName} = {input}";
                }

                if(!input.ToLower().EndsWith(";"))
                {
                    input = $"{input};";
                }


                // rework input to get all the query within the webapi
                input = input.Replace("\"", "\\\"");

                var client = new RestClient($"{CustomParameter.ROSLYNAPIURL}/convert");
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("undefined", $"\"{input}\"", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(QueryExpression));
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(response.Content));
                    queryToTransform = serializer.ReadObject(ms) as QueryExpression;
                    converterHelper.log.LogData(EventType.Event, LogAction.ConvertedWithRoslyn);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var exception = new Exception($"An error occured during the Text to object conversion.{Environment.NewLine} {response.Content}");
                    throw exception;
                }
                else
                {
                    throw new Exception("An error occured during the Text to object conversion.");
                }
            }
            catch (Exception ex)
            {
                converterHelper.log.LogData(EventType.Exception, LogAction.ConvertedWithRoslyn, ex);
                throw;
            }
            
            return queryToTransform;
        }

        #region Linq
        public string ProcessToLinq(QueryExpression query)
        {
            LoadEntityMetadata(query.EntityName);
            var entitySet = $"var {this.converterHelper.queryVariableName} = {this.converterHelper.serviceContextName}.{entityMetadata.SchemaName}Set";
            var conditions = ManageCriteriaLinq(query.Criteria);
            var columns = ManageColumsetToLinq(query.ColumnSet);
            var order = ManageOrdersToLinq(query.Orders.ToList());
            var topCount = ManageTopCountToLinq(query.TopCount);
            return entitySet + conditions + columns + order + topCount + ";";
        }

        public string ManageTopCountToLinq(int? topCount)
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
                var attributeDetails = entityMetadata.Attributes.Where(x => x.LogicalName == condition.AttributeName).FirstOrDefault();
                var schemaAttributeName = attributeDetails.SchemaName;

                if (new List<AttributeTypeCode>() { AttributeTypeCode.Picklist, AttributeTypeCode.Money}
                        .Contains(attributeDetails.AttributeType.Value))
                    schemaAttributeName += ".Value";
                if (attributeDetails.AttributeType == AttributeTypeCode.Lookup)
                    schemaAttributeName += ".Id.ToString()";

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

        public string ManageColumsetToLinq(ColumnSet columnSet)
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

        public string ManageOrdersToLinq(List<OrderExpression> ordersList)
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

                var attr = entityMetadata.Attributes.Where(xx => xx.LogicalName == order.AttributeName).Select(xx => xx.SchemaName).FirstOrDefault();
                orders += $".{prefix}(ord => ord.{attr})";
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

        public void LoadEntityMetadata(string entity)
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = entity
            };

            var attributesList = (RetrieveEntityResponse)this.converterHelper.service.Execute(request);
            if (attributesList != null)
                entityMetadata = attributesList.EntityMetadata;
            else entityMetadata = null;
        }
    }
}
