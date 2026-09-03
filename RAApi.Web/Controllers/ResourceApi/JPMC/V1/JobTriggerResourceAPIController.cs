using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi.JPMC.V1
{
    [Route("api/v1/job-trigger/[action]")]
    public class JobTriggerResourceAPIController : RAWebApiBase
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(JobTriggerResourceAPIController));

        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();

        [HttpPost]
        public async Task<RAReturnMessage> MainPauseDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            return await RouteMultiGeoApiActionAsync(req, RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobPauseDisposalProcess,
                TriggerJobServices.PauseDisposalProcess,
                errorMessage => new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = errorMessage });
        }

        [HttpPost]
        public async Task<RAReturnMessage> OtherPauseDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            return await TriggerJobServices.PauseDisposalProcess(req);
        }

        [HttpPost]
        public async Task<RAReturnMessage> MainResumeDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            return await RouteMultiGeoApiActionAsync(req, RACommonUtility.MultiGeo.MultiGeoOperationType.OtherDCJpmcTriggerJobResumeDisposalProcess,
                TriggerJobServices.ResumeDisposalProcess,
                errorMessage => new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = errorMessage });
        }

        [HttpPost]
        public async Task<RAReturnMessage> OtherResumeDisposalProcess([FromBody] PauseOrResumeReq req)
        {
            return await TriggerJobServices.ResumeDisposalProcess(req);
        }
    }
}
