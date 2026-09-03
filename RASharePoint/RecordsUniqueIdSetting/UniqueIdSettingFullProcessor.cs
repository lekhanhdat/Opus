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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting.Base;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting.JobReport;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.Contract.Exceptions;
namespace AvePoint.RA.SharePoint.RecordsUniqueIdSetting
{
    public class UniqueIdSettingFullProcessor : BaseUniqueIdSettingProcessor
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(UniqueIdSettingFullProcessor));
        private long mLastJobTicks = DateTime.MinValue.Ticks;
        public UniqueIdSettingFullProcessor(SPTreeNodeDto siteNode, RMUniqueIdSetting setting, ClientContext clientContext, string searchSiteColumnFileName, long lastJobTimeTicks) : base(siteNode, setting)
        {
            var bposInfo = PoolUserUtil.GetAveBPOSAccountInfo(siteNode.NodeExtension.BposInfo, siteNode.FullPath);
            var mfactory = MultiAppUtil.CreateAveObjectModelFactory(siteNode.FullPath, bposInfo, AveContextKind.ClientObjectModel);
            curSite = mfactory.CreateSite(siteNode.FullPath);
            currentClientContext = clientContext;
            this.searchSiteColumnFileName = searchSiteColumnFileName;
            mLastJobTicks = lastJobTimeTicks;
        }
        public override async System.Threading.Tasks.Task ProcessSiteCollectionAsync(AveDiscoverSite discoverSite)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    await base.ProcessSiteCollectionAsync(discoverSite);
                    var discoverWebs = discoverSite.GetWebs();
                    reportManager.IncreaseBase(discoverWebs.Count);
                    //ProgressService.IncreaseBase(discoverWebs.Count);
                    foreach (var discoverWeb in discoverWebs.Values)
                    {
                        logger.Info("Process Web UniqueId setting {0}", discoverWeb.FullUrl);
                        await ProcessWebAsync(discoverWeb);
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
                var discoverLists = discoverWeb.GetLists();
                reportManager.IncreaseBase(discoverLists.Count);
                //ProgressService.IncreaseBase(discoverLists.Count);
                foreach (var discoverList in discoverLists.Values)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (discoverList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }
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
                                await ProcessListAsync(discoverList);
                            }
                        }
                    }
                }
             }
            catch (JobStopException)
            {
                logger.Info("Job Stopped");
                throw new JobStopException("This Job is stopped.");
            }
        }
        public override async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverList discoverList)
        {
            if (discoverList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
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
                logger.Info("Skip the system list {0}", discoverList.RootFolderUrl);
                return;
            }

            if (mLastJobTicks != DateTime.MinValue.Ticks)
            {
                await ProcessItemsForSearchDiscoverAsync(list);
            }
            else
            {
                var discoverRootFolder = discoverList.GetRootFolder();

                bool needQueryNext = false;
                int maxItemId = GetLastItemId(list, list.RootFolder);
                int startIdx = 0;
                int lastIdx = 0;
                AveCamlQuery query = GetQuery(list.RootFolder, startIdx, startIdx + MaxItemsPerThrottledOperation, MaxItemsPerThrottledOperation);
                logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl}]");
                do
                {
                    logger.Info($"StartIndex:[{startIdx}] LastIndex:[{lastIdx}] MaxItemId:[{maxItemId}]");
                    var items = list.GetItemsForRecords(query);
                    if (items.Count > 0)
                    {
                        await ProcessItemsAsync(items);
                        int curIdx = items.Max(i => i.ID);
                        startIdx = curIdx;
                    }
                    else
                    {
                        startIdx = lastIdx;
                    }
                    int endIdx = startIdx + MaxItemsPerThrottledOperation;
                    lastIdx = endIdx;
                    needQueryNext = startIdx < maxItemId;
                    if (needQueryNext)
                    {
                        logger.Info($"Query Next");
                        query.ViewXml = GetQueryXml(startIdx, endIdx, MaxItemsPerThrottledOperation);
                    }
                    else
                    {
                        logger.Info($"Query finished.");
                    }
                }
                while (needQueryNext);
                #region old logic
                //string pagerInfo = string.Empty;
                //do
                //{
                //    logger.Info($"Get items under [{discoverRootFolder?.FullUrl}] with pager. PagerInfo:[{pagerInfo}]");
                //    var items = discoverRootFolder.GetItems(ref pagerInfo);
                //    ProcessItems(items, list, discoverRootFolder);
                //}
                //while (!string.IsNullOrEmpty(pagerInfo));

                //var subfolders = discoverRootFolder.GetSubFolders();
                ////ProgressService.IncreaseBase(subfolders.Count);
                //reportManager.IncreaseBase(subfolders.Count);
                //foreach (var folder in subfolders)
                //{
                //    try
                //    {
                //        ProcessFolder(folder);
                //    }
                //    catch (Exception e)
                //    {
                //        reportManager.SendJobDetail(new JMUniqueIDSettingJobDetails() { ObjectName = folder.FullUrl, SourceURL = discoverRootFolder.FullUrl, ColumnName = curSetting.Name, Action = I18NEntity.GetString("RM_UI_Detail_Add"), Status = JobDetailsStatus.Failed, Comment = e.Message });
                //        logger.Error("Proces folder failed {0}", e.ToString());
                //    }
                //}
                #endregion
            }
        }
        

        private async System.Threading.Tasks.Task ProcessItemsForSearchDiscoverAsync(IAveList list)
        {
            bool needQueryNext = false;
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            int maxItemId = GetLastItemId(list, list.RootFolder);

            int startIndex = 0;
            IAveListItemCollection items = null;
            DateTime startTime = DateTime.SpecifyKind(new DateTime(mLastJobTicks), DateTimeKind.Utc);
            DateTime endTime = DateTime.UtcNow;
            do
            {
                using (var queryAuto = new PerformanceScope("UniqueIdSettingInrementalProcessor.SearchQueryData", $"RMEnforceRetentionBase.SearchQueryData{list.RootFolder.ServerRelativeUrl} start{startIndex}", true))
                {
                    AveCamlQuery query = GetSearchDiscoverQuery(list, list.RootFolder, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        using (var performance = new PerformanceScope("UniqueIdSettingInrementalProcessor.GetItemsForRecords", addToStatistics: true))
                        {
                            items = list.GetItemsForRecords(query);
                        }
                    }
                    //JobContext.ReportManager.IncreaseBase(items.Count);
                    logger.Info($"Process items in folder url {list.RootFolder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                }
                using (var queryAuto = new PerformanceScope("UniqueIdSettingInrementalProcessor.ProcessAveItems", $"UniqueIdSettingInrementalProcessor.ProcessAveItems{list.RootFolder.ServerRelativeUrl} count {items.Count}", true))
                {
                    await ProcessItemsAsync(items);
                }
                if (startIndex + rowLimit < maxItemId)
                {
                    needQueryNext = true;
                    startIndex += rowLimit;
                    logger.Info($"PagingInfo:{startIndex}");
                }
                else
                {
                    needQueryNext = false;
                }
            }
            while (needQueryNext);
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
        //public override void ProcessFolder(AveDiscoverFolder discoverFolder)
        //{
        //    base.ProcessFolder(discoverFolder);
        //    var list = discoverFolder.AveFolder.ParentList;
        //    string pagerInfo = string.Empty;
        //    do
        //    {
        //        logger.Info($"Get items under [{discoverFolder?.FullUrl}] with pager. PagerInfo:[{pagerInfo}]");
        //        var items = discoverFolder.GetItems(ref pagerInfo);
        //        ProcessItems(items, list, discoverFolder);
        //    }
        //    while (!string.IsNullOrEmpty(pagerInfo));

        //    var allSubFolders = discoverFolder.GetSubFolders();
        //    reportManager.IncreaseBase(allSubFolders.Count);
        //    //ProgressService.IncreaseBase(allSubFolders.Count);
        //    foreach (var subfolder in allSubFolders)
        //    {
        //        ProcessFolder(subfolder);
        //    }
        //}

        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        public string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result}");
            return result;
        }

        public string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result}");
            return result;
        }

        public int InnerGetLastItemId(IAveList list, IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = list.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        public int GetLastItemId(IAveList list, IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(list, folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(list, folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        public string GetQueryXml(int startIdx, int endIdx, int rowLimit)
        {
            string queryXml = $@"
                <View Scope='RecursiveAll'>
                    <Query>
                        <Where>
                            <And>
                                <Gt><FieldRef Name='ID'/><Value Type='Integer'>{startIdx}</Value></Gt>
                                <Leq><FieldRef Name='ID'/><Value Type='Integer'>{endIdx}</Value></Leq>
                            </And>
                        </Where>
                    </Query>
                    <RowLimit>{rowLimit}</RowLimit>
                </View>";
            logger.Info($"ApplyExisting query xml: {queryXml}");
            return queryXml;
        }

        public AveCamlQuery GetQuery(IAveFolder folder, int startIndex, int endIndex, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            query.ViewXml = GetQueryXml(startIndex, endIndex, rowLimit);
            return query;
        }
    }
}
