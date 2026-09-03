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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Common;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOFileExtensionFilter;
using AvePoint.GCommon.Contract.CommonFilter;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOTimeBaseFilter;
using DocumentFormat.OpenXml.Spreadsheet;
using static System.Resources.NetStandard.ResXFileRef;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Rule.Criteria;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using Newtonsoft.Json;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Utility;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using DocumentFormat.OpenXml.Drawing.Charts;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter
{
    public class DO2SORuleConverter
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RMDiscoveryOffice365OptimizationSetting mDOSettings = null;
        private IRMDiscoveryOffice365BasicInfoQueryService mBasicInfoQueryService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365BasicInfoQueryService>();

        private List<RMDiscoveryFileExtensionDataInfo> mRMDiscoveryFileExtensionDataInfos = null;
        public List<RMDiscoveryFileExtensionDataInfo> RMDiscoveryFileExtensionDataInfos
        {
            get
            {
                if (mRMDiscoveryFileExtensionDataInfos == null)
                {
                    mRMDiscoveryFileExtensionDataInfos = mBasicInfoQueryService.GetFileExtensionsAsync(new Guid(mDOSettings.O365TenantId)).GetAwaiter().GetResult();
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
                    mRMDiscoveryWithoutInDateDataInfos = mBasicInfoQueryService.GetWithoutInDateListAsync().GetAwaiter().GetResult();
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
                    mRMDiscoverySizeRangeDataInfos = mBasicInfoQueryService.GetSizeRangeListAsync().GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---RMDiscoverySizeRangeDataInfos : {SerializerHelper.SerializeByJsonSerializer(mRMDiscoverySizeRangeDataInfos)}");
                }
                return mRMDiscoverySizeRangeDataInfos;
            }
            private set { }
        }

        private List<RMDiscoveryOffice365RuleInfo> mInactiveRuleInfos = null;
        public List<RMDiscoveryOffice365RuleInfo> InactiveRuleInfos
        {
            get
            {
                if (mInactiveRuleInfos == null)
                {
                    mInactiveRuleInfos = DiscoverUtil.DiscoverUtil.GetInactiveRuleAsync(mDOSettings.InactiveRuleQueryParameter, mDOSettings.ArchiveDataType).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---InactiveRuleInfos : {SerializerHelper.SerializeByJsonSerializer(mInactiveRuleInfos)}");
                }
                return mInactiveRuleInfos;
            }
            private set { }
        }

        private List<RMDiscoveryOffice365RuleInfo> mROTRuleInfos = null;
        public List<RMDiscoveryOffice365RuleInfo> ROTRuleInfos
        {
            get
            {
                if (mROTRuleInfos == null)
                {
                    mROTRuleInfos = DiscoverUtil.DiscoverUtil.GetROTRuleAsync(mDOSettings.ROTRuleQueryParameter, mDOSettings.ArchiveDataType).GetAwaiter().GetResult();
                    mLog.Info($"---[Settings]---ROTRuleInfos : {SerializerHelper.SerializeByJsonSerializer(mROTRuleInfos)}");
                }
                return mROTRuleInfos;
            }
            private set { }
        }

        public DO2SORuleConverter(RMDiscoveryOffice365OptimizationSetting DOSettings)
        {
            mDOSettings = DOSettings;
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

            if (mDOSettings.SizeRangeQueryParameter.SizeRange == 0 || mDOSettings.SizeRangeQueryParameter.QueryMode == RMDiscoverySizeRangeQueryMode.None)
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
            List<RMDiscoveryOffice365RuleInfo> discoveryRules = new List<RMDiscoveryOffice365RuleInfo>();
            var _RotRuleInfos = ROTRuleInfos;
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
                foreach (RMDiscoveryRuleCriteriaInfo criteriaInfo in mRuleDefinition.CriteriaInfoes.OrderBy(c=>c.Order))
                {
                    List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                    IDO2SOFilterConverter converter = GetDocumentConverter(criteriaInfo, sequenceNo);
                    filters.AddRange(converter.Convert());
                    FilterGroup fg = new FilterGroup(filters, converter.AndOrString, criteriaInfo.Order, criteriaInfo.LogicType);
                    mFilterGroups.Add(fg);
                    sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                    sequenceNo++;
                }

                Rule DocumentTagRule = new Rule();
                DocumentTagRule.Id = Guid.NewGuid().ToString();
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

        public List<Rule> GetDocumentVersionTagRules()
        {
            List<Rule> rules = new List<Rule>();
            List<RMDiscoveryOffice365RuleInfo> discoveryRules = new List<RMDiscoveryOffice365RuleInfo>();

            var _RotRuleInfos = ROTRuleInfos;
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

            int ruleOrder = 1;
            int sequenceNo = 1;
            foreach (var disRule in discoveryRules)
            {
                List<FilterGroup> mFilterGroups = new List<FilterGroup>();

                var mRuleDefinition = RMDiscoveryRuleConverter.Convert(disRule);
                foreach (RMDiscoveryRuleCriteriaInfo criteriaInfo in mRuleDefinition.CriteriaInfoes.OrderBy(c => c.Order))
                {
                    List<SOFilterPolicy> filters = new List<SOFilterPolicy>();
                    IDO2SOFilterConverter converter = GetDocumentVersionConverter(criteriaInfo, sequenceNo);
                    filters.AddRange(converter.Convert());
                    FilterGroup fg = new FilterGroup(filters, converter.AndOrString, criteriaInfo.Order, criteriaInfo.LogicType);
                    mFilterGroups.Add(fg);
                    sequenceNo = DO2SOFilterConverterBase.GetLastSequenceNo(filters);
                    sequenceNo++;
                }

                Rule DocumentVersionTagRule = new Rule();
                DocumentVersionTagRule.Id = Guid.NewGuid().ToString();
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

        private IDO2SOFilterConverter GetDocumentConverter(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;

            if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.Name)
            {
                converter = GetConverterForDocumentName(criteriaInfo, sequenceNo);
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
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentSiteCollectionBoolean)
            {
                converter = GetConverterForParentSiteCollectionBoolean(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentLibraryBoolean)
            {
                converter = GetConverterForParentLibraryBoolean(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentSiteCollectionDateTime)
            {
                converter = GetConverterForParentSiteCollectionDateTime(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentLibraryDateTime)
            {
                converter = GetConverterForParentLibraryDateTime(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentSiteCollectionText)
            {
                converter = GetConverterForParentSiteCollectionText(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentLibraryText)
            {
                converter = GetConverterForParentLibraryText(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentSiteCollectionNumber)
            {
                converter = GetConverterForParentSiteCollectionNumber(criteriaInfo, sequenceNo);
            }
            else if (criteriaInfo.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ParentLibraryNumber)
            {
                converter = GetConverterForParentLibraryNumber(criteriaInfo, sequenceNo);
            }
            else
            {
                switch(criteriaInfo.CriteriaType)
                {
                    case (int)RMDiscoveryDocumentCriteriaType.PropertyBagText:
                        converter = GetConverterForPropertyBagText(criteriaInfo, sequenceNo);
                        break;
                    case (int)RMDiscoveryDocumentCriteriaType.PropertyBagNumber:
                        converter = GetConverterForPropertyBagNumber(criteriaInfo, sequenceNo);
                        break;
                    case (int)RMDiscoveryDocumentCriteriaType.PropertyBagBoolean:
                        converter = GetConverterForPropertyBagBoolean(criteriaInfo, sequenceNo);
                        break;
                    case (int)RMDiscoveryDocumentCriteriaType.PropertyBagDateTime:
                        converter = GetConverterForPropertyBagDateTime(criteriaInfo, sequenceNo);
                        break;
                    default:
                        mLog.Warn($"Not support CriteriaType : {criteriaInfo.CriteriaType}");
                        break;
                }
            }

            if (converter == null)
            {
                mLog.Error($"Convert DocumentTagRule failed : {JsonConvert.SerializeObject(criteriaInfo)}");
                throw new ScheduleJobConfigurationError();
            }
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForPropertyBagDateTime(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            DateTimeConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMDiscoveryDateTimeConditionType.OlderThan => DateTimeConditionType.OlderThan,
                (int)RMDiscoveryDateTimeConditionType.Before => DateTimeConditionType.Before,
                _ => DateTimeConditionType.None,
            };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out var dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SOPropertyBagDateTime(criteriaInfo.ConditionInfo.Value, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, PolicyValueUnit.None);
                }
                else
                {
                    mLog.Error($"Convert CreatedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var values = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SOPropertyBagDateTime(values.Unit.ToString(), sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, DO2SOConvertUtils.ConvertDateTimeUnitType(values.UnitType));
            }
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForPropertyBagBoolean(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            BooleanConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMBooleanExtraInputConditionType.Equals => BooleanConditionType.Equals,
                (int)RMBooleanExtraInputConditionType.DoesNotEqual => BooleanConditionType.DoesNotEqual,
                _ => BooleanConditionType.None,
            };
            converter = new DO2SOPropertyBagBoolean(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForPropertyBagNumber(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            NumberConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMNumberExtraInputConditionType.GreaterThanEquals => NumberConditionType.GreaterThanEquals,
                (int)RMNumberExtraInputConditionType.LessThanEquals => NumberConditionType.LessThanEquals,
                _ => NumberConditionType.None,
            };
            converter = new DO2SOPropertyBagNumber(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private IDO2SOFilterConverter GetConverterForPropertyBagText(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            TextConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMTextExtraConditionType.Equals => TextConditionType.Equals,
                (int)RMTextExtraConditionType.DoesNotEqual => TextConditionType.DoesNotEqual,
                (int)RMTextExtraConditionType.Matches => TextConditionType.Matches,
                (int)RMTextExtraConditionType.DoesNotMatches => TextConditionType.DoesNotMatch,
                (int)RMTextExtraConditionType.Contains => TextConditionType.Contains,
                (int)RMTextExtraConditionType.DoesNotContain => TextConditionType.DoesNotContain,
                _ => TextConditionType.None,
            };
            converter = new DO2SOPropertyBagText(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentSiteCollectionBoolean(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            BooleanConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMBooleanExtraInputConditionType.Equals => BooleanConditionType.Equals,
                (int)RMBooleanExtraInputConditionType.DoesNotEqual => BooleanConditionType.DoesNotEqual,
                _ => BooleanConditionType.None,
            };
            converter = new DO2SOParentSiteCollectionBoolean(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentLibraryBoolean(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            BooleanConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMBooleanExtraInputConditionType.Equals => BooleanConditionType.Equals,
                (int)RMBooleanExtraInputConditionType.DoesNotEqual => BooleanConditionType.DoesNotEqual,
                _ => BooleanConditionType.None,
            };
            converter = new DO2SOParentLibraryBoolean(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentSiteCollectionDateTime(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            DateTimeConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMDiscoveryDateTimeConditionType.OlderThan => DateTimeConditionType.OlderThan,
                (int)RMDiscoveryDateTimeConditionType.Before => DateTimeConditionType.Before,
                _ => DateTimeConditionType.None,
            };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out var dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SOParentSiteCollectionDateTime(criteriaInfo.ConditionInfo.Value, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, PolicyValueUnit.None);
                }
                else
                {
                    mLog.Error($"Convert CreatedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var values = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SOParentSiteCollectionDateTime(values.Unit.ToString(), sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, DO2SOConvertUtils.ConvertDateTimeUnitType(values.UnitType));
            }
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentLibraryDateTime(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            DateTimeConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMDiscoveryDateTimeConditionType.OlderThan => DateTimeConditionType.OlderThan,
                (int)RMDiscoveryDateTimeConditionType.Before => DateTimeConditionType.Before,
                _ => DateTimeConditionType.None,
            };
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before)
            {
                string value1 = string.Empty;
                if (DateTime.TryParse(criteriaInfo.ConditionInfo.Value, out var dateTime))
                {
                    value1 = dateTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    converter = new DO2SOParentLibraryDateTime(criteriaInfo.ConditionInfo.Value, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, PolicyValueUnit.None);
                }
                else
                {
                    mLog.Error($"Convert CreatedTime Criteria failed, convert to DateTime error : value {criteriaInfo.ConditionInfo.Value}");
                }
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.OlderThan)
            {
                var values = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criteriaInfo.ConditionInfo.Value);
                converter = new DO2SOParentLibraryDateTime(values.Unit.ToString(), sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue, DO2SOConvertUtils.ConvertDateTimeUnitType(values.UnitType));
            }
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentSiteCollectionText(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            TextConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMTextExtraConditionType.Equals => TextConditionType.Equals,
                (int)RMTextExtraConditionType.DoesNotEqual => TextConditionType.DoesNotEqual,
                (int)RMTextExtraConditionType.Matches => TextConditionType.Matches,
                (int)RMTextExtraConditionType.DoesNotMatches => TextConditionType.DoesNotMatch,
                (int)RMTextExtraConditionType.Contains => TextConditionType.Contains,
                (int)RMTextExtraConditionType.DoesNotContain => TextConditionType.DoesNotContain,
                _ => TextConditionType.None,
            };
            converter = new DO2SOParentSiteCollectionText(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentLibraryText(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            TextConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMTextExtraConditionType.Equals => TextConditionType.Equals,
                (int)RMTextExtraConditionType.DoesNotEqual => TextConditionType.DoesNotEqual,
                (int)RMTextExtraConditionType.Matches => TextConditionType.Matches,
                (int)RMTextExtraConditionType.DoesNotMatches => TextConditionType.DoesNotMatch,
                (int)RMTextExtraConditionType.Contains => TextConditionType.Contains,
                (int)RMTextExtraConditionType.DoesNotContain => TextConditionType.DoesNotContain,
                _ => TextConditionType.None,
            };
            converter = new DO2SOParentLibraryText(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentSiteCollectionNumber(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            NumberConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMNumberExtraInputConditionType.GreaterThanEquals => NumberConditionType.GreaterThanEquals,
                (int)RMNumberExtraInputConditionType.LessThanEquals => NumberConditionType.LessThanEquals,
                _ => NumberConditionType.None,
            };
            converter = new DO2SOParentSiteCollectionNumber(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private static IDO2SOFilterConverter GetConverterForParentLibraryNumber(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = criteriaInfo.ConditionInfo.Value;
            NumberConditionType condition = criteriaInfo.ConditionInfo.Logic switch
            {
                (int)RMNumberExtraInputConditionType.GreaterThanEquals => NumberConditionType.GreaterThanEquals,
                (int)RMNumberExtraInputConditionType.LessThanEquals => NumberConditionType.LessThanEquals,
                _ => NumberConditionType.None,
            };
            converter = new DO2SOParentLibraryNumber(values, sequenceNo, condition, criteriaInfo.ConditionInfo.ExtraValue);
            return converter;
        }

        private IDO2SOFilterConverter GetDocumentVersionConverter(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
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

        private IDO2SOFilterConverter GetConverterForDocumentName(RMDiscoveryRuleCriteriaInfo criteriaInfo, int sequenceNo)
        {
            IDO2SOFilterConverter converter = null;
            var values = JsonConvert.DeserializeObject<List<string>>(criteriaInfo.ConditionInfo.Value);
            if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextMatchIn)
            {
                converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchIn);
            }
            else if (criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn)
            {
                converter = new DO2SOFileNameFilter(values, sequenceNo, DO2SOFileNameFilter.FileNameCondition.TextMatchNotIn);
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

    public class FilterGroup
    {
        public int Order;
        public List<SOFilterPolicy> Filters;
        public string AndOrString;
        public RMDiscoveryCriteriaLogicType LogicType;

        public FilterGroup(List<SOFilterPolicy> filters, string andOrString,int order, RMDiscoveryCriteriaLogicType logicType)
        {
            Filters = filters;
            AndOrString = andOrString;
            Order = order;
            LogicType = logicType;
        }
    }
}
