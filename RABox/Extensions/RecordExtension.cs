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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RABox.Util;

namespace RABox.Converters
{
    public static class RecordExtension
    {
        public static BoxItemInfo ConvertBoxItemInfo(this Record item)
        {
            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(item.MetaInfo);
            return new BoxItemInfo()
            {
                Title = item.LeafName,
                Name = item.LeafName,
                Created = new DateTime(item.TimeCreated, DateTimeKind.Utc),
                Modified = new DateTime(item.TimeModified, DateTimeKind.Utc),
                Size = metaInfo.FileSize,
                Path = item.DirPath,
                Id = item.Id.ToString(),
            };
        }
   
        public static ManualExportReportInfo ConvertToManualExportReportForBox(this Record record)
        {
            if (record == null)
            {
                return new ManualExportReportInfo();
            }

            return new ManualExportReportInfo()
            {
                LeafName = record.LeafName,
                RuleID = record.RuleId.ToString(),
                ScopeID = record.ScopeId.ToString(),
                ParentID = record.ParentId,
                Status = (SOApproveDBStatus)record.ManualApprovedStatus,
                ObjectLevel = record.NodeType == (int)RMNodeLevel.BoxFile ? RMReportObjectLevel.BoxFile : RMReportObjectLevel.Folder,
                NodeID = record.Id,
                ArchivedTime = record.DestroyedTime,
                CreatedBy = record.CreatedBy,
                ModifiedBy = record.ModifiedBy,
                Path = record.DirPath,
                FolderPath = record.NodeType == (int)RMNodeLevel.BoxFile ? GetFolderPath(record.DirPath) : record.DirPath,
                ExportToRECO = record.ExportToRECO,
                RecordStatus = (RMRecordStatus)record.RecordStatus,
                HasRelatedDocument = record.RelatedRecordsCount,
                DeleteRelatedRecords = record.DeleteRelatedRecords,
                RelatedRecordInfo = record.RelatedRecords,
                Ancestors = record.Ancestors,
                ModifiedTime = record.ManualModifiedTime,
            };
        }

        public static JMArchiverActionJobDetails GenerateDisposalActionJobDetail(this Record item, string action, string ruleName, string comment)
        {
            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(item.MetaInfo);
            var detail = new JMArchiverActionJobDetails();
            detail.SourceLocation = item.DirPath;
            detail.DestinationLocation = item.DirPath;
            detail.Size = metaInfo?.FileSize.ToString();
            detail.FileSize = metaInfo.FileSize;
            detail.RuleName = ruleName;
            detail.Level = item.NodeType == (int)RMNodeLevel.BoxFolder ? I18NResource.DataTypeBoxFolder : I18NResource.ObjectLevelDocument;
            detail.ActionTab = (int)ActionTab.Action;
            detail.Action = action;
            detail.FinishTime = DateTime.UtcNow.Ticks;
            detail.Comment = comment;

            return detail;
        }

        public static JMBoxDataSyncDetail GenerateSyncActionDetail(this Record item, string comment = "")
        {
            var detail = new JMBoxDataSyncDetail();
            detail.ObjectName = item.LeafName;
            detail.FullPath = item.DirPath;
            detail.Comment = comment;
            detail.ItemType = item.NodeType == (int)RMNodeLevel.BoxFolder ? I18NResource.DataTypeBoxFolder : I18NResource.ObjectLevelDocument;
            return detail;
        }

        public static SyncFailureItemEntity GenerateFailureItemEntity(this Record item, string jobId)
        {
            var entity = new SyncFailureItemEntity(item.ScopeId.ToString(), item.Id.ToString())
            {
                DataSource = (int)SourceFlag.Box,
                FullPath = item.DirPath,
                ParentId = item.ParentId.ToString(),
                NodeId = item.ExternalId,
                ContainerId = item.ContainerId.ToString(),
                OwnerId = item.AveSiteId,
                IsDirectory = item.NodeType == (int)RMNodeLevel.BoxFolder,
                JobId = jobId,
            };

            return entity;
        }

        public static JMReportJobDetails GenerateReportJobDetail(this Record record, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = record.NodeType == (int)RMNodeLevel.BoxFile ? I18NResource.ObjectLevelBoxFile : I18NResource.ObjectLevelBoxFolder;
            detail.TitleOrName = record.LeafName;
            detail.Url = record.DirPath;
            detail.Comment = comments;
            return detail;
        }

        public static JMCreateAndDestroyedFileReportJobDetail GenerateCreateAndDestroyedReportJobDetail(this Record record, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = record.NodeType == (int)RMNodeLevel.BoxFile ? I18NResource.DataTypeBoxFile : I18NResource.DataTypeBoxFolder;
            detail.Title = record.LeafName;
            detail.URL = record.DirPath;
            detail.Comment = comments;
            return detail;
        }

        public static bool CheckBoxItemPropertiesChanged(this Record record, BoxItemProxy boxItemProxy, BoxTreeNode scanNode)
        {
            var boxItemFullPath = boxItemProxy.Id == scanNode.RealId || boxItemProxy.Id == BoxUtility.BoxRootFolderId ? scanNode.FullPath : boxItemProxy.CombinePath(scanNode.FullPath, boxItemProxy.FullPath);

            var modifiedBy = boxItemProxy.ModifiedBy?.Id == BoxUtility.BoxAnonymousUserId ? I18NEntity.GetString(I18NResource.BoxAnonymousUser) : boxItemProxy.ModifiedBy?.Name;

            if (boxItemProxy.Type == BoxType.file.ToString() && boxItemProxy is BoxFileProxy fileProxy)
            {
                modifiedBy = boxItemProxy.ModifiedBy?.Id == BoxUtility.BoxAnonymousUserId ?
                    string.IsNullOrEmpty(fileProxy.UploaderDisplayName) ?
                    I18NEntity.GetString(I18NResource.BoxAnonymousUser) : fileProxy.UploaderDisplayName : boxItemProxy.ModifiedBy?.Name;
            }

            if (record.LeafName != boxItemProxy.Name || record.DirPath != boxItemFullPath || record.TimeModified != boxItemProxy.Modified || record.ModifiedBy != modifiedBy)
            {
                return true;
            }

            if (boxItemProxy.Type == BoxType.file.ToString() && record.NodeType == (int)RMNodeLevel.BoxFile)
            {
                var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                var boxItemSize = boxItemProxy.Size ?? 0;

                if (metaInfo.FileSize != boxItemSize
                 )
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetFolderPath(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                return string.Empty;
            }

           var targetIndex = dirPath.LastIndexOf('\\');
            return dirPath.Substring(0, targetIndex);
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
            record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
            record.ManualArchivedTime = 0;
            record.ManualReviewer = Array.Empty<int>();
            record.ManualLastApproveRejectComment = string.Empty;
            record.ManualLastReviewedBy = string.Empty;
            record.ManualLastlReviewTime = 0;
        }
    }
}
