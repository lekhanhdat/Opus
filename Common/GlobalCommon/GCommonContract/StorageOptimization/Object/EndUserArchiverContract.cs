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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Storage.Entity;
using DocAveOnline.WebApi.Contracts;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// control和web part通信使用的契约
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RelativeDataArchiverContract
    {
        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string WebappId { get; set; }

        [DataMember]
        public string WebappUrl { get; set; }

        [DataMember]
        public string SiteId { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string NodeName { get; set; }

        [DataMember]
        public int NodeLevel { get; set; }

        [DataMember]
        public string FullPath { get; set; }

        /// <summary>
        /// 当期web part机器登陆SharePoint的用户名，用于在control端的job monitor中的detail显示使用
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// 当前web part所在机器的相关信息，在给client发请求起job的时候要根据这个属性给当前的agent发消息
        /// </summary>
        [DataMember]
        public string AgentAddress { get; set; }

        /// <summary>
        /// control根据该属性组装一个rule的Dictionary给client跑job使用
        /// </summary>
        [DataMember]
        public string RuleId { get; set; }

        [DataMember]
        public Rule Rule { get; set; }

        /// <summary>
        /// 该属性保存从web part传过来的属性经过control不处理直接传给client用来做job
        /// </summary>
        [DataMember]
        public string MetaData { get; set; }

        [DataMember]
        public EndUserRestoreOption EndUserRestoreOption { get; set; }

        /// <summary>
        /// 此属性用于在control端判断给client返回的数据
        /// </summary>
        [DataMember]
        public int ViewRestoreOption { get; set; }
    }

    /// <summary>
    /// 在End User View的时候client向control要相关信息的时候用的契约
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserViewInfo
    {
        [DataMember]
        public List<MediaSettingInfo> MediaSettingInfos { get; set; }

        [DataMember]
        public LogicalDeviceDto IndexDeviceInfo { get; set; }

        [DataMember]
        public bool IsLicenseAvailable { get; set; }

        [DataMember]
        public List<LogicalDeviceDto> LogicalDeviceInfos { get; set; }

        [DataMember]
        public List<RestoreSecurityInfoWrapper> RestoreSecurityInfos { get; set; }
    }

    /// <summary>
    /// 此类用于给client传Media信息使用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MediaSettingInfo
    {
        [DataMember]
        public string MediaAddress { get; set; }

        [DataMember]
        public int MediaPort { get; set; }

        [DataMember]
        public string Schema { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserArchiverMessage
    {
        [DataMember]
        public EndUserArchiverMessageType MessageType { get; set; }

        [DataMember]
        public string JobID { get; set; }

        [DataMember]
        public EndUserArchiverFailedType FailedType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TagInfo
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EndUserArchiverMessageType
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Failed
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EndUserArchiverFailedType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MeidaIsDown = 1,
        [EnumMember]
        LogicalDeviceIsUnavailable = 2,
        [EnumMember]
        AgentIsDown = 3,
        [EnumMember]
        PoolIsNotExist =4,
        [EnumMember]
        AgentIsNotExist = 5,
        [EnumMember]
        IndexDeviceIsNotConfiged = 6,
        [EnumMember]
        LisenceIsUnAailable = 7,
        [EnumMember]
        SiteMasterInfoIsNull = 8,
        [EnumMember]
        CrawlProfileNotExist =9,
        [EnumMember]
        CrawlProfileIsNotConfiged = 10,
        [EnumMember]
        CrawlIndexNotExist = 11,
        [EnumMember]
        NoCrawlIndexData = 12
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EndUserViewOption
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        LisenceInfo = 1,
        [EnumMember]
        MediaInfo = 2,
        [EnumMember]
        IndexDeviceInfo = 4,
        [EnumMember]
        LogicalDeviceInfo = 8,
        [EnumMember]
        SecurityProfileInfo = 16
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EndUserRestoreOption
    {
        [EnumMember]
        OverWrite,
        [EnumMember]
        NotOverWrite
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserRestoreJobConfig
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public List<AdvanceSearchResult> Items { get; set; }
        [DataMember]
        public string RunJobUser { get; set; }
        [DataMember]
        public string RestoreStorage { get; set; }
        [DataMember]
        public CheckPermissionType PermissionCheckType { get; set; }
        [DataMember]
        public string GroupID { get; set; }
        [DataMember]
        public string Mail { get; set; }
        [DataMember]
        public ArchiveIntegrationModules IntegrationModule { get; set; }
        [DataMember]
        public RestoreType RestoreType { get; set; }
        [DataMember]
        public string StubType { get; set; }
        [DataMember]
        public string OopStubUrl { get; set; }
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public string O365TenantId { get; set; }
        [DataMember]
        public string AppProfileId { get; set; }
        [DataMember]
        public string SiteAdminUrl { get; set; }
        [DataMember]
        public bool IsExportJob { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ArchiveIntegrationModules
    {
        [EnumMember]
        None,
        [EnumMember]
        Recenter,
        [EnumMember]
        Records
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserRestoreItem
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string PathMD5 { get; set; }
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public string IndexString { get; set; }
        public string TreeNode { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckPermissionType
    {
        [EnumMember]
        None,
        [EnumMember]
        SharePointSite,
        [EnumMember]
        StubRestoreLink,
        [EnumMember]
        GroupOrTeams,
    }

}
