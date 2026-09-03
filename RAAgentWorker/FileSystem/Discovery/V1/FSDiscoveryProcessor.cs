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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Discovery.V1.Analyzer;
using RAFileSystem.FileSystem.Discovery.V1.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Discovery
{
    public class FSDiscoveryProcessor : IScheduleJobWorker
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DEFAULT_THREAD_COUNT = 5;

        private FSDiscoveryDataAnalyzer _discoveryDataAnalyzer;

        private FSDiscoveryDataQueue _dataQueue;

        private KeyValuePair<string, Guid> _connectionCache;

        private string _rootPath;

        public FSDiscoveryProcessor()
        {
            _dataQueue = new FSDiscoveryDataQueue();
        }

        public void Bind(string msgStr)
        {
            try
            {
                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                _connectionCache = msg.ConnectionCache.FirstOrDefault();
                if (string.IsNullOrEmpty(_connectionCache.Key) || string.IsNullOrEmpty(_connectionCache.Value.ToString()))
                {
                    throw new ArgumentNullException($"Could not found the connection info. Job Id [{msg.JobId}], Job Type [{msg.JobType}].");
                }
                JobContext.Current.JobMessage = msgStr;
                _rootPath = _connectionCache.Key.TrimEnd('\\');
                IXSystem _system = ExternalUtil.OpenXSystem(_rootPath);
                if (!_system.DirectoryExists(new StorageInfo()))
                {
                    throw new FileNotFoundException($"Could not found the directory, root path [{_rootPath}].");
                }
                _discoveryDataAnalyzer = new FSDiscoveryDataAnalyzer(_connectionCache.Value);
            }
            catch (Exception ex)
            {
                s_logger.Error("Failed to initialize the directory. Exception:{0}", ex.ToString());
                throw;
            }
        }

        public void Run()
        {
            try
            {
                ExecuteScanAndAnalyze().GetAwaiter().GetResult();
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
            }
            catch (Exception ex)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Failed, JobContext.Current.JobId);
            }
            finally
            {
                JobContext.Current.Cleanup();
            }
        }

        private async Task ExecuteScanAndAnalyze()
        {
            try
            {
                s_logger.Info($"Begin execute scanning and analyzing process. Connection ID [{_connectionCache.Value}].");
                _discoveryDataAnalyzer.Init();
                bool isEmptyConnection = !Directory.EnumerateFileSystemEntries(_rootPath, "*", SearchOption.AllDirectories).Any();
                if (isEmptyConnection)
                {
                    s_logger.Info($"The connection is empty, no files to scan. Connection ID [{_connectionCache.Value}].");
                    _discoveryDataAnalyzer.Analyze(null, null);
                    return;
                }
                IFSDiscoveryDataProcessor producer = new FSDiscoveryDataProducer(_dataQueue, _rootPath);
                Task producingTask = Task.Run(() => producer.Execute());
                Task[] consumingTasks = new Task[DEFAULT_THREAD_COUNT];
                for (int i = 0; i < consumingTasks.Length; i++)
                {
                    consumingTasks[i] = Task.Run(() =>
                    {
                        FSDiscoveryDataConsumer consumer = new FSDiscoveryDataConsumer(_dataQueue, _discoveryDataAnalyzer);
                        consumer.Execute();
                    });
                }
                await producingTask;
                await Task.WhenAll(consumingTasks);
                s_logger.Info($"End of scanning and analyzing process. Connection ID [{_connectionCache.Value}], Processed files count [{_discoveryDataAnalyzer.GetProcessedFileCount()}].");
            }
            catch (Exception ex)
            {
                s_logger.Error($"An error occurred during scan & analyze FS data process, Connection ID [{_connectionCache.Value}]. Ex: {ex.Message}.");
                throw;
            }
            finally
            {
                _discoveryDataAnalyzer.CommitAnalyzedFile();
            }
        }
    }
}
