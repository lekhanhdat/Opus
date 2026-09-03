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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval
{
    public class ManualApprovalAzureTableManager
    {
        private static IRMManualApproveHistoryDao ManualApproveHistoryDao => PlatformWindsorManager.GetService<IRMManualApproveHistoryDao>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public static RMManualApproveHistory BuildHistoryData(Record item, SOApproveDBStatus approvalStatus, int approvalAccountId, bool isManualTimerJob = false)
        {
            if (approvalStatus == SOApproveDBStatus.Approved) 
            {
                item.QuickReason = string.Empty;
            }
            return new RMManualApproveHistory
            {
                Id = Guid.NewGuid(),
                Level = (RMNodeLevel)item.NodeType,
                Source = (SourceFlag)item.SourceFlag,
                LeafName = item.LeafName,
                RecordsId = item.RecordsId,
                FileExtension = item.ExtensionForFile,
                FullPath = item.ManualFullPath,
                FolderPath = item.ManualFolderPath,
                ApprovedStatus = approvalStatus,
                ActionTime = isManualTimerJob ? item.ManualActionTime : DateTime.UtcNow.Ticks,
                RuleId = item.RuleId,
                RuleName = item.ManualRuleName,
                RuleCriteria = item.ManualRuleCriteria,
                RuleDisposalClass = item.ManualRuleDisposalClass,
                IsRelatedRecords = item.ManualIsRelatedRecords,
                RelatedRecords = item.ManualRelatedRecords,
                RelatedRecordsAction = item.ManualRelatedRecordsAction,
                EscalateFrom = item.ManualEscalateFrom,
                EscalateTo = "|" + string.Join("|", item.ManualReviewer) + "|",
                EscalatedComment = item.ManualEscalatedComment,
                ArchivedTime = 0,
                CreatedBy = item.CreatedBy,
                ModifiedBy = item.ModifiedBy,
                ApprovedBy = approvalAccountId,
                ExtendComment = item.ManualExtendComment,
                CollectionTime = item.ManualCollectionTime,
                RetentionStatus = item.ManualRetentionStatus,
                ScopeId = GetRocordsScopeId(item),
                ExplorerItemId = item.Id,
                ManualApprovalComment = item.ManualApprovalComment,
                QuickReason = item.QuickReason,
                SiteUrl = item.ManualSiteUrl,
            };
        }

        /*public static async Task<int> RemoveOldestHistoryDatasForJob()
        {
            var historyTotalCount = await ManualApproveHistoryDao.Count(new ManualApprovalHistoryDBQueryDefinition());
            if(historyTotalCount > 1000)
            {
                return await ManualApproveHistoryDao.DeleteOldestDatasForJob(historyTotalCount);
            }
            return 0;
        }*/

        public static void CreateHistory(RMManualApproveHistory historyData)
        {
            ManualApproveHistoryDao.Add(historyData);
        }

        public static void RebuildAudits(Record item, SOApproveDBStatus approvalStatus, RMAccount approvalAccount)
        {
            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = approvalAccount.DisplayName,
                Action = approvalStatus == SOApproveDBStatus.Approved ? "RM_MA_Approve" : "RM_MA_Reject",
                Comment = item.ManualApprovalComment,
                QuickReason = item.QuickReason,
            });
            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }

        public static async Task RebuildAuditsAsync(Record item, SOApproveDBStatus approvalStatus, RMAccount approvalAccount, ManualApprovalExtendType extendType, int extendNumber, DateTime customeExtendDate )
        {
            var extendTime = extendType switch
            {
                ManualApprovalExtendType.After1Month => DateTime.UtcNow.AddMonths(1),
                ManualApprovalExtendType.After3Month => DateTime.UtcNow.AddMonths(3),
                ManualApprovalExtendType.After6Month => DateTime.UtcNow.AddMonths(6),
                ManualApprovalExtendType.After1Year => DateTime.UtcNow.AddYears(1),
                ManualApprovalExtendType.Month => DateTime.UtcNow.AddMonths(extendNumber),
                ManualApprovalExtendType.Year => DateTime.UtcNow.AddYears(extendNumber),
                _ => customeExtendDate,
            };

            var extendSimplifyFormatTime = await GeneralSettingService.ConvertTiksToDateTimeAsync(extendTime.Ticks, true);

            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = approvalAccount.DisplayName,
                Action = approvalStatus == SOApproveDBStatus.Approved ? "RM_MA_Approve" : "RM_MA_ApproveStatus_RejectAndExtend", //"RM_MA_Reject"
                Comment = item.ManualApprovalComment,
                QuickReason = item.QuickReason,
                ExtendTime = approvalStatus == SOApproveDBStatus.Approved ? string.Empty : extendSimplifyFormatTime.SimplifyFormatTime.ToString(),
            });
            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }

        public static void ReBuildReassignAudits(Record item, RMAccount approvalAccount)
        {
            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = approvalAccount.DisplayName,
                Action = "RM_MA_Reassign"
            });

            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }


        private static string GetRocordsScopeId(Record item)
        {
            if (item.SourceFlag == (int)SourceFlag.Physical)
            {
                return item.LocationId.ToString();
            }
            if (item.SourceFlag >= 1000)
            {
                return item.ContainerId;
            }
            return item.ScopeId.ToString();
        }
    }
}
