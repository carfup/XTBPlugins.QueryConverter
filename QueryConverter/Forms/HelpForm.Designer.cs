namespace Carfup.XTBPlugins.QueryConverter.Forms
{
    partial class HelpForm
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
            this.buttonCloseHelp = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCloseHelp
            // 
            this.buttonCloseHelp.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCloseHelp.Location = new System.Drawing.Point(889, 526);
            this.buttonCloseHelp.Name = "buttonCloseHelp";
            this.buttonCloseHelp.Size = new System.Drawing.Size(75, 23);
            this.buttonCloseHelp.TabIndex = 2;
            this.buttonCloseHelp.Text = "Close";
            this.buttonCloseHelp.UseVisualStyleBackColor = true;
            this.buttonCloseHelp.Click += new System.EventHandler(this.buttonCloseHelp_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Carfup.XTBPlugins.Properties.Resources.helpscreenshot;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(952, 508);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 552);
            this.Controls.Add(this.buttonCloseHelp);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpForm";
            this.ShowIcon = false;
            this.Text = "Help";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonCloseHelp;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}