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
using AvePoint.GCommon;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Wrapper.Common.MultiThread;
using AvePoint.Wrapper.Backup;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using System.Threading.Tasks;
using System.Diagnostics;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class MultiBackupController : IBackupController, IDisposable
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(MultiBackupController));
        private readonly IArchiverBackupDataWriter archiverBackupDataWriter;
        private readonly bool enableMulti;
        private readonly int threadNumber;
        private readonly int transferQueueNumber;
        private TaskThreadPool threadPool;
        private TransferBackupDataTask transferTask;
        private AveSPList lastList = null;
        private Guid disableIRMListId = Guid.Empty;

        public MultiBackupController(IArchiverBackupDataWriter archiverBackupDataWriter, int threadNumber, bool enable, int transferQueueNumber)
        {
            this.archiverBackupDataWriter = archiverBackupDataWriter;
            this.threadNumber = threadNumber;
            this.transferQueueNumber = transferQueueNumber;
            enableMulti = enable;
        }

        public async Task ProcessAsync(BackupNodeParameters nodeParameters)
        {
            CacheNode cacheNode = nodeParameters.CacheSPObjs.ParentValueInCacheOfLevel(nodeParameters.Node.CacheNodeType); //SAAS-14493 add IRM setting
            if (enableMulti)
            {
                if (nodeParameters.BackupObj is IMultiBackup)
                {
                    if (threadPool == null)
                    {
                        threadPool = new TaskThreadPool(threadNumber, "MultiBackup");
                        transferTask = new TransferBackupDataTask(archiverBackupDataWriter, transferQueueNumber);
                        threadPool.ExecuteTask(transferTask);
                    }

                    //nodeParameters.CacheNodeDisposeAction = nodeParameters.CacheNode.CustomizedDisposeAction;
                    //nodeParameters.CacheNode.CustomizedDisposeAction = null;

                    var task = new MultiBackupTask(nodeParameters);
                    long fileSize = nodeParameters.Node.DocumentSize;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    transferTask.AddBackupTask(task);
                    stopwatch.Stop();
                    Logger.Info("Performance Monitor MultiBackupController Process AddBackupTask WaitTime:{0}.", stopwatch.Elapsed);
                    threadPool.ExecuteTask(task);
                    //10GB
                    //10737418240
                    if (fileSize > 10737418240)
                    {
                        Logger.Info("Archiver MultiBackupController.File size over 10GB and ResetQueueCapacity 1.FileSize:{0}.", fileSize);
                        WaitForMultiBackup();
                        Logger.Info("Archiver Finished MultiBackupController.File size over 10GB and ResetQueueCapacity 1.FileSize:{0}.", fileSize);
                    }
                }
                else
                {
                    WaitForMultiBackup();
                    Logger.Info($"begin to backup entity :{nodeParameters.Node.NodeId} {nodeParameters.Node.CacheNodeType}");
                    cacheNode = nodeParameters.CacheSPObjs.ParentValueInCacheOfLevel(nodeParameters.Node.CacheNodeType); //SAAS-14493 add IRM setting
                    int backupResult = await BackupAsync(cacheNode, nodeParameters);
                    nodeParameters.CacheSPObjs.PutIn(nodeParameters.CacheNode, nodeParameters.Node.CacheNodeType, false);
                    Logger.Info("end to backup entity :{0}", nodeParameters.Node.NodeId);
                }
            }
            else
            {
                Logger.Info("begin to backup entity :{0}", nodeParameters.Node.NodeId);
                int backupResult = await BackupAsync(cacheNode, nodeParameters);
                nodeParameters.CacheSPObjs.PutIn(nodeParameters.CacheNode, nodeParameters.Node.CacheNodeType, false);
                Logger.Info($"end to backup entity :{nodeParameters.Node.NodeId} {nodeParameters.Node.CacheNodeType}");
            }
            if (AvePoint.Wrapper.Common.WrapperConfiguration.WrapperConfigurationForBPOS.DisableInformationRightsManagement && nodeParameters.Node.CacheNodeType == (int)CacheNodeType.List)
            {
                var aveWeb = cacheNode.WrapperObject as AveSPWeb;
                AveSPList currentList = new AveSPList(aveWeb, new Guid(nodeParameters.Node.NodeId), nodeParameters.Node.LeafName, true);
                if (disableIRMListId != currentList.Id)
                {
                    EnableListIRMSettings();
                }
                currentList.BeforeBackupItems();
                lastList = currentList;
                disableIRMListId = currentList.Id;
            }
        }

        private async Task<int> BackupAsync(CacheNode parent, BackupNodeParameters nodeParameters)
        {
            if (nodeParameters.Node.IsRepeatProcess)
            {
                if(nodeParameters.Configuration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB == true)
                {
                    return await nodeParameters.BackupObj.RepeatProcessContainerNode(parent, nodeParameters.CacheNode, nodeParameters.Node, nodeParameters.RuleName, nodeParameters.SubJobId, nodeParameters.RuleLevel, nodeParameters.MediaName, nodeParameters.Sender);
                }
                else
                {
                    Logger.Info($"Current virtual job is not latest vritual job of current sc, will skip process processed node:{nodeParameters.Node.NodeId}");
                    return (int)JobDetailsStatus.Successful;
                }
            }
            else
            {
                return await nodeParameters.BackupObj.BackupAsync(parent, nodeParameters.CacheNode, nodeParameters.Node, nodeParameters.RuleName, nodeParameters.SubJobId, nodeParameters.RuleLevel, nodeParameters.MediaName, nodeParameters.Sender);
            }
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        private void WaitForMultiBackup()
        {
            if (transferTask != null)
            {
                //ArchiverUtility.Logger(AveLogLevel.INFO, "wait for multi backup");
                transferTask.RaiseException();
                transferTask.WaitForTransferJob();
            }
        }

        public void Finish()
        {
            Dispose();
            EnableListIRMSettings();
        }

        public void Dispose()
        {
            WaitForMultiBackup();
            if (transferTask != null)
            {
                transferTask.Dispose();
            }
            if (threadPool != null)
            {
                threadPool.Dispose();
            }
            Logger.Info("backup end");
        }

        private void EnableListIRMSettings()
        {
            if (lastList != null)
            {
                lastList.AfterBackupItems();
                lastList = null;
                disableIRMListId = Guid.Empty;
            }
        }
    }
}
