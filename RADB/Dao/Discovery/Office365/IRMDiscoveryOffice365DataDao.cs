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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365DataDao
    {
        Task AddSiteInactiveDataAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInactiveData[] dataList);

        Task AddSiteRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteRotData[] dataList);

        Task AddOrUpdateContainerInactiveDataAsync(Guid o365TenantId, int containerId, params RMDiscoveryOffice365ContainerInactiveData[] dataList);

        Task<List<RMDiscoveryOffice365ContainerInactiveData>> GetContainerInactiveDataListAsync(Guid o365Tenant, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365Tenant, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicInactiveDataAsync(Guid o365TenantId, SourceFlag contentSource, params RMDiscoveryOffice365BasicInactiveData[] dataList);

        Task<List<RMDiscoveryOffice365BasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365Tenant, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateContainerRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRotData[] dataList);

        Task AddOrUpdateContainerRotDataAsync(Guid o365TenantId, int containerId, params RMDiscoveryOffice365ContainerRotData[] dataList);

        Task<List<RMDiscoveryOffice365ContainerRotData>> GetContainerRotDataListAsync(Guid o365TenantId, int containerId);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerRotData> GetContainerRotDataListAsync(Guid o365TenantId);

        Task AddOrUpdateBasicRotDataAsync(Guid o365TenantId, SourceFlag contentSource, params RMDiscoveryOffice365BasicRotData[] dataList);

        Task AddBasicRotDataAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRotData[] dataList);

        Task<List<RMDiscoveryOffice365BasicRotData>> GetBasicRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        Task<RMDiscoveryOffice365AggregateTotalData> GetAggregateTotalDataAsync(Guid o365TenantId, SourceFlag contentSource);

        Task<List<RMDiscoveryOffice365AggregateTotalData>> GetAggregateTotalDataListAsync(Guid o365TenantId);

        Task AddOrUpdateAggregateTotalDataAsync(Guid o365TenantId, RMDiscoveryOffice365AggregateTotalData data);

        Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365AggregateTotalData data);

        Task<List<RMDiscoveryOffice365SiteRotData>> GetSiteRotDataListAsync(Guid o365TenantId, int siteId);

        Task<List<RMDiscoveryOffice365SiteInactiveData>> GetSiteInactiveDataListAsync(Guid o365TenantId, int siteId, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryOffice365SiteRotData> GetSiteRotDataListByContainerAsync(Guid o365TenantId, int containerId);

        IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataListByContainerAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteSiteRotDataListAsync(Guid o365TenantId, int siteId);

        Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId);

        Task DeleteSitesDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules);

        Task DeleteContainersDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules);

        Task DeleteBasicDuplicateDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365RuleInfo[] duplicateRules);
    }
}
