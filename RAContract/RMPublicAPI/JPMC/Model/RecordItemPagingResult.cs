using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC.Model
{
    public class RecordItemPagingResult
    {
        public List<RecordItem> Items { get; set; } = new List<RecordItem>();
        public long Count { get; set; }
        public string ContinuationToken { get; set; }
    }

    public class RecordItem
    {
        public Guid NodeId { get; set; }
        public string FullPath { get; set; }
        public Guid ConnectionId { get; set; }
        public Guid ConnectionGroupId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ParentId { get; set; }
        public Guid PartitionKeyId { get; set; }
        public int Level { get; set; }
    }
}
