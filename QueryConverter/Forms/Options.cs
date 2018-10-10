using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Carfup.XTBPlugins.AppCode;

namespace Carfup.XTBPlugins.Forms
{
    public partial class Options : Form
    {
        private QueryConverter.QueryConverter qc;

        public Options(QueryConverter.QueryConverter qc)
        {
            InitializeComponent();
            this.qc = qc;
            PopulateSettings(this.qc.settings);
        }

        private void PopulateSettings(PluginSettings settings)
        {
            if (settings == null)
            {
                settings = new PluginSettings();
            }

            checkboxAllowStats.Checked = settings.AllowLogUsage != false;
            comboBoxFavTheme.SelectedItem = settings.FavoriteTheme;
        }

        internal PluginSettings GetSettings()
        {
            var settings = this.qc.settings;
            settings.AllowLogUsage = checkboxAllowStats.Checked;
            settings.CurrentVersion = QueryConverter.QueryConverter.CurrentVersion;
            settings.FavoriteTheme = comboBoxFavTheme.SelectedItem.ToString();

            return settings;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.qc.settings = GetSettings();
            this.qc.SaveSettings();
            this.Close();
        }

        private void comboBoxFavTheme_TextChanged(object sender, EventArgs e)
        {
            this.qc.settings = GetSettings();
            this.qc.UpdateThemeDisplayed();
        }
    }
}
