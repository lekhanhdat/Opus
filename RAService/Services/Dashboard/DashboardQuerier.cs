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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Collections;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.Dashboard.Query;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Dashboard
{
    public class DashboardQuerier
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DashboardQuerier));

        private static readonly Dictionary<SourceFlag, DashboardQueryable> DashboardQueries = new Dictionary<SourceFlag, DashboardQueryable>();

        private static ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        private static IDashboardDao DashboardDao => PlatformWindsorManager.GetService<IDashboardDao>();
        private static IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();

        private static readonly ExpireConcurrentDictionary<string, SecurityUserPermissionsDto> PermissionCache = new();

        private static string LogonUserId => TenantLocalValue.LogonUserId;

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IRMArchiveGDriveInfoDao RMArchiveGDInfoDao => PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();

        private static IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        static DashboardQuerier()
        {
            try
            {
                var collectorType = typeof(DashboardQueryable);
                var assembly = Assembly.GetAssembly(collectorType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;
                    if (type.BaseType?.Name == collectorType.Name)
                    {
                        var instance = Activator.CreateInstance(type) as DashboardQueryable;
                        DashboardQueries.Add(instance.Flag, instance);
                    }
                }

                Logger.Info($"Successful initialize dashboard querier.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialize dashboard querier. Error: {e}");
            }
        }

        private static async Task<SecurityUserPermissionsDto> GetUserPermissionAsync(bool isFromGControl = false)
        {

            if (!PermissionCache.TryGet(LogonUserId, out var permission))
            {
                permission = await SecurityGroupManagementService.GetUserScopePermissionsAsync(LogonUserId, isFromGControl);
                if (!PermissionCache.TryAdd(LogonUserId, permission))
                {
                    Logger.Warn($"Add current user: [{LogonUserId}] permission to cache failed.");
                }
            }

            return permission;
        }

        public static async Task<bool> IsPhysicalEndUserAsync()
        {
            var permission = await GetUserPermissionAsync();
            return permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical)?.SubPermission == SubPermissionType.EndUser;
        }

        public static async Task<List<DashboardKeyValue<int>>> GetSourceFlagsAsync()
        {
            var permission = await GetUserPermissionAsync();
            var sourceFlags = FilterOpusILSource(permission);

            var isEndUser = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical)?.SubPermission == SubPermissionType.EndUser;
            if (isEndUser)
            {
                sourceFlags = sourceFlags.Where(item => item != SourceFlag.Physical);
            }
            var result = sourceFlags.OrderBy(item => DashboardConfig.SourceFlagOrder[item]).ToList()
                .ConvertAll(item => new DashboardKeyValue<int>(
                    I18NEntity.GetString(DashboardI18ns.SourceFlagI18ns[item]),
                    (int)item)
                );
            return result;
        }

        public static async Task<List<DashboardKeyValue<int>>> GetManualApprovalStatusAsync()
        {
            var result = new List<DashboardKeyValue<int>>();

            var queryDefinition = new ManualApprovalQueryDefinition();
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            FilterPermission(queryDefinition);
            var waitingApprovalCount = await ManualApprovalQuerier.CountAsync(queryDefinition);
            result.Add(new DashboardKeyValue<int>(I18NEntity.GetString(DashboardI18ns.ApprovalStatusI18ns[SOApproveDBStatus.WaitingApprove]), waitingApprovalCount));

            queryDefinition.Filters.Clear();
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            FilterPermission(queryDefinition);
            var waitingForDisposalCount = await ManualApprovalQuerier.CountAsync(queryDefinition);
            result.Add(new DashboardKeyValue<int>(I18NEntity.GetString("RM_MA_WaitDisposal"), waitingForDisposalCount));

            return result;
        }

        public static async Task<List<RMDashboardTermUsage>> GetTop10MostUsedTermsAsync(SourceFlag sourceFlag, bool isFromGControl = false)
        {
            List<RMDashboardTermUsage> result = new List<RMDashboardTermUsage>();
            var permission = await GetUserPermissionAsync(isFromGControl);
            var sourceFlags = permission.ScopePermissionInfo.Select(item => item.DataSourceType);

            if (!sourceFlags.Contains(sourceFlag))
            {
                return new List<RMDashboardTermUsage>();
            }

            //if (permission.IsAdmin)
            //{
            //    result = DashboardDao.GetTop10TermUsageInfos(sourceFlag);
            //}

            //else 
            if (permission.TermPermissionInfo.TermGroups == null)
            {
                return new List<RMDashboardTermUsage>();
            }
            else
            {
                var termSetIds = permission.TermPermissionInfo.TermGroups.SelectMany(group => group.SubTerms).Select(item => item.UniqueId.ToString());
                result = DashboardDao.GetTop10TermUsageInfos(sourceFlag, termSetIds);
            }

            Logger.Info($"top 10 term :{TenantLocalValue.LogonGroupId}, {TenantLocalValue.LogonUserId}, {string.Join(',', result.Select(r => r.Active))}");
            return result;
        }

        public static async Task<List<RMDashboardDataUsage>> GetTop10MostUsedSitesAsync(SourceFlag sourceFlag, bool isFromGControl = false)
        {
            var permission = await GetUserPermissionAsync(isFromGControl);
            var sourceFlags = permission.ScopePermissionInfo.Select(item => item.DataSourceType);

            if (!sourceFlags.Contains(sourceFlag))
            {
                return new List<RMDashboardDataUsage>();
            }

            if (permission.IsAdmin || sourceFlag == SourceFlag.FileSystem || sourceFlag == SourceFlag.SharePointOnPrem || sourceFlag == SourceFlag.AzureFileShare || sourceFlag == SourceFlag.Box || sourceFlag == SourceFlag.Google || sourceFlag == SourceFlag.GGControl)
            {
                return DashboardDao.GetTop10SiteUsageInfos(sourceFlag);
            }

            var guidContainerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == sourceFlag)?.ScopeIds;
            if (guidContainerIds == null)
            {
                return new List<RMDashboardDataUsage>();
            }

            if(sourceFlag == SourceFlag.Physical)
            {
                var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(guidContainerIds).Select(id => id.ToString());
                return DashboardDao.GetTop10LocationUsageInfos(sourceFlag, bottomLocationIdsUnderTopLocations);
            }
            var containerIds = guidContainerIds.ConvertAll(item => item.ToString());
            if (TeamsPermissionHelper.HasUpgradeTeamsFeature() && sourceFlag == SourceFlag.Teams)
            {
                var siteUrlUnderTeams = new List<string>();
                var teamsIds = RemoteNodeDao.GetTeamsIdByContainerId(containerIds);
                var dicNodes = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsIds(teamsIds, true);
                foreach (var node in dicNodes)
                {
                    siteUrlUnderTeams.AddRange(node.Value?.Select(_ => _.url) ?? new List<string>());
                }
                return DashboardDao.GetTop10SiteUsageInfos(sourceFlag, containerIds, siteUrlUnderTeams);
            }
            return DashboardDao.GetTop10SiteUsageInfos(sourceFlag, containerIds);
        }

        public static List<RMDashboardUserWaitingApprovalCount> GetTop10UserRecordsWaitingApproval(SourceFlag flag)
        {
            return DashboardDao.GetTop10UserRecordsWaitingApproval(flag);
        }

        public static List<DashboardLineChartItem> GetLineChartItems(SourceFlag sourceFlag, ChartDateRange dateRange)
        {

            var result = new List<DashboardLineChartItem>();

            var now = DateTime.UtcNow;
            var endTime = new DateTime(now.Year, now.Month, now.Day);

            DateTime GetStartTime()
            {
                if (dateRange == ChartDateRange.Last10Days)
                {
                    return endTime.AddDays(-10);
                }
                else if (dateRange == ChartDateRange.Last10Weeks)
                {
                    var day = (int)endTime.DayOfWeek - 1;
                    return endTime.Subtract(TimeSpan.FromDays(day)).Subtract(TimeSpan.FromDays(9 * 7));
                }
                return new DateTime(endTime.Year, endTime.Month, 1).AddMonths(-11);
            }

            var startTime = GetStartTime();
            Logger.Info($"The get line chart items by data range of [{startTime} - {endTime}]");

            var datas = DashboardDao.GetDataUsageOfDates(sourceFlag, startTime);

            var startPointer = startTime;
            var rangePointer = GetRangePointer();

            DateTime GetRangePointer()
            {
                if (dateRange == ChartDateRange.Last10Days)
                {
                    return startPointer;
                }
                else if (dateRange == ChartDateRange.Last10Weeks)
                {
                    return startPointer.AddDays(6);
                }
                return startPointer.AddMonths(1).AddDays(-1);
            }

            string GetDateFormat()
            {
                if (dateRange == ChartDateRange.Last10Days)
                {
                    return rangePointer.ToString("yyyy-MM-dd");
                }
                return startPointer.ToString("yyyy-MM-dd") + "~" + rangePointer.ToString("yyyy-MM-dd");
            }

            void ProcessData()
            {
                var dateFormat = GetDateFormat();
                var createdLineChartItem = new DashboardLineChartItem(I18NEntity.GetString("RM_DSB_Created"), 0, dateFormat);
                var destroyedLineChartItem = new DashboardLineChartItem(I18NEntity.GetString("RM_DSB_Destroyed"), 0, dateFormat);
                var waitingLineChartItem = new DashboardLineChartItem(I18NEntity.GetString("RM_DSB_Approval"), 0, dateFormat);

                var rangeData = datas.Where(item => item.Date >= startPointer.Ticks && item.Date <= rangePointer.Ticks);

                foreach (var data in rangeData)
                {
                    createdLineChartItem.Value += data.Created;
                    destroyedLineChartItem.Value += data.Destroyed;
                    waitingLineChartItem.Value += data.WaitingApproved;
                }

                startPointer = rangePointer.AddDays(1);
                rangePointer = GetRangePointer();
                result.Add(createdLineChartItem);
                result.Add(destroyedLineChartItem);
                result.Add(waitingLineChartItem);
            }

            while (rangePointer < endTime)
            {
                ProcessData();
            }

            if (rangePointer >= endTime)
            {
                rangePointer = endTime;
                ProcessData();
            }

            return result;
        }

        public static async Task<List<DashboardKeyValue<long>>> GetTermApplyRuleUsagesAsync()
        {
            var termApplyRuleUsages = new List<RMDashboardTermApplyRuleUsage>();
            var permission = await GetUserPermissionAsync();
            if (permission.IsAdmin)
            {
                termApplyRuleUsages = DashboardDao.GetTermApplyRuleUsages();
            }

            if (permission.TermPermissionInfo.TermGroups == null)
            {
                return new List<DashboardKeyValue<long>>();
            }

            var termSetIds = permission.TermPermissionInfo.TermGroups.SelectMany(group => group.SubTerms).Select(item => item.UniqueId.ToString());
            termApplyRuleUsages = DashboardDao.GetTermApplyRuleUsages(termSetIds);

            var applyRuleCount = termApplyRuleUsages.Sum(item => item.TermApplyRuleCount);
            var nonApplyRuleCount = termApplyRuleUsages.Sum(item => item.TermNonApplyRuleCount);

            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ApplyRule"), applyRuleCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_NotApplyRule"), nonApplyRuleCount),
            };
        }

        public static List<DashboardKeyValue<long>> GetLabelApplyRuleUsagesAsync()
        {
            var termApplyRuleUsages = new List<RMDashboardTermApplyRuleUsage>();

            termApplyRuleUsages = DashboardDao.GetLabelApplyRuleUsages();

            var applyRuleCount = termApplyRuleUsages.Sum(item => item.TermApplyRuleCount);
            var nonApplyRuleCount = termApplyRuleUsages.Sum(item => item.TermNonApplyRuleCount);

            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ApplyRule"), applyRuleCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_NotApplyRule"), nonApplyRuleCount),
            };
        }

        public static async Task<List<DashboardSourceFlagKeyValue>> GetSourcesSettingCountAsync()
        {
            var permission = await GetUserPermissionAsync();
            var sourceFlags = FilterOpusILSource(permission);
            try
            {
                return sourceFlags.OrderBy(item => DashboardConfig.SourceFlagOrder[item]).ToList().ConvertAll(item =>
                {
                    var count = DashboardQueries[item].GetApplySettingCount(permission);
                    return new DashboardSourceFlagKeyValue(I18NEntity.GetString(DashboardI18ns.SourceFlagI18ns[item]), count, (int)item);
                }).Where(item => item.Value > 0).ToList();
            } catch (Exception ex)
            {
                throw;
            }

        }

        public static async Task<List<DashboardSourceFlagKeyValue>> GetSourcesActiveCountAsync()
        {
            var permission = await GetUserPermissionAsync();
            var sourceFlags = permission.ScopePermissionInfo.Select(item => item.DataSourceType);
            var result = sourceFlags.OrderBy(item => DashboardConfig.SourceFlagOrder[item]).ToList().ConvertAll(item =>
            {
                var count = DashboardQueries[item].GetSourceActiveCount(permission);
                return new DashboardSourceFlagKeyValue(I18NEntity.GetString(DashboardI18ns.SourceFlagI18ns[item]), count, (int)item);
            }).Where(item => item.Value > 0).ToList();
            Logger.Info($"source active count :{TenantLocalValue.LogonGroupId}, {TenantLocalValue.LogonUserId}, {string.Join(',', result.Select(r => r.Value))}");
            return result;
        }

        public static async Task<List<DashboardKeyValue<long>>> GetManagedRecordsCountAsync()
        {
            var acitveCount = 0L;
            var destoryedCount = 0L;
            var archivedCount = 0L;

            var permission = await GetUserPermissionAsync();
            var sourceFlags = FilterOpusILSource(permission);
            sourceFlags.ToList().ForEach(item =>
            {
                acitveCount += DashboardQueries[item].GetSourceActiveCount(permission);
                destoryedCount += DashboardQueries[item].GetSourceDestroyedCount(permission);
                archivedCount += DashboardQueries[item].GetSourceArchivedCount(permission);
            });

            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ActiveRecords"), acitveCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ArchivedRecords"), archivedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_DestroyedRecords"), destoryedCount),
            };
        }

        public static async Task<List<DashboardKeyValue<long>>> GetPhysicalRequestsAsync()
        {
            long creationCount;
            long loanCount;
            long moveCount;
            var permission = await GetUserPermissionAsync();

            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            if (permission.IsAdmin)
            {
                creationCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Creation);
                loanCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Loan);
                moveCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Move);
            }
            else if(physicalPermission?.SubPermission == SubPermissionType.Admin)
            {
                var permissionTopLocationIds = physicalPermission?.ScopeIds ?? new List<Guid>();
                var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(permissionTopLocationIds);

                creationCount = DashboardDao.GetPhysicalRequestByLocationIds(PhysicalRequestType.Creation, bottomLocationIdsUnderTopLocations);
                loanCount = DashboardDao.GetPhysicalRequestByLocationIds(PhysicalRequestType.Loan, bottomLocationIdsUnderTopLocations);
                moveCount = DashboardDao.GetPhysicalRequestByLocationIds(PhysicalRequestType.Move, bottomLocationIdsUnderTopLocations);
            }
            else
            {
                creationCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Creation, LogonUserId);
                loanCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Loan, LogonUserId);
                moveCount = DashboardDao.GetPhysicalRequest(PhysicalRequestType.Move, LogonUserId);
            }
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_PhysicalCreation"), creationCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_PhysicalLoan"), loanCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_PhysicalMove"), moveCount),
            };
        }

        public static async Task<List<DashboardSourceFlagKeyValue>> GetWaitingDisposalApprovalAsync()
        {
            var permission = await GetUserPermissionAsync();
            var sourceFlags = ValidPermission(new Dictionary<SourceFlag, int>(DashboardConfig.SourceFlagOrder)).Keys;
            var result = (await sourceFlags.OrderBy(item => DashboardConfig.SourceFlagOrder[item]).ToList().ConvertAllAsync(async item =>
            {
                var queryDefinition = new ManualApprovalQueryDefinition();
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
                queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.Source,
                    Value = JsonConvert.SerializeObject(new List<SourceFlag> { item })
                });
                var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
                return new DashboardSourceFlagKeyValue(I18NEntity.GetString(DashboardI18ns.SourceFlagI18ns[item]), count, (int)item);
            })).Where(item => item.Value > 0).ToList();
            return result;
        }

        private static Dictionary<SourceFlag, int> ValidPermission(Dictionary<SourceFlag,int> sourceFlags)
        {
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isBoxLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box);
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isFSLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            if (!isSPOLicense)
            {
                sourceFlags.Remove(SourceFlag.SharePointOnPrem);
            }
            if (!isBoxLicense)
            {
                sourceFlags.Remove(SourceFlag.Box);
            }
            if (!isFSLicense)
            {
                sourceFlags.Remove(SourceFlag.FileSystem);
            }
            if (!isGoogleLicense)
            {
                sourceFlags.Remove(SourceFlag.Google);
            }
            return sourceFlags;
        }
        public static List<DashboardKeyValue<long>> GetMyCreationPhysicalRequest()
        {
            var waitingApprovalCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Creation, PhysicalRequestStatus.WaitingForApproval, LogonUserId);
            var approvedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Creation, PhysicalRequestStatus.Approved, LogonUserId);
            var rejectedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Creation, PhysicalRequestStatus.Rejected, LogonUserId);
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_WaitingApproval"), waitingApprovalCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Approved"), approvedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Rejected"), rejectedCount),
            };
        }

        public static List<DashboardKeyValue<long>> GetMyLoanPhysicalRequest()
        {
            var waitingApprovalCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Loan, PhysicalRequestStatus.WaitingForApproval, LogonUserId);
            var approvedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Loan, PhysicalRequestStatus.Approved, LogonUserId);
            var rejectedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Loan, PhysicalRequestStatus.Rejected, LogonUserId);
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_WaitingApproval"), waitingApprovalCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Approved"), approvedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Rejected"), rejectedCount),
            };
        }

        public static List<DashboardKeyValue<long>> GetMyMovePhysicalRequest()
        {
            var waitingApprovalCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Move, PhysicalRequestStatus.WaitingForApproval, LogonUserId);
            var approvedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Move, PhysicalRequestStatus.Approved, LogonUserId);
            var rejectedCount = DashboardDao.GetMyPhysicalRequest(PhysicalRequestType.Move, PhysicalRequestStatus.Rejected, LogonUserId);
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_WaitingApproval"), waitingApprovalCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Approved"), approvedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Rejected"), rejectedCount),
            };
        }

        public static async Task<string> GetLastCollectTimeAsync()
        {
            var lastCollectTick = DashboardDao.GetLastCollectTime();
            if (lastCollectTick == 0)
            {
                return string.Empty;
            }
            return (await GeneralSettingService.ConvertTiksToDateTimeAsync(lastCollectTick, false)).FormaTime;
        }

        public static async Task<string> GetNextCollectTimeAsync()
        {
            var nextCollectTick = DashboardDao.GetNextCollectTime();
            if (nextCollectTick == 0)
            {
                return string.Empty;
            }
            return (await GeneralSettingService.ConvertTiksToDateTimeAsync(nextCollectTick, false)).FormaTime;
        }

        public static long GetLastCollectTimeTick()
        {
            return DashboardDao.GetLastCollectTime();
        }

        public static async Task<long> GetPhysicalTermTotalAsync()
        {
            var permission = await GetUserPermissionAsync();
            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            if (physicalPermission == null || physicalPermission.SubPermission == SubPermissionType.EndUser)
            {
                return 0;
            }

            if (permission.IsAdmin)
            {
                return DashboardDao.GetPhysicalTermTotal();
            }

            var termSetIds = permission.TermPermissionInfo.TermGroups.SelectMany(group => group.SubTerms).Select(item => item.UniqueId.ToString());

            return DashboardDao.GetPhysicalTermTotal(termSetIds);
        }

        public static async Task<long> GetPhysicalLocationTotalAsync()
        {
            var permission = await GetUserPermissionAsync();
            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            if (physicalPermission == null || physicalPermission.SubPermission == SubPermissionType.EndUser)
            {
                return 0;
            }

            if (permission.IsAdmin)
            {
                return DashboardDao.GetPhysicalLocationTotal();
            }

            var topLocationPermissionIds = physicalPermission.ScopeIds;

            return DashboardDao.GetCountLocationUnderTopLocations(topLocationPermissionIds);
        }

        public static async Task<Dictionary<DashboardPhysicalLoan, long>> GetPhysicalLoanExpriedAndTotalAsync()
        {
            var permission = await GetUserPermissionAsync();

            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            Logger.Info($"The current user: [{LogonUserId}] has physical permission: [{physicalPermission != null}]");

            if (physicalPermission == null)
            {
                Logger.Info($"The current user [{LogonUserId}] not has physical permission.");
                return new Dictionary<DashboardPhysicalLoan, long>
                {
                    {DashboardPhysicalLoan.LoanExpiredTotal, 0 },
                    {DashboardPhysicalLoan.LoanTotal, 0 }
                };
            }

            var explorerQueryService = ExplorerQueryService;

            var isPhysicalAdmin = permission.IsAdmin ||
                physicalPermission.SubPermission == SubPermissionType.Admin;

            Logger.Info($"The current user: [{LogonUserId}] is physical admin: [{isPhysicalAdmin}]");

            async Task<ExplorerQueryV3Dto> GetBasicQueryDtoAsync()
            {
                var dto = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>
                        {
                            new ExplorerSearchOptionV3
                            {
                                Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Physical }),
                                Column = new ExplorerQueryColumn
                                {
                                    Id = QueryCloumnIds.SourceFlag,
                                }
                            }
                        }
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = string.Empty,
                        PageSize = 1,
                    }
                };

                if (!isPhysicalAdmin)
                {
                    var currentUser = await UserService.GetUsersByIdsAsync(new List<string> { LogonUserId });

                    dto.QueryOption.Values.Add(
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(currentUser),
                            Column = new ExplorerQueryColumn
                            {
                                Id = DefaultColumnIDs.LoanedBy,
                                Type = Contract.TemplateManagement.ColumnType.PeopleOrGroup
                            }
                        }
                    );
                } else if (!permission.IsAdmin)
                {
                    var permissionTopLocationIds = physicalPermission?.ScopeIds ?? new List<Guid>();
                    var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(permissionTopLocationIds).Select(id => id.ToString());
                    dto.QueryOption.Values.Add(
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(bottomLocationIdsUnderTopLocations),
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.LocationId,
                            }
                        }
                    );
                }

                return dto;
            }

            async Task<long> GetPhysicalLoanTotalAsync()
            {
                var basicQueryDto = await GetBasicQueryDtoAsync();
                basicQueryDto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Loan },
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.All,
                    })
                });
                return (await explorerQueryService.QueryDataListWithTotalAsync(basicQueryDto)).PagingInfo.Total;
            }

            async Task<long> GetPhysicalExpriedLoanTotalAsync()
            {
                var basicQueryDto = await GetBasicQueryDtoAsync();
                basicQueryDto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Loan },
                    Value = JsonConvert.SerializeObject(new DateInfo
                    {
                        Condition = DateCondition.BeforeNow
                    })
                });
                return (await explorerQueryService.QueryDataListWithTotalAsync(basicQueryDto)).PagingInfo.Total;
            }

            var loanTotal = await GetPhysicalLoanTotalAsync();
            Logger.Info($"The physical loan total: [{loanTotal}].");

            var loanExpiredTotal = await GetPhysicalExpriedLoanTotalAsync();
            Logger.Info($"The physical expired loan total: [{loanExpiredTotal}].");

            return new Dictionary<DashboardPhysicalLoan, long>
                {
                    {DashboardPhysicalLoan.LoanExpiredTotal, loanExpiredTotal },
                    {DashboardPhysicalLoan.LoanTotal, loanTotal }
                };
        }

        public static async Task<List<DashboardKeyValue<long>>> GetPhysicalRequestsByPhysicalExplorerAsync()
        {
            long waitingApprovalCount;
            long approvedCount;
            long rejectedCount;
            var permission = await GetUserPermissionAsync();

            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);
            if (physicalPermission == null)
            {
                return new List<DashboardKeyValue<long>>();
            }

            var isPhysicalAdmin = permission.IsAdmin ||
                    physicalPermission.SubPermission == SubPermissionType.Admin;

            if (isPhysicalAdmin)
            {
                if (permission.IsAdmin) 
                {
                    waitingApprovalCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.WaitingForApproval);
                    approvedCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.Approved);
                    rejectedCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.Rejected);
                }
                else
                {
                    var permissionTopLocationIds = physicalPermission?.ScopeIds ?? new List<Guid>();
                    var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(permissionTopLocationIds);
                    waitingApprovalCount = DashboardDao.GetCountPhysicalRequestByStatusAndLocationIds(PhysicalRequestStatus.WaitingForApproval, bottomLocationIdsUnderTopLocations);
                    approvedCount = DashboardDao.GetCountPhysicalRequestByStatusAndLocationIds(PhysicalRequestStatus.Approved, bottomLocationIdsUnderTopLocations);
                    rejectedCount = DashboardDao.GetCountPhysicalRequestByStatusAndLocationIds(PhysicalRequestStatus.Rejected, bottomLocationIdsUnderTopLocations);
                }
            }
            else
            {
                waitingApprovalCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.WaitingForApproval, LogonUserId);
                approvedCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.Approved, LogonUserId);
                rejectedCount = DashboardDao.GetPhysicalRequestByStatus(PhysicalRequestStatus.Rejected, LogonUserId);
            }
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_WaitingApproval"), waitingApprovalCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Approved"), approvedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_Rejected"), rejectedCount),
            };
        }

        public static async Task<long> GetPhysicalLoanPenddingTotalAsync()
        {
            var permission = await GetUserPermissionAsync();

            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            Logger.Info($"The current user: [{LogonUserId}] has physical permission: [{physicalPermission != null}]");

            if (physicalPermission == null)
            {
                Logger.Info($"The current user [{LogonUserId}] not has physical permission.");
                return 0;
            }

            var explorerQueryService = ExplorerQueryService;

            var isPhysicalAdmin = permission.IsAdmin ||
                physicalPermission.SubPermission == SubPermissionType.Admin;

            Logger.Info($"The current user: [{LogonUserId}] is physical admin: [{isPhysicalAdmin}]");

            async Task<ExplorerQueryV3Dto> GetBasicQueryDtoAsync()
            {
                var dto = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>
                        {
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Physical })
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn{ Id = QueryCloumnIds.FileExtension },
                                Value = JsonConvert.SerializeObject(new List<string> { ((int)RMNodeLevel.PhysicalCustom).ToString(), ((int)RMNodeLevel.PhysicalBox).ToString(), ((int)RMNodeLevel.PhysicalFile).ToString() })
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.LoanPickStatus },
                                Value = JsonConvert.SerializeObject(new List<PickStatusType>(){ PickStatusType.Pendding })
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Loan },
                                Value = JsonConvert.SerializeObject(new DateInfo
                                {
                                    Condition = DateCondition.All,
                                })
                            }
                        }
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = string.Empty,
                        PageSize = 1,
                    }
                };

                if (!isPhysicalAdmin)
                {
                    var currentUser = await UserService.GetUsersByIdsAsync(new List<string> { LogonUserId });
                    dto.QueryOption.Values.Add(
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(currentUser),
                            Column = new ExplorerQueryColumn
                            {
                                Id = DefaultColumnIDs.LoanedBy,
                                Type = Contract.TemplateManagement.ColumnType.PeopleOrGroup
                            }
                        }
                    );
                }
                else if (!permission.IsAdmin)
                {
                    var permissionTopLocationIds = physicalPermission?.ScopeIds ?? new List<Guid>();
                    var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(permissionTopLocationIds).Select(id => id.ToString());
                    dto.QueryOption.Values.Add(
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(bottomLocationIdsUnderTopLocations),
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.LocationId,
                            }
                        }
                    );
                }

                return dto;
            }

            async Task<long> GetPhysicalLoanTotalAsync()
            {
                var basicQueryDto = await GetBasicQueryDtoAsync();
                return (await explorerQueryService.QueryDataListWithTotalAsync(basicQueryDto)).PagingInfo.Total;
            }

            var loanTotal = await GetPhysicalLoanTotalAsync();
            Logger.Info($"The physical loan total: [{loanTotal}].");
            return loanTotal;
        }

        public static async Task<long> GetPhysicalDestructionPenddingTotalAsync()
        {
            var permission = await GetUserPermissionAsync();

            var physicalPermission = permission.ScopePermissionInfo.Find(item => item.DataSourceType == SourceFlag.Physical);

            Logger.Info($"The current user: [{LogonUserId}] has physical permission: [{physicalPermission != null}]");

            if (physicalPermission == null)
            {
                Logger.Info($"The current user [{LogonUserId}] not has physical permission.");
                return 0;
            }

            var explorerQueryService = ExplorerQueryService;

            var isPhysicalAdmin = permission.IsAdmin ||
                physicalPermission.SubPermission == SubPermissionType.Admin;

            Logger.Info($"The current user: [{LogonUserId}] is physical admin: [{isPhysicalAdmin}]");

            ExplorerQueryV3Dto GetBasicQueryDto()
            {
                var dto = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>
                        {
                            new ExplorerSearchOptionV3
                            {
                                Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Physical }),
                                Column = new ExplorerQueryColumn{ Id = QueryCloumnIds.SourceFlag }
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn{ Id = QueryCloumnIds.FileExtension },
                                Value = JsonConvert.SerializeObject(new List<string> { ((int)RMNodeLevel.PhysicalCustom).ToString(), ((int)RMNodeLevel.PhysicalBox).ToString(), ((int)RMNodeLevel.PhysicalFile).ToString() })
                            },
                            new ExplorerSearchOptionV3
                            {
                                Value = JsonConvert.SerializeObject(new List<ChoiceColumnValue>() { new ChoiceColumnValue() { Value = ((int)RMRecordStatus.Destroyed).ToString() } }),
                                Column = new ExplorerQueryColumn
                                {
                                    Id = DefaultColumnIDs.Status,
                                    Type = Contract.TemplateManagement.ColumnType.SingleChoice
                                },
                            },
                            new ExplorerSearchOptionV3
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.DestructionPickStatus },
                                Value = JsonConvert.SerializeObject(new List<PickStatusType>(){ PickStatusType.Pendding })
                            },
                        }
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = string.Empty,
                        PageSize = 1,
                    }
                };

                if (!permission.IsAdmin)
                {
                    var permissionTopLocationIds = physicalPermission?.ScopeIds ?? new List<Guid>();
                    var bottomLocationIdsUnderTopLocations = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(permissionTopLocationIds).Select(id => id.ToString());
                    dto.QueryOption.Values.Add(
                        new ExplorerSearchOptionV3
                        {
                            Value = JsonConvert.SerializeObject(bottomLocationIdsUnderTopLocations),
                            Column = new ExplorerQueryColumn
                            {
                                Id = QueryCloumnIds.LocationId,
                            }
                        }
                    );
                }

                //if (!isPhysicalAdmin)
                //{
                //    var currentUser = UserService.GetUsersByIds(new List<string> { LogonUserId });

                //    dto.QueryOption.Values.Add(
                //        new ExplorerSearchOptionV3
                //        {
                //            Value = JsonConvert.SerializeObject(currentUser),
                //            Column = new ExplorerQueryColumn
                //            {
                //                Id = DefaultColumnIDs.ModifiedBy,
                //                Type = Contract.TemplateManagement.ColumnType.PeopleOrGroup
                //            }
                //        }
                //    );
                //}

                return dto;
            }
            var basicQueryDto = GetBasicQueryDto();
            var loanTotal =  (await explorerQueryService.QueryDataListWithTotalAsync(basicQueryDto)).PagingInfo.Total;
            Logger.Info($"The physical loan total: [{loanTotal}].");
            return loanTotal;
        }

        #region Licenses
        private static List<SourceFlag> GetUserLicensesAsync()
        {
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isBoxLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box);

            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isFSLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            List<SourceFlag> sources = new List<SourceFlag>();
            if (!isSPOLicense)
            {
                sources.Add(SourceFlag.SharePointOnPrem);
            }
            if (!isBoxLicense)
            {
                sources.Add(SourceFlag.Box);
            }
            if (!isFSLicense)
            {
                sources.Add(SourceFlag.FileSystem);
            }
            if (!isGoogleLicense)
            {
                sources.Add(SourceFlag.Google);
            }
            return sources;
        }
        private static void FilterPermission(ManualApprovalQueryDefinition queryDefinition)
        {
            var sources = GetUserLicensesAsync();
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Permission,
                Value = JsonConvert.SerializeObject(sources)
            });

        }

        private static IEnumerable<SourceFlag> FilterOpusILSource(SecurityUserPermissionsDto permission)
        {
            var hasOpusILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForProduct.OpusIL);
            if(!hasOpusILLicense)
            {
                return permission.ScopePermissionInfo.Where(item => item.DataSourceType != SourceFlag.SharePoint && item.DataSourceType != SourceFlag.OneDrive && item.DataSourceType != SourceFlag.Teams).Select(item => item.DataSourceType);
            }
            if (!TeamsPermissionHelper.HasUpgradeTeamsFeature())
            {
                return permission.ScopePermissionInfo.Where(item => item.DataSourceType != SourceFlag.Teams).Select(item => item.DataSourceType);
            }
            return permission.ScopePermissionInfo.Select(item => item.DataSourceType);
        }
        #endregion

        #region GoogleOne
        public static async Task<List<DashboardKeyValue<long>>> GetManagedRecordsCountForGGOneAsync()
        {
            var acitveCount = 0L;
            var destoryedCount = 0L;
            var archivedCount = 0L;

            var permission = await GetUserPermissionAsync(true);

            acitveCount = DashboardQueries[SourceFlag.GGControl].GetSourceActiveCount(permission);
            //destoryedCount += DashboardQueries[SourceFlag.GGControl].GetSourceDestroyedCount(permission);
            //archivedCount += DashboardQueries[SourceFlag.GGControl].GetSourceArchivedCount(permission);
            var tempArchivedCount = await RMArchiveGDInfoDao.GetGoogleArchivedFileCount4DashboardAsync();
            archivedCount = (int)(tempArchivedCount * 1000); // convert 0.001 to 1
            destoryedCount = await RMArchiveGDInfoDao.GetGoogleDeletedFileCount4DashboardAsync();
            Logger.Info($"archive count, native:{tempArchivedCount}, archive:{archivedCount}");
            return new List<DashboardKeyValue<long>>
            {
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ActiveRecords"), acitveCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_ArchivedRecords"), archivedCount),
                new DashboardKeyValue<long>(I18NEntity.GetString("RM_DSB_DestroyedRecords"), destoryedCount),
            };
        }
        public static async Task<List<DashboardKeyValue<int>>> GetManualApprovalStatusForGGOneAsync()
        {
            var result = new List<DashboardKeyValue<int>>();

            var queryDefinition = new ManualApprovalQueryDefinition();
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.GControlApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Permission,
                Value = JsonConvert.SerializeObject(new List<SourceFlag> { SourceFlag.Google })
            });
            var waitingApprovalCount = await ManualApprovalQuerier.CountAsync(queryDefinition);
            result.Add(new DashboardKeyValue<int>(I18NEntity.GetString(DashboardI18ns.ApprovalStatusI18ns[SOApproveDBStatus.WaitingApprove]), waitingApprovalCount));

            return result;
        }
        #endregion
    }
}
