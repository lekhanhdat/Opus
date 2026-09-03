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
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.TermManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MyHub.Views.ClassCode
{
    public class RMMyhubReadClassCodeMethod
    {
        private static readonly string[] CountryCodeFields = { "[CountryCode]", "CountryCode" };

        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private ITermDao _TermDao;
        public ITermDao TermDao => PlatformWindsorManager.GetService(ref _TermDao);

        private ITermRuleAssociationDao _TermRuleAssociationDao;
        public ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService(ref _TermRuleAssociationDao);

        private IRuleManagerService _RuleManagerService;
        public IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService(ref _RuleManagerService);

        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();

        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private IFSConnectionDao _FSConnectionDao;
        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);
        public async Task<List<RMMyhubClassCodeItem>> ReadAllClassCodeNameByTerms(ReadAllClassCodeNameReq req)
        {
            try
            {
                List<Guid> gPartitionKeyIds = req.PartitionKeyIds.Select(Guid.Parse).ToList();
                var connectionGroupIds = FSConnectionDao.GetConnectionByIds(gPartitionKeyIds).Select(x => x.GroupId).Distinct().ToList();
                var reqList = gPartitionKeyIds.Concat(connectionGroupIds).ToList();
                var termSetIds = FileSystemSettingDao.LoadAllSettingsByScopeIds(reqList).Select(s => s.TermSetId).ToList();

                var termSets = await TermSetDao.GetTermSetsByTermSetIds(termSetIds);
                var ids = termSets.Select(s => s.Id).ToList();

                var allTerms = TermDao.GetActiveTermByTermSetIds(ids).Where(t => !t.IsDeprecated).ToList();

                var fullPathCache = new Dictionary<Guid, string>();

                var result = new List<RMMyhubClassCodeItem>();
                foreach (var term in allTerms)
                {

                    var termFullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId);

                    var item = new RMMyhubClassCodeItem
                    {
                        TermName = term.Name,
                        TermFullPath = termFullPath,
                        TermUniqueId = term.UniqueId
                    };

                    result.Add(item);
                }

                result = result.OrderBy(x => x.TermName, StringComparer.OrdinalIgnoreCase).ToList();

                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to read all class code by PartitionKeyIds. PartitionKeyIds : {req.PartitionKeyIds}. Error message : {e.Message}");
                return null;
            }

        }

        public List<string> ReadAllClassCodeName()
        {
            var terms = ReadAllClassCode();
            return terms.Select(term => term.Name).Where(name => !string.IsNullOrEmpty(name)).Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(name => name).Distinct().ToList();

        }

        //暂时保持同步版本，后续如果存在性能问题进行异步版本处理
        public List<string> ReadAllCountryCodeNameByTerms()
        {
            var terms = TermDao.GetAllNotRemoveTermsForce();

            var termIds = terms.Select(term => term.Id).ToList();
            var termRuleMappings = TermRuleAssociationDao.GetTermRuleInfoByTermIds(termIds);
            var ruleIds = termRuleMappings.Select(trm => trm.RuleId).Distinct().ToList();
            var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);
            var countryCodes = new List<string>();
            foreach (var mapping in termRuleMappings)
            {
                if (!allRules.TryGetValue(mapping.RuleId.ToString(), out var rule) || rule.FSRule == null)
                {
                    continue;
                }

                var fsRule = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals) &&
                    CountryCodeFields.Contains(f.Rule?.Value1)
                    && f.Rule is ColumnTextRule);

                if (fsRule != null && !string.IsNullOrEmpty(fsRule.Value?.Value1))
                {
                    countryCodes.AddRange(fsRule.Value.Value1.Split(";", StringSplitOptions.RemoveEmptyEntries));
                }
            }

            return countryCodes.Distinct().OrderBy(code => code == "US" ? 0 : 1).ThenBy(code => code).ToList();
        }
        public List<string> ReadCountryCodeNameByClassCode(string classCode)
        {
            if (string.IsNullOrWhiteSpace(classCode))
            {
                return new List<string>();
            }

            var termIds = ReadAllClassCode()
                .Where(term => !string.IsNullOrWhiteSpace(term.Name)
                    && string.Equals(term.Name, classCode, StringComparison.OrdinalIgnoreCase))
                .Select(term => term.Id)
                .Distinct()
                .ToList();

            return ReadCountryCodeNamesByTermIds(termIds);
        }
        public List<string> ReadCountryCodeNameByClassCode(RMMyhubClassCodeItem item)
        {
            var term = TermDao.GetRMTermByUniqueId(item.TermUniqueId);
            return ReadCountryCodeNamesByTermId(term.Id);
        }
        private List<string> ReadCountryCodeNamesByTermId(int termId)
        {
            return ReadCountryCodeNamesByTermIds(new List<int> { termId });
        }
        private List<string> ReadCountryCodeNamesByTermIds(List<int> termIds)
        {
            if (termIds == null || termIds.Count == 0)
            {
                return new List<string>();
            }

            var termRuleMappings = TermRuleAssociationDao.GetTermRuleInfoByTermIds(termIds);
            if (termRuleMappings == null || termRuleMappings.Count == 0)
            {
                return new List<string>();
            }

            var ruleIds = termRuleMappings.Select(trm => trm.RuleId).Distinct().ToList();
            var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);
            var countryCodes = new List<string>();

            foreach (var mapping in termRuleMappings)
            {
                if (!allRules.TryGetValue(mapping.RuleId.ToString(), out var rule) || rule.FSRule == null)
                {
                    continue;
                }

                countryCodes.AddRange(GetCountryCodes(rule));
            }

            var result = countryCodes
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (result.Remove("US"))
            {
                result.Insert(0, "US");
            }
            return result;
        }

        private static IEnumerable<string> GetCountryCodes(Rule rule)
        {
            var fsRule = rule.FSRule.Filters.FirstOrDefault(f => (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals) &&
                CountryCodeFields.Contains(f.Rule?.Value1)
                && f.Rule is ColumnTextRule);

            if (fsRule == null || string.IsNullOrEmpty(fsRule.Value?.Value1))
            {
                return Enumerable.Empty<string>();
            }

            return fsRule.Value.Value1
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrEmpty(code));
        }

        private List<RMTerm> ReadAllClassCode()
        {
            var terms = TermDao.GetAllNotRemoveTermsForce();
            return terms;
        }

        public async Task<List<RMMyhubClassCodeCascadeDataDto>> ReadAllClassifyDataByTerms(ReadAllClassCodeNameReq req)
        {
            try
            {
                List<Guid> gPartitionKeyIds = req.PartitionKeyIds.Select(Guid.Parse).ToList();
                var connectionGroupIds = FSConnectionDao.GetConnectionByIds(gPartitionKeyIds).Select(x => x.GroupId).Distinct().ToList();
                var reqList = gPartitionKeyIds.Concat(connectionGroupIds).ToList();
                var termSetIds = FileSystemSettingDao.LoadAllSettingsByScopeIds(reqList).Select(s => s.TermSetId).Distinct().ToList();

                var tasks = termSetIds.Select(termSetId => GetAllClassifyDataAsync(termSetId.ToString()));
                var results = await Task.WhenAll(tasks);
                var classifyDataList = results.SelectMany(x => x).ToList();

                return classifyDataList;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to read all class code by PartitionKeyIds. PartitionKeyIds : {req.PartitionKeyIds}. Error message : {e.Message}");
                return null;
            }
        }

        public async Task<List<RMMyhubClassCodeCascadeDataDto>> GetAllClassifyDataAsync(string termSetId)
        {
            var result = await TaxonomyService.RMMyhubGetClassCodeCascadeDataAsync(termSetId);
            return result;
        }
    }
}
