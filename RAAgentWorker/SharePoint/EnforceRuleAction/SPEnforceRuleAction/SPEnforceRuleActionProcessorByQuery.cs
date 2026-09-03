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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AvePoint.RA.SharePoint.EnforceRuleAction
{
    public class SPEnforceRuleActionProcessorByQuery : BaseSPEnforceRuleActionProcessor
    {
        protected int QueryConditionMaxCount = 2000;
        public SPEnforceRuleActionProcessorByQuery() : base()
        {

        }
        public SPEnforceRuleActionProcessorByQuery(SPTreeNodeDto current, EnforceRuleActionJobMessage mMessage) : base(current, mMessage)
        {

        }
        public override void ProcessSiteCollection(SPTreeNodeDto site)
        {
            ProgressService.IncreaseBase(1);
            IAveSite aveSite = null;
            AveDiscoverSite discoverSite = null;
            aveSite = ObjectModelFactory.CreateSite(site.FullPath);
            QueryConditionMaxCount = GetMaxItemsPerThrottledOperation(aveSite);
            InitWssIdsForTerms(aveSite);
            discoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
            //var allDiscoverWebs = discoverSite.GetWebs().Values;
            var discoverWeb = discoverSite.GetRootWeb();
            //foreach (var discoverWeb in allDiscoverWebs)
            {
                try
                {
                    ProcessSite(discoverWeb);
                }
                catch (Exception e)
                {
                    JobHasErrorNode = true;
                    AddFailedDetail(discoverWeb, e.Message);
                    logger.Warn($"Process site collection failed {e.ToString()}.");
                }
            }
            DisposeSPObj(aveSite);
            base.ProcessSiteCollection(site);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="site"></param>
        /// <param name="skipCheckBreakInherit">SubSite节点Run Job，不check当前节点打破继承</param>
        public override void ProcessSite(AveDiscoverWeb site, bool skipCheckBreakInherit = false)
        {
            ProgressService.IncreaseBase(1);
            logger.Info($"Process site {site.WebID}.");
            if (!skipCheckBreakInherit && !site.AveWeb.IsRootWeb && IsBreakInheritNode(site.AveWeb.Url))
            {
                logger.Info($"Current site IsBreakInheritNode {site.AveWeb.ID}.");
                return;
            }
            lock (LockObj)
            {
                try
                {
                    TimeZones.Add(site.AveWeb.ID, site.AveWeb.RegionalSettings.TimeZone);
                }
                catch (Exception e)
                {
                    logger.Info($"Init time zone failed {e.ToString()}.");
                }
            }
            foreach (var list in site.GetLists())
            {
                try
                {
                    string listUrl = string.IsNullOrEmpty(list.Value.RootFolderUrl) ? string.Empty : list.Value.RootFolderUrl;//System Root Folder Url is null.
                    if (IsBreakInheritNode(listUrl))
                    {
                        logger.Info($"Current list IsBreakInheritNode {listUrl.LogBase64()}.");
                        continue;
                    }
                    ProcessList(list.Value);
                }
                catch (Exception e)
                {
                    JobHasErrorNode = true;
                    AddFailedDetail(list.Value, e.Message);
                    logger.Warn($"Process list failed {e.ToString()}.");
                }
            }
            logger.Info($"Current run job node is site and need discover sub site.Url: {site.WebID}.");
            //site.GetSubWebs()获取的只是当前层Site的SubSite，对于SubSite的SubSite是获取不到的
            var allDiscoverWebs = site.GetSubWebs().Values;
            foreach (var discoverWeb in allDiscoverWebs)
            {
                try
                {
                    ProcessSite(discoverWeb);
                }
                catch (Exception e)
                {
                    JobHasErrorNode = true;
                    AddFailedDetail(discoverWeb, e.Message);
                    logger.Warn($"Process site failed {e.ToString()}.");
                }
            }
            DisposeSPObj(site);
            base.ProcessSite(site);
        }
        public override void ProcessList(AveDiscoverList discoverList)
        {
            ProgressService.IncreaseBase(1);
            if (IsSystemList(discoverList))
            {
                return;
            }
            IAveList list = null;
            try
            {
                list = discoverList.GetListObject();
            }
            catch (Exception e)
            {
                logger.Info($"Skip list type {discoverList.ServerTemplate} :{discoverList.Title.LogBase64()} Can't get listobj.");
                return;
            }
            #region
            //if (list.BaseType != AveBaseType.DocumentLibrary)
            //{
            //    logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
            //    return;
            //}
            //IAveTaxonomyField mmsField = GetTaxonomyField(list.Fields, BCSColumnName);
            //var BCSColumnInternalName = mmsField.InternalName;
            //var termIds = GetTermIds(mmsField);
            //#region init Camel Query
            //bool hasRecordsColumn = false;
            //if (list.Fields.ContainsField("_vti_ItemDeclaredRecord"))
            //{
            //    hasRecordsColumn = true;
            //}
            //List<CAMLManager> cms = new List<CAMLManager>();
            //if (termIds.Count < QueryConditionMaxCount)
            //{
            //    CAMLManager cm = InitCamlQuery(list, list.Fields, mmsField, termIds,!hasRecordsColumn);
            //    if (cm != null)
            //    {
            //        cms.Add(cm);
            //    }
            //}
            //else
            //{
            //    int index = 0;
            //    while (termIds.Skip(index).Take(QueryConditionMaxCount) != null && termIds.Skip(index).Take(QueryConditionMaxCount).Count() != 0)
            //    {
            //        var queryIds = termIds.Skip(index).Take(QueryConditionMaxCount).ToList();
            //        index += QueryConditionMaxCount;
            //        if (queryIds.Count() != 0)
            //        {
            //            CAMLManager cm = InitCamlQuery(list, list.Fields, mmsField, queryIds,!hasRecordsColumn);
            //            if (cm != null)
            //            {
            //                cms.Add(cm);
            //            }
            //        }
            //    }
            //}
            //#endregion
            //if (cms.Count != 0)
            //{
            //    var discoverFolders = CommonUtility.GetAllFolders(list, QueryConditionMaxCount);
            //    logger.Info("The folder count:" + discoverFolders.Count);
            //    foreach (var discoverFolder in discoverFolders)
            //    {
            //        foreach (CAMLManager cm in cms)
            //        {
            //            AveCamlQuery query = new AveCamlQuery();
            //            cm.ScopeType = Types.ScopeTypes.Default;
            //            cm.RowLimit = QueryConditionMaxCount;
            //            string queryXml = cm.GetFullCAML(false);
            //            query.ViewXml = queryXml;
            //            query.DatesInUtc = true;
            //            query.FolderServerRelativeUrl = discoverFolder.ServerRelativeUrl;
            //            logger.Info($"Process Folder {discoverFolder.ServerRelativeUrl} query xml {queryXml}");
            //            IAveListItemCollection items = list.GetItems(query);
            //            logger.Debug(string.Format("Folder name:{0}, item's count:{1}", discoverFolder.Name, items.Count));
            //            foreach (var item in items)
            //            {
            //                ProcessItem(item, mmsField.InternalName);
            //            }
            //            while (items.ListItemCollectionPosition != null)
            //            {
            //                query.ListItemCollectionPosition = items.ListItemCollectionPosition;
            //                logger.Info($"query list item collection position {query.ListItemCollectionPosition}");
            //                items = list.GetItems(query);
            //                foreach (var item in items)
            //                {
            //                    ProcessItem(item, mmsField.InternalName);
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
            ProcessList(list);
            DisposeSPObj(list);
            base.ProcessList(discoverList);
        }
        public override void ProcessList(IAveList list)
        {
            ProgressService.IncreaseBase(1);
            logger.Info($"Process list {list.Title.LogBase64()}.");
            //IAveList list = discoverList.GetListObject();
            //if (list.BaseType != AveBaseType.DocumentLibrary)
            //{
            //    logger.Info($"Skip all other list type {list.BaseType} :{list.Title}");
            //    return;
            //}
            //if (IsLibraryInExcludeList(ConfigSiteSetting.ExcludeList, list))
            //{
            //    SendReport(list.Title, WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), KeepDataOption.DeclareRecord.ToString(), "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "RM_SS_SkipExcludeList");
            //    return;
            //}
            lock (LockObj)
            {
                try
                {
                    if (!TimeZones.ContainsKey(list.ParentWeb.ID))
                    {
                        TimeZones.Add(list.ParentWeb.ID, list.ParentWeb.RegionalSettings.TimeZone);
                    }
                }
                catch (Exception e)
                {
                    logger.Info($"Init time zone failed {e.ToString()}.");
                }
            }
            IAveTaxonomyField mmsField = GetTaxonomyField(list.Fields, BCSColumnName);
            if (mmsField == null)
            {
                logger.Info($"Current list not config bcs column: {list.Title.LogBase64()}.BCSColumnName:{BCSColumnName.LogBase64()}.");
                return;
            }
            var BCSColumnInternalName = mmsField.InternalName;
            var termIds = GetTermIds(mmsField);
            #region init Camel Query
            List<CAMLManager> cms = new List<CAMLManager>();
            if (termIds.Count < QueryConditionMaxCount)
            {
                CAMLManager cm = InitCamlQuery(list, list.Fields, mmsField, termIds, true);
                if (cm != null)
                {
                    cms.Add(cm);
                }
            }
            else
            {
                int index = 0;
                while (termIds.Skip(index).Take(QueryConditionMaxCount) != null && termIds.Skip(index).Take(QueryConditionMaxCount).Count() != 0)
                {
                    var queryIds = termIds.Skip(index).Take(QueryConditionMaxCount).ToList();
                    index += QueryConditionMaxCount;
                    if (queryIds.Count() != 0)
                    {
                        CAMLManager cm = InitCamlQuery(list, list.Fields, mmsField, queryIds, true);
                        if (cm != null)
                        {
                            cms.Add(cm);
                        }
                    }
                }
            }
            #endregion
            if (cms.Count != 0)
            {
                //var discoverFolders = CommonUtility.GetAllFolders(list, QueryConditionMaxCount); //Folder performance issue. remove it first ,Query Item by rootfolder.
                //logger.Info("The folder count:" + discoverFolders.Count);
                var azureTableRecords = GetAzureTableDataByListId(list.ID.ToString()).ToDictionary(r => r.Id);
                var explorerDBRecords = GetExplorerDataByFolder(list.ID.ToString(), list.ParentWeb.Site.ID.ToString()).ToDictionary(r => r.Id);
                int maxIndex = GetListItemMaxId(list.RootFolder);
                logger.Info($"Max index in library {list.Title.LogBase64()}:{maxIndex}.");
                //foreach (var discoverFolder in discoverFolders)
                //{
                foreach (CAMLManager cm in cms)
                {
                    AveCamlQuery query = new AveCamlQuery();

                    bool needQueryNext = false;
                    int nextIndex = 0;
                    int startIndex = 0;
                    int endIndex = QueryConditionMaxCount;
                    do
                    {
                        var tempCM = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                        tempCM.QueryGroup = cm.QueryGroup;
                        tempCM.QueryGroup.Conditions = new List<QueryCondition>();
                        AddRowLimitQueryCondition(tempCM, startIndex, endIndex);
                        string queryXml = tempCM.GetFullCAML(true);
                        query.ViewXml = queryXml;
                        query.DatesInUtc = true;
                        query.FolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                        logger.Info($"Process Folder {list.Title.LogBase64()} query xml {queryXml.LogBase64()}.");
                        IAveListItemCollection items = list.GetItemsForRecords(query);
                        logger.Debug(string.Format("Folder id:{0}, item's count:{1}.", list?.ID, items.Count));
                        if (ActionUseMultiThreads && items.Count > ThreadCount)
                        {
                            logger.Info($"Use multi thread. item count {items.Count}.");
                            var cts = new CancellationTokenSource();
                            RunMultiThreadsProcessItem(items, ThreadCount, cts, mmsField.InternalName, azureTableRecords, explorerDBRecords);
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                ProcessItem(item, mmsField.InternalName, azureTableRecords, explorerDBRecords);
                            }
                        }
                        //needQueryNext = items.Count > 0;
                        //if (needQueryNext)
                        //{
                        //    nextIndex = items.Max(i => i.ID);
                        //}
                        //startIndex = nextIndex;
                        if (startIndex + QueryConditionMaxCount < maxIndex)
                        {
                            needQueryNext = true;
                            startIndex = startIndex + QueryConditionMaxCount;
                        }
                        else
                        {
                            needQueryNext = false;
                        }
                        endIndex = startIndex + QueryConditionMaxCount;
                    }
                    while (needQueryNext);
                    //while (items.ListItemCollectionPosition != null)
                    //{
                    //    query.ListItemCollectionPosition = items.ListItemCollectionPosition;
                    //    logger.Info($"query list item collection position {query.ListItemCollectionPosition}");
                    //    items = list.GetItems(query);
                    //    foreach (var item in items)
                    //    {
                    //        ProcessItem(item, mmsField.InternalName);
                    //    }
                    //}
                }
                // }
            }
            else
            {
                logger.Error($"CamlQuery count is 0.List: {list.Title.LogBase64()}.");
            }
            DisposeSPObj(list);
            base.ProcessList(list);
        }

        public override void ProcessFolder(IAveFolder folder)
        {
            ProgressService.IncreaseBase(1);
            logger.Info($"Process folder {folder?.UniqueId}.");
            lock (LockObj)
            {
                try
                {
                    if (!TimeZones.ContainsKey(folder.ParentList.ParentWeb.ID))
                    {
                        TimeZones.Add(folder.ParentList.ParentWeb.ID, folder.ParentList.ParentWeb.RegionalSettings.TimeZone);
                    }
                }
                catch (Exception e)
                {
                    logger.Info($"Init time zone failed {e.ToString()}.");
                }
            }
            IAveTaxonomyField mmsField = GetTaxonomyField(folder.ParentList.Fields, BCSColumnName);
            if (mmsField == null)
            {
                logger.Info($"Current folder not config bcs column: {folder?.UniqueId}.BCSColumnName:{BCSColumnName.LogBase64()}.");
                return;
            }
            var BCSColumnInternalName = mmsField.InternalName;
            var termIds = GetTermIds(mmsField);
            #region init Camel Query
            List<CAMLManager> cms = new List<CAMLManager>();
            if (termIds.Count < QueryConditionMaxCount)
            {
                CAMLManager cm = InitCamlQuery(folder.ParentList, folder.ParentList.Fields, mmsField, termIds, true);
                if (cm != null)
                {
                    cms.Add(cm);
                }
            }
            else
            {
                int index = 0;
                while (termIds.Skip(index).Take(QueryConditionMaxCount) != null && termIds.Skip(index).Take(QueryConditionMaxCount).Count() != 0)
                {
                    var queryIds = termIds.Skip(index).Take(QueryConditionMaxCount).ToList();
                    index += QueryConditionMaxCount;
                    if (queryIds.Count() != 0)
                    {
                        CAMLManager cm = InitCamlQuery(folder.ParentList, folder.ParentList.Fields, mmsField, queryIds, true);
                        if (cm != null)
                        {
                            cms.Add(cm);
                        }
                    }
                }
            }
            #endregion
            if (cms.Count != 0)
            {
                var azureTableRecords = GetAzureTableDataByListId(folder.ParentList.ID.ToString()).ToDictionary(r => r.Id);
                var explorerDBRecords = GetExplorerDataByFolder(folder.ParentList.ID.ToString(), folder.ParentList.ParentWeb.Site.ID.ToString()).ToDictionary(r => r.Id);
                int maxIndex = GetListItemMaxId(folder.ParentList.RootFolder);
                logger.Info($"Max index in library {folder.ParentList.Title.LogBase64()}:{maxIndex}.");
                foreach (CAMLManager cm in cms)
                {
                    AveCamlQuery query = new AveCamlQuery();

                    bool needQueryNext = false;
                    int startIndex = 0;
                    int endIndex = QueryConditionMaxCount;
                    do
                    {
                        var tempCM = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                        tempCM.QueryGroup = cm.QueryGroup;
                        tempCM.QueryGroup.Conditions = new List<QueryCondition>();
                        AddRowLimitQueryCondition(tempCM, startIndex, endIndex);
                        string queryXml = tempCM.GetFullCAML(true);
                        query.ViewXml = queryXml;
                        query.DatesInUtc = true;
                        query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                        logger.Info($"Process Folder {folder?.UniqueId} query xml {queryXml.LogBase64()}.");
                        IAveListItemCollection items = folder.ParentList.GetItemsForRecords(query);
                        logger.Debug(string.Format("Folder unique Id:{0}, item's count:{1}.", folder?.UniqueId, items.Count));
                        if (ActionUseMultiThreads && items.Count > ThreadCount)
                        {
                            logger.Info($"Use multi thread. item count {items.Count}.");
                            var cts = new CancellationTokenSource();
                            RunMultiThreadsProcessItem(items, ThreadCount, cts, mmsField.InternalName, azureTableRecords, explorerDBRecords);
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                ProcessItem(item, mmsField.InternalName, azureTableRecords, explorerDBRecords);
                            }
                        }
                        if (startIndex + QueryConditionMaxCount < maxIndex)
                        {
                            needQueryNext = true;
                            startIndex = startIndex + QueryConditionMaxCount;
                        }
                        else
                        {
                            needQueryNext = false;
                        }
                        endIndex = startIndex + QueryConditionMaxCount;
                    }
                    while (needQueryNext);
                }
            }
            else
            {
                logger.Error($"CamlQuery count is 0.List: {folder.Name.LogBase64()}.");
            }
            DisposeSPObj(folder);
            base.ProcessFolder(folder);
        }

        public override void ProcessItem(IAveListItem item, string columnInternalName, Dictionary<Guid, OnPremiseSPListCacheDto> azureTableRecords, Dictionary<Guid, OnPremiseSPListCacheDto> exploreDBRecords)
        {
            ProgressService.IncreaseBase(1);
            base.ProcessItem(item, columnInternalName, azureTableRecords, exploreDBRecords);
        }
        protected int GetListItemMaxId(IAveFolder folder)
        {
            AveCamlQuery query = new AveCamlQuery();

            query.ViewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Ascending='FALSE' Name='ID' /></OrderBy></Query><RowLimit>1</RowLimit></View>";

            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            var items = folder.ParentList.GetItemsForRecords(query);
            if (items.Count <= 0) return 0;
            int maxId = items[0].ID;
            return maxId;
        }
        protected void AddRowLimitQueryCondition(CAMLManager cm, int startIndex, int endIndex)
        {
            //cm.ScopeType = Types.ScopeTypes.Default;
            cm.RowLimit = QueryConditionMaxCount;
            cm.QueryGroup.Conditions.Add(new QueryCondition(
               Types.JoinTypes.And,
               Types.FieldRefTypes.Name,
               "ID",
               Types.FieldTypes.Number,
               Types.QueryTypes.Leq,
               endIndex.ToString(), false));
            cm.QueryGroup.Conditions.Add(new QueryCondition(
              Types.JoinTypes.And,
              Types.FieldRefTypes.Name,
              "ID",
              Types.FieldTypes.Number,
              Types.QueryTypes.Gt,
             startIndex.ToString(), false));
        }
        protected CAMLManager InitCamlQuery(IAveList list, IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds, bool includeRecords)
        {
            CAMLManager cm = new CAMLManager(Types.ScopeTypes.RecursiveAll);
            logger.Info("Begin to Deal TermIds , Count {0} , Time {1}", termIds.Count, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            foreach (var termId in termIds)
            {
                QueryGroup group = null;
                RMRuleItemCollection checkerColl = null;
                int wssid = 0;
                var isGetWssidOfTerm = false;
                isGetWssidOfTerm = GetWssidOfTerm(list, taxonomyField, termId, out wssid);
                if (TermAndRulesMapping.TryGetValue(termId, out checkerColl) && isGetWssidOfTerm)
                {
                    var groupFactory = new QueryGroupFactory(
                        checkerColl,
                        listFields,
                        TimeZones[list.ParentWeb.ID],
                        null,
                        RunJobUTCTime,
                        taxonomyField.InternalName,
                        wssid);
                    group = groupFactory.GetQueryGroupByRuleCheckerCollection(includeRecords);
                }

                if (group != null && (group.Conditions.Count != 0 || group.Groups.Count != 0))
                {
                    cm.QueryGroup.AddGroup(group);
                }
            }
            logger.Info("End Dealing TermIds , Count {0} , Time {1}", termIds.Count, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));

            if (cm.QueryGroup.Groups.Count > 0)
            {
                #region
                //cm.AddViewFields("Title");
                //cm.AddViewFields("FileRef");
                //cm.AddViewFields(BCSColumnInternalName);
                //cm.AddViewFields("Author");
                //cm.AddViewFields("Created");
                //cm.AddViewFields("Editor");
                //cm.AddViewFields("Modified");
                //cm.AddViewFields("_vti_ItemHoldRecordStatus");  //for check is record
                //cm.AddViewFields("REVIMLifecycleStatus");
                //cm.AddViewFields("REVIMHomeLocation");
                //cm.AddViewFields("REVIMBox");
                //cm.AddViewFields("REVIMCurrentlyHeldBy");
                //cm.AddViewFields("REVIMAvailability");
                #endregion
                return cm;
            }
            else
            {
                return null;
            }
        }
        public override bool Run()
        {
            bool result = false;
            try
            {
                //Multithread is need in site collection level???
                logger.Info($"Current Node Level: {CurrentNode.Level.ToString().LogBase64()} : URL: {CurrentNode.Url.LogBase64()}.");
                switch (CurrentNode.Level)
                {
                    case NodeLevel.SiteCollection:
                        ProcessSiteCollection(CurrentSiteColTreeNode);
                        break;
                    case NodeLevel.Site:
                        var aveSite = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
                        QueryConditionMaxCount = GetMaxItemsPerThrottledOperation(aveSite);
                        var discoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
                        var allDiscoverWebs = discoverSite.GetWebs();
                        var site = allDiscoverWebs[new Guid(CurrentNode.SPObjectId)];//TO DO Performance..
                        ProcessSite(site, true);
                        break;
                    case NodeLevel.List://No Need discover obj.
                        var aveSite1 = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
                        var web = aveSite1.OpenWeb(new Guid(CurrentNode.Parent.Parent.SPObjectId));
                        var list = web.GetList(new Guid(CurrentNode.SPObjectId));
                        ProcessList(list);
                        break;
                    case NodeLevel.Folder:
                        //当前无论是多层SubFolder还是多层SubSite，对应节点的NodeLevel都是Folder/Site，没有像DA一样递增的逻辑.
                        //if (CurrentNode.Level >= NodeLevel.Folder && CurrentNode.Level < NodeLevel.Item)
                        {
                            var aveSiteFolderLevel = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
                            var webFolderLevel = aveSiteFolderLevel.OpenWeb(new Guid(CurrentWebTreeNode.SPObjectId));
                            var listFolderLevel = webFolderLevel.GetList(new Guid(CurrentListTreeNode.SPObjectId));
                            var folder = listFolderLevel.GetFolder(CurrentNode.FullPath);
                            ProcessFolder(folder);
                        }
                        break;
                }
            }
            finally
            {
                try
                {
                    FinalOperationForBusinessLayer();
                    JobContext.Current.ApiClient.MoveOnPremiseSPItemsToStatic(ScopePath);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while moving onpremise SP record to static table. Error:{0}.", e.ToString());
                }
                result = base.Run();
            }
            return result;
        }

        private List<OnPremiseSPListCacheDto> GetAzureTableDataByListId(string listId)
        {
            using (new AgentPerformanceScope("OnPremiseSPDiscover.GetAzureDataByListId", $"OnPremiseSPDiscover.GetAzureDataByListId.ListId:{listId}", true))
            {
                List<OnPremiseSPListCacheDto> listRecords = new List<OnPremiseSPListCacheDto>();
                long sortTicks = DateTime.MinValue.Ticks;
                while (true)
                {
                    var data = JobContext.Current.ApiClient.GetOnPremiseSPAzureDataByListId(listId, ScopePath, sortTicks, ExternalUtil.TransferDataCount);
                    if (data != null && data.Count > 0)
                    {
                        listRecords.AddRange(data);
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                }
                logger.Info($"Azure Table Data count is: {listRecords.Count}.");
                return listRecords;
            }
        }

        private List<OnPremiseSPListCacheDto> GetExplorerDataByFolder(string listId, string scodeId)
        {
            using (new AgentPerformanceScope("OnPremiseSPDiscover.QueryExplorerFiles", $"OnPremiseSPDiscover.QueryExplorerFiles.ListId:{listId}", true))
            {
                List<OnPremiseSPListCacheDto> folderRecords = new List<OnPremiseSPListCacheDto>();
                long sortTicks = DateTime.MinValue.Ticks;
                while (true)
                {
                    var data = JobContext.Current.ApiClient.GetOnPremiseSPExplorerDataByListId(listId, scodeId, sortTicks, ExternalUtil.TransferDataCount);
                    if (data != null && data.Count > 0)
                    {
                        folderRecords.AddRange(data);
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                }
                return folderRecords;
            }
        }
    }
}
