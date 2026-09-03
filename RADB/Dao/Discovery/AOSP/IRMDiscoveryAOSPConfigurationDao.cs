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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.AOSP
{
    public interface IRMDiscoveryAOSPConfigurationDao
    {
        Task<T> GetAsync<T>(RMDiscoveryConfigurationType type);

        Task<T> GetByO365TenantIdAsync<T>(RMDiscoveryConfigurationType type, string O365TenantId);

        Task<T> GetByO365TenantIdAsync<T>(RMDiscoveryConfigurationType type, string O365TenantId, T defaultValue);

        Task<T> GetAsync<T>(RMDiscoveryConfigurationType type, T defaultValue);

        Task<List<RMDiscoveryAOSPConfiguration>> GetAsync(params RMDiscoveryConfigurationType[] types);

        Task<List<RMDiscoveryAOSPConfiguration>> GetByO365TenantIdAsync(string O365TenantId, params RMDiscoveryConfigurationType[] types);

        Task AddOrUpdateAsync(params RMDiscoveryAOSPConfiguration[] configurations);

        Task AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryAOSPConfiguration[] configurations);

        Task DeleteByO365TenantIdAsync(RMDiscoveryDBEFContext efContext, string O365TenantId);

        Task DeleteByO365TenantIdAndTypeAsync(string o365TenantId, RMDiscoveryConfigurationType type);

        Task<bool> ExistAsync(RMDiscoveryConfigurationType type);

        Task<bool> ExistByO365TenantIdAsync(RMDiscoveryConfigurationType type, string O365TenantId);

        Task<int> UpdateDiscoveryConfigurationAsync(RMDiscoveryAOSPConfiguration configInfo);
    }
}
