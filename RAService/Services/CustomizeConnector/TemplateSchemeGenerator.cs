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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector
{
    public class TemplateSchemeGenerator
    {
        public static string GenerateJson(RMCustomizeConnectorContentSource contentSourceInfo)
        {
            var connectorInfo = CustomizeConnectorConvertor.Convert(contentSourceInfo);

            var scheme = new ExpandoObject();
            scheme.AddOrReplaceInternal("id", connectorInfo.Id);
            scheme.AddOrReplaceInternal("conflictOption", CustomizeConnectorConflictOption.Overwrite.ToString());
            var dataList = new List<ExpandoObject>();
            scheme.AddOrReplaceInternal("data", dataList);
            var data = new ExpandoObject();
            dataList.Add(data);
            var columns = connectorInfo.ColumnInfoes.OrderBy(item => item.Order);
            foreach (var column in columns)
            {
                data.AddOrReplaceInternal(column.InternalName, column.Type.ToString());
            }

            return JsonSerializer.Serialize(scheme, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
        }
    }
}
