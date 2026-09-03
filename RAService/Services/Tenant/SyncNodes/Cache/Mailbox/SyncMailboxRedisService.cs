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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache
{
    public class SyncMailboxRedisService : AbstractSyncNodeRedisService, ISyncMailboxRedisService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SyncMailboxRedisService));
        private IRMMailboxService MailboxService => PlatformWindsorManager.GetService<IRMMailboxService>();

        #region ISyncMailboxRedisService
        public void InsertMailbox(EmailAccountDto mailbox, Action sqlAction)
        {
            if (mailbox == null)
            {
                logger.Info("No mailbox to insert into redis.");
                return;
            }
            var caches = new List<SyncRemoteNodePara> { SyncDataConverter.ConvertDBNodeModelToCacheModel(mailbox) };
            AddNodesToCache(TenantLocalValue.LogonGroupId, ConvertCacheListToDict(caches), sqlAction);
            logger.Info("Insert mailboxes to redis successfully.");
        }

        public void DeleteMailboxes(List<string> mailboxes, Action sqlAction, bool ignoreCase = true)
        {
            if (mailboxes == null || mailboxes.Count == 0)
            {
                logger.Info("No mailboxes to be deleted.");
                return;
            }
            DeleteNodesFromCache(TenantLocalValue.LogonGroupId, mailboxes, sqlAction, ignoreCase);
            logger.Info("Delete mailboxes to redis successfully.");
        }

        public void UpdateMailbox(EmailAccountDto mailbox, Action sqlAction)
        {
            if (mailbox == null)
            {
                logger.Info("No mailbox to update into redis.");
                return;
            }
            var caches = new List<SyncRemoteNodePara> { SyncDataConverter.ConvertDBNodeModelToCacheModel(mailbox) };
            UpdateNodesToCache(TenantLocalValue.LogonGroupId, ConvertCacheListToDict(caches), sqlAction);
            logger.Info("Update mailboxes to redis successfully.");
        }

        public void UpdateMailboxes(List<EmailAccountDto> mailboxes, Action sqlAction)
        {
            if (mailboxes == null || mailboxes.Count == 0)
            {
                logger.Info("No mailboxes to be updated.");
                return;
            }
            var caches = mailboxes.ConvertAll(SyncDataConverter.ConvertDBNodeModelToCacheModel);
            UpdateNodesToCache(TenantLocalValue.LogonGroupId, ConvertCacheListToDict(caches), sqlAction);
        }

        private Dictionary<string, SyncRemoteNodePara> ConvertCacheListToDict(List<SyncRemoteNodePara> list)
        {
            var dict = new Dictionary<string, SyncRemoteNodePara>();
            foreach (SyncRemoteNodePara node in list)
            {
                dict.Add(node.NodeName, node);
            }
            return dict;
        }
        #endregion

        #region AbstractSyncNodeRedisService
        protected override string GenerateCacheKey(string tenantGroupId)
        {
            return RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.Mailbox);
        }

        protected override string GenerateFieldKeyForGroup(RMCompatibleRemoteNode aosSyncNode)
        {
            //return RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(aosSyncNode);
            return RedisFieldKeyUtil.GenerateMailboxGroupFieldKeyByAosId(aosSyncNode);
        }

        protected override string GenerateFieldKeyForGroup(RemoteNodePara daoGroup)
        {
            //return RedisFieldKeyUtil.GenerateMailboxGroupFieldKey(daoGroup);
            return RedisFieldKeyUtil.GenerateMailboxGroupFieldKeyByAosId(daoGroup);
        }

        protected override List<RemoteNodePara> GetAllGroupsInDB()
        {
            var groups = new List<RemoteNodePara>();
            try
            {
                groups = MailboxService.GetRemoteMailGroupNodes();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get all groups. Exception is {0}.", ex.ToString());
            }
            return groups;
        }

        protected override IEnumerable<List<SyncRemoteNodePara>> GetAllNodesInDBByPage()
        {
            var pageSize = 1000;
            for (var pageIndex = 0; ; pageIndex++)
            {
                var res = MailboxService.GetAllMailboxNodesByPage(pageIndex, pageSize);
                if (!res.Any())
                {
                    yield break;
                }

                yield return res;
            }
        }

        //protected override List<SyncRemoteNodePara> GetAllNodesInDB()
        //{
        //    var nodes = new List<SyncRemoteNodePara>();
        //    try
        //    {
        //        nodes = MailboxService.GetAllMailboxNodes();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to get all nodes. Exception is {0}.", ex.ToString());
        //    }
        //    return nodes;
        //}
        #endregion
    }
}
