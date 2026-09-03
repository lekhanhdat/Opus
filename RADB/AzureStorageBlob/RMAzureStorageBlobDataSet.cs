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
using AvePoint.RA.Contract.Tenant;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.DB.AzureStorageBlob
{
    public class RMAzureStorageBlobDataSet
    {
        public RMAzureStorageBlobContext Context { get; }

        public string ContainerName { get; }

        public bool EnableMultipleTenant { get; }

        public RMAzureStorageBlobDataSet(RMAzureStorageBlobContext context, string containerName)
            : this(context, containerName, false)
        { }

        public RMAzureStorageBlobDataSet(RMAzureStorageBlobContext context, string containerName, bool enableMultipleTenant)
        {
            Context = context;
            ContainerName = containerName;
            EnableMultipleTenant = enableMultipleTenant;
        }

        public async Task CreateContainerIfNotExists()
        {
            var containerClient = await GetContainerClient(true).ConfigureAwait(false);
            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        public async Task<bool> ContainerExists()
        {
            var containerClient = await GetContainerClient(false).ConfigureAwait(false);
            var response = await containerClient.ExistsAsync().ConfigureAwait(false);
            return response.Value;
        }

        public async Task UploadAsync(string blobName, Stream content, string contentType = null)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);
            var options = new BlobUploadOptions();
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                options.HttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            }

            var response = await blobClient.UploadAsync(content, options).ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureStorageBlobException(response.GetRawResponse().ReasonPhrase);
            }
        }

        public async Task<Stream> OpenWriteAsync(string blobName, bool overwrite = true, string contentType = null)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);

            var options = new BlobOpenWriteOptions
            {
                HttpHeaders = string.IsNullOrWhiteSpace(contentType) ? null : new BlobHttpHeaders { ContentType = contentType }
            };

            return await blobClient.OpenWriteAsync(overwrite, options).ConfigureAwait(false);
        }

        public async Task UploadAsync(string blobName, byte[] content, string contentType = null)
        {
            using var stream = new MemoryStream(content, writable: false);
            await UploadAsync(blobName, stream, contentType).ConfigureAwait(false);
        }

        public async Task<byte[]> DownloadAsync(string blobName)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);
            var response = await blobClient.DownloadContentAsync().ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureStorageBlobException(response.GetRawResponse().ReasonPhrase);
            }

            return response.Value.Content.ToArray();
        }

        public async Task<Stream> OpenReadAsync(string blobName)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);
            return await blobClient.OpenReadAsync().ConfigureAwait(false);
        }

        public async Task DownloadToFileAsync(string blobName, string localFilePath, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
            {
                throw new ArgumentNullException(nameof(localFilePath));
            }

            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);

            if (!overwrite && File.Exists(localFilePath))
            {
                throw new IOException($"File already exists at {localFilePath}");
            }

            var response = await blobClient.DownloadToAsync(localFilePath).ConfigureAwait(false);
            if (response.IsError)
            {
                throw new RMAzureStorageBlobException(response.ReasonPhrase);
            }
        }

        public async Task<bool> ExistsAsync(string blobName)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);
            var response = await blobClient.ExistsAsync().ConfigureAwait(false);
            return response.Value;
        }

        public async Task DeleteAsync(string blobName)
        {
            var blobClient = await GetBlobClient(blobName, true).ConfigureAwait(false);
            var response = await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
            if (!response.Value && response.GetRawResponse().IsError)
            {
                throw new RMAzureStorageBlobException(response.GetRawResponse().ReasonPhrase);
            }
        }

        public async Task<string> GetSasUriAsync(string blobName, BlobSasPermissions permissions, TimeSpan expiresOn)
        {
            _ = await GetBlobClient(blobName, true).ConfigureAwait(false);
            var containerName = GenerateContainerName();
            return StorageUtil.GenerateSasUri(Context.ConnectionString, containerName, blobName, expiresOn, permissions);
        }

        public async Task<byte[]> DownloadWithSasAsync(string sasUri)
        {
            if (string.IsNullOrWhiteSpace(sasUri))
            {
                throw new ArgumentNullException(nameof(sasUri));
            }

            var blobClient = new BlobClient(new Uri(sasUri));
            var response = await blobClient.DownloadContentAsync().ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureStorageBlobException(response.GetRawResponse().ReasonPhrase);
            }

            return response.Value.Content.ToArray();
        }

        public async Task<Stream> OpenReadWithSasAsync(string sasUri)
        {
            if (string.IsNullOrWhiteSpace(sasUri))
            {
                throw new ArgumentNullException(nameof(sasUri));
            }

            var blobClient = new BlobClient(new Uri(sasUri));
            return await blobClient.OpenReadAsync().ConfigureAwait(false);
        }

        public async Task DownloadToFileWithSasAsync(string sasUri, string localFilePath, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(sasUri))
            {
                throw new ArgumentNullException(nameof(sasUri));
            }

            if (string.IsNullOrWhiteSpace(localFilePath))
            {
                throw new ArgumentNullException(nameof(localFilePath));
            }

            if (!overwrite && File.Exists(localFilePath))
            {
                throw new IOException($"File already exists at {localFilePath}");
            }

            var blobClient = new BlobClient(new Uri(sasUri));
            var response = await blobClient.DownloadToAsync(localFilePath).ConfigureAwait(false);
            if (response.IsError)
            {
                throw new RMAzureStorageBlobException(response.ReasonPhrase);
            }
        }

        public async Task DeleteWithSasAsync(string sasUri)
        {
            if (string.IsNullOrWhiteSpace(sasUri))
            {
                throw new ArgumentNullException(nameof(sasUri));
            }

            var blobClient = new BlobClient(new Uri(sasUri));
            var response = await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);
            if (!response.Value && response.GetRawResponse().IsError)
            {
                throw new RMAzureStorageBlobException(response.GetRawResponse().ReasonPhrase);
            }
        }

        private async Task<BlobContainerClient> GetContainerClient(bool createIfNotExists)
        {
            var containerName = GenerateContainerName();

            return await Context.GetContainerClientAsync(containerName, createIfNotExists).ConfigureAwait(false);
        }

        private string GenerateContainerName()
        {
            if (EnableMultipleTenant)
            {
                return ContainerName + TenantLocalValue.LogonGroupId.Replace("-", string.Empty).ToLower();
            }

            return ContainerName;
        }

        private async Task<BlobClient> GetBlobClient(string blobName, bool createContainer)
        {
            var containerClient = await GetContainerClient(createContainer).ConfigureAwait(false);
            return containerClient.GetBlobClient(blobName);
        }
    }
}
