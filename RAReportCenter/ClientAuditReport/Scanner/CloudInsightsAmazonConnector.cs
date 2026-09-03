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
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Cloud.Sdk.Data.CloudInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport.Scanner
{
    internal class CloudInsightsAmazonConnector : CloudInsightsConnectorBase
    {
        private static IRALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool SPAuditPacketStorage = false;
        private AmazonS3Dto mAmazonS3Dto = null;
        private AWSStorageUtil mAWSStorageUtil = null;
        private DateTime mStartTime;
        private DateTime mEndTime;
        List<string> nodes = new List<string>();
        public CloudInsightsAmazonConnector(string jobid, AmazonS3Model amazonS3Dto, DateTime startTime, DateTime endTime) : base(jobid)
        {
            mAmazonS3Dto = new AmazonS3Dto(amazonS3Dto);
            this.mStartTime = startTime;
            this.mEndTime = endTime;
            mAWSStorageUtil = new AWSStorageUtil(mAmazonS3Dto);
        }

        public override Dictionary<string, string> GetUserMappings()
        {
            mLog.Info("Get the displayName mapping.");
            var spMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var blobs = mAWSStorageUtil.ListAllObjectKeys("O365Info/User");
            foreach (var blob in blobs)
            {
                mLog.Info($"Download audit User blob: {blob}");
                var fileFullPath = Path.Combine(tmpFolder, string.Format("Mgt_{0}.txt", Guid.NewGuid().ToString()));
                mAWSStorageUtil.DownloadAmazonS3Object(blob, fileFullPath);
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

        public override void SetAuditPacketStore(bool packetStore, List<string> siteUrls)
        {
            SPAuditPacketStorage = packetStore;
            nodes = siteUrls;
        }

        public override void Run()
        {
            var startTime = TruncateToMonthDay(mStartTime);
            var amazonS3Keys = new List<string>();
            if (SPAuditPacketStorage)
            {
                var letters = GetSCFirstTwoLetters(nodes);
                foreach (var scFolder in letters)
                {
                    var tempKeys = new List<string>();
                    mAWSStorageUtil.ListObjectKeysByTime($"SPAudit/{scFolder}/", mStartTime, mEndTime, tempKeys);
                    mLog.Info($"Site collection folder: {scFolder}, blob number: {tempKeys.Count}");
                    amazonS3Keys.AddRange(tempKeys);
                }
            }
            mLog.Info("First amazonS3 blobs count {0}", amazonS3Keys.Count);

            var tempS3keys = new List<string>();
            mAWSStorageUtil.ListObjectKeysByTime("O365Audit/SharePoint/", mStartTime, mEndTime, tempS3keys);
            mLog.Info("Second blobs count {0}", tempS3keys.Count);
            amazonS3Keys.AddRange(tempS3keys);

            foreach (var key in amazonS3Keys)
            {
                try
                {
                    mLog.Info("Process AmazonS3 blob {0}", key);
                    string tempFolder = GetNewFolderInTemp();
                    var zipFileFullPath = Path.Combine(tempFolder, string.Format("SharePoint_{0}.zip", Guid.NewGuid().ToString()));
                    AveRetryPolicy.DefaultExponential.ExecuteAction(() =>
                    {
                        mAWSStorageUtil.DownloadAmazonS3Object(key, zipFileFullPath);
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
                catch (Exception ex)
                {
                    if (IncreaseProgress != null)
                    {
                        IncreaseProgress(this, new IncreaseProgressEventArgs() { HasError = true }); ;
                    }
                    mLog.Error($"An error occurred while process the blob: {key}, Exception is: {ex}");
                }
            }
        }
    }

    public class AmazonS3Dto
    {
        public string AccountKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public int Region { get; set; }

        public AmazonS3Dto() { }

        public AmazonS3Dto(AmazonS3Model model)
        {
            AccountKey = model.AccountKey;
            SecretKey = model.SecretKey;
            BucketName = model.BucketName;
            Region = model.Region;
        }

        public bool HasPropertyIsNull()
        {
            return string.IsNullOrEmpty(this.AccountKey) || string.IsNullOrEmpty(this.SecretKey) || string.IsNullOrEmpty(this.BucketName);
        }

        public override string ToString()
        {
            string builder = $"AccountKey: {AccountKey}, SecretKey: {SecretKey}, BucketName: {BucketName}, Region: {Region}";
            return $"connString" + Convert.ToBase64String(Encoding.UTF8.GetBytes(builder));
        }
    }

    public class AWSStorageUtil
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static AmazonS3Client s3Client;
        private static string bucketName;

        public AWSStorageUtil(AmazonS3Dto mAmazonS3Dto)
        {
            s3Client = new AmazonS3Client(mAmazonS3Dto.AccountKey, mAmazonS3Dto.SecretKey, ConvertRegion((AmazonS3Region)mAmazonS3Dto.Region));
            bucketName = mAmazonS3Dto.BucketName;
        }

        public void ListObjectKeysByTime(string prefix, DateTime start, DateTime end, List<string> result)
        {
            DateTime tempTime = new DateTime(start.Ticks, DateTimeKind.Utc);
            while (tempTime <= end.AddDays(1))
            {
                var response = ListObjectKeys(string.Format("{0}{1}", prefix, tempTime.ToString("yyyy/M/d")));
                foreach (var key in response)
                {
                    logger.Info($"Need download object name: {key}");
                    result.Add(key);
                }
                tempTime = tempTime.AddDays(1);
            }
        }

        public List<string> ListAllObjectKeys(string prefix)
        {
            var result = new List<string>();
            logger.Info($"List objectkeys by prefix: {prefix}");
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            ListObjectsV2Response response;
            do
            {
                response = s3Client.ListObjectsV2Async(request).Result;
                foreach (S3Object s3object in response.S3Objects)
                {
                    if (!s3object.Key.EndsWith("/"))
                    {
                        result.Add(s3object.Key);
                    }
                }
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated == true);
            logger.Info($"All blobs: {result.Count}");
            return result;
        }

        private List<string> ListObjectKeys(string prefix = null)
        {
            logger.Info($"List objectkeys by prefix: {prefix}");
            var result = new List<string>();
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            ListObjectsV2Response response;
            do
            {
                response = s3Client.ListObjectsV2Async(request).Result;
                result.AddRange(response.S3Objects.Select(o => o.Key).Where(o => o.EndsWith(".zip")));
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated == true);
            return result;
        }

        public void DownloadAmazonS3Object(string objectKey, string destinationFilePath, bool append = false)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
            };
            using (GetObjectResponse response = s3Client.GetObjectAsync(request).Result)
            {
                response.WriteResponseStreamToFileAsync(destinationFilePath, append, CancellationToken.None).Wait();
            }
        }

        private RegionEndpoint ConvertRegion(AmazonS3Region region)
        {
            var fieldInfo = typeof(RegionEndpoint).GetField(region.ToString());
            if (fieldInfo != null)
            {
                return (RegionEndpoint)fieldInfo.GetValue(null);
            }
            else
            {
                throw new NotSupportedException("Invaild region");
            }
        }
    }

    internal enum AmazonS3Region
    {
        AFSouth1 = 0,
        USIsobEast1 = 1,
        USIsoWest1 = 2,
        USIsoEast1 = 3,
        USGovCloudWest1 = 4,
        USGovCloudEast1 = 5,
        CNNorthWest1 = 6,
        CNNorth1 = 7,
        USWest1 = 8,
        USEast2 = 9,
        USEast1 = 10,
        SAEast1 = 11,
        MESouth1 = 12,
        EUWest3 = 13,
        USWest2 = 14,
        EUWest1 = 15,
        EUWest2 = 16,
        APNortheast1 = 17,
        APNortheast2 = 18,
        APNortheast3 = 19,
        APSouth1 = 20,
        APSoutheast1 = 21,
        APEast1 = 22,
        CACentral1 = 23,
        EUCentral1 = 24,
        EUNorth1 = 25,
        EUSouth1 = 26,
        APSoutheast2 = 27
    }
}
