using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IMultiGeoSettingDao
    {
        Dictionary<string,string> GetDicDCAndIpAddresses();
        Task AddOrUpdateMultipleGeoSettings(List<MultiGeoSettingInfo> settings);
        Task<long> MultiGeoInsertMultiGeoSettingTableAsync(IEnumerable<MultiGeoSettingInfo> multiGeoSettingInfos);
        Task<long> MultiGeoDeleteAllMultiGeoSettingAsync();
        Task<IEnumerable<object>> LoadByPager(int pageIndex, int pageSize);
    }
}
