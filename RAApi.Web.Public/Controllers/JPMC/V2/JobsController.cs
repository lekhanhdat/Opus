using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.Api.Services.Services;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Common.Response;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers.JPMC.V2
{
    [Route("jobs")]
    [MultiGeoValidIPFilter]
    public class JobsController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RestoreController));
        private IRetriveDataServices RetriveDataServices => PlatformWindsorManager.GetService<IRetriveDataServices>();
        private ITriggerJobServices TriggerJobServices => PlatformWindsorManager.GetService<ITriggerJobServices>();
        private IRestorePublicService RestorePublicService => PlatformWindsorManager.GetService<IRestorePublicService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        [HttpGet("{jobId}/status")]
        public IActionResult GetRestoreJobStatus(string jobId)
        {
            if (!HasNewOpusLicense(nameof(GetRestoreJobStatus), jobId: jobId))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access GetRestoreJobStatus Public API. Id:[{TenantLocalValue.LogonUserId}], JobId:[{jobId}].");
            var result = RestorePublicService.GetRestoreJobStatus(jobId);
            return this.FromRestoreReturn(result);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetJobReport(
            [FromQuery] long? startTime,
            [FromQuery] long? endTime,
            [FromQuery] JobType? jobType,
            [FromQuery] JobStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string partitionKey = null)
        {
            try
            {
                if (!startTime.HasValue)
                {
                    return this.BadRequestApi("StartTime is required.");
                }

                var report = await RetriveDataServices.GetJobReportAsync(new JobReportParam
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    JobType = jobType,
                    Status = status,
                    Page = page,
                    PageSize = pageSize,
                    PartitionKey = partitionKey
                });

                if (report == null)
                {
                    return this.NotFoundApi("Job report not found.");
                }

                return this.OkApi(report);
            }
            catch (System.Exception)
            {
                return this.InternalServerErrorApi("An error occurred while getting the job report");
            }
        }

        [HttpGet("details")]
        public async Task<IActionResult> GetJobDetails(
            [FromQuery] string jobId,
            [FromQuery] int jobType,
            [FromQuery] string searchValue,
            [FromQuery] string[] searcheKeys,
            [FromQuery] int pageSize,
            [FromQuery] int currentPage,
            [FromQuery] JobDetailsStatus[] statusFilters,
            [FromQuery] int[] entityTypeFilters,
            [FromQuery] ActionTab[] actionTabFilters,
            [FromQuery] string[] archiverActionFilters,
            [FromQuery] JobStatus[] subJobStatusFilters)
        {
            try
            {
                var queryModel = new JMDetailsQuery
                {
                    JobID = jobId,
                    JobType = jobType,
                    SearchValue = searchValue,
                    SearcheKeys = searcheKeys,
                    PageSize = pageSize,
                    CurrentPage = currentPage,
                    StatusFilters = statusFilters,
                    EntityTypeFilters = entityTypeFilters,
                    ActionTabFilters = actionTabFilters,
                    ArchiverActionFilters = archiverActionFilters,
                    SubJobStatusFilters = subJobStatusFilters
                };

                var (isValid, errorMessage) = IsValidJobDetailsQuery(queryModel);
                if (!isValid)
                {
                    return this.BadRequestApi(errorMessage);
                }

                var details = await RetriveDataServices.GetJobDetails(queryModel);
                if (details == null)
                {
                    return this.NotFoundApi("Job details not found.");
                }

                return this.OkApi(details);
            }
            catch (System.Exception)
            {
                return this.InternalServerErrorApi("An error occurred while getting the job details");
            }
        }

        [HttpPost("batch-stop")]
        public async Task<IActionResult> StopJobs([FromBody] List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return this.BadRequestApi("No job IDs provided.");
            }

            return this.FromReturnMessage(await TriggerJobServices.StopJobsAsync(ids));
        }

        private static (bool IsValid, string ErrorMessage) IsValidJobDetailsQuery(JMDetailsQuery queryModel)
        {
            if (queryModel == null)
            {
                return (false, "Query model is required.");
            }
            if (queryModel.PageSize <= 0 || queryModel.CurrentPage <= 0)
            {
                return (false, "PageSize and CurrentPage must be greater than 0.");
            }
            if (string.IsNullOrEmpty(queryModel.JobID))
            {
                return (false, "JobID is required.");
            }
            if (queryModel.JobType <= 0)
            {
                return (false, "JobType must be greater than 0.");
            }

            return (true, string.Empty);
        }

        private bool HasNewOpusLicense(string apiName, string scope = null, string jobId = null)
        {
            var hasOpusILLicense = LicenseHelperService.HasOpusILLicense;
            var hasOpusSOLicense = LicenseHelperService.HasOpusSOLicense;
            Logger.Info($"License intercept: HasOpusILLicense=[{hasOpusILLicense}], HasOpusSOLicense=[{hasOpusSOLicense}]");
            if (hasOpusILLicense || hasOpusSOLicense)
            {
                return true;
            }

            Logger.Warn($"[{TenantLocalValue.DisplayName}] access {apiName} Public API without Opus subscription. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}], JobId:[{jobId}].");
            return false;
        }

        private static string GetNoLicenseMessage()
        {
            return I18NEntity.GetString("RM_SF_APP_Error_HasNoLicense_Desc");
        }
    }
}

