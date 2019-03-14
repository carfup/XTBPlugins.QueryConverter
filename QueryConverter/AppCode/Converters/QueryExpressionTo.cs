using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private List<string> linkEntityToLinqEntityFirstLetters = new List<string>();
        private List<string> ToLinqColumnsToKeepForLinkEntities = null;
        private List<string> ToLinqConditionsToKeepForLinkEntities = null;
        private string multipleConditions = null;

        /// <summary>
        /// QueryExpressionTo class constructor
        /// </summary>
        /// <param name="convertHelper"></param>
        public QueryExpressionTo(ConverterHelper convertHelper)
        {
            this.converterHelper = convertHelper;
        }

        /// <summary>
        /// Convert a QueryExpression in string format to FetchXml
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ProcessToFetchXml(string input)
        {
            var queryToTransform = this.FromStringToQueryExpression(input);
            var fetchXml = FromQueryExpressionToFetchXml(queryToTransform);
            XDocument doc = XDocument.Parse(fetchXml);

            return doc.ToString();
        }

        /// <summary>
        /// Convert a QueryExpression to FetchXml
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Convert a string into a QueryExpression
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Process the conversion from QE to LinQ
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string ProcessToLinq(QueryExpression query)
        {
            linkEntityToLinqEntityFirstLetters = new List<string>();
            ToLinqColumnsToKeepForLinkEntities = new List<string>();
            ToLinqConditionsToKeepForLinkEntities = new List<string>();

            var linkentities = ManageLinkEntitiesLinq(query.LinkEntities);
            LoadEntityMetadata(query.EntityName);
            var variableName = $"var {this.converterHelper.queryVariableName} = ";
            var entitySet = ManageEntitySetToLinq();
            var conditions = ManageCriteriaLinq(query.Criteria);
            var columns = ManageColumsetToLinq(query.ColumnSet);
            var order = ManageOrdersToLinq(query.Orders.ToList());
            var topCount = ManageTopCountToLinq(query.TopCount);
            var distinct = query.Distinct ? ".Distinct()": null;
            
            return variableName + ((topCount != null || distinct != null) ? "(" : "") + entitySet + linkentities + conditions + columns + order + ((topCount != null || distinct != null) ? ")" : "") + topCount + distinct + ";";
        }

        /// <summary>
        /// render the first part of the query for linq syntax (method base or not)
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public string ManageEntitySetToLinq(string syntax = LinqSyntax.SQLSynxtax)
        {
            var entitySet = "";

            var entityName = entityMetadata.SchemaName;

            if (syntax == LinqSyntax.SQLSynxtax)
                entitySet += $"from {entityName} in ";
            //entitySet += $"from {ManageUsedFirstLettersToLinq(entityName)} in ";

            entitySet += $"{ this.converterHelper.serviceContextName}.{entityMetadata.SchemaName}Set.AsEnumerable()";

            return entitySet;
        }

        /// <summary>
        /// Next function to manage LinkEntities conversion ..
        /// </summary>
        public string ManageLinkEntitiesLinq(DataCollection<LinkEntity> linkEntities, string result = null, string syntax = LinqSyntax.SQLSynxtax)
        {
            if (linkEntities.Count == 0)
                return result;

            // LinkEntities, not implemented yet for MethodSyntax
            if (linkEntities.Count > 0 && syntax == LinqSyntax.MethodSynxtax)
            {
                MessageBox.Show(
                    $"Sorry the LinkEntities are not supported yet for conversion. {Environment.NewLine}You might have a partial result.",
                    "LinkEntities are not supported yet.", MessageBoxButton.OK, MessageBoxImage.Warning);

                return "";
            }

            List<string> linkEntitiesList = new List<string>();

            foreach (LinkEntity le in linkEntities)
            {
                // manage metadata for LinkToEntity
                LoadEntityMetadata(le.LinkFromEntityName);
                var linkFromAttrName = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == le.LinkFromAttributeName)?.SchemaName;

                // manage metadata for LinkToEntity
                LoadEntityMetadata(le.LinkToEntityName);
                var linkToAttrName = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == le.LinkToAttributeName)?.SchemaName;

                var entityAlias = le.EntityAlias == null ? null : $"{le.EntityAlias}";
                var linkentityString = $"{Environment.NewLine}join {le.LinkToEntityName}{(entityAlias == null ? "" : " as "+entityAlias)} in {le.LinkToEntityName}Set.AsEnumerable()";
                linkentityString += $" on {le.LinkFromEntityName}.{linkFromAttrName}.Value equals {le.LinkToEntityName}.{linkToAttrName}.Id";

                if (le.LinkCriteria.Conditions.Count > 0)
                {
                    ToLinqConditionsToKeepForLinkEntities.Add(ManageConditionsToLinq(le.LinkCriteria.Conditions, le.LinkCriteria.FilterOperator, entityAlias ?? le.LinkToEntityName));
                }

                foreach (var column in le.Columns.Columns)
                {
                    var columnSchemaName = entityMetadata.Attributes.Where(xx => xx.LogicalName == column)
                        .Select(xx => xx.SchemaName).FirstOrDefault();
                    ToLinqColumnsToKeepForLinkEntities.Add($"{entityAlias ?? le.LinkToEntityName}.{columnSchemaName}");
                }

                if (le.LinkEntities.Count > 0)
                    linkentityString = ManageLinkEntitiesLinq(le.LinkEntities, linkentityString);

                linkEntitiesList.Add(linkentityString);
            }

            result += String.Join(Environment.NewLine, linkEntitiesList);

            return result;
        }

        /// <summary>
        /// Handle TopCount attribute
        /// </summary>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public string ManageTopCountToLinq(int? topCount)
        {
            var result = "";
            if (topCount != null)
                result = $"{Environment.NewLine}.Take({topCount.Value})";

            return result;
        }

        /// <summary>
        /// Handle the criteria from QE to LinQ
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="linkEntity"></param>
        /// <returns></returns>
        public string ManageCriteriaLinq(FilterExpression criteria, string syntax = LinqSyntax.SQLSynxtax, bool linkEntity = false)
        {
            var conditions = "";

            if (criteria.Conditions.Count == 0 && criteria.Filters.Count == 0)
                return conditions;

            if (criteria.Conditions.Count > 0 || criteria.Filters.Count > 0)
            {
                // start criteria + conditions
                conditions += Environment.NewLine;
                conditions += syntax == LinqSyntax.SQLSynxtax ? $"where " : $".Where(w => ";

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
                    conditions += ManageConditionsToLinq(criteria.Conditions, criteria.FilterOperator, null, syntax);
                }

                if (ToLinqConditionsToKeepForLinkEntities.Count > 0)
                    conditions += " and " + String.Join(" ", ToLinqConditionsToKeepForLinkEntities);

                // end criteria
                if(syntax == LinqSyntax.MethodSynxtax)
                    conditions += ")";
            }

            return conditions;
        }

        /// <summary>
        /// Handle the conditions from QE to LinQ
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        public string ManageConditionsToLinq(DataCollection<ConditionExpression> conditions, LogicalOperator logicalOperator, string prefix = null, string syntax = LinqSyntax.SQLSynxtax)
        {
            var conditionsString = "";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                var attributeDetails = entityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == condition.AttributeName);
                var schemaAttributeName = attributeDetails?.SchemaName;

                if (attributeDetails?.AttributeType != null && new List<AttributeTypeCode>() { AttributeTypeCode.Picklist, AttributeTypeCode.Money}
                        .Contains(attributeDetails.AttributeType.Value))
                    schemaAttributeName += ".Value";
                if (attributeDetails?.AttributeType == AttributeTypeCode.Lookup)
                    schemaAttributeName += ".Id.ToString()";

                var pref = prefix == null ? attributeDetails.EntityLogicalName : prefix;

                var formatedCondition = this.converterHelper.ConditionHandling("queryexpression", "linq",
                    condition.Operator.ToString(), schemaAttributeName, condition.Values.ToList(), pref);

                if (formatedCondition == null)
                    continue;

                conditionExpressions.Add($"{formatedCondition}");
            }

            var logicalOperatorForLinq = logicalOperator.ToString().ToLower() == "and" ? "&&" : "||";

            conditionsString += String.Join($" {logicalOperatorForLinq} ", conditionExpressions);

            return  "(" + conditionsString + ")";
        }

        /// <summary>
        /// Handle the columns from QE to LinQ
        /// </summary>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        public string ManageColumsetToLinq(ColumnSet columnSet, string syntax = LinqSyntax.SQLSynxtax)
        {
            var columns = "";

            if (columnSet.AllColumns || columnSet.Columns.Count == 0)
                return columns;

            if (columnSet.Columns.Count > 0)
            {
                var columnslist = columnSet.Columns;
                columns = String.Join(",", columnslist);
                if(syntax == LinqSyntax.SQLSynxtax)
                    columns = columns.Count() > 1 ? string.Join(", ", columnslist.Select(x => string.Format("{0}.{1}", entityMetadata.SchemaName, entityMetadata.Attributes.Where(xx => xx.LogicalName == x)
                        .Select(xx => xx.SchemaName).FirstOrDefault())).ToList()) : columns;
                else if (syntax == LinqSyntax.MethodSynxtax)
                    columns = columns.Count() > 1 ? string.Join(", ", columnslist.Select(x => string.Format("col.{0}", entityMetadata.Attributes.Where(xx => xx.LogicalName == x)
                        .Select(xx => xx.SchemaName).FirstOrDefault())).ToList()) : columns;
            }

            if (ToLinqColumnsToKeepForLinkEntities != null)
                columns += "," + String.Join(",", ToLinqColumnsToKeepForLinkEntities);


            var stringq = $"{Environment.NewLine}";
            stringq += syntax == LinqSyntax.SQLSynxtax ? $"select new {{{columns}}}" : $".Select(col => new {{{columns}}})";

            return stringq;
        }

        /// <summary>
        /// Handle the Orders from QE to LinQ
        /// </summary>
        /// <param name="ordersList"></param>
        /// <returns></returns>
        public string ManageOrdersToLinq(List<OrderExpression> ordersList, string syntax = LinqSyntax.SQLSynxtax)
        {
            if (ordersList.Count == 0)
                return "";

            var orders =  Environment.NewLine;
            orders += syntax == LinqSyntax.SQLSynxtax ? "order by " : "";

            List<string> attrList = new List<string>();

            for (var i = 0; i < ordersList.Count; i++)
            {
                var order = ordersList[i];
                var attrDetails = entityMetadata.Attributes.Where(xx => xx.LogicalName == order.AttributeName);
                var attr = attrDetails.Select(xx => xx.SchemaName).FirstOrDefault();

                if (syntax == LinqSyntax.MethodSynxtax)
                {
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

                    attr = $"{prefix}(ord => ord.{attr})";                   
                }

                attrList.Add(attrDetails.Select(x => x.EntityLogicalName).FirstOrDefault() + "."+ attr);
            }

            var separator = syntax == LinqSyntax.SQLSynxtax ? "," : ".";
            orders += String.Join(separator, attrList);

            return orders;
        }

        /// <summary>
        /// Making sure that each user entity in the query use an unique alias
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public string ManageUsedFirstLettersToLinq(string entityName)
        {
            var sizeOfEntityName = entityName.Length;
            string returnLetter = null;

            for (int i = 0; i < sizeOfEntityName; i++)
            {
                returnLetter = entityName.Substring(0, i);
                if (!linkEntityToLinqEntityFirstLetters.Contains(returnLetter))
                    return returnLetter;
            }
            
            return returnLetter;
        } 

        #endregion

        #region WebApi

        /// <summary>
        /// Process the conversion from QE to WebApi
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Check if that's the first parameter or not (? or &)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string exclaOrIntePoint(string link)
        {
            if (link.EndsWith("?"))
                return "";
            else
                return "&";
        }

        /// <summary>
        /// Manage TopCount from QE to WebApi
        /// </summary>
        /// <param name="topCount"></param>
        /// <returns></returns>
        public string ManageTopCountToWebApi(int? topCount)
        {
            return topCount == null ? "" : $"$top={topCount.Value}";
        }

        /// <summary>
        /// Manager Orders from QE to WebApi
        /// </summary>
        /// <param name="orderExpressions"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Manage conditions from QE to WebApi
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="conditionsString"></param>
        /// <param name="depth"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Manage Columns from QE to WebApi
        /// </summary>
        /// <param name="columnSet"></param>
        /// <param name="linkEntity"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load Entity Metadata for post processing
        /// </summary>
        /// <param name="entity"></param>
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
