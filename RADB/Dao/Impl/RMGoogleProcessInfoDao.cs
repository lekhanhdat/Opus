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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMGoogleProcessInfoDao : RMNodeFlagDao, IRMGoogleProcessInfoDao
    {
        public void UpsertLastJobProcessTime(long lastRunTime, Guid containerId, Guid nodeId, int type)
        {
            using (var context = GetNewContext())
            {
                var exist = context.NodeFlag.Any(item => item.GroupId == containerId && item.NodeId == nodeId && item.NodeFlagType == type);
                if (!exist)
                {
                    var info = new RMNodeFlag
                    {
                        NodeId = nodeId,
                        GroupId = containerId,
                        CollectionTime = lastRunTime,
                        NodeFlagType = type,
                    };
                    Create(info, context);
                }
                else
                {
                    var info = context.NodeFlag.First(item => item.NodeId == nodeId && item.NodeFlagType == type);
                    info.CollectionTime = lastRunTime;
                    ApplyCurrentValues(context, info);
                }
            }
        }
    }
}