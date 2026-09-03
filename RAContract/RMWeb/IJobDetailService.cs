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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IJobDetailService
    {
        string DownloadReports(BaseJobDto jobInfo);
        void SyncJobDetails(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo);
        void UpdateJobDetails(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo);
        bool MergeJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobInfo);
        bool InsertMainJobDetails(BaseJobDto sourceJobInfo, BaseJobDto targetJobInfo);
        IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo);
        IEnumerable<JMJobDetails> GetDataForRetentionSimulateDetails(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo);
        IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo);
        IEnumerable<JMJobDetails> GetDataForTermSelection(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo);
        JMJobDetails GetDataForSOSummaryDetails(string conditionFilter, BaseJobDto jobInfo);
        JMJobDetails GetDataForRestoreSummaryDetails(string conditionFilter, BaseJobDto jobInfo);
        void ClearSOSummaryDetails(BaseJobDto jobInfo);
        void UploadJobDetailsAndReport(BaseJobDto jobInfo);
        void UploadJobDetailsAndReportToTempLocation(BaseJobDto jobInfo);
        void UploadReportFile(BaseJobDto jobInfo);
        void SetInnerReport(IRMReportService reportService);
        bool SendReport(HBReportFileInfo hBReportInfo);

        void RemoveDuplicateDataOfJobDetails(BaseJobDto jobInfo);

        System.Threading.Tasks.Task MigrateToRptAndDeleteAsync(string mainJobId, int jobType);
        Task<bool> UpdateRemainingSubJobStatusAsync(string mainJobId, HashSet<int> originalStatuses, int newStatus);
    }
}
