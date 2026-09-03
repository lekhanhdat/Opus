using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMPublicAPI.OpusReport.SharePoint;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/OpusReport/[action]")]
    public class OpusReportApiController : RAWebApiBase
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(OpusReportApiController));

        private IRMSharePointSiteMetricsReportService _spReportExportService;
        private IRMSharePointSiteMetricsReportService SPReportExportService => PlatformWindsorManager.GetService(ref _spReportExportService);

        [HttpPost]
        public async Task<SPReportExportResponse> GenerateReport([FromBody] SPReportExportRequest request)
        {
            try
            {
                if (request.SiteCollectionUrls?.Count == 0 || string.IsNullOrWhiteSpace(request.DestinationLibraryUrl))
                {
                    return new SPReportExportResponse
                    {
                        Success = false,
                        Message = "Invalid request. Please provide at least one valid Site Collection URL and a Destination Library URL."
                    };
                }

                var invalidUrls = await SPReportExportService.SubmitSPReportExportJobAsync(request);

                if (!string.IsNullOrEmpty(invalidUrls))
                {
                    return new SPReportExportResponse
                    {
                        Success = false,
                        Message = $"One or more Site Collections were not found in the system. Invalid URLs: [{invalidUrls}]"
                    };
                }

                return new SPReportExportResponse
                {
                    Success = true,
                    Message = "Report export job has been submitted successfully."
                };
            }
            catch (Exception ex)
            {
                s_logger.Error($"Error submitting SP report export job. Error: {ex}");
                return new SPReportExportResponse
                {
                    Success = false,
                    Message = "An error occurred while submitting the report export job."
                };
            }
        }
    }
}