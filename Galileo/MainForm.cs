﻿using System;
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
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            //webBrs.Navigate(hupaiUrl);
        }

        private void webBrs_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            textBox1.Text += e.Url.ToString();
        }

        /*
        private void btnGoto_Click(object sender, EventArgs e)
        {
            webBrs.Navigate(tbUrl.Text);
        }
        */
    }
}