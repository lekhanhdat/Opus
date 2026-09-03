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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Dao.Services;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateMasterIndexStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Migrate ArchiverSiteMasterIndex";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(9, 11);

        private IArchiverSiteMasterIndexService SiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();

        public override string JobDetailType => throw new NotImplementedException();

        protected override async Task InnerExecuteAsync()
        {
            List<ArchiverSiteMasterIndexContract> siteMasterIndexes = null;
            int fetchSize = 2000;
            int offset = 0;

            do
            {
                logger.Info($"Fetch master indexes from {offset}");
                siteMasterIndexes = await FetchDataAsync(offset, fetchSize);

                var count = siteMasterIndexes?.Count ?? 0;
                logger.Info($"Fetched master indexes: {count}");

                if(siteMasterIndexes != null)
                {
                    foreach (var masterIndex in siteMasterIndexes)
                    {
                        logger.Info($"Migrate master index : {masterIndex.JobId} | {masterIndex.RuleId}");
                        var (_, storageDevice, fromCache) = await JobExecutor.StorageMigrationService.GetStorageDeviceByDAOLogicalDeviceIdAsync(masterIndex.IndexDeviceId);
                        masterIndex.IndexDeviceId = storageDevice?.Id ?? Guid.Empty.ToString();
                        // fromCache = true 的之前发过Job Detail，不需要重复发
                        if (storageDevice == null && !fromCache)
                        {
                            JobReportManager.AddJobDetail(
                                Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, 
                                masterIndex.IndexDeviceId, 
                                "RM_JS_ArchiverMigration_DataType_Storage",
                                "RM_JS_ArchiverMigration_Comment_ConvertLogicalDeviceFailed");
                        }

                        masterIndex.StoragePolicyId = JobExecutor.StorageMigrationService.GetStorageDeviceIdByDAOStoragePolicyId(masterIndex.StoragePolicyId);
                        masterIndex.SiteId = JobExecutor.SPNodeService.GetSiteNodeId(masterIndex.SiteURL).ToString();
                        var siteGroupId = JobExecutor.SPNodeService.GetGroupNodeId4Site(masterIndex.SiteURL).ToString();
                        masterIndex.WebId = siteGroupId;
                        JobExecutor.ArchiverJobIdAndSourceFlagMappings[masterIndex.JobId] = JobExecutor.SPNodeService.GetSourceFlagBySiteGroupId(siteGroupId);
                    }

                    await SiteMasterIndexService.BulkCopySiteMasterIndexesAsync(siteMasterIndexes);
                }
                
                JobProgressUpdater.Increase(count);

                offset += count;
            } while (siteMasterIndexes != null && siteMasterIndexes.Count >= fetchSize);

        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return GetArchiverMigrationDataAsync<int>((service) =>
            {
                return service.GetAllSiteMasterIndexesCount();
            });
        }

        private async Task<List<ArchiverSiteMasterIndexContract>> FetchDataAsync(int offset, int fetchSize)
        {
            var data = await GetArchiverMigrationDataAsync<List<ArchiverSiteMasterIndexContract>>((service) =>
            {
                return service.GetSiteMasterIndexes(new() { Offset = offset, FetchSize = fetchSize });
            });
            return data;
        }

    }
}
