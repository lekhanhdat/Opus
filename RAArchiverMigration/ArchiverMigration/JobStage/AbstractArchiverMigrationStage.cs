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
using Aos.Sdk;
using AvePoint.Media.Service;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RADataBroker.Common;
using Cloud.sdk.Data.Opus.Migration;
using Cloud.Sdk.Dao.Services;
using Cloud.Sdk.Data.Dao;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal abstract class AbstractArchiverMigrationStage
    {
        public abstract string StageType { get; }
        public abstract int JobProgressWeight { get; }
        public abstract string JobDetailType { get; }

        protected RALogger logger;
        protected ArchiverMigrationJobExecutor JobExecutor { get; set; }
        protected JobProgressStageUpdater JobProgressUpdater => JobExecutor.ProgressStageUpdater;
        protected ArchiverMigrationJobReportManager JobReportManager => JobExecutor.JobReportManager;

        public AbstractArchiverMigrationStage()
        {
            logger = RALogger.GetInstance(this.GetType());
        }

        protected abstract Task InnerExecuteAsync();
        public abstract Task<int> GetStageProgressBaseSizeAsync();


        public virtual async Task ExecuteAsync()
        {
            logger.Info($"Start execute {StageType}");

            await PreExecuteAsync();

            await JobExecutor.ResetJobProgressUpdaterAsync(this);
            await InnerExecuteAsync();

            JobProgressUpdater.Flush();

            await PostExecuteAsync();

            logger.Info($"Finish execute {StageType}");
        }

        protected virtual Task PreExecuteAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task PostExecuteAsync()
        {
            return Task.CompletedTask;
        }

        protected Task<T> CallArchiverMigrationApi<T>(Func<IArchiverMigrationService, Task<T>> action)
        {
            return JobExecutor.DAOApiClient.ExecuteAsync((apiClient) =>
            {
                return action(apiClient.ArchiverMigrationService);
            });
        }

        protected Task<T> GetArchiverMigrationDataAsync<T>(Func<IArchiverMigrationService, Task<ArchiverMigrationData>> action, bool isDataContract = false)
        {
            return JobExecutor.DAOApiClient.GetArchiverMigrationDataAsync<T>(action, isDataContract);
        }

        public AbstractArchiverMigrationStage SetJobExecutor(ArchiverMigrationJobExecutor executor)
        {
            JobExecutor = executor;
            return this;
        }


        protected void AddJobDetail(JobDetailsStatus status, string objectName, string? comment = null)
        {
            JobReportManager.AddJobDetail(status, objectName, JobDetailType, comment);
        }

    }
}
