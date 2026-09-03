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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.CP;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateStorageStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Migrate StorageDevices";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        public override string JobDetailType => "RM_JS_ArchiverMigration_DataType_Storage";

        private int[] UnsupportStorageType = new int[] { (int)StorageType.Box, (int)StorageType.Rackspace, (int)StorageType.NetApp_Alta_Vault };

        private List<StorageDeviceDto>? migrateStorageDevices;

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        protected override async Task PreExecuteAsync()
        {
            migrateStorageDevices = await JobExecutor.StorageMigrationService.LoadStoragesFromDaoStoragePoliciesAsync();
        }

        protected override async Task InnerExecuteAsync()
        {
            var unsupportStorages = migrateStorageDevices.Where(s => UnsupportStorageType.Contains(s.Type));
            if (unsupportStorages.Any())
            {
                unsupportStorages.ForEach(s => AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, s.Name, "RM_JS_ArchiverMigration_Storage_Unsupported"));
                throw new Exception("RM_JS_ArchiverMigration_Error_UnsupportedStorages");
            }

            await DatabaseUtility.BatchOperationAsync(migrateStorageDevices, async (batchItems) =>
            {
                await StorageDeviceService.BatchCreateStorageDevicesAsync(batchItems);

                JobProgressUpdater.Increase(batchItems.Count());
                batchItems.Where(s => s.Status != RMConstants.STORAGE_OLD_DATA_TYPE).ForEach(s => AddJobDetail(Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful, s.Name));
            }, 200);

        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return Task.FromResult(migrateStorageDevices.Count);
        }

    }
}
