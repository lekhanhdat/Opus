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



using System.Collections.Generic;

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDHoldItems : IList<EDHoldItem>
    {
        private List<EDHoldItem> _edHoldItems;

        public EDHoldItems()
        {
            _edHoldItems = new List<EDHoldItem>();
        }

        public int IndexOf(EDHoldItem item)
        {
            return _edHoldItems.IndexOf(item);
        }

        public void Insert(int index, EDHoldItem item)
        {
            _edHoldItems.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _edHoldItems.RemoveAt(index);
        }

        public EDHoldItem this[int index]
        {
            get { return _edHoldItems[index]; }
            set { _edHoldItems[index] = value; }
        }

        public void Add(EDHoldItem item)
        {
            _edHoldItems.Add(item);
        }

        public void Clear()
        {
            _edHoldItems.Clear();
        }

        public bool Contains(EDHoldItem item)
        {
            return _edHoldItems.Contains(item);
        }

        public void CopyTo(EDHoldItem[] array, int arrayIndex)
        {
            _edHoldItems.CopyTo(array,arrayIndex);
        }

        public int Count
        {
            get { return _edHoldItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(EDHoldItem item)
        {
            return _edHoldItems.Remove(item);
        }

        public IEnumerator<EDHoldItem> GetEnumerator()
        {
            return _edHoldItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _edHoldItems.GetEnumerator();
        }
    }
}
