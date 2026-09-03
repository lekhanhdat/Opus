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
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Bulk
{
    public interface ICosmosBulkOperator
    {
        /// <summary>
        /// 初始化相关设置.
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="updateSucceedAction"></param>
        /// <param name="updateFailedAction"></param>
        void Start(int bufferSize, Func<Record, Task> updateSucceedAction, Action<Record, Exception> updateFailedAction);
        /// <summary>
        /// 添加一个record到处理列表中
        /// </summary>
        /// <param name="record"></param>
        void Add(Record record);
        /// <summary>
        /// 结束相关操作，包括stop内部线程，把剩余未处理的数据都给处理掉
        /// </summary>
        void Complete();
        /// <summary>
        /// 外围可能会多次调用Complete方法，需要重置_isCompleted，_exitEvent
        /// </summary>
        void Reset();
    }

    public class CosmosBulkOperator : ICosmosBulkOperator, IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(CosmosBulkOperator));
        public const int DefualtBufferSize = 5;
        private int _bufferSize;
        private bool _isCompleted = false;

        private readonly SemaphoreSlim Semaphore = new(1);

        private readonly static object locker = new object();
        private Func<Record, Task> _updateSucceedAction;
        private Action<Record, Exception> _updateFailedAction;
        private Queue _dataQueue = null;

        private IExplorerDao _explorerDao;

        private IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    logger.Info($"Start init ExplorerDao.");
                    this._explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return this._explorerDao;
            }
        }
        private static ICosmosBulkOperator _instance;
        public static ICosmosBulkOperator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new CosmosBulkOperator();
                        }
                    }
                }
                return _instance;
            }
        }

        public CosmosBulkOperator()
        {
            this._dataQueue = Queue.Synchronized(new Queue());
        }

        public void Start(int bufferSize, Func<Record, Task> updateSucceedAction, Action<Record, Exception> updateFailedAction)
        {
            _bufferSize = bufferSize > 0 ? bufferSize : DefualtBufferSize;
            _updateSucceedAction = updateSucceedAction;
            _updateFailedAction = updateFailedAction;
            AveTenantThread updateDetail = new AveTenantThread(new ThreadStart(ProcessData));
            updateDetail.IsBackground = true;
            updateDetail.Start();
        }

        public void Add(Record record)
        {
            if (record != null) _dataQueue.Enqueue(record);
        }

        public void Complete()
        {
            try
            {
                _isCompleted = true;

                logger.Info($"wait ProcessData thread end.");

                Semaphore.Wait();

                logger.Info("Cosmos bulk operate end.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while completing the bulk operation. error: {e.ToString()}");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public void Reset()
        {
            if(!_isCompleted || _dataQueue.Count > 0)
            {
                try
                {
                    if (Semaphore.CurrentCount > 0)
                    {
                        Semaphore.Wait();
                    }
                }
                finally
                {
                    Semaphore.Release();
                }
            }
            _isCompleted = false;
        }

        private void ProcessData()
        {
            logger.Info("ProcessData thread start.");

            try
            {
                Semaphore.Wait();
                while (!_isCompleted || _dataQueue.Count > 0)
                {
                    try
                    {
                        if (_dataQueue.Count >= _bufferSize || _isCompleted)
                        {
                            logger.Info($"DataQueue count before action is : {_dataQueue.Count}");
                            var records = GetRecords();
                            UpdateAsync(records).Wait();
                            logger.Info($"DataQueue count after action is : {_dataQueue.Count}");
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occurred while process data. error: {e.ToString()}");
                    }
                }
                logger.Info("ProcessData thread end.");
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while process data. Error: {e}");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task UpdateAsync(List<Record> records)
        {
            using (new RA.Common.PerformanceScope("CosmosBulkOperator.Update", addToStatistics: true))
            {
                try
                {
                    if (records.Count == 0) return;
                    var failedRecords = ExplorerDao.BulkUpsert(records);
                    if (_updateFailedAction != null && failedRecords.Count > 0)
                    {
                        ExecuteFailedAction(_updateFailedAction, failedRecords);
                    }

                    if (_updateSucceedAction != null)
                    {
                        if (failedRecords.Count == 0)
                        {
                            await ExecuteActionAsync(_updateSucceedAction, records);
                        }
                        else
                        {
                            var failedIds = failedRecords.Select(r => r.Item1.Id);
                            await ExecuteActionAsync(_updateSucceedAction, records.Where(r => !failedIds.Contains(r.Id)).ToList());
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while bulk update records. error: {e.ToString()}");
                }
            }
        }

        private Task ExecuteActionAsync(Func<Record, Task> action, List<Record> records)
        {
            return records.ForEachAsync(async record =>
            {
                try
                {
                    await action(record);
                }
                catch (Exception e)
                {
                    logger.Warn($"error in ExecuteAction: {e.ToString()}");
                }
            });
        }

        private void ExecuteFailedAction(Action<Record, Exception> action, List<(Record, Exception)> records)
        {
            records.ForEach(record =>
            {
                try
                {
                    action(record.Item1, record.Item2);
                }
                catch (Exception e)
                {
                    logger.Warn($"error in ExecuteFailedAction: {e.ToString()}");
                }
            });
        }

        private List<Record> GetRecords(bool getAllData = false)
        {
            var records = new List<Record>();
            var count = getAllData ? _dataQueue.Count : Math.Min(_dataQueue.Count, _bufferSize);
            for (int i = 0; i < count && _dataQueue.Count > 0; i++)
            {
                records.Add(_dataQueue.Dequeue() as Record);
            }
            return records;
        }

        public void Dispose()
        {
            if (Semaphore != null)
            {
                try
                {
                    Semaphore.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while dispose res: {e}");
                }
            }
        }
    }
}
