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
using AvePoint.GCommon.Contract.Replicator.Object.Message;
using AvePoint.GCommon.Contract.Replicator.Object.ProfileContents;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object
{
    /// <summary>
    /// Replicator模块，Agent与Manager或者Agent交互的消息。
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorMessage: AveMessage 
    {
        [DataMember]
        public ReplicatorMessageType MessageType { get; set; }

        /// <summary>
        /// 请将属性写到Context中，不要在这里添加类成员，本类暂时先固定两个成员。
        /// </summary>
        [DataMember]
        public ReplicatorContext Context { get; set; }
    }

    /// <summary>
    /// Replicator消息类型，有Schedule Job, UpdateGlobalMapping, Offline等
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorMessageType
    {
        #region -- Manager --
        [EnumMember]
        ReplicatorJob,
        [EnumMember]
        ReplicatorQuickJob,
        [EnumMember]
        ReplicatorResumeJob,
        [EnumMember]
        UpdateGlobalMappings,
        [EnumMember]
        AcquireConfigDBInfo,
        [EnumMember]
        SiteDeletion,
        [EnumMember]
        ConfigDB,
        [EnumMember]
        TestConfigDB,

        [EnumMember]
        JobStop,

        [EnumMember]
        JobPause,

        [EnumMember]
        JobStart,
        #endregion

        #region -- Agent --
        [EnumMember]
        CheckAndStartSecondary,
        [EnumMember]
        StartListenerHost,
        [EnumMember]
        StartAnalyzerHost,
        #endregion
    }

    /// <summary>
    /// Replicator消息具体内容，把公共的放到一起，offline和Schedule区分开。
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorContext
    {
        #region -- Common Properties --
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public ServiceDto Agent { get; set; }
        /// <summary>
        /// 用来判断是Test Run，还是Run
        /// </summary>
        [DataMember]
        public ReplicatorRunOption RunOption { get; set; }
        /// <summary>
        /// 是Full还是incremental
        /// </summary>
        [DataMember]
        public ReplicatorRunLevel RunLevel { get; set; }

        [DataMember]
        public ReplicatorJobCategory JobType { get; set; }
        #endregion

        #region -- Replicator Job --
        /// <summary>
        /// 是否转移Modified的content。
        /// </summary>
        [DataMember]
        public bool IsReplicatorModify { get; set; }
        /// <summary>
        /// 是否同步删除。
        /// </summary>
        [DataMember]
        public bool IsReplicatorDeletion { get; set; }
        /// <summary>
        /// 是否使用指定时间来进行incremental job。
        /// </summary>
        [DataMember]
        public bool IsUsingSpecialTime { get; set; }
        [DataMember]
        public int SpecialTimeNumber { get; set; }
        [DataMember]
        public TimeUnit SpecialTimeUnit { get; set; }
        /// <summary>
        /// 保证该集合的Mapping Id是按照Order的顺序。
        /// </summary>
        [DataMember]
        public List<string> MappingRefs { get; set; }

        [DataMember]
        public List<string> ExpiredMappingList { get; set; }
        [DataMember]
        public Dictionary<string, InvalidMappingType> InvalidMappings { get; set; }
        #endregion
       
        #region Config Database
        [DataMember]
        public List<ServiceDto> Agents { get; set; }

        [DataMember]
        public bool RemoteFarm { get; set; }
        #endregion

        #region -- Offline --

        #endregion

        #region -- UTC --
        [DataMember]
        public ReplicatorAgentUTCConfig SourceAgentUTCConfig { get; set; }
        #endregion
        [DataMember]
        public bool IsResendMessage { get; set; }

        [DataMember]
        public GlobalMappings GlobalMapping { get; set; }

        #region -- Replicator Cache Database --
        [DataMember]
        public ReplicatorConfigDBContent OldReplicatorDB { get; set; }

        [DataMember]
        public ReplicatorConfigDBContent NewReplicatorDB { get; set; }
        #endregion

        #region super user
        [DataMember]
        public Dictionary<string, SuperUserConfigurationDto> SuperUserConfigurationSiteUrlMappings { get; set; }
        #endregion
    }

    [DataContract(Namespace=ContractConstants.Namespace)]
    public enum InvalidMappingType
    {
        [EnumMember]
        Default=0,
        [EnumMember]
        BackUpFailed = 1,
        [EnumMember]
        LicensceOutDate = 100,
        [EnumMember]
        SCLevelNodeNotExists = 1000,
    }
    /// <summary>
    /// job的启动形式，Full还是incrmental
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorRunLevel : int
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental = 1,
    }

    /// <summary>
    /// Agent端UTC时间配置
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorAgentUTCConfig
    {
        [DataMember]
        public bool Enable { get; set; }

        [DataMember]
        public DateTime UTCTime { get; set; }

        [DataMember]
        public TimeSpan Interval { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorMessageReturnResult
    {
        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public string ErrorDetails { get; set; }

        [DataMember]
        public ReplicatorMessageExceptionType ExceptionType { get; set; }

        [DataMember]
        public ReplicatorMessageReturnContent ReturnContent { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorMessageExceptionType : byte
    {
        [EnumMember]
        NoIssue,
        [EnumMember]
        UTC,
        [EnumMember]
        StartProcessFailed,
        [EnumMember]
        InitWCFServiceFailed,
        [EnumMember]
        GetResponseTimeout,
        [EnumMember]
        QueryCacheDBInfoFailed,
        [EnumMember]
        VerifyCacheDBFailed,
        [EnumMember]
        WrapperAveMessageFailed,
        [EnumMember]
        SendMessageFailed,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorMessageReturnContent
    {
        #region -- Replicator Cache Database --
        [DataMember]
        public string DBServer { get; set; }

        [DataMember]
        public string DBName { get; set; }

        [DataMember]
        public DatabaseCredentials DbCredentials { get; set; }

        [DataMember]
        public bool DBCreatedSuccessfully { get; set; }
        #endregion

    }

}
