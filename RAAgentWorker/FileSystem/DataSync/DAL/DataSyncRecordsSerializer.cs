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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.Contract.Global.Object;
using RAFileSystem.FileSystem.Common;
using System.IO;
using Newtonsoft.Json;
using AvePoint.RA.Common.Utils;
using static AvePoint.GCommon.Utility.I18N.EventIds.SharePoint;

namespace AvePoint.RA.FileSystem.Collect
{
    public class DataSyncRecordsSerializer
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService progressService;
        private IReportService<JMJobDetails> reportServcie;
        private MemoryListCacheService<FileSystemRecordDto> cachedRecords;
        public DataSyncRecordsSerializer()
        {
            cachedRecords = new MemoryListCacheService<FileSystemRecordDto>();
            progressService = JobContext.Current.mProgressManager.Create();
            reportServcie = JobContext.Current.JobDetailManager.Create();
        }
        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("PersistThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.SerializerThreadMonitor.Increment();
                while (true)
                {
                    if (FSJobCache.Instance.RecordCache.Count == 0 && FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                    {
                        logger.Info("There is no records to be store. AND there is no analyzer thread running. Persist Thread [{0}] Exiting....", Thread.CurrentThread.Name);
                        break;
                    }
                    if (FSJobCache.Instance.RecordCache.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    IEnumerable<FileSystemRecordDto> temp = FSJobCache.Instance.RecordCache.Take(100);
                    logger.Info("RecordsSerializer got {1} records. There are {0} records to be stored left in the cache.", FSJobCache.Instance.RecordCache.Count, temp.Count());
                    // int count = temp.Count(t => string.IsNullOrEmpty(t.RecordsId) && (t.NodeType == 2100 || t.NodeType == 2200));
                    //AllocateRecordId(count);
                    if (temp != null && temp.Count() > 0)
                    {
                        using (new AgentPerformanceScope("RecordsSerializer.SaveRecords", string.Format("RecordsSerializer.Save {0} Records", temp.Count()), true))
                        {
                            foreach (FileSystemRecordDto item in temp)
                            {
                                try
                                {
                                    //assemble unique id in api web
                                    AddRecords(item);
                                    progressService.Increase();
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Failed to store the record to db.  Exception:{0}", ex.ToString());
                                    reportServcie.Commit(
                                         new FSDataSyncJobReportDetail()
                                         {
                                             AgentName = OSInformation.HostName,
                                             ObjectName = item.LeafName,
                                             FullPath = item.FullPath,
                                             Status = JobDetailsStatus.Failed,
                                             Comment = "RM_JM_FSFailedAddToExplorer"
                                         });
                                    FSJobCache.Instance.FailedCount++;
                                    Add2FailedItemCache(item);
                                }
                            }
                        }
                    }
                }
                FinalAddRecords();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to store records to the explorer database. Exception:{0}", ex.ToString());
                //JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, ex.Message);
            }
            finally
            {
                FSJobCache.Instance.SerializerThreadMonitor.Decrement();
            }
        }

        private void AddRecords(FileSystemRecordDto dto)
        {
            cachedRecords.Add(dto);
            if (cachedRecords.Count > ExternalUtil.TransferDataCount)
            {
                var sendRecords = cachedRecords.Take(ExternalUtil.TransferDataCount).ToList();
                logger.Debug("Start to sync data to explorer. Data length:{0}", SerializerHelper.SerializeByDataContractSerializer(sendRecords).Length);
                try
                {
                    Contract.FileSystem.AgentSyncDataResultDto result;
                    var failedIds = new List<Guid>();
                    using (new AgentPerformanceScope("RecordsSerializer.SyncData", "RecordsSerializer.SyncData, count:" + sendRecords.Count, true))
                    {
                        failedIds = GenerateUniqueId(sendRecords);
                        result = JobContext.Current.ApiClient.SyncData(sendRecords.Where(record => !failedIds.Contains(record.NodeId)).ToList());
                    }
                    logger.Debug("Sync data finished. Failed ids:{0}", string.Join(",", result.FailedGuids));
                    failedIds.AddRange(result?.FailedGuids);
                    SendReports(sendRecords, failedIds, result?.SkippedGuids);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while sync data. Error:{0}", e.ToString());
                    SendReports(sendRecords, sendRecords.Select(r => r.NodeId).ToList(), new List<Guid>());
                }
            }
        }

        private void FinalAddRecords()
        {
            if (cachedRecords.Count > 0)
            {
                var sendRecords = cachedRecords.TakeAll().ToList();
                try
                {
                    Contract.FileSystem.AgentSyncDataResultDto result;
                    var failedIds = new List<Guid>();
                    using (new AgentPerformanceScope("RecordsSerializer.SyncData", "RecordsSerializer.SyncData, count:" + sendRecords.Count, true))
                    {
                        failedIds = GenerateUniqueId(sendRecords);
                        result = JobContext.Current.ApiClient.SyncData(sendRecords.Where(record => !failedIds.Contains(record.NodeId)).ToList());
                    }
                    failedIds.AddRange(result?.FailedGuids);
                    SendReports(sendRecords, failedIds, result?.SkippedGuids);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final sync data. Error:{0}", e.ToString());
                    SendReports(sendRecords, sendRecords.Select(r => r.NodeId).ToList(), new List<Guid>());
                }
            }
        }

        private void SendReports(List<FileSystemRecordDto> sendRecords, List<Guid> failedGuids, List<Guid> skippedGuids)
        {
            List<FSDataSyncJobReportDetail> reports = new List<FSDataSyncJobReportDetail>();
            sendRecords.ForEach(r =>
            {
                if (r.NodeType != (int)NodeLevel.FSConnectionGroup && r.NodeType != (int)NodeLevel.FSConnectionGroups)
                {
                    var isFailed = false;
                    FSDataSyncJobReportDetail report = new FSDataSyncJobReportDetail()
                    {
                        AgentName = OSInformation.HostName,
                        ObjectName = r.LeafName,
                        FullPath = r.FullPath,
                    };
                    if (failedGuids != null && failedGuids.Contains(r.NodeId))
                    {
                        Add2FailedItemCache(r);
                        report.Status = JobDetailsStatus.Failed;
                        report.Comment = "RM_JM_FSFailedAddToExplorer";
                        FSJobCache.Instance.FailedCount++;
                        isFailed = true;
                    }
                    else if (skippedGuids != null && skippedGuids.Contains(r.NodeId))
                    {
                        report.Status = JobDetailsStatus.Skipped;
                        report.Comment = "RM_JM_FSSkipAddToExplorer";
                    }
                    else
                    {
                        //同步上次job失败的数据成功了，success detail打一条提示
                        if (Add2SuccessItemCache(r))
                        {
                           // report.Comment = "RM_JM_SyncFailedItemSuccess";
                        }
                        report.Status = JobDetailsStatus.Successful;
                        FSJobCache.Instance.SuccessCount++;
                    }
                    if (r.HasTermChanged || r.HasRuleChanged ||r.NodeType == (int)NodeLevel.FSFolder || Add2SuccessItemCache(r) || isFailed)
                    {
                        reports.Add(report);
                    }
                }
            });

            reportServcie.CommitBatch(reports);
        }

        private void Add2FailedItemCache(FileSystemRecordDto dto)
        {
            var fullPath = ExternalUtil.CombinePath(dto.DirPath, dto.LeafName);
            Guid nodeId = Guid.Empty;
            if (fullPath.Length == FSJobCache.Instance.RootPath.Length)
            {
                nodeId = dto.NodeId;
            }
            else
            {
                nodeId = fullPath.Substring(FSJobCache.Instance.RootPath.Length + 1).ToLowerInvariant().ToMd5();
            }
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling
                && dto.NodeType == (int)NodeLevel.FSFile && !FSJobCache.Instance.LastJobFailedItemIds.Contains(nodeId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = dto.FullPath.Substring(FSJobCache.Instance.RootPath.Length + 1),
                    SortTicks = Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    NodeId = nodeId.ToString(),
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = dto.LeafName,
                    Message = "RM_JM_FSFailedAddToExplorer"
                };
                FSJobCache.Instance.FailedItems.Add(item);
            }
        }

        private bool Add2SuccessItemCache(FileSystemRecordDto dto)
        {
            bool success = false;
            var fullPath = ExternalUtil.CombinePath(dto.DirPath, dto.LeafName);
            Guid nodeId = Guid.Empty;
            if (fullPath.Length == FSJobCache.Instance.RootPath.Length)
            {
                nodeId = dto.NodeId;
            }
            else
            {
                nodeId = fullPath.Substring(FSJobCache.Instance.RootPath.Length + 1).ToMd5();
            }
            if (FSJobCache.Instance.LastJobFailedItemIds.Contains(nodeId)
                && !FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Contains(nodeId))
            {
                FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Add(nodeId);
                success = true;
            }
            return success;
        }

        private List<Guid> GenerateUniqueId(List<FileSystemRecordDto> sendRecords)
        {
            var failedGuid = new List<Guid>();
            var recordWithADS = new List<FileSystemRecordDto>();
            var uniqueSetting = JobContext.Current.ApiClient.GetUniqueIdSetting();
            if (uniqueSetting != null)
            {

                var needSyncedRecordNodeIds = sendRecords.Where(record =>
                            string.IsNullOrEmpty(record.RecordsId)
                            && record.NodeType != (int)NodeLevel.FSConnectionGroups
                            && record.NodeType != (int)NodeLevel.FSConnectionGroup).Select(item => item.NodeId);

                var count = needSyncedRecordNodeIds.Count();

                if(FSDataCollector.ClassificationLevel != NodeLevel.FSFile)
                {
                    if (needSyncedRecordNodeIds.Any())
                    {
                        var dbFolderRecords = JobContext.Current.ApiClient.GetFSDBRecords(needSyncedRecordNodeIds.ToList());
                        var dbFolderRecordsDict = dbFolderRecords.ToDictionary(x => x.NodeId, x => x.RecordsId);

                        foreach (var sendRecord in sendRecords)
                        {
                            if (dbFolderRecordsDict.TryGetValue(sendRecord.NodeId, out var recordsId))
                            {
                                sendRecord.RecordsId = recordsId;
                            }
                        }

                        count -= dbFolderRecords.Count();
                    }
                }

                logger.Info($"Need to generate unique id count is [{count}]");

                var isActived = uniqueSetting.IsActived;
                if (isActived)
                {
                    var uniqueIdList = JobContext.Current.ApiClient.GetUniqueIdList(count);
                    var isStored = uniqueSetting.IsStored;
                    var index = 0;
                    foreach (var record in sendRecords)
                    {
                        try
                        {
                            if (record.NodeType == (int)NodeLevel.FSConnectionGroups || record.NodeType == (int)NodeLevel.FSConnectionGroup)
                            {
                                continue;
                            }

                            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                            var fullPath = metaInfo.LocalFullPath.EndsWith("\\") ? metaInfo.LocalFullPath.Remove(metaInfo.LocalFullPath.LastIndexOf("\\")) : metaInfo.LocalFullPath;
                            var adsUniqueInfoStr = AdsHelper.ReadUniqueIdAds(fullPath);

                            if (string.IsNullOrEmpty(adsUniqueInfoStr))
                            {
                                var uniqueId = string.Empty;
                                logger.Info($"Current item does't have ADS ID, node id is : [{record.NodeId}]");
                                if (!string.IsNullOrEmpty(record.RecordsId))
                                {
                                    uniqueId = record.RecordsId;
                                }
                                else
                                {
                                    uniqueId = FormateCurrentId(uniqueSetting, uniqueIdList[index++]);
                                    record.RecordsId = uniqueId;
                                }

                                logger.Info($"Current item unique id is : [{record.RecordsId}], node id is : [{record.NodeId}]");
                                if (isStored)
                                {
                                    logger.Info($"Store current item unique id: [{record.RecordsId}] to ads, node id is : [{record.NodeId}]");
                                    try
                                    {
                                        var uniqueInfo = new FileSystemADSUniqueInfo { UniqueId = uniqueId };
                                        var termInfo = new FileSystemADSTermInfo { TermId = record.TermId.ToString() };
                                        AdsHelper.WriteUniqueIdAdsAndRevertTime(fullPath, uniqueInfo, record.NodeType == (int)NodeLevel.FSFolder);
                                        AdsHelper.WriteTermIdAdsAndRevertTime(fullPath, termInfo, record.NodeType == (int)NodeLevel.FSFolder);
                                    }
                                    catch(Exception e)
                                    {
                                        logger.Error($"Store current item unique id: [{record.RecordsId}] to ads failed, node id is : [{record.NodeId}], error : {e}");
                                        failedGuid.Add(record.NodeId);
                                    }
                                }
                                continue;
                            }

                            var adsUniqueInfo = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(adsUniqueInfoStr);
                            var adsID = adsUniqueInfo.UniqueId;
                            logger.Info($"Current item has ADS ID : [{adsID}], node id is : [{record.NodeId}]");
                            record.ADSID = adsID;
                            recordWithADS.Add(record);
                        }
                        catch(Exception ex)
                        {
                            logger.Error($"Genenrate unqiue id and store ads ID failed, error : {ex}");
                            failedGuid.Add(record.NodeId);
                        }
                    }

                    ProcessRecordsWithADS(recordWithADS, isStored, uniqueSetting, uniqueIdList, index);
                }
            }
            return failedGuid;
        }

        private void ProcessRecordsWithADS(List<FileSystemRecordDto> recordsWithADS, bool isStored, FileSystemUniqueIdDto uniqueSetting, List<long> uniqueIdList, int index)
        {
            var adsIdList = recordsWithADS.Select(r => r.ADSID).Distinct().ToList();
            var dbRecords = JobContext.Current.ApiClient.GetFSDBRecordsByRecordsId(adsIdList);
            var dbRecordsDict = dbRecords.GroupBy(r => r.RecordsId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var record in recordsWithADS)
            {
                try
                {
                    if (dbRecordsDict.TryGetValue(record.ADSID, out var sameIdRecords))
                    {
                        foreach (var dbRecord in sameIdRecords)
                        {
                            var dbMetaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(dbRecord.MetaInfo);
                            var dbFullPath = Path.GetFullPath(dbMetaInfo.LocalFullPath.TrimEnd('\\'));

                            bool exists = dbRecord.NodeType == (int)NodeLevel.FSFolder
                                ? Directory.Exists(dbFullPath)
                                : File.Exists(dbFullPath);

                            if (!exists)
                            {
                                MergeHoldInfo(record, dbRecord);
                                JobContext.Current.ApiClient.DeleteMovedItem(dbRecord);
                                logger.Warn($"Cannot find local {(dbRecord.NodeType == (int)NodeLevel.FSFolder ? "folder" : "file")}. Node id: {dbRecord.NodeId} , delete db record.");
                            }
                        }

                        if (string.IsNullOrEmpty(record.RecordsId))
                        {
                            logger.Info($"Current item is not exist unique id, use adsID, node id is : [{record.NodeId}]");
                            record.RecordsId = record.ADSID;
                        }
                    }
                    else
                    {
                        logger.Info($"Current cosmos db is not exist item with ADS ID : [{record.ADSID}], node id is : [{record.NodeId}]");
                        if (string.IsNullOrEmpty(record.RecordsId))
                        {
                            if (!isStored)
                            {
                                logger.Info($"Current item is not exist unique id and not store ADS, general new unique id, node id is : [{record.NodeId}]");
                                var uniqueId = FormateCurrentId(uniqueSetting, uniqueIdList[index++]);
                                record.RecordsId = uniqueId;
                                continue;
                            }
                            logger.Info($"Current item is not exist unique id and store ADS, general new unique id, node id is : [{record.NodeId}]");
                            record.RecordsId = record.ADSID;
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"ProcessRecordsWithADS failed, error : {ex}");
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

        private FileSystemRecordDto MergeHoldInfo(FileSystemRecordDto target, FileSystemRecordDto source)
        {
            if (target == null || source == null) return target;

            target.HoldStatus = source.HoldStatus;
            target.HoldType = source.HoldType;
            target.HoldReleaseTime = source.HoldReleaseTime;
            target.HoldId = source.HoldId;
            target.HoldBy = source.HoldBy;
            target.HoldByUsers = source.HoldByUsers;
            target.HoldUntilTimes = source.HoldUntilTimes;
            target.AppendHolds_Array = source.AppendHolds_Array;
            target.DisposalDueDate = source.DisposalDueDate;

            return target;
        }
    }
}
