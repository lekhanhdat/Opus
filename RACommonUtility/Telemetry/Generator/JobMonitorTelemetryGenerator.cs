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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry.Generator
{
    public class JobMonitorTelemetryGenerator : TelemetryGenerator
    {
        private readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        public override TelemetryModule Module => TelemetryModule.JobMonitor;

        protected override Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>> EventTypeGenerateMapping =>
            new Dictionary<TelemetryEventType, Func<IList<object>, CloudRecordsCommonRecord>>
            {
                { TelemetryEventType.RunJob, RunJob },
                { TelemetryEventType.MonitorFailedJob, MonitorJob },
                { TelemetryEventType.MonitorSpecificExceptionJob, MonitorJob },
                { TelemetryEventType.MonitorLongRunningJob, MonitorJob }
            };

        public CloudRecordsCommonRecord RunJob(IList<object> args)
        {
            var record = new CloudRecordsJobMonitorRecord();
            var jobId = Convert.ToString(args[0]);
            var job = JobMonitorDao.GetJob(jobId);
            record.JobId = jobId;
            record.JobStatus = ((JobStatus)job.Status).ToString();
            record.JobType = ((JobType)job.JobType).ToString();
            record.JobUsageTime = (long)(new DateTime(job.EndTime, DateTimeKind.Utc) - new DateTime(job.StartTime, DateTimeKind.Utc)).TotalMilliseconds;
            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
            {
                TenantLocalValue.LogonUserEmail = job.UserName;
            }
            return record;
        }

        public CloudRecordsCommonRecord MonitorJob(IList<object> args)
        {
            var record = new CloudRecordsJobMonitorRecord();
            var job = args[0] as DB.Model.RMJobMonitor;
            record.JobId = job.Id;
            record.JobStatus = ((JobStatus)job.Status).ToString();
            record.JobType = ((JobType)job.JobType).ToString();
            var endTime = job.EndTime > 0 ? job.EndTime : DateTime.UtcNow.Ticks;
            record.JobUsageTime = (long)(new TimeSpan(endTime - job.StartTime).TotalHours);
            record.JobCommentType = job.ExceptionType.ToString();
            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail))
            {
                TenantLocalValue.LogonUserEmail = job.UserName;
            }
            return record;
        }
    }
}
