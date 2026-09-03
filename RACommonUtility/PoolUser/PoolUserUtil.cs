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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft365.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.RACommonUtility
{
    public class CommonPoolUserUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(CommonPoolUserUtil));
        private static bool isEnableAccountPool = false;
        private static IUserService UserService = null;
        private static Dictionary<string, PoolUserDto> urlBposInfoMapping = null;
        private static IRMAppProfileDao RMAppProfileDao = null;
        private readonly static object locker = new object();

        public static void Init(bool enableAccountPool)
        {
            lock (locker)
            {
                if (UserService == null)
                {
                    UserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
                }
                if (urlBposInfoMapping == null)
                {
                    urlBposInfoMapping = new Dictionary<string, PoolUserDto>();
                }
                isEnableAccountPool = enableAccountPool;
            }

        }

        public static Wrapper.Common.AveBPOSAccountInfo GetAveBPOSAccountInfo(GCommon.Contract.CentralAdmin.Object.BposInfo info, string siteUrl, bool useCache = true)
        {
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            if (info == null || info.UserAccountInfo == null)
            {
                throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", siteUrl));
            }
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            if (info != null)
            {
                var tenantId = info.UserAccountInfo.TenantId;
                var siteAdminUrl = string.IsNullOrEmpty(info.UserAccountInfo.AdminUrl) ? WebUtil.GetSPAdminUrl(siteUrl, tenantId) : info.UserAccountInfo.AdminUrl;

                logger.Info("Get site bpos info, SiteUrl:{0}, AuthType:{1}, AADEnvironment:{2}", siteUrl, info.ConnectionType, info.UserAccountInfo.AADEnvironment);

                RMAosAuthenticationProfile profile = RMAosApiClient.GetSPOnlineProfile(TenantLocalValue.LogonGroupId, tenantId,useCache);

                if (profile == null)
                {
                    logger.Info("No available app profile. Use Service Account that bound with site.");
                    string username = string.Empty;
                    string password = string.Empty;
                    string domain = string.Empty;
                    bool hasPoolUser = false;

                    if (!hasPoolUser)
                    {
                        username = info.UserAccountInfo.Username;
                        domain = ".".Equals(info.UserAccountInfo.Domain) ? string.Empty : info.UserAccountInfo.Domain;
                        password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
                    }

                    accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                    {
                        Domain = domain,
                        UserName = username,
                        Password = password.ToSecureString(),
                        AdminUrl = siteAdminUrl,
                        TenantGroupId = TenantLocalValue.LogonGroupId,
                        ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                        AADEnvironment = (AveAzureEnvironment)info.UserAccountInfo.AADEnvironment,
                        TenantId = tenantId
                    };
                }
                else
                {
                    var clientId = profile.AppClientId;
                    logger.Info($"Use app profile: {profile?.Id}");
                    //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
                    accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                    {
                        TenantId = tenantId,
                        AdminUrl = siteAdminUrl,
                        ClientId = clientId,
                        ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                        TenantGroupId = TenantLocalValue.LogonGroupId,
                        AuthenticationProfileId = profile.Id,
                        AppType = RMAosApiClient.ConvertIdentityTypeToAppType((Cloud.Sdk.Data.AosModern.IdentityProviderType)profile.AppType),
                        AADEnvironment = (AveAzureEnvironment)profile.AADEnvironment,
                        //AppCert = apponlyCertificate
                    };
                }

            }

            return accountInfo;
        }

        public static BposInfo GetBPOSInfoForTeams(RemoteSiteCollection site, bool isLimitAppTypes, bool useCache = true, bool enableMultipleAppProfile = true)
        {
            var appTypes = new List<int>()
            {
                (int)Cloud.Sdk.Data.AosModern.IdentityProviderType.CloudRecords,
                //(int)Cloud.Sdk.Data.AosModern.IdentityProviderType.Office365,
                //(int)Cloud.Sdk.Data.AosModern.IdentityProviderType.SharePoint,
                //(int)Cloud.Sdk.Data.AosModern.IdentityProviderType.Exchange,
                //(int)Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomAzureApp,
                (int)Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomDelegateApp,
            };

            var info = GetBPOSInfoAsync(
                site, useCache, enableMultipleAppProfile, 
                isLimitAppTypes ? appTypes : null
            ).GetAwaiter().GetResult();

            if (info == null)
            {
                logger.Error($"GetBPOSInfoAsync for teams is null: Site {site.url}");
                throw new Exception("RM_JM_Details_Teams_CustomAppNotFound_Error");
            }

            return new BposInfo()
            {
                SiteUrl = site.url,
                AppType = info.AppType,
                ConnectionType = (BposConnectionType)info.ConnectionType,
                TenantGroupId = info.TenantGroupId,
                CustomerId = TenantLocalValue.LogonGroupId,
                UserAccountInfo = new()
                {
                    AppId = info.AuthenticationProfileId,
                    AppClientId = info.ClientId,
                    AdminUrl = info.AdminUrl,
                    Domain = info.Domain,
                    TenantId = info.TenantId,
                    Username = info.UserName,
                    AADEnvironment = (AADEnvironment)info.AADEnvironment
                },
            };
        }

        /// <param name="appTypes">null is ignore app type; Cloud.Sdk.Data.AosModern.IdentityProviderType</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Wrapper.Common.AveBPOSAccountInfo> GetBPOSInfoAsync(RemoteSiteCollection site, bool useCache = true, bool enableMultipleAppProfile = true, List<int> appTypes = null)
        {
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            if (site == null)
            {
                logger.Error("Get AveBPOSAccountInfo Failed, Site not found.");
                throw new Exception(I18NEntity.GetString("RM_SS_WebIsNotExist"));
            }
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            var siteAdminUrl = string.IsNullOrEmpty(site.AdminUrl) ? WebUtil.GetSPAdminUrl(site.url, site.TenantId) : site.AdminUrl;

            var appTypesStr = string.Join(",", appTypes ?? new List<int>());
            logger.Info("Get site bpos info, RemoteSiteUrl:{0}, appTypes:{1} enableMultipleAppProfile:{2}", site.url, appTypesStr, enableMultipleAppProfile);

            var tenantId = site.TenantId;
            RMAosAuthenticationProfile profile = null;
            if (enableMultipleAppProfile)
            {
                RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));
                var bestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId), appTypes);
                if (bestApp != null)
                {
                    profile = RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, tenantId, bestApp.AppClientId.ToString(), (CloudAos.IdentityProviderType)bestApp.AppType, false);
                }
                if (profile == null)
                {
                    logger.Warn("App profile no longer exist, try to get app profiles from aos again.");
                    var authenticationProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, new List<string>() { tenantId });
                    logger.Info($"Get app profiles from aos finished. Count:{authenticationProfiles?.Count}");
                    if (appTypes?.Any() == true)
                    {
                        var requiredProfiles = authenticationProfiles.Where(a => appTypes.Contains(a.AppType)).ToList();
                        logger.Info($"After filtered by appTypes: {appTypesStr}, Count: {requiredProfiles.Count}");
                        if(requiredProfiles.Count == 0)
                        {
                            return null;
                        }
                    }
                    if (authenticationProfiles != null && authenticationProfiles.Count > 0)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(tenantId), authenticationProfiles.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                        var newBestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId), appTypes);
                        if (newBestApp != null)
                        {
                            profile = RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, tenantId, newBestApp.AppClientId.ToString(), (CloudAos.IdentityProviderType)newBestApp.AppType, false);
                        }
                    }
                }
            }
            else
            {
                profile = RMAosApiClient.GetSPOnlineProfile(TenantLocalValue.LogonGroupId, tenantId, useCache);
            }

            if (profile == null)
            {
                logger.Info("No available app profile.");
                return null;
                //logger.Info("No available app profile. Use Service Account that bound with site.");
                //accountInfo = GetConnectionSAInfo(site);
                //accountInfo.ExsitAppProfile = false;
            }
            else
            {
                var clientId = profile.AppClientId;
                logger.Info($"Record used multiple app: [{profile?.Id} - {profile.AppType}]");
                //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
                accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                {
                    TenantId = tenantId,
                    AdminUrl = siteAdminUrl,
                    ClientId = clientId,
                    ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    AppType = RMAosApiClient.ConvertIdentityTypeToAppType((CloudAos.IdentityProviderType)profile.AppType),
                    AuthenticationProfileId = profile.Id,
                    AADEnvironment = (AveAzureEnvironment)profile.AADEnvironment,
                    ExsitAppProfile = true,
                    //AppCert = apponlyCertificate
                };
            }

            return accountInfo;
        }
        
        private static RMAppProfileInfo Convert2RMAppProfileInfo(RMAosAuthenticationProfile aosAuthenticationProfile)
        {
            return new RMAppProfileInfo()
            {
                AppClientId = new Guid(aosAuthenticationProfile.AppClientId),
                TenantId = new Guid(aosAuthenticationProfile.TenantId),
                UsedTimes = 0,
                AppType = aosAuthenticationProfile.AppType,
            };
        }
        
        public static Wrapper.Common.AveBPOSAccountInfo GetConnectionSAInfo(RemoteSiteCollection site)
        {
            string username = string.Empty;
            string password = string.Empty;
            string domain = string.Empty;
            string siteAdminUrl = string.IsNullOrEmpty(site.AdminUrl) ? WebUtil.GetSPAdminUrl(site.url,site.TenantId) : site.AdminUrl;
            bool hasPoolUser = false;

            if (!hasPoolUser)
            {
                if (string.IsNullOrEmpty(site.username))
                {
                    username = RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, site.TenantId).FirstOrDefault()?.UserName;
                }
                else
                {
                    username = site.username;
                }
                domain = ".".Equals(site.domain) ? string.Empty : site.domain;
                password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
            }

            return new Wrapper.Common.AveBPOSAccountInfo()
            {
                Domain = domain,
                UserName = username,
                Password = password?.ToSecureString(),
                AdminUrl = siteAdminUrl,
                ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                AADEnvironment = (AveAzureEnvironment)site.AADEnvironment,
                TenantId = site.TenantId
            };
        }
        
        public static Wrapper.Common.AveBPOSAccountInfo GetBPOSInfo(RemoteSiteCollection site, bool useCache = true)
        {
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            if (site == null)
            {
                ArgumentCheck.NotNull(site, nameof(site));
                throw new Exception(string.Format("Get AveBPOSAccountInfo Failed, Site fullPath: {0}.", site?.url));
            }
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;

            logger.Info("Getting site bpos info. RemoteSiteUrl: {0}, AuthType: {1}", site.url, site.AuthType);
            var tenantId = site.TenantId;
            var siteAdminUrl = string.IsNullOrEmpty(site.AdminUrl) ? WebUtil.GetSPAdminUrl(site.url, tenantId) : site.AdminUrl;
            RMAosAuthenticationProfile profile = RMAosApiClient.GetSPOnlineProfile(TenantLocalValue.LogonGroupId, tenantId, useCache);

            if (profile == null)
            {
                logger.Info("No available app profile. Use Service Account that bound with site.");
                string username = string.Empty;
                string password = string.Empty;
                string domain = string.Empty;
                bool hasPoolUser = false;

                if (!hasPoolUser)
                {
                    if (string.IsNullOrEmpty(site.username))
                    {
                        username = RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, tenantId).First().UserName;
                    }
                    else
                    {
                        username = site.username;
                    }
                    domain = ".".Equals(site.domain) ? string.Empty : site.domain;
                    password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
                }

                accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                {
                    Domain = domain,
                    UserName = username,
                    Password = password.ToSecureString(),
                    AdminUrl = siteAdminUrl,
                    ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    AADEnvironment = (AveAzureEnvironment)site.AADEnvironment,
                    TenantId = site.TenantId
                };
            }
            else
            {
                var clientId = profile.AppClientId;
                logger.Info($"Use app profile: {profile?.Id}, appType:{profile.AppType}");
                //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
                accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                {
                    TenantId = tenantId,
                    AdminUrl = siteAdminUrl,
                    ClientId = clientId,
                    ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    AppType = RMAosApiClient.ConvertIdentityTypeToAppType((CloudAos.IdentityProviderType)profile.AppType),
                    AuthenticationProfileId = profile.Id,
                    AADEnvironment = (AveAzureEnvironment)profile.AADEnvironment,
                    //AppCert = apponlyCertificate
                };
            }

            return accountInfo;
        }

        /*private static PoolUserDto GetPoolUser(string siteUrl, string tenantId)
        {
            PoolUserDto user = null;
            lock (locker)
            {
                if (urlBposInfoMapping.ContainsKey(siteUrl))
                {
                    user = urlBposInfoMapping[siteUrl];
                }
                else
                {
                    user = UserService.GetAvailableUser(tenantId);
                    if (user != null)
                    {
                        urlBposInfoMapping.Add(siteUrl, user);
                    }
                }
            }

            return user;

        }

        public static void Dispose()
        {
            lock (locker)
            {
                if (urlBposInfoMapping != null && urlBposInfoMapping.Count > 0)
                {
                    urlBposInfoMapping = new Dictionary<string, PoolUserDto>();
                }
            }

        }*/

    }
}
