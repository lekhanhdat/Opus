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
using AvePoint.RA.DB.Model.Discovery.Google;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System;
using AvePoint.RA.DB.Core.Discovery.Context;

namespace AvePoint.RA.DB.Dao.Discovery.Google
{
    public interface IRMDiscoveryGoogleDataDao
    {
        Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryGoogleContainerInactiveData> GetContainerInactiveDataListAsync(string googleOrganizationId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicInactiveDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicInactiveData[] dataList);

        Task<List<RMDiscoveryGoogleBasicInactiveData>> GetBasicInactiveDataListAsync(string googleOrganizationId, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteBasicInactiveDataAsync(string googleOrganizationId);

        Task DeleteDriveInactiveDataListAsync(string googleOrganizationId, int driveId);

        Task AddDriveInactiveDataListAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryGoogleDriveInactiveData> GetDriveInactiveDataByContainerIdAsync(string googleOrganizationId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryGoogleDriveInactiveData> GetDriveInactiveDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteDriveRuleLevelRotDataListAsync(string googleOrganizationId, int driveId);

        Task AddDriveRuleLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveRuleLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryGoogleDriveRuleLevelRotData> GetDriveRuleLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId);

        Task DeleteDriveCategoryLevelRotDataListAsync(string googleOrganizationId, int driveId);

        Task AddDriveCategoryLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveCategoryLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryGoogleDriveCategoryLevelRotData> GetDriveCategoryLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId);

        Task DeleteDriveRootLevelRotDataListAsync(string googleOrganizationId, int driveId);

        Task AddDriveRootLevelRotDataListAsync(string googleOrganizationId, List<RMDiscoveryGoogleDriveRootLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryGoogleDriveRootLevelRotData> GetDriveRootLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryGoogleDriveCategoryLevelRotData> GetDriveCategoryLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryGoogleDriveRuleLevelRotData> GetDriveRuleLevelRotDataBySqlConditionalExpressionAsync(string googleOrganizationId, int driveId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryGoogleDriveRootLevelRotData> GetDriveRootLevelRotDataByContainerIdAsync(string googleOrganizationId, int containerId);

        Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerRuleLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryGoogleContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(string googleOrganizationId, int containerId);

        Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerCategoryLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryGoogleContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(string googleOrganizationId, int containerId);

        Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerRootLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryGoogleContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(string googleOrganizationId, int containerId);

        Task AddOrUpdateBasicRuleLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicRuleLevelRotData[] dataList);

        Task<List<RMDiscoveryGoogleBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(string googleOrganizationId);

        Task AddOrUpdateBasicCategoryLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicCategoryLevelRotData[] dataList);

        Task<List<RMDiscoveryGoogleBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(string googleOrganizationId);

        Task AddOrUpdateBasicRootLevelRotDataAsync(string googleOrganizationId, params RMDiscoveryGoogleBasicRootLevelRotData[] dataList);

        Task<List<RMDiscoveryGoogleBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(string googleOrganizationId);

        Task<RMDiscoveryGoogleAggregateTotalData> GetAggregateTotalDataAsync(string organizationId);

        Task AddOrUpdateAggregateTotalDataAsync(string organizationId, RMDiscoveryGoogleAggregateTotalData data);

        Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleAggregateTotalData data);
    }
}
