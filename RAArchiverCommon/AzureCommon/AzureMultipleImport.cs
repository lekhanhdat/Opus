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
using System.Xml;
using AvePoint.GCommon;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using RAArchiverCommon;

namespace HSMAzureCommon
{

    public delegate bool WaitSourceFilesUploadedDelegate(string manifestContainerName);
    public delegate void UpdateProgressDelegate(AzureQueueMessage queueMessage);
    public delegate void UpdateErrorReportDelegate(AzureQueueMessage queueMessage, MutliImportParameter multiImportParameter = null);
    public delegate void UpdateErrorReportsDelegate(Dictionary<string, AzureQueueMessage> ErrorItems, MutliImportParameter multiImportParameter = null);
    public delegate void AnalyzeLogDelegate(string logFolderPath);
    public delegate void JobErrorDelegate(string errorMessage);
    public delegate void PostActionDelegate(MutliImportParameter multiImportParameter);
    public delegate void SendJobReportDelegate(MutliImportParameter multiImportParameter, bool isImportJobCanceled);
    public delegate void SendErrorJobReportDelegate(string errorMessage, MutliImportParameter multiImportParameter);
    public delegate void SendPackageResultDelegate(string ManifestContainerDir);

    public class AzureMultipleImport : AveMultiReceiveTask
    {

        private static AveLogger logger = AveLogger.GetInstance(typeof(AzureMultipleImport));
        private MutliImportParameter multiImportParameter;
        public event WaitSourceFilesUploadedDelegate WaitSourceFilesUploadedEvent;
        public event UpdateProgressDelegate updateProgressEvent;
        public event UpdateErrorReportDelegate updateErrorReportEvent;
        public event UpdateErrorReportsDelegate updateErrorReportsEvent;
        public event AnalyzeLogDelegate analyzeLogEvent;
        public event JobErrorDelegate jobErrorEvent;
        public PostActionDelegate PostActionEvent;
        public event SendJobReportDelegate sendJobReportEvent;
        public event SendErrorJobReportDelegate sendErrorJobReportEvent;
        public event SendPackageResultDelegate sendPackageResultEvent;
        private bool isImportJobCanceled = false;
        private List<string> importLogNamesList = new List<string>();
        private Dictionary<string, AzureQueueMessage> ErrorItems = new Dictionary<string, AzureQueueMessage>();

        public AzureMultipleImport(MutliImportParameter multiImportParameter, int level, bool ismultiply = true)
            : base(level, ismultiply)
        {
            this.multiImportParameter = multiImportParameter;
        }

        #region Override
        public override void PostAction()
        {
            //base.PostAction();
            if (PostActionEvent != null)
            {
                PostActionEvent(multiImportParameter);
            }
        }
        public override void PreAction()
        {
            //base.PreAction();
        }
        public override void Process()
        {
            if (!this.multiImportParameter.IsFreeContainer)
            {
                UploadData();
                CreateImportJob();
            }
            else
            {
                if (!WrapperConfiguration.IsRestoreJob)
                {
                    UploadFreeAzureData();
                }
                if (multiImportParameter.MigrationModuleType == MigrationModuleType.SPMigration)
                {
                    UpdateManifestXml();
                }
                CreateFreeContainerImportJob();
            }
        }

        public void UpdateManifestXml()
        {
            try
            {
                if (multiImportParameter.UploadFileHashDic.IsNullOrEmpty())
                {
                    return;
                }
                logger.Info($"Need update MD5 and Checksum into Manifest file. UploadFileHashDic count: {multiImportParameter.UploadFileHashDic.Count}");

                string filePath = Path.Combine(this.multiImportParameter.ManifestContainerDir, "Manifest.xml");
                if (!File.Exists(filePath)) return;

                var doc = new XmlDocument()
                {
                    XmlResolver = null
                };
                doc.Load(filePath);
                XmlNamespaceManager? xnsm = null;
                string np = doc.DocumentElement?.NamespaceURI ?? "";
                xnsm = new XmlNamespaceManager(doc.NameTable);
                xnsm.AddNamespace("ns", np);
                XmlNodeList? nodes = doc.DocumentElement?.SelectNodes("/ns:SPObjects/ns:SPObject/ns:File", xnsm);
                if (nodes != null)
                {
                    UpdateNodes(nodes);
                }

                nodes = doc.DocumentElement?.SelectNodes("/ns:SPObjects/ns:SPObject/ns:File/ns:Versions/ns:File", xnsm);
                if (nodes != null)
                {
                    UpdateNodes(nodes);
                }
                
                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while updating manifest xml. Ex: {ex}");
            }
        }

        private void UpdateNodes(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node is not XmlElement xe || !xe.HasAttribute("FileValue")) continue;
                string name = Convert.ToString(xe.GetAttribute("FileValue"));
                if (!multiImportParameter.UploadFileHashDic.TryGetValue(name, out var fileHash)) continue;

                xe.SetAttribute("MD5Hash", fileHash.MD5Hash);
                if (fileHash.LargeFile)
                {
                    xe.SetAttribute("Checksum", fileHash.Checksum);
                }
                else
                {
                    if (this.multiImportParameter.IsEncryption)
                    {
                        xe.SetAttribute("InitializationVector", fileHash.IV);
                    }
                }
            }
        }

        public override void Complete()
        {
        }
        public override void Exception(Exception e)
        {
            //base.Exception(e);
        }
        #endregion

        private void CreateImportJob()
        {
            try
            {
                using (AzureBlobManager manager = new AzureBlobManager(this.multiImportParameter.AzureInfo.AzureIused, this.multiImportParameter.AzureInfo.EndPointSuffixm, this.multiImportParameter.AzureInfo.AccountName, this.multiImportParameter.AzureInfo.AccountKey))
                {
                    if (!manager.LoginBlob())
                    {
                        throw new Exception("LoginAzureFail");
                    }
                    if (!manager.CreateDataContainer(this.multiImportParameter.AzureInfo.AzureManifestContainerName))
                    {
                        logger.Info("CreateDataContainer {0} failed", this.multiImportParameter.AzureInfo.AzureManifestContainerName);
                        throw new Exception("CreateContainerError");
                    }
                    else
                    {
                        logger.Info("CreateDataContainer {0} successfully", this.multiImportParameter.AzureInfo.AzureManifestContainerName);
                    }
                    if (GetImportToken(this.multiImportParameter.AzureInfo, manager))
                    {
                        if (manager.UploadMutipleFilesToAzure(this.multiImportParameter.ManifestContainerDir, this.multiImportParameter.IsEncryption))
                        {
                            CheckSourceFileUploaded(this.multiImportParameter.AzureInfo.AzureManifestContainerName);
                            ClearLocalManifestContainer();

                            Guid jobId = CreateMigrationJob();
                            logger.Debug("Migration Job ID:{0}", jobId.ToString());
                            if (jobId != Guid.Empty)
                            {
                                bool Successful = false;
                                ListenJobState(this.multiImportParameter.AzureInfo, jobId, ref Successful);
                                if (Successful && !isImportJobCanceled)
                                {
                                    DeleteAzureData(manager);
                                    if (ErrorItems.Count > 0 && updateErrorReportsEvent != null)
                                    {
                                        this.updateErrorReportsEvent(ErrorItems, this.multiImportParameter);
                                    }
                                    if (sendPackageResultEvent != null)
                                    {
                                        this.sendPackageResultEvent(this.multiImportParameter.ManifestContainerDir);
                                    }
                                }
                                else if (StopController.NeedStopJob && !Successful)
                                {
                                    logger.Info("jobId :{0}, ManifestDir:{1} was canceled", jobId, this.multiImportParameter.ManifestContainerDir);
                                    this.isImportJobCanceled = true;
                                }
                                else
                                {
                                    logger.Debug("Current Job {0} is finished with error, ManifestPath:{1} ,start retrying", jobId, this.multiImportParameter.ManifestContainerDir);
                                    ErrorItems.Clear();
                                    this.isImportJobCanceled = false;
                                    RetryMigrationJob(jobId, manager);
                                }
                                if (sendJobReportEvent != null)
                                {
                                    this.sendJobReportEvent(this.multiImportParameter, this.isImportJobCanceled);
                                }
                            }
                        }
                        else
                        {
                            logger.Error("UploadMutipleFilesToAzure Failed.");
                            throw new Exception("UploadMutipleFilesToAzure Failed.");
                        }
                    }
                    else
                    {
                        logger.Warn("An error occurred at Get Import token");
                        throw new Exception("GetImportTokenError");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("XmlDirpath :{0} import job failed,reason:{1}", this.multiImportParameter.ManifestContainerDir, ex.ToString());
                if (jobErrorEvent != null)
                {
                    jobErrorEvent(ex.Message);
                }
                if (sendErrorJobReportEvent != null)
                {
                    sendErrorJobReportEvent(ex.Message, this.multiImportParameter);
                }
            }
        }

        private Boolean WaitJobFinished(AzureUploadSetting setting, Guid jobId)
        {
            bool result = false;
            logger.Info("begin wait for the job finished.");
            Uri url = new Uri(setting.AzureSetting.AccessPoint);
            try
            {
                bool useHttps = url.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? true : false;
                string endPointSuffixm = url.DnsSafeHost;
                string accountName = setting.AzureSetting.AccountName;

                endPointSuffixm = endPointSuffixm.Substring(accountName.Length + 1);
                endPointSuffixm = endPointSuffixm.Substring(endPointSuffixm.IndexOf('.') + 1);

                string accountKey = setting.AzureSetting.AccountKey;
                using (AzureBlobManager manager = new AzureBlobManager(useHttps, endPointSuffixm, accountName, accountKey))
                {
                    manager.SetBlobRequestOptions(setting.BlobRequestOptionsServerTimeoutHour, setting.BlobRequestOptionsServerTimeoutMinute, setting.BlobRequestOptionsClientTimeoutHour, setting.BlobRequestOptionsClientTimeoutMinute);
                    manager.SetQueueRequestOptions(setting.BlobRequestOptionsServerTimeoutHour, setting.BlobRequestOptionsServerTimeoutMinute);
                    if (manager.LoginBlob())
                    {
                        result = manager.WaitJobFinished(setting, this.multiImportParameter.ManifestContainerDir, this.multiImportParameter.RetryMigrationJobTime, jobId, this.multiImportParameter.Site, WaitJobFinishedAction);
                    }
                    else
                    {
                        logger.Warn("login Azure Failed");
                        result = false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Info("An error occurred while getting the job status.Exception:{0}", e.ToString());
                result = false;
            }
            finally
            {
                logger.Info("End wait for the job finished.");
            }
            return result;
        }

        private void WaitJobFinishedAction(string msg)
        {
            try
            {
                if (BackgroundSettings.GetInstance().IsOutputVerboseLog)
                {
                    logger.Info($"QueueMessage:");
                    logger.InfoEncryptMessage(msg);
                }
                AzureQueueMessage queueMessage = JsonUtility.DeserializerFromJson<AzureQueueMessage>(msg);
                switch (queueMessage.Event)
                {
                    case "JobProgress":
                        if (updateProgressEvent != null)
                        {
                            updateProgressEvent(queueMessage);
                        }
                        break;
                    case "JobLogFileCreate":
                        if (!this.importLogNamesList.Contains(queueMessage.FileName))
                        {
                            importLogNamesList.Add(queueMessage.FileName);
                        }
                        break;
                    case "JobWarning":
                    case "JobError":
                        if (!string.IsNullOrEmpty(queueMessage.Id))
                        {
                            //ErrorItems[queueMessage.Id] = queueMessage;
                            if (ErrorItems.TryGetValue(queueMessage.Id, out var existing))
                            {
                                if (!string.Equals(existing.Message, queueMessage.Message))
                                {
                                    logger.Error($"JobError duplicate for file [{queueMessage.Id} {queueMessage.Url}] but different message. Existing: {existing.Message}, New: {queueMessage.Message}");
                                }
                            }
                            else
                            {
                                ErrorItems[queueMessage.Id] = queueMessage;
                            }
                        }
                        //if (updateErrorReportEvent != null)
                        //{
                        //    updateErrorReportEvent(queueMessage, this.multiImportParameter);
                        //}
                        break;
                    case "JobRestart":
                        ErrorItems.Clear();
                        break;
                    case "JobCancelled":
                    case "JobFatalError":
                        this.isImportJobCanceled = true;
                        logger.Warn($"JobCancelled or JobFatalError for file {queueMessage.Url}, QueueEvent: {queueMessage.Event}, QueueMessage: {queueMessage.Message}");
                        break;
                    default: break;
                }

            }
            catch (Exception e)
            {
                logger.Debug("An error occurred while De-serialize the queue message,exception:{0}", e);
            }
        }

        private void CheckSourceFileUploaded(string manifestContainerName)
        {
            if ((this.multiImportParameter.MigrationModuleType == MigrationModuleType.LotusNotesMigration || this.multiImportParameter.MigrationModuleType == MigrationModuleType.FileMigration || this.multiImportParameter.MigrationModuleType == MigrationModuleType.DocumentumMigration) && this.multiImportParameter.IsNeedCheckSourceFilesUploaded)
            {
                bool isUploadSourceFileFinished = WaitSourceFilesUploadedEvent(manifestContainerName);
                if (!isUploadSourceFileFinished)
                {
                    logger.Error("Didn't finish uploading source content files , Manifest Path:{0},run job anyway", this.multiImportParameter.ManifestContainerDir);
                }
            }
        }

        private Guid CreateMigrationJob()
        {
            Guid jobId = Guid.NewGuid();
            if (this.multiImportParameter.IsFreeContainer)
            {
                IAveEncryptionOption option = new EncryptionOption()
                {
                    AES256CBCKey = this.multiImportParameter.FCParameters.EncryptionKey
                };
                jobId = this.multiImportParameter.Site.CreateMigrationJobEncrypted(this.multiImportParameter.WebId, this.multiImportParameter.FCParameters.DataContainerUri, this.multiImportParameter.FCParameters.MetadataContainerUri, this.multiImportParameter.FCParameters.JobQueueUri, option);
            }
            else
            {
                if (this.multiImportParameter.IsEncryption)
                {
                    IAveEncryptionOption option = new EncryptionOption()
                    {
                        AES256CBCKey = AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(256 / 8)
                    };
                    jobId = this.multiImportParameter.Site.CreateMigrationJobEncrypted(this.multiImportParameter.WebId, this.multiImportParameter.AzureInfo.AzureContainerSourceUri, this.multiImportParameter.AzureInfo.AzureContainerManifestUri, this.multiImportParameter.AzureInfo.AzureQueueReportUri, option);
                }
                else
                {
                    jobId = this.multiImportParameter.Site.CreateMigrationJob(this.multiImportParameter.WebId, this.multiImportParameter.AzureInfo.AzureContainerSourceUri, this.multiImportParameter.AzureInfo.AzureContainerManifestUri, this.multiImportParameter.AzureInfo.AzureQueueReportUri);
                }
            }

            return jobId;
        }

        private Guid CreateMigrationJobWithRetry()
        {
            Guid jobId = Guid.NewGuid();
            if (this.multiImportParameter.IsFreeContainer)
            {
                IAveEncryptionOption option = new EncryptionOption()
                {
                    AES256CBCKey = this.multiImportParameter.FCParameters.EncryptionKey
                };
                jobId = CreateMigrationJobEncryptedWithRetry(this.multiImportParameter.WebId, this.multiImportParameter.FCParameters.DataContainerUri, this.multiImportParameter.FCParameters.MetadataContainerUri, this.multiImportParameter.FCParameters.JobQueueUri, option);
            }
            else
            {
                if (this.multiImportParameter.IsEncryption)
                {
                    IAveEncryptionOption option = new EncryptionOption()
                    {
                        AES256CBCKey = AzureCommonWrapper.Instance.GenerateTempKey().GetBytes(256 / 8)
                    };
                    jobId = CreateMigrationJobEncryptedWithRetry(this.multiImportParameter.WebId, this.multiImportParameter.AzureInfo.AzureContainerSourceUri, this.multiImportParameter.AzureInfo.AzureContainerManifestUri, this.multiImportParameter.AzureInfo.AzureQueueReportUri, option);
                }
                else
                {
                    jobId = this.multiImportParameter.Site.CreateMigrationJob(this.multiImportParameter.WebId, this.multiImportParameter.AzureInfo.AzureContainerSourceUri, this.multiImportParameter.AzureInfo.AzureContainerManifestUri, this.multiImportParameter.AzureInfo.AzureQueueReportUri);
                }
            }

            return jobId;
        }

        /// <summary>
        /// 1.5分钟Retry一次，Retry6次
        /// 2.半小时内,6次都失败则Create Migration Job Failed
        /// </summary>
        private Guid CreateMigrationJobEncryptedWithRetry(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            Guid jobId = Guid.Empty;
            int retryCount = 6;
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    jobId = this.multiImportParameter.Site.CreateMigrationJobEncrypted(gWebId, azureContainerSourceUri, azureContainerManifestUri, azureQueueReportUri, options);
                    break;
                }
                catch (Exception e)
                {
                    logger.Warn($"CreateMigrationJobEncryptedWithRetry : {i} , error :{e.ToString()}");
                    Thread.Sleep(5 * 60 * 1000);
                    this.multiImportParameter.Site.ReloadSite();
                    if (i == retryCount - 1)
                    {
                        throw;
                    }
                }
            }
            return jobId;
        }

        private void RetryMigrationJob(Guid lastJobId, AzureBlobManager manager)
        {
            //if (this.multiImportParameter.Site.GetMigrationJobStatus(lastJobId) != AveMigrationJobState.None)
            {
                try
                {
                    logger.Info("Delete Job:{0}", lastJobId);
                    this.multiImportParameter.Site.DeleteMigrationJob(lastJobId);
                }
                catch (Exception ex)
                {
                    logger.Warn("Delete Job failed when RetryMigrationJob:{0}", ex);
                }
            }
            Guid retryJobId = CreateMigrationJob();
            logger.Debug("Retry Migration Job ID:{0}", retryJobId.ToString());
            if (retryJobId != Guid.Empty)
            {
                bool retrySuccessful = false;
                ListenJobState(this.multiImportParameter.AzureInfo, retryJobId, ref retrySuccessful);
                if (retrySuccessful)
                {
                    DeleteAzureData(manager);
                    if (ErrorItems.Count > 0 && updateErrorReportsEvent != null)
                    {
                        this.updateErrorReportsEvent(ErrorItems, this.multiImportParameter);
                    }
                    if (sendPackageResultEvent != null)
                    {
                        this.sendPackageResultEvent(this.multiImportParameter.ManifestContainerDir);
                    }
                }
                else
                {
                    logger.Info("Retry migration job still failed.");
                    logger.Info($"try delete migration job. Job: {retryJobId}");
                    if (this.multiImportParameter.Site.NeedDeleteMigrationJob(retryJobId))
                    {
                        logger.Info("Delete Retry Job:{0}", retryJobId);
                        this.multiImportParameter.Site.DeleteMigrationJob(retryJobId);
                    }
                    throw new Exception("WaitJobFinishTimeOut");
                }
            }
        }

        private bool GetImportToken(WinAzure AzureInfo, AzureBlobManager manager)
        {
            AzureUploadSetting Gettokensetting = new AzureUploadSetting()
            {
                AzureSetting = new AzureLocationInfo()
                {
                    AccessPoint = AzureInfo.AccessPoint,
                    AccountKey = AzureInfo.AccountKey,
                    AccountName = AzureInfo.AccountName
                },
                MainfestContainerName = AzureInfo.AzureManifestContainerName,
                LifeTime = 5,
                SourceContainerName = AzureInfo.AzureSourceContainerName,
                QueueContainName = AzureInfo.AzureQueueReportContainerName,
                BlobRequestOptionsClientTimeoutHour = 10,
                BlobRequestOptionsClientTimeoutMinute = 0,
                BlobRequestOptionsServerTimeoutMinute = 0,
                BlobRequestOptionsServerTimeoutHour = 10
            };
            AzureResult temptoken = AzureCommonWrapper.Instance.GetAzureContainerToken(Gettokensetting, manager);
            if (temptoken != null)
            {
                if (!temptoken.AzureIused)
                {
                    if (!string.IsNullOrEmpty(temptoken.ErrorMessage))
                    {
                        throw new Exception(temptoken.ErrorMessage);
                    }
                }
                else
                {
                    this.multiImportParameter.AzureInfo.AzureContainerManifestUri = temptoken.AzureContainerManifestUri;
                    this.multiImportParameter.AzureInfo.AzureContainerSourceUri = temptoken.AzureContainerSourceUri;
                    this.multiImportParameter.AzureInfo.AzureQueueReportUri = temptoken.AzureQueueReportUri;
                    this.multiImportParameter.AzureInfo.AzureManifestContainerName = temptoken.AzureManifestContainerName;
                    this.multiImportParameter.AzureInfo.AzureQueueReportContainerName = temptoken.AzureQueueReportContainerName;
                }
            }
            return true;
        }

        private void ListenJobState(WinAzure azureInfo, Guid jobId, ref bool Successful)
        {
            AzureUploadSetting Gettokensetting = new AzureUploadSetting()
            {
                AzureSetting = new AzureLocationInfo()
                {
                    AccessPoint = azureInfo.AccessPoint,
                    AccountKey = azureInfo.AccountKey,
                    AccountName = azureInfo.AccountName,
                },
                MainfestContainerName = azureInfo.AzureManifestContainerName,
                LifeTime = 3600,
                SourceContainerName = azureInfo.AzureSourceContainerName,
                QueueContainName = azureInfo.AzureQueueReportContainerName,
                BlobRequestOptionsClientTimeoutHour = 10,
                BlobRequestOptionsClientTimeoutMinute = 0,
                BlobRequestOptionsServerTimeoutMinute = 0,
                BlobRequestOptionsServerTimeoutHour = 10,
                IsEncryption = this.multiImportParameter.IsEncryption
            };

            if (WaitJobFinished(Gettokensetting, jobId))
            {
                logger.Info("Finished Job id is:{0}, XmlDirpath :{1},Create Migration Job has been Finished", jobId, this.multiImportParameter.ManifestContainerDir);
                Successful = true;
                if (DownloadAzureImportLog(azureInfo))
                {
                    if (analyzeLogEvent != null)
                    {
                        analyzeLogEvent(this.multiImportParameter.ManifestContainerDir);
                    }
                }
                else
                {
                    logger.Error("DownloadAzureImportLog Failed .");
                }
            }
            else
            {
                logger.Warn("XmlDirpath :{0},Create Migration Job has been Timeout,jobId:{1}.", this.multiImportParameter.ManifestContainerDir, jobId);
            }
        }

        private Boolean DownloadAzureImportLog(WinAzure azureInfo)
        {
            bool result = false;
            int count = 0;
            try
            {
                AuzreDownLoadSetting setting = new AuzreDownLoadSetting()
                {
                    AzureSetting = new AzureLocationInfo()
                    {
                        AccessPoint = azureInfo.AccessPoint,
                        AccountKey = azureInfo.AccountKey,
                        AccountName = azureInfo.AccountName
                    },
                    ExportLocation = this.multiImportParameter.ManifestContainerDir,
                    MainfestContainerName = azureInfo.AzureManifestContainerName,
                    FileDonwloadType = FileDownloadType.Logs,
                    BlobRequestOptionsClientTimeoutHour = 10,
                    BlobRequestOptionsClientTimeoutMinute = 10,
                    BlobRequestOptionsServerTimeoutMinute = 10,
                    BlobRequestOptionsServerTimeoutHour = 10,
                    NeedDelete = true,
                    IsEncryption = this.multiImportParameter.IsEncryption
                };
                using (AzureBlobManager manager = new AzureBlobManager(this.multiImportParameter.AzureInfo.AzureIused, this.multiImportParameter.AzureInfo.EndPointSuffixm, this.multiImportParameter.AzureInfo.AccountName, this.multiImportParameter.AzureInfo.AccountKey))
                {
                    manager.SetBlobRequestOptions(setting.BlobRequestOptionsServerTimeoutHour, setting.BlobRequestOptionsServerTimeoutMinute, setting.BlobRequestOptionsClientTimeoutHour, setting.BlobRequestOptionsClientTimeoutMinute);
                    if (manager.LoginBlob())
                    {
                        logger.Info("begin Check file on the Azure,FileType:{0}.Thread:{1}", setting.FileDonwloadType, Thread.CurrentThread.ManagedThreadId);
                        while (true)
                        {
                            try
                            {
                                result = AzureCommonWrapper.Instance.VerifyAndDownloadAzureFile(setting, manager);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("An error occurred while download log file,exception:{0}", ex.ToString());
                                break;
                            }
                            if (result == true || count >= 300)
                            {
                                break;
                            }
                            else
                            {
                                System.Threading.Thread.Sleep(2000);
                                count++;
                            }
                        }
                    }
                    else
                    {
                        logger.Error("login Azure Failed");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred in function LoadFileFromAzure , exception:{0}", ex.ToString());
            }
            logger.Debug("Download log file result:{0},count:{1}", result, count);
            return result;
        }

        private void DeleteAzureData(AzureBlobManager manager)
        {
            try
            {
                manager.DeleteQueueContainer(this.multiImportParameter.AzureInfo.AzureQueueReportContainerName);
                manager.DeleteBlobContainer(this.multiImportParameter.AzureInfo.AzureManifestContainerName);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred at deleta azure data, msg: {0}", ex.ToString());
            }
        }

        private void UploadData()
        {
            if (!string.IsNullOrEmpty(this.multiImportParameter.DataContainerDir))
            {
                try
                {
                    using (AzureBlobManager manager = new AzureBlobManager(this.multiImportParameter.AzureInfo.AzureIused, this.multiImportParameter.AzureInfo.EndPointSuffixm, this.multiImportParameter.AzureInfo.AccountName, this.multiImportParameter.AzureInfo.AccountKey))
                    {
                        if (!manager.LoginBlob())
                        {
                            //throw new Exception(MigrationJobSummaryMessage.HSMLoginAzureFail);
                        }
                        if (!manager.CreateDataContainer(this.multiImportParameter.AzureInfo.AzureSourceContainerName))
                        {
                            logger.Info("CreateDataContainer {0} failed", this.multiImportParameter.AzureInfo.AzureSourceContainerName);
                            //throw new Exception(MigrationJobSummaryMessage.HSMCreateContainerError);
                        }
                        else
                        {
                            logger.Info("CreateDataContainer {0} successfully", this.multiImportParameter.AzureInfo.AzureSourceContainerName);
                        }

                        if (manager.UploadMutipleFilesToAzure(multiImportParameter.DataContainerDir, multiImportParameter.IsEncryption, true))
                        {
                            logger.Info("Upload data {0} successfully", this.multiImportParameter.AzureInfo.AzureSourceContainerName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Upload data {0} failed,reason:{1}", multiImportParameter.DataContainerDir, ex.ToString());
                    if (jobErrorEvent != null)
                    {
                        jobErrorEvent(ex.Message);
                    }
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(multiImportParameter.DataContainerDir))
                        {
                            Directory.Delete(multiImportParameter.DataContainerDir, true);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while deleting dataContainer {0}. Exception: {1}.", multiImportParameter.DataContainerDir, e.ToString());
                    }
                }
            }
        }

        private void UploadFreeAzureData()
        {
            if (!string.IsNullOrEmpty(this.multiImportParameter.DataContainerDir))
            {
                try
                {
                    FreeContainerManager fcManager = new FreeContainerManager(multiImportParameter.FCParameters);
                    fcManager.UploadDataDir(this.multiImportParameter.DataContainerDir);
                }
                catch (Exception ex)
                {
                    logger.Error("Upload FreeAzure data {0} failed,reason:{1}", multiImportParameter.DataContainerDir, ex.ToString());
                    if (jobErrorEvent != null)
                    {
                        jobErrorEvent(ex.Message);
                    }
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(multiImportParameter.DataContainerDir))
                        {
                            Directory.Delete(multiImportParameter.DataContainerDir, true);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while deleting dataContainer {0}. Exception: {1}.", multiImportParameter.DataContainerDir, e.ToString());
                    }
                }
            }
        }

        private void ClearLocalManifestContainer()
        {
            try
            {
                if (Directory.Exists(multiImportParameter.ManifestContainerDir))
                {
                    foreach (var file in Directory.GetFiles(multiImportParameter.ManifestContainerDir))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while deleting ManifestContainer {0}. Exception: {1}.", multiImportParameter.ManifestContainerDir, e.ToString());
            }
        }

        #region FreeContainer
        private void CreateFreeContainerImportJob()
        {
            try
            {
                FreeContainerManager fcManager = new FreeContainerManager(this.multiImportParameter.FCParameters);
                if (fcManager.UploadManifestDir(this.multiImportParameter.ManifestContainerDir))
                {
                    logger.Info("Start to create freeContainer job");
                    CheckSourceFileUploaded(this.multiImportParameter.FCParameters.MetadataContainerUri);
                    ClearLocalManifestContainer();

                    Guid jobId = CreateMigrationJobWithRetry();
                    logger.Info("Migration Job ID:{0}", jobId.ToString());
                    if (jobId != Guid.Empty)
                    {
                        if (fcManager.WaitJobFinished(this.multiImportParameter.ManifestContainerDir, this.multiImportParameter.RetryMigrationJobTime, jobId, this.multiImportParameter.Site, WaitJobFinishedAction) && !this.isImportJobCanceled)
                        {
                            foreach (string logName in importLogNamesList)
                            {
                                if (fcManager.DownloadFCImportLog(logName, this.multiImportParameter.ManifestContainerDir))
                                {
                                    logger.Info("download log {0} to {1} successfully !", logName, this.multiImportParameter.ManifestContainerDir);
                                }
                            }
                            if (ErrorItems.Count > 0 && updateErrorReportsEvent != null)
                            {
                                this.updateErrorReportsEvent(ErrorItems, this.multiImportParameter);
                            }
                        }
                        else if (StopController.NeedStopJob)
                        {
                            logger.Info("jobId :{0}, ManifestDir:{1} was stopped", jobId, this.multiImportParameter.ManifestContainerDir);
                            this.isImportJobCanceled = true;
                        }
                        else
                        {
                            ErrorItems.Clear();
                            logger.Debug("Current Job {0} is finished with error, ManifestPath:{1} ,start retrying", jobId, this.multiImportParameter.ManifestContainerDir);
                            this.isImportJobCanceled = false;
                            RetryFCMigrationJob(jobId, fcManager);
                        }
                        if (sendJobReportEvent != null)
                        {
                            this.sendJobReportEvent(this.multiImportParameter, this.isImportJobCanceled);
                        }
                    }
                }
                else
                {
                    logger.Error("UploadManifestDir Failed.");
                    throw new Exception("UploadManifestDir Failed.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("XmlDirpath :{0} import job failed,reason:{1}", this.multiImportParameter.ManifestContainerDir, ex.ToString());
                if (jobErrorEvent != null)
                {
                    jobErrorEvent(ex.Message);
                }
                if (sendErrorJobReportEvent != null)
                {
                    sendErrorJobReportEvent(ex.Message, this.multiImportParameter);
                }
            }
        }

        private void RetryFCMigrationJob(Guid lastJobId, FreeContainerManager fcManager)
        {
            //if (this.multiImportParameter.Site.GetMigrationJobStatus(lastJobId) != AveMigrationJobState.None)
            {
                try
                {
                    logger.Debug("Delete Job:{0}", lastJobId);
                    this.multiImportParameter.Site.DeleteMigrationJob(lastJobId);
                }
                catch (Exception ex)
                {
                    logger.Warn("Delete Job failed when RetryFCMigrationJob:{0}", ex);
                }
            }
            Guid retryJobId = CreateMigrationJob();
            logger.Debug("Retry Migration Job ID:{0}", retryJobId.ToString());
            if (fcManager.WaitJobFinished(this.multiImportParameter.ManifestContainerDir, this.multiImportParameter.RetryMigrationJobTime, retryJobId, this.multiImportParameter.Site, WaitJobFinishedAction))
            {
                foreach (string logName in importLogNamesList)
                {
                    if (fcManager.DownloadFCImportLog(logName, this.multiImportParameter.ManifestContainerDir))
                    {
                        logger.Info("download log {0} to {1} successfully !", logName, this.multiImportParameter.ManifestContainerDir);
                    }
                }
                if (ErrorItems.Count > 0 && updateErrorReportsEvent != null)
                {
                    this.updateErrorReportsEvent(ErrorItems, this.multiImportParameter);
                }
            }
            else
            {
                logger.Info("Retry migration job still failed.");
                throw new Exception("WaitJobFinishTimeOut");
            }
        }
        #endregion
    }
}
