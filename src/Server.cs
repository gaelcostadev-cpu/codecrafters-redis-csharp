using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

List<Socket> clients = [server.AcceptSocket()];

// 1024 bytes pode ser qualquer valor, dependendo do tamanho da msg que você espera receber
byte[] msg = new byte[1024]; 

while (true)
{
    for (int i = 0; i < clients.Count; i++)
    {
        Socket client = clients[i];
        if (!client.Connected)
        {
            clients.RemoveAt(i);
            i--;
            continue;
        }
        client.Receive(msg);
        client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
    }
}