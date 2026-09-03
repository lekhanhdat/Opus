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
using AvePoint.Media.Core.IO.Output;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Telemetry;
using Cloud.Sdk.Telemetry.Data.CloudRecords;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DisposalProgress.Impl
{
    public class CompressionStatistic : BaseDisposalStatistic
    {

        private long _beforeCompressionSize;

        private long _afterCompressionSize;

        private DateTime _startTime;

        #region instance

        private readonly static object _instanceLock = new object();

        private static CompressionStatistic _instance;

        public static CompressionStatistic Instance()
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new CompressionStatistic();
                    }
                }
            }
            return _instance;
        }
        #endregion

        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();

        private void _beforeCompressionSizeAction (long size)
        {
            Interlocked.Add(ref _beforeCompressionSize, size);
        }

        private void _afterCompressionSizeAction(long size)
        {
            Interlocked.Add(ref _afterCompressionSize, size);
        }

        public override void Init(DisposalStaticInitObject initObject)
        {
            if (AlreadyInit() || _statisticState != ThreadState.Unstarted)
            {
                _mLog.Warn("CompressionStatistic already initialized.");
                return;
            }
            _startTime = DateTime.UtcNow;
            _beforeCompressionSize = 0;
            _afterCompressionSize = 0;
            _mainJobId = initObject.MainJobId;
            _subJobId = initObject.SubJobId;
            _jobType = initObject.JobType;
            _alreadyInit = true;
            CompressedFormatedOutputStream.BeforeCompressEvent += _beforeCompressionSizeAction;
            CompressedFormatedOutputStream.AfterCompressEvent += _afterCompressionSizeAction;
        }

        public override ThreadState GetStatisticState()
        {
            return base.GetStatisticState();
        }

        public override bool AlreadyInit()
        {
            return base.AlreadyInit();
        }

        public override void StartStatistic()
        {
            _statisticState = ThreadState.Running;
        }

        public override void PrepareEndStatistic()
        {
            _statisticState = ThreadState.StopRequested;
        }

        public override void WaitEndStatistic()
        {
            try
            {
                object[] args = new object[1];
                args[0] = new CloudRecordsCompressionRecord()
                {
                    MainJobId = _mainJobId,
                    JobId = _subJobId,
                    JobType = _jobType.ToString(),
                    BeforeCompressionLength = _beforeCompressionSize,
                    AfterCompressionLength = _afterCompressionSize,
                    EndTime = DateTime.UtcNow,
                    StartTime = _startTime,
                    CompressionSpeed = GlobalStorageSettingDao?.GetGlobalSettingInfoFromRA()?.CompressionSpeed ?? 0
                };
                TelemetryContext.SendToQueue(TelemetryModule.JobCompressionInfoRecord, TelemetryEventType.RunJob, args);
                _mLog.Info($"CompressionStatistic WaitEndStatistic: MainJobId:{_mainJobId}, SubJobId:{_subJobId}, JobType:{_jobType}, BeforeCompressionLength:{_beforeCompressionSize}, AfterCompressionLength:{_afterCompressionSize}, StartTime:{_startTime}, EndTime:{DateTime.UtcNow}");
            }
            catch(Exception ex)
            {
                _mLog.Error($"CompressionStatistic WaitEndStatistic error:{ex}");
            }
            CompressedFormatedOutputStream.BeforeCompressEvent -= _beforeCompressionSizeAction;
            CompressedFormatedOutputStream.AfterCompressEvent -= _afterCompressionSizeAction;
            _statisticState = ThreadState.Stopped;
        }

        public override void Dispose()
        {
            _beforeCompressionSize = 0;
            _afterCompressionSize = 0;
            _statisticState = ThreadState.Unstarted;
            _mainJobId = default;
            _subJobId = default;
            _jobType = default;
            _alreadyInit = default;
        }
    }
}
