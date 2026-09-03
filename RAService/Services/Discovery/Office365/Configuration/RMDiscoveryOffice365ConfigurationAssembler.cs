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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Configuration
{
    public class RMDiscoveryOffice365ConfigurationAssembler
    {
        private RMDiscoveryOffice365ScopeInfo _scopeInfo;

        private RMDiscoveryExclusionInfo _exclusionInfo;

        private List<RMDiscoverySizeRangeDataInfo> _sizeRangeInfoes;

        private List<RMDiscoveryWithoutInDateDataInfo> _dateRangeInfoes;

        private RMDiscoveryOffice365InactiveDefinition _inactiveDefinition;

        private RMDiscoveryOffice365RotDefinition _rotDefinition;

        private List<RMDiscoveryOffice365RuleInfo> _rules;

        private RMDiscoveryOffice365ConfigurationAssembler() { }

        public static RMDiscoveryOffice365ConfigurationAssembler Instance => new();

        public RMDiscoveryOffice365ConfigurationAssembler AddScopeInfo(RMDiscoveryOffice365ScopeInfo scopeInfo)
        {
            _scopeInfo = scopeInfo;
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddExclusionInfo(RMDiscoveryExclusionInfo exclusionInfo)
        {
            _exclusionInfo = exclusionInfo;
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddSizeRangeInfo(List<RMDiscoverySizeRangeDataInfo> sizeRangeInfoes)
        {
            _sizeRangeInfoes = sizeRangeInfoes;
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddDateRangeInfo(List<RMDiscoveryWithoutInDateDataInfo> dateRangeInfoes)
        {
            _dateRangeInfoes = dateRangeInfoes;
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddInactiveDefinition(RMDiscoveryOffice365InactiveDefinition inactiveDefinition)
        {
            _inactiveDefinition = inactiveDefinition;
            if (!inactiveDefinition.Enable)
            {
                _inactiveDefinition = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_INACTIVE_DEFINITION;
                _inactiveDefinition.Enable = false;
            }
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddRotDefinition(RMDiscoveryOffice365RotDefinition rotDefinition)
        {
            _rotDefinition = rotDefinition;
            if (!rotDefinition.Enable)
            {
                _rotDefinition = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_ROT_DEFINITION;
                _rotDefinition.Enable = false;
            }
            return this;
        }

        public RMDiscoveryOffice365ConfigurationAssembler AddRules(IEnumerable<RMDiscoveryOffice365RuleInfo> rules)
        {
            _rules = rules.ToList();
            return this;
        }

        public RMDiscoveryOffice365ConfigurationInfo Assemble()
        {
            var res = new RMDiscoveryOffice365ConfigurationInfo();
            var groupedRules = _rules.GroupBy(item => item.DefinitionKind).ToDictionary(item => item.Key, item => item.OrderBy(item => item.Order).ToList());
            res.ScopeInfo = _scopeInfo;
            res.SizeRangeInfoes = _sizeRangeInfoes;
            res.DateRangeInfoes = _dateRangeInfoes;
            res.ExclusionInfo = _exclusionInfo;
            res.InactiveDefinition = _inactiveDefinition;
            res.RotDefinition = _rotDefinition;

            if (_scopeInfo != null && groupedRules.TryGetValue(RMDiscoveryRuleDefinitionKind.Inactive, out var inactiveRules))
            {
                res.InactiveDefinition.Rules = inactiveRules.ConvertAll(item => RMDiscoveryRuleConverter.Convert(item));
            }

            if (groupedRules.TryGetValue(RMDiscoveryRuleDefinitionKind.ROT, out var rotRules))
            {
                var groupedRotRules = rotRules.GroupBy(item => item.Category).ToDictionary(item => item.Key, item => item.OrderBy(item => item.Order).ToList());
                if (groupedRotRules.TryGetValue(RMDiscoveryRuleCategory.Redundant, out var redundantRules))
                {
                    res.RotDefinition.RedundantRules = redundantRules.ConvertAll(item => RMDiscoveryRuleConverter.Convert(item));
                }
                if (groupedRotRules.TryGetValue(RMDiscoveryRuleCategory.Obsolete, out var obsoleteRules))
                {
                    res.RotDefinition.ObsoleteRules = obsoleteRules.ConvertAll(item => RMDiscoveryRuleConverter.Convert(item));
                }
                if (groupedRotRules.TryGetValue(RMDiscoveryRuleCategory.Trivial, out var trivialRules))
                {
                    res.RotDefinition.TrivialRules = trivialRules.ConvertAll(item => RMDiscoveryRuleConverter.Convert(item));
                }
            }
            return res;
        }
    }
}
