using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace Galileo
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            //webBrs.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrs_DocumentCompleted);
        }

        //是否开始图像扫描
        private Boolean isStartScan = false;


        private void webBrs_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //　确认网页已经加载完毕
            while (webBrs.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }

            textBox1.Text += e.Url.ToString();
            if (e.Url.ToString() == "http://test.alltobid.com/moni/gerenbid.html")
            {
                Thread threadGetData = new Thread(new ThreadStart(ThreadGetData));
                threadGetData.SetApartmentState(ApartmentState.STA);
                //调用Start方法执行线程
                isStartScan = true;
                threadGetData.Start();
                //threadGetData.Join();
            }
        }





        /// <summary>
        /// 创建循环读取时间和最低成交价格的线程
        /// </summary>
        private void ThreadGetData()
        {
            //bSystem.InvalidOperationException”类型的未经处理的异常在 System.Windows.Forms.dll 中发生其他信息: 线程间操作无效: 从不是创建控件“label1”的线程访问它。
            Control.CheckForIllegalCrossThreadCalls = false;
            while (isStartScan)
            {
                Console.WriteLine("开始扫描...");
                saveDataShot();
                //设置扫描时间间隔
                Thread.Sleep(global.scanInterval);
                
            }
            
        }

        [DllImport("User32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        //声明一个API函数
        //[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
        //private static extern bool BitBlt(
        //     IntPtr hdcDest, // 目标 DC的句柄
        //     int nXDest,
        //     int nYDest,
        //     int nWidth,
        //     int nHeight,
        //     IntPtr hdcSrc,  // 源DC的句柄
        //     int nXSrc,
        //     int nYSrc,
        //     System.Int32 dwRop  // 光栅的处理数值
        //     );


        private void saveDataShot()
        {
            /*
            // 这种方法是截取整个屏幕
            Bitmap b = new Bitmap(this.webBrs.ClientSize.Width, this.webBrs.ClientSize.Height);
            Point sP = this.webBrs.PointToScreen(this.webBrs.Location);
            Graphics.FromImage(b).CopyFromScreen(sP, new Point(0, 0), this.webBrs.ClientSize);
            b.Save("d:\\img.jpg");
            */

            /*这种方法只能在网页加载完毕后截图
            try
            {
                WebBrowser wb = new WebBrowser();
                wb = webBrs;
                Bitmap docImage = new Bitmap(this.webBrs.Width, this.webBrs.Height);
                webBrs.DrawToBitmap(docImage, new Rectangle(webBrs.Location.X, webBrs.Location.Y, webBrs.Width, webBrs.Height));
                //this.webBrs.DrawToBitmap(docImage, new Rectangle(this.webBrs.Location.X, this.webBrs.Location.Y, this.webBrs.Width, this.webBrs.Height));
                docImage.Save("D://img.jpg");
                wb.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            */


            try
            {
                int screenWidth = webBrs.Width;
                int screenHeight = webBrs.Height;

                IntPtr myIntptr = webBrs.Handle;
                int hwndInt = myIntptr.ToInt32();
                IntPtr hwnd = myIntptr;

                // Set hdc to the bitmap
                Bitmap bm = new Bitmap(screenWidth, screenHeight);
                Graphics g = Graphics.FromImage(bm);
                //得到webbrowser的DC
                IntPtr hdc = g.GetHdc();
                
                // Snapshot the WebBrowser

                bool result = PrintWindow(hwnd, hdc, 0);
                //BitBlt(hdc, 20, 20, 50, 50, myIntptr, 30, 30, 13369376);
                g.ReleaseHdc(hdc);
                g.Flush();

                // Save the bitmap, if successful

                if (result == true)
                    bm.Save("img.jpg");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         }



        private void frmMain_Load(object sender, EventArgs e)
        {
            //webBrs.Size = webPanel.Size;
            //this.webPanel.Controls.Add(webBrs);
            //webBrs.ScrollBarsEnabled = false;  // 隐藏滚动条
            webBrs.Navigate(global.hupaiUrl);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            isStartScan = false;
        }


        [DllImport("User32.dll")]
        public extern static System.IntPtr GetDC(System.IntPtr hWnd);

        private void btnCheckPos_Click(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Pen pen = new Pen(Color.Red, 5);
            g.DrawRectangle(pen, new Rectangle(980, 0, 50, 50));

            //PictureBox pb = new PictureBox();
            //pb.Location = (new Point(50,50));
            //pb.BackColor = Color.Transparent;
            //pb.BorderStyle = BorderStyle.FixedSingle;
            //this.Controls.Add(pb);

            //pb.BringToFront();


        }
    }
}
