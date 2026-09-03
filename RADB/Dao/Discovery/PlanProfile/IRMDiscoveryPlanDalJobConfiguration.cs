using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Plan;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.PlanProfile
{
    public interface IRMDiscoveryPlanDalJobConfiguration
    {
        Task AddOrUpdateAsync(params RMDiscoveryDalJobConfiguration[] configurations);
        Task<T> GetAsync<T>(RMDiscoveryConfigurationType type);
    }
}
