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
            Application.Run(new login());
        }
    }


    //定义常量
    public static class global
    {
        //public const string hupaiUrl = "http://test.alltobid.com/moni/gerenlogin.html";            //国拍模拟拍卖主页
        //public const string layPriceUrl = "http://test.alltobid.com/moni/gerenbid.html";           //国拍模拟出价页面
        public const string hupaiUrl = "http://moni.51hupai.org/";                                 //51沪牌模拟主页
        //public const string hupaiUrl = "https://paimai.alltobid.com";                                //
        //public const string hupaiUrl = "https://paimai2.alltobid.com/bid/2018062301/login.htm";    //国拍正式拍卖主页  
        public const int scanInterval = 300;                                                         //扫描间隔时间，单位ms
        public const string wholeShotImgPath = "img/wholeShotImg.jpg";                               //整屏截图的图像文件
        public const string timeImgPath = "img/time.png";                                            //时间图像文件
        public const string priceImgPath = "img/price.png";                                          //价格图像文件
        //public const string testTypeTick = "11:29:40";                                               //试打码时间
        public const int testAddPrice = 300;                                                         //测试价格在最低价位基础上加价多少
        //public const string layPriceTick = "11:29:50";                                               //正式出价时间
        public const int AddPrice = 700;                                                             //正式出价在最低价位基础上加价多少
        public const string setBDPriceTick = "11:29:52";                                             //设定标定价格时间
        public const int bdAddPrice = 300;                                                           //标定价格在最低价位基础上加价多少
    }
}
