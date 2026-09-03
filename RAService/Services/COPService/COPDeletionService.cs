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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Cloud.Sdk.Data.Cop.DataDeletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.COPService
{
    [Audit]
    public class COPDeletionService : ICOPDeletionService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private ITenantService _tenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IMultiGeoDataCenterService _multiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();

        public async Task<bool> DeleteMarkedTenantsAsync()
        {
            try
            {
                var markedTenants = _tenantService.GetTenantInfoByTenantStatusAndMultiGeoStatus(
                    (int)TenantStatus.HardDeleting, (int)MultiGeoStatus.MultiGeoDC);

                if (!markedTenants.Any())
                {
                    return true;
                }

                var results = new List<bool>();
                foreach (var tenant in markedTenants)
                {
                    bool success = false;
                    await TenantUtil.RunUnderTenantAsync(tenant.TenantId, tenant.RegisterEmail, async () =>
                    {
                        success = await DeleteMarkedTenantAsync(tenant);
                        _logger.Info($"Delete marked tenant: tenantId:{tenant.TenantId}, success:{success}");
                    });
                    results.Add(success);
                }

                return results.All(item => item);
            }
            catch (Exception ex)
            {
                _logger.Error($"DeleteMarkedTenantsAsync failed: {ex}");
                return false;
            }
        }

        public async Task<CheckCOPDeletionPrepareResponse> PrepareDeleteCurrentDataCenterAsync(CheckCOPDeletionPrepareRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                var softCount = UpdateDeletionStatus(request.SoftDeletionTenants, TenantStatus.SoftDeleted);
                var hardCount = UpdateDeletionStatus(request.HardDeletionTenants, TenantStatus.HardDeleting);
                return new CheckCOPDeletionPrepareResponse
                {
                    Success = true,
                    SoftDeletionCount = softCount,
                    HardDeletionCount = hardCount,
                    Message = "COP deletion candidates prepared."
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"PrepareDeleteCurrentDataCenterAsync failed: {ex}");
                return new CheckCOPDeletionPrepareResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool>  SoftDeleteOtherDCTenantsAsync(List<ToBeDeletedCustomersResult> softDeletionTenants)
        {
            var targetDataCenters = await _multiGeoDataCenterService.GetOtherDataCentersAsync();
            if (!targetDataCenters.Any())
            {
                return true;
            }

            if (!softDeletionTenants.Any())
            {
                return true;
            }

            var request = new CheckCOPDeletionPrepareRequest
            {
                SoftDeletionTenants = softDeletionTenants
                    .Where(tenant => tenant != null && !string.IsNullOrWhiteSpace(tenant.CustomerId))
                    .Select(tenant => new CheckCOPDeletionPrepareTenantRequest { TenantId = tenant.CustomerId })
                    .ToList()
            };

            var responses = await RAMultiGeoClient.RouteApiActionWithRetryAsync<CheckCOPDeletionPrepareRequest, CheckCOPDeletionPrepareResponse>(
                MultiGeoOperationType.PrepareCheckCOPDeletion,
                request,
                targetDataCenters);

            return responses.Values.All(response => response != null && response.Success);
        }
        public async Task<bool> PrepareHardDeleteMarkedTenantsAsync(
            List<ToBeDeletedCustomersResult> hardDeletionTenants)
        {
            var targetDataCenters = await _multiGeoDataCenterService.GetOtherDataCentersAsync();
            if (!targetDataCenters.Any())
            {
                return true;
            }

            if (!hardDeletionTenants.Any())
            {
                return true;
            }

            var request = new CheckCOPDeletionPrepareRequest
            {
                HardDeletionTenants = hardDeletionTenants
                    .Where(tenant => tenant != null && !string.IsNullOrWhiteSpace(tenant.CustomerId))
                    .Select(tenant => new CheckCOPDeletionPrepareTenantRequest { TenantId = tenant.CustomerId })
                    .ToList()
            };

            var responses = await RAMultiGeoClient.RouteApiActionWithRetryAsync<CheckCOPDeletionPrepareRequest, CheckCOPDeletionPrepareResponse>(
                MultiGeoOperationType.PrepareCheckCOPDeletion,
                request,
                targetDataCenters);

            return responses.Values.All(response => response != null && response.Success);
        }

        private int UpdateDeletionStatus(IEnumerable<CheckCOPDeletionPrepareTenantRequest> tenants, TenantStatus status)
        {
            var count = 0;
            if (tenants == null)
            {
                return count;
            }

            foreach (var tenant in tenants)
            {
                if (tenant == null || string.IsNullOrWhiteSpace(tenant.TenantId))
                {
                    continue;
                }

                _tenantService.ChangeAccountStatus(tenant.TenantId, status);
                count++;
            }

            return count;
        }

        private async Task<bool> DeleteMarkedTenantAsync(TenantInfoDto tenant)
        {
            if (null == tenant)
            {
                return true;
            }
            _tenantService.ChangeAccountStatus(tenant.TenantId, TenantStatus.Disabled);
            return await _tenantService.DeleteExpiredTenantAsync(tenant.TenantId);
        }
    }
}