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
using AvePoint.Wrapper.Common.Office;
using Microsoft.Office.Server.Search.Administration;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19.Office
{
    class AveOSearchServiceInstance : AveWindowsServiceInstance, IAveOSearchServiceInstance
    {
        private SearchServiceInstance mSearchServiceInstance;

        public AveOSearchServiceInstance(SearchServiceInstance searchServiceInstance)
            : base(searchServiceInstance)
        {
            mSearchServiceInstance = searchServiceInstance;
        }

        public AveOSearchServiceInstance()
            : this(new SearchServiceInstance())
        { }

        public AveOSearchServiceInstance(IAveServer server, IAveWindowsService service)
            : this(new SearchServiceInstance((server as AveServer).Server, (service as AveWindowsService).windowsService))
        { }

        public AveOSearchServiceInstance(string name, IAveServer server, IAveService service)
            : this(new SearchServiceInstance(name, (server as AveServer).Server, (service as AveService).Service))
        { }

        public AveOSearchServiceInstance(string name, IAveServer server, IAveWindowsService service)
            : base(name, server, service)
        {
            mSearchServiceInstance = new SearchServiceInstance(name, (server as AveServer).Server, (service as AveWindowsService).windowsService);
        }

        public AveORole Role
        {
            get { return (AveORole)mSearchServiceInstance.Role; }
        }
    }
}
