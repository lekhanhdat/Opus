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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RADataBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Upgrade
{
    public class ManualSepCIUpgrade
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualSepCIUpgrade));

        private static IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService<IArchiverTableDao>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static readonly string ConnectString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private readonly ManualApprovalRecordRepository Repository;

        private readonly AzureTableConnectContract ConnectContract;

        private bool mNeedSyncArchiverTable = false;

        public ManualSepCIUpgrade()
        {
            try
            {
                ConnectContract = new DAOAPIClientV1().GetArchiverDataBaseConfigAsync().Result;
                Repository = new ManualApprovalRecordRepository();
                mNeedSyncArchiverTable = !TenantService.IsNewOpusTenant();
            }
            catch(Exception e)
            {
                Logger.Error($"[Sep CI Upgrade ERROR] An error occurred while execute sep ci upgrade. Error: {e}");
            }
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            try
            {
                var succeedCount = 0;
                var failedCount = 0;
                var needSyncStatusToArchiveTableItems = await Repository.QueryItemsAsync(item =>
                 item.IsManualSynced &&
                 (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected) &&
                 item.ManualArchiveStatus == 0
                );
                Logger.Info($"[Sep CI Upgrade] Current tenant need sync status to archive table item count: [{needSyncStatusToArchiveTableItems}].");
                foreach(var item in needSyncStatusToArchiveTableItems)
                {
                    try
                    {
                        await SyncStatusToArchiveAsync(item);
                        Logger.Info($"[Sep CI Upgrade] Current item: [{item.Id}] sync status to archive table succeed.");
                        succeedCount++;
                    }
                    catch(Exception e)
                    {
                        Logger.Error($"[Sep CI Upgrade ERROR] An error occurred while upgrade item: [{item.Id}]. Error: {e}");
                        failedCount++;
                    }
                }

                Logger.Error($"[Sep CI Upgrade Completed] succeed item count: [{succeedCount}], failed item count: [{failedCount}].");
            }
            catch(Exception e)
            {
                Logger.Error($"[Sep CI Upgrade ERROR] An error occurred while run manual sep ci upgrade. Error: {e}");
            }
        }

        private async System.Threading.Tasks.Task SyncStatusToArchiveAsync(ManualApprovalRecord item)
        {
            if (mNeedSyncArchiverTable)
            {
                var partionKey = item.ManualPartitionKey;
                var rowKey = item.ManualRowKey;
                var tenantId = TenantLocalValue.LogonGroupId;
                var approved = item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved;

                switch ((SourceFlag)item.SourceFlag)
                {
                    case SourceFlag.Physical:
                        //item.DisposalStatus = item.DisposalStatus;
                        break;
                    case SourceFlag.Exchange:
                        await ArchiverTableDao.UpdateItemStatusForEXOAsync(ConnectContract, tenantId, partionKey, rowKey, approved);
                        break;
                    case SourceFlag.FileSystem:
                        await ArchiverTableDao.UpdateItemStatusForFSAsync(ConnectString, tenantId, partionKey, rowKey, approved);
                        break;
                    case SourceFlag.SharePointOnPrem:
                        await ArchiverTableDao.UpdateItemStatusForSPOnPremAsync(ConnectString, tenantId, partionKey, item.NodeId, approved);
                        break;
                    case SourceFlag.OneDrive:
                    case SourceFlag.SharePoint:
                        await ArchiverTableDao.UpdateItemStatusAsync(ConnectContract, tenantId, partionKey, item.NodeId, approved);
                        break;
                    case SourceFlag.LifecycleRetention:
                        await ArchiverTableDao.UpdateItemStatusAsync(ConnectContract, tenantId, partionKey, item.NodeId, approved);
                        break;

                }
            }
        }
    }
}
