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
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server16
{
    class AveTerm : AveTermSetItem, IAveTerm, IDisposable
    {
        private Term mTerm;
        private AveTerm mSourceTerm;
        private AveTermSerializer mTermSerializer;
        private AveTermStore mTermStore;
        private AveLabelCollection mLabels;
        private AveTerm mParent;
        private Dictionary<string, string> mChangedLCPSourceValue = new Dictionary<string, string>();

        public AveTerm(TermSetItem term)
            : base(term)
        {
            mTerm = (Term)term;
        }

        internal Term Term
        {
            get
            {
                return mTerm;
            }
        }

        public override IAveTermCollection Terms
        {
            get
            {
                return new AveTermCollection(mTerm.Terms);
            }
        }

        #region IAveTerm Members

        public override string Name
        {
            get
            {
                return mTerm.Name;
            }
            set
            {
                mTerm.Name = value;
            }
        }

        public IAveTermStore TermStore
        {
            get
            {
                if (mTermStore == null)
                {
                    TermStore termStore = mTerm.TermStore;
                    if (termStore != null)
                    {
                        mTermStore = new AveTermStore(termStore);
                    }
                }
                return mTermStore;
            }
        }

        //public IAveTerm CreateTerm(string name, int lcid, Guid newTermId)
        //{
        //    return new AveTerm(mTerm.CreateTerm(name, lcid, newTermId));
        //}

        public string GetDefaultLabel(int defaultID)
        {
            return mTerm.GetDefaultLabel(defaultID);
        }

        public string Owner
        {
            get
            {
                return mTerm.Owner;
            }
            set
            {
                mTerm.Owner = value;
            }
        }

        public bool IsAvailableForTagging
        {
            get
            {
                return mTerm.IsAvailableForTagging;
            }
            set
            {
                mTerm.IsAvailableForTagging = value;
            }
        }

        public Dictionary<int, string> GetAllDescriptions()
        {
            var descriptions = new Dictionary<int, string>();
            foreach (var lcid in TermStore.Languages)
            {
                descriptions.Add(lcid, GetDescription(lcid));
            }
            return descriptions;
        }

        public string GetDescription(int lcid)
        {
            return mTerm.GetDescription(lcid);
        }

        public string GetDescription()
        {
            return mTerm.GetDescription();
        }

        public IAveLabelCollection Labels
        {
            get
            {
                if (mLabels == null)
                {
                    mLabels = new AveLabelCollection(mTerm.Labels);
                }
                return mLabels;
            }
        }

        public bool IsKeyword
        {
            get
            {
                return mTerm.IsKeyword;
            }
        }

        public bool IsRoot
        {
            get
            {
                return mTerm.IsRoot;
            }
        }

        public IAveTerm SourceTerm
        {
            get
            {
                if (mSourceTerm == null)
                {
                    Term sourceTerm = mTerm.SourceTerm;
                    if (sourceTerm != null)
                    {
                        mSourceTerm = new AveTerm(mTerm.SourceTerm);
                    }
                }
                return mSourceTerm;
            }
        }

        public void SetDescription(string description, int lcid)
        {
            mTerm.SetDescription(description, lcid);
        }

        public IAveLabel CreateLabel(string lableName, int lcid, bool isDefault)
        {
            return new AveLabel(mTerm.CreateLabel(lableName, lcid, isDefault));
        }

        public IAveTermSerializer TermSerializer
        {
            get
            {
                if (this.mTermSerializer == null)
                {
                    this.mTermSerializer = new AveTermSerializer(this.mTerm);
                }
                return this.mTermSerializer;
            }
        }

        public override Dictionary<string, string> CustomProperties
        {
            get
            {
                return mTerm.CustomProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        public override void SetCustomProperty(string name, string value)
        {
            mTerm.SetCustomProperty(name, value);
        }

        public void Delete()
        {
            this.mTerm.Delete();
        }

        public override string CustomSortOrder
        {
            get
            {
                return mTerm.CustomSortOrder;
            }
            set
            {
                mTerm.CustomSortOrder = value;
            }
        }

        public bool IsDeprecated
        {
            get
            {
                return mTerm.IsDeprecated;
            }
        }
        public void Deprecate(bool doDeprecate)
        {
            this.mTerm.Deprecate(doDeprecate);
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

        public void DeleteAllLocalCustomProperties()
        {
            this.mTerm.DeleteAllLocalCustomProperties();
        }

        public override void DeleteAllCustomProperties()
        {
            this.mTerm.DeleteAllCustomProperties();
        }

        public Dictionary<string, string> LocalCustomProperties
        {
            get
            {
                return this.mTerm.LocalCustomProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
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

        public void SetLocalCustomProperty(string name, string value)
        {
            this.mTerm.SetLocalCustomProperty(name, value);
        }

        public void DeleteLocalCustomProperty(string name)
        {
            this.mTerm.DeleteLocalCustomProperty(name);
        }

        public override void DeleteCustomProperty(string name)
        {
            this.mTerm.DeleteCustomProperty(name);
        }

        public void ReassignSourceTerm(IAveTerm reusedTerm)
        {
            this.mTerm.ReassignSourceTerm((reusedTerm as AveTerm).Term);
        }
        #endregion

        public IAveTerm Parent
        {
            get
            {
                if (mParent == null && mTerm.Parent != null)
                {
                    mParent = new AveTerm(mTerm.Parent);
                }
                return mParent;
            }
        }

        public void Dispose()
        {
            if (mTermStore != null)
                mTermStore.Dispose();

        }


        public bool IsReused
        {
            get { return mTerm.IsReused; }
        }

        public bool IsSourceTerm
        {
            get { return mTerm.IsSourceTerm; }
        }

        public bool IsPinned
        {
            get { return mTerm.IsPinned; }
        }

        public bool IsPinnedRoot
        {
            get { return mTerm.IsPinnedRoot; }
        }


        public IAveLabelCollection GetAllLabels(int language)
        {
            return new AveLabelCollection(this.mTerm.GetAllLabels(language));
        }


        public IAveTermSet TermSet
        {
            get { return new AveTermSet(mTerm.TermSet); }
        }


        public Guid PinSourceTermSetId
        {
            get { return mTerm.PinSourceTermSet.Id; }
        }


        public List<Guid> MergedTermIds
        {
            get
            {
                if (mTerm.MergedTermIds != null)
                {
                    return mTerm.MergedTermIds.ToList();
                }
                else
                {
                    return new List<Guid>();
                }
            }
        }

        public string PathOfTerm
        {
            get
            {
                return mTerm.GetPath();
            }
        }
    }
}
