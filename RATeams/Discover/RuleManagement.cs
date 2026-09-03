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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System.Collections;
using System.Reflection;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace RATeams.Discover
{
    public class RuleManagement
    {
        #region private member

        private readonly RuleOrderCache mCache = new RuleOrderCache();
        private static RALogger _logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RuleCollection mRuleCollection;

        #endregion

        #region property
        public bool HasFolderCondition { get; private set; }
        public bool HasDocumentCondition { get; private set; }
        public bool HasAttachmentCondition { get; private set; }
        public bool HasDocVersionCondition { get; private set; }
        public bool HasItemVersionCondition { get; private set; }
        public bool HasItemCondition { get; private set; }
        public bool HasListCondition { get; private set; }
        public bool HasListFilterCondition { get; private set; }
        public bool HasSiteCondition { get; private set; }
        public bool HasSiteFilterCondition { get; private set; }
        public bool HasSiteCollectionCondition { get; private set; }
        public bool HasSiteCollectionFilterCondition { get; private set; }
        private int RuleLevelNumber { get; set; }
        private List<FilterPolicy> FilterPolicyCollection { get; set; }
        public bool IsCGArchiver { get; set; }
        public string CurrentRuleId { get; set; }
        public string ForceFitTeamsRuleID { get; set; }
        #endregion property

        #region public method

        public RuleManagement(Dictionary<int, Rule> Rules, string jobId = "")
        {
            var ruleCollection = new RuleCollection();
            ruleCollection.Rules = Rules;
            mRuleCollection = ruleCollection;
            //Set WrapperConfiguration.UseStubAccessTimeRule Value false ADO-117596
            WrapperConfiguration.UseStubAccessTimeRule = false;

            #region find all conditions type.

            if (mRuleCollection != null)
                foreach (var rule in mRuleCollection.Rules.Select(rulet => rulet.Value))
                {
                    if (!HasAttachmentCondition)
                    {
                        HasAttachmentCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Attachment) != null);
                    }
                    if (!HasDocumentCondition)
                    {
                        HasDocumentCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Document) != null);
                    }
                    if (!HasFolderCondition)
                    {
                        HasFolderCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Folder) != null);
                    }
                    if (!HasDocVersionCondition)
                    {
                        HasDocVersionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.DocumentVersion) != null);
                    }
                    if (!HasItemVersionCondition)
                    {
                        HasItemVersionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.ItemVersion) != null);
                    }
                    if (!HasItemCondition)
                    {
                        HasItemCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Item) != null);
                    }
                    if (!HasItemCondition)
                    {
                        HasItemCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Newsfeed) != null);
                    }
                    if (!HasListCondition)
                    {
                        HasListCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.List) != null);
                    }
                    if (!HasSiteCondition)
                    {
                        HasSiteCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Site) != null);
                    }
                    if (!HasSiteCollectionCondition)
                    {
                        HasSiteCollectionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.SiteCollection || tmp.Level == PolicyLevel.Teams) != null);
                    }
                    if (HasDocVersionCondition && HasAttachmentCondition && HasDocumentCondition
                        && HasSiteCollectionCondition && HasSiteCondition && HasListCondition && HasItemCondition && HasItemVersionCondition)
                    {
                        break;
                    }
                }
            if (HasItemCondition || HasAttachmentCondition || HasDocVersionCondition || HasItemVersionCondition || HasDocumentCondition)
            {//为判断是否有低级别rule
                RuleLevelNumber = (int)CacheNodeType.Item;
                MergeFilterPolicy();
                return;
            }
            if (HasFolderCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.Folder;
                MergeFilterPolicy();
                return;
            }
            if (HasListCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.List;
                MergeFilterPolicy();
                return;
            }
            if (HasSiteCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.Web;
                MergeFilterPolicy();
                return;
            }
            if (HasSiteCollectionCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.SiteCollection;
                MergeFilterPolicy();
                return;
            }
            #endregion
        }

        public RuleManagement(RuleCollection sheduleRuleCollection, string jobId = "") : this(sheduleRuleCollection.Rules, jobId)
        {
        }

        /*private Dictionary<PolicyLevel, string> GetAndOrExpress(Dictionary<PolicyLevel, string> andOrExpress, PolicyLevel policyLevel, int currentCount, int appendCount)
        {
            Dictionary<PolicyLevel, string> andOrValue = andOrExpress;
            if (andOrValue == null)
            {
                andOrValue = new Dictionary<PolicyLevel, string>();
                andOrValue.Add(policyLevel, "(1)");
                currentCount += 1;
                for (int i = 0; i < appendCount - 1; i++)
                {
                    currentCount++;
                    andOrValue[policyLevel] = "(" + andOrValue[policyLevel] + "and" + currentCount.ToString() + ")";
                }
            }
            else
            {
                for (int i = 0; i < appendCount; i++)
                {
                    currentCount++;
                    andOrValue[policyLevel] = "(" + andOrValue[policyLevel] + "and" + currentCount.ToString() + ")";
                }
            }
            return andOrValue;
        }*/

        public enum ActionType
        {
            BackUp_Action = 0,
            CreateStub_Action = 1,
            Declare_Action = 2,
            Delete_Action = 3,
            ExportOnly_Action = 4,
            RecordManager_Action = 5,
            Tag_Action = 6,
        }



        /// <summary>
        /// 用来检查一个文件是否符合Rule. // modify the discoveritem to IAveListItem  
        /// </summary>
        public Rule CheckItemCriteria(Guid docId, object oItem)
        {
            IAveListItem item = null;
            if (oItem is IAveListItem)
            {
                item = oItem as IAveListItem;
            }
            else if (oItem is AveDiscoverItem)
            {
                item = ((AveDiscoverItem)oItem).CurrentItem;
            }
            if (item == null)
            {
                return null;
            }
            ObjectInfoBase baseInfo = null;

            // var baseInfo = item.GetFilterObjectInfo(FilterPolicyCollection);
            using (PerformanceScope sc = new PerformanceScope("RuleManagement.getbaseinfo", "getbaseinfo", true))
            {
                if (item.ParentList.BaseType == AveBaseType.DocumentLibrary)
                {

                    List<FilterPolicy> docFilters = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document) && !t.Rule.GetType().Name.Equals("StubLastAccessTimeRule", StringComparison.OrdinalIgnoreCase)).ToList();
                    var documentInfoRe = new DocumentInfo();
                    baseInfo = FilterAnalyser.SetVersionAlwaysTrue(docFilters, CommonDocumentFilter(ref docFilters, item, documentInfoRe));
                    int accessTimeRuleCount = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document) && t.Rule.GetType().Name.Equals("StubLastAccessTimeRule", StringComparison.OrdinalIgnoreCase)).ToList().Count();
                    if (accessTimeRuleCount > 0)
                    {
                        var documentInfo = baseInfo as DocumentInfo;
                        documentInfo.StubLastAccessTime = GetItemAccessTime(item);
                    }

                    int activeTimeRuleCount = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document) && t.Rule.GetType().Name.Equals("StubLastActiveTimeRule", StringComparison.OrdinalIgnoreCase)).ToList().Count();
                    if (activeTimeRuleCount > 0)
                    {
                        var documentInfo = baseInfo as DocumentInfo;
                        documentInfo.LastAccessCompatibleModifiedTime = GetItemAccessTime(item, true);
                    }
                }
                else
                {
                    List<FilterPolicy> itemFilters = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Item)).ToList();
                    baseInfo = FilterAnalyser.SetVersionAlwaysTrue(itemFilters, FilterAnalyser.GetItemFilterInfo(itemFilters, item));
                }
                ItemInfo itemInfo = baseInfo as ItemInfo;
                //mDebugLogger.LogToXml(string.Format("ItemInfo:{0}", item.LeafName), baseInfo);


                if (itemInfo != null)
                {
                    //Office365 APi Discussion Board and Survey List's Item title is null,we need to give it string.Empty
                    itemInfo.Name = itemInfo.Name ?? string.Empty;
                    itemInfo.Title = itemInfo.Title ?? string.Empty;
                }
            }
            Rule rs = CheckCriteria(baseInfo);
            //if (null != rs)
            //{
            //    mCache.AddCacheInfo(docId, rs);
            //}
            if (rs == null)
            {
                _logger.Info(string.Format("ItemInfo:{0} NotFitRuleItemInfoStart", docId) + SerializerHelper.SerializeToBase64StringUseJson(baseInfo) + "NotFitRuleItemInfoEnd");
            }
            else if ((rs.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
            {
                if (!((rs.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion))
                {
                    _logger.Info(string.Format("DeleteOnlyActionItemInfo:{0} DeleteOnlyActionStart", docId) + SerializerHelper.SerializeToBase64StringUseJson(baseInfo) + "DeleteOnlyActionEnd");
                }
                else
                {
                    _logger.Info("This DeleteOnly action include keep latest version,no need log fileInfo");
                }
            }
            return rs;
        }



        /// <summary>
        /// 当Version存在于 AllDocVersions表 中时, 调用这个函数进行检查这个Version是不是符合Document Version Rule.
        /// </summary>
        public Rule CheckItemVersionCriteria(Guid docId, object oItem, object oItemVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RuleManagement.CheckItemVersionCriteria"))
            {
                var item = oItem as AveDiscoverItem;
                var itemVersion = oItemVersion as AveVersionObject;
                if (item == null || itemVersion == null)
                {
                    return null;
                }

                var baseInfo = item.GetVersionObjectInfo(FilterPolicyCollection, itemVersion.Uiversion);

                //这个Version对应的当前版本的Rule的Order值
                var k = mCache.GetRuleOrder(docId);

                var versionInfo = baseInfo as VersionedObjectInfoBase;
                if (versionInfo != null && versionInfo.IsCurrentVersion)
                {
                    if (k == -1)
                    {
                        return null;
                    }

                    var rulet = mRuleCollection.Rules.Values.Where(r => r.Order == k).FirstOrDefault();
                    if (rulet != null)
                    {
                        return rulet;
                    }
                    else
                    {
                        return null;
                    }

                    //foreach (var rulet in mRuleCollection.Rules.Values.Where(rulet => rulet.Order == k))
                    //{
                    //    return rulet;
                    //}
                }

                var result = CheckCriteria(baseInfo, k);
                if (result == null)
                {
                    _logger.LogToXml(string.Format("ItemVersionInfo:{0}", item.LeafName), baseInfo);
                }
                return result;
            }
        }

        /// <summary>
        /// 检查Attachment是否符合Attachment Rule.
        /// </summary>
        public Rule CheckAttachmentCriteria(Guid docId, object oItem, object oAttachment)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RuleManagement.CheckAttachmentCriteria"))
            {
                var attachment = oAttachment as AveItemObject;
                if (oItem is AveDiscoverItem)
                {
                    var item = oItem as AveDiscoverItem;
                    if (attachment != null)
                    {
                        var baseInfo = item.GetFilterAttachmentInfo(FilterPolicyCollection, attachment.LeafName);
                        var k = mCache.GetRuleOrder(docId);
                        var result = CheckCriteria(baseInfo, k);
                        if (result == null)
                        {
                            _logger.LogToXml(string.Format("ItemAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                else if (oItem is AveDiscoverFolder)
                {
                    var item = oItem as AveDiscoverFolder;
                    if (attachment != null)
                    {
                        var baseInfo = item.GetFilterAttachmentInfo(FilterPolicyCollection, attachment.LeafName);
                        var result = CheckCriteria(baseInfo);
                        if (result == null)
                        {
                            _logger.LogToXml(string.Format("FolderAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                return null;
            }
        }
        public Rule CheckAttachmentCriteria(object oItem, object oAttachment)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RuleManagement.CheckAttachmentCriteria1"))
            {
                var attachment = oAttachment as AveItemObject;
                if (oItem is AveDiscoverItem)
                {
                    var item = oItem as AveDiscoverItem;
                    if (attachment != null)
                    {
                        var baseInfo = item.GetFilterAttachmentInfo(FilterPolicyCollection, attachment.LeafName);
                        var result = CheckCriteria(baseInfo);
                        if (result == null)
                        {
                            _logger.LogToXml(string.Format("ItemAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                else if (oItem is AveDiscoverFolder)
                {
                    var item = oItem as AveDiscoverFolder;
                    if (attachment != null)
                    {
                        var baseInfo = item.GetFilterAttachmentInfo(FilterPolicyCollection, attachment.LeafName);
                        var result = CheckCriteria(baseInfo);
                        if (result == null)
                        {
                            _logger.LogToXml(string.Format("FolderAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Modify the IAveDiscoverlist = > IAveList
        /// </summary>
        /// <param name="oList"> </param>
        /// <returns></returns>
        public Rule CheckListCriteria(object oList)
        {
            IAveList list = null;
            if (oList is IAveList)
            {
                list = oList as IAveList;
            }
            else if (oList is AveDiscoverList)
            {
                var disList = oList as AveDiscoverList;
                list = disList.GetListObject();
            }
            if (list == null)
            {
                return null;
            }
            var baseinfo = FilterAnalyser.GetListFilterInfo(FilterPolicyCollection, list);
            var rs = CheckCriteria(baseinfo);
            if (rs == null)
            {
                _logger.LogToXml("ListInfo:", baseinfo);
            }
            return rs;
        }
        //change IAveDiscoverFolder to IAveFolder
        public Rule CheckFolderCriteria(object oFolder, bool IsMicroFeedList = false)
        {
            Rule rule = null;
            IAveFolder folder = null;

            if (oFolder is IAveFolder)
            {
                folder = oFolder as IAveFolder;
            }
            else if (oFolder is AveDiscoverFolder)
            {
                var disFolder = oFolder as AveDiscoverFolder;
                folder = disFolder.AveFolder;
            }

            if (folder == null)
            {
                return null;
            }
            var baseInfo = FilterAnalyser.GetFolderFilterInfo(FilterPolicyCollection, folder);
            //  var baseInfo = folder.GetFilterObjectInfo(FilterPolicyCollection);
            //mDebugLogger.LogToXml(string.Format("FolderInfo : {0}", folder.LeafName), baseInfo);
            rule = CheckCriteria(baseInfo);
            if (rule != null && rule.PolicyLevel == PolicyLevel.Folder && IsMicroFeedList)
            {
                _logger.Info("Folder rule doesn't process MicroFeedList. Folder id:{0}.", folder.ID);
                rule = null;
            }
            if (rule == null)
            {
                _logger.LogToXml(string.Format("FolderInfo : {0}", folder.ServerRelativeUrl), baseInfo);
            }
            return rule;
        }

        //Check Or operation and list type policy All rules
        private bool CheckOrOperation()
        {
            try
            {
                bool hasOrOperation = false;
                bool hasNoListTypePolicy = false;
                foreach (var rule in mRuleCollection.Rules)
                {
                    foreach (var expression in rule.Value.AndOrExpression)
                    {
                        string temp = expression.Value;
                        if (!String.IsNullOrEmpty(temp))
                        {
                            if (temp.IndexOf("Or", StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                hasOrOperation = true;
                            }
                        }
                    }
                    bool hasListpolicy = false;
                    foreach (var filter in rule.Value.Filters)
                    {
                        string policyName = filter.Rule.GetType().Name;
                        policyName = policyName.Substring(policyName.LastIndexOf('.') + 1);
                        if (policyName.Equals("ListTypeRule", StringComparison.OrdinalIgnoreCase))
                        {
                            hasListpolicy = true;
                        }
                    }
                    if (!hasListpolicy)
                    {
                        hasNoListTypePolicy = true;
                    }
                }
                if (!hasOrOperation && !hasNoListTypePolicy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //mLog.Warn(LOGRESOURCEnew.SCURuleManagementCheckOrOperation + ex.ToString());
                return false;
            }
        }

        private bool CheckFilterListType(string listType, List<FilterPolicy> Filters)
        {
            foreach (var filter in Filters)
            {
                string policyName = filter.Rule.GetType().Name;
                policyName = policyName.Substring(policyName.LastIndexOf('.') + 1);
                if (policyName.Equals("ListTypeRule", StringComparison.OrdinalIgnoreCase))
                {
                    string value = filter.Value.Value1;
                    if ((filter.Condition == PolicyCondition.Exactly
                        && !listType.Equals(value, StringComparison.OrdinalIgnoreCase))
                        || (filter.Condition == PolicyCondition.IsExactlyNot
                        && listType.Equals(value, StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        //public bool CheckListType(object oList)
        //{
        //    var list = oList as IAveDiscoverList;
        //    var avelist = list.GetListObject();
        //    bool result = true;
        //    try
        //    {
        //        string listType = ((int)avelist.BaseTemplate).ToString();

        //        if (!CheckOrOperation())
        //        {
        //            return true;
        //        }

        //        bool allListTypeCheck = false;
        //        //if match check list type condition,will check list type rule.
        //        foreach (var rule in mRuleCollection.Rules)
        //        {
        //            if (CheckFilterListType(listType, rule.Value.Filters))
        //            {
        //                allListTypeCheck = true;
        //            }
        //        }
        //        if (!allListTypeCheck)
        //        {
        //            return false;
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Warn(LOGRESOURCEnew.SCURuleManagementCheckListType + ex.ToString());
        //        return true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name = "oWeb" > </ param >
        /// < returns ></ returns >
        public Rule CheckSiteCriteria(object oWeb)
        {
            IAveWeb web = null;

            if (oWeb is IAveWeb)
            {
                web = oWeb as IAveWeb;
            }
            else if (oWeb is AveDiscoverWeb)
            {
                var disWeb = oWeb as AveDiscoverWeb;
                web = disWeb.AveWeb;
            }

            if (web == null)
            {
                return null;
            }

            var baseInfo = FilterAnalyser.GetWebFilterInfo(FilterPolicyCollection, web);
            var rs = CheckCriteria(baseInfo);
            if (rs == null)
            {
                _logger.LogToXml("SiteInfo:", baseInfo);
            }
            return rs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oSitecollection"> </param>
        /// <returns></returns>
        public Rule CheckSiteCollectionCriteria(object oSitecollection)
        {
            IAveSite sitecollection = null;

            if (oSitecollection is IAveSite)
            {
                sitecollection = oSitecollection as IAveSite;
            }
            else if (oSitecollection is AveDiscoverSite)
            {
                var disSite = oSitecollection as AveDiscoverSite;
                sitecollection = disSite.Site;
            }

            if (sitecollection == null)
            {
                return null;
            }
            var baseInfo = FilterAnalyser.GetSiteFilterInfo(FilterPolicyCollection, sitecollection);
            var rs = CheckCriteria(baseInfo);
            if (rs == null)
            {
                _logger.LogToXml("SiteCollectionInfo:", baseInfo);
            }
            return rs;
        }

        public bool HaveCurrentLevelRule(int cacheNodeType)
        {
            if (cacheNodeType > (int)CacheNodeType.SiteCollection && cacheNodeType < (int)CacheNodeType.List)
            {
                //web
                return RuleLevelNumber == (int)CacheNodeType.Web;
            }
            if (cacheNodeType > (int)CacheNodeType.List && cacheNodeType < (int)CacheNodeType.Item)
            {
                //Folder
                return RuleLevelNumber == (int)CacheNodeType.Folder;
            }
            return RuleLevelNumber == cacheNodeType;
        }

        public bool HasLowerLevelRule(int cacheNodeType)
        {
            return cacheNodeType < RuleLevelNumber;
        }

        public Rule GetDueDisposalRule(IAveListItem aveItem, ref string dueDisposalTime)
        {
            DateTime resutlTime = DateTime.MinValue;
            Rule resultRule = null;
            PolicyLevel policyLevel = PolicyLevel.Item;

            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                policyLevel = PolicyLevel.Document;
            }
            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == policyLevel && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));
            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;

                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, aveItem);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckItemDueDateCriteria(aveItem.UniqueId, aveItem, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, aveItem);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }
        public Rule GetDueDisposalRule(IAveFolder aveFolder, ref string dueDisposalTime)
        {
            Rule resultRule = null;
            DateTime resutlTime = DateTime.MinValue;
            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == PolicyLevel.Folder && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));

            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;

                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, aveFolder);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckFolderDueDateCriteria(aveFolder, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, aveFolder);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }
        public Rule GetDueDisposalRule(IAveList aveList, ref string dueDisposalTime)
        {
            Rule resultRule = null;
            DateTime resutlTime = DateTime.MinValue;
            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => (r.PolicyLevel == PolicyLevel.List || r.PolicyLevel == PolicyLevel.Library)
                    && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));

            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;

                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, aveList);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckListDueDateCriteria(aveList, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, aveList);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }
        public Rule GetDueDisposalRule(IAveWeb aveWeb, ref string dueDisposalTime)
        {
            Rule resultRule = null;
            DateTime resutlTime = DateTime.MinValue;
            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == PolicyLevel.Site
                    && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));
            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;

                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, aveWeb);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckSiteDueDateCriteria(aveWeb, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, aveWeb);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }
        public Rule GetDueDisposalRule(IAveSite aveSite, ref string dueDisposalTime)
        {
            Rule resultRule = null;
            DateTime resutlTime = DateTime.MinValue;
            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == PolicyLevel.SiteCollection
                    && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));
            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;

                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, aveSite);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckSiteCollectionDueDateCriteria(aveSite, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, aveSite);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }

        public Rule CheckItemDueDateCriteria(Guid docId, object oItem, Rule rule)
        {
            var item = oItem as IAveListItem;
            if (item == null)
            {
                return null;
            }
            ObjectInfoBase baseInfo = null;

            if (item.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {

                List<FilterPolicy> docFilters = rule.Filters.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document) && !t.Rule.GetType().Name.Equals("AccessTimeRule", StringComparison.OrdinalIgnoreCase)).ToList();
                var documentInfoRe = new DocumentInfo();
                baseInfo = FilterAnalyser.SetVersionAlwaysTrue(docFilters, CommonDocumentFilter(ref docFilters, item, documentInfoRe));
                int accessTimeRuleCount = rule.Filters.AsQueryable().Where(t => t.Rule.GetType().Name.Equals("AccessTimeRule", StringComparison.OrdinalIgnoreCase)).ToList().Count();
                if (accessTimeRuleCount > 0)
                {
                    var documentInfo = baseInfo as DocumentInfo;
                    documentInfo.StubLastAccessTime = GetItemAccessTime(item); // DocumentFilter(item.File).StubLastAccessTime;
                }
                int activeTimeRuleCount = FilterPolicyCollection.AsQueryable().Where(t => t.Rule.GetType().Name.Equals("StubLastActiveTimeRule", StringComparison.OrdinalIgnoreCase)).ToList().Count();
                if (activeTimeRuleCount > 0)
                {
                    var documentInfo = baseInfo as DocumentInfo;
                    documentInfo.LastAccessCompatibleModifiedTime = GetItemAccessTime(item, true);
                }
            }
            else
            {
                List<FilterPolicy> itemFilters = rule.Filters.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Item)).ToList();
                baseInfo = FilterAnalyser.SetVersionAlwaysTrue(itemFilters, FilterAnalyser.GetItemFilterInfo(itemFilters, item));
            }
            ItemInfo itemInfo = baseInfo as ItemInfo;
            //mDebugLogger.LogToXml(string.Format("ItemInfo:{0}", item.LeafName), baseInfo);

            if (itemInfo != null)
            {
                //Office365 APi Discussion Board and Survey List's Item title is null,we need to give it string.Empty
                itemInfo.Name = itemInfo.Name ?? string.Empty;
                itemInfo.Title = itemInfo.Title ?? string.Empty;
            }
            //set time to min
            if (baseInfo is DocumentInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    DocumentInfo docInfo = baseInfo as DocumentInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        docInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        docInfo.Modified = DateTime.MinValue;
                    }
                    if (policy.Rule is StubLastAccessTimeRule)
                    {
                        docInfo.StubLastAccessTime = DateTime.MinValue;
                    }
                    if (policy.Rule is StubLastActiveTimeRule)
                    {
                        docInfo.LastAccessCompatibleModifiedTime = DateTime.MinValue;
                    }
                    #region custom column date
                    try
                    {
                        policy = rule.Filters.Where(f => f.Rule is ColumnDateTimeRule && f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                        if (policy != null)
                        {
                            var columnName = policy.Rule.Value1;
                            if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                            {
                                var internalName = columnName.Trim(new char[] { '[', ']' }).ToLowerInvariant();

                                if (docInfo.ColumnInfos.ContainsKey(internalName))
                                {
                                    docInfo.ColumnInfos[internalName] = DateTime.MinValue;
                                }

                            }
                            else
                            {
                                if (docInfo.ColumnInfos.ContainsKey(columnName))
                                {
                                    docInfo.ColumnInfos[columnName] = DateTime.MinValue;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("set custom date to min date error:{0}", ex.ToString());
                    }
                    #endregion
                }
            }
            if (baseInfo is ItemInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    ItemInfo docInfo = baseInfo as ItemInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        docInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        docInfo.Modified = DateTime.MinValue;
                    }
                    if (policy.Rule is StubLastAccessTimeRule)
                    {
                        docInfo.StubLastAccessTime = DateTime.MinValue;
                    }
                    if (policy.Rule is StubLastActiveTimeRule)
                    {
                        docInfo.LastAccessCompatibleModifiedTime = DateTime.MinValue;
                    }
                    #region custom column date
                    try
                    {
                        policy = rule.Filters.Where(f => f.Rule is ColumnDateTimeRule && f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                        if (policy != null)
                        {
                            var columnName = policy.Rule.Value1;
                            if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                            {
                                var internalName = columnName.Trim(new char[] { '[', ']' }).ToLowerInvariant();
                                if (docInfo.ColumnInfos.ContainsKey(internalName))
                                {
                                    docInfo.ColumnInfos[internalName] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                if (docInfo.ColumnInfos.ContainsKey(columnName))
                                {
                                    docInfo.ColumnInfos[columnName] = DateTime.MinValue;
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("set custom date to min date error:{0}", ex.ToString());
                    }
                    #endregion
                }
            }
            return CheckCurrentCriteria(baseInfo, rule);
        }
        public Rule CheckFolderDueDateCriteria(object oFolder, Rule rule, bool IsMicroFeedList = false)
        {
            Rule result = null;
            var folder = oFolder as IAveFolder;
            if (folder == null)
            {
                return null;
            }
            var baseInfo = FilterAnalyser.GetFolderFilterInfo(rule.Filters, folder);
            if (baseInfo is FolderInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    var fInfo = baseInfo as FolderInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        fInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        fInfo.Modified = DateTime.MinValue;
                    }
                    //fInfo.AccessTime = DateTime.MinValue;
                    #region custom column date
                    try
                    {
                        policy = rule.Filters.Where(f => f.Rule is ColumnDateTimeRule && f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                        if (policy != null)
                        {
                            var columnName = policy.Rule.Value1;
                            if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                            {
                                var internalName = columnName.Trim(new char[] { '[', ']' }).ToLowerInvariant();
                                if (fInfo.ColumnInfos.ContainsKey(internalName))
                                {
                                    fInfo.ColumnInfos[internalName] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                if (fInfo.ColumnInfos.ContainsKey(columnName))
                                {
                                    fInfo.ColumnInfos[columnName] = DateTime.MinValue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("set custom date to min date error:{0}", ex.ToString());
                    }
                    #endregion
                }
            }
            //  var baseInfo = folder.GetFilterObjectInfo(FilterPolicyCollection);
            //mDebugLogger.LogToXml(string.Format("FolderInfo : {0}", folder.LeafName), baseInfo);
            result = CheckCurrentCriteria(baseInfo, rule);
            if (result != null && result.PolicyLevel == PolicyLevel.Folder && IsMicroFeedList)
            {
                _logger.Info("Folder rule doesn't process MicroFeedList. Folder id:{0}.", folder.ID);
                result = null;
            }
            return result;
        }
        public Rule CheckListDueDateCriteria(object oList, Rule rule)
        {
            var list = oList as IAveList;
            if (list == null)
            {
                return null;
            }
            var baseinfo = FilterAnalyser.GetListFilterInfo(rule.Filters, list);
            if (baseinfo is ListInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    var fInfo = baseinfo as ListInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        fInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        fInfo.Modified = DateTime.MinValue;
                    }
                    //fInfo.AccessTime = DateTime.MinValue;
                }
            }
            return CheckCurrentCriteria(baseinfo, rule);
        }
        public Rule CheckSiteDueDateCriteria(object oWeb, Rule rule)
        {
            var web = oWeb as IAveWeb;
            if (web == null)
            {
                return null;
            }
            var baseInfo = FilterAnalyser.GetWebFilterInfo(rule.Filters, web);
            if (baseInfo is SiteInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    var fInfo = baseInfo as SiteInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        fInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        fInfo.Modified = DateTime.MinValue;
                    }
                    //fInfo.AccessTime = DateTime.MinValue;
                }
            }
            return CheckCurrentCriteria(baseInfo, rule);
        }
        public Rule CheckSiteCollectionDueDateCriteria(object oSitecollection, Rule rule)
        {
            var sitecollection = oSitecollection as IAveSite;
            if (sitecollection == null)
            {
                return null;
            }
            var baseInfo = FilterAnalyser.GetSiteFilterInfo(rule.Filters, sitecollection);
            if (baseInfo is SiteCollectionInfo)
            {
                var policy = rule.Filters.Where(f => f.Condition == PolicyCondition.OlderThan).FirstOrDefault();
                if (policy != null)
                {
                    var fInfo = baseInfo as SiteCollectionInfo;
                    if (policy.Rule is CreatedRule)
                    {
                        fInfo.Created = DateTime.MinValue;
                    }
                    if (policy.Rule is ModifiedRule)
                    {
                        fInfo.Modified = DateTime.MinValue;
                    }
                    //fInfo.AccessTime = DateTime.MinValue;
                }
            }
            return CheckCurrentCriteria(baseInfo, rule);
        }
        #endregion

        #region private method
        private DateTime GetDueDate(FilterPolicy filter, IAveListItem aveItem)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
            {

                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = (DateTime)aveItem["Modified"];//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = (DateTime)aveItem["Created"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                }
                else if (filter.Rule is StubLastAccessTimeRule)
                {
                    timeValue = RuleManagement.GetItemAccessTime(aveItem);
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                }
                else if (filter.Rule is StubLastActiveTimeRule)
                {
                    timeValue = RuleManagement.GetItemAccessTime(aveItem, isCompatibleByModifiedTime: true);
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                }
                else if (filter.Rule is ColumnDateTimeRule)
                {
                    try
                    {
                        timeValue = (DateTime)aveItem[filter.Rule.Value1];
                        timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("no such column {0}:{1}:{2}", aveItem.ID, filter.Rule.Value1, e.ToString());
                        return timeValue;
                    }
                }

                #endregion
                //the forecase only work for older than condition
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }

                    #endregion

                }
            }
            return timeValue;
        }
        private DateTime GetDueDate(FilterPolicy filter, IAveFolder aveFolder)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule) // || filter.Rule is AccessTimeRule)
            {

                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = (DateTime)aveFolder.Item["Modified"];//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = (DateTime)aveFolder.Item["Created"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                }
                else if (filter.Rule is ColumnDateTimeRule)
                {
                    try
                    {
                        timeValue = (DateTime)aveFolder.Item[filter.Rule.Value1];
                        timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("no such column {0}:{1}:{2}", aveFolder.ID, filter.Rule.Value1, e.ToString());
                        return timeValue;
                    }
                }
                #endregion
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }

            }
            return timeValue;

        }
        private DateTime GetDueDate(FilterPolicy filter, IAveList aveList)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                   || filter.Rule is ColumnDateTimeRule) // || filter.Rule is AccessTimeRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = (DateTime)aveList.LastItemModifiedDate;//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveList.ParentWeb);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = (DateTime)aveList.Created;
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveList.ParentWeb);
                }
                else if (filter.Rule is CustomPropertyDateTimeRule)
                {

                    try
                    {
                        timeValue = (DateTime)aveList.RootFolder.Properties[filter.Rule.Value1];
                        timeValue = ToUniversalTimeWithTimeZone(timeValue, aveList.ParentWeb);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("no such property {0}:{1}:{2}", aveList.RootFolder.ServerRelativeUrl, filter.Rule.Value1, e.ToString());
                        return timeValue;
                    }
                }

                #endregion
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }

            }
            return timeValue;

        }
        private DateTime GetDueDate(FilterPolicy filter, IAveWeb aveWeb)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule) // || filter.Rule is AccessTimeRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = (DateTime)aveWeb.LastItemModifiedDate;//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveWeb);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = (DateTime)aveWeb.Created;
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveWeb);
                }
                else if (filter.Rule is CustomPropertyDateTimeRule)
                {

                    try
                    {
                        timeValue = (DateTime)aveWeb.AllProperties[filter.Rule.Value1];
                        timeValue = ToUniversalTimeWithTimeZone(timeValue, aveWeb);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("no such property {0}:{1}:{2}", aveWeb.Url, filter.Rule.Value1, e.ToString());
                        return timeValue;
                    }
                }

                #endregion
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }
            }

            return timeValue;

        }
        private DateTime GetDueDate(FilterPolicy filter, IAveSite aveSite)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule) // || filter.Rule is AccessTimeRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = (DateTime)aveSite.RootWeb.LastItemModifiedDate;//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveSite.RootWeb);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = (DateTime)aveSite.RootWeb.Created;
                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveSite.RootWeb);
                }
                else if (filter.Rule is CustomPropertyDateTimeRule)
                {

                    try
                    {
                        timeValue = (DateTime)aveSite.RootWeb.AllProperties[filter.Rule.Value1];
                        timeValue = ToUniversalTimeWithTimeZone(timeValue, aveSite.RootWeb);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("no such property {0}:{1}:{2}", aveSite.RootWeb.Url, filter.Rule.Value1, e.ToString());
                        return timeValue;
                    }
                }

                #endregion
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }

            }

            return timeValue;

        }

        private ObjectInfoBase CommonDocumentFilter(ref List<FilterPolicy> policies, IAveListItem item, DocumentInfo result)
        {
            var documentPolicies = CreateDocumentFiltersCopy(policies, PolicyLevel.Document);
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.Document);
            foreach (FilterPolicy policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                ArgumentCheck.CheckNotNull(item);
                switch (ruleName)
                {
                    case "SizeRule":
                        result.Size = Convert.ToInt64(item?.FieldValues["File_x0020_Size"]);//debug
                        break;
                    case "NameRule":
                        try
                        {
                            result.Name = item?.FieldValues["FileLeafRef"].ToString();
                        }
                        catch (ArgumentException)
                        {
                            result.Name = string.Empty;
                        }
                        break;
                    case "RetentionLabelRule":
                        result.ColumnInfosOfInternalName = GetItemInternalColumns(item)[1];
                        break;
                    case "UrlRule":
                        if (item != null)
                        {
                            result.Url = item.ParentList.ParentWeb.Url + "/" + item.Url;//debug....
                        }
                        //else
                        //{

                        //    result.Url = file.ParentFolder.ParentWeb.Url + "/" + file.Url;
                        //}
                        break;
                    case "ModifiedRule":
                        //systemFile.Item为空
                        var fileModfied = item?["Modified"];//item == null ? file.TimeLastModified : item["Modified"];
                        result.Modified = ToUniversalTimeWithTimeZone((DateTime)fileModfied, item?.ParentList.ParentWeb);
                        break;
                    case "CreatedRule":
                        var fileCreated = item?["Created"];//item == null ? file.TimeCreated : item["Created"];
                        result.Created = ToUniversalTimeWithTimeZone((DateTime)fileCreated, item?.ParentList.ParentWeb);
                        break;
                    case "ModifiedByRule":
                        GetAuthorOrEditorInfo(item, result, false);
                        break;
                    case "CreatedByRule":
                        GetAuthorOrEditorInfo(item, result, true);
                        break;


                    case "ContentTypeRule":
                    case "ContentTypeNameRule":
                    case "CustomContentTypeRule":
                        if (item != null && item.ContentType != null)
                        {
                            result.ContentType = item.ContentType.Name;
                        }
                        else
                        {
                            result.ContentType = "Document";
                        }
                        break;
                    //case "ContentTypeIdRule":
                    //    if (item != null && item.ContentType != null)
                    //    {
                    //        result.ContentTypeId = item.ContentType.ID.ToString();
                    //    }
                    //    else
                    //    {
                    //        result.ContentTypeId = "0x01010072635879AE55BF4AA70560362FF4ABF8";//Document
                    //    }
                    //    break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                    case "MetadataTextColumnRule":
                    case "MetadataNumberColumnRule":
                        if (result.ColumnInfos == null)
                        {
                            result.ColumnInfos = GetItemColumns(item);
                        }
                        if (result.ColumnInfosOfInternalName == null)
                        {
                            result.ColumnInfosOfInternalName = GetItemInternalColumns(item)[1];
                        }
                        foreach (var documentPolicy in documentPolicies)
                        {
                            string documentRuleName = documentPolicy.Rule.GetType().Name;
                            documentRuleName = documentRuleName.Substring(documentRuleName.LastIndexOf('.') + 1);
                            if (documentRuleName.EqualIgnoreCase("ColumnBooleanRule") || documentRuleName.EqualIgnoreCase("ColumnTextRule"))
                            {
                                if (result.ListColumnExistInfos == null)
                                {
                                    result.ListColumnExistInfos = new Dictionary<string, bool>();
                                }
                                if (item.ParentList.Fields != null && item.ParentList.Fields.ContainsField(documentPolicy.Rule.Value1))
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = true;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, true);
                                    }
                                }
                                else
                                {
                                    if (result.ListColumnExistInfos.ContainsKey(documentPolicy.Rule.Value1))
                                    {
                                        result.ListColumnExistInfos[documentPolicy.Rule.Value1] = false;
                                    }
                                    else
                                    {
                                        result.ListColumnExistInfos.Add(documentPolicy.Rule.Value1, false);
                                    }
                                }
                            }
                        }
                        break;

                    //case "CustomPropertyTextRule":
                    //case "CustomPropertyNumberRule":
                    //case "CustomPropertyDateTimeRule":
                    //case "CustomPropertyBooleanRule":
                    //    result.ColumnInfosOfDisplayName = GetCustomPropertyInfo(result.ColumnInfosOfDisplayName, item.Properties);
                    //    result.ColumnInfosOfInternalName = GetCustomPropertyInfo(result.ColumnInfosOfInternalName, item.Properties);
                    //    break;

                    case "ListTypeRule":
                        if (item != null)
                        {
                            result.ListType = ((int)item.ParentList.BaseTemplate).ToString();
                        }

                        break;
                    case "StubLastAccessTimeRule":
                    case "StubLastActiveTimeRule":
                        break;
                    //case "AccessTimeRule":

                    //    CheckAccessTimeRuleStatus(file.Web.Site);
                    //    string listId = file.ParentFolder.ParentList != null ? file.ParentFolder.ParentList.ID.ToString() : null;
                    //    DateTime modfied = item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                    //    modfied = ToUniversalTimeWithTimeZone(modfied, file.Web);
                    //    result.AccessTime = GetAccessTime(file.Web.Site, listId, file.UniqueId.ToString(), modfied);

                    //    break;
                    case "ParentFolderNameRule":

                        result.ParentFolderName = item?.File.ParentFolder.Name;

                        //RECO-10748 This is modified according to the code of DAOL
                        //if (item.ParentList.RootFolder.Name == item.File.ParentFolder.Name)
                        //{
                        //    result.ParentFolderName = item.ParentList.Title;
                        //}
                        break;

                    case "ParentFolderNameHeirarchicallyRule":
                        try
                        {
                            ArgumentCheck.CheckNotNull(item);
                            var rootFolderPath = item.ParentList.ParentWeb.ServerRelativeUrl;
                            var itemPath = item["FileRef"].ToString();
                            var folderPath = string.Empty;
                            var fileIndex = itemPath.LastIndexOf('/');
                            if (fileIndex >= 0)
                            {
                                folderPath = itemPath[..fileIndex];
                            }
                            string folderNameHeirarchically = string.Empty;
                            if (rootFolderPath.Length + 1 < folderPath.Length)
                            {
                                folderNameHeirarchically = folderPath.Substring(rootFolderPath.Length + 1);
                            }
                            else
                            {
                                var dirindex = folderPath.LastIndexOf('/'); //itemProperty.ContainsKey("DirName") ? itemProperty["DirName"].ToString() : (itemProperty.ContainsKey("FileDirRef") ? itemProperty["FileDirRef"].ToString() : string.Empty);
                                if (dirindex >= 0)
                                {
                                    folderNameHeirarchically = folderPath.Substring(dirindex + 1);
                                }
                            }
                            result.ParentFolderIncludingName = folderNameHeirarchically;
                        }
                        catch (Exception e)
                        {
                            _logger.Info($"use file object to init name path property {e}");
                            //use web url is because the ParentFolderNameRule will set list url name to folder name, so keep this logic.
                            var rootFolderPath = item.File.ParentFolder.ParentList.ParentWeb.ServerRelativeUrl;
                            var folderPath = item.File.ServerRelativeUrl;

                            string folderNameHeirarchically = string.Empty;
                            if (rootFolderPath.Length + 1 < folderPath.Length)
                            {
                                folderNameHeirarchically = folderPath.Substring(rootFolderPath.Length + 1);
                            }
                            else
                            {
                                var dirindex = item.File.ServerRelativeUrl.LastIndexOf('/');
                                if (dirindex >= 0)
                                {
                                    folderNameHeirarchically = item.File.ServerRelativeUrl.Substring(dirindex + 1);
                                }
                            }
                            result.ParentFolderIncludingName = folderNameHeirarchically;
                        }
                        break;
                    case "ParentListNameRule":
                        string listName = item?.ParentList.Title;
                        result.ParentListName = listName;
                        break;
                    case "SensitivityLabelRule":
                        result.SensitivityLabel = string.Empty;
                        if (item?.FieldValues != null)
                        {
                            //if (item.FieldValues.ContainsKey(SPColumnConstants.Sensitive_Label_Id))
                            //{
                            //    string ipLabel = (item.FieldValues[SPColumnConstants.Sensitive_Label_Id] as string) ?? string.Empty;
                            //    string sensitiveLabelNameKey = string.Format(SPColumnConstants.SP_Sensitive_Name, ipLabel);
                            //    if (!string.IsNullOrEmpty(ipLabel) && item?.Properties != null && item.Properties.ContainsKey(sensitiveLabelNameKey))
                            //    {
                            //        result.SensitivityLabel = (item.Properties[sensitiveLabelNameKey] as string) ?? string.Empty;
                            //        break;
                            //    }
                            //}
                            if (item.FieldValues.ContainsKey(SPColumnConstants.Sensitive_Label_Display_Name))
                            {
                                var labelDispalyName = (item.FieldValues[SPColumnConstants.Sensitive_Label_Display_Name] as string) ?? string.Empty;
                                try
                                {
                                    var splitLabelDisplayName = labelDispalyName.Split("\\");
                                    result.SensitivityLabel = splitLabelDisplayName.Length > 0 ? splitLabelDisplayName[splitLabelDisplayName.Length - 1].TrimStart() : string.Empty;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Info($"Get sensitive label display name have some issue {ex}");
                                    result.SensitivityLabel = labelDispalyName;
                                }
                            }
                        }
                        break;
                    case "SensitivityLabelFullNameRule":
                        result.SensitivityLabelFullName = string.Empty;
                        if (item?.FieldValues != null)
                        {
                            if (item.FieldValues.ContainsKey(SPColumnConstants.Sensitive_Label_Display_Name))
                            {
                                var labelDispalyName = (item.FieldValues[SPColumnConstants.Sensitive_Label_Display_Name] as string) ?? string.Empty;
                                try
                                {
                                    result.SensitivityLabelFullName = labelDispalyName;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Info($"Get sensitive label full name have some issue {ex}");
                                    result.SensitivityLabelFullName = labelDispalyName;
                                }
                            }
                        }
                        break;
                    case "ParentLibraryTextRule":
                    case "ParentLibraryNumberRule":
                    case "ParentLibraryBooleanRule":
                    case "ParentLibraryDateTimeRule":
                        result.ParentListColumnInfos = item?.ParentList.RootFolder.Properties;
                        break;
                    case "ParentSiteCollectionTextRule":
                    case "ParentSiteCollectionNumberRule":
                    case "ParentSiteCollectionBooleanRule":
                    case "ParentSiteCollectionDateTimeRule":
                        result.ParentSiteCollectionColumnInfos = FilterAnalyser.FillSiteColumns(item?.Web.Site.RootWeb);
                        break;
                    case "PropertyBagTextRule":
                    case "PropertyBagNumberRule":
                    case "PropertyBagBooleanRule":
                    case "PropertyBagDateTimeRule":
                        if (result.ParentSiteColumnInfos == null)
                        {
                            result.ParentSiteColumnInfos = item?.ParentList.ParentWeb.AllProperties;
                        }
                        break;
                    default:
                        throw new Exception(string.Format("The rule:{0} is invalid", ruleName));
                }
            }
            return result;
        }

        private static Hashtable GetItemColumns(IAveListItem item)
        {
            Hashtable columnCollection = new Hashtable(StringComparer.OrdinalIgnoreCase);
            _logger.Info($"Process item {item.ID} {item.FieldValues.Count}");
            if (item != null)
            {
                string indexOutRangeColumns = string.Empty;
                string keyNotFoundColumns = string.Empty;
                foreach (var field in item.Fields)
                {
                    try
                    {
                        if (field.Hidden && !string.Equals(field.InternalName, "FileDirRef"))
                        {
                            continue;
                        }
                        if (string.Equals(field.InternalName, "FileDirRef"))
                        {
                            string dirName = item[field.ID].ToString();
                            if (dirName != string.Empty)
                            {
                                string folderName = dirName.TrimEnd('/').Substring(dirName.TrimEnd('/').LastIndexOf('/')).TrimStart('/');
                                columnCollection[field.InternalName.ToLower()] = folderName;//为符合Parent folder name需要做的特殊处理。 
                            }
                            continue;
                        }
                        if (item[field.ID] == null)
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Integer)
                            {//text match * need this.
                                columnCollection[field.Title.ToLower()] = string.Empty;
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            //在rule判断时，会判断数据类型。
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                                columnCollection[field.Title.ToLower()] = item[field.ID];
                                break;
                            case AveFieldType.DateTime:
                                columnCollection[field.Title.ToLower()] = ToUniversalTime((DateTime)item[field.ID]);
                                break;
                            case AveFieldType.User:
                                var value = item[field.ID];
                                var stringVlue = value as string;
                                if (!string.IsNullOrEmpty(stringVlue))
                                {
                                    columnCollection[field.Title.ToLower()] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                }
                                else if (value is IEnumerable)
                                {
                                    StringBuilder users = new StringBuilder();
                                    foreach (var userinfo in (value as IEnumerable))
                                    {
                                        var user = userinfo.ToString();
                                        users.Append(user.Substring(user.IndexOf('#') + 1));
                                        users.Append(';');
                                    }
                                    users.Length = Math.Max(0, users.Length - 1);
                                    columnCollection[field.Title.ToLower()] = users.ToString();
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = value;
                                }
                                break;
                            case AveFieldType.Lookup:
                                var lookupValue = item[field.ID];
                                var realValue = lookupValue as IAveFieldLookupValue;
                                if (realValue != null)
                                {
                                    columnCollection[field.Title.ToLower()] = realValue.LookupValue;
                                }
                                else if (lookupValue is string)
                                {
                                    var vaules = (lookupValue as string).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (vaules.Length == 2)
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[1];
                                    }
                                    else
                                    {
                                        columnCollection[field.Title.ToLower()] = vaules[0];
                                    }
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = lookupValue;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(item[field.ID]);
                                }
                                else
                                {
                                    columnCollection[field.Title.ToLower()] = item[field.ID];
                                }
                                break;
                            default:
                                //field.GetFieldValueAsText should not throw exception, if any modify the override method.(Luo Qinglong)
                                columnCollection[field.Title.ToLower()] = field.GetFieldValueAsText(item[field.ID]).Trim();
                                break;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        indexOutRangeColumns = indexOutRangeColumns + "#" + field.Title;
                    }
                    catch (KeyNotFoundException)
                    {
                        keyNotFoundColumns = indexOutRangeColumns + "#" + field.Title;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Get the metadata of item error.Field id:{field.Title} type {field.Type}.Exception:{ex.Message}");
                    }
                }
                if (!string.IsNullOrEmpty(indexOutRangeColumns) || !string.IsNullOrEmpty(keyNotFoundColumns))
                {
                    _logger.Info($"Data column exception Index Out Range Columns {indexOutRangeColumns} Data exception column Get the metadata of item error.Field Name:{keyNotFoundColumns}");
                }
            }
            return columnCollection;
        }
        private static List<Hashtable> GetItemInternalColumns(IAveListItem item)
        {
            Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable columnCollectionOfInterName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable intrToDisp = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable dispToType = new Hashtable(StringComparer.OrdinalIgnoreCase);
            Hashtable specialCollection = new Hashtable(StringComparer.OrdinalIgnoreCase);
            List<Hashtable> ret = new List<Hashtable>();
            if (item != null)
            {
                string indexOutRangeColumns = string.Empty;
                string keyNotFoundColumns = string.Empty;
                foreach (var field in item.Fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                        if (field.Hidden)
                        {
                            continue;
                        }
                        if (item[field.ID] == null)
                        {
                            if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                            {//text match * need this.                                
                                columnCollectionOfDisplayName[fieldTitle] = string.Empty;
                                columnCollectionOfInterName[fieldInternalName] = string.Empty;
                                intrToDisp[fieldInternalName] = fieldTitle;
                                dispToType[fieldTitle] = field.Type.ToString().ToLower(CultureInfo.InvariantCulture);
                            }
                            continue;
                        }
                        switch (field.Type)
                        {
                            //在rule判断时，会判断数据类型。
                            case AveFieldType.Boolean:
                            case AveFieldType.Number:
                                //columnCollection[fieldTitle] = item[field.ID];
                                columnCollectionOfDisplayName[fieldTitle] = item[field.ID];
                                columnCollectionOfInterName[fieldInternalName] = item[field.ID];
                                break;
                            case AveFieldType.Counter:
                                columnCollectionOfDisplayName[fieldTitle] = Convert.ToDouble(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = Convert.ToDouble(item[field.ID]);
                                break;
                            case AveFieldType.DateTime:
                                //columnCollection[fieldTitle] = ToUniversalTime((DateTime)item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.Web);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            case AveFieldType.User:
                                var value = item[field.ID];
                                var stringVlue = value as string;
                                if (stringVlue != null)
                                {
                                    //columnCollection[fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    columnCollectionOfDisplayName[fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else if (value is IEnumerable)
                                {
                                    StringBuilder users = new StringBuilder();
                                    foreach (var userinfo in (value as IEnumerable))
                                    {
                                        var user = userinfo.ToString();
                                        users.Append(user.Substring(user.IndexOf('#') + 1));
                                        users.Append(';');
                                    }
                                    users.Length = Math.Max(0, users.Length - 1);
                                    //columnCollection[fieldTitle] = users.ToString();
                                    columnCollectionOfDisplayName[fieldTitle] = users.ToString();
                                    columnCollectionOfInterName[fieldInternalName] = users.ToString();
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = value;
                                    columnCollectionOfDisplayName[fieldTitle] = value;
                                    columnCollectionOfInterName[fieldInternalName] = value;
                                }
                                break;
                            case AveFieldType.Lookup:
                                var lookupValue = item[field.ID];
                                DateTime lookupDateTime = DateTime.MinValue;
                                if (DateTime.TryParse(lookupValue.ToString(), out lookupDateTime))
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone(lookupDateTime, item.Web);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                    break;
                                }
                                var realValue = lookupValue as IAveFieldLookupValue;
                                if (realValue != null)
                                {
                                    //columnCollection[fieldTitle] = realValue.LookupValue;
                                    columnCollectionOfDisplayName[fieldTitle] = realValue.LookupValue;
                                    columnCollectionOfInterName[fieldInternalName] = realValue.LookupValue;
                                }
                                else if (string.Equals(field.TypeAsString, "Lookup", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(field.TypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(lookupValue);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = lookupValue;
                                    columnCollectionOfDisplayName[fieldTitle] = lookupValue;
                                    columnCollectionOfInterName[fieldInternalName] = lookupValue;
                                }
                                break;
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                }
                                else
                                {
                                    //columnCollection[fieldTitle] = item[field.ID];
                                    columnCollectionOfDisplayName[fieldTitle] = item[field.ID];
                                    columnCollectionOfInterName[fieldInternalName] = item[field.ID];
                                }
                                break;
                            case AveFieldType.ModStat:
                                specialCollection[fieldInternalName] = item[field.ID];
                                //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            case AveFieldType.Calculated:
                                var calculatedValue = item[field.ID];
                                var calValue = calculatedValue as IAveFieldCalculated;
                                if (calValue != null)
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = calValue.Formula;
                                }
                                else if (calculatedValue is string)
                                {
                                    var vaules = (calculatedValue as string).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (vaules.Length == 2)
                                    {
                                        string colValue = vaules[1];
                                        if ((field as IAveFieldCalculated).OutputType == AveFieldType.DateTime)
                                        {
                                            DateTime columnValue;
                                            if (DateTime.TryParse(colValue, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out columnValue))
                                            {
                                                columnCollectionOfDisplayName[fieldTitle] = ToUniversalTimeWithTimeZone(columnValue, item.Web);
                                            }
                                            else
                                            {
                                                columnCollectionOfDisplayName[fieldTitle] = colValue;
                                            }
                                        }
                                        else if (vaules.Length == 1)//13环境Check In Comment在没有值得情况下为empty
                                        {
                                            columnCollectionOfDisplayName[fieldTitle] = vaules[0];
                                        }
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[fieldTitle] = calculatedValue;
                                    }
                                }
                                else if (calculatedValue is DateTime)
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = (DateTime)calculatedValue;
                                }
                                else
                                {
                                    columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                }
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                            default:
                                //field.GetFieldValueAsText should not throw exception, if any modify the override method.(Luo Qinglong)
                                //columnCollection[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfDisplayName[fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                columnCollectionOfInterName[fieldInternalName] = columnCollectionOfDisplayName[fieldTitle];
                                break;
                        }
                        //columnCollection[fieldTitle] = AveStringHelper.Trim(columnCollection[fieldTitle]);
                        columnCollectionOfDisplayName[fieldTitle] = AveStringHelper.Trim(columnCollectionOfDisplayName[fieldTitle]);
                        columnCollectionOfInterName[fieldInternalName] = AveStringHelper.Trim(columnCollectionOfInterName[fieldInternalName]);
                        intrToDisp[fieldInternalName] = fieldTitle;
                        dispToType[fieldTitle] = field.Type.ToString().ToLower(CultureInfo.InvariantCulture);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        indexOutRangeColumns = indexOutRangeColumns + "#" + field.Title;
                    }
                    catch (KeyNotFoundException)
                    {
                        keyNotFoundColumns = keyNotFoundColumns + field.Title;
                    }
                    catch (Exception ex)
                    {
                        _logger.Info($"Get the metadata of item error.Field id:{field.Title} type {field.Type}.Exception:{ex.Message}");
                    }
                }
                if (!string.IsNullOrEmpty(indexOutRangeColumns) || !string.IsNullOrEmpty(keyNotFoundColumns))
                {
                    _logger.Info($"Internal Data column exception Index Out Range Columns {indexOutRangeColumns} Data exception column Get the metadata of item error.Field Name:{keyNotFoundColumns}");
                }
            }
            ret.Add(columnCollectionOfDisplayName);
            ret.Add(columnCollectionOfInterName);
            ret.Add(intrToDisp);
            ret.Add(dispToType);
            ret.Add(specialCollection);
            return ret;
        }

        private static DateTime ToUniversalTime(DateTime datetime)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = datetime.ToUniversalTime();
            }
            return datetime;
        }
        private void GetAuthorOrEditorInfo(IAveListItem item, CommonInfoBase result, bool authorOrEditor)
        {
            string logonName = string.Empty;
            string title = string.Empty;
            string email = string.Empty;
            string columnName = authorOrEditor ? "Author" : "Editor";
            GetUserInfo(item, columnName, ref logonName, ref title, ref email);
            if (authorOrEditor)
            {
                result.CreatedByTitle = title;
                result.CreatedByLogonName = logonName;
                result.CreateByEmail = email;
            }
            else
            {
                result.ModifiedByTitle = title;
                result.ModifiedByLogonName = logonName;
                result.ModifiedByEmail = email;
            }
        }

        private static void GetUserInfo(IAveListItem item, string columnName, ref string loginName, ref string title, ref string email)
        {

            string itemUserInfo = item[columnName].ToString();
            string[] sArray = itemUserInfo.Split(new Char[] { ';', '#' }, StringSplitOptions.RemoveEmptyEntries);
            title = sArray[1].ToString();
            IAveUser user = item.ParentList.ParentWeb.SiteUsers.GetByID(int.Parse(sArray[0].ToString()));
            if (user != null)
            {
                loginName = user.LoginName;
                title = user.Name;
                email = user.Email;
            }

        }
        private List<FilterPolicy> CreateDistinctFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();
            }
            return new List<FilterPolicy>();
        }

        private List<FilterPolicy> CreateDocumentFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).ToList();
            }
            return new List<FilterPolicy>();
        }

        public static DateTime GetItemAccessTime(IAveListItem item, bool isCompatibleByModifiedTime = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.GetItemAccessTimeRaTeams"))
            {
                //为了防止每个Item都要执行GetFile， 直接通过Item获取LAT, from CI for Cardinia
                CallContext.SetData("TenantGroupId", TenantLocalValue.LogonGroupId);
                var fileModfied = item["Modified"];//item == null ? file.TimeLastModified : item["Modified"];
                DateTime modified = ToUniversalTimeWithTimeZone((DateTime)fileModfied, item.ParentList.ParentWeb);
                DateTime result = item.GetLastAccessTime(item.UniqueId, item.ParentList.RootFolder.ServerRelativeUrl, modified, isCompatibleByModifiedTime);
                _logger.Info("Fetch last access time {0} on item {1}", result, item.UniqueId);
                return result;
                //var file = item.File;
                //var temp = DocumentFilter(file);
                //return temp.StubLastAccessTime;
            }
        }

        private Rule CheckCurrentCriteria(ObjectInfoBase info, Rule rulet, int thresholdRuleOrder = -1)
        {
            //在这里遍历Rule时, 应该考虑到Rule的Order, 如果能准确依赖于Manager发的Order的话, 这里可以不考虑Order, 否则
            //应该在遍历的时候考虑到Rule的Order.

            if (-1 == thresholdRuleOrder || rulet.Order < thresholdRuleOrder)
            {
                //如果一个Version的检查过程中， threshold不等于－1表示这个version的当前版本有对应的Rule,
                //因此，如果一个rulet.Order小于threshold时， 继续检查

                try
                {
                    //我们需要filter out模式
                    var engine = new FilterEngine(rulet.Filters, rulet.AndOrExpression, true);
                    if (engine.IsQualified(info))
                    {
                        return rulet;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        _logger.Error("A property was not assigned while checking a Teams rule. Exception:{0}", ex.ToString());
                    }
                    // mLog.Error(LOGRESOURCE.StorageOptimization13_SOARCOMCheckRuleManagementCriteria + ex);
                    //throw new Exception(LOGRESOURCE.StorageOptimization13_SOARCOMRuleManagementCheckCriteriaException + rulet.Compression);
                    throw new Exception(ex.ToString());
                }

            }
            else if (rulet.Order == thresholdRuleOrder)
            {
                return rulet;
            }

            return null;
        }

        private static DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                TimeZoneInfo webZone = GeneralSettingConfig.FindSystemTimeZoneById(AveTimeZoneUtility.ToTimeZoneInfoId(web.RegionalSettings.TimeZone.ID));
                datetime = TimeZoneInfo.ConvertTimeToUtc(datetime, webZone);
            }
            return datetime;
        }
        private Rule CheckCriteria(ObjectInfoBase info, int thresholdRuleOrder = -1)
        {
            using (PerformanceScope sc = new PerformanceScope("RuleManagement.CheckCriteria"))
            {
                //在这里遍历Rule时, 应该考虑到Rule的Order, 如果能准确依赖于Manager发的Order的话, 这里可以不考虑Order, 否则
                //应该在遍历的时候考虑到Rule的Order.
                foreach (var rulet in mRuleCollection.Rules.Values)
                {
                    if (!string.IsNullOrEmpty(ForceFitTeamsRuleID))
                    {
                        if (rulet.Id != ForceFitTeamsRuleID)
                            continue;
                        return rulet;
                    }
                    if (IsCGArchiver)
                    {
                        if (rulet.Id != CurrentRuleId)
                            continue;
                    }
                    if (-1 == thresholdRuleOrder || rulet.Order < thresholdRuleOrder)
                    {
                        //如果一个Version的检查过程中， threshold不等于－1表示这个version的当前版本有对应的Rule,
                        //因此，如果一个rulet.Order小于threshold时， 继续检查

                        try
                        {
                            //我们需要filter out模式
                            var engine = new FilterEngine(rulet.Filters, rulet.AndOrExpression, true);
                            if (engine.IsQualified(info))
                            {
                                return rulet;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is PropertyNotAssignedException)
                            {
                                _logger.Error("A property was not assigned while checking a Teams rule. Exception:{0}", ex.ToString());
                            }
                            // mLog.Error(LOGRESOURCE.StorageOptimization13_SOARCOMCheckRuleManagementCriteria + ex);
                            //throw new Exception(LOGRESOURCE.StorageOptimization13_SOARCOMRuleManagementCheckCriteriaException + rulet.Compression);
                            throw new Exception(ex.ToString());
                        }

                    }
                    else if (rulet.Order == thresholdRuleOrder)
                    {
                        return rulet;
                    }
                }
                return null;
            }
        }
        internal class FilterRuleTypeEqualityComparer : IEqualityComparer<FilterPolicy>
        {
            private static FilterRuleTypeEqualityComparer instance;

            private FilterRuleTypeEqualityComparer()
            {
            }
            public static FilterRuleTypeEqualityComparer GetInstance()
            {
                if (instance == null)
                {
                    instance = new FilterRuleTypeEqualityComparer();
                }
                return instance;
            }
            public bool Equals(FilterPolicy x, FilterPolicy y)
            {
                return x.Rule.GetType().Equals(y.Rule.GetType());
            }

            public int GetHashCode(FilterPolicy obj)
            {
                return 0;
            }
        }




        /// <summary>
        /// 这个类型的主要功能：
        /// 将Item/Document当前版本的Check Rule结果缓存起来（如果有的话），当它们的Version做Check Rule的时候， Version只检查
        /// Order大于 当前版本符合的rule的Order， 如果这之前的Order中有符合的Version的Rule,则返回该Rule,否则返回当前版本符合的Rule,
        /// 不会再做Order更小的Version Rule的Check. 但是如果Document/Item当前版本没有符合的Rule,则Version检查所有的Rule, 直到结尾
        /// </summary>
        private class RuleOrderCache
        {
            /// <summary>
            /// 用来缓存当前版本的Item/Document的Rule信息
            /// </summary>
            private KeyValuePair<Guid, int> mCacheLocalValue;
            /// <summary>
            /// 住缓存中加入一个item/Document对应的Rule信息
            /// </summary>
            /// <param name="id"></param>
            /// <param name="rule"></param>
            public void AddCacheInfo(Guid id, Rule rule)
            {
                //理论上来说， 不应该有已经存在的相同Id的信息
                mCacheLocalValue = new KeyValuePair<Guid, int>(id, rule.Order);
            }
            /// <summary>
            /// 获取一个Item/Document对应的Rule信息的Order值
            /// </summary>
            /// <param name="id"></param>
            /// <returns>Rule的Order值，如果不存在，返回－1</returns>
            public int GetRuleOrder(Guid id)
            {
                if ((mCacheLocalValue.Key.Equals(id)))
                {
                    return mCacheLocalValue.Value;
                }
                return -1;
            }

        }


        private void MergeFilterPolicy()
        {
            FilterPolicyCollection = new List<FilterPolicy>();
            var filterPolicyType = new List<Type>();
            foreach (var filterPolicy in mRuleCollection.Rules.Values.SelectMany(rule => rule.Filters))
            {
                FilterPolicyCollection.Add(filterPolicy);
                filterPolicyType.Add(filterPolicy.Rule.GetType());
                //if (!filterPolicyType.Contains(filterPolicy.Rule.GetType()))
                //{由于同样的rule不同level会产生错误，暂时先注掉，以后解决此问题

                //}
            }
        }

        #endregion

    }
}
