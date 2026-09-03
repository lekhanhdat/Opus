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
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.I18N.Core;
using System;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using System.Text;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using AvePoint.RA.RADataBroker;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Tenant;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Xml;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.Common.Util;
using System.Threading;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMRuleManageMent;
using System.Linq;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.SystemSetting;
using Microsoft.Exchange.WebServices.Data;
using RAArchiverCommon.DestructionCache;
using DocumentFormat.OpenXml.Drawing;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using Microsoft.SharePoint.Client.RecordsRepository;
using System.Threading.Tasks;
using Microsoft.Graph;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Contract;

namespace AvePoint.RA.SharePoint.Discover
{
    public class CreationAndDestroyedFileReportProcessor : RMReportProcessor
    {
        private const string DESTROYED = "Destroyed";
        private const string CONTENT_TYPE_DOCUMENT_NAME = "Document";
        private const string CONTENT_TYPE_PHYSICAL_RECORD_NAME = "Physical Record";
        private const string CONTENT_TYPE_PHYSICAL_FILE_NAME = "Physical File";
        private const string CONTENT_TYPE_OfficeDataConnectionFile_NAME = "Office Data Connection File";
        private const string CONTENT_TYPE_Folder_NAME = "Folder";
        private const string CONTENT_TYPE_DOCUMENT_SET_NAME = "Document Set";
        private const string SP_FIELD_NAME_NAME = "Name";
        private const string SP_FIELD_CREATED_NAME = "Created";
        private const string SP_FIELD_CREATED_BY_NAME = "Created By";
        private const string SP_FIELD_AUTHOR_NAME = "Author";

        //private const string SP_FIELD_MODIFIED_NAME = "Modified";
        private const string SP_FIELD_MODIFIED_BY_NAME = "Modified By";
        private const string SP_DESTROYED_TIME_NAME = "Destroyed Time";

        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";
        private const string ARCHIVER_XML_NODE_NAME = "Name";
        private const string ARCHIVER_XML_NODE_VALUE = "Value";
        private const string ARCHIVER_XML_NODE_CONTENT_TYPE = "content type";
        //private const string ARCHIVER_XML_NODE_MODIFIED = "modified";
        private const string ARCHIVER_XML_NODE_MODIFIED_BY = "modified by";
        private const string ARCHIVER_XML_NODE_LIFECYCLE_STATUS = "lifecycle status";
        private const string ARCHIVER_XML_NODE_BOX = "box";
        private const string ARCHIVER_XML_NODE_AVAILABILITY = "availability";
        private const string ARCHIVER_XML_NODE_CURRENTLY_HELD_BY = "currently held by";
        private const string ARCHIVER_XML_NODE_EXTEND_VALUE = "ExtendValue";

        private string commomErrorMessage = string.Empty;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private RMCreationJobMessage msg = null;
        protected SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        private IAveSite mBufferSite = null;
        private IAveWeb mBufferWeb = null;
        private List<string> reportedListIds = null;      
        private IAveSite CurrentSite
        {
            get
            {
                return mBufferSite;
            }
            set
            {
                if (mBufferSite != null)
                {
                    mBufferSite.Dispose();
                }
                mBufferSite = value;
            }
        }
        private IAveWeb CurrentWeb
        {
            get
            {
                return mBufferWeb;
            }
            set
            {
                if (mBufferWeb != null)
                {
                    mBufferWeb.Dispose();
                }
                mBufferWeb = value;
            }
        }
        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = new TermDao();
                }
                return mTermDao;
            }
        }
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

        public CreationAndDestroyedFileReportProcessor(RMCreationJobMessage msg)
            : base(msg.JobID, (int)JobType.CreateAndDestroyedFileReport, false)
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
            reportedListIds = new List<string>();
        }

       

        public override async System.Threading.Tasks.Task RunReportJobAsync()
        {
            try
            {
                await InitRulesInfoAsync();
                foreach (var SiteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    if (mBCSColumnNameDics.TryGetValue(SiteCollectionNodeItem.Id, out mBCSColumnName))
                    {
                        await ProcessSiteAsync(SiteCollectionNodeItem);
                    }
                    else
                    {
                        mLog.Warn("Get BCS Column Name error.");
                    }
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
            if (CurrentSite == null || !Guid.Equals(siteNode.Id, CurrentSite.ID))
            {
                var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrlV1(siteNode.FullPath);
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                CurrentSite = mfactory.CreateSite(siteNode.FullPath);
            }
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
                    mLog.Error($"error occured when LoadDestructionCache,error:{e}");
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
            var (startUtcTicks, endUtcTicks) = (queryStartUtcTime.Ticks, queryEndUtcTime.Ticks);

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
                    //过滤掉不在report time range内的数据
                    records = records.Where(r => r.ArchivedTime >= startUtcTicks && r.ArchivedTime <= endUtcTicks).ToList();
                    if (records.Count > 0)
                    {
                        mLog.Info($"The amount of data in the report time range: [{records.Count}], listId:[{listId}], startUtcTicks:[{startUtcTicks}], endUtcTicks:[{endUtcTicks}]");
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

		private async Task<List<ArchiverTableEntity>> GetEntitiesFromArhicverTableAsync(IAveList list, bool isPhysical, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            var mDocAveClient = new DAOAPIClientV1(true);
            mAzureTableConnectInfo = await mDocAveClient.GetArchiverDataBaseConfigAsync();
            List<ArchiverTableEntity> infos = mArchiverTableDao.GetDestroyedItemsByListId(mAzureTableConnectInfo, mTenantGroupId, list.ParentWeb.Site.ID.ToString(), list.ID, queryStartUtcTime, queryEndUtcTime, isPhysical);
            return infos;
        }

        protected override System.Threading.Tasks.Task ProcessWebAsync(NodeItem webNode, bool IsProcessLists = true)
        {
            return base.ProcessWebAsync(webNode, IsProcessLists);
        }

        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem listNode)
        {
            using (PerformanceScope scope = new PerformanceScope($"CreationAndDestroyedFileReportProcessor.ProcessList.[{listNode.NameOrTitle}]"))
            {
                mLog.Debug("Process List {0}", listNode.FullPath);
                try
                {
                    CheckNodeLevel(listNode, NodeLevel.List);
                    mLog.Info("Start web process. fullPath: [{0}], isIncludeNew : [{1}].", listNode.FullPath, listNode.IncludeNew);
                    await RealReportAsync(listNode);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", listNode.FullPath, e.ToString());
                }
            }
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items)
        {
            return -1;
        }

        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds)
        {
            return null;
        }

        protected override CAMLManager InitUnclassificationCamlQuery(IAveFieldCollection listFields, IAveWeb web, IAveList list, RMReportExtension reportExt)
        {
            return null;
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

        private async System.Threading.Tasks.Task RealReportAsync(NodeItem listNode)
        {
            NodeItem siteNode = GetParentNode(listNode, NodeLevel.SiteCollection);
            NodeItem webNode = GetParentNode(listNode, NodeLevel.Site);

            if (CurrentSite == null || !Guid.Equals(siteNode.Id, CurrentSite.ID))
            {
                var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrl(siteNode.FullPath);
                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                CurrentSite = mfactory.CreateSite(siteNode.FullPath);
            }

            if (webNode.Id == null || webNode.Id.Equals(Guid.Empty))
            {
                CurrentWeb = CurrentSite.RootWeb;
            }
            else
            {
                CurrentWeb = CurrentSite.OpenWeb(webNode.Id);
            }

            IAveList list = CurrentWeb.GetList(listNode.Id);
            //IAveList list = listNode.DiscoverObj as IAveList;

            await ReportListAsync(list);
        }

        private async System.Threading.Tasks.Task ReportListAsync(IAveList list)
        {
            if (reportedListIds.Contains(list.ParentWeb.Site.ID.ToString() + list.ID.ToString())
                    || list.Hidden
                    || (list.BaseType != AveBaseType.DocumentLibrary && list.BaseTemplate != AveListTemplateType.PictureLibrary)
                    || SPCommonUtility.CheckIsDesignList(list.RootFolder.Name + (int)list.BaseTemplate)
                    )
            {
                return;
            }
            try
            {
                if (msg.SelectCreated)
                {
                    BuildCreatedReport(list);
                }
                if (msg.SelectDestroyed)
                {
                    await BuildDestroyedReportAsync(list, false);
                }
            }
            catch (JobStopException ex)
            {
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
            finally
            {
                string value = list.ParentWeb.Site.ID.ToString() + list.ID.ToString();
                if (!reportedListIds.Contains(value))
                {
                    reportedListIds.Add(value);
                }
                ReportManager.Increase();
            }
        }

        private void RunMultiThreadsReport(IAveListItemCollection items, int itemsPerTask, string listTitle, CancellationTokenSource cts)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item => {
                ProcessOneItem(item, listTitle);
            });
        }
        private void BuildCreatedReport(IAveList list)
        {

            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            bool needQueryNext = false;
            int startIndex = 0;
            int maxIndex = SPCommonUtility.GetLastItemFolderId(list, list.RootFolder);
            mLog.Info($"Max index in library {list.RootFolder?.Url}:{maxIndex}");
            IAveListItemCollection items = null;
            do
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        AveCamlQuery query = GetCreationCamlQuery(startIndex, startIndex + rowLimit, rowLimit);
                        items = list.GetItemsForRecords(query);
                        ReportManager.IncreaseBase(items.Count);
                        mLog.Info($"creation process item count:[{items.Count}]");
                        if (items.Count > itemsPerTask)
                        {
                            var cts = new CancellationTokenSource();
                            RunMultiThreadsReport(items, itemsPerTask, list.Title, cts);
                            return;
                        }
                        AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(items);
                        foreach (IAveListItem item in items)
                        {
                            ProcessOneItem(item, list.Title);
                        }

                        if (startIndex + rowLimit < maxIndex)
                        {
                            needQueryNext = true;
                            startIndex += rowLimit;
                            mLog.Info($"PagingInfo:{startIndex}");
                        }
                        else
                        {
                            needQueryNext = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mJobHasException = true;
                    mLog.Error($"report creation ERROR:{ex.ToString()}");
                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
                    detail.Title = string.Format("Web Url:[{0}],List title:[{1}]", list.ParentWeb.Url, list.Title);
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message);
                    ReportManager.SendJobDetail(ConvertToDetail(detail));
                }
               
            }
            while (needQueryNext);
            
        }

        private AveCamlQuery GetCreationCamlQuery(int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            string queryStr = string.Empty;
            CAMLManager cm = new CAMLManager(Types.ScopeTypes.Recursive);
            var group = new QueryGroup();
            AddTimeContidion(group, startUtcTime, endUtcTime);
            AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
            cm.QueryGroup.AddGroup(group);
            string queryXml = cm.GetFullCAML(true);
            query.ViewXml = queryXml;
            query.DatesInUtc = true;
            return query;
        }

        protected void AddTimeContidion(QueryGroup group, DateTime startTime, DateTime endTime)
        {
            group.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               SPBuiltInFieldName.CreatedTime,
               Types.FieldTypes.DateTime,
               Types.QueryTypes.FromTo,
               CreateISO8601DateTimeFromSystemDateTime(startTime),
                CreateISO8601DateTimeFromSystemDateTime(endTime),
                           true));
        }
        protected void AddRowLimitQueryCondition(CAMLManager cm, QueryGroup group, int startIndex, int endIndex, int QueryConditionMaxCount)
        {
            //cm.ScopeType = Types.ScopeTypes.Default;
            cm.RowLimit = QueryConditionMaxCount;
            group.Conditions.Add(new QueryCondition(
                              Types.JoinTypes.And,
                              Types.FieldRefTypes.Name,
                               "ID",
                             Types.FieldTypes.Number,
                             Types.QueryTypes.Leq,
                              endIndex.ToString(), false));
            group.Conditions.Add(new QueryCondition(
                                 Types.JoinTypes.And,
                                 Types.FieldRefTypes.Name,
                                 "ID",
                                 Types.FieldTypes.Number,
                                  Types.QueryTypes.Gt,
                                 startIndex.ToString(), false));
        }

        private void ProcessOneItem(IAveListItem item, string listTitle, int manualApproveStatus = 0)
        {
            using (PerformanceScope scope = new PerformanceScope($"CreationAndDestroyedFileReportProcessor.dealwithitem.[{listTitle}.{item.DisplayName}]"))
            {
                ReportManager.Increase();
                SendJobDetail(item, OperationType.Created, false, null);
            }
        }
        private async System.Threading.Tasks.Task BuildDestroyedReportAsync(IAveList list, bool isPhysical, string BcsColumnName = "")
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
                            List<ArchiverTableEntity> destroyedItemsInArchiverTable = await GetEntitiesFromArhicverTableAsync(list, isPhysical, queryArchiverTableTimeRange.Item1, queryArchiverTableTimeRange.Item2);
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
                    infos = await GetEntitiesFromArhicverTableAsync(list, isPhysical, startUtcTime, endUtcTime);
                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLog.Error("Failed to retrieve the archived data that meets the rule settings. table not exist, List Title:[{0}],error:{1}", list.Title, ex.ToString());
                commomErrorMessage = I18NEntity.GetString("RM_DAM_NoTable");
                mJobHasException = true;
            }
            catch (SqlException se)
            {
                //REC-2281
                mLog.Warn("Get destroyed items failed,error:{0}", se.ToString());
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
                    if(item?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                    {
                        if (item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                        {
                            cacheAllRecordsApprovedStatus.TryAdd(item.Id, (int)SOApproveDBStatus.Cancelled);
                            cacheAllRecordsInternalApprovedStatus.TryAdd(item.Id, (int)SOApproveDBStatus.Cancelled);
                        }
                        else
                        {
                            cacheAllRecordsApprovedStatus.TryAdd(item.Id, item.ManualApprovedStatus);
                            cacheAllRecordsInternalApprovedStatus.TryAdd(item.Id, item.ManualInternalApprovedStatus);
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
                mLog.Info($"Report total count: [{infos.Count}]");
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
                            NodeLevel spNodeLevel = (NodeLevel)asd.SPNodeLevel;
                            detail.CreatedTime = asd.CreatedTime;
                            detail.LastModifiedTime = asd.CDLastModifiedTime;
                            detail.FileType = asd.FileType;
                            detail.URL = WebUtil.MakeFullUrl(list.ParentWeb.Url, asd.Path.Replace('\\', '/'));
                            detail.Title = asd.LeafName;
                            detail.Operation = (int)OperationType.Destroyed;
                            detail.OperationTime = asd.ArchivedTime.Ticks.ToString();
                            //detail.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                            detail.OperationBy = "RM_RC_TimeFrame_ArchiverByRASystem";

                            if (isPhysical && string.IsNullOrEmpty(asd.Metadata))
                            {
                                mLog.Warn("Fields info is null,Url:[{0}]", detail.URL);
                                continue;
                            }
                            else if (!string.IsNullOrEmpty(asd.Metadata))
                            {
                                #region Get metadata
                                ObjectLevel oLevel = ObjectLevel.None;
                                mLog.Info("Id:[{0}],FieldsInfo:[{1}]", asd.ScopeID, asd.Metadata);
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
                                            oLevel = GetObjectLevelByContentType(fieldValue, isPhysical);
                                            if (isPhysical && oLevel != ObjectLevel.PhysicalFile)
                                            {
                                                continue;
                                            }
                                            break;
                                        case ARCHIVER_XML_NODE_MODIFIED_BY:
                                            //既然OperationBy信息是写死的, 那就不用判断Metadata了.
                                            //detail.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                                            break;
                                        case ARCHIVER_XML_NODE_LIFECYCLE_STATUS:
                                            detail.LifecycleStatus = fieldValue;
                                            break;
                                        case ARCHIVER_XML_NODE_BOX:
                                            detail.Box = fieldValue;
                                            break;
                                        case ARCHIVER_XML_NODE_AVAILABILITY:
                                            detail.Availablity = fieldValue;
                                            break;
                                        case ARCHIVER_XML_NODE_CURRENTLY_HELD_BY:
                                            detail.CurrentHeldBy = fieldValue;
                                            break;
                                        default:
                                            if (string.Equals(fieldName, isPhysical ? BcsColumnName : mBCSColumnName, StringComparison.OrdinalIgnoreCase) || string.Equals(fieldName, RcordsBuiltInColumn.ITEM_BCS_NAME, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (TermDao != null)
                                                {
                                                    if (node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null)
                                                    {
                                                        string bcStr = node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value;
                                                        if (!string.IsNullOrEmpty(bcStr))
                                                        {
                                                            Guid bcTermId = new Guid(bcStr.Split('|')[1]);
                                                            detail.TermName = TermDao.GetTermFullPathForDestroyReport(bcTermId);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (string.Equals(fieldName, homeLocationName, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (TermDao != null)
                                                {
                                                    if (node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null)
                                                    {
                                                        string homeLocationStr = node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value;
                                                        if (!string.IsNullOrEmpty(homeLocationStr))
                                                        {
                                                            Guid homeLocationTermId = new Guid(homeLocationStr.Split('|')[1]);
                                                            detail.HomeLocation = TermDao.GetTermFullPathForDestroyReport(homeLocationTermId);
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }

                                if (isPhysical && oLevel != ObjectLevel.PhysicalFile)
                                {
                                    mLog.Warn("skip physical item:{0}", detail.URL);
                                    continue;
                                }
                                if(!isPhysical && oLevel == ObjectLevel.None)
                                {
                                    //自定义的Document ContentType， 无法用Name识别出是Document
                                    oLevel = ObjectLevel.Document;

                                    if (spNodeLevel == NodeLevel.Folder)
                                    {
                                        oLevel = ObjectLevel.Folder;
                                    }
                                }
                                detail.ObjectLevel = oLevel.ToString();
                                #endregion
                            }
                            else
                            {
                                mLog.Info($"File {info.NodeID} node level is {spNodeLevel}");
                                if (spNodeLevel == NodeLevel.Folder)
                                {
                                    detail.ObjectLevel = ObjectLevel.Folder.ToString();
                                }
                                else
                                {
                                    detail.ObjectLevel = ObjectLevel.Document.ToString();
                                }
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
                            if(cacheAllRecordsApprovedStatus.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int approvedStatus))
                            {
                                detail.ApprovalStatus = approvedStatus;
                            }
                            if (cacheAllRecordsInternalApprovedStatus.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int internalApprovedStatus))
                            {
                                detail.InternalApprovedStatus = internalApprovedStatus;
                            }
                            detail.Status = JobDetailsStatus.Successful;
                            //result.HasSuccessful = true;
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

                        //result.HasFailed = true;
                        mLog.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
                    }
                }
            }
        }

        public static Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return new Guid(HashCodeHelper.ToMD5HashCode(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()));
        }

        private string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dtValue.Year.ToString("0000"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Month.ToString("00"));
            stringBuilder.Append("-");
            stringBuilder.Append(dtValue.Day.ToString("00"));
            stringBuilder.Append("T");
            stringBuilder.Append(dtValue.Hour.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Minute.ToString("00"));
            stringBuilder.Append(":");
            stringBuilder.Append(dtValue.Second.ToString("00"));
            stringBuilder.Append("Z");
            return stringBuilder.ToString();
        }

        private void SendJobDetail(IAveListItem item, OperationType operationType, bool isPhysicalLibrary, Record record, string BcsColumnName = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                ObjectLevel oLevel =  GetObjectLevelByContentType(item.ContentType.Name, isPhysicalLibrary);
                if(!isPhysicalLibrary && oLevel == ObjectLevel.None)
                {
                    //自定义的Document Content Type
                    if(item.ContentType.Parent.Group == "Document Set Content Types")
                    {
                        return;
                    }
                    oLevel = ObjectLevel.Document; 
                }

                //只过滤Physical Library的Item Level，  普通Library已经在List级别和CAML Query限制只取Document
                if (isPhysicalLibrary && (oLevel == ObjectLevel.PhysicalRecord || oLevel == ObjectLevel.None))
                {
                    mLog.Info("Physical library, and item level is {0}, skip", oLevel);
                    detail = null;
                    return;
                }
                var itemName = item.GetObjectName();

                detail.CreatedTime = item.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? item.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0;
                detail.LastModifiedTime = item.FieldValues.ContainsKey(SPColumnConstants.Modified) ? item.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0;
                detail.ObjectLevel = oLevel.ToString();
                detail.Title = GetFieldValue(item, SP_FIELD_NAME_NAME);
                if (string.IsNullOrEmpty(detail.Title)) detail.Title = item.Name;
                detail.FileType = GetItemExtension(detail.Title, item);
                DateTime operationTimeDt = DateTime.MinValue;
                string operationByStr = string.Empty;
                switch (operationType)
                {
                    case OperationType.Created:
                        operationTimeDt = GetDateTimeFieldValue(item, SP_FIELD_CREATED_NAME);
                        operationByStr = GetFieldValue(item, SP_FIELD_CREATED_BY_NAME);
                        if (string.IsNullOrEmpty(operationByStr)) operationByStr = GetFieldValue(item, SP_FIELD_AUTHOR_NAME);
                        break;
                    case OperationType.Destroyed:
                        if(record?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                        {
                            if (record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
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
                        operationTimeDt = GetDateTimeFieldValue(item, SP_DESTROYED_TIME_NAME);
                        operationByStr = GetFieldValue(item, SP_FIELD_MODIFIED_BY_NAME);
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

                detail.URL = item.ParentList.ParentWeb.Url.TrimEnd('/') + "/" + item.Url.TrimStart('/');
                detail.Operation = (int)operationType;

                if (isPhysicalLibrary)
                {
                    detail.LifecycleStatus = GetFieldValue(item, lifecycleStatusName);
                    detail.Box = GetFieldValue(item, boxName);
                    detail.Availablity = GetFieldValue(item, availabilityName);
                    string currentHeldByStr = GetFieldValue(item, currentlyHeldByName);
                    detail.CurrentHeldBy = string.IsNullOrEmpty(currentHeldByStr) ? currentHeldByStr : currentHeldByStr.Split('#')[1];
                    if (TermDao != null)
                    {
                        string homeLocationStr = GetFieldValue(item, homeLocationName);
                        if (!string.IsNullOrEmpty(homeLocationStr))
                        {
                            Guid homeLocationTermId = new Guid(homeLocationStr.Split('|')[1]);
                            detail.HomeLocation = TermDao.GetTermFullPathForDestroyReport(homeLocationTermId);
                        }
                    }
                }

                if (TermDao != null)
                {
                    var columnName = string.Empty;
                    if (isPhysicalLibrary)
                    {
                        columnName = BcsColumnName;
                    }
                    else
                    {
                        columnName = mBCSColumnName;
                    }
                    string bcStr = GetFieldValue(item, columnName);
                    if (!string.IsNullOrEmpty(bcStr))
                    {
                        Guid bcTermId = new Guid(bcStr.Split('|')[1]);
                        try
                        {
                            detail.TermName = TermDao.GetTermFullPathForDestroyReport(bcTermId);
                        }
                        catch (Exception ex)
                        {
                            detail.TermName = "";
                            mLog.Warn("Get term from term store failed. item url: {0}, message:{1}", detail.URL, ex.Message);
                        }
                    }
                    else
                    {
                        mLog.Warn("No term found. Item url: {0}, column name: {1}", detail.URL, columnName);
                        //mLog.Warn("No term found, skip item. Item url: {0}, column name: {1}", detail.URL, columnName);
                        //return;
                    }
                }

                detail.Status = JobDetailsStatus.Successful;
                //result.HasSuccessful = true;
                if (!(operationType == OperationType.Created && string.Equals(DESTROYED, detail.LifecycleStatus, StringComparison.OrdinalIgnoreCase))
                    && !string.Equals(detail.ObjectLevel, ObjectLevel.PhysicalRecord.ToString(), StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(detail.ObjectLevel))
                {
                    ReportManager.SendJobReport(ConvertToReport(detail));
                    ReportManager.SendJobDetail(ConvertToDetail(detail));
                }
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = commomErrorMessage;
                ReportManager.SendJobDetail(ConvertToDetail(detail));

                //result.HasFailed = true;
                mLog.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
            }
        }

        private string GetItemExtension(string objectName, IAveListItem aveItem)
        {
            var result = string.Empty;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                var ext = System.IO.Path.GetExtension(objectName);
                result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
            }
            else
            {
                result = "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return result;
        }

        private ObjectLevel GetObjectLevelByContentType(string contentTypeName, bool isPhysicalLibrary)
        {
            ObjectLevel level = ObjectLevel.None;
            switch (contentTypeName)
            {
                case CONTENT_TYPE_DOCUMENT_NAME:
                case CONTENT_TYPE_OfficeDataConnectionFile_NAME:
                    level = ObjectLevel.Document;
                    break;
                case CONTENT_TYPE_PHYSICAL_RECORD_NAME:
                    level = ObjectLevel.PhysicalRecord;
                    break;
                case CONTENT_TYPE_PHYSICAL_FILE_NAME:
                    level = ObjectLevel.PhysicalFile;
                    break;
                case CONTENT_TYPE_Folder_NAME:
                case CONTENT_TYPE_DOCUMENT_SET_NAME:
                    level = ObjectLevel.Folder;
                    break;
            }
            return level;
        }

        private string GetFieldValue(IAveListItem item, string fieldName)
        {
            try
            {
                if (item[fieldName] == null)
                {
                    return string.Empty;
                }
                else
                {
                    return item[fieldName].ToString();
                }

            }
            catch (Exception)
            {
                mLog.Warn("Get field value failed.Field name:[{0}]", fieldName);
                return string.Empty;
            }
        }

        private CreateAndDestroyedFileReport ConvertToReport(JMCreateAndDestroyedFileReportJobDetail detail)
        {
            CreateAndDestroyedFileReport report = new CreateAndDestroyedFileReport();
            report.FileType = detail.FileType;
            //PhysicalFile;Document
            if (string.Equals(detail.ObjectLevel, "Document", StringComparison.OrdinalIgnoreCase))
            {
                report.LevelStr = (int)RMReportObjectLevel.Document;
            }
            else if (string.Equals(detail.ObjectLevel, "PhysicalFile", StringComparison.OrdinalIgnoreCase))
            {
                report.LevelStr = (int)RMReportObjectLevel.PhysicalFile;
                report.FileType = "RM_JS_Rule_ObjectLevel_PhysicalFile";
            }
            else if (string.Equals(detail.ObjectLevel, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                report.LevelStr = (int)RMReportObjectLevel.Folder;
                report.FileType = "RM_Common_ObjectLevel_Folder";
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
            report.Box = detail.Box;
            report.Availablity = detail.Availablity;
            report.ApprovalStatus = detail.ApprovalStatus;
            report.InternalApprovedStatus = detail.InternalApprovedStatus;
            report.CurrentHeldBy = detail.CurrentHeldBy;
            report.Operation = detail.Operation;
            report.ApprovedBy = detail.ApprovedBy;
            report.ApprovedByUPN = detail.ApprovedByUPN;
            report.CreatedTime = detail.CreatedTime;
            report.LastModifiedTime = detail.LastModifiedTime;
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
            else if (string.Equals(detail.ObjectLevel, "PhysicalFile", StringComparison.OrdinalIgnoreCase))
            {
                convertDetail.ObjectLevel = "RM_JS_Rule_ObjectLevel_PhysicalFile";
            }
            else if (string.Equals(detail.ObjectLevel, "Folder", StringComparison.OrdinalIgnoreCase))
            {
                convertDetail.ObjectLevel = "RM_Common_ObjectLevel_Folder";
            }
            return convertDetail;
        }

        private NodeItem GetParentNode(NodeItem node, NodeLevel level)
        {
            if (node.NodeLevel == level)
            {
                return node;
            }
            else
            {
                return GetParentNode(node.Parent, level);
            }
        }
    }
    public enum ObjectLevel
    {
        None,
        Document,
        PhysicalRecord,
        PhysicalFile,
        DocumentSet,
        Folder
    }

    public enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }
}

