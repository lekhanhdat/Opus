using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public class RMPerformanceMetricSummary
    {

        private long _callCount;

        private long _errorCount;

        private long _totalElapsedMilliseconds;

        private long _minimumElapsedMilliseconds = long.MaxValue;

        private long _maximumElapsedMilliseconds = long.MinValue;

        public string Name { get; }

        public long CallCount => Interlocked.Read(ref _callCount);

        public long ErrorCount => Interlocked.Read(ref _errorCount);

        public long TotalElapsedMilliseconds => Interlocked.Read(ref _totalElapsedMilliseconds);

        public long MinimumElapsedMilliseconds => Interlocked.Read(ref _minimumElapsedMilliseconds);

        public long MaximumElapsedMilliseconds => Interlocked.Read(ref _maximumElapsedMilliseconds);

        public RMPerformanceMetricSummary(string name)
        {
             Name = name;
        }

        public void Add(long elapsedMilliseconds, bool isError)
        {
            Interlocked.Increment(ref _callCount);
            if (isError)
            {
                Interlocked.Increment(ref _errorCount);
            }

            Interlocked.Add(ref _totalElapsedMilliseconds, elapsedMilliseconds);

            UpdateMinimum(elapsedMilliseconds);
            UpdateMaximum(elapsedMilliseconds);
        }

        public RMPerformanceMetricSnapshot GetSnapshot()
        {
            long callCount = CallCount;
            long totalElapsedMilliseconds = TotalElapsedMilliseconds;

            return new RMPerformanceMetricSnapshot
            {
                Name = Name,
                CallCount = callCount,
                ErrorCount = ErrorCount,
                MinimumElapsedMilliseconds = MinimumElapsedMilliseconds,
                MaximumElapsedMilliseconds = MaximumElapsedMilliseconds,
                AverageElapsedMilliseconds = callCount > 0 ? totalElapsedMilliseconds / callCount : 0
            };
        }

        private void UpdateMinimum(long candidate)
        {
            long current;
            do
            {
                current = Interlocked.Read(ref _minimumElapsedMilliseconds);
                if (candidate >= current)
                {
                    return;
                }
            } while (Interlocked.CompareExchange(ref _minimumElapsedMilliseconds, candidate, current) != current);
        }

        private void UpdateMaximum(long candidate)
        {
            long current;
            do
            {
                current = Interlocked.Read(ref _maximumElapsedMilliseconds);
                if (candidate <= current)
                {
                    return;
                }
            } while (Interlocked.CompareExchange(ref _maximumElapsedMilliseconds, candidate, current) != current);
        }
    }
}
