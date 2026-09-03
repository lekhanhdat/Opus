using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC.Model
{
    public class RecordItemQueryDefinition
    {
        [DataMember]
        public Guid ConnectionGroupId { get; set; }

        [DataMember]
        public Guid ConnectionId { get; set; }

        [DataMember]
        public string FullPathConnection { get; set; }

        [DataMember]
        public string ContinuationToken { get; set; }

        [DataMember]
        public int PageSize { get; set; } = 10;

        [DataMember]
        public int Level { get; set; } = 2100;

        [DataMember]
        public bool IsDesc { get; set; } = false;

    }
}
