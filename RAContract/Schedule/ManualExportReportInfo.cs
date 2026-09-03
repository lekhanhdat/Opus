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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Schedule
{
    public class ManualExportReportInfo
    {
        public int SourceFlag { get; set; }
        public string PartKey { get; set; }
        public int Level { get; set; }

        public int UIVersion { get; set; }

        public string Path { get; set; }

        public string FolderPath { get; set; }

        public string ServerRelativeUrl { get; set; }

        public string SiteUrl { get; set; }

        public string QuickReason { get; set; }

        public string ManualLastReasonForRejection { get; set; }

        public SOApproveDBStatus Status { get; set; }

        public string ScopeID { get; set; }

        public string ScanJobId { get; set; }

        public string RuleID { get; set; }

        public string RowKey { get; set; }

        public string LeafName { get; set; }

        public int ArchiveLevel { get; set; }
        public long ArchivedTime { get; set; }

        public RMReportObjectLevel ObjectLevel { set; get; }

        public string UnMD5NodeId { get; set; }

        public Guid NodeID { get; set; }

        public Guid ParentID { get; set; }

        public Guid ListID { get; set; }

        public Guid WebID { get; set; }
        public Guid RegistedSiteId { get; set; }

        public Guid SiteID { get; set; }

        public Guid SiteGroupID { get; set; }
        public string JsonMeta { get; set; }
        public string ContentType { get; set; }
        public string ModifiedBy { get; set; }
        public long ModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public ManualRuleInfo RuleInfo { get; set; }

        public int HasRelatedDocument { get; set; }
        public int DeleteRelatedRecords { get; set; }
        public string RelatedRecordInfo { get; set; }
        public Guid MailBoxID { get; set; }
        public Guid LocationID { get; set; }
        public Guid TopLocationID { get; set; }
        public bool ExportToRECO { get; set; }
        public RMRecordStatus RecordStatus { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public string DisposalClass { get; set; }
        /// <summary>
        /// 非0 代码是Retention的数据， 在Excutor里赋值
        /// </summary>
        public int RetentionStatus { set; get; }

        public long CreatedTime { get; set; }

        public List<Guid> Ancestors { get; set; }
        public SOApproveDBStatus InternalStatus { get; set; }
        public int ManualApprovalBy { set; get; }
        public int ManualEscalateFrom { set; get; }
        public long DestroyedTime { get; set; }
        public bool IsFSHighPerformanceMode { get; set; }
    }

    public enum ManualApprovalAction
    {
        Export = 0,
        Import
    }
   
    public enum ActionStatus
    {
        None = 0,
        Archiverd = 1,
        Keeped = 2,
        Moved = 4,
        
    }

    public enum FilterWorkflowStatus
    {
        None = -2,
        All = -1,
        Inprogress = 0,
        Complete = 1
    }
}
