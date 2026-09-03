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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using RAFileSystem.Disposal;
using RAFileSystem.Disposal.Archive;
using RAFileSystem.FileSystem.BaseProcessor;

namespace RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies
{
    internal class DisposalExecutionStrategyV1 : BaseDisposalExecutionStrategy, IFSExecutionStrategy
    {
        private AveLogger _logger;
        private FSJobProcessorContext _context;

        public void Initialize(FSJobProcessorContext context, AveLogger logger)
        {
            _logger = logger;
            _context = context;
            FSDataDisposal.ClassificationLevel = context.ClassificationLevel;
            FSDataDisposal.currentSetting = context.Setting;
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
            FSDataDisposal._rootStub = context.RootStub;
            FSJobCache.Instance.DisposalFSFolderCache.Add(context.RootStub);
            FSJobCache.Instance.AnalyzerCache.Add(context.RootStub);
        }

        public void FinalizeInitialization(FSJobProcessorContext context)
        {
            // No additional finalization needed for V1 legacy mode
        }

        public void HandleMissingDirectory(FSJobProcessorContext context)
        {
            JobContext.Current.JobDetailManager.Create().Commit(new JMFSDisposalJobDetails
            {
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                ObjectName = Path.GetFileName(context.Node.FullPath),
                SourceLocation = context.Node.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_PathCanNotAccess"
            });
        }

        public void HandleBindException(Exception exception)
        {
            // No cleanup needed for V1 legacy mode
        }

        public async Task ExecuteAsync()
        {
            try
            {
                if (FSDataDisposal.ClassificationLevel == NodeLevel.FSFile)
                {
                    ExecuteFileLevelClassification();
                }
                else
                {
                    ExecuteFolderLevelClassification();
                }
            }
            catch (Exception e)
            {
                FSJobCache.Instance.FailedCount++;
                _logger.Error($"Error occurred while running disposal job. Error:{e.ToString()}");
            }
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsync();
        }

        private void ExecuteFileLevelClassification()
        {
            GetAllRecords();
            var allFolderCache = GetDisposalDiscoverFolders();
            if (allFolderCache != null && allFolderCache.Count > 0)
            {
                FSJobCache.Instance.DisposalFolderCache.AddBatch(allFolderCache.AsEnumerable());
                StartSubThreads();
            }
            else
            {
                _logger.Warn("No available folder path, skip running job.");
            }
        }

        private void ExecuteFolderLevelClassification()
        {
            var allExceptFolderCache = GetAllFolders(_context);
            FSJobCache.Instance.DisposalDifferentFolderCache.AddRange(allExceptFolderCache.AsEnumerable());
            StartSubThreads();
        }

        private void StartSubThreads()
        {
            StartDiscoveryThread();
            Thread.Sleep(2000);
            StartWorkerThread();
            Thread.Sleep(2000);
            StartReportThread();
            Thread.Sleep(2000);
            WaitForDiscoveryThreadExit();
            WaitForAnalyzerThreadExit();
            WaitForPersistThreadExit();
            RunSendEmailJob();
        }

        private void RunSendEmailJob()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0
                    && FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count == 0
                    && FSJobCache.Instance.DisposalScanCache.Count == 0
                    && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                    && FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count == 0
                    && FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                {
                    _logger.Info("There is no send email serializer thread running now...");
                    JobContext.Current.ApiClient.RunSendEmailJob(JobContext.Current.JobId);
                    break;
                }
            }
        }

        private void WaitForDiscoveryThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                _logger.Debug("{0}, {1}, {2}",
                    FSJobCache.Instance.DisposalScanCache.Count,
                    FSJobCache.Instance.DiscoverThreadMonitor.Count,
                    FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count);
                if (FSJobCache.Instance.DisposalScanCache.Count == 0
                    && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                    && FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count == 0)
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
                _logger.Debug("{0},", FSJobCache.Instance.AnalyzerThreadMonitor.Count);
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
                _logger.Debug("{0}, {1}",
                    FSJobCache.Instance.SerializerThreadMonitor.Count,
                    FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count);
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0
                    && FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count == 0)
                {
                    _logger.Info("There is no serializer thread running now...");
                    break;
                }
            }
        }

        private void StartReportThread()
        {
            int serializerThreadCount = 1;
            for (int i = 0; i < serializerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalDataUpdater serializer = new DisposalDataUpdater();
                    serializer.Run();
                });
            }
        }

        private void StartWorkerThread()
        {
            int analyzerThreadCount = 1;
            for (int i = 0; i < analyzerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalWorker analyzer = new DisposalWorker();
                    analyzer.Run();
                });
            }
        }

        private void StartDiscoveryThread()
        {
            int discoveryThreadCount = 1;
            for (int i = 0; i < discoveryThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalDiscover discovery = new DisposalDiscover();
                    discovery.Run();
                });
            }
        }
    }
}
