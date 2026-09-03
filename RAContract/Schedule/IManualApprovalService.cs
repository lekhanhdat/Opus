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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Schedule
{
    public interface IManualApprovalService
    {
        string RealRunManualApprovalJob(JobRunBy jobRunBy, string jobRunByUser);
        void UpgradeManualApprovalDataJob();

        string RunManualApprovalJob(JobRunBy jobRunBy);
        string RunManualApprovalTimerJob(JobRunBy jobRunBy);
        string RealRunManualApprovalTimerJob(JobRunBy jobRunBy, string jobRunByUser, string preGenerated = "");
        System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partionKey, string rowKey, SourceFlag sourceFlag = SourceFlag.SharePoint);
        System.Threading.Tasks.Task GenerateReportForManualApprovalReviewingAsync(string folderPath, string fileName, string sheetName, string serverUrl);

        Task<string> GesEscalateUsersAsync(string userIdString);

        ManualExportReportInfo GetDestoryItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version, bool isRetention = false);

        ManualExportReportInfo GetDestoryItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version);
        ManualExportReportInfo GetDestoryItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string rowKey);
        void MarkToExportedStatusForPhysical(Guid physicalItemId);
        void MarkToExportedStatusForBox(Guid recordId);
        ManualExportReportInfo GetPhysicalRecord(Guid id);
        ManualExportReportInfo GetBoxRecord(Guid id);
        Task<List<AccountDto>> GetUserIdsForManualJobAsync(WorkflowDefinitionDto workflowDefinition, Guid siteId);
        ManualExportReportInfo GetDestoryItemForFS(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, string rowKey);
        System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partionKey, string rowKey);
        System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusForSPOnPremAsync(string connectString, string tenantGroupId, string partionKey, string rowKey);
    }
}
