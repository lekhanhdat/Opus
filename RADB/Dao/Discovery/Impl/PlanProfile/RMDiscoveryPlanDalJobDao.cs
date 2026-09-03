using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model.Discovery.Plan;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile
{
    public class RMDiscoveryPlanDalJobDao : IRMDiscoveryPlanDalJobDao
    {
        public async Task AddOrUpdateJobAsync(RMDiscoveryPlanDalJob planDalJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            planDalJob.LastModifiedTime = DateTime.UtcNow.Ticks;
            efContext.PlanDalJobs.AddOrUpdate(planDalJob);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryPlanDalJob>> GetJobsByMainJobIdAsync(string mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.PlanDalJobs
                .Where(j => j.MainJobId == mainJobId)
                .ToListAsync();
        }
    }
}
