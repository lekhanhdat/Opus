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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPConfigurationDao : IRMDiscoveryAOSPConfigurationDao
    {
        public async Task<T> GetAsync<T>(RMDiscoveryConfigurationType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configInfo = await efContext.AOSPConfigurations.FirstAsync(item => item.ConfigurationType == type);
            return JsonConvert.DeserializeObject<T>(configInfo.ValueJson);
        }

        public async Task<T> GetByO365TenantIdAsync<T>(RMDiscoveryConfigurationType type, string o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configInfo = await efContext.AOSPConfigurations.FirstAsync(item => item.ConfigurationType == type && item.O365TenantId == o365TenantId);
            return JsonConvert.DeserializeObject<T>(configInfo.ValueJson);
        }

        public async Task<T> GetByO365TenantIdAsync<T>(RMDiscoveryConfigurationType type, string o365TenantId, T defaultValue)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configInfo = await efContext.AOSPConfigurations.FirstOrDefaultAsync(item => item.ConfigurationType == type && item.O365TenantId == o365TenantId);
            if (configInfo == null)
            {
                return defaultValue;
            }

            return JsonConvert.DeserializeObject<T>(configInfo.ValueJson);
        }

        public async Task<T> GetAsync<T>(RMDiscoveryConfigurationType type, T defaultValue)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configInfo = await efContext.AOSPConfigurations.FirstOrDefaultAsync(item => item.ConfigurationType == type);
            if (configInfo == null)
            {
                return defaultValue;
            }
            return JsonConvert.DeserializeObject<T>(configInfo.ValueJson);
        }

        public async Task<List<RMDiscoveryAOSPConfiguration>> GetAsync(params RMDiscoveryConfigurationType[] types)
        {
            if (!types.Any())
            {
                return new();
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configs = await efContext.AOSPConfigurations.Where(item => Enumerable.Contains(types, item.ConfigurationType)).ToListAsync();
            return configs;
        }

        public async Task<List<RMDiscoveryAOSPConfiguration>> GetByO365TenantIdAsync(string O365TenantId, params RMDiscoveryConfigurationType[] types)
        {
            if (!types.Any())
            {
                return new();
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configs = await efContext.AOSPConfigurations.Where(item => Enumerable.Contains(types, item.ConfigurationType) && item.O365TenantId == O365TenantId).ToListAsync();
            return configs;
        }

        public async Task AddOrUpdateAsync(params RMDiscoveryAOSPConfiguration[] configurations)
        {
            if (!configurations.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.AOSPConfigurations.AddOrUpdate(configurations);
            await efContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryAOSPConfiguration[] configurations)
        {
            if (!configurations.Any())
            {
                return;
            }

            efContext.AOSPConfigurations.AddOrUpdate(configurations);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteByO365TenantIdAndTypeAsync(string o365TenantId, RMDiscoveryConfigurationType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var existConfigs = await efContext.AOSPConfigurations.Where(item => item.O365TenantId == o365TenantId && item.ConfigurationType == type).ToListAsync();
            if (existConfigs.Count != 0)
            {
                efContext.AOSPConfigurations.RemoveRange(existConfigs);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByO365TenantIdAsync(RMDiscoveryDBEFContext efContext, string O365TenantId)
        {
            var existConfigs = await efContext.AOSPConfigurations.Where(item => item.O365TenantId == O365TenantId).ToListAsync();
            if (existConfigs.Count != 0)
            {
                efContext.AOSPConfigurations.RemoveRange(existConfigs);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistAsync(RMDiscoveryConfigurationType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPConfigurations.AnyAsync(item => item.ConfigurationType == type);
        }

        public async Task<bool> ExistByO365TenantIdAsync(RMDiscoveryConfigurationType type, string O365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPConfigurations.AnyAsync(item => item.ConfigurationType == type && item.O365TenantId == O365TenantId);
        }

        public async Task<int> UpdateDiscoveryConfigurationAsync(RMDiscoveryAOSPConfiguration configInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.AOSPConfigurations.AddOrUpdate(configInfo);
            return await efContext.SaveChangesAsync();
        }
    }
}
