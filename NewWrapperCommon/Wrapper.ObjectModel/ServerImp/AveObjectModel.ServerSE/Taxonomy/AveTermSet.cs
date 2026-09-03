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
using System.Linq;
using System.Collections.Generic;
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;
using System.Collections.ObjectModel;
using System.Reflection;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTermSet : AveTermSetItem, IAveTermSet, IDisposable
    {
        private TermSet mTermSet;
        private AveTermSetSerializer mTermSetSerializer;
        private AveTermStore mTermStore;
        private AveTaxonomyGroup mGroup;

        public AveTermSet(TermSetItem termSet)
            : base(termSet)
        {
            mTermSet = (TermSet)termSet;
        }

        public override IAveTermCollection Terms
        {
            get
            {
                return new AveTermCollection(mTermSet.Terms);
                
            }
        }

        #region IAveTermSet Members

        public override string Name
        {
            get
            {
                return mTermSet.Name;
            }
            set
            {
                mTermSet.Name = value;
            }
        }

        public IAveTermStore TermStore
        {
            get
            {
                if (mTermStore == null)
                {
                    TermStore termStore = mTermSet.TermStore;
                    if (termStore != null)
                    {
                        mTermStore = new AveTermStore(mTermSet.TermStore);
                    }
                }
                return mTermStore;
            }
        }

        public IAveTaxonomyGroup Group
        {
            get
            {
                if (mGroup == null)
                {
                    Group group = mTermSet.Group;
                    if (group != null)
                    {
                        mGroup = new AveTaxonomyGroup(mTermSet.Group);
                    }
                }
                return mGroup;
            }
        }

        public IAveTerm GetTerm(Guid termId)
        {
            Term term = mTermSet.GetTerm(termId);
            if (term == null)
            {
                return null;
            }
            return new AveTerm(term);
        }

        //delete for ADO-51757
        //public IAveTerm CreateTerm(string name, int lcid, Guid newTermID)
        //{
        //    return new AveTerm(mTermSet.CreateTerm(name, lcid, newTermID));
        //}

        public string Description
        {
            get
            {
                return mTermSet.Description;
            }
            set
            {
                mTermSet.Description = value;
            }
        }

        public string Contact
        {
            get
            {
                return mTermSet.Contact;
            }
            set
            {
                mTermSet.Contact = value;
            }
        }

        public bool IsAvailableForTagging
        {
            get
            {
                return mTermSet.IsAvailableForTagging;
            }
            set
            {
                mTermSet.IsAvailableForTagging = value;
            }
        }

        public bool IsOpenForTermCreation
        {
            get
            {
                return mTermSet.IsOpenForTermCreation;
            }
            set
            {
                mTermSet.IsOpenForTermCreation = value;
            }
        }

        public string Owner
        {
            get
            {
                return mTermSet.Owner;
            }
            set
            {
                mTermSet.Owner = value;
            }
        }

        public ReadOnlyCollection<string> Stakeholders
        {
            get
            {
                return mTermSet.Stakeholders;
            }
        }

        public void AddStakeholder(string tStakeHolder)
        {
            mTermSet.AddStakeholder(tStakeHolder);
        }

        public void Delete()
        {
            mTermSet.Delete();
        }

        public IAveTermSetSerializer TermSetSerializer
        {
            get
            {
                if (this.mTermSetSerializer == null)
                {
                    this.mTermSetSerializer = new AveTermSetSerializer(this.mTermSet);
                }
                return this.mTermSetSerializer;
            }
        }

        public IAveTermCollection GetTerms(string termLabel, bool trimUnavailable)
        {
            return new AveTermCollection(mTermSet.GetTerms(termLabel, trimUnavailable));
        }

        public override string CustomSortOrder
        {
            get
            {
                return mTermSet.CustomSortOrder;
            }
            set
            {
                mTermSet.CustomSortOrder = value;
            }
        }

        public override Dictionary<string, string> CustomProperties
        {
            get
            {
                return mTermSet.CustomProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        public Dictionary<int, string> Names
        {
            get
            {
                var namesInfo = mTermSet.GetType().GetProperty("Names", BindingFlags.NonPublic | BindingFlags.Instance);
                return (Dictionary<int, string>)namesInfo.GetValue(mTermSet);
            }
        }

        public IAveTermSet Copy()
        {
            throw new NotImplementedException();
        }

        public void Move(IAveTaxonomyGroup targetGroup)
        {
            throw new NotImplementedException();
        }

        public override void SetCustomProperty(string name, string value)
        {
            mTermSet.SetCustomProperty(name, value);
        }

        public override void DeleteAllCustomProperties()
        {
            this.mTermSet.DeleteAllCustomProperties();
        }

        public override void DeleteCustomProperty(string name)
        {
            this.mTermSet.DeleteCustomProperty(name);
        }

        #endregion

        public void Dispose()
        {
            if (mTermStore != null)
            {
                mTermStore.Dispose();
            }
        }
    }
}