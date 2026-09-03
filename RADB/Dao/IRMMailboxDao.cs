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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMMailboxDao
    {
        /// <summary>
        /// 批量删除Email
        /// </summary>
        /// <param name="idz">email id list</param>
        /// <returns>删除的数量</returns>
        void DeleteEmails(List<string> ids);

        /// <summary>
        /// 批量删除Email Group
        /// </summary>
        /// <param name="ids">email group id list</param>
        /// <returns>删除的数量</returns>
        void DeleteEmailGroups(List<string> ids);

        /// <summary>
        /// For debug/test
        /// </summary>
        void ClearAll();

        void AddEmailsForAutoScan(List<EmailAccountDto> emails);

        void CreateEmailGroups(List<EmailAccountGroupDto> emailGroups);

        void UpdateEmailGroups(List<EmailAccountGroupDto> emailGroups);

        void DeleteMailboxByNames(List<string> names);

        void DeleteMailboxByParentIds(List<string> parentIds);

        List<RMMailbox> GetAllContainers();

        void UpdateContainers(List<RMMailbox> containers);

        Dictionary<string, string> GetMailboxNamesByParentIds(List<string> parentIds);

        Dictionary<string, string> GetParentNamesByMailboxes(IEnumerable<string> mailboxNames, bool includeO365Group = false);

        List<RemoteNodePara> GetRemoteMailGroupNodes();

        List<SyncRemoteNodePara> GetAllMailboxNodesByPage(int pageIndex, int pageSize);

        List<SyncRemoteNodePara> GetAllMailboxNodes();
        List<EmailAccountDto> GetAllMailboxNodesWithId();

        int GetMailboxNodesCount();

        List<EmailAccountDto> GetMailboxesByEmailAddressName(List<string> addressNameList);

        RemoteNodePara GetMailGroupByNameAndNodeLevel(string name, int nodeLevel);

        RemoteNodePara GetMailGroupByAosIdAndNodeLevel(string aosId, int nodeLevel);

        void UpdateSyncMails(List<SyncRemoteNodePara> mails);

        List<EmailAccountDto> GetEmailsByEmailGroupIdForBrowse(string emailGroupId);

        EmailAccountDto GetEmailByEmailGroupId(string emailGroupId);

        EmailAccountDto GetEmailById(string id);

        EmailAccountDto GetEmailByEmailAddress(string emailAddress);

        List<EmailAccountDto> GetEmailByIds(List<string> ids);

        EmailAccountDto GetO365GroupById(string id);

        EmailAccountDto GetEmailGroupById(string id);
    }
}