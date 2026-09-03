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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using Cloud.Sdk.Telemetry.Data.Alita;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class ScanDataCache : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(ScanDataCache));
        private readonly static object locker = new object();
        private bool HasInit = false;
        static ScanDataCache _instance;
        public RMSPExplorerSiteLevelCache SiteLevelCache { get; private set; }
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }

        public Dictionary<Guid, RMTerm> Terms { get; private set; }

        public Dictionary<Guid, Rule> Rules { get; private set; }

        public Dictionary<Guid, Rule> RulesBindingInTerms { get; private set; }

        public IScanDataReader ScanDataReader { get; private set; }

        public static ScanDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new ScanDataCache();
                        }
                    }
                }
                return _instance;
            }
        }

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
        private ISharePointSettingDao mSpSettingDao;
        protected ISharePointSettingDao spSettingDao
        {
            get
            {
                if (mSpSettingDao == null)
                {
                    mSpSettingDao = new SharePointSettingDao();
                }
                return mSpSettingDao;
            }
        }
        public void SetSiteLevelCache(string bcsColumnInternalName, string bcsColumnDisplayName, Guid bcsColumnID, string scopeId)
        {
            if (!HasInit)
            {
                throw new Exception("You must call Initialize first.");
            }
            if (SiteLevelCache == null)
            {
                SiteLevelCache = new RMSPExplorerSiteLevelCache();
            }
            SiteLevelCache.BCSColumnInternalName = bcsColumnInternalName;
            SiteLevelCache.BCSColumnID = bcsColumnID;
            SiteLevelCache.BCSColumnDisplayName = bcsColumnDisplayName;
            logger.Info($"SetSiteLevelCache info is :BCSColumnInternalName:{SiteLevelCache.BCSColumnInternalName},BCSColumnID:{SiteLevelCache.BCSColumnID},BCSColumnDisplayName:{SiteLevelCache.BCSColumnDisplayName}");
        }

        public void SetScanDataReader(IScanDataReader scanDataReader)
        {
            if (!HasInit)
            {
                throw new Exception("You must call Initialize first.");
            }
            this.ScanDataReader = scanDataReader;
        }

        public List<Guid> GetScanExistingIds(List<Guid> ids)
        {
            if (!HasInit)
            {
                throw new Exception("You must call Initialize first.");
            }
            if (this.ScanDataReader != null)
            {
                return this.ScanDataReader.ExistInScanJob(ids);
            }
            return null;
        }
        public void Initialize(bool IsOneDrive)
        {
            LoadRules(IsOneDrive);
            LoadTerms();
            AssembleTermRuleMapping();
            AssembleRuleCollection();
            HasInit = true;
        }

        public void Initialize(SourceFlag flag)
        {
            LoadRules(flag);
            LoadTerms();
            AssembleTermRuleMapping();
            AssembleRuleCollection();
            HasInit = true;
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

        private void AssembleRuleCollection()
        {
            RulesBindingInTerms = new Dictionary<Guid, Rule>();
            if (TermRuleMapping != null)
            {
                foreach (var item in TermRuleMapping.Values)
                {
                    if (item.CommonRules != null)
                    {
                        foreach (var rule in item.CommonRules.Rules.Values)
                        {
                            var ruleId = new Guid(rule.Id);
                            if (!RulesBindingInTerms.Keys.Contains(ruleId))
                            {
                                RulesBindingInTerms[ruleId] = Rules[ruleId];
                            }
                        }
                    }
                }
            }
        }

        private void LoadTerms()
        {
            logger.Debug("Begin to load terms to cache.");
            ITermDao termDao = new TermDao();
            Terms = termDao.GetAllTermsForce().ToDictionary(t => t.UniqueId);
            logger.Info("Loaded {0} terms to memory cache.", Terms.Count);
        }

        private void LoadRules(bool IsOneDrive)
        {
            logger.Debug("Begin to Load rules to cache.");
            if (!IsOneDrive)
            {
                Rules = RuleManagerService.GetRulesFromRecords().ToDictionary(rule => new Guid(rule.Id));
            }
            else
            {
                Rules = RuleManagerService.GetRulesFromRecords().Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count > 0).ToDictionary(rule => new Guid(rule.Id),v=> RuleManagerService.ConvertToOneDriveRule(v));
            }
            logger.Debug("End to load Rules to cache");
        }

        private void LoadRules(SourceFlag sourceFlag)
        {
            logger.Debug($"Begin to Load rules to cache. SourceFlag {sourceFlag.ToString()}");
            switch (sourceFlag)
            {
                case SourceFlag.OneDrive:
                    Rules = RuleManagerService.GetRulesFromRecords()
                        .Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count > 0)
                        .ToDictionary(rule => new Guid(rule.Id), RuleManagerService.ConvertToOneDriveRule);
                    break;
                default:
                    Rules = RuleManagerService.GetRulesFromRecords()
                        .ToDictionary(rule => new Guid(rule.Id));
                    break;
            }
            logger.Debug("End to load Rules to cache");
        }

        private Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public void Dispose()
        {
            this.TermRuleMapping.Clear();
            this.Terms.Clear();
            this.Rules.Clear();
            _instance = null;
            SiteLevelCache = null;
        }
    }
}
