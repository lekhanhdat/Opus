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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Threading;
using AvePoint.RA.CommonUtil;
using Aspose.Email.Clients.Exchange.WebService.Schema_2016;
using AvePoint.RA.Contract.Services;

namespace AvePoint.RA.Web.Extentions.Authorize
{
    public  class SessionManger
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(SessionManger));
        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private static IRMSessionStore RedisSessionStore => PlatformWindsorManager.GetService<IRMSessionStore>("AvePoint.RA.Web.Extentions.Authorize.RedisSessionStore");
        private static IRMSessionStore SqlSessionStore => PlatformWindsorManager.GetService<IRMSessionStore>("AvePoint.RA.Web.Extentions.Authorize.SqlSessionStore");
        private static AsyncLocal<Guid> currentSessionId = new AsyncLocal<Guid>();
        public static Guid CurrentSessionId
        {
            get
            {
                return currentSessionId.Value;
            }
            set
            {
                currentSessionId.Value = value;
            }
        }
        static SessionManger() 
        {
        }
        public static bool useSqlSessionStore 
        {
            get { return !Cache.GetCachedRedisAvailability(); }
        }
        public async static Task<RMIdentity> GetAsync(Guid sessionId)
        {
            if (await Cache.CheckRedisAvailable())
            {
                try
                {
                    return await RedisSessionStore.GetAsync(sessionId);
                }
                catch (Exception ex)
                {
                    logger.Warn($"Redis session GetAsync failed, falling back to SQL. {ex.Message}");
                }
            }
            return await SqlSessionStore.GetAsync(sessionId);
        }

        public async static Task SetAsync(RMIdentity identity)
        {
            if (await Cache.CheckRedisAvailable())
            {
                try
                {
                    await RedisSessionStore.SetAsync(identity);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Redis session SetAsync failed, falling back to SQL. {ex.Message}");
                }
            }
            await SqlSessionStore.SetAsync(identity);
        }

        public async static Task RenewAsync(RMIdentity identity, TimeSpan duration)
        {
            if (await Cache.CheckRedisAvailable())
            {
                try
                {
                    await RedisSessionStore.RenewAsync(identity, duration);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Redis session RenewAsync failed, falling back to SQL. {ex.Message}");
                }
            }
            await SqlSessionStore.RenewAsync(identity, duration);
        }

        public async static Task UpdateTimeoutSettingAsync(RMIdentity identity, int timeoutInMinutes)
        {
            if (await Cache.CheckRedisAvailable())
            {
                try
                {
                    await RedisSessionStore.UpdateTimeoutSettingAsync(identity, timeoutInMinutes);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Redis session UpdateTimeoutSettingAsync failed, falling back to SQL. {ex.Message}");
                }
            }
            await SqlSessionStore.UpdateTimeoutSettingAsync(identity, timeoutInMinutes);
        }

        public async static Task DeleteAsync(Guid sessionId)
        {
            if (await Cache.CheckRedisAvailable())
            {
                try
                {
                    await RedisSessionStore.DeleteAsync(sessionId);
                    return;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Redis session DeleteAsync failed, falling back to SQL. {ex.Message}");
                }
            }
            await SqlSessionStore.DeleteAsync(sessionId);
        }

        
    }
}
