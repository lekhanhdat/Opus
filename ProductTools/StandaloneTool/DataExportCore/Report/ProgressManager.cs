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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Collections.Concurrent;

namespace DataExportCore
{
    public class ProgressManager
    {
        readonly object _lock = new object();

        readonly int _totalTaskCount;
        readonly double _taskWeight;
        readonly ConcurrentDictionary<Guid, double> _taskProgresses = new ConcurrentDictionary<Guid, double>();
        readonly ConcurrentDictionary<Guid, JobStatus> _jobStatus = new ConcurrentDictionary<Guid, JobStatus>();

        private long _totalExportSize;
        public long TotalExportSize { get => _totalExportSize; }

        public event Action<double, string> OverallProgressChanged;

        public ProgressManager(int totalTaskCount)
        {
            _totalTaskCount = totalTaskCount;
            _taskWeight = 100.0 / totalTaskCount;
        }

        public void AddProgressReport(Reporter report)
        {
            _taskProgresses.TryAdd(report.ReportId, 0);
            _jobStatus.TryAdd(report.ReportId,JobStatus.None);

            report.ProgressChanged += (progress, currentFile) =>
            {
                _taskProgresses[report.ReportId] = progress;
                CalculateOverallProgress(currentFile);
            };

            report.OnCompleted += (jobStatus, totalSize) =>
            {
                _jobStatus[report.ReportId] = jobStatus;
                Interlocked.Add(ref _totalExportSize, totalSize);
            };
        }

        public void SetCompletedReport(Guid reportId, JobStatus status, long totalSize)
        {
            _jobStatus[reportId] = status;
            Interlocked.Add(ref _totalExportSize, totalSize);
        }

        private void CalculateOverallProgress(string currentFile)
        {
            double overallProgress;

            lock (_lock)
            {
                overallProgress = Math.Min(_taskProgresses.Values.Sum() * _taskWeight / 100.0, 100);
            }

            OverallProgressChanged?.Invoke(overallProgress, currentFile);
        }


        public JobStatus GetFinalJobStatus()
        {
            if(_jobStatus.Values.All(j => j == JobStatus.Failed))
            {
                return JobStatus.Failed;
            }
            else if(_jobStatus.Values.All(j => j == JobStatus.Finished || j == JobStatus.Skipped))
            {
                return JobStatus.Finished;
            }
            return JobStatus.FinishWithException;
        }
    }
}
