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
using System.ServiceModel;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.AccountManager.ViewModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ForgotPassword;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Wcf;

namespace AvePoint.GCommon.Contract.AccountManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMAccountManagerService
    {
        /// <summary>
        /// 创建一个新的PermissionLevel
        /// </summary>
        /// <returns>如果成功的话返回Result.Successful，如果失败的话返回Result.Failed， 如果该PermissionLevel的名字已经存在的话，返回Result.AlreadyExisted</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result AddPermissionLevel(PermissionLevelDto permissionLevel);

        /// <summary>
        /// 编辑PermissionLevel
        /// </summary>
        /// <returns>如果成功的话返回Result.Successful，如果失败的话返回Result.Failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result EditPermissionLevel(PermissionLevelDto permissionLevel);

        /// <summary>
        /// 获取已经创建的所有PermissionLevel
        /// </summary>
        /// <returns>如果不存在任何的PermissionLevel,返回一个Count为0的空列表</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionLevelDto> GetAllPermissionLevels();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionLevelDto> GetAllUpgradePermissionLevels();
        /// <summary>
        /// 通过type（system，tenant）获得对应的list 数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionLevelDto> GetDtosByPermissionType(PermissionLevelType type);

        /// <summary>
        /// 获取初始化的permissionGroup
        /// </summary>
        /// <returns>List</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionGroup> GetDefaultPermissionGroup(AveModuleSpecial aveModuleSpecial);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSystemPasswordPolicy(SystemPasswordPolicy policy);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        SystemPasswordPolicy GetSystemPasswordPolicy();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateSystemSecurityPolicy(SystemSecurityPolicy policy);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        SystemSecurityPolicy GetSystemSecurityPolicy();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionDto> GetPermissionsByAccountId(string userId);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionLevelDto> GetPermissionLevelsByAccountId(string userId);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<PermissionDto> GetPermissionsByAccountIds(IEnumerable<string> idArray);

        /// <summary>
        /// 获取所有的用户
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountMappingDto> GetAllAccounts();

        /// <summary>
        /// 批量删除Permission Level
        /// </summary>
        /// <param name="idArray">PermissionLevel的id集合，可以是数组，列表等</param>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        ResultMessage DeletePermissionLevels(IEnumerable<string> idArray);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<string> GetPermissionLevelUsage(string permissionId);

        /// <summary>
        /// 添加一个新的组，包括Super Admin和Security Trimming
        /// </summary>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，已经存在返回Result.AlreadyExisted，参数为null返回Result.ArgumentNull</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result AddGroup(GroupDto group);

        /// <summary>
        /// 编辑group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result EditGroup(GroupDto group);

        /// <summary>
        /// 根据Id获取一个组
        /// </summary>
        /// <returns>如果不存在返回null</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        GroupDto GetGroup(string id);

        ///// <summary>
        ///// 获取所有的Group，可以使用group.Permissions[i].PermissionLevel.ToString()来获取对应的permission描述
        ///// </summary>
        ///// <returns></returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<GroupDto> GetAllGroups();

        /// <summary>
        /// 获取所有的Group，可以使用group.Permissions[i].PermissionLevel.ToString()来获取对应的permission描述
        /// 和owner信息 但是会清空accounts list 来保存owner
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<GroupDto> GetAllGroupsWhitItsOwner();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<GroupDto> GetGroupsByType(IEnumerable<GroupType> types);

        /// <summary>
        /// 获取AddUser页面对应的数据
        /// </summary>
        /// <returns>包括全局的PwdPolicy和SecuritySetting以及所有的Super Admin组</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AddUserViewModel GetAddUserViewModel();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        PermissionViewModel GetPermissionViewModel();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        PermissionViewModel GetSuperPermissionViewModel();

        /// <summary>
        /// 添加Account
        /// </summary>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，如果要添加的组不存在返回Result.NotExist，如果用户已存在返回Result.AlreadyExisted</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        ResultMessage AddAccount(List<AccountDto> accounts);

        /// <summary>
        /// 添加Account
        /// </summary>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，如果用户已存在返回Result.AlreadyExisted</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        ResultMessage AddRegisterAccount(AccountDto account);

        /// <summary>
        /// 添加Invited Account
        /// </summary>
        /// <param name="accounts"></param>
        /// <returns>成功返回Result.Successful，失败返回Result.Failed，如果用户已存在返回Result.AlreadyExisted</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        ResultMessage AddInvitedAccount(List<AccountDto> accounts);

        /// <summary>
        /// 编辑Account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result EditAccount(AccountDto account);


        /// <summary>
        /// 编辑Account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result EditAccountForAccountManager(AccountDto account);

        ///// <summary>
        ///// 获取所有没有group的用户
        ///// </summary>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<AccountMappingDto> GetAllNoGroupAccounts();

        /// <summary>
        /// 根据ID获取Account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountMappingDto GetAccount(string id);

        /// <summary>
        /// 根据ID获取Account name，找不到返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string GetAccountName(string id);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountMappingDto GetAccountByNameAndAuthenticationType(string name, string type);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        LocalAccountDto GetLocalAccountByUserName(string userName);

        ///// <summary>
        ///// 获取User/Group Permissions页面对应的数据
        ///// </summary>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //UserOrGroupPermissionsViewModel GetUserOrGroupPermissionsViewModel();

        /// <summary>
        /// 根据groupId获取该组下的所有account。
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountMappingDto> GetAccountsByGroup(string groupId);

        /// <summary>
        /// 获取当前用户的所在组的全部成员
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountDto> GetCurrentGroupAccounts(string groupId);

        /// <summary>
        /// 根据部分用户名来查询用户
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountMappingDto> FindAccountByPartOfUserName(string userName);

        /// <summary>
        /// 对组和用户进行集体的权限更新，用于Edit Permission页面
        /// </summary>
        /// <returns>成功返回Result.Successful， 失败返回Result.Failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result UpdateAccountOrGroupPermission(IEnumerable<AccountMappingDto> accounts, IEnumerable<GroupDto> groups);

        /// <summary>
        /// 删除选定的组，只有不含有user的组才可以被删除
        /// </summary>
        /// <returns>列表中包含不允许删除的组的id</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<ErrorMessage> DeleteGroups(IEnumerable<string> idArray);

        /// <summary>
        /// 删除用户或组的权限
        /// </summary>
        /// <param name="idArray">user or group permissions view model</param>
        /// <returns>删除成功返回successful，失败返回failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result RemoveUserPermissions(UserOrGroupPermissionsViewModel model);

        /// <summary>
        /// 删除选定的用户
        /// </summary>
        /// <returns>成功返回Result.Successful， 失败返回Result.Failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result DeleteAccounts(IEnumerable<string> idArray);

        /// <summary>
        /// 删除选定的用户
        /// </summary>
        /// <returns>成功返回Result.Successful， 失败返回Result.Failed</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result DeleteInvitedAccounts(IEnumerable<string> idArray);

        ///// <summary>
        ///// 根据输入的关键字查找相应的Permissions，可以输入用户名，组名或者Email
        ///// </summary>
        ///// <param name="keyWord">username, groupname or email</param>
        ///// <returns>权限以及相关信息，具体内容请参见CheckPermissionsResult中的注释</returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<CheckPermissionsResult> CheckPermissions(string keyWord);

        /// <summary>
        /// 显示所有已经添加的domain
        /// </summary>
        /// <returns>所有的domain</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<DomainDto> GetAllDomains();

        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //DomainDto GetDomainByName(string domainName);

        /// <summary>
        /// 根据输入的字符串来判断在这个domain中是否存在该组或者用户。
        /// </summary>
        /// <param name="userOrGroupName">组名或者用户名，格式是domain\name</param>
        /// <returns>如果还没有添加该domain，返回DomainNotAddedInDB；如果该domain无法check通，返回NotFound；如果check的结果发现是个用户，则返回User；如果check的结果发现还是个组，则返回Group</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CheckUsersResult> CheckUserOrGroupExisted(List<CheckUsersMessage> checkUserMessages);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CheckUsersResult> FindUserOrGroup(List<CheckUsersMessage> checkUserMessages);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CheckUsersResult> CheckWindowsUserOrGroupExisted(List<string> searchNames);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CheckUsersResult> FindWindowsUserOrGroup(string searchName);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CheckUsersResult> CheckUsers(List<string> checkUserMessages);

        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<CheckUsersResult> FindUsers(List<string> checkUserMessages);

        /// <summary>
        /// 查看domain是否能check通
        /// </summary>
        /// <param name="domain">要进行check的domain</param>
        /// <returns>如果domain可以check通，返回domain的name(如avepoint.com返回avepoint)，这样在前台就需要将用户输入的字符串替换为返回的字符串；如果check不同则返回空字符串</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string CheckDomainAccessable(DomainDto domain);

        /// <summary>
        /// 更新Domain的用户名或密码
        /// </summary>
        /// <param name="domain">更改后的domain，带有id，密码为明文</param>
        /// <returns>如果该domain不存在于数据库中，返回NotExist；如果更新成功，返回Successful</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result UpdateDomain(DomainDto domain);

        /// <summary>
        /// 添加已存在的用户到组
        /// </summary>
        /// <param name="accountId">用户id</param>
        /// <param name="groupId">组id</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result AddUsertoGivenGroup(string accountId, string groupId);

        /// <summary>
        /// 根据user name获取Account.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>如果取不到Account,返回null</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountDto GetAccountDtoByUserName(string userName);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountMappingDto GetByObjectIdAndName(string objectId, string objectName);

        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //AccountMappingDto GetSingleUserByUserName(string userName);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountMappingDto> GetAccountsByType(IEnumerable<AvePoint.GCommon.Contract.AccountManager.Object.AccountType> type);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountMappingDto GetAccountByUserName(string userName);

        ///// <summary>
        ///// 禁用不活跃的用户
        ///// </summary>
        ///// <returns>执行成功，返回0</returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //int DisableInactivateUsers();

        /// <summary>
        /// 禁用指定的用户
        /// </summary>
        /// <param name="userNames">用户ID</param>
        /// <returns>执行成功，返回0</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        int DisableSpecificUsers(Dictionary<string,string> ids);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        int EnableSpecificUsers(Dictionary<string, string> ids);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<LogonHistoryDto> GetLogonHistoriesByUserName(string userName);


        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Result RemoveUsersFromGroup(string groupId, IEnumerable<string> userIds);

        /// <summary>
        /// 获取密码过期后的提示语
        /// </summary>
        /// <param name="accountId">用户ID</param>
        /// <param name="isPopup">是popup类型的为true，email类型为false</param>
        /// <returns>应该提示则返回提示语，不符合条件则返回null</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        string ShowNews(string accountId, bool isPopup);

        /// <summary>
        /// 获取密码是否过期
        /// Chengpan Sun Add
        /// [ADO-21151]system option页面，设置密码过期email提醒，当user已经过期时，邮件提醒内容不正确
        /// </summary>
        /// <param name="accountId">用户ID</param>
        /// <returns>过期返回true,没过期返回false</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool IsPasswordExpired(string accountId);


        /// <summary>
        /// Manage a batch of accounts, and return  accounts on effect. 
        /// </summary>
        /// <param name="changeModeTo"></param>
        /// <param name="objectIds"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountMappingDto> ManageAccountsByObjectIds(AccountMode changeModeTo, List<string> objectIds);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<string> GetUsingObjectIds(AvePoint.GCommon.Contract.AccountManager.Object.AccountType accountType);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<AccountDto> GetAllInactiveRegisterAccounts();

        #region For Gateway
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //List<LocalAccountDto> ExtendGetAllLocalSystemAccounts();

        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //void UpdateExpirationTimeAndUserSeat(AccountMappingDto accountMapping);

        #endregion

        #region For GUI

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<DomainDto> GetAllDomainsForGUI();
        #endregion

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void ResetPwdByAccountId(string accountId, string pwd);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateAccountbyAccountDto(LocalAccountDto user);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<IdAndLastChangeDto> GetAllUserIdAdnLastPwdChange();

        void EnableRegisterUser(string userId);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<LogonRecordDto> GetAllUserLogonRecords();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UnlockUsersTask();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, List<AccountDto>> ExtendGetAllAccounts();

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<LocalAccountDto> ExtendGetAllLocalSystemAccounts();
            
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, string> GetAllModuleDisplayNames();

        AccountLanguageType GetAccountLanguage(string accountId);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        void UpdateLocalAccountExtentionByUserName(string userName, string extention);
    }
}
