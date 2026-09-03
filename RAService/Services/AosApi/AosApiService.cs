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
using Aos.Sdk;
using Aos.Sdk.Models;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AosApi
{
    /// <summary>
    /// 这个类计划被弃用,AOS API请使用 RAPortalUtil
    /// </summary>
    public class AosApiService : IAosApiService
    {
        private static readonly object APIInitLock = new object();
        private static RALogger logger = RALogger.GetInstance(typeof(AosApiService));
        private static Dictionary<string, DateTime> _AccesstokenTimeoutCache = new Dictionary<string, DateTime>();
        private static Dictionary<string, string> _AccesstokenCache = new Dictionary<string, string>();
        private static AOSEncryptionProfile systemSecurityProfile = null;

        public AosApiService()
        {
            try
            {
                InitializeAosSDK();
            }
            catch (Exception e)
            {
                logger.Error("Error occurred while initializing aos sdk", e);
            }
        }

        public bool VerifySignature(string product, string data, string signature)
        {
            try
            {
                return RunWithRetry(() => {
                    var publicKey = Aos.Sdk.AosApi.PublicKeyService.GetPublicKey(product);
                    Aos.Sdk.AosApi.PublicKeyService.GetPublicKey(ProductType.COP.Name);
                    var digitalSignatureHelperFactory = new DigitalSignatureHelperFactory();
                    var digitalSignatureHelper = digitalSignatureHelperFactory.CreateByPublickKey(publicKey);
                    var flag = digitalSignatureHelper.Verify(data, signature);
                    logger.Debug("VerifySignature {0}|{1}|{2}|{3}", product, data, signature, flag);
                    return flag;
                });
            }
            catch (Exception e)
            {
                logger.Error("VerifySignature error: {0}", e.ToString());
                throw;
            }
        }

        public List<string> GetAvailableTenants()
        {
            //暂时先用DocAve Online的License做控制，以后AOS添加对RECO的License控制以后，需要改过来
            return RunWithRetry(() => Aos.Sdk.AosApi.CustomerService.GetCustomersByAvaliableProduct(ProductType.AvePointRecords.Name));
        }

        //by Tenant Group Id
        public List<AuthenticationProfile> GetSPOnlineAuthenticationProfiles(string id)
        {
            try
            {
                return RunWithRetry(() => {
                    var profiles1 = Aos.Sdk.AosApi.AuthenticationService.GetAuthenticationProfiles(id, IdentityProviderType.SharePointOnline);
                    var profiles2 = Aos.Sdk.AosApi.AuthenticationService.GetAuthenticationProfiles(id, IdentityProviderType.SharePoint);
                    if (profiles2 != null)
                    {
                        profiles1.AddRange(profiles2);
                    }
                    profiles1.ForEach(p => CacheAccessToken(p.TenantId, p.AccessToken));
                    return profiles1;
                });
            }
            catch (Exception e)
            {
                logger.Error("Get Authentication Profiles failed. Tenant Group Id: {0}, Error: {1}.", id, e.ToString());
            }
            return null;
        }

        public string GetSPOnlineAccessTokenByTenantId(string tenantId)
        {
            string token = null;
            if (_AccesstokenCache.TryGetValue(tenantId, out token))
            {
                DateTime dt;
                if (!string.IsNullOrWhiteSpace(token) && _AccesstokenTimeoutCache.TryGetValue(tenantId, out dt) && DateTime.UtcNow <= dt)
                {
                    return token;
                }
            }

            token = GetAccessToken(tenantId);
            CacheAccessToken(tenantId, token);

            return token;
        }

        public AccountInfo ValidateAccount(string user, string password, string product)
        {
            try
            {
                Aos.Sdk.Models.AccountInfo account = Aos.Sdk.AosApi.UserService.LogOn(user, password, product);
                if (account != null)
                {
                    return account;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public AOSSecurityProfile GetSecurityProfileById(string profileId)
        {
            var profile = Aos.Sdk.AosApi.SecurityProfileService.GetSecurityProfileById(profileId);
            if (profile != null)
            {
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

        public static AOSEncryptionProfile GetSystemSecurityProfile()
        {
            if (systemSecurityProfile == null)
            {
                var systemProfile = Aos.Sdk.AosApi.SecurityProfileService.GetSystemSecurityProfile();
                systemSecurityProfile = new AOSEncryptionProfile
                {
                    Id = systemProfile.Id,
                    Name = systemProfile.Name,
                    SecurityProfileType = (int)systemProfile.Type,
                    KeyIdentity = systemProfile.KeyIdentity,
                    ClientId = systemProfile.ClientId,
                    ClientSecret = systemProfile.ClientSecret
                };
                return systemSecurityProfile;

            }
            else
            {
                return systemSecurityProfile;
            }
        }

        public static AOSEncryptionProfile GetCurrentAppliedSecurityProfileByGroupId(string groupId)
        {
            var profile = Aos.Sdk.AosApi.SecurityProfileService.GetActiveSecurityProfile(groupId);
            if (profile != null)
            {
                logger.Info("Get current applied profile from AOS,name {0},id {1}", profile.Name, profile.Id);
                return new AOSEncryptionProfile
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

        public static AOSEncryptionProfile GetDefaultSecurityProfileByGroupId(string groupId)
        {
            var profile = Aos.Sdk.AosApi.SecurityProfileService.GetDefaultSecurityProfile(groupId);
            if (profile != null)
            {
                return new AOSEncryptionProfile
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

        public void UpdateSyncJob(string aosProfileId, string groupId, bool isFailed)
        {
            Aos.Sdk.Models.SecurityProfile.SecurityProfileApply applyInfo = new Aos.Sdk.Models.SecurityProfile.SecurityProfileApply
            {
                ApplyId = aosProfileId,
                CustomerId = groupId,
                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                State = isFailed ? TenantJobState.Failed : TenantJobState.Finished,
            };
            Aos.Sdk.AosApi.SecurityProfileService.UpdateApplyJobStatus(applyInfo);
        }

        private string GetAccessToken(string tenantId)
        {
            try
            {
                return RunWithRetry(() => Aos.Sdk.AosApi.AuthenticationService.GetAccessTokenByType(tenantId, IdentityProviderType.SharePointOnline));
            }
            catch (Exception e)
            {
                logger.Error("Get access token failed. TenantId: {0}, Error: {1}.", tenantId, e.ToString());
            }
            return null;
        }

        private void CacheAccessToken(string tenantId, string accessToken)
        {
            _AccesstokenCache[tenantId] = accessToken;
            _AccesstokenTimeoutCache[tenantId] = DateTime.UtcNow.AddMinutes(10); // Access Token 每十分钟重新获取
        }


        #region private functions
        private void InitializeAosSDK()
        {
            lock (APIInitLock)
            {
                string token = new ClientSecurityService().GetLoginSignature(RecordsConstants.RECORDS_APPLICATION_NAME, "https://www.cloudrecords.com");
                LoginInfo loginInfo = new LoginInfo
                {
                    ApiUrl = RMGlobalConfiguration.CommonConfig[Contract.Configurations.RMCommonSettingKey.PortalApiUrl],
                    AppUrl = "https://www.cloudrecords.com",
                    Signature = token,
                    Type = RecordsConstants.RECORDS_APPLICATION_NAME,
                };

                logger.Info("Initialize api.");
                Aos.Sdk.AosApi.Init(loginInfo);
                logger.Info("Initialize api success.");
            }
        }

        private T RunWithRetry<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch
            {
                InitializeAosSDK();
                return action();
            }
        }

        private void RunWithRetry(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                InitializeAosSDK();
                action();
            }
        }

        public List<UserInfo> SearchUsers(string tenantId, string searchString)
        {
            return Aos.Sdk.AosApi.UserService.GetUser(tenantId, RecordsConstants.RECORDS_APPLICATION_NAME).Where(u => u.Name.Contains(searchString) || u.Email.Contains(searchString)).ToList();
        }


        #endregion
    }
}
