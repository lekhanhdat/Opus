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
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract;
using RAFileSystemCore.Common.JobHandler;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

namespace AvePoint.Hybrid.AgentService.ServiceEndpoint
{
    public interface IEPJobService
    {
        void StartJob(RecordsJobArgs args);
        void StopJob(string jobId);
    }

    public class EPJobService : IEPJobService
    {
        private static readonly AvePoint.GCommon.AveLogger logger =
            AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private const int PipeConnectTimeoutMs = 3000;

        public void StartJob(RecordsJobArgs args)
        {
            try
            {
                if (args == null)
                {
                    throw new Exception("args is null");
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(Constants.RecordsWorkerExe);
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.UseShellExecute = false;
                string[] arguments = { args.JobId, args.JobType.ToString(), args.TenantId, args.AgentId, args.TenantRegisterEmail, args.Extensions };
                if (arguments != null && arguments.Length != 0)
                {
                    startInfo.Arguments = string.Format("{0} {1} {2} {3} {4} {5}", arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
                }
                startInfo.Verb = "runas";
                if (!StartJobWatcher.Exists(args.JobId))
                {
                    Process.Start(startInfo);
                    logger.Info("Start job successfully. Jobid: " + args?.JobId);
                    StartJobWatcher.Add(args.JobId);
                }
                else
                {
                    logger.Warn($"This job:{args.JobId} was already started, will not start again.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while starting job. JobId: {args?.JobId} Error: {e.ToString()}");
            }
        }

        public void StopJob(string jobId)
        {
            string pipeName = JobStopMonitor.GetPipeName(jobId);
            try
            {
                using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                {
                    client.Connect(PipeConnectTimeoutMs);
                    logger.Info("Stop signal delivered via pipe for job {0}.", jobId);
                }
            }
            catch (TimeoutException)
            {
                logger.Warn("Pipe connect timed out for job {0}. Worker may have already finished.", jobId);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to deliver stop signal via pipe for job {0}. Error: {1}", jobId, ex.Message);
            }
        }
    }
}