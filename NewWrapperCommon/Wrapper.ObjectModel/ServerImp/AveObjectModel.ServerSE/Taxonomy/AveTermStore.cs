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
using System.Collections.ObjectModel;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Taxonomy;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTermStore : IAveTermStore, IDisposable
    {
        private TermStore mTermStore;
        private AveTermStoreSerializer mTermStoreSerializer;
        private AveTermSet mKeywordsTermSet;
        private AveTermSet mOrphanedTermsTermSet;
        private AveTermSet mHashTagsTermSet;
        private AveTaxonomyGroup mSystemGroup;
        private Collection<int> mLanguages;
        private AveServiceApplicationProxy serviceProxy;

        public AveTermStore(TermStore termStore)
        {
            mTermStore = termStore;
        }

        #region IAveTermStore Members

        public IAveTermSet GetTermSet(Guid termSetId)
        {
            TermSet termSet = mTermStore.GetTermSet(termSetId);
            if (termSet == null)
            {
                return null;
            }
            return new AveTermSet(termSet);
        }

        public Guid ID
        {
            get
            {
                return mTermStore.Id;
            }
        }

        public string Name
        {
            get
            {
                return mTermStore.Name;
            }
        }

        public IAveTaxonomyGroupCollection Groups
        {
            get
            {
                return new AveTaxonomyGroupCollection(mTermStore.Groups);
            }
        }

        public IAveTaxonomyGroup GetGroup(Guid groupid)
        {
            Group group = mTermStore.GetGroup(groupid);
            if (group == null)
            {
                return null;
            }
            return new AveTaxonomyGroup(group);
        }

        public IAveTerm GetTerm(Guid termId)
        {
            Term term = mTermStore.GetTerm(termId);
            if (term == null)
            {
                return null;
            }
            return new AveTerm(term);
        }

        public IAveTerm GetTerm(Guid termSetId, Guid termId)
        {
            Term term = mTermStore.GetTerm(termSetId, termId);
            if (term == null)
            {
                return null;
            }
            return new AveTerm(term);
        }

        public void CommitAll()
        {
            mTermStore.CommitAll();
        }

        public IAveTermSet KeywordsTermSet
        {
            get
            {
                if (mKeywordsTermSet == null)
                {
                    TermSet termSet = mTermStore.KeywordsTermSet;
                    if (termSet != null)
                    {
                        mKeywordsTermSet = new AveTermSet(termSet);
                    }
                }
                return mKeywordsTermSet;
            }
        }

        public IAveTermSet HashTagsTermSet
        {
            get
            {
                if (mHashTagsTermSet == null)
                {
                    TermSet termSet = mTermStore.HashTagsTermSet;
                    if (termSet != null)
                    {
                        mHashTagsTermSet = new AveTermSet(termSet);
                    }
                }
                return mHashTagsTermSet;
            }
        }

        public IAveTermSet OrphanedTermsTermSet
        {
            get
            {
                if (mOrphanedTermsTermSet == null)
                {
                    TermSet termSet = mTermStore.OrphanedTermsTermSet;
                    if (termSet != null)
                    {
                        mOrphanedTermsTermSet = new AveTermSet(termSet);
                    }
                }
                return mOrphanedTermsTermSet;
            }
        }

        public int DefaultLanguage
        {
            get
            {
                return mTermStore.DefaultLanguage;
            }
        }

        public int WorkingLanguage
        {
            get
            {
                return mTermStore.WorkingLanguage;
            }
        }

        public SPAcl<TaxonomyRights> TermStoreAdministrators
        {
            get
            {
                return mTermStore.TermStoreAdministrators;
            }
        }

        internal TermStore CurrentTermStore
        {
            get
            {
                return this.mTermStore;
            }
        }

        public IAveTaxonomyGroup SystemGroup
        {
            get
            {
                if (mSystemGroup == null)
                {
                    Group group = mTermStore.SystemGroup;
                    if (group != null)
                    {
                        mSystemGroup = new AveTaxonomyGroup(group);
                    }
                }
                return mSystemGroup;
            }
        }

        public IAveTaxonomyGroup CreateGroup(string groupName)
        {
            return new AveTaxonomyGroup(mTermStore.CreateGroup(groupName));
        }

        public IAveTermSetCollection GetTermSets(string termSetName, int LCID)
        {
            return new AveTermSetCollection(mTermStore.GetTermSets(termSetName, LCID));
        }

        public IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite site)
        {
            return this.GetSiteCollectionGroup(site, true);
        }

        public IAveTaxonomyGroup GetSiteCollectionGroup(IAveSite site, bool createIfMissing)
        {
            SPSite spSite = (site as AveSite).Site;
            Group termGroup = mTermStore.GetSiteCollectionGroup(spSite, createIfMissing);
            if (termGroup == null)
            {
                return null;
            }
            return new AveTaxonomyGroup(termGroup);
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime)
        {
            return new AveChangedItemCollection(mTermStore.GetChanges(startTime));
        }

        public IAveChangedItemCollection GetChanges(TimeSpan sinceTimeAgo)
        {
            return new AveChangedItemCollection(mTermStore.GetChanges(sinceTimeAgo));
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType)
        {
            return new AveChangedItemCollection(mTermStore.GetChanges(startTime, (ChangedItemType)itemType));
        }

        public IAveChangedItemCollection GetChanges(DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            return new AveChangedItemCollection(mTermStore.GetChanges(startTime, (ChangedItemType)itemType, (ChangedOperationType)operationType));
        }

        public Uri ContentTypePublishingHub
        {
            get { return mTermStore.ContentTypePublishingHub; }
        }

        public string GetSiteCollectionGroupName(IAveSite site)
        {
            return AveAssemblyUtility.InvokeMethod(mTermStore, "GetSiteCollectionGroupName", new Type[] { typeof(SPSite) }, new object[] { (site as AveSite).Site }) as string;
        }

        public IAveTermStoreSerializer TermStoreSerializer
        {
            get { throw new NotImplementedException(); }
        }

        public Collection<int> Languages
        {
            get
            {
                return mTermStore.Languages;
            }
        }

        #endregion


        public IAveServiceApplicationProxy SharedServiceProxy
        {
            get
            {
                if (serviceProxy == null)
                {
                    SPServiceApplicationProxy proxy = AveAssemblyUtility.GetPropertyValue(this.mTermStore, "SharedServiceProxy") as SPServiceApplicationProxy;
                    serviceProxy = new AveServiceApplicationProxy(proxy);
                }

                return serviceProxy;
            }
        }

        public void Dispose()
        {
            if (serviceProxy != null)
            {
                serviceProxy.Dispose();
                serviceProxy = null;
            }
        }

        public void AddLanguage(int lcid)
        {
            mTermStore.AddLanguage(lcid);
        }

        public void DeleteLanguage(int lcid)
        {
            mTermStore.DeleteLanguage(lcid);
        }

        public void FlushCache()
        {
            if (mTermStore != null)
            {
                mTermStore.FlushCache();
            }
        }

        public IAveTaxonomyGroup CreateGroup(string groupName, Guid groupId)
        {
            return new AveTaxonomyGroup(mTermStore.CreateGroup(groupName, groupId));
        }
    }
}
