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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExplorerSync.Cache
{
    public class RMSPExplorerDataCache : IDisposable
    {
        public static SourceFlag SourceFlag { get; set; } = SourceFlag.SharePoint;
        private RALogger logger = RALogger.GetInstance(typeof(RMSPExplorerDataCache));
        private readonly static object locker = new object();
        static RMSPExplorerDataCache _instance;
        public static RMSPExplorerDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMSPExplorerDataCache();
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

    public class RMSPExplorerSiteLevelCache : IDisposable
    {
        public RMSPExplorerSiteLevelCache()
        {
        }

        public string BCSColumnInternalName { get; set; }
        public string BCSColumnDisplayName { get; set; }
        public Guid BCSColumnID { get; set; }
        public bool HasErrorNode { get; set; } = false;
        public bool HasSkippedLifecycleList { get; set; } = false;
        public string AveSiteId { get; set; }
        public Guid SPSiteId { get; set; }
        public Guid TeamsId { get; set; }

        public Dictionary<string, AveComplianceTagInfo> SiteRetentionLabelCache { get; set; }

        public void Dispose()
        {
            BCSColumnInternalName = null;
            AveSiteId = null;
            HasErrorNode = false;
            HasSkippedLifecycleList = false;
        }
    }
    
    public class RMSPExplorerListLevelCache : IDisposable
    {
        private readonly static object locker = new object();
        public RMSPExplorerListLevelCache()
        {
        }

        static RMSPExplorerListLevelCache _instance;
        public static RMSPExplorerListLevelCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMSPExplorerListLevelCache();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Add(string key, SyncItemRuleInfo rule)
        {
            lock (locker)
            {
                if (FolderRule.ContainsKey(key))
                {
                    FolderRule[key] = rule;
                }
                else
                {
                    FolderRule.Add(key, rule);
                }
            }

        }

        public bool TryGetRuleByPrefix(string key, out SyncItemRuleInfo rule, out string matchedKey)
        {
            lock (locker)
            {
                foreach (var pair in FolderRule)
                {
                    if (key.StartsWith(pair.Key))
                    {
                        rule = pair.Value;
                        matchedKey = pair.Key;
                        return true;
                    }
                }
            }

            rule = null;
            matchedKey = null;
            return false;
        }

        public Dictionary<string, SyncItemRuleInfo> FolderRule { get; private set; } = new Dictionary<string, SyncItemRuleInfo>();

        public void Dispose()
        {
            lock (locker)
            {
                if (FolderRule != null)
                {
                    FolderRule.Clear();
                }
            }

        }
    }

}
