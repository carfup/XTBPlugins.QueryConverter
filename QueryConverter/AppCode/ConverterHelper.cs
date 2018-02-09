using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Carfup.XTBPlugins.AppCode
{
    class ConverterHelper
    {
      
        public IOrganizationService service { get; set; } = null;
        public string inputType { get; set; } = null;
        public string outputType { get; set; } = null;
        public string inputQuery { get; set; } = null;

        public ConverterHelper(IOrganizationService service)
        {
            this.service = service;
        }

        public string processQuery(string inputType, string outputType, string inputQuery)
        {
            this.inputQuery = inputQuery;
            this.inputType = inputType;
            this.outputType = outputType;

            string outputQuery = "";

            if(this.inputType == ConstantHelper.QueryExpression && this.outputType == ConstantHelper.FetchXml) // QE TO FETCH
            {
                QueryExpression q = new QueryExpression();
               
                //var conversionRequest = new QueryExpressionToFetchXmlRequest
                //{
                //    Query = this.inputQuery
                //};
                //var conversionResponse =
                //    (QueryExpressionToFetchXmlResponse)_serviceProxy.Execute(conversionRequest);

                //// Use the converted query to make a retrieve multiple request to Microsoft Dynamics CRM.
                //String fetchXml = conversionResponse.FetchXml;
            }
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.QueryExpression) // FETCH TO QE
            {
                // Convert the FetchXML into a query expression.
                var conversionRequest = new FetchXmlToQueryExpressionRequest
                {
                    FetchXml = this.inputQuery
                };

                var conversionResponse = (FetchXmlToQueryExpressionResponse)this.service.Execute(conversionRequest);

                // Use the newly converted query expression to make a retrieve multiple
                // request to Microsoft Dynamics CRM.
                var queryExpression = conversionResponse.Query;
                outputQuery = $"var query = @\"{JsonConvert.SerializeObject((object)queryExpression)}\"";
                outputQuery += $"\nQueryExpression queryExpression = JsonConvert.DeserializeObject<QueryExpression>(query);";
            }



            return outputQuery;
        }
    }

    
}
