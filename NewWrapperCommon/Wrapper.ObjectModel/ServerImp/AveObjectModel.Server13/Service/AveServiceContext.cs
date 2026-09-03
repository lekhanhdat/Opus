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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;

namespace AvePoint.ObjectModel.Server13
{
    class AveServiceContext : IAveServiceContext
    {
        private SPServiceContext mServiceContext;
        private AveServiceContext mCurrent;
        private AveSiteSubscriptionIdentifier mSiteSubscriptionId;

        public AveServiceContext()
        { }

        internal AveServiceContext(SPServiceContext serviceContext)
        {
            mServiceContext = serviceContext;
        }

        internal SPServiceContext ServiceContext
        {
            get
            {
                return mServiceContext;
            }
        }

        #region IAveServiceContext Members

        public IAveServiceContext GetContext(IAveSite site)
        {
            SPServiceContext serviceContext = SPServiceContext.GetContext((site as AveSite).Site);
            if (serviceContext == null)
            {
                return null;
            }
            return new AveServiceContext(serviceContext);
        }

        public IAveServiceContext GetContext(IAveServiceApplicationProxyGroup serviceApplicationProxyGroup, IAveSiteSubscriptionIdentifier siteSubscriptionId)
        {
            SPServiceApplicationProxyGroup serviceAppProxyGroup = (serviceApplicationProxyGroup as AveServiceApplicationProxyGroup).ServiceApplicationProxyGroup;
            if (serviceAppProxyGroup == null)
            {
                return null;
            }
            return new AveServiceContext(SPServiceContext.GetContext(serviceAppProxyGroup, (siteSubscriptionId as AveSiteSubscriptionIdentifier).SiteSubscriptionIdentifier));
        }

        public IAveServiceApplicationProxy GetDefaultProxy(Type serviceApplicationProxyType)
        {
            if (serviceApplicationProxyType == null)
            {
                throw new ArgumentNullException("serviceApplicationProxyType");
            }
            string typeMapping = string.Empty;
            typeMapping = XmlConfiguration.GetTypeMapping(serviceApplicationProxyType.Name);
            Type spServiceApplicationProxyType = AveAssemblyUtility.GetGenerticType(serviceApplicationProxyType, typeMapping);
            if (spServiceApplicationProxyType == null)
            {
                return null;
            }
            SPServiceApplicationProxy defaultProxy = mServiceContext.GetDefaultProxy(spServiceApplicationProxyType);
            if (defaultProxy != null)
            {
                return (IAveServiceApplicationProxy)AveServerAssemblyInit.CreateElement(typeof(IAveServiceApplicationProxy), defaultProxy);
            }
            return null;
        }

        public IAveServiceContext Current
        {
            get
            {
                if (mCurrent == null)
                {
                    SPServiceContext serviceContext = SPServiceContext.Current;
                    if (serviceContext != null)
                    {
                        mCurrent = new AveServiceContext(serviceContext);
                    }
                }
                return mCurrent;
            }
        }

        public IAveSiteSubscriptionIdentifier SiteSubscriptionId
        {
            get
            {
                if (mSiteSubscriptionId == null)
                {
                    mSiteSubscriptionId = new AveSiteSubscriptionIdentifier(mServiceContext.SiteSubscriptionId);
                }
                return mSiteSubscriptionId;
            }
        }

        #endregion
    }
}
