using System.Net.Sockets;

namespace NetworkSpeedTester.Sender
{
    public partial class Form1 : Form
    {
        private readonly CancellationTokenSource _tcs = new();
        private const int PORT = 43210;
        public Form1()
        {
            InitializeComponent();
            StartSending();
        }

        private void StartSending()
        {

            Task.Run(() =>
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    long counter = 0;
                    var data = new byte[1024];
                    while (!_tcs.IsCancellationRequested)
                    {
                        var bytes = BitConverter.GetBytes(counter);
                        Array.Copy(bytes, data, bytes.Length);
                        udpClient.Send(data, data.Length, "255.255.255.255", PORT);
                    }
                }
            });

        }
    }
}