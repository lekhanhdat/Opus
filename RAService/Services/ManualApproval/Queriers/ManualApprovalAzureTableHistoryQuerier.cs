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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ControlPlus;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class ManualApprovalAzureTableHistoryQuerier
    {
        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();

        private const int HISTORY_QUERY_LIMIT = 1000;

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public static async Task<List<ManualApprovalItem>> HistoryQueryAsync()
        {
            var tableDBSet = RMRecordStorageAzureTableContext.ManualApproveHistories;

            var last3Months = GetLast3Months();
            var items = new List<RMManualApproveHistoryTableEntity>();
            while(last3Months.Any() && items.Count < HISTORY_QUERY_LIMIT)
            {
                var month = last3Months.Dequeue().ToString();
                var (continuatioinToken, values) = await tableDBSet.QueryWithPagination(item => item.PartitionKey == month, HISTORY_QUERY_LIMIT - items.Count, null);
                items.AddRange(values);
            }

            items = (await PermissionFilterAsync(items));
             
            return await ConvertAsync(items);
        }
        
        public static async Task<List<ManualApprovalItem>> HistoryQueryForGControlAsync()
        {
            var tableDBSet = RMRecordStorageAzureTableContext.ManualApproveHistories;

            var last3Months = GetLast3Months();
            var items = new List<RMManualApproveHistoryTableEntity>();
            while(last3Months.Any() && items.Count < HISTORY_QUERY_LIMIT)
            {
                var month = last3Months.Dequeue().ToString();
                var (continuationToken, values) = await tableDBSet.QueryWithPagination(item => item.PartitionKey == month, HISTORY_QUERY_LIMIT - items.Count, null);
                items.AddRange(values);
            }
            
            items = await GetCPlusAvailableContentSources(items);
            
            return await ConvertAsync(items, true);
        }

        private static Queue<int> GetLast3Months()
        {
            var res = new Queue<int>(3);
            var now = DateTime.UtcNow;
            for(var i = 0; i < 3; i++)
            {
                var month = now.ToString("yyyyMM");
                res.Enqueue(int.Parse(month));
                now = now.AddMonths(-1);
            }

            return res;
        }

        private static async Task<List<RMManualApproveHistoryTableEntity>> PermissionFilterAsync(List<RMManualApproveHistoryTableEntity> items)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isBoxLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box);
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            var isFSLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            List<SourceFlag> sources = DashboardConfig.SourceFlagOrder.Keys.ToList();
            if (!isSPOLicense)
            {
                sources.Remove(SourceFlag.SharePointOnPrem);
            }
            if (!isBoxLicense)
            {
                sources.Remove(SourceFlag.Box);  
            }
            if (!isFSLicense)
            {
                sources.Remove(SourceFlag.FileSystem);
            }
            if (!isGoogleLicense)
            {
                sources.Remove(SourceFlag.Google);
            }
            if (!isILLicense)
            {
                sources.Remove(SourceFlag.SharePoint);
                sources.Remove(SourceFlag.OneDrive);
                sources.Remove(SourceFlag.Exchange);
            }
            //Remove GGControl source
            items = items.Where(item => item.Source != (int)SourceFlag.GGControl).ToList();
            if (isAdmin)
            {
                return items.Where(item => sources.Contains((SourceFlag)item.Source) || item.Source >= (int)SourceFlag.Connector).ToList();
            }

            var userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);

            return items.Where(item =>
            {
                if (!string.IsNullOrEmpty(item.EscalateTo))
                {
                    return userIds.Any(userId => item.EscalateTo.Contains($"|{userId}|")) && ( sources.Contains((SourceFlag)item.Source) || item.Source >= (int)SourceFlag.Connector );
                }
                else
                {
                    return  false;
                }
            }).ToList();

        }
        
        
        private static async Task<List<RMManualApproveHistoryTableEntity>> GetCPlusAvailableContentSources(List<RMManualApproveHistoryTableEntity> items)
        {
            List<SourceFlag> sources = [SourceFlag.GGControl];

            if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
            {
                return items.Where(item => sources.Contains((SourceFlag)item.Source)).ToList();
            }

            var userIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);

            return items.Where(item =>
            {
                if (!string.IsNullOrEmpty(item.EscalateTo))
                {
                    return userIds.Any(userId => item.EscalateTo.Contains($"|{userId}|")) &&  sources.Contains((SourceFlag)item.Source);
                }
                return  false;
            }).ToList();
        }

        private static async Task<List<ManualApprovalItem>> ConvertAsync(List<RMManualApproveHistoryTableEntity> items, bool isGControl = false) 
        {
            var userCache = new Dictionary<int, string>();

            var contentSourceInfoes = await Cache.TryGetAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, async () =>
            {
                return (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)).ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));
            });

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            if (isGControl) generalSetting.TimeZoneId = GeneralSettingService.ConvertBrowserTimeZoneToWindows(TenantLocalValue.TimezoneId);

            return await items.ConvertAllAsync(async item => {
                try
                {
                    return new ManualApprovalItem
                    {
                        RecordsId = item.RecordsId,
                        SourceFlag = item.Source,
                        SourceName = contentSourceInfoes.ContainsKey(item.Source) ? contentSourceInfoes[item.Source] : I18NEntity.GetString("RM_CP_Connector"),
                        SourceIcon = BuildInContentSourceI18Ns.SourceFlagIcons.ContainsKey((SourceFlag)item.Source) ? BuildInContentSourceI18Ns.SourceFlagIcons[(SourceFlag)item.Source] : "fia-connecter",
                        NodeType = item.Level,
                        LeafName = item.LeafName,
                        FileExtension = I18NEntity.GetString(item.FileExtension),
                        RuleId = item.RuleId,
                        RuleName = item.RuleName,
                        RuleCriteria = item.RuleCriteria,
                        RuleDisposalClass = item.RuleDisposalClass ?? string.Empty,
                        ReviewerDisplayNames = GetUsersDisplayNames(userCache, item.EscalateTo),
                        EscalateFromDisplayName = await GetUserDisplayNameAsync(userCache, item.EscalateFrom),
                        FullPath = item.FullPath,
                        ApprovedByUserId = item.ApprovedBy,
                        ApprovedByDisplayName = await GetUserDisplayNameAsync(userCache, item.ApprovedBy),
                        InternalApprovedStatus = item.ApprovedStatus,
                        EscalatedComment = item.EscalatedComment,
                        ExtendComment = item.ExtendComment,
                        CreatedBy = item.CreatedBy,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedTime = item.ModifiedTime> 0 ? GeneralSettingService.ConvertTiksToDateTime(generalSetting, item.ModifiedTime, true).SimplifyFormatTime : string.Empty,
                        CollectionTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, item.CollectionTime, true).SimplifyFormatTime,
                        ActionTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, item.ActionTime, true).SimplifyFormatTime,
                        IsRelatedRecords = item.IsRelatedRecords,
                        RelatedRecordsAction = item.RelatedRecordsAction,
                        RetentionStatus = item.RetentionStatus,
                        RelatedRecords = string.IsNullOrEmpty(item.RelatedRecords) ?
                                        new List<ReportRelatedRecords>() :
                                        SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(item.RelatedRecords),
                        ManualApprovalComment = item.ManualApprovalComment,
                        QuickReason = item.QuickReason,
                        FolderPath = item.FolderPath,
                        WebViewLink = item.WebViewLink,
                    };
                 }
                catch(Exception e)
                {
                    return null;
                }
                });
        }

        private static List<string> GetUsersDisplayNames(Dictionary<int, string> userCache, string userIntIdsStr)
        {
            if (string.IsNullOrEmpty(userIntIdsStr))
            {
                return new List<string>();
            }

            var userIntIds = userIntIdsStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList().ConvertAll(item => int.Parse(item));
            if (userIntIds == null || userIntIds.Count == 0)
            {
                return new List<string>();
            }

            var notInCacheUserIds = userIntIds.Where(item => !userCache.ContainsKey(item)).ToHashSet().ToList();
            if(notInCacheUserIds.Any())
            {
                var users = AccountDao.GetUserWithRemovedByIds(userIntIds.ToHashSet().ToList()).Result;
                users = users.DistinctBy(item => item.UserPrincipalName).ToList();
                users.ForEach(item => userCache[item.Id] = item.DisplayName);
            }

            return userIntIds.Where(item => userCache.ContainsKey(item))
                .ToHashSet()
                .ToList()
                .ConvertAll(item => userCache[item]);
        }

        private static async Task<string> GetUserDisplayNameAsync(Dictionary<int, string> userCache, int userIntId)
        {
            if (userIntId <= 0)
            {
                return "";
            }

            if(!userCache.ContainsKey(userIntId))
            {
                var user = await AccountDao.GetUserByIdAsync(userIntId);
                if(user != null)
                {
                    userCache[userIntId] = user.DisplayName;
                }
            }

            if(!userCache.ContainsKey(userIntId))
            {
                return "";
            }

            return userCache[userIntId];
        }
    }
}
