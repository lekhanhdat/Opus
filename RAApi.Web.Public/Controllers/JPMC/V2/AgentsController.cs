using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Common.Requests;
using AvePoint.RA.Api.Web.Public.Common.Response;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("agents")]
    public class AgentsController : RAWebApiBase
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(AgentsController));

        private IAgentMgmtPublicServices AgentMgmtPublicServices => PlatformWindsorManager.GetService<IAgentMgmtPublicServices>();

        [HttpGet]
        [MultiGeoValidIPFilter]
        public async Task<IActionResult> GetAgents(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string searchValue = null,
            [FromQuery] string sortBy = null,
            [FromQuery] bool isAscending = true)
        {
            var result = await AgentMgmtPublicServices.QueryAgentsAsync(new AgentQueryParam
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                SortBy = sortBy,
                IsAscending = isAscending,
                DataCenterName = RMSSOHelper.CurrentDCName
            });

            return this.FromReturnMessage(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAgent([FromBody] CreateAgentRequest param)
        {
            var requestParam = param?.ToContract();
            var result = await RouteMultiGeoApiActionAsync(requestParam, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtCreateAgentAsync,
                async request =>
                {
                    if (request == null)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Invalid parameters." };
                    }
                    if (string.IsNullOrWhiteSpace(request.Name))
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Name is required." };
                    }

                    return await AgentMgmtPublicServices.CreateAgentAsync(request);
                },
                (request, response) =>
                {
                    if (response?.MessageType == RAMessageType.Successful)
                    {
                        RMAgentDto agent = null;
                        try
                        {
                            agent = JsonConvert.DeserializeObject<RMAgentDto>(response.Extension);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"An error occurred while parsing the response from CreateAgent, error : {e}");
                        }

                        if (agent != null)
                        {
                            request.Id = agent.Id;
                            request.AuthCode = agent.AuthCode;
                            request.ClientId = agent.ClientId;
                            request.CertificateId = agent.CertificateId;
                            request.InstallationCode = agent.InstallationCode;
                        }
                    }

                    return Task.CompletedTask;
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });

            return this.FromReturnMessage(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateAgent(Guid id, [FromBody] UpdateAgentRequest param)
        {
            var requestParam = param?.ToContract(id) ?? new AgentUpdateParam { Id = id };

            var result = await RouteMultiGeoApiActionAsync(requestParam, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtUpdateAgentAsync,
                async request =>
                {
                    if (request == null)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Invalid parameters." };
                    }
                    if (request.Id == Guid.Empty)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Id is required." };
                    }
                    if (string.IsNullOrWhiteSpace(request.Name))
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Name is required." };
                    }

                    return await AgentMgmtPublicServices.UpdateAgentAsync(request);
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });

            return this.FromReturnMessage(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAgent(Guid id)
        {
            var param = new AgentActionParam { Id = id };
            var result = await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtDeleteAgentAsync,
                async request =>
                {
                    if (request == null || request.Id == Guid.Empty)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Id is required." };
                    }
                    return await AgentMgmtPublicServices.DeleteAgentAsync(request);
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });

            return this.FromReturnMessage(result);
        }

        [HttpPatch("{id:guid}/disable")]
        public async Task<IActionResult> DisableAgent(Guid id)
        {
            var param = new AgentActionParam { Id = id };
            var result = await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtDisableAgentAsync,
                async request =>
                {
                    if (request == null || request.Id == Guid.Empty)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Id is required." };
                    }
                    return await AgentMgmtPublicServices.DisableAgentAsync(request);
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });
            return this.FromReturnMessage(result);
        }

        [HttpPatch("{id:guid}/enable")]
        public async Task<IActionResult> EnableAgent(Guid id)
        {
            var param = new AgentActionParam { Id = id };
            var result = await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtEnableAgentAsync,
                async request =>
                {
                    if (request == null || request.Id == Guid.Empty)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Agent Id is required." };
                    }
                    return await AgentMgmtPublicServices.EnableAgentAsync(request);
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });
            return this.FromReturnMessage(result);
        }

        [HttpPatch("job-limit")]
        public async Task<IActionResult> UpdateAgentJobLimit([FromBody] UpdateAgentJobLimitRequest param)
        {
            var requestParam = param?.ToContract();
            var result = await RouteMultiGeoApiActionAsync(requestParam, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtUpdateAgentJobLimit,
                async request =>
                {
                    if (request == null || request.JobLimit <= 0)
                    {
                        return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Valid JobLimit is required." };
                    }
                    return await AgentMgmtPublicServices.UpdateAgentJobLimitAsync(request);
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                });
            return this.FromReturnMessage(result);
        }
    }
}

