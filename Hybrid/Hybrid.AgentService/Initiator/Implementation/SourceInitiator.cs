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
using AvePoint.GCommon;
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Hybrid.Browser.Contract;
using AvePoint.RA.Hybrid.Browser.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.Initiator
{
    public class SourceInitiator : BaseInitiator
    {
        private readonly AveLogger Logger = AveLogger.GetInstance(typeof(SourceInitiator));

        private readonly TimeSpan TimeoutDelay = TimeSpan.FromMinutes(5);

        private readonly string TenantId;

        private readonly string AgentId;

        public override string Name => nameof(SourceInitiator);

        public SourceInitiator()
        {
            try
            {
                TenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                AgentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get agent information. Error: {e}");
            }
        }

        public override void Start()
        {
            try
            {
                if (string.IsNullOrEmpty(TenantId) || string.IsNullOrEmpty(AgentId))
                {
                    Logger.Warn("Can't get agent information.");
                    return;
                }
                SharePointSourceInitialize();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialized agent source. Error: {e}");
            }
        }

        private void SharePointSourceInitialize()
        {
            Logger.Info($"Start initialzation sharepoint on-premise source.");
            using (var cancelToken = new CancellationTokenSource(TimeoutDelay))
            {
                var initialTask = Task.Run(() =>
                {
                    try
                    {
                        var farmId = HybridBrowserUtil.Instance.Browse(HybridBrowserType.SharePointOnPremFarm, "");
                        Logger.Info($"Begin relate farm: [{farmId}] to agent: [{AgentId}]");
                        var agentInfo = new AgentInfo
                        {
                            AgentId = new Guid(AgentId),
                            TenantId = TenantId,
                            SPFarmId = farmId
                        };
                        var res = Task.Run(() => HybridAgentApiClientUtil.Client.AgentMgmtService.UpdateAgentRelateFarmId(agentInfo)).Result;
                        if (!res)
                        {
                            Logger.Warn($"Relate farm: [{farmId}] to agent: [{AgentId}] failed.");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while relate farm to agent: [{AgentId}]. Error: {e}");
                    }
                });

                cancelToken.Token.Register(() =>
                {
                    try
                    {
                        Logger.Warn($"The sharepoint on-premise initialize timeout.");
                        var agentInfo = new AgentInfo
                        {
                            AgentId = new Guid(AgentId),
                            TenantId = TenantId,
                            SPFarmId = ""
                        };
                        var res = Task.Run(() => HybridAgentApiClientUtil.Client.AgentMgmtService.UpdateAgentRelateFarmId(agentInfo)).Result;
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while excute event after sharepoint on-premise initialize timeout. Error: {e}");
                    }
                });

                try
                {
                    initialTask.Wait(cancelToken.Token);
                }
                catch (OperationCanceledException e)
                {
                    Logger.Error($"The initializion sharepoint on-premise task is timeout. Error: {e}");
                }
            }
            Logger.Info($"Successful initialzation sharepoint on-premise source.");
        }
    }
}
