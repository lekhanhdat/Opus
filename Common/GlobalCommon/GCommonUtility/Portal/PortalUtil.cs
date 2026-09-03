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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Feedback.Object;
using AvePoint.GCommon.Contract.Gateway.Object;
using BugTypeDao = AvePoint.GCommon.Contract.Feedback.Object.BugType;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Utility.Cloud;
using System.Configuration;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Utility.Cryptography;
using DaoRemoteNodeType = AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType;
using DaoSiteCollectionType = AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System.Security.Cryptography.X509Certificates;
using AvePoint.GCommon.Contract.Server.ControlPanel.Passphrase;
using System.Threading;

using Office365UserRole = AvePoint.GCommon.Contract.CentralAdmin.Object.Office365UserRole;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
using Microsoft.Extensions.DependencyInjection;
using Cloud.Sdk.Core;
using Cloud.Sdk.Aos;
using System.Net.Http;
using CloudAos = Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token;
using Cloud.Sdk.CloudInsights;
using System.Threading.Tasks;
using Cloud.Sdk.Data.Aos.SecurityProfile;
using AvePoint.GCommon.Utility.Portal.Logger;
using Cloud.Sdk.Dao;
using Cloud.Sdk.Aos.Services;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using Cloud.Sdk.Data.AosModern;
using AvePoint.GCommon.Contract.Storage.Entity;
using Cloud.Sdk.AosModern;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.Common.Portal
{
    public partial class PortalUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(PortalUtil));
        public static bool HasInit = false;
        public const string ProductName = "AvePointRecords";//"DocAve";
        public const string CloudArchiving = "Office365Archiving";
        public const string Archiver = "Archiver";
        public const string AppUrl = "https://www.docaveonline.com";
        public const string DefaultSecurityProfile = "Default Security Profile";
        public const string IdSystemKeyVault = "id_system_keyvault";


        #region Portal AOS SDK
        public static AvePoint.GCommon.Utility.AosTokenResult GetTokenByAOSNewSDK(AvePoint.GCommon.Utility.TokenParam param)
        {
            try
            {
                if (param == null || (param.SpTokenType != AvePoint.GCommon.Utility.SharePointTokenType.Bearer && param.SpTokenType != AvePoint.GCommon.Utility.SharePointTokenType.IDCRL))
                {
                    throw new ArgumentException("Current token param is invalid");
                }
                System.Threading.Tasks.Task<TokenResult> result = null;

                if (param.SpTokenType == AvePoint.GCommon.Utility.SharePointTokenType.IDCRL)
                {
                    result = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(param.CustomerId).ModernTokenService.GetSharePointTokenAsync(
                        param.Identity,
                        new Uri(param.SiteUrl).GetLeftPart(UriPartial.Authority),
                        CloudAos.SharePointTokenType.Bearer
                        );
                }
                else
                {
                    if (TenantLocalValue.CallerType == "PartnerPortal" || param.AppType == GCommon.Contract.CentralAdmin.Object.AppType.AOSPTokenApp || param.AppType == GCommon.Contract.CentralAdmin.Object.AppType.AospCustomDelegateApp)
                    {
                        logger.Info("Current callerType is PartnerPortal, get token from aosp app");
                        var client = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(param.CustomerId);
                        result = client.ImpersonateCallerInvoke<ModernTokenApiClient, TokenResult?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                        {
                            var result = await client.ModernTokenService.GetTokenByAppProfileAsync(
                               GetIdentityProviderType(param.AppType),
                               TokenResourceType.SharePoint,
                               param.TenantId,
                               param.Identity,
                               new Uri(param.SiteUrl).GetLeftPart(UriPartial.Authority),
                               TokenType.ApplicationToken
                            );
                            return result;
                        });
                        var aospToken = result.GetAwaiter().GetResult();
                        if (aospToken == null)
                        {
                            result = client.ImpersonateCallerInvoke<ModernTokenApiClient, TokenResult?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                            {
                                var result = await client.ModernTokenService.GetTokenByAppProfileAsync(
                                   IdentityProviderType.AospSecurityAnalysisCsp,
                                   TokenResourceType.SharePoint,
                                   param.TenantId,
                                   param.Identity,
                                   new Uri(param.SiteUrl).GetLeftPart(UriPartial.Authority),
                                   TokenType.ApplicationToken
                                );
                                return result;
                            });
                        }
                    }
                    else
                    {
                        result = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(param.CustomerId).ModernTokenService.GetTokenByAppProfileAsync(
                           GetIdentityProviderType(param.AppType),
                           TokenResourceType.SharePoint,
                           param.TenantId,
                           param.Identity,
                           new Uri(param.SiteUrl).GetLeftPart(UriPartial.Authority),
                           TokenType.ApplicationToken
                        );
                    }
                }
                //new Uri(param.SiteUrl).GetLeftPart(UriPartial.Authority),
                var token = result.GetAwaiter().GetResult();
                logger.Info("Get token with MSAL from AOS finish.");
                if (token != null)
                {
                    if (string.IsNullOrWhiteSpace(token.Error))
                    {
                        return new AosTokenResult
                        {
                            AccessToken = token.AccessToken,
                            Error = token.Error,
                            ExpiresOn = token.ExpiresOn
                        };
                    }
                    else
                    {
                        throw new Exception(token.Error);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occured when get aos token failed due to {0}", e);
            }
            return default(AvePoint.GCommon.Utility.AosTokenResult);
        }

        private static IdentityProviderType GetIdentityProviderType(AvePoint.GCommon.Contract.CentralAdmin.Object.AppType appType)
        {
            return appType switch
            {
                GCommon.Contract.CentralAdmin.Object.AppType.Office365 => IdentityProviderType.Office365,
                GCommon.Contract.CentralAdmin.Object.AppType.Exchange => IdentityProviderType.Exchange,
                GCommon.Contract.CentralAdmin.Object.AppType.SharePoint => IdentityProviderType.SharePoint,
                GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp => IdentityProviderType.CustomAzureApp,
                GCommon.Contract.CentralAdmin.Object.AppType.CustomDelegateApp => IdentityProviderType.CustomDelegateApp,
                GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords => IdentityProviderType.CloudRecords,
                GCommon.Contract.CentralAdmin.Object.AppType.AOSPTokenApp => IdentityProviderType.AospSecurityAnalysis,
                GCommon.Contract.CentralAdmin.Object.AppType.AospCustomDelegateApp => IdentityProviderType.AospCustomDelegateApp,
                _ => IdentityProviderType.Office365,
            };
        }

        public static AvePoint.GCommon.Utility.AosTokenResult GetTokenByAOSNewSDKForEXO(AvePoint.GCommon.Utility.TokenParam param)
        {
            try
            {
                if (param == null || (param.SpTokenType != AvePoint.GCommon.Utility.SharePointTokenType.Bearer && param.SpTokenType != AvePoint.GCommon.Utility.SharePointTokenType.IDCRL))
                {
                    throw new ArgumentException("Current token param is invalid");
                }
                System.Threading.Tasks.Task<TokenResult> result = null;
                if (param.SpTokenType == AvePoint.GCommon.Utility.SharePointTokenType.IDCRL)
                {
                    result = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(param.CustomerId).ModernTokenService.GetTokenByServiceAccountAsync(
                        TokenResourceType.ExchangeWebService,
                        param.Identity,
                        param.Resource,
                        param.ClientId
                        );
                }
                else
                {
                    result = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(param.CustomerId).ModernTokenService.GetTokenByAppProfileAsync(
                        GetIdentityProviderType(param.AppType),
                        TokenResourceType.ExchangeWebService,
                        param.TenantId,
                        param.Identity,
                        param.Resource,
                        TokenType.ApplicationToken
                    );
                }

                var token = result.GetAwaiter().GetResult();
                logger.Info("Get token with MSAL from AOS finish.");
                if (token != null)
                {
                    if (string.IsNullOrWhiteSpace(token.Error))
                    {
                        return new AosTokenResult
                        {
                            AccessToken = token.AccessToken,
                            Error = token.Error,
                            ExpiresOn = token.ExpiresOn
                        };
                    }
                    else
                    {
                        throw new Exception(token.Error);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occured when get aos token failed due to {0}", e);
            }
            return default(AvePoint.GCommon.Utility.AosTokenResult);
        }

        public static string Encrypt(string plainText, string groupId = "")
        {
            if (string.IsNullOrEmpty(plainText))
            {
                logger.Warn("Encrypt,but plainText is empty.");
                return plainText;
            }
            if (string.IsNullOrEmpty(groupId))
            {
                groupId = TenantThreadLocalValue.LogonGroupId;
            }
            var cipherText = Execute(() => AosApiUtility.AosClient.SecurityProfileService.Encrypt(new TenantEncryptInfo
            {
                CustomerId = groupId,
                PlainText = plainText
            }));
            return cipherText;
        }

        public static string Decrypt(string cipherText, string groupId = "")
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                logger.Warn("Decrypt,but cipherText is empty.");
                return cipherText;
            }
            if (string.IsNullOrEmpty(groupId))
            {
                groupId = TenantThreadLocalValue.LogonGroupId;
            }
            var plainText = Execute(() => AosApiUtility.AosClient.SecurityProfileService.Decrypt(new TenantEncryptInfo
            {
                CustomerId = groupId,
                PlainText = cipherText
            }));
            return plainText;
        }

        public static SecurityProfileResult GetSecurityProfilesSummary(string groupId)
        {
            SecurityProfileResult result = new SecurityProfileResult();
            List<SecurityProfileNameAndId> securityProfiles = new List<SecurityProfileNameAndId>();
            var profiles = Execute(() => AosApiUtility.AosClient.SecurityProfileService.GetAllSecurityProfiles(groupId));
            if (profiles != null && profiles.Any())
            {
                profiles.ForEach(p =>
                {
                    var profile = new SecurityProfileNameAndId
                    {
                        Id = p.Id,
                        Name = p.Name,
                    };
                    if (p.Status == Cloud.Sdk.Data.Aos.SecurityProfile.SecurityProfileStatus.Applied)
                    {
                        result.DefaultSecurityProfileId = p.Id;
                    }
                    securityProfiles.Add(profile);
                });
            }
            result.SecurityProfiles = securityProfiles;
            return result;
        }

        public static AOSSecurityProfile GetSecurityProfileById(string profileId)
        {

            var profile = Execute(() => AosApiUtility.AosClient.SecurityProfileService.GetSecurityProfileById(profileId));
            if (profile != null)
            {
                if (profile.Name.StartsWith(DefaultSecurityProfile + "_"))
                {
                    profile.Name = DefaultSecurityProfile;
                }
                return new AOSSecurityProfile
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    SecurityProfileType = (int)profile.Type,
                    KeyIdentity = profile.KeyIdentity,
                    ClientId = profile.ClientId,
                    ClientSecret = profile.ClientSecret
                };
            }
            return null;
        }

        public static void UpdateSecurityProfileInUse(string profileId, bool inUse)
        {
            var isSuccess = Execute(() => AosApiUtility.AosClient.SecurityProfileService.UpdateProfileRelatedProductStatus(profileId, ProductName, inUse));
            if(!isSuccess)
            {
                throw new Exception("Update security profile failed");
            }

        }

        #endregion
        #region Execute Async

        public static T Execute<T>(Func<Task<T>> func)
        {
            try
            {
                return Task.Run(async () => await func()).Result;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }

        #endregion
    }

    #region Compare

    class UserInfoComparer : IEqualityComparer<UserInfo>
    {

        public bool Equals(UserInfo x, UserInfo y)
        {
            // check whether both the objects reference the same data 
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }
            // check whether any of the object is null         
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
            {
                return false;
            }
            // check whether the properties are equal
            return x.Id == y.Id;
        }

        public int GetHashCode(UserInfo obj)
        {
            // check whether the object is null        
            if (Object.ReferenceEquals(obj, null))
            {
                return 0;
            }
            return obj.Id == null ? 0 : obj.Id.GetHashCode();
        }
    }

    #endregion

}
