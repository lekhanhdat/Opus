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
using Aos.Sdk.Models.Tenant;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public partial class RAPortalUtil
    {
        public const string DefaultSecurityProfile = "Default Security Profile";
        public const string IdSystemKeyVault = "id_system_keyvault";
        private static AOSSecurityProfile systemSecurityProfile = null;

        private static RALogger logger = RALogger.GetInstance(typeof(RAPortalUtil));
        public static void Init()
        {
            try
            {
                logger.Debug("Init Portal Aos Api SDK");

                var appUrl = RMGlobalConfiguration.CommonConfig[Contract.Configurations.RMCommonSettingKey.AppUrl];
                var recordsCert = RMCertificateHelper.GetX509Certificate2(RMCertNames.AvePointRecords);
                string token = new ClientSecurityService().GetLoginSignature(RecordsConstants.RECORDS_APPLICATION_NAME, appUrl, recordsCert);
                LoginInfo loginInfo = new LoginInfo
                {
                    ApiUrl = RMGlobalConfiguration.CommonConfig[Contract.Configurations.RMCommonSettingKey.PortalApiUrl],
                    AppUrl = appUrl,
                    Signature = token,
                    Type = RecordsConstants.RECORDS_APPLICATION_NAME,
                };

                var retryCount = 5;
                while (retryCount > 0)
                {
                    try
                    {
                        Aos.Sdk.AosApi.Init(loginInfo, recordsCert);
                        logger.Debug("Init Portal Aos Api SDK success");
                        retryCount = 0;
                        break;
                    }
                    catch (ApiException e)
                    {
                        retryCount--;
                        if (retryCount <= 0)
                        {
                            throw;
                        }
                        System.Threading.Thread.Sleep(5000);
                        logger.Warn(string.Format("Retry Times:{0}\r\n Error:{1}, api error code:{2}", retryCount, e.ToString(), e.ErrorCode));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Init error: {0}", e.ToString());
                throw;
            }
        }
        public static AOSSecurityProfile GetSecurityProfileById(string profileId)
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
        

        public static AOSSecurityProfile GetSystemSecurityProfile()
        {
            if (systemSecurityProfile == null)
            {
                var systemProfile = Aos.Sdk.AosApi.SecurityProfileService.GetSystemSecurityProfile();
                systemSecurityProfile = new AOSSecurityProfile
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

        public static AOSSecurityProfile GetCurrentAppliedSecurityProfileByGroupId(string groupId)
        {
            var profile = Aos.Sdk.AosApi.SecurityProfileService.GetActiveSecurityProfile(groupId);
            if (profile != null)
            {
                logger.Info("Get current applied profile from AOS,name {0},id {1}", profile.Name, profile.Id);
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

        public static AOSSecurityProfile GetDefaultSecurityProfileByGroupId(string groupId)
        {
            var profile = Aos.Sdk.AosApi.SecurityProfileService.GetDefaultSecurityProfile(groupId);
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

        public static List<AuthenticationProfile> GetSPOnlineAuthenticationProfiles(string spTenandId)
        {
            try
            {
                var profiles = Aos.Sdk.AosApi.AuthenticationService.GetAuthenticationProfiles(spTenandId, IdentityProviderType.SharePointOnline);
                return profiles;
            }
            catch (Exception e)
            {
                logger.Error("Get Authentication Profiles failed. Tenant Group Id: {0}, Error: {1}.", spTenandId, e.ToString());
            }
            return null;
        }

        public static AuthenticationProfile GetSPOnlineProfileByTenantId(string curTenantGroupId,string spTenandId)
        {
            try
            {
                var profile = AosApi.AuthenticationService.GetAuthenticationProfiles(curTenantGroupId, IdentityProviderType.SharePointOnline).Find(p => p.TenantId == spTenandId);
                if (profile == null)
                {
                    profile = AosApi.AuthenticationService.GetAuthenticationProfiles(curTenantGroupId, IdentityProviderType.SharePoint).Find(p => p.TenantId == spTenandId);
                }
                if (profile == null)
                {
                    throw new Exception(string.Format("profile not found, sptenantId:{0}", spTenandId));
                }
                return profile;
            }
            catch (Exception e)
            {
                logger.Error("Get Authentication Profiles failed. Tenant Group Id: {0}, Error: {1}.", spTenandId, e.ToString());
            }
            return null;
        }

        public static Dictionary<string, string> GetServiceAccounts(string customerId, List<string> userNames)
        {
            Dictionary<string, string> userNameToPassDict = new Dictionary<string, string>();
            var param = new Aos.Sdk.Models.Tenant.CustomerAccountList() { CustomerId = customerId, AccountList = userNames };
            List<Aos.Sdk.Models.Tenant.ServiceAccount> serviceAccounts = AosApi.TenantService.GetServiceAccountsByAccountList(param);
            foreach (Aos.Sdk.Models.Tenant.ServiceAccount serviceAccount in serviceAccounts)
            {
                string password = CspCommunicationWrapper.WrapKeyToBase64String(CspCrossPlatformExchangeWrapper.UnWrapKey(serviceAccount.Password));
                userNameToPassDict.Add(serviceAccount.UserName, password);
            }
            return userNameToPassDict;
        }

        public static List<PoolUserDto> GetAccountPoolUsers(string customerId)
        {
            List<ServiceAccount> resultList = new List<ServiceAccount>();
            var tenantIds = AosApi.TenantService.GetServiceAccount(customerId).GroupBy(o => o.TenantId).Select(s => s.Key).ToList();
            foreach (var id in tenantIds)
            {
                try
                {
                    resultList.AddRange(AosApi.TenantService.GetAccountPool(customerId, id, AccountPoolObjectType.Site));
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while get pool user by tenantId:{0},ERROR:{1}", customerId, ex.ToString());
                }
                
            }
            return resultList.ConvertAll(o => ConvertToPoolUserDto(o));
        }

        public static string GetServiceAccountPassword(string customerId, string userName)
        {
            string password = string.Empty;
            Aos.Sdk.Models.Tenant.ServiceAccount serviceAccount = AosApi.TenantService.GetServiceAccountByAccountName(customerId, userName);
            try
            {
                password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(serviceAccount.Password));
            }
            catch
            {
                password = serviceAccount.Password;
            }
            return password;
        }

        public static void UpdateSyncJob(string aosProfileId, string groupId, bool isFailed)
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

        public static AccountDto GetUserByUserId(string tenantId, string userId)
        {
            var user =  RunWithRetry(() => Aos.Sdk.AosApi.UserService.GetUser(tenantId, RecordsConstants.RECORDS_APPLICATION_NAME).Where(u => u.Id.Equals(userId)).FirstOrDefault());
            return ConvertToAccountDto(user);
        }

        public static List<AccountDto> GetGroupAndUsers(string customerId)
        {
            return RunWithRetry(() => Aos.Sdk.AosApi.UserService.GetUser(customerId, RecordsConstants.RECORDS_APPLICATION_NAME).ToList().ConvertAll(o => ConvertToAccountDto(o)));
        }

        public static string GetUserName(UserInfo user)
        {
            var result = string.Empty;
            if ((string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.FirstName.Trim())) &&
                (string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.LastName.Trim())))
            {
                if (!string.IsNullOrEmpty(user.Name))
                {
                    var tempNames = user.Name.Split('@');
                    result = tempNames[0];
                }
            }
            else
            {
                result = user.FirstName + " " + user.LastName;
            }
            return result;
        }

        public static bool VerifySignature(string product, string data, string signature)
        {
            try
            {
                return RunWithRetry(() => {
                    var publicKey = Aos.Sdk.AosApi.PublicKeyService.GetPublicKey(product);
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
        public static List<UserInfo> SearchUsers(string customerId, string searchString)
        {
            return RunWithRetry(() => Aos.Sdk.AosApi.UserService.GetUser(customerId, RecordsConstants.RECORDS_APPLICATION_NAME).Where(u => (u.Name.ToLower().Contains(searchString.ToLower()) || u.Email.ToLower().Contains(searchString.ToLower())) && u.Status != 1).ToList());
        }

        public static UserInfo GetUserByKey(string customerId, string searchString)
        {
            return RunWithRetry(() => Aos.Sdk.AosApi.UserService.GetUser(customerId, RecordsConstants.RECORDS_APPLICATION_NAME).Where(u => (u.Name.Equals(searchString) || u.Email.Equals(searchString)) && u.Status != 1).FirstOrDefault());
        }

        public static UserInfo GetUserByKeyIgnoreCase(string customerId, string searchString)
        {
            return RunWithRetry(() => Aos.Sdk.AosApi.UserService.GetUser(customerId, RecordsConstants.RECORDS_APPLICATION_NAME)
                .Where(u => (u.Name.Equals(searchString, StringComparison.OrdinalIgnoreCase) || u.Email.Equals(searchString, StringComparison.OrdinalIgnoreCase)) && u.Status != 1)
                .FirstOrDefault());
        }

        public static List<PoolUserDto> GetAccountPools(string customerId)
        {
            //(s.Role == Office365UserRole.GlobalAdministrator || s.Role == Office365UserRole.SharePointAdministrator)
            return RunWithRetry(() => Aos.Sdk.AosApi.TenantService.GetAccountPools(customerId).Where(s => s.Status == AccountServiceStatus.Active).ToList().ConvertAll(o => ConvertToPoolUserDto(o)));
        }
        public static PoolUserDto GetPoolUserByName(string customerId, string tenantId, string userName)
        {
            //(s.Role == Office365UserRole.GlobalAdministrator || s.Role == Office365UserRole.SharePointAdministrator)
            return ConvertToPoolUserDto(Aos.Sdk.AosApi.TenantService.GetAccountPool(customerId, tenantId, AccountPoolObjectType.Site).Where(s => s.Status == AccountServiceStatus.Active && s.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
        }

        public static List<string> GetAvailableTenants()
        {
            //暂时先用DocAve Online的License做控制，以后AOS添加对RECO的License控制以后，需要改过来
            return RunWithRetry(() => Aos.Sdk.AosApi.CustomerService.GetCustomersByAvaliableProduct(ProductType.AvePointRecords.Name));
        }

        public static AccountDto GetTenantInfo(string groupId)
        {
            UserInfo customer = null;
            RunWithRetry(() => 
            {
                customer = Aos.Sdk.AosApi.CustomerService.GetCustomerOwnerInfo(groupId);
                
            });
            return ConvertToAccountDto(customer);
        }

        public static List<string> GetTenantGroupId(string spTenantId)
        {
            return RunWithRetry(() => Aos.Sdk.AosApi.TenantService.GetTenantGroupId(spTenantId));
        }

        public static X509Certificate2 GetAppCertificate(string appCertSecret, string appCertContent, string appCertSecretContent)
        {
            X509Certificate2 apponlyCertificate;
            if (!appCertSecretContent.IsNullOrEmpty())
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

        private static PoolUserDto ConvertToPoolUserDto(ServiceAccount dto)
        {
            if (dto == null) return null;
            return new PoolUserDto()
            {
                TenantId = dto.TenantId,
                Password = dto.Password,
                UserName = dto.UserName,
                AdminUrl = dto.AdminUrl,
                Status = (int)dto.Status
            };

        }

        private static AccountDto ConvertToAccountDto(UserInfo dto)
        {
            if (dto == null) return null;
            return new AccountDto()
            {
                UserId = dto.InviteType == InviteType.Group ? dto.ObjectId : dto.Id,
                UserPrincipalName = dto.InviteType == InviteType.Group ? dto.Email : dto.Name,
                DisplayName = GetUserName(dto),
                ObjectType = (Contract.RMWeb.RMActiveDirectoryObjectType)dto.InviteType,
                AccountType = (Contract.RMWeb.RMAccountType)dto.UserType,
                Email = dto.Email,
                LastModifiedTime = dto.LastModifiedTime
            };
        }


        private static T RunWithRetry<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch
            {
                Init();
                return action();
            }
        }

        private static void RunWithRetry(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                Init();
                action();
            }
        }

        
    }
}
