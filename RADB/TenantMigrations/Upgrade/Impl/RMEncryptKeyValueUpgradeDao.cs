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
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Upgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.TenantMigrations.Upgrade.Impl
{
    public class RMEncryptKeyValueUpgradeDao : IDbUpgradeDao
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RMDbContext context = null;

        public async Task UpgradeAsync(RMDbContext newContext)
        {
            try
            {
                context = newContext;
                AddTenantPreNonceAndDefaultEncryptKey();
            }
            catch (Exception e)
            {
                Logger.Error($"[ueod] An error occurred while executing encrypt KeyValue upgrade logic. Error: {e}");
            }
        }

        private void AddTenantPreNonceAndDefaultEncryptKey()
        {
            var entity = context.RMEncryptKeyValues.FirstOrDefault(o => o.Key == RMEncryptKeyHelper.db_key_tenantPreNonce);
            if (entity == null)
            {
                Logger.Info($"[ueod] need to save preNonce and default key. tenant id: {TenantLocalValue.LogonGroupId}");
                var tenantPreNonceBase64Str = RMEncryptKeyHelper.GenTenantPreNonce();
                context.RMEncryptKeyValues.Add(new RMEncryptKeyValue
                {
                    Key = RMEncryptKeyHelper.db_key_tenantPreNonce,
                    Value = tenantPreNonceBase64Str
                });

                context.RMEncryptKeyValues.Add(new RMEncryptKeyValue
                {
                    Key = RMEncryptKeyHelper.db_key_defaultEncryptKey,
                    Value = RMEncryptKeyHelper.GenEncryptKey(tenantPreNonceBase64Str)
                });
                if (context.SaveChanges() > 0)
                {
                    Logger.Info($"[ueod] success to save preNonce and default key.");
                }
            }
            else
            {
                Logger.Info($"[ueod] skip to save preNonce and default key.");
            }
        }
    }
}
