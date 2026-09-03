/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{

	public static class ThreadingExt
	{
		public static void SetApartmentStateExt(this Thread thread, ApartmentState state)
		{
			if (OperatingSystem.IsWindows())
			{
				thread.SetApartmentState(state);
			}
		}
	}

	/// <summary>Provides a scheduler that uses STA threads.</summary>
	internal sealed class StaTaskScheduler : TaskScheduler, IDisposable
	{
		/// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
		private BlockingCollection<Task> _tasks;

		/// <summary>The STA threads used by the scheduler.</summary>
		private readonly List<Thread> _threads;

		/// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
		public override int MaximumConcurrencyLevel => _threads.Count;

		/// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
		/// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
		public StaTaskScheduler(int numberOfThreads)
		{
			if (numberOfThreads < 1)
			{
				throw new ArgumentOutOfRangeException("numberOfThreads");
			}
			_tasks = new BlockingCollection<Task>();
			_threads = Enumerable.Range(0, numberOfThreads).Select((Func<int, Thread>)delegate
			{
				Thread thread = new Thread((ThreadStart)delegate
				{
					foreach (Task item in _tasks.GetConsumingEnumerable())
					{
						TryExecuteTask(item);
					}
				})
				{
					IsBackground = true
				};
				thread.SetApartmentStateExt(ApartmentState.STA);
				return thread;
			}).ToList();
			_threads.ForEach(delegate(Thread t)
			{
				t.Start();
			});
		}

		/// <summary>Queues a Task to be executed by this scheduler.</summary>
		/// <param name="task">The task to be executed.</param>
		protected override void QueueTask(Task task)
		{
			_tasks.Add(task);
		}

		/// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
		/// <returns>An enumerable of all tasks currently scheduled.</returns>
		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return _tasks.ToArray();
		}

		/// <summary>Determines whether a Task may be inlined.</summary>
		/// <param name="task">The task to be executed.</param>
		/// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
		/// <returns>true if the task was successfully inlined; otherwise, false.</returns>
		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				return TryExecuteTask(task);
			}
			return false;
		}

		/// <summary>
		/// Cleans up the scheduler by indicating that no more tasks will be queued.
		/// This method blocks until all threads successfully shutdown.
		/// </summary>
		public void Dispose()
		{
			if (_tasks != null)
			{
				_tasks.CompleteAdding();
				foreach (Thread thread in _threads)
				{
					thread.Join();
				}
				_tasks.Dispose();
				_tasks = null;
			}
		}
	}
}