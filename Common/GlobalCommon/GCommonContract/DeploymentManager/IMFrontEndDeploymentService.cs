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
using AvePoint.GCommon.Contract.DeploymentManager.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMFrontEndDeploymentService
    {
        #region Plan 储存逻辑

        /// <summary>
        /// 创建Front End的plan
        /// </summary>
        /// <param name="fewPlan"></param>
        /// <returns></returns>
        [OperationContract]
        string CreatePlan(FrontEndDeploymentPlanDto fewPlan);

        /// <summary>
        /// 修改Front End的plan
        /// </summary>
        /// <param name="fewPlan"></param>
        /// <returns></returns>
        [OperationContract]
        string UpatePlan(FrontEndDeploymentPlanDto fewPlan);

        /// <summary>
        /// 根据id删除plan
        /// </summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeletePlanById(string id);

        /// <summary>
        /// 根据Front End对象删除Plan
        /// </summary>
        /// <param name="fewPlan"></param>
        [OperationContract]
        void DeletePlanByObj(FrontEndDeploymentPlanDto fewPlan);

        /// <summary>
        /// 批量删除plan
        /// </summary>
        /// <param name="ids"></param>
        [OperationContract]
        void DeletePlans(List<string> ids);

        /// <summary>
        /// 根据id获得当前Plan信息。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        FrontEndDeploymentPlanDto GetPlanById(string id);

        /// <summary>
        /// 获得Front End全部类型的plan
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<FrontEndDeploymentPlanDto> GetAllPlans();

        /// <summary>
        /// 根据planType获得plan
        /// </summary>
        /// <param name="planType"></param>
        /// <returns></returns>
        [OperationContract]
        List<FrontEndDeploymentPlanDto> GetAllPlansByPlanType(int planType);

        #endregion

        #region Plan 验证逻辑

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fewPlan"></param>
        /// <returns></returns>
        [OperationContract]
        string ValidatePlan(FrontEndDeploymentPlanDto fewPlan);

        /// <summary>
        /// 验证Plan Name是否重复。
        /// </summary>
        /// <param name="dmPlan"></param>
        /// <param name="isPlanType">这个参数为可选参数，默认为-1,表示在所有类型当中进行判断,当传递其他类型的时候，则按照其类型进行判断name是否重复</param>
        /// <param name="isFarmOrAgent">这个参数为可选参数，默认为空字符串，当传递agentId后，则会根据Agent Id进行判断name是否重复</param>
        /// <returns></returns>
        [OperationContract]
        bool IsDuplicateFrontEndName(string name, int planType = -1, string agentId = "");

        #endregion

        #region 业务逻辑

        /// <summary>
        /// 用来处理plan即将运行时从页面传递的一些数据信息。
        /// </summary>
        /// <param name="param"></param>
        [OperationContract]
        void SaveNewRunNowPlan(RunNowParam param);

        /// <summary>
        /// 处理与Client交互的一些业务逻辑
        /// </summary>
        /// <param name="dmPlan"></param>
        [OperationContract]
        void Run(FrontEndDeploymentPlanDto plan);

        /// <summary>
        /// 获得当前正在运行的plan
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        FrontEndDeploymentPlanDto GetRunnablePlan(string planId, long startTime);
        #endregion
    }
}
