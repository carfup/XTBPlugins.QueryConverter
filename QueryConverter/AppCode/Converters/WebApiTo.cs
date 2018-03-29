using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Carfup.XTBPlugins.AppCode.Converters
{
    class WebApiTo
    {
        static string entityName = null;
        static string columns = null;
        static string filters = null;
        static string orders = null;
        ConverterHelper convertHelper = null;

        public WebApiTo(ConverterHelper convertHelper)
        {
            this.convertHelper = convertHelper;
        }

        public string processToFetch(string input)
        {
            var queryExpressionStringValue = processToQueryExpression(input);
            var fetch = this.convertHelper.queryExpressionTo.processToFetchXml(queryExpressionStringValue);

            return fetch;
        }
        public string processToQueryExpression(string input)
        {
            var urlParts = input.Split('/');
            var interestingUrlPart = urlParts[urlParts.Length - 1];

            if (interestingUrlPart == null)
                return "Error";

            for (int i = 0; i < interestingUrlPart.Split('$').Length; i++)
            {
                var detail = interestingUrlPart.Split('$')[i];
                var cleanedDetail = detail.Trim(new Char[] { '&', '?' });

                //Getting entityName 
                if (i == 0)
                    entityName = cleanedDetail;
                // handling the other cases
                else
                {
                    var dunno = cleanedDetail.Split('=');
                    var leftSide = dunno[0];

                    switch (leftSide)
                    {
                        case "select":
                            columns = manageColumns(dunno[1]);
                            break;
                        case "filter":
                            filters = manageFilters(dunno[1]);
                            break;
                        case "orderby":
                            orders = managerOrders(dunno[1]);
                            break;
                    }
                }
            }

            string stringq = $"QueryExpression query = new QueryExpression() {{ ";
            stringq += $"EntityName = \"{getEntityName(entityName)}\"";
            //stringq += $"Distinct = {query.Distinct.ToString().ToLower()}";

            // Manage columnset
            stringq += columns;
            // Criteria
            stringq += filters;

            //order
            stringq += orders;

            // Linkentities
            //stringq += manageLinkEntities(query.LinkEntities);

            stringq += "};";

            return stringq;
        }


        public string managerOrders(string ordersList)
        {

            var orders = "";
            var filtersCount = ordersList.Split(',').Count();

            if (filtersCount == 0)
                return orders;

            orders += $", Orders = {{";

            List<string> orderExpressions = new List<string>();
            foreach (var order in ordersList.Split(','))
            {
                var orderDetailed = order.Split(' ');
                var attributeName = orderDetailed[0];
                var orderType = orderDetailed[1];

                orderExpressions.Add($"new OrderExpression(\"{attributeName}\", OrderType.{(orderType == "asc" ? "Ascending" : "Descending")})");
            }

            orders += String.Join($",", orderExpressions);

            orders += "} ";

            return orders;
        }
        public static string manageColumns(string columnsList)
        {
            var columns = "";
            var columnsCount = columnsList.Split(',').Count();
            if (columnsCount == 0)
                columns = "true";

            if (columnsCount > 0)
                columns = columnsList.Split(',').Count() > 1 ? string.Join(",", columnsList.Split(',').Select(x => string.Format("\"{0}\"", x)).ToList()) : columns;

            var stringq = "ColumnSet";
            stringq += $" = new ColumnSet({columns})";

            if (stringq != "")
                stringq = $", {stringq}";

            return stringq;
        }

        public string manageFilters(string filtersList)
        {
            var conditionsString = "";
            var filtersCount = filtersList.Split(',').Count();

            if (filtersCount == 0)
                return conditionsString;

            conditionsString += $", Criteria = {{ Conditions = {{";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in filtersList.Split(new string[] { "or", "and" }, StringSplitOptions.None))
            {
                var conditionDetailed = condition.Split(' ');
                var attributeName = conditionDetailed[0];
                var operatorName = conditionDetailed[1];
                var valuesName = conditionDetailed[2];
                var values = valuesName.Count() > 1 ? string.Join(",", valuesName.Split(',').Select(x => string.Format("\"{0}\"", x)).ToList()) : valuesName;

                conditionExpressions.Add($"new ConditionExpression(\"{attributeName}\", ConditionOperator.{ConstantHelper.operatorsMapping.Where(x => x.Value == operatorName).Select(x => x.Key).FirstOrDefault()}, {values})");
            }

            conditionsString += String.Join($",", conditionExpressions);

            conditionsString += "} }";

            return conditionsString;
        }

        public string getEntityName(string entityName)
        {
            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            // Retrieve the MetaData.
            var response = this.convertHelper.service.Execute(request).Results;

            var entitiesList = ((EntityMetadata[])response.Values.FirstOrDefault()).ToList();

            var entity = entitiesList.Where(x => (x.CollectionSchemaName != null && x.CollectionSchemaName.ToLower() == entityName)).Select(x => x.SchemaName).FirstOrDefault().ToLower();

            return entity;
        }
    }
}
