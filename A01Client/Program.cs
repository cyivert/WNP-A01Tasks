/*
* FILE : Program.cs (CLIENT)
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca, Toro-Abasi Udon
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

            Console.WriteLine($"Client:{clientLogicalID} starting {clientThreads} threads...");

            // List to hold client tasks
            List<Task> clientTasks = new List<Task>();

            // Start total performance timer
            Stopwatch totalTimer = new Stopwatch(); // add to performance tracker later

            // Start total timer
            totalTimer.Start();

            // Create multiple client threads
            for (int i = 0; i < clientThreads; i++)
            {
                string threadId = $"Client:{clientLogicalID} | Thread:{i + 1}";                                            // Unique thread identifier
                clientTasks.Add(Task.Run(async () => {await RunClientThread(serverIP, serverPort, threadId);}));    // Start client thread
            }

            // Wait for all threads to complete
            await Task.WhenAll(clientTasks);
            totalTimer.Stop();

            // **DISABLED**
            // Display performance summary
            //Console.WriteLine("\n================================");
            //Console.WriteLine("PERFORMANCE SUMMARY");
            //Console.WriteLine("================================");
            //Console.WriteLine($"Total Threads: {clientThreads}");
            //Console.WriteLine($"Total Messages Sent: {totalMessages}");
            //Console.WriteLine($"Total Time: {totalTimer.ElapsedMilliseconds} ms");

            // Performance tracker instance
            PerformanceTracker tracker = new PerformanceTracker();
            int messageCount = Constants.INITIAL_MESSAGE_COUNT; // default = 0



            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Client: {clientLogicalID} started");
            Console.ResetColor();

            // Main client loop
            while (isRunning)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        // Start tracking time before connection: enables latency measurement more accurately
                        // as it tracks time taken to establish connection + send message
                        tracker.StartTracking();

                        // Connect to server
                        await client.ConnectAsync(serverIP, serverPort);

                        // Get elapsed time (in milliseconds)
                        long elapsedMs = tracker.GetElapsedMs();

                        // Get the network stream to send data to the server
                        using (NetworkStream stream = client.GetStream())
                        {
                            messageCount++;

                            // payload message sent to server logs
                            string message = $"ID:{clientLogicalID} | Msg:{messageCount} | \n";
                            byte[] data = Encoding.ASCII.GetBytes(message);

                            // Send data asynchronously
                            await stream.WriteAsync(data, 0, data.Length);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sent: {message.Trim()}");
                            Console.ResetColor();
                        }
                    }

                    // Small delay between sends
                    await Task.Delay(300);
                }
                catch (SocketException)
                {
                    // Server is no longer accepting connections (graceful shutdown)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Server unavailable. Shutting down client.");
                    Console.ResetColor();
                    isRunning = false;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Client error: {ex.Message}");
                    Console.ResetColor();
                    isRunning = false;
                }
            }


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Client exited gracefully.");
            Console.ResetColor();
            Console.ReadKey();
        }

        //
        // FUNCTION : RunClientThread
        // DESCRIPTION : Runs an individual client thread
        // PARAMETERS : 
        // string serverIp - Server IP address
        // int serverPort - Server port number
        // string threadId - Unique thread identifier
        // RETURNS : Task - Async task
        //
        private static async Task RunClientThread(string serverIp, int serverPort, string threadId)
        {
            int threadMessageCount = 0;
            PerformanceTracker threadTracker = new PerformanceTracker();

            Console.WriteLine($"[{threadId}] Thread started.");

            while (isRunning)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        // Connect to server
                        await client.ConnectAsync(serverIp, serverPort);

                        using (NetworkStream stream = client.GetStream())
                        {
                            threadMessageCount++;

                            string message = $"Client ID: {threadId}\n";
                            byte[] data = Encoding.ASCII.GetBytes(message);

                            // Start performance tracking
                            threadTracker.StartTracking();

                            // Send data asynchronously
                            await stream.WriteAsync(data, 0, data.Length);

                            long elapsedMs = threadTracker.GetElapsedMs();

                            // Update shared counters
                            lock (lockObject)
                            {
                                totalMessages++;
                            }

                            Console.WriteLine(
                                $"[{threadId}] " +
                                $"" + $"Latency: {elapsedMs} ms | " + 
                                $"Thread Messages: {threadMessageCount} | " + 
                                $"Total Messages: {totalMessages}" );
                        }
                    }

                    // Small delay between sends
                    await Task.Delay(300);
                }
                catch (SocketException)
                {
                    // Server is no longer accepting connections (graceful shutdown)
                    Console.WriteLine($"[{threadId}] Server unavailable. Stopping thread.");
                    isRunning = false;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{threadId}] Error: {ex.Message}");
                    isRunning = false;
                    break;
                }
            }

            Console.WriteLine($"[{threadId}] Thread completed. Sent {threadMessageCount} messages.");

            return;
        }

    }
}