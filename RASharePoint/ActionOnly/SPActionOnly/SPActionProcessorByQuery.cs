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
//using AvePoint.Adonis.Records.Object.ActionOnly;
//using AvePoint.Adonis.StorageOptimization.Common.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.Discover.Base;
//using AvePoint.RA.SharePoint.EnforceRetention.Common;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ActionOnly.SPActionOnly
{
    public class SPActionProcessorByQuery : BaseSPActionProcessor
    {
        protected int QueryConditionMaxCount = 2000;
        public SPActionProcessorByQuery() : base()
        {

        }
        public SPActionProcessorByQuery(SPTreeNodeDto current, List<Rule> recordsRule) : base(current, recordsRule)
        {

        }
        public override void ProcessSiteCollection(SPTreeNodeDto site)
        {
            ReportManager.IncreaseBase(1);
            IAveSite aveSite = null;
            AveDiscoverSite discoverSite = null;
            using (new RA.Common.PerformanceScope(string.Format("Process Site Collection")))
            {
                aveSite = ObjectModelFactory.CreateSite(site.FullPath);
                QueryConditionMaxCount = GetMaxItemsPerThrottledOperation(aveSite);
                discoverSite = new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive);
                var allDiscoverWebs = discoverSite.GetWebs().Values;
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
                        logger.Warn($"Process site collection failed {e.ToString()}");
                    }
                }
            }
            DisposeSPObj(aveSite);
            base.ProcessSiteCollection(site);
        }
        public override void ProcessSite(AveDiscoverWeb site)
        {
            ReportManager.IncreaseBase(1);
            logger.Info($"Process site {site.FullUrl}");
            if (IsInExcludeNodeList(site.AveWeb.ID))
            {
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
                    logger.Info($"Init time zone failed {e.ToString()}");
                }
            }
            foreach (var list in site.GetLists())
            {
                try
                {
                    if (IsInExcludeNodeList(list.Value.ListId))
                    {
                        continue;
                    }
                    ProcessList(list.Value);
                }
                catch (Exception e)
                {
                    JobHasErrorNode = true;
                    AddFailedDetail(list.Value, e.Message);
                    logger.Warn($"Process list failed {e.ToString()}");
                }
            }
            DisposeSPObj(site);
            base.ProcessSite(site);
        }
        public override void ProcessList(AveDiscoverList discoverList)
        {
            ReportManager.IncreaseBase(1);
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
                logger.Info($"Skip list type {discoverList.ServerTemplate} :{discoverList.Title} Can't get listobj");
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
            logger.Info($"Start to process list: {list.RootFolder.ServerRelativeUrl}");
            ReportManager.IncreaseBase(1);
            //if (IsSystemList(discoverList))
            //{
            //    return;
            //}
            //IAveList list = discoverList.GetListObject();
            if (list.BaseTemplate != AveListTemplateType.DocumentLibrary)
            {
                logger.Info($"Skip the list for the template of the list is not DocumentLibrary. Template: [{list.BaseTemplate}] Url:[{list.RootFolder.ServerRelativeUrl}]");
                return;
            }
            if (IsLibraryInExcludeList(ConfigSiteSetting.ExcludeList, list))
            {
                SendReport(list.Title, WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url), KeepDataOption.DeclareRecord.ToString(), "", Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "RM_SS_SkipExcludeList");
                return;
            }
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
                    logger.Info($"Init time zone failed {e.ToString()}");
                }
            }
            var hasM2CField = HasDateOfModified2CreationField(list);
            IAveTaxonomyField mmsField = GetTaxonomyField(list.Fields, BCSColumnName);
            if (mmsField == null)
            {
                logger.Info($"Current list not config bcs column {list.RootFolder.Url}");
                return;
            }
            var BCSColumnInternalName = mmsField.InternalName;
            var termIds = GetTermIds(mmsField);
            #region init Camel Query
            bool hasRecordsColumn = false;
            if (list.Fields.ContainsField("_vti_ItemDeclaredRecord"))
            {
                hasRecordsColumn = true;
            }
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
                int maxIndex = GetListItemMaxId(list.RootFolder);
                logger.Info($"Max index in library {list.RootFolder.Url}:{maxIndex}");
                //foreach (var discoverFolder in discoverFolders)
                //{
                foreach (CAMLManager cm in cms)
                {
                    if (hasM2CField && HasCreatedCondition(cm))
                    {
                        ProcessQuery(cm, list, mmsField, maxIndex, true, true);
                        logger.Info($"[Process query with ReclassDateOfModified2Creation is not null.]");
                        ProcessQuery(cm, list, mmsField, maxIndex, true, false);
                    }
                    else
                    {
                        ProcessQuery(cm, list, mmsField, maxIndex, false, false);
                    }
                }
            }
            DisposeSPObj(list);
            base.ProcessList(list);
        }

        private bool HasDateOfModified2CreationField(IAveList list)
        {
            return list.Fields.ContainsField(CSDFieldName.ReclassDateOfModified2Creation);
        }

        private bool HasCreatedCondition(CAMLManager cm)
        {
            var queryXml = cm.GetFullCAML(false);
            return System.Text.RegularExpressions.Regex.IsMatch(queryXml, @"Name=""Created""");
        }

        private T DeepClone<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return default(T);
            }
            var serializeString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializeString);
        }

        private void ProcessQuery(CAMLManager cm, IAveList list, IAveTaxonomyField mmsField, int maxIndex,
            bool AddCondition4ReclassDateOfModified2Creation, bool ReclassDateOfModified2CreationIsNull)
        {
            logger.Info($"Process query parameter: AddCondition4ReclassDateOfModified2Creation:[{AddCondition4ReclassDateOfModified2Creation}], ReclassDateOfModified2CreationIsNull:[{ReclassDateOfModified2CreationIsNull}]");
            bool checkRule = AddCondition4ReclassDateOfModified2Creation && !ReclassDateOfModified2CreationIsNull;
            AveCamlQuery query = new AveCamlQuery();
            bool needQueryNext = false;
            int nextIndex = 0;
            int startIndex = 0;
            int endIndex = QueryConditionMaxCount;
            bool rebuildTermCondition = AddCondition4ReclassDateOfModified2Creation && !ReclassDateOfModified2CreationIsNull;

            do
            {
                var tempCM = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                if (rebuildTermCondition)
                {
                    var termCondition = RebuildTermCondition(cm.QueryGroup, mmsField.InternalName);
                    tempCM.QueryGroup.AddGroup(new QueryGroup(Types.JoinTypes.And)
                    {
                        Conditions = new List<QueryCondition>() { termCondition }
                    });
                }
                else
                {
                    tempCM.QueryGroup = cm.QueryGroup;
                }
                tempCM.QueryGroup.Conditions = new List<QueryCondition>();
                AddRowLimitQueryCondition(tempCM, startIndex, endIndex);
                if (AddCondition4ReclassDateOfModified2Creation)
                {
                    AddCond4ReclassDateOfModified2Creation(tempCM, ReclassDateOfModified2CreationIsNull);
                }
                string queryXml = tempCM.GetFullCAML(true);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                query.FolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                logger.Info($"Process Folder {list.RootFolder.ServerRelativeUrl} query xml {queryXml}");
                IAveListItemCollection items = list.GetItemsForRecords(query);
                logger.Debug(string.Format("Folder name:{0}, item's count:{1}", list.RootFolder.Url, items.Count));
                if (items.Count > ItemsPerTask)
                {
                    logger.Info($"Use multi thread. item count {items.Count}");
                    var cts = new CancellationTokenSource();
                    RunMultiThreadsProcessItem(items, ItemsPerTask, cts, mmsField.InternalName, checkRule);
                }
                else
                {
                    foreach (var item in items)
                    {
                        ProcessItem(item, mmsField.InternalName, checkRule);
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

        public override void ProcessItem(IAveListItem item, string columnInternalName, bool checkRule)
        {
            ReportManager.IncreaseBase(1);
            base.ProcessItem(item, columnInternalName, checkRule);
        }

        /// <summary>
        /// 注意：这个方法有时获取出来的是folder的最大ID
        /// </summary>
        /// <returns></returns>
        protected string GetLastItemQueryXml()
        {
            string result = $@"<View Scope='RecursiveAll'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            logger.Info($"GetLastItemQueryXml:{result}");
            return result;
        }

        protected string GetLastFileQueryXml()
        {
            string result = $@"<View Scope='Recursive'>
                    <Query>
                        <OrderBy Override='TRUE'><FieldRef Name='ID' Ascending='FALSE'/></OrderBy>
                    </Query>
                    <RowLimit Paged='True'>1</RowLimit>
                </View>";
            logger.Info($"GetLastFileQueryXml:{result}");
            return result;
        }

        protected int InnerGetLastItemId(IAveFolder folder, string queryXml)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.LoadAllItems = false;
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ViewXml = queryXml;
            var itemCollection = folder.ParentList.GetItemsForRecords(query);
            var item = itemCollection.FirstOrDefault();
            return item != null ? item.ID : -1;
        }
        protected int GetListItemMaxId(IAveFolder folder)
        {
            //这个query有时获取出来的是folder的最大ID，不是所有item的最大ID，所以需要在后面，再取一次file的最大ID
            string lastItemQueryXml = GetLastItemQueryXml();
            int lastItemId = InnerGetLastItemId(folder, lastItemQueryXml);

            string fileQueryXml = GetLastFileQueryXml();//include file and item
            int maxFileId = InnerGetLastItemId(folder, fileQueryXml);
            return Math.Max(lastItemId, maxFileId);
        }

        protected void AddCond4ReclassDateOfModified2Creation(CAMLManager cm, bool valueIsNull)
        {
            cm.QueryGroup.Conditions.Add(new QueryCondition(
                                                Types.JoinTypes.And,
                                                Types.FieldRefTypes.Name,
                                                CSDFieldName.ReclassDateOfModified2Creation,
                                                Types.FieldTypes.DateTime,
                                                valueIsNull ? Types.QueryTypes.IsNull : Types.QueryTypes.IsNotNull,
                                                null,
                                                false));
        }


        private void GetWssIdsFromExistingQuery(QueryGroup querygroup, string internalName, ref List<int> wssIds)
        {
            foreach (var cond in querygroup.Conditions)
            {
                if (cond.Query.Field.Equals(internalName, StringComparison.OrdinalIgnoreCase))
                {
                    wssIds.AddRange(cond.Query.Values);
                }
            }

            foreach (var subGroup in querygroup.Groups)
            {
                GetWssIdsFromExistingQuery(subGroup, internalName, ref wssIds);
            }
        }

        private QueryCondition RebuildTermCondition(QueryGroup querygroup, string internalName)
        {
            List<int> existWssIds = new List<int>();
            GetWssIdsFromExistingQuery(querygroup, internalName, ref existWssIds);
            if (existWssIds.Count > 0)
            {
                return QueryConditionFactory.GetTaxonomyQueryCondition(internalName, existWssIds.ToArray(), Types.JoinTypes.And);
            }
            return new QueryCondition();
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
                        wssid,
                        false);
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
                logger.Info($"Current Node Level {CurrentNode.Level.ToString()} : URL {CurrentNode.Url}");
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
                        using (new RA.Common.PerformanceScope(string.Format("Get Discover site obj")))
                        {
                            var site = allDiscoverWebs[new Guid(CurrentNode.SPObjectId)];//TO DO Performance..
                            ProcessSite(site);
                        }
                        break;
                    case NodeLevel.List://No Need discover obj.
                        var aveSite1 = ObjectModelFactory.CreateSite(CurrentSiteColTreeNode.FullPath);
                        var web = aveSite1.OpenWeb(new Guid(CurrentNode.Parent.Parent.SPObjectId));
                        var list = web.GetList(new Guid(CurrentNode.SPObjectId));
                        ProcessList(list);
                        break;
                }
            }
            finally
            {
                result = base.Run();
            }
            return result;
        }
    }
}
