using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
		{
			if(!task.IsCompleted || task.IsFaulted)
			{
				_ = ForgetAwaited(task);
			}
		}

		static async Task ForgetAwaited(Task task)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch { }
		}
    }
}
