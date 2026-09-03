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
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManagementAPIReportScope : BaseScope
    {
        /// <summary>
        /// TimeRangeType为Duration时需要设置调用方当前时间到StartTime用来获取调用方的当前时间以及时区
        /// </summary>
        [DataMember]
        public AuditTimeRangeType TimeRangeType { get; set; }

        //for AuditReportTimeRangeType.Duration
        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }
        /// <summary>
        /// 0 = this 
        /// 1 = last 1
        /// 2 = last 2
        /// 依此类推
        /// 
        /// </summary>
        [DataMember]
        public int DurationValue { get; set; }

        //AuditReportTimeRangeType.Duration这个值用于获取调用方的时区
        //导出report时使用这个获取调用方时区
        [DataMember]
        public TimeSpan TimeOffset { get; set; }


        //记录当地的时区
        [DataMember]
        public double Offset { get; set; }

        /// <summary>
        ///  AuditReportTimeRangeType.StartEnd为开始时间
        /// </summary>
        [DataMember]
        public DateTimeOffset StartTime { get; set; }
        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        //6.3新添加属性，记录start time 、end time是否支持夏令时
        [DataMember]
        public bool StartTimeDst { get; set; }
        [DataMember]
        public bool EndTimeDst { get; set; }

        [DataMember]
        public AuditReportType ReportType { get; set; }

        [DataMember]
        public ReportEmailScope EmailScope { get; set; }

        [DataMember]
        public APIUrlFilterCondition UrlFilter { get; set; }

        [DataMember]
        public APIUserFilterCondition UserFilter { get; set; }

        [DataMember]
        public APIActionFilterCondition ActionFilter { get; set; }

        [DataMember]
        public List<O365ActivityType> ProductTypes { get; set; }

        /// <summary>
        /// Azure AD 中的 Office 365 Group filter
        /// </summary>
        [DataMember]
        public List<O365GroupType> O365GroupTypes { get; set; }

        /// <summary>
        /// SharePoint Sites 分类
        /// </summary>
        [DataMember]
        public List<SharePointOnlineSitesType> SharePointSiteTypes { get; set; }

        /// <summary>
        /// schedule export report时代表下载的路径
        /// </summary>
        [DataMember]
        public string DownloadLocationId { get; set; }

        /// <summary>
        /// schedule export report时 download report name
        /// </summary>
        [DataMember]
        public string ScheduleReportName { get; set; }

        /// <summary>
        /// run now schedule download Location
        /// </summary>
        [DataMember]
        public string ExportReportDownloadLocationId { get; set; }

        /// <summary>
        /// run now  download report name
        /// </summary>
        [DataMember]
        public string RunNowReportName { get; set; }

        /// <summary>
        /// run now  download Report Type
        /// </summary>
        [DataMember]
        public AuditReportType RunNowReportType { get; set; }

        [DataMember]
        public RCScheduleDto JobSchedule { get; set; }

        [DataMember]
        public string StartTimeTimeZoneId { get; set; }

        [DataMember]
        public string EndTimeTimeZoneId { get; set; }

        [DataMember]
        public ManagementAPIReportChartType ReportChartType { get; set; }

        [DataMember]
        public string LastModifyUser { get; set; }

        [DataMember]
        public bool ZipFileUploadToSP { get; set; }

        //标记是否是Run Now export
        [DataMember]
        public bool ZipFileForExportNow { get; set; }

        [DataMember]
        public ManageApiReportTitleType ReportDataTitlesType { get; set; }
        [DataMember]
        public ManageApiReportTitleType ReportDataTitlesType4RunNow { get; set; }

        [DataMember]
        public bool CustomExportReportColumn { get; set; }

        [DataMember]
        public bool CustomExportReportColumnRunNow { get; set; }

        [DataMember]
        public bool BreakInheritanceLibrarySchedule { get; set; }

        [DataMember]
        public bool BreakInheritanceLibraryRunNow { get; set; }

        [DataMember]
        public List<string> Tenants { get; set; }

        [DataMember]
        public bool TenantForRunNow { get; set; }

        [DataMember]
        public bool TenantForSchedule { get; set; }
    }
}
