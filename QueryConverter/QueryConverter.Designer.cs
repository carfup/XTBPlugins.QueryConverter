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
            this.toolStripButtonCloseTool = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.outputCodeEditor = new AceWinforms.CodeEditor();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.inputCodeEditor = new AceWinforms.CodeEditor();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxTheme = new System.Windows.Forms.ComboBox();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.comboBoxOutput = new System.Windows.Forms.ComboBox();
            this.comboBoxInput = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCloseTool});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1042, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonCloseTool
            // 
            this.toolStripButtonCloseTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCloseTool.Image = global::Carfup.XTBPlugins.Properties.Resources.close;
            this.toolStripButtonCloseTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCloseTool.Name = "toolStripButtonCloseTool";
            this.toolStripButtonCloseTool.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonCloseTool.Text = "Close";
            this.toolStripButtonCloseTool.Click += new System.EventHandler(this.toolStripButtonCloseTool_Click);
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1026, 425);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.outputCodeEditor);
            this.groupBox4.Location = new System.Drawing.Point(616, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(407, 419);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Output";
            // 
            // outputCodeEditor
            // 
            this.outputCodeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputCodeEditor.HighlighterMode = "javascript";
            this.outputCodeEditor.Location = new System.Drawing.Point(6, 19);
            this.outputCodeEditor.MinIeVersion = "10";
            this.outputCodeEditor.MinimumSize = new System.Drawing.Size(20, 20);
            this.outputCodeEditor.Name = "outputCodeEditor";
            this.outputCodeEditor.Size = new System.Drawing.Size(395, 394);
            this.outputCodeEditor.TabIndex = 4;
            this.outputCodeEditor.Theme = "monokai";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.inputCodeEditor);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 419);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input";
            // 
            // inputCodeEditor
            // 
            this.inputCodeEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputCodeEditor.HighlighterMode = "javascript";
            this.inputCodeEditor.Location = new System.Drawing.Point(6, 19);
            this.inputCodeEditor.MinIeVersion = "10";
            this.inputCodeEditor.MinimumSize = new System.Drawing.Size(20, 20);
            this.inputCodeEditor.Name = "inputCodeEditor";
            this.inputCodeEditor.Size = new System.Drawing.Size(395, 394);
            this.inputCodeEditor.TabIndex = 5;
            this.inputCodeEditor.Theme = "monokai";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.comboBoxTheme);
            this.groupBox3.Controls.Add(this.buttonConvert);
            this.groupBox3.Controls.Add(this.comboBoxOutput);
            this.groupBox3.Controls.Add(this.comboBoxInput);
            this.groupBox3.Location = new System.Drawing.Point(416, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(194, 419);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Comparaison settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 352);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Theme";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "To";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "From";
            // 
            // comboBoxTheme
            // 
            this.comboBoxTheme.FormattingEnabled = true;
            this.comboBoxTheme.Items.AddRange(new object[] {
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
            this.comboBoxTheme.Location = new System.Drawing.Point(6, 368);
            this.comboBoxTheme.Name = "comboBoxTheme";
            this.comboBoxTheme.Size = new System.Drawing.Size(182, 21);
            this.comboBoxTheme.TabIndex = 3;
            this.comboBoxTheme.TextChanged += new System.EventHandler(this.comboBoxTheme_TextChanged);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(6, 97);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(182, 247);
            this.buttonConvert.TabIndex = 2;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Items.AddRange(new object[] {
            "FetchXml",
            "QueryExpression",
            "Linq",
            "WebApi"});
            this.comboBoxOutput.Location = new System.Drawing.Point(6, 70);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(182, 21);
            this.comboBoxOutput.TabIndex = 1;
            this.comboBoxOutput.TextChanged += new System.EventHandler(this.comboBoxOutput_TextChanged);
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Items.AddRange(new object[] {
            "FetchXml",
            "QueryExpression",
            "Linq",
            "WebApi"});
            this.comboBoxInput.Location = new System.Drawing.Point(6, 33);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(182, 21);
            this.comboBoxInput.TabIndex = 0;
            this.comboBoxInput.TextChanged += new System.EventHandler(this.comboBoxInput_TextChanged);
            // 
            // QueryConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "QueryConverter";
            this.Size = new System.Drawing.Size(1042, 492);
            this.Load += new System.EventHandler(this.QueryConverter_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonCloseTool;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxInput;
        private System.Windows.Forms.ComboBox comboBoxOutput;
        private System.Windows.Forms.Button buttonConvert;
        private AceWinforms.CodeEditor outputCodeEditor;
        private AceWinforms.CodeEditor inputCodeEditor;
        private System.Windows.Forms.ComboBox comboBoxTheme;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
