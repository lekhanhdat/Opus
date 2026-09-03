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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.Multi_Geo;
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
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSConnectionOwnerType = AvePoint.RA.DB.Model.FSConnectionOwnerType;
using MultiGeoOperationType = AvePoint.RA.RACommonUtility.MultiGeo.MultiGeoOperationType;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/MyHub/[action]")]
    [RecordsAPIFSMyhubAuthorizationFilter(new FSConnectionOwnerType[] { FSConnectionOwnerType.InformationOwner, FSConnectionOwnerType.RecordOwner })]
    public class MyHubApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(MyHubApiController));

        private IRMMyhubServices _IRMMyhubServices;
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService(ref _IRMMyhubServices);

        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        private IRMFileSystemSettingsService _RMFSSettingsService;
        private IRMFileSystemSettingsService RMFSSettingsService => PlatformWindsorManager.GetService(ref _RMFSSettingsService);

        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private IFSConnectionDao _FSConnectionDao;
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        private IRMManualApprovalService _ManualApprovalService;
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);

        private IExplorerDao _ExplorerDao;
        private IExplorerDao ExplorerDao => PlatformWindsorManager.GetService(ref _ExplorerDao);

        public IAccountWrapperService AccountWrapperService = PlatformWindsorManager.GetService<IAccountWrapperService>();

        private IMultiGeoSettingService _MultiGeoSettingService;
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService(ref _MultiGeoSettingService);

        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private const int MAX_CLASSIFY_AMOUNT = 10;
        private (string TimeZoneId, bool IsDaylight) GetTimeZoneInfo()
        {
            var timeZoneId = Request.Headers["X-CLOUD-GOVERNANCE-TIMEZONE"].FirstOrDefault();
            bool.TryParse(Request.Headers["X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"].FirstOrDefault(),
                out var isDaylightSavingTime);

            return (timeZoneId, isDaylightSavingTime);
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.GetNodeIdByConnectionId)]
        public async Task<RMMyhubDriveDirectionItem> GetNodeIdByConnectionId([FromBody] RMMyhubDriveDirectionQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                queryInfo,
                MultiGeoOperationType.MyHubGetNodeIdByConnectionId,
                RMMyhubServices.GetNodeInfoByPartitionKeyAsync,
                (errorType) => errorType switch
                {
                    MultiGeoErrorType.InValidIPRequestError => new RMMyhubDriveDirectionItem
                    {
                        IsValid = false,
                    },
                    _ => new()
                });
        }
        [HttpPost]
        public async Task<RMMyhubDriveItemResult> QueryDrives([FromBody] RMMyhubDriveQueryInfo queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RMMyhubServices.GetMyhubDriveItemsAsync(queryInfo);
        }
        [HttpGet]
        public async Task<RMMyhubDriveVolumeItem> QueryDrivesVolume()
        {
            return await RMMyhubServices.GetDrivesVolumeAsync();
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.QueryTreeFolders)]
        public async Task<object> QueryTreeFolders([FromBody] RMMyhubTreeChildFolderQueryInfo queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubQueryMyhubRootTreeFolderItems,
                    RMMyhubServices.GetMyhubTreeFoldersAsync);
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.QueryDetailTable)]
        public async Task<RMMyhubFolderDetailTableItem> QueryDetailTable([FromBody] RMMyhubFolderDetailTableQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubQueryDetailTable,
                    RMMyhubServices.GetMyhubFolderDetailAsync);
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.QueryFolderAndItems)]
        public async Task<RMMyhubFolderAndFileItemResult> QueryFolderAndItems([FromBody] RMMyhubFolderItemQueryInfo queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubQueryFolderAndItems,
                    RMMyhubServices.GetMyhubFolderAndItemsAsync);
        }

        [HttpPost]
        public async Task<List<RMMyhubFolderStatisticsInfo>> GetFolderStatistics([FromBody] RMMyhubFolderStatisticsQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubGetFolderStatistics,
                    RMMyhubServices.GetFolderStatisticsAsync
                );
        }
        [HttpGet]
        public List<string> ReadAllClassCodeName()
        {
            return RMMyhubServices.ReadAllClassCodeName();
        }

        [HttpPost]
        public async Task<List<RMMyhubClassCodeItem>> ReadClassCodeNameByPartitionKeyIds([FromBody] ReadAllClassCodeNameReq req)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(req.PartitionKeyIds[0],
                    req,
                    MultiGeoOperationType.MyhubReadClassCodeName,
                    request => RMMyhubServices.ReadClassCodeNameByPartitionKeyIds(request));
        }
        [HttpPost]
        public async Task<List<RMMyhubClassCodeCascadeDataDto>> ReadClassifyDataByPartitionKeyIds([FromBody] ReadAllClassCodeNameReq req)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(req.PartitionKeyIds[0],
                    req,
                    MultiGeoOperationType.MyhubReadClassCodeName,
                    request => RMMyhubServices.ReadClassifyDataByPartitionKeyIds(request));
        }
        [HttpGet]
        public List<string> ReadAllCountryCodeName()
        {
            return RMMyhubServices.ReadAllCountryCodeName();
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.ClassifyUpdate)]
        public async Task<ActionResult<List<MyhubClassifyReturnMessage>>> ClassifyUpdate([FromBody] RMMyhubClassifyQueryInfo queryInfo)
        {
            var selectedCount = queryInfo.Id.Length;
            if (selectedCount > MAX_CLASSIFY_AMOUNT)
                return BadRequest(new
                {
                    error = $"Maximum {MAX_CLASSIFY_AMOUNT} files allowed per request to Classify",
                    received = selectedCount
                });
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId[0],
                queryInfo,
                MultiGeoOperationType.MyHubClassifyUpdate,
                RMMyhubServices.UpdateMyhubClassifyAsync);
        }
        [HttpPost]
        public async Task<RMMyhubClassifyDto> QueryClassifyInfo([FromBody] RMMyhubClassifyReturnInfo queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubQueryClassifyInfo,
                    RMMyhubServices.UpdateMyhubClassifyReturnValueAsync);
        }
        [HttpPost]
        public List<string> ReadCountryCodeByClassCode([FromBody] RMMyhubClassifyQueryInfo queryInfo)
        {
            return RMMyhubServices.ReadCountryCodeByClassCode(queryInfo.ClassCode);
        }
        [HttpGet]
        public List<string> GetRetentionType()
        {
            return RMMyhubServices.ReadRetentionType();
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.QueryAuditTrial)]
        public async Task<FSAuditQueryResult> QueryAuditTrial([FromBody] RMMyhubAuditTrialQueryInfo queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.QueryParam?.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubQueryAuditTrial,
                    RMMyhubServices.QueryAuditTrailAsync);
        }
        [HttpGet]
        public RMMyhubAuditTrialFilterItem QueryAuditTrialFilters()
        {
            return RMMyhubServices.QueryAuditTrialFilter();
        }
        [HttpGet]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.GetPermissionByConnectionId)]
        public async Task<RMConnectionPermissions> GetConnectionPermission([FromQuery] Guid connectionId)
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var targetDC = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionId(connectionId);
                if(!string.IsNullOrEmpty(targetDC))
                {
                    if (!await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, targetDC))
                    {
                        logger.Warn($"The login IP is not allowed to access data center [{targetDC}]. Reject the request.");
                    return new RMConnectionPermissions();
                }
            }
                else
                {
                    logger.Info("Current connection belong to main data center, no need to validate login IP.");
                }
            }
            return await RMMyhubServices.GetConnectionPermissionAsync(connectionId);
        }

        [HttpGet]
        public RMConnectionAddUserPageInfo SearchAvaliableOwners([FromQuery] string key)
        {
            return RMMyhubServices.SearchAvaliableOwners(TenantLocalValue.LogonGroupId, key);
        }

        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.UpdateConnectionRecordOwners)]
        public Task<bool> UpdateConnectionRecordOwners([FromBody] RMConnectionRecordOwnerUpdateModel updateModels)
        {
            return RouteMultiGeoApiActionAsync(
                updateModels,
                MultiGeoOperationType.MyHubUpdateConnectionRecordOwners,
                async request =>
                {
                    return await RMMyhubServices.UpdateConnectionRecordOwners(updateModels);
                },
                (request, _) =>
                {
                    return Task.CompletedTask;
                },
                _ => false);
        }
        [HttpPost]
        public async Task<int> GetPendingDisposalVolume([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubGetPendingDisposalVolume,
                    RMMyhubServices.GetPendingDisposalVolumeAsync);
        }
        [HttpPost]
        public async Task<Dictionary<Guid, int>> GetPendingDisposalVolumeDisc([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubGetPendingDisposalVolumeDisc,
                    RMMyhubServices.GetChildFolderPendingDisposalVolumeByNodeIdAsync);
        }
        [HttpPost]
        public async Task<RMMyhubPendingDisposalFolderFilterResult> GetPendingDisposalFolderFilter([FromBody] RMMyhubPendingDisposalFolderFilterQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubGetPendingDisposalFolderFilter,
                     RMMyhubServices.GetPendingDisposalFolderFilterAsync);
        }
        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.GetParameterBeforeUnderReviewQuery)]
        public async Task<RMMyhubParameterBeforePendingDisposalQuery> GetParameterBeforeUnderReviewQuery([FromBody] RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyhubGetParameterBeforeUnderReviewQuery,
                    RMMyhubServices.GetParameterBeforeUnderReviewQueryAsync,
                    (errorType) => errorType switch
                    {
                        MultiGeoErrorType.InValidIPRequestError => new RMMyhubParameterBeforePendingDisposalQuery
                        {
                            IsValid = false,
                        },
                        _ => new()
                    });
        }
        #region File System Dashboard
        [HttpPost]
        public async Task<DashboardJobCreationStatus> RunFSDashboardDataSyncJob([FromBody] FileSystemMyhubSelectedNodeDto selectedNode)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(selectedNode.PartitionKeyId,
                    selectedNode,
                    MultiGeoOperationType.MyHubRunFSDashboardDataSyncJob,
                    (request) =>
                    {
                        try
                        {
                            var creationSuccess = RMMyhubServices.RunFSMyHubDashboardJob(JobRunBy.Control, selectedNode);
                            if (creationSuccess)
                            {
                                return Task.FromResult(DashboardJobCreationStatus.Succeed);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error("An error occurred while running file system dashboard data sync job. Error:{1}", e.ToString());
                        }

                        return Task.FromResult(DashboardJobCreationStatus.Failed);
                    });
        }

        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.GetFSDashboardData)]
        public async Task<FSDashboardInformation> GetFSDashboardData([FromBody] RMMyHubFolderDashboard queryInfo)
        {
            var (timeZoneId, isDaylight) = GetTimeZoneInfo();
            queryInfo.TimeZoneId = timeZoneId;
            queryInfo.IsDaylight = isDaylight;
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryInfo.PartitionKeyId,
                    queryInfo,
                    MultiGeoOperationType.MyHubGetFSDashboardData,
                    async (request) =>
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
                    });
        }
        #endregion

        [HttpGet]
        public async Task<List<FSConnectionPermission>> GetAllConnectionPermission()
        {
            return await RMMyhubServices.GetConnectionPermission();
        }

        [HttpPost]
        [ValidMyhubPermissionParameterFilter(ValidMyhubPermissionParameterFilter.ValidMyhubPermissionParameterType.PauseOrResume)]
        public async Task<RAReturnMessage> PauseOrResume([FromBody] PauseOrResumeReq req)
        {
            if (req?.NodeIds?.Count < 1)
            {
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "NodeIds is required."
                };
            }

            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var groupDataCenters = await FSConnectionGroupDao.GetGroupDCInternalNameByConnectionIdsAsync(req?.NodeIds);
                if(groupDataCenters == null && groupDataCenters.Count == 0)
                {
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "NodeIds is invalid."
                    };
                }
                if(groupDataCenters.Count > 1)
                {
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "NodeIds must belong to the same data center."
                    };
                }
                string connectionDataCenter = groupDataCenters.FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(connectionDataCenter) && !await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, connectionDataCenter))
                {
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "You do not have permission to perform this action from your current location."
                    };
                }
            }

            return await RouteMultiGeoApiActionAsync(
                req,
                MultiGeoOperationType.MyHubPauseOrResume,
                request =>
                {
                    return RMMyhubServices.UpdateConnectoinIsPauseAsync(req);
                },
                request => new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "NodeIds is required."
                });

        }

        #region RCC Report

        [HttpPost]
        public async Task<string> LoadRCCInfosById([FromBody] RMRCCReportInfo reportInfo)
        {
            if(await MultiGeoSettingService.IsEnableMultiGeoFeature() && (reportInfo.Ids == null || reportInfo.Ids.Count == 0))
            {
                return JsonConvert.SerializeObject(new RMRCCReportResult
                {
                    IsEnableMultiGeo = true,
                    Datas = new List<RCCReportContentDto>(),
                });
            }

            string connectionId = reportInfo.PartitionKeyId != null && reportInfo.PartitionKeyId.Count > 0 ? reportInfo.PartitionKeyId[0] : string.Empty;

            return await RouteMultiGeoApiActionByConnectionIdAsync(connectionId,
                reportInfo,
                MultiGeoOperationType.MyHubLoadRCCInfosById,
                async request =>
                {
                    var (timeZoneId, isDaylight) = GetTimeZoneInfo();
                    return JsonConvert.SerializeObject(await ExplorerService.LoadRCCInfoByIdAsync(request, timeZoneId, isDaylight));
                });
        }

        [HttpPost]
        public async Task<RAReturnMessage> GenerateRCCReport([FromBody] RCCReportRequest requestMyhub)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(requestMyhub.ConnectionId.ToString(),
                requestMyhub,
                MultiGeoOperationType.MyHubGenerateRCCReport,
                async reqMyHub =>
                {
                    var result = new RAReturnMessage() { MessageType = RAMessageType.Failed };
                    try
                    {
                        var connGroupId = Guid.Empty;
                        var requestNodes = new List<RCCNode>();
                        foreach (var node in reqMyHub.Nodes)
                        {
                            var connectionGroupId = Guid.Empty;

                            if (reqMyHub.Level == 100)
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

                        var (timeZoneId, isDaylight) = GetTimeZoneInfo();
                        var request = new RCCReportRequest
                        {
                            Nodes = requestNodes,
                            Level = reqMyHub.Level,
                            ConnGroupId = connGroupId,
                            ConnectionId = reqMyHub.ConnectionId,
                            TimeRange = reqMyHub.TimeRange,
                            IsMyHub = true,
                            TimeZoneId = timeZoneId,
                            IsDaylight = isDaylight
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
                });
        }

        #endregion

        #region Export Disposal History


        [HttpPost]
        public async Task<string> LoadDisposalReportData([FromBody] RMDisposalHistoryReportInfo request)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(request.PartitionKeyId,
                request,
                MultiGeoOperationType.MyHubLoadDisposalReportData,
                async req =>
                {
                    var (timeZoneId, isDaylight) = GetTimeZoneInfo();
                    return JsonConvert.SerializeObject(await ExplorerService.LoadDisposalHistoryReportAsync(req, timeZoneId, isDaylight));
                });
        }

        [HttpPost]
        public async Task<RAReturnMessage> GenerateDisposalHistoryReport([FromBody] RA.Contract.ManualApproval.Model.ManualApprovalHistoryOption historyOption)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(historyOption.PartitionKeyId,
                historyOption,
                MultiGeoOperationType.MyHubGenerateDisposalHistoryReport,
                async req =>
                {
                    var originalHost = Request.Headers.Host;
                    if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
                    {
                        string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                        originalHost = Request.Headers.GetHeaderValue(originalHostKey);
                    }
                    var serverUrl = !string.IsNullOrEmpty(originalHost) ? $"https://{originalHost}" : "";
                    req.ServiceUrl = serverUrl;
                    if (string.IsNullOrEmpty(historyOption.FullPath))
                    {
                        return new RAReturnMessage()
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = "FullPath is required."
                        };
                    }
                    if (req.CustomDate != null)
                    {
                        var (timeZoneId, isDaylight) = GetTimeZoneInfo();
                        req.CustomDate.TimeZoneId = timeZoneId;
                        req.CustomDate.IsDaylight = isDaylight;
                    }
                    var validOwnerMsg = await RMMyhubServices.CheckOwnerPermissionAsync(new Guid(historyOption.PartitionKeyId), (int)MyhubReportJobType.HistoryContent);
                    if (validOwnerMsg.MessageType != RAMessageType.Successful)
                    {
                        return validOwnerMsg;
                    }
                    return ManualApprovalService.RunExportHistoryDatasJob(serverUrl, req);
                });
        }

        #endregion

        #region Common

        [HttpPost]
        public async Task<string> DeleteReportContent([FromBody] List<Guid> jobIds, int reportType, string PartitionKeyId)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(PartitionKeyId,
                new RMMyHubDeleteReport { JobIds = jobIds, ReportType = reportType },
                MultiGeoOperationType.MyHubDeleteReportContent,
                async request =>
                {
                    var result = await RMMyhubServices.DeleteReportContentAsync(request.JobIds, request.ReportType);
            return JsonConvert.SerializeObject(result);
                });
        }


        [HttpGet]
        public async Task<IActionResult> CheckJobExists([FromQuery] string connectionId)
        {
            var hasMonitorJob = await RouteMultiGeoApiActionByConnectionIdAsync(connectionId,
                connectionId,
                MultiGeoOperationType.MyHubCheckJobExists,
                async request =>
                {
                    return await JobMonitorService.HasRunningFSSyncDataJobAsync(request);
                });
            return Ok(hasMonitorJob);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadReportContentMyhub([FromBody] RMMyhubReportQueryInfo queryInfo)
        {
            string connectionId = queryInfo.PartitionKeyId != null && queryInfo.PartitionKeyId.Count > 0 ? queryInfo.PartitionKeyId[0] : string.Empty;
            return Ok(await RouteMultiGeoApiActionByConnectionIdAsync(connectionId,
                queryInfo,
                MultiGeoOperationType.MyHubDownloadReportContentMyhub,
                async request =>
                {
                    var result = await RMMyhubServices.DownloadReportContentMyhub(request);
                    return result;
                }));
        }

        #endregion

        #region Helpers

        //Download RCC
        //private FileStreamResult GetValidatedFile(Stream stream, string contentType, string fileName)
        //{
        //    if ((SecurityUtils.IsValidFileName(fileName)))
        //    {
        //        return File(stream, contentType, fileName);
        //    }
        //    throw new Exception("Invalid file name");
        //}

        //private string GetContentType(string path)
        //{
        //    var provider = new FileExtensionContentTypeProvider();
        //    string contentType;

        //    if (!provider.TryGetContentType(path, out contentType))
        //    {
        //        contentType = "application/octet-stream";
        //    }

        //    return contentType;
        //}

        //private async Task<string> ConvertStreamToBase64Async(Stream stream)
        //{
        //    if (stream.CanSeek)
        //        stream.Position = 0;
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await stream.CopyToAsync(memoryStream);
        //        byte[] bytes = memoryStream.ToArray();
        //        return Convert.ToBase64String(bytes);
        //    }
        //}
        #endregion
    }
}
