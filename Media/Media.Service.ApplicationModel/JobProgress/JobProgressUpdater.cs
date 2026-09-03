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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceApplicationModel;

    #endregion

    public class JobProgressUpdater : IJobProgressUpdater
    {
        public int CurrentProgress { get; set; }
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void UpdateJobProgress(JobStatusInfo jobStatusInfo, Int64 maxItemNum, Int64 sendItemNum, Boolean isFinalStatusUpdateByMedia = false)
        {
            Int32 tempProgress = (Int32)((sendItemNum * 1.0 / maxItemNum) * 100);
            jobStatusInfo.IsSubJob = jobStatusInfo.Id.Contains("_") ? true : false;
            try
            {
                if (!isFinalStatusUpdateByMedia)
                {
                    this.CurrentProgress = tempProgress;
                    jobStatusInfo.Progress = this.CurrentProgress >= 100 ? 99 : this.CurrentProgress;
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressStartUpdate, jobStatusInfo.Progress);
                    JobStatusUpdater.UpdateJobProgress(jobStatusInfo, isFinalStatusUpdateByMedia);
                }
                else
                {
                    JobStatusUpdater.UpdateJobProgress(jobStatusInfo, isFinalStatusUpdateByMedia);
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressEndUpdate);
                }
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressError, e.ToString());
            }
        }

        public void UpdateJobProgress(JobProgressInfo jobProgressInfo, Int64 maxItemNum, Int64 sendItemNum, Boolean isFinalStatusUpdateByMedia = false)
        {
            Int32 tempProgress = (Int32)((sendItemNum * 1.0 / maxItemNum) * 100);
            jobProgressInfo.IsSubJob = jobProgressInfo.Id.Contains("_") ? true : false;
            try
            {
                if (!isFinalStatusUpdateByMedia)
                {
                    this.CurrentProgress = tempProgress;
                    jobProgressInfo.Progress = this.CurrentProgress >= 100 ? 99 : this.CurrentProgress;
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressStartUpdate, jobProgressInfo.Progress);
                    JobStatusUpdater.UpdateJobProgress(jobProgressInfo, isFinalStatusUpdateByMedia);
                }
                else
                {
                    JobStatusUpdater.UpdateJobProgress(jobProgressInfo, isFinalStatusUpdateByMedia);
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressEndUpdate);
                }
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressError, e.ToString());
            }
        }

        public void UpdateJobProgress(JobStatusInfo jobStatusInfo, bool isFinalStatusUpdateByMedia = false)
        {
            try
            {
                jobStatusInfo.Progress = jobStatusInfo.Progress >= 100 ? 99 : jobStatusInfo.Progress;
                if (!isFinalStatusUpdateByMedia)
                {
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressStartUpdate, jobStatusInfo.Progress);
                    JobStatusUpdater.UpdateJobProgress(jobStatusInfo, isFinalStatusUpdateByMedia);
    }
                else
                {
                    JobStatusUpdater.UpdateJobProgress(jobStatusInfo, isFinalStatusUpdateByMedia);
                    logger.Info(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressEndUpdate);
                }
            }
            catch (Exception e)
            {
                logger.Error(MediaServiceApplicationModelResource.JobProgressUpdaterUpdateJobProgressError, e.ToString());
            }
        }
    }
}