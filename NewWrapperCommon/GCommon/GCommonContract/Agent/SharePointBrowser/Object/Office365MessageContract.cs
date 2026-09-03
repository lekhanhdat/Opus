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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;

namespace AvePoint.GCommon.Contract.SharePointBrowser
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365PersonalSiteRegistrationContract : BrowserContractBase
    {
        [DataMember]
        public String SitesGroupId { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String AdminCenterUrl { get; set; }
        [DataMember]
        public Dictionary<string, Result> Results { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365MessageContract : BrowserContractBase
    {
        [DataMember]
        public String WebAppDomain { get; set; }
        [DataMember]
        public String SiteCollectionUrl { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String DomainName { get; set; }
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public Result Result { get; set; }
        [DataMember]
        public String WebAppUrl { get; set; }
        [DataMember]
        public SiteCollectionScanType ScanType { get; set; }

        /// <summary>
        /// Tenant Admin的Url
        /// </summary>
        [DataMember]
        public String AdminCenterUrl { get; set; }


        #region SuperUserConfiguration Info

        [DataMember]
        public String TenantId { get; set; }

        [DataMember]
        public String TenantName { get; set; }

        #endregion

        [DataMember]
        public AccountValidateResult AccountValidateResult { get; set; }

        #region For App Management
        [DataMember]
        public AzureRegions AzureRegion { get; set; }

        [DataMember]
        public String ApplicationId { get; set; }

        [DataMember]
        public string AppTokenCertBase64String { get; set; }

        [DataMember]
        public string AppTokenCertPassword { get; set; }

        [DataMember]
        public string AuthUrl { get; set; }

        [DataMember]
        public string ResourceUrl { get; set; }

        [DataMember]
        public string GraphToken { get; set; }

        [DataMember]
        public Exception ValidateAppProfileException { get; set; }

        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }

        [DataMember]
        public string AppProfileId { get; set; }
        #endregion

        [DataMember]
        public string AccountProfileId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountValidateResult
    {
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; }

        [DataMember]
        public AccountRole AccountRole { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Result
    {
        [DataMember]
        public String Id { get; set; } 
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; } 
        [DataMember]
        public Boolean Status { get; set; }
        [DataMember]
        public BPOSMould Office365Mould { get; set; }
        [DataMember]
        public String SiteCollectionUrl { get; set; }
        [DataMember]
        public String WebTemplateName { get; set; }
        [DataMember]
        public String WebTemplateTitle { get; set; }
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public CompatibilityLevelType CompatibilityLevel { get; set; }
        [DataMember]
        public Boolean IsOnlineSite { get; set; }
        [DataMember]
        public Boolean IsResidentInLocalFarm { get; set; }
        [DataMember]
        public String FarmId { get; set; }
        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }
        [DataMember]
        public String FarmBuildVersion { get; set; }
        [DataMember]
        public SiteLockStatus SiteLockStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        AdminCenter = 1,
        [EnumMember]
        OneDrive = 2,
        [EnumMember]
        TeamSite = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BPOSMould
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Server = 1,
        [EnumMember]
        BPOS_D = 2,
        [EnumMember]
        BPOS_S = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ErrorInfo
    {
        [EnumMember]
        Unknown = -1,

        //需要对此值特殊处理
        [EnumMember]
        NoError = 0,

        [EnumMember]
        BadUrl = 1,
        [EnumMember]
        UnAuthorized = 2,
        [EnumMember]
        TimeOut = 3,

        //NotFound暂时无法解析
        [EnumMember]
        NotFound = 4,

        [EnumMember]
        DotNet45Required = 5,
        [EnumMember]
        PasswordExpired = 6,
        [EnumMember]
        PasswordNotMatch = 7,
        [EnumMember]
        HostnameCannotResolved = 8,
        [EnumMember]
        ConnectionFailed = 9,
        [EnumMember]
        WebApplicationNotFound = 10,
        [EnumMember]
        NotGlobalAdmin = 11,

        // used for app token
        [EnumMember]
        AppTokenCertAndCertPwdNotMatch = 12,
        [EnumMember]
        AppTokenTenantIdNotFound = 13,
        [EnumMember]
        AppTokenClientIdNotFound = 14,
        [EnumMember]
        AppTokeCertificateNotMatch= 15,
        [EnumMember]
        AppTokenUnknownError = 16,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AzureRegions
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        AzureGlobal = 1,

        [EnumMember]
        Azure21V = 2,

        [EnumMember]
        AzureGerman = 3,

        [EnumMember]
        AzureUSGov = 4,

        [EnumMember]
        AzureUSGovDoD = 5
    }

    #region Office365 CreateSiteCollection

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365CreateMessageContract : BrowserContractBase
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public UserDetail Primary { get; set; }

        [DataMember]
        public UserDetail Secondary { get; set; }

        [DataMember]
        public int CompatibilityLevel { get; set; }

        [DataMember]
        public int TimeZoneId { get; set; }

        [DataMember]
        public int SiteLanguage { get; set; }

        [DataMember]
        public List<string> SiteLanguages { get; set; }

        [DataMember]
        public string SiteTemplate { get; set; }

        [DataMember]
        public List<SiteTemplate> SiteTemplates { get; set; }

        [DataMember]
        public long StorageQuota { get; set; }

        [DataMember]
        public long AvailableStorageQuota { get; set; }

        [DataMember]
        public double ResourceQuota { get; set; }

        [DataMember]
        public double AvailableResourceQuota { get; set; }

        //[DataMember]
        //public List<string> WebAppCommonLanguages { get; set; }

        [DataMember]
        public List<ManagedPath> ManagedPaths { get; set; }

        [DataMember]
        public ManagedPath ManagedPath { get; set; }

        //[DataMember]
        //public string SiteUrlName { get; set; }

        [DataMember]
        public Result Result { get; set; }
    }

    [DataContract(Namespace = "http://www.avepoint.com")]
    public class ManagedPath
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int PrefixType { get; set; }

        public override bool Equals(object obj)
        {
            ManagedPath path = obj as ManagedPath;
            if (path != null && this.Name.Equals(path.Name) && this.PrefixType == path.PrefixType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    public class PrefixType
    {
        public const int Explicit = 0;
        public const int ExplicitInclusion = 0;
        public const int Wildcard = 1;
        public const int WildcardInclusion = 1;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteTemplate
    {
        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public int LCID { get; set; }

        [DataMember]
        public List<Template> Templates { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubTemplate
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            SubTemplate subTemplate = obj as SubTemplate;
            if (this.DisplayName.Equals(subTemplate.DisplayName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.DisplayName.GetHashCode();
        }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Template
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<SubTemplate> SubTemplates { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string TemplateDescription { get; set; }

        /// <summary>
        /// 前台Merge数据时，需要重写自定义类的Equals方法，add by Zhang Hailong
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Template template = obj as Template;
            if (this.Name.Equals(template.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuthorizeType
    {
        //Local模拟365时仍然需要使用UserName+Password形式。此类型便于Manager逻辑中的判断
        [EnumMember]
        UserNameAndPwd = 0,
        [EnumMember]
        AccountInfo = 1,
        [EnumMember]
        AppTokenInfo = 2,
        [EnumMember]
        MixAuthorizeInfo = 3,
    }
}