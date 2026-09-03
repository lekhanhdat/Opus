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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Service.Services.RMGeneralSetting;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrationCompletionStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Completion Stage";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(3, 7);

        public override string JobDetailType => throw new NotImplementedException();

        private ISettingProfileService ProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        private IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IMediaDataDao MediaDataDao => PlatformWindsorManager.GetService<IMediaDataDao>();


        protected override async Task InnerExecuteAsync()
        {
            if(JobReportManager.HasFailedDetail)
            {
                logger.Warn($"Has failed migrated data. Job will finish with exception.");
                JobProgressUpdater.Increase(3);
                return;
            }

            logger.Info($"Delete override profiles.");
            await ProfileService.DeleteOverrideProfilesAfterMigrationAsync();

            await EnableNewOpus();
            JobProgressUpdater.Increase(1);

            var dashboardJobStatus = RunDashboardCollectJob();
            logger.Info($"Run dashboard job status:{dashboardJobStatus}");
            JobProgressUpdater.Increase(1);

            logger.Info($"Update records RMRules.");
            JobExecutor.MigrateRulesStage?.FinalUpdateRecordsRMRules();
            JobProgressUpdater.Increase(1);
        }

        private async Task EnableNewOpus()
        {
            logger.Info("Disable cloud archiver.");
            await DisableCloudArchiverAsync();

            logger.Info("Enable new opus.");
            await KeyValueDao.SaveOrUpdateAsync(new RMKeyValue
            {
                Key = "RunDisposalInRecords",
                Value = true.ToString()
            });
            logger.Info("set Upgrade Opus time.");
            await KeyValueDao.SaveOrUpdateAsync(new RMKeyValue
            {
                Key = "UpgradeOpusUtcTimeTicks",
                Value = DateTime.UtcNow.Ticks.ToString()
            });

            logger.Info("Clear all media cache infoes");
            await MediaDataDao.ClearAllAsync();

            logger.Info($"waiting 3 minutes to refresh cache.");
            await Task.Delay(TimeSpan.FromMinutes(3));
        }

        private DashboardJobCreationStatus RunDashboardCollectJob()
        {
            try
            {
                var settingResult = KeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
                if (settingResult != null)
                {
                    logger.Info($"SyncArchivedSiteInfo : {settingResult.Value}");
                    KeyValueDao.Delete(settingResult);
                }
                if (DashboardService.ExistsJobQueue())
                {
                    return DashboardJobCreationStatus.ExistsJobQueue;
                }
                if (DashboardService.HasRunningJob())
                {
                    return DashboardJobCreationStatus.HasRunningJob;
                }

                var creationSuccess = DashboardService.SchduleRunDashboardJob(JobRunBy.ChangeTab);
                if (creationSuccess)
                {
                    return DashboardJobCreationStatus.Succeed;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while run dashboard collect job. Error: {e}");
            }
            return DashboardJobCreationStatus.Failed;
        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return Task.FromResult(3);
        }

        private async Task DisableCloudArchiverAsync()
        {
            await JobExecutor.DAOApiClient.ExecuteAsync((apiClient) =>
            {
                return apiClient.ArchiverService.DisableCloudArchiver();
            });
        }
    }
}
