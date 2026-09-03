using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Service.Services.Multi_Geo;
using CommonModel.DataModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/SignalRApi/[action]")]
    public class SignalRResourceApiController : RAWebApiBase
    {
        private ISignalRService _signalRService => PlatformWindsorManager.GetService<ISignalRService>();

        [HttpPost]
        public async Task<ICollection<AgentInformation>> GetAgents([FromBody] string tenantId)
        {
            return await _signalRService.GetAgentsByTypeAsync(tenantId, SourceType.FileSystem);
        }
    }
}
