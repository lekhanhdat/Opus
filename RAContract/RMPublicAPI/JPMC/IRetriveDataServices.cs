using AvePoint.RA.Contract.RMPublicAPI.JPMC.Model;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public interface IRetriveDataServices
    {
        Task<JobReportResult> GetJobReportAsync(JobReportParam param);
        Task<FSMetadata> GetFSMetadataAsync(FSMetadataParam param);
        Task<FSFileCount> GetFSFileCountByCategory(FSMetadataByCategoryParam param);
        Task<string> GetJobDetails(JMDetailsQuery queryModel);
        Task<RecordItemPagingResult> GetRecordItemInformation(RecordItemQueryDefinition queryModel);
        Task<RecordItemPagingResult> GetPendingDisposalItem(RecordItemQueryDefinition queryModel);
    }
}
