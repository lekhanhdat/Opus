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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.DB.AzureStorageBlob;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.DataStorage
{
    public abstract class RMDataIngestionAzureStorageBlobHandler
    {
        private static readonly BlobSasPermissions DEFAULT_BLOB_SAS_PERMISSIONS = BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Add;

        private const int DEFAULT_SAS_URI_EXPIRATION_HOURS = 24;

        private const string DEFAULT_CONTENT_TYPE = "application/octet-stream";

        private const string DEFAULT_BLOB_FILE_EXTENSION = ".bin";

        private readonly RMAzureStorageBlobDataSet _dataSet;
        
        private readonly RALogger _logger;

        public abstract RMDataIngestionType IngestionType { get; }

        protected RMDataIngestionAzureStorageBlobHandler(RMAzureStorageBlobDataSet dataSet, Type loggerType)
        {
            _dataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            _logger = RALogger.GetInstance(loggerType ?? typeof(RMDataIngestionAzureStorageBlobHandler));
        }

        public Task<Stream> OpenWriteAsync(string blobName)
        {
            return ExecuteAsync(() => _dataSet.OpenWriteAsync(blobName, true, DEFAULT_CONTENT_TYPE), "Open blob for write failed");
        }

        public Task<Stream> OpenReadAsync(string blobName)
        {
            return ExecuteAsync(() => _dataSet.OpenReadAsync(blobName), "Open blob for read failed");
        }

        public Task DeleteAsync(string blobName)
        {
            return ExecuteAsync(() => _dataSet.DeleteAsync(blobName), "Delete blob failed");
        }

        public Task<string> GetSasUriAsync(string blobName)
        {
            return ExecuteAsync(() => _dataSet.GetSasUriAsync(blobName,
                DEFAULT_BLOB_SAS_PERMISSIONS,
                TimeSpan.FromHours(DEFAULT_SAS_URI_EXPIRATION_HOURS)), "Get SAS URI failed");
        }

        public async Task<RMDataIngestionBlobReference> GenerateBlobReferenceAsync(RMDataIngestionBlobNamingContext blobNamingContext)
        {
            if (blobNamingContext == null)
            {
                throw new ArgumentNullException(nameof(blobNamingContext));
            }
            var binFileName = $"data{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}_{Guid.NewGuid().ToString("N")}{DEFAULT_BLOB_FILE_EXTENSION}";
            var blobName = $"{blobNamingContext.IngestionType}/{blobNamingContext.OperationType}/{blobNamingContext.UniqueId.ToLower()}/{blobNamingContext.BlobType}/{binFileName}";
            var sasUri = await _dataSet.GetSasUriAsync(
                blobName,
                DEFAULT_BLOB_SAS_PERMISSIONS,
                TimeSpan.FromHours(DEFAULT_SAS_URI_EXPIRATION_HOURS));
            _logger.Info("Generated blob reference. BlobFileName: {0}, SAS URI Expiration: {1} hours", binFileName, DEFAULT_SAS_URI_EXPIRATION_HOURS);
            return new RMDataIngestionBlobReference
            {
                BlobName = blobName,
                SasUri = sasUri
            };
        }

        private async Task ExecuteAsync(Func<Task> action, string operation)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("{0}. Error: {1}", operation, ex);
                throw;
            }
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, string operation)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("{0}. Error: {1}", operation, ex);
                throw;
            }
        }
    }
}
