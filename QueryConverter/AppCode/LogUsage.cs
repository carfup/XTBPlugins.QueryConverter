using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Carfup.XTBPlugins.QueryConverter.AppCode
{
    public class LogUsage
    {

        private TelemetryClient telemetry = null;
        private bool forceLog { get; set; } = false;
        private QueryConverter qc = null;

        /// <summary>
        /// LogUsage constructor class
        /// </summary>
        /// <param name="qc">QueryConverter class objact</param>
        public LogUsage(QueryConverter qc)
        {
            this.qc = qc;

            TelemetryConfiguration.Active.InstrumentationKey = CustomParameter.INSIGHTS_INTRUMENTATIONKEY;
            this.telemetry = new TelemetryClient();
            this.telemetry.Context.Component.Version = QueryConverter.CurrentVersion;
            this.telemetry.Context.Device.Id = this.qc.GetType().Name;
            this.telemetry.Context.User.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// For the logging if necessary
        /// </summary>
        public void updateForceLog()
        {
            this.forceLog = true;
        }

        /// <summary>
        /// Log data into App Insights
        /// </summary>
        /// <param name="type">type of event (event, exception, trace)</param>
        /// <param name="action">what was done</param>
        /// <param name="exception">exception stack if necessary</param>
        public void LogData(string type, string action, Exception exception = null)
        {
            if (this.qc.settings.AllowLogUsage == true || this.forceLog)
            {
                switch (type)
                {
                    case EventType.Event:
                        this.telemetry.TrackEvent(action, completeLog(action));
                        break;
                    case EventType.Dependency:
                        //this.telemetry.TrackDependency(todo);
                        break;
                    case EventType.Exception:
                        this.telemetry.TrackException(exception, completeLog(action));
                        break;
                    case EventType.Trace:
                        this.telemetry.TrackTrace(action, completeLog(action));
                        break;
                }
            }

            if (this.forceLog)
                this.forceLog = false;
        }

        /// <summary>
        /// Flush to send the telemetry to azure
        /// </summary>
        public void Flush()
        {
            this.telemetry.Flush();
        }

        /// <summary>
        /// Completion of log with some info from the Plugin and XrmToolBox app
        /// </summary>
        /// <param name="action">the action to log</param>
        /// <returns>the dictionary which will complete the initial log to send to azure</returns>
        public Dictionary<string, string> completeLog(string action = null)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                { "plugin", telemetry.Context.Device.Id },
                { "xtbversion", Assembly.GetEntryAssembly().GetName().Version.ToString() },
                { "pluginversion", QueryConverter.CurrentVersion }
            };

            if (action != null)
                dictionary.Add("action", action);

            return dictionary;
        }

        /// <summary>
        /// Prompt a form to let the user know that by default there is logging
        /// </summary>
        internal void PromptToLog()
        {
            var msg = "Anonymous statistics will be collected to improve plugin functionalities.\n\n" +
                      "You can change this setting in plugin's options anytime.\n\n" +
                      "Thanks!";

            this.qc.settings.AllowLogUsage = true;
            MessageBox.Show(msg);
        }
    }
}
