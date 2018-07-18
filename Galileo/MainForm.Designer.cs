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
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.lbCptnAddr = new System.Windows.Forms.Label();
            this.lbCaptain = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.webPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1053, 267);
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
            this.btnCheckPos.Location = new System.Drawing.Point(1084, 238);
            this.btnCheckPos.Name = "btnCheckPos";
            this.btnCheckPos.Size = new System.Drawing.Size(75, 23);
            this.btnCheckPos.TabIndex = 3;
            this.btnCheckPos.Text = "截图";
            this.btnCheckPos.UseVisualStyleBackColor = true;
            this.btnCheckPos.Click += new System.EventHandler(this.btnCheckPos_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(1021, 327);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(232, 383);
            this.textBox2.TabIndex = 5;
            // 
            // lbCptnAddr
            // 
            this.lbCptnAddr.AutoSize = true;
            this.lbCptnAddr.Font = new System.Drawing.Font("黑体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCptnAddr.Location = new System.Drawing.Point(1012, 19);
            this.lbCptnAddr.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbCptnAddr.Name = "lbCptnAddr";
            this.lbCptnAddr.Size = new System.Drawing.Size(75, 14);
            this.lbCptnAddr.TabIndex = 7;
            this.lbCptnAddr.Text = "总控地址:";
            // 
            // lbCaptain
            // 
            this.lbCaptain.AutoSize = true;
            this.lbCaptain.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbCaptain.Location = new System.Drawing.Point(1102, 19);
            this.lbCaptain.Name = "lbCaptain";
            this.lbCaptain.Size = new System.Drawing.Size(0, 16);
            this.lbCaptain.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "拍手：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(60, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "标书号：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(60, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "label4";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(130, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "密码：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 79);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "身份证：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(178, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "label7";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(60, 79);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "label8";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(1015, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(257, 106);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "标书信息";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 749);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lbCaptain);
            this.Controls.Add(this.lbCptnAddr);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnCheckPos);
            this.Controls.Add(this.webPanel);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmMain";
            this.Text = "Galileo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.webPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel webPanel;
        private System.Windows.Forms.WebBrowser webBrs;
        private System.Windows.Forms.Button btnCheckPos;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label lbCptnAddr;
        private System.Windows.Forms.Label lbCaptain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

