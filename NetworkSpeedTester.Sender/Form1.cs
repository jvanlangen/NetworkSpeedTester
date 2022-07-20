using System.Net.Sockets;

namespace NetworkSpeedTester.Sender
{
    public partial class Form1 : Form
    {
        private readonly CancellationTokenSource _tcs = new();
        private const int PORT = 43211;
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
                    udpClient.Client.EnableBroadcast = true;
                    long counter = 0;
                    var data = new byte[1024*16];
                    var rnd = new Random();

                    for (int i = 0; i < data.Length; i++)
                        data[i] = (byte)rnd.Next();

                    while (!_tcs.IsCancellationRequested)
                    {
                        var bytes = BitConverter.GetBytes(counter++);
                        Array.Copy(bytes, data, bytes.Length);
                        udpClient.Send(data, data.Length, "192.168.8.255", PORT);
                    }
                }
            });

        }
    }
}