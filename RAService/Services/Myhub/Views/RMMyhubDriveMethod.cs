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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MyHub.Items.Views;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.MyHub.NewMethods;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub.Views
{
    public class RMMyhubDriveMethod
    {
        private RMMyhubQueryRecordsMethod _recordStore;
        private RMMyhubQueryRecordsMethod RecordStore => _recordStore ??= new RMMyhubQueryRecordsMethod();
        public async Task<RMMyhubDriveDirectionItem> BaseGetNodeInfoByPartitionKeyAsync(string partitionKeyId)
        {
            var sql = BaseGetDrivesNodeInfoSql();
            var parameters = GetDrivesNodeInfoSqlParameters(partitionKeyId);

            var nodeInfo = await RecordStore.QuerySingleAsync<RMMyhubDriveDirectionItem>(sql, parameters);
            return nodeInfo;
        }
        private string BaseGetDrivesNodeInfoSql()
        {
            return @"SELECT VALUE {
    ""NodeId"":c.nodeId,
    ""FullPath"":CONCAT(c.dirPath,'\\',c.leafName)
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.l2PartitionKey=@l2PartitionKey
AND c.id=c.scopeId
AND c.nodeType = @nodeType
AND c.recordStatus=@statuses
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        public List<SqlParameter> GetDrivesNodeInfoSqlParameters(string partitionKeyId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@nodeType", (int)NodeLevel.FSFolder),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant())
            };
            return sqlParameters;
        }
    }
}
