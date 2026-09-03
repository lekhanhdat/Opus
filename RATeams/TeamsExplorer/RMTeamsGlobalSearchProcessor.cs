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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.RACommonUtility.GlobalLocker;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using RAArchiverCommon.Utility;

namespace RATeams.TeamsExplorer
{
    public class RMTeamsGlobalSearchProcessor
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMTeamsGlobalSearchProcessor));
        protected IAveSite CurrentAveSite;
        private IExplorerDao _explorerDao { get; set; }
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }

        private ISettingProfilesDao mSettingProfileDao;

        private ISettingProfilesDao SettingProfileDao
        {
            get
            {
                if (mSettingProfileDao == null)
                {
                    mSettingProfileDao = (ISettingProfilesDao)PlatformWindsorManager.GetService(typeof(ISettingProfilesDao));
                }
                return mSettingProfileDao;
            }
        }

        private string? _generalRetentionLabel = null;

        public string GeneralRetentionLabel
        {
            get
            {
                if (_generalRetentionLabel == null)
                {
                    try
                    {
                        _generalRetentionLabel = GetGeneralRetentionLabel();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occured when GetGeneralRetentionLabel . Ex: {ex}");
                        _generalRetentionLabel = null;
                    }
                }
                return _generalRetentionLabel ?? string.Empty;
            }
        }

        private Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
        public int FailedCount = 0;
        public RMTeamsGlobalSearchProcessor()
        {
        }

        public async Task HandleDeclareRecords(List<Guid> recordIds, string tempJobId, bool isDeclare, string declaredBy)
        {
            logger.Info("Declared Records action start {0}", tempJobId);
            List<Record> records = new List<Record>();
            var startTime = DateTime.Now;
            if (recordIds != null && recordIds.Count > 0)
            {
                records = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                logger.Warn($" [Declare] 1.time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                var avesiteIds = recDic.Keys.ToList();
                Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                List<Guid> failedIds = new List<Guid>();
                var isNewLogicAccount = TenantService.IsNewOpusTenant();
                if (avesiteIds.Count > 0)
                {
                    siteDic = RABrowserClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                    logger.Warn($"[Declare] 2.time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    foreach (var recList in recDic.Values)
                    {
                        if (recList.Count > 0)
                        {
                            try
                            {
                                if (siteDic.ContainsKey(recList[0].AveSiteId))
                                {
                                    var site = siteDic[recList[0].AveSiteId];
                                    startTime = DateTime.Now;
                                    var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(site);
                                    logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                                    var spSite = factory.CreateSite();
                                    CurrentAveSite = spSite;
                                    logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    var IRecords = factory.CreateRecords();
                                    logger.Warn($"[Declare] 4.2.time elapsed for CreateRecords {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    spSite.EnsureRecordFeatureEnabled(mRecordFeatureId);
                                    logger.Warn($"[Declare] 4.3.time elapsed for EnsureRecordFeatureEnabled {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    if (AccountUtility.IsSupportRecordLabel())
                                    {
                                        if (isDeclare)
                                        {
                                            startTime = DateTime.UtcNow;
                                            (List<Guid> successIds, failedIds) = await AddRecordLabelToItem(spSite, recList, IRecords);
                                            logger.Warn($"[AddRecordLabel] 5.time elapsed for Add Record Label {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.UtcNow;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.LockedByRecordLabel = true; rec.ApplyRecordLabelBy = declaredBy; rec.DeclaredBy = declaredBy; rec.DeclareAsRecord = false; });
                                            logger.Warn($"[Declare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            if (successIds != null && successIds.Count > 0)
                                            {
                                                RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_AddRecordLabel");
                                            }
                                        }
                                        else
                                        {
                                            startTime = DateTime.Now;
                                            (List<Guid> successIds, failedIds) = RemoveRecordLabel(spSite, recList);
                                            logger.Warn($"[UnDeclare] 5.time elapsed for undeclare records {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.LockedByRecordLabel = false; rec.ApplyRecordLabelBy = declaredBy; rec.DeclaredBy = declaredBy; });
                                            logger.Warn($"[UnDeclare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            if (successIds != null && successIds.Count > 0)
                                            {
                                                RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_RemoveRecordLabel");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (isDeclare)
                                        {
                                            startTime = DateTime.Now;
                                            (List<Guid> successIds, failedIds) = await DeclaredRecordAsync(IRecords, spSite, recList);
                                            logger.Warn($"[Declare] 5.time elapsed for declare teams record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
                                            logger.Warn($"[Declare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");

                                            if (successIds != null && successIds.Count > 0)
                                            {
                                                RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_DeclareAsRecord");
                                            }
                                        }
                                        else
                                        {
                                            startTime = DateTime.Now;
                                            List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
                                            logger.Warn($"[UnDeclare] 5.time elapsed for undeclare records {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            startTime = DateTime.Now;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
                                            logger.Warn($"[UnDeclare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                            if (successIds != null && successIds.Count > 0)
                                            {
                                                RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_History_UndeclareAsRecord");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Error($"Site collection not found. Id:{recList[0].AveSiteId}");
                                    throw new Exception("RM_RDM_SCNotFound");
                                }
                            }
                            catch (Exception ee)
                            {
                                failedIds.AddRange(recList.Select(t => t.Id));
                                logger.Warn("Declared Records action failed {0}", ee.ToString());
                                
                                foreach (var record in recList)
                                {
                                    AddDeclareDetailForGlobalSearch(record, JobDetailsStatus.Failed, getRealException(ee), isDeclare, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                }                               
                            }
                        }
                    }
                }
                if (failedIds.Count > 0)
                {
                    FailedCount += failedIds.Count;
                    string failedNames = string.Empty;
                    foreach (var fid in failedIds)
                    {
                        failedNames += records.Where(t => t.Id == fid).FirstOrDefault()?.LeafName + "; ";
                    }
                    if (!string.IsNullOrEmpty(failedNames))
                    {
                        failedNames = failedNames.Trim().TrimEnd(';');
                    }
                    
                    throw new Exception(failedNames);      
                }
            }
        }

        private string GetGeneralRetentionLabel()
        {
            SettingProfileDto profileDto = new SettingProfileDto
            {
                Type = (int)SettingProfilesType.RecordsLabelSetting,
                Name = "RecordsLabelSetting"
            };
            var dto = SettingProfileDao.Load(profileDto);
            return dto?.Settings ?? string.Empty;
        }

        private async Task<(List<Guid> successIds, List<Guid> failedIds)> AddRecordLabelToItem(IAveSite site, List<Record> records, IAveORecords IRecords)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            AveComplianceTagInfo sharePointRetentionLabel;
            try
            {
                try
                {
                    var availableTags = site.GetAvailableTagsForSite();
                    sharePointRetentionLabel = availableTags.FirstOrDefault(_ => _.TagName.Equals(GeneralRetentionLabel, StringComparison.OrdinalIgnoreCase));
                    if (sharePointRetentionLabel == null)
                    {
                        throw new Exception($"Can not find {GeneralRetentionLabel} in current site {site?.Url}");
                    }
                    if (!(sharePointRetentionLabel.BlockDelete && sharePointRetentionLabel.BlockEdit))
                    {
                        throw new Exception("StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                    }
                }
                catch (Exception e)
                {
                    failedIds = records != null ? records.Select(r => r.Id).ToList() : failedIds;
                    logger.Error($"Error occurred while get site retention label. Site Url:{site?.Url} Error:{e.ToString()}");
                    ArgumentNullException.ThrowIfNull(records);
                    foreach (var record in records)
                    {
                        AddRecordLabelDetailForGlobalSearchJob(record, JobDetailsStatus.Failed, e.Message, true, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                    }
                    throw;
                }
                foreach (var record in records)
                {
                    logger.Info($"Add record label to file {record.Id}");
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        if (CheckisRecord(item))
                        {
                           IRecords.UndeclareItemAsRecord(item);
                        }
                        item.LockRecordItem();
                        item.SetComplianceTag(sharePointRetentionLabel.TagName, true, true, false, false);
                        successIds.Add(record.Id);
                        AddRecordLabelDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Add record label to items failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Add record label to item failed {0}", e.ToString());
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }

            return (successIds, failedIds);
        }

        private (List<Guid> successIds, List<Guid> failedIds) RemoveRecordLabel(IAveSite site, List<Record> records)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            Dictionary<string, AveComplianceTagInfo> sharePointRetentionLabels;
            try
            {
                try
                {
                    var availableTags = site.GetAvailableTagsForSite();
                    sharePointRetentionLabels = availableTags.ToDictionary(_ => _.TagName);
                }
                catch (Exception e)
                {
                    logger.Warn($"Init retention label for site {site?.Url} has errors: {e.Message}");
                    sharePointRetentionLabels = new();
                }

                foreach (var record in records)
                {
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        var retentionLabelNameOfItem = item.GetComplianceTagName();
                        if (sharePointRetentionLabels.TryGetValue(retentionLabelNameOfItem, out var tagInfo))
                        {
                            if (tagInfo.BlockDelete && tagInfo.BlockEdit)
                            {
                                logger.Info($"remove record label of file {WebUtil.MakeFullUrl(site.Url, record.DirPath)}");
                                item.SetComplianceTagOnBulkItems("");
                                successIds.Add(record.Id);
                                AddRecordLabelDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);
                            }
                        }
                        else
                        {
                            logger.Warn($"Can not find current retention label {retentionLabelNameOfItem} of file in current site {WebUtil.MakeFullUrl(site.Url, record.DirPath)}");
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Warn("Remove record label to items failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                AddRecordLabelDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, false);
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return (successIds, failedIds);
        }

        private void AddRecordLabelDetailForGlobalSearchJob(Record record, JobDetailsStatus status, string comment, bool isDeclare, bool isDocument)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : CurrentAveSite == null ? record.DirPath : WebUtil.MakeFullUrl(CurrentAveSite.Url, record.DirPath),
                Action = isDeclare ? "RM_BCM_History_AddRecordLabel" : "RM_BCM_History_RemoveRecordLabel",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
        }

        private void AddRecordLabelDetailForGlobalSearchJob(IAveListItem item, JobDetailsStatus status, string comment, bool isDeclare)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = item?.Name,
                FullPath = item?.FullPath(),
                Action = isDeclare ? "RM_BCM_History_AddRecordLabel" : "RM_BCM_History_RemoveRecordLabel",
                Status = status,
                Comment = comment,
                Type = item == null ? "" : item.File != null ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_JS_Rule_ObjectLevel_Item"
            });
        }


        public async Task<(List<Guid>, List<Guid>)> DeclaredRecordAsync(IAveORecords IRecords, IAveSite site, List<Record> records)
        {
            List<Guid> successIds = new List<Guid>();
            List<Guid> failedIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                try
                {
                    if (!site.CheckDeclarationSettingIsBlockEditAndDelete() && !site.IsOD4BSite())
                    {
                        var testRecord = records.First();                   
                        var remoteSite = RABrowserClient.GetRemoteSiteCollectionsByIdList(new List<string> { testRecord.AveSiteId }).FirstOrDefault();
                        var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite);
                        var factory = MultiAppUtil.CreateAveObjectModelFactory(site.Url, bposInfo, AveContextKind.ClientObjectModel);
                        IAveSiteProperties siteProperties = null;
                        try
                        {
                            IAveTenant tenant = factory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, site.Url));
                            siteProperties = tenant.GetSitePropertiesByUrl(site.Url);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Init site properties failed {site.Url}:{e}");
                        }
                        site.EnsureWebDeclarationSetting();
                    }
                }
                catch (Exception e)
                {
                    failedIds = records != null ? records.Select(r => r.Id).ToList() : failedIds;
                    logger.Error($"Error occurred while DisableDenyAddAndCustomizePages. Site Url:{site?.Url} Error:{e.ToString()}");
                    ArgumentNullException.ThrowIfNull(records);
                    foreach (var record in records)
                    {
                        AddDeclareDetailForGlobalSearch(record, JobDetailsStatus.Failed, e.Message, true, record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                    }
                    
                    throw;
                }
                foreach (var record in records)
                {
                    logger.Info("Declared Records {0}", record.Id);
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        if (!AveSiteExtension.IsBlockEditAndDeleteRecord(item))
                        {
                            if (item.FieldValues.ContainsKey("CheckoutUser") &&
                                item.FieldValues["CheckoutUser"] != null && !string.IsNullOrEmpty(item.FieldValues["CheckoutUser"].ToString()))
                            {
                                logger.Warn("The file is in Checked out status, cannot be declared now. File UniqueId: {0} RowId:{1}", item.UniqueId, item.ID);
                                failedIds.Add(record.Id);
                               
                                AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, "RM_JM_GlobalSearch_DeclareCheckout", true);
                                
                            }
                            else
                            {
                                if (CheckisRecord(item))
                                {
                                    IRecords.UndeclareItemAsRecord(item);
                                }
                                var lockerKey = web.Site.ID.ToString();
                                bool lockStatus = false;
                                try
                                {
                                    lockStatus = await RMGlobalLocker.GetRecordsLockerAsync(lockerKey);
                                    site.EnsureWebDeclarationSetting();
                                    IRecords.DeclareItemAsRecord(item);
                                    successIds.Add(record.Id);
                             
                                    AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, true);                      
                                }
                                catch (Exception ex)
                                {
                                    failedIds.Add(record.Id);           
                                    AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, ex.Message, true);
                                    
                                    logger.Error("error occurred while process items,ERROR:{0}", ex.ToString());
                                }
                                finally
                                {
                                    if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                                    {
                                        await RMGlobalLocker.ReleaseRecordsLockerAsync(lockerKey);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath), e.ToString());
                        failedIds.Add(record.Id);
                        AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return (successIds, failedIds);
        }

        public List<Guid> UnDeclaredRecord(IAveORecords IRecords, IAveSite site, List<Record> records, ref List<Guid> failedIds)
        {
            List<Guid> successIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                foreach (var record in records)
                {
                    logger.Info("UnDeclared Records {0}", record.Id);
                    IAveListItem item = null;
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {  
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        item = list.GetItemByUniqueId(record.ItemId);
                        if (CheckisRecord(item))
                        {
                            IRecords.UndeclareItemAsRecord(item);
                            successIds.Add(record.Id);
                            
                            AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);                            
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", record.FullPath, e.ToString());
                        failedIds.Add(record.Id);
                        
                        AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, e.Message, false);                      
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                
                AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, false);               
            }
            finally
            {
                try
                {
                    site?.Dispose();
                    web?.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return successIds;
        }

        private void AddDeclareDetailForGlobalSearchJob(IAveListItem item, JobDetailsStatus status, string comment, bool isDeclare)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = item?.Name,
                FullPath = item?.FullPath(),
                Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                Status = status,
                Comment = comment,
                Type = item == null ? "" : item.File != null ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_JS_Rule_ObjectLevel_Item"
            });
        }

        private void AddDeclareDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDeclare, bool isDocument)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : CurrentAveSite == null ? record.DirPath : WebUtil.MakeFullUrl(CurrentAveSite.Url, record.DirPath),
                Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
        }

        protected string getRealException(Exception e)
        {
            if (e == null)
            {
                return null;
            }
            if (e is System.Reflection.TargetInvocationException && e.InnerException != null)
            {
                return getRealException(e.InnerException);
            }
            return e.Message;
        }

        public bool CheckisRecord(IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
            {
                isRecord = true;
            }
            return isRecord;
        }

    }
}
