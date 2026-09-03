/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Myhub;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Myhub.Model;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Myhub.Permission;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.MyHub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/MyHubApi/[action]")]
    public class MyHubResourceApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(MyHubResourceApiController));
        private IRMMyhubServices _rmMyhubServices;
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService(ref _rmMyhubServices);

        private IExplorerService _explorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _explorerService);

        private IRMManualApprovalService _ManualApprovalService;
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);

        private IRMFileSystemSettingsService _RMFSSettingsService;
        private IRMFileSystemSettingsService RMFSSettingsService => PlatformWindsorManager.GetService(ref _RMFSSettingsService);
        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private IExplorerDao _ExplorerDao;
        private IExplorerDao ExplorerDao => PlatformWindsorManager.GetService(ref _ExplorerDao);

        private IFSConnectionDao _FSConnectionDao;
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        private IDownloadDataInfoDao _DownloadDataInfoDao;
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService(ref _DownloadDataInfoDao);

        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IArchivedContentDownloadService _ArchivedContentDownloadService;
        private IArchivedContentDownloadService ArchivedContentDownloadService => PlatformWindsorManager.GetService(ref _ArchivedContentDownloadService);
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);

        [HttpPost]
        public async Task<RMMyhubDriveVolumeItem> QueryDrivesVolume()
        {
            return await RMMyhubServices.GetDrivesVolumeAsync();
        }

        [HttpPost]
        public async Task<RMMyhubDriveDirectionItem> GetNodeIdByConnectionId([FromBody] RMMyhubDriveDirectionQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetNodeInfoByPartitionKeyAsync(queryInfo);
        }

        [HttpPost]
        public async Task<object> QueryMyhubRootTreeFolderItems([FromBody] RMMyhubTreeChildFolderQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetMyhubTreeFoldersAsync(queryInfo);
        }

        [HttpPost]
        public async Task<object> QueryMyhubTreeFolder([FromBody] RMMyhubTreeChildFolderQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetMyhubTreeFoldersAsync(queryInfo);
        }

        [HttpPost]
        public async Task<RMMyhubFolderDetailTableItem> QueryDetailTable([FromBody] RMMyhubFolderDetailTableQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetMyhubFolderDetailAsync(queryInfo);
        }

        [HttpPost]
        public async Task<RMMyhubFolderAndFileItemResult> QueryFolderAndItems([FromBody] RMMyhubFolderItemQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetMyhubFolderAndItemsAsync(queryInfo);
        }

        [HttpPost]
        public async Task<List<MyhubClassifyReturnMessage>> ClassifyUpdate([FromBody] RMMyhubClassifyQueryInfo queryInfo)
        {
            return await RMMyhubServices.UpdateMyhubClassifyAsync(queryInfo);
        }

        [HttpPost]
        public async Task<RMMyhubClassifyDto> QueryClassifyInfo([FromBody] RMMyhubClassifyReturnInfo queryInfo)
        {
            return await RMMyhubServices.UpdateMyhubClassifyReturnValueAsync(queryInfo);
        }

        [HttpPost]
        public async Task<int> GetPendingDisposalVolume([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetPendingDisposalVolumeAsync(queryInfo);
        }

        [HttpPost]
        public async Task<RMMyhubPendingDisposalFolderFilterResult> GetPendingDisposalFolderFilter([FromBody] RMMyhubPendingDisposalFolderFilterQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetPendingDisposalFolderFilterAsync(queryInfo);
        }
        [HttpPost]
        public async Task<RMMyhubParameterBeforePendingDisposalQuery> GetParameterBeforeUnderReviewQuery([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetParameterBeforeUnderReviewQueryAsync(queryInfo);
        }
        [HttpPost]
        public async Task<Dictionary<Guid, int>> GetPendingDisposalVolumeDisc([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetChildFolderPendingDisposalVolumeByNodeIdAsync(queryInfo);
        }

        [HttpPost]
        public DashboardJobCreationStatus RunFSDashboardDataSyncJob([FromBody] FileSystemMyhubSelectedNodeDto selectedNode)
        {
            try
            {
                var creationSuccess = RMMyhubServices.RunFSMyHubDashboardJob(JobRunBy.Control, selectedNode);
                if (creationSuccess)
                {
                    return DashboardJobCreationStatus.Succeed;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while running file system dashboard data sync job. Error:{1}", e.ToString());
            }

            return DashboardJobCreationStatus.Failed;
        }

        [HttpPost]
        public async Task<FSDashboardInformation> GetFSDashboardData([FromBody] RMMyHubFolderDashboard queryInfo)
        {
            try
            {
                return await RMMyhubServices.GetMyHubDashboardDataAsync(queryInfo);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting file system dashboard data. Error:{1}", e.ToString());
                return new FSDashboardInformation();
            }
        }

        [HttpPost]
        public async Task<List<RMMyhubClassCodeItem>> ReadClassCodeNameByPartitionKeyIds([FromBody] ReadAllClassCodeNameReq req)
        {
            return await RMMyhubServices.ReadClassCodeNameByPartitionKeyIds(req);
        }
        [HttpPost]
        public async Task<List<RMMyhubClassCodeCascadeDataDto>> ReadClassifyDataByPartitionKeyIds([FromBody] ReadAllClassCodeNameReq req)
        {
            return await RMMyhubServices.ReadClassifyDataByPartitionKeyIds(req);
        }
        [HttpPost]
        public Task<bool> UpdateConnectionRecordOwners([FromBody] RMConnectionRecordOwnerUpdateModel updateModels)
        {
            return RMMyhubServices.UpdateConnectionRecordOwnersForOtherDC(updateModels);
        }

        [HttpPost]
        public Task<RAReturnMessage> PauseOrResume([FromBody] PauseOrResumeReq req)
        {
            return RMMyhubServices.UpdateConnectoinIsPauseAsync(req);
        }

        [HttpPost]
        public async Task<string> LoadRCCInfosById([FromBody] RMRCCReportInfo reportInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            return JsonConvert.SerializeObject(await ExplorerService.LoadRCCInfoByIdAsync(reportInfo, timeZoneId, isDaylight));
        }

        [HttpPost]
        public async Task<RAReturnMessage> GenerateRCCReport([FromBody] RCCReportRequest requestMyhub)
        {
            var result = new RAReturnMessage() { MessageType = RAMessageType.Failed };
            try
            {
                var connGroupId = Guid.Empty;
                var requestNodes = new List<RCCNode>();
                foreach (var node in requestMyhub.Nodes)
                {
                    var connectionGroupId = Guid.Empty;

                    if (requestMyhub.Level == 100)
                    {
                        connGroupId = await FSConnectionDao.GetConnectionGroupIdByConnectionIdAsync(Guid.Parse(node.Id.ToString()));
                    }
                    else
                    {
                        var scopeId = ExplorerDao.GetFSRecordById(Guid.Parse(node.Id.ToString()))?.AveSiteId;
                        if (scopeId != null)
                        {
                            connGroupId = await FSConnectionDao.GetConnectionGroupIdByConnectionIdAsync(Guid.Parse(scopeId));
                        }
                    }

                    if (RMMyhubServices.CurrentNodeIsDisableDownloadRCC(connGroupId.ToString(), node.FullPath))
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = "Have node is not Allow IO/RO download RCC report, not start job";
                        return result;
                    }
                    var requestNode = new RCCNode
                    {
                        Id = Guid.Parse(node.Id.ToString()),
                        FullPath = node.FullPath,
                        Name = node.Name
                    };

                    requestNodes.Add(requestNode);
                }

                var request = new RCCReportRequest
                {
                    Nodes = requestNodes,
                    Level = requestMyhub.Level,
                    ConnGroupId = connGroupId,
                    ConnectionId = requestMyhub.ConnectionId,
                    TimeRange = requestMyhub.TimeRange,
                    IsMyHub = true
                };
                if (!RMKeyValueDao.IsEnableJPMCFileSystemFeature())
                {
                    result.ErrorMessage = "This feature is not supported in Non-JPMC environment.";
                    return result;
                }

                if (request.TimeRange == null)
                {
                    result.ErrorMessage = "Time range is required.";
                    return result;
                }

                if (request.TimeRange.PresetType == 0 && (string.IsNullOrEmpty(request.TimeRange.StartDate) || string.IsNullOrEmpty(request.TimeRange.EndDate)))
                {
                    result.ErrorMessage = "Start date and end date are required for custom time range.";
                    return result;
                }

                result = RMFSSettingsService.RunDownloadRCCReportJob(request, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                logger.Error($"An unexpected error occurred in GenerateRCCReport: {ex}");
                result.ErrorMessage = "An unexpected error occurred while generating the report.";
            }
            return result;
        }

        [HttpPost]
        public async Task<string> LoadDisposalReportData([FromBody] RMDisposalHistoryReportInfo request)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            return JsonConvert.SerializeObject(await ExplorerService.LoadDisposalHistoryReportAsync(request, timeZoneId, isDaylight));
        }

        [HttpPost]
        public RAReturnMessage GenerateDisposalHistoryReport([FromBody] RA.Contract.ManualApproval.Model.ManualApprovalHistoryOption historyOption)
        {
            var originalHost = Request.Headers.Host;
            if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
            {
                string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                originalHost = Request.Headers.GetHeaderValue(originalHostKey);
            }
            var serverUrl = !string.IsNullOrEmpty(originalHost) ? $"https://{originalHost}" : "";

            if (string.IsNullOrEmpty(historyOption.FullPath))
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "FullPath is required."
                };
            }
            if (historyOption.CustomDate != null)
            {
                var (timeZoneId, isDaylight) = GetTimeZoneInfo();
                historyOption.CustomDate.TimeZoneId = timeZoneId;
                historyOption.CustomDate.IsDaylight = isDaylight;
            }
            var validOwnerMsg = RMMyhubServices.CheckOwnerPermissionAsync(new Guid(historyOption.PartitionKeyId), (int)MyhubReportJobType.HistoryContent).Result;
            if (validOwnerMsg.MessageType != RAMessageType.Successful)
            {
                return validOwnerMsg;
            }
            historyOption.ServiceUrl = serverUrl;
            return ManualApprovalService.RunExportHistoryDatasJob(serverUrl, historyOption);
        }

        [HttpPost]
        public string QueryDriveSettings([FromBody] List<RMMyhubDriveQuerySettings> queryInfos)
        {
            return JsonConvert.SerializeObject(RMMyhubServices.GetMyhubDriveSettings(queryInfos));
        }

        [HttpPost]
        public async Task<RMMyhubReportDownloadResponse> DownloadReportContentMyhub([FromBody] RMMyhubReportQueryInfo queryInfo)
        {
            return await RMMyhubServices.DownloadReportContentMyhub(queryInfo);
        }

        [HttpPost]
        public async Task<FSAuditQueryResult> QueryAuditTrial([FromBody] RMMyhubAuditTrialQueryInfo queryInfo)
        {
            return await RMMyhubServices.QueryAuditTrailAsync(queryInfo);
        }

        [HttpPost]
        public async Task<List<RMMyhubFolderStatisticsInfo>> GetFolderStatistics([FromBody] RMMyhubFolderStatisticsQueryInfo queryInfo)
        {
            return await RMMyhubServices.GetFolderStatisticsAsync(queryInfo);
        }

        [HttpPost]
        public async Task<bool> CheckJobExists([FromBody] string connectionId)
        {
            return await JobMonitorService.HasRunningFSSyncDataJobAsync(connectionId);
        }

        [HttpPost]
        public async Task<string> DeleteReportContent([FromBody] RMMyHubDeleteReport request)
        {
            var result = await RMMyhubServices.DeleteReportContentAsync(request.JobIds, request.ReportType);
            return JsonConvert.SerializeObject(result);
        }

        private (string TimeZoneId, bool IsDaylight) GetTimeZoneInfo()
        {
            var timeZoneId = Request.Headers["X-CLOUD-GOVERNANCE-TIMEZONE"].FirstOrDefault();
            bool.TryParse(Request.Headers["X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"].FirstOrDefault(),
                out var isDaylightSavingTime);

            return (timeZoneId, isDaylightSavingTime);
        }
    }
}
