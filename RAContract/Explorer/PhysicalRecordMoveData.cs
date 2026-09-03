using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class PhysicalRecordMoveData : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string ItemName { get; set; }
        public string UniqueId { get; set; }
        public string ApproveBy { get; set; }
        public Guid DestinationLocationId { get; set; }
        public string DestinationPath { get; set; }
        public Guid HomeLocationId { get; set; }
        public string HomeLocation { get; set; }
        public string Comment { get; set; }
        public long ExecuteOn { get; set; }
        public int Status { get; set; }
    }
}
