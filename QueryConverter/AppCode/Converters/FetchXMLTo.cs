using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Carfup.XTBPlugins.QueryConverter.AppCode.Converters
{
    public class FetchXMLTo
    {
        public string entityName = null;
        public ConverterHelper converterHelper = null;

        /// <summary>
        /// Constructor for the converter from FetchXML
        /// </summary>
        /// <param name="converterHelper">instance of the ConverterHelper</param>
        public FetchXMLTo(ConverterHelper converterHelper)
        {
            this.converterHelper = converterHelper;
        }

        /// <summary>
        /// Convertir a FetchXml query to QueryExpression (in string)
        /// </summary>
        /// <param name="queryExpression">QueryExpression to parse</param>
        /// <returns>Related QueryExpression in string format</returns>
        public string ProcessToQueryExpression(QueryExpression queryExpression)
        {
            entityName = queryExpression.EntityName;
            string stringq = $"QueryExpression query = new QueryExpression() {{ ";
            stringq += $"EntityName = \"{entityName}\",";
            stringq += $"Distinct = {queryExpression.Distinct.ToString().ToLower()}";

            if(queryExpression.TopCount != null)
                stringq += $", TopCount = {queryExpression.TopCount}";

            // Manage columnset
            stringq += ManageColumset(queryExpression.ColumnSet);

            // Criteria
            stringq += ManageCriteria(queryExpression.Criteria);

            // Linkentities
            stringq += ManageLinkEntities(queryExpression.LinkEntities);

            //Orders
            stringq += ManagerOrders(queryExpression.Orders);

            stringq += "};";

            return stringq;
        }

        /// <summary>
        /// Convertir a FetchXml query to QueryExpression (in string)
        /// </summary>
        /// <param name="input">FetchXml query to parse</param>
        /// <returns>Related WebApi link</returns>
        public string ProcessToWebApi(string input)
        {
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = input
            };

            var conversionResponse =
                (FetchXmlToQueryExpressionResponse)this.converterHelper.service.Execute(conversionRequest);

            // Use the newly converted query expression to make a retrieve multiple
            // request to Microsoft Dynamics CRM.
            QueryExpression queryExpression = conversionResponse.Query;

            var result = this.converterHelper.queryExpressionTo.ProcessToWebApi(this.converterHelper.fetchXmlTo.ProcessToQueryExpression(queryExpression));

            return result;
        }

        /// <summary>
        /// Transform a fetchxml query into QueryExpression query using the SDK
        /// </summary>
        /// <param name="input">the FetchXML query</param>
        /// <returns>the related QueryExpression Query</returns>
        public QueryExpression FromStringToQueryExpression(string input)
        {
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = input
            };

            var conversionResponse =
                (FetchXmlToQueryExpressionResponse)this.converterHelper.service.Execute(conversionRequest);

            // Use the newly converted query expression to make a retrieve multiple
            // request to Microsoft Dynamics CRM.
            QueryExpression queryExpression = conversionResponse.Query;

            return queryExpression;
        }

        /// <summary>
        /// Handles the conversion of the orders from fetchxml to QueryExpression
        /// </summary>
        /// <param name="ordersList">List of OrderExpression from the QueryExpression object</param>
        /// <returns>converted orders into string with QueryExpression syntax</returns>
        public string ManagerOrders(DataCollection<OrderExpression> ordersList)
        {
            var orders = "";

            if (ordersList.Count == 0)
                return orders;

            orders += $", Orders = {{";

            List<string> orderExpressions = new List<string>();
            foreach (var order in ordersList)
            {
                orderExpressions.Add($"new OrderExpression(\"{order.AttributeName}\", OrderType.{order.OrderType.ToString()})");
            }

            orders += String.Join($",", orderExpressions);

            orders += "} ";

            return orders;
        }

        /// <summary>
        /// Handles the conversion of the criteria from fetchxml to QueryExpression
        /// </summary>
        /// <param name="criteria">FilterExpression from the QueryExpression object</param>
        /// <param name="linkEntity">Is there any link entities</param>
        /// <returns>converted criteria into string with QueryExpression syntax</returns>
        public string ManageCriteria(FilterExpression criteria, bool linkEntity = false)
        {
            var conditions = "";

            if (criteria.Conditions.Count == 0 && criteria.Filters.Count == 0)
                return conditions;

            if (criteria.Conditions.Count > 0 || criteria.Filters.Count > 0)
            {
                // start criteria + conditions
                conditions += ((linkEntity) ? "LinkCriteria" : "Criteria") + "= {";

                // Managing filterExpression
                if (!linkEntity && criteria.Filters.Count > 0)
                {
                    conditions += $"Filters = {{";
                    List<string> filterExpressions = new List<string>();
                    foreach (var filter in criteria.Filters)
                    {
                        var filterExpressionString = $"new FilterExpression {{";
                        filterExpressionString += $"FilterOperator = LogicalOperator.{filter.FilterOperator.ToString()},";
                        filterExpressionString += ManageConditions(filter.Conditions);
                        filterExpressionString += "}";
                        filterExpressions.Add(filterExpressionString);
                    }
                    conditions += String.Join($",", filterExpressions);
                    conditions += $"}}";
                }
                else
                {
                    conditions += ManageConditions(criteria.Conditions);
                }

                // end criteria
                conditions += "}";
            }

            if (conditions != "")
                conditions = $", {conditions}";

            return conditions;
        }

        public string ManageConditions(DataCollection<ConditionExpression> conditions)
        {
            var conditionsString = "";
            conditionsString += $"Conditions = {{";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                var formatedCondition = this.converterHelper.ConditionHandling("queryexpression", "queryexpression",
                    condition.Operator.ToString(), condition.AttributeName, condition.Values.ToList());

                if(formatedCondition == null)
                continue;

                conditionExpressions.Add($"new ConditionExpression({formatedCondition})");
            }

            conditionsString += String.Join($",", conditionExpressions);

            conditionsString += "}";

            return conditionsString;
        }

        public string ManageColumset(ColumnSet columnSet, bool linkEntity = false)
        {
            var columns = "";

            if (columnSet.AllColumns)
                columns += "true";
            else if (columnSet.Columns.Count > 0)
            {
                var columnslist = columnSet.Columns;
                columns = String.Join(",", columnslist);
                columns = columns.Count() > 1 ? string.Join(",", columns.Split(',').Select(x => string.Format("\"{0}\"", x)).ToList()) : columns;
            }
            else
            {
                columns += "false";
            }

            var stringq = (linkEntity) ? "Columns" : "ColumnSet";
            stringq += $" = new ColumnSet({columns})";

            if (stringq != "")
                stringq = $", {stringq}";

            return stringq;
        }

        public string ManageLinkEntities(DataCollection<LinkEntity> linkEntities, string result = null, bool depth = false, string entityNameFrom = null)
        {
            if (linkEntities.Count == 0)
                return result;

            result += ",LinkEntities = {";

            List<string> linkEntitiesList = new List<string>();
            foreach (LinkEntity le in linkEntities)
            {
                var linkentityString = "new LinkEntity() {";
                linkentityString += $"LinkFromEntityName = \"{((entityNameFrom == null) ? entityName : entityNameFrom)}\",";
                linkentityString += $"LinkFromAttributeName = \"{le.LinkFromAttributeName}\",";
                linkentityString += $"LinkToEntityName = \"{le.LinkToEntityName}\", ";
                linkentityString += $"LinkToAttributeName = \"{le.LinkToAttributeName}\",";
                linkentityString += (le.EntityAlias != null) ? $"EntityAlias = {le.EntityAlias}," : "";
                linkentityString += $"JoinOperator = JoinOperator.{le.JoinOperator.ToString()}";
                linkentityString += $"{ManageColumset(le.Columns, true)}";
                linkentityString += $"{ManageCriteria(le.LinkCriteria, true)}";

                if (le.LinkEntities.Count > 0)
                    linkentityString = ManageLinkEntities(le.LinkEntities, linkentityString, true, le.LinkToEntityName);

                linkentityString += "}";

                linkEntitiesList.Add(linkentityString);
            }

            result += String.Join(",", linkEntitiesList);

            result += "}";

            return result;
        }
    }
}
