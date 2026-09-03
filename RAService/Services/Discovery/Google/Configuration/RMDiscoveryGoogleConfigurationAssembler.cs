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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.RACommonUtility.Converter.Discovery;

namespace AvePoint.RA.Service.Services.Discovery.Google.Configuration
{
    public class RMDiscoveryGoogleConfigurationAssembler
    {
        private RMDiscoveryGoogleScopeInfo _scopeInfo;

        private List<RMDiscoverySizeRangeDataInfo> _sizeRangeInfoes;

        private List<RMDiscoveryWithoutInDateDataInfo> _dateRangeInfoes;

        private RMDiscoveryGoogleRotDefinition _rotDefinition;

        private List<RMDiscoveryGoogleRuleInfo> _rules;

        private RMDiscoveryGoogleConfigurationAssembler() { }

        private static readonly Lazy<RMDiscoveryGoogleConfigurationAssembler> s_instance = new(() => new RMDiscoveryGoogleConfigurationAssembler());

        public static RMDiscoveryGoogleConfigurationAssembler Instance => s_instance.Value;

        public RMDiscoveryGoogleConfigurationAssembler AddScopeInfo(RMDiscoveryGoogleScopeInfo scopeInfo)
        {
            _scopeInfo = scopeInfo;
            return this;
        }

        public RMDiscoveryGoogleConfigurationAssembler AddSizeRangeInfo(List<RMDiscoverySizeRangeDataInfo> sizeRangeInfoes)
        {
            _sizeRangeInfoes = sizeRangeInfoes;
            return this;
        }

        public RMDiscoveryGoogleConfigurationAssembler AddDateRangeInfo(List<RMDiscoveryWithoutInDateDataInfo> dateRangeInfoes)
        {
            _dateRangeInfoes = dateRangeInfoes;
            return this;
        }

        public RMDiscoveryGoogleConfigurationAssembler AddRotDefinition(RMDiscoveryGoogleRotDefinition rotDefinition)
        {
            _rotDefinition = rotDefinition;
            if (!rotDefinition.Enable)
            {
                _rotDefinition = RMDiscoveryGoogleDefaultConfigurationInfo.DEFAULT_ROT_DEFINITION;
                _rotDefinition.Enable = false;
            }
            return this;
        }

        public RMDiscoveryGoogleConfigurationAssembler AddRules(IEnumerable<RMDiscoveryGoogleRuleInfo> rules)
        {
            _rules = rules.ToList();
            return this;
        }

        public RMDiscoveryGoogleConfigurationInfo Assemble()
        {
            var res = new RMDiscoveryGoogleConfigurationInfo();
            var groupedRules = _rules.GroupBy(item => item.DefinitionKind).ToDictionary(item => item.Key, item => item.OrderBy(item => item.Order).ToList());
            res.ScopeInfo = _scopeInfo;
            res.SizeRangeInfoes = _sizeRangeInfoes;
            res.DateRangeInfoes = _dateRangeInfoes;
            res.RotDefinition = _rotDefinition;

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
