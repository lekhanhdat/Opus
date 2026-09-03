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
using System.Collections.Concurrent;
using System.Collections;

namespace AvePoint.ObjectModel.Common
{
    internal class AveClientObjectData
    {
        protected IAveDictionary<string,object> m_PropertiesCache;
        private Dictionary<string, object> m_ChangedProperties;
        private IAveDictionary<string, WeakReference> m_WeakReferenceCache;
        private readonly Object lockObj = new object();

        public AveClientObjectData()
        {
        }

        public AveClientObjectData(bool ignoreCase)
        {
            if (ignoreCase)
            {
                m_PropertiesCache = new AveDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                m_ChangedProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        protected virtual IAveDictionary<string, object> PropertiesCache
        {
            get 
            {
                if (m_PropertiesCache == null)
                {
                    m_PropertiesCache = new AveDictionary<string, object> { };
                }
                return m_PropertiesCache; 
            }
            set { m_PropertiesCache = value; }
        }

        /// <summary>
        /// clone a property dictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetPropertyCache()
        {
            return (PropertiesCache as IAveDictionary<string,object>).Clone();
        }

        public int PropertyCount
        {
            get
            {
                return PropertiesCache.Count;
            }
        }

        public Dictionary<string, object> ChangedProperties
        {
            get 
            {
                if (m_ChangedProperties == null)
                {
                    m_ChangedProperties = new Dictionary<string, object>();
                }
                return m_ChangedProperties; 
            }
            set { m_ChangedProperties = value; }
        }

        protected virtual IAveDictionary<string, WeakReference> WeakReferenceCache
        {
            get
            {
                if (m_WeakReferenceCache == null)
                {
                    m_WeakReferenceCache = AveDictionaryFactory.CreateDefaultInstance<string, WeakReference>();
                }
                return m_WeakReferenceCache;
            }
            set { m_WeakReferenceCache = value; }
        }



        #region Property Cache Operation

        public virtual bool PropertyAvailable(string property)
        {
            return PropertiesCache.ContainsKey(property);
        }

        public virtual bool IsPropertyNotLoaded(string propertyName)
        {
            return !PropertiesCache.ContainsKey(propertyName);
        }

        public virtual bool IsPropertyAvailable(string propertyName)
        {
            return PropertiesCache.ContainsKey(propertyName);
        }



        /// <summary>
        /// 不影响内部dictionary
        /// </summary>
        /// <param name="properties"></param>
        public virtual void AddProperty(string key,object value)
        {
            PropertiesCache[key] = value;
        }

        public virtual T EnsureLoadProperty<T>(string propertyName, Func<T> loadFunction)
        {
            object value;
            if (!PropertiesCache.TryGetValue(propertyName, out value))
            {
                value = loadFunction();
                PropertiesCache[propertyName] = value;
            }
            return (T)value;
        }

        public virtual T GetPropertyWithoutChange<T>(string key)
        {
            object tValue;
            if (TryGetOriginalProperty(key, out tValue))
            {
                return (T)tValue;
            }
            throw new KeyNotFoundException(key);
        }

        public virtual object GetPropertyWithoutChange(string key)
        {
            object tValue;
            if (TryGetOriginalProperty(key, out tValue))
            {
                return tValue;
            }
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// 不影响内部dictionary
        /// </summary>
        /// <param name="properties"></param>
        public virtual void AddPropertyies(IDictionary<string, object> properties)
        {
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kv in properties)
                {
                    PropertiesCache[kv.Key] = kv.Value;
                }
            }
        }

        public virtual void RefreshProperties(Dictionary<string, object> properties)
        {
            ResetProperties();
            AddPropertyies(properties);
        }

        public virtual void RemoveProperty(string propertyName)
        {
            if (PropertiesCache.ContainsKey(propertyName))
            {
                PropertiesCache.Remove(propertyName);
            }
        }

        #endregion Property Cache Operation

        #region change cache operation

        public virtual void ResetChangedProperties()
        {
            ChangedProperties.Clear();
        }

        public virtual void ResetProperties()
        {
            PropertiesCache.Clear();
        }

        public virtual void AddChangedProperty(string key, object value)
        {
            ChangedProperties[key] = value;            
        }

        public virtual void AddChangedProperties(IDictionary<string, object> properties)
        {
            foreach (KeyValuePair<string, object> kv in properties)
            {
                ChangedProperties[kv.Key] = kv.Value;
            }
        }

        #endregion change cache operation

        #region mix cache operation

        public virtual T GetProperty<T>(string key)
        {
            T tValue;
            TryGetProperty(key, out tValue);
            return tValue;
        }

        public virtual bool TryGetProperty<T>(string key,out T value)
        {
            object tValue;
            if (TryGetChangedProperty<T>(key,out value))
            {
                return true;
            }
            if (TryGetOriginalProperty(key, out tValue))
            {
                value = (T)tValue;
                return true;
            }
            value = default(T);
            return false;
        }

        protected virtual bool TryGetOriginalProperty(string key, out object value)
        {
            object tValue;
            if (PropertiesCache.TryGetValue(key, out tValue))
            {
                value = tValue;
                return true;
            }
            value = tValue;
            return false;
        }

        public virtual bool TryGetChangedProperty<T>(string key, out T value)
        {
            object tValue;
            if (ChangedProperties.TryGetValue(key, out tValue))
            {
                value = (T)tValue;
                return true;
            }
            value = default(T);
            return false;
        }



        public virtual void UpdateProperties(Dictionary<string, object> updateProperties)
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

        public virtual List<IDictionary<string, object>> GetChildren()
        {
            List<IDictionary<string, object>> children = PropertiesCache.GetChildren();
            if (children == null)
            {
                children = ChangedProperties.GetChildren();
            }
            if (children == null)
            {
                children = new List<IDictionary<string, object>>();
            }
            return children;
        }

        #endregion mix cache operation




        #region Weak Reference Cache Operation

        public virtual void AddWeakReferenceHandler(string key, object handler)
        {
            lock (lockObj)
            {
                if (WeakReferenceCache.ContainsKey(key))
                {
                    WeakReferenceCache[key].Target = handler;
                }
                else
                {
                    WeakReferenceCache[key] = new WeakReference(handler, false);
                }
            }
        }

        public virtual void RemoveWeakReferenceHandler(string key)
        {
            if (WeakReferenceCache.ContainsKey(key))
            {
                WeakReferenceCache.Remove(key);
            }
        }

        public virtual object GetWeakReferenceObject(string key)
        {
            if (WeakReferenceCache.ContainsKey(key))
            {
                return WeakReferenceCache[key].Target;
            }
            return null;
        }

        #endregion Weak Reference Cache Operation

        public virtual void Dispose()
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

    internal sealed class AveClientConcurrentObjectData : AveClientObjectData
    {
        //public override IDictionary<string, object> PropertiesCache
        //{
        //    get
        //    {
        //        if (m_PropertiesCache == null)
        //        {
        //            m_PropertiesCache = new ConcurrentDictionary<string, object>();
        //        }
        //        return m_PropertiesCache;
        //    }
        //    set { m_PropertiesCache = value; }
        //}
    }

    internal sealed class AveClientThreadSafeObjectData : AveClientObjectData
    {
        //public override IDictionary<string, object> PropertiesCache
        //{
        //    get
        //    {
        //        if (m_PropertiesCache == null)
        //        {
        //            m_PropertiesCache = new AveDictionary<string, object>();
        //        }
        //        return m_PropertiesCache;
        //    }
        //    set { m_PropertiesCache = value; }
        //}
    }
}
