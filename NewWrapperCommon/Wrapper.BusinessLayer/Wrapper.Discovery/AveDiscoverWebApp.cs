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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverWebApp : IAveDiscoverWebApp, IDisposable
    {
        public IAveWebApplication WebApplication { get; private set; }
        public AveDiscoverWebApp(IAveWebApplication webApp)
        {
            WebApplication = webApp;
        }

        public Dictionary<Guid, AveSiteObject> GetDeleteSites(DateTime startTime,DateTime endTime)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWebApp.GetDeleteSites"))
            {
                Dictionary<Guid, AveSiteObject> deletedSites = new Dictionary<Guid, AveSiteObject>();
                var factory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Auto);
                foreach (var dataBase in WebApplication.ContentDatabases)
                {
                    var query = factory.CreateQueryService<IAveDiscoverQueryService>(dataBase.DatabaseConnectionString);
                    query.GetDeleteSites(deletedSites, startTime, endTime);
                }
                return deletedSites;
            }
        }

        public void Dispose()
        {
        }

    }
}
