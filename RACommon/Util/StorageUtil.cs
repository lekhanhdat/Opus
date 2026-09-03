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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using OpenAI.Containers;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Util.MSAzure;

namespace AvePoint.RA.CommonUtil
{
    public class RAStorageUtil
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static Dictionary<string, string> _storageRegionMapping = new()
        {
            { "usstandard", "MediaStorage_Amazon_US_Standard" },
            { "uswest", "MediaStorage_Amazon_US_West_Northern_California" },
            { "eu", "MediaStorage_Amazon_EU_Ireland" },
            { "london", "MediaStorage_Amazon_EU_London" },
            { "apac", "MediaStorage_Amazon_Asia_Pacific_Singapore" },
            { "tokyo", "MediaStorage_Amazon_Asia_Pacific_Tokyo" },
            { "sydney", "MediaStorage_Amazon_Asia_Pacific_Sydney" },
            { "oregon", "MediaStorage_Amazon_US_West_Oregon" },
            { "saopaulo", "MediaStorage_Amazon_South_America_Saopaulo" },
            { "ohio", "MediaStorage_Amazon_US_Ohio" },
            { "canadacentral", "MediaStorage_Amazon_Canada_Central" },
            { "frankfurt", "MediaStorage_Amazon_EU_Frankfurt" },
            { "seoul", "MediaStorage_Amazon_Asia_Seoul" },
            { "mumbai", "MediaStorage_Amazon_Asia_Mumbai" },
        };

        public const string SKP = "***********";

        public static string GetI18NRegion(string region)
        {
            if (_storageRegionMapping.TryGetValue(region, out var regioni18nStr))
            {
                return regioni18nStr;
            }

            return region;
        }

        public static string GetStorageConfigValue(StorageDeviceDto storage ,string key)
        {
            if (storage.mCurrentXRI.Params.TryGetValue(key, out var value))
            {
                return value;
            }
            logger.Warn("This key does not exist in currentXRI Params list. {0}", key);
            return string.Empty;
        }

        private static string GetConnString(string accountName, string accessKey, int accountType)
        {
            //Maybe useless code
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

        public static void CreateContainerIfNotExists(string accountName, string accessKey, int accountType, string containerName)
        {
            ThrowUtil.ThrowIfNull(accountName, "accountName");
            ThrowUtil.ThrowIfNull(accessKey, "accessKey");
            string connString = GetConnString(accountName, accessKey, accountType);
            AzureUtil.GetBlobContainerClient(connString, containerName, true);
        }

        public static void UploadBlob(string storageConString, string containerName, string blobName, object obj)
        {
            var content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            AzureUtil.UploadTextToBlobContainer(storageConString, containerName, blobName, content);
        }
        public static void UploadBlobByXRIString(string storageXRIString, string blobName, object obj)
        {
            var content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            AzureUtil.UploadTextToBlobContainerByXRIString(storageXRIString, blobName, content);
        }
        /// <summary>
        /// will get the blob and delete it from storage.
        /// </summary>
        /// <param name="storageConString"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public static string GetBlobAsString(string storageConString, string containerName, string blobName)
        {
            return AzureUtil.DownloadBlobToText(storageConString, containerName, blobName);
            //var result = string.Empty;
            //var client = GetBlobClient(storageConString);
            //var container = client.GetContainerReference(containerName);
            //var blob = container.GetBlockBlobReference(blobName);
            //if (blob.Exists())
            //{
            //    result = blob.DownloadText();
            //    blob.Delete();
            //}

            //return result;
        }

        public static void AppendReport(HBReportFileInfo hBReportInfo)
        {
            JobType jobType = (JobType)hBReportInfo.JobType;
            string moduleName = jobType.ToString();
            //一下两个job 要和job role中跑job的路径一致 jobreportutility.AssembleReportPathAfterHalf
            if (jobType == JobType.FSItemsFilesDueDisposal)
            {
                moduleName = "Content Due for Disposal Report";
            }
            else if (jobType == JobType.FSCreateAndDestroyedFileReport || jobType == JobType.BoxCreateAndDestroyedFileReport || jobType == JobType.GoogleCreateAndDestroyedFileReport)
            {
                moduleName = "Content Due for Time Frame Report";
            }
            else if (jobType == JobType.FSRetainSimulate)
            {
                moduleName = "ArchiverRetentionSimulate";
            }
            var tenantFolderName = $"{TenantLocalValue.LogonGroupId}/{moduleName}/";
            var blobName = new StringBuilder();
            blobName.Append(tenantFolderName);
            blobName.Append(Path.GetFileName(hBReportInfo.FileName));
            logger.Info($"start to upload filePath: {blobName} 2 {ReportContainer}");
            CreateReportContainerIfNotExists();
            AppendReportBlob(blobName.ToString(), hBReportInfo.File);
            logger.Info($"finish to append file name:{hBReportInfo.FileName}");
        }

        public static void DownloadReport(BaseJobDto baseJobDto)
        {
            logger.Info($"begin to download report:{baseJobDto.Id}");
            var uri = JobReportUtility.GetJobReportUri(baseJobDto.Id, baseJobDto.JobType, ".rpt");
            string tempPath = JobReportUtility.GetJobReportTempPath(baseJobDto, ".rpt");
            CreateTempFile(tempPath);
            DownloadReportBlobToFile(uri, tempPath);
            logger.Info($"success to download report:{baseJobDto.Id}");
        }

        public static void DownloadChangeLogReport(string blobName, string tempPath)
        {
            logger.Info($"begin to download report");
            CreateTempFile(tempPath);
            DownloadChangeLogBlobToFile(blobName, tempPath);
            logger.Info($"success to download report");
        }

        public static void DownloadRestoreScDetail(string scUrl)
        {
            logger.Info($"begin to download report:{scUrl}");
            var uri = JobReportUtility.GetRestoreReportJobScDetailUri(scUrl);
            string tempPath = JobReportUtility.GetRestoreReportJobScDetailPath(scUrl);
            CreateTempFile(tempPath);
            DownloadReportBlobToFile(uri, tempPath);
            logger.Info($"success to download report:{scUrl}");
        }
        public static void DownloadRestoreGDDetail(string driveName)
        {
            logger.Info($"begin to download report:{driveName}");
            var uri = JobReportUtility.GetRestoreReportJobGDDetailUri(driveName);
            string tempPath = JobReportUtility.GetRestoreReportJobGDDetailPath(driveName);
            CreateTempFile(tempPath);
            DownloadReportBlobToFile(uri, tempPath);
            logger.Info($"success to download report:{driveName}");
        }

        public static void DownloadReport4ArchiverJob(BaseJobDto baseJobDto)
        {
            logger.Info($"begin to download report:{baseJobDto.Id}");
            var uri = JobReportUtility.GetArchiverJobReportUri(baseJobDto.PlanId, baseJobDto.Id, baseJobDto.Category.Value, ".rpt");
            uri = Path.Combine(DaoJobReportContainer, FormatName(baseJobDto.TenantGroupEmail), TenantLocalValue.LogonGroupId, uri);
            string tempPath = JobReportUtility.GetArchiverJobReportPath(baseJobDto, ".rpt");
            CreateTempFile(tempPath);
            AzureUtil.DownloadBlobToFile(DaoJobStorageConnectionString, DaoJobReportContainer, uri, tempPath);
            RAWebLocalCacheReleaser.RecordCacheFile(tempPath);
            logger.Info($"success to download report:{baseJobDto.Id}");
        }

        private static string FormatName(string name)
        {
            return Regex.Replace(name, @"[^a-zA-Z0-9]", "-");
        }

        private static void CreateTempFile(string tempPath)
        {
            FileInfo fileInfo = new FileInfo(tempPath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
                logger.Debug("Create temp Directory,Directory name:", fileInfo.Directory.Name);
            }
            if (!fileInfo.Exists)
            {
                fileInfo.Create().Close();
                logger.Debug("Create temp report file:", fileInfo.Name);
            }
        }

        public static string DownloadFileMessageFromStorageBySasTokenUrl(string sasTokenUrl, string storageLowName)
        {
            try
            {
                var client = new BlobClient(new Uri(sasTokenUrl), null);
                var response = client.DownloadContent();
                return response.Value.Content.ToString();
            }
            catch (Exception ex)
            {
                logger.Error("download file message from azure stroage failed.", ex.ToString());
                throw;
            }
        }

        public static string DownloadFileMessageFromStorageByXri(string storageXri, string storageLowName)
        {
            try
            {
                var blob = GetBlockBlobByStorageXRI(storageXri, storageLowName);
                return blob.DownloadContent().Value.Content.ToString();
            }
            catch (Exception ex)
            {
                logger.Error("download file message from azure stroage failed.", ex.ToString());
                throw;
            }
        }
        public static LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceDto storageDevice)
        {
            if (storageDevice == null) { return null; }
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
            };

            var logical = new LogicalDeviceDto();
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }
        public static BlobContainerClient GetBlobContainerClientByStorageXRI(string xri)
        {
            ConnectionBuilder xriObj = ConnectionBuilder.ValueOf(xri);
            string accessPoint = string.Empty;
            string containerName = string.Empty;
            string accountName = string.Empty;
            string accountKey = string.Empty;

            if (xriObj.Params.ContainsKey("accesspoint"))
            {
                accessPoint = xriObj.Params["accesspoint"];
            }
            if (xriObj.Params.ContainsKey("containername"))
            {
                containerName = xriObj.Params["containername"];
            }
            if (xriObj.Params.ContainsKey("name"))
            {
                accountName = xriObj.Params["name"];
            }
            if (xriObj.Params.ContainsKey("secret"))
            {
                accountKey = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(xriObj.Params["secret"]));
            }
            logger.Info("Blob url: {0}, container name: {1}, account name: {2}", accessPoint, containerName, accountName);

            string connString = null;
            var accessPointUri = new Uri(accessPoint);
            if (string.IsNullOrEmpty(accountKey))
            {
                var authority = accessPointUri.Authority;
                connString = authority.StartsWith(accountName, StringComparison.OrdinalIgnoreCase)
                    ? authority
                    : $"{accountName}.{authority}";
            }
            else
            {
                var blobPrefix = "blob.";
                var endpointSuffix = accessPoint.Substring(accessPoint.LastIndexOf(blobPrefix) + blobPrefix.Length);
                if (endpointSuffix.IndexOf('/') > 0)
                {
                    endpointSuffix = endpointSuffix.Split('/')[0];
                }
                connString = $"DefaultEndpointsProtocol={accessPointUri.Scheme};AccountName={accountName};AccountKey={accountKey};EndpointSuffix={endpointSuffix}";
            }
            return StorageUtil.GetContainerClient(connString, containerName);
        }

        private static BlockBlobClient GetBlockBlobByStorageXRI(string xri, string lowName)
        {
            var container = GetBlobContainerClientByStorageXRI(xri);
            return container.GetBlockBlobClient(lowName);
        }

        #region Report Storage
        private static string ReportStorageConnectionString => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private static string ReportContainer => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_CONTAINER_NAME];
        private static string DaoJobStorageConnectionString => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.DAO_JOB_REPORT_STORAGE_CONNECTION_STRING];
        private static string DaoJobReportContainer => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.DAO_JOB_REPORT_CONTAINER_NAME];

        public static void CreateReportContainerIfNotExists()
        {
            AzureUtil.GetBlobContainerClient(ReportStorageConnectionString, ReportContainer, true);
        }

        public static void DeleteReportBlob(string blobName)
        {
            AzureUtil.DeleteBlob(ReportStorageConnectionString, ReportContainer, blobName);
        }

        public static void DeleteReportBlobs(string folderRelativePath)
        {
            AzureUtil.DeleteBlobs(ReportStorageConnectionString, ReportContainer, folderRelativePath);
        }

        public static void UploadReportBlob(string blobName, Stream content)
        {
            AzureUtil.UploadStorageBlob(ReportStorageConnectionString, ReportContainer, blobName, content);
        }

        public static void UploadReportBlob(string blobName, string filePath)
        {
            AzureUtil.UploadStorageBlob(ReportStorageConnectionString, ReportContainer, blobName, filePath);
        }
        public static void UploadReportBlobToSpecifyStorage(string connectionString,string blobName, string filePath)
        {
            AzureUtil.UploadStorageBlobByXRI(connectionString, blobName, filePath);
        }
        public static List<string> GetAllReportBlobNames(string blobName)
        {
            return AzureUtil.GetAllBlobNames(ReportStorageConnectionString, ReportContainer, blobName);
        }

        public static void AppendReportBlob(string blobName, byte[] content)
        {
            AzureUtil.AppendBlob(ReportStorageConnectionString, ReportContainer, blobName, content);
        }

        public static bool TryGetReportBlobLength(string blobName, out long contentLength)
        {
            return AzureUtil.TryGetBlobLength(ReportStorageConnectionString, ReportContainer, blobName, out contentLength);
        }

        public static Stream DownloadReportBlobToStream(string blobName)
        {
            return AzureUtil.DownloadBlobToStream(ReportStorageConnectionString, ReportContainer, blobName);
        }

        public static void DownloadReportBlobToFile(string blobName, string filePath)
        {
            AzureUtil.DownloadBlobToFile(ReportStorageConnectionString, ReportContainer, blobName, filePath);
            RAWebLocalCacheReleaser.RecordCacheFile(filePath);
        }

        //this method only used for download destruction report cache file
        public static void DownloadAllBlobsInContainer(string containerName, string filePath, DateTime startTime, DateTime endTime)
        {
            AzureUtil.DownloadAllBlobsInContainer(ReportStorageConnectionString, ReportContainer, containerName, filePath, startTime, endTime);
        }
        #endregion

        #region download archived content
        private static string ArchivedContentStorageConnectionString => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL];        
        private static string HistoryContentStorageConnectionString => RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private static string ArchivedContentContainer = "archivedcontent";
        private static string EmailImageBase64Container = "imagecontent";

        public static void DownloadArchivedContentToFile(string blobName, string filePath)
        {
            AzureUtil.DownloadBlobToFile(ArchivedContentStorageConnectionString, ArchivedContentContainer, blobName, filePath);
        }
        public static void DownloadRecordsArchivedContentToFile(string blobName, string filePath)
        {
            AzureUtil.DownloadBlobToFile(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName, filePath);
        }
        public static void DeleteExpiredArchivedContent(string blobName, bool isNewOpusTenant)
        {
            AzureUtil.DeleteBlob(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName);
            AzureUtil.DeleteBlobs(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName);
            if (!isNewOpusTenant)
            {
                AzureUtil.DeleteBlob(ArchivedContentStorageConnectionString, ArchivedContentContainer, blobName);
                AzureUtil.DeleteBlobs(ArchivedContentStorageConnectionString, ArchivedContentContainer, blobName);
            }
        }
        #endregion

        public static void UploadHistoryReport(string blobName, string filePath)
        {
            AzureUtil.UploadStorageBlob(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName, filePath);
        }
        public static void UploadHistoryReport(string blobName, Stream fileStream)
        {
            AzureUtil.UploadStorageBlob(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName, fileStream);
        }

        public static (string, bool) UploadStorageForDownloadCenter(string blobName, Stream fileStream, bool forceNeedSasUri = false,string sharedConnectionString = "")
        {
            return AzureUtil.UploadStorageBlobForDownloadCenter(ArchivedContentContainer, blobName, fileStream, forceNeedSasUri: forceNeedSasUri, sharedConnectionString: sharedConnectionString);
        }

        public static (string, bool) UploadStorageForDownloadCenter(string blobName, string filePath,bool forceNeedSasUri = false, string sharedConnectionString = "")
        {
            return AzureUtil.UploadStorageBlobForDownloadCenter(ArchivedContentContainer, blobName, filePath, forceNeedSasUri: forceNeedSasUri, sharedConnectionString: sharedConnectionString);
        }

        public static void UploadPhysicalBulkZip(string blobName, string filePath)
        {
            AzureUtil.UploadStorageBlob(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName, filePath);
        }

        public static void DownloadHistoryContentToFile(string blobName, string filePath)
        {
            AzureUtil.DownloadBlobToFile(HistoryContentStorageConnectionString, ArchivedContentContainer, blobName, filePath);
        }


        public static void DownloadAllArchivedContentFiles(string blobPrefixName, string folderPath)
        {
            var list = AzureUtil.GetAllBlobNames(HistoryContentStorageConnectionString, ArchivedContentContainer, blobPrefixName);
            logger.Info($"Need downloan files count:{list.Count}");
            foreach (var file in list)
            {
                var name = file.Split("/").Last();
                //DownloadArchivedContentToFile(file, GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, name));
                AzureUtil.DownloadBlobToFile(HistoryContentStorageConnectionString, ArchivedContentContainer, file, GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, name));
            }
        }

        public static void UploadImage(string blobName, string content)
        {
            AzureUtil.UploadTextToBlobContainer(HistoryContentStorageConnectionString, EmailImageBase64Container, blobName, content);
        }

        public static string GetSasUriForImageBlob(string blobName, TimeSpan expiryTime)
        {
            return AzureUtil.GenerateSasUriForRead(HistoryContentStorageConnectionString, EmailImageBase64Container, blobName, expiryTime);
        }

        public static List<string> AllBlobNames(string blobName)
        {
            return AzureUtil.GetAllBlobNames(HistoryContentStorageConnectionString, EmailImageBase64Container, blobName);
        }

        public static Stream DownloadImageBlobToStream(string blobName)
        {
            return AzureUtil.DownloadBlobToStream(HistoryContentStorageConnectionString, EmailImageBase64Container, blobName);
        }
        public static string DownloadImageBlobToText(string blobName)
        {
            return AzureUtil.DownloadBlobToText(HistoryContentStorageConnectionString, EmailImageBase64Container, blobName);
        }

        #region Permission sync job change log
        private static string ChangeLogContainer => "changelogreport";

        public static void UploadChangeLogBlob(string blobName, string filePath)
        {
            AzureUtil.UploadStorageBlob(ReportStorageConnectionString, ChangeLogContainer, blobName, filePath);
        }

        public static void DeleteChangeLogBlob(string blobName)
        {
            AzureUtil.DeleteBlob(ReportStorageConnectionString, ChangeLogContainer, blobName);
        }

        public static void DownloadChangeLogBlobToFile(string blobName, string filePath)
        {
            AzureUtil.DownloadBlobToFile(ReportStorageConnectionString, ChangeLogContainer, blobName, filePath);
        }

        public static List<BlobItem> GetAllChangeLogReportBlobs(string blobName)
        {
            return AzureUtil.GetAllBlobs(ReportStorageConnectionString, ChangeLogContainer, blobName);
        }

        #endregion
    }
}
