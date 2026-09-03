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
    class AveFieldUserValueCollection : AveAbstractCommonCollection<IAveFieldUserValue>, IAveFieldUserValueCollection
    {
        private AveWeb mWeb;
        private string mLookupValue;
        private List<IAveFieldUserValue> mFieldUserValueCollection;

        public AveFieldUserValueCollection()
        {
            mListData = new List<IAveFieldUserValue>();
        }

        public AveFieldUserValueCollection(AveWeb web, string lookupValue)
        {
            mListData = new List<IAveFieldUserValue>();
            mWeb = web;
            mLookupValue = lookupValue;
            InitFieldUserValueCollection();
        }

        private void InitFieldUserValueCollection()
        {
            List<string> subColumnValues = new List<string>();
            AveSPUtility.TryParseMultiColumnValue(mLookupValue, out subColumnValues);

            for (int i = 0; i < subColumnValues.Count; i += 2)
            {
                int lookupId;
                string lookupValue = string.Empty; ;

                if (int.TryParse(subColumnValues[i], out lookupId))
                {
                    if ((i + 1) < subColumnValues.Count)
                    {
                        lookupValue = subColumnValues[i + 1];
                    }
                    mListData.Add(new AveFieldUserValue(mWeb, lookupId, lookupValue));
                }
            }
        }

        #region IList<IAveFieldUserValue> Members

        public IAveFieldUserValue this[int index]
        {
            get
            {
                return mListData[index];
            }
            set
            {
                mListData[index] = value;
            }
        }

        public int IndexOf(IAveFieldUserValue item)
        {
            return mListData.IndexOf(item);
        }

        public void Insert(int index, IAveFieldUserValue item)
        {
            mListData.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mListData.RemoveAt(index);
        }

        #endregion

        #region ICollection<IAveFieldUserValue> Members

        public bool IsReadOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsReadOnly");
            }
        }

        public void Add(IAveFieldUserValue item)
        {
            mListData.Add(item);
        }

        public void Clear()
        {
            mListData.Clear();
        }

        public bool Contains(IAveFieldUserValue item)
        {
            return mListData.Contains(item);
        }

        public void CopyTo(IAveFieldUserValue[] array, int arrayIndex)
        {
            mListData.CopyTo(array, arrayIndex);
        }

        public bool Remove(IAveFieldUserValue item)
        {
            return mListData.Remove(item);
        }

        public override string ToString()
        {
            List<string> columns = new List<string>();
            foreach (AveFieldUserValue value2 in this)
            {
                columns.Add(value2.LookupId.ToString());
                columns.Add(value2.LookupValue);
            }
            return AveSPCommonUtility.ConvertMultiColumnValueToString(columns, false, false);
        }

        #endregion
    }
}
