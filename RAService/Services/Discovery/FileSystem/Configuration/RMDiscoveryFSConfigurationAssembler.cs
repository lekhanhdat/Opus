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
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.RACommonUtility.Converter.Discovery;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Configuration
{
    public class RMDiscoveryFSConfigurationAssembler
    {
        private RMDiscoveryFSScopeInfo _scopeInfo;

        private List<RMDiscoverySizeRangeDataInfo> _sizeRangeInfoes;

        private List<RMDiscoveryWithoutInDateDataInfo> _dateRangeInfoes;

        private RMDiscoveryFSInactiveDefinition _inactiveDefinition;

        private RMDiscoveryFSRotDefinition _rotDefinition;

        private List<RMDiscoveryFSRuleInfo> _rules;

        private RMDiscoveryFSConfigurationAssembler() { }

        public static RMDiscoveryFSConfigurationAssembler Instance => new();

        public RMDiscoveryFSConfigurationAssembler AddScopeInfo(RMDiscoveryFSScopeInfo scopeInfo)
        {
            _scopeInfo = scopeInfo;
            return this;
        }

        public RMDiscoveryFSConfigurationAssembler AddSizeRangeInfo(List<RMDiscoverySizeRangeDataInfo> sizeRangeInfoes)
        {
            _sizeRangeInfoes = sizeRangeInfoes;
            return this;
        }

        public RMDiscoveryFSConfigurationAssembler AddDateRangeInfo(List<RMDiscoveryWithoutInDateDataInfo> dateRangeInfoes)
        {
            _dateRangeInfoes = dateRangeInfoes;
            return this;
        }

        public RMDiscoveryFSConfigurationAssembler AddInactiveDefinition(RMDiscoveryFSInactiveDefinition inactiveDefinition)
        {
            _inactiveDefinition = inactiveDefinition;
            if (!inactiveDefinition.Enable)
            {
                _inactiveDefinition = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_INACTIVE_DEFINITION;
                _inactiveDefinition.Enable = false;
            }
            return this;
        }

        public RMDiscoveryFSConfigurationAssembler AddRotDefinition(RMDiscoveryFSRotDefinition rotDefinition)
        {
            _rotDefinition = rotDefinition;
            if (!rotDefinition.Enable)
            {
                _rotDefinition = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_ROT_DEFINITION;
                _rotDefinition.Enable = false;
            }
            return this;
        }

        public RMDiscoveryFSConfigurationAssembler AddRules(IEnumerable<RMDiscoveryFSRuleInfo> rules)
        {
            _rules = rules.ToList();
            return this;
        }

        public RMDiscoveryFSConfigurationInfo Assemble()
        {
            var res = new RMDiscoveryFSConfigurationInfo();
            var groupedRules = _rules.GroupBy(item => item.DefinitionKind).ToDictionary(item => item.Key, item => item.OrderBy(item => item.Order).ToList());
            res.ScopeInfo = _scopeInfo;
            res.SizeRangeInfoes = _sizeRangeInfoes;
            res.DateRangeInfoes = _dateRangeInfoes;
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
