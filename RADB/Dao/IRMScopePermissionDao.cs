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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMScopePermissionDao : IBaseDao<RMScopePermission>
    {
        /// <summary>
        /// will get the id collection of direct children which should be excluded based on the accounts inputed
        /// </summary>
        /// <param name="scope">scope to be checked</param>
        /// <param name="Accounts">user/group id list</param>
        /// <returns></returns>
        ICollection<int> GetExcludeScopes(string scope, IList<int> Accounts);
        ICollection<int> GetInclueScopes(string scope, IList<int> Accounts);
        /// <summary>
        /// 保存选中physical节点设置的权限
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<Dictionary<string, int>> SaveLocationPermissionAsync(ScopePermissionDto dto);
        /// <summary>
        /// 返回user设置过权限的location name full path
        /// </summary>
        /// <param name="userAndGroupIds"></param>
        /// <returns></returns>
        List<string> GetLocationPathsWithPermission(List<int> userAndGroupIds);
        /// <summary>
        /// 返回scope保存权限时设置的userId
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        List<int> GetUserIdsWithPermission(string scopeId);
        /// <summary>
        /// get permissionId
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        int GetScopePermissionId(string scopeId);

        List<Guid> GetParentBreakInherPermissionNodeIds(List<string> parentScopeIds);

        /// <summary>
        /// return a dictionary , key: scope id; value: scope permission id
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        Dictionary<string, int> GetScopePermissionIds(IList<string> scopePaths);
        /// <summary>
        /// 判断user对scope是否有权限
        /// </summary>
        /// <param name="scopePath">id full path</param>
        /// <param name="accounts">acount id 和其所在group id</param>
        /// <returns></returns>
        bool HasScopePermission(string scopePath, IList<int> accounts);
        /// <summary>
        /// return a dictionary , key: scope id; value: 是否打破继承(true/false)
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        Dictionary<string, bool> GetScopeBreakInherMapping(List<string> scopeIds);
        /// <summary>
        /// 返回userids和是否打破继承状态
        /// </summary>
        /// <param name="scopePath">scopeid full path</param>
        /// <param name="includeSelf">是否查自身权限</param>
        /// <returns></returns>
        Dictionary<List<int>, bool> GetUserIdsAndBreakInheritStatus(string scopePath, bool includeSelf);

        bool IsBottomLocationBreakInherForNormalNode(string scopePath, string normalLocationPath);
        /// <summary>
        /// 默认返回最近父级权限Id,如果includeSelf设置为True，可返回自身权限Id
        /// </summary>
        /// <param name="scopePath"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        int GetInheritPermissionId(string scopePath, bool includeSelf = false);
        /// <summary>
        /// 获取打破继承的所有节点scope
        /// </summary>
        /// <param name="scope">父节点scope</param>
        /// <returns></returns>
        List<string> GetBreakSubScopeIds(string scope);
        ICollection<int> GetPermissionScopes(IList<int> Accounts);
        /// <summary>
        /// 判断当前Box/Folder节点是否是打破继承状态
        /// </summary>
        /// <param name="scopeId">当前Box/Folder节点Id</param>
        /// <returns></returns>
        RMScopePermission HasBreakInheritPermission(string scopeId);
        /// <summary>
        /// 添加或者修改Scope设置权限失败的记录
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <param name="jobId"></param>
        void AddOrUpdatePermissionjobInfo(List<string> scopeIds, string jobId);
        /// <summary>
        /// 删除Scope设置权限失败的记录
        /// </summary>
        /// <param name="scopeIds"></param>
        void DeletePermissionJobInfo(List<string> scopeIds);
        /// <summary>
        /// 查询Scope设置权限失败的记录
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        bool ExistsFailedJobForScopes(List<string> scopeIds);
        /// <summary>
        /// 删除指定scope的权限相关表记录
        /// </summary>
        /// <param name="scopeId"></param>
        void DeleteScopePermission(string scopeId);
        /// <summary>
        /// 指定scope下，找出当前user没有权限的ScopePermissionIds
        /// </summary>
        /// <param name="scopePath">scopeId full path</param>
        /// <param name="accounts">acount id 和其所在group id</param>
        /// <returns></returns>
        List<int> GetExcludeScopePermissions(string scopePath, IList<int> accounts);
        /// <summary>
        /// 指定scope下，找出当前user有权限的ScopePermissionIds
        /// </summary>
        /// <param name="scopePath">scopeId full path</param>
        /// <param name="accounts">acount id 和其所在group id</param>
        /// <returns></returns>
        List<int> GetIncludeScopePermissions(string scopePath, IList<int> accounts);
        /// <summary>
        /// return a dictionary , key: scope ; value:  id
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        Dictionary<string, int> GetScopesPermissionWithIds(List<string> scopeIds);

    }
}
