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
    class AveAppCatalog : IAveAppCatalog
    {
        private IAveRequest mRequest;

        public AveAppCatalog(IAveRequest request)
        {
            mRequest = request;
        }

        public IAveAppInstance GetAppInstance(IAveWeb web, IAveApp app)
        {
            throw new NotImplementedException();
        }

        public IAveAppInstance GetAppInstance(IAveWeb web, Guid appInstanceId)
        {
            return web.GetAppInstanceById(appInstanceId);
        }

        public IAveAppInstance GetAppInstanceForAppWeb(IAveWeb web)
        {
            throw new NotImplementedException();
        }

        public IList<IAveAppInstance> GetAppInstances(IAveWeb web)
        {
            IList<IAveAppInstance> apps = new List<IAveAppInstance>();
            Dictionary<string, object> appsProperties = mRequest.GetApps(web.ServerRelativeUrl);
            IList<Dictionary<string, object>> appListProperties = appsProperties[AveObjectModelConstant.ChildrenProperties] as IList<Dictionary<string, object>>;
            foreach (Dictionary<string, object> appProperty in appListProperties)
            {
                apps.Add(new AveAppInstance(web.Site as AveSite, appProperty));
            }
            return apps;
        }

        public IList<IAveAppInstance> GetAppInstancesByProductId(IAveWeb web, Guid productId)
        {
            return web.GetAppInstancesByProductId(productId);
        }
    }
}
