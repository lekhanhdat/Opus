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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.SharePoint.Archiver.CAMLHelper
{
    public class CamlUtil
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static RuleItemCollection GetRuleItemCollection(DateTime timePoint, List<Rule> rules)
        {
            RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
            int reOrder = 0;
            List<RuleItem> mRules = new List<RuleItem>();
            bool hasUnCamlQueryableCondition = false;
            foreach (var rule in rules)
            {
                if (rule.PolicyLevel != PolicyLevel.None && rule.SOFilters != null && rule.SOFilters.Count > 0)
                {
                    reOrder++;
                    var ruleOBj = CloneSameRuleObject(rule);
                    //var ruleAssember = new RuleAssembler();
                    //ruleOBj = ruleAssember.ConvertToSPRule(ruleOBj);
                    commonRules.Rules.Add(reOrder, ruleOBj);
                    if (ruleOBj.PolicyLevel == PolicyLevel.Item
                        || ruleOBj.PolicyLevel == PolicyLevel.Document
                        || ruleOBj.PolicyLevel == PolicyLevel.DocumentVersion
                        || ruleOBj.PolicyLevel == PolicyLevel.Attachment
                        || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                    {
                        mRules.Add(ConvertRuleChecker(ruleOBj, timePoint));
                    }
                    else
                    {
                        ModifyRuleChecker(ruleOBj, timePoint);
                    }
                }
            }

            if (mRules.Count > 0)
            {
                if (mRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                {
                    hasUnCamlQueryableCondition = true;
                }
            }

            RuleItemCollection tempRC = new RuleItemCollection();
            tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
            tempRC.CommonRules = commonRules;
            tempRC.Rules = mRules;
            return tempRC;
        }

        public static RuleItemCollection GetRuleItemCollectionForVersionRule(DateTime timePoint, List<Rule> rules)
        {
            RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
            int reOrder = 0;
            List<RuleItem> mRules = new List<RuleItem>();
            bool hasUnCamlQueryableCondition = false;
            foreach (var rule in rules)
            {
                if (rule.PolicyLevel != PolicyLevel.None && rule.SOFilters != null && rule.SOFilters.Count > 0)
                {
                    reOrder++;
                    var ruleOBj = CloneSameRuleObject(rule);
                    //var ruleAssember = new RuleAssembler();
                    //ruleOBj = ruleAssember.ConvertToSPRule(ruleOBj);
                    commonRules.Rules.Add(reOrder, ruleOBj);
                    if (ruleOBj.PolicyLevel == PolicyLevel.DocumentVersion)
                    {
                        var ruleItem = ConvertRuleCheckerForVersion(ruleOBj, timePoint);
                        if (ruleItem.HasUnCamlQueryableCondition)
                        {
                            hasUnCamlQueryableCondition = true;
                        }
                        else
                        {
                            mRules.Add(ruleItem);
                        }
                    }
                    else
                    {
                        ModifyRuleChecker(ruleOBj, timePoint);
                    }
                }
            }

            RuleItemCollection tempRC = new RuleItemCollection();
            tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
            tempRC.CommonRules = commonRules;
            tempRC.Rules = mRules;
            return tempRC;
        }

        public static Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }
        public static void ModifyRuleChecker(Rule rule, DateTime timePoint)
        {
            foreach (var filter in rule.Filters)
            {
                filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule)
                {
                    switch (filter.Condition)
                    {
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                {
                                    tempDt = timePoint.AddDays(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                {
                                    tempDt = timePoint.AddDays(-num * 7);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                {
                                    tempDt = timePoint.AddMonths(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                {
                                    tempDt = timePoint.AddYears(-num);
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            //rule.Filters.Add(new FilterPolicy()
            //{
            //    Condition = PolicyCondition.Before,
            //    Level = rule.PolicyLevel,
            //    Rule = new CreatedRule() { Value1 = "Created Time" },
            //    RuleType = PolicyRuleType.CreatedTime,
            //    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
            //    SequenceNo = 1
            //});
            //have a bug here should change order Created Time to last
            //var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            //string andOrExpression = "(1 And";
            //foreach (var str in tempStrs)
            //{
            //    int sequenceNo = 0;
            //    if (int.TryParse(str, out sequenceNo))
            //    {
            //        sequenceNo++;
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
            //    }
            //    else
            //    {
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
            //    }
            //}
            //andOrExpression += ")";
            //rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            //{
            //    { rule.PolicyLevel, andOrExpression }
            //};
        }
        public static RuleItem ConvertRuleChecker(Rule rule, DateTime timePoint)
        {
            RuleItem checker = new RuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();

            if (rule.MoveToRecordCenterAndDelareSetting != null)// rule.spMoveOption != null ||  TO DO(fpwang)
            {
                checker.HasUnCamlQueryableCondition = true;
            }
            foreach (var filter in rule.SOFilters)
            {
                if (filter.Condition == PolicyCondition.DoesNotContains && rule.AndOrExpression != null && rule.AndOrExpression.Count > 0 && !rule.AndOrExpression.FirstOrDefault().Value.Contains("Or"))
                {
                    //如果是DoesNotContains条件，且Rule And Or关系没有Or，则去掉检查DoesNotContains条件，虽然增大了查询范围，但是可以使用SP Query.
                    //举例说明：客户case是Last access time / Modified time older than 2 years
                    //& document size > 1MB
                    //& Name does not contains.JS
                    //& Name does not contains.HTML
                    //& Name does not contains.SPFX
                    //去掉DoesNotContains条件后，可以使用SPQuery，之后文件具体check rule去处理DoesNotContains条件数据.虽然多查询出来一些数据，但是整体效率会提升.
                    if (rule.SOFilters.All(a => a.Condition == PolicyCondition.DoesNotContains))
                    {
                        mLog.Info($"rule.SOFilters all PolicyCondition.DoesNotContains,will not use sp query.");
                    }
                    else
                    {
                        mLog.Info($"HasUnCamlQueryableCondition,skip DoesNotContains condtion. rule name:{rule.Name}.AndOrExpression:{rule.AndOrExpression.FirstOrDefault().Value}.");
                        continue;
                    }
                }
                var arFilter = new ArchiverRuleFilter(filter);
                checker.RuleFilters.Add(arFilter);
                //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                if (!checker.HasUnCamlQueryableCondition)
                {
                    if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch || arFilter.Condition == ArchiverFilterCondition.DoesNotContain)
                    {
                        mLog.Info("HasUnCamlQueryableCondition, condtion:{0} rule name:{1}", arFilter.Condition.ToString(), rule.Name);
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                    {
                        mLog.Info("HasUnCamlQueryableCondition, rule type:{0} condition:{1} rule name:{2}", arFilter.RuleType.ToString(), arFilter.Condition.ToString(), rule.Name);
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy 
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName 
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryName || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryNumber
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryText || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryBoolean
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionDateTime || arFilter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionNumber
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionText || arFilter.RuleType == ArchiverFilterRuleType.ParentSiteCollectionBoolean
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryDateTime || arFilter.RuleType == ArchiverFilterRuleType.PropertyBagBoolean
                        || arFilter.RuleType == ArchiverFilterRuleType.PropertyBagDateTime || arFilter.RuleType == ArchiverFilterRuleType.PropertyBagNumber
                        || arFilter.RuleType == ArchiverFilterRuleType.PropertyBagText
                        //Metadata Column Calm Query暂不支持
                        || arFilter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || arFilter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
                    {
                        mLog.Info("HasUnCamlQueryableCondition, rule type:{0} rule name:{1}", arFilter.RuleType.ToString(), rule.Name);
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime && arFilter.Condition == ArchiverFilterCondition.FromTo)
                    {
                        mLog.Info("HasUnCamlQueryableCondition, rule type:{0} rule name:{1}", arFilter.RuleType.ToString(), rule.Name);
                        checker.HasUnCamlQueryableCondition = true;
                    }
                }
            }

            foreach (var filter in rule.Filters)
            {
                //filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is ContentTypeRule)
                {
                    filter.RuleType = PolicyRuleType.ContentType;
                }
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                {
                    switch (filter.Condition)
                    {
                        #region old
                        // [REC-738] remove timepoint ref FromTo/Before
                        //case PolicyCondition.FromTo:
                        //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                        //    if (toDt > timePoint)
                        //    {
                        //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        //case PolicyCondition.Before:
                        //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    if (ltDt >= timePoint)
                        //    {
                        //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        #endregion
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                try
                                {
                                    if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                    {
                                        tempDt = timePoint.AddDays(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                    {
                                        tempDt = timePoint.AddDays(-num * 7);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                    {
                                        tempDt = timePoint.AddMonths(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                    {
                                        tempDt = timePoint.AddYears(-num);
                                    }
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    mLog.Warn($"The filter policy no.{filter.SequenceNo} of rule [{rule.Id}], name: [{rule.Name}] has time value less than min datetime. Force using min datetime");
                                    tempDt = DateTime.MinValue.AddDays(1); // avoid exception from converting to negative time zone
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            //AddCreateTimeRuleChecker(rule, timePoint);
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url))
            {
                AddMoveToFilter(rule);
            }
            else if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                AddMoveToFilter(rule);
            }
            return checker;
        }

        public static RuleItem ConvertRuleCheckerForVersion(Rule rule, DateTime timePoint)
        {
            RuleItem checker = new RuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();

            List<int> documentSizeRuleSequenceNo = new List<int>();
            List<int> canCamlRuleSequenceNos = new List<int>();
            List<char> andorSequence = new List<char>();

            //(1 Or 2 And 3 And 4 And 5)
            //|&&&
            char[] tempArray = rule.AndOrExpression[PolicyLevel.DocumentVersion].ToLower().Replace("and", "&").Replace("or", "|").ToCharArray();

            for (int i = 0; i < tempArray.Length; i++)
            {
                if (tempArray[i] == '&' || tempArray[i] == '|')
                {
                    andorSequence.Add(tempArray[i]);
                }
            }
            OutLogForIEnumerable(andorSequence);
            foreach (var filter in rule.Filters)
            {
                //getting parent document size rule sequenceNo
                if (filter.Rule is SizeRule)
                {
                    documentSizeRuleSequenceNo.Add(filter.SequenceNo);
                }
            }
            OutLogForIEnumerable(documentSizeRuleSequenceNo);

            if (documentSizeRuleSequenceNo.Count > 0)
            {
                for (int i = 0; i < documentSizeRuleSequenceNo.Count; i++)
                {
                    bool canUseDocumentSizeFilter = true;
                    var seq = documentSizeRuleSequenceNo[i];
                    var index = 0;
                    if (seq >= 2)
                    {
                        index = seq - 2;
                    }

                    for (int j = index; j < andorSequence.Count; j++)
                    {
                        // name|name&size&name can work
                        // size|size&name not work
                        if (andorSequence[j] == '|')
                        {
                            canUseDocumentSizeFilter = false;
                            break;
                        }
                    }

                    if (canUseDocumentSizeFilter) {
                        // add this rule to the document size check list
                        canCamlRuleSequenceNos.Add(seq);
                    }
                }
            }
            if (canCamlRuleSequenceNos.Count() == 0)
            {
                checker.HasUnCamlQueryableCondition = true;
            }

            foreach (var filter in rule.SOFilters)
            {
                if (filter.Rule is SizeRule && canCamlRuleSequenceNos.Contains(filter.SequenceNo))
                {
                    var arFilter = new ArchiverRuleFilter(filter);
                    checker.RuleFilters.Add(arFilter);
                }
                //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                //if(filter.RuleType == ArchiverFilterRuleType.CreatedBy)
            }

            foreach (var filter in rule.Filters)
            {
                //filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is ContentTypeRule)
                {
                    filter.RuleType = PolicyRuleType.ContentType;
                }
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                {
                    switch (filter.Condition)
                    {
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                {
                                    tempDt = timePoint.AddDays(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                {
                                    tempDt = timePoint.AddDays(-num * 7);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                {
                                    tempDt = timePoint.AddMonths(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                {
                                    tempDt = timePoint.AddYears(-num);
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return checker;
        }

        public static void AddCreateTimeRuleChecker(Rule ruleObj, DateTime timePoint)
        {
            var arFilter = new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = ruleObj.PolicyLevel,
                Rule = new CreatedRule() { Value1 = "Created Time" },
                RuleType = PolicyRuleType.CreatedTime,
                Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
            };
            int ruleCount = ruleObj.Filters.Count;
            arFilter.SequenceNo = ruleCount + 1;
            //SOFilterPolicy filterDto = CloneSameFilterObject(arFilter.Dto);
            //ruleObj.SOFilters.Add(filterDto);
            ruleObj.Filters.Add(arFilter);
            AddAndOrExpressionForMove(ruleObj, arFilter);
            //have a bug here should change order Created Time to last
            #region old logic
            //var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            //string andOrExpression = "(1 And";
            //foreach (var str in tempStrs)
            //{
            //    int sequenceNo = 0;
            //    if (int.TryParse(str, out sequenceNo))
            //    {
            //        sequenceNo++;
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
            //    }
            //    else
            //    {
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
            //    }
            //}
            //andOrExpression += ")";
            //rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            //{
            //    { rule.PolicyLevel, andOrExpression }
            //};
            #endregion
        }
        public static void AddMoveToFilter(Rule ruleObj)
        {
            var arFilter = new FilterPolicy();
            //arFilter.CombineMode = ArchiverFilterCombineMode.And;
            arFilter.Condition = PolicyCondition.DoesNotContains;
            arFilter.RuleType = PolicyRuleType.Url;
            var Rule = new UrlRule() { Value1 = "URL" };
            arFilter.Rule = Rule;
            if (ruleObj.MoveToRecordCenterAndDelareSetting != null)
            {
                arFilter.Value = new PolicyValue(ruleObj.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url);
            }
            else
            {
                // TO DO(fpwang)
                if (ruleObj.spMoveOption != null && ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    arFilter.Value = new PolicyValue(ruleObj.spMoveOption.MoveDestination.SPUrl);
                }
                else if (ruleObj.spMoveOption != null && ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.TreeMode && ruleObj.spMoveOption.MoveDestination.SPTreeNode != null)
                {
                    arFilter.Value = new PolicyValue(ruleObj.spMoveOption.MoveDestination.SPTreeNode.FullPath);
                }
                else if (ruleObj.spMoveOption != null && ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.TreeMode && !string.IsNullOrWhiteSpace(ruleObj.spMoveOption.MoveDestination.SPUrl))
                {
                    arFilter.Value = new PolicyValue(ruleObj.spMoveOption.MoveDestination.SPUrl);
                }
                else
                {
                    mLog.Info("Return no need to add filter");
                    return;
                }
            }
            if (arFilter.Value.Value1.Contains("#/"))//add logic to trim url for Onpremise 2013,2016
            {
                var siteUrl = arFilter.Value.Value1.Substring(0, arFilter.Value.Value1.IndexOf("_layouts", StringComparison.OrdinalIgnoreCase));
                var listUrl = arFilter.Value.Value1.Substring(arFilter.Value.Value1.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2).Split('/')[0];
                arFilter.Value.Value1 = siteUrl + listUrl;
            }
            else if (arFilter.Value.Value1.Contains("/Forms/")) //Add logic to trim URL for Office365
            {
                var libUrl = arFilter.Value.Value1.Substring(0, arFilter.Value.Value1.IndexOf("/Forms/"));
                arFilter.Value.Value1 = libUrl;
            }
            if (arFilter.Value.Value1.Contains("%"))
            {
                arFilter.Value.Value1 = HttpUtility.UrlDecode(arFilter.Value.Value1);
            }
            arFilter.Level = ruleObj.PolicyLevel;
            int ruleCount = ruleObj.Filters.Count;
            arFilter.SequenceNo = ruleCount + 1;
            //SOFilterPolicy filterDto = CloneSameFilterObject(arFilter.Dto);
            //ruleObj.SOFilters.Add(filterDto);
            ruleObj.Filters.Add(arFilter);
            AddAndOrExpressionForMove(ruleObj, arFilter);
        }
        private static void AddAndOrExpressionForMove(Rule rule, FilterPolicy filterDto)
        {
            string AndOrExpression = rule.AndOrExpression[rule.PolicyLevel];
            //AndOrExpression += string.Format(" {0} {1}", "Or", filterDto.SequenceNo);
            AndOrExpression = AndOrExpression.Insert(AndOrExpression.Length - 1, string.Format(" {0} {1}", "And", filterDto.SequenceNo));
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
        }

        private static void OutLogForIEnumerable(IEnumerable value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in value) {
                builder.Append(item.ToString());
                builder.Append(",");
            }
            mLog.Info(builder.ToString());
        }

    }
}
