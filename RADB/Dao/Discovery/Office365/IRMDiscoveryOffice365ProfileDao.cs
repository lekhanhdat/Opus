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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365ProfileDao
    {
        Task InitBuildInDataAsync(Guid o365TenantId);

        Task<List<RMDiscoveryOffice365ProfileInfo>> GetProfileInfoesAsync(Guid o365TenantId);

        Task<RMDiscoveryOffice365ProfileInfo> GetProfileInfoByIdAsync(Guid o365TenantId, Guid profileId);

        Task<List<RMDiscoveryOffice365ProfileInfo>> GetProfileInfoesAsync(Guid o365TenantId, RMDiscoveryProfileType type);

        Task AddOrUpdateProfileInfoAsync(Guid o365TenantId, RMDiscoveryOffice365ProfileInfo profileInfo);

        Task DeleteProfileInfoAsync(Guid o365TenantId, Guid id);

        Task<List<RMDiscoveryProfileFailedInfo>> GetProfileFailedInfoesAsync(Guid o365TenantId, Guid profileId);

        Task AddOrUpdateProfileFailedInfoesAsync(Guid o365TenantId, params RMDiscoveryProfileFailedInfo[] failedInfoes);

        Task DeleteProfileFailedInfoesAsync(Guid o365TenantId, Guid profileId);
    }
}
