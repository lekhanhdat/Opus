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
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.GCommon.Utility.AzureBlobStorage
{
    public class AzureBlobStorage
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(AzureBlobStorage));

        private readonly BlobContainerClient _containerClient;
        public AzureBlobStorage(string connectionString, string containerName)
        {
            try
            {
                // var blobServiceClient = new BlobServiceClient(connectionString);
                var blobServiceClient = StorageUtil.GetServiceClient(connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                _containerClient.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                s_logger.Error(" Create Blob Container exception:" + ex.ToString());
                throw;
            }
        }

        public async Task<bool> UploadFileToAzureAsync(string blobName, Stream fileStream)
        {
            try
            {
                var blobClient = _containerClient.GetBlockBlobClient(blobName);

                if (blobClient != null)
                {
                    await blobClient.UploadAsync(fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                s_logger.Warn("An error occurred while uploading file {0} to Azure.Exception:{1}", blobName, ex.ToString());
                throw;
            }
        }

        public async Task<bool> CheckBlobExistAsync(string path)
        {
            var blobClient = _containerClient.GetBlobClient(path);
            return await blobClient.ExistsAsync();
        }
        public async Task<BlobItem> GetBlob(string path)
        {
            var blobs = _containerClient.GetBlobsAsync();
            BlobItem result= null;
            await foreach (var temp in blobs.AsPages())
            {
                var blob=temp.Values.Where(a=>a.Name.StartsWith(path)).ToList().FirstOrDefault();
                if (blob != null)
                {
                    result = blob;
                    break;
                }
            }
            return result;
        }
        public string CreateSASForBLOB(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlockBlobClient(blobName);
                var sasUri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(6));
                return sasUri.ToString();
            }
            catch (Exception e)
            {
                s_logger.Error($"blobName:{blobName},Create SAS for blob exception:" + e.ToString());
                return string.Empty;
            }
        }

    }
}
