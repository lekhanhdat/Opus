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
using AvePoint.RA.Common.Aos.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model.Discovery;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core
{
    public class RMDBContextManager
    {


        public static readonly AveRetryPolicy GroupDbInfoRetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(10, TimeSpan.FromSeconds(20)));
        private static RALogger logger = RALogger.GetInstance(typeof(RMDBContextManager));
        private static ConcurrentDictionary<string, TenantInfoDto> TenantMapping = new ConcurrentDictionary<string, TenantInfoDto>();

        private static ConcurrentDictionary<string, RMTenantDiscoveryDBInfo> DiscoveryDBMapping = new();

        private static IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public static DatabaseContext GetSystemSQLContext()
        {
            var connection = DatabaseUtility.GetConnection(
                RMGlobalConfiguration.DBConfig.ConfigDatabaseInstance, 
                RMGlobalConfiguration.DBConfig.ConfigDatabaseName, 
                RMGlobalConfiguration.DBConfig.ConfigDatabaseUserName, 
                RMGlobalConfiguration.DBConfig.ConfigDatabasePassword);
            return new DatabaseContext(connection);
        }

        public static bool ExistSystemDb()
        {

            return DatabaseUtility.RetryPolicy.ExecuteAction<bool>(() =>
            {
                using (var ctx = GetSystemSQLContext())
                {
                    var sql = "select name from sysobjects where xtype='u' and name=@tableName";
                    var nameParam = new SqlParameter("tableName", RMSysDBContext.TenantTableName);
                    using (var reader = ctx.ExecuteQuery(sql, nameParam))
                    {
                        if (reader.Read())
                        {
                            var schema = reader.GetString(0);

                            return !string.IsNullOrEmpty(schema);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 每次都返回一个新的DbContext，使用完要手动进行释放
        /// </summary>
        public static RMDbContext GetNewDBContext()
        {
            var connection = AzureUtil.GetConnectionUseIdentityToken(DatabaseUtility.GetTenantDbConnectionString());
            var tenantInfo = GetTenantConnectInfo();
            ThrowUtil.ThrowIfNull(tenantInfo, "tenant info is null.");
            var context = new RMDbContext(connection, tenantInfo.SchemaName);
            context.Database.CommandTimeout = 3 * 60;
            return context;
        }
        public static RMDbContext GetNewDBContext(int commandTimeout)
        {
            var connection = AzureUtil.GetConnectionUseIdentityToken(DatabaseUtility.GetTenantDbConnectionString());
            var tenantInfo = GetTenantConnectInfo();
            ThrowUtil.ThrowIfNull(tenantInfo, "tenant info is null.");
            var context = new RMDbContext(connection, tenantInfo.SchemaName);
            context.Database.CommandTimeout = commandTimeout;
            return context;
        }
        internal static async Task<CosmosConnectionInfo> GetCosmosDBConnectionAsync(bool usingNormalDatabase = false)
        {
            var connectInfo = GetTenantConnectInfo();
            ThrowUtil.ThrowIfNull(connectInfo, "cosmos connection tenant info is null.");
           
            var connectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING];
            var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
            var (dbName, resource) = await GetGetExplorerDBInfoByTenantIdAsync(connectInfo.TenantId, usingNormalDatabase);
            if (resource != 0)
            {
                if (!string.IsNullOrEmpty(extensionConnectionStr) )
                {
                    var currentAccount = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr);
                    connectionStr = CipherEncryptionUtil.CipherDecrypt(currentAccount[resource - 1]);
                }
            }
            logger.Info($"Current cosmos resource is {resource}.");
            var accountKey = "AccountKey=";
            var accountEndpoint = "AccountEndpoint=";
            var keyEndpoint = connectionStr.Split(';');
            var endpoint = keyEndpoint[0].Remove(0, accountEndpoint.Length);
            var key = keyEndpoint[1].Remove(0, accountKey.Length);
            var connection = new CosmosConnectionInfo()
            {
                CollectionId = connectInfo.TenantId,
                Key = key,
                Endpoint = endpoint,
                DatabaseId = dbName,
                Resource = resource,
            };

            return connection;
        }

        public static RMSysDBContext GetSystemDBContext()
        {
            return new RMSysDBContext(AzureUtil.GetConnectionUseIdentityToken(DatabaseUtility.GetSystemDbConnectionString()));
        }

        public static void DisposeTenantMapping()
        {
            TenantMapping = new ConcurrentDictionary<string, TenantInfoDto>();
        }

        public static void DisposeCurrentTenantMapping(string tenantId)
        {
            if (TenantMapping.ContainsKey(tenantId))
            {
                TenantMapping.TryRemove(tenantId, out TenantInfoDto removedValue);
            }
           
        }

        public static TenantInfoDto GetTenantConnectInfo()
        {
            var groupId = TenantLocalValue.LogonGroupId;
            TenantInfoDto tenantDBInfo = null;
            if (!TenantMapping.TryGetValue(groupId, out tenantDBInfo))
            {
                tenantDBInfo = GetGroupDbInfoFromDb(groupId);

                if (tenantDBInfo != null && TenantMapping.TryAdd(groupId, tenantDBInfo))
                {
                    logger.Debug("cache db connection info {0}", groupId);
                }
                else 
                {
                    if (!TenantMapping.TryGetValue(groupId, out tenantDBInfo)) 
                    {
                        logger.Debug("cannot find tenant info {0}", groupId);
                    }
                }
            }
            
            return tenantDBInfo;
        }

        public static async Task<RMTenantDiscoveryDBInfo> GetDiscoveryDBConnectInfoAsync()
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            if(!DiscoveryDBMapping.TryGetValue(tenantId, out var value))
            {
                using var context = GetSystemDBContext();
                var res = await context.TenantDiscoveryDBInfoes.FirstAsync(item => item.Id == tenantId);
                DiscoveryDBMapping.TryAdd(tenantId, res);
            }

            return DiscoveryDBMapping[tenantId];
        }

        public static string GetControlDBConnectionString()
        {
            string connectionStr = string.Empty;
            using (var ctx = GetSystemDBContext())
            {
                connectionStr = ctx.Database.Connection.ConnectionString;
            }
            return connectionStr;
        }
        public static Task<CosmosConnectionInfo> GetExplorerDBConnectionInfoAsync()
        {
            return GetCosmosDBConnectionAsync();
        }

        private static TenantInfoDto GetGroupDbInfoFromDb(string tenantId)
        {
            TenantInfoDto infoDto = null;
            GroupDbInfoRetryPolicy.ExecuteAction(() =>
            {
                using (var ctx = GetSystemDBContext())
                {
                    var tInfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId) && !string.IsNullOrEmpty(t.DBName) && !string.IsNullOrEmpty(t.DBSchema)).FirstOrDefault();
                    if (tInfo != null)
                    {
                        infoDto = new TenantInfoDto()
                        {
                            TenantId = tInfo.Id,
                            DBName = tInfo.DBName,
                            SchemaName = tInfo.DBSchema,
                        };
                    }
                }
            });
            return infoDto;
        }

        //private static async Task<string> GetExplorerDBNameByTenantIdAsync(string customerId)

        private static async Task<(string dbName, int resource)> GetIdependentExplorerDBInfoByTenantIdAsync(string customerId)
        {
            var dBInfoDao = new Dao.Impl.DBInfoDao();
            var dbName = dBInfoDao.GetIdependentDBNameByTenantId(customerId);

            if (string.IsNullOrEmpty(dbName))
            {
                dbName = await CreateIndependentNewExplorerDBAsync(customerId);

            }

            var resource = dBInfoDao.GetEIndependentExplorerDBResource(customerId);
            return (dbName, resource);
        }

        private static async Task<(string dbName, int resource)> GetNormalExplorerDBInfoByTenantIdAsync(string customerId)
        {
            var dBInfoDao = new Dao.Impl.DBInfoDao();
            var dbName = dBInfoDao.GetDBNameByTenantId(customerId);

            if (string.IsNullOrEmpty(dbName))
            {
                //check current db size
                dbName = dBInfoDao.GetAvailableExplorerDB();

                if (string.IsNullOrEmpty(dbName))
                {
                    //new db
                    dbName = await CreateNormalNewExplorerDBAsync(customerId);
                }
            }

            var resource = dBInfoDao.GetExplorerDBResource(customerId);
            return (dbName, resource);
        }

        private static async Task<(string dbName, int resource)> GetGetExplorerDBInfoByTenantIdAsync(string customerId, bool usingNormalDatabase = false)
        {
            if (RMCosmosDBIndependentController.IsEnabledIndependent() && !usingNormalDatabase)
            {
                return await GetIdependentExplorerDBInfoByTenantIdAsync(customerId);
            }

            return await GetNormalExplorerDBInfoByTenantIdAsync(customerId);
        }

        private static async Task<string> CreateNormalNewExplorerDBAsync(string customerId)
        {
            bool lockStatus = false;
            var lockerKey = "RECO_ExplorerDB_Locker";
            string dbName = string.Empty;
            try
            {
                var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
                var resource = 0;
                if (!string.IsNullOrEmpty(extensionConnectionStr))
                {
                    var accounts = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr);
                    resource = accounts.Count;
                }
                lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                logger.Info($"begin to create new explorer db:{customerId}, lock status:{lockStatus}.");
                IDBInfoDao dbInfoDao = new Dao.Impl.DBInfoDao();
                dbName = dbInfoDao.GetAvailableExplorerDB();
                if (string.IsNullOrEmpty(dbName))
                {
                    var index = dbInfoDao.GetExplorerDBCount();
                    dbName = BuildExplorerDBName(index);
                    var repsitory = new RecordRepositoryV2();
                    var result = await repsitory.CreateDatabaseIfNotExistsAsync(dbName);
                    dbInfoDao.AddDBInfo(new RMDBInfoDto()
                    {
                        DBName = dbName,
                        DBSize = RecordsConstants.ExplorerDBSize,
                        ContainerName = customerId,
                        Type = RMDBType.ExplorerDB
                    });
                    dbInfoDao.AddExplorerDBMappingInfo(new RMDBInfoDto() { DBName = dbName, ContainerName = customerId, Resource = resource });
                    logger.Info($"success to create new explorer db:{customerId}.");
                }

            }
            catch (Exception ex)
            {
                dbName = string.Empty;
                logger.Error("error occurred while get explorer db,ERROR:{0}", ex.ToString());
                throw;
            }
            finally
            {
                if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                {
                    await RMDBlLocker.ReleaseRecordsLockerAsync(lockerKey);
                }
            }
            return dbName;
        }

        private static async Task<string> CreateIndependentNewExplorerDBAsync(string customerId)
        {
            bool lockStatus = false;
            var lockerKey = "RECO_ExplorerDB_Locker";
            string dbName = string.Empty;
            try
            {
                var extensionConnectionStr = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_COSMOS_DB_CONNECTION_STRING_EXTENSION];
                var resource = 0;
                if (!string.IsNullOrEmpty(extensionConnectionStr))
                {
                    var accounts = JsonConvert.DeserializeObject<List<string>>(extensionConnectionStr);
                    resource = accounts.Count;
                }
                lockStatus = await RMDBlLocker.GetRecordsLockerAsync(lockerKey);
                logger.Info($"begin to create new explorer db:{customerId}, lock status:{lockStatus}.");
                IDBInfoDao dbInfoDao = new Dao.Impl.DBInfoDao();
                var index = dbInfoDao.GetExplorerDBCount();
                dbName = BuildExplorerDBName(index);
                var repsitory = new RecordRepositoryV2();
                var result = await repsitory.CreateDatabaseIfNotExistsAsync(dbName);
                dbInfoDao.AddIndependentDBInfo(new RMDBInfoDto()
                {
                    DBName = dbName,
                    DBSize = RecordsConstants.ExplorerDBSize,
                    ContainerName = customerId,
                    Type = RMDBType.ExplorerDB
                });
                dbInfoDao.AddIndependentExplorerDBMappingInfo(new RMDBInfoDto() { DBName = dbName, ContainerName = customerId, Resource = resource });
                logger.Info($"success to create new explorer db:{customerId}.");

            }
            catch (Exception ex)
            {
                dbName = string.Empty;
                logger.Error("error occurred while get explorer db,ERROR:{0}", ex.ToString());
                throw;
            }
            finally
            {
                if (lockStatus && !string.IsNullOrEmpty(lockerKey))
                {
                    await RMDBlLocker.ReleaseRecordsLockerAsync(lockerKey);
                }
            }
            return dbName;
        }
        private static string BuildExplorerDBName(int index)
        {
            var dbNumber = index + 1;
            return string.Format("{0}_{1}_{2}", "RECO", dbNumber, DateTime.UtcNow.ToString("yyMMddHHmmss"));
        }
    }
}
