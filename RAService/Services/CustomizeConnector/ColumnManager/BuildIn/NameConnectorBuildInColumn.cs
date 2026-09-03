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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn
{
    public class NameConnectorBuildInColumn : IConnectorBuildInColumn
    {
        public Guid ColumnId => CustomizeConnectorBuildColumnIds.Name;

        public void ApplyRecordValue(Record record, Dictionary<string, CustomColumn> customColumnDic)
        {
            if (customColumnDic.TryGetValue(ColumnId.ToString(), out var value))
            {
                record.LeafName = value.Value;
                customColumnDic.Remove(ColumnId.ToString());
            }
        }

        public async Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Record record, bool forDisplay = true)
        {
            return new CustomizeConnectorNameValue<string>
            {
                Name = I18NEntity.GetString(columnInfo.Name),
                Value = record.LeafName
            };
        }
    }
}
