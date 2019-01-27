using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Carfup.XTBPlugins.QueryConverter.AppCode.Converters;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Extensions;

namespace Carfup.XTBPlugins.QueryConverter.AppCode
{
    public class ConverterHelper
    {
      
        public IOrganizationService service { get; set; } = null;
        public LogUsage log = null;
        public QueryExpressionTo queryExpressionTo = null;
        public WebApiTo webApiTo = null;
        public FetchXMLTo fetchXmlTo = null;
        public QueryExpression queryExpressionObject = null;
        public string inputType { get; set; } = null;
        public string outputType { get; set; } = null;
        public string inputQuery { get; set; } = null;
        public JObject operators = null;
        public string queryVariableName = "query";
        public string serviceContextName = "ServiceContext";

        /// <summary>
        /// Constructor of the ConverterHelper class which is then used in all converters class
        /// </summary>
        /// <param name="service">IOrganizationService from the CRM SDK</param>
        /// <param name="log">Log class</param>
        public ConverterHelper(IOrganizationService service, LogUsage log = null)
        {
            this.service = service;
            this.log = log;
            this.queryExpressionTo = new QueryExpressionTo(this);
            this.webApiTo = new WebApiTo(this);
            this.fetchXmlTo = new FetchXMLTo(this);
            this.operators = LoadOperatorsMapping();
        }

        /// <summary>
        /// Process the conversion from input type query to wanted output type query
        /// </summary>
        /// <param name="inputType">Input type query</param>
        /// <param name="outputType">Output type query</param>
        /// <param name="inputQuery">Query to be converted</param>
        /// <returns></returns>
        public string ProcessQuery(string inputType, string outputType, string inputQuery)
        {
            this.inputQuery = inputQuery;
            this.inputType = inputType;
            this.outputType = outputType;

            string outputQuery = "";

            // QueryExpression to FetchXML
            if (this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.FetchXml) 
            {
                outputQuery = this.queryExpressionTo.ProcessToFetchXml(inputQuery);
            }
            // QueryExpression to WebApi
            else if (this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.WebApi) 
            {
                outputQuery = this.queryExpressionTo.ProcessToWebApi(inputQuery);
            }
            // QueryExpression to Linq
            else if (this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.Linq) 
            {
                var queryExpression = this.queryExpressionTo.FromStringToQueryExpression(inputQuery);
                outputQuery = this.queryExpressionTo.ProcessToLinq(queryExpression);
            }
            // FetchXML to QueryExpression
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.QueryExpression) 
            {

                QueryExpression query = this.fetchXmlTo.FromStringToQueryExpression(inputQuery);
                CodeBeautifier.input = this.fetchXmlTo.ProcessToQueryExpression(query);
                var codeBeautifier = CodeBeautifier.doIt();

                outputQuery = codeBeautifier;
                queryExpressionObject = query;
            }
            // FetchXML to WebApi
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.WebApi) 
            {
                outputQuery = this.fetchXmlTo.ProcessToWebApi(inputQuery);
            }
            // FetchXML to Linq
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.Linq) 
            {
                QueryExpression query = this.fetchXmlTo.FromStringToQueryExpression(inputQuery);
                outputQuery = this.queryExpressionTo.ProcessToLinq(query);
            }
            // WebApi to QueryExpression
            else if (this.inputType == ConstantHelper.WebApi && this.outputType == ConstantHelper.QueryExpression) 
            {
                CodeBeautifier.input = this.webApiTo.ProcessToQueryExpression(inputQuery);
                var codeBeautifier = CodeBeautifier.doIt();
                outputQuery = codeBeautifier;

            }
            // WebApi to FetchXML
            else if (this.inputType == ConstantHelper.WebApi && this.outputType == ConstantHelper.FetchXml) 
            {
                outputQuery = this.webApiTo.ProcessToFetchXml(inputQuery);
            }
            // WebApi to Linq
            else if (this.inputType == ConstantHelper.WebApi && this.outputType == ConstantHelper.Linq) 
            {
                var queryExpressionString = this.webApiTo.ProcessToQueryExpression(inputQuery);
                var queryExpression = this.queryExpressionTo.FromStringToQueryExpression(queryExpressionString);
                outputQuery = this.queryExpressionTo.ProcessToLinq(queryExpression);
            }
            else if(this.inputType == this.outputType)
            {
                outputQuery = inputQuery;
            }

            return outputQuery;
        }

         
        /// <summary>
        /// Get the CRM version used in the webapi converter
        /// </summary>
        /// <returns>X.X</returns>
        public string GetCrmVersion()
        {
            RetrieveVersionRequest req = new RetrieveVersionRequest();
            RetrieveVersionResponse resp = (RetrieveVersionResponse)this.service.Execute(req);
            //assigns the version to a string
            var versionNumber = resp.Version.Split('.');

            return $"{versionNumber[0]}.{versionNumber[1]}";
        }

        /// <summary>
        /// For some usage we need to get the plural name of entities
        /// </summary>
        /// <param name="entity">Name of entity</param>
        /// <returns></returns>
        public string GetEntityPlural(string entity)
        {
            var request = new RetrieveEntityRequest()
            {
                LogicalName = entity,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var result = ((RetrieveEntityResponse)this.service.Execute(request));

            return result.EntityMetadata.CollectionSchemaName.ToLower();
        }

        /// <summary>
        /// Load the mapping of condition operator for conversion
        /// </summary>
        /// <returns>the list of all the mappings in JObject</returns>
        private JObject LoadOperatorsMapping()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Carfup.XTBPlugins.AppCode.Mappings.operators.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return JObject.Parse(result);
            }
        }

        /// <summary>
        /// Looking for an operator condition within the mapping
        /// </summary>
        /// <param name="fromQueryType">query type of the input</param>
        /// <param name="toQueryType">query type of the output</param>
        /// <param name="operatorToSearch">operator to look for</param>
        /// <param name="sampleValue">values linked to that operator from the input query</param>
        /// <returns>a string with the formatting of the output query type operator</returns>
        private JToken LookForOperator(string fromQueryType, string toQueryType, string operatorToSearch, object sampleValue)
        {
            try
            {
                var potentialOperators = operators["operators"].Where(x =>
                    x.SelectToken(fromQueryType)?.SelectToken("operator").Value<string>() == operatorToSearch);

                if (potentialOperators.Count() > 1)
                {
                    foreach (var potentialOperator in potentialOperators)
                    {
                        if (potentialOperator.SelectToken(fromQueryType)?.SelectToken("valuepattern") != null)
                        {
                            var pattern = new Regex(potentialOperator.SelectToken(fromQueryType).SelectToken("valuepattern").Value<string>());
                            if (pattern.Match(sampleValue.ToString()).Success)
                            {
                                return potentialOperator.SelectToken(toQueryType);
                            }
                        }
                    }

                    return null;
                }

                return potentialOperators?.FirstOrDefault()?.SelectToken(toQueryType);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Handling condition specifics from the operator
        /// </summary>
        /// <param name="fromType">input query type</param>
        /// <param name="toType">output query type</param>
        /// <param name="operatorToLookFor">operator to look for</param>
        /// <param name="attribute">crm attribute name</param>
        /// <param name="valuesList">value(s) to link with the operator</param>
        /// <returns>string formatted condition</returns>
        public string ConditionHandling(string fromType, string toType, string operatorToLookFor, string attribute, List<object> valuesList)
        {
            var operatorToken = LookForOperator(fromType, toType, operatorToLookFor, valuesList?.FirstOrDefault());
            
            var transformedCondition = operatorToken?.SelectToken("conditionpattern")?.ToString();

            // If condition pattern is missing, then we skip it for now
            if (transformedCondition == null)
                return null;

            transformedCondition = transformedCondition.Replace("{propName}", attribute);
            transformedCondition = transformedCondition.Replace("{operator}", operatorToken.SelectToken("operator").ToString());

            // If no values needed, return in this state
            if (!transformedCondition.Contains("{value}"))
                return transformedCondition;

            // Special case for operator which doesnt need a value
            var operatorCondition = operatorToken.SelectToken("operator").ToString();

            // Preparing the value side
            var valueResult = "";
            var valueResult2 = ""; // if necessary
            var valueResult3 = ""; // if necessary


            // NEED TO REWORK THAT PART => UGLY UGLY UGLY
            if (operatorToken.SelectToken("valuerendering") != null)
            {
                var tempValue = FixedValueIntegration(operatorToken.SelectToken("valuerendering").ToString(), valuesList.FirstOrDefault()?.ToString());
                var type = tempValue.GetType();

                if (type == typeof(Object[]))
                {
                    var objectArray = (Object[])tempValue;

                    if (objectArray.Length < 3)
                        valueResult = (objectArray[0] is int) ? objectArray[0].ToString() : $"\"{objectArray[0]}\"";
                    if (objectArray.Length == 2 || objectArray.Length < 3)
                        valueResult2 = (objectArray[1] is int) ? objectArray[1].ToString() : $"\"{objectArray[1]}\"";
                    if (objectArray.Length == 3)
                        valueResult3 = (objectArray[2] is int) ? objectArray[2].ToString() : $"\"{objectArray[2]}\"";
                    if (objectArray.Length > 3)
                        throw  new Exception("Oups unexpected ValueRendering data");
                }
                else
                {
                    // If we have a int, no need of the double quotes
                    valueResult = (type == typeof(int))
                        ? tempValue.ToString()
                        : $"\"{tempValue}\"";
                }
            }
            else if (valuesList.Count > 1)
            {
                // Assuming we want an array here
                var type = valuesList[0].GetType();
                var typeJValue = type == typeof(JValue) ? ((JValue) valuesList[0])?.Type : null;
                if (type == typeof(int) || typeJValue == JTokenType.Integer)
                {
                    valueResult = String.Join(",", valuesList.ToArray());
                }
                else if (type == typeof(string))
                {
                    valueResult = "\"" + String.Join("\",\"", valuesList.ToArray()) + "\"";
                }
            }
            else
            {

                var type = valuesList.FirstOrDefault().GetType();
                var typeJValue = type == typeof(JValue) ? ((JValue) valuesList.FirstOrDefault())?.Type : null;
                // If we have a int, no need of the double quotes
                valueResult = (type == typeof(int) || typeJValue == JTokenType.Integer)
                    ? valuesList.FirstOrDefault()?.ToString()
                    : $"\"{valuesList.FirstOrDefault()}\"";

                if (toType == "webapi")
                    valueResult = valueResult.Replace("\"", "'");

            }

            transformedCondition = transformedCondition.Replace("{value}", valueResult).Replace("{value2}", valueResult2).Replace("{value3}", valueResult3);
            
            return transformedCondition;
        }
        public object FixedValueIntegration(string valueType, string value)
        {
            var thisYear = DateTime.Now.Year;
            var thisMonth = DateTime.Now.Month;
            var thisDay = DateTime.Now.Day;
            var today = DateTime.Today;
            var thisWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            switch (valueType)
            {
                
                case ConstantHelper.Next7Days:
                    return new object[] { today, today.AddDays(7) };
                case ConstantHelper.Last7Days:
                    return new object[] { today.AddDays(-7), today };
                case ConstantHelper.NextMonth:
                    return new object[] { today, today.AddMonths(1) };
                case ConstantHelper.LastMonth:
                    return new object[] { new DateTime(thisYear, thisMonth - 1, 1), new DateTime(thisYear, thisMonth - 1, DateTime.DaysInMonth(thisYear, thisMonth)) };
                case ConstantHelper.NextXDays:
                    return new object[] { today, today.AddDays(Int32.Parse(value)) };
                case ConstantHelper.LastXDays:
                    return new object[] { today.AddDays(Int32.Parse(value)), today };
                case ConstantHelper.NextXHours:
                    return new object[] { today, today.AddHours(Int32.Parse(value)) };
                case ConstantHelper.LastXHours:
                    return new object[] { today.AddHours(-Int32.Parse(value)), today };
                case ConstantHelper.NextXMinutes:
                    return new object[] { today, today.AddMinutes(Int32.Parse(value)) };
                case ConstantHelper.LastXMinutes:
                    return new object[] { today.AddMinutes(-Int32.Parse(value)), today };
                case ConstantHelper.NextXMonths:
                    return new object[] { today, today.AddMonths(Int32.Parse(value)) };
                case ConstantHelper.LastXMonths:
                    return new object[] { today.AddMonths(-Int32.Parse(value)), today };
                case ConstantHelper.NextXYears:
                    return new object[] { today, today.AddYears(Int32.Parse(value)) };
                case ConstantHelper.LastXYears:
                    return new object[] { today.AddYears(-Int32.Parse(value)), today };
                case ConstantHelper.NextYear:
                    return new object[] { today, today.AddYears(1) };
                case ConstantHelper.LastYear:
                    return new object[] { today.AddYears(-1), today }; 
                case ConstantHelper.ThisMonth:
                    return new object[] {new DateTime(thisYear, thisMonth,1), new DateTime(thisYear, thisMonth, DateTime.DaysInMonth(thisYear,thisMonth)) };
                case ConstantHelper.ThisYear:
                    return new object[] { new DateTime(thisYear, 1, 1), new DateTime(thisYear, 12, 31) };
                case ConstantHelper.Today:
                    return today.ToShortDateString();
                case ConstantHelper.Tomorrow:
                    return today.AddDays(1).ToShortDateString();
                case ConstantHelper.Yesterday:
                    return today.AddDays(-1).ToShortDateString();
                case ConstantHelper.OlderThanXYears:
                    return today.AddYears(-Int32.Parse(value));
                case ConstantHelper.OlderThanXDays:
                    return today.AddDays(-Int32.Parse(value));
                case ConstantHelper.OlderThanXMonths:
                    return today.AddMonths(-Int32.Parse(value));
                case ConstantHelper.OlderThanXHours:
                    return today.AddHours(-Int32.Parse(value));
                case ConstantHelper.OlderThanXMinutes:
                    return today.AddMinutes(-Int32.Parse(value));
                case ConstantHelper.OlderThanXWeeks:
                    return today.AddDays(-Int32.Parse(value)*7);
               
            }
            return null;
        }
    }
}
