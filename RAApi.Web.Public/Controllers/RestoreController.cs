using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using System.Linq;
using RestoreConversationType = AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object.RestoreConversationType;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/restore/[action]")]
    [ApiController]
    public class RestoreController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RestoreController));

        private IRestorePublicService RestorePublicService => PlatformWindsorManager.GetService<IRestorePublicService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMJobService RMJobService => PlatformWindsorManager.GetService<IRMJobService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        // private static IRestorePublicService RestorePublicService => PlatformWindsorManager.GetService<IRestorePublicService>();

        [HttpPost]
        public async Task<RestoreExecutionResponse> RestoreSiteCollection([FromBody] RestoreExecutionRequest request)
        {
            if (!await HasNewOpusLicenseAsync(nameof(RestoreSiteCollection), scope: request?.Scope))
            {
                return CreateFailureExecutionResponse(GetNoLicenseMessage());
            }

            var conflictValidationError = ValidateConflictResolution(request);
            if (conflictValidationError != null)
            {
                Logger.Warn($"[{TenantLocalValue.DisplayName}] access RestoreSiteCollection Public API with invalid conflict resolution. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}], ConflictResolution:[{request?.ConflictResolution}], AppsConflictResolution:[{request?.AppsConflictResolution}].");
                return conflictValidationError;
            }

            if (request != null)
            {
                request.IsPublicRestoreApiRequest = true;
            }


            Logger.Info($"[{TenantLocalValue.DisplayName}] access RestoreSiteCollection Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return await RestorePublicService.RestoreSiteCollectionAsync(request);
        }

        [HttpPost]
        public async Task<RestoreExecutionResponse> RestoreTeamsGroup([FromBody] RestoreExecutionRequest request)
        {
            if (!await HasNewOpusLicenseAsync(nameof(RestoreTeamsGroup), scope: request?.Scope))
            {
                return CreateFailureExecutionResponse(GetNoLicenseMessage());
            }

            var conflictValidationError = ValidateConflictResolution(request);
            if (conflictValidationError != null)
            {
                Logger.Warn($"[{TenantLocalValue.DisplayName}] access RestoreTeamsGroup Public API with invalid conflict resolution. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}], ConflictResolution:[{request?.ConflictResolution}], AppsConflictResolution:[{request?.AppsConflictResolution}].");
                return conflictValidationError;
            }

            var conversationValidationError = ValidateConversationRestoreSettings(request);
            if (conversationValidationError != null)
            {
                Logger.Warn($"[{TenantLocalValue.DisplayName}] access RestoreTeamsGroup Public API with invalid conversation settings. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}], IsSkipRestoreConversation:[{request?.IsSkipRestoreConversation}], RestoreConversationType:[{request?.RestoreConversationType}].");
                return conversationValidationError;
            }

            if (request != null)
            {
                request.IsPublicRestoreApiRequest = true;
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access RestoreTeamsGroup Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return await RestorePublicService.RestoreTeamsGroupAsync(request);
        }

        [HttpGet]
        public RestoreJobStatusResponse GetRestoreJobStatus([FromQuery] string jobId)
        {
            if (!HasNewOpusLicense(nameof(GetRestoreJobStatus), jobId: jobId))
            {
                return CreateFailureResponse<RestoreJobStatusResponse>(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access GetRestoreJobStatus Public API. Id:[{TenantLocalValue.LogonUserId}], JobId:[{jobId}].");
            return RestorePublicService.GetRestoreJobStatus(jobId);
        }

        [HttpPost]
        public async Task<RestoreCommonResponse> SetRestoreGracePeriodSiteCollection([FromBody] RestoreExecutionRequest request)
        {
            if (!await HasNewOpusLicenseAsync(nameof(SetRestoreGracePeriodSiteCollection), scope: request?.Scope))
            {
                return CreateFailureResponse<RestoreCommonResponse>(GetNoLicenseMessage());
            }

            var deleteArchivedDataDaysValidation = ValidateDeleteArchivedDataDaysAfterRestore(request);
            if (deleteArchivedDataDaysValidation != null)
            {
                return deleteArchivedDataDaysValidation;
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access SetRestoreGracePeriodSiteCollection Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return await RestorePublicService.SetRestoreGracePeriodSiteCollection(request);
        }

        [HttpPost]
        public async Task<RestoreCommonResponse> SetRestoreGracePeriodTeamsGroup([FromBody] RestoreExecutionRequest request)
        {
            if (!await HasNewOpusLicenseAsync(nameof(SetRestoreGracePeriodTeamsGroup), scope: request?.Scope))
            {
                return CreateFailureResponse<RestoreCommonResponse>(GetNoLicenseMessage());
            }

            var deleteArchivedDataDaysValidation = ValidateDeleteArchivedDataDaysAfterRestore(request);
            if (deleteArchivedDataDaysValidation != null)
            {
                return deleteArchivedDataDaysValidation;
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access SetRestoreGracePeriodTeamsGroup Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return await RestorePublicService.SetRestoreGracePeriodTeamsGroup(request);
        }

        [HttpGet]
        public async Task<RestoreArchivedDataCheckResponse> HasArchivedSiteCollectionData([FromQuery] string scope)
        {
            if (!await HasNewOpusLicenseAsync(nameof(HasArchivedSiteCollectionData), scope: scope))
            {
                return CreateFailureResponse<RestoreArchivedDataCheckResponse>(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access HasArchivedSiteCollectionData Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}].");
            return await RestorePublicService.HasArchivedSiteCollectionDataAsync(scope);
        }

        [HttpGet]
        public async Task<RestoreArchivedDataCheckResponse> HasArchivedTeamsGroupData([FromQuery] string scope)
        {
            if (!await HasNewOpusLicenseAsync(nameof(HasArchivedTeamsGroupData), scope: scope))
            {
                return CreateFailureResponse<RestoreArchivedDataCheckResponse>(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access HasArchivedTeamsGroupData Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}].");
            return await RestorePublicService.HasArchivedTeamsGroupDataAsync(scope);
        }

        private Task<bool> HasNewOpusLicenseAsync(string apiName, string scope = null, string jobId = null)
        {
            var hasOpusILLicense = LicenseHelperService.HasOpusILLicense;
            var hasOpusSOLicense = LicenseHelperService.HasOpusSOLicense;
            Logger.Info($"License intercept: HasOpusILLicense=[{hasOpusILLicense}], HasOpusSOLicense=[{hasOpusSOLicense}]");
            if (hasOpusILLicense || hasOpusSOLicense)
            {
                return Task.FromResult(true);
            }

            Logger.Warn($"[{TenantLocalValue.DisplayName}] access {apiName} Public API without Opus subscription. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}], JobId:[{jobId}].");
            return Task.FromResult(false);
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

        private bool TenantJobReachedLimit()
        {
            int maxJobCount = RMJobService.GetTenantMainJobCount();
            var jobCount = JobMonitorService.GetRunningJobsCount(JobType.All);
            var discoveryJobCount = JobMonitorService.GetRunningJobsCount(JobType.DiscoveryJob);
            var highJobCount = JobMonitorService.GetRunningJobsCount(RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToList());

            return (jobCount - discoveryJobCount - highJobCount) >= maxJobCount;
        }

        private static RestoreExecutionResponse ValidateConflictResolution(RestoreExecutionRequest request)
        {
            if (request == null)
            {
                return null;
            }

            if (!IsSupportedConflictResolution(request.ConflictResolution) || !IsSupportedConflictResolution(request.AppsConflictResolution))
            {
                return CreateFailureExecutionResponse(I18NEntity.GetString("RM_RESTORE_PUB_InvalidConflictResolution"));
            }

            return null;
        }

        private static bool IsSupportedConflictResolution(int conflictResolution)
        {
            return System.Enum.IsDefined(typeof(RestoreOption), conflictResolution);
        }

        private static RestoreExecutionResponse ValidateConversationRestoreSettings(RestoreExecutionRequest request)
        {
            if (request == null)
            {
                return null;
            }

            if (!System.Enum.IsDefined(typeof(RestoreConversationType), request.RestoreConversationType))
            {
                return CreateFailureExecutionResponse("RestoreConversationType must be -1 (Skip), 0 (Html) or 1 (Original).");
            }

            return null;
        }

        private static RestoreCommonResponse ValidateDeleteArchivedDataDaysAfterRestore(RestoreExecutionRequest request)
        {
            if (request?.DeleteArchivedDataDaysAfterRestore == null)
            {
                return CreateFailureResponse<RestoreCommonResponse>("DeleteArchivedDataDaysAfterRestore is required.");
            }

            if (request.DeleteArchivedDataDaysAfterRestore < 0)
            {
                return CreateFailureResponse<RestoreCommonResponse>("DeleteArchivedDataDaysAfterRestore cannot be less than 0.");
            }

            return null;
        }

        private static RestoreExecutionResponse CreateFailureExecutionResponse(string message)
        {
            return new RestoreExecutionResponse
            {
                Success = false,
                Message = message
            };
        }

        private static TResponse CreateFailureResponse<TResponse>(string message) where TResponse : RestoreCommonResponse, new()
        {
            return new TResponse
            {
                Success = false,
                Message = message
            };
        }

        private static string GetNoLicenseMessage()
        {
            return I18NEntity.GetString("RM_SF_APP_Error_HasNoLicense_Desc");
        }
    }
}
