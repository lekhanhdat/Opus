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
using AvePoint.GCommon.Contract.DeploymentManager.Message;
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMSolutionCenterHandleService
    {
        /// <summary>
        /// detail操作
        /// </summary>
        /// <param name="planDto"></param>
        /// <returns></returns>
        [OperationContract]
        List<SolutionDetailDTO> SolutionDetails(AbstractDMPlanDto planDto);
        /// <summary>
        /// 为gui的timer的detail操作
        /// </summary>
        /// <param name="jobIdList"></param>
        /// <returns></returns>
        [OperationContract]
        List<BaseJobDto> ShowHandleDetails(List<string> jobIdList);
        /// <summary>
        /// 为gui的timer的detail操作，关闭页面的情况
        /// </summary>
        /// <param name=""></param>
        /// <returns>Dictionary<jobId,srcTree></returns>
        [OperationContract]
        Dictionary<string, SPTreeNodeDto> ShowRemovingAndRetractingDetails();
        /// <summary>
        /// Active  Deactive Retract Remove RemoveVersion Upgrade Solution的Job
        /// </summary>
        /// <param name="plan"></param>
        [OperationContract]
        Dictionary<string, SPTreeNodeDto> HandleSolutions(AbstractDMPlanDto planDto, SCMessageType type);
        /// <summary>
        /// 获取HistoryVersion
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        [OperationContract]
        SPTreeNodeDto GetHistoryVersion(SPTreeNodeDto currentNode, List<HistoryVersion> historyVersions);
    }
}
