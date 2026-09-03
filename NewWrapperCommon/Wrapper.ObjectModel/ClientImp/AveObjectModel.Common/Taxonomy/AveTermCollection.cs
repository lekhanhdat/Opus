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
    class AveTermCollection:AveAbstractCommonCollection<IAveTerm>,IAveTermCollection
    {
        private IAveRequest m_Request;
        private AveTermSet m_AveTermSet;
        private AveTerm m_AveTerm;
        private Dictionary<string, object> termsProperties;

        public AveTermCollection(IAveRequest m_Request, AveTermSet aveTermSet, Dictionary<string, object> termsProperties)
        {
            this.m_Request = m_Request;
            this.m_AveTermSet = aveTermSet;
            base.DataCache.AddPropertyies(termsProperties);
            mListData = new List<IAveTerm>();
            InitTermCollection();
        }

        public AveTermCollection(IAveRequest m_Request, AveTermSet termSet, AveTerm aveTerm, Dictionary<string, object> termsProperties)
        {
            this.m_Request = m_Request;
            this.m_AveTermSet = termSet;
            this.m_AveTerm = aveTerm;
            base.DataCache.AddPropertyies(termsProperties);
            mListData = new List<IAveTerm>();
            InitTermCollection();
        }

        private void InitTermCollection()
        {
            foreach (Dictionary<string, object> termproperties in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveTerm term = new AveTerm(m_Request, m_AveTermSet, m_AveTerm, termproperties);
                if (m_AveTermSet != null)
                {
                    m_AveTermSet.DataCache.AddWeakReferenceHandler(term.ID.ToString(), term);
                }
                mListData.Add(term);
            }
        }

        #region IAveTermCollection Members


        public IAveTerm this[string index]
        {
            get
            {
                IAveTerm tm= mListData.Find(
                    delegate(IAveTerm term)
                    {
                        return term.Name.Equals(index, StringComparison.OrdinalIgnoreCase);
                    });
                if (tm == null)
                {
                    throw new ArgumentOutOfRangeException(string.Format("Term not exist{0}.", index));
                }
                return tm;
            }
        }

        public IAveTerm this[Guid index]
        {
            get 
            {
                return mListData.Find(
                    delegate(IAveTerm term)
                    {
                        return term.ID.Equals(index);
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
