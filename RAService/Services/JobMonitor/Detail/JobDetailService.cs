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

using Aspose.Pdf.Operators;
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    public class JobDetailService : IJobDetailService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(JobDetailService));
        public Dictionary<int, AbstractJobDetailWorker> jobTypeAndJobDetailWorkerDictionary { set; get; }
        private List<JMJobDetails> detailsWaiting = new List<JMJobDetails>();
        private readonly object locker = new object();
        private int sendStatus = 0;//0 init;  send +1 ; finish -1
        private IRMReportService innerRMReportservice = null;
        private BaseJobDto baseJobDto;
        private bool finalUpdate = false;
        private readonly static RASimpleLocker _simpleLocker = new RASimpleLocker();
        private IJobDetailDao JobDetailDao => PlatformWindsorManager.GetService<IJobDetailDao>();
        private readonly IJobProgressDao _jobProgressDao = PlatformWindsorManager.GetService<IJobProgressDao>();

        public void SyncJobDetails(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {

            SimpleLocker.Locker locker = _simpleLocker.GetLocker(jobInfo.Id);

            lock (locker)
            {
                try
                {
                    AbstractJobDetailWorker worker = null;
                    if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                    {
                        worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType];
                    }
                    else if (jobInfo.JobType == (int)JobType.DeleteInvalidRecords)
                    {
                        worker = new ManualApprovalJobDetailWorker();
                    }
                    else if (!string.IsNullOrEmpty(jobInfo.SiteCollectionUrl))
                    {
                        worker = new StatisticsSoJobSizeWorker();
                    }
                    if (jobInfo.IsMainJob)
                    {
                        worker = new MainJobDetailWorker();
                    }
                    ArgumentCheck.NotNull(worker, nameof(worker));
                    worker.InsertData(jobDetails, jobInfo);
                }
                catch (Exception e)
                {
                    logger.Error("{0}, {1}", e.ToString(), e.StackTrace);
                }
                finally
                {
                    //free 
                    try
                    {
                        jobDetails.ToList().Clear();
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Dispose job details error {0}", e.ToString());
                    }
                    _simpleLocker.FreeLocker(locker.Key);
                }
            }
        }

        public void UpdateJobDetails(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            baseJobDto = jobInfo;
            lock (locker)
            {
                if (jobDetails != null)
                {
                    detailsWaiting.AddRange(jobDetails);
                }
                if (detailsWaiting.Count >= 10 || finalUpdate)
                {
                    List<JMJobDetails> sends = new List<JMJobDetails>();
                    sends.AddRange(detailsWaiting);
                    detailsWaiting.Clear();
                    sendStatus++;
                    AveTenantThread updateDetails = new AveTenantThread(new ParameterizedThreadStart(DoUpdateDetails));
                    updateDetails.Start(sends);
                }
            }
        }

        private void DoUpdateDetails(object details)
        {
            List<JMJobDetails> jobDetails = (List<JMJobDetails>)details;
            try
            {
                if (jobDetails.Count > 0)
                {
                    AbstractJobDetailWorker worker = null;
                    if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(baseJobDto.JobType))
                    {
                        worker = jobTypeAndJobDetailWorkerDictionary[baseJobDto.JobType];
                    }
                    ArgumentCheck.NotNull(worker, nameof(worker));
                    worker.InsertData(jobDetails, baseJobDto);
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            finally
            {
                //free 
                try
                {
                    sendStatus--;
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose job details error {0}", e.ToString());
                }
            }
        }

        public string DownloadReports(BaseJobDto jobInfo)
        {
            string result = null;
            try
            {
                AbstractJobDetailWorker worker = null;
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                result = worker.DownloadReports(jobInfo);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return result;
        }

        public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            using var _ = new PerformanceScope($"SyncJobDetails limit {PageSize} offset {(StartPage - 1) * PageSize}");
            IEnumerable<JMJobDetails> result = null;
            try
            {
                if (!jobTypeAndJobDetailWorkerDictionary.TryGetValue(jobInfo.JobType, out var worker))
                {
                    ArgumentCheck.NotNull(worker, nameof(worker));
                }
                result = worker.GetData(PageSize, StartPage, conditionFilter, jobInfo);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
                throw;
            }
            return result;
        }

        public bool MergeJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobInfo)
        {
            using var _ = new PerformanceScope($"mergeJobDetails");
            try
            {
                if (!jobTypeAndJobDetailWorkerDictionary.TryGetValue(sourceJobInfo.JobType, out var worker))
                {
                    ArgumentCheck.NotNull(worker, nameof(worker));
                }
                return worker.MergeJobDetails(sourceJobInfo, targetJobInfo);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public bool InsertMainJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobInfo)
        {
            using var _ = new PerformanceScope($"InsertMainJobDetails");
            try
            {
                if (!jobTypeAndJobDetailWorkerDictionary.TryGetValue(sourceJobInfo.JobType, out var worker))
                {
                    ArgumentCheck.NotNull(worker, nameof(worker));
                }
                return worker.InsertMainJobDetails(sourceJobInfo, targetJobInfo);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            using var _ = new PerformanceScope("SyncJobDetails");
            IEnumerable<JMJobDetails> result = null;
            int recordCount = 0;
            try
            {
                AbstractJobDetailWorker worker = null;
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType];
                }
                else if (jobInfo.JobType == (int)JobType.DeleteInvalidRecords)
                {
                    worker = new ManualApprovalJobDetailWorker();
                }
                if (jobInfo.IsMainJob || jobInfo.IsGettingProgress)
                {
                    worker = new MainJobDetailWorker();
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                result = worker.GetData(PageSize, StartPage, ref recordCount, conditionFilter, jobInfo);
                totalCount = recordCount;
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
                throw;
            }
            return result;
        }

        public IEnumerable<JMJobDetails> GetDataForRetentionSimulateDetails(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            using var _ = new PerformanceScope("SyncJobDetails");
            IEnumerable<JMJobDetails> result = null;
            int recordCount = 0;
            try
            {
                AbstractJobDetailWorker worker = new ArchiverRetentionDashboardDetailWorker();

                result = worker.GetData(PageSize, StartPage, ref recordCount, conditionFilter, jobInfo);
                totalCount = recordCount;
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
                throw;
            }
            return result;
        }

        public IEnumerable<JMJobDetails> GetDataForTermSelection(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            try
            {
                BCSTermUsageReportJobDetailWorker worker = null;
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType] as BCSTermUsageReportJobDetailWorker;
                    result = worker.GetDataForTermSelection(PageSize, StartPage, ref totalCount, conditionFilter, jobInfo);
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return result;
        }

        public JMJobDetails GetDataForSOSummaryDetails(string conditionFilter, BaseJobDto jobInfo)
        {
            JMJobDetails result = null;
            try
            {
                if (jobTypeAndJobDetailWorkerDictionary.TryGetValue(jobInfo.JobType, out var detailWorker))
                {
                    result = detailWorker.GetDataForJobSummaryDetails(conditionFilter, jobInfo);
                }
                else
                {
                    logger.Error($"No job details worker implement for: {jobInfo.JobType}");
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return result;
        }

        public JMJobDetails GetDataForRestoreSummaryDetails(string conditionFilter, BaseJobDto jobInfo)
        {
            JMJobDetails result = null;
            try
            {
                if (jobTypeAndJobDetailWorkerDictionary.TryGetValue(jobInfo.JobType, out var detailWorker))
                {
                    result = detailWorker.GetDataForJobSummaryDetails(conditionFilter, jobInfo);
                }
                else
                {
                    logger.Error($"No job details worker implement for: {jobInfo.JobType}");
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return result;
        }

        public void ClearSOSummaryDetails(BaseJobDto jobInfo)
        {
            try
            {
                if (jobTypeAndJobDetailWorkerDictionary.TryGetValue(jobInfo.JobType, out var detailWorker))
                {
                    detailWorker.ClearJobSummaryDetails(jobInfo);
                }
                else
                {
                    logger.Error($"No job details worker implement for: {jobInfo.JobType}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Clear summary failed: {ex}");
            }
        }

        public void SetInnerReport(IRMReportService reportService) {
            innerRMReportservice = reportService;
        }
        public void UploadJobDetailsAndReport(BaseJobDto jobInfo)
        {
            FinalUpdateAndWaitCompleted();
            if (innerRMReportservice != null)
            {
                innerRMReportservice.FinalUpdateAndWaitCompleted();
            }
            UploadReportFile(jobInfo);
        }
        public void UploadReportFile(BaseJobDto jobInfo)
        {
            AbstractJobDetailWorker worker = null;
            try
            {
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType];
                }
                else if (!string.IsNullOrEmpty(jobInfo.SiteCollectionUrl))
                {
                    worker = new StatisticsSoJobSizeWorker();
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                worker.UploadReports(jobInfo);
                //break;
            }
            catch (Exception e)
            {
                logger.Error("upload failed ,{0}, {1}", e.Message, e.StackTrace);
                if(jobInfo.JobType == (int)JobType.SendEmailJob)
                {
                    logger.Error("Send email job no need send job detail report,{0}, {1}", e.Message, e.StackTrace);
                    return;
                }
                Thread.Sleep(10 * 1000);
                try
                {
                      worker?.UploadReports(jobInfo);
                }
                catch (Exception ex)
                {
                    logger.Error("retry upload failed,{0}, {1}", ex.Message, ex.StackTrace);
                }
            }
        }

        public void UploadJobDetailsAndReportToTempLocation(BaseJobDto jobInfo)
        {
            FinalUpdateAndWaitCompleted();
            if (innerRMReportservice != null)
            {
                innerRMReportservice.FinalUpdateAndWaitCompleted();
            }
            UploadReportFileToTempLocation(jobInfo);
        }

        public void UploadReportFileToTempLocation(BaseJobDto jobInfo)
        {
            AbstractJobDetailWorker worker = null;
            try
            {
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = jobTypeAndJobDetailWorkerDictionary[jobInfo.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                worker.UploadReportToTempLocation(jobInfo);
                //break;
            }
            catch (Exception e)
            {
                logger.Error("upload failed ,{0}, {1}", e.Message, e.StackTrace);
                if (jobInfo.JobType == (int)JobType.SendEmailJob)
                {
                    logger.Error("Send email job no need send job detail report,{0}, {1}", e.Message, e.StackTrace);
                    return;
                }
                Thread.Sleep(10 * 1000);
                try
                {
                    worker?.UploadReportToTempLocation(jobInfo);
                }
                catch (Exception ex)
                {
                    logger.Error("retry upload failed,{0}, {1}", ex.Message, ex.StackTrace);
                }
            }
        }



        public bool SendReport(HBReportFileInfo reportInfo)
        {
            bool result = false;
            try
            {
                logger.Info($"begin to upload file:{reportInfo.JobId}, {reportInfo.FileName}");
                if (jobTypeAndJobDetailWorkerDictionary.ContainsKey(reportInfo.JobType))
                {
                    var worker = jobTypeAndJobDetailWorkerDictionary[reportInfo.JobType];
                    worker.SendReport(reportInfo);
                    result = true;
                }
                else
                {
                    throw new NotSupportedException($"jobType:{reportInfo.JobType} not supported upload file.");
                }

            }
            catch (Exception e)
            {
                logger.Error("error occurred while {1}", e.Message, e.ToString());
                throw;
            }
            return result;
        }

        public void RemoveDuplicateDataOfJobDetails(BaseJobDto jobInfo)
        {
            try
            {
                string TABLE_NAME = JobMonitorConstants.JOBDETAIL;
                string SUMMAY_TABLE_NAME = JobMonitorConstants.JOBSUMMAYDETAIL;
                string DELETE_DEDUP_DATA_SQL = string.Format(JobMonitorConstants.REMOVE_DEDUP_DATA_SharePoint_Archiver_Report, TABLE_NAME);
                string DELETE_SUMMARY_DATA_SQL = string.Format(JobMonitorConstants.CLEAN_DATA, SUMMAY_TABLE_NAME);
                string STATISTIC_DETAIL_SQL = string.Format(JobMonitorConstants.STATISTICS_SUMMAY_SharePoint_Archiver_Report, TABLE_NAME);
                string INSERT_SUMMAYDATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_SharePoint_Archiver_SUMMARYReport, SUMMAY_TABLE_NAME);
                string reportFilePath = JobReportUtility.GetJobReportPath(jobInfo, ".rpt");

                if (File.Exists(reportFilePath) && JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME))
                {
                    SQLCommond.ExecuteNonQuery(reportFilePath, DELETE_DEDUP_DATA_SQL);

                    if (JobDetailDao.IsExistTable(reportFilePath, SUMMAY_TABLE_NAME))
                    {
                        JMSOSummaryDetails summary = JobDetailDao.StatisticDiscoverPrescanSummaryFromJobDatas(reportFilePath, STATISTIC_DETAIL_SQL);
                        SQLCommond.ExecuteNonQuery(reportFilePath, DELETE_SUMMARY_DATA_SQL);
                        JobDetailDao.SaveDataIntoTable(reportFilePath, new List<JMJobDetails> { summary }, INSERT_SUMMAYDATA_SQL);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(@$"Fail Remove Duplicate Data Of Job Details,job info:{jobInfo},ex:{ex}");
            }
        }


        private void FinalUpdateAndWaitCompleted()
        {
            finalUpdate = true;
            UpdateJobDetails(null, baseJobDto);
            while (true)
            {
                if (sendStatus == 0)
                {
                    break;
                }
                Thread.Sleep(500);
            }
        }

        public async Task MigrateToRptAndDeleteAsync(string mainJobId, int jobType)
        {
            logger.Info($"Start migrate job details to rpt and delete, mainJobId: {mainJobId}, jobType: {jobType}");
            try
            {
                var mainJobInfo = new BaseJobDto { Id = mainJobId, JobType = jobType, IsMainJob = true };
                var worker = new MainJobDetailWorker();
                long totalCount = 0;
                await foreach (var batch in _jobProgressDao.GetJobProgressesByMainJobIdAsync(mainJobId))
                {
                    worker.InsertData(batch.Select(ConvertUtil.ConvertToProgressJobDetails), mainJobInfo);
                    totalCount += batch.Count();
                }
                logger.Info($"Migrated {totalCount} entries to RPT for main job {mainJobId}.");

                // After migration, delete the job details from the database
                var clearResult = await _jobProgressDao.ClearJobProgressesByJobIdAsync(mainJobId);
                logger.Info($"Deleted job details for main job {mainJobId}, rows affected: {clearResult}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Migrate job details to rpt and delete failed, mainJobId: {mainJobId}, jobType: {jobType}, ex: {ex}");
            }
        }

        public async Task<bool> UpdateRemainingSubJobStatusAsync(string mainJobId, HashSet<int> originalStatuses, int newStatus)
        {
            if (string.IsNullOrEmpty(mainJobId))
            {
                logger.Warn("Main job ID is null or empty for updating remaining sub-job statuses.");
                return false;
            }
            if (originalStatuses == null || originalStatuses.Count == 0)
            {
                logger.Warn($"No original statuses provided for updating remaining sub-job statuses for main job {mainJobId}.");
                return false;
            }
            try
            {
                var result = await _jobProgressDao.UpdateRemainingSubJobStatusAsync(mainJobId, originalStatuses, newStatus);
                return result > 0;
            }
            catch (Exception ex)
            {
                logger.Error($"Update remaining sub-job statuses failed for main job {mainJobId}, ex: {ex}");
                return false;
            }
        }
    }
}
