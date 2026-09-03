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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Xml;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCJobReportManager
    {
        private readonly string _jobId;
        private readonly JobType _jobType;
        private readonly IRMReportManager _reportManager;

        private int _succeedCount = 0;
        private int _failedCount = 0;

        public string JobId => _jobId;
        public JobType JobType => _jobType;

        public string ErrorMessage { get; set; }

        public RMDeleteArchivedSCJobReportManager(string jobId, JobType jobType)
        {
            _jobId = jobId;
            _jobType = jobType;
            _reportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public RMDeleteArchivedSCJobReportManager(IRMReportManager reportManager)
        {
            _jobId = reportManager.JobId;
            _jobType = reportManager.JobType;
            _reportManager = reportManager;
        }

        public void Init()
        {
            ReportMangerFactory.Instance.Init(_jobId, _jobType);
            _reportManager.StartUpdateJobProgress();
        }

        public void IncreaseBase(long subInfoCount)
        {
            _reportManager.IncreaseBase(subInfoCount);
        }

        public void IncreaseProgress(int x = 1)
        {
            _reportManager.Increase(x);
        }

        public void AddSucceedDetail(string url, string jobId, long size, string storageName)
        {
            _succeedCount++;
            _reportManager.SendJobDetail(new JMDeleteArchivedSCJobDetails
            {
                Url = url,
                JobId = jobId,
                Size = size,
                SourceStorageName = storageName,
                Status = JobDetailsStatus.Successful,
                Comment = ""
            });
            IncreaseProgress();
        }

        public void AddSkipDetail(string url, string jobId, long size, string storageName, string comment = null)
        {
            _succeedCount++;
            _reportManager.SendJobDetail(new JMDeleteArchivedSCJobDetails
            {
                Url = url,
                JobId = jobId,
                Size = size,
                SourceStorageName = storageName,
                Status = JobDetailsStatus.Skipped,
                Comment = string.IsNullOrEmpty(comment) ? "" : comment
            });
            IncreaseProgress();
        }

        public void AddFailedDetail(string url, string jobId, long size, string storageName, string comment)
        {
            _failedCount++;
            _reportManager.SendJobDetail(new JMDeleteArchivedSCJobDetails
            {
                Url = url,
                JobId = jobId,
                Size = size,
                SourceStorageName = storageName,
                Status = JobDetailsStatus.Failed,
                Comment = comment
            });
            IncreaseProgress();
        }

        public string GetFullPath(string extraInfo, string url)
        {
            var document = new XmlDocument();
            document.LoadXml(extraInfo);
            var apUrlElements = document.GetElementsByTagName("HeaderExtraAttribute");
            if (apUrlElements != null && apUrlElements.Count > 0)
            {
                var apUrl = apUrlElements[0]?.Attributes["APUrl"]?.Value ?? url;
                return apUrl.Contains("\\") ? apUrl?.Replace("\\", "/") : apUrl;
            }
            return url;
        }

        public void Finish()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                _reportManager.SetJobFinished(JobStatus.Failed, ErrorMessage);
                return;
            }
            var status = JobStatus.Finished;
            var comment = "";
            if (_succeedCount > 0 && _failedCount > 0)
            {
                status = JobStatus.FinishWithException;
            }
            else if (_failedCount > 0 && _succeedCount == 0)
            {
                status = JobStatus.Failed;
                comment = "RM_HS_Criteria_View_Msg_ValidOtherError";
            }

            _reportManager.SetJobFinished(status, comment);
        }
    }
}
