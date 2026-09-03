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
using System.Threading.Tasks;
using ExchangeBackupUtility;
using ExchangeUtility;
using AvePoint.RA.RAExchange.Authorization;
using AvePoint.RA.Contract.Object;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.RAExchange.Common;

namespace AvePoint.RA.RAExchange.RMCollectionData
{
    public class RMEXOSyncDataIncJob : RMEXOSyncDataJobBase
    {
        public RMEXOSyncDataIncJob(ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement) : base(treeNode, jobManagement)
        {
        }

        protected override IEnumerable<ExchangeFolder> GetFolders(ExchangeFolder folder)
        {
            if (folder == null)
            {
                var address = TreeManagement.GetMailboxNode(TreeNodeDto).Name;
                yield return new ExchangeRootFolder(new ExchangeMailbox(address, ExchangeMailboxType.User), AuthorizationManager.Instance.GetAuthObject(address));
            }
            else
            {
                foreach (var f in folder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note"))
                {
                    //在返回Folder 的时候需要计算一下当前Folder 的SyncState，来保证Folder 下次的Inc job 能根据Sync state 进行inc
                    f.GenerateCurrentSyncState();
                    yield return f;
                }
            }
        }

        protected  IEnumerable<ExchangeItem> GetItems(ExchangeFolder folder)
        {
            var nodeInfo = EXONodeInfoDao.GetEXONodeInfo(folder.FolderId.ToMd5(), GroupId, (int)NodeFlagType.ExplorerSync);
            return GetSubItemsAsync(folder, nodeInfo?.ItemSyncState).GetConsumingEnumerable();
        }
    }
}
