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
    class AveLabelCollection : AveAbstractCommonCollection<IAveLabel>, IAveLabelCollection
    {
        private IAveRequest m_Request;
        private AveTerm m_AveTerm;

        public AveLabelCollection()
        {
            base.mListData = new List<IAveLabel>();
        }

        public AveLabelCollection(IAveRequest m_Request, AveTerm aveTerm, Dictionary<string, object> lablesProperties)
        {
            this.m_Request = m_Request;
            this.m_AveTerm = aveTerm;
            base.mListData = new List<IAveLabel>();
            base.DataCache.AddPropertyies(lablesProperties);
            InitLabelCollection();
        }

        private void InitLabelCollection()
        {
            foreach (Dictionary<string, object> labelProperties in base.DataCache.GetProperty<List<Dictionary<string, object>>>("Labels" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                AveLabel label = new AveLabel(m_Request, m_AveTerm, labelProperties);
                mListData.Add(label);
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
