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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Common;
using RAFileSystemCore.Common.JobHandler;
using System;
using System.IO;
using System.Threading;
using FSTreeNodeDto = AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto;
using SerializerHelper = AvePoint.GCommon.Utility.SerializerHelper;

namespace RAFileSystem.FileSystem.BaseProcessor
{
    internal abstract class FSProcessorWorkerBase : IScheduleJobWorker, ISupportsCancellation
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FSProcessorWorkerBase));
        private readonly IFSExecutionStrategy strategy;
        private FSJobProcessorContext context;

        private CancellationToken? _cancellationToken = null;

        protected FSProcessorWorkerBase(IFSExecutionStrategy strategy)
        {
            this.strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            logger.Info($"Initializing {strategy.GetType().Name}");
        }

        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void Bind(string msgStr)
        {
            try
            {
                context = BuildContext(msgStr);
                strategy.Initialize(context, logger);
                strategy.RegisterConnectionGroups(context);
                EnsureDirectoryExists(context);
                BuildRootStub(context);
                strategy.RegisterRootStub(context);
                InitializeJobController(context);
                strategy.FinalizeInitialization(context);
            }
            catch (FSSkipJobException)
            {
                throw;
            }
            catch (Exception ex)
            {
                strategy.HandleBindException(ex);
                logger.Error("Failed to initialize the file system from the tree node dto. Exception:{0}", ex);
                throw;
            }
        }

        public void Run()
        {
            try
            {
                logger.Info(GetJobStartMessage());
                BeforeExecute();

                if (_cancellationToken.HasValue)
                {
                    strategy.ExecuteAsync(_cancellationToken.Value).GetAwaiter().GetResult();
                }
                else
                {
                    strategy.ExecuteAsync().GetAwaiter().GetResult();
                }

                AfterExecute();
                CleanupJobContext();
                NotifyManagerFinalStatus();
                logger.Info(GetJobFinishMessage());
            }
            catch (FSSkipJobException ex)
            {
                logger.Info("No tree nodes found. Skipping job execution and notifying final status. Reason: {0}", ex.Message);
                var jobSummaryService = JobContext.Current.JobSummaryService;
                var jobId = JobContext.Current.JobId;
                CleanupJobContext();
                jobSummaryService.NotifyManager((int)JobStatus.Finished, jobId);
                logger.Info(GetJobFinishMessage());
            }
            catch (AgentJobStopException ex)
            {
                logger.Info("Agent job is stopped via OPUS. Reason: {0}", ex.Message);
                throw;
            }
        }

        protected virtual FSJobProcessorContext BuildContext(string msgStr)
        {
            FSJobMessage message = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
            if (message == null || message.FSTreeNodes == null || message.FSTreeNodes.Count == 0)
            {
                logger.Warn("FSJobMessage is invalid or contains no FSTreeNodes. Job will be skipped gracefully.");
                throw new FSSkipJobException("FSJobMessage is invalid or contains no FSTreeNodes.");
            }
            JobContext.Current.JobMessage = msgStr;
            var classificationLevel = (NodeLevel)message.ClassificationLevel;
            logger.Info("Init classification level:{0}", classificationLevel);
            var node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(message.FSTreeNodes[0]);
            var top3Nodes = ExternalUtil.FindTop3LevelNodes(node);
            logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID);

            string rootPath = top3Nodes.Item3.FullPath;
            FSJobCache.Instance.RootPath = rootPath.TrimEnd('\\');
            FSJobCache.Instance.RecordOwner = message.RecordOwner;
            FSJobCache.Instance.AveConnectionGroupId = new Guid(top3Nodes.Item2.ID);
            FSJobCache.Instance.AveConnectionId = new Guid(top3Nodes.Item3.ID);
            FSJobCache.Instance.ConnectionPath = rootPath;
            FSJobCache.Instance.CurrentNodeIsEnableRecordManagement = HybridApiClient.Instance.LoadFSNodeEnableRecordManagement(new Guid(node?.ID));
            JobContext.Current.BulkImportEnabled = message.BulkImportEnabled;
            JobContext.Current.BulkSize = message.BulkSize;
            FSJobCache.Instance.RunJobScopePath = node.FullPath;
            FSJobCache.Instance.RunJobParentScopePath = node.Parent == null ? node.FullPath : node.Parent.FullPath;
            FSJobCache.Instance.classCodeInfoDtoOnNode = message.ClassCodeDto;
            string highName = node.FullPath.Substring(rootPath.Length).Trim('\\');
            StorageInfo directoryInfo = new StorageInfo { HighName = highName };
            Guid settingScopeId = QueryScopeTermIdSetting(node);
            FSJobCache.Instance.DispoalSettingScopeId = settingScopeId;
            var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
            IXSystem system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);

            return CreateContext(message, node, top3Nodes, rootPath, system, directoryInfo, settingScopeId, setting, classificationLevel);
        }

        protected virtual FSJobProcessorContext CreateContext(
            FSJobMessage message,
            FSTreeNodeDto node,
            Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto> top3Nodes,
            string rootPath,
            IXSystem system,
            StorageInfo directoryInfo,
            Guid settingScopeId,
            FSSettingDto setting,
            NodeLevel classificationLevel)
        {
            return new FSJobProcessorContext(message, node, top3Nodes, rootPath, system, directoryInfo, settingScopeId, setting, classificationLevel);
        }

        protected virtual void BeforeExecute()
        {
        }

        protected virtual void AfterExecute()
        {
        }

        protected virtual string GetJobStartMessage()
        {
            return "Start FS job.";
        }

        protected virtual string GetJobFinishMessage()
        {
            return "Finished FS job.";
        }

        protected Guid GetTermId4Folder(FSJobMessage message, FSSettingDto setting)
        {
            Guid folderTermId;
            if (setting.NeedCheckDefaultValue || string.IsNullOrWhiteSpace(message.FolderTermId))
            {
                folderTermId = setting.DefaultTermId;
            }
            else if (Guid.TryParse(message.FolderTermId, out Guid termId))
            {
                folderTermId = termId;
            }
            else
            {
                folderTermId = setting.DefaultTermId;
            }

            logger.Info($"Get folder termId:{folderTermId}");
            return folderTermId;
        }

        protected static Guid QueryScopeTermIdSetting(FSTreeNodeDto node)
        {
            Guid scopeId = node.Level == NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : new Guid(node.ID);
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(scopeId))
            {
                return scopeId;
            }

            if (node.Parent != null)
            {
                return QueryScopeTermIdSetting(node.Parent);
            }

            return Guid.Empty;
        }

        private void EnsureDirectoryExists(FSJobProcessorContext currentContext)
        {
            if (currentContext.System.DirectoryExists(currentContext.DirectoryInfo))
            {
                return;
            }

            strategy.HandleMissingDirectory(currentContext);
            throw new FileNotFoundException("We cannot open the Dir" + currentContext.Node.FullPath);
        }

        private void BuildRootStub(FSJobProcessorContext currentContext)
        {
            XDirectoryInfo directory = currentContext.System.OpenDirectory(currentContext.DirectoryInfo, FileMode.Open);
            Guid parentId = string.IsNullOrEmpty(currentContext.DirectoryInfo.HighName)
                ? new Guid(currentContext.Top3Nodes.Item2.ID)
                : ExternalUtil.CombinePath(
                    FSJobCache.Instance.RootPath,
                    Path.GetDirectoryName(ExternalUtil.CombinePath(directory.HighName, directory.LowName)))
                    .ToLowerInvariant()
                    .ToMd5();

            string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, directory.HighName, directory.LowName);
            string termName = FSJobCache.Instance.Terms.ContainsKey(currentContext.Setting.DefaultTermId)
                ? FSJobCache.Instance.Terms[currentContext.Setting.DefaultTermId].Name
                : null;

            currentContext.Directory = directory;
            currentContext.RootStub = new FSFolderStub
            {
                MediaObj = directory,
                FullPath = fullPath,
                SelfId = fullPath.ToLowerInvariant().ToMd5(),
                ParentId = parentId,
                ScopeSettingId = currentContext.SettingScopeId,
                TermId4Folder = GetTermId4Folder(currentContext.Message, currentContext.Setting),
                TermName4Folder = termName,
                Depth = 0
            };
        }

        private void InitializeJobController(FSJobProcessorContext currentContext)
        {
            FSJobCache.Instance.JobController.InitJob(
                currentContext.Setting,
                currentContext.RootStub.FullPath.ToLowerInvariant().ToMd5(),
                currentContext.RootStub.FullPath,
                currentContext.Message,
                currentContext.Directory.Name);

            JobContext.Current.mProgressManager.Create().IncreaseBase(3);
        }

        private void CleanupJobContext()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while cleaning up. Error: {0}", ex);
                FSJobCache.Instance.FailedCount++;
                JobContext.Current.HasErrorNode = true;
            }
        }

        private void NotifyManagerFinalStatus()
        {
            var jobContext = JobContext.Current;
            var cache = FSJobCache.Instance;
            int status = CalculateFinalJobStatus(jobContext.AllErrorNode, jobContext.HasErrorNode, cache.SuccessCount, cache.FailedCount);
            jobContext.JobSummaryService.NotifyManager(status, jobContext.JobId);
        }

        private int CalculateFinalJobStatus(bool allError, bool hasError, int success, int failed)
        {
            if (allError)
            {
                return (int)JobStatus.Failed;
            }

            if (hasError || (success > 0 && failed > 0))
            {
                return (int)JobStatus.FinishWithException;
            }

            if (success == 0 && failed > 0)
            {
                return (int)JobStatus.Failed;
            }

            return (int)JobStatus.Finished;
        }
    }
}