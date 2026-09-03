using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery
{
    public interface IRMDiscoverySpecificSiteDao : IBaseDao<RMDiscoverySpecificSite>
    {
        Task<(IEnumerable<RMDiscoverySpecificSite>, long)> LoadM365ExcludeListSitesByPaginationAsync(int pageIndex, int pageSize);
        Task<IEnumerable<RMDiscoverySpecificSite>> GetAllM365ExclusionListSitesAsync();
        int BatchRemoveM365ExclusionListSitesByIds(IEnumerable<int> ids);
        int AddSpecifySites(IEnumerable<RMDiscoverySpecificSite> sites);
        bool IsSiteIncludeInExclusionList(string siteUrl);
        bool ExistM365ExcludeListInSiteUrls(IEnumerable<string> siteUrls);
        void DeleteM365ExcludeList();
        (IEnumerable<string> runnerSite, IEnumerable<string> skipExcludeSite) GetSiteNotInM365ExcludeSite(IEnumerable<string> siteUrls);
        bool IsExistM365ExcludeSite();
    }
}
