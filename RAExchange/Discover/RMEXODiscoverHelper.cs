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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.ArchiverCommon;
using ExchangeBackupUtility;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Discover
{
    public class RMEXODiscoverHelper
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXODiscoverHelper));

        public int MaxBackupItemsThreads { get; set; } = 25;
        public int MinBackupItemsThreads { get; set; } = 10;
        public bool EnableBulkGenerateItems { get; set; } = true;
        public int MaxBulkItemsCount { get; set; } = 50;
        public int MaxBulkItemSize { get; set; } = 20;//in MB

        public List<string> deleteItemIds = new List<string>();
        //异步获取ExchangeItem对象。目前hard code 每次取512 个（MS 建议值）

        public RMEXODiscoverHelper()
        {
            InitFromConfig();
        }

        private void InitFromConfig()
        {
            try
            {
                this.EnableBulkGenerateItems = bool.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_ENABLE_BULK_GENERATE_ITEMS]);
                this.MaxBackupItemsThreads = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
                this.MaxBulkItemsCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_COUNT_LIMIT]);
                this.MaxBulkItemSize = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_SIZE_LIMIT]);
            }
            catch (Exception ex)
            {
                logger.Error($"An exception occurred while trying to get the configuration, reason:{ex.ToString()}. Set the value to default.");
                //this.MaxRestoreItemsThreads = 2;
                //this.MinRestoreItemsThreads = 1;
                //this.MaxTotalSizeOnDownload = 20;
                this.EnableBulkGenerateItems = true;
                this.MaxBulkItemsCount = 50;
                this.MaxBulkItemSize = 20;
                //this.SetApplicationImpersonation = true;
                //this.EWSMonitorMode = 3;
                //this.EWSMonitorInterval = 300;
            }
        }
        public BlockingCollection<ExchangeItem> GetSubItemsAsync(ExchangeFolder folder, string syncState)
        {
            var result = new BlockingCollection<ExchangeItem>();
            ThreadPool.QueueUserWorkItem(obj =>
            {
                using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.GetSubItemsAsync", addToStatistics: true))
                {
                    deleteItemIds = new List<string>();
                    try
                    {
                        const int pageSize = 512;   //use ms recommanded page count to improve performance.
                        int itemCount = 0;
                        //用于对Discover的Item去重, 目前只针对Incremental
                        var allItems = new HashSet<string>();
                        //如果使用Filter, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                        //var maxItemCount = CalculateMaxItemsCount(config.MaxBackupItemsThreads, folder.ItemsCount);
                        var incrementalSync = !string.IsNullOrEmpty(syncState);
                        logger.Info($"Start to sync items, incrementalSync: {incrementalSync}, syncState is {syncState}, total item count:{folder.ItemsCount}.");

                        bool moreAvailable = false;
                        do
                        {

                            #region 1. 根据pageSize和syncState获取folder下的item.
                            List<ExchangeItem> exchangeItems;
                            List<string> deletedItems;
                            moreAvailable = folder.SyncItems(pageSize, ref syncState, allItems, out exchangeItems, out deletedItems);
                            exchangeItems.ForEach(exItem => allItems.Add(exItem.ItemId));

                            itemCount += exchangeItems.Count;

                            exchangeItems.ForEach(exItem => result.Add(exItem));
                            #endregion
                            #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量导出
                            //var itemCollections = ItemEntityCollection.GroupCachedItems(
                            //    exchangeItems.Select(exItemArg => new ItemEntity(exItemArg) { FilterResult = ProcessFilter(exItemArg) }),
                            //    maxItemCount, config.MaxBulkItemSize * 1024 * 1024);
                            //itemCollections.ForEach(collectionArg =>
                            //{
                            //    result.Add(collectionArg);
                            //});
                            #endregion
                            #region 3. 记录delete item集合
                            deleteItemIds.AddRange(deletedItems);
                            #endregion
                            logger.Info($"Current item count is {result.Count}.");
                        } while (moreAvailable);
                        logger.Info("Finish sync items. Changed item count: {0}, deleted item count: {1}.", itemCount, deleteItemIds.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to sync items, error: {ex.ToString()}");
                    }
                    finally
                    {
                        result.CompleteAdding();
                    }
                }
            });
            return result;
        }


        //异步获取ExchangeItemGoup对象。目前hard code 每次取512 个（MS 建议值）
        //外围如果想修改分组数的大小，可以考虑重写ExchangeItemGroup.GroupCachedItems 方法，此方法用于分组
        //如果需要check rule，建议在分组结束之前check，这样保证组内数据一致（符合或者不符合），可以封装一个ExchangeItem +Rule 对象，用于ExchangeItemGroup 的返回值。
        public BlockingCollection<ExchangeItemGroup> GetGroupedItemsAsync(ExchangeFolder folder, string syncState)
        {
            var result = new BlockingCollection<ExchangeItemGroup>();
            ThreadPool.QueueUserWorkItem(obj =>
            {
                using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.GetGroupedItemsAsync", addToStatistics: true))
                {
                    deleteItemIds = new List<string>();
                    try
                    {
                        const int pageSize = 512;//use ms recommanded page count to improve performance.
                        int itemCount = 0;
                        //用于对Discover的Item去重, 目前只针对Incremental
                        var allItems = new HashSet<string>();
                        //如果需要check rule, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                        var maxItemCount = CalculateMaxItemsCount(this.MaxBackupItemsThreads, folder.ItemsCount);
                        var incrementalSync = !string.IsNullOrEmpty(syncState);
                        logger.Info($"Start to sync items, incrementalSync: {incrementalSync}, syncState is {syncState}, total item count:{folder.ItemsCount}.");

                        bool moreAvailable = false;
                        do
                        {
                            #region 1. 根据pageSize和syncState获取folder下的item.
                            List<ExchangeItem> exchangeItems;
                            List<string> deletedItems;
                            moreAvailable = folder.SyncItems(pageSize, ref syncState, allItems, out exchangeItems, out deletedItems);

                            exchangeItems.ForEach(exItem => allItems.Add(exItem.ItemId));
                            itemCount += exchangeItems.Count;

                            #endregion
                            #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量操作
                            using (new PerformanceScope("EXO.RMEXODiscoverHelper.GetGroupedItemsAsync.GroupCachedItems"))
                            {
                                var itemCollections = ExchangeItemGroup.GroupCachedItems(
                                exchangeItems,
                                maxItemCount, this.MaxBulkItemSize * 1024 * 1024);
                                itemCollections.ForEach(collectionArg =>
                                {
                                    result.Add(collectionArg);
                                });
                            }
                            #endregion
                            #region 3. 记录delete item集合
                            deleteItemIds.AddRange(deletedItems);
                            #endregion
                            logger.Info($"Current group count is : {result.Count}.");
                        } while (moreAvailable);
                        logger.Info("Finish sync items. Changed item count: {0}, deleted item count: {1}.", itemCount, deleteItemIds.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to sync items, error: {ex.ToString()}");
                    }
                    finally
                    {
                        result.CompleteAdding();
                    }
                }
            });
            return result;
        }


        public BlockingCollection<ExchangeItemGroup> GetGroupedDeleteItems(ExchangeFolder folder, string syncState)
        {
            var result = new BlockingCollection<ExchangeItemGroup>();

            using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.GetGroupedDeleteItemsAsync", addToStatistics: true))
            {
                deleteItemIds = new List<string>();
                try
                {
                    const int pageSize = 512;//use ms recommanded page count to improve performance.
                    int itemCount = 0;
                    //用于对Discover的Item去重, 目前只针对Incremental
                    var allItems = new HashSet<string>();
                    //如果需要check rule, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                    var maxItemCount = CalculateMaxItemsCount(this.MaxBackupItemsThreads, folder.ItemsCount);
                    var incrementalSync = !string.IsNullOrEmpty(syncState);
                    logger.Info($"Start to sync items, incrementalSync: {incrementalSync}, syncState is {syncState}, total item count:{folder.ItemsCount}.");

                    bool moreAvailable = false;
                    do
                    {
                        #region 1. 根据pageSize和syncState获取folder下的item.
                        //List<ExchangeItem> exchangeItems;
                        List<string> deletedItems;
                        moreAvailable = folder.SyncDeleteItems(pageSize, ref syncState, allItems, out deletedItems);

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
                        deleteItemIds.AddRange(deletedItems);
                        #endregion
                        logger.Info($"Current group count is : {result.Count}.");
                    } while (moreAvailable);
                    logger.Info("Finish sync items. Changed item count: {0}, deleted item count: {1}.", itemCount, deleteItemIds.Count);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to sync items, error: {ex.ToString()}");
                }
                finally
                {
                    result.CompleteAdding();
                }
            }
            return result;
        }

        //通过Search 方式异步获取所有的Item，底层调用FindItem API ， 可以传递SearchFilter。
        //Find方式目前暂不需要去重逻辑，Find方式无法与Inc 结合，需要外围自己写逻辑去cover
        public BlockingCollection<ExchangeItem> FindSubItemsAsync(ExchangeFolder folder, SearchFilter searchFilter = null)
        {
            var result = new BlockingCollection<ExchangeItem>();
            ThreadPool.QueueUserWorkItem(obj =>
            {
                using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.FindSubItemsAsync", addToStatistics: true))
                {
                    deleteItemIds = new List<string>();
                    try
                    {
                        const int pageSize = 100; //Get 100 search result back
                        int offset = 0;
                        int itemCount = 0;
                        bool moreAvailable = false;
                        do
                        {
                            #region 1. 根据pageSize和offset,通过Find 的方式获取folder下的item.
                            List<ExchangeItem> exchangeItems;
                            exchangeItems = folder.FindItems(pageSize, offset, out moreAvailable, searchFilter);
                            offset += pageSize;
                            itemCount += exchangeItems.Count;
                            exchangeItems.ForEach(exItem => result.Add(exItem));
                            #endregion
                            logger.Info($"Current items count is : {result.Count}.");
                        } while (moreAvailable);
                        logger.Info("Finish find items. Changed item count: {0}, deleted item count: {1}.", itemCount, deleteItemIds.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to find items, error: {ex.ToString()}");
                    }
                    finally
                    {
                        result.CompleteAdding();
                    }
                }
            });
            return result;
        }

        //通过Search 方式异步获取所有的Item，底层调用FindItem API ， 可以传递SearchFilter。
        //Find方式目前暂不需要去重逻辑，Find方式无法与Inc 结合，需要外围自己写逻辑去cover
        public BlockingCollection<ExchangeItemGroup> FindGroupedItemsAsync(ExchangeFolder folder, SearchFilter searchFilter = null)
        {
            var result = new BlockingCollection<ExchangeItemGroup>();
            ThreadPool.QueueUserWorkItem(obj =>
            {
                using (var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.FindGroupedItemsAsync", addToStatistics: true))
                {
                    deleteItemIds = new List<string>();
                    try
                    {
                        const int pageSize = 100; //Get 100 search result back
                        int offset = 0;
                        int itemCount = 0;
                        //如果使用Filter, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                        var maxItemCount = CalculateMaxItemsCount(this.MaxBackupItemsThreads, folder.ItemsCount);

                        bool moreAvailable = false;
                        do
                        {
                            #region 1. 根据pageSize和offset,通过Find 的方式获取folder下的item.
                            List<ExchangeItem> exchangeItems;
                            exchangeItems = folder.FindItems(pageSize, offset, out moreAvailable, searchFilter);
                            offset += pageSize;
                            itemCount += exchangeItems.Count;
                            #endregion
                            #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量导出
                            var itemCollections = ExchangeItemGroup.GroupCachedItems(
                                exchangeItems,
                                maxItemCount, this.MaxBulkItemSize * 1024 * 1024);
                            itemCollections.ForEach(collectionArg =>
                            {
                                result.Add(collectionArg);
                            });
                            #endregion
                            logger.Info($"Current group count is {result.Count}.");
                        } while (moreAvailable);
                        logger.Info("Finish find items. Changed item count: {0}, deleted item count: {1}.", itemCount, deleteItemIds.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to find items, error: {ex.ToString()}");
                    }
                    finally
                    {
                        result.CompleteAdding();
                    }
                }
            });
            return result;
        }

        public BlockingCollection<ExchangeItemGroup> FindGroupedItems(ExchangeFolder folder, SearchFilter searchFilter = null)
        {
            var result = new BlockingCollection<ExchangeItemGroup>();
            ThreadPool.QueueUserWorkItem(ob =>
            {
                using var performance = new PerformanceScope("EXO.RMEXOApplySettingBase.FindGroupedItemsAsync",
                    addToStatistics: true);
                deleteItemIds = new List<string>();
                try
                {
                    const int pageSize = 100; //Get 100 search result back
                    int offset = 0;
                    int itemCount = 0;
                    //如果使用Filter, 此处分组参数可能不是最优, 因为folder.ItemsCount不准确
                    var maxItemCount = CalculateMaxItemsCount(this.MaxBackupItemsThreads, folder.ItemsCount);

                    bool moreAvailable = false;
                    do
                    {
                        #region 1. 根据pageSize和offset,通过Find 的方式获取folder下的item.

                        List<ExchangeItem> exchangeItems;
                        if (ArchiverCommonStaticMethod.IsNestleCustomize)
                        {
                            logger.Info("Nestle customize, set IsNestleCustomize to true");
                            folder.IsNestleCustomize = true;
                        }

                        exchangeItems = folder.FindItems(pageSize, offset, out moreAvailable, searchFilter);
                        offset += pageSize;
                        itemCount += exchangeItems.Count;

                        #endregion

                        #region 2. 将#1中返回的Item集合分组并加入result collection中, 用于批量导出

                        var itemCollections = ExchangeItemGroup.GroupCachedItems(
                            exchangeItems,
                            maxItemCount, this.MaxBulkItemSize * 1024 * 1024);
                        itemCollections.ForEach(collectionArg =>
                        {
                            result.Add(collectionArg);
                        });

                        #endregion

                        logger.Info($"Current group count is {result.Count}.");
                    } while (moreAvailable);

                    logger.Info("Finish find items. Changed item count: {0}, deleted item count: {1}.", itemCount,
                        deleteItemIds.Count);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to find items, error: {ex.ToString()}");
                }
                finally
                {
                    result.CompleteAdding();
                }
            });
            
            return result;
        }


        /// <summary>
        /// 计算每组Item 最大数量
        /// </summary>
        /// <param name="consumerCount">最大线程数</param>
        /// <param name="total">item总数</param>
        /// <returns></returns>
        private int CalculateMaxItemsCount(int consumerCount, int total)
        {
            // item 数量不如线程数多,每个线程最多分一个item
            if (total <= consumerCount) return 1;
            // item数量不如总并发数多（线程数 X 每个线程最大item数），则每个线程平分Item
            if (total <= consumerCount * this.MaxBulkItemsCount) return total / consumerCount;
            // total/consumerCount > config.MaxItemsCount
            return this.MaxBulkItemsCount;
        }
    }
}
