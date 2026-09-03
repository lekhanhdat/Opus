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
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Myhub.Views
{
    public class RMMyhubFolderDetailMethod
    {
        public (string sql, List<SqlParameter> parameter) GetFolderDetail(Guid Id)
        {
            var sql = BaseSelectSql();
            var parameter = BaseSqlParameters(Id);
            return (sql, parameter);
        }
        private static string BaseSelectSql()
        {
            return @"SELECT VALUE {
    ""Id"": c.nodeId,
    ""NodeId"":c.nodeId,
    ""Name"": c.leafName,
    ""Path"": CONCAT(c.dirPath, '\\' ,c.leafName),
    ""PartitionKeyId"": c.l2PartitionKey,
    ""ClassCode"": c.classCode,
	""PendingDisposal"": c.manual_approvedStatus,
    ""IsRootFolder"":c.scopeId=c.id
}
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.recordStatus = @statuses
AND c.nodeId = @Id
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }

        private static List<SqlParameter> BaseSqlParameters(Guid Id)
        {
            return
            [
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@Id", Id)
            ];
        }
    }
}
