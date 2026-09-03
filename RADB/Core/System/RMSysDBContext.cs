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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    /// <summary>
    /// control db dbcontext
    /// </summary>
    public class RMSysDBContext : DbContext
    {
        public static readonly string TenantTableName = "RMTenantInfoes";
        public RMSysDBContext() : base(RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL])
        {
            Database.SetInitializer<RMSysDBContext>(null);
            //注意不能使用如下code初始化DB. 会导致DB自动升级的问题.
            //Database.SetInitializer<RMSysDBContext>(new MigrateDatabaseToLatestVersion<RMSysDBContext, AvePoint.RA.DB.ControlMigrations.Configuration>());

        }

        public RMSysDBContext(SqlConnection connection) : base(connection, true)
        {
            Database.SetInitializer<RMSysDBContext>(null);
            //注意不能使用如下code初始化DB. 会导致DB自动升级的问题.
            //Database.SetInitializer<RMSysDBContext>(new MigrateDatabaseToLatestVersion<RMSysDBContext, AvePoint.RA.DB.ControlMigrations.Configuration>());

        }

        #region common functions
        public void DetachLocalObject<T>(T obj) where T : class
        {
            var localObj = FindLocalObject(obj);
            if (localObj != null)
            {
                Detach(localObj);
            }
        }

        public void Detach<T>(T obj) where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
            oc.Detach(obj);
        }

        public T FindLocalObject<T>(T obj) where T : class
        {
            var keys = GetEntityKeys<T>();
            var func = GetFindExp<T>(obj, keys).Compile();
            return Set<T>().Local.FirstOrDefault(func);
        }

        public IEnumerable<string> GetEntityKeys<T>() where T : class
        {
            ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
            var keys = oc.CreateObjectSet<T>().EntitySet.ElementType.KeyProperties.Select(x => x.Name);
            return keys;
        }

        private Expression<Func<T, bool>> GetFindExp<T>(T obj, IEnumerable<string> keys) where T : class
        {
            var pe = Expression.Parameter(typeof(T), "p");

            var keyExps = keys.Select(k =>
            {
                var member = Expression.PropertyOrField(pe, k);
                var val = typeof(T).GetProperty(k).GetValue(obj);
                var eq = Expression.Equal(member, Expression.Constant(val));
                return eq;
            }).ToList();

            if (keys.Count() == 1)
            {
                return Expression.Lambda<Func<T, bool>>(keyExps[0], new[] { pe });
            }

            var combinExp = Expression.AndAlso(keyExps[0], keyExps[1]);
            for (var i = 2; i < keyExps.Count; i++)
            {
                combinExp = Expression.AndAlso(combinExp, keyExps[i]);
            }
            return Expression.Lambda<Func<T, bool>>(combinExp, new[] { pe });
        }
        #endregion

        public DbSet<RMDBInfo> DBInfo { get; set; }
        public DbSet<RMJobProcess> JobProcess { get; set; }
        public DbSet<RMTenantInfo> TenantInfo { get; set; }
        public DbSet<RMTenantDiscoveryDBInfo> TenantDiscoveryDBInfoes { get; set; }
        public DbSet<RMTenantUpgradeInfo> TenantUpgradeInfo { get; set; }
        public DbSet<RMCPGeneralSetting> RMCPGeneralSetting { get; set; }
        public DbSet<RMSecurityProfile> SecurityProfile { get; set; }
        public DbSet<RMJobQueue> JobQueue { get; set; }
        public DbSet<RMProductVersionInfo> ProductVersionInfo { get; set; }
        public DbSet<RMTask> Task { get; set; }
        public DbSet<RMTaskSchedule> RMTaskSchedule { get; set; }
        public DbSet<RMExplorerDBInfoMapping> ExplorerDBMapping { get; set; }
        public DbSet<RMPaidModule> PaidModule { get; set; }
        public DbSet<RMAOSNotification> AOSNotification { get; set; }

        public DbSet<RMTimerInstance> RMTimerInstances { get; set; }

        public DbSet<RMGlobalKeyValue> RMGlobalKeyValue { get; set; }

        public DbSet<RMTenantVectorCosmosMapping> TenantVectorCosmosMapping { get; set; }

        public DbSet<RMTenantVectorPostgreMapping> TenantVectorPostgreMapping { get; set; }

        public DbSet<RMStorageCostEvaluation> StorageCostEvaluations { get; set; }
    }
}
