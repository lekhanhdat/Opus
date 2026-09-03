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
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.CosmosDBControl;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.FileSystem;

namespace AvePoint.RA.Service.RMTasks
{
    public class UpgradeDataCosmosDbForJPMCUtil
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(UpgradeDataCosmosDbForJPMCUtil));

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public async Task ExecutorAsync()
        {
            var tenants = TenantService.GetAllAvailableTenantInfo();
            foreach (var tenant in tenants)
            {
                await TenantUtil.RunUnderTenantAsync(
                    tenant.TenantId,
                    tenant.RegisterEmail,
                    async () =>
                    {
                        if (!RMCosmosDBIndependentController.IsEnabledIndependent(tenant.TenantId))
                        {
                            _logger.Info("Skip JPMC Cosmos DB migration because it is not enabled. Tenant: {0}.", tenant.TenantId);
                            return;
                        }

                        var upgradeService = new RMFSUpgradeDataService();
                        var status = upgradeService.GetUpgradeStatus();
                        if (status is 3)
                        {
                            _logger.Info("Skip JPMC Cosmos DB migration because the migration has already been completed. Tenant: {0}.", tenant.TenantId);
                            return;
                        } 
                        _ = Task.Run(() => upgradeService.EnsureMigrationJobAsync(tenant.TenantId, tenant.RegisterEmail));
                    });
            }
        }
    }
}
