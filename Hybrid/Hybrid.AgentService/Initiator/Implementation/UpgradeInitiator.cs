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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.TransientFault;
using System;
using System.Threading;

namespace AvePoint.Hybrid.AgentService.Initiator
{
    public class UpgradeInitiator : BaseInitiator
    {
        private readonly AveLogger Logger = AveLogger.GetInstance(typeof(UpgradeInitiator));

        private readonly TimeSpan TimeoutDelay = TimeSpan.FromMinutes(5);

        private readonly string TenantId;

        private readonly string AgentId;

        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(10, TimeSpan.FromSeconds(6)));

        public override string Name => nameof(UpgradeInitiator);

        public UpgradeInitiator()
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
                InitializeAgentUpgradeAsync();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialized agent source. Error: {e}");
            }
        }

        private void InitializeAgentUpgradeAsync()
        {
            Logger.Info("Start agent upgrade initialization.");
            AgentManagementArgs args = new AgentManagementArgs
            {
                Type = MessageType.KeepAlive,
                AgentId = AgentId,
                TenantId = TenantId,
                TimeStamp = DateTime.UtcNow.Ticks,
                IsSupportUpgrade = true
            };
            try
            {
                using (var cts = new CancellationTokenSource(TimeoutDelay))
                {
                    var proxy = retryPolicy.ExecuteAction(() => RASignalRProxy.GetManagerProxy());
                    proxy.SendToManagerAsync(new SAgentManagement { MethodArgs = args }).GetAwaiter().GetResult();
                    Logger.Info("Update agent upgrade feature to manager. AgentId={AgentId}, Timestamp={Timestamp}", args.AgentId, args.TimeStamp);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred during agent upgrade initialization. AgentId={AgentId}, Ex: {ex}.");
            }
            Logger.Info("End agent upgrade initialization.");
        }
    }
}
