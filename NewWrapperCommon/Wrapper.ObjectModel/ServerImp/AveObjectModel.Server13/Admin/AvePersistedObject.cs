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
using System.Linq;
using System.Globalization;
using Microsoft.SharePoint.Administration;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    public class AvePersistedObject : AveAutoSerializingObject, IAvePersistedObject, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
       
        static AvePersistedObject()
        {
            mAssembly = Assembly.GetCallingAssembly();
        }

        protected static Assembly mAssembly;
        private const string mAveTypeNamePro = "AvePoint.ObjectModel.Server13.Ave";
        protected SPPersistedObject mPersistedObject;
        private AveConfigurationDatabase mConfigurationDatabase;
        private AvePersistedObject mParent;
        private AveFarm mFarm;
        private Guid mId;
        private long mVersion;
        private Guid mParentId;
        private Hashtable mUpgradedPersistedFields;
        private string mName;
        private Hashtable mProperties;
        private AvePersistedStoreProvider mPersistedStoreProvider;
        private AveLastUpdateInfo mLastUpdateInfo;

        public AvePersistedObject(SPPersistedObject persistedObject)
            : base(persistedObject)
        {
            mPersistedObject = persistedObject;
        }

        public AvePersistedObject(object persistedObject)
            : this((SPPersistedObject)persistedObject)
        { }

        public AvePersistedObject(string name, IAvePersistedObject parent)
            : this(name, parent, Guid.NewGuid())
        { }

        public AvePersistedObject(string name, IAvePersistedObject parent, Guid id)
        {
            mPersistedObject = (parent as AvePersistedObject).PersistedObject;
            mVersion = -1;
            mId = Guid.Empty;
            mParentId = Guid.Empty;
            mUpgradedPersistedFields = new Hashtable(StringComparer.Create(CultureInfo.InvariantCulture, true));
            InitializeCore((parent as AvePersistedObject).PersistedStoreProvider, id, parent.ID, name);
        }

        public AvePersistedObject()
            : this(new SPPersistedObject())
        { }

        internal SPPersistedObject PersistedObject
        {
            get
            {
                return mPersistedObject;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords")]
        public IAveConfigurationDatabase ConfigurationDatabase
        {
            get
            {
                if (mConfigurationDatabase == null)
                {
                    object configurationDatabase = AveAssemblyUtility.GetPropertyValue(mPersistedObject, "ConfigurationDatabase");
                    if (configurationDatabase != null)
                    {
                        mConfigurationDatabase = new AveConfigurationDatabase(configurationDatabase);
                    }
                }
                return mConfigurationDatabase;
            }
        }

        public virtual string DisplayName
        {
            get
            {
                return mPersistedObject.DisplayName;
            }
        }

        public IAveFarm Farm
        {
            get
            {
                if (mFarm == null)
                {
                    SPFarm farm = mPersistedObject.Farm;
                    if (farm != null)
                    {
                        mFarm = new AveFarm(farm);
                    }
                }
                return mFarm;
            }
        }

        public Guid ID
        {
            get
            {
                if (mId.Equals(new Guid()))
                {
                    mId = mPersistedObject.Id;
                }
                return mId;
            }
            set
            {
                mPersistedObject.Id = mId = value;
            }
        }

        public string Name
        {
            get
            {
                if (mName == null)
                {
                    mName = mPersistedObject.Name;
                }
                return mName;
            }
            set
            {
                mPersistedObject.Name = mName = value;
            }
        }

        public IAvePersistedObject Parent
        {
            get
            {
                if (mParent == null)
                {
                    SPPersistedObject parent = mPersistedObject.Parent;
                    if (parent != null)
                    {
                        mParent = (AvePersistedObject)CreateElementInstance(parent);
                    }
                }
                return mParent;
            }
        }

        public string TypeName
        {
            get
            {
                return mPersistedObject.TypeName;
            }
        }

        public IAvePersistedStoreProvider PersistedStoreProvider
        {
            get
            {
                if (mPersistedStoreProvider == null)
                {
                    mPersistedStoreProvider = new AvePersistedStoreProvider(mPersistedObject);
                }
                return mPersistedStoreProvider;
            }
            set
            {
                mPersistedStoreProvider = value as AvePersistedStoreProvider;
                AveAssemblyUtility.SetPropertyValue(mPersistedObject, "PersistedStoreProvider", (value as AvePersistedStoreProvider).PersistedStoreProvider);
            }
        }

        public AveObjectStatus Status
        {
            get
            {
                return (AveObjectStatus)mPersistedObject.Status;
            }
            set
            {
                mPersistedObject.Status = (SPObjectStatus)value;
            }
        }

        public Hashtable Properties
        {
            get
            {
                if (mProperties == null)
                {
                    mProperties = mPersistedObject.Properties;
                }
                return mProperties;
            }
        }

        public bool WasCreated
        {
            get
            {
                return Convert.ToBoolean(AveAssemblyUtility.GetPropertyValue(mPersistedObject, "WasCreated"));
            }
        }

        public virtual void Provision()
        {
            mPersistedObject.Provision();
        }

        public virtual void Unprovision()
        {
            mPersistedObject.Unprovision();
        }

        public virtual void Update(bool ensure)
        {
            mPersistedObject.Update(ensure);
        }

        public virtual void Update()
        {
            try
            {
                mPersistedObject.Update();
            }
            catch (SPUpdatedConcurrencyException ex)
            {
                throw new AveUpdatedConcurrencyException(ex.Message, ex);
            }
        }

        public virtual void Delete()
        {
            mPersistedObject.Delete();
        }

        private void InitializeCore(IAvePersistedStoreProvider persistedStoreProvider, Guid id, Guid parentId, string name)
        {
            mPersistedStoreProvider = (persistedStoreProvider as AvePersistedStoreProvider);
            mId = id;
            mParentId = parentId;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            mName = name.Trim();
        }

        public long Version
        {
            get { return mPersistedObject.Version; }
        }

        internal virtual object CreateElementInstance(object persistedObject)
        {
            Type instanceType = GetAveType(persistedObject.GetType().FullName);
            if (instanceType == null)
            {
                //if we can not get type from Ave assemby, that means we did not implement this type, then we will get it parent type from T;
                instanceType = mAssembly.GetType(mAveTypeNamePro + typeof(IAvePersistedObject).Name.Substring(4));
            }
            object retObj = new object();
            try
            {
                retObj = Activator.CreateInstance(instanceType, new object[] { (SPPersistedObject)persistedObject });
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.CreateElementInstanceError, e.ToString());
            }
            return (AvePersistedObject)retObj;
        }

        internal Type GetAveType(string typeFullName)
        {
            Type retType = null; ;
            Type retOType = null;
            string[] typeNames = typeFullName.Split('.');
            string typeName = typeNames.Last<string>();
            if (typeName.IndexOf("SP", StringComparison.OrdinalIgnoreCase) == 0)
            {
                typeName = typeName.Substring(2);
                retType = mAssembly.GetType(mAveTypeNamePro + typeName);
            }
            else
            {
                if (typeNames[1].Equals("Office"))
                {
                    retOType = mAssembly.GetType(mAveTypeNamePro + "O" + typeName);
                    retType = mAssembly.GetType(mAveTypeNamePro + typeName);
                    if (retOType != null && retType != null)
                    {
                        retType = retOType;
                    }
                    else
                    {
                        retType = null;
                    }
                }
                else
                {
                    retType = mAssembly.GetType(mAveTypeNamePro + typeName);
                }
            }
            return retType;
        }

        public void Dispose()
        {
            if (mConfigurationDatabase != null)
            {
                mConfigurationDatabase.Dispose();
                mConfigurationDatabase = null;
            }
        }

        public void Uncache()
        {
            mPersistedObject.Uncache();
        }

        public IAveLastUpdateInfo LastUpdateInfo
        {
            get
            {
                if (mLastUpdateInfo == null)
                {
                    mLastUpdateInfo = new AveLastUpdateInfo(AveAssemblyUtility.GetPropertyValue(mPersistedObject, "LastUpdateInfo"));
                }
                return mLastUpdateInfo;
            }
            set
            {
                mLastUpdateInfo = value as AveLastUpdateInfo;
                AveAssemblyUtility.SetPropertyValue(mPersistedObject, "LastUpdateInfo", mLastUpdateInfo.LastUpdateInfo);
            }
        }
    }
}
