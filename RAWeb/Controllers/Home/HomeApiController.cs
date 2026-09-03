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
using AvePoint.GCommon.Contract.Server.Service;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Home;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Models;
using AvePoint.RA.Web.Models.Home;
using AvePoint.RA.Web.Models.Resource;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using Google.Apis.Storage.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;
using AvePoint.GCommon.Utility.Cloud;
using System.Management.Automation.Language;
using AvePoint.RA.Contract.Multi_Geo;

namespace AvePoint.RA.Web.Controllers.Home
{
    [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, preferred: false)]
    public class HomeApiController : BaseApiController
    {
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);

        private ITenantService _tenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _tenantService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        public IRMFunctionSettingDao RMFunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private List<ResourceKeys> RestoreReportPageList = new()
        {
            ResourceKeys.RC_RestoreReport_Management,
            ResourceKeys.RC_RestoreReport_Profile,
            ResourceKeys.RC_RestoreReport_ShowReport,
            ResourceKeys.RC_RestoreReport_ViewDetail,
            ResourceKeys.RC_RestoreReport_Create,
            ResourceKeys.RC_RestoreReport_Edit,
        };
        private List<ResourceKeys> ActionAuditPageList = new()
        {
            ResourceKeys.RC_ActionAuditReport_Management,
            ResourceKeys.RC_ActionAuditReport_Profile,
            ResourceKeys.RC_ActionAuditReport_ShowReport,
            ResourceKeys.RC_ActionAuditReport_ViewDetail,
            ResourceKeys.RC_ActionAuditReport_Create,
            ResourceKeys.RC_ActionAuditReport_Edit,
        };
        private List<ResourceKeys> TermUsagePageList = new()
        {
            ResourceKeys.RC_TermUsageReport_Management,
            ResourceKeys.RC_TermUsageReport_Profile,
            ResourceKeys.RC_TermUsageReport_ShowReport,
            ResourceKeys.RC_TermUsageReport_ViewDetail,
            ResourceKeys.RC_TermUsageReport_Create,
            ResourceKeys.RC_TermUsageReport_Edit,
        };
        private List<ResourceKeys> TimeFrameFileReportPageList = new()
        {
            ResourceKeys.RC_TimeFrameFileReport_Management,
            ResourceKeys.RC_TimeFrameFileReport_Profile,
            ResourceKeys.RC_TimeFrameFileReport_ShowReport,
            ResourceKeys.RC_TimeFrameFileReport_ViewDetail,
        };
        private List<ResourceKeys> DueDisposalReportPageList = new()
        {
            ResourceKeys.RC_DueDisposalReport_Management,
            ResourceKeys.RC_DueDisposalReport_Profile,
            ResourceKeys.RC_DueDisposalReport_ShowReport,
            ResourceKeys.RC_DueDisposalReport_ViewDetail,
            ResourceKeys.RC_DueDisposalReport_Create,
            ResourceKeys.RC_DueDisposalReport_Edit,
        };
        private List<ResourceKeys> AvailableSpaceReportsPageList = new()
        {
            ResourceKeys.RC_AvailableSpaceReport_Management,
            ResourceKeys.RC_AvailableSpaceReport_Profile,
            ResourceKeys.RC_AvailableSpaceReport_ShowReport,
            ResourceKeys.RC_AvailableSpaceReport_ViewDetail,
        };
        [HttpPost]
        public Task<List<RMSystemModule>> GetModules()
        {
            return GetHomeModulesAsync();
        }

        [HttpPost]
        public void AddTelemetryRecord([FromBody] TelemetryDto dto)
        {
            TelemetryContext.SendToQueue(dto.Module, dto.EventType, dto.Args);
        }

        public async Task<List<RMSystemModule>> GetHomeModulesAsync()
        {
            List<RMSystemModule> modules = new List<RMSystemModule>();
            var permission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>();
            var soPermission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>();
            var discoveryPermission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryPermissionMasks>();
            var salesforceDiscoveryPermission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoverySalesforcePermissionMask>();
            var googleROTDiscoveryPermission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryGoogleROTPermissionMask>();
            var fsDiscoveryPermission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryFileSystemPermissionMask>();
            //only physical enduser & manual reviewer
            if (permission == RACommonUtility.Permission.PermissionWrappers.StandardUser)
            {
                modules = GetStandardUserModules();
            }
            else
            {
                modules = GetAllModules(permission);
            }
            return await FilterByPermission(modules, permission, soPermission, discoveryPermission, salesforcePermissionMask: salesforceDiscoveryPermission, googleROTPermissionMask: googleROTDiscoveryPermission, fsDiscoveryPermissionMask: fsDiscoveryPermission);
        }

        [HttpPost]
        public async Task<string> GetCurrentUserInfo()
        {
            using (new PerformanceScope($"HomeApiController.GetCurrentUserInfo"))
            {
                var rmIdentity = await HttpContext.Request.GetRMIdentityAsync();
                var forwardToDAORC = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter");
                if (rmIdentity != null && TenantService.CheckTenantExist(rmIdentity.TenantGroupId))
                {

                    TimeSettingModel tsm = await GeneralSettingService.GetTimeSettingModelAsync(rmIdentity.TenantGroupId);
                    var groupIds = await GetGroupIdsAsync(rmIdentity.AccountId);
                    string permission1 = string.Empty;
                    string permission2 = string.Empty;
                    string reportingPermission = string.Empty;
                    string permissionExtension = string.Empty;
                    string soPermission = string.Empty;
                    string discoveryPermission = string.Empty;
                    string salesforceDiscoveryPermission = string.Empty;
                    string googleROTDiscoveryPermission = string.Empty;
                    string fsDiscoveryPermission = string.Empty;
                    List<bool> groupStatus = new List<bool>();
                    bool isAdmin = false;
                    using (new PerformanceScope($"HomeApiController.GetUserPermission"))
                    {
                        permission1 = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>()).ToString();
                        permission2 = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMSubPermissionMasks>()).ToString();
                        permissionExtension = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionExtensionMasks>()).ToString();
                        reportingPermission = ((int)await SecurityTrimmingHelper.GetUserPermissionAsync<RMReportPermissionMasks>()).ToString();
                        soPermission = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>()).ToString();
                        discoveryPermission = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryPermissionMasks>()).ToString();
                        salesforceDiscoveryPermission = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoverySalesforcePermissionMask>()).ToString();
                        googleROTDiscoveryPermission = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryGoogleROTPermissionMask>()).ToString();
                        fsDiscoveryPermission = ((long)await SecurityTrimmingHelper.GetUserPermissionAsync<RMDiscoveryFileSystemPermissionMask>()).ToString();
                        isAdmin = permission1.ThisPermissionIsAllowed(RMPermissionMasks.PhysicalAdmin.ToString());
                        groupStatus = await SecurityTrimmingHelper.GetUserGroupsIsNewGroups();

                    }
                    bool isPrePaidConsumption = IsPrePaidConsumption();
                    bool EnableDeleteOnly = IsEnableDeleteOnlyOptionSetting();
                    bool EnableArchiveOnly = IsEnableArchiveOnlyOptionSetting();
                    bool enableMultiGeoFeature = await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao);
                    int roleType = GetUserRoleType(permission1, soPermission);
                    LoginInfo info = new LoginInfo();
                    info.UserInfo = new LoginUserInfo()
                    {
                        LogonGroupId = rmIdentity.TenantGroupId,
                        Company = rmIdentity.Company,
                        AccountNumber = rmIdentity.AccountNumber,
                        UserName = WebUtil.LogonUserDisplayName,
                        UserId = WebUtil.LogonUserId,
                        EmailAddress = WebUtil.LogOnUserName,
                        IsPhysicalAdmin = isAdmin,
                        UserGroup = JsonConvert.SerializeObject(groupIds),
                        RoleType = roleType,
                        EnableRecordsArchiver = TenantService.IsNewOpusTenant(),
                        EnableArchiverOnly = isPrePaidConsumption && EnableArchiveOnly,
                        EnableDeleteOnly = isPrePaidConsumption || EnableDeleteOnly,
                        EnableArchiverLatestVersion = isPrePaidConsumption || EnableDeleteOnly,
                        EnableArchiverVersionNotIncludeLatest = isPrePaidConsumption || EnableDeleteOnly,
                        HasArchiverLicense = TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusSO),
                        HasRecordsLicense = TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusIL),
                        HasDiscoveryLicense = TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusDiscovery),
                        HasDiscoverySalesforceLicense =  TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusSalesforceDiscovery),
                        HasGoogleLicense = TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusGoogle),
                        HasFileSystemLicense = TenantService.CheckLicenseWithAdditionalDataSource(rmIdentity.TenantGroupId, PaidForModule.FileSystem),
                        HasDiscoveryGoogleLicense = TenantService.CheckLicenseWithAdditionalProduct(rmIdentity.TenantGroupId, PaidForProduct.OpusGoogleWorkspaceDiscovery),
                        HasDiscoveryFileSystemLicense = RMDiscoveryFSLicenseHelper.HasDiscoveryFileSystemLicense(),
                        HasDiscoveryExportRowData = await RMDiscoveryOffice365LicenseHelper.IsAllowedToExportRowDataAsync(),
                        EnableCustomizationApp = TenantService.IsCustomizationAppTenant(),
                        LicenseType = (await RMAosApiClient.GetLicenseInfo(rmIdentity.TenantGroupId)).Type,
                        UseArchiverImportFile = IsUseArchiverImportFile(),
                        EnableFilelevelBackup = IsEnableDeleteOnlyOption(),
                        EnableSoftDelete = IsEnableSoftDeleteSetting(),
                        EnableDeleteOrphanData = IsEnableDeleteOrphanDataSetting(),
                        EnableApplySettingScanAll = IsEnableApplySettingAlwaysScanAll(),
                        HasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams(),
                        EnableTeamsFeature = RMKeyValueDao.EnableTeamsFeature(),
                        EnableZeroShotFeature = RMKeyValueDao.EnableZeroShotFeature(),
                        EnableAIRecommendationFeature = (await RMAosApiClient.IsEnableAIRecommendation(rmIdentity.TenantGroupId)),
                        EnableMachineLearningFeature = await LicenseHelperService.IsEnableMaestroAI(),
                        EnableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled,
                        EnableCustomRetentionSettings = RMKeyValueDao.IsEnableCustomRetentionSettings(),
                        EnableMultiGEOFeature = enableMultiGeoFeature,
                        IsMultiGeoMainDC = MultiGeoDataCenterService.IsMainDC(),
                        HasManageHoldEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.ManageHoldEndUser) ,
                        HasManagerHold = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold),
                    };
                    if (StorageDeviceService.IsDisableRetentionPeriodLimitation())
                    {
                        info.UserInfo.DisableRetentionPeriodLimitation = true;
                    }
                    info.Permission = permission1;
                    info.TimeSettingModel = JsonConvert.SerializeObject(tsm);
                    info.AvaliableSource = await GetAvailableDataSourceAsync();
                    info.UserResources = await GetResourceViaPermission(permission1, permission2, permissionExtension, soPermission, discoveryPermission, salesforceDiscoveryPermission, googleROTDiscoveryPermission, fsDiscoveryPermission, reportingPermission, roleType == (int)RMRoleType.ApplicationAdmin, groupStatus.Contains(false));
                    info.DataCenter = WebUtil.DataCenter;
                    info.ProductVersion = WebUtil.GetProductDisplayVersion();
                    info.Copyright = $"© {DateTime.UtcNow.Year} AvePoint, Inc. All Rights Reserved.";
                    info.ForwardToDAORC = forwardToDAORC;
                    info.CurrentLanguage = Thread.CurrentThread.CurrentUICulture.Name;
                    info.HasIntelligentPermission = (await LicenseHelperService.IsEnableMaestroAI() || RMKeyValueDao.EnableZeroShotFeature());
                    info.EnviromentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                    info.ChatBotPortalURL = RMGlobalConfiguration.AppConfig[RMAppSettingKey.CHAT_BOT_URL];
                    info.FileExtentionsConfig = TenantService.GetFileExtentionsConfig();
                    info.ExportResultLimit = TenantService.GetExportResultLimit();
                    info.AccessToken = rmIdentity.AccessToken;
                    info.EnableDeleteRestoredDataFeature = LicenseHelperService.IsEnableDeleteRestoreDataFeature();
                    string cdnURL = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.RES_CDN_URL];
                    if (string.IsNullOrEmpty(cdnURL))
                        info.CDNUrl = string.Empty;
                    else
                    {
                        if (cdnURL.EndsWith("/"))
                            info.CDNUrl = cdnURL.TrimEnd('/');
                        else
                            info.CDNUrl = cdnURL;
                    }
                    bool disableChatBot = rmIdentity.DisableAVA || RMKeyValueDao.DisableChatBot();
                    info.ChatBotApiURL = disableChatBot ? "" : RMGlobalConfiguration.AppConfig.GetChatBotAPIUrl();
                    info.AOSPortalURL = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL];
                    info.DisableChatBot = disableChatBot;
                    info.ExistAVAUser = rmIdentity.ExistAVAUser;

                    return JsonConvert.SerializeObject(info);
                }
            }

            return string.Empty;
        }

        [HttpGet]
        public async Task<List<ProductCardItemUIDto>> GetSwitchBar()
        {
            List<ProductCardItemUIDto> infos = [];
            try
            {
                var switchBarInfo = await RMAosApiClient.GetSwitchBarAsync(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId);
                foreach (var item in switchBarInfo.ProductCards)
                {
                    infos.Add(new ProductCardItemUIDto()
                    {
                        HiddenIcon = item.HiddenIcon,
                        DisplayName = item.DisplayName,
                        ProductIconBase64 = item.ProductIconBase64,
                        Url = item.Url,
                        ProductType = item.ProductType,
                        IsExpired = item.IsExpired,
                        NavProductName = item.NavProductName,
                        NavProductIcon = item.NavProductIcon,
                        CategoryName = item.CategoryName,
                        CategoryIcon = item.CategoryIcon,
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Get switch bar from AOS error, error message: {e}");
            }
            return infos;
        }

        [HttpPost]
        public object GetChatBotToken([FromBody] Dictionary<string, string> tokenDto)
        {
            tokenDto.TryGetValue("Token", out var tokenStr);
            var tokenHander = new JwtSecurityTokenHandler();
            var tokenInfo = tokenHander.ReadJwtToken(tokenStr);
            if (tokenInfo.ValidTo - DateTime.UtcNow < TimeSpan.FromMinutes(5))
            {
                Logger.Info("Refresh by chat bot request");
                var new_access_token = RMSSOHelper.GetAccessToken(HttpContext.Request.GetRefreshToken()?.refresh_token);
                var chatbotToken = RMSSOHelper.GetBotToken(new_access_token);
                return new { opus = new_access_token, chatbot = chatbotToken };
            }
            else
            {
                var chatbotToken = RMSSOHelper.GetBotToken(tokenStr);
                return new { opus = tokenStr, chatbot = chatbotToken };
            }
        }
        private bool IsUseArchiverImportFile()
        {
            var key = RMKeyValueDao.GetValueByKey("UseArchiverImportFile");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsPrePaidConsumption()
        {
            //string keyString = "NeedStatisticsJobSizeToAOS";
            //var keyValue = RMKeyValueDao.GetValueByKey(keyString);
            //if (keyValue != null)
            //{
            //    bool result = false;
            //    if (bool.TryParse(keyValue.Value, out result))
            //    {
            //        return result;
            //    }
            //    return false;
            //}
            //else
            //{
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                    if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                    {
                        //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //}
                return false;
            }
            catch (Exception e)
            {
                Logger.Error($"some thing went wrong when check Delete only action enabled,error{e.ToString()}");
                return false;
            }
        }
        private bool IsEnableDeleteOnlyOptionSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsEnableArchiveOnlyOptionSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableArchiveOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsEnableDeleteOrphanDataSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOrphanData");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsEnableSoftDeleteSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsEnableDeleteOnlyOption()
        {
            if (int.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
            {
                if (outputStreamLevel == (int)OutputStreamLevel.FileLevel)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsEnableApplySettingAlwaysScanAll()
        {
            var key = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.EnableApplySettingAlwaysScanAll);
            _ = bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool HasUpgradeTeams()
        {
            return RMKeyValueDao.HasUpgradeTeams();
        }

        private int GetUserRoleType(string opusILPermission, string opusSOPermission)
        {
            var roleType = opusILPermission.PermissionToRole();
            roleType = roleType > -1 ? roleType : opusSOPermission.SOPermissionToRole();
            return roleType;
        }

        private async Task<string> GetAvailableDataSourceAsync()
        {
            List<Contract.Explorer.SourceFlag> sourceFlags = new List<Contract.Explorer.SourceFlag>();
            try
            {
                sourceFlags = (await SecurityTrimmingHelper.GetAvailableDataSourceAsync()).ToList();

            }
            catch (Exception ex)
            {
                Logger.Error($"error occurred while get data source:{ex.ToString()}");
            }
            return JsonConvert.SerializeObject(sourceFlags);
        }


        private async Task<string> GetResourceViaPermission(string permission, string subPermission1, string permissionExtension, string soPermission, string discoveryPermission, string salesforceDiscoveryPermission, string googleROTDiscoveryPermission, string fsDiscoveryPermission, string reportingPermission,bool isAdmin,bool existOldGroup)
        {
            var result = "[]";
            var resultItems = new List<UIResourceItem>();
            var isPhyEndUser = !(permission.ThisPermissionIsAllowed(RMPermissionMasks.PhysicalAdmin.ToString()))
                && (permission.ThisPermissionIsAllowed(RMPermissionMasks.PhysicalEndUser.ToString()));
            if (Enum.TryParse(permission, out RMPermissionMasks userPermission))
            {
                Enum.TryParse(permissionExtension, out RMPermissionExtensionMasks userPermissionExtention);
                Enum.TryParse(soPermission, out RMSOPermissionMasks userSOPermissionExtention);
                Enum.TryParse(discoveryPermission, out RMDiscoveryPermissionMasks discoveryPermissionExtention);
                Enum.TryParse(salesforceDiscoveryPermission, out RMDiscoverySalesforcePermissionMask salesforceDiscoveryPermissionExtention);
                Enum.TryParse(googleROTDiscoveryPermission, out RMDiscoveryGoogleROTPermissionMask googleROTDiscoveryPermissionExtention);
                Enum.TryParse(reportingPermission, out RMReportPermissionMasks reportingPermissionExtention);
                Enum.TryParse(fsDiscoveryPermission, out RMDiscoveryFileSystemPermissionMask fsDiscoveryPermissionExtention);
                bool isSOOnlyLicense = !LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense;
                bool enableMultiGeoFeature = await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao);
                var items = enableMultiGeoFeature
                    ? await GResources.GetResourceViaPermissionOfMultiGeoAsync(userPermission, userSOPermissionExtention, discoveryPermissionExtention, reportingPermissionExtention, userPermissionExtention, salesforceDiscoveryPermissionExtention, googleROTDiscoveryPermissionExtention, fsDiscoveryPermissionExtention, isAdmin)
                    : await GResources.GetResourceViaPermission(userPermission, userSOPermissionExtention, discoveryPermissionExtention, reportingPermissionExtention, userPermissionExtention, salesforceDiscoveryPermissionExtention, googleROTDiscoveryPermissionExtention, fsDiscoveryPermissionExtention, isAdmin);
                foreach (var item in items)
                {
                    if ((LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense) && isPhyEndUser && item.Permission == RMPermissionMasks.PhysicalEndUser && !subPermission1.ThisSubPermissionIsAllowed(item.SubPermission.ToString()))
                    {
                        //physical end user没有此sub permission
                        continue;
                    }
                    if (!isAdmin && item.ReportPermission != RMReportPermissionMasks.None)
                    {
                        if (isSOOnlyLicense && existOldGroup)
                        {
                            reportingPermissionExtention = RMReportPermissionMasks.ActionAuditEnduser | RMReportPermissionMasks.RestoredDataEnduser;
                        }
                        if (reportingPermissionExtention == RMReportPermissionMasks.None)
                        {
                            if ((userPermission & RMPermissionMasks.ReportCenterEnduser) == RMPermissionMasks.ReportCenterEnduser)
                            {
                                reportingPermissionExtention = RMReportPermissionMasks.AccessAll;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if ((reportingPermissionExtention & item.ReportPermission) == item.ReportPermission)
                        {
                            if (RestoreReportPageList.Contains(item.Key))
                            {
                                if (((userPermissionExtention & RMPermissionExtensionMasks.GoogleEndUser) == RMPermissionExtensionMasks.GoogleEndUser
                                    || (userPermissionExtention & RMPermissionExtensionMasks.TeamsEndUser) == RMPermissionExtensionMasks.TeamsEndUser
                                    || (userPermission & RMPermissionMasks.SPOEnduser) == RMPermissionMasks.SPOEnduser
                                    || (userPermission & RMPermissionMasks.OneDriveEnduser) == RMPermissionMasks.OneDriveEnduser
                                    || (userSOPermissionExtention & RMSOPermissionMasks.ContentRepositoyEnduser) == RMSOPermissionMasks.ContentRepositoyEnduser)
                                    && await LicenseHelperService.IsNewOpus(false, false)
                                    )
                                { }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (ActionAuditPageList.Contains(item.Key))
                            {
                                if ((userSOPermissionExtention & RMSOPermissionMasks.SPOEnduser) == RMSOPermissionMasks.SPOEnduser
                                    || (userSOPermissionExtention & RMSOPermissionMasks.OneDriveEnduser) == RMSOPermissionMasks.OneDriveEnduser
                                    || (userPermissionExtention & RMPermissionExtensionMasks.TeamsEndUser) == RMPermissionExtensionMasks.TeamsEndUser
                                    || (userSOPermissionExtention & RMSOPermissionMasks.TeamsEndUser) == RMSOPermissionMasks.TeamsEndUser
                                    || (userPermission & RMPermissionMasks.SPOEnduser) == RMPermissionMasks.SPOEnduser
                                    || (userPermission & RMPermissionMasks.OneDriveEnduser) == RMPermissionMasks.OneDriveEnduser
                                    )
                                { }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (TermUsagePageList.Contains(item.Key) || TimeFrameFileReportPageList.Contains(item.Key) || DueDisposalReportPageList.Contains(item.Key))
                            {
                                if (userPermissionExtention == RMPermissionExtensionMasks.AzureFSAdmin
                                    && userSOPermissionExtention == RMSOPermissionMasks.None
                                    && discoveryPermissionExtention == RMDiscoveryPermissionMasks.None
                                    && salesforceDiscoveryPermissionExtention == RMDiscoverySalesforcePermissionMask.None
                                    && googleROTDiscoveryPermissionExtention == RMDiscoveryGoogleROTPermissionMask.None
                                    && fsDiscoveryPermissionExtention == RMDiscoveryFileSystemPermissionMask.None
                                    )
                                {
                                    continue;
                                }
                            }
                            else if (AvailableSpaceReportsPageList.Contains(item.Key))
                            {
                                if ((userPermission & RMPermissionMasks.PhysicalAdmin) == RMPermissionMasks.PhysicalAdmin)
                                {

                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    resultItems.Add(new UIResourceItem
                    {
                        Name = item.Key,
                        Value = item.Value
                    });
                }
                result = JsonConvert.SerializeObject(resultItems);
            }
            return result;
        }

        private async Task<List<string>> GetGroupIdsAsync(string userId)
        {
            try
            {
                IUserService userService = new UserService();
                return await userService.GetGroupIdsAsync(userId);
            }
            catch (Exception ex)
            {
                Logger.Warn($"user: {userId}, get group failed:{ex.ToString()}");
            }
            return new List<string>();

        }

        private static List<RMSystemModule> GetAllModules(RMPermissionMasks userPermission)
        {
            var forwardToDAORC = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_URL], "forwardto/target?product=ReportCenter");
            var myRequestI18n = IsPhysicalEndUser(userPermission) ? I18NEntity.GetString("RM_PRM_MyRequest") : I18NEntity.GetString("RM_PRM_RequestManagement_PageTitle");
            return new List<RMSystemModule>()
                    {
                        new RMSystemModule() {
                            title = I18NEntity.GetString("RM_Home_Module_BusinessClassification"),
                            description = I18NEntity.GetString("RM_Home_Module_BusinessClassificationDesc"),
                            iconClass = RMSystemModuleIconClass.Business_Classification_Management,
                            links = new List<RMSystemModuleLink>()
                            {
                                new RMSystemModuleLink(ResourceKeys.BCM_TermManagement, I18NEntity.GetString("RM_TM_PageTitle"), ResourceKeys.BCM_TermManagement.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.BCM_ContentRepositoryManagement, I18NEntity.GetString("RM_SPS_SharePointSettings"), ResourceKeys.BCM_ContentRepositoryManagement.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.BCM_RecordsExplorer, I18NEntity.GetString("RM_BCM_PageTitle_Explorer"), ResourceKeys.BCM_RecordsExplorer.ToUrl(GResources.RouterUrl_Root)),
                            },
                        },
                        new RMSystemModule() {
                            title = I18NEntity.GetString("RM_Home_Module_RetentionDisposal"),
                            description = I18NEntity.GetString("RM_Home_Module_RetentionDisposalDesc"),
                            iconClass = RMSystemModuleIconClass.Retention_and_Disposal_Management,
                            links = new List<RMSystemModuleLink>()
                            {
                                new RMSystemModuleLink(ResourceKeys.RDM_RuleManagement, I18NEntity.GetString("RM_RDM_RuleManagement"), ResourceKeys.RDM_RuleManagement.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RDM_ManualApprovalReview, I18NEntity.GetString("RM_DAM_ManualApprovalReview"), ResourceKeys.RDM_ManualApprovalReview.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RDM_MAProcessesManagement, I18NEntity.GetString("RM_RDM_WorkFlowManagement"), ResourceKeys.RDM_MAProcessesManagement.ToUrl(GResources.RouterUrl_Root))
                            },
                        },
                        new RMSystemModule() {
                            title = I18NEntity.GetString("RM_Home_Module_ReportCenter"),
                            description = I18NEntity.GetString("RM_Home_Module_ReportCenterDesc"),
                            iconClass = RMSystemModuleIconClass.RM_Report_center,
                            links = new List<RMSystemModuleLink>()
                            {
                                new RMSystemModuleLink(ResourceKeys.RC_Dashboard, I18NEntity.GetString("RM_DSB_PageTitle"), ResourceKeys.RC_Dashboard.ToUrl()),
                                new RMSystemModuleLink(ResourceKeys.RC_DueDisposalReport_Management, I18NEntity.GetString("RM_RC_DueDisposal_PageTitle"), ResourceKeys.RC_DueDisposalReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_TermUsageReport_Management, I18NEntity.GetString("RM_TermUsageReport_PageTitle"), ResourceKeys.RC_TermUsageReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_RuleUsageReport_Management, I18NEntity.GetString("RM_RC_RUR_PageTitle"),ResourceKeys.RC_RuleUsageReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_TimeFrameFileReport_Management, I18NEntity.GetString("RM_JS_RC_TimeFrame_Title"),ResourceKeys.RC_TimeFrameFileReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_AvailableSpaceReport_Management, I18NEntity.GetString("RM_RC_AvailableSpaceReport_PageTitle"), ResourceKeys.RC_AvailableSpaceReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_AuditReport_Management, I18NEntity.GetString("RM_RC_Audit_PageTitle"), ResourceKeys.RC_AuditReport_Management.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.RC_ActionReport_Management, I18NEntity.GetString("RM_RC_DAO_ReportCenter"), forwardToDAORC, "_blank"),
                            },
                        },
                        new RMSystemModule() {
                            title = I18NEntity.GetString("RM_Home_Module_PhysicalRecordManagement"),
                            description = I18NEntity.GetString("RM_Home_Module_PhysicalRecordManagementDes"),
                            iconClass = RMSystemModuleIconClass.Physical_Record_Management,
                            links = new List<RMSystemModuleLink>()
                            {
                                new RMSystemModuleLink(ResourceKeys.PRM_LocationManagement, I18NEntity.GetString("RM_LM_PageTitle"),ResourceKeys.PRM_LocationManagement.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.PRM_RecordsExplorer ,I18NEntity.GetString("RM_PRM_RecordsExplorer_PageTitle"),ResourceKeys.PRM_RecordsExplorer.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.PRM_TemplateManagement ,I18NEntity.GetString("RM_TemplateManage_PageTitle"),ResourceKeys.PRM_TemplateManagement.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.PRM_MyRequest ,myRequestI18n,ResourceKeys.PRM_MyRequest.ToUrl(GResources.RouterUrl_Root)),
                            },
                        }
                    };
        }

        private static bool IsPhysicalEndUser(RMPermissionMasks userPermission)
        {
            var isEnduser = (userPermission & RMPermissionMasks.PhysicalEndUser) == RMPermissionMasks.PhysicalEndUser;
            return isEnduser && ((userPermission & RMPermissionMasks.PhysicalAdmin) != RMPermissionMasks.PhysicalAdmin);
        }

        private static List<RMSystemModule> GetStandardUserModules()
        {
            return new List<RMSystemModule>()
                    {
                        new RMSystemModule() {
                            title = I18NEntity.GetString("RM_Home_Module_RecordsManagement"),
                            description = I18NEntity.GetString("RM_Home_Module_RecordsManagementDes"),
                            iconClass = RMSystemModuleIconClass.Physical_Record_Management,
                            links = new List<RMSystemModuleLink>()
                            {
                                new RMSystemModuleLink(ResourceKeys.RDM_ManualApprovalReview, I18NEntity.GetString("RM_DAM_ManualApprovalReview"), ResourceKeys.RDM_ManualApprovalReview.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.PRM_RecordsExplorer, I18NEntity.GetString("RM_PRM_RecordsExplorer_PageTitle"), ResourceKeys.PRM_RecordsExplorer.ToUrl(GResources.RouterUrl_Root)),
                                new RMSystemModuleLink(ResourceKeys.PRM_MyRequest, I18NEntity.GetString("RM_PRM_MyRequest"), ResourceKeys.PRM_MyRequest.ToUrl(GResources.RouterUrl_Root)),
                            },
                        }
                    };
        }

        private async Task<List<RMSystemModule>> FilterByPermission(List<RMSystemModule> mSystemModules, RMPermissionMasks mPermissionMasks, RMSOPermissionMasks soPermissionMasks, RMDiscoveryPermissionMasks discoveryPermissionMasks, RMDiscoverySalesforcePermissionMask salesforcePermissionMask, RMDiscoveryGoogleROTPermissionMask googleROTPermissionMask, RMDiscoveryFileSystemPermissionMask fsDiscoveryPermissionMask = RMDiscoveryFileSystemPermissionMask.None)
        {
            List<RMSystemModule> result = new List<RMSystemModule>();
            bool enableMultiGeoFeature = await RMFunctionSettingDao.IsEnableMultiGeoFeature(RMKeyValueDao);
            var userResource = enableMultiGeoFeature
                ? await GResources.GetResourceViaPermissionOfMultiGeoAsync(mPermissionMasks, soPermissionMasks, discoveryPermissionMasks, RMReportPermissionMasks.None, salesforceDiscoveryPermissionMasks: salesforcePermissionMask, googleROTPermissionMasks: googleROTPermissionMask, fsDiscoveryPermissionMasks: fsDiscoveryPermissionMask)
                : await GResources.GetResourceViaPermission(mPermissionMasks, soPermissionMasks, discoveryPermissionMasks, RMReportPermissionMasks.None, salesforceDiscoveryPermissionMasks: salesforcePermissionMask, googleROTPermissionMasks: googleROTPermissionMask, fsDiscoveryPermissionMasks: fsDiscoveryPermissionMask);
            List<string> needRemved = new List<string>();
            foreach (var module in mSystemModules)
            {
                var links = module.links.Where(l => userResource.Any(r => r.Key == l.key)).ToList();
                if (links.Count == 0)
                {
                    needRemved.Add(module.title);
                }
                else
                {
                    module.links = links;
                }
            }

            result = mSystemModules.Where(sm => !needRemved.Contains(sm.title)).ToList();
            return result;
        }
    }
}