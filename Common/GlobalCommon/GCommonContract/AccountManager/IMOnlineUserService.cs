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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Wcf;

namespace AvePoint.GCommon.Contract.AccountManager
{
    using System;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Login;
    using AvePoint.GCommon.Contract.Server.UserRegister;

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMOnlineUserService
    {
        /// <summary>
        /// 添加用户并且编辑权限
        /// </summary>
        /// <param name="tenant">UserIds|Permissions|RoleType</param>
        /// <returns>Status|Message</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto AddUsersAndEditPermission(OnlineUserDataDto online);
        ///// <summary>
        ///// 发送邀请邮件(如邀请用户请使用 InvitedUser) For Service
        ///// </summary>
        ///// <param name="users">用户发送</param>
        ///// <param name="online">CurrentAccountMapping</param>
        ///// <returns></returns>
        //[OperationContract]
        //[FaultContract(typeof(WcfException))]
        //void SendInvitedEmail(List<AccountDto> users, OnlineUserDataDto online);

        /// <summary>
        /// 添加账户(如邀请用户请使用 InvitedUser) For Service
        /// </summary>
        /// <param name="users">添加账户</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        ResultMessage AddAccount(List<AccountDto> users);

        /// <summary>
        /// 删除用户 For 前台
        /// </summary>
        /// <param name="ids">用户id数组</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto DeleteAccount(List<String> ids);

        /// <summary>
        /// 邀请用户 For 前台
        /// </summary>
        /// <param name="online">CurrentAccountMapping|Accounts(Emails)|Schema|Host|Port</param>
        /// <returns>SuccAccounts(成功账户)|ExsitedAccounts(存在账户)|FailedAccounts(失败账户)</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto InvitedUser(OnlineUserDataDto online);

        /// <summary>
        /// 邀请用户 For 前台 Portal Api
        /// </summary>
        /// <param name="online">CurrentAccountMapping|Accounts(Emails)|Schema|Host|Port</param>
        /// <returns>SuccAccounts(成功账户)|ExsitedAccounts(存在账户)|FailedAccounts(失败账户)</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto InvitedSupportUser(OnlineUserDataDto online);

        /// <summary>
        /// 获取Plan和SiteCollection关联关系
        /// </summary>
        /// <param name="online">PlanIds|SiteCollectionIds</param>
        /// <returns>Status|PlanSiteCollectionMapping</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, List<string>> GetPlanSiteCollectionMapping(List<string> planIds);

        /// <summary>
        /// 查询用户 For 前台
        /// </summary>
        /// <param name="online">CurrentAccountMapping</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto GetAccountsByGroup(OnlineUserDataDto online);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto GetAccountsByGroup2(OnlineUserDataDto dto, Boolean is4ObjectView = false);

        /// <summary>
        /// 用户查找 For Service
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountDto FindAccountById(string userId);


        /// <summary>
        /// 设置用户权限 For 前台
        /// </summary>
        /// <param name="online">UserId|CurrentAccountMapping</param>
        /// <returns>Status|Message</returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        OnlineUserResultDto EditUserPermission(OnlineUserDataDto online);

        /// <summary>
        /// 获取Invite Support的Name
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        String GetSupportName();

        /// <summary>
        /// 在删除Remote site collection前判断占用它的plan，并给出提示语
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        UsingPlanInfoDto GetUsingPlanByRemoteSiteCollectionIds(List<string> siteCollectionIds);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        bool CreateRegisterUserForPortal(AccountMappingDto userInfo, string groupId, int country);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountDto CreateInviteUserForPortal(AccountMappingDto userInfo, string groupId, string password);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, string> GetPortalNavigationInfo(PortalLoginModel user);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        Dictionary<string, string> GetProductNavigationInfo(CurrentUserModel user);

        /// <summary>
        /// 创建Tenant Group
        /// 初始化Group相关信息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="ownerEmail"></param>
        /// <returns>
        /// true   创建成功
        /// false  创建失败
        /// </returns>
        bool InitTenantGroup(string groupId, string ownerEmail);

        /// <summary>
        /// 创建Admin Group
        /// 初始化Group相关信息
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>
        /// true   创建成功
        /// false  创建失败
        /// </returns>
        bool InitAdminGroup(string groupId);

        /// <summary>
        /// 创建User并创建Role
        /// 如果是Owner则会同时SetOwner(将其他owner降为PowerUser)
        /// 如果User已经存在则抛出异常
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="role"></param>
        /// <param name="country"></param>
        /// <returns>
        /// account mapping id
        /// </returns>
        string CreateUserForPortal(string groupId, string userId, string userName, ObjectRoleType role, int country);

        [OperationContract]
        [FaultContract(typeof(WcfException))]
        AccountDetailDto GetLicenseInfo(string groupId);

        [OperationContract]
        string GetSwitchBarByUser(string userId, string language);

        OnlineUserResultDto GetAllUsersForAM(OnlineUserDataDto user);
        OnlineUserResultDto GetAllObjectsForAM();
        OnlineUserResultDto GetObjectsForAM(string userId, List<UserGroup> userGroups, bool isCheckStandUser);
        List<string> GetObjectIdsForAM(string userId);
    }
}
