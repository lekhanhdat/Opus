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




using System;
using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Wcf;

namespace AvePoint.GCommon.Contract.Server.Job
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAJobDetailService
    {
        /// <summary>
        /// update job detail into the job report file
        /// </summary>
        /// <param name="details"></param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateJobDetails(List<JobDetail> details, BaseJobDto jobInfo);

        /// <summary>
        ///  update sub job details report file
        /// </summary>
        /// <param name="jobSummaryList"></param>
        /// <param name="subJob">需要的字段 sub job id和parent id</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSubJobDetails(List<JobDetail> details, SubJobDto subJob);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void SaveJobContexts(List<JobContexts> contexts, BaseJobDto job);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<JobContexts> GetJobContexts(BaseJobDto job, JobContextSearchDto search);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void ExecutionInJobFolder(Action<string> action, BaseJobDto job);

        /// <summary>
        /// update job report file into the job report db file, only use for PR
        /// </summary>
        /// <param name="stream"> report file </param>
        /// <param name="fileName" report file name </param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateJobReportExtraFile(JobExtraFile jobReportFile, BaseJobDto jobInfo);

        /// <summary>
        /// update DPM solution center job detail into the job report file
        /// </summary>
        /// <param name="details"></param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateDPMSolutionJobDetails(List<SolutionJobDetail> details, SubJobDto subJobInfo);

        /// <summary>
        /// update job summary into the job report file
        /// </summary>
        /// <param name="details"></param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateJobSummary(List<JobSummary> jobSummaryList, BaseJobDto jobInfo);

        /// <summary>
        ///  update sub job summary report file
        /// </summary>
        /// <param name="jobSummaryList"></param>
        /// <param name="subJob">需要的字段 sub job id和parent id</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSubJobSummary(List<JobSummary> jobSummaryList, SubJobDto subJob);

        /// <summary>
        /// 检查对应key的summary是否存在
        /// </summary>
        /// <param name="job"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof (WcfException))]
        bool CheckJobSummaryByKey(BaseJobDto job, string key);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void HandleDeleteContent(List<SPTreeNodeDto> objectList, BaseJobDto jobInfo);

        /// <summary>
        /// 为SO提供的接口
        /// </summary>
        /// <param name="objectList"></param>
        /// <param name="jobInfo"></param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateApproveReport(List<ApproveReport> objectList, BaseJobDto jobInfo);

        /// <summary>
        /// 为CA提供更新Control DB内信息的接口
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="detail"></param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateCAJobDetails(string jobId, string detail);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSubJobAgentInfo(SubJobDto subJob);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateMainJobSummary(List<JobSummary> jobSummaryList, BaseJobDto jobInfo);

        [OperationContract]
        void UpdateSearchJobDetails(SubJobDto subJob, List<SearchJobDetail> details);
    }

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAJobDetailAndSummaryService : IAJobDetailService
    {
        /// <summary>
        ///  update sub job details report file
        /// </summary>
        /// <param name="jobSummaryList"></param>
        /// <param name="subJob">需要的字段 sub job id和parent id</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSubJobDetailsV2(List<JobDetail> details, SubJobDto subJob);

        /// <summary>
        ///  update sub job summary report file
        /// </summary>
        /// <param name="jobSummaryList"></param>
        /// <param name="subJob">需要的字段 sub job id和parent id</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSubJobSummaryV2(List<JobSummary> jobSummaryList, SubJobDto subJob);

        /// <summary>
        ///  commit sub job details report file
        /// </summary>
        /// <param name="subJob">需要的字段 sub job id和parent id</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void CommitSubJobDetailsAndSummary(SubJobDto subJob);
    }
}
