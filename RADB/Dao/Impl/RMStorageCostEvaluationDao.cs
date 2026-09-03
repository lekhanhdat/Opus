using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMStorageCostEvaluationDao : BaseDao<RMStorageCostEvaluation>, IRMStorageCostEvaluationDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMStorageCostEvaluationDao));
        
        public async Task<bool> SaveCostEvaluationAsync(RMStorageCostEvaluation entity)
        {
            using var context = RMDBContextManager.GetSystemDBContext();
            if (entity == null)
            {
                return false;
            }
            try
            {
                context.StorageCostEvaluations.AddOrUpdate(entity);
                return await context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to save cost evaluation for TenantId: {entity.TenantId}, StorageId: {entity.StorageId}. Exception: {ex.Message}");
                return false;
            }
        }
    }
}
