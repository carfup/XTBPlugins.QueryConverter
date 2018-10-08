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

        public string RepositoryName { get; } = "XTBPlugins.QueryConverter";

        public string UserName { get; } = "carfup";

        #endregion
        public QueryConverter()
        {
            InitializeComponent();
        }

        private void QueryConverter_Load(object sender, EventArgs e)
        {
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
            ExecuteMethod(ProcessToConversion);
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
            string inputQuery = inputCodeEditor.Text; //textBoxQueryInput.Text;

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
                        return;
                    }

                    outputCodeEditor.Text = e.Result.ToString();
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
                        return;
                    }

                    inputCodeEditor.Text = inputData;
                    outputCodeEditor.Text = outputData;
                },
                ProgressChanged = e => { SetWorkingMessage(e.UserState.ToString()); }
            });
        }
    }
}
