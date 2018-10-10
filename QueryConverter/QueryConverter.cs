using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Reflection;
using System.Web.UI.WebControls;
using System.IO;
using XrmToolBox.Extensibility.Interfaces;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Diagnostics;
using Microsoft.Xrm.Sdk.Messages;
using Carfup.XTBPlugins.AppCode;
using Microsoft.Crm.Sdk.Messages;
using AceWinforms;

namespace Carfup.XTBPlugins.QueryConverter
{
    public partial class QueryConverter : PluginControlBase, IGitHubPlugin
    {
        #region varibables
        ConverterHelper converter = null;
        internal PluginSettings settings = new PluginSettings();
        public LogUsage log = null;

        public string RepositoryName { get; } = "XTBPlugins.QueryConverter";

        public string UserName { get; } = "carfup";

        #endregion
        public QueryConverter()
        {
            InitializeComponent();
        }

        private void QueryConverter_Load(object sender, EventArgs e)
        {
            log = new LogUsage(this);
            log.LogData(EventType.Event, LogAction.PluginOpened);
            LoadSetting();

            inputCodeEditor.Theme = "twilight";
            inputCodeEditor.HighlighterMode = "csharp";
            inputCodeEditor.Load();

            outputCodeEditor.Theme = "twilight";
            outputCodeEditor.HighlighterMode = "csharp";
            outputCodeEditor.Load();
        }

        private void toolStripButtonCloseTool_Click(object sender, System.EventArgs e)
        {
            CloseTool();
        }

        private void buttonConvert_Click(object sender, EventArgs evt)
        {
            DetectQueryType(inputCodeEditor.Text);
            ExecuteMethod(ProcessToConversion);
        }

        private void DetectQueryType(string query)
        {
            try
            {
                var inputTypeQuery = "QueryExpression";
                if (query.ToLower().StartsWith("https://") || query.ToLower().StartsWith("http://")) // Webapi !
                    inputTypeQuery = "WebApi";
                else if (query.ToLower().StartsWith("<fetch"))
                    inputTypeQuery = "FetchXml";

                comboBoxInput.SelectedItem = inputTypeQuery;

                this.log.LogData(EventType.Event, LogAction.InputQueryTypeDetected);

            }
            catch (Exception e)
            {
                this.log.LogData(EventType.Exception, LogAction.InputQueryTypeDetected, e);
            }
            
        }

        private string GetCodeEditorHighlight(string type)
        {
            switch (type.ToLower())
            {
                case "queryexpression":
                    return "csharp";
                case "fetchxml":
                    return "xml";
                case "webapi":
                    return "curly";
                default:
                    return "csharp";
            }
        }

        private void comboBoxInput_TextChanged(object sender, EventArgs e)
        {
            var data = inputCodeEditor.Text;
            inputCodeEditor.HighlighterMode = GetCodeEditorHighlight(comboBoxInput.Text);
            inputCodeEditor.Load();
            inputCodeEditor.Text = data;
        }

        private void ProcessToConversion()
        {
            string inputType = comboBoxInput.Text;
            string outputType = comboBoxOutput.Text;
            string inputQuery = inputCodeEditor.Text;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Converting the query...",
                Work = (bw, e) =>
                {
                    if (converter == null)
                        converter = new ConverterHelper(Service);

                    e.Result = converter.ProcessQuery(inputType, outputType, inputQuery);
                },
                PostWorkCallBack = e =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.log.LogData(EventType.Exception, LogAction.QueryConverted, e.Error);
                        return;
                    }

                    outputCodeEditor.Text = e.Result.ToString();
                    this.log.LogData(EventType.Event, LogAction.QueryConverted);
                },
                ProgressChanged = e => { SetWorkingMessage(e.UserState.ToString()); }
            });
        }

        private void comboBoxOutput_TextChanged(object sender, EventArgs e)
        {
            var data = outputCodeEditor.Text;
            outputCodeEditor.HighlighterMode = GetCodeEditorHighlight(comboBoxOutput.Text);
            outputCodeEditor.Load();
            outputCodeEditor.Text = data;
        }

        private void comboBoxTheme_TextChanged(object sender, EventArgs evt)
        {
            var inputData = inputCodeEditor.Text;
            var outputData = outputCodeEditor.Text;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Modifying the theme...",
                Work = (bw, e) =>
                {
                    Invoke(new Action(() =>
                    {
                        inputCodeEditor.Theme = comboBoxTheme.SelectedItem.ToString();
                        inputCodeEditor.Load();

                        outputCodeEditor.Theme = comboBoxTheme.SelectedItem.ToString();
                        outputCodeEditor.Load();
                    }));
                },
                PostWorkCallBack = e =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.log.LogData(EventType.Exception, LogAction.ThemeModified, e.Error);
                        return;
                    }

                    inputCodeEditor.Text = inputData;
                    outputCodeEditor.Text = outputData;

                    this.log.LogData(EventType.Event, LogAction.ThemeModified);
                },
                ProgressChanged = e => { SetWorkingMessage(e.UserState.ToString()); }
            });
        }

        public void SaveSettings()
        {
            log.LogData(EventType.Event, LogAction.SettingsSaved);
            SettingsManager.Instance.Save(typeof(QueryConverter), settings);
        }

        private void LoadSetting()
        {
            try
            {
                if (SettingsManager.Instance.TryLoad<PluginSettings>(typeof(QueryConverter), out settings))
                {
                    return;
                }
                else
                    settings = new PluginSettings();

            }
            catch (InvalidOperationException ex)
            {
                log.LogData(EventType.Exception, LogAction.SettingLoaded, ex);
            }

            log.LogData(EventType.Event, LogAction.SettingLoaded);

            if (!settings.AllowLogUsage.HasValue)
            {
                log.PromptToLog();
                SaveSettings();
            }
        }

        public static string CurrentVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
            }
        }
    }
}
