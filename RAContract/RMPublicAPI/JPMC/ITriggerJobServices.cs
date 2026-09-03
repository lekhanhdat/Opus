using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public interface ITriggerJobServices
    {
        Task<RAReturnMessage> RunDataSyncJobAsync(FSJobNodeParam param);
        Task<RAReturnMessage> RunDisposalJobAsync(FSJobNodeParam param);
        Task<RAReturnMessage> RunDisposalByClassCodeAsync(FSDisposalClassCodeParam param);
        Task<RAReturnMessage> RunDownloadRCCReportJobAsync(RCCReportRequestPublic param);
        Task<RAReturnMessage> StopJobsAsync(List<string> ids);
        Task<RAReturnMessage> RunApplyClassCodeAsync (ApplyClassCodeParam param);
        Task<RAReturnMessage> RejectAsync(ManualApprovalActionParams param);
        Task<RAReturnMessage> ApproveAsync(ManualApprovalActionParams param);
        Task<RAReturnMessage> RunExportRecordsForReviewDataJob();
        Task<RAReturnMessage> ExportHistoryData(ManualApprovalHistoryOption historyOption);
        Task<RAReturnMessage> PauseDisposalProcess(PauseOrResumeReq req);
        Task<RAReturnMessage> ResumeDisposalProcess(PauseOrResumeReq req);
        Task<RMFSTreeNode> BuildTreeNodeAsync(RMFSTreeNode sNode);
        Task<RAReturnMessage> RunFSDashboardJobAsync(FileSystemMyhubSelectedNodeDto selectedNode);
        RAReturnMessage IsNodeEligible(FSJobNodeParam nodeParam);
        //Task<bool> ValidateNode(string nodeId, int level, string fullPath);
    }
}
