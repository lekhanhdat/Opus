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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.AOSP
{
    public interface IRMDiscoveryAOSPNodeDao
    {

        Task<List<RMRemoteNode>> GetAOSContainersAsync(string o365TenantId, params SourceFlag[] contentSources);
        Task<List<RMRemoteNode>> GetAOSContainersForAOSPAsync(string o365TenantId, params SourceFlag[] contentSources);

        Task<List<RMRemoteNode>> GetAOSSitesAsync(string o365TenantId, string containerId, int nodeLevel);

        Task<int> CountAOSContainersAsync(string o365TenantId, params SourceFlag[] contentSources);

        Task<int> CountAOSSitesAsync(string o365TenantId, params SourceFlag[] contentSources);

        Task<(bool has, RMDiscoveryAOSPContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(Guid o365TenantId, Guid opusId);

        Task<RMRemoteNode> GetOpusContainerById(Guid id);

        Task AddOrUpdateDiscoveryContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerInfo[] containers);

        Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryAOSPContainerInfo[] containers);

        Task<List<RMDiscoveryAOSPContainerInfo>> GetDiscoveryContainersAsync(Guid o365TenantId, IEnumerable<int> ids);

        Task<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, Guid siteId);

        Task<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, int id);

        Task AddOrUpdateDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryAOSPSiteInfo[] sites);

        Task<List<RMDiscoveryAOSPContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId);

        Task<List<RMDiscoveryAOSPContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId, SourceFlag contentSource);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, params int[] containerIds);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, SourceFlag contentSource, params int[] containerIds);

        Task<List<RMDiscoveryAOSPSiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<long> siteIds);

        Task<List<RMDiscoveryAOSPSiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<Guid> siteUniqueIds);
    }
}
