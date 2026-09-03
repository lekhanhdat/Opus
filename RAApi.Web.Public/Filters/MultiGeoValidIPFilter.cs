using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.Web.Extentions.Util;

namespace AvePoint.RA.Api.Web.Public.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MultiGeoValidIPFilter : Attribute, IAsyncActionFilter
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger s_logger = RALogger.GetInstance(typeof(MultiGeoValidIPFilter));

        private readonly IMultiGeoSettingService _multiGeoSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        private readonly IMultiGeoDataCenterService _multiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string clientIP = ClientRequestLocalValue.ClientIP;
            if (string.IsNullOrEmpty(clientIP))
            {
                clientIP = context.HttpContext.GetClientIP();
            }
            if (await _multiGeoSettingService.IsEnableMultiGeoFeature())
            {
                if (!(_multiGeoDataCenterService.IsMainDC()) && !(await _multiGeoSettingService.ValidateLoginIPAsync(clientIP, RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER])))
                {
                    s_logger.Warn($"The login IP is not allowed to access data center [{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER]}]. Reject the request.");
                    context.Result = new ObjectResult("Current Ip is blocked for this data center") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
            await next();
        }
    }
}
