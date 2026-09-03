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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using Cloud.Sdk.Data.AosModern;
using LS.SPWorkflowProcessor;
using System.Reflection;
using System.Text;

namespace RAGoogle.Restore
{
    public class GDriveRestoreConfig : RestoreConfig
    {
        public static readonly string Configfile = System.IO.Path.Combine(AveEnv.AgentDataPath, "SP2010/Item/SP2010GranularBackupRestore.cfg");
        public static readonly string CACHE_DATA_FOLDER_NAME = "restore";

        public static BposSiteCollectionsConfig BPOSSiteCollectionConfig
        {
            get
            {
                return Singleton<BposSiteCollectionsConfig>.SingletonInstance;
            }
        }

        public AveObjectModelFactory ObjectModelFactory { get; private set; }
        public RMAosGoogleAppProfile appProfile { get; set; }

        public GDriveRestoreConfig() { }


        #region================Private Method================
        public void InitDefaultValueFromConfigFile()
        {
            DisableEventReceiver = true;
            //OutofRestoreConfig.KeepSiteStructure = true;
            //OutofRestoreConfig.KeepFolderStructure = true;
            CreateFieldIfNotExist = false;
            RestorePermissionLevel = false;
            SkipGlobalTermGroup = false;
            SkipLocalTermGroup = false;
            UseSourceLookupValue = !IsOutOfPlaceRestore;
            RestoreSOData = true;
            SkipIfSameModified = false;
            EnablePerformanceLog = false;
        }

        public void InitConfigForArchiver()
        {
            if (!Directory.Exists(JobDir))
            {
                Directory.CreateDirectory(JobDir);
            }
            SPWorkflowProcessorRuntime.RestoreParentAssociationIfNotFound = true;
            ArchiveConfigFileInfo archiverCheckFile = new ArchiveConfigFileInfo();
            if (archiverCheckFile.InverseInterpolatio())
            {
                if (archiverCheckFile.ItemConflictOverWrite())
                {
                    ItemDependencyType = ItemDependencyOption.Overwrite;
                }
                else if (archiverCheckFile.ItemConflictAppend())
                {
                    ItemDependencyType = ItemDependencyOption.Append;
                }
                else if (archiverCheckFile.ItemConflictSkip())
                {
                    ItemDependencyType = ItemDependencyOption.SkipConfilctItem;
                }
                else
                {
                    ItemDependencyType = ItemDependencyOption.SkipConfilctItem;
                }
            }
            else
            {
                ItemDependencyType = ItemDependencyOption.NotRestore;
            }
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView = true;//IncludeListView; //还原时支持include list view
            //Archiver不需要使用该功能
            SkipIfSameModified = false;
        }

        public void SetWorkflowOption(BackupRestoreWorkflow workflowState)
        {
            if (workflowState != null)
            {
                SPWorkflowProcessorRuntime.ProcessAssociation = workflowState.IncludeWorkflowDefinition;
                SPWorkflowProcessorRuntime.ProcessInstance = workflowState.IncludeWorkflowInstance;
                WorkflowState = workflowState;
                //SetAssociationConflictOption(workflowState.DefinitionConflictResolution);
                //SetInstanceConflictOption(workflowState.InstanceConflictResolution);
            }
        }

        #region Workflow Conflict Option, Maybe we will use in future.
        //private void SetInstanceConflictOption(WorkflowConflictResolutionType instanceConflictResolution)
        //{
        //    var wfConflictResolution = WFConflictResolution.Instance;
        //    switch (instanceConflictResolution)
        //    {
        //        case WorkflowConflictResolutionType.NotOverwrite:
        //            wfConflictResolution.InstanceOption = WFInstanceConflictResolutionOption.NotOverwrite;
        //            break;
        //        case WorkflowConflictResolutionType.Overwrite:
        //            wfConflictResolution.InstanceOption = WFInstanceConflictResolutionOption.Overwrite;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void SetAssociationConflictOption(WorkflowConflictResolutionType definitionConflictResolution)
        //{
        //    var wfConflictResolution = WFConflictResolution.Instance;
        //    switch (definitionConflictResolution)
        //    {
        //        case WorkflowConflictResolutionType.NotOverwrite:
        //            wfConflictResolution.AssociationOption = WFAssociationConflictResolutionOption.NotOverwrite;
        //            break;
        //        case WorkflowConflictResolutionType.Append:
        //            wfConflictResolution.AssociationOption = WFAssociationConflictResolutionOption.Append;
        //            break;
        //        case WorkflowConflictResolutionType.OverwriteOrSkipDefinition:
        //            wfConflictResolution.AssociationOption = WFAssociationConflictResolutionOption.Overwrite;
        //            break;
        //        case WorkflowConflictResolutionType.OverwriteDefinitionByForce:
        //            wfConflictResolution.AssociationOption = WFAssociationConflictResolutionOption.ForceOverwrite;
        //            break;
        //        default:
        //            break;
        //    }
        //}
        #endregion

        public void SetAppOption(ConflictResolutionType appConflictResolution)
        {
            switch (appConflictResolution)
            {
                case ConflictResolutionType.Overwrite:
                    AppRestoreMode = AveRestoreMode.OverWrite;
                    break;
                case ConflictResolutionType.Skip:
                default:
                    AppRestoreMode = AveRestoreMode.Default;
                    break;

            }
        }

        public void SetRestoreOption(ConflictResolutionType containerConflictResolution, ConflictResolutionType contentConflictResolution)
        {
            ContainerConflictResolution = containerConflictResolution;
            ContentConflictResolution = contentConflictResolution;

            switch (ContainerConflictResolution)
            {
                case ConflictResolutionType.Skip:
                    ContainerRestoreMode = AveRestoreMode.Default; //Not Overwrite.
                    break;
                case ConflictResolutionType.Merge:
                    ContainerRestoreMode = AveRestoreMode.OverWrite; //Overwrite.
                    break;
                case ConflictResolutionType.Replace:
                    ContainerRestoreMode = AveRestoreMode.Replace; // Replace.
                    break;
                default:
                    ContainerRestoreMode = AveRestoreMode.Default;
                    break;
            }
            if (ContainerConflictResolution != ConflictResolutionType.Replace)
            {
                switch (ContentConflictResolution)
                {
                    case ConflictResolutionType.Skip:
                        ContentRestoreMode = AveRestoreMode.Default; //Not Overwrite.
                        break;
                    case ConflictResolutionType.Overwrite:
                        ContentRestoreMode = AveRestoreMode.OverWrite;
                        break;
                    case ConflictResolutionType.OverwriteByModifiedTime:
                        ContentRestoreMode = AveRestoreMode.OverWriteByModifiedTime;
                        break;
                    case ConflictResolutionType.AppendItemOrDocumentByReNamed:
                        ContentRestoreMode = AveRestoreMode.Append; //Apend.
                        break;
                    case ConflictResolutionType.AppendANewVersion:
                        ContentRestoreMode = AveRestoreMode.AppendANewVersion;
                        break;
                    default:
                        ContentRestoreMode = AveRestoreMode.Default;
                        break;
                }
            }
            else
            {
                ContentRestoreMode = AveRestoreMode.OverWrite;
            }
        }
        #endregion

        #region ================Config Option================

        #region Common Config
        public ushort EventCategory { get; set; }

        public bool EnablePerformanceLog { get; set; }

        public string PlanId { get; set; }

        public string PlanDir { get; set; }

        public string JobId { get; set; }

        public string SubJobId { get; set; }

        public string JobDir { get; set; }

        /// <summary>
        /// Container Level Conflict Resolution including Skipped(AveRestoreMode.Default),Merge(AveRestoreMode.Overwrite),Replace(AveRestoreMode.Replace)
        /// </summary>
        public AveRestoreMode ContainerRestoreMode { get; set; }

        /// <summary>
        /// Content Level Conflict Resolution including
        /// Skipped(AveRestoreMode.Default),Overwrite(AveRestoreMode.Overwrite),Append_1(AveRestoreMode.Append),AppendANewVersion(AveRestoreMode.AppendANewVersion),OverWriteByModifiedTime(AveRestoreMode.OverWriteByModifiedTime)
        /// </summary>
        public AveRestoreMode ContentRestoreMode { get; set; }

        public AveRestoreMode AppRestoreMode { get; set; }

        public bool IsOutOfPlaceRestore
        {
            get { return RestoreType == RestoreType.OutOfPlace; }
        }

        public BackupLevel RestoreLevel { get; set; }
        #endregion

        #region Item Level Config

        /// <summary>
        /// Force user source lookup value for lookup column. Default:True if inplace,else false
        /// </summary>
        public bool UseSourceLookupValue { get; set; }

        /// <summary>
        /// Whether disable event receiver while restore job. Default:True
        /// </summary>
        public bool DisableEventReceiver { get; set; }

        /// <summary>
        /// User Mapping Option
        /// </summary>
        //public UserMappingOption UserDomainMapping { get; set; }

        /// <summary>
        /// Language Mapping Option for list and column
        /// </summary>
        //public LanguageMappingOption LanguageMappingInfo { get; set; }

        public OutofRestoreConfig OutofRestoreConfig { get; set; }

        /// <summary> 标示是否restore Extender or Connector data。</summary>
        public bool RestoreSOData { get; set; }

        /// <summary>
        /// 用于Column反插，还原Column值的时候如果不存在是否反插，默认false
        /// </summary>
        public bool CreateFieldIfNotExist { get; private set; }

        /// <summary>
        ///  Whether to break permission level inheritance if source web has unique permission level definition 
        /// </summary>
        public bool RestorePermissionLevel { get; private set; }

        /// <summary>
        /// Content Type Restore Option
        /// </summary>
        public AveContentTypeRestoreOption ContentTypeRestoreOption { get { return mContentTypeRestoreOption; } }

        private AveContentTypeRestoreOption mContentTypeRestoreOption = new AveContentTypeRestoreOption();

        /// <summary>
        /// Temporary not used
        /// </summary>
        public IncludeConfigurationReport IncludeConfigurationReport { get; private set; }

        /// <summary>
        /// Field Restore Option
        /// </summary>
        public AveFieldRestoreOption FieldRestoreOption
        {
            get
            {
                if (mFieldRestoreOption == null)
                {
                    mFieldRestoreOption = new AveFieldRestoreOption()
                    {
                        FindOption = new FieldFindOption[] { FieldFindOption.FindBySchema, FieldFindOption.FindById, FieldFindOption.Children, FieldFindOption.FindByInternalName, FieldFindOption.FindByStaticName },//ADO-21417:Add find by child
                    };
                }
                return mFieldRestoreOption;
            }
        }

        private AveFieldRestoreOption mFieldRestoreOption;

        /// <summary>
        /// Whether to keep column default value if source column value is null. Default:false
        /// </summary>
        public bool KeepColumnDefaultValue { get; set; }

        /// <summary>
        /// Skip Global Term for metadata service. Default:true
        /// </summary>
        public bool SkipGlobalTermGroup { get; set; }

        /// <summary>
        /// Skip Local Term for metadata service. Default:false
        /// </summary>
        public bool SkipLocalTermGroup { get; set; }

        /// <summary>
        /// Disable language mapping. Default:false
        /// </summary>
        public bool DisableLanguageMapping { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool SkipIfSameModified { get; private set; }

        /// <summary>
        /// SAAS-10617 针对Archiver 的Site Collection rule, Restore Site Collection之后要更新一下数据库，需要用到该属性
        /// </summary>
        public string TenantGroupId { get; set; }

        public GDriveRestoreRequest ArchiverConfigForMedia { get; set; }

        public bool RemoveSiteInRecycleBin { get; set; }


        #endregion

        #region Site/Subsite Level config

        public AveContextKind ContextKind { get; set; }

        /// <summary>
        /// A local path to store temp file for site/site collection level
        /// </summary>
        public string TempPath
        {
            get
            {
                return string.IsNullOrEmpty(this.tempPath) ?
                    AveEnv.AgentTempFolder.TrimEnd(Path.DirectorySeparatorChar) :
                    this.tempPath.TrimEnd(Path.DirectorySeparatorChar);
            }
        }

        private string tempPath;

        #endregion

        #endregion

        public override string ToString()
        {
            StringBuilder config = new StringBuilder();
            config.AppendFormat("\r\nPlanId:                    {0}\r\n", PlanId);
            config.AppendFormat("JobId:                     {0}\r\n", JobId);
            config.AppendFormat("IncludingRecycleBinData:   {0}\r\n", IncludingRecycleBinData);
            config.AppendFormat("IncludeItemsReport:        {0}\r\n", IncludeItemsReport);
            config.AppendFormat("ContainerRestoreMode:      {0}\r\n", ContainerRestoreMode);
            config.AppendFormat("ContentRestoreMode:        {0}\r\n", ContentRestoreMode);
            config.AppendFormat("AppRestoreMode:            {0}\r\n", AppRestoreMode);
            config.AppendFormat("RestoreType:               {0}\r\n", RestoreType);
            config.AppendFormat("JobType:                   {0}\r\n", JobType);
            //config.AppendFormat("RestoreGlobalOption:       {0}\r\n", RestoreGlobalOption.GetSettingInfo());
            config.AppendFormat("IncludeVersion:            {0}\r\n", IncludeVersion);
            config.AppendFormat("RestoreVersionCount:       {0}\r\n", VersionCount);
            //config.AppendFormat("IncludeTermStore:          {0}\r\n", !IsNotIncludeTermStore);
            config.AppendFormat("IncludeCustomPropertyBags: {0}\r\n", IncludeCustomPropertyBags);
            config.AppendFormat("IsIncludeSharedLinks: {0}\r\n", true);
            if (OutofRestoreConfig != null)
            {
                config.AppendFormat("KeepSiteStructure:         {0}\r\n", OutofRestoreConfig.KeepSiteStructure);
                config.AppendFormat("KeepFolderStructure:       {0}\r\n", OutofRestoreConfig.KeepFolderStructure);
                config.AppendFormat("Attach or Merge:           {0}\r\n", OutofRestoreConfig.RestoreContentsToSub ? "Attach" : "Merge");
            }
            return config.ToString();
        }
    }

    public class OutofRestoreConfig
    {
        public OutofRestoreConfig()
        {
            KeepSiteStructure = true;
            KeepFolderStructure = true;
        }

        /// <summary>
        /// Attach/Merge Logic. True if attach, otherwise false
        /// </summary>
        public bool RestoreContentsToSub { get; set; }

        /// <summary>
        /// Whether to keep structure for disordered site
        /// </summary>
        public bool KeepSiteStructure { get; set; }

        /// <summary>
        /// Whether to keep structure for disordered folder
        /// </summary>
        public bool KeepFolderStructure { get; set; }
    }

    public class BposSiteCollectionsConfig : IDisposable, ISingleton
    {
        private static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> groupSiteEmails = null;
        private Dictionary<string, AveBPOSAccountInfo> BPOSAccountInfos = null; //key: site collection url; value: account to access this site collection
        private Dictionary<string, bool> isNodeArchivered = null; //key: site collection url; value: IsNodeArchivered
        private List<string> BPOSWebApplications = null;
        private List<AveBPOSAccountInfo> ServicesAccounts = null;
        private List<Cloud.Sdk.Data.AosModern.AppProfileInfo> CustomerAppForSensitivityLabel = null;
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private BposSiteCollectionsConfig()
        {
            groupSiteEmails = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            BPOSAccountInfos = new Dictionary<string, AveBPOSAccountInfo>(StringComparer.OrdinalIgnoreCase);
            isNodeArchivered = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            BPOSWebApplications = new List<string>();
            ServicesAccounts = new List<AveBPOSAccountInfo>();
            CustomerAppForSensitivityLabel = new List<Cloud.Sdk.Data.AosModern.AppProfileInfo>();
        }
        public System.Threading.Tasks.Task InitAsync(IAveTreeNodeDto rootNode)
        {
            BPOSAccountInfos.Clear();
            isNodeArchivered.Clear();
            return InitBPOSAccountInfosAsync(rootNode);
        }

        public System.Threading.Tasks.Task InitAsync(IAveTreeNodeDto rootNode, string appProfileId, string siteAdminUrl)
        {
            BPOSAccountInfos.Clear();
            isNodeArchivered.Clear();
            return InitAOSPBPOSAccountInfosAsync(rootNode, appProfileId, siteAdminUrl);
        }
        private async System.Threading.Tasks.Task InitBPOSAccountInfosAsync(IAveTreeNodeDto rootNode)
        {
            if (rootNode.Level == NodeLevel.SiteCollection)
            {
                string siteUrl = rootNode.FullPath;
                var mapping = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(siteUrl);
                if (mapping != null && !string.IsNullOrEmpty(mapping.TargetSiteUrl))
                {
                    log.Info($"this site need to mapping new site url token,source:{siteUrl},target:{mapping.TargetSiteUrl}");
                    siteUrl = mapping.TargetSiteUrl;
                }
                AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
                if (rootNode.NodeExtension != null)
                {
                    log.Info("SiteCollection NodeExtension BposInfo is null:{0},URL is:{1}.", rootNode.NodeExtension.BposInfo == null, siteUrl);
                    if (rootNode.NodeExtension.BposInfo != null && rootNode.NodeExtension.BposInfo.UserAccountInfo != null)
                    {
                        BPOSAccountInfos[siteUrl] = rootNode.NodeExtension.BposInfo.ConvertToAveBPOSAccountInfo();
                        //Archiver SC App Profile方式restore，需要username做为SC Administrator
                        if (BPOSAccountInfos[siteUrl].ConnectionType == BposConnectionType.AppToken && rootNode.NodeExtension.BposInfo.UserAccountInfo.Username != null)
                        {
                            BPOSAccountInfos[siteUrl].UserName = rootNode.NodeExtension.BposInfo.UserAccountInfo.Username;
                        }
                    }
                    isNodeArchivered[siteUrl] = rootNode.NodeExtension.IsNodeArchivered;

                    if (remoteSiteCollection == null)
                    {
                        var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
                        AveBPOSAccountInfo siteAccount = null;
                        string tenantId = string.Empty;
                        log.Info($"GetHasADPermissionProfiles profiles count is:{profiles?.Count}");
                        foreach (var temp in profiles)
                        {
                            log.Info($"GetHasADPermissionProfiles profiles temp is:ClientId:{temp.AppClientId},AuthenticationProfileId:{temp.Id},name:{temp.Name}");
                            if (siteUrl.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
                            {
                                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(temp.TenantId).GetAwaiter().GetResult().AdminUrl;

                                siteAccount = new AvePoint.Wrapper.Common.AveBPOSAccountInfo()
                                {
                                    TenantId = temp.TenantId,
                                    AdminUrl = adminUrl,
                                    ClientId = temp.AppClientId,
                                    ConnectionType = AvePoint.Wrapper.Common.BposConnectionType.AppToken,
                                    TenantGroupId = TenantLocalValue.LogonGroupId,
                                    AuthenticationProfileId = temp.Id,
                                    AppType = ConvertIdentityTypeToAppType(temp.Type),
                                    AADEnvironment = (Microsoft365.Authentication.AveAzureEnvironment)temp.AADEnvironment,
                                    //AppCert = apponlyCertificate
                                };
                                tenantId = temp.TenantId;
                                break;
                            }
                        }
                        BPOSAccountInfos[siteUrl] = siteAccount;
                        log.Info($"siteAccount profiles is:ClientId:{siteAccount?.ClientId},AuthenticationProfileId:{siteAccount?.AuthenticationProfileId}");
                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            //ServicesAccounts = PoolUserUtil.GetSAInfoFromAOS(tenantId);
                            //ustomerAppForSensitivityLabel = PoolUserUtil.GetCustomAppProfilesForSensitivityLabel(tenantId);
                        }
                    }
                    else
                    {
                        //AveBPOSAccountInfo aveBPOSAccountInfo = await PoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
                        //BPOSAccountInfos[siteUrl] = aveBPOSAccountInfo;
                        //log.Info($"GetBPOSInfoAsync siteAccount profiles is:ClientId:{aveBPOSAccountInfo?.ClientId},AuthenticationProfileId:{aveBPOSAccountInfo?.AuthenticationProfileId},tenantId:{aveBPOSAccountInfo?.TenantId}");
                        //ServicesAccounts = PoolUserUtil.GetSAInfoFromAOS(remoteSiteCollection.TenantId);
                        //CustomerAppForSensitivityLabel = PoolUserUtil.GetCustomAppProfilesForSensitivityLabel(remoteSiteCollection.TenantId);
                    }
                    if (rootNode.Type == NodeType.O365GroupSites && !string.IsNullOrEmpty(rootNode.NodeExtension.O365GroupEmail))
                    {
                        groupSiteEmails[siteUrl] = rootNode.NodeExtension.O365GroupEmail;
                    }
                    try
                    {
                        var tenantId = string.Empty;
                        if (remoteSiteCollection != null)
                        {
                            tenantId = remoteSiteCollection.TenantId;
                        }
                        else if (BPOSAccountInfos[siteUrl] != null)
                        {
                            tenantId = BPOSAccountInfos[siteUrl].TenantId;
                        }

                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            var accounts = RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, tenantId);
                            if (accounts != null && accounts.Count > 0)
                            {
                                WrapperConfiguration.AddAPPByServiceAccount = true;
                                WrapperConfiguration.accountInfo = new List<AveBPOSAccountInfo>();
                                foreach (var temp in accounts)
                                {
                                    WrapperConfiguration.accountInfo.Add(new AveBPOSAccountInfo()
                                    {
                                        UserName = temp.UserName,
                                        Password = temp.Password.ToSecureStringWithEmptyCheck()
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WrapperConfiguration.AddAPPByServiceAccount = false;
                        log.Warn($"can not get ServiceAccounts info for restore APP,error:{e}");
                    }
                }
            }
            else
            {
                foreach (IAveTreeNodeDto child in rootNode.Children)
                {
                    await InitBPOSAccountInfosAsync(child);
                }
            }
        }

        private async System.Threading.Tasks.Task InitAOSPBPOSAccountInfosAsync(IAveTreeNodeDto rootNode, string appProfileId, string siteAdminUrl)
        {
            if (rootNode.Level == NodeLevel.SiteCollection)
            {
                string siteUrl = rootNode.FullPath;
                var mapping = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(siteUrl);
                if (mapping != null && !string.IsNullOrEmpty(mapping.TargetSiteUrl))
                {
                    log.Info($"this site need to mapping new site url token,source:{siteUrl},target:{mapping.TargetSiteUrl}");
                    siteUrl = mapping.TargetSiteUrl;
                }

                if (rootNode.NodeExtension != null)
                {
                    log.Info("SiteCollection NodeExtension BposInfo is null:{0},URL is:{1}.", rootNode.NodeExtension.BposInfo == null, siteUrl);
                    if (rootNode.NodeExtension.BposInfo != null && rootNode.NodeExtension.BposInfo.UserAccountInfo != null)
                    {
                        BPOSAccountInfos[siteUrl] = rootNode.NodeExtension.BposInfo.ConvertToAveBPOSAccountInfo();
                        //Archiver SC App Profile方式restore，需要username做为SC Administrator
                        if (BPOSAccountInfos[siteUrl].ConnectionType == BposConnectionType.AppToken && rootNode.NodeExtension.BposInfo.UserAccountInfo.Username != null)
                        {
                            BPOSAccountInfos[siteUrl].UserName = rootNode.NodeExtension.BposInfo.UserAccountInfo.Username;
                        }
                    }
                    isNodeArchivered[siteUrl] = rootNode.NodeExtension.IsNodeArchivered;

                    //var aveBPOSAccountInfo = await PoolUserUtil.GetAOSPBPOSInfoAsync(appProfileId, siteAdminUrl);
                    //BPOSAccountInfos[siteUrl] = aveBPOSAccountInfo;
                    //log.Info($"GetBPOSInfoAsync siteAccount profiles is:ClientId:{aveBPOSAccountInfo?.ClientId},AuthenticationProfileId:{aveBPOSAccountInfo?.AuthenticationProfileId},tenantId:{aveBPOSAccountInfo?.TenantId}");
                    //ServicesAccounts = PoolUserUtil.GetSAInfoFromAOS(aveBPOSAccountInfo.TenantId);
                    //CustomerAppForSensitivityLabel = PoolUserUtil.GetCustomAppProfilesForSensitivityLabel(aveBPOSAccountInfo.TenantId);

                    if (rootNode.Type == NodeType.O365GroupSites && !string.IsNullOrEmpty(rootNode.NodeExtension.O365GroupEmail))
                    {
                        groupSiteEmails[siteUrl] = rootNode.NodeExtension.O365GroupEmail;
                    }
                    try
                    {
                        var tenantId = BPOSAccountInfos[siteUrl].TenantId;

                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            var accounts = RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, tenantId);
                            if (accounts != null && accounts.Count > 0)
                            {
                                WrapperConfiguration.AddAPPByServiceAccount = true;
                                WrapperConfiguration.accountInfo = new List<AveBPOSAccountInfo>();
                                foreach (var temp in accounts)
                                {
                                    WrapperConfiguration.accountInfo.Add(new AveBPOSAccountInfo()
                                    {
                                        UserName = temp.UserName,
                                        Password = temp.Password.ToSecureStringWithEmptyCheck()
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WrapperConfiguration.AddAPPByServiceAccount = false;
                        log.Warn($"can not get ServiceAccounts info for restore APP,error:{e}");
                    }
                }
            }
            else
            {
                foreach (IAveTreeNodeDto child in rootNode.Children)
                {
                    await InitBPOSAccountInfosAsync(child);
                }
            }
        }
        /// <summary>       
        /// /// ignore case, will return bpos account info if existed, otherwise will return null.       
        /// /// </summary>       
        /// /// <param name="siteUrl"></param>     
        /// /// <returns></returns>      
        public AveBPOSAccountInfo this[string siteUrl]
        {
            get
            {
                return this.BPOSAccountInfos.ContainsKey(siteUrl) ?
                    this.BPOSAccountInfos[siteUrl] : null;
            }
        }

        public AveBPOSAccountInfo GetServiceAccount()
        {
            return ServicesAccounts.FirstOrDefault();
        }

        public Cloud.Sdk.Data.AosModern.AppProfileInfo GetAppProfileForSensitivityLabel()
        {
            return CustomerAppForSensitivityLabel.FirstOrDefault();
        }

        public int Count
        {
            get
            {
                return BPOSAccountInfos.Count;
            }
        }
        public AveContextKind GetContextKind(string siteUrl)
        {
            return IsBPOSSiteCollection(siteUrl) ?
                AveContextKind.ClientObjectModel : AveContextKind.ServerObjectModel;
        }
        public AveContextKind GetContextKind()
        {
            return AveContextKind.Auto;
        }
        private bool IsBPOSSiteCollection(string siteUrl)
        {
            if (BPOSAccountInfos != null && BPOSAccountInfos.Count > 0)
            {
                return BPOSAccountInfos.ContainsKey(siteUrl);
            }
            else return false;
        }
        public List<string> WebApplications
        {
            get
            {
                return this.BPOSWebApplications;
            }
        }

        public Dictionary<string, string> GroupSiteEmails
        {
            get
            {
                return this.groupSiteEmails;
            }
        }

        public Dictionary<string, bool> IsNodeArchivered
        {
            get
            {
                return isNodeArchivered;
            }
        }

        public void Dispose()
        {
            BPOSAccountInfos.Clear();
            BPOSAccountInfos = null;
            isNodeArchivered.Clear();
            isNodeArchivered = null;
        }

        private AvePoint.GCommon.Contract.CentralAdmin.Object.AppType ConvertIdentityTypeToAppType(IdentityProviderType providerType)
        {
            return providerType switch
            {
                IdentityProviderType.Office365 => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365,
                IdentityProviderType.SharePoint => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.SharePoint,
                IdentityProviderType.Exchange => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Exchange,
                IdentityProviderType.CustomAzureApp => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp,
                IdentityProviderType.CustomDelegateApp => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomDelegateApp,
                IdentityProviderType.CloudRecords => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords,
                _ => AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365,
            };
        }
    }

    public class IncludeConfigurationReport
    {
        public bool Workflow = true;
        public bool Field = true;
        public bool ContentType = true;
    }
}
