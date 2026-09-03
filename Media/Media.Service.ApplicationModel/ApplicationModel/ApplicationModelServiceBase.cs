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




namespace AvePoint.Media.Service
{
    #region using directives
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.DB.Dao;
    #endregion

    public abstract class ApplicationModelServiceBase
    {
        public ICacheService CacheManager { get; set; }
        public IStorageDeviceManager StorageDeviceManager { get; set; }
        public IIndexDeviceValidateManager ValidateManager { get; set; }
        public IIndexDatabaseSynchronizer IndexSynchronizer { get; set; }
        public IDatabaseNameGenerator DatabaseNameGenerator { get; set; }
        public IControlStubService ControlStubs { get; set; }
    }
}