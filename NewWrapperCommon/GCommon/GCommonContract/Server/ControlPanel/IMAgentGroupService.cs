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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Common;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.AccountManager.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAgentGroupService
    {
        [OperationContract]
        Result UpdateAgentGroup(ServiceGroupDto group);

        [OperationContract]
        Result CreateAgentGroup(ServiceGroupDto group);
        /// <summary>
        /// 通过agentGroupId来获得对应的 Agent
        /// </summary>
        /// <param name="agentGroupId"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceDto GetSingleAgentByAgentGroupId(string agentGroupId);

        /// <summary>
        /// 获得默认的agnet group 通过 farmId
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceGroupDto GetDefaultAgentGroupByFarmId(string farmId);

        /// <summary>
        /// 检查Agent Group是否被占用
        /// </summary>
        /// <param name="ids">agent group ids</param>
        /// <returns>被占用 : true; 否 : false</returns>
        [OperationContract]
        bool IsAgentGroupInUse(List<string> ids);

        /// <summary>
        /// 获取agent所在所有group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupsByAgentId(string id);

        [OperationContract]
        bool NeedToShowWarning(IEnumerable<string> agentIds);

        /// <summary>
        /// 根据dto删除Agent Group
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        Result DeleteAgentGroupByDto(ServiceGroupDto dto);

        #region Agent Group Query
        /// <summary>
        /// 获取所有的Agent group, 方法没有任何限制，Group中的Agent可以是任意状态，包括Uninstall状态和License Deny状态的Agent。
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetAllAgentGroups();

        /// <summary>
        /// 获取所有已安装的SP Agent Group，不包含Uninstall状态和License Deny状态的Agent。
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetInstalledSPAgentGroups();

        [OperationContract]
        List<ServiceGroupDto> GetSPAgentGroupsByFarmIdAndAgentType(string farmId, List<string> agentTypes);

        [OperationContract]
        List<ServiceGroupDto> GetSPAgentGroupsByAgentType(List<string> agentTypes);

        /// <summary>
        /// 获取所有可用的SP Agent Group，只包含AgentActive为Active的而且AgentState为Up的Agent。
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetAvailableSPAgentGroups();

        #endregion

        /// <summary>
        /// 根据ID查询agent group.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceGroupDto GetAgentGroupById(string id);

        /// <summary>
        /// 根据名字查询agent group.
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceGroupDto GetAgentGroupByName(string name);

        /// <summary>
        /// 查询farm下的所有Agent Group.
        /// </summary>
        /// <param name="farmId">farm id</param>
        /// <returns>ServiceGroupDto list</returns>
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupByFarmId(string farmId);

        /// <summary> 根据farmId和groupType查找Agent Group. </summary>
        /// <param name="farmId"></param>
        /// <param name="groupType"></param>
        /// <returns>ServiceGroupDto list</returns>
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupByFarmIdAndType(string farmId, int groupType);

        /// <summary>
        /// 获取所有含有bpos agent的group.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetBPOSAgentGroups();

        /// <summary>
        /// 获取group下所有可用的agent.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceDto> GetAvailableAgentsByGroupId(string groupId);

        /// <summary>
        /// 获取group下所有可用的agent.并使用agent type再次过滤.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="agentTypes"></param>
        /// <returns></returns>
        List<ServiceDto> GetAvailableAgentsByGroupIdAndAgentType(string groupId, List<string> agentTypes);

        List<ServiceDto> GetAvailableBposAgentsByGroupIdAndAgentType(string groupId, List<string> agentTypes);
        /// <summary>
        /// 根据agent中JobControl设置来获取可用Agent
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="agentTypes"></param>
        /// <returns></returns>
        List<ServiceDto> GetUsableAgentsByGroupIdAndAgentType(string groupId, List<string> agentTypes);
        /// <summary>
        /// 根据agent中JobControl设置来获取可用Bpos Agent
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="agentTypes"></param>
        /// <returns></returns>
        List<ServiceDto> GetUsableBposAgentsByGroupIdAndAgentType(string groupId, List<string> agentTypes);
    }
}
