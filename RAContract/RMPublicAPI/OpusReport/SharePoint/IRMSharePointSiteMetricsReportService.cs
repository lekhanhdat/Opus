using AvePoint.RA.Contract.CloudService;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.OpusReport.SharePoint
{
    public interface IRMSharePointSiteMetricsReportService
    {
        Task<string> SubmitSPReportExportJobAsync(SPReportExportRequest request);

        Task<string> RealRunSPReportExportJobAsync(JobQueueDto jobQueueDto);
    }
}