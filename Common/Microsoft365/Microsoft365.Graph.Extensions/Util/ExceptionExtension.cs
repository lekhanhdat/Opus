/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */

namespace Microsoft365.Graph.Extensions;

public static class ExceptionExtension
{

    public static bool IsTooManyRequestsException(this Exception? ex) => ex switch
    {
        ODataError ode => IsTooManyRequestsException(ode),
        ApiException apie => IsTooManyRequestsException(apie),
        AggregateException ae => IsTooManyRequestsException(ae),
        _ => false,
    };

    /// <summary>
    /// https://github.com/microsoft/kiota-http-dotnet/blob/main/CHANGELOG.md#138---2024-03-25
    /// </summary>
    /// <param name="ae"></param>
    /// <returns></returns>
    private static bool IsTooManyRequestsException(this AggregateException ae) => IsTooManyRequestsException(ae.InnerException);

    private static bool IsTooManyRequestsException(this ODataError ode) => ErrorConstants.Codes.ThrottlingErrorCodes.Contains(ode.Error?.Code ?? String.Empty);

    private static bool IsTooManyRequestsException(this ApiException apie)
    {
        if (apie is ODataError ode)
        {
            return IsTooManyRequestsException(ode);
        }
        return apie.ResponseStatusCode.IsTooManyRequests();
    }

    private static bool IsTooManyRequests(this int statusCode)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.ServiceUnavailable => true,
            (int)HttpStatusCode.GatewayTimeout => true,
            (int)HttpStatusCode.TooManyRequests => true,
            _ => false,
        };
    }

    public static bool IsLocked(this ApiException se) => se is not null && se.ResponseStatusCode == (int)HttpStatusCode.Locked;

    public static bool IsDriveNotFound(this ApiException se) => se is not null &&
        (se.ResponseStatusCode == (int)HttpStatusCode.NotFound
            || (se.ResponseStatusCode == (int)HttpStatusCode.BadRequest
                    && se.Message.Contains("Unable to retrieve user's mysite URL.", StringComparison.OrdinalIgnoreCase)));

    public static bool IsTenantDontHaveSPOLicenseException(this ApiException apiException)
    {
        return apiException is ODataError oDataError && (oDataError?.Error?.Message?.Contains("Tenant does not have a SPO license.", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public static bool IsUnableToRetrieveTenantServiceInfoException(this ApiException apiException)
    {
        return apiException is ODataError oDataError && (oDataError?.Error?.Message?.Contains("Unable to retrieve tenant service info", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public static bool IsItemNotFound(this Exception ex)=> Extension.IsItemNotFound(ex);

    public static bool IsColumnExists(this ODataError ex)
    {
        return ex.ResponseStatusCode == (int)HttpStatusCode.BadRequest && "columnExists".EqualsIgnoreCase(ex.InnerErrorCode());
    }

    public static string? InnerErrorCode(this ODataError ex)
    {
        if (ex.Error?.InnerError?.AdditionalData.TryGetValue("code", out var code) ?? false)
        {
            return code.ToString();
        }
        return null;
    }

    //"Insufficient privileges to complete the operation."
    public static bool IsInsufficientPrivilegesException(this ODataError ex) => (ex.Error?.Code?.Equals(ErrorConstants.Codes.AuthorizationRequestDenied, StringComparison.OrdinalIgnoreCase) ?? false)
            && ex.Message.Contains("Insufficient privileges", StringComparison.OrdinalIgnoreCase);

    //"Forbidden"
    //"Missing role permissions on the request. API requires one of 'Chat.ReadBasic.All, Chat.Read.All, Chat.ReadWrite.All'. Roles on the request 'User.Read.All'."
    public static bool IsMissingRolePermissionsException(this ODataError ex) => ex.Message.Contains("Missing role permissions on the request", StringComparison.OrdinalIgnoreCase);

    //"Missing scope permissions on the request. API requires one of 'Team.ReadBasic.All, ..."
    public static bool IsMissingScopePermissionsException(this ODataError ex) => ex.Message.Contains("Missing scope permissions on the request", StringComparison.OrdinalIgnoreCase);

    public static bool IsMissingPermissionsException(this ODataError ex) => ex.IsMissingRolePermissionsException()
        || ex.IsMissingScopePermissionsException()
        || ex.IsInsufficientPrivilegesException();

    public static bool IsResyncRequired(this Exception ex)
    {
        //https://learn.microsoft.com/en-us/graph/delta-query-overview#synchronization-reset
        //Assume HTTP 410 Gone = sync state reset. Error codes may not same for all services, e.g. for od it is resyncRequired, for mailbox it is syncStateNotFound
        return ex is ApiException apiEx && apiEx.ResponseStatusCode == (int)HttpStatusCode.Gone;
    }
}