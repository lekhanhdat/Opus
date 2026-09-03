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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.DeleteArchivedData.Cache;
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.SecurityChecker
{
    public class RMDeleteArchivedDataO365SecurityChecker
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataO365SecurityChecker));

        private readonly RMDeleteArchivedDataSiteCacheManager _siteCacheManager;

        private readonly HashSet<int> _existsItems = [];

        private readonly HashSet<int> _nonExistentItems = [];

        private readonly HashSet<int> _nonExisttentSitesInOpus = [];

        public RMDeleteArchivedDataO365SecurityChecker(RMDeleteArchivedDataSiteCacheManager siteCacheManager)
        {
            _siteCacheManager = siteCacheManager;
        }

        public bool CheckIfItemExistsInRestorePath(ArchiverBasicIndex archivedItem, RMRestoredItem restoredItem)
        {
            try
            {
                restoredItem.RestoredUrl = restoredItem.RestoredUrl.Replace("\\", "/");
                var restoredSiteUrlHashCode = restoredItem.RestoredSiteUrl.GetHashCode();

                if (_nonExisttentSitesInOpus.Contains(restoredSiteUrlHashCode))
                {
                    _logger.Error($"The site [{archivedItem.SitePath}] item [{archivedItem.Id}] not found in target site [{restoredItem.RestoredSiteUrl}] in Opus.");
                    return false;
                }

                var targetItemPath = restoredItem.RestoredUrl;
                if (targetItemPath.LastIndexOf(":") != 5)
                {
                    targetItemPath = targetItemPath.Substring(0, targetItemPath.LastIndexOf(":"));
                }
                var targetItemPathHashCode = targetItemPath.GetHashCode();
                if (_existsItems.Contains(targetItemPathHashCode))
                {
                    return true;
                }

                if (_nonExistentItems.Contains(targetItemPathHashCode))
                {
                    _logger.Error($"The site [{archivedItem.SitePath}] item [{archivedItem.Id}] not found in target site [{restoredItem.RestoredSiteUrl}].");
                    return false;
                }

                if(!_siteCacheManager.TryGetSite(restoredItem.RestoredSiteUrl, out var siteRecordPair))
                {
                    _logger.Error($"The site [{restoredItem.RestoredSiteUrl}] not found in Opus.");
                    _nonExisttentSitesInOpus.Add(restoredSiteUrlHashCode);
                    return false;
                }

                var web = siteRecordPair.site.RootWeb;
                if(!string.IsNullOrWhiteSpace(restoredItem.WebId))
                {
                    web = siteRecordPair.site.OpenWeb(new Guid(restoredItem.WebId));
                }

                var fileInfo = web.GetFile(targetItemPath);
                if (!fileInfo.Exists)
                {
                    _nonExistentItems.Add(targetItemPathHashCode);
                    _logger.Error($"The site [{archivedItem.SitePath}] item [{archivedItem.Id}] non existent in target site [{restoredItem.RestoredSiteUrl}].");
                    return false;
                }

                _existsItems.Add(targetItemPathHashCode);

                web.Dispose();

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while check site [{archivedItem.SitePath}] item [{archivedItem.Id}] in target site [{restoredItem.RestoredSiteUrl}]. Error: {e}");
                return false;
            }

        }
    }
}
