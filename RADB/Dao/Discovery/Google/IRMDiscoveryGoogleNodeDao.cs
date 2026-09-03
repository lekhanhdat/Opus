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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Google
{
    public interface IRMDiscoveryGoogleNodeDao
    {
        Task AddOrUpdateDiscoveryContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerInfo[] containers);

        Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryGoogleContainerInfo[] containers);

        Task<(bool has, RMDiscoveryGoogleContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(string googleOrganizationId, Guid opusId);

        Task<List<RMDiscoveryGoogleContainerInfo>> GetAllDiscoveryGoogleContainersAsync(string googleOrganizationId);

        Task<List<RMDiscoveryGoogleContainerInfo>> GetDiscoveryGoogleContainerInfoesAsync(string googleOrganizationId, IEnumerable<int> containerIds);

        IAsyncEnumerable<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoesAsync(string googleOrganizationId, params int[] containerIds);

        Task<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoAsync(string googleOrganizationId, string driveId);

        Task<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoAsync(string googleOrganizationId, int id);

        Task<int> CountDiscoveryGoogleDriveAsync(string googleOrganizationId);

        Task AddOrUpdateDiscoveryGoogleDriveAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInfo[] drives);

        Task DeleteDiscoveryGoogleDrivesAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInfo[] drives);

        Task<RMRemoteNode> GetOpusGoogleContainerById(Guid id);

        Task<int> CountOpusGoogleContainersAsync();

        Task<int> CountOpusGoogleDrivesAsync();

        Task<int> CountOpusGoogleDrivesAsync(IEnumerable<Guid> containerIds);

        Task<List<RMRemoteNode>> GetOpusGoogleContainersAsync();

        Task<List<RMRemoteNode>> GetOpusGoogleContainersAsync(IEnumerable<Guid> ids);

        IAsyncEnumerable<RMRemoteNode> GetOpusGoogleDrivesAsync(Guid containerId);

        Task<List<string>> GetOpusGoogleTenantIdsByContainerAsync(List<NodeLevel> supportNodeLevels, params Guid[] containerIds);

        Task<List<RMRemoteNode>> GetOpusTopGoogleDrviesAsync(int top, params Guid[] containerIds);

        Task<List<RMRemoteNode>> GetOpusTopGoogleDrivesAsync(int top);

        Task<List<RMDiscoveryGoogleDriveInfo>> GetDriveInfoesByContainerIds(string googleOrganizationId, IEnumerable<int> containerIds);

        Task<List<RMDiscoveryGoogleDriveInfo>> GetDriveInfoesByDriveIds(string googleOrganizationId, IEnumerable<long> driveIds);
    }
}
