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
using AvePoint.Hybrid.Contract.SignalR;
using HybirdProxy.EndpointHandler;
using System;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.SignalRHandler
{
    internal class RecordsAgentUpgradeHandler : EndpointHandlerBase<RecordsAgentUpgradeExecute, RecordsAgentUpgradeArgs, RecordsAgentUpgradeResult>
    {
        private readonly static AveLogger logger = new AveLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override RecordsAgentUpgradeResult Process(RecordsAgentUpgradeArgs param)
        {
            RecordsAgentUpgradeResult res = new RecordsAgentUpgradeResult() { Result = RMAgentUpgradeResult.Succeed };
            try
            {
                logger.Info($"Start RecordsAgentUpgrade process for AgentId: {param.AgentInfo.AgentId}");
                Task.Run(async () =>
                {
                    try
                    {
#if DEBUG
                        logger.Info("RecordsAgentUpgradeHandler running in DEBUG mode.");
                        var upgrader = new RecordsAgentUpgrader(param.AgentInfo, param.TargetVersion, true);
#else
                        var upgrader = new RecordsAgentUpgrader(param.AgentInfo, param.TargetVersion);
#endif
                        await upgrader.ProcessUpgradeCloudAgentAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        throw;
                    }
                });
                return res;
            }
            catch (Exception ex)
            {
                logger.Error($"RecordsAgentUpgrade process failed for AgentId: {param.AgentInfo.AgentId}, Error: {ex}");
                res.Result = RMAgentUpgradeResult.Failed;
                res.AgentId = param.AgentInfo.AgentId;
                res.Message = ex.Message;
                return res;
            }
        }
    }
}
