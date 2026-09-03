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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveTermSet : AveTermSetItem, IAveTermSet
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveTermSet));
        private IAveRequest m_Request;
        private AveTaxonomyGroup m_AveTaxonomyGroup;
        private AveTermStore mTermStore;
        private AveTermSetSerializer mTermSetSerializer;
        private bool m_Flag;

        public AveTermSet(IAveRequest m_Request, AveTaxonomyGroup aveTaxonomyGroup, Dictionary<string, object> termSetProperties)
        {
            this.m_Request = m_Request;
            this.m_AveTaxonomyGroup = aveTaxonomyGroup;
            base.DataCache.AddPropertyies(termSetProperties);
        }

        public AveTermSet(IAveRequest m_Request, AveTermStore termStore, Dictionary<string, object> termSetProperties) //for keywordsTermSet
        {
            this.m_Request = m_Request;
            this.m_AveTaxonomyGroup = (AveTaxonomyGroup)termStore.SystemGroup;
            mTermStore = termStore;
            base.DataCache.AddPropertyies(termSetProperties);
        }

        public AveTermSet(IAveRequest m_Request, string name,Guid id, AveTaxonomyGroup aveTaxonomyGroup)
        {
            this.m_Request = m_Request;
            this.m_AveTaxonomyGroup = aveTaxonomyGroup;
            this.ID = id;
            base.DataCache.PropertiesCache["Name"] = name;
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

        new public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
            set
            {
                if (!m_Flag)
                {
                    EnsureKey();
                }
                base.DataCache.AddChangedProperty("Name", value);
            }
        }
        new public Guid ID
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
                //m_AveTaxonomyGroup.CheckUser(value);
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
                System.Collections.ObjectModel.ReadOnlyCollection<string> stakeholders = null;
                if (base.DataCache.GetProperty<List<string>>("Stakeholders") == null)
                {
                    // 因为在add termset之后会update termset的stakeholder，但是由于这个时候没有commit，实际的stakeholders数据为空，这个get方法会抛出异常，影响stakeholders数据的还原。所以在没有数据的时候默认给个空值。
                    stakeholders = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new List<string>());
                }
                else
                {
                    stakeholders = new System.Collections.ObjectModel.ReadOnlyCollection<string>(base.DataCache.GetProperty<List<string>>("Stakeholders"));
                }
                return stakeholders;
            }
        }

        public IAveTermSetSerializer TermSetSerializer
        {
            get
            {
                if (this.mTermSetSerializer == null)
                {
                    this.mTermSetSerializer = new AveTermSetSerializer(this);
                }
                return this.mTermSetSerializer;
            }
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
            if (!m_Flag)
            {
                EnsureKey();
            }
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

            object termObject;
            if (base.DataCache.TryGetValueFromWeakReferenceObject(termId.ToString(), out termObject))
            {
                if (termObject != null)
                {
                    resultTerm = termObject as IAveTerm;
                }
                else
                {
                    resultTerm = GetTermById(Terms, termId);
                    if (resultTerm == null)//已经被GC回收，需要重新获取并加到Cache中。
                    {
                        mLogger.Debug("This term was destructed by GC, retrieve now. Term: {0}", termId);
                        resultTerm = this.TermStore.GetTerm(this.ID, termId);
                        if (resultTerm != null)
                        {
                            base.DataCache.AddWeakReferenceHandler(termId.ToString(), resultTerm);
                        }
                    }
                }
            }

            if (resultTerm == null)
            {
                resultTerm = GetTermById(Terms, termId);
            }                      
            return resultTerm;
        }

        public IAveTerm GetTermById(IAveTermCollection termCollection,Guid searchId)
        {
            IAveTerm resultTerm = null;            
            foreach (IAveTerm term in termCollection)
            {
                if (term.ID.Equals(searchId))
                {
                    resultTerm = term;
                }
                //与API保持一致，API GetTerm[Id]方法也会拿Id到MergedTermIds属性中去find
                else if (term.MergedTermIds != null && term.MergedTermIds.Contains(searchId))
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

        public override IAveTerm CreateTerm(string name, int lcid, Guid newID)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("TermActions"))
            {
                base.DataCache.AddChangedProperty("TermActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            Guid termId;
            AveTermStore adminTermStore = (this.TermStore as AveTermStore).AdminTermStore;
            if (this.Group.IsSiteCollectionGroup && adminTermStore == null)
            {
                termId = Guid.NewGuid();
            }
            else
            {
                var termExist = adminTermStore == null ? (this.TermStore as AveTermStore).IsTermExist(newID) : (adminTermStore as AveTermStore).IsTermExist(newID);
                termId = termExist ? Guid.NewGuid() : newID;
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(name);
            termCreateInfo.Add(lcid.ToString());
            termCreateInfo.Add(termId.ToString());
            (base.DataCache.ChangedProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>)[termId] =
                new Dictionary<string, object> { { "CreateTerm", termCreateInfo } };
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, this, null, lcid);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }
        
        
        public void EnsureTermSetKey()
        {
            object termSetActions;
            if ((TermStore.HashTagsTermSet == null || this.ID != TermStore.HashTagsTermSet.ID)
                 && (TermStore.OrphanedTermsTermSet == null || this.ID != TermStore.OrphanedTermsTermSet.ID))
            {
                if (!m_AveTaxonomyGroup.DataCache.ChangedProperties.TryGetValue("TermSetActions", out termSetActions))
                {
                    termSetActions = new Dictionary<Guid, Dictionary<string, object>>();
                    m_AveTaxonomyGroup.DataCache.ChangedProperties.Add("TermSetActions", termSetActions);
                }
                if (!(termSetActions as Dictionary<Guid, Dictionary<string, object>>).ContainsKey(this.ID))
                {
                    (termSetActions as Dictionary<Guid, Dictionary<string, object>>).Add(this.ID, new Dictionary<string, object>());
                }
                (termSetActions as Dictionary<Guid, Dictionary<string, object>>)[this.ID]["UpdateTermSet"] = base.DataCache.ChangedProperties;
            }
            else
            {
                mLogger.Warn("The built-in term set can not be updated. Term Set Name:{0}", this.Name);
            }
            m_Flag = true;
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
                    Dictionary<string, object> termsProperties = m_Request.GetTerms(this.TermStore.ID, m_AveTaxonomyGroup.ID, this.ID, Guid.Empty);
                    AveTermCollection termCol = new AveTermCollection(m_Request, this, termsProperties);
                    base.DataCache.PropertiesCache["Terms"] = termCol;
                }
                return base.DataCache.GetProperty<IAveTermCollection>("Terms");
            }
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

        public Dictionary<int, string> Names
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<int, string>>("Names");
            }
        }

        public override IAveTerm ReuseTerm(IAveTerm sourceTerm, bool reuseBranch)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("TermActions"))
            {
                base.DataCache.AddChangedProperty("TermActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(sourceTerm.Name);
            termCreateInfo.Add(this.TermStore.DefaultLanguage.ToString());
            termCreateInfo.Add(sourceTerm.ID.ToString());
            (base.DataCache.ChangedProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>)[sourceTerm.ID] =
                new Dictionary<string, object> { { "ReuseTerm", reuseBranch } };
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
            object customProperties;
            base.DataCache.ChangedProperties.TryGetValue("CustomProperties", out customProperties);
            if (customProperties == null)
            {
                customProperties = new Dictionary<string, string>();
                base.DataCache.ChangedProperties["CustomProperties"] = customProperties;
            }
            (customProperties as Dictionary<string, string>)[name] = value;
        }
        public override IAveTerm PinTerm(IAveTerm sourceTerm)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("TermActions"))
            {
                base.DataCache.AddChangedProperty("TermActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(sourceTerm.Name);
            termCreateInfo.Add(this.TermStore.DefaultLanguage.ToString());
            termCreateInfo.Add(sourceTerm.ID.ToString());
            (base.DataCache.ChangedProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>)[sourceTerm.ID] =
                new Dictionary<string, object> { { "PinTerm", false } };
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, this, null, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }
    }
}
