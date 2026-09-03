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
using AvePoint.RA.Contract.CustomizeConnector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.CustomizeConnector
{
    public class BuildInColumns
    {
        public static List<CustomizeConnectorColumnInfo> Columns = new()
        {
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("20fcf752-cc89-4081-fc59-c1b8c6ab3475"),
                Name = "RM_Connecor_RowKey",
                InternalName = "rowKey",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.SingleText,
                IsRequired = true,
                IsHidden = true,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("de5e99cb-4fb4-4e25-b732-a1dce71dd048"),
                Name = "RM_PRM_PRE_MRR_Column_NameOrTitle",
                InternalName = "leafName",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.SingleText,
                IsRequired = true,
                IsHidden = false,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("1339e256-9010-cfb2-5a50-bf2d2d00d461"),
                Name = "RM_PRM_PRE_Column_DisposalClass",
                InternalName = "termFullPath",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.Taxonomy,
                IsRequired = false,
                IsHidden = false,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5"),
                Name = "RM_JS_RDM_Explorer_CreateTime",
                InternalName = "timeCreated",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.DateTime,
                IsRequired = true,
                IsHidden = false,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("3ec9a488-90fa-4d62-835f-0df0cd2e9f97"),
                Name = "RM_PRM_PRE_Column_ModifiedTime",
                InternalName = "timeModified",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.DateTime,
                IsRequired = true,
                IsHidden = false,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("91a08d45-c5dd-43da-b6c4-670f11ac273e"),
                Name = "RM_PRM_PRE_Column_Creator",
                InternalName = "createdBy",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.SingleText,
                IsRequired = false,
                IsHidden = false,
                Extention = "",
            },
            new CustomizeConnectorColumnInfo
            {
                Id = new Guid("1f2e8c3f-e49a-473c-bd16-8647258cf15c"),
                Name = "RM_PRM_PRE_Column_Modifier",
                InternalName = "modifiedBy",
                Origin = Enums.CustomizeConnectorOrigin.BuildIn,
                Scope = Enums.CustomizeConnectorColumnScope.Global,
                Type = TemplateManagement.ColumnType.SingleText,
                IsRequired = false,
                IsHidden = false,
                Extention = "",
            },
        };
    }
}
