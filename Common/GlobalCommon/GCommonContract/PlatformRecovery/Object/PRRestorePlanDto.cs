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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestorePlanDto : PlanDto
    {
        /// <summary>int值对应模块中的类型:
        /// 0-PlatformBackup.PR_INPLACE_LEVEL_RESTORE(DB级别)
        /// 3-PlatformBackup.PR_OUTOFPLACE_LEVEL_RESTORE(DB级别)
        /// 1-PR_ITEM_INPLACE_LEVEL_RESTORE(Item级别)
        /// 2-PR_ITEM_OUTOFPLACE_LEVEL_RESTORE(Item级别)
        /// 4-PR_SSP_INPLACE_LEVEL_RESTORE(SSP级别)
        /// 5-PR_SSP_OUTOFPLACE_LEVEL_RESTORE(SSP级别)
        /// 6-PR_WFE_INPLACE_LEVEL_RESTORE(WFE级别)
        /// 7-PR_WFE_OUTOFPLACE_LEVEL_RESTORE(WFE级别)
        /// 8-RESTORE_RAW_DATABASE
        /// </summary>
        [DataMember]
        public int RestoreType { get; set; }
        [DataMember]
        public bool RestoreDBToMostRecentState { get; set; }
        [DataMember]
        public bool RestoreDBOnly { get; set; }
        [DataMember]
        public bool SafeRestore { get; set; }
        [DataMember]
        public bool OverWrite { get; set; }
        [DataMember]
        public bool ConnectToRestoredFarmAuto { get; set; }
        [DataMember]
        public PRBackupMethod MethodType { get; set; }
        [DataMember]
        public string WfeRestore { get; set; }
        [DataMember]
        public bool IncludeRecycleBinData { get; set; }
        [DataMember]
        public bool OnlyOneJob { get; set; }
        [DataMember]
        public string BackupJobId { get; set; }
        [DataMember]
        public string BackupPlanId { get; set; }
        [DataMember]
        public string DestFarmId { get; set; }
        [DataMember]
        public RestoreOption RestoreOption { get; set; }
        [DataMember]
        public PRItemMessage ItemMessage { get; set; }
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }
        [DataMember]
        public bool UseVDB { get; set; }
        [DataMember]
        public LanguagePair LanguageMapping { get; set; }
        [DataMember]
        public ActionOption Action { get; set; }
        [DataMember]
        public ServiceGroupDto AgentGroup { get; set; }
        [DataMember]
        public bool IsRestoreFromAlternateLocation { get; set; }
        /// <summary>
        /// ITEM级别还原,tree上面的级别显示
        /// </summary>
        [DataMember]
        public BackupLevel IndexLevel { get; set; }
        /// <summary>存放RawDB选择的sql agent</summary>
        [DataMember]
        public string RawDBAgentID { get; set; }
        /// <summary>
        /// Restore Front-end File Security
        /// </summary>
        [DataMember]
        public bool IsRestoreSecurity { get; set; }
        /// <summary>
        /// wfe oop 选中节点全路径
        /// </summary>
        [DataMember]
        public string OOPPath { get; set; }
        /// <summary>
        /// 标识是否为IIS还原
        /// </summary>
        [DataMember]
        public bool IsIISRestore { get; set; }
        /// <summary>
        /// 存放wfe oop还原时选中的agent对象的id属性
        /// </summary>
        [DataMember]
        public string WfeOOPAgentId { get; set; }
        /// <summary>
        /// 存放DBRestore还是DetailRestore标记 
        /// </summary>
        [DataMember]
        public bool IsDBRestore { get; set; }
        /// <summary>
        /// 存储数据地址设置
        /// </summary>
        [DataMember]
        public StoragePolicyDto StoragePolicy { get; set; }
        /// <summary>
        /// SSASetting oop 界面控件Restore the Exported Federated Locations选项
        /// </summary>
        [DataMember]
        public bool ExportedFederatedLocations { get; set; }
        /// <summary>
        /// 支持smsp的属性集合对象
        /// </summary>
        [DataMember]
        public PRSNRestoreInfoDto SNRestoreInfo { get; set; }
        /// <summary>
        /// Farm Rebuild
        /// </summary>
        [DataMember]
        public FarmRebuildInfo FarmRebuildInfo { get;set;}
        /// <summary>
        /// Restore From Alternate Location
        /// </summary>
        [DataMember]
        public RestoreFromAlternateLocationInfo RestoreFromAlternateLocationInfo { get; set; }
        [DataMember]
        public PRPlatformType PlatformType { get; set; }
        [DataMember]
        public string NotificationId { get; set; }

        #region == Mapping setting ==
        /// <summary> 用户选择的Language Mapping setting Id </summary>
        [DataMember]
        public string LanguageMappingSettingId { get; set; }
        /// <summary> 用户选择的User Mapping setting Id </summary>
        [DataMember]
        public string UserMappingSettingId { get; set; }
        /// <summary>用户选择的Domain Mapping setting Id</summary>
        [DataMember]
        public string DomainMappingSettingId { get; set; }
        #endregion ==
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionOption
    {
        [EnumMember]
        Attach = 0,
        [EnumMember]
        Merge = 1,
    }
    
    /// <summary>
    /// 还原选项类型
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreOption
    {
        [EnumMember]
        NotOverwrite = 0,
        [EnumMember]
        Overwrite = 2,
        [EnumMember]
        Append = 1,
        [EnumMember]
        Replace = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DataSelection 
    {
        [EnumMember]
        UnknownLevel = 0,
        [EnumMember]
        DBLevel = 1,
        [EnumMember]
        ItemLevel = 2,
        [EnumMember]
        SSPLevel = 3,
        [EnumMember]
        WFELevel = 4,
        [EnumMember]
        SearchServiceSettingLevel = 5,
        [EnumMember]
        BlobLevel = 6,
    }
    /// <summary>
    /// 用于CEIP，与plan上的restore type不同
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreType
    {
        [EnumMember]
        InPlaceRestore = 0,
        [EnumMember]
        OutOfPlaceRestore = 1,
        [EnumMember]
        RawDBRestore = 3,
        [EnumMember]
        FarmRebuildNormalRestore = 4,
        [EnumMember]
        FarmRebuildRestoreFromAlternateLocation = 5,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRRestoreVersionSetting
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        MajorAndMinor = 1,
        [EnumMember]
        MajorOnly = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FarmRebuildInfo
    {
        [DataMember]
        public List<ServerInfo> ServerList { get; set; }
        [DataMember]
        public List<ComponentInfos> ComponentInfoList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RestoreFromAlternateLocationInfo
    {
        [DataMember]
        public List<PRManuallyResult> ManuallyResultList { get; set; }
    }

}
