using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Captain
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
            Application.Run(new CaptainForm());
        }
    }

    //定义常量
    public static class global
    {
        public const string paramFilePath = "param\\ppConfig.xlsx";       //参数文件路径


    }
}
