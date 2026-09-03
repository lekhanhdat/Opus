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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.Model;
using Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer
{
    public class RMDiscoveryFSAnalyzedDataManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSAnalyzedDataManager));

        private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private static readonly string STORAGE_CONTAINER_NAME = "fs-analyzed-file-container";

        private static readonly string ANALYZED_FILE_NAME = "{0}_data.txt";

        private static readonly string TENANT_ID = TenantLocalValue.LogonGroupId.ToLower();

        private readonly Guid _connectionId;

        private readonly string _folderPath;

        private readonly string _zipFilePath;

        private readonly string _extractedFilePath;

        private bool _hasData = true;

        private bool _hasError = false;

        public Guid ConnectionId => _connectionId;

        public RMDiscoveryFSAnalyzedDataManager(Guid connectionId)
        {
            _connectionId = connectionId;
            var processPath = Environment.CurrentDirectory;
            _folderPath = SecurityUtils.SafeCombinePath(processPath, "analyzed_data_zip_folder");
            _zipFilePath = SecurityUtils.SafeCombinePath(_folderPath, $"{connectionId}.zip");
            _extractedFilePath = SecurityUtils.SafeCombinePath(_folderPath, connectionId + "_data.txt");
        }

        public (bool hasError, bool hasData, RMDiscoveryFSAnalyzedConnectionDataInfo dataInfo) TryGetAnalyzedConnectionDataInfo()
        {
            if (!_hasData || _hasError)
            {
                return (_hasError, _hasData, new());
            }
            using var fileStream = File.Open(_extractedFilePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            var dataJson = reader.ReadLine();
            return (false, true, JsonConvert.DeserializeObject<RMDiscoveryFSAnalyzedConnectionDataInfo>(dataJson));
        }

        public IEnumerable<RMDiscoveryFSAnalyzedDataInfo> GetAnalyzedDataInfoes()
        {
            if (!_hasData || _hasError)
            {
                yield break;
            }

            using var fileStream = File.Open(_extractedFilePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            reader.ReadLine();
            var lineText = string.Empty;
            while (!string.IsNullOrWhiteSpace(lineText = reader.ReadLine()))
            {
                yield return JsonConvert.DeserializeObject<RMDiscoveryFSAnalyzedDataInfo>(lineText);
            }
        }

        public async Task Init()
        {
            try
            {
                PrepareDirectories();
                var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TENANT_ID, string.Format(ANALYZED_FILE_NAME, _connectionId)));
                using (var stream = File.OpenWrite(_extractedFilePath))
                {
                    blobClient.DownloadTo(stream);
                }
                //ZipFile.ExtractToDirectory(_zipFilePath, _folderPath);
            }
            catch (RequestFailedException ae) when (ae.Status == 404)
            {
                _logger.Warn($"The analyzed file of connection [{_connectionId}] no data found. Error: {ae}");
                _hasData = false;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while download analyzed file of connection[{_connectionId}] datas. Error: {e}");
                _hasData = false;
                _hasError = true;
                throw;
            }
        }

        private void PrepareDirectories()
        {
            if (File.Exists(_extractedFilePath))
            {
                File.Delete(_extractedFilePath);
            }
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            if (!File.Exists(_zipFilePath))
            {
                File.Create(_zipFilePath).Dispose();
            }
            else
            {
                File.Delete(_zipFilePath);
                File.Create(_zipFilePath).Dispose();
            }
        }
    }
}
