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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Authorize
{
    public class RMSessionStore : IRMSessionStore
    {
        private const string SESSION_LIST_KEY_PREFIX = "sessionlist:";
        private const string SESSION_KEY_PREFIX = "session:";
        private static AsyncLocal<Guid> currentSessionId = new AsyncLocal<Guid>();
        //private static AsyncLocal<string> currentTenantId = new AsyncLocal<string>();
        private RALogger mLogger = RALogger.GetInstance(typeof(RMSessionStore));
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

        public IRMCache Cache { get; set; }

        public async Task DeleteAsync(Guid sessionId)
        {
            var identity = await GetAsync(sessionId);
            if (identity != null)
            {
                await Cache.RemoveAsync(BuildKey(sessionId), false);

                var listKey = BuildSessionListKey(identity);
                var sessionList = await Cache.GetAsync<HashSet<string>>(listKey);
                if (sessionList != null)
                {
                    sessionList.Remove(sessionId.ToString());
                    if (sessionList.Count > 0)
                    {
                        await Cache.SetAsync(listKey, sessionList, TimeSpan.FromMinutes(identity.SessionOut));
                    }
                    else
                    {
                        await Cache.RemoveAsync(listKey);
                    }
                }
            }
        }

        public async Task<CurrentUserInfo> GetLogonUserInfoAsync()
        {
            if (CurrentSessionId != Guid.Empty)
            {
                mLogger.Info($"current user session id:{CurrentSessionId}.");
                var identity = await GetAsync(CurrentSessionId);
                return ConvertUserInfo(identity);
            }
            
            return null;
        }

        public CurrentUserInfo ConvertUserInfo(RMIdentity identity)
        {
            return new CurrentUserInfo()
            {
                AccountId = identity.AccountId,
                AccountNumber = identity.AccountNumber,
                AccountType = identity.AccountType,
                Company = identity.Company,
                DisplayName = identity.DisplayName,
                TenantGroupId = identity.TenantGroupId,
                LoginName = identity.RegisterEmail,
                RegisterEmail = identity.RegisterEmail,
                SessionId = identity.SessionId.ToString(),
                SessionOut = identity.SessionOut,
                PermissionMark = identity.GPermission,
            };
        }

        public Task<RMIdentity> GetAsync(Guid sessionId)
        {
            return Cache.GetAsync<RMIdentity>(BuildKey(sessionId), false);
        }

        public async Task RenewAsync(RMIdentity identity, TimeSpan duration)
        {
            await Cache.RenewAsync(BuildSessionListKey(identity), duration);
            await Cache.RenewAsync(BuildKey(identity.SessionId), duration, false);
        }

        public async Task UpdateTimeoutSettingAsync(RMIdentity identity, int timeoutInMinutes)
        {
            identity.SessionOut = timeoutInMinutes <= 0 ? 30 : timeoutInMinutes;

            var duration = TimeSpan.FromMinutes(identity.SessionOut);
            await Cache.SetAsync(BuildKey(identity.SessionId), identity, duration, false);

            var listKey = BuildSessionListKey(identity);
            var sessionList = await Cache.GetAsync<HashSet<string>>(listKey);
            if (sessionList != null)
            {
                foreach (var otherSessionId in sessionList)
                {
                    var id = new Guid(otherSessionId);
                    if (id == identity.SessionId)
                    {
                        continue;
                    }
                    var otherSession = await GetAsync(id);
                    if (otherSession != null)
                    {
                        otherSession.SessionOut = identity.SessionOut;
                        await Cache.SetAsync(BuildKey(id), otherSession, duration, false);
                    }
                }
            }
        }

        public async Task SetAsync(RMIdentity identity)
        {
            int sessionTimeout = identity.SessionOut <= 0 ? 30 : identity.SessionOut;
            var duration = TimeSpan.FromMinutes(sessionTimeout);
            await Cache.SetAsync(BuildKey(identity.SessionId), identity, duration, false);

            var listKey = BuildSessionListKey(identity);
            var sessionList = await Cache.GetAsync<HashSet<string>>(listKey);

            if (identity.ForceLogined && sessionList != null)
            {
                foreach (var logoutSessionId in sessionList)
                {
                    var logoutSession = await GetAsync(new Guid(logoutSessionId));
                    if (logoutSession != null)
                    {
                        logoutSession.IsAuthenticated = false;
                        logoutSession.ForcedLogout = true;
                        await Cache.SetAsync(BuildKey(logoutSession.SessionId), logoutSession, TimeSpan.FromMinutes(5), false);
                    }
                }
                sessionList = null;
            }

            if (sessionList == null)
            {
                sessionList = new HashSet<string>() { identity.SessionId.ToString() };
            }
            else
            {
                sessionList.Add(identity.SessionId.ToString());
            }
            await Cache.SetAsync(listKey, sessionList, duration);
        }

        private string BuildKey(Guid sessionId)
        {
            return $"{SESSION_KEY_PREFIX}{sessionId}";
        }

        private string BuildSessionListKey(RMIdentity identity)
        {
            return $"{SESSION_LIST_KEY_PREFIX}{identity.TenantGroupId}{identity.AccountId}";
        }
    }
}