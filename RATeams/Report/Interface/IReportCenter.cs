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

namespace M365GroupTeam
{
    #region directory

    using System;
    using System.Collections.Generic;
    
    using AvePoint.Common;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using ExchangeCommonWrapper;

    #endregion

    public interface IReportCenter
        : IDisposable
    {
        bool HasErrorNode { get; set; }
        //void SetJobFinish(JobStatus jobStatus, string comment = "");
        //void Finish(JobStatus status, string message = "");
        //void RecordFailed(JMJobDetails detail);
        //void RecordSuccessful(JMJobDetails detail);
        //void RecordSkip(JMJobDetails detail);
        void ResetReportManager(string jobId, bool needAddHoldReport = false);
        void AddRestoreReport(ReportDto detail);
        void SetErrorMessage(string message);
        void AddReportRecord(JMJobDetails detail, JobDetailsStatus status = JobDetailsStatus.None, bool isHold = false);
        void Finish();
        void EndDisposalStatistic(string mainJobId);
        void UpdateStatistics(ActionStatistics actionStatistics,ActionTab actionTab);
        void StopJob();
        JobStatus GetJobStatus();
        bool DecreaseTotalPhases(int count = 1);
        bool AdvanceToNextPhase();
    }
}