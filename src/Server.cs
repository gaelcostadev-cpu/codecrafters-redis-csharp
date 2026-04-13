using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

Socket client = server.AcceptSocket();

do {
    client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
} while (client.Connected);