/*
* FILE : LogManager.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* This function contains methods for logging messages to a file asynchronously
*/

using A01Server.utils; // variable name constants
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace A01Server
{
    internal class LogManager
    {
        private string logFilePath = Constants.LOG_FILE_NAME; // "log.txt" file path
        private static readonly SemaphoreSlim fileLock = new SemaphoreSlim(1, 1); // prevent concurrency conflicts (e.g., multiple clients trying to write to log simultaneously)

        //
        // FUNCTION : WriteLogAsync
        // DESCRIPTION : Asynchronously writes a log message to a file and checks if the file size limit is reached
        // PARAMETERS : 
        // string message - The log message to write
        // RETURNS :
        // Task<bool> : True if the file size limit is reached, otherwise false (from App.config)
        //
        public async Task<bool> WriteLogAsync(string message)
        {
            bool limitReached = false;
            long maxSize = 0;

            // Utilize your Constants from the utils folder to remove magic strings
            string configLimit = ConfigurationManager.AppSettings[Constants.FILE_LIMIT];

            // wait to enter critical section
            await fileLock.WaitAsync();

            try
            {
                // Convert string to long for size comparison
                maxSize = long.Parse(configLimit);

                // Use StreamWriter for asynchronous file writing
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    await writer.WriteLineAsync(message); // append message to log file
                }

                // Check if file size hit the limit for graceful stop
                FileInfo fileInfo = new FileInfo(logFilePath);
                if (fileInfo.Length >= maxSize)
                {
                    limitReached = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log Error: " + ex.Message);
            }
            finally
            {
                fileLock.Release(); // release the semaphore
            }

            return limitReached;
        }
    }
}
