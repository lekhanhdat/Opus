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
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{

    public class BoardDto
    {
        public BoardTotalDto TotalInfo { get; set; }
        public SourcePieDto SourcePie { get; set; }
        public AssigneesDto AssigneesDto { get; set; }
        public LineChartInfo LineChartInfo { get; set; }
    }
    public class AssigneesDto
    {
        public List<PieChartDto> Assignees { get; set; }
    }

    public class SourcePieDto
    {
        public List<PieChartDto> Sources { get; set; }
    }

    public class TimeLineDto
    {
        public long ApprovalTotal { get; set; }
        public long MaxNum { get; set; }
        public List<LineData> LineDatas { get; set; }
        public List<string> XAxis { get; set; }
    }

    public class LineData
    {
        public string LineName { get; set; }
        public List<long> Counts { get; set; }
    }

    public class DataOfDayDto
    {
        public long Date { get; set; }
        public long Created { get; set; }
        public long Destroyed { get; set; }
        public long WaitingApproval { get; set; }
        public string Timestamp { get; set; }
    }

    public class PieChartDto
    {
        public int Id { get; set; }
        public string name { get; set; }
        public long data { get; set; }
        public string color { get; set; }
        public DateTime CollectionTime { get; set; }
    }
    [DataContract]
    public class BoardQueryOption
    {
        [DataMember]
        public LineChartPageMode LineChartPageMode { get; set; }
        //public BoardDateRange DateRange { get; set; }
    }

    [DataContract]
    public class LineChartPageMode
    {
        [DataMember]
        public ChartDateRange Range { get; set; }
        [DataMember]
        public SourceFlag SourceFlag { get; set; }
        [DataMember]
        public DateTime? StartTime { get; set; }
        [DataMember]
        public DateTime? EndTime { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string ViewByValue { get; set; }
    }
    [DataContract]
    public enum ChartDateRange
    {
        [DataMember]
        Last12Month = 0,
        [DataMember]
        Last10Weeks = 1,
        [DataMember]
        Last10Days = 2,
        [DataMember]
        Custom = 3,
    }

    public class LineChartInfo
    {
        public long MaxNum { get; set; }
        public List<LineChart> ChartInfos { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class LineChart
    {
        public string LineName { get; set; }
        public LineType LineType { get; set; }
        public List<LineChartNode> Nodes { get; set; }
    }

    public enum LineType
    {
        Created,
        Destroyed,
        Waiting
    }

    public class LineChartNode
    {
        public string LabelStr { get; set; }
        public string LabelVal { get; set; }
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long valueCount { get; set; }
        public DayOfWeek dateOfWeek { get; set; }
    }
    [DataContract]
    public class BoardTotalDto
    {
        [DataMember]
        public int SourceFlag { get; set; }
        [DataMember]
        public long CreatedTotal { get; set; }
        [DataMember]
        public long DestroyTotal { get; set; }
        [DataMember]
        public long WaitingTotal { get; set; }
        [DataMember]
        public long CollectionTime { get; set; }
        [DataMember]
        public string LastJobTime { get; set; }
        //public string NextJobTime { get; set; }
    }

    public class BarChartDto
    {
        public long Content { get; set; }
        public string Title { get; set; }
        public string TooltipValue { get; set; }
    }


}
