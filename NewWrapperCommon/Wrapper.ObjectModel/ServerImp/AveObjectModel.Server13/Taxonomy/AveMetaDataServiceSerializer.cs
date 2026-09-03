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



namespace AvePoint.ObjectModel.Server13
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Taxonomy;
    using System.Threading;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using System.Xml;
    using System.Reflection;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    #endregion

    internal class AveMetadataServiceCache
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMetadataServiceCache));
        //siteId, termstoreId, termstoreInfo
        private static Dictionary<Guid, Dictionary<Guid, AveTermStoreInfo>> CacheTermStoreInfos = new Dictionary<Guid, Dictionary<Guid, AveTermStoreInfo>>();

        public static Dictionary<Guid, AveTermStoreInfo> CacheRelatedTermStoreInfos = new Dictionary<Guid, AveTermStoreInfo>();
        public static Dictionary<Guid, AveMetadataGroupInfo> CacheRelatedGroupInfos = new Dictionary<Guid, AveMetadataGroupInfo>();
        public static Dictionary<Guid, AveTermSetInfo> CacheRelatedTermSetInfos = new Dictionary<Guid, AveTermSetInfo>();
        public static Dictionary<Guid, AveTermInfo> CacheRelatedTermInfos = new Dictionary<Guid, AveTermInfo>();

        public static AveTermStoreInfo GetTermStoreInfo(TermStore termStore, AveMetaDataServiceSerializer serializer)
        {
            Guid termStoreId = termStore.Id;
            Guid currentSiteId = serializer.CurrentSiteId;
            AveTermStoreInfo termStoreInfo = null;
            if (CacheTermStoreInfos.ContainsKey(currentSiteId) && CacheTermStoreInfos[currentSiteId].ContainsKey(termStoreId))
            {
                try
                {
                    termStoreInfo = CacheTermStoreInfos[currentSiteId][termStoreId];
                    Guid uniqueId = termStoreInfo.UniqueId;
                    bool changed = false;
                    var changes = termStore.GetChanges(CacheTermStoreInfos[currentSiteId][termStoreId].LastAccessTime);
                    if (changes != null && changes.Count > 0)
                    {
                        changed = true;
                        logger.Debug("Term store: {0} has {1} changed items after: {2}", termStore.Name, changes.Count, CacheTermStoreInfos[currentSiteId][termStoreId].LastAccessTime);
                    }
                    if (changed)
                    {
                        AveTermStoreInfo cacheInfo = new AveTermStoreInfo();
                        cacheInfo = serializer.GetTermStoreInfo(termStore);
                        cacheInfo.LastAccessTime = DateTime.UtcNow;
                        cacheInfo.UniqueId = uniqueId;
                        lock (CacheTermStoreInfos)
                        {
                            CacheTermStoreInfos[currentSiteId][termStoreId] = cacheInfo;
                        }
                        termStoreInfo = CacheTermStoreInfos[currentSiteId][termStoreId];
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while GetTermStoreInfo. TermStoreName: {0}, error: {1}", termStore.Name, e.ToString());
                }
            }
            else
            {
                try
                {
                    AveTermStoreInfo cacheInfo = new AveTermStoreInfo();
                    cacheInfo = serializer.GetTermStoreInfo(termStore);
                    cacheInfo.LastAccessTime = DateTime.UtcNow;
                    cacheInfo.UniqueId = Guid.NewGuid();
                    lock (CacheTermStoreInfos)
                    {
                        if (!CacheTermStoreInfos.ContainsKey(currentSiteId))
                        {
                            CacheTermStoreInfos[currentSiteId] = new Dictionary<Guid, AveTermStoreInfo>();
                        }
                        CacheTermStoreInfos[currentSiteId][termStoreId] = cacheInfo;
                        termStoreInfo = CacheTermStoreInfos[currentSiteId][termStoreId];
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while GetTermStoreInfo. TermStoreName: {0}, exception: {1}", termStore.Name, e.ToString());
                }
            }
            return termStoreInfo;
        }

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
                        Dictionary<Guid, List<Guid>> temp = new Dictionary<Guid, List<Guid>>();
                        foreach (var siteId in CacheTermStoreInfos.Keys)
                        {
                            temp[siteId] = CacheTermStoreInfos[siteId].Keys.ToList<Guid>();
                        }
                        foreach (var siteId in temp.Keys)
                        {
                            foreach (var termStoreId in temp[siteId])
                            {
                                if (CacheTermStoreInfos[siteId][termStoreId].LastAccessTime.AddMinutes(IdleTime) < DateTime.UtcNow)
                                {
                                    lock (CacheTermStoreInfos)
                                    {
                                        CacheTermStoreInfos[siteId].Remove(termStoreId);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (CacheTermStoreInfos)
                        {
                            CacheTermStoreInfos.Clear();
                        }
                    }
                    Thread.Sleep(30 * 60 * 1000);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Monitor cache data failed: {0}", ex.ToString());
            }
        }
    }
    /// <summary>
    /// 为了合并RelatedMetadata中得到的AveTermStoreInfo
    /// </summary>
    internal class AveRelatedMetadataServiceCache
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMetadataServiceCache));

        public Dictionary<Guid, AveTermStoreInfo> termStoreInfoCache = new Dictionary<Guid, AveTermStoreInfo>();
        public Dictionary<Guid, AveMetadataGroupInfo> groupInfoCache = new Dictionary<Guid, AveMetadataGroupInfo>();
        public Dictionary<Guid, AveTermSetInfo> termSetInfoCache = new Dictionary<Guid, AveTermSetInfo>();
        public Dictionary<Guid, AveTermInfo> termInfoCache = new Dictionary<Guid, AveTermInfo>();

        public List<AveTermStoreInfo> GetResult()
        {
            List<AveTermStoreInfo> result = new List<AveTermStoreInfo>();
            foreach (AveTermStoreInfo info in termStoreInfoCache.Values)
            {
                result.Add(info);
            }
            return result;
        }
        public void AddToTermInfoCache(AveTermInfo termInfo)// bool hasFound
        {
            try
            {
                while (termInfoCache.ContainsKey(termInfo.Id))
                {
                    if (termInfo.Terms.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        termInfo = termInfo.Terms[0];
                    }
                }
                while (termInfo.Terms.Count > 0)
                {
                    termInfoCache.Add(termInfo.Id, termInfo);
                    termInfo = termInfo.Terms[0];
                }
                termInfoCache.Add(termInfo.Id, termInfo);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while adding Metadata Info to RelatedCache. Error: {0}", ex.ToString());
            }
        }
    }

    internal class AveMetaDataServiceSerializer : IAveMetaDataServiceSerializer
    {
        private SPSite m_Site;
        private Guid serviceApplicationId;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMetaDataServiceSerializer));
        private readonly int maxTermDepth = 100;


        public bool SkipGlobalTermGroup { get; set; }

        internal Guid CurrentSiteId
        {
            get
            {
                if (m_Site != null)
                {
                    return m_Site.ID;
                }
                else
                {
                    return Guid.Empty;
                }
            }
        }

        public bool EnableCache { get; set; }

        public AveMetaDataServiceSerializer(Guid serviceAppId)
        {
            this.serviceApplicationId = serviceAppId;
        }

        public AveMetaDataServiceSerializer(SPSite site)
        {
            m_Site = site;
        }

        //MetaData Serializer 的新逻辑, 需要和外围确定后再做修改; 现在需要使用老方法;
        public object GetObjectData2()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetObjectData2"))
            {

                AveManagedMetadataServiceApplicationInfo metadataServiceApplicationInfo = new AveManagedMetadataServiceApplicationInfo();

                //Get the managed metadata service application object
                Type serviceAppType = Type.GetType(ServiceApplicationType.ManagedMetadataServiceApplication);
                object[] parameters = new object[] { this.serviceApplicationId };
                object serviceAppObj = AveAssemblyUtility.InvokeStaticMethod(serviceAppType, "GetApplicationById", parameters);

                //Get the managed metadata service application properties

                metadataServiceApplicationInfo.Name = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "Name");
                metadataServiceApplicationInfo.DatabaseName = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "DatabaseName");
                metadataServiceApplicationInfo.DatabaseServer = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "DatabaseServer");
                metadataServiceApplicationInfo.UseWindowsAuthentication = (bool)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "UseWindowsAuthentication");
                metadataServiceApplicationInfo.SqlAuthenticationUserName = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "SqlAuthenticationUserName");
                metadataServiceApplicationInfo.SqlAuthenticationUserPassword = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "SqlAuthenticationPassword");
                metadataServiceApplicationInfo.FailoverDatabaseServer = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "FailoverDatabaseServer");

                Type serviceAppUtilType = Type.GetType(ServiceApplicationType.ManagedMetadataServiceApplicationUtilities);
                Guid partionId = (Guid)AveAssemblyUtility.GetFieldValue(null, serviceAppUtilType, "DefaultPartitionId");

                Uri hubUri = (Uri)AveAssemblyUtility.InvokeMethod(serviceAppObj, "GetContentTypeSyndicationHubLocal", new object[] { partionId });
                if (hubUri != null)
                {
                    metadataServiceApplicationInfo.ContentTypeHub = hubUri.ToString();
                }

                SPIisWebServiceApplicationPool appPool = (SPIisWebServiceApplicationPool)AveAssemblyUtility.InvokeMethod(serviceAppObj, "ApplicationPool");
                AveIisWebServiceApplicationPoolInfo appPoolInfo = new AveIisWebServiceApplicationPoolInfo();
                appPoolInfo.Name = appPool.Name;
                metadataServiceApplicationInfo.ApplicationPool = appPoolInfo;
                metadataServiceApplicationInfo.IsErrorReportEnabled = (bool)AveAssemblyUtility.InvokeMethod(serviceAppObj, "GetIsSyndicationErrorReportEnabledLocal", new object[] { partionId });
                return metadataServiceApplicationInfo;

            }

        }

        static void CheckMetadataApplicationAvailable(TaxonomySession session)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.IsMetadataApplicationAvailable"))
            {

                session.TermStores.Any((store) =>
                    {
                        var tmp = store.TermStoreAdministrators;
                        return store.Groups.Any((group) => { return group.Contributors != null; });
                    });

            }

        }

        public List<AveTermStoreInfo> GetObjectData()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetObjectData"))
            {

                TaxonomySession taxonomySession = new TaxonomySession(m_Site, true);
                List<AveTermStoreInfo> termStoreInfos = new List<AveTermStoreInfo>();
                CheckMetadataApplicationAvailable(taxonomySession);
                try
                {
                    foreach (TermStore termStore in taxonomySession.TermStores)
                    {
                        if (EnableCache)
                        {
                            AveTermStoreInfo termStoreInfo = AveMetadataServiceCache.GetTermStoreInfo(termStore, this);
                            termStoreInfos.Add(termStoreInfo);
                        }
                        else
                        {
                            AveTermStoreInfo termStoreInfo = GetTermStoreInfo(termStore);
                            termStoreInfos.Add(termStoreInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.TermStoreGetFailed, e);
                }
                return termStoreInfos;

            }

        }

        public List<AveTermStoreInfo> GetTermPropertyWebPartMetadataInfo(IAveSite site, List<string> termPropertyWebPartInfos, AveBackupOption backupOption)
        {
            AveRelatedMetadataServiceCache relatedCache = new AveRelatedMetadataServiceCache();
            TaxonomySession taxonomySession = new TaxonomySession(m_Site, true);
            foreach (var termInfo in termPropertyWebPartInfos)
            {
                RealGetTermPropertyWebPartMetadataInfo(taxonomySession, termInfo, relatedCache, backupOption);
            }
            return relatedCache.GetResult();
        }

        internal void RealGetTermPropertyWebPartMetadataInfo(TaxonomySession taxonomySession, string termInfo, AveRelatedMetadataServiceCache cache, AveBackupOption backupOption)
        {
            try
            {
                Guid termStoreId = Guid.Empty;
                Guid termSetId = Guid.Empty;
                Guid termId = Guid.Empty;
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(termInfo);
                XmlElement rootElmt = xDoc.DocumentElement;
                if (string.Equals(rootElmt.Name, "TermPropertyWebPart", StringComparison.OrdinalIgnoreCase))
                {
                    termStoreId = new Guid(rootElmt.GetAttribute("termStoreID"));
                    termSetId = new Guid(rootElmt.GetAttribute("termSetID"));
                    termId = new Guid(rootElmt.GetAttribute("termID"));
                }
                else
                {
                    logger.Warn("Can not getting the TermProperty Web Part from info {0}.", termInfo);
                    return;
                }
                TermStore termStore = taxonomySession.TermStores[termStoreId];
                TermSet termSet = termStore.GetTermSet(termSetId);
                Group termGroup = termSet.Group;
                Guid termGroupId = termGroup.Id;
                bool backupRelatedTermSets = backupOption.BackupRelatedTermSets;

                AveTermStoreInfo usedTermStoreInfo;
                if (cache.termStoreInfoCache.ContainsKey(termStoreId))
                {
                    usedTermStoreInfo = cache.termStoreInfoCache[termStoreId];
                }
                else
                {
                    usedTermStoreInfo = new AveTermStoreInfo();
                    GetRelatedTermStoreInfo(termStore, termStoreId, usedTermStoreInfo);
                    cache.termStoreInfoCache.Add(usedTermStoreInfo.Id, usedTermStoreInfo);
                }
                AveMetadataGroupInfo groupInfo;
                if (termGroupId == Guid.Empty)
                {
                    termGroupId = GetGroupIdByChild(termStore, termSetId);
                }
                if (cache.groupInfoCache.ContainsKey(termGroupId))
                {
                    groupInfo = cache.groupInfoCache[termGroupId];
                }
                else
                {
                    groupInfo = new AveMetadataGroupInfo();
                    GetRelatedMetadataGroupInfo(termStore, termGroupId, groupInfo);
                    cache.groupInfoCache.Add(groupInfo.Id, groupInfo);
                    usedTermStoreInfo.Groups.Add(groupInfo);
                }
                AveTermSetInfo termSetInfo;
                if (cache.termSetInfoCache.ContainsKey(termSetId))
                {
                    termSetInfo = cache.termSetInfoCache[termSetId];
                }
                else
                {
                    if (!backupRelatedTermSets)
                    {
                        termSetInfo = new AveTermSetInfo();
                        GetRelatedTermSetInfo(termStore, termSetId, termSetInfo);
                    }
                    else
                    {
                        termSetInfo = GetTermSetInfo(termStore.GetTermSet(termSetId));
                    }
                    cache.termSetInfoCache.Add(termSetInfo.Id, termSetInfo);
                    groupInfo.TermSets.Add(termSetInfo);
                }
                if (!backupRelatedTermSets)
                {
                    List<Guid> termIds = new List<Guid>();
                    termIds.Add(termId);
                    GetRelatedTermInfos(termStore, termIds, termSetInfo, cache);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Get WebPart Related Metadata Information Failed. Error: {0}", ex);
            }

        }

        public List<AveTermStoreInfo> GetRelatedMetadataInfo(IAveSite site, List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupOption)
        {
            AveRelatedMetadataServiceCache relatedCache = new AveRelatedMetadataServiceCache();
            TaxonomySession taxonomySession = new TaxonomySession(m_Site, true);
            foreach (AveTaxFieldInfo taxFieldInfo in taxFieldInfos)
            {
                RealGetMetadataInfo(taxonomySession, taxFieldInfo, relatedCache, backupOption);
            }
            return relatedCache.GetResult();
        }

        internal void RealGetMetadataInfo(TaxonomySession taxonomySession, AveTaxFieldInfo fieldInfo, AveRelatedMetadataServiceCache cache, AveBackupOption backupOption)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.RealGetMetadataInfo"))
            {
                try
                {
                    bool backupRelatedTermSets = backupOption.BackupRelatedTermSets;
                    TermStore termStore = null;
                    try
                    {
                        termStore = taxonomySession.TermStores[fieldInfo.SspId];
                    }
                    catch (Exception e)
                    {
                        logger.Debug("An error occurred while getting the Term Store. Term Store ID: {0}, Error: {1}", fieldInfo.SspId, e.ToString());
                        //获取不到，使用DefaultSiteCollectionTermStore
                        termStore = taxonomySession.DefaultSiteCollectionTermStore;
                        //termStore = session.DefaultKeywordsTermStore;
                        if (termStore == null)
                        {
                            termStore = taxonomySession.DefaultKeywordsTermStore;
                        }
                        if (termStore == null)
                        {
                            termStore = taxonomySession.TermStores[0];
                        }
                    }
                    #region Keywords Column
                    if (fieldInfo.IsKeywordsColumn)
                    {
                        foreach (Guid termId in fieldInfo.TermIds)
                        {
                            Term key = termStore.GetTerm(termId);
                            //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                            if (key == null)
                            {
                                foreach (TermStore tStore in taxonomySession.TermStores)
                                {
                                    if (key == null)
                                    {
                                        key = tStore.GetTerm(termId);
                                    }
                                    if (key != null)
                                    {
                                        termStore = tStore;
                                        break;
                                    }
                                }
                            }
                            if (key != null)
                            {
                                Guid realTermStoreId = key.TermStore.Id;
                                Guid realTermSetId = key.TermSet.Id;
                                Guid realGroupId = key.TermSet.Group.Id;
                                AveTermStoreInfo usedTermStoreInfo;
                                if (cache.termStoreInfoCache.ContainsKey(realTermStoreId))
                                {
                                    usedTermStoreInfo = cache.termStoreInfoCache[realTermStoreId];
                                }
                                else
                                {
                                    usedTermStoreInfo = new AveTermStoreInfo();
                                    GetRelatedTermStoreInfo(termStore, realTermStoreId, usedTermStoreInfo);
                                    cache.termStoreInfoCache.Add(usedTermStoreInfo.Id, usedTermStoreInfo);
                                }
                                AveTermSetInfo termSetInfo;
                                bool isAddTermSet = false;
                                if (cache.termSetInfoCache.ContainsKey(realTermSetId))
                                {
                                    termSetInfo = cache.termSetInfoCache[realTermSetId];
                                }
                                else
                                {
                                    termSetInfo = new AveTermSetInfo();
                                    GetRelatedTermSetInfo(termStore, realTermSetId, termSetInfo);
                                    cache.termSetInfoCache.Add(realTermSetId, termSetInfo);
                                    isAddTermSet = true;
                                    //groupInfo.TermSets.Add(termSetInfo);
                                }
                                List<Guid> termIds = new List<Guid> { key.Id };
                                GetRelatedTermInfos(termStore, termIds, termSetInfo, cache);
                                AveMetadataGroupInfo groupInfo;
                                if (cache.groupInfoCache.ContainsKey(realGroupId))
                                {
                                    groupInfo = cache.groupInfoCache[realGroupId];
                                }
                                else
                                {
                                    groupInfo = new AveMetadataGroupInfo();
                                    GetRelatedMetadataGroupInfo(termStore, realGroupId, groupInfo);
                                    cache.groupInfoCache.Add(realGroupId, groupInfo);
                                    usedTermStoreInfo.Groups.Add(groupInfo);
                                }
                                if (isAddTermSet)
                                {
                                    groupInfo.TermSets.Add(termSetInfo);
                                }
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        AveTermStoreInfo usedTermStoreInfo;
                        if (cache.termStoreInfoCache.ContainsKey(fieldInfo.SspId))
                        {
                            usedTermStoreInfo = cache.termStoreInfoCache[fieldInfo.SspId];
                        }
                        else
                        {
                            usedTermStoreInfo = new AveTermStoreInfo();
                            GetRelatedTermStoreInfo(termStore, fieldInfo.SspId, usedTermStoreInfo);
                            cache.termStoreInfoCache.Add(usedTermStoreInfo.Id, usedTermStoreInfo);
                        }
                        AveMetadataGroupInfo groupInfo;
                        if (fieldInfo.GroupId == Guid.Empty)
                        {
                            fieldInfo.GroupId = GetGroupIdByChild(termStore, fieldInfo.TermSetId);
                        }
                        if (cache.groupInfoCache.ContainsKey(fieldInfo.GroupId))
                        {
                            groupInfo = cache.groupInfoCache[fieldInfo.GroupId];
                        }
                        else
                        {
                            groupInfo = new AveMetadataGroupInfo();
                            GetRelatedMetadataGroupInfo(termStore, fieldInfo.GroupId, groupInfo);
                            cache.groupInfoCache.Add(groupInfo.Id, groupInfo);
                            usedTermStoreInfo.Groups.Add(groupInfo);
                        }
                        AveTermSetInfo termSetInfo;
                        if (cache.termSetInfoCache.ContainsKey(fieldInfo.TermSetId))
                        {
                            termSetInfo = cache.termSetInfoCache[fieldInfo.TermSetId];
                        }
                        else
                        {
                            if (!backupRelatedTermSets)
                            {
                                termSetInfo = new AveTermSetInfo();
                                GetRelatedTermSetInfo(termStore, fieldInfo.TermSetId, termSetInfo);
                            }
                            else
                            {
                                termSetInfo = GetTermSetInfo(termStore.GetTermSet(fieldInfo.TermSetId));
                            }
                            cache.termSetInfoCache.Add(termSetInfo.Id, termSetInfo);
                            groupInfo.TermSets.Add(termSetInfo);
                        }
                        if (!backupRelatedTermSets)
                        {
                            GetRelatedTermInfos(termStore, fieldInfo.TermIds, termSetInfo, cache);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Get Related Metadata Information Failed. Error: {0}", ex.ToString());
                }
            }
        }

        internal AveTermStoreInfo GetTermStoreInfo(TermStore termStore)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetTermStoreInfo"))
            {

                AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
                termStoreInfo.Name = termStore.Name;
                termStoreInfo.Id = termStore.Id;
                termStoreInfo.DefaultLanguage = termStore.DefaultLanguage;
                termStoreInfo.WorkingLanguage = termStore.WorkingLanguage;
                GetTermStoreAdministrators(termStore, termStoreInfo);
                foreach (Group group in termStore.Groups)
                {
                    try
                    {
                        if (group.IsSiteCollectionGroup && !group.SiteCollectionAccessIds.Contains(m_Site.ID))
                        {
                            continue;
                        }
                        if (!group.IsSiteCollectionGroup && SkipGlobalTermGroup)
                        {
                            continue;
                        }
                        AveMetadataGroupInfo groupInfo = GetMetadataGroupInfo(group);
                        termStoreInfo.Groups.Add(groupInfo);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while getting MetadataGroupInfo, groupName: {0}, error: {1}.", group.Name, e);
                    }
                }
                return termStoreInfo;

            }

        }

        internal void GetRelatedTermStoreInfo(TermStore termStore, Guid sspid, AveTermStoreInfo tsInfo)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetRelatedTermStoreInfo"))
            {
                lock (AveMetadataServiceCache.CacheRelatedTermStoreInfos)
                {
                    if (AveMetadataServiceCache.CacheRelatedTermStoreInfos.ContainsKey(sspid))
                    {
                        tsInfo.Name = AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid].Name;
                        tsInfo.Id = AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid].Id;
                        tsInfo.DefaultLanguage = AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid].DefaultLanguage;
                        tsInfo.WorkingLanguage = AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid].WorkingLanguage;
                        tsInfo.TermStoreAdministrators = AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid].TermStoreAdministrators;
                    }
                    else
                    {
                        AveTermStoreInfo info = new AveTermStoreInfo();
                        info.Name = termStore.Name;
                        info.Id = termStore.Id;
                        info.DefaultLanguage = termStore.DefaultLanguage;
                        info.WorkingLanguage = termStore.WorkingLanguage;
                        GetTermStoreAdministrators(termStore, info);

                        tsInfo.Name = info.Name;
                        tsInfo.Id = info.Id;
                        tsInfo.DefaultLanguage = info.DefaultLanguage;
                        tsInfo.WorkingLanguage = info.WorkingLanguage;
                        tsInfo.TermStoreAdministrators = info.TermStoreAdministrators;
                        AveMetadataServiceCache.CacheRelatedTermStoreInfos[sspid] = info;
                    }
                }
            }
        }

        private static void GetTermStoreAdministrators(TermStore termStore, AveTermStoreInfo termStoreInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetTermStoreAdministrators"))
            {

                //此处以后可以考虑将TermStoreAdministrators属性移到Common中，这样整体都可以采用IAveTermStore接口对象，避免强转
                foreach (SPAce<TaxonomyRights> administrator in termStore.TermStoreAdministrators)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = administrator.PrincipalName;
                    aceInfo.DisplayName = administrator.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)administrator.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)administrator.DenyRightsMask;
                    termStoreInfo.TermStoreAdministrators.Add(aceInfo);
                }

            }

        }

        private Guid GetGroupIdByChild(TermStore termStore, Guid termSetId)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetGroupIdByChild"))
            {
                lock (AveMetadataServiceCache.CacheRelatedTermSetInfos)
                {
                    if (AveMetadataServiceCache.CacheRelatedTermSetInfos.ContainsKey(termSetId))
                    {
                        return AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].ParentId;
                    }
                    else
                    {
                        AveTermSetInfo termSetInfo = new AveTermSetInfo();
                        TermSet termSet = termStore.GetTermSet(termSetId);
                        if (termSet == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                        }
                        termSetInfo.Name = termSet.Name;
                        termSetInfo.Id = termSet.Id;
                        termSetInfo.Description = termSet.Description;
                        termSetInfo.Contact = termSet.Contact;
                        termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
                        termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
                        termSetInfo.Owner = termSet.Owner;
                        termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
                        termSetInfo.ParentId = termSet.Group.Id;
                        foreach (string stakeholder in termSet.Stakeholders)
                        {
                            termSetInfo.Stakeholders.Add(stakeholder);
                        }
                        AveMetadataServiceCache.CacheRelatedTermSetInfos.Add(termSet.Id, termSetInfo);
                        return termSet.Group.Id;
                    }
                }
            }
        }

        private AveMetadataGroupInfo GetMetadataGroupInfo(Group group)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetMetadataGroupInfo"))
            {

                AveMetadataGroupInfo groupInfo = new AveMetadataGroupInfo();
                groupInfo.Name = group.Name;
                groupInfo.Id = group.Id;
                groupInfo.Description = group.Description;
                groupInfo.IsSystemGroup = group.IsSystemGroup;
                groupInfo.IsSiteCollectionGroup = group.IsSiteCollectionGroup;
                #region get SiteCollectionReadOnlyAccessUrls
                string[] tempArray = new string[group.SiteCollectionReadOnlyAccessUrls.Count];
                group.SiteCollectionReadOnlyAccessUrls.CopyTo(tempArray, 0);
                List<string> SiteCollectionReadOnlyAccessUrls = tempArray.ToList<string>();
                groupInfo.SiteCollectionReadOnlyAccessUrls = SiteCollectionReadOnlyAccessUrls;
                #endregion
                GetGroupInfo(group, groupInfo);
                foreach (TermSet termSet in group.TermSets)
                {
                    try
                    {
                        if (group.IsSystemGroup && group.TermStore.OrphanedTermsTermSet != null)
                        {
                            if (termSet.Id.Equals(group.TermStore.OrphanedTermsTermSet.Id))
                            {
                                continue;
                            }
                        }
                        AveTermSetInfo termSetInfo = GetTermSetInfo(termSet);
                        groupInfo.TermSets.Add(termSetInfo);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, ServerAPIResource.TermSetInfoGetFailed, termSet.Name, e);
                    }
                }
                return groupInfo;

            }

        }

        private void GetRelatedMetadataGroupInfo(TermStore termStore, Guid groupId, AveMetadataGroupInfo groupInfo)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetRelatedMetadataGroupInfo"))
            {
                lock (AveMetadataServiceCache.CacheRelatedGroupInfos)
                {
                    if (AveMetadataServiceCache.CacheRelatedGroupInfos.ContainsKey(groupId))
                    {
                        groupInfo.Name = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].Name;
                        groupInfo.Id = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].Id;
                        groupInfo.Description = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].Description;
                        groupInfo.IsSystemGroup = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].IsSystemGroup;
                        groupInfo.IsSiteCollectionGroup = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].IsSiteCollectionGroup;
                        groupInfo.GroupManagers = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].GroupManagers;
                        groupInfo.Contributors = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].Contributors;
                        groupInfo.SiteCollectionReadOnlyAccessUrls = AveMetadataServiceCache.CacheRelatedGroupInfos[groupId].SiteCollectionReadOnlyAccessUrls;
                    }
                    else
                    {
                        AveMetadataGroupInfo info = new AveMetadataGroupInfo();
                        Group group = termStore.GetGroup(groupId);
                        if (group == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindGroupWithId, groupId, termStore.Name);
                        }
                        info.Name = group.Name;
                        info.Id = group.Id;
                        info.Description = group.Description;
                        info.IsSystemGroup = group.IsSystemGroup;
                        info.IsSiteCollectionGroup = group.IsSiteCollectionGroup;
                        #region get SiteCollectionReadOnlyAccessUrls
                        string[] tempArray = new string[group.SiteCollectionReadOnlyAccessUrls.Count];
                        group.SiteCollectionReadOnlyAccessUrls.CopyTo(tempArray, 0);
                        List<string> SiteCollectionReadOnlyAccessUrls = tempArray.ToList<string>();
                        info.SiteCollectionReadOnlyAccessUrls = SiteCollectionReadOnlyAccessUrls;
                        #endregion
                        GetGroupInfo(group, info);

                        groupInfo.Name = info.Name;
                        groupInfo.Id = info.Id;
                        groupInfo.Description = info.Description;
                        groupInfo.IsSystemGroup = info.IsSystemGroup;
                        groupInfo.IsSiteCollectionGroup = info.IsSiteCollectionGroup;
                        groupInfo.Contributors = info.Contributors;
                        groupInfo.GroupManagers = info.GroupManagers;
                        groupInfo.SiteCollectionReadOnlyAccessUrls = info.SiteCollectionReadOnlyAccessUrls;
                        AveMetadataServiceCache.CacheRelatedGroupInfos[info.Id] = info;
                    }
                }
            }
        }

        private static void GetGroupInfo(Group group, AveMetadataGroupInfo groupInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetGroupInfo"))
            {

                foreach (SPAce<TaxonomyRights> contributor in group.Contributors)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = contributor.PrincipalName;
                    aceInfo.DisplayName = contributor.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)contributor.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)contributor.DenyRightsMask;
                    groupInfo.Contributors.Add(aceInfo);
                }
                foreach (SPAce<TaxonomyRights> groupManager in group.GroupManagers)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = groupManager.PrincipalName;
                    aceInfo.DisplayName = groupManager.DisplayName;
                    aceInfo.GrantRightsMask = (ulong)groupManager.GrantRightsMask;
                    aceInfo.DenyRightsMask = (ulong)groupManager.DenyRightsMask;
                    groupInfo.GroupManagers.Add(aceInfo);
                }

            }

        }

        private Dictionary<int, string> GetTermSetNames(TermSet termSet)
        {
            var namesInfo = termSet.GetType().GetProperty("Names", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Dictionary<int, string>)namesInfo.GetValue(termSet);
        }

        private AveTermSetInfo GetTermSetInfo(TermSet termSet)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetTermSetInfo"))
            {

                AveTermSetInfo termSetInfo = new AveTermSetInfo();
                //termSetInfo.Name = termSet.Name;
                if (termSet.Group.IsSystemGroup)
                {
                    try
                    {
                        termSetInfo.Name = GetTermSetNames(termSet)[termSet.TermStore.DefaultLanguage];
                        logger.Debug("Use default language name in termset name when under system group. Name property:{0}, backup name: {1}", termSet.Name, termSetInfo.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.Debug("Failed to backup default language name for termSet under system group. Exception: {0}", ex);
                    }
                }
                if (string.IsNullOrEmpty(termSetInfo.Name))
                {
                    termSetInfo.Name = termSet.Name;
                }
                termSetInfo.Id = termSet.Id;
                termSetInfo.Description = termSet.Description;
                termSetInfo.Contact = termSet.Contact;
                termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
                termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
                termSetInfo.Owner = termSet.Owner;
                termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
                foreach (string stakeholder in termSet.Stakeholders)
                {
                    termSetInfo.Stakeholders.Add(stakeholder);
                }
                termSetInfo.CustomProperties = termSet.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
                foreach (Term term in termSet.Terms)
                {
                    try
                    {
                        AveTermInfo termInfo = GetTermInfo(term, 0);
                        termSetInfo.Terms.Add(termInfo);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, ServerAPIResource.TermInfoGetFailed, term.Name, e);
                    }
                }
                return termSetInfo;

            }

        }

        private void GetRelatedTermSetInfo(TermStore termStore, Guid termSetId, AveTermSetInfo tSetInfo)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetRelatedTermSetInfo"))
            {
                lock (AveMetadataServiceCache.CacheRelatedTermSetInfos)
                {
                    if (AveMetadataServiceCache.CacheRelatedTermSetInfos.ContainsKey(termSetId))
                    {
                        tSetInfo.Name = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Name;
                        tSetInfo.Id = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Id;
                        tSetInfo.Description = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Description;
                        tSetInfo.Contact = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Contact;
                        tSetInfo.IsAvailableForTagging = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].IsAvailableForTagging;
                        tSetInfo.IsOpenForTermCreation = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].IsOpenForTermCreation;
                        tSetInfo.Owner = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Owner;
                        tSetInfo.ParentId = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].ParentId;
                        tSetInfo.CustomSortOrder = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].CustomSortOrder;
                        tSetInfo.Stakeholders = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].Stakeholders;
                        tSetInfo.CustomProperties = AveMetadataServiceCache.CacheRelatedTermSetInfos[termSetId].CustomProperties;
                    }
                    else
                    {
                        AveTermSetInfo termSetInfo = new AveTermSetInfo();
                        TermSet termSet = termStore.GetTermSet(termSetId);
                        if (termSet == null)
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                        }
                        termSetInfo.Name = termSet.Name;
                        termSetInfo.Id = termSet.Id;
                        termSetInfo.Description = termSet.Description;
                        termSetInfo.Contact = termSet.Contact;
                        termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
                        termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
                        termSetInfo.Owner = termSet.Owner;
                        termSetInfo.ParentId = termSet.Group.Id;
                        termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
                        termSetInfo.CustomProperties = termSet.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
                        foreach (string stakeholder in termSet.Stakeholders)
                        {
                            termSetInfo.Stakeholders.Add(stakeholder);
                        }

                        tSetInfo.Name = termSetInfo.Name;
                        tSetInfo.Id = termSetInfo.Id;
                        tSetInfo.Description = termSetInfo.Description;
                        tSetInfo.Contact = termSetInfo.Contact;
                        tSetInfo.IsAvailableForTagging = termSetInfo.IsAvailableForTagging;
                        tSetInfo.IsOpenForTermCreation = termSetInfo.IsOpenForTermCreation;
                        tSetInfo.Owner = termSetInfo.Owner;
                        tSetInfo.ParentId = termSetInfo.ParentId;
                        tSetInfo.CustomSortOrder = termSetInfo.CustomSortOrder;
                        tSetInfo.Stakeholders = termSetInfo.Stakeholders;
                        tSetInfo.CustomProperties = termSetInfo.CustomProperties;
                        AveMetadataServiceCache.CacheRelatedTermSetInfos[termSet.Id] = termSetInfo;
                    }
                }
            }
        }

        private void CopyTermInfo(AveTermInfo termInfo, AveTermInfo tInfo)
        {
            if (termInfo == null || tInfo == null)
            {
                return;
            }
            tInfo.Name = termInfo.Name;
            tInfo.Id = termInfo.Id;
            tInfo.Description = termInfo.Description;
            tInfo.Owner = termInfo.Owner;
            tInfo.IsDeprecated = termInfo.IsDeprecated;
            tInfo.IsReused = termInfo.IsReused;
            tInfo.IsPinned = termInfo.IsPinned;
            tInfo.IsAvailableForTagging = termInfo.IsAvailableForTagging;
            tInfo.CustomSortOrder = termInfo.CustomSortOrder;
            tInfo.CustomProperties = termInfo.CustomProperties;
            tInfo.LocalCustomProperties = termInfo.LocalCustomProperties;
            tInfo.MergedTermIds = termInfo.MergedTermIds;
            if (termInfo.ParentTermId != Guid.Empty)
            {
                tInfo.ParentTermId = termInfo.ParentTermId;
            }
            else
            {
                tInfo.ParentTermSetId = termInfo.ParentTermSetId;
            }
            tInfo.Labels = termInfo.Labels;
            tInfo.IsSourceTerm = termInfo.IsSourceTerm;
        }

        private AveTermInfo GetTermInfo(Term term, int depth)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveMetaDataServiceSerializer.GetTermInfo"))
            {

                if (depth <= maxTermDepth)
                {
                    AveTermInfo termInfo = GenerateTermInfo(term);
                    depth++;
                    foreach (Term term1 in term.Terms)
                    {
                        AveTermInfo termInfo1 = GetTermInfo(term1, depth);
                        if (termInfo1 != null)
                        {
                            termInfo.Terms.Add(termInfo1);
                        }
                    }
                    return termInfo;
                }
                else
                {
                    logger.Warn("DocAve only support term's depth less than {0}.", maxTermDepth);
                    return null;
                }

            }

        }

        private void GetRelatedTermInfos(TermStore termStore, List<Guid> termIds, AveTermSetInfo tSetInfo, AveRelatedMetadataServiceCache cache)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetRelatedTermInfos"))
            {
                foreach (Guid termId in termIds)
                {
                    if (termId != Guid.Empty)
                    {
                        //因reuse term Id是相同的，所以catch对reuse term无效,要对reuse term特殊判断
                        if (!cache.termInfoCache.ContainsKey(termId) || cache.termInfoCache[termId].IsReused)
                        {
                            var termSet = termStore.GetTermSet(tSetInfo.Id);
                            var termInfo = GetTermInfo(termSet, termId, cache);
                            AveTermInfo rtermInfo = GetRootTermInfo(termSet, termInfo, cache);
                            if (!cache.termInfoCache.ContainsKey(rtermInfo.Id))
                            {
                                tSetInfo.Terms.Add(rtermInfo);
                            }
                            cache.AddToTermInfoCache(rtermInfo);
                        }
                    }
                }
            }
        }

        private AveTermInfo GenerateTermInfo(Term term)
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = term.Name;
            termInfo.Id = term.Id;
            foreach (int lcid in term.TermStore.Languages)
            {
                termInfo.Description[lcid] = term.GetDescription(lcid);
            }
            termInfo.Owner = term.Owner;
            termInfo.IsDeprecated = term.IsDeprecated;
            termInfo.IsReused = term.IsReused;
            termInfo.IsSourceTerm = term.IsSourceTerm;
            termInfo.IsAvailableForTagging = term.IsAvailableForTagging;
            termInfo.CustomSortOrder = term.CustomSortOrder;
            termInfo.CustomProperties = term.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.LocalCustomProperties = term.LocalCustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            if (term.MergedTermIds != null)
            {
                termInfo.MergedTermIds = term.MergedTermIds.ToList();
            }
            if (term.Parent != null)
            {
                termInfo.ParentTermId = term.Parent.Id;
            }
            termInfo.ParentTermSetId = term.TermSet.Id;
            termInfo.IsPinned = term.IsPinned;
            if (term.PinSourceTermSet != null)
            {
                termInfo.PinSourceTermSetId = term.PinSourceTermSet.Id;
            }
            foreach (Label label in term.Labels)
            {
                AveLableInfo labelInfo = new AveLableInfo();
                labelInfo.IsDefaultForLanguage = label.IsDefaultForLanguage;
                labelInfo.Language = label.Language;
                labelInfo.Value = label.Value;
                termInfo.Labels.Add(labelInfo);
            }
            return termInfo;
        }


        private AveTermInfo GetTermInfo(TermSet termSet, Guid termId, AveRelatedMetadataServiceCache cache)
        {
            var tInfo = new AveTermInfo();
            lock (AveMetadataServiceCache.CacheRelatedTermInfos)
            {
                AveTermInfo catchTermInfo;
                if (AveMetadataServiceCache.CacheRelatedTermInfos.TryGetValue(termId, out catchTermInfo))
                {
                    CopyTermInfo(catchTermInfo, tInfo);
                    //Pin & Reuse & Merge =>IsReused = true
                    if (tInfo.IsReused)
                    {
                        //If it is a reused term, need to check parent info
                        CheckReuseTermParentInfo(termSet, tInfo);
                    }
                }
                //TermOnly will not backup reused Info.
                else
                {
                    Term term = termSet.GetTerm(termId);
                    if (term == null)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindTermwithId, termId, termSet.Name);
                    }
                    AveTermInfo termInfo = GenerateTermInfo(term);
                    //Term Only 不支持备份Pinned 属性，因为不能保证source term备份
                    termInfo.IsPinned = false;
                    CopyTermInfo(termInfo, tInfo);
                    AveMetadataServiceCache.CacheRelatedTermInfos[term.Id] = termInfo;
                }
            }
            return tInfo;
        }
        /// <summary>
        /// 返回term tree结构
        /// </summary>
        /// <param name="termSet"></param>
        /// <param name="tInfo"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        private AveTermInfo GetRootTermInfo(TermSet termSet, AveTermInfo tInfo, AveRelatedMetadataServiceCache cache)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Server.AveMetadataServiceSerializer.GetRootTermInfo"))
            {
                if (tInfo.ParentTermId == Guid.Empty)
                {
                    return tInfo;
                }
                AveTermInfo parentTermInfo = new AveTermInfo();
                if (cache.termInfoCache.ContainsKey(tInfo.ParentTermId))
                {
                    var catchTermInfo = cache.termInfoCache[tInfo.ParentTermId];
                    CopyTermInfo(catchTermInfo, parentTermInfo);

                    parentTermInfo.Terms.Add(tInfo);
                    if (parentTermInfo.IsReused)
                    {
                        CheckReuseTermParentInfo(termSet, parentTermInfo);
                    }
                    return parentTermInfo;
                }

                parentTermInfo = GetTermInfo(termSet, tInfo.ParentTermId, cache);
                parentTermInfo.Terms.Add(tInfo);
                return GetRootTermInfo(termSet, parentTermInfo, cache);

            }
        }


        /// <summary>
        /// In cache, use Term.Id as key, to reuse or merge or pin term, Id is same as source term
        /// use this method to check whether a reused term is the right one we will backup.
        /// Parent Info changed means cached term is not we want to backup.
        /// </summary>
        /// <param name="termSet"></param>
        /// <param name="termInfo"></param>
        /// <returns>true if parent info changed, otherwise false</returns>
        private bool CheckReuseTermParentInfo(TermSet termSet, AveTermInfo termInfo)
        {
            bool parentChanged = false;
            Term reusedTerm = termSet.GetTerm(termInfo.Id);
            if (reusedTerm != null)
            {
                if (reusedTerm.Parent != null)
                {
                    if (reusedTerm.Parent.Id != termInfo.ParentTermId)
                    {
                        termInfo.ParentTermId = reusedTerm.Parent.Id;
                        termInfo.ParentTermSetId = Guid.Empty;
                        parentChanged = true;
                    }
                }
                else
                {
                    if (termInfo.ParentTermSetId != reusedTerm.TermSet.Id)
                    {
                        termInfo.ParentTermSetId = reusedTerm.TermSet.Id;
                        termInfo.ParentTermId = Guid.Empty;
                        parentChanged = true;
                    }
                }
                termInfo.IsSourceTerm = reusedTerm.IsSourceTerm;
            }
            return parentChanged;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

    }
}
