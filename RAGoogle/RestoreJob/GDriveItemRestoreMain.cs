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


using AvePoint.Archiver.Media;
using AvePoint.Cryptography;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Report;
using ItemDependencyOption = AvePoint.GCommon.Contract.Server.GranularRestore.Object.ItemDependencyOption;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Restore;
using RAArchiverCommon;
using RAGoogle.Restore.Content;
using RAGoogle.Restore.Service;
using AvePoint.RA.Common.Aos;
using RAGoogle.Report;
using AvePoint.RA.Common.Util;
using Media.Service.ArchiverBackup.Statistics;

namespace RAGoogle.Restore
{
    public class GDriveItemRestoreMain : AbstractDriveItemRestore
    {
        private JobContext jobContext;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        public JobReportImps mJobreport;

        public ReportCenter mReportCenter;

        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(GDriveItemRestoreMain));
        public GDriveItemRestoreMain() { }
        public GDriveItemRestoreMain(string jobId, JobType mJobType)
        {
            JobId = jobId;
            this.mJobType = mJobType;
            jobContext = JobContext.GetInstance(jobId, mJobType);
            jobContext.ReportManager.StartUpdateJobProgress();
            SOGDriveArchiverJobInfoStatistics.Instance.MainJobStartTime = jobContext.MainJobStartTime;
            WrapperConfiguration.TempDirectory = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, "Wrapper");
            ArchiverCommonStaticMethod.CreateDirectory(WrapperConfiguration.TempDirectory);
            InitWrapperConfigurationForBPOS();
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
#if DEBUG
            AvePerformanceMonitor.SetDisable(false);
#endif
            bool performanceMonitorDisabled = false;
            mJobreport = new JobReportImps(jobContext.ReportManager);
            mReportCenter = new ReportCenter();
            GDriveRestoreConfig config = null;
            bool isOutPlace = false;
            try
            {
                using (PerformanceScope pc = new PerformanceScope("GDriveItemRestoreMain.RunNowAsync"))
                using (new CheckJobStopScope())
                {
                    string jobId = JobId;
                    GDriveRestoreSettingAndTree mRestore = new GDriveRestoreSettingAndTree();
                    mRestore = SerializerHelper.DeserializeByDataContractSerializer<GDriveRestoreSettingAndTree>(jobContext.JobContextSetting);
                    byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
                    CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
                    ConfigureMediaEnviroment();

                    int category = 20;
                    TreeNodeConverter conver = new TreeNodeConverter();
                    MediaTCPRequest configForMedia = AssembleRestoreMessage(jobId, mRestore.Tree[0], mRestore);

                    config = await HandleGranularRestoreJobAsync(mRestore, configForMedia, performanceMonitorDisabled, (PlanCategory)category);
                }
            }
            catch (JobStopException ex)
            {
                mLog.Warn("job is stopped");
                mJobreport.HasStop = true;
                mReportCenter.JobHasStopped = true;
                throw;
            }
            catch (Exception e)
            {
                mReportCenter.HasErrorNode = true;
                mReportCenter.SummaryComments = e.Message;
                mLog.Error(@"Looks up a localized string similar to An error occurred while doing the restore job.{0}", e);
            }
            finally
            {
                //AveResourceUsageMonitor.JobId = args[0];
                //AveResourceUsageMonitor.TenantId = IdentityManager.IdentityContent;
                //AveResourceUsageMonitor.StopMonitor();
                SendRestoreJobTelemetryInfo();
                await RecordRestoredFile.UploadRestoredDBToStorageAsync();
                SOGDriveArchiverJobInfoStatistics.Instance.SaveInfoToGDriveDB();
                //if (mJobreport != null)
                //{
                //    if (!isOutPlace)
                //    {
                //        mJobreport.FinishRestoreReport();
                //    }
                //    else
                //    {
                //        if (mJobreport.HasStop)
                //        {
                //            mJobreport.FinishRestoreReport();
                //        }
                //    }
                //}

                // Use Google report for Google restore
                if (mReportCenter != null)
                {
                    mReportCenter.FinishGoogleDriveRestoreReport();                   
                }
                else
                {
                    //jobContext.ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                }

                //string dir = "C:\\";
                //string jobDir = string.Empty;
                //if (config?.JobDir != null && Directory.Exists(SecurityUtils.SafeCombinePath(config.JobDir)))
                //{
                //    jobDir = dir = SecurityUtils.SafeCombinePath(config.JobDir);
                //}
                //AvePerformanceMonitor.WriteToFile(SecurityUtils.SafeCombinePath(dir, "GranularRestorePerformance.xml"));
                //if (!string.IsNullOrEmpty(jobDir))
                //{
                //AveReportUploader.UploadReport(jobDir);
                //}
                //AveLogger.FinallyUploadWithoutOverwrite();

                await TelemetryContext.FlushAsync();
            }

        }

        private void InitWrapperConfigurationForBPOS()
        {
            try
            {
                if (RMKeyValueDao != null && RMKeyValueDao.GetValueByKey("SkipWebPartError") != null)
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.SkipWebPartError = Convert.ToBoolean(RMKeyValueDao.GetValueByKey("SkipWebPartError").Value);
                }
                mLog.Info($"InitWrapperConfigurationForBPOS.SkipWebPartError:{WrapperConfiguration.WrapperConfigurationForBPOS.SkipWebPartError}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitWrapperConfigurationForBPOS,error:{ex}");
            }
        }

        private void SendRestoreJobTelemetryInfo()
        {
            try
            {
                SOGDriveArchiverJobInfoStatistics statistics = SOGDriveArchiverJobInfoStatistics.Instance;
                object[] args = new object[9];
                args[0] = (JobId);
                args[1] = jobContext.MainJobStartTime;
                args[2] = (DateTime.UtcNow.Ticks);
                args[3] = (statistics.ItemSizeSumForTelemetry);
                args[4] = mReportCenter.GetJobStatus().ToString();
                args[5] = statistics.ItemCountForTelemetry;
                args[6] = statistics.ItemAndVersionCountFotTelemetry == 0 ? 0 : statistics.ItemAndVersionExpireSumTime / statistics.ItemAndVersionCountFotTelemetry;
                args[7] = statistics.FileCurrentVersionCount;
                args[8] = statistics.FileHisVersionCount;
                mLog.Info($"total restore size for telemetry is:{statistics.ItemSizeSumForTelemetry},start time {jobContext.MainJobStartTime},finish time {DateTime.UtcNow.Ticks}");
                TelemetryContext.SendToQueue(TelemetryModule.RestoreJob, TelemetryEventType.RunJob, args);
            }
            catch (Exception e)
            {
                mLog.Error($"send restore job telemetry failed,error:{e.ToString()}");
            }
        }

        public static bool HasSelectedNode(SPTreeNodeDto tree)
        {
            if (tree == null)
            {
                return false;
            }
            if (tree.CheckNumber == GConstants.TreeCheckNumber.CHECKED)
            {
                return true;
            }
            if (tree.Level == NodeLevel.Items && tree.SelectAll == SelectAllState.Checked)
            {
                return true;
            }
            foreach (SPTreeNodeDto node in tree.Children)
            {
                if (HasSelectedNode(node))
                {
                    return true;
                }
            }
            return false;
        }

        private static async System.Threading.Tasks.Task RestoreToStoragePolicy(GDriveRestoreConfig config, MediaTCPRequest configForMedia, IDisposable fileReceiver, PlanCategory category, IRMReportManager reportManager)
        {
            switch (category)
            {
                case PlanCategory.ArchiverRestore:
                    if (configForMedia is ArchiverRestoreRequest && ((ArchiverRestoreRequest)configForMedia).IsEndUserRequest)
                    {
                        ArchiverRestoreRequest request = configForMedia as ArchiverRestoreRequest;
                        //UpdateRequestMessage(request);
                        request.TenantGroupId = config.ArchiverConfigForMedia.TenantGroupId;
                        await
                        (fileReceiver as IArchiverRestoreToStorageService).HandleEndUserRestoreRequestAsync(request, reportManager);
                    }
                    else
                    {
                        var archiverResReq = configForMedia as ArchiverRestoreRequest;
                        archiverResReq.TenantGroupId = config.ArchiverConfigForMedia.TenantGroupId;
                        await(fileReceiver as IArchiverRestoreToStorageService).HandleRestoreRequestAsync(archiverResReq, reportManager);
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("Current job is not supported, Category: {0}", category));
            }
            //return requestMessage;
        }

        private static AveBPOSAccountInfo GetBPOSAccountInfos(IAveTreeNodeDto rootNode)
        {
            if (rootNode.Level == NodeLevel.SiteCollection)
            {
                string siteUrl = rootNode.FullPath;
                if (rootNode.NodeExtension != null)
                {
                    AveBPOSAccountInfo aveBPOSAccountInfo = null;
                    mLog.Info("SiteCollection NodeExtension BposInfo is null:{0},URL is:{1}.", rootNode.NodeExtension.BposInfo == null, siteUrl);
                    if (rootNode.NodeExtension.BposInfo != null && rootNode.NodeExtension.BposInfo.UserAccountInfo != null)
                    {
                        aveBPOSAccountInfo = rootNode.NodeExtension.BposInfo.ConvertToAveBPOSAccountInfo();
                        //Archiver SC App Profile方式restore，需要username做为SC Administrator
                        if (aveBPOSAccountInfo.ConnectionType == BposConnectionType.AppToken && rootNode.NodeExtension.BposInfo.UserAccountInfo.Username != null)
                        {
                            aveBPOSAccountInfo.UserName = rootNode.NodeExtension.BposInfo.UserAccountInfo.Username;
                        }
                    }
                    return aveBPOSAccountInfo;
                }
            }
            else
            {
                foreach (IAveTreeNodeDto child in rootNode.Children)
                {
                    var info = GetBPOSAccountInfos(child);
                    if (info != null)
                    {
                        return info;
                    }
                }
            }
            return null;
        }



        private static void AddTreeUserInfoForAPPProfileBackupServiceAccountRestore(IAveTreeNodeDto rootNode, ArchiverRestoreRequest configForMedia)
        {
            if (rootNode.Level == NodeLevel.SiteCollection)
            {
                string siteUrl = rootNode.FullPath;
                if (rootNode.NodeExtension != null && rootNode.NodeExtension.BposInfo != null
                    && rootNode.NodeExtension.BposInfo.ConnectionType == AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount
                    && rootNode.NodeExtension.BposInfo.UserAccountInfo != null
                    && (string.IsNullOrEmpty(rootNode.NodeExtension.BposInfo.UserAccountInfo.Username)))
                {
                    mLog.Info($"AddTreeUserInfoForAPPProfileBackupServiceAccountRestore.UserAccountInfo.Username:{rootNode.NodeExtension.BposInfo.UserAccountInfo.Username}.configForMedia.UserName:{configForMedia.UserName}.");
                    rootNode.NodeExtension.BposInfo.UserAccountInfo.Username = configForMedia.UserName;
                }
            }
            else
            {
                foreach (IAveTreeNodeDto child in rootNode.Children)
                {
                    AddTreeUserInfoForAPPProfileBackupServiceAccountRestore(child, configForMedia);
                }
            }
        }

        private static void ConfigureMediaEnviroment()
        {
            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();
            //MediaEnvironment.MediaServer.MediaServerVersion = AveEnv.AgentVersion.ToString();

            MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();

            //MediaConfigInfo.GranularConfigInfo = container.Resolve<GranularConfigInfo>("AvePoint.Media.Service.DomainModel.GranularConfigInfo");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">Media service Castle</param>
        /// <param name="requestMessage"></param>
        /// <param name="configForMedia">Only for open media, Archiver: ArchiverRestoreRequest, Granular : GranularRestoreRequest</param>
        /// <param name="performanceMonitorDisabled"></param>
        /// <param name="category">4: Grarnular Restore, 20: Archiver Restore</param>
        /// <returns></returns>
        private async Task<GDriveRestoreConfig> HandleGranularRestoreJobAsync(GDriveRestoreSettingAndTree mRestore, MediaTCPRequest configForMedia, bool performanceMonitorDisabled, PlanCategory category)
        {
            GDriveRestoreConfig config = new GDriveRestoreConfig();

            config.appProfile = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, mRestore.SiteGroupId, true);
            config.SubJobId = configForMedia.JobId;
            config.DestinationInfo = new DestinationInfo();
            config.DestinationInfo.OwerLogin = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            config.RestoreGlobalOption = new GlobalRestoreOption();
            config.ArchiverConfigForMedia = configForMedia as GDriveRestoreRequest;
            if (mRestore.Setting.RestoreOption == RestoreOption.OverWrite)
            {
                config.ContainerConflictResolution = ConflictResolutionType.Merge;
                config.ContentConflictResolution = ConflictResolutionType.Overwrite;
            }
            else if (mRestore.Setting.RestoreOption == RestoreOption.NotOverWrite)
            {
                config.ContainerConflictResolution = ConflictResolutionType.Skip;
                config.ContentConflictResolution = ConflictResolutionType.Skip;
            }
            else
            {//append
                config.ContainerConflictResolution = ConflictResolutionType.Skip;
                config.ContentConflictResolution = ConflictResolutionType.AppendItemOrDocumentByReNamed;
            }
            WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode = HasRestoreItemLevelNode(mRestore);
            mLog.Info($"IsEndUserRestore: {WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore}, HasItemLevelNode: {WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode}");
            config.RestoreVersionSetting = RestoreVersionSetting.All;
            config.RestoreGlobalOption = new GlobalRestoreOption();
            config.IsIncludeSharedLinks = mRestore.Setting.IncludeSharingLink;
            WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks = config.IsIncludeSharedLinks;
            config.SetRestoreOption(config.ContainerConflictResolution, config.ContentConflictResolution);
            //ItemVersionFilter.SetConfigAttr(config.RestoreVersionSetting, 0);
            IFileReceiver fileReceiver = null;
            ProductVersion productVersion = ProductVersion.ProductUnknown;
            var report = mJobreport.ReportManager;
            mLog.Info(@"Looks up a localized string similar to The current job is an {0} restore. .", (PlanCategory)category);
            if (!WrapperRuntime.IsInitialized)
            {
                WrapperRuntime.SetGlobalRuntimeSetting(true, new AveWrapperRunningAccountInfo());//设置WrapperRuntime.CurrentContext为多线程共用
            }
            //初始化report
            IAveTreeNodeDto root = mRestore.Tree[0];
            config.EventCategory = 523;
            config.InitDefaultValueFromConfigFile();
            config.ItemDependencyType = ItemDependencyOption.Overwrite;

            bool readDataViaCache = IsReadDataViaCache();
            MediaConfigInfo.CommonConfigInfo.ReadMetaDataViaCache = readDataViaCache;
            MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache = readDataViaCache;
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView = true;
            WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore = false;
            //if(!string.IsNullOrEmpty(mRestore.Setting.AppProfileId) && !string.IsNullOrEmpty(mRestore.Setting.SiteAdminUrl))
            //{
            //    await BPOSSiteCollectionConfig.InitAsync(root, mRestore.Setting.AppProfileId, mRestore.Setting.SiteAdminUrl);
            //}
            //else
            //{
            //    await BPOSSiteCollectionConfig.InitAsync(root);
            //}
            GDriveArchiverRestoreService restoreService = null;
            try
            {
                restoreService = OpenMediaRestoreService((PlanCategory)category, configForMedia, report.SetTotal, ref fileReceiver);

                AvePerformanceMonitor.SetDisable(performanceMonitorDisabled);
                using (var restore = new GDriveItemRestore())
                {
                    try
                    {
                        restore.SetNowAsRestoreFileModifyTime = IsSetNowAsRestoreFileModifyTime();
                        restore.Config = config;
                        restore.Report = mJobreport;
                        restore.ReportCenter = mReportCenter;
                        restore.FileReceiver = fileReceiver;
                        restore.IsEnduserRestore = mRestore.IsEndUserJob;
                        restore.PossiblyStubType = mRestore.Setting?.StubType;
                        restore.OopStubUrl = mRestore.oopStubUrl;
                        await restore.Process();
                    }
                    catch (JobStopException ex)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(ex.ToString());
                        restore.Error = ex;
                    }
                    if (restoreService is GDriveArchiverRestoreService)
                    {
                        if ((restoreService as GDriveArchiverRestoreService).HasBlobInArchiverTier)
                        {
                            //mJobreport.AddJobSummaryComment("ArchiverRehydrationAzureBlobComments");
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error(e.ToString());
                //AOSBR-1474 如果index被破坏，job会hang在1%，不会正常failed.需要解开以下注释。
                //RestoreResultInfo resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                //resultInfo.AddARestoreError(e.Message, null, new string[] { });
                mJobreport.HasErrorNode = true;
                mReportCenter.HasErrorNode = true;
                mReportCenter.SummaryComments = e.Message;
                //report.Finish(resultInfo, e.Message);
            }
            finally
            {
                if (restoreService != null)
                {
                    try
                    {
                        restoreService.Dispose();
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"RestoreService Dispose error {e}.");
                    }
                }
            }
            return config;
        }

        private bool IsSetNowAsRestoreFileModifyTime()
        {
            var key = RMKeyValueDao.GetValueByKey("IsSetNowAsRestoreFileModifyTime");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool IsReadDataViaCache()
        {
            var key = RMKeyValueDao.GetValueByKey("ReadDataViaCache");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private bool HasRestoreItemLevelNode(GDriveRestoreSettingAndTree mRestore)
        {
            return HasItemLevelNode(new SPTreeNodeDto()
            {
                Level = NodeLevel.Undefined,
                //Children = mRestore.Tree
            });
        }
        private static NodeLevel[] _ItemLevels = new NodeLevel[] {
            NodeLevel.Item,
            NodeLevel.ItemVersion,
            NodeLevel.Document,
        };
        private bool HasItemLevelNode(SPTreeNodeDto sPTreeNodeDto)
        {
            var hasItemLevel = sPTreeNodeDto != null ? _ItemLevels.Contains(sPTreeNodeDto.Level) : false;
            if (sPTreeNodeDto?.Children != null)
            {
                foreach (var childNode in sPTreeNodeDto.Children)
                {
                    if (HasItemLevelNode(childNode))
                    {
                        return true;
                    }
                }
            }

            return hasItemLevel;
        }

        private static GDriveArchiverRestoreService OpenMediaRestoreService(PlanCategory categroy, MediaTCPRequest restoreRequest, Action<long> updateProgress, ref IFileReceiver fileReceiver)
        {
            GDriveArchiverRestoreService restoreService = null;
            switch (categroy)
            {
                case PlanCategory.ArchiverRestore: //20: Archiver Restore
                    var archiverDataBlockManager = new ArchiverRestoreDataBlockManger();
                    fileReceiver = new ArchiverMemoryFileReceiver(archiverDataBlockManager);

                    restoreService = new GDriveArchiverRestoreService();
                    restoreService.HandleRequest(restoreRequest, archiverDataBlockManager, updateProgress);

                    break;
                default:
                    throw new NotImplementedException();
            }

            return restoreService;
        }
    }
}

