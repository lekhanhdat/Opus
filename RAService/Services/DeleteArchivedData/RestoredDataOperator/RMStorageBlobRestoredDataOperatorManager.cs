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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.RestoredDataOperator
{
    public class RMStorageBlobRestoredDataOperatorManager
    {

        private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private const string STORAGE_CONTAINER_NAME = "opus-sqlite-database-container";

        private readonly BlobContainerClient _containerClient;

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly RMDeleteArchivedDataSettingManager _settingManager;

        public RMStorageBlobRestoredDataOperatorManager(
            RestoredSitesInfo restoredSiteInfo, 
            RMDeleteArchivedDataSettingManager settingManager)
        {
            _containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
            _restoredSiteInfo = restoredSiteInfo;
            _settingManager = settingManager;
        }

        public IEnumerable<RMStorageBlobRestoredDataOperator> GetOperators()
        {
            var volumeGenerator = new ArchiverVolumeGenerator();
            var siteVolume = volumeGenerator.GenerateSitePath(_restoredSiteInfo.SiteUrl);
            var prefix = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId.ToString().ToLower(), "delete_archived_data", siteVolume).Replace("\\", "/") + "/";
            var asyncPageable = _containerClient.GetBlobs(default, default, prefix: prefix, default);
            foreach(var blobPage in asyncPageable.AsPages(pageSizeHint: 10))
            {
                foreach(var blobItem in blobPage.Values)
                {
                    yield return new RMStorageBlobRestoredDataOperator(_containerClient, blobItem, _settingManager);
                }
            }
        }
    }
}
