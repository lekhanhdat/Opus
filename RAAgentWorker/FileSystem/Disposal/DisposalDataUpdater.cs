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
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.Common.Hybrid;
using AvePoint.GCommon.Contract.Tree.Object;

namespace RAFileSystem.Disposal
{
    public class DisposalDataUpdater
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService ProgressService { get; set; }
        //private IReportService<JMJobDetails> JobDetailService { get; set; }
        private MemoryListCacheService<FSExplorerDeleteDto> explorerDeleteRecordsCache;
        private MemoryListCacheService<FSAzureTableEntityDto> azureTableRecordsCache;
        private MemoryListCacheService<FileSystemRecordDto> explorerMoveToRecordsCache;
        private MemoryListCacheService<FSAzureTableEntityDto> mCachedRecords;
        public DisposalDataUpdater()
        {
            explorerDeleteRecordsCache = new MemoryListCacheService<FSExplorerDeleteDto>();
            azureTableRecordsCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            explorerMoveToRecordsCache = new MemoryListCacheService<FileSystemRecordDto>();
            ProgressService = JobContext.Current.mProgressManager.Create();
            mCachedRecords = new MemoryListCacheService<FSAzureTableEntityDto>();
            //JobDetailService = JobContext.Current.JobDetailManager.Create();
        }

        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("DisposalDataUpdaterThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Increment();
                while (true)
                {
                    //there is no file/folder to be processed and also there is no discovery thread working on..   thread exit..
                    if (FSJobCache.Instance.DisposalArchiveCache.Count == 0
                        && FSJobCache.Instance.DisposalMoveToCache.Count == 0
                        && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                        && FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                    {
                        logger.Info("There is no more task. Disposal data updater thread[{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                        break;
                    }
                    //someone is till working. wait 1 sec for new objects.
                    if (FSJobCache.Instance.DisposalArchiveCache.Count == 0 && FSJobCache.Instance.DisposalMoveToCache.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    // try to get new file/folder..
                    try
                    {
                        FSJobCache.Instance.SerializerThreadMonitor.Increment();
                        IEnumerable<FSAzureTableEntityDto> files = FSJobCache.Instance.DisposalArchiveCache.Take(30);
                        logger.Debug("Disposal updater got {1} files. There are {0} files left in the cache.", FSJobCache.Instance.DisposalArchiveCache.Count, files.Count());
                        //using (new RAAgentPerformanceScope(string.Format("FSDiscover--process {0} folders", stubs.Count())))
                        {
                            foreach (FSAzureTableEntityDto file in files)
                            {
                                try
                                {
                                    //update records in explorer db to destory/moved state
                                    if (FSDataDisposal.ClassificationLevel == AvePoint.GCommon.Contract.Tree.Object.NodeLevel.FSFile)
                                    {
                                        //no need to update explorer, folder level classi
                                        SendToExplorer(ConvertAzureEntityDto2ExplorerDto(file));
                                    }
                                    SendToAzureTable(file);
                                }
                                catch (Exception itemex)
                                {
                                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName);
                                    logger.Error("Failed to process item. Object:{0}, Exception:{1}", fullPath.LogBase64(), itemex.ToString());
                                    ProgressService.Increase();
                                    FSJobCache.Instance.FailedCount++;
                                }
                            }
                        }

                        IEnumerable<FileSystemRecordDto> moveToFiles = FSJobCache.Instance.DisposalMoveToCache.Take(30);
                        {
                            foreach (var file in moveToFiles)
                            {
                                if (FSDataDisposal.ClassificationLevel == AvePoint.GCommon.Contract.Tree.Object.NodeLevel.FSFile)
                                    InsertMoveToData(file);
                            }
                        }
                    }
                    finally
                    {
                        FSJobCache.Instance.SerializerThreadMonitor.Decrement();
                    }
                }
                FinalSendRecordsToCosmos();
                FinalSendToExplorer();
                FinalInsertMoveToData();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to discover the files. Exception:{0}", ex.ToString());
            }
            finally
            {
                JobContext.Current.DisposalArchiveFinish = true;
                WaitForComplete();
                //try
                //{
                //    using (new AgentPerformanceScope("DataUpdater.DeleteAndMoveItemsInScope", addToStatistics: true))
                //    {
                //        JobContext.Current.ApiClient.DeleteAndMoveItemsInScope(FSJobCache.Instance.RootPath, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString());
                //    }
                //}
                //catch (Exception e)
                //{
                //    logger.Error("An error occurred while moving record to static table. Error:{0}", e.ToString());
                //}
                FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Decrement();
            }
        }
        private void FinalSendRecordsToCosmos()
        {
            try
            {
                while (true)
                {

                    if (FSJobCache.Instance.DisposalAzureData.Count == 0)
                    {
                        break;

                    }
                    try
                    {
                        var records = FSJobCache.Instance.DisposalAzureData.Take(100).ToList();
                        if (records != null && records.Count > 0)
                        {
                            mCachedRecords.AddBatch(records);
                            if (mCachedRecords.Count > ExternalUtil.TransferDataCount)
                            {
                                var mSendRecords = mCachedRecords.Take(ExternalUtil.TransferDataCount).ToList();
                                logger.Info("Send file to cosmos count:{0}", mSendRecords.Count);
                                List<Guid> failedIds;
                                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{mSendRecords.Count}", true))
                                {
                                    FSAzureTableEntityDtoWithJobId dtoInfo = new FSAzureTableEntityDtoWithJobId()
                                    {
                                        JobId = JobContext.Current.JobId,
                                        EntityDtos = mSendRecords
                                    };
                                    failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);
                                    HybridApiClient.Instance.AddScanData(mSendRecords?.Where(a=>a.Status == (int)SOApproveDBStatus.Archived).ToList());
                                }
                                if (failedIds.Count > 0)
                                {
                                    logger.Warn("Failed to add fs archived data to cosmos. File ids:" + string.Join(",", failedIds));
                                }
                                //AddReport(result?.FailedGuids, result?.SkippedGuids, mSendRecords);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while send sp data to explorer. Error:{0}", e.ToString());
                    }
                }
                FinialSendDataToCosmos();
            }
            catch (Exception e)
            {
                logger.Error("Error in send data. Error:{0}", e.ToString());
            }
        }

        private void FinialSendDataToCosmos()
        {
            if (mCachedRecords.Count > 0)
            {
                var records = mCachedRecords.TakeAll().ToList();
                logger.Info("Send file to cosmos count:{0}", records.Count);
                List<Guid> failedIds;
                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{records.Count}", true))
                {
                    FSAzureTableEntityDtoWithJobId dtoInfo = new FSAzureTableEntityDtoWithJobId()
                    {
                        JobId = JobContext.Current.JobId,
                        EntityDtos = records
                    };
                    failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);
                    HybridApiClient.Instance.AddScanData(records?.Where(a => a.Status == (int)SOApproveDBStatus.Archived).ToList());
                }
                if (failedIds.Count > 0)
                {
                    logger.Warn("Failed to add fs archived data to cosmos. File ids:" + string.Join(",", failedIds));
                }
                //AddReport(result?.FailedGuids, result?.SkippedGuids, records);
            }
        }
        private void WaitForComplete()
        {
            while (!JobContext.Current.SendDataToAzureTableFinish)
            {
                Thread.Sleep(5000);
                logger.Info("Waiting disposal scan finish.");
            }
        }

        private void SendToExplorer(FSExplorerDeleteDto dto)
        {
            explorerDeleteRecordsCache.Add(dto);
            if (explorerDeleteRecordsCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempCache = explorerDeleteRecordsCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    List<Guid> failedGuids = new List<Guid>();
                    using (new AgentPerformanceScope("DataUpdater.DeleteRecordsInExplorer", "RecordsSerializer.DeleteRecordsInExplorer, count:" + tempCache.Count, true))
                    {
                        failedGuids = JobContext.Current.ApiClient.DeleteRecordsInExplorer(tempCache);
                    }
                    //log
                    if (failedGuids.Count > 0)
                    {
                        logger.Warn("Failed to update fs archived data in explorer db. File ids:" + string.Join(",", failedGuids));
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while sending data to explorer. Error:{0}", e.ToString());
                }
            }
        }

        private void InsertMoveToData(FileSystemRecordDto dto)
        {
            explorerMoveToRecordsCache.Add(dto);
            if (explorerMoveToRecordsCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempCache = explorerMoveToRecordsCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    AgentSyncDataResultDto result;
                    using (new AgentPerformanceScope("DataUpdater.SyncData", "RecordsSerializer.SyncMovedData, count:" + tempCache.Count, true))
                    {
                        GenerateUniqueId(tempCache);
                        result = JobContext.Current.ApiClient.SyncMovedData(tempCache);
                    }
                    //log
                    if (result != null && result.FailedGuids != null && result.FailedGuids.Count > 0)
                    {
                        logger.Warn("Failed to update fs archived data in explorer db. File ids:" + string.Join(",", result.FailedGuids));
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while inserting move to data. Error:{0}", e.ToString());
                }
            }
        }

        private void FinalInsertMoveToData()
        {
            if (explorerMoveToRecordsCache.Count > 0)
            {
                try
                {
                    var tempCache = explorerMoveToRecordsCache.TakeAll().ToList();
                    AgentSyncDataResultDto result;
                    using (new AgentPerformanceScope("DataUpdater.SyncData", "RecordsSerializer.SyncMovedData, count:" + tempCache.Count, true))
                    {
                        GenerateUniqueId(tempCache);
                        result = JobContext.Current.ApiClient.SyncMovedData(tempCache);
                    }
                    //log
                    if (result != null && result.FailedGuids != null && result.FailedGuids.Count > 0)
                    {
                        logger.Warn("Failed to update fs archived data in explorer db. File ids:" + string.Join(",", result.FailedGuids));
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final inserting move to data. Error:{0}", e.ToString());
                }
            }
        }

        private void FinalSendToExplorer()
        {
            if (explorerDeleteRecordsCache.Count > 0)
            {
                try
                {
                    var tempCache = explorerDeleteRecordsCache.TakeAll().ToList();
                    List<Guid> failedGuids = new List<Guid>();
                    using (new AgentPerformanceScope("DataUpdater.DeleteRecordsInExplorer", "RecordsSerializer.DeleteRecordsInExplorer, count:" + tempCache.Count, true))
                    {
                        failedGuids = JobContext.Current.ApiClient.DeleteRecordsInExplorer(tempCache);
                    }
                    //log
                    if (failedGuids != null && failedGuids.Count > 0)
                    {
                        logger.Warn("Failed to update fs archived data in explorer db. File ids:" + string.Join(",", failedGuids));
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final sending data to explorer. Error:{0}", e.ToString());
                }
            }
        }

        private FSExplorerDeleteDto ConvertAzureEntityDto2ExplorerDto(FSAzureTableEntityDto entity)
        {
            FSExplorerDeleteDto dto = new FSExplorerDeleteDto()
            {
                Id = entity.FilePathMd5,
                ConnectionId = entity.ConnectionId,
                RecordStatus = entity.RecordStatus
            };
            return dto;
        }

        private void SendToAzureTable(FSAzureTableEntityDto dto)
        {
            dto.NoNeedSendReport = true;
            FSJobCache.Instance.DisposalAzureData.Add(dto);
        }

        private void GenerateUniqueId(List<FileSystemRecordDto> sendRecords)
        {
            var uniqueSetting = JobContext.Current.ApiClient.GetUniqueIdSetting();
            if (uniqueSetting != null)
            {
                var isActived = uniqueSetting.IsActived;
                if (isActived)
                {
                    var index = 0;
                    var isStored = uniqueSetting.IsStored;
                    if (isStored)
                    {
                        var needSyncedRecords = sendRecords.Where(record =>
                                string.IsNullOrEmpty(record.RecordsId)
                                && record.NodeType != (int)NodeLevel.FSConnectionGroups
                                && record.NodeType != (int)NodeLevel.FSConnectionGroup);
                        var uniqueIdList = JobContext.Current.ApiClient.GetUniqueIdList(needSyncedRecords.Count());
                        foreach(var needSyncedRecord in needSyncedRecords)
                        {
                            needSyncedRecord.RecordsId = FormateCurrentId(uniqueSetting, uniqueIdList[index++]);
                        }
                    }
                    else
                    {
                        var needSyncedRecords = sendRecords.Where(record =>
                                record.NodeType != (int)NodeLevel.FSConnectionGroups
                                && record.NodeType != (int)NodeLevel.FSConnectionGroup);
                        var uniqueIdList = JobContext.Current.ApiClient.GetUniqueIdList(needSyncedRecords.Count());
                        foreach (var needSyncedRecord in needSyncedRecords)
                        {
                            needSyncedRecord.RecordsId = FormateCurrentId(uniqueSetting, uniqueIdList[index++]);
                        }
                    }
                }
            }
        }

        private string FormateCurrentId(FileSystemUniqueIdDto setting, long number)
        {
            var result = string.Empty;
            try
            {
                string templateId = string.Empty;//Electric unique id do not have templateid, so we use templatedId = string.Empty
                string currentFormat = "{0}-{1}";
                result = string.Format(currentFormat, setting.Prefix, FormatNumber(number));
            }
            catch (Exception e)
            {
                logger.Info("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        private string FormatNumber(long number, int digit = 10, bool throwIfOverLength = false)
        {
            var result = string.Empty;
            try
            {
                if (number < (Math.Pow(10, digit)))
                {
                    result = number.ToString().PadLeft(digit, '0');
                }
                else if (throwIfOverLength)
                {
                    throw new Exception("Over the digit number");
                }
                else
                {
                    result = number.ToString();
                }
            }
            catch (Exception e)
            {
                logger.Info(string.Format("Failed to formate number {0} : {1}", number, e.ToString()));
                throw;
            }
            return result;
        }
    }
}
