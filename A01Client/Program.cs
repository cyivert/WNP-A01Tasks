/*
* FILE : Program.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace A01Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Server connection details (hardcoded to avoid config issues)
            string serverIP = "127.0.0.1";
            int serverPort = 5000;

            PerformanceTracker tracker = new PerformanceTracker();

            try
            {
                // Connect to the server
                TcpClient client = new TcpClient();
                await client.ConnectAsync(serverIP, serverPort);
                Console.WriteLine("Connected to server.");

                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                using (StreamReader reader = new StreamReader(stream))
                {
                    int messageCount = 0;
                    bool running = true;

                    while (running)
                    {
                        messageCount++;

                        // Start tracking performance
                        tracker.StartTracking();

                        // Create a log message
                        string message = $"Client log message #{messageCount}";

                        // Send message to server asynchronously
                        await writer.WriteLineAsync(message);

                        // Wait for server response
                        string response = await reader.ReadLineAsync();

                        // Stop performance tracking
                        long elapsedMs = tracker.GetElapsedMs();

                        // Display performance result
                        Console.WriteLine(
                            $"Sent: {message} | Server Response: {response} | Time: {elapsedMs} ms"
                        );

                        // Stop if server requests shutdown
                        if (response == "STOP")
                        {
                            Console.WriteLine("Server requested shutdown. Client stopping.");
                            running = false;
                        }

                        // Slow down message sending slightly
                        await Task.Delay(300);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client Error: " + ex.Message);
            }

            Console.WriteLine("Client exited gracefully.");
            Console.ReadKey();
        }
    }
}
