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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.RMExplorer;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    public class BlobStorageDao
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static BlobContainerClient GetBlobContainer(string containerSasUri)
        {
            ThrowUtil.ThrowIfNull(containerSasUri, "containerSasUri");
            return new BlobContainerClient(new Uri(containerSasUri));
        }

        private static List<BlobHierarchyItem> GetAllListBlockBlobs(BlobContainerClient container, string blobPrefix)
        {
            var blobs = new List<BlobHierarchyItem>();
            string token = null;
            do
            {
                var resultSegment = container.GetBlobsByHierarchy(default, default, delimiter: "/", prefix: blobPrefix).AsPages(token, 100);
                foreach(var page in resultSegment)
                {
                    blobs.AddRange(page.Values);
                    token = page.ContinuationToken;
                }
            } while (!string.IsNullOrWhiteSpace(token));
            return blobs;
        }

        public static List<BlobItem> ListBlockBlobs(BlobContainerClient container, string blobPrefix, long startTime, long endTime)
        {
            var blobs = new List<BlobItem>();
            try
            {
                foreach (var item in GetAllListBlockBlobs(container, blobPrefix))
                {
                    if(item.IsPrefix)
                    {
                        List(container, item, blobs, blobPrefix, startTime, endTime);
                    }
                    else if(item.IsBlob)
                    {
                        blobs.Add(item.Blob);
                    }
                }
                return blobs;
            }
            catch (Exception ex)
            {
                logger.Error("an error occurred. {0}", ex);
                throw;
            }
        }

        private static void List(BlobContainerClient container, BlobHierarchyItem dir, List<BlobItem> blobs, string blobPrefix, long startTime, long endTime)
        {
            foreach (var item in GetAllListBlockBlobs(container, dir.Prefix))
            {
                if(item.IsPrefix)
                {
                    var newStr = item.Prefix.Replace(blobPrefix, "").Trim('/');
                    if (newStr.Length >= 8)
                    {
                        var timeStr = newStr.Split('/');
                        var year = Convert.ToInt32(timeStr[0]);
                        var month = Convert.ToInt32(timeStr[1]);
                        var day = Convert.ToInt32(timeStr[2]);
                        var blobTime = Convert.ToDateTime(string.Format("{0}-{1}-{2}", year, month, day));
                        if (blobTime.Ticks >= startTime && blobTime.Ticks <= endTime)
                        {
                            List(container, item, blobs, blobPrefix, startTime, endTime);
                        }
                    }
                    else
                    {
                        List(container, item, blobs, blobPrefix, startTime, endTime);
                    }
                }
                else if(item.IsBlob)
                {
                    var blob = item.Blob;
                    blobs.Add(blob);
                }
            }
        }

        public static List<BlobItem> ListBlockBlobs(BlobContainerClient container, string blobPrefix)
        {
            var blobs = new List<BlobItem>();
            try
            {
                string token = null;
                do
                {
                    var resultSegment = container.GetBlobs(default, default, prefix: blobPrefix, default).AsPages(token, 100);
                    foreach (var page in resultSegment)
                    {
                        blobs.AddRange(page.Values);
                        token = page.ContinuationToken;
                    }
                } while (!string.IsNullOrWhiteSpace(token));
                return blobs;
            }
            catch (Exception ex)
            {
                logger.Info("an error occurred when get blobs: {0}",ex);
                throw;
            }
        }

        public static void DownloadBlob(BlobContainerClient container, BlobItem blob, string downloadPath)
        {
            var blobClient = container.GetBlobClient(blob.Name);
            blobClient.DownloadTo(downloadPath);
        }
    }
}
