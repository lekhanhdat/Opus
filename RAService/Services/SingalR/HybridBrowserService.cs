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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Multi_Geo;
using CommonModel.DataModel;
using HybirdProxy.Implement;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util.Security;

namespace AvePoint.RA.Service.Services.SignalR
{
    public class HybridBrowserService : RMServiceBase, IHybridBrowserService
    {

        RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly TimeSpan AgentActiveCheckInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan AgentActiveCheckTimeout = TimeSpan.FromMinutes(3);
        private const int MaxConcurrentAgentRedirects = 4;

        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));
        private ISignalRService signalRService => PlatformWindsorManager.GetService<ISignalRService>();

        public IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMemebershipDao => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();

        public IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IAgentMgmtService _agentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        public ICertificateService CertificateService => PlatformWindsorManager.GetService<ICertificateService>();
        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<ValidateResult> ValidateFileSystemUNCPathsAsync(FileSystemUNCPathValidateArgs args, AccessConnectionType accessConnectionType, List<Guid> agentsId)
        {
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            try
            {
                var tenantId = TenantLocalValue.LogonGroupId;

                var agents = await signalRService.GetAgentsByTypeAsync(tenantId, SourceType.FileSystem);
                AgentInformation agent;
                if (accessConnectionType == AccessConnectionType.All)
                {
                    if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
                    {
                        var connectedAgentIds = agents
                            .Select(item => Guid.TryParse(item.AgentId, out var parsedId) ? parsedId : Guid.Empty)
                            .Where(id => id != Guid.Empty)
                            .ToList();

                        var agentsFromDb = await _agentMgmtService.GetAgentsByIdsAsync(connectedAgentIds);
                        var mainDCName = _multiGeoDataCenterService.GetMainDC();
                        var allowedAgentIds = agentsFromDb
                            .Where(a => string.IsNullOrWhiteSpace(a.DCInternalName)
                                || (!string.IsNullOrWhiteSpace(mainDCName)
                                    && string.Equals(a.DCInternalName, mainDCName, StringComparison.OrdinalIgnoreCase)))
                            .Select(a => a.Id)
                            .ToHashSet();

                        agents = agents
                            .Where(item => Guid.TryParse(item.AgentId, out var parsedId) && allowedAgentIds.Contains(parsedId))
                            .ToList();
                    }
                    agent = agents.FirstOrDefault();
                }
                else
                {
                    agent = agents.FirstOrDefault(item => agentsId.Contains(new Guid(item.AgentId)));
                }

                if (agent == null)
                {
                    logger.Warn("No available agent.");
                    throw new NotAvailableAgentException();
                }

                proxy.ConfigureProxy(config =>
                {
                    config.InvokeTimeout = 60;
                });

                var result = await proxy.InvokeOneAgentAysnc<FileSystemUNCPathValidateExecute, FileSystemUNCPathValidateArgs, ValidateResult>(agent, new FileSystemUNCPathValidateExecute() { MethodArgs = args });
                logger.Info($"Finish to send message to agent. {agent.AgentId}");
                return result;

            }
            catch (NotAvailableAgentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<BrowserResult> BrowseTreeNodeAsync(TreeBrowserArgs message)
        {
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            logger.Info("Finish to get proxy.");
            return BrowseTreeNodeAsync(proxy, message);
        }

        public Task<BrowserResult> BrowseTreeNodeByGroupIdAsync(TreeBrowserArgs message, Guid groupId)
        {
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            logger.Info("Finish to get proxy.");
            return BrowseTreeNodeAsync(proxy, message, groupId);
        }

        private async Task<BrowserResult> BrowseTreeNodeAsync(AgentProxy proxy, TreeBrowserArgs message, Guid groupId)
        {
            BrowserResult result = null;
            try
            {
                string tenantId = TenantLocalValue.LogonGroupId;

                //ICollection<AgentInformation> agents = signalRService.GetAgentsByType(tenantId, SourceType.FileSystem);
                ICollection<AgentInformation> agents = await signalRService.GetAgentsByTypeAndConnectionGroupIdAsync(tenantId, SourceType.FileSystem, groupId);

                logger.Info("Available agent count : " + agents.Count);
                if (agents.Count == 0)
                {
                    throw new NotAvailableAgentException();
                }
                proxy.ConfigureProxy(config =>
                {
                    config.InvokeTimeout = 60;
                });

                result = await proxy.InvokeOneAgentAysnc<STreeBrowserExecute, TreeBrowserArgs, BrowserResult>(agents.FirstOrDefault(), new STreeBrowserExecute() { MethodArgs = message });
                logger.Info($"Finish to send message to agent. {agents.FirstOrDefault()?.AgentId}");

            }
            catch (NotAvailableAgentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public async Task<BrowserResult> BrowseTreeNodeAsync(AgentProxy proxy, TreeBrowserArgs message)
        {

            BrowserResult result = null;
            try
            {
                string tenantId = TenantLocalValue.LogonGroupId;

                ICollection<AgentInformation> agents = await signalRService.GetAgentsByTypeAsync(tenantId, SourceType.FileSystem);

                logger.Info("Available agent count : " + agents.Count);
                if (agents.Count == 0)
                {
                    throw new NotAvailableAgentException();
                }
                proxy.ConfigureProxy(config =>
                {
                    config.InvokeTimeout = 60;
                });

                result = await proxy.InvokeOneAgentAysnc<STreeBrowserExecute, TreeBrowserArgs, BrowserResult>(agents.FirstOrDefault(), new STreeBrowserExecute() { MethodArgs = message });
                logger.Info($"Finish to send message to agent. {agents.FirstOrDefault()?.AgentId}");

            }
            catch (NotAvailableAgentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public async Task<bool> CheckHasAvailableAgentAsync(SourceType sourceType)
        {
            string tenantId = TenantLocalValue.LogonGroupId;
            ICollection<AgentInformation> agents = await signalRService.GetAgentsByTypeAsync(tenantId, sourceType);
            return agents.Count != 0;
        }

        public async Task<bool> CheckHasAvailableAgentAsync(SourceType sourceType, Guid groupId)
        {
            string tenantId = TenantLocalValue.LogonGroupId;
            ICollection<AgentInformation> agents = await signalRService.GetAgentsByTypeAndConnectionGroupIdAsync(tenantId, sourceType, groupId);
            return agents.Count != 0;
        }

        public async Task<bool> CheckHasAvailableAgentAsync(SourceType sourceType, List<Guid> agentIds)
        {
            if (agentIds == null || agentIds.Count == 0)
            {
                return false;
            }

            string tenantId = TenantLocalValue.LogonGroupId;
            var agentIdSet = agentIds
                .Where(id => id != Guid.Empty)
                .ToHashSet();

            if (agentIdSet.Count == 0)
            {
                return false;
            }

            var mainDCName = _multiGeoDataCenterService.GetMainDC();

            if (sourceType != SourceType.FileSystem)
            {
                var agents = await signalRService.GetAgentsByTypeAsync(tenantId, sourceType);
                return agents.Any(a => Guid.TryParse(a.AgentId, out var id) && agentIdSet.Contains(id));
            }

            var agentsFromDb = await _agentMgmtService.GetAgentsByIdsAsync(agentIdSet.ToList());
            if (agentsFromDb.Count == 0)
            {
                return false;
            }

            var dcNames = agentsFromDb
                .Select(a => a.DCInternalName)
                .Where(dc => !string.IsNullOrEmpty(dc) && !string.Equals(dc, mainDCName, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var localAgentIds = agentsFromDb
                .Where(a => string.IsNullOrEmpty(a.DCInternalName) || string.Equals(a.DCInternalName, mainDCName, StringComparison.OrdinalIgnoreCase))
                .Select(a => a.Id)
                .ToList();

            if (localAgentIds.Count > 0)
            {
                var localAgents = await signalRService.GetAgentsByTypeAndAgentIdsAsync(tenantId, sourceType, localAgentIds);
                if (localAgents.Any(a => Guid.TryParse(a.AgentId, out var id) && agentIdSet.Contains(id)))
                {
                    return true;
                }
            }

            if (dcNames.Count > 0)
            {
                var dcAgents = await RAMultiGeoClient.RouteApiActionAsync<string, ICollection<AgentInformation>>(
                    MultiGeoOperationType.SignalRGetAgent, tenantId, dcNames);

                if (dcAgents != null)
                {
                    return dcAgents.Values
                        .Where(v => v != null)
                        .SelectMany(v => v)
                        .Any(a => Guid.TryParse(a.AgentId, out var id) && agentIdSet.Contains(id));
                }
            }

            return false;
        }

        public bool ValidateUrl()
        {
            return false;
        }

        public async Task ProcessUpgradeCloudAgent(IEnumerable<Guid> agentIds, string targetVersion)
        {
            try
            {
                var proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
                proxy.ConfigureProxy(cfg => cfg.InvokeTimeout = 60);

                var agents = await signalRService.GetAvailableAgentsAsync(TenantLocalValue.LogonGroupId);

                var agentIdSet = agentIds.ToHashSet();

                var availableAgents = agents
                    .Where(x => Guid.TryParse(x.AgentId, out var id) && agentIdSet.Contains(id))
                    .Select(x => new
                    {
                        Info = x,
                        ParsedId = Guid.Parse(x.AgentId),
                        ParsedStatus = (ServiceStatus)x.ServiceStatus
                    })
                    .ToList();

                if (!availableAgents.Any())
                    throw new NotAvailableAgentException($"No available agent to upgrade to version {targetVersion}.");

                foreach (var agent in availableAgents)
                {
                    _ = SendUpgradeRequestAsync(agent, proxy, targetVersion);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ProcessUpgradeCloudAgent failed.", ex);
                throw;
            }
        }
        private async Task SendUpgradeRequestAsync(dynamic agent, AgentProxy proxy, string targetVersion)
        {
            try
            {
                var args = new RecordsAgentUpgradeArgs
                {
                    AgentInfo = new AgentInfo
                    {
                        AgentId = agent.ParsedId,
                        TenantId = TenantLocalValue.LogonGroupId,
                        Status = agent.ParsedStatus,
                    },
                    TargetVersion = targetVersion
                };

                logger.Info($"Sending upgrade request to agent {agent.ParsedId}...");

                await proxy.InvokeOneAgentAysnc
                <
                    RecordsAgentUpgradeExecute,
                    RecordsAgentUpgradeArgs,
                    RecordsAgentUpgradeResult
                >(agent.Info, new RecordsAgentUpgradeExecute { MethodArgs = args });

                logger.Info($"Upgrade request is sent to {agent.ParsedId}, initial status {agent.ParsedStatus}.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while sending upgrade request. Ex: {ex}");
            }
        }

    }
}
