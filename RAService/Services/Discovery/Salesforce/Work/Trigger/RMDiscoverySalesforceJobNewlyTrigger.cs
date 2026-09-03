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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Model.Salesforce;
using RASalesforce;
using RASalesforce.Util;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Trigger;

public class RMDiscoverySalesforceJobNewlyTrigger : IRMDiscoverySalesforceJobTriggerible
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceJobNewlyTrigger));
    
    private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();


    public async Task<(bool succeed, Dictionary<string, List<SfObjectJobDto>> objectsByOrganization)> GetWillTriggerJobsAsync()
    {
        try
        {
            Dictionary<string, List<SfObjectJobDto>> objectsByOrganization = new();
            
            var sfScopeInfo =
                await _configurationDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope);

            var organizationIds = sfScopeInfo.Organizations.Select(x => x.Id).ToList();
            foreach (var sfOrganizationId in organizationIds)
            {
                var customerId = TenantLocalValue.LogonGroupId;
                var sfService = new SalesforceService(customerId, sfOrganizationId).Build();

                var apiSObjects = (await sfService.GetSalesforceObjectsAsync()).Where(RMSFObjectUtil.IsStorageObject).ToList();
                
                List<SfObjectJobDto> triggerObjects = apiSObjects.ConvertAll(apiSObject => new SfObjectJobDto(sfOrganizationId,apiSObject.Name));
                
                objectsByOrganization.Add(sfOrganizationId, triggerObjects);
            }
            
            _logger.Info($"Successful allocate will trigger jobs.");
            return (true, objectsByOrganization);
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
            return (false, []);
        }
    }

    public async Task<bool> InitTablesAsync(string safesForceTenantId)
    {
        try
        {
            
            await RMDiscoveryDBManager.DropSalesforceTablesAsync(safesForceTenantId);
            
            await RMDiscoveryDBManager.InitSalesforceBasicTablesAsync(safesForceTenantId); 
            await RMDiscoveryDBManager.InitSalesforceInactiveTablesAsync(safesForceTenantId);

            return true;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while init tables async. Error: {e}");
            return false;
        }
    }
}