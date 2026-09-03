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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.Records.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Common
{
    //对IRMReportManager 进行封装，添加了HasErrorNode 属性，标记job状态
    public class JobManagement
    {
        protected static DateTime JobStartTime = DateTime.MinValue;

        public IRMReportManager ReportManager { get; private set; }
        public string SubJobId = string.Empty;
        public bool HasErrorNode { get; set; }
        public bool JobHasStopped { get; set; }

        public bool HasSuccessNode { get; set; }

        private static JobManagement jobManager = null;
        private readonly static object mLock = new object();
        public static JobManagement GetInstance(string jobId, JobType jobType)
        {
            if (jobManager == null)
            {
                lock (mLock)
                {
                    if (jobManager == null)
                    {
                        jobManager = new JobManagement(jobId);
                    }
                }
            }
            return jobManager;
        }
        public static JobManagement GetInstanceV2(string jobId, JobType jobType)
        {
            if (jobManager == null)
            {
                lock (mLock)
                {
                    if (jobManager == null)
                    {
                        jobManager = new JobManagement(jobId, jobType);
                    }
                }
            }
            return jobManager;
        }
        private JobManagement(string jobId)
        {
            JobStartTime = DateTime.UtcNow;
            SubJobId = jobId;
            //ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress();
        }
        private JobManagement(string jobId, JobType jobType)
        {
            JobStartTime = DateTime.UtcNow;
            SubJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            ReportManager.Increase(1);
            ReportManager.StartUpdateJobProgress();
        }

        public void Finish()
        {
            var jobStatus = JobStatus.Finished;
            if (HasErrorNode)
            {
                if (HasSuccessNode)
                {
                    jobStatus = JobStatus.FinishWithException;
                }
                else
                {
                    jobStatus = JobStatus.Failed;
                }
            }
            if (JobHasStopped)
            {
                jobStatus = JobStatus.Stopped;
            }
            ReportManager.SetJobFinished(jobStatus);
        }
    }
    public enum StatisticsLevel
    {
        None = 0,

        // Teams-related
        TeamsGroup = 1,
        Channel = 2,
        ChannelConversation = 3,

        // Mail-related
        GroupMailbox = 10,
        GroupMailboxItem = 11,
        Conversation = 12,
        Event = 13,
        GroupMainboxFolder = 14,
        // SharePoint-related
        SiteCollection = 20,
        Site = 21,
        List = 22,
        Folder = 23,
        Item = 24,

        // Planner-related
        Plan = 30,
        Task = 31,
        Attachment = 32,

        // others
        Exception = 1000,
    }
}
