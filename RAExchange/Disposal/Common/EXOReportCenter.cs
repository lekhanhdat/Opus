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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAExchange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.Common
{
    public class EXOReportCenter
    {
        public ActionStatistics ScanActionStatistics;
        public ActionStatistics BackupActionStatistics;
        public ActionStatistics DeleteActionStatistics;
        public ActionStatistics OtherActionStatistics;
        private ActionStatistics ExportActionStatistics;
        private ActionStatistics RestoreActionStatistics;
        private static readonly object lockObj = new object();
        public IRMReportManager _reportManager { get; set; }
        public EXOReportCenter(IRMReportManager reportManager)
        {
            _reportManager = reportManager;
        }
        public void CommitDisposalAnalysis()
        {
            JMSOSummaryDetails summaryDetails = new JMSOSummaryDetails();
            summaryDetails.ActionStatistics = new List<ActionStatistics>();
            if (ScanActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ScanActionStatistics);
            }
            if (BackupActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(BackupActionStatistics);
            }
            if (DeleteActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(DeleteActionStatistics);
            }
            if (ExportActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(ExportActionStatistics);
            }
            if (OtherActionStatistics != null)
            {
                summaryDetails.ActionStatistics.Add(OtherActionStatistics);
            }
            if (summaryDetails.ActionStatistics.Count > 0)
            {
                _reportManager.SendJobDetail(summaryDetails);
            }
        }
        public void AddReportRecord(JMJobDetails detail)
        {
            _reportManager.SendJobDetail(detail);
            if (detail is JMArchiverActionJobDetails archiveDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(archiveDetail.Level);
                AnalyzeDetailsForSummary(archiveDetail.Size, nodeType, detail.Status, (ActionTab)archiveDetail.ActionTab);
            }
            else if(detail is JMEXOEnforceRuleActionJobDetails exoLCDetail)
            {
                var nodeType = ConvertI18nToStatisticsLevel(exoLCDetail.ItemType);
                AnalyzeDetailsForSummary("0", nodeType, detail.Status, ParseActionTab(exoLCDetail.Action));
            }
        }

        private static ActionTab ParseActionTab(string action)
        {
            switch (action)
            {
                case "RM_JM_Tab_DetailFilter_Scan":
                case "RM_EXODisposal_Action_Scan":
                    return ActionTab.Scan;
                case "RM_EXODisposal_Action_Export":
                    return ActionTab.Export;
                case "RM_EXODisposal_Action_Delete":
                    return ActionTab.Delete;
                case "RM_EXODisposal_Action_Move":
                case "RM_EXODisposal_Action_Keep":
                    return ActionTab.Action;
                default:
                    return ActionTab.None;
            }
        }

        private static StatisticsLevel ConvertI18nToStatisticsLevel(string i18nStr)
        {
            switch (i18nStr)
            {
                case "RM_Archiver_JobDetailTeamsGroupLevel":
                    return StatisticsLevel.TeamsGroup;
                case "RM_Archiver_JobDetailChannelLevel":
                    return StatisticsLevel.Channel;
                case "RM_Archiver_JobDetailChannelConversationLevel":
                    return StatisticsLevel.ChannelConversation;
                case "RM_Archiver_JobDetailGroupMailboxLevel":
                case "RM_EXO_LevelType_ExchangeOnlineMailbox":
                    return StatisticsLevel.GroupMailbox;
                case "RM_Archiver_JobDetailGroupMailboxItemLevel":
                case "RM_EXO_LevelType_ExchangeOnlineItem":
                    return StatisticsLevel.GroupMailboxItem;
                case "RM_EXO_LevelType_ExchangeOnlineFolder":
                    return StatisticsLevel.GroupMainboxFolder;
                case "RM_JS_Rule_ObjectLevel_SiteCollection":
                    return StatisticsLevel.SiteCollection;
                case "RM_JS_Rule_ObjectLevel_Site":
                    return StatisticsLevel.Site;
                case "RM_JS_Rule_ObjectLevel_List":
                    return StatisticsLevel.List;
                case "RM_JS_Rule_ObjectLevel_Folder":
                    return StatisticsLevel.Folder;
                case "RM_JS_Rule_ObjectLevel_Item":
                    return StatisticsLevel.Item;
                case "RM_Archiver_JobDetailPlanLevel":
                    return StatisticsLevel.Plan;
                case "RM_Archiver_JobDetailTaskLevel":
                    return StatisticsLevel.Task;
                case "RM_JS_Rule_ObjectLevel_Attachment":
                case "RM_Archiver_JobDetailConversationLevel":
                case "RM_Archiver_JobDetailEventLevel":
                    return StatisticsLevel.GroupMailboxItem;
                default:
                    return StatisticsLevel.None;
            }
        }
        private void AnalyzeDetailsForSummary(string nodeSizeStr, StatisticsLevel cacheNodeType, JobDetailsStatus status, ActionTab actionTab)
        {
            if (!long.TryParse(nodeSizeStr, out long nodeSize))
            {
                nodeSize = 0;
            }
            switch (actionTab)
            {
                case ActionTab.Scan:
                    AnalyzeScanDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Export:
                    AnalyzeExportDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Backup:
                    AnalyzeBackUpDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Delete:
                    AnalyzeDeleteDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                case ActionTab.Action:
                    AnalyzeOtherDetailsForSummary(nodeSize, cacheNodeType, status);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeDeleteDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (DeleteActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (DeleteActionStatistics == null)
                    {
                        DeleteActionStatistics = new ActionStatistics();
                        DeleteActionStatistics.ActionTab = (int)ActionTab.Delete;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    DeleteActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(DeleteActionStatistics, cacheNodeType, status);
            }
        }

        private void AnalyzeBackUpDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (BackupActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (BackupActionStatistics == null)
                    {
                        BackupActionStatistics = new ActionStatistics();
                        BackupActionStatistics.ActionTab = (int)ActionTab.Backup;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    BackupActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(BackupActionStatistics, cacheNodeType, status);
            }
        }
        private void AnalyzeExportDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (ExportActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ExportActionStatistics == null)
                    {
                        ExportActionStatistics = new ActionStatistics();
                        ExportActionStatistics.ActionTab = (int)ActionTab.Export;
                    }
                }
            }
            if (status == JobDetailsStatus.Successful)
            {
                ExportActionStatistics.Size += nodeSize;
            }
            AnalyzeStatusForSummary(ExportActionStatistics, cacheNodeType, status);
        }
        private void AnalyzeStatusForSummary(ActionStatistics sta, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    AnalyzeObjCount(sta.SuccessfulObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Skipped:
                    AnalyzeObjCount(sta.SkippedObj, cacheNodeType);
                    break;
                case JobDetailsStatus.Failed:
                    AnalyzeObjCount(sta.FailedObj, cacheNodeType);
                    break;
                default:
                    break;
            }
        }

        private void AnalyzeObjCount(ObjectStatistic objSta, StatisticsLevel cacheNodeType)
        {
            switch (cacheNodeType)
            {
                case StatisticsLevel.TeamsGroup:
                    objSta.TeamsGroupCount++;
                    break;
                case StatisticsLevel.Channel:
                    objSta.ChannelCount++;
                    break;
                case StatisticsLevel.ChannelConversation:
                    objSta.ChannelConversationCount++;
                    break;
                case StatisticsLevel.GroupMailbox:
                    objSta.GroupMailboxCount++;
                    break;
                case StatisticsLevel.GroupMailboxItem:
                    objSta.GroupMailboxItemCount++;
                    break;
                case StatisticsLevel.GroupMainboxFolder:
                    objSta.GroupMailboxFolderCount++;
                    break;
                case StatisticsLevel.SiteCollection:
                    objSta.SiteCollectionCount++;
                    break;
                case StatisticsLevel.Site:
                    objSta.SiteCount++;
                    break;
                case StatisticsLevel.List:
                    objSta.ListCount++;
                    break;
                case StatisticsLevel.Folder:
                    objSta.FolderCount++;
                    break;
                case StatisticsLevel.Item:
                    objSta.ItemCount++;
                    break;
                case StatisticsLevel.Plan:
                    objSta.PlanCount++;
                    break;
                case StatisticsLevel.Task:
                    objSta.TaskCount++;
                    break;
                case StatisticsLevel.Attachment:
                    objSta.AttachmentCount++;
                    break;
                case StatisticsLevel.Exception:
                    objSta.ExceptionCount++;
                    break;
                default:
                    break;
            }
        }
        private void AnalyzeOtherDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (OtherActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (OtherActionStatistics == null)
                    {
                        OtherActionStatistics = new ActionStatistics();
                        OtherActionStatistics.ActionTab = (int)ActionTab.Action;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    OtherActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(OtherActionStatistics, cacheNodeType, status);
            }
        }
        private void AnalyzeScanDetailsForSummary(long nodeSize, StatisticsLevel cacheNodeType, JobDetailsStatus status)
        {
            if (ScanActionStatistics == null)
            {
                lock (lockObj)
                {
                    if (ScanActionStatistics == null)
                    {
                        ScanActionStatistics = new ActionStatistics();
                        ScanActionStatistics.ActionTab = (int)ActionTab.Scan;
                    }
                }
            }
            lock (lockObj)
            {
                if (status == JobDetailsStatus.Successful)
                {
                    ScanActionStatistics.Size += nodeSize;
                }
                AnalyzeStatusForSummary(ScanActionStatistics, cacheNodeType, status);
            }
        }

        public void AddRecordDetail(JMJobDetails detail)
        {
            _reportManager.SendJobDetail(detail);
        }
    }
}
