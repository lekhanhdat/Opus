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
//using AvePoint.RA.CommonUtil;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Utility;
using AvePoint.Item.Restore;
using AvePoint.RA.ArchiverMigration;
using AvePoint.RA.ArchiverMigration.JobStage;
using AvePoint.RA.ArtificialIntelligence.MachineLearningTraining;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMEmail;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAArtificialIntelligence.MachineLearningReview;
using AvePoint.RA.RACloudFS.Report;
using AvePoint.RA.RAExchange.ApplySetting;
using AvePoint.RA.RAExchange.Disposal;
using AvePoint.RA.RAExchange.EnforceRetention;
using AvePoint.RA.RAExchange.Report;
using AvePoint.RA.RAExchange.RMCollectionData;
using AvePoint.RA.RAPhysical;
using AvePoint.RA.RAPhysical.ConfiguePermission.Interface;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.ExplorerMove;
using AvePoint.RA.RAPhysical.ExplorerTimer;
using AvePoint.RA.RAPhysical.Export;
using AvePoint.RA.RAPhysical.Import;
using AvePoint.RA.RAPhysical.Loan;
using AvePoint.RA.RAPhysical.PickStatus;
using AvePoint.RA.RAPhysical.Report.Interface;
using AvePoint.RA.RAPhysical.Template;
using AvePoint.RA.RASharePointOnPrem.Report;
using AvePoint.RA.ScheduleJob.JPMC;
using AvePoint.RA.ScheduleJob.Teams;
using AvePoint.RA.Service.JobManagement.Handler;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.CustomizeConnector.Timer;
using AvePoint.RA.Service.Services.DataIngestion.Processor;
using AvePoint.RA.Service.Services.DeleteArchivedData;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Runner;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Analyzer.Runner;
using AvePoint.RA.Service.Services.Discovery.Google.Work;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.Runner;
using AvePoint.RA.Service.Services.Discovery.Office365.Work;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Analyzer;
using AvePoint.RA.Service.Services.SharePoint.Report;
using AvePoint.RA.Service.Services.Tenant.Notification;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using AvePoint.RA.SharePoint.ActionOnly.SPActionOnly;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.AdjustStorageSize;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.ConvertStub;
using AvePoint.RA.SharePoint.DeclaredRecordMigration;
using AvePoint.RA.SharePoint.DeleteArchivedSCJob;
using AvePoint.RA.SharePoint.DeleteArchivedSCJob;
using AvePoint.RA.SharePoint.DeleteArchivedSCJob;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.Discover.Base;
using AvePoint.RA.SharePoint.Discovery.Import;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.ExplorerSync;
using AvePoint.RA.SharePoint.ExportDecryptionIndexDB;
using AvePoint.RA.SharePoint.ExportJob;
using AvePoint.RA.SharePoint.FullTetIndexSiteCollectionlist;
using AvePoint.RA.SharePoint.MoveDataTier;
using AvePoint.RA.SharePoint.OneDrive.Discover;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using AvePoint.RA.SharePoint.OneDrive.EnforceRetention;
using AvePoint.RA.SharePoint.OneDriveExplorerSync;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting;
using AvePoint.RA.SharePoint.RestoreJob;
using AvePoint.RA.SharePoint.RMCustomization4JPMC;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.RA.SharePoint.RMLocationManagement;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using AvePoint.RA.SharePoint.SiteCollectionMapping;
using AvePoint.RA.SharePoint.StubDisposal;
using AvePoint.RA.SharePoint.Teams.ColumnSetting;
using AvePoint.RA.SharePoint.Teams.ColumnSetting.ImportTeamsSetting;
using AvePoint.RA.SharePoint.Teams.Discover;
using AvePoint.RA.SharePoint.Teams.EnforceRetention;
using AvePoint.RA.SharePoint.Teams.RecordsUniqueIdSetting;
using AvePoint.RA.SharePoint.Teams.ReportData;
using AvePoint.RA.SharePoint.Teams.Synchronization;
using AvePoint.RA.SharePoint.Upgrade;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Discover.Cache.SPOStorage.Base;
using Newtonsoft.Json;
using RAArchiverMaintenance;
using RAArchiverMaintenance.Deduplication;
using RAArchiverMaintenance.RebuildIndex;
using RAArchiverMaintenance.Retention.GoogleDrive;
using RAArtificialIntelligence.MachineLearningReport;
using RABox.Report;
using RACloudFS.FSDashBoard;
using RACloudFS.FSFolderJob;
using RACloudFS.FSImportJob;
using RACloudFS.Report;
using RADashboard;
using RADownloadCenter.ArchiverSiteExport;
using RADownloadCenter.IndexExport;
using RADownloadCenter.JobReportExport;
using RADownloadCenter.ReportExport;
using RADownloadCentre.DeduplicationExport;
using RADownloadCentre.DiscoverySpecificSitesExport;
using RADownloadCentre.MigrationSettingResultExport;
using RADownloadCentre.PickMoveExport;
using RADownloadCentre.RCCReport;
using RADownloadCentre.ReturnLoanHistoryExport;
using RADownloadCentre.SettingExport;
using RADownloadCentre.SiteCollectionMapping;
using RADownloadCentre.SiteWhitelist;
using RAFileSystem.FSActions;
using RAGlobalSearch;
using RAGlobalSearch.Export;
using RAGoogle.JobProcess;
using RAGoogle.Report;
using RAGoogle.Report.RestoreReport;
using RAGoogle.Restore;
using RAManualApproval;
using RAManualApproval.BulkAction;
using RAManualApproval.DeleteInvalidRecords;
using RAManualApproval.Discover.ExportDiscoveryProfile;
using RAManualApproval.EmailSchedule;
using RAManualApproval.ExportAction;
using RAManualApproval.ExportAction.ExportTermAndRule;
using RAManualApproval.ExportAction.History;
using RAManualApproval.FolderView;
using RAManualApproval.ImportAction;
using RAManualApproval.Upgrade;
using RAMultiGeo.SyncCommonData.MainDC;
using RAMultiGeo.SyncCommonData.MainDC;
using RAMultiGeo.SyncCommonData.OtherDCs;
using RAMultiGeo.SyncCommonData.OtherDCs;
using RAReportCenter.ClientAuditReport;
using RAReportCenter.CreateAndDestryoedReport;
using RAReportCenter.DisposalReport;
using RAReportCenter.TermUsageReport;
using RATeams.Discover;
using RATeams.Discover.Base;
using RATeams.Upgrade;
using RMSynchronize.SyncNodeFromAOS;
using RMSynchronize.SyncStorage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;


namespace AvePoint.RA.ScheduleJob
{
    public class RMJobProcessor
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMJobProcessor));
        private static IJobMonitorService mJobService;
        protected static IJobMonitorService JobService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }

        protected static IUserService UserService = PlatformWindsorManager.GetService<IUserService>();

        private static IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private static IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static List<JobType> AOSPJobList = new List<JobType>()
        {
            JobType.AOSPRestore,
            JobType.DiscoveryAOSPJob,
            JobType.DiscoveryAOSPOptimization,
            JobType.DiscoveryAOSPOptimizationCalculate,
        };

        /// <summary>
        /// [RECO-17513]此方法不能异步执行，否则会导致TenantLocalValue赋值存在问题
        /// </summary>
        private static void InitCurrentUserId()
        {
            var userEmail = TenantLocalValue.LogonUserEmail;
            if (string.IsNullOrEmpty(userEmail) || userEmail == "RM_TS_RunSchedule")
            {
                var admin = RetryUtility.RetryAlways(
                    () => UserService.GetApplicationAdminsAsync().GetAwaiter().GetResult()?.FirstOrDefault(),
                    3
                );
                
                if (admin != null)
                {
                    logger.Info($"Application admin is: {admin.UserId}");
                    TenantLocalValue.LogonUserId = admin.UserId;
                    return;
                }
                else
                {
                    throw new Exception("Can't get application admin user.");
                }
            }

            var user = UserService.GetUserByNameAsync(userEmail).GetAwaiter().GetResult();
            if(user == null)
            {
                throw new Exception($"Can't get user by {userEmail}");
            }
            TenantLocalValue.LogonUserId = user.UserId;
        }

        public static async Task HandleMessageAsync(string[] args, string currentUser, JobQueueMessage jobQueueMsg)
        {
            if (args == null || args.Length == 0)
            {
                throw new Exception("None job arguments. ");
            }

            JobType jobType;
            if (!Enum.TryParse<JobType>(args[0], out jobType))
            {
                logger.Error($"Cannot convert {args[0]} to JobType");
                throw new Exception("Not support job type.");
            }

            if (!AOSPJobList.Contains(jobType))
            {
                InitCurrentUserId();
            }
            string jobId = string.Empty;
            string jobRunBy = string.Empty;
            string profileId;
            //RMJobMessage msg = null;
            MemoryDataCount.MemoryLimitCount = KeyValueService.GetConvertFolderItemToDBLimitCount();
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveSPAssemblyEventHandler);
            }
            catch (Exception xx)
            {
                logger.Warn("Assembly error,error detail {0}", xx.ToString());
            }
            if (Enum.TryParse<JobType>(args[0], out jobType))
            {
                var commandLineArgs = string.Join(",", args.ToList());
                var tenantRegisterEmail = args[args.Length - 1];
                logger.Info($"Job infos: {commandLineArgs.Replace(tenantRegisterEmail, tenantRegisterEmail.LogBase64())}");
                TenantLocalValue.LogonUserEmail = tenantRegisterEmail;

                if (args.Length >= 2 && jobType != JobType.TermDeletion)
                {
                    if (jobType == JobType.PhysicalTermSynchronization)
                    {
                        jobRunBy = args[1];
                    }
                    else
                    {
                        jobId = args[1];
                        RALogger.SeparateLogToTenant(TenantLocalValue.LogonGroupId, jobId);
                    }
                }

                switch (jobType)
                {
                    case JobType.MigrateDataCosmosDbForJPMC:
                        logger.Info($"Start JPMC Cosmos DB migration job. JobId: {jobId}.");
                        var migrateProcessor = new MigrateDataCosmosDbForJPMCProcessor(jobId);
                        await migrateProcessor.RunAsync();
                        break;
                    case JobType.DataIngestion:
                        var dataIngestionProcessor = new RMDataIngestionProcessor(jobId);
                        await dataIngestionProcessor.ProcessAsync();
                        break;
                    case JobType.RetiredTermReport:
                    case JobType.OrphanedTermReport:
                    case JobType.BCSTermUsageReport:
                        #region OrphanedTermReport & BCSTermUsageReport & RetiredTermReport
                        bool isRetiredTermReport = false;
                        bool IsOrphanedTermReport = false;
                        try
                        {
                            profileId = args[2];
                            IsOrphanedTermReport = Convert.ToBoolean(args[3]);

                            if (!string.IsNullOrEmpty(args[4]))
                            {
                                isRetiredTermReport = Convert.ToBoolean(args[4]);
                            }
                            logger.Info("Run termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                                jobId, profileId, IsOrphanedTermReport, isRetiredTermReport);
                            RMReportProcessor termUsage = new BCSTermUsageReportProcessor(jobId, profileId, IsOrphanedTermReport, isRetiredTermReport);
                            await termUsage.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.SharePointOnlineDeletionSyncUpgrade:
                        DeletionSyncUpgrader.Process(jobId);
                        break;
                    case JobType.SendEmailJob:
                        var executor = new RMSendEmailExecutor(jobId, args[2]);
                        executor.Run().GetAwaiter().GetResult();
                        break;
                    case JobType.ManualFileSystemUpgrade:
                        await ManualApprovalForFSUpgrader.Run(jobId);
                        break;
                    case JobType.DiscoveryJob:
                        var runner = new RMDiscoveryOffice365AnalysisJobRunner(jobId);
                        await runner.RunAsync();
                        break;
                    case JobType.DiscoveryOptimizationCalculate:
                        var calculateRunner = new RMDiscoveryOffice365OptimizationCalculateJobRunner(jobId, new Guid(args[2]), new Guid(args[3]));
                        await calculateRunner.RunAsync();
                        break;
                    case JobType.DiscoveryAOSPOptimizationCalculate:
                        var aospCalulateRunner = new RMDiscoveryAOSPOptimizationCalculateJobRunner(jobId, new Guid(args[2]), new Guid(args[3]));
                        await aospCalulateRunner.RunAsync();
                        break;
                    case JobType.DiscoveryReCalculate:
                        var recalculateRunner = new RMDiscoveryOffice365CalculateJobRunner(jobId);
                        await recalculateRunner.RunAsync();
                        break;
                    case JobType.CosmosDBDirtyDataDeleteUpgrade:
                        RMCosmosDBDirtyDataDeleteUpgrader.Process(jobId);
                        break;
                    case JobType.ItemsFilesDueDisposal:
                        #region ItemsFilesDueDisposal
                        try
                        {
                            profileId = args[2];
                            RMReportProcessor dueDisposal = new DueDisposalReportProcessor(jobId,profileId);
                            await dueDisposal.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Fail run tem file due disposal job, error:{ex}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.SharePointScheduleSetting:
                        #region SharePointScheduleSetting
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            RMSettingProcessor setting = new RMSettingProcessor(jobId, jobQueueMsg);
                            //setting.ApplySharePointSetting();
                            await setting.ApplySPSettingAsync(true);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.ApplySharePointSettings:
                        #region SharePointScheduleSetting
                        try
                        {
                            RMSettingProcessor setting = new RMSettingProcessor(jobId, jobQueueMsg);
                            await setting.ApplySPSettingAsync(false);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.TermSynchronization:
                        #region TermSynchronization
                        try
                        {
                            using (RMSyncTermProcessor syncProcessor = new RMSyncTermProcessor(jobId, JobType.TermSynchronization, bool.Parse(args[2])))
                            {
                                await syncProcessor.SyncTermAsync();
                            }
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.PhysicalFolderSynchronization:
                        #region PhysicalFolderSynchronization
                        jobRunBy = args[2];
                        List<Guid> usedTermIds = new List<Guid>();
                        //特殊类型job
                        //LoggerInitializer.Initialize();
                        RALogger.SeparateLogToTenant(TenantLocalValue.LogonGroupId, jobId);

                        List<string> runningPTermSyncJobs = JobService.GetRunningJobs(JobType.PhysicalTermSynchronization);
                        List<string> runningPFolderSyncJobs = JobService.GetRunningJobs(JobType.PhysicalFolderSynchronization);
                        if (runningPTermSyncJobs.Count > 0 || runningPFolderSyncJobs.Any(j => j != jobId))
                        {
                            JobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                            logger.Info("Skipped this job. A location synchronisation job is already running.");
                        }
                        else
                        {
                            try
                            {
                                using (PerformanceScope scope = new PerformanceScope("Program.RealMain"))
                                {
                                    RMLocationManagement locationProcess = new RMLocationManagement(jobId);
                                    await locationProcess.SyncToPhysicalLibAsync();
                                    usedTermIds = locationProcess.NodeleteTerms;
                                }
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception px)
                            {
                                if (px.Message.Contains("AvePoint.Common.Perm.PermissionControl.Util.PermissionDenyException"))
                                {
                                    JobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SYPFS_Failed");
                                }
                                logger.Error("Physical Folder Synchronization failed {0}", px.ToString());
                                JobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_TS_SS_Summary");
                                throw new Exception("Physical Folder Synchronization failed ");
                            }
                            //finally
                            //{
                            //    RALogger.FinallyUpload(currentUser, jobId);
                            //}

                            string termSyncJobId = "PS" + jobId.Substring(2);
                            try
                            {
                                using (PerformanceScope scope = new PerformanceScope("Program.RealMain"))
                                {
                                    RALogger.SeparateLogToTenant(TenantLocalValue.LogonGroupId, termSyncJobId);
                                    JobService.CreateJobWithJobId(termSyncJobId, JobType.PhysicalTermSynchronization, jobRunBy);
                                    CheckJobStatusUtility.Start(termSyncJobId);
                                    using (RMSyncTermProcessor syncProcessor = new RMSyncTermProcessor(termSyncJobId, JobType.PhysicalTermSynchronization, false)
                                    {
                                        NoDeleteTermids = usedTermIds
                                    })
                                    {
                                        await syncProcessor.SyncTermAsync();
                                    }
                                }
                            }
                            catch (JobStopException)
                            {
                                JobService.UpdateJobStatus(termSyncJobId, JobStatus.Stopped);
                            }
                            catch (Exception sx)
                            {
                                logger.Error("Physical Term Synchronization failed {0}", sx.ToString());
                                JobService.UpdateJobStatus(termSyncJobId, JobStatus.Failed, "RM_TS_SS_Summary");
                                throw new Exception("Physical Term Synchronization failed ");
                                //Term Sync 
                            }
                            //finally
                            //{
                            //    RALogger.FinallyUpload(currentUser, termSyncJobId);
                            //}
                        }
                        #endregion
                        break;
                    case JobType.ImportPhysicalRecords:
                        #region ImportPhysicalRecords
                        try
                        {
                            var msg = new RMImportJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                PhysicalRecordsCSVPath = args[2],
                                SharePointSettingID = Convert.ToInt32(args[3]),
                                GlobalTimeZoneId = args[4].Replace("_", " "),
                                EnableCustomTimeId = Convert.ToInt32(args[5]),
                            };

                            if (msg.SharePointSettingID == 0)
                            {
                                RMTrimImport trimImport = new RMTrimImport(msg);
                                await trimImport.ImportPhysicalRecordsAsync();
                            }
                            else
                            {
                                //new import logic in prod
                                PhysicalBulkImportWork physicalBulkImport = new PhysicalBulkImportWork(msg);
                                await physicalBulkImport.ImportPhysicalRecordsAsync();
                            }
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.PhysicalBulkInsertExport:
                        #region ImportPhysicalZipRecords
                        try
                        {
                            var msg = new RMImportPhysicalZipRecordMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                PhysicalRecordsCSVPath = args[2],
                                SharePointSettingID = Convert.ToInt32(args[3]),
                                GlobalTimeZoneId = args[4].Replace("_", " ")
                            };

                            // zip方式
                            PhysicalBulkZipImportWork physicalBulkZipImportWork = new PhysicalBulkZipImportWork(msg);
                            await physicalBulkZipImportWork.ImportPhysicalRecordsAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.PhysicalBulkEditExport:
                        #region ExportPhysicalZipRecords
                        try
                        {
                            var msg = new RMExportPhysicalZipRecordMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                TemplateIds = args[2],
                                GlobalTimeZoneId = args[3].Replace("_", " ")
                            };

                            ExportPhysicalZipRecordsWork physicalBulkImport = new ExportPhysicalZipRecordsWork(msg);
                            await physicalBulkImport.ImportPhysicalRecordsAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.TrimRecordsDeletion:
                        #region ImportPhysicalRecords
                        try
                        {
                            var msg = new RMImportJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                PhysicalRecordsCSVPath = args[2],
                                GlobalTimeZoneId = args[3].Replace("_", " ")
                            };

                            RMTrimImportedRecordDeletion trimImport = new RMTrimImportedRecordDeletion(msg);
                            await trimImport.ExecuteAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.ImportRecordsRelated:
                        #region ImportRecordsRelated
                        try
                        {
                            var msg = new RMImportJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                SharePointSettingID = Convert.ToInt32(args[2]),
                                GlobalTimeZoneId = args[3].Replace("_", " ")
                            };
                            RMTrimRelatedWorker trimImport = new RMTrimRelatedWorker(msg);
                            trimImport.ProcessRelatedMain();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.UpdateLocation:
                        #region UpdateLocation
                        try
                        {
                            var msg = new RMImportJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId
                            };
                            RMLocationManagement lm = new RMLocationManagement(msg);
                            await lm.UpdateLocationAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.TermDeletion:
                        #region TermDeletion
                        try
                        {
                            Guid termUniqueId = new Guid(args[1]);
                            RMLocationManagement rl = new RMLocationManagement();
                            if (!(await rl.CheckLocationCanBeDeleteAsync(termUniqueId)))
                            {
                                throw new Exception("Can't delete physical term");
                            }
                        }
                        catch (Exception tx)
                        {
                            logger.Error("can't delete term {0}", tx.ToString());
                            throw new Exception("Can't delete physical term");
                        }
                        #endregion
                        break;
                    case JobType.AvailableSpaceReport:
                        #region AvailableSpaceReport
                        {
                            try
                            {
                                profileId = args[2];
                                IPRAvailableSpaceReportService service = (IPRAvailableSpaceReportService)PlatformWindsorManager.GetService(typeof(IPRAvailableSpaceReportService));
                                await service.RunAvailableSpaceReportJobAsync(jobId, profileId);
                            }
                            catch (JobStopException ex)
                            {
                                throw ex;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        #endregion
                        break;
                    case JobType.CreateAndDestroyedFileReport:
                        #region CreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            RMReportProcessor cdfr = new CreationAndDestroyedFileReportProcessor(msg);
                            await cdfr.RunReportJobAsync();
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.TeamsCreateAndDestroyedFileReport:
                        #region TeamsCreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            RMTeamsReportProcessor cdfr = new RMTeamsCreationAndDestroyedFileReportProcessor(msg);
                            await cdfr.RunAsync();
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.RestoreReport:
                        #region RestoreReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                StartTime = Convert.ToDateTime(args[3], dtFormat),
                                EndTime = Convert.ToDateTime(args[4], dtFormat),
                                GlobalTimeZoneId = args[5]
                            };
                            logger.Info(msg.ToString());
                            RMReportProcessor cdfr = new RestoreReportProcessor(msg);
                            await cdfr.RunReportJobAsync();
                        }
                        catch (Exception e)
                        {
                            logger.Error($@"Fail run restore report, ex:{e}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.OneDriverRestoreReport:
                        #region OneDriverRestoreReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                StartTime = Convert.ToDateTime(args[3], dtFormat),
                                EndTime = Convert.ToDateTime(args[4], dtFormat),
                                GlobalTimeZoneId = args[5]
                            };
                            logger.Info(msg.ToString());
                            RMOneDriveReportProcessor cdfr = new OneDriveRestoreReportProcessor(msg);
                            await cdfr.RunReportJobAsync();
                        }
                        catch (Exception e)
                        {
                            logger.Error($@"Fail run onedirve restore report, ex:{e}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.ImportTermStructure:
                        #region ImportTermStructure
                        string extension;
                        string path;
                        bool isControlPlus;
                        try
                        {
                            jobId = args[1];
                            extension = args[2];
                            path = args[3];
                            isControlPlus = bool.Parse(args[4]);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        try
                        {
                            ImportTermProcessor importTerm = new ImportTermProcessor(jobId, jobType, extension, path, isControlPlus);
                            await importTerm.RunJobAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        break;
                    #endregion
                    case JobType.ExportTermStructure:
                        #region ExportTermStructure
                        try
                        {
                            ExportTermAndRuleExportProcessor exportTerm = new ExportTermAndRuleExportProcessor();
                            await exportTerm.RunAsync(args[1]);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    #endregion
                    case JobType.DiscoveryExportO365Profile:
                        try
                        {
                            ExportDiscoveryProfileProcessor exportProcessor  = new ExportDiscoveryProfileProcessor(jobId, args[2], args[3], args[4], bool.Parse(args[5]));
                            await exportProcessor.RunAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.ImportSCMapping:
                        ImportSiteCollectionMappingProcessor importSCMapProcessor = new ImportSiteCollectionMappingProcessor(args[1], args[2]);
                        importSCMapProcessor.RunJob();
                        break;
                    case JobType.ExportSCMapping:
                        ExportSiteCollectionMappingProcessor exportSCMapProcessor = new ExportSiteCollectionMappingProcessor(jobId);
                        await exportSCMapProcessor.RunAsync();
                        break;
                    case JobType.ImportSCWhitelist:
                        new ImportFullTextIndexSiteCollectionlistProcessor(args[1], args[2], JobType.ImportSCWhitelist).RunJob();
                        break;
                    case JobType.ExportSCWhitelist:
                        var exportSCWhitelistProcessor = new ExportFullTextIndexSiteCollectionlistProcessor(jobId, JobType.ExportSCWhitelist);
                        await exportSCWhitelistProcessor.RunAsync();
                        break;
                    case JobType.DiscoveryExportExcludeSCList:
                        var exportExcludeSCListProcessor = new DiscoverySpecificSiteExportProcessor(jobId, JobType.DiscoveryExportExcludeSCList);
                        await exportExcludeSCListProcessor.RunAsync();
                        break;
                    case JobType.DiscoveryImportExcludeSCList:
                        await new ImportDiscoverySpecifySitesProccessor(args[1], args[2], JobType.DiscoveryImportExcludeSCList).RunJob();
                        break;
                    case JobType.ExportSCBlacklist:
                        var exportSCBlacklistProcessor = new ExportFullTextIndexSiteCollectionlistProcessor(jobId, JobType.ExportSCBlacklist);
                        await exportSCBlacklistProcessor.RunAsync();
                        break;
                    case JobType.ImportSCBlacklist:
                        new ImportFullTextIndexSiteCollectionlistProcessor(args[1], args[2], JobType.ImportSCBlacklist).RunJob();
                        break;
                    case JobType.ImportSPSetting:
                        #region ImportSPSetting
                        try
                        {
                            var jobMsg = new RMImportSPSettingJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                CSVPath = args[3],
                            };
                            await new RMImportSPSettingProcessor(jobMsg).ImportCustomSettingAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    #endregion
                    case JobType.ImportFSSetting:
                        try
                        {
                            var jobMsg = new RMImportSPSettingJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                CSVPath = args[3],
                            };
                            //var enableJPMCFeature = KeyValueDao.IsEnableJPMCFileSystemFeature();
                            //if (enableJPMCFeature)
                            //{
                            //    logger.Info("JPMC File System feature is enabled, run JPMC import processor.");
                            //    await new FSImportSettingProcessorJPMC(jobMsg).RunAsync();
                            //}
                            //else
                            //{
                            await new FSImportSettingProcessor(jobMsg).RunAsync();
                            //}
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    case JobType.ManualApprovalLocationTest:
                        #region ManualApprovalLocationTest
                        //ManualApprovalStoreLocation location;
                        //try
                        //{
                        //    location = JsonConvert.DeserializeObject<ManualApprovalStoreLocation>(args[2]);
                        //    JobService.UpdateExcuteResult(jobId, ManualApprovalProcessor.ValidationLocation(location));
                        //}
                        //catch (Exception ex)
                        //{
                        //    JobService.UpdateExcuteResult(jobId, ex.Message);
                        //    throw new Exception("ManualApprovalLocationTest failed. error message:" + ex.ToString());
                        //}
                        break;
                    #endregion
                    case JobType.ManualApprovalTimer:
                        #region ManualApprovalTimer
                        try
                        {
                            //new ManualApprovalProcessorTimer(jobId).RunJob();
                            await ManualApprovalProcessor.RunAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            if ((ex.InnerException ?? ex).Message == "RM_MA_NotFound_CustomApp")
                            {
                                logger.Warn($"SetRecordIsAutoApproval ERROR, RM_MA_NotFound_CustomApp");
                                ManualApprovalJobManager.SetJobFailed("RM_MA_NotFound_CustomApp");
                            }
                            // throw ex;
                        }
                        break;
                    #endregion
                    case JobType.ManualApprovalOrRejectJob:
                        try
                        {
                            await ManualApprovalBulkActionProcessor.RunAsync(jobId, args[2],int.Parse(args[3]), bool.Parse(args[4]));
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        break;
                    case JobType.ManualFolderViewActions:
                        try
                        {
                            var processor = new ManualFolderViewActionProcessor(jobId, args[2]);
                            await processor.RunAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        break;
                    case JobType.UniqueIDSettingFullSchedule:
                    case JobType.UniqueIDSettingIncrementalSchedule:
                        try 
                        {
                            SPUniqueIdSettingWorker uniqueIdSettingWorker = new SPUniqueIdSettingWorker(jobId, null);
                            await uniqueIdSettingWorker.ConfigUniqueIDSettingAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    case JobType.ExportToLocation:
                        await new JobExportToLocationDisposalProcessor().RunJobAsync(jobId, args[2], args[4]);
                        break;
                    case JobType.EnforceRetention:
                        try
                        {
                            RMEnforceRetentionProcessor processor = new RMEnforceRetentionProcessor(jobId);
                            await processor.RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.DataSynchronisation:
                        try 
                        {
                            await new RMSPExplorerProcessor(jobId).RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    case JobType.CollectionDataFull:
                        await new SharePoint.ExplorerSyncNew.Report.RMSPDashboardCalculator(jobId).WorkAsync();
                        break;
                    //case JobType.ReportAfterDataSync:
                    //    new BoardReportWorker(jobId).RunCollectionNow();

                    //    break;
                    case JobType.RecordsExplorerMove:
                        await new RMExplorerMoveProcessor().RunNowAsync(jobId);
                        break;
                    case JobType.EXOApplySetting:
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            await new RMEXOApplySettingProcesser().RunNowAsync(jobId);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.EXODataSynchronisation:
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            await new RMEXOSyncDataProcesser().RunNowAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.EXOTermUsageReport:
                    case JobType.EXORetiredTermUsageReport:
                    case JobType.EXOOrphanedTermUsageReport:
                        #region EXOOrphanedTermReport & EXOBCSTermUsageReport & EXORetiredTermReport
                        bool isEXORetiredTermReport = false;
                        bool IsEXOOrphanedTermReport = false;
                        try
                        {
                            profileId = args[2];
                            if (!string.IsNullOrEmpty(args[3]))
                            {
                                IsEXOOrphanedTermReport = Convert.ToBoolean(args[3]);
                            }
                            if (!string.IsNullOrEmpty(args[4]))
                            {
                                isEXORetiredTermReport = Convert.ToBoolean(args[4]);
                            }
                            logger.Info("Run EXO termusage report jobInfo JobId: {0} ProfileId: {1} IsOrphanedTermReport: {2} IsRetiredTerm Report: {3}.",
                                jobId, profileId, IsEXOOrphanedTermReport, isEXORetiredTermReport);
                            EXOReportProcessor exoTermUsage = new EXOTermUsageReportProcessor(jobId, profileId, IsEXOOrphanedTermReport, isEXORetiredTermReport);
                            await exoTermUsage.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.EXOCreateAndDestroyedFileReport:
                        #region EXOCreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            EXOReportProcessor exoCdfr = new EXOCreationAndDestroyedFileReportProcessor(msg);
                            await exoCdfr.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.EXOItemsFilesDueDisposalReport:
                        #region EXOItemsFilesDueDisposalReport
                        try
                        {
                            profileId = args[2];
                            logger.Info("Run EXO item files Due Disposal report jobInfo JobId: {0} ProfileId: {1}.",
                                jobId, profileId);
                            EXODueDisposalReportProcessor exoDueDisposal = new EXODueDisposalReportProcessor(jobId, profileId);
                            await exoDueDisposal.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Invalid job arguments. error message:" + ex.ToString());
                        }
                        #endregion
                        break;
                    case JobType.EXOEnforceRetention:
                        try
                        {
                            EXOEnforceRetentionProcessor exoDeclareProcesser = new EXOEnforceRetentionProcessor(jobId);
                            exoDeclareProcesser.RunNow();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.PhysicalCreateAndDestroyedFileReport:
                        {
                            try
                            {
                                DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                                {
                                    ShortDatePattern = "yyyy/MM/dd"
                                };
                                var msg = new RMCreationJobMessage()
                                {
                                    JobType = jobType,
                                    JobID = jobId,
                                    ProfileId = args[2],
                                    SelectCreated = Convert.ToBoolean(args[3]),
                                    SelectDestroyed = Convert.ToBoolean(args[4]),
                                    StartTime = Convert.ToDateTime(args[5], dtFormat),
                                    EndTime = Convert.ToDateTime(args[6], dtFormat),
                                    GlobalTimeZoneId = args[7]
                                };
                                logger.Info(msg.ToString());
                                IPRCreationAndDestroyedFileReportService service = (IPRCreationAndDestroyedFileReportService)PlatformWindsorManager.GetService(typeof(IPRCreationAndDestroyedFileReportService));
                                await service.RunPRCreationAndDestroyedFileReportJobAsync(msg);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        break;
                    case JobType.PhysicalItemsFilesDueDisposalReport:
                        {
                            profileId = args[2];
                            var service = (IPRContentDueReportJobService)PlatformWindsorManager.GetService(typeof(IPRContentDueReportJobService));
                            await service.RunReportJobAsync(jobId, profileId);
                        }
                        break;
                    case JobType.PhysicalRecordsDisposal:
                        try
                        {
                            string locationId = args[2];
                            bool skipRemoveContentAndDestroyAction = bool.Parse(args[3]);
                            if (args.Length >= 5)
                            {
                                if (bool.TryParse(args[4], out bool result))
                                {
                                    WrapperConfiguration.IsProcessApprovalDatasOnly = result;
                                }
                            }
                            RMPhysicalDisposalProcessor physicalDisposal = new RMPhysicalDisposalProcessor(jobId, Convert.ToInt32(locationId), skipRemoveContentAndDestroyAction, jobQueueMsg.RunBy);
                            await physicalDisposal.RunNowAsync();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.PhysicalExplorerTimer:
                        try
                        {
                            RMPhysicalExplorerTimerProcessor physicalExplorerTimer = new RMPhysicalExplorerTimerProcessor(jobId);
                            await physicalExplorerTimer.RunNowAsync(jobId);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.ConnectorTimer:
                        try
                        {
                            RMConnectorTimerProcessor connectorTimer = new RMConnectorTimerProcessor(jobId);
                            await connectorTimer.RunNowAsync();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.PhysicalTermUsageReport:
                    case JobType.PhysicalRetiredTermUsageReport:
                    case JobType.PhysicalOrphanedTermUsageReport:
                        #region PhysicalTermUsageReport & PhysicalRetiredTermUsageReport & PhysicalOrphanedTermUsageReport
                        try
                        {
                            var IsPhyOrphanedTermReport = false;
                            var isPhyRetiredTermReport = false;
                            profileId = args[2];
                            if (!string.IsNullOrEmpty(args[3]))
                            {
                                IsPhyOrphanedTermReport = Convert.ToBoolean(args[3]);
                            }
                            if (!string.IsNullOrEmpty(args[4]))
                            {
                                isPhyRetiredTermReport = Convert.ToBoolean(args[4]);
                            }
                            logger.Info("Run Physical termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                                jobId, profileId, IsPhyOrphanedTermReport, isPhyRetiredTermReport);
                            var prTermUsageReportService = (IPRTermUsageReportService)PlatformWindsorManager.GetService(typeof(IPRTermUsageReportService));
                            await prTermUsageReportService.RunReportJobAsync(jobId, profileId, IsPhyOrphanedTermReport, isPhyRetiredTermReport);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.PhysicalExportBarcode:
                        var exportBarcodeDto = new ExportBarcodeDto()
                        {
                            ExportLocationId = args[2],
                            NodeId = new Guid(args[3]),
                            NodeType = (RMNodeType)Enum.Parse(typeof(RMNodeType), args[4]),
                            ExportLocationName = args[5],
                            SuiteId = new Guid(args[6]),
                        };

                        ExportBarcodeProcessor exportBarcodeProcessor = new ExportBarcodeProcessor(jobId, exportBarcodeDto);
                        await exportBarcodeProcessor.RunNowAsync(jobId, exportBarcodeDto);
                        break;
                    case JobType.PhysicalReturnHistoryExport:
                        {
                            var subJobId = args[1];
                            var mainJobId = subJobId.Split('_')[0];
                            ReturnLoanHistoryExportProcessor returnLoanHistoryExportProcessor = new ReturnLoanHistoryExportProcessor(subJobId, mainJobId);
                            await returnLoanHistoryExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.PhysicalMovePickExportJob:
                        {
                            var subJobId = args[1];
                            var mainJobId = subJobId.Split('_')[0];
                            ExportMovePickListProcessor returnLoanHistoryExportProcessor = new ExportMovePickListProcessor(subJobId, mainJobId);
                            await returnLoanHistoryExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.PhysicalMoveDataJob:
                        MoveDataJobProcessor move = new MoveDataJobProcessor(jobId);
                        await move.RunAsync();
                        break;
                    case JobType.ActionOnly:
                        SPActionWorker dactionWorker = new SPActionWorker(jobId);
                        dactionWorker.Run();
                        break;
                    case JobType.PhysicalSetPermission:
                        {
                            var service = (IPhysicalPermissionService)PlatformWindsorManager.GetService(typeof(IPhysicalPermissionService));
                            service.Run(jobId);
                        }
                        break;
                    case JobType.FSDashBoard:
                        FSDashBoardReportWorker worker = new FSDashBoardReportWorker(jobId);
                        await worker.RunCollectionNowAsync();
                        break;
                    case JobType.FSItemsFilesDueDisposal:
                        {
                            profileId = args[2];
                            var fsDueProcessor = new FSContentDueReportService(jobId, profileId);
                            await fsDueProcessor.RunReportJobAsync();
                        }
                        break;
                    case JobType.FSBCSTermUsageReport:
                    case JobType.FSOrphanedTermReport:
                    case JobType.FSRetiredTermReport:
                        var isFSOrphanedTermReport = false;
                        var isFSRetiredTermReport = false;
                        profileId = args[2];
                        if (!string.IsNullOrEmpty(args[3]))
                        {
                            isFSOrphanedTermReport = Convert.ToBoolean(args[3]);
                        }
                        if (!string.IsNullOrEmpty(args[4]))
                        {
                            isFSRetiredTermReport = Convert.ToBoolean(args[4]);
                        }
                        logger.Info("Run Physical termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                            jobId, profileId, isFSOrphanedTermReport, isFSRetiredTermReport);
                        FSBCSTermUsageReportProcessor fsTermUsage = new FSBCSTermUsageReportProcessor(jobId, profileId, isFSOrphanedTermReport, isFSRetiredTermReport);
                        fsTermUsage.RunReportJob();
                        break;
                    case JobType.FSCreateAndDestroyedFileReport:
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            var procesor = new FSCreationAndDestroyedFileReportProcessor();
                            await procesor.RunJobAsync(msg);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.FSFolderChangeTerm:
                        {
                            using (var fsFolderReclassifier = new FSFolderReclassifier(jobId))
                            {
                                await fsFolderReclassifier.RunAsync();
                            }
                        }
                        break;
                    case JobType.FSFolderManageHold:
                        {
                            using (var fsFolderHold = new FSFolderHold(jobId))
                            {
                                await fsFolderHold.RunAsync();
                            }
                        }
                        break;
                    case JobType.SyncSecurityContainer:
                        {
                            await new RMSyncStorageProcessor(jobId, args[2]).RunAsync();
                        }
                        break;
                    case JobType.GlobalSearchAction:
                        {
                            GlobalSearch globalSearch = new GlobalSearch(jobId);
                            await globalSearch.RunAsync();
                        }
                        break;
                    case JobType.ExplorerOfflineSearch:
                        {
                            string userId = args[2];
                            OfflineSearch offlineSearch = new OfflineSearch(jobId, userId);
                            await offlineSearch.RunAsync();
                        }
                        break;
                    case JobType.SyncNodesFromAOS:
                        {
                            await RMSyncNodeProcessor.RunAsync(jobQueueMsg);
                        }
                        break;
                    case JobType.OneDriveDataSynchronisation:
                        try
                        {
                            await new RMOneDriveExplorerProcessor(jobId).RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.OneDriveEnforceRetention:
                        try
                        {
                            RMOneDriveEnforceRetentionProcessor oneDriveProcesser = new RMOneDriveEnforceRetentionProcessor(jobId);
                            await oneDriveProcesser.RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.OneDriveItemsFilesDueDisposalReport:
                        #region OneDriveItemsFilesDueDisposalReport
                        try
                        {
                            profileId = args[2];
                            OneDriveDueDisposalReportProcessor dueDisposal = new OneDriveDueDisposalReportProcessor(jobId, profileId);
                            await dueDisposal.RunReportJobAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.OneDriveTermUsageReport:
                        var isOneDriveOrphanedTermReport = false;
                        var isOneDriveRetiredTermReport = false;
                        profileId = args[2];
                        if (!string.IsNullOrEmpty(args[3]))
                        {
                            isOneDriveOrphanedTermReport = Convert.ToBoolean(args[3]);
                        }
                        if (!string.IsNullOrEmpty(args[4]))
                        {
                            isOneDriveRetiredTermReport = Convert.ToBoolean(args[4]);
                        }
                        logger.Info("Run OneDrive termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                            jobId, profileId, isOneDriveOrphanedTermReport, isOneDriveRetiredTermReport);
                        OneDriveBCSTermUsageReportProcessor oneDriveTermUsage = new OneDriveBCSTermUsageReportProcessor(jobId, profileId, isOneDriveOrphanedTermReport, isOneDriveRetiredTermReport);
                        await oneDriveTermUsage.RunReportJobAsync();
                        break;
                    case JobType.OneDriveCreateAndDestroyedFileReport:
                        #region OneDriveCreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            RMOneDriveReportProcessor cdfr = new OneDriveCreationAndDestroyedReportProcessor(msg);
                            await cdfr.RunReportJobAsync();
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.Dashboard:
                        await DashboardProcessor.RunAsync(jobId, jobQueueMsg.RunBy);
                        break;
                    case JobType.FSMyHubDashboard:
                        FSMyHubDashboardProcessor fSMyHubDashboardProcessor = new FSMyHubDashboardProcessor(jobId, jobQueueMsg.RunBy, jobQueueMsg.Extension);
                        await fSMyHubDashboardProcessor.RunAsync();
                        break;
                    case JobType.TenantUpgrade:
                        await new RMTenantDelayUpgradeProcessor(jobId).RunAsync();
                        break;
                    case JobType.ManualApprovalEmailSchedule:
                        try
                        {
                            await ManualApprovalEmailScheduleProcessor.ProcessAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }                        
                        break;
                    case JobType.ExportSearchResult:
                        ExportSearchResult export = new ExportSearchResult(jobId, args[2]);
                        await export.RunAsync();
                        break;
                    case JobType.PhysicalLoanBox:
                    case JobType.PhysicalReturnBox:
                        LoanBoxJobProcessor loan = new LoanBoxJobProcessor(jobType, jobId);
                        await loan.RunAsync();
                        break;
                    case JobType.ExportHoldRecords:
                        ExportHoldsRecordsProcessor exportHoldRecordsProcessor = new ExportHoldsRecordsProcessor(jobId, args[2]);
                        await exportHoldRecordsProcessor.RunAsync();
                        break;
                    case JobType.ImportHoldRecords:
                        ImportHoldsRecordsProcessor importHoldRecordsProcessor = new ImportHoldsRecordsProcessor(jobId, args[2]);
                        await importHoldRecordsProcessor.RunAsync();
                        break;
                    case JobType.ImportWorkspaceHold:
                        ImportWorkspaceHoldProcessor importWorkspaceHoldProcessor = new ImportWorkspaceHoldProcessor(jobId, args[2]);
                        await importWorkspaceHoldProcessor.RunAsync();
                        break;
                    case JobType.SwitchSecurityProfile:
                        {
                            var service = PlatformWindsorManager.GetService<ISwitchSecurityProfileJobProcessor>();
                            service.Run(jobQueueMsg);
                        }
                        break;
                    case JobType.SPOnPremItemsFilesDueDisposal:
                        profileId = args[2];
                        SPOnPremContentDueReportService spopDueProcessor = new SPOnPremContentDueReportService(jobId, profileId);
                        await spopDueProcessor.RunReportJobAsync();
                        break;
                    case JobType.SPOnPremCreateAndDestroyedFileReport:
                        #region SPOnPremCreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            SPOnPremCreationAndDestroyedFileService spopCreationAndDestroyedFileProcessor = new SPOnPremCreationAndDestroyedFileService(jobId, args[2]);
                            await spopCreationAndDestroyedFileProcessor.RunJobAsync(msg);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;

                    case JobType.SPOnPremBCSTermUsageReport:
                    case JobType.SPOnPremRetiredTermReport:
                    case JobType.SPOnPremOrphanedTermReport:
                        #region SPOnPremBCSTermUsageReport
                        var isSPOnPremOrphanedTermReport = false;
                        var isSPOnPremRetiredTermReport = false;
                        profileId = args[2];
                        if (!string.IsNullOrEmpty(args[3]))
                        {
                            isSPOnPremOrphanedTermReport = Convert.ToBoolean(args[3]);
                        }
                        if (!string.IsNullOrEmpty(args[4]))
                        {
                            isSPOnPremRetiredTermReport = Convert.ToBoolean(args[4]);
                        }
                        logger.Info("Run Physical termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                            jobId, profileId, isSPOnPremOrphanedTermReport, isSPOnPremRetiredTermReport);
                        SPOnPremBCSTermUsageReportService spOnPremTermUsage = new SPOnPremBCSTermUsageReportService(jobId, profileId, isSPOnPremOrphanedTermReport, isSPOnPremRetiredTermReport);
                        spOnPremTermUsage.RunReportJob();
                        #endregion
                        break;
                    case JobType.DisposalReport:
                        DisposalReportProcessor.Process(jobId, Convert.ToInt32(args[2]));
                        break;
                    case JobType.CreateAndDestroyedReport:
                        CreateAndDestryoedReportProcessor.Process(jobId, Convert.ToInt32(args[2]));
                        break;
                    case JobType.TermUsageReport:
                        TermUsageReportProcessor.Process(jobId, Convert.ToInt32(args[2]));
                        break;
                    case JobType.AzureFileShareDataSynchronisation:
                    case JobType.AzureFileShareDataSynchronisationSchedule:
                        try 
                        {
                            await RAAzureFile.DataSync.DataSyncProcessor.ProcessAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.BoxDataSynchronisation:
                    case JobType.BoxDataSynchronisationSchedule:
                        try
                        {
                            await new RABox.DataSyncProcessor(jobType).ProcessAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.BoxRecordsDisposal:
                        try
                        {
                            await new RABox.RuleActionProcessor().ProcessAsync(jobId);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.BoxBCSTermUsageReport:
                    case JobType.BoxOrphanedTermUsageReport:
                    case JobType.BoxRetiredTermUsageReport:
                        var isBoxOrphanedTermReport = false;
                        var isBoxRetiredTermReport = false;
                        profileId = args[2];
                        if (!string.IsNullOrEmpty(args[3]))
                        {
                            isBoxOrphanedTermReport = Convert.ToBoolean(args[3]);
                        }
                        if (!string.IsNullOrEmpty(args[4]))
                        {
                            isBoxRetiredTermReport = Convert.ToBoolean(args[4]);
                        }
                        logger.Info("Run Box termusage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                            jobId, profileId, isBoxOrphanedTermReport, isBoxRetiredTermReport);
                        BoxTermUsageReportProcessor boxTermUsage = new BoxTermUsageReportProcessor(jobId, jobType, isBoxOrphanedTermReport, isBoxRetiredTermReport, profileId);
                        await boxTermUsage.Process();
                        break;
                    case JobType.BoxCreateAndDestroyedFileReport:
                        #region BoxCreateAndDestroyedFileReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            var procesor = new BoxCreationAndDestroyedFileReportProcessor(msg);
                            await procesor.Process();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.BoxItemsFilesDueDisposalReport:
                        {
                            profileId = args[2];
                            var processor = new BoxContentDueReportProcessor(jobId, jobType, profileId);
                            await processor.Process();
                        }
                        break;
                    case JobType.SPOActionAuditReport:
                    case JobType.OneDriveActionAuditReport:
                    case JobType.TeamsActionAuditReport:
                        ClientAuditReportProcessor clientAuditReport = new ClientAuditReportProcessor();
                        await clientAuditReport.ProcessAsync(jobId, args[2]);
                        break;
                    case JobType.EXORecordsDisposal:
                        try
                        {
                            RMEXOEnforceRuleActionProcessor enforceRuleActionProcessor = new RMEXOEnforceRuleActionProcessor();
                            enforceRuleActionProcessor.RunNow(jobId);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.TeamsRecordsDisposal:
                    case JobType.TeamsPreScan:
                    case JobType.TeamsArchiverBackup:
                        {
                            var teamsRuleActionController = new TeamsRuleActionController();
                            var controller = teamsRuleActionController.Build(jobId, jobType);
                            if (jobType != JobType.TeamsRecordsDisposal && teamsRuleActionController.ShouldRunInTeamsController())
                            {
                                await controller.RunAsync();
                            }
                            else
                            {
                                DisposalActivityManagementProcessor disposalActivityManagementProcessorOnTeam = new DisposalActivityManagementProcessor(jobId, jobType);
                                try
                                {
                                    AvePoint.Wrapper.Common.AvePerformanceMonitor.SetDisable(false);
                                    await disposalActivityManagementProcessorOnTeam.RunNowAsync();
                                }
                                finally
                                {
                                    RAArchiverCommon.DisposalProgress.Impl.CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                                    RAArchiverCommon.DisposalProgress.Impl.CompoundDisposalStatistics.Instance.WaitEndStatistic();
                                    JobExecutionProgressStatisticExecutor.Instance.Dispose();
                                    disposalActivityManagementProcessorOnTeam.EndWork();
                                    PerformanceMonitor.WritePerformanceResult();
                                    AvePoint.Wrapper.Common.AvePerformanceMonitor.WritePerformanceResult();
                                }
                                logger.Info("Skip Teams job {0} because current node is not Teams node.", jobId);
                            }
                            break;
                        }
                    case JobType.SpecifyTeamsArchiverBackup:
                        {
                            await new TeamsRuleActionController().Build(jobId, jobType).RunAsync();
                            break;
                        }
                    case JobType.RecordsDisposal:
                    case JobType.OneDriveRecordsDisposal:
                    case JobType.RMArchiverBackup:
                    case JobType.RMEndUserArchiverBackup:
                    case JobType.SOPreScan:
                    case JobType.DiscoveryPreScan:
                    case JobType.DiscoveryPlanProScan:
                    case JobType.DiscoverOptimization:
                    case JobType.DiscoveryPlanProOptimization:
                    case JobType.DiscoveryAOSPOptimization:
                    case JobType.SpecifySitesArchiverBackup:
                    case JobType.ArchiverByHSMXml:
                        DisposalActivityManagementProcessor disposalActivityManagementProcessor = new DisposalActivityManagementProcessor(jobId, jobType);
                        try
                        {
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.SetDisable(false);
                            await disposalActivityManagementProcessor.RunNowAsync();
                        }
                        finally
                        {
                            JobExecutionProgressStatisticExecutor.Instance.Dispose();
                            disposalActivityManagementProcessor.EndWork();
                            PerformanceMonitor.WritePerformanceResult();
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.ApplyClassCode:
                        FSApplyClassCodeProcesser fsApplyClassCodeProcesser = new FSApplyClassCodeProcesser(jobId, jobType);
                        await fsApplyClassCodeProcesser.RunNowAsync();
                        break;
                    case JobType.CleanUpDuplicateDatas:
                        CleanUpDuplicateDatasProcessor cleanUpDuplicateDatasProcessor = new CleanUpDuplicateDatasProcessor(jobId, jobType);
                        try
                        {
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.SetDisable(false);
                            await cleanUpDuplicateDatasProcessor.RunNowAsync();
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.ArchiverRestore:
                    case JobType.ArchiverOutPlaceRestore:
                    case JobType.StubOopRestore:
                    case JobType.AOSPRestore:
                    case JobType.ArchiverToSpoRestore:
                    case JobType.StubArchiverRestore:
                    case JobType.M365InPlaceArchiverRestore:
                        AbstractAveItemRestore archiverRestore = new AveItemRestoreMain(jobId, jobType);
                        try
                        {
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.SetDisable(false);
                            await archiverRestore.RunNowAsync();
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                            AvePoint.Wrapper.Common.AvePerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.BaseArchiveJobIdMultiRestore:
                        BaseArchiveJobIdMultiRestoreExecuter baseArchiveJobIdMultiRestoreExecuter = new BaseArchiveJobIdMultiRestoreExecuter(jobId);
                        baseArchiveJobIdMultiRestoreExecuter.Execute();
                        break;
                    case JobType.SimulateRestore:
                        AbstractAveItemRestore simulateArchiverRestore = new AveItemSimulateResotreMain(jobId, jobType);
                        await simulateArchiverRestore.RunNowAsync();
                        break;
                    case JobType.PreviewRestore:
                        AbstractAveItemRestore PreviewRestore = new AveItemPreviewRestoreMain(jobId, jobType, jobQueueMsg.Extension);
                        await PreviewRestore.RunNowAsync();
                        break;
                    case JobType.ArchiverFullTextIndex:
                        var fullTextIndexRunner = new RMArchivedFullTextIndexJobRunner(jobId);
                        await fullTextIndexRunner.RunAsync();
                        break;
                    case JobType.DeleteRestoredData:
                        var deleteRestoredDataRunner = new RMDeleteArchivedDataProcessor(jobId);
                        await deleteRestoredDataRunner.RunAsync();
                        break;
                    case JobType.DeleteArchivedSiteCollection:
                        var deleteArchivedSiteCollection = new RMDeleteArchivedSCJobHandler(jobId, jobType);
                        await deleteArchivedSiteCollection.RunAsync();
                        break;
                    case JobType.DiscoveryJobV2:
                        var discoveryV2JobRunner = new RMDiscoveryOffice365AnalysisV2JobRunner(jobId);
                        await discoveryV2JobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryJobV3:
                        var discoveryV3JobRunner = new RMDiscoveryOffice365AnalysisV3JobRunner(jobId);
                        await discoveryV3JobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryJobV4:
                        var discoveryV4JobRunner = new RMDiscoveryOffice365AnalysisV4JobRunner(jobId);
                        await discoveryV4JobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryJobV5:
                        var discoveryV5JobRunner = new RMDiscoveryOffice365AnalysisV5JobRunner(jobId);
                        await discoveryV5JobRunner.RunAsync();
                        break;
                    case JobType.SFDiscoveryJob:
                        var sfDiscoveryJobRunner = new RMSFAnalysisJobRunner(jobId);
                        await sfDiscoveryJobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryProfileJob:
                        var discoveryProfileJobRunner = new RMDiscoveryOffice365AnalysisProfileJobRunner(jobQueueMsg);
                        await discoveryProfileJobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryExportRowDataJob:
                        var discoveryExportRowDataJobRunner = new RMDiscoveryOffice365ExportRowDataJobRunner(jobId);
                        await discoveryExportRowDataJobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryGoogleProfileJob:
                        var discoveryGoogleProfileJobRunner = new RMDiscoveryGoogleAnalysisProfileJobRunner(jobQueueMsg);
                        await discoveryGoogleProfileJobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryGoogleJobV1:
                        var discoveryGoogleJobV1Runner = new RMDiscoveryGoogleAnalysisV1JobRunner(jobId);
                        await discoveryGoogleJobV1Runner.RunAsync();
                        break;                    
                    case JobType.DiscoveryAOSPJob:
                        var discoveryAOSPJobRunner = new RMDiscoveryAOSPAnalysisJobRunner(jobId);
                        await discoveryAOSPJobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryAnalysisFileSystemV1:
                        var fsAnalysisV1JobRunner = new RMDiscoveryFSAnalysisV1JobRunner(jobId);
                        await fsAnalysisV1JobRunner.RunAsync();
                        break;
                    case JobType.DiscoveryExportDuplicationReport:
                        var reportRunner = new RMDiscoveryOffice365ExportDuplicationReportRunner(jobId, args[2]);
                        await reportRunner.RunAsync();
                        break;
                    case JobType.ArchiverMoveIndex:
                        ArchiverMoveIndexJobHandler moveIndex = new ArchiverMoveIndexJobHandler(jobId, jobType);
                        await moveIndex.PerformArchiverMoveIndexJobAsync();
                        break;
                    case JobType.ArchiverRetention:
                        ArchiverRetentionJobHandler retention = new ArchiverRetentionJobHandler(jobId, jobType);
                        await retention.RunAsync();
                        break;
                    case JobType.ArchiverRetentionSimulate:
                        ArchiverRetentionJobHandler retentionSimulate = new ArchiverRetentionJobHandler(jobId, jobType);
                        await retentionSimulate.RunAsync();
                        break;
                    case JobType.ArchiverFullMoveRetention:
                        ArchiverFullMoveRetentionJobHandler fullMoveHandler = new(jobId, jobType);
                        await fullMoveHandler.RunAsync();
                        break;
                    case JobType.DeleteOrphanDatas:
                        ArchiverRetentionJobHandler deleteOrphan = new ArchiverRetentionJobHandler(jobId, jobType);
                        await deleteOrphan.RunDeleteOrphanDatasAsync();
                        break;
                    case JobType.RebuildStub:
                        ArchiverRebuildStubJobHandler rebuildStub = new ArchiverRebuildStubJobHandler(jobId, jobType);
                        await rebuildStub.RunAsync();
                        break;
                    case JobType.RebuildIndex:
                        ArchiverRebuildIndexJobHandler rebuildIndex = new ArchiverRebuildIndexJobHandler(jobId, jobType);
                        await rebuildIndex.RunAsync();
                        break;
                    case JobType.RebuildEncryptKeyValue:
                        DefaultJobStateHandler rebuildEncryptKeyValueJobStateHandler = new DefaultJobStateHandler();
                        await rebuildEncryptKeyValueJobStateHandler.RebuildEncryptKeyValue(args[2]);
                        break;
                    case JobType.RebuildSOJobReport:
                        DefaultJobStateHandler defaultJobStateHandler = new DefaultJobStateHandler();
                        defaultJobStateHandler.RebuildSOJobReport(args[2]);
                        break;
                    case JobType.BuildRunningJobReport:
                        DefaultJobStateHandler buildRunningJobHandler = new DefaultJobStateHandler();
                        buildRunningJobHandler.BuildRunningJobReport(args[2]);
                        break;
                    case JobType.ExportDecryptIndexDB:
                        ExportDecryptionIndexDBJobHandler jobHandler = new ExportDecryptionIndexDBJobHandler(jobId, args[2]);
                        jobHandler.Run();
                        break;
                    case JobType.MultiSiteCollectionRestore:
                        MultiSiteCollectionRestoreExecuter jobExecuter = new MultiSiteCollectionRestoreExecuter(jobId, args[2]);
                        jobExecuter.Execute();
                        break;
                    case JobType.RebuildDeDupForWPPMigration:
                        MigrateWPPDeDupStage migrateBackendDataStage = new MigrateWPPDeDupStage();
                        migrateBackendDataStage.RebuildDeDupForWPPMigration();
                        break;
                    case JobType.PhysicalLoanPick:
                        var pickProcessor = new LoanPickStatusProcessor(jobType, jobId);
                        await pickProcessor.RunAsync();
                        break;
                    case JobType.ManualExportHistoryDatasJob:
                        ManualApprovalHistoryExportProcessor manualApprovalHistoryExportProcessor = new(jobId, args[2]);
                        await manualApprovalHistoryExportProcessor.RunAsync();
                        break;
                    case JobType.PhysicalDestructionPick:
                        var destructionProcessor = new DestructionPickStatusProcessor(jobType, jobId);
                        await destructionProcessor.RunAsync();
                        break;
                    case JobType.PhysicalLoanPickExportJob:
                        var loanExportPickProcessor = new ExportLoanPickListProcessor(jobType, jobId);
                        await loanExportPickProcessor.RunAsync();
                        break;
                    case JobType.PhysicalDestructionPickExportJob:
                        var destructionPickListExportProcessor = new ExportDestructionPickListProcessor(jobType, jobId);
                        await destructionPickListExportProcessor.RunAsync();
                        break;
                    case JobType.ExportReportDetails:
                        ReportExportProcessor reportExportProcessor = new ReportExportProcessor(jobId);
                        await reportExportProcessor.RunAsync();
                        break;
                    case JobType.ExportFSSetting:
                        {
                            RMExportSettingJobMessage message = new RMExportSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                            };
                            //var enableJPMCFeature = KeyValueDao.IsEnableJPMCFileSystemFeature();
                            //if (enableJPMCFeature)
                            //{
                            //    FSExportSettingProcessorJPMC settingExportProcessorJPMC = new FSExportSettingProcessorJPMC(message);
                            //    await settingExportProcessorJPMC.RunAsync();
                            //}
                            //else
                            //{
                            FSExportSettingProcessor settingExportProcessor = new FSExportSettingProcessor(message);
                            await settingExportProcessor.RunAsync();
                            //}
                        }
                        break;
                    case JobType.DownloadRCCReport:
                        {
                            jobId = args[1];
                            var request = string.IsNullOrEmpty(jobQueueMsg?.Extension)
                                ? new RCCReportRequest()
                                : JsonConvert.DeserializeObject<RCCReportRequest>(jobQueueMsg.Extension);
                            var processor = new RCCReportProcessor(jobId, request);
                            await processor.RunAsync();
                            break;
                        }
                    case JobType.SharePointSiteMetricsReport:
                        {
                            var destinationLibUrl = args[2];
                            var json = Encoding.UTF8.GetString(Convert.FromBase64String(jobQueueMsg.Extension));
                            var siteUrls = SerializerHelper.DeserializeByJsonSerializer<List<string>>(json);
                            var exportRunner = new RMSharePointSiteMetricsReportRunner(jobId, siteUrls, destinationLibUrl);
                            await exportRunner.RunAsync();
                            break;
                        }
                    case JobType.ExportSPSetting:
                        {
                            if (!Enum.TryParse<ExportSettingType>(args[2], out ExportSettingType type))
                            {
                                type = ExportSettingType.OnlyExportCustomSettingNodes;
                            }
                            RMExportSettingJobMessage message = new RMExportSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                exportSettingType = type
                            };
                            SPExportSettingProcessor settingExportProcessor = new SPExportSettingProcessor(message);
                            await settingExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.ExportSPSOSetting:
                        {
                            if (!Enum.TryParse<ExportSettingType>(args[2], out ExportSettingType type))
                            {
                                type = ExportSettingType.OnlyExportCustomSettingNodes;
                            }
                            RMExportSettingJobMessage message = new RMExportSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = Contract.Tenant.TenantLocalValue.LogonUserEmail,
                                exportSettingType = type
                            };
                            SPExportSOSettingProcessor settingExportProcessor = new(message);
                            await settingExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.MachineLearningTraining:
                        try
                        {
                            var mlProcessor = new MLProcessor(jobId);
                            mlProcessor.Run();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.MachineLearningAnalyse:
                        try
                        {
                            var mlAnalyseProcessor = new MLAnalyseProcessor(jobId);
                            mlAnalyseProcessor.Run();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.MachineLearningReviewApprove:
                    case JobType.MachineLearningReviewReclassify:
                        try
                        {
                            var reviewBulkAction = new MLReclassifyBulkAction(jobId, jobType, args[2]);
                            await reviewBulkAction.RunAsync();
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.ManualExportRecordsForReviewDatasJob:
                        await ManualApprovalExportJobProcessAction.RunAsync(jobId, args[2], Convert.ToInt32(args[3]));
                        break;
                    case JobType.ManualImportUnderReviewDatasJob:
                        ImportUnderReviewDatasProcessor importUnderReviewDatasProcessor = new ImportUnderReviewDatasProcessor(jobId, args[3], args[4]);
                        await importUnderReviewDatasProcessor.RunAsync();
                        break;
                    case JobType.DeleteInvalidRecords:
                        ManualApprovalDeleteInvalidRecordProcessor deleteInvalidRecordProcessor = new ManualApprovalDeleteInvalidRecordProcessor(jobId);
                        await deleteInvalidRecordProcessor.RunAsync();
                        break;
                    case JobType.VeoMerge:
                        MergeVEO veoProcessor = new MergeVEO(jobId);
                        veoProcessor.Merge();
                        break;
                    case JobType.ArchiverExport:
                        ArchiverSiteInfoExportProcessor archiverSiteInfoExportProcessor = new ArchiverSiteInfoExportProcessor(jobId, jobQueueMsg.Extension);
                        await archiverSiteInfoExportProcessor.RunAsync();
                        break;
                    case JobType.MoveDataTier:
                        MoveDataTierMain moveDataTierProcessor = new MoveDataTierMain(jobId);
                        await moveDataTierProcessor.RunAsync();
                        break;
                    case JobType.CloudArchiverMigration:
                        await new ArchiverMigrationJobExecutor(jobQueueMsg).RunAsync();
                        break;
                    case JobType.DownloadJobReports:
                        JobReportExportProcessor jobReportExportProcessor = new JobReportExportProcessor(jobId, jobQueueMsg.Extension, isDownloadJobReports: true);
                        await jobReportExportProcessor.RunAsync();
                        break;
                    case JobType.DownloadJobReportsForCOP:
                        JobReportExportProcessor jobReportExportProcessorCOP = new JobReportExportProcessor(jobId, jobQueueMsg.Extension, true);
                        await jobReportExportProcessorCOP.RunAsync();
                        break;
                    case JobType.MachineLearningExportReportJob:
                        MLReportExportProcessor mlExportReport = new MLReportExportProcessor(jobId, args[2]);
                        await mlExportReport.RunAsync();
                        break;
                    case JobType.AdjustStorageSize:
                        AdjustSizeMain adjustStorageSizeProcessor = new AdjustSizeMain(jobId);
                        await adjustStorageSizeProcessor.RunAsync();
                        break;
                    case JobType.ExportSiteMetrics:
                        await new ExportSiteMetricsProcessor(jobId).RunAsync();
                        break;
                    case JobType.ImportGoogleTermStructure:
                        #region GoogleLabelSyncToLocal
                        try
                        {
                            jobId = args[1];
                            string termGroupId = args[2];
                            ImportLabelProcessor processor = new(jobId, JobType.ImportGoogleTermStructure, termGroupId);
                            await processor.KickOffAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.GoogleApplySettings:
                        #region Google Apply setting job
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            ApplySettingProcessor processor = new ApplySettingProcessor(jobId);
                            // var customerId = TenantLocalValue.LogonGroupId;
                            // var tenantId = RMAosApiClient.GetGoogleTenantIds(customerId).FirstOrDefault();
                            // processor.Build(customerId, tenantId);
                            await processor.KickOffAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.GoogleDataSynchronization:
                        #region Google Synchronization
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            var proccessor = new GoogleExplorerProcessor(jobId);
                            await proccessor.KickOffAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.GoogleRecordsDisposal:
                        #region Google Run Enforce Rule
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            var proccessor = new RecordsDisposalProcessorV2(jobId);
                            await proccessor.KickOffAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This job is stopped.");
                        }
                        catch(Exception e)
                        {
                            throw e;
                        }
                        #endregion
                        break;
                    case JobType.GoogleCreateAndDestroyedFileReport:
                        #region Google Create and Destroyed File Report
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                SelectCreated = Convert.ToBoolean(args[3]),
                                SelectDestroyed = Convert.ToBoolean(args[4]),
                                StartTime = Convert.ToDateTime(args[5], dtFormat),
                                EndTime = Convert.ToDateTime(args[6], dtFormat),
                                GlobalTimeZoneId = args[7]
                            };
                            logger.Info(msg.ToString());
                            var procesor = new GoogleCreationAndDestructionProcessor(msg);
                            await procesor.KickOffAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.GoogleItemsFilesDueDisposalReport:
                        #region GoogleItemsFilesDueDisposalReport
                        try
                        {
                            profileId = args[2];
                            var processor = new GoogleContentDueReportProcessor(jobId, profileId);
                            await processor.KickOffAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.GoogleBCSTermUsageReport:
                    case JobType.GoogleOrphanedTermUsageReport:
                    case JobType.GoogleRetiredTermUsageReport:
                        #region GoogleBCSTermUsageReport
                        try
                        {
                            var isGoogleOrphanedTermReport = false;
                            var isGoogleRetiredTermReport = false;
                            profileId = args[2];
                            if (!string.IsNullOrEmpty(args[3]))
                            {
                                isGoogleOrphanedTermReport = Convert.ToBoolean(args[3]);
                            }
                            if (!string.IsNullOrEmpty(args[4]))
                            {
                                isGoogleRetiredTermReport = Convert.ToBoolean(args[4]);
                            }
                            logger.Info("Run Google term usage report jobInfo JobId {0} ProfileId {1} IsOrphanedTermReport {2} IsRetiredTerm Report {3}",
                                jobId, profileId, isGoogleOrphanedTermReport, isGoogleRetiredTermReport);
                            GoogleTermUsageReportProcessor processor = new GoogleTermUsageReportProcessor(jobId, profileId, isGoogleOrphanedTermReport, isGoogleRetiredTermReport);
                            await processor.KickOffAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.GoogleArchiverRestore:
                        var restoreMain = new GDriveItemRestoreMain(jobId, jobType);
                        await restoreMain.RunNowAsync();
                        break;
                    case JobType.GoogleArchiverRetention:
                        var driveRetention = new GDriveArchiverRetentionJobHandler(jobId, jobType);
                        await driveRetention.RunAsync();
                        break;
                    case JobType.GoogleRestoreReport:

                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                StartTime = Convert.ToDateTime(args[3], dtFormat),
                                EndTime = Convert.ToDateTime(args[4], dtFormat),
                                GlobalTimeZoneId = args[5]
                            };
                            logger.Info(msg.ToString());
                            var processor = new GoogleRestoreReportProcessor(msg);
                            await processor.RunReportAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        break;
                    case JobType.ExportAdvanceSeachResult:
                        ExportAdvanceSearchResultProcessor exportAdvanceSeachResult = new ExportAdvanceSearchResultProcessor(jobId, jobType);
                        await exportAdvanceSeachResult.RunNowAsync();
                        break;
                    case JobType.ExportRestoreCenterSeachResult:
                        ExportAdvanceSearchResultProcessor exportRestoreCenterSeachResult = new ExportAdvanceSearchResultProcessor(jobId, jobType);
                        await exportRestoreCenterSeachResult.RunRestoreCenterExportNowAsync();
                        break;
                    case JobType.ArchiverDeduplication:
                        await new ArchiverDeduplicationJobHandler(jobId, jobType).RunAsync();
                        break;
                    case JobType.ArchiverDeduplicationReport:
                        DeduplicationSiteInfoExportProcessor archiverDedupSiteInfoExportProcessor = new DeduplicationSiteInfoExportProcessor(jobId, jobQueueMsg.Extension);
                        await archiverDedupSiteInfoExportProcessor.RunAsync();
                        break;
                    case JobType.ExportIndex:
                        await new ExportKeyAndIndexProcess(jobId, jobType, args[2]).RunAsync();
                        break;                    
                    case JobType.PhysicalTemplateImport:
                        await new ImportPhysicalTemplateWorker(jobId, args[2]).RunAsync();
                        break;
                    case JobType.ConvertStub:
                        await new ConvertStubJobHandler(jobId, jobType).RunAsync();
                        break;
                    case JobType.JobMonitorArchive:

                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            var moved = await JobService.ArchiveJobRecordsAsync(jobId);
                            logger.Info($"Archived {moved} RMJobMonitor rows to RMJobMonitorArchive in total.");
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"JobMonitorArchive failed: {ex}");
                            JobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }

                        break;
                    #region Teams 
                    case JobType.ApplyTeamsSettings:
                        try
                        {
                            RMTeamsSettingProcessor setting = new RMTeamsSettingProcessor(jobId, jobQueueMsg);
                            await setting.ApplyTeamsSettingAsync(false);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.TeamsUniqueIDSettingFullSchedule:
                    case JobType.TeamsUniqueIDSettingIncrementalSchedule:
                        try
                        {
                            TeamsUniqueIdSettingWorker uniqueIdSettingWorker = new TeamsUniqueIdSettingWorker(jobId, null);
                            await uniqueIdSettingWorker.ConfigUniqueIDSettingAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        break;
                    case JobType.TeamsDataSynchronisation:
                        try
                        {
                            await new RMTeamsSyncProcessor(jobId).RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    case JobType.TeamsScheduleSetting:
                        #region TeamsScheduleSetting
                        try
                        {
                            PerformanceMonitor.InitsStatistics();
                            RMTeamsSettingProcessor setting = new RMTeamsSettingProcessor(jobId, jobQueueMsg);
                            //setting.ApplySharePointSetting();
                            await setting.ApplyTeamsSettingAsync(true);
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        #endregion
                        break;
                    case JobType.TeamsArchiverRestore:
                    case JobType.TeamsOutPlaceRestore:
                    case JobType.MailBoxArchiverRestore:
                        await new RestoreActionController().Build(jobId, jobType).RunAsync();
                        break;
                    case JobType.TeamsEnforceRetention:
                        try
                        {
                            RMTeamsEnforceRetentionProcessor processor = new RMTeamsEnforceRetentionProcessor(jobId);
                            await processor.RunNowAsync();
                        }
                        catch (JobStopException)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            PerformanceMonitor.WritePerformanceResult();
                        }
                        break;
                    case JobType.ExportTeamsSetting:
                        {
                            if (!Enum.TryParse<ExportSettingType>(args[2], out ExportSettingType type))
                            {
                                type = ExportSettingType.OnlyExportCustomSettingNodes;
                            }
                            RMExportSettingJobMessage message = new RMExportSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = TenantLocalValue.LogonUserEmail,
                                exportSettingType = type
                            };
                            TeamsExportSettingProcessor settingExportProcessor = new (message);
                            await settingExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.ExportTeamsSOSetting:
                        {
                            if (!Enum.TryParse<ExportSettingType>(args[2], out ExportSettingType type))
                            {
                                type = ExportSettingType.OnlyExportCustomSettingNodes;
                            }
                            RMExportSettingJobMessage message = new RMExportSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = TenantLocalValue.LogonUserEmail,
                                exportSettingType = type
                            };
                            TeamsExportSOSettingProcessor settingExportProcessor = new(message);
                            await settingExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.ImportTeamsSetting:
                        {
                            RMImportSPSettingJobMessage message = new RMImportSPSettingJobMessage
                            {
                                JobType = jobType,
                                JobID = jobId,
                                JobRunBy = TenantLocalValue.LogonUserEmail,
                                CSVPath = args[3],
                            };
                            RMImportTeamsSettingProcessor settingExportProcessor = new(message);
                            await settingExportProcessor.RunAsync();
                        }
                        break;
                    case JobType.TeamsRestoreReport:
                        #region TeamsRestoreReport
                        try
                        {
                            DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo
                            {
                                ShortDatePattern = "yyyy/MM/dd"
                            };
                            var msg = new RMCreationJobMessage()
                            {
                                JobType = jobType,
                                JobID = jobId,
                                ProfileId = args[2],
                                StartTime = Convert.ToDateTime(args[3], dtFormat),
                                EndTime = Convert.ToDateTime(args[4], dtFormat),
                                GlobalTimeZoneId = args[5]
                            };
                            logger.Info(msg.ToString());
                            TeamsRestoreReportProcessor cdfr = new TeamsRestoreReportProcessor(msg);
                            await cdfr.RunReportJobAsync();
                        }
                        catch (Exception e)
                        {
                            logger.Error($@"Fail run restore report, ex:{e}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.TeamsBCSTermUsageReport:
                    case JobType.TeamsRetiredTermUsageReport:
                    case JobType.TeamsOrphanedTermUsageReport:
                        bool isTeamsRetiredTermReport = false;
                        bool isTeamsOrphanedTermReport = false;
                        try
                        {
                            profileId = args[2];
                            isTeamsOrphanedTermReport = Convert.ToBoolean(args[3]);

                            if (!string.IsNullOrEmpty(args[4]))
                            {
                                isTeamsRetiredTermReport = Convert.ToBoolean(args[4]);
                            }
                            logger.Info("Run termusage report jobInfo JobId {0} ProfileId {1} IsTeamsOrphanedTermReport {2} IsTeamsRetiredTerm Report {3}",
                                jobId, profileId, isTeamsOrphanedTermReport, isTeamsRetiredTermReport);
                            RMTeamsReportProcessor termUsage = new TeamsBCSTermUsageReportProcessor(jobId, profileId, isTeamsOrphanedTermReport, isTeamsRetiredTermReport);
                            await termUsage.RunAsync();
                            break;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    case JobType.TeamsArchiverRetention:
                        TeamsArchiverRetentionJobHandler teamsRetention = new TeamsArchiverRetentionJobHandler(jobId, jobType);
                        await teamsRetention.RunAsync();
                        break;
                    case JobType.EXOArchiverRetention:
                        ExchangeArchiverRetentionJobHandler exchangeRetention = new ExchangeArchiverRetentionJobHandler(jobId, jobType);
                        await exchangeRetention.RunAsync();
                        break;
                    case JobType.TeamsItemsFilesDueDisposalReport:
                        #region TeamsItemsFilesDueDisposalReport
                        try
                        {
                            profileId = args[2];
                            RMTeamsReportProcessor dueDisposal = new TeamsDueDisposalReportProcessor(jobId, profileId);
                            await dueDisposal.RunAsync();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        #endregion
                        break;
                    case JobType.TeamsChannelSettingConflictCheck:
                        var teamsChannelSettingConflictsProcessor = new TeamsChannelSettingConflictsProcessor(jobId);
                        await teamsChannelSettingConflictsProcessor.RunAsync();
                        break;
                    case JobType.TeamsDataUpgrade:
                        var teamsDataUpgrade = new TeamsDataUpgradeProcessor(jobId);
                        await teamsDataUpgrade.RunAsync();
                        break;
                    case JobType.TeamsNodeSettingUpgrade:
                        var teamsNodeSettingUpgradeProcessor = new TeamsNodeSettingUpgradeProcessor(jobId);
                        teamsNodeSettingUpgradeProcessor.Run();
                        break;
                    case JobType.ConflictSettingDetailExport:
                        var conflictSettingDetailExportProcessor = new ConflictSettingResultExportProcessor(jobId);
                        await conflictSettingDetailExportProcessor.RunAsync();
                        break;
                    #endregion
                    case JobType.DeclaredRecordsMigration:
                        #region DeclaredRecordsMigration
                        try
                        {
                            var processor = new DeclaredRecordMigrationProcessor(jobId, jobType);
                            await processor.RunJobAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Fail run DeclaredRecordsMigration job, error:{ex}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.StubDisposal:
                        #region StubDisposal
                        try
                        {
                            var processor = new StubDisposalProcessor(jobId, jobType);
                            await processor.RunJobAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Fail run StubDisposal job, error:{ex}");
                            throw;
                        }
                        #endregion
                        break;
                    case JobType.MultiGeoMainDCSyncCommonData:
                        #region MultiGeoMainDCSyncCommonData
                        {
                            var processor = new MainDCSyncDataProcessor(jobId);
                            await processor.RunAsync();
                        }
                        #endregion
                        break;
                    case JobType.MultiGeoOtherDCSyncCommonData:
                        #region MultiGeoOtherDCSyncCommonData
                        {
                            long needSyncTable = 0;
                            long.TryParse(args[3], out needSyncTable);
                            var processor = new OtherDCSyncDataProcessor(jobId, args[2], needSyncTable, args[4]);
                            await processor.RunAsync();
                        }
                        #endregion
                        break;
                    case JobType.APStorageCostEvaluation:
                        await new APStorageCostEvaluationHandler(jobId).RunAsync();
                        break;
                    case JobType.DispatchedJob:
                        RunDispatchedJob(currentUser, jobQueueMsg);
                        break;
                    default:
                        throw new Exception("Not support job type.");
                }
            }
            else
            {
                logger.Error($"Cannot convert {args[0]} to JobType");
                throw new Exception("Not support job type.");
            }
        }

        private static void RunDispatchedJob(string currentUser, JobQueueMessage jobQueueMsg)
        {
            if (jobQueueMsg == null)
            {
                throw new ArgumentNullException(nameof(jobQueueMsg));
            }

            if (string.IsNullOrWhiteSpace(jobQueueMsg.Extension))
            {
                throw new InvalidOperationException("DispatchedJob requires a valid extension payload.");
            }

            var payload = JsonConvert.DeserializeObject<JobDispatchPayload>(jobQueueMsg.Extension);
            if (payload == null)
            {
                throw new InvalidOperationException("DispatchedJob cannot deserialize payload.");
            }

            var jobRunBy = ResolveJobRunBy(jobQueueMsg);
            if (!string.IsNullOrWhiteSpace(payload.OriginalMessageId)
                        && !string.IsNullOrWhiteSpace(payload.OriginalTenantId))
            {
                PlatformWindsorManager.GetService<IJobQueueService>()
                    .DeleteDBJobQueueMessage(payload.OriginalMessageId, payload.OriginalTenantId);

                logger.Info(
                    "Deferred queue cleanup executed for target job type={0}, original messageId={1}, tenantId={2}.",
                    payload.TargetJobType,
                    payload.OriginalMessageId,
                    payload.OriginalTenantId);
            }

            logger.Info("Start DispatchedJob and invoke target job type {0}.", payload.TargetJobType);

            string spawnedJobId = payload.TargetJobType switch
            {
                JobType.TeamsRecordsDisposal => PlatformWindsorManager.GetService<IRMTeamsSettingsService>().RealRunRecordsDisposalJobAsync(jobRunBy, currentUser, payload.Parameters).GetAwaiter().GetResult(),
                JobType.TeamsPreScan => PlatformWindsorManager.GetService<IRMArchiverSettingsService>().RealRunTeamsPreScanJob(jobRunBy, currentUser, payload.Parameters),
                JobType.TeamsArchiverBackup => PlatformWindsorManager.GetService<IRMArchiverSettingsService>().RealRunTeamsArchiverBackupJob(jobRunBy, currentUser, payload.Parameters),
                JobType.RecordsDisposal => PlatformWindsorManager.GetService<IRMSharePointSettingsService>().RealRunRecordsDisposalJobAsync(jobRunBy, currentUser, payload.Parameters).GetAwaiter().GetResult(),
                JobType.SOPreScan => PlatformWindsorManager.GetService<IRMArchiverSettingsService>().RealRunSOPreScanJob(jobRunBy, currentUser, payload.Parameters),
                JobType.RMArchiverBackup => PlatformWindsorManager.GetService<IRMArchiverSettingsService>().RealRunArchiverBackupJob(jobRunBy, currentUser, payload.Parameters),
                _ => throw new InvalidOperationException($"DispatchedJob does not support target job type {payload.TargetJobType}."),
            };

            if (JobServiceUtility.SkipMergeDetailsJobs.Contains((int)payload.TargetJobType) && !string.IsNullOrEmpty(spawnedJobId))
            {
                try
                {
                    JobService.UpdateJobVersionAsync(spawnedJobId, JobVersion.UnMerged).ExecuteAsyncTask();
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to update job version for spawned job {0}. Error: {1}", spawnedJobId, ex.ToString());
                }
            }

            logger.Info("DispatchedJob finished. Target job type: {0}. Spawned job id: {1}.", payload.TargetJobType, spawnedJobId);
        }

        private static JobRunBy ResolveJobRunBy(JobQueueMessage jobQueueMsg)
        {
            if (jobQueueMsg == null)
            {
                throw new ArgumentNullException(nameof(jobQueueMsg));
            }

            if (jobQueueMsg.RunBy is JobRunBy runBy)
            {
                return runBy;
            }

            if (Enum.TryParse(jobQueueMsg.RunBy.ToString(), true, out JobRunBy parsedRunBy))
            {
                return parsedRunBy;
            }

            throw new InvalidOperationException("Cannot resolve JobRunBy from job queue message.");
        }

        private static Assembly ResolveSPAssemblyEventHandler(object sender, ResolveEventArgs args)
        {
            //Microsoft.SharePoint.Client, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c  
            string currentPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            /*if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client.Runtime, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
            {
                logger.Info(" Assembly Microsoft.SharePoint.Client.Runtime, Version=15.0.0.0, dll , path {0}", currentPath);
                return Assembly.LoadFrom(currentPath + @"2013\Microsoft.SharePoint.Client.Runtime.dll");
            }
            else if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
            {
                logger.Info("Assembly Microsoft.SharePoint.Client, Version=15.0.0.0, dll, path {0}", currentPath);
                return Assembly.LoadFrom(currentPath + @"2013\Microsoft.SharePoint.Client.dll");
            }
            else if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client.Taxonomy, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
            {
                logger.Info("Assembly Microsoft.SharePoint.Client.Taxonomy, Version=15.0.0.0, dll, path {0}", currentPath);
                return Assembly.LoadFrom(currentPath + @"2013\Microsoft.SharePoint.Client.Taxonomy.dll");
            }
            else*/
            try
            {
                if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client.Runtime, Version=16.1.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                {
                    logger.Info(" Assembly Microsoft.SharePoint.Client.Runtime, Version=16.1.0.0, dll , path {0}", currentPath);
                    return Assembly.LoadFrom(currentPath + @"Microsoft.SharePoint.Client.Runtime.dll");
                }
                else if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client, Version=16.1.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                {
                    logger.Info("Assembly Microsoft.SharePoint.Client, Version=16.1.0.0, dll, path {0}", currentPath);
                    return Assembly.LoadFrom(currentPath + @"Microsoft.SharePoint.Client.dll");
                }
                else if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client.Taxonomy, Version=16.1.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                {
                    logger.Info("Assembly Microsoft.SharePoint.Client.Taxonomy, Version=16.1.0.0, dll, path {0}", currentPath);
                    return Assembly.LoadFrom(currentPath + @"Microsoft.SharePoint.Client.Taxonomy.dll");
                }
                else if (args.Name != null && args.Name.Contains("Microsoft.SharePoint.Client.DocumentManagement, Version=16.1.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"))
                {
                    logger.Info("Assembly Microsoft.SharePoint.Client.DocumentManagement, Version=16.1.0.0, dll, path {0}", currentPath);
                    return Assembly.LoadFrom(currentPath + @"Microsoft.SharePoint.Client.DocumentManagement.dll");
                }
                else
                {
                    logger.Info("Do not need load this dll.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Load DLL Error {0}", ex.ToString());
                return null;
            }
        }
    }
}
