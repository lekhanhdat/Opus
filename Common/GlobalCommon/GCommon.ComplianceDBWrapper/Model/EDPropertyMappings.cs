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
using System.Collections;
using System.Collections.Generic;

namespace AvePoint.GCommon.ComplianceDBWrapper.Model
{
    public class EDPropertyMappings : IList<EDPropertyMapping>
    {
        private List<EDPropertyMapping> _mappings;

        public EDPropertyMappings()
        {
            _mappings = new List<EDPropertyMapping>();
        }

        #region - List 扩展方法 -

        public IEnumerator<EDPropertyMapping> GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _mappings.GetEnumerator();
        }

        public void Add(EDPropertyMapping item)
        {
            _mappings.Add(item);
        }

        public void Clear()
        {
            _mappings.Clear();
        }

        public bool Contains(EDPropertyMapping item)
        {
           return _mappings.Contains(item);
        }

        public void CopyTo(EDPropertyMapping[] array, int arrayIndex)
        {
            _mappings.CopyTo(array,arrayIndex);
        }

        public bool Remove(EDPropertyMapping item)
        {
            return _mappings.Remove(item);
        }

        public int Count
        {
            get { return _mappings.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(EDPropertyMapping item)
        {
            return _mappings.IndexOf(item);
        }

        public void Insert(int index, EDPropertyMapping item)
        {
            _mappings.Insert(index,item);
        }

        public void RemoveAt(int index)
        {
            _mappings.RemoveAt(index);
        }

        public EDPropertyMapping this[int index]
        {
            get { return _mappings[index]; }
            set { _mappings[index] = value; }
        }

        #endregion
    }
}
