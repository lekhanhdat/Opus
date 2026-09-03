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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting.Base;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting.JobReport;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using System.Threading;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.Extension;

namespace AvePoint.RA.SharePoint.RecordsUniqueIdSetting
{
    public class UniqueIdSettingInrementalProcessor : BaseUniqueIdSettingProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(UniqueIdSettingInrementalProcessor));       
     
        public UniqueIdSettingInrementalProcessor(SPTreeNodeDto siteNode, RMUniqueIdSetting setting) : base(siteNode, setting)
        {

        }
        public override async System.Threading.Tasks.Task ProcessSiteCollectionAsync(AveDiscoverSite discoverSite)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    await base.ProcessSiteCollectionAsync(discoverSite);
                    //Create site collection && check start time
                    var discoverWebs = discoverSite.GetChangeWebs();
                    //ProgressService.IncreaseBase(discoverWebs.Count);
                    reportManager.IncreaseBase(discoverWebs.Count);
                    foreach (var discoverWeb in discoverWebs.Values)
                    {
                        if (discoverWeb.ChangeType != ChangeType.Delete)
                        {
                            logger.Info("Process Web UniqueId setting {0}", discoverWeb.FullUrl);
                            await ProcessWebAsync(discoverWeb);
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch(Exception ex) 
            {
                logger.Error("Unique ID Job Error");
                throw ex;
            }
        }
        public override async System.Threading.Tasks.Task ProcessWebAsync(AveDiscoverWeb discoverWeb)
        {
            if (!IsEnableRecordManagementForWebOrList(discoverWeb.WebID))
            {
                logger.Info("Process web SharePoint setting is disable {0}", discoverWeb.FullUrl);
                reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails()
                {
                    ObjectName = discoverWeb.Name,
                    SourceURL = discoverWeb.FullUrl,
                    ColumnName = curSetting.Name,
                    Action = "RM_UI_Detail_Add",
                    Status = JobDetailsStatus.Skipped,
                    Comment = "RM_JS_JMD_DisableRecordManagement"
                });
                return;
            }
            try 
            {
                await base.ProcessWebAsync(discoverWeb);
                var discoverLists = discoverWeb.GetChangeLists();
                //ProgressService.IncreaseBase(discoverLists.Count);
                reportManager.IncreaseBase(discoverLists.Count);
                foreach (var discoverList in discoverLists.Values)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (discoverList.ChangeType != ChangeType.Delete)
                        {
                            logger.Info("Process list UniqueId setting {0}", discoverList.RootFolderUrl);
                            var list = discoverList.GetListObject();
                            if (list.BaseType == AveBaseType.DocumentLibrary)
                            {
                                try
                                {
                                    if (!IsEnableRecordManagementForWebOrList(discoverList.ListId))
                                    {
                                        logger.Info("Process list SharePoint setting is disable {0}", discoverList.RootFolderUrl);
                                        reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails()
                                        {
                                            ObjectName = discoverList.Name,
                                            SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url),
                                            ColumnName = DocumentIdDisplayName,
                                            Action = "RM_UI_Detail_Add",
                                            Status = JobDetailsStatus.Skipped,
                                            Comment = "RM_JS_JMD_DisableRecordManagement"
                                        });
                                        continue;
                                    }
                                    if (list.Hidden)
                                    {
                                        logger.Info("Skip the hidden list {0}", discoverList.RootFolderUrl);
                                        continue;
                                    }
                                    if (CheckIsDesignList(list))
                                    {
                                        logger.Info("Skip the system list {0}", discoverList.RootFolderUrl);
                                        continue;
                                    }
                                    var allField = list.Fields;
                                    IAveField field = list.Fields.GetFieldById(DocumentIDColumnID, false);
                                    if (field != null)
                                    {
                                        IAveView defaultView = list.DefaultView;
                                        IAveViewFieldCollection viewFields = defaultView.ViewFields;
                                        if (!viewFields.Exists(SPColumnConstants.DocumentIdUrl))
                                        {
                                            viewFields.Add(field);
                                            defaultView.Update();
                                            reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = DocumentIdDisplayName, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Successful, Comment = string.Empty });
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn("Config Document ID column failed {0}", e.ToString());
                                    reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = list.Title, SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), ColumnName = DocumentIdDisplayName, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Failed, Comment = GetExceptionMessage(e) });
                                    haveErrorNode = true;
                                }
                            }
                            else
                            {
                                if (!NeedSkipList())
                                {
                                    await ProcessListAsync(discoverList, discoverWeb.WebID);
                                }
                            }
                        }
                    }
                } 
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
        
            
        }
        public async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverList discoverList, Guid webId)
        {
            var list = discoverList.GetListObject();
            if (!IsEnableRecordManagementForWebOrList(discoverList.ListId))
            {
                logger.Info("Process list SharePoint setting is disable {0}", discoverList.RootFolderId);
                reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails()
                {
                    ObjectName = discoverList.Name,
                    SourceURL = RA.Common.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url),
                    ColumnName = curSetting.Name,
                    Action = "RM_UI_Detail_Add",
                    Status = JobDetailsStatus.Skipped,
                    Comment = "RM_JS_JMD_DisableRecordManagement"
                });
                return;
            }
            await base.ProcessListAsync(discoverList);
            if (list.Hidden)
            {
                logger.Info("Skip the hidden list {0}", discoverList.RootFolderUrl);
                return;
            }
            if (CheckIsDesignList(list))
            {
                return;
            }

            await ProcessItemsForIncrementalJobAsync(list, discoverList, webId);

        }

        private async System.Threading.Tasks.Task ProcessItemsForIncrementalJobAsync(IAveList list, AveDiscoverList discoverList, Guid webId)
        {
            var changedItems = discoverList.GetListChangedItems(webId);
            logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental UniqueId job.ChangedItems Count:[{changedItems.Count}].");

            var changedObjects = changedItems.Values.Select(i => i as Dictionary<string, object>).Where(i => (i.ContainsKey("Hidden") && !(bool)i["Hidden"]) || !i.ContainsKey("Hidden")).ToList();
            var existingItemIds = changedObjects.Where(i => (int)i["ChangeType"] != (int)Wrapper.Common.ChangeType.Delete).Select(i => (int)i["ItemId"]).ToList();
            reportManager.IncreaseBase(existingItemIds.Count);
            for (int i = 0; i < existingItemIds.Count; i += 2000)
            {
                var rowIds = existingItemIds.Skip(i).Take(2000).ToList();
                IEnumerable<IAveListItem> items = null;
                using (var performance00 = new PerformanceScope("RMSPExplorerProcessor.GetItemsForRecordsByRowIdTotal", addToStatistics: true))
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        for (int j = 0; j < rowIds.Count; j += 100)
                        {
                            //经测试，每次查询120个rowid时性能较好
                            var tempRowIds = rowIds.Skip(j).Take(100).ToList();
                            AveCamlQuery query = GetRowIdDiscoverQuery(list, list.RootFolder, tempRowIds);
                            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetItemsForRecordsByRowId", addToStatistics: true))
                            {
                                var tempItems = list.GetItemsForRecords(query, j == 0);
                                if (tempItems != null)
                                {
                                    if (items == null)
                                    {
                                        items = tempItems;
                                    }
                                    else
                                    {
                                        items = items.Concat(tempItems);
                                    }
                                }
                            }
                        }
                    }
                }
                int existingItemsPerTask = items.Count() / 4;
                CancellationTokenSource cts = null;
                if (items.Count() > 400)
                {
                    cts = new CancellationTokenSource();
                    //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                    AveTenantTasks.RunParallel(items, existingItemsPerTask, cts, changedItem =>
                    {
                        ProcessIncrementalChangedItemAsync(list, changedItem, cts).Wait();
                    });
                }
                else
                {
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(items, nameof(items));
                    foreach (var changedItem in items)
                    {
                        await ProcessIncrementalChangedItemAsync(list, changedItem);
                    }
                }
            }
        }      

        public async System.Threading.Tasks.Task ProcessItemsAsync(IAveListItemCollection items)
        {
            logger.Info($"Process item count:{items.Count}");
            reportManager.IncreaseBase(items.Count);
            foreach (var item in items)
            {
                try
                {
                    //reportManager.Increase();
                    await SetUniqueIdAsync(item);
                }
                catch (Exception e)
                {
                    haveErrorNode = true;
                    //reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = item.LeafName, SourceURL = discoverFolder.FullUrl, ColumnName = curSetting.Name, Action = string.Empty, Status = JobDetailsStatus.Failed, Comment = e.Message });
                    logger.Error("Set Unique item unique id failed {0}", e.ToString());
                }
            }
        }
       
        protected async System.Threading.Tasks.Task ProcessIncrementalChangedItemAsync(IAveList list, IAveListItem aveItem, CancellationTokenSource cts = null)
        {
            try
            {
                if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                {
                    logger.Info($"Current list item is folder so skip it.Url:{aveItem.Url}.Id:{aveItem.ID}.");
                    return;
                }
                await SetUniqueIdAsync(aveItem);//TO DO 
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                haveErrorNode = true;
                reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = aveItem.Name, SourceURL = aveItem.Url, ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Failed, Comment = GetExceptionMessage(e) });
                logger.Warn("Set Unique item unique id failed {0}.", e.ToString());
            }
        }
        public AveCamlQuery GetRowIdDiscoverQuery(IAveList list, IAveFolder folder, List<int> rowIds)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                var group = new QueryGroup();
                foreach (var rowId in rowIds)
                {
                    group.Conditions.Add(new QueryCondition(
                             Types.JoinTypes.Or,
                             Types.FieldRefTypes.Name,
                              "ID",
                            Types.FieldTypes.Number,
                            Types.QueryTypes.Eq,
                             rowId.ToString(), false));
                }
                cm.QueryGroup.AddGroup(group);
                //AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                string queryXml = cm.GetFullCAML(false);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                logger.Info($"Process Folder {folder.ServerRelativeUrl}, row id count: {rowIds.Count}");
                //logger.Info("Query XML:{0}", query.ViewXml);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
            }
            return query;
        }

        public override async System.Threading.Tasks.Task ProcessFolderAsync(AveDiscoverFolder discoverFolder)
        {
            List<AveDiscoverItem> changedItems = null;
            List<AveDiscoverFolder> allSubFolders = null;
            string folderFullPath = null;
            IAveList list = null;
            using (discoverFolder)
            {
                await base.ProcessFolderAsync(discoverFolder);
                list = discoverFolder.AveFolder.ParentList;
                changedItems = discoverFolder.GetChangeItems();
                allSubFolders = discoverFolder.GetChangeSubFolders();
                reportManager.IncreaseBase(allSubFolders.Count);
                folderFullPath = discoverFolder.FullUrl;
            }


            foreach (var item in changedItems)
            {
                //ProgressService.IncreaseBase(1);
                reportManager.Increase();
                if (item.ChangeType != ChangeType.Delete)
                {
                    if (item.ID != null && item.ID != 0)
                    {
                        await SetUniqueIdAsync(list.GetItemById((int)item.ID));
                    }
                }
            }
            //ProgressService.IncreaseBase(allSubFolders.Count);
            foreach (var subfolder in allSubFolders)
            {
                if (subfolder.ChangeType != ChangeType.Delete)
                {
                    try
                    {
                        await ProcessFolderAsync(subfolder);
                    }
                    catch (Exception e)
                    {
                        reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = subfolder.FullUrl, SourceURL = folderFullPath, ColumnName = curSetting.Name, Action = "RM_UI_Detail_Add", Status = JobDetailsStatus.Failed, Comment = GetExceptionMessage(e) });
                        logger.Warn("Process folder failed {0}", e.ToString());
                    }
                }
            }
        }

        private DateTime ModifyTime(DateTime time)
        {
            if (time == DateTime.MinValue) return time;

            int offsetInMinuete = 120; // default value is 2 hours
            int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.UNIQUE_ID_JOB_RUN_TIME_OFFSET_MINUTES], out offsetInMinuete);
            var runTime = time.AddMinutes(-offsetInMinuete);
            logger.Info($"Modified job run time : {runTime}");
            return runTime;
        }


        public override async Task<bool> RunAsync()
        {
            var runTime = ModifyTime(DateTime.UtcNow);
            bool isEnableRecordManagement = false;
            bool errorNode = false;
            try
            {
                var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(curNode.NodeExtension.BposInfo, curNode.FullPath);
                var mfactory = MultiAppUtil.CreateAveObjectModelFactory(curNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                curSite = mfactory.CreateSite(curNode.FullPath);
                var startTime = GetStartDate(curNode.Parent.SPObjectId, curNode.ID);//TO DO(agent comment)
                AveDiscoverSite tmpDiscoverSite = null;
                if (IsEnableRecordManagementForSiteOrGroup())
                {
                    //var factory = AveObjectModelFactory.CreateObjectModelFactory(curSite.Url, bposInfo, AveContextKind.ClientObjectModel);
                    //IAveTenant tenant = factory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, curSite.Url));
                    //var siteProperties = tenant.GetSitePropertiesByUrl(curSite.Url);
                    //SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, curSite.Url);
                    isEnableRecordManagement = true;
                    InitSearchContext(bposInfo, curSite.Url);  // init context for search
                    await EnableFeatureAndUpdateBeginIDAsync();
                    if (startTime == DateTime.MinValue)
                    {
                        logger.Info("need start full unique id setting job :{0}", curNode.FullPath);
                        UniqueIdSettingFullProcessor fullProcessor = new UniqueIdSettingFullProcessor(curNode, curSetting, currentClientContext, searchSiteColumnFileName, DateTime.MinValue.Ticks);
                        tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);

                        await fullProcessor.ProcessSiteCollectionAsync(tmpDiscoverSite);
                        errorNode = this.haveErrorNode;
                    }
                    else if (NeedRunSearchDiscover(startTime.Ticks))
                    {
                        logger.Info($"Run job with search discover. Last Job Time:{startTime.ToString()}");
                        UniqueIdSettingFullProcessor fullProcessor = new UniqueIdSettingFullProcessor(curNode, curSetting, currentClientContext, searchSiteColumnFileName, startTime.Ticks);
                        tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
                        await fullProcessor.ProcessSiteCollectionAsync(tmpDiscoverSite);
                        errorNode = this.haveErrorNode;
                    }
                    else
                    {

                        tmpDiscoverSite = new AveDiscoverSite(curSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, startTime, runTime);
                        await this.ProcessSiteCollectionAsync(tmpDiscoverSite);
                        errorNode = this.haveErrorNode;
                    }
                }
                else
                {
                    reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = curSetting.Name, Status = JobDetailsStatus.Skipped, Comment = "RM_JS_JMD_DisableRecordManagement" });
                }
            }
            catch (JobStopException ex)
            {
                logger.Info("Unique ID Settings Incremental Job is stopped.");
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Warn("Set Unique Id error {0}", e.ToString());
                reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = curNode.Name, SourceURL = curNode.FullPath, ColumnName = curSetting.Name, Status = JobDetailsStatus.Failed, Comment = GetExceptionMessage(e) });
                errorNode = true;
            }
            finally
            {
                currentClientContext?.Dispose();

                using (curSite)
                { }
                if (isEnableRecordManagement)
                {
                    RMNodeFlagDao.AddSiteFlagInfo(new RMNodeFlag()
                    {
                        NodeId = new Guid(curNode.SPObjectId),
                        Title = curNode.Name,
                        FullPath = curNode.FullPath,
                        //AveId = new Guid(curNode.ID),(agent comment)
                        CollectionTime = runTime.Ticks,
                        //CollectionTime = DateTime.UtcNow.Ticks,
                        GroupId = new Guid(curNode.Parent.ID),
                        IsRemoved = false,
                        NodeFlagType = (int)(IsTeams ? NodeFlagType.TeamsUniqueId : NodeFlagType.UniqueId)
                    });
                }
            }
            return errorNode;
        }



        //上次运行job是在59天以前，本次Job采用CAML Query方式，防止由于change log被冲掉了导致少查数据
        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }

    }
}
