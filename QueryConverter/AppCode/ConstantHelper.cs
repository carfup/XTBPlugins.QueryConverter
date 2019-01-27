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

        //Value Rendering
        public const string Last7Days = "Last7Days";
        public const string Next7Days = "Next7Days";
        public const string NextXYears = "NextXYears";
        public const string LastXYears = "LastXYears";
        public const string NextXWeeks = "NextXWeeks";
        public const string LastXWeeks = "LastXWeeks";
        public const string NextXMonths = "NextXMonths";
        public const string LastXMonths = "LastXMonths";
        public const string NextYear = "NextYear";
        public const string LastYear = "LastYear";
        public const string NextWeek = "NextWeek";
        public const string LastWeek = "LastWeek";
        public const string NextMonth = "NextMonth";
        public const string LastMonth = "LastMonth";
        public const string NextXMinutes = "NextXMinutes";
        public const string LastXMinutes = "LastXMinutes";
        public const string NextXHours = "NextXHours";
        public const string LastXHours = "LastXHours";
        public const string NextXDays = "NextXDays";
        public const string LastXDays = "LastXDays";
        public const string ThisMonth = "ThisMonth";
        public const string ThisWeek = "ThisWeek";
        public const string ThisYear = "ThisYear";
        public const string Today = "Today";
        public const string Tomorrow = "Tomorrow";
        public const string Yesterday = "Yesterday";
        public const string OlderThanXYears = "OlderThanXYears";
        public const string OlderThanXWeeks = "OlderThanXWeeks";
        public const string OlderThanXMonths = "OlderThanXMonths";
        public const string OlderThanXMinutes = "OlderThanXMinutes";
        public const string OlderThanXHours = "OlderThanXHours";
        public const string OlderThanXDays = "OlderThanXDays";
        public const string Between = "Between";
        #endregion
    }
}
