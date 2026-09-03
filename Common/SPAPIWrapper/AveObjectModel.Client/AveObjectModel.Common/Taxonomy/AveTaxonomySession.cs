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
    class AveTaxonomySession : AveClientObject, IAveTaxonomySession
    {

        private IAveSite m_AveSite;
        private IAveRequest m_Request;

        public AveTaxonomySession(IAveSite aveSite)
        {
            m_AveSite = aveSite;
            m_Request = (aveSite as AveSite).Request;
            Dictionary<string, object> sessionproperties = m_Request.GetTaxonomySession();
            base.DataCache.AddPropertyies(sessionproperties);
        }

        #region IAveTaxonomySession Members

        public IAveTermStoreCollection TermStores
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TermStores"))
                {
                    Dictionary<string, object> termStoreDic = m_Request.GetTermStores();
                    AveTermStoreCollection TermStores = new AveTermStoreCollection(this, m_Request, termStoreDic);
                    base.DataCache.AddProperty("TermStores",TermStores);
                }
                return base.DataCache.GetProperty<IAveTermStoreCollection>("TermStores");
            }
        }

        public IAveTermStore DefaultSiteCollectionTermStore
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultSiteCollectionTermStore"))
                {
                    AveTermStore DefaultSiteCollectionTermStore = new AveTermStore(this, m_Request, null, base.DataCache.GetPropertyWithoutChange<IDictionary<string,object>>("DefaultSiteCollectionTermStore" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.AddProperty("DefaultSiteCollectionTermStore",DefaultSiteCollectionTermStore);
                }
                IAveTermStore termStore = base.DataCache.GetProperty<IAveTermStore>("DefaultSiteCollectionTermStore");
                if (termStore.ID.Equals(Guid.Empty))
                {
                    return null;
                }
                else
                {
                    return termStore;
                }
            }
        }

        public IAveTermStore DefaultKeywordsTermStore
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultKeywordsTermStore"))
                {
                    AveTermStore DefaultSiteCollectionTermStore = new AveTermStore(this, m_Request, null, base.DataCache.GetPropertyWithoutChange<IDictionary<string,object>>("DefaultKeywordsTermStore" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.AddProperty("DefaultKeywordsTermStore",DefaultSiteCollectionTermStore);
                }
                return base.DataCache.GetProperty<IAveTermStore>("DefaultKeywordsTermStore");
            }
        }

        public IAveTermSetCollection GetTermSets(string termSetName, int LCID)
        {
            Dictionary<string, object> termSetsProperties = m_Request.GetTermSetsInTermStores(termSetName, LCID);
            AveTermSetCollection termsetCollection = SetTermSetProperties(termSetsProperties);
            return termsetCollection;
        }

        private AveTermSetCollection SetTermSetProperties(Dictionary<string, object> termSetsProperties)
        {
            AveTermSetCollection termSetCol = new AveTermSetCollection();
            Dictionary<string, Dictionary<string, object>> termStoresProperties = termSetsProperties["TermStores"] as Dictionary<string, Dictionary<string, object>>;
            foreach (KeyValuePair<string, Dictionary<string, object>> termStoreProperties in termStoresProperties)
            {
                Dictionary<string, Dictionary<string, object>> groupsProperties = termStoreProperties.Value["Groups"] as Dictionary<string, Dictionary<string, object>>;
                termStoreProperties.Value.Remove("Groups");
                AveTermStore store = new AveTermStore(this, m_Request, null, termStoreProperties.Value);
                foreach (KeyValuePair<string, Dictionary<string, object>> groupProperties in groupsProperties)
                {
                    Dictionary<string, object> TermSetProperties = groupProperties.Value["TermSet"] as Dictionary<string, object>;
                    groupProperties.Value.Remove("TermSet");
                    AveTaxonomyGroup taxonomyGroup = new AveTaxonomyGroup(m_Request, store, groupProperties.Value);
                    AveTermSet termSet = new AveTermSet(m_Request, taxonomyGroup, TermSetProperties);
                    termSetCol.ListData.Add(termSet);
                }
            }
            return termSetCol;
        }

        public IAveTerm GetTerm(Guid guid)
        {
            return this.TermStores[0].GetTerm(guid);
            //throw new NotImplementedException();
        }

        public IAveTermCollection GetTerms(string termLabel, bool trimUnavailable)
        {
            throw new NotImplementedException();
        }

        public IAveSite AveSite
        {
            get
            {
                return this.m_AveSite;
            }
        }

        #endregion

        public void Dispose()
        {
            mLogger.Info("Start dispose taxonomy session....");
            base.DataCache.Dispose();
        }
    }
}
