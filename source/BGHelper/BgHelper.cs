using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace BgHelper
{
    public class BgHelper
    {
        // Default number of workers.
        private const int DefaultCount = 2000;
        // Current number of workers
        private int _workerCount;
        // Internal collection of BackgroundWorkers
        private BackgroundWorker[] _workerCollection;
        // Second container to keep track of the ThreadIDs, for use with the KillAll command.
        private Thread[] _threadCollection;

        // This is the default amount of time the any method will wait until throwing an exception.
        public int WaitTimeoutInMilliSeconds = 5*60*1000; // 5 Minutes
        // Should we call DoEvents or not for blocking methods.
        public bool CallDoEvents = true;

        /// <summary>
        /// Creates an instance of BgHelper with 2000 workers.
        /// </summary>
        public BgHelper()
        {
            InitBgHelper(DefaultCount);
        }
        /// <summary>
        /// Creates an instance of BgHelper with the set number of workers.
        /// </summary>
        /// <param name="maxWorkers">How many workers should be contained.</param>
        /// <param name="doEvents">Whether or not to call System.Windows.Forms.Application.DoEvents()</param>
        public BgHelper(int maxWorkers, bool doEvents = true)
        {
            InitBgHelper(maxWorkers);
            CallDoEvents = doEvents;
        }
        private void InitBgHelper(int maxWorkers)
        {
            StopLooking = false;
            _workerCount = maxWorkers;
            _workerCollection = new BackgroundWorker[maxWorkers + 1];
            _threadCollection = new Thread[maxWorkers + 1];
        }
        /// <summary>
        /// Can be used to force some blocking methods to stop. 
        /// </summary>
        public bool StopLooking { get; set; }
        /// <summary>
        /// Sets how many seconds until methods throw BgHelperTimeoutExceptions.
        /// </summary>
        public void SetTimeout_Seconds(int seconds)
        {
            WaitTimeoutInMilliSeconds = seconds*1000;
        }
        /// <summary>
        /// Sets how many minutes until methods throws BgHelperTimeoutExceptions.
        /// </summary>
        public void SetTimeout_Minutes(int minutes)
        {
            WaitTimeoutInMilliSeconds = minutes*60*1000;
        }
        /// <summary>
        /// Sets how many hours until methods throws BgHelperTimeoutExceptions.
        /// </summary>
        public void SetTimeout_Hours(int hours)
        {
            WaitTimeoutInMilliSeconds = hours*60*60*1000;
        }

        private void DoEvents()
        {
            if (CallDoEvents) System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// Will loop over the collection of workers and return the next available worker. 
        /// </summary>
        /// <remarks>Does not scale very well with very large collections.</remarks>
        public BackgroundWorker GetNextWorker()
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (StopLooking) return null;
                for (var i = 0; i <= _workerCount - 1; i++)
                {
                    if (_workerCollection[i] != null && _workerCollection[i].IsBusy) continue;
                    _workerCollection[i] = new BackgroundWorker();
                    return _workerCollection[i];
                }
                Thread.Sleep(1);
                DoEvents();
                if (sw.ElapsedMilliseconds > WaitTimeoutInMilliSeconds)
                    throw new BgHelperTimeoutException("Timeout expired for GetNextWorker.");
            }
        }
        /// <summary>
        /// Will wait the set amount of time for all workers to complete.
        /// </summary>
        public void WaitForWorkers()
        {
            var sw = Stopwatch.StartNew();
            var allDone = false;
            while (!allDone)
            {
                allDone = CheckCompletion();
                Thread.Sleep(1);
                DoEvents();
                if (sw.ElapsedMilliseconds > WaitTimeoutInMilliSeconds)
                    throw new BgHelperTimeoutException("Timeout expired for WaitForWorkers.");
            }
            sw.Stop();
        }
        /// <summary>
        /// Simply checks if all workers are complete. 
        /// </summary>
        public bool CheckCompletion()
        {
            var result = true;
            for (var i = 0; i <= _workerCount - 1; i++)
            {
                if (_workerCollection[i] == null || !_workerCollection[i].IsBusy) continue;
                // Worker is busy, return false;
                result = false;
                break;
            }
            return result;
        }
        // Create some objects to lock up collections.
        private readonly object _addLock = new object();
        private readonly object _resetLock = new object();
        /// <summary>
        /// Returns and index of the threads location in the pool. Make sure to call ResetWorkerThread with this index as your worker finishes its task.
        /// </summary>
        public int AddWorkerThread(Thread t)
        {
            lock (_addLock)
            {
                for (var i = 0; i <= _workerCount - 1; i++)
                {
                    if (_threadCollection[i] != null) continue;
                    _threadCollection[i] = t;
                    return i;
                }
                return -1;
            }
        }
        /// <summary>
        /// Resets the specific index in the pool so another thread can pick it up.
        /// </summary>
        public void ResetWorkerThread(int index)
        {
            lock (_resetLock) _threadCollection[index] = null;
        }
        /// <summary>
        /// Calls .Abort() on all threads contained by the AddWorkerThread() method. 
        /// </summary>
        public void KillAllWorkers()
        {
            lock (_addLock)
            {
                lock (_resetLock)
                {
                    for (var i = 0; i <= _workerCount - 1; i++)
                    {
                        if (_threadCollection[i] != null) _threadCollection[i].Abort();
                        _threadCollection[i] = null;
                    }
                }
            }
        }
        /// <summary>
        /// As the name suggests, calls .CancelAsync on all active workers.
        /// </summary>
        public void CancelAllWorkers()
        {
            for (var i = 0; i <= _workerCount - 1; i++)
            {
                if (_workerCollection[i] != null && _workerCollection[i].IsBusy) _workerCollection[i].CancelAsync();
            }
        }

    }

    

}
