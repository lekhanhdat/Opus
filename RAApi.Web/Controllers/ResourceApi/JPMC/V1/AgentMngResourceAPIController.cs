using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
namespace AvePoint.RA.Api.Web.Controllers.ResourceApi.JPMC.V1
{
    [Route("api/v1/agent-mgmt/[action]")]
    public class AgentMngResourceAPIController : RAWebApiBase
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(AgentMngResourceAPIController));

        private IAgentMgmtPublicServices AgentMgmtPublicServices
            => PlatformWindsorManager.GetService<IAgentMgmtPublicServices>();

        [HttpPost]
        public async Task<RAReturnMessage> CreateAgentAsync([FromBody] AgentCreateParam param)
        {
            (bool valid, RAReturnMessage value) = ValidateCreateAgentParameters(param);
            if (!valid)
            {
                return value;
            }
            return await AgentMgmtPublicServices.CreateAgentWithIdAsync(param);
        }

        private (bool valid, RAReturnMessage value) ValidateCreateAgentParameters(AgentCreateParam param)
        {
            if (param == null || param.Id == Guid.Empty)
            {
                return (valid: false, value: FailedResult("Agent id is required for replica requests."));
            }

            if (string.IsNullOrWhiteSpace(param.InstallationCode))
            {
                return (valid: false, value: FailedResult("Installation code is required for replica requests."));
            }

            if (string.IsNullOrWhiteSpace(param.AuthCode))
            {
                return (valid: false, value: FailedResult("Auth code is required for replica requests."));
            }

            if (string.IsNullOrWhiteSpace(param.ClientId))
            {
                return (valid: false, value: FailedResult("Client id is required for replica requests."));
            }

            if (param.CertificateId == Guid.Empty)
            {
                return (valid: false, value: FailedResult("Certificate id is required for replica requests."));
            }

            if (string.IsNullOrWhiteSpace(param.Name))
                return (valid: false, value: FailedResult("Agent Name is required."));

            return (valid: true, value: null);
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateAgentAsync([FromBody] AgentUpdateParam param)
        {
            if (param == null)
                return FailedResult("Invalid parameters.");

            if (param.Id == Guid.Empty)
                return FailedResult("Agent Id is required.");

            if (string.IsNullOrWhiteSpace(param.Name))
                return FailedResult("Agent Name is required.");

            return await AgentMgmtPublicServices.UpdateAgentAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> DeleteAgentAsync([FromBody] AgentActionParam param)
        {
            if (param == null || param.Id == Guid.Empty)
                return FailedResult("Agent Id is required.");

            return await AgentMgmtPublicServices.DeleteAgentAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> DisableAgentAsync([FromBody] AgentActionParam param)
        {
            if (param == null || param.Id == Guid.Empty)
                return FailedResult("Agent Id is required.");

            return await AgentMgmtPublicServices.DisableAgentAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> EnableAgentAsync([FromBody] AgentActionParam param)
        {
            if (param == null || param.Id == Guid.Empty)
                return FailedResult("Agent Id is required.");

            return await AgentMgmtPublicServices.EnableAgentAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateAgentJobLimit([FromBody] AgentJobLimitParam param)
        {
            if (param == null || param.JobLimit <= 0)
                return FailedResult("Valid JobLimit is required.");

            return await AgentMgmtPublicServices.UpdateAgentJobLimitAsync(param);
        }

        private RAReturnMessage FailedResult(string errorMessage)
            => new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = errorMessage };
    }
}
