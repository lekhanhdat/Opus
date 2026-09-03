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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;

namespace RABox
{
    public class RuleManager
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RuleManager));

        private readonly ITermRuleAssociationDao _termRuleAssociationDao;
        private readonly IRuleManagerService _ruleManagerService;
        private readonly Dictionary<string, List<Rule>> _termRulesCache;
        private Dictionary<Guid, RMRuleInfos> _ruleInfoCache;
        private readonly object _locker;

        public RuleManager()
        {
            _termRuleAssociationDao = PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
            _ruleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();
            _termRulesCache = new Dictionary<string, List<Rule>>();
            _ruleInfoCache = new Dictionary<Guid, RMRuleInfos>();
            _locker = new object();
        }

        public bool TryGetTermRelatedRules(string termId, out List<Rule>? rules)
        {
            rules = null;

            if (string.IsNullOrEmpty(termId))
            {
                _logger.Error($"TermId is null or empty.");
                return false;
            }

            using (new PerformanceScope("Box:DataSync:GetTermRelatedRule", "", true))
            {
                if (_termRulesCache.TryGetValue(termId, out rules))
                {
                    return rules?.Count != 0;
                }

                lock (_locker)
                {
                    if (_termRulesCache.TryGetValue(termId, out rules))
                    {
                        return rules?.Count != 0;
                    }

                    _logger.Info($"Can't find term [{termId}] related rule from cache.");
                    var termRelatedRuleInfoes = _termRuleAssociationDao.GetTermRuleInfoByTermUniqueId(new Guid(termId));
                    if (termRelatedRuleInfoes.Count == 0)
                    {
                        _logger.Warn($"Current term [{termId}] not found related rule infoes.");
                        _termRulesCache[termId] = new List<Rule>();
                        return false;
                    }

                    var ruleIds = termRelatedRuleInfoes.Select(item => item.RuleId).ToList();
                    _logger.Info($"Term [{termId}] related rules [{string.Join(", ", ruleIds)}].");
                    rules = _ruleManagerService.GetRulesByIds(ruleIds);
                    rules = rules
                        .Where(item => item.BoxRule != null)
                        .OrderBy(item => termRelatedRuleInfoes.First(i => i.RuleId.ToString() == item.Id).RuleOrder)
                        .ToList();

                    if (rules.Count == 0)
                    {
                        _logger.Warn($"Current term related rules not found in record.");
                        _termRulesCache[termId] = new List<Rule>();
                        return false;
                    }

                    _termRulesCache[termId] = rules;
                }

                return rules?.Count != 0;
            }
        }

        public bool TryGetRule(Guid ruleId, out Rule rule)
        {
            rule = null;

            if (ruleId == Guid.Empty)
            {
                _logger.Error($"RuleId is empty.");
                return false;
            }

            var rules = _ruleManagerService.GetRulesByIds([ruleId]);

            if (rules == null || rules.Count == 0)
            {
                _logger.Warn($"No rules found for the ruleId: {ruleId}");
                return false;
            }

            rule = rules.First();

            return rule != null && rule.BoxRule != null;
        }

        public Dictionary<Guid, Rule> LoadBoxRules()
        {
            try
            {
                _logger.Info("Begin to Load rules.");
                var rulesCache = _ruleManagerService.GetRulesFromRecords().Where(r => r.BoxRule != null && r.BoxRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                _logger.Info("Loaded {0} Rules", rulesCache.Count);

                return rulesCache;
            }
            catch (Exception e)
            {
                _logger.Error($"LoadRules Error: {e}");
                throw new Exception(I18NEntity.GetString("RM_JS_DocAve_CommunicationError"));
            }
        }

        public void AssembleTermRuleMappingAsync(Dictionary<Guid, Rule> ruleCache, Dictionary<Guid, RMTerm> termCache, Dictionary<int, List<int>> memberships)
        {
            _logger.Info("Begin to assemble term rules mappings.");
            Dictionary<int, Guid> termIdUniqueIdMapping = termCache.Values.ToDictionary(r => r.Id, r => r.UniqueId);
            Dictionary<int, List<RMTermRuleAssociation>> termRuleAssociation = _termRuleAssociationDao.GetTermWithRule()
                .GroupBy(t => t.TermId)
                .ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).ToList());
            Dictionary<int, List<Rule>> termRuleMapping = new Dictionary<int, List<Rule>>();

            termRuleAssociation.ForEach(t =>
            {
                List<Rule> rules = new List<Rule>();
                t.Value.ForEach(association =>
                {
                    if (ruleCache.ContainsKey(association.RuleId))
                    {
                        rules.Add(ruleCache[association.RuleId]);
                    }
                });
                termRuleMapping[t.Key] = rules;
            });

            memberships.Keys.OrderBy(k => k).ForEach(pId =>
            {
                if (termRuleMapping.ContainsKey(pId))
                {
                    memberships[pId].ForEach(cId =>
                    {
                        if (!termRuleMapping.ContainsKey(cId))
                        {
                            termRuleMapping[cId] = termRuleMapping[pId];
                        }
                    });
                }
            });

            termRuleMapping.Keys.ForEach(termId =>
            {
                if (termIdUniqueIdMapping.TryGetValue(termId, out Guid termUniqueId))
                {
                    var termGuid = termUniqueId.ToString();
                    _termRulesCache[termGuid] = termRuleMapping[termId];
                }
            });
        }

        public bool TryGetRulesByTermIdFromCache(Guid termId, out List<Rule> rules)
        {
            rules = null;

            if (termId == Guid.Empty)
            {
                _logger.Error($"TermId is empty.");
                return false;
            };

            if (!_termRulesCache.TryGetValue(termId.ToString(), out rules))
            {
                _logger.Warn($"No rules found for the TermId: {termId}");
                return false;
            }

            return rules != null;
        }

        public async Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope("Report.GetRules"))
            {
                var dbRules = await _ruleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    _ruleInfoCache = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        public bool TryGetRuleInfo(Guid id, out RMRuleInfos ruleInfo)
        {
            return _ruleInfoCache.TryGetValue(id, out ruleInfo);
        }
    }
}