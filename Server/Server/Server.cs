using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
 
namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener Listener = new TcpListener(IPAddress.Any, 6667);
            List<TcpClient> Clients = new List<TcpClient>();
            List<String> XMit = new List<String>();
			List<String> Users = new List<String>();
			Byte[] Buffer;

            Listener.Start();
            while (true)
            {
                while (Listener.Pending())
                {
                    Clients.Add(Listener.AcceptTcpClient());
                }
                foreach (TcpClient Client in Clients)
                {
                    if (Client.Available > 0)
                    {
						Buffer = new Byte[Client.Available];

						NetworkStream ClientStream = Client.GetStream();
						ClientStream.Read(Buffer, 0, Buffer.Length);
						XMit.Add(System.Text.Encoding.UTF8.GetString(Buffer));
						XMit.Add("START OF WHO LIST 66671208");
						foreach(string Username in Users){XMit.Add("USERS >> " + Username);}
						String loginOrOut = System.Text.Encoding.UTF8.GetString(Buffer);

						if(loginOrOut.Contains("has logged"))
						{
							if(loginOrOut.Contains(":"))
							{
								return;
							}
							else {
								if(loginOrOut.Contains ("has logged off."))
								{
									Users.Remove(loginOrOut.Substring(0,loginOrOut.LastIndexOf("has logged")-1));
								}
								else {
								Users.Add(loginOrOut.Substring(0,loginOrOut.LastIndexOf("has logged")-1));
								}
							}
						}
						Console.WriteLine("Relaying: " + System.Text.Encoding.UTF8.GetString(Buffer));
                    }
				}
 
                XMit.RemoveAll((X) => String.IsNullOrWhiteSpace(X));
 
                foreach (TcpClient Client in Clients)
                {
                    try
                    {
                        NetworkStream ClientStream = Client.GetStream();
                        foreach (String Line in XMit)
                        {
                            Buffer = System.Text.Encoding.UTF8.GetBytes(Line);
                            ClientStream.Write(Buffer, 0, Buffer.Length);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine(String.Format("Unable to forward message to client @ {0}, network failure", Client.Client.RemoteEndPoint));
                    }
                    catch (ObjectDisposedException)
                    {
                        Console.WriteLine(String.Format("Unable to forward message to client @ {0}, stream closed", Client.Client.RemoteEndPoint));
                    }
                }

                XMit.Clear();
                Clients.RemoveAll((Client) => !Client.Connected);
                Thread.Sleep(5);
            }
        }
    }
}