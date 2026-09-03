using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/ManualApprovalApi/[action]")]
    [ApiController]
    public class ManualApprovalResourceApiController : RAWebApiBase
    {
        private static IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private static IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        [HttpPost]
        public Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            var timeZoneId = Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-TIMEZONE");
            _ = bool.TryParse(Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"), out var isDaylight);
            var removeFilter = queryDefinition.Filters.FirstOrDefault(item => item.FilterOption == ManualApprovalFilterOptions.MyhubFolderNodeId);
            if (removeFilter != null)
            {
                queryDefinition.Filters.Remove(removeFilter);
            }
            return ManualApprovalService.UnderReviewFolderViewQueryAsync(queryDefinition, timeZoneId, isDaylight);
        }

        [HttpPost]
        public Task<ManualApprovalActionResult> Approve([FromBody] ManualApprovalActionParams approveParameters)
        {
            return ManualApprovalService.ApproveAsync(approveParameters, true);
        }

        [HttpPost]
        public Task<ManualApprovalActionResult> Reject([FromBody] ManualApprovalActionParams rejectParameters)
        {
            return ManualApprovalService.RejectAsync(rejectParameters, true);
        }

        [HttpPost]
        public MAReturnMessage RunFolderViewActionJob([FromBody] ManualApprovalActionParams folderViewParameters)
        {
            return ManualApprovalService.RunFolderViewActionJob(folderViewParameters);
        }

        [HttpPost]
        public MAReturnMessage RunBulkActionJob([FromBody] ManualApprovalJobParam param)
        {
            param.IsFromMyhub = true;
            return ManualApprovalService.RunBulkActionJob(param);
        }

        [HttpPost]
        public Task<ManualApprovalFilterFolderPathResult> QueryFolderPath([FromBody] ManualApprovalFolderPathQueryDefinition queryDefinition)
        {
            return ManualApprovalService.QueryFolderPathAsync(queryDefinition);
        }

        [HttpPost]
        public string GetRealTimeJobStatusInfo([FromBody] string jobId)
        {
            return JsonConvert.SerializeObject(ExplorerService.GetRealTimeJobStatusInfo(jobId));
        }

        [HttpPost]
        public async Task<RAReturnMessage> DoAction([FromBody] GlobalSearchActionDto actionDto)
        {
            if (ManualApprovalService.IsJpmc(actionDto.IsJpmc))
            {
                actionDto.SourceFlag = (int)SourceFlag.FileSystem;
            }
            RAReturnMessage message = await ExplorerService.ValidateParameterAsync(actionDto, ChangeTermPage.MyHub);
            if (message.MessageType == RAMessageType.Successful)
            {
                if (actionDto.IsRealTimeAction)
                {
                    message = ExplorerService.DoGlobalSearchRealTimeAction(actionDto);
                }
                else
                {
                    message = ExplorerService.StartGlobalSearchActionJob(actionDto);
                }
            }
            return message;
        }

        [HttpPost]
        public Task<ManualApprovalCommentInfos> GetApprovalCommentOption()
        {
            return ManualApprovalService.GetApprovalCommentOptionAsync();
        }

        [HttpPost]
        public Task<ManualApprovalSettings> GetSettingInfo()
        {
            return ManualApprovalService.GetManualApprovalSettingsAsync();
        }

        [HttpPost]
        public async Task<bool> SaveApprovalSettingInfo([FromBody] ManualApprovalSettingInfo manualApprovalSetting)
        {
            return await ManualApprovalService.SaveApprovalSettingAsync(manualApprovalSetting);
        }
    }
}
