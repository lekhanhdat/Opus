using Newtonsoft.Json;
using System.Collections.Generic;

namespace AvePoint.RA.Common.GraphApi.GroupSite
{
    public class RMGraphGroupDefinition
    {
    }

    public class CheckMemberGroupsResponse
    {
        [JsonProperty("value")]
        public List<string> Value { get; set; }
    }
}
