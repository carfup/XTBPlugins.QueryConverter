using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Carfup.XTBPlugins.AppCode.Converters
{
    class FetchXMLTo
    {
        public string entityName = null;
        public ConverterHelper converterHelper = null;

        public FetchXMLTo(ConverterHelper converterHelper)
        {
            this.converterHelper = converterHelper;
        }

        /// <summary>
        /// Convertir a FetchXml query to QueryExpression (in string)
        /// </summary>
        /// <param name="queryExpression">QueryExpression to parse</param>
        /// <returns>Related QueryExpression in string format</returns>
        public string processToQueryExpression(QueryExpression queryExpression)
        {
            entityName = queryExpression.EntityName;
            string stringq = $"QueryExpression query = new QueryExpression() {{ ";
            stringq += $"EntityName = \"{entityName}\",";
            stringq += $"Distinct = {queryExpression.Distinct.ToString().ToLower()}";

            // Manage columnset
            stringq += manageColumset(queryExpression.ColumnSet);

            // Criteria
            stringq += manageCriteria(queryExpression.Criteria);

            // Linkentities
            stringq += manageLinkEntities(queryExpression.LinkEntities);

            //Orders
            stringq += managerOrders(queryExpression.Orders);

            stringq += "};";

            return stringq;
        }

        /// <summary>
        /// Convertir a FetchXml query to QueryExpression (in string)
        /// </summary>
        /// <param name="input">FetchXml query to parse</param>
        /// <returns>Related WebApi link</returns>
        public string processToWebApi(string input)
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

            var result = this.converterHelper.queryExpressionTo.processToWebApi(this.converterHelper.fetchXmlTo.processToQueryExpression(queryExpression));

            return result;
        }

        public QueryExpression fromStringToQueryExpression(string input)
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
        public string managerOrders(DataCollection<OrderExpression> ordersList)
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


        public string manageCriteria(FilterExpression criteria, bool linkEntity = false)
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
                        filterExpressionString += manageConditions(filter.Conditions);
                        filterExpressionString += "}";
                        filterExpressions.Add(filterExpressionString);
                    }
                    conditions += String.Join($",", filterExpressions);
                    conditions += $"}}";
                }
                else
                {
                    conditions += manageConditions(criteria.Conditions);
                }

                // end criteria
                conditions += "}";
            }

            if (conditions != "")
                conditions = $", {conditions}";

            return conditions;
        }

        public string manageConditions(DataCollection<ConditionExpression> conditions)
        {
            var conditionsString = "";
            conditionsString += $"Conditions = {{";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                var values = String.Join(",", condition.Values);
                values = values.Count() > 1 ? string.Join(",", values.Split(',').Select(x => string.Format("\"{0}\"", x)).ToList()) : values;
                conditionExpressions.Add($"new ConditionExpression(\"{condition.AttributeName}\", ConditionOperator.{condition.Operator.ToString()}, {values})");
            }

            conditionsString += String.Join($",", conditionExpressions);

            conditionsString += "}";

            return conditionsString;
        }

        public string manageColumset(ColumnSet columnSet, bool linkEntity = false)
        {
            var columns = "";

            if (columnSet.AllColumns)
                columns += "true";

            if (columnSet.Columns.Count > 0)
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

        public string manageLinkEntities(DataCollection<LinkEntity> linkEntities, string result = null, bool depth = false, string entityNameFrom = null)
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
                linkentityString += $"{manageColumset(le.Columns, true)}";
                linkentityString += $"{manageCriteria(le.LinkCriteria, true)}";

                if (le.LinkEntities.Count > 0)
                    linkentityString = manageLinkEntities(le.LinkEntities, linkentityString, true, le.LinkToEntityName);

                linkentityString += "}";

                linkEntitiesList.Add(linkentityString);
            }

            result += String.Join(",", linkEntitiesList);

            result += "}";

            return result;
        }
    }
}
