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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Web;

namespace AvePoint.RA.Service.Services.RuleManagement
{
    [RACodeReview("Allen Yin", comment: "此类非常重要，考虑输出拼rule的结果")]
    public class EXORuleAssembler : IDisposable
    {
        public ITermDao TermDao { get; set; }
        private RALogger logger = RALogger.GetInstance(typeof(EXORuleAssembler));
        private Dictionary<int, List<TermRule>> TermRulesWithLevel;
        // private List<TermRule> TermRules;
        private string ColumnName;
        public EXORuleAssembler()
        {
            //TermRules = new List<TermRule>();
            TermRulesWithLevel = new Dictionary<int, List<TermRule>>();
            TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
        }
        public EXORuleAssembler(string columnName)
        {
            this.ColumnName = columnName;
            //TermRules = new List<TermRule>();
            TermRulesWithLevel = new Dictionary<int, List<TermRule>>();
            TermDao = PlatformWindsorManager.GetService(typeof(ITermDao)) as ITermDao;
        }
        public void SetColumnName(string columnName)
        {
            this.ColumnName = columnName;
        }

        /// <summary>
        /// 获取最后要的结果, 带Order的Rule
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Rule> GetRuleDicResult()
        {
            Dictionary<int, Rule> result = new Dictionary<int, Rule>();
            int index = 0;
            Dictionary<int, List<TermRule>> dic = TermRulesWithLevel.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            foreach (var TermRules in dic.Values)
            {
                foreach (TermRule rule in TermRules)
                {
                    Dictionary<string, int> ruleSequence = new Dictionary<string, int>();
                    foreach (var term in rule.Terms)
                    {
                        //bool moveToRule = false;
                        ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                        foreach (KeyValuePair<int, Rule> r in rule.Rules)
                        {
                            //当前没有move
                            //if (r.Value.MoveToRecordCenterAndDelareSetting != null || (r.Value.spMoveOption != null && r.Value.spMoveOption.MoveDestination != null))
                            //{
                            //    moveToRule = true;
                            //}

                            int sequence = 1;
                            if (r.Key <= rule.TermMaxOrder[term.Id])
                            {
                                bool isFirstFilter = true;
                                if (ruleSequence.ContainsKey(r.Value.Id))
                                {
                                    sequence = ruleSequence[r.Value.Id] + 1;
                                    isFirstFilter = false;
                                    ruleSequence[r.Value.Id] = sequence;
                                }
                                else
                                {
                                    //if (moveToRule)
                                    //{
                                    //    isFirstFilter = true;
                                    //    sequence = sequence + 1;
                                    //    moveToRule = false;
                                    //}
                                    ruleSequence.Add(r.Value.Id, sequence);
                                }
                                #region add filter
                                var cateFilterGroup = r.Value.SOFilters.GroupBy(f => f.Level).ToDictionary(o => o.Key, k => k.ToList());

                                foreach (var group in cateFilterGroup)
                                {
                                    var filterLevel = group.Key;
                                    var termFilter = GetFilterPolicy(term.UniqueId.ToString(), r.Value.PolicyLevel, filterLevel);
                                    termFilter.SequenceNo = group.Value.Count + 1;
                                    SOFilterPolicy filterDto = CloneSameFilterObject(termFilter.Dto);
                                    r.Value.SOFilters.Add(filterDto);

                                    AddAndOrExpression(r.Value, filterDto, filterLevel, isFirstFilter);

                                    #endregion

                                }
                            }
                        }
                        
                    }
                    foreach (Rule r in rule.Rules.Values)
                    {
                        //ResetFilterForOrAction(r);
                        result.Add(++index, r);
                        this.LogRule(index, r);
                    }
                }

                
            }
            return result;
        }

            private ArchiverRuleFilter GetFilterPolicy(string termId, PolicyLevel ruleLevel, PolicyLevel filterLevel)
            {
                ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                if (ruleLevel.Equals(PolicyLevel.ExchangeOnlineItem))
                {
                    arFilter.CombineMode = ArchiverFilterCombineMode.Or;
                    arFilter.Condition = ArchiverFilterCondition.Equals;
                    arFilter.RuleType = ArchiverFilterRuleType.Term;
                    arFilter.RuleName = this.ColumnName;
                    arFilter.Value1 = termId;
                    arFilter.Level = filterLevel;

                }
                return arFilter;
            }

            /// <summary>
            /// add destination checking in Last cretia.
            /// </summary>
            /// <param name="ruleObj"></param>
            public void AddMoveToFilter(Rule ruleObj)
            {
                var arFilter = new ArchiverRuleFilter();
                arFilter.CombineMode = ArchiverFilterCombineMode.And;
                arFilter.Condition = ArchiverFilterCondition.DoesNotContain;
                arFilter.RuleType = ArchiverFilterRuleType.URL;
                arFilter.RuleName = "UrlRule";

                if (ruleObj.MoveToRecordCenterAndDelareSetting != null && ruleObj.MoveToRecordCenterAndDelareSetting.DestinationLocation != null && !string.IsNullOrEmpty(ruleObj.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url))
                {
                    logger.Debug("use old move to setting");
                    arFilter.Value1 = ruleObj.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url;
                }
                else
                {
                    //if (ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                    //{


                    //RECO-2474 move rule --> remove/keep rule [ruleObj.spMoveOption != null] but [ruleObj.spMoveOption.MoveDestination == null]
                    arFilter.Value1 = ruleObj.spMoveOption.MoveDestination.SPUrl;
                    //}
                    //else
                    //{
                    //    arFilter.Value1 = ruleObj.spMoveOption.MoveDestination.SPTreeNode.FullPath;
                    //}
                }
                if (string.IsNullOrEmpty(arFilter.Value1))
                {
                    logger.Warn("move rule filter value is null");
                    return;
                }
                if (arFilter.Value1.Contains("#/"))//add logic to trim url for Onpremise 2013,2016
                {
                    var siteUrl = arFilter.Value1.Substring(0, arFilter.Value1.IndexOf("_layouts", StringComparison.OrdinalIgnoreCase));
                    var listUrl = arFilter.Value1.Substring(arFilter.Value1.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2).Split('/')[0];
                    arFilter.Value1 = siteUrl + listUrl;
                }
                else if (arFilter.Value1.Contains("/Forms/")) //Add logic to trim URL for Office365
                {
                    var libUrl = arFilter.Value1.Substring(0, arFilter.Value1.IndexOf("/Forms/"));
                    arFilter.Value1 = libUrl;
                }
                if (arFilter.Value1.Contains("%"))
                {
                    arFilter.Value1 = HttpUtility.UrlDecode(arFilter.Value1);
                }
                arFilter.Level = ruleObj.PolicyLevel;
                int ruleCount = ruleObj.Filters.Count;
                arFilter.SequenceNo = ruleCount + 1;
                SOFilterPolicy filterDto = CloneSameFilterObject(arFilter.Dto);
                ruleObj.SOFilters.Add(filterDto);
                //ruleObj.Filters.Add(filterDto);
                AddAndOrExpressionForMove(ruleObj, filterDto, false);
            }
            private void AddAndOrExpressionForMove(Rule rule, SOFilterPolicy filterDto, bool isFirst)
            {
                string AndOrExpression = rule.AndOrExpression[rule.PolicyLevel];
                //AndOrExpression += string.Format(" {0} {1}", "Or", filterDto.SequenceNo);
                AndOrExpression = AndOrExpression.Insert(AndOrExpression.Length - 1, string.Format(" {0} {1}", "And", filterDto.SequenceNo));
                rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
            }
      
        /// <summary>
        /// 将Term当作Filter拼接在最后一个条件中。 与前面Rule 本身Filter 为 And 关系。 
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="filterDto"></param>
        /// <param name="isFirst"></param>
        /// <param name="isLast"></param>
        private void AddAndOrExpression(Rule rule, SOFilterPolicy filterDto, PolicyLevel filterLevel, bool isFirst)
        {

            string AndOrExpression = rule.AndOrExpression[filterLevel];
            if (isFirst)
            {
                AndOrExpression += string.Format("{0}{1}{2}", " And (", filterDto.SequenceNo, ")");
            }
            else
            {
                AndOrExpression = AndOrExpression.Insert(AndOrExpression.Length - 1, string.Format(" {0} {1}", "Or", filterDto.SequenceNo));
            }
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { filterLevel, AndOrExpression }
            };
        }

        /*private void AddAndOrCategoryExpression(Rule rule, PolicyLevel level, bool isFirst, int categoryCount)
        {
            //if (!rule.AndOrExpression.ContainsKey(level))
            //{
            //    logger.Info("current level no fileterid:{0},{1}", rule.Id, level);
            //    return;
            //}
            string result = rule.AndOrExpression[level];
            string oldAndOrExpression = string.Empty;
            if (rule.AndOrExpression.ContainsKey(rule.PolicyLevel))
            {
                oldAndOrExpression = rule.AndOrExpression[rule.PolicyLevel];
            }
            if (isFirst)
            {
                //only one category no need ()
                result = categoryCount == 1 ? result : string.Format("({0})", result);
            }
            else
            {
                result = oldAndOrExpression.Insert(oldAndOrExpression.Length - 1, string.Format("{1} {0}", "Or", result));
            }
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, result }
            };
        }*/
        /// <summary>
        /// 增加一个Term和Rule Dic的关系组, 可以多次添加
        /// </summary>
        /// <param name="term"></param>
        /// <param name="ruleDic"></param>
        /// <returns></returns>
        public bool AddTermWithRule(RMTerm term, Dictionary<int, Rule> ruleDic, int level)
        {
            return this.ValidateRules(term, ruleDic, level);
        }



        private bool ValidateRules(RMTerm term, Dictionary<int, Rule> ruleDic, int level)
        {
            var TermRules = new List<TermRule>();
            if (TermRulesWithLevel.ContainsKey(level))
            {
                TermRules = TermRulesWithLevel[level];
            }
            else
            {
                TermRulesWithLevel.Add(level, TermRules);
            }
            if (TermRules.Count == 0)
            {
                TermRule tr = new TermRule();
                tr.Terms = new List<RMTerm>() { term };
                tr.TermMaxOrder.Add(term.Id, ruleDic.Keys.Max());
                tr.Rules = CloneRuleDic(ruleDic);
                TermRules.Add(tr);
                return true;
            }
            foreach (TermRule termRule in TermRules)
            {
                //取要比较Dic的最小Count
                int count = ruleDic.Count > termRule.Rules.Count ? termRule.Rules.Count : ruleDic.Count;
                bool sameOrder = true;
                for (int i = 0; i < count; i++)
                {
                    //如果按Order顺序相同有不一样的Rule, 返回False
                    int order = i + 1;
                    if (!termRule.Rules.ContainsKey(order) || !ruleDic.ContainsKey(order))
                    {
                        sameOrder = false;
                        break;
                    }
                    if (termRule.Rules[order].Id != ruleDic[order].Id)
                    {
                        sameOrder = false;
                        break;
                    }
                }
                if (sameOrder)
                {
                    //如果Order上对应的Rule都一样, 将Term加到List中
                    termRule.Rules = ruleDic.Count < termRule.Rules.Count ? termRule.Rules : CloneRuleDic(ruleDic);
                    termRule.Terms.Add(term);
                    termRule.TermMaxOrder.Add(term.Id, ruleDic.Keys.Max());
                    return true;
                }
            }
            TermRule t = new TermRule();
            t.Terms = new List<RMTerm>() { term };
            t.TermMaxOrder.Add(term.Id, ruleDic.Keys.Max());
            t.Rules = CloneRuleDic(ruleDic); //此处需要Clone一份Rule
            TermRules.Add(t);
            return false;
        }

        private Dictionary<int, Rule> CloneRuleDic(Dictionary<int, Rule> ruleDic)
        {
            Dictionary<int, Rule> result = new Dictionary<int, Rule>();
            foreach (KeyValuePair<int, Rule> rule in ruleDic)
            {
                result.Add(rule.Key, CloneRule(rule.Value));
            }
            return result;
        }
        private Rule CloneRule(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public SOFilterPolicy CloneSameFilterObject(SOFilterPolicy filter)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(filter);
            SOFilterPolicy result = SerializerHelper.DeserializeByDataContractSerializer<SOFilterPolicy>(xml);
            return result;
        }
        public void Clear()
        {
            this.ColumnName = null;
            this.TermRulesWithLevel.Clear();
            //this.TermRules.Clear();
        }

        private void LogRule(int order, Rule rule)
        {
            if (logger.IsDebugEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\r\n");
                sb.Append("Rule Order: ").Append(order).Append(";  Name:").Append(rule.Name).Append(";  Level:").Append(rule.PolicyLevel).Append("\r\n");
                sb.Append("Criteria:").Append("\r\n");
                sb.Append(GetRuleCriteriaCondition(rule)).Append("\r\n");

                string criteria = "Criteria Filters: ";
                criteria += string.Join(string.Empty, rule.AndOrExpression.Values.ToArray());
                sb.Append(criteria);
                logger.Debug(sb.ToString());
            }
        }

        private string GetRuleCriteriaCondition(Rule rule)
        {
            string result = string.Empty;
            StringBuilder criteriaCondition = new StringBuilder();
            criteriaCondition.Append("1.");
            for (int i = 0; i < rule.SOFilters.Count; i++)
            {
                string end = null;
                if (i == (rule.SOFilters.Count - 1))
                {
                    end = " . ";
                }
                else
                {
                    end = "; \n" + (i + 2) + ".";
                }
                SOFilterPolicy filterPolicy = rule.SOFilters[i];
                //police level
                criteriaCondition.Append(Enum.GetName(typeof(PolicyLevel), filterPolicy.Level));
                criteriaCondition.Append(",");
                //policy name
                if (null != filterPolicy.Rule)
                {
                    if ((filterPolicy.Rule is CreatedByRule) && (filterPolicy.Level == PolicyLevel.SiteCollection))
                    {
                        criteriaCondition.Append("Primary Administrator");
                    }
                    else
                    {
                        if (filterPolicy.Rule is ColumnTextRule)
                        {
                            criteriaCondition.Append("Column(Text):");
                        }
                        else if (filterPolicy.Rule is ColumnNumberRule)
                        {
                            criteriaCondition.Append("Column(Number):");
                        }
                        else if (filterPolicy.Rule is ColumnBooleanRule)
                        {
                            criteriaCondition.Append("Column(Yes/No):");
                        }
                        else if (filterPolicy.Rule is ColumnDateTimeRule)
                        {
                            criteriaCondition.Append("Column(Date and Time):");
                        }
                        else if (filterPolicy.Rule is CustomPropertyTextRule)
                        {
                            criteriaCondition.Append("Custom Property(Text):");
                        }
                        else if (filterPolicy.Rule is CustomPropertyNumberRule)
                        {
                            criteriaCondition.Append("Custom Property(Number)");
                        }
                        else if (filterPolicy.Rule is CustomPropertyBooleanRule)
                        {
                            criteriaCondition.Append("Custom Property(Yes/No)");
                        }
                        else if (filterPolicy.Rule is CustomPropertyDateTimeRule)
                        {
                            criteriaCondition.Append("Custom Property(Date and Time)");
                        }
                        criteriaCondition.Append(filterPolicy.Rule.Value1);
                    }
                }
                //if (null != filterPolicy.Rule)
                //{
                //    criteriaCondition.Append(filterPolicy.Rule.Value1);
                //}                
                criteriaCondition.Append(",");
                if (filterPolicy.Condition == PolicyCondition.FromTo)
                {
                    criteriaCondition.Append("From");
                    criteriaCondition.Append(" ");
                    criteriaCondition.Append(filterPolicy.BeginTime.StartTime);
                    if (filterPolicy.Value.Value1Unit != PolicyValueUnit.None)
                    {
                        criteriaCondition.Append(filterPolicy.Value.Value1Unit);
                    }
                    criteriaCondition.Append("To");
                    criteriaCondition.Append(" ");
                    criteriaCondition.Append(filterPolicy.EndTime.StartTime);
                    if (filterPolicy.Value.Value2Unit != PolicyValueUnit.None)
                    {
                        criteriaCondition.Append(filterPolicy.Value.Value1Unit);
                    }

                }
                else
                {
                    //policy condition
                    criteriaCondition.Append(Enum.GetName(typeof(PolicyCondition), filterPolicy.Condition));
                    criteriaCondition.Append(",");
                    //policy value
                    if (null != filterPolicy.BeginTime && null != filterPolicy.BeginTime.StartTime)
                    {
                        criteriaCondition.Append(filterPolicy.BeginTime.StartTime);
                    }
                    else
                    {
                        criteriaCondition.Append(filterPolicy.Value.Value1);
                    }
                    if (filterPolicy.Value.Value1Unit != PolicyValueUnit.None)
                    {
                        criteriaCondition.Append(filterPolicy.Value.Value1Unit);
                    }
                }
                criteriaCondition.Append(end);
            }
            result = criteriaCondition.ToString();
            return result;
        }

        public void Dispose()
        {
            try
            {
                //this.AllTerms.Clear();
                this.TermRulesWithLevel.Clear();
            }
            catch (Exception ex)
            {
                logger.Warn("Dispose object error {0}", ex.ToString());
            }
            //throw new NotImplementedException();
        }
    }

    [RACodeReview("Allen Yin")]
    class TermRule
    {
        public List<RMTerm> Terms = new List<RMTerm>();
        /// <summary>
        /// key: TermId, value:Term对应RuleList的最大Order
        /// </summary>
        public Dictionary<int, int> TermMaxOrder = new Dictionary<int, int>();
        /// <summary>
        /// key:Order, value:Rule
        /// </summary>
        public Dictionary<int, Rule> Rules = new Dictionary<int, Rule>();

    }

}
