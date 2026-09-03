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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox
{
    public interface IMailboxService
    {

        /// <summary>
        /// 创建User Group级别的的Mailbox Group
        /// </summary>
        /// <param name="groupId">目标Group</param>
        /// <param name="mailGroup"></param>
        /// <param name="permissionType">mailGroup在User Group中的权限</param>
        void CreateGroupScopeMailboxGroup(string groupId, EmailAccountGroupDto mailGroup, EntityObjectPermissionType permissionType);

        /// <summary>
        /// 批量删除Mailbox
        /// </summary>
        /// <param name="ids">mailbox id list</param>
        /// <returns>删除的数量</returns>
        void DeleteMailbox(List<string> ids);

        void DeleteMailboxByNames(List<string> names);

        void DeleteMailboxByParentIds(List<string> parentIds);

        List<string> GetMailboxNamesByParentIds(List<string> parentIds);

        /// <summary>
        /// 批量删除Mailbox Group
        /// </summary>
        /// <param name="ids">mailbox group id list</param>
        /// <returns>删除的数量</returns>
        void DeleteMailboxGroup(List<string> ids);

        /// <summary>
        /// 更新Mailbox Group信息
        /// </summary>
        /// <param name="mailGroup"></param>
        void UpdateMailboxGroup(EmailAccountGroupDto mailGroup);

        /// <summary>
        /// 获取当前用户有权限的Mailbox
        /// </summary>
        /// <returns></returns>
        List<EmailAccountDto> GetAuthorisedMailboxes();

        /// <summary>
        /// 获取当前用户有权限Mailbox Groups
        /// </summary>
        /// <returns></returns>
        List<EmailAccountGroupDto> GetMailboxGroups();

        /// <summary>
        /// 按Id查询Mailbox
        /// </summary>
        /// <param name="mailId">mailbox id</param>
        /// <returns>EmailAccountDto or null</returns>
        EmailAccountDto GetMailboxById(string mailId);

        /// <summary>
        /// 获取在Mailbox Group下有权限的Mailbox
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        List<EmailAccountDto> GetMailboxesByGroupId(string groupId);

        /// <summary>
        /// 在当前Group中检查是否已经存在相同Name的Mailbox Group for update
        /// </summary>
        /// <param name="address"></param>
        /// <returns>
        /// 存在返回true
        /// 否则返回false
        /// </returns>
        bool IsMailboxGroupExistByNameForUpdate(string name, string id);

        bool IsO365GroupItemExist(string name);

        /// <summary>
        /// 根据Account Id获取有权限的Mailbox
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        List<EmailAccountDto> GetAuthorisedMailboxesByUser(string accountId, bool isUserInGroup = false);

        List<EmailAccountDto> GetEmailsByEmailGroupIdForBrowse(string groupId);

        List<EmailAccountDto> GetAuthorisedMailboxesWithPermission(string accountId, bool withoutPermission, bool isCheckStandUser = false);
        void SyncMailboxs(List<EmailAccountDto> mails);
        void CreateMailboxGroups(List<EmailAccountGroupDto> mailGroups);

        List<RemoteNodePara> GetRemoteMailGroupNodes();

        List<SyncRemoteNodePara> GetAllMailboxNodes();

        void CreateGroupScopeO365GroupsGroup(string groupId, EmailAccountGroupDto group, EntityObjectPermissionType permissionType);

        List<EmailAccountGroupDto> GetO365GroupsGroups();

        List<EmailAccountDto> GetO365GroupsByO365GroupsGroupIdForBrowse(string groupId);

        EmailAccountDto GetO365GroupById(string id);

        List<EmailAccountDto> GetAllO365GroupItems();

        EmailAccountGroupDto CreateOrUpdateO365Group(EmailAccountGroupDto group);

        int DeleteO365Groups(IEnumerable<string> ids);

        int CreateO365GroupItems(BposUserAccountInfo Account, List<EmailAccountDto> items);

        List<EmailAccountGroupDto> GetAllAuthorisedMailboxGroups();

        void UpdateSyncMails(List<SyncRemoteNodePara> mails);

        Dictionary<string, ExchangeOnlineTreeNodeDto> GetRemoteMailsByIds(List<string> ids);

        bool UpdateAppTokenMailUserName(List<EmailAccountDto> mails);

        List<EmailAccountDto> GetAvailableEmailsByParent(string parentId);

        List<EmailAccountDto> GetMailboxesByEmailAddressName(List<string> addressNameList);
        List<EmailAccountDto> GetMailboxesByEmailAddressNameWithoutEncryption(List<string> addressNameList);

        RemoteNodePara GetMailGroupByNameAndNodeLevel(string name, int nodeLevel);
    }
}
