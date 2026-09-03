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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using LOGRESOURCEnew = Merged18NResources.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using System.Reflection;
using AvePoint.Wrapper.Discovery;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Collections;
using System.Text;
using System.Globalization;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract;

namespace AvePoint.RA.SharePoint.Archiver
{
   
    public class RecordsOneDriveRuleManagement
    {
        #region private member

        private readonly RuleOrderCache mCache = new RuleOrderCache();
        private RALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RuleCollection mRuleCollection;

        #endregion

        #region property

        public bool HasDocumentCondition { get; private set; }
        public bool HasAttachmentCondition { get; private set; }
        public bool HasDocVersionCondition { get; private set; }
        public bool HasItemVersionCondition { get; private set; }
        public bool HasItemCondition { get; private set; }
        public bool HasFolderCondition { get; private set; }//add for RevIM folder rule
        public bool HasListCondition { get; private set; }
        public bool HasListFilterCondition { get; private set; }
        public bool HasSiteCondition { get; private set; }
        public bool HasSiteFilterCondition { get; private set; }
        public bool HasSiteCollectionCondition { get; private set; }
        public bool HasSiteCollectionFilterCondition { get; private set; }
        private int RuleLevelNumber { get; set; }
        private List<FilterPolicy> FilterPolicyCollection { get; set; }

        #endregion property

        #region public method

        public RecordsOneDriveRuleManagement(RuleCollection sheduleRuleCollection, string jobId = "")
        {
            mRuleCollection = sheduleRuleCollection;
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
                    if (!HasFolderCondition)//add for RevIM folder rule
                    {
                        HasFolderCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Folder) != null);
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
                        HasSiteCollectionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.SiteCollection) != null);
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
            if (HasFolderCondition)//add for RevIM folder rule
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

        /// <summary>
        /// 用来检查一个文件是否符合Rule. 
        /// </summary>
        public Rule CheckItemCriteria(Guid docId, object oItem, Guid termId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckItemCriteria"))
            {
                ObjectInfoBase baseInfo = null;
                if (oItem is AveDiscoverItem)
                {
                    var item = oItem as AveDiscoverItem;
                    if (item == null)
                    {
                        return null;
                    }
                    baseInfo = item.GetFilterObjectInfo(FilterPolicyCollection);
                }
                else if (oItem is IAveListItem)
                {
                    IAveListItem item = oItem as IAveListItem;
                    if (item.ParentList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        List<FilterPolicy> docFilters = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document)).ToList();
                        baseInfo = FilterAnalyser.GetDocumentFilterInfo(docFilters, item);
                        var docInfo = baseInfo as DocumentInfo;
                    }
                    else
                    {
                        mLog.Info("Only support document library.");
                        return null;
                    }
                }
                //Records One Drive的Term Rule，其Term Value存在ExplorerDB里，需要外围传到Common Filter里后进行check rule
                if (baseInfo is ItemInfo)
                {
                    ItemInfo itemInfo = baseInfo as ItemInfo;
                    //Office365 APi Discussion Board and Survey List's Item title is null,we need to give it string.Empty
                    itemInfo.Name = itemInfo.Name ?? string.Empty;
                    itemInfo.Title = itemInfo.Title ?? string.Empty;
                    System.Collections.Hashtable oneDriveTermInfo = new System.Collections.Hashtable(StringComparer.OrdinalIgnoreCase);
                    oneDriveTermInfo["RecordsOneDriveTerm"] = termId.ToString();
                    itemInfo.TermInfosOfDisplayName = oneDriveTermInfo;
                }
                else if (baseInfo is DocumentInfo)
                {
                    DocumentInfo docInfo = baseInfo as DocumentInfo;
                    System.Collections.Hashtable oneDriveTermInfo = new System.Collections.Hashtable(StringComparer.OrdinalIgnoreCase);
                    oneDriveTermInfo["RecordsOneDriveTerm"] = termId.ToString();
                    docInfo.TermInfosOfDisplayName = oneDriveTermInfo;
                }
                var rs = CheckCriteria(baseInfo);
                if (null != rs)
                {
                    mCache.AddCacheInfo(docId, rs);
                }
                else
                {
                    mLog.LogToXml(string.Format("ItemInfo:{0}", docId), baseInfo);
                }

                return rs;
            }
        }

        public Rule CheckItemVersionCriteria(Guid docId, object oItem, object oItemVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckItemVersionCriteria"))
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
                    mLog.LogToXml(string.Format("ItemVersionInfo:{0}", item.LeafName), baseInfo);
                }
                return result;
            }
        }

        /// <summary>
        /// 检查Attachment是否符合Attachment Rule.
        /// </summary>
        public Rule CheckAttachmentCriteria(Guid docId, object oItem, object oAttachment)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckAttachmentCriteria"))
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
                            mLog.LogToXml(string.Format("ItemAttachmentInfo:{0}", item.LeafName), baseInfo);
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
                            mLog.LogToXml(string.Format("FolderAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                return null;
            }
        }
        public Rule CheckAttachmentCriteria(object oItem, object oAttachment)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckAttachmentCriteria1"))
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
                            mLog.LogToXml(string.Format("ItemAttachmentInfo:{0}", item.LeafName), baseInfo);
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
                            mLog.LogToXml(string.Format("FolderAttachmentInfo:{0}", item.LeafName), baseInfo);
                        }
                        return result;
                    }
                }
                return null;
            }
        }

        #region add for RevIM folder rule
        public Rule CheckFolderCriteria(object oFolder, bool IsMicroFeedList = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckFolderCriteria"))
            {
                Rule rule = null;
                var folder = oFolder as AveDiscoverFolder;
                if (folder == null)
                {
                    return null;
                }
                var baseInfo = folder.GetFilterObjectInfo(FilterPolicyCollection);
                rule = CheckCriteria(baseInfo);
                if (rule != null && rule.PolicyLevel == PolicyLevel.Folder && IsMicroFeedList)
                {
                    mLog.Info("Folder rule doesn't process MicroFeedList. Folder Name:{0}.", folder.LeafName);
                    rule = null;
                }
                //folder move to only support Document library folder and doesn't support list foler.SAAS-33399
                else if (rule != null && rule.MoveToRecordCenterAndDelareSetting != null && folder.AveFolder != null
                    && folder.AveFolder.ParentList != null && folder.AveFolder.ParentList.BaseType != AveBaseType.DocumentLibrary)
                {
                    mLog.Info("Folder rule is Move To and current folder parent List is not DocumentLibrary. Folder Name:{0}, Folder ParentList BaseType:{1}.", folder.LeafName, folder.AveFolder.ParentList.BaseType.ToString());
                    rule = null;
                }
                if (rule == null)
                {
                    mLog.LogToXml(string.Format("FolderInfo : {0}", folder.LeafName), baseInfo);
                }
                return rule;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oList"> </param>
        /// <returns></returns>
        public Rule CheckListCriteria(object oList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckListCriteria"))
            {
                var list = oList as AveDiscoverList;
                if (list == null)
                {
                    return null;
                }
                var baseinfo = list.GetFilterObjectInfo(FilterPolicyCollection);
                return CheckCriteria(baseinfo);
            }
        }

        //Check Or operation and list type policy All rules
        private bool CheckOrOperation()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckOrOperation"))
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
                    mLog.Warn(LOGRESOURCEnew.SCURuleManagementCheckOrOperation + ex.ToString());
                    return false;
                }
            }
        }

        private bool CheckFilterListType(string listType, List<FilterPolicy> Filters)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckFilterListType"))
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
        }


        public bool CheckListType(object oList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckListType"))
            {
                var list = oList as AveDiscoverList;
                var avelist = list.GetListObject();
                bool result = true;
                try
                {
                    string listType = ((int)avelist.BaseTemplate).ToString();

                    if (!CheckOrOperation())
                    {
                        return true;
                    }

                    bool allListTypeCheck = false;
                    //if match check list type condition,will check list type rule.
                    foreach (var rule in mRuleCollection.Rules)
                    {
                        if (CheckFilterListType(listType, rule.Value.Filters))
                        {
                            allListTypeCheck = true;
                        }
                    }
                    if (!allListTypeCheck)
                    {
                        return false;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    mLog.Warn(LOGRESOURCEnew.SCURuleManagementCheckListType + ex.ToString());
                    return true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oWeb"> </param>
        /// <returns></returns>
        public Rule CheckSiteCriteria(object oWeb)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckSiteCriteria"))
            {
                var web = oWeb as AveDiscoverWeb;
                if (web == null)
                {
                    return null;
                }
                var baseInfo = web.GetFilterObjectInfo(FilterPolicyCollection);
                return CheckCriteria(baseInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oSitecollection"> </param>
        /// <returns></returns>
        public Rule CheckSiteCollectionCriteria(object oSitecollection)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckSiteCollectionCriteria"))
            {
                var sitecollection = oSitecollection as AveDiscoverSite;
                if (sitecollection == null)
                {
                    return null;
                }
                var baseInfo = sitecollection.GetFilterObjectInfo(FilterPolicyCollection);
                return CheckCriteria(baseInfo);
            }
        }

        public Rule GetVaultRule()
        {
            if (mRuleCollection.Rules.Count != 0)
            {
                return mRuleCollection.Rules[1];
            }
            else
            {
                return null;
            }
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
                mLog.Info("Folder level check has folder level rule, RuleLevelNumber:{0}.", RuleLevelNumber);
                return RuleLevelNumber == (int)CacheNodeType.Folder;
            }
            return RuleLevelNumber == cacheNodeType;
        }

        public bool HasLowerLevelRule(int cacheNodeType)
        {
            return cacheNodeType < RuleLevelNumber;
        }
        #endregion

        #region private method

        private Rule CheckCriteria(ObjectInfoBase info, int thresholdRuleOrder = -1)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Archiver.RecordsOneDriveRuleManagement.CheckCriteria"))
            {
                //在这里遍历Rule时, 应该考虑到Rule的Order, 如果能准确依赖于Manager发的Order的话, 这里可以不考虑Order, 否则
                //应该在遍历的时候考虑到Rule的Order.
                foreach (var rulet in mRuleCollection.Rules.Values)
                {
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
                            mLog.Error("An error occurred while checking the rule.RuleName:{0}.Message:{1}.", rulet.Name, ex.ToString());
                            throw new Exception(LOGRESOURCE.StorageOptimization13_SOARCOMRuleManagementCheckCriteriaException + rulet.Compression);
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