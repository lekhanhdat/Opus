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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Report
{
    public class ReportConvertor
    {
        public static List<SQLiteParameter> BuildSaveReportParameters(BaseReport report)
        {
            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            if (report is BCSTermUsageReport)
            {
                BCSTermUsageReport reportnfo = report as BCSTermUsageReport;
                parameters.Add(new SQLiteParameter("BCSTermId", reportnfo.BCSTermId));
                parameters.Add(new SQLiteParameter("BCSTermName", reportnfo.BCSTermName));
                parameters.Add(new SQLiteParameter("TermStatus", reportnfo.TermStatus));
                parameters.Add(new SQLiteParameter("BCSTermFullPath", reportnfo.BCSTermFullPath));
                parameters.Add(new SQLiteParameter("CreatedBy", reportnfo.CreatedBy));
                parameters.Add(new SQLiteParameter("CreatedTime", reportnfo.CreatedTime));
                parameters.Add(new SQLiteParameter("LastModifiedBy", reportnfo.LastModifiedBy));
                parameters.Add(new SQLiteParameter("LastModifiedTime", reportnfo.LastModifiedTime));
                parameters.Add(new SQLiteParameter("SPWebTimeZoneID", reportnfo.SPWebTimeZoneName));
                parameters.Add(new SQLiteParameter("LifecycleStatus", reportnfo.LifecycleStatus));
                parameters.Add(new SQLiteParameter("CurrentHeldBy", reportnfo.CurrentHeldBy));
                parameters.Add(new SQLiteParameter("Box", reportnfo.Box));
                parameters.Add(new SQLiteParameter("HomeLocation", reportnfo.HomeLocation));
                parameters.Add(new SQLiteParameter("Availablity", reportnfo.Availablity));
                parameters.Add(new SQLiteParameter("ObjectLevel", report.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", report.TitleOrName));
                parameters.Add(new SQLiteParameter("Url", report.Url));

            }
            else if (report is DueDisposalReport)
            {
                DueDisposalReport reportnfo = report as DueDisposalReport;
                parameters.Add(new SQLiteParameter("BCSTermId", reportnfo.BCSTermId));
                parameters.Add(new SQLiteParameter("BCSTermName", reportnfo.BCSTermName));
                parameters.Add(new SQLiteParameter("AppliedRuleId", reportnfo.AppliedRuleId));
                parameters.Add(new SQLiteParameter("AppliedRuleName", reportnfo.AppliedRuleName));
                parameters.Add(new SQLiteParameter("DisposalAction", reportnfo.DisposalAction));
                parameters.Add(new SQLiteParameter("CreatedBy", reportnfo.CreatedBy));
                parameters.Add(new SQLiteParameter("CreatedTime", reportnfo.CreatedTime));
                parameters.Add(new SQLiteParameter("LastModifiedBy", reportnfo.LastModifiedBy));
                parameters.Add(new SQLiteParameter("LastModifiedTime", reportnfo.LastModifiedTime));
                parameters.Add(new SQLiteParameter("SPWebTimeZoneID", reportnfo.SPWebTimeZoneName));
                parameters.Add(new SQLiteParameter("ManualApproval", reportnfo.ManualApproval));
                parameters.Add(new SQLiteParameter("ExportType", reportnfo.ExportType));
                parameters.Add(new SQLiteParameter("Status", reportnfo.Status));
                parameters.Add(new SQLiteParameter("Comment", reportnfo.Comment));
                parameters.Add(new SQLiteParameter("LifecycleStatus", reportnfo.LifecycleStatus));
                parameters.Add(new SQLiteParameter("CurrentHeldBy", reportnfo.CurrentHeldBy));
                parameters.Add(new SQLiteParameter("Box", reportnfo.Box));
                parameters.Add(new SQLiteParameter("HomeLocation", reportnfo.HomeLocation));
                parameters.Add(new SQLiteParameter("Availablity", reportnfo.Availablity));
                parameters.Add(new SQLiteParameter("ObjectLevel", report.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", report.TitleOrName));
                parameters.Add(new SQLiteParameter("SiteCollectionTitle", reportnfo.SiteCollectionTitle));
                parameters.Add(new SQLiteParameter("Url", report.Url));
                parameters.Add(new SQLiteParameter("RelatedRecords", reportnfo.RelatedRecords));
                parameters.Add(new SQLiteParameter("RelatedRecordsAction", reportnfo.RelatedRecordsAction));
                parameters.Add(new SQLiteParameter("DisposalClass", reportnfo.DisposalClass));
            }
            else if (report is CreateAndDestroyedFileReport)
            {
                CreateAndDestroyedFileReport reportnfo = report as CreateAndDestroyedFileReport;
                parameters.Add(new SQLiteParameter("BCSTermName", reportnfo.TermName));
                parameters.Add(new SQLiteParameter("OperationBy", reportnfo.OperationBy));
                parameters.Add(new SQLiteParameter("OperationTime", reportnfo.OperationTime));
                parameters.Add(new SQLiteParameter("LifecycleStatus", reportnfo.LifecycleStatus));
                parameters.Add(new SQLiteParameter("CurrentHeldBy", reportnfo.CurrentHeldBy));
                parameters.Add(new SQLiteParameter("Box", reportnfo.Box));
                parameters.Add(new SQLiteParameter("HomeLocation", reportnfo.HomeLocation));
                parameters.Add(new SQLiteParameter("Availablity", reportnfo.Availablity));
                parameters.Add(new SQLiteParameter("ObjectLevel", reportnfo.LevelStr));
                parameters.Add(new SQLiteParameter("TitleOrName", reportnfo.Title));
                parameters.Add(new SQLiteParameter("Url", reportnfo.Url));
                parameters.Add(new SQLiteParameter("Operation", reportnfo.Operation));
                parameters.Add(new SQLiteParameter("DisposalClass", reportnfo.DisposalClass));
                parameters.Add(new SQLiteParameter("ApprovedBy", reportnfo.ApprovedBy));
                parameters.Add(new SQLiteParameter("ApprovedByUPN", reportnfo.ApprovedByUPN));
                parameters.Add(new SQLiteParameter("CreatedTime", reportnfo.CreatedTime));
                parameters.Add(new SQLiteParameter("LastModifiedTime", reportnfo.LastModifiedTime));
                parameters.Add(new SQLiteParameter("FileType", reportnfo.FileType));
                parameters.Add(new SQLiteParameter("RecordsId", reportnfo.RecordsId));
                parameters.Add(new SQLiteParameter("RuleName", reportnfo.RuleName));
                parameters.Add(new SQLiteParameter("ApprovalStatus", reportnfo.ApprovalStatus));
                parameters.Add(new SQLiteParameter("InternalApprovedStatus", reportnfo.InternalApprovedStatus));
            }

            else if (report is AvailableSpaceReport)
            {
                AvailableSpaceReport reportnfo = report as AvailableSpaceReport;
                parameters.Add(new SQLiteParameter("Location", reportnfo.Location));
                parameters.Add(new SQLiteParameter("AvailableSpace", reportnfo.AvailableSpace));
                parameters.Add(new SQLiteParameter("LocationSize", reportnfo.LocationSize));
                parameters.Add(new SQLiteParameter("InculdingContainerInfo", reportnfo.InculdingContainerInfo));
            }
            else if (report is ClientSPAuditReport)
            {
                ClientSPAuditReport reportnfo = report as ClientSPAuditReport;
                parameters.Add(new SQLiteParameter("ObjectLevel", reportnfo.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", reportnfo.TitleOrName));
                parameters.Add(new SQLiteParameter("Url", reportnfo.Url));
                parameters.Add(new SQLiteParameter("UserName", reportnfo.User));
                parameters.Add(new SQLiteParameter("EventTypeName", reportnfo.EventTypeName));
                parameters.Add(new SQLiteParameter("EventTypeI18NName", reportnfo.EventTypeI18NName));
                parameters.Add(new SQLiteParameter("Occurred", reportnfo.Occurred));
                parameters.Add(new SQLiteParameter("SiteUrl", reportnfo.SiteUrl));
                parameters.Add(new SQLiteParameter("Event", reportnfo.Event));
                parameters.Add(new SQLiteParameter("DisplayName", reportnfo.DisplayName));
                parameters.Add(new SQLiteParameter("Browser", reportnfo.Browser));
            }
            else if(report is RestoreFileReport)
            {
                RestoreFileReport reportnfo = report as RestoreFileReport;
                parameters.Add(new SQLiteParameter("ObjectLevel", reportnfo.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", reportnfo.TitleOrName));
                parameters.Add(new SQLiteParameter("SourceURL", reportnfo.Url));
                parameters.Add(new SQLiteParameter("Size", reportnfo.Size));
                parameters.Add(new SQLiteParameter("JobId", reportnfo.JobId));
                parameters.Add(new SQLiteParameter("StartTime", reportnfo.StartTime));
                parameters.Add(new SQLiteParameter("FinishTime", reportnfo.EndTime));
                parameters.Add(new SQLiteParameter("RestoreBy", reportnfo.RestoreBy));
                parameters.Add(new SQLiteParameter("RestoreTo", reportnfo.RestoreTo));
                parameters.Add(new SQLiteParameter("IsDaoMigration", reportnfo.IsDaoMigration));
                parameters.Add(new SQLiteParameter("IsEndUserOpt", reportnfo.IsEndUserOpt));
                parameters.Add(new SQLiteParameter("Status", reportnfo.Status));
                parameters.Add(new SQLiteParameter("Comment", reportnfo.Comment));
            }
            else if (report is ArchivedSiteReport)
            {
                var reportInfo = report as ArchivedSiteReport;
                parameters.Add(new SQLiteParameter("ObjectLevel", reportInfo.ObjectLevel));
                parameters.Add(new SQLiteParameter("TitleOrName", reportInfo.TitleOrName));
                parameters.Add(new SQLiteParameter("Url", reportInfo.Url));
                parameters.Add(new SQLiteParameter("Type", reportInfo.Type));
                parameters.Add(new SQLiteParameter("SourceUrl", reportInfo.SourceUrl));
                parameters.Add(new SQLiteParameter("ArchivedDataSize", reportInfo.ArchivedDataSize));
                parameters.Add(new SQLiteParameter("CreatedTime", reportInfo.CreatedTime));
                parameters.Add(new SQLiteParameter("LastModifiedTime", reportInfo.LastModifiedTime));
                parameters.Add(new SQLiteParameter("ArchivedTime", reportInfo.ArchivedTime));
            }

            //已经在需要添加的if分支中添加了 这段可以忽略
            //if (!(report is AvailableSpaceReport))
            //{
            //    parameters.Add(new SQLiteParameter("ObjectLevel", report.ObjectLevel));
            //    parameters.Add(new SQLiteParameter("TitleOrName", report.TitleOrName));
            //    parameters.Add(new SQLiteParameter("Url", report.Url));
            //}
            return parameters;
        }
    }
}
