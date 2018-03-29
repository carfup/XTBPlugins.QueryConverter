using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carfup.XTBPlugins.AppCode.Converters;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Carfup.XTBPlugins.AppCode
{
    class ConverterHelper
    {
      
        public IOrganizationService service { get; set; } = null;
        public QueryExpressionTo queryExpressionTo = null;
        public WebApiTo webApiTo = null;
        public FetchXMLTo fetchXmlTo = null;
        public string inputType { get; set; } = null;
        public string outputType { get; set; } = null;
        public string inputQuery { get; set; } = null;

        public ConverterHelper(IOrganizationService service)
        {
            this.service = service;
            this.queryExpressionTo = new QueryExpressionTo(this);
            this.webApiTo = new WebApiTo(this);
            this.fetchXmlTo = new FetchXMLTo(this);
        }

        public string processQuery(string inputType, string outputType, string inputQuery)
        {
            this.inputQuery = inputQuery;
            this.inputType = inputType;
            this.outputType = outputType;

            string outputQuery = "";

            if(this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.FetchXml) // QueryExpression to FetchXML
            {
                outputQuery = this.queryExpressionTo.processToFetchXml(inputQuery);
            }
            else if (this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.WebApi) // QueryExpression to WebApi
            {
                outputQuery = this.queryExpressionTo.processToWebApi(inputQuery);
            }
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.QueryExpression) // FetchXML to QueryExpression
            {
                QueryExpression query = fromStringToQueryExpression(inputQuery);
                CodeBeautifier.input = this.fetchXmlTo.processToQueryExpression(query);
                var codeBeautifier = CodeBeautifier.doIt();

                outputQuery = codeBeautifier;
            }
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.WebApi) // FetchXML to WebApi
            {
                outputQuery = this.fetchXmlTo.processToWebApi(inputQuery);
            }

            return outputQuery;
        }

        public QueryExpression fromStringToQueryExpression(string input)
        {
            // Convert the FetchXML into a query expression.
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = input
            };

            var conversionResponse =
                (FetchXmlToQueryExpressionResponse)this.service.Execute(conversionRequest);

            // Use the newly converted query expression to make a retrieve multiple
            // request to Microsoft Dynamics CRM.
            QueryExpression queryExpression = conversionResponse.Query;
            return queryExpression;
        }
    }

    
}
