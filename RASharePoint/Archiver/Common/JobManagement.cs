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
namespace AvePoint.RA.SharePoint.Archiver
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Linq;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon;

    public class JobManagement : IAJobStatusUpdater
    {


        /// <summary>
        /// 操作Control数据信息，如Communication Key
        /// </summary>
        public JobManagement()
        {
        }

        public bool UpdateJobStatus(JobStatusInfo jobInfo)
        {
            return true;
        }

        public JobUpdateState UpdateJobProgress(JobStatusInfo jobInfo)
        {
            return JobUpdateState.Successful;
        }

        public int GetJobState(JobStatusInfo jobInfo)
        {
            int jobState = 0;
            return jobState;
        }

        public bool IsFinalState(int jobState)
        {
            return false;
        }

        public void UpdateJobDetails(List<JobDetail> details, BaseJobDto jobInfo)
        {
        }

        public void UpdateJobSummary(List<JobSummary> jobSummaryList, BaseJobDto jobInfo)
        {
        }

        public void UpdateSubJobAgentInfo(SubJobDto subJob)
        {
        }
    }
}
