using System.Windows.Forms;

namespace Carfup.XTBPlugins.AppCode
{
    public class PluginSettings
    {
        public bool? AllowLogUsage { get; set; }
        public string CurrentVersion { get; set; } = QueryConverter.QueryConverter.CurrentVersion;
        public string FavoriteTheme { get; set; } = "twilight";
    }

    // EventType to qualify which type of telemetry we send
    static class EventType
    {
        public const string Event = "event";
        public const string Trace = "trace";
        public const string Dependency = "dependency";
        public const string Exception = "exception";
    }

    public static class CustomParameter
    {
        public static string INSIGHTS_INTRUMENTATIONKEY = "INSIGHTS_INTRUMENTATIONKEY_TOREPLACE";
    }

    // EventType to qualify which action was performed by the plugin
    static class LogAction
    {
        public const string ThemeModified = "ThemeModified";
        public const string QueryConverted = "QueryConverted";
        public const string InputQueryTypeDetected = "InputQueryTypeDetected";
        public const string PluginOpened = "PluginOpened";
        public const string SettingLoaded = "SettingLoaded";
        public const string SettingsSaved = "SettingsSaved";
        public const string StatsAccepted = "StatsAccepted";
        public const string StatsDenied = "StatsDenied";
        public const string InputQueryTypeNotFound = "InputQueryTypeNotFound";
    }
}
