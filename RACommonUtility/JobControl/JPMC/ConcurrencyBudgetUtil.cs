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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.JobControl.JPMC
{
    public class ConcurrencyBudgetUtil
    {
        private RALogger _logger = RALogger.GetInstance(typeof(ConcurrencyBudgetUtil));
        
        private IAgentMgmtService _agentMgmtService = PlatformWindsorManager.GetService<IAgentMgmtService>();
        
        private IJobMonitorService _jobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        
        private IRMSubJobDao _subJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private List<JobType> _agentJobTypes = new List<JobType>
        {
            JobType.FSArchiverRestore,
            JobType.FSDataSynchronization,
            JobType.FSDataSynchronizationSchedule,
            JobType.FSDisposal,
            JobType.FSDisposalSchedule,
            JobType.FSDisposalByClassCode,
            JobType.FSRetain,
            JobType.FSRetainSimulate,
            JobType.FSCreateAndDestroyedFileReport,
            JobType.FSItemsFilesDueDisposal,
            JobType.DiscoveryAnalysisFileSystemV1,
            JobType.DiscoveryFileSystemV1,
            JobType.SPOnPremUniqueIDSettingFullSchedule,
            JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            JobType.SPOnPremApplySetting,
            JobType.SPOnPremDataSync,
            JobType.SPOnPremDataSyncSchedule,
            JobType.SPOnPremTermSynchronization,
            JobType.SPOnPremTermSynchronizationSchedule,
            JobType.SPOnPremEnforceRuleAction,
            JobType.SPOnPremEnforceRuleActionSchedule,
            JobType.SPOnPremItemsFilesDueDisposal,
            JobType.SPOnPremCreateAndDestroyedFileReport,
            JobType.SPOnPremScanLocalNodes,
        };

        private readonly FSHighPerformanceConfiguration _configuration;
        private readonly bool _enableJPMCFileSystemFeature;
        public ConcurrencyBudgetUtil()
        {
            _enableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            var config = FSHighPerformanceUtility.LoadFSHighPerformanceConfig();
            _configuration = config;
        }

        //public async Task<TenantResourceSnapshot> CaptureAsync(string tenantId)
        //{
        //    IList<RMAgentDto> agents = await _agentMgmtService.GetAvailableAgentsAsync(tenantId).ConfigureAwait(false);
        //    double avgCpu = agents.Count > 0 ? agents.Average(a => (double)a.CPUHZ) : 0;
        //    double avgMemMb = agents.Count > 0 ? agents.Average(a => ConvertToMB(a.TotalMemory)) : 0;

        //    return new TenantResourceSnapshot
        //    {
        //        TenantId = tenantId,
        //        AverageCpuHz = avgCpu,
        //        AverageTotalMemoryMb = avgMemMb,
        //        CapturedAtUtc = DateTime.UtcNow
        //    };
        //}

        //public async Task<int> CalMaxJobByAgent(string tenantId)
        //{
        //    TenantResourceSnapshot snapshot = await CaptureAsync(tenantId);
        //    var cpuHZForEachJob = _configuration.Setting.CpuHzForOneJob;
        //    int mbPerJob = _configuration.Setting.MemoryUsageForOneJobInMB;
        //    int maxJobPerAgent = _configuration.Setting.MaxJobPerAgent;

        //    int maxJobsByCpu = (int)Math.Floor(snapshot.AverageCpuHz / cpuHZForEachJob);
        //    int maxJobsByMemory = (int)Math.Floor(snapshot.AverageTotalMemoryMb / mbPerJob);
        //    return Math.Min(Math.Min(maxJobsByCpu, maxJobsByMemory), maxJobPerAgent);
        //}

        public async Task<int> CalMaxJobByTenant(string tenantId)
        {
            //var maxJobsByAgent = await CalMaxJobByAgent(tenantId);
            var agents = await _agentMgmtService.GetAvailableAgentsAsync(tenantId);
            return _configuration.Setting.MaxJobPerAgent * agents.Count;
        }

        public async Task<int> DetermineParallelSubJobCountAsync(string tenantId, int fallbackCount)
        {

            if (!_enableJPMCFileSystemFeature) return fallbackCount;
            var mainJobRunningCount = _jobMonitorService.GetRunningJobsCount(_agentJobTypes);
            var subJobRunningCount = _subJobDao.GetRunningAgentJob(_agentJobTypes).Count();
            var jobRunningCount = mainJobRunningCount + subJobRunningCount;
            _logger.Info($"mainJobRunningCount {mainJobRunningCount} subJobRunningCount {subJobRunningCount}");
            int maxJobPerUser = await CalMaxJobByTenant(tenantId);
            _logger.Info($"The max job per user is {maxJobPerUser} and the job is running count{jobRunningCount}");
            return Math.Max(maxJobPerUser - jobRunningCount, 0);
        }

        public async Task<bool> CheckRunableAgentJob(string tenantId)
        {
            if (!_enableJPMCFileSystemFeature) return true;
            var subJobRunningCount = _subJobDao.GetInProgressAgentJob(_agentJobTypes).Count();
            int maxJobPerUser = await CalMaxJobByTenant(tenantId);
            _logger.Info($"The number of job is running is {subJobRunningCount} and max job per user is {maxJobPerUser}");
            return maxJobPerUser >= subJobRunningCount;
        }

        private static double ConvertToMB(long mem)
        {
            if (mem <= 0) return 0;
            return mem / 1024d;
        }
    }

}
