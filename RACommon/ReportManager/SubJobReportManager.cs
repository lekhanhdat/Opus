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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Report
{
    public class SubJobReportManager : IRMReportManager, IDisposable
    {
        #region Public field
        private BaseJobDto BaseJobDto { get; set; }
        private Queue myDetailSyncQ = null;
        private Queue myReportSyncQ = null;

        private Queue myInsertingDetailSyncQ = null;

        private long lastUpdateTime = DateTime.MinValue.Ticks;
        /// <summary>
        /// 总数，用来计算job进度
        /// </summary>
        private long total;

        public long Total { get { return total; } }

        /// <summary>
        /// 完成数量，每做完一个单位通过++FinishCount来计算进度
        /// </summary>
        private long finished;

        public long Finished { get { return finished; } }

        public string JobId => this.jobId;

        /// <summary>
        /// Job总进度
        /// </summary>
        private int Progress { get; set; }

        /// <summary>
        /// Report缓存数量，默认100个Report发送一次
        /// </summary>
        public int ReportBufferCount
        {
            get
            {
                return reportBufferCount;
            }
            set
            {
                if (value > 0)
                {
                    reportBufferCount = value;
                }
            }
        }

        /// <summary>
        /// Detail缓存数量，默认5个Detail发送一次
        /// </summary>
        public int DetailBufferCount
        {
            get
            {
                return detailBufferCount;
            }
            set
            {
                if (value > 0)
                {
                    detailBufferCount = value;
                }
            }
        }
        #endregion

        #region Private field

        private static RALogger logger = RALogger.GetInstance(typeof(RMReportManager));

        private string jobId = string.Empty;
        private JobType jobType = JobType.None;
        private bool jobFinished = false;
        private bool sendDetailFinish = false;
        private bool sendReportFinish = false;
        private JobStatus jobStatus = JobStatus.None;
        private string jobComment = string.Empty;
        //private IJobMonitorService jobMonitorService = null;
        private IJobDetailService jobDetailService = null;
        private IRMReportService jobReportService = null;
        private IJobInfoUpdater jobInfoUpdate = null;
        private double weightCoefficient = 1;
        private int reportBufferCount = 100;
        private int detailBufferCount = 5;
        private AutoResetEvent exitEvent1;
        #endregion

        public JobType JobType => this.jobType;

        #region Public Method
        public SubJobReportManager(string currentJobId, JobType jobType, bool syncReport = false)
        {
            this.jobId = currentJobId;
            this.jobType = jobType;
            exitEvent1 = new AutoResetEvent(false);
            //safe thread  
            myDetailSyncQ = Queue.Synchronized(new Queue());
            myInsertingDetailSyncQ = Queue.Synchronized(new Queue());

            BaseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)jobType };
            AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(currentJobId);

            jobInfoUpdate = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));

            jobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));


            if (syncReport)
            {
                jobReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                myReportSyncQ = Queue.Synchronized(new Queue());
                sendReportFinish = false;
                AveTenantThread updateReport = new AveTenantThread(new ThreadStart(SyncJobReports));
                updateReport.IsBackground = true;
                updateReport.Start();
            }
            else
            {
                sendReportFinish = true;
            }
            AveTenantThread updateDetail = new AveTenantThread(new ThreadStart(SyncJobDetails));
            updateDetail.IsBackground = true;
            updateDetail.Start();
        }

        public void WeightCoefficient(double w)
        {
            this.weightCoefficient = w;
        }


        /// <summary>
        /// Send job detail,
        /// </summary>
        /// <param name="detail"></param>
        public void SendJobDetail(JMJobDetails detail)
        {
            try
            {
                if (detail != null)
                {
                    myDetailSyncQ.Enqueue(detail);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Add Detail Error {0} {1}", jobId, e.ToString());
            }
        }

        /// <summary>
        /// batch Send job detail,
        /// </summary>
        /// <param name="detail"></param>
        public void BatchSendJobDetail(IEnumerable<JMJobDetails> details)
        {
            try
            {
                if (details != null)
                {
                    foreach (var detail in details)
                    {
                        myDetailSyncQ.Enqueue(detail);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn(" Batch add detail error {0} {1}", jobId, e.ToString());
            }
        }


        private void SyncJobDetails()
        {
            while (true)
            {
                try
                {
                    bool needCache = myDetailSyncQ.Count >= DetailBufferCount;
                    if (needCache || jobFinished)
                    {

                        List<JMJobDetails> details = new List<JMJobDetails>();

                        int totalCnt = needCache ? DetailBufferCount : myDetailSyncQ.Count;
                        for (int i = 0; i < totalCnt; i++)
                        {
                            myInsertingDetailSyncQ.Enqueue(myDetailSyncQ.Peek());
                            var sdetail = myDetailSyncQ.Dequeue();
                            details.Add((JMJobDetails)sdetail);
                        }
                        if (details.Count > 0)
                        {
                            UpdateJobDetails(details);
                        }

                        if (jobFinished && myDetailSyncQ.Count == 0)
                        {
                            sendDetailFinish = true;
                            //Thread.Sleep(3000);
                            if (exitEvent1.WaitOne(2000))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                    myInsertingDetailSyncQ.Clear();
                }
                catch (Exception ex)
                {
                    logger.Error("sync job detail error:{0}", ex.ToString());
                    sendDetailFinish = true;
                    myInsertingDetailSyncQ.Clear();
                }

            }
        }

        private void SyncJobReports()
        {
            while (true)
            {
                try
                {
                    bool needCache = myReportSyncQ.Count >= ReportBufferCount;
                    if (needCache || jobFinished)
                    {

                        List<BaseReport> reports = new List<BaseReport>();
                        int totalCnt = needCache ? ReportBufferCount : myReportSyncQ.Count;
                        for (int i = 0; i < totalCnt; i++)
                        {
                            var sdetail = myReportSyncQ.Dequeue();
                            reports.Add((BaseReport)sdetail);
                        }
                        if (reports.Count > 0)
                        {
                            UpdateJobReports(reports);
                        }

                        if (jobFinished && myReportSyncQ.Count == 0)
                        {
                            sendReportFinish = true;
                            //Thread.Sleep(3000);
                            if (exitEvent1.WaitOne(2000))
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("sync job report error:{0}", ex.ToString());
                    sendReportFinish = true;
                }

            }
        }


        public void SendJobReport(BaseReport report)
        {
            try
            {
                if (report != null)
                {
                    myReportSyncQ.Enqueue(report);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Add Report Error {0} {1}", jobId, e.ToString());
            }
        }

        public void BatchSendJobReport(IEnumerable<BaseReport> reports)
        {
            try
            {
                if (reports != null)
                {
                    foreach (var report in reports)
                    {
                        myReportSyncQ.Enqueue(report);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Batch Send Report Error {0} {1}", jobId, e.ToString());
            }
        }

        public void StartUpdateJobProgress(int updateTime)
        {
            SetJobStatus(jobId, JobStatus.InProgress, "");
            AveTenantThread updateProgress = new AveTenantThread(new ParameterizedThreadStart(UpdateJobProgress));
            updateProgress.IsBackground = true;
            updateProgress.Start(updateTime);
            UpdateJobTime(true);
        }
        /// <summary>
        /// Set Job Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void SetJobStatus(string id, JobStatus status, string message)
        {
            jobInfoUpdate.UpdateJobState(id, (int)status, message);
        }

        /// <summary>
        /// job结束后需要调用，使UpdateProgress进程更新Job状态并关闭，Send最后几个Job Detail。
        /// 然后等待Job结束
        /// </summary>
        /// <param name="jobStatus">Job最终状态</param>
        public void SetJobFinished(JobStatus jobStatus, string jobComment = "")
        {
            this.jobStatus = jobStatus;
            this.jobComment = jobComment;
            jobFinished = true;
            WaitCompleted();
            jobDetailService.UploadReportFile(BaseJobDto);
            UpdateJobTime(false);
            SetJobStatus(jobId, jobStatus, jobComment);
            jobInfoUpdate.UpdateJobProgress(jobId, 100);
        }

        public void WaitReportFinish()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            exitEvent1.Dispose();
        }
        #endregion

        #region Private Method
        #region send details
        //Byron: This function need to be changed later, should not use thread pool here, 
        //we can use a separate one thread to handle the job report, have a memory cache queue to put job details.

        private void UpdateJobDetails(object details)
        {
            try
            {
                List<JMJobDetails> jobDetails = (List<JMJobDetails>)details;
                jobDetailService.SyncJobDetails(jobDetails, BaseJobDto);

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while update job detail,ERROR:{0}", ex.ToString());
            }


        }
        #endregion

        #region send reports


        private void UpdateJobReports(object reports)
        {
            List<BaseReport> jobReports = (List<BaseReport>)reports;
            jobReportService.SyncReportJobDatas(jobReports, BaseJobDto);
        }
        #endregion

        private void UpdateJobProgress(object updateTime)
        {
            while (true)
            {
                try
                {
                    int increment = total == 0 ? 1 : (int)(finished * weightCoefficient * 100 / total);

                    if (increment != 0 || DateTime.UtcNow.AddMinutes(-5).Ticks > lastUpdateTime)
                    {
                        if (total == 0)
                        {
                            Progress += increment;
                        }
                        else
                        {
                            Progress = Math.Max(Progress, increment);
                        }
                        lastUpdateTime = DateTime.UtcNow.Ticks;
                        jobInfoUpdate.UpdateJobProgress(jobId, (Progress > 99) ? 99 : Progress);
                    }

                    if (exitEvent1.WaitOne(Convert.ToInt32(updateTime) * 1000))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("UpdateJobProgress Error and will sleep 3s retry:{0}", e.ToString()));
                    Thread.Sleep(3000);

                }
            }

        }

        #region Update Job Progress By Phase Logic for sub job that has virtual(sub) sub jobs

        private int _totalPhases = 1;
        private double _partWeight = 100.0;
        private int _currentPhaseIndex = 1;

        public void StartUpdateJobProgressByPhase(int totalPhases, int updateTime)
        {
            InitializeJobWeights(totalPhases);
            SetJobStatus(jobId, JobStatus.InProgress, "");
            AveTenantThread updateProgress = new AveTenantThread(new ParameterizedThreadStart(UpdateJobProgressByPhase));
            updateProgress.IsBackground = true;
            updateProgress.Start(updateTime);
            UpdateJobTime(true);
        }

        private void InitializeJobWeights(int totalPhases)
        {
            _totalPhases = totalPhases < 1 ? 1 : totalPhases; //1 + subJobCount + 1;
            _partWeight = 100.0 / _totalPhases;
            _currentPhaseIndex = 1;
            logger.Info($"Job {jobId} initialized with {_totalPhases} phases, each phase weight: {_partWeight}");
        }

        public void AdvanceToNextPhase()
        {
            Interlocked.Increment(ref _currentPhaseIndex);
        }

        public void DecreaseTotalPhases(int count)
        {
            var targetPhases = _totalPhases - count;
            if (targetPhases < _currentPhaseIndex) targetPhases = _currentPhaseIndex;
            _totalPhases = targetPhases;
            _partWeight = 100.0 / _totalPhases;
        }

        private void UpdateJobProgressByPhase(object updateTime)
        {
            while (true)
            {
                try
                {
                    var currentPhase = _currentPhaseIndex;
                    var partWeight = _partWeight;
                    double currentMaxLimit = currentPhase * partWeight;
                    double currentMinLimit = (currentPhase - 1) * partWeight;

                    int phaseMaxProgress = (int)currentMaxLimit;
                    int phaseMinProgress = (int)currentMinLimit;

                    if (phaseMaxProgress > 99) phaseMaxProgress = 99;

                    if (Progress < phaseMaxProgress)
                    {
                        // todo: may need to calculate based on finished/total ratio for more accurate progress if needed
                        // For now, just keep current logic of incrementing by 1
                        Progress += 1;
                    }

                    if (Progress > phaseMaxProgress) Progress = phaseMaxProgress;
                    Progress = Math.Max(Progress, phaseMinProgress);

                    lastUpdateTime = DateTime.UtcNow.Ticks;
                    jobInfoUpdate.UpdateJobProgress(jobId, Progress);

                    if (exitEvent1.WaitOne(Convert.ToInt32(updateTime) * 1000))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"UpdateJobProgressByPhase Error: {e}");
                    Thread.Sleep(3000);
                }
            }
        }
        #endregion

        private void WaitCompleted()
        {
            while (true)
            {
                if (sendDetailFinish && sendReportFinish)
                {
                    exitEvent1.Set();
                    break;
                }
                Thread.Sleep(500);
            }
        }

        public void IncreaseBase(long value)
        {
            Interlocked.Add(ref total, value);
        }

        public void Increase()
        {
            Interlocked.Add(ref finished, 1);
        }

        public void Increase(int x)
        {
            Interlocked.Add(ref finished, x);
        }

        public int GetProgress()
        {
            return Progress;
        }
        public long GetTotal()
        {
            return Total;
        }
        public void SetTotal(long x)
        {
            total = x;
        }
        public long GetFinished()
        {
            return Finished;
        }
        public void SetProgress(int x)
        {
            this.Progress = x;
        }
        public Task MonitorExceptionAsync(string jobId, MonitorExceptionType exceptionType)
        {
            return jobInfoUpdate.MonitorExeptionAsync(jobId, exceptionType);
        }

        private void UpdateJobTime(bool isStartTime)
        {
            jobInfoUpdate.UpdateJobTime(jobId, isStartTime);
        }

        public void WaitFlushAllDetail()
        {
            if(myDetailSyncQ == null || (myDetailSyncQ.Count == 0 && myInsertingDetailSyncQ.Count == 0))
            {
                return;
            }
            int originDetailBufferCount = DetailBufferCount;
            DetailBufferCount = 1;
            
            while(myDetailSyncQ.Count > 0 || myInsertingDetailSyncQ.Count > 0)
            {
                Thread.Sleep(500);
            }
            DetailBufferCount = originDetailBufferCount;
            
        }

        public List<JMJobDetails> GetCacheJobDetails()
        {
            List<JMJobDetails> cacheDetails = new List<JMJobDetails>();
            cacheDetails.AddRange(myInsertingDetailSyncQ?.ToArray().Cast<JMJobDetails>() ?? new List<JMJobDetails>());
            cacheDetails.AddRange(myDetailSyncQ?.ToArray().Cast<JMJobDetails>() ?? new List<JMJobDetails>());
            return cacheDetails;
        }

        #endregion
    }
}
