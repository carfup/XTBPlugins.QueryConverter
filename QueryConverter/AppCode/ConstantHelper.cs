using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carfup.XTBPlugins.AppCode
{
    class ConstantHelper
    {
        #region variables
        //Query Types
        public const string FetchXml = "FetchXml";
        public const string QueryExpression = "QueryExpression";
        public const string Linq = "Linq";
        public const string WebApi = "WebApi";

        public static Dictionary<string, string> operatorsMapping = new Dictionary<string, string>
        {
            { "Equal", "eq" },
            { "NotEqual", "ne"},
            { "GreaterThan", "gt"},
            { "GreaterEqual", "ge"},
            { "LessThan", "lt"},
            { "BeginsWith", "startswith"},
            { "EndsWith", "endswith"},
            { "Contains", "contains"},
        };
        #endregion
    }
}
