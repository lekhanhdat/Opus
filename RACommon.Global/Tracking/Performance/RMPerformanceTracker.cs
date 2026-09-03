using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public class RMPerformanceTracker : IDisposable
    {

        private readonly string _trackerName;

        private readonly double _thresholdInMilliseconds;

        private readonly Action<string> _logger;

        private readonly Stopwatch _stopwatch;

        private readonly ConcurrentQueue<RMPerformanceStepMetric> _steps = new ConcurrentQueue<RMPerformanceStepMetric>();

        private int _hasFaulted;

        internal RMPerformanceTracker(string trackerName, double thresholdInMilliseconds, Action<string> logger)
        {
            _trackerName = trackerName;
            _thresholdInMilliseconds = thresholdInMilliseconds;
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
        }

        public RMPerformanceStepScope Step(string stepName)
        {
            return new RMPerformanceStepScope(stepName, RecordStepMetric);
        }

        /// <summary>
        /// Marks this tracker as faulted so <see cref="Dispose"/> records the tracked
        /// operation as an error. Must be called explicitly from a catch block, since
        /// there is no reliable way to detect an in-flight managed exception from Dispose.
        /// </summary>
        public void MarkFaulted()
        {
            Interlocked.Exchange(ref _hasFaulted, 1);
        }

        public void Step(string stepName, Action action)
        {
            using (var stepScope = Step(stepName))
            {
                action();
            }
        }

        public T Step<T>(string stepName, Func<T> func)
        {
            using (var stepScope = Step(stepName))
            {
                return func();
            }
        }

        public async System.Threading.Tasks.Task StepAsync(string stepName, Func<System.Threading.Tasks.Task> func)
        {
            using (var stepScope = Step(stepName))
            {
                await func().ConfigureAwait(false);
            }
        }

        public async System.Threading.Tasks.Task<T> StepAsync<T>(string stepName, Func<System.Threading.Tasks.Task<T>> func)
        {
            using (var stepScope = Step(stepName))
            {
                return await func().ConfigureAwait(false);
            }
        }

        private void RecordStepMetric(string stepName, long elapsedMilliseconds)
        {
            _steps.Enqueue(new RMPerformanceStepMetric(stepName, elapsedMilliseconds));

            RMPerformanceMetrics.Record($"{_trackerName}.{stepName}", elapsedMilliseconds, false);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            long elapsedMs = _stopwatch.ElapsedMilliseconds;
            bool hasException = Interlocked.CompareExchange(ref _hasFaulted, 0, 0) != 0;

            RMPerformanceMetrics.Record(_trackerName, elapsedMs, hasException);

            if (elapsedMs < _thresholdInMilliseconds || _logger == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($"\n================== [Perf] {_trackerName} {(hasException ? "❌ [ERR]" : "✅")} ==================");
            sb.AppendLine($"[Total Elapsed Time] : {elapsedMs} ms");

            if (!_steps.IsEmpty)
            {
                sb.AppendLine("------------------ Sub-module Breakdown ------------------");
                foreach (var step in _steps)
                {
                    sb.AppendLine($" -> [{step.Name}] Elapsed Time: {step.ElapsedMilliseconds} ms");
                }
            }
            sb.AppendLine("=============================================================\n");
            _logger.Invoke(sb.ToString());

        }
    }
}
