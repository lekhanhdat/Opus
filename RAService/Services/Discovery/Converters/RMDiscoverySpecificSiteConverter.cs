using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.Converters
{
    public static class RMDiscoverySpecificSiteConverter
    {
        public static IEnumerable<RMDiscoverySpecificSite> ToM365ExcludeSiteModel(this IEnumerable<DiscoverySpecificSiteDto> dtos)
        {
            if (dtos == null) return Enumerable.Empty<RMDiscoverySpecificSite>();
            return dtos.Select(dto => new RMDiscoverySpecificSite
            {
                Id = dto.Id,
                Url = dto.SiteCollectionUrl,
                Type = SpecifySiteFlag.Exclude,
                SourceFlag = SourceFlag.SharePoint
            });
        }

        public static IEnumerable<DiscoverySpecificSiteDto> ToDiscoverySpecificSiteDto(this IEnumerable<RMDiscoverySpecificSite> models)
        {
            if (models == null) return Enumerable.Empty<DiscoverySpecificSiteDto>();
            return models.Select(model => new DiscoverySpecificSiteDto
            {
                Id = model.Id,
                SiteCollectionUrl = model.Url
            });
        }
    }
}
