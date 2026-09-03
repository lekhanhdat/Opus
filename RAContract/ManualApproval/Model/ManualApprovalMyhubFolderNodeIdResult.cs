using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalMyhubFolderNodeIdQueryDefinition
    {
        [JsonProperty("PartitionKeyId")]
        public string PartitionKeyId { get; set; }
        [JsonProperty("NodeId")]
        public string NodeId { get; set; }
    }
}
