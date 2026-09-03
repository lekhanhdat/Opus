using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Physical;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IPhysicalRecordsMoveDataTableDao
    {
        IEnumerable<PhysicalRecordMoveData> Add(string tenantGroupId, List<PhysicalRecordMoveData> entities);
        Task<(IEnumerable<PhysicalRecordMoveData>, int)> GetMoveDatasPaginationWithLimit(string tenantGroupId, PickListMoveParam filter, int limit);
        Task<(IEnumerable<PhysicalRecordMoveData>, int)> GetMoveDatasPagination(string tenantGroupId, PickMoveListParam filter, int pageIndex, int pageSize);
    }
}
