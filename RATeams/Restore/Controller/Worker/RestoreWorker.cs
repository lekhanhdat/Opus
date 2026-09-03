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

//namespace Office365GroupRestore
//{
//    #region using directives

//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;

  
//    using AvePoint.Application.Security.Extension;
//    using AvePoint.Common;
//    using AvePoint.GCommon.Contract.CloudServiceCommon;
//    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
//    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
//    using AvePoint.GCommon.Contract.Server.Job.Object;
//    //using AvePoint.GCommon.JobManagement;
//    //using AvePoint.GCommon.MicroKernel;
//    //using AvePoint.Rehydrate;
//    //using AvePoint.RehydrateCore;
//    //using AvePoint.Application.Configuration;
//    //using WorkerServiceWrapper;
//    //using Microsoft365Backup.CommonUtil.Misc;
//    //using Application.Storage.Azure.Metrics.Job;
//    using AvePoint.Media.Common;
//    using AvePoint.Media.Service;
//    using AvePoint.Media.Service.DomainModel;
//    using AvePoint.Media.Service.ExchangeBackup;


//    using ExchangeCommonWrapper;

//    using ExchangeUtility.Graph;

//    using Microsoft365.Common.RequestMonitor;

    
   


//    using Job.ModernManagement.Report;
//    using System.Threading.Tasks;
//    using AvePoint.RA.CommonUtil;
//    using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

//    #endregion

//    public class RestoreWorker : IRestoreWorker
//    {
//        private static RALogger logger = RALogger.GetInstance(typeof(RestoreWorker));

//        public RestoreConfig Config { get; set; }
//        public ERMessage Message { get; set; }
//        public JobQueueMessage QueueMessage { get; set; }
//        public IReportCenter Report { get; set; }

//        private JobStatus jobStatus = JobStatus.Failed;
//        private readonly string cachePath = "";//stodo AveEnv.GetAgentTempFolder(ContextLevel.Process);
//        private String errorMessage = string.Empty;

//        public ICacheService CacheManager { get; set; }

//        //public IExchangeRestoreReport Report { get; set; }
//        public IRestoreService RestoreService { get; set; }

//        public ExchangeRestoreJob ExchangeRestoreJob { get; set; }

//        //public RehydrateTask RehydrateTask { get; set; }

//        public void Initailize(ERMessage message, JobQueueMessage queueMessage)
//        {
//            this.Message = message;
//            this.QueueMessage = queueMessage;
//            this.Config = new RestoreConfig(message);
//            Message.ConfigForMedia.IndexEncryptionInfoWrapper.PutEncryptionInfo();
//            this.InitMediaSetting(message);
//            this.InitGlobalSetting(message);
//            this.InitRestoreReport();
//            EnableEWSNonitor();
//            //RestoreDataMonitor.Instance?.RecordJobInfo(message?.ConfigForMedia?.BackupCycleId, message?.ConfigForMedia?.BackupTime, message?.ConfigForMedia?.OnlyOneJob);
//        }

//        //private void TryRehydrate()
//        //{
//        //    try
//        //    {
//        //        using (var service = WorkerServiceLocator.GetRequiredService<IRehydrateService>())
//        //        {
//        //            var taskContext = new RehydrateTaskContext(
//        //                new RehydrateJobContext
//        //                {
//        //                    BackupCycleId = Message.ConfigForMedia.BackupCycleId,
//        //                    CustomerId = QueueMessage.JobTenantInfo.TenantId,
//        //                    JobId = QueueMessage.JobId,
//        //                    SubJobId = $"{QueueMessage.SubJobId}Rehydrate",
//        //                    JobType = QueueMessage.JobType,
//        //                    ProgressUpdater = (progress) => { },
//        //                    RestoreRequest = Message.ConfigForMedia,
//        //                    TenantOwner = Message.TenantGroupOwner
//        //                },
//        //                new RehydrateSetting
//        //                {
//        //                    ByosRehydrateInt = Message.Config.ByosRehydrateInt,
//        //                    IsByosRestore = Message.Config.IsByosRestore
//        //                });
//        //            RehydrateTask = new RehydrateTask(taskContext, service);
//        //            if (RehydrateTask.NeedRehydrate())
//        //            {
//        //                UpdataRehydrateState(JobState.ReHydrating);
//        //                var result = RehydrateTask.Execute();
//        //                if (result.JobState == JobState.Finished)
//        //                {
//        //                    Message.ConfigForMedia.IsRestoreFromArchiveTier = true;
//        //                }
//        //                UpdataRehydrateState(JobState.Restoring);
//        //            }
//        //        }
//        //    }
//        //    catch (AveDataArchivedException)
//        //    {
//        //        throw;
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        logger.Error("StorageArchiveService initialized error:{0}", e);
//        //    }
//        //}
//        public  void Run()
//        {
//            logger.Info("Restore worker start running.");
//            try
//            {
//                Rehydrate();

//                RestoreService.Open(Config);
//                //(Report as RestoreReport).TotalCount = (RestoreService as ExchangeRestoreService).MaxItemNum;

//                using (var handler = new RestoreDataHandler())//stodo WorkerServiceLocator.GetRequiredService<IRestoreDataHandlerBase>())
//                {
//                    //handler.Start(Config, RestoreService);

//                    var executor = new RestoreExecutorBatch();
//                    //var executor = Message.Config.RestoreType switch
//                    //{
//                    //    EORestoreType.InPlace or
//                    //    EORestoreType.OutOfPlace => WorkerServiceLocator.GetService<IRestoreExecutor>(s => s.IsType(typeof(RestoreExecutorBatch))),
//                    //    //EORestoreType.ToStorage or
//                    //    //_ => WorkerServiceLocator.GetService<IRestoreExecutor>(s => s.IsType(typeof(RestoreToStorageExecutorBatch))),
//                    //};
//                    executor.Execute();
//                }
//            }
//            catch (Exception ex) when (WriteLog(ex)) { }
//            //catch (AveDataArchivedException)
//            //{
//            //    jobStatus = JobState.Failed;
//            //    errorMessage = Message.Config.RestoreType == EORestoreType.ToStorage
//            //        ? RestoreConstants.EXPORT_NOT_SUPPORT_ARCHIVED_DATA
//            //        : RestoreConstants.DATA_ARCHIVED_EXCEPTION;
//            //    AddFailedReport(errorMessage);
//            //}
//            catch (Exception ex)
//            {
//                jobStatus = JobStatus.Failed;
//                errorMessage = ex.Message;
//                AddFailedReport(errorMessage);

//                //if (ex.GetType().ToString().Equals("System.Data.SQLite.SQLiteException", StringComparison.OrdinalIgnoreCase)
//                //    && ex.Message.IndexOf("database disk image is malformed", StringComparison.OrdinalIgnoreCase) >= 0)
//                //{
//                //    var reportService = new MalformedIndexDBReportService(GCommonRoleConfiguration.JobLogStorageXri);
//                //    reportService.RecordMalformedIndexDB(this.Config.TenantGroupOwner, Config.JobId, RestoreConfig.TenantGroupId);
//                //}
//            }
//            finally
//            {
//                //RehydrateTask?.Dispose();
//                UpdateRestoreDataStastics(RestoreConfig.TenantGroupId, Config.JobId, Message.JobId);
//                RestoreService.Close(errorMessage);
//                RemoveCacheIndexDB();
//                Report.Finish(jobStatus, errorMessage);
//                UpdateSubJobStastics(RestoreConfig.TenantGroupId, Config.JobId);
//            }
//        }

//        private static void UpdateSubJobStastics(string tenantId, string subJobId)
//        {
//            try
//            {
//                //JobManagement jobManagement = new JobManagement(IdentityManager.IdentityContent);
//                //var subJobInfo = jobManagement.GetSubJobForAuditor(subJobId);
//                //var office365RequestSummary = new Microsoft365RequestSummary
//                //{
//                //    RequestNumber = EWSMonitor.Instance.RequestNumber,
//                //    ErrorResponseNumber = EWSMonitor.Instance.ErrorResponseNumber - EWSMonitor.Instance.ThrottledResponseNumber,
//                //    ThrottledResponseNumber = EWSMonitor.Instance.ThrottledResponseNumber,
//                //    ThrottlingBlockedTime = new TimeSpan(),
//                //    TokenRequestNumber = AuthMonitor.Instance.TotalAuthNumber
//                //};
//                //var entity = new SubJobStasticEntity().WithMetrics(tenantId, subJobInfo, office365RequestSummary);
//                //new SubJobStasticService(GCommonRoleConfiguration.MetricsStorageAccount?.ConnectionString, subJobId).CommitAsync(entity).ExecuteAsyncTask();
//            }
//            catch (Exception ex)
//            {
//                logger.Warn($"An error occurred while to update subjob stastics. Reason: {ex.ToString()}.");
//            }
//        }
//        private static void UpdateRestoreDataStastics(string tenantId, string subJobId, string jobId)
//        {
//            try
//            {
//                //new RestoreJobMetricService(GCommonRoleConfiguration.MetricsStorageAccount?.ConnectionString)
//                //    .CommitAsync(RestoreJobRecordEntity.CopyFrom(RestoreDataMonitor.Instance, tenantId, subJobId, jobId)).ExecuteAsyncTask();
//            }
//            catch (Exception ex)
//            {
//                logger.Warn($"An error occurred while recording the restore data time distribution. Reason: {ex.ToString()}.");
//            }
//        }
//        private void Rehydrate()
//        {
//            //logger.Info("Rehydrate: Start to rehydrate.");
//            //TryRehydrate();
//            //logger.Info("Rehydrate: End to rehydrate.");
//            //ResetVolume();
//            //logger.Info("Rehydrate: End to reset volume.");
//        }

//        private void ResetVolume()
//        {
//            //var volumeParam = new VolumeParameter(this.Message.ConfigForMedia);
//            //var generator = VolumeGeneratorFactory.GetVolumeGenerator(VolumeType.ExchangeBackup);
//            //this.ExchangeRestoreJob.DataVolume = generator.GenerateIndexVolume(volumeParam);
//            //this.ExchangeRestoreJob.IndexVolume = generator.GenerateIndexVolume(volumeParam);
//            //this.Config.exchangeRestoreJob = this.ExchangeRestoreJob;
//        }

//        private void RemoveCacheIndexDB()
//        {
//            this.Config.exchangeRestoreJob.ExchangeTreeRoot.Children[0].Children.SelectMany(group => group.Children).ForEach(item =>
//            {
//                var hashCode = RestoreCommonUtility.GetAgentIndexName($"{item.EmailAddress}(GroupInfo)", item.MailboxType, true);
//                string currentIndexFolder = Path.Combine(cachePath, this.ExchangeRestoreJob.IndexVolume);
//                logger.Info("Start delete temp file in {0}", currentIndexFolder);
//                if (!Directory.Exists(currentIndexFolder)) return;
//                try
//                {
//                    string[] filePaths = Directory.GetFiles(currentIndexFolder);
//                    foreach (string filePath in filePaths)
//                    {
//                        if (filePath.EndsWith($"{hashCode}.db", StringComparison.OrdinalIgnoreCase) ||
//                            filePath.EndsWith($"{hashCode}.db.properties", StringComparison.OrdinalIgnoreCase))
//                        {
//                            logger.Info("Temp data file will be deleted. FilePath: {0}. ", filePath);
//                            DeleteTempFile(filePath);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    logger.Warn("An error occurred while to clear folder temp files. FolderPath: {0}. Reason: {1}. " + currentIndexFolder, ex.ToString());
//                }
//            });
//        }

//        private static void DeleteTempFile(string filePath)
//        {
//            try
//            {
//                if (File.Exists(filePath))
//                    File.Delete(filePath);
//            }
//            catch (Exception ex)
//            {
//                logger.Warn("An error occurred while to delete temp file. FilePath: {0}. Reason: {1}. ", filePath, ex.ToString());
//            }
//        }

//        #region ==========Init=============

//        private void InitRestoreReport()
//        {
//            //Report.Init(Config);
//            //Report.StartKeepAliveThread();
//        }

//        private void InitGlobalSetting(ERMessage message)
//        {
//            InitIdentityManager(message);
//        }

//        private void InitMediaSetting(ERMessage office365GroupMessage)
//        {
//            InitMediaExchangeRestoreJobInfo(office365GroupMessage);
//            //this.Config.CacheManager = this.CacheManager;
//            this.Config.exchangeRestoreJob = this.ExchangeRestoreJob;
//        }

//        private void InitMediaExchangeRestoreJobInfo(ERMessage office365GroupMessage)
//        {
//            this.ExchangeRestoreJob = new ExchangeRestoreJob(office365GroupMessage.ConfigForMedia);
//            this.ExchangeRestoreJob.CacheSetting = new CacheSettingDto() { Extension = new CacheSettingExtension() };
//            this.ExchangeRestoreJob.CacheSetting.Extension.Path = new List<PathMap>();
//            this.ExchangeRestoreJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = new DiskInfoDto() });
//            this.ExchangeRestoreJob.CacheSetting.Extension.Path[0].DiskInfo.Path = cachePath;
//            //this.ExchangeRestoreJob.IndexEncryptionInfoWrapper.PutEncryptionInfo();
//        }


//        private void InitIdentityManager(ERMessage message)
//        {
//            IdentityManager.IdentityMode = IdentityMode.Process;
//            //IdentityManager.IdentityType = MicroKernelConstant.IdentityTypeGroupId;
//            IdentityManager.IdentityContent = message.TenantGroupId;
//        }

//        private void EnableEWSNonitor()
//        {
//            EWSMonitor.Mode = (EWSMonitorMode)Config.EWSMonitorMode;
//            EWSMonitor.IntervalInSecond = Config.EWSMonitorInterval;
//            logger.Info("Set EWSMonitorMode to {0}, set EWSMonitorIntervalInSecond to {1}.", EWSMonitor.Mode, EWSMonitor.IntervalInSecond);
//        }

//        #endregion

//        private void UpdataRehydrateState(JobState jobState)
//        {
//            //(Report as RestoreReport).UpdateRehydrate(this.Config.JobId, jobState);
//        }

//        private void AddFailedReport(string errorMsg)
//        {
//            foreach (var mailbox in Config.exchangeRestoreJob.ExchangeTreeRoot.Children[0].Children.SelectMany(group => group.Children))
//            {
//                Report.AddReport(new ReportDto
//                {
//                    Name = mailbox.EmailAddress,
//                    Title = mailbox.EmailAddress,
//                    Option = RestoreOption.NewCreated.GetEnumDescription(),
//                    EntityType = JobReportDetailEntityType.Objects,
//                    Size = 0,
//                    Path = mailbox.EmailAddress,
//                    SourcePath = mailbox.EmailAddress,
//                    //Type = Config.IsMicrosoftTeams ? ReportNodeHeader.Team : ReportNodeHeader.Group,
//                    Status = ReportStatus.Failed,
//                    ErrorMessage = errorMsg
//                });
//            }
//        }

//        private bool WriteLog(Exception ex)
//        {
//            logger.Error("Restore worker finished with exception: {0}.", ex);
//            return false;
//        }

//        public void Initailize(ERMessage message)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}