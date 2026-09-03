using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.MyHub;
using AvePoint.RA.Service.SharePointSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub
{
    [AsyncAudit]
    public class RMMyhubAsyncAuditServices : RMServiceBase, IRMMyhubAsyncAuditServices
    {
        RALogger logger = RALogger.GetInstance(typeof(RMMyhubAsyncAuditServices));

        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();

        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.FSMyhub,
           Action = AuditAction.MarkToPause, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler),
           IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<ManualApprovalActionResult> PauseAsync(PauseOrResumeReq req)
        {
            return await UpdateConnectoinIsPauseAsyncDetail(req);
        }

        [AsyncAudit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.FSMyhub,
            Action = AuditAction.MarkToResume, IAsyncAfterHandler = typeof(RMManualApprovalAfterAuditHandler),
            IAsyncBeforeHandler = typeof(RMManualApprovalBeforeAuditHandler))]
        public async Task<ManualApprovalActionResult> ResumeAsync(PauseOrResumeReq req)
        {
            return await UpdateConnectoinIsPauseAsyncDetail(req);
        }

        public async Task<ManualApprovalActionResult> UpdateConnectoinIsPauseAsyncDetail(PauseOrResumeReq req)
        {
            ManualApprovalActionResult res = new ManualApprovalActionResult();
            res.CompletedStatus = ActionCompletedStatus.Succeed;
            List<string> nodeIds = req.NodeIds.Distinct().ToList();
            List<Guid> guidNodeIds = nodeIds.Select(Guid.Parse).ToList();
            List<FSConnection> fList = FSConnectionDao.GetConnectionByIds(guidNodeIds);
            if (fList == null || fList.Count < 1)
            {
                logger.Error($"Error occurred while select Connections from DB by connectionIds. Error: ConnectionIds is error. Param: {req}");
                throw new Exception("Param is error");
            }
            Dictionary<Guid, FSConnection> connDict = fList.ToDictionary(i => i.Id, i => i);

            List<Guid> groupIds = fList.Select(i => i.GroupId).Distinct().ToList();
            guidNodeIds.AddRange(groupIds);

            Dictionary<string, string> map = fList.ToDictionary(i => i.Id.ToString(), i => i.GroupId.ToString());

            List<string> nodeIdStrs = guidNodeIds.Select(i => i.ToString()).ToList();
            List<JobType> jobTypes = new List<JobType>() { JobType.FSDisposal, JobType.FSDisposalByClassCode,
                    JobType.FSDisposalSchedule };
            List<BaseJobDto> runningJobs = RMJobService.GetRunningJobsBatch(jobTypes, nodeIdStrs);

            List<string> pauseNodeIds = new List<string>();
            if (runningJobs != null && runningJobs.Count > 0)
            {
                List<string> scopeIds = runningJobs.Select(i => i.ScopeId).Distinct().ToList();

                List<string> list1 = new List<string>();
                List<string> list2 = new List<string>();
                foreach (var item in map)
                {
                    if (scopeIds.Contains(item.Key) || scopeIds.Contains(item.Value))
                    {
                        list1.Add(item.Key);
                    }
                    else
                    {
                        list2.Add(item.Key);
                    }
                }

                if (list2.Count < 1)
                {
                    res.CompletedStatus = ActionCompletedStatus.Failed;
                    res.Message = I18NEntity.GetString("RM_FS_CONNECTION_PAUSE_ALL_MESSAGE");
                    return res;
                    ;
                }

                List<Guid> noPauseConnIds = list1.Select(Guid.Parse).ToList();
                List<FSConnection> cList = FSConnectionDao.GetConnectionByIds(noPauseConnIds);
                if (cList != null && cList.Count > 0)
                {
                    List<string> cNames = cList.Select(i => i.Name).Distinct().ToList();
                    string connNames = string.Join(",", cNames);
                    res.CompletedStatus = ActionCompletedStatus.HasException;
                    res.Message = $"{string.Format(I18NEntity.GetString("RM_FS_CONNECTION_PAUSE_MESSAGE"), connNames)}";
                }

                pauseNodeIds = list2;
            }
            else
            {
                pauseNodeIds = nodeIds;
            }

            List<Guid> gPauseNodeIds = pauseNodeIds.Select(Guid.Parse).ToList();

            List<ManualApprovalItemActionResult> items = new List<ManualApprovalItemActionResult>();
            foreach (Guid nodeId in gPauseNodeIds)
            {
                ManualApprovalItemActionResult item = new ManualApprovalItemActionResult();
                item.Id = nodeId;
                item.IsSucceed = true;

                FSConnection conn = connDict[nodeId];
                item.OldValue = conn.IsPause;
                item.EffectItemFullPath = conn.UNCPath;

                items.Add(item);
            }
            res.EffectItems = items;

            var result = await FSConnectionDao.UpdateConnectoinIsPauseAsync(gPauseNodeIds, req.IsPause);

            List<ManualApprovalFSAuditRecordDto> list = new List<ManualApprovalFSAuditRecordDto>();
            foreach (Guid connId in gPauseNodeIds)
            {
                FSConnection conn = connDict[connId];
                ManualApprovalFSAuditRecordDto record = BuildAuditRecords(conn, req.IsPause);
                list.Add(record);
            }
            FSAuditSinkService.PauseOrResumeFlushAsync(list);

            return res;
        }


        private ManualApprovalFSAuditRecordDto BuildAuditRecords(FSConnection conn, int IsPause)
        {
            ManualApprovalFSAuditRecordDto record = new ManualApprovalFSAuditRecordDto();
            record.NodeId = conn.Id;
            record.NodeName = conn.Name;
            record.AuditLevel = (int)FSAuditLevel.Connection;
            record.ConnectionId = conn.Id.ToString();
            record.ConnectionGroupId = conn.GroupId.ToString();
            record.FullPath = conn.UNCPath;
            record.Status = (int)AuditStatus.Successful;
            record.IsPause = IsPause;
            return record;
        }
    }
}
