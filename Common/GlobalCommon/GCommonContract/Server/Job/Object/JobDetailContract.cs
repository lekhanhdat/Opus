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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobDetail
    {

        [DataMember]
        public long Date { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string SrcURL { get; set; }
        [DataMember]
        public string DestURL { get; set; }
        /// <summary>
        /// 用来标识URL object name，这样可以方便用户快速定位object
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string PhysicalDevice { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        [Obsolete("该属性不再提倡使用，请用SrcAgentHost")]
        public string Host { get; set; }
        [DataMember]
        public string SrcAgentHost { get; set; }
        [DataMember]
        public string DestAgentHost { get; set; }
        [DataMember]
        public string MediaHost { get; set; }
        [DataMember]
        public long Size { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string Operator { get; set; }
        [DataMember]
        public string Option { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        /// <summary>
        /// EntityType  : 0 Normal Info
        ///             : 2 Error Message
        ///             : 3 Delete Report
        ///             : 4 Archive Deletion
        /// </summary>
        [DataMember]
        public int EntityType { get; set; }
        [DataMember]
        public long Remark1 { get; set; }
        [DataMember]
        public long Remark2 { get; set; }

        /// <summary>
        /// Import data job module: Cycle | PR Backup job [Index status]
        /// </summary>
        [DataMember]
        public string Remark3 { get; set; }

        /// <summary>
        /// Import data job module: Farm | PR Backup job [Verify status](netapp)
        /// </summary>
        [DataMember]
        public string Remark4 { get; set; }

        /// <summary>
        /// Import data job module: Storage Policy | PR restore job [Alternate Location](netapp)
        /// </summary>
        [DataMember]
        public string Remark5 { get; set; }

        /// <summary>
        /// Import data job module: Logical Device| PR farm rebuild job [Action]
        /// </summary>
        [DataMember]
        public string Remark6 { get; set; }
        /// <summary>
        /// PR farm rebuild job [Object Name]
        /// </summary>
        [DataMember]
        public string Remark7 { get; set; }
        /// <summary>
        /// PR Migrator job [Source Location]
        /// </summary>
        [DataMember]
        public string Remark8 { get; set; }
        /// <summary>
        /// PR Migrator job [Destination Location]
        /// </summary>
        [DataMember]
        public string Remark9 { get; set; }
        [DataMember]
        public long Remark10 { get; set; }

        /// <summary>
        /// comment 为多个key的情况
        /// </summary>
        [DataMember]
        public List<PropertyItem> PropertyItems { get; set; }

        /// <summary>
        /// SP Migration   
        /// </summary>
        [DataMember]
        public string Remark11 { get; set; }

        [DataMember]
        public string Remark12 { get; set; }

        [DataMember]
        public string Remark13 { get; set; }

        /// <summary>
        /// key:属性字段，Value对应属性Message可能带的参数
        /// </summary>
        [Obsolete]
        [DataMember]
        public Dictionary<ParamKey, object[]> Args { get; set; }
    }

    /// <summary>
    /// Compare JobDetail
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CompareJobDetail
    {
        [DataMember]
        public string Type
        {
            get;
            set;
        }
        [DataMember]
        public string Name
        {
            get;
            set;
        }
        [DataMember]
        public string PrimaryUrl
        {
            get;
            set;
        }
        [DataMember]
        public string SecondaryUrl
        {
            get;
            set;
        }
        [DataMember]
        public string PrimarySiteTitle
        {
            get;
            set;
        }
        [DataMember]
        public string SecondarySiteTitle
        {
            get;
            set;
        }
        [DataMember]
        public string ListTitle
        {
            get;
            set;
        }
        [DataMember]
        public string Message
        {
            get;
            set;
        }
        [DataMember]
        public string CompareResult
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Check App
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppCheckJobDetail
    {
        /// <summary>
        /// 状态 默认值Successful
        /// </summary>
        [DataMember]
        public string Status { get; set; }
        /// <summary>
        /// AppName
        /// </summary>
        [DataMember]
        public string AppName { get; set; }
        /// <summary>
        /// App所在的Site Url
        /// </summary>
        [DataMember]
        public string RelatedSiteUrl { get; set; }
        /// <summary>
        /// 当前App Version
        /// </summary>
        [DataMember]
        public string CurrentVersion { get; set; }
        /// <summary>
        /// Farm Name
        /// </summary>
        [DataMember]
        public string FarmName { get; set; }
        /// <summary>
        /// 是否可以升级
        /// </summary>
        [DataMember]
        public string CanUpdata { get; set; }
        /// <summary>
        /// Comment
        /// </summary>
        [DataMember]
        public string Message { get; set; }
        /// <summary>
        /// Option Successful,Failed,Skip...
        /// </summary>
        [DataMember]
        public string Option { get; set; }
    }

    /// <summary>
    /// Upgrade App
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AppUpgradeJobDetail
    {
        /// <summary>
        /// Upgrade Status.Successful，Failed 
        /// </summary>
        [DataMember]
        public string Status { get; set; }
        /// <summary>
        /// App Name
        /// </summary>
        [DataMember]
        public string AppName { get; set; }
        /// <summary>
        /// App 所在的Site Url
        /// </summary>
        [DataMember]
        public string RelatedSiteUrl { get; set; }
        /// <summary>
        /// 当前Version
        /// </summary>
        [DataMember]
        public string CurrentVersion { get; set; }
        /// <summary>
        /// Comment
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionJobDetail
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public int Option { get; set; }
        [DataMember]
        public bool IsFarmSolution { get; set; }
        [DataMember]
        public string SourceHostName { get; set; }
        [DataMember]
        public string DestinationHostName { get; set; }
        [DataMember]
        public string SolutionName { get; set; }
        [DataMember]
        public string SolutionID { get; set; }
        [DataMember]
        public int Operation { get; set; }
        [DataMember]
        public string FeatureName { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public int EntityType { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public long Size { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchJobDetail
    {

        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public string Mailbox { get; set; }
        [DataMember]
        public int ItemCount { get; set; }
        [DataMember]
        public JobReportDetailStatus Status { get; set; }
        [DataMember]
        public DateTime FinishTime { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Content { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SolutionJobDetailOption : int
    {
        [EnumMember]
        Skip = 0,
        [EnumMember]
        Retract = 1,
        [EnumMember]
        RetractAndRedeploy = 2,
        [EnumMember]
        Remove = 3,
        [EnumMember]
        Active = 4,
        [EnumMember]
        DeActive = 5,
        [EnumMember]
        DeployToMedia = 6,
        [EnumMember]
        RemoveSolutionVersion = 7,
        [EnumMember]
        Upgrade = 8,
        /// 以下3个用于solution中Operation列
        [EnumMember]
        DeployFromMedia = 9,
        [EnumMember]
        DeployFromDisk = 10,
        [EnumMember]
        Deploy = 11,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobReportDetailStatus : int
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Skipped = 2,
        [EnumMember]
        Filtered = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobReportDetailEntityType : int
    {
        //common
        [EnumMember]
        NormalInfo = 0,


        [EnumMember]
        ErrorMessage = 2,
        [EnumMember]
        DeleteReport = 3,
        [EnumMember]
        ArchiveDeletion = 4,
        [EnumMember]
        Export = 5, //add for RevIM export report
        [EnumMember]
        RecordManager = 6,
        [EnumMember]
        Tag = 7,
        [EnumMember]
        Objects = 10,
        [EnumMember]
        Configuration = 11,
        [EnumMember]
        Security = 12,
        [EnumMember]
        EmailAddress = 13,
        [EnumMember]
        FileRetention = 14,
        [EnumMember]
        RemoveStub = 15,
        [EnumMember]
        ChangeFileTier = 16,
        [EnumMember]
        FileDedup = 17,
        [EnumMember]
        FileDedupReport = 18,

        [EnumMember]
        JobSettings = 95
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportOption : int
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        NotOverwrite = 0,
        [EnumMember]
        Overwrite = 1,
        [EnumMember]
        OverWriteIfNewer = 2,
        [EnumMember]
        Merge = 3,
        [EnumMember]
        Replace = 4,
        [EnumMember]
        Append = 5,
        [EnumMember]
        NewCreated = 6,
        [EnumMember]
        Skip = 7,
        [EnumMember]
        Failed = 8
    }

    /// <summary>
    /// 目前支持的国际化可以带参数的列
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ParamKey
    {
        //标记对应JobDetail Message列
        [EnumMember]
        Message = 0,

        //标记对应JobSummary Value列
        [EnumMember]
        Value = 1,

        //标记对应JobDetail Remark11列
        [EnumMember]
        Remark11 = 2,
    }

    [XmlRoot("Property")]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PropertyItem
    {
        //属性类型Comment etc..
        [DataMember]
        public ParamKey PropertyType { get; set; }
        //国际化key,  如ContentManager_ErrorMessageKey
        [DataMember]
        public string Key { get; set; }
        //参数
        [DataMember]
        public object[] Args { get; set; }

        /// <summary>
        /// 正确的国际化词条，一旦通过key取不到国际化词条时，将默认显示该值
        /// </summary>
        [DataMember]
        public string DefaultValue { get; set; }
    }
}
