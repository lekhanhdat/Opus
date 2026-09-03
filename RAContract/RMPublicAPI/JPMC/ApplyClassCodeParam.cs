using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class ApplyClassCodeParam
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ClassCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string CountryCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int RetentionType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long StartDate { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ApplyToExistingDoc { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermId { set; get; }

    }
}
