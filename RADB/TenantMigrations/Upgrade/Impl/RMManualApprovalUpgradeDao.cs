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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMManualApprovalUpgradeDao : BaseDao<RMManualApprove>, IDbUpgradeDao
    {
        //private RALogger logger = RALogger.GetInstance(typeof(RMManualApprovalUpgradeDao));
        public async Task UpgradeAsync(Core.RMDbContext context)
        {
            var sql = "update {0}.RMManualApproves set SourceFlag = 1 where SourceFlag = 0";
            var newSql = string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
            int row = context.Database.ExecuteSqlCommand(newSql);
        }
    }
}
