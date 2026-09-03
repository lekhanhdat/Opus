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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class PoolUserDao : BaseDao<RMPoolUser>, IPoolUserDao
    {
        public void UpdatePoolUserUsage(string userName, string tenantId, bool isAdd)
        {

            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var obj = ctx.PoolUser.Where(p => p.UserName == userName && p.TenantId == tenantId).FirstOrDefault();
                try
                {
                    if (obj != null)
                    {
                        if (isAdd)
                        {
                            obj.Usage += 1;
                            ApplyCurrentValues(ctx, obj);
                        }
                        //else
                        //{
                        //    if (obj.Usage > 0)
                        //    {
                        //        obj.Usage = obj.Usage - 1;
                        //        ApplyCurrentValues(ctx, obj);
                        //    }

                        //}

                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {

                    throw;
                }

            }

        }

        public void AddPoolUser(RMPoolUser user)
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var obj = ctx.PoolUser.Where(p => p.UserName == user.UserName && p.TenantId == user.TenantId).FirstOrDefault();

                if (obj == null)
                {
                    ctx.PoolUser.Add(user);
                    ctx.SaveChanges();
                }
            }
        }

        public RMPoolUser GetAvailableUser(string tenantId)
        {
            RMPoolUser user = null;
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                user = ctx.PoolUser.AsQueryable().Where(p => p.TenantId == tenantId && p.Status == 0).OrderBy(u => u.Usage).FirstOrDefault();
            }
            return user;
        }

        public RMPoolUser GetPoolUserByName(string tenantId, string userName)
        {
            RMPoolUser user = null;
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                user = ctx.PoolUser.AsQueryable().Where(p => p.TenantId == tenantId && p.Status == 0 && p.UserName.Equals(userName)).FirstOrDefault();
            }
            return user;
        }

        public void UpdatePoolUserStatus(string userName, string tenantId, int status)
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var obj = ctx.PoolUser.Where(p => p.UserName == userName && p.TenantId == tenantId).FirstOrDefault();
                try
                {
                    if (obj != null)
                    {

                        obj.Status = status;
                        ApplyCurrentValues(ctx, obj);

                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {

                    throw;
                }
            }
        }
    }
}
