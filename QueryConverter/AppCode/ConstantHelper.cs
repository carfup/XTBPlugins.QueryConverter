using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;

namespace Carfup.XTBPlugins.QueryConverter.AppCode
{
    class ConstantHelper
    {
        #region variables
        //Query Types
        public const string FetchXml = "FetchXml";
        public const string QueryExpression = "QueryExpression";
        public const string Linq = "Linq";
        public const string WebApi = "WebApi";

        public static List<OperatorMapping> operatorMapping = new List<OperatorMapping>()
        {
            new OperatorMapping() {QeConditionOperator = ConditionOperator.Equal, FeOperator = "eq", QeOperator = "eq", WeOperator = "eq"},
        };

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
            { "NotNull", "ne"},
            { "NotIn", "not contains"},
        };
        #endregion
    }

    public class OperatorMapping
    {
        public ConditionOperator QeConditionOperator { get; set; }
        public string QeOperator { get; set; }
        public string FeOperator { get; set; }
        public string WeOperator { get; set; }
    }
}
