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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount
{
    public interface IMManagedAccountService
    {
        OpertionResult AddAccountProfile(AccountProfileDto profileDto);

        OpertionResult UpdateAccountProfile(AccountProfileDto profileDto);

        AccountProfileDto GetAccountProfileById(string id);

        string UpgradeAccountProfile(string username, string password, ObjectInfoDto objectInfo = null);

        List<AccountProfileDto> GetAllAccountProfiles();

        bool ValidateAccountDataByAgentId(string agentId, string username, string password);

        /// <summary>
        /// 用于Tenant User的account profile升级
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码密文（通信加密）</param>
        /// <param name="createUserId">使用account profile的user id</param>
        /// <returns>升级后的account profile id</returns>
        /// <exception cref="AveException">如果createUserId为空或是不属于一个docave group，将会抛出AveException</exception>
        string UpgradeAccountProfile(string username, string password, string createUserId);

        /// <summary>
        /// Delete profiles, by API Zhongqi.Xu@avepoint.com
        /// </summary>
        /// <param name="profileDtos"></param>
        /// <returns></returns>
        OpertionResult DeleteAccountProfiles(List<AccountProfileDto> profileDtos);
        /// <summary>
        /// Add without validate, by API zhongqi.xu@avepoint.com
        /// </summary>
        /// <param name="profileDto"></param>
        /// <returns></returns>
        OpertionResult AddAccountProfileWithOutValidate(AccountProfileDto profileDto);
        /// <summary>
        /// Update without validate, by API zhongqi.xu@avepoint.com
        /// </summary>
        /// <param name="profileDto"></param>
        /// <returns></returns>
        OpertionResult UpdateAccountProfileWithoutValidate(AccountProfileDto profileDto);

        /// <summary>
        /// load balance环境，当account信息发生修改时，通知其他control service修改缓存内容
        /// </summary>
        /// <param name="profileDto"></param>
        /// <param name="pwdCrc"></param>
        void AddOrUpdateCacheAccountProfileAndSyncPwdCrc(AccountProfileDto profileDto, string pwdCrc);

        /// <summary>
        /// load balance环境，当account信息发生修改时，通知其他control service修改缓存内容
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="pwdCrc"></param>
        void DeleteCacheAccountProfileAndSyncPwdCrc(string profileId, string pwdCrc);

        /// <summary>
        /// load balance环境，当account信息发生修改时，通知其他control service修改缓存内容
        /// </summary>
        /// <param name="profileIds"></param>
        /// <param name="pwdCrc"></param>
        void DeleteCacheAccountProfileAndSyncPwdCrc(List<string> profileIds, string pwdCrc);
    }
}
