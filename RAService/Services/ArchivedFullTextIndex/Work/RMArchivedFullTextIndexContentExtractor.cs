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
using AvePoint.GCommon.Contract.CloudAppAdmin.Message;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RAProcess;
using AvePoint.RA.Common.RAProcess.Extractor;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using Newtonsoft.Json;
using NVelocity.Tool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Util.AI.Text.Extractor;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexContentExtractor
    {
        private const int CAPACITY = 20;

        private readonly RALogger _logger;

        private readonly Channel<RMArchivedFullTextIndexDataInfo> _dataChannel;

        private readonly Channel<(RMArchivedFullTextIndexDataInfo dataInfo, string filePath)> _extractChannel;

        private readonly int _letterCountLimit;

        private readonly int _threadCountLimit;

        private readonly bool _noLimit;

        private readonly TimeSpan _extractTimeout;

        private volatile bool _result = true;

        private readonly object _resultLock = new object();

        private readonly Extractor _extractor;

        public RMArchivedFullTextIndexContentExtractor(
            int letterCountLimit,
            int threadCountLimit,
            bool noLimit,
            TimeSpan extractTimeout)
        {
            _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexContentExtractor));
            Directory.CreateDirectory(RMArchivedFullTextIndexDefinition.MESSAGE_PATH);
            Directory.CreateDirectory(RMArchivedFullTextIndexDefinition.Result_PATH);
            _dataChannel = Channel.CreateBounded<RMArchivedFullTextIndexDataInfo>(new BoundedChannelOptions(CAPACITY)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = true,
            });
            _extractChannel = Channel.CreateBounded<(RMArchivedFullTextIndexDataInfo dataInfo, string filePath)>(new BoundedChannelOptions(CAPACITY)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = true,
                SingleReader = false,
            });
            _letterCountLimit = letterCountLimit;
            _threadCountLimit = threadCountLimit;
            _noLimit = noLimit;
            _extractTimeout = extractTimeout;
            _extractor = new Extractor();
            _ = ProcessAsync();
        }

        public async Task AddDataAsync(RMArchivedFullTextIndexDataInfo dataInfo)
        {
            await _dataChannel.Writer.WriteAsync(dataInfo);
        }

        public async Task AddNeedExtractDataAsync(RMArchivedFullTextIndexDataInfo dataInfo, string filePath)
        {
            await _extractChannel.Writer.WriteAsync((dataInfo, filePath));
        }

        public async IAsyncEnumerable<RMArchivedFullTextIndexDataInfo> GetAllDataAsync()
        {
            await foreach (var data in _dataChannel.Reader.ReadAllAsync())
            {
                yield return data;
            }
        }

        private async Task ProcessAsync()
        {
            var workerTasks = new List<Task>(_threadCountLimit);
            for (int i = 0; i < _threadCountLimit; i++)
            {
                workerTasks.Add(ProcessMessagesAsync());
            }
            await Task.WhenAll(workerTasks);
            _dataChannel.Writer.Complete();
        }

        private async Task ProcessMessagesAsync()
        {
            await foreach (var (dataInfo, filePath) in _extractChannel.Reader.ReadAllAsync())
            {
                await ProcessMessageAsync(dataInfo, filePath);
            }
        }
        private async Task ProcessMessageAsync(RMArchivedFullTextIndexDataInfo dataInfo, string filePath)
        {
            try
            {
                var content = await ExtractContentAsync(dataInfo, filePath);
                dataInfo.Content = content;
                await _dataChannel.Writer.WriteAsync(dataInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while extract file [{dataInfo.FullPath}], FileSize: [{dataInfo.FileSize}] Error: {e}");
                lock (_resultLock)
                {
                    _result &= false;
                }
                dataInfo.Content = string.Empty;
                await _dataChannel.Writer.WriteAsync(dataInfo);
            }
            finally
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"An error occurred while delete temp extract file [{filePath}], Error: {ex}");
                }
            }
        }

        private async Task<string> ExtractContentAsync(RMArchivedFullTextIndexDataInfo dataInfo, string filePath)
        {
            using (new PerformanceScope("Extract file content", $"File: [{dataInfo.FullPath}] FileSize: [{dataInfo.FileSize}]", true))
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var extractTask = Task.Run(async () =>
                    {
                        return await _extractor.ExtractAsync(fileStream, dataInfo.FileType, new ExtractOption
                        {
                            MaxCharsCountPerFile = _noLimit ? int.MaxValue : _letterCountLimit,
                            FastMode = true,
                        });
                    });

                    using (var cts = new CancellationTokenSource(_extractTimeout))
                    {
                        return await extractTask.WaitAsync(cts.Token);
                    }
                }
            }
        }

        public void SetNoDataNeedsExtract()
        {
            _extractChannel.Writer.Complete();
        }

        public async Task CompleteAsync()
        {
            await _extractChannel.Reader.Completion;
        }
        public bool GetResult()
        {
            lock (_resultLock)
            {
                return _result;
            }
        }
    }
}
