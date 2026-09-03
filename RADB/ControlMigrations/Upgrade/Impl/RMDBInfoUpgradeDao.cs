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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.ControlMigrations.Upgrade.Impl
{
    public class RMDBInfoUpgradeDao
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        public void Upgrade(Core.RMSysDBContext context)
        {
            try
            {
                var dbSize = RecordsConstants.TenantDBSize;
                var dbInfos = context.DBInfo.Where(d => d.MaxSize != dbSize && d.Type == 0).ToList();
                foreach (var item in dbInfos)
                {
                    item.MaxSize = dbSize;
                }
                if (dbInfos.Count > 0) 
                {
                    context.SaveChanges();
                }
                if (!context.DBInfo.Any(d => d.DBName == RecordsConstants.ExplorerDBDefaultName))
                {
                    context.DBInfo.Add(new Model.RMDBInfo()
                    {
                        DBName = RecordsConstants.ExplorerDBDefaultName,
                        Type = RMDBType.ExplorerDB,
                        MaxSize = RecordsConstants.ExplorerDBSize
                    });
                    context.SaveChanges();
                } 
                else if (context.DBInfo.Any(d => d.DBName == RecordsConstants.ExplorerDBDefaultName && d.Type == 0)) 
                {
                    var db = context.DBInfo.Where(d => d.DBName == RecordsConstants.ExplorerDBDefaultName && d.Type == 0).FirstOrDefault();
                    db.Type = RMDBType.ExplorerDB;
                    ApplyCurrentValues(context, db);
                    logger.Info("update default explorer db info");
                }
                
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while upgrade dbinfo:{ex.ToString()}");
            }

        }

        private bool ApplyCurrentValues(RMSysDBContext context, RMDBInfo entity)
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Modified)
            {
                return context.SaveChanges() > 0;
            }
            else if (entry.State == EntityState.Detached)
            {
                context.DetachLocalObject<RMDBInfo>(entity);
                context.Set<RMDBInfo>().Attach(entity);
                entry.State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
            return false;
        }
    }
}
