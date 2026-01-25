/*
* FILE : Program.cs (SERVER)
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca 
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using A01Server.utils;

namespace A01Server
{
    internal class Program
    {
        private static bool isRunning = true; // Flag: Server running status
        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings[Constants.SERVER_IP];            // Read server IP from config file
            int port = int.Parse(ConfigurationManager.AppSettings[Constants.SERVER_PORT]);      // Read server port from config file

            IPAddress hostAddress = IPAddress.Parse(ipString);                                  // Parse the IP address string to an IPAddress object named hostAddress
            TcpListener server = new TcpListener(hostAddress, port);                            // Create a TCP listener named server
            LogManager logger = new LogManager();                                               // Create a log manager instance named logger

            try
            {
                server.Start();                                                                 // Start the server
                Console.WriteLine($"Server started on {hostAddress}:{port}");

                while (isRunning)
                {
                    TcpClient client = server.AcceptTcpClient();                                // Accept incoming client connection
                    Console.WriteLine("Client connected.");
                    Task.Run(() => HandleClientAsync(client, logger));                          // Async handle client connection
                }
                Task.Delay(Constants.MAIN_LOOP_DELAY).Wait();                                   // Delay to prevent CPU overuse while waiting for clients

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
            }
            finally
            {
                server.Stop();                                                                   // Stop the server
                Console.WriteLine("Server stopped.");
            }
        }

        //
        // FUNCTION : HandleClientAsync
        // DESCRIPTION : Receives data from a client and uses LogManager to save it. (as planned)
        // PARAMETERS : 
        // TcpClient client : The connected client.
        // LogManager logger : The log manager instance.
        // RETURNS :
        // to be determined
        //
        private static async Task HandleClientAsync(TcpClient client, LogManager logger)
        {
            NetworkStream stream = client.GetStream();                                            // Get network stream from client
            byte[] buffer = new byte[Constants.BUFFER_SIZE];                                      // Buffer for incoming data
            int bytesRead = Constants.DISCONNECT_SIGNAL;                                          // Number of bytes read (initialized to 0)

            try
            {
                bytesRead = await stream.ReadAsync(buffer, Constants.BUFFER_OFFSET, buffer.Length);                     // Read data from client
                if (bytesRead > Constants.DISCONNECT_SIGNAL)
                {
                    string message = Encoding.ASCII.GetString(buffer, Constants.BUFFER_OFFSET, bytesRead);              // ASCII > UTF-8 for choice for the project as UTF-8 supports emojis
                    bool limitReached = await logger.WriteLogAsync(message);                                            // Write log using LogManager

                    // Check if file size limit reached
                    if (limitReached)
                    {
                        Console.WriteLine("Log file reached. Stopping...");
                        isRunning = false; // Stop server if limit reached
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                stream.Close();
                client.Close();
                Console.WriteLine("Client disconnected.");
            }

            return;
        }
    }
}
