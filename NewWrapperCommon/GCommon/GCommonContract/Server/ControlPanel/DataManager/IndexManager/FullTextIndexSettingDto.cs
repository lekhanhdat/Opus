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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexSettingDto
    {
        [DataMember]
        public GenerateMode GenerateMode { get; set; }
        [DataMember]
        public ScheduleDto Schedule { get; set; }
        [DataMember]
        public ProcessDto Process { get; set; }
        [DataMember]
        public FileSettingDto FileSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProcessDto
    {
        [DataMember]
        public int WorkingHourProcess { get; set; }
        [DataMember]
        public int NoWorkingHourProcess { get; set; }
        [DataMember]
        public WorkScheduleDto DefineWorkSchedule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileSettingDto
    {
        [DataMember]
        public IndexScopeType ScopeType { get; set; }
        [DataMember]
        public List<string> FileType { get; set; }
        [DataMember]
        public int FileSize { get; set; }
        [DataMember]
        public bool? PreviewFunction { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WorkScheduleDto
    {
        [DataMember]
        public int WorkStartTime { get; set; }
        [DataMember]
        public int WorkEndTime{ get; set; }
        [DataMember]
        public List<string> WorkingDay{ get; set; }
        [DataMember]
        public string WorkingTimeZoneId { get; set; }
        [DataMember]
        public bool IsDaylightSavingTime { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GenerateMode
    {
        [EnumMember]
        OnJobFinished,
        [EnumMember]
        OnSchedule,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IndexScopeType
    {
        [EnumMember]
        MetaData = 1,
        [EnumMember]
        Content = 2,
        [EnumMember]
        All = MetaData | Content
    }
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum WeekDay
    //{
    //    [EnumMember]
    //    Sunday = 1,
    //    [EnumMember]
    //    Monday = 2,
    //    [EnumMember]
    //    Tuesday = 4,
    //    [EnumMember]
    //    Wednesday = 8,
    //    [EnumMember]
    //    Thursday = 16,
    //    [EnumMember]
    //    Friday = 32,
    //    [EnumMember]
    //    Saturday = 64
    //}

}
