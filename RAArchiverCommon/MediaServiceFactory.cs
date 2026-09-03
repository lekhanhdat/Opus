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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media;
using AvePoint.GCommon.Contract.Media.WcfService;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Archiver.Media
{
    public class MediaServiceFactory
    {

        public static MediaServer CreateMediaServer()
        {
            return new MediaServer()
            {
                MediaServerVersion = RecordsEnv.ProductVersion.ToString(),
            };
        }

        public static CommonConfigInfo CreateCommonConfigInfo()
        {
            return (CommonConfigInfo)PlatformWindsorManager.GetService(typeof(CommonConfigInfo));
        }

        public static ArchiverConfigInfo CreateArchiverConfigInfo()
        {
            return (ArchiverConfigInfo)PlatformWindsorManager.GetService(typeof(ArchiverConfigInfo));
        }

        public static IMergeIndexService CreateArchiverMergeIndexService()
        {
            return PlatformWindsorManager.GetService<IMergeIndexService>();
        }

        public static IArchiverBackupDataWriter CreateArchiverBackupDataWriter()
        {
            return PlatformWindsorManager.GetService<IArchiverBackupDataWriter>();
        }
        
        public static IMoveIndexService CreateArchiverMoveIndexService()
        {
            //return _container.Resolve<IMoveIndexService>("AvePoint.Media.Service.Remoting.ArchiverMoveIndexService");
            return PlatformWindsorManager.GetService<IMoveIndexService>();
        }
        public static IArchiverRestoreService CreateArchiverRestoreService()
        {
            //return _container.Resolve<IMoveIndexService>("AvePoint.Media.Service.Remoting.ArchiverMoveIndexService");
            return PlatformWindsorManager.GetService<IArchiverRestoreService>();
        }
        public static IArchiverRestoreToStorageService CreateArchiverRestoreToStorageService()
        {
            //return _container.Resolve<IMoveIndexService>("AvePoint.Media.Service.Remoting.ArchiverMoveIndexService");
            return PlatformWindsorManager.GetService<IArchiverRestoreToStorageService>();
        }
        public static IEndUserArchiverRestoreService CreateEndUserArchiverRestoreService()
        {
            //return _container.Resolve<IMoveIndexService>("AvePoint.Media.Service.Remoting.ArchiverMoveIndexService");
            return PlatformWindsorManager.GetService<IEndUserArchiverRestoreService>();
        }
        public static IRetentionService CreateLifecycleRetentionService()
        {
            //return _container.Resolve<IRetentionService>("AvePoint.Media.Service.ArchiverBackup.ArchiverLifecycleRetentionService");
            return PlatformWindsorManager.GetService<IRetentionService>();
        }
        public static IRebuildStubService CreateArchiverRebuildStubService()
        {
            return PlatformWindsorManager.GetService<IRebuildStubService>();
        }
    }
}
