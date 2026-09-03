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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMSolutionCenterService
    {
        /*
         * GetAllFarms();
         * GetSolutionsByAgentId(string agentId);
         * GetSolutionsByfarmId(string farmId, int solutionLevel);
         * GetAllAgents();
         * SaveSourceDevice(dto);
         * LoadSourceDevice();
         * DeleteSourceDeviceById(string id);
         * UpdateSourceDevice(dto);
         * RetractSolutions(List<dto>, agentId);
         * DeleteSolutions(List<Dto>, agentId);
         * SaveFilterOption(dto);
         * GetFilterById(string id);
         * DeleteFilterById(string id);
         * GetAgent(string agentId);
         * SaveSolutionCenterPlan(planDto);
         * GetPlanById(string planId);
         * DeletePlanById(string planId);
         */

        /// <summary>
        /// 获取SolutionCenter页面可以使用的farm。
        /// </summary>
        /// <returns></returns>
        List<FarmDto> GetAllFarms();

        /// <summary>
        /// 根据agent id查询solutions
        /// </summary>
        /// <param name="agentId">agent id</param>
        /// <returns></returns>
        List<SolutionDto> GetSolutionsByAgentId(string agentId);

        /// <summary>
        /// 根据farm Id查询solutions
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        List<SolutionDto> GetSolutionsByFarmId(string farmId);

        /// <summary>
        /// 获取SolutionCenter页面可以使用的agent。
        /// </summary>
        /// <returns></returns>
        List<ServiceDto> GetAllAgents();

        /// <summary>
        /// retract solutions
        /// </summary>
        /// <param name="solutionIds"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        Result RetractSolutions(List<string> solutionIds, string agentId);

        /// <summary>
        /// delete solutions
        /// </summary>
        /// <param name="solutionIds"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        Result DeleteSolutions(List<string> solutionIds, string agentId);

        /// <summary>
        /// 获取Agent.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ServiceDto GetAgent(string id);

        /// <summary>
        /// 保存solution center plan。
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>plan id</returns>
        string SaveSolutionCenterPlan(SolutionCenterPlanDto plan);

        /// <summary>
        /// 根据id查询plan.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SolutionCenterPlanDto GetPlanById(string id);

        /// <summary>
        /// 根据id删除plan.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Result DeletePlanById(string id);

        /// <summary>
        /// 根据farm id查询plan.
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        List<SolutionCenterPlanDto> GetPlanByFarm(string farmId);
    }
}
