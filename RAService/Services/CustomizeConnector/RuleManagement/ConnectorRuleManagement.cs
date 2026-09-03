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
using AvePoint.Common.FilterEngine;
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.Common.FilterEngine.ObjectInfos.Connector;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.AzureFileShare.RuleManagement
{
    public class ConnectorRuleManagement
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(ConnectorRuleManagement));
        private readonly List<Rule> _rules;

        public ConnectorRuleManagement(List<Rule> rules)
        {
            _rules = rules;
        }
        public Tuple<Rule, TimeSpan> MatchPotentialRule(ObjectInfoBase obj, bool checkActionDueDate = false)
        {
            if (_rules == null)
            {
                return null;
            }

            var rule = CheckCriteria(obj);
            if (rule != null)
            {
                //directly matched
                return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
            }
            else if (checkActionDueDate)
            {
                //取出来所有带Older Than条件的Rule(包括Criteria有其它条件的Rule)
                
                var potentialRules = _rules.Where(t => t.ConnectorRule.Filters != null && t.ConnectorRule.Filters.Any(f => f.Condition == AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.OlderThan)).ToList();

                foreach (var pr in potentialRules)
                {
                    try
                    {
                        var isAndExpression = pr.ConnectorRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
                        if (isAndExpression)
                        {
                            var connectorObjInfo = obj as CustomizeConnectorItemInfo;
                            var connectorObj = JsonConvert.DeserializeObject<CustomizeConnectorItemInfo>(JsonConvert.SerializeObject(obj));
                            connectorObj.Modified = DateTime.MinValue;
                            connectorObj.Created = DateTime.MinValue;
                            var customDateTimeFilters = pr.ConnectorRule.Filters.Where(item => item.Rule is ColumnDateTimeRule);
                            foreach (var customDateTimeFilter in customDateTimeFilters)
                            {
                                if (connectorObj.ColumnInfos.ContainsKey(customDateTimeFilter.Rule.Value1) && !string.IsNullOrWhiteSpace(connectorObj.ColumnInfos[customDateTimeFilter.Rule.Value1]?.ToString()))
                                {
                                    connectorObj.ColumnInfos[customDateTimeFilter.Rule.Value1] = DateTime.MinValue;
                                }
                            }
                            var isMacth = CheckCriteria(connectorObj) != null;
                            if (!isMacth)
                            {
                                continue;
                            }
                        }
                        var olderThanPolicies = pr.ConnectorRule.Filters.Where(item => item.Condition == PolicyCondition.OlderThan).ToList();
                        var timespans = ComputeOlderThanTimeSpans(obj, olderThanPolicies);
                        if(timespans.Any())
                        {
                            timespans.Sort();
                            if(isAndExpression)
                            {
                                return new Tuple<Rule, TimeSpan>(pr, timespans.Last());
                            }
                            
                            return new Tuple<Rule, TimeSpan>(pr, timespans.First());
                        }
                        //olderThanPolicies[0].
                        //Dictionary<string, TimeSpan> offsets = ComputeCheckRuleOffsets(obj, pr);
                        //var tObj = ObjectConverter.CloneFilterObject(obj, offsets);
                        //var engine = new FilterEngine(pr.ConnectorRule.Filters, pr.ConnectorRule.AndOrExpression, true);
                        //if (engine.IsQualified(tObj))
                        //{
                        //    return new Tuple<Rule, TimeSpan>(pr, ComputeActionDueDateOffsets(obj, pr));
                        //}
                    }
                    catch (Exception ex)
                    {
                        if (ex is PropertyNotAssignedException)
                        {
                            logger.Error("A property was not assigned while checking a connector rule. Exception:{0}", ex.ToString());
                        }
                        //logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.Compression, ex.ToString());
                        throw new Exception("Checked expression failed", ex);
                    }
                }
                return null;
            }
            return null;
        }

        private List<TimeSpan> ComputeOlderThanTimeSpans(ObjectInfoBase objInfo, List<FilterPolicy> policies)
        {
            var timespans = new List<TimeSpan>();
            var info = objInfo as CustomizeConnectorItemInfo;
            var now = DateTime.UtcNow;

            foreach(var policy in policies)
            {
                if(policy.Rule is CreatedRule)
                {
                    var span = ComputeOlderThanDateTime(info.Created, policy.Value) - now;
                    timespans.Add(span);
                }
                else if(policy.Rule is ModifiedRule)
                {
                    var span = ComputeOlderThanDateTime(info.Modified, policy.Value) - now;
                    timespans.Add(span);
                }
                else if(policy.Rule is ColumnDateTimeRule)
                {
                    if (info.ColumnInfos.ContainsKey(policy.Rule.Value1) && !string.IsNullOrWhiteSpace(info.ColumnInfos[policy.Rule.Value1]?.ToString()))
                    {
                        var columnValue = DateTime.Parse(info.ColumnInfos[policy.Rule.Value1].ToString());
                        var span = ComputeOlderThanDateTime(columnValue, policy.Value) - now;
                        timespans.Add(span);
                    }
                }
            }

            return timespans;
        }

        private DateTime ComputeOlderThanDateTime(DateTime time, PolicyValue policyValue)
        {
            var value = int.Parse(policyValue.Value1);
            return policyValue.Value1Unit switch
            {
                PolicyValueUnit.Years => time.AddYears(value),
                PolicyValueUnit.Months => time.AddMonths(value),
                PolicyValueUnit.Weeks => time.AddDays(value * 7),
                PolicyValueUnit.Days => time.AddDays(value),
                _ => DateTime.MinValue
            };
        }

        public Rule MatchRule(ObjectInfoBase obj)
        {
            Rule result = null;
            if (_rules == null)
            {
                return null;
            }

            result = CheckCriteria(obj);
            return result;
        }
        private Rule CheckCriteria(ObjectInfoBase info)
        {
            //already sorted when querying the mappings from the db
            //var rules = _rules.OrderBy(t => t.Order);
            foreach (var rule in _rules)
            {
                try
                {
                    if (rule.ConnectorRule.Filters == null || rule.ConnectorRule.AndOrExpression == null)
                    {
                        continue;
                    }

                    var engine = new FilterEngine(rule.ConnectorRule.Filters, rule.ConnectorRule.AndOrExpression, true);
                    if (engine.IsQualified(info))
                    {
                        return rule;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        logger.Error("A property was not assigned while checking a connector rule. Exception:{0}", ex.ToString());
                    }
                    //logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.Compression, ex.ToString());
                    var thEX = new Exception(string.Format("Checked expression failed.{0}", rule.Compression), ex);
                    thEX.Data.Add("ruleName", rule.Name);
                    throw thEX;
                }
            }
            return null;
        }
    }
}
