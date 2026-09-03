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




using System.ServiceModel;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Wcf;

namespace AvePoint.GCommon.Contract.Server.Job
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAJobStatusUpdater
    {
        /// <summary>
        /// 更新job进度
        /// </summary>
        /// <param name="jobInfo">必填的有Type（job type）   Id（job id）   isSubJob（是否是子job）   weight（子job权重）  progress（job进度） Stamp（时间戳） AgentHost（当前发消息的agent注册时填写的机器名或IP）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        JobUpdateState UpdateJobProgress(JobStatusInfo jobInfo);

        /// <summary>
        /// 更新job状态
        /// </summary>
        /// <param name="jobInfo">必填的有Type（job type）   Id（job id）   isSubJob（是否是子job）   weight（子job权重）  state（job状态） Stamp（时间戳） AgentHost（当前发消息的agent注册时填写的机器名或IP）</param>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool UpdateJobStatus(JobStatusInfo jobInfo);

    }
}
