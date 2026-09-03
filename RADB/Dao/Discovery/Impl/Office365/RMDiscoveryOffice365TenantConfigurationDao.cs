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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365TenantConfigurationDao : IRMDiscoveryOffice365TenantConfigurationDao
    {
        public async Task<T> GetValueAsync<T>(Guid o365TenantId, RMDiscoveryO365TenantConfigurationType configurationType)
        {
            var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var configurationInfo = await efContext.Office365O365TenantConfigurationInfoes.FirstAsync(item => item.Type == configurationType);
            return JsonConvert.DeserializeObject<T>(configurationInfo.JsonValue);
        }

        public async Task AddOrUpdateAsync(Guid o365TenantId, RMDiscoveryO365TenantConfigurationType configurationType, string jsonValue)
        {
            var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var configurationInfo = await efContext.Office365O365TenantConfigurationInfoes.FirstOrDefaultAsync(item => item.Type == configurationType);
            configurationInfo ??= new Model.Discovery.Office365.RMDiscoveryOffice365TenantConfiguration
            {
                Type = configurationType,
                CreateTime = DateTime.UtcNow.Ticks,
            };

            configurationInfo.JsonValue = jsonValue;
            configurationInfo.ModifiedTime = DateTime.UtcNow.Ticks;

            efContext.Office365O365TenantConfigurationInfoes.AddOrUpdate(configurationInfo);
            await efContext.SaveChangesAsync();
        }
    }
}
