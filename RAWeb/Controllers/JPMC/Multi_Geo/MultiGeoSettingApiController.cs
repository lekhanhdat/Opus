using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.Multi_Geo;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.JPMC.Multi_GEO
{
    [ValidSupportMultiGeoFeature]
    [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin, DB.SecurityTrimming.Model.PermissionJoinType.Any, preferred: false)]
    public class MultiGEOSettingApiController : BaseApiController
    {
        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        [HttpGet]
        public async Task<List<MultiGeoSettingInfoDto>> GetAllMultiGeoSetting()
        {
            return await MultiGEOSettingService.GetAllMultiGeoSetting();
        }

        [HttpPost]
        public async Task<RAReturnMessage> SaveMultiGeoSettings([FromBody] List<MultiGeoSettingInfoDto> settings)
        {
            return await RouteMultiGeoApiActionAsync(
                settings,
                MultiGeoOperationType.SaveMultiGeoSettings,
                async request =>
                {
                    RAReturnMessage result = await MultiGEOSettingService.SaveMultiGeoSettings(request);
                    return result;
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }
    }
}
