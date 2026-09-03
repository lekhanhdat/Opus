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
    internal sealed class AveClientObjectData
    {
        private ThreadSafeDictionary<string, object> m_PropertiesCache;
        private ThreadSafeDictionary<string, object> m_ChangedProperties;
        private Dictionary<string, WeakReference> m_WeakReferenceCache;
        
        public AveClientObjectData()
        {
            //m_PropertiesCache = new Dictionary<string, object>();
            //m_MethodReturnCache = new Dictionary<string, object>();
            //m_ChangedProperties = new Dictionary<string, object>();
            //m_WeakReferenceCache = new Dictionary<string, WeakReference>();
        }

        public ThreadSafeDictionary<string, object> PropertiesCache
        {
            get
            {
                if (m_PropertiesCache == null)
                {
                    m_PropertiesCache = new ThreadSafeDictionary<string, object>();
                }
                return m_PropertiesCache;
            }
            set { m_PropertiesCache = value; }
        }

        public ThreadSafeDictionary<string, object> ChangedProperties
        {
            get
            {
                if (m_ChangedProperties == null)
                {
                    m_ChangedProperties = new ThreadSafeDictionary<string, object>();
                }
                return m_ChangedProperties;
            }
            set { m_ChangedProperties = value; }
        }

        public Dictionary<string, WeakReference> WeakReferenceCache
        {
            get
            {
                if (m_WeakReferenceCache == null)
                {
                    m_WeakReferenceCache = new Dictionary<string, WeakReference>();
                }
                return m_WeakReferenceCache;
            }
            set { m_WeakReferenceCache = value; }
        }

        public bool PropertyAvailable(string property)
        {
            return PropertiesCache.ContainsKey(property);
        }

        public void ResetChangedProperties()
        {
            ChangedProperties.Clear();
        }

        public void ResetProperties()
        {
            PropertiesCache.Clear();
        }

        public void AddPropertyies(Dictionary<string, object> properties)
        {
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kv in properties)
                {
                    PropertiesCache[kv.Key] = kv.Value;
                }
            }
        }

        public void AddProperty(string key, object value)
        {
            PropertiesCache[key] = value;
        }

        public void UpdateProperties(Dictionary<string, object> updateProperties)
        {
            if (updateProperties != null)
            {
                if (!updateProperties.ContainsKey(AveObjectModelConstant.ExceptionKey))
                {
                    this.AddPropertyies(updateProperties);
                }
                this.ResetChangedProperties();
            }
        }

        public void RefreshProperties(Dictionary<string, object> properties)
        {
            PropertiesCache.Clear();
            AddPropertyies(properties);
        }

        public void RemoveProperty(string propertyName)
        {
            if (PropertiesCache.ContainsKey(propertyName))
            {
                PropertiesCache.Remove(propertyName);
            }
        }

        public void AddChangedProperty(string key, object value)
        {
            ChangedProperties[key] = value;
        }

        public void AddChangedProperties(Dictionary<string, object> properties)
        {
            foreach (KeyValuePair<string, object> kv in properties)
            {
                ChangedProperties[kv.Key] = kv.Value;
            }
        }

        public T GetProperty<T>(string key)
        {
            if (ChangedProperties.ContainsKey(key))
            {
                return (T)ChangedProperties[key];
            }
            if (PropertiesCache.ContainsKey(key))
            {
                return (T)PropertiesCache[key];
            }
            else
            {
                return default(T);
            }
        }

        public List<Dictionary<string, object>> GetChildren()
        {
            List<Dictionary<string, object>> children = GetProperty<List<Dictionary<string, object>>>(AveObjectModelConstant.ChildrenProperties);
            if (children == null)
            {
                children = new List<Dictionary<string, object>>();
            }
            return children;
        }

        public bool IsPropertyNotLoaded(string propertyName)
        {
            return !PropertiesCache.ContainsKey(propertyName);
        }

        public bool IsPropertyAvailable(string propertyName)
        {
            return PropertiesCache.ContainsKey(propertyName);
        }

        public void AddWeakReferenceHandler(string key, object handler)
        {
            if (WeakReferenceCache.ContainsKey(key))
            {
                WeakReferenceCache[key].Target = handler;
            }
            else
            {
                WeakReferenceCache.Add(key, new WeakReference(handler, false));
            }
        }

        public object GetWeakReferenceObject(string key)
        {
            if (WeakReferenceCache.ContainsKey(key))
            {
                return WeakReferenceCache[key].Target;
            }
            return null;
        }

        public bool TryGetValueFromWeakReferenceObject(string key, out object value)
        {
            value = null;
            WeakReference weakObject;
            if (WeakReferenceCache.TryGetValue(key, out weakObject))
            {
                value = !weakObject.IsAlive ? null : weakObject.Target;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (PropertiesCache != null)
            {
                PropertiesCache.Clear();
                PropertiesCache = null;
            }
            //if (MethodReturnCache != null)
            //{
            //    MethodReturnCache.Clear();
            //    MethodReturnCache = null;
            //}
            if (ChangedProperties != null)
            {
                ChangedProperties.Clear();
                ChangedProperties = null;
            }
            if (WeakReferenceCache != null)
            {
                WeakReferenceCache.Clear();
                WeakReferenceCache = null;
            }
        }
    }
}
