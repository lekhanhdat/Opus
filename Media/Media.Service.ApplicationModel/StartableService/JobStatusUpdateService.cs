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





namespace AvePoint.Media.Service
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceApplicationModel;

    #endregion using directives

    /// <summary>
    /// keep a common service of keeping the job living
    /// </summary>
    public class JobStatusUpdateService
        : Startable
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        readonly static Object syncRoot = new Object();

        Dictionary<String, JobStatusInfo> cachedRunningJobStatusInfoDictionary = new Dictionary<String, JobStatusInfo>();
        //TODO Records
        //IAJobStatusUpdater jobStatusUpdater = JobReportServiceFactory.CreateJobStatusUpdater();

        public override void InternalStart()
        {
            this.logger.Info(MediaServiceApplicationModelResource.JobStatusUpdateServiceStartBegin);
            JobStatusUpdater.JobStatusInfoUpdated += new EventHandler<JobStatusInfoEventArgs>(this.MediaJobStatusUpdater_JobStatusInfoUpdated);
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.Name = ServiceConstants.JobStatusUpdateServiceThreadName;
                while (true)
                {
                    lock (syncRoot)
                    {
                        var filterCachedRunningJobStatusInfoList = FilteRunningJobStatusInfoList(this.cachedRunningJobStatusInfoDictionary);

                        filterCachedRunningJobStatusInfoList.ForEach(jobStatusInfoItem =>
                        {
                            if (jobStatusInfoItem.Progress < 100)
                            {
                                try
                                {
                                    //TODO Records
                                    //JobProcessUtility.CheckIfJobCancelled(this.jobStatusUpdater.UpdateJobProgress(jobStatusInfoItem));
                                }
                                catch (Exception e)
                                {
                                    logger.Error(MediaServiceApplicationModelResource.JobStatusUpdateServiceStartException, jobStatusInfoItem.Id, e.ToString());
                                }
                            }
                        });
                    }
                    Thread.Sleep(60 * 1000);
                }
            });
            this.logger.Info(MediaServiceApplicationModelResource.JobStatusUpdateServiceStartEnd);
        }

        public override void InternalStop()
        {
            this.logger.Info(MediaServiceApplicationModelResource.JobStatusUpdateServiceStopBegin);
            JobStatusUpdater.JobStatusInfoUpdated -= new EventHandler<JobStatusInfoEventArgs>(this.MediaJobStatusUpdater_JobStatusInfoUpdated);
            this.logger.Info(MediaServiceApplicationModelResource.JobStatusUpdateServiceStopEnd);
        }

        private List<JobStatusInfo> FilteRunningJobStatusInfoList(Dictionary<String, JobStatusInfo> allJobInfoDictionary)
        {
            var filtedJobIdList = allJobInfoDictionary.Where(keyValuePair => keyValuePair.Value.Progress >= 100).Select(item => item.Key).ToList();
            filtedJobIdList.ForEach(key => { allJobInfoDictionary.Remove(key); });
            return allJobInfoDictionary.Values.ToList();
        }

        private void MediaJobStatusUpdater_JobStatusInfoUpdated(Object sender, JobStatusInfoEventArgs e)
        {
            lock (syncRoot)
            {
                IdentityManager.IdentityType = ServiceConstants.IdentityTypeJobId;
                IdentityManager.IdentityContent = e.JobStatus.Id.Split('_')[0];
                if (e.JobStatus.Progress < 100 && !e.IsFinalStatusUpdateByMedia)
                {
                    try
                    {
                        JobStatusInfo prevJobStatus;
                        if (this.cachedRunningJobStatusInfoDictionary.TryGetValue(e.JobStatus.Id, out prevJobStatus))
                        {
                            if (e.JobStatus.Progress != prevJobStatus.Progress)
                            {
                                //TODO Records
                                //JobProcessUtility.CheckIfJobCancelled(this.jobStatusUpdater.UpdateJobProgress(e.JobStatus));
                            }
                        }
                        else
                        {
                            //TODO Records
                            //JobProcessUtility.CheckIfJobCancelled(this.jobStatusUpdater.UpdateJobProgress(e.JobStatus));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(MediaServiceApplicationModelResource.JobStatusUpdateServiceStartException, e.JobStatus.Id, ex.ToString());
                    }
                }
                else
                {
                    if (e.IsFinalStatusUpdateByMedia)
                    {
                        e.JobStatus.Progress = 100;
                        try
                        {
                            //TODO Records
                            //this.jobStatusUpdater.UpdateJobStatus(e.JobStatus);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(MediaServiceApplicationModelResource.JobStatusUpdateServiceStartException, e.JobStatus.Id, ex.ToString());
                        }
                    }
                }
                this.cachedRunningJobStatusInfoDictionary.AddOrReplace(e.JobStatus.Id, e.JobStatus);
                if (e.IsFinal)
                    this.cachedRunningJobStatusInfoDictionary.Remove(e.JobStatus.Id);
            }
        }
    }
}