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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;
using AvePoint.Wrapper.Common;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.ProjectServer.Client;
using Azure.Storage.Blobs;
using Azure.Storage;
using Azure.Core.Pipeline;
using Azure.Core;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Azure.Storage.Queues;
using System.Collections;
using Microsoft.Extensions.Azure;
using DocumentFormat.OpenXml.Bibliography;

namespace HSMAzureCommon
{
    public class EncryptionOption : IAveEncryptionOption
    {
        public byte[] AES256CBCKey { get; set; }
    }
    public class AzureBlobManager : IDisposable
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(AzureBlobManager));

        private readonly bool _useHttps = false;

        private readonly string _endpointSuffix = string.Empty;

        private readonly string _accountName = string.Empty;

        private readonly string _accountKey = string.Empty;

        private readonly BlobServiceClient _blobServiceClient;

        private readonly QueueServiceClient _queueServiceClient;

        private BlobContainerClient _blobContainerClient;

        bool isGetProxyFinished = false;

        private readonly BlobClientOptions _blobClientOptions = new()
        {
            RetryPolicy = new RetryPolicy(3)
        };

        private readonly QueueClientOptions _queueClientOptions = new()
        {
            RetryPolicy = new RetryPolicy(3)
        };

        public AzureBlobManager(bool useHttps, string endPointSuffix, string accountName, string accountKey)
        {
            _useHttps = useHttps;
            _endpointSuffix = endPointSuffix;
            _accountName = accountName;
            _accountKey = accountKey;

            var credential = new StorageSharedKeyCredential(accountName, accountKey);

            _blobServiceClient = new BlobServiceClient(new Uri(endPointSuffix), credential, _blobClientOptions);
            _queueServiceClient =  new QueueServiceClient(new Uri(endPointSuffix), credential, _queueClientOptions);

            if (!isGetProxyFinished)
            {
                InitProxy();
            }
        }

        private void InitProxy()
        {
            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            isGetProxyFinished = true;
        }

        public bool LoginBlob()
        {
            var loginSuccess = false;
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("$root");
                var exist = containerClient.Exists();
                loginSuccess = true;
            }
            catch (Exception ex)
            {
                s_logger.Info("Login Azure Blob exception:" + ex.ToString());
                s_logger.Info("Retry LoginBlob");
                DateTime retryStartTime = DateTime.Now;
                int RetryConnectionTimeOutHours = 2;
                while (true)
                {
                    if (DateTime.Now < retryStartTime.AddHours(RetryConnectionTimeOutHours))
                    {
                        s_logger.Info("Start Retry LoginBlob");
                        try
                        {
                            var containerClient = _blobServiceClient.GetBlobContainerClient("$root");
                            var exist = containerClient.Exists();
                            s_logger.Info("RetryLoginBlob successful.");
                            loginSuccess = true;
                            break;
                        }
                        catch (Exception ex1)
                        {
                            s_logger.Info($"RetryLoginBlob 5 minutes later. Error: {ex1}");
                            Thread.Sleep(5 * 60 * 1000);
                        }
                    }
                    else
                    {
                        s_logger.Info("Retry TimeOut ,Timeout hours:{0}", RetryConnectionTimeOutHours);
                        break;
                    }
                }
            }
            return loginSuccess;
        }

        public bool UploadFileToAzure(string blobName, string filePath, bool isEncryption, byte[] IV)
        {
            try
            {
                string checksum = string.Empty;
                string md5Hash = string.Empty;
                var blobClient = _blobContainerClient.GetBlockBlobClient(blobName);
                if (isEncryption)
                {
                    AzureCommonWrapper.Instance.EncryptContainerFile(filePath, AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(256 / 8), IV, out md5Hash, out checksum);
                }
                else
                {
                    if (blobClient != null)
                    {
                        using var fs = AzureCommonWrapper.Instance.Open(filePath, FileMode.Open, FileAccess.Read);
                        blobClient.UploadAsync(fs).GetAwaiter().GetResult();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                s_logger.Warn("An error occurred while uploading file {0} to Azure.Exception:{1}", filePath, ex.ToString());
                throw;
            }
        }

        private BlobContainerClient CreateContainer(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.CreateIfNotExists();
                return containerClient;
            }
            catch (Exception ex)
            {
                s_logger.Debug(" Create Blob Container exception:" + ex.ToString());
                throw;
            }

        }

        public WinAzure GetImportToken(string sourceContainerName, string mainfestContainerName, string queueContainerName, int lifeTime = 60)
        {
            WinAzure azureInfo = new()
            {
                AzureIused = false
            };

            var containerSourceClient = _blobServiceClient.GetBlobContainerClient(sourceContainerName);
            var containerMainfestClient = _blobServiceClient.GetBlobContainerClient(mainfestContainerName);

            if(!containerSourceClient.Exists() || !containerMainfestClient.Exists())
            {
                return azureInfo;
            }

            var containerSourceSasUri = containerSourceClient.GenerateSasUri(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.List, DateTimeOffset.UtcNow.AddDays(lifeTime));

            azureInfo.AzureSourceContainerName = sourceContainerName;
            azureInfo.AzureContainerSourceUri = containerSourceSasUri.ToString();

            var containerMainfestSasUri = containerMainfestClient.GenerateSasUri(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write | BlobContainerSasPermissions.List, DateTimeOffset.UtcNow.AddDays(lifeTime));

            azureInfo.AzureContainerManifestUri = containerMainfestSasUri.ToString();
            azureInfo.AzureManifestContainerName = mainfestContainerName;

            var queueClient = _queueServiceClient.GetQueueClient(queueContainerName);
            queueClient.CreateIfNotExists();
            var queueSasUri = queueClient.GenerateSasUri(QueueSasPermissions.Read | QueueSasPermissions.Add | QueueSasPermissions.Update | QueueSasPermissions.Process, DateTimeOffset.UtcNow.AddDays(lifeTime));

            azureInfo.AzureQueueReportUri = queueSasUri.ToString();
            azureInfo.AzureQueueReportContainerName = queueContainerName;
            azureInfo.AzureIused = true;

            return azureInfo;
        }

        public void Dispose()
        {
            
        }

        public void SetBlobRequestOptions(int serverTimeoutHour, int serverTimeoutMinute, int clientTimeoutHour, int clientTimeoutMinute)
        {
            s_logger.Debug("serverTimeoutHour:{0},serverTimeoutMinute:{1},clientTimeoutHour:{2},clientTimeoutMinute:{3}", serverTimeoutHour, serverTimeoutMinute, clientTimeoutHour, clientTimeoutMinute);
        }

        public void SetQueueRequestOptions(int serverTimeoutHour, int serverTimeoutMinute)
        {
            s_logger.Debug("serverTimeoutHour:{0},serverTimeoutMinute:{1}", serverTimeoutHour, serverTimeoutMinute);
        }

        public bool CreateDataContainer(string ContainerName)
        {
            _blobContainerClient ??= CreateContainer(ContainerName);
            return _blobContainerClient != null;
        }

        public bool UploadMutipleFilesToAzure(string folderPath, bool isEncryption, bool isDataContainer = false)
        {
            try
            {
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach (FileInfo file in folder.GetFiles())
                {
                    byte[] IV = null;
                    if (isDataContainer)
                    {
                        IV = AzureCommonWrapper.Instance.CreateIV();
                    }
                    else
                    {
                        IV = AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(128 / 8);
                    }
                    if (!UploadFileToAzure(file.Name, file.FullName, isEncryption, IV))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                s_logger.Warn("An error occurred while uploading the file to Azure.Exception:{0}", ex.ToString());
                throw;
            }
        }

        public bool WaitJobFinished(AzureUploadSetting setting, string manifestContainerPath, int retryMigrationJobTime, Guid jobId, IAveSite site, Action<string> callback)
        {
            bool finished = false;
            string queueMessages = string.Empty;
            try
            {
                var  queueClient = _queueServiceClient.GetQueueClient(setting.QueueContainName);
                DateTime time = DateTime.Now.AddMinutes(retryMigrationJobTime);
                DateTime waitTime = DateTime.Now;
                Guid currentJobId = Guid.NewGuid();
                while (true)
                {
                    if (StopController.NeedStopJob)
                    {
                        s_logger.Info("Get stop job command and kill current import job :{0}", jobId);
                        if (site.NeedDeleteMigrationJob(jobId))
                        {
                            site.DeleteMigrationJob(jobId);
                        }
                        s_logger.Info("DeleteMigrationJob {0} finished", jobId);
                        finished = true;
                        break;
                    }

                    var message = queueClient.ReceiveMessage().Value;

                    if (message != null)
                    {
                        var asString = message.MessageText;
                        AzureQueueMessage queueMessage = JsonUtility.DeserializerFromJson<AzureQueueMessage>(asString);
                        currentJobId = new Guid(queueMessage.JobId);
                        if (setting.IsEncryption)
                        {
                            byte[] IV = Convert.FromBase64String(queueMessage.IV);
                            asString = AzureCommonWrapper.Instance.DecryptStringFromBytes(Convert.FromBase64String(queueMessage.Content), AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(256 / 8), IV);
                        }
                        if (callback != null)
                        {
                            callback(asString);
                        }
                        queueMessages = string.Format("{0}{1}\n", queueMessages, asString);
                        time = DateTime.Now.AddMinutes(retryMigrationJobTime);
                        if (asString.IndexOf("JobEnd", StringComparison.OrdinalIgnoreCase) >= 0 && currentJobId == jobId)
                        {
                            finished = true;
                            var deleteTask = queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            if (deleteTask != null)
                            {
                                deleteTask.Wait();
                                if (deleteTask.IsCompleted && deleteTask.Exception != null)
                                {
                                    throw deleteTask.Exception;
                                }
                            }
                            break;
                        }
                        try
                        {
                            var deleteTask = queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            if (deleteTask != null)
                            {
                                deleteTask.Wait();
                                if (deleteTask.IsCompleted && deleteTask.Exception != null)
                                {
                                    throw deleteTask.Exception;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (message == null)
                            {
                                s_logger.Error("queue.DeleteMessage Error.message is null,reason:{0}", ex.ToString());
                            }
                            else
                            {
                                s_logger.Error("queue.DeleteMessage Error.message:{0},reason:{1}", asString, ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - waitTime).Minutes >= 3)
                        {
                            s_logger.Info("Job {0} is waiting in the queue to get picked up.", jobId);
                            waitTime = waitTime.AddMinutes(3);
                        }
                        if (DateTime.Now > time)
                        {
                            s_logger.Error("Exceed timeout limit,stop waiting");
                            try
                            {
                                s_logger.Info($"try delete migration job. Job: {jobId}");
                                if (site.NeedDeleteMigrationJob(jobId))
                                {
                                    site.DeleteMigrationJob(jobId);
                                }
                            }
                            catch(Exception ex)
                            {
                                s_logger.Error($"An error occurred while deleting migration job {ex}");
                            }
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                s_logger.Error("An error occurred while getting the job status.Exception:{0}", e.ToString());
            }
            finally
            {
                if (!string.IsNullOrEmpty(queueMessages))
                {
                    string queueMessagePath = Path.Combine(manifestContainerPath, "queueMessge.txt");
                    File.WriteAllText(queueMessagePath, queueMessages);
                }
            }
            return finished;
        }

        public bool VerifyAzureData(string mainfestContainerName, DownloadFileType downloadType)
        {
            var isSuccess = false;

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(mainfestContainerName);
            var resultSegment = blobContainerClient.GetBlobs().AsPages();
            foreach(var blobPage in resultSegment)
            {
                foreach(var blobItem in blobPage.Values)
                {
                    if(!FilterFiles(blobItem.Name, downloadType))
                    {
                        isSuccess = true;
                    }
                }
            }

            return isSuccess;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "wrn")]
        private bool FilterFiles(string blobName, DownloadFileType downloadType)
        {
            switch (downloadType)
            {
                case DownloadFileType.None:
                    {

                        return true;
                    }
                case DownloadFileType.XML:
                    {
                        if (blobName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                case DownloadFileType.Logs:
                    {
                        if (blobName.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                case DownloadFileType.Warn:
                    {
                        if (blobName.EndsWith(".wrn", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                case DownloadFileType.Err:
                    {
                        if (blobName.EndsWith(".err", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                default:
                    {
                        return true;
                    }
            }
            //return true;

        }

        public bool DeleteBlobContainer(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                return containerClient.DeleteIfExistsAsync().GetAwaiter().GetResult().Value;
            }
            catch (Exception ex)
            {
                s_logger.Debug(" delete Blob Container exception:" + ex.ToString());
                return false;
            }
        }

        public bool DeleteQueueContainer(string queueName)
        {
            try
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName);
                return queueClient.DeleteIfExists().Value;
            }
            catch (Exception ex)
            {
                s_logger.Debug(" Delete Queue Blob Container exception:" + ex.ToString());
                return false;
            }
        }

        public Boolean DownloadMutipleFiles(string folderPath, string manifestContainerName, DownloadFileType downloadType, bool isEncrytion, bool needDelete = false)
        {
            bool isSuccess = false;
            var folder = new DirectoryInfo(folderPath);

            var blobClients = new List<BlockBlobClient>();

            if (!folder.Exists)
            {
                s_logger.Debug(" folder {0} is not exists.", folderPath);
                return isSuccess;
            }
            else
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(manifestContainerName);
                var resultSegment = containerClient.GetBlobs().AsPages();
                foreach(var blobPage in resultSegment)
                {
                    foreach(var blobItem in blobPage.Values)
                    {
                        if(FilterFiles(blobItem.Name, downloadType))
                        {
                            continue;
                        }

                        try
                        {
                            var blockBlobClient = containerClient.GetBlockBlobClient(blobItem.Name);
                            if (!isEncrytion)
                            {
                                using var fs = new FileStream(Path.Combine(folder.FullName, blobItem.Name), FileMode.Create);

                                var response = blockBlobClient.DownloadTo(fs);
                                if (response.IsError)
                                {
                                    throw new Exception(response.ReasonPhrase);
                                }
                            }
                            else
                            {
                                using var memoryStream = new MemoryStream();
                                var response = blockBlobClient.DownloadTo(memoryStream);
                                if (response.IsError)
                                {
                                    throw new Exception(response.ReasonPhrase);
                                }

                                var bytes = memoryStream.ToArray();
                                var decryptString = AzureCommonWrapper.Instance.DecryptStringFromBytes(bytes, AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(256 / 8), AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(128 / 8));
                                File.WriteAllText(Path.Combine(folder.FullName, blobItem.Name), decryptString);
                                s_logger.Info("Download file {0} and Decrypt contents", blobItem.Name);
                            }

                            isSuccess = true;
                            if (needDelete)
                            {
                                blobClients.Add(blockBlobClient);
                            }
                        }
                        catch(Exception e)
                        {
                            s_logger.Warn($"Download files {blobItem.Name} exception:{e}.");
                            return isSuccess;
                        }
                    }
                }

                if (needDelete)
                {
                    foreach (var blob in blobClients)
                    {
                        try
                        {
                            var deleted = blob.DeleteIfExistsAsync().Result;
                            s_logger.Debug("Delete the File,Name:{0}", blob.Name);
                        }
                        catch (Exception e)
                        {
                            s_logger.Warn("An error occurred while delete the blob,Name:{0},exception:{1}", blob.Name, e);
                        }
                    }
                }
            }
            return isSuccess;
        }
    }
}
