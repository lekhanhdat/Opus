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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using RAFileSystemCore.Common.JobHandler;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystem.FileSystem.BaseProcessor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SerializerHelper = AvePoint.GCommon.Utility.SerializerHelper;
using AvePoint.RA.Common.Tracking.Performance;

namespace AvePoint.RA.FileSystem
{
    class Program
    {
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            string jobId = string.Empty;
            CancellationTokenSource jobCts = null;
            JobStopMonitor stopMonitor = null;
            try
            {
#if DEBUG
                while (File.Exists(@"c:\\fs.sleep"))
                {
                    System.Threading.Thread.Sleep(3000);
                }
#endif

                if (args == null || args.Length < 3)
                {
                    throw new Exception("args is invalid.");
                }

                ServiceInitializationUtil.InitServicePoint();

                jobId = args[0];

                TenantAgentInfo.JobId = jobId;
                TenantAgentInfo.AgentId = args.Length > 3 ? args[3] : string.Empty;
                TenantAgentInfo.TenantRegisterEmail = args.Length > 4 ? args[4] : string.Empty;
                JobType action = (JobType)Enum.Parse(typeof(JobType), args[1]);
                string tenantId = args[2];
                string extensions = args.LastOrDefault();
                InitLogger(tenantId, jobId);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                CommonConfiguration.InitAppSetting();

                var jobMessage = GetJobMessage(action, jobId);
                if (string.IsNullOrWhiteSpace(jobMessage))
                {
                    throw new Exception("Job message is null.");
                }
                FipsModeUtil.InitControlCryptoMode();
                CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
                JobContext.Current.Init(jobId, GetJobType(action));

                RMPerformanceMonitor.Logger = (message) => new AveLogger(typeof(RMPerformanceMonitor)).Metric(message);

                FSServiceLocator locator = new FSServiceLocator();
                var worker = locator.Lookup(action, extensions);
                if (worker != null)
                {
                    worker.Bind(jobMessage);
                    try
                    {
                        FSJobCache.Instance.EnableJPMC = ExternalUtil.CheckEnableFSJPMCFeature(extensions);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"this job check Enablejpmc failed,error:{e}");
                    }

                    if (worker is ISupportsCancellation cancellableWorker)
                    {
                        jobCts = new CancellationTokenSource();
                        stopMonitor = new JobStopMonitor(jobId, jobCts);
                        cancellableWorker.SetCancellationToken(jobCts.Token);
                        logger.Info("Stop support enabled for job {0}.", jobId);
                    }

                    worker.Run();
                }
            }
            catch (AgentJobStopException)
            {
                logger.Info("Job {0} was stopped by user request.", jobId);
                HandleJobStopped();
            }
            catch (FSSkipJobException)
            {
                logger.Info("Job {0} was skipped and set finished due to no node match term.", jobId);
                HandleDisposalClassCodeJobSkipped();
            }
            catch (Exception e)
            {
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to cleanup. Error:{0}", ex.ToString());
                }
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    try
                    {
                        HybridApiClient.Instance.UpdateJobState(jobId, (int)JobStatus.Failed, "RM_SS_CommonErrorMessage");
                    }
                    catch { }
                }
                logger.Error("An error occurred while running file system job. Error: " + e.ToString());
            }
            finally
            {
                stopMonitor?.Dispose();
                jobCts?.Dispose();

                PerformanceMonitor.WritePerformanceResult();
                RMPerformanceMonitor.LogSummary();
                AveLogger.WaitForAllLogsFlush();
            }
        }

        private static int GetJobType(JobType type)
        {
            int jobType = -1;
            switch (type)
            {
                case JobType.FSDataSync:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSDataSynchronization;
                    break;
                case JobType.FSContentDueReport:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSItemsFilesDueDisposal;
                    break;
                case JobType.FSCreationAndDestructionReport:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSCreateAndDestroyedFileReport;
                    break;
                case JobType.FSDisposal:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSDisposal;
                    break;
                case JobType.FSDisposalByClassCode:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSDisposalByClassCode;
                    break;
                case JobType.SharePointOnPremApplySetting:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremApplySetting;
                    break;
                case JobType.SPOnPremTermSynchronization:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremTermSynchronization;
                    break;
                case JobType.SharePointOnPremEnforceRuleAction:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremEnforceRuleAction;
                    break;
                case JobType.SharePointOnPremDataSync:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremDataSync;
                    break;
                case JobType.SPOnPremUniqueIDSetting:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingFullSchedule;
                    break;
                case JobType.SPOnPremGlobalSearch:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.GlobalSearchAction;
                    break;
                case JobType.SPOnPremScanNode:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.SPOnPremScanLocalNodes;
                    break;
                case JobType.FSArchiverRestore:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSArchiverRestore;
                    break;
                case JobType.FSRetain:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSRetain;
                    break;
                case JobType.FSRetainSimulate:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.FSRetainSimulate;
                    break;
                case JobType.FSDiscovery:
                    jobType = (int)AvePoint.RA.Contract.JobMonitor.JobType.DiscoveryFileSystemV1;
                    break;
            }
            return jobType;
        }

        private static void InitLogger(string tenantId, string jobId)
        {
            logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            AveLogger.SetThreadJobId(jobId, false);
            logger.Info("Records Agent - Logger initialized.");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                logger.Error("Unhandled exception: {0}", e.ExceptionObject.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("Catch Unhandled:{0}", ex.ToString());
            }
            finally
            {
                AveLogger.WaitForAllLogsFlush();
            }
        }

        private static string GetJobMessage(JobType type, string jobId)
        {
            logger.Debug($"Begin get job message for {jobId}, action {type}");
            string message = string.Empty;
            switch (type)
            {
                case JobType.FSDataSync:
                case JobType.FSContentDueReport:
                case JobType.FSCreationAndDestructionReport:
                    message = HybridApiClient.Instance.GetJobMessage(jobId, type);
                    break;
                case JobType.FSDisposal:
                    message = HybridApiClient.Instance.GetDisposalJobMessage(jobId);
                    break;
                case JobType.FSDisposalByClassCode:
                    message = HybridApiClient.Instance.GetDisposalByClassCodeJobMessage(jobId);
                    break;
                case JobType.FSArchiverRestore:
                    message = HybridApiClient.Instance.GetFSArchiverRestoreJobMessage(jobId);
                    break;
                case JobType.FSRetain:
                case JobType.FSRetainSimulate:
                    message = HybridApiClient.Instance.GetFSRetainJobMessage(jobId);
                    break;
                case JobType.SharePointOnPremApplySetting:
                case JobType.SharePointOnPremEnforceRuleAction:
                case JobType.SPOnPremTermSynchronization:
                case JobType.SharePointOnPremDataSync:
                case JobType.SPOnPremUniqueIDSetting:
                case JobType.SPOnPremGlobalSearch:
                    message = HybridApiClient.Instance.GetSPJobMessage(jobId, type);
                    break;
                case JobType.SPOnPremScanNode:
                    message = "SPOnPremScanNode";
                    break;
                case JobType.FSDiscovery:
                    message = HybridApiClient.Instance.GetFSDiscoveryJobMessage(jobId);
                    break;
                default:
                    break;
            }
            return message;
        }

        private static void HandleJobStopped()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to cleanup after job stop. Error: {0}", ex.ToString());
            }

            try
            {
                string jobId = JobContext.Current.JobId;
                HybridApiClient.Instance.UpdateJobState(jobId, (int)JobStatus.Stopped, "RM_JS_JM_Status_Stopped");
                logger.Info("Job {0} reported as Stopped to OPUS.", jobId);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to report Stopped status to OPUS. Error: {0}", ex.ToString());
            }
        }

        private static void HandleDisposalClassCodeJobSkipped()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception ex)
            {
                logger.Error("No node match term. Error: {0}", ex.ToString());
            }
            try
            {
                string jobId = JobContext.Current.JobId;
                HybridApiClient.Instance.UpdateJobState(jobId, (int)JobStatus.Finished, "RM_JS_JM_Status_Finished");
                logger.Info("Job {0} reported as finished to OPUS.", jobId);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to report Skipped status to OPUS. Error: {0}", ex.ToString());
            }
        }
    }
}
