using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/MultiGeoSettingApi/[action]")]
    [ApiController]
    public class MultiGeoSettingResourceApiController : RAWebApiBase
    {
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        [HttpPost]
        public Task<RAReturnMessage> SaveMultiGeoSettings([FromBody] List<MultiGeoSettingInfoDto> settings)
        {
            return MultiGeoSettingService.SaveMultiGeoSettings(settings, true);
        }
    }
}