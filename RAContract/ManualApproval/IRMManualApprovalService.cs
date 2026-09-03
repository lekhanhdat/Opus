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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval
{
    public interface IRMManualApprovalService
    {

        Task<ManualApprovalPaginateResult> UnderReviewQueryAsync(ManualApprovalQueryDefinition queryDefinition);

        Task<ManualApprovalPaginateResult> UnderReviewFolderViewQueryAsync(ManualApprovalQueryDefinition queryDefinition, string timeZoneId, bool isDaylight);

        Task<ManualApprovalPaginateResult> RelatedRecordsQueryAsync(ManualApprovalQueryDefinition queryDefinition);

        Task<ManualApprovalPaginateResult> ExtendQueryAsync(ManualApprovalQueryDefinition queryDefinition);

        Task<ManualApprovalPaginateResult> WaitDiposalQueryAsync(ManualApprovalQueryDefinition queryDefinition);

        Task<List<ManualApprovalItem>> HistoryAzureTableQueryAsync();
        
        Task<List<ManualApprovalItem>> HistoryAzureTableQueryForGControlAsync();

        Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptionsAsync();

        Task<ManualApprovalActionResult> ApproveAsync(ManualApprovalActionParams approveParameters, bool isFromMyhub = false);

        Task<ManualApprovalActionResult> RejectAsync(ManualApprovalActionParams approveParameters, bool isFromMyhub = false);

        Task<ManualApprovalActionResult> EscalateAsync(ManualAprovalEscalateDefinition definition);

        Task<ManualApprovalActionResult> ReassignAsync(ManualAprovalEscalateDefinition definition);

        Task<ManualApprovalActionResult> Extend(ManualApprovalExtendDefinition definition);

        Task<ManualApprovalActionResult> RestoreExtended(List<Guid> itemIds);

        Task<ManualApprovalActionResult> ChangeDiposalAction(ManualApprovalRelatedRecordsDisposalDefinition definition);

        Task<ManualApprovalActionResult> ResetManualReviewForWorkflow(List<Guid> itemIds, bool isFromGControl = false);

        Task<bool> UpdateManualApprovalSetting(ManualApprovalSettings setting);

        Task<ManualApprovalSettings> GetManualApprovalSettingsAsync();

        Task<string> RealRunEmailScheduleJobAsync(JobRunBy runBy);

        bool SchduleRunEmailScheduleJob(JobRunBy runBy);

        MAReturnMessage RunBulkActionJob(ManualApprovalJobParam param);

        Task<string> RealRunBulkActionJobAsync(string param);

        Task<ManualApprovalWorkspacePaginateResult> QueryWorkspacesAsync(ManualApprovalWorkspaceQueryDefinition queryDefinition);

        Task<bool> DisabledEscalateAsync();

        void SendUpgradeJobMessage();

        string RealRunUpgradeJob();

        RAReturnMessage RunExportHistoryDatasJob(string serviceUrl, ManualApprovalHistoryOption historyOption);

        Task<string> RealRunExportHistoryDatasJobAsync(string historyOptionStr);

        Task<RAReturnMessage> RunExportRecordsForReviewDatasJobAsync(ManualApprovalQueryDefinition queryDefinition);

        RAReturnMessage RunDeleteInvalidRecordsJob(JobRunBy jobRunBy, string jobRunByUser);
        string RealRunDeleteInvalidRecordsJob();

        Task<string> RealRunExportRecordsForReviewDatasJobAsync(string queryDefinitionStr);

        RAReturnMessage RunImportUnderReviewDatasJob(string fileName, Stream fileStream);

        Task<string> RealRunImportUnderReviewDatasJobAsync(string importParamStr);

        ManualApprovalCountResult ReadUploadFile(string fileName, Stream fileStream);

        string RealRunFileSystemManualDataUpgradeJob();

        void SendFileSystemManualDataUpgradeJobMessage();

        Task<bool> SaveApprovalCommentOptionAsync(ManualApprovalCommentInfos option);

        Task<bool> SaveApprovalSettingAsync(ManualApprovalSettingInfo settingInfo);

        Task<ManualApprovalCommentInfos> GetApprovalCommentOptionAsync();
        Task<ManualApprovalFilterFolderPathResult> QueryFolderPathAsync(ManualApprovalFolderPathQueryDefinition queryDefinition);

        Task<(bool, string)> NeedRunManualApproveJob();

        Task<bool> EnableFolderPathForDeloitte();

        Task<(bool isOnlyOneLocation, string manualSiteUrl)> EnableFolderPathForDeloitteOnlyOneLocation();

        Task<ManualApprovalSpecialReviewerResult> SpecialReviewerResult();

        Task<ManualApprovalTaskInfos> GetManualApprovalTaskInfo(string timeZoneId, bool isDaylight);

        MAReturnMessage RunFolderViewActionJob(ManualApprovalActionParams approveParameters);

        Task<string> RealRunFolderViewActionJobAsync(string importParamStr);

        Task<bool> IsHideReclassifyBtnInManualApproval();

        bool IsJpmc(bool isJpmc);
    }
}
