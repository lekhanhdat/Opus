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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace AvePoint.ObjectModel.Common
{

    internal class AveMetadataServiceCache
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMetadataServiceCache));
        //siteId, termstoreId, termstoreInfo
        private static Dictionary<Guid, Dictionary<Guid, AveTermStoreInfo>> cacheTermStoreInfos = new Dictionary<Guid, Dictionary<Guid, AveTermStoreInfo>>();

        public static Dictionary<Guid, AveTermStoreInfo> cacheRelatedTermStoreInfos = new Dictionary<Guid, AveTermStoreInfo>();
        public static Dictionary<Guid, AveMetadataGroupInfo> cacheRelatedGroupInfos = new Dictionary<Guid, AveMetadataGroupInfo>();
        public static Dictionary<Guid, AveTermSetInfo> cacheRelatedTermSetInfos = new Dictionary<Guid, AveTermSetInfo>();
        public static Dictionary<Guid, AveTermInfo> cacheRelatedTermInfos = new Dictionary<Guid, AveTermInfo>();

        public static AveTermStoreInfo GetTermStoreInfo(IAveTermStore termStore, AveMetaDataServiceSerializer serializer)
        {
            Guid termStoreId = termStore.ID;
            Guid currentSiteId = serializer.CurrentSiteId;
            AveTermStoreInfo termStoreInfo = null;
            lock (cacheTermStoreInfos)
            {
                if (cacheTermStoreInfos.ContainsKey(currentSiteId) && cacheTermStoreInfos[currentSiteId].ContainsKey(termStoreId))
                {
                    try
                    {
                        termStoreInfo = cacheTermStoreInfos[currentSiteId][termStoreId];
                        Guid uniqueId = termStoreInfo.UniqueId;
                        //bool changed = false;
                        //var changes = termStore.GetChanges(cacheTermStoreInfos[currentSiteId][termStoreId].LastAccessTime);
                        //termstore不支持GetChanges
                        //if (changes != null && changes.Count > 0)
                        //{
                        //    changed = true;
                        //    logger.Debug("Term store:{0} has {1} changed items after:{2}", termStore.Name, changes.Count, cacheTermStoreInfos[currentSiteId][termStoreId].LastAccessTime);
                        //}
                        //if (changed)
                        //{
                        AveTermStoreInfo cacheInfo = new AveTermStoreInfo();
                        cacheInfo = serializer.GetTermStoreInfo(termStore);
                        cacheInfo.LastAccessTime = DateTime.UtcNow;
                        cacheInfo.UniqueId = uniqueId;
                        cacheTermStoreInfos[currentSiteId][termStoreId] = cacheInfo;
                        termStoreInfo = cacheTermStoreInfos[currentSiteId][termStoreId];
                        //}
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while GetTermStoreInfo. termStoreName:{0}, error:{1}", termStore.Name, e.ToString());
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
                        if (!cacheTermStoreInfos.ContainsKey(currentSiteId))
                        {
                            cacheTermStoreInfos[currentSiteId] = new Dictionary<Guid, AveTermStoreInfo>();
                        }
                        cacheTermStoreInfos[currentSiteId][termStoreId] = cacheInfo;
                        termStoreInfo = cacheTermStoreInfos[currentSiteId][termStoreId];
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while GetTermStoreInfo. termStoreName:{0}, exception:{1}", termStore.Name, e.ToString());
                    }
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
                    lock (cacheTermStoreInfos)
                    {
                        if (Enable)
                        {
                            Dictionary<Guid, List<Guid>> temp = new Dictionary<Guid, List<Guid>>();

                            foreach (var siteId in cacheTermStoreInfos.Keys)
                            {
                                temp[siteId] = cacheTermStoreInfos[siteId].Keys.ToList<Guid>();
                            }
                            foreach (var siteId in temp.Keys)
                            {
                                foreach (var termStoreId in temp[siteId])
                                {
                                    if (cacheTermStoreInfos[siteId][termStoreId].LastAccessTime.AddMinutes(IdleTime) < DateTime.UtcNow)
                                    {
                                        cacheTermStoreInfos[siteId].Remove(termStoreId);
                                    }
                                }
                            }
                        }
                        else
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
                logger.Warn("An error occurred while adding Metadata Info to RelatedCache. Error:{0}", ex.ToString());
            }
        }
    }

    internal class AveMetaDataServiceSerializer : IAveMetaDataServiceSerializer
    {
        private AveSite m_Site;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public bool SkipGlobalTermGroup { get; set; }
        public bool EnableCache { get; set; }
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
        public AveMetaDataServiceSerializer(AveSite site)
        {
            m_Site = site;
        }

        public List<AveTermStoreInfo> GetObjectData()
        {
            IAveTaxonomySession taxonomySession = m_Site.AveSPTaxonomySession;
            List<AveTermStoreInfo> termStoreInfos = new List<AveTermStoreInfo>();
            try
            {
                foreach (IAveTermStore termStore in taxonomySession.TermStores)
                {
                    AveTermStoreInfo termStoreInfo = GetTermStoreInfo(termStore);
                    termStoreInfos.Add(termStoreInfo);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while get metadata service object data, error:{0}", e.ToString());
            }
            return termStoreInfos;
        }

        public List<AveTermStoreInfo> GetTermPropertyWebPartMetadataInfo(IAveSite site, List<string> termPropertyWebPartInfos, AveBackupOption backupOption)
        {
            AveRelatedMetadataServiceCache relatedCache = new AveRelatedMetadataServiceCache();
            IAveTaxonomySession taxonomySession = new AveTaxonomySession(m_Site);
            foreach (var termInfo in termPropertyWebPartInfos)
            {
                RealGetTermPropertyWebPartMetadataInfo(taxonomySession, termInfo, relatedCache, backupOption);
            }
            return relatedCache.GetResult();
        }

        internal void RealGetTermPropertyWebPartMetadataInfo(IAveTaxonomySession taxonomySession, string termInfo, AveRelatedMetadataServiceCache cache, AveBackupOption backupOption)
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
                    mLog.Warn("Can not getting the TermProperty Web Part from info {0}.", termInfo);
                    return;
                }
                IAveTermStore termStore = taxonomySession.TermStores[termStoreId];
                IAveTermSet termSet = termStore.GetTermSet(termSetId);
                IAveTaxonomyGroup termGroup = termSet.Group;
                Guid termGroupId = termGroup.ID;
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
                mLog.Warn("Get WebPart Related Metadata Information Failed. Error: {0}", ex);
            }

        }

        public List<AveTermStoreInfo> GetRelatedMetadataInfo(IAveSite site, List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupOption)
        {
            AveRelatedMetadataServiceCache relatedCache = new AveRelatedMetadataServiceCache();
            IAveTaxonomySession taxonomySession = new AveTaxonomySession(m_Site);
            foreach (AveTaxFieldInfo taxFieldInfo in taxFieldInfos)
            {
                RealGetMetadataInfo(taxonomySession, taxFieldInfo, relatedCache, backupOption);
            }
            return relatedCache.GetResult();
        }

        internal void RealGetMetadataInfo(IAveTaxonomySession taxonomySession, AveTaxFieldInfo fieldInfo, AveRelatedMetadataServiceCache cache, AveBackupOption backupOption)
        {
            try
            {
                bool backupRelatedTermSets = backupOption.BackupRelatedTermSets;
                IAveTermStore termStore = null;
                try
                {
                    termStore = taxonomySession.TermStores[fieldInfo.SspId];
                }
                catch (Exception e)
                {
                    mLog.Debug("An error occurred while getting the TermStore. TermStore ID:{0}, Error:{1}", fieldInfo.SspId, e.ToString());
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
                        IAveTerm key = termStore.GetTerm(termId);
                        //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                        if (key == null)
                        {
                            foreach (IAveTermStore tStore in taxonomySession.TermStores)
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
                            Guid realTermSetId = (key as AveTerm).m_AveTermSet.ID;
                            Guid realGroupId = (key as AveTerm).m_AveTermSet.Group.ID;
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
                            List<Guid> termIds = new List<Guid> { key.ID };
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
                mLog.Warn("Get Related Metadata Information Failed. Error:{0}", ex.ToString());
            }
        }

        public AveTermStoreInfo GetTermStoreInfo(IAveTermStore termStore)
        {
            AveTermStoreInfo termStoreInfo = new AveTermStoreInfo();
            termStoreInfo.Name = termStore.Name;
            termStoreInfo.Id = termStore.ID;
            termStoreInfo.DefaultLanguage = termStore.DefaultLanguage;
            termStoreInfo.WorkingLanguage = termStore.WorkingLanguage;

            GetTermStoreAdministrators(termStore, termStoreInfo);
            foreach (IAveTaxonomyGroup group in termStore.Groups)
            {
                try
                {
                    if (!group.IsSiteCollectionGroup && SkipGlobalTermGroup)
                    {
                        continue;
                    }
                    AveMetadataGroupInfo groupInfo = GetMetadataGroupInfo(group);
                    termStoreInfo.Groups.Add(groupInfo);
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while getting MetadataGroupInfo, groupName: {0}, error: {1}.", group.Name, e);
                }
            }
            return termStoreInfo;
        }

        internal void GetRelatedTermStoreInfo(IAveTermStore termStore, Guid sspid, AveTermStoreInfo tsInfo)
        {
            lock (AveMetadataServiceCache.cacheRelatedTermStoreInfos)
            {
                if (AveMetadataServiceCache.cacheRelatedTermStoreInfos.ContainsKey(sspid))
                {
                    tsInfo.Name = AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid].Name;
                    tsInfo.Id = AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid].Id;
                    tsInfo.DefaultLanguage = AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid].DefaultLanguage;
                    tsInfo.WorkingLanguage = AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid].WorkingLanguage;
                    tsInfo.TermStoreAdministrators = AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid].TermStoreAdministrators;
                }
                else
                {
                    AveTermStoreInfo info = new AveTermStoreInfo();
                    info.Name = termStore.Name;
                    info.Id = termStore.ID;
                    info.DefaultLanguage = termStore.DefaultLanguage;
                    info.WorkingLanguage = termStore.WorkingLanguage;
                    GetTermStoreAdministrators(termStore, info);

                    tsInfo.Name = info.Name;
                    tsInfo.Id = info.Id;
                    tsInfo.DefaultLanguage = info.DefaultLanguage;
                    tsInfo.WorkingLanguage = info.WorkingLanguage;
                    tsInfo.TermStoreAdministrators = info.TermStoreAdministrators;
                    AveMetadataServiceCache.cacheRelatedTermStoreInfos[sspid] = info;
                }
            }
        }

        private void GetTermStoreAdministrators(IAveTermStore termStore, AveTermStoreInfo termStoreInfo)
        {
            //此处以后可以考虑将TermStoreAdministrators属性移到Common中，这样整体都可以采用IAveTermStore接口对象，避免强转
            List<Dictionary<string, object>> termStoreadminInfo = ((AveTermStore)termStore).TermStoreAdministrators;
            if (termStoreadminInfo != null)
            {
                foreach (Dictionary<string, object> administrator in termStoreadminInfo)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = administrator["PrincipalName"].ToString();
                    aceInfo.DisplayName = administrator["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)administrator["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)administrator["DenyRightsMask"];
                    termStoreInfo.TermStoreAdministrators.Add(aceInfo);
                }
            }
        }

        private Guid GetGroupIdByChild(IAveTermStore termStore, Guid termSetId)
        {
            lock (AveMetadataServiceCache.cacheRelatedTermSetInfos)
            {
                if (AveMetadataServiceCache.cacheRelatedTermSetInfos.ContainsKey(termSetId))
                {
                    return AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].ParentId;
                }
                else
                {
                    AveTermSetInfo termSetInfo = new AveTermSetInfo();
                    IAveTermSet termSet = termStore.GetTermSet(termSetId);
                    if (termSet == null)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                    }
                    termSetInfo.Name = termSet.Name;
                    termSetInfo.Id = termSet.ID;
                    termSetInfo.Description = termSet.Description;
                    termSetInfo.Contact = termSet.Contact;
                    termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
                    termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
                    termSetInfo.Owner = termSet.Owner;
                    termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
                    termSetInfo.ParentId = termSet.Group.ID;
                    foreach (string stakeholder in termSet.Stakeholders)
                    {
                        termSetInfo.Stakeholders.Add(stakeholder);
                    }
                    AveMetadataServiceCache.cacheRelatedTermSetInfos.Add(termSet.ID, termSetInfo);
                    return termSet.Group.ID;
                }
            }
        }

        private AveMetadataGroupInfo GetMetadataGroupInfo(IAveTaxonomyGroup group)
        {
            AveMetadataGroupInfo groupInfo = new AveMetadataGroupInfo();
            groupInfo.Name = group.Name;
            groupInfo.Id = group.ID;
            groupInfo.Description = group.Description;
            groupInfo.IsSystemGroup = group.IsSystemGroup;
            groupInfo.IsSiteCollectionGroup = group.IsSiteCollectionGroup;
            GetGroupInfo(group, groupInfo);
            foreach (IAveTermSet termSet in group.TermSets)
            {
                try
                {
                    if (group.IsSystemGroup)
                    {
                        if (group.TermStore.OrphanedTermsTermSet != null && termSet.Name.Equals(group.TermStore.OrphanedTermsTermSet.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                    AveTermSetInfo termSetInfo = GetTermSetInfo(termSet);
                    groupInfo.TermSets.Add(termSetInfo);
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while GetTermSetInfo. termSetName:{0}, error:{1}", termSet.Name, e.ToString());
                }
            }
            return groupInfo;
        }

        private void GetRelatedMetadataGroupInfo(IAveTermStore termStore, Guid groupId, AveMetadataGroupInfo groupInfo)
        {
            lock (AveMetadataServiceCache.cacheRelatedGroupInfos)
            {
                if (AveMetadataServiceCache.cacheRelatedGroupInfos.ContainsKey(groupId))
                {
                    groupInfo.Name = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].Name;
                    groupInfo.Id = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].Id;
                    groupInfo.Description = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].Description;
                    groupInfo.IsSystemGroup = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].IsSystemGroup;
                    groupInfo.IsSiteCollectionGroup = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].IsSiteCollectionGroup;
                    groupInfo.GroupManagers = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].GroupManagers;
                    groupInfo.Contributors = AveMetadataServiceCache.cacheRelatedGroupInfos[groupId].Contributors;
                }
                else
                {
                    AveMetadataGroupInfo info = new AveMetadataGroupInfo();
                    //IAveTaxonomyGroup group = termStore.GetGroup(groupId);//Client 不支持Getgroup()
                    IAveTaxonomyGroup group = termStore.Groups[groupId];
                    if (group == null || !group.ID.Equals(groupId))
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindGroupWithId, groupId, termStore.Name);
                    }
                    info.Name = group.Name;
                    info.Id = group.ID;
                    info.Description = group.Description;
                    info.IsSystemGroup = group.IsSystemGroup;
                    info.IsSiteCollectionGroup = group.IsSiteCollectionGroup;
                    GetGroupInfo(group, info);

                    groupInfo.Name = info.Name;
                    groupInfo.Id = info.Id;
                    groupInfo.Description = info.Description;
                    groupInfo.IsSystemGroup = info.IsSystemGroup;
                    groupInfo.IsSiteCollectionGroup = info.IsSiteCollectionGroup;
                    groupInfo.Contributors = info.Contributors;
                    groupInfo.GroupManagers = info.GroupManagers;
                    AveMetadataServiceCache.cacheRelatedGroupInfos[info.Id] = info;
                }
            }
        }

        private void GetGroupInfo(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            List<Dictionary<string, object>> contributors = ((AveTaxonomyGroup)group).Contributors;
            List<Dictionary<string, object>> groupManagers = ((AveTaxonomyGroup)group).GroupManagers;
            if (contributors != null)
            {
                foreach (Dictionary<string, object> contributor in contributors)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = contributor["PrincipalName"].ToString();
                    aceInfo.DisplayName = contributor["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)contributor["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)contributor["DenyRightsMask"];
                    groupInfo.Contributors.Add(aceInfo);
                }
            }
            if (groupManagers != null)
            {
                foreach (Dictionary<string, object> groupManager in groupManagers)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = groupManager["PrincipalName"].ToString();
                    aceInfo.DisplayName = groupManager["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)groupManager["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)groupManager["DenyRightsMask"];
                    groupInfo.GroupManagers.Add(aceInfo);
                }
            }
        }

        private AveTermSetInfo GetTermSetInfo(IAveTermSet termSet)
        {
            AveTermSetInfo termSetInfo = new AveTermSetInfo();
            termSetInfo.Name = termSet.Name;
            termSetInfo.Id = termSet.ID;
            termSetInfo.Description = termSet.Description;
            termSetInfo.Contact = termSet.Contact;
            termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
            termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
            termSetInfo.Owner = termSet.Owner;
            termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
            termSetInfo.CustomProperties = termSet.CustomProperties;
            foreach (string stakeholder in termSet.Stakeholders)
            {
                termSetInfo.Stakeholders.Add(stakeholder);
            }
            foreach (IAveTerm term in termSet.Terms)
            {
                try
                {
                    AveTermInfo termInfo = GetTermInfo(term, termSet.ID);
                    termSetInfo.Terms.Add(termInfo);
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while GetTermInfo. termName:{0}, error:{1}", term.Name, e.ToString());
                }
            }
            return termSetInfo;
        }

        private void GetRelatedTermSetInfo(IAveTermStore termStore, Guid termSetId, AveTermSetInfo tSetInfo)
        {
            lock (AveMetadataServiceCache.cacheRelatedTermSetInfos)
            {
                if (AveMetadataServiceCache.cacheRelatedTermSetInfos.ContainsKey(termSetId))
                {
                    tSetInfo.Name = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Name;
                    tSetInfo.Id = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Id;
                    tSetInfo.Description = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Description;
                    tSetInfo.Contact = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Contact;
                    tSetInfo.IsAvailableForTagging = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].IsAvailableForTagging;
                    tSetInfo.IsOpenForTermCreation = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].IsOpenForTermCreation;
                    tSetInfo.Owner = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Owner;
                    tSetInfo.ParentId = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].ParentId;
                    tSetInfo.CustomSortOrder = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].CustomSortOrder;
                    tSetInfo.CustomProperties = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].CustomProperties;
                    tSetInfo.Stakeholders = AveMetadataServiceCache.cacheRelatedTermSetInfos[termSetId].Stakeholders;
                }
                else
                {
                    AveTermSetInfo termSetInfo = new AveTermSetInfo();
                    IAveTermSet termSet = termStore.GetTermSet(termSetId);
                    if (termSet == null)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                    }
                    termSetInfo.Name = termSet.Name;
                    termSetInfo.Id = termSet.ID;
                    termSetInfo.Description = termSet.Description;
                    termSetInfo.Contact = termSet.Contact;
                    termSetInfo.IsAvailableForTagging = termSet.IsAvailableForTagging;
                    termSetInfo.IsOpenForTermCreation = termSet.IsOpenForTermCreation;
                    termSetInfo.Owner = termSet.Owner;
                    termSetInfo.ParentId = termSet.Group.ID;
                    termSetInfo.CustomSortOrder = termSet.CustomSortOrder;
                    termSetInfo.CustomProperties = termSet.CustomProperties;
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
                    tSetInfo.CustomProperties = termSetInfo.CustomProperties;
                    tSetInfo.Stakeholders = termSetInfo.Stakeholders;
                    AveMetadataServiceCache.cacheRelatedTermSetInfos[termSet.ID] = termSetInfo;
                }
            }
        }

        private AveTermInfo GetTermInfo(IAveTerm term, Guid termSetId)
        {
            AveTermInfo termInfo = GenerateTermInfo(term);
            foreach (IAveTerm term1 in term.Terms)
            {
                AveTermInfo termInfo1 = GetTermInfo(term1, termSetId);
                termInfo1.ParentTermId = term.ID;
                termInfo.Terms.Add(termInfo1);
            }
            return termInfo;
        }

        private void GetRelatedTermInfos(IAveTermStore termStore, List<Guid> termIds, AveTermSetInfo tSetInfo, AveRelatedMetadataServiceCache cache)
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

        /// <summary>
        /// In cache, use Term.Id as key, to reuse or merge or pin term, Id is same as source term
        /// use this method to check whether a reused term is the right one we will backup.
        /// Parent Info changed means cached term is not we want to backup.
        /// </summary>
        /// <param name="termSet"></param>
        /// <param name="termInfo"></param>
        /// <returns>true if parent info changed, otherwise false</returns>
        private bool CheckReuseTermParentInfo(IAveTermSet termSet, AveTermInfo termInfo)
        {
            bool parentChanged = false;
            IAveTerm reusedTerm = termSet.GetTerm(termInfo.Id);
            if (reusedTerm != null)
            {
                if (reusedTerm.Parent != null)
                {
                    if (reusedTerm.Parent.ID != termInfo.ParentTermId)
                    {
                        termInfo.ParentTermId = reusedTerm.Parent.ID;
                        termInfo.ParentTermSetId = Guid.Empty;
                        parentChanged = true;
                    }
                }
                else
                {
                    if (termInfo.ParentTermSetId != reusedTerm.TermSet.ID)
                    {
                        termInfo.ParentTermSetId = reusedTerm.TermSet.ID;
                        termInfo.ParentTermId = Guid.Empty;
                        parentChanged = true;
                    }
                }
                termInfo.IsSourceTerm = reusedTerm.IsSourceTerm;
            }
            return parentChanged;
        }

        private AveTermInfo GenerateTermInfo(IAveTerm term)
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = term.Name;
            termInfo.Id = term.ID;
            termInfo.Description = term.GetAllDescriptions();
            termInfo.Owner = term.Owner;
            termInfo.IsDeprecated = term.IsDeprecated;
            termInfo.IsReused = term.IsReused;
            termInfo.IsSourceTerm = term.IsSourceTerm;
            termInfo.IsAvailableForTagging = term.IsAvailableForTagging;
            termInfo.CustomSortOrder = term.CustomSortOrder;
            termInfo.CustomProperties = term.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.LocalCustomProperties = term.LocalCustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.MergedTermIds = term.MergedTermIds;

            if (term.Parent != null)
            {
                termInfo.ParentTermId = term.Parent.ID;
            }
            if (term.PinSourceTermSetId != Guid.Empty)
            {
                termInfo.PinSourceTermSetId = term.PinSourceTermSetId;
            }
            termInfo.ParentTermSetId = term.TermSet.ID;
            termInfo.IsPinned = term.IsPinned;

            foreach (var label in term.Labels)
            {
                AveLableInfo labelInfo = new AveLableInfo();
                labelInfo.IsDefaultForLanguage = label.IsDefaultForLanguage;
                labelInfo.Language = label.Language;
                labelInfo.Value = label.Value;
                termInfo.Labels.Add(labelInfo);
            }
            return termInfo;
        }

        private AveTermInfo GetTermInfo(IAveTermSet termSet, Guid termId, AveRelatedMetadataServiceCache cache)
        {
            var tInfo = new AveTermInfo();
            if (AveMetadataServiceCache.cacheRelatedTermInfos.ContainsKey(termId))
            {
                lock (AveMetadataServiceCache.cacheRelatedTermInfos)
                {
                    CopyTermInfo(AveMetadataServiceCache.cacheRelatedTermInfos[termId], tInfo);
                }
                if (tInfo.IsReused)
                {
                    //If it is a reused term, need to check parent info
                    CheckReuseTermParentInfo(termSet, tInfo);
                }
            }
            else
            {
                IAveTerm term = termSet.GetTerm(termId);
                if (term == null)
                {
                    throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindTermwithId, termId, termSet.Name);
                }
                AveTermInfo termInfo = GenerateTermInfo(term);
                //Term Only 不支持备份Pinned 属性，因为不能保证source term备份
                termInfo.IsPinned = false;
                CopyTermInfo(termInfo, tInfo);
                lock (AveMetadataServiceCache.cacheRelatedTermInfos)
                {
                    AveMetadataServiceCache.cacheRelatedTermInfos[term.ID] = termInfo;
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
        private AveTermInfo GetRootTermInfo(IAveTermSet termSet, AveTermInfo tInfo, AveRelatedMetadataServiceCache cache)
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
            tInfo.IsSourceTerm = termInfo.IsSourceTerm;
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
        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
