/*
* FILE : Program.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca 
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace A01Server
{
    internal class Program
    {
        // Global flag to control server running state
        private static bool isRunning = true;
        private static LogManager logManager = new LogManager();
        private static TcpListener listener;

        static void Main(string[] args)
        {
            // Read server IP and port from App.config
            string ip = ConfigurationManager.AppSettings["ServerIP"];
            int port = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);

            // Initialize and start the TCP listener
            listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();
            Console.WriteLine($"Server started on {ip}:{port}");

            // Start accepting clients asynchronously
            Task.Run(() => AcceptClientsAsync());

            // Main thread waits for shutdown signal
            while (isRunning)
            {
                Thread.Sleep(500);
            }

            // Stop the listener and exit
            listener.Stop();
            Console.WriteLine("Server stopped gracefully.");
        }



        /// <summary>
        /// Handles communication with a connected client.
        /// </summary>
        /// <param name="client">
        /// The connected TcpClient instance.
        /// </param>
        /// <returns>
        /// Task representing the asynchronous operation.
        /// </returns>
        private static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream) { AutoFlush = true })
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null && isRunning)
                {
                    // Write received message to log file asynchronously
                    bool limitReached = await logManager.WriteLogAsync(line);

                    if (limitReached)
                    {
                        // If file size limit reached, notify client and trigger shutdown
                        isRunning = false;
                        await writer.WriteLineAsync("STOP");
                        break;
                    }
                    else
                    {
                        // Otherwise, acknowledge receipt
                        await writer.WriteLineAsync("OK");
                    }
                }
            }
        }
    }
}