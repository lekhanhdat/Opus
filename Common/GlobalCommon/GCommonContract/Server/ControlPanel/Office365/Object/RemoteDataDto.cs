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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.SkyDrivePro.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;

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
        /// 该属性对应界面的group name
        /// </summary>
        [XmlAttribute("url")]
        [DataMember]
        public String url { get; set; }

        [DataMember]
        [XmlAttribute("description")]
        public String description { get; set; }

        [DataMember]
        [XmlAttribute("modifiedDate")]
        public long modifiedDate { get; set; }

        /// <summary>
        /// CP中用来区分SiteCollection和My Site（Create、Update）
        /// </summary>
        [DataMember]
        [XmlAttribute("nodeType")]
        public RemoveNodeType NodeType { get; set; }

        [IgnoreDataMember]
        public bool FromDAO { get; set; }

        [IgnoreDataMember]
        public string AosId { get; set; }

        public override string ToString()
        {
            return string.Format("RemoteWebApplication[Id {0}, Url {1}]", id, url);
        }
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

        [XmlIgnore]
        [DataMember]
        public String parentName { get; set; }

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

        /// <summary>
        /// 用来区分SP的版本，格式是int32.int32.int32.int32
        /// </summary>
        [XmlAttribute("SPVersion")]
        [DataMember]
        public String SPVersion { get; set; }

        [XmlAttribute("TemplateTitle")]
        [DataMember]
        public String TemplateTitle { get; set; }

        [XmlAttribute("IsPublicWebSite")]
        [DataMember]
        public bool IsPublicWebSite { get; set; }

        /// <summary>
        /// 供My Site界面上显示DisPlayName（email address）用
        /// </summary>
        [XmlAttribute("Name")]
        [DataMember]
        public String Name { get; set; }

        /// <summary>
        /// CP中用来区分SiteCollection和My Site（Create、Update）
        /// </summary>
        [DataMember]
        [XmlAttribute("nodeType")]
        public RemoveNodeType NodeType { get; set; }

        /// <summary>
        /// 对此site collection有权限的Tenant Group Id
        /// </summary>
        [XmlAttribute("TenantGroupId")]
        [DataMember]
        public String TenantGroupId { get; set; }

        [XmlAttribute("SiteCollectionType")]
        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }

        [DataMember]
        public List<ObjectPermissionDto> ObjectPermissions { get; set; }

        [XmlAttribute("AdminUrl")]
        [DataMember]
        public string AdminUrl { get; set; }

        [XmlAttribute("ServiceAccountId")]
        [DataMember]
        public string ServiceAccountId { get; set; }

        [XmlAttribute("TenantId")]
        [DataMember]
        public string TenantId { get; set; }

        [XmlAttribute("AppType")]
        [DataMember]
        public AvePoint.GCommon.Contract.CentralAdmin.Object.AppType AppType { get; set; }


        [XmlAttribute("AuthType")]
        [DataMember]
        public AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType AuthType { get; set; }

        [XmlAttribute("ScanSource")]
        [DataMember]
        public RemoteNodeScanSource ScanSource { get; set; }

        [XmlAttribute("TeamId")]
        [DataMember]
        public String TeamId { get; set; }


        //[XmlAttribute("SecondParentId")]
        //[DataMember]
        //public String SecondParentId { get; set; }

        [XmlAttribute("AADEnvironment")]
        [DataMember]
        public AADEnvironment AADEnvironment { get; set; }

        [XmlAttribute("ObjectId")]
        [DataMember]
        public String ObjectId { get; set; }

        [IgnoreDataMember]
        public bool FromDAO { get; set; }

        [IgnoreDataMember]
        public BposInfo Bpos { get; set; }

        [XmlAttribute("ChannelType")]
        [DataMember]
        public TeamsChannelType ChannelType { get; set; }

        [DataMember]
        public bool isPlanProfileSelected { get; set; }
        public override string ToString()
        {
            return string.Format("RemoteSiteCollection[Id {0}, Url {1}]", id, url);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteCollectionState
    {
        [EnumMember]
        AccessAll,

        [EnumMember]
        AccessSome,

        [EnumMember]
        AccessNone,

        [EnumMember]
        AccountExpired,

        [EnumMember]
        Notinitialize,

        [EnumMember]
        AdminCenterUrlInvalid,
    }

    public enum SiteTestState
    {
        [EnumMember]
        AccessAll,

        [EnumMember]
        AccessSome,

        [EnumMember]
        AccessNone,

        [EnumMember]
        AccountExpired,

        [EnumMember]
        Notinitialize,

        [EnumMember]
        InvalidRegistrationType,

        [EnumMember]
        AdminCenterUrlInvalid
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RemoteNodeScanSource
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        AOS = 1,
        [EnumMember]
        ControlPanel = 2,
        [EnumMember]
        AutoScan = 3,
        [EnumMember]
        CreateContainer = 4,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365TestResult
    {
        [DataMember]
        public SiteTestState SiteCollectionTestState { get; set; }

        [DataMember]
        public Boolean webApplicationState { get; set; }

        [DataMember]
        public Dictionary<String, ErrorInfo> ErrorInfo { get; set; }

        [DataMember]
        public BPOSMould BPOSMould { get; set; }

        [DataMember]
        public List<String> availableAgentIds { get; set; }

        [DataMember]
        public String RealSiteCollection { get; set; }

        [DataMember]
        public String TemplateName { get; set; }

        [DataMember]
        public String SPVersion { get; set; }

        [DataMember]
        public String TemplateTitle { get; set; }

        [DataMember]
        public bool IsPublicWebSite { get; set; }

        [DataMember]
        public SiteCollectionType SiteCollectionType { get; set; }

        [DataMember]
        public SiteCollectionState SiteCollectionState { get; set; }

        [DataMember]
        public String SiteId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365ImportSitesResult
    {
        [DataMember]
        public ImportSitesResultState ResultState { get; set; }

        [DataMember]
        public int UpAgentCount { get; set; }

        [DataMember]
        public int RegisteredCount { get; set; }

        [DataMember]
        public List<Office365TestResult> Results { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365ScanSitesResult
    {
        [DataMember]
        public ScanSitesResultState ResultState { get; set; }

        [DataMember]
        public List<Result> SiteResults { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public long TimeOut { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Office365SelectSitesResult
    {
        [DataMember]
        public bool HasResult { get; set; }

        [DataMember]
        public Dictionary<string, List<Result>> SitesGroupAndSiteMap { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RegisterSitesResult
    {
        [DataMember]
        public RegisterSitesResultState ResultState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReconnectSitesResult
    {
        [DataMember]
        public SiteCollectionState ResultState { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> SiteList { get; set; }
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
        AdminCenterUrlInvalid,
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
        NoPermission,

        [EnumMember]
        HasSmallBusinessSite,

        [EnumMember]
        UnknowError,

        /// <summary>
        /// 没有在aos中创建 Registration Profile，不能scan sites
        /// </summary>
        [EnumMember]
        NoRegistrationProfile,

        [EnumMember]
        UnFinish,

        [EnumMember]
        TimeOut,

        [EnumMember]
        BadAdminUrl,
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
    public class AllUncountSiteAccountInfo
    {
        [DataMember]
        public List<SiteAccountInfo> ExpiredAccounts { get; set; }

        [DataMember]
        public List<SiteAccountInfo> UnaccessAccounts { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteAccountInfo
    {
        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public string Account { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteOrGroupDeleteInfo
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Url { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RemoveNodeType
    {
        [EnumMember]
        SiteCollection,

        [EnumMember]
        SkyDrivePro,

        [EnumMember]
        O365GroupSites,

        [EnumMember]
        PrivateChannel
    }
    [DataContract]
    public class RemoteNodePara
    {
        [DataMember(Name = "NI")]
        public string NodeId { get; set; }
        [DataMember(Name = "NN")]
        public string NodeName { get; set; }
        [DataMember(Name = "NT")]
        public AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType NodeType { get; set; }
        [DataMember(Name = "NL")]
        public NodeLevel NodeLevel { get; set; }
        public string AosId { get; set; }
    }


    [DataContract]
    public class SyncRemoteNodePara
    {
        public string NodeName { get; set; }
        [DataMember(Name = "PI")]
        public string ParentId { get; set; }
        [IgnoreDataMember]
        public string ParentName { get; set; }
        [DataMember(Name = "NL")]
        public NodeLevel NodeLevel { get; set; }
        /// <summary>
        /// 如果是Onedrive Site，对应的是User Name. 如果是Group Site，对应的是Group Name
        /// </summary>
        public string RelatedName { get; set; }
        [DataMember(Name = "AuT")]
        public BposConnectionType AuthType { get; set; }
        [DataMember(Name = "ApT")]
        public AppType AppType { get; set; }
        [DataMember(Name = "SAI")]
        public string ServiceAccountId { get; set; }
        [DataMember(Name = "SS")]
        public RemoteNodeScanSource ScanSource { get; set; }
        [DataMember(Name = "TI")]
        public string TenantId { get; set; }
        [DataMember(Name = "UN")]
        public string UserName { get; set; }
        [DataMember(Name = "TM")]
        public string TeamId { get; set; }
        [DataMember(Name = "SPI")]
        public string SecondParentId { get; set; }
        [IgnoreDataMember]
        public string ObjectId { get; set; }
    }

    public class RemoteNodeBaseInfo
    {
        public string NodeId { get; set; }
        public string NodeName { get; set; }
        public string ParentId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TeamsChannelType
    {
        [EnumMember]
        None = -1,

        [EnumMember]
        Private,

        [EnumMember]
        Shared,
    }

    #region SharePointSites

    public struct Office365SettingDownloadParameter
    {
        public List<string> AccessUrls { get; set; }

        public List<string> UnaccessUrls { get; set; }
    }

    public enum Office365DataRetriveOption
    {
        All,
        WebAppOnly,
        SiteCollectionOnly,
    }

    public struct Office365SharePointSiteGroup
    {
        public List<RemoteWebApplication> WebApps { get; set; }

        public List<RemoteSiteCollection> SiteCollections { get; set; }
    }

    #endregion

    #region Exchange

    public struct ExchangeMailGroup
    {
        public List<EmailAccountGroupDto> Groups { get; set; }

        public List<EmailAccountDto> MailBoxs { get; set; }
    }

    #endregion

    #region One Driver

    public struct OneDriverGroup
    {
        public List<RemoteWebApplication> WebApps { get; set; }

        public List<RemoteSiteCollection> SiteCollections { get; set; }
    }

    #endregion

    #region Office Group

    public struct Office365Group
    {
        public List<EmailAccountGroupDto> Groups { get; set; }

        public List<EmailAccountDto> Items { get; set; }
    }

    #endregion

    #region Office365 Service Account
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class O365ServiceAccountDto
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String UserName { get; set; }

        [DataMember]
        public String Password { get; set; }

        [DataMember]
        public String TenantId { get; set; }

        [DataMember]
        public String TenantName { get; set; }

        [DataMember]
        public String AdminUrl { get; set; }

        [DataMember]
        public long UpdateTime { get; set; }

        public override string ToString()
        {
            return string.Format("O365 Service Account [Id {0}, UserName {1}, TenantId {2}, AdminUrl {3}]", Id, UserName, TenantId, AdminUrl);
        }
    }
    #endregion
}