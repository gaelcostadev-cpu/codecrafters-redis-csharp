using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

List<Socket> clients = [];
Dictionary<string, string> store = new();

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
            string request = Encoding.UTF8.GetString(msg, 0, Convert.ToInt32(client.Receive(msg)));

            //mensagens do tipo RESP tem a seguinte estrutura: *2\r\n$4\r\nECHO\r\n$3\r\nhey\r\n
            string[] parts = request.Split("\r\n");

            //o comando é a terceira parte da mensagem, por exemplo, ECHO ou PING
            string command = parts[2].ToUpper();

            if (command == "ECHO")
            {
                string argument = parts[4];
                string response = $"${argument.Length}\r\n{argument}\r\n";

                //responde com a mensagem do tipo RESP (bulk string)
                //por exemplo: $<length>\r\n<data>\r\n
                client.Send(Encoding.UTF8.GetBytes(response));
            }
            else if (command == "PING")
            {
                client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
            }
            else if (command == "SET")
            {
                string key = parts[4];
                string value = parts[6];

                store[key] = value;

                client.Send(Encoding.UTF8.GetBytes("+OK\r\n"));

                if (parts.Length > 6)
                {
                    string expireCommand = parts[8].ToUpper();
                    string expireValue = parts[10];

                    if (expireCommand == "PX")
                    {
                        int milliseconds = Convert.ToInt32(expireValue);
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(milliseconds * 1000);
                            store.Remove(key);
                        });
                    }
                }
            }
            else if (command == "GET")
            {
                string key = parts[4];

                if (store.TryGetValue(key, out string? value))
                {
                    string response = $"${value.Length}\r\n{value}\r\n";
                    client.Send(Encoding.UTF8.GetBytes(response));
                }
                else
                {
                    client.Send(Encoding.UTF8.GetBytes("$-1\r\n"));
                }
            }
        }
    }
}