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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class EXONodeFlagDao : BaseDao<EXONodeFlag>, IEXONodeFlagDao
    {
        public void AddEXONodeInfo(EXONodeFlag nodeInfo)
        {
            using (var ctx = GetNewContext())
            {
                //NodeID存储的是经过处理的AOS MailboxID，对于普通Mailbox和In Place Archiver Mailbox，两者NodeID 一致，需要添加AOSObjectId逻辑判断是否是同一个Mailbox，避免两个Mailbox共用一个ID.
                if (!ctx.EXONodeFlag.Any(s => s.NodeId == nodeInfo.NodeId && s.GroupId == nodeInfo.GroupId && s.NodeFlagType == nodeInfo.NodeFlagType && s.AOSObjectId == nodeInfo.AOSObjectId))
                {
                    ctx.EXONodeFlag.Add(nodeInfo);
                    ctx.SaveChanges();
                }
                else
                {
                    var entities = ctx.EXONodeFlag.Where(s => s.NodeId == nodeInfo.NodeId && s.GroupId == nodeInfo.GroupId && s.NodeFlagType == nodeInfo.NodeFlagType && s.AOSObjectId == nodeInfo.AOSObjectId).ToList();
                    foreach (var entity in entities)
                    {
                        entity.CollectionTime = nodeInfo.CollectionTime;
                        entity.FullPath = nodeInfo.FullPath;
                        entity.Title = nodeInfo.Title;
                        entity.IsRemoved = nodeInfo.IsRemoved;
                        entity.ItemSyncState = nodeInfo.ItemSyncState;
                        entity.FolderSyncState = nodeInfo.FolderSyncState;
                        entity.AOSObjectId = nodeInfo.AOSObjectId;
                    }
                    BatchUpdate(entities);
                }
            }
        }

        public void DeleteEXONodeInfo(Guid nodeId, Guid groupId, int nodeType)
        {
            using (var ctx = GetNewContext())
            {
                var nodeInfo = ctx.EXONodeFlag.Where(s => s.NodeId == nodeId && s.GroupId == groupId && s.NodeFlagType == nodeType);
                if (nodeInfo != null && nodeInfo.Count() > 0)
                {
                    this.BatchDelete(nodeInfo.ToList());
                }
            }
        }

        public EXONodeFlag GetEXONodeInfo(Guid nodeId, Guid groupId, int nodeType)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.EXONodeFlag.AsNoTracking().Where(s=> s.NodeId == nodeId && s.GroupId == groupId && s.NodeFlagType == nodeType).FirstOrDefault();
            }
        }

        public EXONodeFlag GetEXONodeInfoByAOSMailboxIdAndObjectId(Guid AOXMailboxId, Guid groupId, int nodeType, string AOSObjectId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.EXONodeFlag.Where(s => s.NodeId == AOXMailboxId && s.GroupId == groupId && s.NodeFlagType == nodeType && s.AOSObjectId == AOSObjectId).FirstOrDefault();
            }
        }

        public long GetCollectionTime(int type, Guid mailBoxId, string emailAddress)
        {
            using (var ctx = GetNewContext())
            {
                var info = ctx.EXONodeFlag.Where(s => s.NodeFlagType == type && s.AOSEmailboxId == mailBoxId && s.EmailAdress == emailAddress).FirstOrDefault();
                if (info != null)
                {
                    return info.CollectionTime;
                }
                else
                {
                    return DateTime.MinValue.Ticks;
                }
            }
        }
    }
}
