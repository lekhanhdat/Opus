using AvePoint.RA.DB.Model;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMMultiGeoApiChangeLogDao
    {
        void Add(string tenantGroupId, RMMultiGeoApiChangeLogEntity entity);
        IEnumerable<string> GetAllOperationTypeNeedSync(string logonGroupId, long lastSyncTime);
    }
}