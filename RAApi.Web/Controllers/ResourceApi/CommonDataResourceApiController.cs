using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/CommonDataResourceApi/[action]")]
    [ApiController]
    public class CommonDataResourceApiController : RAWebApiBase
    {
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpPost]
        public string RunSyncCommonDataOtherDCJob([FromBody] SyncCommonDataInforDto syncCommonDataInfor)
        {
            return MultiGeoDataCenterService.RunOtherDCSyncCommonDataJob(syncCommonDataInfor);
        }
    }
}
