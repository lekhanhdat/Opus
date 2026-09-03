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
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TenantInfoDao : BaseDao<RMTenantInfo>, ITenantInfoDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(TenantInfoDao));
        private static readonly int MaxTenantDBSize = 250;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public bool CheckIfExistTenantInfo(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo.AsNoTracking().Any(t => t.Id.Equals(tenantId));
            }
        }

        public async Task<bool> CheckIfExistTenantInfoAsync(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            return await ctx.TenantInfo.AnyAsync(t => t.Id.Equals(tenantId));
        }

        public bool CheckIfExistAOSPTenantInfo(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo.AsNoTracking().Any(t => t.Id.Equals(tenantId) && t.IsUsedForAOSP);
            }
        }

        public void UpdateAOSPToOpusTenantInfo(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var aospTenantInfo = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId) && t.IsUsedForAOSP).FirstOrDefault();
            if (aospTenantInfo != null)
            {
                aospTenantInfo.IsUsedForAOSP = false;
                this.Update(aospTenantInfo);
            }
        }
        /// <summary>
        /// 临时字段用于记录是否升级了Cosmos数据
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public int CheckIfExplorerDataMoved(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                RMTenantInfo tenantInfo = ctx.TenantInfo.Find(tenantId);
                if(tenantInfo != null)
                {
                    return tenantInfo.MovedToNewDB;
                }
            }
            return 0;
        }

        public async Task<List<T>> CalcPermissionsWithModuleAsync<T>(string customerId, List<T> permissionsForUser)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var userProducts = await ctx.PaidModule.Where(m => m.TenantId == customerId).Select(o => o.Module)?.FirstOrDefaultAsync();

                var productAttr = typeof(T).GetCustomAttribute<LinkedToProductAttribute>().PaidForProduct;

                var userModules = await ctx.PaidModule.Where(m => m.TenantId == customerId).Select(m => m.Feature)?.FirstOrDefaultAsync();

                var filteredPermissions =
                    from permission in permissionsForUser
                    let moduleAttr = typeof(T).GetMember(permission.ToString())[0]
                        .GetCustomAttribute<LinkedToFeatureAttribute>()
                    where userProducts.HasAnyFlag(productAttr) && (moduleAttr == null || userModules.HasAnyFlag(moduleAttr.PaidForModule))
                    select permission;

                return filteredPermissions.ToList();
            }
        }

        public void AddOrUpdateTenantLinkedModules(string customerId, RMAosLicenseInfo licenseInfo) 
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var (paidForFeature, paidForProduct, enableMachineLearning) = (licenseInfo.AdditionalDataSource, licenseInfo.AdditionalProduct, licenseInfo.EnableAutoClassification);
                if (ctx.PaidModule.Any(m => m.TenantId == customerId)) 
                {
                    var module = ctx.PaidModule.Where(m => m.TenantId == customerId).FirstOrDefault();
                    module.Feature = paidForFeature;
                    module.LastModified = DateTime.UtcNow.Ticks;
                    module.Module = paidForProduct;
                    module.EnableAutoClassfication = enableMachineLearning;
                    ctx.SaveChanges();
                }
                else
                {
                    ctx.PaidModule.Add(new RMPaidModule() 
                    {
                        TenantId = customerId,
                        Feature = paidForFeature,
                        Module = paidForProduct,
                        EnableAutoClassfication = enableMachineLearning,
                        LastModified = DateTime.UtcNow.Ticks,
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public bool CheckAdditionalDataSource(string customerId, long mAdditionalDataSource) 
        {
            if (Enum.TryParse(mAdditionalDataSource.ToString(), out PaidForModule forModule)) 
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    var module = ctx.PaidModule.Where(m => m.TenantId == customerId).Select(m => m.Feature).FirstOrDefault();
                    return module.HasFlag(forModule);
                }
            }
            return false;
        }

        public bool CheckAdditionalProduct(string customerId, long mAdditionalProduct)
        {
            if (Enum.TryParse(mAdditionalProduct.ToString(), out PaidForProduct forProduct))
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    var product = ctx.PaidModule.Where(m => m.TenantId == customerId).Select(m => m.Module).FirstOrDefault();
                    return product.HasFlag(forProduct);
                }
            }
            return false;
        }

        public bool EnableAdditionalDataSource(string customerId)
        {
            
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var module = ctx.PaidModule.Where(m => m.TenantId == customerId).Select(m => m.Feature).FirstOrDefault();
                return module != PaidForModule.None;
            }
        }

        public void CreateTenantInfo(TenantInfoDto tenantInfo)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfo = ConverterToRMTenantInfo(tenantInfo);
                tInfo.MovedToNewDB = 1;  //新建的TenantInfo 默认值是1
                ctx.TenantInfo.Add(tInfo);
                ctx.SaveChanges();
                
            }

        }

        public void CreateTenantDB(string dbName)
        {
            using (var performace = new PerformanceScope("CreateTenantDB"))
            {
                CreateDB(dbName);
                InitTenantDBRole(dbName);
                AddDBInfo(dbName);
            }
           
        }

        private RMTenantInfo ConverterToRMTenantInfo(TenantInfoDto dto)
        {
            return new Model.RMTenantInfo()
            {
                Id = dto.TenantId,
                RegisterEmail = dto.RegisterEmail,
                StorageUsageQuota = dto.StorageQuota,
                Status = (int)dto.Status,
                DBUsageQuota = dto.DBQuota,
                //EncryptionKey = dto.EncryptionKey,
                StorageSetting = dto.StorageSetting,
                CreateTime = DateTime.UtcNow,
                SyncNodeState = dto.SyncNodeState,
                IsUsedForAOSP = dto.IsUsedForAOSP,
                //SyncSAState = dto.SyncSAState,
                MultiGeoStatus = dto.MultiGeoStatus,
            };
        }
        private TenantInfoDto ConverterToTenantInfoDto(RMTenantInfo dto)
        {
            
            return new TenantInfoDto()
            {
                TenantId = dto.Id,
                RegisterEmail = dto.RegisterEmail,
                StorageQuota = dto.StorageUsageQuota,
                Status = (TenantStatus)dto.Status,
                DBQuota = dto.DBUsageQuota,
                //EncryptionKey = dto.EncryptionKey,
                StorageSetting = dto.StorageSetting,
                DBName = dto.DBName,
                SchemaName = dto.DBSchema,
                SyncNodeState = dto.SyncNodeState ?? (int)RMDependTypeForInitNode.DAO,
                ExplorerUpgradeStatus = dto.MovedToNewDB,
                IsUsedForAOSP = dto.IsUsedForAOSP,
                IsInitForGControlPlatform = dto.IsInitForGControlPlatform,
                //SyncSAState = dto.SyncSAState,
                MultiGeoStatus = dto.MultiGeoStatus,
            };
        }
        private string GetCreateDBServer() 
        {
            var dbServer = FailoverGroupService.GetPrimaryDBServerName();
            var configedDBServer = RMGlobalConfiguration.DBConfig.ConfigDatabaseInstance;
            if (!string.IsNullOrEmpty(dbServer))
            {
                var domain = configedDBServer.Split('.');
                domain[0] = dbServer;
                dbServer = string.Join(".", domain);
            }
            else 
            {
                dbServer = configedDBServer;
            }
            logger.Info($"get create db server name:{dbServer}");
            return dbServer;
        }
        private void CreateDB(string dbName)
        {
            using (var performace = new PerformanceScope("CreateNewDB")) 
            {
                var dbServer = GetCreateDBServer();

                using (var con = AzureUtil.GetConnection(dbServer, "master"))
                {
                    var command = con.CreateCommand();
                    command.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;

                    command.CommandText = @"SELECT CAST(CASE 
		                                                    WHEN SERVERPROPERTY('EditionId')=1674378470
			                                                    THEN 1
		                                                    ELSE 0
	                                                     END AS BIT
	                                                    )";
                    var isAzureDB = (bool)command.ExecuteScalar();
                    if (isAzureDB)
                    {
                        var dbTier = "S0";
                        //if (string.IsNullOrEmpty(dbTier))
                        //{
                        //    throw new Exception("db tier is empty");
                        //}
                        command.CommandText = String.Format("CREATE DATABASE {0} COLLATE Latin1_General_CI_AS_KS_WS (edition='standard',SERVICE_OBJECTIVE ='{2}', MAXSIZE={1}GB)", SecurityUtils.SanitizeSQLSchemaName(dbName), MaxTenantDBSize, dbTier);
                    }
                    else
                    {
                        command.CommandText = String.Format("CREATE DATABASE {0} CONTAINMENT = PARTIAL COLLATE Latin1_General_CI_AS_KS_WS", SecurityUtils.SanitizeSQLSchemaName(dbName));
                    }
                    command.ExecuteNonQuery();
                }
                Thread.Sleep(5 * 1000);//创建DB是异步操作，立刻连接新DB会出现Cannot open database的异常

                using (var con = AzureUtil.GetConnection(dbServer, dbName))
                {
                    var command = con.CreateCommand();
                    command.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;
                    command.CommandText = "select 1";
                    command.ExecuteNonQuery();
                }
            }
           
            
        }
        private void InitTenantDBRole(string dbName)
        {
            var dbServer = GetCreateDBServer();
            var fullConnStr = RMGlobalConfiguration.DBConfig[RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING_FULL];
            var sqlBuilder = new SqlConnectionStringBuilder(fullConnStr);
            
            using (var con = AzureUtil.GetConnection(dbServer, dbName))
            {
                var cmd = con.CreateCommand();
                cmd.CommandTimeout = DatabaseUtility.DefaultCommandTimeout;

                cmd.CommandText = "CREATE ROLE tenantuser";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "GRANT VIEW DATABASE STATE, CREATE TABLE TO tenantuser";
                cmd.ExecuteNonQuery();

                if(!RMGlobalConfiguration.EnvSetting.IsDevEnvironment 
                    && !string.IsNullOrEmpty(sqlBuilder.UserID) && !string.IsNullOrEmpty(sqlBuilder.Password))
                {
                    logger.Info($"Create user {sqlBuilder.UserID} with db_owner role");
                    cmd.CommandText = $"CREATE USER {sqlBuilder.UserID} WITH PASSWORD='{sqlBuilder.Password}';";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $"ALTER ROLE db_owner ADD member {sqlBuilder.UserID};";
                    cmd.ExecuteNonQuery();
                }
            }
            
        }

        public void DeleteTenantInfo(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                try
                {
                    var tinfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                    if (tinfo != null)
                    {
                        ctx.TenantInfo.Remove(tinfo);
                        ctx.SaveChanges();
                        logger.Info("success to delete tenant info:{0}", tenantId);
                    }
                }
                catch (Exception e)
                {
                    logger.Info("Delete tenant info failed , tenant id :{0}, error : {1}", tenantId, e);
                    throw;
                }
            }

        }

        public string GetAvailableTenantDB(int requiredSize)
        {
            var totalSizeQuotaClause =
                       " (CASE                                                                                            "
                     + "	WHEN (select sum(g.DBUsageQuota) from RMTenantInfoes as g where g.DBName = d.DBName) IS NULL THEN 0 "
                     + "	ELSE (select sum(g.DBUsageQuota) from RMTenantInfoes as g where g.DBName = d.DBName)                "
                     + " END)                                                                                             ";
            var getAvaliableDBIdSql = string.Format("select top 1 d.DBName from RMDBInfoes as d"
                + " where (d.MaxSize - {0}) >= @RequiredSize and d.Type = 0"
                + " order by DBName asc", totalSizeQuotaClause);

            return DatabaseUtility.RetryPolicy.ExecuteAction<string>(() =>
            {
                var sizeParam = new SqlParameter("RequiredSize", requiredSize);

                using (var ctx = RMDBContextManager.GetSystemSQLContext())
                {
                    return ctx.ExecuteScalar<string>(getAvaliableDBIdSql, sizeParam);
                }
            });
        }

        private void AddDBInfo(string dbName)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                ctx.DBInfo.Add(new RMDBInfo()
                {
                    DBName = dbName,
                    MaxSize = RecordsConstants.TenantDBSize,
                    Type = RMDBType.TenantDB,
                });
                ctx.SaveChanges();
            }
          
        }

        //public string GetEncryptionKey(string tenantId)
        //{
        //    string result = string.Empty;
        //    using (var ctx = RMDBContextManager.GetSystemDBContext())
        //    {
        //        result = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId)).Select(a => a.EncryptionKey).FirstOrDefault();
        //    }
        //    return result;
        //}

     
        public bool IsUserNameExist(string tenantId, string userName)
        {
            bool reulst = false;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                reulst = ctx.TenantInfo.AsQueryable().Any(t => !t.Id.Equals(tenantId) && t.DBSchema.Equals(userName));
            }
            return reulst;
           
        }

        public void UpdateStatus(string tenantId, TenantStatus status)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {

                var entity = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                if (entity != null)
                {
                    entity.Status = (int)status;
                    entity.LastModified = DateTime.UtcNow;
                    this.Update(entity);
                }
               
            }
            
        }

        /// <summary>
        /// 更新Tenant状态
        /// 
        /// 过期的Tenant
        ///     TenantStatus.Provisioning->TenantStatus.Provisioning //正在初始化的tenant不处理
        ///     TenantStatus.Normal->TenantStatus.Disabled    //用户从正常到过期
        ///     TenantStatus.Disabled->TenantStatus.Disabled  
        ///     TenantStatus.Locked->TenantStatus.Disabled    //被锁定的用户变为过期
        ///     
        /// 
        /// 正常的Tenant
        ///     TenantStatus.Provisioning->TenantStatus.Provisioning //正在初始化的tenant不处理
        ///     TenantStatus.Normal->TenantStatus.Normal
        ///     TenantStatus.Disabled->TenantStatus.Normal   //从过期到正常，一般case是过期后续费
        ///     TenantStatus.Locked->TenantStatus.Locked     //被锁定的账户状态不变 
        ///     
        /// </summary>
        /// <param name="unavailableTenantIds">unavailable tenants</param>
        public void UpdateStatus(List<string> unavailableTenantIds)
        {
            var provisioningStatus = (int)TenantStatus.Provisioning;
            var normalStatus = (int)TenantStatus.Normal;
            var disabledStatus = (int)TenantStatus.Disabled;
            var lockedStatus = (int)TenantStatus.Locked;
            var softDeletedStatus = (int)TenantStatus.SoftDeleted;
            
            string updateSql = null;
            if(unavailableTenantIds != null && unavailableTenantIds.Count > 0)
            {
                var joinIn = string.Join(',', unavailableTenantIds.Select(t => $"'{t}'"));
                updateSql =
@$"UPDATE RMTenantInfoes SET status = 
  (CASE WHEN status = {provisioningStatus} THEN {provisioningStatus} 
        WHEN status = {normalStatus} THEN {disabledStatus} 
        WHEN status = {disabledStatus} THEN {disabledStatus} 
        WHEN status = {lockedStatus} THEN {disabledStatus} 
        WHEN status = {softDeletedStatus} THEN {softDeletedStatus} 
        ELSE {disabledStatus} END)
WHERE Id IN ({joinIn});

UPDATE RMTenantInfoes SET status =  
  (CASE WHEN status = {provisioningStatus} THEN {provisioningStatus}
        WHEN status = {normalStatus} THEN {normalStatus}  
        WHEN status = {disabledStatus} THEN {normalStatus} 
        WHEN status = {lockedStatus} THEN {lockedStatus} 
        WHEN status = {softDeletedStatus} THEN {softDeletedStatus} 
        ELSE {normalStatus} END)
WHERE Id NOT IN ({joinIn})";

            }
            else
            {
                updateSql = $"UPDATE RMTenantInfoes SET status={(int)TenantStatus.Normal} WHERE status={(int)TenantStatus.Disabled}";
            }

            
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var ctx = RMDBContextManager.GetSystemSQLContext())
                {
                    ctx.ExecuteNonQuery(updateSql);
                }
            });
        }

        public void UpdateSyncNodeState(string tenantId, RMInitNodeState state)
        {
            UpdateSyncNodeState(tenantId, RMInitDataType.RemoteNode, state);
        }
        public void UpdateSyncSAState(string tenantId, RMInitNodeState state)
        {
            UpdateSyncNodeState(tenantId, RMInitDataType.ServiceAccount, state);
        }

        public void UpdateMultiGeoStatus(string tenantId, MultiGeoStatus status)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var entity = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                if (entity != null)
                {
                    entity.MultiGeoStatus = (int)status;
                    entity.LastModified = DateTime.UtcNow;
                    this.Update(entity);
                }
            }
        }

        private void UpdateSyncNodeState(string tenantId, RMInitDataType syncType, RMInitNodeState state)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var entity = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                if (entity != null)
                {
                    var isOldTenant = entity.SyncNodeState == null 
                        || (entity.SyncNodeState & (int)RMDependTypeForInitNode.DAO) == (int)RMDependTypeForInitNode.DAO;
                    int finalState = (int)(isOldTenant ? RMDependTypeForInitNode.DAO : RMDependTypeForInitNode.AOS);
                    finalState |= (int)state;
                    switch (syncType)
                    {
                        case RMInitDataType.RemoteNode:
                            entity.SyncNodeState = finalState;
                            break;
                        //case RMSyncDataType.ServiceAccount:
                        //    entity.SyncSAState = (int)state;
                        //    break;
                        default:
                            return;
                    }
                    
                    entity.LastModified = DateTime.UtcNow;
                    this.Update(entity);
                }
            }
        }

        public void ChangeAccountStatus(string tenantId, TenantStatus status)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (tInfo != null && tInfo.Status != (int)status)
                {
                    tInfo.Status = (int)status;
                    tInfo.LastModified = DateTime.UtcNow;
                    this.Update(tInfo);
                }
            }
        }

        public void UpdateTenantDBInfo(string tenantId, string dbName, string userName, string schemaName)
        {

            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                if (tInfo != null)
                {
                    tInfo.DBName = dbName;
                    tInfo.DBSchema = schemaName;
                    tInfo.LastModified = DateTime.UtcNow;
                    //tInfo.DBUser = userName;
                    this.Update(tInfo);
                }
            } 

        }

        public int GetTenantDBCount()
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var count = ctx.DBInfo.Where(d => d.Type == 0).AsNoTracking().Count();
                return count;
            }
        }

        public bool CheckTenantIsAvailable(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo.Any(t => t.Id.Equals(tenantId) && t.Status == (int)TenantStatus.Normal);
            }
        }

        public TenantInfoDto GetTenantInfo(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId)).AsNoTracking().FirstOrDefault();
                
                return tInfo != null ?  ConverterToTenantInfoDto(tInfo) : null;
            }

        }

        public async Task<TenantStatus?> TryGetTenantStatusAsync(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var result = await ctx.TenantInfo
                .Where(t => t.Id == tenantId)
                .Select(t => new { t.Id, t.Status})
                .FirstOrDefaultAsync();
            if(result == null)
            {
                return null;
            }

            return (TenantStatus)result.Status;
        }

        public void UpdateStorageInfo(string tenantId, string storageAccountName)
        {
           
            var sql = "Update RMTenantInfoes set StorageAccountName = @StorageAccountName where Id = @Id";
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                var idParam = new SqlParameter("Id", tenantId);
                var storageParam = new SqlParameter("storageAccountName", storageAccountName);
                using (var ctx = RMDBContextManager.GetSystemSQLContext())
                {
                    ctx.ExecuteNonQuery(sql, idParam, storageParam);
                }
            });
        }


        public void UpdateTenantOwner(string tenantId, string owner)
        {
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var ctx = RMDBContextManager.GetSystemDBContext())
                {
                    var tInfo = ctx.TenantInfo.Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                    if (tInfo != null)
                    {
                        tInfo.RegisterEmail = owner;
                        tInfo.LastModified = DateTime.UtcNow;
                        this.Update(tInfo);
                    }
                }
            });
        }
        public void UpdateStorageSetting(string tenantId, TenantStorageSetting storageSetting)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfo = ctx.TenantInfo.AsQueryable().Where(t => t.Id.Equals(tenantId)).FirstOrDefault();
                if (tInfo != null)
                {
                    tInfo.StorageSetting = JsonUtil.JsonSerializer(storageSetting);
                    this.Update(tInfo);
                }
            }
            
        }

        public List<TenantInfoDto> GetAllAvailableTenantInfo()
        {
            List<TenantInfoDto> infos = new List<TenantInfoDto>();

            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                infos = ctx.TenantInfo.Where(t => t.Status == 0).Select(ConverterToTenantInfoDto).ToList();
               
                return infos;
            }
        }

        public bool NeedUpgradeRemoteNodeForAosId(string tenantId)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                return !context.TenantInfo.First(item => item.Id == tenantId).IsUpgradeRemoteNodeForAosId;
            }
        }

        public bool NeedUpgradeManualData(string tenantId)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                return !context.TenantInfo.First(item => item.Id == tenantId).IsUpgradeManualData;
            }
        }

        public void UpdateContainersUpgradeStatusToSuccessful(string tenantId)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var tenant = context.TenantInfo.FirstOrDefault(item => item.Id.Equals(tenantId));
                if(tenant != null)
                {
                    tenant.IsUpgradeRemoteNodeForAosId = true;
                    Update(tenant);
                }
            }
        }

        public void UpdateManualDataUpgradeStatusToSuccessful(string tenantId)
        {
            using(var context = RMDBContextManager.GetSystemDBContext())
            {
                var tenant = context.TenantInfo.FirstOrDefault(item => item.Id.Equals(tenantId));
                if(tenant != null)
                {
                    tenant.IsUpgradeManualData = true;
                    Update(tenant);
                }
            }
        }

        public List<TenantInfoDto> GetPenddingForSyncNodesTenants()
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo
                    .Where(t => t.Status == 0 && (t.SyncNodeState == null || t.SyncNodeState < (int)RMInitNodeState.Syncing))
                    .Select(ConverterToTenantInfoDto)
                    .ToList();
            }
        }
        public List<TenantInfoDto> GetSyncingNodesTenants()
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo
                    .Where(t => t.Status == 0 && t.SyncNodeState > (int)RMInitNodeState.Syncing && t.SyncNodeState < (int)RMInitNodeState.Synced)
                    .Select(ConverterToTenantInfoDto)
                    .ToList();
            }
        }

        public int GetTenantInitNodeState(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var state = ctx.TenantInfo
                    .Where(t => t.Id == tenantId && t.Status == 0)
                    .Select(t => t.SyncNodeState)
                    .FirstOrDefault();
                return state == null ? (int)RMDependTypeForInitNode.DAO : state.Value;
            }
        }

        public List<TenantInfoDto> GetAllTenantInfo()
        {
            List<TenantInfoDto> infos = new List<TenantInfoDto>();

            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfos = ctx.TenantInfo.ToList();
                foreach (var tInfo in tInfos)
                {
                    var ctInfo = ConverterToTenantInfoDto(tInfo);
                    infos.Add(ctInfo);
                }
                return infos;
            }
        }
        public List<TenantInfoDto> GetTenantInfoByTenantStatusAndMultiGeoStatus(int tenantStatus, int MultiGeoStatus)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo
                    .Where(t => t.Status == tenantStatus && t.MultiGeoStatus == MultiGeoStatus)
                    .Select(ConverterToTenantInfoDto)
                    .ToList();
            }
        }
        public List<TenantInfoDto> GetAllTenantInfo(List<string> tenantIds)
        {
            List<TenantInfoDto> infos = new List<TenantInfoDto>();

            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var tInfos = ctx.TenantInfo.Where(t => tenantIds.Contains(t.Id)).ToList();
                foreach (var tInfo in tInfos)
                {
                    var ctInfo = ConverterToTenantInfoDto(tInfo);
                    infos.Add(ctInfo);
                }
                return infos;
            }
        }

        public void DeleteTenantDBSchema(string dbName, string userName, string schemaName)
        {
            using (var con = DatabaseUtility.GetConnection(RMGlobalConfiguration.DBConfig.ConfigDatabaseInstance, 
                dbName,
                RMGlobalConfiguration.DBConfig.ConfigDatabaseUserName,
                RMGlobalConfiguration.DBConfig.ConfigDatabasePassword))
            {
                
                using (var tran = con.BeginTransaction())
                {
                    try
                    {
                        DeleteTables(con, tran, schemaName);

                        DeleteSchema(con, tran, schemaName);

                        DeleteDBUsers(con, tran, schemaName);

                        tran.Commit();
                        logger.Info("success to remove db schema and table:{0}", schemaName);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Delete Tenant Error:{ex.ToString()}");
                        tran.Rollback();
                        throw;
                    }
                }
                
            }
        }

        private void DeleteDBUsers(SqlConnection conn, SqlTransaction tran, string userName) 
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Count(1) FROM sys.sysusers WHERE name = @userName";
                cmd.Parameters.AddWithValue("@userName", userName);
                var userExists = ((int)cmd.ExecuteScalar()) > 0;
                if (userExists)
                {
                    cmd.CommandText = string.Format("DROP USER [{0}]", userName);
                    cmd.ExecuteNonQuery();
                }
            }
            logger.Info($"delete user success.");
        }
        private void DeleteSchema(SqlConnection conn, SqlTransaction tran, string schemaName) 
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Count(1) FROM sys.schemas WHERE name = @schema";
                cmd.Parameters.AddWithValue("@schema", schemaName);
                var schemaExists = ((int)cmd.ExecuteScalar()) > 0;
                if (schemaExists)
                {
                    cmd.CommandText = string.Format("DROP SCHEMA [{0}]", schemaName);
                    cmd.ExecuteNonQuery();
                }
            }
            logger.Info($"delete schema success.");
        }

        private void DeleteTables(SqlConnection conn, SqlTransaction tran, string schemaName)
        {
            const int batchSize = 10;
            const int commandTimeout = 300;
            const int maxRetries = 3;

            var tableNames = new List<string>();
            var stopwatch = new Stopwatch();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandTimeout = commandTimeout;
                stopwatch.Start();

                cmd.CommandText = "SELECT name FROM sys.tables WHERE schema_name([schema_id]) = @schema";
                cmd.Parameters.AddWithValue("@schema", schemaName);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader.GetString(0));
                    }
                }
            }

            if (tableNames.Count > 0)
            {
                for (int i = 0; i < tableNames.Count; i += batchSize)
                {
                    var batch = tableNames.Skip(i).Take(batchSize).ToList();
                    var tableArr = batch.ConvertAll(t => $"[{SecurityUtils.SanitizeSQLSchemaName(schemaName)}].[{t}]");
                    var tables = string.Join(",", tableArr);

                    int retryCount = 0;
                    bool success = false;
                    while (!success && retryCount < maxRetries)
                    {
                        try
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = tran;
                                cmd.CommandTimeout = commandTimeout;
                                cmd.CommandText = $"DROP TABLE {tables}";
                                cmd.ExecuteNonQuery();
                                success = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            logger.Error($"Attempt {retryCount} failed to delete tables: {tables}. Error: {ex.Message}");
                            if (retryCount >= maxRetries)
                            {
                                throw;
                            }
                        }
                    }
                }

                stopwatch.Stop();
                logger.Info($"Delete tables success. Operation took {stopwatch.ElapsedMilliseconds} ms.");
            }

            logger.Info("Delete tables success.");
        }

        private bool Update(RMTenantInfo entity)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                var entry = ctx.Entry(entity);
                if (entry.State == EntityState.Modified)
                {
                    return ctx.SaveChanges() > 0;
                }
                else if (entry.State == EntityState.Detached)
                {
                    ctx.DetachLocalObject<RMTenantInfo>(entity);
                    ctx.Set<RMTenantInfo>().Attach(entity);
                    entry.State = EntityState.Modified;
                    return ctx.SaveChanges() > 0;
                }
                return false;
            }
           
        }

        public bool IsEnableCSD(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.TenantInfo.Any(t => t.Id.Equals(tenantId) && t.EnableCSD.HasValue && t.EnableCSD.Value);
            }
        }

        public async Task<bool> IsEnableIntelligent(string tenantId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return await ctx.PaidModule.AnyAsync(o => o.TenantId == tenantId && o.EnableAutoClassfication);
            }
        }

        public async Task<bool> UpdateInitStatusForGControlPlatform(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var entity = await ctx.TenantInfo.FirstOrDefaultAsync(t => t.Id.Equals(tenantId));
            
            if (entity != null)
            {
                entity.IsInitForGControlPlatform = true;
                return this.Update(entity);
            }

            return false;
        }

        public async Task<bool> GetGControlPlatformInitStatus(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var entity = await ctx.TenantInfo.FirstOrDefaultAsync(t => t.Id.Equals(tenantId));
            if(entity != null)
            {
                return entity.IsInitForGControlPlatform;
            }

            logger.Error($"Tenant {tenantId} not found when checking GControl platform init status.");
            return false;
        }

        public string GetRegisterEmailByTenantId(string tenantId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            return ctx.TenantInfo.Where(t => t.Id.Equals(tenantId)).Select(t => t.RegisterEmail).FirstOrDefault() ?? string.Empty;
        }

        public async Task<bool> UpdateMultiGeoTenantInitStatus(string tenantId ,MultiGeoStatus multiGeoStatus)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var entity = await ctx.TenantInfo.FirstOrDefaultAsync(t => t.Id.Equals(tenantId));
            
            if (entity != null)
            {
                if(entity.MultiGeoStatus == (int)multiGeoStatus)
                {
                    return true;
                }
                entity.MultiGeoStatus = (int)multiGeoStatus;
                return this.Update(entity);
            }
            return false;
        }
    }
}
