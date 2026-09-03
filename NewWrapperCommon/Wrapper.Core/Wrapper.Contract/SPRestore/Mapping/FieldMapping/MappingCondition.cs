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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Linq;
    using System;
    using AvePoint.Wrapper.Core.Util;

    #region Mapping Condition-- Internal use
    /// <summary>
    /// 用于存放并构造Condition信息，对应界面上的Condition(If these conditions are met)部分
    /// </summary>
    class MappingCondition
    {
        internal List<MappingConditionEntry> SiteCondition {get;private set;}
        internal List<MappingConditionEntry> ListCondition { get; private set; }
        internal List<MappingConditionEntry> ItemCondition { get; private set; }

        #region 保证一个MappingCondition对象上最多有两个condition checker
        private object listLevelCheckerLock = new object();
        private object siteLevelCheckerLock = new object();
        private MappingConditionChecker listLevelConditionChecker;
        private MappingConditionChecker siteLevelConditionChecker;

        private MappingConditionChecker GetConditionChecker(MappingConditionInfo conditionInfo)
        {
            if (conditionInfo is WeMappingConditionInfo)
            {
                return GetSiteLevelConditionChecker(this);
            }
            if (conditionInfo is ListMappingConditionInfo)
            {
                return GetListLevelConditionChecker(this);
            }
            throw new ArgumentException("conditionInfo");
        }

        private MappingConditionChecker GetSiteLevelConditionChecker(MappingCondition mappingCondition)
        {
            if (this.siteLevelConditionChecker==null)
            {
                lock (this.siteLevelCheckerLock)
                {
                    if (this.siteLevelConditionChecker == null)
                    {
                        this.siteLevelConditionChecker = new WebLevelMappingConditionChecker(mappingCondition);
                    }
                }
            }
            return this.siteLevelConditionChecker;
        }

        private MappingConditionChecker GetListLevelConditionChecker(MappingCondition mappingCondition)
        {
            if (this.listLevelConditionChecker == null)
            {
                lock (this.listLevelCheckerLock)
                {
                    if (this.listLevelConditionChecker == null)
                    {
                        this.listLevelConditionChecker = new ListLevelMappingConditionChecker(mappingCondition);
                    }
                }
            }
            return this.listLevelConditionChecker;
        }
        #endregion

        public MappingCondition()
        {
            this.SiteCondition = new List<MappingConditionEntry>();
            this.ListCondition = new List<MappingConditionEntry>();
            this.ItemCondition = new List<MappingConditionEntry>();
        }

        internal virtual void Load(XmlElement node)
        {
            //可以没有condition
            if (node == null)
            {
                return;
            }
            XmlNode siteConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "SiteCondition", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            LoadConditions(SiteCondition, siteConditionNode);
            XmlNode listConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ListCondition", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            LoadConditions(ListCondition, listConditionNode);
            XmlNode itemConditionNode = node.Cast<XmlNode>().Where(child => child is XmlElement && string.Equals(child.Name, "ItemCondition", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            LoadConditions(ItemCondition, itemConditionNode);
        }

        internal bool IsQualified(MappingConditionInfo conditionInfo)
        {
            if (conditionInfo == null)
            {
                return true;
            }
            var checker = GetConditionChecker(conditionInfo);
            return checker.IsQualified(conditionInfo);
        }

        private void LoadConditions(List<MappingConditionEntry> conditions, XmlNode conditionsNode)
        {
            if (conditionsNode == null)
            {
                return;
            }
            foreach (XmlNode n in conditionsNode.ChildNodes)
            {
                var element = n as XmlElement;
                if (element == null) { continue; }
                var conditionInfo = new MappingConditionEntry();
                conditionInfo.Load(element);
                conditions.Add(conditionInfo);
            }
        }
    }
    /// <summary>
    /// 一条condition，对应界面Condition(If these conditions are met)中add的一条记录以及他们之间的And\Or关系
    /// </summary>
    class MappingConditionEntry
    {

        internal void Load(XmlElement node)
        {
            ConditionType = (ConditionType)Enum.Parse(typeof(ConditionType), node.GetAttributeEx("type",true), true);
            Operation = (ConditionOperation)Enum.Parse(typeof(ConditionOperation), node.GetAttributeEx("condition", true), true);
            ConditionValue = node.GetAttributeEx("value", true);
            Relation = (ConditionRelation)Enum.Parse(typeof(ConditionRelation), node.GetAttributeEx("relation", true), true);
        }

        public ConditionType ConditionType { get; set; }

        public ConditionRelation Relation { get; set; }

        public string ConditionValue { get; set; }

        public ConditionOperation Operation { get; set; }
    }
    /// <summary>
    /// todo:oliver,枚举定义不合理，考虑和MappingFilterRule一致
    /// </summary>
    enum ConditionType
    {
        URL = 0,
        SiteContentType = 1,
        /// <summary>
        /// List TemplateID 兼容Contract
        /// </summary>
        TemplateID = 2,
        ListTitle = 3,
        ListContentType = 4,
        Name = 5,
        None = 6
    }

    enum ConditionRelation
    {
        None = 0,
        And = 1,
        Or = 2,
    }

    enum ConditionOperation
    {
        None = 0,
        Contains = 1,
        Equal = 2,
        NotEqual = 3,
        DoesNotContain = 4,
    }
    #endregion
}
