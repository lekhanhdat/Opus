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
using AvePoint.GCommon.Contract.Server.CreateContainer.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Cache;
using AvePoint.RA.DB.SecurityTrimming.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SecurityTrimming
{


    public class RMSecurityTrimmingHelper : IRMSecurityTrimmingHelper
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public string CustomerId => TenantLocalValue.LogonGroupId;

        public string UserId => TenantLocalValue.LogonUserId;

        public bool EnableCache => true;
        private AuthCache authCache;
        public ILnkUserGroupDao LnkUserGroupDao { get { return (ILnkUserGroupDao)PlatformWindsorManager.GetService(typeof(ILnkUserGroupDao)); } }
        public ITenantInfoDao TenantDao { get { return (ITenantInfoDao)PlatformWindsorManager.GetService(typeof(ITenantInfoDao)); } }
        public IRMSecurityGroupMembershipDao SecurityGroupMembershipDao { get { return (IRMSecurityGroupMembershipDao)PlatformWindsorManager.GetService(typeof(IRMSecurityGroupMembershipDao)); } }
        public IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao { get { return (IRMScopeRoleAssignmentDao)PlatformWindsorManager.GetService(typeof(IRMScopeRoleAssignmentDao)); } }
        public IRMSecurityGroupDao RMSecurityGroupDao { get { return (IRMSecurityGroupDao)PlatformWindsorManager.GetService(typeof(IRMSecurityGroupDao)); } }
        public IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao { get { return (IRMCustomizeConnectorContentSourceDao)PlatformWindsorManager.GetService(typeof(IRMCustomizeConnectorContentSourceDao)); } }
        public IRMKeyValueDao RMKeyValueDao { get { return (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao)); } }
        public IRMCache RMCache { get { return (IRMCache)PlatformWindsorManager.GetService(typeof(IRMCache)); } }
        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private string permissionKey
        {
            get
            {
                return $"{CacheKeyPrefix.PERMISSION_MASK}_{CustomerId}";
            }
        }
        public RMSecurityTrimmingHelper()
        {
            authCache = new AuthCache();
            //ArgumentCheck.NotNullOrWhiteSpace(CustomerId, nameof(CustomerId));
            //ArgumentCheck.NotNullOrWhiteSpace(UserId, nameof(UserId));
        }
        public async Task<T> GetUserPermissionAsync<T>(bool checkLicense = true) where T : struct
        {
            ArgumentCheck.NotNullOrWhiteSpace(CustomerId, nameof(CustomerId));
            ArgumentCheck.NotNullOrWhiteSpace(UserId, nameof(UserId));

            Func<Task<string>> func;
            if (checkLicense)
            {
                func = CalcPermisionWithLicenseAsync<T>;
            }
            else
            {
                func = CalcPermisionWithoutLicenseAsync<T>;
            }
            var cacheKey = $"{typeof(T).Name}-{UserId}";
            string permission = await authCache.GetAsync(permissionKey, cacheKey, func);
            return permission.Convert2Permission<T>();
        }
        public async Task<RMSecurityTrimmingCheckResult> CheckAsync(IList<SourceFlag> sourceFlags, bool isGlobalSecurityTrimmingEnabled = true)
        {
            var result = new RMSecurityTrimmingCheckResult();
            if (!isGlobalSecurityTrimmingEnabled) return result;
            var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
            if (!sourceFlags.Any(s => defaultContianerIdSources.Contains(s))) return result;

            var userAndGroupIds = await GetCurrentUserAndGroupsAsync();
            var permissions = await GetUserPermissionAsync<RMPermissionMasks>(false);
            var permissionsExtension = await GetUserPermissionAsync<RMPermissionExtensionMasks>(false);

            var containers = await ScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupIds);
            var allAdmin = true;
            var containerSources = SourceFlagHelper.GetDefaultContainerIdSource();
            foreach (var sourceFlag in sourceFlags)
            {
                if (!containerSources.Contains(sourceFlag)) continue;

                var isAdmin = true;
                if (sourceFlag == SourceFlag.SharePoint)
                {
                    isAdmin = permissions.HasPermission(RMPermissionMasks.SPOAdmin);
                    logger.Info($"isSPOAdmin: {isAdmin}");
                    //result.DataSources[sourceFlag] = GetSourceCheckResult(sourceFlag, isAdmin, containers);
                }
                else if (sourceFlag == SourceFlag.OneDrive)
                {
                    isAdmin = permissions.HasPermission(RMPermissionMasks.OneDriveAdmin);
                    logger.Info($"isOneDriveAdmin: {isAdmin}");
                    //result.DataSources[sourceFlag] = GetSourceCheckResult(sourceFlag, isAdmin, containers);
                }
                else if (sourceFlag == SourceFlag.Exchange)
                {
                    isAdmin = permissions.HasPermission(RMPermissionMasks.EXOAdmin);
                    logger.Info($"isEXOAdmin: {isAdmin}");
                    //result.DataSources[sourceFlag] = GetSourceCheckResult(sourceFlag, isAdmin, containers);
                }
                else
                {
                    switch(sourceFlag)
                    {
                        case SourceFlag.Teams:
                            isAdmin = permissionsExtension.HasPermission(RMPermissionExtensionMasks.TeamsAdmin);
                            logger.Info($"isTeamsAdmin: {isAdmin}");
                            break;
                        case SourceFlag.Physical:
                            var isSupperAdmin = RMSecurityGroupDao.IsSupperAdminUser(userAndGroupIds);
                            isAdmin = permissions.HasPermission(RMPermissionMasks.PhysicalAdmin) && isSupperAdmin;
                            logger.Info($"isPhyAdmin: {isAdmin}");
                            break;
                        default:
                            break;
                    }
                }
                if(sourceFlag == SourceFlag.Physical)
                {
                    var isPhysicalAdmin = permissions.HasPermission(RMPermissionMasks.PhysicalAdmin);
                    var sourceCheck = GetSourceCheckResult(sourceFlag, isAdmin, containers);
                    if (isPhysicalAdmin)
                    {
                        var physicalLocationIds = sourceCheck.Containers.Select(c => Guid.TryParse(c, out Guid r) ? r : Guid.Empty).ToList();
                        var bottomLocationIds = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(physicalLocationIds).Select(_ => _.ToString()).ToList();
                        sourceCheck.Containers = bottomLocationIds;
                    }
                    else
                    {
                        var allTopLocationIds = await RMLocationDao.GetAllTopLocationIds();
                        var bottomLocationIds = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(allTopLocationIds).Select(_ => _.ToString()).ToList();
                        sourceCheck.Containers = bottomLocationIds;
                    }
                    result.DataSources[sourceFlag] = sourceCheck;
                }
                else
                {
                    result.DataSources[sourceFlag] = GetSourceCheckResult(sourceFlag, isAdmin, containers);
                }
                allAdmin = allAdmin && isAdmin;
            }

            result.NeedCheck = !allAdmin;

            return result;
        }

        public static bool IsGlobalSecurityTrimmingEnabled()
        {
            bool enableSecurityTrimming = false;
            bool.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.ENABLE_SECURITY_TRIMMING], out enableSecurityTrimming);
            return enableSecurityTrimming;
        }

        public async Task<List<SourceFlag>> GetAllAvailableSourceFlagsFromDbAsync()
        {
            var contentSources = (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)).ToList();
            var allDataSource = contentSources.ConvertAll(item => (SourceFlag)item.Flag);
            allDataSource = allDataSource.Where(a => a != SourceFlag.All && a != SourceFlag.None && a != SourceFlag.LifecycleRetention).ToList();
            var permission = await GetUserPermissionAsync<RMPermissionMasks>();
            var gControlPermission = await TenantService.HasInitGControlPlatForm();
            allDataSource = permission.RemoveNoPermissionFourceFlags(allDataSource);
            var hasManagerHoldPermission = await DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);
            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.AzureFileShare).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Box).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser) || gControlPermission) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Google).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Teams).ToList();
            }
            return allDataSource;
        }

        public async Task<List<RMCustomizeConnectorContentSource>> GetAllAvailableDataSourceFromDbAsync()
        {
            var contentSources = (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)).ToList();
            var allDataSource = contentSources.ConvertAll(item => (SourceFlag)item.Flag);
            allDataSource = allDataSource.Where(a => a != SourceFlag.All && a != SourceFlag.None && a != SourceFlag.LifecycleRetention).ToList();
            var permission = await GetUserPermissionAsync<RMPermissionMasks>();
            allDataSource =  permission.RemoveNoPermissionFourceFlags(allDataSource);
            var hasManagerHoldPermission = await DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);


            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.AzureFileShare).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Box).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser)) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Google).ToList();
            }
            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser) || !RMKeyValueDao.HasUpgradeTeams()) && !hasManagerHoldPermission)
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Teams).ToList();
            }

            var filterSources = allDataSource.Where(s => s != SourceFlag.GGControl).ToList();

            return contentSources.Where(item => filterSources.Contains((SourceFlag)item.Flag)).ToList();
        }

        public async Task<IList<SourceFlag>> GetAvailableDataSourceAsync()
        {
            List<SourceFlag> result = new List<SourceFlag>();

            var allDataSource = EnumObject.GetAllDataSource();
            allDataSource = allDataSource.Where(a => a != SourceFlag.All && a != SourceFlag.None).ToList();
            var permission = await GetUserPermissionAsync<RMPermissionMasks>();
            result = permission.RemoveNoPermissionFourceFlags(allDataSource);

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSEndUser)))
            {
                result = result.Where(item => item != SourceFlag.AzureFileShare).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxEndUser)))
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Box).ToList();
            }

            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser)))
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Google).ToList();
            }
            if (!(await DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser)))
            {
                allDataSource = allDataSource.Where(item => item != SourceFlag.Teams).ToList();
            }
            return result;
        }

        public async Task<bool> DoesUserHasThisPermissionAsync<T>(T permissionMask, PermissionJoinType joinType = PermissionJoinType.And) where T : struct
        {
            bool result = false;
            var permission = await GetUserPermissionAsync<T>();
            switch (joinType)
            {
                case PermissionJoinType.And:
                    result = permission.UserHasThisPermission(permissionMask);
                    break;
                case PermissionJoinType.Any:
                    List<T> masks = permissionMask.SplitPermission();
                    result = masks.Any(m => permission.UserHasThisPermission(m));
                    break;
            }
            return result;
        }

        public RMSecurityTrimmingCheckResult GetContentScope(IList<SourceFlag> flags)
        {
            throw new NotImplementedException();
        }

        public RMSecurityTrimmingCheckResult GetTermScope()
        {
            throw new NotImplementedException();
        }

        public RMSecurityTrimmingCheckResult GetTermScopeByContentScope(string contentScopeId, DataScope scope)
        {
            throw new NotImplementedException();
        }

        public List<RMSecurityGroup> GetSecurityGroupsByContentScope(List<RMSecurityGroup> securityGroups, SourceFlag sourceFlag, bool excludeBuiltIn)
        {
            if (securityGroups.Any(s => s.RoleId == 1))
            {
                securityGroups = RMSecurityGroupDao.GetAllGroup();
            }
            if (excludeBuiltIn && securityGroups.Any(s => s.RoleId != 1 && s.RoleId != 2))
            {
                securityGroups = securityGroups.Where(s => s.RoleId != 1 && s.RoleId != 2).ToList();
            }
            return RMSecurityGroupDao.GetSecurityGroupsBySource(securityGroups, sourceFlag);
        }

        public List<RMSecurityGroup> TrimEndUserAndFunctionSecurityGroups(List<RMSecurityGroup> securityGroups)
        {
            return RMSecurityGroupDao.TrimEndUserAndFunctionSecurityGroups(securityGroups);
        }

        public List<int> GetSecurityGroupsByContentScope(List<string> containerIds, SourceFlag sourceFlag)
        {
            var containerGuids = containerIds.Select(c => Guid.TryParse(c, out Guid r) ? r : Guid.Empty).ToList();
            return ScopeRoleAssignmentDao.GetAllGroupsByContainerId(containerGuids, (int)sourceFlag);
        }

        public async Task<List<Guid>> GetRuleScopeAsync()
        {
            var userAndGroupIds = await GetCurrentUserAndGroupsAsync();
            return RMSecurityGroupDao.GetSecurityGroupRuleContainers(userAndGroupIds);
        }

        public List<Guid> GetRuleScopeByTermId(string customerId, string userId, string termId)
        {
            return RMSecurityGroupDao.GetSecurityGroupRuleContainers(int.Parse(termId), out int securityGroupId);
        }
        public List<Guid> GetRuleScopeByRuleId(string customerId, string userId, Guid ruleId)
        {
            return RMSecurityGroupDao.GetSecurityGroupRuleContainers(ruleId);
        }

        private RMSecurityTrimmingSourceCheckResult GetSourceCheckResult(SourceFlag sourceFlag, bool isAdmin, Dictionary<int, List<Guid>> containers)
        {
            return new RMSecurityTrimmingSourceCheckResult()
            {
                NeedCheck = !isAdmin,
                Containers = containers.ContainsKey((int)sourceFlag) ? containers[(int)sourceFlag].Select(o => o.ToString()).ToList() : new List<string>()
            };
        }

        /// <summary>
        /// get the current user id and groups id belongs to.
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetCurrentUserAndGroupsAsync()
        {
            var userAndGroupIds = new List<string>
            {
                UserId
            };
            var groupUniqueIds = await LnkUserGroupDao.GetAllGroupIdsAsync(UserId);
            if (groupUniqueIds.Count > 0)
            {
                userAndGroupIds.AddRange(groupUniqueIds);
            }
            return userAndGroupIds;
        }

        private async Task<List<long>> GetPermissionGroupAsync<T>()
        {
            var userAndGroupIds = await GetCurrentUserAndGroupsAsync();
            var permission = SecurityGroupMembershipDao.GetAllPermissoinsByUser(userAndGroupIds);
            return GetPermissionByType<T>(permission);
        }

        private List<long> GetPermissionByType<T>(List<PermissionMask> permissions)
        {
            List<long> result = new List<long>();
            if (typeof(T) == typeof(RMPermissionMasks))
            {
                result = permissions.Select(p => p.PermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMSubPermissionMasks))
            {
                result = permissions.Select(p => p.SubPermission1).ToList();
            }
            else if (typeof(T) == typeof(RMPermissionExtensionMasks))
            {
                result = permissions.Select(p => p.PermissionExtensionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMSOPermissionMasks))
            {
                result = permissions.Select(p => p.SOPermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMDiscoveryPermissionMasks))
            {
                result = permissions.Select(p => p.DiscoveryPermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMDiscoverySalesforcePermissionMask))
            {
                result = permissions.Select(p => p.SalesforceDiscoveryPermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMDiscoveryGoogleROTPermissionMask))
            {
                result = permissions.Select(p => p.GoogleROTDiscoveryPermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMDiscoveryFileSystemPermissionMask))
            {
                result = permissions.Select(p => p.FSDiscoveryPermissionMasks).ToList();
            }
            else if (typeof(T) == typeof(RMReportPermissionMasks))
            {
                result = permissions.Select(p => p.ReportingPermission).ToList();
            }

            return result;
        }

        private async Task<string> CalcPermisionWithLicenseAsync<T>() where T : struct
        {
            var permissionGroup = await GetPermissionGroupAsync<T>();
            if (permissionGroup.Count == 0)
            {
                //access denied
                return default(T).ToString();
            }
            var permissionList = permissionGroup.CombinePermissions<T>().SplitPermission();

            var result = await TenantDao.CalcPermissionsWithModuleAsync<T>(CustomerId, permissionList);
            return result.Count > 0 ? result.PackerPermissions().ToString() : default(T).ToString();
        }

        private async Task<string> CalcPermisionWithoutLicenseAsync<T>()
        {
            var permissionGroup = await GetPermissionGroupAsync<T>();
            return permissionGroup.CombinePermissions<T>().ToString();
        }

        void IRMSecurityTrimmingHelper.DisableCache()
        {
            authCache.RefreshCache(true);
        }

        public async Task<bool> EqualsThisPermission<T>(T mask) where T : struct
        {
            var permissions = await GetUserPermissionAsync<T>();
            return (dynamic)permissions == (dynamic)mask;
        }

        public Task RemovePermissionCacheAsync(List<string> fields = null)
        {
            return authCache.RemoveAsync(permissionKey, fields);
        }

        public async Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync()
        {
            Func<Task<string>> func = async () => {try {
                
                    var userAndGroupId = await GetCurrentUserAndGroupsAsync();
                    var permissionDto = RMSecurityGroupDao.GetAllSecurityTerm(userAndGroupId);
                    return JsonConvert.SerializeObject(permissionDto);
                }
                catch(Exception e){
                    logger.Warn(e.ToString());
                    return "";
                }
            };
            var cacheKey = $"{CacheKeyPrefix.SecurityTermCacheKeyPrefix}_{UserId}";
            string permission = await authCache.GetAsync(permissionKey, cacheKey, func);
            return JsonConvert.DeserializeObject<SecurityTermPermissionDto>(permission);
        }

        public List<Guid> GetRuleScopeBySecurityGroupIds(List<int> securityGroupIds)
        {
            return RMSecurityGroupDao.GetSecurityGroupRuleContainerIds(securityGroupIds);
        }

        public async Task<FunctionSubPermission> GetUserRestoreCenterFunctionPermissionAsync()
        {
            var soPermissionMasks = await GetUserPermissionAsync<RMSOPermissionMasks>();
            if (soPermissionMasks.HasFlag(RMSOPermissionMasks.RestoreCenterFullControl))
            {
                return FunctionSubPermission.RestoreCenterFullControl;
            }
            else if (soPermissionMasks.HasFlag(RMSOPermissionMasks.RestoreCenterExport))
            {
                return FunctionSubPermission.RestoreCenterExport;
            }
            else if (soPermissionMasks.HasFlag(RMSOPermissionMasks.RestoreCenterSearch))
            {
                return FunctionSubPermission.RestoreCenterSearch;
            }
            else
            {
                return FunctionSubPermission.None;
            }


            //return ((long)soPermissionMasks >> 28) switch
            //{
            //    ((long)RMSOPermissionMasks.CommonModuleAccess >> 28) => FunctionSubPermission.RestoreCenterSearch,
            //    ((long)RMSOPermissionMasks.RestoreCenterExport >> 28) => FunctionSubPermission.RestoreCenterExport,
            //    ((long)RMSOPermissionMasks.RestoreCenterAdmin >> 28) => FunctionSubPermission.RestoreCenterFullControl,
            //    _ => FunctionSubPermission.None,
            //};

        }

        public async Task<List<bool>> GetUserGroupsIsNewGroups()
        {
            var userAndGroupIds = await GetCurrentUserAndGroupsAsync();
            var result = SecurityGroupMembershipDao.GetAllGroupStatusByUser(userAndGroupIds);
            return result;
        }

        public async Task<(List<Guid> physicalLocationPermission, bool isAdmin)> GetPhysicalLocationPermissionAsync()
        {
            try
            {
                List<Guid> physicalLocationPermission = null;
                var userIds = await GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var userPermission = RMSecurityGroupDao.GetUserScopePermissions(userIds);
                if (!userPermission.IsAdmin)
                {
                    logger.Info("start load Physical permission location ids");
                    var phyPermission = userPermission.ScopePermissionInfo?.Where(_ => _.DataSourceType == SourceFlag.Physical).FirstOrDefault() ?? new();
                    var locationScopeIds = phyPermission?.ScopeIds ?? new List<Guid>();
                    var physicalBottomPermissionIds = RMLocationDao.LoadAllLocationBottomIdUnderTopLocation(locationScopeIds);
                    physicalLocationPermission = physicalBottomPermissionIds;
                }
                return (physicalLocationPermission, userPermission.IsAdmin);
            }
            catch (Exception e)
            {
                logger.Error($"InitUserPermission have error: {e}");
                return (null, false);
            }
        }

        private async Task<List<string>> GetUserAndGroupUserIdsAsync(string userId)
        {
            try
            {
                var userAndGroupIds = new List<string>
                {
                    userId
                };
                var groupUniqueIds = await LnkUserGroupDao.GetAllGroupIdsAsync(userId);
                if (groupUniqueIds.Count > 0)
                {
                    userAndGroupIds.AddRange(groupUniqueIds);
                }
                var accounts = await AccountDao.GetUserByUserIdsAsync(userAndGroupIds);
                return accounts.Select(o => o.UserId).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when GetAccountAndGroupIds, message:{ex.ToString()}");
                return null;
            }
        }
    }
}
