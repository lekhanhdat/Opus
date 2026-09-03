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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.Wrapper.Common;
using RAArchiverCommon.DestructionCache;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Util;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.CAMLHelper.CAML;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.RACommonUtility.Extension;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RADataBroker;
using Newtonsoft.Json;
using System.Xml;
using System.Data.SqlClient;

namespace RATeams.Discover.Base
{
    public class RMTeamsCreationAndDestroyedFileReportProcessor : RMTeamsReportProcessor
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
        private const string SP_FIELD_MODIFIED_BY_NAME = "Modified By";
        private const string SP_DESTROYED_TIME_NAME = "Destroyed Time";
        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";
        private const string ARCHIVER_XML_NODE_NAME = "Name";
        private const string ARCHIVER_XML_NODE_VALUE = "Value";
        private const string ARCHIVER_XML_NODE_CONTENT_TYPE = "content type";
        private const string ARCHIVER_XML_NODE_MODIFIED_BY = "modified by";
        private const string ARCHIVER_XML_NODE_LIFECYCLE_STATUS = "lifecycle status";
        private const string ARCHIVER_XML_NODE_BOX = "box";
        private const string ARCHIVER_XML_NODE_AVAILABILITY = "availability";
        private const string ARCHIVER_XML_NODE_CURRENTLY_HELD_BY = "currently held by";
        private const string ARCHIVER_XML_NODE_EXTEND_VALUE = "ExtendValue";

        private RMCreationJobMessage msg = null;
        private IAveSite mBufferSite = null;
        private IAveWeb mBufferWeb = null;
        private List<string> reportedListIds = null;

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IExplorerDao ExplorerDao = new ExplorerDao();

        private AzureTableConnectContract AzureTableConnectInfo = null;
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
        protected int itemsPerTask = 10000;
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IArchiverTableDao mArchiverTableDao = null;

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

        private AzureTableConnectContract mAzureTableConnectInfo = null;
        private string mTenantGroupId = TenantLocalValue.LogonGroupId;
        protected Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        protected string commomErrorMessage = string.Empty;
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

        protected DateTime startUtcTime;
        protected DateTime endUtcTime;
        protected DestrunctionReportHelper destrunctionReportHelper = null;
        protected string homeLocationName;
        private IExplorerDao explorerDao = new ExplorerDao();
        public RMTeamsCreationAndDestroyedFileReportProcessor(RMCreationJobMessage msg)
           : base(msg.JobID, JobType.TeamsCreateAndDestroyedFileReport, false)
        {
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_DISCOVER_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            Logger.Info($"SPOItemsPerTask : {itemsPerTask}");
            this.msg = msg;
            this.msg.EndTime = this.msg.EndTime.AddDays(1);
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
            destrunctionReportHelper = new DestrunctionReportHelper(startUtcTime, endUtcTime);
            mArchiverTableDao = (IArchiverTableDao)PlatformWindsorManager.GetService(typeof(IArchiverTableDao));
            reportedListIds = new List<string>();
            commomErrorMessage = "RM_TS_SS_Summary";
        }

        public override async Task RunAsync()
        {
            try
            {
                await InitRulesInfoAsync();
                foreach (var siteCollectionNodeItem in SiteCollectionNodeItems)
                {
                    if (BCSColumnNameDics.TryGetValue(siteCollectionNodeItem.Id, out BCSColumnName))
                    {
                        await ProcessSiteAsync(siteCollectionNodeItem);
                    }
                    else
                    {
                        Logger.Warn("Get BCS Column Name error.");
                    }
                }
            }
            catch (JobStopException e)
            {
                JobHasStopped = true;
            }
            catch (Exception e)
            {
                JobHasException = true;
                Logger.Error($"occured error,msg:{e.Message},stackTrace:{e.StackTrace}");
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (JobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (JobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                if (finalStatus == JobStatus.Finished)
                {
                    commomErrorMessage = string.Empty;
                }
                ReportManager.SetJobFinished(finalStatus, commomErrorMessage);
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        protected override async System.Threading.Tasks.Task ProcessListAsync(NodeItem listNode)
        {
            using (PerformanceScope scope = new PerformanceScope($"CreationAndDestroyedFileReportProcessor.ProcessList.[{listNode.NameOrTitle}]"))
            {
                Logger.Debug("Process List {0}", listNode.FullPath);
                try
                {
                    CheckNodeLevel(listNode, NodeLevel.List);
                    Logger.Info("Start web process. fullPath: [{0}], isIncludeNew : [{1}].", listNode.FullPath, listNode.IncludeNew);
                    await RealReportAsync(listNode);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred while prosess sitecollection, fullPath is :{0}, error message: {1}.", listNode.FullPath, e.ToString());
                }
            }
        }

        private async System.Threading.Tasks.Task RealReportAsync(NodeItem listNode)
        {
            NodeItem siteNode = GetParentNode(listNode, NodeLevel.SiteCollection);
            NodeItem webNode = GetParentNode(listNode, NodeLevel.Site);

            if (CurrentSite == null || !Guid.Equals(siteNode.Id, CurrentSite.ID))
            {
                var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrl(siteNode.FullPath);
                var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite);
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

            await ReportListAsync(list);
        }

        private async Task ReportListAsync(IAveList list)
        {
            if (reportedListIds.Contains(list.ParentWeb.Site.ID.ToString() + list.ID.ToString()) || list.Hidden || (list.BaseType != AveBaseType.DocumentLibrary && list.BaseTemplate != AveListTemplateType.PictureLibrary)
               || SpCommonUtility.CheckIsDesignList(list.RootFolder.Name + (int)list.BaseTemplate))
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
                JobHasException = true;
                JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
                detail.Title = string.Format("Web Url:[{0}],List title:[{1}]", list.ParentWeb.Url, list.Title);
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                ReportManager.SendJobDetail(ConvertToDetail(detail));
                Logger.Error("Report list failed,web url:[{0}]list title:[{1}],error:{2}", list.ParentWeb.Url, list.Title, e);
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

        protected override async System.Threading.Tasks.Task ProcessSiteAsync(NodeItem siteNode)
        {
            Guid siteId = Guid.Empty;
            bool needQueryLiteDB = destrunctionReportHelper.IsNeedQueryLiteDB();
            if (msg.SelectDestroyed && TenantService.IsNewOpusTenant() && needQueryLiteDB)
            {
                siteId = await GetSiteIdAsync(siteNode);
                LoadDestructionCache(siteId);
            }
            if (siteNode.NodeLevel == NodeLevel.SiteCollection)
            {
                await base.ProcessSiteAsync(siteNode);
            }
            if (msg.SelectDestroyed && TenantService.IsNewOpusTenant() && needQueryLiteDB)
            {
                ClearDestructionCache(siteId);
            }
        }

        private bool IsListReportedOrHidden(IAveList list)
        {
            string listKey = list.ParentWeb.Site.ID.ToString() + list.ID.ToString();
            if (reportedListIds.Contains(listKey) || list.Hidden ||
                (list.BaseType != AveBaseType.DocumentLibrary && list.BaseTemplate != AveListTemplateType.PictureLibrary) ||
                SpCommonUtility.CheckIsDesignList(list.RootFolder.Name + (int)list.BaseTemplate))
            {
                Logger.Info($"Skipping list: {list.Title}");
                return true;
            }
            return false;
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

        private async Task<Guid> GetSiteIdAsync(NodeItem siteNode)
        {
            if (CurrentSite == null || !Guid.Equals(siteNode.Id, CurrentSite.ID))
            {
                var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrlV1(siteNode.FullPath);
                var bposInfo = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSite);
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
                        DestructionCacheWrapper.CreateInstance(GetLiteDBPath(siteId.ToString())).Insert(records);
                    }
                    while (readCount == 100);

                }
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch (Exception e)
                {
                    Logger.Error($"error occured when LoadDestructionCache,error:{e}");
                }
            }
            else
            {
                Logger.Warn("Destruction cache file not exist.");
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
            DestructionCacheWrapper.CreateInstance(GetLiteDBPath(siteId.ToString())).Dispose();
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

        protected override System.Threading.Tasks.Task ProcessWebAsync(NodeItem webNode, bool IsProcessLists = true)
        {
            return base.ProcessWebAsync(webNode, IsProcessLists);
        }

        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds, IAveWeb web, IAveList list)
        {
            return null;
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items, string teamsName)
        {
            return -1;
        }

        #region BuildReport
        #region BuildCreatedReport
        public void BuildCreatedReport(IAveList list)
        {
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            int startIndex = 0;
            int maxIndex = SpCommonUtility.GetLastItemFolderId(list, list.RootFolder);
            bool needQueryNext = true;

            Logger.Info($"[BuildCreatedReport] Max index: {maxIndex}");
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
                        Logger.Info($"creation process item count:[{items.Count}]");
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
                            Logger.Info($"PagingInfo:{startIndex}");
                        }
                        else
                        {
                            needQueryNext = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    JobHasException = true;
                    Logger.Error($"report creation ERROR:{ex.ToString()}");
                    JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
                    detail.Title = string.Format("Web Url:[{0}],List title:[{1}]", list.ParentWeb.Url, list.Title);
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), ex.Message);
                    ReportManager.SendJobDetail(ConvertToDetail(detail));
                }

            }
            while (needQueryNext);
        }

        private void ProcessOneItem(IAveListItem item, string listTitle, int manualApproveStatus = 0)
        {
            using (PerformanceScope scope = new PerformanceScope($"CreationAndDestroyedFileReportProcessor.dealwithitem.[{listTitle}.{item.DisplayName}]"))
            {
                ReportManager.Increase();
                SendJobDetail(item, OperationType.Created, false, null);
            }
        }

        private void SendJobDetail(IAveListItem item, OperationType operationType, bool isPhysicalLibrary, Record record)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                ObjectLevel oLevel = GetObjectLevelByContentType(item.ContentType.Name, isPhysicalLibrary);
                if (!isPhysicalLibrary && oLevel == ObjectLevel.None)
                {
                    if(item.ContentType.Parent.Group == "Document Set Content Types")
                    {
                        return;
                    }
                    string contentTypeId = item.ContentType?.ID?.ToString() ?? string.Empty;
                    if (contentTypeId.StartsWith(AveBuiltInContentTypeId.Folder) || contentTypeId.StartsWith(AveBuiltInContentTypeId.DocumentSet))
                    {
                        oLevel = ObjectLevel.Folder;
                    }
                    else
                    {
                        oLevel = ObjectLevel.Document;
                    }
                }

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
                        if (record?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
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

                if (TermDao != null)
                {
                    string bcStr = GetFieldValue(item, BCSColumnName);
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
                            Logger.Warn("Get term from term store failed. item url: {0}, message:{1}", detail.URL, ex.Message);
                        }
                    }
                    else
                    {
                        Logger.Warn("No term found. Item url: {0}, column name: {1}", detail.URL);
                    }
                }

                detail.Status = JobDetailsStatus.Successful;
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
                detail.Comment = "RM_TS_SS_Summary";
                ReportManager.SendJobDetail(ConvertToDetail(detail));
                Logger.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
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

        public JMCreateAndDestroyedFileReportJobDetail ConvertToDetail(JMCreateAndDestroyedFileReportJobDetail detail)
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

        protected DateTime GetDateTimeFieldValue(IAveListItem item, string fieldName)
        {
            try
            {
                DateTime dt = (DateTime)item[fieldName];
                return dt;
            }
            catch (Exception e)
            {
                Logger.Warn($"GetDateTimeFieldValue Cast Error: {e}");
                try
                {
                    return DateTime.Parse(item[fieldName].ToString());
                }
                catch (Exception ex)
                {
                    Logger.Warn($"GetDateTimeFieldValue Parse Error: {ex}");
                    return new DateTime();
                }
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
                Logger.Warn("Get field value failed.Field name:[{0}]", fieldName);
                return string.Empty;
            }
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

        private void RunMultiThreadsReport(IAveListItemCollection items, int itemsPerTask, string listTitle, CancellationTokenSource cts)
        {
            AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
            {
                ProcessOneItem(item, listTitle);
            });
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

        protected void AddRowLimitQueryCondition(CAMLManager cm, QueryGroup group, int startIndex, int endIndex, int QueryConditionMaxCount)
        {
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

        protected void AddTimeContidion(QueryGroup group, DateTime startTime, DateTime endTime)
        {
            group.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               BuiltInFieldName.CreatedTime,
               Types.FieldTypes.DateTime,
               Types.QueryTypes.FromTo,
               CreateISO8601DateTimeFromSystemDateTime(startTime),
               CreateISO8601DateTimeFromSystemDateTime(endTime),
                           true));
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
        #endregion

        #region BuildDestroyedReport
        public async System.Threading.Tasks.Task BuildDestroyedReportAsync(IAveList list, bool isPhysical, string BcsColumnName = "")
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
                        Logger.Warn($"An error while get data from archiver table, message: {ex}");
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
                Logger.Error("Failed to retrieve the archived data that meets the rule settings. table not exist, List Title:[{0}],error:{1}", list.Title, ex.ToString());
                commomErrorMessage = I18NEntity.GetString("RM_DAM_NoTable");
                JobHasException = true;
            }
            catch (SqlException se)
            {
                Logger.Warn("Get destroyed items failed,error:{0}", se.ToString());
            }
            List<Guid> destroyedNodeIds = new List<Guid>();
            Dictionary<Guid, int> cacheAllRecordsApprovedBy = [];
            Dictionary<Guid, int> cacheAllRecordsApprovedStatus = [];
            Dictionary<Guid, int> cacheAllRecordsInternalApprovedStatus = [];
            for (int i = 0; i < infos.Count; i += 1000)
            {
                var temps = ExplorerDao.GetRecordByIds(infos.Skip(i).Take(1000).Select(info => GetRecordId(Site.ID, info.NodeID)).ToList());
                foreach (var item in temps)
                {
                    if (item?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
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
                    if (item.ManualApprovedStatus != (int)SOApproveDBStatus.Rejected)
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
                    if (destroyedNodeIds.Contains(info.NodeID))
                    {
                        Logger.Debug("Dup node id {0}", info.NodeID);
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
                            detail.OperationBy = "RM_RC_TimeFrame_ArchiverByRASystem";

                            if (isPhysical && string.IsNullOrEmpty(asd.Metadata))
                            {
                                Logger.Warn("Fields info is null,Url:[{0}]", detail.URL);
                                continue;
                            }
                            else if (!string.IsNullOrEmpty(asd.Metadata))
                            {
                                #region Get metadata
                                ObjectLevel oLevel = ObjectLevel.None;
                                Logger.Info("Id:[{0}],FieldsInfo:[{1}]", asd.ScopeID, asd.Metadata);
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
                                            bool isBcsTermField =
                                                string.Equals(fieldName, isPhysical ? BcsColumnName : BCSColumnName, StringComparison.OrdinalIgnoreCase)
                                                || string.Equals(fieldName, RcordsBuiltInColumn.ITEM_BCS_NAME, StringComparison.OrdinalIgnoreCase)
                                                || (string.IsNullOrEmpty(fieldName)
                                                    && node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null
                                                    && !string.IsNullOrEmpty(node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value)
                                                    && node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value.Contains("|"));

                                            if (isBcsTermField)
                                            {
                                                if (TermDao != null)
                                                {
                                                    if (node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE] != null)
                                                    {
                                                        string bcStr = node.Attributes[ARCHIVER_XML_NODE_EXTEND_VALUE].Value;
                                                        if (!string.IsNullOrEmpty(bcStr))
                                                        {
                                                            string[] bcParts = bcStr.Split('|');
                                                            if (bcParts.Length >= 2 && Guid.TryParse(bcParts[1], out Guid bcTermId))
                                                            {
                                                                detail.TermName = TermDao.GetTermFullPathForDestroyReport(bcTermId);
                                                            }
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
                                                            string[] hlParts = homeLocationStr.Split('|');
                                                            if (hlParts.Length >= 2 && Guid.TryParse(hlParts[1], out Guid homeLocationTermId))
                                                            {
                                                                detail.HomeLocation = TermDao.GetTermFullPathForDestroyReport(homeLocationTermId);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }

                                if (isPhysical && oLevel != ObjectLevel.PhysicalFile)
                                {
                                    Logger.Warn("skip physical item:{0}", detail.URL);
                                    continue;
                                }
                                if (!isPhysical && oLevel == ObjectLevel.None)
                                {
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
                                Logger.Info($"File {info.NodeID} node level is {spNodeLevel}");
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
                            if (cacheAllRecordsApprovedStatus.TryGetValue(GetRecordId(Site.ID, info.NodeID), out int approvedStatus))
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

                        Logger.Error("Report of created or destroyed file during timeframe  failed.Error:[{0}]", e.ToString());
                    }
                }
            }
        }

        private RMRuleInfos GetRuleInfo(Guid id)
        {
            return idRuleInfoDic.ContainsKey(id) ? idRuleInfoDic[id] : null;
        }

        private async Task<List<ArchiverTableEntity>> GetEntitiesFromArhicverTableAsync(IAveList list, bool isPhysical, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            var mDocAveClient = new DAOAPIClientV1(true);
            AzureTableConnectInfo = await mDocAveClient.GetArchiverDataBaseConfigAsync();
            List<ArchiverTableEntity> infos = mArchiverTableDao.GetDestroyedItemsByListId(AzureTableConnectInfo, TenantLocalValue.LogonGroupId, list.ParentWeb.Site.ID.ToString(), list.ID, queryStartUtcTime, queryEndUtcTime, isPhysical);
            return infos;
        }

        public Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return new Guid(HashCodeHelper.ToMD5HashCode(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()));
        }

        private List<ArchiverTableEntity> GetEntitiesFromLiteDB(Guid siteId, Guid listId, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            List<ArchiverTableEntity> entities = new List<ArchiverTableEntity>();
            var LiteDBWrapper = DestructionCacheWrapper.CreateInstance(GetLiteDBPath(siteId.ToString()));
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
        #endregion

        #endregion
    }
}
