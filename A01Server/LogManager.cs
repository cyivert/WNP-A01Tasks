/*
* FILE : LogManager.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* This function contains methods for logging messages to a file asynchronously
* and checking if the log file size limit is reached for graceful shutdown.
* CancellationTOken is used to handle server shutdown scenarios.
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
        public async Task<bool> WriteLogAsync(string message, CancellationToken token)
        {
            bool limitReached = false;
            long maxSize = long.Parse(ConfigurationManager.AppSettings[Constants.FILE_LIMIT]);
            bool lockAcquired = false; // FLAG: lock acquired status

            try
            {
                // waits for the lock or cancel if  the server is stopping
                await fileLock.WaitAsync(token);
                lockAcquired = true;

                // mark that the lock is acquired
                token.ThrowIfCancellationRequested();

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
            catch (OperationCanceledException)
            {
                // when server is stopping, just exit gracefully
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log Error: " + ex.Message);
            }
            finally
            {
                // release the lock
                if (lockAcquired)
                {
                    fileLock.Release();
                }
            }

            return limitReached;
        }
    }
}
