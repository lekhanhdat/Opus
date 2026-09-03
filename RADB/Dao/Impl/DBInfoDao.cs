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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class DBInfoDao: IDBInfoDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(DBInfoDao));
        public int GetExplorerDBCount()
        {
            int count = 0;
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                count = ctx.ExplorerDBMapping.Select(n => n.DBName).Distinct().Count();
            }
            return count;
        }
        public string GetDBNameByTenantId(string customerId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.ExplorerDBMapping.AsNoTracking().Where(c => c.ContainerName == customerId).Select(n => n.DBName).FirstOrDefault();
            }
        }

        public string GetDBNameByNormalTenantId(string customerId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.ExplorerDBMapping.AsNoTracking().Where(c => c.ContainerName == customerId && !c.IsIndependent).Select(n => n.DBName).FirstOrDefault();
            }
        }

        public string GetIdependentDBNameByTenantId(string customerId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                return ctx.ExplorerDBMapping.Where(c => c.ContainerName == customerId && c.IsIndependent).Select(n => n.DBName).FirstOrDefault();
            }
        }

        public int GetExplorerDBResource(string customerId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            return ctx.ExplorerDBMapping.Where(c => c.ContainerName == customerId).Select(n => n.Resource).FirstOrDefault();
        }

        public int GetEIndependentExplorerDBResource(string customerId)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            return ctx.ExplorerDBMapping.Where(c => c.ContainerName == customerId && c.IsIndependent).Select(n => n.Resource).FirstOrDefault();
        }

        public string GetAvailableExplorerDB()
        {
            string result = string.Empty;

            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                using var context = RMDBContextManager.GetSystemDBContext();

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
                if (context.DBInfo.Any(d => d.DBName != RecordsConstants.ExplorerDBDefaultName))
                {
                    var otherDBs = context.DBInfo.Where(d => d.DBName != RecordsConstants.ExplorerDBDefaultName).ToList();
                    context.DBInfo.RemoveRange(otherDBs);
                    context.SaveChanges();
                }
            }


            using (var ctx = RMDBContextManager.GetSystemSQLContext())
            {
                using (var reader = ctx.ExecuteQuery("select d.DBName, d.MaxSize, COUNT(m.ContainerName) as ContainerCount from RMExplorerDBInfoMappings as m right join RMDBInfoes as d on m.DBName = d.DBName where d.Type = 1 group by d.DBName, d.MaxSize"))
                {
                    while (reader.Read())
                    {
                        var dbName = reader.GetString(0);
                        var dbSize = reader.GetInt32(1);
                        var dbUsage = reader.GetInt32(2);

                        //check db exist in cosmos db
                        if (dbSize - dbUsage > 0
                            && new Explorer.Dao.CosmosImp.RecordRepositoryV2().DatabaseExist(dbName).Result
                            && new Explorer.Dao.CosmosImp.RecordRepositoryV2().QueryContainerCountAsync(dbName).Result < 25)
                        {
                            result = dbName;
                            break;
                        }
                    }
                }
            }
            return result;

        }

        public void AddDBInfo(RMDBInfoDto dBDto)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (ctx.DBInfo.Any(d => d.DBName != dBDto.DBName))
                {
                    ctx.DBInfo.Add(new RMDBInfo()
                    {
                        DBName = dBDto.DBName,
                        MaxSize = dBDto.DBSize,
                        Type = dBDto.Type,
                    });
                    ctx.SaveChanges();
                }

            }
        }

        public void AddIndependentDBInfo(RMDBInfoDto dBDto)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (ctx.DBInfo.Any(d => d.DBName != dBDto.DBName))
                {
                    ctx.DBInfo.Add(new RMDBInfo()
                    {
                        DBName = dBDto.DBName,
                        MaxSize = dBDto.DBSize,
                        Type = dBDto.Type,
                        IsIndependent = true
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public void AddExplorerDBMappingInfo(RMDBInfoDto dBDto)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (!ctx.ExplorerDBMapping.Any(d => d.DBName == dBDto.DBName && d.ContainerName == dBDto.ContainerName))
                {
                    ctx.ExplorerDBMapping.Add(new RMExplorerDBInfoMapping()
                    {
                        DBName = dBDto.DBName,
                        ContainerName = dBDto.ContainerName,
                        Resource = dBDto.Resource,
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public void AddIndependentExplorerDBMappingInfo(RMDBInfoDto dBDto)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                if (!ctx.ExplorerDBMapping.Any(d => d.DBName == dBDto.DBName && d.ContainerName == dBDto.ContainerName))
                {
                    ctx.ExplorerDBMapping.Add(new RMExplorerDBInfoMapping()
                    {
                        DBName = dBDto.DBName,
                        ContainerName = dBDto.ContainerName,
                        Resource = dBDto.Resource,
                        IsIndependent = true
                    });
                    ctx.SaveChanges();
                }
            }
        }

        public bool AddAccountForExplorerDBMappingInfo(string customerId, string account)
        {
            using var ctx = RMDBContextManager.GetSystemDBContext();
            var explorerDBMapping = ctx.ExplorerDBMapping.Where(c => c.ContainerName == customerId).FirstOrDefault();
            //explorerDBMapping.AccountEndpoint = account;
            ctx.ExplorerDBMapping.AddOrUpdate(explorerDBMapping);
            return ctx.SaveChanges() > 0;
        }

        public void RemoveExplorerDBMapping(string customerId)
        {
            using (var ctx = RMDBContextManager.GetSystemDBContext())
            {
                try
                {
                    if (ctx.ExplorerDBMapping.Any(d => d.ContainerName == customerId))
                    {
                        var dbMapping = ctx.ExplorerDBMapping.Where(d => d.ContainerName == customerId).FirstOrDefault();
                        ctx.ExplorerDBMapping.Remove(dbMapping);
                        ctx.SaveChanges();
                        logger.Info("success to remove explorer db mapping:{0}", customerId);
                    }
                }
                catch(Exception e)
                {
                    logger.Info("Remove explorer db mapping failed:{0}, error : {1}", customerId, e);
                    throw;
                }
            }
        }
    }
}
