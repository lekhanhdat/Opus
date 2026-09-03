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
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DestructionCache
{
    public class DestrunctionReportHelper
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(DestrunctionReportHelper));
        private readonly DateTime startDate;
        private readonly DateTime endDate;
        private DateTime upgradeOpusUtcTime;
        private Dictionary<FetchDestroyedDataMode, Tuple<DateTime, DateTime>> fetchDataTimeRangeDic = new();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public DestrunctionReportHelper(DateTime profileStartDate, DateTime profileEndDate)
        {
            startDate = profileStartDate;
            endDate = profileEndDate;
            Init();
        }

        private void Init()
        {
            InitUpgradeOpusTime();
            InitFetchDestroyedDataTimeRangeInfo();
        }

        private void InitUpgradeOpusTime()
        {
            var upgradeOpusUtcTimeTicks = TenantService.GetUpgradeOpusTimeTicks();
            upgradeOpusUtcTime = upgradeOpusUtcTimeTicks > 0 ? new DateTime(upgradeOpusUtcTimeTicks, DateTimeKind.Utc) : DateTime.MinValue;
        }

        private void InitFetchDestroyedDataTimeRangeInfo()
        {
            if (!TenantService.IsNewOpusTenant())
            {
                logger.Info($"get data from archiver table, because tenant is not upgrade opus.");
                fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.ArchiverTable, Tuple.Create(startDate, endDate));
            }
            else
            {
                if (upgradeOpusUtcTime > DateTime.MinValue)
                {
                    if (upgradeOpusUtcTime >= startDate && upgradeOpusUtcTime < endDate)
                    {
                        logger.Info($"get data from archiver table & LiteDB");
                        fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.ArchiverTable, Tuple.Create(startDate, upgradeOpusUtcTime));
                        fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.LiteDB, Tuple.Create(upgradeOpusUtcTime, endDate));
                    }
                    else if (upgradeOpusUtcTime > endDate)
                    {
                        logger.Info($"get data from archiver table, because upgradeOpusUtcTime > endDate.");
                        fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.ArchiverTable, Tuple.Create(startDate, endDate));
                    }
                    else
                    {
                        logger.Info($"get data from LiteDB.");
                        fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.LiteDB, Tuple.Create(startDate, endDate));
                    }
                }
                else
                {
                    logger.Info($"get data from LiteDB, because upgradeOpusUtcTime is null");
                    fetchDataTimeRangeDic.Add(FetchDestroyedDataMode.LiteDB, Tuple.Create(startDate, endDate));
                }
            }
        }

        public Tuple<DateTime, DateTime> GetQueryArchiverTableTimeRange()
        {
            return GetDestroyedFetchDataInfo(FetchDestroyedDataMode.ArchiverTable);
        }

        public Tuple<DateTime, DateTime> GetQueryLiteDBTimeRange()
        {
            return GetDestroyedFetchDataInfo(FetchDestroyedDataMode.LiteDB);
        }

        public bool IsNeedQueryLiteDB()
        {
            if (fetchDataTimeRangeDic != null && fetchDataTimeRangeDic.ContainsKey(FetchDestroyedDataMode.LiteDB))
            {
                logger.Info("The report is need query LiteDB.");
                return true;
            }
            logger.Info("The report is not need query LiteDB.");
            return false;
        }

        private Tuple<DateTime, DateTime> GetDestroyedFetchDataInfo(FetchDestroyedDataMode mode)
        {
            if (fetchDataTimeRangeDic != null && fetchDataTimeRangeDic.ContainsKey(mode))
            {
                return fetchDataTimeRangeDic.Where(o => o.Key.Equals(mode)).Select(o => o.Value).FirstOrDefault();
            }
            return null;
        }
    }

    public enum FetchDestroyedDataMode
    {
        ArchiverTable = 1,
        LiteDB = 2
    }
}
