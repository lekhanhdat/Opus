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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCScheduleDto : ScheduleDto
    {
        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public bool IsNeedExport { get; set; }
        /// <summary>
        /// 在update Profile时一旦schedule的任何属性被更改了该值就为true，否则为false
        /// </summary>
        [DataMember]
        public bool SettingChanged { get; set; }

        [DataMember]
        public RCScheduleModule ScheduleModule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCScheduleModule
    {
        [EnumMember]
        AdminReport = 0,

        [EnumMember]
        ExportReport = 1,

        [EnumMember]
        StorageTrends = 2,

        [EnumMember]
        CheckOutDocuments = 3,

        [EnumMember]
        LoadTimeForSiteCollection = 4,

        [EnumMember]
        DiskSpaceMonitoring = 5,

        [EnumMember]
        UserStorageSize = 6,

        [EnumMember]
        SearchUsage = 7,

        [EnumMember]
        SiteReferrers = 8,

        [EnumMember]
        RcAuditor = 9,

        [EnumMember]
        IISLog = 10,

        [EnumMember]
        DocAveAudit = 11,

        [EnumMember]
        PerformanceMonitor = 12,

        [EnumMember]
        EmailChecker1M = 13,

        [EnumMember]
        EmailChecker1H = 14,

        [EnumMember]
        EmailChecker12H = 15,

        [EnumMember]
        AuditController = 16,

        [EnumMember]
        AuditPruning = 17,

        [EnumMember]
        BestPracticeReport = 18,

        [EnumMember]
        AuditReportNew = 19,

        [EnumMember]
        SearchUsageExport = 20,

        [EnumMember]
        SiteActivityAndUsageExport = 21,

        [EnumMember]
        CheckOutDocumentsExport = 22,

        [EnumMember]
        PageTrafficExport = 23,

        [EnumMember]
        SiteReferrersExport = 24,

        [EnumMember]
        LastAccessedTimeExport = 25,

        [EnumMember]
        FailedLoginAttemptsExport = 26,

        [EnumMember]
        WorkflowStatusExport = 27,

        [EnumMember]
        SharePointAlertExport = 28,

        [EnumMember]
        DownloadRankingExport = 29,

        [EnumMember]
        SiteUsageExport = 30,

        [EnumMember]
        MostActiveUsersExport = 31,

        [EnumMember]
        ContentTypeUsageExport = 32,

        [EnumMember]
        MetadataChangeExport = 33,

        [EnumMember]
        AuditControllerApply = 34,

        [EnumMember]
        ManagementAPIReport = 35,

        [EnumMember]
        UsageReport = 36,
    }
}