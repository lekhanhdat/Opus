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
using AvePoint.GCommon.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Common
{
    class AveTaxonomyGroup : AveClientObject, IAveTaxonomyGroup
    {
        private IAveRequest m_Request;
        private AveTermStore m_termStore;
        private bool m_Flag;
        private AveTaxonomyGroupSerializer mTaxonomyGroupSerializer;
        public AveTaxonomyGroup(IAveRequest m_Request, AveTermStore m_termStore, Dictionary<string, object> taxonomyGroupProperties)
        {
            this.m_Request = m_Request;
            this.m_termStore = m_termStore;
            base.DataCache.AddPropertyies(taxonomyGroupProperties);
        }

        public AveTaxonomyGroup(string groupName, Guid id, AveTermStore aveTermStore, IAveRequest m_Request)
        {
            this.m_termStore = aveTermStore;
            this.m_Request = m_Request;
            this.ID = id;
            base.DataCache.PropertiesCache["Name"] = groupName;
        }

        #region IAveTaxonomyGroup Members

        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureGroupKey();
                }
                base.DataCache.AddChangedProperty("Name", value);
            }
        }
        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
            private set
            {
                base.DataCache.PropertiesCache["Id"] = value;
            }
        }

        public bool IsSiteCollectionGroup
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSiteCollectionGroup");
            }
        }

        public IAveTermStore TermStore
        {
            get { return m_termStore; }
        }

        public List<Guid> SiteCollectionAccessIds
        {
            get
            {
                //return base.DataCache.GetProperty<List<Guid>>("SiteCollectionAccessIds");
                if (base.DataCache.IsPropertyNotLoaded("SiteCollectionAccessIds"))
                {
                    List<Guid> ids = new List<Guid>();
                    ids.Add((this.TermStore as AveTermStore).Session.AveSite.ID);
                    base.DataCache.PropertiesCache["SiteCollectionAccessIds"] = ids;
                }
                return base.DataCache.GetProperty<List<Guid>>("SiteCollectionAccessIds");
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Urls")]
        public ReadOnlyCollection<string> SiteCollectionReadOnlyAccessUrls
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SiteCollectionReadOnlyAccessUrls"))
                {
                    List<string> urls = new List<string>();
                    urls.Add((this.TermStore as AveTermStore).Session.AveSite.Url);
                    ReadOnlyCollection<string> accessUrls = new ReadOnlyCollection<string>(urls);
                    base.DataCache.PropertiesCache["SiteCollectionReadOnlyAccessUrls"] = accessUrls;
                }
                return base.DataCache.GetProperty<ReadOnlyCollection<string>>("SiteCollectionReadOnlyAccessUrls");
            }
        }

        public IAveTermSetCollection TermSets
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TermSets"))
                {
                    Dictionary<string, object> TermSets = m_Request.GetTermSets(m_termStore.ID, this.ID);

                    AveTermSetCollection termSetCollection = new AveTermSetCollection(m_Request, this, TermSets);
                    base.DataCache.PropertiesCache["TermSets"] = termSetCollection;
                }
                return base.DataCache.GetProperty<IAveTermSetCollection>("TermSets");
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureGroupKey();
                }
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public bool IsSystemGroup
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSystemGroup");
            }
        }

        public IAveTaxonomyGroupSerializer TaxonomyGroupSerializer
        {
            get
            {
                if (this.mTaxonomyGroupSerializer == null)
                {
                    this.mTaxonomyGroupSerializer = new AveTaxonomyGroupSerializer(this);
                }
                return this.mTaxonomyGroupSerializer;
            }
        }

        public IAveTermSet CreateTermSet(string name)
        {
            return CreateTermSet(name, Guid.NewGuid());
        }

        public IAveTermSet CreateTermSet(string name, Guid newTermSetId)
        {
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("TermSetActions"))
            {
                base.DataCache.AddChangedProperty("TermSetActions", new Dictionary<Guid, Dictionary<string, object>>());
            }

            AveTermStore adminTermStore = (this.TermStore as AveTermStore).AdminTermStore;
            if (this.IsSiteCollectionGroup && adminTermStore == null)
            {
                newTermSetId = Guid.NewGuid();
            }
            else
            {
                var termExist = adminTermStore == null ? (this.TermStore as AveTermStore).IsTermSetExist(newTermSetId) : (adminTermStore as AveTermStore).IsTermSetExist(newTermSetId);
                newTermSetId = termExist ? Guid.NewGuid() : newTermSetId;
            }

            (base.DataCache.ChangedProperties["TermSetActions"] as Dictionary<Guid, Dictionary<string, object>>)[newTermSetId] =
                new Dictionary<string, object> { { "CreateTermSet", name } };

            AveTermSet newTermset = new AveTermSet(m_Request, name, newTermSetId, this);
            (this.TermSets as AveTermSetCollection).ListData.Add(newTermset);
            return newTermset;
        }

        public void AddGroupManager(string principalName)
        {
            CheckUser(principalName);
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("AddGroupManager"))
            {
                List<string> groupManagers = new List<string>();
                base.DataCache.AddChangedProperty("AddGroupManager", groupManagers);
            }
            (base.DataCache.ChangedProperties["AddGroupManager"] as List<string>).Add(principalName);
        }

        public void AddSiteCollectionReadOnlyAccess(string siteCollectionUrl)
        {
            throw new NotImplementedException();
        }

        public void CheckUser(string principalName)
        {
            AveUtility util = new AveUtility();
            IAveSite site = (this.TermStore as AveTermStore).Session.AveSite;
            IAvePrincipalInfo info = util.ResolvePrincipal(site.RootWeb, principalName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false);
            if (info == null)
            {
                throw new AveException(AveSPResource.GetString("UserCouldNotBeFound", new object[] { principalName }));
            }
        }

        public void AddContributor(string principalName)
        {
            CheckUser(principalName);
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("AddContributor"))
            {
                List<string> groupContributors = new List<string>();
                base.DataCache.AddChangedProperty("AddContributor", groupContributors);
            }
            (base.DataCache.ChangedProperties["AddContributor"] as List<string>).Add(principalName);
        }

        public void Delete()
        {
            if (this.TermSets.Count > 0)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_CannotDeleteGroup, this.Name);
            }
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            base.DataCache.ChangedProperties.Clear();
            base.DataCache.AddChangedProperty("DeleteGroup", true);
        }

        internal void EnsureGroupKey()
        {
            object groupAction;
            if (!m_termStore.DataCache.ChangedProperties.TryGetValue("GroupActions", out groupAction))
            {
                groupAction = new Dictionary<Guid, Dictionary<string, object>>();
                m_termStore.DataCache.AddChangedProperty("GroupActions", groupAction);
            }
            if (!(groupAction as Dictionary<Guid, Dictionary<string, object>>).ContainsKey(this.ID))
            {
                (groupAction as Dictionary<Guid, Dictionary<string, object>>).Add(this.ID, new Dictionary<string, object>());
            }
            (groupAction as Dictionary<Guid, Dictionary<string, object>>)[this.ID]["UpdateGroup"] = this.DataCache.ChangedProperties;
            m_Flag = true;
        }
        #endregion

        #region IAveTaxonomyItem Members

        public int InternalId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public bool IsUpdateKeySet
        {
            get
            {
                return m_Flag;
            }
            set
            {
                m_Flag = value;
            }
        }

        public List<Dictionary<string, object>> Contributors
        {
            get { return base.DataCache.GetProperty<List<Dictionary<string, object>>>("Contributors"); }
        }

        public List<Dictionary<string, object>> GroupManagers
        {
            get { return base.DataCache.GetProperty<List<Dictionary<string, object>>>("GroupManagers"); }
        }
    }
}
