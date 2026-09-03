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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMFileSystemSettingsService
    {
        Task<RMFSTreeNode> LoadFSNodeSettingAsync(RMFSTreeNode sNode, bool loadLocalInfo = false);
        Task<bool> LoadFSNodeEnableRecordManagement(Guid nodeId);
        Task<bool> CheckFullPathConnectionAsync(RMFSTreeNode sNode);
        void LoadFSSettingIcon(List<RMFSTreeNode> nodes);
        System.Threading.Tasks.Task AddFSLocationOwnersAsync(RMFSTreeNode node);
        System.Threading.Tasks.Task SaveFSNodeSettingAsync(RMFSTreeNode sNode);
        System.Threading.Tasks.Task<RAReturnMessage> SaveFSGeneralSetting4JPMC(RMFSTreeNode sNode);
        System.Threading.Tasks.Task SaveFSActiveSettingAsync(RMFSTreeNode sNode);
        System.Threading.Tasks.Task InheritFSParentSettingAsync(RMFSTreeNode node);
        Task<RAReturnMessage> SaveClassCodePolicyAsync(ClassCodePolicyInfo classCodePolicyInfo);
        Task<RAReturnMessage> MyhubSaveClassCodePolicyAsync(ClassCodePolicyInfo classCodePolicyInfo, RMMyhubClassifyQueryInfo queryInfo);
        bool ResetApplyExistingOption(Guid scopeId);
        Task<RAReturnMessage> RunDataSyncJobAsync(RMFSTreeNode selectedTree, JobRunBy jobRunBy);
        RAReturnMessage RunImportFSSettingJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        RAReturnMessage RunExportFSSettingJob(JobRunBy jobRunBy);
        Task<string> RealRunImportFSSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes);
        Task<string> RealRunExportFSSettingJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunFSDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> GetJobMessageAsync(string subJobId);
        Task<string> GetRetentionUnitAsync(ApplyClassCodeSettingDto jobInfo);
        Task<string> GetDisposalJobMessageAsync(string subJobId);
        Task<string> GetFSRestoreJobMessageAsync(string subJobId);
        Task<string> GetFSRetainJobMessageAsync(string subJobId);
        Task<string> GetFSDiscoveryJobMessageAsync(string subJobId);
        bool CheckFSNodeSettingExist(List<Guid> connectionIds);
        RAReturnMessage RunFSDataSyncScheduleJob(JobRunBy jobRunBy);
        Task<string> RealRunDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunFSRestoreJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunDisposalJobForApprovalAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RunFSDisposalScheduleJobAsync(RMFSTreeNode treeNode, JobRunBy jobRunBy);
        Task<RAReturnMessage> RunDisposalJobAsync(RMFSTreeNode selectedTree, JobRunBy jobRunBy);
        Task<RAReturnMessage> RunApplyClassCodeJobAsync(ApplyClassCodeSettingDto settingDto, JobRunBy jobRunBy);
        Task<string> RealRunApplyClassCodeJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        bool ResetApplyExistingOptionForRealTimeJob(string jobId);
        Task<bool> ResetApplyExistingOptionForScheduleJobAsync(string jobId);
        bool IsDeactivedNode(RMFSTreeNode node);

        int GetClassificationLevel();

        System.Threading.Tasks.Task AssembleCacheDataForDisposalAsync(Guid groupId, AvePoint.RA.Contract.Global.Object.FSJobMessage message);
        System.Threading.Tasks.Task SetClassificationLevelAsync(int classificationLevel);

        RAReturnMessage CheckNodeInfo(RMFSTreeNode node);

        public RMFSTreeNode FindConnectionLevelNode(RMFSTreeNode node);
        Task<RAReturnMessage> RunDisposalByClassCodeJobAsync(AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request, JobRunBy jobRunBy);
        Task<string> RealRunDisposalByClassCodeJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> GetDisposalByClassCodeJobMessageAsync(string subJobId);
        Task<bool> HasRunningJobOnSelectedNode(RMFSTreeNode node);
        Task<bool> HasAnyAvailableNodeForClassCodeDisposalAsync(Guid connectionGroupId, Guid nodeId, string fullPath);
        List<Guid> ValidateEnableRecordManagementNodes(List<Guid> nodeIds);
        RAReturnMessage RunDownloadRCCReportJob(RCCReportRequest request, JobRunBy jobRunBy);
        Task<string> RealRunDownloadRCCReportJobAsync(JobRunBy jobRunBy, string jobRunByUser, string requestJson);
        bool HasRunningJobOnAgentIds(List<Guid> agentIds);
    }
}
