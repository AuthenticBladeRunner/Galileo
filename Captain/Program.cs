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
        public const string paramFilePath = @"D:\Visual Studio 2017\workspace\Galileo\Captain\bin\Debug\param\策略组模版.xlsx";       //参数文件路径


    }
}
