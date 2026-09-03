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
using System.Text;
using System.IO;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility;
using System.Threading;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource.Restore;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    internal class AveMetadataServiceCache
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //将其改为Internal, 使用ThreadSafeDictionary, 去掉外层Log。
        internal static ThreadSafeDictionary<Guid, AveTermStoreCacheInfo> cacheTermStoreInfos = new ThreadSafeDictionary<Guid, AveTermStoreCacheInfo>();
        public static bool Enable = true;
        public static long IdleTime = 120;
        private static Thread monitorThread;

        static AveMetadataServiceCache()
        {
            monitorThread = new Thread(MonitorCache);
            monitorThread.IsBackground = true;
            monitorThread.Name = "AveMetadataServiceCache Monitor Thread";
            monitorThread.Start();
        }

        private static void MonitorCache()
        {
            try
            {
                while (true)
                {
                    if (Enable)
                    {
                        List<Guid> temp = new List<Guid>();
                        foreach (var uniqueId in cacheTermStoreInfos.Keys)
                        {
                            temp.Add(uniqueId);
                        }
                        foreach (var uniqueId in temp)
                        {
                            if (cacheTermStoreInfos.ContainsKey(uniqueId) && cacheTermStoreInfos[uniqueId].LastAccessTime.AddMinutes(IdleTime) < DateTime.UtcNow)
                            {
                                cacheTermStoreInfos.Remove(uniqueId);
                            }
                        }
                    }
                    else
                    {
                        cacheTermStoreInfos.Clear();
                    }
                    Thread.Sleep(30 * 60 * 1000);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Monitor cache data failed:{0}", ex.ToString());
            }
        }
    }

    #region moved to wrapper contract
    //public class AveTermStoreCacheInfo
    //{
    //    public DateTime LastAccessTime = DateTime.MinValue;
    //    public Guid UniqueId = Guid.Empty;
    //    public Dictionary<Guid, Guid> TermStoreIdMapping = new Dictionary<Guid, Guid>();
    //    public Dictionary<Guid, Guid> TermGroupIdMapping = new Dictionary<Guid, Guid>();
    //    public Dictionary<Guid, Guid> TermSetIdMapping = new Dictionary<Guid, Guid>();
    //    public Dictionary<Guid, Guid> TermIdMapping = new Dictionary<Guid, Guid>();
    //}
    #endregion

    public class AveMetadataService : IAveMetadataService, IDisposable
    {
        private IAveSite mSPSite;
        private AveObjectModelFactory objectModelFactory;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSiteMappingManager mSiteMappingManager;
        private AveSPMembers mMenbers;
        bool mRestoreManagedMetadataNavigation;
        public static int DefaultLCID = 1033;
        public bool RestoreUsedTermOnly = false;
        public bool EnableCache = false;
        public bool IsFeatureGenerate = false;
        public Dictionary<string, string> TermStoreMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Guid, Guid> mTermStoreIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> mTermGroupIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> mTermSetIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> mTermIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, AveTermStoreInfo> TermStoreInfoCache = new Dictionary<Guid, AveTermStoreInfo>();
        public List<Tuple<Guid, Guid, Guid, Guid>> PinIdInfos = new List<Tuple<Guid, Guid, Guid, Guid>>();
        public Dictionary<Guid, Guid> PinSourceIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, AveTermInfo> PinIdToTermInfoMapping = new Dictionary<Guid, AveTermInfo>();

        //提升效率，内部使用，不应公开
        private Dictionary<string, string> OwnerMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        //只为column value 用，此集合放MappingManager 是否更妥？
        public Dictionary<Guid, List<Guid>> MergedTermIdMapping = new Dictionary<Guid, List<Guid>>();

        public Wrapper.Core.SPRestore.ISPImportProfiler ImportProfiler { get; set; }

        private IReport report = new AveWrapperReport();
        /// <summary>
        /// Restore Term信息时是否忽略Global的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipGlobalTermGroup { get; set; }

        /// <summary>
        /// Restore Term信息时是否忽略Local的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipLocalTermGroup { get; set; }

        /// <summary>
        /// Restore Term信息是是否还原term属性。
        /// </summary>
        public bool RestoreTermProperties { get; set; }

        //无用option, 设置成false 没有意义
        public bool IsGetTermSetFromId { get; set; }

        public Dictionary<Guid, Guid> TermStoreIdMapping
        {
            get
            {
                return mTermStoreIdMapping;
            }
        }

        public Dictionary<Guid, Guid> TermGroupIdMapping
        {
            get
            {
                return mTermGroupIdMapping;
            }
        }

        public Dictionary<Guid, Guid> TermSetIdMapping
        {
            get
            {
                return mTermSetIdMapping;
            }
        }

        public Dictionary<Guid, Guid> TermIdMapping
        {
            get
            {
                return mTermIdMapping;
            }
        }

        /// <summary>
        /// 是否还原Navigation 信息
        /// </summary>
        /// 由于添加了RestoreTermProperties属性更改了逻辑，所以这个属性舍弃了。
        //private bool isRestoreNavigationProperty = false;

        private MetaDataServiceOption mOption;

        private TermSetCommitCalc _currentTermSetCalculator;

        [Obsolete("Use followed method with MetaDataServiceOption")]
        public AveMetadataService(AveSPSite site)
        {
            mSPSite = site.SPSite;
            mMenbers = site.SPMembers;
            this.objectModelFactory = site.ObjectModelFactory;
            this.mOption = new MetaDataServiceOption
            {
                EnableCache = this.EnableCache,
                IsFeatureGenerate = this.IsFeatureGenerate,
                SkipGlobalTermGroup = this.SkipGlobalTermGroup,
                SkipLocalTermGroup = this.SkipLocalTermGroup,
                RestoreTermSetAndTermProperties = this.RestoreTermProperties
            };
            mSiteMappingManager = site.MappingManager.SiteMappingManager;
            mRestoreManagedMetadataNavigation = site.RestoreManagedMetadataNavigation;
        }

        public AveMetadataService(AveSPSite site, MetaDataServiceOption mmsOption)
        {
            mSPSite = site.SPSite;
            mMenbers = site.SPMembers;
            this.objectModelFactory = site.ObjectModelFactory;
            this.mOption = mmsOption;
            mSiteMappingManager = site.MappingManager.SiteMappingManager;
            mRestoreManagedMetadataNavigation = site.RestoreManagedMetadataNavigation;
        }

        public IReport GetReport()
        {
            return report;
        }

        //保存原端传过来的所有termstore 信息
        public void CacheTermStoreInfo(List<AveTermStoreInfo> termStoreInfos)
        {
            foreach (AveTermStoreInfo termStoreInfo in termStoreInfos)
            {
                TermStoreInfoCache[termStoreInfo.Id] = termStoreInfo;
            }
        }

        //当TermStoreIdMapping中不存在sspid，尝试在目的端找到或者创建需要的termstore。	
        public Guid TryRestoreTermStore(Guid sspid)
        {
            IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
            if (session == null)
            {
                return sspid;
            }
            if (session.TermStores.Count <= 0)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_NotRelativeToMetadataService));
                log.Warn("The Destination did not relative to metadata service.");
                return sspid;
            }
            if (TermStoreInfoCache.ContainsKey(sspid))
            {
                string termStoreName = TermStoreInfoCache[sspid].Name;
                IAveTermStore termStore = null;
                try
                {
                    termStore = session.TermStores[termStoreName];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotGetTermStore, termStoreName, e.ToString());
                    //获取不到，使用DefaultSiteCollectionTermStore
                    termStore = session.DefaultSiteCollectionTermStore;
                    //termStore = session.DefaultKeywordsTermStore;
                    if (termStore == null)
                    {
                        termStore = session.DefaultKeywordsTermStore;
                    }
                    if (termStore == null)
                    {
                        termStore = session.TermStores[0];
                    }
                }
                if (!TermStoreIdMapping.ContainsKey(sspid))
                {
                    TermStoreIdMapping.Add(sspid, termStore.ID);
                }
                if (termStore != null)
                {
                    sspid = termStore.ID;
                }
            }
            else
            {
                sspid = Guid.Empty;
            }
            return sspid;
        }

        //当GroupIdMapping不存在groupId时，尝试在目的端找到对应或者创建需要的group
        public Guid TryRestoreGroup(Guid sspId, Guid groupId)
        {
            if (!sspId.Equals(Guid.Empty))
            {
                foreach (Guid termStoreId in TermStoreInfoCache.Keys)
                {
                    if (TermStoreIdMapping[termStoreId].Equals(sspId))
                    {
                        foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                        {
                            if (groupId.Equals(groupInfo.Id))
                            {
                                try
                                {
                                    IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                    IAveTaxonomyGroup destGroup = RestoreMetadataGroupSelf(session.TermStores[sspId], groupInfo);
                                    if (destGroup != null)
                                    {
                                        if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                        {
                                            TermGroupIdMapping.Add(groupInfo.Id, destGroup.ID);
                                        }
                                        return destGroup.ID;
                                    }
                                    log.Warn("there is no enoughPermission to RestoreMetadataGroupSelf");

                                }
                                catch (Exception ex)
                                {
                                    log.Warn("TryRestoreGroup in destination failed. error:{0}", ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
            else
            {	//sspid为null的情况
                foreach (Guid termStoreId in TermStoreInfoCache.Keys)
                {
                    foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                    {
                        if (groupId.Equals(groupInfo.Id))
                        {
                            try
                            {
                                IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                string termStoreName = TermStoreInfoCache[termStoreId].Name;
                                IAveTermStore termStore = null;
                                try
                                {
                                    termStore = session.TermStores[termStoreName];
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotGetTermStore, termStoreName, e.ToString());
                                    //获取不到，使用DefaultSiteCollectionTermStore
                                    termStore = session.DefaultSiteCollectionTermStore;
                                    //termStore = session.DefaultKeywordsTermStore;
                                    if (termStore == null)
                                    {
                                        termStore = session.DefaultKeywordsTermStore;
                                    }
                                    if (termStore == null)
                                    {
                                        termStore = session.TermStores[0];
                                    }
                                }
                                if (!TermStoreIdMapping.ContainsKey(termStoreId))
                                {
                                    TermStoreIdMapping.Add(termStoreId, termStore.ID);
                                }
                                if (termStore != null)
                                {
                                    sspId = termStore.ID;

                                    IAveTaxonomyGroup destGroup = RestoreMetadataGroupSelf(termStore, groupInfo);
                                    if (destGroup != null)
                                    {
                                        if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                        {
                                            TermGroupIdMapping.Add(groupInfo.Id, destGroup.ID);
                                        }
                                        return destGroup.ID;
                                    }
                                    log.Warn("there is no enoughPermission to RestoreMetadataGroupSelf");
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("TryRestoreGroup in destination failed. error:{0}", ex.ToString());
                            }
                        }
                    }
                }
            }
            return Guid.Empty;
        }

        //根绝提供的目的端sspid、groupId和原端的termsetId在目的端找到或者创建对象的termset
        public Guid TryResotreTermSet(Guid sspId, Guid groupId, Guid termSetId)
        {
            if (!sspId.Equals(Guid.Empty))
            {
                foreach (Guid termStoreId in TermStoreInfoCache.Keys)
                {
                    if (TermStoreIdMapping.ContainsKey(termStoreId) && TermStoreIdMapping[termStoreId].Equals(sspId))
                    {
                        if (!groupId.Equals(Guid.Empty))
                        {
                            foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                            {
                                if (TermGroupIdMapping.ContainsKey(groupInfo.Id) && TermGroupIdMapping[groupInfo.Id].Equals(groupId))
                                {
                                    foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                                    {
                                        if (termSetId.Equals(termSetInfo.Id))
                                        {
                                            try
                                            {
                                                IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);

                                                IAveTaxonomyGroup group = session.TermStores[sspId].Groups[groupId];
                                                IAveTermSet termSet = RestoreTermSetSelf(group, termSetInfo);
                                                if (termSet != null)
                                                {
                                                    _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                                                    if (!TermSetIdMapping.ContainsKey(termSetId))
                                                    {
                                                        TermSetIdMapping.Add(termSetId, termSet.ID);
                                                    }
                                                    return termSet.ID;
                                                }
                                                log.Warn("there is no enoughPermission to RestoreTermSetSelf");
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Warn("TryRestoreTermSet in destination failed. error:{0}", ex.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {	//sspid有值、groupId为null。
                            foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                            {
                                foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                                {
                                    if (termSetId.Equals(termSetInfo.Id))
                                    {
                                        try
                                        {
                                            IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                            IAveTermStore termStore = session.TermStores[sspId];
                                            IAveTaxonomyGroup group = RestoreMetadataGroupSelf(termStore, groupInfo);
                                            if (group != null)
                                            {
                                                if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                                {
                                                    TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                                                }
                                                groupId = group.ID;
                                                IAveTermSet termSet = RestoreTermSetSelf(group, termSetInfo);
                                                if (termSet != null)
                                                {
                                                    _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                                                    if (!TermSetIdMapping.ContainsKey(termSetInfo.Id))
                                                    {
                                                        TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                                                    }
                                                    return termSet.ID;
                                                }
                                                log.Warn("there is no enoughPermission to RestoreTermSetSelf");
                                            }
                                            log.Warn("there is no enoughPermission to RestoreMetadataGroupSelf");
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Warn("TryRestoreTermSet in destination failed. error:{0}", ex.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {	//sspid和groupId均为null
                foreach (Guid termStoreId in TermStoreInfoCache.Keys)
                {
                    foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                    {
                        foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                        {
                            if (termSetId.Equals(termSetInfo.Id))
                            {
                                try
                                {
                                    IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                    string termStoreName = TermStoreInfoCache[termStoreId].Name;
                                    IAveTermStore termStore = null;
                                    try
                                    {
                                        termStore = session.TermStores[termStoreName];
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotGetTermStore, termStoreName, e.ToString());
                                        //获取不到，使用DefaultSiteCollectionTermStore
                                        termStore = session.DefaultSiteCollectionTermStore;
                                        //termStore = session.DefaultKeywordsTermStore;
                                        if (termStore == null)
                                        {
                                            termStore = session.DefaultKeywordsTermStore;
                                        }
                                        if (termStore == null)
                                        {
                                            termStore = session.TermStores[0];
                                        }
                                    }
                                    if (!TermStoreIdMapping.ContainsKey(termStoreId))
                                    {
                                        TermStoreIdMapping.Add(termStoreId, termStore.ID);
                                    }
                                    if (termStore != null)
                                    {
                                        sspId = termStore.ID;
                                        IAveTaxonomyGroup group = RestoreMetadataGroupSelf(termStore, groupInfo);
                                        if (group != null)
                                        {
                                            if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                            {
                                                TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                                            }
                                            groupId = group.ID;
                                            IAveTermSet termSet = RestoreTermSetSelf(group, termSetInfo);
                                            if (termSet != null)
                                            {
                                                _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                                                if (!TermSetIdMapping.ContainsKey(termSetInfo.Id))
                                                {
                                                    TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                                                }
                                                return termSet.ID;
                                            }
                                            log.Warn("there is no enoughPermission to RestoreTermSetSelf");
                                        }
                                        log.Warn("there is no enoughPermission to RestoreMetadataGroupSelf");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("TryRestoreTermSet in destination failed. error:{0}", ex.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return Guid.Empty;
        }

        //通过目的端sspid、groupId、termsetId和原端的termId在目的端找到或者创建对应的term
        public Guid TryRestoreTerm(Guid sspId, Guid groupId, Guid termSetId, Guid termId)
        {
            foreach (Guid termStoreId in TermStoreInfoCache.Keys)
            {
                if (TermStoreIdMapping.ContainsKey(termStoreId) && sspId.Equals(TermStoreIdMapping[termStoreId]))
                {
                    if (!groupId.Equals(Guid.Empty))
                    {
                        foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                        {
                            if (TermGroupIdMapping.ContainsKey(groupInfo.Id) && groupId.Equals(TermGroupIdMapping[groupInfo.Id]))
                            {
                                foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                                {
                                    if (TermSetIdMapping.ContainsKey(termSetInfo.Id) && termSetId.Equals(TermSetIdMapping[termSetInfo.Id]))
                                    {
                                        try
                                        {
                                            IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                            IAveTermSet termSet = session.TermStores[sspId].Groups[groupId].TermSets[termSetId];

                                            if (_currentTermSetCalculator == null)
                                            {
                                                _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                                            }
                                            foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                            {
                                                if (termId.Equals(termInfo.Id))
                                                {
                                                    IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                                    if (term != null)
                                                    {
                                                        SetTermIdMapping(termInfo, term);
                                                        return term.ID;
                                                    }
                                                    log.Warn("there is no enoughPermission to RestoreTermSelf");
                                                }
                                            }

                                            Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                            if (subtermId != Guid.Empty)
                                            {
                                                return subtermId;
                                            }
                                            log.Warn("there is no enoughPermission to tryRecycleRestoreTerms");
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Warn("TryRestoreTerm in destination failed. error:{0}", ex.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //sspid有值、groupId为null、termSetid有值。
                        foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[termStoreId].Groups)
                        {
                            foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                            {
                                if (TermSetIdMapping.ContainsKey(termSetInfo.Id) && termSetId.Equals(TermSetIdMapping[termSetInfo.Id]))
                                {
                                    try
                                    {
                                        IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                                        IAveTermSet termSet = session.TermStores[sspId].GetTermSet(termSetId);

                                        if (_currentTermSetCalculator == null)
                                        {
                                            _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                                        }
                                        foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                        {
                                            if (termId.Equals(termInfo.Id))
                                            {
                                                IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                                if (term != null)
                                                {
                                                    SetTermIdMapping(termInfo, term);
                                                    return term.ID;
                                                }
                                                log.Warn("there is no enoughPermission to RestoreTermSelf");
                                            }
                                        }

                                        Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                        if (subtermId != Guid.Empty)
                                        {
                                            return subtermId;
                                        }
                                        log.Warn("there is no enoughPermission to tryRecycleRestoreTerms");
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Warn("TryRestoreTerm in destination failed. error:{0}", ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Guid.Empty;
        }

        private void SetTermIdMapping(AveTermInfo termInfo, IAveTerm term)
        {
            if (!TermIdMapping.ContainsKey(termInfo.Id))
            {
                TermIdMapping.Add(termInfo.Id, term.ID);
            }
            if (termInfo.MergedTermIds != null && termInfo.MergedTermIds.Count > 0 && !MergedTermIdMapping.ContainsKey(term.ID))
            {
                MergedTermIdMapping.Add(term.ID, termInfo.MergedTermIds);
            }
        }

        //在目的端查找或者restore 需要的对应subterm和直属关系的parent subterm
        public Guid TryRestoreSubTerms(AveTermSetInfo termSetInfo, Guid termId, IAveTermSet termSet)
        {
            List<AveTermInfo> listTerms = new List<AveTermInfo>();
            try
            {
                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                {
                    bool find = TryFindSubTerm(termInfo, termId, listTerms);
                    if (find)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("find sub terms in destination failed, error:{0}", ex.ToString());
            }
            if (listTerms.Count > 0)
            {
                IAveTerm term = RestoreTermSelf(termSet, listTerms[listTerms.Count - 1]);
                if (term != null)
                {
                    SetTermIdMapping(listTerms[listTerms.Count - 1], term);
                    for (int i = listTerms.Count - 2; i >= 0; i--)
                    {
                        IAveTerm subTerm = RestoreSubTermSelf(term, listTerms[i]);
                        if (subTerm != null)
                        {
                            SetTermIdMapping(listTerms[i], subTerm);
                            term = subTerm;
                        }
                    }

                }
                if (TermIdMapping.ContainsKey(termId))
                {
                    return TermIdMapping[termId];
                }
            }
            return Guid.Empty;
        }

        //按照subterm id在原端的termInfo中查找对应的subterm
        public bool TryFindSubTerm(AveTermInfo termInfo, Guid termId, List<AveTermInfo> listTerms)
        {
            bool find = false;
            foreach (AveTermInfo subtermInfo in termInfo.Terms)
            {
                if (subtermInfo.Id.Equals(termId))
                {
                    listTerms.Add(subtermInfo);
                    listTerms.Add(termInfo);
                    return true;
                }
            }
            foreach (AveTermInfo subtermInfo in termInfo.Terms)
            {
                find = TryFindSubTerm(subtermInfo, termId, listTerms);

                if (find)
                {
                    listTerms.Add(termInfo);
                    return true;
                }
            }
            return find;
        }

        public bool FindSubTerm(AveTermInfo termInfo, Guid termId)
        {
            if (!termInfo.Terms.Exists(tinfo => tinfo.Id == termId))
            {
                foreach (AveTermInfo subtermInfo in termInfo.Terms)
                {
                    bool find = FindSubTerm(subtermInfo, termId);

                    if (find)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public void Restore(AveManagedMetadataServiceApplicationInfo serviceAppInfo, Guid targetServiceAppId)
        {
            IAveMetadataServiceRestorer serviceAppRestorer = this.objectModelFactory.CreateMetadataServiceRestorer(targetServiceAppId);
            serviceAppRestorer.Restore(serviceAppInfo);
        }

        public void Restore(List<AveTermStoreInfo> termStoreInfos, AveMappingManager siteMappingManager = null, bool restoreManagedMetadataNavigation = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.Restore"))
            {
                if (mSiteMappingManager == null)
                {
                    mSiteMappingManager = siteMappingManager != null ? siteMappingManager.SiteMappingManager : new AveSiteMappingManager();
                    mRestoreManagedMetadataNavigation = restoreManagedMetadataNavigation;
                }

                try
                {
                    IAveTaxonomySession session = mSPSite.AveSPTaxonomySession;
                    if (session == null)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_CannotCreateMMSSession));
                        return;
                    }
                    if (session.TermStores.Count <= 0)
                    {
                        log.Warn("The Destination did not relative to metadata service.");
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_NotRelativeToMetadataService));
                        return;
                    }
                    foreach (AveTermStoreInfo termStoreInfo in termStoreInfos)
                    {
                        try
                        {
                            DefaultLCID = termStoreInfo.DefaultLanguage;
                            if (mOption.EnableCache)
                            {
                                RestoreTermStoreByCache(session, termStoreInfo);
                            }
                            else
                            {
                                IAveTermStore termStore = RestoreTermStore(session, termStoreInfo);
                                if (!TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                                {
                                    TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while restoring term store.Error:{0}", e);
                        }
                    }
                    log.Debug("Finish restoring MMS.");
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn(string.Format("An error occurred while Restore AveMetadataService. error:{0}", ex.ToString()));
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreMetadataService, ex.Message));
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreAveMetadataServiceError, e.Message));
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreMetadataServiceFailedEventMessage(e));
                }
            }
        }

        public void RestoreTermStoreByCache(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo)
        {
            AveTermStoreCacheInfo cacheInfo = null;
            if (AveMetadataServiceCache.cacheTermStoreInfos.ContainsKey(termStoreInfo.UniqueId))
            {
                cacheInfo = AveMetadataServiceCache.cacheTermStoreInfos[termStoreInfo.UniqueId];
                if (cacheInfo.LastAccessTime.Equals(termStoreInfo.LastAccessTime))
                {
                    foreach (var t in cacheInfo.TermStoreIdMapping)
                    {
                        if (!TermStoreIdMapping.ContainsKey(t.Key))
                        {
                            TermStoreIdMapping.Add(t.Key, t.Value);
                        }
                    }
                    foreach (var t in cacheInfo.TermGroupIdMapping)
                    {
                        if (!TermGroupIdMapping.ContainsKey(t.Key))
                        {
                            TermGroupIdMapping.Add(t.Key, t.Value);
                        }
                    }
                    foreach (var t in cacheInfo.TermSetIdMapping)
                    {
                        if (!TermSetIdMapping.ContainsKey(t.Key))
                        {
                            TermSetIdMapping.Add(t.Key, t.Value);
                        }
                    }
                    foreach (var t in cacheInfo.TermIdMapping)
                    {
                        if (!TermIdMapping.ContainsKey(t.Key))
                        {
                            TermIdMapping.Add(t.Key, t.Value);
                        }
                    }
                }
                else
                {
                    lock (cacheInfo)
                    {
                        cacheInfo.LastAccessTime = termStoreInfo.LastAccessTime;
                        IAveTermStore termStore = RestoreTermStore(session, termStoreInfo, cacheInfo);
                        if (!TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                        {
                            TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                        }
                        if (!cacheInfo.TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                        {
                            cacheInfo.TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                        }
                    }
                }
            }
            else
            {
                cacheInfo = new AveTermStoreCacheInfo();
                cacheInfo.LastAccessTime = termStoreInfo.LastAccessTime;
                cacheInfo.UniqueId = termStoreInfo.UniqueId;
                IAveTermStore termStore = RestoreTermStore(session, termStoreInfo, cacheInfo);
                if (!TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                {
                    TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                }
                if (!cacheInfo.TermStoreIdMapping.ContainsKey(termStoreInfo.Id))
                {
                    cacheInfo.TermStoreIdMapping.Add(termStoreInfo.Id, termStore.ID);
                }
                AveMetadataServiceCache.cacheTermStoreInfos[termStoreInfo.UniqueId] = cacheInfo;
            }
        }

        public void OutputDestServiceInfo(IAveTaxonomySession session)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.OutputDestServiceInfo"))
            {
                StringBuilder info = new StringBuilder();
                info.Append("Destination TermStores: ");
                foreach (IAveTermStore store in session.TermStores)
                {
                    info.Append(store.Name + "; ");
                }
                log.Info(info.ToString());
            }
        }

        public void OutputDebugServiceInfo()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.OutputDebugServiceInfo"))
            {

                StringBuilder info = new StringBuilder();
                info.AppendLine("TermStoreIdMapping:");
                foreach (KeyValuePair<Guid, Guid> pair in TermStoreIdMapping)
                {
                    info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
                }
                info.AppendLine("TermGroupIdMapping:");
                foreach (KeyValuePair<Guid, Guid> pair in TermGroupIdMapping)
                {
                    info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
                }
                info.AppendLine("TermSetIdMapping:");
                foreach (KeyValuePair<Guid, Guid> pair in TermSetIdMapping)
                {
                    info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
                }
                info.AppendLine("TermIdMapping:");
                foreach (KeyValuePair<Guid, Guid> pair in TermIdMapping)
                {
                    info.AppendLine(pair.Key.ToString() + " -> " + pair.Value.ToString());
                }
                log.Debug(info.ToString());

            }

        }

        /// <summary>
        /// group的Group Managers，Contributors和termset的Owner，Stakeholders，可能带有类似i:0#.w|的头，
        /// 当是AD group的时候，用API取出来的account是Sid格式的，但是添加的时候需要用Account格式。
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string GetUserRealLoginName(string account, AveObjectModelFactory modelFactory)
        {
            if (modelFactory.APIType == AveAPIType.BPOS_S)
            {
                return account;
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.EnsureAccountName"))
            {
                if (account.IndexOf('|') > 0)
                {
                    account = account.Substring(account.IndexOf('|') + 1);
                }
                if (AveDirectoryServiceUtility.IsStringSid(account))
                {
                    account = AveDirectoryServiceUtility.GetAccountFromSid(account, modelFactory);
                }
                return account;
            }
        }
        //restore itemself之前确认对应MetadataColumn value的term是否应经在目的端存在，如果没有，先check源端是否可用，如果不可用，skip item
        public bool VerifyMetadataColumnValue(IAveList List, Dictionary<string, string> fieldTermMapping, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping)
        {
            try
            {
                foreach (string fieldName in fieldTermMapping.Keys)
                {
                    IAveField field = List.Fields.GetField(fieldName);
                    IAveTaxonomyField tField = field as IAveTaxonomyField;
                    bool isKeywordsColumn = tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38"));
                    IAveTaxonomySession session = List.ParentWeb.Site.AveSPTaxonomySession;
                    IAveTermStore termStore = null;
                    Guid sspId = Guid.Empty;
                    if (tField.SspId == Guid.Empty && !isKeywordsColumn)
                    {
                        object customProperty = field.GetCustomProperty("SspId");
                        if (customProperty != null)
                        {
                            sspId = new Guid(customProperty.ToString());
                        }
                    }
                    else
                    {
                        sspId = tField.SspId;
                    }
                    if (sspId != Guid.Empty)
                    {
                        try
                        {
                            termStore = session.TermStores[sspId];
                        }
                        catch (Exception ex)
                        {
                            //如果原端的field使用的service不在被原端引用，也就是说mms没有被还原，该field的原端属性无法替换，这个sspid也是原端的Id，这时在目的端无法找到
                            //为了保障其他的mms field属性的正确还原，添加try catch，跳过该field的还原
                            //新的还原逻辑：当mms没有被还原，sspid还是原端id时，目的端还原item的mms filed value找不到需要的term及所以termstore，直接抛出，skip item。
                            log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCSetFieldValueError, sspId, ex.ToString());
                            return false;
                        }
                    }
                    else
                    {
                        termStore = session.DefaultKeywordsTermStore;
                        if (termStore == null)
                        {
                            termStore = session.DefaultSiteCollectionTermStore;
                        }
                        if (termStore == null)
                        {
                            termStore = session.TermStores[0];
                        }
                    }
                    IAveTermSet termSet = null;
                    if (tField.TermSetId != Guid.Empty && termStore != null)
                    {
                        termSet = termStore.GetTermSet(tField.TermSetId);
                    }

                    string[] termNames = fieldTermMapping[fieldName].Split(';');
                    foreach (string termName in termNames)
                    {
                        if (string.IsNullOrEmpty(termName))
                        {
                            continue;
                        }
                        IAveTerm term = null;
                        string tName = termName;
                        if (termName.Contains("|"))
                        {
                            try
                            {
                                Guid tTermId = Guid.Empty;
                                string[] temp = termName.Split('|');
                                if (temp.Length == 2)
                                {
                                    tName = temp[0];
                                    tTermId = new Guid(temp[1]);
                                    if (termIdMapping != null && termIdMapping.ContainsKey(tTermId))
                                    {
                                        tTermId = termIdMapping[tTermId];
                                    }
                                    else if (mergedTermIdMapping != null)  //ADO-148478:FindTermById还要考虑term的mergedTermIds属性中元素
                                    {
                                        foreach (var pair in mergedTermIdMapping)
                                        {
                                            if (pair.Value.Contains(tTermId))
                                            {
                                                tTermId = pair.Key;
                                                break;
                                            }
                                        }
                                    }

                                    if (termSet != null)
                                    {
                                        term = termSet.GetTerm(tTermId);
                                    }

                                    if(term == null)
                                    {
                                        term = termStore.GetTerm(tTermId);
                                        //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                                        if (term == null && isKeywordsColumn)
                                        {
                                            foreach (IAveTermStore tStore in session.TermStores)
                                            {
                                                if (term == null)
                                                {
                                                    term = tStore.GetTerm(tTermId);
                                                }
                                                else break;
                                            }
                                        }
                                    }
                                    if (term == null)
                                    {
                                        //如果源端可用，但是目的端没有找到对应的Term，则Verify失败
                                        if (!CheckIfSourceIsValid(termStore, termSet, tTermId, isKeywordsColumn))
                                        {
                                            return false;
                                        }
                                        //如果源端不可用，跳过verify此column value
                                        else
                                        {
                                            log.Log(AveLogLevel.WARN, "Skip to restore metadata column value because it is invalid in source. Field: {0}", fieldName);
                                            continue;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("try get fieldValue term before restore item. field:{0},error:{1}", fieldName, ex.ToString());
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("VerifyMetadataColumnValue failed, list:{0}, error:{1}", List.Title, ex.ToString());
                return false;
            }
            return true;
        }

        //Check 源端term是否是可用状态， 需要优化
        internal bool CheckIfSourceIsValid(IAveTermStore termStore, IAveTermSet termSet, Guid termId, bool isKeywordsColumn)
        {
            if (!isKeywordsColumn)
            {
                if (TermStoreIdMapping.ContainsValue(termStore.ID))
                {
                    KeyValuePair<Guid, Guid> sspId = TermStoreIdMapping.First(keyValue => keyValue.Value == termStore.ID);
                    if (TermStoreInfoCache.ContainsKey(sspId.Key))
                    {
                        AveTermStoreInfo termStoreInfo = TermStoreInfoCache[sspId.Key];
                        if (termSet != null)
                        {
                            if (TermGroupIdMapping.ContainsValue(termSet.Group.ID))
                            {
                                KeyValuePair<Guid, Guid> groupId = TermGroupIdMapping.First(keyValue => keyValue.Value == termSet.Group.ID);
                                AveMetadataGroupInfo groupInfo = termStoreInfo.Groups.Find(info => info.Id == groupId.Key);
                                if (groupInfo != null)
                                {
                                    if (TermSetIdMapping.ContainsValue(termSet.ID))
                                    {
                                        KeyValuePair<Guid, Guid> termSetId = TermSetIdMapping.First(keyValue => keyValue.Value == termSet.ID);
                                        AveTermSetInfo termSetInfo = groupInfo.TermSets.Find(info => info.Id == termSetId.Key);
                                        if (termSetInfo != null)
                                        {
                                            if (termSetInfo.Terms.Exists(tinfo => tinfo.Id == termId))
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                                {
                                                    if (FindSubTerm(termInfo, termId))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (AveTermStoreInfo termStoreInfo in TermStoreInfoCache.Values)
                {
                    foreach (AveMetadataGroupInfo groupInfo in termStoreInfo.Groups)
                    {
                        foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                        {
                            if (termSetInfo.Terms.Exists(tinfo => tinfo.Id == termId))
                            {
                                return true;
                            }
                            else
                            {
                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                {
                                    if (FindSubTerm(termInfo, termId))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public IAveTerm CreateUsedTermOnly(IAveTermStore termStore, IAveTermSet termSet, Guid termId)
        {
            IAveTerm keyword = null;
            foreach (Guid sspid in TermStoreInfoCache.Keys)
            {
                if (TermStoreIdMapping.ContainsKey(sspid) && termStore.ID.Equals(TermStoreIdMapping[sspid]))
                {
                    foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[sspid].Groups)
                    {
                        foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                        {
                            if (termSet != null && TermSetIdMapping.ContainsKey(termSetInfo.Id) && termSet.ID.Equals(TermSetIdMapping[termSetInfo.Id]))
                            {
                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                {
                                    if (termId.Equals(termInfo.Id))
                                    {
                                        IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                        if (term != null)
                                        {
                                            SetTermIdMapping(termInfo, term);
                                            return term;
                                        }
                                        log.Warn("there is no enoughPermission to RestoreTermSelf");
                                    }
                                }

                                Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                if (subtermId != Guid.Empty)
                                {
                                    keyword = termStore.GetTerm(subtermId);
                                    return keyword;
                                }
                            }
                            else
                            {
                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                {
                                    if (termId.Equals(termInfo.Id))
                                    {
                                        IAveTaxonomyGroup group = RestoreMetadataGroupSelf(termStore, groupInfo);
                                        if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                        {
                                            TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                                        }
                                        termSet = RestoreTermSetSelf(group, termSetInfo);
                                        if (!TermSetIdMapping.ContainsKey(termSetInfo.Id))
                                        {
                                            TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                                        }
                                        IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                        if (term != null)
                                        {
                                            SetTermIdMapping(termInfo, term);
                                            return term;
                                        }
                                    }
                                }

                                Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                if (subtermId != Guid.Empty)
                                {
                                    keyword = termStore.GetTerm(subtermId);
                                    return keyword;
                                }
                            }
                        }

                    }
                }
                else
                {
                    foreach (AveMetadataGroupInfo groupInfo in TermStoreInfoCache[sspid].Groups)
                    {
                        foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                        {
                            if (termSet != null && TermSetIdMapping.ContainsKey(termSetInfo.Id) && termSet.ID.Equals(TermSetIdMapping[termSetInfo.Id]))
                            {
                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                {
                                    if (termId.Equals(termInfo.Id))
                                    {
                                        IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                        if (term != null)
                                        {
                                            SetTermIdMapping(termInfo, term);
                                            return term;
                                        }
                                        log.Warn("there is no enoughPermission to RestoreTermSelf");
                                    }
                                }

                                Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                if (subtermId != Guid.Empty)
                                {
                                    keyword = termStore.GetTerm(subtermId);
                                    return keyword;
                                }
                            }
                            else
                            {
                                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                                {
                                    if (termId.Equals(termInfo.Id))
                                    {
                                        IAveTaxonomyGroup group = RestoreMetadataGroupSelf(termStore, groupInfo);
                                        if (!TermGroupIdMapping.ContainsKey(groupInfo.Id))
                                        {
                                            TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                                        }
                                        termSet = RestoreTermSetSelf(group, termSetInfo);
                                        if (!TermSetIdMapping.ContainsKey(termSetInfo.Id))
                                        {
                                            TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                                        }
                                        IAveTerm term = RestoreTermSelf(termSet, termInfo);
                                        if (term != null)
                                        {
                                            SetTermIdMapping(termInfo, term);
                                            return term;
                                        }
                                    }
                                }

                                Guid subtermId = TryRestoreSubTerms(termSetInfo, termId, termSet);
                                if (subtermId != Guid.Empty)
                                {
                                    keyword = termStore.GetTerm(subtermId);
                                    return keyword;
                                }
                            }
                        }

                    }
                }
            }


            return keyword;
        }

        public IAveTermStore RestoreTermStore(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo)
        {
            return RestoreTermStore(session, termStoreInfo, null);
        }

        public IAveTermStore RestoreTermStore(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo, AveTermStoreCacheInfo cacheInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTermStore"))
            {
                string termStoreName = termStoreInfo.Name;
                IAveTermStore termStore = null;
                try
                {
                    Guid termStoreId = Guid.Empty;
                    if (TermStoreIdMapping.TryGetValue(termStoreInfo.Id, out termStoreId))
                    {
                        termStore = session.TermStores[termStoreId];
                    }
                    else
                    {
                        termStore = session.TermStores[termStoreName];
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermDestinationError, termStoreName, e.ToString());
                    //获取不到，使用DefaultSiteCollectionTermStore
                    termStore = session.DefaultSiteCollectionTermStore;
                    //termStore = session.DefaultKeywordsTermStore;
                    if (termStore == null)
                    {
                        termStore = session.DefaultKeywordsTermStore;
                    }
                    if (termStore == null)
                    {
                        termStore = session.TermStores[0];
                    }
                }
                foreach (AveMetadataGroupInfo groupInfo in termStoreInfo.Groups)
                {
                    try
                    {
                        IAveTaxonomyGroup group = RestoreMetadataGroup(termStore, groupInfo, cacheInfo);
                        if (group != null && !TermGroupIdMapping.ContainsKey(groupInfo.Id))
                        {
                            TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                        }
                        if (group != null && cacheInfo != null && !cacheInfo.TermGroupIdMapping.ContainsKey(groupInfo.Id))
                        {
                            cacheInfo.TermGroupIdMapping.Add(groupInfo.Id, group.ID);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while restoring term group, GroupName: {0}, error: {1}", groupInfo.Name, e);
                    }
                }
                PostProcessPinTerms(termStore);
                return termStore;

            }

        }

        //只restore MetadataGroup，不还原下属的termset
        public IAveTaxonomyGroup RestoreMetadataGroupSelf(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            if (mOption.SkipGlobalTermGroup && !groupInfo.IsSiteCollectionGroup)
            {
                return null;
            }
            if (mOption.SkipLocalTermGroup && groupInfo.IsSiteCollectionGroup)
            {
                return null;
            }
            string groupName = groupInfo.Name;
            IAveTaxonomyGroup group = null;
            try
            {

                if (groupInfo.IsSiteCollectionGroup)
                {
                    try
                    {
                        //处理删除sitecollection做inplace还原的时候，由于删除的sitecollection的local group仍然存在，导致无法新建的问题
                        string siteCollectionGroupName = termStore.GetSiteCollectionGroupName(mSPSite);//(string)Invoker.CallMethod(termStore, "GetSiteCollectionGroupName", new Type[] { mSite.GetType() }, new object[] { mSite });
                        group = termStore.Groups[siteCollectionGroupName];
                        if (group.IsSiteCollectionGroup && !group.SiteCollectionAccessIds.Contains(mSPSite.ID))
                        {
                            for (int i = group.TermSets.Count - 1; i >= 0; i--)
                            {
                                group.TermSets[i].Delete();
                            }
                            group.Delete();
                            termStore.CommitAll();
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.CannotDeleteMetadataGroup, e);
                    }
                    group = termStore.GetSiteCollectionGroup(mSPSite);
                    //(IAveTaxonomyGroup)Invoker.CallMethod(termStore, "GetSiteCollectionGroup", new Type[] { mSite.GetType() }, new object[] { mSite });
                    UpdateSiteCollectionGroupProperties(group, groupInfo);
                    //TODO...need to implement in server mode.
                    //group = (IAveTaxonomyGroup)Invoker.CallMethod(termStore, "GetSiteCollectionGroup", new Type[] { mSite.GetType() }, new object[] { mSite });
                }
                else
                {
                    group = termStore.Groups[groupInfo.Name];
                }
            }
            catch (AveException e)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateSiteGroupError, e.Message));
                log.Warn("An error occurred while create Site Group" + e.InnerException.ToString());
            }
            catch (Exception ex)
            {
                log.Warn("Get the group:{0} failed:{1}.", groupInfo.Name, ex.ToString());
                //IsSystemGroup和IsSiteCollectionGroup属性都是只读属性。此处处理为，如果源端IsSystemGroup或IsSiteCollectionGroup等于true，先找对应的Group。
                if (groupInfo.IsSystemGroup && termStore.SystemGroup != null)
                {
                    group = termStore.SystemGroup;
                }
                if (group == null)
                {
                    group = CreateMetadataGroup(termStore, groupInfo);
                    if (group == null)
                    {
                        return null;
                    }
                }
            }
            return group;
        }

        public IAveTaxonomyGroup RestoreMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            return RestoreMetadataGroup(termStore, groupInfo, null);
        }

        public IAveTaxonomyGroup RestoreSiteCollectionGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo, out bool isNewCreated)
        {
            IAveTaxonomyGroup group = null;
            isNewCreated = false;
            try
            {
                //处理删除site collection做inplace还原的时候，由于删除的sitecollection的local group仍然存在，导致无法新建的问题
                string siteCollectionGroupName = termStore.GetSiteCollectionGroupName(mSPSite);
                group = termStore.Groups[siteCollectionGroupName];
                if (group.IsSiteCollectionGroup && !group.SiteCollectionAccessIds.Contains(mSPSite.ID))
                {
                    for (int i = group.TermSets.Count - 1; i >= 0; i--)
                    {
                        group.TermSets[i].Delete();
                    }
                    group.Delete();
                    termStore.CommitAll();
                    group = null;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.CannotDeleteMetadataGroup, e);
            }
            if (group == null)
            {
                group = termStore.GetSiteCollectionGroup(mSPSite, false);
                if (group == null)
                {
                    group = termStore.GetSiteCollectionGroup(mSPSite, true);
                    if (group == null)
                    {
                        return null;
                    }
                    UpdateSiteCollectionGroupProperties(group, groupInfo);
                    isNewCreated = true;
                }
            }
            if (mOption.IsFeatureGenerate)
            {
                UpdateSiteCollectionGroupProperties(group, groupInfo);
                isNewCreated = true;
            }
            return group;
        }

        public IAveTaxonomyGroup RestoreGlobalGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo, out bool isNewCreated)
        {
            IAveTaxonomyGroup group = null;
            isNewCreated = false;
            try
            {
                group = termStore.Groups[groupInfo.Name];
            }
            catch (Exception ex)
            {
                log.Warn("Get term group: {0} failed: {1}", groupInfo.Name, ex.Message);
                if (groupInfo.IsSystemGroup && termStore.SystemGroup != null)
                {
                    group = termStore.SystemGroup;
                }
                else
                {
                    group = CreateMetadataGroup(termStore, groupInfo);
                    if (group == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
            }
            return group;
        }


        /// <summary>
        /// Restore Term Group
        /// </summary>
        /// <param name="termStore"></param>
        /// <param name="groupInfo"></param>
        /// <param name="cacheInfo"></param>
        /// <returns></returns>
        public IAveTaxonomyGroup RestoreMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreMetadataGroup"))
            {
                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermGroupRestored, groupInfo.Name), Status = WrapperRestoreStatus.Successful, Title = groupInfo.Name, Url = this.mSPSite.Url, Type = SPObjectType.TermGroup });
                }

                //如果不还原Global Term Group 就返回
                if (mOption.SkipGlobalTermGroup && !groupInfo.IsSiteCollectionGroup)
                {
                    return null;
                }
                //如果不还原Local Term Group 就返回
                if (mOption.SkipLocalTermGroup && groupInfo.IsSiteCollectionGroup)
                {
                    return null;
                }
                string groupName = groupInfo.Name;
                IAveTaxonomyGroup group = null;
                bool isNewCreated = false;

                if (groupInfo.IsSiteCollectionGroup)
                {
                    group = RestoreSiteCollectionGroup(termStore, groupInfo, out isNewCreated);
                }
                else
                {
                    group = RestoreGlobalGroup(termStore, groupInfo, out isNewCreated);
                }
                if (group == null)
                {
                    return null;
                }

                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermGroupRestored, groupInfo.Name), Status = WrapperRestoreStatus.Successful, Title = group.Name, Url = this.mSPSite.Url, Type = SPObjectType.TermGroup });
                }

                foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                {
                    try
                    {
                        IAveTermSet termSet = RestoreTermSet(group, termSetInfo, isNewCreated, cacheInfo);
                        if (termSet != null && !TermSetIdMapping.ContainsKey(termSetInfo.Id))
                        {
                            TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                        }
                        if (termSet != null && cacheInfo != null && !cacheInfo.TermSetIdMapping.ContainsKey(termSetInfo.Id))
                        {
                            cacheInfo.TermSetIdMapping.Add(termSetInfo.Id, termSet.ID);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while restoring term set, TermSetName: {0}, Error: {1}", termSetInfo.Name, e);
                    }
                }
                sysGroupTermSetNamesCache = null;
                return group;
            }

        }

        private void UpdateGroupProperties(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            group.Description = groupInfo.Description;

            foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
            {
                string principalName = groupManager.PrincipalName;
                string mappedName = mMenbers.GetMappingUserLogin(principalName);
                try
                {
                    mappedName = GetUserRealLoginName(mappedName, objectModelFactory);
                    group.AddGroupManager(mappedName);
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_GetTermSetByNameError, e.Message));
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermSetByNameError + e.ToString());
                }
            }

            foreach (AveAceInfo contributor in groupInfo.Contributors)
            {
                string principalName = contributor.PrincipalName;
                string mappedName = mMenbers.GetMappingUserLogin(principalName);
                try
                {
                    mappedName = GetUserRealLoginName(mappedName, objectModelFactory);
                    group.AddContributor(mappedName);
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_AddGroupContributorError, principalName, e.Message));
                    log.Warn("An error occurred while add group contributor. principalName:{0}, error:{1}", mappedName, e.ToString());
                }
            }
        }

        private void UpdateSiteCollectionGroupProperties(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.UpdateMetadataGroup"))
            {
                try
                {
                    UpdateGroupProperties(group, groupInfo);

                    //此属性只有SP13才有，SP10此属性为空，否则异常影响属性还原。
                    if (groupInfo.IsSiteCollectionGroup && groupInfo.SiteCollectionReadOnlyAccessUrls != null)
                    {
                        foreach (string url in groupInfo.SiteCollectionReadOnlyAccessUrls)
                        {
                            group.AddSiteCollectionReadOnlyAccess(url);
                        }
                    }

                    group.TermStore.CommitAll();
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_UpdateGroupPropertyError, group.Name, e.Message));
                    log.Warn("An error occurred while update group property. groupName:{0}, error:{1}", group.Name, e);
                }
            }

        }

        public IAveTaxonomyGroup CreateMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateMetadataGroup"))
            {
                try
                {
                    IAveTaxonomyGroup group = termStore.CreateGroup(groupInfo.Name);
                    try
                    {
                        UpdateGroupProperties(group, groupInfo);
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetNewCreateGroupPropertyError, groupInfo.Name, e.Message));
                        log.Warn(string.Format("An error occurred while set new create group property. groupName:{0}, error:{1}", groupInfo.Name, e));
                    }
                    group.TermStore.CommitAll();
                    return group;
                }
                catch (Exception e)
                {
                    log.Warn(string.Format("An error occurred while create term Group. group Name:{0}, error:{1}", groupInfo.Name, e.ToString()));
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateTermGroupError, groupInfo.Name, e.Message));
                    return null;
                }
            }
        }

        //只还原termset，不还原下属的term
        public IAveTermSet RestoreTermSetSelf(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
            string termSetName = termSetInfo.Name;
            IAveTermSet termSet = null;
            IAveTermSet tempTermSet = null;
            bool isGetFromId = false;
            tempTermSet = GetTermSet(group, termSetInfo, ref isGetFromId);
            if (!isGetFromId && tempTermSet != null)
            {
                termSet = tempTermSet;
            }
            if (termSet == null)
            {
                termSet = CreateTermSet(group, termSetInfo);
                if (termSet == null)
                {
                    return null;
                }
            }
            return termSet;
        }

        public IAveTermSet RestoreTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo, bool isNewCreatedGroup)
        {
            return RestoreTermSet(group, termSetInfo, isNewCreatedGroup, null);
        }

        private Dictionary<IAveTermSet, Dictionary<int, string>> sysGroupTermSetNamesCache;
        private void InitialSysGroupTermSetNamesCache(IAveTaxonomyGroup group)
        {
            sysGroupTermSetNamesCache = new Dictionary<IAveTermSet, Dictionary<int, string>>();

            foreach (var ts in group.TermSets)
            {
                try
                {
                    sysGroupTermSetNamesCache.Add(ts, ts.Names);
                }
                catch (Exception ex)
                {
                    log.Debug("Failed to add sys group termSet to cache. Name: {0}, exception: {1}", ts.Name, ex);
                }
            }
        }

        public IAveTermSet GetTermSetUnderSystemGroup(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
            log.Debug("Try to get TermSet by all name in system group.");

            if (sysGroupTermSetNamesCache == null)
            {
                log.Debug("TermStore languages are:");
                foreach (var lan in group.TermStore.Languages)
                {
                    string msg = string.Empty;
                    if (group.TermStore.DefaultLanguage == lan)
                    {
                        msg = string.Format("{0} -> DefaultLanguage", lan);
                    }
                    else if (group.TermStore.WorkingLanguage == lan)
                    {
                        msg = string.Format("{0} -> WorkingLanguage", lan);
                    }
                    else
                    {
                        msg = lan.ToString();
                    }
                    log.Debug(msg);
                }
                InitialSysGroupTermSetNamesCache(group);
            }

            foreach (var tsPair in sysGroupTermSetNamesCache)
            {
                foreach (var lanPair in tsPair.Value)
                {
                    if (termSetInfo.Name.Equals(lanPair.Value, StringComparison.Ordinal))
                    {
                        log.Debug("Match name in system group termSet names cache, language: {0}", lanPair.Key);
                        return tsPair.Key;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Restore Term Set
        /// </summary>
        /// <param name="group"></param>
        /// <param name="termSetInfo"></param>
        /// <param name="isNewCreatedGroup"></param>
        /// <param name="cacheInfo"></param>
        /// <returns></returns>
        public IAveTermSet RestoreTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo, bool isNewCreatedGroup, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTermSet"))
            {
                try
                {

                    _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                    IAveTermSet termSet = null;
                    IAveTermSet tempTermSet = null;
                    bool isNewCreated = false;
                    bool isGetFromId = false;

                    //在目的端查找同名Term Set
                    if (!isNewCreatedGroup)
                    {
                        //find逻辑，通过name或者id找到对应的termset。其中是否通过id去找由IsGetTermSetFromId选项控制。
                        tempTermSet = GetTermSet(group, termSetInfo, ref isGetFromId);

                        //冲突判断逻辑：GetTermSet方法以先Name后ID的方式进行Find，Find成功为冲突，Find失败为不冲突即新建
                        if (tempTermSet != null)
                        {
                            termSet = tempTermSet;
                            if (isGetFromId)
                            {
                                log.Debug("Find termSet[{0}] by id successful", termSet.ID);
                            }
                        }
                    }
                    //新建Term Set
                    if (termSet == null)
                    {
                        //SystemGroup下不能创建TermSet
                        if (!group.IsSystemGroup)
                        {
                            termSet = CreateTermSet(group, termSetInfo);
                        }
                        else
                        {
                            log.Debug("It's not reasonable to create a termSet in a system group, group Name:{0}, termSet Name:{1}", group.Name, termSetInfo.Name);
                        }
                        if (termSet == null)
                        {
                            return null;
                        }
                        isNewCreated = true;

                    }
                    else if (mOption.RestoreTermSetAndTermProperties)
                    {
                        UpdateTermSetSetting(termSetInfo, termSet, false);
                    }
                    else if (termSetInfo.CustomProperties != null && termSetInfo.CustomProperties.Count > 0 &&
                        termSetInfo.CustomProperties.ContainsKey("_Sys_Nav_IsNavigationTermSet") &&
                        termSetInfo.CustomProperties["_Sys_Nav_IsNavigationTermSet"].Equals("True", StringComparison.OrdinalIgnoreCase) &&
                        mRestoreManagedMetadataNavigation)
                    {
                        foreach (KeyValuePair<string, string> pair in termSetInfo.CustomProperties)
                        {
                            //ADO-108755,需要对目的端的url进行下处理
                            if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase) && !pair.Key.StartsWith("_Sys_Nav_AttachedWeb", StringComparison.OrdinalIgnoreCase))
                            {
                                string realUrl = "";
                                if (pair.Value.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
                                {
                                    realUrl = mSPSite.ServerRelativeUrl + pair.Value.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
                                }
                                else
                                {
                                    realUrl = pair.Value;
                                }
                                termSet.SetCustomProperty(pair.Key, realUrl);
                            }
                        }

                        termSet.TermStore.CommitAll();
                        //isRestoreNavigationProperty = true;
                    }

                    if (ImportProfiler != null)
                    {
                        ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermSetRestored, termSet.Name), Status = WrapperRestoreStatus.Successful, Title = group.Name, Url = this.mSPSite.Url, Type = SPObjectType.TermSet });
                    }

                    foreach (AveTermInfo termInfo in termSetInfo.Terms)
                    {
                        lock (this)
                        {
                            try
                            {
                                IAveTerm term = RestoreTerm(termSet, termInfo, isNewCreated, cacheInfo);
                                if (term != null)
                                {
                                    SetTermIdMapping(termInfo, term);
                                    if (cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(termInfo.Id))
                                    {
                                        cacheInfo.TermIdMapping.Add(termInfo.Id, term.ID);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Warn("An error occurred while restoring term, TermName: {0}, Error: {1}", termInfo.Name, e);
                            }
                        }
                    }
                    //Children Changed，So Update
                    if (_currentTermSetCalculator.TermSetNeedCommit)
                    {
                        termSet.TermStore.CommitAll();
                    }
                    //对于已经存在的termSet不会去还原它的CustomSortOrder
                    if (isNewCreated)
                    {
                        //此处不需要进行对order的修剪处理，否则会产生ADO-124872的问题。
                        //var newCustomSortOrder = RestoreCustomSortOrder(termSetInfo.CustomSortOrder);
                        if (!string.Equals(termSetInfo.CustomSortOrder, termSet.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                        {
                            //当修改当前级别的属性和添加或者修改它sub的时候，最后CommitAll()的时候会抛 Term update failed because of save conflict这个错误。
                            //restoreterm和restoresubterm的修改同理
                            termSet.CustomSortOrder = termSetInfo.CustomSortOrder;
                            termSet.TermStore.CommitAll();
                        }
                    }

                    //isRestoreNavigationProperty = false;
                    return termSet;
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restoring term set.{0}", e);
                }
                return null;
            }

        }

        private IAveTermSet GetTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo, ref bool isGetFromId)
        {
            IAveTermSet termSet = null;
            try
            {
                termSet = group.TermSets[termSetInfo.Name];
                if (termSet == null)//如果目的端是365,Group.TermSets[""]方法不会抛出异常, 所以在这throw出去,走Catch里的处理逻辑。
                {
                    throw new ArgumentException(termSetInfo.Name);
                }
            }
            catch (Exception ex)
            {
                log.Debug("An error occurred while getting term set \"{0}\" by Name, {1}.", termSetInfo.Name, ex);
                if (group.IsSystemGroup)
                {
                    termSet = GetTermSetUnderSystemGroup(group, termSetInfo);
                }
                if (IsGetTermSetFromId)
                {
                    try
                    {
                        log.Debug("Start to get term set by Id \"{0}\".", termSetInfo.Id);
                        termSet = group.TermSets[termSetInfo.Id];
                        isGetFromId = true;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermSetByNameError, e.ToString());
                    }
                }
            }
            return termSet;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="customSortOrder"></param>
        /// <param name="termSet"></param>
        private string RestoreCustomSortOrder(string customSortOrder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreCustomSortOrder"))
            {
                string sortOrder = null;
                try
                {
                    if (!string.IsNullOrEmpty(customSortOrder))
                    {
                        string[] sortIds = customSortOrder.Split(':');
                        StringBuilder sb = new StringBuilder();
                        foreach (string sortId in sortIds)
                        {
                            Guid key = new Guid(sortId);
                            if (TermIdMapping.Keys.Contains(key))
                            {
                                sb.Append(TermIdMapping[key].ToString());
                                sb.Append(':');
                            }
                        }
                        string tempSortOrder = sb.ToString().TrimEnd(':');
                        if (!string.IsNullOrEmpty(tempSortOrder))
                        {
                            sortOrder = tempSortOrder;
                        }
                    }
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_RestoreCustomOrderError, e.Message));
                    log.Warn("An error occurred while restoring custom sort order. Error:{0}", e);
                }
                return sortOrder;

            }

        }

        /// <summary>
        /// Create Term Set and Restore Property
        /// </summary>
        /// <param name="group"></param>
        /// <param name="termSetInfo"></param>
        /// <returns></returns>
        public IAveTermSet CreateTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateTermSet"))
            {
                try
                {
                    //保持termset ID和源端一致，如果Id被占用在CreateTermSet方法中会判断
                    IAveTermSet termSet = group.CreateTermSet(termSetInfo.Name, termSetInfo.Id);
                    return UpdateTermSetSetting(termSetInfo, termSet, true);
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateTermSetError, termSetInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while create termSet. termSet Name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
                    return null;
                }

            }

        }

        private IAveTermSet UpdateTermSetSetting(AveTermSetInfo termSetInfo, IAveTermSet termSet, bool isNewCreated)
        {
            bool change = isNewCreated;
            try
            {
                if (!string.Equals(termSet.Description, termSetInfo.Description, StringComparison.OrdinalIgnoreCase))
                {
                    termSet.Description = termSetInfo.Description;
                    change = true;
                }


                string owner = termSetInfo.Owner;
                string mappedOwnerName = owner;
                try
                {
                    if (!string.Equals(termSet.Owner, termSetInfo.Owner, StringComparison.OrdinalIgnoreCase))
                    {
                        mappedOwnerName = mMenbers.GetMappingUserLogin(owner);
                        IAvePrincipalInfo info = objectModelFactory.Utility.ResolvePrincipal(mSPSite.RootWeb, mappedOwnerName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                        if (info != null)
                        {
                            owner = GetUserRealLoginName(info.LoginName, objectModelFactory);
                            termSet.Owner = owner;
                        }
                        change = true;
                    }
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetTermSetOwnerError, termSetInfo.Name, owner, e.Message));
                    log.Warn("An error occurred while set term set owner. term set:{0}, owner:{1}.error:{2}", termSetInfo.Name, mappedOwnerName, e.ToString());
                }

                if (!string.Equals(termSet.Contact, termSetInfo.Contact, StringComparison.OrdinalIgnoreCase))
                {
                    termSet.Contact = termSetInfo.Contact;
                    change = true;
                }

                if (termSet.IsOpenForTermCreation != termSetInfo.IsOpenForTermCreation)
                {
                    termSet.IsOpenForTermCreation = termSetInfo.IsOpenForTermCreation;
                    change = true;
                }

                if (termSet.IsAvailableForTagging != termSetInfo.IsAvailableForTagging)
                {
                    termSet.IsAvailableForTagging = termSetInfo.IsAvailableForTagging;
                    change = true;
                }

                foreach (string stakeHolder in termSetInfo.Stakeholders)
                {
                    string tStakeHolder = stakeHolder;
                    string mappedStakeHolderName = stakeHolder;
                    try
                    {
                        mappedStakeHolderName = mMenbers.GetMappingUserLogin(stakeHolder);
                        tStakeHolder = GetUserRealLoginName(mappedStakeHolderName, objectModelFactory);
                        if (!termSet.Stakeholders.Contains(tStakeHolder))
                        {
                            termSet.AddStakeholder(tStakeHolder);
                            change = true;
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_AddTermSetStakeholderError, termSetInfo.Name, stakeHolder, e.Message));
                        log.Warn("An error occurred while add term set stakeholder. term set:{0}, stakeholder:{1}. error:{2}.", termSetInfo.Name, mappedStakeHolderName, e.ToString());
                    }
                }

                #region CustomProperties
                var targetCustomProperties = termSet.CustomProperties;
                if (termSetInfo.CustomProperties != null && termSetInfo.CustomProperties.Count > 0)
                {
                    try
                    {
                        Dictionary<string, string> needReplaceUrlInPostAction = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, string> pair in termSetInfo.CustomProperties)
                        {
                            //ADO-108755,需要对目的端的url进行下处理
                            if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!pair.Key.StartsWith("_Sys_Nav_AttachedWeb", StringComparison.OrdinalIgnoreCase))
                                {
                                    needReplaceUrlInPostAction.Add(pair.Key, pair.Value);
                                    //string newValue = ReplaceTermSetOrTermUrlProperty(pair.Value);
                                    //termSet.SetCustomProperty(pair.Key, newValue);
                                    //change = true;
                                }
                            }
                            else
                            {
                                if (!targetCustomProperties.ContainsKey(pair.Key))
                                {
                                    termSet.SetCustomProperty(pair.Key, pair.Value);
                                    change = true;
                                }
                                else
                                {
                                    var sValue = string.IsNullOrEmpty(pair.Value) ? string.Empty : pair.Value;
                                    if (!sValue.Equals(targetCustomProperties[pair.Key]))
                                    {
                                        termSet.SetCustomProperty(pair.Key, sValue);
                                        change = true;
                                    }
                                }
                            }
                        }
                        if (needReplaceUrlInPostAction.Count > 0)
                        {
                            this.mSiteMappingManager.AddMetadataNeedReplaceUrlPropertyTermOrTermSet(termSet.TermStore.ID, termSet.ID, Guid.Empty, needReplaceUrlInPostAction);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn(string.Format("An error occurred while add term set property. term set: {0}, error: {1}", termSetInfo.Name, ex.ToString()));
                    }
                }
                #endregion

                if (change)
                {
                    termSet.TermStore.CommitAll();
                }
            }
            catch (Exception e)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetTermSetPropertyError, termSetInfo.Name, e.Message));
                log.Warn(string.Format("An error occurred while set term set property. term set name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
            }

            return termSet;
        }

        //只还原term，不还原下属subterm
        public IAveTerm RestoreTermSelf(IAveTermSet termSet, AveTermInfo termInfo)
        {
            string termName = termInfo.Name;
            IAveTerm term = null;
            try
            {
                term = termSet.Terms[termName];
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                term = CreateTerm(termSet, termInfo);
                if (term == null)
                {
                    return null;
                }
            }
            return term;
        }

        public IAveTerm RestoreTerm(IAveTermSet termSet, AveTermInfo termInfo, bool isNewCreatedTermSet)
        {
            return RestoreTerm(termSet, termInfo, isNewCreatedTermSet, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termSet"></param>
        /// <param name="termInfo"></param>
        /// <param name="isNewCreatedTermSet"></param>
        /// <param name="cacheInfo"></param>
        /// <returns></returns>
        public IAveTerm RestoreTerm(IAveTermSet termSet, AveTermInfo termInfo, bool isNewCreatedTermSet, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTerm"))
            {
                string termName = termInfo.Name;
                IAveTerm term = null;
                bool isNewCreated = false;
                bool isTermHasBeenRestored = false;
                if (termInfo.IsPinned)
                {
                    if (!termInfo.PinSourceTermSetId.Equals(Guid.Empty))
                    {
                        //Pin Term 需要等Source Term还原以后再创建
                        Tuple<Guid, Guid, Guid, Guid> temp = new Tuple<Guid, Guid, Guid, Guid>(termInfo.Id, Guid.Empty, termInfo.ParentTermSetId, termInfo.PinSourceTermSetId);
                        PinIdInfos.Add(temp);
                        PinIdToTermInfoMapping[termInfo.Id] = termInfo;
                    }
                    return null;
                }
                if (!isNewCreatedTermSet)
                {
                    var desTermId = Guid.Empty;
                    if (TermIdMapping.TryGetValue(termInfo.Id, out desTermId)) // The term has been restored
                    {
                        //ADO-148347 Reuse Term 由于与Source Term Id 相同，因此需要特殊判断
                        if (termInfo.Terms.Count > 0 || termInfo.IsReused)
                        {
                            try
                            {
                                term = termSet.Terms[desTermId];
                                isTermHasBeenRestored = true;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e);
                                //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                                term = FindTermByLabel(termSet.Terms, termInfo, termSet.TermStore);
                            }
                        }
                        else
                        {
                            log.Debug("The source term '{0}' has no children", termInfo.Name);
                            return null;// There is no sub terms, not need to continue.
                        }
                    }
                    else
                    {
                        try
                        {
                            term = termSet.Terms[termName];
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e);
                            //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                            term = FindTermByLabel(termSet.Terms, termInfo, termSet.TermStore);
                        }
                    }
                }

                if (term == null)
                {
                    term = CreateTerm(termSet, termInfo);
                    if (term == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
                else
                {
                    if (mOption.RestoreTermSetAndTermProperties && !isTermHasBeenRestored)
                    {
                        UpdateTermSetting(termInfo, term, isNewCreated);
                    }
                }

                AddTermIDMappingForPinTerm(termInfo.Id, term.ID, isNewCreated);

                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermRestored, term.Name), Status = WrapperRestoreStatus.Successful, Title = term.Name, Url = this.mSPSite.Url, Type = SPObjectType.Term });
                }

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    IAveTerm sTerm = RestoreSubTerm(term, subTerm, isNewCreated, cacheInfo);
                    if (sTerm != null)
                    {
                        SetTermIdMapping(subTerm, sTerm);
                        if (cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                        {
                            cacheInfo.TermIdMapping.Add(subTerm.Id, sTerm.ID);
                        }
                    }
                }

                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    //var newCustomSortOrder = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(termInfo.CustomSortOrder, term.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        term.CustomSortOrder = termInfo.CustomSortOrder;
                        term.TermStore.CommitAll();
                    }
                }
                return term;

            }

        }

        private IAveTerm FindTermByLabel(IAveTermCollection terms, AveTermInfo termInfo, IAveTermStore termStore)
        {
            IAveTerm term = null;
            try
            {
                string tempName = termInfo.Name;
                foreach (AveLableInfo labelInfo in termInfo.Labels)
                {
                    if (labelInfo.Language == termStore.DefaultLanguage && labelInfo.IsDefaultForLanguage)
                    {
                        tempName = labelInfo.Value;
                        break;
                    }
                }
                term = terms[tempName];
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, ex);
            }
            return term;
        }

        public IAveTerm RestorePinTermToNormalTerm(IAveTermSet termSet, AveTermInfo termInfo, bool isNewCreatedTermSet, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTerm"))
            {
                string termName = termInfo.Name;
                IAveTerm term = null;
                bool isNewCreated = false;
                if (!isNewCreatedTermSet)
                {
                    try
                    {
                        term = termSet.Terms[termName];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                        //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                        try
                        {
                            string tempName = termInfo.Name;
                            foreach (AveLableInfo labelInfo in termInfo.Labels)
                            {
                                if (labelInfo.Language == termSet.TermStore.DefaultLanguage && labelInfo.IsDefaultForLanguage)
                                {
                                    tempName = labelInfo.Value;
                                    break;
                                }
                            }
                            term = termSet.Terms[tempName];
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, ex);
                        }
                    }
                }
                if (term == null)
                {
                    term = CreatePinTermToNormalTerm(termSet, termInfo);
                    if (term == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
                else
                {
                    if (mOption.RestoreTermSetAndTermProperties)
                    {
                        UpdateTermSetting(termInfo, term, isNewCreated);
                    }
                }

                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermRestored, term.Name), Status = WrapperRestoreStatus.Successful, Title = term.Name, Url = this.mSPSite.Url, Type = SPObjectType.Term });
                }

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    IAveTerm sTerm = RestoreSubPinTermToNormalTerm(term, subTerm, isNewCreated, cacheInfo);
                    if (sTerm != null)
                    {
                        SetTermIdMapping(subTerm, sTerm);
                        if (cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                        {
                            cacheInfo.TermIdMapping.Add(subTerm.Id, sTerm.ID);
                        }
                    }
                }
                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    //var newCustomSortOrder = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(termInfo.CustomSortOrder, term.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        term.CustomSortOrder = termInfo.CustomSortOrder;
                        term.TermStore.CommitAll();
                    }
                }
                //if (_currentTermSetCalculator.TermNeedCommit(termInfo.Id, false))
                //{
                //    term.TermStore.CommitAll();
                //}
                return term;

            }

        }

        public IAveTerm CreateTerm(IAveTermSet termSet, AveTermInfo termInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateTerm"))
            {
                try
                {
                    IAveTerm term = null;
                    if (termInfo.IsReused)
                    {
                        //如果本组ReusedTerm已经有还原过的了，新的就靠ReuseTerm()这个API创建
                        if (TermIdMapping != null && TermIdMapping.ContainsKey(termInfo.Id))
                        {
                            Guid sourceTermId = TermIdMapping[termInfo.Id];
                            IAveTerm sourceTerm = termSet.TermStore.GetTerm(sourceTermId);

                            term = termSet.ReuseTerm(sourceTerm, false);
                            //如果现在还原的这个是源，更新本组所有ReusedTerm的SourceTerm为当前Term
                            if (termInfo.IsSourceTerm)
                            {
                                term.SourceTerm.ReassignSourceTerm(term);
                            }
                            RestoreReusedTermNavigation(term, termInfo.LocalCustomProperties);
                            RestoreReusedTermProperties(term, termInfo);
                            term.TermStore.CommitAll();
                            return term;
                        }
                        else
                        {
                            term = termSet.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                        }
                    }
                    else
                    {
                        //保持term ID和源端一致，如果Id被占用在CreateTerm方法中会判断
                        term = termSet.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                    }
                    UpdateTermSetting(termInfo, term, true);
                    term.TermStore.CommitAll();

                    return term;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateTermError, termInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while Create term. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }

            }

        }

        public IAveTerm CreatePinTermToNormalTerm(IAveTermSet termSet, AveTermInfo termInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateTerm"))
            {
                try
                {
                    IAveTerm term = null;
                    term = termSet.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                    try
                    {
                        SetTermDescription(term, termInfo.Description);

                        string owner = termInfo.Owner;
                        string mappedOwner = owner;
                        try
                        {
                            mappedOwner = mMenbers.GetMappingUserLogin(owner);
                            IAvePrincipalInfo info = objectModelFactory.Utility.ResolvePrincipal(mSPSite.RootWeb, mappedOwner, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                            if (info != null)
                            {
                                owner = GetUserRealLoginName(info.LoginName, objectModelFactory);
                                term.Owner = owner;
                            }
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetTermOwnerError, termInfo.Name, owner, e.Message));
                            log.Warn("An error occurred while set term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, mappedOwner, e.ToString());
                        }

                        term.IsAvailableForTagging = termInfo.IsAvailableForTagging;

                        //Deprecate status and labels are shared between reused terms （10 is the same behavior？）
                        term.Deprecate(termInfo.IsDeprecated);
                        foreach (AveLableInfo labelInfo in termInfo.Labels)
                        {
                            if (labelInfo.Value.Equals(term.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            term.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                        }

                        if (termInfo.CustomProperties != null && termInfo.CustomProperties.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> pair in termInfo.CustomProperties)
                            {
                                term.SetCustomProperty(pair.Key, pair.Value);
                            }
                        }

                        if (termInfo.LocalCustomProperties != null)
                        {
                            foreach (KeyValuePair<string, string> pair in termInfo.LocalCustomProperties)
                            {
                                string realUrl = "";
                                if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (pair.Value.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        realUrl = mSPSite.ServerRelativeUrl + pair.Value.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
                                        term.ChangedLCPSourceValue.Add(pair.Key, pair.Value);
                                    }
                                    else
                                    {
                                        realUrl = pair.Value;
                                    }
                                }
                                else
                                {
                                    realUrl = pair.Value;
                                }
                                term.SetLocalCustomProperty(pair.Key, realUrl);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetTermPropertyError, termInfo.Name, e.Message));
                        log.Warn(string.Format("An error occurred while set term property. termName:{0}, error:{1}", termInfo.Name, e.ToString()));
                    }

                    term.TermStore.CommitAll();
                    return term;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateTermError, termInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while Create term. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }

            }
        }

        //只还原单个subterm，不还原其下属subterms
        public IAveTerm RestoreSubTermSelf(IAveTerm term, AveTermInfo termInfo)
        {
            string termName = termInfo.Name;
            IAveTerm sTerm = null;
            try
            {
                sTerm = term.Terms[termName];
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                sTerm = CreateSubTerm(term, termInfo);
                if (sTerm == null)
                {
                    return null;
                }
            }
            return sTerm;
        }

        public IAveTerm RestoreSubTerm(IAveTerm term, AveTermInfo termInfo, bool isNewCreatedTerm)
        {
            return RestoreSubTerm(term, termInfo, isNewCreatedTerm, null);
        }

        public IAveTerm RestoreSubTerm(IAveTerm term, AveTermInfo termInfo, bool isNewCreatedTerm, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreSubTerm"))
            {
                string termName = termInfo.Name;
                IAveTerm sTerm = null;
                bool isNewCreated = false;
                bool isSubTermHasBeenRestored = false;
                if (termInfo.IsPinned)
                {
                    if (!termInfo.PinSourceTermSetId.Equals(Guid.Empty))
                    {
                        Tuple<Guid, Guid, Guid, Guid> temp = new Tuple<Guid, Guid, Guid, Guid>(termInfo.Id, termInfo.ParentTermId, termInfo.ParentTermSetId, termInfo.PinSourceTermSetId);
                        PinIdInfos.Add(temp);
                        PinIdToTermInfoMapping[termInfo.Id] = termInfo;
                    }
                    return null;
                }
                if (!isNewCreatedTerm)
                {
                    var desTermId = Guid.Empty;
                    if (TermIdMapping.TryGetValue(termInfo.Id, out desTermId))
                    {
                        //ADO-148347 Reuse Term 由于与Source Term Id 相同，因此需要特殊判断
                        if (termInfo.Terms.Count > 0 || termInfo.IsReused)
                        {
                            try
                            {
                                sTerm = term.Terms[desTermId];
                                isSubTermHasBeenRestored = true;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e);
                                //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                                sTerm = FindTermByLabel(term.Terms, termInfo, term.TermStore);
                            }
                        }
                        else
                        {
                            log.Debug("The source term '{0}' has no children", termInfo.Name);
                            return null;// There is no sub terms, not need to continue.
                        }
                    }
                    else
                    {
                        try
                        {
                            sTerm = term.Terms[termName];
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                            //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                            sTerm = FindTermByLabel(term.Terms, termInfo, term.TermStore);
                        }
                    }
                }
                if (sTerm == null)
                {
                    sTerm = CreateSubTerm(term, termInfo);
                    if (sTerm == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
                else
                {
                    if (mOption.RestoreTermSetAndTermProperties && !isSubTermHasBeenRestored)
                    {
                        UpdateTermSetting(termInfo, sTerm, isNewCreated);
                    }

                }

                AddTermIDMappingForPinTerm(termInfo.Id, sTerm.ID, isNewCreated);

                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermRestored, term.Name), Status = WrapperRestoreStatus.Successful, Title = term.Name, Url = this.mSPSite.Url, Type = SPObjectType.Term });
                }

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    IAveTerm ssTerm = RestoreSubTerm(sTerm, subTerm, isNewCreated); // ??? why no cacheInfo
                    if (ssTerm != null)
                    {
                        SetTermIdMapping(subTerm, ssTerm);
                        if (cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                        {
                            cacheInfo.TermIdMapping.Add(subTerm.Id, ssTerm.ID);
                        }
                    }
                }
                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    //var newCustomSortOrder = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(termInfo.CustomSortOrder, sTerm.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        sTerm.CustomSortOrder = termInfo.CustomSortOrder;
                        sTerm.TermStore.CommitAll();
                    }
                }
                //if (_currentTermSetCalculator.TermNeedCommit(termInfo.Id, false))
                //{
                //    sTerm.TermStore.CommitAll();
                //}
                return sTerm;

            }

        }

        private void AddTermIDMappingForPinTerm(Guid sourceId, Guid destId, bool isNewCreated)
        {
            if (PinSourceIdMapping != null && !PinSourceIdMapping.ContainsKey(sourceId))
            {
                // 不等用于判断是否是还原过来的term ，特殊情况，job 两端都在同一个term store 下，还过来的term id 也不同，需要再用overwrite option 判断是否保留目的端还是用源端的pin 结构。
                if (!isNewCreated && !(destId.ToString().Equals(sourceId.ToString(), StringComparison.OrdinalIgnoreCase)) && !mOption.RestoreTermSetAndTermProperties)
                {
                    PinSourceIdMapping.Add(sourceId, Guid.Empty);
                }
                else
                {
                    PinSourceIdMapping.Add(sourceId, destId);
                }
            }
        }

        public IAveTerm RestoreSubPinTermToNormalTerm(IAveTerm term, AveTermInfo termInfo, bool isNewCreatedTerm, AveTermStoreCacheInfo cacheInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreSubTerm"))
            {
                string termName = termInfo.Name;
                IAveTerm sTerm = null;
                bool isNewCreated = false;
                if (!isNewCreatedTerm)
                {
                    try
                    {
                        sTerm = term.Terms[termName];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                        //防止多语言情况下因修改目的端Term的Label导致无法用Name find到的问题
                        try
                        {
                            string tempName = termName;
                            foreach (AveLableInfo labelInfo in termInfo.Labels)
                            {
                                if (labelInfo.Language == term.TermStore.DefaultLanguage && labelInfo.IsDefaultForLanguage)
                                {
                                    tempName = labelInfo.Value;
                                    break;
                                }
                            }
                            sTerm = term.Terms[tempName];
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, ex);
                        }
                    }
                }
                if (sTerm == null)
                {
                    sTerm = CreateSubPinTermToNormalTerm(term, termInfo);
                    if (sTerm == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }
                else
                {
                    if (mOption.RestoreTermSetAndTermProperties)
                    {
                        UpdateTermSetting(termInfo, sTerm, isNewCreated);
                    }
                }

                if (ImportProfiler != null)
                {
                    ImportProfiler.OnStatusChanged(new SPImportEventArgs() { Level = SPObjectLevel.SiteCollection, Message = WrapperResource.GetString(WrapperResourceKey.Wrapper_TermRestored, term.Name), Status = WrapperRestoreStatus.Successful, Title = term.Name, Url = this.mSPSite.Url, Type = SPObjectType.Term });
                }

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    try
                    {
                        IAveTerm ssTerm = RestoreSubPinTermToNormalTerm(sTerm, subTerm, isNewCreated, null); // ??? why no cacheInfo
                        if (ssTerm != null)
                        {
                            SetTermIdMapping(subTerm, ssTerm);
                            if (cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                            {
                                cacheInfo.TermIdMapping.Add(subTerm.Id, ssTerm.ID);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while restoring sub term, TermName: {0}, Error: {1}", subTerm.Name, e);
                    }
                }

                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    //var newCustomSortOrder = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(termInfo.CustomSortOrder, sTerm.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        sTerm.CustomSortOrder = termInfo.CustomSortOrder;
                        sTerm.TermStore.CommitAll();
                    }
                }
                //if (_currentTermSetCalculator.TermNeedCommit(termInfo.Id, false))
                //{
                //    sTerm.TermStore.CommitAll();
                //}
                return sTerm;

            }

        }

        public IAveTerm CreateSubTerm(IAveTerm term, AveTermInfo termInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateSubTerm"))
            {
                try
                {
                    IAveTerm sTerm = null;
                    if (termInfo.IsReused)
                    {
                        if (TermIdMapping != null && TermIdMapping.ContainsKey(termInfo.Id))
                        {
                            //reused的term在源端的term id是一样的，如果直接新建会导致TermIdMapping中的关系被覆盖，此处实现还原reused term状态
                            Guid sourceTermId = TermIdMapping[termInfo.Id];
                            IAveTerm sourceTerm = term.TermStore.GetTerm(sourceTermId);
                            sTerm = term.ReuseTerm(sourceTerm, false);

                            if (termInfo.IsSourceTerm)
                            {
                                sTerm.SourceTerm.ReassignSourceTerm(term);
                            }
                            RestoreReusedTermNavigation(sTerm, termInfo.LocalCustomProperties);
                            RestoreReusedTermProperties(sTerm, termInfo);
                            sTerm.TermStore.CommitAll();
                            return sTerm;
                        }
                        else
                        {
                            sTerm = term.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                        }
                    }
                    else
                    {
                        //保持term ID和源端一致，如果Id被占用在CreateTerm方法中会判断
                        sTerm = term.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                    }
                    UpdateTermSetting(termInfo, sTerm, true);

                    sTerm.TermStore.CommitAll();

                    return sTerm;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateSubTermError, termInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while create subTerm. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }

            }

        }

        private void UpdateTermSetting(AveTermInfo termInfo, IAveTerm targetTerm, bool isNewCreated)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting"))
            {
                try
                {
                    bool change = false;
                    using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.Description"))
                    {
                        if (termInfo.Description != null && termInfo.Description.Count > 0)
                        {
                            string description;
                            foreach (int lcid in targetTerm.TermStore.Languages)
                            {
                                if (isNewCreated)
                                {
                                    if (termInfo.Description.TryGetValue(lcid, out description) && !string.IsNullOrEmpty(description))
                                    {
                                        targetTerm.SetDescription(description, lcid);
                                        change = true;
                                    }
                                }
                                else if (termInfo.Description.TryGetValue(lcid, out description) && string.Compare(description, targetTerm.GetDescription(lcid), StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    targetTerm.SetDescription(description, lcid);
                                    change = true;
                                }
                            }
                        }
                    }

                    #region Owner
                    string owner = termInfo.Owner;
                    try
                    {
                        using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.Owner"))
                        {
                            if (!string.IsNullOrEmpty(owner) && !owner.Equals(targetTerm.Owner, StringComparison.OrdinalIgnoreCase))
                            {
                                string tempOwner = string.Empty;
                                if (!OwnerMapping.TryGetValue(owner, out tempOwner))
                                {
                                    string mappedOwnerName = mMenbers.GetMappingUserLogin(owner);
                                    IAvePrincipalInfo info = objectModelFactory.Utility.ResolvePrincipal(mSPSite.RootWeb, mappedOwnerName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                                    if (info == null)
                                    {
                                        OwnerMapping.Add(owner, string.Empty);
                                    }
                                    else
                                    {
                                        tempOwner = GetUserRealLoginName(info.LoginName, objectModelFactory);
                                        OwnerMapping.Add(owner, tempOwner);
                                    }
                                }
                                if (!string.IsNullOrEmpty(tempOwner))//Do not update owner to destination if can not find source owner in destination.
                                {
                                    targetTerm.Owner = tempOwner;
                                    change = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetSubTermOwnerError, termInfo.Name, owner, e.Message));
                        log.Warn("An error occurred while set sub term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e);
                    }
                    #endregion


                    if (termInfo.IsAvailableForTagging != targetTerm.IsAvailableForTagging)
                    {
                        targetTerm.IsAvailableForTagging = termInfo.IsAvailableForTagging;
                        change = true;
                    }

                    if (termInfo.IsDeprecated != targetTerm.IsDeprecated)
                    {
                        targetTerm.Deprecate(termInfo.IsDeprecated);
                        change = true;
                    }

                    #region Lable
                    using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.Lable"))
                    {
                        IAveLabelCollection lableCollection = targetTerm.Labels;
                        foreach (AveLableInfo labelInfo in termInfo.Labels)
                        {
                            bool needCreate = true;
                            if (lableCollection != null)
                            {
                                foreach (var lable in lableCollection)
                                {
                                    if (lable.Language == labelInfo.Language && lable.Value.Equals(labelInfo.Value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        needCreate = false;
                                        break;
                                    }
                                }
                            }
                            if (needCreate && targetTerm.TermStore.Languages.Contains(labelInfo.Language))
                            {
                                targetTerm.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                                change = true;
                            }
                        }
                    }
                    #endregion

                    #region CustomProperties
                    using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.CustomProperties"))
                    {
                        var targetCustomProperties = targetTerm.CustomProperties;
                        if (termInfo.CustomProperties != null && termInfo.CustomProperties.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> pair in termInfo.CustomProperties)
                            {
                                if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase) && pair.Key.StartsWith("_Sys_Nav_AttachedWeb", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                if (!targetCustomProperties.ContainsKey(pair.Key))
                                {
                                    targetTerm.SetCustomProperty(pair.Key, pair.Value);
                                    change = true;
                                }
                                else
                                {
                                    var sValue = string.IsNullOrEmpty(pair.Value) ? string.Empty : pair.Value;
                                    if (!sValue.Equals(targetCustomProperties[pair.Key]))
                                    {
                                        targetTerm.SetCustomProperty(pair.Key, sValue);
                                        change = true;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region LocalCustomProperties
                    using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.LocalCustomProperties"))
                    {
                        if (termInfo.LocalCustomProperties != null)
                        {

                            #region Special properties
                            //ADO-122707,这个属性比较特殊，别的都是包含的，而这个属性选的是不包含的。
                            if (!termInfo.LocalCustomProperties.ContainsKey("_Sys_Nav_ExcludedProviders") && targetTerm.LocalCustomProperties.ContainsKey("_Sys_Nav_ExcludedProviders"))
                            {
                                targetTerm.DeleteLocalCustomProperty("_Sys_Nav_ExcludedProviders");
                                change = true;
                            }

                            //ADO-129407,这个属性和界面上的下边那个是一组，如果选下边那个那么这个属性就为空。
                            if (!termInfo.LocalCustomProperties.ContainsKey("_Sys_Nav_SimpleLinkUrl") && targetTerm.LocalCustomProperties.ContainsKey("_Sys_Nav_SimpleLinkUrl"))
                            {
                                targetTerm.DeleteLocalCustomProperty("_Sys_Nav_SimpleLinkUrl");
                                change = true;
                            }
                            #endregion

                            var targetLocalCustomProperty = targetTerm.LocalCustomProperties;
                            Dictionary<string, string> needReplaceUrlInPostAction = new Dictionary<string, string>();
                            foreach (KeyValuePair<string, string> pair in termInfo.LocalCustomProperties)
                            {
                                if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                                {
                                    needReplaceUrlInPostAction.Add(pair.Key, pair.Value);
                                    //string newValue = ReplaceTermSetOrTermUrlProperty(pair.Value);
                                    //targetTerm.SetLocalCustomProperty(pair.Key, newValue);
                                    //change = true;
                                }
                                else
                                {
                                    string sValue = string.IsNullOrEmpty(pair.Value) ? string.Empty : pair.Value;
                                    string targetValue = null;
                                    if (targetLocalCustomProperty != null && targetLocalCustomProperty.TryGetValue(pair.Key, out targetValue))
                                    {
                                        if (!sValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase))
                                        {
                                            targetTerm.SetLocalCustomProperty(pair.Key, sValue);
                                            change = true;
                                        }
                                    }
                                    else
                                    {
                                        targetTerm.SetLocalCustomProperty(pair.Key, pair.Value);
                                        change = true;
                                    }
                                }
                            }
                            if (needReplaceUrlInPostAction.Count > 0)
                            {
                                this.mSiteMappingManager.AddMetadataNeedReplaceUrlPropertyTermOrTermSet(targetTerm.TermStore.ID, targetTerm.TermSet.ID, targetTerm.ID, needReplaceUrlInPostAction);
                            }
                        }
                    }
                    #endregion

                    if (change)
                    {
                        using (AvePerformanceScope a = new AvePerformanceScope("Restore.AveMetadataService.UpdateTermSetting.CommitAll"))
                        {
                            targetTerm.TermStore.CommitAll();
                        }
                    }
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetSubTermPropertyError, termInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while set subTerm property. subTermName:{0}, error:{1}", termInfo.Name, e));
                }
            }
        }

        private string ReplaceTermSetOrTermUrlProperty(string oldValue)
        {
            foreach (KeyValuePair<string, string> webMap in mSiteMappingManager.WebUrlMapping)
            {
                if (oldValue.StartsWith(webMap.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return webMap.Value + oldValue.Substring(webMap.Key.Length);
                }
            }
            if (oldValue.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
            {
                return this.mSPSite.ServerRelativeUrl + oldValue.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
            }
            return AveReplaceProcessor.UrlReplace(oldValue, mSiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mSiteMappingManager.SourceSiteInfo, mSiteMappingManager.DestSiteInfo.ServerRelativeUrl);
        }

        private void SetTermDescription(IAveTerm term, Dictionary<int, string> descriptionDic)
        {
            if (descriptionDic != null && descriptionDic.Count > 0)
            {
                string description;
                foreach (int lcid in term.TermStore.Languages)
                {
                    if (descriptionDic.TryGetValue(lcid, out description) && !String.IsNullOrEmpty(description))
                    {
                        term.SetDescription(description, lcid);
                    }
                }
            }
        }

        public IAveTerm CreateSubPinTermToNormalTerm(IAveTerm term, AveTermInfo termInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateSubTerm"))
            {
                try
                {
                    IAveTerm sTerm = null;
                    sTerm = term.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                    try
                    {
                        SetTermDescription(term, termInfo.Description);
                        string owner = termInfo.Owner;
                        string mappedOwner = owner;
                        try
                        {
                            mappedOwner = mMenbers.GetMappingUserLogin(owner);
                            IAvePrincipalInfo info = objectModelFactory.Utility.ResolvePrincipal(mSPSite.RootWeb, mappedOwner, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false, false);
                            if (info != null)
                            {
                                owner = GetUserRealLoginName(info != null ? info.LoginName : "", objectModelFactory);
                                sTerm.Owner = owner;
                            }
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetSubTermOwnerError, termInfo.Name, owner, e.Message));
                            log.Warn("An error occurred while set sub term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, mappedOwner, e);
                        }

                        sTerm.IsAvailableForTagging = termInfo.IsAvailableForTagging;

                        sTerm.Deprecate(termInfo.IsDeprecated);
                        foreach (AveLableInfo labelInfo in termInfo.Labels)
                        {
                            if (labelInfo.Value.Equals(sTerm.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            sTerm.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                        }

                        if (termInfo.CustomProperties != null && termInfo.CustomProperties.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> pair in termInfo.CustomProperties)
                            {
                                sTerm.SetCustomProperty(pair.Key, pair.Value);
                            }
                        }

                        if (termInfo.LocalCustomProperties != null)
                        {
                            foreach (KeyValuePair<string, string> pair in termInfo.LocalCustomProperties)
                            {
                                //sTerm.SetLocalCustomProperty(pair.Key, pair.Value);
                                string realUrl = "";
                                if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (pair.Value.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        realUrl = mSPSite.ServerRelativeUrl + pair.Value.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
                                        sTerm.ChangedLCPSourceValue.Add(pair.Key, pair.Value);
                                    }
                                    else
                                    {
                                        realUrl = pair.Value;
                                    }
                                }
                                else
                                {
                                    realUrl = pair.Value;
                                }
                                sTerm.SetLocalCustomProperty(pair.Key, realUrl);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Skipped, AveReportResource.Wrapper_Report_SetSubTermPropertyError, termInfo.Name, e.Message));
                        log.Warn(string.Format("An error occurred while set subTerm property. subTermName:{0}, error:{1}", termInfo.Name, e.ToString()));
                    }

                    sTerm.TermStore.CommitAll();

                    return sTerm;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.MetadataService, AveStatus.Failed, AveReportResource.Wrapper_Report_CreateSubTermError, termInfo.Name, e.Message));
                    log.Warn(string.Format("An error occurred while create subTerm. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }

            }

        }

        /// <summary>
        /// 还原所有 Pin Term
        /// </summary>
        /// <param name="termStore"></param>
        public void PostProcessPinTerms(IAveTermStore termStore)
        {
            if (PinIdInfos != null)
            {
                foreach (Tuple<Guid, Guid, Guid, Guid> pinInfo in PinIdInfos)
                {
                    if (pinInfo.Item1 != Guid.Empty && (pinInfo.Item2 != Guid.Empty || pinInfo.Item3 != Guid.Empty) && pinInfo.Item4 != Guid.Empty)
                    {
                        Guid pinTermIdAfterMapping = PinSourceIdMapping.ContainsKey(pinInfo.Item1) ? PinSourceIdMapping[pinInfo.Item1] : Guid.Empty;
                        Guid pinParentTermIdAfterMapping = TermIdMapping.ContainsKey(pinInfo.Item2) ? TermIdMapping[pinInfo.Item2] : Guid.Empty;
                        Guid pinParentTermSetIdAfterMapping = TermSetIdMapping.ContainsKey(pinInfo.Item3) ? TermSetIdMapping[pinInfo.Item4] : Guid.Empty;
                        Guid SourceTermSetIdAfterMapping = TermSetIdMapping.ContainsKey(pinInfo.Item4) ? TermSetIdMapping[pinInfo.Item4] : Guid.Empty;

                        IAveTerm parentTerm = termStore.GetTerm(pinParentTermSetIdAfterMapping, pinParentTermIdAfterMapping);
                        IAveTermSet parentTermSet = termStore.GetTermSet(pinParentTermSetIdAfterMapping);
                        //区分是还原成pin的term;还是还原成normal term
                        if (pinTermIdAfterMapping != Guid.Empty)
                        {
                            //判断是否还原这个PinTerm，若是还原则连着Properties一起还原；若是不还原，则根据配置文件中的属性来判断是否还原Properties
                            if (isTermRestore(pinTermIdAfterMapping, pinParentTermIdAfterMapping, pinParentTermSetIdAfterMapping, PinIdToTermInfoMapping[pinInfo.Item1].Name, termStore))
                            {
                                IAveTerm pinTerm = null;
                                if (parentTerm != null)
                                {
                                    pinTerm = parentTerm.PinTerm(termStore.GetTerm(SourceTermSetIdAfterMapping, pinTermIdAfterMapping));
                                }
                                else if (parentTermSet != null)
                                {
                                    pinTerm = parentTermSet.PinTerm(termStore.GetTerm(SourceTermSetIdAfterMapping, pinTermIdAfterMapping));
                                }

                                restorePinTermProperties(pinTerm, PinIdToTermInfoMapping[pinInfo.Item1], false);

                                restoreTermNavigation(termStore.GetTerm(pinParentTermSetIdAfterMapping, pinTermIdAfterMapping), PinIdToTermInfoMapping[pinInfo.Item1]);
                            }
                            else if (mOption.RestoreTermSetAndTermProperties)
                            {
                                IAveTerm tempTerm = null;
                                try
                                {
                                    if (parentTerm != null)
                                    {
                                        tempTerm = parentTerm.Terms[PinIdToTermInfoMapping[pinInfo.Item1].Name];
                                    }
                                    else if (parentTermSet != null)
                                    {
                                        tempTerm = parentTermSet.Terms[PinIdToTermInfoMapping[pinInfo.Item1].Name];
                                    }

                                    restorePinTermProperties(tempTerm, PinIdToTermInfoMapping[pinInfo.Item1], true);

                                    restoreTermNavigation(tempTerm, PinIdToTermInfoMapping[pinInfo.Item1]);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                                }

                            }
                        }
                        else
                        {
                            Guid normalTermId = pinInfo.Item1;
                            if (isTermRestore(normalTermId, pinParentTermIdAfterMapping, pinParentTermSetIdAfterMapping, PinIdToTermInfoMapping[pinInfo.Item1].Name, termStore, true))
                            {
                                //normal
                                AveTermInfo termInfo = this.PinIdToTermInfoMapping[normalTermId];

                                if (parentTerm != null)
                                {
                                    RestoreSubPinTermToNormalTerm(parentTerm, termInfo, false, null);
                                }
                                else if (parentTermSet != null)
                                {
                                    RestorePinTermToNormalTerm(parentTermSet, termInfo, false, null);
                                }
                            }
                            else if (mOption.RestoreTermSetAndTermProperties)
                            {
                                IAveTerm tempTerm = null;
                                try
                                {
                                    if (parentTerm != null)
                                    {
                                        tempTerm = parentTerm.Terms[PinIdToTermInfoMapping[pinInfo.Item1].Name];
                                    }
                                    else if (parentTermSet != null)
                                    {
                                        tempTerm = parentTermSet.Terms[PinIdToTermInfoMapping[pinInfo.Item1].Name];
                                    }
                                    //restoreTermNavigation(tempTerm, PinIdToTermInfoMapping[pinInfo.Item1]);
                                    //因为是还原Pin Term的属性到正常Term，而且选择了覆盖属性，所以需要将普通属性覆盖过去。
                                    UpdateTermSetting(PinIdToTermInfoMapping[pinInfo.Item1], tempTerm, false);
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                                }

                            }
                        }
                    }
                }
            }
        }

        /*判断是否存在term不能还原的情况：
          1.目的端存在同Name&结构的Normal Term, 不还原Term
          2.目的端存在同Name&结构的Pin Term; 不还原Term
          3.目的端同结构处存在不同Name, 但同ID的Term; 即曾经还原过Source和Pin, 之后修改了Source Term的Name, 本Case中由于Source Term会不Keep ID还原到目的端, Pin Term需要Pin到不Keep ID还原过来的Source Term上(Term Mapping的目的端)
        */
        private bool isTermRestore(Guid termId, Guid parentTermId, Guid parentTermsetId, string termName, IAveTermStore termStore, bool isNormalTerm = false)
        {
            bool result = false;
            IAveTermSet parentTermset = termStore.GetTermSet(parentTermsetId);
            IAveTerm parentTerm = termStore.GetTerm(parentTermsetId, parentTermId);

            if (isNormalTerm)
            {
                if ((parentTermset != null && parentTermset.Terms.Where(n => n.Name == termName).Count() == 0) || (parentTerm != null && parentTerm.Terms.Where(n => n.Name == termName).Count() == 0))
                {
                    result = true;
                }
            }
            else
            {
                if (((parentTermset != null && parentTermset.Terms.Where(n => n.Name == termName).Count() == 0) || (parentTerm != null && parentTerm.Terms.Where(n => n.Name == termName).Count() == 0)) &&
                ((parentTermset != null && parentTermset.GetTerm(termId) == null) || (parentTerm != null && parentTerm.TermSet.GetTerm(termId) == null)))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 还原Pin Term的不同于Pin Source Term的属性。这些属性都是Pin可以设置的，那些不可以设置的属性没有放没有放在这个方法里。
        /// </summary>
        /// <param name="term"></param>
        /// <param name="pinTermInfo"></param>
        private void restorePinTermProperties(IAveTerm term, AveTermInfo pinTermInfo, bool existInDestination)
        {
            //The independant properties include available for tagging ,navigation ,sort order ,and local custom properties.
            term.IsAvailableForTagging = pinTermInfo.IsAvailableForTagging;

            term.CustomSortOrder = pinTermInfo.CustomSortOrder;
            if (!existInDestination)
            {
                term.DeleteAllLocalCustomProperties();
            }
            IEnumerable<KeyValuePair<string, string>> localProperties = null;
            if (pinTermInfo.LocalCustomProperties != null)
            {
                localProperties = pinTermInfo.LocalCustomProperties.Where(pair => !pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase));
            }
            if (localProperties != null)
            {
                foreach (KeyValuePair<string, string> property in localProperties)
                {
                    if (!term.LocalCustomProperties.ContainsKey(property.Key))
                    {
                        term.SetLocalCustomProperty(property.Key, property.Value);
                    }
                }
            }
            term.TermStore.CommitAll();

            foreach (IAveTerm subTerm in term.Terms)
            {
                AveTermInfo subTermInfo = null;
                try
                {
                    if (pinTermInfo.Terms.Count > 0)
                    {
                        subTermInfo = pinTermInfo.Terms.Where(n => n.Name == subTerm.Name).Single();
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Restore pin term's navigation exception:{0}", e.Message);
                    throw;
                }
                if (subTermInfo != null)
                {
                    restorePinTermProperties(subTerm, subTermInfo, existInDestination);
                }

            }

        }

        private void restorePinTermProperties(IAveTerm term, AveTermInfo pinTermInfo)
        {
            restorePinTermProperties(term, pinTermInfo, false);
        }

        /// <summary>
        /// 还原term及它下边的子term的navigation
        /// </summary>
        /// <param name="term"></param>
        /// <param name="pinTermInfo"></param>
        private void restoreTermNavigation(IAveTerm term, AveTermInfo pinTermInfo)
        {
            restoreTermNavigation(term, pinTermInfo.LocalCustomProperties);

            foreach (IAveTerm subTerm in term.Terms)
            {
                AveTermInfo subTermInfo = null;
                try
                {
                    if (pinTermInfo.Terms.Count > 0)
                    {
                        subTermInfo = pinTermInfo.Terms.Where(n => n.Name == subTerm.Name).Single();
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Restore pin term's navigation exception:{0}", e.Message);
                    throw;
                }
                if (subTermInfo != null)
                {
                    restoreTermNavigation(subTerm, subTermInfo);
                }

            }
        }

        /// <summary>
        /// 还原一个Term的Navigation和Local Custom Property属性,内部有CommitAll()。
        /// </summary>
        /// <param name="term"></param>
        /// <param name="properties"></param>
        private void restoreTermNavigation(IAveTerm term, Dictionary<string, string> properties)
        {
            IAveTermSet termSet = term.TermSet;
            if (termSet != null && termSet.CustomProperties != null && termSet.CustomProperties.Count > 0 &&
                                    termSet.CustomProperties.ContainsKey("_Sys_Nav_IsNavigationTermSet") && termSet.CustomProperties["_Sys_Nav_IsNavigationTermSet"].Equals("True", StringComparison.OrdinalIgnoreCase)
                                    )
            {
                foreach (KeyValuePair<string, string> pair in properties)
                {
                    if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                    {
                        string realValue = "";
                        if (pair.Value.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
                        {
                            realValue = mSPSite.ServerRelativeUrl + pair.Value.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
                            term.ChangedLCPSourceValue.Add(pair.Key, pair.Value);
                        }
                        else
                        {
                            realValue = pair.Value;
                        }
                        term.SetLocalCustomProperty(pair.Key, realValue);
                    }
                }
                term.TermStore.CommitAll();
            }
        }

        private void RestoreReusedTermProperties(IAveTerm term, AveTermInfo reusedTermInfo)
        {
            term.IsAvailableForTagging = reusedTermInfo.IsAvailableForTagging;

            term.CustomSortOrder = reusedTermInfo.CustomSortOrder;

            term.DeleteAllLocalCustomProperties();
            IEnumerable<KeyValuePair<string, string>> localProperties = null;
            if (reusedTermInfo.LocalCustomProperties != null)
            {
                localProperties = reusedTermInfo.LocalCustomProperties.Where(pair => !pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase));
            }
            if (localProperties != null)
            {
                foreach (KeyValuePair<string, string> property in localProperties)
                {
                    if (!term.LocalCustomProperties.ContainsKey(property.Key))
                    {
                        term.SetLocalCustomProperty(property.Key, property.Value);
                    }
                }
            }
            term.TermStore.CommitAll();
        }

        private void RestoreReusedTermNavigation(IAveTerm term, Dictionary<string, string> properties)
        {
            IAveTermSet termSet = term.TermSet;
            if (termSet != null && termSet.CustomProperties != null && termSet.CustomProperties.Count > 0 &&
                                    termSet.CustomProperties.ContainsKey("_Sys_Nav_IsNavigationTermSet") && termSet.CustomProperties["_Sys_Nav_IsNavigationTermSet"].Equals("True", StringComparison.OrdinalIgnoreCase)
                                    )
            {
                foreach (KeyValuePair<string, string> pair in properties)
                {
                    if (pair.Key.StartsWith("_Sys_Nav_", StringComparison.OrdinalIgnoreCase))
                    {
                        string realValue = "";
                        if (pair.Value.StartsWith(mSiteMappingManager.SiteUrlMapping.Keys.Last(), StringComparison.OrdinalIgnoreCase))
                        {
                            realValue = mSPSite.ServerRelativeUrl + pair.Value.Substring(mSiteMappingManager.SiteUrlMapping.Last().Key.Length);
                            term.ChangedLCPSourceValue.Add(pair.Key, pair.Value);
                        }
                        else
                        {
                            realValue = pair.Value;
                        }
                        term.SetLocalCustomProperty(pair.Key, realValue);
                    }
                }
                term.TermStore.CommitAll();
            }
        }

        public void Dispose()
        {
            report.Dispose();
        }

        public void Restore(List<AveTermStoreInfo> termStoreInfos)
        {
            var siteMappingManager = WrapperRuntime.CurrentContext.MappingManager;
            var restoreManagedMetadataNavigation = WrapperRuntime.CurrentContext.RestoreManagedMetadataNavigation;
            Restore(termStoreInfos, siteMappingManager, restoreManagedMetadataNavigation);
        }

        public void PostActionReplaceMetadataTermSetAndTermPropertyUrl()
        {
            try
            {
                bool change = false;
                var replaceInfos = this.mSiteMappingManager.GetMetadataNeedReplaceUrlPropertyTermOrTermSet();
                foreach (var termStoreInfo in replaceInfos)
                {
                    IAveTaxonomySession session = mSPSite.AveSPTaxonomySession;
                    IAveTermStore termStore = session.TermStores[termStoreInfo.Key];
                    foreach (var termSetInfo in termStoreInfo.Value)
                    {
                        IAveTermSet termSet = termStore.GetTermSet(termSetInfo.Key);
                        if (termSet == null)
                        {
                            continue;
                        }
                        foreach (var termInfo in termSetInfo.Value)
                        {
                            if (!Guid.Equals(termInfo.Key, Guid.Empty))//Term Change.
                            {
                                IAveTerm term = termSet.GetTerm(termInfo.Key);
                                if (term != null)
                                {
                                    foreach (var prop in termInfo.Value)
                                    {
                                        string newValue = ReplaceTermSetOrTermUrlProperty(prop.Value);
                                        term.SetLocalCustomProperty(prop.Key, newValue);
                                        change = true;
                                    }
                                }
                            }
                            else//Term Set change.
                            {
                                foreach (var prop in termInfo.Value)
                                {
                                    string newValue = ReplaceTermSetOrTermUrlProperty(prop.Value);
                                    termSet.SetCustomProperty(prop.Key, newValue);
                                    change = true;
                                }
                            }
                        }
                    }
                    if (change)
                    {
                        termStore.CommitAll();
                        change = false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Failed to replace url properties for termset(term) in post action.", ex);
            }
        }

        #region IAveMetadataService Members




        #endregion
    }

    internal class TermSetCommitCalc
    {
        bool _hasChange;

        Dictionary<Guid, bool> _termStatus;

        const int MAXSetTermNumber = 20;//当Term Set下未Commit总数超过时，每个Term需要单独Update，否则容易Timeout
        const int MAXSubTermNumber = 20;//当一个Term的Sub Term总数超过时，该Term需要Commit
        const int MAXTermNumber = 100;//当一个Term下Sub Term总数超过时，即使每个子Sub Term都没超过MAXSubTermNumber，也需要Commit

        public TermSetCommitCalc(AveTermSetInfo set)
        {
            _termStatus = new Dictionary<Guid, bool>();
            CalculatorTermSet(set, _termStatus);
        }

        private void CalculatorTermSet(AveTermSetInfo set, Dictionary<Guid, bool> termStatus)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.CalculatorTermSet"))
            {


                int setTermNumber = Calculator(new AveTermInfo() { Id = set.Id, Terms = set.Terms }, termStatus);
                if (setTermNumber > MAXSetTermNumber)
                {
                    foreach (var sub in set.Terms)
                    {
                        termStatus[sub.Id] = true;
                    }
                }

            }

        }

        private int Calculator(AveTermInfo term, Dictionary<Guid, bool> termStatus)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.Calculator"))
            {

                int totalTermNumber = 1;
                int maxSubTermNumber = 0;
                foreach (var sub in term.Terms)
                {
                    int subTermNumber = Calculator(sub, termStatus);
                    if (subTermNumber > maxSubTermNumber)
                    {
                        maxSubTermNumber = subTermNumber;
                    }
                    totalTermNumber += subTermNumber;
                }
                if (maxSubTermNumber > MAXSubTermNumber || totalTermNumber > MAXTermNumber)
                {
                    foreach (var sub in term.Terms)
                    {
                        termStatus[sub.Id] = true;
                    }
                }
                return totalTermNumber;

            }

        }

        public bool TermSetNeedCommit
        {
            get
            {
                return HasChange();
            }
        }

        private bool HasChange()
        {
            if (_hasChange)
            {
                _hasChange = false; ;
                return true;
            }
            return false;
        }

        public bool TermNeedCommit(Guid termId, bool create)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.TermNeedCommit"))
            {

                _hasChange |= create;
                var needCommit = false;
                if (_termStatus.ContainsKey(termId))
                {
                    needCommit = true;
                }
                if (needCommit)
                {
                    return HasChange();
                }
                return false;

            }

        }

    }
}
