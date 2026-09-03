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

namespace AvePoint.ObjectModel.Common
{
    class AveTermSet : AveTermSetItem, IAveTermSet
    {
        private IAveRequest m_Request;
        private AveTaxonomyGroup m_AveTaxonomyGroup;
        private AveTermStore mTermStore;
        private bool m_Flag;

        public AveTermSet(IAveRequest m_Request, AveTaxonomyGroup aveTaxonomyGroup, IDictionary<string, object> termSetProperties)
        {
            this.m_Request = m_Request as IAveRequest;
            this.m_AveTaxonomyGroup = aveTaxonomyGroup;
            base.DataCache.AddPropertyies(termSetProperties);
        }

        public AveTermSet(IAveRequest m_Request, AveTermStore termStore, IDictionary<string, object> termSetProperties) //for keywordsTermSet
        {
            this.m_Request = m_Request as IAveRequest;
            this.m_AveTaxonomyGroup = (AveTaxonomyGroup)termStore.SystemGroup;
            mTermStore = termStore;
            base.DataCache.AddPropertyies(termSetProperties);
        }

        public AveTermSet(IAveRequest m_Request, string name, AveTaxonomyGroup aveTaxonomyGroup)
        {
            this.m_Request = m_Request as IAveRequest;
            this.Name = name;
            this.m_AveTaxonomyGroup = aveTaxonomyGroup;
        }


        #region IAveTermSet Members

        public IAveTermStore TermStore
        {
            get
            {
                if (mTermStore != null)
                {
                    return mTermStore;
                }
                else
                {
                    return m_AveTaxonomyGroup.TermStore;
                }
            }
        }

        public IAveTaxonomyGroup Group
        {
            get
            {
                return m_AveTaxonomyGroup;
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
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("Description", value);
            }
        }

        public string Contact
        {
            get
            {
                return base.DataCache.GetProperty<string>("Contact");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("Contact", value);
            }
        }

        public bool IsAvailableForTagging
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsAvailableForTagging");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("IsAvailableForTagging", value);
            }
        }

        public bool IsOpenForTermCreation
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsOpenForTermCreation");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("IsOpenForTermCreation", value);
            }
        }

        public string Owner
        {
            get
            {
                return base.DataCache.GetProperty<string>("Owner");
            }
            set
            {
                //try
                //{
                m_AveTaxonomyGroup.CheckUser(value);
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("Owner", value);
                //}
                //catch 
                //{
                //    base.DataCache.AddChangedProperty("Owner", (this.TermStore as AveTermStore).Session.AveSite.RootWeb.CurrentUser.LoginName);
                //}
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> Stakeholders
        {
            get
            {
                System.Collections.ObjectModel.ReadOnlyCollection<string> stakeholders = new System.Collections.ObjectModel.ReadOnlyCollection<string>(base.DataCache.GetProperty<List<string>>("Stakeholders"));
                return stakeholders;
            }
        }

        public IAveTermSetSerializer TermSetSerializer
        {
            get { throw new NotImplementedException(); }
        }

        public override string CustomSortOrder
        {
            get
            {
                return base.DataCache.GetProperty<string>("CustomSortOrder");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("CustomSortOrder", value);
            }
        }

        public void AddStakeholder(string tStakeHolder)
        {
            m_AveTaxonomyGroup.CheckUser(tStakeHolder);
            if (!base.DataCache.ChangedProperties.ContainsKey("AddStakeholder"))
            {
                List<string> stakeHolders = new List<string>();
                base.DataCache.AddChangedProperty("AddStakeholder", stakeHolders);
            }
            (base.DataCache.ChangedProperties["AddStakeholder"] as List<string>).Add(tStakeHolder);
        }

        public void Delete()
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            base.DataCache.ChangedProperties.Clear();
            base.DataCache.AddChangedProperty("DeleteTermSet", true);
        }

        public IAveTerm GetTerm(Guid termId)
        {
            IAveTerm resultTerm = null;           
            resultTerm = GetTermById(Terms, termId);            
            return resultTerm;
        }

        public IAveTerm GetTermById(Guid termId)
        {
            IAveTerm term = null;
            if (base.DataCache.IsPropertyNotLoaded("Terms"))
            {
                term = TermStore.GetTerm(this.ID, termId);
            }
            else
            {
                term = GetTerm(termId);
            }
            return term;
        }

        public IAveTerm GetTermById(IAveTermCollection termCollection, Guid searchId)
        {
            IAveTerm resultTerm = null;
            foreach (IAveTerm term in termCollection)
            {
                if (term.ID.Equals(searchId))
                {
                    resultTerm = term;
                }
                else if (term.Terms.Count > 0)
                {
                    resultTerm = GetTermById(term.Terms, searchId);
                }
                if (resultTerm != null)
                {
                    break;
                }
            }
            return resultTerm;
        }

        public IAveTermCollection GetTerms(string termLabel, bool trimUnavailable)
        {
            Dictionary<string, object> termsProperties = m_Request.GetTerms(TermStore.ID, this.ID, termLabel, trimUnavailable);
            AveTermCollection termCol = new AveTermCollection(m_Request, this, termsProperties);
            return termCol;
        }

        public IAveTermCollection GetAllTerms()
        {
            Dictionary<string, object> termsProperties = m_Request.GetAllTerms(TermStore.ID, this.ID);
            AveTermCollection termCol = new AveTermCollection(m_Request, this, termsProperties);
            return termCol;
        }

        public override IAveTerm CreateTerm(string name, int lcid, Guid newID)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("AddTerm"))
            {
                Dictionary<Guid, List<string>> termNames = new Dictionary<Guid, List<string>>();
                //List<object[]> termNames = new List<object[]>();
                base.DataCache.AddChangedProperty("AddTerm", termNames);
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(name);
            termCreateInfo.Add(lcid.ToString());
            termCreateInfo.Add(newID.ToString());
            (base.DataCache.ChangedProperties["AddTerm"] as Dictionary<Guid, List<string>>).Add(newID, termCreateInfo);
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, this, null, lcid);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }

        public void EnsureTermSetKey()
        {
            if (!m_AveTaxonomyGroup.DataCache.ChangedProperties.ContainsKey("UpdateTermSets"))
            {
                m_AveTaxonomyGroup.DataCache.ChangedProperties.Add("UpdateTermSets", new Dictionary<string, object>());
            }
            if (!(m_AveTaxonomyGroup.DataCache.ChangedProperties["UpdateTermSets"] as Dictionary<string, object>).ContainsKey(this.Name))
            {
                (m_AveTaxonomyGroup.DataCache.ChangedProperties["UpdateTermSets"] as Dictionary<string, object>).Add(this.Name, base.DataCache.ChangedProperties);
            }
        }

        public void EnsureKey()
        {
            if (!m_AveTaxonomyGroup.IsUpdateKeySet)
            {
                m_AveTaxonomyGroup.EnsureGroupKey();
            }
            EnsureTermSetKey();
            m_Flag = true;
        }

        public IAveTermSet Copy()
        {
            throw new NotImplementedException();
        }

        public void Move(IAveTaxonomyGroup targetGroup)
        {
            throw new NotImplementedException();
        }

        #endregion


        public override IAveTermCollection Terms
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Terms"))
                {
                    Dictionary<string, object> termsProperties = m_Request.GetTerms(this.TermStore.ID, m_AveTaxonomyGroup.Name, this.Name, Guid.Empty);
                    AveTermCollection termCol = new AveTermCollection(m_Request, this, termsProperties);
                    base.DataCache.AddProperty("Terms",termCol);
                }
                return base.DataCache.GetProperty<IAveTermCollection>("Terms");
            }
        }

        internal void ResetProperties()
        {
            base.DataCache.RemoveProperty("Terms");
        }

        public override IAveTerm CreateTerm(string name, int lcid)
        {
            return CreateTerm(name, lcid, Guid.NewGuid());
        }

        public bool IsTermSetKeySet
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

        public override IAveTerm PinTerm(IAveTerm sourceTerm)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }

            if (!base.DataCache.ChangedProperties.ContainsKey("PinTerm"))
            {
                Dictionary<Guid, bool> reuseTerms = new Dictionary<Guid, bool>();
                //List<object[]> termNames = new List<object[]>();
                base.DataCache.AddChangedProperty("PinTerm", reuseTerms);
            }

            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(sourceTerm.Name);
            termCreateInfo.Add(this.TermStore.DefaultLanguage.ToString());
            termCreateInfo.Add(sourceTerm.ID.ToString());
            (base.DataCache.ChangedProperties["PinTerm"] as Dictionary<Guid, bool>).Add(sourceTerm.ID, false);
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, this, null, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }

        public override IAveTerm ReuseTerm(IAveTerm sourceTerm, bool reuseBranch)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("ReuseTerm"))
            {
                Dictionary<Guid, bool> reuseTerms = new Dictionary<Guid, bool>();
                //List<object[]> termNames = new List<object[]>();
                base.DataCache.AddChangedProperty("ReuseTerm", reuseTerms);
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(sourceTerm.Name);
            termCreateInfo.Add(this.TermStore.DefaultLanguage.ToString());
            termCreateInfo.Add(sourceTerm.ID.ToString());
            (base.DataCache.ChangedProperties["ReuseTerm"] as Dictionary<Guid, bool>).Add(sourceTerm.ID, reuseBranch);
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, this, null, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }

        public override void SetCustomProperty(string name, string value)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (this.CustomProperties != null)
            {
                this.CustomProperties[name] = value;
            }
            Dictionary<string, string> changedCustomProperties = null;
            if (base.DataCache.ChangedProperties.ContainsKey("ChangedCustomProperties"))
            {
                changedCustomProperties = base.DataCache.ChangedProperties["ChangedCustomProperties"] as Dictionary<string, string>;
            }
            else
            {
                changedCustomProperties = new Dictionary<string, string>();
                base.DataCache.AddChangedProperty("ChangedCustomProperties", changedCustomProperties);
            }
            changedCustomProperties[name] = value;
            //Dictionary<string, string> customProperties = m_Request.SetCustomProperty(this.TermStore.ID, this.ID, Guid.Empty, name, value, AveTermSetItemType.TermSet);
            //base.DataCache.PropertiesCache["CustomProperties"] = customProperties;
        }
    }
}
