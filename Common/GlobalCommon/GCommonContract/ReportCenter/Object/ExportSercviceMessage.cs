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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.Common;
using AvePoint.GCommon.Contract.ReportCenter.ExportReport;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportSercviceMessage
    {
        [DataMember]
        public ExportToDataSheetArguments ExportArgus { get; set; }

        [DataMember]
        public RCExportLocationDto RCLocation { get; set; }

        [DataMember]
        public ReportEmailScope EmailScope { get; set; }

        [DataMember]
        public DownloadReportType DownloadReportType { get; set; }

        [DataMember]
        public ExportReportType ExportType { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string PlanName { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DownloadReportType
    {
        [EnumMember]
        ExportNow,

        [EnumMember]
        PublishSpecify,

        [EnumMember]
        PublishEachSite,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportReportType
    {
        [EnumMember]
        CSVReport,

        [EnumMember]
        XLSXReport
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportToDataSheetArguments
    {
        public string TempFilePath { get; set; }
        /// <summary>
        /// 前台所在时区
        /// </summary>
        [DataMember]
        public TimeSpan TimeZone { get; set; }

        /// <summary>
        /// 报表文件格式
        /// </summary>
        [DataMember]
        public string Format { get; set; }

        /// <summary>
        /// 报表文件名
        /// </summary>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// 报表功能类别
        /// </summary>
        [DataMember]
        public ExportCategory ExportCate { get; set; }

        public List<AbstractExportDto> Infos { get; set; }

        [DataMember]
        public BaseChart Chart { get; set; }

        [DataMember]
        public ScheduleExportReportScope Scope { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportCategory
    {
        [EnumMember]
        UserStorageSize,
        [EnumMember]
        StorageTrends,
        [EnumMember]
        SiteUsage,
        [EnumMember]
        SiteActivityAndUsage,
        [EnumMember]
        ActiveUsers,
        [EnumMember]
        ActiveUsersTopRecords,
        [EnumMember]
        DocAveAudit,
        [EnumMember]
        SiteUsageUser
    }
}
