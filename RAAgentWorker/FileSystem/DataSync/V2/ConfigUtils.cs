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
using AvePoint.Hybrid.Utility;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class ConfigUtils
    {
        public static readonly int DISCOVERY_AND_ANALYZE_THREAD_COUNT = SafeParseConfig(HybridAppSettingKey.DiscoveryAndAnalyzeWorkerCount, 4);
        public static readonly int PERSIST_AND_REPORT_THREAD_COUNT = SafeParseConfig(HybridAppSettingKey.PersistAndReportWorkerCount, 4);
        public static readonly int WORKER_TRANSFER_DATA_COUNT = SafeParseConfig(HybridAppSettingKey.WorkerTransferDataCount, 1000);
        public static readonly int MAX_INFLIGHT_COUNT = 10;

        private static int SafeParseConfig(HybridAppSettingKey key, int defaultValue)
        {
            try
            {
                var value = int.Parse(CommonConfiguration.getConfig(key));
                return value > 0 ? value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}