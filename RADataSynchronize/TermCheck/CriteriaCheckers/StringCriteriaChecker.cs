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
using AvePoint.GCommon;
using AvePoint.RA.Contract.RMRuleManageMent;
using RADataSynchronize.TermCheck.Model;

namespace RADataSynchronize.TermCheck.CriteriaCheckers
{
    public abstract class StringCriteriaChecker : CriteriaChecker
    {
        public override bool Check(CriteriaInfo criteriaInfo, object objValue)
        {
            if (objValue is List<string> values)
            {
                return values.Any(value => Check(value, criteriaInfo));
            }
            return Check(objValue.ToString()!, criteriaInfo);
        }
        
        private bool Check(string value, CriteriaInfo criteriaInfo)
        {
            var criteriaValue = criteriaInfo.Value1.ToString();
            switch (criteriaInfo.Condition)
            {
                case ArchiverFilterCondition.Contains:
                    return ConditionChecker.Contains(value, criteriaValue);
                case ArchiverFilterCondition.DoesNotContain:
                    return !ConditionChecker.Contains(value, criteriaValue);
                case ArchiverFilterCondition.Matches:
                    return ConditionChecker.Match(value, criteriaValue);
                case ArchiverFilterCondition.DoesNotMatch:
                    return !ConditionChecker.Match(value, criteriaValue);
                case ArchiverFilterCondition.Equals:
                    return ConditionChecker.IsExactly(value, criteriaValue);
                case ArchiverFilterCondition.DoesNotEqual:
                    return !ConditionChecker.IsExactly(value, criteriaValue);
                default:
                    throw new InvalidOperationException($"Criteria: [{CriteriaType}] not has condition [{criteriaInfo.Condition}] check logic.");
            }
        }
    }
}
