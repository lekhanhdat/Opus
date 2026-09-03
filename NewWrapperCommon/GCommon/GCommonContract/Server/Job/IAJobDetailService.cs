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
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Wcf;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System.IO;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;

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
        void UpdateDPMSolutionJobDetails(List<SolutionJobDetail> details, BaseJobDto jobInfo);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateHealthAnalyzerScanDetails(List<HealthAnalyzerJobDetail> details, BaseJobDto jobInfo, bool isUpdate = false);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void CreateHealthAnalyzerScanDetails(List<HealthAnalyzerJobDetail> details, BaseJobDto jobInfo);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateCompareJobDetails(List<CompareJobDetail> detail, BaseJobDto jobInfo);

        /// <summary>
        /// update job summary into the job report file
        /// </summary>
        /// <param name="details"></param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        /// <param name="replaceIfExist">对于已存在Summary记录是继续插入数据还是替换数据(key和subJob均相同视为相同记录)</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateOrReplaceJobSummary(List<JobSummary> jobSummaryList, BaseJobDto jobInfo, bool replaceIfExist);

        /// <summary>
        /// update job summary into the job report file
        /// </summary>
        /// <param name="details"></param>
        /// <param name="JobInfo">必须有值的4个字段  Id（job的id）、Category（plan的Category）、PlanId、Type（job type）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateJobSummary(List<JobSummary> jobSummaryList, BaseJobDto jobInfo);

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
        /// 为小migration提供的接口
        /// </summary>
        /// <param name="job"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        int Execute(BaseJobDto job, string sql);

        /// <summary>
        /// 为DPM AppUpdate提供的接口
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateAppUpgradeJobDetails(List<AppCheckJobDetail> detail, BaseJobDto jobInfo);

        /// <summary>
        /// 以byte[]数组形式传输文件
        /// </summary>
        /// <param name="Buffer">文件的Byte[]，支持buffer</param>
        /// <param name="jobInfo"></param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateJobReportExtraBlobFile(JobExtraBlobFile Buffer, BaseJobDto jobInfo);

        /// <summary>
        ///处理DPM SPCAF Report
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="jobInfo"></param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void HandleSPCAFReport(JobExtraBlobFile Buffer, BaseJobDto jobInfo);
    }
}
