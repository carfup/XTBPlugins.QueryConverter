using System.Windows.Forms;
using XrmToolBox.Extensibility;
using System.Reflection;
using XrmToolBox.Extensibility.Interfaces;
using System;
using System.Diagnostics;
using Carfup.XTBPlugins.QueryConverter.AppCode;
using Carfup.XTBPlugins.Forms;
using Carfup.XTBPlugins.QueryConverter.Forms;

namespace Carfup.XTBPlugins.QueryConverter
{
    public partial class QueryConverter : PluginControlBase, IGitHubPlugin, IMessageBusHost
    {
        #region varibables
        ConverterHelper converter = null;
        internal PluginSettings settings = new PluginSettings();
        public LogUsage log = null;
        private MessageBusEventArgs callerArgs = null;


        public string RepositoryName { get; } = "XTBPlugins.QueryConverter";

        public string UserName { get; } = "carfup";

        #endregion
        public QueryConverter()
        {
            InitializeComponent();
        }

        public event EventHandler<MessageBusEventArgs> OnOutgoingMessage;


        public void OnIncomingMessage(MessageBusEventArgs message)
        {
            callerArgs = message;
            string query = null;
            if (message.TargetArgument != null)
            {
                if (message.TargetArgument is QueryConverterMessageBusArgument)
                {
                    var qcArg = (QueryConverterMessageBusArgument)message.TargetArgument;
                    comboBoxInput.SelectedItem = qcArg.qCRequest.ToString();

                    switch (qcArg.qCRequest)
                    {
                        case QCMessageBusRequest.FetchXML:
                            query = qcArg.FetchXml;
                            break;
                        case QCMessageBusRequest.Linq:
                            query = qcArg.Linq;
                            break;
                        case QCMessageBusRequest.QueryExpression:
                            
                        case QCMessageBusRequest.QueryExpressionString:
                            query = qcArg.queryExpressionString;
                            break;
                        case QCMessageBusRequest.WebApi:
                            query = qcArg.WebApi;
                            break;
                    }
                }
                else if (message.TargetArgument is string)
                {
                    query = (string)message.TargetArgument;
                    DetectQueryType(inputCodeEditor.Text, false);
                }

                inputCodeEditor.Text = query;
            }

            toolStripButtonReturnQuery.Visible = true;
            toolStripSeparator4.Visible = true;
        }

        private void QueryConverter_Load(object sender, EventArgs e)
        {
            log = new LogUsage(this);
            log.LogData(EventType.Event, LogAction.PluginOpened);
            LoadSetting();

            inputCodeEditor.HighlighterMode = "csharp";
            outputCodeEditor.HighlighterMode = "csharp";
            UpdateThemeDisplayed();
        }

        private void toolStripButtonCloseTool_Click(object sender, System.EventArgs e)
        {
            CloseTool();
        }

        private void buttonConvert_Click(object sender, EventArgs evt)
        {
            if (String.IsNullOrEmpty(inputCodeEditor.Text))
            {
                this.log.LogData(EventType.Event, LogAction.InputQueryEmpty);
                MessageBox.Show("The input box is empty. Fill it with your query !", "The input box is empty!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var proceed = DetectQueryType(inputCodeEditor.Text);

                if (proceed != null)
                    ExecuteMethod(ProcessToConversion);
            }
        }

        /// <summary>
        /// Detect which type of query is pushed to the input code editor field 
        /// </summary>
        /// <param name="query">query code</param>
        /// <param name="displayAlert">display popup for not recognize query</param>
        /// <returns></returns>
        private string DetectQueryType(string query, bool displayAlert = true)
        {
            try
            {
                string inputTypeQuery = null;
                if (query.ToLower().StartsWith("https://") || query.ToLower().StartsWith("http://")) // Webapi !
                    inputTypeQuery = "WebApi"; 
                else if (query.ToLower().StartsWith("<fetch"))
                    inputTypeQuery = "FetchXml";
                else if (query.Contains("new QueryExpression("))
                    inputTypeQuery = "QueryExpression";
                //else if (query.Contains(".Where(") || query.Contains(".Select"))
                //    inputTypeQuery = "Linq";

                if (String.IsNullOrEmpty(inputTypeQuery) && displayAlert)
                {
                    MessageBox.Show("We didn't recognize the query you are try to convert. Maybe we didn't manage that type yet or there is an issue somewhere ...", "Query Not supported yet for conversion !", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    this.log.LogData(EventType.Event, LogAction.InputQueryTypeNotFound);
                }
                else
                {
                    comboBoxInput.SelectedItem = inputTypeQuery;
                    this.log.LogData(EventType.Event, LogAction.InputQueryTypeDetected);
                }

                return inputTypeQuery;
            }
            catch (Exception e)
            {
                this.log.LogData(EventType.Exception, LogAction.InputQueryTypeDetected, e);
            }   
            return "";            
        }

        /// <summary>
        /// Return the type of query to be converted for the code editor rendering
        /// </summary>
        /// <param name="type">query type</param>
        /// <returns></returns>
        private string GetCodeEditorHighlight(string type)
        {
            switch (type.ToLower())
            {
                case "queryexpression":
                case "linq":
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

        /// <summary>
        /// Call the helpers to perform the actual conversion from the input into output query wanted
        /// </summary>
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
                        converter = new ConverterHelper(Service, log);

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

            ManageMandatoryFields(comboBoxOutput.Text);
        }

        /// <summary>
        /// Based on the query type, we display or not some fields
        /// </summary>
        /// <param name="type">Query Type</param>
        private void ManageMandatoryFields(string type)
        {
            switch (type.ToLower())
            {
                case "queryexpression":
                    groupBoxConversionDetails.Visible = true;
                    textBoxCrmContext.Visible = false;
                    labelCrmContext.Visible = false;
                    textBoxQueryVariable.Visible = true;
                    labelQueryVariable.Visible = true;
                    break;
                case "linq":
                    groupBoxConversionDetails.Visible = true;
                    textBoxQueryVariable.Visible = true;
                    labelQueryVariable.Visible = true;
                    textBoxCrmContext.Visible = true;
                    labelCrmContext.Visible = true;
                    break;
                default:
                    groupBoxConversionDetails.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Allow user to update the theme for the code editor rendering
        /// </summary>
        public void UpdateThemeDisplayed()
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
                        if(inputCodeEditor.Theme != this.settings.FavoriteTheme)
                        {
                            inputCodeEditor.Theme = this.settings.FavoriteTheme;
                            inputCodeEditor.Load();
                        }

                        if(outputCodeEditor.Theme != this.settings.FavoriteTheme)
                        {
                            outputCodeEditor.Theme = this.settings.FavoriteTheme;
                            outputCodeEditor.Load();
                        }
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

        /// <summary>
        /// Saving settings if user updates any
        /// </summary>
        public void SaveSettings()
        {
            log.LogData(EventType.Event, LogAction.SettingsSaved);
            SettingsManager.Instance.Save(typeof(QueryConverter), settings);
            UpdateThemeDisplayed();
        }

        /// <summary>
        /// Load user settings to prefill some information with his preferences
        /// </summary>
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

        /// <summary>
        /// Return the version of the plugin assembly
        /// </summary>
        public static string CurrentVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
            }
        }

        private void toolStripButtonOptions_Click(object sender, EventArgs e)
        {
            var allowLogUsage = settings.AllowLogUsage;
            var optionDlg = new Options(this);
            if (optionDlg.ShowDialog(this) == DialogResult.OK)
            {
                settings = optionDlg.GetSettings();
                if (allowLogUsage != settings.AllowLogUsage)
                {
                    if (settings.AllowLogUsage == true)
                    {
                        this.log.updateForceLog();
                        this.log.LogData(EventType.Event, LogAction.StatsAccepted);
                    }
                    else if (!settings.AllowLogUsage == true)
                    {
                        this.log.updateForceLog();
                        this.log.LogData(EventType.Event, LogAction.StatsDenied);
                    }
                }
            }
        }


        private void textBoxQueryVariable_TextChanged(object sender, EventArgs e)
        {
            this.converter.queryVariableName = textBoxQueryVariable.Text;
        }

        private void textBoxCrmContext_TextChanged(object sender, EventArgs e)
        {
            this.converter.serviceContextName = textBoxCrmContext.Text;
        }

        private void toolStripButtonOpenFXB_Click(object sender, EventArgs e)
        {
            // TEMP AND UGLY FIX, removing useraworderby="false" programmatically
            var fetchXml = inputCodeEditor.Text;

            var inputType = DetectQueryType(fetchXml, false);
            if (inputType == "FetchXml" && fetchXml != null)
            {
                fetchXml = fetchXml.Replace("useraworderby=\"false\"", "").Replace("useraworderby=\"true\"", "");

                var messageBusEventArgs = new MessageBusEventArgs("FetchXML Builder")
                {
                    SourcePlugin = "QueryConverter",
                    TargetArgument = fetchXml
                };
                OnOutgoingMessage(this, messageBusEventArgs);
            }
            else
            {
                var messageBusEventArgs = new MessageBusEventArgs("FetchXML Builder")
                {
                    SourcePlugin = "QueryConverter",
                    TargetArgument = ""
                };
                OnOutgoingMessage(this, messageBusEventArgs);

            }
        }

        private void toolStripButtonHelp_Click(object sender, EventArgs e)
        {
            var helpDlg = new HelpForm();
            helpDlg.ShowDialog(this);
        }

        // Thanks to Jonas Rapp for this piece of code !
        private void toolStripButtonReturnQuery_Click(object sender, EventArgs e)
        {
            if (callerArgs == null)
                return;

            this.log.LogData(EventType.Event, $"{LogAction.ReturnedTo}-{callerArgs.SourcePlugin}");
            var result = outputCodeEditor.Text;

            if (string.IsNullOrWhiteSpace(result))
                return;

            var message = new MessageBusEventArgs(callerArgs.SourcePlugin);
            if (callerArgs.TargetArgument is QueryConverterMessageBusArgument)
            {
                var qcArgs = (QueryConverterMessageBusArgument)callerArgs.TargetArgument;
                switch (qcArgs.qCRequest)
                {
                    case QCMessageBusRequest.FetchXML:
                        // STILL REALLY UGLY
                        result = result.Replace("useraworderby=\"false\"", "").Replace("useraworderby=\"true\"", "");
                        qcArgs.FetchXml = result;
                        break;

                    case QCMessageBusRequest.QueryExpressionString:
                        qcArgs.queryExpressionString = result;
                        break;

                    case QCMessageBusRequest.WebApi:
                        qcArgs.WebApi = result;
                        break;

                    case QCMessageBusRequest.Linq:
                        qcArgs.Linq = result;
                        break;
                }
                message.TargetArgument = qcArgs;
            }
            else
            {
                message.TargetArgument = result;
            }
            OnOutgoingMessage(this, message);

            toolStripButtonReturnQuery.Visible = false;
            toolStripSeparator4.Visible = false;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/carfup/XTBPlugins.QueryConverter/issues");
        }
    }
}
