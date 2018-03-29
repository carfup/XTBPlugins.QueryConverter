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
            var queryToTransform = fromStringToQueryExpression(input);
            var fetchXml = fromQueryExpressionToFetchXml(queryToTransform);
            XDocument doc = XDocument.Parse(fetchXml);

            return doc.ToString();
        }

        public string fromQueryExpressionToFetchXml(QueryExpression query)
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

        public QueryExpression fromStringToQueryExpression(string input)
        {
            List<object> ll = new List<object>();

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
                    ll.Add(variable.Value);
                }
                return s;

            }).Result;

            QueryExpression queryToTransform = null;

            foreach (var v in ll)
            {
                if (ll.FirstOrDefault().GetType() == typeof(QueryExpression))
                    queryToTransform = v as QueryExpression;
            }

            return queryToTransform;
        }

        public string processToWebApi(string input)
        {
            QueryExpressionTo queryExpressionTo = new QueryExpressionTo(this.convertHelper);

            var queryExpression = queryExpressionTo.fromStringToQueryExpression(input);

            var url = ((OrganizationServiceProxy)this.convertHelper.service).EndpointSwitch.PrimaryEndpoint.Host;
            var completeLink = $"https://{url}/api/v{getCrmVersion()}/{getEntityPlural(queryExpression.EntityName.ToLower())}?";

            // We go for the FetchXml WebApi
            if (queryExpression.LinkEntities.Count > 0)
            {
                var fetchXml = queryExpressionTo.fromQueryExpressionToFetchXml(queryExpression);

                // working the url encoded here
                fetchXml = WebUtility.UrlEncode(fetchXml);

                completeLink += $"fetchXml={fetchXml}";
            }
            else // we might go for the url
            {
                var conditions = manageConditions(queryExpression.Criteria);
                var columns = manageColumset(queryExpression.ColumnSet);
                var order = manageOrders(queryExpression.Orders);
                var pageInfo = queryExpression.PageInfo;

                completeLink += $"{(columns != "" ? columns : "")}";
                completeLink += $"{(conditions != "" ? "&" + conditions : "")}";
                completeLink += $"{(order != "" ? "&" + order : "")}";
            }

            return completeLink;
        }



        public string getCrmVersion()
        {
            RetrieveVersionRequest req = new RetrieveVersionRequest();
            RetrieveVersionResponse resp = (RetrieveVersionResponse)this.convertHelper.service.Execute(req);
            //assigns the version to a string
            var VersionNumber = resp.Version.Split('.');

            return $"{VersionNumber[0]}.{VersionNumber[1]}";
        }

        public string getEntityPlural(string entity)
        {
            var request = new RetrieveEntityRequest()
            {
                LogicalName = entity,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var result = ((RetrieveEntityResponse)this.convertHelper.service.Execute(request));

            return result.EntityMetadata.CollectionSchemaName.ToLower();
        }

        public string manageOrders(DataCollection<OrderExpression> orderExpressions)
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

        public string manageConditions(FilterExpression filterExpression)
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
                var values = String.Join(",", condition.Values);
                values = values.Count() > 1 ? string.Join(",", values.Split(',').Select(x => string.Format("{0}", x)).ToList()) : values;
                var operatorValue = ConstantHelper.operatorsMapping.Where(x => x.Key == condition.Operator.ToString()).First().Value;

                // Handling special cases :
                if (operatorValue == "contains" || operatorValue == "startswith" || operatorValue == "endswith")
                    conditionExpressions.Add($"{operatorValue}({condition.AttributeName}, '{values}')");
                else
                    conditionExpressions.Add($"{condition.AttributeName} {operatorValue} {values}");
            }


            conditionsString += String.Join($"{logicalOperator.ToString().ToLower()} ", conditionExpressions);

            return conditionsString;
        }

        public string manageColumset(ColumnSet columnSet, bool linkEntity = false)
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
