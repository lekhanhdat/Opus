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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    public class DueDisposalReport : BaseReport
    {
        public string BCSTermId { get; set; }
        public string BCSTermName { get; set; }
        public string AppliedRuleId { get; set; }
        public string AppliedRuleName { get; set; }
        /// <summary>
        /// 由之前的RMContentDisposalAction枚举改为int值 以|的形式存储 以&的形式解析 可以适应更多的情况
        /// 0代表ArchiveAndRemove 1代表ArchiveAndKeepData
        /// 26代表同时ArchiveLeaveStub RelatedRecords DeclaredRecords
        /// </summary>
        public int DisposalAction { get; set; }
        public RMDisposalManualApproval ManualApproval { get; set; }
        public RMExportTypeValue ExportType { get; set; }
        public RMReportStatus Status { get; set; }
        public string Comment { get; set; }
        public string LifecycleStatus { get; set; }//open closed Pending Destruction Destroyed
        public string Availablity { get; set; }//Availabe Loaned Missing
        public string HomeLocation { get; set; }
        public string CurrentHeldBy { get; set; }
        public string Box { get; set; }
        public string RelatedRecords { get; set; }
        public int RelatedRecordsAction { get; set; }
        public string SiteCollectionTitle { get; set; }
        public string DisposalClass { get; set; }
    }

    public class ReportRelatedRecords
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class AvailableSpaceReport : BaseReport
    {
        public string Location { get; set; }
        public double AvailableSpace { get; set; }
        public double LocationSize { get; set; }
        public string InculdingContainerInfo { get; set; }
    }
}

