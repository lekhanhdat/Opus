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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Timer
{
    public class RMConnectorTimerProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMConnectorTimerProcessor));
        private bool mJobHasException = false;        
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        public RMConnectorTimerProcessor(string jobId)
        {           
            ReportManager.StartUpdateJobProgress();
        }

        public async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                RMConnectorTimerBase explorerTimerBase = new RMConnectorTimerBase();
                await explorerTimerBase.RunNowAsync();
                mJobHasException |= explorerTimerBase.HasError;                
            }
            catch (Exception ex)
            {
                mJobHasException = true;
                logger.Warn($"Error in run physical explorer timer job, reason : {ex.ToString()}.");
            }
            finally
            {
                if (mJobHasException)
                {
                    ReportManager.SetJobFinished(JobStatus.FinishWithException);
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.Finished);
                }
            }
        }
    }
}
