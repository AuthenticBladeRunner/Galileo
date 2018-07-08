﻿using System;
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
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;


namespace Captain
{
    public partial class CaptainForm : Form
    {
        public CaptainForm()
        {
            InitializeComponent();
        }

        private void CaptainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(global.paramFilePath))
            {
                DataTable paramTable = ExcelToDataTable(global.paramFilePath, true);
                tbAddTestTimeCol(paramTable, 1, 0.1);     //左右1秒，间隔0.1秒进行测试
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
        private DataTable tbAddTestTimeCol(DataTable dt, int dev, double interval)
        {
            DataTable newTable = dt;
            int rowNum = dt.Rows.Count;
            newTable.Columns.Add("测试偏离时间");
            double tickTime = -1 * dev + interval;
            for (int i = 0; i < rowNum; i++)
            {
                if (tickTime >= dev)
                {
                    tickTime = -dev + interval;
                }
                newTable.Rows[i]["测试偏离时间"] = tickTime;
                tickTime = Math.Round(tickTime + interval, 1);
            }
            DataToExcel(newTable, "temp");
            return newTable;

        }

        // https://stackoverflow.com/questions/9612389/how-to-scan-for-a-port-waiting-for-a-connection-on-a-network
        // https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast
        // https://stackoverflow.com/questions/40616911/c-sharp-udp-broadcast-and-receive-example
        private void listen()
        {
            int PORT = 8850;
            UdpClient udpCli = new UdpClient(PORT);
            // udpCli.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            var from = new IPEndPoint(IPAddress.Any, 0);
            var rspData = Encoding.UTF8.GetBytes("Hey Junior");
            Task.Run(() =>
            {
                while (true)
                {
                    var recvBuffer = udpCli.Receive(ref from);
                    var recvString = Encoding.UTF8.GetString(recvBuffer);
                    Console.WriteLine("Received {0} from {1}:{2}, sending response", recvString, from.Address.ToString(), from.Port);
                    udpCli.Send(rspData, rspData.Length, from);
                }
            });

            
        }


        /// <summary>  
        /// 将excel导入到datatable  
        /// </summary>  
        /// <param name="filePath">excel路径</param>  
        /// <param name="isColumnName">第一行是否是列名</param>  
        /// <returns>返回datatable</returns>  
        public static DataTable ExcelToDataTable(string filePath, bool isColumnName)
        {
            DataTable dataTable = null;
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
                        dataTable = new DataTable();
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
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        column = new DataColumn("column" + (i + 1));
                                        dataTable.Columns.Add(column);
                                    }
                                }

                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {
                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
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
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                return null;
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

    }
}
