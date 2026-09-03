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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser.DeploymentManager.BrowserFilter
{
    public class DesignManagerBroswerFilter
    {
        public static System.Collections.Generic.List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> GetFilterInfoListFromPolicyInfo(AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object.FilterPolicyInfo policyInfo)
        {
            System.Collections.Generic.List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> policies = new System.Collections.Generic.List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            if (policyInfo != null && policyInfo.FItems != null)
            {
                foreach (AvePoint.GCommon.Contract.CommonFilter.FilterPolicy policy in policyInfo.FItems)
                {
                    policies.Add(policy);
                }
            }
            return policies;
        }

        public static System.Collections.Generic.Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string> GetFilterPolicyAndOrExpression(AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object.FilterPolicyInfo policyInfo)
        {
            System.Collections.Generic.Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string> filterConditionExpressions = new System.Collections.Generic.Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string>();
            if (policyInfo != null && policyInfo.AndOrExpression != null)
            {
                filterConditionExpressions = policyInfo.AndOrExpression;
            }
            return filterConditionExpressions;
        }
        public static DateTime ToUniversalTime(DateTime datetime)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = datetime.ToUniversalTime();
            }
            return datetime;
        }
        /// <summary>
        /// 获得该Level每种Filter的不重复的Rule
        /// </summary>
        public static List<FilterPolicy> CreateDistinctFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();
            }
            return new List<FilterPolicy>();
        }
        public static Hashtable FillSiteColumns(IAveWeb web)
        {
            Hashtable siteCollectionColumn = new Hashtable();
            foreach (string key in web.AllProperties.Keys)
            {
                siteCollectionColumn[key] = web.AllProperties[key];
            }
            return siteCollectionColumn;
        }
    }
    #region Internal Classes

    internal class FilterRuleTypeEqualityComparer : IEqualityComparer<FilterPolicy>
    {
        private static FilterRuleTypeEqualityComparer instance;

        private FilterRuleTypeEqualityComparer()
        {
        }
        public static FilterRuleTypeEqualityComparer GetInstance()
        {
            if (instance == null)
            {
                instance = new FilterRuleTypeEqualityComparer();
            }
            return instance;
        }
        public bool Equals(FilterPolicy x, FilterPolicy y)
        {
            return x.Rule.GetType().Equals(y.Rule.GetType());
        }

        public int GetHashCode(FilterPolicy obj)
        {
            return 0;
        }
    }

    #endregion
}
