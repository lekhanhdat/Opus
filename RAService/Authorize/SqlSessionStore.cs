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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object.Session;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Extentions.Authorize
{
    public class SqlSessionStore : IRMSessionStore
    {
        public IRMCache Cache { get; set; }
        private IRMSessionDao SessionDao => PlatformWindsorManager.GetService<IRMSessionDao>();

        public async Task DeleteAsync(Guid sessionId)
        {
            await SessionDao.Remove(sessionId);
        }

        public async Task<RMIdentity> GetAsync(Guid sessionId)
        {
            if (string.IsNullOrEmpty(TenantLocalValue.LogonGroupId)) 
            {
                return null;
            }
            var session = await SessionDao.GetAsync(sessionId);
            if (!string.IsNullOrEmpty(session?.Extension))
            {
                return JsonConvert.DeserializeObject<RMIdentity>(session?.Extension);
            }
            return null;
        }
      
        public async Task RenewAsync(RMIdentity identity, TimeSpan duration)
        {
            await SessionDao.UpdateAsync(new Contract.Object.Session.RMSessionDto()
            {
                Id = identity.SessionId,
                UserId = identity.AccountId,
                Expiration = DateTime.UtcNow.AddMinutes(duration.TotalMinutes),
                Extension = JsonConvert.SerializeObject(identity)
            });
        }

        public async Task UpdateTimeoutSettingAsync(RMIdentity identity, int timeoutInMinutes)
        {
            identity.SessionOut = timeoutInMinutes <= 0 ? 30 : timeoutInMinutes;

            var duration = TimeSpan.FromMinutes(identity.SessionOut);

            await SessionDao.UpdateAsync(new Contract.Object.Session.RMSessionDto()
            {
                Id = identity.SessionId,
                UserId = identity.AccountId,
                Expiration = DateTime.UtcNow.AddMinutes(duration.TotalMinutes),
                Extension = JsonConvert.SerializeObject(identity)
            });
            var sessionList = await SessionDao.ListAsync(identity.AccountId);

            foreach (var session in sessionList)
            {
                if (session.Id != identity.SessionId)
                {
                    var otherIdentity = JsonConvert.DeserializeObject<RMIdentity>(session.Extension);
                    otherIdentity.SessionOut = identity.SessionOut;
                    await SessionDao.UpdateAsync(new Contract.Object.Session.RMSessionDto()
                    {
                        Id = session.Id,
                        UserId = session.UserId,
                        Expiration = DateTime.UtcNow.AddMinutes(duration.TotalMinutes),
                        Extension = JsonConvert.SerializeObject(otherIdentity)
                    });
                    
                }
            }
        }

        public async Task SetAsync(RMIdentity identity)
        {
            int sessionTimeout = identity.SessionOut <= 0 ? 30 : identity.SessionOut;
            var duration = TimeSpan.FromMinutes(sessionTimeout);
            await SessionDao.UpdateAsync(new Contract.Object.Session.RMSessionDto()
            {
                Id = identity.SessionId,
                UserId = identity.AccountId,
                Expiration = DateTime.UtcNow.AddMinutes(duration.TotalMinutes),
                Extension = JsonConvert.SerializeObject(identity)
            });
            var sessionList = await SessionDao.ListAsync(identity.AccountId);
            if (identity.ForceLogined && sessionList != null)
            {
                foreach (var session in sessionList)
                {
                    var logoutSession = JsonConvert.DeserializeObject<RMIdentity>(session.Extension);
                    if (logoutSession != null && session.Id != identity.SessionId)
                    {
                        logoutSession.IsAuthenticated = false;
                        logoutSession.ForcedLogout = true;
                        await SessionDao.UpdateAsync(new Contract.Object.Session.RMSessionDto()
                        {
                            Id = session.Id,
                            UserId = session.UserId,
                            Expiration = DateTime.UtcNow.AddMinutes(duration.TotalMinutes),
                            Extension = JsonConvert.SerializeObject(logoutSession)
                        });
                    }
                }
            }
        }

    }
}