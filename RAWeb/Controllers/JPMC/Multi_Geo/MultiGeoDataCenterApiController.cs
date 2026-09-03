using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.Multi_Geo;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.JPMC.Multi_Geo
{
    [ValidSupportMultiGeoFeature]
    public class MultiGEODataCenterApiController : BaseApiController
    {
        private readonly IMultiGeoDataCenterService MultiGEODataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        [HttpGet]
        public async Task<MultiGeoDCInfo> GetMultiGEODCInformation()
        {
            return await MultiGEODataCenterService.GetMultiGeoDCInformation();
        }
    }
}
