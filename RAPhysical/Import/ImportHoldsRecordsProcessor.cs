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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using DnsClient.Protocol;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Import
{
    public class ImportHoldsRecordsProcessor
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ImportHoldsRecordsProcessor));

        private static readonly IJobInfoUpdater JobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private static readonly IRMSubJobDao SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
        private static readonly IExplorerService ExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
        private static readonly IHoldDao HoldDao = (IHoldDao)PlatformWindsorManager.GetService(typeof(IHoldDao));
        private static readonly RA.DB.Explorer.Dao.IExplorerDao ExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
        private static readonly IUserService UserService = (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
        private static IRMRemoteNodeService RMRemoteNodeService = (IRMRemoteNodeService)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeService));
        private readonly string mJobId;
        private readonly string mParentJobId;
     
        private bool HasSucceedDetail { get; set; }
        private bool HasFailedDetail { get; set; }
        private string JobComment { get; set; }
        private List<HoldAssignmentBatch> physicalBatches = new List<HoldAssignmentBatch>();
        private Dictionary<string, string> aveSiteIds = new Dictionary<string, string>();

        private static readonly string[] SharePointOnlineSources = { "SharePoint Online", "SPO" };
        private static readonly string[] TeamsSources = { "Teams", "TE" };
        private static readonly string[] ExchangeOnlineSources = { "Exchange Online", "EXO" };
        private static readonly string[] PhysicalRecordSources = { "Physical Records", "PR" };
        private static readonly string[] FileSystemSources = { "File System", "FS" };
        private static readonly string[] SharePointOnPremSources = { "SharePoint On-Premises", "SPOP" };
        private static readonly string[] OneDriveSources = { "OneDrive", "OD" };
        private static readonly string[] AzureFileShareSources = { "Azure File Share", "AFS" };
        private static readonly string[] ConnectorSources = { "Connector" };
        private static readonly string[] BoxSources = { "Box" };

        private static readonly Dictionary<string, int> ContentSourceToFlag = BuildContentSourceToFlagMap();

        public ImportHoldsRecordsProcessor(string jobId, string parentJobId)
        {
            mJobId = jobId;
            mParentJobId = parentJobId;
            ReportMangerFactory.Instance.Init(mJobId, JobType.ImportHoldRecords);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
        }

        private static Dictionary<string, int> BuildContentSourceToFlagMap()
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            AddAliases(map, SharePointOnlineSources, (int)SourceFlag.SharePoint);
            AddAliases(map, TeamsSources, (int)SourceFlag.Teams);
            AddAliases(map, ExchangeOnlineSources, (int)SourceFlag.Exchange);
            AddAliases(map, PhysicalRecordSources, (int)SourceFlag.Physical);
            AddAliases(map, FileSystemSources, (int)SourceFlag.FileSystem);
            AddAliases(map, SharePointOnPremSources, (int)SourceFlag.SharePointOnPrem);
            AddAliases(map, OneDriveSources, (int)SourceFlag.OneDrive);
            AddAliases(map, AzureFileShareSources, (int)SourceFlag.AzureFileShare);
            AddAliases(map, ConnectorSources, (int)SourceFlag.Connector);
            AddAliases(map, BoxSources, (int)SourceFlag.Box);

            return map;
        }

        private static void AddAliases(Dictionary<string, int> map, IEnumerable<string> aliases, int sourceFlag)
        {
            foreach (var alias in aliases)
            {
                if (!string.IsNullOrWhiteSpace(alias))
                {
                    map[alias.Trim()] = sourceFlag;
                }
            }
        }

        private static bool TryMapContentSource(string contentSource, out int sourceFlag)
        {
            sourceFlag = (int)SourceFlag.None;
            if (string.IsNullOrWhiteSpace(contentSource))
            {
                return false;
            }

            return ContentSourceToFlag.TryGetValue(contentSource.Trim(), out sourceFlag);
        }

        public async System.Threading.Tasks.Task RunAsync()
        {
            logger.Info($"ImportHoldsRecordsProcessor start. JobId: {mJobId}, ParentJobId: {mParentJobId}.");
            RMSubJob subJob = SubJobDao.GetSubJob(mJobId, true);
            string blobName = subJob.JobContext?.Content;

            string localFilePath = Path.Combine(Path.GetTempPath(), $"HoldImport_{mJobId}.csv");
            try
            {
                RAStorageUtil.DownloadReportBlobToFile(blobName, localFilePath);

                await ProcessCsvAsync(localFilePath);

                if (!HasSucceedDetail && !HasFailedDetail)
                {
                    HasSucceedDetail = true;
                }

                logger.Info("Import hold records complete.");
            }
            catch (Exception ex)
            {
                HasFailedDetail = true;
                JobComment = ex.Message;
                logger.Error($"Import hold records failed, error: {ex}");
            }
            finally
            {
                TryDeleteLocalFile(localFilePath);
                var jobFinishStatus = HasSucceedDetail && HasFailedDetail
                    ? JobStatus.FinishWithException
                    : (HasFailedDetail ? JobStatus.Failed : JobStatus.Finished);
                ReportManager.SetJobFinished(jobFinishStatus, JobComment);
            }
        }

        private async Task ProcessCsvAsync(string filePath)
        {
            logger.Info($"Start parsing hold import CSV. FilePath: {filePath}");

            var allHolds = HoldDao.GetAllHolds((int)HoldProfileType.All)?.ToDictionary(h => h.Name?.Trim(), h => h, StringComparer.OrdinalIgnoreCase)
                           ?? new Dictionary<string, RMHold>(StringComparer.OrdinalIgnoreCase);
            var loggedInUser = TenantLocalValue.LogonUserEmail;

            using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8);
            string headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                logger.Warn("CSV file is empty or missing header.");
                return;
            }

            ValidateHeaders(headerLine);
            var headerIndexes = BuildHeaderIndex(headerLine);
            var requiredKeys = new[]
            {
                I18NEntity.GetString("RM_PRM_PRE_Column_HoldType"),
                I18NEntity.GetString("RM_PRM_PRE_Column_Name"),
                I18NEntity.GetString("RM_PRM_PRE_Column_ID"),
                I18NEntity.GetString("RM_PRM_PRE_Column_HoldRecordPath"),
                I18NEntity.GetString("RM_PRM_PRE_Column_ContentSource"),
            };

            var missingColumns = requiredKeys
                .Where(col => !headerIndexes.ContainsKey(NormalizeHeader(col)))
                .ToList();

            if (missingColumns.Count > 0)
            {
                var missing = string.Join(", ", missingColumns);
                logger.Warn($"CSV header is missing required columns: {missing}.");
                throw new Exception($"CSV header is missing required columns.");
            }

            int holdTitleIndex = headerIndexes[NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_HoldType"))];
            int nameIndex = headerIndexes[NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_Name"))];
            int uniqueIdIndex = headerIndexes[NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_ID"))];
            int contentSourceIndex = headerIndexes[NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_ContentSource"))];
            int locationUrlIndex = headerIndexes[NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_HoldRecordPath"))];
            headerIndexes.TryGetValue(NormalizeHeader(I18NEntity.GetString("RM_PRM_PRE_Column_HoldBy")), out int holdByIndex);

            logger.Info($"CSV header indexes resolved. Name={nameIndex}, UniqueId={uniqueIdIndex}, Url={locationUrlIndex}, HoldTitle={holdTitleIndex}, HoldBy={holdByIndex}, ContentSource={contentSourceIndex}.");

            int rowCount = 0;
            string line = string.Empty;
            bool special = false;
            string rowStr = string.Empty;
            var buffer = new List<HoldCsvRowItem>();

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                rowStr += line;
                int remainder = (line.Split(new char[] { '"' }, StringSplitOptions.None).Length - 1) % 2;
                if (remainder != 0)
                {
                    if (special)
                    {
                        special = false;
                    }
                    else
                    {
                        rowStr += Environment.NewLine;
                        special = true;
                        continue;
                    }
                }
                else if (special)
                {
                    rowStr += Environment.NewLine;
                    continue;
                }

                rowCount++;
                var columns = CSVHelper.AnalyseCSVRow2Array(rowStr);
                rowStr = string.Empty;

                string name = GetColumnValue(columns, nameIndex);
                string uniqueId = GetColumnValue(columns, uniqueIdIndex);
                string contentSource = GetColumnValue(columns, contentSourceIndex);
                string locationUrl = GetColumnValue(columns, locationUrlIndex);
                string holdTitle = GetColumnValue(columns, holdTitleIndex);
                string holdBy = GetColumnValue(columns, holdByIndex);

                if (!string.IsNullOrWhiteSpace(holdBy))
                {
                    try
                    {
                        var account = await UserService.GetUserByNameAsync(holdBy);
                        if (account == null)
                        {
                            logger.Info($"Row {rowCount}: HoldBy user '{holdBy}' not found in Opus. Use import user '{loggedInUser}'.");
                            holdBy = loggedInUser;
                        }
                        else
                        {
                            holdBy = !string.IsNullOrWhiteSpace(account.DisplayName) ? account.DisplayName : (!string.IsNullOrWhiteSpace(account.Email) ? account.Email : holdBy);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Row {rowCount}: Validate HoldBy user '{holdBy}' failed: {ex.Message}. Use import user '{loggedInUser}'.");
                        holdBy = loggedInUser;
                    }
                }
                else
                {
                    holdBy = loggedInUser;
                }

                if (string.IsNullOrWhiteSpace(holdTitle))
                {
                    logger.Warn($"Row {rowCount}: Hold title is empty.");
                    AddJobDetail(name, locationUrl, holdTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_Missing_HoldType");
                    continue;
                }

                if (!allHolds.TryGetValue(holdTitle.Trim(), out RMHold targetHold))
                {
                    logger.Warn($"Row {rowCount}: Hold '{holdTitle}' not found.");
                    AddJobDetail(name, locationUrl, holdTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_NotFound_HoldType");
                    continue;
                }

                buffer.Add(new HoldCsvRowItem
                {
                    RowNumber = rowCount,
                    Name = name,
                    UniqueId = uniqueId,
                    Url = locationUrl,
                    HoldTitle = holdTitle,
                    HoldBy = holdBy,
                    ContentSource = contentSource,
                    TargetHold = targetHold
                });

                if (buffer.Count < 1000)
                {
                    continue;
                }

                logger.Info($"Processing CSV batch. BufferCount={buffer.Count}, ParsedRows={rowCount}.");
                var items = ProcessBatchRowsAsync(buffer);
                if (items.Any())
                {
                    logger.Info($"Batch produced {items.Count} hold batch(es). Start assignment.");
                    ProcessAssignmentsAsync(items);
                }
                else
                {
                    logger.Info("Batch produced no hold update DTO.");
                }

                buffer.Clear();
            }

            if (buffer.Count > 0)
            {
                logger.Info($"Processing tail CSV batch. BufferCount={buffer.Count}, ParsedRows={rowCount}.");
                var items = ProcessBatchRowsAsync(buffer);
                if (items.Any())
                {
                    logger.Info($"Tail batch produced {items.Count} hold batch(es). Start assignment.");
                    ProcessAssignmentsAsync(items);
                }
                else
                {
                    logger.Info("Tail batch produced no hold update DTO.");
                }
            }

            if (physicalBatches.Any())
            {
                logger.Info($"Start deferred physical assignment. PhysicalBatchCount={physicalBatches.Count}.");
               ProcessAssignmentsAsync(physicalBatches);
                physicalBatches.Clear();
            }

            logger.Info($"Finished parsing hold import CSV. TotalRowsParsed={rowCount}.");
        }

        private List<HoldAssignmentBatch> ProcessBatchRowsAsync(List<HoldCsvRowItem> rows)
        {
            var records = PreloadRecords(rows);
            var batches = new List<HoldAssignmentBatch>();
            var batchCache = new Dictionary<string, HoldAssignmentBatch>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (TryMapContentSource(row.ContentSource, out int sourceFlag) && sourceFlag == (int)SourceFlag.Google)
                {
                    AddJobDetail(row.Name, row.Url, row.HoldTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_NotSupport_Google");
                    continue;
                }
                var record = FindRecordInBatch(row, records);
                if (record == null)
                {
                    AddJobDetail(row.Name, row.Url, row.HoldTitle, JobDetailsStatus.Failed, "RM_PRM_PRE_NotFound_Item");
                    continue;
                }
                string holdId = row.TargetHold.Id;
                if (IsRecordAlreadyInHold(record, holdId))
                {
                    logger.Info($"Row {row.RowNumber}: Record '{record.Id}' already belongs to hold '{row.TargetHold.Name}', skipped.");
                    AddJobDetail(record.LeafName, row.Url, row.HoldTitle, JobDetailsStatus.Skipped, "RM_PRM_PRE_Existed_HoldType");
                    continue;
                }
                if (record.SourceFlag == (int)SourceFlag.Physical && record.NodeType != (int)RMNodeType.PhyBox)
                {
                    if (record.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        logger.Info($"Row {row.RowNumber}: Physical record node skipped. RecordId={record.Id}.");
                        continue;
                    }

                    ProcessPhysicalRecord(row, record, holdId, batchCache);
                    continue;
                }
                if (record.SourceFlag != (int)SourceFlag.FileSystem && record.NodeType == (int)RMNodeLevel.Folder)
                {
                    logger.Info($"Row {row.RowNumber}: Folder node type is skipped because we can't assign holds to folders except Physical or File systems folders. RecordId={record.Id}.");
                    continue;
                }
                ProcessDefaultRecord(row, record, holdId, batchCache, batches);
            }
            aveSiteIds.Clear();
            return batches;
        }


        private List<RA.DB.Explorer.Model.Record> PreloadRecords(List<HoldCsvRowItem> rows)
        {
            var uniqueIds = rows
                .Select(r => r.UniqueId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return new List<RA.DB.Explorer.Model.Record>();
            }

            return ExplorerDao
                .QueryAll(r =>
                    uniqueIds.Contains(r.RecordsId))
                .ToList();
        }

        private void ProcessPhysicalRecord(
            HoldCsvRowItem row,
            RA.DB.Explorer.Model.Record record,
            string holdId,
            Dictionary<string, HoldAssignmentBatch> batchCache)
        {
            string cacheKey = $"{holdId}_{row.HoldBy}_{record.SourceFlag}_{record.NodeType}_{record.BoxId}";

            if (!batchCache.TryGetValue(cacheKey, out var batch))
            {
                batch = new HoldAssignmentBatch { Dto = CreateHoldDto(row, RecordsConstants.RecordHold_PhyProfile) };
                batchCache[cacheKey] = batch;
                physicalBatches.Add(batch);
            }

            AddRelatedIdIfNotExist(batch.Dto, record.Id);
            batch.Rows.Add(row);
        }

        private void ProcessDefaultRecord(
            HoldCsvRowItem row,
            RA.DB.Explorer.Model.Record record,
            string holdId,
            Dictionary<string, HoldAssignmentBatch> batchCache,
            List<HoldAssignmentBatch> batches)
        {
            string cacheKey = $"{holdId}_{row.HoldBy}_{record.SourceFlag}_{record.NodeType}";

            if (!batchCache.TryGetValue(cacheKey, out var batch))
            {
                bool isPhysicalBox =
                    record.SourceFlag == (int)SourceFlag.Physical &&
                    record.NodeType == (int)RMNodeType.PhyBox;

                int holdCategory = isPhysicalBox
                    ? RecordsConstants.RecordHold_PhyProfile
                    : RecordsConstants.RecordHold_Default;

                var dto = CreateHoldDto(row, holdCategory);
                dto.NeedCheckOverride = isPhysicalBox;
                dto.IsOverRide = false;
                dto.FileIds = new List<CompactRecord>();

                batch = new HoldAssignmentBatch { Dto = dto };
                batchCache[cacheKey] = batch;
                batches.Add(batch);
            }

            AddRelatedIdIfNotExist(batch.Dto, record.Id);
            AddCompactRecordIfNotExist(batch.Dto, record);
            batch.Rows.Add(row);
        }

        private static void AddCompactRecordIfNotExist(
            UpdateHoldDto dto,
            RA.DB.Explorer.Model.Record record)
        {
            if (dto.FileIds == null)
            {
                dto.FileIds = new List<CompactRecord>();
            }

            if (dto.FileIds.Any(item => item.Id == record.Id))
            {
                return;
            }

            dto.FileIds.Add(new CompactRecord
            {
                Id = record.Id,
                NodeType = (RMNodeType)record.NodeType,
                BoxId = record.BoxId
            });
        }

        public static (string SiteUrl, string RelativePath) SplitUrl(string fullUrl, int contentSource)
        {
            if (string.IsNullOrWhiteSpace(fullUrl))
                return (string.Empty, string.Empty);

            if (contentSource == (int)SourceFlag.FileSystem)
            {
                string cleanUrl = fullUrl.TrimEnd('/', '\\');
                string normalizedUrl = cleanUrl.Replace('/', '\\');
                int lastSlashIndex = normalizedUrl.LastIndexOf('\\');

                if (lastSlashIndex == -1)
                    return (string.Empty, cleanUrl);

                string directoryPath = cleanUrl.Substring(0, lastSlashIndex);
                string fileName = cleanUrl.Substring(lastSlashIndex + 1);

                return (directoryPath, fileName);
            }

            int markerIndex = fullUrl.IndexOf("/sites/", StringComparison.OrdinalIgnoreCase);
            if (markerIndex == -1)
            {
                markerIndex = fullUrl.IndexOf("/personal/", StringComparison.OrdinalIgnoreCase);
            }

            if (markerIndex == -1)
                return (fullUrl, string.Empty);

            int secondSlash = fullUrl.IndexOf('/', markerIndex + 1);
            if (secondSlash == -1)
                return (fullUrl, fullUrl.Substring(markerIndex));

            int siteNameEndIndex = fullUrl.IndexOf('/', secondSlash + 1);
            if (siteNameEndIndex == -1)
                return (fullUrl, fullUrl.Substring(markerIndex));

            string siteUrl = fullUrl.Substring(0, siteNameEndIndex);
            string relativePath = fullUrl.Substring(markerIndex);

            return (siteUrl, relativePath);
        }

        private static UpdateHoldDto CreateHoldDto(HoldCsvRowItem row, int holdCategory)
        {
            return new UpdateHoldDto
            {
                ReletedIds = new List<Guid>(),
                HoldSetting = new HoldSetting
                {
                    Id = row.TargetHold.Id,
                    Name = row.TargetHold.Name,
                    Type = (HoldDateType)row.TargetHold.HoldDateType,
                    Number = row.TargetHold.Number,
                    Unit = (HoldDateUnit)row.TargetHold.HoldUnit
                },
                HoldCategory = holdCategory
            };
        }

        private static void AddRelatedIdIfNotExist(UpdateHoldDto dto, Guid recordId)
        {
            if (!dto.ReletedIds.Contains(recordId))
            {
                dto.ReletedIds.Add(recordId);
            }
        }


        private RA.DB.Explorer.Model.Record FindRecordInBatch(HoldCsvRowItem row,List<RA.DB.Explorer.Model.Record> records)
        {

            if (!string.IsNullOrWhiteSpace(row.UniqueId) && records != null && records.Count > 0)
            {
                var recordByUniqueId = records.FirstOrDefault(r => string.Equals(r.RecordsId, row.UniqueId.Trim(), StringComparison.OrdinalIgnoreCase));
                if (recordByUniqueId != null)
                {
                    logger.Info($"Row {row.RowNumber}: Matched by UniqueID '{row.UniqueId}'.");
                    return recordByUniqueId;
                }
            }
            if (!string.IsNullOrWhiteSpace(row.Url) && !string.IsNullOrWhiteSpace(row.ContentSource) && TryMapContentSource(row.ContentSource, out int sourceFlag) && sourceFlag != (int)SourceFlag.Physical && sourceFlag != (int)SourceFlag.Exchange && sourceFlag != (int)SourceFlag.Connector)
            {
                var record = FindRecordByUrl(row.Url, sourceFlag);
                if(record != null)
                {
                    return record;
                }
            }

            return null;
        }

        private RA.DB.Explorer.Model.Record FindRecordByUrl(string url, int contentSource)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            bool isFileSystem = contentSource == (int)SourceFlag.FileSystem;
            var (siteUrl, relativePath) = SplitUrl(url, contentSource);

            if (isFileSystem)
            {
                string dirPath = siteUrl;
                string leafName = relativePath;

                return ExplorerDao.FindRecordBySiteAndPath(aveSiteId: null, dirPath: dirPath, leafName: leafName, isFileSystem: true);
            }

            if (string.IsNullOrWhiteSpace(siteUrl))
                return null;

            if (!aveSiteIds.TryGetValue(siteUrl, out string aveSiteId))
            {
                var remoteSite = RMRemoteNodeService.GetRemoteSiteCollectionByUrl(siteUrl);
                if (remoteSite == null)
                    return null;

                aveSiteId = remoteSite?.id.ToString();
                aveSiteIds[siteUrl] = aveSiteId;
            }

            return ExplorerDao.FindRecordBySiteAndPath(aveSiteId: aveSiteId, dirPath: relativePath, leafName: null, isFileSystem: false);
        }

        private void ProcessAssignmentsAsync(List<HoldAssignmentBatch> batches)
        {
            logger.Info($"Start assigning hold for batch list. Count={batches?.Count ?? 0}.");

            foreach (var batch in batches)
            {
                var updateHoldDto = batch.Dto;
                var rows = batch.Rows;
                var holdTitle = updateHoldDto?.HoldSetting?.Name ?? string.Empty;
                if (ExplorerService.IsPhysicalRecord(updateHoldDto.ReletedIds.FirstOrDefault()))
                {
                    var cannotHold = ExplorerService.IsFolderHasParentHold(updateHoldDto.ReletedIds, out List<string> holdingBoxes);
                    if (cannotHold)
                    {
                        var boxes = holdingBoxes == null ? string.Empty : string.Join(", ", holdingBoxes);
                        logger.Info($"Skip physical hold assignment because parent box already holds item. Hold='{holdTitle}', Records={updateHoldDto.ReletedIds.Count}, Boxes='{boxes}'.");
                        continue;
                    }
                }
                try
                {
                    ExplorerService.AssignRecordsToHoldAsync(updateHoldDto, rows.FirstOrDefault()?.HoldBy);
                    logger.Info($"Assigned hold successfully. Hold='{holdTitle}', Records={updateHoldDto.ReletedIds.Count}.");
                    foreach (var row in batch.Rows)
                    {
                        AddJobDetail(row.Name, row.Url, row.HoldTitle, JobDetailsStatus.Successful, string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"AssignRecordsToHoldAsync failed. Hold='{holdTitle}', Records={updateHoldDto?.ReletedIds?.Count ?? 0}, ex: {ex}");
                }
            }
        }
        
        private static void ValidateHeaders(string headerLine)
        {
            var headers = CSVHelper.AnalyseCSVRow2Array(headerLine);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new List<string>();

            foreach (var header in headers)
            {
                var key = NormalizeHeader(header);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!seen.Add(key))
                {
                    duplicates.Add(header.Trim());
                }
            }
            if (duplicates.Count > 0)
            {
                var dup = string.Join(", ", duplicates.Distinct());
                logger.Warn($"CSV header contains duplicate columns: {dup}.");
                throw new Exception($"CSV header contains duplicate columns: {dup}.");
            }
        }

        private static Dictionary<string, int> BuildHeaderIndex(string headerLine)
        {
            var headers = CSVHelper.AnalyseCSVRow2Array(headerLine);
            var indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                var key = NormalizeHeader(headers[i]);
                if (!indexMap.ContainsKey(key))
                {
                    indexMap.Add(key, i);
                }
            }
            return indexMap;
        }

        private static string GetColumnValue(string[] columns, int index)
        {
            if (index >= 0 && index < columns.Length)
            {
                return columns[index]?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string NormalizeHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Empty;
            }

            return header.Trim().Replace(" ", string.Empty).Replace("/", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        private static bool IsRecordAlreadyInHold(RA.DB.Explorer.Model.Record record, string holdId)
        {
            if (record == null || string.IsNullOrWhiteSpace(holdId))
            {
                return false;
            }

            if (string.Equals(record.HoldId, holdId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (record.AppendHolds_Array != null && record.AppendHolds_Array.Any(h => string.Equals(h, holdId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private void AddJobDetail(string name, string url, string holdTitle, JobDetailsStatus status, string comment)
        {
            ReportManager.SendJobDetail(new JMHoldRecordsImportJobDetail
            {
                Name = name,
                Url = url,
                HoldTitle = holdTitle,
                Status = status,
                Comment = comment
            });

            if (status == JobDetailsStatus.Failed)
            {
                HasFailedDetail = true;
            }
            else
            {
                HasSucceedDetail = true;
            }
        }

        private class HoldCsvRowItem
        {
            public int RowNumber { get; set; }
            public string Name { get; set; }
            public string UniqueId { get; set; }
            public string Url { get; set; }
            public string HoldTitle { get; set; }
            public string HoldBy { get; set; }
            public string ContentSource { get; set; }
            public RMHold TargetHold { get; set; }
        }
        private class HoldAssignmentBatch
        {
            public UpdateHoldDto Dto { get; set; }

            public List<HoldCsvRowItem> Rows { get; set; } = new List<HoldCsvRowItem>();
        }
        private static void TryDeleteLocalFile(string filePath)
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
                logger.Warn($"Failed to delete temp file '{filePath}': {ex.Message}");
            }
        }
    }
}
