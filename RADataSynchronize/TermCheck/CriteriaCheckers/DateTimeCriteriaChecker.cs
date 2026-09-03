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

namespace RADataSynchronize.TermCheck.CriteriaCheckers
{
    public abstract class DateTimeCriteriaChecker : CriteriaChecker
    {
        public override bool Check(CriteriaInfo criteriaInfo, object objValue)
        {
            var value = new DateTime(Convert.ToInt64(objValue));
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            switch (criteriaInfo.Condition)
            {
                case ArchiverFilterCondition.FromTo:
                    var fromValue = ToUtc(criteriaInfo.Value1);
                    var toValue = ToUtc(criteriaInfo.Value2);
                    return ConditionChecker.Between(value, fromValue, toValue);
                case ArchiverFilterCondition.Before:
                    var criteriaValue = ToUtc(criteriaInfo.Value1);
                    return ConditionChecker.Before(value, criteriaValue);
                case ArchiverFilterCondition.OlderThan:
                    return OlderThanCheck(criteriaInfo, value);
                default:
                    throw new InvalidOperationException($"Criteria: [{CriteriaType}] not has condition [{criteriaInfo.Condition}] check logic.");
            }
        }

        private bool OlderThanCheck(CriteriaInfo criteriaInfo, DateTime value)
        {
            var criteriaValue = Convert.ToInt32(criteriaInfo.Value1);
            switch (criteriaInfo.Value1Unit)
            {
                case PolicyValueUnit.Days:
                    return ConditionChecker.OlderThanDays(value, criteriaValue);
                case PolicyValueUnit.Weeks:
                    return ConditionChecker.OlderThanWeeks(value, criteriaValue);
                case PolicyValueUnit.Months:
                    return ConditionChecker.OlderThanMonths(value, criteriaValue);
                case PolicyValueUnit.Years:
                    return ConditionChecker.OlderThanYears(value, criteriaValue);
                default:
                    throw new InvalidCastException($"CriteriaType: [{CriteriaType}] not support calculate value type [{criteriaInfo.Value1Unit}].");
            }
        }

        private DateTime ToUtc(object value)
        {
            if (value == null)
            {
                return DateTime.MinValue;
            }
            var dateTime = Convert.ToDateTime(value);
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}
