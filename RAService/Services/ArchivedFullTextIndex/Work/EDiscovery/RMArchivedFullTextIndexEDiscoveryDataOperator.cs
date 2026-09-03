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
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;
using AvePoint.GCommon.Contract.FileUploader.Object;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.Extentions;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.EDiscovery
{
    public abstract class RMArchivedFullTextIndexEDiscoveryDataOperator
    {
        private static readonly string s_connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        protected const long SINGLE_FILE_SIZE_LIMIT = 300L * 1024 * 1024;

        protected const long SINGLE_FILE_ITEM_COUNT_LIMIT = 1_000_000;

        protected const string CATEGORY_NAME = "RestoreFullTextIndex";

        protected readonly RALogger _logger;

        protected abstract Cloud.Sdk.Data.EDiscovery.IndexType OperateType { get; }

        protected abstract string OperateName { get; }

        protected readonly RMArchivedFullTextIndexSiteManager _siteManager;

        protected readonly RMArchivedFullTextIndexJobManager _jobManager;

        protected readonly RMArchivedFullTextIndexSyncJobManager _syncJobManager;

        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private readonly Dictionary<long, RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1> _runningJobInfoesV1 = [];

        private readonly EDiscoveryApiClient _apiClient;

        private BlobContainerClient _containerClient;

        private string _zipFolderPath;

        private string _blobFolderPath;

        private string _dataFolderPath;

        protected readonly Dictionary<string, string> _dataFolderPaths = [];

        protected string _dataFilePath;

        private bool _succeed = true;

        protected sealed class AppendState
        {
            public long DataSize { get; set; }

            public long DataCount { get; set; }
        }

        public RMArchivedFullTextIndexEDiscoveryDataOperator(
            RMArchivedFullTextIndexSiteManager siteManager,
            RMArchivedFullTextIndexJobManager jobManager,
            RMArchivedFullTextIndexSyncJobManager syncJobManager
            )
        {
            _logger = RALogger.GetInstance(GetType());
            _apiClient = AosApiUtility.GetEDiscoveryApiClient();
            _siteManager = siteManager;
            _jobManager = jobManager;
            _syncJobManager = syncJobManager;
            InitStorage();
        }

        private IndexInfo GetIndexInfo(string sasToken)
        {
            var indexInfo = new IndexInfo
            {
                Category = CATEGORY_NAME,
                DocSeperator = new DocSeparator
                {
                    Field = new()
                    {
                        Name = "archiverTime",
                        FieldType = FieldType.Long | FieldType.NeedIndex | FieldType.NeedStore
                    },
                    Type = SeparatorType.Month
                },
                BlobSasToken = sasToken,
                Type = OperateType,
                DeleteBlob = true
            };
            if(OperateType == Cloud.Sdk.Data.EDiscovery.IndexType.Upsert)
            {
                indexInfo.FildNameAsTerm = "pathMd5";
            }
            return indexInfo;
        }

        private void InitStorage()
        {
            var eDiscoveryFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "e_discovery");
            EnsureDirectory(eDiscoveryFolderPath);

            var jobFolderPath = SecurityUtils.SafeCombinePath(eDiscoveryFolderPath, _syncJobManager.ArchiverJobId);
            EnsureDirectory(jobFolderPath);

            _dataFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "data");
            EnsureDirectory(_dataFolderPath);

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, CATEGORY_NAME);
            EnsureDirectory(_dataFolderPath);

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, OperateName);
            EnsureDirectory(_dataFolderPath);

            _dataFilePath = SecurityUtils.SafeCombinePath(_dataFolderPath, $"{OperateName}_data.txt");

            _zipFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "zip");
            EnsureDirectory(_zipFolderPath);

            _zipFolderPath = SecurityUtils.SafeCombinePath(_zipFolderPath, CATEGORY_NAME);
            EnsureDirectory(_zipFolderPath);

            _zipFolderPath = SecurityUtils.SafeCombinePath(_zipFolderPath, OperateName);
            EnsureDirectory(_zipFolderPath);

            var siteFolderPath = new ArchiverVolumeGenerator().GenerateSitePath(_siteManager.SiteUrl);
            _blobFolderPath = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, siteFolderPath, _jobManager.JobId, _syncJobManager.ArchiverJobId, CATEGORY_NAME, OperateName);

            _containerClient = StorageUtil.GetContainerClient(s_connectionString, "e-discovery-container");
            _containerClient.CreateIfNotExists();
        }

        protected static void EnsureDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        protected async Task UploadDataAsync(string dateFolderPath = "")
        {
            if(!File.Exists(_dataFilePath))
            {
                return;
            }

            using (new PerformanceScope($"Upload data to ED [{OperateName}]", $"[{_siteManager.SiteUrl}]", true))
            {
                var zipName = $"data.zip";
                var zipPath = SecurityUtils.SafeCombinePath(_zipFolderPath, zipName);
                if (!string.IsNullOrEmpty(dateFolderPath))
                {
                    ZipFile.CreateFromDirectory(dateFolderPath, zipPath);
                }
                else
                {
                    ZipFile.CreateFromDirectory(_dataFolderPath, zipPath);
                }
                _logger.Info($"The [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data has been zip.");

                try
                {
                    var blobFolderPath = SecurityUtils.SafeCombinePath(_blobFolderPath, $"{_syncJobManager.ArchiverJobId}_{DateTime.UtcNow.Ticks}");
                    var blobPath = SecurityUtils.SafeCombinePath(blobFolderPath, zipName);

                    using (new PerformanceScope($"Upload data to storage [{OperateName}]", $"[{_siteManager.SiteUrl}]", true))
                    {
                        await _containerClient.UploadBlobWithRetryAsync(zipPath, blobPath);
                    }

                    _logger.Info($"The [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data zip has been upload to storage [{blobPath}].");

                    var convertedBlobFolderPath = blobFolderPath.Replace("\\", "/");
                    var sasToken = GenerateSasToken(convertedBlobFolderPath);

                    using (new PerformanceScope($"Trigger ED's job [{OperateName}]", $"[{_siteManager.SiteUrl}]", true))
                    {
                        await TriggerJobAsync(sasToken);
                    }

                    _logger.Info($"[{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] data has been uploaded.");
                }
                finally
                {
                    File.Delete(_dataFilePath);
                    File.Delete(zipPath);
                    _logger.Info($"[{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] has been deleted local file and zip.");
                }
            }
        }

        protected async Task AppendLineAndUploadIfNeededAsync(
            string line,
            AppendState appendState,
            string writeLogMessage,
            string reachLimitLogMessage,
            string appendDateFolderPath = "")
        {
            if (appendState.DataCount == 0 && appendState.DataSize == 0)
            {  
                File.Create(_dataFilePath).Dispose();
            }

            using (new PerformanceScope(writeLogMessage, $"[{_siteManager.SiteUrl}]", true))
            {
                using (var fileStream = File.Open(_dataFilePath, FileMode.Append))
                {
                    using var streamWriter = new StreamWriter(fileStream);
                    await streamWriter.WriteLineAsync(line);
                }
            }

            appendState.DataSize += Encoding.Default.GetByteCount(line);
            appendState.DataCount++;

            if (!(appendState.DataCount >= SINGLE_FILE_ITEM_COUNT_LIMIT || appendState.DataSize >= SINGLE_FILE_SIZE_LIMIT))
            {
                return;
            }

            _logger.Info(reachLimitLogMessage);

            await UploadDataAsync(appendDateFolderPath ?? string.Empty);

            appendState.DataCount = 0;
            appendState.DataSize = 0;
        }

        private async Task TriggerJobAsync(string sasToken)
        {
            try
            {
                var indexInfo = GetIndexInfo(sasToken);
                var result = await _apiClient.IndexService.AddWithRetryAsync(indexInfo);
                _succeed &= result.Successful;

                _logger.Info($"The [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] has been create e-discovery job [{result.JobId}]. Succeed: [{result.Successful}]. Error code: [{result.ErrorCode}].");

                var jobInfo = new RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1
                {
                    FullTextIndexJobId = _syncJobManager.Id,
                    EDiscoveryJobId = result.JobId,
                    EDiscoveryJobState = result.Successful ? JobState.Running : JobState.Failed,
                    EDiscoveryErrorCode = result.ErrorCode,
                    StartTime = DateTime.UtcNow.Ticks,
                    EndTime = DateTime.UtcNow.Ticks,
                    IndexType = OperateType
                };

                await _archivedFullTextIndexDao.AddOrUpdateEDiscoveryJobInfoAsync(jobInfo);
                if(result.Successful)
                {
                    _runningJobInfoesV1.Add(jobInfo.Id, jobInfo);
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while trigger [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] job for part data to e-discovery. Error: {e}");
                _succeed &= false;
            }
        }

        private string GenerateSasToken(string blobFolderPath)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = "e-discovery-container",
                Resource = "b",
                BlobName = string.Empty,
                StartsOn = DateTimeOffset.UtcNow.AddSeconds(-60),
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.All);
            var token = string.Empty;
            if (s_connectionString.StartsWith("DefaultEndpointsProtocol="))
            {
                var dictionary = StorageUtil.ParseConnectionString(s_connectionString);
                var sharedKeyCredential = new StorageSharedKeyCredential(dictionary["AccountName"], dictionary["AccountKey"]);
                token = sasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString();
            }
            else
            {
                var utcNow = DateTime.UtcNow;
                var delegationKey = StorageUtil.GetServiceClient(s_connectionString).GetUserDelegationKey(utcNow.AddMinutes(-5), utcNow.AddDays(7)).Value;
                var accountName = s_connectionString.Substring(0, s_connectionString.IndexOf("."));
                token = sasBuilder.ToSasQueryParameters(delegationKey, accountName).ToString();
            }

            _logger.Info($"Begin GenerateSasToken GetBlobClient.FolderPath:{blobFolderPath}.");
            var blobClient = _containerClient.GetBlobClient(blobFolderPath);
            _logger.Info($"Finish GenerateSasToken GetBlobClient.FolderPath:{blobFolderPath}.");
            var tokenStr = $"{blobClient.Uri}?{token}";
            return tokenStr;
        }

        public async Task<bool> WaitAsync()
        {
            if (OperateName == "ItemAppend")
            {
                foreach (var dataFolderPath in _dataFolderPaths.Values)
                {
                    await UploadDataAsync(dataFolderPath);
                }
            }
            else
            {
                await UploadDataAsync(_dataFolderPath);    
            }

            using (new PerformanceScope($"Wait ED's jobs complete [{OperateName}]", $"[{_siteManager.SiteUrl}]", true))
            {
                while (_runningJobInfoesV1.Count > 0)
                {
                    _logger.Info($"The [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] has running e-discovery job count [{_runningJobInfoesV1.Count}].");

                    foreach (var jobInfo in _runningJobInfoesV1.Values.ToList())
                    {
                        try
                        {
                            var eDiscoveryJob = await _apiClient.IndexService.GetJobWithRetryAsync(jobInfo.EDiscoveryJobId);
                            var isRunning = eDiscoveryJob.State == JobState.Running || eDiscoveryJob.State == JobState.None || eDiscoveryJob.State == JobState.Waiting;
                            var isTimeout = isRunning && ((eDiscoveryJob.StartedTime.HasValue && eDiscoveryJob.StartedTime < DateTime.UtcNow.AddHours(-2).Ticks) ||
                             (eDiscoveryJob.UpdatedTime.HasValue && eDiscoveryJob.UpdatedTime < DateTime.UtcNow.AddMinutes(-30).Ticks));
                            if (!isTimeout && isRunning)
                            {
                                continue;
                            }

                            _succeed &= eDiscoveryJob.State == JobState.Finished;
                            _logger.Info($"The [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] e-discovery job [{eDiscoveryJob.Id}] is [{eDiscoveryJob.State}]. timeout [{isTimeout}]");
                            jobInfo.StartTime = eDiscoveryJob.StartedTime ?? 0;
                            jobInfo.EndTime = eDiscoveryJob.FinishedTime ?? 0;
                            jobInfo.EDiscoveryJobState = isTimeout ? JobState.Failed : eDiscoveryJob.State;
                            await _archivedFullTextIndexDao.AddOrUpdateEDiscoveryJobInfoAsync(jobInfo);

                            _runningJobInfoesV1.Remove(jobInfo.Id);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"An error occurred while check e-discovery [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] job [{jobInfo.EDiscoveryJobId}] state. Error: {e}");
                            _runningJobInfoesV1.Remove(jobInfo.Id);
                            _succeed &= false;
                        }
                    }

                    if (_runningJobInfoesV1.Count > 0)
                    {
                        await Task.Delay(20 * 1000);
                    }
                }
            }

            _logger.Info($"Finished check [{OperateName}] [{_siteManager.SiteUrl}] [{_syncJobManager.ArchiverJobId}] all e-discovery jobs.");

            return _succeed;
        }
    }
}
