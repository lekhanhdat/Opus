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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Model;
using Azure;
using Azure.Storage.Blobs;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General
{
    public class RMDiscoveryOffice365AnalyzedDataManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365AnalyzedDataManager));

        private readonly Guid _o365TenantId;

        private readonly Guid _siteId;

        private readonly string _folderPath;

        private readonly string _zipFilePath;

        private readonly string _extractedFilePath;

        private readonly IEApiClient _ieApiClient;

        private bool _hasData = true;

        private bool _hasError = false;

        public Guid SiteId => _siteId;

        public RMDiscoveryOffice365AnalyzedDataManager(Guid o365TenantId, Guid siteId)
        {
            _o365TenantId = o365TenantId;
            _siteId = siteId;
            var processPath = Environment.CurrentDirectory;
            _folderPath = SecurityUtils.SafeCombinePath(processPath, "analyzed_data_zip_folder");
            _zipFilePath = SecurityUtils.SafeCombinePath(_folderPath, $"{siteId}.zip");
            _extractedFilePath = SecurityUtils.SafeCombinePath(_folderPath, siteId + "_data.txt");
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            Init();
        }

        public (bool hasError, bool hasData, RMDiscoveryOffice365AnalyzedSiteDataInfo dataInfo) TryGetAnalyzedSiteDataInfo()
        {
            if(!_hasData || _hasError)
            {
                return (_hasError, _hasData, new());
            }

            using var fileStream = File.Open(_extractedFilePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            var dataJson = reader.ReadLine();
            return (false, true, JsonConvert.DeserializeObject<RMDiscoveryOffice365AnalyzedSiteDataInfo>(dataJson));
        }

        public IEnumerable<RMDiscoveryOffice365AnalyzedDataInfo> GetAnalyzedDataInfoes()
        {
            if (!_hasData || _hasError)
            {
                yield break;
            }

            using var fileStream = File.Open(_extractedFilePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            reader.ReadLine();
            var lineText = string.Empty;
            while(!string.IsNullOrWhiteSpace(lineText = reader.ReadLine()))
            {
                yield return JsonConvert.DeserializeObject<RMDiscoveryOffice365AnalyzedDataInfo>(lineText);
            }
        }

        private void Init()
        {
            try
            {
                if(File.Exists(_extractedFilePath))
                {
                    File.Delete(_extractedFilePath);
                }

                var sasUri = _ieApiClient.SharePointSiteService.GetDataSasUri(_o365TenantId.ToString().ToLower(), _siteId.ToString().ToLower()).GetAwaiter().GetResult();
                var blobClient = new BlobClient(new Uri(sasUri));
                if (!Directory.Exists(_folderPath))
                {
                    Directory.CreateDirectory(_folderPath);
                }

                if (!File.Exists(_zipFilePath))
                {
                    File.Create(_zipFilePath).Dispose();
                }

                using (var stream = File.OpenWrite(_zipFilePath))
                {
                    blobClient.DownloadTo(stream);
                }

                ZipFile.ExtractToDirectory(_zipFilePath, _folderPath);
            }
            catch(RequestFailedException ae) when (ae.Status == 404)
            {
                _logger.Warn($"The o365 tenant [{_o365TenantId}] site [{_siteId}] no data found. Error: {ae}");
                _hasData = false;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while download o365 tenant [{_o365TenantId}] site [{_siteId}] datas. Error: {e}");
                _hasData = false;
                _hasError = true;
                throw;
            }
        }
    }
}
