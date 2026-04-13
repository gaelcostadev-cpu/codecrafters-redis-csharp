using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

List<Socket> clients = [];

// 1024 bytes pode ser qualquer valor, dependendo do tamanho da msg que você espera receber
byte[] msg = new byte[1024]; 

while (true)
{
    //enquanto houver uma conexão pendente, aceita e adiciona a lista de clientes
    if (server.Pending()) clients.Add(server.AcceptSocket());
    
    for (int i = 0; i < clients.Count; i++)
    {
        Socket client = clients[i];

        //cliente desconectado, remove da lista
        if (!client.Connected)
        {
            clients.RemoveAt(i);
            i--;
            continue;
        }

        //determina se o cliente tem dados para ler, se sim, lê a mensagem e responde com +PONG
        if (client.Poll(0, SelectMode.SelectRead))
        {
            client.Receive(msg);
            client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
        }
    }
}