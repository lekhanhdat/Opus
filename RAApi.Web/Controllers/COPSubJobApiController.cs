using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.RMWeb;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/CopSubjobApi")]
    [ApiController]
    public class COPSubJobApiController : RAWebApiBase
    {
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        [HttpPost("GetSubJobsAsync")]
        public async Task<List<SubJobsResult>> GetSubJobsAsync([FromBody] COPSubJobRequest request)
        {
            return await JobMonitorService.GetSubJobsAsync(request);
        }
    }
}
