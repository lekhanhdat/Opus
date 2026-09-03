using System;

namespace AvePoint.RA.Contract.Telemetry
{
    public class APStorageCostEvaluationJobTelemetry
    {
        public string TenantId { get; set; }
        public string JobId { get; set; }
        public string JobType { get; set; }
        public string StorageId { get; set; }
        public DateTime CalculatedDate { get; set; }
        public double TotalArchivedSizeInGB { get; set; }
        public double TotalBlobSizeInGB { get; set; }
        public double TotalUnrecordedSizeInGB { get; set; }
    }
}
