using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carfup.XTBPlugins.QueryConverter.AppCode
{
    public class QueryConverterMessageBusArgument
    {
        //----------------------------------//
        //  Thanks to Jonas for this part ! //
        //----------------------------------//

        /// <summary>Defines what is requested to be returned</summary>
        public QCMessageBusRequest qCRequest { get; set; }

        /// <summary>Constructor for the QueryConverterMessageBusArgument class</summary>
        /// <param name="Request">Requested type to return from FXB</param>
        public QueryConverterMessageBusArgument(QCMessageBusRequest qCRequest)
        {
            this.qCRequest = qCRequest;
        }

        public string TypeToReturn { get; set; }

        public QueryExpression queryExpression { get; set; }
        public string Linq { get; set; }
        public string WebApi { get; set; }
        public string FetchXml { get; set; }
    }

    public enum QCMessageBusRequest
    {
        FetchXML,
        QueryExpression,
        WebApi,
        Linq,
        None
    }
}
