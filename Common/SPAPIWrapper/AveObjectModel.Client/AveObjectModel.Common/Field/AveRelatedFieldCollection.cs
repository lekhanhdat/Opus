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
    class AveRelatedFieldCollection : AveAbstractCommonCollection<IAveRelatedField>, IAveRelatedFieldCollection
    {
        private IAveRequest m_Request;
        private AveList m_AveList;

        public AveRelatedFieldCollection(IAveRequest mRequest, AveList aveList, Dictionary<string, object> relatedFieldProperties)
        {
            this.m_Request = mRequest;
            this.m_AveList = aveList;
            base.DataCache.AddPropertyies(relatedFieldProperties);
            mListData = new List<IAveRelatedField>();
            InitRelatedFieldCollection();
        }

        private void InitRelatedFieldCollection()
        {
            var relatedFieldsProperties = base.DataCache.GetChildren();
            foreach (var relatedFieldProperties in relatedFieldsProperties)
            {
                AveRelatedField relatedField = new AveRelatedField(m_Request, m_AveList, this, relatedFieldProperties);
                mListData.Add(relatedField);
            }
        }
        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}