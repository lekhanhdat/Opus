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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
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
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Discover
{
    public class OneDriveCreationAndDestroyedReportProcessor : RMOneDriveReportProcessor
    {
        private const string CONTENT_TYPE_DOCUMENT_NAME = "Document";
        private const string CONTENT_TYPE_OfficeDataConnectionFile_NAME = "Office Data Connection File";
        private const string CONTENT_TYPE_Folder_NAME = "Folder";
        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";
        private const string ARCHIVER_XML_NODE_NAME = "Name";
        private const string ARCHIVER_XML_NODE_VALUE = "Value";
        private const string ARCHIVER_XML_NODE_CONTENT_TYPE = "content type";
        private const string ARCHIVER_XML_NODE_MODIFIED_BY = "modified by";
        private const string ARCHIVER_XML_NODE_LIFECYCLE_STATUS = "lifecycle status";
        private const string ARCHIVER_XML_NODE_AVAILABILITY = "availability";
        private const string ARCHIVER_XML_NODE_CURRENTLY_HELD_BY = "currently held by";

        private string commomErrorMessage = string.Empty;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private RMCreationJobMessage msg = null;
        private IArchiverTableDao mArchiverTableDao = null;
        private AzureTableConnectContract mAzureTableConnectInfo = null;
        private string mTenantGroupId = TenantLocalValue.LogonGroupId;
        private int itemsPerTask = 10000; // items count per task, default value=10000;
        private Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        private DestrunctionReportHelper destrunctionReportHelper = null;
        private IRuleManagerService mRuleManagerService;
        protected IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }



        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private IExplorerDao explorerDao = new ExplorerDao();

        public OneDriveCreationAndDestroyedReportProcessor(RMCreationJobMessage msg)
            : base(msg.JobID, (int)JobType.OneDriveCreateAndDestroyedFileReport, false)
        {
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_DISCOVER_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            mLog.Info($"SPOItemsPerTask : {itemsPerTask}");

            commomErrorMessage = "RM_TS_SS_Summary";
            this.msg = msg;
            this.msg.EndTime = this.msg.EndTime.AddDays(1);//包含当天
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
            destrunctionReportHelper = new DestrunctionReportHelper(startUtcTime, endUtcTime);
            mArchiverTableDao = (IArchiverTableDao)PlatformWindsorManager.GetService(typeof(IArchiverTableDao));
        }

        public override async System.Threading.Tasks.Task RunReportJobAsync()
        {
            try
            {
                await InitRulesInfoAsync();
                foreach (var SiteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    groupId = SiteCollectionNodeItem.Parent.Id;
                    siteId = SiteCollectionNodeItem.Id;
                    await ProcessSiteAsync(SiteCollectionNodeItem);
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
            }
            catch (Exception ex)
            {
                mJobHasException = true;
                mLog.Error($"occured error,msg:{ex.Message},stackTrace:{ex.StackTrace}");
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                if (finalStatus == JobStatus.Finished)
                {
                    commomErrorMessage = string.Empty;
                }
                ReportManager.SetJobFinished(finalStatus, commomErrorMessage);
            }
        }

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem siteNode)
        {
            Guid siteId = Guid.Empty;
            bool needQueryLiteDB = destrunctionReportHelper.IsNeedQueryLiteDB();
            if (msg.SelectDestroyed && TenantService.IsNewOpusTenant() && needQueryLiteDB)
            {              
                siteId = await GetSiteIdAsync(siteNode);
                LoadDestructionCache(siteId);
            }
            await base.ProcessSiteAsync(siteNode);
            if (msg.SelectDestroyed && TenantService.IsNewOpusTenant() && needQueryLiteDB)
            {
                ClearDestructionCache(siteId);
            }
        }

        private async Task<Guid> GetSiteIdAsync(NodeItem siteNode)
        {
            var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrlV1(siteNode.FullPath);
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
            var mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            var CurrentSite = mfactory.CreateSite(siteNode.FullPath);
            return CurrentSite.ID;
        }

        private void LoadDestructionCache(Guid siteId)
        {
            string filePath = String.Empty;
            var timeRange = destrunctionReportHelper.GetQueryLiteDBTimeRange();
            using (PerformanceScope scope = new PerformanceScope("CreationAndDestroyedFileReportProcessor.DownloadCacheFromStorage"))
            {
                filePath = DestructionFactory.GetInstance(siteId.ToString(), string.Empty).DownloadCacheFromStorage(siteId.ToString(), timeRange.Item1, timeRange.Item2);
            }           
            if (!string.IsNullOrWhiteSpace(filePath) && System.IO.Directory.Exists(filePath))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(filePath);
                foreach (var file in dir.GetFiles())
                {
                    DestructionUtility destructionUtility = new DestructionUtility(file.FullName);
                    int readCount = 0;
                    int pageSize = 100;
                    int pageIndex = 0;
                    do
                    {
                        var records = destructionUtility.SelectValuesFromDB(pageIndex, pageSize);
                        pageIndex += records.Count;
                        readCount = records.Count;
                        DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString())).Insert(records);
                    }
                    while (readCount == 100);

                }
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch(Exception e)
                {
                    mLog.Error($"Delete file failed. Error : {e}");
                }
            }
            else
            {
                mLog.Warn("Destruction cache file not exist.");
            }
            DestructionFactory.Dispose(siteId.ToString(), string.Empty);
        }

        private string GetLiteDBPath(string siteId)
        {
            return SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER], "DestructionLiteDB", siteId);
        }

        private void ClearDestructionCache(Guid siteId)
        {
            DestructionFactory.Dispose(siteId.ToString(), string.Empty);
            DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString())).Dispose();
        }

        private List<ArchiverTableEntity> GetEntitiesFromLiteDB(Guid siteId, Guid listId, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            List<ArchiverTableEntity> entities = new List<ArchiverTableEntity>();
            var LiteDBWrapper = DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString()));
            int index = 0;
            int pageSize = 1000;
            bool hasMore = true;
            List<DestructionReport> records = null;
            do
            {
                using (new PerformanceScope("CreationAndDestroyedFileReportProcessor.QueryAllByPage", addToStatistics: true))
                {
                    records = LiteDBWrapper.QueryAllByPage(index, pageSize, listId);
                }
                if (records != null && records.Count > 0)
                {
                    index++;
                    hasMore = true;
                    records = records.Where(r => r.ArchivedTime >= queryStartUtcTime.Ticks && r.ArchivedTime <= queryEndUtcTime.Ticks).ToList();
                    if (records.Count > 0)
                    {
                        entities.AddRange(records.ConvertAll(r => ConvertDestructionReport2ArchiverTableEntity(r)));
                    }
                }
                else
                {
                    hasMore = false;
                }
            } while (hasMore);
            return entities;
        }

        private async Task<List<ArchiverTableEntity>> GetEntitiesFromArhicverTableAsync(IAveList list, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            var mDocAveClient = new DAOAPIClientV1(true);
            mAzureTableConnectInfo = await mDocAveClient.GetArchiverDataBaseConfigAsync();
            List<ArchiverTableEntity> infos = mArchiverTableDao.GetDestroyedItemsByListIdForOneDrive(mAzureTableConnectInfo, mTenantGroupId, list.ParentWeb.Site.ID.ToString(), list.ID, queryStartUtcTime, queryEndUtcTime);
            return infos;
        }

        private ArchiverTableEntity ConvertDestructionReport2ArchiverTableEntity(DestructionReport destructionReport)
        {
            ArchiverTableEntity archiverTableEntity = new ArchiverTableEntity()
            {
                NodeID = new Guid(destructionReport.NodeId),
                RuleID = destructionReport.RuleID,
                JsonMeta = destructionReport.JsonMeta
            };
            return archiverTableEntity;
        }

        protected override async System.Threading.Tasks.Task ProcessWebAsync(NodeItem webNode, bool IsProcessLists = true)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, webNode.Id))
            {
                AddDisabledReportDetail(webNode);
                mLog.Info("Process web sharepoint setting is disable {0}", webNode.FullPath);
                return;
            }
            await base.ProcessWebAsync(webNode, IsProcessLists);
        }

        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem listNode)
        {
            if (!OneDriveSettingDao.GetSettingEnableInfoByScope(groupId, siteId, listNode.Id))
            {
                AddDisabledReportDetail(listNode);
                mLog.Info("Process list sharepoint setting is disable {0}", listNode.FullPath);
                return;
            }

            if (msg.SelectDestroyed)
            {
                var mList = listNode.DiscoverObj as IAveList;
                await BuildDestroyedReportAsync(mList);
            }
            if (msg.SelectCreated)
            {
                await base.ProcessListAsync(listNode);
            }
        }

        protected override ExplorerQueryV2Dto GetFilterOption(Guid scopeId, Guid listId)
        {
            var filter = base.GetFilterOption(scopeId, listId);
            filter.QueryOption.FilterOption.CreatedDateInfo = new Contract.RMWeb.DateInfo
            {
                Condition = Contract.RMWeb.DateCondition.FromTo,
                //TimeZoneId = this.msg.GlobalTimeZoneId.Replace("_", " "),
                TimeZoneId = "UTC",
                Value1 = startUtcTime.ToString(),
                Value2 = endUtcTime.ToString(),
            };
            return filter;
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<BaseRecordDto> items)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("OneDriveCreationAndDestroyedReportProcessor.ProcessItems"))
                {
                    var siteId = web.Site.ID;
                    var recordIds = items.Select(o => o.Id).ToList();
                    if (items.Count > itemsPerTask)
                    {
                        results = RunMultiThreadsProcessItems(items, list);
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            results += ProcessOneItem(list, item);
                        }
                    }
                }
            }
            return results;
        }

        private async System.Threading.Tasks.Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope($"Report.GetRules"))
            {
                var dbRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    idRuleInfoDic = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        private RMRuleInfos GetRuleInfo(Guid id)
        {
            return idRuleInfoDic.ContainsKey(id) ? idRuleInfoDic[id] : null;
        }

        private int RunMultiThreadsProcessItems(List<BaseRecordDto> items, IAveList list)
        {
            mLog.Info($"Run multi threads to process items, items count : {items.Count}");
            var cts = new CancellationTokenSource();
            var t = AveTenantTasks.RunAndWaitResult(items, cts, item =>
            {
                return ProcessOneItem(list, item, cts);
            });
            return t;
        }

        private int ProcessOneItem(IAveList list, BaseRecordDto item, CancellationTokenSource cts = null)
        {
            var result = 0;
            try
            {
                ReportManager.Increase();
                DateTime itemCreateTime = new DateTime(item.TimeCreated);
                //只Report设置时间段的数据
                if (itemCreateTime > startUtcTime && itemCreateTime < endUtcTime)
                {
                    SendJobDetail(item, OperationType.Created, null);
                }
            }
            catch (JobStopException ex)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                mJobHasException = true;
                JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
                detail.Title = string.Format("Web Url:[{0}],List title:[{1}]", list.ParentWeb.Url, list.Title);
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                ReportManager.SendJobDetail(ConvertToDetail(detail));
                //result.HasFailed = true;
                mLog.Error("Report list failed,web url:[{0}]list title:[{1}],error:{2}", list.ParentWeb.Url, list.Title, e);
            }
            return result;
        }

        private async System.Threading.Tasks.Task BuildDestroyedReportAsync(IAveList list)
        {
            List<ArchiverTableEntity> infos = new();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    try
                    {
                        var queryArchiverTableTimeRange = destrunctionReportHelper.GetQueryArchiverTableTimeRange();
                        if (queryArchiverTableTimeRange != null)
                        {
                            List<ArchiverTableEntity> destroyedItemsInArchiverTable = await GetEntitiesFromArhicverTableAsync(list, queryArchiverTableTimeRange.Item1, queryArchiverTableTimeRange.Item2);
                            if (destroyedItemsInArchiverTable != null)
                            {
                                infos = infos.Concat(destroyedItemsInArchiverTable).ToList();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"An error while get data from archiver table, message: {ex}");
                    }

                    var queryLiteDBTimeRange = destrunctionReportHelper.GetQueryLiteDBTimeRange();
                    if (queryLiteDBTimeRange != null)
                    {
                        List<ArchiverTableEntity> destroyedItemsInLiteDB = GetEntitiesFromLiteDB(list.ParentWeb.Site.ID, list.ID, queryLiteDBTimeRange.Item1, queryLiteDBTimeRange.Item2);
                        if (destroyedItemsInLiteDB != null)
                        {
                            infos = infos.Concat(destroyedItemsInLiteDB).ToList();
                        }
                    }
                }
                else
                {
                    infos = await GetEntitiesFromArhicverTableAsync(list, startUtcTime, endUtcTime);
                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLog.Error("Failed to retrieve the one drive archived data that meets the rule settings. table not exist, List Title:[{0}],error:{1}", list.Title, ex.ToString());
                commomErrorMessage = I18NEntity.GetString("RM_DAM_NoTable");
                mJobHasException = true;
            }
            catch (SqlException se)
            {
                //REC-2281
                mLog.Warn("Get one drive destroyed items failed,error:{0}", se.ToString());
            }
            List<Guid> destroyedNodeIds = new List<Guid>();
            Dictionary<Guid, int> cacheAllRecordsApprovedBy = [];
            Dictionary<Guid, int> cacheAllRecordsApprovedStatus = [];
            Dictionary<Guid, int> cacheAllRecordsInternalApprovedStatus = [];
            for (int i = 0; i < infos.Count; i += 1000)
            {
                var temps = explorerDao.GetRecordByIds(infos.Skip(i).Take(1000).Select(info => GetRecordId(Site.ID, info.NodeID)).ToList());
                foreach (var item in temps)
                {
                    if(item?.ManualArchiveStatus == (int)Contract.Schedule.ActionStatus.Archiverd)
                    {
                        if (item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                        {
                            cacheAllRecordsInternalApprovedStatus.TryAdd(item.Id, (int)SOApproveDBStatus.Cancelled);
                            cacheAllRecordsApprovedStatus.TryAdd(item.Id, (int)SOApproveDBStatus.Cancelled);
                        }
                        else
                        {
                            cacheAllRecordsInternalApprovedStatus.TryAdd(item.Id, item.ManualInternalApprovedStatus);
                            cacheAllRecordsApprovedStatus.TryAdd(item.Id, item.ManualApprovedStatus);
                        }
                    }
                    if (item.ManualApprovedStatus != (int)Contract.SOApproveDBStatus.Rejected)
                    {
                        cacheAllRecordsApprovedBy.TryAdd(item.Id, item.ManualApprovedBy); 
                    }
                }
            }
            var cacheAllUsers = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            if (infos != null && infos.Count > 0)
            {
                ReportManager.IncreaseBase(infos.Count);
                foreach (ArchiverTableEntity info in infos)
                {
                    ReportManager.Increase();
                    //去除重复项
                    if (destroyedNodeIds.Contains(info.NodeID))
                    {
                        mLog.Debug("Dup node id {0}", info.NodeID);
                        continue;
                    }
                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
                    try
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            var asd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(info.JsonMeta);
                            detail.CreatedTime = asd.CreatedTime;
                            detail.LastModifiedTime = asd.CDLastModifiedTime;
                            detail.FileType = asd.FileType;
                            detail.URL = WebUtil.MakeFullUrl(list.ParentWeb.Url, asd.Path.Replace('\\', '/'));
                            detail.Title = asd.LeafName;
                            detail.Operation = (int)OperationType.Destroyed;
                            detail.OperationTime = asd.ArchivedTime.Ticks.ToString();
                            detail.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                            detail.TermName = asd.OnedriveTermName;
                            if (string.IsNullOrEmpty(asd.Metadata))
                            {
                                mLog.Warn("Fields info is null,Url:[{0}]", detail.URL.LogBase64());
                                continue;
                            }
                            else if (!string.IsNullOrEmpty(asd.Metadata))
                            {
                                #region Get metadata
                                ObjectLevel oLevel = ObjectLevel.None;
                                mLog.Info("LeafName:[{0}],FieldsInfo:[{1}]", asd.LeafName.LogBase64(), asd.Metadata);
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(asd.Metadata);
                                XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
                                foreach (XmlNode node in root.ChildNodes)
                                {
                                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
                                    string termId = string.Empty;
                                    switch (fieldName)
                                    {
                                        case ARCHIVER_XML_NODE_CONTENT_TYPE:
                                            oLevel = GetObjectLevelByContentType(fieldValue);
                                            break;
                                        case ARCHIVER_XML_NODE_MODIFIED_BY:
                                            //既然OperationBy信息是写死的, 那就不用判断Metadata了.
                                            //detail.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                                            break;
                                        case ARCHIVER_XML_NODE_LIFECYCLE_STATUS:
                                            detail.LifecycleStatus = fieldValue;
                                            break;
                                        case ARCHIVER_XML_NODE_AVAILABILITY:
                                            detail.Availablity = fieldValue;
                                            break;
                                        case ARCHIVER_XML_NODE_CURRENTLY_HELD_BY:
                                            detail.CurrentHeldBy = fieldValue;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                if (oLevel == ObjectLevel.None)
                                {
                                    oLevel = ObjectLevel.Document;
                                }
                                detail.ObjectLevel = oLevel.ToString();
                                #endregion
                            }
                            else
                            {
                                detail.ObjectLevel = ObjectLevel.Document.ToString();
                            }
                            detail.DisposalClass = GetRuleInfo(info.RuleID)?.DisposalClass;
                            detail.RuleName = GetRuleInfo(info.RuleID)?.RuleName;
                            detail.RecordsId = asd.RecordsId;
                            if (cacheAllRecordsApprovedBy.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int approvedBy))
                            {
                                if (cacheAllUsers.TryGetValue(approvedBy, out RMAccount approveUser))
                                {
                                    detail.ApprovedBy = approveUser.DisplayName;
                                    detail.ApprovedByUPN = approveUser.UserPrincipalName;
                                }
                            }
                            if (cacheAllRecordsApprovedStatus.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int approvedStatus))
                            {
                                detail.ApprovalStatus = approvedStatus;
                            }
                            if (cacheAllRecordsInternalApprovedStatus.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int internalApprovedStatus))
                            {
                                detail.InternalApprovedStatus = internalApprovedStatus;
                            }

                            detail.Status = JobDetailsStatus.Successful;
                            destroyedNodeIds.Add(info.NodeID);
                            ReportManager.SendJobReport(ConvertToReport(detail));
                            ReportManager.SendJobDetail(ConvertToDetail(detail));
                        }
                    }
                    catch (JobStopException ex)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        detail.Status = JobDetailsStatus.Failed;
                        detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                        ReportManager.SendJobDetail(ConvertToDetail(detail));
                        mLog.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
                    }
                }
            }
        }

        public static Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return new Guid(HashCodeHelper.ToMD5HashCode(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()));
        }

        protected override void AddDisabledReportDetail(NodeItem item)
        {
            var detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = JobReportUtility.ConvertItemTypeForDetails(item.NodeLevel);
            detail.Title = item.NameOrTitle;
            detail.URL = item.FullPath;
            detail.Status = JobDetailsStatus.Skipped;
            detail.Comment = "RM_JS_JMD_DisableRecordManagement";
            //ReportManager.SendJobReport(ConvertToReport(detail));
            ReportManager.SendJobDetail(ConvertToDetail(detail));
        }

        private void SendJobDetail(BaseRecordDto item, OperationType operationType, Record record)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                ObjectLevel oLevel = ObjectLevel.Document;
                detail.CreatedTime = item.TimeCreated;
                detail.LastModifiedTime = item.TimeLastModified;
                detail.FileType = item.ExtensionForFile;
                detail.ObjectLevel = oLevel.ToString();
                detail.Title = item.LeafName;
                DateTime operationTimeDt = DateTime.MinValue;
                string operationByStr = string.Empty;
                switch (operationType)
                {
                    case OperationType.Created:
                        operationTimeDt = new DateTime(item.TimeCreated);
                        operationByStr = item.CreatedBy;
                        break;
                    case OperationType.Destroyed:
                        operationTimeDt = new DateTime(item.TimeLastModified);
                        operationByStr = item.ModifiedBy;
                        if(record?.ManualArchiveStatus == (int)Contract.Schedule.ActionStatus.Archiverd)
                        {
                            if(record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                            {
                                detail.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                                detail.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                            }
                            else
                            {
                                detail.ApprovalStatus = record.ManualApprovedStatus;
                                detail.InternalApprovedStatus = record.ManualInternalApprovedStatus;
                            }
                        }
                        break;
                }
                detail.OperationTime = operationTimeDt.Equals(DateTime.MinValue) ? string.Empty : operationTimeDt.Ticks.ToString();
                if (string.IsNullOrEmpty(operationByStr))
                {
                    detail.OperationBy = operationByStr;
                }
                else
                {
                    string[] sArray = operationByStr.Split('#');
                    if (sArray.Length > 1)
                    {
                        detail.OperationBy = sArray[1];
                    }
                    else
                    {
                        detail.OperationBy = sArray[0];
                    }
                }
                detail.URL = WebUtil.MakeFullUrl(Site.Url, item.DirPath);
                detail.Operation = (int)operationType;
                detail.TermName = item.TermName;
                detail.Status = JobDetailsStatus.Successful;
                ReportManager.SendJobReport(ConvertToReport(detail));
                ReportManager.SendJobDetail(ConvertToDetail(detail));
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = commomErrorMessage;
                ReportManager.SendJobDetail(ConvertToDetail(detail));
                mLog.Error("Report of created or destroyed file during timeframe failed.Error:[{0}]", e.ToString());
            }
        }

        private ObjectLevel GetObjectLevelByContentType(string contentTypeName)
        {
            ObjectLevel level = ObjectLevel.None;
            switch (contentTypeName)
            {
                case CONTENT_TYPE_DOCUMENT_NAME:
                case CONTENT_TYPE_OfficeDataConnectionFile_NAME:
                    level = ObjectLevel.Document;
                    break;
                case CONTENT_TYPE_Folder_NAME:
                    level = ObjectLevel.Folder;
                    break;
            }
            return level;
        }

        private CreateAndDestroyedFileReport ConvertToReport(JMCreateAndDestroyedFileReportJobDetail detail)
        {
            CreateAndDestroyedFileReport report = new CreateAndDestroyedFileReport();
            if (string.Equals(detail.ObjectLevel, "Document", StringComparison.OrdinalIgnoreCase))
            {
                report.LevelStr = (int)RMReportObjectLevel.Document;
            }
            else if (string.Equals(detail.ObjectLevel, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                report.LevelStr = (int)RMReportObjectLevel.Folder;
            }
            //report.LevelStr = detail.ObjectLevel;
            report.Title = detail.Title;
            report.OperationTime = detail.OperationTime;
            report.OperationBy = detail.OperationBy;
            report.TermName = detail.TermName;
            report.DisposalClass = detail.DisposalClass;
            report.Url = detail.URL;
            report.LifecycleStatus = detail.LifecycleStatus;
            report.HomeLocation = detail.HomeLocation;
            report.Availablity = detail.Availablity;
            report.CurrentHeldBy = detail.CurrentHeldBy;
            report.Operation = detail.Operation;
            report.ApprovedBy = detail.ApprovedBy;
            report.ApprovalStatus = detail.ApprovalStatus;
            report.InternalApprovedStatus = detail.InternalApprovedStatus;
            report.ApprovedByUPN = detail.ApprovedByUPN;
            report.CreatedTime = detail.CreatedTime;
            report.LastModifiedTime = detail.LastModifiedTime;
            report.FileType = detail.FileType;
            report.RecordsId = detail.RecordsId;
            report.RuleName = detail.RuleName;
            return report;
        }

        private JMCreateAndDestroyedFileReportJobDetail ConvertToDetail(JMCreateAndDestroyedFileReportJobDetail detail)
        {
            var convertDetail = detail;
            if (string.Equals(detail.ObjectLevel, "Document", StringComparison.OrdinalIgnoreCase))
            {
                convertDetail.ObjectLevel = "RM_Template_Column_Value_Format_Document";
            }
            else if (string.Equals(detail.ObjectLevel, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                convertDetail.ObjectLevel = "RM_Common_ObjectLevel_Folder";
            }
            return convertDetail;
        }

    }
}
