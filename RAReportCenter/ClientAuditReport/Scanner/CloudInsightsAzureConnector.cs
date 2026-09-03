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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    internal class CloudInsightsAzureConnector : CloudInsightsConnectorBase
    {
        private static IRALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool SPAuditPacketStorage = false;
        private string AzureSASString = string.Empty;
        private DateTime mStartTime;
        private DateTime mEndTime;
        private BlobContainerClient mCloudBlobContainer = null;
        private List<BlobObject> blobs = new List<BlobObject>();
        List<string> nodes = new List<string>();


        internal BlobContainerClient CloudBlobContainer
        {
            get
            {
                if (mCloudBlobContainer == null)
                {
                    mCloudBlobContainer = BlobStorageDao.GetBlobContainer(AzureSASString);
                }
                return mCloudBlobContainer;
            }
            private set { }
        }

        public CloudInsightsAzureConnector(string jobid, string SASString, DateTime startTime, DateTime endTime) : base(jobid)
        {
            AzureSASString = SASString;
            this.mStartTime = startTime;
            this.mEndTime = endTime;
        }
        public override void SetAuditPacketStore(bool packetStore, List<string> siteUrls)
        {
            SPAuditPacketStorage = packetStore;
            nodes = siteUrls;
        }

        public override void Run()
        {
            var startTime = TruncateToMonthDay(mStartTime);
            var prefixs = new List<string>();
            if (SPAuditPacketStorage)
            {
                var letters = GetSCFirstTwoLetters(nodes);
                foreach (var scFolder in letters)
                {
                    prefixs.Add($"SPAudit/{scFolder}");
                }
                //need optimize later
                prefixs.Add("O365Audit/SharePoint");
            }
            else
            {
                prefixs.Add("O365Audit/SharePoint");
            }

            var allBlobs = new List<BlobItem>();
            foreach (var prefix in prefixs)
            {
                var currentBlobs = BlobStorageDao.ListBlockBlobs(CloudBlobContainer, prefix, this.mStartTime.Ticks, this.mEndTime.Ticks);
                allBlobs.AddRange(currentBlobs);
                mLog.Info($"get allBlobs count is {allBlobs.Count}");
            }
            if (SetRportCount != null)
            {
                SetRportCount(this, new SetCalculatedCountEventArgs() { Count = allBlobs.Count });
            }
            foreach (var blob in allBlobs)
            {
                try
                {
                    if (!blobs.Exists(s => s.Key.Equals(blob.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        blobs.Add(new BlobObject()
                        {
                            Key = blob.Name,
                            Blob = blob,
                            blobStatus = BlobStatus.Unprocessed
                        });
                    }
                    else if (blobs.Exists(s => s.Key.Equals(blob.Name, StringComparison.OrdinalIgnoreCase) && s.blobStatus == BlobStatus.Unprocessed))
                    {
                        var existBlob = blobs.FirstOrDefault(s => s.Key.Equals(blob.Name, StringComparison.OrdinalIgnoreCase) && s.blobStatus == BlobStatus.Unprocessed);
                        if (existBlob != null)
                        {
                            existBlob.Blob = blob;
                        }
                    }

                    mLog.Info("Process blob {0}", blob.Name);
                    string tempFolder = GetNewFolderInTemp();
                    var zipFileFullPath = Path.Combine(tempFolder, string.Format("SharePoint_{0}.zip", Guid.NewGuid().ToString()));
                    AveRetryPolicy.DefaultExponential.ExecuteAction(() =>
                    {
                        BlobStorageDao.DownloadBlob(CloudBlobContainer, blob, zipFileFullPath);
                    });
                    mLog.Info("Download blob successfully.");
                    var watchUnZip = new Stopwatch();
                    watchUnZip.Start();
                    ZipUtil.UnZipFile(zipFileFullPath, tempFolder);
                    watchUnZip.Stop();
                    var unZipTimeSpan = watchUnZip.Elapsed;
                    File.Delete(zipFileFullPath);
                    mLog.Info($"Unzip blob files spend time: {unZipTimeSpan.TotalSeconds} (s).");
                    AuditDownloadDataInfo auditFileInfo = new AuditDownloadDataInfo()
                    {
                        FileFolder = tempFolder,
                    };
                    Add2Queue(auditFileInfo);
                    if (IncreaseProgress != null)
                    {
                        IncreaseProgress(this, new IncreaseProgressEventArgs() { });
                    }
                }
                catch (Exception e)
                {
                    if (IncreaseProgress != null)
                    {
                        IncreaseProgress(this, new IncreaseProgressEventArgs() { HasError = true });
                    }
                    mLog.Error($"An error occurred while process the blob: {blob.Name}, Exception is: {e}");
                }
            }
        }

        public override Dictionary<string, string> GetUserMappings()
        {
            mLog.Info("Get the displayName mapping.");
            var spMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var blobs = BlobStorageDao.ListBlockBlobs(CloudBlobContainer, "O365Info/User");
            foreach (var blob in blobs)
            {
                mLog.Info($"Download audit User blob: {blob.Name}");
                var fileFullPath = Path.Combine(tmpFolder,
                    string.Format("SharePoint_{0}.txt", Guid.NewGuid().ToString()));
                BlobStorageDao.DownloadBlob(CloudBlobContainer, blob, fileFullPath);
                using (var reader = new TsvReader(fileFullPath))
                {
                    while (reader.Read())
                    {
                        if (!spMapping.ContainsKey(reader.GetString(1)) && !string.IsNullOrEmpty(reader.GetString(4)))
                        {
                            spMapping.Add(reader.GetString(1), reader.GetString(4));
                        }
                    }
                }
                File.Delete(fileFullPath);
            }
            mLog.Info($"Get the user displayName mapping success. {spMapping.Keys.Count}");
            return spMapping;
        }
    }

    public class BlobObject
    {
        public string Key;
        public BlobItem Blob;
        public BlobStatus blobStatus;
    }

    public enum BlobStatus
    {
        Processed = 0,
        Unprocessed = 1
    }
}
