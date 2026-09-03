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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;

namespace RASPAppWeb
{
    public class TenantService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(TenantService));

        public static bool ValidateTenant(string o365TenantId, string o365Domain, string tenantId)
        {
            var isValidTenant = false;
            //tenantId = GetAveId(o365TenantId, o365Domain);
            if (!string.IsNullOrEmpty(tenantId) && RMAosApiClient.IsCustomerLicenseAvailable(tenantId))
            {
                var o365IDs = RMAosApiClient.GetO365TenantIds(tenantId);
                isValidTenant = o365IDs.Contains(o365TenantId);
                if (!isValidTenant)
                {
                    logger.Warn("Invalid o365 {0} {1}, O365IDs in {2}: [{3}]", o365TenantId, o365Domain, tenantId, string.Join(",", o365IDs.ToArray()));
                }
            }
            else
            {
                logger.Warn("Invalid tenant {0} from o365 {1} {2}", tenantId, o365TenantId, o365Domain);
            }
            return isValidTenant;
        }

        //private static string GetAveId(string o365TenantId, string o365Domain)
        //{
        //    Func<string> getObj = () =>
        //    {
        //        var provider = Office365ApiProviderFactory.GetInstance(o365TenantId, o365Domain);
        //        var adminSiteUrl = $"https://{o365Domain}-admin.sharepoint.com";
        //        if (provider.ValidateResource())
        //        {
        //            return provider.GetAveId(adminSiteUrl);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    };
        //    return CacheService.Get("AveId", o365Domain, getObj, TimeSpan.FromMinutes(30));
        //}

    }
}