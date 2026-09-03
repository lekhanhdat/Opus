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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Service.Services.AccountManager.AuditHandler;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Contract.Logon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Common.ClientRequest;


namespace AvePoint.RA.Service.Security
{
    [Audit]
    public class LoginService : ILoginService
    {
        private IRALogger Logger = RALogger.GetInstance(typeof(LoginService));

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMSecurityTrimmingHelper _TrimmingHelper;
        private IRMSecurityTrimmingHelper TrimmingHelper => PlatformWindsorManager.GetService(ref _TrimmingHelper);
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IGeneralSettingDao GeneralSettingDao = PlatformWindsorManager.GetService<IGeneralSettingDao>();      
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IMultiGeoSettingService MultiGEOSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        public async Task InitSecurityProfileAsync()
        {
            var usingSecurityProfile = GlobalStorageSettingDao.FindAll().FirstOrDefault();
            if (usingSecurityProfile == null)
            {
                RMCPGlobalStorageSetting dataEncryptionDto = new RMCPGlobalStorageSetting();
                dataEncryptionDto.StoragePolicyId = Guid.Empty;
                dataEncryptionDto.ExportLocationId = Guid.Empty;
                dataEncryptionDto.SecurityProfileId = Guid.Empty;
                dataEncryptionDto.SecurityProfileName = string.Empty;
                dataEncryptionDto.UseCompression = true;
                dataEncryptionDto.UseEncryption = false;
                dataEncryptionDto.CompressionSpeed = 5;
                dataEncryptionDto.CompressionMethod = GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                dataEncryptionDto.EncryptionMethod = GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                dataEncryptionDto.Extentions = string.Empty;
                await GlobalStorageSettingDao.SaveOrUpdateAsync(dataEncryptionDto);
            }
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.Login, Category = AuditCategory.LogOut, Action = AuditAction.LogOut, AfterHandler = typeof(LogOutAfterAuditHandler))]
        public async Task LogOutAsync(RMIdentity identity)
        {
            if (identity != null) 
            {
                await SessionManger.DeleteAsync(identity.SessionId);
            }
        }

        public async System.Threading.Tasks.Task CreateSessionAsync(RMIdentity identity)
        {
            int sessionTimeout = 15;
            try
            {
                if(identity.SessionId == Guid.Empty)
                {
                    identity.SessionId = Guid.NewGuid();
                    identity.SessionFrom = DateTime.UtcNow.Ticks;
                    
                }
                SessionManger.CurrentSessionId = identity.SessionId;

                var settings = await GeneralSettingDao.GetGeneralSettingByUserAsync(identity.TenantGroupId);
                if (settings != null)
                {
                    sessionTimeout = GeneralSettingService.CaculateTimeByUnit(settings.SessionTime, settings.SessionTimeUnit);
                    identity.SessionOut = sessionTimeout;
                }
                else 
                {
                    identity.SessionOut = sessionTimeout;
                }
                identity.ExpiredTime = DateTime.UtcNow.AddMinutes(sessionTimeout);
                await SessionManger.SetAsync(identity);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured when getting claims principal, reaons:{0}", ex.ToString());
                throw;
            }
        }

        public async Task<ClaimsPrincipal> ConvertClaimsPrincipalAsync(RMIdentity identity)
        {
            int sessionTimeout = 20;
            ClaimsPrincipal cp = null;
            try
            {
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(identity.AuthenticationType.ToString());
                //bool isADFS = identity.AuthenticationType == AuthenticationTypes.Federation.ToString();
                var sessionId = identity.SessionId == Guid.Empty ? Guid.NewGuid() : identity.SessionId;
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.AuthByRM, "true"));
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, identity.Name));
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identity.Name));

                claimsIdentity.AddClaim(new Claim(RMClaimTypes.AccountId, identity.AccountId == null ? "" : identity.AccountId.ToString()));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.RegisterEmail, identity.Name == null ? "" : identity.Name));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.TenantGroupId, identity.TenantGroupId == null ? "" : identity.TenantGroupId));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.AccountType, identity.AccountType.ToString()));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.DisplayName, identity.DisplayName == null ? "" : identity.DisplayName));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.RecordsUrl, identity.Url == null ? "" : identity.Url));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.AuthType, identity.AuthenticationType.ToString()));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.SessionType, sessionId.ToString()));

                claimsIdentity.AddClaim(new Claim(RMClaimTypes.Permission, identity.GPermission == null ? "0" : identity.GPermission));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.ForceLogined, identity.ForceLogined.ToString()));

                claimsIdentity.AddClaim(new Claim(RMClaimTypes.Company, identity.Company == null ? "" : identity.Company));
                claimsIdentity.AddClaim(new Claim(RMClaimTypes.AccountNumber, identity.AccountNumber == null ? "" : identity.AccountNumber));

                var settings = await GeneralSettingDao.GetGeneralSettingByUserAsync(identity.TenantGroupId);
                if (settings != null)
                {
                    sessionTimeout = GeneralSettingService.CaculateTimeByUnit(settings.SessionTime, settings.SessionTimeUnit);
                    claimsIdentity.AddClaim(new Claim(RMClaimTypes.GeneralSettingID, settings.Id.ToString()));
                }

                if (identity.Claims != null && identity.Claims.Count > 0)
                {
                    claimsIdentity.AddClaims(identity.Claims.Select(c => new Claim(c.Key, c.Value)));
                }
                claimsIdentity.Label = identity.RegisterEmail;

                cp = new ClaimsPrincipal(claimsIdentity);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured when getting claims principal, reaons:{0}", ex.ToString());
                throw;
            }

            return cp;
        }

        public async System.Threading.Tasks.Task UpdateSessionTimeoutSettingAsync(int sessionTimeout)
        {
            try
            {
                var identity = await SessionManger.GetAsync(SessionManger.CurrentSessionId);
                await SessionManger.UpdateTimeoutSettingAsync(identity, sessionTimeout);
            }
            catch(Exception ex)
            {
                Logger.Error($"Update session timeout setting failed. {ex}");
            }
        }


        public async Task<(RAReturnMessage, RMIdentity)> SSOLoginAsync(RMLogonInfo logonInfo)
        {
            using PerformanceScope scope = new PerformanceScope($"sso check Login.");
            var (message, user) = LoginPreCheck(logonInfo.token);
            if (user == null || message.MessageType == RAMessageType.Failed)
            {
                return (message, new RMIdentity() { Name = user?.Name, DataCenter = user?.DataCenter });
            }
            RMIdentity identity = await InitTenant(user);
            TenantLocalValue.Init(identity);

            return (message, identity);
        }

        [Audit(Module = AuditModule.Login, Category = AuditCategory.LogIn, Action = AuditAction.SSOLogIn, AfterHandler = typeof(LogInAfterAuditHandler), StartNewThread = false)]
        public async Task<(RAReturnMessage, RMIdentity)> SSOLoginAsync(RMLogonInfo logonInfo, RMIdentity identity)
        {
            var message = await LicenseHelperService.ValidateLicense();
            if (message.MessageType == RAMessageType.Failed)
            {
                return (message, identity);
            }
            var userPermission = await TrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>();
            var userSOPermission = await TrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>();
            var userExtensionPermission = await TrimmingHelper.GetUserPermissionAsync<RMPermissionExtensionMasks>();
            var userDiscoveryPermission = await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryPermissionMasks>();
            var userSalesforceDiscoveryPermission = await TrimmingHelper.GetUserPermissionAsync<RMDiscoverySalesforcePermissionMask>();
            var userGoogleROTDiscoveryPermission = await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryGoogleROTPermissionMask>();
            var userFSDiscoveryPermission = await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryFileSystemPermissionMask>();
            if (userPermission == RMPermissionMasks.None 
                && userSOPermission == RMSOPermissionMasks.None 
                && userDiscoveryPermission == RMDiscoveryPermissionMasks.None 
                && userExtensionPermission == RMPermissionExtensionMasks.None 
                && userSalesforceDiscoveryPermission== RMDiscoverySalesforcePermissionMask.None 
                && userGoogleROTDiscoveryPermission == RMDiscoveryGoogleROTPermissionMask.None
                && userFSDiscoveryPermission == RMDiscoveryFileSystemPermissionMask.None)
            {
                Logger.Error("login failed user have no access.");
                message.ErrorMessage = $"the user have no access.";
                message.MessageType = RAMessageType.Failed;
                message.FaildType = RAFailedType.AccessDenied;
                return (message, identity);
            }
            identity.GPermission = ((long)userPermission).ToString();
            #region Add sub permismsion to cache
            //refresh sub permision cache
            await TrimmingHelper.GetUserPermissionAsync<RMSubPermissionMasks>();
            #endregion
            await TrimmingHelper.GetUserPermissionAsync<RMPermissionExtensionMasks>();

            await TrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>();

            await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryPermissionMasks>();
            
            await TrimmingHelper.GetUserPermissionAsync<RMDiscoverySalesforcePermissionMask>();
            await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryGoogleROTPermissionMask>();
            await TrimmingHelper.GetUserPermissionAsync<RMDiscoveryFileSystemPermissionMask>();

            await InitSecurityProfileAsync();
            //AddPermissionForArchiver(user.CustomerId, user.Id);
            message.MessageType = RAMessageType.Successful;
            if (identity != null && identity.IsAuthenticated)
            {
                using (new PerformanceScope($"sso set user principal."))
                {
                    identity.AccessToken = logonInfo.access_token;
                    await CreateSessionAsync(identity);
                    message.MessageType = RAMessageType.Successful;
                }
            }

            return (message, identity);
        }
        private async Task<RMIdentity> InitTenant(AosSsoIdentityInfo user)
        {
            RMIdentity identity = null;
            AosAuthentication aosAuthentication = new AosAuthentication();
            var credential = Convert2AOSCredential(user);
            TenantService.ChangeAccountStatus(credential.TenantGroupId, TenantStatus.Normal);
            identity = aosAuthentication.AuthenticateCredential(credential);
            identity.Url = "";//new dto not have url
            TenantLocalValue.LogonUserEmail = identity.Name;
            TenantLocalValue.LogonGroupId = identity.TenantGroupId;
            TenantLocalValue.LogonUserId = identity.AccountId;
            TenantLocalValue.PartnerUser = identity.PartnerUser;
            TenantLocalValue.UserGroups = Convert2AzureADGroups(user.UserGroups);
            TenantLocalValue.RecordsUrl = "";//new dto not have url
            TenantLocalValue.Company = "";
            TenantLocalValue.AccountNumber = user.AccountNumber;
           
            var isNewTenant = await TenantService.InitTenantAsync();
            if (!isNewTenant)
            {
                await UserService.SyncLogonUserGroupAsync(user.Id);
            }
            var userFromDB = await UserService.GetUserByNameAsync(user.Name);
            TenantLocalValue.DisplayName = userFromDB?.DisplayName;
            identity.DisplayName = userFromDB?.DisplayName;
            TrimmingHelper.DisableCache();
            await LicenseHelperService.UpdateLicense(true);
            identity.IsEnableMultiGeo = await FunctionSettingDao.IsEnableMultiGeoFeature(KeyValueDao);
            var isAdmin = await IsOpusAdmin();
            identity.AccountType = isAdmin ? RMAccountType.ApplicationAdmin : RMAccountType.StandardUser;
            identity.ForceLogined = user.EnabledForceLogined;
            identity.DisableAVA = user.DisableAVA;
            identity.ExistAVAUser = user.ExistAVAUser;
            TenantLocalValue.AccountType = identity.AccountType;
            return identity;

        }
        private (RAReturnMessage, AosSsoIdentityInfo) LoginPreCheck(string token, bool checkDataCenter = true) 
        {
            RAReturnMessage message = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful,
            };
            using PerformanceScope scope = new PerformanceScope($"sso check Login.");

            //验证SSO Token
            var (tokenValidate, aosTokenInfo) = ValidateToken(message, token);
            var user = aosTokenInfo?.IdentityInfo;
            if (aosTokenInfo == null)
            {
                message.ErrorMessage = RMSSOLoginFailedType.FailedLogin.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }
            if (!tokenValidate)
            {
                message.ErrorMessage = RMSSOLoginFailedType.UserNotExists.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }

            if(user.AccountType == RMAccountType.None)
            {
                message.FaildType = RAFailedType.AccessDenied;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }

            if (checkDataCenter && !CheckUserDataCenter(user))
            {
                //User所属DC和当前环境DC不匹配
                message.ErrorMessage = RMSSOLoginFailedType.FailedLogin.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }
            if (checkDataCenter && !CheckUserMultiGeoDataCenter(user))
            {
                message.ErrorMessage = RMSSOLoginFailedType.IPMultiGeoForbidden.ToString();
                message.FaildType = RAFailedType.BlockedByIpRestriction;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }
            message.Extension = user?.Name;

            if (!ValidateLoginResult(aosTokenInfo.Result, out RMSSOLoginFailedType failedResultType))
            {
                //SSO 登录失败
                Logger.Error($"[SsoLogin] login failed, result: [{aosTokenInfo.Result}]");
                message.ErrorMessage = failedResultType.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return (message, user);
            }
            var checkLicenseMessage = CheckRecordsLicense(aosTokenInfo);
            if (checkLicenseMessage.MessageType == RAMessageType.Failed)
            {
                //Check License Failed
                return (checkLicenseMessage, user);
            }
            
            TenantInfoDto tenant = TenantService.GetTenantInfo(user?.CustomerId);
            if (tenant?.Status == TenantStatus.SoftDeleted)
            {
                message.FaildType = RAFailedType.SoftDeleted;
                message.MessageType = RAMessageType.Failed;
                message.ErrorMessage = $"the tenant is soft deleted, {tenant?.TenantId}";
                return (message, null);
            }
            Logger.Info($"[SsoLogin] begin to validate license info.");
            return (message, user);
        }
        private (bool, AosSsoTokenInfo) ValidateToken(RAReturnMessage message, string token) 
        {
            //验证SSO Token
            if (!RMSSOHelper.ValidateSsoToken(token, RMAosApiClient.GetPortalPublicKey()))
            {
                Logger.Info($"[SsoLogin] validate token failed.");
                return (false, null);
            }
            //解析 SSO Token
            var aosTokenInfo = RMSSOHelper.AnalysisToken(token);
            if (aosTokenInfo == null)
            {
                Logger.Error($"[SsoLogin] failed because token is null");
                message.ErrorMessage = RMSSOLoginFailedType.FailedLogin.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return (false, aosTokenInfo);
            }
            return (true, aosTokenInfo);
        }
        public async Task<(RAReturnMessage, RMLoginInfo)> MobileSSOLogin(string state, string token, string accessToken)
        {

            var (message, user) = LoginPreCheck(token, false);
            if (user == null)
            {
                return (message, null);
            }
            if (!IsAdmin(user)) 
            {
                Logger.Warn($"Not admin: {user?.CustomerId}, {user?.Email}");
                message.ErrorMessage = RAFailedType.AccessDenied.ToString();
                message.FaildType = RAFailedType.AccessDenied;
                return (message, null);
            }
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(user.CustomerId);
            if (!licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusIL)) 
            {
                Logger.Error("mobile login failed license is invalid.");
                message.ErrorMessage = RAFailedType.LicenseExpired.ToString();
                message.FaildType = RAFailedType.LicenseExpired;
                return (message, null);
            }
            //SSO 登录成功
            Logger.Info($"mobile [SsoLogin] login result is successful.");
            var customerId = user.CustomerId;
            var apiUrl = GetApiUrl(customerId);
            var loginInfo = new RMLoginInfo()
            {
                TenantGroupId = customerId,
                Email = user.Email,
                Type = "App",
                AccessToken = accessToken,
                AppUrl = apiUrl
            };
            //if (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && TenantService.IsNewOpusTenant())
            //{
            //    if (licenseInfo.StorageLicenseInfo != null && !licenseInfo.StorageLicenseInfo.Byos)
            //    {
            //        //Check and create AvePoint Storage
            //        Logger.Info("AvePoint Storage license.");
            //        await StorageDeviceService.CreateDefaultStorageDeviceAsync();
            //    }
            //}
            message.MessageType = RAMessageType.Successful;
            Logger.Info($"tenant: {customerId} mobile login success.");
            return (message, loginInfo);
        }
        private string GetApiUrl(string customerId) 
        {
            var serviceUrl = RMAosApiClient.GetRecordsServiceUrl(customerId);
            var productUrl = new Uri(serviceUrl);
            return productUrl.Scheme + "://" + productUrl.Authority;
        }
        private bool IsAdmin(AosSsoIdentityInfo user)
        {
            return  user.Roles.Any(r => r.RoleName.Equals("tenantadmin") || r.RoleName.Equals("appadminrevim"));
        }
        private bool ValidateLoginResult(SSOLoginResultMessage result, out RMSSOLoginFailedType failedResultType)
        {
            var isSuccessful = false;
            failedResultType = RMSSOLoginFailedType.None;
            switch (result)
            {
                case SSOLoginResultMessage.Failed:
                case SSOLoginResultMessage.ThirdApplicationAuthenticationFailed:
                    failedResultType = RMSSOLoginFailedType.FailedLogin;
                    break;
                case SSOLoginResultMessage.UserUnactivated:
                case SSOLoginResultMessage.UserDeactived:
                    failedResultType = RMSSOLoginFailedType.UserNotActivated;
                    break;
                case SSOLoginResultMessage.NeedInviteTenantUser:
                case SSOLoginResultMessage.UserNotExists:
                case SSOLoginResultMessage.SignUp:
                case SSOLoginResultMessage.UserInMultipleTenant:
                case SSOLoginResultMessage.JustAllowLocalMethod:
                    failedResultType = RMSSOLoginFailedType.UserNotExists;
                    break;
                //case ResultInfo.IdentityServerLogin: AOS logic
                //case ResultInfo.NoSalesForceApp: records not support salesforce
                case SSOLoginResultMessage.ForceLogin:
                    failedResultType = RMSSOLoginFailedType.FailedLogin;
                    break;
                case SSOLoginResultMessage.IPForbidden:
                    failedResultType = RMSSOLoginFailedType.IPForbidden;
                    break;
                case SSOLoginResultMessage.Successful:
                    isSuccessful = true;
                    break;
            }
            return isSuccessful;
        }

        private RAReturnMessage CheckRecordsLicense(AosSsoTokenInfo aosTokenInfo)
        {
            RAReturnMessage message = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful
            };
            var recordsLicenseInfo = aosTokenInfo.Licenses?.Find(o => o.Product.Equals(RecordsConstants.RECORDS_APPLICATION_NAME));
            if (recordsLicenseInfo == null || recordsLicenseInfo.ExpirationTime < DateTime.UtcNow.Ticks)
            {
                //No License or license expired
                Logger.Error($"[SsoLogin] failed because no reco license or license expired.");
                message.ErrorMessage = RMSSOLoginFailedType.NoAvailableLicense.ToString();
                message.FaildType = RAFailedType.SSOLoginFailed;
                message.MessageType = RAMessageType.Failed;
                return message;
            }

            if (!recordsLicenseInfo.AcceptedLicenseAgreement)
            {
                if (aosTokenInfo.IdentityInfo.AccountType != RMAccountType.ApplicationAdmin && (DateTime.UtcNow.Ticks - recordsLicenseInfo.LicenseAgreementUpdateTime) / (double)TimeSpan.TicksPerDay < 30)
                {
                    return message;
                }
                // Not Accept LicenseAgreement 
                Logger.Warn($"[SsoLogin] current user not accept la");
                var customerId = aosTokenInfo.IdentityInfo.CustomerId;
                message.Extsion1 = new SsoSamplerUserInfo { CustomerId = customerId, UserName = aosTokenInfo.IdentityInfo.Name, UserId = aosTokenInfo.IdentityInfo.Id, AccountType = aosTokenInfo.IdentityInfo.AccountType, LAContent = RMAosApiClient.GetLicenseAgreement(customerId) };
                message.FaildType = RAFailedType.NotAcceptLicenseAgreement;
                message.MessageType = RAMessageType.Failed;
                return message;
            }
            return message;
        }

        public string GetSsoLoginFailedMessage(string failedType)
        {
            var result = I18NEntity.GetString("RM_SSO_Message_FailedLogin");
            try
            {
                Enum.TryParse(failedType, out RMSSOLoginFailedType failedResultType);
                switch (failedResultType)
                {
                    case RMSSOLoginFailedType.UserNotActivated:
                        result = I18NEntity.GetString("RM_SSO_Message_UserNotActivated");
                        break;
                    case RMSSOLoginFailedType.UserNotExists:
                        result = I18NEntity.GetString("RM_SSO_Message_UserNotExists");
                        break;
                    case RMSSOLoginFailedType.IPForbidden:
                        result = I18NEntity.GetString("RM_SSO_Message_IPForbidden");
                        break;
                    case RMSSOLoginFailedType.NoAvailableLicense:
                        result = I18NEntity.GetString("RM_SSO_Message_NoAvailableLicense");
                        break;
                    case RMSSOLoginFailedType.None:
                    case RMSSOLoginFailedType.FailedLogin:
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"An error while GetSsoLoginFailedMessage, failedType:{failedType} ,message:{e}");
            }
            
            return result;
        }

        private List<AzureADGroupInfo> Convert2AzureADGroups(List<AosSsoUserGroups> ssoUserGroups)
        {
            if (ssoUserGroups != null)
            {
                return ssoUserGroups.ConvertAll(o => new AzureADGroupInfo
                {
                    ObjectId = o.ObjectId,
                    DisplayName = o.GroupName
                });
            }
            return null;
        }

        private AOSCredential Convert2AOSCredential(AosSsoIdentityInfo user)
        {
            if (user != null)
            {
                return new AOSCredential()
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    TenantGroupId = user.CustomerId,
                    AccountType = user.AccountType,
                    AccountNumber = user.AccountNumber,
                    Company = user.Organization,
                    PatnerUser = user.PartnerUser,
                    PatnerOwner = user.PartnerOwner,
                };
            }
            return null;
        }

        public bool AcceptLicenseAgreement(SsoSamplerUserInfo userInfo, string ipAddress)
        {
            var dto = new Cloud.Sdk.Data.Aos.AcceptedLicenseAgreementModel
            {
                CustomerId = userInfo.CustomerId,
                AcceptedBy = userInfo.UserName,
                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                AcceptedByIPAddress = ipAddress
            };
            return  RMAosApiClient.AcceptLicenseAgreement(dto);
        }

        private bool CheckUserDataCenter(AosSsoIdentityInfo identityInfo)
        {
            var currentEnvDC = RMSSOHelper.CurrentDCName;
            var userDC = identityInfo?.DataCenter;
            Logger.Info($"user data center: {userDC}, current data center: {currentEnvDC}");
            if (string.IsNullOrEmpty(userDC) || string.IsNullOrEmpty(currentEnvDC)) 
            {
                Logger.Debug($"user data center or current data center is null.");
                return false;
            }
            if (userDC != currentEnvDC)
            {
                if (string.IsNullOrEmpty(identityInfo?.CustomerId))
                {
                    Logger.Debug($"CustomerId is null or empty, can't validate data center,don't allow login.");
                    return false;
                }
                TenantLocalValue.LogonGroupId = identityInfo.CustomerId;
                var existTenant = TenantService.CheckTenantExist(identityInfo?.CustomerId);
                if (!existTenant)
                {
                    Logger.Debug($"Wrong data center login. Don't init the tenant.{identityInfo?.Name}, {identityInfo?.CustomerId}");
                    return false;
                }
            }
            return true;
        }

        private bool CheckUserMultiGeoDataCenter(AosSsoIdentityInfo identityInfo)
        {
            var currentEnvDC = RMSSOHelper.CurrentDCName;
            var userDC = identityInfo?.DataCenter;
            if (userDC != currentEnvDC)
            {
                var isEnableJPMCMultiGeo = FunctionSettingDao.IsEnableMultiGeoFeature(KeyValueDao).GetAwaiter().GetResult();
                if (isEnableJPMCMultiGeo)
                {
                    Logger.Info($"JPMC multi-geo feature is enabled, validate user login with other data center. {identityInfo?.Name}, {identityInfo?.CustomerId}");
                    if (!MultiGEOSettingService.ValidateLoginIPAsync(ClientRequestLocalValue.ClientIP, currentEnvDC).GetAwaiter().GetResult())
                    {
                        Logger.Warn($"User login from invalid IP address. {identityInfo?.Name}, {identityInfo?.CustomerId}");
                        return false;
                    }
                    Logger.Info($"JPMC multi-geo feature is enabled, allow user login with other data center. {identityInfo?.Name}, {identityInfo?.CustomerId}");
                    return true;
                }
                Logger.Debug($"Wrong data center login. {identityInfo?.CustomerId}");
                return false;
            }
            return true;
        }

        private async Task<bool> IsOpusAdmin()
        {
            return await TrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin) || await TrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ControlPanelAdmin);
        }
    }

}
