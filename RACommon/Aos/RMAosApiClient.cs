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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Cloud.Sdk.Data.Aos;
using Cloud.Sdk.Data.Aos.SecurityProfile;
using Cloud.Sdk.Data.Aos.Tenant;
//using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token;
using Google.GenAI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AppType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Common.Aos
{
    /// <summary>
    /// Service 启动时需要先调用RMCloudSdk.Init(certificate)初始化, 之后才可以调用此类方法.
    /// </summary>
    public class RMAosApiClient
    {
        public const string DefaultSecurityProfile = "Default Security Profile";
        public const string IdSystemKeyVault = "id_system_keyvault";
        public const string RECORDS_PRODUCT_NAME = "CloudRecords";
        private const string CUSTOM_SETTING = "CUSTOM_SETTING";
        private const int AppProfileCacheExpiredMinutes = 30;
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMAosApiClient));
        private static readonly Dictionary<string, Tuple<CloudAos.AppProfileInfo, DateTime>> _appProfileCache = new Dictionary<string, Tuple<CloudAos.AppProfileInfo, DateTime>>();
        private static readonly Dictionary<string, Tuple<CloudAos.AppProfileInfo, DateTime>> _aospAppProfileCache = new Dictionary<string, Tuple<CloudAos.AppProfileInfo, DateTime>>();
        private static readonly Dictionary<string, Tuple<CloudAos.GsuiteCustomAppProfile, DateTime>> _googleProfileCache = new Dictionary<string, Tuple<CloudAos.GsuiteCustomAppProfile, DateTime>>();
        private static readonly List<CloudAos.IdentityProviderType> SupportAppProfiles = new()
        {
            CloudAos.IdentityProviderType.CloudRecords,
            CloudAos.IdentityProviderType.Office365,
            CloudAos.IdentityProviderType.SharePoint,
            CloudAos.IdentityProviderType.Exchange,
            CloudAos.IdentityProviderType.CustomAzureApp,
            CloudAos.IdentityProviderType.CustomDelegateApp,
        };

        private static readonly List<CloudAos.IdentityProviderType> PartnerPortalAppProfiles = new()
        {
            CloudAos.IdentityProviderType.AospCustomDelegateApp,
            CloudAos.IdentityProviderType.AospSecurityAnalysis,
            CloudAos.IdentityProviderType.AospSecurityAnalysisCsp,
        };

        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private static IGlobalKeyValueService GlobalKeyValueService => PlatformWindsorManager.GetService<IGlobalKeyValueService>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private static IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        public static RMAosSecurityProfile GetCurrentAppliedSecurityProfile(string customerId)
        {
            //获取所有的 keyvault profile
            //如果有 Applying 的， 则取 applying profile
            //如果没有 Applying 的， 则取 applied profile
            var profiles = Execute(() => AosApiUtility.AosClient.SecurityProfileService.GetAllSecurityProfiles(customerId).Result);
            if (profiles.Any())
            {
                var profile = profiles.FirstOrDefault(p => p.Status == SecurityProfileStatus.Applying);
                if (profile != null)
                {
                    logger.Info("Get current applying profile from AOS,id {0}", profile.Id);
                    return RMAOSConvertUtil.Convert2SecurityProfile(profile);
                }
                profile = profiles.FirstOrDefault(p => p.Status == SecurityProfileStatus.Applied);
                if (profile != null)
                {
                    logger.Info("Get current applied profile from AOS,id {0}", profile.Id);
                    return RMAOSConvertUtil.Convert2SecurityProfile(profile);
                }
                var @default = profiles.FirstOrDefault(i => i.Name.StartsWith("Default"));
                if (@default != null)
                {
                    logger.Warn($"choose a default profile for {customerId}, profile id is {@default.Id}");
                    return RMAOSConvertUtil.Convert2SecurityProfile(@default);
                }
            }
            logger.Error($"Cannot find any security profile from aos under {customerId}.");
            return null;
        }
        public static void UpdateApplyJobStatus(string applyJobId, string tenantId, bool isFailed)
        {
            SecurityProfileApply applyInfo = new SecurityProfileApply
            {
                ApplyId = applyJobId,
                CustomerId = tenantId,
                Product = RECORDS_PRODUCT_NAME,
                State = isFailed ? TenantJobState.Failed : TenantJobState.Finished,
            };
            var result = AosApiUtility.AosClient.SecurityProfileService.UpdateApplyJobStatus(applyInfo).GetAwaiter().GetResult();
            logger.Info($"UpdateApplyJobStatus {result}");
        }

        public static RMAosAuthenticationProfile GetSPOnlineProfile(string customerId, string o365TenantId, bool useCache = false)
        {
            CloudAos.AppProfileInfo profile = GetProfile(customerId, o365TenantId, useCache);
            if (profile != null)
            {
                return RMAOSConvertUtil.Convert2AuthenticationProfile(profile);
            }
            return null;
        }

        public static RMAosGoogleAppProfile GetGoogleAppProfile(string customerId, string tenantId, bool useCache = false)
        {
            CloudAos.GsuiteCustomAppProfile profile = null;
            var isGControl = TenantService.HasInitGControlPlatForm().Result;

            logger.Info($"Start to get google app profile, is gcontorl:{isGControl}");
            if (isGControl)
            {
                profile = GetContorlPlusAppProfile(customerId, tenantId).GetAwaiter().GetResult();
                logger.Info($"Get app {profile?.Id} from gcontrol.");
            }

            if(profile == null)
            {
                profile = GetGoogleProfileInAosOrCache(customerId, tenantId, useCache);
                logger.Info($"Get app {profile?.Id} from opus.");
            }

            if (profile != null)
            {
                logger.Info($"End to get google app profile, app:{profile.Id}, is gcontorl:{isGControl}");
                return RMAOSConvertUtil.Convert2GoogleAppProfile(profile, customerId);
            }
            return null;
        }

        public static async Task<Cloud.Sdk.Data.AosModern.TokenResult?> GetGoogleTokenByAppProfileAsync(string appId, string tenantId, Cloud.Sdk.Data.AosModern.IdentityProviderType providerType, IEnumerable<string> scopes, string userName)
        {
            var isGControl = TenantService.HasInitGControlPlatForm().Result;

            logger.Info($"Start to get google token by profile, is gcontorl:{isGControl}");

            var client = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);

            if (isGControl)
            {
                return await client.ImpersonateCallerInvoke<ModernTokenApiClient, CloudAos.TokenResult?>(Cloud.Sdk.Data.Core.CallerType.GoogleControl, async (client) =>
                {
                    return await client.ModernTokenService.GetTokenByGoogleAppProfileAsync(appId, tenantId, providerType, scopes.ToList(), userName);
                });
            }

            return await client.ModernTokenService.GetTokenByGoogleAppProfileAsync(appId, tenantId, providerType, scopes.ToList(), userName);
        }

        public static CloudAos.AppProfileInfo GetSalesforceAppProfile(string customerId, string organizationId, bool useCache = false)
        {
            return GetSalesforceProfile(customerId, organizationId, useCache);           
        }

        public static async Task<List<CloudAos.AppProfileInfo>> GetSalesforceAppProfiles(string customerId)
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(customerId);
                var app = await client.AppProfileService.GetByTypesAsync([CloudAos.IdentityProviderType.OpusForSalesforce,
                    CloudAos.IdentityProviderType.OpusForSandbox]);
                return app.Where(IsActiveApp).ToList();
            }
            catch (Exception e)
            {
                logger.Error("Error getting all profiles for salesforce of Customer {0} from AOS. {1}", customerId, e.ToString());
            }
            return [];
        }

        public static AppType ConvertIdentityTypeToAppType(CloudAos.IdentityProviderType providerType)
        {
            return providerType switch
            {
                CloudAos.IdentityProviderType.Office365 => AppType.Office365,
                CloudAos.IdentityProviderType.SharePoint => AppType.SharePoint,
                CloudAos.IdentityProviderType.Exchange => AppType.Exchange,
                CloudAos.IdentityProviderType.CustomAzureApp => AppType.CustomAzureApp,
                CloudAos.IdentityProviderType.CustomDelegateApp => AppType.CustomDelegateApp,
                CloudAos.IdentityProviderType.CloudRecords => AppType.CloudRecords,
                CloudAos.IdentityProviderType.AospSecurityAnalysis => AppType.AOSPTokenApp,
                CloudAos.IdentityProviderType.AospSecurityAnalysisCsp => AppType.AOSPTokenApp,
                CloudAos.IdentityProviderType.AospCustomDelegateApp => AppType.AospCustomDelegateApp,
                _ => AppType.Office365,
            };
            }

        private static int GetAospProfilePriority(CloudAos.IdentityProviderType type)
        {
            return type switch
            {
                CloudAos.IdentityProviderType.AospCustomDelegateApp => 0,
                CloudAos.IdentityProviderType.AospSecurityAnalysis => 1,
                CloudAos.IdentityProviderType.AospSecurityAnalysisCsp => 2,
                _ => int.MaxValue,
            };
        }

        /// <summary>
        /// 获取或缓存Office365的App的优先级为：
        /// All permission(Build-in App) > All permission(Custom App) > Sharepoint online permission(Build-in App) > Sharepoint online permission(Custom App)
        /// </summary>
        private static CloudAos.AppProfileInfo GetProfile(string customerId, string o365TenantId, bool useCache = false)
        {
            CloudAos.AppProfileInfo profile = null;
            Tuple<CloudAos.AppProfileInfo, DateTime> cacheObj = null;
            Guid gTenantId = new Guid(o365TenantId);
            try
            {
                if (useCache)
                {
                    lock (_appProfileCache)
                    {
                        if (_appProfileCache.TryGetValue(o365TenantId, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                        {
                            return cacheObj.Item1;
                        }
                    }
                }

                    profile = Execute(() =>
                    {
                    var appProfiles = GetHasADPermissionProfiles(customerId).Where(item => o365TenantId.Equals(item.TenantId, StringComparison.OrdinalIgnoreCase));

                    foreach (var group in appProfiles.GroupBy(p => p.TenantId))
                        {
                            if (group.Key == o365TenantId)
                            {
                                var pf = group.OrderBy(p => p.Type).FirstOrDefault();
                                if (useCache)
                                {
                                    lock (_appProfileCache)
                                    {
                                        _appProfileCache[group.Key] = Tuple.Create(pf, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                                    }
                                }

                                if (group.Key.Equals(o365TenantId, StringComparison.OrdinalIgnoreCase))
                                {
                                    return pf;
                                }
                            }
                        }
                        return null;
                    });

                    if (profile != null)
                    {
                        return profile;
                    }
                }
            catch (Exception e)
            {
                logger.Error("Error getting app profile for Office365 {0} of Customer {1} from AOS. {2}", o365TenantId, customerId, e.ToString());
            }
            return null;
        }

        private static CloudAos.AppProfileInfo GetSalesforceProfile(string customerId, string tenantId, bool useCache = false)
        {
            CloudAos.AppProfileInfo profile = null;
            Tuple<CloudAos.AppProfileInfo, DateTime> cacheObj = null;
            try
            {
                if (useCache)
                {
                    lock (_appProfileCache)
                    {
                        if (_appProfileCache.TryGetValue(tenantId, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                        {
                            return cacheObj.Item1;
                        }
                    }
                }

                profile = Execute(() =>
                {
                    var client = AosApiUtility.GetAosModernClient(customerId);
                    var appProfiles = client.AppProfileService.GetByTypesAsync([CloudAos.IdentityProviderType.OpusForSalesforce,
                        CloudAos.IdentityProviderType.OpusForSandbox]).GetAwaiter().GetResult();
                    if (appProfiles.IsNotNullOrEmpty())
                    {
                        var appProfile = appProfiles.FirstOrDefault(app => app.TenantId.EqualsIgnoreCase(tenantId) && IsActiveApp(app));
                        if (useCache)
                        {
                            lock (_appProfileCache)
                            {
                                _appProfileCache[appProfile.TenantId] = Tuple.Create(appProfile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                            }
                        }
                        if (appProfile.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            return appProfile;
                        }
                    }
                    return null;
                });

                if (profile != null)
                {
                    return profile;
                }
            }
            catch (Exception e)
            {
                logger.Error("Error getting custom app profile for Google {0} of Customer {1} from AOS. {2}", tenantId, customerId, e.ToString());
            }
            return null;
        }

        private static CloudAos.GsuiteCustomAppProfile GetGoogleProfileInAosOrCache(string customerId, string tenantId, bool useCache = false)
        {
            CloudAos.GsuiteCustomAppProfile profile = null;
            Tuple<CloudAos.GsuiteCustomAppProfile, DateTime> cacheObj = null;
            try
            {
                if (useCache)
                {
                    lock (_googleProfileCache)
                    {
                        if (_googleProfileCache.TryGetValue(tenantId, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                        {
                            return cacheObj.Item1;
                        }
                    }
                }

                profile = Execute(() =>
                {
                    var client = AosApiUtility.GetAosModernClient(customerId);
                    var appProfiles = client.AppProfileService.GetGsuiteCustomAppProfilesAsync(tenantId).GetAwaiter().GetResult();
                    if (appProfiles.IsNotNullOrEmpty())
                    {
                        var appProfile = appProfiles.FirstOrDefault();
                        logger.Info($"Get app profile {appProfile.Id} from the AOS.");
                        if (useCache)
                        {
                            lock (_googleProfileCache)
                            {
                                _googleProfileCache[appProfile.TenantId] = Tuple.Create(appProfile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                            }
                        }
                        if (appProfile.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            return appProfile;
                        }
                    }
                    return null;
                });

                if (profile != null)
                {
                    return profile;
                }
            }
            catch (Exception e)
            {
                logger.Error("Error getting custom app profile for Google {0} of Customer {1} from AOS. {2}", tenantId, customerId, e.ToString());
            }
            return null;
        }
        public static async Task<CloudAos.GsuiteCustomAppProfile> GetContorlPlusAppProfile(string customerId, string tenantId, bool useCache = false)
        {
            try
            {
                CloudAos.GsuiteCustomAppProfile profile = null;
                Tuple<CloudAos.GsuiteCustomAppProfile, DateTime> cacheObj = null;
                if (useCache)
                {
                    lock (_googleProfileCache)
                    {
                        if (_googleProfileCache.TryGetValue(tenantId, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                        {
                            return cacheObj.Item1;
                        }
                    }
                }

                logger.Info($"Try to get app profile from contorl plus.");
                profile = Execute(() =>
                {
                    var client = AosApiUtility.GetAosModernClient(customerId);
                    var appProfile = client.ImpersonateCallerInvoke<AosModernApiTenantClient, CloudAos.GsuiteCustomAppProfile?>(Cloud.Sdk.Data.Core.CallerType.GoogleControl, async (client) =>
                    {
                        var profiles = await client.AppProfileService.GetGsuiteCustomAppProfilesAsync(tenantId);
                        return profiles.FirstOrDefault();
                    }).GetAwaiter().GetResult();
                    if (appProfile != null)
                    {
                        logger.Info($"Get app profile {appProfile.Id} with type gcontorl from the AOS.");
                        if (useCache)
                        {
                            lock (_googleProfileCache)
                            {
                                _googleProfileCache[appProfile.TenantId] = Tuple.Create(appProfile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                            }
                        }
                        if (appProfile.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase))
                        {
                            return appProfile;
                        }
                    }
                    else
                    {
                        logger.Info($"Not found app profiles with type gcontorl from the AOS.");
                    }
                    return null;
                });
                return await Task.FromResult(profile);
            }
            catch(Exception ex)
            {
                logger.Error($"GetContorlPlusAppProfile Error:{ex.ToString()}");
            }
            return null;
        }
        public static CloudAos.AppProfileInfo GetAppProfile(string customerId, string o365TenantId, bool useCache = false)
        {
            return GetProfile(customerId, o365TenantId, useCache);
        }
        public static async Task<CloudAos.TenantConnectionInfo> GetO365TenantInfoByIdAsync(string tenantId)
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);

            return await Cache.TryGetAsync<CloudAos.TenantConnectionInfo>(IRMCache.Keys.Office365_Tenant_Info + "_" + tenantId, () =>
            {
                return client.TenantManagementService.GetByTenantIdAsync(tenantId);
            });
        }

        public static async Task<CloudAos.UserInfo> GetUserByPrincipalName(string userPrincipalName)
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var userInfo = await client.UserService.GetUserAsync(userPrincipalName, "AvePointRecords");
            return userInfo;
        }

        //build-in app profile在多tenant下的client id是一样的，使用app id从缓存中会获取到前一个tenant缓存的app profile，对于跨tenant的站点useCache使用false，每次从aos重新获取app profile
        public static RMAosAuthenticationProfile GetAuthProfileByAppId(string customerId, string o365TenantId, string appId, CloudAos.IdentityProviderType appType, bool useCache = false)
        {
            CloudAos.AppProfileInfo profile = GetProfileByAppId(customerId, o365TenantId, appId, appType, useCache);
            if (profile != null)
            {
                return RMAOSConvertUtil.Convert2AuthenticationProfile(profile);
            }
            return null;
        }

        public static async Task<RMAosAuthenticationProfile> GetAuthProfileByAppId(string customerId, string appId)
        {
            var client = AosApiUtility.GetAosModernClient(customerId);
            var appProfile = await client.ImpersonateCallerInvoke<AosModernApiTenantClient, CloudAos.AppProfileInfo?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
            {
                var profile = await client.AppProfileService.GetByIdAsync(appId);

                return profile;
            });

            if (appProfile.Status != CloudAos.AppProfileStatus.Active)
            {
                logger.Warn($"The app {appProfile.Name} Status is {appProfile.Status}.");
                return null;
            }

            if (appProfile != null)
            {
                return RMAOSConvertUtil.Convert2AuthenticationProfile(appProfile);
            }
            return null;
        }

        public static async Task<CloudAos.AppProfileInfo> GetAOSPAuthProfileByAppId(string customerId, string appId)
        {
            var client = AosApiUtility.GetAosModernClient(customerId);
            var appProfile = await client.ImpersonateCallerInvoke<AosModernApiTenantClient, CloudAos.AppProfileInfo?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
            {
                var profile = await client.AppProfileService.GetByIdAsync(appId);
                return profile;
            });

            if (appProfile.Status != CloudAos.AppProfileStatus.Active)
            {
                logger.Warn($"The app {appProfile.Name} Status is {appProfile.Status}.");
                return null;
            }

            return appProfile;
        }
        public static async Task<CloudAos.AppProfileInfo> GetAOSPAuthProfile(string customerId, string o365TenantId)
        {
            Tuple<CloudAos.AppProfileInfo, DateTime> cacheObj = null;
            lock (_aospAppProfileCache)
            {
                if (_aospAppProfileCache.TryGetValue(o365TenantId, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                {
                    logger.Info("this get aosp app use cache,will return it");
                    return cacheObj.Item1;
                }
            }

            var client = AosApiUtility.GetAosModernClient(customerId);
            var appProfile = await client.ImpersonateCallerInvoke<AosModernApiTenantClient, CloudAos.AppProfileInfo?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
            {
                var profiles = await client.AppProfileService.GetByTypesAsync([CloudAos.IdentityProviderType.AospSecurityAnalysis, CloudAos.IdentityProviderType.AospSecurityAnalysisCsp, CloudAos.IdentityProviderType.AospCustomDelegateApp]);
                logger.Info($"get all profiles,the profile count is:{profiles?.Count}");
                foreach (var profile in profiles)
                {
                    logger.Info($"current profile info is:customerId:{customerId},name:{profile.Name},id:{profile.Id},tenantID:{profile.TenantId},type:{(int)profile.Type}");
                }
                return profiles
                    .Where(p => string.Equals(o365TenantId, p.TenantId, StringComparison.OrdinalIgnoreCase)
                        && (p.Type != CloudAos.IdentityProviderType.AospCustomDelegateApp || p.Products.Contains(CloudAos.ModernProductType.PartnerWorkspaceOnboarding)))
                    .OrderBy(p => GetAospProfilePriority(p.Type))
                    .FirstOrDefault();
            });

            if (appProfile.Status != CloudAos.AppProfileStatus.Active)
            {
                logger.Warn($"The app {appProfile.Name} Status is {appProfile.Status}.");
                return null;
            }
            lock (_aospAppProfileCache)
            {
                _aospAppProfileCache[o365TenantId] = Tuple.Create(appProfile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                logger.Info("this get aosp app use cache,will return it");
            }
            return appProfile;
        }
        public static CloudAos.AppProfileInfo GetProfileByAppId(string customerId, string o365TenantId, string appId, CloudAos.IdentityProviderType appType, bool useCache = false)
        {
            try
            {
                var key = $"{o365TenantId}_{appType}_{appId}";
                Tuple<CloudAos.AppProfileInfo, DateTime> cacheObj = null;
                if (useCache)
                {
                    lock (_appProfileCache)
                    {
                        if (_appProfileCache.TryGetValue(key, out cacheObj) && cacheObj.Item2 > DateTime.UtcNow)
                        {
                            return cacheObj.Item1;
                        }
                    }
                }
                return Execute(() =>
                {
                    var appProfiles = GetHasADPermissionProfiles(customerId)
                    .Where(item => item.TenantId.Equals(o365TenantId, StringComparison.OrdinalIgnoreCase));

                    CloudAos.AppProfileInfo retrunProfile = null;
                    foreach (CloudAos.AppProfileInfo profile in appProfiles)
                    {
                        if (profile.TenantId == o365TenantId && profile.Type == appType && string.Equals(profile.AppClientId, appId, StringComparison.OrdinalIgnoreCase))
                        {
                            retrunProfile = profile;
                            if (useCache)
                            {
                                lock (_appProfileCache)
                                {
                                    _appProfileCache[key] = Tuple.Create(profile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    return retrunProfile;
                });

            }
            catch (Exception e)
            {
                logger.Error($"Error getting profiles by customerID {customerId},  app ID {appId} from AOS. {e.ToString()}");
            }
            return null;
        }

        public static bool ExistAppProfile(string customerId, string o365TenantId, string appId, bool useCache = false)
        {
            bool exists = false;
            try
            {
                if (useCache)
                {
                    lock (_appProfileCache)
                    {
                        exists = _appProfileCache.Values.Any(a => string.Equals(a.Item1.AppClientId, appId, StringComparison.OrdinalIgnoreCase));
                    }
                }

                if(exists)
                {
                    return exists;
                }

                var appProfiles = Execute(() =>
                {
                     return GetHasADPermissionProfiles(customerId);
                });

                foreach (CloudAos.AppProfileInfo profile in appProfiles)
                {
                    if (useCache)
                    {
                        lock (_appProfileCache)
                        {
                            _appProfileCache[$"{o365TenantId}_{profile.Type}_{appId}"] = Tuple.Create(profile, DateTime.UtcNow.AddMinutes(AppProfileCacheExpiredMinutes));
                        }
                    }

                    if (profile.TenantId == o365TenantId && string.Equals(profile.AppClientId, appId, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                    }
                }
            }
            catch (Exception e)
            {

                logger.Error($"Error getting profiles by customerID {customerId},  app ID {appId} from AOS. {e.ToString()}");
            }
            return exists;
        }

        #region AOS new UI

        public static async Task<CloudAos.AppProfileInfo> GetHighLevelPermissionAppProfile(string customerId, string tenantId)
        {
            var client = AosApiUtility.GetAosModernClient(customerId);
            var appProfiles = await client.AppProfileService.GetByTypesAsync(SupportAppProfiles);
            var tenantAppProfiles = appProfiles
                .Where(item => item.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) && item.Type != CloudAos.IdentityProviderType.Exchange)
                .Where(IsActiveApp);
            var groupedAppProfiles = tenantAppProfiles.GroupBy(item => item.Type).ToDictionary(item => item.Key, item => item.ToList());
            if(groupedAppProfiles.TryGetValue(CloudAos.IdentityProviderType.Office365, out var value))
            {
                return value.First();
            }

            if(groupedAppProfiles.TryGetValue(CloudAos.IdentityProviderType.CloudRecords, out value))
            {
                return value.First();
            }

            return tenantAppProfiles.First();
        }
        public static async Task<CloudAos.AppProfileInfo> GetCustomDelegateAppProfile(string customerId, string tenantId)
        {
            var client = AosApiUtility.GetAosModernClient(customerId);
            var appProfiles = await client.AppProfileService.GetByTypesAsync(SupportAppProfiles);
            var tenantAppProfiles = appProfiles
                .Where(item => item.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) && item.Type != CloudAos.IdentityProviderType.Exchange)
                .Where(IsActiveApp);
            var groupedAppProfiles = tenantAppProfiles.GroupBy(item => item.Type).ToDictionary(item => item.Key, item => item.ToList());
            if (groupedAppProfiles.TryGetValue(CloudAos.IdentityProviderType.CustomDelegateApp, out var value))
            {
                return value.First();
            }

            if (groupedAppProfiles.TryGetValue(CloudAos.IdentityProviderType.CloudRecords, out value))
            {
                return value.First();
            }

            return tenantAppProfiles.First();
        }
        public static List<CloudAos.AppProfileInfo> GetAllProfiles(string customerId)
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(customerId);
                var appProfiles = GetProfilesByCallerType(client);
                var res = appProfiles
                    .Where(IsActiveApp)
                    .OrderBy(
                    item => item.Type switch
                    {
                        CloudAos.IdentityProviderType.CustomAzureApp => 10,
                        CloudAos.IdentityProviderType.CloudRecords => 20,
                        CloudAos.IdentityProviderType.CustomDelegateApp => 40,
                        _ => 30
                    }).ToList();

                return res;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all profiles by [{customerId}]. Error: {e}");
                return new List<CloudAos.AppProfileInfo>();
            }
        }

        private static List<CloudAos.AppProfileInfo> GetProfilesByCallerType(AosModernApiTenantClient client)
        {
            if (TenantLocalValue.CallerType == "PartnerPortal")
            {
                logger.Info("Current callerType is PartnerPortal, get app profiles with impersonation.");
                return client.ImpersonateCallerInvoke<AosModernApiTenantClient, List<CloudAos.AppProfileInfo>>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async innerClient =>
                {
                    return await innerClient.AppProfileService.GetByTypesAsync(PartnerPortalAppProfiles);
                }).GetAwaiter().GetResult()?.OrderBy(p => GetAospProfilePriority(p.Type)).ToList() ?? new List<CloudAos.AppProfileInfo>();
            }

            return client.AppProfileService.GetByTypesAsync(SupportAppProfiles).GetAwaiter().GetResult();
        }
        public static List<CloudAos.GsuiteAppProfileInfo> GetAllGoogleProfiles(string customerId)
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(customerId);
                return client.AppProfileService.GetGsuiteAppProfilesAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all google profiles by [{customerId}]. Error: {e}");
                return new List<CloudAos.GsuiteAppProfileInfo>();
            }
        }

        public static List<CloudAos.AppProfileInfo> GetAllCustomAppProfiles(string customerId, string O365TenantId)
        {
            try
            {
                var res = new List<CloudAos.AppProfileInfo>();
                var client = AosApiUtility.GetAosModernClient(customerId);
                var appProfiles = client.AppProfileService.GetByTypesAsync(SupportAppProfiles).GetAwaiter().GetResult();

                var customAzureApps = appProfiles
                    .Where(item => item.Type == CloudAos.IdentityProviderType.CustomAzureApp)
                    .Where(item => item.TenantId.Equals(O365TenantId, StringComparison.OrdinalIgnoreCase))
                    .Where(IsActiveApp);
                res.AddRange(customAzureApps);
                return res;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all profiles by [{customerId}]. Error: {e}");
                return new List<CloudAos.AppProfileInfo>();
            }
        }

        public static List<CloudAos.AppProfileInfo> GetHasADPermissionProfiles(string customerId)
        {
            var res = GetAllProfiles(customerId);
            return res.Where(item => item.Type != CloudAos.IdentityProviderType.Exchange).ToList();
        }

        public static List<CloudAos.AppProfileInfo> GetHasExchangePermissionProfiles(string customerId) 
        {
            var res = GetAllProfiles(customerId);
            return res.Where(item => item.Type != CloudAos.IdentityProviderType.SharePoint).ToList();
        }
        public static async Task<CloudAos.AISettingModel> GetAISettingsAsync(string customerId)
        {
            var client = AosApiUtility.GetAosModernClient(customerId);
            var aiSetting = await client.TenantProfileService.GetAISettingsAsync();
            return aiSetting;
        }
        public static async Task<bool> IsEnableAIRecommendation(string customerId)
        {
            var aiSetting = await GetAISettingsAsync(customerId);
            var res = aiSetting?.Features?.FirstOrDefault(f => f.FeatureType == Cloud.Sdk.Data.AosModern.AIFeatureType.OpusSetRuleForDocs);
            return res?.IsEnabled ?? false;
        }
        public static async Task<CloudAos.AppProfileInfo> GetProfileById(string id)
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var appProfile = await client.AppProfileService.GetByIdAsync(id);

            if (appProfile == null)
            {
                throw new Exception("RM_MA_NotFound_CustomApp");
            }

            if (appProfile.Status != CloudAos.AppProfileStatus.Active)
            {
                logger.Warn($"The app {appProfile.Name} Status is {appProfile.Status}.");
                appProfile = null;
            }

            if (!SupportAppProfiles.Contains(appProfile.Type))
            {
                throw new Exception($"Record not support [{appProfile.Type}] app profile.");
            }

            return appProfile;
        }

        #endregion

        public static List<string> GetO365TenantIds(string customerId)
        {
            try
            {
                var tenants = AosApiUtility.GetAosModernClient(customerId)
                    .TenantManagementService.GetByTypeAsync(CloudAos.PlatformType.Office365)
                    .GetAwaiter().GetResult();

                return tenants.Select(item => item.Id).ToList();
            }
            catch (Exception e)
            {

                logger.Error("Error getting all o365 id of {0} from AOS. {1}", customerId, e);
            }
            return new List<string>();
        }

        public static List<string> GetAOSPO365TenantIds(string customerId)
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(customerId);
                var tenants = client.ImpersonateCallerInvoke<AosModernApiTenantClient, List<CloudAos.TenantConnectionInfo>?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                {
                    var result = await client.TenantManagementService.GetByTypeAsync(CloudAos.PlatformType.Office365);
                    return result;
                }).GetAwaiter().GetResult();

                return tenants.Select(item => item.Id).ToList();
            }
            catch (Exception e)
            {

                logger.Error("Error getting all o365 id of {0} from AOS. {1}", customerId, e);
            }
            return new List<string>();
        }

        public static List<string> GetGoogleTenantIds(string customerId)
        {
            try
            {
                var tenants = AosApiUtility.GetAosModernClient(customerId)
                    .TenantManagementService.GetByTypeAsync(CloudAos.PlatformType.Google)
                    .GetAwaiter().GetResult();

                return tenants.Select(item => item.Id).ToList();
            }
            catch (Exception e)
            {

                logger.Error("Error getting all google id of {0} from AOS. {1}", customerId, e);
            }
            return new List<string>();
        }

        public static List<RMAosGoogleAppProfile> GetAllAppProfilesGoogleTenants(string customerId)
        {
            try
            {
                var isGControl = TenantService.HasInitGControlPlatForm().Result;
                logger.Info($"Start to get all google app profile, is gcontorl:{isGControl}");

                List<RMAosGoogleAppProfile> appProfiles = new();
                var tenants = AosApiUtility.GetAosModernClient(customerId)
                    .TenantManagementService.GetByTypeAsync(CloudAos.PlatformType.Google)
                    .GetAwaiter().GetResult();

                var tenantIds =  tenants.Select(item => item.Id).ToList();
                var client = AosApiUtility.GetAosModernClient(customerId);

                foreach (var tenantId in tenantIds)
                {
                    logger.Info($"Start to get app profile by tenantId {tenantId}.");
                    CloudAos.GsuiteCustomAppProfile profile = null;
                    if (isGControl)
                    {
                        profile = GetContorlPlusAppProfile(customerId, tenantId).GetAwaiter().GetResult();
                        logger.Info($"Get app profile {profile?.Id} from the gcontrol.");
                    }
                    if(profile == null)
                    {
                        profile = GetGoogleProfileInAosOrCache(customerId, tenantId, true);
                        logger.Info($"Get app profile {profile?.Id} from the opus.");
                    }
                    if (profile != null)
                    {
                        logger.Info($"End to get app profile {profile.Id} by tenantId {tenantId}.");
                        var googleProfile = RMAOSConvertUtil.Convert2GoogleAppProfile(profile, customerId);
                        appProfiles.Add(googleProfile);
                    }
                    else
                    {
                       logger.Warn($"Not found app profile by tenantId {tenantId}.");
                    }
                }
                return appProfiles;
            }
            catch (Exception e)
            {
                logger.Error("Error getting all google id of {0} from AOS. {1}", customerId, e);
            }
            return [];
        }

        public static async Task<Dictionary<string, string>> GetGoogleTenants(string customerId)
        {
            try
            {
                var tenants = await AosApiUtility.GetAosModernClient(customerId)
                    .TenantManagementService.GetByTypeAsync(CloudAos.PlatformType.Google);

                return tenants.ToDictionary(tenant => tenant.Id, tenant => tenant.Name);
            }
            catch (Exception e)
            {
                logger.Error("Error getting all google id of {0} from AOS. {1}", customerId, e);
            }
            return new();
        }

        public static List<AADAccount> GetAADAccounts(List<AADAccount> accounts, string customerId)
        {
            try
            {
                var existAccounts = AosApiUtility.GetAosModernClient(customerId)
                    .UserService
                    .GetByNamesOrEmailsAsync(accounts.Select(a => a.Mail).ToList())
                    .GetAwaiter()
                    .GetResult();

                var existAccountLookup = existAccounts
                    .Where(a => a.LoginType == Cloud.Sdk.Data.AosModern.LoginMethod.Office365)
                    .ToDictionary(a => a.ObjectId);

                foreach (var account in accounts)
                {
                    if (existAccountLookup.TryGetValue(account.Id, out var aosAccount))
                    {
                        account.AccountId = aosAccount.Id ?? aosAccount.ObjectId;
                    }
                }

                return accounts
                    .Where(a => !string.IsNullOrEmpty(a.AccountId))
                    .ToList();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting AOS accounts", e);
                return null;
            }
        }
        public static List<AADAccount> RegisterAADAccount(List<AADAccount> accounts, string customerId, string o365TenantId, string inviter)
        {
            try
            {
                var model = new CloudAos.InviteO365UserInfo()
                {
                    O365TenantId = o365TenantId,
                    //Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                    UserGroups = AADAccountConverter.Convert(accounts, inviter)
                };
                return Execute(() =>
                {
                    var task = System.Threading.Tasks.Task.Run(() =>
                    {
                        return AosApiUtility.GetAosModernClient(customerId).UserService.InviteO365UsersAsync(model);
                    });

                    var result = task.GetAwaiter().GetResult();

                    if (result.Count == 0)
                    {
                        var existAccounts = AosApiUtility.GetAosModernClient(customerId).UserService
                            .GetByNamesOrEmailsAsync(accounts.Select(a => a.Mail).ToList())
                            .GetAwaiter().GetResult();

                        result.AddRange(existAccounts
                            .Where(q => q.LoginType == Cloud.Sdk.Data.AosModern.LoginMethod.Office365)
                            .Select(q => new Cloud.Sdk.Data.AosModern.InviteO365UserResult
                            {
                                Id = q.Id,
                                ObjectId = q.ObjectId,
                                InviteType = q.InviteType
                            }));
                    }

                    result.ForEach(r =>
                    {
                        var account = accounts.Find(a => a.Id.Equals(r.ObjectId));
                        if (account != null && account.InviteType == Contract.Object.AccountType.User)
                        {
                            account.AccountId = r.Id == null ? r.ObjectId : r.Id;
                        }
                        else if (account != null && account.InviteType == Contract.Object.AccountType.Group)//AOS issue ,same account can be invite more than once.
                        {
                            account.AccountId = r.ObjectId;
                        }
                    });

                    var registeredAccounts = accounts.Where(o => result.Any(r => (r.Id != null && r.Id.Equals(o.AccountId)) 
                                                                              || (r.ObjectId != null && r.ObjectId.Equals(o.AccountId)))).ToList();
                    if (registeredAccounts.Count == 0)
                    {
                        throw new Exception("No registered users.");
                    }
                    return registeredAccounts;
                }
                );
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while register accounts to AOS", e);
                return null;
            }
        }

        public static IEnumerable<(CloudAos.RemoteNodesResult result, CloudAos.ContainerInfo container)> GetTenantRemoteNodesByPage(string customerId, string tenantId, List<SourceFlag> needSyncedContentSources)
        {
            var supportQueryContainerTypes = new List<CloudAos.RemoteNodeType>();

            if(needSyncedContentSources.Contains(SourceFlag.SharePoint))
            {
                supportQueryContainerTypes.Add(CloudAos.RemoteNodeType.SiteCollection);
                supportQueryContainerTypes.Add(CloudAos.RemoteNodeType.Office365Group);
            }

            if(needSyncedContentSources.Contains(SourceFlag.Exchange))
            {
                supportQueryContainerTypes.Add(CloudAos.RemoteNodeType.Mailbox);
            }

            if(needSyncedContentSources.Contains(SourceFlag.OneDrive))
            {
                supportQueryContainerTypes.Add(CloudAos.RemoteNodeType.OneDrive);
            }

            var containers = AosApiUtility.GetAosModernClient(customerId).
                ContainerService.GetByTenantIdAsync(tenantId, supportQueryContainerTypes).GetAwaiter().GetResult();

            foreach(var container in containers)
            {
                var nodeRes = AosApiUtility.GetAosModernClient(customerId)
                    .RemoteNodeService.QueryRemoteNodesAsync(new CloudAos.RemoteNodesQueryParameter
                    {
                        TenantId = tenantId,
                        NodeTypes = new List<CloudAos.RemoteNodeType> { container.NodeType },
                        ContainerId = container.Id
                    }).GetAwaiter().GetResult();
                yield return (nodeRes, container);
            }

            if (needSyncedContentSources.Contains(SourceFlag.SharePoint))
            {
                var channelRes = AosApiUtility.GetAosModernClient(customerId)
                            .RemoteNodeService.QueryRemoteNodesAsync(new CloudAos.RemoteNodesQueryParameter
                            {
                                TenantId = tenantId,
                                NodeTypes = new List<CloudAos.RemoteNodeType> { CloudAos.RemoteNodeType.Channel },
                            }).GetAwaiter().GetResult();
                yield return (channelRes, new CloudAos.ContainerInfo
                {
                    Name = "",
                    Id = "41cfe969-e07b-45cb-a7d0-b022f967e929"
                });
            }
        }

        public static CloudAos.RemoteNodesResult GetTenantRemoteNodes(string customerId, string tenantId, bool throwError = false)
        {
            try
            {
                //AosApiUtility.GetAosModernClient(customerId).RemoteNodeService.QueryRemoteNodesAsync(new CloudAos.RemoteNodesQueryParameter
                //{
                //    TenantId = tenantId
                //});
                var result = AosApiUtility.GetAosModernClient(customerId).RemoteNodeService.GetByTenantIdAsync(tenantId).GetAwaiter().GetResult();
                return result;
            }
            catch (Exception e)
            {
                logger.Error("Error getting GetTenantRemoteNodes of {0} from AOS. {1}", customerId, e);
                if (throwError)
                {
                    throw;
                }
            }
            return new CloudAos.RemoteNodesResult();
        }

        public static CloudAos.RemoteNodesResult GetModernTenantRemoteNodes(string customerId, string tenantId, bool throwError = false)
        {
            try
            {
                return ExecuteTask(() => AosApiUtility.GetAosModernClient(customerId).RemoteNodeService.QueryRemoteNodesAsync(new CloudAos.RemoteNodesQueryParameter()
                {
                    TenantId = tenantId,
                    NodeTypes = new List<CloudAos.RemoteNodeType>() { CloudAos.RemoteNodeType.Mailbox }
                }));
            }
            catch (Exception e)
            {
                logger.Error("Error getting GetTenantRemoteNodes of {0} from AOS. {1}", customerId, e);
                if (throwError)
                {
                    throw;
                }
            }
            return null;
        }
        /// <summary>
        /// get the remote node by site url or group email
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteurl"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        public static List<RemoteNode> GetRemoteNodeBySiteUrl(string customerId, string siteurl, bool throwError = false)
        {
            try
            {
                var tenantInfos = ExecuteTask(() => AosApiUtility.GetAosModernClient(customerId).Office365TenantService.GetAllAsync());
                using (new PerformanceScope($"Aos--get site{siteurl}"))
                {
                    foreach (var tenantInfo in tenantInfos)
                    {
                        //var containers = ExecuteTask(() => RMCloudSdk.Aos.TenantService.GetContainersByTenantId(customerId, tenantInfo.TenantId));
                        var allSites = ExecuteTask(() => AosApiUtility.AosClient.TenantService.GetTenantRemoteNodes(customerId, tenantInfo.TenantId));
                        var site = allSites.Where(n => (n.Url != null && n.Url.Equals(siteurl)) || (n.Name != null && n.Name.Equals(siteurl))).FirstOrDefault();
                        // var allSites2 = ExecuteTask(() => RMCloudSdk.Aos.TenantService.GetSyncRemoteNodes(customerId, tenantInfo.TenantId));
                        //foreach (var container in containers)
                        //{
                        //    var nodes = ExecuteTask(() => RMCloudSdk.Aos.TenantService.GetPagedTenantRemoteNodes(customerId, tenantInfo.TenantId, container.Id, container.NodeType, 1, 1, siteurl));
                        //    if (nodes.Value.Count > 0)
                        //    {
                        //        return nodes.Value;
                        //    }
                        //}
                        if (site != null)
                        {
                            return new List<RemoteNode>() { site };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error getting GetTenantRemoteNodes {siteurl} of {customerId} from AOS. {e}");
                if (throwError)
                {
                    throw;
                }
            }
            return new List<RemoteNode>();
        }

        public static Dictionary<RemoteNodeType, List<RemoteNode>> GetTenantAllContainers(string customerId)
        {
            var result = new Dictionary<RemoteNodeType, List<RemoteNode>>();
            try
            {
                foreach (RemoteNodeType nodeType in Enum.GetValues(typeof(RemoteNodeType)))
                {
                    var containers = ExecuteTask(() => AosApiUtility.AosClient.TenantService.GetAllContainerByType(customerId, nodeType));
                    if (containers == null)
                    {
                        continue;
                    }

                    result.Add(nodeType, containers);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get tenant: [{customerId}] all remote nodes. Error: {e}");
            }
            return result;
        }

        public static string GetAOSMailboxGuid(string customerId, string tenantId, string address)
        {
            string mailboxGuid = string.Empty;
            var results = GetModernTenantRemoteNodes(customerId, tenantId);
            var findResult = results.Mailboxes.Find(r => r.Name.Equals(address, StringComparison.OrdinalIgnoreCase));
            if (findResult != null)
            {
                mailboxGuid = findResult.ObjectId;
                logger.Info("Mailbox real mailBoxGuid is:{0}.NodeName:{1}.", mailboxGuid, address);
            }
            return mailboxGuid;
        }

        public static bool IsCustomerLicenseAvailable(string customerId)
        {
            try
            {
                return Execute(() =>
                    {                    
                        var license = AosApiUtility.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).Result;
                        return license != null;
                    }
                );
            }
            catch (Exception e)
            {

                logger.Error("Error getting license available of {0} from AOS. {1}", customerId, e);
            }
            return false;
        }

        public static async Task<RMAosLicenseInfo> GetLicenseInfo(string customerId)
        {
            RMAosLicenseInfo result = new RMAosLicenseInfo() { Enable = false, AdditionalDataSource = PaidForModule.None, AdditionalProduct = PaidForProduct.None };
            try
            {
                CloudAos.LicenseInfo opusLicense = await AosApiUtility.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
                if (opusLicense?.Modules != null)
                {
                    result.Type = opusLicense.Type;
                    var opusILLicenseModule = opusLicense.Modules.Where(o => o.Name.Equals(RecordsConstants.OPUS_MODULE_IL_NAME)).FirstOrDefault();
                    if ((opusILLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                    {
                        result.Enable = true;
                        result.AdditionalProduct |= PaidForProduct.OpusIL;
                        result.AdditionalDataSource |= PaidForModule.Office365;
                        if (opusLicense.Extension != null && opusLicense.Extension is CloudAos.CloudRecordsExtension)
                        {
                            CloudAos.CloudRecordsExtension extension = (CloudAos.CloudRecordsExtension)opusLicense.Extension;
                            result.AdditionalDataSource |= (PaidForModule)extension.AdditionalDataSource;
                            result.EnableAutoClassification = extension.EnableAutoClassification;
                        }
                    }

                    var opusSOLicenseModule = opusLicense.Modules.Where(o => o.Name.Equals(RecordsConstants.OPUS_MODULE_SO_NAME)).FirstOrDefault();
                    if ((opusSOLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                    {
                        result.Enable = true;
                        result.AdditionalProduct |= PaidForProduct.OpusSO;
                        result.StorageLicenseInfo = new SOStorageLicenseInfo();
                        result.StorageLicenseInfo.UserSeat = opusSOLicenseModule.UserSeat;

                        if (opusLicense.Extension != null && opusLicense.Extension is CloudAos.CloudRecordsExtension)
                        {
                            CloudAos.CloudRecordsExtension extension = (CloudAos.CloudRecordsExtension)opusLicense.Extension;
                            result.StorageLicenseInfo.Byos = extension.Byos;
                            result.StorageLicenseInfo.SaleType = extension.SaleType;
                            result.StorageLicenseInfo.CustomerSize = extension.CustomerSize;
                            result.StorageLicenseInfo.EnableContentSearch = extension.EnableContentSearch;
                            logger.Info($"OpusSO UserSeat {opusSOLicenseModule.UserSeat} Byos {extension.Byos} SaleType {extension.SaleType} CustomerSize {extension.CustomerSize}");
                        }
                    }

                    if (!"21V China North".Equals(RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME], StringComparison.OrdinalIgnoreCase))
                    {
                        var opusDiscoveryLicenseModule = opusLicense.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_DISCOVERY_NAME));
                        if (opusDiscoveryLicenseModule == null && opusLicense.Type == CloudAos.LicenseType.Trial)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusDiscovery;
                            result.DiscoveryLicenseInfo = new()
                            {
                                TenantTotalSize = 0,
                                //FrequencyPerMonth = 0
                                FrequencyPerYear = 0
                            };
                        }
                        else if ((opusDiscoveryLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusDiscovery;
                            result.DiscoveryLicenseInfo = new()
                            {
                                TenantTotalSize = opusDiscoveryLicenseModule.UserSeat
                            };
                            if (opusLicense.Extension is CloudAos.CloudRecordsExtension extension)
                            {
                                //result.DiscoveryLicenseInfo.FrequencyPerMonth = extension.PurchasedFrequency;
                                result.DiscoveryLicenseInfo.FrequencyPerYear = extension.PurchasedFrequency;
                            }
                        }
                        var opusSalesforceDiscoveryLicenseModule = opusLicense.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_Salesforce_Discovery));
                        if ((opusSalesforceDiscoveryLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusSalesforceDiscovery;
                            result.AdditionalDataSource |= PaidForModule.Salesforce;
                            result.SalesforceDiscoveryLicenseInfo = new()
                            {
                                TenantTotalSize = opusSalesforceDiscoveryLicenseModule.UserSeat
                            };
                            if (opusLicense.Extension is CloudAos.CloudRecordsExtension extension)
                            {
                                //result.SalesforceDiscoveryLicenseInfo.FrequencyPerYear = extension.PurchasedFrequencyForSalesforce;
                            }
                            logger.Info($"Opus Salesforce license UserSeat {opusSalesforceDiscoveryLicenseModule.UserSeat} FrequencyPerYear {result.SalesforceDiscoveryLicenseInfo.FrequencyPerYear}");
                        }

                        var opusGoogleROTDiscoveryLicenseModule = opusLicense.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_Google_WorkSpace_Discovery));
                        if ((opusGoogleROTDiscoveryLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusGoogleWorkspaceDiscovery;
                            result.GoogleROTDiscoveryLicenseInfo = new()
                            {
                                TenantTotalSize = opusGoogleROTDiscoveryLicenseModule.UserSeat
                            };
                            if (opusLicense.Extension is CloudAos.CloudRecordsExtension extension)
                            {
                                result.GoogleROTDiscoveryLicenseInfo.FrequencyPerYear = extension.PurchasedFrequencyForGoogleWorkspace;
                            }
                            logger.Info($"Opus google rot license UserSeat {opusGoogleROTDiscoveryLicenseModule.UserSeat} FrequencyPerYear {result.GoogleROTDiscoveryLicenseInfo.FrequencyPerYear}");

                        }

                        var opusFSDiscoveryLicenseModule = opusLicense.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_FileSystem_Discovery));          

                        if ((opusFSDiscoveryLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusFileSystemDiscovery;
                            result.FSDiscoveryLicenseInfo = new()
                            {
                                TenantTotalSize = opusFSDiscoveryLicenseModule?.UserSeat ?? 0
                            };
                            if (opusLicense.Extension is CloudAos.CloudRecordsExtension extension)
                            {
                                result.FSDiscoveryLicenseInfo.FrequencyPerYear = extension.PurchasedFrequencyForFileSystem;
                            }
                            logger.Info($"Opus FS discovery license UserSeat {opusFSDiscoveryLicenseModule?.UserSeat} FrequencyPerYear {result.FSDiscoveryLicenseInfo.FrequencyPerYear}");
                        }
                    }

                    var opusGoogleLicenseModule = opusLicense.Modules.Where(o => o.Name.Equals(RecordsConstants.OPUS_MODULE_GOOGLE_NAME)).FirstOrDefault();
                    if (opusLicense.Type != CloudAos.LicenseType.Trial)
                    {
                        if ((opusGoogleLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusGoogle;
                            result.AdditionalDataSource |= PaidForModule.Google;
                            result.StorageLicenseInfo = new SOStorageLicenseInfo();
                            result.StorageLicenseInfo.UserSeat = opusGoogleLicenseModule.UserSeat;
                            if (opusLicense.Extension != null && opusLicense.Extension is CloudAos.CloudRecordsExtension)
                            {
                                CloudAos.CloudRecordsExtension extension = (CloudAos.CloudRecordsExtension)opusLicense.Extension;
                                result.AdditionalDataSource |= (PaidForModule)extension.AdditionalDataSource;
                                result.EnableAutoClassification = extension.EnableAutoClassification;
                                result.StorageLicenseInfo.Byos = extension.Byos;
                                result.StorageLicenseInfo.SaleType = extension.SaleType;
                                result.StorageLicenseInfo.CustomerSize = extension.CustomerSize;
                                result.StorageLicenseInfo.EnableContentSearch = extension.EnableContentSearch;
                                logger.Info($"Google UserSeat {opusGoogleLicenseModule.UserSeat} Byos {extension.Byos} SaleType {extension.SaleType} CustomerSize {extension.CustomerSize}");
                            }
                        }
                    }
                    else if (opusLicense.Type == CloudAos.LicenseType.Trial)
                    {
                        var key = $"{CUSTOM_SETTING}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.GlobalCustomSetting}";
                        var keyValue = GlobalKeyValueService.Get(key);
                        var enableGoogleTrial = false;
                        if (keyValue != null)
                        {
                            var globalConfigs = JsonConvert.DeserializeObject<List<RMGlobalConfigDto>>(keyValue?.Value);
                            var enableGoogle = globalConfigs.FirstOrDefault(config => config.Key == "EnableGoogleTrial");
                            if (enableGoogle != null)
                            {
                                _ = bool.TryParse(enableGoogle.Value, out enableGoogleTrial);
                            }
                        }
                        if (enableGoogleTrial && (opusILLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                        {
                            result.Enable = true;
                            result.AdditionalProduct |= PaidForProduct.OpusGoogle | PaidForProduct.OpusGoogleWorkspaceDiscovery;
                            result.AdditionalDataSource |= PaidForModule.Google;
                            result.StorageLicenseInfo = new SOStorageLicenseInfo();
                            result.StorageLicenseInfo.UserSeat = opusILLicenseModule.UserSeat;
                            if (opusLicense.Extension is CloudAos.CloudRecordsExtension)
                            {
                                CloudAos.CloudRecordsExtension extension = (CloudAos.CloudRecordsExtension)opusLicense.Extension;
                                result.AdditionalDataSource |= (PaidForModule)extension.AdditionalDataSource;
                                result.EnableAutoClassification = extension.EnableAutoClassification;
                                result.StorageLicenseInfo.Byos = extension.Byos;
                                result.StorageLicenseInfo.SaleType = extension.SaleType;
                                result.StorageLicenseInfo.CustomerSize = extension.CustomerSize;
                                result.StorageLicenseInfo.EnableContentSearch = extension.EnableContentSearch;
                            }
                            result.GoogleROTDiscoveryLicenseInfo = new();
                        }
                    }


                    logger.Info($"license expirationTime, IL:{(opusILLicenseModule?.ExpirationTime)?.Ticks}, SO:{(opusSOLicenseModule?.ExpirationTime)?.Ticks}, Google: {(opusGoogleLicenseModule?.ExpirationTime)?.Ticks}");
                }

                CloudAos.LicenseInfo googleControlLicense = await AosApiUtility.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(RecordsConstants.GOOGLE_CONTROL_APPLICATION_NAME);
                var opusModuleNameInGControl = googleControlLicense?.Type == CloudAos.LicenseType.Trial
                    ? nameof(CloudAos.LicenseModuleType.GoogleControl)
                    : nameof(CloudAos.LicenseModuleType.GoogleControlInformationManagement);
                var googleControlLicenseModule = googleControlLicense?.Modules
                    .FirstOrDefault(o => o.Name.Equals(opusModuleNameInGControl));
                if ((googleControlLicenseModule?.ExpirationTime)?.Ticks > DateTime.UtcNow.Ticks)
                {
                    result.Enable = true;
                    result.AdditionalProduct |= PaidForProduct.GoogleControl;
                    result.AdditionalDataSource |= PaidForModule.GoogleControl;
                    result.StorageLicenseInfo = new SOStorageLicenseInfo();
                    result.StorageLicenseInfo.UserSeat = googleControlLicenseModule.UserSeat;
                    if (opusLicense?.Extension is CloudAos.CloudRecordsExtension)
                    {
                        CloudAos.CloudRecordsExtension extension =
                            (CloudAos.CloudRecordsExtension)opusLicense.Extension;
                        result.AdditionalDataSource |= (PaidForModule)extension.AdditionalDataSource;
                        result.EnableAutoClassification = extension.EnableAutoClassification;
                        result.StorageLicenseInfo.Byos = extension.Byos;
                        result.StorageLicenseInfo.SaleType = extension.SaleType;
                        result.StorageLicenseInfo.CustomerSize = extension.CustomerSize;
                        result.StorageLicenseInfo.EnableContentSearch = extension.EnableContentSearch;
                        logger.Info($"Google UserSeat {googleControlLicenseModule.UserSeat} Byos {extension.Byos} SaleType {extension.SaleType} CustomerSize {extension.CustomerSize}");
                    }
                    logger.Info($"Google Control: {(googleControlLicenseModule?.ExpirationTime)?.Ticks}");
                }

                (bool licenseExits, bool archiverlicenseExpired, bool byos) = GetLicenseResult(customerId, RecordsConstants.CloudArchiving);
                if (licenseExits)
                {
                    result.RelatedProductLicenses.Add(new RMAosRelatedProductLicense
                    {
                        ProductType = RelatedProductType.CloudArchiving,
                        LicenseExpired = archiverlicenseExpired,
                        Byos = byos
                    });
                }
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                if (isGCP)
                {
                    //Revoke FS license for GCP
                    result.AdditionalDataSource &= ~PaidForModule.FileSystem;

                    //Revoke SharePoint OnPrem license for GCP
                    result.AdditionalDataSource &= ~PaidForModule.SharePointOnPrem;

                    // //Revoke Maestro license for GCP
                    // result.EnableAutoClassification = false;
                }

                return result;

            }
            catch (Exception e)
            {
                logger.Error("Error getting license info for {0} from AOS. {1}", customerId, e);
                throw;
            }
        }

        public static async Task<CloudAos.SwitchBarInfo> GetSwitchBarAsync(string customerId, string userId)
        {
            //TenantLocalValue.LogonUserId
            return await AosApiUtility.GetAosModernClient(customerId).UserService.GetSwitchBarAsync(userId, CultureInfo.CurrentCulture.Name, string.Empty);
        }

        public static async Task<bool> CheckControlPlusLicense(string customerId)
        {
            CloudAos.LicenseInfo googleControlLicense = await AosApiUtility.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(RecordsConstants.GOOGLE_CONTROL_APPLICATION_NAME);
            if (googleControlLicense == null) return false;
            var opusModuleNameInGControl = googleControlLicense?.Type == CloudAos.LicenseType.Trial
                ? nameof(CloudAos.LicenseModuleType.GoogleControl)
                : nameof(CloudAos.LicenseModuleType.GoogleControlInformationManagement);
            var googleControlLicenseModule = googleControlLicense?.Modules
                .FirstOrDefault(o => o.Name.Equals(opusModuleNameInGControl));
            if (googleControlLicenseModule == null) return false;
            return googleControlLicenseModule.ExpirationTime.Ticks > DateTime.UtcNow.Ticks;
        }

        public static (bool isExits, bool isExpired, bool byos) GetLicenseResult(string customerId, string productName)
        {
            (bool isExits, bool isExpired, bool byos) = (false, false, false);
            CloudAos.LicenseInfo licenseInfo = AosApiUtility.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(productName).Result;
            if (licenseInfo != null)
            {
                isExits = true;
                var licenseModule = licenseInfo.Modules.Where(o => o.Name.Equals(productName)).FirstOrDefault();
                if (licenseModule != null && licenseModule.ExpirationTime < DateTime.UtcNow)
                {
                    isExpired = true;
                }
                if (licenseInfo.Extension is Cloud.Sdk.Data.AosModern.O365ArchivingExtension) {
                    var archiverExtension = licenseInfo.Extension as Cloud.Sdk.Data.AosModern.O365ArchivingExtension;
                    byos = archiverExtension.Byos;
                }
            }
            return (isExits, isExpired, byos);
        }


        public static string GetRecordsServiceUrl(string customerId)
        {

            try
            {
                return Execute(() =>
                {
                    var apps = AosApiUtility.GetAosModernClient(customerId).ApplicationService.GetAppsAsync().Result;
                    var url = apps.Where(a => string.Equals(a.ApplicationTypeName, RecordsConstants.RECORDS_APPLICATION_NAME, StringComparison.OrdinalIgnoreCase)).Select(a => a.Url).FirstOrDefault();
                    if (url == null)
                    {
                        return null;
                    }
                    var uri = new Uri(url);
                    return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                }
                );
            }
            catch (Exception e)
            {

                logger.Error("Error getting services url of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static string GetRECENTERServiceUrl(string groupId)
        {

            try
            {
                var apps = Execute(() => AosApiUtility.GetAosModernClient(groupId).ApplicationService.GetAppsAsync().Result);
                return apps.Where(a => string.Equals(a.ApplicationTypeName, RecordsConstants.ReCenter, StringComparison.OrdinalIgnoreCase)).Select(a => a.Url).FirstOrDefault();
            }
            catch (Exception e)
            {

                logger.Error("Error getting services url of {0} from AOS. {1}", groupId, e);
            }
            return null;
        }

        public static void SetPassWordBySiteCollectionuserName(List<RemoteSiteCollection> siteCollections)
        {
            if (siteCollections == null || siteCollections.Count == 0) return;

            var userNames = siteCollections
                            .Where(siteCollection => !string.IsNullOrEmpty(siteCollection.username))
                            .Select(siteCollection => siteCollection.username)
                            .Distinct()
                            .ToList();

            if (userNames.Count == 0) return;

            var userNamePassWordDic = GetUserNameToPassDict(userNames);
            siteCollections.ForEach(siteCollection =>
            {
                if (!string.IsNullOrEmpty(siteCollection.username) && userNamePassWordDic.ContainsKey(siteCollection.username))
                {
                    siteCollection.password = userNamePassWordDic[siteCollection.username];
                }
            });
        }

        public static void UpdateBposInfoPasswordForServiceAccount(GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo)
        {
            if (bposInfo == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(bposInfo.UserAccountInfo?.Username))
            {
                logger.Info("The username is null");
                return;
            }
            var userNameToPasswordDict = GetUserNameToPassDict(new List<string> { bposInfo.UserAccountInfo.Username });
            if (userNameToPasswordDict.ContainsKey(bposInfo.UserAccountInfo.Username))
            {
                bposInfo.UserAccountInfo.Password = userNameToPasswordDict[bposInfo.UserAccountInfo.Username];
            }
            else
            {
                logger.Error("Failed to get password from aos.");
            }
        }

        public static void AddBposInfoCertInfoByEmailNode(GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo, EmailAccountDto mailbox, Dictionary<string, AppProfile> mailboxNameToAppProfileDict)
        {
            if (bposInfo == null)
            {
                logger.Error("Bpos info is null.");
                return;
            }
            bposInfo.TenantGroupId = TenantLocalValue.LogonGroupId;
            if (mailbox == null)
            {
                logger.Error("Mailbox is null.");
                return;
            }
            if (mailboxNameToAppProfileDict == null || mailboxNameToAppProfileDict.Keys.Count == 0)
            {
                return;
            }
            if (mailbox.ConnectionType != GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken && mailbox.ConnectionType != GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern)
            {
                return;
            }
            if (mailbox.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken || mailbox.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern)
            {
                if (mailboxNameToAppProfileDict.ContainsKey(mailbox.Email))
                {
                    AppProfile appProfile = mailboxNameToAppProfileDict[mailbox.Email];
                    bposInfo.UserAccountInfo.AppId = appProfile.Id;
                    bposInfo.UserAccountInfo.AppClientId = appProfile.AppClientId;
                    bposInfo.UserAccountInfo.AppCertSecret = appProfile.AppCertSecret;
                    //bposInfo.UserAccountInfo.AppCertContent = appProfile.AppCertContent;
                    bposInfo.UserAccountInfo.AADEnvironment = appProfile.AADEnvironment;
                    bposInfo.UserAccountInfo.AppCertSecretContent = appProfile.AppCertSecretContent;
                    bposInfo.AppType = appProfile.IdentityProviderType;
                }
                else
                {
                    logger.Error("Failed to find the app profile dict, mailbox email is {0}", mailbox.Email);
                    bposInfo.TenantGroupId = string.Empty;
                    bposInfo.UserAccountInfo.AppId = string.Empty;
                    bposInfo.UserAccountInfo.AppClientId = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecret = string.Empty;
                    //bposInfo.UserAccountInfo.AppCertContent = string.Empty;
                    bposInfo.UserAccountInfo.AppCertSecretContent = string.Empty;
                    bposInfo.UserAccountInfo.AADEnvironment = GCommon.Contract.CentralAdmin.Object.AADEnvironment.None;
                }
            }
        }
        public static void AddBposInfoCertInfoById(GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo, AppProfile mailboxNameToAppProfile)
        {
            AppProfile appProfile = mailboxNameToAppProfile;
            bposInfo.UserAccountInfo.AppId = appProfile.Id;
            bposInfo.UserAccountInfo.AppClientId = appProfile.AppClientId;
            bposInfo.UserAccountInfo.AppCertSecret = appProfile.AppCertSecret;
            //bposInfo.UserAccountInfo.AppCertContent = appProfile.AppCertContent;
            bposInfo.UserAccountInfo.AADEnvironment = appProfile.AADEnvironment;
            bposInfo.UserAccountInfo.AppCertSecretContent = appProfile.AppCertSecretContent;
            bposInfo.AppType = appProfile.IdentityProviderType;
        }
        public static Dictionary<string, string> GetUserNameToPassDict(List<string> userNames)
        {
            var userNameToPassDict = new Dictionary<string, string>();
            if (userNames == null || userNames.Count == 0)
            {
                return userNameToPassDict;
            }
            try
            {
                var param = new CustomerAccountList()
                {
                    CustomerId = TenantLocalValue.LogonGroupId,
                    AccountList = userNames
                };

                var serviceAccounts = Execute(() => AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).ServiceAccountService.GetAllAsync(true).Result);
                foreach (var userName in userNames)
                {
                    var accounts = serviceAccounts.FindAll(a => a.UserName == userName);

                    if (accounts.Count == 0)
                    {
                        logger.Info("this service account doesn't exist in AOS");
                        continue;
                    }
                    userNameToPassDict.Add(accounts[0].UserName, accounts[0].Password);
                }
            }
            catch (Exception e)
            {
                logger.Error("Failed to get service accounts, exception is {0}.", e);
            }
            return userNameToPassDict;
        }

        public static Dictionary<string, AppProfile> GetRemoteNodeUrlToAppProfileDict(List<RemoteSiteCollection> remoteNodes, string aosTenantGroupId = "")
        {
            using (var scope = new PerformanceScope("GetRemoteNodeUrlToAppProfileDict"))
            {
                logger.Info($"Need get profile remote node count: [{remoteNodes?.Count}].");
                if (string.IsNullOrEmpty(aosTenantGroupId))
                {
                    aosTenantGroupId = TenantLocalValue.LogonGroupId;
                }
                logger.Info("Begin to get sitecollection url -> AppProfile, AOS TenantId is {0}.", aosTenantGroupId);
                if (remoteNodes == null || remoteNodes.Count == 0)
                {
                    logger.Info("This is no remote nodes should be handled.");
                    return new Dictionary<string, AppProfile>();
                }
                remoteNodes = remoteNodes.Where(s => s != null && s.AuthType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken
                || s?.AuthType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern).ToList();
                if (remoteNodes.Count == 0)
                {
                    logger.Info("There is no app profile sitecollections.");
                }
                var allO365TenantIds = remoteNodes.Select(a => a.TenantId).Distinct().ToList();
                var o365TenantIdToAppProfileDict = GetAppProfileMappers(aosTenantGroupId, allO365TenantIds);
                var remoteNodeUrlToAppProfileDict = new Dictionary<string, AppProfile>();
                foreach (RemoteSiteCollection remoteNode in remoteNodes)
                {
                    var url = remoteNode.url;
                    if (!remoteNodeUrlToAppProfileDict.ContainsKey(url) && o365TenantIdToAppProfileDict.ContainsKey(remoteNode.TenantId))
                    {
                        var appProfile = GetAppProfileByRemoteNode(remoteNode, o365TenantIdToAppProfileDict[remoteNode.TenantId]);
                        remoteNodeUrlToAppProfileDict.Add(url, appProfile);
                    }
                }
                logger.Info("Finish remoteNodes getting app profiles from aos.");
                return remoteNodeUrlToAppProfileDict;
            }
        }

        public static string GetMailBoxAddressByAOSObjectID(string objectID)
        {
            var mailBoxAddress = string.Empty;
            var O365Tenants = Execute(() => AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).Office365TenantService.GetAllAsync().Result);
            //var serviceAccounts = Task.Run(() => RMCloudSdk.Aos.TenantService.GetServiceAccount(TenantLocalValue.LogonGroupId)).Result;
            var remoteNodes = new List<CloudAos.MailboxRemoteNode>();
            foreach (var item in O365Tenants)
            {
                var nodes = Execute(() => AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).RemoteNodeService.QueryRemoteNodesAsync(new CloudAos.RemoteNodesQueryParameter()
                {
                    TenantId = item.TenantId,
                    NodeTypes = new List<CloudAos.RemoteNodeType>() { CloudAos.RemoteNodeType.Mailbox }
                }).Result);
                remoteNodes.AddRange(nodes?.Mailboxes);
            }
            logger.Info($"Current LogonGroupId: {TenantLocalValue.LogonGroupId} has remotenode count: {remoteNodes.Count}");
            var mailboxs = remoteNodes.Where(x => x.ObjectId == objectID).ToList();
            if (mailboxs.Count > 0)
            {
                mailBoxAddress = mailboxs.FirstOrDefault()?.Name;
                logger.Info($"GetMailBoxAddressByAOSObjectID objectID:{objectID}.mailBoxAddress:{mailBoxAddress}.");
            }
            return mailBoxAddress;
        }
        public static AppProfile GetAppProfileForEXOArchiver(string tenantId, string aosTenantGroupId = "")
        {
            if (string.IsNullOrEmpty(aosTenantGroupId))
            {
                aosTenantGroupId = TenantLocalValue.LogonGroupId;
            }
            logger.Info("Begin to GetAppProfileForEXOArchiver, AOS TenantId is {0}", aosTenantGroupId);
            var emailNameToAppProfileDict = new Dictionary<string, AppProfile>();
            List<string> allO365TenantIds = new List<string>() { tenantId };
            Dictionary<string, List<CloudAos.AppProfileInfo>> o365TenantIdToAppProfileDict = GetAppProfileMappers(aosTenantGroupId, allO365TenantIds);

            List<CloudAos.AppProfileInfo> appProfiles = o365TenantIdToAppProfileDict[tenantId];
            AppProfile appProfile = GetAppProfileByMailBox(new EmailAccountDto(), appProfiles);
            logger.Info("Finish GetAppProfileForEXOArchiver from aos.");
            return appProfile;
        }
        public static Dictionary<string, AppProfile> GetMailboxNameToAppProfileDict(List<EmailAccountDto> mailboxes, string aosTenantGroupId = "", bool usingModernApp = false)
        {
            if (string.IsNullOrEmpty(aosTenantGroupId))
            {
                aosTenantGroupId = TenantLocalValue.LogonGroupId;
            }
            logger.Info("Begin to get mailbox -> AppProfile, AOS TenantId is {0}", aosTenantGroupId);
            if (mailboxes == null || mailboxes.Count == 0)
            {
                logger.Info("This is no mailboxes.");
                return new Dictionary<string, AppProfile>();
            }
            mailboxes = mailboxes.Where(s => s.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken || s.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern).ToList();
            if (mailboxes.Count == 0)
            {
                logger.Info("This is no app profile mailboxes.");
                return new Dictionary<string, AppProfile>();
            }
            var emailNameToAppProfileDict = new Dictionary<string, AppProfile>();
            List<string> allO365TenantIds = mailboxes.Select(m => m.TenantId).Distinct().ToList();
            Dictionary<string, List<CloudAos.AppProfileInfo>> o365TenantIdToAppProfileDict = GetAppProfileMappers(aosTenantGroupId, allO365TenantIds);
            foreach (EmailAccountDto email in mailboxes)
            {
                List<CloudAos.AppProfileInfo> appProfiles = o365TenantIdToAppProfileDict[email.TenantId];
                AppProfile appProfile = GetAppProfileByMailBox(email, appProfiles, usingModernApp);
                emailNameToAppProfileDict.Add(email.Email, appProfile);
            }
            logger.Info("Finish mailbox getting app profiles from aos.");
            return emailNameToAppProfileDict;
        }

        private static Dictionary<string, List<CloudAos.AppProfileInfo>> GetAppProfileMappers(string asoTenantId, List<string> o365TenantIds)
        {
            using (var scope = new PerformanceScope("GetAppProfileMappers"))
            {
                logger.Info($"Need get profile tenant count: [{o365TenantIds?.Count}].");
                var dict = new Dictionary<string, List<CloudAos.AppProfileInfo>>();
                logger.Info("Get app profile by o365tenantids: {0}", string.Join(",", o365TenantIds));
                ArgumentCheck.NotNull(o365TenantIds, nameof(o365TenantIds));

                var appProfiles = GetAllProfiles(TenantLocalValue.LogonGroupId);
                if (appProfiles != null && appProfiles.Count > 0)
                {
                    dict = appProfiles.Where(a => o365TenantIds.Contains(a.TenantId)).GroupBy(a => a.TenantId).ToDictionary(k => k.Key, v => v.ToList());
                }
                logger.Info("Get app profile from aos success.");
                return dict;
            }
        }

        public static List<RMAosAuthenticationProfile> GetSPOAuthenticationProfiles(string aosTenantId, List<string> o365TenantIds)
        {
            //RMAOSConvertUtil.Convert2AuthenticationProfile
            var allAppProfiles = new List<RMAosAuthenticationProfile>();
            logger.Info("Get all app profile by o365tenantids:{0}", string.Join(",", o365TenantIds));

            var appProfiles = GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId)
                .Where(item => o365TenantIds.Contains(item.TenantId))
                .ConvertAll(item => RMAOSConvertUtil.Convert2AuthenticationProfile(item));

            allAppProfiles.AddRange(appProfiles);

            logger.Info("Get all app profile from aos success.");
            return allAppProfiles;
        }

        private static AppProfile GetAppProfileByMailBox(EmailAccountDto email, List<CloudAos.AppProfileInfo> allAppProfiles, bool usingModernApp = false)
        {
            if (email == null || allAppProfiles == null || allAppProfiles.Count == 0)
            {
                logger.Info("There is no profile, mailbox: null or profiles count: 0.");
                return new AppProfile();
            }

            var profiles = allAppProfiles.Where(item => item.Type != CloudAos.IdentityProviderType.SharePoint).ToList();
            if(!profiles.Any())
            {
                logger.Info("There is no maillbox profile");
                    return new AppProfile();
            }

            var priorityAppProfile = usingModernApp
                ? CloudAos.IdentityProviderType.CloudRecords
                : CloudAos.IdentityProviderType.Office365;

            if (usingModernApp)
            {
                var hasOnlyClassicProfiles =
                    profiles.All(profile => profile.Type == CloudAos.IdentityProviderType.Office365);
                if (hasOnlyClassicProfiles)
                {
                    logger.Error("Only classic app profiles are available.");
                    throw new NotSupportedException("Classic app profiles are not supported.");
                }

                profiles = profiles
                    .Where(profile => profile.Type != CloudAos.IdentityProviderType.Office365)
                    .ToList();
            }
            
            logger.Info($"Prioritize app profile {priorityAppProfile}");

            var o365App = profiles.FirstOrDefault(app => app.Type == priorityAppProfile);
            if (o365App != null)
            {
                var appType = ConvertIdentityTypeToAppType(o365App.Type);

                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(o365App.TenantId).GetAwaiter().GetResult().AdminUrl;

                return new AppProfile()
                {
                    Id = o365App.Id.Trim(), // AOS 取回的 app profile Id 可能会出现位数不够空格补齐的情况 
                    ProfileName = o365App.Name,
                    TenantId = o365App.TenantId,
                    AdminUrl = adminUrl,
                    IdentityProviderType = appType,
                    AppClientId = o365App.AppClientId,
                    AADEnvironment = ConvertAADEnvironment(o365App.AADEnvironment)
                };
            }
            else
            {

                var appType = ConvertIdentityTypeToAppType(profiles[0].Type);

                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(profiles[0].TenantId).GetAwaiter().GetResult().AdminUrl;

                return new AppProfile()
                {
                    Id = profiles[0].Id.Trim(), // AOS 取回的 app profile Id 可能会出现位数不够空格补齐的情况 
                    ProfileName = profiles[0].Name,
                    TenantId = profiles[0].TenantId,
                    AdminUrl = adminUrl,
                    IdentityProviderType = appType,
                    AppClientId = profiles[0].AppClientId,
                    AADEnvironment = ConvertAADEnvironment(profiles[0].AADEnvironment)
                };
            }
        }

        private static AppProfile GetAppProfileByRemoteNode(RemoteSiteCollection remoteNode, List<CloudAos.AppProfileInfo> allAppProfiles)
        {
            if (remoteNode == null || allAppProfiles == null || allAppProfiles.Count == 0)
            {
                logger.Info("There is no profile, remoteNode: null or profiles count: 0.");
                return new AppProfile();
            }

            var profiles = allAppProfiles.Where(item => item.Type != CloudAos.IdentityProviderType.Exchange).ToList();
            if (!profiles.Any())
            {
                logger.Info("There is no remoteNode profile");
                    return new AppProfile();
                }

            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(profiles[0].TenantId).GetAwaiter().GetResult().AdminUrl;

            return new AppProfile()
            {
                // AOS 取回的 app profile Id 可能会出现位数不够空格补齐的情况 
                Id = profiles[0].Id.Trim(),
                ProfileName = profiles[0].Name,
                TenantId = profiles[0].TenantId,
                //CustomerId = profiles[0].CustomerId,
                AdminUrl = adminUrl,
                IdentityProviderType = ConvertIdentityTypeToAppType(profiles[0].Type),
                //AppCertContent = profiles[0].AppCertContent,
                //AppCertSecret = profiles[0].AppCertSecret,
                AppClientId = profiles[0].AppClientId,
                //AppCertSecretContent = profiles[0].AppCertSecretContent,
                AADEnvironment = ConvertAADEnvironment(profiles[0].AADEnvironment)
            };
        }

        public static AppProfile ConvertToAppProfile(CloudAos.AppProfileInfo authenticationProfile)
        {
            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(authenticationProfile.TenantId).GetAwaiter().GetResult().AdminUrl;

            return new AppProfile()
            {
                // AOS 取回的 app profile Id 可能会出现位数不够空格补齐的情况 
                Id = authenticationProfile.Id.Trim(),
                ProfileName = authenticationProfile.Name,
                TenantId = authenticationProfile.TenantId,
                AdminUrl = adminUrl,
                IdentityProviderType = ConvertIdentityTypeToAppType(authenticationProfile.Type),
                AppClientId = authenticationProfile.AppClientId,
                AADEnvironment = ConvertAADEnvironment(authenticationProfile.AADEnvironment)
            };
        }

        private static GCommon.Contract.CentralAdmin.Object.AADEnvironment ConvertAADEnvironment(CloudAos.AADEnvironment aADEnvironment)
        {
            var result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.AzureCloud;
            switch (aADEnvironment)
            {
                case CloudAos.AADEnvironment.AzureChinaCloud:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.AzureChinaCloud;
                    break;
                case CloudAos.AADEnvironment.AzureCloud:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.AzureCloud;
                    break;
                case CloudAos.AADEnvironment.AzureGermanyCloud:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.AzureGermanyCloud;
                    break;
                case CloudAos.AADEnvironment.AzurePPE:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.AzurePPE;
                    break;
                case CloudAos.AADEnvironment.USGovernment:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.USGovernment;
                    break;
                case CloudAos.AADEnvironment.USGovernment_DoD:
                    result = GCommon.Contract.CentralAdmin.Object.AADEnvironment.USGovernment_DoD;
                    break;
            }
            return result;
        }

        public static List<CloudAos.ServiceAccount> GetServiceAccounts(string customerId)
        {
            var serviceAccounts = ExecuteTask(() => AosApiUtility.GetAosModernClient(customerId).ServiceAccountService.GetAllAsync(false));
            return serviceAccounts;
        }
        public static List<CloudAos.ServiceAccount> GetServiceAccountsWithPassword(string customerId)
        {
            var serviceAccounts = ExecuteTask(() => AosApiUtility.GetAosModernClient(customerId).ServiceAccountService.GetAllAsync(true));
            return serviceAccounts;
        }
        public static List<CloudAos.ServiceAccount> GetServiceAccountsByTenantIdWithPassword(string customerId,string siteTeanantid)
        {
            var serviceAccounts = ExecuteTask(() => AosApiUtility.GetAosModernClient(customerId).ServiceAccountService.GetByTenantIdAsync(siteTeanantid, true));
            return serviceAccounts;
        }
        public static string GetServiceAccountPassword(string customerId, string userName)
        {
            try
            {
                return Execute(() =>
                {
                    var account = AosApiUtility.GetAosModernClient(customerId).ServiceAccountService.GetByNameAsync(userName).Result;
                    return account?.Password;
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting user psw of {0}, {1} from AOS. {2}", customerId, userName, e);
            }
            return string.Empty;
        }

        public static string GetO365TenantIdByUserAadId(string customerId, string userAadId)
        {
            try
            {
                return Execute(() =>
                {
                    var client = AosApiUtility.GetAosModernClient(customerId);
                    var user = client.UserService.GetByObjectIdAsync(userAadId, RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                    return user.TenantId;
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting user by id of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static AccountDto GetUserByUserId(string customerId, string userId)
        {
            try
            {
                return Execute(() =>
                {
                    var client = AosApiUtility.GetAosModernClient(customerId);
                    var account = client.UserService.GetUsersAsync(RecordsConstants.RECORDS_APPLICATION_NAME).Result.Where(u => u.Id.Equals(userId)).FirstOrDefault();
                    return RMAOSConvertUtil.Convert2RMAccount(account);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting user by id of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<AccountDto> GetGroupByIds(string customerId, List<string> groupIds)
        {
            try
            {
                return Execute(() =>
                {
                    var account = AosApiUtility.GetAosModernClient(customerId).UserService.GetUsersAsync(RecordsConstants.RECORDS_APPLICATION_NAME).Result.Where(u => groupIds.Contains(u.ObjectId) && u.InviteType == CloudAos.InviteType.Group).ToList();
                    return account.ConvertAll(o => RMAOSConvertUtil.Convert2RMAccount(o));
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting user by id of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<AccountDto> GetGroupsByAadIds(string customerId, List<string> groupIds)
        {
            try
            {
                return Execute(() =>
                {
                    var account = AosApiUtility.GetAosModernClient(customerId).UserService.GetUsersAsync(RecordsConstants.RECORDS_APPLICATION_NAME).Result.Where(u => groupIds.Contains(u.ObjectId) && u.InviteType == CloudAos.InviteType.Group).ToList();
                    return account.ConvertAll(o => RMAOSConvertUtil.Convert2RMAccount(o));
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting user by id of {0} from AOS. {1}", customerId, e);
            }
            return [];
        }

        public static List<AccountDto> GetGroupAndUsers(string customerId)
        {
            try
            {
                return Execute(() =>
                {
                    var accounts = AosApiUtility.GetAosModernClient(customerId).UserService.GetUsersAsync(RecordsConstants.RECORDS_APPLICATION_NAME).Result;
                    GetControlPlusUsers(customerId, ref accounts);
                    logger.Info($"users under tenant count is {accounts?.Count}, tenant id: {customerId}");
                    return accounts?.ConvertAll(a => RMAOSConvertUtil.Convert2RMAccount(a));
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting group user of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        private static void GetControlPlusUsers(string customerId, ref List<Cloud.Sdk.Data.AosModern.UserInfo> accounts)
        {
            try
            {
                if (CheckControlPlusLicense(customerId).Result)
                {
                    var controlPlusUsers = AosApiUtility.GetAosModernClient(customerId).UserService.GetUsersAsync(RecordsConstants.GOOGLE_CONTROL_APPLICATION_NAME).Result ?? [];
                    logger.Info($"Control plus users under tenant count is {controlPlusUsers?.Count}, tenant id: {customerId}");
                    accounts = accounts == null ? controlPlusUsers : accounts.UnionBy(controlPlusUsers, u => u.Id).ToList();
                }
            }catch(Exception ex)
            {
                logger.Error($"GetControlPlusUsers failed for tenant {customerId}, exception: {ex}");
            }
            
        }

        //public static bool VerifySignature(string data, string signature)
        //{
        //    try
        //    {
        //        var publicKey = Execute(() => AosApiUtility.GetAosModernApplicationClient().PublicKeyService.GetAosPublicKeyAsync().Result);

        //        using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048))
        //        {
        //            rsaCryptoServiceProvider.FromXmlString(publicKey);
        //            var signatureData = Convert.FromBase64String(signature);
        //            var algorithm = "SHA512";
        //            var flag = rsaCryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(data), algorithm, signatureData);
        //            if (!flag)
        //            {
        //                algorithm = "SHA1";
        //                flag = rsaCryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(data), algorithm, signatureData);
        //            }
        //            return flag;
        //        }
        //        //logger.Info("VerifySignature {0}|{1}|{2}|{3}|{4}",product,data,signature, flag, algorithm);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("VerifySignature error: {0}", e.ToString());
        //        return false;
        //    }
        //}

        //public static bool VerifyAOSSignature(string data, string signature)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signature))
        //        {
        //            logger.Warn("login failed: user or signature is null.");
        //            return false;
        //        }
        //        var user = SerializerHelper.DeserializeByJsonConvert<AosUserInfo>(data);
        //        if (user.IsExpire)
        //        {
        //            logger.Info($"login failed: user signature is expire:{user?.UserId}, {user.CustomerId}, {user.ExpireTime}");
        //            return false;
        //        }

        //        var publicKey = Execute(() => AosApiUtility.GetAosModernApplicationClient().PublicKeyService.GetAosPublicKeyAsync().Result);

        //        using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048))
        //        {
        //            rsaCryptoServiceProvider.FromXmlString(publicKey);
        //            var signatureData = Convert.FromBase64String(signature);
        //            var flag = rsaCryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(data), "SHA1", signatureData);
        //            //logger.Info("VerifySignature Aos {0}|{1}|{2}|{3}", product, data, signature, flag);
        //            return flag;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("VerifySignature Aos error: {0}", e.ToString());
        //        return false;
        //    }
        //}

        public static PoolUserDto GetPoolUserByName(string customerId, string o365TenantId, string userEmail)
        {
            try
            {
                return Execute(() =>
                {
                    var account = AosApiUtility.GetAosModernClient(customerId).ServiceAccountPoolService.GetAccountsByTenantIdAsync(o365TenantId, CloudAos.AccountPoolObjectType.Site).Result.Where(s => s.Status == CloudAos.ServiceAccountStatus.Active && s.UserName.Equals(userEmail, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    return RMAOSConvertUtil.ConvertToRMPoolUser(account);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting group user of {0} from AOS. {1}", customerId, e);
            }
            return null;

        }

        public static AccountDto GetTenantInfo(string customerId)
        {
            try
            {
                return Execute(() =>
                {
                    var account = AosApiUtility.GetAosModernClient(customerId).CustomerService.GetTenantOwnerAsync().Result;
                    return RMAOSConvertUtil.Convert2RMAccount(account,true);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting tenant of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static string GetDatacenter(string customerId)
        {
            using (new PerformanceScope("GetDataCenter"))
            {
                try
                {
                    return Execute(() =>
                    {
                        var datacenter = AosApiUtility.GetAosModernClient(customerId).CustomerService.GetDataCenterAsync().Result;
                        return datacenter;
                    });
                }
                catch (Exception e)
                {
                    logger.Error("Error getting datacenter of {0} from AOS. {1}", customerId, e);
                }
                return null;
            }
        }

        public static List<string> GetTenantGroupId(string o365TenantId)
        {
            try
            {
                return Execute(() =>
                {
                    return AosApiUtility.GetAosModernApplicationClient().CustomerService.GetCustomerIdsAsync(o365TenantId).Result;
                });
            }
            catch (Exception e)
            {
                logger.Error("Error getting tenant group id of {0} from AOS. {1}", o365TenantId, e);
            }
            return null;
        }

        public static X509Certificate2 GetAppCertificate(string appCertSecret, string appCertContent, string appCertSecretContent)
        {
            X509Certificate2 apponlyCertificate;
            if (!string.IsNullOrEmpty(appCertSecretContent))
            {
                ThrowUtil.ThrowIfNullOrEmpty(appCertSecretContent, "AppCertSecretContent");

                apponlyCertificate = new X509Certificate2(Convert.FromBase64String(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(appCertSecretContent))));
            }
            else
            {
                ThrowUtil.ThrowIfNullOrEmpty(appCertContent, "AppCertContent");
                ThrowUtil.ThrowIfNullOrEmpty(appCertSecret, "AppCertSecret");
                var cerContent = appCertContent;
                var cerSercert = appCertSecret;
                var certificateBytes = Convert.FromBase64String(cerContent);
                string secret = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(cerSercert));
                apponlyCertificate = new X509Certificate2(
                    certificateBytes,
                    secret,
                    X509KeyStorageFlags.Exportable |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet);
            }
            return apponlyCertificate;
        }

        public static RMAosAccountInfo LogonAos(string user, string password, string product)
        {
            try
            {
                return Execute(() =>
                {
                    var acc = AosApiUtility.GetAosModernApplicationClient().AccountService.GetByLocalUserAsync(new CloudAos.LogonInfo() { Password = password, Product = product, Username = user }).Result;
                    return RMAOSConvertUtil.Convert2RMAosAccountInfo(acc);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error login aos of {0} from AOS. {1}", user, e);
            }
            return null;
        }

        public static RMAosAccountModelResult ValidateUserByObjectId(string objectId, string name, string tenantId)
        {
            try
            {
                return Execute(() =>
                {
                    var acc = AosApiUtility.GetAosModernApplicationClient().AccountService.GetByOffice365UserAsync(objectId, name, RecordsConstants.RECORDS_APPLICATION_NAME, tenantId).Result;
                    return RMAOSConvertUtil.Convert2RMAccountModelResult(acc);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error validate user of {0} from AOS. {1}", objectId, e);
                throw;
            }
        }
        public static RMAosAccountModelResult ValidateUserByName(string name, string o365DomainName)
        {
            try
            {
                return Execute(() =>
                {
                    var o365TenantId = GetO365TenantId(o365DomainName);
                    var acc = AosApiUtility.GetAosModernApplicationClient().AccountService.GetByOffice365UserAsync("", name, RecordsConstants.RECORDS_APPLICATION_NAME, o365TenantId).Result;
                    return RMAOSConvertUtil.Convert2RMAccountModelResult(acc);
                });
            }
            catch (Exception e)
            {
                logger.Error("Error validate user of {0} from AOS. {1}", name, e);
            }
            return null;

        }

        public static string GetO365TenantId(string o365DomainName)
        {
            return GetO365TenantIdByFullDomain(o365DomainName + ".onmicrosoft.com");
        }

        public static string GetO365TenantIdByFullDomain(string o365DomainName)
        {
            Func<string> getObj = () =>
            {
                var wellKnownUrl = string.Format("https://login.windows.net/{0}/.well-known/openid-configuration", o365DomainName);
                var response = HttpHelper.HttpGet(null, wellKnownUrl);

                if (!string.IsNullOrEmpty(response))
                {
                    JToken token = JToken.Parse(response);
                    var authorizationEndpoint = token["authorization_endpoint"].ToString();
                    var uri = new Uri(authorizationEndpoint);
                    string tenantId = uri.Segments[1].TrimEnd('/');
                    return tenantId;
                }
                else
                {
                    return null;
                }
            };
            return CacheService.Get("O365Id", o365DomainName, getObj);
        }

        public static string GetPortalPublicKey()
        {
            try
            {                
                var publicKey = Execute(() => AosApiUtility.GetAosModernApplicationClient().PublicKeyService.GetAosPublicKeyAsync().Result);
                return publicKey;
            }
            catch (Exception e)
            { 
                logger.Error("An error while GetPortalPublicKey: {0}", e.ToString());
                return "";
            }
        }

        public static string GetLicenseAgreement(string customerId, string product = RecordsConstants.RECORDS_APPLICATION_NAME)
        {
            try
            {
                var content = Execute(() => AosApiUtility.GetAosModernClient(customerId).LicenseAgreementService.GetContentAsync(product).GetAwaiter().GetResult());
                return content;
            }
            catch (Exception e)
            {
                logger.Error("[SsoLogin] An error while GetLicenseAgreement: {0}", e.ToString());
                return "";
            }
        }

        public static bool AcceptLicenseAgreement(AcceptedLicenseAgreementModel model)
        {
            var result = false;
            try
            {
                return Execute(() => AosApiUtility.GetAosModernClient(model.CustomerId).LicenseAgreementService.AcceptLicenseAgreementAsync(new CloudAos.AcceptLicenseAgreementInfo()
                {
                    AcceptedBy = model.AcceptedBy,
                    AcceptedByIPAddress = model.AcceptedByIPAddress,
                    Product = model.Product
                }).GetAwaiter().GetResult());

            }
            catch (Exception e)
            {
                logger.Error("[SsoLogin] An error while AcceptLicenseAgreement: {0}", e.ToString());
            }
            return result;
        }

        public static CloudAos.LicenseInfo GetOPUSLicenseInformation()
        {
            CloudAos.LicenseInfo licenseInfo = new CloudAos.LicenseInfo();
            try
            {
                licenseInfo = Execute(() => AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult());
            }
            catch (Exception e)
            {
                logger.Error("[SsoLogin] An error while GetOPUSLicenseInformation: {0}", e.ToString());
            }
            return licenseInfo;
        }

        /// <summary>
        /// Will call AOS api to get the access token to access Office 365 data later
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static CloudAos.TokenResult GetO365AccessToken(CloudAos.AppProfileInfo profile, CloudAos.TokenType tokenType = CloudAos.TokenType.ApplicationToken)
        {
            return Execute(() =>
            {
                var tokenApiClient = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
                var tokenResult = new CloudAos.TokenResult();

                if (TenantLocalValue.CallerType == "PartnerPortal" 
                    || profile.Type == CloudAos.IdentityProviderType.AospSecurityAnalysis 
                    || profile.Type == CloudAos.IdentityProviderType.AospSecurityAnalysisCsp
                    || profile.Type == CloudAos.IdentityProviderType.AospCustomDelegateApp)
                {
                    tokenResult = tokenApiClient.ImpersonateCallerInvoke<ModernTokenApiClient, CloudAos.TokenResult?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                    {
                        var result = await client.ModernTokenService.GetTokenByAppProfileAsync(
                            profile.Type,
                            CloudAos.TokenResourceType.Graph,
                            profile.TenantId,
                            profile.Id,
                            null,
                            tokenType
                        );
                        return result;
                    }).GetAwaiter().GetResult();
                }
                else
                {
                    tokenResult = tokenApiClient.ModernTokenService.GetTokenByAppProfileAsync(
                        profile.Type,
                        CloudAos.TokenResourceType.Graph,
                        profile.TenantId,
                        profile.Id,
                        null,
                        tokenType
                    ).GetAwaiter().GetResult();
                }

                if (!string.IsNullOrEmpty(tokenResult.Error))
                {
                    logger.Error($"An error occurred while get O365 token from AOS.error : {tokenResult.Error}");
                    return null;
                }

                return tokenResult;
            });
        }


        public static string Encrypt(string plainText, string profileId, string groupId = "")
        {
            if (string.IsNullOrEmpty(plainText))
            {
                logger.Warn("Encrypt,but plainText is empty.");
                return plainText;
            }
            if (string.IsNullOrEmpty(profileId))
            {
                throw new ArgumentNullException("profileId");
            }
            if (string.IsNullOrEmpty(groupId))
            {
                groupId = TenantThreadLocalValue.LogonGroupId;
            }
            var cipherText = ExecuteTask(() => AosApiUtility.AosClient.SecurityProfileService.Encrypt(new TenantEncryptInfo
            {
                CustomerId = groupId,
                PlainText = plainText,
                KeyVaultProfileId = profileId
            }));
            return cipherText;
        }

        public static string Decrypt(string cipherText, string profileId, string groupId = "")
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                logger.Warn("Decrypt,but cipherText is empty.");
                return cipherText;
            }
            if (string.IsNullOrEmpty(profileId))
            {
                throw new ArgumentNullException("profileId");
            }
            if (string.IsNullOrEmpty(groupId))
            {
                groupId = TenantThreadLocalValue.LogonGroupId;
            }
            var plainText = ExecuteTask(() => AosApiUtility.AosClient.SecurityProfileService.Decrypt(new TenantEncryptInfo
            {
                CustomerId = groupId,
                PlainText = cipherText,
                KeyVaultProfileId = profileId
            }));
            return plainText;
        }

        private static T Execute<T>(Func<T> func)
        {
            RetryPolicy retry = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) });
            TimeoutPolicy timeout = Policy.Timeout(TimeSpan.FromMinutes(4), TimeoutStrategy.Pessimistic);
            var wrap = retry.Wrap(timeout);
            try
            {
                return wrap.Execute(func);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }

        private static T ExecuteTask<T>(Func<Task<T>> func)
        {
            try
            {
                return Execute(() => Task.Run(async () => await Execute(func)).Result);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }

        public static byte[] GetProductMasterKey(string customerId, byte[] preNonce)
        {
            try
            {
                return Execute(() => AosApiUtility.GetAosModernClient(customerId).TenantEncryptionService.GetProductMasterKeyAsync((CloudAos.KeyParameter)preNonce).GetAwaiter().GetResult());
    }
            catch (Exception e)
            {
                logger.Error($"An Error occurred whilte get product master key from AOS. tenantid:{customerId}, mesasge: {e}");
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="trainingModelId">训练模型Id</param>
        /// <param name="predictRequest">需要预测Term的文件集合(包含名字和内容)</param>
        /// <returns></returns>
        public static List<ScoreResponse> GetPredictResult(string customerId, Guid trainingModelId, PredictRequest predictRequest)
        {
            RetryPolicy retry = Policy.Handle(RetryHttpRequestExceptionPredicate()).WaitAndRetry(new[] { TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60)});
            TimeoutPolicy timeout = Policy.Timeout(TimeSpan.FromMinutes(8), TimeoutStrategy.Pessimistic);
            var wrap = retry.Wrap(timeout);
            try
            {
                return wrap.Execute(() => AosApiUtility.GetIcsClient(customerId).PredictionService.PredictAsync(trainingModelId, predictRequest).GetAwaiter().GetResult());
            }
            catch (Exception e)
            {
                logger.Error($"An error while get predict reulst from ics, message: {e}");
                throw;
            }
        }

        private static Func<HttpRequestException, bool> RetryHttpRequestExceptionPredicate()
        {
            return (ex) => ex.StatusCode.HasValue &&
                (ex.StatusCode == HttpStatusCode.TooManyRequests ||
                ex.StatusCode == HttpStatusCode.ServiceUnavailable);
        }

        public static bool IsActiveApp(CloudAos.AppProfileInfo app)
        {
            if (app.Status != CloudAos.AppProfileStatus.Active)
            {
                logger.Warn($"This app profile is not Active. App: {app.Name}, Domain: {app.DomainName} , Status: {app.Status}.");
                return false;
            }

            return true;
        }
    }
}
