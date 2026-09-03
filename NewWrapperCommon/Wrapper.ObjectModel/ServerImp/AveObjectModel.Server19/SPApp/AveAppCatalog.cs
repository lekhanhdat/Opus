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
using System.Threading.Tasks;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server19
{
    public class AveAppCatalog : IAveAppCatalog
    {
        #region Fields
        
        #endregion

        #region Methods

        public AveAppCatalog()
        {

        }

        public IAveAppInstance GetAppInstance(IAveWeb web, IAveApp app)
        {
            return new AveAppInstance(SPAppCatalog.GetAppInstance((web as AveWeb).Web, (app as AveApp).App));
        }

        public IAveAppInstance GetAppInstance(IAveWeb web, Guid appInstanceId)
        {
            return new AveAppInstance(SPAppCatalog.GetAppInstance((web as AveWeb).Web, appInstanceId));
        }

        public IAveAppInstance GetAppInstanceForAppWeb(IAveWeb web)
        {
            return new AveAppInstance(SPAppCatalog.GetAppInstanceForAppWeb((web as AveWeb).Web));
        }

        public IList<IAveAppInstance> GetAppInstances(IAveWeb web)
        {
            List<IAveAppInstance> list = new List<IAveAppInstance>();
            IList<SPAppInstance> instances = SPAppCatalog.GetAppInstances((web as AveWeb).Web);

            foreach (SPAppInstance instance in instances)
            {
                list.Add(new AveAppInstance(instance));
            }
            return list;
        }

        public IList<IAveAppInstance> GetAppInstancesByProductId(IAveWeb web, Guid productId)
        {
            List<IAveAppInstance> list = new List<IAveAppInstance>();
            IList<SPAppInstance> instances = SPAppCatalog.GetAppInstancesByProductId((web as AveWeb).Web, productId);

            foreach (SPAppInstance instance in instances)
            {
                list.Add(AveServerAssemblyInit.CreateElement(typeof(IAveAppInstance), instance) as AveAppInstance);
            }
            return list;
        }
        #endregion

    }
}
