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

namespace Carfup.XTBPlugins.QueryConverter
{
    public partial class QueryConverter : PluginControlBase, IGitHubPlugin
    {
        #region varibables
        ConverterHelper converter = null;

        public string RepositoryName
        {
            get
            {
                return "XTBPlugins.PersonalViewsMigration";
            }
        }

        public string UserName
        {
            get
            {
                return "carfup";
            }
        }

        #endregion
        public QueryConverter()
        {
            InitializeComponent();
        }

        private void toolStripButtonCloseTool_Click(object sender, System.EventArgs e)
        {
            CloseTool();
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            string inputType = comboBoxInput.Text;
            string outputType = comboBoxOutput.Text;
            string inputQuery = textBoxQueryInput.Text;

            if (converter == null)
                converter = new ConverterHelper(Service);

            string outputQuery = converter.ProcessQuery(inputType, outputType, inputQuery);
            textBoxQueryOutput.Text = outputQuery;
        }
    }
}
