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

using System;
using System.Threading;
using AvePoint.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Service.JobMonitor;
using RAExportCommon;

namespace AvePoint.RA.ArchiverMigration
{
    public class JobProgressStageUpdater
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(JobProgressStageUpdater));
        private IJobMonitorService _jobMonitorService;
        private IJobMonitorService JobMonitorService => _jobMonitorService != null ? _jobMonitorService : (_jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>());

        private string _jobId;
        private int _startProgress;
        private int _currentProgress = 0;
        private int _increasingProgress = 0;    //_currentProgress 加1时，_increasingProgress 需要减1
        private long _baseSize = 0;
        private long _consumedSize = 0;

        /// <summary>
        /// Use for cloud archiver migration job to update job progress in some job stage
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="increasingProgress">0 <= increasingProgress < 99. If increasingProgress + startProgress >= 100 => increasingProgress = Max(99 - startProgress, 0) </param>
        /// <param name="baseSize"></param>
        public JobProgressStageUpdater(string jobId, int increasingProgress = 1, long baseSize = 1)
        {
            _logger.Info($"create job progress stage updater: {jobId}");
            _jobId = jobId;
            Init(increasingProgress, baseSize);

            _ = AutoUpdateProcess();
        }

        private async Task AutoUpdateProcess()
        {
            while (true)
            {
                await JobMonitorService.UpdateJobWithoutProgressChangeAsync(_jobId);
                await Task.Delay(1000 * 60);
            }
        }

        public void MoveToNextStage(int increasingProgress, long baseSize)
        {
            this.Flush();
            Init(increasingProgress, baseSize);
        }

        private void Init(int increasingProgress, long baseSize)
        {
            _currentProgress = JobMonitorService.GetJobProgress(_jobId);
            _startProgress = _currentProgress;
            _logger.Info($"Start progress: {_startProgress}, increasingProgress: {increasingProgress}, baseSize: {baseSize}");
            var finalIncreasingProgress = increasingProgress;
            if (increasingProgress < 0)
            {
                _logger.Warn($"increasingProgress must be >= 0.");
                finalIncreasingProgress = 0;
            }
            if (increasingProgress + _startProgress >= 100)
            {
                finalIncreasingProgress = Math.Max(99 - _startProgress, 0);
            }
            _logger.Info($"Final increasingProgress: {finalIncreasingProgress}");
            _increasingProgress = finalIncreasingProgress;
            _baseSize = baseSize > 0 ? baseSize : 1;
            _consumedSize = 0;
        }

        public void Increase(long value = 1)
        {
            SingleExecute(() =>
            {
                _consumedSize = Math.Min(_consumedSize + value, _baseSize);

                var consumedProgress = (int)(_increasingProgress * _consumedSize / _baseSize);
                if (consumedProgress > 0)
                {
                    var newProgress = _startProgress + consumedProgress;
                    _logger.Info($"Update job {_jobId} progress from {_currentProgress} to {newProgress}");
                    _currentProgress = newProgress;
                    JobMonitorService.UpdateJobProgress(_jobId, _currentProgress);
                }
            });
        }

        public void Flush()
        {
            if (_consumedSize < _baseSize)
            {
                Increase(_baseSize - _consumedSize);
            }
        }

        private void SingleExecute(Action action)
        {
            if (_increasingProgress <= 0)
            {
                return;
            }
            lock (this)
            {
                if (_increasingProgress <= 0)
                {
                    return;
                }
                action();
            }
        }
    }
}
