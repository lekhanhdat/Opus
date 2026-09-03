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
    public class RMDiscoveryDocumentRuleCriteriaInfoesChecker : RMDiscoveryRuleCriteriaInfoesChecker
    {
        public override RMDiscoveryRuleAnalyseMethod AnalyseMethod => RMDiscoveryRuleAnalyseMethod.Document;

        protected override bool Check(RMDiscoveryRuleCriteriaInfo criteriaInfo)
        {
            var criteriaType = (RMDiscoveryDocumentCriteriaType)criteriaInfo.CriteriaType;
            if (!Enum.IsDefined(criteriaType))
            {
                return false;
            }

            var conditionCategory = criteriaInfo.ConditionInfo.Category;

            var isMatch = criteriaType switch
            {
                RMDiscoveryDocumentCriteriaType.Name => conditionCategory == RMDiscoveryConditionCategory.Array,
                RMDiscoveryDocumentCriteriaType.ParentFolder => conditionCategory == RMDiscoveryConditionCategory.Array,
                RMDiscoveryDocumentCriteriaType.CreatedTime => conditionCategory == RMDiscoveryConditionCategory.DateTime,
                RMDiscoveryDocumentCriteriaType.ModifiedTime => conditionCategory == RMDiscoveryConditionCategory.DateTime,
                RMDiscoveryDocumentCriteriaType.DocumentType => conditionCategory == RMDiscoveryConditionCategory.Array || conditionCategory == RMDiscoveryConditionCategory.BooleanLogic,
                RMDiscoveryDocumentCriteriaType.DocumentSize => conditionCategory == RMDiscoveryConditionCategory.FileSize,
                RMDiscoveryDocumentCriteriaType.ParentLibraryText or RMDiscoveryDocumentCriteriaType.ParentSiteCollectionText or RMDiscoveryDocumentCriteriaType.PropertyBagText => conditionCategory == RMDiscoveryConditionCategory.TextExtraInput,
                RMDiscoveryDocumentCriteriaType.ParentLibraryBoolean or RMDiscoveryDocumentCriteriaType.ParentSiteCollectionBoolean or RMDiscoveryDocumentCriteriaType.PropertyBagBoolean => conditionCategory == RMDiscoveryConditionCategory.BooleanExtraInput,
                RMDiscoveryDocumentCriteriaType.ParentLibraryNumber or RMDiscoveryDocumentCriteriaType.ParentSiteCollectionNumber or RMDiscoveryDocumentCriteriaType.PropertyBagNumber => conditionCategory == RMDiscoveryConditionCategory.NumberExtraInput,
                RMDiscoveryDocumentCriteriaType.ParentLibraryDateTime or RMDiscoveryDocumentCriteriaType.ParentSiteCollectionDateTime or RMDiscoveryDocumentCriteriaType.PropertyBagDateTime => conditionCategory == RMDiscoveryConditionCategory.DateTimeExtraInput,
                _ => false
            };

            return isMatch && RMDiscoveryConditionChecker.Check(criteriaInfo.ConditionInfo);
        }
    }
}
