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



using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using System;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Discovery
{
    [Flags]
    public enum FilterResultMode
    {
        All = 0,
        Trim = 1,
        FilterHidden = 2
    }

    public static class FilterResultModeExtensions
    {
        public static bool HasMode(this FilterResultMode thisMode, FilterResultMode mode)
        {
            return (thisMode & mode) == mode;
        }
    }

    public abstract class AveDiscoverFilterBase
    {
        protected static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected AveDiscoverFilterBase() { }

        protected AveDiscoverFilterBase(AveDiscoverFilterBase filter)
        {
            FilterPolicies = filter.FilterPolicies;
            FilterEngine = filter.FilterEngine;
            FilterExpressions = filter.FilterExpressions;
            FilterMode = filter.FilterMode;
        }

        #region For Filter Policy

        public List<FilterPolicy> FilterPolicies { get; private set; }
        public Dictionary<PolicyLevel, string> FilterExpressions { get; private set; }
        public FilterEngine FilterEngine { get; private set; }
        internal FilterResultMode FilterMode { get; set; }
        private Nullable<bool> isQualified = null;

        public bool HasFilter
        {
            get
            {
                return FilterEngine != null && FilterPolicies != null;
            }
        }
        /// <summary>
        /// 验证当前对象是否是filter对象
        /// </summary>
        /// <param name="aveObject">当前对象的IAve对象，如果为null内部会自动创建</param>
        /// <returns></returns>
        public virtual bool IsQualified()
        {
            if (!isQualified.HasValue)
            {
                if (FilterEngine == null)
                {
                    isQualified = true;
                }
                else
                {
                    isQualified = this.FilterEngine.IsQualified(GetFilterObjectInfo(this.FilterPolicies));
                }
            }
            return isQualified.Value;
        }

        public FilterResultMode ResultMode
        {
            get
            {
                if (this.FilterEngine == null)
                {
                    throw new AveException("Current filter is not exist.");
                }
                else
                {
                    return this.FilterMode;
                }
            }
            set
            {
                this.FilterMode = value;
            }
        }

        public void SetFilterResultMode(FilterResultMode filterMode)
        {
            this.FilterMode = filterMode;
        }

        public void SetFilter(List<FilterPolicy> policies, Dictionary<PolicyLevel, string> expressions, FilterResultMode resultMode = FilterResultMode.All)
        {
            FilterEngine = new FilterEngine(policies, expressions);
            FilterPolicies = policies;
            FilterMode = resultMode;
            FilterExpressions = expressions;
            isQualified = null;
        }

        /// <summary>
        /// For Archiver\Extender
        /// </summary>
        public abstract ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies);

        protected bool HasFilterWithLevel(PolicyLevel level)
        {
            if (FilterPolicies != null)
            {
                return FilterPolicies.Exists(filter => (filter.Level & level) != PolicyLevel.None);
            }
            return false;
        }

        #endregion
    }
}
