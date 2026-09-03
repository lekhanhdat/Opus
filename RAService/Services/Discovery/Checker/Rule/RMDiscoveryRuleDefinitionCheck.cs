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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Service.Services.Discovery.Checker.Rule.CriteriaInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Checker.Rule
{
    public class RMDiscoveryRuleDefinitionChecker
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryRuleDefinitionChecker));

        private readonly List<Func<List<RMDiscoveryRuleDefinition>, bool>> _predicates = new();

        private readonly List<RMDiscoveryRuleDefinition> _ruleDefinitions;

        private RMDiscoveryRuleDefinitionChecker(IEnumerable<RMDiscoveryRuleDefinition> ruleDefinitions)
        {
            _ruleDefinitions = ruleDefinitions.ToList();
        }

        public static RMDiscoveryRuleDefinitionChecker Create(IEnumerable<RMDiscoveryRuleDefinition> ruleDefinitions)
        {
            return new RMDiscoveryRuleDefinitionChecker(ruleDefinitions);
        }

        public RMDiscoveryRuleDefinitionChecker CheckEmpty()
        {
            _predicates.Add(items => items.Any());
            return this;
        }

        public RMDiscoveryRuleDefinitionChecker CheckName()
        {
            _predicates.Add(items => items.All(item => !string.IsNullOrWhiteSpace(item.Name)));
            _predicates.Add(items => items.Select(item => item.Name.ToLower())
                .GroupBy(item => item)
                .All(item => item.Count() == 1));
            return this;
        }

        public RMDiscoveryRuleDefinitionChecker CheckOrder()
        {
            _predicates.Add(items => items.Select(item => item.Order)
                .GroupBy(item => item)
                .All(item => item.Count() == 1));
            _predicates.Add(items => items.Max(item => item.Order) == _ruleDefinitions.Count);
            _predicates.Add(items => items.All(item => item.Order > 0));
            return this;
        }

        public RMDiscoveryRuleDefinitionChecker CheckEnable()
        {
            _predicates.Add(items => items.Any(item => item.IsEnable));
            return this;
        }

        public RMDiscoveryRuleDefinitionChecker CheckKind(RMDiscoveryRuleDefinitionKind kind)
        {
            //_predicates.Add(items => items.All(item => item.Kind == kind));
            return this;
        }

        public RMDiscoveryRuleDefinitionChecker CheckAnalyseMethod(params RMDiscoveryRuleAnalyseMethod[] analyseMethods)
        {
            if (analyseMethods.Any())
            {
                _predicates.Add(items => items.All(item => analyseMethods.Contains(item.AnalyseMethod)));
            }
            return this;
        }

        public bool Check()
        {
            var isPassed = _predicates.All(item => item(_ruleDefinitions));
            if (!isPassed)
            {
                return false;
            }

            return _ruleDefinitions.All(item =>
            {
                var result = RMDiscoveryRuleCriteriaInfoesChecker.Check(item.AnalyseMethod, item.CriteriaInfoes);
                _logger.Info($"Check [{item.Name}] criteria is right : {result}");
                return result;
            });
        }
    }
}
