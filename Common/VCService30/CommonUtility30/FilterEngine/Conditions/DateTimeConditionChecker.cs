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
    using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;
    using System.Reflection;
    #endregion

    internal static class DateTimeConditionChecker
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool IsQualified(PolicyCondition policyCondition, DateTime objectValue, PolicyValue policyValue, bool skipCheckDateTimeMinValue = false)
        {
            if (!skipCheckDateTimeMinValue && (objectValue == DateTime.MinValue || objectValue == DateTime.MaxValue))
            {
                logger.Info($"DateTimeConditionChecker objectValue is ilegal and skip check.objectValue:{objectValue}.");
                return false;
            }
            DateTime policyDateTimeValue1;
            DateTime policyDateTimeValue2;
            objectValue = DateTime.SpecifyKind(objectValue, DateTimeKind.Utc);
            int dayWeekMonthYear;
            switch (policyCondition)
            {
                case PolicyCondition.FromTo:
                    policyDateTimeValue1 = DateTime.Parse(policyValue.Value1);
                    policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
                    policyDateTimeValue2 = DateTime.Parse(policyValue.Value2);
                    policyDateTimeValue2 = DateTime.SpecifyKind(policyDateTimeValue2, DateTimeKind.Utc);
                    return ConditionChecker.Between(objectValue, policyDateTimeValue1, policyDateTimeValue2);
                case PolicyCondition.Before:
                    policyDateTimeValue1 = DateTime.Parse(policyValue.Value1);
                    policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
                    return ConditionChecker.Before(objectValue, policyDateTimeValue1);
                case PolicyCondition.After:
                    policyDateTimeValue1 = DateTime.Parse(policyValue.Value1);
                    policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
                    return ConditionChecker.After(objectValue, policyDateTimeValue1);
                case PolicyCondition.On:
                    policyDateTimeValue1 = DateTime.Parse(policyValue.Value1);
                    policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
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
                case PolicyCondition.OlderThanNow:
                    {
                        return ConditionChecker.OlderThanNow(objectValue);
                    }
                case PolicyCondition.OlderThan:
                    try
                    {
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
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return false;
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
    }
}
