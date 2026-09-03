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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Common
{
    /// <summary>
    /// 此接口以后将作为StorageOptimization模块Manager与GUI的WCF Service使用.
    /// 处理SO模块与GUI的一些公共逻辑.
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMStorageOptimizationService
    {

        /// <summary>
        /// 在rulemanagement页面remove rule nodes
        /// </summary>
        /// <param name="ruleNodes"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage RemoveRuleNodes(List<RuleNodeContract> ruleNodes);

        /// <summary>
        /// 删除SO模块的rule
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage DeleteRules(List<Rule> rules);

        /// <summary>
        /// 获得基于rule的详细信息
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        [OperationContract]
        List<Rule> GetRuleInfoByRule(ProfileType[] types);

        /// <summary>
        /// 获得与所有node关联的rule的信息
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        [OperationContract]
        List<RuleNodeContract> GetRuleInfoByNode(RuleNodeType[] types);

        [OperationContract]
        SOReturnMessage GetRuleInfo(ProfileType[] profileTypes, RuleNodeType[] ruleTypes);

        /// <summary>
        /// For schedule calendar view
        /// </summary>
        /// <param name="schedules"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        [OperationContract]
        List<SOPlan> GetWrapperPlansByProfileId(List<string> profileIds);

        [OperationContract]
        SOReturnMessage DeleteRulesByIdsForRevIMOnline(List<string> ids);
    }
}
