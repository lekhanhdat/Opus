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
using AngleSharp.Common;
using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.Cryptography;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.GCommon.Utility.Exceptions.Job;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.SharePoint.Archiver.Discovery;
using AvePoint.RA.SharePoint.Archiver.Discovery.AOSP;
using AvePoint.RA.SharePoint.Archiver.Move;
using AvePoint.RA.SharePoint.Archiver.Scan;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan.AOSP;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using LS.SPWorkflowProcessor;
using Media.Service.ArchiverBackup.LogicBackup;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAArchiverCommon.TeamsController;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using SOApproveDBStatus = AvePoint.RA.Contract.SOApproveDBStatus;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class DisposalActivityManagementProcessor
    {
        private static readonly AveLogger Logger = AveLogger.GetInstance(typeof(DisposalActivityManagementProcessor));
        private JobContext jobContext = null;
        private string JobId = string.Empty;
        private JobType mJobType;
        ScheduleConfiguration mConfiguration;
        private int subJobNumber = 0;
        private Exception error;
        private Dictionary<int, SPObjectBackup> mVaults;
        private Dictionary<int, SPObjectBackup> mBackups;
        private Dictionary<int, SPObjectBackup> mRecordManager;
        private Dictionary<int, SPObjectBackup> mRelativeDataSPObject;
        private IVaultExport vaultExport = null;
        private Dictionary<string, List<IVaultExport>> NAAMetadatas = new Dictionary<string, List<IVaultExport>>();
        private Dictionary<string, List<IVaultExport>> NARAMetadatas = new Dictionary<string, List<IVaultExport>>();
        private Dictionary<int, ArchiveApproveReport> KeepDataOnlyContainer = new Dictionary<int, ArchiveApproveReport>();
        private List<DeletionNode> deletionNodes = new List<DeletionNode>();
        private string secondHeaderFolderPath = string.Empty;
        private string secondHeaderFilePath = string.Empty;
        private string secondHeaderFilePathGuid = string.Empty;
        private StreamWriter streamWriter = null;
        private DeferredDisposalScope mDeferredDisposalScope = new();
        private bool UseArchiverProfileForAOSP;
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        public IJobDetailService jobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IEXOSettingRuleDao RMEXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        public IExportSettingsDao ExportSettingsDao => (IExportSettingsDao)PlatformWindsorManager.GetService(typeof(IExportSettingsDao));
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
        public IKeyValueService RMKeyValueService => (IKeyValueService)PlatformWindsorManager.GetService(typeof(IKeyValueService));

        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => (IExportDataEncryptionSettingService)PlatformWindsorManager.GetService(typeof(IExportDataEncryptionSettingService));
        public IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private IRMDiscoveryOffice365RuleInfoDao mDiscoveryOffice365RuleInfoDao;
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRuleManagerService mRuleManagerService;
        protected IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        private IExplorerDao ExplorerDao = new ExplorerDao();

        private Dictionary<string, MergeIndexState> mergeJobList = new Dictionary<string, MergeIndexState>();

        private RMDiscoveryOptimizedCalculator _optimizedCalculator;

        private RMDiscoveryOptimizationCalculator _optimizationCalculator;

        private RMDiscoveryAOSPOptimizedCalculator _aospOptimizedCalculator;

        private RMDiscoveryAOSPOptimizationCalculator _aospOptimizationCalculator;
        private RemoteSiteCollection mRemoteNode;
        private List<CleanUpItemEntry> mCleanUpItemEntrys;
        private readonly IRMDiscoveryOffice365TenantConfigurationDao _configurationDao = new RMDiscoveryOffice365TenantConfigurationDao();
        //used for relativeApp
        public DisposalActivityManagementProcessor()
        {
        }

        public DisposalActivityManagementProcessor(string jobId, JobType jobType, RemoteSiteCollection remoteNode = null, List<CleanUpItemEntry> cleanUpItemEntrys = null)
        {
            JobId = jobId;
            mJobType = jobType;
            mRemoteNode = remoteNode;
            mCleanUpItemEntrys = cleanUpItemEntrys;
            jobContext = JobContext.GetInstance(jobId, Contract.JobMonitor.JobType.DisposalActivityManagement);
            jobContext.ReportManager.StartUpdateJobProgress();
            mConfiguration = new ScheduleConfiguration(JobId);

            mConfiguration.O365TenantId = jobContext.O365TenantId;
            mConfiguration.compoundStatistics = CompoundDisposalStatistics.Instance;
            mConfiguration.compoundStatistics.Init(new DisposalStaticInitObject()
            {
                JobType = jobType,
                MainJobId = jobContext.MainJobId,
                SubJobId = jobContext.SubJobId
            });
            mConfiguration.compoundStatistics.StartStatistic();

            mConfiguration.jobtype = jobType;
            if (jobType == JobType.RecordsDisposal || jobType == JobType.OneDriveRecordsDisposal || jobType == JobType.TeamsRecordsDisposal)
            {
                mConfiguration.IsILMode = true;
                WrapperConfiguration.IsILMode = true;
            }
            AveEnv.AgentJobFolder = Path.Combine(mConfiguration.ArchiveTemp, "Job");

            InitVaulters();
            InitBackupers();
            InitRecordManager();
            InitRelativeDataBackupers();
            WrapperConfiguration.TempDirectory = Path.Combine(mConfiguration.ArchiveTemp, "Wrapper");
            ArchiverCommonStaticMethod.CreateDirectory(WrapperConfiguration.TempDirectory);
            secondHeaderFolderPath = SecurityUtils.SafeCombinePath(mConfiguration.ArchiveTemp, mConfiguration.JobId);
            secondHeaderFilePath = SecurityUtils.SafeCombinePath(secondHeaderFolderPath, mConfiguration.JobId + ".tmpheader");
        }

        private string CreateSubJob(string subJobId, object jobSettings, double weight)
        {
            try
            {
                using (new CheckJobStopScope()) { }
                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = mConfiguration.MainJobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)mJobType,
                    Progress = 0,
                    Status = (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.InProgress,
                    Weight = weight,
                    Runable = RecordsConstants.SubJob_Runnable_Exclude,
                    O365TenantId = jobContext.O365TenantId,
                };
                subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(jobSettings) };
                subJob.String1 = jobContext.Scope;
                SubJobDao.CreateJob(subJob);
                Logger.Info($"Create sub job {subJob.Id} sucessfull, type {subJob.JobType}, Scope {jobContext.Scope}");
                return subJobId;
            }
            catch (JobStopException stop)
            {
                Logger.Error(stop.ToString());
                SubJobDao.UpdateStatus(subJobId, (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped, DateTime.UtcNow.Ticks);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while creating a sub job on {jobContext.Scope}. Reason: {ex}");
                throw;
            }
        }


        public async Task RunNowAsync(string forceFitTeamsRule = null)
        {
            try
            {
                Logger.Info("DisposalActivityManagementProcessor Process start. forceFitTeamsRule: {0}", forceFitTeamsRule);
                ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");
                mConfiguration.ForceFitTeamsRuleID = forceFitTeamsRule;
                JobExecutionProcessStatisticExecutor.Instance.StartCalculateReportJobExecitonRecordInfo(mConfiguration);
                // Initialize progress tracking for this job
                if (JobServiceUtility.NewJobDetailsJobs.Contains((int)mConfiguration.jobtype))
                {
                    JobExecutionProgressStatisticExecutor.Instance.InitializeJobExecutionProgressStatictics(
                        scope: mConfiguration.ScopePath,
                        subJobId: mConfiguration.JobId,
                        mainJobId: mConfiguration.MainJobId,
                        jobType: (int)mJobType,
                        isInitStartTime: true);
                }
                byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
                CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
                mConfiguration.JobReportDto = new JobReportImps(jobContext.ReportManager);
                mConfiguration.ProgressDto = mConfiguration.JobReportDto;
                switch (mJobType)
                {
                    case JobType.RecordsDisposal:
                    case JobType.OneDriveRecordsDisposal:
                    case JobType.RMArchiverBackup:
                    case JobType.SpecifySitesArchiverBackup:
                    case JobType.SpecifyTeamsArchiverBackup:
                    case JobType.TeamsArchiverBackup:
                    case JobType.TeamsRecordsDisposal:
                        await ProcessRecordsDisposalAsync();
                        break;
                    case JobType.RMEndUserArchiverBackup:
                        await ProcessEndUserRecordsDisposalAsync();
                        break;
                    case JobType.SOPreScan:
                    case JobType.TeamsPreScan:
                        await SOPreScanJobAsync();
                        break;
                    case JobType.DiscoveryPreScan:
                        await ProcessDiscoverOptimizationPreScanAsync();
                        break;
                    case JobType.DiscoveryPlanProScan:
                        await ProcessDiscoveryPlanProScanAsync();
                        break;
                    case JobType.DiscoverOptimization:
                        await ProcessDiscoverOptimizationAsync();
                        break;
                    case JobType.DiscoveryPlanProOptimization:
                        await ProcessDiscoveryPlanProOptimizationAsync();
                        break;
                    case JobType.DiscoveryAOSPOptimization:
                        await ProcessDiscoverAOSPOptimizationAsync();
                        break;
                    case JobType.ArchiverByHSMXml:
                        await ProcessHSMXmlArchiverAsync();
                        break;
                    case JobType.CleanUpDuplicateDatas:
                        await ProcessDiscoverOptimizationCleanUpDuplicateAsync();
                        break;
                    //case JobType.RMArchiverBackup:
                    //ProcessRecordsDisposal(tempList.FirstOrDefault());
                    //break;
                    default:
                        Logger.Warn($"not mapping any task type:{mJobType}");
                        break;
                }
            }
            catch (SkipException e)
            {
                Logger.Error($@"The Sub job was skip, message:{e.Message}");
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = e.Message;
            }
            catch (MergeIndexException e)
            {
                Logger.Error("[MergeIndexException]" + e.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = e.Message;
            }
            catch (JobStopException stop)
            {
                Logger.Error("[JobStopException]" + stop.ToString());
                mConfiguration.JobReportDto.HasStop = true;
                JobExecutionProgressStatisticExecutor.Instance.UpdateJobStatus(JobStatus.Stopped);
                JobExecutionProgressStatisticExecutor.Instance.ResetJobId();
                throw;
            }
            catch (ScheduleJobConfigurationError configError)
            {
                Logger.Error("[ScheduleJobConfigurationError]" + configError.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = configError.Message;
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                Logger.Error("[LicenseMismatchOfAvePointStorageException]" + lme.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = lme.Message;
            }
            catch (ExportConfigurationFileError exportConfigError)
            {
                Logger.Error("[ExportConfigurationFileError]" + exportConfigError.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = exportConfigError.Message;
            }
            catch (CGDBSCTableNotFoundException ex)
            {
                Logger.Error("CGDBSCTableNotFoundException. Path:{0}. Message:{1}.", mConfiguration.SiteCollectionUrl, ex.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = ex.Message;
            }
            catch (CGDBSummaryTableException ex)
            {
                Logger.Error("CGDBSummaryTableException. Path:{0}. Message:{1}.", mConfiguration.SiteCollectionUrl, ex.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = ex.Message;
            }
            catch (AveExceedStorageLimitException e)
            {
                Logger.Error("AveExceedStorageLimitException. Path:{0}. Message:{1}.", mConfiguration?.SiteCollectionUrl, e.ToString());
                mConfiguration.JobReportDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = e.Message;
            }
            catch (AveSkipLockSiteException ex)
            {
                Logger.Error("AveSkipLockSiteException. Path:{0}. Message:{1}.", mConfiguration.SiteCollectionUrl, ex.ToString());
                mConfiguration.JobReportDto.AddDetailOnly(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, JobDetailsStatus.Failed, mConfiguration.currentRule.Name, ex.Message, string.Empty);
                mConfiguration.ProgressDto.HasErrorNode = true;
                mConfiguration.JobReportDto.summaryComments = ex.Message;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process Disposal Activity Management. EX: {e}");
                mConfiguration.JobReportDto.HasErrorNode = true;
                if (e.Message.StartsWith("Token Result is null"))
                {
                    Logger.Error("Token Result is null.it means that o365 is expired");
                    mConfiguration.JobReportDto.summaryComments = "RM_AR_TokenResult_Null";
                }
                else
                {
                    mConfiguration.JobReportDto.summaryComments = e.Message;
                }
            }
            finally
            {
                await mConfiguration.FlushStubFileRecords();

                if (!mConfiguration.IsTeams && !mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                {
                    mConfiguration.compoundStatistics?.PrepareEndStatistic();
                }

                if (!mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                {
                    JobExecutionProcessStatisticExecutor.Instance.SendReportJobExecutionRecordInfo(JobId, jobContext.MainJobId, mJobType, mConfiguration.JobReportDto);
                    JobExecutionProcessStatisticExecutor.Instance.Dispose();
                }

                UpdateTheRecordStatus();
                if (mJobType == JobType.DiscoverOptimization)
                {
                    try
                    {
                        await _optimizedCalculator.SynchronizeAsync();
                        await _optimizationCalculator.CalculateAsync();
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"some thing went wrong when SynchronizeAsync{e.ToString()}");
                    }
                }
                else if (mJobType == JobType.DiscoveryPlanProOptimization)
                {
                    try
                    {
                        await _optimizedCalculator.SynchronizeAsync();
                        await _optimizationCalculator.CalculateAsync();
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"some thing went wrong when SynchronizeAsync{e.ToString()}");
                    }
                }
                else if (mJobType == JobType.DiscoveryAOSPOptimization)
                {
                    try
                    {
                        if (!UseArchiverProfileForAOSP)
                        {
                            await _aospOptimizedCalculator.SynchronizeAsync();
                            await _aospOptimizationCalculator.CalculateAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"some thing went wrong when SynchronizeAsync{e.ToString()}");
                    }
                }
                CosmosDBManualDataUpdater.WaitComplete();
                UploadDestructionCache();

                if (!mConfiguration.IsTeams && !mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                {
                    mConfiguration.compoundStatistics?.WaitEndStatistic();
                }
                if (mConfiguration.JobReportDto != null)
                {
                    JobExecutionProgressStatisticExecutor.Instance.FinishProgress(mConfiguration.JobReportDto.GetJobStatus());
                    mConfiguration.JobReportDto.FinishReport();
                }
                else
                {
                    JobExecutionProgressStatisticExecutor.Instance.FinishProgress();
                    //jobContext.ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                }
                await TelemetryContext.FlushAsync();
                HSMConnector.ResetInstance();
                CreateLinkFileByPackage.ResetInstance();
                if (mJobType == JobType.ArchiverByHSMXml)
                {
                    await WaitForBackgroundTaskAsync(
                        Task.Run(() => UploadJobMonitorInfo(GetCurrentHsmTraceId())),
                        TimeSpan.FromSeconds(30),
                        "UploadJobMonitorInfo final upload task did not finish within 30 seconds.",
                        "UploadJobMonitorInfo final upload task terminated with error but will be ignored.");
                }
            }
        }

        private void UpdateTheRecordStatus()
        {
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                try
                {
                    var processedNodeLevels = ApprovedDatasSqliteHelper.GetProcessedNodeLevels();
                    if (processedNodeLevels.Count == 0)
                    {
                        Logger.Info("No processed approved data node level was found in SQLite.");
                    }

                    var pendingRecords = ApprovedDatasSqliteHelper.GetPendingRecordsByNodeLevels(processedNodeLevels);
                    if (pendingRecords.Count == 0)
                    {
                        Logger.Info($"No pending approved data records were found by node levels: {string.Join(",", processedNodeLevels)}.");
                        return;
                    }

                    var recordIdentities = pendingRecords
                        .Where(record => record.SiteId != Guid.Empty && record.ItemId != Guid.Empty)
                        .Select(record => Tuple.Create(record.SiteId, record.ItemId))
                        .Distinct()
                        .ToList();

                    mConfiguration.ExplorerDao?.BatchUpdateRecordStatusAndDestroyedTime4Manual(recordIdentities, (int)RMRecordStatus.Destroyed);
                    if (recordIdentities != null && recordIdentities.Count != 0)
                    {
                        try
                        {
                            ManualUtil manualUtil = new ManualUtil(mConfiguration);
                            var recordList = mConfiguration.ExplorerDao.GetRecordsByNodeIds(recordIdentities.Select(a => a.Item2).ToList());
                            foreach (var tempRecord in recordList)
                            {
                                manualUtil.AddManualHistory(tempRecord);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"has error when add manualHistory,error:{e}");
                        }
                    }
                    
                    Logger.Info($"Updated approved data record status by node levels. NodeLevels:{string.Join(",", processedNodeLevels)}, Count:{recordIdentities.Count}.");
                }
                finally
                {
                    ApprovedDatasSqliteHelper.DeleteApprovedDataDbFile();
                }
            }
        }
        public void EndWork()
        {
            Logger.Info("DisposalActivityManagementProcessor EndWork.");
            if (mConfiguration != null)
            {
                try
                {
                    if (mBackups != null && mBackups.Values != null)
                    {
                        foreach (SPObjectBackup backupObj in mBackups.Values)
                        {
                            if (backupObj is IDisposable)
                                using (backupObj) { }
                        }
                    }
                    if (mVaults != null && mVaults.Values != null)
                    {
                        foreach (SPObjectBackup backupObj in mVaults.Values)
                        {
                            if (backupObj is IDisposable)
                                using (backupObj) { }
                        }
                    }
                    if (mRecordManager != null && mRecordManager.Values != null)
                    {
                        foreach (SPObjectBackup backupObj in mRecordManager.Values)
                        {
                            if (backupObj is ListRecordManager)
                            {
                                (backupObj as ListRecordManager).DisposeObj();
                            }
                            else if (backupObj is ItemRecordManager)
                            {
                                (backupObj as ItemRecordManager).DisposeObj();
                            }
                            else if (backupObj is IDisposable)
                                using (backupObj) { }
                        }
                    }
                    //DisposeRelativeDataBackupers();
                }
                catch (Exception e)
                {
                    Logger.Error("Backup Main Dispose Error: {0}.", e.ToString());
                }
            }
        }

        private void SetScheduleSettings(RMSPTreeNode treeNode)
        {

            Logger.Info("Include WorkflowDefinition.");
            SPWorkflowProcessorRuntime.ProcessAssociation = true;

            if (treeNode.IsManagedMetadataService || (treeNode.GetTeamsNode()?.IsManagedMetadataService ?? false) || treeNode.GetGroupNode().IsManagedMetadataService)
            {
                Logger.Info("Include MetadataService.");
                mConfiguration.IncludeMetadataService = true;
            }
            if (treeNode.IsEnableSuperUserDecrypt || (treeNode.GetTeamsNode()?.IsEnableSuperUserDecrypt ?? false) || treeNode.GetGroupNode().IsEnableSuperUserDecrypt)
            {
                WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.Unprotected;
            }
            else
            {
                WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;
            }
            if (treeNode.IsEnableRemoveRetentionLabel || treeNode.Parent.IsEnableRemoveRetentionLabel)
            {
                WrapperConfiguration.EnableRemoveRetentionLabel = true;
            }
            else
            {
                WrapperConfiguration.EnableRemoveRetentionLabel = false;
            }
        }

        private async System.Threading.Tasks.Task SOPreScanJobAsync()
        {
            RMSPTreeNode treeNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting).FirstOrDefault();
            var node = RMDtoConverter.ConvertRMTree2SPTree(treeNode);
            var siteNode = SPTreeNodeManagement.GetSiteCollectionNode(node);
            var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
            var teamsNode = treeNode.GetTeamsNode();
            mConfiguration.IsTeams = teamsNode != null;
            mConfiguration.TeamsId = teamsNode?.TeamsId;
            mConfiguration.TeamsSiteNodeType = siteNode.Type;
            mConfiguration.ContainerId = groupId;
            mConfiguration.SiteCollectionUrl = siteNode.Url;
            mConfiguration.SupportLockedSite = treeNode.SupportLockedSite;
            mConfiguration.SupportArchivedTeams = treeNode.SupportArchivedTeams;
            Logger.Info($"SupportLockedSite: {mConfiguration.SupportLockedSite}, SupportArchivedTeams: {mConfiguration.SupportArchivedTeams}");
            mConfiguration.SiteCollectionID = new Guid(siteNode.ID);
            if (treeNode != null)
            {
                mConfiguration.RunJobNodeLevel = treeNode.Level;
            }
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = GetScopeFullPath(node);
            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                TreeNode = treeNode,
                Configuration = mConfiguration,
            };
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            int i = 1;
            if (mJobType == JobType.SOPreScan || mJobType == JobType.TeamsPreScan)
            {
                rules = GetArhiverRules(treeNode).ToDictionary(v => i++);
            }
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);

            ISharePointScanner scanner = new SOPreViewScanner(scanJobSettings); ;
            using (scanner)
            {
                await scanner.RunAsync();
            }
        }
        private async System.Threading.Tasks.Task ProcessDiscoverOptimizationAsync()
        {
            RMDiscoverOptimizationNode discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationNode>(jobContext.JobContextSetting);
            if (discoverNode != null
                && discoverNode.SettingId == Guid.Empty
                && discoverNode.SiteId == Guid.Empty
                && string.Equals(discoverNode.SiteUrl, "DiscoverOptimizationScope", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("ProcessDiscoverOptimizationAsync temporary passthrough mode for plan-pro compatibility node.");
                await Task.CompletedTask;
                return;
            }
            IRMDiscoveryOffice365OptimizationSettingsInfoDao optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
            var nodeDao = new RMDiscoveryOffice365NodeDao();
            RMDiscoveryOffice365OptimizationSettingsInfo settingInfo = await optimizationSettingsInfoDao.GetSettingInfoByIdAsync(discoverNode.SettingId, discoverNode.O365TenantId);
            RMDiscoveryOffice365OptimizationSetting currentNodeSetting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            Logger.Info($"this job setting id is:{discoverNode.SettingId},tenant id is {discoverNode.O365TenantId}, nodeSetting:{SerializerHelper.SerializeByJsonConvert(currentNodeSetting)}");
            var siteInfo = await nodeDao.GetDiscoverySiteInfoAsync(discoverNode.O365TenantId, discoverNode.SiteId);
            //var discoverRule = ruleInfoDao.GetRuleInfoByRuleIdsAsync();
            _optimizedCalculator = new RMDiscoveryOptimizedCalculator(discoverNode.O365TenantId, siteInfo);
            _optimizationCalculator = new RMDiscoveryOptimizationCalculator(discoverNode.O365TenantId, siteInfo, settingInfo.NextTime);

            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, discoverNode.SiteUrl, mJobType, discoverNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverNode.SiteId;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverNode.sourceFlag == SourceFlag.OneDrive ? true : false;
            mConfiguration.IsTeams = discoverNode.sourceFlag == SourceFlag.Teams;
            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverNode,
                TreeNode = discoverNode.TreeNode
            };
            #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            //rules = GetArhiverRules(treeNode).ToDictionary(v => i++);
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            await InitConfigForDiscoverOptimizationAsync(currentNodeSetting);
            WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;
            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            #endregion
            ISharePointScanner scanner = new DiscoverScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                await scanner.RunAsync();
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                List<Rule> ruleList = new List<Rule>();
                bool hasVersionRule = HasDiscoverOptimizationVersionRule();
                bool hasDocumentRule = HasDiscoverOptimizationDocumentRule();
                var fileRule = GenarateDiscoverRuleSetting(currentNodeSetting, true, currentNodeSetting.MoveToAnotherTierType);
                var versionRule = GenarateDiscoverRuleSetting(currentNodeSetting, false, currentNodeSetting.MoveToAnotherTierType);
                if (!hasVersionRule && !hasDocumentRule)
                {
                    if (fileRule.KeepDataOption == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly)
                    {
                        Logger.Info("this job has no inactive and ROT rule,archive all file,but it is delete only action,so no need get versions");
                        WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
                    }
                    ruleList.Add(fileRule);
                    Logger.Info("this job has no inactive and ROT rule,archive all file");
                }
                else
                {
                    if (hasDocumentRule)
                    {
                        ruleList.Add(fileRule);
                    }
                    if (hasVersionRule)
                    {
                        ruleList.Add(versionRule);
                    }
                }

                if (CheckSiteCollectionShouldSkipBecauseHold(ruleList))
                {
                    return;
                }

                mConfiguration.IsDiscoverOptimization = true;
                bool isDSOJobAndNeedAddOneSiteCollectionDetail = true;
                foreach (var ruleid in ruleList.Select(rule => rule.Id))
                {
                    if (ruleid == fileRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(fileRule);
                        mConfiguration.currentRule = fileRule;
                    }
                    else if (ruleid == versionRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(versionRule);
                        mConfiguration.currentRule = versionRule;
                    }
                    mConfiguration.currentRule.Name = "";
                    mConfiguration.InitOffice365AlertUtil();
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    DiscoverScanner tempScannerForCG = scanner as DiscoverScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);
                    //mConfiguration.ProgressDto.SetActionPhases(mConfiguration.ProgressDto.action);
                    try
                    {
                        bool deleteWithNoBackup = mConfiguration.actionType == ActionType.DeleteOnly || mConfiguration.actionType == ActionType.ExportBeforeDelete || mConfiguration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                        bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfiguration.currentRule);

                        if (isLinkToDucument && deleteWithNoBackup)
                        {
                            var has = await LinkFileCommon.CheckHasRestoreLinkSettings(mConfiguration.currentRule);
                            if (has)
                            {
                                throw new StubUnableGenerateRestoreLinkException();
                            }
                        }
                    }
                    catch (StubUnableGenerateRestoreLinkException stube)
                    {
                        Logger.Error($"StubUnableGenerateRestoreLinkException error {stube}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Item, JobId, mConfiguration.currentRule.Name, "", stube.Message);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"CheckHasRestoreLinkSettings error {e}.");
                    }
                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            _optimizedCalculator.IncreaseArchivedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            _optimizedCalculator.IncreaseDeletedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                    }
                    isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                    if (mConfiguration.mOffice365AlertUtil != null)
                    {
                        mConfiguration.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessDiscoveryPlanProOptimizationAsync()
        {
            RMDiscoverOptimizationNode discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationNode>(jobContext.JobContextSetting);
            if (discoverNode?.PlanProOptimizationSetting == null)
            {
                throw new InvalidOperationException("PlanProOptimizationSetting is required for DiscoveryPlanProOptimization subjob.");
            }

            var currentNodeSetting = discoverNode.PlanProOptimizationSetting;
            currentNodeSetting.O365TenantId = discoverNode.O365TenantId.ToString();

            var nodeDao = new RMDiscoveryOffice365NodeDao();
            var siteInfo = await nodeDao.GetDiscoverySiteInfoAsync(discoverNode.O365TenantId, discoverNode.SiteId);

            _optimizedCalculator = new RMDiscoveryOptimizedCalculator(discoverNode.O365TenantId, siteInfo);
            _optimizationCalculator = new RMDiscoveryOptimizationCalculator(discoverNode.O365TenantId, siteInfo, DateTime.UtcNow.Ticks);

            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, discoverNode.SiteUrl, mJobType, discoverNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverNode.SiteId;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverNode.sourceFlag == SourceFlag.OneDrive;
            mConfiguration.IsTeams = discoverNode.sourceFlag == SourceFlag.Teams;

            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());

            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverNode,
                TreeNode = discoverNode.TreeNode
            };

            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);

            await InitConfigForDiscoverPlanProOptimizationAsync(discoverNode.PlanProRuleDefinitions);
            mConfiguration.RMDiscoveryOptimizationSetting = currentNodeSetting;
            WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;

            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            dataDto.UseDalDataOptimizationService = discoverNode.UseDalDataOptimizationService
                && mJobType == JobType.DiscoveryPlanProOptimization;

            ISharePointScanner scanner = new DiscoverScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                await scanner.RunAsync();
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);

                List<Rule> ruleList = new List<Rule>();
                bool hasVersionRule = HasDiscoverOptimizationVersionRule();
                bool hasDocumentRule = HasDiscoverOptimizationDocumentRule();
                var fileRule = GenarateDiscoverRuleSetting(currentNodeSetting, true, currentNodeSetting.MoveToAnotherTierType);
                var versionRule = GenarateDiscoverRuleSetting(currentNodeSetting, false, currentNodeSetting.MoveToAnotherTierType);
                if (!hasVersionRule && !hasDocumentRule)
                {
                    if (fileRule.KeepDataOption == (int)KeepDataOption.DeleteOnly)
                    {
                        WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
                    }
                    ruleList.Add(fileRule);
                }
                else
                {
                    if (hasDocumentRule)
                    {
                        ruleList.Add(fileRule);
                    }
                    if (hasVersionRule)
                    {
                        ruleList.Add(versionRule);
                    }
                }

                if (CheckSiteCollectionShouldSkipBecauseHold(ruleList))
                {
                    return;
                }

                mConfiguration.IsDiscoverOptimization = true;
                bool isDSOJobAndNeedAddOneSiteCollectionDetail = true;
                foreach (var ruleid in ruleList.Select(rule => rule.Id))
                {
                    if (ruleid == fileRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(fileRule);
                        mConfiguration.currentRule = fileRule;
                    }
                    else if (ruleid == versionRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(versionRule);
                        mConfiguration.currentRule = versionRule;
                    }
                    mConfiguration.currentRule.Name = string.Empty;
                    mConfiguration.InitOffice365AlertUtil();
                    using (new CheckJobStopScope()) { }

                    mConfiguration.ObjectCache.Clear();
                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;

                    DiscoverScanner tempScannerForCG = scanner as DiscoverScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    IEnumerable<ArchiveApproveReport> dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);

                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            _optimizedCalculator.IncreaseArchivedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            _optimizedCalculator.IncreaseDeletedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                    }

                    isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                    if (mConfiguration.mOffice365AlertUtil != null)
                    {
                        mConfiguration.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessDiscoverOptimizationCleanUpDuplicateAsync()
        {
            //RMDiscoverOptimizationNode discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationNode>(jobContext.JobContextSetting);
            //IRMDiscoveryOffice365OptimizationSettingsInfoDao optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
            //var nodeDao = new RMDiscoveryOffice365NodeDao();
            //RMDiscoveryOffice365OptimizationSettingsInfo settingInfo = await optimizationSettingsInfoDao.GetSettingInfoByIdAsync(discoverNode.SettingId, discoverNode.O365TenantId);
            //RMDiscoveryOffice365OptimizationSetting currentNodeSetting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365OptimizationSetting>(RMDiscoveryOffice365OptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            //Logger.Info($"this job setting id is:{discoverNode.SettingId},tenant id is {discoverNode.O365TenantId}, nodeSetting:{SerializerHelper.SerializeByJsonConvert(currentNodeSetting)}");
            //var siteInfo = await nodeDao.GetDiscoverySiteInfoAsync(discoverNode.O365TenantId, discoverNode.SiteId);
            ////var discoverRule = ruleInfoDao.GetRuleInfoByRuleIdsAsync();
            //_optimizedCalculator = new RMDiscoveryOptimizedCalculator(discoverNode.O365TenantId, siteInfo);
            //_optimizationCalculator = new RMDiscoveryOptimizationCalculator(discoverNode.O365TenantId, siteInfo, settingInfo.NextTime);
            CleanUpDuplicateDatasDto storageInfo = new CleanUpDuplicateDatasDto();
            var tenantConfiguration = await _configurationDao.GetValueAsync<RMDiscoveryOffice365CleanupInfo>(new Guid(mRemoteNode.TenantId), RMDiscoveryO365TenantConfigurationType.DuplicationReportConfiguration);
            if (tenantConfiguration != null)
            {
                storageInfo.StorageInfo = new StorageDeviceUIDto() { Id = tenantConfiguration.StoragePolicyId, Name = tenantConfiguration.StoragePolicyName };
            }
            Logger.Info($"start ProcessDiscoverOptimizationCleanUpDuplicateAsync,siteurl:{mRemoteNode.url},item count:{mCleanUpItemEntrys?.Count}");
            RMDiscoverOptimizationNode discoverNode = GenerateDiscoverNodeForDuplicate();
            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, mRemoteNode.url, mJobType, mRemoteNode.ObjectId.ToString());
            mConfiguration.SiteCollectionUrl = mRemoteNode.url;
            mConfiguration.SiteCollectionID = new Guid(mRemoteNode.ObjectId);
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            mConfiguration.IsProcessDuplicateDatas = true;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverNode.SiteUrl.Replace("/", "_");
            //mConfiguration.IsOneDriverSite = discoverNode.sourceFlag == SourceFlag.OneDrive ? true : false;
            //mConfiguration.IsTeams = discoverNode.sourceFlag == SourceFlag.Teams;
            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverNode,
                TreeNode = discoverNode.TreeNode
            };
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;
            ISharePointScanner scanner = new DiscoverScanner(scanJobSettings, new RMDiscoveryOptimizeDataSettingDto() { O365TenantId = mRemoteNode.TenantId }, mCleanUpItemEntrys);
            using (scanner)
            {
                await scanner.RunAsync();
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                List<Rule> ruleList = new List<Rule>();
                bool hasDeleteOnlyAction = DuplicateDatasHasDeleteOnlyAction();
                bool hasArchiverAndRemoveRule = DuplicateDatasHasArchiverAndRemoveAction();
                var archiverAndRemoveRule = GenarateCleanUpDuplicateDatasRuleSetting(storageInfo.StorageInfo, true, FileAction.ArchiveAndRemove);//default tier
                var deleteOnlyRule = GenarateCleanUpDuplicateDatasRuleSetting(storageInfo.StorageInfo, true, FileAction.Remove);//default tier

                if (hasDeleteOnlyAction)
                {
                    ruleList.Add(deleteOnlyRule);
                }
                if (hasArchiverAndRemoveRule)
                {
                    ruleList.Add(archiverAndRemoveRule);
                }

                if (CheckSiteCollectionShouldSkipBecauseHold(ruleList))
                {
                    return;
                }

                mConfiguration.IsDiscoverOptimization = true;
                bool isDSOJobAndNeedAddOneSiteCollectionDetail = true;
                foreach (var ruleid in ruleList.Select(rule => rule.Id))
                {
                    if (ruleid == archiverAndRemoveRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(archiverAndRemoveRule);
                        mConfiguration.currentRule = archiverAndRemoveRule;
                    }
                    else if (ruleid == deleteOnlyRule.Id)
                    {
                        await ReBuildArchiveRuleAsync(deleteOnlyRule);
                        mConfiguration.currentRule = deleteOnlyRule;
                    }
                    mConfiguration.currentRule.Name = "";
                    mConfiguration.InitOffice365AlertUtil();
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    DiscoverScanner tempScannerForCG = scanner as DiscoverScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);
                    //mConfiguration.ProgressDto.SetActionPhases(mConfiguration.ProgressDto.action);
                    try
                    {
                        bool deleteWithNoBackup = mConfiguration.actionType == ActionType.DeleteOnly || mConfiguration.actionType == ActionType.ExportBeforeDelete || mConfiguration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                        bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfiguration.currentRule);

                        if (isLinkToDucument && deleteWithNoBackup)
                        {
                            var has = await LinkFileCommon.CheckHasRestoreLinkSettings(mConfiguration.currentRule);
                            if (has)
                            {
                                throw new StubUnableGenerateRestoreLinkException();
                            }
                        }
                    }
                    catch (StubUnableGenerateRestoreLinkException stube)
                    {
                        Logger.Error($"StubUnableGenerateRestoreLinkException error {stube}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Item, JobId, mConfiguration.currentRule.Name, "", stube.Message);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"CheckHasRestoreLinkSettings error {e}.");
                    }
                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            //_optimizedCalculator.IncreaseArchivedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer, isDSOJobAndNeedAddOneSiteCollectionDetail);
                            //_optimizedCalculator.IncreaseDeletedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            break;
                    }
                    isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                    if (mConfiguration.mOffice365AlertUtil != null)
                    {
                        mConfiguration.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                    }
                }
            }
        }
        private bool DuplicateDatasHasDeleteOnlyAction()
        {
            if (mCleanUpItemEntrys != null && mCleanUpItemEntrys.Count > 0)
            {
                if (mCleanUpItemEntrys.Any(a => a.Action == ArchiveConstants.DestroyAction))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool DuplicateDatasHasArchiverAndRemoveAction()
        {
            if (mCleanUpItemEntrys != null && mCleanUpItemEntrys.Count > 0)
            {
                if (mCleanUpItemEntrys.Any(a => a.Action == ArchiveConstants.ArchiveAction))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private RMDiscoverOptimizationNode GenerateDiscoverNodeForDuplicate()
        {
            RMDiscoverOptimizationNode tempNode = new RMDiscoverOptimizationNode
            {
                SiteUrl = mRemoteNode.url,
                //SiteInfoId = mRemoteNode.Id,
                SiteId = new Guid(mRemoteNode.ObjectId),
                //sourceFlag = siteInfo.ContentSource,
                //SettingId = jobParaInfo.settingInfo.SettingId,
                O365TenantId = new Guid(mRemoteNode.TenantId)
            };
            RMSPTreeNode treeNode = new RMSPTreeNode();
            treeNode.SPObjectId = tempNode.SiteId.ToString();
            treeNode.O365TenantId = mRemoteNode.TenantId;
            treeNode.SiteId = tempNode.SiteId;
            treeNode.Level = 100;//siteCollection
            treeNode.FullPath = tempNode.SiteUrl;
            tempNode.TreeNode = treeNode;
            return tempNode;
        }
        public bool CheckSiteCollectionShouldSkipBecauseHold(IEnumerable<Rule> rules)
        {
            if (!ArchiveJobLimitCollection.ShouldCheckSiteHoldJobTypeSet.Contains(mConfiguration.jobtype))
            {
                Logger.Info("not need Check SiteCollection Should Skip Because Hold , job type not in special scope");
                return false;
            }

            if (!rules.Any(rule => RuleHelper.CheckIsWillDeleteDataAction(rule)))
            {
                Logger.Info("not need chekc SiteCollection Should Skip Because Hold , rule not cintains delete item action");
                return false;
            }

            AveObjectModelFactory factory = mConfiguration.aveObjectModelFactory;
            IAveSite aveSite = factory.CreateSite(mConfiguration.SiteCollectionUrl);
            if (aveSite == null)
            {
                throw new ArchiverCommon.SPObjectNotFoundException(LOGRESOURCE.StorageOptimization13_SOARScanCaculateSiteListCount, "SiteCollection", mConfiguration.SiteCollectionUrl);
            }
            if (aveSite.HasHolds)
            {
                Logger.Warn($"Site collection {mConfiguration.SiteCollectionUrl} is on hold, skip it.");
                if (RMKeyValueDao.EnableArchiveHoldSiteCollection())
                {
                    Logger.Warn($"Archive hold site collection is enabled, will not skip site collection.");
                }
                else
                {
                    Logger.Warn($"Archive hold site collection is un enabled, skip site collection.");
                    mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Skipped, "RM_JM_SiteCollectionHoldAndHaveDeletRule_ErrorMessage");
                    return true;
                }
            }
            return false;
        }

        private bool IsSiteLockedForHold(string siteUrl)
        {
            try
            {
                IAveSite aveSite = mConfiguration.aveObjectModelFactory.CreateSite(siteUrl);
                return aveSite != null && aveSite.HasHolds;
            }
            catch (Exception ex)
            {
                Logger.Warn($"[IsSiteLockedForHold] Could not determine hold state for {siteUrl}. Error: {ex}");
                return false;
            }
        }

        private async System.Threading.Tasks.Task ProcessDiscoverAOSPOptimizationAsync()
        {
            IRMDiscoveryAOSPOptimizationSettingsInfoDao optimizationSettingsInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();
            var discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationNode>(jobContext.JobContextSetting);
            RMDiscoveryAOSPOptimizationSetting currentNodeSetting = new RMDiscoveryAOSPOptimizationSetting();
            RMDiscoveryAOSPOptimizationSettingsInfo settingInfo = new RMDiscoveryAOSPOptimizationSettingsInfo();
            if (discoverNode.ArchiverProfileSetting != null && discoverNode.ArchiverProfileSetting.UseArchiverProfile)
            {
                Logger.Info("this is aosp RunAOSPDiscoverSO,discoverNode.ArchiverProfileSetting is not null");
                currentNodeSetting = discoverNode.ArchiverProfileSetting;
                UseArchiverProfileForAOSP = true;
            }
            else
            {
                settingInfo = await optimizationSettingsInfoDao.GetSettingInfoByIdAsync(discoverNode.SettingId, discoverNode.O365TenantId);
                currentNodeSetting = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryAOSPOptimizationSetting>(RMDiscoveryAOSPOptimizationSetting.XMLCompatibleConvert(settingInfo.Setting));
            }
            if (!discoverNode.SupportLockedSite)
            {
                discoverNode.SupportLockedSite = currentNodeSetting.SupportLockedSite;
            }
            Logger.Info($"AOSP optimization setting loaded for processing. SettingId:{discoverNode.SettingId}, SiteId:{discoverNode.SiteId}, NodeSupportLockedSite:{discoverNode.SupportLockedSite}, SettingSupportLockedSite:{currentNodeSetting.SupportLockedSite}");
            if (currentNodeSetting.UseArchiverProfile)
            {
                Logger.Info("this is aosp RunAOSPArchiverSO");
                await RunAOSPArchiverSO(discoverNode, currentNodeSetting);
            }
            else
            {
                Logger.Info("this is aosp RunAOSPDiscoverSO");
                await RunAOSPDiscoverSO(discoverNode, settingInfo, currentNodeSetting);
            }
        }
        private async Task RunAOSPDiscoverSO(RMDiscoverOptimizationNode discoverNode, RMDiscoveryAOSPOptimizationSettingsInfo settingInfo, RMDiscoveryAOSPOptimizationSetting currentNodeSetting)
        {
            var nodeDao = new RMDiscoveryAOSPNodeDao();
            var jobDao = new RMDiscoveryAOSPJobDao();

            Logger.Info($"this job setting id is:{discoverNode.SettingId},tenant id is {discoverNode.O365TenantId}");
            var siteInfo = await nodeDao.GetDiscoverySiteInfoAsync(discoverNode.O365TenantId, discoverNode.SiteId);
            _aospOptimizedCalculator = new RMDiscoveryAOSPOptimizedCalculator(discoverNode.O365TenantId, siteInfo);
            _aospOptimizationCalculator = new RMDiscoveryAOSPOptimizationCalculator(discoverNode.O365TenantId, siteInfo, settingInfo.NextTime);

            SOArchiverJobInfoStatistics.Instance.InitAOSPInstance(JobId, discoverNode.SiteUrl, mJobType, discoverNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverNode.SiteId;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            mConfiguration.SupportLockedSite = discoverNode.TreeNode?.SupportLockedSite == true || discoverNode.SupportLockedSite;
            Logger.Info($"SupportLockedSite: {mConfiguration.SupportLockedSite}");
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverNode.sourceFlag == SourceFlag.OneDrive ? true : false;
            WrapperConfiguration.WrapperConfigurationForBPOS.O365TenantId = discoverNode.O365TenantId;
            await InitAOSPSiteInfoScheduleConfigAsync(mConfiguration, discoverNode.O365TenantId.ToString(), currentNodeSetting.AppProfileId, currentNodeSetting.SiteAdminUrl);
            mConfiguration.JobReportDto.AddScanReport(discoverNode.SiteUrl, 0, (int)CacheNodeType.SiteCollection, "");
            ScanDataCache.Instance.Initialize(mConfiguration.IsOneDriverSite);
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverNode,
                TreeNode = discoverNode.TreeNode,
                AppProfileId = currentNodeSetting.AppProfileId,
                SiteAdminUrl = currentNodeSetting.SiteAdminUrl,
            };
            #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            //rules = GetArhiverRules(treeNode).ToDictionary(v => i++);
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            await InitConfigForDiscoverOptimizationAsync(discoverNode.O365TenantId.ToString(), currentNodeSetting);
            WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;
            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            #endregion
            ISharePointScanner scanner = new DiscoveryAOSPScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                List<string> ruleIds = new List<string>();
                bool hasVersionRule = HasAOSPDiscoverOptimizationVersionRule();
                bool hasDocumentRule = HasAOSPDiscoverOptimizationDocumentRule();
                var fileRule = GenarateDiscoverRuleSetting(currentNodeSetting, true, currentNodeSetting.MoveToAnotherTierType);
                var versionRule = GenarateDiscoverRuleSetting(currentNodeSetting, false, currentNodeSetting.MoveToAnotherTierType);
                if (!hasVersionRule && !hasDocumentRule)
                {
                    if (fileRule.KeepDataOption == (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly)
                    {
                        Logger.Info("this job has no inactive and ROT rule,archive all file,but it is delete only action,so no need get versions");
                        WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
                    }
                    ruleIds.Add(fileRule.Id);
                    Logger.Info("this job has no inactive and ROT rule,archive all file");
                }
                else
                {
                    if (hasDocumentRule)
                    {
                        ruleIds.Add(fileRule.Id);
                    }
                    if (hasVersionRule)
                    {
                        ruleIds.Add(versionRule.Id);
                    }
                }
                mConfiguration.IsDiscoverOptimization = true;
                foreach (var ruleid in ruleIds)
                {
                    if (ruleid == fileRule.Id)
                    {
                        await ReBuildAOSPArchiveRuleAsync(fileRule);
                        mConfiguration.currentRule = fileRule;
                    }
                    else if (ruleid == versionRule.Id)
                    {
                        await ReBuildAOSPArchiveRuleAsync(versionRule);
                        mConfiguration.currentRule = versionRule;
                    }
                    mConfiguration.currentRule.Name = "";
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    DiscoveryAOSPScanner tempScannerForCG = scanner as DiscoveryAOSPScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);
                    try
                    {
                        bool deleteWithNoBackup = mConfiguration.actionType == ActionType.DeleteOnly || mConfiguration.actionType == ActionType.ExportBeforeDelete || mConfiguration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                        bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfiguration.currentRule);

                        if (isLinkToDucument && deleteWithNoBackup)
                        {
                            var has = await LinkFileCommon.CheckHasRestoreLinkSettings(mConfiguration.currentRule);
                            if (has)
                            {
                                throw new StubUnableGenerateRestoreLinkException();
                            }
                        }
                    }
                    catch (StubUnableGenerateRestoreLinkException stube)
                    {
                        Logger.Error($"StubUnableGenerateRestoreLinkException error {stube}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Item, JobId, mConfiguration.currentRule.Name, "", stube.Message);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"CheckHasRestoreLinkSettings error {e}.");
                    }
                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            _aospOptimizedCalculator.IncreaseArchivedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            _aospOptimizedCalculator.IncreaseArchivedFileCount(SOArchiverJobInfoStatistics.Instance.ItemCount);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            _aospOptimizedCalculator.IncreaseDeletedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            _aospOptimizedCalculator.IncreaseDeletedFileCount(SOArchiverJobInfoStatistics.Instance.ItemCount);
                            break;
                    }
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                }

            }

        }
        private async Task RunAOSPArchiverSO(RMDiscoverOptimizationNode discoverNode, RMDiscoveryAOSPOptimizationSetting currentNodeSetting)
        {
            var nodeDao = new RMDiscoveryAOSPNodeDao();
            Logger.Info($"RunAOSPArchiverSO this job setting id is:{discoverNode.SettingId},tenant id is {discoverNode.O365TenantId}");
            //var siteInfo = await nodeDao.GetDiscoverySiteInfoAsync(discoverNode.O365TenantId, discoverNode.SiteId);
            //_aospOptimizedCalculator = new RMDiscoveryAOSPOptimizedCalculator(discoverNode.O365TenantId, siteInfo);
            //_aospOptimizationCalculator = new RMDiscoveryAOSPOptimizationCalculator(discoverNode.O365TenantId, siteInfo, DateTime.UtcNow.Ticks);

            SOArchiverJobInfoStatistics.Instance.InitAOSPInstance(JobId, discoverNode.SiteUrl, mJobType, discoverNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverNode.SiteId;
            mConfiguration.UseAospArchiverProfile = true;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            mConfiguration.SupportLockedSite = discoverNode.TreeNode?.SupportLockedSite == true || discoverNode.SupportLockedSite;
            Logger.Info($"SupportLockedSite: {mConfiguration.SupportLockedSite}");
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.JobReportDto = new JobReportImps(jobContext.ReportManager);
            mConfiguration.ProgressDto = mConfiguration.JobReportDto;
            mConfiguration.ScopePath = discoverNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverNode.sourceFlag == SourceFlag.OneDrive ? true : false;
            await InitAOSPSiteInfoScheduleConfigAsync(mConfiguration, discoverNode.O365TenantId.ToString(), currentNodeSetting.AppProfileId, currentNodeSetting.SiteAdminUrl);
            mConfiguration.JobReportDto.AddScanReport(discoverNode.SiteUrl, 0, (int)CacheNodeType.SiteCollection, "");
            ScanDataCache.Instance.Initialize(mConfiguration.IsOneDriverSite);
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverNode,
                TreeNode = discoverNode.TreeNode,
                AppProfileId = currentNodeSetting.AppProfileId,
                SiteAdminUrl = currentNodeSetting.SiteAdminUrl,
            };
            #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            //rules = GetArhiverRules(treeNode).ToDictionary(v => i++);
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            await InitConfigForDiscoverOptimizationAsync(discoverNode.O365TenantId.ToString(), currentNodeSetting);
            WrapperConfiguration.OpenBinaryOptions = AveOpenBinaryOptions.None;
            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            #endregion
            ISharePointScanner scanner = new DiscoveryAOSPScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                await scanner.RunAsync();
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                var ruleIds = dataReader.GetAllRuleIds();
                //string ruleid = currentNodeSetting.ArchiverProfileId;
                //var fileRule = GenarateArchiverRuleSetting(currentNodeSetting, currentNodeSetting.MoveToAnotherTierType);
                foreach (var ruleid in ruleIds)
                {
                    //if (ruleid == fileRule.Id)
                    //{
                    var temp = mConfiguration.RuleCollection.Where(r => r.Value.Id == ruleid).FirstOrDefault();
                    var r = temp.Value;
                    GenarateArchiverRuleSetting(r, currentNodeSetting);
                    await ReBuildAOSPArchiveRuleAsync(r);
                    mConfiguration.tempListId = Guid.Empty;//SAAS-15676,RECO-19845,多个rule的情况下应该每个rule都reload content type
                    mConfiguration.currentRule = r;
                    mConfiguration.currentRule.Order = temp.Key;
                    mConfiguration.InitOffice365AlertUtil();
                    //}
                    //else if (ruleid == versionRule.Id)
                    //{
                    //    await ReBuildAOSPArchiveRuleAsync(versionRule);
                    //    mConfiguration.currentRule = versionRule;
                    //}
                    //mConfiguration.currentRule.Name = "";
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    //DiscoveryAOSPScanner tempScannerForCG = scanner as DiscoveryAOSPScanner;
                    //RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    //dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());
                    dataEnumer = dataReader.GetArchiveApproveReports(ruleid);
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);
                    try
                    {
                        bool deleteWithNoBackup = mConfiguration.actionType == ActionType.DeleteOnly || mConfiguration.actionType == ActionType.ExportBeforeDelete || mConfiguration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                        bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfiguration.currentRule);

                        if (isLinkToDucument && deleteWithNoBackup)
                        {
                            var has = await LinkFileCommon.CheckHasRestoreLinkSettings(mConfiguration.currentRule);
                            if (has)
                            {
                                throw new StubUnableGenerateRestoreLinkException();
                            }
                        }
                    }
                    catch (StubUnableGenerateRestoreLinkException stube)
                    {
                        Logger.Error($"StubUnableGenerateRestoreLinkException error {stube}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Item, JobId, mConfiguration.currentRule.Name, "", stube.Message);
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"CheckHasRestoreLinkSettings error {e}.");
                    }
                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            //_aospOptimizedCalculator.IncreaseArchivedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            //_aospOptimizedCalculator.IncreaseArchivedFileCount(SOArchiverJobInfoStatistics.Instance.ItemCount);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            //_aospOptimizedCalculator.IncreaseDeletedSize(SOArchiverJobInfoStatistics.Instance.ItemSizeSum);
                            //_aospOptimizedCalculator.IncreaseDeletedFileCount(SOArchiverJobInfoStatistics.Instance.ItemCount);
                            break;
                    }
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                }

            }

        }
        private async System.Threading.Tasks.Task ProcessDiscoverOptimizationPreScanAsync()
        {
            RMDiscoverOptimizationPreScanNode discoverPreScanNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationPreScanNode>(jobContext.JobContextSetting);
            if (discoverPreScanNode != null
                && discoverPreScanNode.Setting == null
                && discoverPreScanNode.SiteId == Guid.Empty
                && string.Equals(discoverPreScanNode.SiteUrl, "DiscoverOptimizationScope", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info("ProcessDiscoverOptimizationPreScanAsync temporary passthrough mode for plan-pro compatibility node.");
                await Task.CompletedTask;
                return;
            }
            RMDiscoveryOffice365OptimizationSetting currentNodeSetting = discoverPreScanNode.Setting;
            Logger.Info($"this job,tenant id is {discoverPreScanNode.O365TenantId}");
            mDiscoveryOffice365RuleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, discoverPreScanNode.SiteUrl, mJobType, discoverPreScanNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverPreScanNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverPreScanNode.SiteId;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverPreScanNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverPreScanNode.sourceFlag == SourceFlag.OneDrive ? true : false;
            mConfiguration.IsTeams = discoverPreScanNode.sourceFlag == SourceFlag.Teams;
            mConfiguration.IsDiscoverOptimizationPreScan = true;
            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverPreScanNode,
                TreeNode = discoverPreScanNode.TreeNode
            };
            #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();

            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            await InitConfigForDiscoverOptimizationAsync(currentNodeSetting);
            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            #endregion
            ISharePointScanner scanner = new DiscoverScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                await scanner.RunAsync();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount);
                bool hasVersionRule = HasDiscoverOptimizationVersionRule();
                bool hasDocumentRule = HasDiscoverOptimizationDocumentRule();
                var fileRule = GenarateDiscoverRuleSetting(currentNodeSetting, true, currentNodeSetting.MoveToAnotherTierType);
                var versionRule = GenarateDiscoverRuleSetting(currentNodeSetting, false, currentNodeSetting.MoveToAnotherTierType);
                if (!hasVersionRule && !hasDocumentRule)
                {
                    await ReBuildArchiveRuleAsync(fileRule);
                    mConfiguration.RuleCollection.Add((int)fileRule.PolicyLevel, fileRule);
                    Logger.Info("this job has no inactive and ROT rule,scan all file");
                }
                else
                {
                    if (hasDocumentRule)
                    {
                        await ReBuildArchiveRuleAsync(fileRule);
                        mConfiguration.RuleCollection.Add((int)fileRule.PolicyLevel, fileRule);
                    }
                    if (hasVersionRule)
                    {
                        await ReBuildArchiveRuleAsync(versionRule);
                        mConfiguration.RuleCollection.Add((int)versionRule.PolicyLevel, versionRule);
                    }
                }
                mConfiguration.IsDiscoverOptimization = true;
                IEnumerable<string> ruleIds = mConfiguration.RuleCollection.Values.Select(r => r.Id);

                if (CheckSiteCollectionShouldSkipBecauseHold(mConfiguration.RuleCollection.Values))
                {
                    return;
                }
                foreach (var ruleid in ruleIds)
                {
                    if (ruleid == fileRule.Id)
                    {
                        mConfiguration.currentRule = fileRule;
                    }
                    else if (ruleid == versionRule.Id)
                    {
                        mConfiguration.currentRule = versionRule;
                    }
                    mConfiguration.currentRule.Name = "";
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    DiscoverScanner tempScannerForCG = scanner as DiscoverScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    IEnumerable<ArchiveApproveReport> dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());

                    foreach (ArchiveApproveReport entity in dataEnumer)
                    {
                        using (new CheckJobStopScope()) { }
                        if (hasVersionRule && ruleid == versionRule?.Id && entity.CacheNodeType == (int)CacheNodeType.Item)
                        {
                            continue;
                        }
                        mConfiguration.JobReportDto.AddScanReport(mConfiguration.GetNodeFullPath(entity.FullPath), entity.DocumentSize, (int)entity.CacheNodeType, "");
                        mConfiguration.ProgressDto.UpdateProgress();
                    }
                    SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
                    SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();

                }
                mConfiguration.JobReportDto.SendAndWaitFlushAllReport();
                if (hasDocumentRule && hasVersionRule)
                {
                    jobDetailService.RemoveDuplicateDataOfJobDetails(new AvePoint.RA.Contract.JobMonitor.BaseJobDto { Id = JobId, JobType = (int)mJobType });
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessDiscoveryPlanProScanAsync()
        {
            RMDiscoverOptimizationPreScanNode discoverPreScanNode = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationPreScanNode>(jobContext.JobContextSetting);
            if (discoverPreScanNode?.PlanProOptimizationSetting == null && discoverPreScanNode?.Setting == null)
            {
                throw new InvalidOperationException("PlanProOptimizationSetting is required for DiscoveryPlanProScan subjob.");
            }

            RMDiscoveryOffice365OptimizationSetting currentNodeSetting = discoverPreScanNode.PlanProOptimizationSetting ?? discoverPreScanNode.Setting;
            currentNodeSetting.O365TenantId = discoverPreScanNode.O365TenantId.ToString();

            Logger.Info($"ProcessDiscoveryPlanProScanAsync real mode. site:{discoverPreScanNode.SiteUrl}, tenant:{discoverPreScanNode.O365TenantId}");
            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, discoverPreScanNode.SiteUrl, mJobType, discoverPreScanNode.SiteId.ToString());
            mConfiguration.SiteCollectionUrl = discoverPreScanNode.SiteUrl;
            mConfiguration.SiteCollectionID = discoverPreScanNode.SiteId;
            mConfiguration.RunJobNodeLevel = (int)NodeLevel.SiteCollection;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = discoverPreScanNode.SiteUrl.Replace("/", "_");
            mConfiguration.IsOneDriverSite = discoverPreScanNode.sourceFlag == SourceFlag.OneDrive;
            mConfiguration.IsTeams = discoverPreScanNode.sourceFlag == SourceFlag.Teams;
            mConfiguration.IsDiscoverOptimizationPreScan = true;

            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                Configuration = mConfiguration,
                DiscoverNode = discoverPreScanNode,
                TreeNode = discoverPreScanNode.TreeNode
            };

            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            mConfiguration.RuleCollection = rules;
            OutPutRuleInfoIntoAgentLog(rules);
            await InitConfigForDiscoverPlanProOptimizationAsync(discoverPreScanNode.PlanProRuleDefinitions);
            mConfiguration.RMDiscoveryOptimizationSetting = currentNodeSetting;
            var dataDto = await ConvertDiscoverPanalSettingToDto(currentNodeSetting);
            dataDto.UseDalDataOptimizationService = discoverPreScanNode.UseDalDataOptimizationService
                && mJobType == JobType.DiscoveryPlanProScan;

            ISharePointScanner scanner = new DiscoverScanner(scanJobSettings, dataDto);
            using (scanner)
            {
                await scanner.RunAsync();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount);
                bool hasVersionRule = HasDiscoverOptimizationVersionRule();
                bool hasDocumentRule = HasDiscoverOptimizationDocumentRule();
                var fileRule = GenarateDiscoverRuleSetting(currentNodeSetting, true, currentNodeSetting.MoveToAnotherTierType);
                var versionRule = GenarateDiscoverRuleSetting(currentNodeSetting, false, currentNodeSetting.MoveToAnotherTierType);
                if (!hasVersionRule && !hasDocumentRule)
                {
                    await ReBuildArchiveRuleAsync(fileRule);
                    mConfiguration.RuleCollection.Add((int)fileRule.PolicyLevel, fileRule);
                    Logger.Info("this plan pro scan job has no inactive and ROT rule,scan all file");
                }
                else
                {
                    if (hasDocumentRule)
                    {
                        await ReBuildArchiveRuleAsync(fileRule);
                        mConfiguration.RuleCollection.Add((int)fileRule.PolicyLevel, fileRule);
                    }
                    if (hasVersionRule)
                    {
                        await ReBuildArchiveRuleAsync(versionRule);
                        mConfiguration.RuleCollection.Add((int)versionRule.PolicyLevel, versionRule);
                    }
                }
                mConfiguration.IsDiscoverOptimization = true;
                IEnumerable<string> ruleIds = mConfiguration.RuleCollection.Values.Select(r => r.Id);

                if (CheckSiteCollectionShouldSkipBecauseHold(mConfiguration.RuleCollection.Values))
                {
                    return;
                }
                foreach (var ruleid in ruleIds)
                {
                    if (ruleid == fileRule.Id)
                    {
                        mConfiguration.currentRule = fileRule;
                    }
                    else if (ruleid == versionRule.Id)
                    {
                        mConfiguration.currentRule = versionRule;
                    }
                    mConfiguration.currentRule.Name = "";
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    mConfiguration.ObjectCache.Clear();

                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;
                    DiscoverScanner tempScannerForCG = scanner as DiscoverScanner;
                    RunDiscoverOptimizationScan(ruleid, tempScannerForCG);
                    IEnumerable<ArchiveApproveReport> dataEnumer = new CLReader(mConfiguration, tempScannerForCG.GetPCContainer());

                    foreach (ArchiveApproveReport entity in dataEnumer)
                    {
                        using (new CheckJobStopScope()) { }
                        if (hasVersionRule && ruleid == versionRule?.Id && entity.CacheNodeType == (int)CacheNodeType.Item)
                        {
                            continue;
                        }
                        mConfiguration.JobReportDto.AddScanReport(mConfiguration.GetNodeFullPath(entity.FullPath), entity.DocumentSize, (int)entity.CacheNodeType, "");
                        mConfiguration.ProgressDto.UpdateProgress();
                    }
                    SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
                    SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();

                }
                mConfiguration.JobReportDto.SendAndWaitFlushAllReport();
                if (hasDocumentRule && hasVersionRule)
                {
                    jobDetailService.RemoveDuplicateDataOfJobDetails(new AvePoint.RA.Contract.JobMonitor.BaseJobDto { Id = JobId, JobType = (int)mJobType });
                }
            }
        }

        private bool HasDiscoverOptimizationVersionRule()
        {
            if (mConfiguration.InactiveDiscoveryRuleInfos != null && mConfiguration.InactiveDiscoveryRuleInfos.Count > 0)
            {
                return true;
            }
            else if (mConfiguration.ROTDiscoveryRuleInfos != null && mConfiguration.ROTDiscoveryRuleInfos.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version).ToList().Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HasAOSPDiscoverOptimizationVersionRule()
        {
            if (mConfiguration.InactiveDiscoveryAOSPRuleInfos != null && mConfiguration.InactiveDiscoveryAOSPRuleInfos.Count > 0)
            {
                return true;
            }
            else if (mConfiguration.ROTDiscoveryAOSPRuleInfos != null && mConfiguration.ROTDiscoveryAOSPRuleInfos.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version).ToList().Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HasDiscoverOptimizationDocumentRule()
        {
            if (mConfiguration.ROTDiscoveryRuleInfos != null && mConfiguration.ROTDiscoveryRuleInfos.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document).ToList().Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool HasAOSPDiscoverOptimizationDocumentRule()
        {
            if (mConfiguration.ROTDiscoveryAOSPRuleInfos != null && mConfiguration.ROTDiscoveryAOSPRuleInfos.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document).ToList().Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<RMDiscoveryOptimizeDataSettingDto> ConvertDiscoverPanalSettingToDto(RMDiscoveryOffice365OptimizationSetting panalSetting)
        {
            RMDiscoveryOptimizeDataSettingDto result = new();
            result.ArchiveDataType = panalSetting.ArchiveDataType;
            result.DataType = panalSetting.DataType;
            result.SelectedStorage = panalSetting.SelectedStorage;
            result.NodeQueryParameter = panalSetting.NodeQueryParameter;
            result.O365TenantId = panalSetting.O365TenantId;
            result.SizeRangeQueryParameter = panalSetting.SizeRangeQueryParameter;
            result.WithoutDateQueryParameter = panalSetting.WithoutDateQueryParameter;
            result.FileExtensionQueryParameter = panalSetting.FileExtensionQueryParameter;
            result.ScheduleParameter = panalSetting.ScheduleParameter;
            result.ProcessActionParameter = panalSetting.ProcessActionParameter;
            if (mConfiguration.ROTDiscoveryRuleInfos != null && mConfiguration.ROTDiscoveryRuleInfos.Count > 0)
            {
                result.ROTRule = RMDiscoveryRuleConverter.Convert(mConfiguration.ROTDiscoveryRuleInfos);
            }
            if (mConfiguration.InactiveDiscoveryRuleInfos != null && mConfiguration.InactiveDiscoveryRuleInfos.Count > 0)
            {
                result.InactiveRule = RMDiscoveryRuleConverter.Convert(mConfiguration.InactiveDiscoveryRuleInfos);
            }
            return result;
        }
        public async Task<RMDiscoveryAOSPOptimizeDataSettingDto> ConvertDiscoverPanalSettingToDto(RMDiscoveryAOSPOptimizationSetting panalSetting)
        {
            RMDiscoveryAOSPOptimizeDataSettingDto result = new();
            result.ArchiveDataType = panalSetting.ArchiveDataType;
            result.DataType = panalSetting.DataType;
            result.SelectedStorage = new StorageDeviceUIDto { Id = panalSetting.SelectedStorage.Id, Name = panalSetting.SelectedStorage.Name };
            result.NodeQueryParameter = panalSetting.NodeQueryParameter;
            result.O365TenantId = panalSetting.O365TenantId;
            result.SizeRangeQueryParameter = panalSetting.SizeRangeQueryParameter;
            result.WithoutDateQueryParameter = panalSetting.WithoutDateQueryParameter;
            result.FileExtensionQueryParameter = panalSetting.FileExtensionQueryParameter;
            result.ScheduleParameter = panalSetting.ScheduleParameter;
            result.ProcessActionParameter = panalSetting.ProcessActionParameter;
            if (mConfiguration.ROTDiscoveryAOSPRuleInfos != null && mConfiguration.ROTDiscoveryAOSPRuleInfos.Count > 0)
            {
                result.ROTRule = RMDiscoveryRuleConverter.Convert(mConfiguration.ROTDiscoveryAOSPRuleInfos);
            }
            if (mConfiguration.InactiveDiscoveryAOSPRuleInfos != null && mConfiguration.InactiveDiscoveryAOSPRuleInfos.Count > 0)
            {
                result.InactiveRule = RMDiscoveryRuleConverter.Convert(mConfiguration.InactiveDiscoveryAOSPRuleInfos);
            }
            return result;
        }
        private Rule GenarateHSMRuleSetting(StorageDeviceUIDto storage, string stubTemplateId)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.Name = "";
            //rule.Description = info.Description;
            //rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            //rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            rule.StoragePolicyId = storage.Id;
            rule.StoragePolicyName = storage.Name;
            //rule.StubTemplateId = info.ProcessActionParameter.StubSettingDto?.Id;
            //rule.StubTemplateName = info.ProcessActionParameter.StubSettingDto?.Name;
            //rule.MoveToAnotherTierType = moveDataTierType;
            //rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
            if (!string.IsNullOrEmpty(stubTemplateId))
            {
                rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                rule.StubTemplateId = stubTemplateId;
            }
            else
            {
                rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove;
            }

            rule.PolicyLevel = PolicyLevel.Document;

            RebuildStubSettingsAsync(rule).GetAwaiter().GetResult();
            return rule;
        }
        private Rule GenarateCleanUpDuplicateDatasRuleSetting(StorageDeviceUIDto selectedStorage, bool isFileLevelAction, FileAction action)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            rule.StoragePolicyId = selectedStorage.Id;
            rule.StoragePolicyName = selectedStorage.Name;
            //rule.StubTemplateId = info.ProcessActionParameter.StubSettingDto?.Id;
            //rule.StubTemplateName = info.ProcessActionParameter.StubSettingDto?.Name;
            rule.MoveToAnotherTierType = 0;

            rule.PolicyLevel = PolicyLevel.Document;
            if (action == FileAction.ArchiveAndRemove)
            {
                //if (info.ProcessActionParameter.IsEnableLeaveStub)
                //{
                //    rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                //}
                //else
                //{
                rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove;
                //}
            }
            else if (action == FileAction.Remove)
            {
                rule.KeepDataOption = (int)KeepDataOption.DeleteOnly;
            }
            else
            {
                Logger.Info($"this rule action not supported,return null.action:{action.ToString()}");
                return null;
            }
            //else if (info.ProcessActionParameter.FileAction == FileAction.Archive)
            //{
            //    rule.KeepDataOption = (int)KeepDataOption.ArchiverOnly;
            //    if (info.ProcessActionParameter.EnableArchivedOnlyLatestVersion)
            //    {
            //        rule.KeepDataOption += (int)KeepDataOption.ArchiveOnlyLastestVersion;
            //        rule.ArchiverOnlyLastestVersion = info.ProcessActionParameter.ArchivedOnlyLatestVersion;
            //    }
            //}

            //if (info.ProcessActionParameter.DeleteRecords || rule.KeepDataOption == (int)KeepDataOption.ArchiverOnly)
            //{
            //    rule.DeleteRecords = true;
            //}
            //if (info.ProcessActionParameter != null && info.ProcessActionParameter.EnableArchivedLatestVersion)
            //{
            //    rule.KeepDataOption += (int)KeepDataOption.ArchiveLatestVersion;
            //    rule.ArchivedLatestVersion = info.ProcessActionParameter.ArchivedLatestVersion;
            //}

            return rule;
        }
        private Rule GenarateDiscoverRuleSetting(RMDiscoveryOffice365OptimizationSetting info, bool isFileLevelAction, int moveDataTierType)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            //rule.Name = info.RuleName;
            //rule.Description = info.Description;
            //rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            //rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            rule.StoragePolicyId = info.SelectedStorage.Id;
            rule.StoragePolicyName = info.SelectedStorage.Name;
            rule.StubTemplateId = info.ProcessActionParameter.StubSettingDto?.Id;
            rule.StubTemplateName = info.ProcessActionParameter.StubSettingDto?.Name;
            rule.MoveToAnotherTierType = moveDataTierType;
            if (isFileLevelAction)
            {
                rule.PolicyLevel = PolicyLevel.Document;
                if (info.ProcessActionParameter.FileAction == FileAction.ArchiveAndRemove)
                {
                    if (info.ProcessActionParameter.IsEnableLeaveStub)
                    {
                        rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                    }
                    else
                    {
                        rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove;
                    }
                }
                else if (info.ProcessActionParameter.FileAction == FileAction.Remove)
                {
                    rule.KeepDataOption = (int)KeepDataOption.DeleteOnly;
                    rule.DeleteToRecycleBin = info.ProcessActionParameter.DeleteRecordToRecycleBin;
                }
                else if (info.ProcessActionParameter.FileAction == FileAction.Archive)
                {
                    rule.KeepDataOption = (int)KeepDataOption.ArchiverOnly;
                    if (info.ProcessActionParameter.EnableArchivedOnlyLatestVersion)
                    {
                        rule.KeepDataOption += (int)KeepDataOption.ArchiveOnlyLastestVersion;
                        rule.ArchiverOnlyLastestVersion = info.ProcessActionParameter.ArchivedOnlyLatestVersion;
                    }
                }

                if (info.ProcessActionParameter.DeleteRecords || rule.KeepDataOption == (int)KeepDataOption.ArchiverOnly)
                {
                    rule.DeleteRecords = true;
                }
                if (info.ProcessActionParameter != null && info.ProcessActionParameter.EnableArchivedLatestVersion)
                {
                    rule.KeepDataOption += (int)KeepDataOption.ArchiveLatestVersion;
                    rule.ArchivedLatestVersion = info.ProcessActionParameter.ArchivedLatestVersion;
                }
            }
            else
            {
                rule.PolicyLevel = PolicyLevel.DocumentVersion;
                if (info.ProcessActionParameter.VersionAction == VersionAction.ArchiveAndRemoveVerison)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove;
                }
                else if (info.ProcessActionParameter.VersionAction == VersionAction.RemoveVersion)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly;
                    rule.DeleteToRecycleBin = info.ProcessActionParameter.DeleteVersionToRecycleBin;
                }
            }

            //rule.LeaveStubMessage = !string.IsNullOrEmpty(info.ProcessActionParameter.StubSettingDto?) ? HttpUtility.HtmlEncode(info.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            return rule;
        }

        private Rule GenarateDiscoverRuleSetting(RMDiscoveryAOSPOptimizationSetting info, bool isFileLevelAction, int moveDataTierType)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            //rule.Name = info.RuleName;
            //rule.Description = info.Description;
            //rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            //rule.SOFilters = soFilters;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            rule.StoragePolicyId = info.SelectedStorage.Id;
            rule.StoragePolicyName = info.SelectedStorage.Name;
            rule.StubTemplateId = info.ProcessActionParameter.StubSettingDto?.Id;
            rule.StubTemplateName = info.ProcessActionParameter.StubSettingDto?.Name;
            rule.AOSPStubContent = ReplaceStubTags(info.ProcessActionParameter.StubSettingDto?.StubContent);
            rule.AOSPStubType = info.ProcessActionParameter.StubSettingDto != null ? info.ProcessActionParameter.StubSettingDto.StubType : (int)LeaveStubType.Txt;
            rule.MoveToAnotherTierType = moveDataTierType;
            Logger.Info($"aosp stub info is:enable leave stub:{info.ProcessActionParameter.IsEnableLeaveStub}, type:{rule.AOSPStubType},content:{rule.AOSPStubContent}");
            if (isFileLevelAction)
            {
                rule.PolicyLevel = PolicyLevel.Document;
                if (info.ProcessActionParameter.FileAction == FileAction.ArchiveAndRemove)
                {
                    if (info.ProcessActionParameter.IsEnableLeaveStub)
                    {
                        rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                        rule.LeaveStubType = (LeaveStubType)rule.AOSPStubType;
                        if (info.ProcessActionParameter.StubSettingDto.IsEnabledRetention)
                        {
                            rule.LeaveStubIsEnabledRetention = true;
                            rule.LeaveStubRetentionValue = info.ProcessActionParameter.StubSettingDto.RetentionValue;
                            rule.LeaveStubRetentionUnit = (DateUnit)info.ProcessActionParameter.StubSettingDto.RetentionUnit;
                        }
                        WrapperConfiguration.IsAOSPLeaveStub = true;
                        WrapperConfiguration.AOSPStubSettingDto = GenerateStubSettingForAOSP(rule.AOSPStubContent, rule.AOSPStubType);
                        rule.NeedRecordStubId = true;
                    }
                    else
                    {
                        rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove;
                    }
                }
                else if (info.ProcessActionParameter.FileAction == FileAction.Remove)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly;
                }
                if (info.ProcessActionParameter.DeleteRecords)
                {
                    rule.DeleteRecords = true;
                }
            }
            else
            {
                rule.PolicyLevel = PolicyLevel.DocumentVersion;
                if (info.ProcessActionParameter.VersionAction == VersionAction.ArchiveAndRemoveVerison)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove;
                }
                else if (info.ProcessActionParameter.VersionAction == VersionAction.RemoveVersion)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly;
                }
            }

            //rule.LeaveStubMessage = !string.IsNullOrEmpty(info.ProcessActionParameter.StubSettingDto?) ? HttpUtility.HtmlEncode(info.LeaveStubMessage) : I18NEntity.GetString("RM_RDM_CreateRule_LeaveStubOptionMessage_Default");
            return rule;
        }
        private void GenarateArchiverRuleSetting(Rule rule, RMDiscoveryAOSPOptimizationSetting info)
        {
            //rule.Id = info.ArchiverProfileId;
            //rule.Name = info.RuleName;
            //rule.Description = info.Description;
            //rule.DisposalClass = !string.IsNullOrEmpty(info.DisposalClass) ? info.DisposalClass : null;
            //rule.SOFilters = soFilters;
            //a=>a.UniqueId.ToString() == rule.Id
            var ProcessActionParameter = info.RuleDefinition.FirstOrDefault(a => a.UniqueId.ToString() == rule.Id).ProcessActionParameter;
            rule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            rule.StoragePolicyId = info.SelectedStorage.Id;
            rule.StoragePolicyName = info.SelectedStorage.Name;
            rule.StubTemplateId = ProcessActionParameter.StubSettingDto?.Id;
            rule.StubTemplateName = ProcessActionParameter.StubSettingDto?.Name;
            rule.AOSPStubContent = ReplaceStubTags(ProcessActionParameter.StubSettingDto?.StubContent);
            rule.AOSPStubType = ProcessActionParameter.StubSettingDto != null ? ProcessActionParameter.StubSettingDto.StubType : (int)LeaveStubType.Txt;
            rule.MoveToAnotherTierType = info.MoveToAnotherTierType;
            Logger.Info($"aosp stub info is:enable leave stub:{ProcessActionParameter.IsEnableLeaveStub}, type:{rule.AOSPStubType},content:{rule.AOSPStubContent},rule.PolicyLevel:{rule.PolicyLevel}");
            if (ProcessActionParameter.FileAction == FileAction.ArchiveAndRemove)
            {
                if (ProcessActionParameter.IsEnableLeaveStub)
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                    rule.LeaveStubType = (LeaveStubType)rule.AOSPStubType;
                    WrapperConfiguration.IsAOSPLeaveStub = true;
                    WrapperConfiguration.AOSPStubSettingDto = GenerateStubSettingForAOSP(rule.AOSPStubContent, rule.AOSPStubType);
                    rule.NeedRecordStubId = true;
                }
                else
                {
                    rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove;
                }
            }
            else if (ProcessActionParameter.FileAction == FileAction.Remove)
            {
                rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly;
            }
            if (ProcessActionParameter.DeleteRecords)
            {
                rule.DeleteRecords = true;
            }
        }
        private static AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto GenerateStubSettingForAOSP(string settingString, int stubType)
        {
            return new AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto()
            {
                StubContent = settingString,
                StubType = stubType,
            };
        }
        private string ReplaceStubTags(string stubContent)
        {
            if (string.IsNullOrEmpty(stubContent))
            {
                return string.Empty;
            }
            string stubFileName = "[File name]";
            string stubFilePath = "[File path]";
            string stubArchivedTime = "[Archived time]";
            string stubRestoreLink = "[Restore link]";
            if (stubContent.Contains(stubFileName))
            {
                stubContent = stubContent.Replace(stubFileName, RMConstants.STUBFILENAMEMAPPING);
            }
            if (stubContent.Contains(stubFilePath))
            {
                stubContent = stubContent.Replace(stubFilePath, RMConstants.STUBFILEPATHMAPPING);
            }
            if (stubContent.Contains(stubArchivedTime))
            {
                stubContent = stubContent.Replace(stubArchivedTime, RMConstants.STUBARCHIVEDTIMEMAPPING);
            }
            if (stubContent.Contains(stubRestoreLink))
            {
                stubContent = stubContent.Replace(stubRestoreLink, RMConstants.STUBRESTORELINKMAPPING);
            }

            return stubContent;
        }

        private async Task InitConfigForDiscoverPlanProOptimizationAsync(List<RMDiscoveryRuleDefinition> ruleDefinitions)
        {
            var definitions = ruleDefinitions ?? new List<RMDiscoveryRuleDefinition>();
            var rotRuleInfos = new List<RMDiscoveryOffice365RuleInfo>();
            var inactiveRuleInfos = new List<RMDiscoveryOffice365RuleInfo>();

            foreach (var definition in definitions.Where(item => item != null))
            {
                var category = definition.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version
                    ? RMDiscoveryRuleCategory.InactiveVersion
                    : RMDiscoveryRuleCategory.Trivial;
                var kind = definition.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version
                    ? RMDiscoveryRuleDefinitionKind.Inactive
                    : RMDiscoveryRuleDefinitionKind.ROT;

                var ruleInfo = RMDiscoveryRuleConverter.ConvertToOffice365RuleInfo(definition, kind, category);
                if (definition.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version)
                {
                    inactiveRuleInfos.Add(ruleInfo);
                }
                else
                {
                    rotRuleInfos.Add(ruleInfo);
                }
            }

            mConfiguration.ROTDiscoveryRuleInfos = rotRuleInfos;
            mConfiguration.InactiveDiscoveryRuleInfos = inactiveRuleInfos;

            var discoverRulesConfig = new List<RMDiscoveryOffice365RuleInfo>();
            discoverRulesConfig.AddRange(rotRuleInfos);
            discoverRulesConfig.AddRange(inactiveRuleInfos);
            mConfiguration.DiscoveryO365RuleInfoCache = discoverRulesConfig
                .Where(rule => rule != null)
                .GroupBy(rule => rule.ToTagColumn())
                .ToDictionary(rule => rule.Key, rule => rule.First());

            await Task.CompletedTask;
        }

        private async Task InitConfigForDiscoverOptimizationAsync(RMDiscoveryOffice365OptimizationSetting panalSetting)
        {
            var discoverRulesConfig = new List<RMDiscoveryOffice365RuleInfo>();
            mConfiguration.RMDiscoveryOptimizationSetting = panalSetting;
            mConfiguration.ROTDiscoveryRuleInfos = await DiscoverUtil.GetROTRuleAsync(panalSetting.ROTRuleQueryParameter, panalSetting.ArchiveDataType);
            if (mConfiguration.ROTDiscoveryRuleInfos != null && mConfiguration.ROTDiscoveryRuleInfos.Count > 0)
            {
                mConfiguration.ROTDiscoveryRuleInfos = mConfiguration.ROTDiscoveryRuleInfos.Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                discoverRulesConfig.AddRange(mConfiguration.ROTDiscoveryRuleInfos);
            }

            mConfiguration.InactiveDiscoveryRuleInfos = await DiscoverUtil.GetInactiveRuleAsync(panalSetting.InactiveRuleQueryParameter, panalSetting.ArchiveDataType);
            if (mConfiguration.InactiveDiscoveryRuleInfos != null && mConfiguration.InactiveDiscoveryRuleInfos.Count > 0)
            {
                mConfiguration.InactiveDiscoveryRuleInfos = mConfiguration.InactiveDiscoveryRuleInfos.Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                discoverRulesConfig.AddRange(mConfiguration.InactiveDiscoveryRuleInfos);
            }
            mConfiguration.DiscoveryO365RuleInfoCache = discoverRulesConfig.ToDictionary(r => r.ToTagColumn(), r => r);
        }

        private async Task InitConfigForDiscoverOptimizationAsync(string o365TenantId, RMDiscoveryAOSPOptimizationSetting panalSetting)
        {
            mConfiguration.RMDiscoveryAOSPOptimizationSetting = panalSetting;
            mConfiguration.ROTDiscoveryAOSPRuleInfos = await DiscoverUtil.GetROTRuleAsync(o365TenantId, panalSetting.ROTRuleQueryParameter, panalSetting.ArchiveDataType);
            if (mConfiguration.ROTDiscoveryAOSPRuleInfos != null && mConfiguration.ROTDiscoveryAOSPRuleInfos.Count > 0)
            {
                mConfiguration.ROTDiscoveryAOSPRuleInfos = mConfiguration.ROTDiscoveryAOSPRuleInfos.Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            }

            mConfiguration.InactiveDiscoveryAOSPRuleInfos = await DiscoverUtil.GetInactiveRuleAsync(o365TenantId, panalSetting.InactiveRuleQueryParameter, panalSetting.ArchiveDataType);
            if (mConfiguration.InactiveDiscoveryAOSPRuleInfos != null && mConfiguration.InactiveDiscoveryAOSPRuleInfos.Count > 0)
            {
                mConfiguration.InactiveDiscoveryAOSPRuleInfos = mConfiguration.InactiveDiscoveryAOSPRuleInfos.Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            }
        }

        private async Task ProcessEndUserRecordsDisposalAsync()
        {
            try
            {
                RMSPTreeNode treeNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting).First();
                var endUserConfig = treeNode.EndUserArchiveSiteCollectionConfig ?? new EndUserArchiveSiteCollectionConfig();
                mConfiguration.EndUserArchiveSiteCollectionConfig = endUserConfig;

                bool hasFilesToProcess = endUserConfig.FileInfoList == null || endUserConfig.FileInfoList.Any();
                if (!hasFilesToProcess)
                {
                    Logger.Info("End user archive job received no files to process. Writing job details only.");
                    HandleEndUserArchiveWithoutFiles(endUserConfig);
                }
                else
                {
                    await ProcessRecordsDisposalAsync();
                }
            }
            finally
            {
                EnsureEndUserArchiveJobDetailsLogged();
            }
        }
        private async Task ProcessRecordsDisposalAsync()
        {
            RMSPTreeNode treeNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting).First();
            WrapperConfiguration.IsProcessApprovalDatasOnly = treeNode.IsProcessApprovalDatasOnly;
            SetScheduleSettings(treeNode);
            var node = RMDtoConverter.ConvertRMTree2SPTree(treeNode);
            var siteNode = SPTreeNodeManagement.GetSiteCollectionNode(node);
            var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
            mConfiguration.ContainerId = groupId;
            mConfiguration.SupportLockedSite = treeNode.SupportLockedSite;
            mConfiguration.SupportArchivedTeams = treeNode.SupportArchivedTeams;
            Logger.Info($"SupportLockedSite: {mConfiguration.SupportLockedSite}, SupportArchivedTeams: {mConfiguration.SupportArchivedTeams}");
            mConfiguration.SiteCollectionUrl = siteNode.Url;
            mConfiguration.SiteCollectionID = new Guid(siteNode.ID);
            mConfiguration.RunJobNodeLevel = treeNode.Level;
            mConfiguration.EndUserArchiveSiteCollectionConfig = treeNode.EndUserArchiveSiteCollectionConfig;
            mConfiguration.AutoApprovalManualRule = treeNode.ApprovalType == (int)AvePoint.RA.DB.Model.ApprovalType.AutoApproval ? true : false;
            mConfiguration.UseArchiverImportFile = treeNode.UserArchiverImportFile;
            jobContext.ReportManager.IncreaseBase(100);
            mConfiguration.ScopePath = GetScopeFullPath(node);
            var teamsNode = treeNode.GetTeamsNode();
            mConfiguration.IsTeams = teamsNode != null;
            mConfiguration.TeamsId = teamsNode?.TeamsId;
            mConfiguration.TeamsSiteNodeType = siteNode.Type;
            InitSplitJobDBInfo(treeNode);

            using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
            await InitSiteInfoScheduleConfigAsync(mConfiguration);
            ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
            SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, siteNode.Url, mJobType, mConfiguration.SiteCollectionID.ToString());
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                TreeNode = treeNode,
                Configuration = mConfiguration,
            };
            #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
            Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
            int i = 1;
            if (mJobType == JobType.RMArchiverBackup || mJobType == JobType.TeamsArchiverBackup)
            {
                rules = GetArhiverRules(treeNode).ToDictionary(v => i++);
            }
            else if (mJobType == JobType.SpecifyTeamsArchiverBackup)
            {
                rules = new Dictionary<int, Rule>();
                rules.Add(1, RuleManagerService.GetSpecifyTeamsArchiverBackupRule());
                WrapperConfiguration.EnableRemoveRetentionLabel = true;
                Logger.Info($"ProcessRecordsDisposalAsync.Job Type:{mJobType} and EnableRemoveRetentionLabel.");
            }
            else if (mJobType == JobType.RMEndUserArchiverBackup)
            {
                treeNode.IsEnableRemoveRetentionLabel = true;
                WrapperConfiguration.EnableRemoveRetentionLabel = true;
                var storageDevice = StorageDeviceService.GetIndexDevice();
                Rule rule = new Rule
                {
                    Id = mConfiguration.EndUserArchiveSiteCollectionConfig.RuleAction == ApiRuleAction.Archive ? RecordsConstants.END_USER_ARCHIVE_RULE_ID : RecordsConstants.END_USER_DELETE_ONLY_RULE_ID,
                    Name = "N/A",
                    Filters =
                            [
                                new GCommon.Contract.CommonFilter.FilterPolicy
                                {
                                    Condition = PolicyCondition.Match,
                                    Level = PolicyLevel.Document,
                                    Rule = new UrlRule() { Value1 = "Name" },
                                    Value = new PolicyValue(){Value1 = "*"},
                                    SequenceNo = 1
                                },
                            ],
                    DeleteRecords = true,
                    PolicyLevel = PolicyLevel.Document,
                    AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Document, "(1)" } },
                    Order = 1,
                    KeepDataOption = (int)mConfiguration.EndUserArchiveSiteCollectionConfig.RuleAction,
                    ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                    IncludeNew = "1",
                    StoragePolicyId = storageDevice.Id,
                    OneDriveRule = new Rule
                    {
                        Id = mConfiguration.EndUserArchiveSiteCollectionConfig.RuleAction == ApiRuleAction.Archive ? RecordsConstants.END_USER_ARCHIVE_RULE_ID : RecordsConstants.END_USER_DELETE_ONLY_RULE_ID,
                        Name = "N/A",
                        Filters =
                            [
                                new GCommon.Contract.CommonFilter.FilterPolicy
                                {
                                    Condition = PolicyCondition.Match,
                                    Level = PolicyLevel.Document,
                                    Rule = new UrlRule() { Value1 = "Name" },
                                    Value = new PolicyValue(){Value1 = "*"},
                                    SequenceNo = 1
                                },
                            ],
                        DeleteRecords = true,
                        PolicyLevel = PolicyLevel.Document,
                        AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.Document, "(1)" } },
                        Order = 1,
                        KeepDataOption = (int)mConfiguration.EndUserArchiveSiteCollectionConfig.RuleAction,
                        ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                        IncludeNew = "1",
                        StoragePolicyId = storageDevice.Id,
                    }
                };
                rules = new() { { 1, rule } };
            }
            else if (mJobType == JobType.SpecifySitesArchiverBackup)
            {
                var storageDevice = StorageDeviceService.GetIndexDevice();
                rules = new() {
                    {
                        1,
                        new Rule
                        {
                            Id = RecordsConstants.FAKE_SPECIFY_SITES_RULE_ID,
                            Name = "N/A",
                            Filters =
                            [
                                new GCommon.Contract.CommonFilter.FilterPolicy
                                {
                                    Condition = PolicyCondition.Equals,
                                    Level = PolicyLevel.SiteCollection,
                                    Rule = new UrlRule() { Value1 = "URL" },
                                    Value = new PolicyValue(){Value1 = node.FullPath},
                                    RuleType = PolicyRuleType.Url,
                                    SequenceNo = 1
                                },
                            ],
                            PolicyLevel = PolicyLevel.SiteCollection,
                            AndOrExpression = new Dictionary<PolicyLevel, string>() { { PolicyLevel.SiteCollection, "(1)" } },
                            Order = 1,
                            ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                            IncludeNew = "1",
                            StoragePolicyId = storageDevice.Id,
                            IncludeDeleteRecordLabel = true,
                        }
                    }
                };
                WrapperConfiguration.EnableRemoveRetentionLabel = true;
                Logger.Info($"ProcessRecordsDisposalAsync.Job Type:{mJobType} and EnableRemoveRetentionLabel.");
            }
            else
            {
                bool isNullClassification = false;
                Guid siteId = new Guid(treeNode.GetSiteCollectionNode().SPObjectId);
                if (mJobType == JobType.OneDriveRecordsDisposal)
                {
                    groupId = new Guid(treeNode.GetGroupNode().SPObjectId);
                    var groupSetting = OneDriveSettingDao.GetSettingInfoByAgentGroupId(groupId.ToString());
                    isNullClassification = groupSetting.IsNullClassificationSetting;
                }
                if (isNullClassification)
                {
                    mConfiguration.OneDriveNullClassification = true;
                    var ruleIds = await GetNullClassificationRuleIdsAsync(treeNode, groupId, siteId);
                    var allRules = ScanDataCache.Instance.Rules.Values.ToList();
                    int key = 1;
                    foreach (var id in ruleIds)
                    {
                        var rule = allRules.Where(r => r.Id == id.ToString()).FirstOrDefault();
                        if (rule != null)
                        {
                            if (mConfiguration.IsOneDriverSite)
                            {
                                rules.Add(key, RuleManagerService.ConvertToOneDriveRule(rule));
                            }
                            else
                            {
                                rules.Add(key, rule);
                            }
                            key++;
                        }
                    }
                }
                else
                {
                    rules = ScanDataCache.Instance.RulesBindingInTerms.Values.ToDictionary(v => i++);
                }
            }

            mConfiguration.RuleCollection = rules;
            if (rules != null && rules.Count == 1)
            {
                SetIsLoadFileVersions(rules.FirstOrDefault().Value);
            }
            OutPutRuleInfoIntoAgentLog(rules);
            //ReBuildArchiveRules(mConfiguration.RuleCollection);

            #endregion
            ISharePointScanner scanner = GetScanner(scanJobSettings, treeNode);

            await scanner.RunAsync();

            if (mConfiguration.ArchiveJobSplitedDBInfo.IsNeedSplit)
            {
                await SplitAndRunVirtualRecordsDisposalJob(scanner);
            }
            else
            {
                await RealRunRecordsDisposalJob(scanner);
            }
            OutPutWebRoleDefinitions();
        }

        private async Task ProcessHSMXmlArchiverAsync()
        {
            using var uploadJobMonitorInfoCts = new CancellationTokenSource();
            Task uploadJobMonitorInfoTask = null;
            string traceId = string.Empty;
            try
            {
                RMHSMBackupNode discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMHSMBackupNode>(jobContext.JobContextSetting);
                traceId = discoverNode?.TraceId ?? string.Empty;
                var treeNode = discoverNode.TreeNode;
                //SetScheduleSettings(treeNode);
                var node = RMDtoConverter.ConvertRMTree2SPTree(treeNode);
                var siteNode = SPTreeNodeManagement.GetSiteCollectionNode(node);
                //var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(node).SPObjectId);
                //mConfiguration.ContainerId = groupId;
                mConfiguration.SupportLockedSite = treeNode.SupportLockedSite;
                mConfiguration.SupportArchivedTeams = treeNode.SupportArchivedTeams;
                WrapperConfiguration.IsSkipCheckSystemFile = discoverNode.SkipCheckFileExtension;
                Logger.Info($"SupportLockedSite: {treeNode.SupportLockedSite}, SupportArchivedTeams: {mConfiguration.SupportArchivedTeams}, IsSkipCheckSystemFile:{WrapperConfiguration.IsSkipCheckSystemFile}");
                mConfiguration.SiteCollectionUrl = siteNode.Url;
                mConfiguration.SiteCollectionID = new Guid(siteNode.SPObjectId);
                mConfiguration.RunJobNodeLevel = treeNode.Level;
                jobContext.ReportManager.IncreaseBase(100);
                mConfiguration.JobReportDto = new JobReportImps(jobContext.ReportManager);
                mConfiguration.ProgressDto = mConfiguration.JobReportDto;
                mConfiguration.ScopePath = GetScopeFullPath(node);

                //InitSplitJobDBInfo(treeNode);
                using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                await InitSiteInfoScheduleConfigAsync(mConfiguration);
                ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache());
                SOArchiverJobInfoStatistics.Instance.InitInstance(JobId, siteNode.Url, mJobType, mConfiguration.SiteCollectionID.ToString());
                ScanJobSettings scanJobSettings = new ScanJobSettings()
                {
                    SubJobId = JobId,
                    Id = jobContext.MainJobId,
                    TreeNode = treeNode,
                    Configuration = mConfiguration,
                    SourceDataStorageId = discoverNode.SourceDataStorageId,
                    DataContentStorageId = discoverNode.DataContentStorageId,
                    TraceId = discoverNode.TraceId,
                };
                #region records逻辑中由于不确定当前节点使用了哪些rule，获取所有和term绑定的rule，这里的RuleCollection 虚拟出order
                Dictionary<int, Rule> rules = new Dictionary<int, Rule>();
                mConfiguration.RuleCollection = rules;
                mConfiguration.currentRule = GenarateHSMRuleSetting(discoverNode.SelectedStorage, discoverNode.StubTemplateId);
                #endregion
                ISharePointScanner scanner = GetScanner(scanJobSettings, treeNode);
                uploadJobMonitorInfoTask = Task.Run(() => UploadJobMonitorInfoPeriodicallyAsync(traceId, uploadJobMonitorInfoCts.Token));

                await scanner.RunAsync();
                await ReBuildArchiveRuleAsync(mConfiguration.currentRule);
                await RealRunHSMXmlArchiverJob(scanner);
                OutPutWebRoleDefinitions();
            }
            finally
            {
                uploadJobMonitorInfoCts.Cancel();
                if (uploadJobMonitorInfoTask != null)
                {
                    await WaitForBackgroundTaskAsync(
                        uploadJobMonitorInfoTask,
                        TimeSpan.FromSeconds(30),
                        "UploadJobMonitorInfo background task did not stop within 30 seconds after cancellation.",
                        "UploadJobMonitorInfo background task terminated with error but will be ignored.");
                }
            }
        }

        private async Task WaitForBackgroundTaskAsync(Task backgroundTask, TimeSpan timeout, string timeoutMessage, string errorMessage)
        {
            try
            {
                var completedTask = await Task.WhenAny(backgroundTask, Task.Delay(timeout));
                if (completedTask == backgroundTask)
                {
                    UploadJobMonitorInfo(GetCurrentHsmTraceId());
                    await backgroundTask;
                }
                else
                {
                    Logger.Warn(timeoutMessage);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.Warn($"{errorMessage} Error:{ex}");
            }
        }

        private async Task UploadJobMonitorInfoPeriodicallyAsync(string traceId, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        UploadJobMonitorInfo(traceId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"UploadJobMonitorInfo execution failed. Error:{ex}");
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"UploadJobMonitorInfo periodic loop failed unexpectedly but will be ignored. Error:{ex}");
            }
        }

        private void UploadJobMonitorInfo(string traceId)
        {
            var mainJob = JMDao.GetJobById(jobContext.MainJobId);
            ArchiverJobMonitorDto jobMonitorDto = new ArchiverJobMonitorDto()
            {
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Progress = mainJob.Progress,
                JobStatus = (AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus)mainJob.Status,
                SubjobInfoes = new List<ArchiverSubJobDto>()
            };
            var subJobs = SubJobDao.GetAllSubJobByMainJobId(jobContext.MainJobId);
            foreach (var sub in subJobs)
            {
                jobMonitorDto.SubjobInfoes.Add(new ArchiverSubJobDto() {
                    JobStatus = (AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus)sub.Status,
                    SubJobId = sub.Id
                });
            }
            string jobmonitorJsonBlobPath = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}/{1}/{2}", RMConstants.GetImportArchiveDataFolderName(traceId), mainJob.Id, mainJob.Id + ".json");
            RAStorageUtil.UploadBlobByXRIString(WrapperConfiguration.SpecifyReportStorageXRIString, jobmonitorJsonBlobPath, jobMonitorDto);
            Logger.Info("UploadJobMonitorInfo executed.");
        }


        private string GetCurrentHsmTraceId()
        {
            if (string.IsNullOrWhiteSpace(jobContext?.JobContextSetting))
            {
                return string.Empty;
            }

            try
            {
                var discoverNode = SerializerHelper.DeserializeByDataContractSerializer<RMHSMBackupNode>(jobContext.JobContextSetting);
                return discoverNode?.TraceId ?? string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to resolve HSM traceId from job context. Error:{ex}");
                return string.Empty;
            }
        }

        private async Task RealRunRecordsDisposalJob(ISharePointScanner scanner)
        {
            using (scanner)
            {
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                var ruleIds = dataReader.GetAllRuleIds();
                Logger.Info(
                    $"RealRunRecordsDisposalJob null check => scanner:{scanner == null}, mConfiguration:{mConfiguration == null}, JobReportDto:{mConfiguration?.JobReportDto == null}, dataReader:{dataReader == null}, ObjectCache:{mConfiguration?.ObjectCache == null}, RuleCollection:{mConfiguration?.RuleCollection == null}, ruleIds:{ruleIds == null}");
                foreach (var ruleid in ruleIds)
                {
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();
                    var temp = mConfiguration.RuleCollection.Where(r => r.Value.Id == ruleid).FirstOrDefault();
                    var r = temp.Value;
                    if (mConfiguration.IsOneDriverSite && r?.OneDriveRule != null)
                    {
                        r.OneDriveRule.Id = r.Id;
                        r.OneDriveRule.Name = r.Name;
                        //r.OneDriveRule.StoragePolicyId = r.StoragePolicyId;
                        r = r.OneDriveRule;
                        r.Order = temp.Key;
                    }
                    else if (mConfiguration.IsTeams && r?.TeamsRule != null && mJobType != JobType.TeamsRecordsDisposal)
                    {
                        r.TeamsRule.Id = r.Id;
                        r.TeamsRule.Name = r.Name;
                        r.TeamsRule.DisposalClass = r.DisposalClass;
                        r = r.TeamsRule;
                        r.Order = temp.Key;
                    }
                    await ReBuildArchiveRuleAsync(r);
                    mConfiguration.tempListId = Guid.Empty;//SAAS-15676,RECO-19845,多个rule的情况下应该每个rule都reload content type
                    mConfiguration.currentRule = r;
                    mConfiguration.currentRule.Order = temp.Key;
                    mConfiguration.InitOffice365AlertUtil();
                    SOArchiverJobInfoStatistics.Instance.KeepDataOption = mConfiguration.currentRule.KeepDataOption;

                    if (mConfiguration.ArchiveJobSplitedDBInfo is not null && mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                    {
                        ReCalculateRemainingMatchedRuleFiles(dataReader);
                    }

                    if (scanner is CGArchiverSharePointScanner)
                    {
                        CGArchiverSharePointScanner tempScannerForCG = scanner as CGArchiverSharePointScanner;
                        CheckAveExceedStorageLimit(mConfiguration.SiteCollectionUrl);
                        RunCGScan(ruleid, tempScannerForCG);
                        if (tempScannerForCG.siteStorageSizeLimit)
                        {
                            tempScannerForCG.siteStorageSizeLimit = false;
                            throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
                        }
                        dataEnumer = new CLReader(mConfiguration, tempScannerForCG.pcContainer);
                    }
                    else
                    {
                        dataEnumer = dataReader.GetArchiveApproveReports(ruleid);
                    }
                    var action = CheckRuleAction(mConfiguration.currentRule, JobId, true);
                    mConfiguration.JobReportDto.SetActionPhases(action);
                    //mConfiguration.ProgressDto.SetActionPhases(mConfiguration.ProgressDto.action);
                    try
                    {
                        bool deleteWithNoBackup = mConfiguration.actionType == ActionType.DeleteOnly || mConfiguration.actionType == ActionType.ExportBeforeDelete || mConfiguration.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                        bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfiguration.currentRule);

                        if (isLinkToDucument && deleteWithNoBackup)
                        {
                            var has = await LinkFileCommon.CheckHasRestoreLinkSettings(mConfiguration.currentRule);
                            if (has)
                            {
                                throw new StubUnableGenerateRestoreLinkException();
                            }
                        }
                    }
                    catch (StubUnableGenerateRestoreLinkException stube)
                    {
                        Logger.Error($"StubUnableGenerateRestoreLinkException error {stube}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Item, JobId, mConfiguration.currentRule.Name, "", stube.Message);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"CheckHasRestoreLinkSettings error {e}.");
                    }
                    if (JobExecutionProcessStatisticExecutor.Instance.IsRuleHasExport(mConfiguration.currentRule))
                    {
                        JobExecutionProgressStatisticExecutor.Instance.StartProgressForExport();
                    }
                    switch (action)
                    {
                        case ActionType.ArchiverAndRemove:
                        case ActionType.ArchiverAndKeepData:
                        case ActionType.ExportBeforeArchiver:
                        case ActionType.BackupOnly:
                        case ActionType.ArchchiveToStorage:
                            SOArchiverJobInfoStatistics.Instance.IncludeRelatedOrHasBackUp = true;
                            await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            break;
                        case ActionType.ExportOnly:
                            await ExportOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            break;
                        case ActionType.DeleteOnly:
                        case ActionType.ExportBeforeDelete:
                        case ActionType.DeleteDocumentToRecyleBinOnly:
                            await DeleteOnlyActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            break;
                        case ActionType.KeepDataOnly:
                        case ActionType.ExportBeforeKeepDataOnly:
                            var keepDataActor = new KeepDataAction(mConfiguration, mVaults, mBackups, mRecordManager, mRelativeDataSPObject, NARAMetadatas, NAAMetadatas);
                            await keepDataActor.KeepDataOnlyActionAsync(ruleid, JobId, jobContext.MainJobId, dataEnumer);
                            keepDataActor.DisposeVEOExportMetadata();
                            break;
                        case ActionType.ArchiveByMicrosoft:
                            await ArchiveByMicrosoftActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);
                            break;
                        case ActionType.Move:
                            {
                                using (var moveActionActor = new MoveAction(mConfiguration))
                                {
                                    await moveActionActor.MoveActionFunAsync(JobId, jobContext.MainJobId, dataEnumer);
                                }
                            }
                            break;
                    }
                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                    if (mConfiguration.mOffice365AlertUtil != null)
                    {
                        mConfiguration.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                    }
                    DisposeVEOExportMetadata();
                }

                if (mConfiguration.IsILMode && (ruleIds.Count == 0 || !SOArchiverJobInfoStatistics.Instance.IncludeRelatedOrHasBackUp))
                {
                    Logger.Info($"Start LF deletion.rule count:{ruleIds.Count},exsitArchiveRule:{SOArchiverJobInfoStatistics.Instance.IncludeRelatedOrHasBackUp}");
                    try
                    {
                        ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
                        siteInfo.SiteId = mConfiguration.SiteCollectionID.ToString();
                        siteInfo.SiteUrl = mConfiguration.SiteCollectionUrl;
                        siteInfo.WebApplicationUrl = mConfiguration.WebAppUrl;
                        StartMergeIndexJobByAgentSide(JobId, siteInfo, true);
                        ExecuteRetentionAction(JobId, siteInfo);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("An error occurred while starting merge index job action by agent side.Error:{0}", e.ToString());
                    }
                }
                DisposeNARANAAExportMetadata();
            }
            HSMConnector.GetInstance(mConfiguration).UploadDataToReportLocation();
        }

        private void ReCalculateRemainingMatchedRuleFiles(IScanDataReader dataReader)
        {
            var counts = dataReader.GetDataCounts(minCacheNodeType: (int)CacheNodeType.Item, ruleId: mConfiguration.currentRule.Id);
            long totalItemCount = counts.GetValueOrDefault((int)CacheNodeType.Item) + counts.GetValueOrDefault((int)CacheNodeType.HSMItem) + counts.GetValueOrDefault((int)CacheNodeType.ArchiveBy365Item);
            long totalVersionCount = counts.GetValueOrDefault((int)CacheNodeType.ItemVersion) + counts.GetValueOrDefault((int)CacheNodeType.HSMItemVersion);
            long totalAttachmentCount = counts.GetValueOrDefault((int)CacheNodeType.Attachment);

            bool isVersionPolicy = mConfiguration.currentRule.PolicyLevel.ToString().Contains("Version");
            bool isAttachmentPolicy = mConfiguration.currentRule.PolicyLevel == PolicyLevel.Attachment;
            if (isVersionPolicy || isAttachmentPolicy)
            {
                totalItemCount = 0;
            }

            bool hasExport = JobExecutionProcessStatisticExecutor.Instance.IsRuleHasExport(mConfiguration.currentRule);
            bool hasArchive = JobExecutionProcessStatisticExecutor.Instance.IsRuleHasArchive(mConfiguration.currentRule);
            bool hasOtherActions = JobExecutionProcessStatisticExecutor.Instance.IsRuleHasOtherActions(mConfiguration.currentRule);

            Logger.Info($"ReCalculateRemainingMatchedRuleFiles: ruleId={mConfiguration.currentRule.Id}, totalItemCount={totalItemCount}, totalVersionCount={totalVersionCount}, totalAttachmentCount={totalAttachmentCount}, hasExport={hasExport}, hasArchive={hasArchive}, hasOtherActions={hasOtherActions}");
            JobExecutionProgressStatisticExecutor.Instance.ReCalculateRemainingMatchedRuleFiles(
                exportFiles: hasExport ? totalItemCount + totalVersionCount + totalAttachmentCount : 0,
                archiveFiles: hasArchive ? totalItemCount + totalVersionCount + totalAttachmentCount : 0,
                otherActionsFiles: hasOtherActions ? totalItemCount + (isVersionPolicy ? totalVersionCount : 0) + totalAttachmentCount : 0);
        }
        public static SiteStateTransitionScope TryUnlockSiteCollectionForChannelSite(ScheduleConfiguration mConfiguration)
        {
            SiteStateTransitionScope siteStateTransitionScope = new SiteStateTransitionScope(mConfiguration.SiteCollectionUrl, mConfiguration.aveObjectModelFactory, SiteState.Unlock);

            Logger.Info($"[TryUnlockSiteCollectionForChannelSite]Start checking channel site collections unlock status. SiteCollectionUrl:{mConfiguration.SiteCollectionUrl}, MaxRetryCount:5.");
            const int maxRetryCount = 5;
            for (int retryCount = 0; retryCount <= maxRetryCount; retryCount++)
            {
                using var siteCheckScope = new SiteStateTransitionScope(mConfiguration.SiteCollectionUrl, mConfiguration.aveObjectModelFactory, SiteState.Unlock);
                if (siteCheckScope.TryGetSiteProperties(out IAveSiteProperties siteProps)
                    && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) == SiteState.Unlock)
                {
                    Logger.Info($"Channel site collection is unlocked. SiteCollectionUrl:{mConfiguration.SiteCollectionUrl}.");
                    return siteStateTransitionScope;
                }

                if (retryCount == maxRetryCount)
                {
                    Logger.Info("[TryUnlockSiteCollectionForChannelSite]retryCount is maxRetryCount.");
                    break;
                }

                Logger.Info($"[TryUnlockSiteCollectionForChannelSite]Channel site collections are not unlocked yet. SiteCollectionUrl:{mConfiguration.SiteCollectionUrl}, retry:{retryCount + 1}/{maxRetryCount}. Sleep 1 minute and check again.");
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(1));
            }

            Logger.Warn($"[TryUnlockSiteCollectionForChannelSite]Channel site collections are still not unlocked after max retries. SiteCollectionUrl:{mConfiguration.SiteCollectionUrl}, MaxRetryCount:{maxRetryCount}. Continue following the existing flow.");
            return siteStateTransitionScope;
        }

        public static SiteStateTransitionScope TryUnlockSiteCollection(ScheduleConfiguration mConfiguration, bool skipTeamsUnarchiveCheck = false)
        {
            M365APIUtility teamsStateUtility = null;
            bool teamsUnarchived = false;

            if (!skipTeamsUnarchiveCheck)
            {
                teamsStateUtility = TryUnarchiveTeamsForLockedChannelSite(mConfiguration);
                teamsUnarchived = teamsStateUtility.IsTeamsUnarchivedForLockedChannelSite;
            }

            try
            {
                if (teamsUnarchived)
                {
                    // validate the channel site lock state after unarchiving teams
                    var channekSiteStateTransitionScope = TryUnlockSiteCollectionForChannelSite(mConfiguration);
                    channekSiteStateTransitionScope.AttachTeamsScope4Channel(teamsStateUtility);
                    return channekSiteStateTransitionScope;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"[TryUnlockSiteCollection]Failed to validate channel site lock state after unarchiving teams. SiteCollectionUrl:{mConfiguration.SiteCollectionUrl}, Error:{e}");
                teamsStateUtility?.Dispose();
                throw;
            }

            SiteStateTransitionScope siteStateTransitionScope = new SiteStateTransitionScope(mConfiguration.SiteCollectionUrl, mConfiguration.aveObjectModelFactory, SiteState.Unlock);

            if (mConfiguration.SupportLockedSite)
            {
                siteStateTransitionScope.TryConvertToTargetStatus();
            }
            else if (mConfiguration.IsOneDriverSite
                && GetEnableRemoveReadOnlyState()
                && siteStateTransitionScope.TryGetSiteProperties(out IAveSiteProperties siteProps)
                && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) == SiteState.ReadOnly)
            {
                Logger.Info($"Site is OneDrive site and in ReadOnly state, try to convert it to Unlock state.");
                siteStateTransitionScope.TryConvertToTargetStatus();
            }
            else
            {
                //When the job does not select 'remove read only', if the site is read only, a specific type exception needs to be thrown
                try
                {
                    if (siteStateTransitionScope.TryGetSiteProperties(out IAveSiteProperties siteProps1) && SafeConvertExtensions.ToEnum<SiteState>(siteProps1.LockState) < SiteState.Unlock)
                    {
                        throw new AveSkipLockSiteException("RM_AR_Restore_SiteLocked_ErrorMessage");
                    }
                    else
                    {
                        Logger.Info($"TryUnlockSiteCollection Site is Unlock SiteState:{mConfiguration.SiteCollectionUrl}.");
                    }
                }
                catch (AveSkipLockSiteException ase)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Info($"TryUnlockSiteCollection error.Message:{ex}.");
                }
            }
            return siteStateTransitionScope;
        }

        private static bool ValidateChannelSiteLockState(ScheduleConfiguration mConfiguration, SiteStateTransitionScope siteStateTransitionScope)
        {
            siteStateTransitionScope.TryConvertToTargetStatus();
            if (siteStateTransitionScope.TryGetSiteProperties(out IAveSiteProperties siteProps) && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) < SiteState.Unlock)
            {
                Logger.Error($"Failed to unlock site collection after unarchiving teams. SiteCollectionUrl: {mConfiguration.SiteCollectionUrl}, LockState: {siteProps.LockState}");
                return false;
            }
            Logger.Info($"Successfully unlocked site collection after unarchiving teams. SiteCollectionUrl: {mConfiguration.SiteCollectionUrl}");
            return true;
        }

        private async Task RealRunHSMXmlArchiverJob(ISharePointScanner scanner)
        {
            using (scanner)
            {
                CosmosDBManualDataUpdater.Commit();
                mConfiguration.JobReportDto.AscendPhase();
                var dataReader = scanner.GetScanDataReader();
                var dataCount = dataReader.GetDataCount();
                ScanDataCache.Instance.SetScanDataReader(dataReader);
                mConfiguration.JobReportDto.SetBaseCount4Phase(dataCount * 2);
                var ruleIds = dataReader.GetAllRuleIds();

                foreach (var ruleid in ruleIds)
                {
                    using (new CheckJobStopScope()) { }
                    Logger.Info($"Process rule id : {ruleid}");
                    IEnumerable<ArchiveApproveReport> dataEnumer = null;
                    mConfiguration.ObjectCache.Clear();

                    dataEnumer = dataReader.GetArchiveApproveReports(ruleid);
                    mConfiguration.JobReportDto.SetActionPhases(ActionType.ArchiverAndRemove);

                    await ArchiverBackupActionAsync(ruleid, jobContext.MainJobId, JobId, dataEnumer);

                    if (SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction)
                    {
                        SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                    }
                    if (mConfiguration.mOffice365AlertUtil != null)
                    {
                        mConfiguration.mOffice365AlertUtil.EnableAllCacheLibraryAlert();
                    }
                }

            }
            HSMConnector.GetInstance(mConfiguration).UploadDataToReportLocation();
        }


        private async Task SplitAndRunVirtualRecordsDisposalJob(ISharePointScanner scanner)
        {
            scanner.Dispose();
            RMSPTreeNode subJobtreeNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(jobContext.JobContextSetting).First();

            if (subJobtreeNode.SplitScanDBInfo == null)
            {
                Logger.Info("Real split and run,Split scan db info is null, will create default info");
                subJobtreeNode.SplitScanDBInfo = new SplitScanDBInfo();
            }

            subJobtreeNode.SplitScanDBInfo.FinishedSplitAndRunVritalJob = true;
            subJobtreeNode.SplitScanDBInfo.BriefScanDBName = mConfiguration.ScanDBName;
            subJobtreeNode.SplitScanDBInfo.BriefScanDBFolder = mConfiguration.ArchiveTemp;
            subJobtreeNode.SplitScanDBInfo.ArchiveJobSplitLimit = mConfiguration.ArchiveJobSplitedDBInfo.SplitLimit;

            double sumWeight = SubJobDao.GetSubJobWeight(JobId);
            int virtualSubJobCount = mConfiguration.ArchiveJobSplitedDBInfo.SplitedSubsubjobids.Count();
            double subsubJobWeight = 0;

            Logger.Info($"need split to run, vritual job count:{virtualSubJobCount}");
            if (virtualSubJobCount != 0)
            {
                subsubJobWeight = (sumWeight * 0.7) / virtualSubJobCount;
                SubJobDao.UpdateSubJobWeight(JobId, sumWeight * 0.3);
                jobContext.ReportManager.Increase((int)jobContext.ReportManager.GetTotal());

                while (mConfiguration.ArchiveJobSplitedDBInfo.SplitedSubsubjobids.TryDequeue(out string subsubJobId))
                {
                    Logger.Info($"start run vritual job {subsubJobId}");
                    subJobtreeNode.SplitScanDBInfo.IsLatestVirtalJob = !mConfiguration.ArchiveJobSplitedDBInfo.SplitedSubsubjobids.Any();
                    CreateSubJob(subsubJobId, new List<RMSPTreeNode> { subJobtreeNode }, subsubJobWeight);
                    JobContext.GetInstance(subsubJobId, mJobType);
                    await new DisposalActivityManagementProcessor(subsubJobId, mJobType).RunNowAsync(mConfiguration.ForceFitTeamsRuleID);
                    Logger.Info($"end run vritual job {subsubJobId}");
                }
                JobExecutionProgressStatisticExecutor.Instance.ResetJobId();
            }

            if (mConfiguration.IsILMode)
            {
                Logger.Info($"Start IL deletion.");
                try
                {
                    InitMediaForILRetention();
                    ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
                    siteInfo.SiteId = mConfiguration.SiteCollectionID.ToString();
                    siteInfo.SiteUrl = mConfiguration.SiteCollectionUrl;
                    siteInfo.WebApplicationUrl = mConfiguration.WebAppUrl;
                    ExecuteRetentionAction(JobId, siteInfo);
                }
                catch (Exception e)
                {
                    Logger.Error("An error occurred while run retention action.Error:{0}", e.ToString());
                }
            }
        }

        private void InitMediaForILRetention()
        {
            try
            {
                if (MediaEnvironment.MediaServer == null)
                {
                    MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
                }
                if (MediaConfigInfo.CommonConfigInfo == null)
                {
                    MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
                }
                if (MediaConfigInfo.ArchiverConfigInfo == null)
                {
                    MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Can't initialize media information in method InitMediaForLCRetention. Message:{0}", ex.ToString()));
                throw;
            }
        }

        private void InitSplitJobDBInfo(RMSPTreeNode treeNode)
        {
            ScanJobSettings scanJobSettings = new ScanJobSettings()
            {
                SubJobId = JobId,
                Id = jobContext.MainJobId,
                TreeNode = treeNode,
                Configuration = mConfiguration,
            };

            if (treeNode.SplitScanDBInfo == null)
            {
                Logger.Info("Split scan db info is null, will create default info");
                treeNode.SplitScanDBInfo = new SplitScanDBInfo();
            }

            if (treeNode.SplitScanDBInfo.FinishedSplitAndRunVritalJob)
            {
                mConfiguration.ArchiveJobSplitedDBInfo.SplitLimit = treeNode.SplitScanDBInfo.ArchiveJobSplitLimit;
            }
            else
            {
                mConfiguration.ArchiveJobSplitedDBInfo.SplitLimit = RMKeyValueService.GetArchiveJobSplitLimit();
            }

            if (IsCGScan(scanJobSettings) || mConfiguration.ArchiveJobSplitedDBInfo.SplitLimit?.EnableSplit != true || mJobType == JobType.RMEndUserArchiverBackup)
            {
                mConfiguration.ArchiveJobSplitedDBInfo.IsNeedSplit = false;
                mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB = false;
            }
            else if (treeNode.SplitScanDBInfo.FinishedSplitAndRunVritalJob)
            {
                mConfiguration.ArchiveJobSplitedDBInfo.IsNeedSplit = false;
                mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB = true;
                mConfiguration.ScanDBName = treeNode.SplitScanDBInfo.BriefScanDBName;
                mConfiguration.ArchiveTemp = treeNode.SplitScanDBInfo.BriefScanDBFolder;
                mConfiguration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB = treeNode.SplitScanDBInfo.IsLatestVirtalJob;
            }
            else
            {
                mConfiguration.ArchiveJobSplitedDBInfo.IsNeedSplit = true;
                mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB = false;
            }
        }

        private SourceFlag GetSourceFlagForScanDataCache(bool forceSP = false)
        {
            return forceSP ? SourceFlag.SharePoint
                         : mConfiguration.IsTeams ? SourceFlag.Teams
                         : mConfiguration.IsOneDriverSite ? SourceFlag.OneDrive
                         : SourceFlag.SharePoint;
        }

        private void OutPutWebRoleDefinitions()
        {
            try
            {
                if (mConfiguration != null && mConfiguration.aveObjectModelFactory != null && !string.IsNullOrEmpty(mConfiguration.SiteCollectionUrl))
                {
                    var roleDefinitions = mConfiguration.aveObjectModelFactory.CreateSite(mConfiguration.SiteCollectionUrl).RootWeb.RoleDefinitions;
                    foreach (IAveRoleDefinition roleDefinition in roleDefinitions)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        var permissions = roleDefinition.BasePermissions;
                        foreach (var permission in Enum.GetValues(typeof(AveBasePermissions)))
                        {
                            if (permissions.Has((AveBasePermissions)permission))
                            {
                                stringBuilder.Append(permission.ToString() + ";");
                            }
                        }
                        Logger.Info($"OPUS Job Finished.OutPutWebRoleDefinitions.RoleName:{roleDefinition.Name}.RoleDescription:{roleDefinition.Description}.BasePermissions:{stringBuilder.ToString()}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"OutPutWebRoleDefinitions Error.Message:{ex}.");
            }
        }

        private bool IsCGScan(ScanJobSettings scanJobSettings)
        {
            if (mJobType == JobType.RMArchiverBackup || mJobType == JobType.SpecifySitesArchiverBackup || mJobType == JobType.SpecifyTeamsArchiverBackup || mJobType == JobType.TeamsArchiverBackup)
            {
                if ((NodeLevel)scanJobSettings.TreeNode.Level == NodeLevel.SiteCollection)
                {
                    try
                    {
                        var setting = RMKeyValueDao.GetValueByKey("ArchiverExtendSetting");
                        mConfiguration.ArchiverExtendSetting = setting == null ? null : JsonConvert.DeserializeObject<ArchiverExtendSettingDto>(setting.Value);
                        Logger.Info($"Is ArchiverExtendSetting exsit?{setting != null}");
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"some thing went wrong when deserialize ArchiverExtendSetting,error:{e.ToString()}");
                    }
                    if (mConfiguration.ArchiverExtendSetting != null && mConfiguration.ArchiverExtendSetting.IsCGDiscovery)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private ISharePointScanner GetScanner(ScanJobSettings scanJobSettings, RMSPTreeNode treeNode)
        {
            ISharePointScanner scanner = null;
            if (mJobType == JobType.RMEndUserArchiverBackup)
            {
                scanner = new EndUserSharePointScanner(scanJobSettings);
            }
            else if (mJobType == JobType.RecordsDisposal || mJobType == JobType.OneDriveRecordsDisposal || mJobType == JobType.TeamsRecordsDisposal)
            {
                var columnName = new SharePointSettingUtility().GetMedataColumn(new Guid(treeNode.GetGroupNode().SPObjectId));
                mConfiguration.BCSColumnName = columnName;
                scanner = new RecordsSharePointScanner(scanJobSettings);
                mConfiguration.GetCacheDataForRecords();
                if (ScanDataCache.Instance.Rules.Values.Any(r => r.IsManualApproval))
                {
                    mConfiguration.ManualSiteOwners = new List<AADAccount>();
                }
            }
            else if (mJobType == JobType.RMArchiverBackup || mJobType == JobType.SpecifySitesArchiverBackup || mJobType == JobType.SpecifyTeamsArchiverBackup || mJobType == JobType.TeamsArchiverBackup)
            {
                if ((NodeLevel)scanJobSettings.TreeNode.Level == NodeLevel.SiteCollection)
                {
                    try
                    {
                        var setting = RMKeyValueDao.GetValueByKey("ArchiverExtendSetting");
                        mConfiguration.ArchiverExtendSetting = setting == null ? null : JsonConvert.DeserializeObject<ArchiverExtendSettingDto>(setting.Value);
                        Logger.Info($"Is ArchiverExtendSetting exsit?{setting != null}");
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"some thing went wrong when deserialize ArchiverExtendSetting,error:{e.ToString()}");
                    }
                    if (mConfiguration.ArchiverExtendSetting != null && mConfiguration.ArchiverExtendSetting.IsCGDiscovery)
                    {
                        scanner = new CGArchiverSharePointScanner(scanJobSettings);
                        var mCGDBReader = CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, mConfiguration.SiteCollectionID.ToString(), mConfiguration.SiteCollectionUrl);
                        if (string.IsNullOrEmpty(mCGDBReader.MCurrentSiteSummaryTableName) || !mCGDBReader.MCurrentSiteSummaryTableCanConnect)
                        {
                            Logger.Warn($"CG DBDiscover will skip,because the MCurrentSiteSummaryTableName siteUrl:{mConfiguration.SiteCollectionUrl} doesn't exist in CGDB");
                            throw new CGDBSummaryTableException("StorageOptimization_CGDiscoverSiteSummaryTableError");
                        }
                        else if (mCGDBReader.MCurrentSCCGDBTableName == null)
                        {
                            Logger.Warn($"CG DBDiscover will skip,because the MCurrentSCCGDBTableName siteUrl:{mConfiguration.SiteCollectionUrl} doesn't exist in CGDB");
                            throw new CGDBSCTableNotFoundException("StorageOptimization_CGDBSCTableNotFoundError");
                        }
                        JobExecutionProgressStatisticExecutor.Instance.EnableCGScanner();
                    }
                    else
                    {
                        Logger.Info("not CG archiver,scanner will be normal scanner");
                        scanner = new ArchiverSharePointScanner(scanJobSettings);
                    }
                }
                else
                {
                    scanner = new ArchiverSharePointScanner(scanJobSettings);
                }
            }
            else if (mJobType == JobType.ArchiverByHSMXml)
            {
                scanner = new HSMXmlArchiverScanner(scanJobSettings);
            }
            return scanner;
        }

        private void RunDiscoverOptimizationScan(string ruleid, DiscoverScanner tempScannerForCG)
        {
            tempScannerForCG.GetPCContainer().StartProduce();
            Thread t = new Thread(new ThreadStart(tempScannerForCG.RealRun));
            t.IsBackground = true;
            t.Start();
            using (new CheckJobStopScope()) { }
            if (tempScannerForCG.siteStorageLimit)
            {
                throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
            }
        }

        private void RunDiscoverOptimizationScan(string ruleid, DiscoveryAOSPScanner tempScannerForCG)
        {
            tempScannerForCG.GetPCContainer().StartProduce();
            Thread t = new Thread(new ThreadStart(tempScannerForCG.RealRun));
            t.IsBackground = true;
            t.Start();
            using (new CheckJobStopScope()) { }
            if (tempScannerForCG.siteStorageLimit)
            {
                throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
            }
        }

        private void RunCGScan(string ruleid, CGArchiverSharePointScanner tempScannerForCG)
        {
            SetIsLoadFileVersions(mConfiguration.currentRule);
            tempScannerForCG.pcContainer.StartProduce();
            Thread t = new Thread(new ThreadStart(tempScannerForCG.RealRun));
            t.IsBackground = true;
            t.Start();
        }
        private void CheckAveExceedStorageLimit(string siteUrl)
        {
            try
            {
                var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, mConfiguration.user, AveContextKind.ClientObjectModel);
                IAveSite mSite = aveObjectModelFactory.CreateSite(siteUrl);
                long storageMaximumLevel = mSite.Quota.StorageMaximumLevel * 1024L * 1024L;
                Logger.Info($"Current Site:{mSite.Url} StorageMaximumLevel is:{mSite.Quota.StorageMaximumLevel}.Storage is:{mSite.Usage.Storage}.ByteStorageMaximumLevel:{storageMaximumLevel}.");
                if (mSite.Quota.StorageMaximumLevel == 0)
                {
                    //special env,special site does not permission to get this value, so skip this check when size is 0.
                    Logger.Info($"CheckAveExceedStorageLimit.Current Site:{mSite.Url} StorageMaximumLevel is 0, skip check current site storage limit.");
                }
                else if (mSite.Usage.Storage >= storageMaximumLevel)
                {
                    mConfiguration.JobReportDto.summaryComments = "RM_JM_SiteStorageLimit_ErrorMessage";
                    throw new AveExceedStorageLimitException("This site has exceeded its maximum file storage limit.");
                }
            }
            catch (AveExceedStorageLimitException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Warn("Get Site{0} Storage StorageMaximumLevel Error{1}", siteUrl, e.ToString());
            }
        }

        private void SetIsLoadFileVersions(Rule currentRule)
        {
            var effectiveRule = mConfiguration.IsOneDriverSite && currentRule.OneDriveRule is not null ? currentRule.OneDriveRule : currentRule;
            if ((effectiveRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly
                && (effectiveRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion && effectiveRule.KeepLatestMajorAndMinorVersion == 0)
            {
                Logger.Info($"DBBackup Archiver IncludeVersionForPerformance:False.");
                WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
            }
            else
            {
                Logger.Info($"DBBackup Archiver IncludeVersionForPerformance:True.");
                WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = true;
            }
        }
        private async Task<List<Guid>> GetNullClassificationRuleIdsAsync(RMSPTreeNode tree, Guid groupId, Guid siteId)
        {
            //add group level rules
            List<AvePoint.RA.Contract.RMRuleManageMent.RMSimpleRule> rMSimpleRules = EXOSettingRuleDao.GetOneDriveMappingRules(groupId, siteId).OrderBy(x => x.RuleOrder).ToList();
            List<Guid> ruleIds = new List<Guid>();
            ruleIds.AddRange(rMSimpleRules.Select(s => s.RuleId));

            //add sc level rules
            if (tree.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                var siteIds = GetOneDriveBreakInheritSiteIds(tree);
                var scSettings = (await OneDriveSettingDao.FindListAsync(s => s.SiteGroupId == groupId && s.SiteId == s.ScopeId && !siteIds.Contains(s.SiteId) && s.IsNullClassificationSetting && !s.IsRemoved)).ToList();
                if (scSettings != null && scSettings.Count > 0)
                {
                    var scRuleIds = EXOSettingRuleDao.GetSiteCollectionRuleIds(scSettings.Select(s => s.ScopeId).ToList());
                    foreach (var id in scRuleIds)
                    {
                        if (!ruleIds.Contains(id))
                        {
                            ruleIds.Add(id);
                        }
                    }
                }
            }
            return ruleIds;
        }

        private List<Guid> GetOneDriveBreakInheritSiteIds(RMSPTreeNode tree)
        {
            List<Guid> ids = new List<Guid>();
            if (tree.Level == (int)NodeLevel.SkyDriveProGroup)
            {
                try
                {
                    var parentId = ScheduleService.GetProfileId(tree) + "|";
                    var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                    foreach (var item in treeNodes)
                    {
                        var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                        if (node.Level == (int)NodeLevel.SiteCollection)
                        {
                            Guid siteId = new Guid(node.SPObjectId);
                            if (!ids.Contains(siteId))
                            {
                                ids.Add(siteId);
                            }
                        }
                    }
                    var spsettings = OneDriveSettingDao.GetDescendantsDisableNodes(tree);
                    foreach (var item in spsettings)
                    {
                        var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
                        if (node.Level == (int)NodeLevel.SiteCollection)
                        {
                            Guid siteId = new Guid(node.SPObjectId);
                            if (!ids.Contains(siteId))
                            {
                                ids.Add(siteId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
                }
            }
            return ids;
        }
        private void InitRelativeDataSecondHeader(ScheduleConfiguration mConfig)
        {
            secondHeaderFilePathGuid = Guid.NewGuid().ToString();
            secondHeaderFolderPath = SecurityUtils.SafeCombinePath(mConfig.ArchiveTemp, mConfig.JobId);
            secondHeaderFilePath = SecurityUtils.SafeCombinePath(secondHeaderFolderPath, mConfig.JobId + "_related_" + secondHeaderFilePathGuid + ".tmpheader");
        }
        public async Task<bool> RelativeDataBackupAsync(RelativeDataArchiverContract relativeDataArchiverContract, bool isRAJob, string jobId, int sourceFlag)
        {
            var hasErrorNode = false;
            //init
            try
            {
                if (sourceFlag == (int)SourceFlag.Physical)
                {
                    SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;
                }
                mConfiguration = new ScheduleConfiguration(jobId, sourceFlag == (int)SourceFlag.Physical);
                await ReBuildArchiveRuleAsync(relativeDataArchiverContract.Rule, useDefaultStorageWhenNoStorage: true);
                CallProcess callProcess = new CallProcess();
                ScanJobSettings scanJobSettings = new ScanJobSettings()
                {
                    Action = ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST,
                    Configuration = GetRelativeDataConfiguration(relativeDataArchiverContract, isRAJob, jobId, sourceFlag),
                    SubJobId = callProcess.GenerateJobId(ArchiveConstants.RelativeDataJob),
                    Id = ""
                };
                mConfiguration = scanJobSettings.Configuration;
                InitRelativeDataBackupers();
                InitRelativeDataSecondHeader(mConfiguration);
                using DeferredDisposalScope deferredDisposalScope = mDeferredDisposalScope;
                await InitSiteInfoScheduleConfigAsync(scanJobSettings.Configuration);
                ScanDataCache.Instance.Initialize(GetSourceFlagForScanDataCache(true));
                using (ISharePointScanner scanner = new ArchiverSharePointScanner(scanJobSettings))
                {
                    await scanner.RunAsync();
                    var dataReader = scanner.GetScanDataReader();
                    var action = CheckRuleAction(scanJobSettings.Configuration.currentRule, JobId, true);
                    ScanDataEnumer dataEnumer = dataReader.GetArchiveApproveReports(relativeDataArchiverContract.RuleId);
                    hasErrorNode = await RelativeDataArchiverBackupActionAsync(relativeDataArchiverContract.RuleId, jobId, scanJobSettings.SubJobId, dataEnumer, scanJobSettings.Configuration);
                }
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                Logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"Error occurred while end user backup. Id:{relativeDataArchiverContract.FullPath} Error:{e.ToString()}");
                hasErrorNode = true; ;
            }
            finally
            {
                //UploadDestructionCache();
                if (sourceFlag == (int)SourceFlag.Physical)
                {
                    SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
                }
                DisposeRelativeDataBackupers();
                ReleaseLeaveStubCache();
            }
            return !hasErrorNode;
        }

        private void ReleaseLeaveStubCache()
        {
            try
            {
                if (mConfiguration == null)
                {
                    return;
                }
                CreateLinkFileByPackage.GetInstance(mConfiguration).Dispose();
                HSMConnector.GetInstance(mConfiguration).Dispose();
            }
            catch (Exception e)
            {
                Logger.Error("Fail release leave stub cache. Error:{0}", e.ToString());
            }
        }

        private void UploadDestructionCache()
        {
            try
            {
                DestructionFactory.UploadToStorage();
            }
            catch (Exception e)
            {
                Logger.Error($"Error occurred while uploading destrunction cache. Error:{e.ToString()}");
            }
        }

        private ScheduleConfiguration GetRelativeDataConfiguration(RelativeDataArchiverContract relativeDataArchiverContract, bool isRAJob, string jobId, int sourceFlag)
        {
            ScheduleConfiguration scheduleConfiguration = new ScheduleConfiguration(jobId, sourceFlag == (int)SourceFlag.Physical);
            scheduleConfiguration.IsRelativeDataJob = true;
            scheduleConfiguration.currentRule = relativeDataArchiverContract.Rule;
            scheduleConfiguration.IsILMode = isRAJob;
            scheduleConfiguration.relativeDataTreeNodeString = relativeDataArchiverContract.MetaData;
            scheduleConfiguration.RuleCollection = new Dictionary<int, Rule>();
            scheduleConfiguration.RuleCollection.Add(1, relativeDataArchiverContract.Rule);
            scheduleConfiguration.SiteCollectionUrl = relativeDataArchiverContract.SiteUrl;
            scheduleConfiguration.sharePointType = SPType.BPOS;
            scheduleConfiguration.RelativeDataJobSourceFlag = sourceFlag;
            scheduleConfiguration.Action = ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST;
            scheduleConfiguration.BCSColumnName = GetBCSColumn(relativeDataArchiverContract.SiteUrl);
            scheduleConfiguration.JobReportDto = new JobReportImps(ReportMangerFactory.Instance.ReportManager);
            scheduleConfiguration.ProgressDto = scheduleConfiguration.JobReportDto;
            return scheduleConfiguration;
        }

        private string GetBCSColumn(string siteUrl)
        {
            string columnName = string.Empty;
            var site = RemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
            if (site != null)
            {
                var group = RemoteNodeDao.GetWebApplicationById(site.parentId);
                columnName = new SharePointSettingUtility().GetMedataColumn(new Guid(group.id));
            }
            return columnName;
        }

        private List<Rule> GetArhiverRules(RMSPTreeNode node)
        {
            List<Rule> rules = new List<Rule>();
            var ruleIds = GetAppliedRuleIds(node);
            if (ruleIds != null && ruleIds.Count > 0)
            {
                var isTeamsJob = node.GetTeamsNode() != null;
                var allRules = RuleManagerService.GetRulesFromRecords().Where(r => r.ProfileType == GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule).ToList();
                if (mConfiguration.IsOneDriverSite)
                {
                    allRules = allRules.Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count > 0).ToList();
                }
                else if (isTeamsJob)
                {
                    allRules = allRules.Where(r => r.TeamsRule != null && r.TeamsRule.SOFilters != null && r.TeamsRule.SOFilters.Count > 0 || r.SOFilters != null && r.SOFilters.Count > 0).ToList();
                }
                else
                {
                    allRules = allRules.Where(r => r.SOFilters != null && r.SOFilters.Count > 0).ToList();
                }
                foreach (var id in ruleIds)
                {
                    var rule = allRules.Where(r => r.Id == id.ToString()).FirstOrDefault();

                    if (rule != null)
                    {
                        Logger.Info($"Applied Rule:{rule.Name} ID:{rule.Id}");
                        if (mConfiguration.IsOneDriverSite)
                        {
                            rules.Add(RuleManagerService.ConvertToOneDriveRule(rule));
                        }
                        else if (isTeamsJob)
                        {
                            rules.Add(RuleManagerService.ConvertToTeamsRule(rule));
                        }
                        else
                        {
                            rules.Add(rule);
                        }
                    }
                    else
                    {
                        Logger.Info($"Cannot find Rule ID:{id}");
                    }
                }
            }
            else
            {
                throw new Exception("No related rules found.");
            }
            return rules;
        }

        private List<Guid> GetAppliedRuleIds(RMSPTreeNode node)
        {
            if (mJobType == JobType.TeamsArchiverBackup || mJobType == JobType.TeamsPreScan)
            {
                return string.IsNullOrEmpty(mConfiguration.ForceFitTeamsRuleID) ? GetAppliedRuleIdsOnTeamsNode(node) : [new Guid(mConfiguration.ForceFitTeamsRuleID)];
            }
            List<Guid> ruleIds = new List<Guid>();
            var settingId = RMArchiverSettingsService.GetArchiverSettingId(node);
            if (mConfiguration.UseArchiverImportFile)
            {
                Logger.Info("this job is run by archive import site,so need use group node rule");
                settingId = Guid.Empty;
            }
            var rules = RMEXOSettingRuleDao.GetArchiverMappingRules(settingId, (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver);
            if (rules.IsNullOrEmpty() && node.Level == (int)NodeLevel.SiteCollection)
            {
                settingId = RMArchiverSettingsService.GetArchiverSettingId(node.GetGroupNode());
                rules = RMEXOSettingRuleDao.GetArchiverMappingRules(settingId, (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver);
            }
            if (rules != null && rules.Count > 0)
            {
                ruleIds = rules.Select(r => r.RuleId).ToList();
            }
            return ruleIds;
        }

        private List<Guid> GetAppliedRuleIdsOnTeamsNode(RMSPTreeNode node)
        {
            List<Guid> ruleIds = new List<Guid>();

            Guid settingId = Guid.Empty;
            var topContainerLevel = new List<int> {
                (int)NodeLevel.WebApplication,
                (int)NodeLevel.Office365GroupEntire,
                (int)NodeLevel.SiteCollection,
            };

            var groupNode = node.GetGroupNode();
            var teamsNode = node.GetTeamsNode();
            var siteNode = node.GetSiteCollectionNode();
            var groupId = groupNode == null ? Guid.Empty : new Guid(groupNode.Id);
            var teamsId = teamsNode == null ? Guid.Empty : new Guid(teamsNode.TeamsId);
            var siteId = siteNode == null ? Guid.Empty : new Guid(siteNode.Id);
            var currentNodeObjId = Guid.Empty;
            if (topContainerLevel.Contains(node.Level))
            {
                currentNodeObjId = new Guid(node.Id);
            }
            else
            {
                currentNodeObjId = new Guid(node.SPObjectId);
            }

            if (mConfiguration.UseArchiverImportFile)
            {
                Logger.Info("this job is run by archive import site,so need use group node rule");
                settingId = RMArchiverSettingsService.GetTeamsArchiverSettingId(groupId, Guid.Empty, Guid.Empty);
            }
            else
            {
                settingId = RMArchiverSettingsService.GetTeamsArchiverSettingId(currentNodeObjId, siteId, teamsId);

                if (settingId == Guid.Empty && node.Level == (int)NodeLevel.SiteCollection)
                {
                    settingId = RMArchiverSettingsService.GetTeamsArchiverSettingId(teamsId, Guid.Empty, teamsId);

                    if (settingId == Guid.Empty)
                    {
                        settingId = RMArchiverSettingsService.GetTeamsArchiverSettingId(groupId, Guid.Empty, Guid.Empty);
                    }
                }
            }

            if (settingId != Guid.Empty)
            {
                var rules = RMEXOSettingRuleDao.GetArchiverMappingRules(settingId, (int)AvePoint.RA.DB.Dao.Impl.RuleType.Archiver);
                ruleIds = rules.Select(r => r.RuleId).ToList();
            }
            return ruleIds;
        }

        private async System.Threading.Tasks.Task InitSiteInfoScheduleConfigAsync(ScheduleConfiguration config)
        {
            GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(config.SiteCollectionUrl);
            if (remoteSiteCollection == null)
            {
                mConfiguration.JobReportDto.AddScanReport(config.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_JM_Archive_SiteRemoveFromAOS_ErrorMessage");
                throw new Exception("RM_JM_Archive_SiteRemoveFromAOS_ErrorMessage");
            }
            config.AveSiteId = remoteSiteCollection.id;
            //var groupNode = RemoteNodeDao.GetGroupByAosIdAndNodeLevel(aosRemoteNode.ParentId, (int)ConvertGroupNodeLevel(aosRemoteNode.NodeType));
            var webapp = RABrowserClient.GetWebApplicationById(remoteSiteCollection.parentId);
            config.WebAppId = webapp.id;
            config.WebAppUrl = webapp.url;
            AveBPOSAccountInfo bposInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
            config.user = bposInfo;
            config.O365TenantId = remoteSiteCollection.TenantId;
            config.aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(config.SiteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
            if (mJobType == JobType.OneDriveRecordsDisposal || remoteSiteCollection.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro)
            {
                config.IsOneDriverSite = true;
            }

            try
            {
                string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                config.SiteCollectionID = new Guid(siteID);
            }
            catch (AveSkipLockSiteException ex)
            {
                if (!config.SupportLockedSite &&
                    (config.jobtype == Contract.JobMonitor.JobType.SpecifySitesArchiverBackup
                    || config.jobtype == Contract.JobMonitor.JobType.SpecifyTeamsArchiverBackup)
                    && GetEnableRemoveReadOnlyState())
                {
                    Logger.Info("Site is locked, but the job support to run on locked site, so try to unlock site and continue.");
                    config.SupportLockedSite = true;
                }
                if (TryUnlockSite(config))
                {
                    string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                    Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                    config.SiteCollectionID = new Guid(siteID);
                }
                else
                {
                    mConfiguration.JobReportDto.AddScanReport(config.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_AR_Restore_SiteLocked_ErrorMessage");
                    Logger.Info("site locked error,Message:{0}.", ex.ToString());
                }
            }
            catch (Exception ex)
            {
                if (ex is Contract.Global.Exceptions.JobStopException || ex is JobStopException)
                {
                    throw new JobStopException(ex);
                }
                else if (ex?.InnerException is Contract.Global.Exceptions.JobStopException || ex?.InnerException is JobStopException)
                {
                    throw new JobStopException(ex);
                }
                else if (bposInfo == null || !bposInfo.ExsitAppProfile)
                {
                    Logger.Error("App profile for this site not found, Site: {0}, Message:{1}.", config.SiteCollectionUrl, ex.ToString());
                    throw new Exception("RM_JM_AppProfile_NotFoundError");
                }
                else if (ex is WebException || ex?.InnerException is WebException)
                {
                    HandleWebException(config, ex);
                }
                else
                {
                    Logger.Error("Can not get right SiteID,Message:{0}.", ex.ToString());
                    throw new Exception("RM_JM_Archive_UnableConnectSite_ErrorMessage", ex);
                }
            }
            try
            {
                mConfiguration.siteUrlSchemeAndHost = new Uri(config.SiteCollectionUrl).Scheme + @"://" + new Uri(config.SiteCollectionUrl).Authority;
                Logger.Info($"mConfiguration siteUrlSchemeAndHost:{mConfiguration.siteUrlSchemeAndHost}.");
            }
            catch (Exception ex)
            {
                Logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
            }
        }

        private void HandleWebException(ScheduleConfiguration config, Exception ex)
        {
            var we = GetWebException(ex);
            var status = (we?.Response as HttpWebResponse)?.StatusCode;
            if (status == HttpStatusCode.Forbidden)
            {
                if (TryUnlockSite(config, true))
                {
                    string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                    Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                    config.SiteCollectionID = new Guid(siteID);
                    return;
                }
                else
                {
                    Logger.Warn($"Received HTTP 403 but LockState is not NoAccess. Site: {config.SiteCollectionUrl}, Status: {status}");
                    return;
                }
            }

            Logger.Error($"Can not get right SiteID, status: {status} ,Message:{ex}.");
            throw new Exception("RM_JM_Archive_UnableConnectSite_ErrorMessage", ex);
        }

        private static WebException GetWebException(Exception exception)
        {
            while (exception != null)
            {
                if (exception is WebException webException)
                {
                    return webException;
                }

                exception = exception.InnerException;
            }

            return null;
        }

        private bool TryUnlockSite(ScheduleConfiguration config, bool skipIfNotNoAccess = false)
        {
            try
            {
                SiteStateTransitionScope scope = new SiteStateTransitionScope(config.SiteCollectionUrl, config.aveObjectModelFactory, SiteState.ReadOnly);
                if (config.SupportLockedSite)
                {
                    Logger.Info($"Check site lock state and try to unlock it if it is locked. skipIfNotNoAccess: {skipIfNotNoAccess}");
                    if (skipIfNotNoAccess && scope.TryGetSiteProperties(out IAveSiteProperties siteProps) 
                        && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) > SiteState.NoAccess)
                    {
                        Logger.Warn($"Site is not in NoAccess state, skip unlock. Current state: {siteProps.LockState}");
                        return false;
                    }
                    mDeferredDisposalScope.Add(scope);
                    return scope.TryConvertToTargetStatus();
                }
                else if (scope.TryGetSiteProperties(out IAveSiteProperties siteProps) && SafeConvertExtensions.ToEnum<SiteState>(siteProps.LockState) < SiteState.ReadOnly)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error occur when check site lock.Message:{e}.");
            }
            return false;
        }

        private static bool GetEnableRemoveReadOnlyState()
        {
            var keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
            var key = keyValueDao.GetValueByKey("EnableRemoveReadOnlyState");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private async System.Threading.Tasks.Task InitAOSPSiteInfoScheduleConfigAsync(ScheduleConfiguration config, string tenantId, string appProfileId, string siteAdminUrl)
        {
            AveBPOSAccountInfo bposInfo = await PoolUserUtil.GetAOSPBPOSInfoAsync(appProfileId, siteAdminUrl);
            config.user = bposInfo;
            config.aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(config.SiteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
            try
            {
                string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                config.SiteCollectionID = new Guid(siteID);
            }
            catch (AveSkipLockSiteException ex)
            {
                if (TryUnlockSite(config))
                {
                    string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
                    Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
                    config.SiteCollectionID = new Guid(siteID);
                }
                else
                {
                    mConfiguration.JobReportDto.AddScanReport(config.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "", JobDetailsStatus.Failed, "RM_AR_Restore_SiteLocked_ErrorMessage");
                    Logger.Info("site locked error,Message:{0}.", ex.ToString());
                }
            }
            catch (Exception ex)
            {
                if (GetWebException(ex) != null)
                {
                    HandleWebException(config, ex);
                    return;
                }

                Logger.Info("Can not get right SiteID,Message:{0}.", ex.ToString());
                throw new Exception("RM_JM_Archive_UnableConnectSite_ErrorMessage", ex);
            }
            try
            {
                mConfiguration.siteUrlSchemeAndHost = new Uri(config.SiteCollectionUrl).Scheme + @"://" + new Uri(config.SiteCollectionUrl).Authority;
                Logger.Info($"mConfiguration siteUrlSchemeAndHost:{mConfiguration.siteUrlSchemeAndHost}.");
            }
            catch (Exception ex)
            {
                Logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
            }
        }

        /* private async Task<AveBPOSAccountInfo> GetBposInfoBySiteAsync(string siteUrl)
         {
             GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
             AveBPOSAccountInfo aveBPOSAccountInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
             return aveBPOSAccountInfo;
         }*/
        //此方法用来获取scope full path
        /// <summary>
        /// no use for now
        /// </summary>
        /// <param name="scopeFullPath"></param>
        /// <returns></returns>
        private string GetScopeFullPath(SPTreeNodeDto treeNode)
        {
            var scopeFullPath = treeNode.FullPath;
            string fullPath = GetSiteUrl(SPTreeNodeManagement.GetSiteCollectionNode(treeNode).Url);
            switch (treeNode.Level)
            {
                case NodeLevel.SiteCollection://site collection
                case NodeLevel.List://site collection
                    fullPath = scopeFullPath.Replace("/", "_");
                    break;
                case NodeLevel.Site://site
                    fullPath = string.Concat(scopeFullPath, "/.").Replace("/", "_");
                    break;
                case NodeLevel.RootFolder://root folder
                    fullPath = string.Concat(fullPath, scopeFullPath, "/..");
                    fullPath = fullPath.Replace("/", "_").Replace(@"\", "_");
                    break;
                case NodeLevel.Folder://folder
                    fullPath = string.Concat(fullPath, scopeFullPath);
                    fullPath = fullPath.Replace("/", "_").Replace(@"\", "_");
                    break;
                default:// do not support other node
                    break;
            }
            return fullPath;
        }

        private string GetSiteUrl(string mSiteUrl)
        {
            string url = string.Empty;
            url = mSiteUrl.Substring(0, mSiteUrl.LastIndexOf("/"));
            url = url.Substring(0, url.LastIndexOf("/"));
            return url;
        }

        private ActionType CheckRuleAction(Rule currentRule, string jobId, bool isRecordMode)
        {
            Logger.Info($"CheckRuleAction:" +
                $" currentRule.KeepDataOption:{currentRule.KeepDataOption}, " +
                $"currentRule.ExportType:{currentRule.ExportType}, " +
                $"currentRule.ExportInfo.exportSPDataOption:{currentRule.ExportInfo?.exportSPDataOption}, " +
                $"currentRule.MoveToRecordCenterAndDelareSetting.OperateDataMode:{currentRule.MoveToRecordCenterAndDelareSetting?.OperateDataMode}.");
            //mConfiguration.currentRule.KeepDataOption = (int)KeepDataOption.NotBackup;
            ActionType action = ActionType.ArchiverAndRemove;
            //mConfiguration.ProgressDto.ResetProgressWeight();
            //mConfiguration.currentRule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
            if (currentRule.ExportInfo != null && currentRule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                action = ActionType.ExportOnly;
            }
            else if (currentRule.MoveToRecordCenterAndDelareSetting != null && currentRule.MoveToRecordCenterAndDelareSetting.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare && currentRule.ExportInfo == null)
            {
                action = ActionType.Move;
            }
            //mConfiguration.BackupRequest.Rules.ContainsKey(mConfiguration.currentRule.Id) && mConfiguration.BackupRequest.Rules[mConfiguration.currentRule.Id].ExportType != ExportTypeValue.Autonomy
            else if (
                (currentRule.KeepDataOption == (int)KeepDataOption.Delete
                || ((currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument && (currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup)
                || (currentRule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove)
                )
            {
                if (
                    currentRule.ExportType == ExportTypeValue.NAA
                    || currentRule.ExportType == ExportTypeValue.NARA
                    || currentRule.ExportType == ExportTypeValue.VEO
                    )
                {
                    action = ActionType.ExportBeforeArchiver;
                }
                else
                {
                    action = ActionType.ArchiverAndRemove;
                }
                if (currentRule.KeepDataOption == (int)KeepDataOption.LinkDocument)
                {
                    //mConfiguration.ProgressDto.SetProgressWeight(1);
                }
            }
            //Backup only doesn't support Export.
            else if (currentRule.KeepDataOption == (int)KeepDataOption.Keep)
            {
                action = ActionType.BackupOnly;
            }
            //Only DeleteOnly and ExportBeforeDeleteOnly.
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
            {
                if (
                    currentRule.ExportType == ExportTypeValue.NAA
                    || currentRule.ExportType == ExportTypeValue.NARA
                    || currentRule.ExportType == ExportTypeValue.VEO
                    )
                {
                    action = ActionType.ExportBeforeDelete;
                }
                else
                {
                    action = ActionType.DeleteOnly;
                }
            }
            //当前逻辑的前提是Archiver不支持ExportBeforeArchiverAndKeepData，如果支持，需要修改此处判断.
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep)
            {
                if (
                    currentRule.ExportType == ExportTypeValue.NAA
                    || currentRule.ExportType == ExportTypeValue.NARA
                    || currentRule.ExportType == ExportTypeValue.VEO
                    )
                {
                    //Records Keep Data Default use KeepDataOnly.RECO-5524
                    if (isRecordMode)
                    {
                        Logger.Info("Records Export Keep Data use ExportBeforeKeepDataOnly.");
                        action = ActionType.ExportBeforeKeepDataOnly;
                    }
                    else
                    {
                        action = ActionType.ArchiverAndKeepData;
                    }
                }
                else
                {
                    //Records Keep Data Default use KeepDataOnly.RECO-5524
                    if (isRecordMode)
                    {
                        Logger.Info("Records Keep Data use KeepDataOnly.");
                        action = ActionType.KeepDataOnly;
                    }
                    else
                    {
                        action = ActionType.ArchiverAndKeepData;
                    }
                }
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub)
            {
                action = ActionType.ArchchiveToStorage;
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
            {
                if (
                    currentRule.ExportType == ExportTypeValue.NAA
                    || currentRule.ExportType == ExportTypeValue.NARA
                    || currentRule.ExportType == ExportTypeValue.VEO
                    )
                {
                    action = ActionType.ExportBeforeDelete;
                }
                else
                {
                    action = ActionType.DeleteDocumentToRecyleBinOnly;
                }
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) == (int)KeepDataOption.TriggerMicrosoft365Archiving)
            {
                action = ActionType.ArchiveByMicrosoft;
            }
            else if ((currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove
                 || (currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub)
            {
                action = ActionType.ArchchiveToStorage;
            }
            mConfiguration.actionType = action;
            Logger.Info("Current Rule:{0} ActionType is: {1}. JobID is:{2}.", currentRule.Name, action.ToString(), jobId);
            return action;
        }

        //private void InitSiteInfoScheduleConfig(ScheduleConfiguration config)
        //{
        //    var bposInfo = GetBposInfoBySite(config.SiteCollectionUrl);
        //    config.aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(config.SiteCollectionUrl, bposInfo, AveContextKind.ClientObjectModel);
        //    try
        //    {
        //        string siteID = config.aveObjectModelFactory.CreateSite(config.SiteCollectionUrl).ID.ToString();
        //        Logger.Info("Get right Site id for site: {0}, ID: {1},ArchiverMessage SiteID:{2}.", config.SiteCollectionUrl, siteID, config.SiteCollectionID);
        //        config.SiteCollectionID = new Guid(siteID);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Info("Can not get right SiteID,Message:{0}.", ex.ToString());
        //    }
        //}

        //private AveBPOSAccountInfo GetBposInfoBySite(string siteUrl)
        //{
        //    GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
        //    AveBPOSAccountInfo aveBPOSAccountInfo = PoolUserUtil.GetBPOSInfo(remoteSiteCollection);
        //    return aveBPOSAccountInfo;
        //}

        private async System.Threading.Tasks.Task ArchiverBackupActionAsync(string ruleId, string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader, bool isDSOJobAndNeedAddOneSiteCollectionDetail = false)
        {
            bool hasErrorNode = false;
            ResponseHandle responseHandle = null;
            BackupInfoSender aveSender = null;
            string mediaName = string.Empty;
            int errorType = int.MaxValue;
            string ruleName = string.Empty;
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;
            Guid deleteSummaryCookie = Guid.Empty;
            using DeferredDisposalScope deferredSiteStateTransitionScope = new();
            try
            {
                InitExportType(ruleId);
                InitStreamWriter();
                await CheckNeedRecordStubId();
                HSMConnector.GetInstance(mConfiguration).Reset();
                ruleName = mConfiguration.currentRule.Name;
                int ruleLevel = (int)mConfiguration.currentRule.PolicyLevel;
                responseHandle = new ResponseHandle(mConfiguration);
                string indexJobId = string.Empty;
                var sourceFlag = mConfiguration.IsOneDriverSite ? SourceFlag.OneDrive : SourceFlag.SharePoint;
                var dataFlag = sourceFlag;
                if (mConfiguration.IsTeams)
                {
                    sourceFlag = SourceFlag.Teams;
                }
                aveSender = ConfigMedia(ruleId, subJobId, null, ref indexJobId, sourceFlag, dataFlag);
                mConfiguration.CurrentIndexJobID = indexJobId;
                aveSender.permissionLevels = new List<PermissionLevel>();
                JobExecutionProcessStatisticExecutor.Instance.StartCalculateArchiveSummary(ruleId, out Guid archiveSummaryCookie);
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(aveSender, cacheSPObjs);
                    IArchiverBackupDataWriter fileSender = aveSender != null ? aveSender.FileSender : null;  //RM 不连接Media，aveSender为null
                    IBackupController backupController = new MultiBackupController(fileSender,
                                                   mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                   mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                   mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                    foreach (ArchiveApproveReport entity in reader)
                    {
                        using (new CheckJobStopScope())
                        {
                            Logger.Info($"Backup process data:{entity.NodeId}");
                            if (isDSOJobAndNeedAddOneSiteCollectionDetail)
                            {
                                mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                                isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                            }
                            try
                            {
                                #region errorType
                                if (entity.CacheNodeType > errorType)
                                {
                                    Logger.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                    continue;
                                }
                                else
                                {
                                    errorType = int.MaxValue;
                                }
                                #endregion
                                SPObjectBackup backup = GetBackupObject(entity);
                                CacheNode cacheNode = new CacheNode()
                                {
                                    Sender = aveSender,//backup.AveSender,
                                    Configuration = mConfiguration,
                                    Node = entity
                                };
                                cacheNode.DoDelete = entity.DoDelete;
                                RegisterSecondHeaderEventHandler(cacheNode);
                                aveSender.BackupStream.SetStreamTransfered(0);
                                var backupNodeParameters = new BackupNodeParameters()
                                {
                                    CacheSPObjs = cacheSPObjs,
                                    Node = entity,
                                    BackupObj = backup,
                                    CacheNode = cacheNode,
                                    RuleName = ruleName,
                                    SubJobId = subJobId,
                                    RuleLevel = ruleLevel,
                                    MediaName = mediaName,
                                    Sender = aveSender,
                                    Configuration = mConfiguration
                                };
                                await backupController.ProcessAsync(backupNodeParameters);
                                mConfiguration.ProgressDto.HasCompleteNode = true;
                            }
                            #region
                            catch (BlockQueueSyncException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                mConfiguration.JobReportDto.summaryComments = ex.Message;
                                mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                return;
                            }
                            catch (HandShakeException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                mConfiguration.JobReportDto.summaryComments = ex.Message;
                                mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                return;
                            }
                            catch (NetworkBrokenException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                mConfiguration.JobReportDto.summaryComments = ex.Message;
                                mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                return;
                            }
                            catch (ClosedWithErrorException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                mConfiguration.JobReportDto.summaryComments = ex.Message;
                                mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                return;
                            }
                            catch (FileAlreadyExistException ex)
                            {
                                Logger.Warn($"An error occurred while backing up.FileAlreadyExistException:{ex}.");
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                mConfiguration.JobReportDto.summaryComments = ex.Message;
                                mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                //数据块出异常，尤其是Metadata数据块异常时会导致子index不组装，当前类型异常整体不处理备份数据，以防止数据丢失.
                                responseHandle = null;
                                return;
                            }
                            catch (Exception e)
                            {
                                errorType = entity.CacheNodeType;
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                                mConfiguration.JobReportDto.summaryComments = e.Message;
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new CompletedWithExceptionException();
                            }
                            #endregion
                            mConfiguration.JobReportDto.UpdateProgress();
                            SOProgressScAndFileStatistic.Instance().IncreaseFileCount(1, entity.NodeType);
                        }
                    }
                    backupController.Finish();
                    if (isDSOJobAndNeedAddOneSiteCollectionDetail && mConfiguration.JobReportDto.summaryComments != "RM_JM_SiteStorageLimit_ErrorMessage")
                    {
                        mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                        isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                    }
                    JobExecutionProcessStatisticExecutor.Instance.EndCalculateArchiveSummary(ruleId, archiveSummaryCookie);
                    JobExecutionProcessStatisticExecutor.Instance.StartCalculateDeleteAndStubSummary(ruleId, out deleteSummaryCookie);
                    HSMConnector.GetInstance(mConfiguration).Finish();
                    CacheNARANAAExportMetadata(subJobId);
                }
            }
            catch (JobStopException)
            {   //HSM异常需要释放置Finish状态，否则WaitingQueueFinshed会等待三十分钟
                HSMConnector.GetInstance(mConfiguration).Finish();
                throw;
            }
            catch (Exception e)
            {
                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                string commont = e.Message;
                if (e is ExportConfigurationFileError)
                {
                    Logger.Warn("[ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.Message);
                    }
                }
                else if (e.InnerException != null && e.InnerException is ExportConfigurationFileError)
                {
                    Logger.Warn("[InnerException][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.InnerException.Message);
                    }
                }
                else if (e is ArchiverBackupDataWriterException)
                {
                    commont = string.Empty;
                    mConfiguration.JobReportDto.summaryComments = "RM_JM_StorageValid_ErrorMessage";
                }
                //HSM异常需要释放置Finish状态，否则WaitingQueueFinshed会等待三十分钟
                HSMConnector.GetInstance(mConfiguration).Finish();
                foreach (ArchiveApproveReport entity in reader)
                {
                    try
                    {
                        mConfiguration.JobReportDto.AddReport(mConfiguration.GetNodeFullPath(entity.FullPath), 0, JobDetailsStatus.Failed, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, commont);
                        if (mConfiguration.ArchiverExtendSetting != null && mConfiguration.ArchiverExtendSetting.IsCGDiscovery)
                        {
                            string siteIDString = mConfiguration.SiteCollectionID.ToString();
                            string siteUrlString = mConfiguration.SiteCollectionUrl;
                            CGDBReader.GetInstance(mConfiguration.ArchiverExtendSetting, siteIDString, siteUrlString).UpdateStatus(siteIDString, new Guid(entity.NodeId), BackupRestoreStatus.Failed);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"JobReportDto.AddReport path {entity.FullPath} error {ex.ToString()}");
                    }
                }
                Logger.Warn("[DisposalActivityManagementProcessor][ArchiverBackupActionAsync]Set the HasErrorNode true.");
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMedia;
                    //mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                }
                //会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                responseHandle = null;
            }
            finally
            {
                HSMConnector.GetInstance(mConfiguration).WaitingQueueFinshed();
                #region
                if (null != aveSender)
                {
                    try
                    {
                        Logger.Info(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupBeginSend);
                        //web give a string.Empty to let FileSender can throw Exception
                        BackupCloseInfo closeInfo = new BackupCloseInfo()
                        {
                            ErrorMessage = "",
                        };
                        if (null != this.error)
                        {
                            closeInfo.ErrorMessage = this.error.Message;
                            Logger.Error(string.Format("Can't backup successfully, Error Message:{0}", closeInfo.ErrorMessage));
                        }
                        aveSender.FileSender.Close(closeInfo);// string.Empty);
                        Logger.Info(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendend, subJobId);
                        //备份成功的数据需要执行Deletion操作
                        //if (mConfiguration.ProgressDto.HasErrorNode)
                        //{
                        //    mLog.Error("HasErrorNode and don't delete backup data.");
                        //    mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
                        //    responseHandle = null;
                        //}
                        //把释放方法放在这里，为了防止如果FileSender.Close()如果出异常，Job 会卡死在释放方法里。
                        if (WrapperConfiguration.WrapperConfigurationForBPOS.HasArchiverBackupDataWriterException)
                        {
                            Logger.Error("Has Archiver Backup Data Writer Exception. Skip execute delete action.");
                            throw new ArchiverBackupDataWriterException();
                        }
                        if (responseHandle != null)
                        {
                            CacheSecondHeader("END");//Set end for second file header cache

                            //if (mConfiguration.archiverMessage.Action == ArchiverAction.ARCHIVER_BACKUP_JOB_REQUEST)
                            {
                                try
                                {
                                    ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
                                    siteInfo.SiteId = mConfiguration.SiteCollectionID.ToString();
                                    siteInfo.SiteUrl = mConfiguration.SiteCollectionUrl;
                                    siteInfo.WebApplicationUrl = mConfiguration.WebAppUrl;
                                    StartMergeIndexJobByAgentSide(subJobId, siteInfo);
                                    ExecuteRetentionAction(subJobId, siteInfo);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("An error occurred while starting merge index job action by agent side.Error:{0}", e.ToString());
                                }
                            }
                            foreach (var mergeJob in mergeJobList)
                            {
                                if (mergeJob.Value == MergeIndexState.Failed)
                                {
                                    Logger.Error($"merge index job has failed job,job id:{mergeJob.Key},rule id:{ruleId}");
                                    throw new MergeIndexException("RM_Job_MergeIndexFailed");
                                }
                            }
                            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.SendSecondHeaders"))
                            {
                                if (mConfiguration.BackgroundSettings.SOSkipDeletionForTest)
                                {
                                    Logger.Warn("Enabled SOSkipDeletionForTest, so will skip delete.");
                                }
                                else
                                {
                                    //AddAnKeyValueForTest();
                                    //while (true)
                                    //{
                                    //Thread.Sleep(3000);
                                    //bool hasPauseKey = HasAddPauseKeyValue();
                                    //if (!hasPauseKey)
                                    //{
                                    //    break;
                                    //}
                                    //Logger.Info("has paused delete function,will sleep 3s");
                                    //}
                                    Logger.Info("begain SendSecondHeaders.");

                                    deferredSiteStateTransitionScope.Add(TryUnlockSiteCollection(mConfiguration));
                                    SendSecondHeaders(responseHandle);

                                    Logger.Info("Begin Dispose responseHandle object");
                                    try
                                    {
                                        responseHandle.Dispose();
                                        responseHandle = null;
                                    }
                                    catch (Exception e) { Logger.Warn("ResponseHandle disposed error: {0}", e.ToString()); }
                                }
                            }
                        }
                        //try
                        //{
                        //    mConfiguration.soArchiverQueryWorkerForDel.DeleteAndMoveItems(mConfiguration);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Logger.Warn("delete azure table data error, jobId:{0}, error message:{1}", jobId, ex.ToString());
                        //}
                    }
                    catch (MergeIndexException e)
                    {
                        Logger.Error($"Merge index failed: {e.Message}");
                        throw;
                    }
                    catch (BlockQueueSyncException ex)
                    {
                        mConfiguration.JobReportDto.summaryComments = ex.Message;
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][BlockQueueSyncException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (HandShakeException ex)
                    {
                        mConfiguration.JobReportDto.summaryComments = ex.Message;
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][HandShakeException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (NetworkBrokenException ex)
                    {
                        mConfiguration.JobReportDto.summaryComments = ex.Message;
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][NetworkBrokenException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (FileAlreadyExistException ex)
                    {
                        Logger.Warn($"An error occurred while backing up.FileAlreadyExistException:{ex}.");
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][FileAlreadyExistException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (ClosedWithErrorException ex)
                    {
                        if (IsSQLiteException(ex.Message))
                        {
                            mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMediaSQLiteException;
                        }
                        else
                        {
                            mConfiguration.JobReportDto.summaryComments = ex.Message;
                        }
                        mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][ClosedWithErrorException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (NotEnoughFreeSpaceException ex)
                    {
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][NotEnoughFreeSpaceException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new CompletedWithExceptionException();
                    }
                    catch (AveSkipLockSiteException ex)
                    {
                        mConfiguration.JobReportDto.AddDetailOnly(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, JobDetailsStatus.Failed, mConfiguration.currentRule.Name, ex.Message, string.Empty);
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][AveSkipLockSiteException]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new CompletedWithExceptionException();
                    }
                    catch (Exception ex)
                    {
                        mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMediaSQLiteException;
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        Logger.Warn("[ArchiverBackupActionAsync][Exception]Set the HasErrorNode true.");
                        mConfiguration.ProgressDto.HasErrorNode = true;
                        mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new CompletedWithExceptionException();
                    }
                }
                JobExecutionProcessStatisticExecutor.Instance.EndCalculateDeleteAndStubSummary(ruleId, deleteSummaryCookie);
                #endregion
            }
            if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
        }

        public static M365APIUtility TryUnarchiveTeamsForLockedChannelSite(ScheduleConfiguration configuration)
        {
            M365APIUtility m365APIUtility = new();

            if (configuration.CanTryUnarchiveTeams == 0)
            {
                if (!configuration.CanUnarchiveTeamsArchiveJobs.Contains(configuration.jobtype))
                {
                    configuration.CanTryUnarchiveTeams = 2;
                    return m365APIUtility;
                }

                if (!configuration.IsTeams)
                {
                    if (configuration.jobtype != JobType.SpecifySitesArchiverBackup)
                    {
                        // is Teams job but not have Teams node, should not happend
                        Logger.Warn("JobType {0} expects Teams context but there is not Teams node in the tree node", configuration.jobtype);
                        configuration.CanTryUnarchiveTeams = 2;
                        return m365APIUtility;
                    }

                    if (!configuration.IsChannelSite || !configuration.IsSiteReadOnly)
                    {
                        // process normally without needing to Unarchive Teams
                        Logger.Info($"Site {configuration.SiteCollectionUrl} is not a channel site or the channel site is not read-only");
                        configuration.CanTryUnarchiveTeams = 2;
                        return m365APIUtility;
                    }

                    if (!configuration.HasUpgradeTeams)
                    {
                        configuration.CanTryUnarchiveTeams = 3;
                        throw new AveSkipLockSiteException("RM_AR_ChannelSiteLocked_ErrorMessage");
                    }
                }
                else if (!configuration.IsChannelSite || !configuration.IsSiteReadOnly)
                {
                    // process normally without needing to Unarchive Teams
                    Logger.Info($"Site {configuration.SiteCollectionUrl} is not a channel site or the channel site is not read-only");
                    configuration.CanTryUnarchiveTeams = 2;
                    return m365APIUtility;
                }

                if (!configuration.SupportArchivedTeams)
                {
                    Logger.Warn($"Configuration does not support unarchiving Teams, cannot unlock the channel site {configuration.SiteCollectionUrl} without unarchiving the Teams");
                    configuration.CanTryUnarchiveTeams = 3;
                    throw new AveSkipLockSiteException("RM_AR_ChannelSiteLocked_ErrorMessage");
                    // if config not support => throw new i18n: this channel site is locked due to the Teams is archived, cannot unlock the site without unarchiving the Teams.
                }

                configuration.CanTryUnarchiveTeams = 1;
                Logger.Info($"Configuration supports unarchiving Teams, will try to unarchive Teams for channel site {configuration.SiteCollectionUrl}");
            }

            if (configuration.CanTryUnarchiveTeams == 2)
            {
                return m365APIUtility;
            }

            if (configuration.CanTryUnarchiveTeams == 3)
            {
                throw new AveSkipLockSiteException("RM_AR_ChannelSiteLocked_ErrorMessage");
            }

            m365APIUtility = new(
                configuration.ChannelSiteInfo.GroupMailboxAddress, 
                configuration.ChannelSiteInfo.GroupSiteUrl, 
                configuration.O365TenantId, 
                configuration.TeamsId
                );

            try
            {
                if (!m365APIUtility.TryUnarchiveTeamsForLockedChannelSite())
                {
                    Logger.Warn($"Failed to unarchive Teams for channel site {configuration.SiteCollectionUrl}");
                    throw new Exception("RM_AR_TeamsUnarchived_ErrorMessage");
                }

                TeamsDisposalState.HasChannelSiteReadOnly |= true;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to unarchive Teams for channel site {configuration.SiteCollectionUrl}. Error: {e}");
                // release locker (decrease RefCount even failed)
                m365APIUtility.Dispose();
                throw new AveSkipLockSiteException(e.Message);
            }

            return m365APIUtility;
        }

        private bool HasAddPauseKeyValue()
        {
            var key = RMKeyValueDao.GetValueByKey("PauseDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private void AddAnKeyValueForTest()
        {
            RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = "BackupFinishTime", Value = DateTime.UtcNow.Ticks.ToString() }).GetAwaiter().GetResult();
        }
        private async Task CheckNeedRecordStubId()
        {
            if (!string.IsNullOrEmpty(mConfiguration.currentRule.StubTemplateId))
            {
                Logger.Info($"this rule is stub rule,rule name:{mConfiguration.currentRule.Name},stub template id:{mConfiguration.currentRule.StubTemplateId}");
                var stubSetting = await LinkFileCommon.GetStubTemplatesByIdAsync(mConfiguration.currentRule.StubTemplateId);
                if (stubSetting != null)
                {
                    if (mConfiguration.currentRule.LeaveStubType == LeaveStubType.Link)
                    {
                        mConfiguration.currentRule.NeedRecordStubId = true;
                    }
                    else if (stubSetting.StubContent.Contains(RMConstants.STUBRESTORELINKMAPPING))
                    {
                        mConfiguration.currentRule.NeedRecordStubId = true;
                    }
                }
            }
        }
        private void InitStreamWriter()
        {
            if (!Directory.Exists(secondHeaderFolderPath))
            {
                Logger.Info("Begin Create second header temp folder for Deletion");
                Directory.CreateDirectory(secondHeaderFolderPath);
            }
            streamWriter = new StreamWriter(secondHeaderFilePath);
        }
        private void OutPutRuleInfoIntoAgentLog(Dictionary<int, Rule> rules)
        {
            if (rules != null)
            {
                try
                {
                    foreach (var ruleKeyValue in rules)
                    {
                        Logger.Info($"RuleOrder:{ruleKeyValue.Key}." +
                            $"RuleName:{ruleKeyValue.Value.Name}." +
                            $"RuleStoragePolicyName:{ruleKeyValue.Value.StoragePolicyName}." +
                            $"RuleKeepDataOption:{ruleKeyValue.Value.KeepDataOption}." +
                            $"RuleAndOrExpression:{SerializerHelper.SerializeByDataContractSerializer(ruleKeyValue.Value.AndOrExpression)}." +
                            $"RulePolicyLevel:{ruleKeyValue.Value.PolicyLevel}." +
                            $"RuleFilter:{SerializerHelper.SerializeByDataContractSerializer(ruleKeyValue.Value.Filters)}.");
                    }
                    if (mConfiguration != null)
                    {
                        Logger.Info($"Current run job node:{mConfiguration.ScopePath}.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error occurred while OutPutRuleInfoIntoAgentLog. Error:{ex}.");
                }
            }
        }

        private async System.Threading.Tasks.Task<bool> RelativeDataArchiverBackupActionAsync(string ruleId, string jobId, string subJobId, ScanDataEnumer reader, ScheduleConfiguration scheduleConfiguration)
        {
            bool hasErrorNode = false;
            ResponseHandle responseHandle = null;
            BackupInfoSender aveSender = null;
            string mediaName = string.Empty;
            int errorType = int.MaxValue;
            string ruleName = string.Empty;
            try
            {
                InitStreamWriter();
                HSMConnector.GetInstance(scheduleConfiguration).Reset();
                ruleName = scheduleConfiguration.currentRule.Name;
                int ruleLevel = (int)scheduleConfiguration.currentRule.PolicyLevel;
                responseHandle = new ResponseHandle(scheduleConfiguration);
                string indexJobId = string.Empty;
                aveSender = ConfigMedia(ruleId, subJobId, null, ref indexJobId, (SourceFlag)scheduleConfiguration.RelativeDataJobSourceFlag, (SourceFlag)scheduleConfiguration.RelativeDataJobSourceFlag);
                scheduleConfiguration.CurrentIndexJobID = indexJobId;
                aveSender.permissionLevels = new List<PermissionLevel>();
                JobExecutionProgressStatisticExecutor.Instance.StartProgressForArchived();
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitRelativeDataBackupers(aveSender, cacheSPObjs);
                    IArchiverBackupDataWriter fileSender = aveSender != null ? aveSender.FileSender : null;  //RM 不连接Media，aveSender为null
                    IBackupController backupController = new MultiBackupController(fileSender,
                                                   scheduleConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                   scheduleConfiguration.BackgroundSettings.EnableMultiBackup,
                                                   scheduleConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                    foreach (ArchiveApproveReport entity in reader)
                    {
                        using (new CheckJobStopScope())
                        {
                            #region get partSiteCollectionURL
                            if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
                            {
                                try
                                {
                                    scheduleConfiguration.siteUrlSchemeAndHost = new Uri(entity.FullPath).Scheme + @"://" + new Uri(entity.FullPath).Authority;
                                    Logger.Info($"mConfiguration siteUrlSchemeAndHost:{mConfiguration.siteUrlSchemeAndHost}.");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
                                }
                            }
                            #endregion
                            try
                            {
                                #region errorType
                                if (entity.CacheNodeType > errorType)
                                {
                                    Logger.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                    continue;
                                }
                                else
                                {
                                    errorType = int.MaxValue;
                                }
                                #endregion
                                SPObjectBackup backup = GetRelativeDataBackupObject(entity);
                                CacheNode cacheNode = new CacheNode()
                                {
                                    Sender = aveSender,//backup.AveSender,
                                    Configuration = mConfiguration,
                                    Node = entity
                                };
                                cacheNode.DoDelete = entity.DoDelete;
                                RegisterSecondHeaderEventHandler(cacheNode);
                                aveSender.BackupStream.SetStreamTransfered(0);
                                var backupNodeParameters = new BackupNodeParameters()
                                {
                                    CacheSPObjs = cacheSPObjs,
                                    Node = entity,
                                    BackupObj = backup,
                                    CacheNode = cacheNode,
                                    RuleName = ruleName,
                                    SubJobId = subJobId,
                                    RuleLevel = ruleLevel,
                                    MediaName = mediaName,
                                    Sender = aveSender,
                                    Configuration = mConfiguration
                                };
                                await backupController.ProcessAsync(backupNodeParameters);
                                //mConfiguration.ProgressDto.HasCompleteNode = true;
                            }
                            #region
                            catch (BlockQueueSyncException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                //mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                //mConfiguration.JobReportDto.summaryComments = ex.Message;
                                //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                throw;
                            }
                            catch (HandShakeException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                //mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                //mConfiguration.JobReportDto.summaryComments = ex.Message;
                                //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                throw;
                            }
                            catch (NetworkBrokenException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                //mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                //mConfiguration.JobReportDto.summaryComments = ex.Message;
                                //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                throw;
                            }
                            catch (ClosedWithErrorException ex)
                            {
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, ex.ToString());
                                //mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new FailedException();
                                //mConfiguration.JobReportDto.summaryComments = ex.Message;
                                //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                                throw;
                            }
                            catch (Exception e)
                            {
                                errorType = entity.CacheNodeType;
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                                //mConfiguration.ProgressDto.HasErrorNode = true;
                                this.error = new CompletedWithExceptionException();
                                throw;
                            }
                            #endregion
                        }
                    }
                    backupController.Finish();
                    JobExecutionProgressStatisticExecutor.Instance.StartProgressForOther();
                    HSMConnector.GetInstance(scheduleConfiguration).Finish();
                    CacheNARANAAExportMetadata(subJobId);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                //HSM异常需要释放置Finish状态，否则WaitingQueueFinshed会等待三十分钟
                HSMConnector.GetInstance(scheduleConfiguration).Finish();
                //foreach (ArchiveApproveReport entity in reader)
                //{
                //    try
                //    {
                //        mConfiguration.JobReportDto.AddReport(mConfiguration.GetNodeFullPath(entity.FullPath), 0, JobDetailsStatus.Failed, (int)entity.CacheNodeType, subJobId, ruleName, mediaName, e.Message);
                //    }
                //    catch (Exception ex)
                //    {
                //        Logger.Error($"JobReportDto.AddReport path {entity.FullPath} error {ex.ToString()}");
                //    }
                //}
                //mConfiguration.ProgressDto.HasErrorNode = true;
                //if (mConfiguration.JobReportDto.summaryComments == null)
                //{
                //    mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMedia;
                //    mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                //}
                //会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                responseHandle = null;
                hasErrorNode = true;
                throw;
            }
            finally
            {
                HSMConnector.GetInstance(mConfiguration).WaitingQueueFinshed();
                #region
                if (null != aveSender)
                {
                    try
                    {
                        Logger.Info(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupBeginSend);
                        //web give a string.Empty to let FileSender can throw Exception
                        BackupCloseInfo closeInfo = new BackupCloseInfo()
                        {
                            ErrorMessage = "",
                        };
                        if (null != this.error)
                        {
                            closeInfo.ErrorMessage = this.error.Message;
                            Logger.Error(string.Format("Can't backup successfully, Error Message:{0}", closeInfo.ErrorMessage));
                        }
                        aveSender.FileSender.Close(closeInfo);// string.Empty);
                        Logger.Info(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendend, subJobId);
                        //备份成功的数据需要执行Deletion操作
                        //if (mConfiguration.ProgressDto.HasErrorNode)
                        //{
                        //    mLog.Error("HasErrorNode and don't delete backup data.");
                        //    mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
                        //    responseHandle = null;
                        //}
                        //把释放方法放在这里，为了防止如果FileSender.Close()如果出异常，Job 会卡死在释放方法里。
                        if (responseHandle != null)
                        {
                            CacheSecondHeader("END");//Set end for second file header cache

                            //if (mConfiguration.archiverMessage.Action == ArchiverAction.ARCHIVER_BACKUP_JOB_REQUEST)
                            {
                                try
                                {
                                    SOArchiverJobInfoStatistics.Instance.IncludeRelatedOrHasBackUp = true;
                                    ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
                                    siteInfo.SiteId = scheduleConfiguration.SiteCollectionID.ToString();
                                    siteInfo.SiteUrl = scheduleConfiguration.SiteCollectionUrl;
                                    siteInfo.WebApplicationUrl = scheduleConfiguration.WebAppUrl;
                                    StartMergeIndexJobByAgentSide(subJobId, siteInfo);
                                    ExecuteRetentionAction(subJobId, siteInfo);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("An error occurred while starting merge index job action by agent side.Error:{0}", e.ToString());
                                }
                            }
                            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.SendSecondHeaders"))
                            {
                                SendSecondHeaders(responseHandle);
                                Logger.Info("Begin Dispose responseHandle object");
                                try
                                {
                                    responseHandle.Dispose();
                                    responseHandle = null;
                                }
                                catch (Exception e) { Logger.Warn("ResponseHandle disposed error: {0}", e.ToString()); }
                            }
                        }
                        //try
                        //{
                        //    mConfiguration.soArchiverQueryWorkerForDel.DeleteAndMoveItems(mConfiguration);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Logger.Warn("delete azure table data error, jobId:{0}, error message:{1}", jobId, ex.ToString());
                        //}
                    }
                    catch (BlockQueueSyncException ex)
                    {
                        //mConfiguration.JobReportDto.summaryComments = ex.Message;
                        //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (HandShakeException ex)
                    {
                        //mConfiguration.JobReportDto.summaryComments = ex.Message;
                        //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (NetworkBrokenException ex)
                    {
                        //mConfiguration.JobReportDto.summaryComments = ex.Message;
                        //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (ClosedWithErrorException ex)
                    {
                        //if (IsSQLiteException(ex.Message))
                        //{
                        //    mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMediaSQLiteException;
                        //}
                        //else
                        //{
                        //    mConfiguration.JobReportDto.summaryComments = ex.Message;
                        //}
                        //mConfiguration.JobReportDto.AddReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, subJobId, ruleName, mediaName, ex.ToString());
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new FailedException();
                    }
                    catch (NotEnoughFreeSpaceException ex)
                    {
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        // mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new CompletedWithExceptionException();
                    }
                    catch (Exception ex)
                    {
                        //mConfiguration.JobReportDto.summaryComments = LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupErrorInMediaSQLiteException;
                        Logger.Error(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackupsendclose, ex.ToString());
                        //mConfiguration.ProgressDto.HasErrorNode = true;
                        //mConfiguration.ProgressDto.HasCompleteNode = false;
                        this.error = new CompletedWithExceptionException();
                    }
                }

                #endregion
            }
            //if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
            return hasErrorNode;
        }

        private async System.Threading.Tasks.Task<bool> ExportOnlyActionAsync(string ruleId, string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader)
        {
            bool hasErrorNode = false;
            int errorType = int.MaxValue;
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            try
            {
                InitExportType(ruleId);
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(null, cacheSPObjs);
                    IBackupController backupController = new MultiBackupController(null,
                                                       mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                       mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                       mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                    ArchiverDeletion mDeletion = new ArchiverDeletion(mConfiguration);
                    foreach (ArchiveApproveReport entity in reader)
                    {
                        using (new CheckJobStopScope()) { }
                        try
                        {
                            #region get partSiteCollectionURL
                            if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
                            {
                                try
                                {
                                    mConfiguration.siteUrlSchemeAndHost = new Uri(entity.FullPath).Scheme + @"://" + new Uri(entity.FullPath).Authority;
                                    Logger.Info($"mConfiguration siteUrlSchemeAndHost:{mConfiguration.siteUrlSchemeAndHost}.");
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
                                }
                            }
                            #endregion
                            #region errorType
                            if (entity.CacheNodeType > errorType)
                            {
                                Logger.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                if (entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                                {
                                    //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, jobId);
                                }
                                continue;
                            }
                            else
                            {
                                errorType = int.MaxValue;
                            }
                            #endregion
                            if (mConfiguration.actionType == ActionType.ExportOnly)
                            {
                                if (entity.CacheNodeType == (int)CacheNodeType.APP)
                                {
                                    Logger.Info("Skip APP node:{0}.", entity.FullPath);
                                    continue;
                                }
                                SPObjectBackup backup = mVaults[GetCacheNodeType(entity.CacheNodeType)];
                                CacheNode cacheNode = new CacheNode()
                                {
                                    Sender = null,//backup.AveSender,
                                };
                                cacheNode.DoDelete = entity.DoDelete;
                                //RegisterSecondHeaderEventHandler(cacheNode);
                                var backupNodeParameters = new BackupNodeParameters()
                                {
                                    CacheSPObjs = cacheSPObjs,
                                    Node = entity,
                                    BackupObj = backup,
                                    CacheNode = cacheNode,
                                    RuleName = mConfiguration.currentRule.Name,
                                    SubJobId = subJobId,
                                    RuleLevel = (int)mConfiguration.currentRule.PolicyLevel,
                                    MediaName = string.Empty,
                                    Sender = null,
                                    Configuration = mConfiguration
                                };
                                await backupController.ProcessAsync(backupNodeParameters);
                            }
                            mConfiguration.ProgressDto.HasCompleteNode = true;
                        }
                        #region
                        catch (Exception e)
                        {
                            errorType = entity.CacheNodeType;
                            Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                            Logger.Warn("[ExportOnlyActionAsync][CompletedWithExceptionException]Set the HasErrorNode true.");
                            mConfiguration.ProgressDto.HasErrorNode = true;
                            //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, subJobId);
                            AddBackupCommons(entity.CacheNodeType);
                            this.error = new CompletedWithExceptionException();
                        }
                        #endregion
                        mConfiguration.ProgressDto.UpdateProgress();
                        SOProgressScAndFileStatistic.Instance().IncreaseFileCount(1, entity.NodeType);
                    }
                    backupController.Finish();
                    CacheNARANAAExportMetadata(subJobId);
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                if (e is ExportConfigurationFileError)
                {
                    Logger.Warn("[ExportOnlyActionAsync][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.Message);
                    }
                }
                else if (e.InnerException != null && e.InnerException is ExportConfigurationFileError)
                {
                    Logger.Warn("[ExportOnlyActionAsync][InnerException][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.InnerException.Message);
                    }
                }
                Logger.Warn("[ExportOnlyActionAsync][Exception]Set the HasErrorNode true.");
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                }
                //会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                //mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
            }
            finally
            {
                //reader.DisposeApprovalReportProxy();
            }
            if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
            return hasErrorNode;
        }
        private async System.Threading.Tasks.Task ArchiveByMicrosoftActionAsync(string ruleId, string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader, bool isDSOJobAndNeedAddOneSiteCollectionDetail = false)
        {
            //JobExecutionProcessStatisticExecutor.Instance.StartCalculateDeleteAndStubSummary(ruleId, out Guid deleteSummaryCookie);
            bool hasErrorNode = false;
            int errorType = int.MaxValue;
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            SOArchiverJobInfoStatistics.Instance.IsArchiveBy365Action = true;
            try
            {
                var ruleName = mConfiguration.currentRule.Name;
                int ruleLevel = (int)mConfiguration.currentRule.PolicyLevel;
                using SiteStateTransitionScope siteStateTransitionScope = TryUnlockSiteCollection(mConfiguration);
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(null, cacheSPObjs);
                    {
                        IBackupController backupController = new MultiBackupController(null,
                                                           mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                           mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                           mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                        ArchiverDeletion mDeletion = new ArchiverDeletion(mConfiguration);
                        foreach (ArchiveApproveReport entity in reader)
                        {
                            using (new CheckJobStopScope()) { }
                            try
                            {
                                if (isDSOJobAndNeedAddOneSiteCollectionDetail)
                                {
                                    mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                                    isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                                }
                                #region errorType
                                if (entity.CacheNodeType > errorType)
                                {
                                    Logger.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                    if (entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                                    {
                                        //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, jobId);
                                    }
                                    continue;
                                }
                                if (mConfiguration.currentRule.PolicyLevel == PolicyLevel.Document)
                                {
                                    if (entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
                                    {
                                        SOArchiverJobInfoStatistics.Instance.ItemSizeSum += entity.DocumentSize;
                                        continue;
                                    }
                                    entity.IsArchiveBy365 = true;
                                    if (entity.CacheNodeType == (int)CacheNodeType.Item)
                                    {
                                        entity.CacheNodeType = (int)CacheNodeType.ArchiveBy365Item;
                                    }
                                    SPObjectBackup backup = GetBackupObject(entity);
                                    if (entity.CacheNodeType == (int)CacheNodeType.ArchiveBy365Item)
                                    {
                                        entity.CacheNodeType = (int)CacheNodeType.Item;
                                    }
                                    CacheNode cacheNode = new CacheNode()
                                    {
                                        Configuration = mConfiguration,
                                        Node = entity
                                    };
                                    cacheNode.DoDelete = entity.DoDelete;
                                    var backupNodeParameters = new BackupNodeParameters()
                                    {
                                        CacheSPObjs = cacheSPObjs,
                                        Node = entity,
                                        BackupObj = backup,
                                        CacheNode = cacheNode,
                                        RuleName = ruleName,
                                        SubJobId = subJobId,
                                        RuleLevel = ruleLevel,
                                        Configuration = mConfiguration,
                                    };
                                    await backupController.ProcessAsync(backupNodeParameters);
                                    mConfiguration.ProgressDto.HasCompleteNode = true;
                                    SOProgressScAndFileStatistic.Instance().IncreaseFileCount(1, entity.NodeType);
                                }
                                else if (mConfiguration.currentRule.PolicyLevel == PolicyLevel.SiteCollection)
                                {
                                    if (entity.CacheNodeType != (int)CacheNodeType.SiteCollection)
                                        continue;

                                    if (IsSiteLockedForHold(entity.FullPath))
                                    {
                                        Logger.Warn($"Site {entity.FullPath} is under legal hold – skipping.");
                                        mConfiguration.JobReportDto.AddReport(
                                            entity.FullPath, 0,
                                            JobDetailsStatus.Skipped,
                                            (int)CacheNodeType.SiteCollection,
                                            subJobId, ruleName, string.Empty,
                                            "RM_JM_SiteCollectionHoldAndHaveArchiveRule_ErrorMessage");
                                        continue;
                                    }

                                    await ArchiveSiteCollectionByMicrosoftAsync(entity, ruleName, subJobId);
                                    //SOProgressScAndFileStatistic.Instance().IncreaseFileCount(1, entity.NodeType);
                                    continue;
                                }
                            }
                            #endregion
                            #region
                            catch (Exception e)
                            {
                                errorType = entity.CacheNodeType;
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                                Logger.Warn("[ArchiveByMicrosoftActionAsync][CompletedWithExceptionException]Set the HasErrorNode true.");
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, subJobId);
                                AddBackupCommons(entity.CacheNodeType);
                                this.error = new CompletedWithExceptionException();
                            }
                            #endregion
                        }
                        backupController.Finish();
                        if (isDSOJobAndNeedAddOneSiteCollectionDetail && mConfiguration.JobReportDto.summaryComments != "RM_JM_SiteStorageLimit_ErrorMessage")
                        {
                            mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                            isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (AveSkipLockSiteException ex)
            {
                mConfiguration.JobReportDto.AddDetailOnly(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, JobDetailsStatus.Failed, mConfiguration.currentRule.Name, ex.Message, string.Empty);
                Logger.Warn("[ArchiveByMicrosoftActionAsync][AveSkipLockSiteException]Set the HasErrorNode true.");
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryCommentsDetails = ex.Message;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                if (e is ExportConfigurationFileError)
                {
                    Logger.Warn("[ArchiveByMicrosoftActionAsync][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.Message);
                    }
                }
                else if (e.InnerException != null && e.InnerException is ExportConfigurationFileError)
                {
                    Logger.Warn("[ArchiveByMicrosoftActionAsync][InnerException][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.InnerException.Message);
                    }
                }
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                }
                //会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                //mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
            }
            finally
            {
                //JobExecutionProcessStatisticExecutor.Instance.EndCalculateDeleteAndStubSummary(ruleId, deleteSummaryCookie);
            }
            if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
        }

        private async Task ArchiveSiteCollectionByMicrosoftAsync(
            ArchiveApproveReport entity, string ruleName, string subJobId)
        {
            Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] Start - FullPath: {entity.FullPath}, NodeId: {entity.NodeId}, SiteCollectionID: {mConfiguration.SiteCollectionID}, DocumentSize: {entity.DocumentSize}");
            // Check if this is a private/shared channel site via WebTemplate or root SC
            try
            {
                var uri = new Uri(entity.FullPath);
                if (string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')))
                {
                    Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] Skip - root site collection is not supported by Microsoft 365 Archive. FullPath: {entity.FullPath}");
                    mConfiguration.JobReportDto.M365ArchiveAddReport(
                        entity.FullPath, 0,
                        JobDetailsStatus.Skipped,
                        (int)CacheNodeType.SiteCollection,
                        subJobId, ruleName, string.Empty,
                        "RM_JM_RootSiteCollection_ErrorMessage");
                    return;
                }

                var site = mConfiguration.aveObjectModelFactory.CreateSite(entity.FullPath);
                var rootWeb = site.RootWeb;
                var webTemplate = rootWeb.WebTemplate;
                Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] WebTemplate: {webTemplate}, FullPath: {entity.FullPath}");

                if (string.Equals(webTemplate, "TEAMCHANNEL", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] Skip - TEAMCHANNEL site is not supported by Microsoft 365 Archive. FullPath: {entity.FullPath}");
                    mConfiguration.JobReportDto.M365ArchiveAddReport(
                        entity.FullPath, 0,
                        JobDetailsStatus.Skipped,
                        (int)CacheNodeType.SiteCollection,
                        subJobId, ruleName, string.Empty,
                        "RM_JM_SiteCollectionChannelSite_ErrorMessage");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ArchiveSiteCollectionByMicrosoftAsync] Failed to check WebTemplate, will proceed. Error: {ex.Message}");
            }
            try
            {
                var graphManager = new RMGraphTenantManager(mConfiguration.O365TenantId);
                string siteId = mConfiguration.SiteCollectionID.ToString();
                Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] Calling SetSiteToArchiveStatusAsync - SiteId: {siteId}");
                await graphManager.SetSiteToArchiveStatusAsync(siteId);
                Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] SetSiteToArchiveStatusAsync completed - SiteId: {siteId}");

                //SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
                //SOArchiverJobInfoStatistics.Instance.IsArchiveBy365Action = true;
                //SOArchiverJobInfoStatistics.Instance.AccumulationArchiveBy365ItemsSize(
                //    entity.DocumentSize, entity.FullPath);
                //Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] AccumulationArchiveBy365ItemsSize - Size: {entity.DocumentSize}, ItemSizeSum: {SOArchiverJobInfoStatistics.Instance.ItemSizeSum}, ItemCount: {SOArchiverJobInfoStatistics.Instance.ItemCount}");

                mConfiguration.JobReportDto.M365ArchiveAddReport(
                    entity.FullPath, entity.DocumentSize,
                    JobDetailsStatus.Successful,
                    (int)CacheNodeType.SiteCollection,
                    subJobId, ruleName, string.Empty, string.Empty);
                Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] AddReport Successful - FullPath: {entity.FullPath}, Size: {entity.DocumentSize}");

                mConfiguration.ProgressDto.HasCompleteNode = true;
                Logger.Info($"[ArchiveSiteCollectionByMicrosoftAsync] Done - FullPath: {entity.FullPath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"[ArchiveSiteCollectionByMicrosoftAsync] Failed - FullPath: {entity.FullPath}, Error: {ex}");
                mConfiguration.JobReportDto.M365ArchiveAddReport(
                    entity.FullPath, 0,
                    JobDetailsStatus.Failed,
                    (int)CacheNodeType.SiteCollection,
                    subJobId, ruleName, string.Empty, ex.Message);
                mConfiguration.ProgressDto.HasErrorNode = true;
                this.error = new CompletedWithExceptionException();
            }
        }
        private async System.Threading.Tasks.Task DeleteOnlyActionAsync(string ruleId, string jobId, string subJobId, IEnumerable<ArchiveApproveReport> reader, bool isDSOJobAndNeedAddOneSiteCollectionDetail = false)
        {
            JobExecutionProcessStatisticExecutor.Instance.StartCalculateDeleteAndStubSummary(ruleId, out Guid deleteSummaryCookie);
            bool hasErrorNode = false;
            int errorType = int.MaxValue;
            SOArchiverJobInfoStatistics.Instance.IsNeedStatisticsAction = true;
            SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
            SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;
            try
            {
                Logger.Info($"current rule is delete to recycle bin?{mConfiguration.currentRule?.DeleteToRecycleBin}");
                using SiteStateTransitionScope siteStateTransitionScope = TryUnlockSiteCollection(mConfiguration);
                InitExportType(ruleId);
                using (IBackwardDependencyNodeCache<CacheNode> cacheSPObjs = new BackwardDependenceNodeCache<CacheNode>())
                {
                    InitBackupers(null, cacheSPObjs);
                    using (IMultiDeleteController deleteController = new MultiDeleteController(mConfiguration,
                                              mConfiguration.BackgroundSettings.TotalMultiDeleteThreadNumber,
                                              mConfiguration.BackgroundSettings.EnableMultiBackup
                                              ))
                    {
                        IBackupController backupController = new MultiBackupController(null,
                                                           mConfiguration.BackgroundSettings.TotalMultiBackupThreadNumber,
                                                           mConfiguration.BackgroundSettings.EnableMultiBackup,
                                                           mConfiguration.BackgroundSettings.TotalTransferQueueNumber);
                        ArchiverDeletion mDeletion = new ArchiverDeletion(mConfiguration);
                        foreach (ArchiveApproveReport entity in reader)
                        {
                            using (new CheckJobStopScope()) { }
                            try
                            {
                                if (isDSOJobAndNeedAddOneSiteCollectionDetail)
                                {
                                    mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                                    isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                                }
                                #region errorType
                                if (entity.CacheNodeType > errorType)
                                {
                                    Logger.Info("Current item:{0} CacheNodeType:{1} large than errorType:{2} so UpdateStatus to Failed.NodeId:{3}.", entity.FullPath, entity.CacheNodeType, errorType, entity.NodeId);
                                    if (entity.CacheNodeType != (int)CacheNodeType.ItemVersion)
                                    {
                                        //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, jobId);
                                    }
                                    continue;
                                }
                                else
                                {
                                    errorType = int.MaxValue;
                                }
                                #endregion
                                if (mConfiguration.actionType == ActionType.ExportBeforeDelete)
                                {
                                    if (entity.CacheNodeType == (int)CacheNodeType.APP)
                                    {
                                        Logger.Info("Skip APP node:{0}.", entity.FullPath);
                                        continue;
                                    }
                                    SPObjectBackup backup = mVaults[GetCacheNodeType(entity.CacheNodeType)];
                                    CacheNode cacheNode = new CacheNode()
                                    {
                                        Sender = null,//backup.AveSender,
                                    };
                                    cacheNode.DoDelete = entity.DoDelete;
                                    //RegisterSecondHeaderEventHandler(cacheNode);
                                    var backupNodeParameters = new BackupNodeParameters()
                                    {
                                        CacheSPObjs = cacheSPObjs,
                                        Node = entity,
                                        BackupObj = backup,
                                        CacheNode = cacheNode,
                                        RuleName = mConfiguration.currentRule.Name,
                                        SubJobId = subJobId,
                                        RuleLevel = (int)mConfiguration.currentRule.PolicyLevel,
                                        MediaName = string.Empty,
                                        Sender = null,
                                        Configuration = mConfiguration
                                    };
                                    await backupController.ProcessAsync(backupNodeParameters);
                                }
                                GetKeepDataOnlyContainerObject(entity);
                                bool isVersion = false;
                                string message = GetDeletionNodeMessage(entity, ref isVersion);
                                //1.高级别Rule，低级别不需要Delete.Delete Only，什么级别Rule，删除什么级别数据
                                //2.高级别Rule，ExportBeforeDelete操作，需要等Container Level下的数据，Export成功后再删除Container
                                DeletionNode deletionNode = new DeletionNode(message);
                                //mLog.Info("GetDeletionNodeMessage:{0}.", message);
                                //判断是否是Container level rule
                                if (IsContainerLevelRule(mConfiguration.currentRule.PolicyLevel))
                                {
                                    //判断当前这条数据是否符合当前container rule，不符合则不处理.例如document
                                    if (CheckObjectIsFitRulePolicyLevel(entity.CacheNodeType, mConfiguration.currentRule.PolicyLevel))
                                    {
                                        //判断是否是Export Before Delete Only操作，如果是则先Export后删除。否则直接删除Container
                                        if (mConfiguration.actionType == ActionType.ExportBeforeDelete)
                                        {
                                            if (deletionNodes.Count > 0)
                                            {
                                                deleteController.Process(deletionNodes.FirstOrDefault(), mDeletion);
                                                deletionNodes.Clear();
                                            }
                                            deletionNodes.Add(deletionNode);
                                        }
                                        else
                                        {
                                            deleteController.Process(deletionNode, mDeletion);
                                        }
                                    }
                                }
                                else
                                {
                                    if (mConfiguration.currentRule.PolicyLevel == PolicyLevel.Document)
                                    {
                                        //低级别rule不需要处理version
                                        if (entity.CacheNodeType == (int)CacheNodeType.ItemVersion || entity.CacheNodeType == (int)CacheNodeType.Attachment)
                                        {
                                            Logger.Info("Current object is version or attachments so we skip it in DeleteOnly action.");
                                            continue;
                                        }
                                    }
                                    deleteController.Process(deletionNode, mDeletion);
                                }
                                mConfiguration.ProgressDto.HasCompleteNode = true;
                                SOProgressScAndFileStatistic.Instance().IncreaseFileCount(1, entity.NodeType);
                            }
                            #region
                            catch (Exception e)
                            {
                                errorType = entity.CacheNodeType;
                                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARBackupMainBackuperrorinbackup, e.ToString());
                                Logger.Warn("[DeleteOnlyActionAsync][CompletedWithExceptionException]Set the HasErrorNode true.");
                                mConfiguration.ProgressDto.HasErrorNode = true;
                                //mConfiguration.soArchiverQueryWorker.UpdateStatus(SOApproveDBStatus.Failed, entity.NodeId, subJobId);
                                AddBackupCommons(entity.CacheNodeType);
                                this.error = new CompletedWithExceptionException();
                            }
                            #endregion
                        }
                        if (deletionNodes.Count > 0)
                        {
                            deleteController.Process(deletionNodes.FirstOrDefault(), mDeletion);
                            deletionNodes.Clear();
                        }
                        backupController.Finish();
                        if (isDSOJobAndNeedAddOneSiteCollectionDetail && mConfiguration.JobReportDto.summaryComments != "RM_JM_SiteStorageLimit_ErrorMessage")
                        {
                            mConfiguration.JobReportDto.AddScanReport(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, "");
                            isDSOJobAndNeedAddOneSiteCollectionDetail = false;
                        }
                        deleteController.WaitForFinish();
                        CacheNARANAAExportMetadata(subJobId);
                        deleteController.Dispose();
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (AveSkipLockSiteException ex)
            {
                mConfiguration.JobReportDto.AddDetailOnly(mConfiguration.SiteCollectionUrl, 0, (int)CacheNodeType.SiteCollection, JobDetailsStatus.Failed, mConfiguration.currentRule.Name, ex.Message, string.Empty);
                Logger.Warn("[DeleteOnlyActionAsync][AveSkipLockSiteException]Set the HasErrorNode true.");
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryCommentsDetails = ex.Message;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(LOGRESOURCE.StorageOptimization13_SOARSOArchiveBackupError, e.ToString());
                if (e is ExportConfigurationFileError)
                {
                    Logger.Warn("[DeleteOnlyActionAsync][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.Message);
                    }
                }
                else if (e.InnerException != null && e.InnerException is ExportConfigurationFileError)
                {
                    Logger.Warn("[DeleteOnlyActionAsync][InnerException][ExportConfigurationFileError]Set the HasErrorNode true.");
                    mConfiguration.ProgressDto.HasErrorNode = true;
                    if (mConfiguration.JobReportDto != null)
                    {
                        mConfiguration.JobReportDto.AddVaultReport("", 0, JobDetailsStatus.Failed, (int)CacheNodeType.Exception, mConfiguration.JobId, "", "", e.InnerException.Message);
                    }
                }
                mConfiguration.ProgressDto.HasErrorNode = true;
                if (mConfiguration.JobReportDto.summaryComments == null)
                {
                    mConfiguration.JobReportDto.summaryCommentsDetails = e.Message;  //SAAS-13233 更改failed的comment,增加error message
                }
                //会有问题，因为这个地方需要更新的是子子Job 的信息，所以应该是RuleID + subjobid
                //mConfiguration.soArchiverQueryWorkerForJob.UpdateStatus(SOApproveDBStatus.Failed, jobId);
            }
            finally
            {
                JobExecutionProcessStatisticExecutor.Instance.EndCalculateDeleteAndStubSummary(ruleId, deleteSummaryCookie);
            }
            if (mConfiguration.ProgressDto.HasErrorNode && !hasErrorNode) hasErrorNode = mConfiguration.ProgressDto.HasErrorNode; //SAAS-12584 记录下执行完一个rule有没有Error Node。
        }

        private void StartMergeIndexJobByAgentSide(string subJobId, ArchiverSiteInfoDto siteInfo, bool noScanData = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.StartMergeIndexJobByAgentSide"))
            {
                Logger.Info("Begin MergeIndexJobByAgentSide, jobId:{0}. noScandata {1}", subJobId, noScanData);
                IdentityManager.IdentityMode = IdentityMode.Process;
                IdentityManager.IdentityType = ServiceConstants.IdentityTypeGroupId;
                IdentityManager.IdentityContent = TenantLocalValue.LogonGroupId;
                ArchiverMediaCenter mediaCenter = new ArchiverMediaCenter();
                var mergeIndexJobInfo = new MergeIndexJobInfo();
                var sub_sub_JobIds = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubSubJobIDs(subJobId);
                var indexDeviceDto = StorageDeviceService.GetIndexDevice();
                mergeIndexJobInfo.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
                mergeIndexJobInfo.MergeIndexJobsState = new System.Collections.Generic.List<MergeIndexJobState>();
                foreach (var id in sub_sub_JobIds)
                {
                    if (!mergeJobList.Keys.Contains(id))
                    {
                        Logger.Info($"mergeIndexJobInfo ID:{id}");
                        mergeIndexJobInfo.MergeIndexJobsState.Add(new MergeIndexJobState(id, true));//AR20211102135003207019A0_000_001 此处获取当前SubJob下的所有子子Job
                    }
                }
                mergeIndexJobInfo.SiteUrl = siteInfo.SiteUrl;
                mergeIndexJobInfo.JobDto = new GCommon.Contract.Server.ControlPanel.Object.BaseJobDto();
                mergeIndexJobInfo.JobDto.Id = subJobId;
                mergeIndexJobInfo.SiteId = siteInfo.SiteId;
                Logger.Info($"StartMergeIndexJobByAgentSide backupSubsubJobId:{subJobId}.mergeJobId:{subJobId}.msg.SubJobId:{subJobId}. mergeIndexJobInfo.MergeIndexJobsState count:{mergeIndexJobInfo.MergeIndexJobsState.Count}.");
                if (mergeIndexJobInfo.MergeIndexJobsState.Count == 0)
                {
                    Logger.Info($"No files meet the rules and no need merge index job. End MergeIndexJobByAgentSide, jobId:{subJobId}.");
                    noScanData = true;
                }
                var archiverSiteMasterIndexContracts = new System.Collections.Generic.List<ArchiverSiteMasterIndexContract>();
                ArchiverSiteMasterIndexContract contract = new ArchiverSiteMasterIndexContract();
                contract.SiteURL = siteInfo.SiteUrl;
                contract.SiteId = siteInfo.SiteId;
                contract.LockedJobId = subJobId;
                contract.WebId = TenantLocalValue.LogonGroupId;
                archiverSiteMasterIndexContracts.Add(contract);
                var mergeJobStatus = SOApproveDBStatus.Approved;

                try
                {
                    //等到Merge Job 锁一小时
                    using var indexDbLocker = SampleDBLocker.Get4IndexDBUpdater(
                        siteInfo.SiteUrl, siteInfo.SiteId, subJobId, TimeSpan.FromHours(1)
                    ).GetAwaiter().GetResult();

                    if (!noScanData)
                    {
                        var state = mediaCenter.HandleArchiverSubJobMergeIndexMessageWithState(mergeIndexJobInfo, subJobId);

                        foreach (var mergeJob in mergeIndexJobInfo.MergeIndexJobsState)
                        {
                            if (!mergeJobList.Keys.Contains(mergeJob.JobId))
                            {
                                if (state == JobState.Finished)
                                {
                                    mergeJobList.Add(mergeJob.JobId, MergeIndexState.Succeed);
                                }
                                else
                                {
                                    mergeJobList.Add(mergeJob.JobId, MergeIndexState.Failed);
                                }
                            }
                        }
                        mergeJobStatus = SOApproveDBStatus.Archived;
                    }
                }
                catch (Exception mergeJobException)
                {
                    mergeJobStatus = SOApproveDBStatus.Failed;
                    Logger.Error(string.Format("Error in end user merge job, reason : {0}.", mergeJobException.ToString()));
                    foreach (var mergeJob in mergeIndexJobInfo.MergeIndexJobsState)
                    {
                        if (!mergeJobList.Keys.Contains(mergeJob.JobId))
                        {
                            mergeJobList.Add(mergeJob.JobId, MergeIndexState.Failed);
                        }
                    }
                }
                finally
                {
                    Logger.Info(string.Format("Relase lock in merge job, site url is : {0}, job id is : {1}", siteInfo.SiteUrl, subJobId));
                    Logger.Info("End MergeIndexJobByAgentSide, jobId:{0}.", subJobId);
                }
            }
        }

        private void ExecuteRetentionAction(string subJobId, ArchiverSiteInfoDto siteInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverBackup.StartMergeIndexJobByAgentSide"))
            {
                try
                {
                    if (mConfiguration.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
                    {
                        Logger.Info($"wait run lc retention in un virtual job, sub job id:{subJobId}, sc:{siteInfo.SiteUrl}");
                        return;
                    }
                    //等到Merge Job 锁一小时
                    using var indexDbLocker = SampleDBLocker.Get4IndexDBUpdater(
                        siteInfo.SiteUrl, siteInfo.SiteId, subJobId, TimeSpan.FromHours(1)
                    ).GetAwaiter().GetResult();

                    try
                    {
                        var indexDeviceDto = StorageDeviceService.GetIndexDevice();
                        ArchiverPruningJob pruningJob = new ArchiverPruningJob();
                        pruningJob.FarmName = "";
                        pruningJob.JobId = subJobId;
                        pruningJob.SiteUrl = siteInfo.SiteUrl;
                        pruningJob.WebApp = siteInfo.WebApplicationUrl;
                        pruningJob.ArchiverBackupTime = 0;
                        pruningJob.StoragePolicyId = mConfiguration.RuleCollection.First().Value.StoragePolicyId;
                        pruningJob.RetentionAction = MediaArchiverRetentionAction.DeleteData;
                        pruningJob.RetentionJob = new SOJob() { Id = subJobId };

                        var resultRule = ReBuildArchiveRulesAsync(mConfiguration.RuleCollection).GetAwaiter().GetResult();

                        pruningJob.DataLogicalDevice = BuilDataLogical(mConfiguration.RuleCollection.Values);
                        if (pruningJob.DataLogicalDevice.PhysicalDrives?.Count > 0)
                        {
                            pruningJob.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
                            pruningJob.IsDeleteJob = false;
                            CacheSettingDto cache = new CacheSettingDto();
                            cache.Extension = new CacheSettingExtension();
                            cache.Extension.Path = new List<PathMap>
                                    {
                                        new PathMap() { DiskInfo = new DiskInfoDto() { Path = BackgroundSettings.GetInstance().ArchiveTemp } }
                                    };
                            pruningJob.CacheSettings = cache;
                            var retentionInfo = new ArchiverLifecycleRetentionInfo(pruningJob);
                            retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData;
                            retentionInfo.Rules = resultRule;
                            retentionInfo.JobType = (int)mJobType;
                            retentionInfo.NodeLevel = mConfiguration.RunJobNodeLevel;
                            retentionInfo.SiteId = mConfiguration.SiteCollectionID.ToString();
                            retentionInfo.SiteGroupId = new Guid(mConfiguration.WebAppId);
                            retentionInfo.IsOneDriveSite = this.IsOneDriveSite(pruningJob.SiteUrl);
                            retentionInfo.IsTeams = mConfiguration.IsTeams;
                            var retentionService = MediaServiceFactory.CreateLifecycleRetentionService();
                            ArchiverLifecycleRetentionResult result = retentionService.Retain(retentionInfo, new Action<JMJobDetails>(SendRetentionJobReport)) as ArchiverLifecycleRetentionResult;
                            //analyze result
                            if (result.sucessItem != null && result.sucessItem.Count > 0)
                            {
                                Logger.Info("Total sucess retention count {0}", result.sucessItem.Count);
                                foreach (var item in result.sucessItem)
                                {
                                    ExplorerDao.UpdateAll(i => i.ScopeId == mConfiguration.SiteCollectionID && i.NodeId == new Guid(item.NodeGuid) && i.RecordStatus != 10, r => { r.RecordStatus = 10; });
                                    AssembleRetentionDetail(item);
                                }
                            }

                            if (result.manualSkippedItem != null && result.manualSkippedItem.Count > 0)
                            {
                                Logger.Info("Total manual skip retention count {0}", result.sucessItem.Count);
                                foreach (var item in result.manualSkippedItem)
                                {
                                    AssembleRetentionDetailForManualSkip(item);
                                }
                            }

                            if (result.DoesNotSupportSharePointItem != null && result.DoesNotSupportSharePointItem.Count > 0)
                            {
                                Logger.Info("Total manual does not support SP Group retention count {0}", result.DoesNotSupportSharePointItem.Count);
                                foreach (var item in result.DoesNotSupportSharePointItem)
                                {
                                    AssembleRetentionDetailForDoesNotSupportSPGroupForRetention(item);
                                }
                            }
                        }
                        else
                        {
                            Logger.Info("There aren't rules enabled LF retention.");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);
                    }
                }
                catch (Exception d)
                {
                    Logger.Error(string.Format("Error in do retention, reason : {0}.", d.ToString()));
                }
                finally
                {
                    Logger.Info(string.Format("Relase lock in retention job, site url is : {0}, job id is : {1}", siteInfo.SiteUrl, subJobId));
                }
            }
        }

        //private bool CanBeRunMergeIndexJob(string siteId, List<ArchiverSiteMasterIndexContract> archiverSiteMasterIndexContracts)
        //{
        //    bool canBeRunMergeIndexJob = false;
        //    try
        //    {
        //        Logger.Info($"Begin CanBeRunMergeIndexJob.SiteId:{siteId}.");
        //        using (RMRedisLockHandler.LockAsync(RMRedisLockKey.SOCheckMergeIndexLock, siteId, TimeSpan.FromMinutes(5)).GetAwaiter().GetResult())
        //        {
        //            canBeRunMergeIndexJob = ArchiverIndexLockDao.CanBeRunJob(archiverSiteMasterIndexContracts);
        //        }
        //        Logger.Info($"Finished CanBeRunMergeIndexJob.SiteId:{siteId}.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Info($"Warn CanBeRunMergeIndexJob.SiteId:{siteId}.Message:{ex}.");
        //    }
        //    return canBeRunMergeIndexJob;
        //}

        private void SendRetentionJobReport(JMJobDetails details)
        {
            mConfiguration.JobReportDto.SendJobDetailForRetention(details);
        }

        private string GenerageSubJobId(string parentJobId)
        {
            subJobNumber++;
            if (subJobNumber >= 1000)
            {
                return string.Format("{0}_{1:D4}", parentJobId, subJobNumber);
            }
            else
            {
                return string.Format("{0}_{1:D3}", parentJobId, subJobNumber);
            }
        }

        private ArchiverBackupJob ConvertBackupRequestToJob(ArchiverBackupRequest aRequest)
        {
            ArchiverBackupJob archiverBackupJob = new ArchiverBackupJob(aRequest);
            if (this.mConfiguration.IsILMode)
            {
                archiverBackupJob.IsRAJob = true;  //pass isRAJob to media message for new feature
                if (this.mConfiguration.BackgroundSettings.RecordsOutputStreamLevel == 0)
                {
                    archiverBackupJob.OutFileLevelBlock = true;
                }
            }
            else
            {
                if (this.mConfiguration.BackgroundSettings.ArchiverOutputStreamLevel == 0)
                {
                    archiverBackupJob.OutFileLevelBlock = true;
                }
            }
            archiverBackupJob.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = string.Empty,
                UserName = string.Empty,
                Usage = null
            };
            archiverBackupJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            archiverBackupJob.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            archiverBackupJob.O365TenantId = mConfiguration.O365TenantId;
            return archiverBackupJob;
        }

        private LogicalDeviceDto BuilDataLogical(IEnumerable<Rule> rules)
        {
            LogicalDeviceDto logicalDeviceDto = new LogicalDeviceDto();
            if (rules.Count() > 0)
            {
                bool init = false;
                List<string> ids = new List<string>();
                foreach (Rule r in rules)
                {
                    if (r.IsEnableRetention)
                    {
                        if (!init)
                        {
                            logicalDeviceDto = r.StoragePolicyDto.PrimaryStorage;
                            ids.Add(logicalDeviceDto.Id);
                            init = true;
                        }
                        else
                        {
                            foreach (var phy in r.StoragePolicyDto.PrimaryStorage.PhysicalDrives)
                            {
                                if (!ids.Contains(phy.Id))
                                {
                                    logicalDeviceDto.PhysicalDrives.Add(phy);
                                }
                            }
                        }
                    }
                }

            }
            return logicalDeviceDto;
        }

        private void InitBackupers(BackupInfoSender sender, IBackwardDependencyNodeCache<CacheNode> cacheSPObjs, Queue<string> secondHeaderQueue = null)
        {
            foreach (SPObjectBackup backupObj in mBackups.Values)
            {
                backupObj.CacheSPObjs = cacheSPObjs;
            }
            foreach (SPObjectBackup backupObj in mVaults.Values)
            {
                //backupObj.AveSender = sender;
                backupObj.CacheSPObjs = cacheSPObjs;
            }
            foreach (SPObjectBackup backupObj in mRecordManager.Values)
            {
                //backupObj.AveSender = sender;
                backupObj.CacheSPObjs = cacheSPObjs;
            }
        }

        private void InitRelativeDataBackupers(BackupInfoSender sender, IBackwardDependencyNodeCache<CacheNode> cacheSPObjs, Queue<string> secondHeaderQueue = null)
        {
            foreach (SPObjectBackup backupObj in mRelativeDataSPObject.Values)
            {
                backupObj.CacheSPObjs = cacheSPObjs;
            }
        }

        private SPObjectBackup GetRelativeDataBackupObject(ArchiveApproveReport entity)
        {
            //检查JOB是否被停止。
            bool isWeb = entity.CacheNodeType >= (int)CacheNodeType.Web && entity.CacheNodeType < (int)CacheNodeType.APP;
            bool isFolder = false;
            bool isFolderChild = false;
            bool isApp = entity.CacheNodeType == (int)CacheNodeType.APP;
            int x = entity.CacheNodeType / 1000;
            int y = entity.CacheNodeType % 1000;
            isFolder = (x > 1 && x < 10) || (x == 1 && y > 0);
            isFolderChild = x >= 10;

            bool isVersion = entity.CacheNodeType == (int)CacheNodeType.ItemVersion;
            int nodeType = entity.CacheNodeType;
            if (isWeb)
            {
                nodeType = (int)CacheNodeType.Web;
            }
            else if (isFolder)
            {
                nodeType = (int)CacheNodeType.Folder;
            }
            else if (isVersion)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (isApp)
            {
                nodeType = (int)CacheNodeType.APP;
            }
            return mRelativeDataSPObject[nodeType];
        }

        private void DisposeRelativeDataBackupers()
        {
            try
            {
                if (mRelativeDataSPObject != null && mRelativeDataSPObject.Values != null)
                {
                    foreach (SPObjectBackup backupObj in mRelativeDataSPObject.Values)
                    {
                        if (backupObj is IDisposable)
                            using (backupObj) { }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(string.Format("Dispose end user backup failed, reason : {0}", ex.ToString()));
            }
        }

        private SPObjectBackup GetBackupObject(ArchiveApproveReport entity)
        {
            //检查JOB是否被停止。
            bool isWeb = entity.CacheNodeType >= (int)CacheNodeType.Web && entity.CacheNodeType < (int)CacheNodeType.APP;
            bool isFolder = false;
            bool isFolderChild = false;
            bool isApp = entity.CacheNodeType == (int)CacheNodeType.APP;
            int x = entity.CacheNodeType / 1000;
            int y = entity.CacheNodeType % 1000;
            isFolder = (x > 1 && x < 10) || (x == 1 && y > 0);
            isFolderChild = x >= 10;

            bool isVersion = entity.CacheNodeType == (int)CacheNodeType.ItemVersion;
            int nodeType = entity.CacheNodeType;
            if (isWeb)
            {
                nodeType = (int)CacheNodeType.Web;
            }
            else if (isFolder)
            {
                nodeType = (int)CacheNodeType.Folder;
            }
            else if (isVersion)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (isApp)
            {
                nodeType = (int)CacheNodeType.APP;
            }
            if (mConfiguration.currentRule is { MoveToRecordCenterAndDelareSetting: not null, ExportInfo: null })
            {
                return mRecordManager[nodeType];
            }
            else if (mConfiguration.VaultRulesCollection.ContainsKey(mConfiguration.currentRule.Id))
            {
                return mVaults[nodeType];
            }
            else
            {
                return mBackups[nodeType];
            }
        }

        private void SendSecondHeaders(ResponseHandle responseHandle)
        {
            if (System.IO.File.Exists(secondHeaderFilePath))
            {
                Logger.Info($"Second header file exist.path:{secondHeaderFilePath}");
                using (StreamReader streamReader = new StreamReader(secondHeaderFilePath))
                {
                    while (streamReader.Peek() > 0)
                    {
                        string tempHeader = streamReader.ReadLine();
                        try
                        {
                            responseHandle.SaveXmlHeader(tempHeader);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(string.Format("SendSecondHeaders error. Message:{0}", ex.ToString()));
                        }
                    }
                }
                System.IO.File.Delete(secondHeaderFilePath);
            }
            else
            {
                Logger.Info("Second header file not exist.");
            }
        }

        private BackupInfoSender ConfigMedia(string ruleId, string subJobId, ResponseHandle responseHandle, ref string IndexJobId, SourceFlag sourceFlag, SourceFlag dataFlag)
        {
            IArchiverBackupDataWriter fileSender = null;
            try
            {
                MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");
                fileSender = MediaServiceFactory.CreateArchiverBackupDataWriter(); //container.Resolve<IArchiverBackupDataWriter>("AvePoint.Media.Service.ArchiverBackup.Backup.IArchiverBackupDataWriter");
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Can't initialize media information. Message:{0}", ex.ToString()));
                throw;
            }
            //ArchiverBackupRequest aRequest = mConfiguration.BackupRequest;
            ArchiverBackupRequest aRequest = new ArchiverBackupRequest();
            aRequest.RuleId = ruleId;
            aRequest.SourceFlag = (int)sourceFlag;
            aRequest.DataFlag = (int)dataFlag;
            aRequest.JobId = GenerageSubJobId(subJobId);//subJobId;
            IndexJobId = aRequest.JobId;
            StoragePolicyDto storage = mConfiguration.currentRule.StoragePolicyDto;
            aRequest.UseSnapLock = mConfiguration.currentRule.UseSnapLock;
            aRequest.UseArchiverTier = mConfiguration.currentRule.IsArchivedTier;
            aRequest.StoragePolicyId = storage.Id;
            aRequest.AchiverTime = mConfiguration.ArchiverUNCTime.Ticks;
            //set RetentionTimeSpan
            if (storage.RetentionOption != null && storage.RetentionOption.StorageType == StoragePolicyType.ArchiveType && storage.RetentionOption.ArchiveRetentionRules != null && storage.RetentionOption.ArchiveRetentionRules.Count > 0)
            {
                ArchiveRetentionRule retentionRule = storage.RetentionOption.ArchiveRetentionRules[0];
                long keepValue = (long)retentionRule.KeepValue;
                switch (retentionRule.ArchiveDateUnit)
                {
                    case DateUnit.Month:
                        {
                            TimeSpan resultTime = DateTime.Now.AddMonths((int)keepValue).Subtract(DateTime.Now);
                            keepValue = resultTime.Days;
                            break;
                        }
                    case DateUnit.Week:
                        {
                            keepValue = keepValue * 7;
                            break;
                        }
                    default: break;
                }
                aRequest.RetentionTimeSpanSeconds = keepValue * 24 * 3600;
            }
            else
            {
                //when no retention rule ,we give RetentionTimeSpanSeconds = -1 
                aRequest.RetentionTimeSpanSeconds = -1;
            }

            aRequest.LogicalDevice = storage.PrimaryStorage;

            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            aRequest.IndexLogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);

            aRequest.CompressionType = mConfiguration.currentRule.ArchiverCompressionType;
            aRequest.EncryptionMethods = mConfiguration.currentRule.EncryptionMethods;
            aRequest.DataSecurity = mConfiguration.currentRule.ArchiverDataSecurity;
            aRequest.DataEncryptionInfoWrapper = mConfiguration.currentRule.DataEncryptionInfoWrapper;
            WrapperConfiguration.WrapperConfigurationForBPOS.DisableInformationRightsManagement = aRequest.DisableIRMSetting; //SAAS-14493 add IRM setting
                                                                                                                              //if (aRequest.IncludeListView)   ////SAAS-12519 增加Archiver的Include List View功能
                                                                                                                              //{
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView = true;//aRequest.IncludeListView;
            //}
            if (aRequest.WorkflowState != null)
            {
                if (aRequest.WorkflowState.IncludeWorkflowDefinition)
                {
                    SPWorkflowProcessorRuntime.ProcessAssociation = aRequest.WorkflowState.IncludeWorkflowDefinition;
                }
                if (aRequest.WorkflowState.IncludeWorkflowInstance)
                {
                    SPWorkflowProcessorRuntime.ProcessInstance = aRequest.WorkflowState.IncludeWorkflowInstance;
                }
            }
            if (mConfiguration.currentRule.DataEncryptionInfoWrapper != null)
            {
                aRequest.EncryptionInfo = mConfiguration.currentRule.DataEncryptionInfoWrapper.EncryptionInfo;
                DataEncryptionInfoManager.PutEncryptionInfo(mConfiguration.currentRule.DataEncryptionInfoWrapper.EncryptionInfo, mConfiguration.currentRule.DataEncryptionInfoWrapper.DynamicKey);
            }
            else
            {
                aRequest.EncryptionInfo = DataEncryptionInfoManager.DefaultEncryptionInfo;
            }
            Logger.Info("ArchiverBackupRequest EncryptionInfo is:{0}.", aRequest.EncryptionInfo == null ? string.Empty : aRequest.EncryptionInfo.ToString());
            //string backupRequestXml = MediaTCPRequestSerializerHelper.Serialize(aRequest);
            //TODO:Need remove or modified by ManagerSide
            aRequest.ArchiverSiteInfoDto = new ArchiverSiteInfoDto()
            {
                FarmName = "",
                WebApplicationUrl = mConfiguration.WebAppUrl,
                NewWebApplicationUrl = mConfiguration.WebAppUrl,
                WebApplicationId = mConfiguration.WebAppId,
                SiteId = mConfiguration.SiteCollectionID.ToString(),
                SiteUrl = mConfiguration.SiteCollectionUrl,
                NewSiteUrl = mConfiguration.SiteCollectionUrl,
            };

            if (!mConfiguration.CachedBackupJob.ContainsKey(IndexJobId))
            {
                mConfiguration.CachedBackupJob.Add(IndexJobId, aRequest);
            }
            fileSender.Open(ConvertBackupRequestToJob(aRequest));
            return new BackupInfoSender(fileSender);
        }
        private void RegisterSecondHeaderEventHandler(CacheNode cacheNode)
        {
            cacheNode.CustomizedDisposeAction = () =>
            {
                try
                {
                    CacheSecondHeader(cacheNode.GenerateSecondFileHeader());
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("Failed to save header, Message:{0}", ex.ToString()));
                    //TODO:Logging
                }
            };
        }

        private void CacheSecondHeader(string tempHeader)
        {
            if (string.IsNullOrEmpty(tempHeader))
            {
                Logger.Info("Current second Header IsNullOrEmpty.");
                return;
            }

            //mLog.Info(string.Format("Cache second Header for {0}", tempHeader));
            //mSecondFileHeaderCache.Enqueue(tempHeader);
            try
            {
                streamWriter.WriteLine(tempHeader);
                if (tempHeader.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Current second Header write failed,header:{tempHeader},it may caused the file delete failed,error:{e}.");
            }
        }

        private bool IsSQLiteException(string exceptionMsg)
        {
            bool result = false;
            string temp = String.Empty;
            try
            {
                string exceTypeString = "ExceptionType:";
                int tempindex = exceptionMsg.IndexOf(exceTypeString, StringComparison.OrdinalIgnoreCase);
                if (!String.IsNullOrEmpty(exceptionMsg) && tempindex >= 0)
                {
                    temp = exceptionMsg.Substring(tempindex + exceTypeString.Length);
                    if (temp != null && temp.TrimStart().StartsWith("System.Data.SQLite.SQLiteException", StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("IsSQLiteException() Error:{0}", ex.ToString());
            }
            return result;
        }

        private void CacheNARANAAExportMetadata(string subJobId)
        {
            if (mConfiguration.currentRule.ExportType == ExportTypeValue.NAA)
            {
                Logger.Info("Cache NAA Data {0}", subJobId);
                if (!NAAMetadatas.ContainsKey(subJobId))
                {
                    List<IVaultExport> exportObjs = new List<IVaultExport>();
                    exportObjs.Add(vaultExport);
                    NAAMetadatas.Add(subJobId, exportObjs);
                }
                else
                {
                    NAAMetadatas[subJobId].Add(vaultExport);
                }
            }
            if (mConfiguration.currentRule.ExportType == ExportTypeValue.NARA)
            {
                Logger.Info("Cache NARA Data {0}", subJobId);
                if (!NARAMetadatas.ContainsKey(subJobId))
                {
                    List<IVaultExport> exportObjs = new List<IVaultExport>();
                    exportObjs.Add(vaultExport);
                    NARAMetadatas.Add(subJobId, exportObjs);
                }
                else
                {
                    NARAMetadatas[subJobId].Add(vaultExport);
                }
            }
        }

        private void InitVaulters()
        {
            mVaults = new Dictionary<int, SPObjectBackup>();
            mVaults.Add((int)CacheNodeType.SiteCollection, new SiteCollectionVault(Logger) { Configuration = mConfiguration });
            mVaults.Add((int)CacheNodeType.Web, new WebVault(Logger) { Configuration = mConfiguration });
            mVaults.Add((int)CacheNodeType.List, new ListVault(Logger) { Configuration = mConfiguration });
            mVaults.Add((int)CacheNodeType.Folder, new FolderVault(Logger) { Configuration = mConfiguration });
            mVaults.Add((int)CacheNodeType.Item, new ItemVault(Logger) { Configuration = mConfiguration });
            mVaults.Add((int)CacheNodeType.Attachment, new AttachmentVault(Logger) { Configuration = mConfiguration });
        }

        private void InitBackupers()
        {
            mBackups = new Dictionary<int, SPObjectBackup>();
            mBackups.Add((int)CacheNodeType.SiteCollection, new SiteCollectionBackup(Logger) { Configuration = mConfiguration, VaultExport = new SiteCollectionVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.Web, new WebBackup(Logger) { Configuration = mConfiguration, VaultExport = new WebVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.List, new ListBackup(Logger) { Configuration = mConfiguration, VaultExport = new ListVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.Folder, new FolderBackup(Logger) { Configuration = mConfiguration, VaultExport = new FolderVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.Item, new ItemBackup(Logger) { Configuration = mConfiguration, VaultExport = new ItemVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.ArchiveBy365Item, new ArchiveBy365ItemBackup(Logger, mConfiguration.O365TenantId) { Configuration = mConfiguration, VaultExport = new ItemVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.HSMItem, new HSMItemBackup(Logger) { Configuration = mConfiguration, VaultExport = new ItemVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.HSMItemVersion, new HSMItemBackup(Logger) { Configuration = mConfiguration, VaultExport = new ItemVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.Attachment, new AttachmentBackup(Logger) { Configuration = mConfiguration, VaultExport = new AttachmentVault(Logger) { Configuration = mConfiguration } });
            mBackups.Add((int)CacheNodeType.APP, new AppDefinitionBackup(Logger) { Configuration = mConfiguration });
        }

        private void InitRelativeDataBackupers()
        {
            mRelativeDataSPObject = new Dictionary<int, SPObjectBackup>();
            mRelativeDataSPObject.Add((int)CacheNodeType.SiteCollection, new RelativeDataSiteCollectionBackup(Logger) { Configuration = mConfiguration, VaultExport = new SiteCollectionVault(Logger) { Configuration = mConfiguration } });
            mRelativeDataSPObject.Add((int)CacheNodeType.Web, new RelativeDataWebBackup(Logger) { Configuration = mConfiguration, VaultExport = new WebVault(Logger) { Configuration = mConfiguration } });
            mRelativeDataSPObject.Add((int)CacheNodeType.List, new RelativeDataListBackup(Logger) { Configuration = mConfiguration, VaultExport = new ListVault(Logger) { Configuration = mConfiguration } });
            mRelativeDataSPObject.Add((int)CacheNodeType.Folder, new RelativeDataFolderBackup(Logger) { Configuration = mConfiguration, VaultExport = new FolderVault(Logger) { Configuration = mConfiguration } });
            mRelativeDataSPObject.Add((int)CacheNodeType.Item, new RelativeDataItemBackup(Logger) { Configuration = mConfiguration, VaultExport = new ItemVault(Logger) { Configuration = mConfiguration } });
            mRelativeDataSPObject.Add((int)CacheNodeType.Attachment, new RelativeDataAttachmentBackup(Logger) { Configuration = mConfiguration, VaultExport = new AttachmentVault(Logger) { Configuration = mConfiguration } });
        }

        private void InitRecordManager()
        {
            mRecordManager = new Dictionary<int, SPObjectBackup>();
            mRecordManager.Add((int)CacheNodeType.SiteCollection, new SiteCollectionRecordManager(Logger) { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Web, new WebRecordManager(Logger) { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.List, new ListRecordManager(Logger) { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Folder, new FolderRecordManager(Logger) { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Item, new ItemRecordManager(Logger) { Configuration = mConfiguration });
            mRecordManager.Add((int)CacheNodeType.Attachment, new AttachmentRecordManager(Logger) { Configuration = mConfiguration });
        }
        private bool IsOneDriveSite(string siteUrl)
        {
            try
            {
                var daoSite = mConfiguration.IsILMode ? mConfiguration.GetRemoteSiteCollectionByRecords(siteUrl) : mConfiguration.GetRemoteSiteCollectionByDAO(siteUrl);
                bool isOnedrive = daoSite != null && daoSite.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro;
                Logger.Info("Is Onedrive site? {0}", isOnedrive);
                return isOnedrive;
            }
            catch (Exception e)
            {
                Logger.Warn(e.Message, e);
            }
            return false;
        }
        private void AssembleRetentionDetail(ArchiverBasicIndex index)
        {
            this.mConfiguration.JobReportDto.AddDetailOnly(index.Url, index.ContentLength, (int)CacheNodeType.Item, JobDetailsStatus.Successful, "", "");
        }

        private void AssembleRetentionDetailForManualSkip(ArchiverBasicIndex index)
        {
            this.mConfiguration.JobReportDto.AddDetailOnly(index.Url, index.ContentLength, (int)CacheNodeType.Item, JobDetailsStatus.Skipped, "", "RM_JM_FSFileWaitingForApproval");
        }

        private void AssembleRetentionDetailForDoesNotSupportSPGroupForRetention(ArchiverBasicIndex index)
        {
            this.mConfiguration.JobReportDto.AddDetailOnly(index.Url, index.ContentLength, (int)CacheNodeType.Item, JobDetailsStatus.Failed, "", "RM_MA_DoesNotSupportSPGroupForRetention");
        }

        /// <summary>
        /// Init Export Type and vaultExport Vaule
        /// </summary>
        /// <param name="ruleId"></param>
        private void InitExportType(string ruleId)
        {
            vaultExport = null;
            ExportpathGeneratorBase generator = null;
            if (mConfiguration.currentRule.ExportType != ExportTypeValue.None)
            {
                InitVaultState(ref generator, ruleId);
                foreach (SPObjectBackup backup in mBackups.Values)
                {
                    backup.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
                foreach (SPObjectBackup vault in mVaults.Values)
                {
                    vault.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
                foreach (SPObjectBackup RelativeDataExport in mRelativeDataSPObject.Values)
                {
                    RelativeDataExport.VaultBeforeArcInfo = new VaultBefArcInfo()
                    {
                        VaultExport = vaultExport,
                        VaultExportPathGenerator = generator
                    };
                }
            }
            else
            {
                foreach (SPObjectBackup backup in mBackups.Values)
                {
                    backup.VaultBeforeArcInfo = null;
                }
                foreach (SPObjectBackup vault in mVaults.Values)
                {
                    vault.VaultBeforeArcInfo = null;
                }
                foreach (SPObjectBackup RelativeDataExport in mRelativeDataSPObject.Values)
                {
                    RelativeDataExport.VaultBeforeArcInfo = null;
                }
            }
        }

        private void InitVaultState(ref ExportpathGeneratorBase generator, string ruleId)
        {
            VautlExportfactory factory = new VautlExportfactory();
            ExportTypeValue vaultExportType = mConfiguration.currentRule.ExportType;
            PhysicalDeviceDto physicalDto = mConfiguration.currentRule.PhysicalDeviceDto;
            SharePointLocationDto spoDto = null;
            AveBPOSAccountInfo accountInfoOfDestinationSpo = null;
            if (physicalDto == null)
            {
                Logger.Info("Using export to sharepoint library.");
                var (spoLibrary, accountInfo) = new MoveAction(mConfiguration).GetSharePointLibraryAndAccount().GetAwaiter().GetResult();
                spoDto = spoLibrary;
                accountInfoOfDestinationSpo = accountInfo;
            }
            Logger.Info("Vault Export Type is: {0}.", vaultExportType.ToString());
            byte[] exportEncryptionKeyBytes = null;
            byte[] exportEncryptionIVBytes = null;
            if (physicalDto != null || spoDto != null)
            {
                if (vaultExportType == ExportTypeValue.VEO && mConfiguration.IsUpgradedVEOV3 && !string.IsNullOrEmpty(mConfiguration.BackgroundSettings.VEOV3Type))
                {
                    Logger.Info("Export Type will change to :{0}Export.", mConfiguration.BackgroundSettings.VEOV3Type);
                    byte[] contentVEO = mConfiguration.currentRule.VEOContent;
                    byte[] historyVEO = mConfiguration.currentRule.VEOHistory;
                    vaultExport = physicalDto != null
                        ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.BackgroundSettings.VEOV3Type, (int)mConfiguration.currentRule.PolicyLevel, mConfiguration.currentRule.ArchiverSetting, contentVEO, historyVEO, mConfiguration.currentRule.ExportDataEncryptionKey)
                        : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.BackgroundSettings.VEOV3Type, (int)mConfiguration.currentRule.PolicyLevel, mConfiguration.currentRule.ArchiverSetting, contentVEO, historyVEO, mConfiguration.currentRule.ExportDataEncryptionKey);
                    generator = new VEOV3ExportPathGenerator(mConfiguration.TeamsAddress);
                    Logger.Info($"created VEO V3 export path generator. TeamsAddress: [{mConfiguration.TeamsAddress}]");
                    return;
                }

                if (vaultExportType == ExportTypeValue.VEO && !string.IsNullOrEmpty(mConfiguration.BackgroundSettings.VEOType))
                {
                    Logger.Info("Export Type will change to :{0}Export.", mConfiguration.BackgroundSettings.VEOType);
                    byte[] fileVEO = mConfiguration.currentRule.FileVEO;
                    byte[] recordVEO = mConfiguration.currentRule.RecordVEO;
                    byte[] manifestVEO = mConfiguration.currentRule.ManifestVEO;
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;
                    if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        Logger.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }

                    vaultExport = physicalDto != null
                        ? factory.Create(physicalDto, mConfiguration.JobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), mConfiguration.BackgroundSettings.VEOType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                        : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), mConfiguration.BackgroundSettings.VEOType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    //vaultExport.InitExportInfo(mConfiguration.VEOMetadataMapping);
                    generator = new VEOExportPathGenerator();
                }
                else if (vaultExportType == ExportTypeValue.NAA)
                {
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;

                    if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        Logger.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    vaultExport = physicalDto != null
                        ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                        : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    generator = new NAAExportPathGenerator(string.Empty, physicalDto?.Location, GetGlobalSettingColumnName(), mConfiguration.TeamsAddress);
                    Logger.Info($"created NAA export path generator. TeamsAddress: [{mConfiguration.TeamsAddress}]");
                }
                else if (vaultExportType == ExportTypeValue.NARA)
                {
                    var recordsEncryptionKey = mConfiguration.currentRule.ExportDataEncryptionKey;
                    var recordsEncryptionIV = mConfiguration.currentRule.ExportDataEncryptionIV;
                    if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                    {
                        Logger.Info("Export data encryption is enabled.");
                        exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                        exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                    }
                    vaultExport = physicalDto != null
                        ? factory.Create(physicalDto, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                        : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.JobId, mConfiguration.currentRule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), vaultExportType.ToString(), true), mConfiguration.currentRule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                    generator = new NARAExportPathGenerator(string.Empty, physicalDto?.Location, GetGlobalSettingColumnName(), mConfiguration.TeamsAddress);
                    Logger.Info($"created NARA export path generator. TeamsAddress: [{mConfiguration.TeamsAddress}]");
                }
            }
            else
            {
                Logger.Info("The Vault Before Archiver is false.");
            }
        }

        private int GetCacheNodeType(int cacheNodeType)
        {
            int nodeType = 0;
            if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                nodeType = (int)CacheNodeType.SiteCollection;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web && cacheNodeType < (int)CacheNodeType.List)
            {
                nodeType = (int)CacheNodeType.Web;
            }
            else if (cacheNodeType == (int)CacheNodeType.APP)
            {
                nodeType = (int)CacheNodeType.APP;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                nodeType = (int)CacheNodeType.List;
            }
            else if (cacheNodeType > (int)CacheNodeType.List && cacheNodeType < (int)CacheNodeType.Item)
            {
                nodeType = (int)CacheNodeType.Folder;
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (cacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                nodeType = (int)CacheNodeType.Item;
            }
            else if (cacheNodeType == (int)CacheNodeType.Attachment)
            {
                nodeType = (int)CacheNodeType.Attachment;
            }
            return nodeType;
        }

        private string GetObjectType(int cacheNodeType, int nodeType)
        {
            string objectType = string.Empty;
            if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                objectType = AveConstants.TYPE_SITE.ToString();
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web && cacheNodeType < (int)CacheNodeType.List)
            {
                objectType = AveConstants.TYPE_WEB.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.APP)
            {
                objectType = AveConstants.TYPE_APP.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                objectType = AveConstants.TYPE_LIST.ToString();
            }
            else if (cacheNodeType > (int)CacheNodeType.List && cacheNodeType < (int)CacheNodeType.Item)
            {
                objectType = AveConstants.TYPE_FOLDER.ToString();
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                if (nodeType == (int)ArchiverCommon.ItemType.ITEM_TYPE)
                {
                    objectType = AveConstants.TYPE_LISTITEM.ToString();
                }
                else if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT)
                {
                    objectType = AveConstants.TYPE_DOCUMENT.ToString();
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER)
                {
                    objectType = AveConstants.TYPE_VERSION.ToString();
                }
                else if (nodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
                {
                    objectType = AveConstants.TYPE_LISTITEMVERSION.ToString();
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.Attachment)
            {
                objectType = AveConstants.TYPE_ATTACHMENTS.ToString();
            }
            return objectType;
        }

        private void AddBackupCommons(int cacheNodeType)
        {
            string comment = string.Empty;
            if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                comment = LOGRESOURCE.StorageOptimization13_SOARBackupMainAddBackupCommonsSiteCollectionComments;
            }
            else if (cacheNodeType < (int)CacheNodeType.List)
            {
                comment = LOGRESOURCE.StorageOptimization13_SOARBackupMainAddBackupCommonsSiteComments;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                comment = LOGRESOURCE.StorageOptimization13_SOARBackupMainAddBackupCommonsListComments;
            }
            else
            {
                comment = LOGRESOURCE.StorageOptimization13_SOARBackupMainAddBackupCommonsItemComments;
            }

            mConfiguration.JobReportDto.summaryComments = comment;
        }

        /// <summary>
        /// one job -> one sharepoint group
        /// one sharepoint site group -> one column
        /// </summary>
        /// <returns></returns>
        private string GetGlobalSettingColumnName()
        {
            if (ScanDataCache.Instance.SiteLevelCache == null)
            {
                return string.Empty;
            }
            return ScanDataCache.Instance.SiteLevelCache.BCSColumnInternalName;
        }

        private void GetKeepDataOnlyContainerObject(ArchiveApproveReport entity)
        {
            switch (entity.CacheNodeType)
            {
                case (int)CacheNodeType.SiteCollection:
                case (int)CacheNodeType.Web:
                case (int)CacheNodeType.List:
                    #region get partSiteCollectionURL
                    if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
                    {
                        try
                        {
                            mConfiguration.siteUrlSchemeAndHost = new Uri(entity.FullPath).Scheme + @"://" + new Uri(entity.FullPath).Authority;
                            Logger.Info($"mConfiguration siteUrlSchemeAndHost:{mConfiguration.siteUrlSchemeAndHost}.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn("Can not get Site Collection URL while Init Keep Data Arguments." + ex.ToString());
                        }
                    }
                    #endregion
                    if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                    {
                        entity.FullPath = mConfiguration.siteUrlSchemeAndHost + entity.FullPath;
                    }
                    if (KeepDataOnlyContainer.ContainsKey(entity.CacheNodeType))
                    {
                        KeepDataOnlyContainer[entity.CacheNodeType] = entity;
                    }
                    else
                    {
                        KeepDataOnlyContainer.Add(entity.CacheNodeType, entity);
                    }
                    break;
                case (int)CacheNodeType.Item:
                    if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                    {
                        //entity.FullPath = (mConfiguration.siteUrlSchemeAndHost + entity.FullPath).Replace('\\', '/');
                        if (!entity.FullPath.StartsWith("/"))
                        {
                            entity.FullPath = (mConfiguration.siteUrlSchemeAndHost + "/" + entity.FullPath).Replace('\\', '/');
                            Logger.Warn($"Current Item FullPath not StartsWith slash and add slash.FullPath:{entity.FullPath}.");
                        }
                        else
                        {
                            entity.FullPath = (mConfiguration.siteUrlSchemeAndHost + entity.FullPath).Replace('\\', '/');
                        }
                    }
                    break;
                default:
                    //subsite CacheNodeType between 4~999
                    if (entity.CacheNodeType > (int)CacheNodeType.Web && entity.CacheNodeType < (int)CacheNodeType.List)
                    {
                        if (!entity.FullPath.StartsWith(mConfiguration.siteUrlSchemeAndHost))
                        {
                            entity.FullPath = mConfiguration.siteUrlSchemeAndHost + entity.FullPath;
                        }
                        if (KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.Web))
                        {
                            KeepDataOnlyContainer[(int)CacheNodeType.Web] = entity;
                        }
                        else
                        {
                            KeepDataOnlyContainer.Add((int)CacheNodeType.Web, entity);
                        }
                    }
                    break;
            }
        }

        private string GetDeletionNodeMessage(ArchiveApproveReport entity, ref bool isVersion)
        {
            string message = string.Empty;
            XmlDocument doc = new XmlDocument();
            XmlElement fileHeaderXml = doc.CreateElement("FileHeader");
            fileHeaderXml.SetAttribute(KeyWord.PATH, entity.LeafName);
            fileHeaderXml.SetAttribute(KeyWord.TYPE, GetObjectType(entity.CacheNodeType, entity.NodeType));
            fileHeaderXml.SetAttribute(KeyWord.NODEGUID, entity.NodeId);
            fileHeaderXml.SetAttribute(KeyWord.LEVEL, entity.Level.ToString());
            fileHeaderXml.SetAttribute(KeyWord.VERSION, entity.UIVersion.ToString());
            fileHeaderXml.SetAttribute(KeyWord.ID, entity.NodeId);
            fileHeaderXml.SetAttribute(KeyWord.RowId, entity.LibRowId.ToString());
            isVersion = IsVersion(entity.NodeType, entity.LeafName);
            fileHeaderXml.SetAttribute(KeyWord.ISVERSION, isVersion.ToString());
            fileHeaderXml.SetAttribute(KeyWord.URL, entity.FullPath);
            fileHeaderXml.SetAttribute(KeyWord.RULENAME, mConfiguration.currentRule.Name);
            fileHeaderXml.SetAttribute(KeyWord.SUBJOBID, mConfiguration.JobId);
            fileHeaderXml.SetAttribute(KeyWord.MYLEVEL, entity.ArchiveLevel.ToString());
            fileHeaderXml.SetAttribute(KeyWord.TIME, entity.ScanTime.ToString());
            fileHeaderXml.SetAttribute(KeyWord.FULLPATH, entity.FullPath);
            fileHeaderXml.SetAttribute(KeyWord.SIZE, entity.DocumentSize.ToString());
            fileHeaderXml.SetAttribute(KeyWord.Modified, entity.LastModifiedTime.ToString());
            fileHeaderXml.SetAttribute(KeyWord.DoDelete, entity.DoDelete.ToString());
            fileHeaderXml.SetAttribute(KeyWord.DeleteRelatedRecords, entity.DeleteRelatedRecords.ToString());
            fileHeaderXml.SetAttribute(KeyWord.SiteUrl, KeepDataOnlyContainer[(int)CacheNodeType.SiteCollection].SiteUrl);
            fileHeaderXml.SetAttribute(KeyWord.WebId, KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.Web) ? KeepDataOnlyContainer[(int)CacheNodeType.Web].WebID.ToString() : string.Empty);
            fileHeaderXml.SetAttribute(KeyWord.ListId, KeepDataOnlyContainer.ContainsKey((int)CacheNodeType.List) ? KeepDataOnlyContainer[(int)CacheNodeType.List].NodeId.ToString() : string.Empty);
            doc.AppendChild(fileHeaderXml);
            message = doc.InnerXml.ToString();
            return message;
        }

        private bool IsVersion(int nodeType, string leafName)
        {
            bool isVersion = false;
            if (nodeType == (int)ArchiverCommon.ItemType.DOCUMENT_VER || nodeType == (int)ArchiverCommon.ItemType.ITEM_VERSION)
            {
                isVersion = true;
            }
            return isVersion;
        }

        private bool IsContainerLevelRule(PolicyLevel policyLevel)
        {
            bool isContainerLevelRule = false;
            switch (policyLevel)
            {
                case PolicyLevel.SiteCollection:
                case PolicyLevel.Site:
                case PolicyLevel.List:
                case PolicyLevel.Folder:
                    isContainerLevelRule = true;
                    break;
                default:
                    break;
            }
            return isContainerLevelRule;
        }

        private bool CheckObjectIsFitRulePolicyLevel(int cacheNodeType, PolicyLevel currentRulePolicyLevel)
        {
            bool isFitRulePolicyLevel = false;
            switch (cacheNodeType)
            {
                case (int)CacheNodeType.WebApplication:
                    if (currentRulePolicyLevel == PolicyLevel.WebApplication)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                case (int)CacheNodeType.SiteCollection:
                    if (currentRulePolicyLevel == PolicyLevel.SiteCollection)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                case (int)CacheNodeType.List:
                    if (currentRulePolicyLevel == PolicyLevel.List)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                case (int)CacheNodeType.Item:
                    if (currentRulePolicyLevel == PolicyLevel.Item || currentRulePolicyLevel == PolicyLevel.Document)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                case (int)CacheNodeType.ItemVersion:
                    if (currentRulePolicyLevel == PolicyLevel.ItemVersion || currentRulePolicyLevel == PolicyLevel.DocumentVersion)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                case (int)CacheNodeType.Attachment:
                    if (currentRulePolicyLevel == PolicyLevel.Attachment)
                    {
                        isFitRulePolicyLevel = true;
                    }
                    break;
                default:
                    if (cacheNodeType >= (int)CacheNodeType.Web && cacheNodeType < (int)CacheNodeType.List)
                    {
                        if (currentRulePolicyLevel == PolicyLevel.Site)
                        {
                            isFitRulePolicyLevel = true;
                        }
                    }
                    else if (cacheNodeType >= (int)CacheNodeType.Folder && cacheNodeType < (int)CacheNodeType.Item)
                    {
                        if (currentRulePolicyLevel == PolicyLevel.Folder)
                        {
                            isFitRulePolicyLevel = true;
                        }
                    }
                    break;
            }
            return isFitRulePolicyLevel;
        }

        private void DisposeVEOExportMetadata()
        {
            if (vaultExport != null)
            {
                //针对EDRM，参数0 string：rule name ，参数1 int：分xml的个个数，
                if (mConfiguration.currentRule.ExportType == ExportTypeValue.VEO)
                {
                    RMRunningJobRuleMappingDao.AddJobMappingsForVEOMerge(TenantLocalValue.LogonGroupId, mConfiguration.MainJobId);
                    Logger.Info("Begin DisposeVEOExportMetadata.");
                    vaultExport.ExtensionMethod(mConfiguration.currentRule.Name, mConfiguration.BackgroundSettings.ManifestXmlSize);
                    vaultExport.Dispose();
                    Logger.Info("End DisposeVEOExportMetadata.");
                }
            }
        }

        private void DisposeNARANAAExportMetadata()
        {
            if (NAAMetadatas.Count > 0)
            {
                foreach (var jobid in NAAMetadatas.Keys)
                {
                    Logger.Info("begin build naa metadata file {0}.", jobid);
                    List<CsvMetaData> metadatas = new List<CsvMetaData>();
                    IVaultExport export = null;
                    foreach (var exportObj in NAAMetadatas[jobid])
                    {
                        metadatas.AddRange(exportObj.GetCSVMetadata());
                        if (exportObj.GetCSVMetadata().Count > 0)
                        {
                            export = exportObj;
                        }
                    }
                    if (export != null)
                    {
                        export.ExtensionMethod(metadatas);
                    }
                    Logger.Info("build naa metadata file success.metadatas Count:{0}.", metadatas.Count);
                    foreach (var exportObj in NAAMetadatas[jobid])
                    {
                        exportObj.Dispose();
                    }
                }
            }
            if (NARAMetadatas.Count > 0)
            {
                foreach (var jobid in NARAMetadatas.Keys)
                {
                    Logger.Info("begin build nara metadata file {0}.", jobid);
                    List<CsvMetaData> metadatas = new List<CsvMetaData>();
                    IVaultExport export = null;
                    foreach (var exportObj in NARAMetadatas[jobid])
                    {
                        metadatas.AddRange(exportObj.GetCSVMetadata());
                        if (exportObj.GetCSVMetadata().Count > 0)
                        {
                            export = exportObj;
                        }
                    }
                    if (export != null)
                    {
                        export.ExtensionMethod(metadatas);
                    }
                    Logger.Info("build nara metadata file success.metadatas Count:{0}.", metadatas.Count);
                    foreach (var exportObj in NARAMetadatas[jobid])
                    {
                        exportObj.Dispose();
                    }
                }
            }
        }

        private async Task<Dictionary<int, Rule>> ReBuildArchiveRulesAsync(Dictionary<int, Rule> ruleCollection)
        {
            Dictionary<int, Rule> resultRule = new Dictionary<int, Rule>() { };
            int i = 0;
            foreach (var r in ruleCollection.Values)
            {
                try
                {
                    await ReBuildArchiveRuleAsync(r);
                    resultRule.Add(i++, r);
                }
                catch (LicenseMismatchOfAvePointStorageException lme)
                {
                    Logger.Error($"ReBuildArchiveRulesAsync error : {lme}");
                }
            }
            return resultRule;
        }

        private async System.Threading.Tasks.Task ReBuildArchiveRuleAsync(Rule rule, bool useDefaultStorageWhenNoStorage = false)
        {
            try
            {
                WrapperConfiguration.MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving ? true : (rule.MoveToAnotherTierType == (int)Storage.AccessTierType.Other || rule.MoveToAnotherTierType == null) ? false : true;
                WrapperConfiguration.MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : (rule.MoveToAnotherTierType == null ? 0 : rule.MoveToAnotherTierType);
                RebuildRecordsMoveSetting(rule);
                await RebuildStubSettingsAsync(rule);
                RebuildStoragePolicyDto(rule, useDefaultStorageWhenNoStorage);
                RebuildExportSettings(rule);
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                Logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"ReBuildArchiveRule error : {e}");
                throw new ScheduleJobConfigurationError();
            }
        }

        private async System.Threading.Tasks.Task ReBuildAOSPArchiveRuleAsync(Rule rule, bool useDefaultStorageWhenNoStorage = false)
        {
            try
            {
                WrapperConfiguration.MoveToArchiverTierWhenArchiving = rule.MoveToArchiverTierWhenArchiving ? true : (rule.MoveToAnotherTierType == (int)Storage.AccessTierType.Other || rule.MoveToAnotherTierType == null) ? false : true;
                WrapperConfiguration.MoveToAnotherTierType = rule.MoveToArchiverTierWhenArchiving ? (int)Storage.AccessTierType.Archive : (rule.MoveToAnotherTierType == null ? 0 : rule.MoveToAnotherTierType);
                RebuildRecordsMoveSetting(rule);
                await RebuildStubSettingsAsync(rule);
                RebuildAOSPStoragePolicyDto(rule, useDefaultStorageWhenNoStorage);
                RebuildExportSettings(rule);
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                Logger.Error($"LicenseMismatchOfAvePointStorageException error : {lme}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"ReBuildArchiveRule error : {e}");
                throw new ScheduleJobConfigurationError();
            }
        }

        private void RebuildRecordsMoveSetting(Rule rule)
        {
            if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(rule.spMoveOption.MoveDestination.SPUrl))
            {
                rule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting();
                switch (rule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                {
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Skip:
                        rule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = AvePoint.GCommon.Contract.StorageOptimization.Object.ContentConflictResolution.Skip;
                        break;
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByName:
                        rule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = AvePoint.GCommon.Contract.StorageOptimization.Object.ContentConflictResolution.Append;
                        break;
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Overwrite:
                        rule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = AvePoint.GCommon.Contract.StorageOptimization.Object.ContentConflictResolution.Overwrite;
                        break;
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.NotOverwrite:
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByVersion:
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Replace:
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Merge:
                    case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.OverwriteByLastModifiedTime:
                    default:
                        Logger.Info("Not support ContentConflictResolution.");
                        break;
                }
                //rule.MoveToRecordCenterAndDelareSetting.DestFlag = RecordFlag.SP;
                rule.MoveToRecordCenterAndDelareSetting.DestinationLocation = new DestinationLocationInfo();
                rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url = rule.spMoveOption.MoveDestination.SPUrl;
                rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ContainerId = rule.spMoveOption.MoveDestination.ContainerId;
                rule.MoveToRecordCenterAndDelareSetting.DelaredRecord = rule.spMoveOption.MoveDestination.NotDeclareMovedData;
                rule.MoveToRecordCenterAndDelareSetting.IsMoveVersions = rule.spMoveOption.MoveDestination.IsMoveVersions;
                rule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource = false;
                rule.MoveToRecordCenterAndDelareSetting.OperateDataMode = OperatingSharePointDataMode.MoveToRecordCenterAndDelare;
                rule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML = false;
                rule.MoveToRecordCenterAndDelareSetting.UseTransferedFileMode = UseTransferedFileMode.KeepOriginalContentType;
                rule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification = rule.spMoveOption.MoveDestination.KeepSourceClassification;
                rule.MoveToRecordCenterAndDelareSetting.KeepFolderStructure = rule.spMoveOption.MoveDestination.KeepFolderStructure;
            }
            if (rule.ExportInfo is { spMoveOption.MoveDestination: not null } && !string.IsNullOrEmpty(rule.ExportInfo.spMoveOption.MoveDestination.SPUrl))
            {
                rule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting()
                {
                    DestinationLocation = new()
                    {
                        Url = rule.ExportInfo.spMoveOption.MoveDestination.SPUrl,
                        ContainerId = rule.ExportInfo.spMoveOption.MoveDestination.ContainerId
                    },
                    DelaredRecord = rule.ExportInfo.spMoveOption.MoveDestination.NotDeclareMovedData,
                    IsMoveVersions = rule.ExportInfo.spMoveOption.MoveDestination.IsMoveVersions,
                    LeaveLinkInSource = false,
                    OperateDataMode = OperatingSharePointDataMode.MoveToRecordCenterAndDelare,
                    OriginalMetaDataAsXML = false,
                    UseTransferedFileMode = UseTransferedFileMode.KeepOriginalContentType,
                    KeepSourceClassification = rule.ExportInfo.spMoveOption.MoveDestination.KeepSourceClassification,
                    KeepFolderStructure = rule.ExportInfo.spMoveOption.MoveDestination.KeepFolderStructure,
                };

            }

            //if (rule.OneDriveRule != null && rule.OneDriveRule.spMoveOption != null && rule.OneDriveRule.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl))
            //{
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting();
            //    switch (rule.OneDriveRule.spMoveOption.MoveSetting.ItemLevelConflictOption)
            //    {
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Skip:
            //            rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Skip;
            //            break;
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByName:
            //            rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Append;
            //            break;
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Overwrite:
            //            rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Overwrite;
            //            break;
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.NotOverwrite:
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByVersion:
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Replace:
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Merge:
            //        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.OverwriteByLastModifiedTime:
            //        default:
            //            Logger.Info("Not support ContentConflictResolution.");
            //            break;
            //    }
            //    //rule.MoveToRecordCenterAndDelareSetting.DestFlag = RecordFlag.SP;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.DestinationLocation = new DestinationLocationInfo();
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url = rule.OneDriveRule.spMoveOption.MoveDestination.SPUrl;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ContainerId = rule.OneDriveRule.spMoveOption.MoveDestination.ContainerId;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.DelaredRecord = rule.OneDriveRule.spMoveOption.MoveDestination.NotDeclareMovedData;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.IsMoveVersions = false;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource = false;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.OperateDataMode = OperatingSharePointDataMode.MoveToRecordCenterAndDelare;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML = false;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.UseTransferedFileMode = UseTransferedFileMode.KeepOriginalContentType;
            //    rule.OneDriveRule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification = rule.OneDriveRule.spMoveOption.MoveDestination.KeepSourceClassification;
            //}
        }

        private async System.Threading.Tasks.Task RebuildStubSettingsAsync(Rule rule)
        {
            if (!string.IsNullOrEmpty(rule.StubTemplateId))
            {
                var stubSettings = await LinkFileCommon.GetStubTemplatesByIdAsync(rule.StubTemplateId);
                if (stubSettings != null)
                {
                    Logger.Info($"The Stub type is {stubSettings.StubType}");
                    rule.LeaveStubType = (LeaveStubType)stubSettings.StubType;
                    rule.DeclareStubOption = mConfiguration.IsSupportRecordLabel ? (stubSettings.IsDeclareStubAsRecords ? DeclareStubType.AddRecordLabel : DeclareStubType.DeleteRecordLabel) : (stubSettings.IsDeclareStubAsRecords ? DeclareStubType.Declare : DeclareStubType.UnDeclare);
                    rule.DeclareLinkFile = stubSettings.IsDeclareStubAsRecords;
                    if (stubSettings.IsEnabledRetention)
                    {
                        rule.LeaveStubIsEnabledRetention = true;
                        rule.LeaveStubRetentionValue = stubSettings.RetentionValue;
                        rule.LeaveStubRetentionUnit = stubSettings.RetentionUnit;
                    }
                }
                else
                {
                    throw new Exception($"Cannot find the stub template by {rule.StubTemplateId}");
                }
            }
        }

        private void RebuildStoragePolicyDto(Rule rule, bool useDefaultStorageWhenNoStorage = false)
        {
            if ((rule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
            {
                Logger.Info($"RebuildStoragePolicyDto.Current rule is delete only action and skip build storage info.KeepDataOption:{rule.KeepDataOption}.");
                return;
            }
            var globalSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            if (useDefaultStorageWhenNoStorage && (string.IsNullOrWhiteSpace(rule.StoragePolicyId) || rule.StoragePolicyId.Equals(Guid.Empty.ToString())))
            {
                rule.StoragePolicyId = globalSetting.StoragePolicyId.ToString();
            }

            if (!string.IsNullOrEmpty(rule.StoragePolicyId))
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(storageDevice);
                var logical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                rule.StoragePolicyDto = new StoragePolicyDto()
                {
                    Id = storageDevice.Id,
                    Name = rule.Id,
                    PrimaryStorage = logical,
                    Type = storageDevice.Type,
                };
                if (storageDevice.SetupDataRetention)
                {
                    rule.StoragePolicyDto.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(storageDevice.ArchiveRetentionRules);
                }

                if (globalSetting != null)
                {
                    if (globalSetting.UseCompression)
                    {
                        rule.ArchiverCompressionType = (GCommon.Contract.GranularBackup.Object.CompressionType)globalSetting.CompressionSpeed;
                        rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                    }
                    if (globalSetting.UseEncryption)
                    {
                        storageDevice.EncryptionProfileId = globalSetting.SecurityProfileId.ToString();
                        var encryptionInfo = SettingProfileDao.LoadById(new Guid(storageDevice.EncryptionProfileId));
                        DataEncryptionProfile mProfile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);

                        if (mProfile.CurrentProtectionAlgorithm != null && mProfile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                        {
                            rule.EncryptionMethods = GCommon.Contract.GranularBackup.Object.EncryptionMethods.AES_ENCRYPTION;
                            rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                            rule.DataEncryptionProfileId = storageDevice.EncryptionProfileId;
                            rule.DataEncryptionInfoWrapper = new GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
                            var info = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                            byte[] result;
                            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(mProfile.KeyLength / 8);
                            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                            info.EncryptionType = mProfile.AlgorithmType;
                            info.ProfileGuid = storageDevice.EncryptionProfileId;
                            info.ProtectionGuid = storageDevice.EncryptionProfileId;
                            info.ProfileName = "Default Encryption Profile";
                            info.EncryptedDynamicKey = AesEncryptorWrapper.Encrypt(result);
                            rule.DataEncryptionInfoWrapper.EncryptionInfo = info;
                            rule.DataEncryptionInfoWrapper.DynamicKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(result);
                        }
                        else
                        {
                            Logger.Warn("Not the desired encryption method.");
                            if (mProfile.CurrentProtectionAlgorithm != null)
                            {
                                Logger.Warn("CurrentProtectionAlgorithm is null.");
                            }
                            else
                            {
                                Logger.Warn($"CurrentProtectionAlgorithm Type is {mProfile.CurrentProtectionAlgorithm.Type}.");
                            }
                        }
                        //Logger.Info("Get profile from aos by profileid {0}", storageDevice.EncryptionProfileId);
                        //var profile = PortalUtil.GetSecurityProfileById(storageDevice.EncryptionProfileId);
                        //Logger.Info("Get profile from aos by profileid successful");
                        //rule.EncryptionMethods = GCommon.Contract.GranularBackup.Object.EncryptionMethods.AES_ENCRYPTION;
                        //rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                        //rule.DataEncryptionProfileId = storageDevice.EncryptionProfileId;
                        //rule.DataEncryptionInfoWrapper = new GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
                        //var info = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                        //byte[] result;
                        //result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(mProfile.KeyLength / 8);
                        //info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                        //info.EncryptionType = mProfile.AlgorithmType;
                        //info.ProfileGuid = storageDevice.EncryptionProfileId;
                        //info.ProtectionGuid = storageDevice.EncryptionProfileId;
                        //info.ProfileName = profile.Name;
                        //KeyVaultServiceProvider provider = new KeyVaultServiceProvider(profile);
                        //info.EncryptedDynamicKey = provider.EncryptBinary(result);

                        //rule.DataEncryptionInfoWrapper.EncryptionInfo = info;
                        //rule.DataEncryptionInfoWrapper.DynamicKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(result);
                    }
                }
            }
        }

        private void RebuildAOSPStoragePolicyDto(Rule rule, bool useDefaultStorageWhenNoStorage = false)
        {
            var globalSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            if (useDefaultStorageWhenNoStorage && (string.IsNullOrWhiteSpace(rule.StoragePolicyId) || rule.StoragePolicyId.Equals(Guid.Empty.ToString())))
            {
                rule.StoragePolicyId = globalSetting.StoragePolicyId.ToString();
            }

            if (!string.IsNullOrEmpty(rule.StoragePolicyId))
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(rule.StoragePolicyId, needDecryptSecert: true);
                var logical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(storageDevice);
                rule.StoragePolicyDto = new StoragePolicyDto()
                {
                    Id = storageDevice.Id,
                    Name = rule.Id,
                    PrimaryStorage = logical,
                    Type = storageDevice.Type,
                };
                if (storageDevice.SetupDataRetention)
                {
                    rule.StoragePolicyDto.RetentionOption = StorageDeviceConvert.ConvertToRetentionRuleOption(storageDevice.ArchiveRetentionRules);
                }

                if (globalSetting != null)
                {
                    if (globalSetting.UseCompression)
                    {
                        rule.ArchiverCompressionType = (GCommon.Contract.GranularBackup.Object.CompressionType)globalSetting.CompressionSpeed;
                        rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.CompressionMedia;
                    }
                    if (globalSetting.UseEncryption)
                    {
                        storageDevice.EncryptionProfileId = globalSetting.SecurityProfileId.ToString();
                        var encryptionInfo = SettingProfileDao.LoadById(new Guid(storageDevice.EncryptionProfileId));
                        DataEncryptionProfile mProfile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);

                        if (mProfile.CurrentProtectionAlgorithm != null && mProfile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                        {
                            rule.EncryptionMethods = GCommon.Contract.GranularBackup.Object.EncryptionMethods.AES_ENCRYPTION;
                            rule.ArchiverDataSecurity = rule.ArchiverDataSecurity | GCommon.Contract.GranularBackup.Object.DataSecurity.EncryptionMedia;
                            rule.DataEncryptionProfileId = storageDevice.EncryptionProfileId;
                            rule.DataEncryptionInfoWrapper = new GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper.DataEncryptionInfoWrapper();
                            var info = new GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo();
                            byte[] result;
                            result = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(mProfile.KeyLength / 8);
                            info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                            info.EncryptionType = mProfile.AlgorithmType;
                            info.ProfileGuid = storageDevice.EncryptionProfileId;
                            info.ProtectionGuid = storageDevice.EncryptionProfileId;
                            info.ProfileName = "Default Encryption Profile";
                            info.EncryptedDynamicKey = AesEncryptorWrapper.Encrypt(result);
                            rule.DataEncryptionInfoWrapper.EncryptionInfo = info;
                            rule.DataEncryptionInfoWrapper.DynamicKey = AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(result);
                        }
                        else
                        {
                            Logger.Warn("Not the desired encryption method.");
                            if (mProfile.CurrentProtectionAlgorithm != null)
                            {
                                Logger.Warn("CurrentProtectionAlgorithm is null.");
                            }
                            else
                            {
                                Logger.Warn($"CurrentProtectionAlgorithm Type is {mProfile.CurrentProtectionAlgorithm.Type}.");
                            }
                        }
                    }
                }
            }
        }

        private void RebuildExportSettings(Rule rule)
        {
            GetExportConfiguration(rule, (int)SourceFlag.SharePoint);
        }

        private void GetExportConfiguration(Rule rule, int sourceFlag)
        {
            if (mConfiguration != null)
            {
                mConfiguration.IsUpgradedVEOV3 = rule.ExportType == ExportTypeValue.VEO && VEOV3CommonMethod.HasUpgradedVEOV3();
            }

            if (rule.ExportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                var physicalDeviceId = string.Empty;
                if (rule.ExportInfo is { newOptionsOfExportInfo: true })
                {
                    physicalDeviceId = rule.ExportInfo.exportLocationId;
                }
                else
                {
                    SettingProfileDto mDto = new SettingProfileDto()
                    {
                        Type = (int)SettingProfilesType.ExportLocationDevice,
                        Name = "UsingExportLocationDevice"
                    };
                    var dto = SettingProfileDao.Load(mDto);
                    if (dto != null)
                    {
                        physicalDeviceId = dto.Settings;
                    }
                }
                var storageDevice = StorageDeviceService.GetStorageDeviceById(physicalDeviceId);
                if (storageDevice != null)
                {
                    PhysicalDeviceDto physicalDto = new PhysicalDeviceDto()
                    {
                        ConnectionString = storageDevice.ConnectionString,
                        Type = storageDevice.Type
                    };

                    rule.PhysicalDeviceDto = physicalDto;
                }
            }

            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.VEO)
            {
                try
                {
                    var condition = mConfiguration.IsUpgradedVEOV3 ? (Func<RMCPExportSetting, bool>)(s => s.VEOContent != null && s.VEOHistory != null) : (s => s.VEOContent == null && s.VEOHistory == null);
                    var exportSetting = ExportSettingsDao.GetExportSettings((int)ExportSettingType.VEO).FirstOrDefault(condition);
                    if (exportSetting != null)
                    {
                        if (mConfiguration.IsUpgradedVEOV3)
                        {
                            Logger.Info("Get VEO V3 setting in the DB");
                            rule.VEOContent = exportSetting.VEOContent;
                            rule.VEOHistory = exportSetting.VEOHistory;
                        }
                        else
                        {
                            Logger.Info("Get VEO setting in the DB");
                            rule.FileVEO = exportSetting.FileVEO;
                            rule.RecordVEO = exportSetting.RecordVEO;
                            rule.ManifestVEO = exportSetting.ManifestVEO;
                        }

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(exportSetting.ArchiverSetting);
                        rule.ArchiverSetting = new ArchiverSetting();
                        //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                        if (mConfiguration.IsUpgradedVEOV3)
                        {
                            rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                            rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
                        }
                        else
                        {
                            rule.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                            rule.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                            rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                            rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                            rule.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;

                            doc.LoadXml(exportSetting.ArchiverVEOSetting);
                            rule.ArchiverVEOSetting = new ArchiverVEOSetting();
                            rule.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                            rule.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                            rule.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                            rule.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                        }
                    }
                    else
                    {
                        Logger.Info("Load VEO setting in the default config file.");
                        //rule.FileVEO = null;
                        //rule.RecordVEO = null;
                        //rule.ManifestVEO = null;
                        //rule.ArchiverSetting = null;
                        //rule.ArchiverVEOSetting = null;
                        //RECO 自己提供配置文件
                        var veoZipFileName = mConfiguration.IsUpgradedVEOV3 ? "VEO V3 Configuration Files" : "VEO Configuration Files";
                        var filepath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", veoZipFileName + ".zip");
                        var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Temp", "Config", veoZipFileName);
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);

                        string prefix = sourceFlag == (int)SourceFlag.SharePoint ? "" : "EXO";
                        if (mConfiguration.IsUpgradedVEOV3)
                        {
                            rule.VEOContent = GetMemoryStream(unZipFolder, $"{prefix}VEOContent.xml");
                            rule.VEOHistory = GetMemoryStream(unZipFolder, $"{prefix}VEOHistory.xml");
                        }
                        else
                        {
                            rule.FileVEO = GetMemoryStream(unZipFolder, $"{prefix}FileVEO.xml");
                            rule.RecordVEO = GetMemoryStream(unZipFolder, $"{prefix}RecordVEO.xml");
                            rule.ManifestVEO = GetMemoryStream(unZipFolder, $"{prefix}ManifestVEO.xml");

                        }

                        using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverSettings.config"), FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(sr.ReadToEnd());
                                rule.ArchiverSetting = new ArchiverSetting();
                                //rule.ArchiverSetting.NumberOfThreadSendingEmail = int.Parse(doc.SelectSingleNode("Configuration/numberOfThreadsSendingEmail").InnerXml);
                                if (mConfiguration.IsUpgradedVEOV3)
                                {
                                    rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileNumber").InnerXml);
                                    rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOV3Setting/fileSize").InnerXml);
                                }
                                else
                                {
                                    rule.ArchiverSetting.EnableArchiverVEOMerge = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge").Attributes["enable"].Value);
                                    rule.ArchiverSetting.IsDeleteOldFile = bool.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/isDeleteOldFile").InnerXml);
                                    rule.ArchiverSetting.FileNumber = int.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileNumber").InnerXml);
                                    rule.ArchiverSetting.FileSize = double.Parse(doc.SelectSingleNode("Configuration/archiverVEOMerge/fileSize").InnerXml);
                                    rule.ArchiverSetting.FolderName = doc.SelectSingleNode("Configuration/archiverVEOMerge/folderName").InnerXml;
                                }
                            }
                        }

                        if (!mConfiguration.IsUpgradedVEOV3)
                        {
                            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, "ArchiverVEOSettings.config"), FileMode.Open))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(sr.ReadToEnd());
                                    rule.ArchiverVEOSetting = new ArchiverVEOSetting();
                                    rule.ArchiverVEOSetting.AgencyId = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/AgencyID").InnerXml;
                                    rule.ArchiverVEOSetting.SeriesNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/Series_Number").InnerXml;
                                    rule.ArchiverVEOSetting.SeriesIdentifier = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/SeriesIdentifier").InnerXml;
                                    rule.ArchiverVEOSetting.ConsignmentNumber = doc.SelectSingleNode("Configuration/ArchiverJobVEOSetting/ConsignmentNumber").InnerXml;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn("set VEO export setting when run job error {0}", e.ToString());
                }
            }
            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA)
            {
                try
                {
                    var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NAA, sourceFlag);
                    if (nnaExportSetting != null)
                    {
                        rule.NAAConfigFile = nnaExportSetting.ExportConfig;
                    }
                    else
                    {
                        var filepath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "NAA Configuration File.zip");
                        var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Temp", "Config", "NAA Configuration File");
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                        if (sourceFlag == (int)SourceFlag.SharePoint)
                        {
                            rule.NAAConfigFile = GetMemoryStream(unZipFolder, "NAA Configuration File.xml");
                        }
                        else
                        {
                            rule.NAAConfigFile = GetMemoryStream(unZipFolder, "EXO NAA Configuration File.xml");
                        }

                    }
                }
                catch (Exception e)
                {
                    Logger.Warn("set NNA export setting when run job error {0}", e.ToString());
                }

            }
            //NARA
            if (rule.ExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA)
            {
                try
                {
                    var nnaExportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NARA, sourceFlag);
                    if (nnaExportSetting != null)
                    {
                        rule.NARAConfigFile = nnaExportSetting.ExportConfig;
                    }
                    else
                    {
                        var filepath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config", "NARA Configuration File.zip");
                        var unZipFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Temp", "Config", "NARA Configuration File");
                        GCommon.ZipUtil.UnZipFile(filepath, unZipFolder);
                        if (sourceFlag == (int)SourceFlag.SharePoint)
                        {
                            rule.NARAConfigFile = GetMemoryStream(unZipFolder, "NARA Configuration File.xml");
                        }
                        else
                        {
                            rule.NARAConfigFile = GetMemoryStream(unZipFolder, "EXO NARA Configuration File.xml");
                        }

                    }
                }
                catch (Exception e)
                {
                    Logger.Warn("set NARA export setting when run job error {0}", e.ToString());
                }

            }

            var exportEncryptionEnabled = IsExportDataEncryptionEnabled(RMKeyValueDao);
            if (exportEncryptionEnabled)
            {
                var keyIV = ExportDataEncryptionSettingService.GetCurrentAesKey().Extension;
                if (mConfiguration.IsUpgradedVEOV3)
                {
                    rule.ExportDataEncryptionKey = keyIV;
                }
                else if (!string.IsNullOrWhiteSpace(keyIV) && keyIV.IndexOf("|") > 0)
                {
                    rule.ExportDataEncryptionKey = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[0]));
                    rule.ExportDataEncryptionIV = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(keyIV.Split('|')[1]));
                }
                else
                {
                    throw new Exception("Export data encryption is enabled, but we cannot valid encryption key.");
                }
            }
        }

        private void HandleEndUserArchiveWithoutFiles(EndUserArchiveSiteCollectionConfig config)
        {
            bool reportedSkipped = ReportEndUserFileInfos(config.SkipFileInfoList, JobDetailsStatus.Skipped);
            bool reportedExceptions = ReportEndUserFileInfos(config.ExceptionFileInfoList, JobDetailsStatus.Exception);

            if (!reportedSkipped && !reportedExceptions)
            {
                Logger.Info("End user archive job had no skip or exception entries to report.");
            }
        }

        private bool ReportEndUserFileInfos(IEnumerable<EndUserFileInfo> fileInfos, JobDetailsStatus fallbackStatus)
        {
            if (mConfiguration?.JobReportDto == null || fileInfos == null)
            {
                return false;
            }

            bool wroteDetail = false;
            foreach (var fileInfo in fileInfos)
            {
                mConfiguration.JobReportDto.AddScanReport(fileInfo.GetDecodedFullPath(), 0, (int)CacheNodeType.Item, "N/A", fallbackStatus, fileInfo.ErrorMessage);
                wroteDetail = true;
            }

            return wroteDetail;
        }

        private void EnsureEndUserArchiveJobDetailsLogged()
        {
            try
            {
                List<EndUserFileInfo> allFiles = CollectEndUserFileInfos(mConfiguration.EndUserArchiveSiteCollectionConfig);

                if (allFiles.Count == 0)
                {
                    Logger.Error($"no file in end user job");
                    return;
                }

                BaseJobDto jobDto = new() { Id = JobId, JobType = (int)mJobType, NeedQueryFromUploadLocation = true };
                HashSet<string> existingPaths = LoadExistingEndUserDetailPaths(jobDto);
                Logger.Info($"{existingPaths.Count} existing end user job details found: {string.Join(", ", existingPaths)}.");

                var allCollectedFiles = allFiles.Select(f => new { Path = f.GetDecodedFullPath(), ErrorMessage = f.ErrorMessage }).ToList();
                Logger.Info($"{allCollectedFiles.Count} end user files collected: {string.Join(", ", allCollectedFiles.Select(f => $"{f.Path}|{f.ErrorMessage}"))}.");
                foreach (var fileInfo in allCollectedFiles)
                {
                    if (existingPaths.Contains(fileInfo.Path?.ToLower()))
                    {
                        continue;
                    }
                    mConfiguration.JobReportDto.AddScanReport(fileInfo.Path, 0, (int)CacheNodeType.Item, "N/A", JobDetailsStatus.Exception, fileInfo.ErrorMessage);
                    existingPaths.Add(fileInfo.Path);
                }
            }
            catch (Exception detailException)
            {
                Logger.Warn($"Failed to ensure end user archive job details are complete. {detailException}");
            }
        }

        private static List<EndUserFileInfo> CollectEndUserFileInfos(EndUserArchiveSiteCollectionConfig config)
        {
            List<EndUserFileInfo> files = new List<EndUserFileInfo>();
            AppendEndUserFiles(files, config?.FileInfoList);
            AppendEndUserFiles(files, config?.SkipFileInfoList);
            AppendEndUserFiles(files, config?.ExceptionFileInfoList);
            return files;
        }

        private static void AppendEndUserFiles(List<EndUserFileInfo> target, IEnumerable<EndUserFileInfo> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var fileInfo in source)
            {
                if (fileInfo != null)
                {
                    target.Add(fileInfo);
                }
            }
        }

        private HashSet<string> LoadExistingEndUserDetailPaths(BaseJobDto jobDto)
        {
            HashSet<string> existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var reportManager = mConfiguration?.JobReportDto?.ReportManager;
            if (reportManager != null)
            {
                var cachedDetails = reportManager.GetCacheJobDetails();
                var fileDetails = cachedDetails?.OfType<JMArchiverActionJobDetails>()?.Where(detail => detail.ActionTab == 0 && detail.Level == "RM_JS_Rule_ObjectLevel_Item");
                AppendDetailPaths(existingPaths, fileDetails);
            }

            AppendPersistedDetailPaths(existingPaths, jobDto);
            return existingPaths;
        }

        private static void AppendDetailPaths(HashSet<string> target, IEnumerable<JMArchiverActionJobDetails> details)
        {
            if (target == null || details == null)
            {
                return;
            }

            foreach (var detail in details)
            {
                string normalized = NormalizeSourceLocation(detail?.SourceLocation);
                if (!string.IsNullOrEmpty(normalized))
                {
                    target.Add(normalized.ToLower());
                }
            }
        }

        private void AppendPersistedDetailPaths(HashSet<string> target, BaseJobDto jobDto)
        {
            if (target == null || jobDetailService == null || jobDto == null)
            {
                return;
            }

            const int pageSize = 500;
            int currentPage = 1;
            while (true)
            {
                var pageResult = jobDetailService.GetData(pageSize, currentPage, " ActionTab=0 AND Level='RM_JS_Rule_ObjectLevel_Item' ", jobDto);
                var detailList = pageResult?.OfType<JMArchiverActionJobDetails>().ToList();

                AppendDetailPaths(target, detailList);

                if (detailList.Count < pageSize)
                {
                    break;
                }

                currentPage++;
            }
        }

        private static string NormalizeSourceLocation(string sourceLocation)
        {
            if (string.IsNullOrWhiteSpace(sourceLocation))
            {
                return string.Empty;
            }

            string trimmed = sourceLocation.Trim().TrimEnd('/');
            try
            {
                return Uri.UnescapeDataString(trimmed);
            }
            catch
            {
                return trimmed;
            }
        }

        private bool IsExportDataEncryptionEnabled(IRMKeyValueDao dao)
        {
            var result = false;
            var key = $"{KeyNameCollection.ExportDataEncryptionEnabled}{RMNameValueDto.Seprator}{RMNameValueType.ExportDataEncryption}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        private byte[] GetMemoryStream(string unZipFolder, string fileName)
        {
            using (FileStream fs = new FileStream(Path.Combine(unZipFolder, fileName), FileMode.Open))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    } 
}
