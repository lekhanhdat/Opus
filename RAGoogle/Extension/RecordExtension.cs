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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RAGoogle.Models;
using RAGoogle.Util;


namespace RAGoogle.Extension
{
    public static class RecordExtension
    {
        public static JMArchiverActionJobDetails GenerateDisposalActionJobDetail(this Record item, string action, string ruleName, string comment)
        {
            var metaInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(item.MetaInfo);
            var detail = new JMArchiverActionJobDetails
            {
                SourceLocation = item.DirPath,
                DestinationLocation = string.Empty,
                Size = metaInfo?.FileSize.ToString(),
                FileSize = metaInfo!.FileSize,
                RuleName = ruleName,
                Level = item.NodeType == (int)RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile,
                ActionTab = (int)ActionTab.Scan,
                Action = action,
                FinishTime = DateTime.UtcNow.Ticks,
                Comment = comment
            };

            return detail;
        }

        public static JMReportJobDetails GenerateReportJobDetail(this Record record, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = record.NodeType == (int)RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.TitleOrName = record.LeafName;
            detail.Url = record.DirPath;
            detail.Comment = comments;
            return detail;
        }

        public static JMCreateAndDestroyedFileReportJobDetail GenerateCreateAndDestroyedReportJobDetail(this Record record, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = record.NodeType == (int)RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.Title = record.LeafName;
            detail.URL = record.DirPath;
            detail.Comment = comments;
            return detail;
        }

        //public static JMBoxDataSyncDetail GenerateSyncActionDetail(this Record item, string comment = "")
        //{
        //    var detail = new JMBoxDataSyncDetail();
        //    detail.ObjectName = item.LeafName;
        //    detail.FullPath = item.DirPath;
        //    detail.Comment = comment;
        //    detail.ItemType = item.NodeType == (int)RMNodeLevel.GoogleDrive ? I18NResource.DataTypeDriver : I18NResource.ObjectLevelFolder;
        //    return detail;
        //}
        public static JMGoogleDataSyncJobDetails GenerateSyncActionDetail(this Record item, string comment = "")
        {
            var detail = new JMGoogleDataSyncJobDetails();
            detail.ObjectName = item.LeafName;
            detail.FullPath = item.DirPath;
            detail.Comment = comment;
            // to-do add i18n
            detail.ItemType = item.NodeType == (int)RMNodeLevel.GoogleFile ? "RM_JS_Rule_ObjectLevel_GoogleFile" : "RM_JS_Rule_ObjectLevel_GoogleFolder";
            return detail;
        }
        public static SyncFailureItemEntity GenerateFailureItemEntity(this Record item, string jobId)
        {
            var metaInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(item.MetaInfo);
            if (metaInfo == null)
            {
                return null;
            }
            var entity = new SyncFailureItemEntity(item.ScopeId.ToString(), item.Id.ToString())
            {
                DataSource = (int)SourceFlag.Google,
                FullPath = item.DirPath,
                ParentId = item.ParentId.ToString(),
                NodeId = item.NodeId.ToString(),
                ContainerId = item.ContainerId,
                DocId = metaInfo.DocId,
                IsDirectory = item.NodeType == (int)RMNodeLevel.GoogleFolder,
                JobId = jobId,
            };

            return entity;
        }
        public static GoogleItemInfo ConvertToGoogleItemInfo(this Record record)
        {
            var metaInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(record.MetaInfo);
            return new GoogleItemInfo()
            {
                Title = record.LeafName,
                Name = record.LeafName,
                Created = new DateTime(record.TimeCreated, DateTimeKind.Utc),
                Modified = new DateTime(record.TimeModified, DateTimeKind.Utc),
                CreateByEmail = record.CreatedBy,
                ModifiedByEmail = metaInfo!.ModifiedByEmail ?? string.Empty,
                ModifiedByUserDisplayName = record.ModifiedBy,
                Size = metaInfo.FileSize,
                Path = record.DirPath,
                Id = record.Id.ToString(),
                LabelInfos = DataItemExtension.AssignLabelInfos(metaInfo.Labels)
            };
        }

        public static void RemoveManualProperties(this Record record)
        {
            record.ManualInternalApprovedStatus = (int)SOApproveDBStatus.None;
            record.ManualEmailNotificationCount = 0;
            record.ManualEmailNotificationLastTime = 0;
            record.ManualNeedEmailNotification = false;
            record.ManualExtendTime = 0;
            record.ManualExtendCount = 0;
            record.ManualExtendComment = string.Empty;
            record.ManualEscalateFrom = 0;
            record.ManualEscalatedComment = string.Empty;
            record.ManualIsAutoReassigned = false;
            record.IsManualSynced = false;
            record.ManualRuleName = string.Empty;
            record.ManualRuleCriteria = string.Empty;
            record.ManualRuleDisposalClass = string.Empty;
            record.ManualCollectionTime = 0;
            record.ManualWorkflowInstanceId = Guid.Empty;
            record.ManualWorkflowDefinitionId = Guid.Empty;
            record.ManualWorkflowStepId = Guid.Empty;
            record.ManualApprovedBy = 0;
            record.ManualApprovedStatus = (int)SOApproveDBStatus.None;
            record.ManualArchiveStatus = (int)ActionStatus.None;
            record.ManualArchivedTime = 0;
            record.ManualReviewer = [];
            record.ManualLastApproveRejectComment = string.Empty;
            record.ManualLastReviewedBy = string.Empty;
            record.ManualLastlReviewTime = 0;
            record.GControlPlatformTaskId = string.Empty;
            record.GControlApprovalProcessId = string.Empty;
            record.GControlCurrentStageId = string.Empty;
            record.GControlCurrentApproverId = string.Empty;
            record.GControlManualReviewers = [];
            record.GControlManualApprovedStatus = (int)SOApproveDBStatus.None;
            record.IsGControlRecord = false;
            record.GControlManualInternalApprovedStatus = (int)SOApproveDBStatus.None;
        }
    }
}
