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
using AvePoint.RA.Contract.RMRuleManageMent;
using RADataSynchronize.TermCheck.Model;
using System;

namespace RADataSynchronize.TermCheck.CriteriaCheckers
{
    public class DocumentSizeCriteriaChecker : CriteriaChecker
    {
        public override ArchiverFilterRuleType CriteriaType => ArchiverFilterRuleType.DocumentSize;

        public override bool Check(CriteriaInfo criteriaInfo, object objValue)
        {
            var criteriaValue = CalculateSize(Convert.ToDouble(criteriaInfo.Value1), criteriaInfo.Value1Unit);
            var value = Convert.ToDouble(objValue);
            switch (criteriaInfo.Condition)
            {
                case ArchiverFilterCondition.LessThanOrEqualTo:
                    return ConditionChecker.LessOrEqualThan(value, criteriaValue);
                case ArchiverFilterCondition.GreaterThanOrEqualTo:
                    return ConditionChecker.BiggerOrEqualThan(value, criteriaValue);
                default:
                    throw new InvalidOperationException($"Criteria: [{CriteriaType}] not has condition [{criteriaInfo.Condition}] check logic.");
            }
        }

        private double CalculateSize(double criteriaValue, PolicyValueUnit policyValueUnit)
        {
            switch (policyValueUnit)
            {
                case PolicyValueUnit.KB:
                    return criteriaValue * 1024;
                case PolicyValueUnit.MB:
                    return criteriaValue * Math.Pow(1024, 2);
                case PolicyValueUnit.GB:
                    return criteriaValue * Math.Pow(1024, 3);
                default:
                    throw new InvalidCastException($"CriteriaType: [{CriteriaType}] not support calculate value type [{policyValueUnit}].");
            }
        }
    }
}
