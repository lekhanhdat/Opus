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

using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.Api.Contract;

namespace AvePoint.RA.Api.Services.Services
{
    public class TenantInfoService: ITenantInfoService
    {
        private ITenantInfoDao mTenantInfoDao { get; set; }
        public ITenantInfoDao TenantInfoDao
        {
            get
            {
                if (mTenantInfoDao == null)
                {
                    mTenantInfoDao = AvePoint.RA.Common.PlatformWindsorManager.GetService(typeof(ITenantInfoDao)) as ITenantInfoDao;
                }
                return mTenantInfoDao;
            }
        }

        public bool CheckTenantLicenseIsAvailable(string customerId)
        {
            return TenantInfoDao.CheckTenantIsAvailable(customerId);
        }

        public bool CheckLicenseWithAdditionalDataSource(string customerId, RMAdditionalDataSource mAdditionalDataSource)
        {
            return TenantInfoDao.CheckTenantIsAvailable(customerId) && TenantInfoDao.CheckAdditionalDataSource(customerId, (long)mAdditionalDataSource);
        }

        public bool AdditionalDataSourceEnable(string customerId)
        {
            return TenantInfoDao.EnableAdditionalDataSource(customerId);
        }

        public List<string> GetAllAvailableTenantInfo()
        {
            return TenantInfoDao.GetAllAvailableTenantInfo().Select(t => t.TenantId).ToList();
        }
    }
}
