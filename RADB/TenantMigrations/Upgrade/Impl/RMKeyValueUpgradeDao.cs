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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMKeyValueUpgradeDao: IDbUpgradeDao
    {
        public int SubJobCountInMainJob
        {
            get
            {
                var nodeCnt = 5;
                int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.SUB_JOB_COUNT_IN_MAIN_JOB], out nodeCnt);
                return nodeCnt;
            }
        }
        public string searchSiteColumnFileName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.UNIQUE_ID_SP_SEARCH_SITE_COLUMN];

        public string searchListColumnFileName = RMGlobalConfiguration.AppConfig[RMAppSettingKey.UNIQUE_ID_SP_SEARCH_LIST_COLUMN];

        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao
        {
            get { return _RMKeyValueDao ?? (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao)); }
            set { _RMKeyValueDao = value; }
        }
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMKeyValueUpgradeDao));

        public async Task UpgradeAsync(RMDbContext context)
        {
            try
            {
                Logger.Info("Try to execute upgrade in RMKeyValueUpgradeDao");
                var key = $"{KeyNameCollection.COSMOS_SCHEMA_VERSION}{RMNameValueDto.Seprator}{RMNameValueType.CosmosQuery}";

                var entity = context.RMKeyValue.FirstOrDefault(o => o.Key == key);
                if (entity != null)
                {
                    Logger.Info($"key {key} already exists.");
                    return;
                }
                var defaultVersion = "2.0.0.0";
                context.RMKeyValue.Add(new RMKeyValue
                {
                    Key = key,
                    Value = defaultVersion
                });

                if (context.SaveChanges() > 0)
                {
                    Logger.Info($"Set default value {defaultVersion} for key : {key}");
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while executing keyvalue upgrade logic. Error: {e}");
            }
        }

    }
}
