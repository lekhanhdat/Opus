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



using System.Collections;
using System.Collections.Generic;

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDHeldDatas : IList<EDHeldData>
    {
        private List<EDHeldData> _edHeldDatas;

        public EDHeldDatas()
        {
            _edHeldDatas = new List<EDHeldData>();
        }

        #region - List扩展方法 -

        public IEnumerator<EDHeldData> GetEnumerator()
        {
            return _edHeldDatas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _edHeldDatas.GetEnumerator();
        }

        public void Add(EDHeldData item)
        {
            _edHeldDatas.Add(item);
        }

        public void Clear()
        {
            _edHeldDatas.Clear();
        }

        public bool Contains(EDHeldData item)
        {
            return _edHeldDatas.Contains(item);
        }

        public void CopyTo(EDHeldData[] array, int arrayIndex)
        {
            _edHeldDatas.CopyTo(array,arrayIndex);
        }

        public bool Remove(EDHeldData item)
        {
            return _edHeldDatas.Remove(item);
        }

        public int Count
        {
            get { return _edHeldDatas.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(EDHeldData item)
        {
            return _edHeldDatas.IndexOf(item);
        }

        public void Insert(int index, EDHeldData item)
        {
           _edHeldDatas.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            _edHeldDatas.RemoveAt(index);
        }

        public EDHeldData this[int index]
        {
            get { return _edHeldDatas[index]; }
            set { _edHeldDatas[index] = value; }
        }

        #endregion
    }
}
