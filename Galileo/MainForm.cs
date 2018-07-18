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
using Tesseract;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;

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
        private DateTime timeNow = DateTime.Today;                           //当前时间
        DateTime CapTime;                                                    //总控时间
        int CapPrice;                                                        //总控价格
        private int testAddPrice = 700;                                      //测试加价数量
        private int lowerPrice = 0;                                          //最低可成交价
        private int bdPrice;                                                 //标定价格 (价格阈值, 当最低价达到此价格时提交)
        private DateTime setBDPriceTick;                                     //设定标定价格时间
        private int bdAddPrice;                                              //标定价格在最低价位基础上加价多少
        private int bdAddPriceAdj=300;                                       //标定价格加价调根据经验整默认为300
        private DateTime latestLayTick;                                      //最晚出价时间
        private string ambushTime=null;                                      //伏击时间
        private DateTime ambushSecTime;                                      //伏击时间秒部分
        private int ambushTimeMill;                                          //伏击时间毫秒部分
        private string ambushPriceStr=null;                                  //伏击价格规则
        private int ambushPrice = 0;                                         //伏击价格
        private double commtDelay=0;                                         //提交延时
        private Boolean hasSetBDPrice = false;                               //是否已经设定标定价格
        private Boolean hasInitAmbSeq = false;                               //是否已经进入伏击程序
        private Boolean hasAmbushPrice = false;                              //是否已经伏击
        private Boolean hasSendPrice = false;                                //是否已经正式出价
        private Boolean openTestKeyDect = false;                             //开启测试打码键盘监控
        private Boolean openLayPriceKeyDect = false;                         //开启出价打码键盘监控
        //private Boolean testFlag=false;                                    //测试变量

        private Point layPrcInptBoxCP= new Point(680, 420);        //点击出价输入框
        private Point layPrcBtnCP = new Point(845, 420);           //点击出价按钮
        private Point layPrcOkCP = new Point(600, 503);            //点击出价确定
        private Point layPrcCancelCP = new Point(795, 503);        //点击出价取消

        private Rectangle timeRect = new Rectangle(175, 398, 65, 13);        //时间区域
        private Rectangle prcAfter11Rect = new Rectangle(201, 414, 43, 13);  //11:00后价格区域
        private Rectangle prcBefore11Rect = new Rectangle(202, 430, 43, 13); //10:30-11:00的价格区域
        private Rectangle test1 = new Rectangle(202, 430, 43, 13);


        // 智能最高价: max(40s+1100元, 45s+1000元, 48s+800元, 50s+700元)
        // 智能极高价: max(40s+1200元, 45s+1100元, 48s+800元, 50s+700元)
        // 智能最高价及极高价的Tick时间
        private DateTime[] intelPriceTickArr = { DateTime.Parse("11:29:40"), DateTime.Parse("11:29:45"), DateTime.Parse("11:29:48"), DateTime.Parse("11:29:50")};
        private int intelMaxPrice = 0;  //智能最高价出价
        private int intelExtmPrice = 0;  //智能极高价出价
        private int[] intelMaxAddPrice = { 1100, 1000, 800, 700 };   //智能最高价加价
        private int[] intelExtmAddPrice = { 1200, 1100, 800, 700 };   //智能极高价加价


        private Image timeImg = null;   // new Bitmap(0, 0);
        private Image priceImg = null;  // new Bitmap(0, 0);


        private string captainAddr = null;              // 总控的IP地址
        private const int captainPort = 8850;           // 总控用于发送/接收消息的端口号
        private const int juniorPort = 8849;            // 下属用于发送/接收消息的端口号



        // 新建UdpClient并绑定端口，用于发送和接收UDP消息
        private UdpClient udpCli = new UdpClient(juniorPort);

        private string myUserId = null;
        private Dictionary<string, object> myParam = null;      // 参数配置
        // OCR Engine
        private TesseractEngine tessEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

        public login loginForm;
        public bool loggedIn = false;

        public frmMain()
        {
            InitializeComponent();
            //webBrs.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrs_DocumentCompleted);
        }

        public void InitConn()
        {
            var reqBytes = Encoding.UTF8.GetBytes("LookForCaptain");   // 寻找总控 的消息
            var serverEp = new IPEndPoint(IPAddress.Any, 0);

            // 广播这条消息
            udpCli.EnableBroadcast = true;
            udpCli.Send(reqBytes, reqBytes.Length, new IPEndPoint(IPAddress.Broadcast, captainPort));

            // 异步接收消息（收到消息后回调函数会被调用）
            udpCli.BeginReceive(new AsyncCallback(gotUdpMsg), null);
            
        }

        // UdpClient 接收到消息后的回调函数
        private void gotUdpMsg(IAsyncResult res)
        {
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            byte[] msgBin = { };
            try
            {
                msgBin = udpCli.EndReceive(res, ref remoteEp);
            }
            catch (Exception e)         // 如果远程主机关闭, 会产生 System.Net.Sockets.SocketException: 远程主机强迫关闭了一个现有的连接
            {
                Console.WriteLine(e);
            }

            // 继续接收下一条消息
            udpCli.BeginReceive(new AsyncCallback(gotUdpMsg), null);

            if (msgBin.Length <= 0)
                return;

            // 处理接收到的消息
            string msgStr = Encoding.UTF8.GetString(msgBin);
            int iSplit = msgStr.IndexOf(':');   // 找到分隔符的位置
            string msgType, msgCont;            // 消息类型, 消息内容
            if (iSplit >= 0)
            {
                msgType = msgStr.Substring(0, iSplit);
                msgCont = msgStr.Substring(iSplit + 1).Trim();
            }
            else
            {
                msgType = msgStr;
                msgCont = "";
            }
            Console.WriteLine("收到 {0}:{1} 发来的类型为「{2}」的消息, 内容为: 「{3}」", 
                remoteEp.Address.ToString(), remoteEp.Port, msgType, msgCont);

            switch(msgType)
            {
                case "IAmCaptain":          // 「我是总控」
                    captainAddr = remoteEp.Address.ToString();
                    lbCaptain.Text = captainAddr;
                    break;
                case "MyParam":
                    myParam = JsonConvert.DeserializeObject<Dictionary<string, object>>(msgCont);
                    setParam();
                    break;
                case "LoginResult":
                    loginForm.LoginCallback(msgCont);
                    break;
            }

            if (loggedIn)
            {
                switch (msgType)
                {
                    case "FastestData":
                        // 更新时间和价格
                        exeTimeMonitor(msgCont);
                        break;
                    case "initTest":
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            testType();
                        });
                        //Thread threadTestType = new Thread(new ThreadStart(callGUItoTestType));
                        //threadTestType.Start();
                        break;
                    case "canlTest":
                        Thread threadCanlTest = new Thread(new ThreadStart(callGUItoCanlTest));
                        threadCanlTest.Start();
                        break;
                }
            }
        }

        //给参数赋值
        private void setParam()
        {
            //设置伏击时间
            if (myParam["伏击时间"].ToString() != "")
            {
                lbHander.Text = myParam["拍手"].ToString();
                lbBids.Text = myParam["账号"].ToString();
                lbPassword.Text= myParam["密码"].ToString();
                lbID.Text= myParam["身份证"].ToString();
                //double d = double.Parse(myParam["伏击时间"].ToString());
                //将伏击时间由小数转换为时间格式的string
                //ambushTime = DateTime.FromOADate(d).ToString("HH:mm:ss.f");
                ambushTime = myParam["伏击时间"].ToString();
                ambushSecTime = DateTime.Parse(ambushTime.Substring(0, 8));
                ambushTimeMill = int.Parse(ambushTime.Substring(9, 1));
                System.Console.WriteLine("伏击时间: " + ambushTime);
            }
            else
            {
                MessageBox.Show("伏击时间为空，请联系管理员");
            }


            //设置伏击价格
            if (myParam["伏击价格"].ToString() != "")
            {
                ambushPriceStr = myParam["伏击价格"].ToString();
                System.Console.WriteLine("伏击价格: "+ ambushPriceStr);
            }
            else
            {
                MessageBox.Show("伏击价格为空，请联系管理员");
            }

            
            if (myParam["5x提交"].ToString() != "")
            {
                string[] threArr = myParam["5x提交"].ToString().Split('+');

                //设置标定价格时间
                setBDPriceTick = Convert.ToDateTime("11:29:" + threArr[0]);
                System.Console.WriteLine("标定价格时间"+setBDPriceTick);

                //设置标定价格加价多少
                bdAddPrice = int.Parse(threArr[1]);
                System.Console.WriteLine("标定价格加价"+bdAddPrice);
            }
            else
            {
                MessageBox.Show("5x提交为空，请联系管理员");
            }

            //设置最晚提交时间
            if (myParam["最晚提交时间"].ToString() != "")
            {
                latestLayTick = Convert.ToDateTime("11:29:" + myParam["最晚提交时间"].ToString());
                System.Console.WriteLine("最晚提交时间"+latestLayTick);
            }
            else
            {
                MessageBox.Show("最晚提交时间为空，请联系管理员");
            }

            //设置提交延时
            if (myParam["5x提交延时"].ToString() != "")
            {
                commtDelay = double.Parse(myParam["5x提交延时"].ToString());
                System.Console.WriteLine("5x提交延时"+commtDelay);
            }
            else
            {
                MessageBox.Show("5x提交延时为空，请联系管理员");
            }
            

        }

        //public DateTime FromOADatePrecise(double d)
        //{
        //    DateTime oaEpoch = new DateTime(1899, 12, 30);

        //    if (!(d >= 0))
        //        throw new ArgumentOutOfRangeException(); // NaN or negative d not supported

        //    return oaEpoch + TimeSpan.FromTicks(Convert.ToInt64(d * TimeSpan.TicksPerDay));
        //}


        // 向总控发送登录请求 (发送成功返回true)
        public bool ReqLogin(string userId)
        {
            if (String.IsNullOrEmpty(captainAddr))
                return false;
            myUserId = userId;
            byte[] bin = Encoding.UTF8.GetBytes("ReqLogin: " + userId);
            udpCli.Send(bin, bin.Length, captainAddr, captainPort);
            return true;
        }

        // 向总控发送已出价信息 (发送成功返回true)
        public bool reportSendPrice()
        {
            if (String.IsNullOrEmpty(captainAddr))
                return false;
            byte[] bin = Encoding.UTF8.GetBytes("missionComplete: " + myUserId + ";" + ambushPrice);
            udpCli.Send(bin, bin.Length, captainAddr, captainPort);
            return true;
        }

        // 向总控上报我的时间和价格
        private void reportMine(string timeStr, string priceStr)
        {
            if (String.IsNullOrEmpty(captainAddr)) return;
            byte[] bin = Encoding.UTF8.GetBytes("MyData: " + timeStr + ";" + priceStr);
            udpCli.Send(bin, bin.Length, captainAddr, captainPort);
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
                //Console.WriteLine("开始扫描...");
                //截图
                takeScrnshot();

                //识别图片信息
                recogniseImg();

                //执行策略
                //excuteStrategy();

                timeImg.Dispose();
                priceImg.Dispose();

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
        private void takeScrnshot()
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
                Stopwatch sw = new Stopwatch();
                //sw.Reset();
                sw.Start();

                // Create a new bitmap
                Bitmap bm = new Bitmap(webBrs.Width, webBrs.Height);
                Graphics g = Graphics.FromImage(bm);
                IntPtr hdc = g.GetHdc();

                // Print WebBrowser snapshot to hdc
                bool result = PrintWindow(webBrs.Handle, hdc, 0);

                g.ReleaseHdc(hdc);
                g.Flush();


                // Save the bitmap, if successful
                //if (result == true)
                //    bm.Save(global.wholeShotImgPath);
                //bm.Dispose();
                g.Dispose();

                // Crop the screenshot to get the time shot and price shot
                timeImg = cropImage(bm, timeRect);
                //timeImg.Save(global.timeImgPath);
                if (timeNow >= Convert.ToDateTime("11:00:00"))   //11:00至11：30的价格（修改出价时段）
                {
                    priceImg = cropImage(bm, prcAfter11Rect);
                }
                else  //10：30至11：00的价格（首次出价时段）
                {
                    priceImg = cropImage(bm, prcBefore11Rect);
                }
                //priceImg.Save(global.priceImgPath);

                bm.Dispose();

                /*
                //51沪牌模拟
                CaptureImg(global.wholeShotImgPath, global.timeImgPath, timeRect);
                //File.Copy(global.timeImgPath, "img/"+DateTime.Now.Second.ToString() + ".png");
                if (timeNow >= Convert.ToDateTime("11:00:00"))   //11:00至11：30的价格（修改出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, global.priceImgPath, prcAfter11Rect);
                }
                else  //10：30至11：00的价格（首次出价时段）
                {
                    CaptureImg(global.wholeShotImgPath, global.priceImgPath, prcBefore11Rect);
                }
                //*/

                sw.Stop();
                Console.WriteLine("Snapshot time: " + sw.ElapsedMilliseconds + " ms");

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

        // 裁剪图片
        private Image cropImage(Image srcImg, Rectangle r)
        {
            //创建新图位图
            Bitmap desBmp = new Bitmap(r.Width, r.Height);
            //创建作图区域
            Graphics g = Graphics.FromImage(desBmp);
            //截取原图相应区域写入作图区
            g.DrawImage(srcImg, 0, 0, r, GraphicsUnit.Pixel);

            //释放资源   
            g.Dispose();

            // The Bitmap class is an implementation of the Image class. The Image class is an abstract class.
            return desBmp;
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
        
        // TessNet2 is based on Tesseract v2.04 and has not been updated since September 2009.
        // Tesseract 3 .NET wrapper is available here: https://github.com/charlesw/tesseract
        private string executeOCR_By_tesseract(Image srcImg)
        {
            try
            {
                //var img = Pix.LoadFromFile(imgPath);
                //var srcImg = System.Drawing.Image.FromFile(imgPath);
                var img = scaleImage(srcImg, 2.3, 2);        // Scale up and extend the canvas to get a better result
                srcImg.Dispose();

                tessEngine.SetVariable("tessedit_char_whitelist", "0123456789:");   // Digits & colons only
                //tessEngine.DefaultPageSegMode = PageSegMode.SingleWord;     // Without this, the text may not be recognized at all (because of the narrow page margin)

                var page = tessEngine.Process(img, PageSegMode.SingleWord);     // 如果使用SingleBlock, 识别结果中可能包含空格
                var text = page.GetText().Trim();

                page.Dispose();
                img.Dispose();

                Console.WriteLine(text);
                return text;


            }
            catch (Exception e)
            {
                Console.WriteLine("Tesseract error: " + e.ToString());
                return "Tesseract error: " + e.ToString();
            }
        }

        // 按比例缩放图像 (并拓宽边界, 以提高识别率)
        private Bitmap scaleImage(Image srcImg, double wscale, double hscale)
        {
            var desWidth = (int)(srcImg.Width * wscale);
            var desHeight = (int)(srcImg.Height * hscale);

            Bitmap newimg = new Bitmap(desWidth + 20, desHeight + 20);      // 增加边距

            using (Graphics g = Graphics.FromImage(newimg))
            {
                g.Clear(Color.White);

                // Here you set your interpolation mode
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                // Scale the image, by drawing it on the larger bitmap
                g.DrawImage(srcImg, new Rectangle(10, 10, desWidth, desHeight));
            }

            return newimg;
        }

        //识别图像
        private void recogniseImg()
        {
            Stopwatch sw = new Stopwatch();
            //sw.Reset();
            sw.Start();

            //String time = adjOCR(executeOCR_By_Asprise(global.timeImgPath));
            //String price = adjOCR(executeOCR_By_Asprise(global.priceImgPath));

            String time = executeOCR_By_tesseract(timeImg);
            String price = executeOCR_By_tesseract(priceImg);

            sw.Stop();
            Console.WriteLine("OCR time: " + sw.ElapsedMilliseconds + " ms");


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

            var prevTime = timeNow;

            DateTime.TryParse(time, out timeNow);
            int.TryParse(price, out lowerPrice);

            if (timeNow > prevTime)
                reportMine(time, price);    // 向总控汇报我的时间和价格
                

            //timeNow = Convert.ToDateTime(time);
            //lowerPrice = int.Parse(price);

            this.textBox1.Text = time + Environment.NewLine + price;
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

        //时间监控，到时间执行策略
        private void exeTimeMonitor(String msgCont)
        {
            //解析传来的时间和价格信息
            string[] msgArr = msgCont.Split(';');
            CapTime = DateTime.Parse(msgArr[0]);
            CapPrice = int.Parse(msgArr[1]);
            int ambushPrice=1000000;
            //System.Console.WriteLine(CapTime);
            //System.Console.WriteLine(CapPrice);

            //监测是否要提取智能最高价
            int maxPriceArrIdx = Array.IndexOf(intelPriceTickArr, CapTime);
            if (maxPriceArrIdx != -1)
            {
                if (CapPrice + intelMaxAddPrice[maxPriceArrIdx] > intelMaxPrice)
                {
                    intelMaxPrice = CapPrice + intelMaxAddPrice[maxPriceArrIdx];
                }
                if (CapPrice + intelExtmAddPrice[maxPriceArrIdx] > intelExtmPrice)
                {
                    intelExtmPrice = CapPrice + intelExtmAddPrice[maxPriceArrIdx];
                }
            }
            System.Console.WriteLine("intelMaxPrice: " + intelMaxPrice);
            System.Console.WriteLine("intelExtmPrice: " + intelExtmPrice);


            //监测是否到了提取标定价格的时间
            if (hasSetBDPrice == false && CapTime >= setBDPriceTick)
            {
                //设定标定价格
                bdPrice = CapPrice + bdAddPrice - bdAddPriceAdj;
                
                textBox2.AppendText(DateTime.Now.ToString("[HH:mm:ss.fff]") + Environment.NewLine);
                textBox2.AppendText("国拍网时间: " + CapTime.ToString("HH:mm:ss") + Environment.NewLine);
                textBox2.AppendText("设定标定价格..." + Environment.NewLine);
                textBox2.AppendText("当前最低价格: " + CapPrice + Environment.NewLine);
                textBox2.AppendText("标定价格设为: " + bdPrice + Environment.NewLine + Environment.NewLine);
            }

            //监测是否到了伏击时间
            if (CapTime >= ambushSecTime && hasAmbushPrice==false && hasInitAmbSeq==false)
            {
                //Thread threadGetData = new Thread(new ThreadStart(callGUItoLayPrice, CapPrice));
                //threadGetData.Start();
                //textBox1.Text += bdPrice;

                // The "invoke" call tells the form "Please execute this code in your thread rather than mine."
                hasInitAmbSeq = true;
                //如果时间没有遗漏，则进行伏击延时
                if (CapTime == ambushSecTime)
                {
                    Thread.Sleep(ambushTimeMill * 100);
                }
                this.Invoke((MethodInvoker)delegate ()
                {
                    ambushPrice=layPrice();
                });
            }

            //监测是否最低可成交价大于标定价并出价
            if (hasAmbushPrice==true && CapPrice >= bdPrice && hasSendPrice == false)
            {
                textBox2.AppendText("标定价出价...\n\t");
                this.Invoke((MethodInvoker)delegate ()
                {
                    sendPrice();
                });
            }

            //最晚提交时间出价
            if (CapTime >= latestLayTick && hasSendPrice == false)
            {
                textBox2.AppendText("最晚提交时间已到，现在立即出价...\n\t");
                this.Invoke((MethodInvoker)delegate ()
                {
                    sendPrice();
                });
            }

            //最低可成交价大于等于出价-300就立刻出价
            if (hasAmbushPrice == true && CapPrice >= ambushPrice-300 && hasSendPrice == false)
            {
                textBox2.AppendText("最低可成交价已经大于等于出价-300,现在立即出价..\n\t");
                this.Invoke((MethodInvoker)delegate ()
                {
                    sendPrice();
                });
            }
        }

        //执行策略
        //private void excuteStrategy()
        //{
        //    //打码测试
        //    if (timeNow >= Convert.ToDateTime(global.testTypeTick) && hasTestBid == false)
        //    {
        //        //开启新线程
        //        Thread threadGetData = new Thread(new ThreadStart(callGUItoTestType));
        //        //调用Start方法执行线程
        //        threadGetData.Start();
        //        hasTestBid = true;
        //    }
        //    //设定标定价格
        //    if(timeNow >= Convert.ToDateTime(global.setBDPriceTick) && hasSetBDPrice == false)
        //    {
        //        bdPrice = lowerPrice+global.bdAddPrice;
        //        this.textBox2.Text += timeNow.TimeOfDay.ToString() + " 设标价时的最低价格:" + lowerPrice;
        //        textBox2.AppendText("\r\n");
        //        hasSetBDPrice = true;
        //        this.textBox2.Text += timeNow.TimeOfDay.ToString()+" 标定价格:" +bdPrice;
        //        textBox2.AppendText("\r\n");
        //    }
        //    //正式出价
        //    if (timeNow >= Convert.ToDateTime(global.layPriceTick) && hasLayPrice == false)
        //    {
        //        //开启新线程
        //        Thread threadGetData = new Thread(new ThreadStart(callGUItolayPrice));
        //        //调用Start方法执行线程
        //        threadGetData.Start();
        //        hasLayPrice = true;
        //        //textBox1.Text += bdPrice;
        //    }
        //    //发送价格
        //    if (lowerPrice >= bdPrice && waitforSendPrice == true)
        //    {
        //        //开启新线程
        //        Thread threadGetData = new Thread(new ThreadStart(callGUItoSendPrice));
        //        //调用Start方法执行线程
        //        threadGetData.Start();
        //        textBox2.Text += timeNow.TimeOfDay.ToString() + " 出价时的最低成交价：" +lowerPrice;
        //        textBox2.AppendText("\r\n"); ;
        //    }           
        //}

        private void callGUItoTestType()
        {
            //mainThreadSynContext.Post(new SendOrPostCallback(testType), null);//通知主线程
        }

        private void callGUItoLayPrice(int CapPrice)
        {
            //mainThreadSynContext.Post(new SendOrPostCallback(layPrice), null);//通知主线程
        }

        private void callGUItoSendPrice()
        {
            //mainThreadSynContext.Post(new SendOrPostCallback(sendPrice), null);//通知主线程
        }

        private void callGUItoCanlTest()
        {
            mainThreadSynContext.Post(new SendOrPostCallback(cancelTest), null);//通知主线程
        }

        //打码测试函数
        private void testType()
        {
            //按出价输入框
            virtlMouClk(layPrcInptBoxCP);
            /*
            SendKeys.Send  异步模拟按键(不阻塞UI)
            SendKeys.SendWait  同步模拟按键(会阻塞UI直到对方处理完消息后返回)
            */
            SendKeys.SendWait((CapPrice + testAddPrice).ToString());

            textBox2.AppendText(DateTime.Now.ToString("[HH:mm:ss.fff]") + Environment.NewLine);
            textBox2.AppendText("现在验证码测试..." + Environment.NewLine);

            Thread.Sleep(300);
            SendKeys.Flush();

            textBox2.AppendText("国拍网时间: " + CapTime.ToString("HH:mm:ss") + Environment.NewLine);
            textBox2.AppendText("测试出价: " + (CapPrice + testAddPrice).ToString() + Environment.NewLine + Environment.NewLine);

            //按出价
            virtlMouClk(layPrcBtnCP);
            //开启键盘监控
            openTestKeyDect = true;
        }

        //伏击出价
        private int layPrice()
        {
            //按出价输入框
            virtlMouClk(layPrcInptBoxCP);
            /*
            SendKeys.Send  异步模拟按键(不阻塞UI)
            SendKeys.SendWait  同步模拟按键(会阻塞UI直到对方处理完消息后返回)
            */
            ambushPrice = calAmbushPrice(CapPrice);
            SendKeys.SendWait(ambushPrice.ToString());

            textBox2.AppendText(DateTime.Now.ToString("[HH:mm:ss.fff]") + Environment.NewLine);
            textBox2.AppendText("现在正式出价..." + Environment.NewLine);
            textBox2.AppendText("国拍网时间: " + CapTime.ToString("HH:mm:ss") + Environment.NewLine);
            textBox2.AppendText("当前最低价: " + CapPrice + Environment.NewLine);
            textBox2.AppendText("正式出价价格: " + ambushPrice + Environment.NewLine + Environment.NewLine);

            Thread.Sleep(300);
            SendKeys.Flush();

            //按出价
            virtlMouClk(layPrcBtnCP);
            openTestKeyDect = false;
            //开启键盘监控
            openLayPriceKeyDect = true;
            return ambushPrice;
        }

        private int calAmbushPrice(int CapPrice)
        {
            if (ambushPriceStr == "大小数")
            {
                return 0;
            }
            else if(ambushPriceStr == "最高价")
            {
                System.Console.WriteLine(intelMaxPrice);
                //return Math.Max(CapPrice + int.Parse(ambushPriceStr), intelMaxPrice);
                return Math.Max(CapPrice, intelMaxPrice);
            }
            else if (ambushPriceStr == "最高价+100")
            {
                System.Console.WriteLine(intelExtmPrice);
                //return Math.Max(CapPrice + int.Parse(ambushPriceStr), intelExtmPrice);
                return Math.Max(CapPrice, intelExtmPrice);
            }
            else
            {
                return CapPrice + int.Parse(ambushPriceStr);
            }
        }

        // (输完验证码后的)正式提交
        private void sendPrice()
        {
            if (CapPrice == bdPrice)
            {
                Thread.Sleep((int)(commtDelay*1000));
            }
            virtlMouClk(layPrcOkCP);
            hasSendPrice = true;
            reportSendPrice();
        }

        //取消测试
        private void cancelTest(object state)
        {
            if (openTestKeyDect == true)
            {
                virtlMouClk(layPrcCancelCP);
                SendKeys.SendWait("{BACKSPACE}");
                openTestKeyDect = false;
            }
        }

        /*
         * 模拟鼠标点击
         * Point:点击坐标
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
            InitConn();
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
            // 发出登出请求
            if (!String.IsNullOrEmpty(captainAddr))
            {
                byte[] bin = Encoding.UTF8.GetBytes("ReqLogout: " + myUserId);
                udpCli.Send(bin, bin.Length, captainAddr, captainPort);
            }
            System.Environment.Exit(0);
        }

        private void btnCheckPos_Click(object sender, EventArgs e)
        {
            Bitmap bm = new Bitmap(webBrs.Width, webBrs.Height);
            Graphics g = Graphics.FromImage(bm);
            IntPtr hdc = g.GetHdc();

            // Print WebBrowser snapshot to hdc
            bool result = PrintWindow(webBrs.Handle, hdc, 0);

            g.ReleaseHdc(hdc);
            g.Flush();


            // Save the bitmap, if successful
            if (result == true)
                bm.Save("reflash.png");
            bm.Dispose();
            g.Dispose();
            //CaptureImg("7.jpg", "xxx.png", test1);
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
                    hasSendPrice = false;
                    hasAmbushPrice = true;
                    textBox2.AppendText(DateTime.Now.ToString("[HH:mm:ss.fff]") + Environment.NewLine);
                    textBox2.AppendText("国拍网时间: " + CapTime.ToString("HH:mm:ss") + Environment.NewLine);
                    textBox2.AppendText("您已出价，请稍后..." + Environment.NewLine + Environment.NewLine);
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
