// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;

const int BufferSize = 1024 * 64;

object SyncRoot = new();

Console.WriteLine("TCPStreamer Server");

Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

socket.Bind(new IPEndPoint(IPAddress.Any, 20301));

socket.Listen(0);

long dataReceived = 0;

Random rnd = new();

while (true)
{
    var clientSocket = socket.Accept();

    Task.Run(() =>
    {
        try
        {

            var clientBuffer = new byte[BufferSize];

            while (true)
            {
                var received = clientSocket.Receive(clientBuffer);

                lock (SyncRoot)
                    dataReceived += received;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    });
}