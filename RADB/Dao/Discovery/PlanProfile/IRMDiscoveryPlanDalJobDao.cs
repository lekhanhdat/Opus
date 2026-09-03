using AvePoint.RA.DB.Model.Discovery.Plan;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.DB.Dao.Discovery.PlanProfile
{
    public interface IRMDiscoveryPlanDalJobDao
    {
        System.Threading.Tasks.Task AddOrUpdateJobAsync(RMDiscoveryPlanDalJob mainJobInfo);
        System.Threading.Tasks.Task<List<RMDiscoveryPlanDalJob>> GetJobsByMainJobIdAsync(string mainJobId);
    }
}
