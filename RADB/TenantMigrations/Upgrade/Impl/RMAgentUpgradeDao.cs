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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMAgentUpgradeDao: BaseDao<RMAgent>, IDbUpgradeDao
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMAgentUpgradeDao));

        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                var sql = $"Update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMAgents Set SourceType = 1 Where SourceType = 0";
                var rows = context.Database.ExecuteSqlCommand(sql);
                Logger.Info($"Upgrade agent table success, row count: [{rows}].");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while upgrade agent table. Error: {e}");
            }
        }
    }
}
