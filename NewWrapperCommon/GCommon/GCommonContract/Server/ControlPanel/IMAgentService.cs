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
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.AveLicense;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Server.Common.LogCollector;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAgentService
    {
        [OperationContract]
        RegisterResult Register(ServiceDto agentInfo);

        [OperationContract]
        void AgentControl(AgentQueryDto queryDto, AgentControlOperations control);

        [OperationContract]
        AgentConfigResult AgentMonitorConfig(ServiceDto configInfo, IEnumerable<string> agentType, IEnumerable<string> bposAgentType);

        [OperationContract]
        void AgentConfig(AgentQueryDto queryDto, AgentConfigInfo configInfo);

        [OperationContract]
        ServiceDto GetAgent(AgentQueryDto queryDto);

        [OperationContract]
        IList<ServiceDto> GetAgentForList(AgentQueryDto queryDto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IList<ServiceDto> GetAllAgents();

        [OperationContract]
        void UpdateAgentLoadInfo(ServiceDto agentDto);

        [OperationContract]
        ServiceDto GetAgentById(string ID);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="farmID"></param>
        /// <returns></returns>
        [OperationContract]
        IList<ServiceDto> GetAgentsByFarm(string farmID);

        [OperationContract]
        IList<ServiceDto> GetAgentsByFarmIdAndAgentType(string farmId, string agentType);

        [OperationContract]
        IList<ServiceDto> GetAgentsByStatus(ServiceState[] status, bool include);

        [OperationContract]
        IList<ServiceDto> GetAvailableAgents();

        [OperationContract]
        IList<ServiceDto> GetAvailableAgentsByFarmId(string farmId);

        [OperationContract]
        ServiceDto GetAvailableAgentByFarmId(string farmId);

        [OperationContract]
        IList<ServiceDto> GetAgentsByAgentType(string type);

        [OperationContract]
        IList<ServiceDto> GetActiveAgentsByFarmIDAndAgentType(ServiceState state, ServiceActive active, string farmId, string agentType);

        [OperationContract]
        IList<ServiceDto> GetActiveAgentsByFarmIDAndAgentTypes(ServiceState state, ServiceActive active, string farmId, List<string> agentTypes);

        [OperationContract]
        IList<ServiceDto> GetAvailableAgentsByFarmIdAndAgentType(string farmId, string agentType);

        [OperationContract]
        IList<FarmDto> GetAvailableFarms();

        [OperationContract]
        IList<FarmDto> GetAvailableFarmsByAgentType(string agentType);

        [OperationContract]
        IList<ServiceDto> GetAvailableAgentsByIdArray(string[] agentIds);

        [OperationContract]
        IList<ServiceDto> GetInstalledAgents();

        [OperationContract]
        IList<ServiceDto> GetInstalledAgentsForAPI();

        [OperationContract]
        ServiceDto GetAgentByHost(string host);

        //[OperationContract]
        //FarmDto GetFarmByAgentName(string agentName);

        [OperationContract]
        ServiceDto GetAgentByName(string agentName);

        [OperationContract]
        void UninstallAgent(string agentName, string address);

        [OperationContract]
        bool ValidateAgentPassphrase(byte[] passphraseHash);

        [OperationContract]
        ValidateResult ValidateAgentAddress(ServiceDto agent);

        [OperationContract]
        IList<ServiceDto> GetLatestAvailableAgentsByFarmRole(FarmRoles roles);

        [OperationContract]
        List<string> GetAllLicensedAgentTypes(bool isBpos);

        [OperationContract]
        List<ServiceDto> GetAvailableBposAgents();

        //[OperationContract]
        //FarmDto GetFarmByFarmName(string farmName);

        [OperationContract]
        IList<ServiceDto> GetAllAvailableAgentsByFarmIdAndAgentType(SPTreeNodeDto node, string agentType);

        //[OperationContract]
        //void ExpiredLicense(List<ModuleName> moduleName);

        [OperationContract]
        Dictionary<string, string> GetAllModuleDisplayNames();

        [OperationContract]
        string GetModuleDisplayName(AveModule module);

        [OperationContract]
        string GetModuleDisplayNameByModuleName(string moduleName);

        [OperationContract]
        bool IsMatchControlVersion(ServiceDto agentInfo);

        [OperationContract]
        void SaveLog(PersistentLogDto log);

        [OperationContract]
        void SaveSerivce(ServiceUpdateInfoDto infoDto);

        void SetAvailableModule(ServiceDto dto);
    }
}
