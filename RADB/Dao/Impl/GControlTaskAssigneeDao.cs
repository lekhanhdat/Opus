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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl;

public class GControlTaskAssigneeDao : BaseDao<GControlTaskAssigneeMapping>,IGControlTaskAssigneeDao
{
    public async Task<int> BatchSoftDeleteAsync(IEnumerable<int> ids)
    {
        using var context = GetNewContext();
        foreach (var id in ids)
        {
            var entity = new GControlTaskAssigneeMapping
            {
                Id = id
            };
            context.GControlUserTaskMapping.Attach(entity);
            entity.IsRemoved = true;
            context.Entry(entity).Property(e => e.IsRemoved).IsModified = true;
        }
        return await context.SaveChangesAsync();
    }
}