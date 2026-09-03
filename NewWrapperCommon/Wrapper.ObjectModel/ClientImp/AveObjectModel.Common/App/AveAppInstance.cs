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
using System.IO;

namespace AvePoint.ObjectModel.Common
{
    class AveAppInstance : AveClientObject, IAveAppInstance
    {
        private AveSite mSite;
        private IAveRequest mRequest;
        private IAveApp mApp;

        public AveAppInstance(AveSite site, Dictionary<string, object> appProperties)
        {
            mSite = site;
            mRequest = mSite.Request as IAveRequest;
            base.DataCache.AddPropertyies(appProperties);
        }

        public Guid Install()
        {
            throw new NotImplementedException();
        }

        public Guid Uninstall()
        {
            return mRequest.UninstallAppByInstanceId(WebId, Id, App.ProductId, true);
        }

        public void Upgrade(Stream appPackageStream)
        {
            throw new NotImplementedException();
        }

        public void Upgrade(Stream appPackageStream, IAveWeb web, int SPAppSource)
        {
            throw new NotImplementedException();
        }

        public IAveApp App
        {
            get
            {
                if (mApp == null)
                {
                    Dictionary<string, object> appProperties = base.DataCache.GetProperty<Dictionary<string, object>>("App");
                    mApp = new AveApp(mSite, appProperties);
                }
                return mApp;
            }
        }

        public string AppPrincipalId
        {
            get { return base.DataCache.GetProperty<string>("AppPrincipalId"); }
        }

        public Uri AppWebFullUrl
        {
            get { return base.DataCache.GetProperty<Uri>("AppWebFullUrl"); }
        }

        public Guid Id
        {
            get { return base.DataCache.GetProperty<Guid>("Id"); }
        }

        public Uri LaunchUrl
        {
            get { return base.DataCache.GetProperty<Uri>("LaunchUrl"); }
        }

        public Guid SiteId
        {
            get { return base.DataCache.GetProperty<Guid>("SiteId"); }
        }

        public AveAppInstanceStatus Status
        {
            get { return base.DataCache.GetProperty<AveAppInstanceStatus>("Status"); }
        }

        public string Title
        {
            get { return base.DataCache.GetProperty<string>("Title"); }
        }

        public Guid WebId
        {
            get { return base.DataCache.GetProperty<Guid>("WebId"); }
        }
    }
}
