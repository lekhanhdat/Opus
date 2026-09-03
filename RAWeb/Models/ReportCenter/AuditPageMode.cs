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
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Web.Models.ReportCenter
{
    [DataContract]
    public class AuditPageMode
    {
        [DataMember]
        public DateRange Range { get; set; }
        [DataMember]
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? EndTime { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public DisplayColumn ViewBy { get; set; }
        [DataMember]
        public string ViewByValue { get; set; }

        //用于排序和过滤
        [DataMember]
        public bool? IsAscending { get; set; }
        [DataMember]
        public DisplayColumn SortBy { get; set; }
        [DataMember]
        public Dictionary<int, List<dynamic>> FilterInfos { get; set; }
    }

    public class AuditExportMode
    {
        public DateRange Range { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class AuditTable
    {
        public List<AuditTableInfo> TableInfo { get; set; }
        public int PageCount { get; set; }
        public int PageIndex { get; set; }
    }
    public class AuditTableInfo
    {
        public RMAuditInfo Item { get; set; }
        public string ActionStr { get; set; }
        public string ModuleStr { get; set; }
        public string CategoryStr { get; set; }
        public string StatusStr { get; set; }
        public string DateStr { get; set; }
    }
    public class AuditChartInfo
    {
        public List<AuditChart> ChartDatas { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
    public class AuditChart
    {
        public string LabelStr { get; set; }
        public object LabelVal { get; set; }
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int valueCount { get; set; }
        public DayOfWeek dateOfWeek { get; set; }
    }
    public enum DateRange
    {
        Current_Week = 0,
        Current_Month = 2,
        Three_Month = 3,
        Six_Month = 4,
        Custom = 5,
    }
    public class FilterSource
    {
        public Dictionary<int, string> ActionItems { get; set; }
        public Dictionary<int, string> ModuleItems { get; set; }
        public Dictionary<int, string> StatusItems { get; set; }
        public List<string> UserItems { get; set; }
    }
}