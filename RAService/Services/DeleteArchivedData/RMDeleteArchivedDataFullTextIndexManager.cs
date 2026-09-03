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
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.Extentions;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{

    public class RMDeleteArchivedDataFullTextIndexManager
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataFullTextIndexManager));

        private static readonly IRestoreSearchService s_restoreSearchService = PlatformWindsorManager.GetService<IRestoreSearchService>();

        private static readonly bool s_enableFullTextIndex;

        private readonly Dictionary<string, RMDeleteArchivedDataFullTextIndexRunner> _runners = new();

        private readonly HashSet<string> _noCategoryJobIds = new();

        private readonly RMArchivedFullTextIndexCategoryManagement _categoryManagementService = new();
        

        static RMDeleteArchivedDataFullTextIndexManager()
        {
            s_enableFullTextIndex = s_restoreSearchService.IsEnableFullTextIndexSearch();
        }

        public RMDeleteArchivedDataFullTextIndexManager()
        {
            if (!s_enableFullTextIndex)
            {
                _logger.Warn($"Current tenant did not enable full text index feature.");
                return;
            }
        }

        public async Task<bool> DeleteAsync(ArchiverBasicIndex item)
        {
            try 
            {
                if (!s_enableFullTextIndex)
                {
                    return true;
                }

                var archiverJobId = item.JobId;

                if(_noCategoryJobIds.Contains(archiverJobId))
                {
                    _logger.Info($"The archiver job [{archiverJobId}] no category info found. Skip it [{item.Id}]");
                    return true;
                }

                if (!_runners.TryGetValue(archiverJobId, out var runner))
                {
                    runner = new RMDeleteArchivedDataFullTextIndexRunner();
                    _runners[archiverJobId] = runner;
                }

                return await runner.DeleteAsync(item);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while delete item [{item.Id}]. Error: {e}");
                return false;
            }
        }

        public async Task WaitAsync()
        {
            foreach(var runner in _runners.Values)
            {
                await runner.WaitAsync();
                await _categoryManagementService.SyncCategoryDataSizeAsync();
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

        private readonly HashSet<string> _runningJobIds = [];

        private readonly EDiscoveryApiClient _apiClient;

        private BlobContainerClient _containerClient;

        private string _zipFolderPath;

        private string _blobFolderPath;

        private string _dataFolderPath;

        private string _dataFilePath;

        private long _appendedDataSize = 0;

        private long _appendedDataCount = 0;

        public RMDeleteArchivedDataFullTextIndexRunner()
        {
            _apiClient = AosApiUtility.GetEDiscoveryApiClient();
            InitStorage();
        }

        public async Task<bool> DeleteAsync(ArchiverBasicIndex item)
        {
            try
            {
                var queryGroup = BuildDeleteQueryGroup(item);
                var queryGroupJson = JsonConvert.SerializeObject(queryGroup);
                if (_appendedDataCount == 0 && _appendedDataSize == 0)
                {
                    File.Create(_dataFilePath).Dispose();
                }

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

                _appendedDataCount = 0;
                _appendedDataSize = 0;

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

                foreach (var jobId in _runningJobIds.ToList())
                {
                    try
                    {
                        var eDiscoveryJob = await _apiClient.IndexService.GetJobWithRetryAsync(jobId);
                        if (eDiscoveryJob.State == Cloud.Sdk.Data.EDiscovery.JobState.Running || eDiscoveryJob.State == Cloud.Sdk.Data.EDiscovery.JobState.None || eDiscoveryJob.State == Cloud.Sdk.Data.EDiscovery.JobState.Waiting)
                        {
                            continue;
                        }
                        
                        _logger.Info($"The e-discovery job [{jobId}] is [{eDiscoveryJob.State}].");
                        _runningJobIds.Remove(jobId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while check e-discovery job [{jobId}] state. Error: {e}");
                        _runningJobIds.Remove(jobId);
                    }
                }

                if (_runningJobIds.Count > 0)
                {
                    await Task.Delay(20 * 1000);
                }
            }

            _logger.Info($"Finished check full text index all e-discovery jobs.");
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
                                Name = "pathMd5",
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

        private async Task UploadDataAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                return;
            }
            var zipName = $"data.zip";
            var zipPath = SecurityUtils.SafeCombinePath(_zipFolderPath, zipName);
            ZipFile.CreateFromDirectory(_dataFolderPath, zipPath);
            _logger.Info($"The full text index data has been zip.");

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
                    _runningJobIds.Add(result.JobId);
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
            var eDiscoveryFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "e_discovery");
            if (!System.IO.Directory.Exists(eDiscoveryFolderPath))
            {
                System.IO.Directory.CreateDirectory(eDiscoveryFolderPath);
            }

            var jobFolderPath = SecurityUtils.SafeCombinePath(eDiscoveryFolderPath, "delete_archived_data");
            if (!System.IO.Directory.Exists(jobFolderPath))
            {
                System.IO.Directory.CreateDirectory(jobFolderPath);
            }

            _dataFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "data");
            if (!System.IO.Directory.Exists(_dataFolderPath))
            {
                System.IO.Directory.CreateDirectory(_dataFolderPath);
            }

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, CATEGORY_NAME);
            if (!System.IO.Directory.Exists(_dataFolderPath))
            {
                System.IO.Directory.CreateDirectory(_dataFolderPath);
            }

            _dataFolderPath = SecurityUtils.SafeCombinePath(_dataFolderPath, "DeleteItem");
            if (!System.IO.Directory.Exists(_dataFolderPath))
            {
                System.IO.Directory.CreateDirectory(_dataFolderPath);
            }

            _dataFilePath = SecurityUtils.SafeCombinePath(_dataFolderPath, $"delete_archived_data.txt");

            _zipFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "zip");
            if (!System.IO.Directory.Exists(_zipFolderPath))
            {
                System.IO.Directory.CreateDirectory(_zipFolderPath);
            }

            if (!System.IO.Directory.Exists(_zipFolderPath))
            {
                System.IO.Directory.CreateDirectory(_zipFolderPath);
            }

            _zipFolderPath = SecurityUtils.SafeCombinePath(_zipFolderPath, "DeleteItem");
            if (!System.IO.Directory.Exists(_zipFolderPath))
            {
                System.IO.Directory.CreateDirectory(_zipFolderPath);
            }

            _blobFolderPath = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, CATEGORY_NAME, "DeleteItem", "delete_archived_data");

            _containerClient = StorageUtil.GetContainerClient(s_connectionString, "e-discovery-container");
            _containerClient.CreateIfNotExists();
        }
    }
}
