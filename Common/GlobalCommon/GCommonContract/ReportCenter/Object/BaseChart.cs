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
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object;
using AvePoint.GCommon.Contract.ReportCenter.Common;


namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [KnownType(typeof(AuditControllerChart))]
    [KnownType(typeof(AuditReportChart))]
    [KnownType(typeof(AuditPruningChart))]
    [KnownType(typeof(AdminReportChart))]
    [KnownType(typeof(RCJobDeletionChart))]
    [KnownType(typeof(StopSubJobChart))]
    [KnownType(typeof(RunNowScheduleChart))]
    [KnownType(typeof(DocAveAuditChart))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveCodeReviewAttribute("2012/01/13", "DL_DEV_4@avepoint.com", "Dazheng.Yang@avepoint.com",
         new string[] 
        {
           CodeReviewConstants.CHECK_LIST_ID_FA_5
        }, "ADO-24990", true)]
    public class BaseChart
    {
        [DataMember]
        public BaseChartConfig ChartConfig { get; set; }

        public LineBarModel ChartLineBarModel { get; set; }

        public PieModel ChartPieModel { get; set; }

        /// <summary>
        /// 当前登录的UserName
        /// </summary>
        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string PlanName { set; get; }

        [DataMember]
        public BaseScope Scope { get; set; }

        [DataMember]
        public TimeWindow TimeWindow { set; get; }

        [DataMember]
        public PageInfo PagingInfo { set; get; }

        [DataMember]
        public SPUserScope UserScope { get; set; }
        public string DepandProfileId { get; set; }
        public string SchduleJobQueueId { get; set; }
    }

    public class LineBarModel
    {
        public string Unit { get; set; }

        public double LocalMinimum { get; set; }

        public double LocalMaximum { get; set; }

        public double LocalInterval { get; set; }

        public List<List<double>> LocalMetadatas { get; set; }

        public List<string> LocalLabels { get; set; }

        public List<List<object>> LocalTip { get; set; }
    }

    public class LineToolTipModel
    {
        public string Site { get; set; }

        public string ValueX { get; set; }

        public string ValueY { get; set; }

        public string Quota { get; set; }
    }

    public class BarToolTipModel
    {
        public string Value { get; set; }

        public List<string> Sites { get; set; }
    }

    public class PieModel
    {
        public List<double> LocalMetadatas { get; set; }

        public double LocalRadius { get; set; }

        public List<object> LocalTip { get; set; }
    }
}
