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
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMManualApproveHistory : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier", Order = 1)]
        public Guid Id { get; set; }

        [Column(TypeName = "int")]
        public RMNodeLevel Level { get; set; }

        [Column(TypeName = "int")]
        [Index(name: "idx_escalateto_source_approvedby", order: 2)]
        public SourceFlag Source { get; set; }

        [Column(TypeName = "nvarchar")]
        public string LeafName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RecordsId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string FileExtension { get; set; }

        [Column(TypeName = "nvarchar")]
        public string FullPath { get; set; }

        [Column(TypeName = "int")]
        public SOApproveDBStatus ApprovedStatus { get; set; }

        [Column(TypeName = "bigint")]
        public long ActionTime { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid RuleId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleCriteria { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleDisposalClass { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRelatedRecords { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RelatedRecords { get; set; }

        [Column(TypeName = "int")]
        public int RelatedRecordsAction { get; set; }

        [Column(TypeName = "int")]
        public int EscalateFrom { get; set; }

        [Column(TypeName = "varchar")]
        [Index(name: "idx_escalateto_source_approvedby", order: 1)]
        public string EscalateTo { get; set; }

        [Column(TypeName = "nvarchar")]
        public string EscalatedComment { get; set; }

        [Column(TypeName = "bigint")]
        public long ArchivedTime { get; set; }

        [Column(TypeName = "nvarchar")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ModifiedBy { get; set; }
        
        [Column(TypeName = "int")]
        [Index(name: "idx_escalateto_source_approvedby", order: 3)]
        public int ApprovedBy { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ExtendComment { get; set; }

        [Column(TypeName = "bigint")]
        public long CollectionTime { get; set; }

        [Column(TypeName = "int")]
        public int RetentionStatus { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ScopeId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ExplorerItemId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ManualApprovalComment { get; set; }
        [Column(TypeName = "nvarchar")]
        public string QuickReason { get; set; }
        [Column(TypeName = "nvarchar")]
        public string FolderPath { get; set; }
        [Column(TypeName = "nvarchar")]
        public string SiteUrl { get; set; }
    }
}
