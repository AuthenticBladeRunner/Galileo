using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Galileo
{
    public partial class login : Form
    {
        frmMain mainForm = new frmMain();

        public login()
        {
            InitializeComponent();
        }

        private void login_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("57.4".Split('.')[0]);
            mainForm.loginForm = this;
            mainForm.InitConn();
            this.tbUserName.Focus();
        }
        
        // 发送登录验证请求
        private void reqLogin()
        {
            bool res = mainForm.ReqLogin(tbUserName.Text);     // 发送用户登录请求
            if (!res)
                MessageBox.Show("找不到主控服务器！");
        }

        // 处理主控返回的登录验证结果
        public void LoginCallback(string loginResult)
        {
            if (loginResult == "0")
            {
                // The "invoke" call tells the form "Please execute this code in your thread rather than mine."
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Hide();
                    mainForm.loggedIn = true;
                    mainForm.Show();
                });
            }
            else
            {
                MessageBox.Show("用户名不存在！");
            }
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            reqLogin();
        }

        private void tbUserName_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                reqLogin();
            }
        }

        private void login_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
