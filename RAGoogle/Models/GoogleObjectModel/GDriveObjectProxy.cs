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
using RAGoogle.Services;
using System.Collections;
using System.Collections.Concurrent;

namespace RAGoogle.Models.GoogleObjectModel
{
    public class GDriveObjectProxy : IDisposable
    {
        public ConcurrentDictionary<string, object> DataCache { get; set; }
        protected GoogleDriveService driveSerivce { get; set; }

        public GDriveObjectProxy(IDictionary<string, object> properties)
        {
            if (properties.IsNotNullOrEmpty())
            {
                AddPropertyies(properties);
            }
        }
        public GDriveObjectProxy(GoogleDriveService driveSerivce, IDictionary<string, object> properties = default) : this(properties)
        {
            this.driveSerivce = driveSerivce;
        }

        public ConcurrentDictionary<string, object> PropertiesCache
        {
            get
            {
                if (DataCache == null)
                {
                    DataCache = new ConcurrentDictionary<string, object>(4, 256);
                }
                return DataCache;
            }
            set { DataCache = value; }
        }
        public void AddPropertyies(IDictionary<string, object> properties)
        {
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kv in properties)
                {
                    PropertiesCache.TryAdd(kv.Key, kv.Value);
                }
            }
        }
        public void AddProperty(string key, object value)
        {
            PropertiesCache.TryAdd(key, value);
        }
        public void ChangeProperty(string key, object value)
        {
            PropertiesCache[key] = value;
        }
        public T GetProperty<T>(string key)
        {
            T tValue;
            TryGetProperty(key, out tValue);
            return tValue;
        }

        public bool TryGetProperty<T>(string key, out T value)
        {
            object tValue;
            if (TryGetOriginalProperty(key, out tValue))
            {
                value = (T)tValue;
                return true;
            }
            value = default;
            return false;
        }
        protected bool TryGetOriginalProperty(string key, out object value)
        {
            object tValue;
            if (PropertiesCache.TryGetValue(key, out tValue))
            {
                value = tValue;
                return true;
            }
            value = default;
            return false;
        }
        public void Dispose()
        {
            if (DataCache != null)
            {
                try
                {
                    foreach (var keyValue in DataCache)
                    {
                        var value = keyValue.Value;
                        if (value is List<IDictionary<string, object>> collectionValue)
                        {
                            lock (collectionValue)
                            {
                                foreach (var single in collectionValue)
                                {
                                    single.Clear();
                                }
                                collectionValue.Clear();
                            }
                        }
                        else if (value is GDriveObjectProxy gdObject && value is IEnumerable && value is ICollection)
                        {
                            gdObject.Dispose();
                        }
                    }
                }
                catch { }
                DataCache.Clear();
                DataCache = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
