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




using Aspose.Email.PersonalInfo;
using Aspose.Page.XPS.XpsMetadata;
using Aspose.Pdf.Operators;
using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.Common.Portal;
using AvePoint.Cryptography;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CloudServiceCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.Item.Restore.ScanStaging;
using AvePoint.Media.Common;
using AvePoint.Media.Core.IO;
using AvePoint.Media.Core.IO.Input;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Metadata;
using AvePoint.ObjectModel.Common;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.RestoreJob.Restore;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.Wrapper.Common;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Cloud.Sdk.EDiscovery.Services;
using ExchangeUtility.Graph;
using M365.Wrapper.Backup.Auth.Common;
using Media.Service.ArchiverBackup.Restore;
using Newtonsoft.Json;
using PnP.Framework.Diagnostics;
using PnP.Framework.Modernization.Extensions;
using RAArchiverCommon;
using RAArchiverCommon.DisposalProgress;
using RAArchiverCommon.DisposalProgress.Impl;
using RAExportCommon;
using RAGoogle.Restore.Content;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using BackupLevel = AvePoint.GCommon.Contract.GranularBackup.Object.BackupLevel;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using ItemDependencyOption = AvePoint.GCommon.Contract.Server.GranularRestore.Object.ItemDependencyOption;
using RestoreType = AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType;

namespace AvePoint.Item.Restore
{
    public class AveItemRestoreMain : AbstractAveItemRestore
    {
        private JobContext jobContext;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        public IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        private IArchiverSiteMasterIndexService ArchiverIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private readonly IDownloadDataInfoDao _downloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private string _parentJobId = string.Empty;
        private const int OpusStubScanPageSize = 2000;
        private const int OpusStubRestoreBatchSize = 500;
        private string OpusStubRestoreSiteId = string.Empty;

        public static BposSiteCollectionsConfig BPOSSiteCollectionConfig
        {
            get
            {
                return Singleton<BposSiteCollectionsConfig>.SingletonInstance;
            }
        }
        public IAdvancedConditionsHandler _AdvancedConditionsHandler { get; set; }
        public IAdvancedConditionsHandler AdvancedConditionsHandler
        {
            get
            {
                if (_AdvancedConditionsHandler == null)
                {
                    _AdvancedConditionsHandler = new AdvancedConditionsHandler();
                    return _AdvancedConditionsHandler;
                }
                else
                {
                    return _AdvancedConditionsHandler;
                }
            }
            set { }
        }
        private IJobDetailService mJobDetailService;
        private IJobDetailService JobDetailService
        {
            get
            {
                if (mJobDetailService == null)
                {
                    mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
                }
                return mJobDetailService;
            }
        }
        public JobReportImps mJobreport;
        private IMCacheSettingService _CacheSettingService { get; set; }
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_CacheSettingService == null)
                {
                    _CacheSettingService = new CacheSettingService();
                    return _CacheSettingService;
                }
                else
                {
                    return _CacheSettingService;
                }
            }
            set { }
        }
        public ITreeNodeConverter TreeNodeConverter { get { return new TreeNodeConverter(); } set { } }
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(AveItemRestoreMain));
        public AveItemRestoreMain() { }
        public AveItemRestoreMain(string jobId, JobType mJobType)
        {
            JobId = jobId;
            this.mJobType = mJobType;
            jobContext = JobContext.GetInstance(jobId, mJobType);
            jobContext.ReportManager.StartUpdateJobProgress();

            CompoundDisposalStatistics.Instance.Init(new DisposalStaticInitObject()
            {
                JobType = mJobType,
                MainJobId = jobContext.MainJobId,
                SubJobId = jobContext.SubJobId
            });
            CompoundDisposalStatistics.Instance.StartStatistic();

            //mConfiguration = new ScheduleConfiguration(JobId);
            SOArchiverJobInfoStatistics.Instance.MainJobStartTime = jobContext.MainJobStartTime;
            WrapperConfiguration.TempDirectory = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, "Wrapper");
            ArchiverCommonStaticMethod.CreateDirectory(WrapperConfiguration.TempDirectory);
            InitWrapperConfigurationForBPOS();
            InitSkipCacheLookColumnConfigurationForBPOS();
            InitSkipRoleNameConfigurationForBPOS();
            InitSkipRoleIdConfigurationForBPOS();
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            //AveResourceUsageMonitor.StartMonitor(GUIModuleType.Item);
#if DEBUG
            AvePerformanceMonitor.SetDisable(false);
#endif
            bool performanceMonitorDisabled = false;
            //Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["DisablePerformanceMonitor"], out performanceMonitorDisabled);
            mJobreport = new JobReportImps(jobContext.ReportManager);
            ItemRestoreConfig config = null;
            RestoreSettingAndTree mRestore = new RestoreSettingAndTree();
            bool isOutPlace = false;
            RMDownloadDataInfo downloadDataInfo = new RMDownloadDataInfo();

            try
            {
                using (new CheckJobStopScope())
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    string jobId = JobId;
                    if (RestoreSearchService.ShouldQueryInJobForEndUserRestore(jobContext.JobContextSetting))
                    {
                        EndUserRestoreJobConfig endUserRestoreJobConfig = SerializerHelper.DeserializeByDataContractSerializer<EndUserRestoreJobConfig>(jobContext.JobContextSetting);
                        mLog.Info("Run end user restore with in-job index query");
                        mRestore = await RestoreSearchService.BuildRestoreSettingAndTreeForEndUserJobAsync(endUserRestoreJobConfig);
                    }
                    else
                    {
                        mRestore = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(jobContext.JobContextSetting);
                        if (mRestore.Setting.RestoreExecutionRequest != null)
                        {
                            if (mJobType == JobType.StubArchiverRestore || mJobType == JobType.M365InPlaceArchiverRestore)
                            {
                                await ExecuteStagedOpusRestoreAsync(mRestore, performanceMonitorDisabled, downloadDataInfo);
                                return;
                            }
                            else
                            {
                                var restoreNode = await GetRestoreInfoNodeObjects(mRestore.Setting.RestoreExecutionRequest);
                                if (restoreNode == null)
                                {
                                    throw new AveException($"Unable to resolve restore node for scope:{mRestore.Setting.RestoreExecutionRequest.Scope}");
                                }

                                mRestore.Setting.NodeObjects = new List<ArchiverRestoreSerchResult> { restoreNode };
                                mRestore.Tree = BuildRestoreTreeFromNodeObjects(mRestore.Setting.NodeObjects);
                                mLog.Info("Run Public API restore with in-job index query");
                            }
                        }
                    }
                    mLog.Info($"Support locked sites for SPO: {mRestore.Setting.IsSupportLockedSite}");

                    if (!string.IsNullOrEmpty(mRestore.Setting.FailedJobId))
                    {
                        mRestore.Tree = GenerateFailedRestoreTree(mRestore.Setting.FailedJobId, ConvertToJobType(mRestore.Setting.RestoreTypeSelect), mRestore.Setting.SiteUrl);
                    }
                    Stopwatch sw2 = new Stopwatch();
                    sw2.Start();
                    byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
                    CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
                    ConfigureMediaEnviroment();
                    sw2.Stop();
                    mLog.Info($"linkRestoreReport init restore job GetCommunicationEncryptionKey cost time:{sw2.ElapsedMilliseconds}");
                    int category = 20;
                    TreeNodeConverter conver = new TreeNodeConverter();
                    MediaTCPRequest configForMedia = AssembleRestoreMessage(jobId, mRestore.Tree[0], mRestore);
                    sw.Stop();
                    mLog.Info($"linkRestoreReport init restore job cost time:{sw.ElapsedMilliseconds}");
                    //JobQueueMessage requestMessage = GetRequestMessage(new string[1] { jobId }) as JobQueueMessage;
                    //SeperateLogToTenant(requestMessage, jobId);
                    //GRMessage message = GetGRMessage(container, requestMessage, ref category, ref configForMedia);
                    //IMAcceptMediaData ControlStorageService = container.Resolve<IMAcceptMediaData>("AvePoint.GCommon.Contract.Media.IMAcceptMediaData");
                    //ControlStorageService.InitiateMediaDataService(IdentityManager.IdentityContent);
                    if (mRestore != null
                        && mRestore.Setting.RestoreTypeSelect == RestoreType.OutOfPlace)
                    {
                        _parentJobId = SubJobDao.GetParentJobBySubJobId(jobId);
                        downloadDataInfo = _downloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.InProgress]).FirstOrDefault(item => item.JobId == _parentJobId);
                        UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);
                        isOutPlace = true;
                        config = await HandleRestoreToStorageJobAsync(mRestore, configForMedia, performanceMonitorDisabled, (PlanCategory)category, jobContext.ReportManager);
                        UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);

                    }
                    else
                    {
                        config = await HandleGranularRestoreJobAsync(mRestore, configForMedia, performanceMonitorDisabled, (PlanCategory)category);
                    }
                    /*if (config.JobType == 28 || config.JobType == 60)
                    {
                        WrapperConfiguration.EndAddTooManyRequestError();
                        //AveReportUploader.Upload429Report(Path.Combine(AveEnv.AgentJobFolder, message.ArchiverConfigForMedia.JobId), message.TenantGroupOwner);
                    }*/
                }
            }
            catch (JobStopException ex)
            {
                mLog.Warn("job is stopped");
                mJobreport.HasStop = true;
                throw;
            }
            catch (AveSkipLockSiteException ex)
            {
                mLog.Error($"Looks up a localized string similar to An error occurred while doing the restore job. {ex}");
                mJobreport.AddRestoreReport(mRestore.Setting.SPOLibOrFolderPath, 0, (int)RestoreStatus.Failed, "E", 0, mRestore.Setting.SPOLibOrFolderPath, ex.Message);
            }
            catch (Exception e)
            {
                mLog.Error(@"Looks up a localized string similar to An error occurred while doing the restore job.{0}", e);
                if (isOutPlace) UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
                if (e.Message.Equals("RM_JS_Rule_SPDestUrlError"))
                {
                    mJobreport.AddRestoreReport(mRestore.Setting.SPOLibOrFolderPath, 0, (int)RestoreStatus.Failed, "E", 0, mRestore.Setting.SPOLibOrFolderPath, "RM_JS_Rule_SPDestUrlError");
                }
                else if (e.Message.Equals("RM_JS_Rule_SPNoStubFound"))
                {
                    mJobreport.summaryComments = "RM_JS_Rule_SPNoStubFound";
                    mJobreport.HasCompleteNode = true;
                    mJobreport.HasErrorNode = true;
                }
            }
            finally
            {
                //AveResourceUsageMonitor.JobId = args[0];
                //AveResourceUsageMonitor.TenantId = IdentityManager.IdentityContent;
                //AveResourceUsageMonitor.StopMonitor();
                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                if (mJobType != JobType.TeamsArchiverRestore && mJobType != JobType.TeamsOutPlaceRestore)
                {
                    CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                }

                SendRestoreJobTelemetryInfo();
                await RecordRestoredFile.UploadRestoredDBToStorageAsync();
                SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();

                if (mJobType != JobType.TeamsArchiverRestore && mJobType != JobType.TeamsOutPlaceRestore)
                {
                    CompoundDisposalStatistics.Instance.WaitEndStatistic();
                }
                if (mJobreport != null)
                {
                    if (!isOutPlace)
                    {
                        mJobreport.FinishRestoreReport();
                    }
                    else
                    {
                        if (mJobreport.HasStop)
                        {
                            mJobreport.FinishRestoreReport();
                        }
                    }
                }
                else
                {
                    //jobContext.ReportManager.SetJobFinished(Contract.RMWeb.JobMonitor.JobStatus.Finished);
                }
                try
                {
                    var theJobStatus = mJobreport.GetJobStatus();
                    //if (theJobStatus == RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)
                    //{
                    mLog.Warn($"this restore job is {theJobStatus},jobid:{jobContext.MainJobId},will add the context to job monitor for rerun");
                    var siteUrl = mRestore.Tree != null && mRestore.Tree.Count>0? mRestore.Tree[0].SitePath:"";
                    mRestore.Tree = null;
                    if (mRestore.Setting != null)
                    {
                        mRestore.Setting.NodeObjects = null;
                        mRestore.Setting.SerchContract = null;
                        mRestore.Setting.SiteUrl = siteUrl;
                        mRestore.IsSearchAllRestore = false;
                        mRestore.BackUpJobId = "";
                        if (mRestore.Setting.RestoreOption == RestoreOption.NotOverWrite)
                        {
                            mRestore.Setting.RestoreOption = RestoreOption.Append;
                        }
                        mRestore.Setting.RestoreVersionOption = RestoreDocumentVersionsOption.None;
                    }
                    JobMonitorService.UpdateJobExtensionById(jobContext.MainJobId, SerializerHelper.SerializeByJsonSerializer(mRestore));
                }
                catch (Exception e)
                {
                    mLog.Error($"update restore job extension failed,error:{e}");
                }
                //}
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
                sw1.Stop();
                mLog.Info($"linkRestoreReport finish restore cost time:{sw1.ElapsedMilliseconds}");

                await TelemetryContext.FlushAsync();
            }

        }
        private JobType ConvertToJobType(RestoreType restoreType)
        {
            switch (restoreType)
            {
                case RestoreType.InPlace:
                    return JobType.ArchiverRestore;
                case RestoreType.OutOfPlace:
                    return JobType.ArchiverOutPlaceRestore;
                case RestoreType.ToSPOLocation:
                    return JobType.ArchiverToSpoRestore;
                default:
                    throw new Exception($"this type not support rerun failed restore ,type:{restoreType.ToString()}");
            }
        }
        private List<SPTreeNodeDto> GenerateFailedRestoreTree(string failedJobId, JobType jobType, string siteUrl)
        {
            string jobRptPath = string.Empty;
            try
            {
                var jobDto = new BaseJobDto()
                {
                    Id = failedJobId,
                    JobType = (int)jobType,
                    AddValues = new Dictionary<string, object>(),
                    IsMergeRpt = true
                };
                jobRptPath = JobDetailService.DownloadReports(jobDto);
                return RealGenerateRestoreTree(jobDto, siteUrl);
            }
            catch (Exception ex)
            {
                mLog.Error(@$"Fail GenerateFailedRestoreTree,failed jobId:{failedJobId},ex:{ex}");
                return null;
            }
            finally
            {
                SafeDeleteFile(jobRptPath);
            }
        }
        private List<SPTreeNodeDto> RealGenerateRestoreTree(BaseJobDto jobDto, string siteUrl)
        {
            mLog.Info($@"Start generat restore tree, jobId:{jobDto.Id}");
            List<SPTreeNodeDto> result = new List<SPTreeNodeDto>();
            int pageIndex = 1;
            int pageSize = 1000;
            int totalCount = 0;
            var restoreDetails = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            do
            {
                IEnumerable<JMRestoreActionJobDetailes> jMJobDetails = JobDetailService.GetData(pageSize, pageIndex, ref totalCount, null, jobDto)?.Cast<JMRestoreActionJobDetailes>();
                if (jMJobDetails == null)
                {
                    mLog.Error($@"Unable read job detail from rpt, jobId:{jobDto.Id}");
                    throw new Exception(@$"Unable read job details from rpt, jobId:{jobDto.Id}");
                }
                var pageDetails = jMJobDetails.ToList();
                mLog.Info($"Restore tree paging read pageIndex:{pageIndex}, pageSize:{pageSize}, fetched:{pageDetails.Count}, total:{totalCount}, jobId:{jobDto.Id}");
                foreach (var detail in pageDetails)
                {
                    if (detail.Status == JobDetailsStatus.Failed || detail.Status == JobDetailsStatus.Exception || detail.Status == JobDetailsStatus.ContainerFailed)
                    {
                        var levelKey = string.IsNullOrWhiteSpace(detail.PolicyLevel) ? "Unknown" : detail.PolicyLevel.Trim();
                        if (!restoreDetails.TryGetValue(levelKey, out var list))
                        {
                            list = new List<string>();
                            restoreDetails[levelKey] = list;
                        }
                        list.Add(detail.PathMd5);
                    }
                }
            } while (totalCount > pageIndex++ * pageSize);
            ArchiverSiteMasterIndexContract siteIndex = GetSiteCollctionIndex(new SiteCollectionNodesInfo() { SiteUrl = siteUrl });
            if (null == siteIndex)
            {
                mLog.Warn("the siteIndex is null");
                return null;
            }
            List<ArchiverSiteMasterIndexContract> indexes = new List<ArchiverSiteMasterIndexContract> { siteIndex };
            List<SiteCollectionNodesInfo> searchNodes = new List<SiteCollectionNodesInfo>() { GenerateSiteCollectionInfo(siteIndex) };
            List<TreeNode> searchResult = new List<TreeNode>();
            if (restoreDetails.Count > 0)
            {
                //gererate restore tree
                foreach (var detDic in restoreDetails)
                {
                    var level = ConverTypeToLevel(detDic.Key);
                    var tempNode = GetSearchNodesFromMedia(indexes, searchNodes, GenerateFilter(level, detDic.Value), 3000,new ArchiverRestoreOrderBy());
                    searchResult.AddRange(tempNode);
                }
            }
            result = InternalGenerateTreeNode(searchResult);
            mLog.Info(@$"Finish generate restore tree, jobId:{jobDto.Id},result count is :{result}");
            return result;
        }
        private List<SPTreeNodeDto> InternalGenerateTreeNode(List<TreeNode> treeNodes)
        {
            var result = new List<SPTreeNodeDto>();
            List<TreeNode> temp = new List<TreeNode>();
            //PreprocessingSelectedNodes(treeNodes);
            foreach (var tr in treeNodes)
            {
                TreeNode tree = tr;
                var treeClone = Clone(tree);
                tree.Depth = CaculateDepth(treeClone);
                SetIsSelectTreeNode(tree);
                temp.Add(tree);
            }
            TreeNode treeLevel = treeNodes.FirstOrDefault();
            //SPTreeNodeDto
            List<TreeNode> resultChildren = AdvancedConditionsHandler.AssembleTreeByAdvancedConditions(BubbleSort(temp, treeLevel.TreeNodeLevel), "(1)");
            var tempResult = TreeNodeConverter.ConvertTreeNodeListToSPTreeNodeList(resultChildren, ConverNodeLevel(treeLevel.TreeNodeLevel));
            result = ExtractResult(tempResult);
            return result;
        }

        private List<SPTreeNodeDto> BuildRestoreTreeFromNodeObjects(List<ArchiverRestoreSerchResult> nodeObjects)
        {
            if (nodeObjects == null || nodeObjects.Count == 0)
            {
                return new List<SPTreeNodeDto>();
            }

            List<TreeNode> treeNodes = nodeObjects
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.TreeNode))
                .Select(node => SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(node.TreeNode))
                .Where(node => node != null)
                .ToList();

            if (treeNodes.Count == 0)
            {
                return new List<SPTreeNodeDto>();
            }

            return InternalGenerateTreeNode(treeNodes);
        }

        private List<SPTreeNodeDto> BuildRestoreTreeFromScanBatch(OpusScanBatch batch)
        {
            if (batch?.Items == null || batch.Items.Count == 0)
            {
                return new List<SPTreeNodeDto>();
            }

            Dictionary<string, TreeNode> nodeByUrl = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            TreeNode root = CreateScanTreeRoot(batch, nodeByUrl);
            AttachScanContainers(batch.Containers, nodeByUrl, root);
            AttachScanItems(batch.Items, nodeByUrl, root);
            PruneEmptyBranches(root);
            return InternalGenerateTreeNode(new List<TreeNode> { root });
        }

        private static TreeNode CreateScanTreeRoot(OpusScanBatch batch, IDictionary<string, TreeNode> nodeByUrl)
        {
            OpusScanContainer site = batch.Containers.FirstOrDefault(container => container.ContainerType == OpusScanContainerType.SiteCollection);
            string siteUrl = site?.ContainerUrl ?? batch.Items[0].SiteUrl;
            TreeNode root = site == null
                ? new TreeNode
                {
                    Name = siteUrl,
                    DisplayName = siteUrl,
                    Title = siteUrl,
                    FullPath = siteUrl,
                    FullPathForUI = siteUrl,
                    TreeNodeLevel = TreeNodeLevel.SiteCollection,
                    SitePath = siteUrl,
                    Expanded = true,
                    ChildrenLoaded = true,
                    CanChildrenBeLoaded = false,
                }
                : CreateScanTreeNode(site);
            nodeByUrl[siteUrl] = root;
            return root;
        }

        private static void AttachScanContainers(
            IReadOnlyList<OpusScanContainer> containers,
            IDictionary<string, TreeNode> nodeByUrl,
            TreeNode root)
        {
            bool attached;
            do
            {
                attached = false;
                foreach (OpusScanContainer container in containers)
                {
                    if (nodeByUrl.ContainsKey(container.ContainerUrl))
                    {
                        continue;
                    }

                    if (!nodeByUrl.TryGetValue(container.ParentUrl ?? string.Empty, out TreeNode parent))
                    {
                        continue;
                    }

                    TreeNode node = CreateScanTreeNode(container);
                    parent.Children.Add(node);
                    nodeByUrl[container.ContainerUrl] = node;
                    attached = true;
                }
            }
            while (attached);

            foreach (OpusScanContainer container in containers)
            {
                if (nodeByUrl.ContainsKey(container.ContainerUrl))
                {
                    continue;
                }

                TreeNode node = CreateScanTreeNode(container);
                root.Children.Add(node);
                nodeByUrl[container.ContainerUrl] = node;
            }
        }

        private static void AttachScanItems(
            IReadOnlyList<OpusScanItem> items,
            IDictionary<string, TreeNode> nodeByUrl,
            TreeNode root)
        {
            foreach (OpusScanItem item in items)
            {
                TreeNode parent = nodeByUrl.TryGetValue(item.ParentUrl, out TreeNode folder)
                    ? folder
                    : nodeByUrl.TryGetValue(item.ListUrl, out TreeNode list) ? list : root;
                parent.Children.Add(BuildStubRestoreCandidate(item));
            }
        }

        private static TreeNode CreateScanTreeNode(OpusScanContainer container)
        {
            TreeNodeLevel level = container.ContainerType switch
            {
                OpusScanContainerType.SiteCollection => TreeNodeLevel.SiteCollection,
                OpusScanContainerType.Site => TreeNodeLevel.Site,
                OpusScanContainerType.List => TreeNodeLevel.List,
                _ => TreeNodeLevel.Folder,
            };
            return new TreeNode
            {
                Name = container.Name,
                DisplayName = container.DisplayName,
                Title = container.DisplayName,
                FullPath = container.ContainerUrl,
                FullPathForUI = container.FullPathForUI,
                TreeNodeLevel = level,
                SitePath = container.SiteUrl,
                Expanded = true,
                ChildrenLoaded = true,
                CanChildrenBeLoaded = false,
            };
        }

        private static TreeNode BuildStubRestoreCandidate(OpusScanItem item)
        {
            string fileName = item.FileName ?? string.Empty;
            string displayName = Path.ChangeExtension(fileName, null);
            Guid uniqueId = Guid.TryParse(item.UniqueId, out Guid parsedUniqueId) ? parsedUniqueId : Guid.Empty;
            return new TreeNode
            {
                Name = displayName,
                DisplayName = displayName,
                Title = displayName,
                FullPath = item.FileUrl,
                FullPathForUI = GetWebRelativeUrl(item.WebUrl, item.FileUrl),
                TreeNodeLevel = TreeNodeLevel.Item,
                Type = TreeNodeType.Document,
                SitePath = item.SiteUrl,
                ID = uniqueId.ToString(),
                Id = uniqueId.ToString(),
                NodeGuid = uniqueId.ToString(),
                Expanded = true,
                ChildrenLoaded = true,
                CanChildrenBeLoaded = false,
                Extension = item.Extension,
                Size = item.Size,
            };
        }
        private NodeLevel ConverNodeLevel(TreeNodeLevel tLevel)
        {
            NodeLevel nodeLevel = NodeLevel.Item;
            if (tLevel == TreeNodeLevel.Folder)
                nodeLevel = NodeLevel.Folder;
            if (tLevel == TreeNodeLevel.Site)
                nodeLevel = NodeLevel.Site;
            if (tLevel == TreeNodeLevel.List)
                nodeLevel = NodeLevel.List;
            return nodeLevel;
        }
        private List<TreeNode> BubbleSort(List<TreeNode> unsorted, TreeNodeLevel nodeLevel)
        {
            int n = unsorted.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (unsorted[j].Depth > unsorted[j + 1].Depth)
                    {
                        // swap arr[j] and arr[j+1]
                        TreeNode temp = unsorted[j];
                        unsorted[j] = unsorted[j + 1];
                        unsorted[j + 1] = temp;
                    }
                }
            }
            if (nodeLevel == TreeNodeLevel.Folder || nodeLevel == TreeNodeLevel.List || nodeLevel == TreeNodeLevel.Site)
            {
                mLog.Info($"restore level is {nodeLevel}");
                return unsorted;
            }
            return SortItems(unsorted);
        }
        private void SetIsSelectTreeNode(TreeNode treeNode)
        {
            TreeNode temp = treeNode;
            while (temp.Children.Count > 0)
            {
                temp = temp.Children[0];
            }
            temp.IsSelectNode = true;
            return;
        }
        private List<TreeNode> SortItems(List<TreeNode> items)
        {
            items.Sort((x, y) =>
            {
                TreeNode tempX = Clone(x);
                TreeNode tempY = Clone(y);
                while (true)
                {
                    if (tempX.Children != null && tempX.Children.Count > 0)
                    {
                        tempX = tempX.Children[0];
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    if (tempY.Children != null && tempY.Children.Count > 0)
                    {
                        tempY = tempY.Children[0];
                    }
                    else
                    {
                        break;
                    }
                }
                string tempNameX = tempX.Name;
                string tempNameY = tempY.Name;
                if (tempX.Name.Contains(":"))
                {
                    tempNameX = tempNameX.Substring(0, tempNameX.IndexOf(":"));
                }
                if (tempY.Name.Contains(":"))
                {
                    tempNameY = tempNameY.Substring(0, tempNameY.IndexOf(":"));
                }
                int result = string.Compare(tempNameX, tempNameY, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                {
                    if (ItemMajorVersion(tempX) < ItemMajorVersion(tempY))
                        result = -1;
                    else if (ItemMajorVersion(tempX) > ItemMajorVersion(tempY))
                        result = 1;
                    else if (Math.Abs(ItemMajorVersion(tempX) - ItemMajorVersion(tempY)) < 1E-06)
                    {
                        if (ItemMinorVersion(tempX) < ItemMinorVersion(tempY))
                            result = -1;
                        else if (ItemMinorVersion(tempX) > ItemMinorVersion(tempY))
                            result = 1;
                        else
                        {
                            if (string.Compare(tempX.TreeNodeLevel.ToString(), tempY.TreeNodeLevel.ToString(), StringComparison.OrdinalIgnoreCase) > 0)
                                result = -1;
                            else
                                result = 0;
                        }
                    }
                }
                return result;
            });
            return items;
        }
        private float ItemMajorVersion(TreeNode node)
        {
            float majorVersion = float.MaxValue;
            if (node.TreeNodeLevel == TreeNodeLevel.Item || node.TreeNodeLevel == TreeNodeLevel.Document)
            {
                int flag = node.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (flag >= 0)
                {
                    string versionStr = node.Name.Substring(flag + 1);
                    String[] version = versionStr.Split('.');
                    if (!float.TryParse(version[0], out majorVersion))
                    {
                        majorVersion = float.MaxValue;
                    }
                }
            }
            return majorVersion;
        }
        private float ItemMinorVersion(TreeNode node)
        {
            float minorVersion = float.MaxValue;
            if (node.TreeNodeLevel == TreeNodeLevel.Item || node.TreeNodeLevel == TreeNodeLevel.Document)
            {
                int flag = node.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (flag >= 0)
                {
                    string versionStr = node.Name.Substring(flag + 1);
                    String[] version = versionStr.Split('.');
                    if (version.Length >= 2)
                    {
                        if (!float.TryParse(version[1], out minorVersion))
                        {
                            minorVersion = float.MaxValue;
                        }
                    }
                    else
                    {
                        mLog.Warn($"check minor version failed,versionStr:{node.Name}");
                    }
                }
            }
            return minorVersion;
        }

        private List<SPTreeNodeDto> ExtractResult(List<SPTreeNodeDto> works)
        {
            List<SPTreeNodeDto> results = new List<SPTreeNodeDto>();
            List<SPTreeNodeDto> searchNodes = works;
            if (!searchNodes.IsNullOrEmpty())
            {
                foreach (var node in searchNodes)
                {
                    AddVirtualNode(node);
                    results.Add(node);
                }
            }
            return results;
        }

        private void AddVirtualNode(SPTreeNodeDto node)
        {
            if (null == node)
            {
                return;
            }
            foreach (SPTreeNodeDto child in node.Children)
            {
                child.FarmID = node.FarmID;
                child.FarmName = node.FarmName;
                child.SPType = node.SPType;
                AddVirtualNode(child);
            }

            if (node.Level == NodeLevel.Folder || node.Level == NodeLevel.List)
            {
                if (node.Children.Count == 0)
                {
                    return;
                }
                SPTreeNodeDto itemsNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Items, Level = NodeLevel.Items, CanChildrenBeLoaded = false, ChildrenLoaded = true, NodeExtension = new NodeExtensionDto() { IsAdvancedSearchResult = true }, Expanded = true, };
                SPTreeNodeDto foldersNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Folders, Level = NodeLevel.Folders, CanChildrenBeLoaded = false, ChildrenLoaded = true, NodeExtension = new NodeExtensionDto() { IsAdvancedSearchResult = true }, Expanded = true, };
                foreach (var child in node.Children)
                {
                    ///将folder或list下的节点分散到两个虚节点下
                    if (child.Level == NodeLevel.Item)
                    {
                        itemsNode.Children.Add(child);
                        itemsNode.ChildrenLoaded = true;
                        itemsNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.Folder)
                    {
                        foldersNode.Children.Add(child);
                        foldersNode.ChildrenLoaded = true;
                        foldersNode.Expanded = true;
                    }
                }
                node.Children.Clear();
                ///设置虚节点下children的个数。
                itemsNode.ChildrenCount = itemsNode.Children.Count;
                foldersNode.ChildrenCount = foldersNode.Children.Count;
                if (node.Level == NodeLevel.List) //list节点
                {
                    SPTreeNodeDto rootFolderNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.RootFolder, Level = NodeLevel.RootFolder, CanChildrenBeLoaded = true, ChildrenLoaded = true, Expanded = true };
                    rootFolderNode.Children.Add(foldersNode);
                    if (itemsNode.Children.Count > 0)
                    {
                        rootFolderNode.Children.Add(itemsNode);
                    }
                    rootFolderNode.ChildrenCount = rootFolderNode.Children.Count;
                    itemsNode.Parent = rootFolderNode;
                    foldersNode.Parent = rootFolderNode;
                    node.Children.Add(rootFolderNode);
                    rootFolderNode.Parent = node;
                }
                else
                {
                    node.Children.Add(foldersNode);
                    foldersNode.Parent = node;
                    if (itemsNode.Children.Count > 0)
                    {
                        node.Children.Add(itemsNode);
                        itemsNode.Parent = node;
                    }
                }
            }
            else if (node.Level == NodeLevel.Site)
            {
                if (node.Children.Count == 0)
                {
                    return;
                }
                ///构造虚节点
                SPTreeNodeDto listsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Lists, Level = NodeLevel.Lists, CanChildrenBeLoaded = true };
                SPTreeNodeDto sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = true };
                SPTreeNodeDto appsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Apps, Level = NodeLevel.Apps, CanChildrenBeLoaded = true };
                ///将site下的子节点分散到两个虚节点下
                foreach (var child in node.Children)
                {
                    if (child.Level == NodeLevel.List)
                    {
                        listsNode.Children.Add(child);
                        listsNode.ChildrenLoaded = true;
                        listsNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.Site)
                    {
                        sitesNode.Children.Add(child);
                        sitesNode.ChildrenLoaded = true;
                        sitesNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.App)
                    {
                        appsNode.Children.Add(child);
                        appsNode.ChildrenLoaded = true;
                        appsNode.Expanded = true;
                    }
                }
                node.Children.Clear();//清空子节点
                listsNode.ChildrenCount = listsNode.Children.Count;
                sitesNode.ChildrenCount = sitesNode.Children.Count;
                appsNode.ChildrenCount = appsNode.Children.Count;
                ///将虚拟节点添加到该节点下
                node.Children.Add(appsNode);
                appsNode.Parent = node;
                node.Children.Add(listsNode);
                listsNode.Parent = node;
                node.Children.Add(sitesNode);
                sitesNode.Parent = node;

            }
        }
        private int CaculateDepth(TreeNode tree)
        {
            int depth = 0;
            while (tree.Children.Count > 0)
            {
                tree = tree.Children[0];
                depth++;
            }
            return depth;
        }
        private TreeNode Clone(TreeNode source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<TreeNode>(serialized);
        }
        private PolicyLevel ConverTypeToLevel(string type)
        {
            switch (type)
            {
                case "W":
                    return PolicyLevel.Site;
                case "L":
                    return PolicyLevel.List;
                case "F":
                    return PolicyLevel.Folder;
                case "D":
                    return PolicyLevel.DocumentVersion;
                case "I":
                    return PolicyLevel.Item;
                case "A":
                    return PolicyLevel.Attachment;
                default:
                    return PolicyLevel.None;
            }
        }
        private SiteCollectionNodesInfo GenerateSiteCollectionInfo(ArchiverSiteMasterIndexContract index)
        {
            if (index == null)
            {
                mLog.Warn("GenerateSiteCollectionInfo received null index");
                return new SiteCollectionNodesInfo();
            }

            var result = new SiteCollectionNodesInfo()
            {
                SiteUrl = index.SiteURL,
                SPObjectId = index.SiteId,
            };
            mLog.Info($"GenerateSiteCollectionInfo mapped SiteUrl:{result.SiteUrl}, SiteGroupId:{result.SiteGroupId}, SPObjectId:{result.SPObjectId}");
            return result;
        }
        private ArchiverRestoreFilter GenerateFilter(PolicyLevel level,List<string> FullPathMD5List)
        {
            var filter = new ArchiverRestoreFilter();
            filter.Level = level;
            filter.FilterDeleteType = FilterDeletedType.All;
            filter.DataSource = (int)RestoreDataSource.M365;
            filter.FullPathMD5List = FullPathMD5List;
            filter.PageIndex = 0;
            filter.PageSize = int.MaxValue;
            filter.FilterName = string.Empty;
            return filter;
        }
        private ArchiverSiteMasterIndexContract GetSiteCollctionIndex(SiteCollectionNodesInfo node)
        {
            ArchiverSiteMasterIndexContract siteIndex = new ArchiverSiteMasterIndexContract { WebId = node.SiteGroupId, SiteId = node.SPObjectId, SiteURL = node.SiteUrl, SPVersion = 4 };
            return ArchiverIndexService.GetSiteCollectionInfo(siteIndex);
        }
        public List<TreeNode> GetSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy, int openIndexTimeoutInMs, ArchiverRestoreOrderBy orderBy)
        {
            var sitesMap = AssembleSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConverToArchiverAdvancedInfo(sitesMap, filterPolicy);
            advancedSearchInfo.OpenIndexDbTimeoutInMs = openIndexTimeoutInMs;
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.Search(advancedSearchInfo, orderBy);
            return searchResult;
        }
        private ArchiverAdvancedSearchInfo ConverToArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            ArchiverAdvancedSearchInfo searchInfo = new ArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<ArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new ArchiverSearchNodeInfo()
                {
                    BrowseInfo = new ArchiverBrowseInfo(node.SearchParam),
                    SiteId = node.SearchNode.SPObjectId,
                });
            });
            searchInfo.FilterInfors = filterPolicy;
            mLog.Info($"ConverToArchiverAdvancedInfo.searchInfo.NodeInfos count:{searchInfo.NodeInfos.Count}.");
            return searchInfo;
        }
        private List<ArchiverRestoreSearchContractDto> AssembleSearchParamInfo(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes)
        {
            mLog.Info($"AssembleSearchParamInfo.indexes count:{indexes.Count}.searchNodes count:{searchNodes.Count}.");
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();
            ArchiverSiteMasterIndexContract currentIndex = null;
            foreach (var node in searchNodes)
            {
                string siteURL = node.SiteUrl;
                currentIndex = indexes.Where<ArchiverSiteMasterIndexContract>(s => s.SiteURL.Equals(siteURL, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (currentIndex == null)
                {
                    mLog.Warn($"AssembleSearchParamInfo.currentIndex is null.SiteUrl:{siteURL}.");
                    continue;
                }
                else
                {
                    mLog.Warn($"AssembleSearchParamInfo.Succsss add ArchiverRestoreSearchContractDto.SiteUrl:{siteURL}.");
                }
                ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
                paramDto.SearchNode = node;
                paramDto.SearchParam = AssembleRestoreParamDto(currentIndex);
                paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
                sitesMap.Add(paramDto);
            }
            mLog.Info($"Finished AssembleSearchParamInfo.sitesMap count:{sitesMap.Count}.");
            return sitesMap;
        }
        private ArchiverRestoreParamDto AssembleRestoreParamDto(ArchiverSiteMasterIndexContract index)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            ArchiverRestoreParamDto param = new ArchiverRestoreParamDto
            {
                Path = index.SiteURL,
                BackupJobId = index.JobId,
                FarmName = string.Empty,
                BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode,
                StorageInfo = index.StorageInfo,
                SiteUrl = index.SiteURL,
            };
            return param;
        }
        private void SafeDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                mLog.Error("delete file faile." + ex);
            }
        }
        public async Task<Stream> GetStubStreamAsync(DocAveOnline.WebApi.Contracts.PreviewDataParam param)
        {
            try
            {
                ItemRestoreConfig config = new ItemRestoreConfig();
                using (IDisposable restoreToFSService = OpenMediaRestoreFSService())
                {
                    MediaTCPRequest configForMedia = AssembleStubPreviewMessage(param.SitePath);
                    var archiverResReq = configForMedia as ArchiverRestoreRequest;
                    archiverResReq.TenantGroupId = TenantLocalValue.LogonGroupId;
                    archiverResReq.PreviewParam = param;
                    var result = await (restoreToFSService as IArchiverRestoreToStorageService).GetStubStreamForPreviewAsync(archiverResReq);
                    return result;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"error occurd when preview stub content,error:{e}");
                throw;
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
        private void InitSkipCacheLookColumnConfigurationForBPOS()
        {
            try
            {
                var skipCacheLookColumn = RMKeyValueDao?.GetValueByKey("SkipCacheLookColumn");
                if (skipCacheLookColumn != null)
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.SkipCacheLookColumn = Convert.ToBoolean(skipCacheLookColumn.Value);
                }
                mLog.Info($"InitSkipCacheLookColumnConfigurationForBPOS.SkipCacheLookColumn:{WrapperConfiguration.WrapperConfigurationForBPOS.SkipCacheLookColumn}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitSkipCacheLookColumnConfigurationForBPOS,error:{ex}");
            }
        }

        private void InitSkipRoleNameConfigurationForBPOS()
        {
            try
            {
                var skipRoleName = RMKeyValueDao?.GetValueByKey("SkipRoleName");
                if (skipRoleName != null)
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleName = Convert.ToString(skipRoleName.Value).Split(';').ToList();
                    mLog.Info($"InitSkipRoleNameConfigurationForBPOS.SkipRoleName:{skipRoleName.Value}.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitSkipRoleNameConfigurationForBPOS,error:{ex}");
            }
        }

        private void InitSkipRoleIdConfigurationForBPOS()
        {
            try
            {
                var skipRoleId = RMKeyValueDao?.GetValueByKey("SkipRoleId");
                if (skipRoleId != null)
                {
                    WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleId = Convert.ToString(skipRoleId.Value).Split(';').Select(int.Parse).ToList();
                    mLog.Info($"InitSkipRoleIdConfigurationForBPOS.SkipRoleId:{skipRoleId.Value}.");
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitSkipRoleIdConfigurationForBPOS,error:{ex}");
            }
        }

        private bool InitForceDeleteStub()
        {
            bool forceDeleteStub = false;
            try
            {
                if (RMKeyValueDao != null && RMKeyValueDao.GetValueByKey("ForceDeleteStub") != null)
                {
                    forceDeleteStub = Convert.ToBoolean(RMKeyValueDao.GetValueByKey("ForceDeleteStub").Value);
                }
                mLog.Info($"InitForceDeleteStub.ForceDeleteStub:{forceDeleteStub}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitForceDeleteStub,error:{ex}");
            }
            return forceDeleteStub;
        }

        private bool InitThrowExceptionWhenRestoreItemCTAndFields()
        {
            bool throwExceptionWhenRestoreItemCTAndFields = true;
            try
            {
                if (RMKeyValueDao != null && RMKeyValueDao.GetValueByKey("ThrowExceptionWhenRestoreItemCTAndFields") != null)
                {
                    throwExceptionWhenRestoreItemCTAndFields = Convert.ToBoolean(RMKeyValueDao.GetValueByKey("ThrowExceptionWhenRestoreItemCTAndFields").Value);
                }
                mLog.Info($"InitThrowExceptionWhenRestoreItemCTAndFields.ThrowExceptionWhenRestoreItemCTAndFields:{throwExceptionWhenRestoreItemCTAndFields}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"failed InitThrowExceptionWhenRestoreItemCTAndFields,error:{ex}");
            }
            return throwExceptionWhenRestoreItemCTAndFields;
        }

        private void SendRestoreJobTelemetryInfo()
        {
            try
            {
                SOArchiverJobInfoStatistics statistics = SOArchiverJobInfoStatistics.Instance;
                object[] args = new object[9];
                args[0] = (JobId);
                args[1] = jobContext.MainJobStartTime;
                args[2] = (DateTime.UtcNow.Ticks);
                args[3] = (statistics.ItemSizeSumForTelemetry);
                args[4] = mJobreport.GetJobStatus().ToString();
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
        
        
        private ArchiverRestoreRequest AssembleStubPreviewMessage(string sitePath)
        {
            ArchiverRestoreRequest message = new ArchiverRestoreRequest();
            ArchiverSiteMasterIndexContract siteinfo = new ArchiverSiteMasterIndexContract();
            siteinfo.SiteURL = sitePath;
            ArchiverSiteMasterIndexContract index = ArchiverSiteMasterIndexService.GetSiteCollectionStorageInfo(siteinfo);
            List<ArchiverSiteMasterIndexContract> indexWithSubInfos = ArchiverSiteMasterIndexService.GetSiteCollectionWithSubInfos(siteinfo);
            if (!this.ValidateDataForRestore(indexWithSubInfos))
            {
                throw new AveException("The Archiver data has already been deleted by the specified Archiver Retention rules.");
            }
            ArchiverRestoreRequest request = this.BuildStubPreviewRequest(index, indexWithSubInfos);
            request.CacheLocation = GenerateCacheSettings();
            mLog.Info("Build archiver restore request, is searched result ? {0}", request.IsSearchTree);
            message = request;
            return message;
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
        

        
        

        private ArchiverRestoreRequest BuildStubPreviewRequest(ArchiverSiteMasterIndexContract index, List<ArchiverSiteMasterIndexContract> indexWithSubInfos)
        {
            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            if (indexDeviceDto == null)
            {
                throw new Exception("index device not exist");
            }
            ArchiverRestoreRequest request = new ArchiverRestoreRequest();
            request.SiteUrl = index.SiteURL;
            request.ArchiveTime = index.ArchiverTime;
            request.FarmName = string.Empty;
            request.JobId = string.Empty;
            request.IndexLogicalDevice = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
            request.DataLogicalDeviceList = index.SubInfo.Select(
                a => RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(
                    StorageDeviceService.GetStorageDeviceById(string.IsNullOrEmpty(a.CurrentStorageId) ? a.StorageInfo : a.CurrentStorageId, needDecryptSecert:true))
                ).Where(a => a != null).ToList();
            request.LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode;
            List<RestoreSecurityInfoWrapper> restoreSecurityInfos = GetRestoreSecurityInfoList(indexWithSubInfos);
            if (restoreSecurityInfos.Count > 0)
            {
                request.RestoreSecurityInfos = restoreSecurityInfos;
            }
            return request;
        }

        private static async System.Threading.Tasks.Task RestoreToStoragePolicy(ItemRestoreConfig config, MediaTCPRequest configForMedia, IDisposable fileReceiver, PlanCategory category, IRMReportManager reportManager)
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
                        await 
                        (fileReceiver as IArchiverRestoreToStorageService).HandleRestoreRequestAsync(archiverResReq, reportManager);
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
                    && rootNode.NodeExtension.BposInfo.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount
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

        private async Task<ItemRestoreConfig> HandleRestoreToStorageJobAsync(RestoreSettingAndTree mRestore, MediaTCPRequest configForMedia, bool performanceMonitorDisabled, PlanCategory category, IRMReportManager reportManager)
        {
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            var report = new AveItemRestoreReport();
            //WrapperRuntime.SetGlobalRuntimeSetting(true, new AveWrapperRunningAccountInfo());//设置WrapperRuntime.CurrentContext为多线程共用
            IAveTreeNodeDto root = mRestore.Tree[0];
            //var factory = AveObjectModelFactory.CreateObjectModelFactory(string.Empty, null, AveContextKind.Auto);
            ItemRestoreConfig config = new ItemRestoreConfig();
            config.SubJobId = configForMedia.JobId;
            config.DestinationInfo = new DestinationInfo();
            config.DestinationInfo.OwerLogin = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            config.RestoreGlobalOption = new GlobalRestoreOption();
            config.ArchiverConfigForMedia = configForMedia as ArchiverRestoreRequest;
            config.ArchiverConfigForMedia.TenantGroupId = TenantLocalValue.LogonGroupId;
            config.LoadMigrationPreferences(RMKeyValueDao, RMGlobalKeyValueDao);
            sw1.Stop();
            mLog.Info($"linkRestoreReport HandleRestoreToStorageJobAsync init LoadMigrationPreferences cost time:{sw1.ElapsedMilliseconds}");
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            await ItemRestoreConfig.BPOSSiteCollectionConfig.InitAsync(root);
            config.JobCategory = (int)category;
            mLog.Info(@"Looks up a localized string similar to Restore configuration: {0}.", config.ToString());
            sw2.Stop();
            mLog.Info($"linkRestoreReport HandleRestoreToStorageJobAsync ItemRestoreConfig.BPOSSiteCollectionConfig.InitAsync cost time:{sw2.ElapsedMilliseconds}");
            try
            {
                using (IDisposable restoreToFSService = OpenMediaRestoreFSService())
                {
                    AvePerformanceMonitor.SetDisable(performanceMonitorDisabled);
                    await RestoreToStoragePolicy(config, configForMedia, restoreToFSService, category, reportManager);
                    if (restoreToFSService is ArchiverRestoreToStorageService)
                    {
                        if ((restoreToFSService as ArchiverRestoreToStorageService).HasBlobInArchiverTier)
                        {
                            report.AddJobSummaryComment("ArchiverRehydrationAzureBlobComments");
                        }
                    }
                    mLog.Info("Looks up a localized string similar to The current job is an {0} restore. .", (PlanCategory)category);
                }
            }
            catch (JobStopException)
            {
                mLog.Warn("Job will stop,stop Rehydration and delete temp folder");
                throw;
            }
            catch (Exception e)
            {
                mLog.Error("restore to storage failed, error message:{0}", e.ToString());
                //restore to storage的report在media内部处理，只有media直接挂了才需要granular restore处理report
                report.SrcAgentName = AveEnv.AgentName;
                report.DestAgentName = AveEnv.AgentName;
                report.Init(config);
                RestoreResultInfo resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                resultInfo.AddARestoreError(e.Message, null, new string[] { });
                report.Finish(resultInfo, e.Message);
            }
            return config;
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
        private async Task<ItemRestoreConfig> HandleGranularRestoreJobAsync(RestoreSettingAndTree mRestore, MediaTCPRequest configForMedia, bool performanceMonitorDisabled, PlanCategory category, ContainerReportTracker containerReportTracker = null)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ItemRestoreConfig config = new ItemRestoreConfig();
            config.SubJobId = configForMedia.JobId;
            config.JobType = (int)mJobType;
            config.DestinationInfo = new DestinationInfo();
            config.DestinationInfo.OwerLogin = System.Environment.UserDomainName + "\\" + System.Environment.UserName;
            config.ContainerReportTracker = containerReportTracker;
            config.RestoreGlobalOption = new GlobalRestoreOption();
            config.ArchiverConfigForMedia = configForMedia as ArchiverRestoreRequest;
            if (mRestore.Setting.RestoreOption == RestoreOption.OverWrite)
            {
                config.ContainerConflictResolution = ConflictResolutionType.Merge;
                config.ContentConflictResolution = ConflictResolutionType.Overwrite;
                WrapperConfiguration.WrapperConfigurationForBPOS.OverWriteReplaceFoler = true;
            }
            else if (mRestore.Setting.RestoreOption == RestoreOption.NotOverWrite)
            {
                config.ContainerConflictResolution = ConflictResolutionType.Skip;
                config.ContentConflictResolution = ConflictResolutionType.Skip;
                WrapperConfiguration.WrapperConfigurationForBPOS.SkipReplaceFoler = true;
            }
            else
            {
                config.ContainerConflictResolution = ConflictResolutionType.Skip;
                WrapperConfiguration.WrapperConfigurationForBPOS.SkipReplaceFoler = true;
                config.ContentConflictResolution = ConflictResolutionType.AppendItemOrDocumentByReNamed;
            }
            if (mRestore.Setting.RestoreAPPOption == RestoreOption.OverWrite)
            {
                config.AppsConflictResolution = ConflictResolutionType.Overwrite;
                WrapperConfiguration.WrapperConfigurationForBPOS.OverWriteApp = true;
            }
            else
            {
                config.AppsConflictResolution = ConflictResolutionType.Skip;
            }
            config.WorkflowState = new BackupRestoreWorkflow()
            {
                IncludeWorkflowDefinition= mRestore.Setting.IncludeWorkflowDefinition
            };
            if (mRestore.IsEndUserJob)
            {
                WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore = true;
                WrapperConfiguration.WrapperConfigurationForBPOS.RestoreLookupFieldById = true;
            }
            WrapperConfiguration.IsRestoreJob = true;
            WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode = HasRestoreItemLevelNode(mRestore);
            WrapperConfiguration.WrapperConfigurationForBPOS.IsSearchAllRestore = mRestore.IsSearchAllRestore && mRestore.Setting?.SerchContract != null;
            mLog.Info($"IsEndUserRestore: {WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore}, " +
                $"HasItemLevelNode: {WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode}"+
                $"IsSearchAllRestore: {WrapperConfiguration.WrapperConfigurationForBPOS.IsSearchAllRestore}");
            var keepVersionsNumber = mRestore.Setting?.KeepVersionsNumber ?? 0;
            var shouldKeepSpecifiedVersions = mRestore.Setting?.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && keepVersionsNumber > 0;
            config.RestoreVersionSetting = shouldKeepSpecifiedVersions ? RestoreVersionSetting.MajorAndMinor : RestoreVersionSetting.All;
            config.RestoreGlobalOption = new GlobalRestoreOption();
            config.IsIncludeSharedLinks = mRestore.Setting.IncludeSharingLink;
            if (mRestore.Setting.IsSpecifyUser)
            {
                config.SpecifyUser = mRestore.Setting.SpecifyUserList.FirstOrDefault();
            }
            WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks = config.IsIncludeSharedLinks;
            config.SetRestoreOption(config.ContainerConflictResolution, config.ContentConflictResolution);
            config.SetAppOption(config.AppsConflictResolution);
            config.SetWorkflowOption(config.WorkflowState);
            ItemVersionFilter.SetConfigAttr(config.RestoreVersionSetting, shouldKeepSpecifiedVersions ? Math.Max(0, keepVersionsNumber - 1) : 0);
            IFileReceiver fileReceiver = null;
            ProductVersion productVersion = ProductVersion.ProductUnknown;
            var report = mJobreport.ReportManager;
            
            if (!WrapperRuntime.IsInitialized)
            {
                WrapperRuntime.SetGlobalRuntimeSetting(true, new AveWrapperRunningAccountInfo());//设置WrapperRuntime.CurrentContext为多线程共用
            }
            //初始化report
            IAveTreeNodeDto root = mRestore.Tree[0];
            BackupLevel restoreLevel = BackupLevel.Item;
            config.EventCategory = 523;
            config.InitDefaultValueFromConfigFile();
            sw.Stop();
            mLog.Info($"linkRestoreReport HandleGranularRestoreJobAsync init cost time:{sw.ElapsedMilliseconds}");
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            config.LoadMigrationPreferences(RMKeyValueDao, RMGlobalKeyValueDao);
            config.ItemDependencyType= ItemDependencyOption.Overwrite;
            var archiverItemDependencyType = BackgroundSettings.GetInstance().ItemDependencyOption;
            if (archiverItemDependencyType != 0)
            {
                switch (archiverItemDependencyType)
                {
                    case (int)ItemDependencyOption.NotRestore:
                        config.ItemDependencyType = ItemDependencyOption.NotRestore;
                        break;
                    case (int)ItemDependencyOption.SkipConfilctItem:
                        config.ItemDependencyType = ItemDependencyOption.SkipConfilctItem;
                        break;
                    case (int)ItemDependencyOption.Overwrite:
                        config.ItemDependencyType = ItemDependencyOption.Overwrite;
                        break;
                    case (int)ItemDependencyOption.Append:
                        config.ItemDependencyType = ItemDependencyOption.Append;
                        break;
                    case (int)ItemDependencyOption.IgnoreDifference:
                        config.ItemDependencyType = ItemDependencyOption.IgnoreDifference;
                        break;
                    default:
                        break;
                }
                mLog.Info($"Archiver Restore ItemDependencyType:{config.ItemDependencyType.ToString()}");
            }
            config.IncludeListView = true;
            config.IncludeCustomPropertyBags = true;
            bool readDataViaCache = IsReadDataViaCache();
            MediaConfigInfo.CommonConfigInfo.ReadMetaDataViaCache = readDataViaCache;
            MediaConfigInfo.CommonConfigInfo.ReadContentDataViaCache = readDataViaCache;
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView = true;
            WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore = false;
            WrapperConfiguration.WrapperConfigurationForBPOS.IsRestoreToSPOLibOrFolder = mRestore.IsRestoreToSPOLocation;
            sw1.Stop();
            mLog.Info($"linkRestoreReport HandleGranularRestoreJobAsync init2 cost time:{sw1.ElapsedMilliseconds}");
            mLog.Info($"Looks up a localized string similar to The current job config is:{config.ToString()}.");
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            if (!string.IsNullOrEmpty(mRestore.Setting.AppProfileId) && !string.IsNullOrEmpty(mRestore.Setting.SiteAdminUrl))
            {
                await BPOSSiteCollectionConfig.InitAsync(root, mRestore.Setting.AppProfileId, mRestore.Setting.SiteAdminUrl);
            }
            else
            {
                await BPOSSiteCollectionConfig.InitAsync(root);
            }

            if (mRestore.IsRestoreToSPOLocation)
            {
                config.IsRestoreToSPOLibOrFolder = true;
                await BPOSSiteCollectionConfig.InitBPOSAccountInfoAsync(mRestore.Setting.DestDto.SiteCollectionUrl);
            }

            sw2.Stop();
            mLog.Info($"linkRestoreReport BPOSSiteCollectionConfig.InitAsync cost time:{sw2.ElapsedMilliseconds}");
            IDisposable restoreService = null;

            string siteUrl = string.Empty;
            SiteStateTransitionScopeUtility siteStateScopeUtil = null;
            M365APIUtility m365APIUtility = null;
            if (mRestore.Setting.IsSupportLockedSite)
            {
                if (mJobType == JobType.TeamsArchiverRestore)
                {
                    siteUrl = SubJobDao.GetSubJob(JobId).String1;
                }
                else
                {
                    siteUrl = mRestore.IsRestoreToSPOLocation
                        ? mRestore.Setting.DestDto.SiteCollectionUrl
                        : mRestore.Setting.NodeObjects.FirstOrDefault()?.SitePath;
                }
                siteStateScopeUtil = !string.IsNullOrEmpty(siteUrl)
                    ? new SiteStateTransitionScopeUtility(siteUrl, Wrapper.Common.SiteState.Unlock, mRestore.Setting.IsSupportLockedSite, true)
                    : null;
                if (!string.IsNullOrEmpty(siteUrl) && siteStateScopeUtil != null && !siteStateScopeUtil.VerifyCurrentSiteState(Wrapper.Common.SiteState.Unlock) && siteStateScopeUtil.IsTeamPrivateChannelSite())
                {
                    mLog.Info($"The site {siteUrl} is a channel site, and the site state is not unlock, so we will try to unarchive the teams.");
                    var (groupMailboxAddress, groupSiteUrl, groupO365TenantId) = await ArchiverSiteMasterIndexService.GetArchivedChannelSiteInfoAsync(siteUrl);
                    if (string.IsNullOrEmpty(groupMailboxAddress) || string.IsNullOrEmpty(groupSiteUrl) || string.IsNullOrEmpty(groupO365TenantId))
                    {
                        mLog.Info($"The group mailbox address, group site url or group O365 tenant id is empty, so we will try to get the info from remote node service.");
                        (groupMailboxAddress, groupSiteUrl, groupO365TenantId) = await RemoteNodeService.GetChannelSiteInfoAsync(siteUrl);
                    }
                    if (!string.IsNullOrEmpty(groupMailboxAddress) && !string.IsNullOrEmpty(groupSiteUrl) && !string.IsNullOrEmpty(groupO365TenantId))
                    {
                        mLog.Info($"The group mailbox address is {groupMailboxAddress}.");
                        m365APIUtility = new(groupMailboxAddress, groupSiteUrl, groupO365TenantId);
                        m365APIUtility.TryUnarchiveTeamsForLockedChannelSite();
                    }
                    else
                    {
                        mLog.Warn("Cannot get the group mailbox address, group site url or group O365 tenant id, so we cannot unarchive the teams.");
                    }
                }
            }
            try
            {
                restoreService = OpenMediaRestoreService((PlanCategory)category, configForMedia, report.SetTotal, ref fileReceiver);

                AvePerformanceMonitor.SetDisable(performanceMonitorDisabled);
                using (var restore = AveRestoreBase.CreateInstance(config, (GCommon.Contract.GranularBackup.Object.BackupLevel)restoreLevel, productVersion))
                {
                    try
                    {
                        if (restore is AveMigrationRestore)
                        {
                            config.ObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(config.ArchiverConfigForMedia.SiteUrl, BPOSSiteCollectionConfig[config.ArchiverConfigForMedia.SiteUrl], AveContextKind.ClientObjectModel);
                        }
                        restore.SetNowAsRestoreFileModifyTime = IsSetNowAsRestoreFileModifyTime();
                        restore.Config = config;
                        restore.Report = mJobreport;
                        restore.FileReceiver = fileReceiver;
                        restore.IsEnduserRestore = mRestore.IsEndUserJob;
                        restore.PossiblyStubType = mRestore.Setting?.StubType;
                        restore.OopStubUrl = mRestore.oopStubUrl;
                        restore.IsForceDeleteStub = InitForceDeleteStub();
                        restore.ThrowExceptionWhenRestoreItemCTAndFields = InitThrowExceptionWhenRestoreItemCTAndFields();
                        if (config.IsRestoreToSPOLibOrFolder)
                        {
                            restore.DestInfo = mRestore.Setting.DestDto;
                            restore.IsRestoreToSPO = true;
                        }
                        if (mJobType == JobType.StubArchiverRestore || mJobType == JobType.M365InPlaceArchiverRestore)
                        {
                            restore.DestInfo = mRestore.Setting.DestDto;
                            restore.IsAdvancedRestore = true;
                        }
                        restore.Process();
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(ex.ToString());
                        restore.Error = ex;
                    }
                    if (restoreService is ArchiverRestoreService)
                    {
                        if ((restoreService as ArchiverRestoreService).HasBlobInArchiverTier)
                        {
                            //mJobreport.AddJobSummaryComment("ArchiverRehydrationAzureBlobComments");
                        }
                    }
                    if (restoreService is EndUserArchiverRestoreService)
                    {
                        if ((restoreService as EndUserArchiverRestoreService).BlockedRestoreArchiveTierData)
                        {
                            mLog.Error("The current job contains data in the Azure archive tier, and the current setting disables endUser to restore data in the Archive tier.");
                            //report.AddJobSummaryComment("BlockedArchiverRehydrationAzureBlobComments");
                            mJobreport.HasErrorNode = true;
                            //mJobreport.summaryComments=
                        }
                        else if ((restoreService as EndUserArchiverRestoreService).HasBlobInArchiverTier)
                        {
                            //report.AddJobSummaryComment("ArchiverRehydrationAzureBlobComments");
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error(e.ToString());
                //AOSBR-1474 如果index被破坏，job会hang在1%，不会正常failed.需要解开以下注释。
                RestoreResultInfo resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                resultInfo.AddARestoreError(e.Message, null, new string[] { });
                mJobreport.HasErrorNode = true;
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
                if (m365APIUtility != null)
                {
                    m365APIUtility.Dispose();
                }
                if (siteStateScopeUtil != null)
                {
                    try
                    {
                        siteStateScopeUtil.Dispose();
                    }
                    catch (Exception ex)
                    {
                        mJobreport.AddRestoreReport(siteUrl, 0, (int)RestoreStatus.Failed, "E", 0, siteUrl, ex.Message);
                    }
                }
            }
            return config;
        }

        private async Task<ItemRestoreConfig> HandleM365InPlaceRestoreJobAsync(string jobId, RestoreSettingAndTree restoreSetting, bool performanceMonitorDisabled, ContainerReportTracker containerReportTracker = null)
        {
            ItemRestoreConfig config = new ItemRestoreConfig
            {
                SubJobId = jobId,
                JobType = (int)mJobType,
                DestinationInfo = new DestinationInfo(),
            };
            config.DestinationInfo.OwerLogin = Environment.UserDomainName + "\\" + Environment.UserName;
            config.ContainerReportTracker = containerReportTracker;
            if (!string.IsNullOrEmpty(restoreSetting.Setting.AppProfileId) && !string.IsNullOrEmpty(restoreSetting.Setting.SiteAdminUrl))
            {
                await BPOSSiteCollectionConfig.InitAsync(restoreSetting.Tree[0], restoreSetting.Setting.AppProfileId, restoreSetting.Setting.SiteAdminUrl);
            }
            else
            {
                await BPOSSiteCollectionConfig.InitAsync(restoreSetting.Tree[0]);
            }
            try
            {
                AvePerformanceMonitor.SetDisable(performanceMonitorDisabled);
                using (var restore = AveRestoreBase.CreateInstance(config, (GCommon.Contract.GranularBackup.Object.BackupLevel)BackupLevel.Item, ProductVersion.ProductUnknown))
                {
                    try
                    {
                        restore.SetNowAsRestoreFileModifyTime = IsSetNowAsRestoreFileModifyTime();
                        restore.Config = config;
                        restore.Report = mJobreport;
                        restore.DestInfo = restoreSetting.Setting.DestDto;
                        restore.IsAdvancedRestore = true;
                        restore.RestoreSettingAndTree = restoreSetting;
                        await restore.ProcessForM365ArchiveAsync();
                    }
                    catch (JobStopException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(ex.ToString());
                        restore.Error = ex;
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLog.Error(e.ToString());
                RestoreResultInfo resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                resultInfo.AddARestoreError(e.Message, null, new string[] { });
                mJobreport.HasErrorNode = true;
            }
            finally
            {

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

        private bool HasRestoreItemLevelNode(RestoreSettingAndTree mRestore)
        {
            return HasItemLevelNode(new SPTreeNodeDto()
            {
                Level = NodeLevel.Undefined,
                Children = mRestore.Tree
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

        private static IDisposable OpenMediaRestoreService(PlanCategory categroy, MediaTCPRequest restoreRequest, Action<long> updateProgress, ref IFileReceiver fileReceiver)
        {
            IDisposable restoreService = null;
            switch (categroy)
            {
                case PlanCategory.ArchiverRestore: //20: Archiver Restore
                    var archiverDataBlockManager = new ArchiverRestoreDataBlockManger();
                    fileReceiver = new ArchiverMemoryFileReceiver(archiverDataBlockManager);

                    if (restoreRequest is ArchiverRestoreRequest && ((ArchiverRestoreRequest)restoreRequest).IsEndUserRequest)
                    {
                        restoreService = MediaServiceFactory.CreateEndUserArchiverRestoreService();//container.Resolve<IEndUserArchiverRestoreService>("AvePoint.Media.Service.ArchiverBackup.Restore.IEndUserArchiverRestoreService");
                        (restoreService as IEndUserArchiverRestoreService).HandleRequest(restoreRequest, archiverDataBlockManager, updateProgress);
                    }
                    else
                    {
                        restoreService = MediaServiceFactory.CreateArchiverRestoreService();//container.Resolve<IArchiverRestoreService>("AvePoint.Media.Service.ArchiverBackup.Restore.IArchiverRestoreService");
                        (restoreService as IArchiverRestoreService).HandleRequest(restoreRequest, archiverDataBlockManager, updateProgress);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return restoreService;
        }

        private static IDisposable OpenMediaRestoreFSService()
        {
            IDisposable restoreToFSService = null;
            restoreToFSService = MediaServiceFactory.CreateArchiverRestoreToStorageService();
            return restoreToFSService;
        }

        private void UpdateDownloadDataInfo(RMDownloadDataInfo downloadDataInfo, DownloadContentJobStatus downloadStatus)
        {
            const int maxRetry = 3;
            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    downloadDataInfo.JobStatus = (int)downloadStatus;

                    var success = _downloadDataInfoDao.UpdateDownloadInfo(downloadDataInfo);

                    mLog.Info("Update download file status to {0} attempt {1}/{2}: {3}.", downloadStatus, attempt, maxRetry, success ? "successful" : "failure");

                    if (success) return;
                }
                catch (Exception ex)
                {
                    mLog.Warn("Update download file status to {0} attempt {1}/{2} failed. Error: {3}", downloadStatus, attempt, maxRetry, ex.Message);
                }
                Thread.Sleep(1000);
            }
            mLog.Error("Update download file status to {0} failed after {1} retries.", downloadStatus, maxRetry);
        }

        private Task<OpusScanStagingDatabase> ScanSiteForOpusStubsAsync(RestoreSettingAndTree restoreSettingAndTree)
        {
            WrapperConfiguration.WrapperConfigurationForBPOS.RestoreObjectLevel = GetRestoreObjectLevel(restoreSettingAndTree);
            WrapperConfiguration.WrapperConfigurationForBPOS.RestoreScope = restoreSettingAndTree.Setting.RestoreScope;
            mLog.Info($"ScanSiteForOpusStubsAsync: start site scan mode for restore object level:{WrapperConfiguration.WrapperConfigurationForBPOS.RestoreObjectLevel}");
            string siteUrl = restoreSettingAndTree.Setting.DestDto.SiteCollectionUrl;
            string scopeUrl = WrapperConfiguration.WrapperConfigurationForBPOS.RestoreObjectLevel switch
            {
                RestoreObjectLevel.SiteCollection => restoreSettingAndTree.Setting.DestDto.SiteCollectionUrl,
                RestoreObjectLevel.Site => restoreSettingAndTree.Setting.DestDto.WebPath,
                RestoreObjectLevel.List => restoreSettingAndTree.Setting.DestDto.ListPath,
                RestoreObjectLevel.Folder => restoreSettingAndTree.Setting.DestDto.FolderPath,
                _ => null
            };
            if (string.IsNullOrWhiteSpace(scopeUrl))
            {
                mLog.Warn("ScanSiteForOpusStubsAsync received an empty scope.");
                return Task.FromResult<OpusScanStagingDatabase>(null);
            }
            RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
            if (remoteSiteCollection == null)
            {
                mLog.Error($"ScanSiteForOpusStubsAsync: the site [{siteUrl}] was not found in opus db.");
                throw new Exception("RM_JS_Rule_SPDestUrlError");
            }
            var bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
            var objectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            OpusScanStagingDatabase database = null;
            OpusScanStagingWriter writer = null;
            using SiteStateTransitionScopeUtility siteStateScopeUtil = new(siteUrl, Wrapper.Common.SiteState.ReadOnly, restoreSettingAndTree.Setting.IsSupportLockedSite, true);
            try
            {
                database = new OpusScanStagingDatabase(BackgroundSettings.GetInstance().ArchiveTemp, JobId);
                writer = new OpusScanStagingWriter(database);
                writer.WriteContainers(new[]
                {
                    new OpusScanContainer
                    {
                        ContainerUrl = siteUrl,
                        SiteUrl = siteUrl,
                        WebUrl = siteUrl,
                        Name = siteUrl,
                        DisplayName = siteUrl,
                        FullPathForUI = siteUrl,
                        ContainerType = OpusScanContainerType.SiteCollection,
                    }
                });

                using (IAveSite site = objectModelFactory.CreateSite(siteUrl))
                {
                    OpusStubRestoreSiteId = site.ID.ToString();
                    IAveWeb rootWeb = site.OpenWeb();
                    try
                    {
                        StageWebContainer(writer, rootWeb, siteUrl, true);
                        switch (WrapperConfiguration.WrapperConfigurationForBPOS.RestoreObjectLevel)
                        {
                            case RestoreObjectLevel.SiteCollection:
                                ScanWebForOpusStubs(rootWeb, writer, siteUrl, includeDescendantWebs: true);
                                break;
                            case RestoreObjectLevel.Site:
                                IAveWeb targetWeb = site.OpenWeb(scopeUrl);
                                EnsureScopeExists(targetWeb, scopeUrl);
                                StageWebScopeChain(rootWeb, targetWeb, writer, siteUrl);
                                ScanWebForOpusStubs(targetWeb, writer, siteUrl, includeDescendantWebs: true);
                                break;
                            case RestoreObjectLevel.List:
                            case RestoreObjectLevel.Folder:
                                IAveWeb scopeWeb = ResolveScopeWeb(site, scopeUrl);
                                IAveList scopeList = scopeWeb.GetList(scopeUrl);
                                EnsureScopeExists(scopeList, scopeUrl);
                                StageWebScopeChain(rootWeb, scopeWeb, writer, siteUrl);
                                IAveFolder scopeFolder = WrapperConfiguration.WrapperConfigurationForBPOS.RestoreObjectLevel == RestoreObjectLevel.Folder
                                    ? scopeList.GetFolder(NormalizeUrlPath(scopeUrl))
                                    : scopeList.RootFolder;
                                EnsureScopeExists(scopeFolder, scopeUrl);
                                ScanListForOpusStubs(scopeList, scopeFolder, writer, siteUrl);
                                break;
                        }
                    }
                    finally
                    {
                        rootWeb.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                database?.Dispose();
                mLog.Warn($"ScanSiteForOpusStubsAsync: failed to scan scope [{scopeUrl}]. Error:{ex}");
                AddScanFailureReport(scopeUrl, ex.Message);
                throw;
            }
            finally
            {
                writer?.Dispose();
            }

            return Task.FromResult(database);
        }

        private async Task ExecuteStagedOpusRestoreAsync(RestoreSettingAndTree restoreSetting, bool performanceMonitorDisabled, RMDownloadDataInfo downloadDataInfo)
        {
            using OpusScanStagingDatabase database = await ScanSiteForOpusStubsAsync(restoreSetting);
            if (database == null)
            {
                throw new Exception("RM_JS_Rule_SPDestUrlError");
            }

            long itemCount = database.CountScanResults();
            if (itemCount == 0)
            {
                restoreSetting.Tree = new List<SPTreeNodeDto>();
                mLog.Info("Opus stub scan completed with zero files. Finishing the job without restore execution.");
                throw new Exception("RM_JS_Rule_SPNoStubFound");
            }
            mLog.Info($"Opus stub scan completed with {itemCount} items. Proceeding to restore execution.");

            byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
            CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
            ConfigureMediaEnviroment();
            PlanCategory category = (PlanCategory)20;
            bool initializeRestoredFileContext = true;
            ContainerReportTracker containerReportTracker = new ContainerReportTracker();
            OpusScanStagingReader reader = new OpusScanStagingReader(database);
            foreach (OpusScanBatch batch in reader.ReadBatches(OpusStubRestoreBatchSize))
            {
                using (new CheckJobStopScope())
                {
                    restoreSetting.Tree = BuildRestoreTreeFromScanBatch(batch);
                    if (restoreSetting.Tree.Count == 0)
                    {
                        continue;
                    }

                    switch (mJobType)
                    {
                        case JobType.StubArchiverRestore:
                            {
                                MediaTCPRequest configForMedia = AssembleRestoreMessage(JobId, restoreSetting.Tree[0], restoreSetting, isPreview: false, initializeRestoredFileContext: initializeRestoredFileContext);
                                initializeRestoredFileContext = false;
                                await HandleGranularRestoreJobAsync(restoreSetting, configForMedia, performanceMonitorDisabled, category, containerReportTracker);
                                break;
                            }
                        case JobType.M365InPlaceArchiverRestore:
                            {
                                await HandleM365InPlaceRestoreJobAsync(JobId, restoreSetting, performanceMonitorDisabled, containerReportTracker);
                                break;
                            }
                    }
                }
            }
        }

        private static bool PruneEmptyBranches(TreeNode node)
        {
            if (node == null)
            {
                return false;
            }
            if (node.TreeNodeLevel == TreeNodeLevel.Item)
            {
                return true;
            }

            // Recurse first so child Site/List/Folder nodes are pruned bottom-up.
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                TreeNode child = node.Children[i];
                bool childHasContent = PruneEmptyBranches(child);
                bool isPrunableLevel = child.TreeNodeLevel == TreeNodeLevel.Folder
                    || child.TreeNodeLevel == TreeNodeLevel.List
                    || child.TreeNodeLevel == TreeNodeLevel.Site;
                if (isPrunableLevel && !childHasContent)
                {
                    node.Children.RemoveAt(i);
                }
            }

            return HasDocumentDescendant(node);
        }

        private static bool HasDocumentDescendant(TreeNode node)
        {
            if (node?.Children == null || node.Children.Count == 0)
            {
                return false;
            }
            foreach (TreeNode child in node.Children)
            {
                if (child.TreeNodeLevel == TreeNodeLevel.Item)
                {
                    return true;
                }
                if (HasDocumentDescendant(child))
                {
                    return true;
                }
            }
            return false;
        }

        private static TreeNode BuildWebNode(IAveWeb web, string siteUrl, bool isRoot)
        {
            string displayName = isRoot ? "." : web.Title;
            TreeNode webNode = new TreeNode
            {
                Name = isRoot ? "." : web.Name,
                DisplayName = displayName,
                Title = displayName,
                FullPath = web.ServerRelativeUrl,
                FullPathForUI = web.Url,
                TreeNodeLevel = TreeNodeLevel.Site,
                SitePath = siteUrl,
                Expanded = true,
                ChildrenLoaded = true,
                CanChildrenBeLoaded = false,
                Children = [],
            };
            return webNode;
        }

        private static void StageWebContainer(OpusScanStagingWriter writer, IAveWeb web, string siteUrl, bool isRoot)
        {
            string containerUrl = GetWebContainerUrl(web, siteUrl);
            string parentUrl = isRoot || web.ParentWeb == null
                ? siteUrl
                : GetWebContainerUrl(web.ParentWeb, siteUrl);
            writer.WriteContainers(new[]
            {
                new OpusScanContainer
                {
                    ContainerUrl = containerUrl,
                    ParentUrl = isRoot ? siteUrl : parentUrl,
                    SiteUrl = siteUrl,
                    WebUrl = web.ServerRelativeUrl,
                    ContainerType = OpusScanContainerType.Site,
                    Name = isRoot ? "." : web.Name,
                    DisplayName = isRoot ? "." : web.Title,
                    FullPathForUI = web.Url,
                }
            });
        }

        private static void StageWebScopeChain(IAveWeb rootWeb, IAveWeb targetWeb, OpusScanStagingWriter writer, string siteUrl)
        {
            if (targetWeb == null || targetWeb.ID == rootWeb.ID)
            {
                return;
            }

            Stack<IAveWeb> webChain = new Stack<IAveWeb>();
            IAveWeb currentWeb = targetWeb;
            while (currentWeb != null && currentWeb.ID != rootWeb.ID)
            {
                webChain.Push(currentWeb);
                currentWeb = currentWeb.ParentWeb;
            }

            while (webChain.Count > 0)
            {
                StageWebContainer(writer, webChain.Pop(), siteUrl, false);
            }
        }

        private static string GetWebContainerUrl(IAveWeb web, string siteUrl)
        {
            string webPath = NormalizeUrlPath(web?.ServerRelativeUrl);
            return string.IsNullOrEmpty(webPath) ? siteUrl + "#rootweb" : webPath;
        }

        private static bool IsFolderWithinList(string folderPath, string listRootPath)
        {
            return folderPath.Equals(listRootPath, StringComparison.OrdinalIgnoreCase)
                || folderPath.StartsWith(listRootPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddFolderContainers(
            ICollection<OpusScanContainer> containers,
            ISet<string> containerUrls,
            string folderPath,
            string listRootPath,
            string siteUrl,
            string webServerRelativeUrl)
        {
            if (folderPath.Equals(listRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string relativePath = folderPath.Substring(listRootPath.TrimEnd('/').Length).Trim('/');
            string parentUrl = listRootPath;
            string currentUrl = listRootPath;
            foreach (string segment in relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                currentUrl = currentUrl.TrimEnd('/') + "/" + segment;
                if (!containerUrls.Add(currentUrl))
                {
                    parentUrl = currentUrl;
                    continue;
                }

                string folderName = Uri.UnescapeDataString(segment);
                containers.Add(new OpusScanContainer
                {
                    ContainerUrl = currentUrl,
                    ParentUrl = parentUrl,
                    SiteUrl = siteUrl,
                    WebUrl = webServerRelativeUrl,
                    ContainerType = OpusScanContainerType.Folder,
                    Name = folderName,
                    DisplayName = folderName,
                    FullPathForUI = GetWebRelativeUrl(webServerRelativeUrl, currentUrl),
                });
                parentUrl = currentUrl;
            }
        }

        private OpusScanItem BuildStubScanItem(IAveListItem item, string siteUrl, string webServerRelativeUrl, string listRootPath, string folderPath)
        {
            string uniqueId = GetItemFieldGuid(item, "UniqueId").ToString();
            string itemId = GetItemFieldString(item, "ID") ?? uniqueId;
            string siteId = GetGraphSiteId(item);
            string listId = item.ParentList.ID.ToString();
            string rowId = item.ID.ToString();
            return new OpusScanItem
            {
                ItemId = itemId,
                UniqueId = uniqueId,
                SiteUrl = siteUrl,
                WebUrl = webServerRelativeUrl,
                ListUrl = listRootPath,
                ParentUrl = folderPath,
                FileUrl = GetItemFieldString(item, "FileRef") ?? string.Empty,
                FileName = GetItemFieldString(item, SPColumnConstants.FileLeafRef) ?? string.Empty,
                Extension = $"{siteId}|{listId}|{rowId}",
                Size = GetItemFieldLong(item, SPColumnConstants.File_Size),
            };
        }

        private string GetGraphSiteId(IAveListItem item)
        {
            var spWeb = item.ParentList?.ParentWeb;
            if (spWeb == null)
            {
                mLog.Warn("Restore by M365 GetGraphSiteId: spWeb is null, fallback to SiteId");
                return OpusStubRestoreSiteId;
            }
            var graphSiteId = $"{new Uri(spWeb.Url).Host},{OpusStubRestoreSiteId},{spWeb.ID.ToString()}";
            mLog.Info($"Restore by M365 GetGraphSiteId: {graphSiteId}");
            return graphSiteId;
        }

        private static void EnsureScopeExists(object scopeObject, string scopeUrl)
        {
            if (scopeObject == null)
            {
                throw new InvalidOperationException($"The SharePoint scope [{scopeUrl}] could not be resolved.");
            }

            if (scopeObject is IAveWeb web && !web.Exists)
            {
                throw new InvalidOperationException($"The SharePoint web scope [{scopeUrl}] does not exist.");
            }

            if (scopeObject is IAveFolder folder && !folder.Exists)
            {
                throw new InvalidOperationException($"The SharePoint folder scope [{scopeUrl}] does not exist.");
            }
        }

        private static IAveWeb ResolveScopeWeb(IAveSite site, string scopeUrl)
        {
            string candidatePath = NormalizeUrlPath(scopeUrl);
            string sitePath = NormalizeUrlPath(site.ServerRelativeUrl);
            while (!string.IsNullOrEmpty(candidatePath))
            {
                IAveWeb candidateWeb = site.OpenWeb(candidatePath);
                if (candidateWeb.Exists)
                {
                    return candidateWeb;
                }

                if (candidatePath.Equals(sitePath, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                int separatorIndex = candidatePath.LastIndexOf('/');
                candidatePath = separatorIndex <= 0 ? sitePath : candidatePath.Substring(0, separatorIndex);
            }

            return site.RootWeb;
        }

        private void ScanWebForOpusStubs(IAveWeb web, OpusScanStagingWriter writer, string siteUrl, bool includeDescendantWebs)
        {
            if (web == null)
            {
                return;
            }
            try
            {
                mLog.Info($"ScanWebForOpusStubs: scanning web [{web.Url}]");
                using var _ = new CheckJobStopScope();

                foreach (IAveList list in web.Lists)
                {
                    if (list.BaseType != AveBaseType.DocumentLibrary)
                    {
                        continue;
                    }
                    try
                    {
                        ScanListForOpusStubs(list, list.RootFolder, writer, siteUrl);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"ScanWebForOpusStubs: failed to scan library [{list.RootFolder?.ServerRelativeUrl}] on web [{web.Url}]. Error:{ex}");
                        AddScanFailureReport(list.RootFolder?.ServerRelativeUrl ?? web.Url, ex.Message);
                    }
                }

                if (!includeDescendantWebs)
                {
                    return;
                }

                foreach (IAveWeb subWeb in web.Webs)
                {
                    StageWebContainer(writer, subWeb, siteUrl, false);
                    try
                    {
                        ScanWebForOpusStubs(subWeb, writer, siteUrl, includeDescendantWebs: true);
                    }
                    finally
                    {
                        subWeb.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"ScanWebForOpusStubs: failed to scan web [{web.Url}]. Error:{ex}");
                AddScanFailureReport(web.Url, ex.Message);
            }
        }

        private void ScanListForOpusStubs(IAveList list, IAveFolder scanFolder, OpusScanStagingWriter writer, string siteUrl)
        {
            if (list == null || scanFolder == null || !scanFolder.Exists)
            {
                return;
            }

            string listRootPath = NormalizeUrlPath(list.RootFolder?.ServerRelativeUrl);
            string scanFolderPath = NormalizeUrlPath(scanFolder.ServerRelativeUrl);
            string webUrl = GetWebContainerUrl(list.ParentWeb, siteUrl);
            writer.WriteContainers(new[]
            {
                new OpusScanContainer
                {
                    ContainerUrl = listRootPath,
                    ParentUrl = webUrl,
                    SiteUrl = siteUrl,
                    WebUrl = list.ParentWeb.ServerRelativeUrl,
                    ContainerType = OpusScanContainerType.List,
                    Name = list.Title,
                    DisplayName = list.Title,
                    FullPathForUI = list.RootFolder?.ServerRelativeUrl,
                }
            });
            bool includeChildren = WrapperConfiguration.WrapperConfigurationForBPOS.RestoreScope == RestoreScope.IncludeChildrenContainersAndFolders;
            mLog.Info($"ScanListForOpusStubs: scanning list [{listRootPath}] from folder [{scanFolderPath}] (includeChildren={includeChildren})");
            using var _ = new CheckJobStopScope();

            List<OpusScanItem> itemBatch = new List<OpusScanItem>(OpusStubScanPageSize);
            List<OpusScanContainer> containerBatch = new List<OpusScanContainer>(OpusStubScanPageSize);
            HashSet<string> containerUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IAveListItem item in QueryOpusStubItems(list, scanFolderPath, includeChildren))
            {
                try
                {
                    string itemFolderPath = GetItemFieldString(item, "FileDirRef") ?? scanFolderPath;
                    string normalizedFolderPath = NormalizeUrlPath(itemFolderPath);
                    if (!IsFolderWithinList(normalizedFolderPath, listRootPath))
                    {
                        continue;
                    }
                    AddFolderContainers(containerBatch, containerUrls, normalizedFolderPath, listRootPath, siteUrl, list.ParentWeb.ServerRelativeUrl);
                    itemBatch.Add(BuildStubScanItem(item, siteUrl, list.ParentWeb.ServerRelativeUrl, listRootPath, normalizedFolderPath));
                    if (itemBatch.Count >= OpusStubScanPageSize)
                    {
                        writer.WriteContainers(containerBatch);
                        writer.WriteItems(itemBatch);
                        itemBatch.Clear();
                        containerBatch.Clear();
                        containerUrls.Clear();
                    }
                }
                catch (Exception ex)
                {
                    string itemPath = GetItemFieldString(item, "FileRef") ?? scanFolderPath;
                    mLog.Warn($"ScanListForOpusStubs: failed to build stub candidate [{itemPath}]. Error:{ex}");
                    AddScanFailureReport(itemPath, ex.Message);
                }
            }

            writer.WriteContainers(containerBatch);
            writer.WriteItems(itemBatch);
        }

        private static AveCamlQuery CreateOpusStubQuery(string folderPath, int startIndex, int endIndex, bool includeChildren, bool isArchivedByM365)
        {
            CAMLManager cm = new();
            cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIndex.ToString()));
            cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, endIndex.ToString()));
            cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, Types.FieldRefTypes.Name, "FSObjType", Types.FieldTypes.Integer, Types.QueryTypes.Eq, "0"));
            
            if (isArchivedByM365)
            {
                cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, Types.FieldRefTypes.Name, "_FileArchiveStatus", Types.FieldTypes.Text, Types.QueryTypes.IsNotNull, null));
                cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, Types.FieldRefTypes.Name, "_FileArchiveStatus", Types.FieldTypes.Text, Types.QueryTypes.Neq, string.Empty));
            }
            else
            {
                cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, Types.FieldRefTypes.Name, LinkFileCommon.LinkFileFieldName, Types.FieldTypes.Text, Types.QueryTypes.IsNotNull, null));
                cm.QueryGroup.AddCondition(new(Types.JoinTypes.And, Types.FieldRefTypes.Name, LinkFileCommon.LinkFileFieldName, Types.FieldTypes.Text, Types.QueryTypes.Neq, string.Empty));
            }

            cm.AddViewFields(new(SPColumnConstants.SP_ID));
            cm.AddViewFields(new("UniqueId"));
            cm.AddViewFields(new("FileRef"));
            cm.AddViewFields(new("FileDirRef"));
            cm.AddViewFields(new(SPColumnConstants.FileLeafRef));
            cm.AddViewFields(new("FSObjType"));
            cm.AddViewFields(new(SPColumnConstants.File_Size));
            cm.AddViewFields(new(SPColumnConstants.SP_Created));
            cm.AddViewFields(new(SPColumnConstants.Modified));

            cm.ScopeType = includeChildren ? Types.ScopeTypes.RecursiveAll : Types.ScopeTypes.FilesOnly;
            cm.RowLimit = OpusStubScanPageSize;

            AveCamlQuery query = new()
            {
                LoadAllItems = false,
                FolderServerRelativeUrl = NormalizeUrlPath(folderPath),
                ListItemCollectionPosition = new AveItemCollectionPosition(),
                ViewXml = cm.GetFullCAML(),
            };
            return query;
        }

        /// <summary>
        /// Streams one CAML page at a time so large document libraries do not create a
        /// second in-memory collection of matching stub items. The paging token returned
        /// by SharePoint is copied to the next query before requesting the next page.
        /// </summary>
        private IEnumerable<IAveListItem> QueryOpusStubItems(IAveList list, string folderPath, bool includeChildren)
        {
            bool isArchivedByM365 = mJobType == JobType.M365InPlaceArchiverRestore;
            int startIndex = 0;
            var folder = list.GetFolder(folderPath);
            int firstIndex = SPCommonUtility.GetFirstItemFolderId(list, folder);
            int endIndex = SPCommonUtility.GetLastItemFolderId(list, folder);
            while (endIndex > firstIndex)
            {
                startIndex = Math.Max(endIndex - OpusStubScanPageSize, firstIndex - 1);
                mLog.Debug($"QueryOpusStubItems: querying list [{list.RootFolder.ServerRelativeUrl}] folder [{folderPath}], query by id from {startIndex} to {endIndex}");
                AveCamlQuery query = CreateOpusStubQuery(folderPath, startIndex, endIndex, includeChildren, isArchivedByM365);
                IAveListItemCollection page;
                try
                {
                    mLog.Debug($"QueryOpusStubItems: querying list [{list.RootFolder?.ServerRelativeUrl}] folder [{folderPath}] with CAML [{query.ViewXml}]");
                    page = list.GetItems(query);
                }
                catch (Exception ex)
                {
                    mLog.Warn($"QueryOpusStubItems: failed to query list [{list.RootFolder?.ServerRelativeUrl}] folder [{folderPath}]. Error:{ex}");
                    AddScanFailureReport(folderPath, ex.Message);
                    throw;
                }
                if (page == null)
                {
                    yield break;
                }
                foreach (IAveListItem item in page)
                {
                    yield return item;
                }
                endIndex = startIndex;
            }
        }

        private static string GetWebRelativeUrl(string webServerRelativeUrl, string serverRelativeUrl)
        {
            string webPath = NormalizeUrlPath(webServerRelativeUrl);
            string itemPath = NormalizeUrlPath(serverRelativeUrl);
            if (!string.IsNullOrEmpty(webPath) && itemPath.StartsWith(webPath.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase))
            {
                return itemPath.Substring(webPath.TrimEnd('/').Length + 1);
            }
            return itemPath.TrimStart('/');
        }

        private static string GetItemFieldString(IAveListItem item, string fieldName)
        {
            if (item?.FieldValues == null || !item.FieldValues.TryGetValue(fieldName, out object value) || value == null)
            {
                return null;
            }
            return value.ToString();
        }

        private static Guid GetItemFieldGuid(IAveListItem item, string fieldName)
        {
            string value = GetItemFieldString(item, fieldName);
            if (Guid.TryParse(value, out Guid result))
            {
                return result;
            }
            return item?.UniqueId ?? Guid.Empty;
        }

        private static DateTime GetItemFieldDate(IAveListItem item, string fieldName)
        {
            string value = GetItemFieldString(item, fieldName);
            return DateTime.TryParse(value, out DateTime result) ? result : DateTime.MinValue;
        }

        private static long GetItemFieldLong(IAveListItem item, string fieldName)
        {
            string value = GetItemFieldString(item, fieldName);
            return long.TryParse(value, out long result) ? result : 0L;
        }

        private static string NormalizeUrlPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }
            string path = Uri.TryCreate(url, UriKind.Absolute, out Uri parsed) ? parsed.AbsolutePath : url;
            return HttpUtility.UrlDecode(path.TrimEnd('/'));
        }

        private RestoreObjectLevel GetRestoreObjectLevel(RestoreSettingAndTree restoreSettingAndTree)
        {
            if (!string.IsNullOrWhiteSpace(restoreSettingAndTree.Setting.DestDto.FolderPath))
            {
                return RestoreObjectLevel.Folder;
            }
            else if (!string.IsNullOrWhiteSpace(restoreSettingAndTree.Setting.DestDto.ListPath))
            {
                return RestoreObjectLevel.List;
            }
            else if (!string.IsNullOrWhiteSpace(restoreSettingAndTree.Setting.DestDto.WebPath))
            {
                return RestoreObjectLevel.Site;
            }
            else
            {
                return RestoreObjectLevel.SiteCollection;
            }
        }

        private void AddScanFailureReport(string path, string message, JobDetailsStatus status = JobDetailsStatus.Failed)
        {
            if (string.IsNullOrWhiteSpace(message) || message.EqualIgnoreCase("One or more field types are not installed properly. Go to the list settings page to delete these fields."))
            {
                mLog.Warn($"AddScanFailureReport: skipping scan failure report for path [{path}] due to empty or known error message.");
                return;
            }
            try
            {
                mJobreport?.AddRestoreReport(path ?? string.Empty, 0, (int)status, string.Empty, DateTime.UtcNow.Ticks, path ?? string.Empty, message);
            }
            catch (Exception ex)
            {
                mLog.Warn($"AddScanFailureReport: failed to write scan failure report for path [{path}]. Error:{ex}");
            }
        }

        public async Task<ArchiverRestoreSerchResult> GetRestoreInfoNodeObjects(RestoreExecutionRequest request)
        {
            string targetUrl = request.Scope;
            string normalizedTarget = string.IsNullOrWhiteSpace(targetUrl)
                ? string.Empty
                : targetUrl.Trim().TrimEnd('/');

            var siteNodes = await RestoreSearchService.GetSiteCollectionNodesByUrlAsync(targetUrl);
            var siteNode = siteNodes?.FirstOrDefault(node =>
            {
                string nodeUrl = string.IsNullOrWhiteSpace(node?.SiteUrl)
                    ? string.Empty
                    : node.SiteUrl.Trim().TrimEnd('/');
                return string.Equals(nodeUrl, normalizedTarget, StringComparison.OrdinalIgnoreCase);
            });

            if (siteNode == null)
            {
                return null;
            }

            var searchRequest = new ArchiverRestoreResult
            {
                PageIndex = 1,
                PageSize = 1,
                SerchContract = new BackupDataSearchContract
                {
                    SearchNode = siteNode,
                    FilterPolicy = new ArchiverRestoreFilter
                    {
                        FilterDeleteType = FilterDeletedType.All,
                        DataSource = 1,
                        Level = PolicyLevel.SiteCollection,
                        FilterName = string.Empty
                    }
                }
            };
            var searchResult = await RestoreSearchService.GetSearchTreeResultAsync(searchRequest);
            var restoreNode = searchResult?.RestoreSerchNodes?.FirstOrDefault(node =>
                node != null && !string.IsNullOrWhiteSpace(node.TreeNode));

            return restoreNode;
        }

    }
}

