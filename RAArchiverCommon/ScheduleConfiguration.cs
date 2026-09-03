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
using AvePoint.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Browser;
using System.Collections.Concurrent;
using RAArchiverCommon;
using AvePoint.Wrapper.Common.Office;
using System.Diagnostics;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Archiver.Media;
using AvePoint.Media.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Explorer;
using RAManualApprovalCommon;
using RAManualApprovalCommon.Archiver;
using AvePoint.Wrapper.Common.Graph;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.Contract.Archiver;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using HSMCommon;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using RAArchiverCommon.DisposalProgress.Impl;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Model.DisposalStub;
using RAArchiverCommon.Utility;
using RAArchiverCommon.TeamsController;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class ScheduleConfiguration
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ScheduleConfiguration));
        private readonly object mSPLabelLock = new object();

        //public readonly ArchiverMessage archiverMessage;
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IDBInfoDao DBInfoDao => PlatformWindsorManager.GetService<IDBInfoDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IWorkplaceHoldDao WorkplaceHoldDao => PlatformWindsorManager.GetService<IWorkplaceHoldDao>();
        private static readonly ConcurrentDictionary<string, long> _workspaceReleaseTimeCache = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        public string JobId { get; private set; }
        public string MainJobId { get; private set; }
        public DateTime ArchiverUNCTime;
        public string WebAppId;
        public string WebAppUrl;
        public string SiteCollectionUrl;
        public Guid SiteCollectionID;
        public Guid ContainerId;
        public string AveSiteId;
        public int RunJobNodeLevel;
        public string siteUrl = string.Empty;
        public bool UseAospArchiverProfile;

        private ArchiveJobSplitedDBInfo archiveJobSplitedDBInfo = new ArchiveJobSplitedDBInfo();

        public ArchiveJobSplitedDBInfo ArchiveJobSplitedDBInfo { get => archiveJobSplitedDBInfo; set => archiveJobSplitedDBInfo = value; }

        public List<string> RADisplayColumns = new List<string>() { "Content Type", "Created", "Author", "Modified", "Editor" };//add for RevIM report

        public List<string> FailedObjectIds = new List<string>();

        #region Static Config
        public static List<int> ListTemplate = new List<int>() { 100, 101, 103, 104, 106, 107, 108, 115, 119, 433, 700, 851, 1302, };
        public const string linkFileFieldXml = "<Field Type='Text' DisplayName='ArchiverLinkFileType' Name='ArchiverLinkFileType' ID='b4b338db-fc52-4bf4-a363-0ae0b59ec1cd' Hidden='TRUE'/>";
        public static bool IsDeleteRecord = false;
        public static Guid HoldRecordStatus = new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
        public static string BlockDelete = "BlockDelete";
        public static string BlockDeleteEdit = "BlockDelete, BlockEdit";
        public readonly static object mLock = new object();

        #endregion


        public Dictionary<int, Rule> RuleCollection = null;
        public bool OneDriveNullClassification = false;
        public Rule currentRule = null;
        public ActionType actionType = ActionType.ArchiverAndRemove;
        public ArchiverAction Action = ArchiverAction.NONE;
        public Contract.JobMonitor.JobType jobtype = Contract.JobMonitor.JobType.None;
        public DateTime mInitialTime = DateTime.Now;

        public AveObjectModelFactory aveObjectModelFactory;

        public JobReportImps JobReportDto = null;
        public JobReportImps ProgressDto = null;

        public CompoundDisposalStatistics compoundStatistics = null;

        public ArchiverExtendSettingDto ArchiverExtendSetting = null;

        public EndUserArchiveSiteCollectionConfig EndUserArchiveSiteCollectionConfig;

        public RelativeDataJobReortOperation relativeDataJobReportOperation;

        public ScheduleProcedure Procedure { get; private set; }

        public bool mUseQueryDiscover = false;

        #region add for sp query
        public bool DiscoverWithSPQuery = false;
        public bool DiscoverWithSPQueryForVersion = false;
        public bool SkipDiscoverItemForFolderLevelRule = false;
        #endregion

        public bool AutoApproval { get; private set; }

        public string ArchiveTemp { get; set; }

        public BackgroundSettings BackgroundSettings { get; private set; }

        public string ScanDBName { get; set; }

        public bool IsDiscoverOptimizationPreScan = false;
        public bool IsProcessDuplicateDatas = false;
        public Dictionary<string, RMDiscoveryOffice365RuleInfo> DiscoveryO365RuleInfoCache = new Dictionary<string, RMDiscoveryOffice365RuleInfo>();
        public bool EnableDeleteDocumentBatchOptimization { get; private set; }
        public int DeleteDocumentBatchOptimizationBatchSize { get; private set; } = 50;
        
        public string O365TenantId { get; set; }

        public string TenantGroupId { get; private set; }   //SAAS-10617 Support Site Collection rule时，archiver SiteCollection之后，要更新DB需要用到该属性。

        public bool UseListLevelBCSColumn = false;
        public bool IsUseSPQueryOneByOne = false;
        public SPType sharePointType;

        public bool IsOneDriverSite = false;
        public bool IsTeams = false;

        public bool NeedDeleteSCPermanently()
        {
            if (currentRule == null) return false;
            var sourceFlag = IsTeams ? SourceFlag.Teams : IsOneDriverSite ? SourceFlag.OneDrive : SourceFlag.SharePoint;
            var isDeleteSCPermanently = currentRule.IsDeleteSiteCollectionPermanently((int)sourceFlag);
            mLog.Info($"isDeleteSCPermanently: {isDeleteSCPermanently}, sourceFlag: {sourceFlag}, ruleId: {currentRule.Id}, rulelevel: {currentRule.PolicyLevel}, ruleKeepDataOption: {currentRule.KeepDataOption}");
            return isDeleteSCPermanently;
        }

        public GCommon.Contract.Tree.Object.NodeType TeamsSiteNodeType = GCommon.Contract.Tree.Object.NodeType.TeamChannel;
        
        private TeamsChannelType? _teamsChannelType;
        private TeamsChannelType TeamsSiteChannelType
        {
            get
            {
                if (!IsTeams) return TeamsChannelType.None;
                if (_teamsChannelType.HasValue) return _teamsChannelType.Value;

                _teamsChannelType = TeamsSiteNodeType switch
                {
                    GCommon.Contract.Tree.Object.NodeType.TeamPrivateChannel => TeamsChannelType.Private,
                    GCommon.Contract.Tree.Object.NodeType.TeamSharedChannel => TeamsChannelType.Shared,
                    _ => TeamsChannelType.None,
                };

                return _teamsChannelType.Value;
            }
        }

        /// <summary>
        /// When true, scanners should prefer SharePoint change APIs (GetChange*) instead of full crawls.
        /// </summary>
        public bool UseIncrementalDiscover { get; set; } = false;
        public long IncrementalDiscoverStartTimeTicks { get; set; } = DateTime.MinValue.Ticks;
        public long IncrementalDiscoverEndTimeTicks { get; set; } = DateTime.MinValue.Ticks;

        private string _teamsAddress = string.Empty;
        public string TeamsAddress
        {
            get
            {
                lock (mLock)
                {
                    if (IsTeams == true)
                    {
                        if (string.IsNullOrEmpty(_teamsAddress) && !string.IsNullOrEmpty(SiteCollectionUrl))
                        {
                            try
                            {
                                var teamsNode = RMRemoteNodeDao.GetTeamsNodeBySiteUrl(SiteCollectionUrl);
                                if (teamsNode != null)
                                {
                                    _teamsAddress = teamsNode.Name;
                                    _teamsId = teamsNode.TeamId;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Error($"An error occured when retrieving team remote node by siteURL: {SiteCollectionUrl}. EX: {ex}");
                                _teamsAddress = string.Empty;
                            }
                        }

                        return _teamsAddress;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        private string? _generalRetentionLabel = null;

        public string GeneralRetentionLabel
        {
            get
            {
                lock (mLock)
                {
                    if(_generalRetentionLabel == null)
                    {
                        try
                        {
                            _generalRetentionLabel = GetGeneralRetentionLabel();
                        }
                        catch(Exception ex)
                        {
                            mLog.Error($"An error occured when GetGeneralRetentionLabel . Ex: {ex}");
                            _generalRetentionLabel = null;
                        }
                    }
                    return _generalRetentionLabel ?? string.Empty;
                }
            }
        }

        private string _teamsId = string.Empty;
        public string TeamsId
        {
            get
            {
                lock (mLock)
                {
                    if (IsTeams == true)
                    {
                        if (string.IsNullOrEmpty(_teamsId) && !string.IsNullOrEmpty(SiteCollectionUrl))
                        {
                            try
                            {
                                var teamsNode = RMRemoteNodeDao.GetTeamsNodeBySiteUrl(SiteCollectionUrl);
                                if (teamsNode != null)
                                {
                                    _teamsAddress = teamsNode.Name;
                                    _teamsId = teamsNode.TeamId;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Error($"An error occured when retrieving team remote node by siteURL: {SiteCollectionUrl}. EX: {ex}");
                                _teamsId = string.Empty;
                            }
                        }

                        return _teamsId;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            set
            {
                lock (mLock)
                {
                    _teamsId = value;
                }
            }
        }

        public bool IncludeMetadataService = false;

        public string ScopePath = string.Empty;

        public AveBPOSAccountInfo user;

        public IAveList folderParentList = null;

        public Stack<int> needDeleteFolder = new Stack<int>();

        public Guid tempListId = Guid.Empty;

        //Only for Record Manager Restore,Init it every RM rule
        public AveObjectModelFactory recordManagerRestoreOMFactory;

        public AppendItemMapping appendItemMapping = new AppendItemMapping();

        public string subFolderUrl = string.Empty;

        public string moveSourceSiteUrl = string.Empty;
        public string moveSourceFileUrl = string.Empty;

        public bool IsILMode = false;

        private Dictionary<string, RemoteSiteCollection> recordSites = new Dictionary<string, RemoteSiteCollection>();

        public ItemDependencyOption itemDependencyOption = ItemDependencyOption.Overwrite;

        public bool DestinationIsOneDriveSite = false;

        public string siteUrlSchemeAndHost = string.Empty;


        public Dictionary<string, Rule> VaultRulesCollection = new Dictionary<string, Rule>();


        private Dictionary<string, RemoteSiteCollection> remoteSites = new Dictionary<string, RemoteSiteCollection>(); //Key:SiteUrl.Value:RemoteSiteCollection.

        public ConcurrentDictionary<Guid, DestinationListTermSetting> DestinationListTermSettingCache = new ConcurrentDictionary<Guid, DestinationListTermSetting>();


        public ConcurrentDictionary<string, EXOMoveDestinationInfo> SiteBCSColumnDictionary = new ConcurrentDictionary<string, EXOMoveDestinationInfo>();


        #region for Deletion/KeepData
        public List<CacheItemDto> TasksCacheItemDtoCollection = new List<CacheItemDto>();
        public List<CacheItemDto> NeedDeleteTasksCacheItemDtoCollection = new List<CacheItemDto>();
        public string denyAddAndCustomizePagesStatus = string.Empty;
        public Dictionary<Guid, List<string>> cacheRecordAttachments = new Dictionary<Guid, List<string>>();
        public List<TagInfoCollection> tagInfoCollection = new List<TagInfoCollection>();
        public IAveSite DeletionIAveSite = null;
        public IAveWeb DeletionIAveWeb = null;
        public IAveList DeletionIAveList = null;
        private Wrapper.Backup.AveSPSite StubBackupAveSPSite = null;
        private AveSiteInfo AveSiteInfo = null;
        private Wrapper.Backup.AveSPWeb StubBackupAveSPWeb = null;
        private AveWebInfo AveWebInfo = null;
        private Wrapper.Backup.AveSPList StubBackupAveSPList = null;
        private AveListInfo AveListInfo = null;
        public Wrapper.Backup.AveSPFolder StubBackupAveSPRootFolder = null;
        public Wrapper.Backup.AveSPFolder StubOnlyBackupAveSPCurrentFolder = null;
        private Wrapper.Restore.AveSPSite StubRestoreAveSPSite = null;
        private Wrapper.Restore.AveSPWeb StubRestoreAveSPWeb = null;
        private Wrapper.Restore.AveSPList StubRestoreAveSPList = null;
        public Wrapper.Restore.AveSPFolder StubRestoreAveSPRootFolder = null;
        public Wrapper.Restore.AveSPFolder StubOnlyRestoreAveSPCurrentFolder = null;
        public bool HasActiveInPlaceRecordManagementFeature = false;
        private List<Guid> needDeleteFolderCache = new List<Guid>();
        private IAveORecords records;
        public List<AveUserInfo> StubUserInfos = new List<AveUserInfo>();
        public List<AveGroupInfo> StubGroupInfos = new List<AveGroupInfo>();

        #endregion

        private bool SetRecordSetting = false;
        public Dictionary<string, Guid> RetentionLabel = new Dictionary<string, Guid>();
        public Dictionary<string, AveComplianceTagInfo> SharePointRetentionLabel = null;
        private string timeZone = string.Empty;
        public string CurrentIndexJobID = string.Empty;

        private readonly object mSenderLock = new object();
        private IArchiverBackupDataWriter fileSender = null;
        private string currentfileSenderJobId = string.Empty;
        public Dictionary<string, ArchiverBackupRequest> CachedBackupJob = new Dictionary<string, ArchiverBackupRequest>();


        public List<Record> exploreDBSPRecords = new List<Record>();
        public List<string> FailedVersionFileIds = new List<string>();

        public Dictionary<string, BackupIAveListItemCacheDto> ArchiverBackupCacheItems = new Dictionary<string, BackupIAveListItemCacheDto>();
        //key:folderID/SiteID value:folder/Site CacheNodeType
        public Dictionary<Guid, int> ObjectCache = new Dictionary<Guid, int>();
        public bool IsCalculateCRC = false;

        #region relative data
        public string relativeDataTreeNodeString = string.Empty;
        public bool IsRelativeDataJob = false;
        public int RelativeDataJobSourceFlag = 0;
        #endregion

        #region manual
        private ArchiverManualAction mManualAction;

        private ArchiverManualAction ManualAction
        {
            get
            {
                if (mManualAction == null)
                {
                    if (IsOneDriverSite)
                    {
                        mManualAction = new OneDriveArchiverManualAction(MainJobId, this.ContainerId, this.AveSiteId);
                    }
                    else if (IsTeams)
                    {
                        mManualAction = new TeamsArchiverManualAction(MainJobId, this.ContainerId, this.TeamsId, this.AveSiteId);
                    }
                    else
                    {
                        mManualAction = new SharePointOnlineArchiverManualAction(MainJobId, this.ContainerId, this.AveSiteId);
                    }
                }
                return mManualAction;
            }
        }

        public List<AADAccount> ManualSiteOwners { get; set; }
        public bool AutoApprovalManualRule = false;
        #endregion

        #region will delete later
        //public SOArchiverAzureDBWorker soArchiverQueryWorker = null;
        //public SOArchiverAzureDBWorker soArchiverQueryWorkerForDel = null;
        //public SOArchiverAzureDBWorker soArchiverQueryWorkerForJob = null;
        #endregion

        public List<RMDiscoveryOffice365RuleInfo> ROTDiscoveryRuleInfos;
        public List<RMDiscoveryAOSPRuleInfo> ROTDiscoveryAOSPRuleInfos;
        public List<RMDiscoveryOffice365RuleInfo> InactiveDiscoveryRuleInfos;
        public List<RMDiscoveryAOSPRuleInfo> InactiveDiscoveryAOSPRuleInfos;
        public List<RMDiscoveryOffice365RuleInfo> InactiveAndRotVerisonRuleInfos = new List<RMDiscoveryOffice365RuleInfo>();
        public List<RMDiscoveryAOSPRuleInfo> InactiveAndRotVerisonAOSPRuleInfos = new List<RMDiscoveryAOSPRuleInfo>();
        public RMDiscoveryOffice365OptimizationSetting RMDiscoveryOptimizationSetting;
        public RMDiscoveryAOSPOptimizationSetting RMDiscoveryAOSPOptimizationSetting;
        public bool IsDiscoverOptimization = false;
        public bool UseArchiverImportFile = false;

        // for export VEO file action
        public bool IsUpgradedVEOV3 = false;

        // for convert stub job
        public bool IsConvertStubJob = false;
        public bool isConvertSameTypeStub = false;
        public LeaveStubType NeedConvertStubType;
        public Dictionary<string, string> RuleNameByJobIdDic = [];
        public Dictionary<string, StubFileDto> StubCache = []; // stub file uniqueIdStr, StubFileDto
        public bool LibraryHasStubHiddenColumn { get; set; }
        public bool Skip0KBFile { get; set; }
        public bool SupportLockedSite { get; set; }
        public bool SupportArchivedTeams { get; set; }
        public bool SkipCheckManualWhenObjectNotMatchRule { get; set; }

        // Teams
        public string ForceFitTeamsRuleID = null;
        public Office365AlertUtil mOffice365AlertUtil = null;

        public void EnsureBlockEditAndDelete(IAveSite site)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.EnsureBlockEditAndDelete"))
            {
                lock (mLock)
                {
                    mLog.Info($"setRecordSetting status is : {this.SetRecordSetting.ToString()}.");
                    if (!this.SetRecordSetting)
                    {
                        var setting = ArchiverCommonStaticMethod.GetRecordRestrictions(site);
                        if (!string.IsNullOrEmpty(setting))
                        {
                            mLog.Info($"Current record setting is : {setting}.");
                            var recordRestrictions = (RecordRestrictions)Enum.Parse(typeof(RecordRestrictions), setting);
                            var flag = RecordRestrictions.BlockDelete | RecordRestrictions.BlockEdit;
                            //当前setting 不是block edit and delete 的情况下， 变成block edit and delete
                            if ((recordRestrictions & flag) != flag)
                            {
                                ArchiverCommonStaticMethod.SetBlockEditAndDelete(site);
                                mLog.Info($"Set record setting to block edit and delete.");
                            }
                        }
                        else
                        {
                            mLog.Info($"Current record setting is null.");
                            ArchiverCommonStaticMethod.SetBlockEditAndDelete(site);
                        }
                        mLog.Info("Set setRecordSetting to true.");
                        this.SetRecordSetting = true;
                    }
                }
            }
        }
        #region for Move/Leave stub Action
        public bool isRestoreXml;
        #endregion

        private IExplorerDao _explorerDao;
        public IExplorerDao? ExplorerDao
        {
            get
            {
                try
                {
                    if (_explorerDao == null)
                    {
                        if (!string.IsNullOrEmpty(DBInfoDao.GetDBNameByTenantId(TenantLocalValue.LogonGroupId)))
                        {
                            _explorerDao = new ExplorerDao();
                        }
                    }
                    return _explorerDao;
                }
                catch (Exception e)
                {
                    mLog.Warn($"Get explorer dao error:{e}");
                    return _explorerDao;
                }
            }
        }

        private string recenterURL;
        public string ReCenterURL
        {
            get
            {
                lock (mLock)
                {
                    if (string.IsNullOrEmpty(recenterURL))
                    {
                        recenterURL = ArchiverCommonStaticMethod.GetReCenterHost(TenantGroupId);
                        mLog.Info($"Success get ReCenter URL:{recenterURL}.");
                        return recenterURL;
                    }
                    else
                    {
                        return recenterURL;
                    }
                }
            }
        }
        public bool IsSupportRecordLabel;

        public void Init(bool isMainJobId = false)
        {
            IJobMonitorDao JobMonitorDao = new JobMonitorDao();
            //从子job的Context中获取当前需要处理的节点.
            IRMSubJobDao SubJobDao = new RMSubJobDao();
            RMJobMonitor mainJob = null;
            if (isMainJobId)
            {
                mainJob = JobMonitorDao.GetJob(JobId);
            }
            else
            {
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(JobId, true);
                MainJobId = subJobWithContext.ParentId;
                mainJob = JobMonitorDao.GetJob(subJobWithContext.ParentId);
                this.ScopePath = subJobWithContext.String1;
            }

            IsCalculateCRC = IsArchiverCalculateCRC();

            BackgroundSettings = BackgroundSettings.GetInstance();
            WrapperConfiguration.RecordsOutputStreamLevel = (int)BackgroundSettings.RecordsOutputStreamLevel;
            WrapperConfiguration.ArchiverOutputStreamLevel = (int)BackgroundSettings.ArchiverOutputStreamLevel;
            ArchiveTemp = BackgroundSettings.ArchiveTemp;
            if (!System.IO.Directory.Exists(ArchiveTemp))
            {
                Directory.CreateDirectory(ArchiveTemp);
            }
            ArchiverUNCTime = DateTime.UtcNow;
            ScanDBName = string.Format("scan.{0}.db", Guid.NewGuid().ToString());
            mLog.Info($"Temp path :{ArchiveTemp}, ScanDBName:{ScanDBName}");
            #region wrapper config
            WrapperConfiguration.WrapperConfigurationForBPOS.LoadRootFolderUniqueId = true;
            WrapperConfiguration.WrapperConfigurationForBPOS.SetUserAgent(Office365UserAgentGenerator.Create(ModuleUserAgent.Archive, false));
            #endregion

            #region tag
            tagInfoCollection.Add(new TagInfoCollection() { Key = "ArchiveTime", Value = DateTime.UtcNow });

            tagInfoCollection.Add(new TagInfoCollection() { Key = "ArchiveBy", Value = string.Format("{0}", mainJob?.UserName) });
            #endregion
            InitRMKeyValueSettings();
        }

        private void InitRMKeyValueSettings()
        {
            try
            {
                var setting = RMKeyValueDao.GetValueByKey("Skip0KBFile");
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (bool.TryParse(setting.Value, out var enable))
                    {
                        Skip0KBFile = enable;
                        mLog.Info($"InitRMKeyValueSettings Skip0KBFile{Skip0KBFile}.");
                    }
                }

                //SkipCheckManualWhenObjectNotMatchRule
                var SkipCheckManualWhenObjectNotMatchRuleSetting = RMKeyValueDao.GetValueByKey("SkipCheckManualWhenObjectNotMatchRule");
                if (SkipCheckManualWhenObjectNotMatchRuleSetting != null && !string.IsNullOrWhiteSpace(SkipCheckManualWhenObjectNotMatchRuleSetting.Value))
                {
                    if (bool.TryParse(SkipCheckManualWhenObjectNotMatchRuleSetting.Value, out var enableSkipCheckManualWhenObjectNotMatchRuleSetting))
                    {
                        SkipCheckManualWhenObjectNotMatchRule = enableSkipCheckManualWhenObjectNotMatchRuleSetting;
                        mLog.Info($"InitRMKeyValueSettings SkipCheckManualWhenObjectNotMatchRule:{SkipCheckManualWhenObjectNotMatchRule}.");
                    }
                }

                //QUERY_VALUES_LIMITE_FILE
                var QUERY_VALUES_LIMITE_FILE = RMKeyValueDao.GetValueByKey("QUERY_VALUES_LIMITE_FILE");
                if (QUERY_VALUES_LIMITE_FILE != null && !string.IsNullOrWhiteSpace(QUERY_VALUES_LIMITE_FILE.Value))
                {
                    if (Int32.TryParse(QUERY_VALUES_LIMITE_FILE.Value, out var QUERY_VALUES_LIMITE_FILE_Count))
                    {
                        WrapperConfiguration.WrapperConfigurationForBPOS.QUERY_VALUES_LIMITE_FILE = QUERY_VALUES_LIMITE_FILE_Count;
                        mLog.Info($"InitRMKeyValueSettings QUERY_VALUES_LIMITE_FILE:{QUERY_VALUES_LIMITE_FILE_Count}.");
                    }
                }

                var deleteDocumentBatchOptimizationSetting = RMKeyValueDao.GetValueByKey("EnableDeleteDocumentBatchOptimization");
                if (deleteDocumentBatchOptimizationSetting != null && !string.IsNullOrWhiteSpace(deleteDocumentBatchOptimizationSetting.Value))
                {
                    if (bool.TryParse(deleteDocumentBatchOptimizationSetting.Value, out var enableDeleteDocumentBatchOptimization))
                    {
                        EnableDeleteDocumentBatchOptimization = enableDeleteDocumentBatchOptimization;
                        mLog.Info($"InitRMKeyValueSettings EnableDeleteDocumentBatchOptimization:{EnableDeleteDocumentBatchOptimization}.");
                    }
                }

                var deleteDocumentBatchOptimizationBatchSizeSetting = RMKeyValueDao.GetValueByKey("DeleteDocumentBatchOptimizationBatchSize");
                if (deleteDocumentBatchOptimizationBatchSizeSetting != null && !string.IsNullOrWhiteSpace(deleteDocumentBatchOptimizationBatchSizeSetting.Value))
                {
                    if (Int32.TryParse(deleteDocumentBatchOptimizationBatchSizeSetting.Value, out var deleteDocumentBatchOptimizationBatchSize)
                        && deleteDocumentBatchOptimizationBatchSize > 0)
                    {
                        DeleteDocumentBatchOptimizationBatchSize = deleteDocumentBatchOptimizationBatchSize;
                        mLog.Info($"InitRMKeyValueSettings DeleteDocumentBatchOptimizationBatchSize:{DeleteDocumentBatchOptimizationBatchSize}.");
                    }
                }

                IsSupportRecordLabel = AccountUtility.IsSupportRecordLabel();
                
            }
            catch (Exception e)
            {
                mLog.Error($"An error occurred while InitRMKeyValueSettings. Error: {e}");
            }
        }
        public void InitOffice365AlertUtil()
        {
            mOffice365AlertUtil = new Office365AlertUtil(this);
        }

        private bool IsArchiverCalculateCRC()
        {
            SettingProfileDto mDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.ArchiverIsCalculateCRC,
                Name = "ArchiverIsCalculateCRC"
            };
            var dto = SettingProfileDao.Load(mDto);
            return bool.TryParse(dto?.Settings, out var calcCRC) ? calcCRC : false;
        }

        private string GetGeneralRetentionLabel()
        {
            SettingProfileDto profileDto = new SettingProfileDto
            {
                Type = (int)SettingProfilesType.RecordsLabelSetting,
                Name = "RecordsLabelSetting"
            };
            var dto = SettingProfileDao.Load(profileDto);
            return dto?.Settings ?? string.Empty;
        }

        public void GetCacheDataForRecords()
        {
            if (ExplorerDao != null)
            {
                exploreDBSPRecords = ExplorerDao.QueryByPage(
                                    s => (s.SourceFlag == (int)SourceFlag.SharePoint || s.SourceFlag == (int)SourceFlag.OneDrive || s.SourceFlag == (int)SourceFlag.Teams)
                                    && s.ScopeId == SiteCollectionID
                                    && (s.RecordStatus == 1 || s.RecordStatus == 4 || s.RecordStatus == 5 || s.RecordStatus == 9)
                                    , 10000).Item1.ToList();
            }
            else
            {
                mLog.Warn($"CosmosDB has not been initialized, this job will skip updating the logic of CosmosDB");
            }
        }

        public Task<Record> ProcessWaitingForApprovalRecordAsync(Record rec)
        {
            return this.ManualAction.ProcessWaitingForApprovalRecordAsync(rec, this.ManualSiteOwners);
        }
        public Record SetRecordIsAutoApproval(Record rec)
        {
            return this.ManualAction.SetRecordIsAutoApproval(rec);
        }
        public Record ProcessApprovedOrRejectedRecord(Record rec)
        {
            return this.ManualAction.ProcessApprovedOrRejectedRecord(rec);
        }

        public string TimeZone
        {
            get
            {
                return timeZone;
            }
            set
            {
                timeZone = value;
            }
        }
        public static bool CheckisRecord(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckisRecord"))
            {
                bool isRecord = false;
                int result = 0;
                try
                {
                    mLog.Info("start to check is record.");
                    object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                    if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                }
                catch (Exception ex)
                {
                    //mLog.Info(ex.ToString());
                    mLog.Error($"failed to check is record.reason:{ex}");
                    result = 0;
                }
                if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
                {
                    isRecord = true;
                }
                return isRecord;
            }
        }
        public static bool CheckIsHoldOnly(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckIsHoldOnly"))
            {
                bool isHoldOnly = false;
                int result = 0;
                try
                {
                    object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                    if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
                }
                catch (Exception ex)
                {
                    mLog.Info("This Item is not On Hold " + ex.Message);
                    result = 0;
                }
                if (((result & 0x1000) != 0 || (result & 1) != 0 || (result & 0x100) != 0) && !((result & 0x10) != 0))
                {
                    //进入这里说明 次Item 仅仅是Hold 的而不是Declare 的
                    isHoldOnly = true;
                }
                return isHoldOnly;
            }
        }
        public static Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return new Guid(HashCodeHelper.ToMD5HashCode(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()));
        }

        public (bool HasSetting, bool IsInheritParentTerm) TryGetIsEnableInheritTerm(Guid scopeId, NodeLevel nodeLevel, RMSPTreeNode? processingNode = null)
        {
            if (!IsILMode)
            {
                mLog.Warn("Only IL mode supports inheriting terms.");
                return (false, false);
            }
            //mLog.Info($"TryGetIsEnableInheritTerm for node {scopeId}, Level: {nodeLevel}");

            var siteId = string.IsNullOrEmpty(AveSiteId) ? Guid.Empty : new Guid(AveSiteId);
            if (IsTeams)
            {
                //mLog.Info($"Get Teams setting for node {scopeId}");
                var teamsId = string.IsNullOrEmpty(TeamsId) ? Guid.Empty : new Guid(TeamsId);
                RMTeamsSetting? teamsSetting = null;
                
                if (nodeLevel != NodeLevel.Folder)
                {
                    teamsSetting = TeamsSettingDao.GetSettingInfoByScope(ContainerId, teamsId, siteId, scopeId);
                }

                if (teamsSetting == null && processingNode != null)
                {
                    teamsSetting = TeamsSettingDao.LoadClosestContainerSetting(processingNode, ContainerId, teamsId, siteId);
                    mLog.Info($"{scopeId} not found unique setting. Load closest parent Teams setting, Scope: {teamsSetting?.ScopeId}, fullPath: {teamsSetting?.FullPath}");
                }

                if (teamsSetting == null) return (false, false);
                return (true, teamsSetting.isEnableClassification && teamsSetting.IsInheritParentTerm);
            }
            else
            {
                RMSharePointSetting? spSetting = null;
                if (nodeLevel != NodeLevel.Folder)
                {
                    spSetting = SharePointSettingDao.GetSettingInfoByScope(ContainerId, siteId, scopeId);
                }

                if (spSetting == null && processingNode != null)
                {
                    spSetting = SharePointSettingDao.LoadClosestContainerSetting(processingNode, ContainerId, siteId);
                    mLog.Info($"{scopeId} not found unique setting. Load closest parent SPO setting: Scope: {spSetting?.ScopeId}, fullPath: {spSetting?.FullPath}");
                }

                if (spSetting == null) return (false, false);
                return (true, spSetting.isEnableClassification && spSetting.IsInheritParentTerm);
            }
        }

        public bool IsLifecycleManagementEnabledForList(Guid listId, Guid webId)
        {
            var siteId = SiteCollectionID;
            if (siteId == Guid.Empty)
            {
                mLog.Warn($"Unable to resolve SharePoint site scope for lifecycle setting. SiteCollectionID: {SiteCollectionID}");
                return true;
            }

            var scopeIds = new[] { listId, webId, siteId, ContainerId };

            foreach (var scopeId in scopeIds)
            {
                string nodeInfo;
                if (IsTeams)
                {
                    if (!Guid.TryParse(TeamsId, out var teamsId))
                    {
                        mLog.Warn($"Unable to resolve Teams scope for lifecycle setting. TeamsId: {TeamsId}");
                        return true;
                    }

                    var settingTeamsId = scopeId == ContainerId ? Guid.Empty : teamsId;
                    var settingSiteId = scopeId == ContainerId ? Guid.Empty : siteId;
                    nodeInfo = TeamsSettingDao.GetSettingInfoByScope(
                        ContainerId,
                        settingTeamsId,
                        settingSiteId,
                        scopeId)?.NodeInfo;
                }
                else
                {
                    nodeInfo = SharePointSettingDao.GetSettingInfoByScope(
                        ContainerId,
                        scopeId == ContainerId ? Guid.Empty : siteId,
                        scopeId)?.NodeInfo;
                }

                var node = string.IsNullOrWhiteSpace(nodeInfo)
                    ? null
                    : SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(nodeInfo);
                if (node?.EnableLifecycleManagementForSharePointLists.HasValue == true)
                {
                    return node.EnableLifecycleManagementForSharePointLists.Value;
                }
            }

            mLog.Info($"No explicit lifecycle setting found for list {listId}. Keep list processing enabled.");
            return true;
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByRecords(string siteUrl)
        {
            if (recordSites.ContainsKey(siteUrl))
            {
                return recordSites[siteUrl];
            }
            else
            {
                RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                //JobReportServiceFactory.CreateArchiverJobManagementService().GetRemoteSiteCollection(this.archiverMessage.TenantGroupId, siteUrl);
                if (remoteSiteCollection != null && !recordSites.ContainsKey(siteUrl))
                {
                    recordSites.Add(siteUrl, remoteSiteCollection);
                    return remoteSiteCollection;
                }
                else
                {
                    mLog.Info($"GetRemoteSiteCollection sc info is null. Url:{siteUrl}");
                    return null;
                }
            }
        }
        /// <summary>
        /// 此方法返回True 时，表示是Block Edit and Delete 类型的Declare, 但是返回false 的时候，不代表不是declare 文件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsBlockEditAndDeleteRecord(IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public static bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }
        public void InitRetentionLabelCollections(IAveSite site)
        {
            //SharePointRetentionLabel = new Dictionary<string, AveComplianceTagInfo>();
            if (SharePointRetentionLabel == null)
            {
                lock (mSPLabelLock)
                {
                    if (SharePointRetentionLabel == null)
                    {
                        var availableTags = site.GetAvailableTagsForSite();
                        SharePointRetentionLabel = availableTags.ToDictionary(r => r.TagName);
                    }
                }
            }
        }

        public void AddDeleteFolderCache(Guid folderId)
        {
            lock (mLock)
            {
                if (!needDeleteFolderCache.Contains(folderId))
                {
                    needDeleteFolderCache.Add(folderId);
                }
            }
        }

        public void RemoveDeleteFolderCache(Guid folderId)
        {
            lock (mLock)
            {
                needDeleteFolderCache.Remove(folderId);
            }
        }

        public List<Guid> GetAllDeleteFolderCacheDto()
        {
            lock (mLock)
            {
                return needDeleteFolderCache;
            }
        }

        public IAveORecords GetStubIAveORecords()
        {
            if (records == null)
            {
                records = this.aveObjectModelFactory.CreateRecords();
            }
            return records;
        }

        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetHoldAndRecordStatus"))
            {
                int result = 0;
                try
                {
                    if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                    {
                        try
                        {
                            if (item.Fields.Contains(HoldRecordStatus))
                            {
                                object obj2 = item[HoldRecordStatus];
                                if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                                {
                                    result = 0;
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                            result = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
                }
                return result;
            }
        }

        private static bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetBoolIprPropertyCore"))
            {
                bool? nullable = null;
                if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
                {
                    object obj = list.RootFolder.Properties[propName];
                    if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
                }
                return (nullable == true);
            }
        }
        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.IsHoldOrRecordsEnabled"))
            {
                if (list == null || list.Fields == null)
                {
                    throw new ArgumentNullException("list");
                }
                if (list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
                {
                    return (list.Fields[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")] != null);
                }
                else
                {
                    return false;
                }
            }
        }

        public string GetConvertingStubFullUrl(string serverRelativeUrl, bool isNewStub = false, bool hasStubType = true)
        {
            var relativeUrl = serverRelativeUrl;
            if (isConvertSameTypeStub)
            {
                relativeUrl = serverRelativeUrl.Split('/').Last().StartsWith(JobId + "_") ? serverRelativeUrl.Replace(JobId + "_", ""): serverRelativeUrl;
            }

            var stubType = hasStubType ? 
                    isNewStub 
                    ? LinkFileCommon.GetStubFileNameSuffixWithDot(this) 
                    : LinkFileCommon.GetStubFileNameSuffixWithDot(NeedConvertStubType) 
                : "";

            return siteUrlSchemeAndHost + relativeUrl + stubType;
        }

        private IRMStubFileRecordDao StubFileRecordDao => PlatformWindsorManager.GetService<IRMStubFileRecordDao>();
        
        private IRMSiteStubSettingMappingDao SiteStubSettingMappingDao => PlatformWindsorManager.GetService<IRMSiteStubSettingMappingDao>();

        private IRMStubDisposalSiteInfoDao StubDisposalSiteInfoDao => PlatformWindsorManager.GetService<IRMStubDisposalSiteInfoDao>();

        private bool _isBatchingForStubFileRecords = true;

        private Dictionary<Guid, RMStubFileRecordTableEntity> _stubFileRecords = [];

        private Dictionary<Guid, RMSiteStubSettingMapping> _siteStubSettingMappingCache;

        private double? StubRetentionTime4TestValue;

        public void AddStubFileRecord(StubFileRecordDto dto)
        {
            if (currentRule == null)
            {
                return;
            }

            lock (mLock)
            {
                
                if (StubRetentionTime4TestValue == null)
                {
                    try
                    {
                        var time = RMKeyValueDao.GetValueByKey("StubRetentionTime4Test");
                        StubRetentionTime4TestValue = long.Parse(time.Value);
                        mLog.Info($"Get StubRetentionTime4Test from RMKeyValue, value: {StubRetentionTime4TestValue}.");
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"An error occurred while getting StubRetentionTime4Test from RMKeyValue. Error: {e}.");
                        StubRetentionTime4TestValue = 0;
                    }
                    
                }
                if (StubRetentionTime4TestValue.HasValue && StubRetentionTime4TestValue != 0)
                {
                    dto.RefDateTime = dto.RefDateTime.AddDays(StubRetentionTime4TestValue.Value);
                }
            }
            if (IsConvertStubJob)
            {
                dto.ArchivedFileFullPath = GetConvertingStubFullUrl(dto.ArchivedFileFullPath, hasStubType: false);
                if (StubCache.TryGetValue(dto.ArchivedItemId.ToString(), out var result))
                {
                    dto.ArchivedItemId = new Guid(result.BackupFileId);
                }
                
            }
            var createdTime = dto.RefDateTime.Ticks;
            var rowKey = $"{dto.RefDateTime:yyyyMMddHHmmss}_{dto.ArchivedItemId:N}"; // temporary use archived item unique id
            var entity = new RMStubFileRecordTableEntity(SiteCollectionID.ToString(), rowKey) // temporary use site collection id
            {
                StubTemplateId = currentRule.StubTemplateId,
                StubId = dto.StubId,
                StubType = (int)currentRule.LeaveStubType,
                ArchivedFileFullPath = dto.ArchivedFileFullPath.StartsWith(SiteCollectionUrl) 
                    ? dto.ArchivedFileFullPath 
                    : WebUtil.MakeFullUrl(SiteCollectionUrl, dto.ArchivedFileFullPath),
                StubCreatedTime = createdTime,
                ListId = dto.ListId,
                WebId = dto.WebId,
                RecordType = 0
            };

            if (!Guid.TryParse(currentRule.StubTemplateId, out Guid templateId))
            {
                mLog.Warn($"Invalid StubTemplateId: {currentRule.StubTemplateId}");
                return;
            }

            if (_siteStubSettingMappingCache == null)
            {
                lock (mLock)
                {
                    _siteStubSettingMappingCache ??= SiteStubSettingMappingDao.GetAllMappingsBySiteUrlAsync(SiteCollectionUrl)
                            .GetAwaiter().GetResult()?.ToDictionary(m => m.StubTemplateId) ?? [];
                }

                mLog.Info($"Loaded site stub setting mapping for site {SiteCollectionUrl}, count: {_siteStubSettingMappingCache.Count}");
            }

            lock (mLock)
            {
                if (!_siteStubSettingMappingCache.TryGetValue(templateId, out var mapping))
                {
                    mapping = new RMSiteStubSettingMapping()
                    {
                        SiteCollectionUrl = SiteCollectionUrl,
                        StubTemplateId = templateId,
                        FirstStubCreatedTime = createdTime,
                        LastStubCreatedTime = createdTime,
                    };

                    if (currentRule.LeaveStubIsEnabledRetention)
                    {
                        mapping.IsEnabledRetention = true;
                        mapping.RetentionValue = currentRule.LeaveStubRetentionValue;
                        mapping.RetentionUnit = currentRule.LeaveStubRetentionUnit;
                    }

                    _siteStubSettingMappingCache[mapping.StubTemplateId] = mapping;

                    mLog.Info($"Created new mapping for StubTemplateId: {mapping.StubTemplateId}, FirstStubCreatedTime: {mapping.FirstStubCreatedTime}, IsEnabledRetention: {mapping.IsEnabledRetention}, Retention: {mapping.RetentionValue} {mapping.RetentionUnit}");
                }
                else
                {
                    if (createdTime > mapping.LastStubCreatedTime)
                    {
                        mapping.LastStubCreatedTime = createdTime;
                    }

                    if (mapping.FirstStubCreatedTime == 0 || createdTime < mapping.FirstStubCreatedTime)
                    {
                        mapping.FirstStubCreatedTime = createdTime;
                    }

                    mLog.Info($"Updated mapping for StubTemplateId: {mapping.StubTemplateId}, FirstStubCreatedTime: {mapping.FirstStubCreatedTime}, LastStubCreatedTime: {mapping.LastStubCreatedTime}");
                }
            }

            if (_isBatchingForStubFileRecords)
            {
                List<RMStubFileRecordTableEntity>? temp = null;
                lock (mLock)
                {
                    _stubFileRecords[dto.ArchivedItemId] = entity;
                    if (_stubFileRecords.Count >= 100)
                    {
                        temp = _stubFileRecords.Values.ToList();
                        _stubFileRecords = [];
                    }
                }

                if (temp != null)
                {
                    mLog.Info($"Flushing stub file records in batch, count: {temp.Count}");
                    StubFileRecordDao.FlushDeleteCache(TenantGroupId);
                    StubFileRecordDao.AddStubFileRecordEntities(TenantGroupId, temp);
                }
            }
            else
            {
                StubFileRecordDao.AddStubFileRecordEntity(TenantGroupId, entity);
            }
        }

        public async Task FlushStubFileRecords()
        {
            if (currentRule == null)
            {
                mLog.Warn("Current rule is null.");
                //return;
            }

            long minRetentionTime = 0;
            if (_siteStubSettingMappingCache != null)
            {
                foreach (var mapping in _siteStubSettingMappingCache.Values)
                {
                    mLog.Info($"FlushStubFileRecords, processing mapping for StubTemplateId: {mapping.StubTemplateId}, FirstStubCreatedTime: {mapping.FirstStubCreatedTime}, IsEnabledRetention: {mapping.IsEnabledRetention}, Retention: {mapping.RetentionValue} {mapping.RetentionUnit}");

                    SiteStubSettingMappingDao.AddOrUpdateMappingAsync(mapping).GetAwaiter().GetResult();

                    if (!mapping.IsEnabledRetention)
                    {
                        mLog.Info($"Retention is not enabled for stub template id: {mapping.StubTemplateId}");
                        continue;
                    }

                    var firstStubRetentionTime = CalculateStubRetentionTime(mapping.FirstStubCreatedTime, mapping.RetentionValue, mapping.RetentionUnit);
                    if (minRetentionTime == 0 || firstStubRetentionTime < minRetentionTime)
                    {
                        minRetentionTime = firstStubRetentionTime;
                    }
                }

                var siteStubDisposalInfo = await StubDisposalSiteInfoDao.GetStubDisposalSiteInfoBySiteUrlAsync(SiteCollectionUrl);
                var needUpSert = minRetentionTime > 0;
                if (siteStubDisposalInfo == null)
                {
                    siteStubDisposalInfo = new RMStubDisposalSiteInfo()
                    {
                        Id = SiteCollectionID,
                        SiteCollectionUrl = SiteCollectionUrl,
                        MinRetentionTime = minRetentionTime
                    };

                    mLog.Info($"No existing stub disposal info for site {SiteCollectionUrl}, will insert new one with minRetentionTime: {minRetentionTime}");
                }
                else if (minRetentionTime < siteStubDisposalInfo.MinRetentionTime)
                {
                    siteStubDisposalInfo.MinRetentionTime = minRetentionTime;

                    mLog.Info($"Existing stub disposal info found for site {SiteCollectionUrl}, will update minRetentionTime from {siteStubDisposalInfo.MinRetentionTime} to {minRetentionTime}");
                }
                else
                {
                    needUpSert = false;
                    mLog.Info($"No need to update stub disposal info for site {SiteCollectionUrl}, current minRetentionTime {minRetentionTime} >= existing one {siteStubDisposalInfo.MinRetentionTime}");
                }

                if (needUpSert)
                {
                    await StubDisposalSiteInfoDao.AddOrUpdateStubDisposalSiteInfoAsync(siteStubDisposalInfo);
                    mLog.Info($"Upserted stub disposal info for site {SiteCollectionUrl}, minRetentionTime: {minRetentionTime}");
                }
            }

            StubFileRecordDao.FlushDeleteCache(TenantGroupId);
            if (_isBatchingForStubFileRecords && _stubFileRecords.Count > 0)
            {
                mLog.Info($"Flushing remaining stub file records, count: {_stubFileRecords.Count}");
                StubFileRecordDao.AddStubFileRecordEntities(TenantGroupId, _stubFileRecords.Values.ToList());
                _stubFileRecords = [];
            }

            _siteStubSettingMappingCache = [];
        }

        public void DeleteStubFileRecordEntitiesInBatch(string nodeGuid)
        {
            if (!IsConvertStubJob && Guid.TryParse(nodeGuid, out var archivedItemId))
            {
                lock (mLock)
                {
                    if (_stubFileRecords.ContainsKey(archivedItemId))
                    {
                        mLog.Info($"Removing stub file record for archived item id {archivedItemId} from batch cache.");
                        _stubFileRecords.Remove(archivedItemId);
                        return;
                    }
                }
            }

            if (IsConvertStubJob && StubCache.TryGetValue(nodeGuid, out var result))
            {
                nodeGuid = result.BackupFileId;
            }

            LinkFileCommon.DeleteStubFileRecord(SiteCollectionID.ToString(), nodeGuid);
        }

        private long CalculateStubRetentionTime(long stubCreatedTime, int retentionValue, DateUnit retentionUnit)
        {
            var createdUtc = new DateTime(stubCreatedTime, DateTimeKind.Utc);
            DateTime retentionDate;
            try
            {
                retentionDate = retentionUnit switch
                {
                    DateUnit.Day => createdUtc.AddDays(retentionValue),
                    DateUnit.Week => createdUtc.AddDays(7 * retentionValue),
                    DateUnit.Month => createdUtc.AddMonths(retentionValue),
                    DateUnit.Year => createdUtc.AddYears(retentionValue),
                    _ => throw new ArgumentOutOfRangeException(nameof(retentionUnit))
                };
            }
            catch(ArgumentOutOfRangeException ex)
            {
                mLog.Warn($"OutOfRange ex. StubCreatedTime: {createdUtc}, RetentionValue: {retentionValue}, RetentionUnit: {retentionUnit}. Error: {ex}");
                retentionDate = DateTime.MaxValue;
            }
            catch (Exception e)
            {
                mLog.Warn($"An error occurred while calculating stub retention time. StubCreatedTime: {createdUtc}, RetentionValue: {retentionValue}, RetentionUnit: {retentionUnit}. Error: {e}");
                retentionDate = createdUtc;
            }

            return retentionDate.Ticks;
        }

        // may take some time for the site to change lockState after unarchive Teams, so may need retry
        public bool CheckSiteLockState(SiteState checkingState, int retryTimes = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(SiteCollectionUrl) || aveObjectModelFactory == null)
                {
                    return false;
                }
                Logger.Info($"Check site lock state for site {SiteCollectionUrl} with checkingState {checkingState}, retryTimes:{retryTimes}.");
                string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(null, SiteCollectionUrl);
                //Logger.Info($"O365 Admin Url is : {mAdminUrl}");
                var aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                if (aveTenant.TryGetAdminUrlForMultiGeoTenant(mAdminUrl, out string geoAdminUrl))
                {
                    Logger.Info($"O365 Tenant is a multiple geo tenant, old admin url: {mAdminUrl}, geoAdminUrl: {geoAdminUrl}");
                    mAdminUrl = geoAdminUrl;
                    aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                }

                var helper = new AveTaskRetryHelper(retryTimes, true, 5000);
                helper.ExecuteWithRetryMechanismV3(() =>
                {
                    var siteProps = aveTenant.GetSitePropertiesByUrl(SiteCollectionUrl);
                    Logger.Info($"Current site lock state is: {siteProps.LockState}");
                    if (siteProps.LockState.EqualIgnoreCase(checkingState.ToString()))
                    {
                        Logger.Info($"Site is finally match state: {checkingState}. Continue proceeding.");
                        Thread.Sleep(10000); // sleep a bit to sync up with site state
                        return;
                    }
                    else if (retryTimes <= 0) return;

                    throw new Exception($"Site is still not match state: {checkingState}, current state: {siteProps.LockState}.");
                }
                );

                return true;
            }
            catch (Exception e)
            {
                Logger.Info($"Error occur when check site lock.Message:{e}.");
            }
            return false;
        }

        public EXOMoveDestinationInfo GetDestinationColumnSetting(string siteUrl)
        {
            if (SiteBCSColumnDictionary.ContainsKey(siteUrl))
            {
                return SiteBCSColumnDictionary[siteUrl];
            }
            else
            {
                var info = RealGetDestinationColumnSetting(siteUrl);
                if (info != null)
                {
                    SiteBCSColumnDictionary.TryAdd(siteUrl, info);
                }
                else
                {
                    mLog.Warn("Destination site doesn't have column setting, site url:{0}", siteUrl);
                }
                return info;
            }
        }

        private EXOMoveDestinationInfo RealGetDestinationColumnSetting(string siteUrl)
        {
            var site = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteUrl);
            if (!string.IsNullOrEmpty(site.TeamId))
            {
                var teamsRemoteNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId).Item1;
                if (teamsRemoteNode == null) return null;
                Guid groupId = new Guid(teamsRemoteNode.parentId);
                var settings = TeamsSettingDao.LoadTeamsSettingForImportSetting(Guid.Empty, groupId);
                if (settings != null)
                {
                    EXOMoveDestinationInfo info = new EXOMoveDestinationInfo()
                    {
                        Exist = true,
                        UseExisting = settings.IsUsingExistColumnName,
                        ColumnName = settings.IsUsingExistColumnName ? settings.ExistColumnName : settings.ColumnName
                    };
                    return info;
                }
                else { return null; }
            }
            else
            {
                Guid groupId = new Guid(site.parentId);
                var settings = SharePointSettingDao.LoadSharePointSettingForImportSetting(Guid.Empty, groupId);
                if (settings != null)
                {
                    EXOMoveDestinationInfo info = new EXOMoveDestinationInfo()
                    {
                        Exist = true,
                        UseExisting = settings.IsUsingExistColumnName,
                        ColumnName = settings.IsUsingExistColumnName ? settings.ExistColumnName : settings.ColumnName
                    };
                    return info;
                }
                else { return null; }
            }
        }

        #region for RevIM 
        public string BCSColumnName = null;//绑定的column
        #endregion

        public ScheduleConfiguration(string jobId, bool isMainJobId = false)
        {
            this.JobId = jobId;
            TenantGroupId = TenantLocalValue.LogonGroupId;
            Init(isMainJobId);
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByDAO(string siteUrl)
        {
            if (remoteSites.ContainsKey(siteUrl))
            {
                return remoteSites[siteUrl];
            }
            else
            {
                RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                if (remoteSiteCollection != null && !remoteSites.ContainsKey(siteUrl))
                {
                    remoteSites.Add(siteUrl, remoteSiteCollection);
                    return remoteSiteCollection;
                }
                else
                {
                    mLog.Info($"GetRemoteSiteCollection sc info is null.Url: {siteUrl}");
                    return null;
                }
            }
        }

        public void InitDeletionContainer(string mSiteUrl, Guid webGuid, Guid listGuid)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InitDeletionContainer"))
            {
                try
                {
                    if (null == DeletionIAveSite || string.Compare(DeletionIAveSite.Url, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0 || mInitialTime.AddHours(23) < DateTime.Now)
                    {
                        if (string.IsNullOrEmpty(mSiteUrl))
                        {
                            mLog.Error("mSiteUrl is null when InitDeletionContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init DeletionSite when InitDeletionContainer.SiteUrl:{0}.", mSiteUrl);
                            mInitialTime = DateTime.Now;
                            AveObjectModelFactory factory = this.aveObjectModelFactory;
                            DeletionIAveSite = factory.CreateSite(mSiteUrl);
                        }
                    }

                    if (null == DeletionIAveWeb || !DeletionIAveWeb.ID.Equals(webGuid))
                    {
                        if (webGuid.Equals(Guid.Empty))
                        {
                            mLog.Error("webGuid is null when InitDeletionContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init DeletionWeb when InitDeletionContainer.webGuid:{0}.", webGuid);
                            if (DeletionIAveSite == null)
                            {
                                throw new Exception("DeletionIAveSite is null.");
                            }
                            DeletionIAveWeb = DeletionIAveSite.OpenWeb(webGuid);
                        }
                    }

                    if (null == DeletionIAveList || !listGuid.Equals(DeletionIAveList.ID))
                    {
                        if (listGuid.Equals(Guid.Empty))
                        {
                            mLog.Error("listGuid is null when InitDeletionContainer");
                        }
                        else
                        {
                            mLog.Info("Begin init DeletionList when InitDeletionContainer.listGuid:{0}.", listGuid);
                            if (DeletionIAveWeb == null)
                            {
                                throw new Exception("DeletionIAveWeb is null.");
                            }
                            DeletionIAveList = DeletionIAveWeb.Lists[listGuid];
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"InitDeletionContainer failed.Message:{ex}.");
                }
            }
        }

        public void InitStubAveBackupContainer()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InitStubSourceAveBackupContainer"))
            {
                try
                {
                    if (StubBackupAveSPSite == null || DeletionIAveSite.Url != StubBackupAveSPSite.SPSite.Url)
                    {
                        mLog.Info("Begin init StubBackupAveSPSite when InitStubAveBackupContainer.SiteUrl:{0}.", DeletionIAveSite.Url);
                        StubBackupAveSPSite = new Wrapper.Backup.AveSPSite(DeletionIAveSite.Url, AveContextKind.ClientObjectModel, this.user, null);
                        AveSiteInfo = new Wrapper.Backup.AveSPSiteInfo(StubBackupAveSPSite).GetSiteInfo();
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        StubUserInfos = StubBackupAveSPSite.GetUsers();
                        StubGroupInfos = StubBackupAveSPSite.GetGroups();
                        stopwatch.Stop();
                        mLog.Info($"InitStubAveBackupContainer StubUserInfos count:{StubUserInfos.Count},StubGroupInfos count:{StubGroupInfos.Count}.Time:{stopwatch.Elapsed.ToString()}.");
                    }
                    if (StubBackupAveSPWeb == null || StubBackupAveSPWeb.SPWeb.ID != DeletionIAveWeb.ID)
                    {
                        mLog.Info("Begin init StubBackupAveSPWeb when InitStubAveBackupContainer.WebUrl:{0}.", DeletionIAveWeb.ServerRelativeUrl);
                        if (StubBackupAveSPWeb != null)
                        {
                            StubBackupAveSPWeb.Dispose();//spWebId不等于file web ID时，需要先进行dispose
                        }
                        string webName = string.Empty;
                        if (DeletionIAveWeb.IsRootWeb)
                        {
                            webName = ".";
                        }
                        else
                        {
                            if (DeletionIAveSite.ServerRelativeUrl.Equals("/"))
                            {
                                webName = DeletionIAveWeb.ServerRelativeUrl.Substring(DeletionIAveSite.ServerRelativeUrl.Length);
                            }
                            else
                            {
                                webName = DeletionIAveWeb.ServerRelativeUrl.Substring(DeletionIAveSite.ServerRelativeUrl.Length + 1);
                            }
                        }
                        StubBackupAveSPWeb = new Wrapper.Backup.AveSPWeb(StubBackupAveSPSite, DeletionIAveWeb.ID, webName);
                        AveWebInfo = new Wrapper.Backup.AveSPWebInfo(StubBackupAveSPWeb).GetWebInfo();
                    }
                    if (StubBackupAveSPList == null || StubBackupAveSPList.SPList.ID != DeletionIAveList.ID)
                    {
                        mLog.Info("Begin init StubBackupAveSPList when InitStubAveBackupContainer.ListTitle:{0}.", DeletionIAveList.Title);
                        StubBackupAveSPList = new Wrapper.Backup.AveSPList(StubBackupAveSPWeb, DeletionIAveList.ID, DeletionIAveList.Title);
                        AveListInfo = new Wrapper.Backup.AveSPListInfo(StubBackupAveSPList).GetListInfo();
                    }
                    if (StubBackupAveSPRootFolder == null || StubBackupAveSPRootFolder.AveList.SPList.ID != DeletionIAveList.ID)
                    {
                        mLog.Info("Begin init StubBackupAveSPFolder when InitStubAveBackupContainer.ListTitle:{0}.", DeletionIAveList.Title);
                        if (StubBackupAveSPRootFolder != null)
                        {
                            StubBackupAveSPRootFolder.Dispose();
                        }
                        // 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
                        StubBackupAveSPRootFolder = new Wrapper.Backup.AveSPFolder(StubBackupAveSPList);
                        StubOnlyBackupAveSPCurrentFolder = StubBackupAveSPRootFolder;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"InitStubAveBackupContainer failed.Message:{ex}.");
                }
            }
        }

        public void InitStubAveRestoreContainer()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.InitStubAveRestoreContainer"))
            {
                try
                {
                    if (StubRestoreAveSPSite == null || DeletionIAveSite.Url != StubRestoreAveSPSite.SPSite.Url)
                    {
                        mLog.Info("Begin init StubRestoreAveSPSite when InitStubAveRestoreContainer.Site.Url:{0}.", DeletionIAveSite.Url);
                        StubRestoreAveSPSite = new Wrapper.Restore.AveSPSite(DeletionIAveSite.Url, DeletionIAveSite.Url, AveContextKind.ClientObjectModel, user);
                        StubRestoreAveSPSite.RestoreSiteSelf(AveSiteInfo);
                        //Stopwatch stopwatch = Stopwatch.StartNew();
                        //if (StubUserInfos.Count > 0)
                        //{
                        //    StubRestoreAveSPSite.SPMembers.MultiThreadRestoreUsers(StubUserInfos, true, true, false, null, false);
                        //}
                        //if (StubGroupInfos.Count > 0)
                        //{
                        //    StubRestoreAveSPSite.SPMembers.RestoreGroups(StubGroupInfos, false, true);
                        //}
                        //stopwatch.Stop();
                        mLog.Info($"Finish init StubRestoreAveSPSite when InitStubAveRestoreContainer.Site.Url:{DeletionIAveSite.Url}.");
                    }
                    if (StubRestoreAveSPWeb == null || StubRestoreAveSPWeb.SPWeb.ID != DeletionIAveWeb.ID)
                    {
                        mLog.Info("Begin init StubRestoreAveSPWeb when InitStubAveRestoreContainer.Web.Url:{0}.", DeletionIAveWeb.ServerRelativeUrl);
                        if (StubRestoreAveSPWeb != null)
                        {
                            StubRestoreAveSPWeb.Dispose();//spWebId不等于file web ID时，需要先进行dispose
                        }
                        StubRestoreAveSPWeb = new Wrapper.Restore.AveSPWeb(StubRestoreAveSPSite, DeletionIAveWeb.ServerRelativeUrl);
                        StubRestoreAveSPWeb.RestoreWebSelf(AveWebInfo);
                    }
                    if (StubRestoreAveSPList == null || StubRestoreAveSPList.SPList.ID != DeletionIAveList.ID)
                    {
                        mLog.Info("Begin init StubRestoreAveSPList when InitStubAveRestoreContainer.List.Title:{0}.", DeletionIAveList.Title);
                        StubRestoreAveSPList = new Wrapper.Restore.AveSPList(StubRestoreAveSPWeb, DeletionIAveList.Title);
                        StubRestoreAveSPList.RestoreListSelf(AveListInfo, true);
                        //LoadFields(StubRestoreAveSPList);
                    }
                    // 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
                    if (StubRestoreAveSPRootFolder == null || StubRestoreAveSPRootFolder.ParentList.Id != DeletionIAveList.ID)
                    {
                        mLog.Info("Begin init StubRestoreAveSPFolder when InitStubAveRestoreContainer.List.Title:{0}.", DeletionIAveList.Title);
                        StubRestoreAveSPRootFolder = new Wrapper.Restore.AveSPFolder(StubRestoreAveSPList, DeletionIAveList.RootFolder.Name);
                        StubOnlyRestoreAveSPCurrentFolder = StubRestoreAveSPRootFolder;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"InitStubAveRestoreContainer failed.Message:{ex}.");
                }
            }
        }

        //实例化到Subfolder,始终使用RootFolder对象去Get SubFolder
        public Wrapper.Restore.AveSPFolder GetStubRestoreAveCurrentFolder(string subFolderUrl, Guid parentFolderId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ReGetStubRestoreAveSPFolder"))
            {
                lock (mLock)
                {
                    if (!StubOnlyRestoreAveSPCurrentFolder.Id.Equals(parentFolderId))
                    {
                        mLog.Info("GetStubRestoreAveCurrentFolder StubSubFolderUrl:{0}.", subFolderUrl);
                        StubOnlyRestoreAveSPCurrentFolder = GetRestoreSubAveSPFolder(StubRestoreAveSPRootFolder, subFolderUrl);
                        return StubOnlyRestoreAveSPCurrentFolder;
                    }
                    else
                    {
                        return StubOnlyRestoreAveSPCurrentFolder;
                    }
                }
            }
        }

        private Wrapper.Restore.AveSPFolder GetRestoreSubAveSPFolder(Wrapper.Restore.AveSPFolder parentFolder, string destFolderUrl)
        {
            if (string.IsNullOrEmpty(destFolderUrl))
            {
                return parentFolder;
            }
            if (!destFolderUrl.Contains("/"))
            {
                Wrapper.Restore.AveSPFolder subFolder = new Wrapper.Restore.AveSPFolder(parentFolder, destFolderUrl);
                subFolder.InitSPFolder();
                return subFolder;
            }
            int pos = destFolderUrl.IndexOf("/");
            if (pos > -1)
            {
                string subDest = destFolderUrl.Substring(0, pos);
                string subLastDest = destFolderUrl.Substring(pos + 1);
                Wrapper.Restore.AveSPFolder subFolder = new Wrapper.Restore.AveSPFolder(parentFolder, subDest);
                subFolder.InitSPFolder();
                return this.GetRestoreSubAveSPFolder(subFolder, subLastDest);
            }
            return parentFolder;
        }

        /// <summary>
        /// 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
        /// </summary>
        public Wrapper.Backup.AveSPFolder GetCurrentAveBackupFolder(IAveFolder folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ReGetStubRestoreAveSPFolder"))
            {
                lock (mLock)
                {
                    Wrapper.Backup.AveSPFolder result;
                    if (folder.UniqueId != StubOnlyBackupAveSPCurrentFolder.Id)
                    {
                        mLog.Info("GetCurrentAveBackupFolder Current folder :{0} doesn't match StubBackupAveSPCurrentFolder:{1} that need get new folder.", folder.ServerRelativeUrl, StubOnlyBackupAveSPCurrentFolder.ServerRelativeUrl);
                        result = new Wrapper.Backup.AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.ID, 512/*folder.Item.Versions[0].VersionId*/);
                        StubOnlyBackupAveSPCurrentFolder = result;
                    }
                    else
                    {
                        result = StubOnlyBackupAveSPCurrentFolder;
                    }
                    return result;
                }
            }
        }

        public Wrapper.Backup.AveSPFolder GetCurrentAveBackupFolderByRootFolder(IAveFolder folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetCurrentAveBackupFolderByRootFolder"))
            {
                lock (mLock)
                {
                    Wrapper.Backup.AveSPFolder result;
                    if (string.IsNullOrEmpty(folder.ServerRelativeUrl))
                    {
                        mLog.Error("folder ServerRelativeUrl is empty.");
                        throw new Exception("File Not Found.");
                    }
                    if (folder.UniqueId != StubBackupAveSPRootFolder.Id)
                    {
                        mLog.Info("GetCurrentAveBackupFolderByRootFolder Current folder :{0} doesn't match StubBackupAveSPCurrentFolder:{1} that need get new folder.", folder.ServerRelativeUrl, StubBackupAveSPRootFolder.ServerRelativeUrl);
                        result = new Wrapper.Backup.AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.ID, 512/*folder.Item.Versions[0].VersionId*/);
                    }
                    else
                    {
                        result = StubBackupAveSPRootFolder;
                    }
                    return result;
                }
            }
        }

        public bool CheckItemIsRecordsHold(Guid itemId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckItemIsRecordsHold"))
            {
                bool isRecordsHold = false;
                //Explore Hold文件默认不处理.                
                Guid recordID = ScheduleConfiguration.GetRecordId(SiteCollectionID, itemId);
                Guid scopeID = SiteCollectionID;
                if (this.IsILMode && this.ExplorerDao != null)
                {
                    Record record = null;
                    if (IsRelativeDataJob)
                    {
                        record = this.ExplorerDao.ReadById(scopeID, recordID);
                    }
                    else
                    {
                        record = exploreDBSPRecords.Where(x => x.ScopeId == scopeID && x.Id == recordID).FirstOrDefault();
                        if (record == null && exploreDBSPRecords.Count >= 10000)
                        {
                            record = this.ExplorerDao.ReadById(scopeID, recordID);
                        }
                    }
                    if (record != null && record.HoldStatus == true && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
                    {
                        isRecordsHold = true;
                    }
                    if (record != null && !string.IsNullOrWhiteSpace(record.AveSiteId))
                    {
                        if (_workspaceReleaseTimeCache.Count > 500)
                        {
                            _workspaceReleaseTimeCache.Clear();
                        }
                        long workspaceReleaseTime = _workspaceReleaseTimeCache.GetOrAdd(
                            record.AveSiteId,
                            key => WorkplaceHoldDao.GetReleaseTimeByAveSiteIdAsync(key).GetAwaiter().GetResult()
                        );
                        mLog.Debug("CheckItemIsRecordsHold, recordId: {0}, scopeId: {1}, workspaceReleaseTime: {2}, currentUtcTicks: {3}, workspaceId: {4}", recordID, scopeID, workspaceReleaseTime, DateTime.UtcNow.Ticks, record.AveSiteId);

                        if (workspaceReleaseTime > DateTime.UtcNow.Ticks)
                        {
                            return true;
                        }
                    }
                }
                return isRecordsHold;
            }
        }
        public ArchiverBasicIndex GetArchiverIndex(string md5)
        {
            lock (mSenderLock)
            {
                if (!string.IsNullOrWhiteSpace(currentfileSenderJobId) && !currentfileSenderJobId.Equals(CurrentIndexJobID))
                {
                    try
                    {
                        CloseFileSender();
                    }
                    catch (Exception e)
                    {
                        mLog.Error($"Close file sender failed. Error : {e}");
                    }
                    fileSender = null;
                }
                if (fileSender == null)
                {
                    try
                    {
                        MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();//new MediaServer();
                        MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo(); //container.Resolve<CommonConfigInfo>("AvePoint.Media.Service.DomainModel.CommonConfigInfo");
                        MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo(); //container.Resolve<ArchiverConfigInfo>("AvePoint.Media.Service.DomainModel.ArchiverConfigInfo");
                        fileSender = MediaServiceFactory.CreateArchiverBackupDataWriter(); //container.Resolve<IArchiverBackupDataWriter>("AvePoint.Media.Service.ArchiverBackup.Backup.IArchiverBackupDataWriter");
                        fileSender.Open(ConvertBackupRequestToJob(CachedBackupJob[CurrentIndexJobID]));
                        mLog.Info($"Open index successfully. Current job id:{CurrentIndexJobID}");
                        currentfileSenderJobId = CurrentIndexJobID;
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(string.Format("Can't initialize media information. Message:{0}", ex.ToString()));
                        throw;
                    }
                }
            }
            return fileSender.GetArchiverIndex(md5);
        }
        private ArchiverBackupJob ConvertBackupRequestToJob(ArchiverBackupRequest aRequest)
        {
            ArchiverBackupJob archiverBackupJob = new ArchiverBackupJob(aRequest);
            archiverBackupJob.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = ArchiveTemp,
                Type = DeviceType.LocalPath,
                Password = string.Empty,
                UserName = string.Empty,
                Usage = null
            };
            archiverBackupJob.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            archiverBackupJob.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            archiverBackupJob.O365TenantId = this.O365TenantId;
            return archiverBackupJob;
        }

        public void CloseFileSender()
        {
            if (fileSender != null)
            {
                try
                {
                    fileSender.Close(new BackupCloseInfo());
                }
                catch (Exception e)
                {
                    mLog.Error($"Error occurred while closing sender. Error:{e.ToString()}");
                }
            }
        }

        public bool IsSiteReadOnly
        {
            get
            {
                lock (mLock)
                {
                    if (_isSiteSiteReadOnly.HasValue)
                    {
                        return _isSiteSiteReadOnly.Value;
                    }

                    _isSiteSiteReadOnly = CheckIsSiteLocked(SiteState.ReadOnly);

                    if (!_isSiteSiteReadOnly.HasValue)
                    {
                        Logger.Info($"Missing info to get site lock status, will init again later");
                        return false;
                    }
                    return _isSiteSiteReadOnly.Value;
                }
            }
        }

        private bool? _isSiteSiteReadOnly;

        private bool? CheckIsSiteLocked(SiteState checkingState)
        {
            try
            {
                if (string.IsNullOrEmpty(SiteCollectionUrl) || aveObjectModelFactory == null)
                {
                    return null;
                }
                string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, SiteCollectionUrl);
                Logger.Info($"O365 Admin Url is : {mAdminUrl}");
                var aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                if (aveTenant.TryGetAdminUrlForMultiGeoTenant(SiteCollectionUrl, out string geoAdminUrl))
                {
                    Logger.Info($"O365 Tenant is a multiple geo tenant, admin Url is : {geoAdminUrl}");
                    mAdminUrl = geoAdminUrl;
                    aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                }

                var siteProps = aveTenant.GetSitePropertiesByUrl(SiteCollectionUrl);

                Logger.Info($"Current site lock state is: {siteProps.LockState}, site template: {siteProps.Template}");

                _isChannelSite = AveSPWebTemplate.IsTeamPrivateChannelSite(siteProps.Template);
                if (siteProps.LockState.EqualIgnoreCase(checkingState.ToString()))
                {
                    Logger.Info($"Current site is locked.");
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Error occur when check site lock.Message:{e}.");
            }
            return false;
        }

        public bool IsChannelSite
        {
            get
            {
                lock (mLock)
                {
                    if (_isChannelSite.HasValue)
                    {
                        return _isChannelSite.Value;
                    }

                    _isChannelSite = CheckIsSiteChannel();

                    if (!_isChannelSite.HasValue)
                    {
                        Logger.Info($"Missing info to get site lock status, will init again later");
                        return false;
                    }
                    return _isChannelSite.Value;
                }
            }
        }

        private bool? _isChannelSite;

        // do not use any Teams info here, just check site template.
        private bool? CheckIsSiteChannel()
        {
            try
            {
                if (string.IsNullOrEmpty(SiteCollectionUrl) || aveObjectModelFactory == null)
                {
                    return null;
                }
                string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(aveObjectModelFactory.AccountInfo, SiteCollectionUrl);
                Logger.Info($"O365 Admin Url is : {mAdminUrl}");
                var aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                if (aveTenant.TryGetAdminUrlForMultiGeoTenant(SiteCollectionUrl, out string geoAdminUrl))
                {
                    Logger.Info($"O365 Tenant is a multiple geo tenant, admin Url is : {geoAdminUrl}");
                    mAdminUrl = geoAdminUrl;
                    aveTenant = aveObjectModelFactory.CreateTenant(mAdminUrl);
                }

                var siteProps = aveTenant.GetSitePropertiesByUrl(SiteCollectionUrl);

                Logger.Info($"Current site lock state: {siteProps.LockState}, site template: {siteProps.Template}");
                _isSiteSiteReadOnly = siteProps.LockState.EqualIgnoreCase(SiteState.ReadOnly.ToString());

                return AveSPWebTemplate.IsTeamPrivateChannelSite(siteProps.Template);
            }
            catch (Exception e)
            {
                Logger.Info($"Error occur when check is site channel.Message:{e}.");
            }
            return false;
        }

        public List<JobType> CanUnarchiveTeamsArchiveJobs =
        [
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
        ];

        private (string, string, string)? _channelSiteInfo;

        public (string GroupMailboxAddress, string GroupSiteUrl, string O365TenantId) ChannelSiteInfo
        {
            get
            {
                lock (mLock)
                {
                    if (!_channelSiteInfo.HasValue)
                    {
                        _channelSiteInfo = GetChannelSiteInfo();
                    }
                    return _channelSiteInfo.Value;
                }
            }
        }

        public (string, string, string) GetChannelSiteInfo()
        {
            if (string.IsNullOrEmpty(SiteCollectionUrl))
            {
                throw new Exception("[GetChannelSiteInfoAsync]SiteCollectionUrl is null or empty.");
            }
            return RemoteNodeService.GetChannelSiteInfoAsync(SiteCollectionUrl).ExecuteAsyncTask();
        }

        private bool? _hasUpgradeTeams;

        public bool HasUpgradeTeams
        {
            get
            {
                lock (mLock)
                {
                    if (!_hasUpgradeTeams.HasValue)
                    {
                        _hasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams();
                    }
                    return _hasUpgradeTeams.Value;
                }
            }
        }

        private int _canTryUnarchiveTeams;
        public int CanTryUnarchiveTeams
        {
            get
            {
                lock (mLock)
                {
                    return _canTryUnarchiveTeams;
                }
            }
            set
            {
                lock (mLock)
                {
                    _canTryUnarchiveTeams = value;
                }
            }
        }


        public string GetNodeFullPath(string nodePath)
        {
            string nodeFullPath = string.Empty;
            if (nodePath.StartsWith(this.siteUrlSchemeAndHost, StringComparison.OrdinalIgnoreCase) || nodePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                nodeFullPath = nodePath;
            }
            else
            {
                nodeFullPath = this.siteUrlSchemeAndHost + "/" + nodePath.TrimStart('/');
            }
            return nodeFullPath;
        }


        public string GetNodeFullPath(string nodePath, string siteUrl)
        {
            string nodeFullPath = string.Empty;
            string tempurl = new Uri(siteUrl).Scheme + @"://" + new Uri(siteUrl).Authority;
            if (nodePath.StartsWith(tempurl, StringComparison.OrdinalIgnoreCase))
            {
                nodeFullPath = nodePath;
            }
            else
            {
                nodeFullPath = tempurl + "/" + nodePath.TrimStart('/');
            }
            return nodeFullPath;
        }

        public bool CheckOtherThreadHasInitIAveListItem(string nodeId)
        {
            lock (mLock)
            {
                if (ArchiverBackupCacheItems.ContainsKey(nodeId) && ArchiverBackupCacheItems[nodeId].CacheItem != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CheckBackupItemCacheExist(string nodeId)
        {

            bool backupItemCacheExist = false;
            lock (mLock)
            {
                if (ArchiverBackupCacheItems.ContainsKey(nodeId))
                {
                    backupItemCacheExist = true;
                }
                else
                {
                    AddBackupItemCache(nodeId);
                }
            }
            return backupItemCacheExist;
        }

        private void AddBackupItemCache(string nodeId)
        {
            lock (mLock)
            {
                //Only Keep TotalMultiBackupThreadNumber Items
                if (ArchiverBackupCacheItems.Count >= this.BackgroundSettings.TotalMultiBackupThreadNumber * 2)
                {
                    var needRemoveKey = ArchiverBackupCacheItems.Where(y => y.Value.CacheTime != 0).OrderBy(x => x.Value.CacheTime).FirstOrDefault().Key;
                    mLog.Info($"Current node:{needRemoveKey} has large TotalMultiBackupThreadNumber {this.BackgroundSettings.TotalMultiBackupThreadNumber} and need remove from ArchiverBackupCacheItems:{ArchiverBackupCacheItems.Count}.");
                    ArchiverBackupCacheItems.Remove(needRemoveKey);
                }
                if (!ArchiverBackupCacheItems.ContainsKey(nodeId))
                {
                    ArchiverBackupCacheItems.Add(nodeId, new BackupIAveListItemCacheDto() { ItemId = nodeId });
                }
                else
                {
                    mLog.Warn($"Current node:{nodeId} already exist in BackupItemCache when AddToBackupItemCache.");
                }
            }
        }

        public void UpdateBackupItemCache(string nodeId, IAveListItem listItem)
        {
            lock (mLock)
            {
                if (ArchiverBackupCacheItems.ContainsKey(nodeId))
                {
                    ArchiverBackupCacheItems[nodeId].CacheTime = DateTime.UtcNow.Ticks;
                    ArchiverBackupCacheItems[nodeId].CacheItem = listItem;
                }
                else
                {
                    mLog.Warn($"Current node:{nodeId} does not exist in BackupItemCache when UpdateToBackupItemCache.");
                }
            }
        }

        //private bool CheckNeedDiscoverBySPQuery(List<Rule> rule)
        //{
        //    bool useSPQuery = false;
        //    if (!isRAJob)
        //    {
        //        var documentRules = rule.Where(r => r.PolicyLevel == PolicyLevel.Document).ToList();
        //        var nonDocumentsRules = rule.Where(r => r.PolicyLevel != PolicyLevel.Document).ToList();
        //        if (documentRules.Count > 0 && nonDocumentsRules.Count == 0)
        //        {
        //            var spQuerySelected = documentRules.Where(r => r.ScanType == ScanModeOption.Quick).ToList();
        //            var noSPQuerySelected = documentRules.Where(r => r.ScanType == ScanModeOption.Full).ToList();
        //            if (spQuerySelected.Count > 0 && noSPQuerySelected.Count == 0)
        //            {
        //                RuleItemCollections = CamlUtil.GetRuleItemCollection(DateTime.UtcNow, rule.Where(r => r.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document).ToList());
        //                if (!RuleItemCollection.HasUnCamlQueryableCondition)
        //                {
        //                    useSPQuery = true;
        //                }
        //            }
        //            mLog.Info($"Document rule count:{documentRules.Count} quick scan mode cpunt:{spQuerySelected.Count} full scan mode count:{noSPQuerySelected.Count}. ");
        //        }
        //        else
        //        {
        //            mLog.Info($"Document rule count:{documentRules.Count}, non document rule count:{nonDocumentsRules.Count}.");
        //        }
        //    }
        //    mLog.Info($"Use spquery to discover:{useSPQuery}.");
        //    return useSPQuery;
        //}

        public int GetBackupFileType()
        {
            int backupFileType = (int)BackupFileType.DataBlock;
            if (IsILMode)
            {
                if(BackgroundSettings.RecordsOutputStreamLevel == 0)
                {
                    backupFileType = (int)BackupFileType.RecordsFile;
                }
            }
            else
            {
                if (BackgroundSettings.ArchiverOutputStreamLevel == 0)
                {
                    backupFileType = (int)BackupFileType.File;
                }
            }

            return backupFileType;
        }

        public string GetRuleArchiverActionString(Rule rule, bool isSimulation = false)
        {
            return this.jobtype switch
            {
                JobType.DiscoverOptimization or JobType.DiscoveryPreScan or JobType.DiscoveryPlanProScan or JobType.DiscoveryPlanProOptimization => RuleManagerService.GetArchiverRuleActionStringForDiscoveryOptimization(rule, isSimulation),
                _ => RuleManagerService.GetArchiverRuleActionString(rule, this.jobtype)
            };
        }
    }

    public class JobSettingsBase
    {
        public Dictionary<int, Rule> RuleCollection = null;
        public RMSPTreeNode TreeNode = null;
        public JobSettingsBase(RMSPTreeNode node, Dictionary<int, Rule> rules)
        {
            RuleCollection = rules;
            TreeNode = node;
        }
    }
    public class DestinationListTermSetting
    {
        public bool HasDefaultTermValue { get; set; }
        public Guid DefautTermId { get; set; }
        public string DefaultTermName { get; set; }
        public Guid FieldId { get; set; }
        public Guid TextFieldId { get; set; }
    }
    public class EXOMoveDestinationInfo
    {
        public bool Exist { get; set; }
        public bool UseExisting { get; set; }
        public string ColumnName { get; set; }
    }

    public class ArchiveJobSplitedDBInfo
    {
        public bool IsNeedSplit { get; set; }
        public bool IsUseSplitedDB { get; set; }
        public bool IsLatestSplitedDB { get; set; }
        public Queue<string> SplitedSubsubjobids { get => splitedSubsubjobids; set => splitedSubsubjobids = value; }
        private Queue<string> splitedSubsubjobids = new Queue<string>();
        public ArchiveJobSplitLimit? SplitLimit { get; set; }
    }

}
