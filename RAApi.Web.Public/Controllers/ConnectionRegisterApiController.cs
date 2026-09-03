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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.RAExchange.Discover;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/ConnectionManagement/[action]")]
    public class ConnectionManagementApiController : RAWebApiBase
    {
        private static readonly Regex UncPathRegex = new Regex(
            @"^\\\\[^\\/:*?""<>|]+\\[^\\/:*?""<>|]+(\\[^\\/:*?""<>|]+)*$",
            RegexOptions.Compiled);
        private static readonly Regex InternalIdRegex = new Regex(
                @"^[A-Za-z0-9_. -]{1,256}$",
            RegexOptions.Compiled);

        private IRMFileSystemRegisterService _FSRegisterService;
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService(ref _FSRegisterService);
        private IRMFileSystemBrowserService _RMFileSystemBrowserService;
        private IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService(ref _RMFileSystemBrowserService);
        private IAgentMgmtService _AgentMgmtService;
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService(ref _AgentMgmtService);
        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        private IAccountWrapperService _AccountWrapperService;
        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService(ref _AccountWrapperService);
        private RALogger logger = RALogger.GetInstance(typeof(ConnectionManagementApiController));
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private IRMFileSystemSettingsService FileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();


        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<List<ConnectionGroupDto>> GetConnectionGroups()
        {
            var result = await FSRegisterService.GetAllGroupsAsync();
            return result;
        }

        [HttpPost]
        [MultiGeoValidIPFilter]
        public async Task<ConnectionResultData> GetPagedConnections([FromBody] GetConnectionListParam param)  //PageIndex>=1
        {
            return await FSRegisterService.QueryConnectionByPagerAsync(param);
        }

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<List<ConnectionDto>> GetUngroupedConnections()
        {
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature()
                && !string.Equals(RMSSOHelper.CurrentDCName, MultiGeoDataCenterService.GetMainDC(), StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("GetUngroupedConnections is only allowed on Main DC when Multi-Geo is enabled.");
            }

            return await FSRegisterService.LoadAllConnectionAsync(true);
        }

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<ConnectionGroupDto> GetConnectionGroupById(Guid id)
        {
            var group = await FSRegisterService.GetGroupAsync(id);
            return await CanAccessConnectionGroupAsync(group) ? group : null;
        }

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<ConnectionDto> GetConnectionById(Guid id)
        {
            var connection = await FSRegisterService.GetConnectionByIdAsync(id);
            return await CanAccessConnectionAsync(connection) ? connection : null;
        }

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<List<ConnectionDto>> GetConnectionsByGroup(Guid id)
        {
            var group = await FSRegisterService.GetGroupAsync(id);
            if (!await CanAccessConnectionGroupAsync(group))
            {
                return new List<ConnectionDto>();
            }

            return await FSRegisterService.GetAllConnectionsByGroupIdAsync(id);
        }

        [HttpPost]
        public async Task<RAReturnMessage> CreateConnectionGroup([FromBody] ConnectionGroupPublic connectionGroupDto)
        {
            if(connectionGroupDto.Id != Guid.Empty)
            {
                return CreateFailedResult("Invalid connection group payload for creation.");
            }
            var connectionGroup = BuildConnectionGroupDto(connectionGroupDto, isCreate: true);
            return await RouteMultiGeoApiActionAsync(connectionGroup, MultiGeoOperationType.SaveConnectionGroup,
                async request =>
                {
                    return await CreateConnectionGroupInternalAsync(request);
                },
                respone => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
        }

        [HttpPost]
        [MultiGeoValidIPFilter]
        public async Task<List<RAReturnMessage>> CreateConnectionGroups([FromBody] List<ConnectionGroupPublic> connectionGroupDtos)
        {
            try
            {

                if (connectionGroupDtos == null || connectionGroupDtos.Count == 0)
                {
                    return new List<RAReturnMessage> { CreateFailedResult("Invalid connection group payload for creation.") };
                }

                var results = new List<RAReturnMessage>(connectionGroupDtos.Count);
                var connectionGroups = connectionGroupDtos.Where(c => c != null).ConvertAll(connectionGroup => BuildConnectionGroupDto(connectionGroup, isCreate: true));

                foreach (var connectionGroup in connectionGroups)
                {
                    results.Add(await CreateConnectionGroupInternalAsync(connectionGroup));
                }

                await TrySyncCommonDataAfterBatchCreateAsync(results, MultiGeoOperationType.SaveConnectionGroups);

                return results;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create connection groups", ex);
            }
            return new List<RAReturnMessage> { CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")) };
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateConnectionGroup([FromBody] ConnectionGroupPublic connectionGroupDto)
        {
            if (connectionGroupDto == null || connectionGroupDto.Id == Guid.Empty)
            {
                return CreateFailedResult("Invalid connection group payload for update,the Id must be filled in the params");
            }

            if (string.IsNullOrEmpty(connectionGroupDto.Name))
            {
                return CreateFailedResult("Invalid connection group payload for update, the Name must be filled in the params");
            }
            var connectionGroup = BuildConnectionGroupDto(connectionGroupDto);
            var multiGeoValidationResult = await ValidateAssignedAgentsByDCAsync(connectionGroup, connectionGroup.Agents.Select(x=>x.Id).ToList());
            if (multiGeoValidationResult != null)
            {
                return multiGeoValidationResult;
            }
            var validationResult = await ValidateConnectionGroupAsync(connectionGroup, isCreate: false);
            if (validationResult != null)
            {
                return validationResult;
            }
            return await RouteMultiGeoApiActionAsync(connectionGroup, MultiGeoOperationType.SaveConnectionGroup,
                async request =>
                {
                    return await SaveConnectionGroupAsync(request, isCreate: false);
                },
                respone => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
        }

        [HttpPost]
        public async Task<RAReturnMessage> CreateConnection([FromBody] ConnectionDto connectionDto)
        {
            return await RouteMultiGeoApiActionAsync(connectionDto, MultiGeoOperationType.SaveConnection,
                async request =>
                {
                    return await CreateConnectionInternalAsync(request);
                },
                respone => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
        }

        [HttpPost]
        [MultiGeoValidIPFilter]
        public async Task<List<RAReturnMessage>> CreateConnections([FromBody] List<ConnectionDto> connectionDtos)
        {
            if (connectionDtos == null || connectionDtos.Count == 0)
            {
                return new List<RAReturnMessage> { CreateFailedResult("Invalid connection payload for creation.") };
            }

            var results = new List<RAReturnMessage>(connectionDtos.Count);
            foreach (var connectionDto in connectionDtos)
            {
                results.Add(await CreateConnectionInternalAsync(connectionDto));
            }

            await TrySyncCommonDataAfterBatchCreateAsync(results, MultiGeoOperationType.SaveConnections);

            return results;
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateConnection([FromBody] ConnectionDto connectionDto)
        {
            var validationResult = ValidateConnection(connectionDto, isCreate: false);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await RouteMultiGeoApiActionAsync(connectionDto, MultiGeoOperationType.SaveConnection,
                async request =>
                {
                    return await SaveConnectionAsync(request, isCreate: false);
                },
                respone => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
        }

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<List<AgentInformationDtoForPublicApi>> GetAgents()
        {
            var allAgents = await AgentMgmtService.GetAllAsync();
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var currentDC = RMSSOHelper.CurrentDCName;
                var mainDC = MultiGeoDataCenterService.GetMainDC();

                if (!string.Equals(currentDC, mainDC, StringComparison.OrdinalIgnoreCase))
                {
                    allAgents = [.. allAgents.Where(agent => string.Equals(agent.DCInternalName, currentDC, StringComparison.OrdinalIgnoreCase))];
                }
            }

            List<AgentInformationDtoForPublicApi> agentInfos = new List<AgentInformationDtoForPublicApi>();
            foreach (var agent in allAgents)
            {
                agentInfos.Add(new AgentInformationDtoForPublicApi() {
                    AgentId = agent.Id.ToString(),
                    AgentName = agent.Name,
                });
            }
            return agentInfos;
        }

        [HttpPost]
        public async Task<int> DeleteConnections([FromBody] List<Guid> connectionIds)
        {
            return await RouteMultiGeoApiActionAsync(connectionIds, MultiGeoOperationType.DeleteConnection,
                async request =>
                {
                    return await FSRegisterService.DeleteConnectoinAsync(request);
                },
                respone => -1);
        }

        [HttpPost]
        public async Task<int> DeleteConnectionGroups([FromBody] List<Guid> groupsIds)
        {
            return await RouteMultiGeoApiActionAsync(groupsIds, MultiGeoOperationType.DeleteGroup,
                async request =>
                {
                    return await FSRegisterService.DeleteGroupConnectoinAsync(request);
                },
                respone => -1);
        }

        [HttpPost]
        [MultiGeoValidIPFilter]
        public async Task<RAReturnMessage> DisableRecordManagement([FromBody] FSJobNodeParam param)
        {
            RAReturnMessage result = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            if(param == null || param.NodeId == Guid.Empty)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = $"Invalid node Id for disabling record management, the Id must be filled in the params";
                return result;
            }
            if (string.IsNullOrEmpty(param.FullPath) || param.ConnectionGroupId == Guid.Empty || param.Level == 0)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = $"Invalid parameters for disabling record management, the FullPath, ConnectionGroupId, and Level must be filled in the params";
                return result;
            }
            var vaildate = TriggerJobServices.IsNodeEligible(param);
            if (vaildate != null)
            {
                return result;
            }
            try
            {
                var fsNode = new RMFSTreeNode
                {
                    Id = param.NodeId,
                    ConnGroupId = param.ConnectionGroupId,
                    Level = param.Level,
                    FullPath = param.FullPath
                };
                var fsNodeSeting = await TriggerJobServices.BuildTreeNodeAsync(fsNode);
                fsNodeSeting.EnableRecordManagement = (int)RMFSTreeNode.EnableRecordManagementSetting.Disable;
                result = await FileSystemSettingsService.SaveFSGeneralSetting4JPMC(fsNodeSeting);
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = $"An error occurred while disabling record management";
            }
            return result;
        }


        private async Task<List<Guid>> ValidateConnections(ValidateConnectionParam param)
        {
            return await RMFileSystemBrowserService.ValidateTestConnectionsAsync(param);
        }

        private async Task TrySyncCommonDataAfterBatchCreateAsync(List<RAReturnMessage> results, MultiGeoOperationType operationType)
        {
            if (results == null || !results.Any(item => item?.MessageType == RAMessageType.Successful))
            {
                return;
            }

            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return;
            }

            if (!string.Equals(RMSSOHelper.CurrentDCName, MultiGeoDataCenterService.GetMainDC(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            MultiGeoReplicaFailureLogWriter.WriteForJob(TenantLocalValue.LogonGroupId, operationType.ToString());
            await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(JobRunBy.Control);
        }

        private async Task<RAReturnMessage> CreateConnectionGroupInternalAsync(ConnectionGroupDto connectionGroup)
        {
            if (connectionGroup == null)
            {
                logger.Warn("[CreateConnectionGroupInternalAsync] Failed: payload is null.");
                return CreateFailedResult("Invalid connection group payload for creation.");
            }

            if (string.IsNullOrEmpty(connectionGroup.Name))
            {
                logger.Warn("[CreateConnectionGroupInternalAsync] Failed: Name is empty.");
                return CreateFailedResult("Invalid connection group payload for creation, the Name must be filled in the params");
            }

            var agentIds = (connectionGroup.Agents ?? new List<AgentDto>())
                .Select(item => item.Id)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
            logger.Info($"[CreateConnectionGroupInternalAsync] Start create group. GroupId:{connectionGroup.Id}, Name:{connectionGroup.Name}, DataCenterType:{connectionGroup.DataCenterType}, DCInternalName:{connectionGroup.DCInternalName}, AccessConnectionType:{connectionGroup.AccessConnectionType}, AgentsCount:{agentIds.Count}, ConnectionCount:{(connectionGroup.FSConnections ?? new List<ConnectionDto>()).Count}");

            var multiGeoValidationResult = await ValidateAssignedAgentsByDCAsync(connectionGroup, agentIds);
            if (multiGeoValidationResult != null)
            {
                logger.Warn($"[CreateConnectionGroupInternalAsync] Failed at ValidateAssignedAgentsByDCAsync. Error:{multiGeoValidationResult.ErrorMessage}, AgentIds:{string.Join(",", agentIds)}");
                return multiGeoValidationResult;
            }

            var validationResult = await ValidateConnectionGroupAsync(connectionGroup, isCreate: true);
            if (validationResult != null)
            {
                logger.Warn($"[CreateConnectionGroupInternalAsync] Failed at ValidateConnectionGroupAsync. Error:{validationResult.ErrorMessage}");
                return validationResult;
            }

            var saveResult = await SaveConnectionGroupAsync(connectionGroup, isCreate: true);
            logger.Info($"[CreateConnectionGroupInternalAsync] Finished create group. MessageType:{saveResult?.MessageType}, Error:{saveResult?.ErrorMessage}, Extension:{saveResult?.Extension}");
            return saveResult;
        }

        private async Task<RAReturnMessage> CreateConnectionInternalAsync(ConnectionDto connectionDto)
        {
            var validationResult = ValidateConnection(connectionDto, isCreate: true);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await SaveConnectionAsync(connectionDto, isCreate: true);
        }

        private async Task<RAReturnMessage> SaveConnectionGroupAsync(ConnectionGroupDto connectionGroupDto, bool isCreate)
        {
            RAReturnMessage result = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };

            try
            {
                if (isCreate)
                {
                    logger.Info($"[SaveConnectionGroupAsync] Creating group. GroupId:{connectionGroupDto?.Id}, Name:{connectionGroupDto?.Name}");
                    result.Extension = (await FSRegisterService.CreateConnectionGroupAsync(connectionGroupDto)).ToString();
                }
                else
                {
                    logger.Info($"[SaveConnectionGroupAsync] Updating group. GroupId:{connectionGroupDto?.Id}, Name:{connectionGroupDto?.Name}");
                    await FSRegisterService.UpdateConnectionGroupAsync(connectionGroupDto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[SaveConnectionGroupAsync] Failed. IsCreate:{isCreate}, GroupId:{connectionGroupDto?.Id}, Name:{connectionGroupDto?.Name}, Error:{ex.Message}", ex);
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<bool> CanAccessConnectionGroupAsync(ConnectionGroupDto connectionGroup)
        {
            if (connectionGroup == null)
            {
                return false;
            }

            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return true;
            }

            var currentDC = RMSSOHelper.CurrentDCName;
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            if (string.Equals(currentDC, mainDC, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(connectionGroup.DCInternalName)
                && string.Equals(connectionGroup.DCInternalName, currentDC, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> CanAccessConnectionAsync(ConnectionDto connection)
        {
            if (connection == null)
            {
                return false;
            }

            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return true;
            }

            var currentDC = RMSSOHelper.CurrentDCName;
            var mainDC = MultiGeoDataCenterService.GetMainDC();
            if (string.Equals(currentDC, mainDC, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (connection.GroupId == Guid.Empty)
            {
                return false;
            }

            var group = await FSRegisterService.GetGroupAsync(connection.GroupId);
            return await CanAccessConnectionGroupAsync(group);
        }

        private async Task<RAReturnMessage> ValidateConnectionGroupAsync(ConnectionGroupDto connectionGroupDto, bool isCreate)
        {
            if (connectionGroupDto?.DataCenterType == DataCenterType.SpecificDC
                && connectionGroupDto.AccessConnectionType == AccessConnectionType.All)
            {
                logger.Warn("[ValidateConnectionGroupAsync] Failed: AccessConnectionType.All is not allowed when DataCenterType is SpecificDC.");
                return CreateFailedResult("AccessConnectionType.All is not allowed when DataCenterType is SpecificDC.");
            }

            var agentIds = (connectionGroupDto.Agents ?? new List<AgentDto>())
                .Select(item => item.Id)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var param = new ValidateConnectionParam
            {
                AccessConnectionType = connectionGroupDto.AccessConnectionType,
                AgentIds = agentIds,
                ConnectionIds = (connectionGroupDto.FSConnections ?? new List<ConnectionDto>()).Select(item => item.Id).Where(id => id != Guid.Empty).Distinct().ToList(),
                IsPublicApiRole = true,
                TargetDCs = string.IsNullOrEmpty(connectionGroupDto.DCInternalName) ? [] : [connectionGroupDto.DCInternalName]
            };

            if (param.ConnectionIds.Count == 0)
            {
                logger.Info("[ValidateConnectionGroupAsync] Skip connection validation because ConnectionIds is empty.");
                return null;
            }

            if (param.AccessConnectionType == AccessConnectionType.All)
            {
                if (!await RMFileSystemBrowserService.CheckHasAvailableAgentAsync())
                {
                    logger.Warn("[ValidateConnectionGroupAsync] Failed: no available agent for AccessConnectionType.All.");
                    return CreateFailedResult(I18NEntity.GetString("RM_SS_FSNoAvailableAgent"));
                }
            }
            else if (param.AccessConnectionType == AccessConnectionType.Specify)
            {
                if (param.AgentIds.Count == 0 || !await RMFileSystemBrowserService.CheckHasAvailableAgentAsync(param.AgentIds))
                {
                    logger.Warn($"[ValidateConnectionGroupAsync] Failed: no available agent for specified agents. AgentIds:{string.Join(",", param.AgentIds)}");
                    return CreateFailedResult(I18NEntity.GetString("RM_SS_FSNoAvailableAgent"));
                }
            }

            if (!ValidFSConnectionNotHaveOutsideGroup(param.ConnectionIds, connectionGroupDto.Id, isCreate))
            {
                logger.Warn($"[ValidateConnectionGroupAsync] Failed: some connections belong to a different group. GroupId:{connectionGroupDto.Id}, ConnectionIds:{string.Join(",", param.ConnectionIds)}");
                return CreateFailedResult("Some connections belong to a different group.");
            }

            if (!ValidAllConnectionExist(param.ConnectionIds))
            {
                logger.Warn($"[ValidateConnectionGroupAsync] Failed: some connections do not exist. ConnectionIds:{string.Join(",", param.ConnectionIds)}");
                return CreateFailedResult("Invalid connections payload for update, the connection must be exist");
            }

            // Split validation into batches to avoid timeout when there are too many connections.
            const int validationBatchSize = 50;
            var succeededConnectionIds = new List<Guid>(param.ConnectionIds.Count);

            for (var i = 0; i < param.ConnectionIds.Count; i += validationBatchSize)
            {
                var batchConnectionIds = param.ConnectionIds
                    .Skip(i)
                    .Take(validationBatchSize)
                    .ToList();

                var batchParam = new ValidateConnectionParam
                {
                    AccessConnectionType = param.AccessConnectionType,
                    AgentIds = param.AgentIds,
                    ConnectionIds = batchConnectionIds,
                    IsPublicApiRole = param.IsPublicApiRole,
                    TargetDCs = param.TargetDCs
                };

                var batchSucceededConnectionIds = await ValidateConnections(batchParam);
                if (batchSucceededConnectionIds.Count <= 0)
                {
                    logger.Warn($"[ValidateConnectionGroupAsync] Failed: ValidateConnections returned no success for batch. BatchConnectionIds:{string.Join(",", batchConnectionIds)}");
                    return CreateFailedResult(I18NEntity.GetString("RM_FS_Register_FSConnectionValidationTestFailed"));
                }

                var batchFailedConnectionIds = batchConnectionIds.Except(batchSucceededConnectionIds).ToList();
                if (batchFailedConnectionIds.Count > 0)
                {
                    logger.Warn($"[ValidateConnectionGroupAsync] Failed: connection validation has failed items in batch. FailedConnectionIds:{string.Join(",", batchFailedConnectionIds)}, SucceededConnectionIds:{string.Join(",", batchSucceededConnectionIds)}");
                }

                logger.Info($"[ValidateConnectionGroupAsync] Batch validation succeeded. BatchConnectionIds:{string.Join(",", batchConnectionIds)}, SucceededConnectionIds:{string.Join(",", batchSucceededConnectionIds)}");
                succeededConnectionIds.AddRange(batchSucceededConnectionIds);
            }

            return null;
        }

        private bool ValidFSConnectionNotHaveOutsideGroup(List<Guid> connectionIds, Guid id, bool isCreate)
        {
            return RMFileSystemBrowserService.ValidFSConnectionNotHaveOutsideGroup(connectionIds, id, isCreate);
        }

        private async Task<RAReturnMessage> ValidateAssignedAgentsByDCAsync(ConnectionGroupDto connectionGroupDto, List<Guid> agentIds)
        {
            if (!(await MultiGeoSettingService.IsEnableMultiGeoFeature()) || connectionGroupDto == null)
            {
                return null;
            }

            if (connectionGroupDto.DataCenterType == DataCenterType.SpecificDC)
            {
                if (string.IsNullOrWhiteSpace(connectionGroupDto.DCInternalName))
                {
                    logger.Warn("[ValidateAssignedAgentsByDCAsync] Failed: DCInternalName is required when DataCenterType is SpecificDC.");
                    return CreateFailedResult("DCInternalName is required when DataCenterType is SpecificDC.");
                }

                if (agentIds == null || agentIds.Count == 0)
                {
                    logger.Warn("[ValidateAssignedAgentsByDCAsync] Failed: agent list is empty when DataCenterType is SpecificDC.");
                    return CreateFailedResult("Agent list cannot be empty when DataCenterType is SpecificDC.");
                }
            }

            if (agentIds == null || agentIds.Count == 0)
            {
                return null;
            }

            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            var agents = (await AgentMgmtService.GetAgentsByIdsAsync(agentIds))
                .ToDictionary(agent => agent.Id);

            if (agentIds.Any(agentId => !agents.ContainsKey(agentId)))
            {
                var missingAgentIds = agentIds.Where(agentId => !agents.ContainsKey(agentId)).ToList();
                logger.Warn($"[ValidateAssignedAgentsByDCAsync] Failed: some agents do not exist. MissingAgentIds:{string.Join(",", missingAgentIds)}");
                return CreateFailedResult("Invalid agents payload for creation, the agent must exist.");
            }
            if (agents.Values.Any(agent => !IsAgentMatchedSelectedDC(agent, connectionGroupDto, mainDCInternalName)))
            {
                var unmatchedAgentIds = agents.Values
                    .Where(agent => !IsAgentMatchedSelectedDC(agent, connectionGroupDto, mainDCInternalName))
                    .Select(agent => agent.Id)
                    .ToList();
                logger.Warn($"[ValidateAssignedAgentsByDCAsync] Failed: some agents do not match selected DC. DataCenterType:{connectionGroupDto.DataCenterType}, SelectedDC:{connectionGroupDto.DCInternalName}, MainDC:{mainDCInternalName}, UnmatchedAgentIds:{string.Join(",", unmatchedAgentIds)}");
                return connectionGroupDto.DataCenterType == DataCenterType.SpecificDC
                    ? CreateFailedResult("When DataCenterType is SpecificDC, all agents must belong to the selected DC.")
                    : CreateFailedResult("When DataCenterType is DefaultDC, all agents must have an empty DC or belong to the Main DC.");
            }

            return null;
        }

        private static bool IsAgentMatchedSelectedDC(RMAgentDto agent, ConnectionGroupDto connectionGroupDto, string mainDCInternalName)
        {
            if (agent == null || connectionGroupDto == null)
            {
                return false;
            }

            if (connectionGroupDto.DataCenterType == DataCenterType.SpecificDC)
            {
                return !string.IsNullOrWhiteSpace(connectionGroupDto.DCInternalName)
                    && string.Equals(agent.DCInternalName, connectionGroupDto.DCInternalName, StringComparison.OrdinalIgnoreCase);
            }

            if (connectionGroupDto.DataCenterType == DataCenterType.DefaultDC)
            {
                return string.IsNullOrWhiteSpace(agent.DCInternalName)
                    || (!string.IsNullOrWhiteSpace(mainDCInternalName)
                        && string.Equals(agent.DCInternalName, mainDCInternalName, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        private bool ValidAllConnectionExist(List<Guid> connectionIds)
        {
            return RMFileSystemBrowserService.ValidAllConnectionExist(connectionIds);
        }

        private async Task<RAReturnMessage> SaveConnectionAsync(ConnectionDto connectionDto, bool isCreate)
        {
            var result = new RAReturnMessage();
            try
            {
                var resultCode = isCreate
                    ? await FSRegisterService.CreateConnectoinAsync(connectionDto)
                    : await FSRegisterService.UpdateConnectoinAsync(connectionDto);

                if (resultCode == 1)
                {
                    result.MessageType = RAMessageType.Successful;
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    if (resultCode == -2)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_UNCPath_Exist");
                    }
                    if (resultCode == -3)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_JPMCConnectionId_Exist");
                    }
                    if (resultCode == -4) // Duplicate connection name
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_FS_Register_SameConnectionNameErrorMessage");
                    }
                    if (resultCode == -5) // Exceed 255 length
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_JS_Common_Msg_CannotExceed255");
                    }
                    if (resultCode == -6)
                    {
                        result.ErrorMessage = "JPMC Id should not be null";
                    }
                    if (resultCode == -7)
                    {
                        result.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                    }
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString(ex.Message);
            }
            return result;
        }

        private RAReturnMessage ValidateConnection(ConnectionDto connectionDto, bool isCreate)
        {
            if (connectionDto == null)
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation."
                    : "Invalid connection payload for update, the Id must be filled in the params");
            }

            if (isCreate ? connectionDto.Id != Guid.Empty : connectionDto.Id == Guid.Empty)
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation."
                    : "Invalid connection payload for update, the Id must be filled in the params");
            }

            if (isCreate && string.IsNullOrEmpty(connectionDto.JPMCConnectionId))
            {
                return CreateFailedResult("Invalid connection payload for creatio, the JPMCConnectionId must be filled in the params");
            }

            if (!string.IsNullOrWhiteSpace(connectionDto.JPMCConnectionId)
                && !IsValidInternalId(connectionDto.JPMCConnectionId))
            {
                return CreateFailedResult("JPMCConnectionId can contain only letters, numbers, spaces, periods, hyphens, and underscores, and must be 256 characters or fewer.");
            }

            if (string.IsNullOrWhiteSpace(connectionDto.Name))
            {
                return CreateFailedResult(isCreate
                    ? "Invalid connection payload for creation, the Name must be filled in the params"
                    : "Invalid connection payload for update, the Name must be filled in the params");
            }

            var enableJPMCFileSystemFeature = RMKeyValueDao.GetValueByKeyAsync<bool>(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult();
            if (!IsValidUncPath(connectionDto.UNCPath))
            {
                if (enableJPMCFileSystemFeature)
                    return CreateFailedResult(I18NEntity.GetString("RM_FS_Register_PathInputValidateMessage"));
                else
                    return CreateFailedResult(I18NEntity.GetString("RM_FS_Register_UNCPathInputValidateMessage"));
            }
            if (enableJPMCFileSystemFeature)
            {
                if (connectionDto.RecordOwners != null && connectionDto.RecordOwners.Count > 0 && !HasValidOwners(connectionDto.RecordOwners))
                {
                    return CreateFailedResult("RecordOwners is invalid, or it is required because JPMC file system feature is enabled.");
                }

                if (!HasValidOwners(connectionDto.InformationOwners))
                {
                    return CreateFailedResult("Information is invalid, or it is required because JPMC file system feature is enabled");
                }
            }

            return null;
        }

        private static bool IsValidUncPath(string uncPath)
        {
            //Need to clear the direction chars before validation
            var clearPath = new string(uncPath.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Format).ToArray());
            return !string.IsNullOrWhiteSpace(uncPath)
                && UncPathRegex.IsMatch(clearPath.Trim());
        }
        private bool HasValidOwners(List<ToUserInfo> owners)
        {
            if (owners == null || owners.Count == 0)
            {
                return false;
            }

            foreach (var owner in owners)
            {
                if (string.IsNullOrWhiteSpace(owner?.Id))
                {
                    return false;
                }

                var aadAccount = ResolveAADAccount(owner.Id);
                if (aadAccount == null)
                {
                    logger.Warn($"Failed to resolve AAD account for owner Id {owner.Id}. The owner will be considered as invalid.");
                }
                else
                {
                    logger.Info($"Successfully resolved AAD account for owner Id {owner.Id}"); 
                    owner.UserPrincipalName = aadAccount.UserPrincipalName ?? aadAccount.Mail ?? aadAccount.DisplayName;
                    owner.DisplayName = aadAccount.DisplayName;
                    owner.Email = aadAccount.Mail;
                    owner.InviteType = aadAccount.InviteType;
                }
            }

            return true;
        }

        private AADAccount ResolveAADAccount(string aadId)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            var group = TryGetGroup(tenantId, aadId);
            if (group != null)
            {
                group.InviteType = AccountType.Group;
                return group;
            }

            return TryGetUser(tenantId, aadId);
        }

        private AADAccount TryGetUser(string tenantId, string aadId)
        {
            try
            {
                return AccountWrapperService.GetAccount(tenantId, aadId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get user from AAD with tenantId {tenantId} and aadId {aadId}. Exception: {ex} Messsage: {ex.Message}");
                return null;
            }
        }

        private AADAccount TryGetGroup(string tenantId, string aadId)
        {
            try
            {
                return AccountWrapperService.GetGroupsByAadId(tenantId, aadId);
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get group from AAD with tenantId {tenantId} and aadId {aadId}. Exception: {ex} Messsage: {ex.Message}");
                return null;
            }
        }
        private static bool IsValidInternalId(string internalId)
        {
            return !string.IsNullOrWhiteSpace(internalId)
                && InternalIdRegex.IsMatch(internalId.Trim());
        }

        private static ConnectionGroupDto BuildConnectionGroupDto(ConnectionGroupPublic connectionGroup, bool isCreate = false)
        {
            if (connectionGroup == null) return null;
            var dcInternalName = connectionGroup.DataCenterType == DataCenterType.DefaultDC
                ? string.Empty
                : connectionGroup.DCInternalName;

            return new ConnectionGroupDto
            {
                Id = isCreate && connectionGroup.Id == Guid.Empty ? Guid.NewGuid() : connectionGroup.Id,
                Name = connectionGroup.Name,
                Description = connectionGroup.Description,
                AccessConnectionType = connectionGroup.AccessConnectionType,
                FSConnections = (connectionGroup.ConnectionIds ?? new List<Guid>())
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .Select(id => new ConnectionDto { Id = id })
                    .ToList(),
                RemoveFSConnections = (connectionGroup.ConnectionIdsToRemove ?? new List<Guid>())
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .Select(id => new ConnectionDto { Id = id })
                    .ToList(),
                Agents = (connectionGroup.AssignedAgentIds ?? new List<Guid>())
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .Select(id => new AgentDto { Id = id })
                    .ToList(),
                DataCenterType = connectionGroup.DataCenterType,
                DCInternalName = dcInternalName,
            };
        }

        private static RAReturnMessage CreateFailedResult(string errorMessage)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = errorMessage
            };
        }
    }
}
