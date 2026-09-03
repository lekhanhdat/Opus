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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;

namespace AvePoint.RA.FileSystem.Core
{
    public class JobSummaryService
    {
        protected AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public JobSummaryService()
        {
        }
        /// <summary>
        /// Post the summary and job state to manager server
        /// </summary>
        public void NotifyManager(int state, string jobid, bool isSubJob = true, string comment = "")
        {
            try
            {
                HBJobStatusInfo info = new HBJobStatusInfo()
                {
                    JobId = jobid,
                    State = state,
                    IsSubJob = isSubJob,
                    Comment = comment
                };
                JobContext.Current.ApiClient.UpdateJobState(info);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
            }
        }
    }
}
