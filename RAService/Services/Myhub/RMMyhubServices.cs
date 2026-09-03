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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
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
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.Myhub.Actions;
using AvePoint.RA.Service.Services.Myhub.Views;
using AvePoint.RA.Service.Services.MyHub.Actions;
using AvePoint.RA.Service.Services.MyHub.NewMethods;
using AvePoint.RA.Service.Services.MyHub.Views.ClassCode;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.Wrapper.Restore;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using HSMCommon.DeploymentXML;
using Microsoft.Data.SqlClient;
using Microsoft.Graph.Beta.Models.ManagedTenants;
using Newtonsoft.Json;
using PnP.Framework.Extensions;
using PnP.Framework.Utilities;
using RazorEngine.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Service.Services.MyHub
{
    [Audit]
    public class RMMyhubServices : RMServiceBase, IRMMyhubServices
    {
        RALogger logger = RALogger.GetInstance(typeof(RMMyhubServices));


        private IRMFileSystemRegisterService FSRegisterService = PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        public IRMFSConnectionAndOwnerRelationshipDao RMFSConnectionAndOwnerRelationshipDao = PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        public IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();

        private RMMyhubQueryRecordsMethod _recordStore;
        private RMMyhubQueryRecordsMethod RecordStore => _recordStore ??= new RMMyhubQueryRecordsMethod();
        private RMMyhubDriveMethod _driveMethod;
        private RMMyhubDriveMethod DriveMethod => _driveMethod ??= new RMMyhubDriveMethod();
        private RMMyhubFolderTreeMethod _folderMethod;
        private RMMyhubFolderTreeMethod FolderMethod => _folderMethod ??= new RMMyhubFolderTreeMethod();
        private RMMyhubFolderAndItemMethod _folderAndItemMethod;
        private RMMyhubFolderAndItemMethod FolderAndItemMethod => _folderAndItemMethod ??= new RMMyhubFolderAndItemMethod();

        private RMMyhubFolderDetailMethod _detailMethod;
        private RMMyhubFolderDetailMethod DetailMethod => _detailMethod ??= new RMMyhubFolderDetailMethod();

        private RMMyhubReadClassCodeMethod _readClassCodeMethod;
        private RMMyhubReadClassCodeMethod ReadClassCodeMethod => _readClassCodeMethod ??= new RMMyhubReadClassCodeMethod();

        private RMMyhubVolumeMethod _volumeMethod;
        private RMMyhubVolumeMethod VolumeMethod => _volumeMethod ??= new RMMyhubVolumeMethod();

        private RMMyhubClassifyMethodService _classifyMethod;
        private RMMyhubClassifyMethodService ClassifyMethod => _classifyMethod ??= new RMMyhubClassifyMethodService();

        private RMMyhubAuditTrialMethod _auditTrialMethod;
        private RMMyhubAuditTrialMethod AuditTrialMethod => _auditTrialMethod ??= new RMMyhubAuditTrialMethod();

        private RMMyhubRunActionMethod _actionMethod;
        private RMMyhubRunActionMethod ActionMethod => _actionMethod ??= new RMMyhubRunActionMethod();
        private RMMyhubPendingDisposalFolderFilterMethod _pendingDisposalFolderFilterMethod;
        private RMMyhubPendingDisposalFolderFilterMethod PendingDisposalFolderFilterMethod => _pendingDisposalFolderFilterMethod ??= new RMMyhubPendingDisposalFolderFilterMethod();
        private ICreateAndDestryoedReportService CreateAndDestryoedReportService => PlatformWindsorManager.GetService<ICreateAndDestryoedReportService>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IFSMyHubDashboardDao FSMyHubDashboardDao => PlatformWindsorManager.GetService<IFSMyHubDashboardDao>();
        private IGeneralSettingService GeneralService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public IAccountWrapperService AccountWrapperService = PlatformWindsorManager.GetService<IAccountWrapperService>();
        public ILnkUserGroupDao LnkUserGroupDao = PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        private IUserService UserServices = PlatformWindsorManager.GetService<IUserService>();

        private IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private static ManualApprovalRecordRepository _repository => new ManualApprovalRecordRepository();
        private IRMFSConnectionAndOwnerRelationshipDao FSConnectionOwnerDao => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IUserService UserService = PlatformWindsorManager.GetService<IUserService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IArchivedContentDownloadService ArchivedContentDownloadService => PlatformWindsorManager.GetService<IArchivedContentDownloadService>();
        private IRMMyhubAsyncAuditServices RMMyhubAsyncAuditServices = PlatformWindsorManager.GetService<IRMMyhubAsyncAuditServices>();
        private IRMFSConnectionAndOwnerRelationshipDao RMFSConnAndOwnerRela => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        #region ClassCode
        public List<string> ReadAllClassCodeName()
        {
            return ReadClassCodeMethod.ReadAllClassCodeName();
        }

        public async Task<List<RMMyhubClassCodeItem>> ReadClassCodeNameByPartitionKeyIds(ReadAllClassCodeNameReq req)
        {
            return await ReadClassCodeMethod.ReadAllClassCodeNameByTerms(req);
        }
        public async Task<List<RMMyhubClassCodeCascadeDataDto>> ReadClassifyDataByPartitionKeyIds(ReadAllClassCodeNameReq req)
        {
            return await ReadClassCodeMethod.ReadAllClassifyDataByTerms(req);
        }
        public List<string> ReadAllCountryCodeName()
        {
            return ReadClassCodeMethod.ReadAllCountryCodeNameByTerms();
        }
        #endregion
        public async Task<RMMyhubPendingDisposalFolderFilterResult> GetPendingDisposalFolderFilterAsync(RMMyhubPendingDisposalFolderFilterQueryInfo queryInfo)
        {
            var fullPath = await GetPendingDisposalFolderFilterPathAsync(queryInfo.PartitionKeyId, queryInfo.NodeId, true);
            var query = PendingDisposalFolderFilterMethod.GetAllPendingDisposalFolderFilter(queryInfo, fullPath);
            var (results, continuationToken) = await RecordStore.QueryAsync<RMMyhubPendingDisposalFolderFilterItem>(query.sql, query.parameter, queryInfo.ContinuationToken, queryInfo.PageSize);
            return new RMMyhubPendingDisposalFolderFilterResult
            {
                Items = results,
                ContinuationToken = continuationToken
            };
        }
        public async Task<string> GetPendingDisposalFolderFilterPathAsync(string partitionKeyId, string nodeId, bool isFullPath = false)
        {
            var queryPath = PendingDisposalFolderFilterMethod.GetPendingDisposalFolderFilterPath(partitionKeyId, nodeId, isFullPath);
            var path = await RecordStore.QuerySingleAsync<string>(queryPath.sql, queryPath.parameter);
            return path;
        }
        public async Task<Dictionary<Guid, int>> GetChildFolderPendingDisposalVolumeByNodeIdAsync(RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            try
            {
                var result = await VolumeMethod.GetChildFoldersVolumeAsync(queryInfo.PartitionKeyId, queryInfo.NodeId);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetPendingDisposalVolumeByNodeIdsAsync {ex}");
                return new Dictionary<Guid, int>();
            }
        }
        public async Task<int> GetPendingDisposalVolumeAsync(RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            try
            {
                return await GetDisposalVolumeAsync(queryInfo.PartitionKeyId, queryInfo.NodeId);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetPendingDisposalVolumeAsync {ex}");
                return -1;
            }
        }
        private async Task<int> GetDisposalVolumeAsync(string partitonKeyId, Guid NodeId)
        {
            var queryFullPath = VolumeMethod.GetPendingDisposalPath(partitonKeyId, NodeId);
            var fullPath = await RecordStore.QuerySingleAsync<string>(queryFullPath.sql, queryFullPath.parameter);
            return await GetDisposalVolumeByFullPathAsync(partitonKeyId, fullPath);
        }
        private async Task<int> GetDisposalVolumeByFullPathAsync(string partitonKeyId, string fullPath)
        {
            var query = VolumeMethod.GetPendingDisposalVolume(partitonKeyId, fullPath);
            var result = await RecordStore.QuerySingleAsync<int>(query.sql, query.parameter);
            return result;
        }
        #region Drives
        public async Task<RMMyhubDriveVolumeItem> GetDrivesVolumeAsync()
        {
            try
            {
                if (await IsMainDCAndMultiGeoEnabled())
                {
                    return await GetDrivesVolumeFromMultiGeoAsync();
                }
                var query = VolumeMethod.GetDrivesVolume();
                var stopwatch = Stopwatch.StartNew();
                var result = await RecordStore.QuerySingleAsync<dynamic>(query.sql, query.parameter);
                stopwatch.Stop();
                logger.Info($"[DrivesVolume Performance] QuerySingleAsync completed in {stopwatch.ElapsedMilliseconds} ms");
                return new RMMyhubDriveVolumeItem
                {
                    FileVolume = result?.FileVolume ?? 0,
                    FolderVolume = result?.FolderVolume ?? 0
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetDrivesVolumeAsync {ex}");
                return new RMMyhubDriveVolumeItem { FileVolume = -1, FolderVolume = -1 };
            }

        }

        private async Task<RMMyhubDriveVolumeItem> GetDrivesVolumeFromMultiGeoAsync()
        {
            var result = new RMMyhubDriveVolumeItem { FileVolume = 0, FolderVolume = 0 };

            var queryMainDC = VolumeMethod.GetDrivesVolume();
            var stopwatch = Stopwatch.StartNew();
            var maiunDCResult = await RecordStore.QuerySingleAsync<dynamic>(queryMainDC.sql, queryMainDC.parameter);
            stopwatch.Stop();
            logger.Info($"[DrivesVolume FromMultiGeo Performance] QuerySingleAsync completed in {stopwatch.ElapsedMilliseconds} ms");
            result.FileVolume += maiunDCResult?.FileVolume != null ? Convert.ToInt64(maiunDCResult.FileVolume) : 0;
            result.FolderVolume += maiunDCResult?.FolderVolume != null ? Convert.ToInt64(maiunDCResult.FolderVolume) : 0;
            stopwatch.Restart();
            var supportedDCs = (await MultiGeoDataCenterService.GetDCsSupported()).Select(dc => dc.DCInternalName);
            stopwatch.Stop();
            logger.Info($"[DrivesVolume FromMultiGeo Performance] GetDCsSupported completed in {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();
            var otherDCDataResponse = await RAMultiGeoClient.RouteApiActionAsync<RMMyhubDriveVolumeItem>(MultiGeoOperationType.MyHubQueryDrivesVolume, supportedDCs, true);
            stopwatch.Stop();
            logger.Info($"[DrivesVolume FromMultiGeo Performance] RouteApiActionAsync completed in {stopwatch.ElapsedMilliseconds} ms");
            foreach (var dcResult in otherDCDataResponse)
            {
                if (dcResult.Value != null)
                {
                    result.FileVolume += dcResult.Value.FileVolume;
                    result.FolderVolume += dcResult.Value.FolderVolume;
                }
                else
                {
                    logger.Error($"Get drive volume from DC {dcResult.Key.LogBase64()} response is null.");
                }
            }
            return result;
        }

        private async Task<bool> IsMainDCAndMultiGeoEnabled()
        {
            return MultiGeoDataCenterService.IsMainDC() && await MultiGeoSettingService.IsEnableMultiGeoFeature();
        }
        public async Task<RMMyhubDriveDirectionItem> GetNodeInfoByPartitionKeyAsync(RMMyhubDriveDirectionQueryInfo queryInfo)
        {
            var stopwatch = Stopwatch.StartNew();
            var nodeInfo = await DriveMethod.BaseGetNodeInfoByPartitionKeyAsync(queryInfo.PartitionKeyId);
            stopwatch.Stop();
            logger.Info($"[GetNodeInfo Performance] BaseGetNodeInfoByPartitionKeyAsync completed in {stopwatch.ElapsedMilliseconds} ms");
            if (nodeInfo == null)
            {
                return new RMMyhubDriveDirectionItem
                {
                    IsSynced = false,
                    NodeId = string.Empty,
                    PartitionKeyId = queryInfo.PartitionKeyId
                };
            }
            stopwatch.Restart();
            var connectionInfo = FSConnectionDao.GetConnectionById(Guid.Parse(queryInfo.PartitionKeyId));
            stopwatch.Stop();
            logger.Info($"[GetNodeInfo Performance] GetConnectionById completed in {stopwatch.ElapsedMilliseconds} ms");
            return new RMMyhubDriveDirectionItem
            {
                IsSynced = true,
                NodeId = nodeInfo.NodeId,
                PartitionKeyId = queryInfo.PartitionKeyId,
                Name = connectionInfo.Name,
                FullPath = nodeInfo.FullPath,
                Id = connectionInfo.JPMCConnectionId,
                IsPause = connectionInfo.IsPause
            };
        }
        public async Task<RMMyhubDriveItemResult> GetMyhubDriveItemsAsync(RMMyhubDriveQueryInfo queryInfo)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                stopwatch.Stop();
                logger.Info($"[DriveItems Performance] GetGeneralSettingAsync completed in {stopwatch.ElapsedMilliseconds} ms");
                var timeZoneId = queryInfo.TimeZoneId == null ? generalSetting.TimeZoneId : DateTimeUtil.AllTimeZones[Convert.ToInt32(queryInfo.TimeZoneId)];
                stopwatch.Restart();
                var tiz = GeneralSettingConfig.GetTimeZoneInforById(timeZoneId);
                stopwatch.Stop();
                logger.Info($"[DriveItems Performance] GetTimeZoneInforById completed in {stopwatch.ElapsedMilliseconds} ms");
                queryInfo.TimeOffSet = tiz.BaseUtcOffset;
                var (groupDict, results, hasMore, Count) = await QueryDriveRecordsAsync(queryInfo);
                var driveItemResult = await ConvertToDriveItem(results, groupDict, generalSetting, queryInfo.TimeZoneId, queryInfo.IsDaylight);
                driveItemResult.HasMore = hasMore;
                driveItemResult.Count = Count;
                driveItemResult.TimeOffSet = tiz.BaseUtcOffset;
                return driveItemResult;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetMyhubDriveItemsAsync {ex}");
                return new RMMyhubDriveItemResult
                {
                    Items = new List<RMMyhubDriveItem>(),
                    HasMore = false
                };
            }

        }
        private async Task<(Dictionary<Guid, dynamic> groupNameDict, List<FSConnection> result, bool hasMore, int Count)> QueryDriveRecordsAsync(RMMyhubDriveQueryInfo queryInfo)
        {
            var userIds = new List<int>();
            try
            {
                userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get removed/group ids for user [{TenantLocalValue.LogonUserId}]. Fallback to active user only. Error: {ex}");
            }
            var stopwatch = Stopwatch.StartNew();
            var driveList = await FSConnectionDao.QueryConnectionPaginationAsync(userIds, queryInfo);
            var groupIds = driveList.Items.Select(x => x.GroupId).Distinct().ToList();
            stopwatch.Stop();
            logger.Info($"[DriveItems Performance] QueryConnectionPaginationAsync completed in {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Restart();
            var groups = FSConnectionGroupDao.GetGroupByIds(groupIds);
            var groupDict = groups.ToDictionary(g => g.Id, g => (dynamic)new { g.Name, g.DCInternalName });
            stopwatch.Stop();
            logger.Info($"[DriveItems Performance] GetGroupByIds completed in {stopwatch.ElapsedMilliseconds} ms");
            return (groupDict, driveList.Items, driveList.HasMore, driveList.Count);
        }
        public async Task<RMMyhubDriveItemResult> ConvertToDriveItem(List<FSConnection> results, Dictionary<Guid, dynamic> groupDisc, GeneralSettingModel genernalSetting, string timeZoneId, bool isDaylight)
        {
            var driveItems = new List<RMMyhubDriveItem>();
            if (results == null || results.Count == 0)
            {
                return new RMMyhubDriveItemResult
                {
                    Items = driveItems,
                    HasMore = false
                };
            }
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), genernalSetting.DataFormatId), true)];
            List<Guid> ids = results.Select(i => i.Id).ToList();
            Dictionary<Guid, int> statusMap = await getApprovalStatusByIds(ids);

            if (await IsMainDCAndMultiGeoEnabled())
            {
                Dictionary<Guid, List<RMMyhubDriveQuerySettings>> driveQuerySettingRequests = results.Select(r => new RMMyhubDriveQuerySettings
                {
                    ConnectionGroupId = r.GroupId,
                    UNCPath = r.UNCPath,
                    ConnectionId = r.Id,
                }).GroupBy(r => r.ConnectionGroupId)
                .ToDictionary(g => g.Key, g => g.ToList());
                Dictionary<string, RMMyhubDriveSettings> driveSettingDCDataDic = new();
                string currentDC = string.Empty;
                string keyFormat = "{0}-{1}-{2}";
                string key = string.Empty;
                var stopwatch = new Stopwatch();
                var mainDCAndMultiGeoEnabledPerformanceList = new List<long>();
                foreach (var request in driveQuerySettingRequests)
                {
                    stopwatch.Restart();
                    groupDisc.TryGetValue(request.Key, out var group);

                    if (string.IsNullOrEmpty(group.DCInternalName))
                    {
                        var valueList = request.Value.Select(v => v.ConnectionGroupId.ToString()).ToList();
                        var enableRecordDict = BuildDisableStatusDictionary(valueList);
                        var enableRCCDict = BuildDisableDownloadRCCStatusDictionary(valueList);
                        foreach (var query in request.Value)
                        {
                            key = string.Format(keyFormat, query.UNCPath, "MainDC", query.ConnectionGroupId);
                            if (driveSettingDCDataDic.ContainsKey(key))
                            {
                                logger.Warn("UNC Path already exsit in driveSettingDCDataDic: " + key.LogBase64());
                            }
                            else
                            {
                                logger.Info("Add result into driveSettingDCDataDic: " + key.LogBase64());
                                driveSettingDCDataDic[key] = new RMMyhubDriveSettings
                                {
                                    EnableRecordManagement = !CurrentConnectionIsDisable(query.ConnectionId, query.ConnectionGroupId, enableRecordDict),
                                    IsAllowDownloadRCC = CurrentConnectionIsDisableDownloadRCC(query.ConnectionId, query.ConnectionGroupId, enableRCCDict)
                                };
                            }
                        }
                    }
                    else
                    {
                        var jsonResult = await RAMultiGeoClient.RouteToTagertDC<List<RMMyhubDriveQuerySettings>, string>(group.DCInternalName,
                            request.Value, MultiGeoOperationType.MyHubQueryDriveSettings.ToString());

                        if (string.IsNullOrEmpty(jsonResult))
                        {
                            logger.Warn($"Get drive settings from DC {group.DCInternalName} response is null.");
                            continue;
                        }

                        Dictionary<string, RMMyhubDriveSettings> dcResult = JsonConvert.DeserializeObject<Dictionary<string, RMMyhubDriveSettings>>(jsonResult);
                        foreach (var result in dcResult)
                        {
                            key = string.Format(keyFormat, result.Key, group.DCInternalName, request.Key);
                            if (!driveSettingDCDataDic.ContainsKey(key))
                            {
                                logger.Info("Add result into driveSettingDCDataDic: " + key.LogBase64());
                                driveSettingDCDataDic[key] = result.Value;
                            }
                            else
                            {
                                logger.Warn("UNC Path already exsit in driveSettingDCDataDic: " + key.LogBase64());
                            }
                        }
                    }
                    stopwatch.Stop();
                    mainDCAndMultiGeoEnabledPerformanceList.Add(stopwatch.ElapsedMilliseconds);
                    if (mainDCAndMultiGeoEnabledPerformanceList.Any())
                    {
                        var avg = mainDCAndMultiGeoEnabledPerformanceList.Average();
                        var max = mainDCAndMultiGeoEnabledPerformanceList.Max();
                        var min = mainDCAndMultiGeoEnabledPerformanceList.Min();
                        var p95 = mainDCAndMultiGeoEnabledPerformanceList.OrderBy(x => x).Skip((int)(mainDCAndMultiGeoEnabledPerformanceList.Count * 0.95)).FirstOrDefault();

                        logger.Info($"[DriveItems Performance] MainDCAndMultiGeoEnabled: " +
                                    $"Count={mainDCAndMultiGeoEnabledPerformanceList.Count}, " +
                                    $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
                    }
                }
                RMMyhubDriveSettings driveSettings = null;
                Dictionary<string, bool> validIPCache = new Dictionary<string, bool>();
                var mainDCAndMultiGeoEnabledResultPerformanceList = new List<long>();
                foreach (var result in results)
                {
                    stopwatch.Restart();
                    groupDisc.TryGetValue(result.GroupId, out var group);
                    statusMap.TryGetValue(result.Id, out int approvalStatus);
                    key = string.Format(keyFormat, result.UNCPath, string.IsNullOrEmpty(group.DCInternalName) ? "MainDC" : group.DCInternalName, result.GroupId);

                    if (!driveSettingDCDataDic.TryGetValue(key, out driveSettings))
                    {
                        driveSettings = null;
                    }

                    if (driveSettings == null) logger.Warn($"Can not get driveSettings by key {key.LogBase64()}");

                    driveItems.Add(new RMMyhubDriveItem
                    {
                        NodeId = result.Id,
                        Name = result.Name,
                        Id = result.JPMCConnectionId,
                        Path = result.UNCPath,
                        Group = group.Name,
                        DCInternalName = string.IsNullOrEmpty(group.DCInternalName) ? string.Empty : group.DCInternalName,
                        LastSyncTime = result.LastSyncTime > 0 ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, result.LastSyncTime, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, result.LastSyncTime, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime)
                        : null,
                        PartitionKeyId = result.Id.ToString(),
                        IsPause = result.IsPause,
                        EnableRecordManagement = driveSettings?.EnableRecordManagement ?? true,
                        IsAllowDownloadRCC = driveSettings?.IsAllowDownloadRCC ?? false,
                        IsValidConnectionIp = await IsValidConnectionIp(group.DCInternalName)
                    });
                    stopwatch.Stop();
                    mainDCAndMultiGeoEnabledResultPerformanceList.Add(stopwatch.ElapsedMilliseconds);
                }
                if (mainDCAndMultiGeoEnabledResultPerformanceList.Any())
                {
                    var avg = mainDCAndMultiGeoEnabledResultPerformanceList.Average();
                    var max = mainDCAndMultiGeoEnabledResultPerformanceList.Max();
                    var min = mainDCAndMultiGeoEnabledResultPerformanceList.Min();
                    var p95 = mainDCAndMultiGeoEnabledResultPerformanceList.OrderBy(x => x).Skip((int)(mainDCAndMultiGeoEnabledResultPerformanceList.Count * 0.95)).FirstOrDefault();

                    logger.Info($"[DriveItems Performance] MainDCAndMultiGeoEnabledResult: " +
                                $"Count={mainDCAndMultiGeoEnabledResultPerformanceList.Count}, " +
                                $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
                }
                async Task<bool> IsValidConnectionIp(string dcInternalName)
                {
                    if (string.IsNullOrEmpty(dcInternalName))
                    {
                        return true;
                    }
                    if (validIPCache.TryGetValue(dcInternalName, out bool isValid))
                    {
                        return isValid;
                    }
                    var isValidIp = await MultiGeoSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, dcInternalName);
                    validIPCache[dcInternalName] = isValidIp;
                    return isValidIp;
                }

            }
            else
            {
                var valueList = results.Select(c => c.GroupId.ToString()).ToList();
                var stopwatch = Stopwatch.StartNew();
                var enableRecordDict = BuildDisableStatusDictionary(valueList);
                var enableRCCDict = BuildDisableDownloadRCCStatusDictionary(valueList);
                logger.Info($"[DriveItems Performance] Build dictionaries with no Multi-GEO completed in {stopwatch.ElapsedMilliseconds} ms");
                stopwatch.Stop();
                var performanceList = new List<long>();
                foreach (var result in results)
                {
                    stopwatch.Restart();
                    groupDisc.TryGetValue(result.GroupId, out var group);
                    statusMap.TryGetValue(result.Id, out int approvalStatus);
                    driveItems.Add(new RMMyhubDriveItem
                    {
                        NodeId = result.Id,
                        Name = result.Name,
                        Id = result.JPMCConnectionId,
                        Path = result.UNCPath,
                        Group = group.Name,
                        DCInternalName = string.IsNullOrEmpty(group.DCInternalName) ? string.Empty : group.DCInternalName,
                        LastSyncTime = result.LastSyncTime > 0 ? (string.IsNullOrEmpty(timeZoneId)
                            ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, result.LastSyncTime, true).SimplifyFormatTime
                            : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, result.LastSyncTime, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime)
                            : null,
                        PartitionKeyId = result.Id.ToString(),
                        //TermSetId = termSetId,
                        IsPause = result.IsPause,
                        EnableRecordManagement = !CurrentConnectionIsDisable(result.Id, result.GroupId, enableRecordDict),
                        IsAllowDownloadRCC = CurrentConnectionIsDisableDownloadRCC(result.Id, result.GroupId, enableRCCDict),
                        IsValidConnectionIp = true
                    });
                    stopwatch.Stop();
                    performanceList.Add(stopwatch.ElapsedMilliseconds);
                }
                if (performanceList.Any())
                {
                    var avg = performanceList.Average();
                    var max = performanceList.Max();
                    var min = performanceList.Min();
                    var p95 = performanceList.OrderBy(x => x).Skip((int)(performanceList.Count * 0.95)).FirstOrDefault();

                    logger.Info($"[DriveItems Performance] Result with no Multi-GEO : " +
                                $"Count={performanceList.Count}, " +
                                $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
                }
            }
            return new RMMyhubDriveItemResult
            {
                Items = driveItems
            };
        }
        public async Task<Dictionary<Guid, int>> getApprovalStatusByIds(List<Guid> ids)
        {
            List<ManualApprovalRecord> folderFileRecords = await _repository.QueryItemsAsync(
                                        record => ids.Contains(record.NodeId));
            Dictionary<Guid, int> statusMap = folderFileRecords.ToDictionary(x => x.NodeId, x => x.ManualApprovedStatus);
            return statusMap;
        }
        #endregion

        #region Folder Tree
        public async Task<RMMyhubTreeFolderItemResult> GetMyhubTreeFoldersAsync(RMMyhubTreeChildFolderQueryInfo queryInfo)
        {
            try
            {
                var query = FolderMethod.BuildQuery(queryInfo);

                var stopwatch = Stopwatch.StartNew();
                var cosmosQueryTask = RecordStore.QueryAsync<RMMyhubTreeFolderItem>(query.sql, query.parameters, queryInfo.ContinuationToken, queryInfo.PageSize);
                var generalSettingTask = GeneralSettingService.GetGeneralSettingAsync();
                await Task.WhenAll(cosmosQueryTask, generalSettingTask);
                stopwatch.Stop();
                logger.Info($"[FolderTree Performance] Parallel Cosmos Query & Settings fetch completed in {stopwatch.ElapsedMilliseconds} ms");

                var (resultsFromCosmos, continuationToken) = cosmosQueryTask.Result;
                var generalSetting = generalSettingTask.Result;

                var folderItems = await ConvertToTreeFolderItem(resultsFromCosmos, generalSetting, queryInfo.TimeZoneId, queryInfo.IsDaylight, queryInfo.RootFolderId != Guid.Empty);

                folderItems.HasMore = !string.IsNullOrEmpty(continuationToken);
                folderItems.ContinuationToken = continuationToken;

                return folderItems;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while GetMyhubTreeFoldersAsync {ex}");
                return new RMMyhubTreeFolderItemResult
                {
                    Items = new List<RMMyhubTreeFolderItem>(),
                    HasMore = false,
                    ContinuationToken = null
                };
            }
        }
        private async Task<int> GetChildFolderCountAsync(Guid parentId, string partitionKeyId)
        {
            var sql = FolderMethod.IsHasChildrenSql();
            var parameters = FolderMethod.IsHasChildrenSqlParameters(parentId, partitionKeyId);
            var resultsFromCosmos = await RecordStore.QuerySingleAsync<int>(sql, parameters);
            return resultsFromCosmos;
        }

        private async Task<RMMyhubTreeFolderItemResult> ConvertToTreeFolderItem(List<RMMyhubTreeFolderItem> resultsFromCosmos, GeneralSettingModel genernalSetting, string timeZoneId, bool isDaylight, bool isRootFolder = false)
        {
            var folderItems = new List<RMMyhubTreeFolderItem>();
            if (resultsFromCosmos == null || resultsFromCosmos.Count == 0)
            {
                return new RMMyhubTreeFolderItemResult
                {
                    Items = folderItems,
                    HasMore = false
                };
            }

            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), genernalSetting.DataFormatId), true)];

            // Loop-invariant: parse timezone offset once instead of per item.
            int? timeZoneOffset = string.IsNullOrEmpty(timeZoneId) ? (int?)null : Convert.ToInt32(timeZoneId);

            var stopwatch = Stopwatch.StartNew();
            var parentIds = resultsFromCosmos.Where(x => x != null).Select(x => x.Id).ToList();
            var hasChildrenMap = await FolderMethod.GetBatchHasChildrenAsync(parentIds, resultsFromCosmos.First().PartitionKeyId);
            stopwatch.Stop();
            logger.Info($"[FolderTree Performance] GetBatchHasChildrenAsync completed in {stopwatch.ElapsedMilliseconds} ms");

            // Cache root connection name per distinct PartitionKeyId to avoid redundant
            // synchronous DAO round-trips when the same connection appears across multiple items.
            var rootFolderNameCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var performanceLogListForLoop = new List<long>();
            var stopwatch2 = new Stopwatch();
            var rootConnectionIds = resultsFromCosmos
                .Where(item => item != null && (isRootFolder || item.IsRootFolder))
                .Select(item => Guid.TryParse(item.PartitionKeyId, out var connectionId)
                    ? connectionId
                    : Guid.Empty)
                .Where(connectionId => connectionId != Guid.Empty)
                .Distinct()
                .ToList();
            var connectionNames = (rootConnectionIds.Count == 0
                    ? new List<FSConnection>()
                    : FSConnectionDao.GetConnectionByIds(rootConnectionIds) ?? new List<FSConnection>())
                .Where(connection => connection != null)
                .ToDictionary(connection => connection.Id, connection => connection.Name);

            foreach (var cosmosItem in resultsFromCosmos)
            {
                stopwatch.Restart();
                if (cosmosItem == null)
                {
                    continue;
                }

                cosmosItem.ExtentionForFile = null;
                var startDate = long.Parse(cosmosItem.StartDate ?? "0");
                var endDate = long.Parse(cosmosItem.EndDate ?? "0");
                var path = string.Concat(cosmosItem.Path, "\\", cosmosItem.Name);

                if (isRootFolder || cosmosItem.IsRootFolder)
                {
                    if (!rootFolderNameCache.TryGetValue(cosmosItem.PartitionKeyId, out var rootFolderName))
                    {
                        rootFolderName = FSConnectionDao.GetConnectionById(Guid.Parse(cosmosItem.PartitionKeyId)).Name;
                        rootFolderNameCache[cosmosItem.PartitionKeyId] = rootFolderName;
                    }
                    cosmosItem.Name = rootFolderName;
                }

                var hasChildren = hasChildrenMap.TryGetValue(cosmosItem.Id, out var has) && has;

                folderItems.Add(new RMMyhubTreeFolderItem
                {
                    Id = cosmosItem.Id,
                    NodeId = cosmosItem.NodeId,
                    ParentId = cosmosItem.ParentId,
                    PartitionKeyId = cosmosItem.PartitionKeyId,
                    Name = cosmosItem.Name,
                    Path = path,
                    ClassCode = cosmosItem.ClassCode,
                    CountryCode = cosmosItem.CountryCode,
                    FileVolume = cosmosItem.FileVolume,
                    FolderVolume = cosmosItem.FolderVolume,
                    Size = cosmosItem.Size,
                    PendingDisposal = cosmosItem.PendingDisposal,
                    RecordId = cosmosItem.RecordId,
                    EndDate = endDate > 0
                        ? (timeZoneOffset.HasValue
                            ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, endDate, true, timeZoneOffset.Value, isDaylight, dateFormat).SimplifyFormatTime
                            : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, endDate, true).SimplifyFormatTime)
                        : null,
                    StartDate = (startDate > 0 || cosmosItem.RetentionType == "1")
                        ? (timeZoneOffset.HasValue
                            ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, startDate, true, timeZoneOffset.Value, isDaylight, dateFormat).SimplifyFormatTime
                            : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, startDate, true).SimplifyFormatTime)
                        : null,
                    RetentionType = ConvertRetentionType(cosmosItem.RetentionType),
                    HasChildren = hasChildren,
                    IsRootFolder = cosmosItem.IsRootFolder
                });

                stopwatch.Stop();
                performanceLogListForLoop.Add(stopwatch.ElapsedMilliseconds);
            }

            if (performanceLogListForLoop.Any())
            {
                var avg = performanceLogListForLoop.Average();
                var max = performanceLogListForLoop.Max();
                var min = performanceLogListForLoop.Min();
                var p95 = performanceLogListForLoop.OrderBy(x => x).Skip((int)(performanceLogListForLoop.Count * 0.95)).FirstOrDefault();

                logger.Info($"[FolderTree Performance] ConvertToTreeFolderItem: " +
                            $"Count={performanceLogListForLoop.Count}, " +
                            $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
            }

            return new RMMyhubTreeFolderItemResult
            {
                Items = folderItems
            };
        }
        #endregion

        #region Folder and Item

        //public async Task<RMMyhubFolderAndFileItemResult> GetMyhubFolderAndItemsAsync(RMMyhubFolderItemQueryInfo queryInfo)
        //{
        //    try
        //    {
        //        var stopwatch = Stopwatch.StartNew();
        //        var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
        //        stopwatch.Stop();
        //        logger.Info($"[FolderAndItems Performance] GetGeneralSettingAsync completed in {stopwatch.ElapsedMilliseconds} ms");
        //        var query = await FolderAndItemMethod.BuildQuery(queryInfo);
        //        stopwatch.Restart();
        //        var (resultsFromCosmos, continuationToken) = await RecordStore.QueryAsync<RMMyhubFolderAndFileItem>(query.sql, query.parameter, queryInfo.ContinuationToken, queryInfo.PageSize);
        //        stopwatch.Stop();
        //        logger.Info($"[FolderAndItems Performance] QueryAsync completed in {stopwatch.ElapsedMilliseconds} ms");
        //        var connectionInfor = FSConnectionDao.GetConnectionById(new Guid(queryInfo.PartitionKeyId));
        //        var folderItems = await ConvertToFolderAndFile(resultsFromCosmos, connectionInfor, generalSetting, queryInfo.TimeZoneId, queryInfo.IsDaylight);
        //        folderItems.HasMore = !string.IsNullOrEmpty(continuationToken);
        //        folderItems.ContinuationToken = continuationToken;
        //        var countQuery = await FolderAndItemMethod.BuildCountQuery(queryInfo);
        //        folderItems.Count = await RecordStore.QuerySingleAsync<int>(countQuery.sql, countQuery.parameter);
        //        return folderItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"An error occured while GetMyhubFolderAndItemsAsync {ex}");
        //        return new RMMyhubFolderAndFileItemResult
        //        {
        //            Items = new List<RMMyhubFolderAndFileItem>(),
        //            Count = 0,
        //            HasMore = false
        //        };
        //    }
        //}

        public async Task<RMMyhubFolderAndFileItemResult> GetMyhubFolderAndItemsAsync(RMMyhubFolderItemQueryInfo queryInfo)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Independent operations - run concurrently instead of sequential awaits,
                // reducing total wall-clock time to max(GeneralSetting, ConnectionLookup, CosmosQuery).
                var generalSettingTask = GeneralSettingService.GetGeneralSettingAsync();
                var connectionInforTask = Task.Run(() => FSConnectionDao.GetConnectionById(new Guid(queryInfo.PartitionKeyId)));

                var query = await FolderAndItemMethod.BuildQuery(queryInfo);
                var cosmosQueryTask = RecordStore.QueryAsync<RMMyhubFolderAndFileItem>(
                    query.sql, query.parameter, queryInfo.ContinuationToken, queryInfo.PageSize);

                await Task.WhenAll(generalSettingTask, connectionInforTask, cosmosQueryTask);

                var generalSetting = generalSettingTask.Result;
                var connectionInfor = connectionInforTask.Result;
                var (resultsFromCosmos, continuationToken) = cosmosQueryTask.Result;

                stopwatch.Stop();
                logger.Info($"[FolderAndItems Performance] Parallel GetGeneralSettingAsync/GetConnectionById/QueryAsync completed in {stopwatch.ElapsedMilliseconds} ms");

                var folderItems = await ConvertToFolderAndFileAsync(resultsFromCosmos, connectionInfor, generalSetting, queryInfo.TimeZoneId, queryInfo.IsDaylight);
                folderItems.HasMore = !string.IsNullOrEmpty(continuationToken);
                folderItems.ContinuationToken = continuationToken;

                // Count query only needs to run once for the first page: the total row
                // count under a given filter set does not change across ContinuationToken pages.
                if (string.IsNullOrEmpty(queryInfo.ContinuationToken))
                {
                    var countStopwatch = Stopwatch.StartNew();
                    var countQuery = await FolderAndItemMethod.BuildCountQuery(queryInfo);
                    folderItems.Count = await RecordStore.QuerySingleAsync<int>(countQuery.sql, countQuery.parameter);
                    countStopwatch.Stop();
                    logger.Info($"[FolderAndItems Performance] Count query completed in {countStopwatch.ElapsedMilliseconds} ms");
                }

                return folderItems;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetMyhubFolderAndItemsAsync {ex}");
                return new RMMyhubFolderAndFileItemResult
                {
                    Items = new List<RMMyhubFolderAndFileItem>(),
                    Count = 0,
                    HasMore = false
                };
            }
        }

        private async Task<RMMyhubFolderAndFileItemResult> ConvertToFolderAndFile(List<RMMyhubFolderAndFileItem> resultsFromCosmos, FSConnection connectionInfo, GeneralSettingModel genernalSetting, string timeZoneId, bool isDaylight)
        {
            var folderAndFiles = new List<RMMyhubFolderAndFileItem>();
            if (resultsFromCosmos == null || resultsFromCosmos.Count == 0)
            {
                return new RMMyhubFolderAndFileItemResult()
                {
                    Items = folderAndFiles,
                    HasMore = false
                };
            }
            var connectionGroupId = connectionInfo.GroupId.ToString();
            var stopwatch = Stopwatch.StartNew();
            var disableStatusDict = BuildDisableStatusDictionary(connectionGroupId);
            var disableDownloadRCCStatusDict = BuildDisableDownloadRCCStatusDictionary(connectionGroupId);
            var folderInactiveStatusDict = BuildFolderInactiveStatusDictionary(connectionGroupId);
            stopwatch.Stop();
            logger.Info($"[FolderAndItems Performance] Build all dictionaries completed in {stopwatch.ElapsedMilliseconds} ms");
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), genernalSetting.DataFormatId), true)];
            var performanceLogListForLoop = new List<long>();
            var stopwatch2 = new Stopwatch();
            foreach (var cosmosItem in resultsFromCosmos)
            {
                stopwatch2.Restart();
                if (cosmosItem == null)
                {
                    continue;
                }
                var startDate = long.Parse(cosmosItem.StartDate ?? "0");
                var endDate = long.Parse(cosmosItem.EndDate ?? "0");
                folderAndFiles.Add(new RMMyhubFolderAndFileItem
                {
                    Id = cosmosItem.Id,
                    NodeId = cosmosItem.NodeId,
                    Name = cosmosItem.Name,
                    Path = cosmosItem.Path + "\\" + cosmosItem.Name,
                    ClassCode = cosmosItem.ClassCode,
                    CountryCode = cosmosItem.CountryCode,
                    FileVolume = cosmosItem.FileVolume,
                    Size = cosmosItem.Size,
                    PendingDisposal = cosmosItem.PendingDisposal,
                    RecordId = cosmosItem.RecordId,
                    EndDate = endDate > 0 ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, endDate, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, endDate, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime)
                        : null,
                    StartDate = (startDate > 0 && cosmosItem.RetentionType == "1") ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, startDate, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, startDate, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime)
                        : null,
                    RetentionType = ConvertRetentionType(cosmosItem.RetentionType),
                    IsFolder = cosmosItem.IsFolder,
                    ExtentionForFile = cosmosItem.ExtentionForFile,
                    PartitionKeyId = cosmosItem.PartitionKeyId,
                    EnableRecordManagement = cosmosItem.IsFolder
                    ? !CurrentNodeIsDisable(BuildFolderFullPath(cosmosItem.Path, cosmosItem.Name), disableStatusDict)
                    : !CurrentNodeIsDisable(cosmosItem.Path, disableStatusDict),
                    IsAllowDownloadRCC = cosmosItem.IsFolder
                    ? !IsNodeDisabledDownloadRCC(BuildFolderFullPath(cosmosItem.Path, cosmosItem.Name), disableDownloadRCCStatusDict)
                    : !IsNodeDisabledDownloadRCC(cosmosItem.Path, disableDownloadRCCStatusDict),
                    IsActive= cosmosItem.IsFolder
                    ? !IsFolderInactive(BuildFolderFullPath(cosmosItem.Path, cosmosItem.Name), folderInactiveStatusDict)
                    : !IsFolderInactive(cosmosItem.Path, folderInactiveStatusDict)
                });
                stopwatch2.Stop();
                performanceLogListForLoop.Add(stopwatch2.ElapsedMilliseconds);
            }
            if (performanceLogListForLoop.Any())
            {
                var avg = performanceLogListForLoop.Average();
                var max = performanceLogListForLoop.Max();
                var min = performanceLogListForLoop.Min();
                var p95 = performanceLogListForLoop.OrderBy(x => x).Skip((int)(performanceLogListForLoop.Count * 0.95)).FirstOrDefault();

                logger.Info($"[FolderAndItems Performance] ConvertToFolderAndFile: " +
                            $"Count={performanceLogListForLoop.Count}, " +
                            $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
            }
            return new RMMyhubFolderAndFileItemResult
            {
                Items = folderAndFiles
            };

        }

        public async Task<List<RMMyhubFolderStatisticsInfo>> GetFolderStatisticsAsync(RMMyhubFolderStatisticsQueryInfo queryInfo)
        {
            var nodes = queryInfo.Nodes?
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.FolderPath))
                .ToList() ?? new List<RMMyhubFolderNodeInfo>();

            if (nodes.Count == 0)
            {
                return new List<RMMyhubFolderStatisticsInfo>();
            }

            var stopwatch = Stopwatch.StartNew();
            var folderPaths = nodes
                .Select(node => node.FolderPath.TrimEnd('\\'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var (sql, parameters) = VolumeMethod.GetFolderStatisticsBatch( queryInfo.PartitionKeyId, folderPaths);

            try
            {
                var fileRecords = await RecordStore.QueryAllAsync<FolderStatisticsFileRecord>( sql, parameters, queryInfo.PartitionKeyId.ToLowerInvariant());
                var statisticsByPath = CalculateFolderStatistics(fileRecords, folderPaths);

                stopwatch.Stop();
                logger.Info($"[FolderStatistics Performance] Batch query completed in {stopwatch.ElapsedMilliseconds} ms, processed {nodes.Count} nodes");

                return nodes.Select(node =>
                {
                    var path = node.FolderPath.TrimEnd('\\');
                    var statistics = statisticsByPath[path];
                    return new RMMyhubFolderStatisticsInfo
                    {
                        NodeId = node.NodeId,
                        Size = statistics.Size,
                        Volume = statistics.Volume
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while getting folder statistics for {nodes.Count} nodes. Error: {ex}");
                return nodes.Select(node => new RMMyhubFolderStatisticsInfo
                {
                    NodeId = node.NodeId,
                    Size = 0,
                    Volume = 0
                }).ToList();
            }
        }

        private static Dictionary<string, (long Size, long Volume)> CalculateFolderStatistics(
            List<FolderStatisticsFileRecord> fileRecords,
            List<string> folderPaths)
        {
            var statistics = folderPaths.ToDictionary(
                path => path,
                _ => (Size: 0L, Volume: 0L),
                StringComparer.OrdinalIgnoreCase);

            foreach (var file in fileRecords.Where(file => file != null && !string.IsNullOrWhiteSpace(file.DirPath)))
            {
                foreach (var folderPath in folderPaths)
                {
                    var normalizedPath = folderPath.TrimEnd('\\');
                    if (file.DirPath.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase)
                        || file.DirPath.StartsWith(normalizedPath + "\\", StringComparison.OrdinalIgnoreCase))
                    {
                        var current = statistics[folderPath];
                        statistics[folderPath] = (current.Size + file.FileSize, current.Volume + 1);
                    }
                }
            }

            return statistics;
        }

        private sealed class FolderStatisticsFileRecord
        {
            public string DirPath { get; set; }
            public long FileSize { get; set; }
        }
        private async Task<(long FileVolume, long FolderVolume)> GetFolderAndFileVolumeAsync(string partitionKeyId, string path)
        {
            var query = VolumeMethod.GetFolderAndFileVolume(partitionKeyId, path);
            var result = await RecordStore.QuerySingleAsync<dynamic>(query.sql, query.parameter);

            long fileVolume = result?.FileVolume ?? 0;
            long folderVolume = result?.FolderVolume ?? 0;

            return (fileVolume, folderVolume);
        }
        private static string BuildFolderFullPath(string path, string name)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return name ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return path;
            }

            return path.TrimEnd('\\') + "\\" + name;
        }

        public async Task<FSDashboardInformation> GetMyHubDashboardDataAsync(RMMyHubFolderDashboard queryInfo)
        {
            try
            {
                var nodeData = await FSMyHubDashboardDao.GetByFullPathAsync(queryInfo.FullPath);
                if (nodeData == null || string.IsNullOrEmpty(nodeData.MetaData))
                {
                    return new FSDashboardInformation();
                }
                var result = new FSDashboardInformation();
                var metaData = JsonConvert.DeserializeObject<FSDashboard>(nodeData.MetaData);
                var ranges = new List<DateRange>
                {
                    //DateRange.Last_7_Days,
                    //DateRange.Last_30_Days,
                    //DateRange.Three_Month,
                    DateRange.Six_Month
                };

                if (metaData.LineChartData != null && metaData.LineChartData.Any())
                {
                    result.LineChartDatas = new List<LineChartData>();
                    var gls = await GeneralService.GetGeneralSettingAsync();
                    TimeZoneInfo timeZone;

                    if (!string.IsNullOrEmpty(queryInfo.TimeZoneId))
                    {
                        var realTimeZoneId = DateTimeUtil.AllTimeZones[Convert.ToInt32(queryInfo.TimeZoneId)];
                        timeZone = GeneralSettingConfig.FindSystemTimeZoneById(realTimeZoneId);
                    }
                    else
                    {
                        timeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
                    }
                    foreach (var range in ranges)
                    {
                        var rangeDate = await GetRangeDateAsync(range, timeZone);
                        var groupedData = metaData.LineChartData
                                    .Select(x =>
                                    {
                                        var utc = DateTime.ParseExact(
                                            x.Date.ToString(),
                                            "yyyyMMddHH",
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

                                        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);

                                        return new
                                        {
                                            LocalDate = localTime.Date,
                                            x.Created,
                                            x.Modified,
                                            x.Accessed
                                        };
                                    })
                                    .Where(x => x.LocalDate >= rangeDate.Start && x.LocalDate <= rangeDate.End)
                                    .GroupBy(x => x.LocalDate)
                                    .ToDictionary(
                                        g => g.Key,
                                        g => new LineChartData
                                        {
                                            Date = g.Key.ToString("yyyy-MM-dd"),
                                            Created = g.Sum(i => i.Created),
                                            Modified = g.Sum(i => i.Modified),
                                            Accessed = g.Sum(i => i.Accessed)
                                        });

                        var itemsInRange = Enumerable
                            .Range(0, (rangeDate.End - rangeDate.Start).Days + 1)
                            .Select(offset =>
                            {
                                var date = rangeDate.Start.AddDays(offset);

                                return groupedData.TryGetValue(date, out var data)
                                    ? data
                                    : new LineChartData
                                    {
                                        Date = date.ToString("yyyy-MM-dd"),
                                        Created = 0,
                                        Modified = 0,
                                        Accessed = 0
                                    };
                            })
                            .ToList();
                        result.LineChartDatas.AddRange(itemsInRange);
                    }
                }
                if (nodeData.ScopeId != nodeData.NodeId)
                {
                    var connectionData = await FSMyHubDashboardDao.GetByNodeIdAsync(nodeData.ScopeId);
                    if (connectionData != null)
                    {
                        var connectionMetaData = JsonConvert.DeserializeObject<FSDashboard>(connectionData.MetaData);
                        var totalStorage = connectionMetaData.Storage != null ? connectionMetaData.Storage.TotalSize : 0;
                        var totalClasse = connectionMetaData.ClassCodes != null ? connectionMetaData.ClassCodes.Sum(s => s.Usage) : 0;
                        metaData.Storage.TotalSize = totalStorage;
                        metaData.ClassCodesTotal = totalClasse;
                    }
                }
                result.Storage = metaData.Storage;
                result.ClassCodes = metaData.ClassCodes;
                result.ClassCodesTotal = metaData.ClassCodesTotal;
                result.Creators = metaData.Creators;
                result.FileTypes = metaData.FileTypes;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while getting dashboard data for path: {queryInfo.FullPath}. Error: {ex}");
                return new FSDashboardInformation();
            }
        }


        private async Task<(DateTime Start, DateTime End)> GetRangeDateAsync(DateRange dateRange, TimeZoneInfo timeZone)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var end = now.Date;
            DateTime start;

            switch (dateRange)
            {
                case DateRange.Last_7_Days:
                    start = now.AddDays(-7).Date;
                    break;

                case DateRange.Last_30_Days:
                    start = now.AddDays(-30).Date;
                    break;

                case DateRange.Three_Month:
                    start = now.AddMonths(-3).Date;
                    break;

                case DateRange.Six_Month:
                    start = now.AddMonths(-6).Date;
                    break;
                default:
                    start = now.AddDays(-7).Date;
                    break;
            }

            return (start, end);
        }

        #endregion

        #region Detail Table
        public async Task<RMMyhubFolderDetailTableItem> GetMyhubFolderDetailAsync(RMMyhubFolderDetailTableQueryInfo queryInfo)
        {
            try
            {
                var query = DetailMethod.GetFolderDetail(queryInfo.Id);
                var result = await RecordStore.QuerySingleAsync<RMMyhubFolderDetailTableItem>(query.sql, query.parameter);
                if (result == null)
                {
                    return null;
                }
                var volumeResult = await GetFolderAndFileVolumeAsync(result.PartitionKeyId, result.Path);
                result.Size = (await GetFolderStatisticsAsync(
                     new RMMyhubFolderStatisticsQueryInfo
                     {
                         PartitionKeyId = result.PartitionKeyId,
                         Nodes = new List<RMMyhubFolderNodeInfo>
                         {
                            new RMMyhubFolderNodeInfo
                            {
                                NodeId = result.Id.ToString(),
                                FolderPath = result.Path
                            }
                         }
                     }))
                     .FirstOrDefault()?.Size ?? 0;
                result.FileVolume = volumeResult.FileVolume;
                result.FolderVolume = volumeResult.FolderVolume;
                result.PendingDisposal = await GetDisposalVolumeByFullPathAsync(result.PartitionKeyId, result.Path);
                if (result.IsRootFolder)
                    result.Name = FSConnectionDao.GetConnectionById(result.PartitionKeyId.ToGuid())?.Name ?? result.Name;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while GetMyhubFolderDetailAsync {ex}");
                return null;
            }

        }


        #endregion

        #region Classify
        public List<string> ReadCountryCodeByClassCode(string ClassCode)
        {
            return ReadClassCodeMethod.ReadCountryCodeNameByClassCode(ClassCode);
        }
        public List<string> ReadRetentionType()
        {
            var result = new List<string>();
            result.Add(RetentionScheduleType.Event.ToString());
            result.Add(RetentionScheduleType.Flat.ToString());
            return result;
        }
        public async Task<List<MyhubClassifyReturnMessage>> UpdateMyhubClassifyAsync(RMMyhubClassifyQueryInfo queryInfo)
        {
            var selectedTargets = new List<RMMyhubClassifyItem>();
            var returnMessageList = new List<MyhubClassifyReturnMessage>();
            if (queryInfo.Id.Length == 0)
            {
                var gIdList = new List<Guid>();
                foreach (var node in queryInfo.PartitionKeyId)
                {
                    var nodeInfo = await DriveMethod.BaseGetNodeInfoByPartitionKeyAsync(node);
                    bool hasSyncedRecordInCosmos = Guid.TryParse(nodeInfo?.NodeId, out var gId);
                    if (hasSyncedRecordInCosmos && nodeInfo != null)
                        gIdList.Add(gId);
                    else
                    {
                        var noSyncedConnection = FSConnectionDao.GetConnectionById(node.ToGuid());
                        var noSyncedNode = new RMMyhubClassifyItem
                        {
                            PartitionKeyId = node,
                            NodeType = (int)NodeLevel.FSFolder,
                            FullPath = noSyncedConnection.UNCPath
                        };
                        selectedTargets.Add(noSyncedNode);
                    }
                }
                selectedTargets.AddRange(await GetSelectedTargetsAsync(gIdList));
            }
            else
            {
                selectedTargets = await GetSelectedTargetsAsync(queryInfo?.Id);
            }

            var disabledTarget = FindFirstDisabledTarget(selectedTargets);
            if (disabledTarget != null)
            {
                logger.Warn($"Record management is disabled for target: {disabledTarget.FullPath}");
                returnMessageList.Add(new MyhubClassifyReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    Message = $"Have node is disable record management."
                });
                return returnMessageList;
            }

            var folderTargets = selectedTargets
                .Where(ClassifyMethod.IsFolderTarget)
                .ToList();

            var fileIds = selectedTargets
                .Where(item => !ClassifyMethod.IsFolderTarget(item))
                .Select(item => item.Id)
                .Where(item => item != Guid.Empty)
                .Distinct()
                .ToArray();


            if (folderTargets.Count > 0)
            {
                var returnMessages = await ClassifyMethod.RunFolderClassifyJobAsync(folderTargets, queryInfo);
                returnMessageList.AddRange(returnMessages);
            }

            if (fileIds.Length > 0)
            {
                var msgList = await UpdateMyhubFileClassifyAsync(queryInfo, fileIds);
                returnMessageList.AddRange(msgList);
            }

            return returnMessageList;
        }

        public Dictionary<string, RMMyhubClassifyQueryInfo> MultiGeoSeperateRequestRMMyhubClassifyQueryInfo(RMMyhubClassifyQueryInfo queryInfo, Dictionary<string, IEnumerable<string>> connectionIdsByDataCenter)
        {
            var result = new Dictionary<string, RMMyhubClassifyQueryInfo>();
            foreach (var kvp in connectionIdsByDataCenter)
            {
                result.Add(kvp.Key, new RMMyhubClassifyQueryInfo
                {
                    Id = queryInfo.Id,
                    ClassCode = queryInfo.ClassCode,
                    CountryCode = queryInfo.CountryCode,
                    RetentionType = queryInfo.RetentionType,
                    StartDate = queryInfo.StartDate,
                    IsApplySubItem = queryInfo.IsApplySubItem,
                    //PartitionKeyId = kvp.Value.ToArray()
                });
            }
            return result;
        }
        public async Task<RMMyhubClassifyDto> UpdateMyhubClassifyReturnValueAsync(RMMyhubClassifyReturnInfo queryInfo)
        {
            if (queryInfo.Id.Length > 1)
            {
                return null;
            }
            var query = ClassifyMethod.BuildQueryForReturnValue(queryInfo);
            var queryResult = await RecordStore.QuerySingleAsync<RMMyhubClassifyDto>(query.sql, query.parameter);
            return await ConvertToClassifyQueryInfo(queryResult, queryInfo.TimeZoneId, queryInfo.IsDaylight);
        }
        private async Task<RMMyhubClassifyDto> ConvertToClassifyQueryInfo(RMMyhubClassifyDto resultFromCosmos, string timeZoneId, bool isDaylight)
        {
            if (resultFromCosmos == null)
            {
                return new RMMyhubClassifyDto();
            }
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), generalSetting.DataFormatId), true)];
            var cosmosItem = resultFromCosmos;
            if (!long.TryParse(cosmosItem.StartDate, out long startDateticks))
            {
                startDateticks = 0;
            }
            var classCodeInfo = TermDao.GetRMTermByGuId(cosmosItem.TermUniqueId);
            return new RMMyhubClassifyDto
            {
                ClassCode = classCodeInfo.Name,
                CountryCode = cosmosItem.CountryCode,
                RetentionType = ConvertRetentionType(cosmosItem.RetentionType),
                StartDate = (cosmosItem.RetentionType == "1") ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(generalSetting, startDateticks, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(generalSetting, startDateticks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime)
                        : null,
                TermUniqueId = cosmosItem.TermUniqueId
            };
        }
        private string ConvertRetentionType(string type)
        {
            string result;
            result = type switch
            {
                "1" => I18NEntity.GetString("RM_FS_ClassCodePolicy_RetentionEventType"),
                "2" => I18NEntity.GetString("RM_FS_ClassCodePolicy_RetentionFlatType"),
                _ => null
            };
            return result;
        }

        private async Task<List<MyhubClassifyReturnMessage>> UpdateMyhubFileClassifyAsync(RMMyhubClassifyQueryInfo queryInfo, Guid[] fileIds)
        {
            var msgList = new List<MyhubClassifyReturnMessage>();
            var connectionId = Guid.Parse(queryInfo.PartitionKeyId.FirstOrDefault());
            var groupId = FSConnectionDao.GetConnectionById(connectionId).GroupId;
            if (!await FileSystemSettingsService.LoadFSNodeEnableRecordManagement(connectionId) || !await FileSystemSettingsService.LoadFSNodeEnableRecordManagement(groupId))
            {
                msgList.Add(new MyhubClassifyReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    Type = NodeLevel.FSFile,
                    Message = I18NEntity.GetString("RM_FS_Myhub_Classify_RecordManagementDisabled"),
                });
                return msgList;
            }
            var fileQueryInfo = new RMMyhubClassifyQueryInfo
            {
                Id = fileIds,
                ClassCode = queryInfo.ClassCode,
                CountryCode = queryInfo.CountryCode,
                RetentionType = queryInfo.RetentionType,
                StartDate = queryInfo.StartDate,
                IsApplySubItem = false,
                PartitionKeyId = queryInfo.PartitionKeyId,
                TermUniqueId = queryInfo.TermUniqueId,
                TimeZoneId = queryInfo.TimeZoneId,
                IsDaylight = queryInfo.IsDaylight,
                //SelectionNodes = queryInfo.SelectionNodes
            };

            var query = ActionMethod.BuildQueryAsync(fileQueryInfo);

            var queryResults = new List<RMMyhubActionTarget>();
            var result = await RecordStore.QueryAllAsync<RMMyhubActionTarget>(query.sql, query.parameter);
            queryResults.AddRange(result);


            return await ClassifyMethod.UpdateClassifyAsync(queryResults, fileQueryInfo);
        }

        private async Task<List<RMMyhubClassifyItem>> GetSelectedTargetsAsync(IEnumerable<Guid> ids)
        {
            var result = new List<RMMyhubClassifyItem>();
            foreach (var id in ids?.Where(item => item != Guid.Empty).Distinct() ?? Enumerable.Empty<Guid>())
            {
                var target = await GetSelectedTargetAsync(id);
                if (target != null)
                {
                    result.Add(target);
                }
            }

            return result;
        }

        private async Task<RMMyhubClassifyItem> GetSelectedTargetAsync(Guid id)
        {
            var query = ClassifyMethod.BuildFolderTargetQuery(id);
            var target = await RecordStore.QuerySingleAsync<RMMyhubClassifyItem>(query.sql, query.parameter);
            if (target == null)
            {
                return null;
            }

            target.FullPath = BuildFolderFullPath(target.DirPath, target.LeafName);
            return string.IsNullOrWhiteSpace(target.FullPath) ? null : target;
        }

        private async Task<List<RMMyhubClassifyItem>> GetFolderTargetsAsync(IEnumerable<Guid> ids)
        {
            var targets = await GetSelectedTargetsAsync(ids);
            return targets.Where(ClassifyMethod.IsFolderTarget).ToList();
        }


        #endregion
        public async Task<FSAuditQueryResult> QueryAuditTrailAsync(RMMyhubAuditTrialQueryInfo queryInfo)
        {
            try
            {
                var genernalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), genernalSetting.DataFormatId), true)];
                return await AuditTrialMethod.QueryAuditTrailAsync(queryInfo, dateFormat);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured while QueryAuditTrailAsync {ex}");
                return new FSAuditQueryResult();
            }

        }
        public RMMyhubAuditTrialFilterItem QueryAuditTrialFilter()
        {
            try
            {
                return AuditTrialMethod.QueryAuditTrialFilter();
            }
            catch (Exception ex)
            {
                logger.Error($"QueryAuditTrialFilter Failed. ERROR:{ex.Message}");
                return new();
            }
        }
        #region Job
        public bool RunFSMyHubDashboardJob(JobRunBy runBy, FileSystemMyhubSelectedNodeDto selectedNode)
        {
            var id = string.Empty;
            var runJobUserName = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                var queue = new JobQueueDto
                {
                    JobType = JobType.FSMyHubDashboard,
                    JobRunType = runBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = runJobUserName,
                    Parameters = selectedNode == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedNode)
                };

                id = JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while running file system dashboard job. Error: {e}");
            }
            return !string.IsNullOrEmpty(id);
        }
        #endregion

        #region 
        public async Task<RMConnectionPermissions> GetConnectionPermissionAsync(Guid connectionId)
        {
            try
            {
                var connection = await FSRegisterService.GetConnectionByIdAsync(connectionId);
                return new()
                {
                    InformationOwners = connection?.InformationOwners,
                    RecordOwners = connection?.RecordOwners
                };
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get connectin permission.Error: {e}");
                return new();
            }
        }

        public RMConnectionAddUserPageInfo SearchAvaliableOwners(string tenantId, string key)
        {
            try
            {
                logger.Info("Start to search users.");
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
                {
                    return null;
                }
                int total = 20;
                var accounts = new List<AOSUserDto>();

                var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
                var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
                var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);
                var accountsFromAD = AccountWrapperService.SearchAccounts4FSConnection(TenantLocalValue.LogonGroupId, key, 20);

                var includeAccounts = accountsFromAD.Where(a => !(existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id) || existUserIds.Contains(a.Id)))
                    .ToList();
                if (includeAccounts.Count > 0)
                {
                    var offset = includeAccounts.Count > (total - accounts.Count) ? total - accounts.Count : includeAccounts.Count;
                    var actualAccounts = includeAccounts.GetRange(0, offset);
                    var usersInfo = actualAccounts.Select(o => AADAccount.Convert2AOSUserDto(o)).ToList();
                    accounts.AddRange(usersInfo);
                }

                logger.Info($"The final accounts of the search:{accounts.Count}.");

                var info = new RMConnectionAddUserPageInfo
                {
                    Users = accounts,
                    StatusMsg = accounts.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), accounts.Count)
                    : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
                };
                logger.Info("End to search users.");
                return info;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while search avaliable owners. Error: {e}");
                return new();
            }
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.PermissionChange, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.PermissionChange, AuditLevel = FSAuditLevel.Connection, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<bool> UpdateConnectionRecordOwners(RMConnectionRecordOwnerUpdateModel updateModels)
        {
            return await UpdateConnectionRecordOwners(updateModels, true);
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.PermissionChange, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.PermissionChange, AuditLevel = FSAuditLevel.Connection, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<bool> UpdateConnectionRecordOwnersForOtherDC(RMConnectionRecordOwnerUpdateModel updateModels)
        {
            return await UpdateConnectionRecordOwners(updateModels, false);
        }
        private async Task<bool> UpdateConnectionRecordOwners(RMConnectionRecordOwnerUpdateModel updateModels, bool isNeedSyncUser)
        {
            try
            {
                if (updateModels == null || updateModels.ConnectionId == Guid.Empty)
                {
                    logger.Error("UpdateConnectionRecordOwners failed due to invalid input.");
                    return false;
                }

                if (updateModels.RecordOwners == null || updateModels.RecordOwners.Count == 0)
                {
                    //logger.Error("UpdateConnectionRecordOwners failed due to empty record owners.");
                    //return false;
                    await FSConnectionOwnerDao.RemoveAllRecordOwnersByConnectionIdAsync(updateModels.ConnectionId);
                    logger.Info($"Succeed delete record onwers for connection [{updateModels.ConnectionId}] , updateModels.RecordOwners is empty");
                    return true;
                }

                if (isNeedSyncUser)
                {
                    await SyncOwnerUsersAsync(updateModels.RecordOwners);
                }

                updateModels.RecordOwners.ForEach(item =>
                {
                    if (string.IsNullOrWhiteSpace(item.UserId))
                    {
                        var existsAccount = AccountDao.Find(existsItem => existsItem.UserPrincipalName == item.UserPrincipalName && existsItem.IsRemoved == 0);
                        if (existsAccount == null)
                        {
                            throw new Exception($"Can't find user in opus [{item.Id}]");
                        }
                        item.UserId = existsAccount.UserId;
                    }
                });
                var recordOwnerUserIds = updateModels.RecordOwners.Select(item => item.UserId).ToHashSet();
                var existsRecordOwnerIntIds = (await AccountDao.FindListAsync(item => recordOwnerUserIds.Contains(item.UserId) && item.IsRemoved == 0)).Select(item => item.Id).ToList();

                await FSConnectionOwnerDao.RemoveAllRecordOwnersByConnectionIdAsync(updateModels.ConnectionId);

                FSConnectionOwnerDao.AddOwners(updateModels.ConnectionId, [], existsRecordOwnerIntIds);

                logger.Info($"Succeed update record onwers for connection [{updateModels.ConnectionId}]");
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update connection record owners. Error: {e}");
                return false;
            }
        }

        private async Task SyncOwnerUsersAsync(List<ToUserInfo> owners)
        {
            try
            {
                if (owners == null || owners.Count == 0) return;
                var uniqueOwners = owners.GroupBy(u => !string.IsNullOrEmpty(u.Id) ? u.Id.ToLower() : u.UserPrincipalName?.ToLower())
                                        .Where(g => g.Key != null)
                                        .Select(g => g.First())
                                        .ToList();
                await UserServices.SyncUsersAsync(TenantLocalValue.LogonGroupId, uniqueOwners);
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while syncing owner users from AOS. Error: {ex}");
                throw;
            }
        }
        #endregion

        public async Task<RMMyhubParameterBeforePendingDisposalQuery> GetParameterBeforeUnderReviewQueryAsync(RMMyhubPendingDisposalQueryInfo queryInfo)
        {
            var queryFolder = DetailMethod.GetFolderDetail(queryInfo.NodeId);
            var connectionInfo = FSConnectionDao.GetConnectionById(Guid.Parse(queryInfo.PartitionKeyId));
            var result = await RecordStore.QuerySingleAsync<RMMyhubFolderDetailTableItem>(queryFolder.sql, queryFolder.parameter);
            return new RMMyhubParameterBeforePendingDisposalQuery
            {
                DriveName = connectionInfo.Name,
                FolderNodeId = queryInfo.NodeId,
                FolderPath = result.Path,
                IsPause = connectionInfo.IsPause
            };
        }
        public async Task<List<FSConnectionPermission>> GetConnectionPermission()
        {
            var userIds = new List<int>();

            try
            {
                userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get removed/group ids for user [{TenantLocalValue.LogonUserId}]. Fallback to active user only. Error: {ex}");
            }

            var relationships = await RMFSConnectionAndOwnerRelationshipDao.GetOwnersByUserIntIdsAsync(userIds.Distinct().ToList());

            if (!relationships.Any())
            {
                return new List<FSConnectionPermission>();
            }

            return relationships
                .GroupBy(r => r.Type)
                .Select(g => new FSConnectionPermission
                {
                    OwnerType = (Contract.Myhub.FSConnectionOwnerType)g.Key,
                    ConnectionIds = g
                        .Select(r => r.ConnectionId)
                        .Distinct()
                        .ToList()
                })
                .ToList();
        }


        public Dictionary<string, RMMyhubDriveSettings> GetMyhubDriveSettings(List<RMMyhubDriveQuerySettings> queryInfos)
        {
            if (queryInfos == null || queryInfos.Count == 0)
            {
                return new Dictionary<string, RMMyhubDriveSettings>();
            }
            try
            {
                var result = new Dictionary<string, RMMyhubDriveSettings>();
                var valueList = queryInfos.Select(c => c.ConnectionGroupId.ToString()).ToList();
                var enableRecordDict = BuildDisableStatusDictionary(valueList);
                var enableRCCDict = BuildDisableDownloadRCCStatusDictionary(valueList);
                foreach (var queryInfo in queryInfos)
                {
                    result.Add(queryInfo.UNCPath, new RMMyhubDriveSettings()
                    {

                        EnableRecordManagement = !CurrentConnectionIsDisable(queryInfo.ConnectionId, queryInfo.ConnectionGroupId, enableRecordDict),
                        IsAllowDownloadRCC = CurrentConnectionIsDisableDownloadRCC(queryInfo.ConnectionId, queryInfo.ConnectionGroupId, enableRCCDict)
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while getting Myhub drive settings. Error: {ex}");
                return new Dictionary<string, RMMyhubDriveSettings>();
            }
        }

        public async Task<RAReturnMessage> UpdateConnectoinIsPauseAsync(PauseOrResumeReq req)
        {
            var returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };

            try
            {
                if (req == null || req.NodeIds == null || req.NodeIds.Count < 1 || (req.IsPause != 0 && req.IsPause != 1))
                {
                    logger.Error($"Error occurred while Update Connectoin IsPause Async. Error: Param is error. Param: {req}");
                    throw new Exception("Param is error");
                }
                ManualApprovalActionResult res = new ManualApprovalActionResult();
                if (req.IsPause == 1)
                {
                    res = await RMMyhubAsyncAuditServices.PauseAsync(req);
                }
                else
                {
                    res = await RMMyhubAsyncAuditServices.ResumeAsync(req);
                }

                if (res.CompletedStatus == ActionCompletedStatus.Failed)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = res.Message;
                    return returnMessage;
                }
                else if (res.CompletedStatus == ActionCompletedStatus.HasException)
                {
                    returnMessage.MessageType = RAMessageType.Exception;
                    returnMessage.ErrorMessage = res.Message;
                    return returnMessage;
                }
                else
                {
                    return returnMessage;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while Update Connectoin IsPause Async. Error: {e}. param: {req}");
                returnMessage.MessageType = RAMessageType.Exception;
                returnMessage.ErrorMessage = e.Message;
                return returnMessage;
            }
        }

        #region Myhub report action

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.DeleteRCCReport, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteReportContentAsync(List<Guid> jobIds, int reportType)
        {
            logger.Info($"Start to delete report content for job IDs: {string.Join(", ", jobIds)} and report type: {reportType}");
            var returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };

            try
            {
                // Get node
                List<RMMyhubReportAuditItem> auditItems = GetMyhubReports(jobIds, reportType);

                foreach (var auditItem in auditItems)
                {
                    if (auditItem.ConnectionId != Guid.Empty)
                    {
                        var validOwnerMsg = await CheckOwnerPermissionAsync(auditItem.ConnectionId, reportType);
                        if (validOwnerMsg.MessageType != RAMessageType.Successful)
                        {
                            return validOwnerMsg;
                        }
                    }
                }
                // Call Audit and store
                if (auditItems != null && auditItems.Count > 0)
                {
                    var auditType = reportType switch
                    {
                        (int)MyhubReportJobType.HistoryContent => FSAuditType.DeleteDisposalHistory,
                        (int)MyhubReportJobType.DownloadRCCReport => FSAuditType.DeleteRCCReport,
                    };
                    await FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)auditType, reportType);
                }
                else
                {
                    logger.Error($"Error occurred while Delete Report Content Async. Error: No report content found for the provided job IDs. Job IDs: {string.Join(", ", jobIds)}");
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = "No report content found for the provided job IDs.";
                    return returnMessage;
                }

                // Do action to delete report content
                try
                {
                    List<int> finalJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.None,
                    (int)DownloadContentJobStatus.Calculating,
                    (int)DownloadContentJobStatus.Failed,
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.FinishWithException,
                    (int)DownloadContentJobStatus.Skipped,
                    (int)DownloadContentJobStatus.Stopped,
                    (int)DownloadContentJobStatus.Stopping
                };
                    var contentInfos = DownloadDataInfoDao.GetDownloadDataInfos(jobIds, finalJobStatus);
                    if (contentInfos != null && contentInfos.Count > 0)
                    {
                        returnMessage.Extension = JsonConvert.SerializeObject(contentInfos.Select(c => c.Name).ToList());
                    }
                    List<RMDownloadDataInfo> deletedInfos = new List<RMDownloadDataInfo>();
                    ArgumentCheck.NotNull(contentInfos, nameof(contentInfos));
                    foreach (var info in contentInfos)
                    {
                        try
                        {
                            //if (info.JobStatus == (int)DownloadContentJobStatus.Finished)
                            {
                                ArchivedContentDownloadService.DeleteExpiredData(info.JobId);
                            }
                            deletedInfos.Add(info);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Error occurred while deleting archived content. Id:{info?.RecordsId} Error:{e.ToString()}");
                            returnMessage.MessageType = RAMessageType.Failed;
                        }
                    }
                    if (deletedInfos.Count > 0)
                    {
                        try
                        {
                            DownloadDataInfoDao.BatchDelete(deletedInfos);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Error occurred while batch deleting archived content. Error:{e.ToString()}");
                            returnMessage.MessageType = RAMessageType.Failed;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while deleting archived contents. Error:{e.ToString()}");
                    returnMessage.MessageType = RAMessageType.Failed;
                }
                return returnMessage;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while Delete Report Content Async. Error: {e}");
                returnMessage.MessageType = RAMessageType.Exception;
                returnMessage.ErrorMessage = ($"Error occurred while Delete Report Content Async.");
                return returnMessage;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.DownloadRCCReport, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        public async Task<RMMyhubReportDownloadResponse> DownloadReportContentMyhub(RMMyhubReportQueryInfo queryInfo)
        {
            try
            {
                // Get node
                List<RMMyhubReportAuditItem> auditItems = GetMyhubReports(queryInfo.Ids, queryInfo.ReportType);

                foreach (var auditItem in auditItems)
                {
                    if (auditItem.ConnectionId != Guid.Empty)
                    {
                        var validOwnerMsg = await CheckOwnerPermissionAsync(auditItem.ConnectionId, queryInfo.ReportType);
                        if (validOwnerMsg.MessageType != RAMessageType.Successful)
                        {
                            return new RMMyhubReportDownloadResponse()
                            {
                                Success = false,
                                Message = validOwnerMsg.ErrorMessage
                            };
                        }
                    }
                }
                // Call Audit and store
                if (auditItems != null && auditItems.Count > 0)
                {
                    var auditType = queryInfo.ReportType switch
                    {
                        (int)MyhubReportJobType.HistoryContent => FSAuditType.DownloadDisposalHistory,
                        (int)MyhubReportJobType.DownloadRCCReport => FSAuditType.JpmcDownloadRCCReport,
                    };
                    await FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)auditType, queryInfo.ReportType);
                }
                else
                {
                    logger.Error($"Error occurred while Download Report Content Async. Error: No report content found for the provided job IDs. Job IDs: {string.Join(", ", queryInfo.Ids)}");
                    return new RMMyhubReportDownloadResponse
                    {
                        Success = false,
                        Message = "No report content found for the provided job IDs."
                    };
                }

                // Do action to download report content
                try
                {
                    logger.Debug("DownloadRCCArchivedContent controller");
                    if (queryInfo.Ids == null || !queryInfo.Ids.Any())
                    {
                        logger.Warn("Download request is missing or FileIds list is empty.");
                        return new RMMyhubReportDownloadResponse
                        {
                            Success = false,
                            Message = "No file IDs provided"
                        };
                    }
                    string fileName = string.Empty;
                    if (queryInfo.Ids.Count == 1)
                    {
                        logger.Warn("Record id is 1.");
                        string userId = TenantLocalValue.LogonUserId;
                        Guid recordId = queryInfo.Ids[0];
                        var contentInfo = DownloadDataInfoDao.GetDownloadDataInfos(queryInfo.Ids, new List<int>() { (int)DB.Model.DownloadContentJobStatus.Finished }).FirstOrDefault();
                        if (contentInfo == null || string.IsNullOrWhiteSpace(contentInfo.JobId))
                        {
                            logger.Error($"Cannot find download info with id:{queryInfo.Ids[0]}");
                            return new RMMyhubReportDownloadResponse
                            {
                                Success = false,
                                Message = "File not found"
                            };
                        }
                        fileName = HttpUtility.UrlDecode(contentInfo.Name);
                    }
                    else
                    {
                        logger.Warn("Record id more than 1.");
                        string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(DateTime.UtcNow.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                        fileName = I18NEntity.GetString("RM_DC_DownloadMultipleArchivedContent") + "_" + nowTimeStr + ".zip";
                    }
                    FileTransferStream stream = ArchivedContentDownloadService.DownloadArchivedContent(queryInfo.Ids, true);
                    if (stream == null)
                    {
                        logger.Warn("Created File is null.");
                        return new RMMyhubReportDownloadResponse
                        {
                            Success = false,
                            Message = "Stream is null"
                        };
                    }

                    string base64FileData = await ConvertStreamToBase64Async(stream);
                    return new RMMyhubReportDownloadResponse
                    {
                        Success = true,
                        FileData = base64FileData,
                        FileName = fileName,
                        ContentType = GetContentType(fileName),
                        FileSize = stream.Length
                    };
                }
                catch (Exception ex)
                {
                    logger.Error($"Download error: {ex.Message}");
                    logger.Error($"Stack trace: {ex.StackTrace}");
                    return new RMMyhubReportDownloadResponse
                    {
                        Success = false,
                        Message = "An error occurs when download report content.Please contact the administrator for help."
                    };
                }

            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while Delete Report Content Async. Error: {e}");
                return new RMMyhubReportDownloadResponse
                {
                    Success = false,
                    Message = "Error occurred while Delete Report Content Async."
                };
            }
        }

        #endregion


        #region Helpers

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

        private bool CurrentNodeIsDisable(string connectionGroupId, string folderPath)
        {
            var allDisablePath = FileSystemSettingDao.GetAllDisableRecordManagementPath(new Guid(connectionGroupId));
            if (allDisablePath != null && allDisablePath.Count > 0)
            {
                foreach (var path in allDisablePath)
                {
                    if (folderPath.StartsWith(path))
                    {
                        return true;
                    }
                }
            }
            var fsGroupSetting = FileSystemSettingDao.LoadFSSetting(new Guid(connectionGroupId), new Guid(connectionGroupId));
            var isGroupEnable = fsGroupSetting == null ? (int)Contract.Global.Object.EnableRecordManagementSetting.Enable : fsGroupSetting.EnableRecordManagement;
            return isGroupEnable != (int)Contract.Global.Object.EnableRecordManagementSetting.Enable;
        }
        private Dictionary<Guid, bool> BuildDisableStatusDictionary(List<string> connectionGroupIds)
        {
            var result = new Dictionary<Guid, bool>();
            var guidList = connectionGroupIds.Select(id => new Guid(id)).ToList();
            var allNodes = FileSystemSettingDao.LoadAllSettingByGroupIds(guidList).ToList();
            foreach (var node in allNodes)
            {
                result[node.ScopeId] = node.EnableRecordManagement == (int)Contract.Global.Object.EnableRecordManagementSetting.Disable;
            }
            return result;
        }
        private bool CurrentConnectionIsDisable(Guid connectionId, Guid connectionGroupId, Dictionary<Guid, bool> disableStatusDict)
        {
            var result = disableStatusDict.Where(d => d.Key == connectionId).FirstOrDefault();
            if (result.Key == Guid.Empty)
            {
                result= disableStatusDict.Where(d => d.Key == connectionGroupId).FirstOrDefault();
            }
            return result.Value;
        }
        private Dictionary<string, bool> BuildDisableStatusDictionary(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allDisablePath = FileSystemSettingDao.GetAllDisableRecordManagementPath(guid);

            var fsGroupSetting = FileSystemSettingDao.LoadFSSetting(guid, guid);
            var isGroupEnable = fsGroupSetting == null ? (int)Contract.Global.Object.EnableRecordManagementSetting.Enable : fsGroupSetting.EnableRecordManagement;
            var isGroupDisabled = isGroupEnable != (int)Contract.Global.Object.EnableRecordManagementSetting.Enable;
            if (isGroupDisabled)
            {
                result["GROUP_DISABLED"] = true;
                return result;
            }
            if (allDisablePath != null && allDisablePath.Count > 0)
            {
                foreach (var path in allDisablePath)
                {
                    result[path] = true;
                }
            }
            return result;
        }
        private bool CurrentNodeIsDisable(string folderPath, Dictionary<string, bool> disableStatusDict)
        {
            if (disableStatusDict == null || disableStatusDict.Count == 0)
            {
                return false;
            }
            if (disableStatusDict.TryGetValue("GROUP_DISABLED", out var isGroupDisabled) && isGroupDisabled)
            {
                return true;
            }

            foreach (var path in disableStatusDict.Keys)
            {
                if (path != "GROUP_DISABLED" && folderPath.StartsWith(path))
                {
                    return true;
                }
            }

            return false;
        }
        private RMMyhubClassifyItem FindFirstDisabledTarget(List<RMMyhubClassifyItem> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return null;
            }

            var disableStatusCache = new Dictionary<string, Dictionary<string, bool>>();

            foreach (var target in targets)
            {
                if (string.IsNullOrEmpty(target.PartitionKeyId) || string.IsNullOrEmpty(target.FullPath))
                {
                    continue;
                }

                if (!Guid.TryParse(target.PartitionKeyId, out var connectionGuid))
                {
                    logger.Warn($"Invalid PartitionKeyId format: {target.PartitionKeyId}, skipping disable check.");
                    continue;
                }

                var connection = FSConnectionDao.GetConnectionById(connectionGuid);
                if (connection == null)
                {
                    continue;
                }

                var connectionGroupId = connection.GroupId.ToString();

                if (!disableStatusCache.TryGetValue(connectionGroupId, out var disableStatusDict))
                {
                    disableStatusDict = BuildDisableStatusDictionary(connectionGroupId);
                    disableStatusCache[connectionGroupId] = disableStatusDict;
                }

                if (CurrentNodeIsDisable(target.FullPath, disableStatusDict))
                {
                    return target;
                }
            }

            return null;
        }
        public bool CurrentNodeIsDisableDownloadRCC(string connectionGroupId, string folderPath)
        {
            var groupId = new Guid(connectionGroupId);

            var allConfiguredNodes = FileSystemSettingDao.GetAllNodeRCCSettings(groupId);

            if (allConfiguredNodes != null && allConfiguredNodes.Any())
            {
                foreach (var node in allConfiguredNodes)
                {
                    if (folderPath.StartsWith(node.Key + "\\", StringComparison.OrdinalIgnoreCase) || folderPath.EqualsIgnoreCase(node.Key))
                    {
                        return !node.Value;
                    }
                }
            }

            bool isGroupEnable = FileSystemSettingDao.IsConnGroupEnableDownloadRCC(groupId);

            return !isGroupEnable;
        }
        private bool CurrentConnectionIsDisableDownloadRCC(Guid connectionId, Guid connectionGroupId, Dictionary<Guid, bool> disableStatusDict)
        {
            var result = disableStatusDict.Where(d => d.Key == connectionId).FirstOrDefault();
            if (result.Key == Guid.Empty)
            {
                result = disableStatusDict.Where(d => d.Key == connectionGroupId).FirstOrDefault();
            }
            return result.Value;
        }
        private Dictionary<Guid, bool> BuildDisableDownloadRCCStatusDictionary(List<string> connectionGroupIds)
        {
            var result = new Dictionary<Guid, bool>();
            var guidList = connectionGroupIds.Select(id => new Guid(id)).ToList();
            var allNodes = FileSystemSettingDao.LoadAllSettingByGroupIds(guidList).ToList();
            foreach (var node in allNodes)
            {
                result[node.ScopeId] = node.IsAllowUserDownloadRCCReport;
            }
            return result;
        }
        private Dictionary<string, bool> BuildDisableDownloadRCCStatusDictionary(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allConfiguredNodes = FileSystemSettingDao.GetAllNodeRCCSettings(guid);

            if (allConfiguredNodes != null && allConfiguredNodes.Any())
            {
                foreach (var node in allConfiguredNodes)
                {
                    result[node.Key] = !node.Value;
                }
            }
            bool isGroupEnable = FileSystemSettingDao.IsConnGroupEnableDownloadRCC(guid);

            result["GROUP_DISABLED"] = !isGroupEnable;

            return result;
        }
        private bool IsNodeDisabledDownloadRCC(string folderPath, Dictionary<string, bool> disableDownloadRCCStatusDict)
        {
            var matchedPath = disableDownloadRCCStatusDict.Keys.Where(path => folderPath.StartsWith(path + "\\", StringComparison.OrdinalIgnoreCase) ||
                       folderPath.Equals(path, StringComparison.OrdinalIgnoreCase)).OrderByDescending(path => path.Length).FirstOrDefault();

            if (string.IsNullOrEmpty(matchedPath) && disableDownloadRCCStatusDict.TryGetValue("GROUP_DISABLED", out var isGroupDisabled))
            {
                return isGroupDisabled;
            }

            return disableDownloadRCCStatusDict[matchedPath];
        }
        private bool IsFolderInactive(string folderPath, Dictionary<string, bool> inactiveStatusStatusDict)
        {
            var matchedPath= inactiveStatusStatusDict.Keys.Where(path => folderPath.StartsWith(path + "\\", StringComparison.OrdinalIgnoreCase) ||
                       folderPath.Equals(path, StringComparison.OrdinalIgnoreCase)).OrderByDescending(path => path.Length).FirstOrDefault();
            if (string.IsNullOrEmpty(matchedPath)|| inactiveStatusStatusDict.Count==0)
            {
                return false;
            }
            return inactiveStatusStatusDict[matchedPath];
        }
        private Dictionary<string, bool> BuildFolderInactiveStatusDictionary(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allConfiguredNodes = FileSystemSettingDao.GetAllDeactivePath(guid);

            if (allConfiguredNodes != null && allConfiguredNodes.Any())
            {
                foreach (var node in allConfiguredNodes)
                {
                    result[node.Key] = !node.Value;
                }
            }
            return result;
        }
        public List<RMMyhubReportAuditItem> GetMyhubReports(List<Guid> jobIds, int reportType, bool isMyhub = true)
        {
            var auditItems = new List<RMMyhubReportAuditItem>();
            List<int> finalJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.None,
                    (int)DownloadContentJobStatus.Calculating,
                    (int)DownloadContentJobStatus.Failed,
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.FinishWithException,
                    (int)DownloadContentJobStatus.Skipped,
                    (int)DownloadContentJobStatus.Stopped,
                    (int)DownloadContentJobStatus.Stopping
                };
            var contentInfoList = DownloadDataInfoDao.GetDownloadDataInfos(jobIds, finalJobStatus);

            foreach (var contentInfo in contentInfoList)
            {
                if (reportType == (int)MyhubReportJobType.HistoryContent)
                {
                    if (string.IsNullOrWhiteSpace(contentInfo.ExtendString1))
                    {
                        continue;
                    }

                    ManualApprovalHistoryOption historyInfo = null;

                    try
                    {
                        historyInfo = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(contentInfo.ExtendString1);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }
                    if (historyInfo != null && !string.IsNullOrEmpty(historyInfo.FullPath))
                    {
                        var auditItem = BuildHistoryAuditReprotAuditItem(historyInfo.Id, historyInfo.FullPath, historyInfo.DisplayName);
                        if (auditItem != null)
                        {
                            auditItems.Add(auditItem);
                        }
                    }
                }
                else if (reportType == (int)MyhubReportJobType.DownloadRCCReport)
                {
                    var rccInfos = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(contentInfo.ExtendString1 ?? string.Empty) ?? new List<RCCReportContentDto>();
                    if (rccInfos != null && rccInfos.Count > 0)
                    {
                        foreach (var rccInfo in rccInfos)
                        {
                            var auditItem = new RMMyhubReportAuditItem();
                            if (isMyhub)
                            {
                                auditItem = BuildRCCAuditReprotAuditItem(rccInfo.NodeId, rccInfo.Level, rccInfos[0].DisplayName);
                            }
                            else
                            {
                                auditItem = BuildRCCAuditReprotAuditItem(rccInfo.NodeId, rccInfo.Level, contentInfo.JobId);
                            }

                            if (auditItem != null)
                            {
                                auditItems.Add(auditItem);
                            }
                        }
                    }
                }
            }

            return auditItems;
        }

        private RMMyhubReportAuditItem BuildHistoryAuditReprotAuditItem(string nodeId, string fullPath, string displayName)
        {
            var auditItem = new RMMyhubReportAuditItem();
            var connection = FSConnectionDao.GetConnectionById(Guid.Parse(nodeId));
            if (connection != null)
            {
                auditItem.ConnGroupId = connection.GroupId;
                auditItem.ConnectionId = connection.Id;
                auditItem.FullPath = connection.UNCPath;
                auditItem.Level = (int)FSAuditLevel.Connection;
                auditItem.ReportName = displayName;
            }
            return auditItem;
        }

        private RMMyhubReportAuditItem BuildRCCAuditReprotAuditItem(string nodeId, int level, string displayName)
        {
            var auditItem = new RMMyhubReportAuditItem();

            if (level == (int)NodeLevel.SiteCollection)
            {
                var connection = FSConnectionDao.GetConnectionById(Guid.Parse(nodeId));
                if (connection != null)
                {
                    auditItem.ConnGroupId = connection.GroupId;
                    auditItem.ConnectionId = connection.Id;
                    auditItem.FullPath = connection.UNCPath;
                    auditItem.Level = (int)FSAuditLevel.Connection;
                    auditItem.ReportName = displayName;
                }
            }
            else
            {
                var node = ExplorerService.GetFSDBRecords(new List<Guid> { Guid.Parse(nodeId) }).FirstOrDefault();
                var connection = FSConnectionDao.GetConnectionById(Guid.Parse(node.AveSiteId));
                if (node != null && connection != null)
                {
                    auditItem.ConnGroupId = connection.GroupId;
                    auditItem.ConnectionId = connection.Id;
                    auditItem.FullPath = node.DirPath + "\\" + node.LeafName;
                    auditItem.Level = (level == (int)NodeLevel.FSFolder) ? (int)FSAuditLevel.Folder : (int)FSAuditLevel.File;
                    auditItem.ReportName = displayName;
                    auditItem.ItemId = new Guid(nodeId);
                }
            }

            return auditItem;
        }
        private async Task<string> ConvertStreamToBase64Async(Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        // Check Owner Permission
        public async Task<RAReturnMessage> CheckOwnerPermissionAsync(Guid connectionId, int reportType)
        {
            var result = new RAReturnMessage();
            var groupId = TenantLocalValue.LogonGroupId;
            var loginName = TenantLocalValue.LogonUserEmail;

            var curUser = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            var connIds = new HashSet<Guid>();
            if (reportType == (int)MyhubReportJobType.HistoryContent)
            {
                connIds = RMFSConnAndOwnerRela.GetConnectionsByUserIdsAndRoles(curUser.Distinct().ToList(), new List<AvePoint.RA.DB.Model.FSConnectionOwnerType> { AvePoint.RA.DB.Model.FSConnectionOwnerType.InformationOwner }).Result.Select(c => c.Id).ToHashSet();
            }
            else if (reportType == (int)MyhubReportJobType.DownloadRCCReport)
            {
                connIds = RMFSConnAndOwnerRela.GetConnectionsByUserIdsAndRoles(curUser.Distinct().ToList()).Result.Select(c => c.Id).ToHashSet();
            }
            var flag = connIds.Contains(connectionId);
            if (!flag)
            {
                logger.Warn("Current user is not has permission of the connection.");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "Current user is not has permission of the connection.";
                return result;
            }
            return result;
        }

        #endregion

        #region Optimize get Folder and File
        private async Task<RMMyhubFolderAndFileItemResult> ConvertToFolderAndFileAsync(List<RMMyhubFolderAndFileItem> resultsFromCosmos, FSConnection connectionInfo, GeneralSettingModel genernalSetting, string timeZoneId, bool isDaylight)
        {
            var folderAndFiles = new List<RMMyhubFolderAndFileItem>();
            if (resultsFromCosmos == null || resultsFromCosmos.Count == 0)
            {
                return new RMMyhubFolderAndFileItemResult()
                {
                    Items = folderAndFiles,
                    HasMore = false
                };
            }

            var connectionGroupId = connectionInfo.GroupId.ToString();
            var stopwatch = Stopwatch.StartNew();

            // Real async DAO calls (no Task.Run) - removes synchronous thread blocking
            // during the mapping phase.
            var disableStatusDictTask = BuildDisableStatusDictionaryAsync(connectionGroupId);
            var disableDownloadRccDictTask = BuildDisableDownloadRCCStatusDictionaryAsync(connectionGroupId);
            var folderInactiveDictTask = BuildFolderInactiveStatusDictionaryAsync(connectionGroupId);

            await Task.WhenAll(disableStatusDictTask, disableDownloadRccDictTask, folderInactiveDictTask);

            // Build prefix tries once, outside the per-item loop. This converts the
            // previous O(N * M) StartsWith/LINQ scan per item into an O(path depth) lookup.
            var disableTrie = new PathPrefixTrie(disableStatusDictTask.Result);
            var disableRccTrie = new PathPrefixTrie(disableDownloadRccDictTask.Result);
            var inactiveTrie = new PathPrefixTrie(folderInactiveDictTask.Result);

            stopwatch.Stop();
            logger.Info($"[FolderAndItems Performance] Build all dictionaries/tries completed in {stopwatch.ElapsedMilliseconds} ms");

            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), genernalSetting.DataFormatId), true)];

            // Memoize per-request ticks -> formatted-date conversions, since many items
            // commonly share identical retention start/end dates.
            var dateFormatCache = new Dictionary<long, string>();

            string FormatTicks(long ticks)
            {
                if (ticks <= 0)
                {
                    return null;
                }

                if (dateFormatCache.TryGetValue(ticks, out var cached))
                {
                    return cached;
                }

                var formatted = string.IsNullOrEmpty(timeZoneId)
                    ? GeneralSettingService.ConvertTiksToDateTime(genernalSetting, ticks, true).SimplifyFormatTime
                    : GeneralSettingService.ConvertTiksToDateTime(genernalSetting, ticks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;

                dateFormatCache[ticks] = formatted;
                return formatted;
            }

            var performanceLogListForLoop = new List<long>();
            var stopwatch2 = new Stopwatch();

            foreach (var cosmosItem in resultsFromCosmos)
            {
                stopwatch2.Restart();
                if (cosmosItem == null)
                {
                    continue;
                }

                var startDate = long.Parse(cosmosItem.StartDate ?? "0");
                var endDate = long.Parse(cosmosItem.EndDate ?? "0");

                // Compute the full path once and reuse it across all three status checks
                // instead of rebuilding the concatenated string per check.
                var fullPath = cosmosItem.IsFolder
                    ? BuildFolderFullPath(cosmosItem.Path, cosmosItem.Name)
                    : cosmosItem.Path;

                folderAndFiles.Add(new RMMyhubFolderAndFileItem
                {
                    Id = cosmosItem.Id,
                    NodeId = cosmosItem.NodeId,
                    Name = cosmosItem.Name,
                    Path = cosmosItem.Path + "\\" + cosmosItem.Name,
                    ClassCode = cosmosItem.ClassCode,
                    CountryCode = cosmosItem.CountryCode,
                    FileVolume = cosmosItem.FileVolume,
                    Size = cosmosItem.Size,
                    PendingDisposal = cosmosItem.PendingDisposal,
                    RecordId = cosmosItem.RecordId,
                    EndDate = endDate > 0 ? FormatTicks(endDate) : null,
                    StartDate = (startDate > 0 && cosmosItem.RetentionType == "1") ? FormatTicks(startDate) : null,
                    RetentionType = ConvertRetentionType(cosmosItem.RetentionType),
                    IsFolder = cosmosItem.IsFolder,
                    ExtentionForFile = cosmosItem.ExtentionForFile,
                    PartitionKeyId = cosmosItem.PartitionKeyId,
                    EnableRecordManagement = !disableTrie.IsPathDisabled(fullPath),
                    IsAllowDownloadRCC = !disableRccTrie.IsPathDisabled(fullPath),
                    IsActive = !inactiveTrie.IsPathDisabled(fullPath)
                });

                stopwatch2.Stop();
                performanceLogListForLoop.Add(stopwatch2.ElapsedMilliseconds);
            }

            if (performanceLogListForLoop.Any())
            {
                var avg = performanceLogListForLoop.Average();
                var max = performanceLogListForLoop.Max();
                var min = performanceLogListForLoop.Min();
                var p95 = performanceLogListForLoop.OrderBy(x => x).Skip((int)(performanceLogListForLoop.Count * 0.95)).FirstOrDefault();

                logger.Info($"[FolderAndItems Performance] ConvertToFolderAndFileAsync: " +
                            $"Count={performanceLogListForLoop.Count}, " +
                            $"Avg={avg:F2}ms, Max={max}ms, Min={min}ms, P95={p95}ms");
            }

            return new RMMyhubFolderAndFileItemResult
            {
                Items = folderAndFiles
            };
        }

        private async Task<Dictionary<string, bool>> BuildDisableStatusDictionaryAsync(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allDisablePathTask = FileSystemSettingDao.GetAllDisableRecordManagementPathAsync(guid);
            var fsGroupSettingTask = FileSystemSettingDao.LoadFSSettingAsync(guid, guid);

            await Task.WhenAll(allDisablePathTask, fsGroupSettingTask);

            var fsGroupSetting = fsGroupSettingTask.Result;
            var isGroupEnable = fsGroupSetting == null ? (int)Contract.Global.Object.EnableRecordManagementSetting.Enable : fsGroupSetting.EnableRecordManagement;
            var isGroupDisabled = isGroupEnable != (int)Contract.Global.Object.EnableRecordManagementSetting.Enable;
            if (isGroupDisabled)
            {
                result["GROUP_DISABLED"] = true;
                return result;
            }

            var allDisablePath = allDisablePathTask.Result;
            if (allDisablePath != null && allDisablePath.Count > 0)
            {
                foreach (var path in allDisablePath)
                {
                    result[path] = true;
                }
            }
            return result;
        }

        private async Task<Dictionary<string, bool>> BuildDisableDownloadRCCStatusDictionaryAsync(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allConfiguredNodesTask = FileSystemSettingDao.GetAllNodeRCCSettingsAsync(guid);
            var isGroupEnableTask = FileSystemSettingDao.IsConnGroupEnableDownloadRCCAsync(guid);

            await Task.WhenAll(allConfiguredNodesTask, isGroupEnableTask);

            var allConfiguredNodes = allConfiguredNodesTask.Result;
            if (allConfiguredNodes != null && allConfiguredNodes.Any())
            {
                foreach (var node in allConfiguredNodes)
                {
                    result[node.Key] = !node.Value;
                }
            }

            result["GROUP_DISABLED"] = !isGroupEnableTask.Result;
            return result;
        }

        private async Task<Dictionary<string, bool>> BuildFolderInactiveStatusDictionaryAsync(string connectionGroupId)
        {
            var result = new Dictionary<string, bool>();
            var guid = new Guid(connectionGroupId);

            var allConfiguredNodes = await FileSystemSettingDao.GetAllDeactivePathAsync(guid);
            if (allConfiguredNodes != null && allConfiguredNodes.Any())
            {
                foreach (var node in allConfiguredNodes)
                {
                    result[node.Key] = !node.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Segment-based prefix trie replacing the previous per-item O(M) StartsWith/LINQ
        /// scans against a flat dictionary of disable/inactive paths. Lookup cost is
        /// proportional to folder path depth rather than the number of configured override paths.
        /// </summary>
        private sealed class PathPrefixTrie
        {
            private sealed class Node
            {
                public Dictionary<string, Node> Children { get; } = new(StringComparer.OrdinalIgnoreCase);
                public bool? DisabledValue { get; set; }
            }

            private readonly Node _root = new();
            private readonly bool _groupDisabled;
            private readonly bool _hasEntries;

            public PathPrefixTrie(Dictionary<string, bool> sourceDictionary)
            {
                if (sourceDictionary == null || sourceDictionary.Count == 0)
                {
                    _hasEntries = false;
                    return;
                }

                if (sourceDictionary.TryGetValue("GROUP_DISABLED", out var groupDisabled))
                {
                    _groupDisabled = groupDisabled;
                }

                _hasEntries = true;

                foreach (var (path, disabled) in sourceDictionary)
                {
                    if (path == "GROUP_DISABLED")
                    {
                        continue;
                    }

                    var segments = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                    var node = _root;
                    foreach (var segment in segments)
                    {
                        if (!node.Children.TryGetValue(segment, out var child))
                        {
                            child = new Node();
                            node.Children[segment] = child;
                        }
                        node = child;
                    }
                    node.DisabledValue = disabled;
                }
            }

            public bool IsPathDisabled(string folderPath)
            {
                if (!_hasEntries)
                {
                    return false;
                }

                if (_groupDisabled)
                {
                    return true;
                }

                var segments = folderPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                var node = _root;
                bool? deepestMatch = null;

                foreach (var segment in segments)
                {
                    if (!node.Children.TryGetValue(segment, out var child))
                    {
                        break;
                    }

                    node = child;
                    if (node.DisabledValue.HasValue)
                    {
                        deepestMatch = node.DisabledValue;
                    }
                }

                return deepestMatch ?? false;
            }
        }
        #endregion

    }
}
