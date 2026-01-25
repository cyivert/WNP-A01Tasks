/*
* FILE : Program.cs
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
                    // Task.Run(() => HandleClientAsync(client, logger));                       // Async handle client connection
                }

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
            // code here...
        }
    }
}
