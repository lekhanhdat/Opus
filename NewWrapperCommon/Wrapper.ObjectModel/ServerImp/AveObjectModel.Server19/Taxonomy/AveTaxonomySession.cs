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


using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.Office.Server.SocialData;
using System;
using System.Reflection;

namespace AvePoint.ObjectModel.Server19
{
    class AveTaxonomySession : IAveTaxonomySession,IDisposable
    {
        private TaxonomySession mTaxonomySession;
        private AveTermStoreCollection mTermStores;
        private AveTermStore mDefaultKeywordsTermStore;
        private AveTermStore mDefaultSiteCollectionTermStore;

        public AveTaxonomySession(TaxonomySession taxonomySession)
        {
            mTaxonomySession = taxonomySession;
        }

        public AveTaxonomySession(IAveSite aveSite)
        {
            mTaxonomySession = new TaxonomySession((aveSite as AveSite).Site, true);
        }

        public AveTaxonomySession()
        {
            SPServiceContext serviceContext = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);
            SocialTagManager socialTagmanager = new SocialTagManager(serviceContext);
            this.mTaxonomySession = socialTagmanager.TaxonomySession;
        }

        public AveTaxonomySession(IAveServiceContext context)
        {
            BindingFlags flags = BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public
                                                              | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                                                              | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.IgnoreCase
                                                              | BindingFlags.CreateInstance;
            ConstructorInfo info = typeof(TaxonomySession).GetConstructor(flags, null, new Type[] { typeof(SPServiceContext) }, null);
            mTaxonomySession = info.Invoke(new object[] { (context as AveServiceContext).ServiceContext }) as TaxonomySession;
        }
        #region IAveTaxonomySession Members

        public IAveTermStoreCollection TermStores
        {
            get
            {
                if (mTermStores == null)
                {
                    TermStoreCollection termStoreCollection = mTaxonomySession.TermStores;
                    if (termStoreCollection != null)
                    {
                        mTermStores = new AveTermStoreCollection(termStoreCollection);
                    }
                }
                return mTermStores;
            }
        }

        public IAveTermStore DefaultKeywordsTermStore
        {
            get
            {
                if (mDefaultKeywordsTermStore == null)
                {
                    TermStore termStore = mTaxonomySession.DefaultKeywordsTermStore;
                    if (termStore != null)
                    {
                        mDefaultKeywordsTermStore = new AveTermStore(termStore);
                    }
                }
                return mDefaultKeywordsTermStore;
            }
        }

        public IAveTermSetCollection GetTermSets(string termSetName, int LCID)
        {
            return new AveTermSetCollection(mTaxonomySession.GetTermSets(termSetName, LCID));
        }

        public IAveTermStore DefaultSiteCollectionTermStore
        {
            get
            {
                if (mDefaultSiteCollectionTermStore == null)
                {
                    TermStore termStore = mTaxonomySession.DefaultSiteCollectionTermStore;
                    if (termStore != null)
                    {
                        mDefaultSiteCollectionTermStore = new AveTermStore(termStore);
                    }
                }
                return mDefaultSiteCollectionTermStore;
            }
        }

        public IAveTerm GetTerm(Guid guid)
        {
            Term term = mTaxonomySession.GetTerm(guid);
            if (term == null)
            {
                return null;
            }
            return new AveTerm(term);
        }

        public IAveTermCollection GetTerms(string termLabel, bool trimUnavailable)
        {
            TermCollection terms = mTaxonomySession.GetTerms(termLabel, trimUnavailable);
            if (terms == null)
            {
                return null;
            }
            return new AveTermCollection(terms);
        }

        public bool CheckTermStoreAdmin(Guid termStoreId)
        {
            bool result = false;
            foreach (TermStore termStore in mTaxonomySession.TermStores)
            {
                if (termStore.Id == termStoreId && termStore.DoesUserHavePermissions(TaxonomyRights.TermStoreAdministrator))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SetAdminSite(IAveSite site)
        {
            return;
        }
    }
}
