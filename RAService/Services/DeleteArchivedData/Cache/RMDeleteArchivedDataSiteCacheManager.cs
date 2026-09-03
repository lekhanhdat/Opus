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
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Browser;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.Cache
{
    public class RMDeleteArchivedDataSiteCacheManager : IDisposable
    {
        private readonly Dictionary<string, (IAveSite site, IAveORecords record)> _siteCache = new();

        public bool TryGetSite(string siteUrl, out (IAveSite site, IAveORecords record) res)
        {
            res = (null, null);
            if (_siteCache.TryGetValue(siteUrl, out var value))
            {
                res = (value.site, value.record);
                return true;
            }

            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
            if (remoteSiteCollection == null)
            {
                return false;
            }

            var bposInfo = RA.RACommonUtility.CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
            var aveObjectModelFactory = RA.Common.Util.MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            var site = aveObjectModelFactory.CreateSite(siteUrl);
            var record = aveObjectModelFactory.CreateRecords();
            _siteCache[siteUrl] = (site, record);
            res = (site, record);
            return true;
        }

        public void Dispose()
        {
            foreach (var (site, record) in _siteCache.Values)
            {
                site.Dispose();
            }
        }
    }
}
