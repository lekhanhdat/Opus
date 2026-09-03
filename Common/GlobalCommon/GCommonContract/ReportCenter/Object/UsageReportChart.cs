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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    public class UsageReportChart : BaseChart
    {
        [DataMember]
        public UsageReportChartType Type { get; set; }

        [DataMember]
        public ScopeProfile ReportProfile { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SiteActivityRankingType
    {
        [EnumMember]
        Sites = 1,
        [EnumMember]
        Pages = 2,
        [EnumMember]
        Lists = 4,
        [EnumMember]
        Users = 8,
        [EnumMember]
        Items = 16,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UsageReportChartType
    {
        [EnumMember]
        RunAndExportReport = 0,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UsageReportType
    {
        [EnumMember]
        ActiveUser = 1,
        [EnumMember]
        SiteActivityRanking = 2,
        [EnumMember]
        SiteVisitorAndActivity = 4,
        [EnumMember]
        DownloadRanking = 8,
        [EnumMember]
        LastAccessedTime = 16,
    }
}
