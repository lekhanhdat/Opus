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
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Microsoft365.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.SharePoint.Common
{
    public class PoolUserUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(PoolUserUtil));
        private static bool isEnableAccountPool = false;
        private static IUserService UserService = null;
        private static Dictionary<string, PoolUserDto> urlBposInfoMapping = null;
        private readonly static object locker = new object();
        private static IRMAppProfileDao RMAppProfileDao = null;
        private static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

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

                logger.Info("get site bpos info,siteUrl:{0}, AuthType:{1},userName:{2}, AADEnvironment:{3}", siteUrl, info.ConnectionType, info?.UserAccountInfo?.Username, info.UserAccountInfo.AADEnvironment);

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
                        var url = string.IsNullOrEmpty(info.SiteUrl) ? siteUrl: info.SiteUrl;
                        var siteNode = RemoteNodeService?.GetRemoteSiteCollectionByUrl(url);
                        username = siteNode?.username;
                        //username = info.UserAccountInfo.Username;
                        ArgumentCheck.NotNull(siteNode, nameof(siteNode));
                        domain = ".".Equals(siteNode.domain) ? string.Empty : siteNode.domain;
                        password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
                    }

                    accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                    {
                        Domain = domain,
                        UserName = username,
                        Password = password.ToSecureStringWithEmptyCheck(),
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
                    logger.Info($"Use app profile: {profile?.Id}, type: {profile?.AppType}");
                    //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
                    accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                    {
                        TenantId = tenantId,
                        AdminUrl = siteAdminUrl,
                        ClientId = clientId,
                        ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                        TenantGroupId = TenantLocalValue.LogonGroupId,
                        AuthenticationProfileId = profile.Id,
                        AppType = RMAosApiClient.ConvertIdentityTypeToAppType((CloudAos.IdentityProviderType)profile.AppType),
                        AADEnvironment = (AveAzureEnvironment)profile.AADEnvironment,
                        //AppCert = apponlyCertificate
                    };
                }

            }

            return accountInfo;
        }
        public static async Task<Wrapper.Common.AveBPOSAccountInfo> GetBPOSInfoAsync(RemoteSiteCollection site, bool useCache = true, bool enableMultipleAppProfile = true)
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
            logger.Info("Get site bpos info, RemoteSiteUrl:{0}, AuthType:{1} enableMultipleAppProfile:{2}", site.url, site.AuthType, enableMultipleAppProfile);

            var tenantId = site.TenantId;
            RMAosAuthenticationProfile profile = null;
            if (enableMultipleAppProfile)
            {
                RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));
                var bestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
                if (bestApp != null)
                {
                    profile = RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, tenantId, bestApp.AppClientId.ToString(), (CloudAos.IdentityProviderType)bestApp.AppType, false);
                }
                if (profile == null)
                {
                    logger.Warn("App profile no longer exist, try to get app profiles from aos again.");
                    var authenticationProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, new List<string>() { tenantId });
                    logger.Info($"Get app profiles from aos finished. Count:{authenticationProfiles?.Count}");
                    if (authenticationProfiles != null && authenticationProfiles.Count > 0)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(tenantId), authenticationProfiles.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                        var newBestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
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
                logger.Info("No available app profile. Use Service Account that bound with site.");
                accountInfo = GetConnectionSAInfo(site);
                accountInfo.ExsitAppProfile = false;
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

        public static Wrapper.Common.AveBPOSAccountInfo GetAOSPBPOSInfo(string m365tenantId)
        {
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            FipsModeUtil.InitControlCryptoMode();

            var aospProfile = RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, m365tenantId).GetAwaiter().GetResult();
            if (aospProfile == null)
            {
                throw new Exception("RM_JM_AppProfile_NotFoundError");
            }
            else
            {
                var clientId = aospProfile.AppClientId;
                logger.Info($"Record aosp used multiple app: [{aospProfile?.Id} - {aospProfile.Type}]");
                //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
                accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
                {
                    Id = aospProfile.Id,
                    TenantId = aospProfile.TenantId,
                    AdminUrl = aospProfile.AdminUrl,
                    ClientId = clientId,
                    ConnectionType = Wrapper.Common.BposConnectionType.AppToken,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    AppType = RMAosApiClient.ConvertIdentityTypeToAppType((CloudAos.IdentityProviderType)(int)aospProfile.Type),
                    AuthenticationProfileId = aospProfile.Id,
                    AADEnvironment = (AveAzureEnvironment)aospProfile.AADEnvironment,
                    ExsitAppProfile = true,
                    //AppCert = apponlyCertificate
                };
                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(m365tenantId).GetAwaiter().GetResult().AdminUrl;
                logger.Info($"GetAOSPBPOSInfo.Reset adminURL.aospProfile.AdminUrl:{aospProfile.AdminUrl}.ResetURL:{adminUrl}.");
                accountInfo.AdminUrl = adminUrl;
            }
            return accountInfo;
        }

        /// <summary>
        /// If the dedicated app is configured, it will be used first; otherwise, the best available app will be used.
        /// It is currently only available for use with L'Oreal's Restore function.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="useCache"></param>
        /// <param name="enableMultipleAppProfile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Wrapper.Common.AveBPOSAccountInfo> GetBPOSInfo2Async(RemoteSiteCollection site, bool useCache = true, bool enableMultipleAppProfile = true)
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
            logger.Info("Get site bpos info, RemoteSiteUrl:{0}, AuthType:{1} enableMultipleAppProfile:{2}", site.url, site.AuthType, enableMultipleAppProfile);

            var tenantId = site.TenantId;
            RMAosAuthenticationProfile profile = null;
            if (enableMultipleAppProfile)
            {
                var tenantGuid = new Guid(tenantId);

                RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));

                var bestDedicatedApp = RMAppProfileDao.GetBestDedicatedAppProfile(tenantGuid);
                var bestApp = bestDedicatedApp ?? RMAppProfileDao.GetBestAppProfile(tenantGuid);

                if (bestApp != null)
                {
                    var usedAppType = bestDedicatedApp != null ? "dedicated" : "general";
                    logger.Info($"The best {usedAppType} app id is: [{bestApp.AppClientId}], profile id is: [{bestApp.Id}]");

                    profile = RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, tenantId, bestApp.AppClientId.ToString(), (CloudAos.IdentityProviderType)bestApp.AppType, false);
                }
                if (profile == null)
                {
                    logger.Warn("App profile no longer exist, try to get app profiles from aos again.");
                    var authenticationProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, new List<string>() { tenantId });
                    logger.Info($"Get app profiles from aos finished. Count:{authenticationProfiles?.Count}");
                    if (authenticationProfiles != null && authenticationProfiles.Count > 0)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(tenantId), authenticationProfiles.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                        var newBestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
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
                logger.Info("No available app profile. Use Service Account that bound with site.");
                accountInfo = GetConnectionSAInfo(site);
                accountInfo.ExsitAppProfile = false;
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

        public static async Task<Wrapper.Common.AveBPOSAccountInfo> GetAOSPBPOSInfoAsync(string appProfileId, string siteAdminUrl)
        {
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            RMAosAuthenticationProfile profile = null;

            profile = await RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, appProfileId);
            if(profile == null)
            {
                return accountInfo;
            }

            var clientId = profile.AppClientId;
            logger.Info($"Record used multiple app: [{profile?.Id} - {profile.AppType}]");
            //X509Certificate2 apponlyCertificate = RMAosApiClient.GetAppCertificate(profile?.AppCertSecret, profile?.AppCertContent, profile?.AppCertSecretContent);
            accountInfo = new Wrapper.Common.AveBPOSAccountInfo()
            {
                TenantId = profile.TenantId,
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

            return accountInfo;
        }

        public static async Task<CloudAos.AppProfileInfo> GetBPOSInfoAsync(string O365TenantId, bool useCache = true, bool enableMultipleAppProfile = true)
        {
            logger.Info($"Get bpos info, O365TenantId:{O365TenantId} enableMultipleAppProfile:{enableMultipleAppProfile}");
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            var tenantId = O365TenantId;
            CloudAos.AppProfileInfo appProfile = null;
            if (enableMultipleAppProfile)
            {
                RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));
                var bestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
                if (bestApp != null)
                {
                    logger.Info($"bestApp is not null,LogonGroupId is {TenantLocalValue.LogonGroupId}");
                    appProfile = RMAosApiClient.GetProfileByAppId(TenantLocalValue.LogonGroupId, O365TenantId, bestApp.AppClientId.ToString(), (CloudAos.IdentityProviderType)bestApp.AppType, useCache);
                }
                if (appProfile == null)
                {
                    logger.Warn("App profile no longer exist, try to get app profiles from aos again.");
                    var authenticationProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, new List<string>() { tenantId });
                    logger.Info($"Get app profiles from aos finished. Count:{authenticationProfiles?.Count}");
                    if (authenticationProfiles != null && authenticationProfiles.Count > 0)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(tenantId), authenticationProfiles.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                        var newBestApp = RMAppProfileDao.GetBestAppProfile(new Guid(tenantId));
                        if (newBestApp != null)
                        {
                            logger.Info($"newBestApp is not null,LogonGroupId is {TenantLocalValue.LogonGroupId}");
                            appProfile = RMAosApiClient.GetProfileByAppId(TenantLocalValue.LogonGroupId, O365TenantId, bestApp?.AppClientId.ToString(), (CloudAos.IdentityProviderType)newBestApp.AppType, useCache);
                        }
                    }
                }
            }
            else
            {
                appProfile = RMAosApiClient.GetAppProfile(TenantLocalValue.LogonGroupId, O365TenantId, useCache);
            }

            logger.Info($"Record used multiple app: [{appProfile?.Id} - {appProfile?.Type}]");

            return appProfile;
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

        public static async Task<(Wrapper.Common.AveBPOSAccountInfo,bool)> GetCustomBPOSInfoAsync(RemoteSiteCollection site, string appId, bool useCache = true)
        {
            bool useSpeicalApp = false;
            logger.Info($"Custom app profile exist: {!string.IsNullOrWhiteSpace(appId)}");
            Wrapper.Common.AveBPOSAccountInfo accountInfo = null;
            if (site == null)
            {
                throw new Exception(string.Format("Get GetCustomBPOSInfo Failed, Site not found."));
            }
            FipsModeUtil.InitControlCryptoMode();
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            var siteAdminUrl = string.IsNullOrEmpty(site.AdminUrl) ? WebUtil.GetSPAdminUrl(site.url, site.TenantId) : site.AdminUrl;
            logger.Info("Get site bpos info, RemoteSiteUrl:{0}, AuthType:{1}", site.url, site.AuthType);

            var tenantId = site.TenantId;
            RMAosAuthenticationProfile profile = RMAosApiClient.GetAuthProfileByAppId(TenantLocalValue.LogonGroupId, tenantId, appId, CloudAos.IdentityProviderType.CustomAzureApp, useCache);

            if (profile == null)
            {
                logger.Info("No available app profile. Use Service Account that bound with site.");
                useSpeicalApp = false;
                return (await GetBPOSInfoAsync(site), useSpeicalApp);
            }
            else
            {
                var clientId = profile.AppClientId;
                //logger.Info($"Get custom app profile: {clientId}");
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
            useSpeicalApp = true;
            return (accountInfo, useSpeicalApp);
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

        public static List<Wrapper.Common.AveBPOSAccountInfo> GetSAInfoFromAOS(string O365TenantId)
        {
            List<Wrapper.Common.AveBPOSAccountInfo> accounts = new List<Wrapper.Common.AveBPOSAccountInfo>();

            logger.Info("Site user name is empty, so will get active accounts.");
            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(O365TenantId).GetAwaiter().GetResult().AdminUrl;
            foreach (var account in RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, O365TenantId))
            {
                if (account.Status == CloudAos.ServiceAccountStatus.Active)
                {
                    var info = new Wrapper.Common.AveBPOSAccountInfo()
                    {
                        Domain = account.DomainName,
                        UserName = account.UserName,
                        Password = account.Password.ToSecureString(),
                        AdminUrl = adminUrl,
                        ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                        TenantGroupId = TenantLocalValue.LogonGroupId,
                        AADEnvironment = (AveAzureEnvironment)account.AADEnvironment,
                        TenantId = account.TenantId
                    };
                    accounts.Add(info);
                }
                else
                {
                    logger.Warn($"The service account {account.UserName} Status is {account.Status}.");
                }
            }

            return accounts;
        }

        public static List<CloudAos.AppProfileInfo> GetCustomAppProfilesForSensitivityLabel(string O365TenantId)
        {
            List<CloudAos.AppProfileInfo> accounts = new List<CloudAos.AppProfileInfo>();

            logger.Info("Site user name is empty, so will get active accounts.");
            var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(O365TenantId).GetAwaiter().GetResult().AdminUrl;
            foreach (var profile in RMAosApiClient.GetAllCustomAppProfiles(TenantLocalValue.LogonGroupId, O365TenantId))
            {
                if (profile.Status == CloudAos.AppProfileStatus.Active)
                {
                    accounts.Add(profile);
                }
                else
                {
                    logger.Warn($"The app {profile.Name} Status is {profile.Status}.");
                }
            }
            logger.Info($"GetCustomAppProfilesForSensitivityLabel count {accounts.Count}.");
            return accounts;
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

        }
    }
}
