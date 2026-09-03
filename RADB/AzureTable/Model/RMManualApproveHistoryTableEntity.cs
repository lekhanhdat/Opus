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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;

namespace AvePoint.RA.DB.AzureTable.Model
{
    public class RMManualApproveHistoryTableEntity : ITableEntity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ETag ETag { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public Guid ExplorerItemId { get; set; }

        public string ScopeId { get; set; }

        // RMNodeLevel
        public int Level { get; set; }

        //SourceFlag
        public int Source { get; set; }

        public string LeafName { get; set; }

        public string RecordsId { get; set; }

        public string FileExtension { get; set; }

        public string FullPath { get; set; }

        //SOApproveDBStatus
        public int ApprovedStatus { get; set; }

        public long ActionTime { get; set; }

        public Guid RuleId { get; set; }

        public string RuleName { get; set; }

        public string RuleCriteria { get; set; }

        public string RuleDisposalClass { get; set; }

        public bool IsRelatedRecords { get; set; }

        public string RelatedRecords { get; set; }

        public int RelatedRecordsAction { get; set; }

        public int EscalateFrom { get; set; }

        public string EscalateTo { get; set; }

        public string EscalatedComment { get; set; }

        public long ArchivedTime { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public long ModifiedTime { get; set; }

        public int ApprovedBy { get; set; }

        public string ExtendComment { get; set; }

        public long CollectionTime { get; set; }

        public int RetentionStatus { get; set; }

        public string ManualApprovalComment { get; set; }
        public string QuickReason { get; set; }

        public string FolderPath { get; set; }
        public string SiteUrl { get; set; }
        public string WebViewLink { get; set; }
    }
}
