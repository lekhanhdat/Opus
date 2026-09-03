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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Extension.FullTextIndex;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCFullTextIndexManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedSCFullTextIndexManager));

        protected const string CATEGORY_NAME = "RestoreFullTextIndex";
        private IRestoreSearchService _restoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();

        private readonly bool _enableFullTextIndex;
        private readonly bool _isGCPEnvironment;

        private readonly Dictionary<string, RMDeleteArchivedDataFullTextIndexRunner> _runners = new();
        private readonly HashSet<string> _noCategoryJobIds = new();
        private readonly EDiscoveryApiClient _apiClient;
        private readonly string _jobId;

        private readonly RMDeleteArchivedSCJobReportManager _reportManager;

        public RMDeleteArchivedSCFullTextIndexManager(RMDeleteArchivedSCJobReportManager reportManager)
        {
            _isGCPEnvironment = RMGlobalConfiguration.EnvSetting.IsGCPEnvironment;
            if (_isGCPEnvironment)
            {
                _logger.Warn($"Current environment is GCP, full text index feature is not supported.");
                return;
            }

            _enableFullTextIndex = _restoreSearchService.IsEnableFullTextIndexSearch();
            if (!_enableFullTextIndex)
            {
                _logger.Warn($"Current tenant did not enable full text index feature.");
                return;
            }

            _reportManager = reportManager;
            _apiClient = AosApiUtility.GetEDiscoveryApiClient();
        }

        public async Task<bool> DeleteAsync(ArchiverBasicIndex item)
        {
            try
            {
                if (_isGCPEnvironment || !_enableFullTextIndex) return true;

                var archiverJobId = item.JobId;

                if (_noCategoryJobIds.Contains(archiverJobId))
                {
                    _logger.Info($"The archiver job [{archiverJobId}] no category info found. Skip it [{item.Id}]");
                    return true;
                }

                if (!_runners.TryGetValue(archiverJobId, out var runner))
                {
                    runner = new RMDeleteArchivedDataFullTextIndexRunner(_apiClient, _reportManager);
                    _runners[archiverJobId] = runner;
                }

                return await runner.DeleteAsync(item);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete item [{item.Id}]. Error: {e}");
                return false;
            }
        }

        public async Task WaitAsync(string archivedJobId)
        {
            if (_isGCPEnvironment || !_enableFullTextIndex) return;


            if (_runners.TryGetValue(archivedJobId, out var runner))
            {
                _logger.Info($"Start to wait full text index job runner for archived job [{archivedJobId}].");
                await runner.WaitAsync();
                _logger.Info($"Finished wait full text index job runner for archived job [{archivedJobId}].");
            }
            else
            {
                _logger.Info($"No full text index job runner for archived job [{archivedJobId}].");
            }
        }

        public async Task FlushDataAsync()
        {
            if (_isGCPEnvironment || !_enableFullTextIndex) return;

            if (_runners.Count == 0)
            {
                _logger.Info($"No full text index data need to flush to e-discovery, category [{CATEGORY_NAME}].");
                return;
            }

            try
            {
                var flushingRunner = _runners.FirstOrDefault(r => r.Value != null);

                _logger.Info($"Flush full text index data to e-discovery, category [{CATEGORY_NAME}] using runner of archived subjob: {flushingRunner.Key}");
                await flushingRunner.Value.UploadDataAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while flush full text index data to e-discovery, category [{CATEGORY_NAME}]. Error: {e}");
            }
            finally
            {
                _reportManager.IncreaseProgress();
            }
        }

        public async Task WaitAllAsync()
        {
            if (_isGCPEnvironment || !_enableFullTextIndex) return;


            _logger.Info($"Start to wait all full text index job runners. runner count: {_runners.Count}");

            foreach (var runner in _runners)
            {
                _logger.Info($"Start to wait full text index job runner for archived job [{runner.Key}].");
                await runner.Value.WaitAsync();
                _logger.Info($"Finished wait full text index job runner for archived job [{runner.Key}].");
            }

            await SyncCategoryDataSizeAsync();
            _logger.Info($"Finished wait all full text index job runners.");
        }

        public async Task SyncCategoryDataSizeAsync()
        {
            if (_isGCPEnvironment || !_enableFullTextIndex) return;

            try
            {
                var res = await _apiClient.IndexService.CalculateCatalogSizeAsync(new SearchInfo()
                {
                    Category = CATEGORY_NAME,
                    Filter = []
                });
                if (!res.Successful)
                {
                    _logger.Error($"The calculate category total size failed. Skipped sync.");
                    return;
                }
                _logger.Info($"The total category data size is [{res.Size}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while sync category data size. Error: {e}");
            }
        }
    }

    public class RMDeleteArchivedDataFullTextIndexRunner
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataFullTextIndexRunner));

        private static readonly string s_connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private const long SINGLE_FILE_SIZE_LIMIT = 100L * 1024 * 1024;

        private const long SINGLE_FILE_ITEM_COUNT_LIMIT = 100_000;

        protected const string CATEGORY_NAME = "RestoreFullTextIndex";

        private readonly Dictionary<string, int> _runningJobIds = []; // jobId, retryCount

        private BlobContainerClient _containerClient;
        private string _zipFolderPath;
        private string _blobFolderPath;
        private string _dataFolderPath;
        private string _dataFilePath;
        private static long _appendedDataSize = 0;
        private static long _appendedDataCount = 0;

        private readonly EDiscoveryApiClient _apiClient;
        private readonly RMDeleteArchivedSCJobReportManager _reportManager;
        private readonly string _operationName = "DeleteItem";

        public RMDeleteArchivedDataFullTextIndexRunner(EDiscoveryApiClient apiClient, RMDeleteArchivedSCJobReportManager reportManager)
        {
            _apiClient = apiClient;
            _reportManager = reportManager;
            InitStorage();
        }

        public async Task<bool> DeleteAsync(ArchiverBasicIndex item)
        {
            try
            {
                var queryGroup = BuildDeleteQueryGroup(item);
                var queryGroupJson = JsonConvert.SerializeObject(queryGroup);
                //if (_appendedDataCount == 0 && _appendedDataSize == 0)
                //{
                //    File.Create(_dataFilePath).Dispose();
                //}

                using (var fileStream = File.Open(_dataFilePath, FileMode.Append))
                {
                    using var streamWriter = new StreamWriter(fileStream);
                    await streamWriter.WriteLineAsync(queryGroupJson);
                }

                _appendedDataSize += Encoding.Default.GetByteCount(queryGroupJson);
                _appendedDataCount++;

                _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] will be delete in e-discovery, category [{CATEGORY_NAME}].");

                if (!(_appendedDataCount >= SINGLE_FILE_ITEM_COUNT_LIMIT || _appendedDataSize >= SINGLE_FILE_SIZE_LIMIT))
                {
                    return true;
                }

                _logger.Info($"The full text index need to upload data to e-discovery, category [{CATEGORY_NAME}].");

                await UploadDataAsync();

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete site [{item.SitePath}] item [{item.PathMD5}] data, category [{CATEGORY_NAME}]. Error: {e}");
                return false;
            }
        }

        public async Task WaitAsync()
        {
            await UploadDataAsync();
            while (_runningJobIds.Count > 0)
            {
                _logger.Info($"The full text index has running e-discovery job count [{_runningJobIds.Count}], category [{CATEGORY_NAME}].");

                foreach (var eJob in _runningJobIds.ToList())
                {
                    try
                    {
                        var eDiscoveryJob = await _apiClient.IndexService.GetJobWithRetryAsync(eJob.Key);

                        var isRunning = eDiscoveryJob.State == JobState.Running || eDiscoveryJob.State == JobState.None || eDiscoveryJob.State == JobState.Waiting;

                        // timeout after 2 hours running or 30 minutes no update, consider it as failed and remove from running job list to avoid infinite waiting
                        var isTimeout = isRunning && ((eDiscoveryJob.StartedTime.HasValue && eDiscoveryJob.StartedTime < DateTime.UtcNow.AddHours(-2).Ticks) ||
                         (eDiscoveryJob.UpdatedTime.HasValue && eDiscoveryJob.UpdatedTime < DateTime.UtcNow.AddMinutes(-30).Ticks));

                        if (!isTimeout && isRunning)
                        {
                            _runningJobIds[eJob.Key] = eJob.Value + 1;
                            continue;
                        }

                        _logger.Info($"The [{_reportManager.JobType}][{_operationName}] e-discovery job [{eDiscoveryJob.Id}] is [{eDiscoveryJob.State}]. timeout [{isTimeout}], retryCount: {eJob.Value}, StartTime: {eDiscoveryJob.StartedTime}, EndTime: {eDiscoveryJob.FinishedTime}");
                        _runningJobIds.Remove(eJob.Key);
                        _reportManager.IncreaseProgress();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while check e-discovery job [{eJob.Key}] state. Error: {e}");
                        _runningJobIds.Remove(eJob.Key);
                    }
                }

                if (_runningJobIds.Count > 0)
                {
                    await Task.Delay(20 * 1000);
                }
            }
        }

        private static List<QueryGroup> BuildDeleteQueryGroup(ArchiverBasicIndex item)
        {
            return new List<QueryGroup>
            {
                new QueryGroup
                {
                    QueryFields = new List<FieldQuery>
                    {
                        new FieldQuery
                        {
                            Field = new Field
                            {
                                Name = "pathMd5", // or "indexDBUniqueId"
                                Value = item.PathMD5,
                                FieldType = FieldType.String
                            },
                            Operator = FilterOperator.And
                        }
                    },
                    Operator = FilterOperator.And
                }
            };
        }

        public async Task UploadDataAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                return;
            }
            var zipName = $"data.zip";
            var zipPath = SecurityUtils.SafeCombinePath(_zipFolderPath, zipName);
            ZipFile.CreateFromDirectory(_dataFolderPath, zipPath);
            _logger.Info($"The full text index data has been zip. AppendedDataCount: {_appendedDataCount}, AppendedDataSize:{_appendedDataSize}");

            try
            {
                var blobFolderPath = SecurityUtils.SafeCombinePath(_blobFolderPath, $"{DateTime.UtcNow.Ticks}");
                var blobPath = SecurityUtils.SafeCombinePath(blobFolderPath, zipName);

                await _containerClient.UploadBlobWithRetryAsync(zipPath, blobPath);

                _logger.Info($"The full text index data zip has been upload to storage [{blobPath}], category [{CATEGORY_NAME}].");

                var convertedBlobFolderPath = blobFolderPath.Replace("\\", "/");
                var sasToken = GenerateSasToken(convertedBlobFolderPath);

                await TriggerJobAsync(sasToken);

                _logger.Info($"The full text index data has been uploaded, category [{CATEGORY_NAME}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while upload full text index data, category [{CATEGORY_NAME}]. Error: {e}");
            }
            finally
            {
                _appendedDataCount = 0;
                _appendedDataSize = 0;

                File.Delete(_dataFilePath);
                File.Delete(zipPath);
                _logger.Info($"The full text index data has been deleted local file and zip, category [{CATEGORY_NAME}].");
            }
        }

        private async Task TriggerJobAsync(string sasToken)
        {
            try
            {
                var indexInfo = new IndexInfo
                {
                    Category = CATEGORY_NAME,
                    BlobSasToken = sasToken,
                    Type = Cloud.Sdk.Data.EDiscovery.IndexType.Delete,
                    DeleteBlob = true
                };
                var result = await _apiClient.IndexService.AddWithRetryAsync(indexInfo);

                _logger.Info($"Has full text index data been create e-discovery job [{result.JobId}] under category [{CATEGORY_NAME}]. Succeed: [{result.Successful}]. Error code: [{result.ErrorCode}].");

                if (result.Successful)
                {
                    _runningJobIds.Add(result.JobId, 0);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger job for part data to e-discovery, category [{CATEGORY_NAME}]. Error: {e}");
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
                var delegationKey = StorageUtil.GetServiceClient(s_connectionString).GetUserDelegationKey(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(7)).Value;
                var accountName = s_connectionString.Substring(0, s_connectionString.IndexOf("."));
                token = sasBuilder.ToSasQueryParameters(delegationKey, accountName).ToString();
            }

            var blobClient = _containerClient.GetBlobClient(blobFolderPath);
            var tokenStr = $"{blobClient.Uri}?{token}";
            return tokenStr;
        }

        private void InitStorage()
        {
            var fileName = "delete_archived_data";

            var eDiscoveryFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "e_discovery");
            EnsureDirectory(eDiscoveryFolderPath);

            var jobFolderPath = SecurityUtils.SafeCombinePath(eDiscoveryFolderPath, _reportManager.JobType.ToString(), _reportManager.JobId);
            EnsureDirectory(jobFolderPath);

            // data folder
            _dataFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "data");
            EnsureDirectory(_dataFolderPath);

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, CATEGORY_NAME);
            EnsureDirectory(_dataFolderPath);

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, _operationName);
            EnsureDirectory(_dataFolderPath);

            _dataFilePath = SecurityUtils.SafeCombinePath(_dataFolderPath, $"{fileName}.txt");

            // zip folder
            _zipFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "zip");
            EnsureDirectory(_zipFolderPath);

            _zipFolderPath = SecurityUtils.SafeCombinePath(_zipFolderPath, _operationName);
            EnsureDirectory(_zipFolderPath);

            _blobFolderPath = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, CATEGORY_NAME, _operationName, _reportManager.JobType.ToString(), _reportManager.JobId);

            _containerClient = StorageUtil.GetContainerClient(s_connectionString, "e-discovery-container");
            _containerClient.CreateIfNotExists();
        }

        protected static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
