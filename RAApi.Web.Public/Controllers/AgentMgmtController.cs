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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/AgentMgmt/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class AgentMgmtController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(AgentMgmtController));

        private IKeyValueService _KeyValueService;

        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService(ref _KeyValueService);

        private IAgentMgmtService _AgentMgmtService;

        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService(ref _AgentMgmtService);

        [HttpPost]
        public bool Validate([FromBody] AgentConfigurtion configuration)
        {
            try
            {
                var value = KeyValueService.Get(configuration.PackageId, RA.Contract.Object.RMNameValueType.AppAgentInstallation);
                return value == null;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to validate agent, agent Id {configuration.Id}, package id : {configuration.PackageId}, error : {e.ToString()}");
                return false;
            }
            
        }

        [HttpPost]
        public async Task<bool> Install([FromBody] AgentConfigurtion configuration)
        {
            try
            {
                var dto = new RMNameValueDto()
                {
                    Name = configuration.PackageId,
                    Value = configuration.Id,
                    Type = RMNameValueType.AppAgentInstallation
                };
                return await KeyValueService.SaveAsync(dto);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to update agent installation config, agent Id {configuration.Id}, package id : {configuration.PackageId}, error : {e.ToString()}");
                return false;
            }
        }

        [HttpPost]
        public Task<bool> UpdateAgentRelateFarmId([FromBody] AgentInfo agentInfo)
        {
            return TenantUtil.RunUnderTenantAsync<bool>(agentInfo.TenantId, null, async () =>
            {
                try
                {
                    await AgentMgmtService.UpdateAgentRelateFarmIdAsync(agentInfo.AgentId, agentInfo.SPFarmId);
                    return true;
                }
                catch(Exception e)
                {
                    logger.Error($"An error occur while update agent relate farm id. Error: {e}");
                    return false;
                }
            });
        }

        [HttpPost]
        public Task<ServiceStatus> GetAgentStatus([FromBody] AgentInfo agentInfo)
        {
            return TenantUtil.RunUnderTenantAsync<ServiceStatus>(agentInfo.TenantId, null, async () =>
            {
                try
                {
                    var dto = AgentMgmtService.Get(agentInfo.AgentId);
                    return dto.Status;
                }
                catch (Exception e)
                {
                    logger.Error($"An error occur while get agent status. Error: {e}");
                    return ServiceStatus.InActive;
                }
            });
        }

        [HttpPost]
        public Task<RMAgentDto> GetAgentInfor([FromBody] AgentInfo agentInfo)
        {
            return TenantUtil.RunUnderTenantAsync<RMAgentDto>(agentInfo.TenantId, null, async () =>
            {
                try
                {
                    var dto = AgentMgmtService.Get(agentInfo.AgentId);
                    return dto;
                }
                catch (Exception e)
                {
                    logger.Error($"An error occur while get agent information. Error: {e}");
                    return null;
                }
            });
        }

        [HttpPost]
        public Task<bool> UpdateAgentStatus([FromBody] AgentInfo agentInfo)
        {
            return TenantUtil.RunUnderTenantAsync<bool>(agentInfo.TenantId, null, async () =>
            {
                try
                {
                    if (await RAMultiGeoClient.ShouldPostToMainDcAsync())
                    {
                        var synced = await RAMultiGeoClient.PostToMainDcAsync<AgentInfo, bool>(
                            agentInfo,
                            MultiGeoOperationType.SyncAgentStatusAfterUpgrade);

                        if (!synced)
                        {
                            logger.Warn($"Failed to sync upgraded agent status to main DC. AgentId: {agentInfo.AgentId}, TenantId: {agentInfo.TenantId}, Status: {agentInfo.Status}");
                            return false;
                        }
                    }

                    return await AgentMgmtService.UpdateStatusAsync(agentInfo.AgentId, agentInfo.Status);
                }
                catch (Exception e)
                {
                    logger.Error($"An error occur while update agent status to upgrading. Error: {e}");
                    return false;
                }
            });
        }

    }
}
