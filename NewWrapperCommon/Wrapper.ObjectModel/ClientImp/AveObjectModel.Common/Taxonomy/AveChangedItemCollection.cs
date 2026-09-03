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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace AvePoint.ObjectModel.Common
{
    class AveChangedItemCollection : AveAbstractCommonCollection<IAveChangedItem>, IAveChangedItemCollection
    {
        private IAveRequest m_Request;
        private AveTermStore m_termStore;

        public AveChangedItemCollection(IAveRequest m_Request, AveTermStore termstore, Dictionary<string, object> changedItemsProperties)
        {
            this.m_Request = m_Request;
            m_termStore = termstore;
            this.mListData = new List<IAveChangedItem>();
            base.DataCache.AddPropertyies(changedItemsProperties);
            InitchangedItemsProperties();
        }

        private void InitchangedItemsProperties()
        {
            foreach (Dictionary<string, object> changedProperties in base.DataCache.GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties))
            {
                AveChangedItem taxonomyGroup = new AveChangedItem(m_Request, m_termStore, changedProperties);
                mListData.Add(taxonomyGroup);
            }
        }
    }
}
