using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Object;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery
{
    public interface IRMDiscoverySpecificSiteService
    {
        Task<DiscoverySpecificSiteInfo> LoadM365ExclusionListSitesByPaginationAsync(int pageIndex, int pageSize);
        Task<List<DiscoverySpecificSiteDto>> GetAllM365ExclusionListSites();
        RAReturnMessage RemoveM365ExclusionListSitesByIds(IEnumerable<int> ids);
        RAReturnMessage AddM365ExcludeSites(IEnumerable<DiscoverySpecificSiteDto> sites);
        bool IsSiteIncludeInExclusionList(string siteUrl);
        RAReturnMessage ImportExcludeSCList(Stream csvExcludeFileStream);
        RAReturnMessage ExportSCExcludelist();
        string RealRunExportSCExcludeList(string jobRunByUser);
        string RealRunImportSCExcludeList(string jobRunByUser, string filePath);
        bool ValidM365ListSites(IEnumerable<DiscoverySpecificSiteDto> sites,
            out List<DiscoverySpecificSiteDto> notExistSites,
            out List<string> dupSites,
            out List<DiscoverySpecificSiteDto> validSites);
        void DeleteM365ExcludeList();
        (IEnumerable<string> runnerSite, IEnumerable<string> skipExcludeSite) GetRunnableAndExcludedM365Sites(IEnumerable<string> siteUrls);
    }
}
