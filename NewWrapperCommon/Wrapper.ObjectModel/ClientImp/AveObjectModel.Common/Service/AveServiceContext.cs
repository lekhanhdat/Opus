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
using AvePoint.ObjectModel.Common.Office;

namespace AvePoint.ObjectModel.Common
{
    class AveServiceContext : IAveServiceContext
    {
        private AveSiteSubscriptionIdentifier mSiteSubscriptionId;
        private AveServiceApplicationProxyGroup mServiceAppProxyGroup;
        private IAveRequest mRequest;
        private IAveSite mSite;

        public AveServiceContext()
        { }
        public AveServiceContext(AveServiceApplicationProxyGroup serviceAppProxyGroup, AveSiteSubscriptionIdentifier siteSubscriptonId)
        {
            mServiceAppProxyGroup = serviceAppProxyGroup;
            mSiteSubscriptionId = siteSubscriptonId;
        }

        public AveServiceContext( IAveSite site )
        {
            if (site.WebApplication != null)
            {
                mServiceAppProxyGroup = site.WebApplication.ServiceApplicationProxyGroup as AveServiceApplicationProxyGroup;
                mSiteSubscriptionId = new AveSiteSubscriptionIdentifier(Guid.Empty);
            }
            mSite = site;
            mRequest = (mSite as AveSite).Request;
        }

        #region IAveServiceContext Members

        public IAveServiceContext GetContext(IAveSite site)
        {
            if (site.WebApplication != null)
            {
                return new AveServiceContext(
                    site.WebApplication.ServiceApplicationProxyGroup as AveServiceApplicationProxyGroup,
                    (new AveSiteSubscriptionIdentifier(Guid.Empty)) as AveSiteSubscriptionIdentifier
                    );
            }
            return new AveServiceContext(site);
        }

        public IAveServiceContext GetContext(IAveServiceApplicationProxyGroup serviceApplicationProxyGroup, IAveSiteSubscriptionIdentifier siteSubscriptionId)
        {
            return new AveServiceContext(
                serviceApplicationProxyGroup as AveServiceApplicationProxyGroup, 
                siteSubscriptionId as AveSiteSubscriptionIdentifier
                );
        }

        #endregion


        public IAveServiceApplicationProxy GetDefaultProxy(Type serviceApplicationProxyType)
        {
            switch (serviceApplicationProxyType.Name)
            {
                case "IAveOSearchServiceApplicationProxy":
                    return new AveOSearchServiceApplicationProxy(mSite);
                default:
                    return null;
            }
        }

        public IAveServiceContext Current
        {
            get { throw new NotImplementedException(); }
        }

        public IAveSiteSubscriptionIdentifier SiteSubscriptionId
        {
            get { throw new NotImplementedException(); }
        }

        public IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }
    }
}
