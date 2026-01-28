/*
* FILE : Program.cs (CLIENT)
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Configuration;

namespace A01Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Client identification
            string clientLogicalID;

            if (args.Length > 0)
            {
                clientLogicalID = args[0];
            }
            else
            {
                clientLogicalID = "1";
            }

            string serverIP = ConfigurationManager.AppSettings["ServerIP"];
            int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);

            PerformanceTracker tracker = new PerformanceTracker();
            bool running = true;
            int messageCount = 0;

            Console.WriteLine($"Client:{clientLogicalID} started.");

            while (running)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        // Connect to server
                        await client.ConnectAsync(serverIP, serverPort);

                        using (NetworkStream stream = client.GetStream())
                        {
                            messageCount++;

                            string message = $"Client message #{messageCount}\n";
                            byte[] data = Encoding.ASCII.GetBytes(message);

                            // Start performance tracking
                            tracker.StartTracking();

                            // Send data asynchronously
                            await stream.WriteAsync(data, 0, data.Length);

                            long elapsedMs = tracker.GetElapsedMs();

                            Console.WriteLine(
                                $"Sent: {message.Trim()} | Transmission Time: {elapsedMs} ms"
                            );
                        }
                    }

                    // Small delay between sends
                    await Task.Delay(300);
                }
                catch (SocketException)
                {
                    // Server is no longer accepting connections (graceful shutdown)
                    Console.WriteLine("Server unavailable. Shutting down client.");
                    running = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                    running = false;
                }
            }

            Console.WriteLine("Client exited gracefully.");
            Console.ReadKey();
        }
    }
}