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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMScopePermissionDao : BaseDao<RMScopePermission>, IRMScopePermissionDao
    {
        public ICollection<int> GetExcludeScopes(string scope, IList<int> Accounts)
        {
            scope = scope.Trim();
            using (var ctx = GetNewContext())
            {
                //找出没有权限的直接子节点
                var queryPermissionId = ctx.ScopePermission.Where(o => o.ParentScope == scope).Select(o => o.Id);
                var innerHaspermissionQuery = ctx.ScopeAccountMapping.Where(o => Accounts.Contains(o.Account) && queryPermissionId.Contains(o.ScopePermission)).Select(o => o.ScopePermission);

                var result = queryPermissionId.Where(o => !innerHaspermissionQuery.Contains(o));
                return result.ToList();
            }
        }
        public ICollection<int> GetPermissionScopes(IList<int> Accounts)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.ScopeAccountMapping.Where(s => Accounts.Contains(s.Account)).Select(t => t.ScopePermission).Distinct().ToList();
            }
        }
        public ICollection<int> GetInclueScopes(string scope, IList<int> Accounts)
        {
            scope = scope.Trim();
            using (var ctx = GetNewContext())
            {
                //找出有权限的直接子节点
                var child = from p in ctx.ScopePermission.Where(o => o.ParentScope == scope)
                            join m in ctx.ScopeAccountMapping.Where(o => Accounts.Contains(o.Account))
                            on p.Id equals m.ScopePermission
                            select p.Id;

                return child.ToList();
            }
        }

        public List<int> GetExcludeScopePermissions(string scopePath, IList<int> Accounts)
        {
            using (var ctx = GetNewContext())
            {
                //找出当前节点下所有打破继承的权限Ids
                var permissionIds = ctx.ScopePermission.Where(o => o.ScopePath.StartsWith(scopePath) && o.ScopePath != scopePath).Select(o => o.Id).ToList();
                //已经设置了Permission的account mapping
                var accountMapping = ctx.ScopeAccountMapping.Where(o => permissionIds.Contains(o.ScopePermission)).Select(o => new { o.ScopePermission, o.Account });
                var permissionIdsWithUser = accountMapping.Select(o => o.ScopePermission).Distinct().ToList();
                //没设置权限User，enduser对它没有权限，需要过滤
                var permissionIdsWithOutUser = permissionIds.Except(permissionIdsWithUser);
                //找出当前account有权限的Permission
                var permissionIds4CurrentUser = accountMapping.Where(o => permissionIdsWithUser.Contains(o.ScopePermission) && Accounts.Contains(o.Account)).Select(o => o.ScopePermission).Distinct();
                //设置权限user，但不是当前enduser,需要过滤
                var currentUserPermissionIds = permissionIdsWithUser.Except(permissionIds4CurrentUser).ToList();
                var exceptPermissionIds = ctx.ScopeAccountMapping.Where(o => !Accounts.Contains(o.Account) && currentUserPermissionIds.Contains(o.ScopePermission)).Select(o => o.ScopePermission).ToList();
                return exceptPermissionIds.Concat(permissionIdsWithOutUser).ToList();
            }
        }

        public List<int> GetIncludeScopePermissions(string scopePath, IList<int> Accounts)
        {
            using (var ctx = GetNewContext())
            {
                //找出当前节点下所有打破继承的权限Ids
                var permissionIds = ctx.ScopePermission.Where(o => o.ScopePath.StartsWith(scopePath) && o.ScopePath != scopePath).Select(o => o.Id).ToList();
                //过滤当前User有权限的Ids
                return ctx.ScopeAccountMapping.Where(o => Accounts.Contains(o.Account) && permissionIds.Contains(o.ScopePermission)).Select(o => o.ScopePermission).ToList();
            }
        }

        public bool HasScopePermission(string scopePath, IList<int> accounts)
        {
            var hasPermission = true;
            var scopePaths = GetScopePaths(scopePath);
            using (var ctx = GetNewContext())
            {
                var scopePermission = ctx.ScopePermission.Where(o => scopePaths.Contains(o.ScopePath)).OrderByDescending(o => o.ScopePath).FirstOrDefault();
                if (scopePermission != null)
                {
                    hasPermission = ctx.ScopeAccountMapping.Any(o => o.ScopePermission == scopePermission.Id && accounts.Contains(o.Account));
                }
                return hasPermission;
            }
        }

        public Dictionary<string, bool> GetScopeBreakInherMapping(List<string> scopeIds)
        {
            Dictionary<string, bool> scopeBreakInherDic = new Dictionary<string, bool>();
            using (var ctx = GetNewContext())
            {
                var existScopes = ctx.ScopePermission.Where(o => scopeIds.Contains(o.Scope)).Select(o => o.Scope).ToList();
                var notExistScopes = scopeIds.Except(existScopes).ToList();

                existScopes.ForEach(s => scopeBreakInherDic[s] = true);
                notExistScopes.ForEach(s => scopeBreakInherDic[s] = false);
                return scopeBreakInherDic;
            }
        }
        
        public async Task<Dictionary<string, int>> SaveLocationPermissionAsync(ScopePermissionDto dto)
        {
            Dictionary<string, int> scopeIdWithPermissionDic = new Dictionary<string, int>();
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var scopeIds = dto.ScopeInfos.Select(o => o.ScopeId).ToList();
                    var oldPermissions = await ctx.ScopePermission.Where(s => scopeIds.Contains(s.Scope)).ToListAsync();
                    var oldPermissionIds = oldPermissions.Select(o => o.Id);
                    var oldPermissionMappings = ctx.ScopeAccountMapping.Where(o => oldPermissionIds.Contains(o.ScopePermission));

                    if (dto.IsInheritSave) //inheritance
                    {
                        ctx.ScopeAccountMapping.RemoveRange(oldPermissionMappings);
                        ctx.ScopePermission.RemoveRange(oldPermissions);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        foreach (var sItem in dto.ScopeInfos)
                        {
                            var oldPermission = oldPermissions.Where(s => s.Scope == sItem.ScopeId).FirstOrDefault();
                            if (oldPermission == null)
                            {
                                //之前没设置过权限
                                var newPermission = new RMScopePermission
                                {
                                    Scope = sItem.ScopeId,
                                    ParentScope = sItem.ParentScopeId,
                                    ScopePath = sItem.ScopeFullPath
                                };
                                ctx.ScopePermission.Add(newPermission);
                                ctx.SaveChanges();

                                var newPermissionId = newPermission.Id;
                                if (!scopeIdWithPermissionDic.ContainsKey(sItem.ScopeId))
                                {
                                    scopeIdWithPermissionDic.Add(sItem.ScopeId, newPermissionId);
                                }
                                //保存account和permission的关联关系
                                //如果没有保存Account Mapping，代表只有admin有权限
                                var accountIds = new List<int>();
                                if (dto.UserConflictOption == PermissionUserConflictOption.Append)
                                {
                                    //Append方式需要保留Parent设置的Account信息
                                    accountIds = GetInheritPermissionAccountIds(sItem.ScopeFullPath);
                                }
                                accountIds = accountIds.Concat(dto.AccountIds).Distinct().ToList();
                                SaveAccountPermissonMapping(newPermissionId, accountIds);
                            }
                            else
                            {
                                //已设置过权限，只修改Account Mapping
                                //删除不存在的User，添加新设置的User
                                oldPermission.ParentScope = sItem.ParentScopeId;
                                oldPermission.ScopePath = sItem.ScopeFullPath;
                                await UpdateAsync(oldPermission);
                                UpdateAccountPermissionMapping(oldPermission.Id, dto);
                            }
                        }
                    }
                    tran.Commit();
                }
                return scopeIdWithPermissionDic;
            }
        }

        public List<string> GetLocationPathsWithPermission(List<int> userAndGroupIds)
        {
            using (var ctx = GetNewContext())
            {
                var innerQuery = ctx.ScopeAccountMapping.Where(o => userAndGroupIds.Contains(o.Account)).Select(o => o.ScopePermission).Distinct();
                var query = ctx.ScopePermission.Where(o => innerQuery.Contains(o.Id)).Select(o => o.ScopePath);
                return query.ToList();
            }
        }

        public List<int> GetUserIdsWithPermission(string scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var accountIds = new List<int>();
                var scopePermission = ctx.ScopePermission.Where(o => o.Scope == scopeId).FirstOrDefault();
                if (scopePermission != null)
                {
                    accountIds = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == scopePermission.Id).Select(o => o.Account).ToList();
                }
                return accountIds;
            }
        }

        public Dictionary<string, int> GetScopesPermissionWithIds(List<string> scopeIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.ScopePermission
                    .AsNoTracking()
                    .Where(o => scopeIds.Contains(o.Scope))
                    .Select(o => new { o.Scope, o.Id })
                    .ToDictionary(x => x.Scope, x => x.Id);
            }
        }

        public int GetScopePermissionId(string scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var permissionInfo = ctx.ScopePermission.Where(o => o.Scope.Equals(scopeId)).FirstOrDefault();
                return permissionInfo != null ? permissionInfo.Id : 0;
            }
        }

        public List<Guid> GetParentBreakInherPermissionNodeIds(List<string> parentScopeIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.ScopePermission.Where(o => parentScopeIds.Contains(o.Scope)).Select(o=> new Guid(o.Scope)).ToList();
            }
        }

        private void SaveAccountPermissonMapping(int permissionId, List<int> accountIds)
        {
            using var ctx = GetNewContext();
            var items = new List<RMScopeAccountMapping>();

            var existAccountIds = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == permissionId).Select(o => o.Account);
            var notExistAccountIds = accountIds.Except(existAccountIds);
            foreach (var accountId in notExistAccountIds)
            {
                items.Add(new RMScopeAccountMapping
                {
                    Account = accountId,
                    ScopePermission = permissionId,
                    Permission = RMScopePermissionEnum.All
                });
            }
            if (items.Count > 0)
            {
                ctx.ScopeAccountMapping.AddRange(items);
                ctx.SaveChanges();
            }
        }

        private void UpdateAccountPermissionMapping(int permissionId, ScopePermissionDto dto)
        {
            var uiAccountIds = dto.AccountIds;
            using var ctx = GetNewContext();
            var needAddAccountIds = new List<int>();
            var removedAccountIds = new List<int>();
            var addedItems = new List<RMScopeAccountMapping>();
            var removedItems = new List<RMScopeAccountMapping>();
            var dbItems = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == permissionId).ToList();
            if (dbItems.Count > 0)
            {
                switch (dto.UserConflictOption)
                {
                    case PermissionUserConflictOption.Append:
                        var existAccountIds = dbItems.Select(o => o.Account).ToList();
                        needAddAccountIds = uiAccountIds.Except(existAccountIds).ToList();
                        break;
                    case PermissionUserConflictOption.Overwrite:
                        if (uiAccountIds.Count > 0)
                        {
                            removedItems = dbItems.Where(o => !uiAccountIds.Contains(o.Account)).ToList();
                            needAddAccountIds = uiAccountIds.Except(removedAccountIds).ToList();
                        }
                        else
                        {
                            //可设置空user,代表只有admin有权限
                            if (dbItems.Count > 0)
                            {
                                ctx.ScopeAccountMapping.RemoveRange(dbItems);
                                ctx.SaveChanges();
                            }
                        }
                        break;
                    default:
                        throw new Exception("Conflict options that do not exist.");
                }
            }
            else
            {
                needAddAccountIds = uiAccountIds;
            }

            //删除db中存在但ui中不存在的account信息
            if (removedItems.Count > 0)
            {
                removedAccountIds = removedItems.Select(o => o.Account).ToList();
                ctx.ScopeAccountMapping.RemoveRange(removedItems);
                ctx.SaveChanges();
            }

            //添加新增account信息
            foreach (var accountId in needAddAccountIds)
            {
                if (!ctx.ScopeAccountMapping.Any(o => o.Account == accountId && o.ScopePermission == permissionId))
                {
                    addedItems.Add(new RMScopeAccountMapping
                    {
                        Account = accountId,
                        ScopePermission = permissionId,
                        Permission = RMScopePermissionEnum.All
                    });
                }
            }

            if (addedItems.Count > 0)
            {
                ctx.ScopeAccountMapping.AddRange(addedItems);
                ctx.SaveChanges();
            }
        }

        public Dictionary<string, int> GetScopePermissionIds(IList<string> scopePaths)
        {
            using (var ctx = GetNewContext())
            {
                var dic = new Dictionary<string, int>();
                foreach (var scopeIdPath in scopePaths)
                {
                    var tempIdPath = scopeIdPath.TrimEnd('/');
                    var scopeId = tempIdPath.Substring(tempIdPath.LastIndexOf("/") + 1);
                    var permissionId = GetInheritPermissionId(scopeIdPath, true);
                    if (!dic.ContainsKey(scopeId))
                    {
                        dic.Add(scopeId, permissionId);
                    }
                }
                return dic;
            }
        }

        private List<string> GetScopePaths(string scopeIdPath)
        {
            var scopeIdPaths = new List<string>();
            MatchCollection mc = Regex.Matches(scopeIdPath, "/", RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
            foreach (Match item in mc)
            {
                scopeIdPaths.Add(scopeIdPath.Substring(0, item.Index + 1));
            }
            scopeIdPaths.Reverse();
            return scopeIdPaths;
        }

        public Dictionary<List<int>, bool> GetUserIdsAndBreakInheritStatus(string scopePath, bool includeSelf)
        {
            var breakInherit = false;
            var result = new Dictionary<List<int>, bool>();
            var scopePaths = GetScopePaths(scopePath);
            
            var accountIds = new List<int>();
            using (var ctx = GetNewContext())
            {
                if (!includeSelf && scopePaths.Count > 0)
                {
                    //不查当前节点，只查父级
                    scopePaths.RemoveAt(0);
                }
                var scopePermission = ctx.ScopePermission.Where(o => scopePaths.Contains(o.ScopePath)).OrderByDescending(o => o.ScopePath).FirstOrDefault();
                if (scopePermission != null)
                {
                    if (scopePermission.ScopePath == scopePath && includeSelf || !includeSelf)
                    {
                        breakInherit = true;
                    }
                    accountIds = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == scopePermission.Id).Select(o => o.Account).ToList();
                }
                result.Add(accountIds, breakInherit);
                return result;
            }
        }

        public bool IsBottomLocationBreakInherForNormalNode(string scopePath, string normalLocationPath)
        {
            var isBreakInherit = false;
            var scopePaths = GetScopePaths(scopePath);
            using (var ctx = GetNewContext())
            {
                var scopePermission = ctx.ScopePermission.Where(o => scopePaths.Contains(o.ScopePath)).OrderByDescending(o => o.ScopePath).FirstOrDefault();
                if (scopePermission != null)
                {
                    if (scopePermission.ScopePath.Contains(normalLocationPath) && scopePermission.ScopePath.Length > normalLocationPath.Length)
                    {
                        isBreakInherit = true;
                    }
                }
            }
            return isBreakInherit;
        }
        public int GetInheritPermissionId(string scopePath, bool includeSelf = false)
        {
            int permissionId = 0;
            var scopePaths = GetScopePaths(scopePath);

            var accountIds = new List<int>();
            using (var ctx = GetNewContext())
            {
                if (scopePaths.Count > 0 && !includeSelf)
                {
                    //不查当前节点，只查父级
                    scopePaths.RemoveAt(0);
                }
                var scopePermission = ctx.ScopePermission.Where(o => scopePaths.Contains(o.ScopePath)).OrderByDescending(o => o.ScopePath).FirstOrDefault();
                if (scopePermission != null)
                {
                    permissionId = scopePermission.Id;
                }
                return permissionId;
            }
        }

        public List<string> GetBreakSubScopeIds(string scope)
        {
            scope = scope.Trim();
            using (var ctx = GetNewContext())
            {
                return ctx.ScopePermission.Where(o => o.ParentScope == scope).Select(o => o.Scope).ToList();
            }
        }

        public RMScopePermission HasBreakInheritPermission(string scopeId)
        {
            using (var ctx = GetNewContext())
            {
                var scopePermission = ctx.ScopePermission.Where(o => o.Scope == scopeId).FirstOrDefault();
                if (scopePermission != null)
                {
                    return scopePermission;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 添加或者更新传入Scope的设置权限Job的失败记录
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <param name="jobId"></param>
        public void AddOrUpdatePermissionjobInfo(List<string> scopeIds, string jobId)
        {
            using (var ctx = GetNewContext())
            {
                var existsItems = ctx.ScopePermissionJobInfo.Where(o => scopeIds.Contains(o.ScopeId)).ToList();
                var existsScopeIds = existsItems.Select(o => o.ScopeId).ToList();
                var newItems = new List<RMScopePermissionJobInfo>();
                var editItems = new List<RMScopePermissionJobInfo>();
                foreach (var scopeId in scopeIds)
                {
                    if (existsScopeIds.Contains(scopeId))
                    {
                        //edit
                        var existsItem = existsItems.Where(o => o.ScopeId.Equals(scopeId)).FirstOrDefault();
                        existsItem.JobId = jobId;
                        existsItem.LastUpdatedTime = DateTime.UtcNow;
                        editItems.Add(existsItem);
                    }
                    else
                    {
                        //add
                        newItems.Add(new RMScopePermissionJobInfo
                        {
                            ScopeId = scopeId,
                            JobId = jobId,
                            LastUpdatedTime = DateTime.UtcNow
                        });
                    }
                }

                if (newItems.Count > 0)
                {
                    ctx.ScopePermissionJobInfo.AddRange(newItems);
                    ctx.SaveChanges();
                }
                if (editItems.Count > 0)
                {
                    BatchUpdatePermissionJobInfo(editItems);
                }
            }
        }

        /// <summary>
        /// 删除scope设置权限Job的失败记录
        /// </summary>
        /// <param name="scopeIds"></param>
        public void DeletePermissionJobInfo(List<string> scopeIds)
        {
            using (var ctx = GetNewContext())
            {
                var items = ctx.ScopePermissionJobInfo.Where(o => scopeIds.Contains(o.ScopeId)).ToList();
                if (items.Count > 0)
                {
                    ctx.ScopePermissionJobInfo.RemoveRange(items);
                    ctx.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 查询传入的scope是否有设置权限job的失败记录，有返回true，否则返回false
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        public bool ExistsFailedJobForScopes(List<string> scopeIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.ScopePermissionJobInfo.Any(o => scopeIds.Contains(o.ScopeId));
            }
        }

        public int BatchUpdatePermissionJobInfo(List<RMScopePermissionJobInfo> entities)
        {
            using var context = GetNewContext();
            foreach (RMScopePermissionJobInfo entity in entities)
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    context.DetachLocalObject<RMScopePermissionJobInfo>(entity);
                    context.Set<RMScopePermissionJobInfo>().Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }
            return context.SaveChanges();
        }

        public void DeleteScopePermission(string scopeId)
        {
            using (var ctx = GetNewContext())
            {
                using (var tran = ctx.Database.BeginTransaction())
                {
                    var scopePermission = ctx.ScopePermission.Where(o=> o.Scope == scopeId).FirstOrDefault();
                    if (scopePermission != null)
                    {
                        var accountMappings = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == scopePermission.Id);
                        ctx.ScopeAccountMapping.RemoveRange(accountMappings);
                        ctx.ScopePermission.Remove(scopePermission);
                        ctx.SaveChanges();
                    }
                    tran.Commit();
                }
            }
        }

        private List<int> GetInheritPermissionAccountIds(string scopePath)
        {
            var scopePaths = GetScopePaths(scopePath);
            var accountIds = new List<int>();
            using (var ctx = GetNewContext())
            {
                if (scopePaths.Count > 0)
                {
                    scopePaths.RemoveAt(0);
                }
                var scopePermission = ctx.ScopePermission.Where(o => scopePaths.Contains(o.ScopePath)).OrderByDescending(o => o.ScopePath).FirstOrDefault();
                if (scopePermission != null)
                {
                    accountIds = ctx.ScopeAccountMapping.Where(o => o.ScopePermission == scopePermission.Id).Select(o => o.Account).ToList();
                }
            }
            return accountIds;
        }
    }
}
