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
using AvePoint.RA.RAPhysical.ExplorerTimer.Report;
//using AvePoint.RA.DB.Dao;
//using AvePoint.RA.DB.Model;
//using AvePoint.RA.DB.Dao.Impl;

namespace AvePoint.RA.RAPhysical.ExplorerTimer
{
    public class RMPhysicalExplorerTimerProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMPhysicalExplorerTimerProcessor));
        private bool mJobHasException = false;
        //private static Guid stampPhsicalTimerIncremental = new Guid("D6B71820-33BD-4570-9A23-3485E4FD7473");
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
        public RMPhysicalExplorerTimerProcessor(string jobId)
        {
            //ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExplorerTimer);
            ReportManager.StartUpdateJobProgress();
        }

        //private IRMPhysicalNodeFlagDao mPhysicalNodeInfoDao;
        //protected IRMPhysicalNodeFlagDao PhysicalNodeInfoDao
        //{
        //    get
        //    {
        //        if (mPhysicalNodeInfoDao == null)
        //        {
        //            mPhysicalNodeInfoDao = new RMPhysicalNodeFlagDao();
        //        }
        //        return mPhysicalNodeInfoDao;
        //    }
        //}

        public async Task RunNowAsync(string jobId)
        {
            try
            {
                RMPhysicalExplorerTimerBase explorerTimerBase = new RMPhysicalExplorerTimerBase();
                await explorerTimerBase.RunNowAsync();
                mJobHasException |= explorerTimerBase.HasError;
                //RMPhysicalNodeFlag physicalInfo = new RMPhysicalNodeFlag();
                //physicalInfo.NodeId = stampPhsicalTimerIncremental;
                //physicalInfo.CollectionTime = DateTime.UtcNow.Ticks;
                //PhysicalNodeInfoDao.AddPhysicalNodeInfo(physicalInfo);
            }
            catch(Exception ex)
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
