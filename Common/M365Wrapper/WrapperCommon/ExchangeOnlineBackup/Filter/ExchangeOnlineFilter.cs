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

    public class ExchangeOnlineFilter
    {
        public List<AbstractFilterRule> FilterRules { set; get; }

        public void CreateFilterRule(ExchangeOnlineBackupFilterDto EOBackupFilter)
        {
            FilterRules = new List<AbstractFilterRule>();
            foreach (BaseFilterItem baseFilter in EOBackupFilter.FItems)
            {
                AbstractFilterRule rule;
                if (baseFilter.FilterCustomRuleType == EOCustomRuleType.String || baseFilter.FilterCustomRuleType == EOCustomRuleType.Enum)
                {
                    rule = new TextFilter();
                }
                else if (baseFilter.FilterCustomRuleType == EOCustomRuleType.Size)
                {
                    rule = new SizeFilter();
                }
                else
                {
                    rule = new TimeFilter();
                }
                rule.Initialize(baseFilter);
                FilterRules.Add(rule);
            }
        }

        public FilterResult CheckFilterCondition(Dictionary<string, ProposeInfo> propValueDic, EOCategoryType filterType, bool isFolderFilter)
        {
            //using (new AvePerformanceScope("ExchangeOnlineBackup.ExchangeOnlineFilter.CheckFilterCondition"))
            //{
                FilterResult result = new();
                bool isCheckNext = true;
                foreach (AbstractFilterRule rule in FilterRules)
                {
                    if (filterType != rule.CategoryType)
                    {
                        continue;
                    }

                    result = rule.CheckFilterStatus(propValueDic, filterType);

                    isCheckNext = (rule.AndOrInfo == EOAndOrType.And) != (result.State == FilterState.Filtered);
                    if (!isCheckNext)
                    {
                        break;
                    }
                }
                return result;
            //}
        }
    }
}