using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMFSMyHubDashboardDao : BaseDao<RMFSMyHubDashboard>, IFSMyHubDashboardDao
    {
        public async Task AddOrUpdateBatchAsync(IEnumerable<RMFSMyHubDashboard> dashboards)
        {
            using var context = GetNewContext();
            var inputDict = dashboards.ToDictionary(x => x.NodeId);
            var nodeIds = inputDict.Keys.ToList();
            var existData = context.RMFSMyHubDashboards
                .Where(x => nodeIds.Contains(x.NodeId))
                .ToList();

            // Update
            foreach (var exist in existData)
            {
                var update = inputDict[exist.NodeId];
                exist.GroupId = update.GroupId;
                exist.MetaData = update.MetaData;
                exist.FullPath = update.FullPath;
            }

            // Add
            var existIds = existData.Select(x => x.NodeId).ToHashSet();
            var needAdd = dashboards
                .Where(x => !existIds.Contains(x.NodeId))
                .ToList();

            context.RMFSMyHubDashboards.AddRange(needAdd);

            await context.SaveChangesAsync();
        }

        public async Task<RMFSMyHubDashboard> GetByNodeIdAsync(Guid nodeId)
        {
            using var context = GetNewContext();
            return await context.RMFSMyHubDashboards
                .FirstOrDefaultAsync(x => x.NodeId == nodeId);
        }

        public async Task<RMFSMyHubDashboard> GetByFullPathAsync(string fullPath)
        {
            using var context = GetNewContext();
            return await context.RMFSMyHubDashboards
                .FirstOrDefaultAsync(x => x.FullPath == fullPath);
        }
    }
}
