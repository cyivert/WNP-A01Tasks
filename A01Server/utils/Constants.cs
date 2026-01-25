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
        public const string IP_KEY = "serverIP";       //
        public const string PORT_KEY = "serverPort";   //
        public const string FILE_LIMIT_KEY = "maxFileSize"; //

        // Default Values if Config fails
        public const int DEFAULT_PORT = 5000;
        public const string DEFAULT_IP = "127.0.0.1";

        // Buffer and Timing
        public const int BUFFER_SIZE = 1024;
    }
}
