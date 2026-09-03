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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataJobManager
    {
        private readonly string _jobId;

        private readonly IRMReportManager _reportManager;

        private long _succeedCount = 0;

        private long _failedCount = 0;

        public string JobId => _jobId;

        public RMDeleteArchivedDataJobManager(string jobId)
        {
            _jobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, Contract.JobMonitor.JobType.DeleteRestoredData);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public void Init(int siteCount)
        {
            _reportManager.IncreaseBase(siteCount);
            _reportManager.StartUpdateJobProgress();
        }

        public void IncreaseProgress()
        {
            _reportManager.Increase();
        }

        public void AddSucceedDetail(RMDeleteArchivedDataSettingManager settingManager, string url, string restoredUrl, bool isRelated)
        {
            _succeedCount++;
            _reportManager.SendJobDetail(new JMArchiverDeleteRestoredDataJobDetails
            {
                Url = url,
                RestoredUrl = restoredUrl,
                CleanOption = settingManager.IsEnableDeleteRelatedVersion() ? "RM_AR_SPS_General_DelRelatedFileOrVersion" : "RM_AR_SPS_General_DelFileAndVersion",
                CleanDelayDays = settingManager.DailyDays(),
                IsRelatedDelete = isRelated ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                Status = JobDetailsStatus.Successful,
                Comment = ""
            });
        }

        public void AddFailedDetail(RMDeleteArchivedDataSettingManager settingManager, string url, string restoredUrl, bool isRelated, string comment)
        {
            _failedCount++;
            _reportManager.SendJobDetail(new JMArchiverDeleteRestoredDataJobDetails
            {
                Url = url,
                RestoredUrl = restoredUrl,
                CleanOption = settingManager.IsEnableDeleteRelatedVersion() ? "RM_AR_SPS_General_DelRelatedFileOrVersion" : "RM_AR_SPS_General_DelFileAndVersion",
                CleanDelayDays = settingManager.DailyDays(),
                IsRelatedDelete = isRelated ? "RM_JS_Common_Yes" : "RM_JS_Common_No",
                Status = JobDetailsStatus.Failed,
                Comment = comment
            });
        }

        public void Finish()
        {
            var status = Contract.RMWeb.JobMonitor.JobStatus.Finished;
            var comment = "";
            if (_succeedCount > 0 && _failedCount > 0)
            {
                status = Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }
            else if (_failedCount > 0 && _succeedCount == 0)
            {
                status = Contract.RMWeb.JobMonitor.JobStatus.Failed;
                comment = "RM_HS_Criteria_View_Msg_ValidOtherError";
            }

            _reportManager.SetJobFinished(status, comment);
        }

        public void Fail()
        {
            _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
        }

        public void Skip()
        {
            _reportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Skipped, "RM_Job_JobConflictOrNotExistData");
        }
    }
}
