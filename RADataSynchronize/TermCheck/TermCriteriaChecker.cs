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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using RADataSynchronize.TermCheck.CriteriaCheckers;
using RADataSynchronize.TermCheck.Model;
using System.Reflection;

namespace RADataSynchronize.TermCheck
{
    public class TermCriteriaChecker
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TermCriteriaChecker));

        private static readonly IGeneralSettingService GeneralSettingService =
            PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly Dictionary<ArchiverFilterRuleType, CriteriaChecker> Checkers =
            new Dictionary<ArchiverFilterRuleType, CriteriaChecker>();

        private static readonly GeneralSettingModel GeneralSetting = GeneralSettingService.GetGeneralSettingAsync().Result;

        static TermCriteriaChecker()
        {
            var checkerType = typeof(CriteriaChecker);
            var assembly = Assembly.GetAssembly(checkerType);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.BaseType?.Name != checkerType.Name && type.BaseType?.BaseType?.Name != checkerType.Name) continue;
                var instance = Activator.CreateInstance(type) as CriteriaChecker;
                Checkers.Add(instance.CriteriaType, instance);
            }
        }

        public static bool TryGetAccordWithTermInfo(List<ClassificationRule> classificationRules, Dictionary<ArchiverFilterRuleType, object> values, out TermInfo termInfo)
        {
            termInfo = null;
            try
            {
                var criterias = classificationRules.Where(item => !item.IsDefaultRule)
                    .OrderBy(item => item.RuleOrder).ToList();
                foreach (var criteria in criterias)
                {
                    if (TryGetAccordWithTermInfo(criteria, values, out termInfo))
                    {
                        return true;
                    }
                }

                Logger.Warn("The data is not match all criterias. Use default critera setting.");
                var defaultCriteria = classificationRules.First(item => item.IsDefaultRule);
                termInfo = new TermInfo
                {
                    IsManually = defaultCriteria.NoDefaultTerm,
                    TermId = defaultCriteria.TermId,
                    TermName = defaultCriteria.TermName,
                    TermIsDeprecated = defaultCriteria.TermIsDeprecated,
                    TermIsRemoved = defaultCriteria.TermIsRemoved
                };

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while try get accord with term info. Error: {e}");
                return false;
            }
        }

        public static bool TryGetAccordWithLabelInfo(List<ClassificationRule> classificationRules, Dictionary<ArchiverFilterRuleType, object> values, out LabelInfo labelInfo)
        {
            labelInfo = null;
            try
            {
                var criterias = classificationRules.Where(item => !item.IsDefaultRule)
                    .OrderBy(item => item.RuleOrder).ToList();
                foreach (var criteria in criterias)
                {
                    if (TryGetAccordWithLabelInfo(criteria, values, out labelInfo))
                    {
                        labelInfo.ApplyLabelType = ApplyLabelType.AutoPopulateApply;
                        return true;
                    }
                }

                Logger.Warn("The data is not match all criterias. Use default critera setting.");
                var defaultCriteria = classificationRules.First(item => item.IsDefaultRule);
                labelInfo = new LabelInfo
                {
                    IsManually = defaultCriteria.NoDefaultTerm,
                    UniqueLabelId = defaultCriteria.TermId,
                    LabelName = defaultCriteria.TermName,
                    ApplyLabelType = ApplyLabelType.ApplyDefaultLabel
                };

                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while try get accord with label info. Error: {e}");
                return false;
            }
        }

        private static bool TryGetAccordWithTermInfo(ClassificationRule classificationRule, Dictionary<ArchiverFilterRuleType, object> values, out TermInfo termInfo)
        {
            var criteriaGroup = classificationRule.FilterGroups.First();
            termInfo = new TermInfo
            {
                IsManually = false,
                TermId = classificationRule.TermId,
                TermName = classificationRule.TermName,
                TermIsDeprecated = classificationRule.TermIsDeprecated,
                TermIsRemoved = classificationRule.TermIsRemoved,
            };
            return CheckFilterGroup(criteriaGroup, values);
        }

        private static bool TryGetAccordWithLabelInfo(ClassificationRule classificationRule, Dictionary<ArchiverFilterRuleType, object> values, out LabelInfo termInfo)
        {
            var criteriaGroup = classificationRule.FilterGroups.First();
            termInfo = new LabelInfo
            {
                IsManually = false,
                UniqueLabelId = classificationRule.TermId,
                LabelName = classificationRule.TermName,
            };
            return CheckFilterGroup(criteriaGroup, values);
        }

        private static bool CheckFilterGroups(List<FilterGroup> filterGorups, Dictionary<ArchiverFilterRuleType, object> values, ArchiverFilterCombineMode combineMode)
        {
            var matchedFilterGroup = 0;
            foreach (var filterGroup in filterGorups)
            {
                if (CheckFilterGroup(filterGroup, values))
                {
                    if (combineMode == ArchiverFilterCombineMode.Or)
                    {
                        return true;
                    }
                    matchedFilterGroup++;
                }
                else
                {
                    if (combineMode == ArchiverFilterCombineMode.And)
                    {
                        return false;
                    }
                }
            }

            return matchedFilterGroup == filterGorups.Count;
        }

        private static bool CheckFilterGroup(FilterGroup filterGroup, Dictionary<ArchiverFilterRuleType, object> values)
        {
            var combineMode = filterGroup.CombineMode;
            Logger.Info($"Current filter group combine mode: [{combineMode}].");

            var filters = filterGroup.Filters;
            var filterGroups = filterGroup.FilterGroups;

            if (filters.Count > 0)
            {
                if (CheckFilters(filters, values, combineMode))
                {
                    if (combineMode == ArchiverFilterCombineMode.Or)
                    {
                        Logger.Info($"The data is match current filter group.");
                        return true;
                    }

                    return CheckFilterGroups(filterGroups, values, combineMode);
                }
                else
                {
                    if (combineMode == ArchiverFilterCombineMode.And)
                    {
                        Logger.Info($"The data is not match current filter group.");
                        return false;
                    }
                }
            }

            return filterGroups.Count > 0 && CheckFilterGroups(filterGroups, values, combineMode);

        }

        private static bool CheckFilters(List<RuleFilter> filters, Dictionary<ArchiverFilterRuleType, object> values, ArchiverFilterCombineMode combineMode)
        {
            var matchedFilterCount = 0;
            foreach (var filter in filters)
            {
                if (!values.TryGetValue(filter.RuleType, out var value))
                {
                    Logger.Warn($"The data values not contains: [{filter.RuleType}] value.");
                    continue;
                }

                if (CheckFilter(filter, value))
                {
                    if (combineMode == ArchiverFilterCombineMode.Or)
                    {
                        Logger.Info($"The data is match filter: [{filter.RuleType}].");
                        return true;
                    }
                    matchedFilterCount++;
                }
                else
                {
                    if (combineMode == ArchiverFilterCombineMode.And)
                    {
                        Logger.Warn($"The data is not match current filter group.");
                        return false;
                    }
                }
            }

            return matchedFilterCount == filters.Count;
        }

        private static bool CheckFilter(RuleFilter filter, object value)
        {
            if (!Checkers.TryGetValue(filter.RuleType, out var checker))
            {
                throw new Exception($"Not found {filter.RuleType} criteria checker.");
            }

            var criteriaInfo = new CriteriaInfo
            {
                Condition = filter.Condition,
                Value1 = filter.Value1,
                Value2 = filter.Value2,
                Value1Unit = filter.Value1Unit,
                Value2Unit = filter.Value2Unit
            };

            PerProcessCriteria(filter.RuleType, criteriaInfo);
            return checker.Check(criteriaInfo, value);
        }

        private static void PerProcessCriteria(ArchiverFilterRuleType ruleType, CriteriaInfo criteriaInfo)
        {
            if (ruleType != ArchiverFilterRuleType.ModifiedTime &&
                ruleType != ArchiverFilterRuleType.CreatedTime &&
                ruleType != ArchiverFilterRuleType.LastAccessedTime &&
                ruleType != ArchiverFilterRuleType.LastActiveTime
                 ||
                criteriaInfo.Condition == ArchiverFilterCondition.OlderThan)
            {
                return;
            }

            if (!string.IsNullOrEmpty(criteriaInfo.Value1?.ToString()))
            {
                criteriaInfo.Value1 = GeneralSettingService.ConvertToUTCDateTime(criteriaInfo.Value1.ToString(), GeneralSetting);
            }

            if (!string.IsNullOrEmpty(criteriaInfo.Value2?.ToString()))
            {
                criteriaInfo.Value2 = GeneralSettingService.ConvertToUTCDateTime(criteriaInfo.Value2.ToString(), GeneralSetting);
            }
        }
    }
}
