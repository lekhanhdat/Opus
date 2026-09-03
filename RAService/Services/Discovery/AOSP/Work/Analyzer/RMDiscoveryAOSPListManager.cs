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
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using Azure.Storage.Blobs;
using Azure;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer
{
    public class RMDiscoveryAOSPListManager
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPListManager));

        private readonly Guid _o365TenantId;

        private readonly Guid _siteId;

        private readonly string _folderPath;

        private readonly string _zipFilePath;

        private readonly string _extractedFilePath;

        private readonly IEApiClient _ieApiClient;

        public RMDiscoveryAOSPListManager(Guid o365TenantId, Guid siteId)
        {
            _o365TenantId = o365TenantId;
            _siteId = siteId;
            var processPath = Environment.CurrentDirectory;
            _folderPath = Path.Combine(processPath, "list_zip_folder");
            _zipFilePath = Path.Combine(_folderPath, $"{siteId}.zip");
            _extractedFilePath = Path.Combine(_folderPath, siteId + ".txt");
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
        }

        public async Task<List<Guid>> GetListsAsync()
        {
            try
            {
                var res = new List<Guid>();

                if (!File.Exists(_extractedFilePath))
                {
                    var sasUri = await _ieApiClient.SharePointSiteService.GetListSasUri(_o365TenantId.ToString().ToLower(), _siteId.ToString().ToLower());
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
                        await blobClient.DownloadToAsync(stream);
                    }

                    ZipFile.ExtractToDirectory(_zipFilePath, _folderPath);
                }

                using var fileStream = File.Open(_extractedFilePath, FileMode.Open);
                using var reader = new StreamReader(fileStream);
                var lineText = string.Empty;
                while (!string.IsNullOrWhiteSpace(lineText = reader.ReadLine()))
                {
                    res.Add(Guid.Parse(lineText));
                }

                return res;
            }
            catch (RequestFailedException ae) when (ae.Status == 404)
            {
                _logger.Warn($"The o365 tenant [{_o365TenantId}] site [{_siteId}] no list found. Error: {ae}");
                return [];
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get o365 tenant [{_o365TenantId}] site [{_siteId}] lists. Error: {e}");
                throw;
            }
        }
    }
}
