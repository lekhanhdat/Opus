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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Extension;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.MultiGeo;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SignalR
{
    public class HybridAgentService : RMServiceBase, IHybridAgentService
    {

        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IAgentMgmtService agentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public async System.Threading.Tasks.Task ProcessMessageAsync(AgentManagementArgs args)
        {

            TenantLocalValue.LogonGroupId = args.TenantId;
            logger.Info("agent management message received from agent , agent Id : " + args.AgentId + ", tenant id: " + args.TenantId + ", server host: " + args.HostName + ", Type: " + args.Type);


            var dto = new RMAgentDto
            {
                JobCounts = args.JobCounts,
                CPUHZ = args.CPUHZ,
                CPUUsage = args.CPUUSage,
                TotalMemory = args.TotalMemory,
                AvailableMemeory = args.AvailableMemeory,
                ClientId = args.AgentId,
                TenantId = args.TenantId,
                TimeStamp = args.TimeStamp,
                OSName = args.OSName,
                OSVersionNumber = args.OSVersionNumber,
                ServerName = args.HostName,
                Version = args.Version,
                Errors = args.Errors,
                Id = new Guid(args.AgentId),
                IsSupportUpgrade = args.IsSupportUpgrade,
            };

            try
            {
                var configVersionSting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AGENT_LATEST_VERSION];
                PrepareAgentRuntimeStatus(dto, args.Type, configVersionSting, args.TenantId);

                var shouldSyncToMainDc = await RAMultiGeoClient.ShouldPostToMainDcAsync();
                var isUpdated = shouldSyncToMainDc
                    ? await TrySyncAgentRuntimeStatusToMainDcAsync(dto, args.Type)
                    : await UpdateAgentRuntimeStatusAsync(dto, args.Type);

                if (isUpdated)
                {
                    await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                        CreateAgentRuntimeStatusSyncRequest(dto, args.Type),
                        MultiGeoOperationType.UpdateAgentRuntimeStatus);
                }

                logger.Info($"Finish to update agent, host: {dto.ServerName}, agent Id : { args.AgentId } , tenant id: { args.TenantId}, status: {dto.Status}, agent version: {dto.Version}, latest agent version: {configVersionSting}");
            }
            catch (Exception e)
            {
                logger.Error("Update agent fail ,error ", e);
            }
        }

        private void CheckSourceError(RMAgentDto before, RMAgentDto after)
        {
            if(before.SourceType.HasFlag(SourceType.SharePoint))
            {
                CheckSharePointSourceError(before, after);
            }
        }

        private void CheckSharePointSourceError(RMAgentDto before, RMAgentDto after)
        {
            if(string.IsNullOrEmpty(before.FarmId) && !after.Errors.HasFlag(ServiceErrors.SharePoint))
            {
                after.Errors |= ServiceErrors.SharePoint;
            }
        }

        private void CheckMismatch(RMAgentDto dto, string configVersionSting)
        {
            if (!string.IsNullOrEmpty(configVersionSting) && !string.IsNullOrEmpty(dto.Version))
            {
                var configVersion = new Version(configVersionSting);
                var dtoVersion = new Version(dto.Version);
                if (IsMaxVersionUpdate(dtoVersion, configVersion))
                {
                    dto.Status = ServiceStatus.Mismatched;
                }
            }
        }

        private void CheckLicenseError(RMAgentDto agent, SourceType allSourceType, string tenantId)
        {
            //if (!(agent.Status == ServiceStatus.Active || agent.Status == ServiceStatus.ActiveException)) return;
            var tempErrors = ServiceErrors.None;
            var allLicenseExpired = true;
            var hasLicenseExpired = false;
            foreach (var sourceType in allSourceType.Split())
            {
                var sourceLicenseExpired = !HasLiense(tenantId, sourceType);
                allLicenseExpired = allLicenseExpired && sourceLicenseExpired;
                hasLicenseExpired = hasLicenseExpired || sourceLicenseExpired;
                if (sourceLicenseExpired)
                {
                    tempErrors |= sourceType.Map2LicenseServerErrors();
                }
            }

            agent.Status = allLicenseExpired ? ServiceStatus.InActive : hasLicenseExpired ? ServiceStatus.ActiveException : agent.Status;

            if (tempErrors != ServiceErrors.None)
            {
                agent.Errors = tempErrors;
            }
        }

        private void CheckGeneralError(RMAgentDto dto, SourceType allSourceType)
        {
            if (dto.Errors != Hybrid.Contract.Object.ServiceErrors.None)
            {
                var errors = ServiceErrors.None;

                foreach (var sourceType in allSourceType.Split())
                {
                    var error = sourceType.Map2GeneralServerErrors();
                    if (dto.Errors.HasFlag(error))
                    {
                        errors |= error;
                    }
                }
                dto.Errors = errors;
                if (dto.Errors != ServiceErrors.None)
                {
                    dto.Status = Hybrid.Contract.Object.ServiceStatus.ActiveException;
                }
            }
        }

        private bool HasLiense(string tenantId, SourceType sourceType)
        {
            if (sourceType == SourceType.FileSystem)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(tenantId, sourceType.Map2PaidForModule())
                    || TenantService.CheckLicenseWithAdditionalProduct(tenantId, PaidForProduct.OpusFileSystemDiscovery)
                    || TenantService.CheckLicenseWithAdditionalDataSource(tenantId, PreviewFeature.FileSystemDiscovery);
            }

            return TenantService.CheckLicenseWithAdditionalDataSource(tenantId, sourceType.Map2PaidForModule());
        }

        private bool IsMaxVersionUpdate(Version agentDtoVersion, Version configVersion)
        {
            return configVersion.Major > agentDtoVersion.Major;
        }

        private void PrepareAgentRuntimeStatus(RMAgentDto dto, MessageType messageType, string configVersionSting, string tenantId)
        {
            if (messageType == MessageType.Onstop)
            {
                dto.Status = Hybrid.Contract.Object.ServiceStatus.InActive;
                return;
            }

            if (messageType != MessageType.KeepAlive)
            {
                return;
            }

            var agent = agentMgmtService.Get(dto.Id);

            dto.Status = Hybrid.Contract.Object.ServiceStatus.Active;
            CheckMismatch(dto, configVersionSting);

            CheckLicenseError(dto, agent.SourceType, tenantId);

            CheckSourceError(agent, dto);

            if (dto.Status == ServiceStatus.Active)
            {
                CheckGeneralError(dto, agent.SourceType);
            }
        }

        private Task<bool> UpdateAgentRuntimeStatusAsync(RMAgentDto dto, MessageType messageType)
        {
            if (messageType == MessageType.Onstop)
            {
                return agentMgmtService.UpdateStatusAsync(dto.Id, dto.Status);
            }

            if (messageType == MessageType.KeepAlive)
            {
                return agentMgmtService.UpdateAgentResourceUsageAsync(dto);
            }

            return System.Threading.Tasks.Task.FromResult(false);
        }

        private async System.Threading.Tasks.Task<bool> TrySyncAgentRuntimeStatusToMainDcAsync(RMAgentDto dto, MessageType messageType)
        {
            var request = CreateAgentRuntimeStatusSyncRequest(dto, messageType);

            try
            {
                var synced = await RAMultiGeoClient.PostToMainDcAsync<AgentRuntimeStatusSyncRequest, bool>(
                    request,
                    MultiGeoOperationType.UpdateAgentRuntimeStatus);

                if (!synced)
                {
                    logger.Warn($"Sync agent runtime status to main DC returned false, agent id: {dto.Id}, tenant id: {dto.TenantId}, action: {request.Action}.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Warn($"Failed to sync agent runtime status to main DC, agent id: {dto.Id}, tenant id: {dto.TenantId}, error: {e}");
                return false;
            }
        }

        private AgentRuntimeStatusSyncRequest CreateAgentRuntimeStatusSyncRequest(RMAgentDto dto, MessageType messageType)
        {
            return new AgentRuntimeStatusSyncRequest
            {
                Agent = dto,
                Action = messageType == MessageType.Onstop
                    ? AgentRuntimeStatusSyncAction.UpdateStatus
                    : AgentRuntimeStatusSyncAction.UpdateResourceUsage,
            };
        }
    }
}
