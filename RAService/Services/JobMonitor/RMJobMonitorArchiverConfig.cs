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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.CommonUtil;
using System;

namespace AvePoint.RA.Service.JobMonitor
{
    public static class RMJobMonitorArchiverConfig
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMJobMonitorArchiverConfig));
        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public const string EnabledKey = "JM_ARCHIVE_ENABLED";
        public const string MaxRowsPerRunKey = "JM_ARCHIVE_MAX_ROWS_PER_RUN";
        public const string OlderThanDaysKey = "JM_ARCHIVE_OLDER_THAN_DAYS";

        public static bool TryGetBool(string key, bool defaultValue, out bool value)
        {
            value = defaultValue;
            try
            {
                var kv = KeyValueDao.GetValueByKey(key);
                if (kv != null && bool.TryParse(kv.Value, out var b)) { value = b; return true; }
            }
            catch (Exception ex)
            {
                s_logger.Warn($"RMJobMonitorArchiverConfig.TryGetBool failed for key {key}: {ex}");
            }
            return false;
        }

        public static int GetInt(string key, int defaultValue)
        {
            try
            {
                var kv = KeyValueDao.GetValueByKey(key);
                if (kv != null && int.TryParse(kv.Value, out var i)) return i;
            }
            catch (Exception ex)
            {
                s_logger.Warn($"RMJobMonitorArchiverConfig.GetInt failed for key {key}: {ex}");
            }
            return defaultValue;
        }
    }
}
