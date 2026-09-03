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
using AvePoint.RA.Contract.RMWeb.Account.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace AvePoint.RA.Web.Common.Utils
{
    public class ClaimUtils
    {
        public static Guid GetSessionId(ClaimsPrincipal principal)
        {
            if (principal == null) return Guid.Empty;
            var claimValue = GetClaimValue(principal, RMClaimTypes.SessionType);
            if (claimValue == null) return Guid.Empty;

            return new Guid(claimValue);
        }

        public static string GetAccountId(ClaimsPrincipal principal)
        {
            if (principal == null) return string.Empty;
            var claimValue = GetClaimValue(principal, RMClaimTypes.AccountId);
            if (claimValue == null) return string.Empty;

            return claimValue;
        }

        public static string GetTenantId(ClaimsPrincipal principal)
        {
            var claimValue = GetClaimValue(principal, RMClaimTypes.TenantGroupId);
            if (claimValue == null) return string.Empty;

            return claimValue;
        }

        public static string GetClaimValue(ClaimsPrincipal principal, string claimName)
        {
            if (principal == null) return null;

            string value = null;
            var claim = principal.FindFirst(claimName);
            value = claim?.Value;

            return value;
        }

        public static bool GetForceLoginedStatus(ClaimsPrincipal principal)
        {
            if (principal == null) return false;
            var claimValue = GetClaimValue(principal, RMClaimTypes.ForceLogined);
            if (claimValue == null) return false;
            Boolean.TryParse(claimValue, out bool forceLoingedStatus);
            return forceLoingedStatus;
        }
    }
}