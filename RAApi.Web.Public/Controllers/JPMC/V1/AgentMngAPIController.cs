using AvePoint.RA.Api.Web.Public.Common;
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

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V1
{
    [Route("api/v1/agent-mgmt/[action]")]
    public class AgentMngAPIController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(AgentMngAPIController));

        private IAgentMgmtPublicServices AgentMgmtPublicServices => PlatformWindsorManager.GetService<IAgentMgmtPublicServices>();

        [HttpPost]
        [MultiGeoValidIPFilter]
        public async Task<RAReturnMessage> QueryAgentsAsync([FromBody] AgentQueryParam param)
        {
            if (param == null)
            {
                return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = "Invalid parameters." };
            }
            param.DataCenterName = RMSSOHelper.CurrentDCName;
            return await AgentMgmtPublicServices.QueryAgentsAsync(param);
        }

        [HttpPost]
        public async Task<RAReturnMessage> CreateAgentAsync([FromBody] AgentCreateParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtCreateAgentAsync,
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
                    if(response?.MessageType == RAMessageType.Successful)
                    {
                        RMAgentDto agent = null;
                        try
                        {
                            agent = JsonConvert.DeserializeObject<RMAgentDto>(response.Extension);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"An error occurred while parsing the response from CreateAgentAsync, error : {e}");
                            agent = null;
                        }
                        if (agent != null)
                        {
                            logger.Info($"Successfully created agent with Id: {agent.Id}");
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
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateAgentAsync([FromBody] AgentUpdateParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtUpdateAgentAsync,
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
        }

        [HttpPost]
        public async Task<RAReturnMessage> DeleteAgentAsync([FromBody] AgentActionParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtDeleteAgentAsync,
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
        }

        [HttpPost]
        public async Task<RAReturnMessage> DisableAgentAsync([FromBody] AgentActionParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtDisableAgentAsync,
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
        }

        [HttpPost]
        public async Task<RAReturnMessage> EnableAgentAsync([FromBody] AgentActionParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtEnableAgentAsync,
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
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateAgentJobLimit([FromBody] AgentJobLimitParam param)
        {
            return await RouteMultiGeoApiActionAsync(param, RACommonUtility.MultiGeo.MultiGeoOperationType.JpmcAgentMgmtUpdateAgentJobLimit,
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
            
        }
    }
}