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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using RAArchiverCommon;

namespace HSMAzureCommon
{
    public class FreeContainerManager
    {

        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private FreeContainerParameters parameters;
        public FreeContainerParameters Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public FreeContainerManager() { }

        public FreeContainerManager(FreeContainerParameters paras)
        {
            this.parameters = paras;
        }

        public FreeContainerParameters CreateFreeContainers(IAveSite Site)
        {
            FreeContainerParameters paras = new FreeContainerParameters();
            try
            {
                AveProvisionedMigrationContainersInfo containers = Site.ProvisionMigraitonContainers();
                AveProvisionedMigrationQueueInfo queue = Site.ProvisionMigrationQueue();

                paras.MetadataContainerUri = containers.MetadataContainerUri;
                paras.DataContainerUri = containers.DataContainerUri;
                paras.JobQueueUri = queue.JobQueueUri;
                paras.EncryptionKey = containers.EncryptionKey;
            }
            catch (Exception ex)
            {
                mLog.Error("CreateFreeContainers Error:{0}", ex.ToString());
                try
                {
                    mLog.Info("Sleep 1 minute and ReCreate FreeContainers");
                    Thread.Sleep(60 * 1000);
                    AveProvisionedMigrationContainersInfo containers = Site.ProvisionMigraitonContainers();
                    AveProvisionedMigrationQueueInfo queue = Site.ProvisionMigrationQueue();

                    paras.MetadataContainerUri = containers.MetadataContainerUri;
                    paras.DataContainerUri = containers.DataContainerUri;
                    paras.JobQueueUri = queue.JobQueueUri;
                    paras.EncryptionKey = containers.EncryptionKey;
                }
                catch (Exception ex2)
                {
                    mLog.Error("Recreate still failed ,reason:{0}", ex2.ToString());
                    paras = null;
                }
            }
            return paras;
        }

        public bool UploadManifestDir(string folderPath)
        {
            bool result = false;
            try
            {
                var folder = new DirectoryInfo(folderPath);
                var containerClient = new BlobContainerClient(new Uri(this.parameters.MetadataContainerUri));
                foreach (FileInfo file in folder.GetFiles())
                {
                    UploadFile(file, containerClient);
                }
                var fileNames = folder.GetFiles().Select(f => f.Name).ToList();
                mLog.Info($"UploadManifestDir finished. files:{string.Join(",", fileNames)}");
                result = true;
            }
            catch (Exception ex)
            {
                mLog.Error("UploadManifestDir Error:{0}", ex.ToString());
            }
            return result;
        }

        public bool UploadDataDir(string folderPath)
        {
            bool result = false;
            try
            {
                var folder = new DirectoryInfo(folderPath);
                BlobClientOptions blobOptions = new()
                {
                    Retry = { // can keep default settings for retry config
                        //Delay = TimeSpan.FromSeconds(2),
                        //MaxRetries = 5,
                        //Mode = RetryMode.Exponential,
                        //MaxDelay = TimeSpan.FromSeconds(10),
                        NetworkTimeout = new TimeSpan(24, 0, 0) // as suggested, extend network timeout for large file upload, default 100 seconds is too short
                    },
                };
                var mContainer = new BlobContainerClient(new Uri(this.parameters.DataContainerUri), blobOptions);
                var folderFiles = folder.GetFiles();
                mLog.Info($"UploadDataDir folderPath:{folderPath}.FilesCount:{folderFiles.Length}.");
                foreach (FileInfo file in folderFiles)
                {
                    UploadFile(file, mContainer, true);
                }
                result = true;
                mLog.Info($"UploadDataDir finished. files:{string.Join(",", folderFiles.Select(f => f.Name))}");
            }
            catch (Exception ex)
            {
                mLog.Error("UploadDataDir Error:{0}", ex.ToString());
            }
            return result;
        }

        public void UploadFile(FileInfo file, BlobContainerClient container, bool isDataContainer = false, Dictionary<string, FileHash>? uploadFileHashDic = null)
        {
            var tempMBlob = container.GetBlockBlobClient(file.Name);
            byte[] IV = null;
            if (isDataContainer)
            {
                IV = AzureCommonWrapper.Instance.CreateIV();
            }
            else
            {
                IV = AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(128 / 8);
            }

            AzureCommonWrapper.Instance.EncryptContainerFile(file.FullName, this.parameters.EncryptionKey, IV, out string md5Hash, out string checksum, tempMBlob);
            if (isDataContainer && uploadFileHashDic != null 
                && !string.IsNullOrEmpty(checksum) && !string.IsNullOrEmpty(md5Hash))
            {
                uploadFileHashDic[file.Name] = new()
                {
                    Checksum = checksum,
                    MD5Hash = md5Hash,
                    IV = Convert.ToBase64String(IV),
                    LargeFile = file.Length >= AzureCommonWrapper.LimitFileSize
                };
            }
        }

        public void UploadFile(string filePath, string fileName, string containerUri, byte[] IV)
        {
            var containerClient = new BlobContainerClient(new Uri(containerUri));
            var tempBlob = containerClient.GetBlockBlobClient(fileName);
            AzureCommonWrapper.Instance.EncryptContainerFile(filePath, this.parameters.EncryptionKey, IV, out _, out _, tempBlob);
        }

        public bool WaitJobFinished(string manifestContainerPath, int retryMigrationJobTime,Guid jobId,IAveSite site, Action<string> callback)
        {
            bool finished = false;
            mLog.Info("begin wait for the job finished.");
            string queueMessages = string.Empty;
            try
            {
                var option = new QueueClientOptions();
                option.MessageEncoding = QueueMessageEncoding.Base64;
                var queueClient = new QueueClient(new Uri(this.parameters.JobQueueUri), option);
                DateTime time = DateTime.Now.AddMinutes(retryMigrationJobTime);
                DateTime waitTime = DateTime.Now;
                Guid currentJobId = Guid.NewGuid();

                #region test SharePoint Migration API Changes, will remove later RECO-32371
                try
                {
                    var keyValue = _keyValueDao.GetValueByKey("CheckMigrationJobModeForTesting");
                    if (keyValue == null || !int.TryParse(keyValue.Value, out var checkMigrationJobMode))
                    {
                        checkMigrationJobMode = 0;
                    }
                    mLog.Info($"RECO-32371. CheckMigrationJobMode: {checkMigrationJobMode}");
                    if (checkMigrationJobMode > 0) // > 0 mean force check and delete running job
                    {
                        mLog.Info("RECO-32371. Get stop job command and kill current import job :{0}", jobId);
                        if (checkMigrationJobMode == 1) // 1: old Api
                        {
                            mLog.Info("RECO-32371. Using old Api");
                            if (site.GetMigrationJobStatus(jobId) != AveMigrationJobState.None)
                            {
                                site.DeleteMigrationJob(jobId);
                                mLog.Info("RECO-32371. DeleteMigrationJob {0} success", jobId);
                            }
                        }
                        else // otherwise will use new Api
                        {
                            mLog.Info("RECO-32371. Using new Api. Job: {jobId}");
                            if (site.NeedDeleteMigrationJob(jobId))
                            {
                                site.DeleteMigrationJob(jobId);
                                mLog.Info("RECO-32371. DeleteMigrationJob {0} success", jobId);
                            }
                        }
                        mLog.Info("RECO-32371. DeleteMigrationJob {0} finished", jobId);
                        finished = true;
                        return finished;
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("RECO-32371. An error occurred while testing SharePoint Migration API Changes, skip it. Exception:{0}", e);
                }
                #endregion

                while (true)
                {
                    if (StopController.NeedStopJob)
                    {
                        mLog.Info("Get stop job command and kill current import job :{0}", jobId);
                        if (site.NeedDeleteMigrationJob(jobId))
                        {
                            site.DeleteMigrationJob(jobId);
                        }
                        mLog.Info("DeleteMigrationJob {0} finished", jobId);
                        finished = true;
                        break;
                    }
                    var message = queueClient.ReceiveMessage();
                    if (message != null && message.Value != null)
                    {
                        string asString = message.Value.MessageText;
                        AzureQueueMessage queueMessage = JsonUtility.DeserializerFromJson<AzureQueueMessage>(asString);
                        currentJobId = new Guid(queueMessage.JobId);
                        byte[] IV = Convert.FromBase64String(queueMessage.IV);
                        asString = AzureCommonWrapper.Instance.DecryptStringFromBytes(Convert.FromBase64String(queueMessage.Content), this.parameters.EncryptionKey, IV);
                        if (callback != null)
                        {
                            callback(asString);
                        }
                        queueMessages = string.Format("{0}{1}\n", queueMessages, asString);
                        time = DateTime.Now.AddMinutes(retryMigrationJobTime);
                        if (asString.IndexOf("JobEnd", StringComparison.OrdinalIgnoreCase) >= 0 && currentJobId == jobId)
                        {
                            finished = true;
                            var deleteTask = queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
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
                            var deleteTask = queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
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
                                mLog.Error("queue.DeleteMessage Error.message is null,reason:{0}", ex.ToString());
                            }
                            else
                            {
                                mLog.Error("queue.DeleteMessage Error.message:{0},reason:{1}", asString, ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        if ((DateTime.Now - waitTime).Minutes >= 3)
                        {
                            mLog.Info("Job {0} is waiting in the queue to get picked up.", jobId);
                            waitTime = waitTime.AddMinutes(3);
                        }
                        if (DateTime.Now > time)
                        {
                            mLog.Error("Exceed timeout limit,stop waiting");
                            try
                            {
                                //MigrationJob超时后，删除对应Job，避免更多的脏数据产生
                                mLog.Info($"try delete migration job. Job: {jobId}");
                                if (site.NeedDeleteMigrationJob(jobId))
                                {
                                    site.DeleteMigrationJob(jobId);
                                }
                                mLog.Info($"DeleteMigrationJob {jobId} finished when exceed timeout limit.");
                            }
                            catch (Exception ex)
                            {
                                mLog.Error($"DeleteMigrationJob {jobId} failed when exceed timeout limit.Error:{ex.ToString()}.");
                            }
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while getting the job status.Exception:{0}", e.ToString());
            }
            finally
            {
                if (!string.IsNullOrEmpty(queueMessages))
                {
                    string queueMessagePath = Path.Combine(manifestContainerPath, "queueMessge.txt");
                    File.WriteAllText(queueMessagePath, queueMessages);
                }
                mLog.Info("End wait for the job finished.");
            }
            return finished;
        }

        public bool DownloadFCImportLog(string logFileName, string downloadLoaction)
        {
            bool result = false;
            try
            {
                var mContainer = new BlobContainerClient(new Uri(this.parameters.MetadataContainerUri));
                var tempMBlob = mContainer.GetBlockBlobClient(logFileName);
                if (tempMBlob != null && tempMBlob.ExistsAsync().Result)
                {
                    return Retry(() =>
                    {
                        return DownloadDecryptedFile(logFileName, downloadLoaction, tempMBlob);
                    }, 3);
                }
                else
                {
                    mLog.Info("download fc import log failed, cannot find block reference {0} to {1}", logFileName, downloadLoaction);
                }
            }
            catch (Exception ex)
            {
                mLog.Error("DownloadFCImportLog Error :{0}", ex.ToString());
            }
            return result;
        }

        private bool DownloadDecryptedFile(string logFileName, string downloadLoaction, BlockBlobClient blobItem)
        {
            string localPath = Path.Combine(downloadLoaction, logFileName);
            try
            {
                mLog.Info("download fc import log {0} to {1}", logFileName, downloadLoaction);
                using (MemoryStream ms = new MemoryStream())
                {
                    var task = blobItem.DownloadToAsync(ms);
                    if (task.Wait(TimeSpan.FromMinutes(20)))
                    {
                        byte[] bytes = ms.ToArray();
                        string DecryptString = AzureCommonWrapper.Instance.DecryptStringFromBytes(bytes, this.parameters.EncryptionKey, AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(128 / 8));
                        if (BackgroundSettings.GetInstance().IsOutputVerboseLog)
                        {
                            mLog.InfoEncryptMessage(DecryptString);
                        }
                        System.IO.File.WriteAllText(localPath, DecryptString);
                        return true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(localPath))
                        {
                            if (File.Exists(localPath))
                            {
                                mLog.Info("[{0}]Delete partial downloaded file {1}", logFileName, localPath);
                                File.Delete(localPath);
                            }
                        }
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(localPath))
                {
                    if (File.Exists(localPath))
                    {
                        mLog.Info("[{0}]Delete partial downloaded file {1}", logFileName, localPath);
                        File.Delete(localPath);
                    }
                }
                mLog.Warn("[{0}] download file failed.Error:{1}", logFileName, e);
                return false;
            }
        }

        private bool Retry(Func<bool> downloadAction, int times)
        {
            bool hasDownload = false;
            for (int k = 0; k < times; k++)
            {
                if (downloadAction())
                {
                    hasDownload = true;
                    break;
                }
            }
            return hasDownload;
        }

        public bool DownloadImportLog(Guid jobId, IAveSite site, string downloadLocation)
        {
            bool result = false;
            try
            {
                string logFolderUrl = string.Format("{0}/{1}", site.ServerRelativeUrl, "_catalogs/MaintenanceLogs");
                IAveFolder folder = site.RootWeb.GetFolder(logFolderUrl);
                foreach (IAveFile file in folder.Files)
                {
                    if (file.Name.Contains(jobId.ToString()))
                    {
                        string downloadFilePath = Path.Combine(downloadLocation, file.Name);
                        byte[] fileBytes = file.OpenBinary();
                        File.WriteAllBytes(downloadFilePath, fileBytes);
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                mLog.Error("DownloadImportLog Error:{0}", ex.ToString());
            }
            return result;
        }
    }
    public class FreeContainerParameters
    {
        private string metadataContainerUri;
        public string MetadataContainerUri
        {
            get { return metadataContainerUri; }
            set { metadataContainerUri = value; }
        }
        private string dataContainerUri;
        public string DataContainerUri
        {
            get { return dataContainerUri; }
            set { dataContainerUri = value; }
        }
        private string jobQueueUri;
        public string JobQueueUri
        {
            get { return jobQueueUri; }
            set { jobQueueUri = value; }
        }
        private byte[] encryptionKey;
        public byte[] EncryptionKey
        {
            get { return encryptionKey; }
            set { encryptionKey = value; }
        }
    }
}
