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
        static void Main(string[] args)
        {
            string ipString = ConfigurationManager.AppSettings[Constants.SERVER_IP];            // Read server IP from config file
            int port = int.Parse(ConfigurationManager.AppSettings[Constants.SERVER_PORT]);      // Read server port from config file

            IPAddress hostAddress = IPAddress.Parse(ipString);                                  // Parse the IP address string to an IPAddress object named hostAddress
            TcpListener server = new TcpListener(hostAddress, port);                            // Create a TCP listener named server
            LogManager logger = new LogManager();                                               // Create a log manager instance named logger
        }
    }
}
