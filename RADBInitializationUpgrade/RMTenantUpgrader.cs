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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Core.Discovery.Upgrader;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Upgrade;

namespace RADBInitializationUpgrade
{
    public class RMTenantUpgrader : TenantUpgradeTaskBase
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMTenantUpgrader));

        private readonly ITenantInfoDao _tenantDao = PlatformWindsorManager.GetService<ITenantInfoDao>();

        private readonly ITenantUpgradeService _tenantUpgradeService = PlatformWindsorManager.GetService<ITenantUpgradeService>();
        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        public override bool IsNeedUpgrade() => true;

        protected override Task<List<string>> GetTenantIdsAsync()
        {
            var tenantInfoes = _tenantDao.GetAllTenantInfo();
            var tenantIds = tenantInfoes.Select(item => item.TenantId).ToList();
            return Task.FromResult(tenantIds);
        }

        protected override async Task UpgradeTenantDataAsync(string tenantId)
        {
            // tenant upgarde info
            await TenantUtil.RunUnderTenantAsync(tenantId, "", async () =>
            {
                var immediatelyProcessor = new RMTenantImmediatelyUpgradeProcessor();
                await immediatelyProcessor.RunAsync();

                if(RMTenantUpgradeHelper.NeedRunDelayUpgradeJob(tenantId))
                {
                    _tenantUpgradeService.SendUpgradeJobMessage();
                }
            });
        }

        protected override async Task UpgradeTenantTableAsync(string tenantId)
        {
            try
            {
                _logger.Info($"Start upgrade tenant [{tenantId}] table.");

                await TenantUtil.RunUnderTenantAsync(tenantId, "", async () =>
                {
                    RMDBInitializer.UpgradTenantDBModel();
                    _logger.Info($"End to UpgradTenantDBModel.");
                    await RMDBInitializer.UpgradeDBDataAsync();
                    _logger.Info($"End to UpgradeDBDataAsync.");
                    await RMDBInitializer.UpgradeSecurityDBDataAsync();
                    _logger.Info($"End to UpgradeSecurityDBDataAsync.");
                    await RMDiscoveryUpgrader.UpgradeAsync();
                    _logger.Info($"End to Upgrade Discovery Tables.");
                    //if (_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.DiscoveryPlan))
                    //{
                    //    _logger.Info($"Start to Init Plan Tables.");
                    //    await RMDiscoveryDBManager.InitPlanTablesAsync();
                    //    _logger.Info($"End to Init Plan Tables.");
                    //}
                    //_logger.Info($"Start to Init Plan Tables.");
                    //await RMDiscoveryDBManager.InitPlanTablesAsync();
                    //_logger.Info($"End to Init Plan Tables.");
                });

                _logger.Info($"Successful upgrade tenant [{tenantId}] table.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while upgrade tenant [{tenantId}] table. Error: {e}");
                throw;
            }
        }
    }
}
