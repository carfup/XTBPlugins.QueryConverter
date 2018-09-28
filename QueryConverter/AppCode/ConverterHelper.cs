using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carfup.XTBPlugins.AppCode.Converters;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
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

        public string ProcessQuery(string inputType, string outputType, string inputQuery)
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
                outputQuery = this.queryExpressionTo.ProcessToWebApi(inputQuery);
            }
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.QueryExpression) // FetchXML to QueryExpression
            {
                QueryExpression query = this.fetchXmlTo.FromStringToQueryExpression(inputQuery);
                CodeBeautifier.input = this.fetchXmlTo.ProcessToQueryExpression(query);
                var codeBeautifier = CodeBeautifier.doIt();

                outputQuery = codeBeautifier;
            }
            else if (this.inputType == ConstantHelper.FetchXml && this.outputType == ConstantHelper.WebApi) // FetchXML to WebApi
            {
                outputQuery = this.fetchXmlTo.ProcessToWebApi(inputQuery);
            }
            else if (this.inputType == ConstantHelper.WebApi && this.outputType == ConstantHelper.QueryExpression) // WebApi to QueryExpression
            {
                //TODO
            }
            else if (this.inputType == ConstantHelper.WebApi && this.outputType == ConstantHelper.FetchXml) // WebApi to FetchXML
            {
                //TODO
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
    }

    
}
