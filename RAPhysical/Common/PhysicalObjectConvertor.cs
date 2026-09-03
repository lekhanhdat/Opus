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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Discover;
using AvePoint.RA.RAPhysical.Discover.DiscoverImps;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.RMWeb;
using System.Globalization;

namespace AvePoint.RA.RAPhysical.Common
{
    public class PhysicalObjectConvertor
    {
        private static readonly List<string> cultures = new List<string> { "en-US", "ja-JP", "zh-CN", "ko-KR", "fr-FR", "fr-CA" };
        private static IPhysicalDiscover _physicalfullDiscover = new PhysicalFullDiscover();
        public static ObjectInfoBase ConvertPhysicalBoxFilterObject(List<FilterPolicy> policies, IPhysicalBox xObj, Dictionary<Guid, TemplateColumnDto> columnCollection, Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn)
        {
            PhysicalBoxInfo physicalBoxInfo = new PhysicalBoxInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.PhysicalBox);
            foreach (var policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                switch (ruleName)
                {
                    case "NameRule":
                        physicalBoxInfo.Name = xObj.Name;
                        break;
                    case "CreatedByRule":
                        physicalBoxInfo.CreatedByTitle = xObj.CreateBy.Trim();
                        physicalBoxInfo.CreateByEmail = xObj.CreateBy.Trim();
                        physicalBoxInfo.CreatedByLogonName = xObj.CreateBy.Trim();
                        break;
                    case "ModifiedByRule":
                        physicalBoxInfo.ModifiedByTitle = xObj.ModifiedBy.Trim();
                        physicalBoxInfo.ModifiedByEmail = xObj.ModifiedBy.Trim();
                        physicalBoxInfo.ModifiedByLogonName = xObj.ModifiedBy.Trim();
                        break;
                    case "CreatedRule":
                        physicalBoxInfo.Created = new DateTime(xObj.CreateTimeTicks);
                        break;
                    case "ModifiedRule":
                        physicalBoxInfo.Modified = new DateTime(xObj.ModifiedTimeTicks);
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (physicalBoxInfo.ColumnInfos == null || physicalBoxInfo.ColumnInfos.Count == 0)
                        {
                            physicalBoxInfo.ColumnInfos = GetColumns(xObj, columnCollection, columnIdAndPushColumn);
                        }
                        break;
                    case "LastestFolderDisposalDueDateRule":
                        physicalBoxInfo.LastestFolderDisposalDueDate = new DateTime(GetLastestFolderDisposalDueDateRuleUnderBox(xObj));
                        break;
                    default:
                        throw new Exception($"Do not support the criteria : {ruleName}.");
                }

            }
            if (policies != null && policies.Count > 0)
            {
                if (physicalBoxInfo.ColumnInfos == null) { physicalBoxInfo.ColumnInfos = new Hashtable(StringComparer.OrdinalIgnoreCase); }
                physicalBoxInfo.ColumnInfos.Add(PhysicalRuleEngine.MoveToDestUrlKey, xObj.LocationId.ToString());
            }
            return physicalBoxInfo;
        }

        public static ObjectInfoBase ConvertPhysicalBoxFilterObject(List<FilterPolicy> policies, Record record, Dictionary<Guid, TemplateColumnDto> columnCollection)
        {
            PhysicalBoxInfo physicalBoxInfo = new PhysicalBoxInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.PhysicalBox);
            foreach (var policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                switch (ruleName)
                {
                    case "NameRule":
                        physicalBoxInfo.Name = record.LeafName;
                        break;
                    case "CreatedByRule":
                        physicalBoxInfo.CreatedByTitle = record.CreatedBy.Trim();
                        physicalBoxInfo.CreateByEmail = record.CreatedBy.Trim();
                        physicalBoxInfo.CreatedByLogonName = record.CreatedBy.Trim();
                        break;
                    case "ModifiedByRule":
                        physicalBoxInfo.ModifiedByTitle = record.ModifiedBy.Trim();
                        physicalBoxInfo.ModifiedByEmail = record.ModifiedBy.Trim();
                        physicalBoxInfo.ModifiedByLogonName = record.ModifiedBy.Trim();
                        break;
                    case "CreatedRule":
                        physicalBoxInfo.Created = new DateTime(record.TimeCreated);
                        break;
                    case "ModifiedRule":
                        physicalBoxInfo.Modified = new DateTime(record.TimeModified);
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (physicalBoxInfo.ColumnInfos == null || physicalBoxInfo.ColumnInfos.Count == 0)
                        {
                            physicalBoxInfo.ColumnInfos = GetColumns(record, columnCollection);
                        }
                        break;
                    case "LastestFolderDisposalDueDateRule":
                        physicalBoxInfo.LastestFolderDisposalDueDate = new DateTime(GetLastestFolderDisposalDueDateRuleUnderBox(new PhysicalBox(record)));
                        break;
                    default:
                        throw new Exception($"Do not support the criteria : {ruleName}.");
                }

            }
            if (policies != null && policies.Count > 0)
            {
                if (physicalBoxInfo.ColumnInfos == null) { physicalBoxInfo.ColumnInfos = new Hashtable(StringComparer.OrdinalIgnoreCase); }
                physicalBoxInfo.ColumnInfos.Add(PhysicalRuleEngine.MoveToDestUrlKey, record.LocationId.ToString());
            }
            return physicalBoxInfo;
        }

        public static ObjectInfoBase ConvertPhysicalFileFilterObject(List<FilterPolicy> policies, IPhysicalFile xObj, Dictionary<Guid, TemplateColumnDto> columnCollection, Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn)
        {
            PhysicalFileInfo physicalFileInfo = new PhysicalFileInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.PhysicalFile);
            foreach (var policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                switch (ruleName)
                {
                    case "NameRule":
                        physicalFileInfo.Name = xObj.Name;
                        break;
                    case "CreatedByRule":
                        physicalFileInfo.CreatedByTitle = xObj.CreateBy.Trim();
                        physicalFileInfo.CreateByEmail = xObj.CreateBy.Trim();
                        physicalFileInfo.CreatedByLogonName = xObj.CreateBy.Trim();
                        break;
                    case "ModifiedByRule":
                        physicalFileInfo.ModifiedByTitle = xObj.ModifiedBy.Trim();
                        physicalFileInfo.ModifiedByEmail = xObj.ModifiedBy.Trim();
                        physicalFileInfo.ModifiedByLogonName = xObj.ModifiedBy.Trim();
                        break;
                    case "CreatedRule":
                        physicalFileInfo.Created = new DateTime(xObj.CreateTimeTicks);
                        break;
                    case "ModifiedRule":
                        physicalFileInfo.Modified = new DateTime(xObj.ModifiedTimeTicks);
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (physicalFileInfo.ColumnInfos == null || physicalFileInfo.ColumnInfos.Count == 0)
                        {
                            physicalFileInfo.ColumnInfos = GetColumns(xObj, columnCollection, columnIdAndPushColumn);
                        }
                        break;
                    default:
                        throw new Exception($"Do not support the criteria : {ruleName}.");
                }
            }
            if (policies != null && policies.Count > 0)
            {
                if (physicalFileInfo.ColumnInfos == null) { physicalFileInfo.ColumnInfos = new Hashtable(StringComparer.OrdinalIgnoreCase); }
                //File 可能在box 也可能在location下，所以需要下面的判断，决定文件parent id
                physicalFileInfo.ColumnInfos.Add(PhysicalRuleEngine.MoveToDestUrlKey, xObj.BoxId == Guid.Empty ? xObj.LocationId.ToString() : xObj.BoxId.ToString());
            }
            return physicalFileInfo;
        }

        public static ObjectInfoBase ConvertPhysicalFileFilterObject(List<FilterPolicy> policies, Record record, Dictionary<Guid, TemplateColumnDto> columnCollection)
        {
            PhysicalFileInfo physicalFileInfo = new PhysicalFileInfo();
            policies = CreateDistinctFiltersCopy(policies, PolicyLevel.PhysicalFile);
            foreach (var policy in policies)
            {
                string ruleName = policy.Rule.GetType().Name;
                switch (ruleName)
                {
                    case "NameRule":
                        physicalFileInfo.Name = record.LeafName;
                        break;
                    case "CreatedByRule":
                        physicalFileInfo.CreatedByTitle = record.CreatedBy.Trim();
                        physicalFileInfo.CreateByEmail = record.CreatedBy.Trim();
                        physicalFileInfo.CreatedByLogonName = record.CreatedBy.Trim();
                        break;
                    case "ModifiedByRule":
                        physicalFileInfo.ModifiedByTitle = record.ModifiedBy.Trim();
                        physicalFileInfo.ModifiedByEmail = record.ModifiedBy.Trim();
                        physicalFileInfo.ModifiedByLogonName = record.ModifiedBy.Trim();
                        break;
                    case "CreatedRule":
                        physicalFileInfo.Created = new DateTime(record.TimeCreated);
                        break;
                    case "ModifiedRule":
                        physicalFileInfo.Modified = new DateTime(record.TimeModified);
                        break;
                    case "ColumnTextRule":
                    case "ColumnNumberRule":
                    case "ColumnDateTimeRule":
                    case "ColumnBooleanRule":
                    case "CustomColumnRule":
                        if (physicalFileInfo.ColumnInfos == null || physicalFileInfo.ColumnInfos.Count == 0)
                        {
                            physicalFileInfo.ColumnInfos = GetColumns(record, columnCollection);
                        }
                        break;
                    default:
                        throw new Exception($"Do not support the criteria : {ruleName}.");
                }
            }
            if (policies != null && policies.Count > 0)
            {
                if (physicalFileInfo.ColumnInfos == null) { physicalFileInfo.ColumnInfos = new Hashtable(StringComparer.OrdinalIgnoreCase); }
                //File 可能在box 也可能在location下，所以需要下面的判断，决定文件parent id
                physicalFileInfo.ColumnInfos.Add(PhysicalRuleEngine.MoveToDestUrlKey, record.BoxId == Guid.Empty ? record.LocationId.ToString() : record.BoxId.ToString());
            }
            return physicalFileInfo;
        }

        /// <summary>
        /// 获得该Level每种Filter的不重复的Rule
        /// </summary>
        private static List<FilterPolicy> CreateDistinctFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();
            }
            return new List<FilterPolicy>();
        }
        public static long GetLastestFolderDisposalDueDateRuleUnderBox(IPhysicalBox xObj)
        {
            var folders = _physicalfullDiscover.GetPhysicalFiles(xObj);

            if (!folders.Any() || folders.Any(f => f.DisposalDueDate <= 0))
            {
                return 0;
            }

            return folders.Max(f => f.DisposalDueDate);
        }
        private static Hashtable GetColumns(IPhysicalFields fields, Dictionary<Guid, TemplateColumnDto> columnCollection, Dictionary<Guid, List<RMPhysicalPushColumn>> columnIdAndPushColumn)
        {
            Hashtable result = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if(fields == null || fields.Fields == null)
            {
                return result;
            }
            foreach (var fieldKey in fields.Fields.Keys)
            {
                var columnName = fieldKey;
                var value = fields.Fields[fieldKey];
                object valueObj = value;
                Guid fieldId;
                if (Guid.TryParse(fieldKey, out fieldId))
                {
                    if (columnCollection != null && columnCollection.ContainsKey(fieldId))
                    {
                        var column = columnCollection[fieldId];
                        columnName = column.columnName;
                        //columnName = I18N.Core.I18NEntity.GetString(column.columnName);
                        if (column.pushToChild)
                        {
                            List<RMPhysicalPushColumn> pushColumns = columnIdAndPushColumn[column.uniqueId];
                            if (pushColumns.Count > 0)
                            {
                                valueObj = GetColumnValue(column, pushColumns[0].ColumnValue);
                            }
                        }
                        else
                        {
                            valueObj = GetColumnValue(column, value);
                        }
                    }
                }
                foreach (var culture in cultures)
                {
                    string I18NValue = I18N.Core.I18NEntity.GetString(columnName, CultureInfo.CreateSpecificCulture(culture));
                    if (!result.ContainsKey(I18NValue))
                    {
                        result[I18NValue] = valueObj;
                    }
                }
            }
            return result;
        }

        private static Hashtable GetColumns(Record record, Dictionary<Guid, TemplateColumnDto> columnCollection)
        {
            Hashtable result = new Hashtable(StringComparer.OrdinalIgnoreCase);
            var metaInfo = new Dictionary<string, string>();
            using (new RA.Common.PerformanceScope("PhysicalRecord.RA.DB.Core.ConvertUtil.ConvertRMBaseRecordToPhysical.ReerializeMetaInfo"))
            {
                metaInfo = string.IsNullOrEmpty(record.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
            }
            if (record == null || metaInfo == null)
            {
                return result;
            }
            foreach (var fieldKey in metaInfo.Keys)
            {
                var columnName = fieldKey;
                var value = metaInfo[fieldKey];
                object valueObj = value;
                Guid fieldId;
                if (Guid.TryParse(fieldKey, out fieldId))
                {
                    if (columnCollection != null && columnCollection.ContainsKey(fieldId))
                    {
                        var column = columnCollection[fieldId];
                        columnName = I18N.Core.I18NEntity.GetString(column.columnName);
                        valueObj = GetColumnValue(column, value);
                    }
                }
                result[columnName] = valueObj;
            }
            return result;
        }

        private static object GetColumnValue(TemplateColumnDto column, string value)
        {
            object result = value;
            if (!string.IsNullOrEmpty(value))
            {
                switch (column.typeId)
                {
                    case (int)ColumnType.SingleChoice:
                        {
                            var field = JsonConvert.DeserializeObject<ChoiceColumnValue>(value);
                            result = field.Name;
                            Dictionary<int, string> columnOption = JsonConvert.DeserializeObject<Dictionary<int, string>>(column.optionsJSON);
                            int fieldId;
                            if (int.TryParse(field.Value, out fieldId))
                            {
                                if (columnOption.ContainsKey(fieldId))
                                {
                                    result = columnOption[fieldId];
                                }
                            }
                            break;
                        }
                    case (int)ColumnType.MultipleChoice:
                        {
                            var field = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(value);
                            Dictionary<int, string> columnOption = JsonConvert.DeserializeObject<Dictionary<int, string>>(column.optionsJSON);
                            var columnNameList = new List<string>();
                            field.ForEach(f =>
                            {
                                var columnDisplayValue = f.Name;
                                int fieldId;
                                if (int.TryParse(f.Value, out fieldId))
                                {
                                    if (columnOption.ContainsKey(fieldId))
                                    {
                                        columnDisplayValue = columnOption[fieldId];
                                    }
                                }
                                columnNameList.Add(columnDisplayValue);
                            });
                            result = string.Join(";", columnNameList).Trim(';');
                            break;
                        }
                    case (int)ColumnType.Taxonomy:
                        {
                            var field = JsonConvert.DeserializeObject<TaxonomyColumnValue>(value);
                            result = field.Name;
                            break;
                        }
                    case (int)ColumnType.DateTime:
                        {
                            var field = JsonConvert.DeserializeObject<DateTimeColumnValue>(value);
                            result = field.GetUtcDate();
                            break;
                        }
                    case (int)ColumnType.PeopleOrGroup:
                        {
                            var field = JsonConvert.DeserializeObject<List<PeopleColumnValue>>(value);
                            result = string.Join(";", field.Select(f => f.DisplayName.Trim()).ToList()).Trim(';');
                            break;
                        }
                    case (int)ColumnType.SingleText:
                    case (int)ColumnType.MultipleText:
                    case (int)ColumnType.Number:
                    default:
                        break;

                }
            }
            return result;
        }

    }
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
}
