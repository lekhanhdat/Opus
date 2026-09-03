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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using RADiscovery.Query.Parameter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query
{
    public class DiscoveryBasicInfoQuerier
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(DiscoveryBasicInfoQuerier));

        public static async Task<List<RMDiscoveryO365TenantInfo>> GetTenants()
        {
            try
            {
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var sql = "SELECT UniqueId, Name, AdminUrl FROM [dbo].[RMO365TenantInfoes]";
                var dataCollection = await context.ExecuteQueryAsync(sql);
                return dataCollection.ToList<RMDiscoveryO365TenantInfo>().OrderBy(item => item.Name).ToList();
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while get tenants. Error: {e}");
                return new();
            }
        }

        public static async Task<List<RMDiscoveryWithoutInDate>> GetWithoutInDateList()
        {
            try
            {
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var sql = "SELECT Id, Unit, UnitType, [Order] FROM [dbo].[RMModifiedWithoutInDate]";
                var dataCollection = await context.ExecuteQueryAsync(sql);
                return dataCollection.ToList<RMDiscoveryWithoutInDate>().OrderBy(item => item.Order).ToList();
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while get tenants. Error: {e}");
                return new();
            }
        }

        public static async Task<List<DiscoveryTableColumnInfo>> GetInactiveTableRuleColumns()
        {
            try
            {
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var sql = @"SELECT UniqueId, Name FROM [dbo].[RMRuleInfo] WHERE DefinitionKind = 1 AND IsEnable = 1";
                var dataCollection = await context.ExecuteQueryAsync(sql);
                var inactiveRules = dataCollection.ToList<RMDiscoveryRuleInfo>();
                var ruleColumns = inactiveRules.ConvertAll(item => new DiscoveryTableColumnInfo(item.Name, "c" + item.UniqueId.ToString().Replace("-", ""))).ToList();
                return ruleColumns;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while get tenants. Error: {e}");
                return new();
            }
        }

        public static async Task<List<Dictionary<string,object>>> GetROTRuleCategoryInfo()
        {
            try 
            {
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var sql = @"SELECT [Category], [Id], [Name] FROM [dbo].[RMRuleInfo] WHERE DefinitionKind != 1 AND IsEnable = 1";
                var dataCollection = await context.ExecuteQueryAsync(sql);
                return dataCollection.ToDictionary();
            } 
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while get tenants. Error: {e}");
                return new();
            }
        }

        public static async Task<List<RMDiscoverySizeRange>> GetDiscoverySizeRange()
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = @"SELECT [Id],[GenerateEqual],[LessThan],[Order],[DisplayName] FROM [dbo].[RMSizeRange]";
            var dataCollection = await context.ExecuteQueryAsync(sql);
            var sizeRanges = dataCollection.ToList<RMDiscoverySizeRange>();
            return sizeRanges;
        }
    }

    public class DiscoveryRotRuleInfo
    {
        public string Name { get; set; }

        public int Category { get; set; }


    }

    public class DiscoveryTableColumnInfo
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("internalName")]
        public string InternalName { get; set; }

        public DiscoveryTableColumnInfo(string displayName, string internalName)
        {
            DisplayName = displayName;
            InternalName = internalName;
        }  
    };
}
