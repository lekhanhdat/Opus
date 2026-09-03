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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPruningRuleDto : ISystemSettingContent
    {
        /// <summary>
        /// prune rule detail, this list contains rules of each module, also a general rule.
        /// </summary>
        [DataMember]
        public List<JobPruningRuleDetailDto> Detail { get; set; }

        /// <summary>
        /// job pruning schedule.
        /// </summary>
        [DataMember]
        public JobPruningSettingDetailDto Setting { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPruningRuleDetailDto
    {
        /// <summary>
        /// job category
        /// </summary>
        [DataMember]
        public int Category { get; set; }

        /// <summary>
        /// the life cycle
        /// </summary>
        [DataMember]
        public int LifeCycle { get; set; }

        /// <summary>
        /// life cycle interval
        /// </summary>
        [DataMember]
        public int LifeCycleInterval { get; set; }

        /// <summary>
        /// remain job count
        /// </summary>
        [DataMember]
        public int JobCount { get; set; }

        /// <summary>
        /// no pruning ??
        /// </summary>
        [DataMember]
        public PruningRuleType PruningRuleType { get; set; }

        /// <summary>
        /// whether remove the backup data, for granular backup.
        /// </summary>
        [DataMember]
        public bool RemoveBackupData { get; set; }

        [DataMember]
        public bool RemoveExportData { get; set; }

        /// <summary>
        /// 计算临界时间
        /// </summary>
        [DataMember]
        public long TimeCut { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPruningSettingDetailDto
    {
        /// <summary>
        /// true .....NoSchedule  ,false ......configure schedule myself
        /// </summary>
        [DataMember]
        public bool ScheduleDefined { get; set; }

        [DataMember]
        public List<ScheduleDto> JobPruningScheduleDtos { get; set; }

        [DataMember]
        public NotificationDto Notification { get; set; }

        [DataMember]
        public ProfileDto Profile { get; set; } // 用于记录job pruning 中的setting 选中的profile， 原来是直接save  Notification（Notification =ProfileDto.Content as Notification）

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PruningRuleType : int
    {
        [EnumMember]
        NoPruning = 0,

        [EnumMember]
        KeepJobCount = 1,

        [EnumMember]
        KeepCycle = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleGroupType : int
    {
        [EnumMember]
        Administration = 0,
        [EnumMember]
        ControlPanel = 1,
        [EnumMember]
        DataProtection = 2,
        [EnumMember]
        ReportCenter = 3,
        [EnumMember]
        StorageOptimization = 4,
        [EnumMember]
        Migration=5,
        [EnumMember]
        Compliance=6,

    }

}
