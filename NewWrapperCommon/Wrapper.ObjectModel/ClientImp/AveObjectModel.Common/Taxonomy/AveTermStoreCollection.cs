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
    class AveTermStoreCollection : AveAbstractCommonCollection<IAveTermStore>, IAveTermStoreCollection
    {
        private AveTaxonomySession m_AveTaxonomySession;
        private IAveRequest m_Request;


        public AveTermStoreCollection(AveTaxonomySession aveTaxonomySession, IAveRequest request, Dictionary<string, object> termStoreDic)
        {
            this.m_AveTaxonomySession = aveTaxonomySession;
            this.m_Request = request;
            base.DataCache.AddPropertyies(termStoreDic);
            this.mListData = new List<IAveTermStore>();
            InitTermStoreCollection();
        }

        private void InitTermStoreCollection()
        {
            var termStoreCollection = base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            if (termStoreCollection != null)
            {
                foreach (Dictionary<string, object> termStoreDic in termStoreCollection)
                {
                    AveTermStore termStore = new AveTermStore(m_AveTaxonomySession, m_Request, this, termStoreDic);
                    mListData.Add(termStore);
                }
            }
        }

        public int Count
        {
            get { return base.ListData.Count; }
        }

        #region IAveTermStoreCollection Members

        public IAveTermStore this[Guid id]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveTermStore termStore)
                    {
                        return termStore.ID.Equals(id);
                    });
            }
        }

        public IAveTermStore this[string termName]
        {
            get
            {
                IAveTermStore tStore = mListData.Find(
                    delegate(IAveTermStore termStore)
                    {
                        return termStore.Name.Equals(termName);
                    });
                if (tStore == null)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return tStore;
            }
        }

        #endregion

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
