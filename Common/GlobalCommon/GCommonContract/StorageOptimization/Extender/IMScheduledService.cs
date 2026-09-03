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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Extender
{
    /// <summary>
    /// Extender scheduled provider services to agent and gui.
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMScheduledService
    {
        [OperationContract]
        List<Rule> GetExistingRules();

        [OperationContract]
        SOReturnMessage CreateRuleInProfile(Rule rule);

        #region For Extender Profile
        /// <summary>
        /// 在Schedule Rule and setting页面使用，当点击节点之后，需要load出当前Farm下的Profile
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        List<SORuleInfoContract> GetAllByFarm(string farmId);

        /// <summary>
        /// 进入Profile页面，初始化表格中的信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<SORuleInfoContract> GetAllProfile();

        /// <summary>
        /// 在创建Profile时，需要用到一个Farm列表
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IList<FarmDto> GetAllFarm();

        /// <summary>
        /// 创建Profile时使用
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage CreateProfile(SORuleInfoContract profile);

        /// <summary>
        /// 编辑Profile时使用
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage EditProfile(SORuleInfoContract profile);

        /// <summary>
        /// 删除Profile时使用
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage DeleteProfile(List<SORuleInfoContract> profiles);

        [OperationContract]
        SORuleInfoContract GetProfileById(string profileId);

        [OperationContract]
        List<string> GetNodesByProfile(string profileId);

        [OperationContract]
        List<SORuleInfoContract> ValidateProfiles(List<SORuleInfoContract> profiles);

        [OperationContract]
        bool IsProfileNameExist(string name);

        [OperationContract]
        SOReturnMessage IsTimeEarlier(ScheduleDto scheduleDto);

        /// <summary>
        /// 使用这个rule的profiles
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetProfilesByRule(Rule rule);

        #endregion For Extender Profile

        [OperationContract]
        RuleNodeStatus GetRuleNodeStatus(SPTreeNodeDto node);

        /// <summary>
        /// Scheduled在GUI上点击finish时，传选中的node和收集好的rule.
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage CreateRule(List<SPTreeNodeDto> selectedNodes, Rule rule);

        /// <summary>
        /// enable or disable rules
        /// </summary>
        [OperationContract]
        SOReturnMessage EnableRules(List<SPTreeNodeDto> selectedNodes, List<Rule> rules, bool isEnable);
        /// <summary>
        /// Scheduled在GUI点击OK时传选中node和plan信息
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage Apply(List<SPTreeNodeDto> selectedNodes, SORuleInfoContract ruleInfo);

        /// <summary>
        /// 立即Run 一个job
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="ruleInfo"></param>
        [OperationContract]
        string RunNow(List<SPTreeNodeDto> selectedNodes, SORuleInfoContract ruleInfo);

        /// <summary>
        /// 在schedule页面删除rule
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage RemoveRules(List<SPTreeNodeDto> selectedNodes, List<Rule> rules);

        /// <summary>
        /// 继承父node的rule信息
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage InheritRule(List<SPTreeNodeDto> selectedNodes);

        /// <summary>
        /// 打破继承rule，将父node的设置移到子node上，包括rule设置和schedule设置
        /// </summary>
        /// <param name="selectedNodes"></param>
        /// <param name="ruleInfo"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage StopInheritRule(List<SPTreeNodeDto> selectedNodes, SORuleInfoContract ruleInfo);

        /// <summary>
        /// 在GUI上修改的一条rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        SOReturnMessage EditRule(Rule rule);

        /// <summary>
        /// GUI传过来一个rule，判断这个rule还应用过那个node，把node name返回给GUI
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetNodesByRule(Rule rule);

        /// <summary>
        /// 根据页面传过来的node，返回此node对应的scheduled rule信息
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        SORuleInfoContract GetRuleInfo(SPTreeNodeDto node);

        /// <summary>
        /// 点击ribbon时判断是否设置过blob provider
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool ValidateBlobProvider();

        /// <summary>
        /// 只收集Scheduled的Settings，避免使用SOService中的方法加载多余数据
        /// </summary>
        [OperationContract]
        SORulesAndSettings GetRulesAndSettings(SPTreeNodeDto node);

        /// <summary>
        /// For schedule calendar view
        /// </summary>
        /// <param name="schedules"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        #region  FOR EXTENDER DATA UPGRADE
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDevice(string farmId);

        [OperationContract]
        void SaveSettings(SOPlan plan);

        [OperationContract]
        SOReturnMessage ConfigUpgradeSettings(List<ArchiverIndexDeviceTreeNodeDto> tree);

        [OperationContract]
        List<ProfileDto> GetAllNotificationMessage();
        
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupListByFarm(string farmId);
        #endregion

        #region  FOR EBS STUB TO RBS STUB UPGRADE
        [OperationContract]
        SOReturnMessage ValidateEBSRulesAndRunningJobs(FarmDto farmDto);

        [OperationContract]
        SOReturnMessage HandleEBSSettingAndContentDB(FarmDto farmDto);

        [OperationContract]
        void SaveEBSStubUpgradeSetting(SOPlan plan);
        #endregion
    }
}
