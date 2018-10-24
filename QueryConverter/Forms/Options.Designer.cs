namespace Carfup.XTBPlugins.Forms
{
    partial class Options
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBoxFavTheme = new System.Windows.Forms.ComboBox();
            this.bgStats = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkboxAllowStats = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.bgStats.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.AutoSize = true;
            this.groupBox3.Controls.Add(this.comboBoxFavTheme);
            this.groupBox3.Location = new System.Drawing.Point(8, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(181, 61);
            this.groupBox3.TabIndex = 23;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Favorite Theme";
            // 
            // comboBoxFavTheme
            // 
            this.comboBoxFavTheme.FormattingEnabled = true;
            this.comboBoxFavTheme.Items.AddRange(new object[] {
            "chrome",
            "clouds",
            "crimson_editor",
            "dawn",
            "dreamweaver",
            "eclipse",
            "github",
            "solarized_light",
            "textmate",
            "tomorrow",
            "xcode",
            "ambiance",
            "chaos",
            "clouds_midnight",
            "cobalt",
            "idle_fingers",
            "kr_theme",
            "merbivore",
            "merbivore_soft",
            "terminal",
            "tomorrow_night",
            "tomorrow_night_blue",
            "tomorrow_night_bright",
            "tomorrow_night_eighties",
            "twilight",
            "vibrant_ink"});
            this.comboBoxFavTheme.Location = new System.Drawing.Point(11, 21);
            this.comboBoxFavTheme.Name = "comboBoxFavTheme";
            this.comboBoxFavTheme.Size = new System.Drawing.Size(155, 21);
            this.comboBoxFavTheme.TabIndex = 0;
            this.comboBoxFavTheme.TextChanged += new System.EventHandler(this.comboBoxFavTheme_TextChanged);
            // 
            // bgStats
            // 
            this.bgStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bgStats.AutoSize = true;
            this.bgStats.Controls.Add(this.label1);
            this.bgStats.Controls.Add(this.checkboxAllowStats);
            this.bgStats.Location = new System.Drawing.Point(9, 77);
            this.bgStats.Name = "bgStats";
            this.bgStats.Size = new System.Drawing.Size(382, 130);
            this.bgStats.TabIndex = 20;
            this.bgStats.TabStop = false;
            this.bgStats.Text = "Statistics";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(369, 71);
            this.label1.TabIndex = 6;
            this.label1.Text = "This plugin collects ONLY anonymous usage statistics. \r\nNo information related yo" +
    "ur CRM / Organization will be retrieve. \r\n\r\nThis will help us to improve the mos" +
    "t used features !\r\n";
            // 
            // checkboxAllowStats
            // 
            this.checkboxAllowStats.AutoSize = true;
            this.checkboxAllowStats.Checked = true;
            this.checkboxAllowStats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxAllowStats.Location = new System.Drawing.Point(10, 94);
            this.checkboxAllowStats.Name = "checkboxAllowStats";
            this.checkboxAllowStats.Size = new System.Drawing.Size(94, 17);
            this.checkboxAllowStats.TabIndex = 5;
            this.checkboxAllowStats.Text = "Allow statistics";
            this.checkboxAllowStats.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(316, 213);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 22;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(235, 213);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 21;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // Options
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 246);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.bgStats);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.ShowIcon = false;
            this.Text = "Options";
            this.groupBox3.ResumeLayout(false);
            this.bgStats.ResumeLayout(false);
            this.bgStats.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxFavTheme;
        private System.Windows.Forms.GroupBox bgStats;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkboxAllowStats;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
    }
}