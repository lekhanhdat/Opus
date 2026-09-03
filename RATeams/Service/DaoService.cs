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
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M365GroupTeam
{
    public static class DaoService
    {
        private static ISettingProfilesDao _settingProfileDao;
        public static ISettingProfilesDao SettingProfileDao
        {
            get
            {
                if (_settingProfileDao == null)
                {
                    _settingProfileDao = PlatformWindsorManager.GetService<ISettingProfilesDao>();
                }
                return _settingProfileDao;
            }
        }


        private static IArchiverIndexSubInfoDao _archiverIndexSubInfoDao;
        public static IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao
        {
            get
            {
                if (_archiverIndexSubInfoDao == null)
                {
                    _archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
                }
                return _archiverIndexSubInfoDao;
            }
        }

        private static ICommonSiteMasterIndexDao _CommonSiteMasterIndexDao;
        public static ICommonSiteMasterIndexDao CommonSiteMasterIndexDao
        {
            get
            {
                if (_CommonSiteMasterIndexDao == null)
                {
                    _CommonSiteMasterIndexDao = PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
                }
                return _CommonSiteMasterIndexDao;
            }
        }

        private static IRMRemoteNodeDao _rmNodeDao;
        public static IRMRemoteNodeDao RMNodeDao
        {
            get
            {
                if (_rmNodeDao == null)
                {
                    _rmNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
                }
                return _rmNodeDao;
            }
        }

        private static IRMKeyValueDao _rmKeyValueDao;
        public static IRMKeyValueDao RMKeyValueDao
        {
            get
            {
                if (_rmKeyValueDao == null)
                {
                    _rmKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                }
                return _rmKeyValueDao;
            }
        }

        private static IEXOArchiverIndexSubInfoDao _exoArchiverIndexSubInfoDao;
        public static IEXOArchiverIndexSubInfoDao EXOArchiverIndexSubInfoDao
        {
            get
            {
                if (_exoArchiverIndexSubInfoDao == null)
                {
                    _exoArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
                }
                return _exoArchiverIndexSubInfoDao;
            }
        }

        private static IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao;
        public static IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao
        {
            get
            {
                if (_archiverSiteMasterIndexDao == null)
                {
                    _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
                }
                return _archiverSiteMasterIndexDao;
            }
        }

        private static IRMArchiveTeamsGroupInfoDao _archiveTeamsGroupInfoDao;
        public static IRMArchiveTeamsGroupInfoDao ArchiveTeamsGroupInfoDao
        {
            get
            {
                if (_archiveTeamsGroupInfoDao == null)
                {
                    _archiveTeamsGroupInfoDao = PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();
                }
                return _archiveTeamsGroupInfoDao;
            }
        }
    }
}
