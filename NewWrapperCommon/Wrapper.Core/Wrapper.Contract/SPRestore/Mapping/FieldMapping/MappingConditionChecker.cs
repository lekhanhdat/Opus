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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    #region 用于判断filter column condition
    class MappingConditionChecker
    {
        protected MappingCondition conditionRule;

        public MappingConditionChecker(MappingCondition conditionRule)
        {
            this.conditionRule = conditionRule;
        }

        internal virtual bool IsQualified(MappingConditionInfo conditionInfo)
        {
            return true;
        }
    }

    class ListLevelMappingConditionChecker : MappingConditionChecker
    {
        public ListLevelMappingConditionChecker(MappingCondition conditionRule) : base(conditionRule) { }

        internal override bool IsQualified(MappingConditionInfo conditionInfo)
        {
            var listConditionInfo = conditionInfo as ListMappingConditionInfo;
            if (listConditionInfo == null)
            {
                throw new ArgumentException("conditionInfo is not type of ListMappingConditionInfo", "conditionInfo");
            }
            return IsWebLevelQualified(listConditionInfo) && IsListLevelQualified(listConditionInfo);
        }

        private bool IsWebLevelQualified(ListMappingConditionInfo conditionInfo)
        {
            bool result = true;
            if (this.conditionRule.SiteCondition.Count > 0)
            {
                ConditionRelation relation = ConditionRelation.And;
                //todo:oliver and/or优先级，是否支持括号
                foreach (var conditionEntry in this.conditionRule.SiteCondition)
                {
                    //list级别不需要考虑site content type rule
                    if (conditionEntry.ConditionType == ConditionType.SiteContentType)
                    {
                        continue;
                    }
                    if (relation == ConditionRelation.And)
                    {
                        result = result && conditionEntry.IsQualified(conditionInfo);
                    }
                    else
                    {
                        result = result || conditionEntry.IsQualified(conditionInfo);
                    }
                    relation = conditionEntry.Relation;
                }
            }
            return result;
        }

        private bool IsListLevelQualified(ListMappingConditionInfo conditionInfo)
        {
            bool result = true;
            if (this.conditionRule.ListCondition.Count > 0)
            {
                ConditionRelation relation = ConditionRelation.And;
                //todo:oliver and/or优先级，是否支持括号
                foreach (var conditionEntry in this.conditionRule.ListCondition)
                {
                    if (relation == ConditionRelation.And)
                    {
                        result = result && conditionEntry.IsQualified(conditionInfo);
                    }
                    else
                    {
                        result = result || conditionEntry.IsQualified(conditionInfo);
                    }
                    relation = conditionEntry.Relation;
                }
            }
            return result;
        }
    }

    class WebLevelMappingConditionChecker : MappingConditionChecker
    {
        public WebLevelMappingConditionChecker(MappingCondition conditionRule) : base(conditionRule) { }

        internal override bool IsQualified(MappingConditionInfo conditionInfo)
        {
            var webConditionInfo = conditionInfo as WeMappingConditionInfo;
            if (webConditionInfo == null)
            {
                throw new ArgumentException("conditionInfo is not type of WebMappingConditionInfo", "conditionInfo");
            }
            //todo:oliver是否需要判断list和item级别的rule
            return this.conditionRule.ListCondition.Count == 0 && this.conditionRule.ItemCondition.Count == 0 &&
                IsQualifiedInternal(webConditionInfo);
        }
        private bool IsQualifiedInternal(WeMappingConditionInfo conditionInfo)
        {
            bool result = true;
            if (this.conditionRule.SiteCondition.Count > 0)
            {
                ConditionRelation relation = ConditionRelation.And;
                //todo:oliver and/or优先级，是否支持括号
                foreach (var conditionEntry in this.conditionRule.SiteCondition)
                {
                    if (relation == ConditionRelation.And)
                    {
                        result = result && conditionEntry.IsQualified(conditionInfo);
                    }
                    else
                    {
                        result = result || conditionEntry.IsQualified(conditionInfo);
                    }
                    relation = conditionEntry.Relation;
                }
            }
            return result;
        }
    }

    static class MappingConditionEntryExtension
    {
        internal static bool IsQualified(this MappingConditionEntry condition, WeMappingConditionInfo info)
        {
            switch (condition.ConditionType)
            {
                case ConditionType.URL:
                    return CheckWebURLCondition(condition, info.WebUrl);
                case ConditionType.SiteContentType:
                    return CheckContentTypeCondition(condition, info.WebContentTypes, info.FieldId);
                case ConditionType.TemplateID:
                case ConditionType.ListTitle:
                case ConditionType.ListContentType:
                default:
                    //List级别的rule对于web上的field不需要检查，全部认为是true
                    //todo:oliver 国际化
                    throw new InvalidOperationException();
            }
        }

        private static bool CheckContentTypeCondition(MappingConditionEntry condition, Dictionary<string, List<Guid>> cts, Guid fieldId)
        {
            return MappingConditionHelper.CheckContentTypeCondtion(cts, fieldId, condition.ConditionValue, condition.Operation);
        }

        private static bool CheckWebURLCondition(MappingConditionEntry condition, string webUrl)
        {
            return MappingConditionHelper.CheckStringCondtion(webUrl, condition.ConditionValue, condition.Operation);
        }

        internal static bool IsQualified(this MappingConditionEntry condition, ListMappingConditionInfo info)
        {
            switch (condition.ConditionType)
            {
                case ConditionType.URL:
                    return CheckWebURLCondition(condition, info.WebUrl);
                case ConditionType.TemplateID:
                    return CheckListTemplateID(condition, info.ListTemplateID);
                case ConditionType.ListTitle:
                    return CheckListTitleCondition(condition, info.ListTitle);
                case ConditionType.ListContentType:
                    return CheckContentTypeCondition(condition, info.ListContentTypes, info.FieldId);

                case ConditionType.SiteContentType:
                default:
                    //List级别的rule对于web上的field不需要检查，全部认为是true
                    //todo:oliver 国际化
                    throw new InvalidOperationException();
            }
        }

        private static bool CheckListTemplateID(MappingConditionEntry condition, string listTemplateID)
        {
            return MappingConditionHelper.CheckStringCondtion(listTemplateID, condition.ConditionValue, condition.Operation);
        }

        private static bool CheckListTitleCondition(MappingConditionEntry condition, string listTitle)
        {
            return MappingConditionHelper.CheckStringCondtion(listTitle, condition.ConditionValue, condition.Operation);
        }


    }

    static class MappingConditionHelper
    {
        static readonly List<Guid> emptyList = new List<Guid>();

        internal static bool CheckStringCondtion(string value, string conditionValue, ConditionOperation operation)
        {
            //string值为null认为不合法，暂时返回false
            if (value == null || conditionValue == null)
            {
                return false;
            }
            switch (operation)
            {
                case ConditionOperation.Equal:
                    return string.Equals(value, conditionValue, StringComparison.OrdinalIgnoreCase);
                case ConditionOperation.NotEqual:
                    return !string.Equals(value, conditionValue, StringComparison.OrdinalIgnoreCase);
                case ConditionOperation.Contains:
                    return value.ToUpper(CultureInfo.InvariantCulture).Contains(conditionValue.ToUpper(CultureInfo.InvariantCulture));
                case ConditionOperation.DoesNotContain:
                    return !value.ToUpper(CultureInfo.InvariantCulture).Contains(conditionValue.ToUpper(CultureInfo.InvariantCulture));
            }
            return false;
        }

        internal static bool CheckContentTypeCondtion(Dictionary<string, List<Guid>> contenttypeNames, Guid fieldId, string name, ConditionOperation operation)
        {
            //todo:oliver fieldid==guid.empty的case
            if (fieldId == Guid.Empty)
            {
                return true;
            }
            return GetQualifiedFieldIds(name, contenttypeNames, operation).Contains(fieldId);
        }

        static List<Guid> GetQualifiedFieldIds(string ctName, Dictionary<string, List<Guid>> contenttypeNames, ConditionOperation operation)
        {
            //todo:oliver 忽略大小写
            //todo:oliver 效率，Dictionary<ctname, List<fieldId>>  -> Dictionary<fieldID, List<ctname>>?
            //可读性     
            switch (operation)
            {
                case ConditionOperation.Equal:
                    return contenttypeNames.ContainsKey(ctName) ? contenttypeNames[ctName] : emptyList;
                case ConditionOperation.NotEqual:
                    return contenttypeNames.Where(kv => !string.Equals(kv.Key, ctName)).SelectMany(kv => kv.Value).Distinct().ToList();
                case ConditionOperation.Contains:
                    return contenttypeNames.Where(kv => kv.Key.Contains(ctName)).SelectMany(kv => kv.Value).Distinct().ToList();
                case ConditionOperation.DoesNotContain:
                    return contenttypeNames.Where(kv => !kv.Key.Contains(ctName)).SelectMany(kv => kv.Value).Distinct().ToList();
            }
            return emptyList;
        }
    }
    #endregion

}
