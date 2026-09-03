using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IFSMyHubDashboardDao : IBaseDao<RMFSMyHubDashboard>
    {
        Task AddOrUpdateBatchAsync (IEnumerable<RMFSMyHubDashboard> rMFSMyHubDashboards);
        Task<RMFSMyHubDashboard> GetByNodeIdAsync(Guid nodeId);
        Task<RMFSMyHubDashboard> GetByFullPathAsync(string fullPath);
    }   
}
