using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMPublicAPI
{
    public class AgentMgmtPublicServices : IAgentMgmtPublicServices
    {
        private RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private ICertificateService CertificateService => PlatformWindsorManager.GetService<ICertificateService>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();


        public async Task<RAReturnMessage> QueryAgentsAsync(AgentQueryParam param)
        {
            try
            {
                var queryDto = new AgentQueryParams
                {
                    PageIndex = param.PageIndex,
                    PageSize = param.PageSize,
                    SearchValue = param.SearchValue,
                    SortBy = param.SortBy,
                    IsAscending = param.IsAscending,
                    DataCenterName = param.DataCenterName
                };

                var result = await AgentMgmtService.QueryAgentsAsync(queryDto);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                    Extension = JsonConvert.SerializeObject(result)
                };
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while querying agents.", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to query agents."
                };
            }
        }

        public async Task<RAReturnMessage> CreateAgentWithIdAsync(AgentCreateParam param)
        {
            try
            {

                (bool validName, RAReturnMessage value) = await CheckAgentNameConflictAsync(param);
                if (!validName)
                {
                    return value;
                }

                var request = new RMAgentDto
                {
                    Id = param.Id,
                    Name = param.Name,
                    Description = param.Description,
                    SourceType = SourceType.FileSystem,
                    ClientId = param.ClientId,
                    CertificateId = param.CertificateId,
                    InstallationCode = param.InstallationCode,
                    AuthCode = param.AuthCode,
                    DCInternalName = param.DCInternalName,
                    CollectLog = param.CollectLog,
                };

                var createdAgentId = await AgentMgmtService.CreateReplicaAgentAndGetIdAsync(request);
                if (createdAgentId.HasValue && createdAgentId.Value != Guid.Empty)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful,
                    };
                }

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to create agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while creating agent: {param.Name}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while creating agent."
                };
            }
        }

        private async Task<(bool validName, RAReturnMessage value)> CheckAgentNameConflictAsync(AgentCreateParam param)
        {
            var allAgents = await AgentMgmtService.GetAllAsync();
            if (allAgents.Any(o => o.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return (validName: false, value: new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "An agent with the same name already exists."
                });
            }

            return (validName: true, value: null);
        }

        public async Task<RAReturnMessage> CreateAgentAsync(AgentCreateParam param)
        {
            try
            {
                param.Name = TrimAgentName(param.Name);

                var dcValidationResult = await ValidateCreateAgentMultiGeoAsync(param.DCInternalName);
                if (dcValidationResult != null)
                {
                    return dcValidationResult;
                }

                var allAgents = await AgentMgmtService.GetAllAsync();
                if (allAgents.Any(o => o.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "An agent with the same name already exists."
                    };
                }

                var clientId = KeyValueService.Get(KeyNameCollection.AppManagementClientId, RMNameValueType.AppManagementClientId)?.Value;
                if (string.IsNullOrEmpty(clientId))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Client ID is not configured."
                    };
                }

                var certificates = await CertificateService.GetAllWithoutBinaryDataAsync(false);
                var defaultCertIdStr = KeyValueService.Get(KeyNameCollection.DefaultCertificateId, RMNameValueType.DefaultCertificate)?.Value;

                Guid certificateId = Guid.Empty;
                if (!string.IsNullOrEmpty(defaultCertIdStr) && Guid.TryParse(defaultCertIdStr, out Guid parsedId))
                {
                    certificateId = parsedId;
                }

                var defaultCert = certificates.FirstOrDefault(o => o.Id == certificateId) ?? certificates.FirstOrDefault();
                if (defaultCert == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "No valid certificate found."
                    };
                }

                var request = new RMAgentDto
                {
                    Name = param.Name,
                    Description = param.Description,
                    SourceType = AvePoint.Hybrid.Contract.Object.SourceType.FileSystem,
                    ClientId = clientId,
                    CertificateId = defaultCert.Id,
                    InstallationCode = GenerateRandomString(),
                    AuthCode = GenerateRandomString(),
                    CollectLog = param.CollectLog,
                    DCInternalName = param.DCInternalName
                };

                var createdAgentId = AgentMgmtService.CreateAgentAndGetId(request);
                if (createdAgentId.HasValue && createdAgentId.Value != Guid.Empty)
                {
                    request.Id = createdAgentId.Value;
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful,
                        Extension = JsonConvert.SerializeObject(request)
                    };
                }

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to create agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while creating agent: {param.Name}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while creating agent."
                };
            }
        }

        public async Task<RAReturnMessage> UpdateAgentAsync(AgentUpdateParam param)
        {
            try
            {
                param.Name = TrimAgentName(param.Name);
                var existingAgent = AgentMgmtService.Get(param.Id);
                if (existingAgent == null)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Agent not found."
                    };
                }

                var allAgents = await AgentMgmtService.GetAllAsync();
                if (allAgents.Any(o => o.Id != param.Id && o.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "An agent with the same name already exists."
                    };
                }

                var updateValidationResult = await ValidateUpdateAgentMultiGeoAsync(param, existingAgent);
                if (updateValidationResult != null)
                {
                    return updateValidationResult;
                }

                existingAgent.Name = param.Name;
                existingAgent.Description = param.Description;
                existingAgent.CollectLog = param.CollectLog;
                existingAgent.DCInternalName = param.DCInternalName;

                 var updateResult = await AgentMgmtService.UpdateAgentAsync(existingAgent);
                if (updateResult == "0")
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful
                    };
                }

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Failed to update agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while updating agent: {param.Id}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while updating agent."
                };
            }
        }

        public async Task<RAReturnMessage> DeleteAgentAsync(AgentActionParam param)
        {
            try
            {
                var result = await AgentMgmtService.DeleteAsync(param.Id);
                return new RAReturnMessage
                {
                    MessageType = result ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = result ? string.Empty : "Failed to delete agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while deleting agent: {param.Id}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while deleting agent."
                };
            }
        }

        public async Task<RAReturnMessage> DisableAgentAsync(AgentActionParam param)
        {
            try
            {
                var agent = AgentMgmtService.Get(param.Id);
                if (agent == null)
                {
                    return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent not found" };
                }
                if (agent.Status == ServiceStatus.NotInstalled || agent.Status == ServiceStatus.InActive)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Agent is inactive or not installed."
                    };
                }
                var result = await AgentMgmtService.DisableAsync(param.Id);
                return new RAReturnMessage
                {
                    MessageType = result ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = result ? string.Empty : "Failed to disable agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while disabling agent: {param.Id}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while disabling agent."
                };
            }
        }

        public async Task<RAReturnMessage> EnableAgentAsync(AgentActionParam param)
        {
            try
            {
                var agent = AgentMgmtService.Get(param.Id);
                if (agent == null)
                {
                    return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent not found" };
                }
                if (agent.Status == ServiceStatus.NotInstalled || agent.Status == ServiceStatus.InActive)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = "Agent is inactive or not installed."
                    };
                }
                var result = await AgentMgmtService.EnableAsync(param.Id);
                return new RAReturnMessage
                {
                    MessageType = result ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = result ? string.Empty : "Failed to enable agent."
                };
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while enabling agent: {param.Id}", ex);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while enabling agent."
                };
            }
        }

        public async Task<RAReturnMessage> UpdateAgentJobLimitAsync(AgentJobLimitParam param)
        {
            try
            {
                var setting = RMKeyValueDao.GetValueByKey(KeyNameCollection.EnableFileSystemHighPerformanceMode);

                var config = setting == null
                    ? new FSHighPerformanceConfiguration()
                    : JsonConvert.DeserializeObject<FSHighPerformanceConfiguration>(setting.Value)
                        ?? new FSHighPerformanceConfiguration();

                config.Setting ??= new FSHighPerformanceSetting();

                if (config.Setting.MaxJobPerAgent == param.JobLimit)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful
                    };
                }

                config.Setting.MaxJobPerAgent = param.JobLimit;

                var result = await RMKeyValueDao.SaveOrUpdateAsync(new()
                {
                    Key = KeyNameCollection.EnableFileSystemHighPerformanceMode,
                    Value = SerializerHelper.SerializeByJsonSerializer(config)
                });

                return new RAReturnMessage
                {
                    MessageType = result
                        ? RAMessageType.Successful
                        : RAMessageType.Failed,
                    ErrorMessage = result
                        ? string.Empty
                        : "Failed to update agent job limit."
                };
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"An error occurred while updating agent job limit: {param.JobLimit}",
                    ex);

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = "Error occurred while updating agent job limit."
                };
            }
        }

        private async Task<RAReturnMessage> ValidateCreateAgentMultiGeoAsync(string dcInternalName)
        {
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(dcInternalName))
            {
                return null;
            }

            return await ValidateDCInternalNameAsync(dcInternalName);
        }

        private async Task<RAReturnMessage> ValidateUpdateAgentMultiGeoAsync(AgentUpdateParam param, RMAgentDto existingAgent)
        {
            if (!await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                return null;
            }

            if (param.DCInternalName == null)
            {
                param.DCInternalName = existingAgent.DCInternalName;
                return null;
            }

            var dcValidationResult = await ValidateDCInternalNameAsync(param.DCInternalName);
            if (dcValidationResult != null)
            {
                return dcValidationResult;
            }

            if (!IsSameDCInternalName(param.DCInternalName, existingAgent.DCInternalName))
            {
                return FailedResult("Selected DC cannot be modified when multi-geo is enabled.");
            }

            param.DCInternalName = existingAgent.DCInternalName;
            return null;
        }

        private async Task<RAReturnMessage> ValidateDCInternalNameAsync(string dcInternalName)
        {
            if (string.IsNullOrWhiteSpace(dcInternalName))
            {
                return null;
            }

            var supportedDCInternalNames = await GetSupportedDCInternalNamesAsync();
            if (supportedDCInternalNames.Contains(dcInternalName))
            {
                return null;
            }

            return FailedResult("The specified DCInternalName is invalid.");
        }

        private async Task<HashSet<string>> GetSupportedDCInternalNamesAsync()
        {
            var supportedDCInternalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var supportedDCs = await MultiGeoDataCenterService.GetDCsSupported();
            foreach (var dc in supportedDCs.Where(dc => !string.IsNullOrWhiteSpace(dc?.DCInternalName)))
            {
                supportedDCInternalNames.Add(dc.DCInternalName);
            }

            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            if (!string.IsNullOrWhiteSpace(mainDCInternalName))
            {
                supportedDCInternalNames.Add(mainDCInternalName);
            }

            return supportedDCInternalNames;
        }

        private bool IsSameDCInternalName(string requestedDCInternalName, string existingDCInternalName)
        {
            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            return string.Equals(
                NormalizeDCInternalName(requestedDCInternalName, mainDCInternalName),
                NormalizeDCInternalName(existingDCInternalName, mainDCInternalName),
                StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizeDCInternalName(string dcInternalName, string mainDCInternalName)
        {
            return string.IsNullOrWhiteSpace(dcInternalName) ? mainDCInternalName ?? string.Empty : dcInternalName.Trim();
        }

        private static RAReturnMessage FailedResult(string errorMessage)
        {
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = errorMessage
            };
        }

        private string GenerateRandomString(int num = 11)
        {
            System.Threading.Thread.Sleep(100);
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()abcdefghijklmnopqrstuvwxyz";
            string str = "";
            for (int i = 0; i < num; i++)
            {
                str += chars[SecurityUtils.GetRandomNumber(0, chars.Length)];
            }
            return str;
        }

        private string TrimAgentName(string name)
        {
            return string.IsNullOrEmpty(name) ? name : name.Length > 255 ? name.Substring(0, 255) : name;
        }
    }
}