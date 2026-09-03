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

using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using Cloud.Sdk.Aos;
using Cloud.Sdk.Core;
using Cloud.Sdk.Data.Aos;
using Cloud.Sdk.Data.Aos.License;
using Cloud.Sdk.Data.Aos.SecurityProfile;
using Cloud.Sdk.Data.Aos.Tenant;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Amls.Ics;
using Cloud.Sdk.Data.Amls.Ics.Contracts;


namespace AvePoint.RA.Common.Aos
{
    /// <summary>
    /// Service 启动时需要先调用RMCloudSdk.Init(certificate)初始化, 之后才可以调用此类方法.
    /// </summary>
    public class RMAosApiClient
    {
        public const string DefaultSecurityProfile = "Default Security Profile";
        public const string IdSystemKeyVault = "id_system_keyvault";

        public static RMAosSecurityProfile GetSecurityProfileById(string customerId, string profileId)
        {
            try
            {

                var profile = Execute(() => RMCloudSdk.AosClient.SecurityProfileService.GetSecurityProfile(profileId, customerId).Result);
                if (profile != null)
                {
                    profile.KeyIdentity = profile.KeyIdentity.TrimStart();
                    return new RMAosSecurityProfile
                    {
                        Id = profile.Id,
                        Name = profile.Name,
                        SecurityProfileType = (int)profile.Type,
                        KeyIdentity = profile.KeyIdentity,
                        ClientId = profile.ClientId,
                        ClientSecret = profile.ClientSecret
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting security profile of {0} from AOS. {1}", profileId, e);
            }
            return null;
        }

        public static CloudAos.RemoteNodesResult GetTenantRemoteNodes(string customerId, string tenantId, bool throwError = false)
        {
            try
            {
                return RMCloudSdk.GetAosModernClient(customerId).RemoteNodeService.GetByTenantIdAsync(tenantId).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting GetTenantRemoteNodes of {0} from AOS. {1}", customerId, e);
                if (throwError)
                {
                    throw;
                }
            }
            return null;
        }

        public static List<ScoreResponse> GetPredictResult(string customerId, Guid trainingModelId)
        {
            try
            {
                ////predict
                PredictRequest predictRequest = new()
                {
                    ScoreRequests = new List<ScoreRequest>
                        {
                            new ScoreRequest() { Name = "Hip Hop", Content = "Hip Hop's Online Shop Celebrity fashion is booming. These webpreneurs are bringing it to main street" }
                        }
                };
                return RMCloudSdk.GetIcsClient(customerId).PredictionService.PredictAsync(trainingModelId, predictRequest).Result;
            }
            catch (Exception e)
            {
               
            }
            return null;
        }

        public static void GetLicenseInfo(string customerId, string product = RecordsConstants.RECORDS_APPLICATION_NAME)
        {

            try
            {
                var license = RMCloudSdk.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync(product).Result;
                if (license != null)
                {
                    //result.Enable = license.Units.First().ExpirationTime > DateTime.UtcNow.Ticks;

                    if (license.Extension != null)
                    {
                        var a = license.Extension;

                    }
                }
                //return result;


            }
            catch (Exception e)
            {
                //logger.Error("Error getting license info for {0} from AOS. {1}", customerId, e);
            }
            //return result;
        }


        public static RMAosSecurityProfile GetCurrentAppliedSecurityProfile(string customerId)
        {
            //获取所有的 keyvault profile
            //如果有 Applying 的， 则取 applying profile
            //如果没有 Applying 的， 则取 applied profile
            var profiles = Execute(() => RMCloudSdk.AosClient.SecurityProfileService.GetAllSecurityProfiles(customerId).Result);
            if (profiles.Any())
            {
                var profile = profiles.FirstOrDefault(p => p.Status == SecurityProfileStatus.Applying);
                if (profile != null)
                {
                    Console.WriteLine("Get current applying profile from AOS,name {0},id {1}", profile.Name, profile.Id);
                    return RMAOSConvertUtil.Convert2SecurityProfile(profile);
                }
                profile = profiles.FirstOrDefault(p => p.Status == SecurityProfileStatus.Applied);
                if (profile != null)
                {
                    Console.WriteLine("Get current applied profile from AOS,name {0},id {1}", profile.Name, profile.Id);
                    return RMAOSConvertUtil.Convert2SecurityProfile(profile);
                }
                var @default = profiles.FirstOrDefault(i => i.Name.StartsWith("Default"));
                if (@default != null)
                {
                    Console.WriteLine($"choose a default profile for {customerId}, profile id is {@default.Id}");
                    return RMAOSConvertUtil.Convert2SecurityProfile(@default);
                }
            }
            Console.WriteLine($"Cannot find any security profile from aos under {customerId}.");
            return null;
        }

        [Obsolete]
        public static RMAosSecurityProfile GetActiveSecurityProfile(string customerId)
        {
            try
            {
                var profile = Execute(() => RMCloudSdk.AosClient.SecurityProfileService.GetActiveSecurityProfile(customerId).Result);
                if (profile != null)
                {
                    profile.KeyIdentity = profile.KeyIdentity.TrimStart();
                    return new RMAosSecurityProfile
                    {
                        Id = profile.Id,
                        Name = profile.Name,
                        SecurityProfileType = (int)profile.Type,
                        KeyIdentity = profile.KeyIdentity,
                        ClientId = profile.ClientId,
                        ClientSecret = profile.ClientSecret
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting active security profile of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }


        public static RMAosSecurityProfile GetDefaultSecurityProfile(string customerId)
        {
            try
            {

                var profile = Execute(() => RMCloudSdk.AosClient.SecurityProfileService.GetDefaultSecurityProfile(customerId).Result);
                if (profile != null)
                {
                    profile.KeyIdentity = profile.KeyIdentity.TrimStart();
                    return new RMAosSecurityProfile
                    {
                        Id = profile.Id,
                        Name = profile.Name,
                        SecurityProfileType = (int)profile.Type,
                        KeyIdentity = profile.KeyIdentity,
                        ClientId = profile.ClientId,
                        ClientSecret = profile.ClientSecret
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting default security profile of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static RMAosAuthenticationProfile GetSPOnlineProfile(string customerId, string o365TenantId)
        {
            try
            {
                var profile = Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.Office365).Result).Where(p => p.TenantId == o365TenantId).FirstOrDefault();
                if (profile == null)
                {
                    profile = Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.SharePoint).Result).Where(p => p.TenantId == o365TenantId).FirstOrDefault();
                    if (profile != null)
                    {
                        return RMAOSConvertUtil.Convert2AuthenticationProfile(profile);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting sp app profile of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<string> GetO365TenantIds(string customerId)
        {
            try
            {
                return Execute(() => RMCloudSdk.GetAosModernClient(customerId).Office365TenantService.GetAllAsync().Result).Select(t => t.TenantId).ToList();
            }
            catch (Exception e)
            {

                Console.WriteLine("Error getting all o365 id of {0} from AOS. {1}", customerId, e);
            }
            return new List<string>();
        }

        public static bool IsCustomerLicenseAvailable(string customerId)
        {
            try
            {
                return Execute(() =>
                    {
                        //bool result = false;
                        var license = RMCloudSdk.GetAosModernClient(customerId).LicenseService.CheckLicenseAsync("AvePointRecords").Result;
                        //if (license != null)
                        //{
                        //    result = license.CustomerId;
                        //}
                        return license != null;
                    }
                );
            }
            catch (Exception e)
            {

                Console.WriteLine("Error getting license available of {0} from AOS. {1}", customerId, e);
            }
            return false;
        }

        public static string GetRecordsServiceUrl(string customerId)
        {

            try
            {
                return Execute(() =>
                {
                    var apps = RMCloudSdk.GetAosModernClient(customerId).ApplicationService.GetAppsAsync().Result;
                    var url = apps.Where(a => string.Equals(a.ApplicationTypeName, "AvePointRecords", StringComparison.OrdinalIgnoreCase)).Select(a => a.Url).FirstOrDefault();
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

                Console.WriteLine("Error getting services url of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<PoolUserDto> GetAccountPoolUsers(string customerId)
        {
            try
            {
                return Execute(() =>
                        {
                            var tenants = RMCloudSdk.GetAosModernClient(customerId).Office365TenantService.GetAllAsync().Result;
                            List<CloudAos.ServiceAccount> account = new List<CloudAos.ServiceAccount>();
                            foreach (var tenant in tenants)
                            {
                                var spPool = RMCloudSdk.GetAosModernClient(customerId).ServiceAccountPoolService.GetAccountsByTenantIdAsync(tenant.TenantId, CloudAos.AccountPoolObjectType.Site).Result;
                                account.AddRange(spPool);
                            }
                            return account.ConvertAll(a => RMAOSConvertUtil.ConvertToPoolUserDto(a));
                        });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting account pool user of {0} from AOS. {1}", customerId, e);
            }
            return new List<PoolUserDto>();
        }

        public static List<CloudAos.ServiceAccount> GetServiceAccounts(string customerId)
        {
            var serviceAccounts = Execute(() => RMCloudSdk.GetAosModernClient(customerId).ServiceAccountService.GetAllAsync(false)).Result;
            return serviceAccounts;
        }
        public static List<CloudAos.ServiceAccount> GetServiceAccountsWithPassword(string customerId)
        {
            var serviceAccounts = Execute(() => RMCloudSdk.GetAosModernClient(customerId).ServiceAccountService.GetAllAsync(true)).Result;
            return serviceAccounts;
        }
       
      
        public static AccountDto GetUserByUserId(string customerId, string userId)
        {
            try
            {
                return Execute(() =>
                {
                    var account = RMCloudSdk.GetAosModernClient(customerId).UserService.GetUsersAsync("AvePointRecords").Result.Where(u => u.Id.Equals(userId)).FirstOrDefault();
                    return RMAOSConvertUtil.Convert2RMAccount(account);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting user by id of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<AccountDto> GetGroupAndUsers(string customerId)
        {
            try
            {
                return Execute(() =>
                {
                    var accounts = RMCloudSdk.GetAosModernClient(customerId).UserService.GetUsersAsync("AvePointRecords").Result;
                    return accounts.ConvertAll(a => RMAOSConvertUtil.Convert2RMAccount(a));
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting group user of {0} from AOS. {1}", customerId, e);
            }
            return new List<AccountDto>();
        }


        public static bool VerifySignature(string product, string data, string signature)
        {
            try
            {
                var publicKey = Execute(() => RMCloudSdk.GetAosModernApplicationClient().PublicKeyService.GetAosPublicKeyAsync().Result);
                var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
                rsaCryptoServiceProvider.FromXmlString(publicKey);
                var signatureData = Convert.FromBase64String(signature);
                var flag = rsaCryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(data), "SHA1", signatureData);
                Console.WriteLine("VerifySignature {0}|{1}|{2}|{3}", product, data, signature, flag);
                return flag;
            }
            catch (Exception e)
            {
                Console.WriteLine("VerifySignature error: {0}", e.ToString());
                return false;
            }
        }


        public static CloudAos.AppProfileInfo GetProfile(string customerId, string o365TenantId)
        {
            try
            {
                var profile = Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.Office365).Result).Where(p => p.TenantId == o365TenantId).FirstOrDefault();
                if (profile == null)
                {
                    profile = Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.SharePoint).Result).Where(p => p.TenantId == o365TenantId).FirstOrDefault();
                }
                return profile;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting sp app profile of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<CloudAos.AppProfileInfo> GetAllProfile(string customerId)
        {
            try
            {
                List<CloudAos.AppProfileInfo> authentications = new List<CloudAos.AppProfileInfo>();
                authentications = Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.Office365).Result);
                if (authentications != null)
                {
                    authentications.AddRange(Execute(() => RMCloudSdk.GetAosModernClient(customerId).AppProfileService.GetByTypeAsync(CloudAos.IdentityProviderType.SharePoint).Result));
                }
                return authentications;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting sp app profile of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }


        private static Cloud.Sdk.Data.Aos.ServiceTokenType GetServiceTokenType(AuthenticationProfile profile)
        {
            ServiceTokenType tokenType = ServiceTokenType.SharePoint;
            switch (profile.IdentityProviderType)
            {
                case IdentityProviderType.SharePointOnline:
                    tokenType = ServiceTokenType.Office365;
                    break;
                case IdentityProviderType.CustomAzureApp:
                    tokenType = ServiceTokenType.CustomizedApp;
                    break;

            }
            return tokenType;
        }


        public static RMAosUserInfo SearchUser(string customerId, string searchKey)
        {
            try
            {
                return Execute(() =>
                {
                    var account = RMCloudSdk.GetAosModernClient(customerId).UserService.GetUsersAsync("AvePointRecords").Result.Where(u => (u.Name.Equals(searchKey) || u.Email.Equals(searchKey)) && (int)u.Status != 1).FirstOrDefault();
                    return RMAOSConvertUtil.Convert2RMAosUserInfo(account);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error search user of {0} from AOS. {1}", customerId, e);
            }
            return null;

        }

        public static PoolUserDto GetPoolUserByName(string customerId, string o365TenantId, string userEmail)
        {
            try
            {
                return Execute(() =>
                {
                    var account = RMCloudSdk.GetAosModernClient(customerId).ServiceAccountPoolService.GetAccountsByTenantIdAsync(o365TenantId, CloudAos.AccountPoolObjectType.Site).Result.Where(s => s.Status == CloudAos.ServiceAccountStatus.Active && s.UserName.Equals(userEmail, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    return RMAOSConvertUtil.ConvertToRMPoolUser(account);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting group user of {0} from AOS. {1}", customerId, e);
            }
            return null;

        }

        public static List<string> GetAvailableTenants()
        {
            try
            {
                return Execute(() =>
                {
                    return RMCloudSdk.GetAosModernApplicationClient().CustomerService.GetCustomersByProductAsync("AvePointRecords").Result.Select(c => c.CustomerId).ToList();
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting available tenant from AOS. {1}", e);
            }
            return new List<string>();

        }

        public static AccountDto GetTenantInfo(string customerId)
        {
            try
            {
                return Execute(() =>
                {
                    var account = RMCloudSdk.GetAosModernClient(customerId).CustomerService.GetTenantOwnerAsync().Result;
                    return RMAOSConvertUtil.Convert2RMAccount(account, true);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting tenant of {0} from AOS. {1}", customerId, e);
            }
            return null;
        }

        public static List<string> GetTenantGroupId(string o365TenantId)
        {
            try
            {
                return Execute(() =>
                {
                    return RMCloudSdk.GetAosModernApplicationClient().CustomerService.GetCustomerIdsAsync(o365TenantId).Result;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting tenant group id of {0} from AOS. {1}", o365TenantId, e);
            }
            return null;
        }


        public static RMAosAccountInfo LogonAos(string user, string password, string product)
        {
            try
            {
                return Execute(() =>
                {
                    var acc = RMCloudSdk.GetAosModernApplicationClient().AccountService.GetByLocalUserAsync(
                        new CloudAos.LogonInfo()
                        {
                            Username = user,
                            Password = password,
                            Product = product
                        }).Result;
                    return RMAOSConvertUtil.Convert2RMAosAccountInfo(acc);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error login aos of {0} from AOS. {1}", user, e);
            }
            return null;
        }

        public static RMAosAccountModelResult ValidateUser(string name, string tenantId)
        {
            try
            {
                return Execute(() =>
                {
                    var acc = RMCloudSdk.GetAosModernApplicationClient().AccountService.GetByOffice365UserAsync("", name, "AvePointRecords", tenantId).Result;
                    return RMAOSConvertUtil.Convert2RMAccountModelResult(acc);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error validate user of {0} from AOS. {1}", name, e);
            }
            return null;

        }



        private static T Execute<T>(Func<T> func)
        {
            RetryPolicy retry = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60) });
            TimeoutPolicy timeout = Policy.Timeout(TimeSpan.FromMinutes(4), TimeoutStrategy.Pessimistic);
            var wrap = retry.Wrap(timeout);
            try
            {
                return wrap.Execute(func);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }

    }


}

public class RecordsConstants
{
    public static readonly string Records_Processor_Name = "RecordsProcessor.exe";
    public static readonly int Reocrds_Processor_Port = 18006;

    public const int RecordHold_Default = 0;
    public const int RecordHold_Electronic = 1; //默认表示SP和EXO的老数据
    public const int RecordHold_PhyProfile = 2;
    public const int RecordHold_Personal = 3;

    public const int Explorer_RealTime_Success = 0;
    public const int Explorer_RealTime_Failed_Partial = 1;
    public const int Explorer_RealTime_Failed_All = 2;
    public const int Explorer_RealTime_Running = 3;
    public const int Explorer_RealTime_Finished = 4;

    public const int SubJob_Runnable_Waiting = 0;
    public const int SubJob_Runnable_CanRun = 1;
    public const int SubJob_Runnable_Runing = 2;

    public const int TenantDBSize = 50;
    public const int ExplorerDBSize = 25;
    public const string ExplorerDBDefaultName = "RECO";

    public const string UniqueId_NoNeedRunJob = "3";

    public const string EXOLocationFormat = "{0}{1}_{2}";


    public const string RECORDS_APPLICATION_NAME = "AvePointRecords";
    public const string RECORDS_HYBRID_NAME = "HybridAgent";
    public const string RequestIdPrefix = "RC-";
    public const string Office365LogonUrl = "";
    public const string GraphResource = "";
    public const string RedirectOffice365LogOnUrl = "";

    public static Guid FS_ROOT_GUID = new Guid("71A6C027-0773-4C6C-B0E5-8FA9F789B668");

    public static int ExplorerQueryPageSize = 20000;

    public const string TYPE_STRING_ROOT = "Root";
    public const string TYPE_STRING_TERM_GROUP = "TermGroup";
    public const string TYPE_STRING_TERM_SET = "TermSet";
    public const string TYPE_STRING_TERM = "Term";
    public const string TYPE_STRING_SUB_TERM = "SubTerms";
    public const string TYPE_STRING_BOXES = "Boxes";
    public const string TYPE_STRING_FILES = "Files";
    public const string TYPE_STRING_PhyBox = "PhyBox";
    public const string TYPE_STRING_PhyFile = "PhyFile";

    public const string SecurityTermCacheKeyPrefix = "SecurityTerms_";
    public const string PhysicalSubPermissionCacheKeyPrefix = "PhysicalSubPermission_";
}