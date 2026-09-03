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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.RMTasks;
using AvePoint.RA.Timer.Task;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.FileSystem;

namespace RATimerWorkerRole
{
    public class WorkerRole
    {
        RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        static string Lock_Id = "id_lock_task";
        static Random random4Sleep = new Random();
        IRMTaskJobDao taskJob;

        private static readonly ITimerInstanceDao TimerInstanceDao = PlatformWindsorManager.GetService<ITimerInstanceDao>();

        public void Run()
        {
            logger.Info("Timer.Worker entry point called.");
            ThreadPool.SetMinThreads(30, 30);

            var hostName = Dns.GetHostName();

            taskJob = new RMTaskJobDao();
            var CommonService = PlatformWindsorManager.GetService<ICommonService>();

            if(CommonService.IsPrimaryTimer(hostName))
            {
                logger.Info("Current role is primary.");
                TimerInstanceDao.ReleaseTask(TaskType.ObserveAOSNotification);
            }

            while (true)
            {
                try
                {
                    CommonService.RefreshTimer(hostName, DateTime.UtcNow.AddMinutes(-1).Ticks);

                    if (!Locked())
                    {
                        logger.Info($"start to process timer new task.");
                        using (var performance = new PerformanceScope($"TimerWorker.Run. memory used: {ProcessUtil.GetProcessMemoryMB()}"))
                        {
                            var tasks = taskJob.GetAllTask().Where(task => task != null).ToList();
                            logger.Info($"timer all task count: {tasks.Count}.");
                            foreach (var task in tasks)
                            {
                                try
                                {
                                    logger.Info($"process one timer task: {task.Id}, {task.Type}.");
                                    if (taskJob.LockTask(task))
                                    {
                                        logger.Info($"task lock success: {task.Id}, {task.Type}.");
                                        ThreadPool.QueueUserWorkItem(Process, task);
                                    }
                                    else
                                    {
                                        logger.Info($"task already locked, task:{task.Type}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error($"process task error:{task.Type}, ERROR:{ex.ToString()}");
                                }
                            }
                            ReleaseLocked();
                        }
                        RandomSleepSeconds(25, 35);
                    }
                    else
                    {
                        RandomSleepSeconds(15, 25);
                        logger.Info($"timer already locked.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Worker role failed to run task: {ex.ToString()}");
                    RandomSleepSeconds(15, 25);
                }
            }
        }

        private void RandomSleepSeconds(int minValue, int maxValue)
        {
            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details:  this method
            * Ignore Reason: random用于Thread.Sleep
            */
            int val = random4Sleep.Next(minValue, maxValue + 1);
            Thread.Sleep(val * 1000);
        }

        private void Process(object o)
        {
            TaskBase task = o as TaskBase;
            try
            {
                logger.Info($"begin to process task: {task.Type}, {task.Id}.");
                using (var performance = new PerformanceScope($"Task.Run.{task.Type}. memory used: {ProcessUtil.GetProcessMemoryMB()}"))
                {
                    var taskExecutor = TaskExecutorFactory.GetTaskExecutor(task.Type);

                    taskExecutor.ExecutorAsync(task).Wait();

                    taskJob.ReleaseTask(task);
                }
                logger.Info($"success to process task: {task.Id}, {task.Type}.");

            }
            catch (Exception ex)
            {
                logger.Error($"Execute task error {task.Id}, {task.Type}: {ex.ToString()}.");
            }
        }

        private bool Locked()
        {
            var profileId = GetCurrentEnv().Replace("_", "").Replace(" ", "") + "_" + DateTime.UtcNow.Ticks;
            var result = taskJob.LockTimer(new TimerLocker()
            {
                Id = Lock_Id,
                Status = RMTaskStatus.Processing,
                Timeout = 5,
                ProfileId = profileId
            });
            if (result.FaildType == RAFailedType.UpdateFailed)
            {
                logger.Warn($"Worker role locked, current:{profileId}.");
                return true;
            }
            return false;
        }
        private void ReleaseLocked()
        {
            taskJob.ReleaseTimer(new TimerLocker()
            {
                Id = Lock_Id,
                Type = TaskType.TimerLocker,
                Status = RMTaskStatus.Completed,
                ProfileId = string.Empty
            });
        }
        protected string GetCurrentEnv()
        {
            string result = string.Empty;
            try
            {
                result = RMGlobalConfiguration.EnvSetting.RoleId;
            }
            catch (Exception ex)
            {
                logger.Warn("Get current env failed: {0}.", ex.ToString());
                result = System.Net.Dns.GetHostName();
            }
            return result;
        }

        public void OnStart()
        {
            logger.Info($"RATimerWorkerRole - begin start, memory used: {ProcessUtil.GetProcessMemoryMB()}");
            // Set the maximum number of concurrent connections
			RMServiceManagerUtil.Init();

            try
            {
                var httpClient = new System.Net.Http.HttpClient();// do not remove this, otherwise the related dlls can't be load properly in coantiner env.
            }
            catch(Exception e)
            {
                logger.Error($"error occured when OnStart,error:{e}");
            }
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            RMDBContextManager.DisposeTenantMapping();
            GlobalConfig.InitCastle();

            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
            try
            {
                logger.Info($"begin to upgrade db, memory used: {ProcessUtil.GetProcessMemoryMB()}.");
                //new UpgradeDBModeTaskExecutor().ExecutorAsync(null).Wait();
                new DBUpgradeUtil().Execute();
                logger.Info($"success to upgrade db, memory used: {ProcessUtil.GetProcessMemoryMB()}.");
                logger.Info($"begin to upgrade cosmos data db, memory used: {ProcessUtil.GetProcessMemoryMB()}.");
                //new UpgradeDBModeTaskExecutor().ExecutorAsync(null).Wait();
                new UpgradeDataCosmosDbForJPMCUtil().ExecutorAsync().ExecuteAsyncTask();
                logger.Info($"success to upgrade cosmos data db, memory used: {ProcessUtil.GetProcessMemoryMB()}.");
            }
            catch (Exception ex)
            {
                logger.Error($"error upgrade db:{ex.ToString()}");
            }
            Trace.TraceInformation("RATimerWorkerRole has been started");
            logger.Info("RATimerWorkerRole has been started");
        }

    }
}
