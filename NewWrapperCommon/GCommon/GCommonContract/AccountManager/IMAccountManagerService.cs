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

using System.Collections.Generic;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.AccountManager.ViewModel;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;

namespace AvePoint.GCommon.Contract.AccountManager
{
    public interface IMAccountManagerService
    {
        /// <summary>
        /// 获取所有的用户
        /// </summary>
        List<AccountMappingDto> GetAllAccounts();

        List<AccountMappingDto> GetAccountsByObjectIds(List<string> objectIds);

        string ShowNews(string accountId, bool isPopup);

        List<string> GetUsingObjectIds(Object.AccountType accountType);

        List<AccountMappingDto> GetAccountsByType(IEnumerable<Object.AccountType> type);
        
        PermissionViewModel GetPermissionViewModel();

        /// <summary>
        /// 删除选定的用户
        /// </summary>
        /// <returns>成功返回Result.Successful， 失败返回Result.Failed</returns>
        Result DeleteAccounts(IEnumerable<string> idArray);
        /// <summary>
        /// 删除帐户信息,放到API里面的原因：调用了DoOperationOnUserDeleted方法
        /// </summary>
        /// <param name="idArray"></param>
        /// <returns></returns>
        List<ErrorMessage> DeleteAccountsReturnErrorMessage(IEnumerable<string> idArray);

        /// <summary>
        /// 添加Account
        /// </summary>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，如果要添加的组不存在返回Result.NotExist，如果用户已存在返回Result.AlreadyExisted</returns>
        ResultMessage AddAccount(List<AccountDto> accounts);

        /// <summary>
        /// 批量添加Account，为DocAve Batch import AD user使用，其他人请使用AddAccount方法
        /// </summary>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，如果要添加的组不存在返回Result.NotExist，如果用户已存在返回Result.AlreadyExisted</returns>
        ResultMessage BatchAddAccounts(List<AccountDto> accounts);

        /// <summary>
        /// 编辑Account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Result EditAccountForAccountManager(AccountDto account);

        AccountStatusDto GetAccountStatusById(string id);

        Dictionary<string, List<AccountStatusDto>> GetCurrentLogonAccounts(bool needAccountInfo);

        List<GroupDto> GetGroupsByType(IEnumerable<GroupType> types);

        LocalAccountDto GetLocalAccountByUserName(string userName);

        UserLockedOutDto GetLocalAccountNameAndPwdCount(string userName);

        /// <summary>
        /// 获取AddUser页面对应的数据
        /// </summary>
        /// <returns>包括全局的PasswordPolicy和SecuritySetting以及所有的Super Admin组</returns>
        AddUserViewModel GetAddUserViewModel();

        /// <summary>
        /// 获取AddUser页面对应的数据
        /// </summary>
        /// <returns>包括全局的PasswordPolicy和SecuritySetting以及所有的Super Admin组</returns>
        AddUserViewModel LoadAddUserViewModel();

        void InitDefaultAccountManager();
        /// <summary>
        /// 根据输入的字符串来判断在这个domain中是否存在该组或者用户。
        /// </summary>
        /// <param name="userOrGroupName">组名或者用户名，格式是domain\name</param>
        /// <returns>如果还没有添加该domain，返回DomainNotAddedInDB；如果该domain无法check通，返回NotFound；如果check的结果发现是个用户，则返回User；如果check的结果发现还是个组，则返回Group</returns>
        List<CheckUsersResult> CheckUserOrGroupExisted(List<CheckUsersMessage> checkUserMessages);

        /// <summary>
        /// Get all domain information,
        /// This method is for Governance Automation
        /// </summary>
        /// <returns></returns>
        List<DomainDto> GetAllDomains();
        List<DomainDto> GetAllDomainsForGUI();
        AccountStatusDto ChangeUserStatus(string accountId, long logonTime, string ip, bool isLogOn, string serializedPermission);
        List<PermissionDto> GetCurrentAccountPermissionByAccountStatusId(string id);
        AccountMappingDto GetAccountByNameAndAuthenticationType(string name, string type);
        bool IsPasswordExpired(string accountId);
        AccountDto GetAccountDtoByUserName(string userName);

        AccountMappingDto GetSingleUserByUserName(string userName);

        void RemoveOfflineUsersTask();

        /// <summary>
        /// 根据ID获取Account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        AccountMappingDto GetAccount(string id);

        Result EditAccount(AccountDto account);

        List<ErrorMessage> ValidateADUsersOrGroupsCanDelete(IEnumerable<string> idArray);

        List<ErrorMessage> ValidateAccountsReturnErrorMessage(List<string> idArray);

        List<PermissionGroup> GetDefaultPermissionGroup(AveModuleSpecial aveModuleSpecial);

        List<PermissionDto> GetPermissionsByAccountId(string userId);

        List<PermissionDto> GetPermissionsByAccountIdForCompatibleLowVersionAPI(string userId);

        Result AddPermissionLevel(PermissionLevelDto permissionLevel);

        List<PermissionDto> FilterAccountPermissions(List<PermissionDto> permissions);

        List<PermissionDto> GetPermissionsByAccountIds(IEnumerable<string> idArray);

        string CheckDomainAccessable(DomainDto domain, AccountProfileDto profile);

        List<AccountMappingDto> GetAccountsByUserName(string userName);

        void UpdateUserOperationTimeStamp(string id);

        SystemPasswordPolicy GetSystemPasswordPolicy();

        SystemSecurityPolicy GetSystemSecurityPolicy();

        AccountMappingDto GetByObjectIdAndName(string objectId, string objectName);

        Result AddUsertoGivenGroup(string accountId, string groupId);

        Result EditGroup(GroupDto group);

        Result AddGroup(GroupDto group);

        List<ErrorMessage> DeleteGroups(IEnumerable<string> idArray);

        Result RemoveUsersFromGroup(string groupId, IEnumerable<string> userIds);

        AddUserViewModel LoadAddUserViewModelWithoutUsers();

        LogonInfoItem GetLastLogonInfo(string accountId);
        string CompareAccount(AccountDto account, AccountMappingDto accountMapping);

        Dictionary<string, string> GetContent(GroupDto group, GroupDto groupDto);
    }
}
