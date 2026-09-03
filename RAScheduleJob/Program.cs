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
//using AvePoint.RA.CommonUtil;

using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Configurations.Bootstrap;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Azure;
using Castle.MicroKernel.Proxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using RAExportCommon;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Util;

namespace AvePoint.RA.ScheduleJob
{
    internal class Program
    {
        private static string jobId;
        private static string mainjobId;
        private static string currentUser;
        private static IRALogger mLog = null;
        private static JobContext jobContext;
        private static JobQueueMessage jobQueueMessage;

        private static List<JobType?> AOSPJobList = new List<JobType?>()
        {
            JobType.AOSPRestore,
            JobType.DiscoveryAOSPJob,
            JobType.DiscoveryAOSPOptimization,
            JobType.DiscoveryAOSPOptimizationCalculate,
        };

        private static void Main(string[] args)
        {
            try
            {

#if DEBUG
                while (File.Exists("c:\\RevIMScheduleJob.sleep") || File.Exists("d:\\sleep.txt"))
                {
                    Thread.Sleep(1000);
                }
#endif

#if DEBUG
                RALogger.ConfigFile = "AgentLog4net.dev.config";
#else
                RALogger.ConfigFile = "AgentLog4net.config";
#endif
                InitTenantAndJobIdByArgs(args);
                InitLogger();
                Environment.SetEnvironmentVariable("MIP_APP_ID", "ED375797-1ADE-4CE3-8466-C36EF203A8E0", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("MIP_APP_NAME", "AvePoint Opus", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("MIP_APP_VERSION", "0.0.0l1", EnvironmentVariableTarget.Process);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if DEBUG
                RunInLocal(args);
#else
                RunInContainer(args);
#endif
                mLog.Info($"job ending ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                mLog?.Error($"Run job failed. {ex}");
                RALogger.WaitForAllLogsFlush();
                throw;
            }
            finally
            {
                RALogger.WaitForAllLogsFlush();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            mLog.Error($"Unhandled exception: {sender}, error:{ex?.ToString()}");
        }

        /// <summary>
        /// 本地环境Run Job的方法
        /// </summary>
        private static void RunInLocal(string[] args)
        {
            try
            {
                RMGlobalConfiguration.Init();

                jobQueueMessage = GetJobQueueMessage(TenantLocalValue.LogonGroupId, jobId);
                RealRun(args);
            }
            catch (Exception ex)
            {
                mLog.Error($"excute job: {string.Join(", ", args)}, error:{ex}");
            }
        }

        private static void RealRun(string[] args)
        {
            try
            {
                InitEnv(args);
            }
            catch (Exception ex)
            {
                mLog.Error($"init env error:{ex.ToString()}");
                var jobFailedContext = JobContext.GetInstance(jobId, GetJobTypeByArgs(args));
                jobFailedContext.ReportManager.SetJobFinished(JobStatus.Failed, string.Empty);
                //throw;
            }
            RealMain(args);
        }

        /// <summary>
        /// In container env, the values of args parameter are different from that in normal env(e.g. local, cloud service).
        /// </summary>
        /// <param name="args">it contains 3 params in sequence: job type, job id and tenant id</param>
        private static void RunInContainer(string[] args)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            try
            {
                var httpClient = new System.Net.Http.HttpClient();// do not remove this, otherwise the related dlls can't be load properly in coantiner env.
                RMGlobalConfiguration.Init();

                jobQueueMessage = GetJobQueueMessage(tenantId, jobId);
                var realArgs = GetCommandLineArgs(jobQueueMessage);
                RealRun(realArgs);
            }
            catch (Exception ex)
            {
                mLog.Error($"run job failed, tenantId:{tenantId}, jobId:{jobId}, Error:{ex.ToString()}");
                throw;
            }
        }

        private static string[] GetCommandLineArgs(JobQueueMessage jobQueueMsg)
        {
            string command = string.Empty;
            if (jobQueueMsg != null)
            {
                command = string.Format("{0} {1} {2}", jobQueueMsg.CommandLine, jobQueueMsg.JobTenantInfo?.TenantId, jobQueueMsg.JobTenantInfo?.RegisterEmail);
            }
            return command.Split(' ');
        }

        private static JobQueueMessage GetJobQueueMessage(string tenantId, string jobId)
        {
            string contextStr = null;
            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                var blobName = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(RecordsEnv.LogFolder, $"{tenantId}/JobContext/{jobId}.json");
                contextStr = File.ReadAllText(blobName);
            }
            else
            {
                var blobName = $"{tenantId}/{jobId}.json";
                contextStr = RAStorageUtil.GetBlobAsString(
                    RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING],
                    RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.JOB_CONTEXT_CONTAINER_NAME],
                    blobName);
            }

            if (string.IsNullOrEmpty(contextStr))
            {
                throw new Exception($"Job message {tenantId}-{jobId} not found.");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JobQueueMessage>(contextStr);
        }

        private static void RealMain(string[] args)
        {
            try
            {
                InitJobInfo(args);

                mLog.Info("Current process running under: {0}, MachineName: {1}", currentUser.LogBase64(), Environment.MachineName);

                if (args != null && args.Length > 0 && !string.IsNullOrEmpty(jobId))
                {
                    using (PerformanceScope scope = new PerformanceScope("Program.RealMain"))
                    {
                        try
                        {
                            using (CheckJobStopScope stopScope = new CheckJobStopScope())
                            {
                                RMJobProcessor.HandleMessageAsync(args, currentUser, jobQueueMessage).Wait();
                            }
                        }
                        catch (AggregateException ae)
                        {
                            if (ae.InnerExceptions != null)
                            {
                                foreach (var ex in ae.InnerExceptions)
                                {
                                    // Handle the JobStopException.
                                    if (ex is JobStopException)
                                    {
                                        throw ex;
                                    }
                                    // Rethrow any other exception.
                                    else
                                    {
                                        mLog.Error("error message: {1}.", ex.ToString());
                                    }
                                }
                            }
                            throw;
                        }
                    }
                    return;
                }
                else
                {
                    throw new ArgumentException($"Invalid Args:{string.Join(", ", args)}");
                }
            }
            //不分Sub Job的Job走这部分逻辑, 更新最后的Job状态
            catch (JobStopException ex)
            {
                mLog.Warn("Job ID: {0}, message: {1}.", mainjobId, ex.ToString());
                //var jobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                //jobService.UpdateJobStatus(mainjobId, JobStatus.Stopped);
                jobContext?.ReportManager.SetJobFinished(JobStatus.Stopped);
            }
            catch (Exception ex)
            {

                mLog.Error("Report job failed! jobId: {0}, error message: {1}.", mainjobId, ex.ToString());
                jobContext?.ReportManager.SetJobFinished(JobStatus.Failed, ex.Message);
                //var jobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                //jobService.UpdateJobStatus(mainjobId, JobStatus.Failed);
            }
            finally
            {
                PoolUserUtil.Dispose();
            }
        }

        private static void InitEnv(string[] args)
        {
            CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
            InitCastle();
            RMServiceManagerUtil.Init();
            StorageApiConfiguration.Setup();
            AsposeLicenseBootstrap.Setup();
            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));

            if (AOSPJobList.Contains(jobQueueMessage?.JobType))
            {
                TenantLocalValue.CallerType = "PartnerPortal";
            }

            PoolUserUtil.Init(true);
            if (jobQueueMessage?.JobType == JobType.ImportFSSetting || jobQueueMessage?.JobType == JobType.ExportFSSetting || jobQueueMessage?.JobType == JobType.ManualApprovalTimer || jobQueueMessage?.JobType == JobType.PhysicalRecordsDisposal || jobQueueMessage?.JobType == JobType.DownloadRCCReport)
            {
                InitSignalR();
            }
            InitCurrentUser(args);
        }

        private static void InitLogger()
        {
            RALogger.SeparateLogToTenant(TenantLocalValue.LogonGroupId, jobId);
            RALogger.SetCustomizedLogPostfix("V: " + WebUtil.GetProductVersion());
#if DEBUG
            RACustomLogger.Init(TenantLocalValue.LogonGroupId, jobId, true);
#else
            RACustomLogger.Init(TenantLocalValue.LogonGroupId, jobId, false);
#endif
            mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            LoggerFactory.CustomizedLoggerFactory = new OpusCustomizedLoggerFactory();
        }

        private static void InitTenantAndJobIdByArgs(string[] args)
        {
            Console.WriteLine($"job args: {string.Join(",", args)}");
            string tenantId = string.Empty, currentUser = string.Empty;
            jobId = args[1];
            
#if DEBUG
            tenantId = args[args.Length - 2];
            currentUser = args[args.Length - 1];
            TenantLocalValue.LogonUserEmail = currentUser;
#else
            tenantId = args[0];
#endif
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(jobId))
            {
                throw new ArgumentException($"invalid args, TenantId/JobId not found, {string.Join("; ", args)}.");
            }
            TenantLocalValue.LogonGroupId = tenantId;
        }

        private static void InitCurrentUser(string[] args)
        {
            if (args?.Length > 2)
            {
                currentUser = args[args.Length - 1];
                TenantLocalValue.LogonUserEmail = currentUser;
            }
        }

        private static void InitJobInfo(string[] args)
        {
            if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(jobId))
            {
                //从子job的Context中获取当前需要处理的节点.   更新进度和状态用JobInfoUpdater
                IRMSubJobDao SubJobDao = new RMSubJobDao();
                var subjob = SubJobDao.GetSubJob(jobId, false);
                if (subjob == null)
                {
                    throw new Exception($"Can not Find subJob by{jobId}");
                }
                mainjobId = subjob.ParentId;
                if (AvePoint.RA.Common.JobService.JobServiceUtility.IsFinalState(subjob.Status))
                {
                    throw new Exception($"This job {jobId} is final state {subjob.Status}");
                }
            }
            else
            {
                mainjobId = jobId;
            }
            //check job stop, TODO: subJob stop
            if (!string.IsNullOrEmpty(mainjobId))
            {
                CheckJobStatusUtility.Start(mainjobId);
            }
            var jobType = GetJobTypeByArgs(args);
            jobContext = JobContext.GetInstance(jobId, jobType);
        }

        private static JobType GetJobTypeByArgs(string[] args)
        {
            JobType jobType = JobType.None;
            Enum.TryParse<JobType>(args[0], out jobType);
            return jobType;
        }

        private static void InitCastle()
        {
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            WindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Register(
                Component.For<IWindsorContainer>().Instance(windsorContainer)
            );

            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                Path.Combine(installPath, "Config/Castle/ServiceCastle.config")));
            var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
            windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
            AppDomain.CurrentDomain.SetData("CoreIOCContainerIdentifier", windsorContainer);
            PlatformWindsorManager.SetUp(windsorContainer);
        }

        private static void InitSignalR()
        {
            try
            {
                Thread curr = new Thread(() =>
                {
                    mLog.Info("Begin to set up signalr server connection.");

                    ISignalRService signalrService = (ISignalRService)PlatformWindsorManager.GetService("AvePoint.RA.Service.Services.SignalR.SignalRService", typeof(ISignalRService));
                    signalrService.SignalRSetup();

                    mLog.Info("Successfully set up signalr server connection");

                });
                curr.Start();
                mLog.Info("Start thread to init sigalr setup.");

            }
            catch (Exception e)
            {
                mLog.Error("Fail to setup signalr server.", e);
            }

        }
    }
}
