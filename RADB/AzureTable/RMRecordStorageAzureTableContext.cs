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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.AzureTable
{
    public class RMRecordStorageAzureTableContext : RMAzureTableContext
    {

        private static RMRecordStorageAzureTableContext Context;

        private static readonly object Locker = new ();

        private RMRecordStorageAzureTableContext() : base(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING])
        {
        }

        private static RMRecordStorageAzureTableContext GetInstance()
        {
            if(Context == null)
            {
                lock(Locker)
                {
                    Context ??= new RMRecordStorageAzureTableContext();
                }
            }

            return Context;
        }

        public static readonly RMAzureTableDataSet<RMManualApproveHistoryTableEntity> ManualApproveHistories 
            = new (GetInstance(), "ManualApproveHistories", true);

        public static readonly RMAzureTableDataSet<RMManualArchiverSharePointOnPremiseTableEntity> ManualArchiverSharePointOnPremiseItems
            = new (GetInstance(), "SOOnPremiseSPArchiverDB", true);

        public static readonly RMAzureTableDataSet<RMManualArchiverFileSystemTableEntity> ManualArchiverFileSystemItems
            = new(GetInstance(), "SOFSArchiverDB", true);

        public static readonly RMAzureTableDataSet<RMDataOptimizationSettingsHistoryTableEntity> RMDiscoverDataOptimizationSettingsHistory
            = new(GetInstance(), "DataOptimizationSettingsHistory", true);

        public static readonly RMAzureTableDataSet<RMNeedDeleteArchivedDataTableEntity> NeedDeleteArchivedDataList
            = new(GetInstance(), "NeedDeleteArchivedDataList", true);

        public static readonly RMAzureTableDataSet<RMDataIngestionExecutionResultTableEntity> DataIngestionExecuteResultList
            = new(GetInstance(), "DataIngestionExecuteResultList", true);

        public static readonly RMAzureTableDataSet<RMDataIngestionMessageTableEntity> DataIngestionMessageList
            = new(GetInstance(), "DataIngestionMessageList", true);

        public static readonly RMAzureTableDataSet<SyncFailureItemEntity> DataSyncFailureList
           = new(GetInstance(), "RECODataSyncFailure", true);
    }
}
