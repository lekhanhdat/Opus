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
using AvePoint.RA.DB.Model.Discovery.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365ProfileDataDao
    {
        Task DeleteSiteInactiveDataBySiteIdAsync(Guid o365TenantId, Guid profileId, int siteId);

        Task AddSiteInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileSiteInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryProfileSiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteContainerInactiveDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId);

        Task AddContainerInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileContainerInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryProfileContainerInactiveData> GetContainerInactiveDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteBasicInactiveDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource);

        Task AddBasicInactiveDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileBasicInactiveData[] dataList);

        Task DeleteSiteRotDataBySiteIdAsync(Guid o365TenantId, Guid profileId, int siteId);

        Task AddSiteRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileSiteRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryProfileSiteRotData> GetSiteRotDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteContainerRotDataByContainerIdAsync(Guid o365TenantId, Guid profileId, int containerId);

        Task AddContainerRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileContainerRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryProfileContainerRotData> GetContainerRotDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteBasicRotDataByContentSourceAsync(Guid o365TenantId, Guid profileId, SourceFlag contentSource);

        Task AddBasicRotDataListAsync(Guid o365TenantId, Guid profileId, params RMDiscoveryProfileBasicRotData[] dataList);
    }
}
