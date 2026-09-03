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


namespace ExchangeOnlineBackup
{
    #region namespace

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;

    #endregion namespace

    public class TimeFilter : AbstractFilterRule
    {
        public override void Initialize(BaseFilterItem baseFilterItem)
        {
            CategoryType = baseFilterItem.FilterCategoryType;
            AndOrInfo = baseFilterItem.AndOr;
            ConditionType = baseFilterItem.FilterConditionType;
            RuleType = baseFilterItem.FilterRuleType;
            FilterValue = baseFilterItem.FilterValue;
        }

        public override FilterResult CheckFilterStatus(Dictionary<string, ProposeInfo> propValueDic, EOCategoryType type)
        {
            FilterResult result = new FilterResult();
            string propertyValue = null;
            BaseProperty = GetProperty(type);
            if (!propValueDic.ContainsKey(BaseProperty) || propValueDic[BaseProperty].Value == null)
            {
                result.State = FilterState.Passed;
                return result;
            }
            else
            {
                propertyValue = propValueDic[BaseProperty].Value;
            }
            switch (ConditionType)
            {
                case EOConditionType.Before:
                    result = CheckSignleTime(propertyValue, true);
                    break;
                case EOConditionType.After:
                    result = CheckSignleTime(propertyValue, false);
                    break;
                case EOConditionType.Within:
                    result = CheckComplexTime(propertyValue, true);
                    break;
                case EOConditionType.OldThan:
                    result = CheckComplexTime(propertyValue, false);
                    break;
            }
            return result;
        }

        private FilterResult CheckSignleTime(string propertyValue, bool isBefore)
        {
            EODateTimeZoneValue datetimeZoneValue = FilterValue as EODateTimeZoneValue;
            FilterResult result = new FilterResult();
            TimeZoneInfo zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(datetimeZoneValue.TimeZoneId);
            DateTime convertTime = new DateTime(datetimeZoneValue.Value.Ticks);
            DateTime tempTime = TimeZoneInfo.ConvertTimeToUtc(convertTime, zoneInfo);
            DateTime propTime;
            if (!string.IsNullOrEmpty(propertyValue) && DateTime.TryParse(propertyValue, out propTime))
            {
                if (propTime != null)
                {
                    propTime = propTime.ToLocalTime().ToUniversalTime();
                    int second = 0 - propTime.Second;
                    propTime = propTime.AddSeconds(second);
                    if (isBefore == (propTime <= tempTime))
                    {
                        result.State = FilterState.Passed;
                    }
                    else
                    {
                        result.State = FilterState.Filtered;
                        //result.message = "The item does not fulfills the criterion.";
                        result.Message = "EOBFilterResultMessage";
                    }
                }
            }
            return result;
        }

        private FilterResult CheckComplexTime(string propertyValue, bool isWithIn)
        {
            FilterResult result = new FilterResult();
            if (FilterValue is EODateTimeRangeValue datetimeRangeValue)
            {
                if (!string.IsNullOrEmpty(datetimeRangeValue.Value) && int.TryParse(datetimeRangeValue.Value, out int interval))
                {
                    DateTime tempTime = DateTime.Now.ToUniversalTime();
                    switch (datetimeRangeValue.TimeUnit)
                    {
                        case EODateTimeType.Day:
                            tempTime = tempTime.AddDays((-1) * interval);
                            break;
                        case EODateTimeType.Weeks:
                            tempTime = tempTime.AddDays((-7) * interval);
                            break;
                        case EODateTimeType.Months:
                            tempTime = tempTime.AddMonths((-1) * interval);
                            break;
                        case EODateTimeType.Years:
                            tempTime = tempTime.AddYears((-1) * interval);
                            break;
                        default:
                            break;
                    }
                    if (!string.IsNullOrEmpty(propertyValue) && DateTime.TryParse(propertyValue, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime propTime))
                    {
                        if (propTime != null)
                        {
                            propTime = propTime.ToLocalTime().ToUniversalTime();
                            if (isWithIn == (tempTime <= propTime))
                            {
                                result.State = FilterState.Passed;
                            }
                            else
                            {
                                result.State = FilterState.Filtered;
                                //result.message = "The item does not fulfills the criterion.";
                                result.Message = "EOBFilterResultMessage";
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}