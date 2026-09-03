using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.Object;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Api.Web.Public.Common.Response
{
    public static class ApiResponseExtensions
    {
        public static IActionResult OkApi(this ControllerBase controller, object data, string message = "OK")
        {
            return controller.Ok(StandardApiResponse.Success(data, message));
        }

        public static IActionResult BadRequestApi(this ControllerBase controller, string message, object data = null)
        {
            return controller.BadRequest(StandardApiResponse.Error(ApiResponseStatusCode.BadRequest, message, data));
        }

        public static IActionResult NotFoundApi(this ControllerBase controller, string message, object data = null)
        {
            return controller.NotFound(StandardApiResponse.Error(404, message, data));
        }

        public static IActionResult ForbiddenApi(this ControllerBase controller, string message, object data = null)
        {
            return controller.StatusCode(403, StandardApiResponse.Error(403, message, data));
        }

        public static IActionResult InternalServerErrorApi(this ControllerBase controller, string message, object data = null)
        {
            return controller.StatusCode(500, StandardApiResponse.Error(ApiResponseStatusCode.InternalServerError, message, data));
        }

        public static IActionResult FromRestoreReturn(this ControllerBase controller, RestoreCommonResponse result)
        {
            if (result == null)
            {
                return controller.InternalServerErrorApi("Internal operation result is null.");
            }
            if (result.Success)
            {
                switch (result)
                {
                    case RestoreExecutionResponse execution:
                        return controller.OkApi(execution.JobId);
                    case RestoreArchivedDataCheckResponse archivedDataCheck:
                        return controller.OkApi(new { archivedDataCheck.HasArchivedData, archivedDataCheck.Scope });
                    case RestoreJobStatusResponse jobStatus:
                        return controller.OkApi(jobStatus.Job);
                    default:
                        return controller.OkApi(result);
                }
            }
            return ConvertRestoreErrorStatusToAction(controller, result);
        }

        private static IActionResult ConvertRestoreErrorStatusToAction(ControllerBase controller, RestoreCommonResponse result) => result.ErrorType switch
        {
            RestoreErrorType.JobNotFound or RestoreErrorType.ScopeNotFound or RestoreErrorType.UserNotFound => controller.NotFoundApi(result.Message),
            RestoreErrorType.JobIdIsRequired or RestoreErrorType.ScopeIsRequired => controller.BadRequestApi(result.Message),
            RestoreErrorType.UnknowError => controller.Ok(StandardApiResponse.Error(ApiResponseStatusCode.SomeDataOperationFailed, result.Message)),
            RestoreErrorType.DoNotHavePermission => controller.ForbiddenApi(result.Message),
            _ => controller.InternalServerErrorApi("Internal server error.")
        };

        public static IActionResult FromReturnMessage(this ControllerBase controller, RAReturnMessage result, string successMessage = "OK")
        {
            if (result == null)
            {
                return controller.InternalServerErrorApi("Internal operation result is null.");
            }

            if (result.MessageType == RAMessageType.Successful)
            {
                return controller.OkApi(DeserializeExtension(result.Extension), successMessage);
            }

            if (result.MessageType == RAMessageType.Exception || result.MessageType == RAMessageType.FailedWithEx)
            {
                return controller.InternalServerErrorApi(string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Internal server error." : result.ErrorMessage, result);
            }

            var errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Operation failed." : result.ErrorMessage;

            if (result.FaildType == RAFailedType.ParameterIsIncorrect)
            {
                return controller.BadRequestApi(errorMessage, result);
            }

            if (IsNotFoundError(errorMessage))
            {
                return controller.NotFoundApi(errorMessage, result);
            }

            if (result.MessageType == RAMessageType.Failed)
            {
                return controller.Ok(StandardApiResponse.Error(ApiResponseStatusCode.SomeDataOperationFailed, errorMessage));
            }

            return controller.BadRequestApi(errorMessage, result);
        }

        public static IActionResult FromBatchOperation(this ControllerBase controller, BatchOperationResponse result, string successMessage, string partialFailureMessage)
        {
            if (result == null)
            {
                return controller.InternalServerErrorApi("Internal batch operation result is null.");
            }

            if (result.FailedCount > 0)
            {
                return controller.Ok(StandardApiResponse.Error(ApiResponseStatusCode.SomeDataOperationFailed, partialFailureMessage, result));
            }

            return controller.OkApi(result, successMessage);
        }

        private static bool IsNotFoundError(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            var message = errorMessage.ToLowerInvariant();
            return message.Contains("not found")
                   || message.Contains("cannot find")
                   || message.Contains("does not exist");
        }

        private static object DeserializeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject(extension);
            }
            catch (JsonException)
            {
                return extension;
            }
        }
    }
}

