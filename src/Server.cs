using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

Socket client = server.AcceptSocket();
byte[] buffer = new byte[1024];

while (client.Connected) {
    client.Receive(buffer);
    client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
}  