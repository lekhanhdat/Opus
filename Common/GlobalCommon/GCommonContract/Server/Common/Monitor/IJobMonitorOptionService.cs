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



using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IJobMonitorOptionService
    {
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="jobMonitorParam"></param>
        /// <returns></returns>
        [OperationContract]
        MonitorModule GetJobMonitorModule(JobMonitorParam jobMonitorParam);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="jobDto"></param>
        /// <returns></returns>
        [OperationContract]
        JobSummaryInfos GetJobSummaryInfos(BaseJobDto jobDto);


        [OperationContract]
        JobSummaryInfos GetJobSummaryInfo(JobDetailSearchDto search);


        [OperationContract]
        JobDetailInfos GetJobDetailInfos(JobDetailSearchDto searchDto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="subJobDto"></param>
        /// <returns></returns>
        [OperationContract]
        JobSummaryInfos GetSubJobSummaryInfos(SubJobDto subJobDto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="subJobDto"></param>
        /// <param name="status"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [OperationContract]
        JobDetailInfos GetSubJobDetail(SubJobDto subJobDto, JobReportDetailStatus[] status, long from, long to);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        BaseJobDto LoadJobById(string jobId);

        [OperationContract]
        List<BaseJobDto> LoadJobByIds(List<string> jobIds);

        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        List<BaseJobDto> GetJobByPlanId(string planId);


        /// <summary>
        /// Method for RevIM Monitor AR Job
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [OperationContract]
        List<BaseJobDto> GetJobByString7(List<string> key);

        [OperationContract]
        string TestReportExportLocationPath(ReportExportLocationDto dto);

        [OperationContract]
        string ValidateReportExportLocation(ReportExportLocationDto dto);

        [OperationContract]
        void DeleteReportExportLocation(string id);

        [OperationContract]
        ReportExportLocationDto GetReportExportLocation();

        /// <summary>
        /// 获取与jobs相关联的所有job
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        [OperationContract]
        List<RelationJobGroupDto> GetRelationJobGroups(List<BaseJobDto> jobs);

        [OperationContract]
        string CheckJobReport(List<string> jobIds, bool isIncludesuccessfulJob);

        [OperationContract]
        BaseJobDto GetLatestJobByPlanId4API(string planId);
    }
}
