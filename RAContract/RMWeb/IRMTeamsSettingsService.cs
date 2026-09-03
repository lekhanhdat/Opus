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
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Teams;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMTeamsSettingsService
    {
        System.Threading.Tasks.Task LoadTeamsSettingIconAsync(List<RMSPSampleTreeNode> nodes);

        List<string> GetDesignLists();
        Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode curSetting);
        bool CheckParentNodeDisable(RMSPTreeNode nodeSetting, string teamsId, string siteId, bool isCheckSelfNode = true);
        Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode node);
        System.Threading.Tasks.Task LoadSiteSettingsUnderTeamsNodeAsync(List<RMSPTreeNode> nodes, RMSPTreeNode teamsNode);
        Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddGeneralSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode);
        Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users);
        Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode);
        Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode node);
        bool CheckRunningTeamsSettingJob();
        RAReturnMessage ApplySettingsOnSelectedNode(RMSPTreeNode node);
        RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod);
        bool ExistConfiguredSettings(JobType jobType);
        bool NeedRunUniqueIdJob(List<RMSPTreeNode> needRunNodes = null);
        Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string teamsId = null, string siteId = null, string fullPath = null, JobPriority priority = JobPriority.Normal);
        void FilterSitesModified(List<RMSPTreeNode> sites, out List<RMSPTreeNode> modifiedSites);
        #region Data Synchronisation
        RAReturnMessage RunDataSyncJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        RAReturnMessage RunDataSyncScheduleJob(JobRunBy jobRunBy);
        Task<string> RealRunDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);
        #endregion
        RAReturnMessage RunRecordsDisposalJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        Task<string> RunRecordsDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode);

        Task<RAReturnMessage> UpgradeTeams(bool isUpgradeSettings);
        string RunTeamsSettingsScheduleJob(JobRunBy schedule);
        Task<string> RealTeamsSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority jobPriority = JobPriority.Normal);
        RAReturnMessage RunExportTeamsSOSettingJob(ExportSettingType type ,JobRunBy jobRunBy);
        RAReturnMessage RunExportTeamsSettingJob(ExportSettingType type ,JobRunBy jobRunBy);
        Task<string> RealRunExportTeamsSettingJobAsync(JobRunBy jobRunBy, string exportSettingType, string jobRunByUser = null);
        Task<string> RealRunExportTeamsSOSettingJobAsync(JobRunBy jobRunBy, string exportSettingType, string jobRunByUser = null);
        string RunImportTeamsSettingJob(JobRunBy jobRunBy, string extension, string blobName);
        Task<string> RealRunImportTeamsSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes);

        string RunTeamsChannelSettingConflictCheckJob();

        string RunTeamsNodeSettingUpgradeJob();

        string RunTeamsDataUpgradeJob();

        string RealRunTeamsChannelSettingConflictCheckJob(JobRunBy jobRunBy, string jobRunByUser = null);

        string RealRunTeamsDataUpgradeJob(JobRunBy jobRunBy, string jobRunByUser = null);

        string RealRunTeamsNodeSettingUpgradeJob(JobRunBy jobRunBy, string jobRunByUser = null);

        TeamsChannelConflictQueryResult GetTeamsChannelConflictsList(TeamsChannelConflictQueryParameter queryParameter);

        string RunConflictSettingDetailExportJob();
        string RealRunConflictSettingDetailExportJob(JobRunBy jobRunBy, string jobRunByUser = null, string param = null);
    }
}
