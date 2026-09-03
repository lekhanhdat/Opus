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
using AvePoint.GCommon;
using AvePoint.RA.Common.Retrying;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.Vml.Office;
using PnP.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.Extentions
{
    public static class RMFullTextIndexStorageClientExtension
    {
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static async Task UploadBlobWithRetryAsync(this BlobContainerClient client, string localFilePath, string storagePath)
        {
            var retryer = RMRetryerBuilder.CreateBuilder().WithStopStrategy(new RMRetryStopAfterAttemptStrategy(5)).Build();
            await retryer.RetryAsync(async () =>
            {
                var blobClient = client.GetBlobClient(storagePath);
                using var fileStream = File.OpenRead(localFilePath);
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                log.Info($"Begin UploadBlobAsync.localFilePath:{localFilePath}.");
                await blobClient.UploadAsync(fileStream, true, cts.Token);
                log.Info($"Finished UploadBlobAsync.localFilePath:{localFilePath}.");
            });
        }
    }
}
