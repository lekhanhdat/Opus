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
    class AveTaxonomyGroupCollection : AveAbstractCommonCollection<IAveTaxonomyGroup>, IAveTaxonomyGroupCollection
    {
        private IAveRequest m_Request;
        private AveTermStore m_termStore;

        public AveTaxonomyGroupCollection(IAveRequest m_Request, AveTermStore termstore, Dictionary<string, object> TaxonomyGroupsProperties)
        {
            this.m_Request = m_Request;
            m_termStore = termstore;
            this.mListData = new List<IAveTaxonomyGroup>();
            base.DataCache.AddPropertyies(TaxonomyGroupsProperties);
            InitTaxonomyGroupCollection();
        }

        private void InitTaxonomyGroupCollection()
        {
            foreach(Dictionary<string,object> taxonomyGroupProperties in base.DataCache.GetProperty<List<Dictionary<string,object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveTaxonomyGroup taxonomyGroup = new AveTaxonomyGroup(m_Request, m_termStore, taxonomyGroupProperties);
                mListData.Add(taxonomyGroup);
            }
        }


        #region IAveTaxonomyGroupCollection Members


        public IAveTaxonomyGroup this[string groupName]
        {
            get
            {
                IAveTaxonomyGroup taxonomyGroup = mListData.Find(
                    delegate(IAveTaxonomyGroup group)
                    {
                        return group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase);
                    });
                if (taxonomyGroup == null)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return taxonomyGroup;
            }
        }

        public IAveTaxonomyGroup this[Guid groupId]
        {
            get 
            {
                IAveTaxonomyGroup taxonomyGroup = mListData.Find(
                    delegate(IAveTaxonomyGroup group)
                    {
                        return group.ID.Equals(groupId);
                    });
                if (taxonomyGroup == null)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return taxonomyGroup;
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
