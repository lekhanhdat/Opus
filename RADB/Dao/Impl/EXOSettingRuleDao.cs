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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class EXOSettingRuleDao : BaseDao<RMExchangeOnlineSettingRuleMapping>, IEXOSettingRuleDao
    {
        public List<RMSimpleRule> GetMappingRules(Guid scopeId)
        {
            using (var context = GetNewContext())
            {
                return context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId).Select(o => new RMSimpleRule { RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).OrderBy(o => o.RuleOrder).ToList();
            }
        }

        public List<Guid> GetSiteCollectionRuleIds(List<Guid> siteCollectionIds)
        {
            List<Guid> ruleIds = new List<Guid>();
            using (var context = GetNewContext())
            {
                return context.RMExchangeOnlineSettingRuleMappings
                    .Where(m => siteCollectionIds.Contains(m.ScopeId))
                    .Select(r => r.RuleId)
                    .Distinct()
                    .ToList();
            }
        }

        public void SaveMappingRules(RMEXOTreeNode node)
        {
            var needSaveRules = node.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup && node.IsNullClassificationSetting;
            if (needSaveRules)
            {
                using (var context = GetNewContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        var scopeId = new Guid(node.Id);
                        var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId).ToList();
                        context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                        context.SaveChanges();
                        var entityRules = node?.Rules.Select(o => new RMExchangeOnlineSettingRuleMapping { ScopeId = scopeId, RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).ToList();
                        if (entityRules.Count > 0)
                        {
                            context.RMExchangeOnlineSettingRuleMappings.AddRange(entityRules);
                            context.SaveChanges();
                        }
                        tran.Commit();
                    }
                }
            }

        }

        public void SaveOneDriveMappingRules(RMSPTreeNode node)
        {
            var needSaveRules = (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.SiteCollection) && node.IsNullClassificationSetting;
            if (needSaveRules)
            {
                using (var context = GetNewContext())
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        var scopeId = new Guid(node.Id);
                        var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId && o.Type != (int)RuleType.Archiver).ToList();
                        context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                        context.SaveChanges();
                        var entityRules = node?.Rules.Select(o => new RMExchangeOnlineSettingRuleMapping { ScopeId = scopeId, RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = (int)o.RuleOrder }).ToList();
                        if (entityRules.Count > 0)
                        {
                            context.RMExchangeOnlineSettingRuleMappings.AddRange(entityRules);
                            context.SaveChanges();
                        }
                        tran.Commit();
                    }
                }
            }

            if (!node.IsNullClassificationSetting)
            {
                //remove mapping rules
                if (node.Level == (int)NodeLevel.SiteCollection)
                {
                    using (var context = GetNewContext())
                    {
                        var scopeId = new Guid(node.Id);
                        var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId).ToList();
                        context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                        context.SaveChanges();
                    }

                }
                else if (node.Level == (int)NodeLevel.WebApplication)
                {
                    using (var context = GetNewContext())
                    {
                        var groupId = new Guid(node.Id);
                        List<Guid> scopeIds = new List<Guid>() { groupId };
                        var scSettingIds = context.RMOneDriveSettings.Where(s => s.SiteGroupId == groupId && s.SiteId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.ScopeId).ToList();
                        if (scSettingIds != null && scSettingIds.Count > 0)
                        {
                            scopeIds.AddRange(scSettingIds);
                        }
                        var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => scopeIds.Contains(o.ScopeId)).ToList();
                        context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                        context.SaveChanges();
                    }
                }
            }
        }

        public void SaveArchiverMappingRules(RMSPTreeNode node, Guid settingId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == settingId && o.Type == (int)RuleType.Archiver).ToList();
                    var migratedRuleIds = existsRules.Where(r => r.DAOMigrated == true).Select(r => r.RuleId).ToHashSet();
                    context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                    context.SaveChanges();
                    var entityRules = node?.Rules.Select(o => new RMExchangeOnlineSettingRuleMapping { 
                        ScopeId = settingId, RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = (int)o.RuleOrder, Type = (int)RuleType.Archiver,
                        DAOMigrated = migratedRuleIds.Contains(o.RuleId)
                    }).ToList();
                    if (entityRules != null && entityRules.Count > 0)
                    {
                        context.RMExchangeOnlineSettingRuleMappings.AddRange(entityRules);
                        context.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        public List<RMSimpleRule> GetOneDriveMappingRules(Guid groupId, Guid siteId)
        {
            using (var context = GetNewContext())
            {
                List<RMExchangeOnlineSettingRuleMapping> settings = null;
                if (siteId != Guid.Empty)
                {
                    settings = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == siteId && o.Type != (int)RuleType.Archiver).ToList();
                }
                if (settings == null || settings.Count == 0)
                {
                    settings = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == groupId && o.Type != (int)RuleType.Archiver).ToList();
                }
                return settings.Select(o => new RMSimpleRule { RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).OrderBy(o => o.RuleOrder).ToList();
            }
        }

        public int RemoveMappingRules(Guid scopeId)
        {
            using (var context = GetNewContext())
            {
                var existsRules = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId).ToList();
                context.RMExchangeOnlineSettingRuleMappings.RemoveRange(existsRules);
                return context.SaveChanges();
            }
        }

        public List<RMSimpleRule> GetArchiverMappingRules(Guid scopeId, int type)
        {
            using (var context = GetNewContext())
            {
                var tempList = context.RMExchangeOnlineSettingRuleMappings.Where(o => o.ScopeId == scopeId && o.Type == (int)RuleType.Archiver).Select(o => new RMSimpleRule { RuleId = o.RuleId, RuleName = o.RuleName, RuleOrder = o.RuleOrder }).OrderBy(o => o.RuleOrder).ToList();
                List<Guid> levellist = tempList.Select(r => r.RuleId).ToList();
                var rules = context.RMRule.Where(r => levellist.Contains(r.RuleId)).ToDictionary(r => r.RuleId, r => r.RuleLevel);
                foreach (var temp in tempList)
                {
                    temp.IntRuleLevel = rules[temp.RuleId];
                }
                return tempList;
            }
        }

        public List<RMExchangeOnlineSettingRuleMapping> GetAllTeamsNodeRuleMappings(List<Guid> scopeIds)
        {
            using var context = GetNewContext();
            return context.RMExchangeOnlineSettingRuleMappings.Where(s => scopeIds.Contains(s.ScopeId)).ToList();
        }
    }
    public enum RuleType
    {
        None = 0,
        Archiver = 1
    }
}
