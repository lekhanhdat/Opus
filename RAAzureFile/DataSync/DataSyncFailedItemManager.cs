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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncFailedItemManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DataSyncFailedItemManager));

        private static readonly ISyncFailureItemDao SyncFailureItemDao = PlatformWindsorManager.GetService<ISyncFailureItemDao>();

        private static readonly int MaxStorageFailedItemLimit = 2000;

        private static readonly List<SyncFailureItemEntity> PreviouslyFailedItems = new List<SyncFailureItemEntity>();

        private static readonly ConcurrentQueue<SyncFailureItemEntity> CurrentlyFailedItems = new ConcurrentQueue<SyncFailureItemEntity>();

        public static bool IsLimitExceeded => CurrentlyFailedItems.Count > MaxStorageFailedItemLimit;

        public static string ScopeId { get; private set; }

        public static void Initialization(string scopeId)
        {
            ScopeId = scopeId;
            var failedItems = SyncFailureItemDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, ScopeId, (int)SourceFlag.AzureFileShare);
            PreviouslyFailedItems.AddRange(failedItems);
            Logger.Info($"The scope [{scopeId}] has failed items count [{failedItems.Count}].");
        }

        public static void AddFailedItem(AzureFileShareApiItem failedItem)
        {
            if(IsLimitExceeded)
            {
                return;
            }

            var entity = new SyncFailureItemEntity(ScopeId, failedItem.Id.ToString())
            {
                DataSource = (int)SourceFlag.AzureFileShare,
                FullPath = failedItem.FullPath,
                IsDirectory = failedItem.IsDirectory
            };
            CurrentlyFailedItems.Enqueue(entity);
        }

        public static void AddFailedItem(Record failedItem)
        {
            if(IsLimitExceeded)
            {
                return;
            }

            var entity = new SyncFailureItemEntity(ScopeId, failedItem.Id.ToString())
            {
                DataSource = (int)SourceFlag.AzureFileShare,
                FullPath = AzureFileShareApiUtil.UrlCombin(failedItem.DirPath, failedItem.LeafName),
                IsDirectory = failedItem.NodeType == (int)RMNodeLevel.AzureFileShareDirectory
            };
            CurrentlyFailedItems.Enqueue(entity);
        }

        public static void AddFailedItem(AzureFileShareApiDirectoryClient failedItem)
        {
            if (IsLimitExceeded)
            {
                return;
            }

            var entity = new SyncFailureItemEntity(ScopeId, failedItem.Id.ToString())
            {
                DataSource = (int)SourceFlag.AzureFileShare,
                FullPath = failedItem.FullPath,
                IsDirectory = true
            };
            CurrentlyFailedItems.Enqueue(entity);
        }
        
        public static bool HasPreviouslyFailedItem(Guid id)
        {
            return PreviouslyFailedItems.Any(item => item.RowKey == id.ToString());
        }

        public static bool StorageFailedItems()
        {
            try
            {
                SyncFailureItemDao.RemoveAll(TenantLocalValue.LogonGroupId, ScopeId);
                return SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, CurrentlyFailedItems.ToList());
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while storage failed items. Error: {e}");
                return false;
            }
        }
    }
}
