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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    /// <summary>
    /// 如果SubJob_Runnable_Runing和SubJob_Runnable_CanRun状态的sub job数量小于maxSubJobCount，那么会再把一些SubJob_Runnable_Waiting状态的sub job设置为SubJob_Runnable_CanRun状态
    /// </summary>
    public class WaitingSubJobTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(WaitingSubJobTaskExecutor));

        #region castle object
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private const int FullTextIndexTotalSubJobCounts = 5;

        private static readonly List<JobType> s_exceptJobTypes =
        [
            JobType.DiscoveryJob, .. RMO365TenantSubJobControlConstants.CONTROLLED_JOBS
        ];

        #endregion

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {

                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, SubJobChecker);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Wating Sub Job Checker Task error {0}", e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async Task SubJobChecker()
        {
            try
            {
                //1.get all running and rannable sub jobs count by job type
                //2.for each type sub job, if the count less than maxcount, then will change some jobs' status to canrun
                // var jobs = await JobMonitorService.GetRunningAndRunnableSubJobCount();
                var hasWaitingSubJobExceptControlledJobs = await SubJobDao.HasWaitingSubJobCountExpectJobTypesAsync(s_exceptJobTypes.ToArray());
                if (!hasWaitingSubJobExceptControlledJobs)
                {
                    mLogger.Info("No waiting sub job found except controlled job types, skip checking.");
                    return;
                }

                //3.get all waiting sub jobs group by job type
                var exceptJobTypeInts = s_exceptJobTypes.ConvertAll(t => (int)t);
                var jobs = await SubJobDao.GetWaitingSubJobsGroups(exceptJobTypeInts);
                foreach (var job in jobs)
                {
                    // Special handling for PhysicalSetPermission job type
                    if (job.Key == JobType.PhysicalSetPermission)
                    {
                        var runningJobs = SubJobDao.GetRunningSetPermissionJobIds();
                        if (runningJobs != null && runningJobs.Count > 0)
                        {
                            mLogger.Info("Already has physical permission job running. Id:{0}", string.Join(",", runningJobs));
                            continue;
                        }
                    }

                    // Get all running main jobs of this job type
                    var runningMainJobIds = await SubJobDao.GetSubJobsParentIdsAsync((int)job.Key);
                    var runningMainJobs = JMDao.GetRunningExpectStoppingJobs([.. runningMainJobIds]);

                    // Get max runnable sub job count based on running main job count
                    var maxRunnableSubJobCount = GetMaxSubJobCountByJobType(job.Key, runningMainJobs.Count);

                    // Get current running and runnable sub job count mapping by job type
                    var mainJobRunningAndSubJobCountDict = SubJobDao.GetRunningAndRunnableMainJobIdAndSubJobCountByJobType(job.Key);
                    var runningSubJobCount = mainJobRunningAndSubJobCountDict.Values.Sum();

                    // Calculate how many more sub jobs can be promoted to runnable
                    var canRunSubJobCount = maxRunnableSubJobCount - runningSubJobCount;
                    mLogger.Info("JobType : {0}, MaxRunnableSubJobCount: {1}, RunningSubJobCount: {2}, CanRunSubJobCount: {3}",
                                        job.Key, maxRunnableSubJobCount, runningSubJobCount, canRunSubJobCount);

                    if (canRunSubJobCount <= 0)
                    {
                        mLogger.Info("JobType : {0}, no capacity to promote waiting sub jobs.", job.Key);
                        continue;
                    }

                    var readyUpdateSubJobList = new List<RMSubJob>();

                    // Order running main jobs by priority desc and start time asc
                    var orderedMainJobs = runningMainJobs
                        .OrderByDescending(j => j.JobPriority)
                        .ThenBy(j => j.StartTime)
                        .ToList();

                    if (orderedMainJobs == null || orderedMainJobs.Count == 0)
                    {
                        mLogger.Info("JobType : {0}, no running main jobs found. Skip promoting.", job.Key);
                        continue;
                    }

                    // Pre-group waiting sub jobs per main job to avoid repeated scans
                    var waitingSubJobs = job.Value;
                    var orderedRunningMainJobIds = orderedMainJobs.Select(j => j.Id).ToHashSet();

                    // Filter waiting sub jobs whose parent main job is running
                    var filteredWaiting = waitingSubJobs
                        .Where(sj => orderedRunningMainJobIds.Contains(sj.ParentId))
                        .ToList();

                    // Group waiting sub jobs per main job
                    var perMainJobQueues = filteredWaiting
                        .GroupBy(sj => sj.ParentId)
                        .ToDictionary(g => g.Key, g => new Queue<RMSubJob>(g));

                    mLogger.Info($"JobType : {job.Key}, Running+Runnable: {runningSubJobCount}, Waiting: {filteredWaiting.Count}, Max: {maxRunnableSubJobCount}, Capacity: {canRunSubJobCount}. Start allocation by rules.");

                    // First ensure at least one running sub job per main job
                    foreach (var mainJob in orderedMainJobs)
                    {
                        if (canRunSubJobCount <= 0)
                        {
                            break;
                        }
                        var hasRunning = mainJobRunningAndSubJobCountDict.TryGetValue(mainJob.Id, out var runCnt) && runCnt > 0;
                        if (hasRunning)
                        {
                            continue;
                        }

                        if (perMainJobQueues.TryGetValue(mainJob.Id, out var q) && q.Count > 0)
                        {
                            readyUpdateSubJobList.Add(q.Dequeue());
                            canRunSubJobCount--;
                            mLogger.Info($"JobType : {job.Key}, promote one waiting sub job for main job {mainJob.Id} to ensure at least one running sub job.");
                        }
                    }

                    // Then allocate remaining capacity by priority groups with round-robin within each group
                    if (canRunSubJobCount > 0)
                    {
                        // Group main jobs by priority
                        var groupedByPriority = orderedMainJobs
                            .GroupBy(j => j.JobPriority)
                            .OrderByDescending(g => g.Key)
                            .ToList();

                        foreach (var priorityGroup in groupedByPriority)
                        {
                            if (canRunSubJobCount <= 0)
                            {
                                break;
                            }

                            var groupJobIds = priorityGroup.Select(j => j.Id).ToList();
                            var groupCount = groupJobIds.Count;
                            if (groupCount == 0)
                            {
                                continue;
                            }

                            if (groupCount == 1)
                            {
                                var jobId = groupJobIds[0];
                                if (perMainJobQueues.TryGetValue(jobId, out var q) && q.Count > 0)
                                {
                                    var takeCount = Math.Min(canRunSubJobCount, q.Count);
                                    for (var i = 0; i < takeCount; i++)
                                    {
                                        readyUpdateSubJobList.Add(q.Dequeue());
                                    }
                                    canRunSubJobCount -= takeCount;
                                }
                                continue;
                            }

                            // Multiple jobs at this priority: round-robin to achieve even distribution, honoring capacity
                            var anyAvailable = true;
                            while (canRunSubJobCount > 0 && anyAvailable)
                            {
                                anyAvailable = false;
                                foreach (var jobId in groupJobIds)
                                {
                                    if (canRunSubJobCount <= 0)
                                    {
                                        break;
                                    }

                                    if (perMainJobQueues.TryGetValue(jobId, out var q) && q.Count > 0)
                                    {
                                        readyUpdateSubJobList.Add(q.Dequeue());
                                        canRunSubJobCount -= 1;
                                        anyAvailable = true;
                                    }
                                }
                            }
                        }
                    }

                    var result = await SubJobDao.UpdateSubJobToRunnableByIdsAsync(readyUpdateSubJobList.Select(subJob => subJob.Id).ToList());
                    mLogger.Info("JobType {0} allocated {1} waiting sub jobs to runnable. Result:{2}", job.Key, readyUpdateSubJobList.Count, result);
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while check waiting sub job:{0}", ex.ToString());
            }
        }
        
        private int GetMaxSubJobCountByJobType(JobType jobType, int runningMainJobCount)
        {
            var maxSubJobCount = RMKeyValueDao.GetTotalSubJobCountFromDB((int)jobType);;
            if (maxSubJobCount == 0)
            {
                if (jobType == JobType.ArchiverFullTextIndex)
                {
                    mLogger.Info("For archiver full text index job not config use default max runnable sub job. Default count: {0}", FullTextIndexTotalSubJobCounts);
                    return FullTextIndexTotalSubJobCounts;
                }
                mLogger.Info("JobType : {0}, not config max runnable sub job count in db.", jobType);
                var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                maxSubJobCount = subJobCountInConfigFile * runningMainJobCount;
            }
            return maxSubJobCount;
        }
    }
}
