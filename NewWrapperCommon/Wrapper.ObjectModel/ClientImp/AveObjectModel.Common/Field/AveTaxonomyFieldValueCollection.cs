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
    class AveTaxonomyFieldValueCollection : AveAbstractCommonCollection<IAveTaxonomyFieldValue>, IAveTaxonomyFieldValueCollection
    {
        public AveTaxonomyFieldValueCollection()
        {
            mListData = new List<IAveTaxonomyFieldValue>();
        }
        #region IAveTaxonomyFieldValueCollection Members

        public void Add(IAveTaxonomyFieldValue item)
        {
            mListData.Add(item);
        }

        #endregion

        #region IList<IAveTaxonomyFieldValue> Members

        public int IndexOf(IAveTaxonomyFieldValue item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IAveTaxonomyFieldValue item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public IAveTaxonomyFieldValue this[int index]
        {
            get
            {
                return mListData[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<IAveTaxonomyFieldValue> Members


        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IAveTaxonomyFieldValue item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IAveTaxonomyFieldValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(IAveTaxonomyFieldValue item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region toString()
        public override string ToString()
        {
            StringBuilder tempBulider = new StringBuilder();
            foreach (var item in mListData)
            {
                tempBulider.Append(item.ToString()+";");
            }
            return tempBulider.ToString().TrimEnd(';');
        }
        #endregion
    }
}
