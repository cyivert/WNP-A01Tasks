/*
* FILE : Constants.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* This class contains constant values used across the application to avoid magic strings as possible.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A01Server.utils
{
    internal class Constants
    {
        // Network Configuration Keys (from your App.config)
        public const string SERVER_IP = "serverIP";                     // App.config for server IP address
        public const string SERVER_PORT = "serverPort";                 // App.config for server port number
        public const string FILE_LIMIT = "maxFileSize";                 // App.config for maximum log file size to trigger graceful stop

        // Default Values if Config fails
        public const int DEFAULT_PORT = 5000;                           // Default port number
        public const string DEFAULT_IP = "127.0.0.1";                   // Default IP address

        // Network Logic
        public const int BUFFER_SIZE = 1024;                            // Size of the buffer for data transmission 1024 bytes (1 KB)
        public const int DISCONNECT_SIGNAL = 0;                         // Signal for client disconnection
        public const int BUFFER_OFFSET = 0;                             // Buffer offset for data reading

        // Task Delays
        public const int MAIN_LOOP_DELAY = 100;                         // Main loop delay in milliseconds to prevent CPU overuse (100 ms)

        // Log File
        public const string LOG_FILE_NAME = "log.txt";                  // Log file name
    }
}
