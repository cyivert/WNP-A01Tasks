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

namespace A01Client.utils
{
    internal class Constants
    {
        // Network Logic
        public const int BUFFER_SIZE = 1024;                            // Size of the buffer for data transmission 1024 bytes (1 KB)
        public const int DISCONNECT_SIGNAL = 0;                         // Signal for client disconnection
        public const int BUFFER_OFFSET = 0;                             // Buffer offset for data reading

        // Client Defaults
        public const string CLIENT_DEFAULT_ID = "1";                     // Default client logical ID
        public const int INITIAL_MESSAGE_COUNT = 0;                      // Size of the buffer for data transmission 1024 bytes (1 KB)
    }
}
