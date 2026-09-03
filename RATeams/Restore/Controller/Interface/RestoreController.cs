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

//namespace Office365GroupRestore
//{
//    using System;

//    using AvePoint.Common;
//    using AvePoint.GCommon.Contract.CloudServiceCommon;
//    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
//    using AvePoint.GCommon.Contract.Server.Job;
//    using AvePoint.GCommon.Contract.Server.Job.Object;
//    using AvePoint.GCommon.JobManagement;
//    using AvePoint.GCommon.JobManagement.Modules.ExchangeOnline.Interface;
//    using AvePoint.GCommon.MicroKernel;
//    using AvePoint.RA.CommonUtil;
//    using AvePoint.Wrapper.Common;

//    using ExchangeUtility.Graph;
//    using Microsoft365Backup.CommonUtil.Misc;


//    public class RestoreController : IController
//    {
//        private static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreController));

//        //public IMExchangeOnlineRestoreJobManagementService ERJobManagementService { get; set; }

//        //public IJobReportServiceFactory JobReportServiceFactory { get; set; }

//        public IRestoreWorker Worker { get; set; }

//        private ERMessage GetBackupMessageByJobId(string jobId, JobQueueMessage jobMessage)
//        {
//            ERMessage restoreMessage = null;
//            logger.Info("Start to receive request object from control.");
//            try
//            {
//                //var jobMessage = JobQueueMessageManager.Get(jobId);
//                SetIdentityManager(jobMessage.JobTenantInfo);
//                restoreMessage = ERJobManagementService.GetExchangeOnlineRestoreMessage(AveEnv.GetAgentTempFolder(ContextLevel.Process), jobMessage);
//                logger.Info("Receive request object successfully.");
//            }
//            catch (Exception ex)
//            {
//                logger.Error("An error occurred while receiving request object from control. Reason: {0}", ex.ToString());
//            }
//            return restoreMessage;
//        }

//        private static void SetIdentityManager(JobTenantInfo tenantInfo)
//        {
//            if (tenantInfo == null || string.IsNullOrEmpty(tenantInfo.TenantId)) throw new ArgumentNullException("tenantInfo");

//            IdentityManager.IdentityMode = IdentityMode.Process;
//            IdentityManager.IdentityType = MicroKernelConstant.IdentityTypeGroupId;
//            IdentityManager.IdentityContent = tenantInfo.TenantId;
//        }

//        public void Run(string jobId)
//        {
//            ERMessage message = null;
//            RestoreConfig config = null;
//            try
//            {
//                var jobqueueMessage = JobQueueMessageManager.Get(jobId);
//                message = GetBackupMessageByJobId(jobId, jobqueueMessage);
//                if (message == null)
//                {
//                    logger.Info("Get request object from control failed.");
//                    SetJobStatus(jobId);
//                    return;
//                }
//                logger.Info("Restore controller start running.");
//                Worker.Initailize(message, jobqueueMessage);
//                Worker.Run();
//            }
//            catch (Exception ex)
//            {
//                logger.Error("An error occurred while restore worker run. Reason: {0}", ex.ToString());
//            }
//            SafeDeleteTempFileInJobDir(config);
//        }

//        private void SafeDeleteTempFileInJobDir(RestoreConfig config)
//        {
//            try
//            {
//                if (config == null || string.IsNullOrEmpty(config.JobDir))
//                {
//                    logger.Warn("Cannot find job dir.");
//                    return;
//                }
//                foreach (var file in System.IO.Directory.GetFiles(config.JobDir, "*.fts"))
//                {
//                    System.IO.File.Delete(file);
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("Failed to delete temp file in job dir: {0}, error: {1}", config?.JobDir, ex);
//            }
//        }

//        private void SetJobStatus(string jobId)
//        {
//            var jobStatusService = JobReportServiceFactory.CreateJobStatusUpdater();
//            var jobInfo = new JobStatusInfo { AgentHost = AveEnv.AgentAddress, Id = jobId, Type = 0, Progress = 0 };
//            jobStatusService.UpdateJobStatus(jobInfo);
//        }
//    }
//}