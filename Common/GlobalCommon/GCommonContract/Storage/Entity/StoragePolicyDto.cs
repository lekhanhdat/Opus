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
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StoragePolicyDto
    {
        //        private string id;
        //        private string name;
        //        private string description;
        private LogicalDeviceDto primaryStorage;
        //        private int retentionPolicyType;
        //        private long modifyTime;
        //        private int isOldRecord;
        private string primaryLogicalId;

        #region == 这部分属性各个功能修改以后会删除掉 ==

        #endregion

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public LogicalDeviceDto PrimaryStorage  //Logical Device
        {
            get
            {
                if (this.primaryStorage == null)
                {
                    this.primaryStorage = new LogicalDeviceDto();
                }
                return this.primaryStorage;
            }
            set
            {
                this.primaryStorage = value;
            }
        }

        [DataMember]
        public int RetentionPolicyType { get; set; }  //Retention Policy Type
        [DataMember]
        public long ModifyTime { get; set; }  //Storage Policy 的修改时间。
        [DataMember]
        public int IsOldRecord { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string PrimaryLogicalId
        {
            get
            {
                return this.primaryLogicalId;
            }
            set
            {
                this.primaryLogicalId = value;
            }
        }
        [DataMember]
        public float FreeSpaceAvailable { set; get; } //Storage Policy的剩余总空间。
        [DataMember]
        public float TotleSpace { set; get; } //存储Storage Policy磁盘总空间的大小。

        #region == 这部分属性各个功能修改以后会删除掉 ==
        [DataMember]
        public List<TimeBasedPolicy> TimeBasedPolicys { get; set; }
        [DataMember]
        public List<EventBasedPolicy> EventBasedPolicys { get; set; }
        [DataMember]
        public List<CycleBasedPolicy> CycleBasedPolicys { get; set; }
        #endregion

        [DataMember]
        public RetentionRuleOption RetentionOption { get; set; }

        [DataMember]
        public FullTextIndexSettingDto FullTextIndexSetting { get; set; }
        [DataMember]
        public NotificationDto Notification { get; set; }
        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }
        [DataMember]
        public string NotificationId { get; set; }
        /// <summary>
        /// GUI用来判断当前的Storage Policy是否本选中，该属性只在页面中使用
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public string BackupStoragePolicyId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetentionRuleOption
    {
        /// <summary>
        /// 对应页面中的Storage Policy Type
        /// </summary>
        [DataMember]
        public StoragePolicyType StorageType { get; set; }

        [DataMember]
        public StoragePolicyLicenseType LicType { get; set; }

        /// <summary>
        /// 对应界面中Setup Data Retention选项，标示Retention设置是否生效。默认false，不生效
        /// </summary>
        [DataMember]
        public bool SetupDataRetention { set; get; }
        [DataMember]
        public List<BackupRetentionRule> BackupRetentionRules { get; set; }
        [DataMember]
        public List<ArchiveRetentionRule> ArchiveRetentionRules { get; set; }

        /// <summary>
        /// 选择Archive Type时需要设置的Schedule
        /// </summary>
        [DataMember]
        public ScheduleDto Schedule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StroagePolicyContentDto
    {
        [DataMember]
        public bool DeleteTheData { set; get; }  //Delete the data
        [DataMember]
        public bool RemoveTheJob { set; get; }  //Remove The job
        [DataMember]
        public bool IsMove { set; get; }  //Move the data to Logical device
        //[DataMember]
        //public string MoveLogicalDeviceId { set; get; } //Move the data to Logical device id

        #region == 用户操作流控制 ==
        /// <summary>
        /// 对应界面中Setup Data Retention选项，标示Retention设置是否生效。默认false，不生效
        /// </summary>
        [DataMember]
        public bool SetupDataRetention { set; get; }

        /// <summary>
        /// MoveLogicalDeviceId对应的Logical Device的名称
        /// </summary>
        [DataMember]
        public string LogicalName { set; get; }

        /// <summary>
        /// Move the data to logical device,对应的ComboBox列表
        /// </summary>
        [DataMember]
        public List<LogicalDeviceDto> MoveToLogicalDtos { set; get; }

        /// <summary>
        /// Move the data to logical device,对应的ComboBox的选定值，在Edit时，该值不能被赋值，
        /// 否则会造成Primary Storage以外的Storage无法显示。
        /// </summary>
        [DataMember]
        public LogicalDeviceDto MoveLogicalDeviceDto { set; get; }

        /// <summary>
        /// Action 包含的Radio Button的Group Name
        /// </summary>
        [DataMember]
        public string ActionGroupName { set; get; }

        /// <summary>
        /// 显示Details时，子层级的标题
        /// </summary>
        [DataMember]
        public string MoToDeviceHeader { get; set; }

        /// <summary>
        /// 子层级的标题, 对应的描述
        /// </summary>
        [DataMember]
        public string LogicalDescription { set; get; }

        /// <summary>
        /// 验证的错误信息，GUI部门使用。
        /// </summary>
        [DataMember]
        public string ErrorMessage { set; get; }

        /// <summary>
        /// 控制AUIOverGrid标题的显示和隐藏状态
        /// </summary>
        [DataMember]
        public bool HeaderVisibility { set; get; }

        /// <summary>
        /// Retention对应的组标题
        /// </summary>
        [DataMember]
        public string GroupTitleHeader { get; set; }

        /// <summary>
        /// KelpExpandar的标题
        /// </summary>
        [DataMember]
        public string KelpTitle { get; set; }

        /// <summary>
        /// EnableRetention是否可见
        /// </summary>
        [DataMember]
        public bool EnableRetentionVisibility { get; set; }

        /// <summary>
        /// 当前Retention的序号
        /// </summary>
        [DataMember]
        public int RetentionIndex { get; set; }

        /// <summary>
        /// 用来标示是否可以进行Move the data to logical device 设置
        /// </summary>
        [DataMember]
        public bool CanSetMoveTo { get; set; }

        /// <summary>
        /// Move the data to logical device验证时，使用的提示语
        /// </summary>
        [DataMember]
        public string MoveToValidationMsg { get; set; }

        #endregion
    }

    #region == 这部分属性各个功能修改以后会删除掉 ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimeBasedPolicy : StroagePolicyContentDto
    {
        private bool afterBackup;
        private bool beforeBackup;
        [DataMember]
        public int KeepDataType { set; get; }
        [DataMember]
        public int KeepDataValue { set; get; }
        [DataMember]
        public bool BeforeBackup  //Before the job started
        {
            set
            {
                if (value)
                {
                    this.afterBackup = false;
                }
                beforeBackup = value;
            }
            get
            {
                return this.beforeBackup;
            }
        }
        [DataMember]
        public bool AfterBackup  //After the job completed
        {
            set
            {
                if (value)
                {
                    this.BeforeBackup = false;
                }
                this.afterBackup = value;
            }
            get
            {
                return this.afterBackup;
            }
        }
        [DataMember]
        public bool Complated { set; get; } //Complated
        [DataMember]
        public bool ComplatedWithException { set; get; } //Completed with Exception
        [DataMember]
        public string TriggerGroupName { set; get; }
        [DataMember]
        public AfterJobState JobState { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EventBasedPolicy : StroagePolicyContentDto
    {
        [DataMember]
        public int TriggerEvent { set; get; }  //Tigger Event
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CycleBasedPolicy : StroagePolicyContentDto
    {
        [DataMember]
        public bool FullBackup { set; get; } //Full Backup
        [DataMember]
        public bool IncrementalBackup { set; get; }  //incremental Backup
        [DataMember]
        public bool DifferentBackup { set; get; }  //Differential Backup
        [DataMember]
        public int KeepCycles { set; get; }  //FullBackup Cycle(s)
        [DataMember]
        public bool KeepBackupFailedJobs { set; get; }  //Keep backup data for failed jobs
    }

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupRetentionRule : StroagePolicyContentDto
    {
        /// <summary>
        /// 对应Cycle oriented rule选项
        /// </summary>
        [DataMember]
        public bool CycleOrientedRule { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles的Radio button
        /// </summary>
        [DataMember]
        public bool IsCheckKeepCycles { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles
        /// </summary>
        [DataMember]
        public int KeepCyclesValue { set; get; }

        /// <summary>
        /// 对应页面中设置间隔时间的Radio Button
        /// </summary>
        [DataMember]
        public bool IsCheckKeepDate { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles
        /// </summary>
        [DataMember]
        public int KeepCyclesDateValue { set; get; }

        /// <summary>
        /// 对应页面中的Cycle oriented rule处的时间单位
        /// </summary>
        [DataMember]
        public DateUnit CycleDateUnit { set; get; }

        /// <summary>
        /// 对应页面中Full Backup Oriented Rule选项
        /// </summary>
        [DataMember]
        public bool FullBackupOriented { set; get; }

        /// <summary>
        /// 对应Keep the last full backups的Radio button
        /// </summary>
        [DataMember]
        public bool IsCheckKeepBackupValue { set; get; }

        /// <summary>
        /// 对应页面中Keep the last full backups所设定的值
        /// </summary>
        [DataMember]
        public int KeepBackupValue { set; get; }

        /// <summary>
        /// 对应页面中设置Full backup oriented rule间隔时间的Radio Button
        /// </summary>
        [DataMember]
        public bool IsCheckKeepDateValue { set; get; }

        /// <summary>
        /// 对应页面中Keep The Backup Date的时间设置。
        /// </summary>
        [DataMember]
        public int KeepBackupDateValue { set; get; }

        /// <summary>
        /// 对应页面中的Full Backup Oriented Rule处的时间单位
        /// </summary>
        [DataMember]
        public DateUnit BackupDateUnit { set; get; }

        /// <summary>
        /// 对应页面中Before the job started
        /// </summary>
        [DataMember]
        public bool BeforeBackup { set; get; }  //Before the job started

        /// <summary>
        /// 对应页面中After the job completed
        /// </summary>
        [DataMember]
        public bool AfterBackup { set; get; }  //After the job completed

        /// <summary>
        /// 对应页面中Include the job completed with exception
        /// </summary>
        [DataMember]
        public bool ComplatedWithException { set; get; } //Completed with Exception

        // <summary>
        /// 对应页面中completed
        /// </summary>
        [DataMember]
        public bool Completed { get; set; }

        /// <summary>
        /// 对应页面中Keep backup data for failed jobs
        /// </summary>
        [DataMember]
        public bool KeepBackupFailedJobs { set; get; }  //Keep backup data for failed jobs

        /// <summary>
        /// Backup类型的Retention，Cycle Oriented Rule对应的GroupName
        /// </summary>
        [DataMember]
        public string CycleGroupName { set; get; }

        /// <summary>
        /// Backup类型的Retention，Full Backup Oriented Rule对应的GroupName
        /// </summary>
        [DataMember]
        public string FullBackupGroupName { set; get; }

        /// <summary>
        /// Keep the last cycles错误信息
        /// </summary>
        [DataMember]
        public string KeepCyclesValueErrorMessage { get; set; }

        /// <summary>
        /// Keep the last cycles错误信息
        /// </summary>
        [DataMember]
        public string KeepCyclesDateValueErrorMessage { get; set; }

        /// <summary>
        /// Keep the last full backups错误信息
        /// </summary>
        [DataMember]
        public string KeepBackupValueErrorMessage { get; set; }

        /// <summary>
        /// Keep The Backup Date错误信息
        /// </summary>
        [DataMember]
        public string KeepBackupDateValueErrorMessage { get; set; }

        [DataMember]
        public bool IsCustomAction { get; set; }  //

        [DataMember]
        public string CustomActionValue { get; set; }

        [DataMember]
        public string CustomDesc { get; set; }

        #region == 用户操作流控制 ==

        /// <summary>
        /// 获取或设置一个值，该值表示Advanced区域是展开还是折叠
        /// </summary>
        [DataMember]
        public bool IsAdvancedExpanded { get; set; }

        #endregion

        #region == PR SN retention
        [DataMember]
        public BackupManagementGroupType BackupManagementGroup { get; set; }
        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiveRetentionRule : StroagePolicyContentDto
    {
        [DataMember]
        public int KeepValue { get; set; }
        [DataMember]
        public DateUnit ArchiveDateUnit { get; set; }
        [DataMember]
        public string KeepValueErrorMessage { get; set; }
        [DataMember]
        public bool TakeEffectToExistingData { get; set; }


        //[DataMember]
        //public ScheduleDto Schedule { get; set; }
        //[DataMember]
        //public DateTime NotifyDay { get; set; }


        [DataMember]
        public bool ReceiveReportAboutRemovingData { get; set; }

        [DataMember]
        public int NotifyDaysPriorToRemoveData { get; set; }

        [DataMember]
        public ScheduleDto NotifySchedule { get; set; }
        [DataMember]
        public bool RemoveOrphanedStub { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StoragePolicyXmlDto
    {
        #region == 这部分属性各个功能修改以后会删除掉 ==
        public List<TimeBasedPolicy> TimeBasedPolicys { get; set; }
        [DataMember]
        public List<EventBasedPolicy> EventBasedPolicys { get; set; }
        [DataMember]
        public List<CycleBasedPolicy> CycleBasedPolicys { get; set; }
        [DataMember]
        public int RetentionPolicyType { get; set; }
        #endregion

        [DataMember]
        public StoragePolicyType StorageType { get; set; }
        [DataMember]
        public bool SetupDataRetention { set; get; }
        [DataMember]
        public List<BackupRetentionRule> BackupRetentionRules { get; set; }
        [DataMember]
        public List<ArchiveRetentionRule> ArchiveRetentionRules { get; set; }
    }

    /// <summary>
    /// 这个类用于存储显示统计各个某块所做数据信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StoragePolicyDetailDto
    {
        [DataMember]
        public int Category { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public double DataSize { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool PlanDeleted { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SnaplockResultDto
    {
        [DataMember]
        public bool IsShowSpanlock { get; set; }
        [DataMember]
        public List<SnaplockDto> Snaplocks { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SnaplockDto
    {
        [DataMember]
        public string Volume { get; set; }
        [DataMember]
        public string MinimumPeriod { get; set; }
        [DataMember]
        public string MaximumPeriod { get; set; }
        [DataMember]
        public string MinUnit { get; set; }
        [DataMember]
        public long MinValue { get; set; }
        [DataMember]
        public string MaxUnit { get; set; }
        [DataMember]
        public long MaxValue { get; set; }
    }

    /// <summary>
    /// 统计Detail信息，与media定义的协议。
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDevicePara
    {
        [DataMember]
        public PhysicalDeviceDto PDDto { get; set; }
        [DataMember]
        public List<IndexInfoPara> IndexInfos { get; set; }
    }

    /// <summary>
    /// 存储从Master Index表格里面插出来的部分数据
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IndexInfoPara
    {
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public int PlanType { get; set; }
        [DataMember]
        public int Category { get; set; }
        [DataMember]
        public long DataSize { get; set; }
        [DataMember]
        public string FarmName { get; set; }
        /// <summary>
        /// 计算Archive需要的属性
        /// </summary>
        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public List<PRCyclePara> PRCycleParas { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Plan id:{0}, plan name:{1}, plan type:{2},category:{3},data size:{4},farm name:{5},site url:{6}",
                this.PlanId, this.PlanName, this.PlanType, this.Category, this.DataSize, this.FarmName, this.SiteUrl);
            stringBuilder.AppendLine();
            if (this.PRCycleParas != null)
            {
                stringBuilder.Append("Pr cycles:");
                stringBuilder.AppendLine();
                foreach (var cycle in this.PRCycleParas)
                {
                    stringBuilder.Append(cycle);
                    stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRCyclePara
    {
        [DataMember]
        public string CycleId { get; set; }
        [DataMember]
        public string JobId { get; set; }

        public override String ToString()
        {
            return String.Format("Crcle id:{0}, Job id:{1}", this.CycleId, this.JobId);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AfterJobState : int
    {
        [EnumMember]
        Completed = 0,
        [EnumMember]
        CompletedWithException = 1,
        [EnumMember]
        Both = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StoragePolicyType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        BackupType = 1,
        [EnumMember]
        ArchiveType = 2,
        [EnumMember]
        Both = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StoragePolicyLicenseType : int
    {
        [EnumMember]
        Docave = 0,
        [EnumMember]
        Netapp = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DateUnit : int
    {
        [EnumMember]
        Day = 0,
        [EnumMember]
        Week = 1,
        [EnumMember]
        Month = 2,
        [EnumMember]
        Year = 3
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum KeepDateType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ArchiveTime = 1,
        [EnumMember]
        ModifiedTime = 2,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RetainType : int
    {
        [EnumMember]
        Retention = 0,
        [EnumMember]
        DeleteOrphanDatas = 1,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupManagementGroupType : int
    {
        [EnumMember]
        Standard = 0,
        [EnumMember]
        Daily = 1,
        [EnumMember]
        Weekly = 2,
        [EnumMember]
        All = 3,
    }
}