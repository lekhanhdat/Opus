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



using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.RecordsDisposal.Action.DeleteOnly;
using System.Reflection;
using Util;

namespace RAGoogle.Archive
{
    internal class MessageProcessor : IDisposable
    {
        #region Private fields
        private AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int maxCacheSize = 1000;
        private DataQueue<string> _messageContainer { get; set; }
        private GoogleConfiguration _config { get; set; }
        private Task _processTask { get; set; }
        private CancellationTokenSource _cts { get; set; }
        public int MaxDegreeOfParallelism => 5;

        #endregion

        #region constructor
        public MessageProcessor(GoogleConfiguration config, CancellationTokenSource cts)
        {
            this._config = config;
            _cts = cts;
            maxCacheSize = config.BackgroundSettings.MaxDeletionCacheSize;
            _messageContainer = new DataQueue<string>(maxCacheSize);
        }
        #endregion

        #region Methords
        /// <summary>
        /// store response message from media,此方法会立即返回
        /// </summary>
        public async Task SaveXmlHeaderAsync(string message)
        {
            await _messageContainer.WriteAsync(message);
            if (message.Equals("End", StringComparison.OrdinalIgnoreCase))//
            {
                _messageContainer.Complete();
            }
        }
        public async Task StartProcessAsync()
        {
            _processTask = Task.Run(ProcessorAsync);
        }
        /// <summary>
        /// 等待Delete线程退出
        /// </summary>
        public async Task WaitingForCompleted()
        {
            if (_processTask == null) return;
            Task.WaitAll([_processTask], _cts.Token);
            await _processTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.Error("MessageProcessor task faulted.", t.Exception);
                }
                else if (t.IsCanceled)
                {
                    logger.Warn("MessageProcessor task was canceled.");
                }
                else
                {
                    logger.Info("MessageProcessor task completed successfully.");
                }
            }, _cts.Token);
        }

        public void Dispose()
        {
            WaitingForCompleted().GetAwaiter().GetResult();
        }
        public async Task ProcessorAsync()
        {
            try
            {
                await _messageContainer.ToIEnumerable().ParallelExecute(async item =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("MessageProcessor:ProcessorAsync"))
                            {
                                var _deleteContorller = new DeleteOnlyController(this._config, null);
                                await _deleteContorller.ProcessStringItemAsync(item);
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        logger.Warn("The records disposal job has been stopped.");
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while process item {item}. Error: {ex}");
                    }
                }, MaxDegreeOfParallelism, _cts.Token);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to process items to dete, exception:{e}");
            }
        }
        #endregion
    }
}