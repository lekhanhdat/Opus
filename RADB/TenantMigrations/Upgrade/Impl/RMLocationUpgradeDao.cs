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
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMLocationUpgradeDao : BaseDao<RMTerm>, IDbUpgradeDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMLocationUpgradeDao));

        public async Task UpgradeAsync(Core.RMDbContext context)
        {
            try
            {
                //logger.Info("init data in location table.");
                //if (!context.RMLocation.Any(a => a.LocationType == (int)LocationType.Root))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = 0;
                //    temp.UniqueId = Guid.NewGuid();
                //    temp.Name = "My Registered Location";
                //    temp.LocationType = (int)LocationType.Root;
                //    temp.DirPath = "";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                #region OtherData
                //var RootLocationId = -1;
                //if (context.RMLocation.Any(a => a.LocationType == (int)LocationType.Root))
                //{
                //    RootLocationId = context.RMLocation.Where(a => a.LocationType == (int)LocationType.Root).FirstOrDefault().Id;
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("Sunshine Building", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = RootLocationId;
                //    temp.Name = "Sunshine Building";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("Shenlan Building", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = RootLocationId;
                //    temp.Name = "Shenlan Building";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                //var sunshineLocationId = -1;
                //if (context.RMLocation.Any(a => a.Name.Equals("Sunshine Building", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    sunshineLocationId = context.RMLocation.Where(a => a.Name.Equals("Sunshine Building", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault().Id;
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("11F", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = sunshineLocationId;
                //    temp.Name = "11F";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/" + sunshineLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("12F", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = sunshineLocationId;
                //    temp.Name = "12F";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/" + sunshineLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                //var E11FLocationId = -1;
                //if (context.RMLocation.Any(a => a.Name.Equals("11F", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    E11FLocationId = context.RMLocation.Where(a => a.Name.Equals("11F", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault().Id;
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("1101", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = E11FLocationId;
                //    temp.Name = "1101";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/" + sunshineLocationId.ToString() + "/" + E11FLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                //if (!context.RMLocation.Any(a => a.Name.Equals("1102", StringComparison.CurrentCultureIgnoreCase)))
                //{
                //    RMLocation temp = new RMLocation();
                //    temp.ParentId = E11FLocationId;
                //    temp.Name = "1102";
                //    temp.LocationType = (int)LocationType.Normal;
                //    temp.DirPath = RootLocationId.ToString() + "/" + sunshineLocationId.ToString() + "/" + E11FLocationId.ToString() + "/";
                //    context.RMLocation.Add(temp);
                //    context.SaveChanges();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade location:{0}", ex.ToString());
            }
        }
    }
}
