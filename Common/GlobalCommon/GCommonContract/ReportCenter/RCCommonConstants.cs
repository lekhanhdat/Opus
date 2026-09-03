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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.ReportCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        SharePointService = 1,
        [EnumMember]
        CpuAndMemory = 2,
        [EnumMember]
        Networking = 3,
        [EnumMember]
        SharePointSearchService = 4,
        [EnumMember]
        FarmExplorer = 5,
        [EnumMember]
        EnvironmentSearch = 6,
        [EnumMember]
        DifferenceReports = 7,
        [EnumMember]
        StorageTrends = 8,
        [EnumMember]
        BlobCalculator = 9,
        [EnumMember]
        SharePointAlert = 10,
        [EnumMember]
        LoadTimeForSiteCollection = 11,
        [EnumMember]
        CheckOutDocuments = 12,
        [EnumMember]
        LastAccessedTime = 13,
        [EnumMember]
        UserStorageSize = 14,
        [EnumMember]
        SearchUsage = 15,
        [EnumMember]
        WorkflowStatus = 16,
        [EnumMember]
        SiteUsage = 17,
        [EnumMember]
        SiteReferrers = 18,
        [EnumMember]
        SiteActivityAndUsage = 19,
        [EnumMember]
        PageTraffic = 20,
        [EnumMember]
        MostActiveUsers = 21,
        [EnumMember]
        DownloadRanking = 22,
        [EnumMember]
        FailedLoginAttempts = 23,
        [EnumMember]
        //for failedlogin and download ranking collector
        IISLog = 24,
        [EnumMember]
        // rc auditor related collector
        RcAuditor = 25,
        [EnumMember]
        SharePointTopology = 26,
        [EnumMember]
        DiskSpaceMonitoring = 27,
        [EnumMember]
        AuditController = 28,
        [EnumMember]
        AuditReport = 29,
        [EnumMember]
        AuditPruning = 30,
        [EnumMember]
        JobPerformanceMonitoring = 31,
        [EnumMember]
        PerformanceMonitoring = 32,
        [EnumMember]
        DocAveAudit = 33,
        [EnumMember]
        AuditReportNew = 34,
        [EnumMember]
        AdminReport = 35,
        [EnumMember]
        AuditRestore = 36,
        [EnumMember]
        DocAveTopology = 37,
        [EnumMember]
        AuditControllerApply = 38,
        [EnumMember]
        BlobGenerateRawData = 39,
        [EnumMember]
        BestPracticeReport = 40,
        [EnumMember]
        ContentTypeUsage = 41,
        [EnumMember]
        MetadataChanges = 42,
        [EnumMember]
        ContentTypeChanges = 43,
        [EnumMember]
        PublishingReport = 44,
        [EnumMember]
        ScheduleExportReport = 45,
        [EnumMember]
        ManagementAPIReport = 46,
        [EnumMember]
        UsageReport = 47,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCEmailType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Normal = 1,
    }
  
}