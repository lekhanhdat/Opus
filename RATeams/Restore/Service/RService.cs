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
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;

namespace M365GroupTeam
{
    public static class RService
    {
        private static IStorageDeviceService _storageDeviceService;
        public static IStorageDeviceService StorageDeviceService
        {
            get
            {
                if (_storageDeviceService == null)
                {
                    _storageDeviceService = PlatformWindsorManager.GetService(ref _storageDeviceService);
                }
                return _storageDeviceService;
            }
        }

        public static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static IStorageDeviceManager _storageDeviceManager;
        public static IStorageDeviceManager StorageDeviceManager
        {
            get
            {
                if (_storageDeviceManager == null)
                {
                    _storageDeviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
                }
                return _storageDeviceManager;
            }
        }

        private static ICacheService _ICacheService;
        public static ICacheService CacheService
        {
            get
            {
                if (_ICacheService == null)
                {
                    _ICacheService = PlatformWindsorManager.GetService<ICacheService>();
                }
                return _ICacheService;
            }
        }

        private static ICommonSiteMasterIndexService _commonSiteMasterIndexService;
        public static ICommonSiteMasterIndexService CommonSiteMasterIndexService
        {
            get
            {
                if (_storageDeviceService == null)
                {
                    _commonSiteMasterIndexService = PlatformWindsorManager.GetService(ref _commonSiteMasterIndexService);
                }
                return _commonSiteMasterIndexService;
            }
        }

        private static IArchiverSiteMasterIndexDao _archiverSiteInfoIndexDao;
        public static IArchiverSiteMasterIndexDao ArchiverSiteInfoIndexDao
        {
            get
            {
                if(_archiverSiteInfoIndexDao == null)
                {
                    _archiverSiteInfoIndexDao = PlatformWindsorManager.GetService(ref _archiverSiteInfoIndexDao);
                }
                return _archiverSiteInfoIndexDao;
            }
        }

    }
}
