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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using DocAveOnline.WebApi.Contracts;

namespace AvePoint.GCommon.Contract.SharePointBrowser
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365PersonalSiteRegistrationContract : BrowserContractBase
    {
        [DataMember]
        public String SitesGroupId { get; set; }
        [DataMember]
        public CentralAdmin.Object.BposInfo BPOSInfo { get; set; }
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
    public class ScanMySitesInfo
    {
        [DataMember]
        public Office365PersonalSiteRegistrationContract Message { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365MessageContract : BrowserContractBase
    {
        [DataMember]
        public String ServiceAccountId { get; set; }
        [DataMember]
        public String CustomId { get; set; }
        [DataMember]
        public String WebAppDomain { get; set; }
        [DataMember]
        public String SitesGroupId { get; set; }
        [DataMember]
        public String SiteCollectionUrl { get; set; }
        [DataMember]
        public CentralAdmin.Object.BposInfo BPOSInfo { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public String DomainName { get; set; }
        [DataMember]
        public Result Result { get; set; }
        [DataMember]
        public String ListUrl { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string AdminUrl { get; set; }
        [DataMember]
        public bool UseAppToken { get; set; }
        [DataMember]
        public TokenType TokenType { get; set; }
        [DataMember]
        public String NeedCheckedUserMail { get; set; }
        [DataMember]
        public String FileFullUrl { get; set; }
        [DataMember]
        public String StubType { get; set; }
        [DataMember]
        public CheckPermissionAction CheckPermissionAction { get; set; }
        [DataMember]
        public string NeedCheckedGroupId { get; set; }
        [DataMember]
        public string SpecifiedGroupNameForSharePointSite { get; set; }
        [DataMember]
        public String NeedCheckedUserUPN { get; set; }
        [DataMember]
        public bool UseSiteMapping { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365CheckChangeMessageContract : BrowserContractBase
    {
        [DataMember]
        public String SiteCollectionUrl { get; set; }
        [DataMember]
        public CentralAdmin.Object.BposInfo BPOSInfo { get; set; }
        [DataMember]
        public String Password { get; set; }
        [DataMember]
        public String UserName { get; set; }
        [DataMember]
        public bool Changed { get; set; }
        [DataMember]
        public long StartTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScanSitesInfo
    {
        [DataMember]
        public Office365MessageContract Message { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365UserContract : BrowserContractBase
    {
        [DataMember]
        public String Domain { get; set; }
        [DataMember]
        public Int32 UserCount { get; set; }
        [DataMember]
        public Int32 SiteCollectionCount { get; set; }
        [DataMember]
        public int OneDriveCount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Result
    {
        [DataMember]
        public string ErrorDetail { get; set; }
        [DataMember]
        public ErrorInfo ErrorInfo { get; set; }
        [DataMember]
        public Boolean Status { get; set; }
        [DataMember]
        public BPOSMould Office365Mould { get; set; }
        [DataMember]
        public string SiteCollectionUrl { get; set; }
        [DataMember]
        public string WebTemplateName { get; set; }
        [DataMember]
        public string SPVersion { get; set; }
        [DataMember]
        public string TemplateTitle { get; set; }
        [DataMember]
        public Boolean IsPublicWebSite { get; set; }
        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }
        [DataMember]
        public uint Lcid { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public bool IsReadOnlySite { get; set; }
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
        [EnumMember]
        NoError = 0,
        [EnumMember]
        BadUrl = 1,
        [EnumMember]
        UnAuthorized = 2,
        [EnumMember]
        TimeOut = 3,
        [EnumMember]
        NotFound = 4,
        [EnumMember]
        PasswordExpired = 5,
        [EnumMember]
        InvalidRegistrationType = 6,
        [EnumMember]
        SmallBusinessSiteError = 7,
        [EnumMember]
        InvalidFileExtension = 8,
        [EnumMember]
        InvalidSolutionFile = 9,
        [EnumMember]
        GlobalAdminError = 10,
        [EnumMember]
        ValidateListTemplateError = 11,
        [EnumMember]
        CannotScanSmallBusinessSite = 12,
        [EnumMember]
        SameTenant = 13,
        [EnumMember]
        AdminCenterUrlInvalid = 14,
        [EnumMember]
        InvalidProjectAccount = 15,
        [EnumMember]
        UserCannotFound = 16,
        [EnumMember]
        SecurityTrimingException = 17,
        [EnumMember]
        InsufficientPrivileges = 18,
        [EnumMember]
        SiteCollectionLocked = 19,
        [EnumMember]
        UserNotGroupOwnerOrMember = 20,
        [EnumMember]
        UserNotOwnerForSharePointSite = 21,
        [EnumMember]
        UserNotOwnerOrMemberForSharePointSite = 22,
        [EnumMember]
        UserNotOwnerOrSpecifiedGroupForSharePointSite = 23,
        [EnumMember]
        PermissionError = 24,
        [EnumMember]
        OopStubNotFound = 25,
        [EnumMember]
        UserNotOwnerOrMemberOrVisitorForSharePointSite = 26,
        [EnumMember]
        ActiveAppProfileNotFound = 27,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        AdminCenter = 1,
        [EnumMember]
        Teams = 2,
        [EnumMember]
        PrivateChannel = 3,
        [EnumMember]
        Group = 4,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckPermissionAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SiteOwner = 1,
        [EnumMember]
        ArchiverStubFile = 2,
        [EnumMember]
        GroupOwner = 3,
        [EnumMember]
        GroupOwnerOrMember = 4,
        [EnumMember]
        SiteOwnerOrSiteMember = 5,
        [EnumMember]
        SiteOwnerOrSpecialGroup = 6,
        [EnumMember]
        SiteOwnerOrSiteMemberGroupOrSiteVisitor = 7
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

        [DataMember]
        public string CustomerSolutionPath { get; set; }

        [DataMember]
        public string ForderPath { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool OverwriteRecyclebin { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365CustomSolutionContract : BrowserContractBase
    {
        [DataMember]
        public int SiteLanguage { get; set; }

        [DataMember]
        public string CustomerSolutionPath { get; set; }

        [DataMember]
        public List<SiteTemplate> CustomerSiteTemplates { get; set; }

        [DataMember]
        public Result Result { get; set; }

        [DataMember]
        public string ForderPath { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public CentralAdmin.Object.BposInfo BPOSInfo { get; set; }

        [DataMember]
        public String Password { get; set; }

        [DataMember]
        public String UserName { get; set; }

        [DataMember]
        public byte[] fileData;

        [DataMember]
        public String fileType;
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
            if (template == null) return false;
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

}
