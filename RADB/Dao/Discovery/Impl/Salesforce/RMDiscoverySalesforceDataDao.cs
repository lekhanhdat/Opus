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
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Salesforce;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;

public class RMDiscoverySalesforceDataDao : IRMDiscoverySalesforceDataDao
{
    public async Task AddNewObjectInfoAsync(string organizationId, RMDiscoverySalesforceObjectInfo sfObjectInfos)
    {
        using var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
        context.SalesforceObjectInfos.Add(sfObjectInfos);
        await context.SaveChangesAsync();
    }

    private string GetQueryForAggregateDataTable()
    {
        var tableName = "RMSalesforceObjectInfoData";
        var totalItemCount = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.TotalItemCount));
        var objectType = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.ObjectType));
        var oldestRecordCreateTime = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.OldestRecordsCreatedTime));
        var displayName = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.DisplayName));
        var totalSize = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.TotalSize));
        var lastestModifiedTime = SecurityUtils.SanitizeSQLSchemaName(nameof(RMDiscoverySalesforceObjectInfo.LatestModifiedTime));
        return
            $@"Select Count(*) as [{nameof(SfAggrerateDataDto.ObjectTotalCount)}]
            ,(SELECT Sum({totalItemCount}) FROM OrganizationScheme.[{tableName}]) as [{nameof(SfAggrerateDataDto.RecordsTotalCount)}]
            ,Min([{oldestRecordCreateTime}]) as [{nameof(SfAggrerateDataDto.OldestRecordsCreatedTime)}]
            ,(Select TOP 1 [{displayName}] FROM OrganizationScheme.[{tableName}] where {totalSize} = (Select Max({totalSize}) FROM OrganizationScheme.[{tableName}] WHERE {objectType} in (0,1)) and {objectType} in (0,1) ORDER BY {lastestModifiedTime} DESC, [{displayName}] ASC) as [{nameof(SfAggrerateDataDto.BiggestObjectByDataSize)}]
            ,(Select TOP 1 [{displayName}] FROM OrganizationScheme.[{tableName}] where {totalSize} = (Select Max({totalSize}) FROM OrganizationScheme.[{tableName}] WHERE {objectType} in (2,3)) and {objectType} in (2,3) ORDER BY {lastestModifiedTime} DESC, [{displayName}] ASC) as [{nameof(SfAggrerateDataDto.BiggestObjectByFileSize)}]
            ,(Select TOP 1 [{displayName}] FROM OrganizationScheme.[{tableName}] where {totalItemCount} = (Select Max({totalItemCount}) FROM OrganizationScheme.[{tableName}]) ORDER BY {lastestModifiedTime} DESC, [{displayName}] ASC) as [{nameof(SfAggrerateDataDto.BiggestObjectByRecordCount)}]
            FROM OrganizationScheme.[{tableName}]";
    }

    public async Task<SfAggrerateDataDto> GetAggregateTotalDataAsync(string organizationId)
    {
        using var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
        var query = GetQueryForAggregateDataTable().Replace("OrganizationScheme", RMDiscoveryDBManager.GetSalesforceSchemaName(organizationId));
        var result = await context.Database.SqlQuery<SfAggrerateDataDto>(query).FirstOrDefaultAsync();
        return result;
    }

    public async Task AddAggregateTotalDataAsync(string organizationId, RMDiscoverySalesforceAggregateTotalData data)
    {
        using var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
        context.SalesforceAggregateTotalData.AddOrUpdate(data);
        await context.SaveChangesAsync();
    }

    public async Task AddRecordBasicDataAsync(string organizationId, List<RMDiscoverySalesforceRecordInactiveData> data)
    {
        if (data.Count > 0)
        {
            using var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
            context.SalesforceRecordInactiveData.AddRange(data);
            await context.SaveChangesAsync();
        }
    }


    public async Task AddFileInactiveDatasAsync(string organizationId, List<RMDiscoverySalesforceFileInactiveData> data)
    {
        if (data.Count > 0)
        {
            using var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
            context.SalesforceFileInactiveData.AddRange(data);
            await context.SaveChangesAsync();
        }
    }
}