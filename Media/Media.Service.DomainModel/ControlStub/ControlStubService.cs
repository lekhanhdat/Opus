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



namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler;
    using AvePoint.GCommon.Contract.GranularBackup;
    using AvePoint.GCommon.Contract.GranularRestore;
    using AvePoint.GCommon.Contract.Media;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Service;
    using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore;
    using AvePoint.RA.Contract.Archiver;
    using AvePoint.RA.Common;
    using AvePoint.RA.DB.Dao;

    #endregion using directives

    internal class ControlStubService
        : IControlStubService
    {
        public IAJobDetailService JobDetailService { get; set; }

        public IMArchiverJobManagementService ArchiverBackupService { get; set; }
        public IMArchiverService ArchiverUpgradeServiceSiteMasterIndexService { get; set; }
        public IMArchiverSiteMasterIndexService ArchiverGeneralServiceSiteMasterIndexService { get; set; }

        //public IMAcceptMediaData ControlStorageService { get; set; }
        public IMediaDataDao ControlStorageService => PlatformWindsorManager.GetService<IMediaDataDao>();
        public IMServManageService ControlManageService { get; set; }
    }
}