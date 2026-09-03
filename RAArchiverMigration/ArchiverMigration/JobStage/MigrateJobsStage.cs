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
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using static AvePoint.GCommon.Contract.Server.Common.LogCollector.LogConstants;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateJobsStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Jobs Stage";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_Job";

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();


        protected override async Task InnerExecuteAsync()
        {
            //await JobMonitorService.ClearOldArchiverJobsAsync();

            List<ArchiverMigrationJobDto> archiverJobs = null;
            int fetchSize = 2000;
            int offset = 0;

            do
            {
                logger.Info($"Fetch archiver jobs from {offset}");
                archiverJobs = await FetchArchiverJobsAsync(offset, fetchSize);

                var count = archiverJobs?.Count ?? 0;
                logger.Info($"Fetched archiver jobs: {count}");

                //24, ArchiverScan
                //28, ArchiverRestore
                //29, ArchiverBackup
                //30, ArchiverMergeIndex
                //35, ArchiverRetention
                //124, ExchangeArchiverScan
                //125, ExchangeArchiverBackup
                //126, ArchiverFileLevelRetention
                //507, ArchiverDuplication
                //4000, PhysicalRecords
                var subJobs = archiverJobs.Where(j => j.JobType == 24 || j.JobType == 29 || j.JobType == 124 || j.JobType == 125 || j.JobType == 4000).ToList();
                await JobMonitorService.BulkMigrateArchiverJobs(subJobs);

                var mainJobs = archiverJobs.Where(j => j.JobType == 24 || j.JobType == 28 || j.JobType == 35 || j.JobType == 124 || j.JobType == 126 || j.JobType == 507 || j.JobType == 4000).ToList();
                foreach (var mainJob in mainJobs)
                {
                    logger.Info($"Migrate job : {mainJob.Id} - {mainJob.JobType}");
                    if (mainJob.JobType == 507)
                    {
                        mainJob.JobType = (int)JobType.ArchiverDeduplication;
                    }
                    else if (mainJob.JobType != 28 && mainJob.JobType != 35)
                    {
                        var backupSubJob = subJobs.FirstOrDefault(s => s.Id == mainJob.Id.Replace("S", "A0"));
                        if (backupSubJob != null)
                        {
                            var totalProgress = mainJob.Progress + backupSubJob.Progress;
                            var status = backupSubJob?.Status ?? -2;
                            logger.Info($"Set job[{mainJob.Id}] status by archiver status [{status}]");
                            mainJob.Status = status;
                            mainJob.EndTime = backupSubJob?.EndTime ?? 0;
                            mainJob.Comment = backupSubJob?.Comment;
                            mainJob.Progress = CalcProgress(totalProgress);
                        }
                    }
                }
                await JobMonitorService.BulkMigrateJobsAsync(mainJobs);

                JobProgressUpdater.Increase(archiverJobs.Count);

                archiverJobs.Where(j => j.JobType == 24 || j.JobType == 28 || j.JobType == 29 || j.JobType == 35 || j.JobType == 124 || j.JobType == 125 || j.JobType == 126 || j.JobType == 507 || j.JobType == 4000)
                    .ForEach(i => AddJobDetail(JobDetailsStatus.Successful, i.Id));

                offset += count;
            } while (archiverJobs != null && archiverJobs.Count >= 2000);

            //Place the update logic above to set the job status
            //await JobMonitorService.UpdateMigratedJobsInfoAsync();
        }
        private int CalcProgress(double progress)
        {
            double dProgress = progress / 2;
            if (dProgress > 0 && dProgress < 1)
            {
                return 1;
            }
            return (int)(dProgress);
        }
        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return GetArchiverMigrationDataAsync<int>((service) =>
            {
                return service.GetAllSOArchiverJobsCount();
            });
        }

        private async Task<List<ArchiverMigrationJobDto>> FetchArchiverJobsAsync(int offset, int fetchSize)
        {
            return await GetArchiverMigrationDataAsync<List<ArchiverMigrationJobDto>>((service) =>
            {
                return service.GetSOArchiverJobs(new() { Offset = offset, FetchSize = fetchSize });
            });
        }
    }
}
