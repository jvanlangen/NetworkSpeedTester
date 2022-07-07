using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpeedTester
{
    public partial class FormSpeedTester : Form
    {
        private const double MessureSeconds = 5;
        private const int PORT = 43210;
        private const int BlockSize = 1024*4;
        private readonly CancellationTokenSource _tcs = new();
        private readonly Dictionary<IPEndPoint, ClientData> _clients = new();

        public FormSpeedTester()
        {
            InitializeComponent();

            StartReceiver();
        }



        public class ClientData
        {
            public long SequenceCounter { get; set; }
            public long MissedPackages { get; set; }

            public double BlocksPerSecond { get; set; }
            public List<DateTime> ReceivedTimeStamps { get; } = new();
        }


        private void StartReceiver()
        {
            Task.Run(() =>
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.EnableBroadcast = true;
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

                    var from = new IPEndPoint(0, 0);

                    while (!_tcs.IsCancellationRequested)
                    {
                        var recvBuffer = udpClient.Receive(ref from);
                        if (recvBuffer == null)
                            break;

                        if (recvBuffer.Length < 4)
                            continue;

                        var now = DateTime.UtcNow;

                        var sequence = BitConverter.ToInt32(recvBuffer);

                        lock (_clients)
                        {
                            if (!_clients.TryGetValue(from, out var clientData))
                                _clients.Add(from, clientData = new());

                            var delta = sequence - (clientData.SequenceCounter + 1);

                            clientData.SequenceCounter += delta;

                            while (true)
                            {
                                var first = clientData.ReceivedTimeStamps.FirstOrDefault();
                                if (first == default)
                                    break;

                                if (first.AddSeconds(MessureSeconds) >= now)
                                    break;

                                clientData.ReceivedTimeStamps.RemoveAt(0);
                            }
                            clientData.ReceivedTimeStamps.Add(now);

                            clientData.BlocksPerSecond = clientData.ReceivedTimeStamps.Count / MessureSeconds;
                        }
                    }
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var text = new StringBuilder();
            lock (_clients)
            {
                foreach (var item in _clients)
                {
                    text.AppendLine(FormatNumber((long)item.Value.BlocksPerSecond * BlockSize * 8));
                    text.AppendLine(item.Value.MissedPackages.ToString() + " packages missed");
                }
            }
            label1.Text = text.ToString();
        }

        private string FormatNumber(long v)
        {
            if (v > 1024 * 1024 * 10)
                return (v / (1024 * 1024)).ToString("0.0") + " Mbit/s";
            else if (v > 1024 * 10)
                return (v / 1024).ToString("0.0") + " kbit/s";
            else return v.ToString("0") + " bit/s";
        }
    }
}