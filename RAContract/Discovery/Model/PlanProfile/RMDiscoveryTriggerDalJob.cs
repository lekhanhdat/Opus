using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.RA.Contract.Discovery.Model.PlanProfile
{
    public class RMDiscoveryTriggerDalJob
    {
        [JsonProperty("scopeType")]
        public RMDiscoveryOffice365ScopeType ScopeType { get; set; } = RMDiscoveryOffice365ScopeType.DataSource;

        [JsonProperty("contentSources")]
        public List<SourceFlag> ContentSources { get; set; } = [];

        [JsonProperty("specifyContainerIds")]
        public List<Guid> SpecifyContainerIds { get; set; } = [];

        public RMDiscoveryTriggerDalJob CompatibleConvert()
        {
            if (ScopeType != RMDiscoveryOffice365ScopeType.DataSource)
            {
                return this;
            }

            if (!ContentSources.Any())
            {
                ContentSources.Add(SourceFlag.SharePoint);
            }

            return this;
        }

    }
} 
