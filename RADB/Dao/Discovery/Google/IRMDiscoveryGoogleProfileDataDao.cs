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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery.Profile;

namespace AvePoint.RA.DB.Dao.Discovery.Google;

public interface IRMDiscoveryGoogleProfileDataDao
{
    Task DeleteDriveInactiveDataByDriveIdAsync(string googleOrganizationId, Guid profileId, int driveInfoId);
    
    Task AddDriveInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileDriveInactiveData[] dataList);
    
    Task DeleteContainerInactiveDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId);
    
    IAsyncEnumerable<RMDiscoveryGoogleProfileDriveInactiveData> GetDriveInactiveDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns);
    
    Task AddContainerInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileContainerInactiveData[] dataList);

    Task DeleteBasicInactiveDataAsync(string googleOrganizationId, Guid profileId);

    IAsyncEnumerable<RMDiscoveryGoogleProfileContainerInactiveData> GetContainerInactiveDataAsync(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> customColumns);
    
    Task AddBasicInactiveDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileBasicInactiveData[] dataList);

    Task DeleteDriveRotDataByDriveIdAsync(string googleOrganizationId, Guid profileId, int driveId);
    
    Task AddDriveRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileDriveRotData[] dataList);
    
    Task DeleteContainerRotDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId);
    
    IAsyncEnumerable<RMDiscoveryGoogleProfileDriveRotData> GetDriveRotDataByContainerIdAsync(string googleOrganizationId, Guid profileId, int containerId, List<RMDiscoveryCustomColumn> customColumns);
    
    Task AddContainerRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileContainerRotData[] dataList);

    Task DeleteBasicRotDataAsync(string googleOrganizationId, Guid profileId);
    
    Task AddBasicRotDataListAsync(string googleOrganizationId, Guid profileId, params RMDiscoveryGoogleProfileBasicRotData[] dataList);

    IAsyncEnumerable<RMDiscoveryGoogleProfileContainerRotData> GetContainerRotDataAsync(string googleOrganizationId, Guid profileId, List<RMDiscoveryCustomColumn> customColumns);

}