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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Salesforce;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.DB.Model.Discovery.Salesforce.Enum;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce
{
    public class RMDiscoverySalesforceDataQueryDao : IRMDiscoverySalesforceDataQueryDao
    {
        private IReadOnlyList<int> RMSFObjectTypeOfData = [(int)RMDiscoverySalesforceObjectType.StandardObject, (int)RMDiscoverySalesforceObjectType.CustomObject];
        private IReadOnlyList<int> RMSFObjectTypeOfFile = [(int)RMDiscoverySalesforceObjectType.FileObject, (int)RMDiscoverySalesforceObjectType.AttachmentObject];
        public async Task<RMDiscoverySalesforceAggregateTotalData> GetAggregateTotalDataAsync(string organizationId)
        {
            organizationId = organizationId.IsNullOrEmpty()
                ? await GetOrginazationId()
                : organizationId;
            
            var efContext = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
            return await efContext.SalesforceAggregateTotalData.FirstAsync(x => x.OrgId == organizationId);
        }

        public async Task<List<RMDiscoverySalesforceObjectInfo>> GetDataAnalysis(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(salesforceQueryParameter.OrganizationId);
            return await context.SalesforceObjectInfos
                .Where(x => Enumerable.Contains(RMSFObjectTypeOfData, x.ObjectType))
                .ToListAsync();
        }

        public async Task<T> GetDataAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            var res = dataCollection.ToList<T>().FirstOrDefault();
            return res;
        }

        public async Task<List<RMDiscoverySalesforceObjectInfo>> GetFileAnalysis(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(salesforceQueryParameter.OrganizationId);
            return await context.SalesforceObjectInfos
                .Where(x => Enumerable.Contains(RMSFObjectTypeOfData, x.ObjectType))
                .ToListAsync();
        }

        public async Task<List<T>> GetDataListAsync<T>(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            return dataCollection.ToList<T>();
        }
        public async Task<List<Dictionary<string, object>>> GetDataDictionaryListAsync(string sql, params SqlParameter[] parameters)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, parameters);
            return dataCollection.ToDictionary();
        }
        public async Task<List<RMSFObjectSelected>> GetObjectByName(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            salesforceQueryParameter.OrganizationId = salesforceQueryParameter.OrganizationId.IsNullOrEmpty()
                ? await GetOrginazationId()
                : salesforceQueryParameter.OrganizationId;
            var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(salesforceQueryParameter.OrganizationId);

            var searchKey = salesforceQueryParameter.NodeQueryParameter?.SearchKey;

            var query = context.SalesforceObjectInfos
                .Where(x => (string.IsNullOrEmpty(searchKey) || x.DisplayName.Contains(searchKey))
                            && Enumerable.Contains(RMSFObjectTypeOfData, x.ObjectType))
                .Select(objectInfos => new RMSFObjectSelected
                {
                    ObjectId = objectInfos.Id,
                    DisplayName = objectInfos.DisplayName,
                    ObjectType = objectInfos.ObjectType,
                })
                .OrderBy(x => x.DisplayName);

            return await query.ToListAsync();
        }


        public async Task<int> GetMonthLastest()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.SalesforceWithoutInDateList.MaxAsync(x => x.Unit);
        }

        public async Task<string> GetOrginazationId()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var organition = await efContext.Configurations.FirstOrDefaultAsync(x => x.ConfigurationType == RMDiscoveryConfigurationType.SalesforceNewlyScope);
            if (organition == null) return null;
            return JsonConvert.DeserializeObject<RMDiscoverySalesforceScopeInfo>(organition.ValueJson).Organizations.First().Id;
        }

        public async Task<List<RMDiscoverySalesforceObjectInfo>> GetAllObjectInfor()
        {
            var organizationId = await GetOrginazationId();
            var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
            return await context.SalesforceObjectInfos.ToListAsync();
        }

        public async Task<int> CountAllObjectInforAsync()
        {
            var organizationId = await GetOrginazationId();
            var context = await RMDiscoveryDBManager.GetSalesforceEFContextAsync(organizationId);
            return await context.SalesforceObjectInfos.CountAsync();
        }
    }
}
