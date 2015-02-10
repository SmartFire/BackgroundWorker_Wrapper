# BackgroundWorker_Wrapper
This is a wrapper class I have written for the System.ComponentModel.BackgroundWorker class. 
It provides many methods to assist you when it comes to working with a large number of BackgroundWorkers.

Documentation and Examples can be found [here](http://zejustin.info/BackgroudWorker_Wrapper/)


# Simple Example (seen better [here](http://zejustin.info/BackgroudWorker_Wrapper/examples.php)):
	class Examples
	{
		// Create a new instance of BgHelper with 200 workers, and tell it to not call DoEvents().
		readonly BgHelper _workersHelper = new BgHelper(200, false);

		public Examples()
		{
			for (var i = 0; i < 200; i++)
			{
				// Grab the next available worker in the pool
				BackgroundWorker worker = _workersHelper.GetNextWorker();
				// Assign the DoWork event, or any other event.
				worker.DoWork += WorkerOnDoWork;
				// Start the worker, optionally passing an argument.
				worker.RunWorkerAsync();
			}

			// If you want to do something wile waiting for the workers to complete
			while (!_workersHelper.CheckCompletion())
			{
				// Do Some Stuff

				// If at any time you need to cancel all of your workers, simply call the method.
				_workersHelper.CancelAllWorkers();
				// Or, because we are using the thread collection in this example
				// we can use KillAllWorkers to forcefully abort the threads themselves.
				_workersHelper.KillAllWorkers();
			}

			// Or if you simply want to wait until all workers complete instead, just use this.
			_workersHelper.WaitForWorkers();
		}

		private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			// Add this worker to the thread collection so that I can call KillAllWorkers later.
			// This is completely optional. Only use this if you plan to use KillAllWorkers
			var index = _workersHelper.AddWorkerThread(Thread.CurrentThread);

			// Perform some task here...
			//...
			//...

			// Because I am using the thread collection, I need to reset this
			// workers location in the pool so the next worker can take its place.
			_workersHelper.ResetWorkerThread(index);
		}
	}