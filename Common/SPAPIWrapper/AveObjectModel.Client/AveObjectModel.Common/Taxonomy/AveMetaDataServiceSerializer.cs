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

namespace AvePoint.ObjectModel.Common
{
    internal class AveMetaDataServiceSerializer : IAveMetaDataServiceSerializer
    {
        private AveSite m_Site;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public bool SkipGlobalTermGroup { get; set; }
        public bool IsTeamsLevelJob { get; set; }
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
                if (!group.IsSiteCollectionGroup)
                {
                    if (SkipGlobalTermGroup)
                    {
                        continue;
                    }

                    if (IsTeamsLevelJob && !(m_Site.RootWeb != null && m_Site.RootWeb.WebTemplate.Equals("GROUP", StringComparison.OrdinalIgnoreCase)))
                    {
                        mLog.Info($"Skipping term store info, group: {group.Name}");
                        continue;
                    }

                    AveMetadataGroupInfo groupInfo = GetMetadataGroupInfo(group);
                    termStoreInfo.Groups.Add(groupInfo);
                }
            }
            IAveTaxonomyGroup siteCollectionGroup = termStore.GetSCLevelTermGroup(m_Site);
            if (siteCollectionGroup != null)
            {
                AveMetadataGroupInfo groupInfo1 = GetMetadataGroupInfo(siteCollectionGroup);
                termStoreInfo.Groups.Add(groupInfo1);
            }
            return termStoreInfo;
        }

        private void GetTermStoreAdministrators(IAveTermStore termStore, AveTermStoreInfo termStoreInfo)
        {
            //此处以后可以考虑将TermStoreAdministrators属性移到Common中，这样整体都可以采用IAveTermStore接口对象，避免强转
            if (((AveTermStore)termStore).TermStoreAdministrators != null)
            {
                foreach (Dictionary<string, object> administrator in ((AveTermStore)termStore).TermStoreAdministrators)
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

        private void GetGroupInfo(IAveTaxonomyGroup group, AveMetadataGroupInfo groupInfo)
        {
            if (((AveTaxonomyGroup)group).Contributors != null)
            {
                foreach (Dictionary<string, object> contributor in ((AveTaxonomyGroup)group).Contributors)
                {
                    AveAceInfo aceInfo = new AveAceInfo();
                    aceInfo.PrincipalName = contributor["PrincipalName"].ToString();
                    aceInfo.DisplayName = contributor["DisplayName"].ToString();
                    aceInfo.GrantRightsMask = (ulong)contributor["GrantRightsMask"];
                    aceInfo.DenyRightsMask = (ulong)contributor["DenyRightsMask"];
                    groupInfo.Contributors.Add(aceInfo);
                }
            }
            if (((AveTaxonomyGroup)group).GroupManagers != null)
            {
                foreach (Dictionary<string, object> groupManager in ((AveTaxonomyGroup)group).GroupManagers)
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
            ICollection<DelegateTask> getTermTaskes = new List<DelegateTask>(termSet.Terms.Count);
            object termLock = new object();
            foreach (IAveTerm term in termSet.Terms)
            {
                try
                {
                    IAveTerm tempTerm = term;
                    getTermTaskes.Add(() =>
                    {
                        AveTermInfo termInfo = GetTermInfo(tempTerm);
                        lock (termLock)
                        {
                            termSetInfo.Terms.Add(termInfo);
                        }
                    });
                }
                catch (Exception e)
                {
                    mLog.Warn("An error occurred while GetTermInfo. termName:{0}, error:{1}", term.Name, e.ToString());
                }
            }
            try
            {
                var taskExecutor = new CountableTaskExecutor(10);
                taskExecutor.Execute(getTermTaskes,false);
            }
            catch (Exception e)
            {
                mLog.Error("failed to get term due to: {0}", e.ToString());
            }
            return termSetInfo;
        }

        private AveTermInfo GetTermInfo(IAveTerm term)
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = term.Name;
            termInfo.Id = term.ID;
            termInfo.Description = term.GetDescription();
            termInfo.Owner = term.Owner;
            termInfo.IsAvailableForTagging = term.IsAvailableForTagging;
            termInfo.IsPinned = term.IsPinned;
            termInfo.IsDeprecated = term.IsDeprecated;
            termInfo.IsReused = term.IsReused;
            termInfo.IsSourceTerm = term.IsSourceTerm;
            termInfo.CustomSortOrder = term.CustomSortOrder;
            termInfo.CustomProperties = term.CustomProperties;
            termInfo.LocalCustomProperties = term.LocalCustomProperties;
            termInfo.PinSourceTermSetId = term.PinSourceTermSet;
            termInfo.ParentTermId = term.ParentTermId;
            termInfo.ParentTermSetId = term.TermSet.ID;

            foreach (IAveLabel label in term.Labels)
            {
                AveLableInfo labelInfo = new AveLableInfo();
                labelInfo.IsDefaultForLanguage = label.IsDefaultForLanguage;
                labelInfo.Language = label.Language;
                labelInfo.Value = label.Value;
                termInfo.Labels.Add(labelInfo);
            }
            foreach (IAveTerm term1 in term.Terms)
            {
                AveTermInfo termInfo1 = GetTermInfo(term1);
                termInfo.Terms.Add(termInfo1);
            }
            return termInfo;
        }

        public object SetObjectData(object obj)
        {
            return null;
        }

        public List<AveTermStoreInfo> GetRelatedMetadataInfo(IAveSite site, List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupOption)
        {
            AveRelatedMetadataServiceCache relatedCache = new AveRelatedMetadataServiceCache();
            IAveTaxonomySession taxonomySession = new AveTaxonomySession(m_Site);
            foreach (AveTaxFieldInfo taxFieldInfo in taxFieldInfos)
            {
                RealGetMetadataInfo(taxonomySession, taxFieldInfo, relatedCache, backupOption);
            }
            if (backupOption.BackupRelatedTermSets)
            {
                AddPinSourceTermSetInfos(taxonomySession, relatedCache);
            }
            return relatedCache.GetResult();
        }

        private void AddPinSourceTermSetInfos(IAveTaxonomySession taxonomySession, AveRelatedMetadataServiceCache relatedCache)
        {
            IAveTermStore termStore = null;
            foreach (AveTermStoreInfo termStoreInfo in relatedCache.termStoreInfoCache.Values)
            {
                try
                {
                    termStore = taxonomySession.TermStores[termStoreInfo.Id];
                }
                catch (Exception e)
                {
                    mLog.Debug("An error occurred while getting the TermStore. TermStore ID:{0}, Error:{1}", termStoreInfo.Id, e.ToString());
                    //获取不到，使用DefaultSiteCollectionTermStore
                    termStore = taxonomySession.DefaultSiteCollectionTermStore;
                    if (termStore == null)
                    {
                        termStore = taxonomySession.DefaultKeywordsTermStore;
                    }
                    if (termStore == null)
                    {
                        termStore = taxonomySession.TermStores[0];
                    }
                }
                RealAddPinSourceTermSetInfos(termStore, relatedCache, termStoreInfo);
            }
        }
        private void RealAddPinSourceTermSetInfos(IAveTermStore termStore, AveRelatedMetadataServiceCache cache, AveTermStoreInfo usedTermStoreInfo)
        {
            List<Guid> sourceTermSetIds = GetPinSourceTermSetIds(cache);
            foreach (Guid sourceTermSetId in sourceTermSetIds)
            {
                try
                {
                    if (cache.termSetInfoCache.ContainsKey(sourceTermSetId))
                    {
                        continue;
                    }
                    Guid GroupId = GetGroupIdByChild(termStore, sourceTermSetId);
                    AveMetadataGroupInfo groupInfo;
                    if (!cache.groupInfoCache.TryGetValue(GroupId,out groupInfo))
                    {
                        groupInfo = new AveMetadataGroupInfo();
                        GetRelatedMetadataGroupInfo(termStore, GroupId, groupInfo);
                        cache.groupInfoCache.Add(groupInfo.Id, groupInfo);
                        usedTermStoreInfo.Groups.Add(groupInfo);
                    }
                    AveTermSetInfo sourceTermSetInfo = GetTermSetInfo(termStore.GetTermSet(sourceTermSetId));
                    groupInfo.TermSets.Add(sourceTermSetInfo);
                    cache.termSetInfoCache.Add(sourceTermSetId, sourceTermSetInfo);
                }
                catch (Exception e)
                {
                    mLog.Warn("Get Pin Source Term Set Failed.Term Set ID : {0}. due : {1}", sourceTermSetId, e.Message);
                }
            }
        }

        private List<Guid> GetPinSourceTermSetIds(AveRelatedMetadataServiceCache cache)
        {
            List<Guid> sourceTermSetIds = new List<Guid>();
            foreach (AveTermSetInfo termSetInfo in cache.termSetInfoCache.Values)
            {
                foreach (AveTermInfo termInfo in termSetInfo.Terms)
                {
                    RealGetTermSetIds(termInfo, sourceTermSetIds);
                }
            }
            return sourceTermSetIds;
        }

        private void RealGetTermSetIds(AveTermInfo termInfo, List<Guid> sourceTermSetIds)
        {
            if (termInfo.IsPinned)
            {
                if (!sourceTermSetIds.Contains(termInfo.PinSourceTermSetId))
                {
                    sourceTermSetIds.Add(termInfo.PinSourceTermSetId);
                }
            }
            else
            {
                foreach (AveTermInfo term in termInfo.Terms)
                {
                    RealGetTermSetIds(term, sourceTermSetIds);
                }
            }
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
                            if (!cache.termStoreInfoCache.TryGetValue(fieldInfo.SspId, out usedTermStoreInfo))
                            {
                                usedTermStoreInfo = new AveTermStoreInfo();
                                GetRelatedTermStoreInfo(termStore, fieldInfo.SspId, usedTermStoreInfo);
                                cache.termStoreInfoCache.Add(usedTermStoreInfo.Id, usedTermStoreInfo);
                            }
                            AveTermSetInfo termSetInfo;
                            bool isAddTermSet = false;
                            if (!cache.termSetInfoCache.TryGetValue(realTermSetId, out termSetInfo))
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
                            if (!cache.groupInfoCache.TryGetValue(realGroupId, out groupInfo))
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
                    if (!cache.termStoreInfoCache.TryGetValue(fieldInfo.SspId, out usedTermStoreInfo))
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
                    if (!cache.groupInfoCache.TryGetValue(fieldInfo.GroupId, out groupInfo))
                    {
                        groupInfo = new AveMetadataGroupInfo();
                        GetRelatedMetadataGroupInfo(termStore, fieldInfo.GroupId, groupInfo);
                        cache.groupInfoCache.Add(groupInfo.Id, groupInfo);
                        usedTermStoreInfo.Groups.Add(groupInfo);
                    }
                    AveTermSetInfo termSetInfo;
                    if (!cache.termSetInfoCache.TryGetValue(fieldInfo.TermSetId, out termSetInfo))
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
                        //throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                        throw new AveWrapperException(string.Format("Not Find Term Set with Id.TermSet Id:{0},TermStore:{1}", termSetId, termStore));//zdtian
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
                        //throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindGroupWithId, groupId, termStore.Name);
                        throw new AveWrapperException(string.Format("Not Find Term Group with Id.TermSet Id:{0},TermStore:{1}", groupId, termStore));//zdtian
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
                        //throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_NotFindTermSetwithId, termSetId, termStore.Name);
                        throw new AveWrapperException(string.Format("Not Find Term Group with Id.TermSet Id:{0},TermStore:{1}", termSetId, termStore));//zdtian
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

        private void GetRelatedTermInfos(IAveTermStore termStore, List<Guid> termIds, AveTermSetInfo tSetInfo, AveRelatedMetadataServiceCache cache)
        {
            foreach (Guid termId in termIds)
            {
                if (termId != Guid.Empty)
                {
                    //因reuse term Id是相同的，所以catch对reuse term无效,要对reuse term特殊判断
                    if (!cache.termInfoCache.ContainsKey(termId) || cache.termInfoCache[termId].IsReused)
                    {
                        AveTermInfo ctermInfo = new AveTermInfo();
                        bool hasFound = false;
                        AveTermInfo rtermInfo = GetRelatedTermInfo(termStore.GetTermSet(tSetInfo.Id), termId, ctermInfo, cache, out hasFound);
                        cache.AddToTermInfoCache(rtermInfo);
                        if (!hasFound)
                        {
                            tSetInfo.Terms.Add(rtermInfo);
                        }
                    }
                }
            }
        }

        private AveTermInfo GetRelatedTermInfo(IAveTermSet termSet, Guid termId, AveTermInfo tInfo, AveRelatedMetadataServiceCache cache, out bool hasFound)
        {
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
                    //throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindTermwithId, termId, termSet.Name);
                    throw new AveWrapperException(string.Format("Not Find Term with Id.TermSet Id:{0},TermStore:{1}", termId, termSet.Name));//zdtian
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
            hasFound = false;
            while (tInfo.ParentTermId != Guid.Empty)
            {
                AveTermInfo ptermInfo = new AveTermInfo();
                if (cache.termInfoCache.ContainsKey(tInfo.ParentTermId))
                {
                    ptermInfo = cache.termInfoCache[tInfo.ParentTermId];
                    ptermInfo.Terms.Add(tInfo);
                    hasFound = true;
                    if (ptermInfo.IsReused && CheckReuseTermParentInfo(termSet, ptermInfo))
                    {
                        tInfo = ptermInfo;
                        hasFound = false;
                    }
                }
                else
                {
                    GetRelatedTermInfo(termSet, tInfo.ParentTermId, ptermInfo, cache, out hasFound);
                    ptermInfo.Terms.Add(tInfo);
                    tInfo = ptermInfo;
                }
                if (hasFound)
                {
                    return tInfo;
                }
            }
            return tInfo;
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

        private bool CheckReuseTermParentInfo(IAveTermSet termSet, AveTermInfo termInfo)
        {
            //zdtian
            bool parentChanged = false;
            IAveTerm reusedTerm = termSet.GetTerm(termInfo.Id);
            if (reusedTerm != null)
            {
                //if (reusedTerm.Parent != null)
                //{
                //    if (reusedTerm.Parent.ID != termInfo.ParentTermId)
                //    {
                //        termInfo.ParentTermId = reusedTerm.Parent.ID;
                //        termInfo.ParentTermSetId = Guid.Empty;
                //        parentChanged = true;
                //    }
                //}
                //else
                //{
                if (termInfo.ParentTermSetId != reusedTerm.TermSet.ID)
                {
                    termInfo.ParentTermSetId = reusedTerm.TermSet.ID;
                    termInfo.ParentTermId = Guid.Empty;
                    parentChanged = true;
                }
                //}
                termInfo.IsSourceTerm = reusedTerm.IsSourceTerm;
            }
            return parentChanged;
        }

        private AveTermInfo GenerateTermInfo(IAveTerm term)
        {
            AveTermInfo termInfo = new AveTermInfo();
            termInfo.Name = term.Name;
            termInfo.Id = term.ID;
            termInfo.Description = term.GetDescription();
            termInfo.Owner = term.Owner;
            termInfo.IsDeprecated = term.IsDeprecated;
            termInfo.IsReused = term.IsReused;
            termInfo.IsSourceTerm = term.IsSourceTerm;
            termInfo.IsAvailableForTagging = term.IsAvailableForTagging;
            termInfo.CustomSortOrder = term.CustomSortOrder;
            termInfo.CustomProperties = term.CustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            termInfo.LocalCustomProperties = term.LocalCustomProperties.ToDictionary(pair => pair.Key, paire => paire.Value);
            //termInfo.MergedTermIds = term.MergedTermIds;

            if (term.ParentTermId != null)
            {
                termInfo.ParentTermId = term.ParentTermId;
            }
            //if (term.PinSourceTermSet != Guid.Empty)
            //{
            //    termInfo.PinSourceTermSetId = term.PinSourceTermSet;
            //}
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
    }

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
}
