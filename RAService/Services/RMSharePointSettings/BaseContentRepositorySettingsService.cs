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
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMSharePointSettings
{
    public class BaseContentRepositorySettingsService: RMServiceBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(BaseContentRepositorySettingsService));
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        public IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public IJobMonitorService JobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private SourceFlag SourceType = SourceFlag.None;

        private bool IsEnableInsightsDataCollection = false;

        #region Build Auto
        protected void AddFilterCretiaProperty(List<ClassificationRule> autoRules, SourceFlag sourceType = SourceFlag.None)
        {
            SourceType = sourceType;
            if (autoRules != null)
            {
                foreach (var autoRule in autoRules)
                {
                    if (!CheckBackgroundValue(autoRule))
                    {
                        throw new Exception("Background value exception.");
                    }
                    InnerAddFilterCretia(autoRule.FilterGroups);
                    string result = string.Empty;
                    autoRule.AndOrExpression = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                }
            }
        }
        private bool CheckBackgroundValue(ClassificationRule autoRule)
        {
            if (autoRule.FilterGroups.IsNotNullOrEmpty())
            {
                if (string.IsNullOrEmpty(autoRule.TermName) || string.IsNullOrEmpty(autoRule.TermId) || autoRule.TermId.Equals(Guid.Empty.ToString()))
                {
                    return false;
                }
            }
            return true;
        }
        private string GetGroupAndOrExpression(FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, filterGroup.CombineMode);
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
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }
        private void InnerAddFilterCretia(List<FilterGroup> filterGroups)
        {
            if (filterGroups != null)
            {
                foreach (var filterGroup in filterGroups)
                {
                    if (filterGroup.Filters != null)
                    {
                        foreach (var filter in filterGroup.Filters)
                        {
                            if (!IsEnableInsightsDataCollection && (SourceType == SourceFlag.SharePoint || SourceType == SourceFlag.OneDrive || SourceType == SourceFlag.Teams))
                            {
                                RuleManagerService.EnableInsightsDataCollection(filterGroup.Filters);
                                IsEnableInsightsDataCollection = true;
                            }
                            filter.FilterCretia = GetFilterCretia(filter);
                        }
                    }
                    InnerAddFilterCretia(filterGroup.FilterGroups);
                }
            }
        }
        private string GetFilterCretia(RuleFilter filter)
        {
            ArchiverRuleFilter archiverRuleFilter = BuildArchiverRuleFilter(filter);
            return archiverRuleFilter.FilterCretia();
        }
        private ArchiverRuleFilter BuildArchiverRuleFilter(RuleFilter filter)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
            arFilter.CombineMode = filter.CombineMode;
            arFilter.SequenceNo = filter.SequenceNo;
            arFilter.Level = filter.Level;
            if ((int)filter.Condition == (int)PolicyCondition.Exactly)
            {
                arFilter.Condition = (ArchiverFilterCondition)PolicyCondition.Equals;
            }
            else
            {
                arFilter.Condition = filter.Condition;
            }
            arFilter.RuleType = filter.RuleType;
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            if (string.IsNullOrEmpty(filter.Value1) && filter.Condition != ArchiverFilterCondition.IsEmpty)
            {
                throw new Exception("Rule value can not be null or empty.");
            }
            if (!TenantService.IsNewOpusTenant() && (filter.RuleType == ArchiverFilterRuleType.RetentionLabel || filter.RuleType == ArchiverFilterRuleType.SensitivityLabel))
            {
                throw new Exception("Doesn't support Retention/Sensitivity Label for old logic account.");
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn
         || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty || arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC
         || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime)
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
                    arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.Before)
                {
                    // ValidateValueCount(value, 3);
                    DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    //ValidateValueCount(value, 1);
                    //SetValueForOlderThan(value[0]);
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = filter.Value1Unit;
                    arFilter.Value2Unit = filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter;
        }

        protected void SetAutoTermStatus(List<ClassificationRule> autoRules)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                foreach (var autoRule in autoRules)
                {
                    if (string.IsNullOrEmpty(autoRule.TermId) || autoRule.TermId == Guid.Empty.ToString())
                    {
                        continue;
                    }
                    var term = TermDao.GetRMTermByGuId(new Guid(autoRule.TermId));
                    autoRule.TermName = term.Name;
                    autoRule.TermIsDeprecated = term.IsDeprecated || TermDao.IsExpiredTerm(term.Id);
                    autoRule.TermIsRemoved = term.IsRemoved;
                }
            }
            catch (Exception e)
            {
                logger.Error("Set auto term status error:{0}", e.ToString());
            }
        }

        protected void SetAzureAutoTermStatus(List<ClassificationRule> autoRules)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                foreach (var autoRule in autoRules)
                {
                    if (autoRule.IsDefaultRule || string.IsNullOrEmpty(autoRule.TermId) || autoRule.TermId == Guid.Empty.ToString())
                    {
                        continue;
                    }
                    var term = TermDao.GetRMTermByGuId(new Guid(autoRule.TermId));
                    autoRule.TermIsDeprecated = term.IsDeprecated || TermDao.IsExpiredTerm(term.Id);
                    autoRule.TermIsRemoved = term.IsRemoved;
                }
            }
            catch (Exception e)
            {
                logger.Error("Set auto term status error:{0}", e.ToString());
            }
        }
        #endregion

        #region Modify Auto Rule TimeZone
        protected async System.Threading.Tasks.Task ConvertClassificationRuleTimeZoneAsync(List<ClassificationRule> autoRules)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                var gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var rule in autoRules)
                {
                    var filterGroups = rule.FilterGroups;
                    if (filterGroups == null)
                    {
                        continue;
                    }
                    ConvertFilterGroupsTimeZone(gls, filterGroups);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"ConvertClassificationRuleTimeZone Error: {e}");
            }
        }

        protected void ConvertClassificationRuleAndOrExpression(List<ClassificationRule> autoRules)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                foreach (var rule in autoRules)
                {
                    
                    if (rule.IsDefaultRule)
                    {
                        continue;
                    }
                    rule.AndOrExpression = rule.AndOrExpression.Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));
                }
            }
            catch (Exception e)
            {
                logger.Warn($"ConvertClassificationRuleAndOrExpression Error: {e}");
            }
        }

        public void ConvertFilterGroupsTimeZone(GeneralSettingModel gls, List<FilterGroup> filterGroups)
        {
            if (filterGroups == null || filterGroups.Count == 0)
            {
                return;
            }
            foreach (var filterGroup in filterGroups)
            {
                if (filterGroup.Filters == null)
                {
                    continue;
                }
                foreach (var f in filterGroup.Filters)
                {
                    ArchiverRuleFilter archiverRuleFilter = BuildArchiverRuleFilter(f);
                    RuleUtil.ModifyDisplayDateTimeByPolicyValue(archiverRuleFilter.Dto.BeginTime, archiverRuleFilter.Value1, gls);
                    RuleUtil.ModifyDisplayDateTimeByPolicyValue(archiverRuleFilter.Dto.EndTime, archiverRuleFilter.Value2, gls);
                    f.FilterCretia = archiverRuleFilter.FilterCretia();
                    f.StartTimeInfo = archiverRuleFilter.GetFilterDateTimeInfo(true);
                    f.EndTimeInfo = archiverRuleFilter.GetFilterDateTimeInfo(false);
                }
                ConvertFilterGroupsTimeZone(gls, filterGroup.FilterGroups);
            }
        }

        #endregion

        protected void UpdateJobVersion(string jobId, JobType jobType)
        {
            if (JobServiceUtility.SkipMergeDetailsJobs.Contains((int)jobType) && !string.IsNullOrEmpty(jobId))
            {
                try
                {
                    JobService.UpdateJobVersionAsync(jobId, JobVersion.UnMerged).ExecuteAsyncTask();
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to update job version for job {0}. Error: {1}", jobId, ex.ToString());
                }
            }
        }
    }

}
