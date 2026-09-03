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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public abstract class BaseDao<TModel> : IBaseDao<TModel> where TModel : BaseModel
    {
        private RALogger Logger = RALogger.GetInstance(typeof(BaseDao<TModel>));
        /// <summary>
        /// CurrentDBContext会在当前线程中被共享，因此不要释放CurrentDBContext，否则CurrentDBContext里缓存的本地Cache会被清空。
        /// CurrentDBContext会在每次HttpRequest结束后被释放。
        /// 在起多个线程的代码中使用SharedDbContext，注意线程间调用SaveChanges()时相互干扰的问题，可以通过互斥锁等手段解决问题。
        /// </summary>
        //protected RMDbContext SharedDbContext
        //{
        //    get
        //    {
        //        return RMDBContextManager.CurrentDBContext;
        //    }
        //}

        /// <summary>
        /// 每次都返回一个新的DbContext，使用完要手动进行释放
        /// </summary>
        /// <returns></returns>
        protected RMDbContext GetNewContext()
        {
            return RMDBContextManager.GetNewDBContext();
        }

        protected IRMCacheManager RMCacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();

        protected IQueryable<TModel> Queryable
        {
            get
            {
                using var context = GetNewContext();
                return context.Set<TModel>().AsQueryable();
            }
        }
        protected IQueryable<TModel> QueryNoTracking
        {
            get
            {
                using var context = GetNewContext();
                return context.Set<TModel>().AsNoTracking().AsQueryable();
            }
        }


        public TModel Create(TModel entity, RMDbContext context)
        {
            try
            {
                context.Entry(entity).State = EntityState.Added;
                context.SaveChanges();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public TModel Create(TModel entity)
        {
            RMDbContext context = null; 
            try
            {
                context = GetNewContext();
                context.Entry(entity).State = EntityState.Added;
                context.SaveChanges();
                return entity;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (context != null)
                {
                    context.Dispose();
                }
            }
            
        }

        public async Task<TModel> CreateAsync(TModel entity)
        {
            using RMDbContext context = GetNewContext();
            context.Entry(entity).State = EntityState.Added;
            await context.SaveChangesAsync();
            return entity;
        }

        public int BatchCreate(List<TModel> entities)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                context.Entry(entity).State = EntityState.Added;
            }
            return context.SaveChanges();
        }

        public async Task<int> BatchCreateAsync(IEnumerable<TModel> entities)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                context.Entry(entity).State = EntityState.Added;
            }
            return await context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(TModel entity)
        {
            using var context = GetNewContext();
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Modified)
            {
                return (await context.SaveChangesAsync()) > 0;
            }
            else if (entry.State == EntityState.Detached)
            {
                context.DetachLocalObject<TModel>(entity);
                context.Set<TModel>().Attach(entity);
                entry.State = EntityState.Modified;
                return (await context.SaveChangesAsync()) > 0;
            }
            return false;
        }

        public bool ApplyCurrentValues(RMDbContext context, TModel entity)
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Modified)
            {
                return context.SaveChanges() > 0;
            }
            else if (entry.State == EntityState.Detached)
            {
                context.DetachLocalObject<TModel>(entity);
                context.Set<TModel>().Attach(entity);
                entry.State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public bool Update<TProperty>(TModel entity, params Expression<Func<TModel, TProperty>>[] properties)
        {
            using var context = GetNewContext();
            var entry = context.Entry(entity);
            bool isModified = entry.State == EntityState.Modified,
                 isDetached = entry.State == EntityState.Detached;
            if (isModified || isDetached)
            {
                if (isDetached)
                {
                    context.DetachLocalObject<TModel>(entity);
                    context.Set<TModel>().Attach(entity);
                }
                if (null != properties)
                {
                    foreach (var pro in properties)
                    {
                        entry.Property(pro).IsModified = true;
                    }
                }
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public int BatchUpdate(RMDbContext context, List<TModel> entities)
        {
            foreach (TModel entity in entities)
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    context.DetachLocalObject<TModel>(entity);
                    context.Set<TModel>().Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }
            return context.SaveChanges();
        }
        public int BatchUpdate(List<TModel> entities)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    context.DetachLocalObject<TModel>(entity);
                    context.Set<TModel>().Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }
            return context.SaveChanges();
        }

        public int BatchUpdate<TProperty>(List<TModel> entities, params Expression<Func<TModel, TProperty>>[] properties)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    context.DetachLocalObject<TModel>(entity);
                    context.Set<TModel>().Attach(entity);
                }
                if (null != properties)
                {
                    foreach (var pro in properties)
                    {
                        entry.Property(pro).IsModified = true;
                    }
                }
            }
            return context.SaveChanges();
        }

        public bool Delete(TModel entity)
        {
            using var context = GetNewContext();
            context.Set<TModel>().Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;
            return context.SaveChanges() > 0;
        }

        public int BatchDelete(List<TModel> entities)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                context.Set<TModel>().Attach(entity);
                context.Entry(entity).State = EntityState.Deleted;
            }
            return context.SaveChanges();
        }

        public async Task<int> BatchDeleteAsync(List<TModel> entities)
        {
            using var context = GetNewContext();
            foreach (TModel entity in entities)
            {
                context.Set<TModel>().Attach(entity);
                context.Entry(entity).State = EntityState.Deleted;
            }
            return await context.SaveChangesAsync();
        }

        public async Task<int> BatchDeleteAsync(Expression<Func<TModel, bool>> whereLambda)
        {
            using var context = GetNewContext();
            var entities = await context.Set<TModel>().Where(whereLambda).ToListAsync();
            if (entities.Count == 0) return 0;

            context.Set<TModel>().RemoveRange(entities);
            return await context.SaveChangesAsync();
        }

        public async Task<int> BatchDeleteAsync(Expression<Func<TModel, bool>> whereLambda, RMDbContext context)
        {
            var entities = await context.Set<TModel>().Where(whereLambda).ToListAsync();
            if (entities.Count == 0) return 0;

            context.Set<TModel>().RemoveRange(entities);
            return await context.SaveChangesAsync();
        }

        public bool DeleteByKey(object keyValue)
        {
            using var context = GetNewContext();
            TModel entity = context.Set<TModel>().Find(keyValue);
            if (entity == null)
            {
                return true;
            }
            context.Set<TModel>().Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;
            return context.SaveChanges() > 0;
        }

        public bool DeleteByKey(RMDbContext context, object keyValue)
        {
            TModel entity = context.Set<TModel>().Find(keyValue);
            context.Set<TModel>().Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;
            return context.SaveChanges() > 0;
        }

        public bool Exist(Expression<Func<TModel, bool>> anyLambda)
        {
            using var context = GetNewContext();
            return context.Set<TModel>().Any(anyLambda);
        }

        public int CountAll()
        {
            using var context = GetNewContext();
            int entity = context.Set<TModel>().Count<TModel>();
            return entity;
        }

        public int Count(Expression<Func<TModel, bool>> whereLambda)
        {
            using var context = GetNewContext();
            int entity = context.Set<TModel>().Count<TModel>(whereLambda);
            return entity;
        }
        
        public async Task<int> CountAsync(Expression<Func<TModel, bool>> whereLambda)
        {
            using var context = GetNewContext();
            int entity = await context.Set<TModel>().CountAsync<TModel>(whereLambda);
            return entity;
        }


        /// <summary>
        /// 按条件查询，返回单条记录
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// Find(w => w.Stage == 1, o => o.Video);
        /// </summary>
        /// <param name="whereLambda">where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        public TModel Find(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties)
        {
            using var context = GetNewContext();
            TModel entity = context.Set<TModel>().AddInclude(includeProperties).FirstOrDefault<TModel>(whereLambda);
            return entity;
        }


        /// <summary>
        /// 按条件查询，返回单条记录
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// Find(w => w.Stage == 1, o => o.Video);
        /// </summary>
        /// <param name="whereLambda">where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        public TModel FindWithNewContext(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties)
        {
            using (var ctx = GetNewContext())
            {
                TModel entity = ctx.Set<TModel>().AddInclude(includeProperties).FirstOrDefault<TModel>(whereLambda);
                return entity;
            }
            
        }

        /// <summary>
        /// 获取全部记录
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindAll(o => o.Form, o => o.Video);
        /// </summary>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        public List<TModel> FindAll(params Expression<Func<TModel, object>>[] includeProperties)
        {
            using var context = GetNewContext();
            return context.Set<TModel>().AddInclude(includeProperties).ToList();
        }

        /// <summary>
        /// 按条件查询列表
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindList(w => w.Stage == 1, o => o.Video);
        /// </summary>
        /// <param name="whereLambda">where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        public Task<List<TModel>> FindListAsync(Expression<Func<TModel, bool>> whereLambda, params Expression<Func<TModel, object>>[] includeProperties)
        {
            return this.FindListWithMultipleAsync<dynamic>(false, null, null, whereLambda, includeProperties);
        }

        /// <summary>
        /// 按条件查询列表,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindListWithColumns(c => new { c.Id, VideoName = c.Video.Name, Form = c.Video });
        /// </summary>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <returns></returns>
        public Task<List<TModel>> FindListWithColumnsAsync(Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null)
        {
            return this.FindListWithMultipleAsync<dynamic>(false, null, columnsLambda, whereLambda);
        }

        /// <summary>
        /// 按条件查询列表,以指定字段排序
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindListWithOrder(true, o => o.CreatedTime, null, o => o.Video);
        /// </summary>
        /// <typeparam name="TO">排序字段类型</typeparam>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <param name="includeProperties">可选参数,需加载的关联属性</param>
        /// <returns></returns>
        public Task<List<TModel>> FindListWithOrderAsync<TO>(bool isAsc, Expression<Func<TModel, TO>> orderLambda, Expression<Func<TModel, bool>> whereLambda = null,
            params Expression<Func<TModel, object>>[] includeProperties)
        {
            return this.FindListWithMultipleAsync(isAsc, orderLambda, null, whereLambda, includeProperties);
        }

        /// <summary>
        /// 按条件查询列表,以指定字段排序,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindListWithOrderColumns(true, o => o.CreatedTime, c => new { c.Id, c.Title }, c => c.Stage == 0);
        /// </summary>
        /// <typeparam name="TO">排序字段类型</typeparam>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">where条件</param>
        /// <returns></returns>
        public Task<List<TModel>> FindListWithOrderColumnsAsync<TO>(bool isAsc, Expression<Func<TModel, TO>> orderLambda, Expression<Func<TModel, object>> columnsLambda,
            Expression<Func<TModel, bool>> whereLambda = null)
        {
            return this.FindListWithMultipleAsync(isAsc, orderLambda, columnsLambda, whereLambda);
        }

        private async Task<List<TModel>> FindListWithMultipleAsync<TO>(bool isAsc, Expression<Func<TModel, TO>> orderLambda, Expression<Func<TModel, object>> columnsLambda,
            Expression<Func<TModel, bool>> whereLambda = null, params Expression<Func<TModel, object>>[] includeProperties)
        {
            using var context = GetNewContext();
            return await context.Set<TModel>().AddWhere(whereLambda).Order(isAsc, orderLambda).AddInclude(includeProperties).SelectListAsync(columnsLambda);
        }

        /// <summary>
        /// 按条件分页查询列表,以指定字段排序
        /// 查询全部字段,及指定的关联属性
        /// example:
        /// FindPageListWithOrder(1, 10, out total, true, o => o.CreatedTime, w => w.Stage == 0, o => o.Video);
        /// </summary>
        /// <typeparam name="S">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="totalRecord">out参数,返回总记录数</param>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <param name="includeProperties">可选参数,需加载关联属性</param>
        /// <returns></returns>
        public Task<(List<TModel>, int)> FindPageListWithOrderAsync<S>(int pageIndex, int pageSize, bool isAsc, Expression<Func<TModel, S>> orderLambda, Expression<Func<TModel, bool>> whereLambda = null, params Expression<Func<TModel, object>>[] includeProperties)
        {
            return this.FindPageListWithMultipleAsync(pageIndex, pageSize, isAsc, orderLambda, null, whereLambda, includeProperties);
        }

        /// <summary>
        /// 按条件分页查询列表,以指定字段排序,返回指定字段
        /// 查询部分字段,可指定关联属性
        /// example:
        /// FindPageListWithOrderColumns(1, 10, out total, true, o => o.CreatedTime, c => new { c.Id, c.Title, c.Video });
        /// </summary>
        /// <typeparam name="S">排序字段类型</typeparam>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="totalRecord">out参数,返回总记录数</param>
        /// <param name="isAsc">true:ASC,false:DESC</param>
        /// <param name="orderLambda">排序字段</param>
        /// <param name="columnsLambda">返回哪些字段,类型须匿名类或DTO</param>
        /// <param name="whereLambda">可选参数,where条件</param>
        /// <returns></returns>
        public Task<(List<TModel>, int)> FindPageListWithOrderColumnsAsync<S>(int pageIndex, int pageSize, bool isAsc, Expression<Func<TModel, S>> orderLambda,
            Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null)
        {
            return this.FindPageListWithMultipleAsync(pageIndex, pageSize, isAsc, orderLambda, columnsLambda, whereLambda);
        }

        private async Task<(List<TModel>, int)> FindPageListWithMultipleAsync<S>(int pageIndex, int pageSize, bool isAsc, Expression<Func<TModel, S>> orderLambda,
            Expression<Func<TModel, object>> columnsLambda, Expression<Func<TModel, bool>> whereLambda = null, params Expression<Func<TModel, object>>[] includeProperties)
        {
            int totalRecord = 0;
            using var context = GetNewContext();
            var list = context.Set<TModel>().AddWhere(whereLambda);
            totalRecord = list.Count();
            return (await list.Order(isAsc, orderLambda).Paging(pageIndex, pageSize).AddInclude(includeProperties).SelectListAsync(columnsLambda), totalRecord);
        }
        public static string GeneratedId()
        {
            return Guid.NewGuid().ToString();
        }

        public void SystemDBExecuteWithRetry(Action<RMSysDBContext> action)
        {
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    context.Database.CommandTimeout = 600;
                    action(context);
                }
            });
        }

        public T SystemDBExecuteWithRetry<T>(Func<RMSysDBContext, T> func)
        {
            return DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var context = RMDBContextManager.GetSystemDBContext())
                {
                    return func(context);
                }
            });
        }

        public void ExecuteWithRetry(Action<RMDbContext> action)
        {
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var context = GetNewContext())
                {
                    action(context);
                }
            });
        }

        public T ExecuteWithRetry<T>(Func<RMDbContext, T> func)
        {
            return DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using (var context = GetNewContext())
                {
                    return func(context);
                }
            });
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<RMDbContext, Task<T>> func)
        {
            return await DatabaseUtility.RetryPolicy.ExecuteAsync(async () =>
            {
                using var context = GetNewContext();
                return await func(context);
            }, 3, TimeSpan.FromSeconds(15));
        }

        public void BatchAdd(DataTable dataTable, string tableName)
        {
            BatchAdd(DatabaseUtility.GetTenantDbConnectionString(), dataTable, tableName);
        }

        public Task BatchAddAsync(DataTable dataTable, string tableName)
        {
            return BatchAddAsync(DatabaseUtility.GetTenantDbConnectionString(), dataTable, tableName);
        }

        public Task BatchAddAsync(DataTable dataTable, string tableName, int timeOutInSecond)
        {
            return BatchAddAsync(DatabaseUtility.GetTenantDbConnectionString(), dataTable, tableName, timeOutInSecond);
        }

        public void SystemDBBatchAdd(DataTable dataTable, string tableName)
        {
            BatchAdd(DatabaseUtility.GetSystemDbConnectionString(), dataTable, tableName);
        }

        public Task SystemDBBatchAddAsync(DataTable dataTable, string tableName)
        {
            return BatchAddAsync(DatabaseUtility.GetSystemDbConnectionString(), dataTable, tableName);
        }

        private void BatchAdd(string sqlConnStr, DataTable dataTable, string tableName)
        {
            DatabaseUtility.RetryPolicy.ExecuteAction(() =>
            {
                using var conn = AzureUtil.GetConnectionUseIdentityToken(sqlConnStr);
                using var bulkCopy = new SqlBulkCopy(conn);

                foreach (DataColumn col in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BulkCopyTimeout = 300;
                bulkCopy.BatchSize = 1000;
                bulkCopy.WriteToServer(dataTable);
            });
        }

        private async Task BatchAddAsync(string sqlConnStr, DataTable dataTable, string tableName, int timeOutInSecond = 300)
        {
            await DatabaseUtility.RetryPolicy.ExecuteAsync(async () =>
            {
                using var conn = AzureUtil.GetConnectionUseIdentityToken(sqlConnStr);
                using var bulkCopy = new SqlBulkCopy(conn);

                foreach (DataColumn col in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BulkCopyTimeout = timeOutInSecond;
                bulkCopy.BatchSize = 1000;
                await bulkCopy.WriteToServerAsync(dataTable);
                return true;
            }, 3, TimeSpan.FromSeconds(15));
        }

        public string GetTenantSchemaName()
        {
            var tenantInfo = RMDBContextManager.GetTenantConnectInfo();
            return SecurityUtils.SanitizeSQLSchemaName(tenantInfo?.SchemaName);
        }

        public async Task ExecuteSetInsertIdentityOn(RMDbContext context, string tableName)
        {
            await context.Database.ExecuteSqlCommandAsync($"SET IDENTITY_INSERT {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{tableName} ON");
        }

        public async Task ExecuteSetInsertIdentityOff(RMDbContext context, string tableName)
        {
            await context.Database.ExecuteSqlCommandAsync($"SET IDENTITY_INSERT {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{tableName} Off");
        }

        public async Task<long> TruncateAllDataInTableAsync(string tableName)
        {
            using var context = GetNewContext();
            try
            {
                return await context.Database.ExecuteSqlCommandAsync($"TRUNCATE TABLE {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{tableName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Delete all data in {tableName} table has error: {ex}");
                return 0;
            }
        }
    }
    static class ExtensionQueryable
    {
        internal static IQueryable<TModel> Order<TModel, TS>(this IQueryable<TModel> queryable, bool isAsc, Expression<Func<TModel, TS>> orderLambda)
        {
            if (null != orderLambda)
            {
                return (isAsc ? queryable.OrderBy(orderLambda) : queryable.OrderByDescending(orderLambda));
            }
            return queryable;
        }

        internal static IQueryable<TModel> Paging<TModel>(this IQueryable<TModel> queryable, int pageIndex, int pageSize)
        {
            if (pageIndex > -1 && pageSize > -1)
            {
                return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            return queryable;
        }

        internal static IQueryable<TModel> PagingWithNextFirst<TModel>(this IQueryable<TModel> queryable, int pageIndex, int pageSize)
        {
            if (pageIndex > -1 && pageSize > -1)
            {
                return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize + 1);
            }
            return queryable;
        }

        internal static IQueryable<TModel> Paging<TModel>(this IQueryable<TModel> queryable, int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            if (pageIndex > -1 && pageSize > -1)
            {
                totalCount = queryable.Count();
                return queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            return queryable;
        }

        internal static IQueryable<TModel> AddWhere<TModel>(this IQueryable<TModel> queryable, Expression<Func<TModel, bool>> whereLambda)
        {
            if (null != whereLambda)
            {
                return queryable.Where(whereLambda);
            }
            return queryable;
        }

        internal static IQueryable<TModel> AddInclude<TModel>(this IQueryable<TModel> queryable, params Expression<Func<TModel, object>>[] includeProperties)
        {
            IQueryable<TModel> result = queryable;
            if (null != includeProperties)
            {
                foreach (Expression<Func<TModel, object>> p in includeProperties)
                {
                    result = result.Include(p);
                }
            }
            return result;
        }

        internal static async Task<List<TModel>> SelectListAsync<TModel>(this IQueryable<TModel> queryable, Expression<Func<TModel, object>> selector)
        {
            if (null != selector)
            {
                return (await queryable.Select(selector).ToListAsync()).ConvertAll<TModel>(o => Convert<TModel>(o));
            }
            return (await queryable.ToListAsync());
        }

        // 转换对象为指定类型
        private static T Convert<T>(object source)
        {
            // 值类型
            if (typeof(T).IsValueType)
            {
                return (T)source;
            }
            // 引用类型
            T val = Activator.CreateInstance<T>();
            PropertyInfo propertyInfo = null;
            foreach (PropertyInfo pi in source.GetType().GetProperties())
            {
                if (null != pi.GetValue(source))
                {
                    propertyInfo = val.GetType().GetProperty(pi.Name);
                    // 同名属性
                    if (null != propertyInfo && propertyInfo.CanWrite)
                    {
                        if (pi.PropertyType.Equals(propertyInfo.PropertyType))
                        {
                            // 类型相同 拷贝属性值
                            propertyInfo.SetValue(val, pi.GetValue(source));
                        }
                        else
                        {
                            // 类型不同 转换类型
                            MethodInfo method = typeof(ExtensionQueryable).GetMethod("Convert", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (null != method)
                            {
                                method = method.MakeGenericMethod(propertyInfo.PropertyType);
                                propertyInfo.SetValue(val, method.Invoke(null, new object[] { pi.GetValue(source) }));
                            }
                        }
                    }
                }
            }
            return val;
        }


    }

    enum OperationType
    {
        Add = 0,
        Modify = 1
    }

}
