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
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IFSAuditSinkService
    {
        System.Threading.Tasks.Task FlushAsync(FSAuditRecord record);
        System.Threading.Tasks.Task FlushAsync(List<FsRecordProcessDto> records);
        System.Threading.Tasks.Task FlushAsync(List<RMFileSystemAudit> records);
        System.Threading.Tasks.Task RCCFlushAsync(RCCReportRequest request, string jobId);
        System.Threading.Tasks.Task ApproveOrRejectFlushAsync(List<ManualApprovalFSAuditRecordDto> records);
        System.Threading.Tasks.Task PauseOrResumeFlushAsync(List<ManualApprovalFSAuditRecordDto> records);
        System.Threading.Tasks.Task MyhubReportContentFlushAsync(List<RMMyhubReportAuditItem> records, int auditType, int reportType);
        Task<(List<FSAuditRecord> Items, int TotalCount)> QueryAsync(List<FSAuditQueryFilter> filters, int? skip = null, int? take = null, FSAuditQueryOrder order = null);
        System.Threading.Tasks.Task BulkInsertAsync(IReadOnlyList<FSAuditRecord> records);
        Dictionary<int, string> FetchAllAuditTypes();
        List<string> FetchAllAuditUsers();
    }
}
