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
using AvePoint.Common.Portal;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using Microsoft.Identity.Client;
using PnP.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class AccountDao : BaseDao<RMAccount>, IAccountDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(AccountDao));

        public async Task<RMAccount> CreateAsync(RMAccount entity)
        {
            Validate(entity);
            if (!string.IsNullOrEmpty(entity.UserId))
            {
                var account = await GetActiveUserByUserIdAsync(entity.UserId); ; // already exists one with same user id
                if (account != null) return account;
            }
            return base.Create(entity);
        }

        private void Validate(RMAccount entity)
        {
            if (string.IsNullOrEmpty(entity.UserId)) throw new ArgumentNullException("UserId");
        }

        public List<RMAccount> GetUserByIdsV2(List<int> ids)
        {
            using (var context = GetNewContext())
            {
                return context.Account.AsNoTracking().Where(u => ids.Contains(u.Id) && u.IsRemoved == 0).ToList();
            }
        }

        /// <summary>
        /// for manual CI
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<RMAccount>> GetUserWithRemovedByIds(List<int> ids)
        {
            var idsNotHit = new List<int>();
            var resultListFromCache = new List<RMAccount>();
            var resultListFromDB = new List<RMAccount>();

            //fetch from cache
            foreach (var id in ids)
            {
                var result = await RMCacheManager.Cache.GetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + id);
                if (result != null)
                {
                    resultListFromCache.Add(result);
                }
                else
                {
                    idsNotHit.Add(id);
                }
            }

            if (idsNotHit.Count > 0)
            {
                //fetch from db
                using (var context = GetNewContext())
                {
                    resultListFromDB = await context.Account.AsNoTracking().Where(u => idsNotHit.Contains(u.Id)).ToListAsync();
                    foreach (var account in resultListFromDB)
                    {
                        await RMCacheManager.Cache.SetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + account.Id, account);
                    }
                }


                //merge result
                resultListFromCache.AddRange(resultListFromDB);

                //sort according to parameters
                var tempDic = new Dictionary<int, int>();
                for (int i = 0; i < ids.Count; i++)
                {
                    tempDic[ids[i]] = i;
                }

                resultListFromCache.Sort((x, y) => tempDic[x.Id].CompareTo(tempDic[y.Id]));

                return resultListFromCache;
            }
            else
            {
                //all fit
                return resultListFromCache;
            }
        }

        public async Task<List<RMAccount>> GetUserByIdsAsync(List<int> ids)
        {
            var idsNotHit = new List<int>();
            var resultListFromCache = new List<RMAccount>();
            var resultListFromDB = new List<RMAccount>();

            //fetch from cache
            foreach (var id in ids)
            {
               
                var result = await RMCacheManager.Cache.GetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + id);
                if(result != null)
                {
                    resultListFromCache.Add(result);
                }
                else
                {
                    idsNotHit.Add(id);
                }
            }
          
            if(idsNotHit.Count > 0)
            {
                //fetch from db
                using (var context = GetNewContext())
                {
                    resultListFromDB = await context.Account.AsNoTracking().Where(u => idsNotHit.Contains(u.Id) && u.IsRemoved == 0).ToListAsync();
                    foreach(var account in resultListFromDB)
                    {
                        await RMCacheManager.Cache.SetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + account.Id, account);
                    }
                }


                //merge result
                resultListFromCache.AddRange(resultListFromDB);

                //sort according to parameters
                var tempDic = new Dictionary<int, int>();
                for(int i= 0; i< ids.Count; i++)
                {
                    tempDic[ids[i]] = i;
                }

                resultListFromCache.Sort((x, y) => tempDic[x.Id].CompareTo(tempDic[y.Id]));

                return resultListFromCache;
            }
            else
            {
                //all fit
                return resultListFromCache;
            }
        }

        public async Task<RMAccount> GetUserForImportAsync(string name)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.AsNoTracking().FirstOrDefaultAsync(u => u.UserPrincipalName == name || u.DisplayName == name);
            }
        }

        public async Task<RMAccount> GetActiveUserByNameAsync(string name)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.AsNoTracking().FirstOrDefaultAsync(u => u.IsRemoved == 0 && (u.UserPrincipalName == name || u.DisplayName == name));
            }
        }

        public async Task<RMAccount> GetUserByIdAsync(int id)
        {
            var result = await RMCacheManager.Cache.GetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + id);
            if (result != null)
            {
                return result;
            }
            else
            {
                using (var context = GetNewContext())
                {
                    result =  await context.Account.AsQueryable().Where(u => u.Id == id).FirstOrDefaultAsync();
                    if(result != null)
                    {
                        await RMCacheManager.Cache.SetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + result.Id, result);
                    }
                    return result;
                }
            }
        }

        public async Task<RMAccount> GetUserByAADIdAsync(string id)
        {
            var result = await RMCacheManager.Cache.GetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + id);
            if (result != null)
            {
                return result;
            }
            else
            {
                using (var context = GetNewContext())
                {
                    result = await context.Account.AsQueryable().Where(u => u.AADId == id).FirstOrDefaultAsync();
                    if (result != null)
                    {
                        await RMCacheManager.Cache.SetAsync<RMAccount>(IRMCache.Keys.AccountDao_GetUserById + result.AADId, result);
                    }
                    return result;
                }
            }
        }

        public List<RMAccount> GetUserWithRemovedByUserIds(List<string> userIds)
        {
            using var context = GetNewContext();
            return context.Account.Where(item => userIds.Contains(item.UserId) || userIds.Contains(item.AADId)).ToList();
        }

        public List<RMAccount> GetUserWithRemovedByPrincipalNames(IEnumerable<string> principalNames)
        {
            using var context = GetNewContext();
            return context.Account.Where(item => principalNames.Contains(item.UserPrincipalName)).ToList();
        }

        public async Task<List<RMAccount>> GetUserByUserIdsAsync(List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.Where(u => userIds.Contains(u.UserId) && u.IsRemoved == 0).ToListAsync();
            }
        }
        
        public async Task<List<RMAccount>> GetGoogleUserByUserIdsAsync(List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.Where(u => (userIds.Contains(u.UserId) || userIds.Contains(u.AADId)) && u.IsRemoved == 0).ToListAsync();
            }
        }

        public async Task<List<int>> GetIdsOfUserByUserIdsAsync(List<string> userIds)
        {
            //return RMCacheManager.Cache.GetAsync(IRMCache.Keys.AccountDao_GetIdsOfUserByUserIdsAsync, async () =>
            //{
                using (var context = GetNewContext())
                {
                    return await context.Account.Where(u => userIds.Contains(u.UserId) && u.IsRemoved == 0).Select(u => u.Id).ToListAsync();
                }
            //});
        }

        public async Task<RMAccount> GetUserByUserIdAsync(string userId)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.FirstOrDefaultAsync(u => u.UserId == userId);
            }
        }

        public async Task<RMAccount> GetActiveUserByUserIdAsync(string userId)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.FirstOrDefaultAsync(u => u.UserId == userId && u.IsRemoved == 0);
            }
        }

        public async Task<List<string>> GetExistUserIdsAsync(List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                return await context.Account.Where(o => userIds.Contains(o.UserId) && o.IsRemoved == 0).Select(o => o.UserId).ToListAsync();
            }
        }
        
        public async Task<List<(string, string)>> GetExistGoogleUserIdsAsync(List<string> userIds)
        {
            using var context = GetNewContext();
            var result = await context.Account.Where(o => userIds.Contains(o.AADId) && o.IsRemoved == 0).Select(o => new
            {
                UserId = o.UserId,
                AadId = o.AADId
            }).ToListAsync();
            return result.Select(o => (o.AadId, o.UserId)).ToList();
        }

        public void DeleteUserMapping(List<string> userIds)
        {
            BatchDeleteUserGroupMapping(userIds);
            BatchDeleteRoleMapping(userIds);
        }

        public bool CheckAdminRole(string userId)
        {
            var isAdmin = false;
            var adminType = (int)RMRoleType.ApplicationAdmin;
            using (var context = GetNewContext())
            {
                if (this.Exist(a => a.UserId == userId && a.IsRemoved == 0))
                {
                    var user = this.Find(a => a.UserId == userId && a.IsRemoved == 0);
                    isAdmin = context.LnkUserRole.Any(l => user.UserId.Equals(l.UserId) && l.RoleId == adminType);
                    if (!isAdmin)
                    {
                        //check group admin role
                        var groupIds = context.LnkUserGroup.Where(u => u.UserId == userId).Select(g => g.GroupId).ToList();
                        if (groupIds.Count > 0)
                        {
                            isAdmin = context.LnkUserRole.Any(l => groupIds.Contains(l.UserId) && l.RoleId == adminType);
                        }
                    }

                }
            }

            return isAdmin;
        }

        private void BatchDeleteUserGroupMapping(List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                var userLinks = context.LnkUserGroup.Where(u => userIds.Contains(u.UserId)).ToList();
                if (userLinks.Count > 0)
                {
                    context.Set<RMLnkUserGroup>().RemoveRange(userLinks);
                    context.SaveChanges();
                }
            }

        }


        private void BatchDeleteRoleMapping(List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                var userRoles = context.LnkUserRole.Where(u => userIds.Contains(u.UserId)).ToList();
                if (userRoles.Count > 0)
                {
                    context.Set<RMLnkUserRole>().RemoveRange(userRoles);
                    context.SaveChanges();
                }
            }

        }

        public void UpdateByUserId(string firstName,string lastName,long lastUpdateTime,string userId) 
        {
            using var context = GetNewContext();
            var accountDb = context.Account.SingleOrDefault(a => a.UserId == userId && a.IsRemoved == 0);
            if (accountDb == null)
            {
                return;
            }
            accountDb.FirstName = firstName;
            accountDb.LastName = lastName;
            accountDb.LastUpdateTime = lastUpdateTime;
            context.SaveChanges();

        }

        public async Task<IEnumerable<RMAccount>> LoadByPager(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            return await context.Account.AsNoTracking().OrderBy(a => a.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertAccountTableAsync(IEnumerable<RMAccount> accounts)
        {
            using var context = GetNewContext();
            string tableName = "RMAccounts";
            try
            {
                await ExecuteSetInsertIdentityOn(context, tableName);
                string schemaName = SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var sqlBuilder = new System.Text.StringBuilder();
                var parameters = new List<System.Data.SqlClient.SqlParameter>();
                int paramIndex = 0;

                sqlBuilder.AppendLine($"INSERT INTO {schemaName}.{tableName} (Id, UserId, UserPrincipalName, DisplayName, ObjectType, IsRemoved, CreateTime, LastUpdateTime, AADId, FirstName, LastName) VALUES ");
                int i = 0;
                foreach (var item in accounts)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    sqlBuilder.AppendLine($"(@p{paramIndex}, @p{paramIndex + 1}, @p{paramIndex + 2}, @p{paramIndex + 3}, @p{paramIndex + 4}, @p{paramIndex + 5}, @p{paramIndex + 6}, @p{paramIndex + 7}, @p{paramIndex + 8}, @p{paramIndex + 9}, @p{paramIndex + 10})");

                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex}", item.Id));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 1}", (object)item.UserId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 2}", (object)item.UserPrincipalName ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 3}", (object)item.DisplayName ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 4}", (int)item.ObjectType));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 5}", item.IsRemoved));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 6}", item.CreateTime));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 7}", item.LastUpdateTime));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 8}", (object)item.AADId ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 9}", (object)item.FirstName ?? DBNull.Value));
                    parameters.Add(new System.Data.SqlClient.SqlParameter($"@p{paramIndex + 10}", (object)item.LastName ?? DBNull.Value));
                    paramIndex += 11;
                    i++;
                }
                return await context.Database.ExecuteSqlCommandAsync(sqlBuilder.ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert RMAccounts data has error: {ex}");
                return 0;
            }
            finally
            {
                await ExecuteSetInsertIdentityOff(context, tableName);
            }
        }
        public async Task<long> MultiGeoDeleteAllAccountAsync()
        {
            return await TruncateAllDataInTableAsync("RMAccounts");
        }
        /// <summary>
        /// RoleType = 1, isRemoved = 0; 获取没有被删除的Application Admin
        /// </summary>
        /// <returns></returns>
        public List<RMAccount> GetAppAdminAccounts()
        {
            using (var context = GetNewContext())
            {
                string sql = $"SELECT a.* FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMAccounts as a Join {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMLnkUserRoles as l on a.UserId = l.UserId Join (select RoleId from  {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRoles where RoleType = 1) as r on r.RoleId = l.RoleId where a.IsRemoved = 0";
                List<RMAccount> accounts = context.Database.SqlQuery<RMAccount>(sql).ToList();
                return accounts;
            }
        }

        public List<RMAccount> GetUserInGroup(string groupId)
        {
            using (var context = GetNewContext())
            {
                var userIds = context.LnkUserGroup.Where(u => u.GroupId == groupId).Select(u => u.UserId).ToList();
                return context.Account.Where(a => userIds.Contains(a.UserId) && a.IsRemoved == 0).ToList();
            }
        }
        public async Task AddUserGroupMappingAsync(string userId, List<string> groupIds)
        {
            using (var context = GetNewContext())
            {
                foreach (var item in groupIds)
                {
                    if (!await context.LnkUserGroup.AnyAsync(u => u.UserId == userId && u.GroupId == item))
                    {
                        context.LnkUserGroup.Add(new RMLnkUserGroup() { UserId = userId, GroupId = item });
                    }
                }
                var groupMappingObjs = await context.LnkUserGroup.Where(u => u.UserId == userId && !groupIds.Contains(u.GroupId)).ToListAsync();
                if (groupMappingObjs.Count > 0)
                {
                    context.LnkUserGroup.RemoveRange(groupMappingObjs);
                }
                await context.SaveChangesAsync();
                await RMCacheManager.LnkUserGroupAdded(KeyType._Default, userId);
                await RMCacheManager.LnkUserGroupDeleted(KeyType._Default, userId);
            }
        }

        public List<RMAccount> QueryUsers(UserQueryParams queryDto, out int totalCount)
        {
            using (var ctx = GetNewContext())
            {
                var pageIndex = queryDto.PageIndex;
                var pageSize = queryDto.PageSize;
                var searchValue = queryDto.SearchValue;
                var sortBy = "CreateTime";
                var sortDirection = SortDirectionEnum.Descending;
                Expression<Func<RMAccount, bool>> isActivedUserLambda = o => o.IsRemoved == 0;
                Expression<Func<RMAccount, bool>> searchLambda = null;

                if (!string.IsNullOrEmpty(queryDto.SortBy))
                {
                    sortBy = queryDto.SortBy;
                    sortDirection = queryDto.IsAscending? SortDirectionEnum.Ascending: SortDirectionEnum.Descending;
                }
               
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchLambda = o => o.DisplayName.ToLower().Contains(searchValue.ToLower());
                }

                IQueryable<RMAccount> query = null;
                if (searchLambda != null)
                {
                    query = ctx.Account.AsNoTracking().Where(isActivedUserLambda).Where(searchLambda).SortBy(sortBy, sortDirection);
                }
                else {
                    query = ctx.Account.AsNoTracking().Where(isActivedUserLambda).SortBy(sortBy, sortDirection);
                }
                totalCount = query.Count();
                return query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public Dictionary<string, bool> GetUserAdminRoleDic(List<string> userIds)
        {
            Dictionary<string, bool> userRoleDic = new Dictionary<string, bool>();
            var isAdmin = false;
            var adminType = (int)RMRoleType.ApplicationAdmin;
            using (var context = GetNewContext())
            {
                foreach (var userId in userIds)
                {
                    if (this.Exist(a => a.UserId == userId))
                    {
                        var user = this.Find(a => a.UserId == userId);
                        isAdmin = context.LnkUserRole.Any(l => user.UserId.Equals(l.UserId) && l.RoleId == adminType);
                        if (!isAdmin)
                        {
                            //check group admin role
                            var groupIds = context.LnkUserGroup.Where(u => u.UserId == userId).Select(g => g.GroupId).ToList();
                            if (groupIds.Count > 0)
                            {
                                isAdmin = context.LnkUserRole.Any(l => groupIds.Contains(l.UserId) && l.RoleId == adminType);
                            }
                        }
                        userRoleDic.Add(userId, isAdmin);
                    }
                }
            }
            return userRoleDic;
        }

        public List<string> GetUserParentGroupObjectIdByUserId(string accountId)
        {
            throw new NotImplementedException();
        }

        public List<RMAccount> GetAccountsActive()
        {
            using (var context = GetNewContext())
            {
                return context.Account.AsNoTracking().Where(x => x.IsRemoved == 0).ToList();
            }
        }
    }
}
