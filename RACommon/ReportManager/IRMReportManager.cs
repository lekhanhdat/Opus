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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Report
{
    public interface IRMReportManager
    {
        string JobId { get; }
        JobType JobType { get; }
        int DetailBufferCount { get; set; }
        void IncreaseBase(long value);
        void Increase();
        void Increase(int x);
        int GetProgress();
        long GetFinished();
        long GetTotal();
        void SetTotal(long x);
        void SetProgress(int x);
        void WeightCoefficient(double w);
        void SendJobDetail(JMJobDetails detail);
        void SendJobReport(BaseReport report);
        ///// <summary>
        ///// 开始更新job进度，默认每8S更新一次。
        ///// </summary>
        ///// <param name="updateTime">每隔多长时间更新一次Job进度，单位S</param>
        void StartUpdateJobProgress(int updateTime = 8);
        void SetJobFinished(JobStatus jobStatus, string jobComment = "");
        void BatchSendJobDetail(IEnumerable<JMJobDetails> details);
        void BatchSendJobReport(IEnumerable<BaseReport> reports);
        void WaitReportFinish();
        Task MonitorExceptionAsync(string jobId, MonitorExceptionType exceptionType);
        void WaitFlushAllDetail();
        List<JMJobDetails> GetCacheJobDetails();

        /// <summary>
        /// Starts updating job progress, updating every 8 seconds by default.
        /// </summary>
        /// <param name="totalPhases">how many phases the job progress need to split</param>
        /// <param name="updateTime">How often to update job progress, in seconds</param>
        void StartUpdateJobProgressByPhase(int totalPhases, int updateTime = 8);

        void AdvanceToNextPhase();
        void DecreaseTotalPhases(int count = 1);
    }
}
