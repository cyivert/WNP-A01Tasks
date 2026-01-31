/*
* FILE : PerformanceTracker.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* The functions in this file are used to ...
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A01Client
{
    internal class PerformanceTracker
    {
        private Stopwatch watch = new Stopwatch();

        //
        // FUNCTION : StartTracking
        // DESCRIPTION : This function starts the performance tracking stopwatch.
        // PARAMETERS : n/a
        // RETURNS : n/a
        //
        public void StartTracking()
        {
            watch.Restart();
            return;
        }

        //
        // FUNCTION : GetElapsedMs
        // DESCRIPTION : This function stops the stopwatch and returns the elapsed time in milliseconds.
        // PARAMETERS : n/a
        // RETURNS : 
        // long result : The elapsed time in milliseconds.
        //
        public long GetElapsedMs()
        {
            watch.Stop();
            long result = watch.ElapsedMilliseconds;
            return result;
        }
    }
}
