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
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IAccountDao : IBaseDao<RMAccount>
    {
        /// <summary>
        /// Crate a new account. if there is one active account with the same user id, then will not create a new one, but return the exist account
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<RMAccount> CreateAsync(RMAccount entity);
        Task<List<RMAccount>> GetUserByIdsAsync(List<int> ids);
		Task<List<RMAccount>> GetUserWithRemovedByIds(List<int> ids);
        Task<RMAccount> GetUserByIdAsync(int id);
        Task<RMAccount> GetUserByAADIdAsync(string id);
        Task<RMAccount> GetUserForImportAsync(string name);
        Task<RMAccount> GetActiveUserByNameAsync(string name);

        Task<List<RMAccount>> GetUserByUserIdsAsync(List<string> userIds);
        
        Task<List<RMAccount>> GetGoogleUserByUserIdsAsync(List<string> userIds);

        Task<List<int>> GetIdsOfUserByUserIdsAsync(List<string> userIds);
        List<RMAccount> GetUserWithRemovedByUserIds(List<string> userIds);

        List<RMAccount> GetUserWithRemovedByPrincipalNames(IEnumerable<string> principalNames);

        /// <summary>
        /// 根据输入的userIds，检查哪些已经存在，返回已经存在的user id集合
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Task<List<string>> GetExistUserIdsAsync(List<string> userIds);
        
        Task<List<(string, string)>> GetExistGoogleUserIdsAsync(List<string> userIds);

        Task<RMAccount> GetUserByUserIdAsync(string userId);

        Task<RMAccount> GetActiveUserByUserIdAsync(string userId);

        List<RMAccount> GetAppAdminAccounts();

        void DeleteUserMapping(List<string> userId);

        bool CheckAdminRole(string userId);
        List<RMAccount> GetUserInGroup(string groupId);
        Task AddUserGroupMappingAsync(string userId, List<string> groupIds);
        List<RMAccount> QueryUsers(UserQueryParams queryDto, out int totalCount);
        Dictionary<string, bool> GetUserAdminRoleDic(List<string> userIds);
        List<string> GetUserParentGroupObjectIdByUserId(string accountId);
        void UpdateByUserId(string firstName, string lastName, long lastUpdateTime, string userId);
        Task<IEnumerable<RMAccount>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertAccountTableAsync(IEnumerable<RMAccount> accounts);
        Task<long> MultiGeoDeleteAllAccountAsync();
        List<RMAccount> GetAccountsActive();
    }
}
