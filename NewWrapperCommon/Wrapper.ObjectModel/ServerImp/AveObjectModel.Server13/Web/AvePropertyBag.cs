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
using Microsoft.SharePoint.Utilities;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AvePropertyBag : IAvePropertyBag
    {
        private SPPropertyBag mPropertyBag;

        public AvePropertyBag(SPPropertyBag propertyBag)
        {
            mPropertyBag = propertyBag;
        }

        #region IAvePropertyBag Members

        public void Update()
        {
            mPropertyBag.Update();
        }

        public int Count
        {
            get { return mPropertyBag.Count; }
        }

        public bool IsSynchronized
        {
            get { return mPropertyBag.IsSynchronized; }
        }

        public System.Collections.ICollection Keys
        {
            get { return mPropertyBag.Keys; }
        }

        public object SyncRoot
        {
            get { return mPropertyBag.SyncRoot; }
        }

        public System.Collections.ICollection Values
        {
            get { return mPropertyBag.Values; }
        }

        public string this[string key]
        {
            get
            {
                return mPropertyBag[key];
            }
            set
            {
                mPropertyBag[key] = value;
            }
        }

        public void Add(string key, string value)
        {
            mPropertyBag.Add(key, value);
        }

        public void Clear()
        {
            mPropertyBag.Clear();
        }

        public bool ContainsKey(string key)
        {
            return mPropertyBag.ContainsKey(key);
        }

        public bool ContainsValue(string value)
        {
            return mPropertyBag.ContainsValue(value);
        }

        public void CopyTo(Array array, int index)
        {
            mPropertyBag.CopyTo(array, index);
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return mPropertyBag.GetEnumerator();
        }

        public void Remove(string key)
        {
            mPropertyBag.Remove(key);
        }

        #endregion
    }
}
