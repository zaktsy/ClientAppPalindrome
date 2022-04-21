using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClientApp
{
    public class PalindromeChecker
    {
        const int port = 7000;
        const string ipAddress = "127.0.0.1";

        public async Task CheckPalindromeCandidate(PalindromeСandidate pal)
        {
            bool check = false;

            while (!check)
            {
                try
                {
                    TcpClient client = new TcpClient(ipAddress, port);
                    string jsonString = JsonSerializer.Serialize(pal);
                    byte[] request = Encoding.Unicode.GetBytes(jsonString);
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(request, 0, request.Length);


                    var responce = new byte[1024];
                    await stream.ReadAsync(responce, 0, responce.Length);
                    string resp = Encoding.Unicode.GetString(responce);
                    resp = resp.Replace("\0", "");
                    pal = JsonSerializer.Deserialize<PalindromeСandidate>(resp);

                    Status status = (Status)Enum.Parse(typeof(Status), pal.Status);
                    switch (status)
                    {
                        case Status.Yes:
                            check = true;
                            pal.Status = "Yes";
                            break;
                        case Status.No:
                            check = true;
                            pal.Status = "No";
                            break;
                        case Status.Processing:
                            check = false;
                            pal.Status = "Ожидание...";
                            break;
                        case Status.Queue:
                            check = false;
                            pal.Status = "В очереди";
                            Thread.Sleep(1000);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
