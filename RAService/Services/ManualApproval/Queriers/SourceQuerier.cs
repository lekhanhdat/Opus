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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Dashboard;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ControlPlus;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class SourceQuerier : IFilterWithHistory, IDefaultValue
    {
        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.Source;

        public ManualApprovalDefaultOptions DefaultValueOption => ManualApprovalDefaultOptions.Source;


        private static readonly List<SourceFlag> Sources = new List<SourceFlag>
                        {
                            SourceFlag.Teams,
                            SourceFlag.SharePoint,
                            SourceFlag.Exchange,
                            SourceFlag.Physical,
                            SourceFlag.FileSystem,
                            SourceFlag.OneDrive,
                            SourceFlag.SharePointOnPrem,
                            SourceFlag.Box,
                            SourceFlag.Google
                        };

        public Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var filterSources = JsonConvert.DeserializeObject<List<int>>(value);
            return System.Threading.Tasks.Task.FromResult<Expression<Func<ManualApprovalRecord, bool>>>((root) => filterSources.Contains(root.SourceFlag));
        }

        public async Task<object> GetDefaultValueAsync()
        {
            if(TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
            {
                return new List<KeyValuePair<int, string>>
                {
                    new KeyValuePair<int, string>((int)SourceFlag.Google, I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[SourceFlag.Google])),
                };
            }
            static bool CheckLicense(SourceFlag sourceFlag)
            {
                switch(sourceFlag)
                {
                    case SourceFlag.SharePoint:
                    case SourceFlag.OneDrive:
                    case SourceFlag.Exchange:
                        return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForProduct.OpusIL);
                    case SourceFlag.FileSystem:
                        return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
                    case SourceFlag.SharePointOnPrem:
                        return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
                    case SourceFlag.Box:
                        return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.Box);
                    case SourceFlag.Google:
                        return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForProduct.OpusGoogle);
                    case SourceFlag.Teams:
                        return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForProduct.OpusIL) && TeamsPermissionHelper.HasUpgradeTeamsFeature();
                }
                return true;
            }

            var sources = Sources.Where(item => CheckLicense(item))
                .ConvertAll(item => new KeyValuePair<int, string>((int)item, I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[item])))
                .OrderBy(item =>
                {
                    if (DashboardConfig.SourceFlagOrder.TryGetValue((SourceFlag)item.Key, out var result))
                    {
                        return result;
                    }
                    return item.Key;
                }).ToList();

            if(TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForProduct.OpusIL))
            {
                var customizeContentSources = (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.ExternalCustomize))
                .OrderBy(item => item.Flag)
                .ToList().ConvertAll(item => new KeyValuePair<int, string>(item.Flag, item.Name));
                sources.AddRange(customizeContentSources);
            }
            return sources;
        }

        public Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var filterSources = JsonConvert.DeserializeObject<List<int>>(value);
            var result = new ManualApprovalSqlDefintion();
            
            var sql = $"Source IN {DatabaseUtility.BuildInClause(filterSources)}";
            result.Sql = sql;
            
            return System.Threading.Tasks.Task.FromResult(result);
        }
    }
}
