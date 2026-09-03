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
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Media.StorageService;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Discover.DiscoverImpl;
using AvePoint.RA.RAExchange.Disposal.Action;
using AvePoint.RA.RAExchange.Disposal.Action.ExchangeObjects.Deletion;
using AvePoint.RA.RAExchange.Disposal.Action.ExchangeObjects.Tag;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.Disposal.Object;
using AvePoint.RA.RAExchange.Extension;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.ObjectModel.Storage.Entity;
using AvePoint.Wrapper.Restore;
using ExchangeBackupUtility.Graph;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using RAArchiverCommon.DisposalProgress.Impl;
using RAExportCommon;
using RAManualApprovalCommon.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;
using ExchangeItemBulkHelper = ExchangeBackupUtility.ExchangeItemBulkHelper;
using IExchangeFolder = ExchangeBackupUtility.Graph.IExchangeFolder;
using IExchangeItem = ExchangeBackupUtility.Graph.IExchangeItem;
using IExchangeItemBulkHelper = ExchangeBackupUtility.Graph.IExchangeItemBulkHelper;
using SharePointItemType = AvePoint.RA.SharePoint.ArchiverCommon.ItemType;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class RMEXOEnforceRuleActionBase : RMEXODiscoverBaseV2
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOEnforceRuleActionBase));
        #region interface
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
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
        private ITermRuleAssociationDao termRuleAssociationDao;
        protected ITermRuleAssociationDao TermRuleInfos
        {
            get
            {
                if (termRuleAssociationDao == null)
                {
                    termRuleAssociationDao = (ITermRuleAssociationDao)PlatformWindsorManager.GetService(typeof(ITermRuleAssociationDao));
                }
                return termRuleAssociationDao;
            }
        }

        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }

        private IExplorerDao _explorerDao;
        protected IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }

        private IArchiverRuleService mArchiverRuleService;
        public IArchiverRuleService ArchiverRuleService
        {
            get { return mArchiverRuleService ?? (IArchiverRuleService)PlatformWindsorManager.GetService(typeof(IArchiverRuleService)); }
            set { mArchiverRuleService = value; }
        }
        private IStorageDeviceService mStorageDeviceService;
        public IStorageDeviceService StorageDeviceService
        {
            get { return mStorageDeviceService ?? (IStorageDeviceService)PlatformWindsorManager.GetService(typeof(IStorageDeviceService)); }
            set { mStorageDeviceService = value; }
        }

        private ISettingProfilesDao mSettingProfilesDao;
        public ISettingProfilesDao SettingProfileDao
        {
            get { return mSettingProfilesDao ?? (ISettingProfilesDao)PlatformWindsorManager.GetService(typeof(ISettingProfilesDao)); }
            set { mSettingProfilesDao = value; }
        }
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        #endregion
        private const string EXO = "EXO";
        protected JobManagement JobManagement = null;
        private IBatchDiscoverV2 discover = null;
        protected Guid GroupId = Guid.Empty;
        protected Guid AOSMailboxId = Guid.Empty;
        private Microsoft.Exchange.WebServices.Data.SearchFilter mSearchFilter = null;
        private List<Rule> allRulesList = null;
        private Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping;
        //private Dictionary<Guid, string> ReviewedUserIdsAndNodeIdMapping;
        //private Dictionary<Guid, string> TermIdAndNameMapping;
        //private Dictionary<Guid, RMRule> mRuleCache = new Dictionary<Guid, RMRule>();
        private RuleCollection mRuleCollection = null;
        private EXOConfiguration mConfiguration = null;
        private IBackupController backupController;
        private EXOExportBeforeArcInfo EXOExportBefArcInfo = null;
        private IEXOExport EXOExport = null;
        private bool skipRemoveAction = false;
        private int mThreadCount = 3;
        private long currentUtcTime;
        private List<Guid> rejectEmailId = new List<Guid>();

        private SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);

        private IRMCustomIndexMetadataDao CustomIndexMetadataDao => PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();
        private IRMCustomMetadataColumnDao CustomMetadataColumnDao => PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();

        private List<RMCustomIndexMetadata> CustomIndexMetadatas = new();
        private List<RMCustomMetadataColumn> CustomMetadataColumns = new();

        public RMEXOEnforceRuleActionBase(ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement, bool isNullClassification)
            : base(treeNode, isNullClassification)
        {
            JobManagement = jobManagement;
            skipRemoveAction = treeNode.SkipRemoveContentAndDestroyAction;
        }
        public override void Init()
        {
            base.Init();

            _ = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnableCustomColumn);
            if (isEnableCustomColumn)
            {
                Task.Run(async () =>
                {
                    var indexMetadatasTask = CustomIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.Exchange);
                    var metadataColumnsTask = CustomMetadataColumnDao.GetAllCustomMetadataColumnsAsync();

                    await Task.WhenAll(indexMetadatasTask, metadataColumnsTask);

                    CustomIndexMetadatas = indexMetadatasTask.Result.ToList();
                    CustomMetadataColumns = metadataColumnsTask.Result.ToList();
                }).GetAwaiter().GetResult();
            }

            GroupId = new Guid(TreeManagement.GetGroupNode(TreeNodeDto).ID);
            AOSMailboxId = new Guid(MailboxGuid);
            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.GetRulesFromRecords", "", true))
            {
                allRulesList = RuleManagerService.GetRulesFromRecords();
            }
            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.GetTermAndRuleMappings", "", true))
            {
                TermAndRulesMapping = GetTermAndRuleMappings();
            }

            var rules = GetRuleCollection(IsNullClassification);
            ConvertCorrectEXORule(rules);
            ConvertEXORecordsMoveSetting(rules);
            TreeManagement tm = new TreeManagement();
            var mailBoxStringId = tm.GetRealMailboxStringId(TreeNodeDto);
            mConfiguration = new EXOConfiguration(AOSMailboxId, mailBoxStringId, TreeNodeDto.Name, Service, this.IsSupportGraphApi);
            mConfiguration.HasUpgradeVEOV3 = VEOV3CommonMethod.HasUpgradedVEOV3();
            mConfiguration.ContainerId = GroupId;
            mConfiguration.MailBoxTreeNodeId = TreeManagement.GetMailboxNode(TreeNodeDto).ID;
            mConfiguration.SubJobId = JobManagement.SubJobId;
            mConfiguration.IsSupportGraphAPI = this.IsSupportGraphApi;
            mConfiguration.MailboxId = this.CurrentFolder.MailBoxId;
            mRuleCollection = new RuleCollection() { Rules = rules };
            //TODO: Research search
            EWSSearchFilterUtility eWSSearchFilterUtility = new EWSSearchFilterUtility(mRuleCollection, IsNullClassification, IsSupportGraphApi);
            mSearchFilter = GetSearchFilter(eWSSearchFilterUtility, IsNullClassification);
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!System.IO.Directory.Exists(archiveTemp))
            {
                System.IO.Directory.CreateDirectory(archiveTemp);
            }
            AvePoint.Common.AveEnv.AgentJobFolder = archiveTemp;
            currentUtcTime = DateTime.UtcNow.Ticks;
            InitThreadCount();

            if (IsSupportGraphApi)
            {
                this.mThreadCount = this.MaxBackupItemsThreads;
            }
            _semaphore = new(MaxBackupItemsThreads, MaxBackupItemsThreads);
        }
        public void SetDiscoverObject(IBatchDiscoverV2 discover)
        {
            this.discover = discover;
        }
        public void Scan()
        {
            Init();
            logger.Info("Begin to scan mailbox.");
            ProcessFolder(CurrentFolder);
            CosmosDBManualDataUpdater.Commit();
            logger.Info("Finish scan mailbox.");
        }
        private void RemoveManualFields()
        {
            try
            {
                if (mRuleCollection?.Rules?.Values?.Count > 0)
                {
                    logger.Info("Begin to RemoveManualFields.");
                    foreach (var rule in mRuleCollection.Rules.Values)
                    {
                        logger.Info($"Begin to RemoveManualFields.rule id:{rule.Id}");
                        IEnumerable<Record> needRemoveManulFields = null;
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            needRemoveManulFields = ExplorerDao.QueryAll(e => e.ManualApprovedStatus == (int)SOApproveDBStatus.Approved && e.ManualArchiveStatus == (int)Contract.Schedule.ActionStatus.None && e.EmailAddress.Equals(MailboxAddress) && e.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase) && e.SourceFlag == (int)SourceFlag.Exchange && e.CollectTime < currentUtcTime);
                        }
                        else
                        {
                            if (!skipRemoveAction)
                            {
                                needRemoveManulFields = ExplorerDao.QueryAll(e => (e.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || e.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected) && e.ManualArchiveStatus == (int)Contract.Schedule.ActionStatus.None && e.EmailAddress.Equals(MailboxAddress) && e.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase) && e.SourceFlag == (int)SourceFlag.Exchange && e.CollectTime < currentUtcTime);
                            }
                        }
                        if (needRemoveManulFields != null)
                        {
                            foreach (var item in needRemoveManulFields)
                            {
                                if (rejectEmailId.Contains(item.Id))
                                {
                                    logger.Info($"this record is reject and it fit rule,do not remove,id:{item.Id.ToString()}");
                                    continue;
                                }
                                item.RemoveManualFields();
                                CosmosDBManualDataUpdater.Add(item);
                            }
                        }
                    }
                    CosmosDBManualDataUpdater.Commit();
                    logger.Info("Finish RemoveManualFields.");
                }
                else
                {
                    logger.Info("No rules to remove manual fields.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while remove manual fields, error:{ex.ToString()}");
            }
        }
        public void Archive()
        {
            logger.Info("Begin to archive mailbox.");
            var rules = GetAllRules();
            logger.Info($"Rule count:{rules?.Count}");
            if (rules != null && rules.Count > 0)
            {
                try
                {
                    foreach (var ruleId in rules)
                    {
                        ArchiveDataForRule(ruleId);
                    }
                }
                catch (JobStopException)
                {
                    throw;
                }
                finally
                {
                    if (EXOExportBefArcInfo != null && EXOExportBefArcInfo.EXOExport != null)
                    {
                        if(mConfiguration.CurrentRule.ExportType == ExportTypeValue.VEO && mConfiguration.HasUpgradeVEOV3)
                        {
                            EXOExport.ExtensionMethod(true);
                        }
                    }
                    DestructionFactory.GetInstance(mConfiguration.mailboxStringId, mConfiguration.SubJobId).UploadToStorage();
                }
            }
            RemoveManualFields();
            logger.Info("Finish archive mailbox.");
        }


        #region private method

        private void InitThreadCount()
        {
            try
            {
                mThreadCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while get max thread count, error:{e.ToString()}");
                mThreadCount = 3;
            }
        }
        private void ConvertCorrectEXORule(Dictionary<int, Rule> rules)
        {
            foreach (Rule rule in rules.Values)
            {
                rule.PolicyLevel = PolicyLevel.ExchangeOnlineItem;
                Dictionary<PolicyLevel, string> filterConditionExpressionLists = new Dictionary<PolicyLevel, string>();
                //目前Records Rule界面只有MessageLevel，目前Rule里面的AndOrExpression赋值为ExchangeOnlineItem
                foreach (PolicyLevel level in rule.AndOrExpression.Keys)
                {
                    if (level == PolicyLevel.ExchangeOnlineItem)
                    {
                        filterConditionExpressionLists.Add(PolicyLevel.ExchangeOnlineItem_Message, rule.AndOrExpression[level]);
                    }
                    else
                    {
                        filterConditionExpressionLists.Add(level, rule.AndOrExpression[level]);
                    }
                }
                rule.AndOrExpression = filterConditionExpressionLists;
                foreach (FilterPolicy filter in rule.Filters)
                {
                    if (filter.Level == PolicyLevel.ExchangeOnlineItem)
                    {
                        filter.Level = PolicyLevel.ExchangeOnlineItem_Message;
                    }
                }
            }
        }
        private void ConvertEXORecordsMoveSetting(Dictionary<int, Rule> rules)
        {
            foreach (Rule rule in rules.Values)
            {
                if (rule.EXORule.spMoveOption != null && rule.EXORule.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(rule.EXORule.spMoveOption.MoveDestination.SPUrl))
                {
                    rule.EXORule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting();
                    switch (rule.EXORule.spMoveOption.MoveSetting.ItemLevelConflictOption)
                    {
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Skip:
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Skip;
                            break;
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByName:
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Append;
                            break;
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Overwrite:
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.ContentConflictResolution = ContentConflictResolution.Overwrite;
                            break;
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.NotOverwrite:
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.AppendByVersion:
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Replace:
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.Merge:
                        case AvePoint.GCommon.Contract.StorageOptimization.Object.ConflictOption.OverwriteByLastModifiedTime:
                        default:
                            logger.Info("Not support ContentConflictResolution.");
                            break;
                    }
                    //rule.MoveToRecordCenterAndDelareSetting.DestFlag = RecordFlag.SP;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation = new DestinationLocationInfo();
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url = rule.EXORule.spMoveOption.MoveDestination.SPUrl;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.ContainerId = rule.EXORule.spMoveOption.MoveDestination.ContainerId;
                    //SPAccount 是365用，FSAccount是local from records
                    var remoteNodeInfo = RABrowserClient.GetRemoteSiteCollectionByListUrl(rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url);
                    //GetRemoteNodeInfo(rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url, false);
                    if (remoteNodeInfo == null)
                    {
                        logger.Info("AOS RemoteNodeInfo is null when connect AOS.");
                    }
                    else
                    {
                        var bopsInfo = CommonPoolUserUtil.GetBPOSInfo(remoteNodeInfo);
                        if (bopsInfo.ConnectionType == AvePoint.Wrapper.Common.BposConnectionType.ServiceAccount)
                        {
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.UserName = bopsInfo.UserName;
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Password = bopsInfo.Password.ToPlainString();
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo = new BposInfo()
                            {
                                ConnectionType = GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount,
                                TenantGroupId = TenantLocalValue.LogonGroupId,
                                SiteUrl = remoteNodeInfo.url,
                                Mode = BPOSMode.Office365,
                                AppType = AppType.Office365,
                                UserAccountInfo = new BposUserAccountInfo()
                                {
                                    Username = bopsInfo.UserName,
                                    Password = bopsInfo.Password.ToPlainString(),
                                    Domain = bopsInfo.Domain,
                                    TenantId = remoteNodeInfo.TenantId,
                                    AdminUrl = remoteNodeInfo.AdminUrl,
                                }
                            };
                        }
                        else
                        {
                            rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo = new BposInfo()
                            {
                                SiteUrl = remoteNodeInfo.url,
                                Mode = BPOSMode.Office365,
                                ConnectionType = GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken,
                                AppType = bopsInfo.AppType,
                                TenantGroupId = TenantLocalValue.LogonGroupId,
                                UserAccountInfo = new BposUserAccountInfo()
                                {
                                    Domain = bopsInfo.Domain,
                                    TenantId = remoteNodeInfo.TenantId,
                                    AdminUrl = remoteNodeInfo.AdminUrl,
                                    AppClientId = bopsInfo.ClientId,
                                    //AppCertSecret = bopsInfo.UserAccountInfo.AppCertSecret,
                                    //AppCertContent = remoteNodeInfo.AppCertContent,
                                    //AppCertSecretContent = bopsInfo.UserAccountInfo.AppCertSecretContent,
                                    AADEnvironment = remoteNodeInfo.AADEnvironment,
                                    AppId = bopsInfo.AuthenticationProfileId,
                                }
                            };
                        }
                    }
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.DelaredRecord = rule.EXORule.spMoveOption.MoveDestination.NotDeclareMovedData;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.DeleteSourceItem = rule.EXORule.spMoveOption.MoveDestination.DeleteSourceItem;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.KeepSourceClassification = rule.EXORule.spMoveOption.MoveDestination.KeepSourceClassification;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.IsMoveVersions = false;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.LeaveLinkInSource = false;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.OperateDataMode = OperatingSharePointDataMode.MoveToRecordCenterAndDelare;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.OriginalMetaDataAsXML = false;
                    rule.EXORule.MoveToRecordCenterAndDelareSetting.UseTransferedFileMode = UseTransferedFileMode.KeepOriginalContentType;
                }
            }
        }
        private Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappings()
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = allRulesList.ToDictionary(r => new Guid(r.Id));//get rule from DA//RuleService.GetRulesFromDA().ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();

                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            reOrder++;
                            var ruleOBj = CloneSameRuleObject(rule);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                        }
                    }
                }

                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection
                        {
                            TermId = refTerm.UniqueId,
                            TermName = refTerm.Name
                        };
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }

                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;

                }
            }

            return termRuleMappings;
        }
        public Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }
        private RuleCollection RebuldDARules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].EXORule != null && rules.CommonRules.Rules[order].EXORule.SOFilters != null && rules.CommonRules.Rules[order].EXORule.SOFilters.Count > 0)
                {
                    reOrder++;
                    var commonRule = rules.CommonRules.Rules[order];
                    var rule = commonRule.EXORule;
                    rule.Id = commonRule.Id;
                    rule.Name = commonRule.Name;
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }
        private Dictionary<int, Rule> GetRuleCollection(bool isNullClassification)
        {
            return ArchiverRuleService.GetEXORuleCollection(GroupId, isNullClassification);
        }
        private Microsoft.Exchange.WebServices.Data.SearchFilter GetSearchFilter(EWSSearchFilterUtility eWSSearchFilterUtility, bool isNullClassfication)
        {
            if (eWSSearchFilterUtility.HasUnSupportCriteria)
            {
                if (isNullClassfication)
                {
                    return null;
                }
                else
                {
                    Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition extendedPropertyDefinition = new Microsoft.Exchange.WebServices.Data.ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, Microsoft.Exchange.WebServices.Data.MapiPropertyType.String);
                    return new Microsoft.Exchange.WebServices.Data.SearchFilter.Exists(extendedPropertyDefinition);
                }
            }
            else
            {
                return eWSSearchFilterUtility.SearchFilter;
            };
        }
        private void ProcessFolder(IExchangeFolder folder)
        {
            logger.Info($"Begin processing folder : {folder.FolderId}.");
            //此处用GetItems 的值更合理，但是很多getitems是异步的，没有办法获取所有值
            JobManagement.ReportManager.IncreaseBase(folder.ItemsCount);
            using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.ProcessFolder", "", true))
            {
                try
                {
                    foreach (var mFolder in GetFolders(folder))
                    {
                        ProcessFolder(mFolder);
                    }
                    ProcessGroupedItems(folder);
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;
                    logger.Error($"Error in process folder : {folder.DisplayFolderPath}, reason : {ex.ToString()}.");
                    EXOCommonUtil.AddDetail(NodeLevel.ExchangeFolder, folder.FolderName, MailboxAddress + folder.DisplayFolderPath, "", "", JobDetailsStatus.Failed, "RM_JM_Tab_DetailFilter_Scan");
                }
            }
        }
        private void ProcessGroupedItems(IExchangeFolder folder)
        {
            using (new CheckJobStopScope()) { }
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
            {
                taskExecutor.StartExecute();
                using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.ProcessGroupedItems", "", true))
                {
                    IEnumerable<IExchangeItemGroup> exchangeItems = null;
                    using (var performance1 = new PerformanceScope("RMEXOEnforceRuleActionBase.GetGroupItems", "", true))
                    {
                        exchangeItems = discover.GetGroupedItems(folder, mSearchFilter);
                    }
                    bool jobNeedStop = false;
                    foreach (var itemGroup in exchangeItems)
                    {
                        taskExecutor.AddTask(async () =>
                        {
                            try
                            {
                                using (new CheckJobStopScope()) { }
                                TenantLocalValue.LogonGroupId = logonGroupId;
                                TenantLocalValue.LogonUserEmail = logonUserEmail;
                                logger.Info($"Begin processing items, items count is : {itemGroup.ItemsCount}.");
                                await ProcessItemsAsync(itemGroup, folder);
                            }
                            catch (JobStopException)
                            {
                                jobNeedStop = true;
                                return;
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                            finally
                            {
                                if (jobNeedStop)
                                {
                                    JobManagement.JobHasStopped = true;
                                    jobNeedStop = false;
                                }
                            }
                        });
                    }
                }
                logger.Info($"ProcessGroupedItems: Add items to task executor finished.");
                if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                {
                    //todo: handle timeout
                    logger.Error($"Time out exception.");
                }
            }
            logger.Info($"ProcessItems finish.");
        }
        private async System.Threading.Tasks.Task ProcessItemsAsync(IExchangeItemGroup itemGroup, IExchangeFolder folder, int retryCount = 0)
        {
            try
            {
                logger.Info($"Begin process grouped item, item count: {itemGroup.ItemsCount}.");
     
                using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.LoadExtendPropery", addToStatistics: true))
                {
                    IExchangeItemBulkHelper bulkHelper = IsSupportGraphApi ?
                            new ExchangeGraphItemBulkHelper(CurrentFolder.MailBoxId, folder.FolderId, CurrentFolder.GetCredential()) :
                            new ExchangeItemBulkHelper(CurrentFolder as ExchangeFolder);

                    await bulkHelper.LoadExtendProperties(itemGroup.Items, IsNullClassification);
                }

                var records = new List<Record>();
                using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.GetRecordsByIds", $"RMEXOEnforceRuleActionBase.GetRecordsByIds.Count:{itemGroup.Items.Count()}", true))
                {
                    records = GetRecordsByNodeIds(AOSMailboxId, itemGroup.Items.Select(i => i.ItemId.ToMd5()).ToList());
                    logger.Info($"Get {records.Count} records from db.");
                }
                foreach (var item in itemGroup.Items)
                {
                    using (new CheckJobStopScope()) { }
                    try
                    {
                        using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.ProcessItem", addToStatistics: true))
                        {
                            Rule rs = null;
                            if (!IsNullClassification)
                            {
                                string value;
                                if (item.TryGetExtendProperty(ExtendProperty.Term,out value))
                                {
                                    RMRuleItemCollection rules = null;
                                    Guid termId;
                                    if (Guid.TryParse(value, out termId))
                                    {
                                        using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.CheckRule", addToStatistics: true))
                                        {
                                            if (TermAndRulesMapping.TryGetValue(termId, out rules))
                                            {
                                                if (rules == null || rules.CommonRules == null || rules.CommonRules.Rules.Count == 0)
                                                {
                                                    logger.Warn($"No rules realted to the item {item.ItemId}.");
                                                }
                                                else
                                                {
                                                    var newRuleCol = RebuldDARules(rules);
                                                    if (newRuleCol.Rules.Count == 0)
                                                    {
                                                        logger.Info($"No DA rules realted to the item: {item.ItemId}.");
                                                        //return null;
                                                    }
                                                    RuleManagement ruleManagement = new RuleManagement(newRuleCol);
                                                    if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                                                    {
                                                        var recordInDB = records.FirstOrDefault(r => r.NodeId == item.ItemId.ToMd5());
                                                        logger.Info($"this item will not check rule {item.ItemId}");
                                                        if (recordInDB != null)
                                                        {
                                                            logger.Info($"this item will not check rule {item.ItemId},and the record not null,termid:{recordInDB.TermId},ruleid:{recordInDB.RuleId}");
                                                            rs = ruleManagement.GetRuleFromRuleCollectionByRuleId(recordInDB.RuleId.ToString());
                                                            if (rs == null)
                                                            {
                                                                logger.Info($"this item will not check rule {item.ItemId},ruleid:{recordInDB.RuleId},can not find the rule in the rule result by rule id");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.CheckItemCriteria", addToStatistics: true))
                                                        {
                                                            rs = ruleManagement.CheckItemCriteria(item);
                                                        }
                                                    }
                                                    //文件已经符合Rule，直接获取action 以及due date
                                                    await ProcessItemAsync(rs, item, termId, records);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Info($"Cannot get term id for item : {item.ItemId}.");
                                    }
                                }
                                else
                                {
                                    logger.Info($"Item : {item.ItemId} does not have term value, so we will not archive it.");

                                }
                            }
                            else
                            {
                                RuleManagement ruleManagement = new RuleManagement(mRuleCollection);
                                if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                                {
                                    var recordInDB = records.FirstOrDefault(r => r.NodeId == item.ItemId.ToMd5());
                                    logger.Info($"NullClassification this item will not check rule {item.ItemId}");
                                    if (recordInDB != null)
                                    {
                                        logger.Info($"NullClassification this item will not check rule {item.ItemId},and the record not null,termid:{recordInDB.TermId},ruleid:{recordInDB.RuleId}");
                                        rs = ruleManagement.GetRuleFromRuleCollectionByRuleId(recordInDB.RuleId.ToString());
                                        if (rs == null)
                                        {
                                            logger.Info($"NullClassification this item will not check rule {item.ItemId},ruleid:{recordInDB.RuleId},can not find the rule in the rule result by rule id");
                                        }
                                    }
                                }
                                else
                                {
                                    using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.CheckItemCriteria", addToStatistics: true))
                                    {
                                        rs = ruleManagement.CheckItemCriteria(item);
                                    }
                                }
                                //文件已经符合Rule，直接获取action 以及due date
                                await ProcessItemAsync(rs, item, Guid.Empty, records);
                            }
                        }
                    }
                    catch (NotImplementedException ex)
                    {
                        JobManagement.HasErrorNode = true;
                        logger.Error($"An error occur in ProcessItem, item id {item?.ItemId}, reason : {ex.ToString()}.");
                        AddDetail(item, JobDetailsStatus.Failed, "", "RM_EXODisposal_Action_Scan", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        JobManagement.HasErrorNode = true;
                        logger.Error($"An error occur in ProcessItem, item id {item?.ItemId}, reason : {ex.ToString()}.");
                        string errorMessage = ex.Message;
                        AddDetail(item, JobDetailsStatus.Failed, "", "RM_EXODisposal_Action_Scan", errorMessage);
                    }
                }
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while sync item in folder:{folder.FolderId} Error:{e.ToString()}");

            }
            finally
            {
                JobManagement.ReportManager.Increase(itemGroup.ItemsCount);
            }
        }

        private bool NeedSkipCurrentRule(Rule rule)
        {
            bool needSkipCurrentRule = false;
            if (rule != null
                && !(rule.EXORule?.spMoveOption?.MoveDestination?.SPUrl != null || rule.spMoveOption?.MoveDestination?.SPUrl != null)
                && !(rule.ExportInfo?.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                && skipRemoveAction
                &&
                (rule.KeepDataOption == (int)KeepDataOption.Delete
                || (rule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument
                || (rule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (rule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                )
            {
                needSkipCurrentRule = true;
            }
            return needSkipCurrentRule;
        }

        private async System.Threading.Tasks.Task ProcessItemAsync(Rule rs, IExchangeItem item, Guid termId, List<Record> records)
        {
            ProcessManualResult processManualResult = new();
            if (rs != null)
            {
                if (NeedSkipCurrentRule(rs))
                {
                    logger.Info("Current object level:{0} fit rule:{1} and SkipRemoveContentAndDestroyAction is true.", item.ItemId, rs.Name);
                    AddDetail(item, JobDetailsStatus.Skipped, rs.Name, "RM_EXODisposal_Action_Scan", "StorageOptimization_SkipRemoveContentAndDestroyAction");
                    return;
                }

                Record recordInDB;
                var isProcessByOwners = string.IsNullOrEmpty(rs.WorkflowId);
                bool needUpdateRecord = false;

                using (var performance0 = new PerformanceScope("RMEXOEnforceRuleActionBase.GetDBRecord", addToStatistics: true))
                {
                    recordInDB = records.FirstOrDefault(r => r.NodeId == item.ItemId.ToMd5());
                }

                if (recordInDB != null)
                {
                    var customColumns = GetEXOCustomMetadata(item, recordInDB);
                    if (customColumns != null && customColumns.Count > 0)
                    {
                        recordInDB.CustomColumnDic = customColumns;
                        needUpdateRecord = true;
                    }
                }

                if (!rs.IsManualApproval)
                {
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                    {
                        logger.Info($"Item:{item.ItemId} not match manual rule,and it is process ApprovalDatasOnly");
                        if (needUpdateRecord)
                        {
                            CosmosDBManualDataUpdater.Add(recordInDB);
                        }
                        return;
                    }

                    SaveItemToLiteDB(item, rs, termId);

                    if (recordInDB != null && recordInDB.IsManualSynced && recordInDB.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                    {
                        logger.Info($"Item:{item.ItemId} not match manual rule, New rule id:{rs.Id}");
                        recordInDB.RemoveManualFields();
                        CosmosDBManualDataUpdater.Add(recordInDB);
                    }
                    else if (needUpdateRecord)
                    {
                        CosmosDBManualDataUpdater.Add(recordInDB);
                    }

                    AddDetail(item, JobDetailsStatus.Successful, rs.Name, "RM_EXODisposal_Action_Scan");
                }
                else
                {
                    if (recordInDB != null)
                    {
                        recordInDB.ManualFullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                        if (recordInDB.RuleId.ToString() == rs.Id)
                        {
                            if (recordInDB.ManualExtendTime >= DateTime.UtcNow.Ticks)
                            {
                                logger.Info($"Item:{recordInDB.LeafName} match manual rule, but is extend time data.");
                                rejectEmailId.Add(recordInDB.Id);
                                if (needUpdateRecord)
                                {
                                    CosmosDBManualDataUpdater.Add(recordInDB);
                                }
                                return;
                            }

                            if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                            {
                                logger.Info($"Item:{item.ItemId} match manual rule, and approve status is approved.");
                                SaveItemToLiteDB(item, rs, termId);
                                if (needUpdateRecord)
                                {
                                    CosmosDBManualDataUpdater.Add(recordInDB);
                                }
                                AddDetail(item, JobDetailsStatus.Successful, rs.Name, "RM_EXODisposal_Action_Scan");
                                return;
                            }
                            else if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                            {
                                logger.Info($"Item:{item.ItemId} match manual rule,and it is process ApprovalDatasOnly");
                                if (needUpdateRecord)
                                {
                                    CosmosDBManualDataUpdater.Add(recordInDB);
                                }
                                return;
                            }
                            else if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                            {
                                logger.Info($"Item:{item.ItemId} match manual rule, and approve status is rejected.");
                                rejectEmailId.Add(recordInDB.Id);
                                mConfiguration.AddHistory(recordInDB);
                                recordInDB.ManualModifiedTime = item.Modified.Ticks;
                                processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners);
                            }
                            else if (recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                            {
                                recordInDB.ManualModifiedTime = item.Modified.Ticks;
                                processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners);
                            }
                            else
                            {
                                logger.Info($"Item:{item.ItemId} match manual rule, and approve status is {recordInDB.ManualApprovedStatus}.");
                                if (needUpdateRecord)
                                {
                                    CosmosDBManualDataUpdater.Add(recordInDB);
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"Item:{item.ItemId} match rule id changed. Old rule id:{recordInDB.RuleId.ToString()} New rule id:{rs.Id}");
                            recordInDB.RuleId = new Guid(rs.Id);
                            recordInDB.ManualExtendCount = 0;
                            recordInDB.ManualModifiedTime = item.Modified.Ticks;
                            processManualResult = await InnerProcessWaitingForApprovalRecordAsync(recordInDB, isProcessByOwners);
                        }
                    }
                    else
                    {
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            logger.Info($"Item:{item.ItemId} match manual rule,and it is process ApprovalDatasOnly");
                        }
                        else
                        {
                            var newRecord = GenerateManualRecord(item, rs);
                            newRecord.ManualFullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                            newRecord.ManualModifiedTime = item.Modified.Ticks;
                            newRecord.CustomColumnDic = GetEXOCustomMetadata(item, newRecord);
                            processManualResult = await InnerProcessWaitingForApprovalRecordAsync(newRecord, isProcessByOwners);
                        }
                    }

                    if (recordInDB != null && recordInDB.ManualExtendTime > mConfiguration.ArchiverUNCTime.Ticks && recordInDB.RuleId.ToString() == rs.Id)
                    {
                        logger.Info($"Item:{item.ItemId} match manual rule, but is extend time data.");
                    }
                    else
                    {
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            logger.Info($"Item match manual rule,and it is process ApprovalDatasOnly");
                        }
                        else if (processManualResult?.IsSuccess ?? false)
                        {
                            AddDetail(item, JobDetailsStatus.Skipped, rs.Name, "RM_EXODisposal_Action_Scan", "RM_JM_FSFileWaitingForApproval");
                        }
                        else
                        {
                            if (processManualResult?.ErrorType == ProcessManualErrorType.NoOwnerError)
                            {
                                AddDetail(item, JobDetailsStatus.Failed, rs.Name, "RM_EXODisposal_Action_Scan", "RM_MA_NoRecordOwner");
                                JobManagement.HasErrorNode = true;
                            }
                            else
                            {
                                logger.Warn("Unsupported exception type");
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Debug($"Item:{item.ItemId} not match rule");
                Record recordInDB;
                bool needUpdateRecord = false;

                using (var performance0 = new PerformanceScope("RMEXOEnforceRuleActionBase.GetDBRecord", addToStatistics: true))
                {
                    recordInDB = records.FirstOrDefault(r => r.NodeId == item.ItemId.ToMd5());
                }

                if (recordInDB != null)
                {
                    var customColumns = GetEXOCustomMetadata(item, recordInDB);
                    if (customColumns != null && customColumns.Count > 0)
                    {
                        recordInDB.CustomColumnDic = customColumns;
                        needUpdateRecord = true;
                    }
                }

                if (WrapperConfiguration.IsProcessApprovalDatasOnly && recordInDB != null && recordInDB.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                {
                    logger.Info($"Item:{item.ItemId} not match rule, and it is process ApprovalDatasOnly and it is approved,will set status.");
                    recordInDB.RemoveManualFields();
                    CosmosDBManualDataUpdater.Add(recordInDB);
                }
                else if (recordInDB != null && recordInDB.IsManualSynced && recordInDB.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd && !WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    recordInDB.RemoveManualFields();
                    CosmosDBManualDataUpdater.Add(recordInDB);
                }
                else if (needUpdateRecord)
                {
                    CosmosDBManualDataUpdater.Add(recordInDB);
                }
            }
        }

        private async Task<ProcessManualResult> InnerProcessWaitingForApprovalRecordAsync(Record recordInDB, bool isProcessByOwners)
        {
            ProcessManualResult result = new();
            var newRec = await mConfiguration.AddManualFieldsAsync(recordInDB);
            if (isProcessByOwners && newRec.ManualReviewer.Length == 0)
            {
                result.IsSuccess = false;
                result.ErrorType = ProcessManualErrorType.NoOwnerError;

            }
            else
            {
                CosmosDBManualDataUpdater.Add(newRec);
            }
            return result;
        }

        private void SaveItemToLiteDB(IExchangeItem item, Rule rs, Guid termId)
        {
            Dictionary<string, string> itemProperties = new Dictionary<string, string>();
            try
            {
                itemProperties = item.GetProperties();
            }
            catch (Exception e)
            {
                logger.Error($"there is some thing wrong with get item properties,error:{e.ToString()}");
            }
            EXOArchiveData archiveData = new EXOArchiveData()
            {
                ItemId = item.ItemId,
                RuleId = rs.Id,
                ParentFolderId = item.ParentFolderId,
                FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                TermId = termId.ToString(),
                ItemProperties = itemProperties
            };
            EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(JobManagement.SubJobId)).Insert(new List<EXOArchiveData>() { archiveData });
            JobManagement.ReportManager.IncreaseBase(1);
        }
        private Record GenerateManualRecord(IExchangeItem item, Rule rule)
        {
            using (new RA.Common.PerformanceScope("RMEXOEnforceRuleActionBase.GenerateManualRecord"))
            {
                Record rec = new Record();
                try
                {
                    _semaphore.WaitAsync().ExecuteAsyncTask();
                    var itemId = item.ItemId.ToMd5();
                    RecordMetaInfo metaInfo = new RecordMetaInfo
                    {
                        FileSize = item.ItemSize,
                        AttachmentNames = item.AttachmentNames,
                    };
                    var jsonStr = JsonConvert.SerializeObject(metaInfo);
                    rec = new Record()
                    {
                        Id = AvePoint.RA.RAExchange.Common.IDGenerator.GetRecordId(MailboxAddress, item.ItemId),
                        ScopeId = AOSMailboxId,
                        NodeId = itemId,
                        DirPath = item.ItemPath,
                        FullPath = item.ItemPath,
                        LeafName = item.ItemName,
                        ExtensionForFile = "msg",//Confirm in Demo with Moses, we use msg here
                        AveSiteId = AOSMailboxId.ToString(),
                        WebId = Guid.Empty,
                        ListId = Guid.Empty,
                        ItemId = itemId,
                        CollectTime = DateTime.UtcNow.Ticks,
                        TimeCreated = item.SendDateUTC.Ticks,
                        NodeType = (int)NodeLevel.ExchangeOnlineItem,

                        FolderId = item.ParentFolderId.ToMd5(),//to do next validate folder id
                                                               //folderRowId = aveItem.Folder.Item.ID,
                        MetaInfo = jsonStr,
                        HoldStatus = false,
                        RelatedRecords = "",
                        RelatedRecordsCount = 0,
                        SourceFlag = (int)SourceFlag.Exchange,
                        CreatedBy = item.SenderDisplayName, //item.Sender,
                        ModifiedBy = item.ModifiedBy,
                        ManualModifiedTime = item.Modified.Ticks,
                        DeclareAsRecord = false,
                        TimeModified = item.Modified.Ticks,
                        ItemRowId = 0,
                        RuleId = rule != null ? new Guid(rule.Id) : Guid.Empty,
                        RuleLevel = rule != null ? (int)rule.PolicyLevel : 0,
                        RecordStatus = (int)RMRecordStatus.ManualPreSync,
                        //RecordOwner = ManualApproveDao.GetLastReviewedUserIds(AOSMailboxId, itemId),

                        ExternalId = item.ItemId,
                        EmailAddress = MailboxAddress,
                        SendTo = item.DisplayTo,
                        ContainerId = GroupId.ToString()
                    };
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while generate record for item: {item.ItemId}, error: {ex.ToString()}");
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
                return rec;
            }
        }
        private EXODisposalAction GetRuleAction(Rule rule)
        {
            //根据不同的RuleAction 实例化不同的Controller对象。 不同的Controller 继承了IBackupController 方法，并且实现了Process 方法。
            //这样每次过来的节点，就能通过接口分发到对应的Process 方法，然后Process 方法内部进行具体处理
            //rule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return EXODisposalAction.Export;
            }
            else if ((rule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                return EXODisposalAction.Tag;
            }
            else if (rule.EXORule != null && rule.EXORule.MoveToRecordCenterAndDelareSetting != null)
            {
                return EXODisposalAction.Move;
            }
            else
            {
                return EXODisposalAction.Remove;
            }
        }

        private EXODisposalAction GetEXORuleAction(Rule exorule)
        {
            //根据不同的RuleAction 实例化不同的Controller对象。 不同的Controller 继承了IBackupController 方法，并且实现了Process 方法。
            //这样每次过来的节点，就能通过接口分发到对应的Process 方法，然后Process 方法内部进行具体处理
            //rule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
            if (exorule.ExportInfo != null && exorule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return EXODisposalAction.Export;
            }
            else if ((exorule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                return EXODisposalAction.Tag;
            }
            else if (exorule != null && exorule.MoveToRecordCenterAndDelareSetting != null)
            {
                return EXODisposalAction.Move;
            }
            else
            {
                return EXODisposalAction.Remove;
            }
        }

        private string ConvertEXODisposalAction2RuleAction(EXODisposalAction action)
        {
            return action switch
            {
                EXODisposalAction.Tag => "Keep",
                EXODisposalAction.Move => "Move",
                EXODisposalAction.Export => "Export",
                EXODisposalAction.Remove => "Delete",
                _ => ""
            };
        }

        private string ConvertEXODisposalAction2RuleActionI18N(EXODisposalAction action)
        {
            return action switch
            {
                EXODisposalAction.Tag => "RM_EXODisposal_Action_Keep",
                EXODisposalAction.Move => "RM_EXODisposal_Action_Move",
                EXODisposalAction.Export => "RM_EXODisposal_Action_Export",
                EXODisposalAction.Remove => "RM_EXODisposal_Action_Delete",
                _ => ""
            };
        }

        private void AddDetail(IExchangeItem item, JobDetailsStatus status, string ruleName, string action, string errorMessage = null)
        {
            EXOCommonUtil.AddDetail(NodeLevel.ExchangeOnlineItem, item.ItemName, MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R")
                , ruleName, "", status, action,  errorMessage);
        }
        private List<Record> GetRecordsByNodeIds(Guid scopeId, List<Guid> nodeIds)
        {
            var records = new List<Record>();
            try
            {
                records = ExplorerDao.QueryAll(r => r.ScopeId == scopeId && nodeIds.Contains(r.NodeId)).ToList();
            }
            catch (Exception ex)
            {
                logger.Warn($"Cannot get records by ids, scope id is : {scopeId.ToString()}, reason : {ex.ToString()}.");
            }
            return records;
        }
        private List<string> GetAllRules()
        {
            return EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(JobManagement.SubJobId)).GetAllRules();
        }

        private string GetTermNameByRuleId(string ruleId)
        {
            string termName = string.Empty;
            var termId = EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(JobManagement.SubJobId)).GetTermIdByRuleId(ruleId);
            if (!string.IsNullOrWhiteSpace(termId) && Guid.TryParse(termId, out Guid termUniqueId))
            {
                var term = TermDao.GetRMTermByUniqueId(termUniqueId);
                if (term != null)
                {
                    termName = term.Name;
                }
            }
            return termName;
        }
        private void ArchiveDataForRule(string ruleId)
        {
            try
            {
                mConfiguration.CurrentRule = mRuleCollection.Rules.Select(r => r.Value).Where(r => r.Id == ruleId).FirstOrDefault();
                logger.Info($"Begin to archive data for rule:{mConfiguration.CurrentRule.Name} Exo rule exist:{mConfiguration.CurrentRule.EXORule != null}");
                if (GetEXORuleAction(mConfiguration.CurrentRule.EXORule) == EXODisposalAction.Remove && skipRemoveAction)
                {
                    logger.Info($"Current rule is remove rule and SkipRemoveContentAndDestroyAction option is checked, so skip current rule.");
                    return;
                }
                mConfiguration.RuleName = mConfiguration.CurrentRule.Name;
                mConfiguration.SubJobId = JobManagement.SubJobId;
                InitRuleInfo(mConfiguration.CurrentRule);
                if (IsSupportGraphApi)
                {
                    InitBackupGraphController(mConfiguration.CurrentRule);
                }
                else
                {
                    InitBackupController(mConfiguration.CurrentRule);

                }
                EXOLiteDBWrapper eXOLiteDBWrapper = EXOLiteDBWrapper.CreateInstance(EXOPathUtil.GetDisposalDueRecordDBPath(JobManagement.SubJobId));
                int index = 0;
                int pageSize = 1000;
                bool hasMore = true;
                List<EXOArchiveData> records = null;
                do
                {
                    using (new PerformanceScope("RMEXOEnforceRuleActionBase.QueryAllByPage", addToStatistics: true))
                    {
                        records = eXOLiteDBWrapper.QueryAllByPage(index, pageSize, ruleId);
                    }
                    if (records != null && records.Count > 0)
                    {
                        index++;
                        hasMore = true;
                        ArchiveItems(records);
                    }
                    else
                    {
                        hasMore = false;
                    }
                } while (hasMore);
                logger.Info($"Get exo disposal records for rule:{ruleId} finished. Count:{index}");
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                JobFinish();
            }
        }
        private void ArchiveItems(List<EXOArchiveData> records)
        {
            using (new CheckJobStopScope()) { }
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            int threadCount = backupController is ExchangeMoveToController or ExchangeGraphMoveToController ? 1 : mThreadCount;
            bool isJobStopped = false;
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(threadCount))
            {
                taskExecutor.StartExecute();
                using (var performance = new PerformanceScope("RMEXOEnforceRuleActionBase.ArchiveItem", "", true))
                {
                    foreach (var record in records)
                    {
                        taskExecutor.AddTask(() =>
                        {
                            try
                            {
                                using CheckJobStopScope jScope = new();
                                TenantLocalValue.LogonGroupId = logonGroupId;
                                TenantLocalValue.LogonUserEmail = logonUserEmail;
                                ArchiveItem(record);
                            }
                            catch (JobStopException)
                            {
                                isJobStopped = true;
                            }
                        });
                    }
                }
                logger.Info($"ArchiveItems: Add items to task executor finished.");
                if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                {
                    //todo: handle timeout
                    logger.Error($"Time out exception.");
                }
            }
            if (isJobStopped)
            {
                throw new JobStopException("This Job is stopped.");
            }
            logger.Info($"ProcessItems finish.");
        }
        public void InitRuleInfo(Rule rule)
        {
            if (rule == null) { throw new Exception("Rule is null."); }
            if (rule.EXORule.MoveToRecordCenterAndDelareSetting?.OperateDataMode == OperatingSharePointDataMode.MoveToRecordCenterAndDelare)
            {
                mConfiguration.isRecordManagerJob = true;
                mConfiguration.appendItemMapping.RemoveAll();
                if (!CheckEXORecordManagerDestUrl(rule))
                {
                    logger.Error("List Url Do not Available,Rule Name is {0}", rule.Name);
                    //mConfiguration.ProgressDto.HasErrorNode = true;
                    throw new Exception(string.Format("Destination url is unavailable, rule name is : {0}", rule.Name));
                }
                //InitRecordRestoreConfig();
            }
            else
            {
                mConfiguration.isRecordManagerJob = false;
            }
            if (rule.ExportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None && rule.ExportInfo != null)
            {
                InitExportType(rule);
            }
            else
            {
                EXOExportBefArcInfo = null;
            }
        }
        private bool CheckEXORecordManagerDestUrl(Rule rule)
        {
            bool recordUrlAvailable = false;
            string desUrl = rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
            desUrl = HttpUtility.UrlDecode(desUrl);
            var remoteSite = RABrowserClient.GetRemoteSiteCollectionByListUrl(desUrl);
            var destBposInfo = rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo;
            if (string.IsNullOrEmpty(destBposInfo.UserAccountInfo.AdminUrl))
            {
                destBposInfo.UserAccountInfo.AdminUrl = WebUtil.GetSPAdminUrl(remoteSite.url, remoteSite.TenantId);
            }
            AveBPOSAccountInfo user = rule.EXORule.MoveToRecordCenterAndDelareSetting.DestinationLocation.BposInfo.ConvertToAveBPOSAccountInfo();
            mConfiguration.recordManagerRestoreOMFactory = AveObjectModelFactory.CreateObjectModelFactory(remoteSite.url, user, AveContextKind.ClientObjectModel);
            logger.Info($"CheckRecordDesUrl: Init BPOS Factory Successful. Site URL:{remoteSite.url}, Admin URL:{user.AdminUrl}");
            IAveSiteServiceHelper siteServiceHelper = mConfiguration.recordManagerRestoreOMFactory.CreateSiteServiceHelper();
            string siteUrl = siteServiceHelper.TryToRectifySiteUrl(desUrl, user);
            try
            {
                using (IAveSite restoreSite = mConfiguration.recordManagerRestoreOMFactory.CreateSite(siteUrl))
                {
                    try
                    {
                        Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                        if (restoreSite.Features[mRecordFeatureId] == null)
                        {
                            restoreSite.Features.Add(mRecordFeatureId, true);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        //mConfiguration.JobReportDto.summaryRecordManagerComments = "StorageOptimization_SOARSORecordManagerNoInPlaceRecrdFeature";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                        throw;
                    }
                    try
                    {
                        using (IAveWeb restoreWeb = restoreSite.OpenWeb())
                        {
                            IAveList restoreList;
                            if (desUrl.Contains("#/"))
                            {
                                //restoreList = restoreWeb.GetListFromUrl(desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                                desUrl = desUrl.Substring(desUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2);
                            }
                            restoreList = restoreWeb.GetList(desUrl);
                            //int listTemplate = (int)restoreList.BaseTemplate;
                            if (!(restoreList.BaseTemplate == AveListTemplateType.DocumentLibrary || restoreList.BaseTemplate == AveListTemplateType.RecordLib || restoreList.BaseTemplate == AveListTemplateType.OneDriveDocumentLibrary))
                            {
                                logger.Error("List Template Error :{0}", restoreList.BaseTemplate.ToString());
                                throw new Exception("List Template Error");
                            }
                            logger.Info("List Auto Check Out Property is:{0}", restoreList.ForceCheckout.ToString());
                        }
                    }
                    catch (Exception listException)
                    {
                        logger.Error("Check List Url error,Message:{0}", listException.ToString());
                        //mConfiguration.JobReportDto.summaryRecordManagerComments = "StorageOptimization13_SOARSORecordManagerLibraryNotExist";
                        throw;
                    }
                }
                recordUrlAvailable = true;
            }
            catch (Exception ex)
            {
                logger.Error("Can not get destination Site, Des url : {0}, Reason: {1}", desUrl, ex.ToString());
            }
            return recordUrlAvailable;
        }
        private void InitBackupController(Rule rule)
        {
            //根据不同的RuleAction 实例化不同的Controller对象。 不同的Controller 继承了IBackupController 方法，并且实现了Process 方法。
            //这样每次过来的节点，就能通过接口分发到对应的Process 方法，然后Process 方法内部进行具体处理
            //rule.ExportInfo.exportSPDataOption = ExportSPDataOption.ExportWithoutArchive;
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                backupController = new ExchangeExportController(mConfiguration, EXOExportBefArcInfo);
            }
            else if ((rule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                backupController = new ExchangeTagController(mConfiguration, rule, EXOExportBefArcInfo);
            }
            else if (rule.EXORule != null && rule.EXORule.MoveToRecordCenterAndDelareSetting != null)
            {
                backupController = new ExchangeMoveToController(mConfiguration);
            }
            else
            {
                backupController = new ExchangeDeleteController(mConfiguration, EXOExportBefArcInfo);
            }
        }
        private void InitBackupGraphController(Rule rule)
        {
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                backupController = new ExchangeGraphExportController(mConfiguration, EXOExportBefArcInfo);
            }
            else if ((rule.KeepDataOption & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
            {
                backupController = new ExchangeGraphTagController(mConfiguration, rule, EXOExportBefArcInfo);
            }
            else if (rule.EXORule != null && rule.EXORule.MoveToRecordCenterAndDelareSetting != null)
            {
                backupController = new ExchangeGraphMoveToController(mConfiguration);
            }
            else
            {
                backupController = new ExchangeGraphDeleteController(mConfiguration, EXOExportBefArcInfo);
            }
        }
        private void InitExportType(Rule rule)
        {
            EXOExport = null;
            EXOExportPathGeneratorBase generator = null;
            if (rule != null && rule.ExportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                InitVaultState(ref generator, rule);
                EXOExportBefArcInfo = new EXOExportBeforeArcInfo()
                {
                    EXOExport = EXOExport,
                    EXOExportPathGenerator = generator
                };
            }
        }

        private void InitVaultState(ref EXOExportPathGeneratorBase generator, Rule rule)
        {
            EXOExportFactory factory = new EXOExportFactory();
            ExportTypeValue vaultExportType = rule.ExportType;
            var physicalDeviceId = string.Empty; 
            if (rule.ExportInfo is { newOptionsOfExportInfo: true })
            {
                physicalDeviceId = rule.ExportInfo.exportLocationId;
            }
            else
            {
                var profile = SettingProfileDao.LoadByType((int)SettingProfilesType.ExportLocationDevice);
                if (profile == null)
                {
                    throw new Exception("RM_RDM_Rule_ConfigureExportLocation");
                }
                physicalDeviceId = profile.Settings;
            }
            var device = StorageDeviceService.GetStorageDeviceById(physicalDeviceId, needDecryptSecert: true);
            PhysicalDeviceDto physicalDto = null;
            SharePointLocationDto spoDto = null;
            AveBPOSAccountInfo accountInfoOfDestinationSpo = null;
            if (device != null)
            {
                physicalDto = new()
                {
                    ConnectionString = device.ConnectionString,
                    Type = device.Type
                };
            }
            if (physicalDto == null)
            {
                logger.Info("Using export to sharepoint library.");
                var (spoLibrary, accountInfo) = GetSharePointLibraryAndAccount().GetAwaiter().GetResult();
                spoDto = spoLibrary;
                accountInfoOfDestinationSpo = accountInfo;
            }
            //physicalDto = rule.StoragePolicyDto.PrimaryStorage.PhysicalDrives[0];
            string globalSettingColumnName = GetTermNameByRuleId(rule.Id);
            logger.Info("Vault Export Type is: {0}.", vaultExportType.ToString());
            byte[] exportEncryptionKeyBytes = null;
            byte[] exportEncryptionIVBytes = null;
            var veoType = BackgroundSettings.GetInstance().VEOType;
            var veoTypeV3 = BackgroundSettings.GetInstance().VEOV3Type;
            
            if (mConfiguration.HasUpgradeVEOV3 && vaultExportType == ExportTypeValue.VEO && !string.IsNullOrEmpty(veoTypeV3))
            {
                logger.Info("Export Type will change to :{0}Export.", veoTypeV3);
                var exportFormat = $"{EXO}{veoTypeV3}";
                byte[] veoContent = rule.VEOContent;
                byte[] veoHistory = rule.VEOHistory;
                EXOExport = physicalDto != null 
                            ? factory.Create(physicalDto, mConfiguration.SubJobId, exportFormat, veoContent, veoHistory, rule.ArchiverSetting, rule.ExportDataEncryptionKey)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.SubJobId, exportFormat, veoContent, veoHistory, rule.ArchiverSetting, rule.ExportDataEncryptionKey);
                generator = new EXOVEOExportPathGenerator(string.Empty, physicalDto?.Location, globalSettingColumnName);
            }
            else if(vaultExportType == ExportTypeValue.VEO && !string.IsNullOrEmpty(veoType))
            {
                logger.Info("Export Type will change to :{0}Export.", veoType);
                #region for debug VEO
                //byte[] test = File.ReadAllBytes(@"C:\VEO\EXOFileVEO.xml");
                //using (MemoryStream ms = new MemoryStream(test))
                //{
                //    mConfiguration.BackupRequest.Rules[rule.Id].FileVEO = ms.ToArray();
                //}
                //byte[] test1 = File.ReadAllBytes(@"C:\VEO\EXOManifestVEO.xml");
                //using (MemoryStream ms = new MemoryStream(test1))
                //{
                //    mConfiguration.BackupRequest.Rules[rule.Id].ManifestVEO = ms.ToArray();
                //}
                //byte[] test2 = File.ReadAllBytes(@"C:\VEO\EXORecordVEO.xml");
                //using (MemoryStream ms = new MemoryStream(test2))
                //{
                //    mConfiguration.BackupRequest.Rules[rule.Id].RecordVEO = ms.ToArray();
                //}
                #endregion
                byte[] fileVEO = rule.FileVEO;
                byte[] recordVEO = rule.RecordVEO;
                byte[] manifestVEO = rule.ManifestVEO;
                var recordsEncryptionKey = rule.ExportDataEncryptionKey;
                var recordsEncryptionIV = rule.ExportDataEncryptionIV;
                if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                {
                    logger.Info("Export data encryption is enabled.");
                    exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                    exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                }
                EXOExport = physicalDto != null
                            ? factory.Create(physicalDto, mConfiguration.SubJobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + veoType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.SubJobId, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + veoType, true), fileVEO, recordVEO, manifestVEO, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                generator = new EXOVEOExportPathGenerator(string.Empty, physicalDto?.Location, globalSettingColumnName);
            }
            else if (vaultExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NAA)
            {
                #region for debug NAA
                //byte[] test = File.ReadAllBytes(@"C:\EXO NAA Configuration File.xml");
                //using (MemoryStream ms = new MemoryStream(test))
                //{
                //    mConfiguration.BackupRequest.Rules[rule.Id].NAAConfigFile = ms.ToArray();
                //}
                #endregion
                var recordsEncryptionKey = rule.ExportDataEncryptionKey;
                var recordsEncryptionIV = rule.ExportDataEncryptionIV;
                if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                {
                    logger.Info("Export data encryption is enabled.");
                    exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                    exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                }
                EXOExport = physicalDto != null 
                            ? factory.Create(physicalDto, mConfiguration.SubJobId, rule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + vaultExportType.ToString(), true), rule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.SubJobId, rule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + vaultExportType.ToString(), true), rule.NAAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                generator = new EXONAAExportPathGenerator(string.Empty, physicalDto?.Location, globalSettingColumnName);
            }
            else if (vaultExportType == AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.NARA)
            {
                #region for debug NARA
                //byte[] test = File.ReadAllBytes(@"C:\EXO NARA Configuration File.xml");
                //using (MemoryStream ms = new MemoryStream(test))
                //{
                //    mConfiguration.BackupRequest.Rules[rule.Id].NARAConfigFile = ms.ToArray();
                //}
                #endregion
                var recordsEncryptionKey = rule.ExportDataEncryptionKey;
                var recordsEncryptionIV = rule.ExportDataEncryptionIV;
                if (!string.IsNullOrWhiteSpace(recordsEncryptionKey) && !string.IsNullOrWhiteSpace(recordsEncryptionIV))
                {
                    logger.Info("Export data encryption is enabled.");
                    exportEncryptionKeyBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionKey)));
                    exportEncryptionIVBytes = Encoding.UTF8.GetBytes(CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(recordsEncryptionIV)));
                }
                EXOExport = physicalDto != null 
                            ? factory.Create(physicalDto, mConfiguration.SubJobId, rule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + vaultExportType.ToString(), true), rule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes)
                            : factory.Create(spoDto, accountInfoOfDestinationSpo, spoDto.SiteUrl, mConfiguration.SubJobId, rule.DisposalClass, (VaultExportFormat)Enum.Parse(typeof(VaultExportFormat), EXO + vaultExportType.ToString(), true), rule.NARAConfigFile, exportEncryptionKeyBytes, exportEncryptionIVBytes);
                generator = new EXONARAExportPathGenerator(string.Empty, physicalDto?.Location, globalSettingColumnName);
            }
        }

        private async Task<(SharePointLocationDto, AveBPOSAccountInfo)> GetSharePointLibraryAndAccount()
        {
            try
            {
                string listUrl = mConfiguration.CurrentRule.EXORule.ExportInfo.spMoveOption.MoveDestination.SPUrl;
                listUrl = HttpUtility.UrlDecode(listUrl);
                GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection remoteSiteCollection =
                    RABrowserClient.GetRemoteSiteCollectionByListUrl(listUrl);
                if (remoteSiteCollection == null)
                {
                    throw new Exception("RM_SO_MoveAction_DestinationSiteNotExist");
                }

                var siteUrl = remoteSiteCollection.url;
                var user = await CommonPoolUserUtil.GetBPOSInfoAsync(remoteSiteCollection);
                var recordManagerRestoreOMFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);

                var recordUrlAvailable = GetCorrectRecordDesUrl(listUrl, recordManagerRestoreOMFactory, siteUrl);
                if (string.IsNullOrEmpty(recordUrlAvailable))
                {
                    logger.Info(
                        "CheckRecordDesUrl is illegal ,this error form MoveAction calss MoveActionFun function");
                    return (null,null);
                }

                //mConfiguration.SiteUrl = recordUrlAvailable;
                logger.Info($"GetcorrectRecordDesUrl: {recordUrlAvailable}");

                listUrl = recordUrlAvailable;

                SharePointLocationDto result = new();
                var site = recordManagerRestoreOMFactory.CreateSite(siteUrl);
                var web = site.OpenWeb(site.GetWebServerRelativeUrl(listUrl));
                var list = web.GetList(listUrl);
                var currentIAveFolder = web.GetFolder(list.RootFolder.ServerRelativeUrl);
                if (!currentIAveFolder.Exists)
                {
                    throw new Exception(string.Format("Folder Not Exists :{0}", currentIAveFolder.Name));
                }

                var jobFolder = currentIAveFolder.ServerRelativeUrl + "/" + mConfiguration.SubJobId;
                var newJobFolder = currentIAveFolder.Folders.Add(jobFolder);
                result.ParentFolderId = newJobFolder.UniqueId;
                result.ParentWebUrl = web.ServerRelativeUrl;
                result.JobFolder = newJobFolder;
                result.SiteUrl = siteUrl;
                return (result, user);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            return (null,null);
        }
        
        private string GetCorrectRecordDesUrl(string listUrl, AveObjectModelFactory recordManagerRestoreOMFactory, string siteUrl)
        {
            string returnValue = string.Empty;
            listUrl = HttpUtility.UrlDecode(listUrl);
            try
            {
                using (IAveSite restoreSite = recordManagerRestoreOMFactory.CreateSite(siteUrl))
                {
                    try
                    {
                        Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
                        if (restoreSite.Features[mRecordFeatureId] == null)
                        {
                            restoreSite.Features.Add(mRecordFeatureId, true);
                            using (IAveSite checkSite = recordManagerRestoreOMFactory.CreateSite(siteUrl))
                            {
                                ArchiverCommonStaticMethod.UpdateSiteRecordDeclarationSettings(checkSite, ScheduleConfiguration.BlockDeleteEdit);
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        logger.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
                        throw;
                    }
                    try
                    {
                        var webUrl = restoreSite.GetWebServerRelativeUrl(listUrl);
                        using (IAveWeb restoreWeb = restoreSite.OpenWeb(webUrl))
                        {
                            IAveList restoreList;
                            if (listUrl.Contains("#/"))
                            {
                                restoreList = restoreWeb.GetListFromUrl(listUrl.Substring(listUrl.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
                            }
                            else
                            {
                                restoreList = restoreWeb.GetList(listUrl);
                            }
                            //int listTemplate = (int)restoreList.BaseTemplate;
                            if (!(restoreList.BaseTemplate == AveListTemplateType.DocumentLibrary
                                || restoreList.BaseTemplate == AveListTemplateType.RecordLib
                                || restoreList.BaseTemplate == AveListTemplateType.OneDriveDocumentLibrary))
                            {
                                logger.Error("List Template Error :{0}", restoreList.BaseTemplate.ToString());
                                throw new Exception("List Template Error");
                            }
                            returnValue = restoreList.FullUrl();
                            logger.Info("List Auto Check Out Property is:{0}", restoreList.ForceCheckout.ToString());
                        }
                    }
                    catch (Exception listException)
                    {
                        logger.Error("Check List Url error,Message:{0}", listException.ToString());
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Can not get destination Site, Des url : {0}, Reason: {1}", listUrl, ex.ToString());
            }

            return returnValue;
        }
        
        private void ArchiveItem(EXOArchiveData data)
        {
            try
            {
                backupController.Process(data);
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while archive item:{data.ItemId} Error:{e.ToString()}");
                JobManagement.HasErrorNode = true;
            }
            finally
            {
                SOProgressScAndFileStatistic.Instance()?.IncreaseFileCount(1, (int)SharePointItemType.DOCUMENT);
                JobManagement.ReportManager.Increase();
            }
        }

        public void JobFinish()
        {
            if (EXOExport != null)
            {
                if (mConfiguration.CurrentRule.ExportType == ExportTypeValue.NAA)
                {
                    logger.Info("begin build naa metadata file");
                    List<CsvMetaData> metadatas = new List<CsvMetaData>();
                    metadatas.AddRange(EXOExport.GetCSVMetadata());
                    if (EXOExport.GetCSVMetadata().Count > 0)
                    {
                        EXOExport.ExtensionMethod(metadatas);
                    }
                    logger.Info("build naa metadata file success.metadatas Count:{0}.", metadatas.Count);
                    EXOExport.Dispose();

                }
                else if (mConfiguration.CurrentRule.ExportType == ExportTypeValue.NARA)
                {
                    logger.Info("begin build nara metadata file");
                    List<CsvMetaData> metadatas = new List<CsvMetaData>();
                    metadatas.AddRange(EXOExport.GetCSVMetadata());
                    if (EXOExport.GetCSVMetadata().Count > 0)
                    {
                        EXOExport.ExtensionMethod(metadatas);
                    }
                    logger.Info("build nara metadata file success.metadatas Count:{0}.", metadatas.Count);
                    EXOExport.Dispose();
                }
                else if (mConfiguration.CurrentRule.ExportType == ExportTypeValue.VEO)
                {
                    RMRunningJobRuleMappingDao.AddJobMappingsForVEOMerge(TenantLocalValue.LogonGroupId, mConfiguration.SubJobId.Substring(0, mConfiguration.SubJobId.IndexOf('_')));
                    if(mConfiguration.HasUpgradeVEOV3)
                    {
                        EXOExport.ExtensionMethod(false);
                    }
                    else
                    {
                        EXOExport.ExtensionMethod(mConfiguration.CurrentRule.Name, BackgroundSettings.GetInstance().ManifestXmlSize);
                    }
                    EXOExport.Dispose();
                }
            }
        }
        #endregion

        #region Index metadata column

        //private async System.Threading.Tasks.Task LoadCustomIndexMetadataAsync()
        //{
        //    try
        //    {
        //        if (!RMKeyValueDao.TryGetBoolValue(AvePoint.RA.Contract.Common.KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnabled) || !isEnabled)
        //        {
        //            logger.Info("Custom index metadata is disabled. Skipping load.");
        //            return;
        //        }

        //        CustomIndexMetadatas = (await CustomIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.Exchange)).ToList();
        //        CustomMetadataColumns = (await CustomMetadataColumnDao.GetAllCustomMetadataColumnsAsync()).ToList();
        //        logger.Info($"Loaded {CustomIndexMetadatas.Count} custom index metadata mappings for Exchange.");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error($"Failed to load custom index metadata. Error: {ex}");
        //    }
        //}

        private Dictionary<string, CustomColumn> GetEXOCustomMetadata(object item, Record record)
        {
            var dic = new Dictionary<string, CustomColumn>();
            if (CustomIndexMetadatas == null || CustomIndexMetadatas.Count == 0 || item == null)
            {
                return dic;
            }

            logger.Debug($"Start extracting {CustomIndexMetadatas.Count} custom columns for item: [{record.ItemId}]");

            foreach (var mapping in CustomIndexMetadatas)
            {
                try
                {
                    var columnInfo = CustomMetadataColumns.FirstOrDefault(c => c.UniqueId == mapping.TargetColumnId);
                    if (columnInfo == null)
                    {
                        logger.Warn($"Target column not found for mapping: {mapping.SourceColumnName}");
                        continue;
                    }

                    object value = item switch
                    {
                        ExchangeItem ewsItem => GetEwsItemPropertyValue(ewsItem, mapping.SourceColumnName),
                        IExchangeItem graphItem => GetGraphItemPropertyValue(graphItem, mapping.SourceColumnName),
                        _ => null
                    };

                    if (value == null)
                    {
                        logger.Warn($"Cannot get value for EXO column [{mapping.SourceColumnName}] on item [{record.ItemId}].");
                        continue;
                    }

                    logger.Debug($"Successfully extracted [{mapping.SourceColumnName}] for item: [{record.ItemId}]");

                    dic[columnInfo.UniqueId.ToString()] = BuildCustomColumn(columnInfo, mapping.SourceColumnName, value);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to get custom column [{mapping.SourceColumnName}]. Error: {ex}");
                }
            }

            logger.Debug($"Finished extracting. Successfully mapped {dic.Count} columns for item: [{record.ItemId}]");

            return dic;
        }

        private object GetGraphItemPropertyValue(IExchangeItem item, string sourceColumnName)
        {
            if (item == null) return null;

            switch (sourceColumnName.ToLowerInvariant().Trim())
            {
                case "attachment" or "has attachment" or "hasattachment" or "hasattach":
                    return item.HasAttach;
                case "size" or "itemsize":
                    return (object)item.ItemSize;
                case "sent time" or "sent" or "senddateutc":
                    return item.SendDateUTC;
                //case "received time" or "received":
                //    return item.Received;
                case "created date" or "created" or "createddate":
                    return item.Created;
                case "from" or "sender":
                    return item.SenderEmailAddress;
                case "cc" or "displaycc":
                    return item.DisplayCc;
                case "importance":
                    return ConvertImportanceToString(item.Importance);
                case "retention label" or "retentionlabel":
                    return item.RetentionLabel;
            }

            var props = item.GetProperties();
            var dictKey = MapSourceColumnToPropertiesKey(sourceColumnName);
            if (dictKey != null && props != null && props.TryGetValue(dictKey, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }

        private object GetEwsItemPropertyValue(ExchangeItem item, string sourceColumnName)
        {
            if (item == null) return null;

            switch (sourceColumnName.ToLowerInvariant().Trim())
            {
                case "attachment" or "has attachment" or "hasattachment" or "hasattach":
                    return item.HasAttach;
                case "size" or "itemsize":
                    return (object)item.ItemSize;
                case "sent time" or "sent" or "senddateutc":
                    return item.SendDateUTC;
                //case "received time" or "received":
                //    return item.Received;
                case "created date" or "created" or "createddate":
                    return item.Created;
                case "from" or "sender":
                    return item.SenderEmailAddress;
                case "cc" or "displaycc":
                    return item.DisplayCc;
                case "importance":
                    return ConvertImportanceToString(item.Importance);
                case "retention label" or "retentionlabel":
                    return item.RetentionLabel;
            }

            var props = item.GetProperties();
            var dictKey = MapSourceColumnToPropertiesKey(sourceColumnName);
            if (dictKey != null && props != null && props.TryGetValue(dictKey, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }

        private static string MapSourceColumnToPropertiesKey(string sourceColumnName) =>
            sourceColumnName.ToLowerInvariant().Trim() switch
            {
                "subject" => "Subject",
                "conversation" or "conversationtopic" => "Conversation",
                "from" or "sender" or "fromemail" => "From",
                "to" or "displayto" => "To",
                "cc" or "displaycc" => "Cc",
                "recipient name" or "recipientname" => "Recipient Name",
                "email account" or "emailaccount" => "Email Account",
                "received representing name" or "receivedrepresentingname" => "Received Representing Name",
                "sensitivity" => "Sensitivity",
                "importance" => "Importance",
                "flag status" or "flagstatus" => "Flag Status",
                "flag start date" or "flagstartdate" or "start date" => "Start Date",
                "flag due date" or "flagduedate" or "due date" => "Due Date",
                "size" or "itemsize" => "Size",
                "sent time" or "sent" or "senddateutc" => "Sent",
                "received time" or "received" => "Received",
                "created date" or "created" => "Created",
                _ => null
            };

        private static string ConvertImportanceToString(int importance) => importance switch
        {
            0 => "Low",
            1 => "Normal",
            2 => "High",
            _ => importance.ToString()
        };

        private CustomColumn BuildCustomColumn(RMCustomMetadataColumn column, string sourceColumnName, object value)
        {
            var customColumn = new CustomColumn();
            switch (column.ColumnType)
            {
                case CustomColumnType.SingleText:
                    customColumn.Value = value?.ToString() ?? string.Empty;
                    customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                    return customColumn;

                case CustomColumnType.Number:
                    if (!double.TryParse(value.ToString(), out var numberValue))
                    {
                        logger.Warn($"Cannot parse Number value for column [{sourceColumnName}].");
                        return customColumn;
                    }
                    customColumn.Value = numberValue.ToString();
                    customColumn.Number = numberValue;
                    customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                    return customColumn;

                case CustomColumnType.YesOrNo:
                    bool boolValue;
                    if (value is bool b)
                    {
                        boolValue = b;
                    }
                    else if (!bool.TryParse(value.ToString(), out boolValue))
                    {
                        logger.Warn($"Cannot parse YesOrNo value for column [{sourceColumnName}].");
                        return customColumn;
                    }
                    customColumn.Value = boolValue.ToString();
                    customColumn.YesOrNo = boolValue ? "Yes" : "No";
                    return customColumn;

                case CustomColumnType.DateTime:
                    DateTime dateTimeValue;
                    if (value is DateTime dt)
                    {
                        dateTimeValue = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                    }
                    else if (value is string dateStr
                        && DateTime.TryParse(dateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
                        && parsed != DateTime.MinValue)
                    {
                        dateTimeValue = parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
                    }
                    else
                    {
                        logger.Warn($"Cannot parse DateTime value for column [{sourceColumnName}].");
                        return customColumn;
                    }
                    var timeColumn = new DateTimeColumnValue() { Date = dateTimeValue, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTimeValue;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;

                default:
                    return customColumn;
            }
        }

        #endregion
    }
}
