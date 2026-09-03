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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Discovery.Model.Rule.Criteria;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Google.Apis.Calendar.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOTimeBaseFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter
{
    public class DAO2SORuleConverter
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RMDiscoveryAOSPOptimizationSetting mDOSettings = null;
        private bool UseArchiverProfile;
        private IRMDiscoveryAOSPBasicInfoQueryService mBasicInfoQueryService = PlatformWindsorManager.GetService<IRMDiscoveryAOSPBasicInfoQueryService>();

        private List<RMDiscoveryFileExtensionDataInfo> mRMDiscoveryFileExtensionDataInfos = null;
        public List<RMDiscoveryFileExtensionDataInfo> RMDiscoveryFileExtensionDataInfos
        {
            get
            {
                if (mRMDiscoveryFileExtensionDataInfos == null)
                {
                    mRMDiscoveryFileExtensionDataInfos = mBasicInfoQueryService.GetFileExtensionsAsync(mDOSettings.O365TenantId).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---RMDiscoveryFileExtensionDataInfos : {SerializerHelper.SerializeByJsonSerializer(mRMDiscoveryFileExtensionDataInfos)}");
                }
                return mRMDiscoveryFileExtensionDataInfos;
            }
            private set { }
        }

        private List<RMDiscoveryWithoutInDateDataInfo> mRMDiscoveryWithoutInDateDataInfos = null;
        public List<RMDiscoveryWithoutInDateDataInfo> RMDiscoveryWithoutInDateDataInfos
        {
            get
            {
                if (mRMDiscoveryWithoutInDateDataInfos == null)
                {
                    mRMDiscoveryWithoutInDateDataInfos = mBasicInfoQueryService.GetWithoutInDateListAsync(mDOSettings.O365TenantId).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---RMDiscoveryWithoutInDateDataInfos : {SerializerHelper.SerializeByJsonSerializer(mRMDiscoveryWithoutInDateDataInfos)}");
                }
                return mRMDiscoveryWithoutInDateDataInfos;
            }
            private set { }
        }

        private List<RMDiscoverySizeRangeDataInfo> mRMDiscoverySizeRangeDataInfos = null;
        public List<RMDiscoverySizeRangeDataInfo> RMDiscoverySizeRangeDataInfos
        {
            get
            {
                if (mRMDiscoverySizeRangeDataInfos == null)
                {
                    mRMDiscoverySizeRangeDataInfos = mBasicInfoQueryService.GetSizeRangeListAsync(mDOSettings.O365TenantId).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---RMDiscoverySizeRangeDataInfos : {SerializerHelper.SerializeByJsonSerializer(mRMDiscoverySizeRangeDataInfos)}");
                }
                return mRMDiscoverySizeRangeDataInfos;
            }
            private set { }
        }

        private List<RMDiscoveryAOSPRuleInfo> mInactiveRuleInfos = null;
        public List<RMDiscoveryAOSPRuleInfo> InactiveRuleInfos
        {
            get
            {
                if (mInactiveRuleInfos == null)
                {
                    mInactiveRuleInfos = DiscoverUtil.DiscoverUtil.GetAOSPInactiveRuleAsync(mDOSettings.O365TenantId, mDOSettings.InactiveRuleQueryParameter, mDOSettings.ArchiveDataType).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---InactiveRuleInfos : {SerializerHelper.SerializeByJsonSerializer(mInactiveRuleInfos)}");
                }
                return mInactiveRuleInfos;
            }
            private set { }
        }

        private List<RMDiscoveryAOSPRuleInfo> mROTRuleInfos = null;
        public List<RMDiscoveryAOSPRuleInfo> ROTRuleInfos
        {
            get
            {
                if (mROTRuleInfos == null)
                {
                    mROTRuleInfos = DiscoverUtil.DiscoverUtil.GetROTRuleAsync(mDOSettings.O365TenantId, mDOSettings.ROTRuleQueryParameter, mDOSettings.ArchiveDataType).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---ROTRuleInfos : {SerializerHelper.SerializeByJsonSerializer(mROTRuleInfos)}");
                }
                return mROTRuleInfos;
            }
            private set { }
        }
        private List<RMDiscoveryAOSPRuleInfo> mArchiverRuleInfos = null;
        public List<RMDiscoveryAOSPRuleInfo> ArchiverRuleInfos
        {
            get
            {
                if (mArchiverRuleInfos == null)
                {
                    mArchiverRuleInfos = new List<RMDiscoveryAOSPRuleInfo>();
                    foreach (var ruleDef in mDOSettings.RuleDefinition)
                    {
                        mArchiverRuleInfos.Add(DiscoverUtil.DiscoverUtil.ConvertDiscoverRuleDefinationToRMDiscoveryAOSPRuleInfo(ruleDef));
                    }
                    
                    mLog.Info($"---[Settings]---ArchiverRuleInfos : {SerializerHelper.SerializeByJsonSerializer(mDOSettings.RuleDefinition)}");
                }
                return mArchiverRuleInfos;
            }
            private set { }
        }
        public DAO2SORuleConverter(RMDiscoveryAOSPOptimizationSetting DOSettings,bool useArchiverProfile)
        {
            mDOSettings = DOSettings;
            UseArchiverProfile = useArchiverProfile;
        }

        public Rule GetScopeDocumentRule()
        {
            Rule scopeDocumentRule = new Rule();
            scopeDocumentRule.Id = Guid.NewGuid().ToString();
            scopeDocumentRule.Name = "Document Rule For Data Optimization Scope";
            scopeDocumentRule.PolicyLevel = GCommon.Contract.CommonFilter.PolicyLevel.Document;
            scopeDocumentRule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            scopeDocumentRule.Order = 1;
            //scopeDocumentRule.SOFilters = new List<SOFilterPolicy>();
            //scopeDocumentRule.Filters = new List<FilterPolicy>();
            scopeDocumentRule.AndOrExpression = new Dictionary<GCommon.Contract.CommonFilter.PolicyLevel, string>();
            StringBuilder mAndOrExpressionBuilder = new StringBuilder();
            List<FilterGroup> mFilterGroups = new List<FilterGroup>();

            int sequenceNo = 1;
            int order = 1;
            if (mDOSettings.FileExtensionQueryParameter.FileExtensions != null && mDOSettings.FileExtensionQueryParameter.FileExtensions.Count == 0)
            {
                //all file type, don't need add this criteria
            }
            else
            {
                var mFileExtensionInfos = RMDiscoveryFileExtensionDataInfos.Where(i => mDOSettings.FileExtensionQueryParameter.FileExtensions.Contains(i.Id)).ToList();
                List<string> mFileExtension = mFileExtensionInfos.ConvertAll(i => i.RealName).ToList();
                IDO2SOFilterConverter converter = new DO2SOFileExtensionFilter(mFileExtension, sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.In, false);
                List<SOFilterPolicy> filters = converter.Convert();
                FilterGroup fg = new FilterGroup(filters, converter.AndOrString, order, RMDiscoveryCriteriaLogicType.And);
                mFilterGroups.Add(fg);
                sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                sequenceNo++;
                order++;
            }

            if (mDOSettings.WithoutDateQueryParameter.From <= -1)
            {
                //all modified time, don't need add this criteria
            }
            else
            {
                var from = RMDiscoveryWithoutInDateDataInfos?.FirstOrDefault(i => i.Id == mDOSettings.WithoutDateQueryParameter.From);
                if (from != null)
                {
                    PolicyValueUnit valueUnit = PolicyValueUnit.None;
                    if (from.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Year)
                    {
                        valueUnit = PolicyValueUnit.Years;
                    }
                    else if (from.UnitType == Contract.Discovery.Model.RMDiscoveryWithoutInUnitType.Month)
                    {
                        valueUnit = PolicyValueUnit.Months;
                    }

                    IDO2SOFilterConverter converter = new DO2SOModifiedTimeFilter(from.Unit.ToString(), valueUnit, sequenceNo, DO2SOModifiedTimeFilter.TimeCondition.OlderThan, false);
                    List<SOFilterPolicy> filters = converter.Convert();
                    FilterGroup fg = new FilterGroup(filters, converter.AndOrString, order, RMDiscoveryCriteriaLogicType.And);
                    mFilterGroups.Add(fg);
                    sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                    sequenceNo++;
                    order++;
                }
            }

            if (mDOSettings.SizeRangeQueryParameter.SizeRange == 0 || mDOSettings.SizeRangeQueryParameter.QueryMode == RMDiscoveryAOSPSizeRangeQueryMode.None)
            {
                //all size, don't need add this criteria
            }
            else
            {
                var sizeRange = RMDiscoverySizeRangeDataInfos.FirstOrDefault(i => i.Id == mDOSettings.SizeRangeQueryParameter.SizeRange);
                if (sizeRange != null)
                {
                    bool needAddLessThan = false;
                    if (sizeRange.GenerateEqual > 0)
                    {
                        List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                        IDO2SOFilterConverter converter = null;
                        converter = new DO2SOSizeFilter(sizeRange.GenerateEqual.ToString(), PolicyValueUnit.MB, sequenceNo, DO2SOSizeFilter.SizeCondition.GreaterOrEqualThan, false);
                        filters.AddRange(converter.Convert());
                        FilterGroup fg = new FilterGroup(filters, converter.AndOrString, order, RMDiscoveryCriteriaLogicType.And);
                        mFilterGroups.Add(fg);
                        sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                        sequenceNo++;
                        order++;
                    }
                    else
                    {
                        needAddLessThan = true;
                    }

                    if (needAddLessThan && sizeRange.LessThan > 0)
                    {
                        List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                        IDO2SOFilterConverter converter = null;
                        converter = new DO2SOSizeFilter(sizeRange.LessThan.ToString(), PolicyValueUnit.MB, sequenceNo, DO2SOSizeFilter.SizeCondition.LessOrEqualThan, false);
                        filters.AddRange(converter.Convert());
                        FilterGroup fg = new FilterGroup(filters, converter.AndOrString, order, RMDiscoveryCriteriaLogicType.And);
                        mFilterGroups.Add(fg);
                        sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                        sequenceNo++;
                        order++;
                    }
                }
                else
                {
                    mLog.Warn($"Cannot find RMDiscoverySizeRangeDataInfos by id {mDOSettings.SizeRangeQueryParameter.SizeRange}");
                }
            }
            scopeDocumentRule.AndOrExpression.Add(GCommon.Contract.CommonFilter.PolicyLevel.Document, DO2SOFilterConverterBase.GetMergedAndOrExpression(mFilterGroups));
            var mergedFilters = DO2SOFilterConverterBase.GetMergedFilters(mFilterGroups);
            scopeDocumentRule.SOFilters = mergedFilters;
            scopeDocumentRule.Filters = RA.Common.Util.RMDtoConverter.ConvertCommonSOFiletrPolicyToCommonFilterPolicy(mergedFilters);

            mLog.Info($"---[Settings]---ScopeDocumentRule : {SerializerHelper.SerializeByJsonSerializer(scopeDocumentRule)}");

            return scopeDocumentRule;
        }

        public List<Rule> GetDocumentTagRules()
        {
            List<Rule> rules = new List<Rule>();
            List<RMDiscoveryAOSPRuleInfo> discoveryRules = new List<RMDiscoveryAOSPRuleInfo>();
            List<RMDiscoveryAOSPRuleInfo> _RotRuleInfos = null;
            if (UseArchiverProfile)
            {
                mLog.Info("current rule is discover archiver rule,will use archiver profile");
                _RotRuleInfos = ArchiverRuleInfos;
            }
            else
            {
                _RotRuleInfos = ROTRuleInfos;
            }
            if (_RotRuleInfos != null && _RotRuleInfos.Count > 0)
            {
                foreach (var rule in _RotRuleInfos)
                {
                    if (rule.AnalyseMethod == Contract.Discovery.Model.Rule.RMDiscoveryRuleAnalyseMethod.Document)
                    {
                        discoveryRules.Add(rule);
                    }
                }
            }

            int ruleOrder = 1;
            int sequenceNo = 1;
            foreach (var disRule in discoveryRules)
            {
                List<FilterGroup> mFilterGroups = new List<FilterGroup>();

                var mRuleDefinition = RMDiscoveryRuleConverter.Convert(disRule);
                foreach (RMDiscoveryRuleCriteriaInfo criteriaInfo in mRuleDefinition.CriteriaInfoes.OrderBy(c => c.Order))
                {
                    List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                    IDO2SOFilterConverter converter = GetDocumentConverter(criteriaInfo, sequenceNo, UseArchiverProfile);
                    filters.AddRange(converter.Convert());
                    FilterGroup fg = new FilterGroup(filters, converter.AndOrString, criteriaInfo.Order, criteriaInfo.LogicType);
                    mFilterGroups.Add(fg);
                    sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                    sequenceNo++;
                }

                Rule DocumentTagRule = new Rule();
                if (UseArchiverProfile)
                {
                    AddRuleActionForRule(DocumentTagRule, disRule.ProcessActionParameter);
                    DocumentTagRule.Id = disRule.UniqueId.ToString();
                }
                else
                {
                    DocumentTagRule.Id = Guid.NewGuid().ToString();
                }

                DocumentTagRule.Name = $"Document Tag Rule For Data Optimization {ruleOrder}";
                DocumentTagRule.PolicyLevel = GCommon.Contract.CommonFilter.PolicyLevel.Document;
                DocumentTagRule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
                DocumentTagRule.Order = ruleOrder;
                DocumentTagRule.AndOrExpression = new Dictionary<GCommon.Contract.CommonFilter.PolicyLevel, string>();
                DocumentTagRule.AndOrExpression.Add(GCommon.Contract.CommonFilter.PolicyLevel.Document, DO2SOFilterConverterBase.GetMergedAndOrExpression(mFilterGroups));
                var mergedFilters = DO2SOFilterConverterBase.GetMergedFilters(mFilterGroups);
                DocumentTagRule.SOFilters = mergedFilters;
                DocumentTagRule.Filters = RA.Common.Util.RMDtoConverter.ConvertCommonSOFiletrPolicyToCommonFilterPolicy(mergedFilters);

                ruleOrder++;
                rules.Add(DocumentTagRule);
            }

            mLog.Info($"---[Settings]---DocumentTagRules : {SerializerHelper.SerializeByJsonSerializer(rules)}");

            return rules;
        }
        private void AddRuleActionForRule(Rule rule,string processAction)
        {
            ProcessActionParameter ProcessActionParameter = JsonConvert.DeserializeObject<ProcessActionParameter>(processAction);
            rule.StubTemplateId = ProcessActionParameter.StubSettingDto?.Id;
            rule.StubTemplateName = ProcessActionParameter.StubSettingDto?.Name;
            //rule.MoveToAnotherTierType = moveDataTierType;
            //if (isFileLevelAction)
            //{
                rule.PolicyLevel = PolicyLevel.Document;
                if (ProcessActionParameter.FileAction == FileAction.ArchiveAndRemove)
                {
                    if (ProcessActionParameter.IsEnableLeaveStub)
                    {
                        rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
                    }
                    else
                    {
                        rule.KeepDataOption = (int)KeepDataOption.ArchiveBackupAndRemove;
                    }
                }
                else if (ProcessActionParameter.FileAction == FileAction.Remove)
                {
                    rule.KeepDataOption = (int)KeepDataOption.DeleteOnly;
                }
                else if (ProcessActionParameter.FileAction == FileAction.Archive)
                {
                    rule.KeepDataOption = (int)KeepDataOption.ArchiverOnly;
                    if (ProcessActionParameter.EnableArchivedOnlyLatestVersion)
                    {
                        rule.KeepDataOption += (int)KeepDataOption.ArchiveOnlyLastestVersion;
                        rule.ArchiverOnlyLastestVersion =ProcessActionParameter.ArchivedOnlyLatestVersion;
                    }
                }

                if (ProcessActionParameter.DeleteRecords || rule.KeepDataOption == (int)KeepDataOption.ArchiverOnly)
                {
                    rule.DeleteRecords = true;
                }
                if (ProcessActionParameter != null && ProcessActionParameter.EnableArchivedLatestVersion)
                {
                    rule.KeepDataOption += (int)KeepDataOption.ArchiveLatestVersion;
                    rule.ArchivedLatestVersion =ProcessActionParameter.ArchivedLatestVersion;
                }
            //}
            //else
            //{
            //    rule.PolicyLevel = PolicyLevel.DocumentVersion;
            //    if (info.ProcessActionParameter.VersionAction == VersionAction.ArchiveAndRemoveVerison)
            //    {
            //        rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.ArchiveBackupAndRemove;
            //    }
            //    else if (info.ProcessActionParameter.VersionAction == VersionAction.RemoveVersion)
            //    {
            //        rule.KeepDataOption = (int)AvePoint.GCommon.Contract.StorageOptimization.Object.KeepDataOption.DeleteOnly;
            //    }
            //}
        }
        public List<Rule> GetDocumentVersionTagRules()
        {
            List<Rule> rules = new List<Rule>();
            List<RMDiscoveryAOSPRuleInfo> discoveryRules = new List<RMDiscoveryAOSPRuleInfo>();
            var _RotRuleInfos = ROTRuleInfos;
            if (UseArchiverProfile)
            {
                mLog.Info("current rule is discover archiver rule,will use archiver profile");
                _RotRuleInfos = ArchiverRuleInfos;
            }
            if (_RotRuleInfos != null && _RotRuleInfos.Count > 0)
            {
                foreach (var rule in _RotRuleInfos)
                {
                    if (rule.AnalyseMethod == Contract.Discovery.Model.Rule.RMDiscoveryRuleAnalyseMethod.Version)
                    {
                        discoveryRules.Add(rule);
                    }
                }
            }
            if (!UseArchiverProfile)
            {
                var _InactiveRuleInfos = InactiveRuleInfos;
                if (_InactiveRuleInfos != null && _InactiveRuleInfos.Count > 0)
                {
                    foreach (var rule in _InactiveRuleInfos)
                    {
                        if (rule.AnalyseMethod == Contract.Discovery.Model.Rule.RMDiscoveryRuleAnalyseMethod.Version)
                        {
                            discoveryRules.Add(rule);
                        }
                    }
                }
            }
            int ruleOrder = 1;
            int sequenceNo = 1;
            foreach (var disRule in discoveryRules)
            {
                List<FilterGroup> mFilterGroups = new List<FilterGroup>();

                var mRuleDefinition = RMDiscoveryRuleConverter.Convert(disRule);
                foreach (RMDiscoveryRuleCriteriaInfo criteriaInfo in mRuleDefinition.CriteriaInfoes.OrderBy(c => c.Order))
                {
                    List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                    IDO2SOFilterConverter converter = GetDocumentVersionConverter(criteriaInfo, sequenceNo, UseArchiverProfile);
                    filters.AddRange(converter.Convert());
                    FilterGroup fg = new FilterGroup(filters, converter.AndOrString, criteriaInfo.Order, criteriaInfo.LogicType);
                    mFilterGroups.Add(fg);
                    sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                    sequenceNo++;
                }

                Rule DocumentVersionTagRule = new Rule();
                if (UseArchiverProfile)
                {
                    AddRuleActionForRule(DocumentVersionTagRule, disRule.ProcessActionParameter);
                    DocumentVersionTagRule.Id = disRule.UniqueId.ToString();
                }
                else
                {
                    DocumentVersionTagRule.Id = Guid.NewGuid().ToString();
                }
                DocumentVersionTagRule.Name = $"Document Tag Rule For Data Optimization {ruleOrder}";
                DocumentVersionTagRule.PolicyLevel = GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion;
                DocumentVersionTagRule.ProfileType = GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
                DocumentVersionTagRule.Order = ruleOrder;
                DocumentVersionTagRule.AndOrExpression = new Dictionary<GCommon.Contract.CommonFilter.PolicyLevel, string>();
                DocumentVersionTagRule.AndOrExpression.Add(GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion, DO2SOFilterConverterBase.GetMergedAndOrExpression(mFilterGroups));
                var mergedFilters = DO2SOFilterConverterBase.GetMergedFilters(mFilterGroups);
                DocumentVersionTagRule.SOFilters = mergedFilters;
                DocumentVersionTagRule.Filters = RA.Common.Util.RMDtoConverter.ConvertCommonSOFiletrPolicyToCommonFilterPolicy(mergedFilters);

                ruleOrder++;

                rules.Add(DocumentVersionTagRule);
            }

            mLog.Info($"---[Settings]---DocumentVersionTagRules : {SerializerHelper.SerializeByJsonSerializer(rules)}");

            return rules;
        }

        private IDO2SOFilterConverter GetDocumentConverter(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo,bool useArchiveProfile)
        {
            IDO2SOFilterConverter converter = null;

            if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.Name)
            {
                converter = GetConverterForDocumentName(criteriaInfo, sequenceNo, useArchiveProfile);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.CreatedTime)
            {
                converter = GetConverterForCreatedTime(criteriaInfo, sequenceNo, false);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ModifiedTime)
            {
                converter = GetConverterForModifiedTime(criteriaInfo, sequenceNo, false);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.DocumentType)
            {
                converter = GetConverterForDocumentType(criteriaInfo, sequenceNo, false);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.DocumentSize)
            {
                converter = GetConverterForDocumentSize(criteriaInfo, sequenceNo, false);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentFolder)
            {
                converter = GetConverterForParentFolder(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.CreateBy)
            {
                converter = GetConverterForCreatedBy(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ModifiedBy)
            {
                converter = GetConverterForModifiedBy(criteriaInfo, sequenceNo,false);
            }
            else
            {
                mLog.Warn($"Not support CriteriaType : {criteriaInfo.CriteriaType}");
            }

            if (converter == null)
            {
                mLog.Error($"Convert DocumentTagRule failed : {JsonConvert.SerializeObject(criteriaInfo)}");
                throw new ScheduleJobConfigurationError();
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForCreatedBy(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            List<string> values = null;
            values = new List<string>() { criteriaInfo.ConditionInfo.Value };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileUserConditionType.Contains)
            {
                converter = new DO2SOCreatedByFilter(values, sequenceNo, DO2SOCreatedByFilter.CreatedByCondition.Contains);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileUserConditionType.Equal)
            {
                converter = new DO2SOCreatedByFilter(values, sequenceNo, DO2SOCreatedByFilter.CreatedByCondition.Equals);
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForModifiedBy(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool isVersionFilter)
        {
            IDO2SOFilterConverter converter = null;
            List<string> values = null;
            values = new List<string>() { criteriaInfo.ConditionInfo.Value };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileUserConditionType.Contains)
            {
                converter = new DO2SOModifiedByFilter(values, sequenceNo, DO2SOModifiedByFilter.ModifiedByCondition.Contains, isVersionFilter);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileUserConditionType.Equal)
            {
                converter = new DO2SOModifiedByFilter(values, sequenceNo, DO2SOModifiedByFilter.ModifiedByCondition.Equals, isVersionFilter);
            }
            return converter;
        }
        private IDO2SOFilterConverter GetDocumentVersionConverter(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo ,bool useArchiveProfile)
        {
            IDO2SOFilterConverter converter = null;

            if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.KeepLastVersions)
            {
                converter = GetConverterForKeepLastVersions(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.ModifiedTime)
            {
                converter = GetConverterForModifiedTime(criteriaInfo, sequenceNo, true);
            }

            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.DocumentType)
            {
                converter = GetConverterForDocumentType(criteriaInfo, sequenceNo, true);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.DocumentSize)
            {
                converter = GetConverterForDocumentSize(criteriaInfo, sequenceNo, true);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.Name)
            {
                converter = GetConverterForVersionName(criteriaInfo, sequenceNo, useArchiveProfile);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.Title)
            {
                converter = GetConverterForVersionTitle(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.DocumentModifiedTime)
            {
                converter = GetConverterForDocumentModifiedTimeForVersion(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryVersionCriteriaType.ModifiedBy)
            {
                converter = GetConverterForModifiedBy(criteriaInfo, sequenceNo, true);
            }
            else
            {
                mLog.Warn($"Not support CriteriaType : {criteriaInfo.CriteriaType}");
            }

            if (converter == null)
            {
                mLog.Error($"Convert DocumentVersionTagRule failed : {JsonConvert.SerializeObject(criteriaInfo)}");
                throw new ScheduleJobConfigurationError();
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForVersionTitle(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            List<string> values = null;
            values = new List<string>() { criteriaInfo.ConditionInfo.Value };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextMatchIn)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.TextMatchIn);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextNotMatchIn)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.TextMatchNotIn);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Contains)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.Contains);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotContains)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.NotContains);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Equal)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.Equals);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotEqual)
            {
                converter = new DO2SOVersionTitleFilter(values, sequenceNo, DO2SOVersionTitleFilter.VersionTitleCondition.NotEquals);
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForDocumentName(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo,bool useArchiveProfile)
        {
            IDO2SOFilterConverter converter = null;
            List<string> values = null;
            if (useArchiveProfile)
            {
                values = new List<string>() { criteriaInfo.ConditionInfo.Value };
                if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextNotMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchNotIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Contains)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.Contains);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotContains)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.NotContains);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Equal)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.Equals);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotEqual)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.NotEquals);
                }
            }
            else
            {
                values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);

                if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchNotIn);
                }
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForVersionName(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool useArchiveProfile)
        {
            IDO2SOFilterConverter converter = null;
            List<string> values = null;
            if (useArchiveProfile)
            {
                values = new List<string>() { criteriaInfo.ConditionInfo.Value };
                if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextMatchIn)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.TextMatchIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.TextNotMatchIn)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.TextMatchNotIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Contains)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.Contains);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotContains)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.NotContains);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.Equal)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.Equals);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMAOSPArchiveProfileConditionType.NotEqual)
                {
                    converter = new DO2SOVersionNameFilter(values, sequenceNo, DO2SOVersionNameFilter.VersionNameCondition.NotEquals);
                }
            }
            else
            {
                values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);

                if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchIn);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn)
                {
                    converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchNotIn);
                }
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForDocumentType(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool isVersionFilter)
        {
            IDO2SOFilterConverter converter = null;
            if (criteriaInfo.ConditionInfo.Category == RMDiscoveryConditionCategory.BooleanLogic)
            {
                if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryBooleanConditionType.IsEmpty)
                {
                    if (criteriaInfo.ConditionInfo.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        converter = new DO2SOFileExtensionFilter(new List<string>(), sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.IsEmpty, isVersionFilter);
                    }
                    else
                    {
                        converter = new DO2SOFileExtensionFilter(new List<string>(), sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.IsNotEmpty, isVersionFilter);
                    }
                }
            }
            else
            {
                if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.In)
                {
                    var values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);
                    converter = new DO2SOFileExtensionFilter(values, sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.In, isVersionFilter);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.NotIn)
                {
                    var values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);
                    converter = new DO2SOFileExtensionFilter(values, sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.NotIn, isVersionFilter);
                }
                else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.None)
                {
                    if (criteriaInfo.ConditionInfo.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        converter = new DO2SOFileExtensionFilter(new List<string>(), sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.IsEmpty, isVersionFilter);
                    }
                    else
                    {
                        converter = new DO2SOFileExtensionFilter(new List<string>(), sequenceNo, DO2SOFileExtensionFilter.FileExtensionCondition.IsNotEmpty, isVersionFilter);
                    }
                }
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForDocumentModifiedTimeForVersion(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var value = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SODocumentModifiedTimeForVersionFilter(value.Unit.ToString(), DO2SOConvertUtils.ConvertDateTimeUnitType(value.UnitType), sequenceNo, TimeCondition.OlderThan);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                DateTime dateTime;
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SODocumentModifiedTimeForVersionFilter(value1, PolicyValueUnit.None, sequenceNo, TimeCondition.Before);
                }
                else
                {
                    mLog.Error($"Convert Document ModifiedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.FromTo)
            {
                converter = CreateTimeRangeConverter(criteriaInfo.ConditionInfo.Value, sequenceNo, true, (value1, value2) => new DO2SODocumentModifiedTimeForVersionFilter(value1, value2, PolicyValueUnit.None, sequenceNo, TimeCondition.FromTo), "Document ModifiedTime");
            }
            return converter;
        }
        private IDO2SOFilterConverter GetConverterForModifiedTime(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool isVersionFilter)
        {
            IDO2SOFilterConverter converter = null;
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var value = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SOModifiedTimeFilter(value.Unit.ToString(), DO2SOConvertUtils.ConvertDateTimeUnitType(value.UnitType), sequenceNo, TimeCondition.OlderThan, isVersionFilter);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                DateTime dateTime;
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SOModifiedTimeFilter(value1, PolicyValueUnit.None, sequenceNo, TimeCondition.Before, isVersionFilter);
                }
                else
                {
                    mLog.Error($"Convert ModifiedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.FromTo)
            {
                converter = CreateTimeRangeConverter(criteriaInfo.ConditionInfo.Value, sequenceNo, isVersionFilter, (value1, value2) => new DO2SOModifiedTimeFilter(value1, value2, PolicyValueUnit.None, sequenceNo, TimeCondition.FromTo, isVersionFilter), "ModifiedTime");
            }
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForCreatedTime(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool isVersionFilter)
        {
            IDO2SOFilterConverter converter = null;
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var value = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SOCreatedTimeFilter(value.Unit.ToString(), DO2SOConvertUtils.ConvertDateTimeUnitType(value.UnitType), sequenceNo, TimeCondition.OlderThan, isVersionFilter);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                DateTime dateTime;
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SOCreatedTimeFilter(value1, PolicyValueUnit.None, sequenceNo, TimeCondition.Before, isVersionFilter);
                }
                else
                {
                    mLog.Error($"Convert CreatedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.FromTo)
            {
                converter = CreateTimeRangeConverter(criteriaInfo.ConditionInfo.Value, sequenceNo, isVersionFilter, (value1, value2) => new DO2SOCreatedTimeFilter(value1, value2, PolicyValueUnit.None, sequenceNo, TimeCondition.FromTo, isVersionFilter), "CreatedTime");
            }
            return converter;
        }

        private IDO2SOFilterConverter CreateTimeRangeConverter(string conditionValue, int sequenceNo, bool isVersionFilter, Func<string, string, IDO2SOFilterConverter> factory, string ruleName)
        {
            var value = JsonConvert.DeserializeObject<RMDiscoveryDateConditionFromToInfo>(conditionValue);
            if (value == null || !DateTime.TryParse(value.Value1, out var startTime) || !DateTime.TryParse(value.Value2, out var endTime))
            {
                mLog.Error($"Convert {ruleName} Criteria failed, convert FromTo value to DateTime error : value {conditionValue}");
                return null;
            }

            return factory(startTime.ToString(APIDateTimeFormat.DATETYPEForAPI003), endTime.ToString(APIDateTimeFormat.DATETYPEForAPI003));
        }

        private IDO2SOFilterConverter GetConverterForDocumentSize(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo, bool isVersionFilter)
        {
            IDO2SOFilterConverter converter = null;
            var value = JsonConvert.DeserializeObject<RMDiscoveryFileSizeConditionValue>(criteriaInfo.ConditionInfo.Value);
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryFileSizeConditionType.GreaterThanEquals)
            {
                converter = new DO2SOSizeFilter(value.Unit.ToString(), DO2SOConvertUtils.ConvertFileSizeUnitType(value.UnitType), sequenceNo, DO2SOSizeFilter.SizeCondition.GreaterOrEqualThan, isVersionFilter);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryFileSizeConditionType.LessThanEquals)
            {
                converter = new DO2SOSizeFilter(value.Unit.ToString(), DO2SOConvertUtils.ConvertFileSizeUnitType(value.UnitType), sequenceNo, DO2SOSizeFilter.SizeCondition.LessOrEqualThan, isVersionFilter);
            }
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForKeepLastVersions(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = JsonConvert.DeserializeObject<string>(criteriaInfo.ConditionInfo.Value);
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryVersionConditionType.MajorAndMinor)
            {
                converter = new DO2SOKeepLastVersionFilter(values, sequenceNo, DO2SOKeepLastVersionFilter.KeepLastVersionCondition.MajorAndMintorVersions);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryVersionConditionType.MajorAndNoMinor)
            {
                converter = new DO2SOKeepLastVersionFilter(values, sequenceNo, DO2SOKeepLastVersionFilter.KeepLastVersionCondition.MajorWithoutMinorVersions);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryVersionConditionType.MinorVersionOfEachMajor)
            {
                converter = new DO2SOKeepLastVersionFilter(values, sequenceNo, DO2SOKeepLastVersionFilter.KeepLastVersionCondition.MinorOfEachMajorVersion);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryVersionConditionType.MinorVersionsOfLatestMajor)
            {
                converter = new DO2SOKeepLastVersionFilter(values, sequenceNo, DO2SOKeepLastVersionFilter.KeepLastVersionCondition.MinorOfTheLatestMajorVersion);
            }
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForParentFolder(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextMatchIn)
            {
                converter = new DO2SODocumentParentFolderFilter(values, sequenceNo, DO2SODocumentParentFolderFilter.DocumentParentFolderCondition.TextMatchIn);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn)
            {
                converter = new DO2SODocumentParentFolderFilter(values, sequenceNo, DO2SODocumentParentFolderFilter.DocumentParentFolderCondition.TextMatchNotIn);
            }
            return converter;
        }
    }
}
