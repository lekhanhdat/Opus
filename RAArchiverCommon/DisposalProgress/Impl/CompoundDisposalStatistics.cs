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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DisposalProgress.Impl
{
    public class CompoundDisposalStatistics : BaseThreadDisposalStatistic
    {
        private static readonly HashSet<Type> _archiveJobStatisticSet = 
            [
                typeof(SOProgressScAndFileStatistic),
                typeof(ThrottlingStatistic),
                typeof(CompressionStatistic),
            ];

        private static readonly HashSet<Type> _restoreJobStatisticSet =
            [
                typeof(ThrottlingStatistic),
            ];

        private static readonly HashSet<Type> _applySettingJobStatisticSet =
            [
                typeof(ThrottlingStatistic),
            ];

        private static readonly HashSet<Type> _dataSyncJobStatisticSet =
            [
                typeof(ThrottlingStatistic),
            ];

        private static readonly HashSet<Type> _retentionJobStatisticSet =
            [
                typeof(ThrottlingStatistic),
            ];

        private static readonly Dictionary<JobType, HashSet<Type>> _jobTypeAndStatisticMap = new(){
            {JobType.RecordsDisposal, _archiveJobStatisticSet},
            {JobType.OneDriveRecordsDisposal, _archiveJobStatisticSet},
            {JobType.EXORecordsDisposal, _archiveJobStatisticSet},
            {JobType.RMArchiverBackup, _archiveJobStatisticSet},
            {JobType.SOPreScan, _archiveJobStatisticSet},
            {JobType.DiscoveryPreScan, _archiveJobStatisticSet},
            {JobType.DiscoveryPlanProScan, _archiveJobStatisticSet},
            {JobType.DiscoverOptimization, _archiveJobStatisticSet},
            {JobType.DiscoveryPlanProOptimization, _archiveJobStatisticSet},
            {JobType.DiscoveryAOSPOptimization, _archiveJobStatisticSet},
            {JobType.SpecifySitesArchiverBackup, _archiveJobStatisticSet},
            {JobType.SpecifyTeamsArchiverBackup, _archiveJobStatisticSet},
            {JobType.TeamsArchiverBackup, _archiveJobStatisticSet},
            {JobType.TeamsRecordsDisposal, _archiveJobStatisticSet},
            {JobType.TeamsPreScan, _archiveJobStatisticSet},
            {JobType.RMEndUserArchiverBackup, _archiveJobStatisticSet},
            {JobType.ArchiverByHSMXml, _archiveJobStatisticSet},
            {JobType.CleanUpDuplicateDatas, _archiveJobStatisticSet},
            {JobType.PhysicalRecordsDisposal, new (){ typeof(CompressionStatistic) } },

            {JobType.TeamsArchiverRestore, _restoreJobStatisticSet},
            {JobType.ArchiverRestore, _restoreJobStatisticSet},
            {JobType.ArchiverToSpoRestore, _restoreJobStatisticSet},
            {JobType.ArchiverOutPlaceRestore, _restoreJobStatisticSet},
            {JobType.StubOopRestore, _restoreJobStatisticSet},
            {JobType.AOSPRestore, _restoreJobStatisticSet},
            {JobType.TeamsOutPlaceRestore, _restoreJobStatisticSet },

            {JobType.EXOApplySetting, _applySettingJobStatisticSet},
            {JobType.ApplySharePointSettings, _applySettingJobStatisticSet},
            {JobType.SharePointScheduleSetting, _applySettingJobStatisticSet},
            {JobType.ApplyTeamsSettings, _applySettingJobStatisticSet},
            {JobType.TeamsScheduleSetting, _applySettingJobStatisticSet},

            {JobType.DataSynchronisation, _dataSyncJobStatisticSet},
            {JobType.TeamsDataSynchronisation, _dataSyncJobStatisticSet},
            {JobType.OneDriveDataSynchronisation, _dataSyncJobStatisticSet},
            {JobType.EXODataSynchronisation, _dataSyncJobStatisticSet},

            {JobType.ArchiverRetention, _retentionJobStatisticSet},
            {JobType.ArchiverRetentionSimulate, _retentionJobStatisticSet},
            {JobType.EXOArchiverRetention, _retentionJobStatisticSet},
            {JobType.TeamsArchiverRetention, _retentionJobStatisticSet},
        };

        private readonly static object _instanceLock = new object();

        private static CompoundDisposalStatistics? _instance;

        public static CompoundDisposalStatistics Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CompoundDisposalStatistics();
                        }
                    }
                }
                return _instance;
            }
        }

        private HashSet<IDisposalStatistic> _subStatistics = new ();

        public override ThreadState GetStatisticState()
        {
            return _statisticState;
        }

        private CompoundDisposalStatistics()
        {

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

                _mainJobId = initObject.MainJobId;
                _subJobId = initObject.SubJobId;
                _jobType = initObject.JobType;
                HashSet<Type> subStatisticTypes = _jobTypeAndStatisticMap.GetValueOrDefault(_jobType, new());
                _mLog.Info($"current job type:{_jobType}, will use statistics:[ {string.Join(", ", subStatisticTypes)} ]");
                foreach (var type in subStatisticTypes)
                {
                    ActionWithLockAndCatch(() =>
                    {
                        MethodInfo? method = type.GetMethod("Instance", BindingFlags.Public | BindingFlags.Static);
                        if (method != null)
                        {
                            object? instance = method.Invoke(null, null);
                            if (instance != null && (instance as IDisposalStatistic) != null)
                            {
                                IDisposalStatistic disposalStatistic = (IDisposalStatistic)instance;
                                if (!disposalStatistic.AlreadyInit())
                                {
                                    disposalStatistic.Init(initObject);
                                }
                                else
                                {
                                    _mLog.Warn($"Type:{type.FullName} already init");
                                }
                                _subStatistics.Add(disposalStatistic);
                            }
                            else
                            {
                                _mLog.Warn($"type:{type.FullName} not contains Instance method");
                            }
                        }
                    }, $"Fail init statistic type:{type.FullName}");
                }
                _alreadyInit = true;
            }
        }

        public override void PrepareEndStatistic()
        {
            ActionWithLockAndCatch(() =>
            {
                foreach (IDisposalStatistic subStatistic in _subStatistics)
                {
                    subStatistic.PrepareEndStatistic();
                }
            }, "Fail Prepare End Statistic");
        }

        public override void StartStatistic()
        {
            ActionWithLockAndCatch(() =>
            {
                foreach (IDisposalStatistic subStatistic in _subStatistics)
                {
                    subStatistic.StartStatistic();
                }
            }, "Fail Start Static");
        }

        public override void WaitEndStatistic()
        {
            try
            {
                foreach (IDisposalStatistic subStatistic in _subStatistics)
                {
                    subStatistic.WaitEndStatistic();
                }
            }
            catch(Exception e)
            {
                _mLog.Error($"Fail Wait End Statistic,ex:{e}");
            }
        }

        public override bool AlreadyInit()
        {
            lock (_lockObject)
            {
                return _alreadyInit;
            }
        }
    }
}
