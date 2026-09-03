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


using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class AbstractSchedule
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// 用于存放 Plan Group Id.
        /// </summary>
        [DataMember]
        public string PlanGroupId { get; set; }

        /// <summary>
        /// 用于存放 Plan Group Name.
        /// </summary>
        [DataMember]
        public string PlanGroup { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public IsDayLightSavingTimeType DaylightSavingTimeType { get; set; }

        [DataMember]
        public ScheduleDescription Description { get; set; }

        [DataMember]
        public bool IsDisabled { get; set; }

        public DisableState Disable
        {
            get { return this.IsDisabled ? DisableState.Disable : DisableState.Enable; }
        }

        [DataMember]
        public PlanCategory Category { get; set; }

        [DataMember]
        public ScheduleOwnerType OwnerType { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DisableState
    {
        [EnumMember]
        [Description("Enabled")]
        Enable = 0,

        [EnumMember]
        [Description("Disabled")]
        Disable = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnlyStartTimeScheduleDto : AbstractSchedule
    {

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long StartTimeUTC { get; set; }

        [DataMember]
        public long NextTimeUTC { get; set; }

        [DataMember]
        public long NextTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleIntervalScheduleDto : OnlyStartTimeScheduleDto
    {
        [DataMember]
        public IntervalType IntervalType { get; set; }

        [DataMember]
        public int Interval { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleIntervalWithEndScheduleDto : SimpleIntervalScheduleDto
    {
        [DataMember]
        public ScheduleEndType EndType { get; set; }

        [DataMember]
        public int Occurrences { get; set; }

        [DataMember]
        public int OccurrencesTotal { get; set; }

        [DataMember]
        public long EndTime { get; set; }

        [DataMember]
        public string EndTimeZoneId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdvanceIntervalScheduleDto : SimpleIntervalWithEndScheduleDto
    {
        [DataMember]
        public List<ScheduleMonth> Months { get; set; }

        [DataMember]
        public List<DayOfWeek> DayOfWeeks { get; set; }

        [DataMember]
        public ScheduleSequence WeekSequence { get; set; }

        [DataMember]
        public List<SpecifiedTime> SpecifiedTimes { get; set; }

        [DataMember]
        public List<ProductionTime> TimeRangs { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
    public class ScheduleDto : AdvanceIntervalScheduleDto
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }
        public ScheduleDto Clone()
        {
            return new ScheduleDto()
            {
                Category = this.Category,
                DaylightSavingTimeType = this.DaylightSavingTimeType,
                DayOfWeeks = this.DayOfWeeks,
                Description = this.Description,
                EndTime = this.EndTime,
                EndTimeZoneId = this.EndTimeZoneId,
                EndType = this.EndType,
                Id = this.Id,
                Interval = this.Interval,
                IntervalType = this.IntervalType,
                IsDisabled = this.IsDisabled,
                Months = this.Months,
                NextTime = this.NextTime,
                NextTimeUTC = this.NextTimeUTC,
                Occurrences = this.Occurrences,
                OccurrencesTotal = this.OccurrencesTotal,
                OwnerType = this.OwnerType,
                PlanGroup = this.PlanGroup,
                PlanGroupId = this.PlanGroupId,
                PlanId = this.PlanId,
                PlanName = this.PlanName,
                SpecifiedTimes = this.SpecifiedTimes,
                StartTime = this.StartTime,
                StartTimeUTC = this.StartTimeUTC,
                TimeRangs = this.TimeRangs,
                TimeZoneId = this.TimeZoneId,
                WeekSequence = this.WeekSequence
            };
        }

        /// <summary>
        /// Debug用
        /// </summary>
        public DateTime NextTime4Debug
        {
            get { return new DateTime(this.NextTimeUTC, DateTimeKind.Utc); }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleDescription
    {
        [DataMember]
        public long CreateTime { get; set; }

        [DataMember]
        public long ModifyTime { get; set; }

        [DataMember]
        public List<DateTime> SkipTimes { get; set; }

        [DataMember]
        public ScheduleExtension Extension { get; set; }

        /// <summary> 标示schedule所属的schedule scheme </summary>
        [DataMember]
        public string SchemeId { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string StartTimeStr { get; set; }

        [DataMember]
        public string NextTimeStr { get; set; }

        [DataMember]
        public ScheduleType Type { get; set; }

        [DataMember]
        public int GUISettingType { get; set; }

        /// <summary>
        /// GUI新添加schedule或Update Plan时,edit schedule settings需要将该属性设置成True.
        /// </summary>
        [DataMember]
        public bool NeedCheckStartTime { get; set; }

        [DataMember]
        public PRObjectBase PRScheduleAdvanceOption { get; set; }

        // 存放smsp备份信息
        [DataMember]
        public PRObjectBase PRSNBackupInfoDto { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SpecifiedTime
    {
        [DataMember]
        public ScheduleMonth Month { get; set; }

        [DataMember]
        public ScheduleDay Day { get; set; }

        [DataMember]
        public ScheduleHour Hour { get; set; }

        [DataMember]
        public ScheduleMinute Minute { get; set; }

        [DataMember]
        public AvailableUnitType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProductionTime
    {
        public ProductionTime()
        {
        }

        public ProductionTime(ScheduleHour startHour, ScheduleMinute startMinute, ScheduleHour endHour, ScheduleMinute endMinute)
        {
            this.StartTime = new SpecifiedTime();
            this.StartTime.Hour = startHour;
            this.StartTime.Minute = startMinute;
            this.StartTime.Type = AvailableUnitType.Hour_Minute;
            this.EndTime = new SpecifiedTime();
            this.EndTime.Hour = endHour;
            this.EndTime.Minute = endMinute;
            this.EndTime.Type = AvailableUnitType.Hour_Minute;

        }

        [DataMember]
        public SpecifiedTime StartTime { get; set; }

        [DataMember]
        public SpecifiedTime EndTime { get; set; }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum IsDayLightSavingTimeType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        StartTime = 1,
        [EnumMember]
        EndTime = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AvailableUnitType
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        Hour_Minute = 1,
        [EnumMember]
        Day = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IntervalType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        OnlyOnce = 1,
        [EnumMember]
        Minutely = 2,
        [EnumMember]
        Hourly = 3,
        [EnumMember]
        Daily = 4,
        [EnumMember]
        Weekly = 5,
        [EnumMember]
        Monthly = 6

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleSettingType
    {
        [EnumMember]
        Simple_Interval = 0,
        [EnumMember]
        Monthly_Day_Of_Interval_Month = 1,
        [EnumMember]
        Monthly_Day_Of_Month = 2,
        [EnumMember]
        Monthly_Week_Of_Interval_Month = 3,
        [EnumMember]
        Monthly_Week_Of_Month = 4,
        [EnumMember]
        Hourly_Advance_Production_Time = 5,
        [EnumMember]
        Hourly_Advance_Select_Time = 6,
        [EnumMember]
        Weekly_Advance = 7,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Disabled = 1,
        [EnumMember]
        TestRun = 2,
        [EnumMember]
        Predefined = 4,
        [EnumMember]
        RestoreSchedule = 8,
        [EnumMember]
        RunNowSchedule = 16,
        [EnumMember]
        PRMaintenanceSchedule = 32,
        [EnumMember]
        RestartJobSchedule = 64,
        [EnumMember]
        Hide = 128,
        [EnumMember]
        ObeyPlanGroup = 512,
        [EnumMember]
        CAPolicyAuditorModeSchedule = 1024,
        [EnumMember]
        CAPolicyScanModeSchedule = 2048,
        [EnumMember]
        CAPolicyRealTimeModeSchedule = 4096,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleEndType
    {
        [EnumMember]
        NoEnd = 0,
        [EnumMember]
        EndByTime = 1,
        [EnumMember]
        EndByOccurrences = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleMonth
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        January = 1,
        [EnumMember]
        February = 2,
        [EnumMember]
        March = 3,
        [EnumMember]
        April = 4,
        [EnumMember]
        May = 5,
        [EnumMember]
        June = 6,
        [EnumMember]
        July = 7,
        [EnumMember]
        August = 8,
        [EnumMember]
        September = 9,
        [EnumMember]
        October = 10,
        [EnumMember]
        November = 11,
        [EnumMember]
        December = 12
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleDay
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Day_1 = 1,
        [EnumMember]
        Day_2 = 2,
        [EnumMember]
        Day_3 = 3,
        [EnumMember]
        Day_4 = 4,
        [EnumMember]
        Day_5 = 5,
        [EnumMember]
        Day_6 = 6,
        [EnumMember]
        Day_7 = 7,
        [EnumMember]
        Day_8 = 8,
        [EnumMember]
        Day_9 = 9,
        [EnumMember]
        Day_10 = 10,
        [EnumMember]
        Day_11 = 11,
        [EnumMember]
        Day_12 = 12,
        [EnumMember]
        Day_13 = 13,
        [EnumMember]
        Day_14 = 14,
        [EnumMember]
        Day_15 = 15,
        [EnumMember]
        Day_16 = 16,
        [EnumMember]
        Day_17 = 17,
        [EnumMember]
        Day_18 = 18,
        [EnumMember]
        Day_19 = 19,
        [EnumMember]
        Day_20 = 20,
        [EnumMember]
        Day_21 = 21,
        [EnumMember]
        Day_22 = 22,
        [EnumMember]
        Day_23 = 23,
        [EnumMember]
        Day_24 = 24,
        [EnumMember]
        Day_25 = 25,
        [EnumMember]
        Day_26 = 26,
        [EnumMember]
        Day_27 = 27,
        [EnumMember]
        Day_28 = 28,
        [EnumMember]
        Day_29 = 29,
        [EnumMember]
        Day_30 = 30,
        [EnumMember]
        Day_31 = 31
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleHour
    {
        [EnumMember]
        Hour_12_AM = 0,
        [EnumMember]
        Hour_1_AM = 1,
        [EnumMember]
        Hour_2_AM = 2,
        [EnumMember]
        Hour_3_AM = 3,
        [EnumMember]
        Hour_4_AM = 4,
        [EnumMember]
        Hour_5_AM = 5,
        [EnumMember]
        Hour_6_AM = 6,
        [EnumMember]
        Hour_7_AM = 7,
        [EnumMember]
        Hour_8_AM = 8,
        [EnumMember]
        Hour_9_AM = 9,
        [EnumMember]
        Hour_10_AM = 10,
        [EnumMember]
        Hour_11_AM = 11,
        [EnumMember]
        Hour_12_PM = 12,
        [EnumMember]
        Hour_1_PM = 13,
        [EnumMember]
        Hour_2_PM = 14,
        [EnumMember]
        Hour_3_PM = 15,
        [EnumMember]
        Hour_4_PM = 16,
        [EnumMember]
        Hour_5_PM = 17,
        [EnumMember]
        Hour_6_PM = 18,
        [EnumMember]
        Hour_7_PM = 19,
        [EnumMember]
        Hour_8_PM = 20,
        [EnumMember]
        Hour_9_PM = 21,
        [EnumMember]
        Hour_10_PM = 22,
        [EnumMember]
        Hour_11_PM = 23
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleMinute
    {
        [EnumMember]
        Minute_00 = 0,
        [EnumMember]
        Minute_01 = 1,
        [EnumMember]
        Minute_02 = 2,
        [EnumMember]
        Minute_03 = 3,
        [EnumMember]
        Minute_04 = 4,
        [EnumMember]
        Minute_05 = 5,
        [EnumMember]
        Minute_06 = 6,
        [EnumMember]
        Minute_07 = 7,
        [EnumMember]
        Minute_08 = 8,
        [EnumMember]
        Minute_09 = 9,
        [EnumMember]
        Minute_10 = 10,
        [EnumMember]
        Minute_11 = 11,
        [EnumMember]
        Minute_12 = 12,
        [EnumMember]
        Minute_13 = 13,
        [EnumMember]
        Minute_14 = 14,
        [EnumMember]
        Minute_15 = 15,
        [EnumMember]
        Minute_16 = 16,
        [EnumMember]
        Minute_17 = 17,
        [EnumMember]
        Minute_18 = 18,
        [EnumMember]
        Minute_19 = 19,
        [EnumMember]
        Minute_20 = 20,
        [EnumMember]
        Minute_21 = 21,
        [EnumMember]
        Minute_22 = 22,
        [EnumMember]
        Minute_23 = 23,
        [EnumMember]
        Minute_24 = 24,
        [EnumMember]
        Minute_25 = 25,
        [EnumMember]
        Minute_26 = 26,
        [EnumMember]
        Minute_27 = 27,
        [EnumMember]
        Minute_28 = 28,
        [EnumMember]
        Minute_29 = 29,
        [EnumMember]
        Minute_30 = 30,
        [EnumMember]
        Minute_31 = 31,
        [EnumMember]
        Minute_32 = 32,
        [EnumMember]
        Minute_33 = 33,
        [EnumMember]
        Minute_34 = 34,
        [EnumMember]
        Minute_35 = 35,
        [EnumMember]
        Minute_36 = 36,
        [EnumMember]
        Minute_37 = 37,
        [EnumMember]
        Minute_38 = 38,
        [EnumMember]
        Minute_39 = 39,
        [EnumMember]
        Minute_40 = 40,
        [EnumMember]
        Minute_41 = 41,
        [EnumMember]
        Minute_42 = 42,
        [EnumMember]
        Minute_43 = 43,
        [EnumMember]
        Minute_44 = 44,
        [EnumMember]
        Minute_45 = 45,
        [EnumMember]
        Minute_46 = 46,
        [EnumMember]
        Minute_47 = 47,
        [EnumMember]
        Minute_48 = 48,
        [EnumMember]
        Minute_49 = 49,
        [EnumMember]
        Minute_50 = 50,
        [EnumMember]
        Minute_51 = 51,
        [EnumMember]
        Minute_52 = 52,
        [EnumMember]
        Minute_53 = 53,
        [EnumMember]
        Minute_54 = 54,
        [EnumMember]
        Minute_55 = 55,
        [EnumMember]
        Minute_56 = 56,
        [EnumMember]
        Minute_57 = 57,
        [EnumMember]
        Minute_58 = 58,
        [EnumMember]
        Minute_59 = 59

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleSequence
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        First = 1,
        [EnumMember]
        Second = 2,
        [EnumMember]
        Third = 3,
        [EnumMember]
        Fourth = 4,
        [EnumMember]
        Fifth = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "ScheduleExtension")]
    public class ScheduleExtension
    {
        ///<summary>
        ///module:Granular Backup,Granular Restore
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public bool IncludeItemsReport { get; set; }


        [DataMember]
        [XmlAttribute]
        public int GUISettingType { get; set; }

        /// <summary>
        /// module:Granular Backup,Granular Restore
        /// scope:Restart job; Rerun job
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public string RestartJobId { get; set; }

        ///<summary>
        ///module:Granular Backup,Granular Restore
        ///<example>FB,IB,DB</example>
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public BackupType GranularBackupType { get; set; }

        ///<summary>
        ///module:Platform Recovery
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public PRBackupLevel PRBackupLevel { get; set; }

        ///<summary>
        ///module:Platform Recovery
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public PRBackupType PRBackupType { get; set; }

        ///<summary>
        ///module:Extender
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public ExtenderType ExtenderType { get; set; }
        ///<summary>
        ///module:Archiver
        ///</summary>
        [DataMember]
        [XmlAttribute]
        public ArchiverType ArchiverType { get; set; }

        /// <summary>
        /// module:Replicator,Content Manager  注释:调Item模块Backup或Restore功能的Job的Id,需保存到Backup Job或Restore Job里.
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public String RelatedJobId { get; set; }

        /// <summary>
        /// module:Replicator,Content Manager  注释:详见PlanCategory enum
        /// </summary>
        [DataMember]
        [XmlAttribute]
        public int JobCategory { get; set; }

        [DataMember]
        public ModuleScheduleExtension ReplicatorExtension { get; set; }

        [DataMember]
        public ModuleScheduleExtension MigrationExtension { get; set; }

        [DataMember]
        public PGScheduleType PGScheduleType { get; set; }

        [DataMember]
        public PlanGroupScheduleExtension PlanGroupExtension { get; set; }

        [DataMember]
        public ModuleScheduleExtension VMExtension { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
    public class ModuleScheduleExtension 
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupType
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental,
        [EnumMember]
        Differential
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        FullBackup = 1,
        [EnumMember]
        IncrementalBackup = 2,
        [EnumMember]
        DifferentialBackup = 4,

        // add for PR Schedule
        [EnumMember]
        Database = 8,
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

    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleOwnerType
    {
        [EnumMember]
        Plan = 0,
        [EnumMember]
        PlanGroup = 1,
        [EnumMember]
        RPHealthCheck=2,

    }
}
