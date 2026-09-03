using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Common.Response;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers.V2
{
    [Route("restore")]
    [ApiController]
    public class RestoreController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RestoreController));

        private IRestorePublicService RestorePublicService => PlatformWindsorManager.GetService<IRestorePublicService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        [HttpPost("site-collections")]
        public async Task<IActionResult> RestoreSiteCollection([FromBody] RestoreExecutionRequest request)
        {
            if(!HasNewOpusLicense(nameof(RestoreSiteCollection), request?.Scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            if (!ValidateConflictResolution(request))
            {
                Logger.Warn($"[{TenantLocalValue.DisplayName}] access RestoreSiteCollection Public API with invalid conflict resolution. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}], ConflictResolution:[{request?.ConflictResolution}], AppsConflictResolution:[{request?.AppsConflictResolution}].");
                return this.BadRequestApi(GetErrorConflictResolution());
            }

            if(request != null)
            {
                request.IsPublicRestoreApiRequest = true;
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access RestoreSiteCollection Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return this.FromRestoreReturn(await RestorePublicService.RestoreSiteCollectionAsync(request));
            
        }

        [HttpPost("teams")]
        public async Task<IActionResult> RestoreTeamsGroup([FromBody] RestoreExecutionRequest request)
        {
            if(!HasNewOpusLicense(nameof(RestoreTeamsGroup), request?.Scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            if (!ValidateConflictResolution(request))
            {
                Logger.Warn($"[{TenantLocalValue.DisplayName}] access RestoreTeamsGroup Public API with invalid conflict resolution. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}], ConflictResolution:[{request?.ConflictResolution}], AppsConflictResolution:[{request?.AppsConflictResolution}].");
                return this.BadRequestApi(GetErrorConflictResolution());
            }

            if(request != null)
            {
                request.IsPublicRestoreApiRequest = true;
            }
            Logger.Info($"[{TenantLocalValue.DisplayName}] access RestoreTeamsGroup Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return this.FromRestoreReturn(await RestorePublicService.RestoreTeamsGroupAsync(request));
        }


        [HttpPatch("site-collections/grace-period")]
        public async Task<IActionResult> SetRestoreGracePeriodSiteCollection([FromBody] RestoreExecutionRequest request)
        {
            if (!HasNewOpusLicense(nameof(SetRestoreGracePeriodSiteCollection), scope: request?.Scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            var deleteArchivedDataDaysValidation = ValidateDeleteArchivedDataDaysAfterRestore(request);
            if (!string.IsNullOrEmpty(deleteArchivedDataDaysValidation))
            {
                return this.BadRequestApi(deleteArchivedDataDaysValidation);
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access SetRestoreGracePeriodSiteCollection Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return this.FromRestoreReturn(await RestorePublicService.SetRestoreGracePeriodSiteCollection(request));
        }

        [HttpPatch("teams/grace-period")]
        public async Task<IActionResult> SetRestoreGracePeriodTeamsGroup([FromBody] RestoreExecutionRequest request)
        {
            if (!HasNewOpusLicense(nameof(SetRestoreGracePeriodTeamsGroup), scope: request?.Scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            var deleteArchivedDataDaysValidation = ValidateDeleteArchivedDataDaysAfterRestore(request);
            if (!string.IsNullOrEmpty(deleteArchivedDataDaysValidation))
            {
                return this.BadRequestApi(deleteArchivedDataDaysValidation);
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access SetRestoreGracePeriodTeamsGroup Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{request?.Scope}].");
            return this.FromRestoreReturn(await RestorePublicService.SetRestoreGracePeriodTeamsGroup(request));
        }

        [HttpGet("site-collections")]
        public async Task<IActionResult> HasArchivedSiteCollectionData([FromQuery] string scope)
        {
            if (!HasNewOpusLicense(nameof(HasArchivedSiteCollectionData), scope: scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access HasArchivedSiteCollectionData Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}].");
            return this.FromRestoreReturn(await RestorePublicService.HasArchivedSiteCollectionDataAsync(scope));
        }

        [HttpGet("teams")]
        public async Task<IActionResult> HasArchivedTeamsGroupData([FromQuery] string scope)
        {
            if (!HasNewOpusLicense(nameof(HasArchivedTeamsGroupData), scope: scope))
            {
                return this.ForbiddenApi(GetNoLicenseMessage());
            }

            Logger.Info($"[{TenantLocalValue.DisplayName}] access HasArchivedTeamsGroupData Public API. Id:[{TenantLocalValue.LogonUserId}], Scope:[{scope}].");
            return this.FromRestoreReturn(await RestorePublicService.HasArchivedTeamsGroupDataAsync(scope));
        }

        private static string ValidateDeleteArchivedDataDaysAfterRestore(RestoreExecutionRequest request)
        {
            if (request?.DeleteArchivedDataDaysAfterRestore == null)
            {
                return "DeleteArchivedDataDaysAfterRestore is required.";
            }

            if (request.DeleteArchivedDataDaysAfterRestore < 0)
            {
                return "DeleteArchivedDataDaysAfterRestore cannot be less than 0.";
            }

            return string.Empty;
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

        private static string GetErrorConflictResolution()
        {
            return I18NEntity.GetString("RM_RESTORE_PUB_InvalidConflictResolution");
        }

        private static bool ValidateConflictResolution(RestoreExecutionRequest request)
        {
            if (request == null)
            {
                return true;
            }

            if (!IsSupportedConflictResolution(request.ConflictResolution) || !IsSupportedConflictResolution(request.AppsConflictResolution))
            {
                return false;
            }

            return true;
        }

        private static bool IsSupportedConflictResolution(int conflictResolution)
        {
            return System.Enum.IsDefined(typeof(RestoreOption), conflictResolution);
        }
    }
}
