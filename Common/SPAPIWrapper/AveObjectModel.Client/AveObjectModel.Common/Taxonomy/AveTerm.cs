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
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.ObjectModel.Common
{
    class AveTerm : AveTermSetItem, IAveTerm
    {
        private IAveRequest m_Request;
        public AveTermSet m_AveTermSet;
        private AveTerm m_parentTerm;
        private AveTerm m_SourceTerm;
        private bool m_Flag;

        public AveTerm(IAveRequest m_Request, List<string> termCreateInfo, AveTermSet aveTermSet, AveTerm parentTerm, int LCID)
            : this(m_Request, termCreateInfo, aveTermSet, parentTerm, null, LCID)
        {
        }

        public AveTerm(IAveRequest m_Request, List<string> termCreateInfo, AveTermSet aveTermSet, AveTerm parentTerm, AveTerm sourceTerm, int LCID)
        {
            this.m_Request = m_Request as IAveRequest;
            this.m_AveTermSet = aveTermSet;
            this.m_SourceTerm = sourceTerm;
            this.Name = termCreateInfo[0];
            base.DataCache.AddProperty("Id",new Guid(termCreateInfo[2]));
            this.m_parentTerm = parentTerm;
            if (parentTerm == null)
            {
                (m_AveTermSet.Terms as AveTermCollection).ListData.Add(this);
            }
            else
            {
                (m_parentTerm.Terms as AveTermCollection).ListData.Add(this);
            }
            AveLabelCollection lableCol = new AveLabelCollection();
            AveLabel lable = new AveLabel(m_Request, this.Name, LCID, true, this);
            lableCol.ListData.Add(lable);
            base.DataCache.AddProperty("Labels",lableCol);
            m_AveTermSet.DataCache.AddWeakReferenceHandler(this.ID.ToString(), this);
        }

        public AveTerm(IAveRequest m_Request, AveTermSet m_AveTermSet, AveTerm parentTerm, IDictionary<string, object> termproperties)
        {
            this.m_Request = m_Request as IAveRequest;
            this.m_AveTermSet = m_AveTermSet;
            this.m_parentTerm = parentTerm;
            base.DataCache.AddPropertyies(termproperties);
        }


        #region IAveTerm Members
        public bool IsAvailableForTagging
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsAvailableForTagging");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsAvailableForTagging", value);
            }
        }

        public bool IsKeyword
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsKeyword");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsKeyword", value);
            }
        }

        public bool IsPinned
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsPinned");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsPinned", value);
            }
        }

        public bool IsRoot
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsRoot");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsRoot", value);
            }
        }

        public bool IsReused
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsReused");
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

        public bool IsDeprecated
        {
            get { return base.DataCache.GetProperty<bool>("IsDeprecated"); }
        }

        public void Deprecate(bool doDeprecate)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            base.DataCache.AddChangedProperty("Deprecate", doDeprecate);
        }

        public IAveLabelCollection Labels
        {
            get
            {
                if (!base.DataCache.IsPropertyAvailable("Labels"))
                {
                    Dictionary<string, object> labelProperties = base.DataCache.GetProperty<Dictionary<string, object>>("Labels" + AveObjectModelConstant.ObjectPropertySuffix);
                    if (labelProperties == null)
                    {
                        labelProperties = m_Request.GetLables(this.TermStore.ID, m_AveTermSet.ID, this.ID);
                    }
                    AveLabelCollection labelCol = new AveLabelCollection(m_Request, this, labelProperties);
                    base.DataCache.AddProperty("Labels",labelCol);
                }
                return base.DataCache.GetProperty<IAveLabelCollection>("Labels");
            }
        }

        public IAveTermStore TermStore
        {
            get
            {
                return m_AveTermSet.TermStore;
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
                (m_AveTermSet.Group as AveTaxonomyGroup).CheckUser(value);
                base.DataCache.AddChangedProperty("Owner", value);
            }
        }

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

        public string PathOfTerm
        {
            get
            {
                return base.DataCache.GetProperty<string>("PathOfTerm");
            }
        }

        public int TermsCount
        {
            get
            {
                return base.DataCache.GetProperty<int>("TermsCount");
            }
        }

        public IAveTerm SourceTerm
        {
            get
            {
                if (m_SourceTerm == null)
                {
                    Dictionary<string, object> sourceTermProp = new Dictionary<string, object>();
                    sourceTermProp["Id"] = base.DataCache.GetProperty<Guid>("SourceTermId");
                    sourceTermProp["Name"] = base.DataCache.GetProperty<string>("SourceTermName");
                    m_SourceTerm = new AveTerm(m_Request, null, null, sourceTermProp);
                }
                return m_SourceTerm;
            }
            set
            {
                m_SourceTerm = value as AveTerm;
            }
        }

        public override IAveTerm CreateTerm(string name, int lcid, Guid newTermId)
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
            termCreateInfo.Add(newTermId.ToString());
            (base.DataCache.ChangedProperties["AddTerm"] as Dictionary<Guid, List<string>>).Add(newTermId, termCreateInfo);
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, m_AveTermSet, this, lcid);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }

        public string GetDefaultLabel(int defaultID)
        {
            return m_Request.GetDefaultLabel(this.TermStore.ID, this.ID, defaultID);
        }

        public string GetDescription(int lcid)
        {
            throw new NotImplementedException();
        }

        public string GetDescription()
        {
            return base.DataCache.GetProperty<string>("Description");
        }

        public void SetDescription(string description, int lcid)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            List<string> functionParam = new List<string>();
            functionParam.Add(description);
            functionParam.Add(lcid.ToString());
            base.DataCache.AddChangedProperty("SetDescription", functionParam);
        }

        public IAveLabel CreateLabel(string labelName, int lcid, bool isDefault)
        {
            if (IsLabelExist(labelName))
            {
                throw new AveException("Cannot create a duplicated label {0}.", labelName);
            }
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("CreateLabel"))
            {
                List<List<string>> paramms = new List<List<string>>();
                base.DataCache.AddChangedProperty("CreateLabel", paramms);
            }
            List<string> functionParam = new List<string>();
            functionParam.Add(labelName);
            functionParam.Add(lcid.ToString());
            functionParam.Add(isDefault.ToString());
            (base.DataCache.ChangedProperties["CreateLabel"] as List<List<string>>).Add(functionParam);
            AveLabel lable = new AveLabel(m_Request, labelName, lcid, isDefault, this);
            (this.Labels as AveLabelCollection).ListData.Add(lable);
            return lable;
        }

        private bool IsLabelExist(string lableName)
        {
            IAveLabel label = (this.Labels as AveLabelCollection).ListData.Find(
            delegate(IAveLabel l)
            {
                return l.Value.Equals(lableName);
            });
            if (label != null)
            {
                return true;
            }
            return false;
        }

        public IAveTermSerializer TermSerializer
        {
            get { throw new NotImplementedException(); }
        }

        public void Delete()
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            base.DataCache.ChangedProperties.Clear();
            base.DataCache.AddChangedProperty("DeleteTerm", true);
        }

        private void EnsureTermKey()
        {
            //if (m_parentTerm == null)
            //{
            if (!m_AveTermSet.DataCache.ChangedProperties.ContainsKey("UpdateTerms"))
            {
                m_AveTermSet.DataCache.ChangedProperties.Add("UpdateTerms", new Dictionary<string, object>());
            }
           
            if (!(m_AveTermSet.DataCache.ChangedProperties["UpdateTerms"] as Dictionary<string, object>).ContainsKey(this.ID.ToString()))
            {
                (m_AveTermSet.DataCache.ChangedProperties["UpdateTerms"] as Dictionary<string, object>).Add(this.ID.ToString(), base.DataCache.ChangedProperties);
            }
                        
            //}
            //else
            //{
            //    if (!m_parentTerm.DataCache.ChangedProperties.ContainsKey("UpdateTerms"))
            //    {
            //        m_parentTerm.DataCache.ChangedProperties.Add("UpdateTerms", new Dictionary<string, object>());
            //    }
            //    if (!(m_parentTerm.DataCache.ChangedProperties["UpdateTerms"] as Dictionary<string, object>).ContainsKey(this.ID.ToString()))
            //    {
            //        (m_parentTerm.DataCache.ChangedProperties["UpdateTerms"] as Dictionary<string, object>).Add(this.ID.ToString(), base.DataCache.ChangedProperties);
            //    }
            //}
        }

        private void EnsureKey()
        {
            if (!(m_AveTermSet.Group as AveTaxonomyGroup).IsUpdateKeySet)
            {
                (m_AveTermSet.Group as AveTaxonomyGroup).EnsureGroupKey();
            }
            if (!m_AveTermSet.IsTermSetKeySet)
            {
                m_AveTermSet.EnsureTermSetKey();
            }
            this.EnsureTermKey();
            m_Flag = true;
        }

        public IAveTerm Copy(bool doCopyChildren)
        {
            throw new NotImplementedException();
        }

        public IAveTerm Merge(IAveTerm termToMerge)
        {
            throw new NotImplementedException();
        }

        public void Move(IAveTerm newParentTerm)
        {
            throw new NotImplementedException();
        }

        public void Move(IAveTermSet parentTermSet)
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
                    Dictionary<string, object> termsProperties = null;
                    if (this.TermsCount == 0)
                    {
                        termsProperties = new Dictionary<string, object>();
                        var termsList = new List<IDictionary<string, object>>();
                        termsProperties.AddChildren(termsList);
                    }
                    else
                    {
                        termsProperties = m_Request.GetTerms(this.TermStore.ID, m_AveTermSet.Group.Name, m_AveTermSet.Name, this.ID);
                    }
                    AveTermCollection termCol = new AveTermCollection(m_Request, m_AveTermSet, this, termsProperties);
                    base.DataCache.AddProperty("Terms",termCol);
                }
                return base.DataCache.GetProperty<IAveTermCollection>("Terms");
            }
        }

        public override IAveTerm CreateTerm(string name, int lcid)
        {
            return CreateTerm(name, lcid, Guid.NewGuid());
        }

        public bool IsTermKeySet
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

        public bool IsSourceTerm
        {
            get { return base.DataCache.GetProperty<bool>("IsSourceTerm"); }
        }      

        public bool IsPinnedRoot
        {
            get { return base.DataCache.GetProperty<bool>("IsPinnedRoot"); }
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
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, m_AveTermSet, this, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
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
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, m_AveTermSet, this, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }

        #region add for SP2013
        public Dictionary<string, string> LocalCustomProperties
        {
            get
            {
                return base.DataCache.GetProperty<Dictionary<string, string>>("LocalCustomProperties");
            }
        }

        public void DeleteAllLocalCustomProperties()
        {
            throw new NotImplementedException();
        }

        public void DeleteLocalCustomProperty(string name)
        {
            throw new NotImplementedException();
        }

        public void SetLocalCustomProperty(string name, string value)
        {
            //Dictionary<string, string> localCustomProperties = m_Request.SetLocalCustomProperty(this.TermStore.ID, this.m_AveTermSet.ID, this.ID, name, value);
            //base.DataCache.PropertiesCache["LocalCustomProperties"] = localCustomProperties;
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (this.LocalCustomProperties != null)
            {
                this.LocalCustomProperties[name] = value;
            }
            Dictionary<string, string> changedCustomProperties = null;
            if (base.DataCache.ChangedProperties.ContainsKey("ChangedLocalCustomProperties"))
            {
                changedCustomProperties = base.DataCache.ChangedProperties["ChangedLocalCustomProperties"] as Dictionary<string, string>;
            }
            else
            {
                changedCustomProperties = new Dictionary<string, string>();
                base.DataCache.AddChangedProperty("ChangedLocalCustomProperties", changedCustomProperties);
            }
            changedCustomProperties[name] = value;
        }

        public override void SetCustomProperty(string name, string value)
        {
            //Dictionary<string, string> customProperties = m_Request.SetCustomProperty(this.TermStore.ID, this.m_AveTermSet.ID, this.ID, name, value, AveTermSetItemType.Term);
            //base.DataCache.PropertiesCache["CustomProperties"] = customProperties;
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
        }

        #endregion



        public void ReassignSourceTerm(IAveTerm reusedTerm)
        {
            AveTerm term = reusedTerm as AveTerm;
            term.EnsureKey();
            if (!term.DataCache.ChangedProperties.ContainsKey("ReassignSourceTerm"))
            {
                term.DataCache.ChangedProperties["ReassignSourceTerm"] = true;
            }
        }

        public Guid ParentTermId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("ParentTermId");
            }
        }

        public IAveTerm Parent
        {
            get
            {
                if (m_parentTerm == null && base.DataCache.IsPropertyAvailable("ParentTermId"))
                {
                    Guid parentId = DataCache.GetPropertyWithoutChange<Guid>("ParentTermId");
                    Dictionary<string, object> parentProperties = m_Request.GetTerm(Guid.Empty, parentId);
                    if (parentProperties.ContainsKey("Group"))
                    {
                        Dictionary<string, object> groupProperties = parentProperties["Group"] as Dictionary<string, object>;
                        Dictionary<string, object> setProperties = null;
                        if (groupProperties.ContainsKey("TermSet"))
                        {
                            setProperties = groupProperties["TermSet"] as Dictionary<string, object>;
                            groupProperties.Remove("TermSet");
                        }
                        AveTaxonomyGroup group = new AveTaxonomyGroup(m_Request, null, groupProperties);
                        Dictionary<string, object> termProperties = null;
                        if (setProperties.ContainsKey("Term"))
                        {
                            termProperties = setProperties["Term"] as Dictionary<string, object>;
                            setProperties.Remove("Term");
                        }
                        AveTermSet termSet = new AveTermSet(m_Request, group, setProperties);
                        m_parentTerm = new AveTerm(m_Request, termSet, null, termProperties);
                    }
                }
                return m_parentTerm;
            }
        }

        public IAveLabelCollection GetAllLabels(int language)
        {
            throw new NotImplementedException();
        }


        public IAveTermSet TermSet
        {
            get { return m_AveTermSet; }
        }


        public Guid PinSourceTermSet
        {            
            get { return base.DataCache.GetProperty<Guid>("PinSourceTermSetId"); }
        }
    }
}
