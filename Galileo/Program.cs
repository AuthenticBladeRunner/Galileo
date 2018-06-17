using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Galileo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }

    //定义常量
    public static class global
    {
        //public const string hupaiUrl = "http://test.alltobid.com/moni/gerenlogin.html";  //国拍模拟拍卖主页
        //public const string layPriceUrl = "http://test.alltobid.com/moni/gerenbid.html"; //国拍模拟出价页面
        public const string hupaiUrl = "http://moni.51hupai.org/";                         //51沪牌模拟主页
        public const int scanInterval = 250;                                               //扫描间隔时间，单位ms
        public const string wholeShotImgPath = "img/wholeShotImg.jpg";                     //整屏截图的图像文件
        public const string timeImgPath = "img/time.png";                                  //时间图像文件
        public const string priceImgPath = "img/price.png";                                //价格图像文件
        public const string firstBidTick = "14:57:45";                                     //第一次出价时间
    }
}
