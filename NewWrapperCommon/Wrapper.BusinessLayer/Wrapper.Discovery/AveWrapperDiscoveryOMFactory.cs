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
using AvePoint.Wrapper.Contract;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Discovery
{
    public class AveWrapperDiscoveryOMFactory : AveDiscoveryOMFactory
    {
        static AveDiscoverFilterUtility filterUtility = new AveDiscoverFilterUtility();

        public override IAveDiscoverFilterUtility FilterUtility
        {
            get
            {
                return filterUtility;
            }
        }
        public override IAveDiscoverWebApp CreateDiscoverWebApp(IAveWebApplication webApp)
        {
            return new AveDiscoverWebApp(webApp);
        }
        public override IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module)
        {
            return new AveDiscoverSite(aveSite, account, kind, module);
        }


        public override IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverSite(aveSite, module, kind, objectModelFactory);
        }

        public override IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
            return new AveDiscoverSite(aveSite, account, kind, module, startTime, endTime);
        }


        public override IAveDiscoverSite CreateDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, DateTime startTime, DateTime endTime)
        {
            return new AveDiscoverSite(aveSite, module, kind, objectModelFactory, startTime, endTime);
        }

        public override IAveDiscoverWeb CreateDiscoverWeb()
        {
            return new AveDiscoverWeb();
        }


        public override IAveDiscoverWeb CreateDiscoverWeb(IAveSite site, string webRelativeUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverWeb(site, webRelativeUrl, module,kind, objectModelFactory);
        }

        public override IAveDiscoverWeb CreateDiscoverWeb(IAveSite site, string webRelativeUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverWeb(site, webRelativeUrl, startTime, endTime, module, kind, objectModelFactory);
        }

        public override IAveDiscoverList CreateDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverList(site, webId, listRootFolderUrl, module, kind, objectModelFactory);
        }

        public override IAveDiscoverList CreateDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverList(site, web, listRootFolderUrl, module, kind, objectModelFactory);
        }

        public override IAveDiscoverList CreateDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverList(site, webId, listRootFolderUrl, startTime, endTime, module,kind, objectModelFactory);
        }

        public override IAveDiscoverList CreateDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            return new AveDiscoverList(site, web, listRootFolderUrl, startTime, endTime, module, kind, objectModelFactory);
        }

        public override IAveDiscoverFolder CreateDiscoverFolder()
        {
            return new AveDiscoverFolder();
        }
                
        public override IAveDiscoverFolder CreateDiscoverFolder(IAveSite site, Guid webId, string folderRelativeUrl, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, Guid listId = default(Guid), IAveWeb web = null)
        {
            return new AveDiscoverFolder(site, webId, folderRelativeUrl, module,kind, objectModelFactory, listId, web);
        }
    }
}
