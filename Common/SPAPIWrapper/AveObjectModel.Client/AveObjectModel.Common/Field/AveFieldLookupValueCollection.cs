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
using System.Globalization;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.Common
{
    class AveFieldLookupValueCollection:AveAbstractCommonCollection<IAveFieldLookupValue>,IAveFieldLookupValueCollection
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveFieldLookupValueCollection));
        public AveFieldLookupValueCollection()
        {
            mListData = new List<IAveFieldLookupValue>();
        }

        public AveFieldLookupValueCollection(string fieldValue)
        {
            mListData = new List<IAveFieldLookupValue>();
            if (string.IsNullOrEmpty(fieldValue))
            {
                return;
            }
            Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
            List<string> list = AveFieldMultiColumnValue.ParseMultiColumnValue(fieldValue);
            int i = 0;
            while (i < list.Count)
            {
                string s = list[i];
                string lookupValue = (i < list.Count - 1) ? list[i + 1] : string.Empty;
                int lookupId = 0;
                try
                {
                    lookupId = int.Parse(s, CultureInfo.InvariantCulture);
                    AveFieldLookupValue sPFieldLookupValue = new AveFieldLookupValue(lookupId, lookupValue);
                    bool flag;
                    if (!dictionary.TryGetValue(sPFieldLookupValue.LookupId, out flag))
                    {
                        dictionary.Add(sPFieldLookupValue.LookupId, true);
                        this.Add(sPFieldLookupValue);                     
                    }
                }
                catch (FormatException e)
                {
                    logger.Error($"error occured when AveFieldLookupValueCollection,error :{e}");
                }                                                        

                i += 2;
            }
        }

        #region IList<IAveFieldLookupValue> Members
        public IAveFieldLookupValue this[int index] 
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

        public int IndexOf(IAveFieldLookupValue item)
        {
            return mListData.IndexOf(item);
        }

        public void Insert(int index, IAveFieldLookupValue item)
        {
            mListData.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mListData.RemoveAt(index);
        }
        #endregion

        #region ICollection<IAveFieldLookupValue> Members
        public bool IsReadOnly 
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsReadOnly");
            }
        }

        public void Add(IAveFieldLookupValue item)
        {
            mListData.Add(item);
        }
        public void Clear()
        {
            mListData.Clear();
        }

        public bool Contains(IAveFieldLookupValue item)
        {
            return mListData.Contains(item);
        }

        public void CopyTo(IAveFieldLookupValue[] array, int arrayIndex)
        {
            mListData.CopyTo(array, arrayIndex);
        }

        public bool Remove(IAveFieldLookupValue item)
        {
            return mListData.Remove(item);
        }

        public override string ToString()
        {
            List<string> columns = new List<string>();
            foreach (AveFieldLookupValue value2 in this)
            {
                columns.Add(value2.LookupId.ToString());
                columns.Add(value2.LookupValue);
            }
            return AveSPCommonUtility.ConvertMultiColumnValueToString(columns, false, false);
        }

        #endregion
    }
}
