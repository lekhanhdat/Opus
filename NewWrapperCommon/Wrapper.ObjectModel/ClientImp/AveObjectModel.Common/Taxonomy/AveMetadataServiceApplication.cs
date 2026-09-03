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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using AvePoint.GCommon;
using System.Reflection;

namespace AvePoint.ObjectModel.Common
{
    /// <summary>
    /// 支持真实365. 对于Local模拟, 只支持获取Default term store.
    /// </summary>
    class AveMetadataServiceApplication : IAveMetadataServiceApplication
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        AveSite site;
        AveTermStore termStore;
        public AveMetadataServiceApplication(IAveSite site)
        {
            this.site = site as AveSite;
            var session = new AveTaxonomySession(site);
            if (session.TermStores.Count < 0)
            {
                throw new Exception(string.Format("This site do not have any association term store. Site: {0}", site.Url));
            }
            termStore = session.DefaultSiteCollectionTermStore as AveTermStore;
        }
        #region NotImplemented
        public List<Guid> GetSiteCollectionId(Guid groupId)
        {
            throw new NotImplementedException();
        }
        public AveTermInfo GetTerm(Guid termSetId, int termId)
        {
            throw new NotImplementedException();
        }

        public string GetTermDefaultLabel(int termId)
        {
            throw new NotImplementedException();
        }

        public AveTermSetInfo GetTermSet(int setId)
        {
            throw new NotImplementedException();
        }
        public AveMetadataGroupInfo GetGroup(int groupId)
        {
            throw new NotImplementedException();
        }
        public Uri GetContentTypeSyndicationHubLocal()
        {
            throw new NotImplementedException();
        }
        public bool IsConnected(IAveServiceApplicationProxy proxy)
        {
            throw new NotImplementedException();
        }
        public bool IsPublished(string contentTypeId)
        {
            throw new NotImplementedException();
        }
        public bool IsUnPublished(string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public void Provision()
        {
            throw new NotImplementedException();
        }

        public void Uncache()
        {
            throw new NotImplementedException();
        }

        public void Unprovision()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Update(bool ensure)
        {
            throw new NotImplementedException();
        }
        public XmlDocument GetStateXml()
        {
            throw new NotImplementedException();
        }
        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public Hashtable Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public IAveServiceApplicationProxyGroup ServiceApplicationProxyGroup
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public AveObjectStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string TypeName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long Version
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<Guid, Version> Versions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool WasCreated
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool CheckServiceApplicationPermission(object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            return;
        }

        public Guid ApplicationClassId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveIisWebServiceApplicationPool ApplicationPool
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAveDatabase Database
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public IAveFarm Farm
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Guid ID
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public AveTriState IsBackwardsCompatible
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IAveLastUpdateInfo LastUpdateInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public bool NeedsUpgrade
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool NeedsUpgradeIncludeChildren
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IAvePersistedObject Parent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Guid PartitionId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion
        #region NotImplemented IB Method.
        public List<AveTermChangeItem> GetAllChanges(DateTime? sinceTime)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetAllChanges(DateTime? sinceTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, DateTime? sinceTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime, DateTime? toTime, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region NotImplemented Partition Method
        public bool IsUnPublished(string contentTypeId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public bool IsPublished(string contentTypeId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public bool IsSiteCollectionGroup(Guid groupId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public AveTermStoreInfo GetTermStore(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public bool IsMetadataPartition(Guid ApplicationId)
        {
            throw new NotImplementedException();
        }
        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public List<AveTermSetInfo> GetTermSets(Guid groupId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public AveTermSetInfo GetTermSet(int setId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public AveTermSetInfo GetTermSet(Guid setId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public List<ServiceSetting> GetPartitionServiceSettings()
        {
            throw new NotImplementedException();
        }
        public List<Guid> GetSiteCollectionId(Guid groupId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public List<AveSiteMapVisible> GetTenancyAdminSiteId(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public AveTermInfo GetTerm(Guid termSetId, Guid termId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public AveTermInfo GetTerm(Guid termSetId, int termId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public string GetTermDefaultLabel(int termId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public Uri GetContentTypeSyndicationHubLocal(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public List<AveMetadataGroupInfo> GetGlobalGroups(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public AveMetadataGroupInfo GetGroup(string groupName, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public AveMetadataGroupInfo GetGroup(int groupId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }

        public AveMetadataGroupInfo GetGroup(Guid groupId, Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public void GetLanguage(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        public List<AveMetadataGroupInfo> GetLocalGroups(Guid defaultPartitionId)
        {
            throw new NotImplementedException();
        }
        #endregion
        public int DefaultLanguage { get; set; }

        public void GetLanguage()
        {
            DefaultLanguage = termStore.DefaultLanguage;
        }
        public List<AveMetadataGroupInfo> GetGlobalGroups()
        {
            return termStore.Groups.Where(g => !g.IsSiteCollectionGroup).Select(g => g.TaxonomyGroupSerializer.GetObjectData()).ToList();
        }

        public AveMetadataGroupInfo GetGroup(string groupName)
        {
            return termStore.Groups[groupName].TaxonomyGroupSerializer.GetObjectData();
        }

        public AveMetadataGroupInfo GetGroup(Guid groupId)
        {
            return termStore.Groups[groupId].TaxonomyGroupSerializer.GetObjectData();
        }
        /// <summary>
        /// 如果是Admin Site,获取所有Local Groups. 普通站点只获取Site collection group.
        /// </summary>
        /// <returns></returns>
        public List<AveMetadataGroupInfo> GetLocalGroups()
        {
            var result = new List<AveMetadataGroupInfo>();
            if (!this.site.IsAdminCenter)
            {
                var group = this.termStore.GetSiteCollectionGroup(this.site, false);
                if (group != null)
                {
                    result.Add(group.TaxonomyGroupSerializer.GetObjectData());
                }
            }
            else
            {
                result = termStore.Groups.Where(g => g.IsSiteCollectionGroup).Select(g => g.TaxonomyGroupSerializer.GetObjectData()).ToList();
            }
            return result;
        }
        public AveTermInfo GetTerm(Guid termSetId, Guid termId)
        {
            var term = termStore.GetTerm(termSetId, termId);
            if (term != null)
            {
                return term.TermSerializer.GetObjectData();
            }
            log.Warn("Can not find this term. termSetId: {0}, termId: {1}.", termSetId, termId);
            return null;
        }

        public AveTermSetInfo GetTermSet(Guid setId)
        {
            var termSet = termStore.GetTermSet(setId);
            if (termSet != null)
            {
                return termSet.TermSetSerializer.GetObjectData();
            }
            log.Warn("Can not find this term set. termSetId: {0}.", setId);
            return null;
        }

        public List<AveTermSetInfo> GetTermSets(Guid groupId)
        {
            List<AveTermSetInfo> termSetInfos = new List<AveTermSetInfo>();
            var group = termStore.Groups.FirstOrDefault(g => g.ID == groupId);
            if (group != null)
            {
                termSetInfos = group.TermSets.Select(set => set.TermSetSerializer.GetObjectData()).ToList();
            }
            return termSetInfos;
        }
    

        /// <summary>
        /// 和Local保持一致,只获取Term下的第一层Terms.
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public List<AveTermInfo> GetTermsInTerm(Guid termSetId, Guid termId)
        {
            List<AveTermInfo> termInfos = new List<AveTermInfo>();
            var term = termStore.GetTerm(termSetId, termId);
            if (term != null)
            {
                termInfos = term.Terms.Select(sub => sub.TermSerializer.GetObjectData()).ToList();
            }
            else
            {
                log.Warn("Can not find this term. termSetId: {0}, termId: {1}.", termSetId, termId);
            }
            return termInfos;
        }
        /// <summary>
        /// 和local实现保持一致,只获取TermSet下的第一层Terms。
        /// </summary>
        /// <param name="termSetId"></param>
        /// <returns></returns>
        public List<AveTermInfo> GetTermsInTermSet(Guid termSetId)
        {
            List<AveTermInfo> termInfos = new List<AveTermInfo>();
            var termSet = termStore.GetTermSet(termSetId);
            if (termSet != null)
            {
                termInfos = termSet.Terms.Select(sub => sub.TermSerializer.GetObjectData()).ToList();
            }
            else
            {
                log.Warn("Can not find this term set. termSetId: {0}.", termSetId);
            }
            return termInfos;
        }
        public AveTermStoreInfo GetTermStore()
        {
            return termStore.TermStoreSerializer.GetObjectData();
        }
        public bool IsSiteCollectionGroup(Guid groupId)
        {
            return termStore.Groups[groupId].IsSiteCollectionGroup;
        }

        public List<AveTermChangeItem> GetChangesInStore(DateTime? sinceTime, bool isGlobal)
        {
            if(sinceTime.HasValue)
            {
                throw new Exception("Do not support incremental in client mode.");
            }
            
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            foreach(var group in termStore.Groups.Where(g=>g.IsSiteCollectionGroup != isGlobal))
            {
                AveTermChangeItem item = new AveTermChangeItem();
                item.Id = group.ID;
                item.GroupId = group.ID;
                item.ItemType = (AveTermChangeItem.ChangedItemType)3;
                item.ChangeType = 0;
                item.Name = group.Name;
                items.Add(item);
            }
            return items;
        }
        public List<AveTermChangeItem> GetChangesInGroup(Guid groupId, DateTime? sinceTime)
        {
            if (sinceTime.HasValue)
            {
                throw new Exception("Do not support incremental in client mode.");
            }
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            foreach(var termSet in termStore.Groups[groupId].TermSets)
            {
                AveTermChangeItem item = new AveTermChangeItem();
                item.TermSetId = termSet.ID;
                item.ItemType = (AveTermChangeItem.ChangedItemType)2;
                item.ChangeType = 0;
                item.Id = termSet.ID;
                item.Name = termSet.Name;
                item.GroupId = groupId;
                item.TermSetType = GetTermSetType(termSet);
                items.Add(item);
            }
            return items;
        }
        public List<AveTermChangeItem> GetChangesInTermSet(Guid termSetId, DateTime? sinceTime)
        {
            if (sinceTime.HasValue)
            {
                throw new Exception("Do not support incremental in client mode.");
            }
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            var termSet = this.termStore.GetTermSet(termSetId);
            foreach(var term in termSet.Terms)
            {
                AveTermChangeItem item = new AveTermChangeItem();

                item.TermSetId = termSetId;
                item.ItemType = (AveTermChangeItem.ChangedItemType)1;
                item.ChangeType = 0;
                item.Id = term.ID;
                item.Name = term.Name;
                item.IsPinned = term.IsPinned;
                item.IsReused = term.IsReused;
                item.IsRoot = term.IsRoot;
                item.IsSourceTerm = term.IsSourceTerm;
                item.PinSourceTermSetId = term.PinSourceTermSetId;
                item.Path = term.PathOfTerm;
                items.Add(item);
            }
            return items;
        }
        public List<AveTermChangeItem> GetChangesInTerm(Guid termSetId, Guid termId, DateTime? sinceTime)
        {
            if (sinceTime.HasValue)
            {
                throw new Exception("Do not support incremental in client mode.");
            }
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            var parentTerm = this.termStore.GetTermSet(termSetId).GetTerm(termId);
            foreach (var term in parentTerm.Terms)
            {
                AveTermChangeItem item = new AveTermChangeItem();

                item.TermSetId = termSetId;
                item.ItemType = (AveTermChangeItem.ChangedItemType)1;
                item.ChangeType = 0;
                item.Id = term.ID;
                item.Name = term.Name;
                item.IsPinned = term.IsPinned;
                item.IsReused = term.IsReused;
                item.IsRoot = term.IsRoot;
                item.IsSourceTerm = term.IsSourceTerm;
                item.PinSourceTermSetId = term.PinSourceTermSetId;
                item.Path = term.PathOfTerm;
                items.Add(item);
            }
            return items;
        }
        private byte GetTermSetType(IAveTermSet termSet)
        {
            byte type;
            if (termSet.TermStore.KeywordsTermSet != null && termSet.TermStore.KeywordsTermSet.ID == termSet.ID)
            {
                type = (byte)1;
            }
            else if (termSet.TermStore.OrphanedTermsTermSet != null && termSet.TermStore.OrphanedTermsTermSet.ID == termSet.ID)
            {
                type = (byte)2;
            }
            else
            {
                type = (byte)0;
            }
            return type;
        }
        private void RetrieveAllTermInTermSet(IAveTermCollection terms, bool hasParent, List<AveTermChangeItem> items)
        {
            foreach (var term in terms)
            {
                if (hasParent && term.IsSourceTerm && !term.IsRoot)
                {
                    AveTermChangeItem item = new AveTermChangeItem();
                    item.Id = term.ID;
                    item.Name = term.Name;
                    item.IsPinned = term.IsPinned;
                    item.IsReused = term.IsReused;
                    item.IsRoot = term.IsRoot;
                    item.IsSourceTerm = term.IsSourceTerm;
                    item.PinSourceTermSetId = term.PinSourceTermSetId;
                    item.ParentTermId = term.Parent.ID;
                    item.TermSetId = term.TermSet.ID;
                    item.ItemType = AveTermChangeItem.ChangedItemType.Term;
                    item.ChangeType = 0;
                    item.Path = term.PathOfTerm;
                    item.GroupId = term.TermSet.Group.ID;
                    item.IsGlobalGroup = !term.TermSet.Group.IsSiteCollectionGroup;
                    items.Add(item);
                }
                RetrieveAllTermInTermSet(term.Terms, true, items);
            }
        }
        public AveTermChangeItem GetTermSetParent(Guid termSetId, Guid partitionId)
        {
            AveTermChangeItem item = new AveTermChangeItem();
            var termGroup = termStore.GetTermSet(termSetId).Group;
            item.Id = termGroup.ID;
            item.GroupId = termGroup.ID;
            item.ChangeType = 0;
            item.ItemType = AveTermChangeItem.ChangedItemType.Group;
            item.Name = termGroup.Name;
            item.IsGlobalGroup = !termGroup.IsSiteCollectionGroup;
            return item;
        }
        /// <summary>
        /// 和Local保持一致,获取TermSet下所有isSource为tree并且Parent==null的Term
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="partitionId"></param>
        /// <returns></returns>
        public List<AveTermChangeItem> GetTermSetChildren(Guid termSetId, Guid partitionId)
        {
            List<AveTermChangeItem> items = new List<AveTermChangeItem>();
            var termSet = termStore.GetTermSet(termSetId);
            RetrieveAllTermInTermSet(termSet.Terms, false, items);
            return items;
        }
        /// <summary>
        /// 和Local保持一致.  如果是Source获取ParentTerm或ParentTermSet信息。如果不是Source,获取SourceTerm信息。
        /// </summary>
        /// <param name="termSetId"></param>
        /// <param name="termId"></param>
        /// <param name="parentTermId"></param>
        /// <param name="partitionId"></param>
        /// <param name="isRoot"></param>
        /// <param name="isSourceTerm"></param>
        /// <returns></returns>
        public AveTermChangeItem GetTermParent(Guid termSetId, Guid termId, Guid parentTermId, Guid partitionId, bool isRoot, bool isSourceTerm)
        {
            AveTermChangeItem item = new AveTermChangeItem();
            if (isSourceTerm)
            {
                if(isRoot)
                {
                    var termSet = termStore.GetTermSet(termSetId);
                    item.TermSetId = termSet.ID;
                    item.ItemType = AveTermChangeItem.ChangedItemType.TermSet;
                    item.ChangeType = 0;
                    item.Id = termSet.ID;
                    item.Name = termSet.Name;
                    item.GroupId = termSet.Group.ID;
                    item.TermSetType = GetTermSetType(termSet);
                }
                else
                {
                    var parentTerm = termStore.GetTerm(termSetId, parentTermId);
                    item.ItemType = AveTermChangeItem.ChangedItemType.Term;
                    item.TermSetId = termSetId;
                    item.Id = parentTerm.ID;
                    item.ChangeType = 0;
                    item.Name = parentTerm.Name;
                    item.IsPinned = parentTerm.IsPinned;
                    item.IsReused = parentTerm.IsReused;
                    item.IsRoot = parentTerm.IsRoot;
                    item.IsSourceTerm = parentTerm.IsSourceTerm;
                    item.PinSourceTermSetId = parentTerm.PinSourceTermSetId;
                    if (!parentTerm.IsRoot)
                    {
                        item.ParentTermId = parentTerm.Parent.ID;
                    }
                    item.Path = parentTerm.PathOfTerm;
                    item.GroupId = parentTerm.TermSet.Group.ID;
                    item.IsGlobalGroup = !parentTerm.TermSet.Group.IsSiteCollectionGroup;
                }
            }
            else
            {
                var term = termStore.GetTerm(termSetId, termId);
                var sourceTerm = termStore.GetTerm(term.PinSourceTermSetId, termId);
                item.ItemType = AveTermChangeItem.ChangedItemType.Term;
                item.TermSetId = term.PinSourceTermSetId;
                item.Id = sourceTerm.ID;
                item.ChangeType = 0;
                item.Name = sourceTerm.Name;
                item.IsPinned = sourceTerm.IsPinned;
                item.IsReused = sourceTerm.IsReused;
                item.IsRoot = sourceTerm.IsRoot;
                item.IsSourceTerm = sourceTerm.IsSourceTerm;
                item.PinSourceTermSetId = sourceTerm.PinSourceTermSetId;
                if (!sourceTerm.IsRoot)
                {
                    item.ParentTermId = sourceTerm.Parent.ID;
                }
                item.Path = sourceTerm.PathOfTerm;
                item.GroupId = sourceTerm.TermSet.Group.ID;
                item.IsGlobalGroup = !sourceTerm.TermSet.Group.IsSiteCollectionGroup;
            }
            return item;
        }
    }
}
