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
using tessnet2;


namespace Galileo
{
    public partial class frmMain : Form
    {

        [DllImport("AspriseOCR.dll", EntryPoint = "OCR", CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr OCR(string file, int type);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]

        static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]

        static extern IntPtr OCRBarCodes(string file, int type);

        [DllImport("AspriseOCR.dll", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]

        static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);


        //[DllImport("AspriseOCR.dll", EntryPoint = "OCR")]
        //public static extern IntPtr OCR(string file, int type);

        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRpart")]
        //static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);

        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRBarCodes")]
        //static extern IntPtr OCRBarCodes(string file, int type);

        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRpartBarCodes")]
        //static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);

        private DateTime timeNow=DateTime.Today;       //当前时间
        private int lowerPrice=0;         //最低可成交价
        private Boolean hasFirstBid;    //是否已经第一次出价
        private Boolean testFlag=false;  //测试变量


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
            if (e.Url.ToString() == global.layPriceUrl)
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
                //截图
                saveDataShot();

                //识别图片
                recogniseImg();

                //执行策略
                excuteStrategy();
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

        //截图
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
                g.ReleaseHdc(hdc);
                g.Flush();

                // Save the bitmap, if successful

                if (result == true)
                    bm.Save(global.wholeShotImgPath);
                bm.Dispose();
                g.Dispose();



                ////正式情况下用
                CaptureImg(global.wholeShotImgPath, 176, 359, global.timeImgPath, 58, 13);
                if (timeNow >= Convert.ToDateTime("14:56:00"))   //11:00至11：30的价格（修改出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, 202, 375, global.priceImgPath, 43, 13);
                }
                else  //10：30至11：00的价格（首次出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, 202, 390, global.priceImgPath, 43, 13);
                }


                //测试的情况下用
                //CaptureImg(global.wholeShotImgPath, 176, 359, global.timeImgPath, 58, 13);
                //if (testFlag == true)   //11:00至11：30的价格（修改出价时段）
                //{
                //    CaptureImg(global.wholeShotImgPath, 202, 375, global.priceImgPath, 43, 13);
                //}
                //else  //10：30至11：00的价格（首次出价时段）
                //{
                //    CaptureImg(global.wholeShotImgPath, 202, 390, global.priceImgPath, 43, 13);
                //}


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region 从大图中截取一部分图片
        /// <summary>
        /// 从大图中截取一部分图片
        /// </summary>
        /// <param name="fromImagePath">来源图片地址</param>        
        /// <param name="offsetX">从偏移X坐标位置开始截取</param>
        /// <param name="offsetY">从偏移Y坐标位置开始截取</param>
        /// <param name="toImagePath">保存图片地址</param>
        /// <param name="width">保存图片的宽度</param>
        /// <param name="height">保存图片的高度</param>
        /// <returns></returns>
        private void CaptureImg(string fromImagePath, int offsetX, int offsetY, string toImagePath, int width, int height)
        {
            //原图片文件
            Image fromImage = Image.FromFile(fromImagePath);
            //创建新图位图
            Bitmap bitmap = new Bitmap(width, height);
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            //截取原图相应区域写入作图区
            graphic.DrawImage(fromImage, 0, 0, new Rectangle(offsetX, offsetY, width, height), GraphicsUnit.Pixel);
            //从作图区生成新图
            Image saveImage = Image.FromHbitmap(bitmap.GetHbitmap());
            //保存图片
            saveImage.Save(toImagePath, System.Drawing.Imaging.ImageFormat.Png);
            //释放资源   
            saveImage.Dispose();
            graphic.Dispose();
            bitmap.Dispose();
            fromImage.Dispose();
        }
        #endregion

        //Asprise图像识别
        private string executeOCR_By_Asprise(String imgPath)
        {
            string result = Marshal.PtrToStringAnsi(OCR(imgPath, -1));
            return result;
        }

        //tessnet2图像识别暂时不用
        private string executeOCR_By_tessnet2(String imgPath)
        {
            try
            {
                var image = new Bitmap(imgPath);
                var ocr = new Tesseract();
                ocr.SetVariable("tessedit_char_whitelist", "0123456789:"); // If digit only
                //@"C:\OCRTest\tessdata" contains the language package, without this the method crash and app breaks
                ocr.Init("tessdata", "eng", true);
                var result = ocr.DoOCR(image, Rectangle.Empty);
                String res = "";
                foreach (Word word in result)
                    res += word.Text;
                //Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
                return res;
            }
            catch (Exception exception)
            {
                return "tessnet2 error";
            }
        }

        //识别图像
        private void recogniseImg()
        {
            String time= adjOCR(executeOCR_By_Asprise(global.timeImgPath));
            String price= adjOCR(executeOCR_By_Asprise(global.priceImgPath));

            DateTime.TryParse(time, out timeNow);
            int.TryParse(price, out lowerPrice);

            //timeNow = Convert.ToDateTime(time);
            //lowerPrice = int.Parse(price);

            this.textBox1.Text = time;
            this.textBox1.Text += "\n";
            this.textBox1.Text += price;
        }

        //OCR纠错
        private string adjOCR(string text)
        {
            string result = text.Replace(" ", "");
            result = result.Replace("S", "3");
            result = result.Replace("l", "1");
            result = result.Replace("O", "0");
            result = result.Replace("o", "0");
            return result;
        }

        //执行策略
        private void excuteStrategy()
        {
            if(timeNow>=Convert.ToDateTime(global.firstBidTick) && hasFirstBid == false)
            {
                MessageBox.Show("第一次出价");
                hasFirstBid = true;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Console.WriteLine(timeNow);
            Console.WriteLine(lowerPrice);
            //webBrs.Size = webPanel.Size;
            //this.webPanel.Controls.Add(webBrs);
            //webBrs.ScrollBarsEnabled = false;  // 隐藏滚动条
            webBrs.Navigate(global.hupaiUrl);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            isStartScan = false;
        }


        //[DllImport("User32.dll")]
        //public extern static System.IntPtr GetDC(System.IntPtr hWnd);

        private void btnCheckPos_Click(object sender, EventArgs e)
        {
            testFlag = true;
            //this.textBox1.Text=executeOCR_By_Asprise("timetest.png");
            //this.textBox1.Text += "\n";
            //this.textBox1.Text += executeOCR_By_tessnet2("timetest.png");


            //Graphics g = this.CreateGraphics();
            //Pen pen = new Pen(Color.Red, 5);
            //g.DrawRectangle(pen, new Rectangle(980, 0, 50, 50));

            //System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Red);//画笔
            //System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷
            //System.Drawing.Graphics formGraphics = this.CreateGraphics();
            //formGraphics.FillEllipse(myBrush, new Rectangle(0, 0, 100, 200));//画实心椭圆
            //formGraphics.DrawEllipse(myPen, new Rectangle(0, 0, 100, 200));//空心圆
            //formGraphics.FillRectangle(myBrush, new Rectangle(0, 0, 100, 200));//画实心方
            //formGraphics.DrawRectangle(myPen, new Rectangle(0, 0, 100, 200));//空心矩形
            //formGraphics.DrawLine(myPen, 0, 0, 200, 200);//画线
            //formGraphics.DrawPie(myPen, 90, 80, 140, 40, 120, 100); //画馅饼图形
            //                                                        //画多边形
            //formGraphics.DrawPolygon(myPen, new Point[]{ new Point(30,140), new Point(270,250), new Point(110,240), new Point
            //(200,170), new Point(70,350), new Point(50,200)});
            ////清理使用的资源
            //myPen.Dispose();
            //myBrush.Dispose();
            //formGraphics.Dispose();


        }
    }
}
