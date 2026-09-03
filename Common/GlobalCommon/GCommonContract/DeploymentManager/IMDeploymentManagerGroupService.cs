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
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager
{
    /// <summary>
    /// 为GUI提供处理PlanGroup的接口,
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMDeploymentManagerGroupService
    {
        /// <summary>
        /// 单线程运行Queue, 被GUI使用
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string RunGroup(DeploymentManagerPlanGroupDto planGroupDto, RunNowParam param);

        /// <summary>
        /// 运行多个Group
        /// </summary>
        /// <param name="planGroupDtos"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        string RunGroups(List<DeploymentManagerPlanGroupDto> planGroups, RunNowParam param);

        /// <summary>
        /// 多线程运行Queue
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string RunnParallel(DeploymentManagerPlanGroupDto planGroup);

        /// <summary>
        /// 另存Group
        /// </summary>
        /// <param name="planGroupDto"></param>
        /// <param name="plans"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto SaveAsGroup(DeploymentManagerPlanGroupDto planGroupDto, List<AbstractDMPlanDto> modifiedPlans);

        /// <summary>
        /// SaveAndRunNow
        /// </summary>
        /// <param name="planGroupDto"></param>
        /// <param name="modifiedPlans"></param>
        /// <returns>MainJobID</returns>
        [OperationContract]
        string SaveAndRunNow(DeploymentManagerPlanGroupDto planGroupDto, List<AbstractDMPlanDto> modifiedPlans);

        /// <summary>
        /// 创建Group
        /// </summary>
        /// <returns></returns>
        /*[OperationContract]
        DeploymentManagerPlanGroupDto CreateGroup(DeploymentManagerPlanGroupDto planGroupDto);*/

        /// <summary>
        /// 更新Plan Group, 并将PlanOrderInfo排序, 被GUI使用
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto UpdateGroup(DeploymentManagerPlanGroupDto planGroupDto);

        /// <summary>
        /// Edit PlanGroup 并Run
        /// </summary>
        /// <param name="planGroupDto"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto UpdateGroupAndRunNow(DeploymentManagerPlanGroupDto planGroupDto);
        /// <summary>
        /// 修改Group和Plan
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto UpdateGroupAndPlan(DeploymentManagerPlanGroupDto planGroupDto, AbstractDMPlanDto planDto);

        /// <summary>
        /// 从数据库中获得Group的信息, 并将信息返回给GUI
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto LoadGroup(string groupId);

        /// <summary>
        /// 在plan manager中使用, 用来获得全部Plan Group
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IList<DeploymentManagerPlanGroupDto> GetAllGroup();

        /// <summary>
        /// 删除多个Group, 被GUI使用
        /// </summary>
        /// <param name="id"></param>
        [OperationContract]
        DeploymentManagerPlanGroupDto DeleteGroup(string id);

        /// <summary>
        /// 删除多个Group
        /// </summary>
        /// <param name="ids"></param>
        [OperationContract]
        List<DeploymentManagerPlanGroupDto> DeleteGroups(List<string> ids);

        /// <summary>
        /// 是否有正在运行的Plan
        /// </summary>
        /// <param name="ids"></param>
        [OperationContract]
        Dictionary<string, int> HasRunningGroup(List<string> planIds);

        /// <summary>
        /// 创建plan，并将新建的plan添加到PlanGroup中。如果创建的plan时不存在planGroup，则首先创建planGroup
        /// </summary>
        /// <param name="planDto">plan的信息</param>
        /// <returns>返回planGroup的内容，方便GUI刷新页面</returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto CreatePlan(AbstractDMPlanDto planDto);

        /// <summary>
        /// 修改Group当中的plan, 被GUI使用
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        AbstractDMPlanDto UpdatePlan(AbstractDMPlanDto planDto);

        /// <summary>
        /// 根据Plan Id获得plan信息, 被GUI使用
        /// </summary>
        /// <param name="planInfo">planInfo中的ID属性不能为空</param>
        /// <returns></returns>
        [OperationContract]
        AbstractDMPlanDto GetPlanById(PlanOrderInfo planInfo);

        /// <summary>
        /// 删除Plan信息
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto DeletePlan(AbstractDMPlanDto planDto);

        /// <summary>
        /// 删除多个Plan信息
        /// </summary>
        /// <param name="plans"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto DeletePlans(List<AbstractDMPlanDto> plans);

        /// <summary>
        /// 根据PlanGroup类型获得该类型下的所有PlanGroup
        /// </summary>
        /// <param name="groupType"></param>
        /// <returns></returns>
        [OperationContract]
        IList<DeploymentManagerPlanGroupDto> GetPlanGroupByType(PlanGroupType groupType);

        /// <summary>
        /// 根据PlanGroup获得所有SubJob信息
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto GetDeploymentManagerJobInfo(string groupId);

        /// <summary>
        /// 修改Queue的可用状态. 只有enable和disabled两种状态
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="planIds"></param>
        /// <param name="planState"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto UpdatePlanInfoState(string groupId, List<string> planIds, PlanState planState);

        /// <summary>
        /// 获取RunnablePlan
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto GetRunnablePlan(string planId, long startTime);

        /// <summary>
        /// Stsadm获取所有Farm信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IList<FarmDto> GetAllFarm();

        /// <summary>
        ///  Stsadm获取所有Agent信息
        /// </summary>
        /// <param name="farmID"></param>
        /// <returns></returns>
        [OperationContract]
        IList<ServiceDto> GetAllAgentDto(string farmID);

        /// <summary>
        /// 获取所有Solution Compare时候WebApp的信息
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        List<SPTreeNodeDto> GetAllWebAppNodes(SPTreeNodeDto node);

        [OperationContract]
        List<PlanGroupDtoForOtherModule> LoadExistedPlatformPlanGroups();

        [OperationContract]
        DeploymentManagerPlanGroupDto SaveAsPlanAndQueue(DeploymentManagerPlanGroupDto planGroupDto, List<AbstractDMPlanDto> modifiedPlans);

        [OperationContract]
        int CheckPlanNeedShareSiteCollections(DeploymentManagerPlanGroupDto planGroup);

        [OperationContract]
        void UpdateSharedPlanPermission(DeploymentManagerPlanGroupDto planGroup);


        #region add by hang for gui
        /// <summary>
        /// 创建Compare的plan，并将新建的plan添加到PlanGroup中。如果创建的plan时不存在planGroup，则首先创建planGroup
        /// </summary>
        /// <param name="planDto">plan的信息</param>
        /// <returns>返回planGroup的内容，方便GUI刷新页面</returns>
        [OperationContract]
        DeploymentManagerPlanGroupDto CreateComparePlans(List<AbstractDMPlanDto> planDtos);

        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        [OperationContract]
        DMScheduleValidationResult ValidateSchedule(ScheduleDto schedule);
        #endregion

        #region RollBack
        [OperationContract]
        string Rollback(List<BaseJobDto> jobDtos);
        #endregion

        //SAAS-9024
        [OperationContract]
        void CompareNow(CompareNowPlanDto planDto);

        DeploymentManagerPlanGroupDto CreateOrUpdate(DeploymentManagerPlanExecuteParameter parameter);
    }
}