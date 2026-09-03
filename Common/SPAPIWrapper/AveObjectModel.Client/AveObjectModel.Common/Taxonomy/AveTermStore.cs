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
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.ObjectModel.Common
{
    class AveTermStore : AveClientObject, IAveTermStore
    {
        private AveTaxonomySession m_AveTaxonomySession;
        private IAveRequest m_Request;
        private AveTermStoreCollection m_AveTermStoreCollection;
        static private AveLogger mLogger = AveLogger.GetInstance(typeof(AveTermStore));

        public AveTermStore(AveTaxonomySession m_AveTaxonomySession, IAveRequest m_Request, AveTermStoreCollection aveTermStoreCollection, IDictionary<string, object> termStoreDic)
        {
            this.m_AveTaxonomySession = m_AveTaxonomySession;
            this.m_Request = m_Request;
            this.m_AveTermStoreCollection = aveTermStoreCollection;
            base.DataCache.AddPropertyies(termStoreDic);
        }

        #region IAveTermStore Members

        public IAveTermSet KeywordsTermSet
        {
            get { throw new NotImplementedException(); }
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
                    base.DataCache.AddProperty("Groups",taxonomyGroupCollection);
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
            get { throw new NotImplementedException(); }
        }

        public Collection<int> Languages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("CastedLanguages"))
                {
                    Collection<int> languages = null;
                    if (base.DataCache.IsPropertyAvailable("Languages"))
                    {
                        List<int> fakeLanguages = base.DataCache.GetProperty<List<int>>("Languages");
                        languages = new Collection<int>(fakeLanguages);
                    }
                    else
                    {
                        languages = new Collection<int>();
                    }
                    base.DataCache.AddProperty("CastedLanguages",languages);
                }
                return base.DataCache.GetProperty<Collection<int>>("CastedLanguages");
            }
        }

        public IAveTermSet GetTermSet(Guid termSetId)
        {
            Dictionary<string, object> termSetProperties = m_Request.GetTermSet(this.ID, termSetId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> setProperties = null;
                if (groupProperties.ContainsKey("TermSet"))
                {
                    setProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                    groupProperties.Remove("TermSet");
                }
                AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, this, groupProperties);
                if (setProperties == null)
                {
                    return null;
                }
                AveTermSet termSet = new AveTermSet(m_Request, group, setProperties);
                return termSet;
            }
            return null;
        }

        public IAveTaxonomyGroup GetGroup(Guid groupId)
        {
            return this.Groups[groupId];
        }

        public IAveTerm GetTerm(Guid termId)
        {
            Dictionary<string, object> termSetProperties = m_Request.GetTerm(this.ID, termId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> setProperties = null;
                if (groupProperties.ContainsKey("TermSet"))
                {
                    setProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                    groupProperties.Remove("TermSet");
                }
                AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, this, groupProperties);
                Dictionary<string, object> termProperties = null;
                ArgumentCheck.CheckNotNull(setProperties);
                if (setProperties.ContainsKey("Term"))
                {
                    termProperties = setProperties["Term"] as Dictionary<string, object>;
                    setProperties.Remove("Term");
                }
                AveTermSet termSet = new AveTermSet(m_Request, group, setProperties);
                AveTerm term = new AveTerm(m_Request, termSet, null, termProperties);
                return term;
            }
            return null;
        }

        public IAveTerm GetTerm(Guid termSetId, Guid termId)
        {
            Dictionary<string, object> termSetProperties = (m_Request as IAveRequest).GetTerm(this.ID, termSetId, termId);
            if (termSetProperties.ContainsKey("Group"))
            {
                Dictionary<string, object> groupProperties = termSetProperties["Group"] as Dictionary<string, object>;
                Dictionary<string, object> setProperties = null;
                if (groupProperties.ContainsKey("TermSet"))
                {
                    setProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                    groupProperties.Remove("TermSet");
                }
                AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, this, groupProperties);
                Dictionary<string, object> termProperties = null;
                ArgumentCheck.CheckNotNull(setProperties);
                if (setProperties.ContainsKey("Term"))
                {
                    termProperties = setProperties["Term"] as Dictionary<string, object>;
                    setProperties.Remove("Term");
                }
                AveTermSet termSet = new AveTermSet(m_Request, group, setProperties);
                AveTerm term = new AveTerm(m_Request, termSet, null, termProperties);
                return term;
            }
            return null;
        }

        public void CommitAll()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> termStoreProperties = m_Request.UpdateTermStore(this.ID, base.DataCache.ChangedProperties);
                Resetproperties(termStoreProperties);
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
                AveTaxonomyGroup group = this.Groups[oneGroup.Key] as AveTaxonomyGroup;
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
                AveTermSet termSet = group.TermSets[oneTermSet.Key] as AveTermSet;
                //termSet.ResetProperties();
                Dictionary<string, object> termSetProperties = oneTermSet.Value as Dictionary<string, object>;
                if (termSetProperties.ContainsKey("Term"))
                {
                    Dictionary<string, object> termsProperties = termSetProperties["Term"] as Dictionary<string, object>;
                    ResetTermProperties(termSet, termsProperties);
                    termSetProperties.Remove("Term");
                }
                termSet.DataCache.UpdateProperties(termSetProperties);
                termSet.IsTermSetKeySet = false;
            }
        }

        private void ResetTermProperties(AveTermSet termSet, Dictionary<string, object> termsProperties)
        {
            foreach (KeyValuePair<string, object> oneTerm in termsProperties)
            {
                AveTerm term = termSet.GetTerm(new Guid(oneTerm.Key)) as AveTerm;
                if (term != null)
                {
                    Dictionary<string, object> termProperties = oneTerm.Value as Dictionary<string, object>;
                    term.DataCache.UpdateProperties(termProperties);
                    term.IsTermKeySet = false;
                }
            }
        }

        public IAveTaxonomyGroup CreateGroup(string groupName)
        {
            //CheckIsGroupExist(groupName);
            if (!base.DataCache.ChangedProperties.ContainsKey("AddGroup"))
            {
                List<string> groupNames = new List<string>();
                base.DataCache.AddChangedProperty("AddGroup", groupNames);
            }
            (base.DataCache.ChangedProperties["AddGroup"] as List<string>).Add(groupName);
            AveTaxonomyGroup newGroup = new AveTaxonomyGroup(groupName, this, m_Request);
            (this.Groups as AveTaxonomyGroupCollection).ListData.Add(newGroup);
            return newGroup;
        }

 

        public IAveTermSetCollection GetTermSets(string termSetName, int LCID)
        {
            Dictionary<string, object> termSetsProperties = m_Request.GetTermSetsInTermStores(termSetName, LCID);
            AveTermSetCollection termsetCollection = SetTermSetProperties(termSetsProperties);
            return termsetCollection;
        }

        private AveTermSetCollection SetTermSetProperties(Dictionary<string, object> termSetsProperties)
        {
            AveTermSetCollection termSetCol = new AveTermSetCollection();
            Dictionary<string, Dictionary<string, object>> termStoresProperties = termSetsProperties["TermStores"] as Dictionary<string, Dictionary<string, object>>;
            foreach (KeyValuePair<string, Dictionary<string, object>> termStoreProperties in termStoresProperties)
            {
                Dictionary<string, Dictionary<string, object>> groupsProperties = termStoreProperties.Value["Groups"] as Dictionary<string, Dictionary<string, object>>;
                termStoreProperties.Value.Remove("Groups");
                AveTermStore store = new AveTermStore(this.Session, m_Request, null, termStoreProperties.Value);
                foreach (KeyValuePair<string, Dictionary<string, object>> groupProperties in groupsProperties)
                {
                    Dictionary<string, object> TermSetProperties = groupProperties.Value["TermSet"] as Dictionary<string, object>;
                    groupProperties.Value.Remove("TermSet");
                    AveTaxonomyGroup taxonomyGroup = new AveTaxonomyGroup(m_Request, store, groupProperties.Value);
                    AveTermSet termSet = new AveTermSet(m_Request, taxonomyGroup, TermSetProperties);
                    termSetCol.ListData.Add(termSet);
                }
            }
            return termSetCol;
        }

        public IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite iAveSite, AveMetadataGroupInfo groupInfo = null)
        {
            AveTaxonomyGroup siteCollectionGroup;
            foreach (AveTaxonomyGroup group in this.Groups)
            {
                if (groupInfo != null)
                {
                    if (group.IsSiteCollectionGroup)
                    {
                        if (group.Name == groupInfo.Name)
                        {
                            mLogger.Info($"get site group success.name:{group.Name}");
                            return group;
                        }
                    }
                }
                else if(group.IsSiteCollectionGroup)
                {
                    return group;
                }
            }
            bool isMysite = AveUrlUtility.IsTenantMySite(iAveSite.Url);
            //onedrive site 不能创建SiteCollectionGroup会抛权限不足的异常。SAAS-11102
            if (!isMysite)
            {
                Dictionary<string, object> SiteCollectionGroupProperties = m_Request.GetSiteCollectionGroup(this.ID, iAveSite.Url);
                siteCollectionGroup = new AveTaxonomyGroup(m_Request, this, SiteCollectionGroupProperties);
                (this.Groups as AveTaxonomyGroupCollection).ListData.Add(siteCollectionGroup);
                return siteCollectionGroup;
            }
            mLogger.Warn("get site group failed.");
            return null;
        }

        public IAveTaxonomyGroup GetSCLevelTermGroup(IAveSite iAveSite)
        {
            //onedrive site 不能创建SiteCollectionGroup会抛权限不足的异常。SAAS-11102
            if (!AveUrlUtility.IsTenantMySite(iAveSite.Url))
            {
                Dictionary<string, object> SiteCollectionGroupProperties = m_Request.GetSiteCollectionGroup(this.ID, iAveSite.Url);
                AveTaxonomyGroup siteCollectionGroup = new AveTaxonomyGroup(m_Request, this, SiteCollectionGroupProperties);
                (this.Groups as AveTaxonomyGroupCollection).ListData.Add(siteCollectionGroup);
                return siteCollectionGroup;
            }
            else
            {
                mLogger.Info("Unable create sc term group for onedrive");
            }
            return null;
        }

        public string GetSiteCollectionGroupName(IAveSite site)
        {
            StringBuilder builder = new StringBuilder();
            using (IAveWeb web = site.OpenWeb())
            {
                Uri uri = new Uri(site.Url);
                builder.Append(AveSPResource.GetString((web as AveWeb).UICulture.LCID, "SiteCollectionGroupPrefix"));
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
    }
}
