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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
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
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Report
{
    public class RMReportManager : IRMReportManager, IDisposable
    {
        #region Public field
        private BaseJobDto BaseJobDto { get; set; }
        private Queue myDetailSyncQ = null;
        private Queue myInsertingDetailSyncQ = new Queue();
        private Queue myReportSyncQ = null;

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
        private IJobMonitorService jobMonitorService = null;
        private IJobDetailService jobDetailService = null;
        private IRMReportService jobReportService = null;
        private List<JobStatus> endJobStatus = null;
        private double weightCoefficient = 1;
        private int reportBufferCount = 100;
        private int detailBufferCount = 5;
        private AutoResetEvent exitEvent1;
        #endregion

        public JobType JobType => this.jobType;

        public string JobId => this.jobId;

        #region Public Method
        public RMReportManager(string currentJobId, JobType jobType, bool syncReport = false)
        {
            this.jobId = currentJobId;
            this.jobType = jobType;
            exitEvent1 = new AutoResetEvent(false);
            //safe thread  
            myDetailSyncQ = Queue.Synchronized(new Queue());

            BaseJobDto = new BaseJobDto() { Id = currentJobId, JobType = (int)jobType };

            jobMonitorService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
            jobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));

            endJobStatus = new List<JobStatus>()
            {
                JobStatus.Finished ,
                JobStatus.FinishWithException,
                JobStatus.Failed,
                JobStatus.Stopped,
                JobStatus.Skipped
            };
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
                logger.Warn("Send Detail Error {0} {1}", jobId, e.ToString());
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
                logger.Warn("Batch Send detail error {0} {1}", jobId, e.ToString());
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
                        if(myInsertingDetailSyncQ == null)
                        {
                            myInsertingDetailSyncQ = new Queue();
                        }
                        List<JMJobDetails> details = new List<JMJobDetails>();
                        int totalCnt = needCache ? DetailBufferCount : myDetailSyncQ.Count;
                        for (int i = 0; i < totalCnt; i++)
                        {
                            myInsertingDetailSyncQ?.Enqueue(myDetailSyncQ.Peek());
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
                    logger.Error("sync job detail error:{0}", ex.ToString());
                    sendDetailFinish = true;
                }
                finally
                {
                    myInsertingDetailSyncQ?.Clear();
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
                logger.Warn("Send Report Error {0} {1}", jobId, e.ToString());
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
            AveTenantThread updateProgress = new AveTenantThread(new ParameterizedThreadStart(UpdateJobProgress));
            updateProgress.IsBackground = true;
            updateProgress.Start(updateTime);
        }
        /// <summary>
        /// Set Job Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void SetJobStatus(string id, JobStatus status, string message)
        {
            jobMonitorService.UpdateJobStatus(id, status, message);
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
            SetJobStatus(jobId, jobStatus, jobComment);
        }

        /// <summary>
        /// for board update detail without upload detail
        /// </summary>
        public void WaitReportFinish()
        {
            this.jobFinished = true;
            WaitCompleted();
        }

        public void Dispose()
        {
            exitEvent1.Dispose();
        }

        public List<JMJobDetails> GetCacheJobDetails()
        {
            List<JMJobDetails> cacheDetails = new List<JMJobDetails>();
            cacheDetails.AddRange(myInsertingDetailSyncQ?.ToArray().Cast<JMJobDetails>() ?? new List<JMJobDetails>());
            cacheDetails.AddRange(myDetailSyncQ?.ToArray().Cast<JMJobDetails>() ?? new List<JMJobDetails>());
            return cacheDetails;
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
                    Progress += increment;
                    var tempProgress = Progress;
                    if(tempProgress < 1)
                    {
                        tempProgress = 1;
                    }
                    else if(tempProgress > 99)
                    {
                        tempProgress = 99;
                    }

                    jobMonitorService.UpdateJobProgress(jobId, tempProgress);
                    //Thread.Sleep(Convert.ToInt32(updateTime) * 1000);
                    if (exitEvent1.WaitOne(Convert.ToInt32(updateTime) * 1000))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("UpdateJobProgress Error and will sleep 3s retry:{0} ", e.ToString()));
                    Thread.Sleep(3000);
                }
            }

        }

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

        public Task MonitorExceptionAsync(string jobId, MonitorExceptionType exceptionType)
        {
            throw new NotImplementedException();
        }

        public void WaitFlushAllDetail()
        {
            throw new NotImplementedException();
        }

        public void StartUpdateJobProgressByPhase(int totalPhases, int updateTime = 8)
        {
            throw new NotImplementedException();
        }
        public void AdvanceToNextPhase()
        {
            throw new NotImplementedException();
        }
        public void DecreaseTotalPhases(int count)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
