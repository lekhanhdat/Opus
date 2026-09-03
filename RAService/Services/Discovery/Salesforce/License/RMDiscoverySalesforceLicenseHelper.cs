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
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using Cloud.Sdk.Data.AosModern;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.License;

public class RMDiscoverySalesforceLicenseHelper
{
    private static readonly IRMDiscoverySalesforceExecutionInfoDao s_executionInfoDao = new RMDiscoverySalesforceExecutionInfoDao();

    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceLicenseHelper));

    
    public static async Task<LicenseType> GetLicenseTypeAsync()
    {
        var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
        s_logger.Info($"Current tenant salesforce license is [{licenseInfo.Type}].");
        return licenseInfo.Type;
    }
    
    public static async Task<bool> IsMeetLimitAsync()
    {
        var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
        
        var discoveryLicenseInfo = licenseInfo.SalesforceDiscoveryLicenseInfo;
        var (_, _, currentYearCount) = await s_executionInfoDao.CalculateAsync(licenseInfo.Type);
        s_logger.Info($"Current year count value: {currentYearCount}");
        return discoveryLicenseInfo.FrequencyPerYear > currentYearCount;
    }
    
    public static async Task IncreaseConsumedFrequencyPerYearAsync()
    {
        var aosApiClient = AosApiUtility.GetAosModerClient();
        var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
        if (info.Type == LicenseType.Trial)
        {
            return;
        }

        if (info.Extension is CloudRecordsExtension extension)
        {
            // var currentConsumedFrequency = extension.ConsumedFrequencyForSalesforce;
            // extension.ConsumedFrequencyForSalesforce++;
            var result = await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
            {
                LicenseId = info.Id,
                Extension = extension
            });
            //s_logger.Info($"Current consumed frequency value: {currentConsumedFrequency}, New consumed frequency value: {extension.ConsumedFrequencyForSalesforce}, status: {result}");
        }
    }

    public static async Task DecreaseConsumedFrequencyPerYearAsync()
    {
        var aosApiClient = AosApiUtility.GetAosModerClient();
        var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
        if (info.Type == LicenseType.Trial)
        {
            return;
        }

        // if (info.Extension is CloudRecordsExtension { ConsumedFrequencyForSalesforce: > 0 } extension)
        // {
        //     var currentConsumedFrequency = extension.ConsumedFrequencyForSalesforce;
        //     extension.ConsumedFrequencyForSalesforce--;
        //     var result = await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
        //     {
        //         LicenseId = info.Id,
        //         Extension = extension
        //     });
        //     s_logger.Info($"Current consumed frequency value: {currentConsumedFrequency}, New consumed frequency value: {extension.ConsumedFrequencyForSalesforce}, status: {result}");
        // }
    }
    
    public static async Task RemoveAllExecutionAsync()
    {
        await s_executionInfoDao.DeleteAllRecordsAsync();
    }
    
    public static async Task<bool> ClearLicenseUsageAsync()
    {
        try
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info.Type == LicenseType.Trial)
            {
                return true;
            }
            // if (info.Extension is CloudRecordsExtension { ConsumedFrequencyForSalesforce: > 0 } extension)
            // {
            //     extension.ConsumedFrequencyForSalesforce = 0;
            //     await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
            //     {
            //         LicenseId = info.Id,
            //         Extension = extension
            //     });
            // }
            await RemoveAllExecutionAsync();
            return true;
        }
        catch (Exception e)
        {
            s_logger.Error($"Clear salesforce discovery license usage failed, error: {e}");
            return false;
        }
    }
}