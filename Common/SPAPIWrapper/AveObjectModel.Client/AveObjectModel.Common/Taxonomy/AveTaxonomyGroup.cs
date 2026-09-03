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

namespace AvePoint.ObjectModel.Common
{
    class AveTaxonomyGroup : AveClientObject, IAveTaxonomyGroup
    {
        private IAveRequest m_Request;
        private AveTermStore m_termStore;
        private bool m_Flag;

        public AveTaxonomyGroup(IAveRequest m_Request, AveTermStore m_termStore, IDictionary<string, object> taxonomyGroupProperties)
        {
            this.m_Request = m_Request;
            this.m_termStore = m_termStore;
            base.DataCache.AddPropertyies(taxonomyGroupProperties);
        }

        public AveTaxonomyGroup(string groupName, AveTermStore aveTermStore, IAveRequest m_Request)
        {
            this.Name = groupName;
            this.m_termStore = aveTermStore;
            this.m_Request = m_Request;
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
                base.DataCache.AddChangedProperty("Name", value);
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
                if (base.DataCache.IsPropertyNotLoaded("SiteCollectionAccessIds"))
                {
                    List<Guid> ids = new List<Guid>();
                    ids.Add((this.TermStore as AveTermStore).Session.AveSite.ID);
                    base.DataCache.AddProperty("SiteCollectionAccessIds",ids);
                }
                return base.DataCache.GetProperty<List<Guid>>("SiteCollectionAccessIds");
            }
        }

        public IAveTermSetCollection TermSets
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TermSets"))
                {
                    Dictionary<string, object> TermSets = m_Request.GetTermSets(m_termStore.ID, this.Name);

                    AveTermSetCollection termSetCollection = new AveTermSetCollection(m_Request, this, TermSets);
                    base.DataCache.AddProperty("TermSets",termSetCollection);
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
            get { throw new NotImplementedException(); }
        }

        public IAveTermSet CreateTermSet(string name)
        {
            return CreateTermSet(name, Guid.NewGuid());
        }

        public IAveTermSet CreateTermSet(string name, Guid newTermSetId)
        {
            IAveTermSet temp = this.TermSets[newTermSetId];
            if (temp != null || this.IsSiteCollectionGroup)
            {
                newTermSetId = Guid.NewGuid();
            }
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("AddTermSet"))
            {
                List<Dictionary<string, object>> termSetList = new List<Dictionary<string, object>>();
                base.DataCache.AddChangedProperty("AddTermSet", termSetList);
            }
            Dictionary<string, object> termSetProp = new Dictionary<string, object>();
            termSetProp.Add("Name", name);
            termSetProp.Add("Id", newTermSetId);
            (base.DataCache.ChangedProperties["AddTermSet"] as List<Dictionary<string, object>>).Add(termSetProp);
            AveTermSet newTermset = new AveTermSet(m_Request, name, this);
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
            base.DataCache.AddChangedProperty("AddGroupManager", principalName);
        }

        public void CheckUser(string principalName)
        {
            AveUtility util = new AveUtility();
            IAveSite site = (this.TermStore as AveTermStore).Session.AveSite;
            IAveUser user = site.RootWeb.SiteUsers.GetByLoginName(principalName);
            if (user == null)
            {
                IAvePrincipalInfo info = util.ResolvePrincipal(site.RootWeb, principalName, AvePrincipalType.SecurityGroup | AvePrincipalType.User, AvePrincipalSource.All, null, false);
                if (info == null || info.PrincipalID < 0)
                {
                    throw new AveException(AveSPResource.GetString("UserCouldNotBeFound", new object[] { principalName }));
                }
            }
        }

        public void AddContributor(string principalName)
        {
            CheckUser(principalName);
            if (!m_Flag)
            {
                EnsureGroupKey();
            }
            base.DataCache.AddChangedProperty("AddContributor", principalName);
        }

        public void Delete()
        {
            if (this.TermSets.Count > 0)
            {
                throw new AveException("A Group cannot be deleted unless it is empty {0}", this.Name);
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
            if (!m_termStore.DataCache.ChangedProperties.ContainsKey("UpdateGroups"))//SAAS-10368 Revert Changed  by zma
            {
                m_termStore.DataCache.AddChangedProperty("UpdateGroups", new Dictionary<string, object>());
            }
            if (!(m_termStore.DataCache.ChangedProperties["UpdateGroups"] as Dictionary<string, object>).ContainsKey(this.Name))
            {
                (m_termStore.DataCache.ChangedProperties["UpdateGroups"] as Dictionary<string, object>).Add(this.Name, base.DataCache.ChangedProperties);
            }
            m_Flag = true;
        }
        #endregion

        #region IAveTaxonomyItem Members


        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

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
