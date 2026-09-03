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
using System.Collections.ObjectModel;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveTermStore : AveClientObject, IAveTermStore
    {
        private AveTaxonomySession m_AveTaxonomySession;
        private IAveRequest m_Request;
        private AveTermStoreCollection m_AveTermStoreCollection;
        static private AveLogger mLogger = AveLogger.GetInstance(typeof(AveTermStore));

        public AveTermStore(AveTaxonomySession m_AveTaxonomySession, IAveRequest m_Request, AveTermStoreCollection aveTermStoreCollection, Dictionary<string, object> termStoreDic)
        {
            this.m_AveTaxonomySession = m_AveTaxonomySession;
            this.m_Request = m_Request;
            this.m_AveTermStoreCollection = aveTermStoreCollection;
            base.DataCache.AddPropertyies(termStoreDic);
        }

        #region IAveTermStore Members

        public IAveTermSet KeywordsTermSet
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("KeywordsTermSet") && base.DataCache.IsPropertyAvailable("KeywordsTermSet" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> termSetProp = base.DataCache.PropertiesCache["KeywordsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    AveTermSet termSet = new AveTermSet(m_Request, this, termSetProp);
                    base.DataCache.PropertiesCache["KeywordsTermSet"] = termSet;
                    return termSet;
                }
                return base.DataCache.GetProperty<IAveTermSet>("KeywordsTermSet");
            }
        }

        public IAveTermSet OrphanedTermsTermSet
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("OrphanedTermsTermSet") && base.DataCache.IsPropertyAvailable("OrphanedTermsTermSet" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> termSetProp = base.DataCache.PropertiesCache["OrphanedTermsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    AveTermSet termSet = new AveTermSet(m_Request, this, termSetProp);
                    base.DataCache.PropertiesCache["OrphanedTermsTermSet"] = termSet;
                    return termSet;
                }
                return base.DataCache.GetProperty<IAveTermSet>("OrphanedTermsTermSet");
            }
        }

        public IAveTermSet HashTagsTermSet
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("HashtagsTermSet") && base.DataCache.IsPropertyAvailable("HashtagsTermSet" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> termSetProp = base.DataCache.PropertiesCache["HashtagsTermSet" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    AveTermSet termSet = new AveTermSet(m_Request, this, termSetProp);
                    base.DataCache.PropertiesCache["HashtagsTermSet"] = termSet;
                    return termSet;
                }
                return base.DataCache.GetProperty<IAveTermSet>("HashtagsTermSet");
            }
        }

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }

        public IAveTaxonomyGroupCollection Groups
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Groups"))
                {
                    Dictionary<string, object> TaxonomyGroupsProperties = m_Request.GetTaxonomyGroups(this.ID);
                    AveTaxonomyGroupCollection taxonomyGroupCollection = new AveTaxonomyGroupCollection(m_Request, this, TaxonomyGroupsProperties);
                    base.DataCache.PropertiesCache["Groups"] = taxonomyGroupCollection;
                }
                return base.DataCache.GetProperty<IAveTaxonomyGroupCollection>("Groups");
            }
        }

        public int DefaultLanguage
        {
            get
            {
                return base.DataCache.GetProperty<int>("DefaultLanguage");
            }
        }

        public int WorkingLanguage
        {
            get
            {
                return base.DataCache.GetProperty<int>("WorkingLanguage");
            }
        }

        public IAveTaxonomyGroup SystemGroup
        {
            get
            {
                foreach (AveTaxonomyGroup group in this.Groups)
                {
                    if (group.IsSystemGroup)
                    {
                        return group;
                    }
                }
                return null;
            }
        }

        public Uri ContentTypePublishingHub
        {
            get { throw new NotImplementedException(); }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public IAveTermStoreSerializer TermStoreSerializer
        {
            get { return new AveTermStoreSerializer(this); }
        }

        public Collection<int> Languages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Languages"))
                {
                    Collection<int> languages = null;
                    if (base.DataCache.IsPropertyAvailable("Languages" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        List<int> fakeLanguages = base.DataCache.GetProperty<List<int>>("Languages" + AveObjectModelConstant.ObjectPropertySuffix);
                        languages = new Collection<int>(fakeLanguages);
                    }
                    else
                    {
                        languages = new Collection<int>();
                    }
                    base.DataCache.PropertiesCache["Languages"] = languages;
                }
                return base.DataCache.GetProperty<Collection<int>>("Languages");
            }
        }

        public IAveTermSet GetTermSet(Guid termSetId)
        {
            Dictionary<string, object> termSetProperties = m_Request.GetTermSet(this.ID, termSetId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> setProperties = null;
                string groupName = null;
                string termSetName = null;
                IAveTermSet termSet = null;
                if (groupProperties.ContainsKey("TermSet"))
                {
                    setProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                    termSetName = setProperties["Name"].ToString();
                    groupProperties.Remove("TermSet");
                }
                groupName = groupProperties["Name"].ToString();
                if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(termSetName)) //在list.SetTaxonomyField的时候如果有新创建的term需要添加到同一个session下Cache中方便后续使用
                {
                    termSet = this.Groups[groupName].TermSets[termSetName];
                }
                //AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, this, groupProperties);
                //if (setProperties == null)
                //{
                //    return null;
                //}
                //AveTermSet termSet = new AveTermSet(m_Request, group, setProperties);
                return termSet;
            }
            return null;
        }

        public IAveTaxonomyGroup GetGroup(Guid groupId)
        {
            //要用Groups里的Group,否则对象不一致。
            return this.Groups[groupId];
            //Dictionary<string, object> groupProperties = m_Request.GetTermGroup(this.ID, groupId);
            //AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, this, groupProperties);
            //return group;
        }
        internal bool IsTermExist(Guid termId)
        {
            return m_Request.IsTermExist(this.ID, termId);
        }

        internal bool IsTermSetExist(Guid termId)
        {
            return m_Request.IsTermSetExist(this.ID, termId);
        }

        public IAveTerm GetTerm(Guid termId)
        {
            Dictionary<string, object> termSetProperties = m_Request.GetTerm(this.ID, termId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> tempTermSetProperties = null;
                string groupName = null;
                string termSetName = null;
                groupName = groupProperties["Name"].ToString();
                if (!string.IsNullOrEmpty(groupName))
                {

                    if (groupProperties.ContainsKey("TermSet"))
                    {
                        tempTermSetProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                        groupProperties.Remove("TermSet");
                        termSetName = tempTermSetProperties["Name"].ToString();
                    }
                    if (!string.IsNullOrEmpty(termSetName))
                    {
                        Dictionary<string, object> termProperties = null;
                        if (tempTermSetProperties.ContainsKey("Term"))
                        {
                            termProperties = tempTermSetProperties["Term"] as Dictionary<string, object>;
                            tempTermSetProperties.Remove("Term");
                        }
                        return new AveTerm(m_Request, this.Groups[groupName].TermSets[termSetName] as AveTermSet, null, termProperties);
                    }
                }
            }
            return null;
        }

        public IAveTerm GetTerm(Guid termSetId, Guid termId)
        {
            Dictionary<string, object> termSetProperties = m_Request.GetTerm(this.ID, termSetId, termId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> tempTermSetProperties = null;
                string groupName = null;
                string termSetName = null;
                groupName = groupProperties["Name"].ToString();
                if (!string.IsNullOrEmpty(groupName))
                {
                    if (groupProperties.ContainsKey("TermSet"))
                    {
                        tempTermSetProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                        groupProperties.Remove("TermSet");
                        termSetName = tempTermSetProperties["Name"].ToString();
                    }
                    if (!string.IsNullOrEmpty(termSetName))
                    {
                        Dictionary<string, object> termProperties = null;
                        if (tempTermSetProperties.ContainsKey("Term"))
                        {
                            termProperties = tempTermSetProperties["Term"] as Dictionary<string, object>;
                            tempTermSetProperties.Remove("Term");
                        }
                        return new AveTerm(m_Request, this.Groups[groupName].TermSets[termSetName] as AveTermSet, null, termProperties);
                    }
                }
            }
            return null;
        }

        public void CommitAll()
        {
            try
            {
                if (base.DataCache.ChangedProperties.Count > 0)
                {
                    Dictionary<string, object> termStoreProperties = m_Request.UpdateTermStore(this.ID,this.DefaultLanguage, base.DataCache.ChangedProperties);
                    Resetproperties(termStoreProperties);
                }
            }
            catch (Exception e)
            {
                //避免下次提交出现问题
                ResetChangedPropertiesWhenCommitFailed();
                mLogger.Error("An error while commiting metadata service data, error: {0}", e);
                throw;
            }
        }

        private void ResetChangedPropertiesWhenCommitFailed()
        {
            if (base.DataCache.ChangedProperties.ContainsKey("GroupActions"))
            {
                var groupActions = base.DataCache.ChangedProperties["GroupActions"] as Dictionary<Guid, Dictionary<string, object>>;
                ResetGroupUpdateKeySetWhenCommitFailed(groupActions);
            }
            base.DataCache.ResetChangedProperties();
        }

        private void ResetGroupUpdateKeySetWhenCommitFailed(Dictionary<Guid, Dictionary<string, object>> groupActions)
        {
            foreach (var groupAction in groupActions)
            {
                try
                {
                    AveTaxonomyGroup group = this.Groups[groupAction.Key] as AveTaxonomyGroup;
                    group.IsUpdateKeySet = false;

                    if (groupAction.Value.ContainsKey("UpdateGroup"))
                    {
                        Dictionary<string, object> updateGroupProperties = groupAction.Value["UpdateGroup"] as Dictionary<string, object>;
                        if (updateGroupProperties.ContainsKey("TermSetActions"))
                        {
                            var termSetActions = updateGroupProperties["TermSetActions"] as Dictionary<Guid, Dictionary<string, object>>;
                            ResetTermSetUpdateKeySetWhenCommitFailed(group, termSetActions);
                        }
                    }
                    group.DataCache.ResetChangedProperties();
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Failed to set group update key set, group id: {0}, exception: {1}", groupAction.Key, ex);
                }
            }
        }

        private void ResetTermSetUpdateKeySetWhenCommitFailed(AveTaxonomyGroup group, Dictionary<Guid, Dictionary<string, object>> termSetActions)
        {
            foreach (var termSetAction in termSetActions)
            {
                try
                {
                    AveTermSet termSet = group.TermSets[termSetAction.Key] as AveTermSet;
                    termSet.IsTermSetKeySet = false;

                    if (termSetAction.Value.ContainsKey("UpdateTermSet"))
                    {
                        Dictionary<string, object> updateTermSetProperties = termSetAction.Value["UpdateTermSet"] as Dictionary<string, object>;
                        if (updateTermSetProperties.ContainsKey("TermActions"))
                        {
                            var termActions = updateTermSetProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>;
                            ResetTermUpdateKeySetWhenCommitFailed(termSet, termActions);
                        }
                    }
                    termSet.DataCache.ResetChangedProperties();
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Failed to set term set update key set, term set id: {0}, exception: {1}", termSetAction.Key, ex);
                }
            }
        }

        private void ResetTermUpdateKeySetWhenCommitFailed(AveTermSet termSet, Dictionary<Guid, Dictionary<string, object>> termActions)
        {
            foreach (var termAction in termActions)
            {
                try
                {
                    AveTerm term = termSet.GetTerm(termAction.Key) as AveTerm;
                    term.IsTermKeySet = false;

                    if (termAction.Value.ContainsKey("UpdateTerm"))
                    {
                        Dictionary<string, object> updateTermProperties = termAction.Value["UpdateTerm"] as Dictionary<string, object>;
                        if (updateTermProperties.ContainsKey("TermActions"))
                        {
                            var subTermActions = updateTermProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>;
                            ResetTermUpdateKeySetWhenCommitFailed(termSet, subTermActions);
                        }
                    }
                    term.DataCache.ResetChangedProperties();
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Failed to set term update key set, term id: {0}, exception: {1}", termAction.Key, ex);
                }
            }
        }

        private void Resetproperties(Dictionary<string, object> termStoreProperties)
        {
            Dictionary<string, object> storeProperties = termStoreProperties[this.ID.ToString()] as Dictionary<string, object>;
            if (storeProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupsProperties = storeProperties["Group"] as Dictionary<string, object>;
                ResetGroupProperties(groupsProperties);
                storeProperties.Remove("Group");
            }
            base.DataCache.UpdateProperties(storeProperties);
        }

        private void ResetGroupProperties(Dictionary<string, object> groupsProperties)
        {
            foreach (KeyValuePair<string, object> oneGroup in groupsProperties)
            {
                AveTaxonomyGroup group = this.Groups[new Guid(oneGroup.Key)] as AveTaxonomyGroup;
                Dictionary<string, object> groupProperties = oneGroup.Value as Dictionary<string, object>;
                if (groupProperties.ContainsKey("TermSet"))
                {
                    Dictionary<string, object> termSetsProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                    ResetTermSetProperties(group, termSetsProperties);
                    groupProperties.Remove("TermSet");
                }
                group.DataCache.UpdateProperties(groupProperties);
                group.IsUpdateKeySet = false;
            }
        }

        private void ResetTermSetProperties(AveTaxonomyGroup group, Dictionary<string, object> termSetsProperties)
        {
            foreach (KeyValuePair<string, object> oneTermSet in termSetsProperties)
            {
                AveTermSet termSet = group.TermSets[new Guid(oneTermSet.Key)] as AveTermSet;
                Dictionary<string, object> termSetProperties = oneTermSet.Value as Dictionary<string, object>;
                if (termSetProperties.ContainsKey("Term"))
                {
                    Dictionary<string, object> termsProperties = termSetProperties["Term"] as Dictionary<string, object>;
                    ResetTermProperties(termSet, termsProperties);
                    termSetProperties.Remove("Term");
                }
                termSet.DataCache.UpdateProperties(termSetProperties);
                termSet.IsTermSetKeySet = false;
                //KeywordsTermSet 和其在Group中取到的对应对象不是同一个,需要统一
                try
                {
                    if (group.IsSystemGroup && termSet.Name.Equals(this.KeywordsTermSet.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Dictionary<string, object> keywordsTermSet = new Dictionary<string, object>();
                        keywordsTermSet["KeywordsTermSet"] = termSet;
                        base.DataCache.UpdateProperties(keywordsTermSet);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug("Reset KeywordsTermSet failed. Error message: {0}", e.ToString());
                }
            }
        }

        private void ResetTermProperties(AveTermSet termSet, Dictionary<string, object> termsProperties)
        {
            foreach (KeyValuePair<string, object> oneTerm in termsProperties)
            {
                AveTerm term = termSet.GetTerm(new Guid(oneTerm.Key)) as AveTerm;
                Dictionary<string, object> termProperties = oneTerm.Value as Dictionary<string, object>;
                term.DataCache.UpdateProperties(termProperties);
                term.IsTermKeySet = false;
            }
        }

        private void ResetChildTermProperties(AveTerm term, Dictionary<string, object> childTermsProperties)
        {
            foreach (KeyValuePair<string, object> oneTerm in childTermsProperties)
            {
                AveTerm childTerm = term.Terms[oneTerm.Key] as AveTerm;
                Dictionary<string, object> termProperties = oneTerm.Value as Dictionary<string, object>;
                if (termProperties.ContainsKey("Term"))
                {
                    Dictionary<string, object> cchildTermsProperties = termProperties["Term"] as Dictionary<string, object>;
                    ResetChildTermProperties(term, cchildTermsProperties);
                    termProperties.Remove("Term");
                }
                childTerm.DataCache.UpdateProperties(termProperties);
            }
        }



        public IAveTaxonomyGroup CreateGroup(string groupName)
        {
            //CheckIsGroupExist(groupName);
            if (!base.DataCache.ChangedProperties.ContainsKey("GroupActions"))
            {
                base.DataCache.AddChangedProperty("GroupActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            Guid groupId = Guid.NewGuid();
            var action = new Dictionary<string, string>();
            (base.DataCache.ChangedProperties["GroupActions"] as Dictionary<Guid, Dictionary<string, object>>)[groupId] =
                new Dictionary<string, object> { { "CreateGroup", groupName } };

            AveTaxonomyGroup newGroup = new AveTaxonomyGroup(groupName, groupId, this, m_Request);
            (this.Groups as AveTaxonomyGroupCollection).ListData.Add(newGroup);
            return newGroup;
        }
        public IAveTaxonomyGroup CreateGroup(string groupName,Guid groupId)
        {
            //CheckIsGroupExist(groupName);
            if (!base.DataCache.ChangedProperties.ContainsKey("GroupActions"))
            {
                base.DataCache.AddChangedProperty("GroupActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            //Guid groupId = Guid.NewGuid();
            var action = new Dictionary<string, string>();
            (base.DataCache.ChangedProperties["GroupActions"] as Dictionary<Guid, Dictionary<string, object>>)[groupId] =
                new Dictionary<string, object> { { "CreateGroup", groupName } };

            AveTaxonomyGroup newGroup = new AveTaxonomyGroup(groupName, groupId, this, m_Request);
            (this.Groups as AveTaxonomyGroupCollection).ListData.Add(newGroup);
            return newGroup;
        }

        private void CheckIsGroupExist(string groupName)
        {
            try
            {
                IAveTaxonomyGroup group = this.Groups[groupName];
                //throw new AveException("Group names must be unique {0}", groupName);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get group:{0} failed.Error Message:{1}.", groupName, ex.ToString());
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_UniqueGroupNameError, groupName);
            }
        }

        public IAveTermSetCollection GetTermSets(string termSetName, int LCID)
        {
            throw new NotImplementedException();
        }

        public IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite iAveSite)
        {
            return this.GetSiteCollectionGroup(iAveSite, true);
        }

        public IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite iAveSite, bool createIfMissing)
        {
            Dictionary<string, object> SiteCollectionGroupProperties = m_Request.GetSiteCollectionGroup(this.ID, iAveSite.Url, createIfMissing);
            if (SiteCollectionGroupProperties == null)
            {
                return null;
            }
            AveTaxonomyGroup siteCollectiongGroup = new AveTaxonomyGroup(m_Request, this, SiteCollectionGroupProperties);
            var groupInCache = (this.Groups as AveTaxonomyGroupCollection).ListData.Find(
                    delegate (IAveTaxonomyGroup group)
                    {
                        return group.ID.Equals(siteCollectiongGroup.ID);
                    });
            if (groupInCache == null)
            {
                (this.Groups as AveTaxonomyGroupCollection).ListData.Add(siteCollectiongGroup);
                return siteCollectiongGroup;
            }
            else
            {
                return groupInCache;
            }
        }

        public string GetSiteCollectionGroupName(IAveSite site)
        {
            StringBuilder builder = new StringBuilder();
            using (IAveWeb web = site.OpenWeb())
            {
                Uri uri = new Uri(site.Url);
                builder.Append(AveSPResource.GetString("SiteCollectionGroupPrefix", new object[] { (web as AveWeb).UICulture.LCID }));
                builder.Append(" - ");
                builder.Append(uri.Host);
                if (!uri.IsDefaultPort)
                {
                    builder.Append("-");
                    builder.Append(uri.Port);
                }
                if (uri.LocalPath.Length > 1)
                {
                    builder.Append(uri.LocalPath.Replace("/", "-"));
                }
            }
            return builder.ToString();
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime)
        {
            if (base.DataCache.IsPropertyNotLoaded("ChangedItems"))
            {
                Dictionary<string, object> changedItemsProperties = m_Request.GetChanges(this.ID, startTime);
                AveChangedItemCollection changedItemsCollection = new AveChangedItemCollection(m_Request, this, changedItemsProperties);
                base.DataCache.PropertiesCache["ChangedItems"] = changedItemsCollection;
            }
            return base.DataCache.GetProperty<IAveChangedItemCollection>("ChangedItems");
        }

        public IAveChangedItemCollection GetChanges(TimeSpan sinceTimeAgo)
        {
            if (base.DataCache.IsPropertyNotLoaded("ChangedItems"))
            {
                Dictionary<string, object> changedItemsProperties = m_Request.GetChanges(this.ID, sinceTimeAgo);
                AveChangedItemCollection changedItemsCollection = new AveChangedItemCollection(m_Request, this, changedItemsProperties);
                base.DataCache.PropertiesCache["ChangedItems"] = changedItemsCollection;
            }
            return base.DataCache.GetProperty<IAveChangedItemCollection>("ChangedItems");
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType)
        {
            if (base.DataCache.IsPropertyNotLoaded("ChangedItems"))
            {
                Dictionary<string, object> changedItemsProperties = m_Request.GetChanges(this.ID, startTime, itemType);
                AveChangedItemCollection changedItemsCollection = new AveChangedItemCollection(m_Request, this, changedItemsProperties);
                base.DataCache.PropertiesCache["ChangedItems"] = changedItemsCollection;
            }
            return base.DataCache.GetProperty<IAveChangedItemCollection>("ChangedItems");
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            if (base.DataCache.IsPropertyNotLoaded("ChangedItems"))
            {
                Dictionary<string, object> changedItemsProperties = m_Request.GetChanges(this.ID, startTime, itemType, operationType);
                AveChangedItemCollection changedItemsCollection = new AveChangedItemCollection(m_Request, this, changedItemsProperties);
                base.DataCache.PropertiesCache["ChangedItems"] = changedItemsCollection;
            }
            return base.DataCache.GetProperty<IAveChangedItemCollection>("ChangedItems");
        }

        public AveTaxonomySession Session
        {
            get
            {
                return this.m_AveTaxonomySession;
            }
        }

        #endregion

        public IAveServiceApplicationProxy SharedServiceProxy
        {
            get { throw new NotImplementedException(); }
        }

        public List<Dictionary<string, object>> TermStoreAdministrators
        {
            get { return base.DataCache.GetProperty<List<Dictionary<string, object>>>("TermStoreAdministrators"); }
        }

        public void AddLanguage(int lcid)
        {
            throw new NotImplementedException();
        }

        public void DeleteLanguage(int lcid)
        {
            throw new NotImplementedException();
        }

        public void FlushCache()
        {

        }
        internal AveTermStore AdminTermStore
        {
            get
            {
                if (m_AveTaxonomySession.AdminTaxonomySession != null)
                {
                    return m_AveTaxonomySession.AdminTaxonomySession.TermStores[this.ID] as AveTermStore;
                }
                return null;
            }
        }
    }
}
