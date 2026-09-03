using Azure;
using Azure.Data.Tables;
using System;

namespace AvePoint.RA.DB.Model
{
    public class RMMultiGeoApiChangeLogEntity : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public DateTime CreatedOn { get; set; }

        public string TenantGroupId { get; set; }

        public string CurrentDataCenter { get; set; }

        public string MainDataCenter { get; set; }

        public string TargetDataCenter { get; set; }

        public string TargetApiUrl { get; set; }

        public string OperationType { get; set; }

        public string ApiPath { get; set; }

        public string TriggeredBy { get; set; }

        public bool IsPrimarySuccess { get; set; }

        public bool IsOverallSuccess { get; set; }

        public bool IsBlockedRequest { get; set; }

        public int ReplicaCount { get; set; }

        public int ReplicaFailureCount { get; set; }

        public string FailureReason { get; set; }

        public string RequestBody { get; set; }

        public string ErrorResponse { get; set; }

        public string PrimaryResponse { get; set; }

        public string ReplicaResults { get; set; }

        public static RMMultiGeoApiChangeLogEntity Create(string operationType)
        {
            var now = DateTime.UtcNow;
            return new RMMultiGeoApiChangeLogEntity
            {
                PartitionKey = now.ToString("yyyyMMdd"),
                RowKey = $"{now.Ticks:D20}_{Guid.NewGuid():N}",
                CreatedOn = now,
                OperationType = operationType,
            };
        }
    }
}