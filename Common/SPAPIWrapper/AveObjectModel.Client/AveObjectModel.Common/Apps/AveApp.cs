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
    class AveApp : AveClientObject, IAveApp
    {
        private AveSite mSite;
        private IAveRequest mRequest;

        public AveApp(AveSite site, Dictionary<string, object> appProperties)
        {
            mSite = site;
            mRequest = site.Request as IAveRequest;
            base.DataCache.AddPropertyies(appProperties);
        }

        public Guid CreateAppInstance(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetPackage()
        {
            return null;
        }

        public Guid ProductId
        {
            get { return base.DataCache.GetProperty<Guid>("ProductId"); }
        }

        public Guid SiteId
        {
            get { return mSite.Id; }
        }

        public string VersionString
        {
            get { return base.DataCache.GetProperty<string>("VersionString"); }
        }

        public AveAppSource Source
        {
            get { return base.DataCache.GetProperty<AveAppSource>("Source"); }
        }

        public bool IsUpdateAvailable
        {
            get { return false; }
        }
    }
}
