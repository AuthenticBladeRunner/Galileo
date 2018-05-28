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
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.btnGoto = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrs
            // 
            this.webBrs.Location = new System.Drawing.Point(1, 59);
            this.webBrs.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrs.Name = "webBrs";
            this.webBrs.Size = new System.Drawing.Size(996, 460);
            this.webBrs.TabIndex = 0;
            this.webBrs.Url = new System.Uri("https://www.reddit.com", System.UriKind.Absolute);
            // 
            // tbUrl
            // 
            this.tbUrl.Location = new System.Drawing.Point(21, 21);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(716, 21);
            this.tbUrl.TabIndex = 1;
            // 
            // btnGoto
            // 
            this.btnGoto.Location = new System.Drawing.Point(775, 20);
            this.btnGoto.Name = "btnGoto";
            this.btnGoto.Size = new System.Drawing.Size(92, 21);
            this.btnGoto.TabIndex = 2;
            this.btnGoto.Text = "Go to";
            this.btnGoto.UseVisualStyleBackColor = true;
            this.btnGoto.Click += new System.EventHandler(this.btnGoto_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1197, 520);
            this.Controls.Add(this.btnGoto);
            this.Controls.Add(this.tbUrl);
            this.Controls.Add(this.webBrs);
            this.Name = "frmMain";
            this.Text = "Galileo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrs;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Button btnGoto;
    }
}

