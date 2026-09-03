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
    using AvePoint.GCommon.Contract.CommonFilter;
    using System.Collections.Generic;

    public class FilterOption
    {
        /// <summary>
        /// FilterPolicy类集合. 每一个FilterPolicy(FilterCondition更为恰当)类对应一条Filter Condition, 例如：SiteCollection Url Contains https://
        /// </summary>
        public List<FilterPolicy> FilterPolicies { get; set; }
        /// <summary>
        /// FilterCondtion表达式集合. 每个Level对应一条表达式(如果该Level存在Filter), 例如：1 and 2
        /// </summary>
        public Dictionary<PolicyLevel, string> FilterConditionExpressions { get; set; }
        /// <summary>
        /// 用于控制, 如果在某个PolicyLevel上没有设置Rule, 如何进行Filter. True: IsQualified()返回false, 否则返回true, 默认为false.
        /// </summary>
        public bool IsNoRuleFilterOut { get; set; }
        /// <summary>
        /// 是否FilterOut. True: FilterOut, 即对于符合条件的节点返回false, False: FilterIn, 即对于符合条件的节点返回true. 默认为false.
        /// </summary>
        public bool IsRealFilterOut { get; set; }
        /// <summary>
        /// 对于FilterPolicy打log的Level
        /// </summary>
        public RecordFilterPolicyLog LogLevel { get; set; }

        public FilterOption() 
        {
            this.FilterPolicies = new List<FilterPolicy>();
            this.FilterConditionExpressions = new Dictionary<PolicyLevel, string>();
            this.IsNoRuleFilterOut = false;
            this.IsRealFilterOut = false;
            this.LogLevel = RecordFilterPolicyLog.None;
        }
    }

    public enum RecordFilterPolicyLog
    {
        /// <summary>
        /// 不打任何DEBUG Log
        /// </summary>
        None = 0,
        /// <summary>
        /// 对于每个节点的所有Filter Condition都打DEBUG Log
        /// </summary>
        All = 1,
        /// <summary>
        /// 只对不符合条件的节点的Filter Condition打DEBUG Log
        /// </summary>
        Portion = 2,
    }
}
