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
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Disposal
{
    internal class RMPhysicalDisposalCache
    {
        private RALogger logger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object locker = new object();
        private static RMPhysicalDisposalCache _instance = null;
        [Obsolete("Current no use")]
        public static RMPhysicalDisposalCache Instance
        {
            get
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new RMPhysicalDisposalCache();
                        _instance.Initialize();
                    }
                }
                return _instance;
            }
        }

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

        /// <summary>
        /// key=Term unique ID  value=termObj
        /// </summary>
        public Dictionary<Guid, RMTerm> Terms { get; set; }
        /// <summary>
        /// key=rule id  value=rule
        /// </summary>
        public Dictionary<Guid, Rule> Rules { get; set; }
        /// <summary>
        /// key=termid  value=binded rules
        /// </summary>
        public Dictionary<Guid, List<Rule>> TermRuleMapping { get; set; }
        private void Initialize()
        {           
            AssembleTermRuleMapping();
        }
        private void AssembleTermRuleMapping()
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            TermRuleMapping = new Dictionary<Guid, List<Rule>>();
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<Guid, Rule> ruleIdDic = RuleManagerService.GetRulesFromRecords().Where(r => r.PhysicalRule != null && r.PhysicalRule.SOFilters != null && r.PhysicalRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
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

            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                List<Rule> physicalRules = new List<Rule>();

                Rule rule;
                var ruleIds = termRules[term.Id];
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (ruleIdDic.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            physicalRules.Add(rule);
                        }
                    }
                }
                if (physicalRules.Count == 0)
                {
                    //no physical rule
                    continue;
                }
                if (!TermRuleMapping.ContainsKey(term.UniqueId))
                {
                    TermRuleMapping.Add(term.UniqueId, physicalRules);
                }

                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms);
                foreach (var refTerm in refTerms)
                {
                    if (!TermRuleMapping.TryGetValue(refTerm.UniqueId, out List<Rule> tempIds))
                    {
                        TermRuleMapping.Add(refTerm.UniqueId, physicalRules);
                    }
                }
            }
        }             
    }
}
