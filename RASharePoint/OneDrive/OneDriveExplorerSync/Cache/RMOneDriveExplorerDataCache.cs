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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerFilterPolicy = AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache
{
    public class RMOneDriveExplorerDataCache : IDisposable
    {

        private RALogger logger = RALogger.GetInstance(typeof(RMOneDriveExplorerDataCache));
        private readonly static object locker = new object();
        static RMOneDriveExplorerDataCache _instance;
        public static RMOneDriveExplorerDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMOneDriveExplorerDataCache();
                            _instance.Initialize();
                        }
                    }
                }
                return _instance;
            }
        }
        #region init service castle
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
        private IRMReportService mReportService;
        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        private IUniqueIdSettingDao mUniqueIdSettingDao;
        protected IUniqueIdSettingDao UniqueIdSettingDao
        {
            get
            {
                if (mUniqueIdSettingDao == null)
                {
                    mUniqueIdSettingDao = (IUniqueIdSettingDao)PlatformWindsorManager.GetService(typeof(IUniqueIdSettingDao));
                }
                return mUniqueIdSettingDao;
            }
        }
        private ITermRuleAssociationDao termRuleAssociationDao;
        protected ITermRuleAssociationDao TermRuleInfos
        {
            get
            {
                if (termRuleAssociationDao == null)
                {
                    termRuleAssociationDao = new TermRuleAssociationDao();
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
                    mTermDao = new TermDao();
                }
                return mTermDao;
            }
        }


        private IOneDriveSettingDao mOneDriveSettingDao;
        protected IOneDriveSettingDao OneDriveSettingDao
        {
            get
            {
                if (mOneDriveSettingDao == null)
                {
                    mOneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
                }
                return mOneDriveSettingDao;
            }
        }
        #endregion

        private RMUniqueIdSetting _uniqueIdSetting;
        public RMUniqueIdSetting UniqueIdSetting
        {
            get
            {
                if (_uniqueIdSetting == null)
                {
                    _uniqueIdSetting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                }
                return _uniqueIdSetting;
            }
        }
        public Dictionary<string, RMSPExplorerSiteLevelCache> SiteLevelCache = null;
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }
        public Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>> AutoRuleCollections { get; set; }
        public Dictionary<string, Guid> AutoRuleIdTermIdMapping { get; set; }
        public Dictionary<Guid, RMTerm> Terms { get; private set; }

        public Dictionary<Guid, Rule> Rules { get; private set; }

        public SOArchiverSettings ArchiverSettings { get; private set; }

        private void Initialize()
        {
            SiteLevelCache = new Dictionary<string, RMSPExplorerSiteLevelCache>();
            LoadRules();
            LoadTerms();
            AssembleTermRuleMapping();
            AssembleArchiverSettings();
            LoadAutoRuleCollections();
        }

        public void InitSiteLevelCache(string key, RMSPExplorerSiteLevelCache value)
        {
            if (!SiteLevelCache.ContainsKey(key))
            {
                SiteLevelCache.Add(key, value);
            }
        }

        public void AssembleArchiverSettings()
        {
            logger.Debug("Begin to assemble archiver setting to cache.");
            ArchiverSettings = ReportService.GetSOArchiverSettings();
        }

        private void AssembleTermRuleMapping()
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            TermRuleMapping = new Dictionary<Guid, RMRuleItemCollection>();
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
            
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };

                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (Rules.TryGetValue(ruleIds[idx], out rule))
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

                }
            }

            TermRuleMapping = termRuleMappings;
        }

        private void LoadTerms()
        {
            logger.Debug("Begin to load terms to cache.");
            ITermDao termDao = new TermDao();
            Terms = termDao.GetAllTermsForce().ToDictionary(t => t.UniqueId);
            logger.Info("Loaded {0} terms to memory cache.", Terms.Count);
        }

        private void LoadRules()
        {
            logger.Debug("Begin to Load rules to cache.");
            Rules = RuleManagerService.GetRulesFromRecords().ToDictionary(rule => new Guid(rule.Id));
            logger.Debug("End to load Rules to cache");
        }

        private void LoadAutoRuleCollections()
        {
            logger.Debug("Begin to Load AutoRuleCollections to cache.");
            AutoRuleCollections = new Dictionary<Guid, List<Rule>>();
            AutoRuleIdTermIdMapping = new Dictionary<string, Guid>();
            var allSettings = OneDriveSettingDao.LoadAllSetting();
            foreach (var setting in allSettings)
            {
                if (setting.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification)
                {
                    continue;
                }
                List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = GetRuleCollection(autoRules, setting.ScopeId);
                if (!AutoRuleCollections.ContainsKey(setting.ScopeId))
                {
                    AutoRuleCollections.Add(setting.ScopeId, rules);
                }
            }
            logger.Debug("End to load AutoRuleCollections to cache");
        }

        private List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> GetRuleCollection(List<ClassificationRule> autoRules, Guid scopeId)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters;
            foreach (ClassificationRule autoRule in autoRules)
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
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr);
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);
                    string key = scopeId.ToString() + '_' + soRule.OneDriveRule.Id;
                    if (!AutoRuleIdTermIdMapping.ContainsKey(key))
                    {
                        AutoRuleIdTermIdMapping.Add(key, new Guid(autoRule.TermId));
                    }
                }
            }
            return rules;
        }

        public static List<FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<SOFilterPolicy> soFilters)
        {
            List<FilterPolicy> filerPolicies = new List<FilterPolicy>();
            foreach (var filter in soFilters)
            {
                FilterPolicy filterPolicy = new FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }
        private string GetGroupsAndOrExpression(List<FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
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

        private string GetGroupAndOrExpression(FilterGroup filterGroup)
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

        private string GetFiltersAndOrExpression(List<RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                RuleFilter filter = filters[i];
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

        public static Rule ConvertToSORule(ClassificationRule autoRule, List<SOFilterPolicy> soFilters, List<FilterPolicy> filerPolicies, string andOrStr)
        {
            Rule rule = new Rule();
            rule.OneDriveRule = new AvePoint.GCommon.Contract.StorageOptimization.Object.Rule()
            {
                Id = Guid.NewGuid().ToString(),
                SOFilters = soFilters,
                Filters = filerPolicies,
                PolicyLevel = (PolicyLevel)autoRule.RuleLevel,
                AndOrExpression = new Dictionary<PolicyLevel, string>() { { autoRule.RuleLevel, andOrStr } },
                Order = autoRule.RuleOrder,
                ProfileType = AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule,
                IncludeNew = "1"
            };
            return rule;
        }

        private void ConvertToSOFilters(List<FilterGroup> filterGroups, ref int sequenceNo, ref List<AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy> soFilters)
        {
            foreach (FilterGroup filterGroup in filterGroups)
            {
                foreach (RuleFilter raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }
       

        private AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy BuildSOFilter(RuleFilter filter, int sequenceNo)
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
                if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value1Unit;
                    arFilter.Value2Unit = (AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit)filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter.Dto;
        }
       

        private Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }   
}
