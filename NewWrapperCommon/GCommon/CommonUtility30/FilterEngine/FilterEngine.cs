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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CommonFilter;
    using System.Linq;
    #endregion

    /// <summary>
    /// FilterEngine suite唯一公开接口类, 
    /// </summary>
    public class FilterEngine
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(FilterEngine));

        #region Options
        private FilterOption option;

        /// <summary>
        /// FilterOption.FilterPolicies
        /// </summary>
        public List<FilterPolicy> FilterPolicies
        {
            get { return this.option.FilterPolicies; }
        }

        /// <summary>
        /// FilterOption.FilterConditionExpressions
        /// </summary>
        public Dictionary<PolicyLevel, string> FilterConditionExpressions
        {
            get { return this.option.FilterConditionExpressions; }
        }

        //This is for SO modules. We should filter out all the objects if there are no rules on this level.
        /// <summary>
        /// FilterOption.IsNoRuleFilterOut
        /// </summary>
        public bool IsNoRuleFilterOut
        {
            get { return this.option.IsNoRuleFilterOut; }
        }

        //This is for dynamic filter functionality of SP Migration.
        /// <summary>
        /// FilterOption.IsRealFilterOut
        /// </summary>
        public bool IsRealFilterOut
        {
            get { return this.option.IsRealFilterOut; }
        }

        /// <summary>
        /// FilterOption.LogLevel
        /// </summary>
        public RecordFilterPolicyLog LogLevel { get { return option.LogLevel; } }
        #endregion

        #region Constructors
        /// <summary>
        /// 构造一个FilterEngine(据说高富帅的跑车里都会有一个这样的引擎).
        /// </summary>
        /// <param name="policyLists">FilterPolicy类集合. 每一个FilterPolicy(FilterCondition更为恰当)类对应一条Filter Condition, 例如：SiteCollection Url Contains https://</param>
        /// <param name="filterConditionExpressionLists">FilterCondtion表达式集合. 每个Level对应一条表达式(如果该Level存在Filter), 例如：1 and 2</param>
        /// <param name="isNoRuleFilterOut">用于控制, 如果在某个PolicyLevel上没有设置Rule, 如何进行Filter. True: IsQualified()返回false, 否则返回true, 默认为false.</param>
        /// <param name="isRealFilterOut">是否FilterOut. True: FilterOut, 即对于符合条件的节点返回false, False: FilterIn, 即对于符合条件的节点返回true. 默认为false.</param>
        [Obsolete("This constructor is obsolete, new options will not be available, please use public FilterEngine(FilterOption option) instead.",false)]
        public FilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, bool isNoRuleFilterOut = false, bool isRealFilterOut = false)
            : this(new FilterOption()
            {
                FilterPolicies = policyLists,
                FilterConditionExpressions = filterConditionExpressionLists,
                IsNoRuleFilterOut = isNoRuleFilterOut,
                IsRealFilterOut = isRealFilterOut,
            })
        {
        }

        /// <summary>
        /// 构造一个FilterEngine(据说高富帅的跑车里都会有一个这样的引擎).
        /// </summary>
        /// <param name="option">Filter控制选项, 参见FilterOption类</param>
        public FilterEngine(FilterOption option)
        {
            this.option = option;
        }
        #endregion

        #region Methods
        public bool IsFilterExist(PolicyLevel level)
        {
            return this.FilterPolicies.Any(p => p.Level == level);
        }

        /// <summary>
        /// 判断objectInfo中的信息，是否符合Filter条件
        /// 根据objectInfo的类型，初始化对于的FilterEngineBase子类，并调用其IsQualified方法
        /// </summary>
        /// <param name="objectInfo">ObjectInfoBase子类，数据类存放对于数据源中的相应属性。</param>
        /// <returns>true:符合条件, false:不符合条件</returns>
        public bool IsQualified(ObjectInfoBase objectInfo)
        {
            //使用Factory创建IFilterEngine子类, 扩展IFilterEngine子类不需要修改FilterEngine核心类
            using (IFilterEngine filterBase = FilterEngineFactory.GetFilterEngine(this.option, objectInfo.Level))
            {
                try
                {
                    return filterBase.IsQualified(objectInfo);
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, "An error occurred while doing filter. Error: {0}", e.ToString());
                    //为啥要返回false??
                    return false;
                }
            }
        }

        private static void Example()
        {
            FilterPolicy appPolicy1 = new FilterPolicy();
            appPolicy1.SequenceNo = 1;
            appPolicy1.Level = PolicyLevel.WebApplication;
            appPolicy1.Rule = new UrlRule();
            appPolicy1.Condition = PolicyCondition.Contains;
            appPolicy1.Value = new PolicyValue("*demo*");

            FilterPolicy siteCollectionPolicy1 = new FilterPolicy();
            siteCollectionPolicy1.SequenceNo = 2;
            siteCollectionPolicy1.Level = PolicyLevel.SiteCollection;
            siteCollectionPolicy1.Rule = new OwnerRule();
            siteCollectionPolicy1.Condition = PolicyCondition.Contains;
            siteCollectionPolicy1.Value = new PolicyValue("*Lance*");

            FilterPolicy siteCollectionPolicy2 = new FilterPolicy();
            siteCollectionPolicy2.SequenceNo = 3;
            siteCollectionPolicy2.Level = PolicyLevel.SiteCollection;
            siteCollectionPolicy2.Rule = new OwnerRule();
            siteCollectionPolicy2.Condition = PolicyCondition.Contains;
            siteCollectionPolicy2.Value = new PolicyValue("*Lance Lee*");

            List<FilterPolicy> policies = new List<FilterPolicy>();
            policies.Add(appPolicy1);
            policies.Add(siteCollectionPolicy1);
            policies.Add(siteCollectionPolicy2);

            Dictionary<PolicyLevel, string> expressions = new Dictionary<PolicyLevel, string>();
            expressions.Add(PolicyLevel.WebApplication, "(1)");
            expressions.Add(PolicyLevel.SiteCollection, "(2 and 3)");

            FilterEngine engine = new FilterEngine(policies, expressions);

            WebAppInfo appInfo = new WebAppInfo();
            appInfo.Url = "http://sp-demo:8080";
            bool filtered = engine.IsQualified(appInfo);

            SiteCollectionInfo siteCollectionInfo = new SiteCollectionInfo();
            siteCollectionInfo.Url = "http://sp-demo:8080/it";
            siteCollectionInfo.Title = "test";
            siteCollectionInfo.Owner = "Lance Lee";
            siteCollectionInfo.Template = "STS#1";
            siteCollectionInfo.Modified = DateTime.UtcNow.AddDays(-3);
            siteCollectionInfo.Created = DateTime.UtcNow.AddHours(-300);
            filtered = engine.IsQualified(siteCollectionInfo);

            SiteInfo siteInfo = new SiteInfo();
            siteInfo.Url = "http://sp-demo:8080/it/dev1";
            siteInfo.Title = "test";
            siteInfo.CreatedByTitle = "Lance Lee";
            siteInfo.Template = "STS#1";
            siteInfo.Modified = DateTime.UtcNow.AddDays(-3);
            siteInfo.Created = DateTime.UtcNow.AddHours(-300);
            filtered = engine.IsQualified(siteInfo);
        }
        #endregion
    }
}
