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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMDownloadDataInfo : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid RecordsId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string JobId { get; set; }

        [Column(TypeName = "int")]
        public int JobStatus { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2000)]
        public string UserId { get; set; }

        [Column(TypeName = "bigint")]
        [Index(name: "IX_FileDownloadTime")]
        public long FileDownloadTime { get; set; }

        [Column(TypeName = "int")]
        public DownloadContentType DownloadType { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2000)]
        public string BlobSasUri { get; set; }

        [Column(TypeName = "bigint")]
        public long? FileSize { get; set; }

        /// <summary>
        /// JobReportContent: ID of the export selected jobs
        /// </summary>
        [Column(TypeName = "nvarchar")]
        [MaxLength(4000)]
        public string ExtendString1 { get; set; }
    }

    public enum DownloadContentJobStatus
    {
        None = -1,
        Wait = 0,
        InProgress = 1,
        Finished = 2,
        Failed = 3,
        FinishWithException = 4,
        Stopped = 5,
        Skipped = 6,
        Stopping = 7,
        Calculating = 8,
    }

    public enum DownloadContentType
    {
        ArchivedContent = 0,
        Others = 1, //disabled 此处枚举值用于兼容老数据.
        LoanPickListContent = 2,
        DestructionPickListContent = 3,
        ZipPasswordInfo = 4,
        ReportContent = 5,
        HistoryContent = 6,
        UnderReviewContent = 7,
        WaitingForDisposalContent = 8,
        JobReportContent = 9,
        PhysicalBuklExport = 10,
        MachineLearningExportReport = 11,
        DisposalExtendContent = 12,
        RelatedRecordsContent = 13,
        ExportSettings = 14,
        ExportTermStructure = 15,
        ExportSiteMetrics = 16,
        ExportIndex = 17,
        ExportSearchRecords = 18,
        ExportDiscoveryProfile = 19,
        DiscoveryExportRowDataJob = 20,
        ReturnLoanHistory = 21,
        ExportConflictSettingDetail = 22,
        JobReportContentForCOP = 23,
        ExportRestoreCenterSeachResult = 24,
        ExportDeduplicationReport = 25,
        ExportSCMapping = 26,
        ExportSCWhitelist = 27,
        ExportSCBlacklist = 28,
        ExportTeamsSOSetting = 29,
        ExportSPSOSetting = 30,
        DiscoveryExportDuplicationReport = 31,
        DownloadRCCReport = 32,
        ExportHoldRecords = 33,
        DiscoveryExportExcludeList = 34,
        SharePointSiteMetricsReport = 35,
        MovePickListContent = 36
    }
}
