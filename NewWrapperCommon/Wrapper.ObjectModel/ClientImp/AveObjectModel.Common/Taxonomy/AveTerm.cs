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
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    class AveTerm : AveTermSetItem, IAveTerm
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveTerm));
        private IAveRequest m_Request;
        public AveTermSet m_AveTermSet;
        private AveTerm m_parentTerm;
        private AveTerm m_SourceTerm;
        private AveTermSerializer mTermSerializer;
        private bool m_Flag;
        private Dictionary<string, string> mChangedLCPSourceValue = new Dictionary<string, string>();

        public AveTerm(IAveRequest m_Request, List<string> termCreateInfo, AveTermSet aveTermSet, AveTerm parentTerm, int LCID)
            : this(m_Request, termCreateInfo, aveTermSet, parentTerm, null, LCID)
        {
        }

        public AveTerm(IAveRequest m_Request, List<string> termCreateInfo, AveTermSet aveTermSet, AveTerm parentTerm, AveTerm sourceTerm, int LCID)
        {
            this.m_Request = m_Request;
            this.m_AveTermSet = aveTermSet;
            this.m_SourceTerm = sourceTerm;
            base.DataCache.PropertiesCache["Id"] = new Guid(termCreateInfo[2]);
            this.m_parentTerm = parentTerm;
            if (parentTerm == null)
            {
                (m_AveTermSet.Terms as AveTermCollection).ListData.Add(this);
            }
            else
            {
                (m_parentTerm.Terms as AveTermCollection).ListData.Add(this);
            }
            base.DataCache.PropertiesCache["Name"] = termCreateInfo[0];
            AveLabelCollection lableCol = new AveLabelCollection();
            AveLabel lable = new AveLabel(m_Request, this.Name, LCID, true, this);
            lableCol.ListData.Add(lable);
            base.DataCache.PropertiesCache["Labels"] = lableCol;
            m_AveTermSet.DataCache.AddWeakReferenceHandler(this.ID.ToString(), this);
        }

        public AveTerm(IAveRequest m_Request, AveTermSet m_AveTermSet, AveTerm parentTerm, Dictionary<string, object> termproperties)
        {
            this.m_Request = m_Request;
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
                    Dictionary<string, object> labelProperties = base.DataCache.GetProperty<Dictionary<string, object>>("tempLabels");
                    if (labelProperties == null)
                    {
                        labelProperties = m_Request.GetLables(this.TermStore.ID, m_AveTermSet.ID, this.ID);
                    }
                    AveLabelCollection labelCol = new AveLabelCollection(m_Request, this, labelProperties);
                    base.DataCache.PropertiesCache["Labels"] = labelCol;
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
            set
            {
                base.DataCache.AddChangedProperty("Id", value);
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

        public List<Guid> MergedTermIds
        {
            get
            {
                return base.DataCache.GetProperty<List<Guid>>("MergedTermIds");
            }
        }
        public override IAveTerm CreateTerm(string name, int lcid, Guid newTermId)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("TermActions"))
            {
                base.DataCache.AddChangedProperty("TermActions", new Dictionary<Guid, Dictionary<string, object>>());
            }
            Guid termId = newTermId;
            AveTermStore adminTermStore = (this.TermStore as AveTermStore).AdminTermStore;
            if (this.TermSet.Group.IsSiteCollectionGroup && adminTermStore == null)
            {
                termId = Guid.NewGuid();
            }
            else
            {
                var termExist = adminTermStore == null ? (this.TermStore as AveTermStore).IsTermExist(newTermId) : (adminTermStore as AveTermStore).IsTermExist(newTermId);
                termId = termExist ? Guid.NewGuid() : newTermId;
            }
            List<string> termCreateInfo = new List<string>();
            termCreateInfo.Add(name);
            termCreateInfo.Add(lcid.ToString());
            termCreateInfo.Add(termId.ToString());
            (base.DataCache.ChangedProperties["TermActions"] as Dictionary<Guid, Dictionary<string, object>>)[termId] =
                new Dictionary<string, object> { { "CreateTerm", termCreateInfo } };
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
            try
            {
                return m_Request.GetDescription(this.TermStore.ID, this.TermSet.ID, this.ID, lcid);
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to get description by lcid, lcid: {0}. Exception: {1}", lcid, e);
                return GetDescription();
            }
        }
        /// <summary>
        /// 获取当前Term下所有language的description。
        /// 1.如果当前TermStore只存在一个Language,不需要走Request。
        /// 2.如果当前TermStore存在多个Language,走一次Request即可。
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> GetAllDescriptions()
        {
            if (TermStore.Languages.Count == 1)
            {
                return new Dictionary<int, string> { { TermStore.Languages[0], GetDescription() } };
            }
            var descriptions = new Dictionary<int, string>();
            try
            {
                descriptions = m_Request.GetAllDescriptions(this.TermStore.ID, this.TermSet.ID, this.ID, this.TermStore.Languages);
            }
            catch (Exception e)
            {
                mLogger.Warn("Failed to get all descriptions. Term: {0}, Exception: {1}", this.ID, e);
            }
            return descriptions;
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
            if (!base.DataCache.ChangedProperties.ContainsKey("SetDescription"))
            {
                base.DataCache.AddChangedProperty("SetDescription", new List<List<string>>() { functionParam });
            }
            else
            {
                (base.DataCache.ChangedProperties["SetDescription"] as List<List<string>>).Add(functionParam);
            }
        }

        public IAveLabel CreateLabel(string labelName, int lcid, bool isDefault)
        {
            if (IsLabelExist(labelName, lcid))
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_Common_CannotCreateDuplicatedLabel, labelName);
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

        private bool IsLabelExist(string lableName, int lcid)
        {
            IAveLabel label = (this.Labels as AveLabelCollection).ListData.Find(
            delegate (IAveLabel l)
            {
                return l.Value.Equals(lableName) && l.Language.Equals(lcid);
            });
            if (label != null)
            {
                return true;
            }
            return false;
        }

        public IAveTermSerializer TermSerializer
        {
            get
            {
                if (this.mTermSerializer == null)
                {
                    this.mTermSerializer = new AveTermSerializer(this);
                }
                return this.mTermSerializer;
            }
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
            object termActions;
            if (m_parentTerm == null)
            {
                if (!m_AveTermSet.DataCache.ChangedProperties.TryGetValue("TermActions", out termActions))
                {
                    termActions = new Dictionary<Guid, Dictionary<string, object>>();
                    m_AveTermSet.DataCache.ChangedProperties.Add("TermActions", termActions);
                }
                if (!(termActions as Dictionary<Guid, Dictionary<string, object>>).ContainsKey(this.ID))
                {
                    (termActions as Dictionary<Guid, Dictionary<string, object>>).Add(this.ID, new Dictionary<string, object>());
                }
                (termActions as Dictionary<Guid, Dictionary<string, object>>)[this.ID]["UpdateTerm"] = base.DataCache.ChangedProperties;
            }
            else
            {
                m_parentTerm.EnsureKey();
                if (!m_parentTerm.DataCache.ChangedProperties.TryGetValue("TermActions", out termActions))
                {
                    termActions = new Dictionary<Guid, Dictionary<string, object>>();
                    m_parentTerm.DataCache.ChangedProperties.Add("TermActions", termActions);
                }
                if (!(termActions as Dictionary<Guid, Dictionary<string, object>>).ContainsKey(this.ID))
                {
                    (termActions as Dictionary<Guid, Dictionary<string, object>>).Add(this.ID, new Dictionary<string, object>());
                }
                (termActions as Dictionary<Guid, Dictionary<string, object>>)[this.ID]["UpdateTerm"] = base.DataCache.ChangedProperties;
            }
        }

        private void EnsureKey()
        {
            if (m_Flag)
            {
                return;
            }
            if (!m_AveTermSet.IsTermSetKeySet)
            {
                m_AveTermSet.EnsureKey();
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
                        List<Dictionary<string, object>> termsList = new List<Dictionary<string, object>>();
                        termsProperties[AveObjectModelConstant.ChildrenProperties] = termsList;
                    }
                    else
                    {
                        termsProperties = m_Request.GetTerms(this.TermStore.ID, m_AveTermSet.Group.ID, m_AveTermSet.ID, this.ID);
                    }
                    AveTermCollection termCol = new AveTermCollection(m_Request, m_AveTermSet, this, termsProperties);
                    base.DataCache.PropertiesCache["Terms"] = termCol;
                }
                return base.DataCache.GetProperty<IAveTermCollection>("Terms");
            }
        }

        private int TermsCount
        {
            get
            {
                return base.DataCache.GetProperty<int>("TermsCount");
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
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, m_AveTermSet, this, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            return newTerm;
        }

        #region add for SP2013

        /// <summary>
        /// 由于LocalCustomProperties是集合，Get时将Property和ChangeProperty合并。如果想Set，请调用SetLocalCustomProperty方法。ADO-155569
        /// </summary>
        public Dictionary<string, string> LocalCustomProperties
        {
            get
            {
                Dictionary<string, string> properties;
                Dictionary<string, string> changeProperties;

                var value = base.DataCache.PropertiesCache.FirstOrDefault(p => p.Key.Equals("LocalCustomProperties", StringComparison.Ordinal));
                properties = value.Value == null ? null : ((Dictionary<string, string>)value.Value).ToDictionary(k => k.Key, v => v.Value);
                value = base.DataCache.ChangedProperties.FirstOrDefault(p => p.Key.Equals("LocalCustomProperties", StringComparison.Ordinal));
                changeProperties = value.Value == null ? null : ((Dictionary<string, string>)value.Value).ToDictionary(k => k.Key, v => v.Value);

                if (properties == null && changeProperties == null)
                {
                    return new Dictionary<string, string>();
                }
                else if (properties != null && changeProperties != null)//merge两个property
                {
                    foreach (var cProperty in changeProperties)
                    {
                        properties[cProperty.Key] = cProperty.Value;
                    }
                    return properties;
                }
                else
                {
                    return properties == null ? changeProperties : properties;
                }
            }
        }

        public Dictionary<string, string> ChangedLCPSourceValue
        {
            get
            {
                return mChangedLCPSourceValue;
            }
            set
            {
                mChangedLCPSourceValue = value;
            }
        }

        public void DeleteAllLocalCustomProperties()
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            base.DataCache.ChangedProperties["DeleteAllLocalCustomProperty"] = "True";
        }

        public void DeleteLocalCustomProperty(string name)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            object needDeleteProperties;
            base.DataCache.ChangedProperties.TryGetValue("DeleteLocalCustomPropertyByName", out needDeleteProperties);
            if (needDeleteProperties == null)
            {
                needDeleteProperties = new List<string>();
                base.DataCache.ChangedProperties["DeleteLocalCustomPropertyByName"] = needDeleteProperties;
            }
            (needDeleteProperties as List<string>).Add(name);
        }

        public void SetLocalCustomProperty(string name, string value)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("LocalCustomProperties"))
            {
                base.DataCache.ChangedProperties["LocalCustomProperties"] = new Dictionary<string, string>();
            }
            (base.DataCache.ChangedProperties["LocalCustomProperties"] as Dictionary<string, string>)[name] = value;
        }

        public override void SetCustomProperty(string name, string value)
        {
            if (!m_Flag)
            {
                EnsureKey();
            }
            if (!base.DataCache.ChangedProperties.ContainsKey("CustomProperties"))
            {
                Dictionary<string, string> customProperties = new Dictionary<string, string>();
                base.DataCache.ChangedProperties["CustomProperties"] = customProperties;
            }
            (base.DataCache.ChangedProperties["CustomProperties"] as Dictionary<string, string>)[name] = value;
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


        public IAveTerm Parent
        {
            get
            {
                if (m_parentTerm == null && base.DataCache.PropertiesCache.ContainsKey("ParentTermId"))
                {
                    Guid parentId = (Guid)base.DataCache.PropertiesCache["ParentTermId"];
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


        public bool IsSourceTerm
        {
            get { return base.DataCache.GetProperty<bool>("IsSourceTerm"); }
        }

        public bool IsPinned
        {
            get { return base.DataCache.GetProperty<bool>("IsPinned"); }
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
            AveTerm newTerm = new AveTerm(m_Request, termCreateInfo, m_AveTermSet, this, sourceTerm as AveTerm, this.TermStore.DefaultLanguage);
            //(this.Terms as AveTermCollection).ListData.Add(newTerm);
            return newTerm;
        }


        public IAveLabelCollection GetAllLabels(int language)
        {
            throw new NotImplementedException();
        }


        public IAveTermSet TermSet
        {
            get { return m_AveTermSet; }
        }


        public Guid PinSourceTermSetId
        {
            get { return base.DataCache.GetProperty<Guid>("PinSourceTermSetId"); }
        }

        public string PathOfTerm
        {
            get
            {
                return base.DataCache.GetProperty<string>("PathOfTerm");
            }
        }
    }
}
