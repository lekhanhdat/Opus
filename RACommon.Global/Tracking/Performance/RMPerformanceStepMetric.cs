namespace AvePoint.RA.Common.Tracking.Performance
{
    public struct RMPerformanceStepMetric
    {

        public string Name { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public RMPerformanceStepMetric(string name, long elapsedMilliseconds)
        {
            Name = name;
            ElapsedMilliseconds = elapsedMilliseconds;
        }
    }
}
