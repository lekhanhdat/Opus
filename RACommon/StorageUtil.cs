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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;

namespace AvePoint.RA.CommonUtil
{
    public class StorageUtil
    {
        private static readonly IRetryPolicy DefaultStorageRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(5), 3);

        public static string GetConnString(string accountName, string accessKey, int accountType)
        {
            ThrowUtil.ThrowIfNull(accountName, "accountName");
            ThrowUtil.ThrowIfNull(accessKey, "accessKey");
            string connString = null;
            if (accountType == 0)
            {
                connString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", accountName, accessKey);
            }
            else if (accountType == 1)
            {
                connString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.usgovcloudapi.net", accountName, accessKey);
            }
            else
            {
                throw new NotSupportedException("Unsupported account type");
            }
            return connString;
        }

        public static CloudBlobClient GetBlobClient(string accountName, string accessKey, int accountType)
        {
            ThrowUtil.ThrowIfNull(accountName, "accountName");
            ThrowUtil.ThrowIfNull(accessKey, "accessKey");
            string connString = GetConnString(accountName, accessKey, accountType);
            return GetBlobClient(connString);
        }

        public static CloudBlobClient GetBlobClient(string connString)
        {
            ThrowUtil.ThrowIfNull(connString, "connString");
            var storageAccount = CloudStorageAccount.Parse(connString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = DefaultStorageRetryPolicy;
            return blobClient;
        }

        public static string GetSasUri(string connString, string containerName)
        {
            ThrowUtil.ThrowIfNull(connString, "connString");
            ThrowUtil.ThrowIfNull(containerName, "containerName");
            var client = GetBlobClient(connString);
            var container = client.GetContainerReference(containerName);
            var adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read,
            };
            var sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            return container.Uri + sasContainerToken;
        }

        public static List<CloudBlockBlob> ListBlockBlobs(CloudBlobContainer container, string prefix = null)
        {
            var blobs = new List<CloudBlockBlob>();
            HandleBlobs(container, b => blobs.Add(b), prefix);
            return blobs;
        }

        public static void HandleBlobs(CloudBlobContainer container, Action<CloudBlockBlob> action, string prefix = null)
        {
            foreach (var item in container.ListBlobs(prefix))
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    var dir = (CloudBlobDirectory)item;
                    HandleBlobs(dir, action);
                }
                else if (item is CloudBlockBlob)
                {
                    var blob = (CloudBlockBlob)item;
                    action(blob);
                }
            }
        }

        private static void HandleBlobs(CloudBlobDirectory dir, Action<CloudBlockBlob> action)
        {
            foreach (IListBlobItem item in dir.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    var subDir = (CloudBlobDirectory)item;
                    HandleBlobs(subDir, action);
                }
                else if (item is CloudBlockBlob)
                {
                    var blob = (CloudBlockBlob)item;
                    action(blob);
                }
            }
        }
    }
}
