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
using AvePoint.Application.StorageApiModern;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using Azure.Storage.Blobs.Models;
using Media.Common;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using Storage.Cloud.Azure;
using Storage.Cloud.Google;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using FileAlreadyExistException = Storage.Util.FileAlreadyExistException;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public class ArchiverFullMoveRetentionService : MoveServiceBase, IArchiverFullMoveRetentionService
    {
        private readonly IRALogger _logger;
        private readonly IStorageDeviceManager _deviceManager;
        private readonly IRMKeyValueDao _keyValueDao;

        private const string DEFAULT_STORAGE_ID = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

        private ArchiverFullMoveRetentionInfo _archiverInfo = new();
        private Action<JMArchiverRententionJobDetails>? _reportAction;
        private Action<JMArchiverRententionMigrationDetails>? _migrationReportAction;

        private string _dataVolume { get; set; } = string.Empty;
        private IXSystem _sourceDevice { get; set; } = default!;
        private IXSystem _destinationDevice { get; set; } = default!;

        private SafeDictionary<string, BLOBRehydrationMapping> _rehydrationMapping = [];
        private string _rehydrationTemp { get; set; } = string.Empty;

        private bool _isProcessingArchivedFile { get; set; }

        private readonly string _reportActionStr;

        public ArchiverFullMoveRetentionService()
        {
            _logger = RALogger.GetInstance(typeof(ArchiverFullMoveRetentionService));
            _deviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
            _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

            _reportActionStr = I18NEntity.GetString(_keyValueDao.IsEnableCopyToAnotherLocation() ? "RM_JS_Common_Copy" : "RM_PRM_PRE_Move");
        }

        /// <summary>
        /// Init the full move service with necessary information, such as source/destination device and data volume, and open the devices for write operations.
        /// This method must be called before performing any move operations.
        /// The provided report action will be used to log details of the retention job during the move process.
        /// </summary>
        /// <param name="archiverInfo"></param>
        /// <param name="reportAction"></param>
        public void Open(ArchiverFullMoveRetentionInfo archiverInfo, Action<JMArchiverRententionJobDetails> reportAction,
            Action<JMArchiverRententionMigrationDetails> migrationReportAction)
        {
            _archiverInfo = archiverInfo;
            _reportAction = reportAction;
            _dataVolume = archiverInfo.DataVolume;

            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDevice);
            _sourceDevice = XFactory.InstanceSystem(archiverInfo.SourceDevice.GetXRIS(PhysicalDeviceUsage.Data)[0]);
            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenIndexAndDataDeviceFinished);

            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDevice);
            _destinationDevice = _deviceManager.OpenDataSystemForWrite(archiverInfo.DestinationDevice);
            _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceOpenDestinationDataDeviceFinished);

            // Currently, we only support full move from Azure to Google Cloud storage
            if (_sourceDevice.StorageType != XStorageType.Azure || _destinationDevice.StorageType != XStorageType.GoogleCloud)
            {
                throw new Exception("Currently, only support full move from Azure to Google Cloud storage. Please check the source and destination device configuration.");
            }

            _rehydrationTemp = SafeCombinePath(_dataVolume, "Temp", Guid.NewGuid().ToString());

            _migrationReportAction = migrationReportAction;
        }

        /// <summary>
        /// Performs a full move operation of data for the current retention job asynchronously.
        /// </summary>
        /// <remarks>This method logs the start and completion of the move operation, as well as any
        /// errors encountered. It throws exceptions if preconditions are not met or if the operation is stopped or
        /// fails. The method should be awaited to ensure the move operation completes before proceeding.</remarks>
        public async Task FullMoveDataAsync()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                ValidatePreConditions();
                _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataBegin, _archiverInfo.JobId);
                var movedSize = await MoveDataAsync();
                _logger.Info(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataFinished, _archiverInfo.JobId, movedSize);
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format(MediaServiceArchiverBackupResource.RetentionServiceMoveJobDataError, _archiverInfo.JobId, ex);
                throw new Exception(errorMessage);
            }
            finally
            {
                sw.Stop();
                _logger.Info($"Full move operation completed for job {_archiverInfo.JobId} in {sw.Elapsed.TotalSeconds} seconds.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_rehydrationTemp))
                {
                    await _sourceDevice.DeleteAsync(XConvert.FromNames(_rehydrationTemp, string.Empty), true);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"An error occurred while deleting rehydration temp folder. error: {ex}");
            }
            if (_deviceManager is not null)
            {
                _deviceManager.Close(_sourceDevice);
                _deviceManager.Close(_destinationDevice);
            }
        }

        #region Private Methods

        private string SafeCombinePath(params string[] paths)
        {
            return SecurityUtils.SafeCombinePath(paths);
        }

        private void AddToReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            if (_reportAction != null && rententionJobDetails != null)
            {
                _reportAction(rententionJobDetails);
            }
        }

        private void AddToMigrationReport(JMArchiverRententionMigrationDetails migrationDetails)
        {
            if (_migrationReportAction != null && migrationDetails != null)
            {
                _migrationReportAction(migrationDetails);
            }
        }

        private string GetJobIdFromFileName(string fileName)
        {
            var parts = fileName.Split('_');
            return parts.Length >= 3 ? $"{parts[0]}_{parts[1]}_{parts[2]}" : string.Empty;
        }

        private void ValidatePreConditions()
        {
            if (_archiverInfo is null)
            {
                throw new InvalidOperationException("Retention info not initialized. Please call Open() first.");
            }
            if (_reportAction is null)
            {
                throw new InvalidOperationException("Report action is not set. Please call Open() first.");
            }
            if (string.IsNullOrEmpty(_dataVolume))
            {
                throw new InvalidOperationException(
                    "Data volume is not set. Cannot proceed with retention operation.");
            }
            if (_sourceDevice is null)
            {
                throw new InvalidOperationException(
                    "Source device is not opened. Device must be opened before retention. Please call Open() first.");
            }
            if (_destinationDevice is null)
            {
                throw new InvalidOperationException(
                    "Destination device is not opened. Device must be opened before retention. Please call Open() first.");
            }
            _logger.Info($"Preconditions validated successfully for job {_archiverInfo.JobId}");
        }

        #endregion

        #region Move Methods

        private async IAsyncEnumerable<XFileInfo> ListAllFilesAsync(StorageInfo dirInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var excludedPath = SafeCombinePath(_dataVolume, "Temp");
            if (_sourceDevice is IXCloudSystem cloudSystem)
            {
                await foreach (var file in cloudSystem.ListAllFilesAsync(dirInfo, cancellationToken))
                {
                    if (file is not null && !file.HighPlusLowName.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                        yield return file;
                }
            }
        }

        private async Task<BigInteger> MoveDataAsync()
        {
            BigInteger totalSize = 0;

            // Determine block length for file operations
            long blockLength = 100; // MB
            blockLength = ConnectionBuilder
                .ValueOf(_archiverInfo.DestinationDevice.GetXRIS(PhysicalDeviceUsage.Data).First())
                .GetInt64(XRIParameterKeys.BLOCK_LENGTH, blockLength);
            _logger.Info($"Block length for file operations: {blockLength} bytes, destination storage type: {_destinationDevice!.StorageType}");

            var deferredFiles = new List<XFileInfo>();
            int totalFileCount = 0;

            using var cts = new CancellationTokenSource();
            var fileList = this.ListAllFilesAsync(XConvert.FromNames(_dataVolume, string.Empty), cts.Token);
            try
            {
                #region--- Stream, filter, start rehydration, and process non-archive files immediately

                await foreach (var file in fileList.WithCancellation(cts.Token))
                {
                    using (new CheckJobStopScope()) { }
                    totalFileCount++;

                    // Start rehydration for archive-tier blobs and defer them
                    await VerifyAndCopyArchiverToHotAsync(file, cts.Token);
                    if (_rehydrationMapping.ContainsKey(file.HighPlusLowName))
                    {
                        deferredFiles.Add(file);
                        continue;
                    }

                    // Process non-archive file immediately
                    var movedSize = await MoveFileAsync(file, blockLength, cts.Token);
                    totalSize += movedSize;
                }

                _logger.Info($"Full move mode: processing {totalFileCount} files ({deferredFiles.Count} deferred for rehydration) for job {_archiverInfo.JobId}");

                #endregion

                #region--- Wait for rehydration and process deferred archive-tier files

                if (deferredFiles.Count > 0)
                {
                    try
                    {
                        await WaitingRehydrationAsync(cts.Token);
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"An error occurred while waiting for blob rehydration. Error: {ex}");
                    }

                    _logger.Info($"Rehydration completed for {deferredFiles.Count} files, starting to move deferred files for job {_archiverInfo.JobId}");
                    _isProcessingArchivedFile = true;
                    foreach (var file in deferredFiles)
                    {
                        using (new CheckJobStopScope()) { }
                        var movedSize = await MoveFileAsync(file, blockLength, cts.Token);
                        totalSize += movedSize;
                    }
                }

                #endregion
            }
            catch
            {
                await cts.CancelAsync();
                throw;
            }
            return totalSize;
        }

        private async Task<long> MoveFileAsync(XFileInfo item, long blockLength, CancellationToken cancellationToken)
        {
            XFileInfo srcInfo = item;
            var destInfo = item.ToCorrectTypeStorageInfo(_destinationDevice);
            var originalInfo = XConvert.FromNames(item.HighName, item.Name);
            if (_rehydrationMapping.ContainsKey(originalInfo.HighPlusLowName))
            {
                srcInfo = (XFileInfo)_rehydrationMapping[originalInfo.HighPlusLowName].MappedBlobInfo;
            }
            else
            {
                srcInfo.MetaInfos["Platform"] = ServiceConstants.DocAve;
                srcInfo.MetaInfos["Component"] = "ArchiverBackup";
                srcInfo.MetaInfos["Archive-JobId"] = _archiverInfo.JobId;
                // Preserve existing meta info if possible
                originalInfo.MetaInfos.AddRange(srcInfo.MetaInfos, true);
            }
            if (item.FileSize > 0)
            {
                srcInfo.Length = item.FileSize;
                _logger.Debug("Using FileSize {0} from ListFiles for {1}", item.FileSize, item.LowName);
            }
            else
            {
                // Defensive: fallback to OpenFile only if FileSize not available
                _logger.Warn("FileSize not available from ListFiles for {0}, performing OpenFile", item.Name);
                srcInfo.Length = (await _sourceDevice.OpenFileAsync(srcInfo))?.FileSize ?? 0;
            }
            var report = new JMArchiverRententionJobDetails()
            {
                Size = "0",
                Status = JobDetailsStatus.Successful,
                Comment = string.Empty,
                SrcStorageName = srcInfo.LowName,
                Action = _reportActionStr,
            };
            try
            {
                var storageResult = await RealMoveAsync(srcInfo, destInfo, blockLength, cancellationToken);
                if (!_keyValueDao.IsEnableCopyToAnotherLocation())
                {
                    var deleteResult = _sourceDevice.DeleteFileExt(originalInfo);
                    if (!deleteResult.IsDeleted)
                    {
                        report.Status = JobDetailsStatus.Exception;
                        report.Comment = deleteResult.Message;
                    }
                }
                report.Size = srcInfo.Length.ToString();
            }
            catch (FileAlreadyExistException)
            {
                report.Status = JobDetailsStatus.Skipped;
                report.Comment = $"File {destInfo.HighPlusLowName} already exists, so skip it.";
                report.Size = srcInfo.Length.ToString();
            }
            catch (Exception ex)
            {
                report.Comment = $"Error moving file {srcInfo.HighPlusLowName} to {destInfo.HighPlusLowName}: {ex}";
                report.Status = JobDetailsStatus.Failed;
                AddToReport(report);
                return 0;
            }
            finally
            {
                if (report.Status == JobDetailsStatus.Successful)
                {
                    AddToMigrationReport(new JMArchiverRententionMigrationDetails()
                    {
                        SiteUrl = string.Empty,
                        SharePointUrl = string.Empty,
                        SrcStorageName = _archiverInfo.SourceDevice.Name,
                        DesStorageName = _archiverInfo.DestinationDevice.Name,
                        BlobPath = srcInfo.HighPlusLowName.Replace('\\', '/'),
                        Status = report.Status,
                        Size = srcInfo.Length.ToString(),
                        JobId = GetJobIdFromFileName(report.SrcStorageName),
                        Comment = report.Comment,
                        Action = _reportActionStr,
                    });
                }
            }
            AddToReport(report);
            return srcInfo.Length;
        }

        private async Task<StorageResult> RealMoveAsync(StorageInfo sourceInfo, StorageInfo destinationInfo, long blockLength = 100, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult;
            double totalMB = sourceInfo.Length * 1.0 / XConstants.MB;
            var sw = Stopwatch.StartNew();
            // KeepFileTier feature has the same control key-value pair with ArchiverFullMoveRetention job
            KeepFileTier(_sourceDevice.StorageType, sourceInfo, _destinationDevice.StorageType, destinationInfo);
            if (totalMB > blockLength)
            {
                _logger.Info($"Source file is larger than block length, use MoveLargeItemAsync. Source: {sourceInfo.LowName}, Size: {totalMB:F2}MB");
                storageResult = await MoveLargeItemAsync(sourceInfo, _sourceDevice, destinationInfo, _destinationDevice, false, cancellationToken);
            }
            else
            {
                _logger.Info($"Source file is smaller than block length, use MoveSmallItemAsync. Source: {sourceInfo.LowName}, Size: {totalMB:F2}MB");
                storageResult = await MoveSmallItemAsync(sourceInfo, _sourceDevice, destinationInfo, _destinationDevice, false, cancellationToken);
            }
            sw.Stop();
            var throughputMBps = sw.Elapsed.TotalSeconds > 0 ? totalMB / sw.Elapsed.TotalSeconds : 0;
            _logger.Info($"File moved throughput metrics: Size: {totalMB:F2}MB, Time: {sw.Elapsed.TotalSeconds:F2}s, Throughput: {throughputMBps:F2}MB/s");
            return storageResult ?? new StorageResult();
        }

        private void KeepFileTier(XStorageType sourceType, StorageInfo sourceInfo, XStorageType destinationType, StorageInfo destinationInfo)
        {
            if ((sourceType == XStorageType.Azure && destinationType == XStorageType.GoogleCloud)
                && (sourceInfo is AzureCloudInfo azureSrcInfo && destinationInfo is GoogleCloudInfo gcDestInfo))
            {
                // Handle rehydration scenario because the archived file in Azure will be rehydrated to hot tier before moving.
                if (_isProcessingArchivedFile)
                {
                    gcDestInfo.StorageClass = GoogleStorageClass.Archive;
                }
                else
                {
                    gcDestInfo.StorageClass = azureSrcInfo.FileTierType.ToGoogleStorageClass();
                }
            }
        }

        public static string ConvertToSiteUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            input = input.Replace("\\", "/");
            int start = input.IndexOf("https#", StringComparison.OrdinalIgnoreCase);
            if (start < 0)
                return input;
            var encoded = input.Substring(start);
            var parts = encoded.Split('#', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return input;
            string scheme = parts[0]; // https
            string host = parts[2];   // domain
            var url = $"{scheme}://{host}";
            if (parts.Length > 3)
            {
                var path = string.Join("/", parts.Skip(3));
                url = SecurityUtils.SafeCombinePath(url, path);
            }
            return url;
        }

        #endregion

        #region Support Rehydration
        private async Task VerifyAndCopyArchiverToHotAsync(XFileInfo info, CancellationToken cancellationToken)
        {
            if (info is not null && info is AzureCloudInfo azureFile && azureFile.FileTierType == AccessTierType.Archive)
            {
                string tempPath = SafeCombinePath(_rehydrationTemp, info.HighName.Substring(info.HighName.IndexOf("DataVolume") + "DataVolume".Length).TrimStart('\\', '/'));
                if (!_rehydrationMapping.ContainsKey(info.HighPlusLowName))
                {
                    AzureCloudInfo tempInfo = new() { HighName = tempPath, LowName = info.LowName, FileTierType = AccessTierType.Hot };
                    StorageCopyResult copyResult = new();
                    if (_sourceDevice is XLibrary xLibrary)
                    {
                        try
                        {
                            if (xLibrary.GetWorkingSystem().SystemID.EqualsIgnoreCase(DEFAULT_STORAGE_ID))
                            {
                                string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(CommonUtilityForSpecialTenant.StorageStringType.DefaultStorage);
                                var client = Util.MSAzure.StorageUtil.GetContainerClient(defaultConnectionString, TenantLocalValue.LogonGroupId);
                                var scrBlobClient = client.GetBlobClient(info.HighPlusLowName);
                                var desBlobClient = client.GetBlobClient(tempInfo.HighPlusLowName);
                                var response = await desBlobClient.StartCopyFromUriAsync(scrBlobClient.Uri, new BlobCopyFromUriOptions
                                {
                                    AccessTier = AccessTier.Hot
                                }, cancellationToken);
                                if (response is not null && response.HasCompleted && response.HasValue)
                                {
                                    copyResult.IsCopyed = true;
                                }
                            }
                            else
                            {
                                copyResult = await _sourceDevice.CopyFileAsync(azureFile, tempInfo, true);
                            }
                        }
                        catch
                        {
                            _logger.Error($"Some thing went wrong when copy file, storage id: {xLibrary.GetWorkingSystem().SystemID}");
                            copyResult = await _sourceDevice.CopyFileAsync(azureFile, tempInfo, true);
                        }
                    }
                    else
                    {
                        copyResult = await _sourceDevice.CopyFileAsync(azureFile, tempInfo, true);
                    }
                    if (copyResult.IsCopyed)
                    {
                        _rehydrationMapping.Add(info.HighPlusLowName, new()
                        {
                            AlreadyRehydration = false,
                            MappedBlobInfo = tempInfo,
                            StartTime = DateTime.Now
                        });
                    }
                }
            }
        }

        private async Task WaitingRehydrationAsync(CancellationToken cancellationToken)
        {
            DateTime startTime = DateTime.Now;
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    bool needContinueSleep = false;
                    foreach (var r in _rehydrationMapping)
                    {
                        using (new CheckJobStopScope()) { }
                        if (!r.Value.AlreadyRehydration)
                        {
                            var file = _sourceDevice.OpenFile(r.Value.MappedBlobInfo);
                            if (file is AzureCloudInfo azureFile)
                            {
                                if (!azureFile.Exists || azureFile.FileTierType == AccessTierType.Archive)
                                {
                                    _logger.Info($"The {r.Key} need to rehydration, " +
                                        $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                        $"exists: {azureFile.Exists} , " +
                                        $"start time: {r.Value.StartTime.ToString()}");
                                    needContinueSleep = true;
                                    break;
                                }
                                else
                                {
                                    _logger.Info($"The {r.Key} already rehydration, " +
                                        $"mapping data: {r.Value.MappedBlobInfo.ToString()}, " +
                                        $"exists: {azureFile.Exists} , " +
                                        $"start time: {r.Value.StartTime.ToString()}");
                                    r.Value.AlreadyRehydration = true;
                                }
                            }
                        }
                    }

                    if (needContinueSleep && DateTime.Now - startTime < TimeSpan.FromDays(5))
                    {
                        _logger.Info("Will sleep 15 min to wait blob rehydration.");
                        await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
                    }
                    else
                    {
                        if (needContinueSleep)
                        {
                            // Timeout exceeded without full rehydration
                            var elapsed = DateTime.Now - startTime;
                            throw new TimeoutException($"Blob rehydration timed out after {elapsed.TotalDays:F2} days (max 5 days) for job {_archiverInfo.JobId}");
                        }
                        _logger.Info($"Exit waiting blob rehydration, all the datas rehydration : {!needContinueSleep} .");
                        break;
                    }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("Job stopped, stop Rehydration.");
                throw;
            }
        }
        #endregion
    }
}
