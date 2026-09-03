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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using Cloud.Sdk.Data.Aos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMAppProfileDao : BaseDao<RMAppProfileInfo>, IRMAppProfileDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMAppProfileDao));
        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly string _configDedicatedAppsKey  = "ConfigDedicatedAppClientIds";
        public RMAppProfileInfo GetBestAppProfile(Guid o365tenantId, List<int> appTypes = null)
        {
            using (var context = this.GetNewContext())
            {
                RMAppProfileInfo app = null;
                using (var tran = context.Database.BeginTransaction())
                {
                    IQueryable<RMAppProfileInfo> query = null;
                    if(appTypes?.Any() == true)
                    {
                        query = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId && appTypes.Contains(a.AppType));
                    }
                    else
                    {
                        query = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId);
                    }

                    //Exclude the dedicated apps
                    if (TryGetDedicatedAppClientIds(out List<Guid> dedicatedAppIds))
                    {
                        query = query.Where(a => !dedicatedAppIds.Contains(a.AppClientId));
                    }

                    app = query.OrderBy(o => o.UsedTimes).ThenBy(o => o.AppClientId).FirstOrDefault();
                    if (app != null)
                    {
                        var addUsedTimesSql = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.[RMAppProfileInfoes] SET UsedTimes = UsedTimes + 1 WHERE AppClientId = @AppClientId And TenantId = @TenantId";                  
                        context.Database.ExecuteSqlCommand(addUsedTimesSql, new SqlParameter("@AppClientId", app.AppClientId), new SqlParameter("@TenantId", app.TenantId));
                    }
                    tran.Commit();
                }
                return app;
            }
        }

        public RMAppProfileInfo GetBestDedicatedAppProfile(Guid o365tenantId, List<int> appTypes = null)
        {
            using (var context = this.GetNewContext())
            {
                RMAppProfileInfo app = null;

                if (TryGetDedicatedAppClientIds(out List<Guid> dedicatedAppIds))
                {
                    using (var tran = context.Database.BeginTransaction())
                    {
                        IQueryable<RMAppProfileInfo> query = null;
                        if (appTypes?.Any() == true)
                        {
                            query = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId && appTypes.Contains(a.AppType) && dedicatedAppIds.Contains(a.AppClientId));
                        }
                        else
                        {
                            query = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId && dedicatedAppIds.Contains(a.AppClientId));
                        }

                        app = query.OrderBy(o => o.UsedTimes).ThenBy(o => o.AppClientId).FirstOrDefault();
                        if (app != null)
                        {
                            var addUsedTimesSql = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.[RMAppProfileInfoes] SET UsedTimes = UsedTimes + 1 WHERE AppClientId = @AppClientId And TenantId = @TenantId";
                            context.Database.ExecuteSqlCommand(addUsedTimesSql, new SqlParameter("@AppClientId", app.AppClientId), new SqlParameter("@TenantId", app.TenantId));
                        }
                        tran.Commit();
                    }
                }
                
                return app;
            }
        }

        public void ResetAppProfilesForTenant(Guid o365tenantId, List<RMAppProfileInfo> appProfileInfos)
        {
            if (appProfileInfos == null || appProfileInfos.Count == 0)
            {
                return;
            }
            if (appProfileInfos.Any(o => o.AppClientId.Equals(Guid.Empty)))
            {
                throw new Exception("Invalid app profile client id.");
            }

            if (appProfileInfos.Any(o => !o.TenantId.Equals(o365tenantId)))
            {
                throw new Exception("Some app profile does not belong to current tenant. Tenant id:" + o365tenantId);
            }
            using (var context = this.GetNewContext())
            {
                var currentApps = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId).ToList();
                if (currentApps != null && currentApps.Count > 0)
                {
                    context.RMAppProfileInfo.RemoveRange(currentApps);
                    context.SaveChanges();
                }
                context.RMAppProfileInfo.AddRange(appProfileInfos);
                context.SaveChanges();
            }
        }

        public async Task UpdateAppProfilesForTenantAsync(Guid o365tenantId, List<RMAppProfileInfo> appProfileInfos)
        {
            if (appProfileInfos == null || appProfileInfos.Count == 0)
            {
                return;
            }
            if (appProfileInfos.Any(o => o.AppClientId.Equals(Guid.Empty)))
            {
                throw new Exception("Invalid app profile client id.");
            }
            if (appProfileInfos.Any(o => !o.TenantId.Equals(o365tenantId)))
            {
                throw new Exception("Some app profile does not belong to current tenant. Tenant id:" + o365tenantId);
            }
            using (var context = this.GetNewContext())
            {
                var lockerKey = "Multiple_AppProfile_Locker_" + TenantLocalValue.LogonGroupId;
                var lockStatus = false;
                try
                {
                    lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                    logger.Info($"Begin update app profiles. Lock status:{lockStatus}");
                    var currentApps = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId).ToList();
                    Dictionary<string, RMAppProfileInfo> appProfileInfoDict = appProfileInfos.ToDictionary(a => $"{a.AppType}{a.AppClientId}");
                    var currentAppIds = currentApps.Select(a => $"{a.AppType}{a.AppClientId}").ToHashSet();
                    //remove apps no longer exists
                    var noExistingApps = currentApps.Where(a => !appProfileInfoDict.ContainsKey($"{a.AppType}{a.AppClientId}")).ToList();
                    if (noExistingApps != null && noExistingApps.Count > 0)
                    {
                        context.RMAppProfileInfo.RemoveRange(noExistingApps);
                        context.SaveChanges();
                    }
                    //add new app profiles
                    var newApps = appProfileInfos.Where(a => !currentAppIds.Contains($"{a.AppType}{a.AppClientId}")).ToList();
                    if (newApps != null && newApps.Count > 0)
                    {
                        context.RMAppProfileInfo.AddRange(newApps);
                        context.SaveChanges();
                    }

                    //reset used time to 0 for all app profile
                    var allApps = context.RMAppProfileInfo.Where(a => a.TenantId == o365tenantId).ToList();
                    allApps.ForEach(a =>
                    {
                        a.UsedTimes = 0;

                        if(appProfileInfoDict.TryGetValue($"{a.AppType}{a.AppClientId}", out RMAppProfileInfo appInfo))
                        {
                            a.AppType = appInfo.AppType;
                        }
                    });
                    this.BatchUpdate(context, allApps);
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while updating app prpfiles. Error:{e.ToString()}"); 
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

        public void RemoveAppProfilesForTenant(List<Guid> o365tenantIds)
        {
            using (var context = this.GetNewContext())
            {
                var currentApps = context.RMAppProfileInfo.Where(a => o365tenantIds.Contains(a.TenantId)).ToList();
                if (currentApps != null && currentApps.Count > 0)
                {
                    context.RMAppProfileInfo.RemoveRange(currentApps);
                    context.SaveChanges();
                }
            }
        }

        private bool TryGetDedicatedAppClientIds(out List<Guid> ids)
        {
            ids = [];
            var setting = KeyValueDao.GetValueByKey(_configDedicatedAppsKey);
            if(setting != null)
            {
                if (!string.IsNullOrEmpty(setting.Value))
                {
                    ids = JsonConvert.DeserializeObject<List<Guid>>(setting.Value);
                    return true;
                }
            }
            return false;
        }
    }
}
