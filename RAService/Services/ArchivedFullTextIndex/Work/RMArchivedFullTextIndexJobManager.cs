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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexJobManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexJobManager));

        private readonly Dictionary<string, List<(string JobId, Contract.RMWeb.JobMonitor.JobStatus Status)>> _siteJobStatus = [];

        private readonly IRMReportManager _reportManager;

        public string JobId { get; private set; }

        public RMArchivedFullTextIndexJobManager(string jobId)
        {
            JobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, Contract.JobMonitor.JobType.ArchiverFullTextIndex);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public void Init(int totalProgress)
        {
            _reportManager.IncreaseBase(totalProgress + 1);
            _reportManager.StartUpdateJobProgress();
            _reportManager.Increase();
        }

        public void Add(string siteUrl, string jobId, Contract.RMWeb.JobMonitor.JobStatus status)
        {
            _reportManager.Increase();
            if(!_siteJobStatus.TryGetValue(siteUrl, out var jobs))
            {
                jobs = [];
                _siteJobStatus.Add(siteUrl, jobs);
            }

            jobs.Add((jobId, status));
        }

        public void Failed()
        {
            _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
        }

        public void Finish()
        {
            var jobStatusJson = JsonConvert.SerializeObject(_siteJobStatus);
            _logger.Info(jobStatusJson);

            _siteJobStatus.ForEach(item =>
            {
                var jobStatus = GetDetailStatus(item.Value.Select(item => item.Status));

                _reportManager.SendJobDetail(new JMArchiverFullTextIndexJobDetails
                {
                    Url = item.Key,
                    Status = jobStatus,
                    Comment = jobStatus != JobDetailsStatus.Successful ? "RM_HS_Criteria_View_Msg_ValidOtherError" : ""
                });
            });

            var jobStatus = GetJobStatus(_siteJobStatus.SelectMany(item => item.Value).Select(item => item.Status));
            _reportManager.SetJobFinished(jobStatus, jobStatus == JobStatus.Finished ? "" : "RM_HS_Criteria_View_Msg_ValidOtherError");
        }

        private static JobDetailsStatus GetDetailStatus(IEnumerable<Contract.RMWeb.JobMonitor.JobStatus> siteStatuses)
        {
            var siteSucceedSet = siteStatuses.ToHashSet();
            if (siteSucceedSet.Count > 1 || (siteSucceedSet.Count == 1 && siteSucceedSet.Contains(Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)))
            {
                return JobDetailsStatus.Exception;
            }

            if (siteSucceedSet.Count == 1 && siteSucceedSet.Contains(Contract.RMWeb.JobMonitor.JobStatus.Failed))
            {
                return JobDetailsStatus.Failed;
            }

            return JobDetailsStatus.Successful;
        }

        private static JobStatus GetJobStatus(IEnumerable<Contract.RMWeb.JobMonitor.JobStatus> jobStatuses)
        {
            var jobSucceedSet = jobStatuses.ToHashSet();
            if (jobSucceedSet.Count > 1 || (jobSucceedSet.Count == 1 && jobSucceedSet.Contains(Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)))
            {
                return JobStatus.FinishWithException;
            }

            if (jobSucceedSet.Count == 1 && jobSucceedSet.Contains(Contract.RMWeb.JobMonitor.JobStatus.Failed))
            {
                return JobStatus.Failed;
            }

            return JobStatus.Finished;
        }
    }
}
