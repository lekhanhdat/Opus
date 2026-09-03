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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RemoteWebApplication
    {
        [XmlIgnore]
        [DataMember]
        public String id { get; set; }
        /// <summary>
        /// 该属性不在使用
        /// </summary>
        [XmlAttribute("domainName")]
        [DataMember]
        public String domainName { get; set; }
        /// <summary>
        /// 该属性不在使用
        /// </summary>
        [XmlAttribute("useSSL")]
        [DataMember]
        public Boolean useSSL { get; set; }
        /// <summary>
        /// 该属性对应界面的domain name
        /// </summary>
        [XmlAttribute("url")]
        [DataMember]
        public String url { get; set; }
        [XmlAttribute("agentGroupId")]
        [DataMember]
        public String agentGroupId { get; set; }
        [XmlAttribute("agentGroupName")]
        [DataMember]
        public String agentGroupName { get; set; }
        [DataMember]
        [XmlAttribute("description")]
        public String description { get; set; }
        [DataMember]
        [XmlAttribute("modifiedDate")]
        public long modifiedDate { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [DataMember]
        [XmlAttribute("SiteGroupType")]
        public SiteGroupType SiteGroupType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RemoteSiteCollection
    {
        [XmlIgnore]
        [DataMember]
        public String id { get; set; }
        [XmlAttribute("url")]
        [DataMember]
        public String url { get; set; }
        [XmlIgnore]
        [DataMember]
        public String parentId { get; set; }
        [XmlAttribute("username")]
        [DataMember]
        public String username { get; set; }
        [XmlAttribute("domain")]
        [DataMember]
        public String domain { get; set; }
        [XmlAttribute("password")]
        [DataMember]
        public String password { get; set; }
        [XmlAttribute("state")]
        [DataMember]
        public SiteCollectionState state { get; set; }
        /// <summary>
        /// 该属性不应该再被使用
        /// </summary>
        [XmlAttribute("agentGroupId")]
        [DataMember]
        public String agentGroupId { get; set; }
        [XmlAttribute("BPOSMould")]
        [DataMember]
        public String BPOSMould { get; set; }
        [XmlAttribute("AvailableAgentIds")]
        [DataMember]
        public List<String> AvailableAgentIds { get; set; }
        /// <summary>
        /// 该属性给tree检测License使用
        /// </summary>
        [XmlAttribute("CreateTime")]
        [DataMember]
        public long CreateTime { get; set; }

        /// <summary>
        /// sitecollection 的template name
        /// </summary>
        [XmlAttribute("TemplateName")]
        [DataMember]
        public String TemplateName { get; set; }

        [XmlAttribute("SPVersion")]
        [DataMember]
        public String SPVersion { get; set; }

        [XmlAttribute("CompatibilityLevel")]
        [DataMember]
        public CompatibilityLevelType CompatibilityLevel { get; set; }

        [XmlAttribute("IsOnlineSite")]
        [DataMember]
        public IsOnlineSite IsOnlineSite { get; set; }

        [XmlAttribute("IsResidentInLocalFarm")]
        [DataMember]
        public Boolean IsResidentInLocalFarm { get; set; }

        [XmlAttribute("FarmId")]
        [DataMember]
        public String FarmId { get; set; }

        [XmlAttribute("WebTemplateTitle")]
        [DataMember]
        public String TemplateTitle { get; set; }

        [XmlAttribute("SiteCollectionType")]
        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }

        [XmlAttribute("RealId")]
        [DataMember]
        public String RealId { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [XmlAttribute("FarmBuildVersion")]
        [DataMember]
        public String FarmBuildVersion { get; set; }

        [XmlAttribute("SiteLockStatus")]
        [DataMember]
        public SiteLockStatus SiteLockStatus { get; set; }

        [XmlAttribute("AuthorizeType")]
        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }

        [XmlAttribute("AppProfileId")]
        [DataMember]
        public string AppProfileId { get; set; }

        [XmlAttribute("AppProfileName")]
        [DataMember]
        public string AppProfileName { get; set; }

        [XmlAttribute("AccountProfileId")]
        [DataMember]
        public string AccountProfileId { get; set; }

        [XmlAttribute("AccountProfileName")]
        [DataMember]
        public string AccountProfileName { get; set; }

        public override string ToString()
        {
            return string.Format("RemoteSiteCollection[Id {0}, Url {1}]", id, url);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IsOnlineSite
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        IsOnline = 1,
        [EnumMember]
        Others = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionState
    {
        [EnumMember]
        AccessAll = 0,
        [EnumMember]
        AccessSome = 1,
        [EnumMember]
        AccessNone = 2,
        [EnumMember]
        Unmatched = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365TestResult
    {
        [DataMember]
        public String RealId { get; set; }
        [DataMember]
        public String OriginalSiteCollection { get; set; }
        [DataMember]
        public SiteCollectionState SiteCollectionState { get; set; }
        [DataMember]
        public Boolean WebApplicationState { get; set; }
        [DataMember]
        public Dictionary<String, ErrorInfo> ErrorInfo { get; set; }
        [DataMember]
        public BPOSMould BPOSMould { get; set; }
        [DataMember]
        public List<String> AvailableAgentIds { get; set; }
        [DataMember]
        public String RealSiteCollection { get; set; }
        [DataMember]
        public String TemplateName { get; set; }
        [DataMember]
        public String SPVersion { get; set; }
        [DataMember]
        public CompatibilityLevelType CompatibilityLevel { get; set; }
        [DataMember]
        public IsOnlineSite IsOnlineSite { get; set; }
        [DataMember]
        public Boolean IsResidentInLocalFarm { get; set; }
        [DataMember]
        public String FarmId { get; set; }
        [DataMember]
        public String TemplateTitle { get; set; }
        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }
        [DataMember]
        public Boolean HasAvailableAgentByStatus { get; set; }
        [DataMember]
        public String FarmBuildVersion { get; set; }
        [DataMember]
        public Boolean isTypeMismatch { get; set; }
        [DataMember]
        public SiteLockStatus SiteLockStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365ScanSitesResult
    {
        [DataMember]
        public ScanSitesResultState ResultState { get; set; }
        [DataMember]
        public List<Result> SiteResults { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanSitesResultState
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Error,
        [EnumMember]
        NoSiteCollection,
        [EnumMember]
        NoAvailableAgent,
        [EnumMember]
        UnAuthorized,
        [EnumMember]
        PasswordExpired,
        [EnumMember]
        WebApplicationNotFound,
        [EnumMember]
        NoPermission,
        [EnumMember]
        DotNet45Required,
        [EnumMember]
        UnknowError
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RegisterSitesResult
    {
        [DataMember]
        public RegisterSitesResultState ResultState { get; set; }

        [DataMember]
        public List<string> FailedSiteUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RegisterSitesResultState
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Error,
        [EnumMember]
        NoAvailableAgent,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuthorisedRemoteNodeDto
    {
        [DataMember]
        public List<string> AuthorisedSiteCollectionIds { get; set; }
        /// <summary>
        /// 有权限的WebAppIds和farmId 对应关系
        /// 如果为Null则不需要过滤webApp节点
        /// </summary>
        [DataMember]
        public Dictionary<string, List<string>> AuthorisedWebApplicationIds { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365RequestInfo
    {
        //删除Site和Site Group的时候判断用
        [DataMember]
        public DeleteMode DeleteMode { get; set; }

        [DataMember]
        public List<string> SiteCollectionIds { get; set; }

        [DataMember]
        public List<string> WebApplicationIds { get; set; }

        [DataMember]
        public Office365MessageContract Message { get; set; }

        [DataMember]
        public string WebApplicationId { get; set; }

        [DataMember]
        public string AgentGroupId { get; set; }

        [DataMember]
        public List<string> SiteCollectionUrl { get; set; }

        [DataMember]
        public List<ScanModeSiteCollection> ScanModeSiteCollections { get; set; }

        [DataMember]
        public List<string> DeleteSiteCollectionIds { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> UpdateSiteCollections { get; set; }

        [DataMember]
        public int ScanScope { get; set; }

        [DataMember]
        public string AccountProfileId { get; set; }
        
        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }
        
        [DataMember]
        public string AppProfileId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScanModeSiteCollection
    {
        [DataMember]
        public Office365MessageContract Message { get; set; }

        [DataMember]
        public string WebApplicationId { get; set; }

        [DataMember]
        public string AgentGroupId { get; set; }

        [DataMember]
        public List<string> SiteCollectionUrl { get; set; }

        [DataMember]
        public SiteGroupType SiteGroupType { get; set; }

        [DataMember]
        public ScanModeOperation SiteModeOperation { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365ResponseInfo
    {
        [DataMember]
        public OperateResult Result { get; set; }

        [DataMember]
        public List<string> UsingPlanNames { get; set; }

        [DataMember]
        public List<string> NotDeleteSiteCollectionIds { get; set; }

        [DataMember]
        public List<string> NotDeleteWebApplicationIds { get; set; }

        [DataMember]
        public List<string> DeletedNames { get; set; }

        [DataMember]
        public Office365ScanSitesResult ScanSitesResult { get; set; }

        [DataMember]
        public RegisterSitesResult RegisterSitesResult { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> RemoteSiteCollections { get; set; }

        [DataMember]
        public List<Office365TestResult> SaveFailedScanSiteCollsResults { get; set; }

        [DataMember]
        public SiteCollectionScanType ScanType { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> SavedSiteCollections { get; set; }

        [DataMember]
        public List<string> ArchiverUsingInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365ImportSitesResult
    {
        [DataMember]
        public ImportSitesResultState ResultState { get; set; }
        [DataMember]
        public int RegisteredCount { get; set; }
        [DataMember]
        public List<Office365TestResult> Results { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ImportSitesResultState
    {
        [EnumMember]
        NoSiteCollection,
        [EnumMember]
        NoAvailableAgent,
        [EnumMember]
        SitesAccessSome,
        [EnumMember]
        ReadFileError,
        [EnumMember]
        AllSitesExist,
        [EnumMember]
        SitesAccessNone,
        /// <summary>
        /// Batch Add Site Collection专用，正常检测不会有此返回结果
        /// </summary>
        [EnumMember]
        SomeSitesExist,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionScanType
    {
        /// <summary>
        /// for Normal Site & OneDrive Site
        /// </summary>
        [EnumMember]
        Online = 0,
        /// <summary>
        /// for Local Farm
        /// </summary>
        [EnumMember]
        OnPremise = 1,
        /// <summary>
        /// for Group Team Site
        /// </summary>
        [EnumMember]
        TeamSite = 2,
    }

    public enum DeleteMode
    {
        [EnumMember]
        Unkown,
        [EnumMember]
        SiteCollection,
        [EnumMember]
        WebAppliation
    }

    public enum OperateResult
    {
        [EnumMember]
        Unkown,
        [EnumMember]
        Success,
        [EnumMember]
        Failure
    }

    public enum ScanModeOperation
    {
        [EnumMember]
        Add,
        [EnumMember]
        Remove,
        [EnumMember]
        Update
    }

    public enum SiteGroupType
    {
        [EnumMember]
        SharePointSites = 0,
        [EnumMember]
        OneDriveForBusiness = 1,
        [EnumMember]
        TeamSiteGroup = 2
    }

    public enum SiteLockStatus
    {
        [EnumMember]
        None,
        [EnumMember]
        ReadOnly
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CreateSiteCollection4GAResult
    {
        //如果保存成功 返回SiteCollection对象
        [DataMember]
        public RemoteSiteCollection SiteCollection { get; set; }
        [DataMember]
        public AuthorizeType AuthorizeType { get; set; }
        //整体Test结果
        [DataMember]
        public Test4GAResultType Type { get; set; }
        //整体异常信息
        [DataMember]
        public string ExceptionMessage { get; set; }
        //每个AppProfile的Test结果
        [DataMember]
        public List<Test4GAResult> TestResultList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Test4GAResultType
    {
        [EnumMember]
        UnknowException = -1,
        [EnumMember]
        Success = 0,
        [EnumMember]
        UrlAlreadyExist = 1,
        [EnumMember]
        SaveSiteCollectionFailed = 2,
        [EnumMember]
        NoDefaultSiteGroup = 3,
        [EnumMember]
        AllTestFailed = 4,
        [EnumMember]
        NoDefaultBposAgentGroup = 5,
        [EnumMember]
        GetOrCreateDefaultSiteGroupFailed = 6,
        [EnumMember]
        GetOrCreateOffice365AccountProfileFailed = 7,
        [EnumMember]
        NoAvailableAuthorizeInfo = 8
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Test4GAResult
    {
        [DataMember]
        public string AppProfileId { get; set; }
        [DataMember]
        public string AppProfileName { get; set; }
        [DataMember]
        public string AccountProfileId { get; set; }
        [DataMember]
        public string AccountProfileName { get; set; }
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public Office365TestResult TestResult { get; set; }
    }
}