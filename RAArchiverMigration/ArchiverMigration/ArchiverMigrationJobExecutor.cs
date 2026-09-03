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
using AvePoint.RA.ArchiverMigration.ArchiverMigration;
using AvePoint.RA.ArchiverMigration.JobStage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.RADataBroker;
using Cloud.sdk.Data.Opus.Migration;

namespace AvePoint.RA.ArchiverMigration
{
    public class ArchiverMigrationJobExecutor
    {
        private const int _totalProgressRatio = 99;
        private static RALogger _logger = RALogger.GetInstance(typeof(ArchiverMigrationJobExecutor));
        private string _jobId;
        internal DAOAPIClientV1 DAOApiClient { get; private set; }
        internal StorageMigrationService StorageMigrationService { get; private set; }
        internal SPNodeMigrationService SPNodeService { get; private set; } = new();
        internal ArchiverMigrationJobSettings JobSettings { get; private set; }
        internal RMCPGlobalStorageSetting GlobalStorageSetting { get; private set; }
        internal RMAesEncryptorWrapper AesEncryptorWrapper => new();
        /// <summary>
        /// (int, string) => (rule level, rule name)
        /// </summary>
        internal Dictionary<Guid, (int, string)> RuleIdAndRuleInfoMappings { get; private set; } = new Dictionary<Guid, (int, string)>();
        /// <summary>
        /// key: archiver job id
        /// value: SourceFlag
        /// </summary>
        internal Dictionary<string, SourceFlag> ArchiverJobIdAndSourceFlagMappings { get; private set; } = new Dictionary<string, SourceFlag>();

        private JobProgressStageUpdater _progressStageUpdater;
        private ArchiverMigrationJobReportManager _jobReportManager;

        internal JobStatus JobStatus { get; private set; }
        private string _jobComment = string.Empty;

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();

        internal MigrateRulesStage? MigrateRulesStage => JobStages.FirstOrDefault(s => s is MigrateRulesStage) as MigrateRulesStage;

        private List<AbstractArchiverMigrationStage> JobStages = new()
        {
            new MigrationClearHistoryDataStage(),
            new MigrateStorageStage(),
            new MigrateMasterIndexStage(),
            new MigrateIndexSubInfoStage(),
            new MigrateSettingProfilesStage(),
            new MigrateRulesStage(),
            new MigrateRuleMappingsStage(),
            new MigrateExportSettingStage(),
            new MigrateRestoreSettingStage(),
            new MigrateJobsStage(),
            new MigrateBackendDataStage(),
            new MigrationCompletionStage()
        };

        internal JobProgressStageUpdater ProgressStageUpdater => _progressStageUpdater;
        internal ArchiverMigrationJobReportManager JobReportManager => _jobReportManager;

        public ArchiverMigrationJobExecutor(JobQueueMessage jobMsg)
        {
            _jobId = jobMsg.JobId;
            JobSettings = SerializerHelper.DeserializeByJsonConvert<ArchiverMigrationJobSettings>(jobMsg.Extension);
            DAOApiClient = new DAOAPIClientV1(true);
            StorageMigrationService = new StorageMigrationService(DAOApiClient);
            GlobalStorageSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            _jobReportManager = new(_jobId);
            _progressStageUpdater = new(_jobId, 100 - _totalProgressRatio);
        }

        public async Task RunAsync()
        {
            try
            {
                _progressStageUpdater.Increase();

                foreach (var jobStage in JobStages)
                {
                    jobStage.SetJobExecutor(this);
                    await jobStage.ExecuteAsync();
                }

                JobStatus = _jobReportManager.HasFailedDetail ? JobStatus.FinishWithException : JobStatus.Finished;
            }
            catch (Exception ex)
            {
                JobStatus = JobStatus.Failed;
                _jobComment = ex.Message;
                _logger.Error($"Job execute failed. {ex}");
            }
            finally
            {
                await OnJobFailedAsync();
                _jobReportManager.UploadReportFile();
                SetJobStatus(ArchiverMigrationJobStatus.PreparingDownloadReportBlob);
                await GenerateMigrationJobExcelReport();
                UpdateJobStatus(ArchiverMigrationJobStatus.None);
            }
        }

        private async Task GenerateMigrationJobExcelReport()
        {
            try
            {
                if (JobStatus == JobStatus.Failed || JobStatus == JobStatus.FinishWithException)
                {
                    await JobMonitorService.UploadMigrationJobReportToStorageBlob(_jobId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"General job report Sas Uri failed. {ex.Message}");
            }
        }

        private async Task OnJobFailedAsync()
        {
            if(JobStatus == JobStatus.Failed || JobStatus == JobStatus.FinishWithException)
            {
                _logger.Info($"Clear migrated history data.");
                try
                {
                    await new MigrationHistoryDataService().ClearAllMigratedHistoryData();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Clear migrated history data failed : {ex}");
                }
            }
        }

        private int _completedStagesWeight = 0;
        internal async Task ResetJobProgressUpdaterAsync(AbstractArchiverMigrationStage stageProcessor)
        {
            var sumWeights = JobStages.Sum(s => s.JobProgressWeight);
            var increasedProgress = _completedStagesWeight * _totalProgressRatio / sumWeights;

            _completedStagesWeight += stageProcessor.JobProgressWeight;
            // increased progress after current stage completed
            var newIncreasedProgress = _completedStagesWeight * _totalProgressRatio / sumWeights;
            var increasingProgress = newIncreasedProgress - increasedProgress;

            var baseSize = await stageProcessor.GetStageProgressBaseSizeAsync();
            _progressStageUpdater.MoveToNextStage(increasingProgress, baseSize);
        }

        
        internal void SetJobComment(string msg)
        {
            _jobComment = msg;
        }

        private void SetJobStatus(ArchiverMigrationJobStatus archiverMigrationJobStatus)
        {
            JobMonitorService.UpdateMigrationJobStatus(_jobId, JobStatus, _jobComment, archiverMigrationJobStatus);
        }

        private void UpdateJobStatus(ArchiverMigrationJobStatus archiverMigrationJobStatus)
        {
            JobMonitorService.UpdateMigrationJobAdditionalInformation(_jobId, archiverMigrationJobStatus);
        }
        
    }
}
