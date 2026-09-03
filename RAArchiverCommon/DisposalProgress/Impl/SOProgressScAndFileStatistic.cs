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
using AvePoint.GCommon.Contract.Server.Common.Performance;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;

namespace RAArchiverCommon.DisposalProgress.Impl
{
    public class SOProgressScAndFileStatistic : BaseThreadDisposalStatistic
    {

        private volatile int _finishedFileCount;

        private volatile int _finishedScCount;

        private volatile int _endFlag;

        private int _tryUpdateExceptionAmountAfterJobEnd;

        private long _lastUpdateTime;

        private long _subJobIndex;

        private static readonly object _instanceLock = new object();

        public static SOProgressScAndFileStatistic _instance;

        public static SOProgressScAndFileStatistic Instance()
        {
            if(_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if(_instance == null)
                        {
                            _instance = new SOProgressScAndFileStatistic();
                        }
                    }
                }
                return _instance;
        }

        private SOProgressScAndFileStatistic()
        {
            _executeThreadAction = UpdateScAndFileCount;
        }

        public override void Init(DisposalStaticInitObject initObject)
        {
            lock (_lockObject)
            {
                if (_alreadyInit || _statisticState != ThreadState.Unstarted)
                {
                    _mLog.Error($"CompoundStatistics unable init, init status:{_alreadyInit}, statistic state:{_statisticState}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(this._mainJobId))
                {
                    this._mainJobId = initObject.MainJobId;
                    this._subJobId = initObject.SubJobId;
                    this._jobType = initObject.JobType;
                    _lastUpdateTime = DateTime.UtcNow.Ticks;
                    if(!long.TryParse(_subJobId?.Split('_')?[1], out _subJobIndex))
                    {
                        _subJobIndex = -1;
                        _mLog.Error(@$"SOProgressScAndFileStatistic fail get subJobIndex,main job Id:{_mainJobId}, sub job id:{_subJobId}");
                    }
                    else
                    {
                        _mLog.Info(@$"SOProgressScAndFileStatistic success init,main job Id:{_mainJobId}, sub job id:{_subJobId}");
                    }
                    _finishedFileCount = 0;
                    _finishedScCount = 0;
                }
                _alreadyInit = true;
            }
        }

        public bool IncreaseFileCount(int fileCount, int itemType)
        {
            if (itemType != (int)ItemType.DOCUMENT)
            {
                return false;
            }
            lock (_lockObject)
            {
                _finishedFileCount += fileCount;
            }
            return true;
        }

        private void UpdateScAndFileCount()
        {
            while (true)
            {
                #region pre check
                if (_statisticState == ThreadState.Stopped)
                {
                    break;
                }
                #endregion

                DateTime startTime = DateTime.UtcNow;
                #region realUpdate
                if ((_finishedFileCount > 0 || _finishedScCount > 0)
                    && (_statisticState == ThreadState.StopRequested || DateTime.UtcNow.Ticks - _lastUpdateTime >= 5L * 60L * 10000000L))
                {
                    string oldJobExtentionJson = null;
                    try
                    {
                        oldJobExtentionJson = JobMonitorService.GetJobExtension(_mainJobId);
                        JobExtension newJobExtension = null;
                        try
                        {
                            newJobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(oldJobExtentionJson);
                        }
                        catch(Exception ex)
                        {
                            _mLog.Error(@$"Deserialize job extention fail,oldJobExtention:{oldJobExtentionJson},error :{ex}");
                            break;
                        }
                        if(newJobExtension?.SOProgressFileAndSCCount == null)
                        {
                            _mLog.Warn($@"The job is older then hotfix RECO-24966 or not set SOProgressFileAndSCCount object");
                            break;
                        }
                        lock (_lockObject)
                        {
                            _mLog.Info($@"start update so job progress file and sc count,current cache file count:{_finishedFileCount}, sc count:{_finishedScCount}, main job id :{_mainJobId}, sub job id: {_subJobId}, old jobExtention:{oldJobExtentionJson}");
                            if (newJobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr == null || _subJobIndex == -1)
                            {   //for old logic
                                newJobExtension.SOProgressFileAndSCCount.ProgressedFileCount += _finishedFileCount;
                                newJobExtension.SOProgressFileAndSCCount.ProgressedSCCount += _finishedScCount;
                            }
                            else
                            {
                                int oldFileCount = newJobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr[_subJobIndex];

                                newJobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr[_subJobIndex] = _finishedFileCount;
                                newJobExtension.SOProgressFileAndSCCount.ProgressedSCCountArr[_subJobIndex] = _finishedScCount;
                                try
                                {
                                    var mainJob = JobMonitorService.GetJobMonitorStatisDto(_mainJobId);
                                    if(mainJob != null && (mainJob.JobType == (int)JobType.RMArchiverBackup ||
                                        mainJob.JobType == (int)JobType.TeamsArchiverBackup ||
                                        mainJob.JobType == (int)JobType.TeamsRecordsDisposal ||
                                        mainJob.JobType == (int)JobType.RecordsDisposal ||
                                        mainJob.JobType == (int)JobType.OneDriveRecordsDisposal))
                                    {
                                        newJobExtension.SOProgressFileAndSCCount.ProgressedFileCount += (_finishedFileCount - oldFileCount);
                                        _mLog.Info($@"SOProgressScAndFileStatistic, mainJob.JobType: {mainJob.JobType}");
                                    }
                                    
                                }
                                catch (Exception ex) {
                                    _mLog.Warn($@"SOProgressScAndFileStatistic fail to update ProgressedFileCount and ProgressedSCCount, main job id: {_mainJobId} sub job id: {_subJobId}");
                                }

                            }
                            string newJobExtensionJson = SerializerHelper.SerializeByJsonConvert(newJobExtension);
                            bool updateSuccess = JobMonitorService.AtomicityUpdateJobExtension(_mainJobId, oldJobExtentionJson, newJobExtensionJson);
                            _lastUpdateTime = DateTime.UtcNow.Ticks;
                            if (updateSuccess)
                            {
                                if (newJobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr == null || _subJobIndex == -1)
                                {   //for old logic
                                    _finishedFileCount = 0;
                                    _finishedScCount = 0;
                                }
                                if (_statisticState == ThreadState.StopRequested)
                                {
                                    _mLog.Info($@"finish update so job progress file and SC");
                                    _statisticState = ThreadState.Stopped;
                                    break;
                                }
                            }
                            else
                            {
                                _mLog.Warn($@"unable update so job progress file and sc count,current cache file count:{_finishedFileCount}, sc count:{_finishedScCount}, main job id :{_mainJobId}, old jobExtention:{oldJobExtentionJson}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _mLog.Error($@"Fail update so job progress file and sc count,current cache file count:{_finishedFileCount}, sc count:{_finishedScCount}, main job id :{_mainJobId}, old jobExtention:{oldJobExtentionJson}
, error message:{ex.Message}, error:{ex}");
                        if(_statisticState == ThreadState.StopRequested && ++_tryUpdateExceptionAmountAfterJobEnd >= 50)
                        {
                            _mLog.Error(@$"After try end job, when try update happen exception amount:{_tryUpdateExceptionAmountAfterJobEnd}, will end update thread");
                            break;
                        }
                    }
                }
                #endregion
                DateTime endTime = DateTime.UtcNow;

                #region set next check time
                int lastUpdateCostTimeInMs = (startTime - endTime).Milliseconds;
                int sleepTimeInMs = lastUpdateCostTimeInMs * 10;
               
                if (sleepTimeInMs < 6 * 1000)
                {
                    Thread.Sleep(6 * 1000);
                }
                else if (sleepTimeInMs > 10 * 60 * 1000)
                {
                    Thread.Sleep(10 * 60 * 1000);
                }
                else
                {
                    Thread.Sleep(sleepTimeInMs);
                }
                #endregion
            }
        }

        public override void StartStatistic()
        {
            base.StartStatistic();
        }

        public override void PrepareEndStatistic()
        {
            lock (_lockObject)
            {
                if (_statisticState == ThreadState.StopRequested ||  _statisticState == ThreadState.Stopped)
                {
                    _mLog.Error($"state: {_statisticState},already unable request stop");
                    return;
                }
                _statisticState = ThreadState.StopRequested;
                _finishedScCount = 1;
            }
        }

        public override void WaitEndStatistic()
        {
            if (_executeThread != null)
            {
                _executeThread.WaitThreadFinish();
            }
            else
            {
                _mLog.Warn($@"Don't have update progress SC and file count thread");
            }
            _finishedFileCount = 0;
            _finishedScCount = 0;
        }

        public override ThreadState GetStatisticState()
        {
            return _statisticState;
        }

        public override bool AlreadyInit()
        {
            return _alreadyInit;
        }
    }
}
