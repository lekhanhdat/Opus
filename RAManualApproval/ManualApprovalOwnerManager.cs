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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using Microsoft.Exchange.WebServices.Data;
using RAManualApproval.ManualExceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval
{
    public class ManualApprovalOwnerManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalOwnerManager));

        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly ConcurrentDictionary<string, int> OwnerCache = new ConcurrentDictionary<string, int>();

        private static readonly ConcurrentDictionary<int, string> OwnerDisplayNameCache = new ConcurrentDictionary<int, string>();

        private static readonly ConcurrentDictionary<string, string> OwnerDisplayNameCacheWithKeyUserId = new ConcurrentDictionary<string, string>();

        public static IEnumerable<int> GetOwnerIds(List<UserInfo> userInfos)
        {
            var userIds = userInfos.Select(item => item.UserId);
            return GetOwnerIds(userIds);
        }

        public static IEnumerable<int> GetOwnerIds(IEnumerable<string> userIds)
        {
            foreach (var userId in userIds)
            {
                if (TryGetOwnerId(userId, out var ownerId))
                {
                    yield return ownerId;
                }
            }
        }

        public static IEnumerable<string> GetOwnerDisplayNames(IEnumerable<int> userIntIds)
        {
            foreach(var userIntId in userIntIds)
            {
                if(TryGetOwnerDisplayName(userIntId, out var displayName))
                {
                    yield return displayName;
                }
            }
        }

        private static bool TryGetOwnerDisplayName(int userIntId, out string displayName)
        {
            displayName = string.Empty;

            if(!OwnerDisplayNameCache.TryGetValue(userIntId, out displayName))
            {
                var owners = AccountDao.FindListAsync(item => item.Id == userIntId).GetAwaiter().GetResult();
                if (owners == null || !owners.Any())
                {
                    throw new ManualApprovalException("RM_MA_NoOwner");
                }

                var owner = owners.FirstOrDefault(item => item.IsRemoved == 0);
                owner ??= owners.First();

                displayName = owner.DisplayName;
                var ownerId = owner.Id;


                if (!OwnerCache.TryAdd(owner.UserId, ownerId))
                {
                    Logger.Warn($"The add owner user id: [{owner.UserId}] mapping owner id: [{ownerId}] to cache failed.");
                }

                if (!OwnerDisplayNameCache.TryAdd(ownerId, owner.DisplayName))
                {
                    Logger.Warn($"The add owner user id: [{ownerId}] displayName mapping to cache failed.");
                }
            }

            return true;
        }

        public static IEnumerable<string> GetOwnerDisplayNamesByUserIds(IEnumerable<string> userIds)
        {
            foreach (var userId in userIds)
            {
                if (TryGetOwnerDisplayName(userId, out var displayName))
                {
                    yield return displayName;
                }
            }

        }

        private static bool TryGetOwnerDisplayName(string userId, out string displayName)
        {
            displayName = string.Empty;

            Logger.Info($"Try get display name of userId {userId}");

            if (!OwnerDisplayNameCacheWithKeyUserId.TryGetValue(userId, out displayName))
            {
                var owners = AccountDao.FindListAsync(item => item.AADId == userId).GetAwaiter().GetResult();
                if (owners == null || !owners.Any())
                {
                    throw new ManualApprovalException("RM_MA_NoOwner");
                }

                var owner = owners.FirstOrDefault(item => item.IsRemoved == 0);
                owner ??= owners.First();

                displayName = owner.DisplayName;
            }

            return true;
        }

        public static bool TryGetOwnerId(string userId, out int ownerId)
        {
            ownerId = -1;
            if (!OwnerCache.TryGetValue(userId, out ownerId))
            {
                var owners = AccountDao.FindListAsync(item => item.UserId == userId).GetAwaiter().GetResult();
                if (owners == null || !owners.Any())
                {
                    throw new ManualApprovalException("RM_MA_NoOwner");
                }

                var owner = owners.FirstOrDefault(item => item.IsRemoved == 0);
                owner ??= owners.First();

                ownerId = owner.Id;

                if (!OwnerCache.TryAdd(userId, ownerId))
                {
                    Logger.Warn($"The add owner user id: [{userId}] mapping owner id: [{ownerId}] to cache failed.");
                }

                if (!OwnerDisplayNameCache.TryAdd(ownerId, owner.DisplayName))
                {
                    Logger.Warn($"The add owner user id: [{ownerId}] displayName mapping to cache failed.");
                }
            }
            return true;
        }

        public static void AddOwnersCache(List<AccountDto> owners)
        {
            foreach (var owner in owners)
            {
                if (!OwnerCache.TryAdd(owner.UserId, owner.Id))
                {
                    Logger.Warn($"The add owner user id: [{owner.UserId}] mapping owner id: [{owner.Id}] to cache failed.");
                }

                if (!OwnerDisplayNameCache.TryAdd(owner.Id, owner.DisplayName))
                {
                    Logger.Warn($"The add owner user id: [{owner.Id}] displayName mapping to cache failed.");
                }
            }
        }
    }
}
