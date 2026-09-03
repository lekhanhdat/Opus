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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SignalR
{
    public class HybridSharePointOnPremBrowserService : RMServiceBase, IHybridSharePointOnPremBrowserService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(HybridSharePointOnPremBrowserService));

        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));

        private ISignalRService SignalRService => PlatformWindsorManager.GetService<ISignalRService>();

        public async Task<SharePointOnPremBrowserResult> BrowseAsync(SharePointOnPremBrowserArgs args)
        {
            Logger.Info("Begin get proxy.");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy.");
            try
            {
                var tenantId = TenantLocalValue.LogonGroupId;

                var agents = await SignalRService.GetAgentsByTypeAsync(tenantId, Hybrid.Contract.Object.SourceType.SharePoint);
                Logger.Info($"Available agent count: [{agents.Count}]");

                if(agents.Count == 0)
                {
                    throw new NotAvailableAgentException();
                }

                proxy.ConfigureProxy(config =>
                {
                    config.InvokeTimeout = 60;
                });
                var agent = agents.FirstOrDefault();
                Logger.Info($"Begin send message to agent: [{agent?.AgentId}]");
                var result = await proxy.InvokeOneAgentAysnc<SharePointOnPremBrowserExecute, SharePointOnPremBrowserArgs, SharePointOnPremBrowserResult>(agent, new SharePointOnPremBrowserExecute { MethodArgs = args });
                Logger.Info($"End send message to agent: [{agent?.AgentId}]");
                return result;
            }
            catch (NotAvailableAgentException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
