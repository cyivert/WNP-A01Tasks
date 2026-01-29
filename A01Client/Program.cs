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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Configuration;
using A01Client.utils;

namespace A01Client
{
    internal class Program
    {
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

            // Performance tracker instance
            PerformanceTracker tracker = new PerformanceTracker();
            int messageCount = Constants.INITIAL_MESSAGE_COUNT; // default = 0

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Client: {clientLogicalID} started");
            Console.ResetColor();

            while (isRunning)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
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
                            string message = $"ID:{clientLogicalID} | Msg:{messageCount} | Latency:{elapsedMs}ms\n";
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
    }
}