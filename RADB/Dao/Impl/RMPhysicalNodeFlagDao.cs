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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMPhysicalNodeFlagDao : BaseDao<RMPhysicalNodeFlag>, IRMPhysicalNodeFlagDao
    {
        public void AddPhysicalNodeInfo(RMPhysicalNodeFlag nodeInfo)
        {
            using (var ctx = GetNewContext())
            {
                if (!ctx.RMPhysicalNodeFlag.Any(s => s.NodeId == nodeInfo.NodeId && s.GroupId == nodeInfo.GroupId && s.NodeFlagType == nodeInfo.NodeFlagType))
                {
                    ctx.RMPhysicalNodeFlag.Add(nodeInfo);
                    ctx.SaveChanges();
                }
                else
                {
                    var entities = ctx.RMPhysicalNodeFlag.Where(s => s.NodeId == nodeInfo.NodeId && s.GroupId == nodeInfo.GroupId && s.NodeFlagType == nodeInfo.NodeFlagType).ToList();
                    foreach (var entity in entities)
                    {
                        entity.CollectionTime = nodeInfo.CollectionTime;
                        entity.FullPath = nodeInfo.FullPath;
                        entity.Title = nodeInfo.Title;
                        entity.IsRemoved = nodeInfo.IsRemoved;
                    }
                    BatchUpdate(entities);
                }
            }
        }

        public void DeletePhysicalNodeInfo(Guid nodeId, Guid groupId, int nodeType)
        {
            using (var ctx = GetNewContext())
            {
                var nodeInfo = ctx.RMPhysicalNodeFlag.Where(s => s.NodeId == nodeId && s.GroupId == groupId && s.NodeFlagType == nodeType);
                if (nodeInfo != null && nodeInfo.Count() > 0)
                {
                    this.BatchDelete(nodeInfo.ToList());
                }
            }
        }

        public RMPhysicalNodeFlag GetPhysicalNodeInfo(Guid nodeId, Guid groupId, int nodeType)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMPhysicalNodeFlag.Where(s => s.NodeId == nodeId && s.GroupId == groupId && s.NodeFlagType == nodeType).FirstOrDefault();
            }
        }

        public long GetCollectionTime(int type, Guid nodeId)
        {
            using (var ctx = GetNewContext())
            {
                var info = ctx.RMPhysicalNodeFlag.Where(s => s.NodeFlagType == type && s.NodeId == nodeId).FirstOrDefault();
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
