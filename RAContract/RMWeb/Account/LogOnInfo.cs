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
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Account
{
    public class LogOnInfo
    {
        public string product { get; set; }
        public string signature { get; set; }
        public string user { get; set; }
    }

    public class AosUserInfo
    {
        public const string ApplicationName = "AvePointRecords";
        public string UserId { get; set; }
        public string Username { get; set; }
        public string CustomerId { get; set; }
        public string Url { get; set; }

        public string Company { get; set; }

        public string AccountNumber { get; set; }

        private RMAccountType mType = RMAccountType.None;
        public DateTime ExpireTime { get; set; }
        /// <summary>
        /// 默认为1分钟超时, 由于RECO-8498问题 临时调整为2分钟.
        /// </summary>
        public bool IsExpire
        {
            get
            {
                if (ExpireTime != DateTime.MinValue)
                {
                    return ExpireTime.AddMinutes(1).Ticks <= DateTime.UtcNow.Ticks;
                }
                return true;
            }
        }
        public RMAccountType AccountType
        {
            get
            {
                if (mType == RMAccountType.None)
                {
                    var role = this.Roles?.Where(r => r.ApplicationName.Equals(ApplicationName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (role != null)
                    {
                        mType = role.UserType == RMAccountType.RegisteredUser ? RMAccountType.ApplicationAdmin : role.UserType;
                        Url = role.Url;
                    }
                }
                return mType;
            }
        }
        public RMActiveDirectoryObjectType InviteType { get; set; }
        public List<AosRole> Roles { get; set; }
        public List<AvePoint.RA.Contract.Tenant.AzureADGroupInfo> UserGroups { get; set; }
        /// <summary>
        ///  Allow concurrent sign-ins from multiple locations for the same account 没有勾选，ForceLogined为True，否则为False
        /// </summary>
        public bool ForceLogined { get; set; }
    }
    public class AosRole
    {
        public string ApplicationName { get; set; }
        public string Url { get; set; }
        public bool IsAcceptedLicenseAgreement { get; set; }
        public RMAccountType UserType { get; set; }
    }

    public enum CheckSessionResult
    { 
        Success = 1,
        SessionTimeout = 2,
        ForcedLogout = 3
    }

    public class AosSsoIdentityInfo
    {
        public const string ApplicationName = "AvePointRecords";
        public const string AllProducts = "All Product";
        public String Id { get; set; }

        public String Name { get; set; }

        public String ObjectId { get; set; }

        public String Email { get; set; }

        public String FirstName { get; set; }

        public String LastName { get; set; }

        public String DisplayName { get; set; }

        public String CustomerId { get; set; }

        public String Region { get; set; }

        public String Organization { get; set; }

        public String DataCenter { get; set; }
        public string AccountNumber { get; set; }

        public PortalType PortalType { get; set; }

        public AosSsoIdentityProviderType IdentityType { get; set; }

        public AosSsoIdentityProviderType LoginType { get; set; }

        public AosSsoInviteType InviteType { get; set; }

        private RMAccountType mType = RMAccountType.None;
        public RMAccountType AccountType
        {
            get
            {
                if (mType == RMAccountType.None)
                {
                    var role = this.Roles?.Where(r => r.ApplicationName.Equals(ApplicationName, StringComparison.OrdinalIgnoreCase) || r.ApplicationName.Equals(AllProducts, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (role != null)
                    {
                        mType = role.Type == RMAccountType.RegisteredUser ? RMAccountType.ApplicationAdmin : role.Type;
                    }
                }
                return mType;
            }
        }

        public String DomainName { get; set; }

        public String TenantId { get; set; }

        public List<AosSsoRole> Roles { get; set; } = new List<AosSsoRole>();

        public string Permissions { get; set; }

        public int SessionOutDuration { get; set; }

        public List<string> GeoLocations { get; set; }

        public bool IsMultiGeo { get; set; }

        public bool EnabledForceLogined { get; set; }

        public bool EnableIPAddress { get; set; }

        //public DateTime ExpireTime { get; set; } = DateTime.UtcNow.AddMinutes(1);

        public List<AosSsoUserGroups> UserGroups { get; set; } = new List<AosSsoUserGroups>();

        public List<String> FavouriteProducts { get; set; }

        public Boolean IsTenantOwner { get; set; }

        public string PartnerUser { get; set; }

        public string PartnerOwner { get; set; }

        public bool DisableAVA { get; set; }

        public bool ExistAVAUser { get; set; }
    }

    public enum AosSsoInviteType
    {
        User = 0,
        Group = 1,
        UserInGroup = 2,
        PortalSupport = 3,
        ProductSupport = 4
    }

    public class AosSsoUserGroups
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string ObjectId { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public string CustomerId { get; set; }
        public string UserExtensionContent { get; set; }
    }

    public enum AccountStatus
    {
        Unactivated = 0,
        Deactive = 1,
        Active = 2
    }

    public enum AosSsoIdentityProviderType
    {
        Local = 0,
        Office365 = 6,
        SalesForce = 7,
        Yammer = 8,
        AzureAD = 9,
        Sandbox = 10,
        SharePoint = 11,
        Exchange = 12,
        WindowsAzure = 13,
        CustomAzureApp = 15,
        DynamicsAX = 16,
        DynamicsCustomerEngagement = 17,
        APElementsAutomation = 18,
        Google = 19,
        FLYOnline = 20,
        SaaSManagementPlatform = 21,
        VirtualDataRoom = 22,
        AzureBackup = 23,
        MicrosoftDelegate = 30,
    }

    public enum PortalType
    {
        AOS = 0,
        APE = 1
    }

    public class AosSsoRole
    {
        public string ApplicationName { get; set; }
        public string RoleName { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public RMAccountType Type { get; set; }
    }

    public class AosSsoLicenseInfo
    {
        public string Product { get; set; }
        public long ExpirationTime { get; set; }
        public bool AcceptedLicenseAgreement { get; set; }
        public long LicenseAgreementUpdateTime { get; set; }
    }

    public enum SSOLoginResultMessage
    {
        Successful,
        Failed,
        ThirdApplicationAuthenticationFailed,
        UserUnactivated,
        UserDeactived,
        NeedInviteTenantUser,
        UserNotExists,
        SignUp,
        UserInMultipleTenant,
        JustAllowLocalMethod,
        ForceLogin,
        IPForbidden
    }


    public enum RMSSOLoginFailedType {
        None,
        FailedLogin,
        UserNotActivated,
        UserNotExists,
        IPForbidden,
        NoAvailableLicense ,
        IPMultiGeoForbidden,
    }

    public class AosSsoTokenInfo {
        public AosSsoIdentityInfo IdentityInfo { get; set; }
        public List<AosSsoLicenseInfo> Licenses { get; set; }
        public SSOLoginResultMessage Result { get; set; }
    }

    public class SsoSamplerUserInfo {
        public string CustomerId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string LAContent { get; set; }
        public string FailedMessage { get; set; }

        public RMAccountType AccountType {  get; set; }
    }

    public class LoginInfo
    {
        public LoginUserInfo UserInfo { get; set; }
        public string DataCenter { get; set; }
        public string ProductVersion { get; set; }
        public string Copyright { get; set; }
        public string ForwardToDAORC { get; set; }
        public string CurrentLanguage { get; set; }
        public string TimeSettingModel { get; set; }
        public string Permission { get; set; }
        public string UserResources { get; set; }
        public string AvaliableSource { get; set; }
        public bool HasIntelligentPermission { get; set; }
        public string EnviromentName { get; set; }
        public FileExtentionsConfig FileExtentionsConfig { get; set; }
        public int ExportResultLimit { get; set; }
        public string AccessToken { get; set; }
        public string AOSPortalURL { get; set; }
        public string ChatBotApiURL { get; set; }
        public string ChatBotPortalURL { get; set; }
        public bool DisableChatBot { get; set; }
        public bool ExistAVAUser { get; set; }
        public bool EnableDeleteRestoredDataFeature { get; set; }
        public string CDNUrl { get; set; }
    }
    public class LoginUserInfo 
    {
        public string LogonGroupId { get; set; }
        public string Company { get; set; }
        public string AccountNumber { get; set; }
        public string UserId { get; set; }
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
        public bool IsPhysicalAdmin { get; set; }
        public int RoleType { get; set; }
        public string UserGroup { get; set; }
        public bool EnableRecordsArchiver { get; set; }
        public bool HasArchiverLicense { get; set; }
        public bool HasRecordsLicense { get; set; }
        public bool HasDiscoveryLicense { get; set; }
        public bool HasDiscoverySalesforceLicense { get; set; }
        public bool HasDiscoveryGoogleLicense { get; set; }
        public bool HasDiscoveryFileSystemLicense { get; set; }
        public bool HasDiscoveryExportRowData { get; set; }
        public bool HasGoogleLicense { get; set; }
        public bool HasFileSystemLicense { get; set; }
        public bool EnableDeleteOnly { get; set; }
        public bool EnableArchiverOnly { get; set; }
        public bool EnableArchiverLatestVersion { get; set; }
        public bool EnableArchiverVersionNotIncludeLatest { get; set; }
        public bool EnableCustomizationApp { get; set; }
        public bool? DisableRetentionPeriodLimitation { get; set; }
        public bool EnableFilelevelBackup { get; set; }
        public LicenseType LicenseType { get; set; }
        public bool UseArchiverImportFile { get; set; }
        public bool EnableSoftDelete { get; set; }
        public bool EnableDeleteOrphanData { get; set; }
        public bool EnableApplySettingScanAll { get; set; }
        public bool HasUpgradeTeams { get; set; }
        public bool EnableTeamsFeature { get; set; }
        public bool EnableZeroShotFeature { get; set; }
        public bool EnableAIRecommendationFeature { get; set; }
        public bool EnableMachineLearningFeature { get; set; }
        public bool EnableJPMCFileSystemFeature { get; set; }
        public bool EnableCustomRetentionSettings { get; set; }
        public bool EnableMultiGEOFeature { get; set; }
        public bool IsMultiGeoMainDC { get; set; }
        public bool HasManageHoldEndUser { get; set; }
        public bool HasManagerHold { get; set; }
    }
}
