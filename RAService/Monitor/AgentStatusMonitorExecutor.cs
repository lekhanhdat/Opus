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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Monitor
{
    public class AgentStatusMonitorExecutor : IMonitorExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(AgentStatusMonitorExecutor));
        public IAgentMgmtService AgentMgmtService => (IAgentMgmtService)PlatformWindsorManager.GetService(typeof(IAgentMgmtService));
        public async System.Threading.Tasks.Task ExecutorAsync(MonitorBase monitor)
        {
            try
            {
                mLogger.Info("begin to monitor agent status:{0}", TenantLocalValue.LogonGroupId);
                var issuseAgents = (await AgentMgmtService.GetAllAsync()).Where(s => s.Status == Hybrid.Contract.Object.ServiceStatus.ActiveException
                || s.Status == Hybrid.Contract.Object.ServiceStatus.Disabled || s.Status == Hybrid.Contract.Object.ServiceStatus.Mismatched
                || s.Status == Hybrid.Contract.Object.ServiceStatus.InActive);
                if (issuseAgents.Count() == 0)
                {
                    mLogger.Info($"All Agent Status is active or agent not added {TenantLocalValue.LogonGroupId}");
                }
                else
                {
                    foreach (var agent in issuseAgents)
                    {
                        mLogger.Warn($"Monitor Agent Status Tenant ID: {TenantLocalValue.LogonGroupId} issue agents {agent.Name} : {agent.Status}");
                        TelemetryContext.SendToQueue(TelemetryModule.AgentManagement, TelemetryEventType.MonitorAgentStatus, new List<object> { agent.Id, agent.Status });
                    }

                    await TelemetryContext.FlushAsync();
                }

                mLogger.Info("finish to monitor agent status:{0}", TenantLocalValue.LogonGroupId);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while get agent status,ERROR:{0}", ex.ToString());
            }
        }
    }
}
