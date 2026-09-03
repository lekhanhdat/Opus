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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Wcf;


namespace AvePoint.GCommon.Contract.Server.Job
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMJobMonitorService
    {
        [OperationContract]
        void TestService();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string CreateJob(BaseJobDto dto);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        BaseJobDto LoadJobById(string jobId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">
        /// Need below parameters.
        /// int start = param.Start;
        /// int length = param.Length;
        /// int type = param.Type;
        /// string orderBy = param.OrderBy;
        /// </param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<BaseJobDto> GetJobs(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void DeleteJob(BaseJobDto jobDto);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void DeleteJobs(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        object InvokeHandlerMethod(string type, string methodName, JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<BaseJobDto> GetJobsByFilter(JobMonitorParameter param);



        /// <param name="param">
        /// int start = param.Start;
        /// int length = param.Length;
        /// int jobType = param.Type;
        /// string orderBy = param.OrderBy;
        /// Dictionary<string, List<object>> filter = param.Filter;
        /// </param>

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobMonitorQueryResult GetFilteredJobsAndPaging(JobMonitorParameter param);



        /// <param name="param">
        /// param.Type; required.
        /// param.PropName; required.
        /// </param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<object> GetDistinctValues(JobMonitorParameter param);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateProgress(string jobId, int progress);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        long FetchUpdateTime(int jobType);

    }
}
