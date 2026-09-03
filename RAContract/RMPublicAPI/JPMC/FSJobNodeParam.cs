using AvePoint.RA.Contract.JPMC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class FSJobNodeParam
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid NodeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ConnectionGroupId { get; set; }
        public int Level { get; set; }
        public string FullPath { get; set; }   
    }

    public class FSDisposalClassCodeParam
    {
        [DataMember(EmitDefaultValue = false)]
        public FSJobNodeParam JobNodeParam { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonConverter(typeof(SingleOrArrayConverter<Guid>))]
        public List<Guid> Terms { get; set; }
    }

    public class RCCReportRequestPublic
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("nodes")]
        public List<RCCNode> Nodes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("connGroupId")]
        public Guid ConnGroupId;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("connectionId")]
        public Guid ConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("jpmcId")]
        public string JPMCId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("level")]
        public int Level;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("timeRange")]
        public RCCReportTimeRangePublic TimeRange { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("isMyhub")]
        public bool IsMyHub { get; set; } = false;
    }

    public class RCCReportTimeRangePublic
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("presetType")]
        public int PresetType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        public (DateTime start, DateTime end) Resolve()
        {
            DateTime now = DateTime.UtcNow;
            return PresetType switch
            {
                1 => (now, now.AddMonths(3)),
                2 => (now, now.AddMonths(6)),
                3 => (now, now.AddYears(1)),
                _ => (new DateTime(StartDate, DateTimeKind.Utc), new DateTime(EndDate, DateTimeKind.Utc))
            };
        }
    }
}
