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
using AvePoint.RA.Contract.RMWeb.Explorer;
using System;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder.Extension
{
    public static class ExplorerQueryColumnExtension
    {
        /// <summary>
        /// get formated column name.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetColumnName(this ExplorerQueryColumn column)
        {
            if (!column.IsCustomColumn()) return column.Name.FormatColumnName();
            return column.GetOrderByCustomColumnName();
        }

        public static bool IsCustomColumn(this ExplorerQueryColumn column)
        {
            return !string.IsNullOrEmpty(column.Id) && column.Type.HasValue;
        }

        public  static string GetCustomColumnDictionaryName(this ExplorerQueryColumn column)
        {
            return $"{CosmosConst.C_CustomColumnsDic.FormatColumnName()}{column.Id.FormatColumnName()}";
        }
        public static string GetCustomColumnDictionaryName(this ExplorerQueryColumn column, Guid id)
        {
            return $"{CosmosConst.C_CustomColumnsDic.FormatColumnName()}{id.ToString().FormatColumnName()}";
        }
        public static string GetCustomColumnName_Value(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsValue.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Number(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsNumber.FormatColumnName()}";
        }

        public static string GetCustomColumnName_YesOrNo(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsYesOrNo.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Name(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsName.FormatColumnName()}";
        }

        public static string GetCustomColumnName_ValueArray(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsValueArray.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Date(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsDate.FormatColumnName()}";
        }

        public static string GetCustomColumnName_MultipleChoice(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsMultiChoice.FormatColumnName()}";
        }

        public static string GetCustomColumnName_PeopleOrGroup(this ExplorerQueryColumn column)
        {
            return $"{column.GetCustomColumnDictionaryName()}{CosmosConst.C_CustomColumnsUsers.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Date(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsDate.FormatColumnName()}";
        }
        public static string GetCustomColumnName_Value(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsValue.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Number(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsNumber.FormatColumnName()}";
        }

        public static string GetCustomColumnName_YesOrNo(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsYesOrNo.FormatColumnName()}";
        }

        public static string GetCustomColumnName_Name(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsName.FormatColumnName()}";
        }

        public static string GetCustomColumnName_ValueArray(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsValueArray.FormatColumnName()}";
        }
         
        public static string GetCustomColumnName_MultipleChoice(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsMultiChoice.FormatColumnName()}";
        }

        public static string GetCustomColumnName_PeopleOrGroup(this ExplorerQueryColumn column, Guid id)
        {
            return $"{column.GetCustomColumnDictionaryName(id)}{CosmosConst.C_CustomColumnsUsers.FormatColumnName()}";
        }
        public static string GetOrderByCustomColumnName(this ExplorerQueryColumn column)
        {
            switch (column.Type)
            {
                case Contract.TemplateManagement.ColumnType.DateTime:
                    return column.GetCustomColumnName_Date();

                //case Contract.TemplateManagement.ColumnType.MultipleChoice:
                //    return column.GetCustomColumnName_MultipleChoice();

                //case Contract.TemplateManagement.ColumnType.PeopleOrGroup:
                //    return column.GetCustomColumnName_PeopleOrGroup();

                case Contract.TemplateManagement.ColumnType.SingleChoice:
                    return column.GetCustomColumnName_Name();

                case Contract.TemplateManagement.ColumnType.Number:
                    return column.GetCustomColumnName_Number();

                case Contract.TemplateManagement.ColumnType.SingleText:
                case Contract.TemplateManagement.ColumnType.MultipleText:
                    return column.GetCustomColumnName_Value();
                case Contract.TemplateManagement.ColumnType.YesOrNo:
                    return column.GetCustomColumnName_YesOrNo();
                default:
                    break;
            }

            return string.Empty;
        }
    }
}
