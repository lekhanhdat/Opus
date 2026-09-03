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
    using System.Linq;
    using System.Text;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;

    #endregion namespace

    public class TextFilter : AbstractFilterRule
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
                case EOConditionType.StringContains:
                    result = CheckContain(propertyValue, true);
                    break;
                case EOConditionType.StringNotContains:
                    result = CheckContain(propertyValue, false);
                    break;
                case EOConditionType.StringEquals:
                    result = CheckEqual(propertyValue, true, true);
                    break;
                case EOConditionType.EnumEquals:
                case EOConditionType.EnumIs:
                    result = CheckEqual(propertyValue, true, false);
                    break;
                case EOConditionType.StringNotEquals:
                    result = CheckEqual(propertyValue, false, true);
                    break;
                case EOConditionType.EnumNotEquals:
                case EOConditionType.EnumIsNot:
                    result = CheckEqual(propertyValue, false, false);
                    break;
            }
            return result;
        }

        private FilterResult CheckEqual(string propertyValue, bool isEqual, bool isString)
        {
            FilterResult result = new FilterResult();
            string conditionValue=string.Empty;
            if (isString)
            {
                conditionValue = (FilterValue as EOStringValue).Value;
            }
            else
            {
                conditionValue = (FilterValue as EOEnumValue).Value;
            }
            if (isEqual == propertyValue.Equals(conditionValue, StringComparison.OrdinalIgnoreCase))
            {
                result.State = FilterState.Passed;
            }
            else
            {
                result.State = FilterState.Filtered;
                //result.message = "The item does not fulfills the criterion.";
                result.Message = "EOBFilterResultMessage";
            }
            return result;
        }

        private FilterResult CheckContain(string propertyValue, bool isContain)
        {
            FilterResult result = new FilterResult();
            string conditionValue = (FilterValue as EOStringValue).Value;
            if (isContain == propertyValue.ToUpper().Contains(conditionValue.ToUpper()))
            {
                result.State = FilterState.Passed;
            }
            else
            {
                result.State = FilterState.Filtered;
                //result.message = "The item does not fulfill the criterion.";
                result.Message ="EOBFilterResultMessage";
            }
            return result;
        }
    }
}