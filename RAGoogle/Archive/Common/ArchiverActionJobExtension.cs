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

using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Util;

namespace RAGoogle.Archive.Common
{
    public static class ArchiverActionJobExtension
    {
        public static JMArchiverActionJobDetails ConvertApproveReportToJMArchiverActionJobDetails(this ArchiveApproveReport approveReport, ActionTab action, JobDetailsStatus status,
                                                 long size, string ruleName, string message)
        {
            var nodeLevel = (NodeLevel)approveReport.SPNodeLevel;
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.Size = size.ToString();
            mArchiverActionJobDetails.FileSize = size;
            mArchiverActionJobDetails.RuleName = ruleName;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.ActionTab = (int)action;
            mArchiverActionJobDetails.Action = string.Empty ;
            mArchiverActionJobDetails.Comment = message;
            switch (nodeLevel)
            {
                case NodeLevel.GoogleMyDrive:
                case NodeLevel.GoogleSharedDrive:
                    mArchiverActionJobDetails.SourceLocation = approveReport.FullPath;
                    mArchiverActionJobDetails.Level = I18NResource.ObjectLevelGoogleDrive;
                    break;
                case NodeLevel.GoogleFolder:
                    mArchiverActionJobDetails.SourceLocation = approveReport.FullPath;
                    mArchiverActionJobDetails.Level = I18NResource.ObjectLevelFolder;
                    break;
                case NodeLevel.GoogleFile:
                    mArchiverActionJobDetails.SourceLocation = approveReport.FullPath;
                    mArchiverActionJobDetails.Level = I18NResource.ObjectLevelFile;
                    if (approveReport.CacheNodeType == (int)GoogleCacheNodeType.ItemVersion)
                    {
                        mArchiverActionJobDetails.Level = I18NResource.ObjectLevelGoogleDriveFileVersion;
                    }
                    break;
            }

            return mArchiverActionJobDetails;
        }

        public static void AddToReportsByArchiveApproveReport(this ArchiveApproveReport scanReportItem, Dictionary<ActionTab, List<JMArchiverActionJobDetails>> actionApproveReports,
                                      ActionTab action, JobDetailsStatus status, long size, string ruleName, string message)
        {
            if (!actionApproveReports.TryGetValue(action, out var bucket))
            {
                bucket = new List<JMArchiverActionJobDetails>();
                actionApproveReports[action] = bucket;
            }

            var actionDetails = scanReportItem.ConvertApproveReportToJMArchiverActionJobDetails(action, status, size, ruleName, message);
            bucket.Add(actionDetails);
        }

        public static void AddToOtherSummaryReportsByGoogleItem(this GoogleItemData googleItem, Dictionary<ActionTab, List<JMArchiverActionJobDetails>> actionApproveReports, JobDetailsStatus status, string ruleName, string message, string action)
        {
            if (!actionApproveReports.TryGetValue(ActionTab.Action, out var bucket))
            {
                bucket = new List<JMArchiverActionJobDetails>();
                actionApproveReports[ActionTab.Action] = bucket;
            }

            var actionDetails = googleItem.GenerateDisposalActionJobDetail(action, ruleName, message);
            actionDetails.Status = status;
            bucket.Add(actionDetails);
        }
        
        public static void AddToExportSummaryReportsByGoogleItem(this GoogleItemData googleItem, Dictionary<ActionTab, List<JMArchiverActionJobDetails>> actionApproveReports, JobDetailsStatus status, string ruleName, string message)
        {
            if (!actionApproveReports.TryGetValue(ActionTab.Export, out var bucket))
            {
                bucket = new List<JMArchiverActionJobDetails>();
                actionApproveReports[ActionTab.Export] = bucket;
            }

            var actionDetails = googleItem.GenerateExportDisposalActionJobDetail(I18NResource.ExportAction, ruleName, message);
            actionDetails.Status = status;
            bucket.Add(actionDetails);
        }

        public static string HandleRelativePathWithFileVersion(this string relativePath, string fileName, string versionName = null)
        {
            int index = relativePath.LastIndexOf(fileName, StringComparison.Ordinal);
            if (index < 0)
            {
                return relativePath;
            }
            string directory = relativePath.Substring(0, index);
            return !string.IsNullOrEmpty(versionName) ? directory + versionName : relativePath;
        }
    }
}
