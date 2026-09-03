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
using AvePoint.Hybrid.AgentService.RecordsCloudAgentUpgrader;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.Hybrid;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.LogCollector
{
    public class AgentLogCollectTask : IDisposable
    {
        private static int AgentLogCollectInterval = AveEnv.AgentLogCollectInterval;
        private readonly AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReadOnlyCollection<IAgentLogCollector> collectors;
        private Thread workerThread;
        private CancellationTokenSource cancellationTokenSource;
        private static readonly TimeSpan ExecutionInterval = TimeSpan.FromMinutes(AgentLogCollectInterval);
        private static HybridApiClient ApiClient { get { return HybridApiClient.Instance; } }

        public AgentLogCollectTask(IEnumerable<IAgentLogCollector> collectors)
        {
            if(collectors == null)
            {
                logger.Warn("No collector log need to upload to azure storage");
                throw new ArgumentNullException(nameof(collectors));
            }
            this.collectors = collectors.ToList().AsReadOnly();
        }

        public void Start()
        {
            if (workerThread != null && workerThread.IsAlive)
            {
                logger.Warn("Agent log collect task is already running.");
                return;
            }
            this.cancellationTokenSource = new CancellationTokenSource();
            workerThread = new Thread(() => Run(cancellationTokenSource.Token))
            {
                IsBackground = true,
                Name = "AgentLogCollectTask"
            };
            workerThread.Start();
        }
    
        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void Run(CancellationToken cancellationToken)
        {
            try
            {
                var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
                var agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
                
                logger.Info("Agent log collect task started.");
                while (!cancellationToken.IsCancellationRequested)
                {
                    var agentInfor = ApiClient.GetAgentInformation(new AgentInfo
                    {
                        AgentId = new Guid(agentId),
                        TenantId = tenantId,
                    });
                    logger.Info($"Agent status is {agentInfor.Status}, Agent collector is {agentInfor.CollectLog}.");
                    if (agentInfor.CollectLog == true && (agentInfor.Status == ServiceStatus.Active || agentInfor.Status == ServiceStatus.ActiveException))
                    {
                        logger.Info($"Agent status is {agentInfor.Status} collect started.");
                        ExecuteCollectors(cancellationToken);
                    }
                    if (cancellationToken.WaitHandle.WaitOne(ExecutionInterval))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Agent log collect task encountered an error. Details: {0}.", ex.ToString());
            }
            finally
            {
                logger.Info("Agent log collect task stopped.");
            }
        }

        private void ExecuteCollectors(CancellationToken cancellationToken)
        {
            foreach (var collector in this.collectors)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    logger.Info($"Start to collect {collector.GetType()} log");
                    collector.CollectAsync(cancellationToken).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error("Log collector '{0}' failed. Details: {1}.", collector.Name, ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
