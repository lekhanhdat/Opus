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
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMSharePointSettingsService
    {

        System.Threading.Tasks.Task AddGlobalColumnAsync(SaveTreePage setting);
        System.Threading.Tasks.Task AddCustomColumnAsync(List<RMSPTreeNode> nodes, bool isForWebAPi = false, string siteGroupId = null, bool needCheckDefaultVaule = false, int applyType = 0, bool enableRelatedRecords = false);
        System.Threading.Tasks.Task LoadSPSettingAsync(List<RMSPTreeNode> nodes);
        System.Threading.Tasks.Task LoadScheduleAsync(List<RMSPTreeNode> nodes);
        void CheckIsContainScheduleForOwnAndChildNodes(List<RMSPTreeNode> nodes);
        //string GetMetadataColumn();
        string GetMetadataColumn(Guid nodeId);
        System.Threading.Tasks.Task DeleteDirtySPSDataAsync(RMSPTreeNode current, List<RMSPTreeNode> children);
        System.Threading.Tasks.Task CheckDirtyDataAsync();
        /// <summary>
        /// 在过滤list时使用，提供需要过滤的list的name
        /// </summary>
        /// <returns></returns>
        List<string> GetDesignLists();
        // void SaveGlobalSettingExistColumn(List<RMSPTreeNode> rootNodes);
        bool IsUseExistingColumn(List<Guid> groupSpObjectIds);
        string RunSharepointSettingsScheduleJob(JobRunBy jobRunBy);
        Task<string> RealSharepointSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority jobPriority = JobPriority.Normal);
        bool CheckRunningSharePointSettingJob();
        RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod);
        RAReturnMessage ApplySettingsOnSelectedNode(RMSPTreeNode node);
        Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string siteId = null,string folderPath = null, JobPriority jobPriority = JobPriority.Normal);
        Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode sNode);

        Task<RMSPTreeNode> LoadSampleNodeSettingsByScopeId(Guid scopeId, int id);
        //bool LoadEnableRecordManagementNodeSettings(GCommon.Contract.Tree.Object.SPTreeNodeDto sNode);
        System.Threading.Tasks.Task LoadSPSettingIconAsync(List<RMSPSampleTreeNode> nodes);
        Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode);
        Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode);
        //RAReturnMessage AddNodeSettingDisposeSchedule(RMSPTreeNode node, bool isRemove = false);
        //void AddNodeSettingCollectionSchedule(RMSPTreeNode node, bool isRemove = false);
        Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node);

        System.Threading.Tasks.Task UpgradeScheduleProfileId4SharePointSettingQueryAsync();
        //string GetTreeNodeInfoByScheduleId(ScheduleType type, string Id);
        //RAReturnMessage RunCollectionJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        bool NeedRunUniqueIdJob(List<RMSPTreeNode> needRunNodes = null);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);

        void SendDeletionSyncUpgradeJobMessage();

        void SendDirtyDataDeleteJobMessage();

        string RealRunDeletionSyncUpgradeJob();

        string RealRunDirtyDataDeleteUpgradeJob();

        RAReturnMessage RunDataSyncJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);

        System.Threading.Tasks.Task LoadExchangeSettingIconAsync(List<RMSampleEXOTreeNode> nodes);
        Task<RMEXOTreeNode> LoadExchangeNodeSettingAsync(RMSampleEXOTreeNode nodes);
        bool CheckRunningEXOSettingJob();
        RAReturnMessage ApplyEXOSettings(JobRunBy jobRunBy, bool fromTimerJobPage);
        bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId,bool isCheckSelfNode = true);
        RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node);
        bool CheckEXONodeDisable(RMEXOTreeNode settingNode, bool isCheckSelfNode = true);
        Task<RAReturnMessage> SaveEXONodeSettingAsync(RMEXOTreeNode sNode);
        Task<string> RealRunApplyEXOSettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority priority = JobPriority.Normal);
        Task<RAReturnMessage> AddEnableColumnSettingAsync(RMEXOTreeNode settingNode);
        Task<RAReturnMessage> AddEXOLocationOwnersAsync(RMEXOTreeNode node);
        //RAReturnMessage AddEXONodeSettingDisposeSchedule(RMEXOTreeNode node, bool isRemove = false);
        //void AddEXONodeSettingCollectionSchedule(RMEXOTreeNode node, bool isRemove = false);
        RAReturnMessage RunEXODataSyncJob(RMEXOTreeNode selectedTree, JobRunBy jobRunBy);
        System.Threading.Tasks.Task InheritParentEXOSettingAsync(RMEXOTreeNode node);
        Task<string> RealRunEXODataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddGeneralSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddEXOGeneralSettingAsync(RMEXOTreeNode settingNode);
        Task<RAReturnMessage> AddIsSyncEXOSettingAsync(RMEXOTreeNode settingNode);

        RAReturnMessage RunSPDataSyncScheduleJob(JobRunBy jobRunBy);
        RAReturnMessage RunEXODataSyncScheduleJob(JobRunBy jobRunBy);

        Task<string> RealRunSPDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);
        Task<string> RealRunEXODataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);

        RAReturnMessage RunEXOSettingsScheduleJob(JobRunBy jobRunBy);
        Task<string> RealRunApplyEXOSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, JobPriority priority = JobPriority.Normal);
        bool ExistConfiguredSettings(JobType jobType);
        RAReturnMessage RunPhysicalDisposalJob(int location, JobRunBy jobRunBy);

        //string GetEXOTreeNodeInfoByScheduleId(ScheduleType type, string Id);
        string RunImportSPSetting(JobRunBy jobRunBy, string extension, string strBytes);
        RAReturnMessage RunExportSPSetting(ExportSettingType type, JobRunBy jobRunBy);
        RAReturnMessage RunExportSPSOSetting(ExportSettingType type, JobRunBy jobRunBy);
        Task<string> RealRunImportSPSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes);
        Task<string> RealRunExportSPSettingJob(JobRunBy jobRunBy, string jobRunByUser, string exportSettingType);
        Task<string> RealRunExportSPSOSettingJob(JobRunBy jobRunBy, string jobRunByUser, string exportSettingType);
        Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users);
        RAReturnMessage RunRecordsDisposalJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);

        RAReturnMessage RunRebuildStubJob(RebuildStubInfo rebuildStubInfo, JobRunBy jobRunBy);
        RAReturnMessage RunRebuildIndexJob(string rebuildIndexData, JobRunBy jobRunBy);
        Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RunRecordsDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode);
        Task<string> RealRunApprovalProcessJobAsync(JobRunBy jobRunBy, string jobRunByUser, List<RMSPTreeNode> nodes,JobType jobType);
        RAReturnMessage RunEXORecordsDisposalJob(RMEXOTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealRunEXORecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RealRunEXORecordsDisposalJobForApprovalAsync(JobRunBy jobRunBy, string jobRunByUser);
        Task<string> RunEXORecordDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMEXOTreeNode selectedNode);

        #region Custom index metadata
        Task<List<CustomMetadataColumnInfo>> GetAllCustomMetadataColumnInfoAsync();
        Task<List<CustomMetadataColumnInfo>> GetInUsedCustomMetadataColumnInfoAsync();

        Task<RAReturnMessage> AddOrUpdateCustomMetadataColumnAsync(List<CustomMetadataColumnInfo> customMetadataColumnInfo);

        Task<RAReturnMessage> AddOrUpdateCustomIndexMetadataAsync(CustomIndexMetadataInfo customIndexMetadataInfo, SourceFlag sourceFlag);

        Task<CustomIndexMetadataInfo> GetAllCustomIndexMetadataAsync();
        Task<CustomIndexMetadataInfo> GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag sourceFlag);

        #endregion
    }
}
