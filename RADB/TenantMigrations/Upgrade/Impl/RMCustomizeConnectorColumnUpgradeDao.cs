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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMCustomizeConnectorColumnUpgradeDao : IDbUpgradeDao
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMCustomizeConnectorColumnUpgradeDao));

        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                var buildinColumns = context.RMCustomizeConnectorColumns.Where(item => item.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn).ToList();
                var buildinColumnIds = buildinColumns.Select(item => item.Id).ToHashSet();
                var columns = BuildInColumns.Columns;
                var needAddedColumns = columns.Where(item => !buildinColumnIds.Contains(item.Id));

                Logger.Info($"Need added build-in columns: [{string.Join(", ", needAddedColumns)}].");
                var now = DateTime.UtcNow.Ticks;

                var dbColumns = needAddedColumns.ToList().ConvertAll(item => new RMCustomizeConnectorColumn
                {
                    Id = item.Id,
                    Name = item.Name,
                    InternalName = item.InternalName,
                    Origin = item.Origin,
                    Scope = item.Scope,
                    Type = item.Type,
                    Created = now,
                    Modified = now,
                    CreatedBy = SystemAccountInfo.UserId.ToString(),
                    ModifiedBy = SystemAccountInfo.UserId.ToString(),
                    IsRequired = item.IsRequired,
                    IsHidden = item.IsHidden,
                    Extention = item.Extention,
                    IsRemoved = false,
                });
                context.RMCustomizeConnectorColumns.AddRange(dbColumns);
                context.SaveChanges();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while upgrade customize connector column. Error: {e}");
            }
        }
    }
}
