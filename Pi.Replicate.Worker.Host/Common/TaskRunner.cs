using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Common
{
    public class TaskRunner
    {
		private readonly List<Task> _runningTasks = new List<Task>();
		private readonly SemaphoreSlim _semaphoreSlim;
		private Timer _cleanupJob;

		public TaskRunner(int maxAmountOfRunningTasks)
		{
			_semaphoreSlim = new SemaphoreSlim(maxAmountOfRunningTasks);
			_cleanupJob = new Timer(x => { _runningTasks.RemoveAll(x => x.IsCompleted); },null,TimeSpan.FromSeconds(0),TimeSpan.FromMinutes(1));
		}

        public void Add(Func<Task> action)
		{
			_runningTasks.Add(Task.Run(async () =>
			{
				await _semaphoreSlim.WaitAsync();
				await action();
				_semaphoreSlim.Release();
			}));
		}
    }
}
