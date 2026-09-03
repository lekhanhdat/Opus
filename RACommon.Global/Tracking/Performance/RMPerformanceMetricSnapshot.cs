using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public class RMPerformanceMetricSnapshot
    {
        public string Name { get; set; }

        public long CallCount { get; set; }

        public long ErrorCount { get; set; }

        public long MinimumElapsedMilliseconds { get; set; }

        public long MaximumElapsedMilliseconds { get; set; }

        public long AverageElapsedMilliseconds { get; set; }
    }
}
