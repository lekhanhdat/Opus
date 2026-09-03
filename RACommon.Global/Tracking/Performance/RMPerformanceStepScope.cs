using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public struct RMPerformanceStepScope : IDisposable
    {

        private readonly string _name;

        private readonly Action<string, long> _onComplete;

        private readonly Stopwatch _stopwatch;

        public RMPerformanceStepScope(string name, Action<string, long> onComplete)
        {
            _name = name;
            _onComplete = onComplete;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _onComplete(_name, _stopwatch.ElapsedMilliseconds);
        }
    }
}
