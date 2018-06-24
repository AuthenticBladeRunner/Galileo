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

        private SynchronizationContext mainThreadSynContext;
        private DateTime timeNow = DateTime.Today;       //当前时间
        private int lowerPrice = 0;                      //最低可成交价
        private int bdPrice;                             //标定价格
        private Boolean hasTestBid = false;              //是否已经测试打码
        private Boolean hasSetBDPrice = false;           //是否已经设定标定价格
        private Boolean hasLayPrice = false;             //是否已经正式出价
        private Boolean waitforSendPrice = false;        //
        private Boolean openTestKeyDect = false;         //开启测试打码键盘监控
        private Boolean openLayPriceKeyDect = false;     //开启出价打码键盘监控
        //private Boolean testFlag=false;                //测试变量

        private Point layPrcInptBoxCP= new Point(680, 420);        //点击出价输入框
        private Point layPrcBtnCP = new Point(845, 420);           //点击出价按钮
        private Point layPrcOkCP = new Point(600, 503);            //点击出价确定
        private Point layPrcCancelCP = new Point(795, 503);        //点击出价取消

        private Rectangle wholeScreenRect = new Rectangle(175, 398, 65, 13); //整屏区域
        private Rectangle prcAfter11Rect = new Rectangle(201, 414, 43, 13);  //11:00后价格区域
        private Rectangle prcBefore11Rect = new Rectangle(202, 430, 43, 13); //10:30-11:00的价格区域
        private Rectangle test1 = new Rectangle(202, 430, 43, 13);





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
            //while (webBrs.ReadyState != WebBrowserReadyState.Complete)
            //{
            //    Application.DoEvents();
            //}

            //textBox1.Text += e.Url.ToString();
            //if (e.Url.ToString() == global.layPriceUrl)
            //{
            //    Thread threadGetData = new Thread(new ThreadStart(ThreadGetData));
            //    threadGetData.SetApartmentState(ApartmentState.STA);
            //    //调用Start方法执行线程
            //    isStartScan = true;
            //    threadGetData.Start();
            //    //threadGetData.Join();
            //}
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
                //整个browser截图
                saveDataShot();

                //识别图片信息
                recogniseImg();

                //执行策略
                excuteStrategy();
                //设置扫描时间间隔
                Thread.Sleep(global.scanInterval);

            }

        }

        [DllImport("User32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

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



                //51沪牌模拟
                CaptureImg(global.wholeShotImgPath, global.timeImgPath, wholeScreenRect);
                //File.Copy(global.timeImgPath, "img/"+DateTime.Now.Second.ToString() + ".png");
                if (timeNow >= Convert.ToDateTime("11:00:00"))   //11:00至11：30的价格（修改出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, global.priceImgPath, prcAfter11Rect);
                }
                else  //10：30至11：00的价格（首次出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, global.priceImgPath, prcBefore11Rect);
                }

                ////正式情况下用
                //CaptureImg(global.wholeShotImgPath, 176, 359, global.timeImgPath, 58, 13);
                //if (timeNow >= Convert.ToDateTime("14:56:00"))   //11:00至11：30的价格（修改出价时段）
                //{
                //    CaptureImg(global.wholeShotImgPath, 202, 375, global.priceImgPath, 43, 13);
                //}
                //else  //10：30至11：00的价格（首次出价时段）
                //{
                //    CaptureImg(global.wholeShotImgPath, 202, 390, global.priceImgPath, 43, 13);
                //}


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
        private void CaptureImg(string fromImagePath, string toImagePath, Rectangle r)
        {
            //原图片文件
            Image fromImage = Image.FromFile(fromImagePath);
            //创建新图位图
            Bitmap bitmap = new Bitmap(r.Width, r.Height);
            //创建作图区域
            Graphics graphic = Graphics.FromImage(bitmap);
            //截取原图相应区域写入作图区
            graphic.DrawImage(fromImage, 0, 0, r, GraphicsUnit.Pixel);
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

            //string result = Marshal.PtrToStringAnsi(OCR(imgPath, -1));
            //return result;
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
            String time = adjOCR(executeOCR_By_Asprise(global.timeImgPath));
            String price = adjOCR(executeOCR_By_Asprise(global.priceImgPath));

            //部分识别速度是否会快一些
            //String time = adjOCR(executeOCR_By_Asprise(global.wholeShotImgPath, 175, 398, 65, 13));
            //String price = "";
            //if (timeNow >= Convert.ToDateTime("11:00:00"))   //11:00至11：30的价格（修改出价时段)
            //{
            //    price = adjOCR(executeOCR_By_Asprise(global.wholeShotImgPath, 201, 414, 43, 13));
            //}
            //else //10：30至11：00的价格（首次出价时段）
            //{
            //    price = adjOCR(executeOCR_By_Asprise(global.wholeShotImgPath, 202, 390, 43, 13));
            //}     

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
            //方案一
            string result = text.Replace(" ", "4");
            result = result.Replace("S", "3");
            result = result.Replace("l", "1");
            result = result.Replace("O", "0");
            result = result.Replace("o", "0");
            return result;

            //方案二
            //string result = text.Replace(" ", "");
            //result = result.Replace("S", "3");
            //result = result.Replace("l", "1");
            //result = result.Replace("O", "0");
            //result = result.Replace("o", "0");
            //return result;
        }

        //执行策略
        private void excuteStrategy()
        {
            //打码测试
            if (timeNow >= Convert.ToDateTime(global.testTypeTick) && hasTestBid == false)
            {
                //开启新线程
                Thread threadGetData = new Thread(new ThreadStart(callGUItoTestType));
                //调用Start方法执行线程
                threadGetData.Start();
                hasTestBid = true;
            }
            //设定标定价格
            if(timeNow >= Convert.ToDateTime(global.setBDPriceTick) && hasSetBDPrice == false)
            {
                bdPrice = lowerPrice+global.bdAddPrice;
                this.textBox2.Text += timeNow.TimeOfDay.ToString() + " 设标价时的最低价格:" + lowerPrice;
                textBox2.AppendText("\r\n");
                hasSetBDPrice = true;
                this.textBox2.Text += timeNow.TimeOfDay.ToString()+" 标定价格:" +bdPrice;
                textBox2.AppendText("\r\n");
            }
            //正式出价
            if (timeNow >= Convert.ToDateTime(global.layPriceTick) && hasLayPrice == false)
            {
                //开启新线程
                Thread threadGetData = new Thread(new ThreadStart(callGUItolayPrice));
                //调用Start方法执行线程
                threadGetData.Start();
                hasLayPrice = true;
                //textBox1.Text += bdPrice;
            }
            //发送价格
            if (lowerPrice >= bdPrice && waitforSendPrice == true)
            {
                //开启新线程
                Thread threadGetData = new Thread(new ThreadStart(callGUItoSendPrice));
                //调用Start方法执行线程
                threadGetData.Start();
                textBox2.Text += timeNow.TimeOfDay.ToString() + " 出价时的最低成交价：" +lowerPrice;
                textBox2.AppendText("\r\n"); ;
            }

            
        }

        private void callGUItoTestType()
        {
            mainThreadSynContext.Post(new SendOrPostCallback(testType), null);//通知主线程
        }

        private void callGUItolayPrice()
        {
            mainThreadSynContext.Post(new SendOrPostCallback(layPrice), null);//通知主线程
        }

        private void callGUItoSendPrice()
        {
            mainThreadSynContext.Post(new SendOrPostCallback(sendPrice), null);//通知主线程
        }

        //打码测试函数
        private void testType(object state)
        {
            //按出价输入框
            virtlMouClk(layPrcInptBoxCP);
            /*
            SendKeys.Send  异步模拟按键(不阻塞UI)
            SendKeys.SendWait  同步模拟按键(会阻塞UI直到对方处理完消息后返回)
            */
            SendKeys.SendWait((lowerPrice + global.testAddPrice).ToString());
            Thread.Sleep(300);
            SendKeys.Flush();

            textBox2.Text += timeNow.TimeOfDay.ToString() + " 测试出价：" + (lowerPrice + global.testAddPrice).ToString();
            textBox2.AppendText("\r\n"); ;

            //按出价
            virtlMouClk(layPrcBtnCP);
            //开启键盘监控
            openTestKeyDect = true;
        }

        //正式出价
        private void layPrice(object state)
        {
            //按出价输入框
            virtlMouClk(layPrcInptBoxCP);
            /*
            SendKeys.Send  异步模拟按键(不阻塞UI)
            SendKeys.SendWait  同步模拟按键(会阻塞UI直到对方处理完消息后返回)
            */
            SendKeys.SendWait((lowerPrice + global.AddPrice).ToString());
            textBox2.Text += timeNow.TimeOfDay.ToString() + " 正式出价时最低价：" + lowerPrice.ToString();
            this.textBox2.AppendText("\r\n");
            textBox2.Text += timeNow.TimeOfDay.ToString() + " 正式出价：" + (lowerPrice + global.AddPrice).ToString();
            this.textBox2.AppendText("\r\n");
            Thread.Sleep(300);
            SendKeys.Flush();

            //按出价
            virtlMouClk(layPrcBtnCP);
            openTestKeyDect = false;
            //开启键盘监控
            openLayPriceKeyDect = true;
        }

        //发送价格
        private void sendPrice(object state)
        {
            virtlMouClk(layPrcOkCP);
            waitforSendPrice = false;
        }

        /*
         * 模拟鼠标点击
         * param x:横坐标
         * param y:纵坐标
         * 
         */
        private void virtlMouClk(Point point)
        {
            //x = 100; // X coordinate of the click 
            //y = 80; // Y coordinate of the click 
            IntPtr handle = webBrs.Handle;
            StringBuilder className = new StringBuilder(100);
            while (className.ToString() != "Internet Explorer_Server") // The class control for the browser 
            {
                handle = GetWindow(handle, 5); // Get a handle to the child window 
                GetClassName(handle, className, className.Capacity);
            }

            IntPtr lParam = (IntPtr)((point.Y << 16) | point.X); // The coordinates 
            IntPtr wParam = IntPtr.Zero; // Additional parameters for the click (e.g. Ctrl) 
            const uint downCode = 0x201; // Left click down code 
            const uint upCode = 0x202; // Left click up code 
            SendMessage(handle, downCode, wParam, lParam); // Mouse button down 
            SendMessage(handle, upCode, wParam, lParam); // Mouse button up 
            //textBox1.Text+="click:"+x+" "+y;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            mainThreadSynContext = SynchronizationContext.Current; //在这里记录主线程的上下文
            //Console.WriteLine(timeNow);
            //Console.WriteLine(lowerPrice);
            webBrs.Navigate(global.hupaiUrl);

            //开启线程
            Thread threadGetData = new Thread(new ThreadStart(ThreadGetData));
            threadGetData.SetApartmentState(ApartmentState.STA);
            //调用Start方法执行线程
            isStartScan = true;
            threadGetData.Start();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            isStartScan = false;
        }

        private void btnCheckPos_Click(object sender, EventArgs e)
        {
            CaptureImg("7.jpg", "xxx.png", test1);
        }

        private void webBrs_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (openTestKeyDect == true)
            {
                if (e.KeyCode.ToString() == "Return")
                {
                    //MessageBox.Show("按了回车");
                    virtlMouClk(layPrcCancelCP);
                    SendKeys.SendWait("{BACKSPACE}");
                    openTestKeyDect = false;
                }
            }

            if (openLayPriceKeyDect == true)
            {
                if (e.KeyCode.ToString() == "Return")
                {
                    openLayPriceKeyDect = false;
                    waitforSendPrice = true;
                    textBox2.Text+="您已出价，请稍后...";
                    this.textBox2.AppendText("\r\n");
                }
            }
            
        }

        private void webBrs_NewWindow(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            webBrs.Navigate(webBrs.StatusText);
        }
    }
}
