using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Newtonsoft.Json.Linq;

namespace Carfup.XTBPlugins.QueryConverter.AppCode.Converters
{
    public class WebApiTo
    {
        static string entityName = null;
        static string columns = null;
        static string filters = null;
        static string orders = null;
        static string topCount = null;
        ConverterHelper converterHelper = null;

        public WebApiTo(ConverterHelper convertHelper)
        {
            this.converterHelper = convertHelper;
        }

        public string ProcessToFetchXml(string input)
        {
            var queryExpressionStringValue = ProcessToQueryExpression(input);
            var fetch = this.converterHelper.queryExpressionTo.ProcessToFetchXml(queryExpressionStringValue);

            return fetch;
        }
        public string ProcessToQueryExpression(string input)
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
                            columns = ManageColumns(dunno[1]);
                            break;
                        case "filter":
                            filters = ManageFilters(cleanedDetail);
                            break;
                        case "orderby":
                            orders = ManagerOrders(dunno[1]);
                            break;
                        case "top":
                            topCount = ManageTopCount(dunno[1]);
                            break;
                        case "apply":
                            orders = ManagerOrders(dunno[1]);
                            break;
                    }
                }
            }

            string stringq = $"QueryExpression {this.converterHelper.queryVariableName} = new QueryExpression() {{ ";
            stringq += $"EntityName = \"{GetEntityName(entityName)}\"";
            //stringq += $"Distinct = {query.Distinct.ToString().ToLower()}";

            // Manage columnset
            stringq += columns != null ? $", {columns}" : "";

            // Criteria
            stringq += filters != null ? $", {filters}" : ""; ;

            //order
            stringq += orders != null ? $", {orders}" : ""; ;

            //top
            stringq += topCount != null ? $", {topCount}" : "";

            // Linkentities
            //stringq += manageLinkEntities(query.LinkEntities);

            stringq += "};";

            return stringq;
        }

        public string ManageTopCount(string top)
        {
            var topCount = "";
            
            if (top != null && top != "")
                topCount = $"TopCount = {top}";

            return topCount;
        }

        public string ManagerOrders(string ordersList)
        {

            var orders = "";
            var filtersCount = ordersList.Split(',').Count();

            if (filtersCount == 0)
                return orders;

            orders += "Orders = { ";

            List<string> orderExpressions = new List<string>();
            foreach (var order in ordersList.Split(','))
            {
                var orderDetailed = order.Split(' ');
                var attributeName = orderDetailed[0];
                var orderType = orderDetailed[1];

                orderExpressions.Add($"new OrderExpression(\"{attributeName}\", OrderType.{(orderType == "asc" ? "Ascending" : "Descending")})");
            }

            orders += String.Join($",", orderExpressions);

            orders += "}";

            return orders;
        }
        public string ManageColumns(string columnsList)
        {
            var columns = "";
 
            var columnsCount = columnsList?.Split(',').Count() ?? 0;
            if (columnsCount == 0 || columnsList == null || columnsList == "" || columnsList == "*")
                columns = "true";
            else if (columnsCount > 0)
                columns = columnsCount > 1 ? string.Join(",", columnsList.Split(',').Select(x => $"\"{x.Trim()}\"").ToList()) : $"{columnsList}";

            var stringq = $"ColumnSet = new ColumnSet({columns})";

            return stringq;
        }

        public string ManageFilters(string filtersList)
        {
            filtersList = filtersList.Replace("filter=", "");
            var conditionsString = "";
            var filtersCount = filtersList.Split(',').Count();

            if (filtersCount == 0)
                return conditionsString;

            conditionsString += "Criteria = { Filters = { ";

            List<string> conditionExpressions = new List<string>();

            //checking if there are groups
            var groups = filtersList.Split(new string[] { ") and (" }, StringSplitOptions.None);

            for (int i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                // resetting the list
                conditionExpressions = new List<string>();

                conditionsString += i == 0
                    ? "new FilterExpression { Conditions = { "
                    : ", new FilterExpression { Conditions = { ";

                var groupType = "And";
                var subGroupAnd = group.Split(new string[] {" and "}, StringSplitOptions.None);
                var subGroupOr = group.Split(new string[] {" or "}, StringSplitOptions.None);

                if (subGroupOr.Length > 1 && subGroupAnd.Length == 1) // by default if no match return 1
                    groupType = "Or";

                foreach (var condition in group.Split(new string[] {" or ", " and "}, StringSplitOptions.None))
                {
                    var conditionToCheck = condition.TrimStart(' ','(').TrimEnd();

                    var simpleCondition =
                        new Regex(@"(\w+)\s(\w+)\s('?(\w+)?'?)"); // attr = g1, operator = g2, value = g3
                    var invertedCondition =
                        new Regex(@"(.+)\('?(\w+)'?,\s?('?.+'?)\)"); // operator = g1, attr = g2, value = g3
                    var complexCondition =
                        new Regex(
                            @"(.+)\(PropertyName='?(\w+)?'(,PropertyValues?=(\[?.+\]?))?\)"); // operator = g1, attr = g2, value = g3

                    string conditionattribute = null;
                    string conditionOperator = null;
                    List<object> conditionValues = null;

                    var matchResult = simpleCondition.Match(conditionToCheck);

                    if (simpleCondition.Match(conditionToCheck).Success)
                    {
                        conditionattribute = matchResult.Groups[1]?.Value;
                        conditionOperator = matchResult.Groups[2]?.Value;
                        conditionValues = new List<object>() {JToken.Parse(matchResult.Groups[3]?.Value)};

                        //Special case for notnull
                        if (matchResult.Groups[3]?.Value == "null" && conditionOperator == "ne")
                            conditionOperator = "ne null";
                    }
                    else if (invertedCondition.Match(conditionToCheck).Success)
                    {
                        matchResult = invertedCondition.Match(conditionToCheck);
                        conditionOperator = matchResult.Groups[1]?.Value;
                        conditionattribute = matchResult.Groups[2]?.Value;
                        conditionValues = new List<object>() {JToken.Parse(matchResult.Groups[3]?.Value.TrimEnd(')'))};
                    }
                    else if (complexCondition.Match(conditionToCheck).Success)
                    {
                        matchResult = complexCondition.Match(conditionToCheck);
                        conditionOperator = matchResult.Groups[1]?.Value;
                        conditionattribute = matchResult.Groups[2]?.Value;
                        var tempValue = matchResult.Groups[4]?.Value == "" ? "" : JToken.Parse(matchResult.Groups[4]?.Value.TrimEnd(')'));

                        // complex conditions can have one or multiple values
                        conditionValues = !tempValue.Any()
                            ? new List<object>() {tempValue}
                            : tempValue.Select(x => x.ToString()).ToList<object>();
                    }

                    var formatedCondition = this.converterHelper.ConditionHandling("webapi", "queryexpression",
                        conditionOperator, conditionattribute, conditionValues);

                    if (formatedCondition == null)
                        continue;

                    conditionExpressions.Add($"new ConditionExpression({formatedCondition})");
                }

                conditionsString += String.Join($",", conditionExpressions);
                conditionsString = $"{conditionsString} }}, FilterOperator = LogicalOperator.{groupType}}}";
            }

            conditionsString += "}}";

            return conditionsString;
        }

        public string GetEntityName(string entityName)
        {
            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            // Retrieve the MetaData.
            var response = this.converterHelper.service.Execute(request).Results;

            var entitiesList = ((EntityMetadata[])response.Values.FirstOrDefault()).ToList();

            var entity = entitiesList.Where(x => (x.CollectionSchemaName != null && x.CollectionSchemaName.ToLower() == entityName)).Select(x => x.SchemaName).FirstOrDefault().ToLower();

            return entity;
        }
    }
}
