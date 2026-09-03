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
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core.DaoMigration;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RAJobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.JobMonitor.Summary
{
    public class MigrationArchiverRetentionJobSummaryService : MigrationBaseJobSummaryService
    {
        public override string[] GetSummaryAttributes()
        {
            return new string[] {
                JOBINFORMATION,
                JOBID,
                STARTTIME,
                ENDTIME,

                STATISTICS,
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
                jobInfo = new()
                {
                    Id = job.Id,
                    Category = jobExtension?.JobCategory ?? int.MinValue,
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
            if (GetSummaryAttributes().Contains(JOBINFORMATION))
            {
                RMJobSummaryItem jobInfoItem = new()
                {
                    Title = SOI18NResource.Get("StorageOptimization.Service_Job Information", "Job Information"),
                    SummaryRow = GetJobInfoRows(job, gsm)
                };
                result.SummaryItem.Add(jobInfoItem);
            }
            if (GetSummaryAttributes().Contains(STATISTICS))
            {
                RMJobSummaryItem statisticsItem = new()
                {
                    Title = SOI18NResource.Get("StorageOptimization.Service_4f0bd5df-04eb-432b-9c0f-a908f0d005da", "Statistics"),
                    SummaryRow = GetStaticticsRows(job)
                };
                result.SummaryItem.Add(statisticsItem);
            }
            return result;
        }

        private List<RMJobSummaryRow> GetStaticticsRows(BaseJobDto jobDto)
        {
            JobReportDetailEntityType[] entityTypes = new JobReportDetailEntityType[] { JobReportDetailEntityType.NormalInfo };
            AbstractDaoMigrationJobDetailWorker worker = GetDetailWorker(jobDto);
            if (worker != null)
            {
                Contract.JobMonitor.BaseJobDto raJobDto = GetRABaseJobDto(jobDto);
                List<JobSummary> jobSummaries = worker.GetJobSummary(raJobDto, entityTypes);
                return GetStatisticsRows(jobDto, entityTypes, jobSummaries);
            }
            return null;
        }

        protected List<RMJobSummaryRow> GetStatisticsRows(BaseJobDto jobDto, JobReportDetailEntityType[] entityTypes, List<JobSummary> jobSummaries, List<SubJobDto> inCorrectSubJobs = null)
        {
            List<RMJobSummaryRow> result = new List<RMJobSummaryRow>();

            Dictionary<string, string> jobSummaryMap = AssembleSummaryMap(jobSummaries);

            if (GetSummaryAttributes().Contains(STATUS))
            {
                result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("StorageOptimization.Service_955d4d30-3430-462f-a541-aa5b253e1aee", "Status"), Value = ConvertJobStatusToString((JobState)jobDto.State) });
            }

            if (GetSummaryAttributes().Contains(COMMENTS))
            {
                result.Add(GetComments(jobDto, entityTypes, jobSummaryMap));
            }

            if (GetSummaryAttributes().Contains(TOTALSIZE))
            {
                if (jobDto.State == (int)JobState.Failed || jobDto.State == (int)JobState.Skiped)
                {
                    result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ControlPanel.Service_Total Size", "Total Size"), Value = "N/A" });
                }
                else
                {
                    double dataSize = 0;
                    if (jobSummaryMap.ContainsKey(SOConstants.DataSize))
                    {
                        dataSize = Convert.ToDouble(jobSummaryMap[SOConstants.DataSize], CultureInfo.CurrentCulture);
                    }
                    result.Add(new RMJobSummaryRow { Key = SOI18NResource.Get("ControlPanel.Service_Total Size", "Total Size"), Value = JobDetailHelper.GetDataSizeToView(dataSize) });
                }

            }

            return result;
        }
    }
}
