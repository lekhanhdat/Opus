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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.DB.Dao;
using Cloud.sdk.Data.Opus;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AvePoint.Api.Web.ApiControllers
{
    [Route("api/provision/[action]")]
    //[Authorize]
    [ApiController]
    public class ProvisionController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(ProvisionController));

        private ISPProvisioningContainerDao ProvisioningContainerDao => PlatformWindsorManager.GetService<ISPProvisioningContainerDao>();


        [HttpPost]
        [TypeFilter(typeof(APIRateLimitFilter))]
        [APIScopeFilter(ContractConstants.RecordsPublicScope)]
        public Task<bool> RegisterWebhook([FromBody]ProvisionSPListInfo listInfo)
        {
            return SaveProvisioningListAsync(listInfo.TenantID, listInfo.WebUrl, listInfo.ListID);
        }






        private Task<bool> SaveProvisioningListAsync(string tenantId, string webUrl, string listId)
        {
            if (string.IsNullOrWhiteSpace(listId) || !Guid.TryParse(listId, out _))
            {
                logger.Error($"Invalid listID. {tenantId}|{webUrl}|{listId}");
                return Task.FromResult(false);
            }

            return SaveProvisioningContainerAsync(tenantId, webUrl, listId);
        }
        private async Task<bool> SaveProvisioningContainerAsync(string tenantId, string webUrl, string listId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(webUrl))
                {
                    logger.Error($"Invalid parameters. {tenantId}|{webUrl}|{listId}");
                    return false;
                }

                var result = await ProvisioningContainerDao.CreateIfNotExistsAsync(tenantId, webUrl, listId);
                logger.Info($"Save provisioning container result: {result}");
                return result;
            }
            catch (System.Exception ex)
            {
                logger.Error($"Save provisioning container failed for {webUrl}|{listId}. {ex}");
            }
            
            return false;
        }
    }

}
