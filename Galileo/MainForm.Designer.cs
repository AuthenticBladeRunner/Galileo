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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.webPanel = new System.Windows.Forms.Panel();
            this.webBrs = new System.Windows.Forms.WebBrowser();
            this.btnCheckPos = new System.Windows.Forms.Button();
            this.test = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.webPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1022, 129);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 54);
            this.textBox1.TabIndex = 1;
            // 
            // webPanel
            // 
            this.webPanel.Controls.Add(this.webBrs);
            this.webPanel.Location = new System.Drawing.Point(0, 0);
            this.webPanel.Name = "webPanel";
            this.webPanel.Size = new System.Drawing.Size(996, 710);
            this.webPanel.TabIndex = 2;
            // 
            // webBrs
            // 
            this.webBrs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrs.Location = new System.Drawing.Point(0, 0);
            this.webBrs.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrs.Name = "webBrs";
            this.webBrs.ScrollBarsEnabled = false;
            this.webBrs.Size = new System.Drawing.Size(996, 710);
            this.webBrs.TabIndex = 0;
            this.webBrs.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrs_DocumentCompleted);
            this.webBrs.NewWindow += new System.ComponentModel.CancelEventHandler(this.webBrs_NewWindow);
            this.webBrs.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.webBrs_PreviewKeyDown);
            // 
            // btnCheckPos
            // 
            this.btnCheckPos.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCheckPos.Location = new System.Drawing.Point(1053, 49);
            this.btnCheckPos.Name = "btnCheckPos";
            this.btnCheckPos.Size = new System.Drawing.Size(75, 23);
            this.btnCheckPos.TabIndex = 3;
            this.btnCheckPos.Text = "校对截图";
            this.btnCheckPos.UseVisualStyleBackColor = true;
            this.btnCheckPos.Click += new System.EventHandler(this.btnCheckPos_Click);
            // 
            // test
            // 
            this.test.Location = new System.Drawing.Point(1053, 90);
            this.test.Name = "test";
            this.test.Size = new System.Drawing.Size(75, 23);
            this.test.TabIndex = 4;
            this.test.Text = "test";
            this.test.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(1022, 224);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(232, 383);
            this.textBox2.TabIndex = 5;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 712);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.test);
            this.Controls.Add(this.btnCheckPos);
            this.Controls.Add(this.webPanel);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmMain";
            this.Text = "Galileo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.webPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel webPanel;
        private System.Windows.Forms.WebBrowser webBrs;
        private System.Windows.Forms.Button btnCheckPos;
        private System.Windows.Forms.Button test;
        private System.Windows.Forms.TextBox textBox2;
    }
}

