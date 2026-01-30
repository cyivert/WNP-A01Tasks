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
        // Static variables for tracking across all threads
        private static int totalMessages = 0;
        private static readonly object lockObject = new object();
        private static bool isRunning = true; // Flag: Client running status

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

            List<Task> clientTasks = new List<Task>();
            Stopwatch totalTimer = new Stopwatch();
            totalTimer.Start();

            // Create multiple client threads
            for (int i = 0; i < clientThreads; i++)
            {
                string threadId = (i + 1).ToString(); 
                clientTasks.Add(Task.Run(async () =>
                {
                    await RunClientThread(serverIP, serverPort, clientLogicalID, threadId);
                }));
            }

            // Wait for all threads to complete
            await Task.WhenAll(clientTasks);
            totalTimer.Stop();

            // Display performance summary
            Console.WriteLine("\n========================================");
            Console.WriteLine("PERFORMANCE SUMMARY");
            Console.WriteLine("========================================");
            Console.WriteLine($"Client ID: {clientLogicalID}");
            Console.WriteLine($"Total Threads: {clientThreads}");
            Console.WriteLine($"Total Messages Sent: {totalMessages}");
            Console.WriteLine($"Total Time: {totalTimer.ElapsedMilliseconds} ms");

            if (totalTimer.ElapsedMilliseconds > 0)
            {
                double messagesPerSecond = totalMessages / (totalTimer.ElapsedMilliseconds / 1000.0);
                Console.WriteLine($"Average Messages per Second: {messagesPerSecond:F2}");
                double averageLatency = totalTimer.ElapsedMilliseconds / (double)totalMessages;
                Console.WriteLine($"Average Latency per Message: {averageLatency:F2} ms");
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
        // string clientId - Client identifier
        // string threadId - Thread identifier (just the number)
        // RETURNS : Task - Async task
        //
        private static async Task RunClientThread(string serverIp, int serverPort, string clientId, string threadId)
        {
            int threadMessageCount = 0;
            PerformanceTracker threadTracker = new PerformanceTracker();

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

                            // Start performance tracking for this message
                            Stopwatch messageTimer = Stopwatch.StartNew();

                            // Format: ID: {clientId} | Thread: {threadId} | Message: {message} | Total Message: {totalMessage} | Latency: {latency}ms
                            int currentTotal;
                            lock (lockObject)
                            {
                                totalMessages++;
                                currentTotal = totalMessages;
                            }

                            // Create the message to send
                            string message = $"Message#{threadMessageCount}";
                            string fullMessage = $"ID:{clientId}|Thread:{threadId}|Message:{message}|TotalMessage:{currentTotal}\n";
                            byte[] data = Encoding.ASCII.GetBytes(fullMessage);

                            // Send data asynchronously
                            await stream.WriteAsync(data, 0, data.Length);

                            messageTimer.Stop();
                            long latency = messageTimer.ElapsedMilliseconds;

                            Console.WriteLine(
                                $"ID: {clientId} | " +
                                $"Thread: {threadId} | " +
                                $"Message: {message} | " +
                                $"Total Message: {currentTotal} | " +
                                $"Latency: {latency}ms"
                            );
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
                    Console.WriteLine($"ID: {clientId} | Thread: {threadId} | Server unavailable. Stopping thread.");
                    Console.ResetColor();
                    isRunning = false;
                    break;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ID: {clientId} | Thread: {threadId} | Error: {ex.Message}");
                    Console.ResetColor();
                    isRunning = false;
                    break;
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ID: {clientId} | Thread: {threadId} | Thread completed. Sent {threadMessageCount} messages.");
            Console.ResetColor();

            return;
        }
    }
}