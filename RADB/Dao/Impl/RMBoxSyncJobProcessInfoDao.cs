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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMBoxSyncJobProcessInfoDao : RMNodeFlagDao, IRMBoxSyncJobProcessInfoDao
    {
        public Tuple<long, string> GetCollectionTimeAndStreamPosition(int type, Guid groupId, Guid nodeId)
        {
            using (var ctx = GetNewContext())
            {
                var info = ctx.NodeFlag.AsNoTracking().Where(s => s.NodeFlagType == type && s.GroupId == groupId && s.NodeId == nodeId).FirstOrDefault();
                if (info != null)
                {
                    return new Tuple<long, string> (info.CollectionTime,info.StreamPosition);
                }
                else
                {
                    return new Tuple<long, string>(DateTime.MinValue.Ticks, "now");
                }
            }
        }

        public void UpsertLastJobProcessTime(string streamPosition, Guid groupId, Guid scopeId)
        {
            using(var context = GetNewContext()) 
            {
                var exist = context.NodeFlag.Any(item => item.GroupId == groupId && item.NodeId == scopeId && item.NodeFlagType == (int)NodeFlagType.BoxSync);
                if(!exist)
                {
                    var info = new RMNodeFlag
                    {
                        NodeId = scopeId,
                        GroupId = groupId,
                        CollectionTime = DateTime.UtcNow.Ticks,
                        NodeFlagType = (int)NodeFlagType.BoxSync,
                        StreamPosition = streamPosition,
                    };
                    Create(info, context);
                }
                else
                {
                    var info = context.NodeFlag.First(item => item.NodeId == scopeId && item.NodeFlagType == (int)NodeFlagType.BoxSync);
                    info.CollectionTime = DateTime.UtcNow.Ticks;
                    info.StreamPosition = streamPosition;
                    ApplyCurrentValues(context, info);
                }
            }
        }
    }
}
