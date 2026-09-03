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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class TermUsageReportDao : BaseDao<RMProfile>, ITermUsageReportDao
    {
        public async Task<bool> Create(RMProfile profile)
        {
            using (var context = GetNewContext())
            {
                profile.CreateProfileLogonUserId = TenantLocalValue.LogonUserId;
                profile.Modified = DateTime.UtcNow.Ticks;
                context.Profile.Add(profile);
                var effectCount = await context.SaveChangesAsync();
                return effectCount > 0;
            }
        }

        public async Task<bool> Edit(RMProfile profile)
        {
            using (var context = GetNewContext())
            {
                var existResult = await context.Profile.FirstOrDefaultAsync(item => item.Id == profile.Id && !item.IsRemoved);
                if (existResult == null)
                {
                    throw new Exception($"Can't find term usage profile: [{profile.Name}] by: [{profile.Id}]");
                }

                existResult.Name = profile.Name;
                existResult.Description = profile.Description;
                existResult.Extension1 = profile.Extension1;
                existResult.Extension2 = profile.Extension2;
                existResult.Modified = DateTime.UtcNow.Ticks;

                var effectCount = await context.SaveChangesAsync();
                return effectCount > 0;
            }
        }

        public async Task<bool> Delete(int id)
        {
            using (var context = GetNewContext())
            {
                var existResult = await context.Profile.FindAsync(id);
                if (existResult == null)
                {
                    throw new Exception($"Can't find term usage profile by: [{id}]");
                }

                existResult.IsRemoved = true;

                var effectCount = await context.SaveChangesAsync();
                return effectCount > 0;
            }
        }

        public async Task<bool> RealDelete(int id)
        {
            using (var context = GetNewContext())
            {

                var existResult = await context.Profile.FindAsync(id);
                if (existResult == null)
                {
                    throw new Exception($"Can't find term usage profile by: [{id}]");
                }

                context.Set<RMProfile>().Attach(existResult);
                context.Entry(existResult).State = EntityState.Deleted;

                var effectCount = await context.SaveChangesAsync();
                return effectCount > 0;
            }
        }

        public async Task<RMProfile> Get(int id)
        {
            using (var context = GetNewContext())
            {
                var existResult = await context.Profile.FirstOrDefaultAsync(item => item.Id == id && !item.IsRemoved);
                if (existResult == null)
                {
                    throw new Exception($"Can't find term usage profile by: [{id}]");
                }

                return existResult;
            }
        }
    }
}
