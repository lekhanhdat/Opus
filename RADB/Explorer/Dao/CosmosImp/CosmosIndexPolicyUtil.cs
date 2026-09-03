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
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp
{
    public class CosmosIndexPolicyUtil
    {
        /// <summary>
        /// Get the custom column index policy based on column type
        /// </summary>
        /// <param name="columnType"></param>
        /// <param name="columnId"></param>
        /// <returns>null or a string represents the index policy path</returns>
        public static string GetDynamicCustomColumnIndexPolicyPath(AvePoint.RA.Contract.TemplateManagement.ColumnType columnType, Guid columnId)
        {
            var columnName = string.Empty;
            switch (columnType)
            {
                case Contract.TemplateManagement.ColumnType.MultipleChoice:
                case Contract.TemplateManagement.ColumnType.PeopleOrGroup:
                case Contract.TemplateManagement.ColumnType.MultipleText:
                    break;
                case Contract.TemplateManagement.ColumnType.SingleText:
                    columnName = CosmosConst.C_CustomColumnsValue;
                    break;
                case Contract.TemplateManagement.ColumnType.Number:
                    columnName = CosmosConst.C_CustomColumnsNumber;
                    break;
                case Contract.TemplateManagement.ColumnType.DateTime:
                    columnName = CosmosConst.C_CustomColumnsDate;
                    break;
                default:
                    columnName = CosmosConst.C_CustomColumnsName;
                    break;
            }

            return !string.IsNullOrEmpty(columnName)? $"/{CosmosConst.C_CustomColumnsDic}/'{columnId.ToString().ToLower()}'/{columnName}/?": null;

        }

        /// <summary>
        /// get the index path for built in column of physical records.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBuiltinPhysicalColumnIndexPolicyPath()
        {
            return new List<string>
            {
                $"/{CosmosConst.C_CustomColumnsDic}/'{DefaultColumnIDs.Capability}'/{CosmosConst.C_CustomColumnsNumber}/?", //size
                $"/{CosmosConst.C_CustomColumnsDic}/'{DefaultColumnIDs.Format}'/{CosmosConst.C_CustomColumnsName}/?",
                $"/{CosmosConst.C_CustomColumnsDic}/'{DefaultColumnIDs.ProtectiveMarking}'/{CosmosConst.C_CustomColumnsName}/?",
                //$"/{CosmosConst.C_CustomColumnsDic}/'{DefaultColumnIDs.Status}'/{CosmosConst.C_CustomColumnsName}/?",
                $"/{CosmosConst.C_CustomColumnsDic}/'{DefaultColumnIDs.DateClosed}'/{CosmosConst.C_CustomColumnsDate}/?",
            };

        }

        /// <summary>
        /// get default physical record columns that can be sorted.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDefaultColumnsCanSort()
        {
            return new List<string>
            { 
                DefaultColumnIDs.Capability,
                DefaultColumnIDs.Format,
                DefaultColumnIDs.ProtectiveMarking,
                DefaultColumnIDs.DateClosed
            };

        }
    }
}
