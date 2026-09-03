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



namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon;
    #endregion

    internal static class DateTimeConditionChecker
    {
        public static bool IsQualified(PolicyCondition policyCondition, DateTime objectValue, PolicyValue policyValue)
        {
            DateTime policyDateTimeValue1;
            DateTime policyDateTimeValue2;
            if (objectValue.Kind != DateTimeKind.Utc)
            {
                //policyValue传进来的是UTC时间，所以将objectValue也转化为UTC时间
                //objectValue = objectValue.ToUniversalTime();
                objectValue = GetFilterDateTimeValue(objectValue.ToString());
            }
            int dayWeekMonthYear;
            switch (policyCondition)
            {
                case PolicyCondition.FromTo:
                    policyDateTimeValue1 = GetFilterDateTimeValue(policyValue.Value1);
                    policyDateTimeValue2 = GetFilterDateTimeValue(policyValue.Value2);
                    return ConditionChecker.Between(objectValue, policyDateTimeValue1, policyDateTimeValue2);
                case PolicyCondition.Before:
                    policyDateTimeValue1 = GetFilterDateTimeValue(policyValue.Value1);
                    return ConditionChecker.Before(objectValue, policyDateTimeValue1);
                case PolicyCondition.After:
                    policyDateTimeValue1 = GetFilterDateTimeValue(policyValue.Value1);
                    return ConditionChecker.After(objectValue, policyDateTimeValue1);
                case PolicyCondition.On:
                    policyDateTimeValue1 = GetFilterDateTimeValue(policyValue.Value1);
                    return ConditionChecker.On(objectValue, policyDateTimeValue1);
                case PolicyCondition.WithIn:
                    switch (policyValue.Value1Unit)
                    {
                        case PolicyValueUnit.Days:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.WithInDays(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Weeks:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.WithInWeeks(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Months:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.WithInMonths(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Years:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.WithInYears(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.None:
                        case PolicyValueUnit.KB:
                        case PolicyValueUnit.MB:
                        case PolicyValueUnit.GB:
                        default:
                            throw new PolicyValueUnitNotSupportedException(policyValue.Value1Unit.ToString());
                    }
                case PolicyCondition.OlderThan:
                    switch (policyValue.Value1Unit)
                    {
                        case PolicyValueUnit.Days:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.OlderThanDays(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Weeks:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.OlderThanWeeks(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Months:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.OlderThanMonths(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Years:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.OlderThanYears(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.None:
                        case PolicyValueUnit.KB:
                        case PolicyValueUnit.MB:
                        case PolicyValueUnit.GB:
                        default:
                            throw new PolicyValueUnitNotSupportedException(policyValue.Value1Unit.ToString());
                    }
                case PolicyCondition.LessThan:
                    switch (policyValue.Value1Unit)
                    {
                        case PolicyValueUnit.Days:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.LessThanDays(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Weeks:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.LessThanWeeks(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Months:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.LessThanMonths(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.Years:
                            dayWeekMonthYear = int.Parse(policyValue.Value1);
                            return ConditionChecker.LessThanYears(objectValue, dayWeekMonthYear);
                        case PolicyValueUnit.None:
                        case PolicyValueUnit.KB:
                        case PolicyValueUnit.MB:
                        case PolicyValueUnit.GB:
                        default:
                            throw new PolicyValueUnitNotSupportedException(policyValue.Value1Unit.ToString());
                    }
                case PolicyCondition.Exactly:
                case PolicyCondition.Contains:
                case PolicyCondition.StartWith:
                case PolicyCondition.EndWith:
                case PolicyCondition.LessOrEqualThan:
                case PolicyCondition.GreaterOrEqualThan:
                case PolicyCondition.OnlyLastNVersions:
                case PolicyCondition.OnlyLastMajorNVersions:
                case PolicyCondition.OnlyMajorVersions:
                case PolicyCondition.OnlyApproved:
                default:
                    throw new ConditionNotSupportedException(policyCondition.ToString());
            }
        }

        private static DateTime GetFilterDateTimeValue(string dateTime)
        {
            DateTime filterValue;
            if (!DateTime.TryParse(dateTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out filterValue))
            {
                filterValue = DateTime.Parse(dateTime);
            }
            return DateTime.SpecifyKind(filterValue, DateTimeKind.Utc);
        }
    }
}
