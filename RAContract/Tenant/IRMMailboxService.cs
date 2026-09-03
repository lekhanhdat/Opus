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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Tenant
{
    public interface IRMMailboxService
    {
        List<RemoteNodePara> GetRemoteMailGroupNodes();

        List<SyncRemoteNodePara> GetAllMailboxNodesByPage(int pageIndex, int pageSize);

        List<SyncRemoteNodePara> GetAllMailboxNodes();
        List<EmailAccountDto> GetAllMailboxNodesWithId();

        int GetMailboxNodesCount();

        Dictionary<string, string> GetMailboxNamesByParentIds(List<string> parentIds);

        Dictionary<string, string> GetParentNamesByMailboxes(IEnumerable<string> mailboxNames, bool includeO365Group = false);

        RemoteNodePara GetMailGroupByNameAndNodeLevel(string name, int nodeLevel);

        RemoteNodePara GetMailGroupByAosIdAndNodeLevel(string aosId, int nodeLevel);

        List<EmailAccountDto> GetMailboxesByEmailAddressNameWithoutEncryption(List<string> addressNameList);

        void CreateMailboxGroups(List<EmailAccountGroupDto> mailGroups);

        void UpdateEmailGroups(List<EmailAccountGroupDto> emailGroups);

        void DeleteMailboxByNames(List<string> names);

        void DeleteMailboxByParentIds(List<string> parentIds);

        void DeleteMailboxGroup(List<string> ids);

        void SyncMailboxs(List<EmailAccountDto> mails);

        void UpdateSyncMails(List<SyncRemoteNodePara> mails);

        List<EmailAccountDto> GetEmailsByEmailGroupIdForBrowse(string emailGroupId);

        ExchangeOnlineTreeNodeDto GetExchangeNodeByIdAndAddress(string id, string address);

        List<EmailAccountDto> GetMailboxesByEmailAddressName(List<string> addressNameList);

        EmailAccountDto GetMailboxById(string mailId);

        BposInfo GetBPOSInfoByExchangeNode(ExchangeOnlineTreeNodeDto treeDto);
        BposInfo GetBPOSInfoById(string tenantId);
    }
}
