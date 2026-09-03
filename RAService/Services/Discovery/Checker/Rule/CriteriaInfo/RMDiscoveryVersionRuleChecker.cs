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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Criteria;
using AvePoint.RA.Service.Services.Discovery.Checker.Rule.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Checker.Rule.CriteriaInfo
{
    public class RMDiscoveryVersionRuleChecker : RMDiscoveryRuleCriteriaInfoesChecker
    {
        public override RMDiscoveryRuleAnalyseMethod AnalyseMethod => RMDiscoveryRuleAnalyseMethod.Version;

        protected override bool Check(RMDiscoveryRuleCriteriaInfo criteriaInfo)
        {
            var criteriaType = (RMDiscoveryVersionCriteriaType)criteriaInfo.CriteriaType;
            if (!Enum.IsDefined(criteriaType))
            {
                return false;
            }

            var conditionCategory = criteriaInfo.ConditionInfo.Category;

            var isMatch = criteriaType switch
            {
                RMDiscoveryVersionCriteriaType.KeepLastVersions => conditionCategory == RMDiscoveryConditionCategory.Version,
                RMDiscoveryVersionCriteriaType.ModifiedTime => conditionCategory == RMDiscoveryConditionCategory.DateTime,
                RMDiscoveryVersionCriteriaType.DocumentType => conditionCategory == RMDiscoveryConditionCategory.Array || conditionCategory == RMDiscoveryConditionCategory.BooleanLogic,
                RMDiscoveryVersionCriteriaType.DocumentSize => conditionCategory == RMDiscoveryConditionCategory.FileSize,
                _ => false
            };

            return isMatch && RMDiscoveryConditionChecker.Check(criteriaInfo.ConditionInfo);
        }
    }
}
