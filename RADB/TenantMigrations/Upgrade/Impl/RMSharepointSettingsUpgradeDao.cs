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
using AvePoint.RA.Common.Util;
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
//using Z.EntityFramework.Plus;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMSharepointSettingsUpgradeDao : BaseDao<RMSharePointSetting>
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSharepointSettingsUpgradeDao));

        public void Upgrade(RMDbContext context)
        {
            Logger.Info("begin upgrade sp setting data.");
            try
            {
                int updateCount = 0;
                using (var ctx = RMDBContextManager.GetNewDBContext())
                {
                    //updateCount = ctx.RMSharePointSettings.Where(s => s.DocLevelEnableClassification == 0).Update(s => new RMSharePointSetting() { DocLevelEnableClassification = 1 });
                    var list = ctx.RMSharePointSettings.ToList();
                    var isShowUId = false;
                    try
                    {
                        var uniqueIdSetting = ctx.UniqueIdSetting.FirstOrDefault();
                        if (uniqueIdSetting != null)
                        {
                            isShowUId = uniqueIdSetting.IsActived;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn("get exist unique id setting error {0}", e.ToString());
                    }

                    foreach (var item in list)
                    {
                        if (item.ColumnRequired == null)
                        {
                            item.ColumnRequired = true;
                        }
                        if (item.ColumnHidden == null)
                        {
                            item.ColumnHidden = false;
                        }
                        //item.EnableRecordManagement = 1;
                        if (item.IsShowUniqueId == null)
                        {
                            item.IsShowUniqueId = isShowUId;
                        }
                    }
                    if (list.Count > 0)
                    {
                        updateCount = this.BatchUpdate(list);
                    }
                }

                Logger.Info("upgrade sp setting data success row,schemaName:{0}:{1}.", context.SchemaName, updateCount);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while upgrade sp setting,ERROR:{0}", ex.ToString());
            }
        }
    }
}
