using AvePoint.RA.DB.Model.Discovery.Office365;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Model
{
    public sealed class RMDiscoveryOffice365SiteAnalysisResult
    {
        public required RMDiscoveryOffice365SiteInfo SiteInfo { get; init; }

        public required RMDiscoveryOffice365AggregateTotalData AggregateInfo { get; init; }

        public required List<RMDiscoveryOffice365SiteInactiveData> InactiveDataList { get; init; }

        public required List<RMDiscoveryOffice365SiteRuleLevelRotData> RuleLevelRotDataList { get; init; }

        public required List<RMDiscoveryOffice365SiteCategoryLevelRotData> CategoryLevelRotDataList { get; init; }

        public required List<RMDiscoveryOffice365SiteRootLevelRotData> RootLevelRotDataList { get; init; }
    }
}
