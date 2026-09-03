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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RedisCache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class SyncItemArchiverStatusAction
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SyncItemArchiverStatusAction));

        private static IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService<IArchiverTableDao>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static readonly string ArchiverDataBaseConfigCacheKeyPrefix = RecordsConstants.ArchiverDataBaseConfigCacheKeyPrefix;

        private readonly IRedisCacheProvider _redis = RedisCacheService.CacheProvider;

        private readonly bool _needSyncArchiverTable;

        private readonly AzureTableConnectContract _connectContract;

        private static readonly string s_connectString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private static readonly List<SourceFlag> _needSyncArchvierSourceFlag = new List<SourceFlag>()
        {
            SourceFlag.FileSystem,
            SourceFlag.SharePointOnPrem,
        };

        public SyncItemArchiverStatusAction()
        {
            _needSyncArchiverTable = !TenantService.IsNewOpusTenant();
            if (_needSyncArchiverTable)
            {
                var cacheKey = ArchiverDataBaseConfigCacheKeyPrefix + TenantLocalValue.LogonGroupId;
                var existKey = _redis.KeyExists(cacheKey);
                if (!existKey)
                {
                    _connectContract = new DAOAPIClientV1().GetArchiverDataBaseConfigAsync().Result;
                    var connectContractStr = SerializerHelper.SerializeByJsonConvert(_connectContract);
                    _redis.StringSet(cacheKey, connectContractStr);
                }
                var cacheValue = _redis.StringGet(cacheKey);
                _connectContract = SerializerHelper.DeserializeByJsonConvert<AzureTableConnectContract>(cacheValue);

            }
        }

        public async System.Threading.Tasks.Task UpdateItemArchiverStatusAsync(Record item)
        {

            if (_needSyncArchiverTable || _needSyncArchvierSourceFlag.Contains((SourceFlag)item.SourceFlag) || item.SourceFlag >= 1000)
            {
                try
                {
                    var partionKey = item.ManualPartitionKey;
                    var rowKey = item.ManualRowKey;
                    var tenantId = TenantLocalValue.LogonGroupId;
                    var approved = item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved;

                    switch ((SourceFlag)item.SourceFlag)
                    {
                        case SourceFlag.Physical:
                            item.DisposalStatus = item.ManualApprovedStatus;
                            break;
                        case SourceFlag.Exchange:
                            await ArchiverTableDao.UpdateItemStatusForEXOAsync(_connectContract, tenantId, partionKey, rowKey, approved);
                            break;
                        case SourceFlag.FileSystem:
                            await ArchiverTableDao.UpdateItemStatusForFSAsync(s_connectString, tenantId, partionKey, rowKey, approved);
                            break;
                        case SourceFlag.SharePointOnPrem:
                            await ArchiverTableDao.UpdateItemStatusForSPOnPremAsync(s_connectString, tenantId, partionKey, item.NodeId, approved);
                            break;
                        case SourceFlag.OneDrive:
                        case SourceFlag.SharePoint:
                            await ArchiverTableDao.UpdateItemStatusAsync(_connectContract, tenantId, partionKey, item.NodeId, approved);
                            break;
                        case SourceFlag.LifecycleRetention:
                            await ArchiverTableDao.UpdateItemStatusAsync(_connectContract, tenantId, partionKey, item.NodeId, approved);
                            break;
                        case var flag when (int)flag >= 1000:
                            item.DisposalStatus = item.ManualApprovedStatus;
                            break;
                    }
                }
                catch(Exception e)
                {
                    Logger.Error($"Update item archiver status failed, item id {item.Id} ,error :{e}");
                    throw;
                }
            }
        }

        public async System.Threading.Tasks.Task ResetItemArchiverStatusAsync(Record item)
        {
            if (_needSyncArchiverTable || _needSyncArchvierSourceFlag.Contains((SourceFlag)item.SourceFlag) || item.SourceFlag >= 1000)
            {
                try
                {
                    var partionKey = item.ManualPartitionKey;
                    var rowKey = item.ManualRowKey;
                    var tenantId = TenantLocalValue.LogonGroupId;
                    var isDisposed = false;

                    switch ((SourceFlag)item.SourceFlag)
                    {
                        case SourceFlag.Physical:
                            if (item.DisposalStatus == (int)SOApproveDBStatus.Archived //Approved
                                        || (item.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove && !item.ExportToRECO) //Rejected
                                        || item.RecordStatus == (int)RMRecordStatus.Missing
                                        || item.RecordStatus == (int)RMRecordStatus.RMDeleted
                                        || item.RecordStatus == (int)RMRecordStatus.Destroyed)
                            {
                                isDisposed = true;
                            }
                            if (!isDisposed) item.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                            break;
                        case SourceFlag.Exchange:
                            isDisposed = ArchiverTableDao.GetArchiverItemForEXO(_connectContract, tenantId, partionKey, rowKey) == null;
                            if (!isDisposed) await ArchiverTableDao.ResetItemStatusForEXOAsync(_connectContract, tenantId, partionKey, rowKey);
                            break;
                        case SourceFlag.FileSystem:
                            isDisposed = ArchiverTableDao.GetArchiverItemForFS(s_connectString, tenantId, partionKey, rowKey) == null;
                            if (!isDisposed) await ArchiverTableDao.ResetItemStatusForFSAsync(s_connectString, tenantId, partionKey, rowKey);
                            break;
                        case SourceFlag.SharePointOnPrem:
                            isDisposed =  ArchiverTableDao.GetArchiverItemForSPOnPrem(s_connectString, tenantId, partionKey, rowKey) == null;
                            if (!isDisposed) await ArchiverTableDao.ResetItemStatusForSPOnPremAsync(s_connectString, tenantId, partionKey, item.NodeId);
                            break;
                        case SourceFlag.OneDrive:
                        case SourceFlag.SharePoint:
                            isDisposed =  ArchiverTableDao.GetArchiverItem(_connectContract, tenantId, partionKey, rowKey) == null;
                            if (!isDisposed) await ArchiverTableDao.ResetItemStatusAsync(_connectContract, tenantId, partionKey, item.NodeId);
                            break;
                        case SourceFlag.LifecycleRetention:
                            isDisposed =  ArchiverTableDao.GetArchiverItem(_connectContract, tenantId, partionKey, rowKey) == null;
                            if (!isDisposed) await ArchiverTableDao.ResetItemStatusAsync(_connectContract, tenantId, partionKey, item.NodeId);
                            break;
                        case var flag when (int)flag >= 1000:
                            item.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                            break;
                    }

                    if (isDisposed)
                    {
                        throw new Exception("RM_JS_MA_ItemDisposal");
                    }
                }
                catch(Exception e)
                {
                    Logger.Error($"Reset item archiver status failed, item id {item.Id} ,error :{e}");
                    throw;
                }
            }   
        }
    }
}
