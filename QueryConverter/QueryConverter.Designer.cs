namespace Carfup.XTBPlugins.QueryConverter
{
    partial class QueryConverter
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.inputCodeEditor = new AceWinforms.CodeEditor();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBoxConversionDetails = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelQueryVariable = new System.Windows.Forms.Label();
            this.textBoxQueryVariable = new System.Windows.Forms.TextBox();
            this.labelCrmContext = new System.Windows.Forms.Label();
            this.textBoxCrmContext = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxOutput = new System.Windows.Forms.ComboBox();
            this.comboBoxInput = new System.Windows.Forms.ComboBox();
            this.outputCodeEditor = new AceWinforms.CodeEditor();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.toolStripButtonCloseTool = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpenFXB = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOptions = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxConversionDetails.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCloseTool,
            this.toolStripSeparator1,
            this.toolStripButtonOpenFXB,
            this.toolStripSeparator2,
            this.toolStripButtonOptions,
            this.toolStripSeparator3,
            this.toolStripButtonHelp});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1042, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1042, 510);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.outputCodeEditor);
            this.groupBox4.Location = new System.Drawing.Point(624, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(415, 504);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.inputCodeEditor);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(415, 504);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input";
            // 
            // inputCodeEditor
            // 
            this.inputCodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputCodeEditor.HighlighterMode = "javascript";
            this.inputCodeEditor.Location = new System.Drawing.Point(3, 16);
            this.inputCodeEditor.MinIeVersion = "10";
            this.inputCodeEditor.MinimumSize = new System.Drawing.Size(20, 20);
            this.inputCodeEditor.Name = "inputCodeEditor";
            this.inputCodeEditor.Size = new System.Drawing.Size(409, 485);
            this.inputCodeEditor.TabIndex = 5;
            this.inputCodeEditor.Theme = "monokai";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonConvert);
            this.groupBox3.Controls.Add(this.groupBoxConversionDetails);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.comboBoxOutput);
            this.groupBox3.Controls.Add(this.comboBoxInput);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(424, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(194, 504);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Settings";
            // 
            // groupBoxConversionDetails
            // 
            this.groupBoxConversionDetails.AutoSize = true;
            this.groupBoxConversionDetails.Controls.Add(this.flowLayoutPanel1);
            this.groupBoxConversionDetails.Location = new System.Drawing.Point(0, 130);
            this.groupBoxConversionDetails.Name = "groupBoxConversionDetails";
            this.groupBoxConversionDetails.Size = new System.Drawing.Size(194, 127);
            this.groupBoxConversionDetails.TabIndex = 6;
            this.groupBoxConversionDetails.TabStop = false;
            this.groupBoxConversionDetails.Text = "Conversion details :";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.labelQueryVariable);
            this.flowLayoutPanel1.Controls.Add(this.textBoxQueryVariable);
            this.flowLayoutPanel1.Controls.Add(this.labelCrmContext);
            this.flowLayoutPanel1.Controls.Add(this.textBoxCrmContext);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(182, 89);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // labelQueryVariable
            // 
            this.labelQueryVariable.AutoSize = true;
            this.labelQueryVariable.Location = new System.Drawing.Point(3, 0);
            this.labelQueryVariable.Name = "labelQueryVariable";
            this.labelQueryVariable.Size = new System.Drawing.Size(110, 13);
            this.labelQueryVariable.TabIndex = 31;
            this.labelQueryVariable.Text = "Query variable name :";
            // 
            // textBoxQueryVariable
            // 
            this.textBoxQueryVariable.Location = new System.Drawing.Point(3, 16);
            this.textBoxQueryVariable.Name = "textBoxQueryVariable";
            this.textBoxQueryVariable.Size = new System.Drawing.Size(154, 20);
            this.textBoxQueryVariable.TabIndex = 32;
            this.textBoxQueryVariable.Text = "query";
            // 
            // labelCrmContext
            // 
            this.labelCrmContext.AutoSize = true;
            this.labelCrmContext.Location = new System.Drawing.Point(3, 39);
            this.labelCrmContext.Name = "labelCrmContext";
            this.labelCrmContext.Size = new System.Drawing.Size(67, 13);
            this.labelCrmContext.TabIndex = 33;
            this.labelCrmContext.Text = "CrmContext :";
            // 
            // textBoxCrmContext
            // 
            this.textBoxCrmContext.Location = new System.Drawing.Point(3, 55);
            this.textBoxCrmContext.Name = "textBoxCrmContext";
            this.textBoxCrmContext.Size = new System.Drawing.Size(154, 20);
            this.textBoxCrmContext.TabIndex = 34;
            this.textBoxCrmContext.Text = "ServiceContext";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Convert into :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Convert from :";
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Items.AddRange(new object[] {
            "FetchXml",
            "QueryExpression",
            "WebApi",
            "Linq"});
            this.comboBoxOutput.Location = new System.Drawing.Point(6, 93);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(182, 21);
            this.comboBoxOutput.TabIndex = 1;
            this.comboBoxOutput.TextChanged += new System.EventHandler(this.comboBoxOutput_TextChanged);
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.Enabled = false;
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Items.AddRange(new object[] {
            "FetchXml",
            "QueryExpression",
            "Linq",
            "WebApi"});
            this.comboBoxInput.Location = new System.Drawing.Point(6, 43);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(182, 21);
            this.comboBoxInput.TabIndex = 0;
            this.comboBoxInput.TextChanged += new System.EventHandler(this.comboBoxInput_TextChanged);
            // 
            // outputCodeEditor
            // 
            this.outputCodeEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputCodeEditor.HighlighterMode = "javascript";
            this.outputCodeEditor.Location = new System.Drawing.Point(3, 16);
            this.outputCodeEditor.MinIeVersion = "10";
            this.outputCodeEditor.MinimumSize = new System.Drawing.Size(20, 20);
            this.outputCodeEditor.Name = "outputCodeEditor";
            this.outputCodeEditor.Size = new System.Drawing.Size(409, 485);
            this.outputCodeEditor.TabIndex = 4;
            this.outputCodeEditor.Theme = "monokai";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Image = global::Carfup.XTBPlugins.Properties.Resources.arrow_right_64;
            this.buttonConvert.Location = new System.Drawing.Point(15, 263);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(166, 90);
            this.buttonConvert.TabIndex = 7;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // toolStripButtonCloseTool
            // 
            this.toolStripButtonCloseTool.Image = global::Carfup.XTBPlugins.Properties.Resources.close;
            this.toolStripButtonCloseTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCloseTool.Name = "toolStripButtonCloseTool";
            this.toolStripButtonCloseTool.Size = new System.Drawing.Size(56, 22);
            this.toolStripButtonCloseTool.Text = "Close";
            this.toolStripButtonCloseTool.Click += new System.EventHandler(this.toolStripButtonCloseTool_Click);
            // 
            // toolStripButtonOpenFXB
            // 
            this.toolStripButtonOpenFXB.Image = global::Carfup.XTBPlugins.Properties.Resources.fxb;
            this.toolStripButtonOpenFXB.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpenFXB.Name = "toolStripButtonOpenFXB";
            this.toolStripButtonOpenFXB.Size = new System.Drawing.Size(139, 22);
            this.toolStripButtonOpenFXB.Text = "Use FetchXMLBuilder";
            this.toolStripButtonOpenFXB.ToolTipText = "Open or build in FetchXMLBuilder";
            this.toolStripButtonOpenFXB.Click += new System.EventHandler(this.toolStripButtonOpenFXB_Click);
            // 
            // toolStripButtonOptions
            // 
            this.toolStripButtonOptions.Image = global::Carfup.XTBPlugins.Properties.Resources.gear;
            this.toolStripButtonOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOptions.Name = "toolStripButtonOptions";
            this.toolStripButtonOptions.Size = new System.Drawing.Size(69, 22);
            this.toolStripButtonOptions.Text = "Options";
            this.toolStripButtonOptions.Click += new System.EventHandler(this.toolStripButtonOptions_Click);
            // 
            // toolStripButtonHelp
            // 
            this.toolStripButtonHelp.Image = global::Carfup.XTBPlugins.Properties.Resources.help;
            this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHelp.Name = "toolStripButtonHelp";
            this.toolStripButtonHelp.Size = new System.Drawing.Size(52, 22);
            this.toolStripButtonHelp.Text = "Help";
            this.toolStripButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelp_Click);
            // 
            // QueryConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "QueryConverter";
            this.Size = new System.Drawing.Size(1042, 541);
            this.Load += new System.EventHandler(this.QueryConverter_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxConversionDetails.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxInput;
        private System.Windows.Forms.ComboBox comboBoxOutput;
        private AceWinforms.CodeEditor inputCodeEditor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOptions;

        private System.Windows.Forms.GroupBox groupBoxConversionDetails;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenFXB;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label labelQueryVariable;
        private System.Windows.Forms.TextBox textBoxQueryVariable;
        private System.Windows.Forms.Label labelCrmContext;
        private System.Windows.Forms.TextBox textBoxCrmContext;
        private System.Windows.Forms.Button buttonConvert;
        private AceWinforms.CodeEditor outputCodeEditor;
        private System.Windows.Forms.ToolStripButton toolStripButtonCloseTool;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
    }
}
