/*
* FILE : Program.cs (CLIENT)
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca, Toro-Abasi Udon, Ritik Sanjiv Vyas
* DESCRIPTION :
* This file contains the main client program that connects to a server, asynchronously sends messages,
* measures transmission time, and handles graceful shutdown upon server unavailability.
*/

// REFERENCE //
/* 
 * Microsoft. (n/a). Console.ForegroundColor property. Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/api/system.console.foregroundcolor
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using A01Client.utils;

namespace A01Client
{
    internal class Program
    {
        private static int totalMessages = 0;                           // Counter: Total messages sent across all threads
        private static readonly object lockObject = new object();       // Lock object for thread-safe counter increment
        private static double totalLatencyMs = 0;                        // Counter: Total latency in milliseconds
        private static readonly object latencyLock = new object();      // Lock object for thread-safe latency increment
        private static bool isRunning = true;                           // Flag: Client running status
        static async Task Main(string[] args)
        {
            // Client identification
            string clientLogicalID;

            // check for command line argument for client ID
            if (args.Length > 0)
            {
                clientLogicalID = args[0];
            }
            else
            {
                clientLogicalID = Constants.CLIENT_DEFAULT_ID; // default = 1
            }

            string serverIP = ConfigurationManager.AppSettings["ServerIP"];                             // Server IP from config
            int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);                 // Server Port from config
            int clientThreads = int.Parse(ConfigurationManager.AppSettings["clientThreads"]);           // Number of client threads from config

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Client:{clientLogicalID} starting {clientThreads} threads...");
            Console.ResetColor();

            // List to hold client tasks
            List<Task> clientTasks = new List<Task>();

            // Start total performance timer
            Stopwatch totalTimer = new Stopwatch(); // add to performance tracker later

            // Start total timer
            totalTimer.Start();

            // Create multiple client threads
            for (int i = 0; i < clientThreads; i++)
            {
                // Unique thread identifier
                string clienThreadId = $"[{DateTime.Now:HH:mm:ss.fff}] Client:{clientLogicalID} | Thread:{i + 1} |";

                // Start client thread task and add to list using lambda to pass parameters to async method
                // Each thread runs RunClientThread method
                clientTasks.Add(Task.Run(async () => {await RunClientThread(serverIP, serverPort, clienThreadId);}));
            }

            // Wait for all threads to complete
            await Task.WhenAll(clientTasks);
            totalTimer.Stop();

            // Display performance summary
            Console.WriteLine("\n========================================");
            Console.WriteLine("CLIENT PERFORMANCE SUMMARY");
            Console.WriteLine("========================================");
            Console.WriteLine($"Client ID: {clientLogicalID}");
            Console.WriteLine($"Total Threads: {clientThreads}");
            Console.WriteLine($"Total Messages Sent: {totalMessages}");
            Console.WriteLine($"Total Time: {totalTimer.ElapsedMilliseconds} ms ({totalTimer.Elapsed.TotalSeconds:F2} seconds)");

            if (totalTimer.ElapsedMilliseconds > 0)
            {
                double messagesPerSecond = totalMessages / (totalTimer.ElapsedMilliseconds / 1000.0);
                Console.WriteLine($"Average Messages per Second: {messagesPerSecond:F2}");
                if (totalMessages > 0)
                {
                    double averageLatency = totalLatencyMs / (double)totalMessages;
                    Console.WriteLine($"Average Latency per Message: {averageLatency:F2} ms");
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine("All client threads exited gracefully.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return;
        }

        //
        // FUNCTION : RunClientThread
        // DESCRIPTION : Runs an individual client thread
        // PARAMETERS : 
        // string serverIp - Server IP address
        // int serverPort - Server port number
        // string clienThreadId - Unique thread identifier
        // RETURNS : Task - Async task
        //
        private static async Task RunClientThread(string serverIp, int serverPort, string clientThreadId)
        {
            int threadMessageCount = 0;

            Console.WriteLine($"{clientThreadId} Thread started.");

            while (isRunning)
            {

                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        // 1 Create a new TcpClient for each connection
                        PerformanceTracker threadTracker = new PerformanceTracker();

                        // 2 Start tracking time before connection: enables latency measurement more accurately
                        threadTracker.StartTracking();

                        //3  Connect to the server asynchronously
                        await client.ConnectAsync(serverIp, serverPort);

                        // 4 Get elapsed time since tracking started
                        double elapsedMs = threadTracker.GetElapsedMs();

                        // Get the network stream to send data to the server
                        using (NetworkStream stream = client.GetStream())
                        {
                            threadMessageCount++;

                            // lock and increment total message count
                            lock (lockObject)
                            {
                                totalMessages++;
                            }

                            // lock and accumulate total latency
                            lock (latencyLock)
                            {
                                totalLatencyMs += elapsedMs;
                            }

                            // paylaod message to server logs
                            string message = 
                                $"{clientThreadId} " +
                                $"Latency:{elapsedMs:F2} ms | " +
                                $"Messages:{threadMessageCount} | " +
                                $"Total Msgs:{totalMessages}\n";

                            byte[] data = Encoding.ASCII.GetBytes(message);

                            // Send data asynchronously
                            await stream.WriteAsync(data, 0, data.Length);

                            // Display message sent info to client console
                            Console.WriteLine(
                                $"{clientThreadId} " +
                                $"" + $"Latency:{elapsedMs:F2} ms | " + 
                                $"Messages:{threadMessageCount} | " + 
                                $"Total Messages:{totalMessages}" );

                            await stream.FlushAsync();

                            // Read server response (for shutdown signal)
                            byte[] responseBuffer = new byte[Constants.BUFFER_SIZE];
                            int responseBytes = await stream.ReadAsync(responseBuffer, Constants.BUFFER_OFFSET, responseBuffer.Length);
                            if (responseBytes > 0)
                            {
                                string response = Encoding.ASCII.GetString(responseBuffer, Constants.BUFFER_OFFSET, responseBytes).Trim();
                                if (response.Contains("SERVER_SHUTDOWN"))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"{clientThreadId} Received shutdown signal from server. Stopping...");
                                    Console.ResetColor();
                                    isRunning = false;
                                }
                            }
                        }
                    }

                    // Small delay between sends
                    await Task.Delay(300);
                }
                catch (SocketException)
                {
                    // Server is no longer accepting connections (graceful shutdown)
                    Console.WriteLine($"{clientThreadId} Server unavailable. Stopping thread.");
                    isRunning = false;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{clientThreadId} Error: {ex.Message}");
                    isRunning = false;
                    break;
                }
            }

            Console.WriteLine($"{clientThreadId} Thread completed. Sent {threadMessageCount} messages.");

            return;
        }
    }
}