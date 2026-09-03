using AvePoint.GCommon.Utility.I18N;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Public.Common.Requests;
using AvePoint.RA.Api.Web.Public.Common.Response;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("connection-groups")]
    public class ConnectionGroupsController : RAWebApiBase
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(ConnectionGroupsController));

        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        private IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetConnectionGroups()
        {
            return this.OkApi(await FSRegisterService.GetAllGroupsAsync());
        }

        [HttpGet("{id:guid}")]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetConnectionGroupById(Guid id)
        {
            var group = await FSRegisterService.GetGroupOrNullAsync(id);
            if (group == null)
            {
                return this.NotFoundApi("Connection group not found.");
            }

            if (!await CanAccessConnectionGroupAsync(group))
            {
                return this.ForbiddenApi("The connection group is not available in the current data center.");
            }

            return this.OkApi(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConnectionGroup([FromBody] ConnectionGroupRequest connectionGroupDto)
        {
            if (connectionGroupDto != null && connectionGroupDto.Id != Guid.Empty)
            {
                return this.BadRequestApi("Invalid connection group payload for creation.");
            }

            var connectionGroup = BuildConnectionGroupDto(connectionGroupDto?.ToContract(), isCreate: true);
            var result = await RouteMultiGeoApiActionAsync(connectionGroup, MultiGeoOperationType.SaveConnectionGroup,
                CreateConnectionGroupInternalAsync,
                _ => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
            return this.FromReturnMessage(result);
        }

        [HttpPost("batch")]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> CreateConnectionGroups([FromBody] List<ConnectionGroupRequest> connectionGroupDtos)
        {
            if (connectionGroupDtos == null || connectionGroupDtos.Count == 0)
            {
                return this.BadRequestApi("Invalid connection group payload for creation.");
            }

            var results = new List<RAReturnMessage>(connectionGroupDtos.Count);
            var connectionGroups = connectionGroupDtos.Where(c => c != null).Select(c => BuildConnectionGroupDto(c.ToContract(), isCreate: true)).ToList();
            foreach (var connectionGroup in connectionGroups)
            {
                results.Add(await CreateConnectionGroupInternalAsync(connectionGroup));
            }

            await TrySyncCommonDataAfterBatchCreateAsync(results, MultiGeoOperationType.SaveConnectionGroups);
            var batchResponse = BatchOperationResponseFactory.Create(results, "Created successfully.", "Creation failed.");
            return this.FromBatchOperation(batchResponse, "All connection groups were created successfully.", "One or more connection groups could not be created.");
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateConnectionGroup(Guid id, [FromBody] ConnectionGroupRequest connectionGroupDto)
        {
            var requestParam = connectionGroupDto?.ToContract(id) ?? new ConnectionGroupPublic { Id = id };
            if (requestParam.Id == Guid.Empty)
            {
                return this.BadRequestApi("Invalid connection group payload for update,the Id must be filled in the params");
            }
            if (string.IsNullOrEmpty(requestParam.Name))
            {
                return this.BadRequestApi("Invalid connection group payload for update, the Name must be filled in the params");
            }

            var connectionGroup = BuildConnectionGroupDto(requestParam);
            return this.FromReturnMessage(await UpdateConnectionGroupInternalAsync(connectionGroup));
        }

        [HttpPost("batch-delete")]
        public async Task<IActionResult> DeleteConnectionGroups([FromBody] List<Guid> groupsIds)
        {
            var result = await RouteMultiGeoApiActionAsync(groupsIds, MultiGeoOperationType.DeleteGroup,
                request => FSRegisterService.DeleteGroupConnectoinAsync(request),
                _ => -1);

            return this.OkApi(result);
        }

        private async Task<RAReturnMessage> UpdateConnectionGroupInternalAsync(ConnectionGroupDto connectionGroup)
        {
            var multiGeoValidationResult = await ValidateAssignedAgentsByDCAsync(connectionGroup, connectionGroup.Agents.Select(x => x.Id).ToList());
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
                request => SaveConnectionGroupAsync(request, isCreate: false),
                _ => CreateFailedResult(I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")));
        }

        private async Task<RAReturnMessage> CreateConnectionGroupInternalAsync(ConnectionGroupDto connectionGroup)
        {
            if (connectionGroup == null)
            {
                return CreateFailedResult("Invalid connection group payload for creation.");
            }
            if (string.IsNullOrEmpty(connectionGroup.Name))
            {
                return CreateFailedResult("Invalid connection group payload for creation, the Name must be filled in the params");
            }

            var agentIds = (connectionGroup.Agents ?? []).Select(item => item.Id).Where(id => id != Guid.Empty).Distinct().ToList();
            var multiGeoValidationResult = await ValidateAssignedAgentsByDCAsync(connectionGroup, agentIds);
            if (multiGeoValidationResult != null)
            {
                return multiGeoValidationResult;
            }
            var validationResult = await ValidateConnectionGroupAsync(connectionGroup, isCreate: true);
            if (validationResult != null)
            {
                return validationResult;
            }

            return await SaveConnectionGroupAsync(connectionGroup, isCreate: true);
        }

        private async Task<RAReturnMessage> SaveConnectionGroupAsync(ConnectionGroupDto connectionGroupDto, bool isCreate)
        {
            var result = new RAReturnMessage { MessageType = RAMessageType.Successful };
            try
            {
                if (isCreate)
                {
                    result.Extension = (await FSRegisterService.CreateConnectionGroupAsync(connectionGroupDto)).ToString();
                }
                else
                {
                    await FSRegisterService.UpdateConnectionGroupAsync(connectionGroupDto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to save connection group {connectionGroupDto?.Name}", ex);
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

        private async Task<RAReturnMessage> ValidateConnectionGroupAsync(ConnectionGroupDto connectionGroupDto, bool isCreate)
        {
            if (connectionGroupDto?.DataCenterType == DataCenterType.SpecificDC && connectionGroupDto.AccessConnectionType == AccessConnectionType.All)
            {
                return CreateFailedResult("AccessConnectionType.All is not allowed when DataCenterType is SpecificDC.");
            }

            var agentIds = (connectionGroupDto.Agents ?? []).Select(item => item.Id).Where(id => id != Guid.Empty).Distinct().ToList();
            var param = new ValidateConnectionParam
            {
                AccessConnectionType = connectionGroupDto.AccessConnectionType,
                AgentIds = agentIds,
                ConnectionIds = (connectionGroupDto.FSConnections ?? []).Select(item => item.Id).Where(id => id != Guid.Empty).Distinct().ToList(),
                IsPublicApiRole = true,
                TargetDCs = string.IsNullOrEmpty(connectionGroupDto.DCInternalName) ? [] : [connectionGroupDto.DCInternalName]
            };

            if (param.ConnectionIds.Count == 0)
            {
                return null;
            }
            if (param.AccessConnectionType == AccessConnectionType.All)
            {
                if (!await RMFileSystemBrowserService.CheckHasAvailableAgentAsync())
                {
                    return CreateFailedResult(I18NEntity.GetString("RM_SS_FSNoAvailableAgent"));
                }
            }
            else if (param.AccessConnectionType == AccessConnectionType.Specify)
            {
                if (param.AgentIds.Count == 0 || !await RMFileSystemBrowserService.CheckHasAvailableAgentAsync(param.AgentIds))
                {
                    return CreateFailedResult(I18NEntity.GetString("RM_SS_FSNoAvailableAgent"));
                }
            }
            if (!RMFileSystemBrowserService.ValidFSConnectionNotHaveOutsideGroup(param.ConnectionIds, connectionGroupDto.Id, isCreate))
            {
                return CreateFailedResult("Some connections belong to a different group.");
            }
            if (!RMFileSystemBrowserService.ValidAllConnectionExist(param.ConnectionIds))
            {
                return CreateFailedResult("Invalid connections payload for update, the connection must be exist");
            }

            var successedConnectionIds = await RMFileSystemBrowserService.ValidateTestConnectionsAsync(param);
            if (successedConnectionIds.Count <= 0)
            {
                return CreateFailedResult(I18NEntity.GetString("RM_FS_Register_FSConnectionValidationTestFailed"));
            }

            return null;
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
                    return CreateFailedResult("DCInternalName is required when DataCenterType is SpecificDC.");
                }
                if (agentIds == null || agentIds.Count == 0)
                {
                    return CreateFailedResult("Agent list cannot be empty when DataCenterType is SpecificDC.");
                }
            }
            if (agentIds == null || agentIds.Count == 0)
            {
                return null;
            }

            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            var agents = (await AgentMgmtService.GetAgentsByIdsAsync(agentIds)).ToDictionary(agent => agent.Id);
            if (agentIds.Any(agentId => !agents.ContainsKey(agentId)))
            {
                return CreateFailedResult("Invalid agents payload for creation, the agent must exist.");
            }
            if (agents.Values.Any(agent => !IsAgentMatchedSelectedDC(agent, connectionGroupDto, mainDCInternalName)))
            {
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

            MultiGeoReplicaFailureLogWriter.WriteForJob(global::AvePoint.RA.Contract.Tenant.TenantLocalValue.LogonGroupId, operationType.ToString());
            await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(JobRunBy.Control);
        }

        private static ConnectionGroupDto BuildConnectionGroupDto(ConnectionGroupPublic connectionGroup, bool isCreate = false)
        {
            if (connectionGroup == null)
            {
                return null;
            }

            var dcInternalName = connectionGroup.DataCenterType == DataCenterType.DefaultDC ? string.Empty : connectionGroup.DCInternalName;
            return new ConnectionGroupDto
            {
                Id = isCreate && connectionGroup.Id == Guid.Empty ? Guid.NewGuid() : connectionGroup.Id,
                Name = connectionGroup.Name,
                Description = connectionGroup.Description,
                AccessConnectionType = connectionGroup.AccessConnectionType,
                FSConnections = (connectionGroup.ConnectionIds ?? []).Where(id => id != Guid.Empty).Distinct().Select(id => new ConnectionDto { Id = id }).ToList(),
                RemoveFSConnections = (connectionGroup.ConnectionIdsToRemove ?? []).Where(id => id != Guid.Empty).Distinct().Select(id => new ConnectionDto { Id = id }).ToList(),
                Agents = (connectionGroup.AssignedAgentIds ?? []).Where(id => id != Guid.Empty).Distinct().Select(id => new AgentDto { Id = id }).ToList(),
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

