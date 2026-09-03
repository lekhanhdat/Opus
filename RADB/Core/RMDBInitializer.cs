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
using AvePoint.RA.Common;
using AvePoint.RA.DB.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AvePoint.RA.Common.Cache;
using System.Data.Entity.Migrations.History;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.ControlMigrations.Upgrade.Impl;
using AvePoint.RA.DB.TenantMigrations.Upgrade.Impl;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao.Impl;
using System.IO;
using System.Threading;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using System.Data.SqlClient;

namespace AvePoint.RA.DB.Core
{
    public class RMDBInitializer
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMDBInitializer));
        public RMDBInitializer()
        {
        }

        public static void InitializeControlDatabase()
        {
            logger.Info($"Start to init system database, memory used: {ProcessUtil.GetProcessMemoryMB()}....");
#if DEBUG
            while (File.Exists("c:\\initDB.sleep"))
            {
                Thread.Sleep(1000);
            }
#endif
            using (new PerformanceScope($"InitializeControlDatabase"))
            {
                if (!RMDBContextManager.ExistSystemDb())
                {
                    //create table
                    CreateSystemDBModel();
                }
                else
                {
                    //update
                    UpgradSystemDBModel();
                }
            }
            logger.Info($"Init system database, memory used: {ProcessUtil.GetProcessMemoryMB()}....");
        }

        #region Tenant DB

        /// <summary>
        ///upgrade db model
        /// </summary>
        public static void UpgradTenantDBModel()
        {
            try
            {
                logger.Info($"begin to UpgradTenantDBModel: memory used: {ProcessUtil.GetProcessMemoryMB()}");
                
                using (var ctx = RMDBContextManager.GetNewDBContext())
                {
                    var schemaName = ctx.SchemaName;
                    var dbFullConnStr = GetTenantDBConnectionString(ctx);
                    var tenantDataMigrationsConfiguration = new TenantMigrations.Configuration();
                    tenantDataMigrationsConfiguration.SetSqlGenerator("System.Data.SqlClient", new TenantMigrations.SqlServerSchemaAwareMigrationSqlGenerator(schemaName));
                    tenantDataMigrationsConfiguration.SetHistoryContextFactory("System.Data.SqlClient", (existingConnection, defaultSchema) => new HistoryContext(existingConnection, schemaName));
                    tenantDataMigrationsConfiguration.TargetDatabase = new DbConnectionInfo(dbFullConnStr, "System.Data.SqlClient");
                    tenantDataMigrationsConfiguration.MigrationsAssembly = typeof(RMDbContext).Assembly;
                    tenantDataMigrationsConfiguration.MigrationsNamespace = "AvePoint.RA.DB.TenantMigrations";
                    logger.Info($"begin to tenantDataCtxMigrator: memory used: {ProcessUtil.GetProcessMemoryMB()}");
                    DbMigrator tenantDataCtxMigrator = new DbMigrator(tenantDataMigrationsConfiguration);
                    logger.Info($"during to tenantDataCtxMigrator: memory used: {ProcessUtil.GetProcessMemoryMB()}");
                    tenantDataCtxMigrator.Update();
                    logger.Info($"end to tenantDataCtxMigrator: memory used: {ProcessUtil.GetProcessMemoryMB()}");

                }
                logger.Info($"finish to UpgradTenantDBModel: memory used: {ProcessUtil.GetProcessMemoryMB()}");
                


            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrad tenant db, ERROR:{0}", ex.ToString());
                throw;
            }

        }

        private static string GetTenantDBConnectionString(RMDbContext ctx)
        {
            var builder = new SqlConnectionStringBuilder(RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL]);
            builder.DataSource = ctx.Database.Connection.DataSource;
            builder.InitialCatalog = ctx.Database.Connection.Database;
            return builder.ToString();
        }

        public static async Task UpgradeSecurityDBDataAsync()
        {
            logger.Info($"begin to UpgradTenantDBModel: memory used: {ProcessUtil.GetProcessMemoryMB()}");
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                await new RMRoleAssignmentUpgradeDao().UpgradeManagerRole(ctx);
                await new RMSecurityGroupUpgradeDao().UpgradeAsync(ctx);
                await new RMSecurityGroupUpgradeDao().UpgradeManagerHoldData(ctx);
                await new RMSecurityTermMappingUpgradeDao().UpgradeAsync(ctx);
                await new RMBarcodeTemplateUpgradgeDao().UpgradeAsync(ctx);
                await new RMRoleUpgradeDao().UpgradeAsync(ctx);
            }
            logger.Info($"finish to UpgradTenantDBModel: memory used: {ProcessUtil.GetProcessMemoryMB()}");
        }

        public static async Task UpgradeDBDataAsync()
        {
            try
            {
                logger.Info($"begin to UpgradeDBDataAsync: memory used: {ProcessUtil.GetProcessMemoryMB()}");

                using (var ctx = RMDBContextManager.GetNewDBContext())
                {

                    new RMEamilTemplateDao().InitDefaultData(ctx);
                    await new RMTemplateManagementUpgradgeDao().UpgradeAsync(ctx);
                    await new RMTemplateManagementUpgradgeDao().UpgradeTemplateColumn(ctx);
                    await new RMDashboardUpgradeDao().UpgradeAsync(ctx);
                    await new RMRuleContainerUpgradgeDao().UpgradeAsync(ctx);
                    await new RMCustomizeConnectorContentSourceUpgradeDao().UpgradeAsync(ctx);
                    await new RMCustomizeConnectorColumnUpgradeDao().UpgradeAsync(ctx);
                    await new RMArchiveFullTextIndexUpgradeDao().UpgradeAsync(ctx);
                }
                
                logger.Info($"finish to UpgradeDBDataAsync: memory used: {ProcessUtil.GetProcessMemoryMB()}");

            }
            catch (Exception e)
            {
                logger.Error($"upgrade db data error:{e.ToString()}");
            }
        }

        public static async Task InitDBAsync()
        {
            try
            {
                using (var performace = new PerformanceScope("InitTenantDBDefaultData"))
                {
                    logger.Info($"begin to init data for tenant:{TenantLocalValue.LogonGroupId}");
                    using (var ctx = RMDBContextManager.GetNewDBContext())
                    {
                        await new RMTermManagementUpgradeDao().UpgradeAsync(ctx);

                        new RMRoleAssignmentUpgradeDao().Upgrade(ctx);

                        await new RMTemplateManagementUpgradgeDao().UpgradeAsync(ctx);
                        await new RMBarcodeTemplateUpgradgeDao().UpgradeAsync(ctx);

                        new RMEamilTemplateDao().InitDefaultData(ctx);
                        await new RMSecurityGroupUpgradeDao().UpgradeAsync(ctx);
                        await new RMSecurityTermMappingUpgradeDao().UpgradeAsync(ctx);
                        await new RMRoleUpgradeDao().UpgradeAsync(ctx);
                        await new RMDashboardUpgradeDao().UpgradeAsync(ctx);
                        await new RMKeyValueUpgradeDao().UpgradeAsync(ctx);
                        await new RMRuleContainerUpgradgeDao().UpgradeAsync(ctx);
                        await new RMCustomizeConnectorContentSourceUpgradeDao().UpgradeAsync(ctx);
                        await new RMCustomizeConnectorColumnUpgradeDao().UpgradeAsync(ctx);
                        await new RMEncryptKeyValueUpgradeDao().UpgradeAsync(ctx);
                        await new RMArchiveFullTextIndexUpgradeDao().UpgradeAsync(ctx);
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error($"init db data error:{e.ToString()}");
            }
        }

        public static async Task ReInitRMEncryptKeyValue()
        {
            try
            {
                using (var performace = new PerformanceScope("ReInitRMEncryptKeyValue"))
                {
                    logger.Info($"begin to ReInitRMEncryptKeyValue for tenant:{TenantLocalValue.LogonGroupId}");
                    using (var ctx = RMDBContextManager.GetNewDBContext())
                    {
                        await new RMEncryptKeyValueUpgradeDao().UpgradeAsync(ctx);
                    }
                    logger.Info($"finish to ReInitRMEncryptKeyValue for tenant:{TenantLocalValue.LogonGroupId}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"ReInitRMEncryptKeyValue data error:{e.ToString()}");
            }
        }

        #endregion Tenant DB

        #region Control DB
        /// <summary>
        /// 需要手动创建Control DB
        /// </summary>
        public static void CreateSystemDBModel()
        {
            try
            {
                Database.SetInitializer<RMSysDBContext>(new CreateDatabaseIfNotExists<RMSysDBContext>());
                using (var ctx = new RMSysDBContext())
                {
                    Database.SetInitializer<RMSysDBContext>(new MigrateDatabaseToLatestVersion<RMSysDBContext, ControlMigrations.Configuration>());
                    ctx.Database.Initialize(true);
                    UpgradeSysDBData(ctx);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while create system db, ERROR:{0}", ex.ToString());
                throw;
            }
            finally
            {
                Database.SetInitializer<RMSysDBContext>(null);
            }
        }

        public static void UpgradSystemDBModel()
        {
            try
            {
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    bool sameModel = false;
                    try
                    {
                        sameModel = context.Database.CompatibleWithModel(true);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    if (!sameModel)
                    {
                        logger.Info("Start to migrat control table structure using global configuration");
                        var dbConnStr = GetControlDBConnectionString(context);
                        var configuration = new AvePoint.RA.DB.ControlMigrations.Configuration();
                        configuration.TargetDatabase = new DbConnectionInfo(dbConnStr, "System.Data.SqlClient");
                        var migrator = new DbMigrator(configuration);
                        migrator.Update();
                    }

                    UpgradeSysDBData(context);
                }
                logger.Info("upgrade control db success:{0}", TenantLocalValue.LogonGroupId);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while Upgrad system db, ERROR:{0}", ex.ToString());
                throw;
            }
        }

        private static void UpgradeSysDBData(RMSysDBContext context)
        {

            try
            {
                logger.Info("begin to update control DB data.");
                new RMTaskUpgradeDao().Upgrade(context);
                logger.Info("finish to update control DB data.");
            }
            catch (Exception e)
            {
                logger.Error("Update DB Error {0}", e.ToString());
            }
        }

        private static string GetControlDBConnectionString(RMSysDBContext ctx)
        {
            var builder = new SqlConnectionStringBuilder(RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL]);
            builder.DataSource = ctx.Database.Connection.DataSource;
            builder.InitialCatalog = ctx.Database.Connection.Database;
            return builder.ToString();
        }
        #endregion Control DB


        public static void SetDBMigrateDatabaseToLatestVersion()
        {
            Database.SetInitializer<RMDbContext>(new MigrateDatabaseToLatestVersion<RMDbContext, AvePoint.RA.DB.TenantMigrations.Configuration>());
            Database.SetInitializer<RMSysDBContext>(new MigrateDatabaseToLatestVersion<RMSysDBContext, AvePoint.RA.DB.ControlMigrations.Configuration>());
        }
    }

}
