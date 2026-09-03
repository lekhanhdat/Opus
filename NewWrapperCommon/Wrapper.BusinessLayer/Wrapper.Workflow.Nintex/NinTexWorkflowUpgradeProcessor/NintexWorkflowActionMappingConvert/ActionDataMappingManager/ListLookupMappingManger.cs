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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LS.SPWorkflowProcessor
{
    internal class ListLookupMappingManger
    {
        public const string CURRENTLIST = "[Current Item]";
        /// <summary>
        /// 源端list lookup 关联list 如果是当前workflow 关联的task或者history list 的话，workflow template中记录的并不是list id，而是以下两个值
        /// </summary>
        private const string HISTORYLIST = "__historylist";
        private const string TASKLIST = "__tasklist";
        private INintexDataMappingManager mappingManager;
        private NWListLookupCacheManager listLookupCacheManager;
        private string taskListId;
        private string historyListId;
        public ListLookupMappingManger(NWListLookupCacheManager listLookupCacheManager, INintexDataMappingManager mappingManager, string taskListId, string historyListId)
        {
            this.listLookupCacheManager = listLookupCacheManager;
            this.mappingManager = mappingManager;
            this.historyListId = historyListId;
            this.taskListId = taskListId;
        }

        /// <summary>
        /// internal name, field title, field type
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="fieldInternalNames"></param>
        /// <returns></returns>
        private FieldInfo GetFieldInfo(IAveList list, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return new FieldInfo();
            }
            var name = fieldName;
            if (WrapperConfiguration.workflowIneternalNameMapping.ContainsKey(fieldName))
            {
                name = WrapperConfiguration.workflowIneternalNameMapping[fieldName];
            }

            var tempField = list.Fields.GetFieldByInternalName(name);

            string type = GetFieldType(tempField);
            return new FieldInfo { InternalName = tempField.InternalName, Title = tempField.Title, Type = type };
        }

        private string GetFieldType(IAveField field)
        {
            string sourceType = field.TypeAsString;
            if (field is IAveFieldCalculated)
            {
                sourceType = (field as IAveFieldCalculated).OutputType.ToString();
            }
            return NWFieldTypeMapping.ConvertFieldType(sourceType);
        }

        private void SetListLookupFieldInfo(ListLookup listLookuo, FieldInfo selectFieldInfo, FieldInfo whereFieldInfo)
        {
            if (selectFieldInfo != null)
            {
                listLookuo.SelectField = selectFieldInfo.InternalName;
                listLookuo.SelectFieldType = selectFieldInfo.Type;
                listLookuo.DisplayValue = selectFieldInfo.Title;
            }
            if (whereFieldInfo != null)
            {
                listLookuo.WhereField = whereFieldInfo.InternalName;
                listLookuo.WhereFieldType = whereFieldInfo.Type;
            }
        }

        private IAveList GetSelectList(ListLookup valueLookup)
        {
            if (Validator.IsGuid(valueLookup.SelectList))
            {
                Guid destListId = mappingManager.GetListIdFromMapping(valueLookup.SelectList);
                return mappingManager.GetParentWeb().Lists.GetListById(destListId, true);
            }
            if (string.Equals(valueLookup.SelectList, HISTORYLIST, StringComparison.OrdinalIgnoreCase))
            {
                return mappingManager.GetParentWeb().Lists.GetListByName(historyListId, true);
            }
            else if (string.Equals(valueLookup.SelectList, TASKLIST, StringComparison.OrdinalIgnoreCase))
            {
                return mappingManager.GetParentWeb().Lists.GetListByName(taskListId, true);
            }
            else //valueLookup.SelectList is list title in query list action
            {
                string destListTitle = mappingManager.GetListTitleFromMapping(valueLookup.SelectList);
                return mappingManager.GetParentWeb().Lists.GetListByName(destListTitle, true);
            }
            throw new NWListNotFoundException(valueLookup.SelectList);
        }

        public void MappingListLookupData(ListLookup valueLookup)
        {
            if (valueLookup.SelectList.Equals(CURRENTLIST, StringComparison.OrdinalIgnoreCase))
            {
                var fieldInfo = GetFieldInfo(mappingManager.GetParentList(), valueLookup.SelectField);
                SetListLookupFieldInfo(valueLookup, fieldInfo, null);
            }
            else
            {
                var list = GetSelectList(valueLookup);
                var selectFieldInfo = GetFieldInfo(list, valueLookup.SelectField);
                var whereFieldInfo = GetFieldInfo(list, valueLookup.WhereField);
                valueLookup.SelectList = list.ID.ToString();
                valueLookup.DisplayName = list.Title;

                SetListLookupFieldInfo(valueLookup, selectFieldInfo, whereFieldInfo);
            }
            if (valueLookup.WhereValue != null)
            {
                if (valueLookup.WhereValue.ListLookup != null)
                {
                    MappingListLookupData(valueLookup.WhereValue.ListLookup);
                }
                if (valueLookup.WhereValue.PrimitiveValue != null)
                {
                    MappingPrimitiveValueListLookupData(valueLookup.WhereValue.PrimitiveValue);
                }
            }
            AddListLookup(valueLookup);
        }

        public void MappingPrimitiveValueListLookupData(PrimitiveValue primitiveValue)
        {
            if (primitiveValue.FormatValues != null)
            {
                foreach (var formatValue in primitiveValue.FormatValues)
                {
                    if (formatValue.SelectedValue != null)
                    {
                        if (formatValue.SelectedValue.ListLookup != null)
                        {
                            MappingListLookupData(formatValue.SelectedValue.ListLookup);
                        }

                        if (formatValue.SelectedValue.PrimitiveValue != null)
                        {
                            MappingPrimitiveValueListLookupData(formatValue.SelectedValue.PrimitiveValue);
                        }
                    }
                }
            }
        }

        private void AddListLookup(ListLookup listLookup)
        {
            if (listLookup != null)
            {
                var listID = string.Equals(listLookup.SelectList, CURRENTLIST, StringComparison.OrdinalIgnoreCase)
                    ? mappingManager.GetParentList().ID : new Guid(listLookup.SelectList);

                if (!string.IsNullOrEmpty(listLookup.SelectField))
                {
                    listLookupCacheManager.AddField(listID, listLookup.SelectField);
                }

                if (!string.IsNullOrEmpty(listLookup.WhereField))
                {
                    listLookupCacheManager.AddField(listID, listLookup.WhereField);
                }

                if (listLookup.WhereValue != null)
                {
                    AddListLookup(listLookup.WhereValue.ListLookup);
                }
            }
        }
        /// <summary>
        /// 内部类 用于记录field信息
        /// </summary>
        class FieldInfo
        {
            public string InternalName { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}
