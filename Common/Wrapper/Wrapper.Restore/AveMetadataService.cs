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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility;
using System.Threading;
using AvePoint.GCommon.Utility.I18N;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    internal class AveMetadataServiceCache
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static Dictionary<Guid, AveTermStoreCacheInfo> cacheTermStoreInfos = new Dictionary<Guid, AveTermStoreCacheInfo>();
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
                                lock (cacheTermStoreInfos)
                                {
                                    cacheTermStoreInfos.Remove(uniqueId);
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (cacheTermStoreInfos)
                        {
                            cacheTermStoreInfos.Clear();
                        }
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

    public class AveTermStoreCacheInfo
    {
        public DateTime LastAccessTime = DateTime.MinValue;
        public Guid UniqueId = Guid.Empty;
        public Dictionary<Guid, Guid> TermStoreIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermGroupIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermSetIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermIdMapping = new Dictionary<Guid, Guid>();
    }

    public class AveMetadataService: IDisposable
    {
        private IAveSite mSPSite;
        private AveSPSite mAveSPSite;
        private AveObjectModelFactory objectModelFactory;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static int DefaultLCID = 1033;
        public bool RestoreUsedTermOnly = false;
        public bool EnableCache = false;
        public Dictionary<string, string> TermStoreMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, Guid> TermStoreIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermGroupIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermSetIdMapping = new Dictionary<Guid, Guid>();
        public Dictionary<Guid, Guid> TermIdMapping = new Dictionary<Guid, Guid>();
        public List<Guid> NeedDeprecateTermId = new List<Guid>();
        public Dictionary<Guid, AveTermStoreInfo> TermStoreInfoCache = new Dictionary<Guid, AveTermStoreInfo>();
        public List<Guid> GlobalTermSetIds = new List<Guid>();
        public Dictionary<Guid, Dictionary<Guid, Guid>> PinIdMapping = new Dictionary<Guid, Dictionary<Guid, Guid>>();
        private IReport report = new AveWrapperReport();
        /// <summary>
        /// Restore Term信息时是否忽略Global的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipGlobalTermGroup { get; set; }

        /// <summary>
        /// Restore Term信息时是否忽略Local的Term Group，默认为不忽略。
        /// </summary>
        public bool SkipLocalTermGroup { get; set; }

        private TermSetCommitCalc _currentTermSetCalculator;

        public AveMetadataService(AveSPSite site)
        {
            mAveSPSite = site;
            mSPSite = site.SPSite;
            this.objectModelFactory = site.ObjectModelFactory;
            mAveSPSite.MappingManager.TermMappingManager.TermStoreIdMapping = TermStoreIdMapping;
            mAveSPSite.MappingManager.TermMappingManager.TermGroupIdMapping = TermGroupIdMapping;
            mAveSPSite.MappingManager.TermMappingManager.TermSetIdMapping = TermSetIdMapping;
            mAveSPSite.MappingManager.TermMappingManager.TermIdMapping = TermIdMapping;
        }


        public AveMetadataService(IAveSite site, AveObjectModelFactory objectModelFactory)
        {
            mSPSite = site;
            this.objectModelFactory = objectModelFactory;
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
                if (!TermStoreInfoCache.ContainsKey(termStoreInfo.Id))
                {
                    TermStoreInfoCache.Add(termStoreInfo.Id, termStoreInfo);
                }
            }
        }

        //当TermStoreIdMapping中不存在sspid，尝试在目的端找到或者创建需要的termstore。	
        public Guid TryRestoreTermStore(Guid sspid)
        {
            IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
            if (session == null)
            {
                return Guid.Empty;
            }
            if (session.TermStores.Count <= 0)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, "The Destination did not relative to metadata service."));
                log.Warn("The Destination did not relative to metadata service.");
                return Guid.Empty;
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
                    return sspid;
                }
            }
            return Guid.Empty;
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
                                                        if (!TermIdMapping.ContainsKey(termInfo.Id))
                                                        {
                                                            TermIdMapping.Add(termInfo.Id, term.ID);
                                                        }
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
                                                    if (!TermIdMapping.ContainsKey(termInfo.Id))
                                                    {
                                                        TermIdMapping.Add(termInfo.Id, term.ID);
                                                    }
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
                    if (!TermIdMapping.ContainsKey(listTerms[listTerms.Count - 1].Id))
                    {
                        TermIdMapping.Add(listTerms[listTerms.Count - 1].Id, term.ID);
                    }
                    for (int i = listTerms.Count - 2; i >= 0; i--)
                    {
                        IAveTerm subTerm = RestoreSubTermSelf(term, listTerms[i]);
                        if (subTerm != null)
                        {
                            if (!TermIdMapping.ContainsKey(listTerms[i].Id))
                            {
                                TermIdMapping.Add(listTerms[i].Id, subTerm.ID);
                            }
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

        public void Restore(AveManagedMetadataServiceApplicationInfo serviceAppInfo, Guid targetServiceAppId)
        {
            IAveMetadataServiceRestorer serviceAppRestorer = this.objectModelFactory.CreateMetadataServiceRestorer(targetServiceAppId);
            serviceAppRestorer.Restore(serviceAppInfo);
        }

        public void Restore(List<AveTermStoreInfo> termStoreInfos)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.Restore"))
            {
#endif
            try
            {
                DateTime start = DateTime.Now;
                log.Info(string.Format("Before Restore MetadataService Time:{0}", start));
                IAveTaxonomySession session = this.objectModelFactory.CreateTaxonomySession(mSPSite);
                if (session == null)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, "Can not create mms session."));
                    return;
                }
                if (session.TermStores.Count <= 0)
                {
                    log.Warn("The Destination did not relative to metadata service.");
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, "The Destination did not relative to metadata service."));
                    return;
                }
                OutputDestServiceInfo(session);

                OutputSourceTermGroupInfo(termStoreInfos);

                foreach (AveTermStoreInfo termStoreInfo in termStoreInfos)
                {
                    DefaultLCID = termStoreInfo.DefaultLanguage;
                    if (EnableCache)
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
                var finish = DateTime.Now;
                log.Info("Restore Metadata Service Finished.Start:{0},Finished:{1},Time Cost:{2}", start, finish, finish - start);
                OutputDebugServiceInfo();
            }
            catch (Exception e)
            {
                AddMMSFailedReportWithException(e, "MetadataService", "");
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreMetadataServiceFailedEventMessage(e));
            }
#if PerformanceLog
            }
#endif
        }

        private void AddMMSFailedReportWithException(Exception e,string currentObject, string currentObjectType)
        {
            if (e is AveSecurityTrimingException||IsUnauthorizedException(e))
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("Wrapper_ConfirmTermStoreAdmin", WrapperRestoreReportResource.Wrapper_ConfirmTermStoreAdmin, mAveSPSite.BPOSUserAccountInfo.UserName)));
            }
            else if (e is TargetInvocationException)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Failed, string.Format("An error occurred while restoring {0}. Name:{1}, error:{2}", currentObjectType, currentObject, e?.InnerException?.Message)));
            }
            else
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Failed, string.Format("An error occurred while restoring {0}. Name:{1}, error:{2}", currentObjectType, currentObject, e.Message)));
            }
        }

        private static void OutputSourceTermGroupInfo(List<AveTermStoreInfo> termStoreInfos)
        {
            try
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine("SourceTermStoreInfo");
                foreach (AveTermStoreInfo termStoreInfo in termStoreInfos)
                {
                    output.AppendLineByLevel(1, "TermStore:" + termStoreInfo.Name);
                    foreach (var group in termStoreInfo.Groups)
                    {
                        var stastic = GetGroupStastic(group);
                        string groupName = group != null ? group.Name : "GroupIsNull";
                        string termSetCount = group != null ? group.TermSets != null ? group.TermSets.Count.ToString() : "TermSetInfosIsNull" : "GroupIsNull";
                        output.AppendLineByLevel(2, "Group:" + groupName + "(" + stastic + ")");
                    }
                }
                log.Info(output.ToString());
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while output source term store info.{0}",e);
            }
        }

        private static TermGroupStastic GetGroupStastic(AveMetadataGroupInfo group)
        {
            var result = new TermGroupStastic();
            if (group == null)
            {
                return new TermGroupStastic();
            }
            result.TermSetCount += group.TermSets.Count;
            foreach (var set in group.TermSets)
            {
                foreach (var term in set.Terms)
                {
                    ProcessTerm(term,result);
                }
            }
            return result;
        }

        private static void ProcessTerm(AveTermInfo termInfo, TermGroupStastic result)
        {
            foreach (var sub in termInfo.Terms)
            {
                ProcessTerm(sub, result);
            }
            result.LabelCount += termInfo.Labels.Count;
            result.TermCount++;
        }

        private class TermGroupStastic
        {
            public long TermCount { get; set; } = 0;
            public long TermSetCount { get; set; } = 0;
            public long LabelCount { get; set; } = 0;
            public override string ToString()
            {
                return string.Format("TermSet:{0},Term:{1},Label:{2}",TermSetCount,TermCount,LabelCount);
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
                lock (AveMetadataServiceCache.cacheTermStoreInfos)
                {
                    AveMetadataServiceCache.cacheTermStoreInfos[termStoreInfo.UniqueId] = cacheInfo;
                }
            }
        }

        public void OutputDestServiceInfo(IAveTaxonomySession session)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.OutputDestServiceInfo"))
            {
#endif
                StringBuilder info = new StringBuilder();
                info.Append("Destination TermStores:");
                foreach (IAveTermStore store in session.TermStores)
                {
                    info.AppendLine(store.Name);
                }
                log.Info(info.ToString());
#if PerformanceLog
            }
#endif
        }

        public void OutputDebugServiceInfo()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.OutputDebugServiceInfo"))
            {
#endif
                StringBuilder info = new StringBuilder();
                info.AppendLine("TermStoreMapping:");
                foreach (KeyValuePair<string, string> pair in TermStoreMapping)
                {
                    info.AppendLine(pair.Key + " -> " + pair.Value);
                }
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
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// group的Group Managers，Contributors和termset的Owner，Stakeholders，可能带有类似i:0#.w|的头，
        /// 当是AD group的时候，用API取出来的account是Sid格式的，但是添加的时候需要用Account格式。
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string EnsureAccountName(string account, AveObjectModelFactory modelFactory)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.EnsureAccountName"))
            {
#endif
                if (account.IndexOf('|') > 0)
                {
                    account = account.Substring(account.IndexOf('|') + 1);
                }
                if (AveDirectoryServiceUtility.IsStringSid(account))
                {
                    account = AveDirectoryServiceUtility.GetAccountFromSid(account, modelFactory);
                }
                return account;
#if PerformanceLog
            }
#endif
        }

        //restore itemself之前确认对应MetadataColumn value的term是否应经在目的端存在，如果没有则需要创建需要的term
        public bool VerifyMetadataColumnValue(AveBaseItemInfo info, IAveList List, Dictionary<string, string> fieldTermMapping, Dictionary<Guid, Guid> termIdMapping, AveObjectModelFactory modelFactory)
        {
            try
            {
                List<Dictionary<string, object>> needUpdateTaonoxyFields = new List<Dictionary<string, object>>();
                foreach (string fieldName in fieldTermMapping.Keys)
                {
                    IAveField field = List.Fields.GetField(fieldName);
                    IAveTaxonomyField tField = field as IAveTaxonomyField;
                    IAveTaxonomySession session = List.ParentWeb.Site.AveSPTaxonomySession;
                    IAveTermStore termStore = null;
                    Guid sspId = Guid.Empty;
                    if (tField.SspId == Guid.Empty && !tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
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


                    int LCID = (int?)(termStore?.DefaultLanguage) ?? 0;

                    if (termStore != null && termStore.Languages.Contains(DefaultLCID))
                    {
                        DefaultLCID = termStore.DefaultLanguage;
                        LCID = DefaultLCID;
                    }

                    bool submit = false;
                    string[] termNames = fieldTermMapping[fieldName].Split(';');
                    string[] termHiberarchy = null;
                    //TaxonomyFieldValueCollection values = item[fieldName] as TaxonomyFieldValueCollection;
                    List<IAveTerm> terms = new List<IAveTerm>();
                    foreach (string termName in termNames)
                    {
                        if (string.IsNullOrEmpty(termName))
                        {
                            continue;
                        }
                        IAveTerm term = null;
                        termHiberarchy = null;
                        string tName = termName;
                        try
                        {
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
                                        if (termSet != null)
                                        {
                                            term = termSet.GetTerm(tTermId);
                                            //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                                            if (term == null && tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
                                            {
                                                foreach (IAveTermStore tStore in session.TermStores)
                                                {
                                                    if (term == null)
                                                    {
                                                        term = tStore.GetTerm(tTermId);
                                                    }
                                                }
                                            }
                                        }
                                        else if (termStore != null)
                                        {
                                            term = termStore.GetTerm(tTermId);
                                        }
                                        if (term == null)
                                        {   //满足restore term only的逻辑，保证item的mms column value对应的term在目的端存在。
                                            term = CreateUsedTermOnly(termStore, termSet, tTermId);
                                            submit = true;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("try get fieldValue term before restore item. field:{0},error:{1}", fieldName, ex.ToString());
                                }
                            }
                            //'<'表示term的层次关系。
                            else if (termName.Contains("<"))
                            {
                                termHiberarchy = termName.Split('<');
                                term = termSet?.Terms[termHiberarchy[0]];
                                for (int i = 1; i < termHiberarchy.Length; i++)
                                {
                                    if (string.IsNullOrEmpty(termHiberarchy[i]))
                                    {
                                        continue;
                                    }
                                    term = term.Terms[AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[i])];
                                }
                            }
                            if (term == null && termSet != null)
                            {
                                try
                                {
                                    term = termSet.Terms[AveTaxonomyFieldUtility.NormalizeName(tName)];
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermSetByNameError, e.ToString());
                                    //DOC-78396 使用此方法刷新对象
                                    IAveTermCollection ts = termSet.GetTerms(AveTaxonomyFieldUtility.NormalizeName(tName).Trim(), true);
                                    term = termSet.Terms[AveTaxonomyFieldUtility.NormalizeName(tName).Trim()];
                                }
                            }
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            if (termSet != null)
                            {
                                if (termHiberarchy != null && termHiberarchy.Length > 0)
                                {
                                    try
                                    {
                                        if (string.IsNullOrEmpty(termHiberarchy[0]))
                                        {
                                            continue;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermFailed, e.ToString());
                                        //DOC-78396
                                        try
                                        {
                                            term = termSet.CreateTerm(AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[0]).Trim(), LCID, Guid.NewGuid());
                                            termSet.TermStore.CommitAll();
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ClearTermError, ex.ToString());
                                            //DOC-78396 使用此方法刷新对象
                                            IAveTermCollection ts = termSet.GetTerms(AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[0]).Trim(), true);
                                            term = termSet.Terms[AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[0]).Trim()];
                                        }
                                    }
                                    for (int i = 1; i < termHiberarchy.Length; i++)
                                    {
                                        try
                                        {
                                            if (string.IsNullOrEmpty(termHiberarchy[i]))
                                            {
                                                continue;
                                            }
                                            term = term.Terms[AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[i])];
                                        }
                                        catch (Exception e)
                                        {
                                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByNameError, e.ToString());
                                            term = term.CreateTerm(AveTaxonomyFieldUtility.NormalizeName(termHiberarchy[i]).Trim(), LCID, Guid.NewGuid());
                                            term.TermStore.CommitAll();
                                        }
                                    }
                                }
                                else
                                {
                                    term = termSet.CreateTerm(tName, LCID, Guid.NewGuid());
                                    submit = true;
                                }
                            }
                        }
                        if (term != null)
                        {
                            terms.Add(term);
                            //如果field不允许多值，没有必要找多个term了。
                            if (!tField.AllowMultipleValues)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (modelFactory.ContextKind == AveContextKind.ClientObjectModel)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        //TaxonomyFieldValue value = new TaxonomyFieldValue(field);
                        //value.TermGuid = myTerm.Id.ToString();
                        //value.Label = myTerm.Name;
                        //values.Add(value);
                    }
                    if (submit)
                    {
                        try
                        {
                            termStore.CommitAll();
                            submit = false;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CommitTermStoreError, e.ToString());
                            terms.Clear();
                            foreach (string termName in termNames)
                            {
                                if (string.IsNullOrEmpty(termName))
                                {
                                    continue;
                                }
                                try
                                {
                                    //DOC-78396 使用此方法刷新对象
                                    IAveTermCollection ts = termSet.GetTerms(AveTaxonomyFieldUtility.NormalizeName(termName).Trim(), true);
                                    terms.Add(termSet.Terms[AveTaxonomyFieldUtility.NormalizeName(termName).Trim()]);
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("submit the new terms exception, term:{0}, error:{1}", termName, ex.ToString());
                                    return false;
                                }
                            }
                        }
                    }
                    if (modelFactory.ContextKind == AveContextKind.ClientObjectModel)
                    {
                        if (tField.AllowMultipleValues)
                        {
                            Dictionary<string, object> taxonomyfield = new Dictionary<string, object>();
                            taxonomyfield.Add("FieldName", fieldName);
                            List<string> mutipleText = new List<string>();
                            foreach (IAveTerm tTerm in terms)
                            {
                                if (tTerm != null)
                                {
                                    int effectiveLcid = LCID;
                                    string text = tTerm.GetDefaultLabel(effectiveLcid) + "|" + tTerm.ID;
                                    mutipleText.Add(text);
                                }
                            }
                            taxonomyfield.Add("Text", mutipleText);
                            taxonomyfield.Add("AllowMultipleValues", true);
                            needUpdateTaonoxyFields.Add(taxonomyfield);
                        }
                        else
                        {
                            if (terms.Count > 0)
                            {
                                int effectiveLcid = LCID;
                                string text = terms[0].GetDefaultLabel(effectiveLcid) + "|" + terms[0].ID;
                                Dictionary<string, object> taxonomyfield = new Dictionary<string, object>();
                                taxonomyfield.Add("FieldName", fieldName);
                                taxonomyfield.Add("Text", text);
                                taxonomyfield.Add("AllowMultipleValues", false);
                                needUpdateTaonoxyFields.Add(taxonomyfield);
                            }
                        }
                    }
                }

                if (modelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    info.FieldsInfo.Fields.Add("TaxonomyFields", needUpdateTaonoxyFields);
                }
            }
            catch (Exception ex)
            {
                log.Warn("VerifyMetadataColumnValue failed, list:{0}, error:{1}", List.Title, ex.ToString());
                return false;
            }
            return true;
        }

        public IAveTerm CreateUsedTermOnly(IAveTermStore termStore, IAveTermSet termSet, Guid termId)
        {
            IAveTerm keyword = null;
            foreach (Guid sspid in TermStoreInfoCache.Keys)
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
                                        if (!TermIdMapping.ContainsKey(termInfo.Id))
                                        {
                                            TermIdMapping.Add(termInfo.Id, term.ID);
                                        }
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
                                        if (!TermIdMapping.ContainsKey(termInfo.Id))
                                        {
                                            TermIdMapping.Add(termInfo.Id, term.ID);
                                        }
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


            return keyword;
        }

        public IAveTermStore RestoreTermStore(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo)
        {
            return RestoreTermStore(session, termStoreInfo, null);
        }

        public IAveTermStore RestoreTermStore(IAveTaxonomySession session, AveTermStoreInfo termStoreInfo, AveTermStoreCacheInfo cacheInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTermStore"))
            {
#endif
                string termStoreName = termStoreInfo.Name;
                if (TermStoreMapping != null && TermStoreMapping.ContainsKey(termStoreName))
                {
                    termStoreName = TermStoreMapping[termStoreName];
                }
                IAveTermStore termStore = null;
                try
                {
                    termStore = session.TermStores[termStoreName];
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, WrapperRestoreResource.GetTermDestinationError, termStoreName, e.Message);
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
                PostProcessPinTerms(termStore);
                PostProcessReuseTerms(termStore);  //SAAS-11627 添加Deprecate ReuseTerm 的处理过程
                termStore.CommitAll();
                return termStore;
#if PerformanceLog
            }
#endif
        }

        //只restore MetadataGroup，不还原下属的termset
        public IAveTaxonomyGroup RestoreMetadataGroupSelf(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
            if (SkipGlobalTermGroup && !groupInfo.IsSiteCollectionGroup)
            {
                return null;
            }
            if (SkipLocalTermGroup && groupInfo.IsSiteCollectionGroup)
            {
                return null;
            }
            bool isMysite = AveUrlUtility.IsTenantMySite(mAveSPSite.SPSite.Url);
            if (groupInfo.IsSiteCollectionGroup && isMysite)
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
                    UpdateMetadataGroup(group, groupInfo);
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
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Failed, "An error occurred while create Site Group" + e.Message));
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

        public IAveTaxonomyGroup RestoreMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo, AveTermStoreCacheInfo cacheInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreMetadataGroup"))
            {
#endif
            log.Info($"current group is site group?{groupInfo.IsSiteCollectionGroup}");
            if (SkipGlobalTermGroup && !groupInfo.IsSiteCollectionGroup)
            {
                foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
                {
                    GlobalTermSetIds.Add(termSetInfo.Id);
                }
                return null;
            }
            if (SkipLocalTermGroup && groupInfo.IsSiteCollectionGroup)
            {
                log.Warn("current group is SiteCollectionGroup and SkipLocalTermGroup is true ,skip restore");
                return null;
            }
            bool isMysite = AveUrlUtility.IsTenantMySite(mAveSPSite.SPSite.Url);
            if (groupInfo.IsSiteCollectionGroup && isMysite)
            {
                log.Warn("this group is MySite,skip restore");
                return null;
            }
            string groupName = groupInfo.Name;
            IAveTaxonomyGroup group = null;
            bool isNewCreated = false;           
            try
            {

                if (groupInfo.IsSiteCollectionGroup)
                {
                    bool existSCLevelTermGroup = mSPSite.ExistSCTermGroup();
                    if (!existSCLevelTermGroup)
                    {
                        mSPSite.UpdateSCTermGroupName(groupInfo.Name);
                        isNewCreated = true;
                        termStore.GetSCLevelTermGroup(mSPSite);
                    }
                    group = termStore.Groups[groupInfo.Name];
                    UpdateMetadataGroup(group, groupInfo);
                }
                else
                {
                    group = termStore.Groups[groupInfo.Name];
                }
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
                    isNewCreated = true;
                }
            }
            foreach (AveTermSetInfo termSetInfo in groupInfo.TermSets)
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
            return group;
#if PerformanceLog
            }
#endif
        }

        private void UpdateMetadataGroup(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.UpdateMetadataGroup"))
            {
#endif
            try
            {
                group.Description = groupInfo.Description;
                foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
                {
                    string principalName = groupManager.PrincipalName;
                    if (principalName.Contains('|'))
                    {
                        principalName = principalName.Substring(principalName.IndexOf('|') + 1);
                    }
                    group.AddGroupManager(principalName);
                }
                foreach (AveAceInfo contributor in groupInfo.Contributors)
                {
                    string principalName = contributor.PrincipalName;
                    if (principalName.Contains('|'))
                    {
                        principalName = principalName.Substring(principalName.IndexOf('|') + 1);
                    }
                    group.AddContributor(principalName);
                }
            }
            catch (Exception e)
            {
                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, string.Format("An error occurred while update group property. groupName:{0}, error:{1}", group.Name, e.Message)));
                log.Warn("An error occurred while update group property. groupName:{0}, error:{1}", group.Name, e.ToString());
            }
            group.TermStore.CommitAll();
#if PerformanceLog
            }
#endif
        }

        public IAveTaxonomyGroup CreateMetadataGroup(IAveTermStore termStore, AveMetadataGroupInfo groupInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateMetadataGroup"))
            {
#endif
            try
            {
                IAveTaxonomyGroup group = termStore.CreateGroup(groupInfo.Name);
                try
                {
                    group.Description = groupInfo.Description;
                    foreach (AveAceInfo groupManager in groupInfo.GroupManagers)
                    {
                        string principalName = groupManager.PrincipalName;
                        try
                        {
                            principalName = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(principalName);
                            group.AddGroupManager(principalName);
                        }
                        catch (Exception e)
                        {
                            //report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("GetTermSetByNameError", WrapperRestoreReportResource.GetTermSetByNameError, e.Message)));
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetTermSetByNameError, e.ToString());
                        }
                    }
                    foreach (AveAceInfo contributor in groupInfo.Contributors)
                    {
                        string principalName = contributor.PrincipalName;
                        try
                        {
                            principalName = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(principalName);
                            group.AddContributor(principalName);
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("AddMetadataGroupContributorFailed", WrapperRestoreReportResource.AddMetadataGroupContributorFailed, principalName, e.Message)));
                            log.Warn("An error occurred while add group contributor. principalName:{0}, error:{1}", principalName, e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("SetMetadataGroupPropertiesFailed", WrapperRestoreReportResource.SetMetadataGroupPropertiesFailed, groupInfo.Name, e.Message)));
                    log.Warn(string.Format("An error occurred while set new create group property. groupName:{0}, error:{1}", groupInfo.Name, e.ToString()));
                }
                group.TermStore.CommitAll();
                return group;
            }
            catch (Exception e)
            {
                log.Warn(string.Format("An error occurred while create term Group. group Name:{0}, error:{1}", groupInfo.Name, e.ToString()));
                AddMMSFailedReportWithException(e, groupInfo.Name,"Term Group");             
                return null;
            }
#if PerformanceLog
            }
#endif
        }

        private static bool IsUnauthorizedException(Exception ex)
        {
            bool isUnauthorizedException = false;
            if (ex == null)
            {
                isUnauthorizedException= false;
            }
            else
            {
                if (string.Equals(ex.GetType().FullName, "Microsoft.SharePoint.Client.ServerUnauthorizedAccessException", StringComparison.OrdinalIgnoreCase))
                {
                    isUnauthorizedException= true;
                }
                else
                {
                    isUnauthorizedException= IsUnauthorizedException(ex.InnerException);
                }
            }
            log.Info($"Validate Exception.{ex?.GetType().FullName},isUnauthorizedException:{isUnauthorizedException}");
            return isUnauthorizedException;
        }

        //只还原termset，不还原下属的term
        public IAveTermSet RestoreTermSetSelf(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
            string termSetName = termSetInfo.Name;
            IAveTermSet termSet = null;
            try
            {
                termSet = group.TermSets[termSetName];
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
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

        public IAveTermSet RestoreTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo, bool isNewCreatedGroup, AveTermStoreCacheInfo cacheInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTermSet"))
            {
#endif
                _currentTermSetCalculator = new TermSetCommitCalc(termSetInfo);
                string termSetName = termSetInfo.Name;
                IAveTermSet termSet = null;
                bool isNewCreated = false;
                if (!isNewCreatedGroup)
                {
                    try
                    {
                        termSet = group.TermSets[termSetName];
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetTermByNameError, e.ToString());
                    }
                }
                if (termSet == null)
                {
                    //system Group不允许创建TermSet
                    if (group.IsSystemGroup)
                    {
                        return null;
                    }
                    termSet = CreateTermSet(group, termSetInfo);
                    if (termSet == null)
                    {
                        return null;
                    }
                    isNewCreated = true;
                }

                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                {
                    IAveTerm term = RestoreTerm(termSet, termInfo, isNewCreated, cacheInfo);
                    if (term != null && !TermIdMapping.ContainsKey(termInfo.Id))
                    {
                        TermIdMapping.Add(termInfo.Id, term.ID);
                    }
                    if (term != null && cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(termInfo.Id))
                    {
                        cacheInfo.TermIdMapping.Add(termInfo.Id, term.ID);
                    }
                }
                //Update Children
                if (_currentTermSetCalculator.TermSetNeedCommit) termSet.TermStore.CommitAll();
                //对于已经存在的termSet不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    var tmp = RestoreCustomSortOrder(termSetInfo.CustomSortOrder);
                    if (!string.Equals(tmp, termSet.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        //当修改当前级别的属性和添加或者修改它sub的时候，最后CommitAll()的时候会抛 Term update failed because of save conflict这个错误。
                        //restoreterm和restoresubterm的修改同理
                        termSet.TermStore.CommitAll();
                        termSet.CustomSortOrder = tmp;
                        termSet.TermStore.CommitAll();
                    }
                }
                if (termSetInfo.CustomProperties != null && termSetInfo.CustomProperties.Count > 0)
                {
                    foreach (KeyValuePair<string, string> pair in termSetInfo.CustomProperties)
                    {
                        if (pair.Key.StartsWith("_Sys_Nav_AttachedWeb", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        else if (pair.Key.StartsWith("_Sys_Nav_TargetUrlForChildTerms", StringComparison.OrdinalIgnoreCase))
                        {
                            termSet.SetCustomProperty(pair.Key, FixNavigationTermUrl(pair.Value));
                        }
                        else if (pair.Key.StartsWith("_Sys_Nav_CatalogTargetUrlForChildTerms", StringComparison.OrdinalIgnoreCase))
                        {
                            termSet.SetCustomProperty(pair.Key, FixNavigationTermUrl(pair.Value));
                        }
                        else
                        {
                            termSet.SetCustomProperty(pair.Key, pair.Value);
                        }
                    }
                }
                return termSet;
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="customSortOrder"></param>
        /// <param name="termSet"></param>
        private string RestoreCustomSortOrder(string customSortOrder)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreCustomSortOrder"))
            {
#endif
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
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, string.Format("An error occurred while restoring custom sort order. Error:{0}", e.Message)));
                    log.Warn("An error occurred while restoring custom sort order. Error:{0}", e);
                }
                return sortOrder;
#if PerformanceLog
            }
#endif
        }

        public IAveTermSet CreateTermSet(IAveTaxonomyGroup group, AveTermSetInfo termSetInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateTermSet"))
            {
#endif
                try
                {
                    //保持termset ID和源端一致，如果Id被占用在CreateTermSet方法中会判断
                    IAveTermSet termSet = null;
                    if (group.IsSiteCollectionGroup)
                    {
                        termSet = group.CreateTermSet(termSetInfo.Name, Guid.NewGuid());
                    }
                    else
                    {
                        termSet = group.CreateTermSet(termSetInfo.Name, termSetInfo.Id);
                    }
                    try
                    {
                        termSet.Description = termSetInfo.Description;
                        //termSet.Owner = termSetInfo.Owner;
                        string owner = termSetInfo.Owner;
                        try
                        {
                            owner = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(owner);
                            termSet.Owner = owner;
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("An error occurred while set term set owner. term set:{0}, owner:{1}.error:{2}", termSetInfo.Name, owner, e.Message)));
                            log.Warn("An error occurred while set term set owner. term set:{0}, owner:{1}.error:{2}", termSetInfo.Name, owner, e.ToString());
                        }

                        termSet.Contact = termSetInfo.Contact;
                        termSet.IsOpenForTermCreation = termSetInfo.IsOpenForTermCreation;
                        termSet.IsAvailableForTagging = termSetInfo.IsAvailableForTagging;
                        foreach (string stakeHolder in termSetInfo.Stakeholders)
                        {
                            string tStakeHolder = stakeHolder;
                            try
                            {
                                tStakeHolder = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(tStakeHolder);
                                termSet.AddStakeholder(tStakeHolder);
                            }
                            catch (Exception e)
                            {
                                report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("AddTermSetStakeholderFailed", WrapperRestoreReportResource.AddTermSetStakeholderFailed, termSetInfo.Name, stakeHolder, e.Message)));
                                log.Warn("An error occurred while add term set stakeholder. term set:{0}, stakeholder:{1}. error:{2}.", termSetInfo.Name, stakeHolder, e.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("SetTermSetPropertiesFailed", WrapperRestoreReportResource.SetTermSetPropertiesFailed, termSetInfo.Name, e.Message)));
                        log.Warn(string.Format("An error occurred while set term set property. term set name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
                    }
                    termSet.TermStore.CommitAll();
                    return termSet;
                }
                catch (Exception e)
                {
                    AddMMSFailedReportWithException(e, termSetInfo.Name, "Term Set");
                    log.Warn(string.Format("An error occurred while create termSet. termSet Name:{0}, error:{1}", termSetInfo.Name, e.ToString()));
                    return null;
                }
#if PerformanceLog
            }
#endif
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

        public IAveTerm RestoreTerm(IAveTermSet termSet, AveTermInfo termInfo, bool isNewCreatedTermSet, AveTermStoreCacheInfo cacheInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreTerm"))
            {
#endif
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

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    IAveTerm sTerm = RestoreSubTerm(term, subTerm, isNewCreated, cacheInfo);
                    if (sTerm != null && !TermIdMapping.ContainsKey(subTerm.Id))
                    {
                        TermIdMapping.Add(subTerm.Id, sTerm.ID);
                    }
                    if (sTerm != null && cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                    {
                        cacheInfo.TermIdMapping.Add(subTerm.Id, sTerm.ID);
                    }
                }
                RestoreTermProperties(term, termInfo);
                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    var tmp = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(tmp, term.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        term.TermStore.CommitAll();
                        term.CustomSortOrder = tmp;
                        term.TermStore.CommitAll();
                    }
                }
                if (_currentTermSetCalculator.TermNeedCommit(termInfo.Id, false))
                {
                    term.TermStore.CommitAll();
                }
                return term;
#if PerformanceLog
            }
#endif
        }

        private void RestoreTermProperties(IAveTerm term, AveTermInfo termInfo)
        {
            if ((termInfo.CustomProperties != null && termInfo.CustomProperties.Count > 0) || (termInfo.LocalCustomProperties != null && termInfo.LocalCustomProperties.Count > 0))
            {
                try
                {
                    if (!termInfo.IsPinned)
                    {
                        foreach (KeyValuePair<string, string> pair in termInfo.CustomProperties)
                        {
                            if (pair.Key.StartsWith("_Sys_Nav_AttachedWeb", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            term.SetCustomProperty(pair.Key, pair.Value);
                        }
                    }
                    //if (termInfo.IsReused)
                    //{
                    //    term.DeleteAllLocalCustomProperties();
                    //}
                    foreach (KeyValuePair<string, string> pair in termInfo.LocalCustomProperties)
                    {
                        if ("_Sys_Nav_SimpleLinkUrl".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_TargetUrl".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_TargetUrlForChildTerms".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_CatalogTargetUrl".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_CatalogTargetUrlForChildTerms".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_CategoryImageUrl".Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
                            || "_Sys_Nav_AssociatedFolderUrl".Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            term.SetLocalCustomProperty(pair.Key, FixNavigationTermUrl(pair.Value));
                        }
                        else
                        {
                            term.SetLocalCustomProperty(pair.Key, pair.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, "Error occurred when set term custom property" + e.ToString());
                }
            }
        }

        private string FixNavigationTermUrl(string sourceUrl)
        {
            if (string.IsNullOrEmpty(sourceUrl))
            {
                return sourceUrl;
            }
            else
            {
                //AveSiteMappingManager siteMappingManager = mAveSPSite.MappingManager.SiteMappingManager;
                //ReplaceOption replaceOption = new ReplaceOption(true, true);
                //return AveReplaceProcessor.UrlReplace(sourceUrl, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);

                //AOSBR-4448,对于term中的url，由于是个全局的url，一般指向特定的site collection，不需要根据site collection的mapping进行替换，目前只对跨tenant的情况进行tenant url的替换
                string sourceTenantUrl = AveUrlUtility.GetServerUrl(mAveSPSite.SourceSiteInfo.Url);
                string destTenantUrl = AveUrlUtility.GetServerUrl(mAveSPSite.SiteUrl);
                if (sourceUrl.StartsWith(sourceTenantUrl) && !sourceTenantUrl.Equals(destTenantUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return sourceUrl.Replace(sourceTenantUrl, destTenantUrl);
                }
                return sourceUrl;
            }
        }

        public IAveTerm CreateTerm(IAveTermSet termSet, AveTermInfo termInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateTerm"))
            {
#endif
                try
                {
                    IAveTerm term = null;
                    if (termInfo.IsPinned)
                    {
                        if (!termInfo.PinSourceTermSetId.Equals(Guid.Empty))
                        {
                            //Pin Term 需要等Source Term还原以后再创建
                            if (!PinIdMapping.ContainsKey(termInfo.ParentTermSetId))
                            {
                                Dictionary<Guid, Guid> temp = new Dictionary<Guid, Guid>();
                                temp.Add(termInfo.Id, termInfo.PinSourceTermSetId);
                                PinIdMapping.Add(termInfo.ParentTermSetId, temp);
                            }
                            else
                            {
                                PinIdMapping[termInfo.ParentTermSetId].Add(termInfo.Id, termInfo.PinSourceTermSetId);
                            }
                        }
                        return null;
                    }
                    else if (termInfo.IsReused)
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
                            term.TermStore.CommitAll();
                            return term;
                        }
                    }

                    //保持term ID和源端一致，如果Id被占用在CreateTerm方法中会判断
                    if (term == null)
                    {
                        if (termSet.Group.IsSiteCollectionGroup)
                        {
                            term = termSet.CreateTerm(termInfo.Name, DefaultLCID, Guid.NewGuid());
                        }
                        else
                        {
                            term = termSet.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                        }
                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(termInfo.Description))
                        {
                            term.SetDescription(termInfo.Description, DefaultLCID);
                        }
                        string owner = termInfo.Owner;
                        try
                        {
                            owner = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(owner);
                            term.Owner = owner;
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("SetTermOwnerFailed", WrapperRestoreReportResource.SetTermOwnerFailed, termInfo.Name, owner, e.Message)));
                            log.Warn("An error occurred while set term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e.ToString());
                        }
                        term.IsAvailableForTagging = termInfo.IsAvailableForTagging;
                        if (termInfo.IsReused && termInfo.IsDeprecated)   //SAAS-11627 吧Deprecated的term的Id保存，最后处理
                        {
                            NeedDeprecateTermId.Add(term.ID);
                        }
                        else
                        {
                            term.Deprecate(termInfo.IsDeprecated);
                        }
                        foreach (AveLableInfo labelInfo in termInfo.Labels)
                        {
                            if (labelInfo.Value.Equals(term.Name))
                            {
                                continue;
                            }
                            term.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("SetTermPropertiesFailed", WrapperRestoreReportResource.SetTermPropertiesFailed, termInfo.Name, e.Message)));
                        log.Warn(string.Format("An error occurred while set term property. termName:{0}, error:{1}", termInfo.Name, e.ToString()));
                    }

                    term.TermStore.CommitAll();
                    return term;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Failed, AveWrapperHandleErrorMessage.ConvertErrorMessageToXML("CreateTermFailed", WrapperRestoreReportResource.CreateTermFailed, termInfo.Name, e.Message)));
                    log.Warn(string.Format("An error occurred while Create term. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.RestoreSubTerm"))
            {
#endif
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

                foreach (AveTermInfo subTerm in termInfo.Terms)
                {
                    IAveTerm ssTerm = RestoreSubTerm(sTerm, subTerm, isNewCreated);
                    if (ssTerm != null && !TermIdMapping.ContainsKey(subTerm.Id))
                    {
                        TermIdMapping.Add(subTerm.Id, ssTerm.ID);
                    }
                    if (ssTerm != null && cacheInfo != null && !cacheInfo.TermIdMapping.ContainsKey(subTerm.Id))
                    {
                        cacheInfo.TermIdMapping.Add(subTerm.Id, ssTerm.ID);
                    }
                }
                RestoreTermProperties(sTerm, termInfo);
                //对于已经存在的term不会去还原它的CustomSortOrder
                if (isNewCreated)
                {
                    var tmp = RestoreCustomSortOrder(termInfo.CustomSortOrder);
                    if (!string.Equals(tmp, sTerm.CustomSortOrder, StringComparison.OrdinalIgnoreCase))
                    {
                        sTerm.TermStore.CommitAll();
                        sTerm.CustomSortOrder = tmp;
                        sTerm.TermStore.CommitAll();
                    }
                }
                if (_currentTermSetCalculator.TermNeedCommit(termInfo.Id, false))
                {
                    sTerm.TermStore.CommitAll();
                }
                return sTerm;
#if PerformanceLog
            }
#endif
        }

        public IAveTerm CreateSubTerm(IAveTerm term, AveTermInfo termInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveMetadataService.CreateSubTerm"))
            {
#endif
                try
                {
                    List<string> labelNames = new List<string>();
                    IAveTerm sTerm = null;
                    if (termInfo.IsPinned)
                    {
                        if (!termInfo.PinSourceTermSetId.Equals(Guid.Empty))
                        {
                            if (!PinIdMapping.ContainsKey(termInfo.ParentTermId))
                            {
                                Dictionary<Guid, Guid> temp = new Dictionary<Guid, Guid>();
                                temp.Add(termInfo.Id, termInfo.PinSourceTermSetId);
                                PinIdMapping.Add(termInfo.ParentTermId, temp);
                            }
                            else
                            {
                                PinIdMapping[termInfo.ParentTermId].Add(termInfo.Id, termInfo.PinSourceTermSetId);
                            }
                        }
                        return null;
                    }
                    else if (termInfo.IsReused && TermIdMapping != null && TermIdMapping.ContainsKey(termInfo.Id))
                    {
                        //reused的term在源端的term id是一样的，如果直接新建会导致TermIdMapping中的关系被覆盖，此处实现还原reused term状态
                        Guid sourceTermId = TermIdMapping[termInfo.Id];
                        IAveTerm sourceTerm = term.TermStore.GetTerm(sourceTermId);
                        sTerm = term.ReuseTerm(sourceTerm, false);
                        foreach (IAveLabel label in sourceTerm.Labels)
                        {
                            labelNames.Add(label.Value);
                        }
                    }
                    else
                    {
                        //保持term ID和源端一致，如果Id被占用在CreateTerm方法中会判断
                        if (term.TermSet.Group.IsSiteCollectionGroup)
                        {
                            sTerm = term.CreateTerm(termInfo.Name, DefaultLCID, Guid.NewGuid());
                        }
                        else
                        {
                            sTerm = term.CreateTerm(termInfo.Name, DefaultLCID, termInfo.Id);
                        }

                    }
                    try
                    {
                        if (!string.IsNullOrEmpty(termInfo.Description))
                        {
                            sTerm.SetDescription(termInfo.Description, DefaultLCID);
                        }
                        //sTerm.Owner = termInfo.Owner;
                        string owner = termInfo.Owner;
                        try
                        {
                            owner = mAveSPSite.SPMembers.CreateAndFindMemberLoginName(owner);
                            sTerm.Owner = owner;
                        }
                        catch (Exception e)
                        {
                            report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, string.Format("An error occurred while set sub term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e.Message)));
                            log.Warn("An error occurred while set sub term owner. term:{0}, owner:{1}.error:{2}", termInfo.Name, owner, e.ToString());
                        }
                        sTerm.IsAvailableForTagging = termInfo.IsAvailableForTagging;
                        if (termInfo.IsReused && termInfo.IsDeprecated) //SAAS-7352 [RP]one way mapping中被deprecate的sub term转移失败
                        {
                            NeedDeprecateTermId.Add(sTerm.ID);
                        }
                        else
                        {
                            sTerm.Deprecate(termInfo.IsDeprecated);
                        }
                        foreach (AveLableInfo labelInfo in termInfo.Labels)
                        {
                            if (labelInfo.Value.Equals(sTerm.Name) || labelNames.Contains(sTerm.Name))
                            {
                                continue;
                            }
                            sTerm.CreateLabel(labelInfo.Value, labelInfo.Language, labelInfo.IsDefaultForLanguage);
                        }
                    }
                    catch (Exception e)
                    {
                        report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Skipped, string.Format("An error occurred while set subTerm property. subTermName:{0}, error:{1}", termInfo.Name, e.Message)));
                        log.Warn(string.Format("An error occurred while set subTerm property. subTermName:{0}, error:{1}", termInfo.Name, e.ToString()));
                    }

                    sTerm.TermStore.CommitAll();
                    return sTerm;
                }
                catch (Exception e)
                {
                    report.AddDetail(new AveWrapperReportDto("RestoreMetadataService", "RestoreMetadataService", AveReportObjectType.RestoreMetadataService, AveStatus.Failed, string.Format("An error occurred while create subTerm. term Name:{0}, error:{1}", termInfo.Name, e.Message)));
                    log.Warn(string.Format("An error occurred while create subTerm. term Name:{0}, error:{1}", termInfo.Name, e.ToString()));
                    return null;
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 还原所有 Pin Term
        /// </summary>
        /// <param name="termStore"></param>
        public void PostProcessPinTerms(IAveTermStore termStore)
        {
            if (PinIdMapping != null)
            {
                foreach (Guid termOrTermSetId in PinIdMapping.Keys)
                {
                    Guid termOrTermSetIdAfterMapping = Guid.Empty;
                    if (TermSetIdMapping.TryGetValue(termOrTermSetId, out termOrTermSetIdAfterMapping))
                    {
                        IAveTermSet parentTermSet = termStore.GetTermSet(termOrTermSetIdAfterMapping);
                        foreach (Guid pinTermId in PinIdMapping[termOrTermSetId].Keys)
                        {
                            Guid sourceTermSetId = PinIdMapping[termOrTermSetId][pinTermId];
                            IAveTerm pinTerm = GetSourcePinTerm(termStore, pinTermId, sourceTermSetId);
                            if (pinTerm != null)
                            {
                                parentTermSet.PinTerm(pinTerm);
                            }
                            else
                            {
                                log.Warn("Can not get source pin term {0}, Restore pin term failed", pinTermId);
                            }
                        }
                    }
                    else if (TermIdMapping.TryGetValue(termOrTermSetId, out termOrTermSetIdAfterMapping))
                    {
                        IAveTerm parentTerm = termStore.GetTerm(termOrTermSetIdAfterMapping);
                        foreach (Guid pinTermId in PinIdMapping[termOrTermSetId].Keys)
                        {
                            Guid sourceTermSetId = PinIdMapping[termOrTermSetId][pinTermId];
                            IAveTerm pinTerm = GetSourcePinTerm(termStore, pinTermId, sourceTermSetId);
                            if (pinTerm != null)
                            {
                                parentTerm.PinTerm(pinTerm);
                            }
                            else
                            {
                                log.Warn("Can not get source pin term {0}, Restore pin term failed", pinTermId);
                            }
                        }
                    }
                    termStore.CommitAll();
                }
                #region original pinterm
                /*foreach (Guid pinTermId in PinIdMapping.Keys)
                {
                    foreach (Guid termOrTermSetId in PinIdMapping[pinTermId].Keys)
                    {
                        Guid parentTermSetId = PinIdMapping[pinTermId][termOrTermSetId];
                        Guid pinTermIdAfterMapping = Guid.Empty;
                        Guid parentTermSetIdAfterMapping = Guid.Empty;
                        if (TermIdMapping.ContainsKey(pinTermId) && TermSetIdMapping.ContainsKey(parentTermSetId))
                        {
                             pinTermIdAfterMapping = TermIdMapping[pinTermId];
                             parentTermSetIdAfterMapping = TermSetIdMapping[parentTermSetId];
                        }
                        //SAAS-7811 同tenant下,pin global term.  pinTermId和sourceTermSetId不变  
                        else if (GlobalTermSetIds.Contains(parentTermSetId))
                        {
                            pinTermIdAfterMapping = pinTermId;
                            parentTermSetIdAfterMapping = parentTermSetId;
                        }
                        else { continue; }
                      
                        if (TermSetIdMapping.ContainsKey(termOrTermSetId))
                        {
                            Guid termSetIdAfterMapping = TermSetIdMapping[termOrTermSetId];
                            IAveTerm pinTerm = termStore.GetTerm(parentTermSetIdAfterMapping, pinTermIdAfterMapping);
                            if (pinTerm != null)
                            {
                                termStore.GetTermSet(termSetIdAfterMapping).PinTerm(pinTerm);
                            }
                        }
                        else if (TermIdMapping.ContainsKey(termOrTermSetId))
                        {
                            Guid termIdAfterMapping = TermIdMapping[termOrTermSetId];
                            IAveTerm pinTerm = termStore.GetTerm(parentTermSetIdAfterMapping, pinTermIdAfterMapping);
                            if (pinTerm != null)
                            {
                                termStore.GetTerm(termIdAfterMapping).PinTerm(pinTerm);
                            }
                        }
                        //termStore.CommitAll();
                
                    }
                }*/
                #endregion
            }
        }

        public IAveTerm GetSourcePinTerm(IAveTermStore termStore, Guid pinTermId, Guid sourceTermSetId)
        {
            Guid pinTermIdAfterMapping = Guid.Empty;
            Guid sourceTermSetIdAfterMapping = Guid.Empty;
            if (TermIdMapping.ContainsKey(pinTermId) && TermSetIdMapping.ContainsKey(sourceTermSetId))
            {
                pinTermIdAfterMapping = TermIdMapping[pinTermId];
                sourceTermSetIdAfterMapping = TermSetIdMapping[sourceTermSetId];
            }  
            else
            {
                // 如果获取不到可能为同同tenant下,pin global term.  pinTermId和sourceTermSetId不变，直接用原id去getterm 
                pinTermIdAfterMapping = pinTermId;
                sourceTermSetIdAfterMapping = sourceTermSetId;
            }
            return termStore.GetTerm(sourceTermSetIdAfterMapping, pinTermIdAfterMapping);
        }

        /// <summary>
        /// 还原所有的Depercate reuse terms 
        /// </summary>
        /// <param name="termStore"></param>
        public void PostProcessReuseTerms(IAveTermStore termStore)
        {
            foreach (Guid depercateTermId in NeedDeprecateTermId)
            {
                IAveTerm term = termStore.GetTerm(depercateTermId);
                term.Deprecate(true);
                termStore.CommitAll();   //这里单个提交会影响效率，以后需要修改成提交一次。
            }

        }

        public void Dispose()
        {
            if(report != null)
            {
                report.Dispose();
            }
        }
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.CalculatorTermSet"))
            {
#endif

                int setTermNumber = Calculator(new AveTermInfo() { Id = set.Id, Terms = set.Terms }, termStatus);
                if (setTermNumber > MAXSetTermNumber)
                {
                    foreach (var sub in set.Terms)
                    {
                        termStatus[sub.Id] = true;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private int Calculator(AveTermInfo term, Dictionary<Guid, bool> termStatus)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.Calculator"))
            {
#endif
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
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.TermSetCommitCalc.TermNeedCommit"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

    }
}
