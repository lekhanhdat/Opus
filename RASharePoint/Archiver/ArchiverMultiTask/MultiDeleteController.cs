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
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using System.Collections.Generic;
using AvePoint.Wrapper.Common.MultiThread;
using System;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Linq;
using HSMCommon;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.GCommon;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class MultiDeleteController : IMultiDeleteController
    {
        private readonly int threadNumber;
        private readonly bool enableMulti;
        private readonly ScheduleConfiguration configuration;
        private TaskThreadPool threadPool;
        //private DeletionNode lastNode;
        private List<DeletionNode> deletionInfos;
        private string lastNodeSPId = string.Empty;
        private int currentTaskDocumentCount = 0;
        private static AveLogger mLog = AveLogger.GetInstance(typeof(MultiDeleteController));
        private HSMConnector HSMConnectorInstance = null;
        private HSMConnector HSMConnector
        {
            get
            {
                if (HSMConnectorInstance == null)
                {
                    HSMConnectorInstance = HSMConnector.GetInstance(configuration);
                }
                return HSMConnectorInstance;
            }
        }

        public MultiDeleteController(ScheduleConfiguration config, int threadNumber, bool enable)
        {
            this.threadNumber = threadNumber;
            enableMulti = enable;
            configuration = config;
            CreateLinkFileByPackage.GetInstance(configuration).Init();
        }

        public void Process(DeletionNode deletionNode, ArchiverDeletion deleteAction)
        {
            if (deletionNode.IsValid)
            {
                bool delteWithNoBackup = configuration.actionType == ActionType.DeleteOnly || configuration.actionType == ActionType.ExportBeforeDelete || configuration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;

                //当前判断只有Cloud Archiver Leave Stub 且勾选RemoveMetadata时才能走到，此处多线程并行Level为Folder，即可以多folder下数据同时Leave Stub.
                if (enableMulti
                    && ((configuration.currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument)
                    && configuration.currentRule.IsLeaveStubRemoveMetadata
                    && !configuration.IsILMode
                    && (deletionNode.ObjectType == AveConstants.TYPE_LISTITEM ||
                        deletionNode.ObjectType == AveConstants.TYPE_VERSION ||
                        deletionNode.ObjectType == AveConstants.TYPE_DOCUMENT ||
                        deletionNode.ObjectType == AveConstants.TYPE_LISTITEMVERSION ||
                        deletionNode.ObjectType == AveConstants.TYPE_FOLDER ||
                        deletionNode.ObjectType == AveConstants.TYPE_FOLDER_VERSION))
                {
                    if (threadPool == null)
                    {
                        threadPool = new TaskThreadPool(threadNumber, "MultiDelete");
                    }
                    deleteAction.ActiveInPlaceRecordsFeature(deletionNode);
                    configuration.InitDeletionContainer(deletionNode.HeaderInfo.GetAttribute(KeyWord.SiteUrl), new Guid(deletionNode.HeaderInfo.GetAttribute(KeyWord.WebId)), new Guid(deletionNode.HeaderInfo.GetAttribute(KeyWord.ListId)));
                    if (LinkFileCommon.IsLeaveStubRule(configuration.currentRule))
                    {
                        configuration.InitStubAveBackupContainer();
                        //configuration.InitStubAveRestoreContainer();
                    }

                    if (!configuration.EnableDeleteDocumentBatchOptimization)
                    {
                        var currentNodeSpId = deletionNode.SPId;
                        if (string.Compare(currentNodeSpId, lastNodeSPId, System.StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (deletionInfos == null)
                            {
                                deletionInfos = new List<DeletionNode>();
                            }
                            deletionInfos.Add(deletionNode);
                        }
                        else
                        {
                            if (deletionInfos != null && deletionInfos.Count > 0)
                            {
                                var deleteTask = new MultiDeleteTask(deletionInfos, new ArchiverDeletion(configuration));
                                threadPool.ExecuteTask(deleteTask);
                            }
                            deletionInfos = new List<DeletionNode>() { deletionNode };
                            lastNodeSPId = currentNodeSpId;
                        }
                    }
                    else
                    {
                        // Optimized logic: group up to 50 documents into one delete task when the optimization is enabled.
                        if (deletionInfos == null)
                        {
                            deletionInfos = new List<DeletionNode>();
                        }
                        string currentNodeSpId = deletionNode.SPId;
                        if (string.Compare(currentNodeSpId, lastNodeSPId, StringComparison.OrdinalIgnoreCase) != 0 && deletionInfos.Count > 0)
                        {
                            bool needDispatchCurrentTask = false;
                            if (configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion
                                || (configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document
                                    && (configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers))
                            {
                                needDispatchCurrentTask = true;
                            }
                        else if (currentTaskDocumentCount >= configuration.DeleteDocumentBatchOptimizationBatchSize)
                            {
                                needDispatchCurrentTask = true;
                            }

                            if (needDispatchCurrentTask)
                            {
                                mLog.Info($"DeleteDocumentBatchOptimization dispatch task. CurrentTaskDocumentCount:{currentTaskDocumentCount}. NodeCount:{deletionInfos.Count}. BatchSize:{configuration.DeleteDocumentBatchOptimizationBatchSize}.");
                                var deleteTask = new MultiDeleteTask(deletionInfos, new ArchiverDeletion(configuration));
                                threadPool.ExecuteTask(deleteTask);
                                deletionInfos = new List<DeletionNode>();
                                lastNodeSPId = string.Empty;
                                currentTaskDocumentCount = 0;
                            }
                        }

                        deletionInfos.Add(deletionNode);
                        if (string.Compare(currentNodeSpId, lastNodeSPId, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            currentTaskDocumentCount++;
                            lastNodeSPId = currentNodeSpId;
                        }
                    }
                }
                else if (enableMulti && (deletionNode.ObjectType == AveConstants.TYPE_LISTITEM ||
                                    deletionNode.ObjectType == AveConstants.TYPE_VERSION ||
                                    deletionNode.ObjectType == AveConstants.TYPE_DOCUMENT ||
                                    deletionNode.ObjectType == AveConstants.TYPE_LISTITEMVERSION))
                {
                    bool convertResult = false;
                    if (bool.TryParse(deletionNode.HeaderInfo.GetAttribute(KeyWord.DoDelete),out convertResult))
                    {
                        if(!convertResult)
                        {
                            mLog.Warn($"current document not fit rule,return,url:{deletionNode?.FullPath}");
                            return;
                        }
                    }
                    else
                    {
                        mLog.Warn($"Can not parse doDelete to bool,doDelete:{deletionNode.HeaderInfo.GetAttribute(KeyWord.DoDelete)}");
                        return;
                    }
                    if (threadPool == null)
                    {
                        threadPool = new TaskThreadPool(threadNumber, "MultiDelete");
                    }
                    if ((configuration.currentRule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                    {
                        deleteAction.CreateTagColumn(deletionNode);
                    }
                    if ((configuration.currentRule.KeepDataOption & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord
                        || LinkFileCommon.IsLeaveStubRule(configuration.currentRule))
                    {
                        deleteAction.ActiveInPlaceRecordsFeature(deletionNode);
                    }
                    if (configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection)
                    {
                        deleteAction.PreDeleteSiteCollection(deletionNode);
                    }
                    DisableLibraryAlert(deletionNode);
                    //Get Container level from cache to avoid multi init.
                    //Media回发Header的时候会返给Agent SiteURL，WebID，ListID，如果使用其它属性值请考虑media回发逻辑。
                    configuration.InitDeletionContainer(deletionNode.HeaderInfo.GetAttribute(KeyWord.SiteUrl), new Guid(deletionNode.HeaderInfo.GetAttribute(KeyWord.WebId)), new Guid(deletionNode.HeaderInfo.GetAttribute(KeyWord.ListId)));



                    if ((configuration.currentRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                        || LinkFileCommon.IsLeaveStubRule(configuration.currentRule))
                    {
                        //目前不止Not Backup调用此方法，Archive Content的功能也会调用此方法，后续可以优化此处逻辑.暂时改为Lifecycle Mode调用此方法
                        //if ((configuration.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                        if (configuration.IsILMode)
                        {
                            configuration.InitStubAveBackupContainer();
                            configuration.InitStubAveRestoreContainer();
                        }
                        WrapperConfiguration.WrapperConfigurationForBPOS.OnlyGetCurrentVersion = true;
                    }

                    if (!configuration.EnableDeleteDocumentBatchOptimization)
                    {
                        var currentNodeSpId = deletionNode.SPId;
                        if (string.Compare(currentNodeSpId, lastNodeSPId, System.StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (deletionInfos == null)
                            {
                                deletionInfos = new List<DeletionNode>();
                            }
                            deletionInfos.Add(deletionNode);
                        }
                        else
                        {
                            if (deletionInfos != null && deletionInfos.Count > 0)
                            {
                                var deleteTask = new MultiDeleteTask(deletionInfos, new ArchiverDeletion(configuration));
                                threadPool.ExecuteTask(deleteTask);
                            }
                            deletionInfos = new List<DeletionNode>() { deletionNode };
                            lastNodeSPId = currentNodeSpId;
                        }
                    }
                    else
                    {
                        // Optimized logic: group up to 50 documents into one delete task when the optimization is enabled.
                        if (deletionInfos == null)
                        {
                            deletionInfos = new List<DeletionNode>();
                        }
                        string currentNodeSpId = deletionNode.SPId;
                        if (string.Compare(currentNodeSpId, lastNodeSPId, StringComparison.OrdinalIgnoreCase) != 0 && deletionInfos.Count > 0)
                        {
                            bool needDispatchCurrentTask = false;
                            if (configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion
                                || (configuration.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document
                                    && (configuration.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers))
                            {
                                needDispatchCurrentTask = true;
                            }
                        else if (currentTaskDocumentCount >= configuration.DeleteDocumentBatchOptimizationBatchSize)
                            {
                                needDispatchCurrentTask = true;
                            }

                            if (needDispatchCurrentTask)
                            {
                                mLog.Info($"DeleteDocumentBatchOptimization dispatch task. CurrentTaskDocumentCount:{currentTaskDocumentCount}. NodeCount:{deletionInfos.Count}. BatchSize:{configuration.DeleteDocumentBatchOptimizationBatchSize}.");
                                var deleteTask = new MultiDeleteTask(deletionInfos, new ArchiverDeletion(configuration));
                                threadPool.ExecuteTask(deleteTask);
                                deletionInfos = new List<DeletionNode>();
                                lastNodeSPId = string.Empty;
                                currentTaskDocumentCount = 0;
                            }
                        }

                        deletionInfos.Add(deletionNode);
                        if (string.Compare(currentNodeSpId, lastNodeSPId, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            currentTaskDocumentCount++;
                            lastNodeSPId = currentNodeSpId;
                        }
                    }
                }
                else
                {
                    WaitForFinish();
                    deleteAction.HandleResponseMessage(deletionNode);
                }

                bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(configuration.currentRule);

                if (!delteWithNoBackup && isLinkToDucument && deletionNode.ObjectType == AveConstants.TYPE_LIST)
                {
                    try
                    {
                        mLog.Info($"Begin GenerateStubsInListByHSM:{deletionNode.FullPath}.");
                        string webId = string.Empty;
                        if (configuration.DeletionIAveWeb != null)
                        {
                            webId = configuration.DeletionIAveWeb.ID.ToString();
                        }
                        else
                        {
                            webId = deletionNode.HeaderInfo.GetAttribute(KeyWord.WebId);
                        }
                        GenerateStubsInListByHSM(configuration.currentRule.Id, webId, deletionNode.ListId.ToString(), deletionNode.FullPath);
                        mLog.Info($"Finish GenerateStubsInListByHSM:{deletionNode.FullPath}.");
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"Exception with GenerateStubsInListByHSM:{deletionNode.FullPath}.Message:{ex}");
                    }
                }
                else if (deletionNode.ObjectType == AveConstants.TYPE_LIST)
                {
                    EnableLibraryAlert(deletionNode);
                }
            }
        }
        private void DisableLibraryAlert(DeletionNode deletionNode)
        {
            try
            {
                var mHeaderInfo = deletionNode.HeaderInfo;
                string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
                Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
                Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
                configuration.mOffice365AlertUtil.DisableLibraryAlert(siteUrl, webID, listID);
            }
            catch (Exception ex)
            {
                mLog.Info($"Failed DisableLibraryAlert.Message:{ex}.");
            }
        }

        private void EnableLibraryAlert(DeletionNode deletionNode)
        {
            try
            {
                var mHeaderInfo = deletionNode.HeaderInfo;
                string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
                Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
                Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
                configuration.mOffice365AlertUtil.EnableLibraryAlert(siteUrl, webID, listID);
            }
            catch (Exception ex)
            {
                mLog.Info($"Failed DisableLibraryAlert.Message:{ex}.");
            }
        }

        public void WaitForFinish()
        {
            if (deletionInfos != null && deletionInfos.Count > 0)
            {
                // Shared logic: always dispatch the remaining queued nodes before waiting for completion.
                var deleteTask = new MultiDeleteTask(deletionInfos, new ArchiverDeletion(configuration));
                threadPool.ExecuteTask(deleteTask);
                deletionInfos = null;
                lastNodeSPId = string.Empty;
                currentTaskDocumentCount = 0;
            }
            if (threadPool != null)
            {
                threadPool.WaitForRunningTask();
            }
        }

        private void GenerateStubsInListByHSM(string ruleId,string webid, string listId,string listUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("HSMStub.GenerateStubsInListByHSM"))
            {
                var containerIds = HSMConnector.DBForHSMStub.GetContainerIds(ruleId, listId);
                mLog.Info($"GenerateStubsInListByHSM ContainerIds.Count:{containerIds.Count}. ruleId:{ruleId}.listId:{listId}.listUrl:{listUrl}.");
                foreach (var containerId in containerIds)
                {
                    if (NeedStopCurrentJob())
                    {
                        return;
                    }
                    var stubs = HSMConnector.DBForHSMStub.GetRecords(ruleId, listId, containerId);
                    var failedStubs = stubs.FindAll(s => s.Status != StubExportStauts.Verified);
                    if (failedStubs.Count == stubs.Count)
                    {
                        mLog.Info($"GenerateStubsInListByHSM failedStubs.Count:{failedStubs.Count} == stubs.Count:{stubs.Count}.listUrl:{listUrl}.");
                        foreach (var file in failedStubs)
                        {
                            configuration.JobReportDto.AddDeletionReport(configuration.GetNodeFullPath(file.FileUrl),
                                             0,
                                             JobDetailsStatus.Failed,
                                             (int)CacheNodeType.Item,
                                             configuration.JobId,
                                             configuration.currentRule.Name,
                                             "",
                                             "SO_Action_LevelStub",
                                             "SO_Action_LevelStubFailed",
                                             "");
                        }
                        continue;
                    }
                    if (failedStubs.Count > 0)
                    {
                        HSMConnector.RebuildJobManifestXML(containerId, failedStubs);
                    }
                    var list = from s in stubs where s.Status == StubExportStauts.Verified select s.Conver2RestoreFileInfo(configuration.JobId);
                    configuration.JobReportDto.UpdateProgress(stubs.Count());
                    HSMConnector.AddImportJobTask(configuration.DeletionIAveSite, new Guid(webid), new Guid(listId), containerId, false, listUrl, list.ToList());
                }
            }
        }

        private bool NeedStopCurrentJob()
        {
            try
            {
                using (new CheckJobStopScope()) { }
            }
            catch (JobStopException)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            CreateLinkFileByPackage.GetInstance(configuration).SplitPackage(true);
            CreateLinkFileByPackage.GetInstance(configuration).WatingCompleted();
            HSMConnector.WatingCompleted();
            if (threadPool != null)
            {
                threadPool.Dispose();
                threadPool = null;
            }
        }
    }
}