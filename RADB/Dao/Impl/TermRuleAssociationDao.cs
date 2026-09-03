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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermRuleAssociationDao : BaseDao<RMTermRuleAssociation>, ITermRuleAssociationDao
    {
        public IRMEXOLabelDao mEXOLabelDao;
        protected IRMEXOLabelDao EXOLabelDao
        {
            get
            {
                if (mEXOLabelDao == null)
                {
                    mEXOLabelDao = (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao));
                }
                return mEXOLabelDao;
            }
        }

        public IRMRuleDao mRMRuleDao;
        protected IRMRuleDao RMRuleDao
        {
            get
            {
                if (mRMRuleDao == null)
                {
                    mRMRuleDao = (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao));
                }
                return mRMRuleDao;
            }
        }

        private RALogger logger = RALogger.GetInstance(typeof(TermRuleAssociationDao));

        public List<RMTermRuleAssociation> GetTermRuleInfoByTermUniqueId(Guid termUniqueId)
        {
            using(var context = GetNewContext())
            {
                var query = from term in context.Terms
                            join termRuleAssoicateInfo in context.RMTermRuleAssociations
                            on term.Id equals termRuleAssoicateInfo.TermId
                            into termRuleAssoicateInfoes
                            from subTermRuleAssoicateInfo in termRuleAssoicateInfoes.DefaultIfEmpty()
                            where term.UniqueId == termUniqueId
                            select new
                            {
                                TermInfo = term,
                                TermRuleInfo = subTermRuleAssoicateInfo
                            };
                var queryResult = query.GroupBy(item => item.TermInfo.Id).ToDictionary(item => item.Key, item => item.ToList()).FirstOrDefault();
                var termInfo = queryResult.Value?.Select(item => item.TermInfo).FirstOrDefault();
                if(termInfo == null)
                {
                    return new List<RMTermRuleAssociation>();
                }

                var ruleInfoes = queryResult.Value?.Select(item => item.TermRuleInfo).Where(item => item != null).ToList();
                if(ruleInfoes?.Count > 0)
                {
                    ConvertRuleInfoRuleLevel(ruleInfoes);
                    return ruleInfoes;
                }

                if(termInfo.BreakInheritFromParent)
                {
                    return new List<RMTermRuleAssociation>();
                }

                var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termInfo.Id)).FirstOrDefault();
                if(termMembership == null)
                {
                    return new List<RMTermRuleAssociation>();
                }

                var termPath = termMembership.Path;
                var parentTermIds = termPath.Split('/').ToList();
                var ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent))
                {
                    var tInfo = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent).OrderByDescending(t => t.Id).Select(t => new { Id = t.Id, rInfo = t.RuleInfo }).FirstOrDefault();

                    if (tInfo != null && !string.IsNullOrEmpty(tInfo.rInfo))
                    {
                        ruleInfoes = context.RMTermRuleAssociations.AsQueryable().Where(r => tInfo.Id == r.TermId).ToList();
                    }
                }
                ConvertRuleInfoRuleLevel(ruleInfoes);

                return ruleInfoes;
            }
        }

        public List<RMTermRuleAssociation> GetTermRuleInfoByTermid(int termId, SourceFlag sourceFlag = SourceFlag.All)
        {
            List<RMTermRuleAssociation> ruleInfo = null;
            using var context = GetNewContext();
            ruleInfo = context.RMTermRuleAssociations.AsQueryable().Where(r => r.TermId.Equals(termId)).ToList();
            if (ruleInfo != null && ruleInfo.Count != 0)
            {
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
                ConvertRuleInfoRuleLevel(ruleInfo);
                return ruleInfo;
            }
            if (context.Terms.AsQueryable().Any(t => t.Id.Equals(termId) && t.BreakInheritFromParent == true))
            {
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
                ConvertRuleInfoRuleLevel(ruleInfo);
                return ruleInfo;
            }
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            if (termMembership != null)
            {
                var termPath = termMembership.Path;
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
                {
                    var tInfo = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true).OrderByDescending(t => t.Id).Select(t => new { Id = t.Id, rInfo = t.RuleInfo }).FirstOrDefault();

                    if (tInfo != null && !string.IsNullOrEmpty(tInfo.rInfo))
                    {
                        ruleInfo = context.RMTermRuleAssociations.AsQueryable().Where(r => tInfo.Id == r.TermId).ToList();
                    }
                }
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
            }
            ConvertRuleInfoRuleLevel(ruleInfo);
            return ruleInfo;
        }

        public List<RMTermRuleAssociation> GetTermRuleInfoByTermIds(List<int> termIds)
        {
            List<RMTermRuleAssociation> ruleInfo = null;
            using var context = GetNewContext();
            ruleInfo = context.RMTermRuleAssociations.AsNoTracking().Where(r => termIds.Contains(r.TermId)).ToList();
            return ruleInfo;
        }

        public List<RMTermRuleAssociation> GetTermRuleInfoByRuleIds(List<Guid> ruleIds)
        {
            List<RMTermRuleAssociation> ruleInfo = null;
            using var context = GetNewContext();
            ruleInfo = context.RMTermRuleAssociations.AsNoTracking().Where(r => ruleIds.Contains(r.RuleId)).ToList();
            return ruleInfo;
        }

        private void ConvertRuleInfoRuleLevel(List<RMTermRuleAssociation> ruleInfo)
        {
            using var context1 = GetNewContext();
            var idList = ruleInfo.Select(i => i.RuleId);
            var dbRules = context1.RMRule.Where(r => idList.Contains(r.RuleId));
            foreach (var item in ruleInfo)
            {
                var dbRule = dbRules.FirstOrDefault(r => r.RuleId == item.RuleId);
                if (dbRule != null)
                {
                    item.RuleLevel = GetStrByRuleLevel(dbRule.RuleLevel);
                }
            }
        }

        private string GetStrByRuleLevel(int level) {
            switch ((PolicyLevel)level)
            {
                case PolicyLevel.SiteCollection:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_SiteCollection");
                case PolicyLevel.Site:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Site");
                case PolicyLevel.List:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_List");
                case PolicyLevel.Folder:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Folder");
                case PolicyLevel.Item:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Item");
                case PolicyLevel.Document:
                    return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Document");
            }
            return "";
        }

        /// <summary>
        /// add cache for export
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="ruleInfos"></param>
        /// <param name="sourceFlag"></param>
        /// <returns></returns>
        public List<RMTermRuleAssociation> GetTermRuleInfoByTermid(int termId, Dictionary<int, List<RMTermRuleAssociation>> ruleInfos, SourceFlag sourceFlag = SourceFlag.All)
        {

            using var context = GetNewContext();
            var ruleInfo = ruleInfos.ContainsKey(termId) ? ruleInfos[termId] : new List<RMTermRuleAssociation>();//context.RMTermRuleAssociations.AsQueryable().Where(r => r.TermId.Equals(termId)).ToList();
            if (ruleInfo != null && ruleInfo.Count != 0)
            {
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
                return ruleInfo;
            }
            if (context.Terms.AsQueryable().Any(t => t.Id.Equals(termId) && t.BreakInheritFromParent == true))
            {
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
                return ruleInfo;
            }
            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            if (termMembership != null)
            {
                var termPath = termMembership.Path;
                List<string> parentTermIds = termPath.Split('/').ToList();
                List<string> ids = parentTermIds.Skip(1).Take(parentTermIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
                {
                    var tInfo = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true).OrderByDescending(t => t.Id).Select(t => new { Id = t.Id, rInfo = t.RuleInfo }).FirstOrDefault();

                    if (tInfo != null && !string.IsNullOrEmpty(tInfo.rInfo))
                    {
                        ruleInfo = ruleInfos.ContainsKey(tInfo.Id) ? ruleInfos[tInfo.Id] : new List<RMTermRuleAssociation>();//context.RMTermRuleAssociations.AsQueryable().Where(r => tInfo.Id == r.TermId).ToList();
                    }
                }
                ruleInfo = FilterRuleBySourceFlag(sourceFlag, ruleInfo);
            }
            return ruleInfo;
        }

        private List<RMTermRuleAssociation> FilterRuleBySourceFlag(SourceFlag sourceFlag, List<RMTermRuleAssociation> ruleInfo)
        {
            List<RMTermRuleAssociation> tempRuleInfo = new List<RMTermRuleAssociation>();
            if (sourceFlag == SourceFlag.All)
            {
                tempRuleInfo = ruleInfo;
            }
            else if (sourceFlag == SourceFlag.Physical)
            {
                foreach (var rule in ruleInfo)
                {
                    var tempRule = RMRuleDao.GetRuleById(rule.RuleId);
                    if (tempRule.PhysicalDisposalAction != (int)RMContentDisposalAction.None)
                    {
                        tempRuleInfo.Add(rule);
                    }
                }
            }
            return tempRuleInfo;
        }

        public TermSettingsInfo GetParentSettingsByTermId(int termId)
        {
            TermSettingsInfo settingInfos = new TermSettingsInfo();
            List<RMTermRuleAssociation> ruleInfos = new List<RMTermRuleAssociation>();
            using var context = GetNewContext();

            var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(termId)).FirstOrDefault();
            if (termMembership != null)
            {
                var parentIds = termMembership.Path.Split('/').ToList();
                var ids = parentIds.Skip(1).Take(parentIds.Count - 2).ToList();
                var tIds = ids.ConvertAll(i => { return int.Parse(i); });
                if (context.Terms.AsQueryable().Any(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true))
                {
                    var tInfo = context.Terms.AsQueryable().Where(t => tIds.Contains(t.Id) && t.BreakInheritFromParent == true).OrderByDescending(t => t.Id).Select(t => new { Id = t.Id, rInfo = t.RuleInfo, ef = t.EnforceRetention }).FirstOrDefault();

                    if (tInfo != null)
                    {
                        ruleInfos = context.RMTermRuleAssociations.AsQueryable().Where(r => tInfo.Id == r.TermId).ToList();
                        settingInfos.EnforceRetention = tInfo.ef;
                        //settingInfos.label = tInfo.label;
                        DealWithRetentionLabel(settingInfos);
                    }
                }
            }
            settingInfos.infos = ruleInfos.ConvertAll(r =>
            {
                return new RuleDisplayInfo()
                {
                    Id = r.Id,
                    RuleId = r.RuleId.ToString(),
                    RuleLevel = r.RuleLevel,
                    RuleName = r.RuleName,
                    RuleOrder = r.RuleOrder,
                };
            });
            return settingInfos;
        }

        public List<string> GetRelatedTermsByRuleId(Guid ruleId)
        {
            List<string> termNames = new List<string>();
            using var context = GetNewContext();
            List<int> relatedTermIds = context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleId.Equals(ruleId)).Select(r => r.TermId).ToList();
            foreach (var termId in relatedTermIds)
            {
                var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).First();
                #region check term status
                if (term.IsDeprecated)
                {
                    continue;
                }
                if (term.IsRemoved)
                {
                    continue;
                }
                if (term.TermExpirationFrom > 0 && term.TermExpirationTo > 0)
                {
                    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom || DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    {
                        continue;
                    }
                }
                else if (term.TermExpirationFrom > 0)
                {
                    if (DateTime.UtcNow.Ticks < term.TermExpirationFrom)
                    {
                        continue;
                    }
                }
                else if (term.TermExpirationTo > 0)
                {
                    if (DateTime.UtcNow.Ticks > term.TermExpirationTo)
                    {
                        continue;
                    }
                }
                #endregion
                if (!termNames.Contains(term.Name))
                {
                    termNames.Add(term.Name);
                }
                var termMembership = context.TermSetMemberships.AsQueryable().Where(t => t.TermId.Equals(term.Id)).FirstOrDefault();
                var subTermMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId == termMembership.TermId).ToList();
                AddSubTermRule(subTermMemberships, ruleId, ref termNames);
            }
            return termNames;
        }

        public List<string> GetTermNamesByRuleId(Guid ruleId)
        {
            List<string> termNames = new List<string>();
            using var context = GetNewContext();
            List<int> relatedTermIds = context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleId.Equals(ruleId)).Select(r => r.TermId).ToList();
            foreach (var termId in relatedTermIds)
            {
                var term = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).FirstOrDefault();
                if (term != null && !termNames.Contains(term.Name) && !term.IsPermanent)
                {
                    termNames.Add(term.Name);
                }
            }
            return termNames;
        }

        private void AddSubTermRule(List<RMTermSetMembership> subTermMemberships, Guid ruleId, ref List<string> termNames)
        {
            foreach (var subTermMember in subTermMemberships)
            {
                using var context = GetNewContext();
                var subTermRules = context.RMTermRuleAssociations.AsQueryable().Where(r => r.TermId.Equals(subTermMember.TermId));
                bool needAddSubTerm = true;
                foreach (var subTermRule in subTermRules)
                {
                    if (!subTermRule.RuleId.Equals(ruleId))
                    {
                        needAddSubTerm = false;
                        break;
                    }
                }
                if (needAddSubTerm)
                {
                    var subTerm = context.Terms.AsQueryable().Where(t => t.Id.Equals(subTermMember.TermId)).FirstOrDefault();
                    var subTermName = subTerm?.Name;
                    #region check term status
                    if (subTerm.IsDeprecated)
                    {
                        continue;
                    }
                    if (subTerm.IsRemoved)
                    {
                        continue;
                    }
                    if (subTerm.TermExpirationFrom > 0 && subTerm.TermExpirationTo > 0)
                    {
                        if (DateTime.UtcNow.Ticks < subTerm.TermExpirationFrom || DateTime.UtcNow.Ticks > subTerm.TermExpirationTo)
                        {
                            continue;
                        }
                    }
                    else if (subTerm.TermExpirationFrom > 0)
                    {
                        if (DateTime.UtcNow.Ticks < subTerm.TermExpirationFrom)
                        {
                            continue;
                        }
                    }
                    else if (subTerm.TermExpirationTo > 0)
                    {
                        if (DateTime.UtcNow.Ticks > subTerm.TermExpirationTo)
                        {
                            continue;
                        }
                    }
                    #endregion
                    if (!termNames.Contains(subTermName))
                    {
                        termNames.Add(subTermName);
                    }
                    var termMemberships = context.TermSetMemberships.AsQueryable().Where(t => t.ParentTermId.Equals(subTerm.Id)).ToList();
                    AddSubTermRule(termMemberships, ruleId, ref termNames);
                }
            }
        }
        public List<Guid> GetAllRules()
        {
            using var context = GetNewContext();
            var ruleIds = context.RMTermRuleAssociations.AsQueryable().Distinct().Select(r => r.RuleId).ToList();
            return ruleIds;
        }

        public void DeleteTermRuleInfos(int termId)
        {
            // var context = SharedDbContext;
            using (var context = GetNewContext())
            {
                var rules = context.RMTermRuleAssociations.Where(t => t.TermId.Equals(termId));

                if (rules != null && rules.Count() > 0)
                {
                    int count = rules.Count();
                    context.RMTermRuleAssociations.RemoveRange(rules);
                    context.SaveChanges();
                    logger.Info("remove older rule info success {0},count {1}", termId, count);
                }
            }
        }

        public void DeleteTermRuleInfos(Guid ruleId)
        {
            using var context = GetNewContext();
            var rules = context.RMTermRuleAssociations.AsQueryable().Where(t => t.RuleId.Equals(ruleId));
            context.RMTermRuleAssociations.RemoveRange(rules);
            context.SaveChanges();
        }

        public List<RMTermRuleAssociation> GetTermWithRule()
        {
            using var context = GetNewContext();
            List<RMTermRuleAssociation> termIds = context.RMTermRuleAssociations.AsQueryable().ToList();
            return termIds;
        }
        public List<RMTermRuleAssociation> GetTermWithRule(int level)
        {
            using var context = GetNewContext();
            NodeLevel nodeLevel = (NodeLevel)level;
            List<RMTermRuleAssociation> termIds = new List<RMTermRuleAssociation>();
            switch (nodeLevel)
            {
                case NodeLevel.SiteCollection:
                    termIds = context.RMTermRuleAssociations.AsQueryable().Where(t => t.RuleLevel.Equals(PolicyLevel.SiteCollection.ToString())).ToList();
                    break;
                case NodeLevel.Site:
                    termIds = context.RMTermRuleAssociations.AsQueryable().Where(t => t.RuleLevel.Equals(PolicyLevel.Site.ToString())).ToList();
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    termIds = context.RMTermRuleAssociations.AsQueryable().Where(t => t.RuleLevel.Equals(PolicyLevel.List.ToString())
                    || t.RuleLevel.Equals(PolicyLevel.Library.ToString())).ToList();
                    break;
                default:
                    termIds = context.RMTermRuleAssociations.AsQueryable().Where(t => t.RuleLevel.Equals(PolicyLevel.Item.ToString())
                    || t.RuleLevel.Equals(PolicyLevel.Document.ToString())).ToList();
                    break;
            }
            return termIds;
        }


        public List<RMTermRuleAssociation> GetTermWithRuleLevel(int level, List<Rule> daRules)
        {
            using var context = GetNewContext();
            NodeLevel nodeLevel = (NodeLevel)level;
            List<RMTermRuleAssociation> termRules = new List<RMTermRuleAssociation>();
            List<string> ruleIds = new List<string>();
            switch (nodeLevel)
            {
                case NodeLevel.WebApplication:
                case NodeLevel.SiteCollection:
                    termRules = context.RMTermRuleAssociations.AsQueryable().ToList();
                    break;
                case NodeLevel.Site:
                    ruleIds = daRules.Where(r => r.PolicyLevel.Equals(PolicyLevel.Site)
                    || r.PolicyLevel.Equals(PolicyLevel.List)
                    || r.PolicyLevel.Equals(PolicyLevel.Library)
                    || r.PolicyLevel.Equals(PolicyLevel.Folder)
                    || r.PolicyLevel.Equals(PolicyLevel.Document)
                    || r.PolicyLevel.Equals(PolicyLevel.Item)
                    ).Select(t => t.Id).ToList();
                    termRules = context.RMTermRuleAssociations.AsQueryable().Where(r => ruleIds.Contains(r.RuleId.ToString())).ToList();
                    break;
                case NodeLevel.List:
                case NodeLevel.Library:
                    ruleIds = daRules.Where(r => r.PolicyLevel.Equals(PolicyLevel.List)
                    || r.PolicyLevel.Equals(PolicyLevel.Library)
                    || r.PolicyLevel.Equals(PolicyLevel.Folder)
                    || r.PolicyLevel.Equals(PolicyLevel.Document)
                    || r.PolicyLevel.Equals(PolicyLevel.Item)
                    ).Select(t => t.Id).ToList();
                    termRules = context.RMTermRuleAssociations.AsQueryable().Where(r => ruleIds.Contains(r.RuleId.ToString())).ToList();
                    break;
                case NodeLevel.Folder:
                    ruleIds = daRules.Where(r => r.PolicyLevel.Equals(PolicyLevel.Folder)
                    || r.PolicyLevel.Equals(PolicyLevel.Folder)
                    || r.PolicyLevel.Equals(PolicyLevel.Document)
                    || r.PolicyLevel.Equals(PolicyLevel.Item)
                    ).Select(t => t.Id).ToList();
                    termRules = context.RMTermRuleAssociations.AsQueryable().Where(r => ruleIds.Contains(r.RuleId.ToString())).ToList();
                    break;
                case NodeLevel.GoogleSharedDriveContainer:
                case NodeLevel.GoogleMyDriveContainer:
                case NodeLevel.GoogleMyDrive:
                case NodeLevel.GoogleSharedDrive:
                    ruleIds = daRules.Where(r => r.GoogleDriveRule.PolicyLevel.Equals(PolicyLevel.GoogleDriveDocument)).Select(t => t.Id).ToList();
                    termRules = context.RMTermRuleAssociations.AsQueryable().Where(r => ruleIds.Contains(r.RuleId.ToString())).ToList();
                    break;
                default:
                    termRules = context.RMTermRuleAssociations.AsQueryable().ToList();
                    break;
            }
            return termRules;
        }

        public async Task<IEnumerable<RMTermRuleAssociation>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.RMTermRuleAssociations.AsNoTracking().OrderBy(t => t.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertTermRuleAssociationTableAsync(IEnumerable<RMTermRuleAssociation> termRuleAssociations)
        {
            using var context = GetNewContext();
            string tableName = "RMTermRuleAssociations";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var dataList = termRuleAssociations.ToList();

                var sqlBuilder = new StringBuilder();
                var parameters = new List<SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, TermId, TermName, RuleId, RuleName, RuleLevel, RuleOrder) VALUES ");
                int i = 0;
                foreach (var item in termRuleAssociations)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6})");

                    parameters.Add(new SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 1}", item.TermId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 2}", item.TermName));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 3}", item.RuleId));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 4}", item.RuleName));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 5}", item.RuleLevel));
                    parameters.Add(new SqlParameter($"@p{paramIndex + 6}", item.RuleOrder));
                    paramIndex += 7;
                    i++;
                }

                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray()); ;
            }
            catch (Exception ex)
            {
                logger.Error($"Insert RMTermRuleAssociations data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }

        public async Task<long> MultiGeoDeleteAllTermRuleAssociationAsync()
        {
            return await TruncateAllDataInTableAsync("RMTermRuleAssociations");
        }

        [RACodeReview("Allen yin")]
        public List<int> GetTermIdWithRule()
        {
            using var context = GetNewContext();
            List<int> termIds = context.RMTermRuleAssociations.AsQueryable().Select(r => r.TermId).Distinct().ToList();
            return termIds;
        }

        [RACodeReview("Allen yin")]
        public List<int> GetTermIdsByRuleId(string ruleId)
        {
            using var context = GetNewContext();
            return context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleId.ToString().Equals(ruleId)).Select(r => r.TermId).ToList();
        }

        public List<Guid> GetTermUniqueIdsByRuleId(string ruleId)
        {
            List<Guid> termUniqueIds = new List<Guid>();
            using (var context = GetNewContext())
            {
                List<int> relatedTermIds = context.RMTermRuleAssociations.AsQueryable().Where(r => r.RuleId.Equals(new Guid(ruleId))).Select(r => r.TermId).Distinct().ToList();
                foreach (var termId in relatedTermIds)
                {
                    var uniqueId = context.Terms.AsQueryable().Where(t => t.Id.Equals(termId)).Select(t => t.UniqueId).FirstOrDefault();
                    if (!uniqueId.Equals(Guid.Empty) && !termUniqueIds.Contains(uniqueId))
                    {
                        termUniqueIds.Add(uniqueId);
                    }
                }
                return termUniqueIds;
            }
        }

        private void DealWithRetentionLabel(TermSettingsInfo result)
        {
            var tempLabels = EXOLabelDao.GetLabelByStatus((int)RMRetentionLabelStatus.FromGUI).ToList();
            foreach (var item in tempLabels)
            {
                if (item.Type == (int)RMRetentionSourceType.Exchange)
                {
                    result.EXORetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else if (item.Type == (int)RMRetentionSourceType.SharePoint)
                {
                    result.SPRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else if (item.Type == (int)RMRetentionSourceType.OneDrive)
                {
                    result.OneDriveRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                }
                else
                {
                    switch (item.Type)
                    {
                        case (int)RMRetentionSourceType.Teams:
                            result.TeamsRetentionLabel = string.IsNullOrEmpty(item.LabelName) ? string.Empty : item.LabelName;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
