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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using Azure.Storage.Sas;
using Google.Cloud.Logging.V2;
using Newtonsoft.Json;
using OpenAI.Containers;
using Storage;
using Storage.Cloud.Google;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Util;
using Util.MSAzure;

namespace AvePoint.RA.Common.Util
{
    public class AzureUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AzureUtil));

        public static SqlConnection GetConnectionUseIdentityToken(string connectionStr)
        {
            string token = null;
            var sqlBuilder = new SqlConnectionStringBuilder(connectionStr);
            if (string.IsNullOrEmpty(sqlBuilder.Password))
            {
                var resId = GetAzureSqlResourceId();
                token = GetTokenByPodIdentity(resId);
            }
            var rnd = new Random();
            return TransientFaultHandler.Process(() =>
            {
                var conn = new SqlConnection(connectionStr);
                if (!string.IsNullOrEmpty(token))
                {
                    conn.AccessToken = token;
                }
                conn.Open();
                return conn;
                /* Fortify Issue Type: Insecure Randomness 
                * Sink Details: this position 
                * Ignore Reason: random用于重试时间间隔 
                */
            }, 10, r => TimeSpan.FromSeconds(5 + rnd.Next(1, 5)));
        }

        /// <summary>
        /// return Microsoft.Data.SqlClient.SqlConnection;
        /// can't use with System.Data.SqlClient.SqlParameter;
        /// only use for create tenant db
        /// </summary>
        public static DbConnection GetConnection(string dbServer, string dbName, string identityUserId = null)
        {
            var sqlBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
            sqlBuilder.DataSource = dbServer;
            sqlBuilder.InitialCatalog = dbName;
            sqlBuilder.ConnectTimeout = 300;
            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                sqlBuilder.Encrypt = false;
                sqlBuilder.UserID = RMGlobalConfiguration.DBConfig.ConfigDatabaseUserName;
                sqlBuilder.Password = RMGlobalConfiguration.DBConfig.ConfigDatabasePassword;
            }
            else if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                sqlBuilder.UserID = RMGlobalConfiguration.DBConfig.ConfigDatabaseUserName;
                sqlBuilder.Password = RMGlobalConfiguration.DBConfig.ConfigDatabasePassword;
                sqlBuilder.TrustServerCertificate = true;
            }

            return SQLUtil.GetConnection(sqlBuilder.ToString());
        }

        #region Storage Util
        public static BlobContainerClient GetBlobContainerClient(string connectionString, string containerName, bool createIfNotExists = false)
        {
            var client = StorageUtil.GetContainerClient(connectionString, containerName);
            if (createIfNotExists)
            {
                client.CreateIfNotExists();
            }
            return client;
        }
        public static BlobContainerClient GetBlobContainerClientByXRI(string connectionString,bool createIfNotExists = false)
        {
            var client = RAStorageUtil.GetBlobContainerClientByStorageXRI(connectionString);
            if (createIfNotExists)
            {
                client.CreateIfNotExists();
            }
            return client;
        }
        public static BlobServiceClient GetBlobServiceClient(string connectionString)
        {
            return StorageUtil.GetServiceClient(connectionString);
        }

        public static void UploadStorageBlob(string connectionString, string containerName, string blobName, string filePath, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName, true);
            client.GetBlobClient(blobName).Upload(filePath, overwrite);
        }
        public static void UploadStorageBlobByXRI(string connectionString, string blobName, string filePath, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClientByXRI(connectionString, true);
            client.GetBlobClient(blobName).Upload(filePath, overwrite);
        }
        public static void UploadStorageBlob(string connectionString, string containerName, string blobName, string filePath, AccessTier accessTier, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName, true);
            client.GetBlobClient(blobName).Upload(
                filePath, 
                new BlobUploadOptions() { 
                    AccessTier = AccessTier.Cool,
                    Conditions = overwrite ? null : new BlobRequestConditions { IfNoneMatch = new ETag("*") }
                });
        }

        public static void UploadStorageBlob(string connectionString, string containerName, string blobName, Stream content, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName, true);
            client.GetBlobClient(blobName).Upload(content, overwrite);
        }



        /// <summary>
        /// 该方法为上传文件到storage并且生成到Download Center使用,根据文件大小判断文件是否需要Sasuri,上传文件如果需要生成sasUri则需要使用shared connection string ,其余情况可以使用其他connection string.
        /// 
        /// </summary>
        public static (string, bool) UploadStorageBlobForDownloadCenter(string containerName, string blobName, string filePath, string connectionString = null, bool overwrite = true, bool forceNeedSasUri = false, string sharedConnectionString="")
        {

            if (connectionString == null)
            {
                connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            }
            var needSasUri = forceNeedSasUri ? true : NeedSasUri(filePath);
            if (needSasUri)
            {
                connectionString = sharedConnectionString;
                containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                blobName = Path.Combine("downloadcenter", blobName);
            }
            UploadStorageBlob(connectionString, containerName, blobName, filePath);
            return (blobName, needSasUri);
        }
        public static (string, bool) UploadStorageBlobForDownloadCenter(string containerName, string blobName, Stream stream, string connectionString = null, bool overwrite = true, bool forceNeedSasUri = false, string sharedConnectionString = "")
        {
            if (connectionString == null)
            {
                connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            }
            var needSasUri = forceNeedSasUri ? true : NeedSasUri(stream);
            if (needSasUri)
            {
                connectionString = sharedConnectionString;
                containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                blobName = Path.Combine("downloadcenter", blobName);
            }
            UploadStorageBlob(connectionString, containerName, blobName, stream);
            return (blobName, needSasUri);
        }

        public static (string, bool) UploadStorageBlobWithFolderContainName(string containerName, string blobName, string filePath, string folderContainName, string connectionString = null, bool overwrite = true, bool forceNeedSasUri = false, string sharedConnectionString = "")
        {

            if (connectionString == null)
            {
                connectionString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            }
            var needSasUri = forceNeedSasUri ? true : NeedSasUri(filePath);
            if (needSasUri)
            {
                connectionString = sharedConnectionString;
                containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                blobName = Path.Combine(folderContainName, blobName);
            }
            UploadStorageBlob(connectionString, containerName, blobName, filePath);
            return (blobName, needSasUri);
        }
        private static bool NeedSasUri(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length > RMGlobalConfiguration.GetDCDownloadFileSizeLimit();
        }
        private static bool NeedSasUri(Stream stream)
        {
            return stream.Length > RMGlobalConfiguration.GetDCDownloadFileSizeLimit();
        }

        public static void UploadTextToBlobContainer(string connectionString, string containerName, string blobName, string content, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName, true);
            var blob = client.GetBlobClient(blobName);
            blob.Upload(new BinaryData(content), overwrite);
        }
        public static string GenerateSasUriForRead(string connectionString, string containerName, string blobName, TimeSpan expiryTime)
        {
            return StorageUtil.GenerateSasUriForRead(connectionString, containerName, blobName, expiryTime);
        }
        public static void UploadTextToBlobContainerByXRIString(string connectionString, string blobName, string content, bool overwrite = true)
        {
            BlobContainerClient client = GetBlobContainerClientByXRI(connectionString, true);
            var blob = client.GetBlobClient(blobName);
            blob.Upload(new BinaryData(content), overwrite);
        }
        public static void AppendBlob(string connectionString, string containerName, string blobName, byte[] content, bool createIfNotExists = false)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName, createIfNotExists);
            var appendBlobClient = client.GetAppendBlobClient(blobName);
            appendBlobClient.CreateIfNotExists();
            appendBlobClient.AppendBlock(new MemoryStream(content));
        }

        public static void DeleteBlob(string connectionString, string containerName, string blobName)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName);
            var blob = client.GetBlobClient(blobName);
            blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public static async Task DeleteContainer(string connectionString, string containerName)
        {
            BlobContainerClient client = GetBlobContainerClient(connectionString, containerName);
            await client.DeleteIfExistsAsync();
        }

        public static void DeleteBlobs(string connectionString, string containerName, string folderPath, bool createIfNotExists = false)
        {
            folderPath = folderPath.Replace('\\', '/');
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName, createIfNotExists);
            var resultSegment = containerClient.GetBlobsByHierarchy(default, default, prefix: folderPath, delimiter: "/")
                .AsPages(default, 100);
            foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
            {
                foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                {
                    if (blobhierarchyItem.IsPrefix)
                    {
                        DeleteBlobs(connectionString, containerName, blobhierarchyItem.Prefix);
                    }
                    else
                    {
                        containerClient.DeleteBlobIfExists(blobhierarchyItem.Blob.Name, DeleteSnapshotsOption.IncludeSnapshots);
                    }
                }
            }
        }

        public static bool TryGetBlobLength(string connectionString, string containerName, string blobName, out long contentLength)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            if (!blobClient.Exists().Value)
            {
                contentLength = 0;
                return false;
            }

            contentLength = blobClient.GetProperties().Value.ContentLength;
            return true;
        }

        public static Stream DownloadBlobToStream(string connectionString, string containerName, string blobName)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName);           
            var blobClient = containerClient.GetBlobClient(blobName);
            blobClient.DownloadContent().Value.Content.ToStream();
            if (blobClient.Exists().Value)
            {
                return blobClient.DownloadContent().Value.Content.ToStream();
            }
            return null;
        }

        public static async Task DownloadBlobToAsync(string connectionString, string containerName, string blobName, string destinationPath)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName, true);
            await DownloadBlobToAsync(containerClient, blobName, destinationPath);
        }

        public static async Task DownloadBlobToAsync(BlobContainerClient containerClient, string blobName, string destinationPath)
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            if(await blobClient.ExistsAsync())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                await blobClient.DownloadToAsync(destinationPath);
            }
        }

        public static void DownloadBlobToFile(string connectionString, string containerName, string blobName, string filePath)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName,true);
            var blobClient = containerClient.GetBlobClient(blobName);
            if (blobClient.Exists().Value)
            {
                var blobType = blobClient.GetProperties().Value.BlobType;
                //TODO: ? 是否需要区分
                if (blobType == BlobType.Block || blobType == BlobType.Append)
                {
                    blobClient.DownloadTo(filePath);
                }
            }
        }

        //this method only used for download destruction report cache file
        public static void DownloadAllBlobsInContainer(string connectionString, string containerName, string subFolderPath, string filePath, DateTime startTime, DateTime endTime)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName);
            var blobs = containerClient.GetBlobs(default, default, prefix: subFolderPath.Replace("\\", "/").TrimEnd('/') + "/", default).ToList();            
            foreach (var blob in blobs)
            {
                var name = blob.Name.Split("/").Last();
                if (name.Contains(".rpt") && name.Length > 4)
                {
                    if (IsBlobInTimeRange(startTime, endTime, name))
                    {
                        var blobClient = containerClient.GetBlobClient(blob.Name);
                        if (blobClient.Exists().Value)
                        {
                            var blobType = blobClient.GetProperties().Value.BlobType;
                            //TODO: ? 是否需要区分
                            if (blobType == BlobType.Block || blobType == BlobType.Append)
                            {
                                blobClient.DownloadTo(Path.Combine(filePath, name));
                                logger.Info($"Blob: {blob.Name} download success.");
                            }
                        }
                    }
                    else
                    {
                        logger.Info($"blob: {name} not in time range.");
                    }
                }
                else
                {
                    logger.Warn($"Invalid blob name: {name}");
                }
            }
        }

        //如果cache文件的开始时间或者结束时间在report time range内，则下载此report, cache file name format is starttime_endtime_jobId.rpt
        private static bool IsBlobInTimeRange(DateTime reportStartTime, DateTime reportEndTime, string blobName)
        {
            bool inRange = false;
            try
            {
                string startTimeStr = blobName.Split('_')[0];
                string endTimeStr = blobName.Split('_')[1];
                if (long.TryParse(startTimeStr, out long startTicks) && long.TryParse(endTimeStr, out long endTicks))
                {
                    DateTime startDate = new DateTime(startTicks, DateTimeKind.Utc);
                    DateTime endDate = new DateTime(endTicks, DateTimeKind.Utc);

                    if ((startDate >= reportStartTime && startDate <= reportEndTime)
                        || (endDate >= reportStartTime && endDate <= reportEndTime))
                    {
                        inRange = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while checking blob time range. Name:{blobName} Error:{e.ToString()}");
                inRange = false;
            }
            return inRange;
        }

        public static string DownloadBlobToText(string connectionString, string containerName, string blobName)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName, true);
            var blobClient = containerClient.GetBlobClient(blobName);
            blobClient.DownloadContent().Value.Content.ToStream();
            if (blobClient.Exists().Value)
            {
                return blobClient.DownloadContent().Value.Content.ToString();
            }
            return null;
        }

        public string CreateSASForBLOB(string blobName, string connectionString, string containerName)
        {
            try
            {
                BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName, true);
                var sasUri = containerClient.GenerateSasUri(BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(6));
                return sasUri.ToString();
            }
            catch (Exception e)
            {
                logger.Error($"blobName:{blobName},Create SAS for blob exception:" + e.ToString());
                return string.Empty;
            }
        }

        public static List<string> GetAllBlobNames(string connectionString, string containerName, string blobName)
        {
            return GetAllBlobs(connectionString, containerName, blobName).Select(blob => blob.Name).ToList();
        }

        public static List<BlobItem> GetAllBlobs(string connectionString, string containerName, string blobName)
        {
            BlobContainerClient containerClient = GetBlobContainerClient(connectionString, containerName, true);
            return containerClient.GetBlobs(BlobTraits.None, BlobStates.None, blobName, default).ToList();
        }

        public static TableClient GetTableClient(string connectionString, string tableName, bool createIfNotExists = false)
        {
            connectionString = GetTableString(connectionString);
            var client = TableUtil.GetTableClient(connectionString, tableName);
            if (createIfNotExists)
            {
                client.CreateIfNotExists();
            }
            return client;
        }

        public static TableServiceClient GetServiceClient(string connectionString)
        {
            connectionString = GetTableString(connectionString);
            return TableUtil.GetServiceClient(connectionString);
        }

        private static string GetTableString(string connectionString)
        {
            return connectionString.Replace(".blob.", ".table.");
        }
        private static IDictionary<string, string> ParseStringIntoSettings(string connectionString)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] array = connectionString.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(new char[1] { '=' }, 2);
                if (array2.Length != 2)
                {
                    logger.Warn("Settings must be of the form \"name=value\".");
                    return null;
                }

                if (dictionary.ContainsKey(array2[0]))
                {
                    logger.Warn(string.Format(CultureInfo.InvariantCulture, "Duplicate setting '{0}' found.", array2[0]));
                    return null;
                }

                dictionary.Add(array2[0], array2[1]);
            }

            return dictionary;
        }

        public static string GetGoogleConnectionBuilderString(string connString, string containerName)
        {
            if (!string.IsNullOrEmpty(connString))
            {
                var builder = ConnectionBuilder.ValueOf(connString);
                builder.Params.Add(GoogleXRIParameterKeys.RootFolder_Key, containerName);
                return builder.ToString();
            }
            else
            {
                return connString;
            }
        }
        public static string GetConnectionBuilderString(string connString, string containerName)
        {
            if (!string.IsNullOrEmpty(connString))
            {
                var builder = new ConnectionBuilder { StorageName = StorageName.Azure };
                builder.Params.Add(Storage.Cloud.Azure.AzureXRIParameterKeys.ContainerKey, containerName);
                builder.Params.Add(XRIParameterKeys.CREATE_IF_NOT_EXISTS, "true");
                builder.Params.Add(XRIParameterKeys.CustomizedModeKey, "CustomizedOnly");
                builder.Params.Add(XRIParameterKeys.CustomizedMetaKey, "{[com,avepoint],[dataVersion,2022-02]}");
                builder.Params.Add(XRIParameterKeys.ADVANCED_KEY, "false");
                if (IsConnectionString(connString))
                {
                    //var tempConn = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                    var setting = ParseStringIntoSettings(connString);
                    builder.Params.Add(XRIParameterKeys.AccessPointKey, $"{setting["DefaultEndpointsProtocol"]}://blob.{setting["EndpointSuffix"]}");
                    builder.Params.Add(XRIParameterKeys.USERNAME_KEY, setting["AccountName"]);
                    builder.Params.Add(XRIParameterKeys.PASSWORD_KEY, setting["AccountKey"]);
                }
                else
                {
                    //recoqatest.blob.core.windows.net
                    var accountName = connString.Substring(0, connString.IndexOf("."));
                    var accessPoint = "https://" + connString.Replace(accountName + ".", "");

                    builder.Params.Add(XRIParameterKeys.AccessPointKey, accessPoint);
                    builder.Params.Add(XRIParameterKeys.USERNAME_KEY, accountName);
                    builder.Params.Add(XRIParameterKeys.PASSWORD_KEY, "");
                }
                return builder.ToString();
            }
            else
            {
                return connString;
            }
        }

        /// <summary>
        /// (accountName, accessPoint)
        /// </summary>
        public static (string, string) ParseConfigConnectionString(string connString)
        {
            if (IsConnectionString(connString))
            {
                var setting = ParseStringIntoSettings(connString);
                return (setting["AccountName"], $"{setting["DefaultEndpointsProtocol"]}://blob.{setting["EndpointSuffix"]}".TrimEnd('/'));
            }
            else
            {
                var accountName = connString.Substring(0, connString.IndexOf("."));
                return (accountName, "https://" + connString.Replace(accountName + ".", "").TrimEnd('/'));
            }
        }

        /// <summary>
        /// (accountName, accessPoint)
        /// </summary>
        public static (string, string) ParseSavedConnectionString(string connString)
        {
            var builder = ConnectionBuilder.ValueOf(connString);
            builder.Params.TryGetValue(XRIParameterKeys.AccessPointKey, out string accessPoint);
            builder.Params.TryGetValue(XRIParameterKeys.USERNAME_KEY, out string accountName);
            return (accountName, accessPoint?.TrimEnd('/'));
        }

        private static bool IsConnectionString(string connectionString)
        {
            return connectionString.StartsWith("DefaultEndpointsProtocol=");
        }
        #endregion

        public static string GetManagementResourceId(string azureEnvName = null)
        {
            return GetAzureEndpoints(azureEnvName).Management;
        }

        private static AzureEnvironment GetAzureEnvironment(string azureEnvName = null)
        {
            if (string.IsNullOrEmpty(azureEnvName))
            {
                azureEnvName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.AZURE_ENVIRONMENT];
            }
            AzureEnvironment environment;
            if (!Enum.TryParse(azureEnvName, out environment))
            {
                environment = AzureEnvironment.Worldwide;
            }
            return environment;
        }

        public static Endpoints GetAzureEndpoints(string azureEnvName = null)
        {
            var environment = GetAzureEnvironment(azureEnvName);
            return Endpoints.GetEndpoints(environment);
        }

        public static string GetAzureSqlResourceId(string azureEnvName = null)
        {
            var environment = GetAzureEnvironment(azureEnvName);
            switch (environment)
            {
                case AzureEnvironment.USGovDoD:
                case AzureEnvironment.USGovGCCHigh:
                    return "https://database.usgovcloudapi.net";
                case AzureEnvironment.China:
                    return "https://database.chinacloudapi.cn";
                case AzureEnvironment.Germany:
                    return "https://database.cloudapi.de";
                case AzureEnvironment.Worldwide:
                default:
                    return "https://database.windows.net";
            }
        }

        public static string GetTokenByPodIdentity(string resId)
        {
            var task = Task.Run(() => IdentityUtil.GetTokenAsync(resId));
            task.Wait(new TimeSpan(0, 5, 0));
            return task.Result;
        }

        public static async Task DeleteTableAsync(string connectionStr,string tableName)
        {
            try
            {
                var client = GetTableClient(connectionStr, tableName);
                if (client != null)
                {
                    await client.DeleteAsync();
                    logger.Info($"Delete {tableName} successful");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Delete {tableName} failed, error :{e}");
            }
        }
    }
}
