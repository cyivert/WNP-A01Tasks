/*
* FILE : LogManager.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A01Server
{
    internal class LogManager
    {
        private string logFilePath = "CentralLog.txt";
        private long maxSizeInBytes = 10000; // 10KB limit for graceful stop

        //
        // FUNCTION : WriteLogAsync
        // DESCRIPTION : Asynchronously writes a log message to the log file.
        // PARAMETERS : 
        // string message : The log message to write.
        // RETURNS :
        // Task<bool> : A task that represents the asynchronous operation. The task result is true if the log file size limit is reached; otherwise, false.
        //
        public async Task<bool> WriteLogAsync(string message)
        {
            bool limitReached = false;

            try
            {
                // Use StreamWriter for async writing in .NET Framework 4.7.2
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    await writer.WriteLineAsync(message);
                }

                // Check file size for graceful stop requirement
                FileInfo fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Length >= maxSizeInBytes)
                {
                    limitReached = true;
                }
            }
            catch (Exception ex)
            {
                // Good programming practice: handle exceptions specifically
                Console.WriteLine("File Error: " + ex.Message);
            }

            return limitReached; // Single return statement at the end
        }
    }
}
