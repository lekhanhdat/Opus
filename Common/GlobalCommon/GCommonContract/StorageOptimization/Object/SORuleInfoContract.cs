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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// SO Rules 页面用到的Contract
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SORuleInfoContract : SONodeContract<SORuleInfoContract>
    {
        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public string ProfileName { set; get; }

        [DataMember]
        public FarmDto FarmDto { set; get; }

        [DataMember]
        public string Description { set; get; }

        /// <summary>
        /// 设置过rules，realtime，scheduled，archiver中的一种
        /// </summary>
        [DataMember]
        public List<Rule> Rules { set; get; }

        [DataMember]
        public Dictionary<Guid, List<Guid>> TermRuleMapping { get; set; }

        [DataMember]
        public RecordsStorageInfo RecordsStorageInfo { set; get; }
        [DataMember]
        public int SourceFlag { set; get; }

        /// <summary>
        /// save settings for scheduled and archiver
        /// </summary>
        [DataMember]
        public SOPlan Plan { set; get; }

        /// <summary>
        /// realtime中rule的继承情况,GUI根据此枚举显示StopInherit或Inherit
        /// </summary>
        [DataMember]
        public RuleNodeStatus RuleNodeStatus { get; set; }

        [DataMember]
        public ChooseFunction Function { get; set; }

        [DataMember]
        public ApplyInfo NodeInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RecordsStorageInfo
    {
        public DataSecurity ArchiverDataSecurity { get; set; }
        public CompressionType ArchiverCompressionType { get; set; }
        public String DataEncryptionProfileId { get; set; }
        public String DataEncryptionProfileName { get; set; }
        public string StoragePolicyId { get; set; }
        public string StoragePolicyName { get; set; }
        public string ExportLocationId { set; get; }
        public string ExportLocationName { set; get; }
        public byte[] FileVEO { get; set; }
        public byte[] RecordVEO { get; set; }
        public byte[] ManifestVEO { get; set; }
        public byte[] NAAConfigFile { get; set; }
        public byte[] NARAConfigFile { get; set; }
        public string ExportDataEncryptionKey { get; set; }
        public string ExportDataEncryptionIV { get; set; }
        public ArchiverSetting ArchiverSetting { get; set; }
        public ArchiverVEOSetting ArchiverVEOSetting { get; set; }
        public List<StubSettingDto> StubTemplatesList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SORuleCollection
    {
        [DataMember]
        public SORuleInfoContract RealTimeRule { get; set; }
        [DataMember]
        public SORuleInfoContract ScheduledRule { get; set; }
        [DataMember]
        public SORuleInfoContract ArchiverRule { get; set; }
        [DataMember]
        public ConnectorInfoDto ConnectorRule { get; set; }
    }

    /// <summary>
    /// 此类用于向GUI返回页面需要的初始化数据和设置过的setting信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SORulesAndSettings
    {
         /// <summary>
         /// 是否安装stub db
         /// </summary>
         [DataMember]
         public bool IsInstallStubDB { set; get; }

         /// <summary>
         /// archiver判断是否config index device.
         /// </summary>
         [DataMember]
         public bool IsConfigIndexDevice { set; get; }

        [DataMember]
         public bool isConfigArchiverDB { set; get; }
         /// <summary>
         /// 此node的provider type
         /// </summary>
         [DataMember]
         public BlobProviderType ProviderType { set; get; }

         /// <summary>
         /// 所有logical device，realtime和scheduled供GUI使用
         /// </summary>
         [DataMember]
         public List<LogicalDeviceDto> LogicalDevices { set; get; }

         [DataMember]
         public List<StoragePolicyDto> StoragePolicies { get; set; }

         [DataMember]
         public List<ProcessingPoolContract> ProcessingPools { set; get; }

         /// <summary>
         /// 所有realtime rule供GUI使用
         /// </summary>
         [DataMember]
         public List<Rule> RealtimeRules { set; get; }

         /// <summary>
         /// 所有scheduled rule供GUI使用
         /// </summary>
         [DataMember]
         public List<Rule> ScheduledRules { set; get; }

         /// <summary>
         /// 所有archiver rule供GUI使用
         /// </summary>
         [DataMember]
         public List<Rule> ArchiverRules { set; get; }

         /// <summary>
         /// 包含了此node设置rule的情况
         /// </summary>
         [DataMember]
         public SORuleCollection SORuleCollection { get; set; }

         /// <summary>
         /// 所有的security profile供GUI使用
         /// </summary>
         [DataMember]
         public List<DataEncryptionProfile> DataEncryptionProfiles { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StubSettingDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int StubType { get; set; }
        public string StubContent { get; set; }
        public int StubCustomizeTags { get; set; }
        public bool IsDeclareStubAsRecords { get; set; }
        public string LastModifiedTime { get; set; }
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum ProfileCategory
    //{
    //    [EnumMember]
    //    None = 0,
    //    [EnumMember]
    //    GAPlus = 1,
    //    [EnumMember]
    //    DocAve = 2
    //}


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ChooseFunction
    {
        [EnumMember]
        RunNow = 0,
        [EnumMember]
        TestRun = 1,
        [EnumMember]
        ApplyandRun = 2,
        [EnumMember]
        ApplyandTest = 3
    }
  
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleStatus
    { 
        [EnumMember]
        None = 0,
        [EnumMember]
        Enabled = 1,
        [EnumMember]
        Disabled = 2
    }

    /// <summary>
    /// current node与parent node rule的apply情况
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleNodeStatus
    {
        /// <summary>
        /// current no and parent no
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// current yes and parent no
        /// </summary>
        [EnumMember]
        Self = 1,
        /// <summary>
        /// current no and parent yes
        /// </summary>
        [EnumMember]
        Inherited = 2,
        /// <summary>
        /// current yes and parent yes
        /// </summary>
        [EnumMember]
        Individual = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleAllianceContract
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string RuleId { get; set; }
        [DataMember]
        public RuleAllianceType Type { get; set; }
        [DataMember]
        public int Active { get; set; }
        /// <summary>
        /// 用于表示GUI页面上rule是否勾选
        /// </summary>
        [DataMember]
        public ActionStatus CheckStatus { get; set; }
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// mark rule by provider type for realtime.
        /// </summary>
        [DataMember]
        public BlobProviderType ProviderType { get; set; }
        /// <summary>
        /// 临时属性,不存数据库, 只为Archiver Rule排序用, 减少比较时查DB的开销.
        /// </summary>
        [DataMember]
        public PolicyLevel PolicyLevel { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleAllianceType
    {
        [EnumMember]
        RealTime = 0,
        [EnumMember]
        Scheduled = 1,
        [EnumMember]
        Archiver = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleAllianceColumn
    {
        [EnumMember]
        Active = 0
    }
}
