using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Common.Tracking.Performance
{
    public class RMPerformanceMetrics
    {

        private static readonly ConcurrentDictionary<string, RMPerformanceMetricSummary> s_store = new ConcurrentDictionary<string, RMPerformanceMetricSummary>();

        internal static void Record(string metricName, long elapsedMilliseconds, bool isError)
        {
            // Use the factory overload so a new summary (and its internal state) is only
            // allocated on an actual cache miss, not on every call to Record.
            var summary = s_store.GetOrAdd(metricName, key => new RMPerformanceMetricSummary(key));
            summary.Add(elapsedMilliseconds, isError);
        }

        public static void Reset() => s_store.Clear();

        public static string BuildReport()
        {
            var snapshots = new List<RMPerformanceMetricSnapshot>();
            int nameColumnWidth = "Metric Name".Length;
            foreach (var kvp in s_store)
            {
                var snapshot = kvp.Value.GetSnapshot();
                snapshots.Add(snapshot);
                nameColumnWidth = Math.Max(nameColumnWidth, snapshot.Name.Length);
            }

            var sb = new StringBuilder();
            sb.AppendLine("\n============================== [Overall Performance Summary Report] ==============================");
            sb.AppendLine(string.Format("{0,-" + nameColumnWidth + "} | {1,6} | {2,4} | {3,10} | {4,8} | {5,8}",
                "Metric Name", "Call Count", "Error Count", "Average Elapsed Milliseconds", "Minimum Elapsed Milliseconds", "Maximum Elapsed Milliseconds"));
            sb.AppendLine(new string('-', nameColumnWidth + 50));

            foreach (var snapshot in snapshots)
            {
                sb.AppendLine(string.Format("{0,-" + nameColumnWidth + "} | {1,6} | {2,4} | {3,8:F1}ms | {4,6}ms | {5,6}ms",
                    snapshot.Name,
                    snapshot.CallCount,
                    snapshot.ErrorCount,
                    snapshot.AverageElapsedMilliseconds,
                    snapshot.MinimumElapsedMilliseconds == long.MaxValue ? 0 : snapshot.MinimumElapsedMilliseconds,
                    snapshot.MaximumElapsedMilliseconds == long.MinValue ? 0 : snapshot.MaximumElapsedMilliseconds));
            }
            sb.AppendLine("========================================================================================");
            return sb.ToString();
        }
    }
}
