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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.DeleteArchivedData;
using AvePoint.RA.Service.Services.DeleteArchivedData.Archived;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataReportManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataReportManager));
        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private readonly IArchiverIndexSubInfoDao _archiveIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private readonly IRMArchiveSiteInfoDao archiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private readonly IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private readonly IRMSODashboardMonthlySnapshotDao _monthlySnapshotDao = PlatformWindsorManager.GetService<IRMSODashboardMonthlySnapshotDao>();
        private readonly RMArchivedIndexDBOperator _archivedIndexDBOperator;
        private readonly RestoredSitesInfo _restoredSiteInfo;

        public RMDeleteArchivedDataReportManager(RestoredSitesInfo restoredSiteInfo, RMArchivedIndexDBOperator archivedIndexDBOperator)
        {
            _restoredSiteInfo = restoredSiteInfo;
            _archivedIndexDBOperator = archivedIndexDBOperator;
        }

        public void Calculate()
        {
            try
            {
                var fileCount = _archivedIndexDBOperator.GetFileCount();
                var versionCount = _archivedIndexDBOperator.GetFileVersionCount();
                var siteUrlAndJobIdMapping = _archiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(new List<string>() { _restoredSiteInfo.SiteUrl });
                var siteUrlAndSizeMapping = _archiveIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
                var totalSize = siteUrlAndSizeMapping[_restoredSiteInfo.SiteUrl]; // double, GB
                var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(_restoredSiteInfo.SiteUrl)?.TenantId;

                double previousSizeGB = archiveSiteInfoDao
                    .GetSiteInfoesBySiteUrls(new List<string>() { _restoredSiteInfo.SiteUrl })
                    ?.FirstOrDefault()?.ArchivedSize ?? 0;
                double destroyedSizeGB = previousSizeGB > totalSize ? previousSizeGB - totalSize : 0;
                //_logger.Info($"DestroyedFromArchive: site [{_restoredSiteInfo.SiteUrl}], " +
                //    $"previousSize [{previousSizeGB}] GB, totalSize [{totalSize}] GB, " +
                //    $"destroyedSize [{destroyedSizeGB}] GB.");

                archiveSiteInfoDao.UpdateArchiverInfo(_restoredSiteInfo.SiteUrl, fileCount, versionCount, o365TenantId, totalSize);
                _logger.Info($"Succeed update site [{_restoredSiteInfo.SiteUrl}] report. " +
                    $"File count [{fileCount}], Version count [{versionCount}], Total size [{totalSize}].");

                if (destroyedSizeGB > 0 && !string.IsNullOrWhiteSpace(o365TenantId))
                {
                    var period = DateTime.UtcNow.ToString("yyyyMM");
                    bool isOneDrive = _restoredSiteInfo.SiteUrl.Contains("-my.sharepoint.com");
                    long destroyedSizeBytes = (long)(destroyedSizeGB * 1024 * 1024 * 1024);

                    //_logger.Info($"UpsertMonthlySnapshot SpoDestroyedFromArchiveSize. " +
                    //    $"TenantId:[{o365TenantId}], Period:[{period}], " +
                    //    $"IsOneDrive:[{isOneDrive}], DestroyedSizeBytes:[{destroyedSizeBytes}].");

                    _monthlySnapshotDao.UpsertMonthlySnapshotAsync(
                        o365TenantId, period,
                        spoArchivedSize: 0,
                        odArchivedSize: 0,
                        spoDestroyedFromArchiveSize: isOneDrive ? 0 : destroyedSizeBytes,
                        odDestroyedFromArchiveSize: isOneDrive ? destroyedSizeBytes : 0,
                        spoDestroyedFromLiveSize: 0,
                        odDestroyedFromLiveSize: 0
                    ).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate site [{_restoredSiteInfo.SiteUrl}] report. Error: {e}");
            }
        }
    }
}