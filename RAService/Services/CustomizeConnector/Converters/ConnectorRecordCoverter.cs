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
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using System.Dynamic;
using PnP.Core;
using AvePoint.Common.FilterEngine.ObjectInfos.Connector;

namespace AvePoint.RA.Service.Services.CustomizeConnector.Converters
{
    public static class ConnectorRecordCoverter
    {
        public static CustomizeConnectorItemInfo ConvertRecord2DocumentInfo(Record record, Dictionary<string, object> rulePolicyValues)
        {
            return new CustomizeConnectorItemInfo()
            {
                Title = record.LeafName,
                Name = record.LeafName,
                Modified = new DateTime(record.TimeModified, DateTimeKind.Utc),
                Created = new DateTime(record.TimeCreated, DateTimeKind.Utc),
                ModifiedByTitle = string.IsNullOrWhiteSpace(record.ModifiedBy) ? "" : record.ModifiedBy,
                CreatedByTitle = string.IsNullOrWhiteSpace(record.CreatedBy) ? "" : record.CreatedBy,
                ColumnInfos = rulePolicyValues
            };
        }

        public static async Task<ExpandoObject> ConvertRecord2QueryResultAsync(Record record, CustomizeConnectorInfo connectorInfo)
        {
            var res = new ExpandoObject();
            var customizeColumnValue = record.CustomColumnDic;            
            var columnManager = new ConnectorColumnManager(connectorInfo.ColumnInfoes);         
            foreach (var columnInfo in connectorInfo.ColumnInfoes.OrderBy(item => item.Order))
            {               
                if (columnInfo.Origin == CustomizeConnectorOrigin.BuildIn)
                {
                    var nameValue = await ConnectorBuildInColumnManager.ConvertToNameValueAsync(columnInfo, record, false);                  
                    res.AddOrReplaceInternal(columnInfo.InternalName, nameValue.Value);
                }
                else
                {
                    var nameValue = await columnManager.ConvertToNameValueAsync(columnInfo, customizeColumnValue, false);                    
                    res.AddOrReplaceInternal(columnInfo.InternalName, nameValue.Value);
                }
            }
            return res;
        }
    }
}
