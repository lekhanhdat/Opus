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
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTaxonomyGroup : AveTaxonomyItem, IAveTaxonomyGroup, IDisposable
    {
        private Group mGroup;
        private AveTaxonomyGroupSerializer mTaxonomyGroupSerializer;
        private AveTermStore mTermStore;

        public AveTaxonomyGroup(Group mGroup)
            : base(mGroup)
        {
            this.mGroup = mGroup;
        }

        #region IAveTaxonomyGroup Members

        public override string Name
        {
            get
            {
                return mGroup.Name;
            }
            set
            {
                mGroup.Name = value;
            }
        }

        public bool IsSiteCollectionGroup
        {
            get
            {
                return mGroup.IsSiteCollectionGroup;
            }
        }

        public Guid Id
        {
            get
            {
                return mGroup.Id;
            }
        }

        public IAveTermStore TermStore
        {
            get
            {
                if (mTermStore == null)
                {
                    TermStore termstore = mGroup.TermStore;
                    if (termstore != null)
                    {
                        mTermStore = new AveTermStore(termstore);
                    }
                }
                return mTermStore;
            }
        }

        public List<Guid> SiteCollectionAccessIds
        {
            get
            {
                return mGroup.SiteCollectionAccessIds;
            }
        }

        public ReadOnlyCollection<string> SiteCollectionReadOnlyAccessUrls
        {
            get
            {
                return mGroup.SiteCollectionReadOnlyAccessUrls;
            }
        }

        public IAveTermSetCollection TermSets
        {
            get
            {
                return new AveTermSetCollection(mGroup.TermSets);
            }
        }

        public string Description
        {
            get
            {
                return mGroup.Description;
            }
            set
            {
                mGroup.Description = value;
            }
        }

        public bool IsSystemGroup
        {
            get
            {
                return mGroup.IsSystemGroup;
            }
        }

        public SPAcl<TaxonomyRights> Contributors
        {
            get
            {
                return mGroup.Contributors;
            }
        }

        public SPAcl<TaxonomyRights> GroupManagers
        {
            get
            {
                return mGroup.GroupManagers;
            }
        }

        public IAveTermSet CreateTermSet(string termsetName)
        {
            return new AveTermSet(mGroup.CreateTermSet(termsetName));
        }

        public IAveTermSet CreateTermSet(string termsetName, Guid newTermSetId)
        {
            var temp = mGroup.TermStore.GetTermSet(newTermSetId);
            if (temp == null)
            {
                //如果是local termset，通过TermStore.GetTermSet只能获取到当前site collection关联的local termset，使用以下方法查询termStore下其他site collection关联的local termset。
                object[] args = new object[] { newTermSetId };
                Type[] paramTypes = new Type[] { typeof(Guid) };
                var objectManager = AveAssemblyUtility.GetPropertyValue(mGroup.TermStore, "ObjectManager");
                var temp1 = AveAssemblyUtility.InvokeMethod(objectManager, "GetTermSet", paramTypes, args);
                if (temp1 != null)
                {
                    return new AveTermSet(mGroup.CreateTermSet(termsetName));
                }
            }
            if (temp != null)
            {
                return new AveTermSet(mGroup.CreateTermSet(termsetName));
            }
            else
            {
                return new AveTermSet(mGroup.CreateTermSet(termsetName, newTermSetId));
            }
        }

        public void AddGroupManager(string principalName)
        {
            mGroup.AddGroupManager(principalName);
        }

        public void AddContributor(string principalName)
        {
            mGroup.AddContributor(principalName);
        }

        public void AddSiteCollectionReadOnlyAccess(string siteCollectionUrl)
        {
            mGroup.AddSiteCollectionReadOnlyAccess(siteCollectionUrl);
        }

        public void Delete()
        {
            mGroup.Delete();
        }

        public IAveTaxonomyGroupSerializer TaxonomyGroupSerializer
        {
            get
            {
                if (this.mTaxonomyGroupSerializer == null)
                {
                    this.mTaxonomyGroupSerializer = new AveTaxonomyGroupSerializer(this.mGroup);
                }
                return this.mTaxonomyGroupSerializer;
            }
        }

        #endregion

        public void Dispose()
        {
            if (mTermStore != null)
                mTermStore.Dispose();
        }
    }
}
