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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMManualApprove : BaseModel
    {

        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "int")]
        public int ObjectLevel { get; set; }

        [Column(TypeName = "int")]
        public int SourceFlag { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string LeafName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Url { get; set; }

        [Column(TypeName = "int")]
        public int Status { get; set; }

        [Column(TypeName = "int")]
        public int ArchiveLevel { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Version { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ContentType { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "nvarchar")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ApprovedBy { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Criteria { get; set; }

        [Column(TypeName = "nvarchar")]
        public string PartKey { get; set; }

        [Column(TypeName = "nvarchar")]
        [Index]
        public string RowKey { get; set; }

        [Column(TypeName = "int")]
        public int ActionStatus { get; set; }

        [Column(TypeName = "bigint")]
        public long CollectionTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ActionTime { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid SiteId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid NodeId { get; set; }

        public string EscalateFrom { get; set; }

        public string EscalateTo { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string Comment { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Audits { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string RelatedRecords { get; set; }

        [Column(TypeName = "int")]
        public int RelatedRecordsAction { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRelatedRecords { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid WorkflowInstanceId { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string DisposalClass { get; set; }
        [Column(TypeName = "bigint")]
        public long ExtendDispositionCustomTime { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string ExtendDispositionComment { get; set; }
    }
}
