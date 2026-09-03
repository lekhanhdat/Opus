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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Timer.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.Service.Common;

namespace RADBInitializationUpgrade
{
    internal class UpgradeDBExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(UpgradeDBModeTaskExecutor));

        private readonly static object thisLock = new object();
        public void Executor()
        {
            try
            {
                logger.Info($"Initialization begin execute.");
                UpgradeControlDB();
                logger.Info($"Initialization control db finish execute.");
                UpgradeTenantDB();
                logger.Info($"Initialization tenant db finish execute.");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade DB Mode,ERROR:{0}", ex.ToString());
            }
        }
        public void UpgradeControlDB()
        {
            try
            {
                RMDBInitializer.InitializeControlDatabase();
            }
            catch (Exception ex)
            {
                logger.Error("errror occurred while upgrade control db mode from service:{0}", ex.ToString());
            }

        }
        private void UpgradeTenantDB() 
        {
            try
            {
                ITenantInfoDao TenantDao = (ITenantInfoDao)PlatformWindsorManager.GetService(typeof(ITenantInfoDao));
                logger.Info($"finish to init TenantDao, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                lock (thisLock)
                {
                    var tInfos = TenantDao.GetAllTenantInfo();
                    foreach (var tInfo in tInfos)
                    {
                        TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, UpgradeTenantDBAsync).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("errror occurred while upgrade tenant db from service:{0}", ex.ToString());
            }
            
        }

        public async System.Threading.Tasks.Task UpgradeTenantDBAsync()
        {
            bool lockStatus = false;
            var lockerKey = "UpgradeTenantDB_" + TenantLocalValue.LogonGroupId; //根据Tenant去Lock

            try
            {
                logger.Info($"begin to UpgradeTenantDB, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                using (new PerformanceScope($"upgradeTenantDB."))
                {
                    lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                    logger.Info($"finish to get locker: {TenantLocalValue.LogonGroupId}, memory used: {ProcessUtil.GetProcessMemoryMB()}, lock status:{lockStatus}.");
                    RMDBInitializer.UpgradTenantDBModel();
                    logger.Info($"end to UpgradTenantDBModel, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                    await RMDBInitializer.UpgradeDBDataAsync();
                    logger.Info($"end to UpgradeDBDataAsync, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                    await RMDBInitializer.UpgradeSecurityDBDataAsync();
                    logger.Info($"end to UpgradeSecurityDBDataAsync, memory used: {ProcessUtil.GetProcessMemoryMB()}");
                }
                logger.Info($"finish to UpgradeTenantDB, memory used: {ProcessUtil.GetProcessMemoryMB()}");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while UpgradeTenantDB, ERROR:{0}", ex.ToString());
            }
            finally
            {
                if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                {
                    await RMDBlLocker.ReleaseRecordsLockerAsync(lockerKey);
                }
            }
        }
    }
}
