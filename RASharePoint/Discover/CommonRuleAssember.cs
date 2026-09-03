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
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.SharePoint.Discover
{
    public class RuleAssembler
    {

        private List<TermRule> TermRules;
        private string ColumnName;
        public RuleAssembler()
        {
            TermRules = new List<TermRule>();
        }
        public RuleAssembler(string columnName)
        {
            this.ColumnName = columnName;
            TermRules = new List<TermRule>();
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
            foreach (TermRule rule in TermRules)
            {
                List<string> termNames = rule.Terms.Select(a => a.Name).ToList();
                int seq = termNames.Count + 1;
                int sequenceNo = 0;
                foreach (var termName in termNames)
                {
                    ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
                    arFilter.CombineMode = ArchiverFilterCombineMode.Or;
                    arFilter.Condition = ArchiverFilterCondition.Equals;
                    if (termNames[0].Equals(termName))
                    {
                        arFilter.CombineMode = ArchiverFilterCombineMode.And;
                    }
                    arFilter.RuleType = ArchiverFilterRuleType.TextColumn;
                    arFilter.RuleName = this.ColumnName;
                    arFilter.Value1 = termName;
                    arFilter.SequenceNo = ++sequenceNo;
                    foreach (Rule r in rule.Rules.Values)
                    {
                        foreach (SOFilterPolicy filter in r.SOFilters)
                        {
                            filter.SequenceNo += sequenceNo;
                        }
                        r.SOFilters.Insert(0, arFilter.Dto);
                        ResetAndOrExpression(r);
                    }
                }
                foreach (Rule r in rule.Rules.Values)
                {
                    ResetFilterForOrAction(r);
                    result.Add(++index, r);
                }
            }
            return result;
        }
        private void ResetFilterForOrAction(Rule rule)
        {
            SOFilterPolicy dto = rule.SOFilters[rule.SOFilters.Count - 1];
            if (!dto.IsAnd)
            {
                string AndOrExpression = "(";
                for (int i = 0; i < rule.SOFilters.Count; i++)
                {
                    SOFilterPolicy filterDto = rule.SOFilters[i];
                    filterDto.Level = rule.PolicyLevel;
                    rule.SOFilters[i].SequenceNo = i + 1;
                    if (filterDto.IsAnd)
                    {
                        AndOrExpression += string.Format("{0} {1} {2} {3}", filterDto.SequenceNo, ")", filterDto.IsAnd ? "And" : "Or", "(");
                    }
                    else if (i == rule.SOFilters.Count - 1)
                    {
                        AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                    }
                    else
                    {
                        AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                    }

                }
                AndOrExpression += ")";
                rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
            }
        }
        /// <summary>
        /// 重置Rule中的AndOrExpression
        /// </summary>
        /// <param name="rule"></param>
        private void ResetAndOrExpression(Rule rule)
        {
            string AndOrExpression = "(";
            for (int i = 0; i < rule.SOFilters.Count; i++)
            {
                SOFilterPolicy filterDto = rule.SOFilters[i];
                filterDto.Level = rule.PolicyLevel;
                rule.SOFilters[i].SequenceNo = i + 1;
                if (i == rule.SOFilters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filterDto.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filterDto.SequenceNo, filterDto.IsAnd ? "And" : "Or");
                }
            }
            AndOrExpression += ")";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
        }
        /// <summary>
        /// 增加一个Term和Rule Dic的关系组, 可以多次添加
        /// </summary>
        /// <param name="term"></param>
        /// <param name="ruleDic"></param>
        /// <returns></returns>
        public bool AddTermWithRule(RMTerm term, Dictionary<int, Rule> ruleDic)
        {
            return this.ValidateRules(term, ruleDic);
        }

        private bool ValidateRules(RMTerm term, Dictionary<int, Rule> ruleDic)
        {
            if (TermRules.Count == 0)
            {
                TermRule tr = new TermRule();
                tr.Terms = new List<RMTerm>() { term };
                tr.Rules = ruleDic;
                TermRules.Add(tr);
                return true;
            }
            foreach (TermRule termRule in TermRules)
            {
                //取要比较Dic的最小Count
                int count = ruleDic.Count > termRule.Rules.Count ? termRule.Rules.Count : ruleDic.Count;
                for (int i = 0; i <= count; i++)
                {
                    //如果按Order顺序相同有不一样的Rule, 返回False
                    int order = i + 1;
                    if (!termRule.Rules.ContainsKey(order) || !ruleDic.ContainsKey(order))
                    {
                        break;
                    }
                    if (termRule.Rules[order].Id != ruleDic[order].Id)
                    {
                        TermRule t = new TermRule();
                        t.Terms = new List<RMTerm>() { term };
                        t.Rules = CloneRuleDic(ruleDic); //此处需要Clone一份Rule
                        TermRules.Add(t);
                        return false;
                    }
                }
                //如果Order上对应的Rule都一样, 将Term加到List中
                termRule.Rules = ruleDic.Count < termRule.Rules.Count ? termRule.Rules : ruleDic;
                termRule.Terms.Add(term);
            }
            return true;
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
            result.Name += "1";
            return result;
        }

        public void Clear()
        {
            this.ColumnName = null;
            this.TermRules.Clear();
        }
    }

    class TermRule
    {
        public List<RMTerm> Terms = new List<RMTerm>();
        public Dictionary<int, Rule> Rules = new Dictionary<int, Rule>();

    }
}
