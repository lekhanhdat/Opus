using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalFSAuditRecordDto
    {
        [JsonProperty("nodeId")]
        public Guid NodeId { get; set; }

        [JsonProperty("auditType")]
        public int AuditType { get; set; }

        [JsonProperty("auditLevel")]
        public int AuditLevel { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("actionTimeUtc")]
        public long ActionTimeUtc { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

        [JsonProperty("connectionGroupId")]
        public string ConnectionGroupId { get; set; }

        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }

        [JsonProperty("currentPath")]
        public string FullPath { get; set; }

        [DataMember]
        public SOApproveDBStatus ActionType { get; set; }

        [JsonProperty("isPause")]
        public int IsPause { get; set; }

    }
}
