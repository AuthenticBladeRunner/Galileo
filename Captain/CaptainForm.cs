using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


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
            listen();
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

    }
}
