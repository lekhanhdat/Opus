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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    #endregion

    /// <summary>
    /// Provide a way of updating media job progress
    /// </summary>
    public static class JobStatusUpdater
    {
        static EventHandler<JobStatusInfoEventArgs> jobStatusInfoUpdated;
        readonly static Object syncRoot = new Object();

        public static event EventHandler<JobStatusInfoEventArgs> JobStatusInfoUpdated
        {
            add
            {
                lock (syncRoot)
                    jobStatusInfoUpdated += value;
            }
            remove
            {
                lock (syncRoot)
                    jobStatusInfoUpdated += value;
            }
        }

        public static void UpdateJobProgress(JobStatusInfo jobStatusInfo, Boolean isFinalStatusUpdateByMedia = false)
        {
            OnJobStatusInfoUpdated(new JobStatusInfoEventArgs(jobStatusInfo, isFinalStatusUpdateByMedia));
        }

        public static void UpdateJobProgress(JobProgressInfo jobProgressInfo, Boolean isFinalStatusUpdateByMedia = false)
        {
            OnJobStatusInfoUpdated(new JobStatusInfoEventArgs(jobProgressInfo, isFinalStatusUpdateByMedia));
        }

        static void OnJobStatusInfoUpdated(JobStatusInfoEventArgs jobStatusInfoArgs)
        {
            var temp = jobStatusInfoUpdated;
            if (temp != null)
                temp(null, jobStatusInfoArgs);
        }
    }
}
