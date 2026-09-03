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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.RetentionDisposal
{
    public class ManualApprovalReviewResult
    {
        public int TotalNumber { get; set; }

        public IEnumerable<ManualApprovalReviewDetails> Details { get; set; }
    }

    public class ManualApprovalReviewDetails
    {
        public int Id { set; get; }


        public string LeafName { get; set; }

        public string Type { get; set; }

        public string Url { get; set; }

        public int Status { get; set; }

        public string ContentType { get; set; }

        public string ModifiedBy { get; set; }

        public string CreatedBy { get; set; }

        public string RuleName { get; set; }

        public string RuleId { get; set; }

        public string Criteria { get; set; }

        public string PartKey { get; set; }

        public string RowKey { get; set; }

        public string EscalateFrom { get; set; }

        public string RecordOwner { get; set; }

        public string ApprovedBy { get; set; }

        public string Comments { get; set; }

        public string CreatedTime { get; set; }
        public List<ReportRelatedRecords> RelatedRecordsList { get; set; }
        public int RelatedRecordsAction { get; set; }
        public int SourceFlag { get; set; }
        public string DisposalClass { get; set; }

    }
}
