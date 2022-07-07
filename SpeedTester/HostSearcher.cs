using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeedTester
{
    internal class HostSearcher : IDisposable
    {
        private readonly CancellationTokenSource _tcs = new();
        public HostSearcher()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 43210));


            Task.Run(() =>
            {
                var from = new IPEndPoint(0, 0);

                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    Trace.WriteLine(Encoding.UTF8.GetString(recvBuffer));
                }
            });
        }

        public void Dispose()
        {
            
        }
    }
}
