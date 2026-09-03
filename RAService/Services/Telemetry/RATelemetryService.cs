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
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.O365Tenant;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Telemetry;
using AvePoint.RA.Contract.Telemetry;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Telemetry
{
    public class RATelemetryService : RMServiceBase, IRATelemetryService
    {
        private RALogger Logger = RALogger.GetInstance(typeof(RATelemetryService)); 
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();


        #region for archvie job
        private void MergeStatisticObject(ref MainJobExecutionProcessStatictics mainJobExecution, JobExecutionProcessStatictics subJobExecution)
        {
            if (mainJobExecution == null)
            {
                mainJobExecution = new MainJobExecutionProcessStatictics();
            }
            ++mainJobExecution.CalCulatedSubJobCount;
            if (subJobExecution == null)
            {
                return;
            }

            if (subJobExecution.ArchiveSummaryDic != null)
            {
                foreach (ArchiveSummary summary in subJobExecution.ArchiveSummaryDic.Values)
                {
                    mainJobExecution.ArchivedFileCount += summary.ArchivedFileCount;
                    mainJobExecution.ArchivedFileSize += summary.ArchivedFileSize;
                }
            }

            if (mainJobExecution.FirstSubJobStartTime.Ticks == 0 ||
                (subJobExecution.ScanSummary != null
                && subJobExecution.ScanSummary.ScanStartTime.Ticks != 0
                && mainJobExecution.FirstSubJobStartTime > subJobExecution.ScanSummary.ScanStartTime
                )
              )
            {
                mainJobExecution.FirstSubJobStartTime = subJobExecution.ScanSummary.ScanStartTime;
            }
        }

        private void SendReportJobExecutionRecordInfo(RMJobMonitor mainJob, MainJobExecutionProcessStatictics mainJobStatistic)
        {
            try
            {
                CloudRecordsReportJobExecutionRecord jobExecutionRecord = new CloudRecordsReportJobExecutionRecord();
                jobExecutionRecord.JobId = mainJob.Id;
                jobExecutionRecord.MainJobId = mainJob.Id;
                jobExecutionRecord.JobType = ((JobType)mainJob.JobType).ToString();
                jobExecutionRecord.EndTime = DateTime.UtcNow;
                jobExecutionRecord.TenantId = TenantLocalValue.LogonGroupId;
                jobExecutionRecord.JobStatus = ((JobStatus)mainJob.Status).ToString();
                string executionRecord = JsonConvert.SerializeObject(mainJobStatistic);
                jobExecutionRecord.JobExecutionRecord = executionRecord;
                object[] args = new object[1];
                args[0] = jobExecutionRecord;
                TelemetryContext.SendToQueue(TelemetryModule.ReportJobExecutionRecord, TelemetryEventType.RunJob, args);
                //TelemetryContext.FlushAsync().GetAwaiter().GetResult(); 
            }
            catch (Exception e)
            {
                Logger.Warn($"send restore job telemetry failed,error:{e}");
            }
        }

        private async Task StatisticUserSeatForArchiveStatistic(string mainJobId, MainJobExecutionProcessStatictics mainJobStatistic)
        {
            try
            {
                string o365TenantId = await RMSubJobDao.Get365TenantIdByMainJobId(mainJobId);
                mainJobStatistic.O365TenantId = o365TenantId;

                RMO365TenantSubJobController controller = new RMO365TenantSubJobController();
                RMO365TenantSubscribed subscribed = await controller.GetTenantSubscribedInfoBy365TenantId(o365TenantId);
                mainJobStatistic.UserSeats = subscribed.UserSeats;

                Dictionary<string, RMO365TenantSubJobControlDefinition> tenantSubJobControlDefinitions = await controller.GetTenantSubJobControlDefinitions(new List<RMO365TenantSubscribed> { subscribed });
                if (!tenantSubJobControlDefinitions.ContainsKey(subscribed.Id))
                {
                    Logger.Error(@$"unable statistic user seat for archvie statistic, o365tenantId:{o365TenantId}, subScribed:{subscribed}, TenantSubJobControlDefinitions:{tenantSubJobControlDefinitions}");
                    return;
                }

                var maxRunSubJobCount = controller.CalculateSubJobCount(subscribed.UserSeats, tenantSubJobControlDefinitions[subscribed.Id]);
                mainJobStatistic.MaxRunSubJobCount = maxRunSubJobCount;
            }
            catch (Exception ex) 
            {
                Logger.Error($@"Fail statistic userSeat for archvie Statistic,ex:{ex}");
            }
        }


        public async Task MergeAndUpoladArchiveMainJobStatistic(string jobId)
        {
            MainJobExecutionProcessStatictics mainJobStatistic = new MainJobExecutionProcessStatictics();
            List<RMJobContext> contexts = new List<RMJobContext>();
            try
            {
                RMJobMonitor mainJob = JobMonitorDao.GetJobById(jobId);
                if (mainJob?.JobType != (int)JobType.RecordsDisposal
                    && mainJob?.JobType != (int)JobType.OneDriveRecordsDisposal
                    && mainJob?.JobType != (int)JobType.RMArchiverBackup
                    && mainJob?.JobType != (int)JobType.RMEndUserArchiverBackup
                    && mainJob?.JobType != (int)JobType.DiscoverOptimization
                    && mainJob?.JobType != (int)JobType.DiscoveryAOSPOptimization
                    && mainJob?.JobType != (int)JobType.SpecifySitesArchiverBackup
                    && mainJob?.JobType != (int)JobType.SpecifyTeamsArchiverBackup
                    && mainJob?.JobType != (int)JobType.TeamsArchiverBackup
                    && mainJob?.JobType != (int)JobType.TeamsRecordsDisposal
                    && mainJob?.JobType != (int)JobType.GoogleRecordsDisposal
                    && mainJob?.JobType != (int)JobType.ArchiverByHSMXml
                    && mainJob?.JobType != (int)JobType.CleanUpDuplicateDatas
                    )
                {
                    return;
                }
                mainJobStatistic.MainJobId = jobId;
                mainJobStatistic.SubJobCount = mainJob.SubJobCount;
                mainJobStatistic.LastSubJobEndTime = DateTime.UtcNow;
                mainJobStatistic.JobMonitorStartTime = new DateTime(mainJob.StartTime);

                int page = 1;
                int size = 100;
                do
                {
                    contexts = await RMSubJobDao.PageQueryJobContextByMainJobId(jobId, page++, size);
                    foreach (RMJobContext context in contexts)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(context.Content))
                            {
                                Logger.Warn($"sub job:{context?.JobId} is null, may becasue it is splited db virtual subsubsub job");
                                continue;
                            }
                            JobExecutionProcessStatictics subJobStatistic = JsonConvert.DeserializeObject<JobExecutionProcessStatictics>(context.Content);
                            MergeStatisticObject(ref mainJobStatistic, subJobStatistic);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($@"Fail statistic sub job:{context?.JobId}, ex:{ex}");
                        }
                    }
                } while (contexts != null && contexts.Count() == 100);

                await StatisticUserSeatForArchiveStatistic(jobId, mainJobStatistic);

                SendReportJobExecutionRecordInfo(mainJob, mainJobStatistic);
            }
            catch (Exception e)
            {
                Logger.Error($@"Fail statis or upload main job,ex:{e}");
            }
        }
        #endregion

        #region for retention job
        public async Task AddTelemetryForRetentionJob(RARetentionJobTelemetry retentionRecord)
        {
            try
            {
                if (retentionRecord == null)
                {
                    throw new Exception("Fail set retention job telemetry, object is null");
                }
                TelemetryContext.SendToQueue(TelemetryModule.RetentionJob, TelemetryEventType.RunJob, new List<object> { ConvertToRetentionJobRecord(retentionRecord) });
                await TelemetryContext.FlushAsync();
            }
            catch (Exception e) 
            {
                Logger.Error($@"Fail add telemetry for retention job: ex:{e}");
            }
        }

        private CloudRecordsRetentionTelemetryRecord ConvertToRetentionJobRecord(RARetentionJobTelemetry telemetry)
        {
            return new CloudRecordsRetentionTelemetryRecord
            {
                JobId = telemetry.JobId,
                MainJobId = telemetry.MainJobId,
                JobType = telemetry.JobType,
                StorageName = telemetry.StorageName,
                RetentionObject = telemetry.RetentionObject,
                ArchivedSubJobId = telemetry.ArchivedSubJobId,
                MediaDataSize = telemetry.MediaDataSize,
                RetentionDataSize = telemetry.RetentionDataSize,
                RemainingMediaDataSize = telemetry.RemainingMediaDataSize,
                RetentionAction = telemetry.RetentionAction
            };
        }
        #endregion

        #region for storage cost evaluation job
        public async Task AddTelemetryForStorageCostEvaluationJobAsync(APStorageCostEvaluationJobTelemetry storageCostEvaluationRecord)
        {
            try
            {
                if (storageCostEvaluationRecord == null)
                {
                    throw new Exception("Fail set storage cost evaluation job telemetry, object is null");
                }
                TelemetryContext.SendToQueue(TelemetryModule.StorageCostEvaluation, TelemetryEventType.RunJob, new List<object> { ConvertToStorageCostEvaluationJobRecord(storageCostEvaluationRecord) });
                await TelemetryContext.FlushAsync();
            }
            catch (Exception e)
            {
                Logger.Error($@"Fail add telemetry for storage cost evaluation job: ex:{e}");
            }
        }

        private CloudRecordsCommonRecord ConvertToStorageCostEvaluationJobRecord(APStorageCostEvaluationJobTelemetry telemetry)
        {
            return new CloudRecordsStorageCostEvaluationRecord
            {
                TenantId = telemetry.TenantId,
                JobId = telemetry.JobId,
                JobType = telemetry.JobType,
                StorageId = telemetry.StorageId,
                CalculatedDate = telemetry.CalculatedDate,
                TotalArchivedSize = telemetry.TotalArchivedSizeInGB,
                TotalBlobSize = telemetry.TotalBlobSizeInGB,
                TotalUnrecordedSize = telemetry.TotalUnrecordedSizeInGB,
            };
        }
        #endregion
    }

}
