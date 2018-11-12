using System;
using System.Collections.Generic;
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

namespace Carfup.XTBPlugins.QueryConverter.AppCode
{
    public class ConverterHelper
    {
      
        public IOrganizationService service { get; set; } = null;
        public QueryExpressionTo queryExpressionTo = null;
        public WebApiTo webApiTo = null;
        public FetchXMLTo fetchXmlTo = null;
        public string inputType { get; set; } = null;
        public string outputType { get; set; } = null;
        public string inputQuery { get; set; } = null;
        public JObject operators = null;
        public string queryVariableName = "query";
        public string serviceContextName = "ServiceContext";

        public ConverterHelper(IOrganizationService service)
        {
            this.service = service;
            this.queryExpressionTo = new QueryExpressionTo(this);
            this.webApiTo = new WebApiTo(this);
            this.fetchXmlTo = new FetchXMLTo(this);
            this.operators = LoadOperatorsMapping();
        }

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

            return outputQuery;
        }

         
        public string GetCrmVersion()
        {
            RetrieveVersionRequest req = new RetrieveVersionRequest();
            RetrieveVersionResponse resp = (RetrieveVersionResponse)this.service.Execute(req);
            //assigns the version to a string
            var versionNumber = resp.Version.Split('.');

            return $"{versionNumber[0]}.{versionNumber[1]}";
        }

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
            if (valuesList.Count > 1)
            {
                if (operatorToken.SelectToken("valuerendering") != null)
                {

                }
                else
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
            }
            else
            {
                var type = valuesList.FirstOrDefault().GetType();
                var typeJValue = type == typeof(JValue) ? ((JValue)valuesList.FirstOrDefault())?.Type : null;
                // If we have a int, no need of the double quotes
                valueResult = (type == typeof(int) || typeJValue == JTokenType.Integer) ? valuesList.FirstOrDefault()?.ToString() : $"\"{valuesList.FirstOrDefault()}\"";

                if (toType == "webapi")
                    valueResult = valueResult.Replace("\"", "'");
            }

            transformedCondition = transformedCondition.Replace("{value}", valueResult);
            
            return transformedCondition;
        }
    }

    
}
