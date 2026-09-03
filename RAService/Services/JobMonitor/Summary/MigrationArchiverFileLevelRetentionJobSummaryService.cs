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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.I18N.Core.DaoMigration;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.RA.Service.Services.StorageDevice;
using System;
using System.Collections.Generic;
using System.Globalization;
using RAJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary
{
    public class MigrationArchiverFileLevelRetentionJobSummaryService : MigrationBaseJobSummaryService
    {
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        public override string[] GetSummaryAttributes()
        {
            return new string[] {
                JOBID,
                SOTRAGEDEVICE,
                STARTTIME,
                ENDTIME,

                STATUS,
                COMMENTS,
                TOTALSIZE
            };
        }

        public override (JMJobSummary, SOJob) GetSummaryBasicInfo(string jobId, GeneralSettingModel gsm)
        {
            JMJobSummary summary = new();
            SOJob jobInfo = new();
            var job = JMDao.GetJob(jobId);
            if (job != null)
            {
                ArchiverMigratedJobExtension jobExtension = null;
                try
                {
                    jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(job.AdditionalInformation);
                }
                catch (Exception e)
                {
                    logger.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");
                }
                AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(jobExtension);
                jobInfo = new()
                {
                    Id = job.Id,
                    Category = jobExtension.JobCategory,
                    PlanId = jobExtension.PlanId,
                    Type = job.JobType,
                    State = (int)ConvertArchiverJobStatusToOpus((JobStatus)job.Status),
                    StartTime = job.StartTime,
                    FinishTime = job.EndTime,
                    Scope = job.ScopeId,
                    UserName = job.UserName,
                    Detail = job.Comment,
                };
                summary = new JMJobSummary()
                {
                    JobId = jobId,
                    JobType = (RAJobType)job.JobType,
                    Scope = job.ScopeId,
                    StartTime = GeneralSettingService.ConvertTiksToDateTime(gsm, job.StartTime, true).SimplifyFormatTime,
                    EndTime = GeneralSettingService.ConvertTiksToDateTime(gsm, job.EndTime, true).SimplifyFormatTime,
                    JobRunBy = job.UserName,
                };
            }
            return (summary, jobInfo);
        }
        public override RMJobSummaryInfos GetJobSummaryInfo(BaseJobDto job, GeneralSettingModel gsm)
        {
            RMJobSummaryInfos result = new()
            {
                SummaryItem = new List<RMJobSummaryItem>(),
                JobId = job.Id,
                JobType = (RAJobType)job.Type
            };

            var jobInfoRows = GetJobInfoRows(job, gsm);
            if (!string.IsNullOrEmpty(job.PlanId))
            {
                var storage = StorageDeviceService?.GetStorageDeviceByDAOStoragePolicyId(job.PlanId);
                jobInfoRows.Insert(1, new RMJobSummaryRow { Key = SOI18NResource.Get("RM_JS_ArchiverMigration_DataType_Storage", "Storage"), Value = storage?.Name });
            }
            else
            {
                logger.Warn($"Plan id is empty of the job: {job.Id}");
            }
            result.SummaryItem.Add(new()
            {
                Title = SOI18NResource.Get("StorageOptimization.Service_Job Information", "Job Information"),
                SummaryRow = jobInfoRows
            });
            result.SummaryItem.Add(new()
            {
                Title = SOI18NResource.Get("StorageOptimization.Service_C4F5F513-C406-44A1-9AC6-38BA2C28E02D", "Retention Statistics"),
                SummaryRow = GetStaticticsForRetentionRows(job)
            });
            result.SummaryItem.Add(new()
            {
                Title = SOI18NResource.Get("StorageOptimization.Service_B5D82D5C-BDBB-4B52-952A-E335DA903E46", "Removed Stub Statistics"),
                SummaryRow = GetStaticticsForRemoveStubRows(job)
            });
            result.SummaryItem.Add(new()
            {
                Title = SOI18NResource.Get("StorageOptimization.Service_43EE71B5-CDF0-4841-AC22-ECF7B3DA79D2", "Change to Access Tiers Statistics"),
                SummaryRow = GetStaticticsForSetArchiveTierRows(job)
            });
            return result;
        }

        private List<RMJobSummaryRow> GetStaticticsForRetentionRows(BaseJobDto jobDto)
        {
            JobReportDetailEntityType[] entityTypes = new JobReportDetailEntityType[] { JobReportDetailEntityType.FileRetention };
            AbstractDaoMigrationJobDetailWorker worker = GetDetailWorker(jobDto);
            Contract.JobMonitor.BaseJobDto raJobDto = GetRABaseJobDto(jobDto);
            List<JobSummary> jobSummaries = worker.GetJobSummary(raJobDto, entityTypes);
            return GetStatisticsForRetentionRows(jobDto, entityTypes, jobSummaries);
        }

        private List<RMJobSummaryRow> GetStaticticsForRemoveStubRows(BaseJobDto jobDto)
        {
            JobReportDetailEntityType[] entityTypes = new JobReportDetailEntityType[] { JobReportDetailEntityType.RemoveStub };
            AbstractDaoMigrationJobDetailWorker worker = GetDetailWorker(jobDto);
            Contract.JobMonitor.BaseJobDto raJobDto = GetRABaseJobDto(jobDto);
            List<JobSummary> jobSummaries = worker.GetJobSummary(raJobDto, entityTypes);
            return GetStatisticsRows(jobDto, entityTypes, jobSummaries);
        }
        private List<RMJobSummaryRow> GetStaticticsForSetArchiveTierRows(BaseJobDto jobDto)
        {
            JobReportDetailEntityType[] entityTypes = new JobReportDetailEntityType[] { JobReportDetailEntityType.ChangeFileTier };
            AbstractDaoMigrationJobDetailWorker worker = GetDetailWorker(jobDto);
            Contract.JobMonitor.BaseJobDto raJobDto = GetRABaseJobDto(jobDto);
            List<JobSummary> jobSummaries = worker.GetJobSummary(raJobDto, entityTypes);
            return GetStatisticsRows(jobDto, entityTypes, jobSummaries);
        }

        protected List<RMJobSummaryRow> GetStatisticsRows(BaseJobDto jobDto, JobReportDetailEntityType[] entityTypes, List<JobSummary> jobSummaries, List<SubJobDto> inCorrectSubJobs = null)
        {
            List<RMJobSummaryRow> result = new List<RMJobSummaryRow>();

            Dictionary<string, string> jobSummaryMap = AssembleSummaryMap(jobSummaries);
            int itemCount = 0;
            int failedItemCount = 0;
            int skippedItemCount = 0;
            int filteredItemCount = 0;

            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.ItemCount)) { itemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.ItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FailedItemCount)) { failedItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FailedItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.SkippedItemCount)) { skippedItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.SkippedItemCount]); }
            if (jobSummaryMap.ContainsKey(GConstants.JobSummaryKey.FilteredItemCount)) { filteredItemCount = Convert.ToInt32(jobSummaryMap[GConstants.JobSummaryKey.FilteredItemCount]); }

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_955d4d30-3430-462f-a541-aa5b253e1aee", "Status"), Value = ConvertJobStatusToString((JobState)jobDto.State) });

            result.Add(GetComments(jobDto, entityTypes, jobSummaryMap));

            int succeedItemCount = itemCount - failedItemCount - skippedItemCount - filteredItemCount;
            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_55485CCE-D8D7-48DA-8BD8-EA91B83D08D2", "The Number of Successful Objects"), Value = succeedItemCount.ToString() });

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_BBA5B0FC-3627-4B9C-8E16-4DFE17CFB924", "The Number of Exception Objects"), Value = failedItemCount.ToString() });

            result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_875E7F3A-851A-4F86-ACB0-44E47EA57511", "Number of Skipped Objects"), Value = skippedItemCount.ToString() });

            return result;
        }

        protected List<RMJobSummaryRow> GetStatisticsForRetentionRows(BaseJobDto jobDto, JobReportDetailEntityType[] entityTypes, List<JobSummary> jobSummaries, List<SubJobDto> inCorrectSubJobs = null)
        {
            List<RMJobSummaryRow> result = new List<RMJobSummaryRow>();
            result.AddRange(GetStatisticsRows(jobDto, entityTypes, jobSummaries, inCorrectSubJobs));

            Dictionary<string, string> jobSummaryMap = AssembleSummaryMap(jobSummaries);

            if (jobDto.State == (int)JobState.Failed || jobDto.State == (int)JobState.Skiped)
            {
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_41DDD334-93A3-44BC-A4A0-D9508F1A7BFC", "Total Size"), Value = "N/A" });
            }
            else
            {
                double dataSize = 0;
                if (jobSummaryMap.ContainsKey(SOConstants.DataSize))
                {
                    dataSize = Convert.ToDouble(jobSummaryMap[SOConstants.DataSize], CultureInfo.CurrentCulture);
                }
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_88F30A4C-940C-46DB-8A67-EF6BA2EC143A", "Total Size"), Value = JobDetailHelper.GetDataSizeToView(dataSize) });
            }
            return result;
        }
    }
}
