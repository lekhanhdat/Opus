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
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.HybridLogger;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility;
using CommonModel.DataModel;
using CommonModel.MethodInfo;
using HybirdProxy.Implement;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SignalR
{
    public class SignalRService : RMServiceBase, ISignalRService
    {

        private RALogger logger = RALogger.GetInstance(typeof(SignalRService));
        private IAgentMgmtService agentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private IMultiGeoSettingService multiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        public void SignalRSetup()
        {
            try
            {
                if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    logger.Info("GCP environment skip to setup SignalR.");
                    return;
                }

                string hubServerUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SIGNALR_SERVER_URL];
                string agentId = RMGlobalConfiguration.EnvSetting.RoleId;
                string agentAuth = "None";
                string IdentityServerClientId = RMGlobalConfiguration.AppConfig[RMAppSettingKey.CLIENT_ID_IN_IDENTITY_SERVICE];
                string IdentityServerAddress = RMGlobalConfiguration.AppConfig[RMAppSettingKey.IDENTITY_SERVICE_URL];
                 
                //X509Certificate2 cert = RMCertificateHelper.GetX509Certificate2(RMCertNames.AvePointRecords);

                logger.Info($"[{agentId}] register to SignalR server. Hub server: [{hubServerUrl}], Identity server address : [{IdentityServerAddress}].");

                if(string.IsNullOrEmpty(hubServerUrl))
                {
                    logger.Warn("SignalR server not configured, skip to initliaze.");
                    return;
                }

                var proxy = RASignalRAgentProxy.GetAgentProxy(hubServerUrl, agentId, agentAuth, APIScope.Manager, IdentityServerClientId, IdentityServerAddress, () => RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords), null,
                    LoggerFactory.Create(builder => { builder.AddProvider(HybridLogger.loggerProvider); }));

                proxy.ConfigureProxy(config => {
                    config.RetryInterval = 5000;
                    config.Retrytime = 3;
                });

                logger.Info("Finish to get agent proxy.");

                proxy.AgentConnectionStateChange += Proxy_AgentConnectionStateChange;

                MethodTable.Registered(MethodMapping.MT);
                proxy.RegisterEndpoint(hub =>
                {

                    hub.On<SAgentManagement>(Hybrid.Contract.MethodMapping.MT[typeof(SAgentManagement)], (agentManagement) =>
                    {
                        logger.Info("Receive agent management message : " + agentManagement.MethodArgs.AgentId);
                        
                        System.Threading.Tasks.Task.Run(() => 
                        {
                            TenantLocalValue.LogonGroupId = agentManagement.MethodArgs.TenantId;
                            TenantLocalValue.LogonGroupEmail = string.Empty;
                            TenantLocalValue.LogonUserEmail = string.Empty;
                            IHybridAgentService agentService = (IHybridAgentService)PlatformWindsorManager.GetService("AvePoint.RA.Contract.RMWeb.SignalR.IHybridAgentService", typeof(IHybridAgentService));
                            agentService.ProcessMessageAsync(agentManagement.MethodArgs).Wait();
                        });
                    });

                });

                logger.Info("begin to setup connection to SignalR server.");

                bool result = proxy.EnsureConnect();

                RASignalRAgentProxy.SignalRConnected(result);

                logger.Info("Finish to setup connection to SignalR server.");

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        private void Proxy_AgentConnectionStateChange(object sender, EventArgs e)
        {
            logger.Info("agent connection state change.");
            //do something 
            ((AgentProxy)sender).GetAllAgentsForce();
        }


        public async Task<ICollection<AgentInformation>> GetAgentsAsync(string tenantId)
        {

            logger.Info("Begin to get agent.");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);

            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info("Agents registed in signalr server : " + agents.Count);

            IList<RMAgentDto> allAgents = (await agentMgmtService.GetAllAsync())
                .Where(o => o.Status == ServiceStatus.Active || o.Status == ServiceStatus.ActiveException)
                .OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info("Active/ActiveException agents registed in  manager  : " + allAgents.Count);

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.ToLower().Equals(agent.Id.ToString().ToLower()))
                        .FirstOrDefault();
                    if (agentInfo != null)
                    {
                        logger.Info(string.Format("Agent {0} {1} is active and connected and could to be used.", agentInfo.TenantId, agentInfo.AgentId));
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get agent fail, ", e);
            }

            if (agentsList.Count == 0)
            {
                logger.Warn("No avaiable agents");

            }

            return agentsList;

        }

        public async Task<ICollection<AgentInformation>> GetAgentsByTypeAndConnectionGroupIdAsync(string tenantId, SourceType sourceType, Guid connectionGroupId)
        {
            logger.Info($"Begin to get agent tenantId: [{tenantId}], connection group id: [{connectionGroupId}].");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);

            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info("Agents registed in signalr server : " + agents.Count);

            var allAgents = (await agentMgmtService.GetAvailableAgentsBySourceTypeAndConnectionGroupIdAsync(tenantId, sourceType, connectionGroupId))
                .OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info($"Active agents registed in manager and under connection group: [{connectionGroupId}] count: [{allAgents.Count}].");

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.ToLower().Equals(agent.Id.ToString().ToLower()))
                        .FirstOrDefault();
                    if (agentInfo != null)
                    {
                        logger.Info(string.Format("Agent {0} {1} is active and connected and could to be used.", agentInfo.TenantId, agentInfo.AgentId));
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get agent fail, ", e);
            }

            if (agentsList.Count == 0)
            {
                logger.Warn("No avaiable agents");

            }

            return agentsList;
        }

        public async Task<ICollection<AgentInformation>> GetAgentsByTypeAsync(string tenantId, SourceType type)
        {

            logger.Info("Begin to get agent.");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);

            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info("Agents registed in signalr server : " + agents.Count);

            IList<RMAgentDto> allAgents = (await agentMgmtService.GetAvailableAgentsBySourceTypeAsync(tenantId, type))
                .OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info("Active agents registed in  manager  : " + allAgents.Count);

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.ToLower().Equals(agent.Id.ToString().ToLower()))
                        .FirstOrDefault();
                    if (agentInfo != null)
                    {
                        logger.Info(string.Format("Agent {0} {1} is active and connected and could to be used.", agentInfo.TenantId, agentInfo.AgentId));
                        agentInfo.CertificateId = agent.CertificateId;
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get agent fail, ", e);
            }

            if (agentsList.Count == 0)
            {
                logger.Warn("No avaiable agents");

            }

            return agentsList;

        }

        public async Task<ICollection<AgentInformation>> GetAgentsByFarmIdAsync(string tenantId, string farmId)
        {
            logger.Info("Begin to get agent.");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);

            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info($"Farm: [{farmId}] agents registed in signalr server: [{agents.Count}].");

            var allAgents = (await agentMgmtService.GetAvailableAgentsBySourceTypeAsync(tenantId, SourceType.SharePoint)).Where(item => item.FarmId == farmId).OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info($"Farm: [{farmId}] active agents registed in  manager: [{allAgents.Count}].");

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.ToLower().Equals(agent.Id.ToString().ToLower()))
                        .FirstOrDefault();
                    if (agentInfo != null)
                    {
                        logger.Info($"Farm: [{farmId}] Agent {agentInfo.TenantId} {agentInfo.AgentId} is active and connected and could to be used.");
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Get farm: [{farmId}] agent fail. Error: {e}");
            }

            if (agentsList.Count == 0)
            {
                logger.Warn($"Farm: [{farmId}] No avaiable agents");
            }

            return agentsList;
        }

        public async Task<ICollection<AgentInformation>> GetAvailableAgentsAsync(string tenantId)
        {
            logger.Info("Begin to get agent.");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);

            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info("Agents registed in signalr server : " + agents.Count);

            IList<RMAgentDto> allAgents = (await agentMgmtService.GetAvailableAgentsAsync(tenantId))
                .OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info("Active agents registed in  manager  : " + allAgents.Count);

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.Equals(agent.Id.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (agentInfo != null)
                    {
                        agentInfo.ServiceStatus = (int)agent.Status;
                        logger.Info(string.Format("Agent {0} {1} is active and connected and could to be used.", agentInfo.TenantId, agentInfo.AgentId));
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get agent fail, ", e);
            }

            if (agentsList.Count == 0)
            {
                logger.Warn("No avaiable agents");

            }

            return agentsList;
        }

        private ICollection<AgentInformation> GetAgentsWithRefresh(string tenantId)
        {
            var proxy = RASignalRAgentProxy.GetProxy();

            try
            {
                return proxy.GetAgents(tenantId);
            }
            catch (ArgumentOutOfRangeException ex) when (ShouldForceRefreshTenantAgents(ex, tenantId))
            {
                logger.Warn($"Tenant [{tenantId}] was not found in cached SignalR agent info and Multi-Geo is enabled. Force refresh agent list.");
                return proxy.GetAgentsForce(tenantId);
            }
        }

        private bool ShouldForceRefreshTenantAgents(ArgumentOutOfRangeException ex, string tenantId)
        {
            return ex.ParamName != null
                && ex.ParamName.Equals($"unknown tenantId:{tenantId}", StringComparison.OrdinalIgnoreCase)
                && multiGeoSettingService.IsEnableMultiGeoFeature().GetAwaiter().GetResult();
        }

        public async Task<ICollection<AgentInformation>> GetAgentsByTypeAndAgentIdsAsync(string tenantId, SourceType sourceType, List<Guid> agentIds)
        {
            logger.Info($"Begin to get agents by type and agent IDs. TenantId: [{tenantId}], agent count: [{agentIds.Count}].");
            ICollection<AgentInformation> agents = GetAgentsWithRefresh(tenantId);
            agents = agents.Where(a => a.AgentId != null && agentIds.Contains(Guid.Parse(a.AgentId))).ToList();
            List<AgentInformation> agentsList = new List<AgentInformation>();

            logger.Info("Agents registered in signalr server: " + agents.Count);

            IList<RMAgentDto> allAgents = (await agentMgmtService.GetAvailableAgentsBySourceTypeAsync(tenantId, sourceType))
                .Where(o => agentIds.Contains(o.Id))
                .OrderBy(o => o.CPUUsage).ThenByDescending(o => o.AvailableMemeory).ToList();

            logger.Info($"Active agents matching specified IDs: [{allAgents.Count}].");

            try
            {
                foreach (var agent in allAgents)
                {
                    var agentInfo = agents.Where(o => o.Status == ConnectionStatus.Connected && o.AgentId.Equals(agent.Id.ToString(), StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                    if (agentInfo != null)
                    {
                        logger.Info($"Agent {agentInfo.TenantId} {agentInfo.AgentId} is active and connected and could be used.");
                        agentsList.Add(agentInfo);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Get agent fail, ", e);
            }

            if (agentsList.Count == 0)
            {
                logger.Warn("No available agents");
            }

            return agentsList;
        }
    }
}
