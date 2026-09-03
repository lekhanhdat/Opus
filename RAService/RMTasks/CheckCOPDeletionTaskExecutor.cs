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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.Message;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.Cop.Common;
using Cloud.Sdk.Data.Cop.DataDeletion;
using CopApiUnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AvePoint.RA.Service.RMTasks
{
    public class CheckCOPDeletionTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();
        public IMultiGeoDataCenterService _multiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public IMultiGeoSettingService _multiGeoSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        public ICOPDeletionService _copDeletionService = PlatformWindsorManager.GetService<ICOPDeletionService>();

        private static int SoftCount { get; set; }
        private static int HardCount { get; set; }

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            _logger.Debug("Check COP Task run task.");

            try
            {
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    _logger.Info("debug skip to excute cop deletion.");
                    return;
                }

                _logger.Debug("process Check COP Task.");

                await _copDeletionService.DeleteMarkedTenantsAsync();
                _logger.Info("Finish delete marked tenants.");

                var dataCenter = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];
                _logger.Info($"Start to check cop deletion for data center {dataCenter}.");
                var softDeletionTenants = await COPAPIClient.GetToBeDeletedCustomers(DeletionType.SoftDelete, ProductType.AvePointRecords, dataCenter);

                var hardDeletionTenants = await COPAPIClient.GetToBeDeletedCustomers(DeletionType.HardDelete, ProductType.AvePointRecords, dataCenter);

                if (softDeletionTenants.Any())
                {
                    SoftDeletion(softDeletionTenants);
                }

                if (hardDeletionTenants.Any())
                {
                    await HardDeletionAsync(hardDeletionTenants);
                }

                _logger.Info($"Finish process check cop task. Soft deletion count : {SoftCount}, hard deletion count : {HardCount}");

            }
            catch (Exception e)
            {
                _logger.Error("Run check COP deletion Task error: {0}", e);
            }
        }

        private void SoftDeletion(List<ToBeDeletedCustomersResult> softDeletionTenants)
        {
            foreach (var tenant in softDeletionTenants)
            {
                if (tenant == null)
                {
                    continue;
                }
                TenantUtil.RunUnderTenant(tenant.CustomerId, "",
                async () =>
                {
                    _logger.Info($"check COP soft deletion tenant: tenantId:{tenant?.CustomerId}, product:{tenant?.ProductType},recordId:{tenant?.RecordId}");
                    COPReturnMessage rMsg = new()
                    {
                        RecordId = tenant?.RecordId,
                        Status = COPTenantStatus.SoftFailed,
                        Product = RecordsConstants.RECORDS_APPLICATION_NAME
                    };
                    try
                    {
                        rMsg.Status = COPTenantStatus.SoftDeleting;
                        COPAPIClient.DataDeletion(rMsg);

                        if (!_tenantService.CheckTenantExist(tenant.CustomerId))
                        {
                            _logger.Info($"tenant soft not exist in records db:{tenant.CustomerId}.");
                            rMsg.Status = COPTenantStatus.SoftDeleted;
                        }
                        else if (!RMAosApiClient.IsCustomerLicenseAvailable(tenant.CustomerId))
                        {
                            _logger.Info($"tenant soft is expired:{tenant.CustomerId}.");
                            if (await _multiGeoSettingService.IsEnableMultiGeoFeature() && _multiGeoDataCenterService.IsMainDC())
                            {
                                _logger.Info($"Start to soft delete tenant {tenant.CustomerId} in other data centers.");
                                bool softDeleteOtherDCSuccess =  await _copDeletionService.SoftDeleteOtherDCTenantsAsync(softDeletionTenants);
                                if (!softDeleteOtherDCSuccess) 
                                {
                                    throw new Exception($"Soft delete tenant {tenant.CustomerId} in other data centers failed.");
                                }
                                _logger.Info($"Finish soft delete tenant {tenant.CustomerId} in other data centers.");
                            }
                            _tenantService.ChangeAccountStatus(tenant.CustomerId, TenantStatus.SoftDeleted);
                            rMsg.Status = COPTenantStatus.SoftDeleted;
                            _logger.Info($"success soft delete by cop tenant:{tenant?.CustomerId}");
                            SoftCount++;
                        }
                        else
                        {
                            rMsg.Status = COPTenantStatus.SoftFailed;
                            _logger.Info($"can not delete soft tenant:{tenant.CustomerId}, the tenant is avilable.");
                        }
                    }
                    catch (Exception ex)
                    {
                        rMsg.Status = COPTenantStatus.SoftFailed;
                        _logger.Error($"clean tenant message soft failed for group {tenant?.CustomerId} record id {tenant?.CustomerId},ERROR: {ex}.");
                    }
                    COPAPIClient.DataDeletion(rMsg);

                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine($"Product：Opus");
                    sb.AppendLine($"COP Record Id: {tenant?.RecordId}");
                    sb.AppendLine($"TenantId: {tenant?.CustomerId}");
                    sb.AppendLine($"Data types: Db Schema, Database, Azure Storage Data");  // 这里面的type，比如Db Schema, Database, Azure Storage Data 等
                    if (rMsg.Status == COPTenantStatus.SoftDeleted)
                    {
                        sb.AppendLine("Mark for deletion.");
                    }
                    else
                    {
                        sb.AppendLine("Mark for deletion failed.");
                    }
                    _logger.Info(sb.ToString());
                });
            }
        }

        private async System.Threading.Tasks.Task HardDeletionAsync(List<ToBeDeletedCustomersResult> hardDeletionTenants)
        {
            foreach (var tenant in hardDeletionTenants)
            {
                await TenantUtil.RunUnderTenantAsync(tenant.CustomerId, "",
                    async () =>
                    {
                        _logger.Info($"check COP hard deletion tenant: tenantId:{tenant?.CustomerId}, product:{tenant?.ProductType},recordId:{tenant?.RecordId}");
                        COPReturnMessage rMsg = new()
                        {
                            RecordId = tenant?.RecordId,
                            Status = COPTenantStatus.HardFailed,
                            Product = RecordsConstants.RECORDS_APPLICATION_NAME
                        };
                        try
                        {
                            rMsg.Status = COPTenantStatus.HardDeleting;
                            COPAPIClient.DataDeletion(rMsg);
                            _logger.Info($"validate cop message success:{tenant?.CustomerId}.");
                            if (!_tenantService.CheckTenantExist(tenant?.CustomerId))
                            {
                                _logger.Info($"tenant not exist in records db:{tenant?.CustomerId}.");
                                rMsg.Status = COPTenantStatus.HardDeleted;
                            }
                            else
                            {
                                _logger.Info($"tenant exist in records db, manual deletion by cop ,tenant is:{tenant?.CustomerId}.");
                                if (await _multiGeoSettingService.IsEnableMultiGeoFeature() && _multiGeoDataCenterService.IsMainDC())
                                {
                                    _logger.Info($"Start to prepare hard delete marked tenant {tenant.CustomerId} in other data centers.");
                                    bool prepareHardSuccess = await _copDeletionService.PrepareHardDeleteMarkedTenantsAsync(hardDeletionTenants);
                                    if (!prepareHardSuccess)
                                    {
                                        throw new Exception($"Prepare hard delete marked tenant {tenant.CustomerId} in other data centers failed.");
                                    }
                                    _logger.Info($"Finish prepare hard delete marked tenant {tenant.CustomerId} in other data centers.");
                                }
                                _tenantService.ChangeAccountStatus(tenant?.CustomerId, TenantStatus.Disabled);
                                if (await _tenantService.DeleteExpiredTenantAsync(tenant?.CustomerId))
                                {
                                    rMsg.Status = COPTenantStatus.HardDeleted;
                                    _logger.Info($"success delete by cop tenant:{tenant?.CustomerId}");
                                    HardCount++;
                                }
                                else
                                {
                                    rMsg.Status = COPTenantStatus.HardFailed;
                                    _logger.Info($"failed delete by cop tenant:{tenant?.CustomerId}");
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            rMsg.Status = COPTenantStatus.HardFailed;
                            _logger.Error($"clean tenant message failed for group {tenant?.CustomerId} record id {tenant?.RecordId},ERROR: {ex}.");
                        }
                        COPAPIClient.DataDeletion(rMsg);

                        var sb = new StringBuilder();
                        sb.AppendLine();
                        sb.AppendLine($"Product：Opus");
                        sb.AppendLine($"COP Record Id: {tenant?.RecordId}");
                        sb.AppendLine($"TenantId: {tenant?.CustomerId}");
                        sb.AppendLine($"Data types: Db Schema, Database, Azure Storage Data");
                        if (rMsg.Status == COPTenantStatus.HardDeleted)
                        {
                            sb.AppendLine("Deletion completed.");
                        }
                        else
                        {
                            sb.AppendLine("Deletion failed.");
                        }
                        _logger.Info(sb.ToString());
                    }
                 );
            }
        }
    }
}
