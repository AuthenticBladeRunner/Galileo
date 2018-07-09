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

        private void btLogin_Click(object sender, EventArgs e)
        {
            gotoMainFrm();
        }

        //跳转到主程序
        private void gotoMainFrm()
        {
            if (verifyUserPwd(tbUserName.Text, tbPassword.Text))
            {
                this.Hide();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("您输入的用户名或密码有误");
            }
        }

        //验证用户名密码
        private Boolean verifyUserPwd(String userName, String password)
        {
            if (userName == "admin" && password == "111111")
            {
                
                return true;
            }
            else
            {
                return false;
            }
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                gotoMainFrm();
            }
        }

        private void login_Load(object sender, EventArgs e)
        {
            this.tbUserName.Focus();
            mainForm.loginForm = this;
        }
    }
}
