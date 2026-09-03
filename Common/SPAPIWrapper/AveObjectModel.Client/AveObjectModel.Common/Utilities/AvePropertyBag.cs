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
using System.Collections.Specialized;

namespace AvePoint.ObjectModel.Common
{
    class AvePropertyBag : AveClientObject, IAvePropertyBag
    {
        private IAveWeb mWeb;
        private IAveAlert mAlert;
        private IAveRequest mRequest;
        private string mPropertyBagSource;

        public AvePropertyBag()
        {
        }

        public AvePropertyBag(IAveWeb web,IAveRequest request, Dictionary<string, object> Properties) : base(true)
        {
            mWeb = web;
            mRequest = request;
            mPropertyBagSource = "web.properties";            
            base.DataCache.AddPropertyies(Properties);
        }

        public AvePropertyBag(IAveAlert alert, IAveRequest request, Dictionary<string, object> Properties) : base(true)
        {
            mAlert = alert;
            mRequest = request;
            mPropertyBagSource = "alert.properties";            
            base.DataCache.AddPropertyies(Properties);
        }

        public void Update()
        {
            if ((mAlert != null || mWeb != null) && base.DataCache.ChangedProperties.Count > 0)
            {
                if (mAlert != null)
                {
                    (mAlert as AveAlert).DataCache.ChangedProperties["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.ChangedProperties;
                }
                else
                {
                    (mWeb as AveWeb).DataCache.ChangedProperties["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.ChangedProperties;
                    //由于client api中没有properties属性，所以无法update properties对应的属性。而properties和allproperties一致，所以用allproperties还原相应的属性
                    Dictionary<string, object> changedProperties = base.DataCache.ChangedProperties["ChangeProperties"] as Dictionary<string, object>;
                    System.Collections.Hashtable allProperties = (mWeb as AveWeb).AllProperties;
                    foreach (string key in changedProperties.Keys)
                    {
                        allProperties[key] = changedProperties[key];
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return base.DataCache.PropertyCount;
            }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ICollection Keys
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ICollection Values
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string key]
        {
            get
            {
                return base.DataCache.GetProperty<string>(key);
            }
            set
            {
                if (!base.DataCache.ChangedProperties.ContainsKey("ChangeProperties"))
                {
                    Dictionary<string, object> changeProperties = new Dictionary<string, object>();
                    base.DataCache.ChangedProperties.Add("ChangeProperties", changeProperties);
                }
                base.DataCache.AddProperty(key,value);
                (base.DataCache.ChangedProperties["ChangeProperties"] as Dictionary<string, object>)[key] = value;
            }
        }

        public void Add(string key, string value)
        {
            this[key] = value;
        }

        public void Clear()
        {
            base.DataCache.ResetProperties();
            base.DataCache.ResetChangedProperties();
            base.DataCache.AddChangedProperty("ClearProperties",true);
        }

        public bool ContainsKey(string key)
        {
            return base.DataCache.IsPropertyAvailable(key);
        }

        public bool ContainsValue(string value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }


        #region IEnumerable Members

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
