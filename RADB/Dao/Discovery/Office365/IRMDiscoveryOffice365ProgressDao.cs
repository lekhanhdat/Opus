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
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365ProgressDao
    {
        Task AddOrUpdateSiteOptimizedInfoAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteOptimizedInfo[] dataList);

        Task AddOrUpdateContainerOptimizedInfoAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerOptimizedInfo[] dataList);

        Task<RMDiscoveryOffice365SiteOptimizedInfo> GetSiteOptimizedInfoAsync(Guid o365TenantId, long siteId);

        Task<RMDiscoveryOffice365ContainerOptimizedInfo> GetContainerOptimizedInfoAsync(Guid o365TenantId, int containerId);

        Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfoAsync(Guid o365TenantId);

        Task<List<RMDiscoveryProgressContainerOptimizedInfo>> GetContainerOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo);

        Task<int> CountContainerOptimizedAsync(Guid o365TenantId);

        Task<List<RMDiscoveryProgressSiteOptimizedInfo>> GetSiteOptimizedInfoesAsync(RMDiscoveryProgressPaginateInfo paginateInfo);

        Task<int> CountSiteOptimizedAsync(Guid o365TenantId);
    }
}
