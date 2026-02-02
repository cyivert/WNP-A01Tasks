/*
* FILE : PerformanceTracker.cs
* PROJECT : A01-Tasks
* PROGRAMMER : Cy Iver Torrefranca
* DESCRIPTION :
* This class is responsible for tracking performance metrics such as elapsed time and provides methods to start and stop the tracking.
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
        public double GetElapsedMs()
        {
            watch.Stop();
            double result = watch.Elapsed.TotalMilliseconds;  // Sub-millisecond precision
            return result;
        }
    }
}
