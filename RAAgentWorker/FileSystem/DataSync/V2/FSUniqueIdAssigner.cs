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
using AvePoint.RA.Common.Utils;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSUniqueIdAssigner
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly long MaxDigitValue = 10_000_000_000L;

        private readonly FileSystemUniqueIdDto _fileSystemUniqueId;

        private NodeLevel _classificationLevel;

        private UniqueIdAssignResult _result;


        public FSUniqueIdAssigner(FileSystemUniqueIdDto fileSystemUniqueId, NodeLevel classificationLevel)
        {
            _fileSystemUniqueId = fileSystemUniqueId;
            _classificationLevel = classificationLevel;
        }

        public UniqueIdAssignResult AssignUniqueIds(List<FileSystemRecordDto> sendRecords)
        {
            _result = new UniqueIdAssignResult();

            if (_fileSystemUniqueId != null)
            {
                _logger.Info($"Unique id setting found. IsActived: {_fileSystemUniqueId.IsActived}, Prefix: {_fileSystemUniqueId.Prefix}, IsStored: {_fileSystemUniqueId.IsStored}.");
                AssignWithSetting(sendRecords);
            }
            else
            {
                _logger.Info("No unique id setting found, will assign unique id by default.");
                AssignByDefault(sendRecords);
            }

            return _result;
        }

        private void AssignWithSetting(List<FileSystemRecordDto> sendRecords)
        {
            using (new AgentPerformanceScope("FSUniqueIdAssigner.AssignWithSetting", addToStatistics: true))
            {
                var recordsNeedingId = sendRecords
                .Where(r => string.IsNullOrEmpty(r.RecordsId)
                    && r.NodeType != (int)NodeLevel.FSConnectionGroups
                    && r.NodeType != (int)NodeLevel.FSConnectionGroup)
                .Select(item => item.NodeId)
                .ToList();

                var count = recordsNeedingId.Count;

                if (_classificationLevel != NodeLevel.FSFile && recordsNeedingId.Count > 0)
                {
                    count -= BackfillRecordsIdsFromDB(sendRecords, recordsNeedingId);
                }

                _logger.Info($"Need to generate unique id count is [{count}]");

                if (!_fileSystemUniqueId.IsActived) return;

                var uniqueIdList = JobContext.Current.ApiClient.GetUniqueIdList(count);
                var uniqueIdQueue = new ConcurrentQueue<long>(uniqueIdList ?? Enumerable.Empty<long>());

                var processableRecords = FilterProcessableRecords(sendRecords);
                var adsReadResults = ReadAdsInParallel(processableRecords);
                var recordsWithAds = AssignUniqueIdsFromAdsResults(adsReadResults, uniqueIdQueue);

                if (_fileSystemUniqueId.IsStored)
                {
                    WriteAdsInParallel(adsReadResults);
                }

                ProcessRecordsWithADS(recordsWithAds, uniqueIdQueue);
            }
        }
        
        /// <summary>
        /// Filters out connection group records and resolves full paths.
        /// Returns a list of record ready for ADS processing.
        /// </summary>
        private List<FileSystemRecordDto> FilterProcessableRecords(List<FileSystemRecordDto> sendRecords)
        {
            var notProcessNodeTypes = new List<int>
                { (int)NodeLevel.FSConnectionGroups, (int)NodeLevel.FSConnectionGroup };

            return sendRecords.Where(record => !notProcessNodeTypes.Contains(record.NodeType)).ToList();
        }
        
        /// <summary>
        /// Reads ADS unique info from all files in parallel.
        /// Returns a list of (record, fullPath, adsUniqueInfoStr) for further processing.
        /// </summary>
        private List<(FileSystemRecordDto Record, string FullPath, string AdsInfo)> ReadAdsInParallel(
            List<FileSystemRecordDto> processableRecords)
        {
            var results = new ConcurrentBag<(FileSystemRecordDto Record, string FullPath, string AdsInfo)>();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.ForEach(processableRecords, parallelOptions, record =>
            {
                try
                {
                    var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                    var fullPath = metaInfo.LocalFullPath.TrimEnd('\\');
                    var adsUniqueInfoStr = AdsHelper.ReadUniqueIdAds(fullPath);
                    results.Add((record, fullPath, adsUniqueInfoStr));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Generate unique id and store ADS failed for node [{record.NodeId}]. Error: {ex}");
                    _result.AddFailure(record.NodeId);
                }
            });
            
            return results.ToList();
        }
        
        private List<FileSystemRecordDto> AssignUniqueIdsFromAdsResults(
            List<(FileSystemRecordDto Record, string FullPath, string AdsInfo)> adsReadResults,
            ConcurrentQueue<long> uniqueIdList)
        {
            var recordsWithADS = new List<FileSystemRecordDto>();

            foreach (var (record, fullPath, adsInfo) in adsReadResults)
            {
                try
                {
                    if (string.IsNullOrEmpty(adsInfo))
                    {
                        AssignNewUniqueId(record, uniqueIdList);
                    }
                    else
                    {
                        var adsUniqueInfo = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(adsInfo);
                        record.ADSID = adsUniqueInfo.UniqueId;
                        _logger.Debug($"Record [{record.NodeId}] has existing ADS ID: [{adsUniqueInfo.UniqueId}]");
                        recordsWithADS.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Generate unique id and store ADS ID failed, error: {ex}");
                    _result.AddFailure(record.NodeId);
                }
            }

            return recordsWithADS;
        }
        
        private void AssignNewUniqueId(FileSystemRecordDto record, ConcurrentQueue<long> uniqueIdQueue)
        {
            var uniqueId = ResolveOrAllocateUniqueId(record, uniqueIdQueue);
            if (uniqueId == null) return;

            record.RecordsId = uniqueId;
            _result.IncrementSuccess();
            _logger.Debug($"Assigned unique id [{record.RecordsId}] for node [{record.NodeId}]");
        }
        
        private void WriteAdsInParallel(List<(FileSystemRecordDto Record, string FullPath, string AdsInfo)> adsReadResults)
        {
            var recordsToWrite = adsReadResults
                .Where(r => string.IsNullOrEmpty(r.AdsInfo) && !string.IsNullOrEmpty(r.Record.RecordsId))
                .ToList();

            if (recordsToWrite.Count == 0) return;

            _logger.Info($"Writing ADS for [{recordsToWrite.Count}] records in parallel.");
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

            Parallel.ForEach(recordsToWrite, parallelOptions, item =>
            {
                try
                {
                    var isFolder = item.Record.NodeType == (int)NodeLevel.FSFolder;
                    var uniqueInfo = new FileSystemADSUniqueInfo { UniqueId = item.Record.RecordsId };
                    var termInfo = new FileSystemADSTermInfo { TermId = item.Record.TermId.ToString() };

                    AdsHelper.WriteUniqueIdAdsAndRevertTime(item.FullPath, uniqueInfo, isFolder);
                    AdsHelper.WriteTermIdAdsAndRevertTime(item.FullPath, termInfo, isFolder);

                    _logger.Debug($"Stored ADS for node [{item.Record.NodeId}], unique id: [{item.Record.RecordsId}]");
                }
                catch (Exception e)
                {
                    _logger.Error($"Store unique id [{item.Record.RecordsId}] to ADS failed, node [{item.Record.NodeId}], error: {e}");
                    _result.AddFailure(item.Record.NodeId);
                }
            });
        }

        private int BackfillRecordsIdsFromDB(List<FileSystemRecordDto> sendRecords, List<Guid> nodeIds)
        {
            var dbRecords = JobContext.Current.ApiClient.QueryFileSystemRecords(FSJobCache.Instance.AveConnectionId.ToString(), nodeIds);
            var dbDict = dbRecords.ToDictionary(x => x.NodeId, x => x.RecordsId);
            _logger.Debug("Queried {0} file system records from db.", dbRecords?.Count ?? 0);

            foreach (var record in sendRecords)
            {
                if (dbDict.TryGetValue(record.NodeId, out var recordsId))
                {
                    record.RecordsId = recordsId;
                }
            }

            return dbRecords.Count;
        }

        private void ProcessRecordForUniqueId(FileSystemRecordDto record, Queue<long> uniqueIdQueue, List<FileSystemRecordDto> recordsWithAds)
        {
            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
            var fullPath = metaInfo.LocalFullPath.TrimEnd('\\');
            var adsUniqueInfoStr = AdsHelper.ReadUniqueIdAds(fullPath);

            if (!string.IsNullOrEmpty(adsUniqueInfoStr))
            {
                var adsUniqueInfo = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(adsUniqueInfoStr);
                _logger.Debug($"Record [{record.NodeId}] has existing ADS ID: [{adsUniqueInfo.UniqueId}]");
                record.ADSID = adsUniqueInfo.UniqueId;
                recordsWithAds.Add(record);
                return;
            }

            _logger.Debug($"Record [{record.NodeId}] does not have ADS ID.");

            var uniqueId = ResolveOrAllocateUniqueId(record, uniqueIdQueue);
            if (uniqueId == null) return;

            record.RecordsId = uniqueId;
            _result.IncrementSuccess();
            _logger.Debug($"Assigned unique id [{record.RecordsId}] to node [{record.NodeId}]");

            if (_fileSystemUniqueId.IsStored)
            {
                StoreUniqueIdToAds(record, fullPath, uniqueId);
            }
        }

        private string ResolveOrAllocateUniqueId(FileSystemRecordDto record, Queue<long> uniqueIdQueue)
        {
            if (!string.IsNullOrEmpty(record.RecordsId))
            {
                return record.RecordsId;
            }

            if (uniqueIdQueue.Count == 0)
            {
                _logger.Error($"Exhausted pre-allocated unique IDs. Cannot assign ID to node [{record.NodeId}].");
                _result.AddFailure(record.NodeId);
                return null;
            }

            return FormatPrefixedId(_fileSystemUniqueId.Prefix, uniqueIdQueue.Dequeue());
        }
        
        private string ResolveOrAllocateUniqueId(FileSystemRecordDto record, ConcurrentQueue<long> uniqueIdQueue)
        {
            if (!string.IsNullOrEmpty(record.RecordsId))
            {
                return record.RecordsId;
            }

            if (uniqueIdQueue.Count == 0 || !uniqueIdQueue.TryDequeue(out var uniqueId))
            {
                _logger.Error($"Exhausted pre-allocated unique IDs. Cannot assign ID to node [{record.NodeId}].");
                _result.AddFailure(record.NodeId);
                return null;
            }

            return FormatPrefixedId(_fileSystemUniqueId.Prefix, uniqueId);
        }

        private void StoreUniqueIdToAds(FileSystemRecordDto record, string fullPath, string uniqueId)
        {
            try
            {
                var uniqueInfo = new FileSystemADSUniqueInfo { UniqueId = uniqueId };
                var termInfo = new FileSystemADSTermInfo { TermId = record.TermId.ToString() };
                bool isFolder = record.NodeType == (int)NodeLevel.FSFolder;
                AdsHelper.WriteUniqueIdAdsAndRevertTime(fullPath, uniqueInfo, isFolder);
                AdsHelper.WriteTermIdAdsAndRevertTime(fullPath, termInfo, isFolder);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to store unique id [{uniqueId}] to ADS for node [{record.NodeId}]. Error: {e}");
                _result.AddFailure(record.NodeId);
            }
        }

        private void ProcessRecordsWithADS(List<FileSystemRecordDto> recordsWithADS, ConcurrentQueue<long> uniqueIdQueue)
        {
            if (recordsWithADS.Count == 0) return;
            var adsIdList = recordsWithADS.Select(r => r.ADSID).Distinct().ToList();
            var dbRecords = JobContext.Current.ApiClient.QueryFileSystemRecordsByRecordsId(FSJobCache.Instance.AveConnectionId.ToString(), adsIdList);
            _logger.Debug($"Queried {dbRecords?.Count ?? 0} file system records from db by ADS IDs.");
            var dbRecordsDict = dbRecords.GroupBy(r => r.RecordsId).ToDictionary(g => g.Key, g => g.ToList());
            var deleteMovedItems = new ConcurrentBag<FsRecordProcessDto>();

            Parallel.ForEach(recordsWithADS, new ParallelOptions { MaxDegreeOfParallelism = 5 }, record =>
            {
                try
                {
                    ProcessSingleAdsRecord(record, uniqueIdQueue, dbRecordsDict, deleteMovedItems);
                }
                catch (Exception ex)
                {
                    _logger.Error($"ProcessRecordsWithADS failed for node [{record.NodeId}]. Error: {ex}");
                    _result.AddFailure(record.NodeId);
                }
            });

            if (deleteMovedItems.Count > 0)
            {
                Task.Run(()=>JobContext.Current.ApiClient.DeleteMovedItems(deleteMovedItems.ToList()));
            }
        }

        private void ProcessSingleAdsRecord(FileSystemRecordDto record, ConcurrentQueue<long> uniqueIdQueue, Dictionary<string, List<FsRecordProcessDto>> dbRecordsDict, ConcurrentBag<FsRecordProcessDto> deleteMovedItems)
        {
            if (dbRecordsDict.TryGetValue(record.ADSID, out var sameIdRecords))
            {
                CleanupMovedRecords(record, sameIdRecords, deleteMovedItems);
                if (string.IsNullOrEmpty(record.RecordsId))
                {
                    _logger.Debug($"Record [{record.NodeId}] has no unique id, using ADS ID.");
                    record.RecordsId = record.ADSID;
                }
            }
            else
            {
                _logger.Debug($"No DB record found with ADS ID [{record.ADSID}] for node [{record.NodeId}].");
                AssignIdForOrphanAdsRecord(record, uniqueIdQueue);
            }
        }

        private void CleanupMovedRecords(FileSystemRecordDto record, List<FsRecordProcessDto> sameIdRecords, ConcurrentBag<FsRecordProcessDto> deleteMovedItems)
        {
            foreach (var dbRecord in sameIdRecords)
            {
                var dbMetaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(dbRecord.MetaInfo);
                var dbFullPath = Path.GetFullPath(dbMetaInfo.LocalFullPath.TrimEnd('\\'));
                bool exists = dbRecord.NodeType == (int)NodeLevel.FSFolder ? Directory.Exists(dbFullPath) : File.Exists(dbFullPath);
                if (!exists)
                {
                    MergeHoldInfo(record, dbRecord);
                    if (dbRecord.NodeType == (int)NodeLevel.FSFile)
                    {
                        var sourcePath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(dbRecord.FullPath.TrimEnd('\\'));
                        var destinationPath = Alphaleonis.Win32.Filesystem.Path.Combine(record.DirPath, record.LeafName).TrimEnd('\\');
                        AssembleMovedInfoes(dbRecord, sourcePath, destinationPath);
                        dbRecord.NewNodeId = record.NodeId;
                    }
                    deleteMovedItems.Add(dbRecord);
                    _logger.Info($"Record [{record.NodeId}] with ADS ID [{record.ADSID}] is identified as moved item.");
                }
            }
        }

        private void AssignIdForOrphanAdsRecord(FileSystemRecordDto record, ConcurrentQueue<long> uniqueIdQueue)
        {
            if (!string.IsNullOrEmpty(record.RecordsId)) return;

            if (!_fileSystemUniqueId.IsStored)
            {
                _logger.Debug($"Record [{record.NodeId}] has no unique id and ADS storage disabled, generating new id.");
                var newRecordsId = AllocateFromQueue(uniqueIdQueue, record.NodeId);
                if (newRecordsId != null)
                {
                    record.RecordsId = newRecordsId;
                }
                return;
            }

            _logger.Debug($"Record [{record.NodeId}] has no unique id but ADS storage enabled, using ADS ID.");
            record.RecordsId = record.ADSID;
        }

        private string AllocateFromQueue(ConcurrentQueue<long> queue, Guid nodeId)
        {
            if (queue.Count == 0 || !queue.TryDequeue(out var uniqueId))
            {
                _logger.Error($"Exhausted pre-allocated unique IDs. Cannot assign ID to node [{nodeId}].");
                _result.AddFailure(nodeId);
                return null;
            }
            return FormatPrefixedId(_fileSystemUniqueId.Prefix, uniqueId);
        }

        private void AssignByDefault(List<FileSystemRecordDto> sendRecords)
        {
            using (new AgentPerformanceScope("FSUniqueIdAssigner.AssignByDefault", addToStatistics: true))
            {
                var targetRecords = sendRecords
                                .Where(r => string.IsNullOrWhiteSpace(r.RecordsId) && (r.NodeType == (int)NodeLevel.FSFile || r.NodeType == (int)NodeLevel.FSFolder))
                                .ToList();

                _logger.Info($"Unique id setting is null, generate unique id by default. Record count [{targetRecords.Count}].");
                if (targetRecords.Count == 0) return;

                var uniqueIdList = JobContext.Current.ApiClient.GetUniqueIdList(targetRecords.Count);
                if (uniqueIdList == null || uniqueIdList.Count < targetRecords.Count)
                {
                    _logger.Warn("Failed to get sufficient unique ids from server.");
                    _result.AddFailures(targetRecords.Select(r => r.NodeId));
                    return;
                }

                for (int i = 0; i < targetRecords.Count; i++)
                {
                    try
                    {
                        targetRecords[i].RecordsId = FormatDefaultId(uniqueIdList[i]);
                        _result.IncrementSuccess();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Default unique id assignment failed for node [{targetRecords[i].NodeId}]. Error: {ex}");
                        _result.AddFailure(targetRecords[i].NodeId);
                    }
                }
            }
        }

        private string FormatDefaultId(long number)
        {
            try
            {
                return string.Format("{0}-{1}", ContractConstants.UniqueId_DefaultPrefix, FormatNumber(number));
            }
            catch (Exception e)
            {
                _logger.Error("Failed to format default unique id. Error: " + e.ToString());
                return string.Empty;
            }
        }

        private string FormatPrefixedId(string prefix, long number)
        {
            try
            {
                return string.Format("{0}-{1}", prefix, FormatNumber(number));
            }
            catch (Exception e)
            {
                _logger.Error("Failed to format prefixed unique id. Error: " + e.ToString());
                throw;
            }
        }

        private string FormatNumber(long number, int digit = 10)
        {
            if (number < MaxDigitValue)
            {
                return number.ToString().PadLeft(digit, '0');
            }
            return number.ToString();
        }

        private void MergeHoldInfo(FileSystemRecordDto target, FsRecordProcessDto source)
        {
            if (target == null || source == null) return;
            target.HoldStatus = source.HoldStatus;
            target.HoldType = source.HoldType;
            target.HoldReleaseTime = source.HoldReleaseTime;
            target.HoldId = source.HoldId;
            target.HoldBy = source.HoldBy;
            target.HoldByUsers = source.HoldByUsers;
            target.HoldUntilTimes = source.HoldUntilTimes;
            target.AppendHolds_Array = source.AppendHolds_Array;
            target.DisposalDueDate = source.DisposalDueDate;
            ResetHoldInfo(source);
        }

        private static void ResetHoldInfo(FsRecordProcessDto source)
        {
            source.HoldStatus = false;
            source.HoldType = default;
            source.HoldReleaseTime = default;
            source.HoldId = null;
            source.HoldBy = null;
            source.HoldByUsers = null;
            source.HoldUntilTimes = null;
            source.AppendHolds_Array = null;
            source.DisposalDueDate = null;
        }

        private void AssembleMovedInfoes(FsRecordProcessDto movedRecord, string sourcePath, string destinationPath)
        {
            var cache = FSJobCache.Instance;
            movedRecord.ConnectionGroupId = cache.AveConnectionGroupId;
            movedRecord.ConnectionId = cache.AveConnectionId;
            movedRecord.AuditLevel = sourcePath.Equals(cache.ConnectionPath) ? (int)FSJPMCAuditLevel.Connection : (int)FSJPMCAuditLevel.Folder;
            movedRecord.NewPath = destinationPath;
        }
    }

    public class UniqueIdAssignResult
    {
        private readonly ConcurrentBag<Guid> _failedNodeIds = new ConcurrentBag<Guid>();
        public IReadOnlyList<Guid> FailedNodeIds => _failedNodeIds.ToList();

        public int SuccessCount { get; private set; }

        public int FailureCount => _failedNodeIds.Count;

        internal void AddFailure(Guid nodeId)
        {
            _failedNodeIds.Add(nodeId);
        }

        internal void AddFailures(IEnumerable<Guid> nodeIds)
        {
            foreach (var nodeId in nodeIds)
            {
                _failedNodeIds.Add(nodeId);
            }
        }

        internal void IncrementSuccess()
        {
            SuccessCount++;
        }
    }
}
