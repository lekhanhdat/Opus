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
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.FileSystem
{
    public interface IRMDiscoveryFSDataDao
    {
        Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(params RMDiscoveryFSContainerInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryFSConnectionInactiveData> GetConnectionInactiveDataByContainerIdAsync(int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task<List<RMDiscoveryFSBasicInactiveData>> GetBasicInactiveDataListAsync(List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryFSContainerInactiveData> GetContainerInactiveDataListAsync(int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicRuleLevelRotData[] dataList);

        Task<List<RMDiscoveryFSBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync();

        IAsyncEnumerable<RMDiscoveryFSContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(int containerId);

        Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicCategoryLevelRotData[] dataList);

        Task<List<RMDiscoveryFSBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync();

        Task<List<RMDiscoveryFSBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync();

        IAsyncEnumerable<RMDiscoveryFSContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(int containerId);

        Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(params RMDiscoveryFSBasicRootLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryFSContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(int containerId);

        Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerRuleLevelRotData[] dataList);

        Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerCategoryLevelRotData[] dataList);

        Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(params RMDiscoveryFSContainerRootLevelRotData[] dataList);

        Task<RMDiscoveryFSAggregateTotalData> GetAggregateTotalDataAsync();

        Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryFSAggregateTotalData data);

        Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSAggregateTotalData data);
    }
}
