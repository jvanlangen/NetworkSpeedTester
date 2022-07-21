// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

await Task.Delay(1000);

const int BufferSize = 1024 * 64;

object SyncRoot = new();

Console.WriteLine("TCPStreamer Client");

Socket clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

var host = Environment.GetCommandLineArgs()[1];

clientSocket.Connect(host, 20301);

long dataSent = 0;

Random rnd = new();

Stopwatch? sw = null;


string FormatNumber(long v)
{
    if (v > 1024 * 1024 * 10)
        return (v / (1024 * 1024)).ToString("0.0") + " Mbit/s";
    else if (v > 1024 * 10)
        return (v / 1024).ToString("0.0") + " kbit/s";
    else return v.ToString("0") + " bit/s";
}



Task.Run(async () =>
{
    List<long> values = new();

    while (true)
    {
        await Task.Delay(1000);

        long rate;

        lock (SyncRoot)
        {
            if (sw == null)
                continue;

            rate = (long)(dataSent / (sw.ElapsedMilliseconds / 1000.0));
        }

        Console.WriteLine($"Data rate {FormatNumber(rate)}");
        values.Add(rate);

        if (values.Count > 10)
        {
            Console.WriteLine("");
            Console.WriteLine($"Average of rate is {FormatNumber((long)values.Average())}");
            Environment.Exit(0);
        }


    }
});

Task.Run(async () =>
{
    await Task.Delay(10000);
    lock (SyncRoot)
    {
        sw = Stopwatch.StartNew();
        dataSent = 0;
    }
});


var clientBuffer = new byte[BufferSize];

for (int i = 0; i < clientBuffer.Length; i++)
    clientBuffer[i] = (byte)rnd.Next();

while (true)
{
    var sent = clientSocket.Send(clientBuffer);

    lock (SyncRoot)
        dataSent += sent;
}