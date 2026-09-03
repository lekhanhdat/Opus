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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMManualApprovalDao
    {
        Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForSharePointOnline(int pageIndex, int pageSize, string searchValue);

        Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForOneDrive(int pageIndex, int pageSize, string searchValue);

        IEnumerable<List<RMManualApprove>> GetHistoryDatas(int limit = 1000);

        IEnumerable<List<RMManualApprove>> GetUnArchiveDatas(SourceFlag source, int limit = 1000);

        IEnumerable<List<RMManualApprove>> GetNeedSyncToCosmosDbHistoryDatas(SourceFlag source, int limit = 1000);

        RMWorkflowStatus GetWorkflowInstanceStatus(Guid workflowInstanceId);
        Task<ManualApprovalFilterFolderPathResult> GetFolderPathResults(
                    ManualApprovalFilterFolderPathResult result, ManualApprovalRecordRepository repository,
                    Expression<Func<ManualApprovalRecord, bool>> predicate, Expression<Func<ManualApprovalRecord, bool>> notAdminpredicate,
                    int pageSize);
        Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForTeams(int pageIndex, int pageSize, string searchValue);
        Task<(List<ManualApprovalWorkspaceItem> Items, int Count, int SearchCount)> GetWorkspacesForGoogle(int pageIndex, int pageSize, string searchValue);
    }
}
