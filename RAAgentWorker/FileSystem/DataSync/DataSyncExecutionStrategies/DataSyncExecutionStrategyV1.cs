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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.DataSync.Utils;

namespace RAFileSystem.FileSystem.DataSync.DataSyncExecutionStrategies
{
    internal class DataSyncExecutionStrategyV1 : IFSExecutionStrategy
    {
        private AveLogger _logger;
        
        private IReportService<JMJobDetails> JobDetailService { get; set; }

        public void Initialize(FSJobProcessorContext context, AveLogger logger)
        {
            _logger = logger;
            var dataSyncContext = context as FSDataSyncJobContext;
            if (dataSyncContext == null)
            {
                throw new ArgumentException("Data sync strategy requires a data sync job context.", nameof(context));
            }

            FSDataCollector.ClassificationLevel = dataSyncContext.ClassificationLevel;
            JobContext.Current.EnableFSHighPerformanceMode = false;
        }

        public void RegisterConnectionGroups(FSJobProcessorContext context)
        {
            FSJobCache.Instance.AnalyzerCache.Add(new FSConnectionGroupsStub
            {
                FullPath = context.Top3Nodes.Item1.Name,
                SelfId = new Guid(context.Top3Nodes.Item1.ID)
            });

            FSJobCache.Instance.AnalyzerCache.Add(new FSConnectionGroupStub
            {
                FullPath = context.Top3Nodes.Item2.Name,
                SelfId = new Guid(context.Top3Nodes.Item2.ID),
                ParentId = new Guid(context.Top3Nodes.Item1.ID)
            });
        }

        public void RegisterRootStub(FSJobProcessorContext context)
        {
            FSJobCache.Instance.DiscoveryCache.Add(context.RootStub);
        }

        public void FinalizeInitialization(FSJobProcessorContext context)
        {
        }

        public void HandleMissingDirectory(FSJobProcessorContext context)
        {
            JobContext.Current.JobDetailManager.Create().Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = Path.GetFileName(context.Node.FullPath),
                FullPath = context.Node.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_PathCanNotAccess"
            });
            JobContext.Current.HasErrorNode = true;
            FSJobCache.Instance.FailedCount++;
        }

        public void HandleBindException(Exception exception)
        {
        }

        public Task ExecuteAsync()
        {
            JobDetailService = JobContext.Current.JobDetailManager.Create();

            StartDiscoveryThread();
            Thread.Sleep(1000);
            StartAnalyzerThread();
            Thread.Sleep(1000);
            StartPersistThread();
            Thread.Sleep(1000);

            WaitForDiscoveryThreadExit();
            WaitForAnalyzerThreadExit();
            WaitForPersistThreadExit();

            return Task.CompletedTask;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync();
        }

        private void WaitForDiscoveryThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (FSJobCache.Instance.AnalyzerCache.Count == 0 && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0)
                {
                    _logger.Info("There is no discovery thread running now..");
                    break;
                }
            }
        }

        private void WaitForAnalyzerThreadExit()
        {
            while (true)
            {
                if (FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                {
                    break;
                }
                Thread.Sleep(3000);
            }
        }

        private void WaitForPersistThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0)
                {
                    _logger.Info("There is no serializer thread running now...");
                    break;
                }
            }
        }

        private void StartPersistThread()
        {
            int serializerThreadCount = 1;
            try
            {
                serializerThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.PersistThreadCount));
                _logger.Info("serializerThreadCount is " + serializerThreadCount);
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while gettting serializerThreadCount.Error:{0}", e.ToString());
            }

            for (int i = 0; i < serializerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DataSyncRecordsSerializer serializer = new DataSyncRecordsSerializer();
                    serializer.Run();
                });
            }
        }

        private void StartAnalyzerThread()
        {
            int analyzerThreadCount = 1;
            try
            {
                analyzerThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.AnalyzerThreadCount));
                _logger.Info("analyzerThreadCount is " + analyzerThreadCount);
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while gettting analyzerThreadCount.Error:{0}", e.ToString());
            }

            for (int i = 0; i < analyzerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DataSyncAnalyzerThread analyzer = new DataSyncAnalyzerThread();
                    analyzer.Run();
                });
            }
        }

        private void StartDiscoveryThread()
        {
            int discoveryThreadCount = 1;
            try
            {
                discoveryThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.DiscoveryThreadCount));
                _logger.Info("discoveryThreadCount is " + discoveryThreadCount);
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while gettting discoveryThreadCount.Error:{0}", e.ToString());
            }

            for (int i = 0; i < discoveryThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    FSDiscover discovery = new FSDiscover();
                    discovery.Run();
                });
            }
        }
    }
}
