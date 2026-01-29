/*
* FILE : Program.cs (SERVER)
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca 
* DESCRIPTION :
* This file contains the main server program that listens for client connections,
* receives messages asynchronously, logs them to a file, and handles graceful shutdown
* when the log file size limit is reached.
*/

// REFERENCE //
/* 
 * Microsoft. (n/a). Console.ForegroundColor property. Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/api/system.console.foregroundcolor
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using A01Server.utils;

namespace A01Server
{
    internal class Program
    {
        private static bool isRunning = true; // Flag: Server running status
        private static int clientCounter = 0; // Counter: Number of connected clients
        private static CancellationTokenSource cts = new CancellationTokenSource(); // Token source for graceful shutdown
        static async Task Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings[Constants.SERVER_IP];                            // Read server IP from config file
            int port = int.Parse(ConfigurationManager.AppSettings[Constants.SERVER_PORT]);                      // Read server port from config file

            IPAddress hostAddress = IPAddress.Parse(ipString);                                                  // Parse the IP address string to an IPAddress object named hostAddress
            TcpListener server = new TcpListener(hostAddress, port);                                            // Create a TCP listener named server
            LogManager logger = new LogManager();                                                               // Create a log manager instance named logger

            try
            {
                // Start the server
                server.Start();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Server started on {hostAddress}:{port}");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Waiting for clients...");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Log file: {Constants.LOG_FILE_NAME}");
                Console.ResetColor();

                while (isRunning)
                {
                    TcpClient client = server.AcceptTcpClient(); // Accept incoming client connection
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] CONNECTED: {client.Client.RemoteEndPoint.ToString()}");
                    Console.ResetColor();
                    await Task.Run(() => HandleClientAsync(client, logger, cts.Token)); // Async handle client connection
                }

                // Delay to prevent CPU overuse while waiting for clients
                Task.Delay(Constants.MAIN_LOOP_DELAY).Wait();

            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                string errorMsg = $"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.ResetColor();

                await logger.WriteLogAsync(errorMsg, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                cts.Dispose();                                                                  // Dispose cancellation token source
                server.Stop();                                                                  // Stop the server
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Server stopped.");
                Console.ResetColor();
            }
        }

        //
        // FUNCTION : HandleClientAsync
        // DESCRIPTION : Receives data from a client and uses LogManager to save it. (as planned)
        // PARAMETERS : 
        // TcpClient client - The connected client.
        // LogManager logger - The log manager instance.
        // RETURNS :
        // to be determined
        //
        private static async Task HandleClientAsync(TcpClient client, LogManager logger, CancellationToken token)
        {
            string serverId = Interlocked.Increment(ref clientCounter).ToString();                // Unique client ID
            string clientInfo = client.Client.RemoteEndPoint.ToString();      

            // Client endpoint info
            string rawMessage = string.Empty;
            string formattedMessage = string.Empty;

            try
            {
                using (NetworkStream stream = client.GetStream())                                                                                         // Get network stream
                {
                    byte[] buffer = new byte[Constants.BUFFER_SIZE];                                                                                      // Buffer for incoming data
                    int bytesRead = Constants.DISCONNECT_SIGNAL;
                    bytesRead = await stream.ReadAsync(buffer, Constants.BUFFER_OFFSET, buffer.Length);                                                   // Read data from client

                    if (bytesRead > Constants.DISCONNECT_SIGNAL)
                    {
                        rawMessage = Encoding.ASCII.GetString(buffer, Constants.BUFFER_OFFSET, bytesRead);                                                // ASCII > UTF-8 for choice for the project as UTF-8 supports emojis
                        formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] Received on server: {serverId} ({clientInfo}) | {rawMessage.Trim()}";          // log file message format
                        bool limitReached = await logger.WriteLogAsync(formattedMessage, token);                                                                 // Write log using LogManager

                        // Check if file size limit reached
                        if (limitReached)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] WARNING: File limit reached! Initiating graceful shutdown...");
                            Console.ResetColor();
                            isRunning = false; // Stop server if limit reached
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] PROCESSED: {clientInfo}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (OperationCanceledException) // Handle task cancellation
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SYSTEM: Task for {clientInfo} cancelled due to server shutdown.");
                Console.ResetColor();
            }
            catch (Exception ex) // General exception handling
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {clientInfo} | {ex.Message}");
                Console.ResetColor();
            }
            finally // Ensure client is closed
            {
                client.Close();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] DISCONNECTED: {clientInfo}\n");
                Console.ResetColor();
            }

            return;
        }
    }
}