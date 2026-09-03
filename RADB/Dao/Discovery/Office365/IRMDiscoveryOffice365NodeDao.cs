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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365NodeDao
    {
        Task AddOrUpdateDiscoveryContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerInfo[] containers);

        Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryOffice365ContainerInfo[] containers);

        Task<(bool has, RMDiscoveryOffice365ContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(Guid o365TenantId, Guid opusId);
        Task<List<string>> GetContainerNamesByIds(Guid o365TenantId, IEnumerable<int> ids);

        Task<List<RMDiscoveryOffice365ContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId);

        Task<List<RMDiscoveryOffice365ContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId, SourceFlag contentSource);

        Task<List<RMDiscoveryOffice365ContainerInfo>> GetDiscoveryContainerInfoesAsync(Guid o365TenantId, IEnumerable<int> containerIds);

        IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, params int[] containerIds);

        IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, SourceFlag contentSource, params int[] containerIds);

        Task<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, Guid siteId);

        Task<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, int id);

        Task<int> CountDiscoverySiteAsync(Guid o365TenantId);

        Task<int> CountDiscoverySiteAsync(List<Guid> o365TenantIds);

        Task AddOrUpdateDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInfo[] sites);

        Task DeleteDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInfo[] sites);

        Task<RMRemoteNode> GetOpusContainerById(Guid id);

        Task<int> CountOpusContainersAsync(params SourceFlag[] contentSources);

        Task<int> CountOpusSitesAsync(params SourceFlag[] contentSources);

        Task<int> CountOpusSitesAsync(IEnumerable<Guid> containerIds);

        Task<List<RMRemoteNode>> GetOpusContainersAsync(params SourceFlag[] contentSources);

        Task<List<RMRemoteNode>> GetOpusContainersAsync(IEnumerable<Guid> ids);

        IAsyncEnumerable<RMRemoteNode> GetOpusSitesAsync(Guid containerId);

        Task<List<Guid>> GetOpusO365TenantIdsByContainerAsync(List<NodeLevel> supportNodeLevels, params Guid[] containerIds);

        Task<List<RMRemoteNode>> GetOpusTopSitesAsync(int top, params Guid[] containerIds);

        Task<List<RMRemoteNode>> GetOpusTopSitesAsync(int top, params SourceFlag[] contentSources);

        Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosByContainerIds(Guid o365TenantId, IEnumerable<int> containerIds);

        Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<long> siteIds);
        Task<List<string>> GetSiteUrlBySiteIds(Guid o365TenantId, IEnumerable<int> siteIds);
        Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosBySiteUrl(Guid o365TenantId, IEnumerable<string> siteUrls);
        Task<int> DeleteSiteDataBySiteIdAsync(Guid o365TenantId, Guid siteId);
    }
}
