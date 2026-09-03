using AvePoint.RA.DB.Model;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMStorageCostEvaluationDao : IBaseDao<RMStorageCostEvaluation>
    {
        Task<bool> SaveCostEvaluationAsync(RMStorageCostEvaluation entity);
    }
}
