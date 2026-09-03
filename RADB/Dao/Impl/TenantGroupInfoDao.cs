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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.System;
using AvePoint.RA.Common.Util;
using System.Data.SqlClient;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TenantGroupInfoDao : ITenantGroupInfoDao
    {

        public bool CheckTenantGroupInfo()
        {
            
            var groupId = WebUtil.TenantId;
            var sql = "select Count(1) from TenantGroup where TenantGroupId = @TenantGroupId";
            return DatabaseUtility.RetryPolicy.ExecuteAction<bool>(() =>
            {
                var idParam = new SqlParameter("TenantGroupId", groupId);
                using (IDatabaseContext ctx = new DatabaseContext())
                {
                    var count = ctx.ExecuteScalar<int>(sql, idParam);
                    return count > 0;
                }
            });
        }

        public int CreateTenantGroup(TenantGroupInfoDto groupInfo)
        {
            var result = -1;
            var sql = "insert into TenantGroupInfo(TenantGroupId, Status, LoginName, DBUser, DBPassword, DBName, SchemaName, SizeQuota) "
                + "values(@TenantGroupId, @LoginName, @DBUser, @DBPassword, @DBName, @SchemaName, @SizeQuota, @Status)";
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                var groupIdParam = new SqlParameter("TenantGroupId", groupInfo.TenantGroupId);
                var loginNameParam = new SqlParameter("LoginName", groupInfo.LoginName);
                var userParam = new SqlParameter("DBUser", groupInfo.DBUser);
                var passwordParam = new SqlParameter("DBPassword", groupInfo.DBPassword);
                var dbNameParam = new SqlParameter("DBName", groupInfo.DBName);
                var dbSchemaParam = new SqlParameter("SchemaName", groupInfo.SchemaName);
                var sizeParam = new SqlParameter("SizeQuota", groupInfo.SizeQuota);
                var statusParam = new SqlParameter("Status", groupInfo.Status);
                using (var ctx = new DatabaseContext())
                {
                    result = ctx.ExecuteNonQuery(sql, groupIdParam, loginNameParam, userParam, passwordParam, dbNameParam, dbSchemaParam, sizeParam, statusParam);
                }
            });

            return result;
        }
    }



}
