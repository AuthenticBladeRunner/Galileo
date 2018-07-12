using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Collections;
using Newtonsoft.Json;

namespace Captain
{
    public partial class CaptainForm : Form
    {
        //测试时间的Tick,已往左偏离1秒方便计算
        private DateTime[] testTickArr= { DateTime.Parse("11:27:04"), DateTime.Parse("11:28:04"), DateTime.Parse("11:29:04"), DateTime.Parse("11:29:19"), DateTime.Parse("11:29:29"), DateTime.Parse("11:29:39") };
        private int dev = 1;  //测试左右偏离多少秒
        private double testTickIntval=0.1;  //间隔多少秒
        private const int captainPort = 8850;           // 总控用于接收消息的端口号
        private const int juniorPort = 8849;            // 下属用于接收消息的端口号

        // 新建UdpClient并绑定端口，用于发送和接收UDP消息
        private UdpClient udpCli = new UdpClient(captainPort);
        // 用于给属下广播的地址
        private IPEndPoint brdcsEp = new IPEndPoint(IPAddress.Broadcast, juniorPort);

        private DateTime fastestTime = DateTime.Today;  // 从下属拿到的最快的时间
        private int fastestPrice = 0;                   // 从下属拿到的最快的价格

        private DataTable paramTable = new DataTable();        // 配置表

        public CaptainForm()
        {
            InitializeComponent();
        }

        private void CaptainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(global.paramFilePath))
            {
                ExcelToDataTable(global.paramFilePath, true);
                tbAddTestTimeCol(paramTable, dev, testTickIntval);     //左右1秒，间隔0.1秒进行测试
                //MessageBox.Show(temp.Rows[1][1].ToString());
                ////MessageBox.Show((temp.Rows.Count).ToString());
                //DataRow[] drs3 = paramTable.Select("组号 = 'A1'");
                //MessageBox.Show(drs3.Length.ToString());
                //textBox1.Text = drs3[0]["最迟提交时间"].ToString();

            }
            else
            {
                MessageBox.Show("无法读取参数文件，请联系工作人员");
                //System.Environment.Exit(0);
            }
            listen();
        }

        /*table加一列记录每台机器测试时的偏离值，以防止集中测试会造成风险
         * param dev:左右偏离多少秒
         * param interval:间隔多少秒
         */
        private DataTable tbAddTestTimeCol(DataTable dt, int dev, double testTickIntval)
        {
            //DataTable newTable = dt;
            int rowNum = dt.Rows.Count;
            dt.Columns.Add("节点", typeof(IPEndPoint));
            dt.Columns.Add("测试顺序", typeof(int)); 
            int seq = 1;
            for (int i = 0; i < rowNum; i++)
            {
                if (seq > (int)(2 * dev / testTickIntval))
                {
                    seq = 1;
                }
                dt.Rows[i]["测试顺序"] = seq;
                seq++;
            }


            //double tickTime = 0;
            //for (int i = 0; i < rowNum; i++)
            //{
            //    if (tickTime > 2 * dev)
            //    {
            //        tickTime = 0;
            //    }
            //    newTable.Rows[i]["测试偏离时间"] = tickTime;
            //    tickTime = Math.Round(tickTime + interval, 1);
            //}
            DataToExcel(dt, "temp");
            return dt;

        }

        // https://stackoverflow.com/questions/9612389/how-to-scan-for-a-port-waiting-for-a-connection-on-a-network
        // https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast
        // https://stackoverflow.com/questions/40616911/c-sharp-udp-broadcast-and-receive-example
        private void listen()
        {
            // udpCli.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            // https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient.beginreceive(v=vs.110).aspx
            udpCli.BeginReceive(new AsyncCallback(gotUdpMsg), null);

            //Task.Run( () => {} );
        }

        // UdpClient 接收到消息后的回调函数
        // https://stackoverflow.com/questions/7266101/receive-messages-continuously-using-udpclient
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

            switch (msgType)
            {
                case "LookForCaptain":      // 「寻找总控」
                    // 回复「我是总控」
                    var rspData = Encoding.UTF8.GetBytes("IAmCaptain");
                    udpCli.Send(rspData, rspData.Length, remoteEp);
                    break;
                case "ReqLogin":            // 用户登录请求
                    userLogin(msgCont, remoteEp);
                    break;
                case "MyData":              // 属下上报的价格和时间
                    updTimeAndPrice(msgCont);
                    break;
            }
        }

        private void userLogin(string userId, IPEndPoint remoteEp)
        {
            // https://msdn.microsoft.com/en-us/library/det4aw50(v=vs.110).aspx
            DataRow[] foundRows = paramTable.Select("手机号 = '" + userId + "'");
            int res = 0;
            if (foundRows.Length > 0)
            {
                DataRow row = foundRows[0];
                // https://www.newtonsoft.com/json/help/html/SerializeDictionary.htm
                // 需要先把DataRow转为Dictionary, 因为对于普通Object, JsonConvert仅转换其property
                // 另外, 转换为JSON需要在给「节点」赋值前进行, 因为json.net无法把IPEndPoint转为字符串
                var rowDict = row.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => row[col.ColumnName]);
                string json = JsonConvert.SerializeObject(rowDict);
                byte[] jsBin = Encoding.UTF8.GetBytes("MyParam: " + json);
                udpCli.Send(jsBin, jsBin.Length, remoteEp);

                row["登陆"] = "是";
                row["节点"] = remoteEp;
            }
            else
            {
                res = -1;
            }

            byte[] bin = Encoding.UTF8.GetBytes("LoginResult: " + res);
            udpCli.Send(bin, bin.Length, remoteEp);
        }

        private void updTimeAndPrice(string cont)
        {
            string[] comp = cont.Split(';');
            if (comp.Length < 2)
                return;
            
            DateTime jrTime = DateTime.Today;
            int jrPrice = 0;

            if (DateTime.TryParse(comp[0], out jrTime))
            {
                if (jrTime > fastestTime)
                {
                    fastestTime = jrTime;   // 是否需要加lock?
                    if (int.TryParse(comp[1], out jrPrice))
                    {
                        fastestPrice = jrPrice;
                        // 只有当时间和价格都更新时, 才发送最快时间信息
                        byte[] bin = Encoding.UTF8.GetBytes("FastestData: " + 
                            fastestTime.ToString("HH:mm:ss") + ";" + fastestPrice);
                        lock (udpCli)
                        {
                            udpCli.Send(bin, bin.Length, brdcsEp);
                        }
                    }
                    //当测试时间到时推送消息
                    //MessageBox.Show(fastestTime.ToString() + "  " + testTick.ToString());
                    int id = Array.IndexOf(testTickArr, fastestTime);
                    if (id!=-1)
                    {
                        //MessageBox.Show(fastestTime.ToString());
                        //new Task(sendTestInstr).Start();
                        Thread threadSendTestInstr = new Thread(new ThreadStart(sendTestInstr));
                        //调用Start方法执行线程
                        threadSendTestInstr.Start();

                        //Task.Run(() => {
                        //    int sendSeq = 1; //发送的顺序
                        //    for(int i= sendSeq; i<= 2 * dev / testTickIntval; sendSeq++)
                        //    {
                        //        DataRow[] foundRows = paramTable.Select("测试顺序 = '" + sendSeq + "'");
                        //        if (foundRows.Length > 0)
                        //        {
                        //            for (int t = 0; t < foundRows.Length; t++)
                        //            {
                        //                byte[] binSendTest = Encoding.UTF8.GetBytes("initTest");
                        //                if ((foundRows[t]["节点"]).GetType() is IPEndPoint)
                        //                {
                        //                    udpCli.Send(binSendTest, binSendTest.Length, (IPEndPoint)foundRows[t]["节点"]);
                        //                    MessageBox.Show("111");
                        //                }
                        //            }

                        //        }
                        //        Thread.Sleep((int)(testTickIntval*50000));
                        //    }



                        //});
                        //byte[] bin = Encoding.UTF8.GetBytes("initTest");
                        //udpCli.Send(bin, bin.Length, brdcsEp);
                    }
                }
            }
        }

        private void sendTestInstr()
        {
            int sendSeq = 1; //发送的顺序
            for (int i = sendSeq; i <= 2 * dev / testTickIntval; sendSeq++)
            {
                DataRow[] foundRows = paramTable.Select("测试顺序 = '" + sendSeq + "'");
                if (foundRows.Length > 0)
                {
                    for (int t = 0; t < foundRows.Length; t++)
                    {
                        byte[] binSendTest = Encoding.UTF8.GetBytes("initTest");
                        //DataToExcel(paramTable, "temp2");
                        if (!foundRows[t].IsNull("节点"))
                        {
                            lock (udpCli)
                            {
                                udpCli.Send(binSendTest, binSendTest.Length, (IPEndPoint)foundRows[t]["节点"]);
                            }
                        }
                    }

                }
                Thread.Sleep((int)(testTickIntval * 1000));
            }
        }

        /// <summary>  
        /// 将excel导入到datatable  
        /// </summary>  
        /// <param name="filePath">excel路径</param>  
        /// <param name="isColumnName">第一行是否是列名</param>  
        /// <returns>返回datatable</returns>  
        private void ExcelToDataTable(string filePath, bool isColumnName)
        {
            FileStream fs = null;
            DataColumn column = null;
            DataRow dataRow = null;
            IWorkbook workbook = null;
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            int startRow = 0;
            try
            {
                using (fs = File.OpenRead(filePath))
                {
                    // 2007版本  
                    if (filePath.IndexOf(".xlsx") > 0)
                        workbook = new XSSFWorkbook(fs);
                    // 2003版本  
                    else if (filePath.IndexOf(".xls") > 0)
                        workbook = new HSSFWorkbook(fs);

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheetAt(0);//读取第一个sheet，当然也可以循环读取每个sheet  
                        //configTable = new DataTable();
                        if (sheet != null)
                        {
                            int rowCount = sheet.LastRowNum;//总行数  
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(0);//第一行  
                                int cellCount = firstRow.LastCellNum;//列数  

                                //构建datatable的列  
                                if (isColumnName)
                                {
                                    startRow = 1;//如果第一行是列名，则从第二行开始读取  
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                paramTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        paramTable.Columns.Add(column);
                                    }
                                }

                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = paramTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        if (cell == null)
                                        {
                                            dataRow[j] = "";
                                        }
                                        else
                                        {
                                            //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                            switch (cell.CellType)
                                            {
                                                case CellType.Blank:
                                                    dataRow[j] = "";
                                                    break;
                                                case CellType.Numeric:
                                                    short format = cell.CellStyle.DataFormat;
                                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        dataRow[j] = cell.DateCellValue;
                                                    else
                                                        dataRow[j] = cell.NumericCellValue;
                                                    break;
                                                case CellType.String:
                                                    dataRow[j] = cell.StringCellValue;
                                                    break;
                                            }
                                        }
                                    }
                                    paramTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                //return configTable;
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                //return null;
            }
        }

        /// <summary>
        /// Datatable生成Excel表格并返回路径
        /// </summary>
        /// <param name="m_DataTable">Datatable</param>
        /// <param name="s_FileName">文件名</param>
        /// <returns></returns>
        public string DataToExcel(System.Data.DataTable m_DataTable, string s_FileName)
        {
            string FileName = AppDomain.CurrentDomain.BaseDirectory + s_FileName + ".xls";  //文件存放路径
            if (System.IO.File.Exists(FileName))                                //存在则删除
            {
                System.IO.File.Delete(FileName);
            }
            System.IO.FileStream objFileStream;
            System.IO.StreamWriter objStreamWriter;
            string strLine = "";
            objFileStream = new System.IO.FileStream(FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            objStreamWriter = new System.IO.StreamWriter(objFileStream, Encoding.Unicode);
            for (int i = 0; i < m_DataTable.Columns.Count; i++)
            {
                strLine = strLine + m_DataTable.Columns[i].Caption.ToString() + Convert.ToChar(9);      //写列标题
            }
            objStreamWriter.WriteLine(strLine);
            strLine = "";
            for (int i = 0; i < m_DataTable.Rows.Count; i++)
            {
                for (int j = 0; j < m_DataTable.Columns.Count; j++)
                {
                    if (m_DataTable.Rows[i].ItemArray[j] == null)
                        strLine = strLine + " " + Convert.ToChar(9);                                    //写内容
                    else
                    {
                        string rowstr = "";
                        rowstr = m_DataTable.Rows[i].ItemArray[j].ToString();
                        if (rowstr.IndexOf("\r\n") > 0)
                            rowstr = rowstr.Replace("\r\n", " ");
                        if (rowstr.IndexOf("\t") > 0)
                            rowstr = rowstr.Replace("\t", " ");
                        strLine = strLine + rowstr + Convert.ToChar(9);
                    }
                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";
            }
            objStreamWriter.Close();
            objFileStream.Close();
            return FileName;        //返回生成文件的绝对路径
        }

        private void CaptainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
