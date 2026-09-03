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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMachineLearningReviewerUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly ConcurrentDictionary<int, int[]> spRecordOwnerCache = new();
        private static readonly ConcurrentDictionary<int, int[]> oneDriveRecordOwnerCache = new();
        private static readonly ConcurrentDictionary<int, int[]> teamsRecordOwnerCache = new();

        public static async Task<int[]> GetRecordOwnersAsync(int settingId, RecordOwnerSettingType settingType)
        {
            var recordOwnerCache = GetRecordOwnerCache(settingType);
            if (!recordOwnerCache.TryGetValue(settingId, out int[] recordOwners))
            {
                var owners = RecordOwnerDao.GetRecordOwner(settingId, settingType);
                if (owners != null && owners.Count > 0)
                {
                    logger.Info($"start to get setting record owners, setting id:{settingId}, owners count: {owners.Count}");
                    try
                    {
                        var recordOwnerIDs = owners.Select(a => a.ObjectId).ToList();
                        recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.Id)?.ToArray();
                        if (recordOwners != null && recordOwners.Length > 0)
                        {
                            if (!recordOwnerCache.TryAdd(settingId, recordOwners))
                            {
                                logger.Warn($"failed to add record owner cache, settingId: {settingId}, recordOwners:{recordOwnerIDs}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"failed to get record owners, message: {ex}");
                    }
                }

            }
            return recordOwners;
        }

        private static ConcurrentDictionary<int, int[]> GetRecordOwnerCache(RecordOwnerSettingType settingType)
        {
            ConcurrentDictionary<int, int[]> recordOwnerCache = settingType switch
            {
                RecordOwnerSettingType.AISharePointOnline => spRecordOwnerCache,
                RecordOwnerSettingType.AIOneDrive => oneDriveRecordOwnerCache,
                RecordOwnerSettingType.AITeams => teamsRecordOwnerCache,
                _ => throw new Exception("not support current record owner setting type."),
            };
            return recordOwnerCache;
        }

    }
}
