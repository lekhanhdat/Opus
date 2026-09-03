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
    public class RMAzureFileShareSyncJobProcessInfoDao : BaseDao<RMAzureFileShareSyncJobProcessInfo>, IRMAzureFileShareSyncJobProcessInfoDao
    {
        public long GetLastJobProcessTime(Guid scopeId, string fullPath)
        {
            using(var context = GetNewContext())
            {
                var info = context.RMAzureFileShareSyncJobProcessInfoes.FirstOrDefault(item => item.ScopeId == scopeId && item.FullPath == fullPath);
                if(info == null)
                {
                    return DateTime.MinValue.Ticks;
                }
                return info.LastJobProcessTime;
            }
        }

        public void UpsertLastJobProcessTime(Guid scopeId, string fullPath)
        {
            using(var context = GetNewContext())
            {
                var exist = context.RMAzureFileShareSyncJobProcessInfoes.Any(item => item.ScopeId == scopeId && item.FullPath == fullPath);
                if(!exist)
                {
                    var info = new RMAzureFileShareSyncJobProcessInfo
                    {
                        Id = Guid.NewGuid(),
                        ScopeId = scopeId,
                        LastJobProcessTime = DateTime.UtcNow.Ticks,
                        FullPath = fullPath,
                    };
                    Create(info, context);
                }
                else
                {
                    var info = context.RMAzureFileShareSyncJobProcessInfoes.First(item => item.ScopeId == scopeId && item.FullPath == fullPath);
                    info.LastJobProcessTime = DateTime.UtcNow.Ticks;
                    ApplyCurrentValues(context, info);
                }
            }
        }
    }
}
