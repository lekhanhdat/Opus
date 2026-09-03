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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class HistoryAddAction
    {
        private int IncrementNumber;

        private readonly string MachineId;

        private readonly RMAzureTableDataSet<RMManualApproveHistoryTableEntity> TableDBSet;

        private readonly RMRetryer Retryer;

        private readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        public HistoryAddAction()
        {
            IncrementNumber = 0;
            MachineId = Dns.GetHostName().GetHashCode().ToString();
            TableDBSet = RMRecordStorageAzureTableContext.ManualApproveHistories;
            Retryer = RMRetryerBuilder.CreateBuilder().Build();
        }

        public System.Threading.Tasks.Task AddAsync(Record item)
        {
            var entity = Convert(item);
            return AddAsync(entity);
        }

        public System.Threading.Tasks.Task AddRangeAsync(IEnumerable<Record> items)
        {
            var entities = items.ToList().ConvertAll(Convert);
            return AddRangeAsync(entities);
        }

        public System.Threading.Tasks.Task AddAsync(RMManualApproveHistoryTableEntity entity)
        {
            var rowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - new DateTime(entity.ActionTime, DateTimeKind.Utc).Ticks);
            var processId = Environment.ProcessId.ToString();
            var threadId = Environment.CurrentManagedThreadId.ToString();
            Interlocked.Increment(ref IncrementNumber);
            var rowKeyInfoes = new List<string>
            {
                rowKey,
                entity.ApprovedBy.ToString(),
                MachineId,
                processId,
                threadId,
                IncrementNumber.ToString(),
            };
            rowKey = string.Join("_", rowKeyInfoes);
            entity.PartitionKey = new DateTime(entity.ActionTime, DateTimeKind.Utc).ToString("yyyyMM");
            entity.RowKey = rowKey;
            return Retryer.RetryAsync(async () =>
            {
                await TableDBSet.Add(entity);
            });
        }

        public System.Threading.Tasks.Task AddRangeAsync(List<RMManualApproveHistoryTableEntity> entities)
        {
            var processId = Environment.ProcessId.ToString();
            var threadId = Environment.CurrentManagedThreadId.ToString();
            foreach(var entity in entities)
            {
                Interlocked.Increment(ref IncrementNumber);
                var rowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - new DateTime(entity.ActionTime, DateTimeKind.Utc).Ticks);
                var rowKeyInfoes = new List<string>
                {
                    rowKey,
                    entity.ApprovedBy.ToString(),
                    MachineId,
                    processId,
                    threadId,
                    IncrementNumber.ToString(),
                };
                rowKey = string.Join("_", rowKeyInfoes);
                entity.PartitionKey = new DateTime(entity.ActionTime, DateTimeKind.Utc).ToString("yyyyMM");
                entity.RowKey = rowKey;
            }

            return Retryer.RetryAsync(async () =>
            {
                await TableDBSet.AddRange(entities);
            });
        }

        public RMManualApproveHistoryTableEntity Convert(Record item)
        {
            return Convert(item, (SOApproveDBStatus)item.ManualApprovedStatus, item.ManualApprovedBy);
        }

        public RMManualApproveHistoryTableEntity Convert(Record item, SOApproveDBStatus approvedStatus, int approvedBy)
        {
            return Convert(item, approvedStatus, approvedBy, DateTime.UtcNow.Ticks);
        }

        public RMManualApproveHistoryTableEntity Convert(Record item, SOApproveDBStatus approvedStatus, int approvedBy, long actionTimeTicks)
        {
            var scopeId = item.ScopeId.ToString();
            if (item.SourceFlag == (int)SourceFlag.Physical)
            {
                scopeId = item.LocationId.ToString();
            }
            else if (item.SourceFlag >= 1000)
            {
                scopeId = item.ContainerId;
            }
            if ((int)approvedStatus == (int)SOApproveDBStatus.Approved) 
            {
                item.QuickReason = string.Empty;
            }
            var sourceFlag = item.IsGControlRecord ? (int) SourceFlag.GGControl : item.SourceFlag;
            return new()
            {
                Level = item.NodeType,
                Source = sourceFlag,
                RecordsId = item.RecordsId,
                LeafName = item.LeafName,
                FileExtension = item.ExtensionForFile,
                FullPath = item.ManualFullPath,
                FolderPath = item.ManualFolderPath,
                SiteUrl = item.ManualSiteUrl,
                ApprovedStatus = (int)approvedStatus,
                ActionTime = actionTimeTicks,
                RuleId = item.RuleId,
                RuleName = item.ManualRuleName,
                RuleCriteria = item.ManualRuleCriteria,
                RuleDisposalClass = item.ManualRuleDisposalClass,
                IsRelatedRecords = item.ManualIsRelatedRecords,
                RelatedRecords = item.ManualRelatedRecords,
                RelatedRecordsAction = item.ManualRelatedRecordsAction,
                EscalateFrom = item.ManualEscalateFrom,
                EscalateTo = GetReviewersHistoryAsync(item).Result,
                EscalatedComment = item.ManualEscalatedComment,
                ArchivedTime = 0,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy,
                ApprovedBy = approvedBy,
                ExtendComment = item.ManualExtendComment,
                CollectionTime = item.ManualCollectionTime,
                RetentionStatus = item.ManualRetentionStatus,
                ScopeId = scopeId,
                ExplorerItemId = item.Id,
                ManualApprovalComment = item.ManualApprovalComment,
                QuickReason = item.QuickReason,
                ModifiedTime = item.ManualModifiedTime,
                WebViewLink = item.WebViewLink
            };
        }

        public RMManualApproveHistoryTableEntity ConvertForFS(Record item, SOApproveDBStatus approvedStatus, int approvedBy, long actionTimeTicks)
        {
            var scopeId = item.ScopeId.ToString();
            if ((int)approvedStatus == (int)SOApproveDBStatus.Approved)
            {
                item.QuickReason = string.Empty;
            }
            var sourceFlag = item.IsGControlRecord ? (int)SourceFlag.GGControl : item.SourceFlag;
            return new()
            {
                Level = item.NodeType,
                Source = sourceFlag,
                RecordsId = item.RecordsId,
                LeafName = item.LeafName,
                FileExtension = item.ExtensionForFile,
                FullPath = item.ManualFullPath,
                FolderPath = item.ManualFolderPath,
                SiteUrl = item.ManualSiteUrl,
                ApprovedStatus = (int)approvedStatus,
                ActionTime = actionTimeTicks,
                RuleId = item.RuleId,
                RuleName = item.ManualRuleName,
                RuleCriteria = item.ManualRuleCriteria,
                RuleDisposalClass = item.ManualRuleDisposalClass,
                IsRelatedRecords = item.ManualIsRelatedRecords,
                RelatedRecords = item.ManualRelatedRecords,
                RelatedRecordsAction = item.ManualRelatedRecordsAction,
                EscalateFrom = item.ManualEscalateFrom,
                EscalateTo = item.ManualReviewerForHistory != null ? "|" + string.Join("|", item.ManualReviewerForHistory) + "|" : null,
                EscalatedComment = item.ManualEscalatedComment,
                ArchivedTime = 0,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy,
                ApprovedBy = approvedBy,
                ExtendComment = item.ManualExtendComment,
                CollectionTime = item.ManualCollectionTime,
                RetentionStatus = item.ManualRetentionStatus,
                ScopeId = scopeId,
                ExplorerItemId = item.Id,
                ManualApprovalComment = item.ManualApprovalComment,
                QuickReason = item.QuickReason,
                ModifiedTime = item.ManualModifiedTime,
                WebViewLink = item.WebViewLink
            };
        }

        private async Task<string?> GetReviewersHistoryAsync(Record item)
        {
            if (item.IsGControlRecord)
            {
                var reviewers = new List<int>();

                RMAccount? account = null;

                if (item.GControlCurrentApproverId.IsNotNullOrEmpty() && item.GControlCurrentApproverId != Guid.Empty.ToString())
                {
                    account = (await _accountDao.GetUserByAADIdAsync(item.GControlCurrentApproverId));

                    if (account != null) reviewers.Add(account.Id);
                }

                if (item.GControlManualReviewers.IsNotNullOrEmpty())
                {
                    reviewers.AddRange(item.GControlManualReviewers.Where(r => r != account?.Id));
                }

                return reviewers.Count > 0 ? $"|{string.Join("|", reviewers)}|" : null;
            }

            return item.ManualReviewer != null ? "|" + string.Join("|", item.ManualReviewer) + "|" : null;
        }
    }
}
