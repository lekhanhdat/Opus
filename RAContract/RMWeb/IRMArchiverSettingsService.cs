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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Retention;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMArchiverSettingsService
    {
        void LoadArchiverSettingIcon(List<RMSPSampleTreeNode> nodes, ScheduleType type);
        RMSPTreeNode LoadSampleNodeSettings(RMSPSampleTreeNode sNode, ScheduleType type);
        Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node);
        Task<RAReturnMessage> InheritSubNodeToCurrentAsync(RMSPTreeNode node);
        RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node);
        Task<RAReturnMessage> SaveArchiverSettingAsync(RMSPTreeNode node);
        void DisableSCArchiverManageMent(Guid siteId);
        Task<List<RMRuleInfos>> GetArchiverRuleListAsync(string containerId, SourceFlag sourceFlag);
        Task<RAReturnMessage> SaveGeneralSettingAsync(RMSPTreeNode node);
        RAReturnMessage RunArchiverJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        HSMArchiverResult RunHSMArchiverJob(HSMArchiverDto hsmDto, JobRunBy jobRunBy);
        RAReturnMessage RunTeamsArchiverJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        RAReturnMessage RunSOPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        RAReturnMessage RunODPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        RAReturnMessage RunTeamsPreScanJobWrapper(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        string RealRunArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunArchiverBackupJobOnSelectedNode(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode);
        string RealRunTeamsArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunSOPreScanJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunTeamsPreScanJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunRebuildStubJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunRebuildIndexJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunRebuildSOJobReportJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunRebuildEncryptKeyValueJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunDispatchedJob(JobRunBy jobRunBy, string jobRunByUser, JobType targetJobType, string param, string originalMessageId, string originalTenantId);

        string RealRunBuildRunningJobReportJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunExportDecryptIndexDBJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunBaseArchiveJobIdMultiRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunRebuildDeDupForWPPMigrationJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        RAReturnMessage RunArchiverMoveIndexJob(JobRunBy jobRunBy, string jobRunByUser, string sourceDeviceId, string DestinationDeviceId);
        string RealRunArchiverMoveIndexJob(JobRunBy jobRunBy, string jobRunByUser, string param);

        RAReturnMessage RunArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser);
        RAReturnMessage RunFSRetentionJob(JobRunBy jobRunBy, string jobRunByUser);
        RAReturnMessage RunTeamsArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser);
        RAReturnMessage RunEXOArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser);
        RAReturnMessage RunGDriveArchiverRetentionJob(JobRunBy jobRunBy, string jobRunByUser);
        RAReturnMessage RunDeleteOrphanDatasJob(JobRunBy jobRunBy, string jobRunByUser, List<string> needDeleteJobIds);
        RAReturnMessage RunApprovalProcessJob(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob = false, string previousJobId = "");
        Task<string> RealRunGDriveArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob = false, string previousJobId = "");
        Task<string> RealRunTeamsArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunEXOArchiverRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunFSRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool isSimulateJob = false, string previousJobId = "");
        Task<string> RealRunArchiverFullMoveRetentionJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunDeleteOrphanDatasJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunArchiverDedupJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<bool> UpdateDedupSettingFile(string fileName, Stream fileStream);
        Dictionary<string, string> GetSavedDedupFileInfo();
        Stream DownloadDedupSettingsFileToStream(out string filename);
        string DownloadDedupTemplate();

        RAReturnMessage RunArchiverDeleteRestoredDataJob(JobRunBy jobRunBy, string jobRunByUser);

        RAReturnMessage RunArchiverDeduplicationJob(JobRunBy jobRunBy, string jobRunByUser);

        string RealRunArchiverDeleteRestoredDataJob(JobRunBy jobRunBy, string jobRunByUser);

        Task<List<RMSPTreeNode>> GetApprovalProcessJobSites();

        Task<string> RealRunApprovalProcessJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunVeoMergeJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunMoveDataTierJobAsync(JobRunBy jobRunBy, string jobRunByUser, Dictionary<string, List<string>> jobidMapping);
        Task<string> RealRunAdjustStorageSizeJobAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunOptimizationJobFromManifestAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunAOSPOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunOptimizationPreScanJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunDiscoveryPlanProOptimizationJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunDiscoveryPlanProScanJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Guid GetArchiverSettingId(RMSPTreeNode node);
        Guid GetTeamsArchiverSettingId(Guid id, Guid siteId, Guid teamsId);
        void DeleteArchiverSetting(Guid ObjectId, Guid siteId);
        Task<RAReturnMessage> RunExportIndexJob();
        Task<string> RealRunExportIndexJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        string CopyPasswordAudit();
        RAReturnMessage RunSpecifySitesArchiverBackupJob(List<string> siteUrls);
        RMEndUserArchiveReturnMessage RunEndUserArchiverBackupJob(EndUserArchiveRequestParam request);
        RAReturnMessage RunSpecifyTeamsArchiverBackupJob(List<string> teamIdList);
        string RealRunSpecifySitesArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunEndUserArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunSpecifyTeamsArchiverBackupJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        bool CheckRemoteNodeHaveRunningJob(RMSPTreeNode selectedTree, List<JobType> checkJobTypes);

        bool CheckTeamsRemoteNodeHaveRunningJob(RMSPTreeNode selectedTree);
        List<RMSPTreeNode> AssembleDisposalRunnableNode(RMSPTreeNode selectedNode);
        List<RMSPTreeNode> AssembleDisposalRunnableNodeForImport(RMSPTreeNode selectedNode, List<string> importSiteUrls);
        List<RMSPTreeNode> AssembleTeamsDisposalRunnableNode(RMSPTreeNode selectedNode);

        ArchiverSettingInfo LoadChannelSampleNodeSettings(Guid scopeId, string id);

        // JobMonitor archive: initialize via job framework instead of direct execution
        RAReturnMessage RunJobMonitorArchiveJob(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RealRunJobMonitorArchiveJobAsync(JobRunBy jobRunBy, string jobRunByUser);

        Task<RetentionSettingsDto> GetRetentionSettingsAsync();
        Task<Stream> GetCurrentRetentionSettingsFileStream();
        Task<RAReturnMessage> SaveRetentionSettingsAsync(Stream fileStream, string fileName);
        Task<RAReturnMessage> RemoveRetentionSettingsAsync();
        Task<string> GetUploadedCustomRetentionSettingsFileName();

        Task<string> RealRunAPStorageCostEvaluationJobAsync(JobRunBy jobRunBy, string jobRunByUser);
    }
}
