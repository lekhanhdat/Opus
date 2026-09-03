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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server19
{
    class AveServiceApplicationProxyGroup : AvePersistedUpgradableObject, IAveServiceApplicationProxyGroup
    {
        private const string mServiceApplicationProxyGroup_Type = "Microsoft.SharePoint.Administration.SPServiceApplicationProxyGroup";
        private SPServiceApplicationProxyGroup mServiceApplicationProxyGroup;
        private AveServiceApplicationProxyGroup mDefault;
        private IEnumerable<IAveServiceApplicationProxy> mProxies;

        public AveServiceApplicationProxyGroup()
            : this(new SPServiceApplicationProxyGroup())
        { }

        public AveServiceApplicationProxyGroup(SPServiceApplicationProxyGroup serviceAppProxyGroup)
            : base(serviceAppProxyGroup)
        {
            mServiceApplicationProxyGroup = serviceAppProxyGroup;
        }

        public AveServiceApplicationProxyGroup(string name, IAveFarm farm)
            : this(new SPServiceApplicationProxyGroup(name, (farm as AveFarm).Farm))
        { }

        internal SPServiceApplicationProxyGroup ServiceApplicationProxyGroup
        {
            get
            {
                return mServiceApplicationProxyGroup;
            }
        }

        #region IAveServiceApplicationProxyGroup Members

        public IAveServiceApplicationProxyGroup Default
        {
            get
            {
                if (mDefault == null)
                {
                    SPServiceApplicationProxyGroup serviceApplicationProxyGroup = SPServiceApplicationProxyGroup.Default;
                    if (serviceApplicationProxyGroup != null)
                    {
                        mDefault = new AveServiceApplicationProxyGroup(serviceApplicationProxyGroup);
                    }
                }
                return mDefault;
            }
        }

        public bool IsCustom
        {
            get
            {
                return (Boolean)AveAssemblyUtility.GetPropertyValue(mServiceApplicationProxyGroup, "IsCustom");
            }
        }

        public IEnumerable<IAveServiceApplicationProxy> Proxies
        {
            get
            {
                foreach (var proxy in mServiceApplicationProxyGroup.Proxies)
                {
                    //yield return new AveServiceApplicationProxy(proxy);
                    yield return (IAveServiceApplicationProxy)AveServerAssemblyInit.CreateElement(typeof(IAveServiceApplicationProxy), proxy);
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                mServiceApplicationProxyGroup.Clear();
                foreach (IAveServiceApplicationProxy proxy in value)
                {
                    mServiceApplicationProxyGroup.Add((proxy as AveServiceApplicationProxy).ServiceApplicationProxy);
                }

            }
        }

        public IEnumerable<IAveServiceApplicationProxy> DefaultProxies
        {
            get
            {
                foreach (var proxy in mServiceApplicationProxyGroup.DefaultProxies)
                {
                    yield return (IAveServiceApplicationProxy)AveServerAssemblyInit.CreateElement(typeof(IAveServiceApplicationProxy), proxy);
                }
            }
        }

        public IAveServiceApplicationProxyGroup GetOrCreate(IAveFarm farm, string proxyGroupName, bool custom)
        {
            if (farm.Farm == null)
            {
                return null;
            }
            return new AveServiceApplicationProxyGroup(AveAssemblyUtility.InvokeStaticMethod(mServiceApplicationProxyGroup_Type, "GetOrCreate", new Type[] { typeof(SPFarm), typeof(string), typeof(bool) }, new object[] { SPServer.Local.Farm, proxyGroupName, custom }) as SPServiceApplicationProxyGroup);
        }

        public void Update()
        {
            mServiceApplicationProxyGroup.Update();
        }

        public void Add(IAveServiceApplicationProxy serviceApplicationProxy)
        {
            mServiceApplicationProxyGroup.Add((serviceApplicationProxy as AveServiceApplicationProxy).ServiceApplicationProxy);
        }

        public void Clear()
        {
            mServiceApplicationProxyGroup.Clear();
        }

        public bool ContainsType(Type serviceApplicationProxyType)
        {
            return mServiceApplicationProxyGroup.ContainsType(serviceApplicationProxyType);
        }

        public bool IsDefault(IAveServiceApplicationProxy proxy)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mServiceApplicationProxyGroup, "IsDefault", new Type[] { typeof(SPServiceApplicationProxy) }, new object[] { (proxy as AveServiceApplicationProxy).ServiceApplicationProxy });
            //var spServiceApplicationProxy = AveAssemblyUtility.GetFieldValue(proxy, proxy.GetType(), "mServiceApplicationProxy");
            //return (Boolean)AveAssemblyUtility.InvokeMethod(mServiceApplicationProxyGroup, "IsDefault", new object[] { spServiceApplicationProxy });
        }
        #endregion

        public void SetDefaultProxy(IAveServiceApplicationProxy serviceApplicationProxy)
        {
            AveAssemblyUtility.InvokeMethod(mServiceApplicationProxyGroup, "SetDefaultProxy", new Type[] { typeof(SPServiceApplicationProxy) }, new object[] { (serviceApplicationProxy as AveServiceApplicationProxy).ServiceApplicationProxy });
        }
    }
}
