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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object.Session;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSessionDao : BaseDao<RMSession>, IRMSessionDao
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMSessionDao));
        public bool Exist(Guid sessionId)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    return ctx.RMSession.Any(o => o.Id == sessionId);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while checking the session with sessionId '{sessionId}'. error: {e.ToString()}");
                return false;
            }
        }
        public async Task SetExpireAsync(Guid Id, TimeSpan duration) 
        {
            using (var ctx = this.GetNewContext())
            {
                if (ctx.RMSession.Any(s => s.Id == Id))
                {
                    var session = await ctx.RMSession.Where(s => s.Id == Id).FirstAsync();
                    session.Expiration = DateTime.UtcNow.Add(duration);
                    await ctx.SaveChangesAsync();
                }
            }
        }
        public async Task SetExpireAsync(string userId, TimeSpan duration)
        {
            using (var ctx = this.GetNewContext())
            {
                if (ctx.RMSession.Any(s => s.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var sessionList = await ctx.RMSession.Where(s => s.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)).ToListAsync();
                    foreach (var item in sessionList)
                    {
                        item.Expiration = DateTime.UtcNow.Add(duration);
                    }
                    
                    await ctx.SaveChangesAsync();
                }
            }
        }
        public void RemoveByExpiration(DateTime time)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    var session = ctx.RMSession.Where(s => s.Expiration < time);
                    ctx.RMSession.RemoveRange(session);
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while removing expired sessions. error: {e.ToString()}");
            }
        }

        public void RemoveBySessionIdAndExpiredTime(Guid sessionId, DateTime time)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    if (ctx.RMSession.Any(s => s.Id == sessionId)) 
                    {
                        var session = ctx.RMSession.Where(s => s.Id == sessionId || s.Expiration < time);
                        ctx.RMSession.RemoveRange(session);
                        ctx.SaveChanges();

                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while removing expired sessions byId:{sessionId}. error: {e.ToString()}");
            }
        }

        public void RemoveByUserId(string userId)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    if (ctx.RMSession.Any(s => s.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)))
                    {
                        var session = ctx.RMSession.Where(s => s.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
                        ctx.RMSession.RemoveRange(session);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while removing sessions by userId:{userId}. error: {e}");
            }
        }

        public async Task<int> Remove(Guid Id)
        {
            using (var ctx = this.GetNewContext())
            {
                if (ctx.RMSession.Any(s => s.Id == Id))
                {
                    var session = await ctx.RMSession.Where(s => s.Id == Id).FirstAsync();
                    ctx.RMSession.Remove(session);
                    return await ctx.SaveChangesAsync();
                }
            }
            return -1; 
        }

        public async Task<RMSession> GetAsync(Guid Id)
        {
            using (var ctx = this.GetNewContext())
            {
               return await ctx.RMSession.Where(s => s.Id == Id).FirstOrDefaultAsync();
            }
        }
        public async Task<List<RMSession>> ListAsync(string userId)
        {
            using (var ctx = this.GetNewContext())
            {
                return await ctx.RMSession.Where(s => s.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToListAsync();
            }
        }
        public void Add(RMSessionDto dto)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    var session = ctx.RMSession.Find(dto.Id);
                    if (session == null)
                    {
                        session = new RMSession()
                        {
                            Id = dto.Id,
                            Expiration = dto.Expiration,
                            UserId = dto.UserId
                        };
                        ctx.RMSession.Add(session);
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while adding new session. error: {e.ToString()}");
            }
        }
        public bool IsExpired(Guid sessionId)
        {
            bool expired = true;
            using (var ctx = this.GetNewContext())
            {
                if (ctx.RMSession.Any(s => s.Id == sessionId)) 
                {
                    var session = ctx.RMSession.Find(sessionId);
                    if (session != null)
                    {
                        expired = DateTime.UtcNow > session.Expiration;
                    }
                }
                
            }
            return expired;
        }
        public async Task<bool> UpdateAsync(RMSessionDto dto) 
        {
            bool result = false;
            using (var ctx = this.GetNewContext())
            {
                if (await ctx.RMSession.AnyAsync(s => s.Id == dto.Id))
                {
                    var session = await ctx.RMSession.Where(s => s.Id == dto.Id).FirstAsync();
                    session.Expiration = dto.Expiration;
                    session.Extension = dto.Extension;
                    result = await ctx.SaveChangesAsync() > 0;
                }
                else 
                {
                    var session = new RMSession()
                    {
                        Id = dto.Id,
                        Expiration = dto.Expiration,
                        UserId = dto.UserId,
                        Extension = dto.Extension,
                    };
                    ctx.RMSession.Add(session);
                    result = await ctx.SaveChangesAsync() > 0;
                }
            }
            return result;
        }

        public void MarkIsRemovedByUserId(string userId)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    if (ctx.RMSession.Any(s => s.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)))
                    {
                        var sessions = ctx.RMSession.Where(s => s.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (var session in sessions)
                        {
                            session.IsMarkRemoved = true;
                        }
                        this.BatchUpdate(ctx, sessions);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while mark removed sessions by userId:{userId}. error: {e}");
            }
        }

        public bool IsMarkRemoved(Guid sessionId)
        {
            try
            {
                using (var ctx = this.GetNewContext())
                {
                    return ctx.RMSession.Any(o => o.Id == sessionId && o.IsMarkRemoved);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while check session IsMarkRemoved the session with sessionId  error: {e}");
                return false;
            }
        }

    }
}
