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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.ArchiverCommon;
using ExchangeBackupUtility.Graph;
using Microsoft.Exchange.WebServices.Data;

namespace AvePoint.RA.RAExchange.Discover.DiscoverImplV2;

public abstract class BaseDiscover
{
    private readonly IRALogger _logger;

    private List<string> _deleteItemIds = [];
    
    private int _maxBackupItemsThreads { get; set; } = 25;
    private int _minBackupItemsThreads { get; set; } = 10;
    private bool _enableBulkGenerateItems { get; set; } = true;
    private int _maxBulkItemsCount { get; set; } = 50;
    private int _maxBulkItemSize { get; set; } = 20;//in MB

    public BaseDiscover()
    {
        _logger = RALogger.GetInstance(GetType());
        try
        {
            _enableBulkGenerateItems = bool.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_ENABLE_BULK_GENERATE_ITEMS]);
            _maxBackupItemsThreads = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
            _maxBulkItemsCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_COUNT_LIMIT]);
            _maxBulkItemSize = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_SIZE_LIMIT]);
        }
        catch (Exception ex)
        {
            _logger.Error($"An exception occurred while trying to get the configuration, reason:{ex.ToString()}. Set the value to default.");
            _enableBulkGenerateItems = true;
            _maxBulkItemsCount = 50;
            _maxBulkItemSize = 20;
        }
    }

    protected BlockingCollection<IExchangeItemGroup> FindGroupedItems(IExchangeFolder folder,
        SearchFilter searchFilter = null)
    {
        var result = new BlockingCollection<IExchangeItemGroup>();

        ThreadPool.QueueUserWorkItem(obj =>
        {
            using var performance =
                new PerformanceScope("EXO.RMEXOApplySettingBase.FindGroupedItemsAsync", addToStatistics: true);
            _deleteItemIds = [];
            try
            {
                const int pageSize = 100; //Get 100 search result back
                int offset = 0;
                int itemCount = 0;
                //如果使用Filter, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                var maxItemCount = CalculateMaxItemsCount(_maxBackupItemsThreads, folder.ItemsCount);

                bool moreAvailable = false;
                do
                {
                    #region 1. 根据pageSize和offset,通过Find 的方式获取folder下的item.

                    if (ArchiverCommonStaticMethod.IsNestleCustomize)
                    {
                        _logger.Info("Nestle customize, set IsNestleCustomize to true");
                        folder.IsNestleCustomize = true;
                    }

                    List<IExchangeItem> exchangeItems = [];
                    using (CheckJobStopScope checkJobStopScope = new())
                    {
                        exchangeItems = folder.FindItems(pageSize, offset, out moreAvailable, searchFilter);
                    }
                    offset += pageSize;
                    itemCount += exchangeItems.Count;

                    #endregion

                    #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量导出

                    var itemCollections = IExchangeItemGroup.GroupCachedItems(
                        exchangeItems,
                        maxItemCount, _maxBulkItemSize * 1024 * 1024);
                    itemCollections.ForEach(collectionArg => { result.Add(collectionArg); });

                    #endregion

                    _logger.Info($"Current group count is {result.Count}.");
                } while (moreAvailable);

                _logger.Info("Finish find items. Changed item count: {0}, deleted item count: {1}.", itemCount,
                    _deleteItemIds.Count);
            }
            catch (JobStopException)
            {
                _logger.Warn("Job is stopping, stop finding items.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to find items, error: {ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
        });

        return result;
    }

    protected BlockingCollection<IExchangeItemGroup> GetGroupedItemsAsync(IExchangeFolder folder, string syncState)
    {
        var result = new BlockingCollection<IExchangeItemGroup>();
        ThreadPool.QueueUserWorkItem(obj =>
        {
            using var performance =
                new PerformanceScope("EXO.RMEXOApplySettingBase.GetGroupedItemsAsync", addToStatistics: true);
            _deleteItemIds = new List<string>();
            try
            {
                const int pageSize = 512;
                int itemCount = 0;
                //用于对Discover的Item去重, 目前只针对Incremental
                var allItems = new HashSet<string>();
                //如果需要check rule, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                var maxItemCount = CalculateMaxItemsCount(_maxBackupItemsThreads, folder.ItemsCount);
                var incrementalSync = !string.IsNullOrEmpty(syncState);
                _logger.Info(
                    $"Start to sync items, incrementalSync: {incrementalSync}, syncState is {syncState}, total item count:{folder.ItemsCount}.");

                bool moreAvailable = false;
                do
                {
                    #region 1. 根据pageSize和syncState获取folder下的item.

                    List<IExchangeItem> exchangeItems = [];
                    List<string> deletedItems = [];
                    using (CheckJobStopScope checkJobStopScope = new())
                    {
                        moreAvailable = folder.SyncItems(pageSize, ref syncState, allItems, out exchangeItems, out deletedItems);
                    }

                    exchangeItems.ForEach(exItem => allItems.Add(exItem.ItemId.ToRestId()));
                    itemCount += exchangeItems.Count;

                    #endregion

                    #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量操作

                    using (new PerformanceScope("EXO.RMEXODiscoverHelper.GetGroupedItemsAsync.GroupCachedItems"))
                    {
                        var itemCollections = IExchangeItemGroup.GroupCachedItems(
                            exchangeItems,
                            maxItemCount, _maxBulkItemSize * 1024 * 1024);
                        itemCollections.ForEach(collectionArg => { result.Add(collectionArg); });
                    }

                    #endregion

                    #region 3. 记录delete item集合

                    _deleteItemIds.AddRange(deletedItems);

                    #endregion

                    _logger.Info($"Current group count is : {result.Count}.");
                } while (moreAvailable);

                _logger.Info("Finish sync items. Changed item count: {0}, deleted item count: {1}.", itemCount,
                    _deleteItemIds.Count);
            }
            catch (JobStopException)
            {
                _logger.Warn("Job is stopping, stop getting items.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to sync items, error: {ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
        });
        return result;
    }
    
    private int CalculateMaxItemsCount(int consumerCount, int total)
    {
        // item 数量不如线程数多,每个线程最多分一个item
        if (total <= consumerCount) return 1;
        // item数量不如总并发数多（线程数 X 每个线程最大item数），则每个线程平分Item
        if (total <= consumerCount * _maxBulkItemsCount) return total / consumerCount;
        // total/consumerCount > config.MaxItemsCount
        return _maxBulkItemsCount;
    }

    public IEnumerable<IExchangeItemGroup> GetGroupedDeleteItems(IExchangeFolder folder, string syncState)
    {
        var result = new BlockingCollection<IExchangeItemGroup>();

        using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.GetGroupedDeleteItemsAsync", addToStatistics: true))
        {
            _deleteItemIds = new List<string>();
            try
            {
                const int pageSize = 512;//use ms recommanded page count to improve performance.
                int itemCount = 0;
                //用于对Discover的Item去重, 目前只针对Incremental
                var allItems = new HashSet<string>();
                //如果需要check rule, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                var maxItemCount = CalculateMaxItemsCount(this._maxBackupItemsThreads, folder.ItemsCount);
                var incrementalSync = !string.IsNullOrEmpty(syncState);
                _logger.Info($"Start to sync items, incrementalSync: {incrementalSync}, syncState is {syncState}, total item count:{folder.ItemsCount}.");

                bool moreAvailable = false;
                do
                {
                    #region 1. 根据pageSize和syncState获取folder下的item.
                    //List<ExchangeItem> exchangeItems;
                    List<string> deletedItems = [];
                    using (CheckJobStopScope checkJobStopScope = new())
                    {
                        moreAvailable = folder.SyncDeleteItems(pageSize, ref syncState, allItems, out deletedItems);
                    }

                    //exchangeItems.ForEach(exItem => allItems.Add(exItem.ItemId));
                    //itemCount += exchangeItems.Count;

                    #endregion
                    //#region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量操作
                    //using (new PerformanceScope("EXO.RMEXODiscoverHelper.GetGroupedItemsAsync.GroupCachedItems"))
                    //{
                    //    var itemCollections = ExchangeItemGroup.GroupCachedItems(
                    //    exchangeItems,
                    //    maxItemCount, this.MaxBulkItemSize * 1024 * 1024);
                    //    itemCollections.ForEach(collectionArg =>
                    //    {
                    //        result.Add(collectionArg);
                    //    });
                    //}
                    //#endregion
                    #region 3. 记录delete item集合
                    _deleteItemIds.AddRange(deletedItems);
                    #endregion
                    _logger.Info($"Current group count is : {result.Count}.");
                } while (moreAvailable);
                _logger.Info("Finish sync items. Changed item count: {0}, deleted item count: {1}.", itemCount, _deleteItemIds.Count);
            }
            catch (JobStopException)
            {
                _logger.Warn("Job is stopping, stop getting deleted items.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to sync items, error: {ex.ToString()}");
            }
            finally
            {
                result.CompleteAdding();
            }
        }
        return result;
    }

    public List<string> GetDeleteItemIds() => this._deleteItemIds;
}