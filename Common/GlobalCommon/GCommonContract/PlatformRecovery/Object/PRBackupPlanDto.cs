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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    /// <summary>
    /// GUI use only,not for Agent
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(PRTreeNodeDto))]
    public class PRBackupPlanDto : PlanDto
    {
        /// <summary>
        /// 存储数据地址设置
        /// </summary>
        [DataMember]
        public StoragePolicyDto StoragePolicy { get; set; }
        /// <summary>
        /// plan类型和备份级别(备份级别和模块枚举的或值)
        /// </summary>
        [DataMember]
        public PRBackupPlanType PRPlanType { get; set; }

        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }

        [DataMember]
        public DataSecurity DataSecurity { get; set; }

        [DataMember]
        public CompressionType CompressionType { get; set; }

        [DataMember]
        public List<string> Notifications { get; set; }

        [DataMember]
        public DateTime ModifyTime { get; set; }

        [DataMember]
        public string SecurityPolicyName { get; set; }

        [DataMember]
        public string ClientPassword { get; set; }

        [DataMember]
        public string HostFullName { get; set; }

        [DataMember]
        public List<string> AssocitedPlanGroups { get; set; }

        /// <summary>
        /// VSS, VDI
        /// </summary>
        [DataMember]
        public PRBackupMethod BackupMethod { get; set; }

        /// <summary>
        /// full,diff,inc
        /// </summary>
        [DataMember]
        public PRBackupType BackupOption { get; set; }

        [DataMember]
        public bool IsPersistSnapshot { get; set; }

        [DataMember]
        public bool LogBackup { get; set; }

        [DataMember]
        public bool CopyOnly { get; set; }

        [DataMember]
        public bool GenerateVDBMapping { get; set; }

        /// <summary>
        /// 选中为true
        /// </summary>
        [DataMember]
        public bool IsBackupImmediately { get; set; }

        /// <summary>
        /// 对应planManager界面的Plan Template控件
        /// </summary>
        [DataMember]
        public bool IsPlanTemplate { get; set; }

        /// <summary>
        /// 对应4.1Snapshot Rentention界面输入框控件
        /// </summary>
        [DataMember]
        public int SnapshotLimitNum { get; set; }

        /// <summary>
        /// 对应4.1Snapshot Rentention界面单选框控件
        /// </summary>
        [DataMember]
        public SnapshotRententionOption SnapshotOption { get; set; }

        /// <summary>
        /// 对应overview界面的save current plan as plan template输入框控件
        /// </summary>
        [DataMember]
        public string TemplatePlanName { get; set; }

        /// <summary>
        /// maintenance界面option选项copy snapshot data to media server for last ? backups
        /// </summary>
        [DataMember]
        public string CopySnapshotNum { get; set; }

        /// <summary>
        /// maintenance界面option选项Generate virtual database mapping for last ? backups
        /// </summary>
        [DataMember]
        public string GenerateVDBNum { get; set; }

        /// <summary>
        /// maintenance界面option选项Generate granular restore index for last ? backups
        /// </summary>
        [DataMember]
        public string GenerateIndexNum { get; set; }

        /// <summary>
        /// 标识tree的选中节点是否改变
        /// </summary>
        [DataMember]
        public bool IsTreeNodeChanged { get; set; }
        [DataMember]
        public PRBackupPlanDto PRBackupMaintenancePlan { get; set; }
        [DataMember]
        public string CurrentBackupPlanID { get; set; }
        // 加密字符GUID
        [DataMember]
        public string ProfileGuid { get; set; }

        [DataMember]
        public PRPlatformType PlatformType { get; set; }

        [DataMember]
        public string NotificationId { get; set; }
        #region SMSP
        // 存放smsp相关的maintenance信息
        [DataMember]
        public PRSNMaintenanceOptionDto MaintenanceOption { get; set; }
        //// 存放smsp备份信息
        //[DataMember]
        //public SMSPPRBackupInfoDto SMSPPRBackupInfoDto { get; set; }
        #endregion
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRBackupPlanType
    {
        [EnumMember]
        None = 8,   //同枚举PRBackupLevel中None
        [EnumMember]
        Database = 0, //同枚举PRBackupLevel中Database
        [EnumMember]
        SiteCollectionLevel = 16,
        [EnumMember]
        SiteLevel = 32,
        [EnumMember]
        FolderLevel = 64,
        [EnumMember]
        ItemLevel = 128,
        [EnumMember]
        ItemVersionLevel = 256,

        [EnumMember]
        QuickBackupDefaultSetting = 1,
        [EnumMember]
        QuickBackupPlan = 2,
        [EnumMember]
        BackupPlanBuilder = 4
    }
    
    [DataContract(Namespace = ContractConstants.Namespace)]
     public enum SnapshotRententionOption
     {
         [EnumMember]
         DirectlyFail = 0,
         [EnumMember]
         ScheduleOption = 1,
         [EnumMember]
         FailNew = 2
     }
}