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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect.Utils;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.Common;
using RAFileSystem.FileSystem.Backup;
using RAFileSystem.FileSystem.DataIngestion;
using AveDateTimeUtility = AvePoint.GCommon.Utility.AveDateTimeUtility;
using SerializerHelper = AvePoint.GCommon.Utility.SerializerHelper;
using ServerProfileObject = AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.RA.FileSystem.Collect
{
    internal class FSJobCache
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Lazy initialization for the singleton instance with initialization logic
        private static readonly Lazy<FSJobCache> _lazyInstance = new Lazy<FSJobCache>(() =>
        {
            var instance = new FSJobCache();
            instance.Initialize(JobContext.Current.JobMessage);
            return instance;
        }, true);

        // Lazy initialization for restore instance without initialization logic
        private static readonly Lazy<FSJobCache> _lazyRestoreInstance = new Lazy<FSJobCache>(() =>
        {
            return new FSJobCache();
        }, true);

        private int mDiscoveryCacheThrottling = 10000000;
        private int mAnalyzerCacheThrottling = 100000;
        private int mPersistCacheThrottling = 100000;
        private int mFailedItemThrottling = 100000;
        public int FailedItemThrottling
        {
            get
            {
                return mFailedItemThrottling;
            }
        }

        /// <summary>
        /// Gets the singleton instance with full initialization.
        /// Thread-safe and lazy-initialized using Lazy&lt;T&gt; pattern.
        /// </summary>
        public static FSJobCache Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Gets the singleton instance for restore scenario without initialization.
        /// Thread-safe and lazy-initialized using Lazy&lt;T&gt; pattern.
        /// </summary>
        public static FSJobCache RestoreInstance
        {
            get
            {
                return _lazyRestoreInstance.Value;
            }
        }
        /// <summary>
        /// for discovery.    put the containers here.
        /// </summary>
        public ICacheService<Stub> DiscoveryCache { get; set; }


        //used to cache data need sync to azure table
        public ICacheService<FSAzureTableEntityDto> DisposalAzureData { get; set; }//this property has changed to DisposalCosmosData

        //used to cache data need to get information from cosmos db
        public ICacheService<FSAzureTableEntityDto> DisposalCosmosDBData { get; set; }
        /// <summary>
        /// for analyzer,   analyzer get the tasks from this cache.
        /// </summary>
        public ICacheService<Stub> AnalyzerCache { get; set; }
        /// <summary>
        /// for persister..  persister get tasks from this cache
        /// </summary>
        public ICacheService<FileSystemRecordDto> RecordCache { get; set; }

        public ICacheService<FSAzureTableEntityDto> DisposalScanCache { get; set; }

        public ICacheService<FSDisposalDiscoverFolder> DisposalFolderCache { get; set; }
        public ICacheService<FSFolderStub> DisposalFSFolderCache { get; set; }
        public List<FileSystemRecordDto> DisposalDifferentFolderCache { get; set; }

        public ConcurrentDictionary<Guid, FSObjectBackup> RuleActionCache { get; set; }


        //used to update deleted file to destoryed status
        public ICacheService<FSAzureTableEntityDto> DisposalArchiveCache { get; set; }

        //used to insert new record to explorer db for move to data
        public ICacheService<FileSystemRecordDto> DisposalMoveToCache { get; set; }


        public Channel<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> DiscoveryToWorker { get; set; }
        public Channel<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> WorkerToUpdater { get; set; }

        public Channel<FSAzureTableEntityDto> DiscoveryToCosmos { get; set; }
        public Channel<FSAzureTableEntityDto> ManualInFolderToCosmos { get; set; }

        public RMDataIngestionExecutionResultCollector DataIngestionResultCollector { get; set; }

        public RMDataIngestionDataCollector DataIngestionDataCollector { get; set; }

        public RAFileSystem.FileSystem.Common.RMDataIngestMessageExtensionManager DataIngestMessageExtensionManager { get; set; }

        /// <summary>
        /// key=Term unique ID  value=termObj
        /// </summary>
        public Dictionary<Guid, FSTermDto> Terms { get; set; }
        /// <summary>
        /// key=rule id  value=rule
        /// </summary>
        public Dictionary<Guid, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> Rules { get; set; }
        /// <summary>
        /// key=termid  value=binded rules
        /// </summary>
        public Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>> TermRuleMapping { get; set; }

        public List<string> BreakNodeUrls { get; set; }

        public List<RMAgentSyncFailureItem> LastJobFailedItems = new List<RMAgentSyncFailureItem>();

        public List<Guid> LastJobFailedItemIds = new List<Guid>();

        public ConcurrentBag<Guid> CurrentJobFailedItemIds = new ConcurrentBag<Guid>();

        public ConcurrentBag<Guid> SuccessItemIdsInLastJobFailedItems = new ConcurrentBag<Guid>();

        public ConcurrentBag<RMAgentSyncFailureItem> FailedItems = new ConcurrentBag<RMAgentSyncFailureItem>();

        public ThreadCounter DiscoverThreadMonitor { get; set; }
        public ThreadCounter AnalyzerThreadMonitor { get; set; }
        public ThreadCounter SerializerThreadMonitor { get; set; }
        public ThreadCounter WaitingApprovalReportThreadMonitor { get; set; }
        public ThreadCounter DisposalDataUpdaterThreadMonitor { get; set; }

        /// <summary>
        /// key=scope id  value=FSSettingDto
        /// </summary>
        public Dictionary<Guid, FSSettingDto> ScopeSettingCache { get; set; }
        public Dictionary<string, FSSettingDto> GroupSettingCache { get; set; }
        public string RootPath { get; internal set; }
        public string ConnectionPath { get; internal set; }
        public string FSRestoreCachePath { get; internal set; }
        public string FSRestoreLocation { get; internal set; }
        public RestoreOption FSRestoreOption { get; internal set; }
        public string RunJobScopePath { get; internal set; }

        public string RunJobParentScopePath { get; internal set; }
        public Guid DispoalSettingScopeId { get; set; }
        //public string UserName { get; internal set; }
        //public string SecPwd { get; internal set; }
        public string RecordOwner { get; internal set; }
        public Guid AveConnectionGroupId { get; set; }
        public Guid AveConnectionId { get; set; }

        public bool CurrentNodeIsEnableRecordManagement { get; set; }
        //Move to job context... todo hyw
        public int FailedCount { get; set; }
        public int SuccessCount { get; set; }
        public FSJobController JobController { get; set; }

        //public IUniqueIdService UniqueIdService { get; set; }

        public FSPropertiesMapping PropertiesMapping { get; set; }

        public Dictionary<string, Guid> ConnectionCache { get; set; }

        public List<string> RunningJobNodeUrls { get; set; }

        public GeneralSettingModel TimeSettingModel { get; set; }

        public string TimeFormat { get; set; }

        public List<Guid> ChangedTermIds { get; set; }
        /// <summary>
        /// key=scope id  value=RuleCollection
        /// </summary>
        public Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>> AutoRuleCollections { get; set; }

        /// <summary>
        /// key= scopeId + "_" + autoRuleId   value=TermId
        /// </summary>   
        public Dictionary<string, Guid> AutoRuleIdTermIdMapping { get; set; }

        public ClassCodeInfoDto classCodeInfoDtoOnNode { get; set; }
        public List<Guid> ClassCodeIds { get; set; } = new List<Guid>();
        public Dictionary<Guid, ClassCodeInfoDto> ClassCodeInfoByTermId { get; set; } = new Dictionary<Guid, ClassCodeInfoDto>();

        public Dictionary<Guid, ClassCodeInfoDto> ContainerLevelClassCodeCache = new Dictionary<Guid, ClassCodeInfoDto>();
        public Dictionary<string, OlderThanTimeDtoForAgent> RuleUnitClassCodeCache = new Dictionary<string, OlderThanTimeDtoForAgent>();
        public object ContainerLevelClassCodeCacheLock { get; } = new object();
        public object RuleUnitClassCodeCacheLock { get; } = new object();
        public bool EnableJPMC { get; set; }
        //public FSJobCommonConfig Config { get; private set; }

        private FSJobCache()
        {
            GetCacheThrottling();
            // Config = ConfigurationManager.GetSection("FSCollectJobConfig") as FSJobCommonConfig;
            DiscoveryCache = new MemoryListCacheService<Stub>();
            DiscoveryCache.SetThrottling(mDiscoveryCacheThrottling);
            logger.Info($"DiscoveryCache Count:{mDiscoveryCacheThrottling}");
            AnalyzerCache = new MemoryListCacheService<Stub>();
            AnalyzerCache.SetThrottling(mAnalyzerCacheThrottling);
            logger.Info($"AnalyzerCache Count:{mAnalyzerCacheThrottling}");
            RecordCache = new MemoryListCacheService<FileSystemRecordDto>();
            RecordCache.SetThrottling(mPersistCacheThrottling);
            logger.Info($"RecordCache Count:{mPersistCacheThrottling}");
            DisposalScanCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            DisposalScanCache.SetThrottling(mAnalyzerCacheThrottling);
            logger.Info($"DisposalScanCache Count:{mAnalyzerCacheThrottling}");
            DisposalArchiveCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            DisposalFolderCache = new MemoryListCacheService<FSDisposalDiscoverFolder>();
            DisposalFolderCache.SetThrottling(mAnalyzerCacheThrottling);
            DisposalFSFolderCache = new MemoryStackCacheService<FSFolderStub>();
            //DisposalFSFolderCache.SetThrottling(mAnalyzerCacheThrottling);
            DisposalDifferentFolderCache = new List<FileSystemRecordDto>();

            DisposalArchiveCache.SetThrottling(mAnalyzerCacheThrottling);
            logger.Info($"DisposalArchiveCache Count:{mAnalyzerCacheThrottling}");
            RuleActionCache = new ConcurrentDictionary<Guid, FSObjectBackup>();
            DisposalMoveToCache = new MemoryListCacheService<FileSystemRecordDto>();

            DisposalAzureData = new MemoryListCacheService<FSAzureTableEntityDto>();
            DisposalAzureData.SetThrottling(1000000);

            DisposalCosmosDBData = new MemoryListCacheService<FSAzureTableEntityDto>();
            DisposalCosmosDBData.SetThrottling(1000000);

            DiscoverThreadMonitor = new ThreadCounter(0);
            SerializerThreadMonitor = new ThreadCounter(0);
            AnalyzerThreadMonitor = new ThreadCounter(0);
            WaitingApprovalReportThreadMonitor = new ThreadCounter(0);
            DisposalDataUpdaterThreadMonitor = new ThreadCounter(0);
            JobController = new FSJobController();

            TermRuleMapping = new Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>>();
            Terms = new Dictionary<Guid, FSTermDto>();
            Rules = new Dictionary<Guid, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
            ScopeSettingCache = new Dictionary<Guid, FSSettingDto>();
            RunningJobNodeUrls = new List<string>();
            AutoRuleCollections = new Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>>();
            AutoRuleIdTermIdMapping = new Dictionary<string, Guid>();

        }

        private void GetCacheThrottling()
        {
            try
            {
                mAnalyzerCacheThrottling = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.AnalyzerCacheThrottling));
                logger.Info("mAnalyzerCacheThrottling is " + mAnalyzerCacheThrottling);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get mAnalyzerCacheThrottling. Error:{0}", e.ToString());
            }
            try
            {
                mDiscoveryCacheThrottling = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.DiscoveryCacheThrottling));
                logger.Info("mDiscoveryCacheThrottling is " + mDiscoveryCacheThrottling);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get mDiscoveryCacheThrottling. Error:{0}", e.ToString());
            }

            try
            {
                mPersistCacheThrottling = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.PersistCacheThrottling));
                logger.Info("mPersistCacheThrottling is " + mPersistCacheThrottling);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get mPersistCacheThrottling. Error:{0}", e.ToString());
            }


            try
            {
                mFailedItemThrottling = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.FailedItemThrottling));
                logger.Info("mFailedItemThrottling is " + mFailedItemThrottling);
            }
            catch (Exception e)
            {
                mFailedItemThrottling = 100000;
                logger.Warn("Failed to get mFailedItemThrottling. Error:{0}", e.ToString());
            }
        }

        private void Initialize(object msg)
        {
            //CodeContract.Requires(msg is RecordsJobMessage, "Type of message is not supported");
            FSJobMessage jobMsg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msg.ToString());
            AssembleRules(jobMsg);
            AssembleTerms(jobMsg);
            AssembleTermRuleMapping(jobMsg);
            AssembleScopeSettingCache(jobMsg);
            //InitUniqueIdService(jobMsg);
            AssembleAutoClassificationRule();
            AssembleBreakNodeUrls(jobMsg);
            AssembleRunningJobNodeUrls(jobMsg);
            AssemblePropertiesMapping();
            AssembleConnectionCache(jobMsg);
            AssembleTimeSettingModel(jobMsg);
            TimeFormat = jobMsg.TimeFormat;
            ChangedTermIds = jobMsg.ChangedTermIds;
            ClassCodeIds = jobMsg.ClassCodeIds ?? new List<Guid>();
            AssembleClassCodeInfoMapping(jobMsg);
        }

        private void AssembleClassCodeInfoMapping(FSJobMessage jobMsg)
        {
            ClassCodeInfoByTermId = new Dictionary<Guid, ClassCodeInfoDto>();

            if (jobMsg.ClassCodeInfoList != null)
            {
                foreach (var info in jobMsg.ClassCodeInfoList)
                {
                    if (info != null && info.TermId != Guid.Empty && !ClassCodeInfoByTermId.ContainsKey(info.TermId))
                    {
                        ClassCodeInfoByTermId.Add(info.TermId, info);
                    }
                }
            }

            if (ClassCodeInfoByTermId.Count > 0) return;

            if (jobMsg.ClassCodeDto != null)
            {
                if (jobMsg.ClassCodeDto.TermId != Guid.Empty)
                {
                    ClassCodeInfoByTermId[jobMsg.ClassCodeDto.TermId] = jobMsg.ClassCodeDto;
                    return;
                }

                if (jobMsg.ClassCodeIds != null && jobMsg.ClassCodeIds.Count > 0)
                {
                    foreach (var termId in jobMsg.ClassCodeIds)
                    {
                        if (termId != Guid.Empty && !ClassCodeInfoByTermId.ContainsKey(termId))
                        {
                            ClassCodeInfoByTermId.Add(termId, new ClassCodeInfoDto
                            {
                                TermId = termId,
                                ClassCode = jobMsg.ClassCodeDto.ClassCode,
                                CountryCode = jobMsg.ClassCodeDto.CountryCode,
                                RetentionType = jobMsg.ClassCodeDto.RetentionType,
                                StartDate = jobMsg.ClassCodeDto.StartDate,
                                EndTime = jobMsg.ClassCodeDto.EndTime,
                                PolicyValueUnit = jobMsg.ClassCodeDto.PolicyValueUnit,
                                PolicyValueNumber = jobMsg.ClassCodeDto.PolicyValueNumber,
                            });
                        }
                    }
                    logger.Info("AssembleClassCodeInfoMapping: mapped {0} class code(s) from ClassCodeDto fallback.", ClassCodeInfoByTermId.Count);
                    return;
                }
            }

            if (jobMsg.ClassCodeIds != null && jobMsg.ClassCodeIds.Count > 0)
            {
                foreach (var termId in jobMsg.ClassCodeIds)
                {
                    if (termId != Guid.Empty && !ClassCodeInfoByTermId.ContainsKey(termId))
                    {
                        ClassCodeInfoByTermId.Add(termId, new ClassCodeInfoDto { TermId = termId });
                    }
                }
                logger.Warn("AssembleClassCodeInfoMapping: ClassCodeDto is null. Mapped {0} TermId(s) without metadata (ClassCode/CountryCode/EndTime will be empty). " + "The server must populate FSJobMessage.ClassCodeDto for FSDisposalByClassCode jobs.", ClassCodeInfoByTermId.Count);
                return;
            }

            logger.Warn("AssembleClassCodeInfoMapping: No class code data found in job message. " + "ClassCodeInfoList, ClassCodeDto, and ClassCodeIds are all null/empty.");
        }

        private void AssembleAutoClassificationRule()
        {
            try
            {
                foreach (KeyValuePair<Guid, FSSettingDto> scopeSetting in ScopeSettingCache)
                {
                    Guid scopeId = scopeSetting.Key;
                    FSSettingDto setting = scopeSetting.Value;
                    if (setting.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification)
                    {
                        continue;
                    }
                    List<Contract.Global.Object.ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<Contract.Global.Object.ClassificationRule>>(setting.AutoClassificationRules);
                    List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = GetRuleCollection(autoRules, scopeId);
                    if (!AutoRuleCollections.ContainsKey(scopeId))
                    {
                        AutoRuleCollections.Add(scopeId, rules);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> GetRuleCollection(List<Contract.Global.Object.ClassificationRule> autoRules, Guid scopeId)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters;
            foreach (Contract.Global.Object.ClassificationRule autoRule in autoRules)
            {
                if (autoRule.IsDefaultRule)
                {
                    if (autoRule.NoDefaultTerm)
                    {
                        string key = scopeId.ToString() + '_' + Guid.Empty.ToString();
                        if (!AutoRuleIdTermIdMapping.ContainsKey(key))
                        {
                            AutoRuleIdTermIdMapping.Add(key, Guid.Empty);
                        }
                    }
                    else
                    {
                        string key = scopeId.ToString() + '_' + Guid.Empty.ToString();
                        if (!AutoRuleIdTermIdMapping.ContainsKey(key))
                        {
                            AutoRuleIdTermIdMapping.Add(key, new Guid(autoRule.TermId));
                        }
                    }
                }
                else
                {
                    soFilters = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy>();
                    int sequenceNo = 0;
                    ConvertToSOFilters(autoRule.FilterGroups, ref sequenceNo, ref soFilters);
                    List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies = ConvertSOFiletrPolicyToFilterPolicy(soFilters);
                    string andOrExpressionStr = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr.LogBase64());
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);

                    string key = scopeId.ToString() + '_' + soRule.FSRule.Id;
                    if (!AutoRuleIdTermIdMapping.ContainsKey(key))
                    {
                        AutoRuleIdTermIdMapping.Add(key, new Guid(autoRule.TermId));
                    }
                }
            }
            return rules;
        }

        private AvePoint.GCommon.Contract.StorageOptimization.Object.Rule ConvertToSORule(Contract.Global.Object.ClassificationRule autoRule, List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters, List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies, string andOrStr)
        {
            PolicyLevel ruleLevel = PolicyLevel.FileSysFile;
            AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = new AvePoint.GCommon.Contract.StorageOptimization.Object.Rule();
            rule.FSRule = new AvePoint.GCommon.Contract.StorageOptimization.Object.Rule()
            {
                Id = Guid.NewGuid().ToString(),
                SOFilters = soFilters,
                Filters = filerPolicies,
                PolicyLevel = ruleLevel,
                AndOrExpression = new Dictionary<PolicyLevel, string>() { { ruleLevel, andOrStr } },
                Order = autoRule.RuleOrder,
                ProfileType = ServerProfileObject.ProfileType.ArchiverRule,
                IncludeNew = "1"
            };
            return rule;
        }

        private string GetFiltersAndOrExpression(List<Contract.Global.Object.RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                Contract.Global.Object.RuleFilter filter = filters[i];
                if (i == filters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filter.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == (int)ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }

        private string GetGroupAndOrExpression(Contract.Global.Object.FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, (ArchiverFilterCombineMode)filterGroup.CombineMode);
                groupAndOrExpression += " " + filterGroup.CombineMode.ToString() + " " + groupsResult;
            }

            if (filterGroup.Filters.Count == 1 && filterGroup.FilterGroups.Count == 0)
            {
                //do nothing
            }
            else
            {
                groupAndOrExpression = "(" + groupAndOrExpression + ")";
            }
            return groupAndOrExpression;
        }
        private string GetGroupsAndOrExpression(List<Contract.Global.Object.FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
        {
            string result = string.Empty;
            for (int i = 0; i < filterGroups.Count; i++)
            {
                string groupResult = GetGroupAndOrExpression(filterGroups[i]);
                if (i == 0)
                {
                    result = groupResult;
                }
                else
                {
                    result += " " + combineMode.ToString() + " " + groupResult;
                }
            }
            return result;
        }

        private AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy BuildSOFilter(Contract.Global.Object.RuleFilter filter, int sequenceNo)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter
            {
                CombineMode = (ArchiverFilterCombineMode)filter.CombineMode,
                //arFilter.SequenceNo = filter.SequenceNo;
                SequenceNo = sequenceNo,
                Level = (PolicyLevel)filter.Level,
                Condition = (ArchiverFilterCondition)filter.Condition,
                RuleType = (ArchiverFilterRuleType)filter.RuleType
            };
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
            {
                string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                {

                    DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                    if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    arFilter.Value1 = startUtcTime.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                    arFilter.Value2 = endUtcTime.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.Before)
                {
                    // ValidateValueCount(value, 3);
                    DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    arFilter.Value1 = utcTime.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    //ValidateValueCount(value, 1);
                    //SetValueForOlderThan(value[0]);
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == (int)ArchiverFilterRuleType.DocumentSize || filter.RuleType == (int)ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == (int)ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value1Unit;
                    arFilter.Value2Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter.Dto;
        }

        private List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> filerPolicies = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            foreach (AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy filter in soFilters)
            {
                AvePoint.GCommon.Contract.CommonFilter.FilterPolicy filterPolicy = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                //filterPolicy.Level = filter.Level;
                filterPolicy.Level = PolicyLevel.FileSysFile;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }

        private void ConvertToSOFilters(List<Contract.Global.Object.FilterGroup> filterGroups, ref int sequenceNo, ref List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            foreach (Contract.Global.Object.FilterGroup filterGroup in filterGroups)
            {
                foreach (Contract.Global.Object.RuleFilter raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }

        //private void InitUniqueIdService(FSJobMessage jobMsg)
        //{
        //    //IUniqueIdSettingDao dao = new UniqueIdSettingDao();
        //    //string setting = apiUtility.LoadUniqueIdSetting();
        //    string prefix = ContractConstants.UniqueId_DefaultPrefix;
        //    if (!string.IsNullOrWhiteSpace(jobMsg.UniqueIdPrefix))
        //    {
        //        prefix = jobMsg.UniqueIdPrefix;
        //    }
        //    UniqueIdService = new UniqueIdService() { UniqueIdPrefix = prefix };
        //}

        private void AssembleScopeSettingCache(FSJobMessage jobMsg)
        {
            try
            {
                //IFileSystemSettingDao settingDao = new FileSystemSettingDao();
                GroupSettingCache = new Dictionary<string, FSSettingDto>();
                ScopeSettingCache = jobMsg.RMScopeSettings.ToDictionary(s => s.ScopeId);
                //apiUtility.GetFileSystemSettings().ToDictionary(t => t.ScopeId);
                logger.Info("Loaded {0} scope-term settings from Records database.", ScopeSettingCache.Count);
                //for the top3 levels, only the id from docave is stored in the db. so we need to get new id for the top3 levels.
                List<FSSettingDto> settings = ScopeSettingCache.Values.ToList();
                foreach (FSSettingDto setting in settings)
                {
                    //full path is encrypt in db setting
                    setting.FullPath = RAEncodeUtil.DecryptByCommunicationKey(setting.FullPath);
                    if (!GroupSettingCache.ContainsKey(setting.FullPath))
                    {
                        GroupSettingCache.Add(setting.FullPath, setting);
                    }
                    Guid id = setting.FullPath.ToLowerInvariant().ToMd5();
                    if (!ScopeSettingCache.ContainsKey(id))
                    {
                        ScopeSettingCache[id] = setting;
                    }
                }
                //GroupSettingCache = GroupSettingCache.OrderByDescending(s => s.Key);
                //NOT SURE
                logger.Info("Totally, there are {0} scope-term settings after the id revised.", ScopeSettingCache.Count);
            }
            catch
            {
                throw;
            }
        }



        private void AssembleTermRuleMapping(FSJobMessage jobMsg)
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>> termRuleMapping = new Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>>();
            foreach (var r in jobMsg.TermRuleMapping)
            {
                List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
                foreach (var i in r.Value)
                {
                    if (Rules.ContainsKey(i))
                    {
                        rules.Add(Rules[i]);
                    }
                }
                termRuleMapping.Add(r.Key, rules);
            }
            TermRuleMapping = termRuleMapping;
            //TermRuleMapping = new Dictionary<Guid, List<Rule>>();
            //Dictionary<int, Guid> termIdUniqueIdMapping = Terms.Values.ToDictionary(r => r.Id, r => r.UniqueId);
            ////ITermRuleAssociationDao termRuleAssociationDao = new TermRuleAssociationDao();
            //Dictionary<int, List<RMTermRuleAssociation>> tempMapping = apiUtility.GetTermRuleAssociations()
            //    .GroupBy(t => t.TermId)
            //    .ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).ToList());
            //Dictionary<int, List<Rule>> termRuleMapping = new Dictionary<int, List<Rule>>();
            //tempMapping.ForEach(t =>
            //{
            //    List<Rule> rules = new List<Rule>();
            //    t.Value.ForEach(association => rules.Add(Rules[association.RuleId]));
            //    termRuleMapping[t.Key] = rules;
            //});

            ////ITermSetMembershipDao membershipDao = new TermSetMembershipDao();
            //Dictionary<int, List<int>> memberships = apiUtility.FindMembership();
            ////membershipDao.FindListWithColumns(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved)
            ////    .GroupBy(t => t.ParentTermId, v => v.TermId)
            ////    .ToDictionary(t => t.Key, v => v.ToList());

            //memberships.Keys.OrderBy(k => k).ForEach(pId =>
            //{
            //    if (termRuleMapping.ContainsKey(pId))
            //    {
            //        memberships[pId].ForEach(cId =>
            //        {
            //            if (!termRuleMapping.ContainsKey(cId))
            //            {
            //                termRuleMapping[cId] = termRuleMapping[pId];
            //            }
            //        });
            //    }
            //});
            //termRuleMapping.Keys.ForEach(termId =>
            //{
            //    if (termIdUniqueIdMapping.ContainsKey(termId))
            //    {
            //        Guid termGuid = termIdUniqueIdMapping[termId];
            //        TermRuleMapping[termGuid] = termRuleMapping[termId];
            //    }
            //});
        }

        private void AssembleTerms(FSJobMessage jobMsg)
        {
            logger.Debug("Begin to load terms to cache.");
            Terms = jobMsg.AllTerms.ToDictionary(t => t.UniqueId);
            logger.Info("Loaded {0} terms to memory cache.", Terms.Count);
        }

        private void AssembleRules(FSJobMessage jobMsg)
        {
            logger.Debug("Begin to assemble rules to cache.");

            //JsonConvert.DeserializeObject<List<Rule>>(jobMsg.AllRecordsRule).ForEach(d =>
            //{
            //    var rule = FSDtoConverter.ConvertRuleDto2Rule(d);
            //    Rules.Add(new Guid(rule.Id), rule);
            //});
            var globalRules = SerializerHelper.DeserializeByDataContractSerializer<List<AvePoint.RA.Contract.Global.Object.Rule>>(jobMsg.AllRecordsRule);
            Rules = globalRules.ConvertAll(r => DtoConverter.ConvertGlobalRule2Rule(r)).ToDictionary(r => new Guid(r.Id));
        }

        private void AssembleBreakNodeUrls(FSJobMessage jobMsg)
        {
            if (jobMsg.BreakTreeNodeUrls != null && jobMsg.BreakTreeNodeUrls.Count > 0)
            {
                logger.Debug("Begin to assemble break node urls to cache.");
                BreakNodeUrls = jobMsg.BreakTreeNodeUrls;
            }
        }

        private void AssembleRunningJobNodeUrls(FSJobMessage jobMsg)
        {
            if (jobMsg.RunningJobNodeUrls != null && jobMsg.RunningJobNodeUrls.Count > 0)
            {
                logger.Debug("Begin to assemble running job node urls to cache.");
                RunningJobNodeUrls = jobMsg.RunningJobNodeUrls;
            }
        }

        private void AssemblePropertiesMapping()
        {
            try
            {
                string globalEnglishMappingPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"Config\FilePropertiesMapping_EN.xml";
                string globalMappingContent = File.ReadAllText(globalEnglishMappingPath);
                if (!string.IsNullOrEmpty(globalMappingContent))
                {
                    FSPropertiesMapping setting = SerializerHelper.DeserializeFromXmlString<FSPropertiesMapping>(globalMappingContent);
                    PropertiesMapping = setting;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Add illegal char to the hash table failed. Detail: {0}", ex.ToString());
            }
        }

        private void AssembleConnectionCache(FSJobMessage jobMsg)
        {
            try
            {
                Dictionary<string, Guid> connectionCache = new Dictionary<string, Guid>();
                if (jobMsg.ConnectionCache != null && jobMsg.ConnectionCache.Count > 0)
                {
                    foreach (var connection in jobMsg.ConnectionCache)
                    {
                        //connectionCache.Add(RAEncodeUtil.DecryptByCommunicationKey(connection.Key).ToLowerInvariant(), connection.Value);
                        connectionCache.Add(connection.Key.ToLowerInvariant(), connection.Value);
                    }
                }
                ConnectionCache = connectionCache;
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while AssembleConnectionCache. Error:{0}", e.ToString());
            }
        }

        private void AssembleTimeSettingModel(FSJobMessage jobMsg)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(jobMsg.GeneralSettingModel))
                {
                    TimeSettingModel = SerializerHelper.DeserializeByDataContractSerializer<GeneralSettingModel>(jobMsg.GeneralSettingModel);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Failed to get TimeSettingModel. Error:{0}", e.ToString());
            }
        }
    }
}
