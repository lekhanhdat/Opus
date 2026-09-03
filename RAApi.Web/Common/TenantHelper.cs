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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Common
{
    public class TenantHelper
    {
        private readonly static RALogger logger = RALogger.GetInstance(typeof(TenantHelper));
        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private static string AosTenant4Spfx = "AosIdByAadTenant";
        public static async Task<string> GetTenantBySiteUrlAsync(string siteUrl)
        {
            try
            {
                var domain = WebUtil.GetTenantDomainName(siteUrl);
                return await GetTenantByDomainNameAsync(domain, cidInRequest: string.Empty);
            }
            catch (Exception e)
            {
                logger.Warn($"GetTenantBySiteUrlAsync error, {e}");
                return "";
            }
        }

        public static async Task<string> GetTenantByUPNAsync(string upn, string cidInRequest)
        {
            var domain = upn.Split('@').Last();
            return await GetTenantByDomainNameAsync(domain, cidInRequest);
        }

        private static async Task<string> GetTenantByDomainNameAsync(string domain, string cidInRequest)
        {
            logger.Info($"Get AOS tenant id by {domain}");
            var cacheKey = $"{AosTenant4Spfx}-{domain}-{cidInRequest}";
            try
            {
                var aosTenantId = await Cache.GetAsync<string>(cacheKey, false);
                if (!string.IsNullOrEmpty(aosTenantId))
                {

                    if (string.IsNullOrEmpty(cidInRequest) || cidInRequest == aosTenantId)
                    {
                        logger.Info($"Get AOS tenant. aad domain:{domain} - {aosTenantId} - {cidInRequest}");
                        return aosTenantId;
                    }
                    else
                    {
                        logger.Info($"Get AOS tenant. aad domain:{domain} - {aosTenantId} - {cidInRequest}, but different with the request cid.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Get aos tenant id from cache error, we will get from aos. error: {e}");
            }
            logger.Info($"We will get tenant id from aos {domain}, cidInRequest is: {cidInRequest}");
            var aadTenantId = WebUtil.GetOffice365tenantIdByDomain(domain);
            List<string> tenantIds = [];
            var cid = string.Empty;
            var customerIDs = await AosApiUtility.GetAosModernApplicationClient().CustomerService.GetCustomerIdsAsync(aadTenantId);
            List<string> cids = [];
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                cids = ctx.TenantInfo.Where(t => t.Status == 0 & customerIDs.Contains(t.Id)).OrderByDescending(t => t.CreateTime).Select(t => t.Id).ToList();
            }
            if (!string.IsNullOrEmpty(cidInRequest))
            {
                cid = cids.Find(cid => cid == cidInRequest);
            }
            if (string.IsNullOrEmpty(cid))
            {
                logger.Warn("Tenant id from request is empty, we will get the first one.");
                cid = cids.FirstOrDefault();
            }
            try
            {
                await Cache.SetAsync(cacheKey, cid, TimeSpan.FromHours(8), false);
            }
            catch (Exception e)
            {
                logger.Warn($"Set aos tenant id to cache error. error: {e}");
            }
            return cid;
        }
    }
}
