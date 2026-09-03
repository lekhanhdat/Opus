using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model.Discovery.Plan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl
{
    public class RMDiscoveryDalJobConfigurationDao : IRMDiscoveryPlanDalJobConfiguration
    {
        public async Task AddOrUpdateAsync(params RMDiscoveryDalJobConfiguration[] configurations)
        {
            if (!configurations.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.PlanDalJobConfigurations.AddOrUpdate(configurations);
            await efContext.SaveChangesAsync();
        }

        public async Task<T> GetAsync<T>(RMDiscoveryConfigurationType type)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var configInfo = await efContext.PlanDalJobConfigurations.FirstOrDefaultAsync(item => item.ConfigurationType == type);
            return JsonConvert.DeserializeObject<T>(configInfo.ValueJson);
        }
    }
}
