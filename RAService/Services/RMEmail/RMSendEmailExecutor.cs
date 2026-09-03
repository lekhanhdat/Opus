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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMEmail
{
    public class RMSendEmailExecutor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSendEmailExecutor));

        private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;

        private readonly RMEmailSender _emailSender;

        public RMSendEmailExecutor(string jobId, string prefix)
        {
            _emailSender = new(new RMEmailRedisStorage(prefix, new RMEMailStorageManualMiddleware()));

            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.SendEmailJob);
            _reportManager.StartUpdateJobProgress();
            _reportManager.IncreaseBase(10000);
        }

        public async System.Threading.Tasks.Task Run()
        {
            try
            {
                _ = System.Threading.Tasks.Task.Run(async () =>
                {
                    while (true)
                    {
                        await System.Threading.Tasks.Task.Delay(1000 * 60);
                        _reportManager.Increase();
                    }
                });

                await _emailSender.SendAsync();
                _reportManager.SetJobFinished(RMWeb.JobMonitor.JobStatus.Finished);
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while send email. Error: {e}");
                _reportManager.SetJobFinished(RMWeb.JobMonitor.JobStatus.Failed);
            }
        }
    }
}
