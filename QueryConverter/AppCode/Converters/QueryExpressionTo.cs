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

namespace Carfup.XTBPlugins.AppCode.Converters
{
    class QueryExpressionTo
    {
        public ConverterHelper convertHelper = null;

        public QueryExpressionTo(ConverterHelper convertHelper)
        {
            this.convertHelper = convertHelper;
        }

        public string processToFetchXml(string input)
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
                (QueryExpressionToFetchXmlResponse)this.convertHelper.service.Execute(conversionRequest);

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

        public string ProcessToWebApi(string input)
        {
           // QueryExpressionTo queryExpressionTo = new QueryExpressionTo(this.convertHelper);

            var queryExpression = this.FromStringToQueryExpression(input);

            var url = ((OrganizationServiceProxy)this.convertHelper.service).EndpointSwitch.PrimaryEndpoint.Host;
            var completeLink = $"https://{url}/api/data/v{this.convertHelper.GetCrmVersion()}/{this.convertHelper.GetEntityPlural(queryExpression.EntityName.ToLower())}?";

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
                var conditions = ManageConditionsToWebApi(queryExpression.Criteria);
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

        public string ManageConditionsToWebApi(FilterExpression filterExpression)
        {
            var conditionsString = "";
            var logicalOperator = filterExpression.FilterOperator;
            var conditions = filterExpression.Conditions;

            if (conditions.Count == 0)
                return conditionsString;

            conditionsString = "$filter=";

            List<string> conditionExpressions = new List<string>();
            foreach (var condition in conditions)
            {
                //var operatorValue = ConstantHelper.operatorsMapping.FirstOrDefault(x => x.Key == condition.Operator.ToString()).Value;
                var operatorToken =
                    this.convertHelper.LookForOperator("queryexpression", condition.Operator.ToString());
                var operatorValue = operatorToken.SelectToken("webapi")?.ToString();

                // If operator is missing, then we skip it for now
                if (operatorValue == null)
                    continue;

                var values = (condition.Values.Count == 0 || operatorToken.Value<bool>("emptyValue")) ? null : String.Join(",", condition.Values);

                if(values == "") // handling empty values
                    values = "''";
                else if(values != null)
                    values = values.Count() > 1 ? string.Join(",", values.Split(',').Select(x => $"{x}").ToList()) : values;

                // Handling special cases :
                var specialOperators = new string[] {"contains", "startswith", "endswith", "not contains"};
                if (specialOperators.Contains(operatorValue))
                {
                    conditionExpressions.AddRange(values.Split(',').Select(value => $"{operatorValue}({condition.AttributeName}, '{value}')"));
                }
                else
                    conditionExpressions.Add($"{condition.AttributeName} {operatorValue} {values}");
            }


            conditionsString += String.Join($" {logicalOperator.ToString().ToLower()} ", conditionExpressions);

            return conditionsString;
        }

        public string ManageColumsetToWebApi(ColumnSet columnSet, bool linkEntity = false)
        {
            var columns = "";

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
    }
}
