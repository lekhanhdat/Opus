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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AvePoint.RA.SharePoint.Common.CAMLHelper.CAML.Types;

namespace AvePoint.RA.SharePoint.DeclaredRecordMigration
{
    public class DeclaredRecordMigrationProcessor
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(DeclaredRecordMigrationProcessor));
        private List<string> DesignLists = new List<string>();
        private Guid mWebApplicationId = Guid.Empty;
        private Guid siteId;
        private Guid groupId;
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
        private bool mJobHasException = false;
        private bool mJobHasStopped = false;
        private bool mJobHasSuccess = false;
        private string mSummaryComment = string.Empty;
        private AveObjectModelFactory mFactory = null;

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();


        //private ISettingProfilesDao SettingProfilesDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private string _recordsLabel;
        private RMSPTreeNode _processingNode;
        private IAveSite Site;

        private Dictionary<string, AveComplianceTagInfo> SharePointRetentionLabel = null;
        private readonly object mSPLabelLock = new object();
        private bool IsProcessListInParallel = true;
        private IAveORecords Record;
        private bool IsEnableJPMCCustomization = false;
        private Dictionary<Guid, string> JPMCRecordStatusFieldNames = new Dictionary<Guid, string>();
        private string CurrentRecordStatusFieldName;

        public DeclaredRecordMigrationProcessor(string jobId, JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress();
            DesignLists = WebUtil.GetDesignLists(JobContext.IsCSDTenant);
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            InitJPMCConfiguration();
            if (string.IsNullOrWhiteSpace(subJobWithContext?.JobContext?.Settings))
            {
                mLog.Warn("job context not found");
                ReportManager.SetJobFinished(JobStatus.Finished);
                return;
            }

            var jobContextDto = SerializerHelper.DeserializeByDataContractSerializer<DeclaredRecordsMigrationDto>(subJobWithContext.JobContext.Settings);
            if (jobContextDto == null)
            {
                mLog.Warn("No site collections found in job context.");
                ReportManager.SetJobFinished(JobStatus.Finished);
                return;
            }


            _processingNode = jobContextDto.NodeSetting;
            _recordsLabel = jobContextDto.RecordsLabel;
            ReportManager.IncreaseBase(1);
            mLog.Info($"Start job for site collection: {_processingNode.FullPath}, RecordsLabel: {_recordsLabel}");
        }

        public async Task RunJobAsync()
        {
            try
            {
                await ProcessAsync(_processingNode);
            }
            catch (JobStopException)
            {
                mJobHasStopped = true;
                mSummaryComment = string.Empty;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while running Declared Record Migration job, error message: {0}.", e.ToString());
                mJobHasException = true;
                mSummaryComment = e.Message; // I18n
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasStopped)
                    finalStatus = JobStatus.Stopped;
                else if (mJobHasException)
                {
                    if (mJobHasSuccess)
                        finalStatus = JobStatus.FinishWithException;
                    else
                        finalStatus = JobStatus.Failed;
                }

                ReportManager.SetJobFinished(finalStatus, mSummaryComment);
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        public async System.Threading.Tasks.Task ProcessAsync(RMSPTreeNode node)
        {
            groupId = node.SiteGroupId;
            siteId = node.SiteId;
            try
            {
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    await ProcessSiteAsync(node);
                }
            }
            catch (JobStopException)
            {
                mJobHasStopped = true;
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                mLog.Error("An error occurred while farm process. fullPath: [{0}], error message : {1}.", node.FullPath, ex.ToString());
                throw;
            }
        }

        private void InitJPMCConfiguration()
        {
            try
            {
                var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization")?.Value;
                if (!string.IsNullOrEmpty(jsonConfig))
                {
                    var configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig);
                    foreach (var cfg in configs)
                    {
                        var remoteSite = RABrowserClient.GetSiteNode(cfg.ConfigSiteUrl);
                        JPMCRecordStatusFieldNames[new Guid(remoteSite.TenantId)] = cfg.CustomColumns.RecordStatus;
                        mLog.Info($"Init JPMC customization for tenant {remoteSite.TenantId}, record status field name: {cfg.CustomColumns.RecordStatus}.");
                    }

                    IsEnableJPMCCustomization = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while initializing JPMC config {ex}");
            }
        }

        protected async Task ProcessSiteAsync(RMSPTreeNode site)
        {
            using (PerformanceScope scope = new PerformanceScope("DeclaredRecordMigrationProcessor.ProcessSite", $"DeclaredRecordMigrationProcessor.ProcessSite.[{site.Name}]", addToStatistics: true))
            {
                //IAveWeb discoverWeb = null;
                try
                {
                    using CheckJobStopScope jScope = new CheckJobStopScope();
                    var remoteSite = RABrowserClient.GetSiteNode(site.FullPath);
                    var tenantId = new Guid(remoteSite.TenantId);
                    CurrentRecordStatusFieldName = IsEnableJPMCCustomization && JPMCRecordStatusFieldNames.TryGetValue(tenantId, out var recordStatusFieldName) ? recordStatusFieldName : string.Empty;
                    
                    var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSite);
                    mFactory = MultiAppUtil.CreateAveObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                    try
                    {
                        Site = mFactory.CreateSite(site.FullPath);
                        Record = mFactory.CreateRecords();
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Can not connect to the site collection, fullPath is :{0}, error message: {1}.", site.FullPath, e.ToString());
                        AddFailedJobDetailReport(site.FullPath, "Site Collection", "RM_TS_NoRegister");
                        return;
                    }

                    var webCount = Site.AllWebs.Count;
                    ReportManager.IncreaseBase(webCount);

                    if (IsRecordTypeComplianceTag(Site, _recordsLabel, out var errorMessage))
                    {
                        await ProcessWebAsync(Site.RootWeb);
                    }
                    else
                    {
                        AddFailedJobDetailReport(site.FullPath, "Site Collection", string.IsNullOrWhiteSpace(errorMessage) ? $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{_recordsLabel}" : errorMessage);
                    }

                    ReportManager.Increase();
                }
                catch (JobStopException)
                {
                    mJobHasStopped = true;
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing site: {0}, error message: {1}.", site.FullPath, e);
                    throw;
                }
            }
        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName, out string errorMessage)
        {
            try
            {
                if (SharePointRetentionLabel == null)
                {
                    InitRetentionLabelCollections(site);
                    mLog.Info($"Init retention label collection for site collection: {site.Url}, total label count: {SharePointRetentionLabel.Count}");
                }
                if (SharePointRetentionLabel.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        mLog.Info($"The config retention label has the setting of BlockEdit and BlockDelete, tag name:{complianceTagName}, defaultUnlocked: {info.UnlockedAsDefault}, site url:{site.Url}");
                        errorMessage = string.Empty;
                        return true;
                    }
                    mLog.Warn($"The config retention label does not have the setting of BlockEdit and BlockDelete, tag name:{complianceTagName}, site url:{site.Url}");
                    errorMessage = "RM_JM_JD_DeclaredRecordsMigration_Comment_MissingBlock";
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                    errorMessage = "RM_JM_JD_DeclaredRecordsMigration_Comment_LabelNotFound";
                }
                return false;
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                errorMessage = ex.Message;
                return false;
            }
        }

        public void InitRetentionLabelCollections(IAveSite site)
        {
            if (SharePointRetentionLabel == null)
            {
                lock (mSPLabelLock)
                {
                    if (SharePointRetentionLabel == null)
                    {
                        var availableTags = site.GetAvailableTagsForSite();
                        SharePointRetentionLabel = availableTags.ToDictionary(r => r.TagName, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
        }

        protected async Task ProcessWebAsync(IAveWeb discoverWeb)
        {
            using (PerformanceScope scope = new PerformanceScope("DeclaredRecordMigrationProcessor.ProcessWeb", $"DeclaredRecordMigrationProcessor.ProcessWeb.[{discoverWeb.Title}]", addToStatistics: true))
            {
                try
                {
                    mLog.Info($"Start process web: {discoverWeb.Url}, isRootWeb: {discoverWeb.IsRootWeb}");
                    ReportManager.Increase();
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        await ProcessListsAsync(discoverWeb);

                        if (discoverWeb.Webs.Count == 0) return;

                        ReportManager.IncreaseBase(discoverWeb.Webs.Count);
                        foreach (var discoverSubWeb in discoverWeb.Webs)
                        {
                            await ProcessWebAsync(discoverSubWeb);
                        }
                    }
                }
                catch (JobStopException)
                {
                    mJobHasStopped = true;
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while processing web: {0}, error message: {1}.", discoverWeb.Url, e);
                    AddFailedJobDetailReport(discoverWeb.Url, "Site", e.Message);
                }
                finally
                {
                    ReportManager.Increase();
                    SafeDisposeObject(discoverWeb);
                }
            }
        }

        private async Task ProcessListsAsync(IAveWeb parentWeb)
        {
            if (parentWeb.Lists.Count == 0) return;

            ReportManager.IncreaseBase(parentWeb.Lists.Count);

            foreach (var discoverList in parentWeb.Lists)
            {
                await ProcessListAsync(discoverList, parentWeb);
            }
        }

        private bool CheckIsJPMCFinalList(IAveList discoverList, out bool hasJPMCRecordStatusField)
        {
            hasJPMCRecordStatusField = false;
            try
            {
                if (!IsEnableJPMCCustomization)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(CurrentRecordStatusFieldName))
                {
                    mLog.Warn($"Current JPMC record status column is empty");
                    return false;
                }

                var statusField = discoverList.Fields.GetFieldByInternalName(CurrentRecordStatusFieldName, false);
                if (statusField != null)
                {
                    hasJPMCRecordStatusField = true;
                }
                if (statusField?.DefaultValue == RMSyncCustomization4JPMC.RECORDSTATUS_FINAL)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while checking if list {discoverList.RootFolder.ServerRelativeUrl} is JPMC final list, error: {ex}");
                throw;
            }
            
            return false;
        }
        private void SetLabelForJPMCFinalList(IAveList discoverList)
        {
            try
            {
                discoverList.SetListComplianceTag(new AveComplianceTagInfo() { TagName = _recordsLabel, BlockEdit = true, BlockDelete = true });
            }
            catch (Exception ex)
            {
                mLog.Error($"Error occurred while setting label for JPMC final list: {discoverList.Title}, error: {ex}");
                throw;
            }
        }

        protected async Task ProcessListAsync(IAveList discoverList, IAveWeb parentWeb, CancellationTokenSource cts = null)
        {
            var listFullUrl = MakeFullUrl(parentWeb.Url, discoverList.RootFolder.Url);
            using (PerformanceScope scope = new PerformanceScope("DeclaredRecordMigrationProcessor.ProcessList", $"DeclaredRecordMigrationProcessor.ProcessList.[{listFullUrl}]", addToStatistics: true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        int listTemplate = (int)discoverList.BaseTemplate;
                        if (listTemplate == 600)
                        {
                            mLog.Info("Skip external list {0}", discoverList.RootFolder.Name);
                            return;
                        }

                        //Skip the system list & custom list
                        if (CheckIsDesignList(discoverList.RootFolder.Name + listTemplate.ToString()) || discoverList.Hidden)
                        {
                            mLog.Info("Skip the design list & system list{0}", discoverList.RootFolder.Name);
                            return;
                        }

                        mLog.Info($"Start process list: {listFullUrl}, listTemplate: {listTemplate}");

                        if (CheckIsJPMCFinalList(discoverList, out var hasJPMCRecordStatusField))
                        {
                            SetLabelForJPMCFinalList(discoverList);
                        }

                        int rowLimit = Site.GetMaxItemsPerThrottledOperation();
                        var cm = GetCAMLManager(hasJPMCRecordStatusField);
                        var endIndex = SPCommonUtility.GetLastItemFolderId(discoverList, discoverList.RootFolder);
                        if (cm != null)
                        {
                            ConfigItemsByQueryInfo(
                                new SPQueryInfo()
                                {
                                    List = discoverList,
                                    CAML = cm,
                                    RowLimit = rowLimit,
                                    MaxItemId = endIndex,
                                    CurrentFolder = discoverList.RootFolder,
                                    ScopeType = Types.ScopeTypes.RecursiveAll
                                },
                                items =>
                                {
                                    return ProcessItems(parentWeb, discoverList, items, hasJPMCRecordStatusField);
                                }
                            );
                        }
                    }
                }
                catch (JobStopException)
                {
                    mJobHasStopped = true;
                    cts?.Cancel();
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException is ServerException se)
                    {
                        if (se.ServerErrorCode == AveSPErrorCode.TP_E_FIELDNOTFOUND)
                        {
                            mLog.Warn($"The list {listFullUrl} is not enable Declare Record Setting and does not have and declared record item. Skip");
                            return;
                        }
                    }
                    mLog.Error("An error occurred while processing list: {0}, error message: {1}.", listFullUrl, e.ToString());
                    AddFailedJobDetailReport(listFullUrl, "List", e.Message);
                }
                finally
                {
                    ReportManager.Increase();
                    SafeDisposeObject(discoverList);
                }
            }
        }

        private CAMLManager GetCAMLManager(bool hasJPMCRecordStatusField)
        {
            CAMLManager cm = new CAMLManager();
            var condition = new QueryCondition(
                   Types.JoinTypes.And,
                   Types.FieldRefTypes.Name,
                   "_vti_ItemDeclaredRecord",
                   //"_vti_ItemHoldRecordStatus"
                   Types.FieldTypes.DateTime,
                   Types.QueryTypes.IsNotNull,
                   null,
                   true);

            if(IsEnableJPMCCustomization && hasJPMCRecordStatusField)
            {
                cm.QueryGroup.AddGroup(
                    new QueryGroup(Types.JoinTypes.And)
                    {
                        Conditions = {
                        new QueryCondition(Types.JoinTypes.Or, Types.FieldRefTypes.Name, Archiver.CAMLHelper.SPBuiltInFieldName.FSObjType, Types.FieldTypes.Integer, Types.QueryTypes.Eq, "1", null),
                        condition,
                        }
                    });
            }
            else
            {
                cm.QueryGroup.AddCondition(condition);
            }

            return cm;
        }

        public static long ConfigItemsByQueryInfo(SPQueryInfo queryInfo, Func<List<RMDiscoverItem>, int> callbackFun)
        {
            IAveList list = null;
            IAveFolder folder = null;
            long total = 0;
            if (queryInfo.Valid())
            {
                list = queryInfo.List;
                folder = queryInfo.CurrentFolder;
                if (queryInfo.MaxItemId > 0)
                {
                    var startIndex = queryInfo.StartIndex;
                    var endIndex = queryInfo.MaxItemId;
                    var rowLimit = queryInfo.RowLimit;
                    var cm = queryInfo.CAML;
                    ScopeTypes ScopeType = queryInfo.ScopeType;

                    while (endIndex > queryInfo.StartIndex)
                    {
                        //清理每次的ID Query 条件, 重新设置ID查询范围
                        cm.QueryGroup.Conditions.RemoveAll(g => g.Query.Field == SPColumnConstants.SP_ID);
                        startIndex = endIndex - rowLimit > queryInfo.StartIndex ? endIndex - rowLimit : queryInfo.StartIndex;
                        mLog.Debug($"query list index: {list.RootFolder.ServerRelativeUrl}, query by id from {startIndex} to {endIndex}.");
                        AveCamlQuery query = new AveCamlQuery();

                        cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
                        cm.QueryGroup.AddCondition(new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
                        cm.ScopeType = ScopeType;
                        if (!string.IsNullOrEmpty(queryInfo.ServerRelativeUrl))
                        {
                            query.FolderServerRelativeUrl = queryInfo.ServerRelativeUrl;
                        }
                        cm.RowLimit = rowLimit;
                        string queryXml = cm.GetFullCAML();
                        query.ViewXml = queryXml;
                        mLog.Debug($"query list xml:{queryXml}");
                        query.LoadAllItems = false;
                        List<RMDiscoverItem> items = new List<RMDiscoverItem>();
                        IAveListItemCollection tempItems;
                        using (PerformanceScope scope = new PerformanceScope("List.GetItemsForRecordsByCAMLs", addToStatistics: true))
                        {
                            tempItems = list.GetItemsForRecords(query);
                        }
                        foreach (var item in tempItems)
                        {
                            items.Add(new RMDiscoverItem(item, null));
                        }

                        mLog.Debug($"list result:{list.RootFolder.ServerRelativeUrl}, item count:{items.Count}");
                        total += callbackFun(items);

                        endIndex = startIndex != queryInfo.StartIndex ? startIndex : queryInfo.StartIndex;

                    }
                }

            }

            return total;
        }

        protected int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items, bool hasJPMCRecordStatusField)
        {
            int results = 0;
            if (items != null && items.Count > 0)
            {
                ReportManager.IncreaseBase(items.Count);
                using (PerformanceScope scope = new PerformanceScope("DeclaredRecordMigrationProcessor.ProcessItems", $"DeclaredRecordMigrationProcessor.ProcessItemsOfList[{list.Title}]", addToStatistics: true))
                {
                    //int tempCounter = 0;
                    //int objectLevel = list.BaseType == AveBaseType.DocumentLibrary ? (int)RMReportObjectLevel.Document : (int)RMReportObjectLevel.Item;
                    foreach (var item in items)
                    {
                        results += ProcessOneItem(web, list, item, hasJPMCRecordStatusField);
                    }
                }
            }
            return results;
        }

        //private int RunMultiThreadsProcessItems(List<RMDiscoverItem> items, IAveWeb web, IAveList list)
        //{
        //    using (PerformanceScope scope = new PerformanceScope("RunMultiThreadsProcessItems", $"RunMultiThreadsProcessItemsOfList[{list.Title}]", addToStatistics: true))
        //    {
        //        mLog.Info($"Run multi threads to process items, items count : {items.Count}");
        //        var cts = new CancellationTokenSource();
        //        var t = AveTenantTasks.RunAndWaitResult(items, cts, item =>
        //        {
        //            return ProcessOneItem(web, list, item, cts);
        //        });
        //        return t;
        //    }
        //}

        private int ProcessOneItem(IAveWeb web, IAveList list, RMDiscoverItem discoverItem, bool hasJPMCRecordStatusField, CancellationTokenSource cts = null)
        {
            var result = 0;
            using (PerformanceScope scope = new PerformanceScope("DeclaredRecordMigrationProcessor.ProcessItem", $"DeclaredRecordMigrationProcessor.ProcessItemOfList[{list.Title}]", addToStatistics: true))
            {
                var item = discoverItem.CurrentItem;
                var report = new JMDeclaredRecordsMigrationJobDetails()
                {
                    //Url = item.Url,
                    //NodeType = "Item",
                    Status = JobDetailsStatus.Successful,
                };
                try
                {
                    using CheckJobStopScope jScope = new CheckJobStopScope();
                    //mLog.Info("Process item {0}", item.Url);
                    //objectLevel = ProcessObjectLevel(list, item, objectLevel);

                    if (list.BaseType == AveBaseType.DocumentLibrary)
                    {
                        report.Url = MakeFullUrl(web.Url, item.Url);
                        report.NodeType = RMReportObjectLevel.Document.ToString();
                    }
                    else
                    {
                        report.Url = WebUtil.GetListItemRealPath(web.Url, list.RootFolder.ServerRelativeUrl, item.Url);
                        report.NodeType = RMReportObjectLevel.Item.ToString();
                    }
                    
                    var isFolder = item.FileSystemObjectType == AveFileSystemObjectType.Folder;
                    if (isFolder && hasJPMCRecordStatusField)
                    {
                        try
                        {
                            var status = item.GetItemFieldValue(CurrentRecordStatusFieldName);
                            if (status == RMSyncCustomization4JPMC.RECORDSTATUS_FINAL)
                            {
                                mLog.Info($"Start to set compliance tag to folder, item id: {item.ID}, uniqueId: {item.UniqueId}, exist label?: {item.GetComplianceTagName()}");
                                item.SetComplianceTag(_recordsLabel, true, true, false, false);
                                mLog.Info($"Set compliance tag to folder successfully, itemid: {item.ID}, uniqueId: {item.UniqueId}");
                            }
                            else
                            {
                                mLog.Info($"The folder item id: {item.ID}, uniqueId: {item.UniqueId}, record status value: {status}");
                            }
                        }
                        catch (JobStopException)
                        {
                            throw;
                        }
                        catch (ServerException se)
                        {
                            mLog.Error($"An server error occurred while set compliance tag to folder, item id: {item?.UniqueId}, error code: {se.ServerErrorCode}, error message: {se}.");
                            throw;
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("An error occurred while set compliance tag to folder, item id: {0}, error message: {1}.", item?.UniqueId, ex);
                            if (ex.InnerException != null && ex.InnerException is JobStopException) // The JobStopException thrown may be wrapped by another ex from GetResponse
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            throw new Exception("RM_JM_JD_DeclaredRecordsMigration_Comment_ApplyLabelFail");
                        }
                    }
                    else
                    {
                        try
                        {
                            Record.UndeclareItemAsRecord(item);
                        }
                        catch (JobStopException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("An error occurred while undeclare item as record, item id: {0}, error message: {1}.", item?.UniqueId, ex);
                            if (ex.InnerException != null && ex.InnerException is JobStopException) // The JobStopException thrown may be wrapped by another ex from GetResponse
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                        }

                        try
                        {
                            mLog.Info($"Start to set compliance tag to item, item id: {item.ID}, uniqueId: {item.UniqueId}, exist label?: {item.GetComplianceTagName()}");
                            item.SetComplianceTag(_recordsLabel, true, true, false, false);
                            mLog.Info($"Set compliance tag to item successfully, itemid: {item.ID}, uniqueId: {item.UniqueId}");
                        }
                        catch (JobStopException)
                        {
                            throw;
                        }
                        catch (ServerException se)
                        {
                            mLog.Error($"An server error occurred while set compliance tag to item, item id: {item?.UniqueId}, error code: {se.ServerErrorCode}, error message: {se}.");
                            throw;
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("An error occurred while set compliance tag to item, item id: {0}, error message: {1}.", item?.UniqueId, ex);
                            if (ex.InnerException != null && ex.InnerException is JobStopException) // The JobStopException thrown may be wrapped by another ex from GetResponse
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            throw new Exception("RM_JM_JD_DeclaredRecordsMigration_Comment_ApplyLabelFail");
                        }
                    }

                    mJobHasSuccess = true;
                }
                catch (JobStopException)
                {
                    mJobHasStopped = true;
                    cts?.Cancel();
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mJobHasException = true;
                    mLog.Warn("Report item failed. item id: {0}, error message: {1}.", item?.UniqueId, ex.ToString());
                    report.Status = JobDetailsStatus.Failed;
                    report.Comment = ex.Message;
                }
                finally
                {
                    if (!mJobHasStopped)
                    {
                        report.FinishTime = DateTime.UtcNow.Ticks;
                        ReportManager.SendJobDetail(report);
                        ReportManager.Increase();
                        result = 1;
                    }
                }
                return result;
            }
        }

        protected string MakeFullUrl(string webUrl, string relativeUrl)
        {
            if (webUrl == null)
            {
                throw new ArgumentNullException("webUrl");
            }
            if (relativeUrl == null)
            {
                throw new ArgumentNullException("relativeUrl");
            }
            relativeUrl = relativeUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (relativeUrl.StartsWith("/"))
            {
                stringBuilder.Append(webUrl);
                stringBuilder.Append(relativeUrl);
            }
            else
            {
                stringBuilder.Append(webUrl);
                if (relativeUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(relativeUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        protected static void SafeDisposeObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var disposeObj = (obj as IDisposable);

            if (disposeObj != null)
            {
                disposeObj.Dispose();
            }
        }

        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (this.DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        public void AddFailedJobDetailReport(string fullPath, string nodeType, string comment)
        {
            mJobHasException = true;
            ReportManager.SendJobDetail(new JMDeclaredRecordsMigrationJobDetails()
            {
                Url = fullPath,
                NodeType = nodeType,
                FinishTime = DateTime.UtcNow.Ticks,
                Status = JobDetailsStatus.Failed,
                Comment = comment
            });
        }
    }
}
