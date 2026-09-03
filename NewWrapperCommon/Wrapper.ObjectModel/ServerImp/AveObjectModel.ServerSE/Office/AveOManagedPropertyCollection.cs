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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using System.Collections;

namespace AvePoint.ObjectModel.ServerSE.Office
{
    class AveOManagedPropertyCollection : AveAbstractCommonCollection, IAveOManagedPropertyCollection
    {
        private ManagedPropertyCollection mManagedPropertyCollection;
        private AveOSchemaDatabase mDatabase;

        public AveOManagedPropertyCollection(ManagedPropertyCollection managedPropertyCollection)
            :base(managedPropertyCollection)
        {
            mManagedPropertyCollection = managedPropertyCollection;
        }

        internal override object CreatElementInstance(object obj)
        {
            return new AveOManagedProperty(obj as ManagedProperty);
        }

        public IAveOManagedProperty this[string name]
        {
            get
            {
                if (mManagedPropertyCollection[name] != null)
                {
                    return new AveOManagedProperty(mManagedPropertyCollection[name]);
                }
                return null;
            }
        }

        public int Count
        {
            get
            {
                return mManagedPropertyCollection.Count;
            }
        }

        public IAveOSchemaDatabase Database
        {
            get
            {
                if (mDatabase == null)
                {
                    object schemaDataBase = AveAssemblyUtility.GetPropertyValue(mManagedPropertyCollection, "Database");
                    if (schemaDataBase != null)
                    {
                        mDatabase = new AveOSchemaDatabase(schemaDataBase);
                    }
                }
                return mDatabase;
            }
        }

        public IAveOManagedProperty this[int pid]
        {
            get
            {
                Dictionary<int, ManagedProperty> managedPropertiesIdDictionary = AveAssemblyUtility.GetFieldValue(mManagedPropertyCollection, "managedPropertiesIdDictionary") as Dictionary<int, ManagedProperty>;
                if (!managedPropertiesIdDictionary.ContainsKey(pid))
                {
                    throw new KeyNotFoundException(pid.ToString());
                }
                ManagedProperty managedProperty = managedPropertiesIdDictionary[pid];
                if (managedProperty != null)
                {
                    return new AveOManagedProperty(managedProperty);
                }
                return null;
            }
        }

        public IAveOSearchServiceApplication SearchApplication
        {
            get
            {
                SearchServiceApplication searchServiceApplication = AveAssemblyUtility.GetPropertyValue(mManagedPropertyCollection, "SearchApplication") as SearchServiceApplication;
                if (searchServiceApplication != null)
                {
                    return new AveOSearchServiceApplication(searchServiceApplication);
                }
                return null;
            }
        }

        public IAveOManagedProperty CreateWithPid(string name, AveManagedDataType managedType, int pid)
        {
            ManagedProperty mManagedProperty = AveAssemblyUtility.InvokeMethod(mManagedPropertyCollection, "CreateWithPid", new Type[] { typeof(string), typeof(ManagedDataType), typeof(int) }, new object[] { name, (ManagedDataType)managedType, pid }) as ManagedProperty;
            if (mManagedProperty != null)
            {
                return new AveOManagedProperty(mManagedProperty);
            }
            return null;
        }

        public void EnsurePopulated()
        {
            AveAssemblyUtility.InvokeMethod(mManagedPropertyCollection, "EnsurePopulated", new Type[] { }, new object[] { });
        }

        public Dictionary<string, IAveOManagedProperty>.Enumerator GetEnumeratorInternal()
        {
            this.EnsurePopulated();
            Dictionary<string, IAveOManagedProperty> managedPropertiesDictionary = null;
            Dictionary<string, ManagedProperty> spManagedPropertiesDictionary = AveAssemblyUtility.GetFieldValue(mManagedPropertyCollection, "managedPropertiesDictionary") as Dictionary<string, ManagedProperty>;
            if (spManagedPropertiesDictionary != null)
            {
                managedPropertiesDictionary = new Dictionary<string, IAveOManagedProperty>();
                foreach (string key in spManagedPropertiesDictionary.Keys)
                {
                    if (spManagedPropertiesDictionary[key] != null)
                    {
                        managedPropertiesDictionary.Add(key, new AveOManagedProperty(spManagedPropertiesDictionary[key]));
                    }
                    else
                    {
                        managedPropertiesDictionary.Add(key, null);
                    }
                }
            }
            return managedPropertiesDictionary.GetEnumerator();
        }

        public void Populate()
        {
            AveAssemblyUtility.InvokeMethod(mManagedPropertyCollection, "Populate", new Type[] { }, new object[] { });
        }

        public void Remove(IAveOManagedProperty managedProperty)
        {
            ManagedProperty spManagedProperty = null;
            if (managedProperty != null)
            {
                spManagedProperty = (managedProperty as AveOManagedProperty).ManagedProperty;
            }
            AveAssemblyUtility.InvokeMethod(mManagedPropertyCollection, "Remove", new Type[] { typeof(ManagedProperty) }, new object[] { spManagedProperty });
        }

        public bool Contains(string name)
        {
            return mManagedPropertyCollection.Contains(name);
        }

        public IAveOManagedProperty Create(string name, AveManagedDataType managedType)
        {
            return new AveOManagedProperty(mManagedPropertyCollection.Create(name, (ManagedDataType)managedType));
        }

        public IAveOManagedProperty CreateCrawlMonProperty()
        {
            return new AveOManagedProperty(mManagedPropertyCollection.CreateCrawlMonProperty());
        }
    }
}
