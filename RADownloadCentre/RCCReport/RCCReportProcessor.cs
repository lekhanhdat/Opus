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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using Newtonsoft.Json;
using RADownloadCenter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Record = AvePoint.RA.DB.Explorer.Model.Record;

namespace RADownloadCentre.RCCReport
{
    public class RCCReportProcessor
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(RCCReportProcessor));
        private readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private readonly IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();
        private readonly IFileSystemSettingDao FileSystemSettingDao = PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        private readonly IMyhubReportJobDao MyhubReportJobDao = PlatformWindsorManager.GetService<IMyhubReportJobDao>();
        private IExplorerDao _explorerDao;
        private IExplorerDao ExplorerDao => _explorerDao ??= new ExplorerDao();

        private readonly string JobId;
        private readonly string FolderPath;
        private readonly RCCReportRequest _request;
        private readonly long _filterStartTicks;
        private readonly long _filterEndTicks;

        private GeneralSettingModel _generalSettings;
        private string _generatedDateTimeStr;
        private string _fileName;
        private string _displayName;
        private string _connectionName;
        private bool _isMyHub;

        private List<FSConnection> _cachedGroupConnections;
        private List<string> _allDisablePaths;
        private List<KeyValuePair<string, bool>> _allDeactivePaths;
        private List<KeyValuePair<string, bool>> _allConfiguredNodes;
        private bool _isGroupEnableDownloadRcc;
        private int _groupEnableRecordManagement;
        private bool _isGroupActive;

        private readonly Dictionary<string, bool> _pathEligibilityCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private const string StartFileName = "RCC_Report_";
        private const int ColumnCount = 14;
        private const int MaxRecordsPerFile = 1000000;

        private static readonly Dictionary<string, string> RetentionPeriodUnitMap = new()
        {
            { "0", "" },
            { "4", "day(s)" },
            { "5", "week(s)" },
            { "6", "month(s)" },
            { "7", "year(s)" },
        };

        #region I18N Column Headers
        private readonly string Col_ConnectionName = I18NEntity.GetString("RM_JS_BCM_Export_ConnectionNameColumn");
        private readonly string Col_FileName = I18NEntity.GetString("RM_JS_DC_FileName");
        private readonly string Col_FolderName = I18NEntity.GetString("RM_RC_Request_ApproveMsg_FolderName");
        private readonly string Col_FullPath = I18NEntity.GetString("RM_EBR_FullPath");
        private readonly string Col_FileExtension = I18NEntity.GetString("RM_RCCReport_Col_FileExtension");
        private readonly string Col_FileType = I18NEntity.GetString("RM_JS_JM_Discovery_Report_FileType");
        private readonly string Col_CreateDate = I18NEntity.GetString("RM_RCCReport_Col_CreateDate_WithOutUTC");
        private readonly string Col_LastModifiedDate = I18NEntity.GetString("RM_RCCReport_Col_LastModifiedDate_WithOutUTC");
        private readonly string Col_LastAccessedDate = I18NEntity.GetString("RM_RCCReport_Col_LastAccessedDate_WithOutUTC");
        private readonly string Col_SizeKB = I18NEntity.GetString("RM_RCCReport_Col_FileSize");
        private readonly string Col_RCCName = I18NEntity.GetString("RM_RCCReport_Col_RCCName");
        private readonly string Col_RCCNameCountryCode = I18NEntity.GetString("RM_RCCReport_Col_RCCNameCountryCode");
        private readonly string Col_EventDate = I18NEntity.GetString("RM_RCCReport_Col_EventDate_WithOutUTC");
        private readonly string Col_DispositionEligibilityDate = I18NEntity.GetString("RM_RCCReport_Col_DispositionEligibilityDate_WithOutUTC");
        private readonly string Col_RetentionPeriod = I18NEntity.GetString("RM_Audit_Stub_RetentionPeriod");
        #endregion

        public RCCReportProcessor(string jobId, RCCReportRequest request)
        {
            if (string.IsNullOrEmpty(jobId))
                throw new ArgumentNullException(nameof(jobId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            JobId = jobId;
            _request = request;
            _isMyHub = request.IsMyHub;

            GenerateAndUploadFileManager.Init(jobId, JobType.DownloadRCCReport);
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(
                new BaseJobDto { Id = jobId, JobType = (int)JobType.DownloadRCCReport });

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            _generalSettings = GeneralSettingService.GetGeneralSettingAsync().Result;

            _generalSettings.TimeZoneId = request.TimeZoneId;
            _generalSettings.DayLight = request.IsDaylight;
            _filterStartTicks = request.TimeRange.StartDateTicks;
            _filterEndTicks = request.TimeRange.EndDateTicks;
        }

        public async Task RunAsync()
        {
            var reportProfile = DownloadDataInfoDao
                .GetDownloadDataInfosByStatus(new List<int> { (int)DownloadContentJobStatus.Wait })
                .FirstOrDefault(item => item.JobId == JobId);

            if (reportProfile == null)
            {
                GenerateAndUploadFileManager.HasFailed = true;
                Logger.Error("Cannot find report download info for RCC report. JobId: {0}", JobId);
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
                return;
            }

            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    reportProfile.JobStatus = (int)DownloadContentJobStatus.InProgress;


                    await InitializeSettingsAsync(reportProfile);
                    WarmCaches();

                    if (!TryResolveFileName(_request.ConnectionId))
                    {
                        GenerateAndUploadFileManager.HasFailed = true;
                        Logger.Error("Failed to resolve file name for connectionId [{0}].", _request.ConnectionId);
                        reportProfile.JobStatus = (int)DownloadContentJobStatus.Failed;
                        await DownloadDataInfoDao.UpdateAsync(reportProfile);
                        await MyhubReportJobDao.UpdateStatusByJobId(reportProfile.JobId, MyhubReportJobStatus.Failed);

                        return;
                    }
                    // store fileName
                    var rccReportContentList = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(reportProfile.ExtendString1);

                    if (rccReportContentList != null && rccReportContentList.Count > 0)
                    {
                        rccReportContentList[0].DisplayName = $"{_displayName}";

                        reportProfile.ExtendString1 = JsonConvert.SerializeObject(rccReportContentList);
                    }

                    await DownloadDataInfoDao.UpdateAsync(reportProfile);
                    await MyhubReportJobDao.UpdateStatusByJobId(reportProfile.JobId, MyhubReportJobStatus.InProgress);

                    CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                    var nodes = _request.Nodes;
                    if (nodes == null || nodes.Count == 0)
                    {
                        GenerateAndUploadFileManager.HasFailed = true;
                        GenerateAndUploadFileManager.JobComment = "No records found in the selected scope and time range.";
                        Logger.Warn("No nodes to export for RCC report. JobId: {0}", JobId);
                        //reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;
                        //await DownloadDataInfoDao.UpdateAsync(reportProfile);
                        //await MyhubReportJobDao.UpdateStatusByJobId(reportProfile.JobId, MyhubReportJobStatus.Finished);

                        //return;
                    }
                    else
                    {
                        GenerateReportCsv(GetRecordsStreamFromNodes(nodes));
                    }
                    Logger.Info("RCC report CSV generated successfully.");

                    long fileSize = await UploadBlobAsync();

                    reportProfile.FileSize = fileSize;
                    reportProfile.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();
                    reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;

                    if (rccReportContentList != null && rccReportContentList.Count > 0)
                    {
                        rccReportContentList[0].DisplayName = $"{_displayName.Substring(0, _displayName.Length - 4)}_{_generatedDateTimeStr}.zip";
                        if (_request.IsMyHub)
                        {
                            reportProfile.Name = rccReportContentList[0].DisplayName;
                        }
                        reportProfile.ExtendString1 = JsonConvert.SerializeObject(rccReportContentList);
                    }

                    await DownloadDataInfoDao.UpdateAsync(reportProfile);
                    await MyhubReportJobDao.UpdateStatusByJobId(reportProfile.JobId, MyhubReportJobStatus.Finished);

                    Logger.Info("RCC report completed successfully. JobId: {0}", JobId);
                }
            }
            catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
            {
                Logger.Error($"Download RCC job was stopped by user.");
                throw;
            }
            catch (Exception e)
            {
                reportProfile.JobStatus = (int)DownloadContentJobStatus.Failed;
                await DownloadDataInfoDao.UpdateAsync(reportProfile);
                await MyhubReportJobDao.UpdateStatusByJobId(reportProfile.JobId, MyhubReportJobStatus.Failed);
                GenerateAndUploadFileManager.HasFailed = true;
                GenerateAndUploadFileManager.JobComment = e.Message;
                Logger.Error("RCC report generation failed. JobId: {0}, Error: {1}", JobId, e);
            }
            finally
            {
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
            }
        }

        private async Task InitializeSettingsAsync(RMDownloadDataInfo reportProfile)
        {
            if (reportProfile.FileDownloadTime > 0)
            {
                var timeModel = GeneralSettingService.ConvertTiksToDateTime(_generalSettings, reportProfile.FileDownloadTime, false);
                _generatedDateTimeStr = timeModel.DataTime.ToString("yyyyMMdd_HHmmss");
            }
            else
            {
                _generatedDateTimeStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            }
        }

        private void WarmCaches()
        {
            Guid groupId = _request.ConnGroupId;
            _cachedGroupConnections = FSConnectionDao.GetAllConnectionsByGroupId(groupId) ?? new List<FSConnection>();

            _allDisablePaths = FileSystemSettingDao.GetAllDisableRecordManagementPath(groupId) ?? new List<string>();
            _allConfiguredNodes = FileSystemSettingDao.GetAllNodeRCCSettings(groupId) ?? new List<KeyValuePair<string, bool>>();
            _allDeactivePaths = FileSystemSettingDao.GetAllDeactivePath(groupId) ?? new List<KeyValuePair<string, bool>>();
            _isGroupEnableDownloadRcc = FileSystemSettingDao.IsConnGroupEnableDownloadRCC(groupId);
            _isGroupActive = FileSystemSettingDao.IsConnGroupActive(groupId);

            var fsGroupSetting = FileSystemSettingDao.LoadFSSetting(groupId, groupId);
            _groupEnableRecordManagement = fsGroupSetting == null ? (int)AvePoint.RA.Contract.Global.Object.EnableRecordManagementSetting.Enable : fsGroupSetting.EnableRecordManagement;

            Logger.Info("Warmed caches for groupId [{0}]: [{1}] connections, [{2}] disable paths, [{3}] RCC settings.",
                groupId, _cachedGroupConnections.Count, _allDisablePaths.Count, _allConfiguredNodes.Count);
        }

        #region File Name Resolution

        private bool TryResolveFileName(Guid connectionId)
        {
            var connection = _cachedGroupConnections.FirstOrDefault(c => c.Id == connectionId);
            if (connection == null)
            {
                Logger.Warn("Connection not found in cache for connectionId [{0}].", connectionId);
                GenerateAndUploadFileManager.JobComment = $"Connection not found for id [{connectionId}].";
                return false;
            }

            string jpmcId = SanitizeFileName(connection.JPMCConnectionId);
            string connName = SanitizeFileName(connection.Name ?? connection.UNCPath);

            if (string.IsNullOrWhiteSpace(jpmcId) && string.IsNullOrWhiteSpace(connName))
            {
                Logger.Warn("Connection [{0}] produced an empty file name after sanitization.", connectionId);
                AddJobDetail(connection.Name ?? connectionId.ToString(), connection.UNCPath,
                    isSuccess: false, comment: "File name is empty after removing invalid characters.");
                GenerateAndUploadFileManager.JobComment = "Connection name and JPMCId are empty after sanitization.";
                return false;
            }

            jpmcId = TruncateString(jpmcId, 100);
            _connectionName = TruncateString(connName, 100);
            _fileName = $"{StartFileName}{jpmcId}_{_connectionName}_{_generatedDateTimeStr}.csv";
            _displayName = $"{StartFileName}{jpmcId}_{_connectionName}.zip";
            return true;
        }

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (Array.IndexOf(invalidChars, c) >= 0)
                {
                    sb.Append('_');
                }
                else if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }

            string sanitized = sb.ToString();
            sanitized = Regex.Replace(sanitized, @"_{2,}", "_");

            return sanitized.Trim();
        }

        #endregion

        #region Generate Report

        private void GenerateReportCsv(IEnumerable<Record> recordsStream)
        {
            int currentRecordCount = 0;
            int fileIndex = 0;
            int totalFailedCount = 0;

            string baseFileName = _fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? _fileName.Substring(0, _fileName.Length - 4)
                : _fileName;

            StreamWriter sw = null;

            try
            {
                foreach (var record in recordsStream)
                {
                    if (currentRecordCount == 0)
                    {
                        string currentFileName = fileIndex == 0
                            ? $"{baseFileName}.csv"
                            : $"{baseFileName}_{fileIndex:D3}.csv";

                        string csvPath = Path.Combine(FolderPath, currentFileName);

                        sw = new StreamWriter(csvPath, false, new UTF8Encoding(true));
                        WriteHeader(sw);
                    }

                    try
                    {
                        CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                        var row = ConvertRecordToRow(record);
                        sw.WriteLine(string.Join(",", row.Select(c => $"\"{EscapeCsv(c)}\"")));
                        AddJobDetail(record.LeafName, $"{record.DirPath}\\{record.LeafName}", isSuccess: true, comment: "Exported successfully.");
                    }
                    catch (AvePoint.RA.Contract.Global.Exceptions.JobStopException)
                    {
                        Logger.Error($"RCC report was stopped by user.");
                        throw;
                    }
                    catch (Exception e)
                    {
                        totalFailedCount++;
                        string fullPath = BuildFullPath(record?.DirPath, record?.LeafName);
                        Logger.Warn("Failed to write record [{0}]. Error: {1}", fullPath, e.Message);
                    }

                    currentRecordCount++;

                    if (currentRecordCount >= MaxRecordsPerFile)
                    {
                        sw?.Dispose();
                        sw = null;
                        currentRecordCount = 0;
                        fileIndex++;
                    }
                }

                if (totalFailedCount > 0)
                {
                    AddJobDetail("RCCReport", $"{totalFailedCount} record(s) failed to export.", isSuccess: false,
                        comment: $"{totalFailedCount} records could not be converted.");
                }
            }
            finally
            {
                sw?.Dispose();
            }
        }

        private void WriteHeader(StreamWriter sw)
        {
            sw.WriteLine($"\"{EscapeCsv(Col_ConnectionName)}\",\"{EscapeCsv(_connectionName)}\"");

            var headers = new[]
            {
                Col_FileName, Col_FolderName, Col_FullPath,
                Col_FileExtension, Col_FileType,
                Col_CreateDate, Col_LastModifiedDate, Col_LastAccessedDate,
                Col_SizeKB, Col_RCCName, Col_RCCNameCountryCode,
                Col_EventDate, Col_DispositionEligibilityDate, Col_RetentionPeriod
            };
            sw.WriteLine(string.Join(",", headers.Select(h => $"\"{EscapeCsv(h)}\"")));
        }

        private string[] ConvertRecordToRow(Record record)
        {
            var row = new string[ColumnCount];
            var metaInfo = ParseMetaInfo(record.MetaInfo);

            row[0] = record.LeafName ?? string.Empty;
            row[1] = ExtractFolderName(record.DirPath);
            row[2] = BuildFullPath(record.DirPath, record.LeafName);

            string ext = record.ExtensionForFile ?? string.Empty;
            if (string.IsNullOrEmpty(ext) && !string.IsNullOrEmpty(record.LeafName))
            {
                ext = Path.GetExtension(record.LeafName)?.TrimStart('.') ?? string.Empty;
            }

            row[3] = ext;
            row[4] = metaInfo?.FileTypeName ?? string.Empty;
            row[5] = FormatTicks(record.TimeCreated);
            row[6] = FormatTicks(record.TimeModified);
            row[7] = metaInfo != null ? FormatTicks(metaInfo.LastAccessTime) : string.Empty;

            long sizeBytes = record.JPMCFSFileSize > 0 ? record.JPMCFSFileSize : (metaInfo?.FileSize ?? 0);
            row[8] = FormatFileSize(sizeBytes);

            row[9] = record.ClassCode ?? string.Empty;
            row[10] = BuildClassCodeCombo(record.ClassCode, record.CountryCode);
            row[11] = FormatTicks(record.StartDate);
            row[12] = FormatTicks(record.EndTime);
            row[13] = BuildRetentionPeriod(record.PolicyValueNumber, record.PolicyValueUnit);

            return row;
        }

        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;

            string processed = field.Replace("\"", "\"\"");

            if (processed.Length > 0 && "=+-@".Contains(processed[0]))
            {
                processed = "'" + processed;
            }

            return processed;
        }

        #endregion

        #region Load Records By Scope (Optimized)

        private IEnumerable<Record> GetRecordsStreamFromNodes(List<RCCNode> nodes)
        {
            using (PerformanceScope scope0 = new PerformanceScope("RCCReportProcessor.GetRecordsStreamFromNodes"))
            {
                var processedRecordIds = new HashSet<Guid>();

                var validNodes = nodes.Where(n => n != null && n.Id != Guid.Empty).ToList();
                if (validNodes.Count == 0) yield break;

                if (_request.Level == (int)NodeLevel.FSFile)
                {
                    int batchSize = 1000;

                    for (int i = 0; i < validNodes.Count; i += batchSize)
                    {
                        var batchIds = validNodes.Skip(i).Take(batchSize).Select(n => n.Id).ToList();

                        var fileRecords = ExplorerDao.QueryAll(r =>
                            batchIds.Contains(r.Id) &&
                            r.RecordStatus == 1 &&
                            r.SourceFlag == (int)SourceFlag.FileSystem &&
                            (int)r.NodeType == (int)NodeLevel.FSFile)
                            .ToList();

                        foreach (var fileRecord in fileRecords)
                        {
                            if (!string.IsNullOrEmpty(fileRecord.DirPath) && IsNodeFullyEligibleCached(fileRecord.DirPath))
                            {
                                if (fileRecord.EndTime >= _filterStartTicks && fileRecord.EndTime <= _filterEndTicks)
                                {
                                    if (processedRecordIds.Add(fileRecord.Id))
                                    {
                                        yield return fileRecord;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var node in validNodes)
                    {
                        IEnumerable<Record> stream = null;

                        switch (_request.Level)
                        {
                            case (int)NodeLevel.SiteCollection:
                                var connection = _cachedGroupConnections.FirstOrDefault(c => c.Id == node.Id);
                                if (connection != null && IsNodeFullyEligibleCached(connection.UNCPath))
                                {
                                    stream = QueryRecordsByTraversing(node.Id.ToString(), node.FullPath);
                                }
                                break;

                            case (int)NodeLevel.FSFolder:
                                if (IsNodeFullyEligibleCached(node.FullPath))
                                {
                                    var parentConnection = FindParentConnection(node.FullPath);
                                    if (parentConnection != null)
                                    {
                                        stream = QueryRecordsByTraversing(parentConnection.Id.ToString(), node.FullPath);
                                    }
                                }
                                break;
                        }

                        if (stream != null)
                        {
                            foreach (var record in stream)
                            {
                                if (processedRecordIds.Add(record.Id))
                                {
                                    yield return record;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Cache-Based Lookup

        private FSConnection FindParentConnection(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;

            return _cachedGroupConnections
                .Where(c => !string.IsNullOrEmpty(c.UNCPath)
                    && fullPath.StartsWith(c.UNCPath, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.UNCPath.Length)
                .FirstOrDefault();
        }

        private bool IsNodeFullyEligibleCached(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;

            if (_pathEligibilityCache.TryGetValue(folderPath, out bool isEligible))
            {
                return isEligible;
            }

            isEligible = IsNodeFullyEligible(folderPath);
            _pathEligibilityCache[folderPath] = isEligible;
            return isEligible;
        }

        private bool IsNodeFullyEligible(string folderPath)
        {
            bool isRecordManagementEnabled = !CurrentNodeIsDisable(folderPath);

            bool isDownloadRccAllowed = false;
            if (_isMyHub)
            {
                isDownloadRccAllowed = !CurrentNodeIsDisableDownloadRCC(folderPath);
            }
            else
            {
                isDownloadRccAllowed = true;
            }

            bool isDeactive = !CurrentNodeIsDeactive(folderPath);

            return isRecordManagementEnabled && isDownloadRccAllowed && isDeactive;
        }

        private bool CurrentNodeIsDisable(string folderPath)
        {
            if (_allDisablePaths != null && _allDisablePaths.Count > 0)
            {
                var closestDisablePath = _allDisablePaths
                    .Where(p => folderPath.Equals(p, StringComparison.OrdinalIgnoreCase) ||
                                folderPath.StartsWith(p + "\\", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.Length)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(closestDisablePath)) return true;
            }
            return _groupEnableRecordManagement != (int)AvePoint.RA.Contract.Global.Object.EnableRecordManagementSetting.Enable;
        }

        private bool CurrentNodeIsDisableDownloadRCC(string folderPath)
        {
            if (_allConfiguredNodes != null && _allConfiguredNodes.Any())
            {
                var deepestNode = _allConfiguredNodes
                    .Where(node => folderPath.Equals(node.Key, StringComparison.OrdinalIgnoreCase) ||
                                   folderPath.StartsWith(node.Key + "\\", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(node => node.Key.Length)
                    .FirstOrDefault();

                if (deepestNode.Key != null)
                {
                    return !deepestNode.Value;
                }
            }
            return !_isGroupEnableDownloadRcc;
        }

        private bool CurrentNodeIsDeactive(string folderPath)
        {
            if (_allDeactivePaths != null && _allDeactivePaths.Any())
            {
                var deepestNode = _allDeactivePaths
                    .Where(node => folderPath.Equals(node.Key, StringComparison.OrdinalIgnoreCase) ||
                                   folderPath.StartsWith(node.Key + "\\", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(node => node.Key.Length)
                    .FirstOrDefault();

                if (deepestNode.Key != null)
                {
                    return !deepestNode.Value;
                }
            }
            return !_isGroupActive;
        }

        #endregion

        #region Targeted DB Queries for Records

        private IEnumerable<Record> QueryRecordsByTraversing(string connectionId, string startPath)
        {
            using (PerformanceScope scope0 = new PerformanceScope("RCCReportProcessor.QueryRecordsByTraversing"))
            {
                var queue = new Queue<string>();
                queue.Enqueue(startPath);

                while (queue.Count > 0)
                {
                    CheckJobStatusUtility.ThrowExceptionIfJobNeedStop();
                    string currentPath = queue.Dequeue();

                    bool isCurrentNodeEligible = IsNodeFullyEligibleCached(currentPath);

                    if (isCurrentNodeEligible)
                    {
                        int batchSize = 10000;
                        Guid? lastId = null;
                        bool hasMoreFiles = true;

                        while (hasMoreFiles)
                        {
                            var query = ExplorerDao.QueryAll(r =>
                                r.AveSiteId == connectionId &&
                                r.RecordStatus == 1 &&
                                r.SourceFlag == (int)SourceFlag.FileSystem &&
                                r.EndTime >= _filterStartTicks &&
                                r.EndTime <= _filterEndTicks &&
                                (int)r.NodeType == (int)NodeLevel.FSFile &&
                                r.DirPath == currentPath);

                            if (lastId.HasValue)
                            {
                                query = query.Where(r => r.Id.CompareTo(lastId.Value) > 0);
                            }

                            var filesBatch = query.OrderBy(r => r.Id).Take(batchSize).ToList();

                            if (filesBatch.Count == 0)
                            {
                                hasMoreFiles = false;
                                break;
                            }

                            foreach (var file in filesBatch)
                            {
                                yield return file;
                            }

                            lastId = filesBatch.Last().Id;
                        }
                    }

                    bool shouldTraverseDeeper = isCurrentNodeEligible || HasOverrideConfigUnderneath(currentPath);

                    if (!shouldTraverseDeeper)
                    {
                        continue;
                    }

                    var subFolders = ExplorerDao.QueryAll(r =>
                        r.AveSiteId == connectionId &&
                        r.RecordStatus == 1 &&
                        r.SourceFlag == (int)SourceFlag.FileSystem &&
                        (int)r.NodeType == (int)NodeLevel.FSFolder &&
                        r.DirPath == currentPath)
                        .ToList();

                    foreach (var folder in subFolders)
                    {
                        string nextPath = BuildFullPath(folder.DirPath, folder.LeafName);
                        queue.Enqueue(nextPath);
                    }
                }
            }
        }

        private bool HasOverrideConfigUnderneath(string currentPath)
        {
            string prefix = currentPath + "\\";

            if (_allConfiguredNodes != null && _allConfiguredNodes.Any(n => n.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        private Record QuerySingleFileRecord(Guid fileId)
        {
            try
            {
                return ExplorerDao.QueryAll(r =>
                    r.Id == fileId &&
                    r.RecordStatus == 1 &&
                    r.SourceFlag == (int)SourceFlag.FileSystem &&
                    (int)r.NodeType == (int)NodeLevel.FSFile)
                    ?.FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to query single file record [{0}]. Error: {1}", fileId, e);
                return null;
            }
        }

        #endregion

        #region Upload

        private async Task<long> UploadBlobAsync()
        {
            string zipPath = FolderPath + ".zip";
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, zipPath, Encoding.UTF8);

            var blobName = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, JobId + ".zip");

            await Retryer.RetryAsync(() =>
            {
                blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, zipPath);
                Logger.Info("RCC report uploaded to storage. BlobName: [{0}].", blobName);
                return Task.CompletedTask;
            });

            var zipFileInfo = new FileInfo(zipPath);
            return zipFileInfo.Exists ? zipFileInfo.Length : 0;
        }

        #endregion

        #region Helpers

        private static string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length > maxLength ? text[..maxLength] : text;
        }

        private static RecordMetaInfo ParseMetaInfo(string metaInfoJson)
        {
            if (string.IsNullOrEmpty(metaInfoJson)) return null;
            try
            {
                return JsonConvert.DeserializeObject<RecordMetaInfo>(metaInfoJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string ExtractFolderName(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath)) return string.Empty;
            int lastSep = dirPath.LastIndexOf('\\');
            return lastSep >= 0 ? dirPath[(lastSep + 1)..] : dirPath;
        }

        private static string BuildFullPath(string dirPath, string leafName)
        {
            if (string.IsNullOrEmpty(dirPath)) return string.Empty;
            if (string.IsNullOrEmpty(leafName)) return dirPath;
            return dirPath.TrimEnd('\\') + "\\" + leafName;
        }

        private static string BuildClassCodeCombo(string classCode, string countryCode)
        {
            if (string.IsNullOrEmpty(classCode) || string.IsNullOrEmpty(countryCode))
                return string.Empty;
            return $"{classCode} + {countryCode}";
        }

        private string FormatTicks(long ticks)
        {
            if (ticks <= 0) return string.Empty;
            try
            {
                return GeneralSettingService.ConvertTiksToDateTime(_generalSettings, ticks, true).SimplifyFormatTime ?? string.Empty;
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to format ticks [{0}]. Error: {1}", ticks, e.Message);
                return string.Empty;
            }
        }

        private static string BuildRetentionPeriod(string policyValueNumber, string policyValueUnit)
        {
            if (string.IsNullOrEmpty(policyValueNumber) || policyValueNumber == "0")
                return "0 day";

            string effectiveNumber = policyValueNumber;
            string effectiveUnit = policyValueUnit ?? "";

            if (effectiveUnit == "5" && int.TryParse(effectiveNumber, out int weeks))
            {
                effectiveNumber = (weeks * 7).ToString();
                effectiveUnit = "4";
            }

            string unitDisplay = RetentionPeriodUnitMap.TryGetValue(effectiveUnit, out var unit) ? unit : string.Empty;
            return string.IsNullOrEmpty(unitDisplay) ? string.Empty : $"{effectiveNumber} {unitDisplay}";
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "0";
            double sizeInKb = (double)bytes / 1024;
            return $"{Math.Round(sizeInKb, 2)}";
        }

        private void AddJobDetail(string objectName, string url, bool isSuccess = true, string comment = "")
        {
            var detail = new JMImportSPSettingDetail
            {
                ObjectName = objectName,
                Url = url,
                Status = isSuccess ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                Comment = comment,
            };

            if (isSuccess)
            {
                GenerateAndUploadFileManager.AddSucceedJobDetail(detail);
                return;
            }
            GenerateAndUploadFileManager.AddFailedJobDetail(detail);
        }

        #endregion
    }
}