using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public static class RMPerformanceMonitor
    {
        public static Action<string> Logger { get; set; }

        public static RMPerformanceTracker Scope(string trackerName, double thresholdInMilliseconds = 0)
        {
            return new RMPerformanceTracker(trackerName, thresholdInMilliseconds, Logger);
        }

        public static void Track(string trackerName, Action action, double thresholdInMilliseconds = 0)
        {
            using (var tracker = new RMPerformanceTracker(trackerName, thresholdInMilliseconds, Logger))
            {
                try
                {
                    action();
                }
                catch
                {
                    tracker.MarkFaulted();
                    throw;
                }
            }
        }

        public static T Track<T>(string trackerName, Func<T> func, double thresholdInMilliseconds = 0)
        {
            using (var tracker = new RMPerformanceTracker(trackerName, thresholdInMilliseconds, Logger))
            {
                try
                {
                    return func();
                }
                catch
                {
                    tracker.MarkFaulted();
                    throw;
                }
            }
        }

        public static async System.Threading.Tasks.Task TrackAsync(string trackerName, Func<System.Threading.Tasks.Task> func, double thresholdInMilliseconds = 0)
        {
            using (var tracker = new RMPerformanceTracker(trackerName, thresholdInMilliseconds, Logger))
            {
                try
                {
                    await func();
                }
                catch
                {
                    tracker.MarkFaulted();
                    throw;
                }
            }
        }

        public static async System.Threading.Tasks.Task<T> TrackAsync<T>(string trackerName, Func<System.Threading.Tasks.Task<T>> func, double thresholdInMilliseconds = 0)
        {
            using (var tracker = new RMPerformanceTracker(trackerName, thresholdInMilliseconds, Logger))
            {
                try
                {
                    return await func();
                }
                catch
                {
                    tracker.MarkFaulted();
                    throw;
                }
            }
        }

        public static void LogSummary() => Logger?.Invoke(RMPerformanceMetrics.BuildReport());

        public static void ResetMetrics() => RMPerformanceMetrics.Reset();
    }
}
