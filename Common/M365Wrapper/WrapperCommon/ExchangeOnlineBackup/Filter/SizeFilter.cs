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

    public class SizeFilter : AbstractFilterRule
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
            FilterResult result = new();
            BaseProperty = GetProperty(type);
            string propertyValue;
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
                case EOConditionType.LargerThan:
                    result = CheckLargeThan(propertyValue, true);
                    break;
                case EOConditionType.LessThan:
                    result = CheckLargeThan(propertyValue, false);
                    break;
            }
            return result;
        }

        private FilterResult CheckLargeThan(string filterValue, bool isLarge)
        {
            EOSizeValue value = FilterValue as EOSizeValue;
            EOSizeType sizeType = value.SizeUnit;

            FilterResult result = new();
            long conditionValue;
            if (sizeType == EOSizeType.GB)
            {
                conditionValue = Int64.Parse(value.Value) * 1024 * 1024;
            }
            if (sizeType == EOSizeType.MB)
            {
                conditionValue = Int64.Parse(value.Value) * 1024;
            }
            else
            {
                conditionValue = Int64.Parse(value.Value);
            }

            if (isLarge == (Int64.Parse(filterValue) / 1024 >= conditionValue))
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
    }
}