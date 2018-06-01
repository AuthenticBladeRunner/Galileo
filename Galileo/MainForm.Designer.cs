namespace Galileo
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrs = new System.Windows.Forms.WebBrowser();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // webBrs
            // 
            this.webBrs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.webBrs.Location = new System.Drawing.Point(0, 0);
            this.webBrs.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrs.Name = "webBrs";
            this.webBrs.ScrollBarsEnabled = false;
            this.webBrs.Size = new System.Drawing.Size(996, 710);
            this.webBrs.TabIndex = 0;
            this.webBrs.Url = new System.Uri("http://test.alltobid.com/moni/gerenlogin.html#", System.UriKind.Absolute);
            this.webBrs.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrs_DocumentCompleted);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1022, 36);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 541);
            this.textBox1.TabIndex = 1;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 712);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.webBrs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmMain";
            this.Text = "Galileo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrs;
        private System.Windows.Forms.TextBox textBox1;
    }
}

