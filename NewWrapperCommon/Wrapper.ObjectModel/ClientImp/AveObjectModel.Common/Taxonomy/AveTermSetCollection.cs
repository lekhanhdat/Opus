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
    class AveTermSetCollection : AveAbstractCommonCollection<IAveTermSet>,IAveTermSetCollection
    {
        private IAveRequest m_Request;
        private AveTaxonomyGroup m_AveTaxonomyGroup;

        public AveTermSetCollection()
        {
            mListData = new List<IAveTermSet>();
        }

        public AveTermSetCollection(IAveRequest m_Request, AveTaxonomyGroup aveTaxonomyGroup, Dictionary<string, object> TermSets)
        {
            this.m_Request = m_Request;
            this.m_AveTaxonomyGroup = aveTaxonomyGroup;
            base.DataCache.AddPropertyies(TermSets);
            mListData = new List<IAveTermSet>();
            InitTermSetCollection();
        }

        private void InitTermSetCollection()
        {
            foreach (Dictionary<string,object> termSetProperties in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveTermSet termSet = new AveTermSet(m_Request,m_AveTaxonomyGroup, termSetProperties);
                mListData.Add(termSet);
            }
        }

        #region IAveTermSetCollection Members


        public IAveTermSet this[Guid termId]
        {
            get
            {
                return mListData.Find(
                    delegate (IAveTermSet termset)
                    {
                        return termset.ID.Equals(termId);
                    });
            }
        }

        public IAveTermSet this[string termName]
        {
            get
            {
                return mListData.Find(
                    delegate(IAveTermSet termset)
                    {
                        return termset.Name.Equals(termName);
                    });
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
