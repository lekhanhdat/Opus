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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Model.Extension
{
    public static class RMTemplateExtension
    {
        public static List<TemplateColumn4Display> GetColumnList4Display(this RMTemplate template)
        {
            var result = new List<TemplateColumn4Display>();
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
            foreach (ColumnXmlSchema column in schemaTemp.Columns)
            {
                var column4Display = column.Convert2TemplateColumn4Display();
                column4Display.Templates.Add(new NameAndIdDto { Id = template.UniqueId.ToString().ToLower() });
                result.Add(column4Display);
            }
            return result;
        }

        /// <summary>
        /// get the column options json string
        /// </summary>
        /// <param name="template"></param>
        /// <param name="columnUniqueId"></param>
        /// <returns></returns>
        public static string GetColumnOptionsJson(this RMTemplate template, Guid columnUniqueId)
        {
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
            var column = schemaTemp?.Columns.Where(o => columnUniqueId == o.UniqueId).FirstOrDefault();
            
            return column?.OptionsJSON;
        }

		
	}

    public static class ColumnXmlSchemaExtension
    {
        public static TemplateColumn4Display Convert2TemplateColumn4Display(this ColumnXmlSchema column)
        {
            var r = new TemplateColumn4Display
            {
                UniqueId = column.UniqueId,
                ColumnName = column.Name,
                ColumnType = column.ColumnType,
                OptionsJSON = column.OptionsJSON,
                AllowSort = column.AllowSort,
                IdsWithDuplicateName = new List<Guid>() { column.UniqueId }
            };

            if (column.pushFoldTemplateCategoriesId != null && column.pushFoldTemplateCategoriesId.Count() > 0)
            {
                r.Templates.AddRange(column.pushFoldTemplateCategoriesId.Select(o => new NameAndIdDto { Id = o.tempalteId.ToLower() }));
            }

            if (column.pushRecordTemplateCategoriesId != null && column.pushRecordTemplateCategoriesId.Count() > 0)
            {
                r.Templates.AddRange(column.pushRecordTemplateCategoriesId.Select(o => new NameAndIdDto { Id = o.tempalteId.ToLower() }));
            }

            return r;
        }

        /// <summary>
        /// 对于build in的column，不允许在GUI上设置Allow sort选项，返回null;
        /// 其他的column，默认返回false
        /// </summary>
        /// <param name="columnId"></param>
        /// <returns>false or nothing</returns>
        public static bool? DefaultColumnAllowSortValue(Guid columnUniqueId)
        {
            var allIds = DefaultColumnIDs.AllIDs.Select(o => new Guid(o));

            return allIds.Contains(columnUniqueId) ? default(bool?) : false;
        }

        /// <summary>
        /// 是否允许在GUI上设置Allow sort选项
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public static bool AllowEditSort(Guid columnId, ColumnType columnType)
        {
            return ColumnTypeExtension.GetCanSortColumnTypes().Contains(columnType) && !DefaultColumnIDs.AllIDs.Contains(columnId.ToString().ToLower());
        }

        /// <summary>
        /// 是否允许在GUI上设置Allow sort选项
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool AllowEditSort(this ColumnXmlSchema column)
        {
            return AllowEditSort(column.UniqueId, column.ColumnType);
        }
    }

    public static class TemplateColumn4DisplayExtension
    {
		public static Guid GetNameHash(this TemplateColumn4Display templateColumn4Display)
		{

			if (DefaultColumnIDs.AllIDs.Contains(templateColumn4Display.UniqueId.ToString()))
			{
				return templateColumn4Display.UniqueId;
			}
			else
			{
				string temp = templateColumn4Display.ColumnName + AvePoint.RA.Contract.Explorer.ColumnType.GetName(templateColumn4Display.ColumnType.GetType(), templateColumn4Display.ColumnType);
				return AvePoint.GCommon.Utility.HashCodeHelper.StringHash(temp);
			}

		}
	}
}
