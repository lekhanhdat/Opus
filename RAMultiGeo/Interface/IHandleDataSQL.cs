using AvePoint.RA.DB.Model;

namespace RAMultiGeo.Interface
{
    public interface IHandleDataSQL
    {
        Task<IEnumerable<object>> QueryByPagerAsync(int pageIndex, int pageSize);

        Task<long> BatchInsertDataAsync(IEnumerable<object> data);
        Task<long> DeleteAllDataAsync();
    }
}
