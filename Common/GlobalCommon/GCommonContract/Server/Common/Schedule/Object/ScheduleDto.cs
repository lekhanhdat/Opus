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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long CreateTime { get; set; }

        [DataMember]
        public long ModifyTime { get; set; }

        [DataMember]
        public long StartTime { get; set; }

        /*
         * 开始时间的字符串格式，如：2010-01-01 00:00:00
         */
        [DataMember]
        public string StartTimeStr { get; set; }

        [DataMember]
        public long NextTime { get; set; }


        /*
         * schedule的开始时间和运行时间的时区Id值
         */
        [DataMember]
        public string TimeZoneId { get; set; }


        /*
         * schedule停止运行的时间
         */
        [DataMember]
        public long EndTime { get; set; }

        /*
         * schedule停止运行的时间的时区Id
         */
        [DataMember]
        public string EndTimeZoneId { get; set; }

        /*
         * schedule运行的类型，共有13种，参考常量Constants的scheduleType for schedule
         */
        [DataMember]
        public int ScheduleType { get; set; }


        /*
         * FB IB DB
         */
        [DataMember]
        public BackupType BackupType { get; set; }

        /*
        * FB IB DB  of PlatformRecoveryBackup
        */
        [DataMember]
        public PRBackupType PRBackupType { get; set; }

        /// <summary>
        /// PR的Inex Level，从DB到Item version
        /// </summary>
        [DataMember]
        public PRBackupLevel PRBackupLevel { get; set; }

        /// <summary>
        /// PR的Defer Index选项
        /// </summary>
        [DataMember]
        public bool PRDeferIndexEnabled { get; set; }

        /*
         * schedule的类型，比如：test run的schedule，默认为normal=0，参考Constants的type for schedule
         */
        [DataMember]
        public ScheduleJobType Type { get; set; }


        /*
         * schedule循环运行的间隔
         */
        [DataMember]
        public int Interval { get; set; }

        [DataMember]
        public string PlanId { get; set; }


        /*
         * schedule的结束方式
         * 0 : No end date
         * 1 : 按结束时间endTime方式结束schedule
         * 2 : 按运行次数endOccurrences方式结束schedule
         */
        [DataMember]
        public ScheduleEndType EndType { get; set; }

        /*
         * schedule已经运行的次数，和endOccurrences一起使用，用于end type == 2 时的schedule
         */
        [DataMember]
        public int Occurrences { get; set; }

        /*
         * schedule应该运行的总次数，和occurrences一起使用，用于end type == 2 时的schedule
         */
        [DataMember]
        public int EndOccurrences { get; set; }


        /*
         * 对于复杂的schedule[6 <= scheduleType <= 13]，该属性存放schedule的高级设置，具体：
         *
         *       scheduleType                           advanceSetting
         *---------------------------------------------------------------------------
         * HOURLY_RANGE_OF_ADVANCED                 from time hh : advanceSetting[0]
         *                                          from time mm : advanceSetting[1]
         *                                          to   time hh : advanceSetting[3]
         *                                          to   time mm : advanceSetting[4]
         *---------------------------------------------------------------------------
         * HOURLY_POINTS_OF_ADVANCED                     time hh : advanceSetting[i]
         *                                               time mm : advanceSetting[i+1]
         *---------------------------------------------------------------------------
         * DAYWEEK_OF_ADVANCED                     day week(0-6) : advanceSetting[i]
         *---------------------------------------------------------------------------
         * WEEK_OF_ADVANCED                        day week(0-6) : advanceSetting[i]
         *---------------------------------------------------------------------------
         * DAY_EVERY_MONTH_OF_ADVANCED              day of month : advanceSetting[0]
         *---------------------------------------------------------------------------
         * DAY_MONTH_OF_ADVANCED                    day of month : advanceSetting[0]
         *                                           month(0-11) : advanceSetting[1]
         *---------------------------------------------------------------------------
         * DAYWEEK_EVERY_MONTH_OF_ADVANCED         day week(0-6) : advanceSetting[0]
         *                                                 order : advanceSetting[1]
         *---------------------------------------------------------------------------
         * DAYWEEK_MONTH_OF_ADVANCED                 month(0-11) : advanceSetting[0]
         *                                         day week(0-6) : advanceSetting[1]
         *                                                 order : advanceSetting[1]
         *---------------------------------------------------------------------------
         */
        [DataMember]
        public int[] AdvanceSetting { get; set; }

        /*
         * 通过按位与标识开始时间和结束时间是否采用夏令时，具体规则是：
         * 低位第一位表示开始时间，低位第二位表示结束时间，比如
         * 
         *    值       开始时间是否采用夏令时  结束时间是否采用夏令时
         * 0 = (00)b             否                    否
         * 1 = (01)b             是                    否
         * 3 = (11)b             是                    是
         */
        [DataMember]
        public int IsDayLightSavingTime { get; set; }


        /*
         * schedule的描述信息
         */
        [DataMember]
        public string Description { get; set; }

        /*
         * 用于retry job的schedule，存储retry job id
         */
        [DataMember]
        public string RetryJobId { get; set; }

        /*
         * 存储schedule运行的排除时间，schedule在这些时间内不运行
         */
        [DataMember]
        public List<TimeRangeDto> ExcludeTimes { get; set; }

        /*
         * 记录schedule skip的次数
         */
        [DataMember]
        public int SkipCount { get; set; }

        [DataMember]
        public Dictionary<string, string> Extension { get; set; }

        /*
         *Schedule Scheme保存在Profile表里，用来和Profile表Id相关联
         */
        [DataMember]
        public string SchemeId { get; set; }

        [DataMember]
        public bool Disabled { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleEndType
    {
       [EnumMember]
        None = 0,
       [EnumMember]
        ByEndTime = 1,
       [EnumMember]
        ByEndOccurrences = 2,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleJobType
    { 
       [EnumMember]
        Normal = 0,
        [EnumMember]
        TestRun = 1,
    }
}
