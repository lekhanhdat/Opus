using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/CPAgentMgmtApi/[action]")]
    public class CPAgentMgmtResourceApiController : RAWebApiBase
    {
        private const int DefaultCertificationDurationInYear = 2;
        private readonly RALogger logger = RALogger.GetInstance(typeof(CPAgentMgmtResourceApiController));
        private IAgentMgmtService _agentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private ICertificateService _certificateService => PlatformWindsorManager.GetService<ICertificateService>();
        private IKeyValueService _keyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMFileSystemSettingsService _fileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        [HttpPost]
        public async Task<RMAgentCreateResult> CreateAgent([FromBody] RMAgentDto dto)
        {
            ValidateReplicaCreateAgentRequest(dto);

            if (!IsValidAgentType(dto)) return RMAgentCreateResult.Failed;
            if (await CheckSameAgentNameAsync(dto)) return RMAgentCreateResult.SameNameExist;

            var createdAgentId = await _agentMgmtService.CreateReplicaAgentAndGetIdAsync(dto);
            if (createdAgentId.HasValue)
            {
                dto.Id = createdAgentId.Value;
                return RMAgentCreateResult.Succeed;
            }

            return RMAgentCreateResult.Failed;
        }

        [HttpPost]
        public async Task<string> UpdateAgent([FromBody] RMAgentDto dto)
        {
            if (!IsValidAgentType(dto) || await CheckSameAgentNameAsync(dto))
            {
                return "1";
            }
            return await _agentMgmtService.UpdateAgentAsync(dto);
        }

        [HttpPost]
        public Task<bool> DeleteAgent([FromBody] Guid id)
        {
            return _agentMgmtService.DeleteAsync(id);
        }

        [HttpPost]
        public Task<bool> DisableAgent([FromBody] Guid id)
        {
            return _agentMgmtService.DisableAsync(id);
        }

        [HttpPost]
        public Task<bool> EnableAgent([FromBody] Guid id)
        {
            return _agentMgmtService.EnableAsync(id);
        }

        [HttpPost]
        public Task<bool> SetupNotify()
        {
            return _keyValueService.SaveAsync(
                new RMNameValueDto
                {
                    Name = TenantLocalValue.LogonUserId,
                    Value = bool.TrueString,
                    Type = RMNameValueType.AppManagementDoNotShowNotify
                });
        }

        [HttpPost]
        public Task<bool> SaveClientId([FromBody] string clientId)
        {
            return _agentMgmtService.SaveClientIdAsync(clientId);
        }

        [HttpPost]
        public Task<bool> SyncAgentRuntimeStatus([FromBody] AgentRuntimeStatusSyncRequest request)
        {
            if (request?.Agent == null)
            {
                throw new InvalidOperationException("Agent runtime status request is required.");
            }

            return request.Action == AgentRuntimeStatusSyncAction.UpdateStatus
                ? _agentMgmtService.UpdateStatusAsync(request.Agent.Id, request.Agent.Status)
                : _agentMgmtService.UpdateAgentResourceUsageAsync(request.Agent);
        }

        [HttpPost]
        public Task<bool> SyncAgentStatusAfterUpgrade([FromBody] AgentInfo agentInfo)
        {
            if (agentInfo == null || agentInfo.AgentId == Guid.Empty)
            {
                throw new InvalidOperationException("Agent status sync request is required.");
            }

            return _agentMgmtService.UpdateStatusAsync(agentInfo.AgentId, agentInfo.Status);
        }

        [HttpPost]
        public async Task<bool> CreateCertificate([FromBody] RMCertificateCreateRequest request)
        {
            ValidateReplicaCreateCertificateRequest(request);

            var newId = await _certificateService.CreateReplicaCertificateAsync(request.Certificate);
            if (Guid.Empty != newId && request.SetAsDefault) await SetDefaultCertificateAsync(newId);
            return newId != Guid.Empty;
        }

        [HttpPost]
        public async Task<bool> HasAgentsRunningJobs([FromBody] List<Guid> agentIds)
        {
            return _fileSystemSettingsService.HasRunningJobOnAgentIds(agentIds);
        }

        [HttpPost]
        public async Task<(List<RMAgentDto>, RMAgentUpgradeResult)> UpgradeCloudAgent([FromBody] RMAgentUpgradeDto dto)
        {
            var key = _keyValueDao.GetValueByKey("ENABLE_JPMC_FILE_SYSTEM_FEATURE");
            bool.TryParse(key?.Value, out bool result);
            if (result == false)
            {
                return (new List<RMAgentDto>(), RMAgentUpgradeResult.Failed);
            }
            return (await _agentMgmtService.UpgradeCloudAgentAsync(dto));
        }

        private Task<bool> SetDefaultCertificateAsync(Guid certificateId)
        {
            return _certificateService.SetAsDefaultCertificateAsync(certificateId);
        }
        private Task<IList<RMAgentDto>> GetAllAgentsInfoAsync()
        {
            return _agentMgmtService.GetAllAsync();
        }

        private async Task<bool> CheckSameAgentNameAsync([FromBody] RMAgentDto dto)
        {
            return (await GetAllAgentsInfoAsync()).Any(o => o.Id != dto.Id && o.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsValidAgentType([FromBody] RMAgentDto dto)
        {
            return dto.SourceType == SourceType.SharePoint || dto.SourceType == SourceType.FileSystem || (dto.SourceType == (SourceType.SharePoint | SourceType.FileSystem));
        }


        private static void ValidateReplicaCreateAgentRequest(RMAgentDto dto)
        {
            if (dto == null || dto.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Agent id is required for replica requests.");
            }

            if (string.IsNullOrWhiteSpace(dto.InstallationCode))
            {
                throw new InvalidOperationException("Installation code is required for replica requests.");
            }

            if (string.IsNullOrWhiteSpace(dto.AuthCode))
            {
                throw new InvalidOperationException("Auth code is required for replica requests.");
            }

            if (string.IsNullOrWhiteSpace(dto.ClientId))
            {
                throw new InvalidOperationException("Client id is required for replica requests.");
            }

            if (dto.CertificateId == Guid.Empty)
            {
                throw new InvalidOperationException("Certificate id is required for replica requests.");
            }
        }

        private static void ValidateReplicaCreateCertificateRequest(RMCertificateCreateRequest request)
        {
            if (request?.Certificate == null)
            {
                throw new InvalidOperationException("Certificate payload is required for replica requests.");
            }

            if (request.Certificate.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Certificate id is required for replica requests.");
            }

            if (string.IsNullOrWhiteSpace(request.Certificate.Name))
            {
                throw new InvalidOperationException("Certificate name is required for replica requests.");
            }

            if (string.IsNullOrWhiteSpace(request.Certificate.PWD))
            {
                throw new InvalidOperationException("Certificate password is required for replica requests.");
            }

            if (!request.Certificate.ValidFrom.HasValue)
            {
                throw new InvalidOperationException("Certificate valid-from is required for replica requests.");
            }

            if (!request.Certificate.ValidTo.HasValue)
            {
                throw new InvalidOperationException("Certificate valid-to is required for replica requests.");
            }
        }
    }
}
