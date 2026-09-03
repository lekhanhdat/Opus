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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Storage.Entity
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StoragePolicyXml
    {
        #region == 这部分属性各个功能修改以后会删除掉 ==
        private List<TimeBasedPolicyXml> timeBasedPolicys = new List<TimeBasedPolicyXml>();
        private List<EventBasedPolicyXml> eventBasedPolicys = new List<EventBasedPolicyXml>();
        private List<CycleBasedPolicyXml> cycleBasedPolicys = new List<CycleBasedPolicyXml>();
        [DataMember]
        public List<TimeBasedPolicyXml> TimeBasedPolicys { get; set; }
        [DataMember]
        public List<EventBasedPolicyXml> EventBasedPolicys { get; set; }
        [DataMember]
        public List<CycleBasedPolicyXml> CycleBasedPolicys { get; set; }
        [DataMember]
        [XmlAttribute("retentionPolicyType")]
        public int RetentionPolicyType { get; set; }
        #endregion

        [DataMember]
        public NotificationDto Notification { get; set; }

        [DataMember]
        public string NotificationId { get; set; }

        [XmlAttribute("storageType")]
        [DataMember]
        public StoragePolicyType StorageType { get; set; }
        [DataMember]
        public StoragePolicyLicenseType LicType { get; set; }
        [DataMember]
        public string BackupStoragePolicyId { get; set; }
        [XmlAttribute("setupDataRetention")]
        [DataMember]
        public bool SetupDataRetention { set; get; }
        [DataMember]
        public ScheduleDto Schedule { set; get; }
        [DataMember]
        public FullTextIndexSettingDto FullTextIndexSetting { set; get; }
        [DataMember]
        public List<BackupRetentionRuleXml> BackupRetentionRules { get; set; }
        [DataMember]
        public List<ArchiveRetentionRuleXml> ArchiveRetentionRules { get; set; }
        /// <summary>
        /// Media 是否采用自定义排序
        /// </summary>
        [DataMember]
        public bool MediaSpecifyOrder { get; set; }
        /// <summary>
        /// Media排序信息
        /// </summary>
        [DataMember]
        public List<MediaOrderInfoXml> MediaOrderInfos { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StroagePolicyContentXml
    {
        [DataMember]
        [XmlAttribute("deleteTheData")]
        public bool DeleteTheData { set; get; }
        [DataMember]
        [XmlAttribute("removeTheJob")]
        public bool RemoveTheJob { set; get; }
        [DataMember]
        [XmlAttribute("isMove")]
        public bool IsMove { set; get; }
        [DataMember]
        [XmlAttribute("moveLogicalDeviceId")]
        public string MoveLogicalDeviceId { set; get; }
        [DataMember]
        [XmlAttribute("setupDataRetention")]
        public bool SetupDataRetention { set; get; }

        /// <summary>
        /// MoveLogicalDeviceId对应的名称
        /// </summary>
        [DataMember]
        [XmlAttribute("logicalName")]
        public string LogicalName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimeBasedPolicyXml : StroagePolicyContentXml
    {
        [DataMember]
        [XmlAttribute("keepDataType")]
        public int KeepDataType { set; get; }
        [DataMember]
        [XmlAttribute("keepDataValue")]
        public int KeepDataValue { set; get; }
        [DataMember]
        [XmlAttribute("beforeBackup")]
        public bool BeforeBackup { set; get; }
        [DataMember]
        [XmlAttribute("afterBackup")]
        public bool AfterBackup { set; get; }
        [DataMember]
        [XmlAttribute("complated")]
        public bool Complated { set; get; }
        [DataMember]
        [XmlAttribute("complatedWithException")]
        public bool ComplatedWithException { set; get; }
        [DataMember]
        [XmlAttribute("triggerGroupName")]
        public string TriggerGroupName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EventBasedPolicyXml : StroagePolicyContentXml
    {
        [DataMember]
        [XmlAttribute("triggerEvent")]
        public int TriggerEvent { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CycleBasedPolicyXml : StroagePolicyContentXml
    {
        [DataMember]
        [XmlAttribute("fullBackup")]
        public bool FullBackup { set; get; }
        [DataMember]
        [XmlAttribute("incrementalBackup")]
        public bool IncrementalBackup { set; get; }
        [DataMember]
        [XmlAttribute("differentBackup")]
        public bool DifferentBackup { set; get; }
        [DataMember]
        [XmlAttribute("keepCycles")]
        public int KeepCycles { set; get; }
        [DataMember]
        [XmlAttribute("KeepBackupFailedJobs")]
        public bool KeepBackupFailedJobs { set; get; }
        [DataMember]
        [XmlAttribute("isCustomAction")]
        public bool IsCustomAction { get; set; }

        [DataMember]
        [XmlAttribute("customActionValue")]
        public string CustomActionValue { get; set; }

        [DataMember]
        [XmlAttribute("customDesc")]
        public string CustomDesc { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupRetentionRuleXml : StroagePolicyContentXml
    {
        /// <summary>
        /// 对应Snapvault Retention的checkbox选中状态
        /// </summary>
        [DataMember]
        [XmlAttribute("IsCheckSnapvaultRetention")]
        public Boolean IsCheckSnapvaultRetention { get; set; }

        /// <summary>
        /// 对应Snapvault Retention中Keep Last Full的Radio button选中状态
        /// </summary>
        [DataMember]
        [XmlAttribute("IsCheckSnapvaultKeepFull")]
        public Boolean IsCheckSnapvaultKeepFull { get; set; }

        /// <summary>
        /// 对应Snapvault Retention中Keep Last Data的Radio button选中状态
        /// </summary>
        [DataMember]
        [XmlAttribute("IsCheckSnapvaultKeepDate")]
        public Boolean IsCheckSnapvaultKeepDate { get; set; }

        /// <summary>
        /// 对应Snapvault Retention中Keep Last Full的textbox填写文本
        /// </summary>
        [DataMember]
        [XmlAttribute("KeepSnapvaultFullValue")]
        public Int32 KeepSnapvaultFullValue { get; set; }

        /// <summary>
        /// 对应Snapvault Retention中Keep Last Data的textbox填写文本
        /// </summary>
        [DataMember]
        [XmlAttribute("KeepSnapvaultDateValue")]
        public Int32 KeepSnapvaultDateValue { get; set; }

        /// <summary>
        /// 对应Snapvault Retention中Keep Last Data选择的Unit
        /// </summary>
        [DataMember]
        [XmlAttribute("KeepSnapvaultDateUnit")]
        public DateUnit KeepSnapvaultDateUnit { get; set; }

        /// <summary>
        /// 对应Cycle oriented rule选项
        /// </summary>
        [DataMember]
        [XmlAttribute("cycleOrientedRule")]
        public bool CycleOrientedRule { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles的Radio button
        /// </summary>
        [DataMember]
        [XmlAttribute("isCheckKeepCycles")]
        public bool IsCheckKeepCycles { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles
        /// </summary>
        [DataMember]
        [XmlAttribute("keepCyclesValue")]
        public int KeepCyclesValue { set; get; }

        /// <summary>
        /// 对应页面中设置间隔时间的Radio Button
        /// </summary>
        [DataMember]
        [XmlAttribute("isCheckKeepDate")]
        public bool IsCheckKeepDate { get; set; }

        /// <summary>
        /// 对应页面中Keep the last cycles
        /// </summary>
        [DataMember]
        [XmlAttribute("keepCyclesDateValue")]
        public int KeepCyclesDateValue { set; get; }

        /// <summary>
        /// 对应页面中的Cycle oriented rule处的时间单位
        /// </summary>
        [DataMember]
        [XmlAttribute("cycleDateUnit")]
        public DateUnit CycleDateUnit { set; get; }


        /// <summary>
        /// 对应页面中Full Backup Oriented Rule选项
        /// </summary>
        [DataMember]
        [XmlAttribute("fullBackupOriented")]
        public bool FullBackupOriented { set; get; }

        /// <summary>
        /// 对应Keep the last full backups的Radio button
        /// </summary>
        [DataMember]
        [XmlAttribute("isCheckKeepBackupValue")]
        public bool IsCheckKeepBackupValue { set; get; }

        /// <summary>
        /// 对应页面中Keep the last full backups所设定的值
        /// </summary>
        [DataMember]
        [XmlAttribute("keepBackupValue")]
        public int KeepBackupValue { set; get; }

        /// <summary>
        /// 对应页面中设置Full backup oriented rule间隔时间的Radio Button
        /// </summary>
        [DataMember]
        [XmlAttribute("isCheckKeepDateValue")]
        public bool IsCheckKeepDateValue { set; get; }

        /// <summary>
        /// 对应页面中Keep The Backup Date的时间设置。
        /// </summary>
        [DataMember]
        [XmlAttribute("keepBackupDateValue")]
        public int KeepBackupDateValue { set; get; }

        /// <summary>
        /// 对应页面中的Full Backup Oriented Rule处的时间单位
        /// </summary>
        [DataMember]
        [XmlAttribute("backupDateUnit")]
        public DateUnit BackupDateUnit { set; get; }

        /// <summary>
        /// 对应页面中Before the job started
        /// </summary>
        [DataMember]
        [XmlAttribute("beforeBackup")]
        public bool BeforeBackup { set; get; }  //Before the job started

        /// <summary>
        /// 对应页面中After the job completed
        /// </summary>
        [DataMember]
        [XmlAttribute("afterBackup")]
        public bool AfterBackup { set; get; }  //After the job completed

        /// <summary>
        /// 对应页面中Include the job completed with exception
        /// </summary>
        [DataMember]
        [XmlAttribute("complatedWithException")]
        public bool ComplatedWithException { set; get; } //Completed with Exception

        // <summary>
        /// 对应页面中completed
        /// </summary>
        [DataMember]
        [XmlAttribute("completed")]
        public bool Completed { get; set; }

        // <summary>
        /// 对应页面中BackType中的FullBackup
        /// </summary>
        [DataMember]
        [XmlAttribute("fullBackup")]
        public bool FullBackup { get; set; }

        // <summary>
        /// 对应页面中BackType中的Incremental Backup
        /// </summary>
        [DataMember]
        [XmlAttribute("incrementalBackup")]
        public bool IncrementalBackup { get; set; }

        // <summary>
        /// 对应页面中BackType中的 Differential Backup
        /// </summary>
        [DataMember]
        [XmlAttribute("differentialBackup")]
        public bool DifferentialBackup { get; set; }

        /// <summary>
        /// 对应页面中Keep backup data for failed jobs
        /// </summary>
        [DataMember]
        [XmlAttribute("keepBackupFailedJobs")]
        public bool KeepBackupFailedJobs { set; get; }  //Keep backup data for failed jobs

        [DataMember]
        [XmlAttribute("isCustomAction")]
        public bool IsCustomAction { get; set; }

        [DataMember]
        [XmlAttribute("customActionValue")]
        public string CustomActionValue { get; set; }

        [DataMember]
        [XmlAttribute("customDesc")]
        public string CustomDesc { get; set; }

        [DataMember]
        public BackupManagementGroupType BackupManagementGroup { get; set; }
    }

    public class ArchiveRetentionRuleXml : StroagePolicyContentXml
    {
        [XmlAttribute("keepValue")]
        public int KeepValue { get; set; }
        [XmlAttribute("archiveDateUnit")]
        public DateUnit ArchiveDateUnit { get; set; }
        [DataMember]
        public ScheduleDto ScheduleDto { get; set; }
        [DataMember]
        public bool ReceiveReportAboutRemovingData { get; set; }

        [DataMember]
        public int NotifyDaysPriorToRemoveData { get; set; }

        [DataMember]
        public ScheduleDto NotifySchedule { get; set; }
        [DataMember]
        public bool ManualApproval { get; set; }
    }

    /// <summary>
    /// Media排序序列化
    /// </summary>
    public class MediaOrderInfoXml
    {
        [XmlAttribute("order")]
        public int Order { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
